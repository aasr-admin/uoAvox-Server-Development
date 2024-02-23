using Server.ContextMenus;
using Server.Multis;
using Server.Network;
using Server.Prompts;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	/// BulletinBoard: Civic
	public abstract class BaseBulletinBoard : Item
	{
		private string m_BoardName;

		[CommandProperty(AccessLevel.GameMaster)]
		public string BoardName
		{
			get => m_BoardName;
			set => m_BoardName = value;
		}

		public BaseBulletinBoard(int itemID) : base(itemID)
		{
			m_BoardName = "bulletin board";
			Movable = false;
		}

		// Threads will be removed six hours after the last post was made
		private static readonly TimeSpan ThreadDeletionTime = TimeSpan.FromHours(6.0);

		// A player may only create a thread once every two minutes
		private static readonly TimeSpan ThreadCreateTime = TimeSpan.FromMinutes(2.0);

		// A player may only reply once every thirty seconds
		private static readonly TimeSpan ThreadReplyTime = TimeSpan.FromSeconds(30.0);

		public static bool CheckTime(DateTime time, TimeSpan range)
		{
			return (time + range) < DateTime.UtcNow;
		}

		public static string FormatTS(TimeSpan ts)
		{
			var totalSeconds = (int)ts.TotalSeconds;
			var seconds = totalSeconds % 60;
			var minutes = totalSeconds / 60;

			if (minutes != 0 && seconds != 0)
			{
				return String.Format("{0} minute{1} and {2} second{3}", minutes, minutes == 1 ? "" : "s", seconds, seconds == 1 ? "" : "s");
			}
			else if (minutes != 0)
			{
				return String.Format("{0} minute{1}", minutes, minutes == 1 ? "" : "s");
			}
			else
			{
				return String.Format("{0} second{1}", seconds, seconds == 1 ? "" : "s");
			}
		}

		public virtual void Cleanup()
		{
			var items = Items;

			for (var i = items.Count - 1; i >= 0; --i)
			{
				if (i >= items.Count)
				{
					continue;
				}

				var msg = items[i] as BulletinMessage;

				if (msg == null)
				{
					continue;
				}

				if (msg.Thread == null && CheckTime(msg.LastPostTime, ThreadDeletionTime))
				{
					msg.Delete();
					RecurseDelete(msg); // A root-level thread has expired
				}
			}
		}

		private void RecurseDelete(BulletinMessage msg)
		{
			var found = new List<Item>();
			var items = Items;

			for (var i = items.Count - 1; i >= 0; --i)
			{
				if (i >= items.Count)
				{
					continue;
				}

				var check = items[i] as BulletinMessage;

				if (check == null)
				{
					continue;
				}

				if (check.Thread == msg)
				{
					check.Delete();
					found.Add(check);
				}
			}

			for (var i = 0; i < found.Count; ++i)
			{
				RecurseDelete((BulletinMessage)found[i]);
			}
		}

		public virtual bool GetLastPostTime(Mobile poster, bool onlyCheckRoot, ref DateTime lastPostTime)
		{
			var items = Items;
			var wasSet = false;

			for (var i = 0; i < items.Count; ++i)
			{
				var msg = items[i] as BulletinMessage;

				if (msg == null || msg.Poster != poster)
				{
					continue;
				}

				if (onlyCheckRoot && msg.Thread != null)
				{
					continue;
				}

				if (msg.Time > lastPostTime)
				{
					wasSet = true;
					lastPostTime = msg.Time;
				}
			}

			return wasSet;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (CheckRange(from))
			{
				Cleanup();

				var state = from.NetState;

				state.Send(new BBDisplayBoard(this));
				if (state.ContainerGridLines)
				{
					state.Send(new ContainerContent6017(from, this));
				}
				else
				{
					state.Send(new ContainerContent(from, this));
				}
			}
			else
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		public virtual bool CheckRange(Mobile from)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}

			return (from.Map == Map && from.InRange(GetWorldLocation(), 2));
		}

		public void PostMessage(Mobile from, BulletinMessage thread, string subject, string[] lines)
		{
			if (thread != null)
			{
				thread.LastPostTime = DateTime.UtcNow;
			}

			AddItem(new BulletinMessage(from, thread, subject, lines));
		}

		public BaseBulletinBoard(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_BoardName);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_BoardName = reader.ReadString();
						break;
					}
			}
		}

		public static void Initialize()
		{
			PacketHandlers.Register(0x71, 0, true, new OnPacketReceive(BBClientRequest));
		}

		public static void BBClientRequest(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			int packetID = pvSrc.ReadByte();
			var board = pvSrc.ReadItem() as BaseBulletinBoard;

			if (board == null || !board.CheckRange(from))
			{
				return;
			}

			switch (packetID)
			{
				case 3: BBRequestContent(from, board, pvSrc); break;
				case 4: BBRequestHeader(from, board, pvSrc); break;
				case 5: BBPostMessage(from, board, pvSrc); break;
				case 6: BBRemoveMessage(from, board, pvSrc); break;
			}
		}

		public static void BBRequestContent(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
		{
			var msg = pvSrc.ReadItem() as BulletinMessage;

			if (msg == null || msg.Parent != board)
			{
				return;
			}

			from.Send(new BBMessageContent(board, msg));
		}

		public static void BBRequestHeader(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
		{
			var msg = pvSrc.ReadItem() as BulletinMessage;

			if (msg == null || msg.Parent != board)
			{
				return;
			}

			from.Send(new BBMessageHeader(board, msg));
		}

		public static void BBPostMessage(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
		{
			var thread = pvSrc.ReadItem() as BulletinMessage;

			if (thread != null && thread.Parent != board)
			{
				thread = null;
			}

			var breakout = 0;

			while (thread != null && thread.Thread != null && breakout++ < 10)
			{
				thread = thread.Thread;
			}

			var lastPostTime = DateTime.MinValue;

			if (board.GetLastPostTime(from, (thread == null), ref lastPostTime))
			{
				if (!CheckTime(lastPostTime, (thread == null ? ThreadCreateTime : ThreadReplyTime)))
				{
					if (thread == null)
					{
						from.SendMessage("You must wait {0} before creating a new thread.", FormatTS(ThreadCreateTime));
					}
					else
					{
						from.SendMessage("You must wait {0} before replying to another thread.", FormatTS(ThreadReplyTime));
					}

					return;
				}
			}

			var subject = pvSrc.ReadUTF8StringSafe(pvSrc.ReadByte());

			if (subject.Length == 0)
			{
				return;
			}

			var lines = new string[pvSrc.ReadByte()];

			if (lines.Length == 0)
			{
				return;
			}

			for (var i = 0; i < lines.Length; ++i)
			{
				lines[i] = pvSrc.ReadUTF8StringSafe(pvSrc.ReadByte());
			}

			board.PostMessage(from, thread, subject, lines);
		}

		public static void BBRemoveMessage(Mobile from, BaseBulletinBoard board, PacketReader pvSrc)
		{
			var msg = pvSrc.ReadItem() as BulletinMessage;

			if (msg == null || msg.Parent != board)
			{
				return;
			}

			if (from.AccessLevel < AccessLevel.GameMaster && msg.Poster != from)
			{
				return;
			}

			msg.Delete();
		}
	}

	/// BulletinBoard: House
	public abstract class BasePlayerBB : Item, ISecurable
	{
		private PlayerBBMessage m_Greeting;
		private List<PlayerBBMessage> m_Messages;
		private string m_Title;
		private SecureLevel m_Level;

		public List<PlayerBBMessage> Messages => m_Messages;

		public PlayerBBMessage Greeting
		{
			get => m_Greeting;
			set => m_Greeting = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Title
		{
			get => m_Title;
			set => m_Title = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level
		{
			get => m_Level;
			set => m_Level = value;
		}

		public BasePlayerBB(int itemID) : base(itemID)
		{
			m_Messages = new List<PlayerBBMessage>();
			m_Level = SecureLevel.Anyone;
		}

		public BasePlayerBB(Serial serial) : base(serial)
		{
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			SetSecureLevelEntry.AddTo(from, this, list);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1);

			writer.Write((int)m_Level);

			writer.Write(m_Title);

			if (m_Greeting != null)
			{
				writer.Write(true);
				m_Greeting.Serialize(writer);
			}
			else
			{
				writer.Write(false);
			}

			writer.WriteEncodedInt(m_Messages.Count);

			for (var i = 0; i < m_Messages.Count; ++i)
			{
				m_Messages[i].Serialize(writer);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Level = (SecureLevel)reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						if (version < 1)
						{
							m_Level = SecureLevel.Anyone;
						}

						m_Title = reader.ReadString();

						if (reader.ReadBool())
						{
							m_Greeting = new PlayerBBMessage(reader);
						}

						var count = reader.ReadEncodedInt();

						m_Messages = new List<PlayerBBMessage>(count);

						for (var i = 0; i < count; ++i)
						{
							m_Messages.Add(new PlayerBBMessage(reader));
						}

						break;
					}
			}
		}

		public static bool CheckAccess(BaseHouse house, Mobile from)
		{
			if (house.Public || !house.IsAosRules)
			{
				return !house.IsBanned(from);
			}

			return house.HasAccess(from);
		}

		public override void OnDoubleClick(Mobile from)
		{
			var house = BaseHouse.FindHouseAt(this);

			if (house == null || !house.IsLockedDown(this))
			{
				from.SendLocalizedMessage(1062396); // This bulletin board must be locked down in a house to be usable.
			}
			else if (!from.InRange(GetWorldLocation(), 2) || !from.InLOS(this))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
			else if (CheckAccess(house, from))
			{
				from.SendGump(new PlayerBBGump(from, house, this, 0));
			}
		}

		public class PostPrompt : Prompt
		{
			private readonly int m_Page;
			private readonly BaseHouse m_House;
			private readonly BasePlayerBB m_Board;
			private readonly bool m_Greeting;

			public PostPrompt(int page, BaseHouse house, BasePlayerBB board, bool greeting)
			{
				m_Page = page;
				m_House = house;
				m_Board = board;
				m_Greeting = greeting;
			}

			public override void OnCancel(Mobile from)
			{
				OnResponse(from, "");
			}

			public override void OnResponse(Mobile from, string text)
			{
				var page = m_Page;
				var house = m_House;
				var board = m_Board;

				if (house == null || !house.IsLockedDown(board))
				{
					from.SendLocalizedMessage(1062396); // This bulletin board must be locked down in a house to be usable.
					return;
				}
				else if (!from.InRange(board.GetWorldLocation(), 2) || !from.InLOS(board))
				{
					from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
					return;
				}
				else if (!CheckAccess(house, from))
				{
					from.SendLocalizedMessage(1062398); // You are not allowed to post to this bulletin board.
					return;
				}
				else if (m_Greeting && !house.IsOwner(from))
				{
					return;
				}

				text = text.Trim();

				if (text.Length > 255)
				{
					text = text.Substring(0, 255);
				}

				if (text.Length > 0)
				{
					var message = new PlayerBBMessage(DateTime.UtcNow, from, text);

					if (m_Greeting)
					{
						board.Greeting = message;
					}
					else
					{
						board.Messages.Add(message);

						if (board.Messages.Count > 50)
						{
							board.Messages.RemoveAt(0);

							if (page > 0)
							{
								--page;
							}
						}
					}
				}

				from.SendGump(new PlayerBBGump(from, house, board, page));
			}
		}

		public class SetTitlePrompt : Prompt
		{
			private readonly int m_Page;
			private readonly BaseHouse m_House;
			private readonly BasePlayerBB m_Board;

			public SetTitlePrompt(int page, BaseHouse house, BasePlayerBB board)
			{
				m_Page = page;
				m_House = house;
				m_Board = board;
			}

			public override void OnCancel(Mobile from)
			{
				OnResponse(from, "");
			}

			public override void OnResponse(Mobile from, string text)
			{
				var page = m_Page;
				var house = m_House;
				var board = m_Board;

				if (house == null || !house.IsLockedDown(board))
				{
					from.SendLocalizedMessage(1062396); // This bulletin board must be locked down in a house to be usable.
					return;
				}
				else if (!from.InRange(board.GetWorldLocation(), 2) || !from.InLOS(board))
				{
					from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
					return;
				}
				else if (!CheckAccess(house, from))
				{
					from.SendLocalizedMessage(1062398); // You are not allowed to post to this bulletin board.
					return;
				}

				text = text.Trim();

				if (text.Length > 255)
				{
					text = text.Substring(0, 255);
				}

				if (text.Length > 0)
				{
					board.Title = text;
				}

				from.SendGump(new PlayerBBGump(from, house, board, page));
			}
		}
	}
}