using Server.Commands;

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Server.Accounting
{
	public class Accounts
	{
		private static Dictionary<string, IAccount> m_Accounts = new Dictionary<string, IAccount>();

		public static string FilePath => Path.Combine(Core.CurrentSavesDirectory, "Accounts", "accounts.xml");

		public static int Count => m_Accounts.Count;

		public static void Configure()
		{
			EventSink.WorldLoad += new WorldLoadEventHandler(Load);
			EventSink.WorldSave += new WorldSaveEventHandler(Save);
		}

		public static ICollection<IAccount> GetAccounts()
		{
			return m_Accounts.Values;
		}

		public static IAccount GetAccount(string username)
		{
			IAccount a;

			m_Accounts.TryGetValue(username, out a);

			return a;
		}

		public static void Add(IAccount a)
		{
			m_Accounts[a.Username] = a;
		}

		public static void Remove(string username)
		{
			m_Accounts.Remove(username);
		}

		public static void Load()
		{
			m_Accounts = new Dictionary<string, IAccount>(32, StringComparer.OrdinalIgnoreCase);

			var filePath = FilePath;

			if (!File.Exists(filePath))
			{
				return;
			}

			var doc = new XmlDocument();
			doc.Load(filePath);

			var root = doc["accounts"];

			foreach (XmlElement account in root.GetElementsByTagName("account"))
			{
				try
				{
					var acct = new Account(account);
				}
				catch
				{
					Console.WriteLine("Warning: Account instance load failed");
				}
			}
		}

		public static void Save(WorldSaveEventArgs e)
		{
			var filePath = FilePath;

			Directory.CreateDirectory(Path.GetDirectoryName(filePath));

			using (var op = new StreamWriter(filePath))
			{
				var xml = new XmlTextWriter(op) {
					Formatting = Formatting.Indented,
					IndentChar = '\t',
					Indentation = 1
				};

				xml.WriteStartDocument(true);

				xml.WriteStartElement("accounts");

				xml.WriteAttributeString("count", m_Accounts.Count.ToString());

				foreach (Account a in GetAccounts())
				{
					a.Save(xml);
				}

				xml.WriteEndElement();

				xml.Close();
			}
		}
	}
}

namespace Server.Commands
{
	public partial class CommandHandlers
	{
		[Usage("Save")]
		[Description("Saves the world.")]
		private static void Save_OnCommand(CommandEventArgs e)
		{
			Misc.AutoSave.Save();
		}

		[Usage("BackgroundSave")]
		[Aliases("BGSave", "SaveBG")]
		[Description("Saves the world, writing to the disk in the background")]
		private static void BackgroundSave_OnCommand(CommandEventArgs e)
		{
			Misc.AutoSave.Save(true);
		}
	}
}

namespace Server.Misc
{
	public class AutoSave : Timer
	{
		private static readonly TimeSpan m_Delay = TimeSpan.FromMinutes(5.0);
		private static readonly TimeSpan m_Warning = TimeSpan.Zero;
		//private static TimeSpan m_Warning = TimeSpan.FromSeconds( 15.0 );

		public static void Initialize()
		{
			new AutoSave().Start();
			CommandSystem.Register("SetSaves", AccessLevel.Administrator, new CommandEventHandler(SetSaves_OnCommand));
		}

		private static bool m_SavesEnabled = true;

		public static bool SavesEnabled
		{
			get => m_SavesEnabled;
			set => m_SavesEnabled = value;
		}

		[Usage("SetSaves <true | false>")]
		[Description("Enables or disables automatic shard saving.")]
		public static void SetSaves_OnCommand(CommandEventArgs e)
		{
			if (e.Length == 1)
			{
				m_SavesEnabled = e.GetBoolean(0);
				e.Mobile.SendMessage("Saves have been {0}.", m_SavesEnabled ? "enabled" : "disabled");
			}
			else
			{
				e.Mobile.SendMessage("Format: SetSaves <true | false>");
			}
		}

		public AutoSave() : base(m_Delay - m_Warning, m_Delay)
		{
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			if (!m_SavesEnabled || AutoRestart.Restarting)
			{
				return;
			}

			if (m_Warning == TimeSpan.Zero)
			{
				Save(true);
			}
			else
			{
				var s = (int)m_Warning.TotalSeconds;
				var m = s / 60;
				s %= 60;

				if (m > 0 && s > 0)
				{
					World.Broadcast(0x35, true, "The world will save in {0} minute{1} and {2} second{3}.", m, m != 1 ? "s" : "", s, s != 1 ? "s" : "");
				}
				else if (m > 0)
				{
					World.Broadcast(0x35, true, "The world will save in {0} minute{1}.", m, m != 1 ? "s" : "");
				}
				else
				{
					World.Broadcast(0x35, true, "The world will save in {0} second{1}.", s, s != 1 ? "s" : "");
				}

				Timer.DelayCall(m_Warning, Save);
			}
		}

		public static void Save()
		{
			AutoSave.Save(false);
		}

		public static void Save(bool permitBackgroundWrite)
		{
			if (AutoRestart.Restarting)
			{
				return;
			}

			World.WaitForWriteCompletion();

			try { Backup(); }
			catch (Exception e) { Console.WriteLine("WARNING: Automatic backup FAILED: {0}", e); }

			World.Save(true, permitBackgroundWrite);
		}

		private static readonly string[] m_Backups = new string[]
			{
				"Third Backup",
				"Second Backup",
				"Most Recent"
			};

		private static void Backup()
		{
			if (m_Backups.Length == 0)
			{
				return;
			}

			var root = Path.Combine(Core.BaseDirectory, "Export/Saves/Archive"); // Where It Saves The Backup

			if (!Directory.Exists(root))
			{
				Directory.CreateDirectory(root);
			}

			var existing = Directory.GetDirectories(root);

			for (var i = 0; i < m_Backups.Length; ++i)
			{
				var dir = Match(existing, m_Backups[i]);

				if (dir == null)
				{
					continue;
				}

				if (i > 0)
				{
					var timeStamp = FindTimeStamp(dir.Name);

					if (timeStamp != null)
					{
						try { dir.MoveTo(FormatDirectory(root, m_Backups[i - 1], timeStamp)); }
						catch { }
					}
				}
				else
				{
					try { dir.Delete(true); }
					catch { }
				}
			}

			var saves = Path.Combine(Core.BaseDirectory, "Export/Saves/Current"); // Directory Being Backed Up

			if (Directory.Exists(saves))
			{
				Directory.Move(saves, FormatDirectory(root, m_Backups[m_Backups.Length - 1], GetTimeStamp()));
			}
		}

		private static DirectoryInfo Match(string[] paths, string match)
		{
			for (var i = 0; i < paths.Length; ++i)
			{
				var info = new DirectoryInfo(paths[i]);

				if (info.Name.StartsWith(match))
				{
					return info;
				}
			}

			return null;
		}

		private static string FormatDirectory(string root, string name, string timeStamp)
		{
			return Path.Combine(root, String.Format("{0} ({1})", name, timeStamp));
		}

		private static string FindTimeStamp(string input)
		{
			var start = input.IndexOf('(');

			if (start >= 0)
			{
				var end = input.IndexOf(')', ++start);

				if (end >= start)
				{
					return input.Substring(start, end - start);
				}
			}

			return null;
		}

		private static string GetTimeStamp()
		{
			var now = DateTime.UtcNow;

			return String.Format("{0}-{1}-{2} {3}-{4:D2}-{5:D2}",
					now.Day,
					now.Month,
					now.Year,
					now.Hour,
					now.Minute,
					now.Second
				);
		}
	}
}