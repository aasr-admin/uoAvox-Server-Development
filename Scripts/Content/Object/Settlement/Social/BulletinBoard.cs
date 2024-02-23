using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	/// BulletinBoard: Civic
	[Flipable(0x1E5E, 0x1E5F)]
	public class BulletinBoard : BaseBulletinBoard
	{
		[Constructable]
		public BulletinBoard() : base(0x1E5E)
		{
		}

		public BulletinBoard(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	public struct BulletinEquip
	{
		public int itemID;
		public int hue;

		public BulletinEquip(int itemID, int hue)
		{
			this.itemID = itemID;
			this.hue = hue;
		}
	}

	public class BulletinMessage : Item
	{
		private Mobile m_Poster;
		private string m_Subject;
		private DateTime m_Time, m_LastPostTime;
		private BulletinMessage m_Thread;
		private string m_PostedName;
		private int m_PostedBody;
		private int m_PostedHue;
		private BulletinEquip[] m_PostedEquip;
		private string[] m_Lines;

		public string GetTimeAsString()
		{
			return m_Time.ToString("MMM dd, yyyy");
		}

		public override bool CheckTarget(Mobile from, Server.Targeting.Target targ, object targeted)
		{
			return false;
		}

		public override bool IsAccessibleTo(Mobile check)
		{
			return false;
		}

		public BulletinMessage(Mobile poster, BulletinMessage thread, string subject, string[] lines) : base(0xEB0)
		{
			Movable = false;

			m_Poster = poster;
			m_Subject = subject;
			m_Time = DateTime.UtcNow;
			m_LastPostTime = m_Time;
			m_Thread = thread;
			m_PostedName = m_Poster.Name;
			m_PostedBody = m_Poster.Body;
			m_PostedHue = m_Poster.Hue;
			m_Lines = lines;

			var list = new List<BulletinEquip>();

			for (var i = 0; i < poster.Items.Count; ++i)
			{
				var item = poster.Items[i];

				if (item.Layer >= Layer.OneHanded && item.Layer <= Layer.Mount)
				{
					list.Add(new BulletinEquip(item.ItemID, item.Hue));
				}
			}

			m_PostedEquip = list.ToArray();
		}

		public Mobile Poster => m_Poster;
		public BulletinMessage Thread => m_Thread;
		public string Subject => m_Subject;
		public DateTime Time => m_Time;
		public DateTime LastPostTime { get => m_LastPostTime; set => m_LastPostTime = value; }
		public string PostedName => m_PostedName;
		public int PostedBody => m_PostedBody;
		public int PostedHue => m_PostedHue;
		public BulletinEquip[] PostedEquip => m_PostedEquip;
		public string[] Lines => m_Lines;

		public BulletinMessage(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_Poster);
			writer.Write(m_Subject);
			writer.Write(m_Time);
			writer.Write(m_LastPostTime);
			writer.Write(m_Thread != null);
			writer.Write(m_Thread);
			writer.Write(m_PostedName);
			writer.Write(m_PostedBody);
			writer.Write(m_PostedHue);

			writer.Write(m_PostedEquip.Length);

			for (var i = 0; i < m_PostedEquip.Length; ++i)
			{
				writer.Write(m_PostedEquip[i].itemID);
				writer.Write(m_PostedEquip[i].hue);
			}

			writer.Write(m_Lines.Length);

			for (var i = 0; i < m_Lines.Length; ++i)
			{
				writer.Write(m_Lines[i]);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
				case 0:
					{
						m_Poster = reader.ReadMobile();
						m_Subject = reader.ReadString();
						m_Time = reader.ReadDateTime();
						m_LastPostTime = reader.ReadDateTime();
						var hasThread = reader.ReadBool();
						m_Thread = reader.ReadItem() as BulletinMessage;
						m_PostedName = reader.ReadString();
						m_PostedBody = reader.ReadInt();
						m_PostedHue = reader.ReadInt();

						m_PostedEquip = new BulletinEquip[reader.ReadInt()];

						for (var i = 0; i < m_PostedEquip.Length; ++i)
						{
							m_PostedEquip[i].itemID = reader.ReadInt();
							m_PostedEquip[i].hue = reader.ReadInt();
						}

						m_Lines = new string[reader.ReadInt()];

						for (var i = 0; i < m_Lines.Length; ++i)
						{
							m_Lines[i] = reader.ReadString();
						}

						if (hasThread && m_Thread == null)
						{
							Delete();
						}

						if (version == 0)
						{
							ValidationQueue<BulletinMessage>.Add(this);
						}

						break;
					}
			}
		}

		public void Validate()
		{
			if (!(Parent is BulletinBoard && ((BulletinBoard)Parent).Items.Contains(this)))
			{
				Delete();
			}
		}
	}

	public class BBDisplayBoard : Packet
	{
		public BBDisplayBoard(BaseBulletinBoard board) : base(0x71)
		{
			var name = board.BoardName;

			if (name == null)
			{
				name = "";
			}

			EnsureCapacity(38);

			var buffer = Utility.UTF8.GetBytes(name);

			m_Stream.Write((byte)0x00); // PacketID
			m_Stream.Write(board.Serial); // Bulletin board serial

			// Bulletin board name
			if (buffer.Length >= 29)
			{
				m_Stream.Write(buffer, 0, 29);
				m_Stream.Write((byte)0);
			}
			else
			{
				m_Stream.Write(buffer, 0, buffer.Length);
				m_Stream.Fill(30 - buffer.Length);
			}
		}
	}

	public class BBMessageHeader : Packet
	{
		public BBMessageHeader(BaseBulletinBoard board, BulletinMessage msg) : base(0x71)
		{
			var poster = SafeString(msg.PostedName);
			var subject = SafeString(msg.Subject);
			var time = SafeString(msg.GetTimeAsString());

			EnsureCapacity(22 + poster.Length + subject.Length + time.Length);

			m_Stream.Write((byte)0x01); // PacketID
			m_Stream.Write(board.Serial); // Bulletin board serial
			m_Stream.Write(msg.Serial); // Message serial

			var thread = msg.Thread;

			if (thread == null)
			{
				m_Stream.Write(0); // Thread serial--root
			}
			else
			{
				m_Stream.Write(thread.Serial); // Thread serial--parent
			}

			WriteString(poster);
			WriteString(subject);
			WriteString(time);
		}

		public void WriteString(string v)
		{
			var buffer = Utility.UTF8.GetBytes(v);
			var len = buffer.Length + 1;

			if (len > 255)
			{
				len = 255;
			}

			m_Stream.Write((byte)len);
			m_Stream.Write(buffer, 0, len - 1);
			m_Stream.Write((byte)0);
		}

		public string SafeString(string v)
		{
			if (v == null)
			{
				return String.Empty;
			}

			return v;
		}
	}

	public class BBMessageContent : Packet
	{
		public BBMessageContent(BaseBulletinBoard board, BulletinMessage msg) : base(0x71)
		{
			var poster = SafeString(msg.PostedName);
			var subject = SafeString(msg.Subject);
			var time = SafeString(msg.GetTimeAsString());

			EnsureCapacity(22 + poster.Length + subject.Length + time.Length);

			m_Stream.Write((byte)0x02); // PacketID
			m_Stream.Write(board.Serial); // Bulletin board serial
			m_Stream.Write(msg.Serial); // Message serial

			WriteString(poster);
			WriteString(subject);
			WriteString(time);

			m_Stream.Write((short)msg.PostedBody);
			m_Stream.Write((short)msg.PostedHue);

			var len = msg.PostedEquip.Length;

			if (len > 255)
			{
				len = 255;
			}

			m_Stream.Write((byte)len);

			for (var i = 0; i < len; ++i)
			{
				var eq = msg.PostedEquip[i];

				m_Stream.Write((short)eq.itemID);
				m_Stream.Write((short)eq.hue);
			}

			len = msg.Lines.Length;

			if (len > 255)
			{
				len = 255;
			}

			m_Stream.Write((byte)len);

			for (var i = 0; i < len; ++i)
			{
				WriteString(msg.Lines[i], true);
			}
		}

		public void WriteString(string v)
		{
			WriteString(v, false);
		}

		public void WriteString(string v, bool padding)
		{
			var buffer = Utility.UTF8.GetBytes(v);
			var tail = padding ? 2 : 1;
			var len = buffer.Length + tail;

			if (len > 255)
			{
				len = 255;
			}

			m_Stream.Write((byte)len);
			m_Stream.Write(buffer, 0, len - tail);

			if (padding)
			{
				m_Stream.Write((short)0); // padding compensates for a client bug
			}
			else
			{
				m_Stream.Write((byte)0);
			}
		}

		public string SafeString(string v)
		{
			if (v == null)
			{
				return String.Empty;
			}

			return v;
		}
	}


	/// BulletinBoard: House
	public class PlayerBBSouth : BasePlayerBB
	{
		public override int LabelNumber => 1062421;  // bulletin board (south)

		[Constructable]
		public PlayerBBSouth() : base(0x2311)
		{
			Weight = 15.0;
		}

		public PlayerBBSouth(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	public class PlayerBBEast : BasePlayerBB
	{
		public override int LabelNumber => 1062420;  // bulletin board (east)

		[Constructable]
		public PlayerBBEast() : base(0x2312)
		{
			Weight = 15.0;
		}

		public PlayerBBEast(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	public class PlayerBBMessage
	{
		private DateTime m_Time;
		private Mobile m_Poster;
		private string m_Message;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime Time
		{
			get => m_Time;
			set => m_Time = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Poster
		{
			get => m_Poster;
			set => m_Poster = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Message
		{
			get => m_Message;
			set => m_Message = value;
		}

		public PlayerBBMessage(DateTime time, Mobile poster, string message)
		{
			m_Time = time;
			m_Poster = poster;
			m_Message = message;
		}

		public PlayerBBMessage(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						m_Time = reader.ReadDateTime();
						m_Poster = reader.ReadMobile();
						m_Message = reader.ReadString();
						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_Time);
			writer.Write(m_Poster);
			writer.Write(m_Message);
		}
	}

	public class PlayerBBGump : Gump
	{
		private readonly int m_Page;
		private readonly Mobile m_From;
		private readonly BaseHouse m_House;
		private readonly BasePlayerBB m_Board;

		private const int LabelColor = 0x7FFF;
		private const int LabelHue = 1153;

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var page = m_Page;
			var from = m_From;
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
			else if (!BasePlayerBB.CheckAccess(house, from))
			{
				from.SendLocalizedMessage(1062398); // You are not allowed to post to this bulletin board.
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Post message
					{
						from.Prompt = new BasePlayerBB.PostPrompt(page, house, board, false);
						from.SendLocalizedMessage(1062397); // Please enter your message:

						break;
					}
				case 2: // Set title
					{
						if (house.IsOwner(from))
						{
							from.Prompt = new BasePlayerBB.SetTitlePrompt(page, house, board);
							from.SendLocalizedMessage(1062402); // Enter new title:
						}

						break;
					}
				case 3: // Post greeting
					{
						if (house.IsOwner(from))
						{
							from.Prompt = new BasePlayerBB.PostPrompt(page, house, board, true);
							from.SendLocalizedMessage(1062404); // Enter new greeting (this will always be the first post):
						}

						break;
					}
				case 4: // Scroll up
					{
						if (page == 0)
						{
							page = board.Messages.Count;
						}
						else
						{
							page -= 1;
						}

						from.SendGump(new PlayerBBGump(from, house, board, page));

						break;
					}
				case 5: // Scroll down
					{
						page += 1;
						page %= board.Messages.Count + 1;

						from.SendGump(new PlayerBBGump(from, house, board, page));

						break;
					}
				case 6: // Banish poster
					{
						if (house.IsOwner(from))
						{
							if (page >= 1 && page <= board.Messages.Count)
							{
								var message = board.Messages[page - 1];
								var poster = message.Poster;

								if (poster == null)
								{
									from.SendGump(new PlayerBBGump(from, house, board, page));
									return;
								}

								if (poster.AccessLevel > AccessLevel.Player && from.AccessLevel <= poster.AccessLevel)
								{
									from.SendLocalizedMessage(501354); // Uh oh...a bigger boot may be required.
								}
								else if (house.IsFriend(poster))
								{
									from.SendLocalizedMessage(1060750); // That person is a friend, co-owner, or owner of this house, and therefore cannot be banished!
								}
								else if (poster is PlayerVendor)
								{
									from.SendLocalizedMessage(501351); // You cannot eject a vendor.
								}
								else if (house.Bans.Count >= BaseHouse.MaxBans)
								{
									from.SendLocalizedMessage(501355); // The ban limit for this house has been reached!
								}
								else if (house.IsBanned(poster))
								{
									from.SendLocalizedMessage(501356); // This person is already banned!
								}
								else if (poster is BaseCreature && ((BaseCreature)poster).NoHouseRestrictions)
								{
									from.SendLocalizedMessage(1062040); // You cannot ban that.
								}
								else
								{
									if (!house.Bans.Contains(poster))
									{
										house.Bans.Add(poster);
									}

									from.SendLocalizedMessage(1062417); // That person has been banned from this house.

									if (house.IsInside(poster) && !BasePlayerBB.CheckAccess(house, poster))
									{
										poster.MoveToWorld(house.BanLocation, house.Map);
									}
								}
							}

							from.SendGump(new PlayerBBGump(from, house, board, page));
						}

						break;
					}
				case 7: // Delete message
					{
						if (house.IsOwner(from))
						{
							if (page >= 1 && page <= board.Messages.Count)
							{
								board.Messages.RemoveAt(page - 1);
							}

							from.SendGump(new PlayerBBGump(from, house, board, 0));
						}

						break;
					}
				case 8: // Post props
					{
						if (from.AccessLevel >= AccessLevel.GameMaster)
						{
							var message = board.Greeting;

							if (page >= 1 && page <= board.Messages.Count)
							{
								message = board.Messages[page - 1];
							}

							from.SendGump(new PlayerBBGump(from, house, board, page));
							from.SendGump(new PropertiesGump(from, message));
						}

						break;
					}
			}
		}

		public PlayerBBGump(Mobile from, BaseHouse house, BasePlayerBB board, int page) : base(50, 10)
		{
			from.CloseGump(typeof(PlayerBBGump));

			m_Page = page;
			m_From = from;
			m_House = house;
			m_Board = board;

			AddPage(0);

			AddImage(30, 30, 5400);

			AddButton(393, 145, 2084, 2084, 4, GumpButtonType.Reply, 0); // Scroll up
			AddButton(390, 371, 2085, 2085, 5, GumpButtonType.Reply, 0); // Scroll down

			AddButton(32, 183, 5412, 5413, 1, GumpButtonType.Reply, 0); // Post message

			if (house.IsOwner(from))
			{
				AddButton(63, 90, 5601, 5605, 2, GumpButtonType.Reply, 0);
				AddHtmlLocalized(81, 89, 230, 20, 1062400, LabelColor, false, false); // Set title

				AddButton(63, 109, 5601, 5605, 3, GumpButtonType.Reply, 0);
				AddHtmlLocalized(81, 108, 230, 20, 1062401, LabelColor, false, false); // Post greeting
			}

			var title = board.Title;

			if (title != null)
			{
				AddHtml(183, 68, 180, 23, title, false, false);
			}

			AddHtmlLocalized(385, 89, 60, 20, 1062409, LabelColor, false, false); // Post

			AddLabel(440, 89, LabelHue, page.ToString());
			AddLabel(455, 89, LabelHue, "/");
			AddLabel(470, 89, LabelHue, board.Messages.Count.ToString());

			var message = board.Greeting;

			if (page >= 1 && page <= board.Messages.Count)
			{
				message = board.Messages[page - 1];
			}

			AddImageTiled(150, 220, 240, 1, 2700); // Separator

			AddHtmlLocalized(150, 180, 100, 20, 1062405, 16715, false, false); // Posted On:
			AddHtmlLocalized(150, 200, 100, 20, 1062406, 16715, false, false); // Posted By:

			if (message != null)
			{
				AddHtml(255, 180, 150, 20, message.Time.ToString("yyyy-MM-dd HH:mm:ss"), false, false);

				var poster = message.Poster;
				var name = (poster == null ? null : poster.Name);

				if (name == null || (name = name.Trim()).Length == 0)
				{
					name = "Someone";
				}

				AddHtml(255, 200, 150, 20, name, false, false);

				var body = message.Message;

				if (body == null)
				{
					body = "";
				}

				AddHtml(150, 240, 250, 100, body, false, false);

				if (message != board.Greeting && house.IsOwner(from))
				{
					AddButton(130, 395, 1209, 1210, 6, GumpButtonType.Reply, 0);
					AddHtmlLocalized(150, 393, 150, 20, 1062410, LabelColor, false, false); // Banish Poster

					AddButton(310, 395, 1209, 1210, 7, GumpButtonType.Reply, 0);
					AddHtmlLocalized(330, 393, 150, 20, 1062411, LabelColor, false, false); // Delete Message
				}

				if (from.AccessLevel >= AccessLevel.GameMaster)
				{
					AddButton(135, 242, 1209, 1210, 8, GumpButtonType.Reply, 0); // Post props
				}
			}
		}
	}
}