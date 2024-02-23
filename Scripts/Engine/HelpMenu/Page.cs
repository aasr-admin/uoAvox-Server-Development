using Server.Accounting;
using Server.Commands;
using Server.Engines.Reports;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Prompts;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;

namespace Server.Engines.Help
{
	public enum PageType
	{
		Bug,
		Stuck,
		Account,
		Question,
		Suggestion,
		Other,
		VerbalHarassment,
		PhysicalHarassment
	}

	public class PagePrompt : Prompt
	{
		private readonly PageType m_Type;

		public PagePrompt(PageType type)
		{
			m_Type = type;
		}

		public override void OnCancel(Mobile from)
		{
			from.SendLocalizedMessage(501235, "", 0x35); // Help request aborted.
		}

		public override void OnResponse(Mobile from, string text)
		{
			from.SendLocalizedMessage(501234, "", 0x35); /* The next available Counselor/Game Master will respond as soon as possible.
															* Please check your Journal for messages every few minutes.
															*/

			PageQueue.Enqueue(new PageEntry(from, text, m_Type));
		}
	}

	public class PagePromptGump : Gump
	{
		private readonly Mobile m_From;
		private readonly PageType m_Type;

		public PagePromptGump(Mobile from, PageType type) : base(0, 0)
		{
			m_From = from;
			m_Type = type;

			from.CloseGump(typeof(PagePromptGump));

			AddBackground(50, 50, 540, 350, 2600);

			AddPage(0);

			AddHtmlLocalized(264, 80, 200, 24, 1062524, false, false); // Enter Description
			AddHtmlLocalized(120, 108, 420, 48, 1062638, false, false); // Please enter a brief description (up to 200 characters) of your problem:

			AddBackground(100, 148, 440, 200, 3500);
			AddTextEntry(120, 168, 400, 200, 1153, 0, "");

			AddButton(175, 355, 2074, 2075, 1, GumpButtonType.Reply, 0); // Okay
			AddButton(405, 355, 2073, 2072, 0, GumpButtonType.Reply, 0); // Cancel
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 0)
			{
				m_From.SendLocalizedMessage(501235, "", 0x35); // Help request aborted.
			}
			else
			{
				var entry = info.GetTextEntry(0);
				var text = (entry == null ? "" : entry.Text.Trim());

				if (text.Length == 0)
				{
					m_From.SendMessage(0x35, "You must enter a description.");
					m_From.SendGump(new PagePromptGump(m_From, m_Type));
				}
				else
				{
					m_From.SendLocalizedMessage(501234, "", 0x35); /* The next available Counselor/Game Master will respond as soon as possible.
																	  * Please check your Journal for messages every few minutes.
																	  */

					PageQueue.Enqueue(new PageEntry(m_From, text, m_Type));
				}
			}
		}
	}

	public class PageEntry
	{
		// What page types should have a speech log as attachment?
		public static readonly PageType[] SpeechLogAttachment = new PageType[]
			{
				PageType.VerbalHarassment
			};

		private readonly Mobile m_Sender;
		private Mobile m_Handler;
		private readonly DateTime m_Sent;
		private readonly string m_Message;
		private readonly PageType m_Type;
		private Point3D m_PageLocation;
		private readonly Map m_PageMap;
		private readonly List<SpeechLogEntry> m_SpeechLog;

		private readonly PageInfo m_PageInfo;

		public PageInfo PageInfo => m_PageInfo;

		public Mobile Sender => m_Sender;

		public Mobile Handler
		{
			get => m_Handler;
			set
			{
				PageQueue.OnHandlerChanged(m_Handler, value, this);
				m_Handler = value;
			}
		}

		public DateTime Sent => m_Sent;

		public string Message => m_Message;

		public PageType Type => m_Type;

		public Point3D PageLocation => m_PageLocation;

		public Map PageMap => m_PageMap;

		public List<SpeechLogEntry> SpeechLog => m_SpeechLog;

		private Timer m_Timer;

		public void Stop()
		{
			if (m_Timer != null)
			{
				m_Timer.Stop();
			}

			m_Timer = null;
		}

		public void AddResponse(Mobile mob, string text)
		{
			if (m_PageInfo != null)
			{
				lock (m_PageInfo)
				{
					m_PageInfo.Responses.Add(PageInfo.GetAccount(mob), text);
				}

				if (PageInfo.ResFromResp(text) != PageResolution.None)
				{
					m_PageInfo.UpdateResolver();
				}
			}
		}

		public PageEntry(Mobile sender, string message, PageType type)
		{
			m_Sender = sender;
			m_Sent = DateTime.UtcNow;
			m_Message = Utility.FixHtml(message);
			m_Type = type;
			m_PageLocation = sender.Location;
			m_PageMap = sender.Map;

			var pm = sender as PlayerMobile;
			if (pm != null && pm.SpeechLog != null && Array.IndexOf(SpeechLogAttachment, type) >= 0)
			{
				m_SpeechLog = new List<SpeechLogEntry>(pm.SpeechLog);
			}

			m_Timer = new InternalTimer(this);
			m_Timer.Start();

			var history = Reports.Reports.StaffHistory;

			if (history != null)
			{
				m_PageInfo = new PageInfo(this);

				history.AddPage(m_PageInfo);
			}
		}

		private class InternalTimer : Timer
		{
			private static readonly TimeSpan StatusDelay = TimeSpan.FromMinutes(2.0);

			private readonly PageEntry m_Entry;

			public InternalTimer(PageEntry entry) : base(TimeSpan.FromSeconds(1.0), StatusDelay)
			{
				m_Entry = entry;
			}

			protected override void OnTick()
			{
				var index = PageQueue.IndexOf(m_Entry);

				if (m_Entry.Sender.NetState != null && index != -1)
				{
					m_Entry.Sender.SendLocalizedMessage(1008077, true, (index + 1).ToString()); // Thank you for paging. Queue status : 
					m_Entry.Sender.SendLocalizedMessage(1008084); // You can reference our website at www.uo.com or contact us at support@uo.com. To cancel your page, please select the help button again and select cancel.

					if (m_Entry.Handler != null && m_Entry.Handler.NetState == null)
					{
						m_Entry.Handler = null;
					}
				}
				else
				{
					if (index != -1)
					{
						m_Entry.AddResponse(m_Entry.Sender, "[Logout]");
					}

					PageQueue.Remove(m_Entry);
				}
			}
		}
	}

	public class PageQueue
	{
		private static readonly ArrayList m_List = new ArrayList();
		private static readonly Hashtable m_KeyedByHandler = new Hashtable();
		private static readonly Hashtable m_KeyedBySender = new Hashtable();

		public static void Initialize()
		{
			CommandSystem.Register("Pages", AccessLevel.Counselor, new CommandEventHandler(Pages_OnCommand));
		}

		public static bool CheckAllowedToPage(Mobile from)
		{
			var pm = from as PlayerMobile;

			if (pm == null)
			{
				return true;
			}

			if (pm.DesignContext != null)
			{
				from.SendLocalizedMessage(500182); // You cannot request help while customizing a house or transferring a character.
				return false;
			}
			else if (pm.PagingSquelched)
			{
				from.SendMessage("You cannot request help, sorry.");
				return false;
			}

			return true;
		}

		public static string GetPageTypeName(PageType type)
		{
			if (type == PageType.VerbalHarassment)
			{
				return "Verbal Harassment";
			}
			else if (type == PageType.PhysicalHarassment)
			{
				return "Physical Harassment";
			}
			else
			{
				return type.ToString();
			}
		}

		public static void OnHandlerChanged(Mobile old, Mobile value, PageEntry entry)
		{
			if (old != null)
			{
				m_KeyedByHandler.Remove(old);
			}

			if (value != null)
			{
				m_KeyedByHandler[value] = entry;
			}
		}

		[Usage("Pages")]
		[Description("Opens the page queue menu.")]
		private static void Pages_OnCommand(CommandEventArgs e)
		{
			var entry = (PageEntry)m_KeyedByHandler[e.Mobile];

			if (entry != null)
			{
				e.Mobile.SendGump(new PageEntryGump(e.Mobile, entry));
			}
			else if (m_List.Count > 0)
			{
				e.Mobile.SendGump(new PageQueueGump());
			}
			else
			{
				e.Mobile.SendMessage("The page queue is empty.");
			}
		}

		public static bool IsHandling(Mobile check)
		{
			return m_KeyedByHandler.ContainsKey(check);
		}

		public static bool Contains(Mobile sender)
		{
			return m_KeyedBySender.ContainsKey(sender);
		}

		public static int IndexOf(PageEntry e)
		{
			return m_List.IndexOf(e);
		}

		public static void Cancel(Mobile sender)
		{
			Remove((PageEntry)m_KeyedBySender[sender]);
		}

		public static void Remove(PageEntry e)
		{
			if (e == null)
			{
				return;
			}

			e.Stop();

			m_List.Remove(e);
			m_KeyedBySender.Remove(e.Sender);

			if (e.Handler != null)
			{
				m_KeyedByHandler.Remove(e.Handler);
			}
		}

		public static PageEntry GetEntry(Mobile sender)
		{
			return (PageEntry)m_KeyedBySender[sender];
		}

		public static void Remove(Mobile sender)
		{
			Remove(GetEntry(sender));
		}

		public static ArrayList List => m_List;

		public static void Enqueue(PageEntry entry)
		{
			m_List.Add(entry);
			m_KeyedBySender[entry.Sender] = entry;

			var isStaffOnline = false;

			foreach (var ns in NetState.Instances)
			{
				var m = ns.Mobile;

				if (m != null && m.AccessLevel >= AccessLevel.Counselor && m.AutoPageNotify && !IsHandling(m))
				{
					m.SendMessage("A new page has been placed in the queue.");
				}

				if (m != null && m.AccessLevel >= AccessLevel.Counselor && m.AutoPageNotify && Core.TickCount - m.LastMoveTime < 600000)
				{
					isStaffOnline = true;
				}
			}

			if (!isStaffOnline)
			{
				entry.Sender.SendMessage("We are sorry, but no staff members are currently available to assist you.  Your page will remain in the queue until one becomes available, or until you cancel it manually.");
			}

			if (Email.FromAddress != null && Email.SpeechLogPageAddresses != null && entry.SpeechLog != null)
			{
				SendEmail(entry);
			}
		}

		private static void SendEmail(PageEntry entry)
		{
			var sender = entry.Sender;
			var time = DateTime.UtcNow;

			var mail = new MailMessage(Email.FromAddress, Email.SpeechLogPageAddresses) {
				Subject = "RunUO Speech Log Page Forwarding"
			};

			using (var writer = new StringWriter())
			{
				writer.WriteLine("RunUO Speech Log Page - {0}", PageQueue.GetPageTypeName(entry.Type));
				writer.WriteLine();

				writer.WriteLine("From: '{0}', Account: '{1}'", sender.RawName, sender.Account is Account ? sender.Account.Username : "???");
				writer.WriteLine("Location: {0} [{1}]", sender.Location, sender.Map);
				writer.WriteLine("Sent on: {0}/{1:00}/{2:00} {3}:{4:00}:{5:00}", time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
				writer.WriteLine();

				writer.WriteLine("Message:");
				writer.WriteLine("'{0}'", entry.Message);
				writer.WriteLine();

				writer.WriteLine("Speech Log");
				writer.WriteLine("==========");

				foreach (var logEntry in entry.SpeechLog)
				{
					var from = logEntry.From;
					var fromName = from.RawName;
					var fromAccount = from.Account is Account ? from.Account.Username : "???";
					var created = logEntry.Created;
					var speech = logEntry.Speech;

					writer.WriteLine("{0}:{1:00}:{2:00} - {3} ({4}): '{5}'", created.Hour, created.Minute, created.Second, fromName, fromAccount, speech);
				}

				mail.Body = writer.ToString();
			}

			Email.AsyncSend(mail);
		}
	}

	#region PageQueue Gump

	public class MessageSentGump : Gump
	{
		private readonly string m_Name, m_Text;
		private readonly Mobile m_Mobile;

		public MessageSentGump(Mobile mobile, string name, string text) : base(30, 30)
		{
			m_Name = name;
			m_Text = text;
			m_Mobile = mobile;

			Closable = false;

			AddPage(0);

			AddBackground(0, 0, 92, 75, 0xA3C);

			AddImageTiled(5, 7, 82, 61, 0xA40);
			AddAlphaRegion(5, 7, 82, 61);

			AddImageTiled(9, 11, 21, 53, 0xBBC);

			AddButton(10, 12, 0x7D2, 0x7D2, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(34, 28, 65, 24, 3001002, 0x7FFF, false, false); // Message
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			m_Mobile.SendGump(new PageResponseGump(m_Mobile, m_Name, m_Text));

			//m_Mobile.SendMessage( 0x482, "{0} tells you:", m_Name );
			//m_Mobile.SendMessage( 0x482, m_Text );
		}
	}

	public class PageQueueGump : Gump
	{
		private readonly PageEntry[] m_List;

		public PageQueueGump() : base(30, 30)
		{
			Add(new GumpPage(0));
			//Add( new GumpBackground( 0, 0, 410, 448, 9200 ) );
			Add(new GumpImageTiled(0, 0, 410, 448, 0xA40));
			Add(new GumpAlphaRegion(1, 1, 408, 446));

			Add(new GumpLabel(180, 12, 2100, "Page Queue"));

			var list = PageQueue.List;

			for (var i = 0; i < list.Count;)
			{
				var e = (PageEntry)list[i];

				if (e.Sender.Deleted || e.Sender.NetState == null)
				{
					e.AddResponse(e.Sender, "[Logout]");
					PageQueue.Remove(e);
				}
				else
				{
					++i;
				}
			}

			m_List = (PageEntry[])list.ToArray(typeof(PageEntry));

			if (m_List.Length > 0)
			{
				Add(new GumpPage(1));

				for (var i = 0; i < m_List.Length; ++i)
				{
					var e = m_List[i];

					if (i >= 5 && (i % 5) == 0)
					{
						Add(new GumpButton(368, 12, 0xFA5, 0xFA7, 0, GumpButtonType.Page, (i / 5) + 1));
						Add(new GumpLabel(298, 12, 2100, "Next Page"));
						Add(new GumpPage((i / 5) + 1));
						Add(new GumpButton(12, 12, 0xFAE, 0xFB0, 0, GumpButtonType.Page, (i / 5)));
						Add(new GumpLabel(48, 12, 2100, "Previous Page"));
					}

					var typeString = PageQueue.GetPageTypeName(e.Type);

					var html = String.Format("[{0}] {1} <basefont color=#{2:X6}>[<u>{3}</u>]</basefont>", typeString, e.Message, e.Handler == null ? 0xFF0000 : 0xFF, e.Handler == null ? "Unhandled" : "Handling");

					Add(new GumpHtml(12, 44 + ((i % 5) * 80), 350, 70, html, true, true));
					Add(new GumpButton(370, 44 + ((i % 5) * 80) + 24, 0xFA5, 0xFA7, i + 1, GumpButtonType.Reply, 0));
				}
			}
			else
			{
				Add(new GumpLabel(12, 44, 2100, "The page queue is empty."));
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info.ButtonID >= 1 && info.ButtonID <= m_List.Length)
			{
				if (PageQueue.List.IndexOf(m_List[info.ButtonID - 1]) >= 0)
				{
					var g = new PageEntryGump(state.Mobile, m_List[info.ButtonID - 1]);

					g.SendTo(state);
				}
				else
				{
					state.Mobile.SendGump(new PageQueueGump());
					state.Mobile.SendMessage("That page has been removed.");
				}
			}
		}
	}

	public class PredefinedResponse
	{
		// Format: new("title", "message html")
		public static PredefinedResponse[] List { get; set; } = 
		{
			#region Entries

			new("Not Enough Info",
				"Please provide a more informative description of your problem so that we may better help you."
			),

			new("Bad Stuck",
				"We apologize, but we do not move players unless they are physically stuck."
			),

			new("Cannot Teleport",
				"We apologize, but staff members do not move players across the map."
			),

			new("Item Request",
				"We apologize, but we do not give items to players."
			),

			new("Murder System Query",
				"Hello! Information regarding the murder system is available <a href=\"http://uo.stratics.com/content/reputation/murder.shtml\">here</a>."
			),

			new("Pet Bonding",
				"Hail! The pet bonding system works just as it does on Origin's official UO shards. " +
				"<br>" +
				"You can find all the information you need about it <a href=\"http://uo.stratics.com/content/professions/taming/taming-pets.shtml#bonding\">here</a>!"
			),

			new("Thief Guild",
				"In order to join the thief guild, your character must be at least one week old, " +
				"must have been logged into the game for a total of 48 hours, " +
				"and must have 60.0 or more stealing skill. " +
				"<br>" +
				"We are not be able to tell you how long until your character will meet these requirements. "
			),

			new("Password Change",
				"In order to change your password, please type:" +
				"<br>" +
				"[password <u><i>newpassword</i></u> <u><i>newpassword</i></u>" +
				"<br><br>" +
				"Be sure to replace <u><i>newpassword</i></u> with the password you would like to use."
			),

			new("Cannot Help",
				"Sorry, we cannot help you with that."
			),

			new("Online Request",
				"We do not reveal the online status of players."
			),

			#endregion
		};

		public string Title { get; set; }
		public string Message { get; set; }

		public PredefinedResponse(string title, string message)
		{
			Title = title;
			Message = message;
		}
	}

	public class PredefGump : Gump
	{
		private const int LabelColor32 = 0xFFFFFF;

		public string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public string Color(string text, int color)
		{
			return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
		}

		public PredefGump(Mobile from, PredefinedResponse response) : base(30, 30)
		{
			from.CloseGump(typeof(PredefGump));

			AddPage(0);

			if (response == null)
			{
				AddImageTiled(0, 0, 410, 448, 0xA40);
				AddAlphaRegion(1, 1, 408, 446);

				AddHtml(10, 10, 390, 20, Color(Center("Predefined Responses"), LabelColor32), false, false);

				var list = PredefinedResponse.List;

				AddPage(1);

				int i;

				for (i = 0; i < list.Length; ++i)
				{
					if (i >= 5 && (i % 5) == 0)
					{
						AddButton(368, 10, 0xFA5, 0xFA7, 0, GumpButtonType.Page, (i / 5) + 1);
						AddLabel(298, 10, 2100, "Next Page");
						AddPage((i / 5) + 1);
						AddButton(12, 10, 0xFAE, 0xFB0, 0, GumpButtonType.Page, i / 5);
						AddLabel(48, 10, 2100, "Previous Page");
					}

					var resp = list[i];

					var html = String.Format("<u>{0}</u><br>{1}", resp.Title, resp.Message);

					AddHtml(12, 44 + ((i % 5) * 80), 350, 70, html, true, true);
				}
			}
		}
	}

	public class PageEntryGump : Gump
	{
		private readonly PageEntry m_Entry;
		private readonly Mobile m_Mobile;

		private static readonly int[] m_AccessLevelHues = new int[]
			{
				2100,
				2122,
				2117,
				2129,
				2415,
				2415,
				2415
			};

		public PageEntryGump(Mobile m, PageEntry entry) : base(30, 30)
		{
			try
			{
				m_Mobile = m;
				m_Entry = entry;

				var buttons = 0;

				var bottom = 356;

				AddPage(0);

				AddImageTiled(0, 0, 410, 456, 0xA40);
				AddAlphaRegion(1, 1, 408, 454);

				AddPage(1);

				AddLabel(18, 18, 2100, "Sent:");
				AddLabelCropped(128, 18, 264, 20, 2100, entry.Sent.ToString());

				AddLabel(18, 38, 2100, "Sender:");
				AddLabelCropped(128, 38, 264, 20, 2100, String.Format("{0} {1} [{2}]", entry.Sender.RawName, entry.Sender.Location, entry.Sender.Map));

				AddButton(18, bottom - (buttons * 22), 0xFAB, 0xFAD, 8, GumpButtonType.Reply, 0);
				AddImageTiled(52, bottom - (buttons * 22) + 1, 340, 80, 0xA40/*0xBBC*//*0x2458*/ );
				AddImageTiled(53, bottom - (buttons * 22) + 2, 338, 78, 0xBBC/*0x2426*/ );
				AddTextEntry(55, bottom - (buttons++ * 22) + 2, 336, 78, 0x480, 0, "");

				AddButton(18, bottom - (buttons * 22), 0xFA5, 0xFA7, 0, GumpButtonType.Page, 2);
				AddLabel(52, bottom - (buttons++ * 22), 2100, "Predefined Response");

				if (entry.Sender != m)
				{
					AddButton(18, bottom - (buttons * 22), 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
					AddLabel(52, bottom - (buttons++ * 22), 2100, "Go to Sender");
				}

				AddLabel(18, 58, 2100, "Handler:");

				if (entry.Handler == null)
				{
					AddLabelCropped(128, 58, 264, 20, 2100, "Unhandled");

					AddButton(18, bottom - (buttons * 22), 0xFB1, 0xFB3, 5, GumpButtonType.Reply, 0);
					AddLabel(52, bottom - (buttons++ * 22), 2100, "Delete Page");

					AddButton(18, bottom - (buttons * 22), 0xFB7, 0xFB9, 4, GumpButtonType.Reply, 0);
					AddLabel(52, bottom - (buttons++ * 22), 2100, "Handle Page");
				}
				else
				{
					AddLabelCropped(128, 58, 264, 20, m_AccessLevelHues[(int)entry.Handler.AccessLevel], entry.Handler.Name);

					if (entry.Handler != m)
					{
						AddButton(18, bottom - (buttons * 22), 0xFA5, 0xFA7, 2, GumpButtonType.Reply, 0);
						AddLabel(52, bottom - (buttons++ * 22), 2100, "Go to Handler");
					}
					else
					{
						AddButton(18, bottom - (buttons * 22), 0xFA2, 0xFA4, 6, GumpButtonType.Reply, 0);
						AddLabel(52, bottom - (buttons++ * 22), 2100, "Abandon Page");

						AddButton(18, bottom - (buttons * 22), 0xFB7, 0xFB9, 7, GumpButtonType.Reply, 0);
						AddLabel(52, bottom - (buttons++ * 22), 2100, "Page Handled");
					}
				}

				AddLabel(18, 78, 2100, "Page Location:");
				AddLabelCropped(128, 78, 264, 20, 2100, String.Format("{0} [{1}]", entry.PageLocation, entry.PageMap));

				AddButton(18, bottom - (buttons * 22), 0xFA5, 0xFA7, 3, GumpButtonType.Reply, 0);
				AddLabel(52, bottom - (buttons++ * 22), 2100, "Go to Page Location");

				if (entry.SpeechLog != null)
				{
					AddButton(18, bottom - (buttons * 22), 0xFA5, 0xFA7, 10, GumpButtonType.Reply, 0);
					AddLabel(52, bottom - (buttons++ * 22), 2100, "View Speech Log");
				}

				AddLabel(18, 98, 2100, "Page Type:");
				AddLabelCropped(128, 98, 264, 20, 2100, PageQueue.GetPageTypeName(entry.Type));

				AddLabel(18, 118, 2100, "Message:");
				AddHtml(128, 118, 250, 100, entry.Message, true, true);

				AddPage(2);

				var preresp = PredefinedResponse.List;

				AddButton(18, 18, 0xFAE, 0xFB0, 0, GumpButtonType.Page, 1);
				AddButton(410 - 18 - 32, 18, 0xFAB, 0xFAC, 9, GumpButtonType.Reply, 0);

				if (preresp.Length == 0)
				{
					AddLabel(52, 18, 2100, "There are no predefined responses.");
				}
				else
				{
					AddLabel(52, 18, 2100, "Back");

					for (var i = 0; i < preresp.Length; ++i)
					{
						AddButton(18, 40 + (i * 22), 0xFA5, 0xFA7, 100 + i, GumpButtonType.Reply, 0);
						AddLabel(52, 40 + (i * 22), 2100, preresp[i].Title);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public void Resend(NetState state)
		{
			var g = new PageEntryGump(m_Mobile, m_Entry);

			g.SendTo(state);
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info.ButtonID != 0 && PageQueue.List.IndexOf(m_Entry) < 0)
			{
				state.Mobile.SendGump(new PageQueueGump());
				state.Mobile.SendMessage("That page has been removed.");
				return;
			}

			switch (info.ButtonID)
			{
				case 0: // close
					{
						if (m_Entry.Handler != state.Mobile)
						{
							var g = new PageQueueGump();

							g.SendTo(state);
						}

						break;
					}
				case 1: // go to sender
					{
						var m = state.Mobile;

						if (m_Entry.Sender.Deleted)
						{
							m.SendMessage("That character no longer exists.");
						}
						else if (m_Entry.Sender.Map == null || m_Entry.Sender.Map == Map.Internal)
						{
							m.SendMessage("That character is not in the world.");
						}
						else
						{
							m_Entry.AddResponse(state.Mobile, "[Go Sender]");
							m.MoveToWorld(m_Entry.Sender.Location, m_Entry.Sender.Map);

							m.SendMessage("You have been teleported to that page's sender.");

							Resend(state);
						}

						break;
					}
				case 2: // go to handler
					{
						var m = state.Mobile;
						var h = m_Entry.Handler;

						if (h != null)
						{
							if (h.Deleted)
							{
								m.SendMessage("That character no longer exists.");
							}
							else if (h.Map == null || h.Map == Map.Internal)
							{
								m.SendMessage("That character is not in the world.");
							}
							else
							{
								m_Entry.AddResponse(state.Mobile, "[Go Handler]");
								m.MoveToWorld(h.Location, h.Map);

								m.SendMessage("You have been teleported to that page's handler.");
								Resend(state);
							}
						}
						else
						{
							m.SendMessage("Nobody is handling that page.");
							Resend(state);
						}

						break;
					}
				case 3: // go to page location
					{
						var m = state.Mobile;

						if (m_Entry.PageMap == null || m_Entry.PageMap == Map.Internal)
						{
							m.SendMessage("That location is not in the world.");
						}
						else
						{
							m_Entry.AddResponse(state.Mobile, "[Go PageLoc]");
							m.MoveToWorld(m_Entry.PageLocation, m_Entry.PageMap);

							state.Mobile.SendMessage("You have been teleported to the original page location.");

							Resend(state);
						}

						break;
					}
				case 4: // handle page
					{
						if (m_Entry.Handler == null)
						{
							m_Entry.AddResponse(state.Mobile, "[Handling]");
							m_Entry.Handler = state.Mobile;

							state.Mobile.SendMessage("You are now handling the page.");
						}
						else
						{
							state.Mobile.SendMessage("Someone is already handling that page.");
						}

						Resend(state);

						break;
					}
				case 5: // delete page
					{
						if (m_Entry.Handler == null)
						{
							m_Entry.AddResponse(state.Mobile, "[Deleting]");
							PageQueue.Remove(m_Entry);

							state.Mobile.SendMessage("You delete the page.");

							var g = new PageQueueGump();

							g.SendTo(state);
						}
						else
						{
							state.Mobile.SendMessage("Someone is handling that page, it can not be deleted.");

							Resend(state);
						}

						break;
					}
				case 6: // abandon page
					{
						if (m_Entry.Handler == state.Mobile)
						{
							m_Entry.AddResponse(state.Mobile, "[Abandoning]");
							state.Mobile.SendMessage("You abandon the page.");

							m_Entry.Handler = null;
						}
						else
						{
							state.Mobile.SendMessage("You are not handling that page.");
						}

						Resend(state);

						break;
					}
				case 7: // page handled
					{
						if (m_Entry.Handler == state.Mobile)
						{
							m_Entry.AddResponse(state.Mobile, "[Handled]");
							PageQueue.Remove(m_Entry);

							m_Entry.Handler = null;

							state.Mobile.SendMessage("You mark the page as handled, and remove it from the queue.");

							var g = new PageQueueGump();

							g.SendTo(state);
						}
						else
						{
							state.Mobile.SendMessage("You are not handling that page.");

							Resend(state);
						}

						break;
					}
				case 8: // Send message
					{
						var text = info.GetTextEntry(0);

						if (text != null)
						{
							m_Entry.AddResponse(state.Mobile, "[Response] " + text.Text);
							m_Entry.Sender.SendGump(new MessageSentGump(m_Entry.Sender, state.Mobile.Name, text.Text));
							//m_Entry.Sender.SendMessage( 0x482, "{0} tells you:", state.Mobile.Name );
							//m_Entry.Sender.SendMessage( 0x482, text.Text );
						}

						Resend(state);

						break;
					}
				case 9: // predef overview
					{
						Resend(state);
						state.Mobile.SendGump(new PredefGump(state.Mobile, null));

						break;
					}
				case 10: // View Speech Log
					{
						Resend(state);

						if (m_Entry.SpeechLog != null)
						{
							Gump gump = new SpeechLogGump(m_Entry.Sender, m_Entry.SpeechLog);
							state.Mobile.SendGump(gump);
						}

						break;
					}
				default:
					{
						var index = info.ButtonID - 100;
						var preresp = PredefinedResponse.List;

						if (index >= 0 && index < preresp.Length)
						{
							m_Entry.AddResponse(state.Mobile, "[PreDef] " + preresp[index].Title);
							m_Entry.Sender.SendGump(new MessageSentGump(m_Entry.Sender, state.Mobile.Name, preresp[index].Message));
						}

						Resend(state);

						break;
					}
			}
		}
	}

	#endregion

	public class PageResponseGump : Gump
	{
		private readonly Mobile m_From;
		private readonly string m_Name, m_Text;

		public PageResponseGump(Mobile from, string name, string text) : base(0, 0)
		{
			m_From = from;
			m_Name = name;
			m_Text = text;

			AddBackground(50, 25, 540, 430, 2600);

			AddPage(0);

			AddHtmlLocalized(150, 40, 360, 40, 1062610, false, false); // <CENTER><U>Ultima Online Help Response</U></CENTER>

			AddHtml(80, 90, 480, 290, String.Format("{0} tells {1}: {2}", name, from.Name, text), true, true);

			AddHtmlLocalized(80, 390, 480, 40, 1062611, false, false); // Clicking the OKAY button will remove the reponse you have received.
			AddButton(400, 417, 2074, 2075, 1, GumpButtonType.Reply, 0); // OKAY

			AddButton(475, 417, 2073, 2072, 0, GumpButtonType.Reply, 0); // CANCEL
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID != 1)
			{
				m_From.SendGump(new MessageSentGump(m_From, m_Name, m_Text));
			}
		}
	}
}