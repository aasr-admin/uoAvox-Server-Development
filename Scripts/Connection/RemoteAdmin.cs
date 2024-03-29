﻿using Server.Accounting;
using Server.Items;
using Server.Network;

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Server.RemoteAdmin
{
	public class AdminNetwork
	{
		private const string ProtocolVersion = "2";

		private static ArrayList m_Auth = new ArrayList();
		private static bool m_NewLine = true;
		private static readonly StringBuilder m_ConsoleData = new StringBuilder();

		private const string DateFormat = "MMMM dd hh:mm:ss.f tt";

		public static void Configure()
		{
			PacketHandlers.Register(0xF1, 0, false, new OnPacketReceive(OnReceive));

#if !MONO
			Core.MultiConsoleOut.Add(new EventTextWriter(new EventTextWriter.OnConsoleChar(OnConsoleChar), new EventTextWriter.OnConsoleLine(OnConsoleLine), new EventTextWriter.OnConsoleStr(OnConsoleString)));
#endif
			Timer.DelayCall(TimeSpan.FromMinutes(2.5), TimeSpan.FromMinutes(2.5), CleanUp);
		}

		public static void OnConsoleString(string str)
		{
			string outStr;
			if (m_NewLine)
			{
				outStr = String.Format("[{0}]: {1}", DateTime.UtcNow.ToString(DateFormat), str);
				m_NewLine = false;
			}
			else
			{
				outStr = str;
			}

			m_ConsoleData.Append(outStr);
			RoughTrimConsoleData();

			SendToAll(outStr);
		}

		public static void OnConsoleChar(char ch)
		{
			if (m_NewLine)
			{
				string outStr;
				outStr = String.Format("[{0}]: {1}", DateTime.UtcNow.ToString(DateFormat), ch);

				m_ConsoleData.Append(outStr);
				SendToAll(outStr);

				m_NewLine = false;
			}
			else
			{
				m_ConsoleData.Append(ch);
				SendToAll(ch);
			}

			RoughTrimConsoleData();
		}

		public static void OnConsoleLine(string line)
		{
			string outStr;
			if (m_NewLine)
			{
				outStr = String.Format("[{0}]: {1}{2}", DateTime.UtcNow.ToString(DateFormat), line, Console.Out.NewLine);
			}
			else
			{
				outStr = String.Format("{0}{1}", line, Console.Out.NewLine);
			}

			m_ConsoleData.Append(outStr);
			RoughTrimConsoleData();

			SendToAll(outStr);

			m_NewLine = true;
		}

		private static void SendToAll(string outStr)
		{
			SendToAll(new ConsoleData(outStr));
		}

		private static void SendToAll(char ch)
		{
			SendToAll(new ConsoleData(ch));
		}

		private static void SendToAll(ConsoleData packet)
		{
			packet.Acquire();
			for (var i = 0; i < m_Auth.Count; i++)
			{
				((NetState)m_Auth[i]).Send(packet);
			}

			packet.Release();
		}

		private static void RoughTrimConsoleData()
		{
			if (m_ConsoleData.Length >= 4096)
			{
				m_ConsoleData.Remove(0, 2048);
			}
		}

		private static void TightTrimConsoleData()
		{
			if (m_ConsoleData.Length > 1024)
			{
				m_ConsoleData.Remove(0, m_ConsoleData.Length - 1024);
			}
		}

		public static void OnReceive(NetState state, PacketReader pvSrc)
		{
			var cmd = pvSrc.ReadByte();
			if (cmd == 0x02)
			{
				Authenticate(state, pvSrc);
			}
			else if (cmd == 0xFE)
			{
				state.Send(new CompactServerInfo());
				state.Dispose();
			}
			else if (cmd == 0xFF)
			{
				var statStr = String.Format(", Name={0}, Age={1}, Clients={2}, Items={3}, Chars={4}, Mem={5}K, Ver={6}", Server.Misc.ServerList.ServerName, (int)(DateTime.UtcNow - Server.Items.Clock.ServerStart).TotalHours, NetState.Instances.Count, World.Items.Count, World.Mobiles.Count, (int)(System.GC.GetTotalMemory(false) / 1024), ProtocolVersion);
				state.Send(new UOGInfo(statStr));
				state.Dispose();
			}
			else if (!IsAuth(state))
			{
				Console.WriteLine("ADMIN: Unauthorized packet from {0}, disconnecting", state);
				Disconnect(state);
			}
			else
			{
				if (!RemoteAdminHandlers.Handle(cmd, state, pvSrc))
				{
					Disconnect(state);
				}
			}
		}

		private static void DelayedDisconnect(NetState state)
		{
			Timer.DelayCall(TimeSpan.FromSeconds(15.0), Disconnect, state);
		}

		private static void Disconnect(object state)
		{
			m_Auth.Remove(state);
			((NetState)state).Dispose();
		}

		public static void Authenticate(NetState state, PacketReader pvSrc)
		{
			var user = pvSrc.ReadString(30);
			var pw = pvSrc.ReadString(30);

			var a = Accounts.GetAccount(user) as Account;
			if (a == null)
			{
				state.Send(new Login(LoginResponse.NoUser));
				Console.WriteLine("ADMIN: Invalid username '{0}' from {1}", user, state);
				DelayedDisconnect(state);
			}
			else if (!a.HasAccess(state))
			{
				state.Send(new Login(LoginResponse.BadIP));
				Console.WriteLine("ADMIN: Access to '{0}' from {1} denied.", user, state);
				DelayedDisconnect(state);
			}
			else if (!a.CheckPassword(pw))
			{
				state.Send(new Login(LoginResponse.BadPass));
				Console.WriteLine("ADMIN: Invalid password for user '{0}' from {1}", user, state);
				DelayedDisconnect(state);
			}
			else if (a.AccessLevel < AccessLevel.Administrator || a.Banned)
			{
				Console.WriteLine("ADMIN: Account '{0}' does not have admin access. Connection Denied.", user);
				state.Send(new Login(LoginResponse.NoAccess));
				DelayedDisconnect(state);
			}
			else
			{
				Console.WriteLine("ADMIN: Access granted to '{0}' from {1}", user, state);
				state.Account = a;
				a.LogAccess(state);
				a.LastLogin = DateTime.UtcNow;

				state.Send(new Login(LoginResponse.OK));
				TightTrimConsoleData();
				state.Send(Compress(new ConsoleData(m_ConsoleData.ToString())));
				m_Auth.Add(state);
			}
		}

		public static bool IsAuth(NetState state)
		{
			return m_Auth.Contains(state);
		}

		private static void CleanUp()
		{//remove dead instances from m_Auth
			var list = new ArrayList();
			for (var i = 0; i < m_Auth.Count; i++)
			{
				var ns = (NetState)m_Auth[i];
				if (ns.Running)
				{
					list.Add(ns);
				}
			}

			m_Auth = list;
		}

		public static Packet Compress(Packet p)
		{
			int length;
			var source = p.Compile(false, out length);

			if (length > 100 && length < 60000)
			{
				var dest = new byte[(int)(length * 1.001) + 10];
				var destSize = dest.Length;

				var error = Compression.Pack(dest, ref destSize, source, length, ZLibQuality.Default);

				if (error != ZLibError.Okay)
				{
					Console.WriteLine("WARNING: Unable to compress admin packet, zlib error: {0}", error);
					return p;
				}
				else
				{
					return new AdminCompressedPacket(dest, destSize, length);
				}
			}
			else
			{
				return p;
			}
		}
	}

	public class EventTextWriter : System.IO.TextWriter
	{
		public delegate void OnConsoleChar(char ch);
		public delegate void OnConsoleLine(string line);
		public delegate void OnConsoleStr(string str);

		private readonly OnConsoleChar m_OnChar;
		private readonly OnConsoleLine m_OnLine;
		private readonly OnConsoleStr m_OnStr;

		public EventTextWriter(OnConsoleChar onChar, OnConsoleLine onLine, OnConsoleStr onStr)
		{
			m_OnChar = onChar;
			m_OnLine = onLine;
			m_OnStr = onStr;
		}

		public override void Write(char ch)
		{
			if (m_OnChar != null)
			{
				m_OnChar(ch);
			}
		}

		public override void Write(string str)
		{
			if (m_OnStr != null)
			{
				m_OnStr(str);
			}
		}

		public override void WriteLine(string line)
		{
			if (m_OnLine != null)
			{
				m_OnLine(line);
			}
		}

		public override System.Text.Encoding Encoding => System.Text.Encoding.ASCII;
	}

	public class RemoteAdminHandlers
	{
		public enum AcctSearchType : byte
		{
			Username = 0,
			IP = 1,
		}

		private static readonly OnPacketReceive[] m_Handlers = new OnPacketReceive[256];

		static RemoteAdminHandlers()
		{
			//0x02 = login request, handled by AdminNetwork
			Register(0x04, new OnPacketReceive(ServerInfoRequest));
			Register(0x05, new OnPacketReceive(AccountSearch));
			Register(0x06, new OnPacketReceive(RemoveAccount));
			Register(0x07, new OnPacketReceive(UpdateAccount));
		}

		public static void Register(byte command, OnPacketReceive handler)
		{
			m_Handlers[command] = handler;
		}

		public static bool Handle(byte command, NetState state, PacketReader pvSrc)
		{
			if (m_Handlers[command] == null)
			{
				Console.WriteLine("ADMIN: Invalid packet 0x{0:X2} from {1}, disconnecting", command, state);
				return false;
			}
			else
			{
				m_Handlers[command](state, pvSrc);
				return true;
			}
		}

		private static void ServerInfoRequest(NetState state, PacketReader pvSrc)
		{
			state.Send(AdminNetwork.Compress(new ServerInfo()));
		}

		private static void AccountSearch(NetState state, PacketReader pvSrc)
		{
			var type = (AcctSearchType)pvSrc.ReadByte();
			var term = pvSrc.ReadString();

			if (type == AcctSearchType.IP && !Utility.IsValidIP(term))
			{
				state.Send(new MessageBoxMessage("Invalid search term.\nThe IP sent was not valid.", "Invalid IP"));
				return;
			}
			else
			{
				term = term.ToUpper();
			}

			var list = new ArrayList();

			foreach (Account a in Accounts.GetAccounts())
			{
				if (!CanAccessAccount(state.Account, a))
				{
					continue;
				}

				switch (type)
				{
					case AcctSearchType.Username:
						{
							if (a.Username.ToUpper().IndexOf(term) != -1)
							{
								list.Add(a);
							}

							break;
						}
					case AcctSearchType.IP:
						{
							for (var i = 0; i < a.LoginIPs.Length; i++)
							{
								if (Utility.IPMatch(term, a.LoginIPs[i]))
								{
									list.Add(a);
									break;
								}
							}
							break;
						}
				}
			}

			if (list.Count > 0)
			{
				if (list.Count <= 25)
				{
					state.Send(AdminNetwork.Compress(new AccountSearchResults(list)));
				}
				else
				{
					state.Send(new MessageBoxMessage("There were more than 25 matches to your search.\nNarrow the search parameters and try again.", "Too Many Results"));
				}
			}
			else
			{
				state.Send(new MessageBoxMessage("There were no results to your search.\nPlease try again.", "No Matches"));
			}
		}

		private static bool CanAccessAccount(IAccount beholder, IAccount beheld)
		{
			return beholder.AccessLevel == AccessLevel.Owner || beheld.AccessLevel < beholder.AccessLevel;  // Cannot see accounts of equal or greater access level unless Owner
		}

		private static void RemoveAccount(NetState state, PacketReader pvSrc)
		{
			if (state.Account.AccessLevel < AccessLevel.Administrator)
			{
				state.Send(new MessageBoxMessage("You do not have permission to delete accounts.", "Account Access Exception"));
				return;
			}

			var a = Accounts.GetAccount(pvSrc.ReadString());

			if (a == null)
			{
				state.Send(new MessageBoxMessage("The account could not be found (and thus was not deleted).", "Account Not Found"));
			}
			else if (!CanAccessAccount(state.Account, a))
			{
				state.Send(new MessageBoxMessage("You cannot delete an account with an access level greater than or equal to your own.", "Account Access Exception"));
			}
			else if (a == state.Account)
			{
				state.Send(new MessageBoxMessage("You may not delete your own account.", "Not Allowed"));
			}
			else
			{
				RemoteAdminLogging.WriteLine(state, "Deleted Account {0}", a);
				a.Delete();
				state.Send(new MessageBoxMessage("The requested account (and all it's characters) has been deleted.", "Account Deleted"));
			}
		}

		private static void UpdateAccount(NetState state, PacketReader pvSrc)
		{
			if (state.Account.AccessLevel < AccessLevel.Administrator)
			{
				state.Send(new MessageBoxMessage("You do not have permission to edit accounts.", "Account Access Exception"));
				return;
			}

			var username = pvSrc.ReadString();
			var pass = pvSrc.ReadString();

			var a = Accounts.GetAccount(username) as Account;

			if (a != null && !CanAccessAccount(state.Account, a))
			{
				state.Send(new MessageBoxMessage("You cannot edit an account with an access level greater than or equal to your own.", "Account Access Exception"));
			}
			else
			{
				var CreatedAccount = false;
				var UpdatedPass = false;
				var oldbanned = a == null ? false : a.Banned;
				var oldAcessLevel = a == null ? 0 : a.AccessLevel;

				if (a == null)
				{
					a = new Account(username, pass);
					CreatedAccount = true;
				}
				else if (pass != "(hidden)")
				{
					a.SetPassword(pass);
					UpdatedPass = true;
				}

				if (a != state.Account)
				{
					var newAccessLevel = (AccessLevel)pvSrc.ReadByte();
					if (a.AccessLevel != newAccessLevel)
					{
						if (newAccessLevel >= state.Account.AccessLevel)
						{
							state.Send(new MessageBoxMessage("Warning: You may not set an access level greater than or equal to your own.", "Account Access Level update denied."));
						}
						else
						{
							a.AccessLevel = newAccessLevel;
						}
					}
					var newBanned = pvSrc.ReadBoolean();
					if (newBanned != a.Banned)
					{
						oldbanned = a.Banned;
						a.Banned = newBanned;
						a.Comments.Add(new AccountComment(state.Account.Username, newBanned ? "Banned via Remote Admin" : "Unbanned via Remote Admin"));
					}
				}
				else
				{
					pvSrc.ReadInt16();//skip both
					state.Send(new MessageBoxMessage("Warning: When editing your own account, Account Status and Access Level cannot be changed.", "Editing Own Account"));
				}

				var list = new ArrayList();
				var length = pvSrc.ReadUInt16();
				var invalid = false;
				for (var i = 0; i < length; i++)
				{
					var add = pvSrc.ReadString();
					if (Utility.IsValidIP(add))
					{
						list.Add(add);
					}
					else
					{
						invalid = true;
					}
				}

				if (list.Count > 0)
				{
					a.IPRestrictions = (string[])list.ToArray(typeof(string));
				}
				else
				{
					a.IPRestrictions = new string[0];
				}

				if (invalid)
				{
					state.Send(new MessageBoxMessage("Warning: one or more of the IP Restrictions you specified was not valid.", "Invalid IP Restriction"));
				}

				if (CreatedAccount)
				{
					RemoteAdminLogging.WriteLine(state, "Created account {0} with Access Level {1}", a.Username, a.AccessLevel);
				}
				else
				{
					var changes = String.Empty;
					if (UpdatedPass)
					{
						changes += " Password Changed.";
					}

					if (oldAcessLevel != a.AccessLevel)
					{
						changes = String.Format("{0} Access level changed from {1} to {2}.", changes, oldAcessLevel, a.AccessLevel);
					}

					if (oldbanned != a.Banned)
					{
						changes += a.Banned ? " Banned." : " Unbanned.";
					}

					RemoteAdminLogging.WriteLine(state, "Updated account {0}:{1}", a.Username, changes);
				}

				state.Send(new MessageBoxMessage("Account updated successfully.", "Account Updated"));
			}
		}
	}

	#region RemoteAdmin Packets

	public enum LoginResponse : byte
	{
		NoUser = 0,
		BadIP,
		BadPass,
		NoAccess,
		OK
	}

	public sealed class AdminCompressedPacket : Packet
	{
		public AdminCompressedPacket(byte[] CompData, int CDLen, int unCompSize) : base(0x01)
		{
			EnsureCapacity(1 + 2 + 2 + CDLen);
			m_Stream.Write((ushort)unCompSize);
			m_Stream.Write(CompData, 0, CDLen);
		}
	}

	public sealed class Login : Packet
	{
		public Login(LoginResponse resp) : base(0x02, 2)
		{
			m_Stream.Write((byte)resp);
		}
	}

	public sealed class ConsoleData : Packet
	{
		public ConsoleData(string str) : base(0x03)
		{
			EnsureCapacity(1 + 2 + 1 + str.Length + 1);
			m_Stream.Write((byte)2);

			m_Stream.WriteAsciiNull(str);
		}

		public ConsoleData(char ch) : base(0x03)
		{
			EnsureCapacity(1 + 2 + 1 + 1);
			m_Stream.Write((byte)3);

			m_Stream.Write((byte)ch);
		}
	}

	public sealed class ServerInfo : Packet
	{
		public ServerInfo() : base(0x04)
		{
			var netVer = Environment.Version.ToString();
			var os = Environment.OSVersion.ToString();

			EnsureCapacity(1 + 2 + (10 * 4) + netVer.Length + 1 + os.Length + 1);
			var banned = 0;
			var active = 0;

			foreach (Account acct in Accounts.GetAccounts())
			{
				if (acct.Banned)
				{
					++banned;
				}
				else
				{
					++active;
				}
			}

			m_Stream.Write(active);
			m_Stream.Write(banned);
			m_Stream.Write(Firewall.List.Count);
			m_Stream.Write(NetState.Instances.Count);

			m_Stream.Write(World.Mobiles.Count);
			m_Stream.Write(Core.ScriptMobiles);
			m_Stream.Write(World.Items.Count);
			m_Stream.Write(Core.ScriptItems);

			m_Stream.Write((uint)(DateTime.UtcNow - Clock.ServerStart).TotalSeconds);
			m_Stream.Write((uint)GC.GetTotalMemory(false));                        // TODO: uint not sufficient for TotalMemory (long). Fix protocol.
			m_Stream.WriteAsciiNull(netVer);
			m_Stream.WriteAsciiNull(os);
		}
	}

	public sealed class AccountSearchResults : Packet
	{
		public AccountSearchResults(ArrayList results) : base(0x05)
		{
			EnsureCapacity(1 + 2 + 2);

			m_Stream.Write((byte)results.Count);

			foreach (Account a in results)
			{
				m_Stream.WriteAsciiNull(a.Username);

				var pwToSend = a.PlainPassword;

				if (pwToSend == null)
				{
					pwToSend = "(hidden)";
				}

				m_Stream.WriteAsciiNull(pwToSend);
				m_Stream.Write((byte)a.AccessLevel);
				m_Stream.Write(a.Banned);
				unchecked { m_Stream.Write((uint)a.LastLogin.Ticks); } // TODO: This doesn't work, uint.MaxValue is only 7 minutes of ticks. Fix protocol.

				m_Stream.Write((ushort)a.LoginIPs.Length);
				for (var i = 0; i < a.LoginIPs.Length; i++)
				{
					m_Stream.WriteAsciiNull(a.LoginIPs[i].ToString());
				}

				m_Stream.Write((ushort)a.IPRestrictions.Length);
				for (var i = 0; i < a.IPRestrictions.Length; i++)
				{
					m_Stream.WriteAsciiNull(a.IPRestrictions[i]);
				}
			}
		}
	}

	public sealed class CompactServerInfo : Packet
	{
		public CompactServerInfo() : base(0x51)
		{
			EnsureCapacity(1 + 2 + (4 * 4) + 8);

			m_Stream.Write(NetState.Instances.Count - 1);                      // Clients
			m_Stream.Write(World.Items.Count);                                 // Items
			m_Stream.Write(World.Mobiles.Count);                               // Mobiles
			m_Stream.Write((uint)(DateTime.UtcNow - Clock.ServerStart).TotalSeconds);  // Age (seconds)

			var memory = GC.GetTotalMemory(false);
			m_Stream.Write((uint)(memory >> 32));                                   // Memory high bytes
			m_Stream.Write((uint)memory);                                           // Memory low bytes
		}
	}

	public sealed class UOGInfo : Packet
	{
		public UOGInfo(string str) : base(0x52, str.Length + 6) // 'R'
		{
			m_Stream.WriteAsciiFixed("unUO", 4);
			m_Stream.WriteAsciiNull(str);
		}
	}

	public sealed class MessageBoxMessage : Packet
	{
		public MessageBoxMessage(string msg, string caption) : base(0x08)
		{
			EnsureCapacity(1 + 2 + msg.Length + 1 + caption.Length + 1);

			m_Stream.WriteAsciiNull(msg);
			m_Stream.WriteAsciiNull(caption);
		}
	}

	#endregion

	public class RemoteAdminLogging
	{
		private const string LogBaseDirectory = "Logs";
		private const string LogSubDirectory = "RemoteAdmin";

		private static StreamWriter m_Output;
		private static bool m_Enabled = true;

		public static bool Enabled { get => m_Enabled; set => m_Enabled = value; }

		public static StreamWriter Output => m_Output;

		private static bool Initialized = false;
		public static void LazyInitialize()
		{
			if (Initialized || !m_Enabled)
			{
				return;
			}

			Initialized = true;

			if (!Directory.Exists(LogBaseDirectory))
			{
				Directory.CreateDirectory(LogBaseDirectory);
			}

			var directory = Path.Combine(LogBaseDirectory, LogSubDirectory);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			try
			{
				m_Output = new StreamWriter(Path.Combine(directory, String.Format(LogSubDirectory + "{0}.log", DateTime.UtcNow.ToString("yyyyMMdd"))), true) {
					AutoFlush = true
				};

				m_Output.WriteLine("##############################");
				m_Output.WriteLine("Log started on {0}", DateTime.UtcNow);
				m_Output.WriteLine();
			}
			catch
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("RemoteAdminLogging: Failed to initialize LogWriter.");
				Utility.PopColor();
				m_Enabled = false;
			}
		}

		public static object Format(object o)
		{
			o = Commands.CommandLogging.Format(o);
			if (o == null)
			{
				return "(null)";
			}

			return o;
		}

		public static void WriteLine(NetState state, string format, params object[] args)
		{
			for (var i = 0; i < args.Length; i++)
			{
				args[i] = Commands.CommandLogging.Format(args[i]);
			}

			WriteLine(state, String.Format(format, args));
		}

		public static void WriteLine(NetState state, string text)
		{
			LazyInitialize();

			if (!m_Enabled)
			{
				return;
			}

			try
			{
				var acct = state.Account as Account;
				var name = acct == null ? "(UNKNOWN)" : acct.Username;
				var accesslevel = acct == null ? "NoAccount" : acct.AccessLevel.ToString();
				var statestr = state == null ? "NULLSTATE" : state.ToString();

				m_Output.WriteLine("{0}: {1}: {2}: {3}", DateTime.UtcNow, statestr, name, text);

				var path = Core.BaseDirectory;

				Commands.CommandLogging.AppendPath(ref path, LogBaseDirectory);
				Commands.CommandLogging.AppendPath(ref path, LogSubDirectory);
				Commands.CommandLogging.AppendPath(ref path, accesslevel);
				path = Path.Combine(path, String.Format("{0}.log", name));

				using (var sw = new StreamWriter(path, true))
				{
					sw.WriteLine("{0}: {1}: {2}", DateTime.UtcNow, statestr, text);
				}
			}
			catch
			{
			}
		}
	}
}