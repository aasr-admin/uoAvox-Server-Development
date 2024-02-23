using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

#region Developer Notations

/// ToDo:
/// This system needs to export the .cs file as well
/// This system '.xml' output should be changed to '.json'
/// Double Check And Test If Books Automatically Go Into Library Bookshelves - If NOT, Then Make It Happen
/// Create A New Mobile Called The 'BookSeller' And Sell Player Created Books On Them.

#endregion

namespace Server.Engines.Publishing
{
	public class PublishedBook : BaseBook
	{
		[Constructable]
		public PublishedBook() : this(Utility.Random(Publisher.Books.Count))
		{ }

		[Constructable]
		public PublishedBook(int index) : base(Utility.RandomList(0xFF2, 0xFEF, 0xFF1, 0xFF0))
		{
			Writable = false;
			if (index >= Publisher.Books.Count)
			{
				return;
			}

			var bc = Publisher.Books[index];

			Title = bc.Title;
			Author = bc.Author;

			var pagesSrc = bc.Pages;
			for (var i = 0; i < pagesSrc.Length && i < Pages.Length; i++)
			{
				var pageSrc = pagesSrc[i];
				var pageDst = Pages[i];

				var length = pageSrc.Lines.Length;
				pageDst.Lines = new string[length];

				for (var j = 0; j < length; j++)
				{
					pageDst.Lines[j] = pageSrc.Lines[j];
				}
			}
		}

		public PublishedBook(Serial serial) : base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}

	public class BookPubGump : Gump
	{
		private readonly List<BookContent> bc;

		private readonly int m_Page;

		public BookPubGump(Mobile from) : this(from, 0) { }

		public BookPubGump(Mobile from, int page) : base(0, 0)
		{

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage(0);

			bc = Publisher.Books;

			m_Page = page;

			AddBackground(0, 1, 500, 550, 9200);
			AddLabel(190, 5, 32, "Book Publisher");
			if (page > 0)
			{
				AddButton(10, 515, 4014, 4016, 1, GumpButtonType.Reply, 0);
			}

			if ((page + 1) * 10 < bc.Count)
			{
				AddButton(460, 515, 4005, 4007, 2, GumpButtonType.Reply, 0);
			}

			for (var i = page * 10; i < ((page + 1) * 10 < bc.Count ? (page + 1) * 10 : bc.Count); i++)
			{
				AddBookDetail(i, from.AccessLevel > AccessLevel.Player);
			}
		}

		private void AddBookDetail(int i, bool admin)
		{
			var b = bc[i];

			if (i % 2 != 0)
			{
				AddImageTiled(0, 25 + (45 * (i % 10)), 500, 45, 9274);
			}

			AddItem(5, 35 + (45 * (i % 10)), 4079);

			AddLabel(45, 35 + (45 * (i % 10)), 0, b.Title);
			AddLabel(235, 35 + (45 * (i % 10)), 0, "By: " + b.Author);

			AddButton(admin ? 410 : 460, 35 + (45 * (i % 10)), 4011, 4013, (i * 2) + 3, GumpButtonType.Reply, 0);

			if (admin)
			{
				AddButton(460, 35 + (45 * (i % 10)), 4020, 4022, (i * 2) + 4, GumpButtonType.Reply, 0);
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var from = sender.Mobile;

			switch (info.ButtonID)
			{
				case 0:
					{
						break;
					}
				case 1:
					{
						_ = from.SendGump(new BookPubGump(from, m_Page - 1));
						break;
					}
				case 2:
					{
						_ = from.SendGump(new BookPubGump(from, m_Page + 1));
						break;
					}
				default:
					{
						var i = info.ButtonID - 3;
						switch (i % 2)
						{
							case 0:
								{
									var cost = 300 - from.Backpack.ConsumeUpTo(typeof(Gold), 300);

									if (cost > 0)
									{
										from.SendMessage("You can not afford this.");

										if (300 - cost > 0)
										{
											_ = from.AddToBackpack(new Gold(300 - cost));
										}

										_ = from.SendGump(new BookPubGump(from, m_Page));
										return;
									}

									var pb = new PublishedBook(i / 2);

									if (pb != null)
									{
										_ = from.AddToBackpack(pb);
									}

									break;
								}
							case 1:
								{
									var x = i / 2;
									if (x > -1 && x < bc.Count)
									{
										bc.RemoveAt(x);
									}

									break;
								}
						}

						_ = from.SendGump(new BookPubGump(from, m_Page));
						break;
					}
			}
		}
	}

	public class BP_Save
	{
		public const int Version = 1;

		public static string FilePath => Path.Combine(Core.CurrentSavesDirectory, "Publisher", "books.xml");

		[CallPriority(101)]
		public static void Initialize()
		{
			EventSink.WorldSave += new WorldSaveEventHandler(WriteData);
		}

		public static void WriteData(WorldSaveEventArgs e)
		{
			var filePath = FilePath;

			Directory.CreateDirectory(Path.GetDirectoryName(filePath));

			using var file = new StreamWriter(filePath);
			var xml = new XmlTextWriter(file)
			{
				Formatting = Formatting.Indented,
				IndentChar = '\t',
				Indentation = 1
			};

			xml.WriteStartDocument(true);

			xml.WriteStartElement("BookPublisher");

			xml.WriteAttributeString("Version", Version.ToString());

			for (var i = 0; i < Publisher.Books.Count; i++)
			{
				WriteBookNode(Publisher.Books[i], xml);
			}

			xml.WriteEndElement();

			xml.Close();
		}

		public static void WriteBookNode(BookContent bc, XmlTextWriter xml)
		{
			xml.WriteStartElement("PublishedBook");

			xml.WriteAttributeString("Title", bc.Title);
			xml.WriteAttributeString("Author", bc.Author);
			xml.WriteAttributeString("PagesCount", bc.Pages.Length.ToString());
			for (var i = 0; i < bc.Pages.Length; i++)
			{
				WritePageNode(bc.Pages[i], xml);
			}

			xml.WriteEndElement();
		}

		public static void WritePageNode(BookPageInfo bpi, XmlTextWriter xml)
		{
			xml.WriteStartElement("Page");
			xml.WriteAttributeString("Lines", bpi.Lines.Length.ToString());
			for (var i = 0; i < bpi.Lines.Length; i++)
			{
				xml.WriteAttributeString("Line" + i.ToString(), bpi.Lines[i]);
			}

			xml.WriteEndElement();
		}
	}

	public class BP_Load
	{
		[CallPriority(101)]
		public static void Initialize()
		{
			ReadData(BP_Save.FilePath);
		}

		public static void ReadData(string filePath)
		{
			if (!File.Exists(filePath))
			{
				return;
			}

			Console.WriteLine();
			Console.WriteLine("Book Publishing: Loading...");

			var doc = new XmlDocument();
			doc.Load(filePath);

			var root = doc["BookPublisher"];
			_ = Utility.ToInt32(root.GetAttribute("Version"));

			if (root.HasChildNodes)
			{
				foreach (XmlElement book in root.GetElementsByTagName("PublishedBook"))
				{
					try
					{
						ReadBookNode(book);
					}
					catch
					{
						Console.WriteLine("Warning: Book Publisher load failed.");
					}
				}
			}
		}

		public static void ReadBookNode(XmlElement parent)
		{
			try
			{
				var title = parent.GetAttribute("Title");
				var author = parent.GetAttribute("Author");
				var pgcnt = Utility.ToInt32(parent.GetAttribute("PagesCount"));
				var pages = new BookPageInfo[pgcnt];

				if (parent.HasChildNodes)
				{
					var i = 0;
					var child = parent.FirstChild as XmlElement;
					pages[i++] = ReadPageNode(child);
					while (child.NextSibling != null && i < pages.Length)
					{
						child = child.NextSibling as XmlElement;
						pages[i++] = ReadPageNode(child);
					}
				}

				Publisher.Books.Add(new BookContent(title, author, pages));
			}
			catch
			{
				Console.WriteLine("failed.");
			}

			Console.WriteLine("done.");
		}

		public static BookPageInfo ReadPageNode(XmlElement parent)
		{
			var lncnt = Utility.ToInt32(parent.GetAttribute("Lines"));
			var lines = new string[lncnt];

			for (var i = 0; i < lncnt; i++)
			{
				lines[i] = parent.GetAttribute("Line" + i.ToString());
			}

			return new BookPageInfo(lines);
		}
	}

	public delegate void YesNoCallback(Mobile from, bool yesNo);

	public delegate void YesNoCallbackState(Mobile from, bool yesNo, params object[] state);

	public class YesNo
	{
		public static void SimpleConfirm(YesNoCallback callback, Mobile callingPlayer, bool dragable)
		{
			SimpleConfirmMsg(callback, callingPlayer, "Are you sure?", dragable);
		}

		public static void SimpleConfirm(YesNoCallbackState callback, Mobile callingPlayer, bool dragable, params object[] state)
		{
			SimpleConfirmMsg(callback, callingPlayer, "Are you sure?", dragable, state);
		}

		public static void SimpleConfirmMsg(YesNoCallback callback, Mobile callingPlayer, string msg, bool dragable)
		{
			_ = callingPlayer.SendGump(new SimpleConfirmGump(callback, callingPlayer, msg, dragable));
		}

		public static void SimpleConfirmMsg(YesNoCallbackState callback, Mobile callingPlayer, string msg, bool dragable, params object[] state)
		{
			_ = callingPlayer.SendGump(new SimpleConfirmStateGump(callback, callingPlayer, msg, dragable, state));
		}

		public static void AskYesNo(YesNoCallback callback, Mobile callingPlayer, string theQuestion, bool dragable)
		{
			AskYesNo(callback, callingPlayer, "Query:", theQuestion, "Yes", "No", dragable);
		}

		public static void AskYesNo(YesNoCallback callback, Mobile callingPlayer, string queryTitle, string theQuestion, bool dragable)
		{
			AskYesNo(callback, callingPlayer, queryTitle, theQuestion, "Yes", "No", dragable);
		}

		public static void AskYesNo(YesNoCallback callback, Mobile callingPlayer, string queryTitle, string theQuestion, string yesString, string noString, bool dragable)
		{
			_ = callingPlayer.SendGump(new YesNoGump(callback, callingPlayer, queryTitle, theQuestion, yesString, noString, dragable));
		}

		private class YesNoGump : Gump
		{
			private readonly YesNoCallback _CallBack;
			private readonly Mobile _CallingPlayer;

			public YesNoGump(YesNoCallback callback, Mobile callingPlayer, string queryTitle, string theQuestion, string yesString, string noString, bool dragable)
				: base(100, 100)
			{
				_CallBack = callback;
				_CallingPlayer = callingPlayer;

				Disposable = false;
				Dragable = dragable;

				AddPage(0);
				AddBackground(0, 0, 400, 360, 0x13BE);
				AddHtml(10, 10, 380, 20, "<basefont color=#FFFFFF size=5><center>" + queryTitle + "</center></basefont>", false, false);
				AddHtml(10, 60, 380, 170, theQuestion, true, true);

				AddButton(20, 245, 0x7538, 0x7539, 0xffff, GumpButtonType.Reply, 0);
				AddLabel(40, 240, 1150, yesString);
				AddButton(20, 265, 0x7538, 0x7539, 0, GumpButtonType.Reply, 0);
				AddLabel(40, 260, 1150, noString);
			}

			//Handles button presses
			public override void OnResponse(NetState state, RelayInfo info)
			{
				var theAnswer = info.ButtonID != 0;

				_CallBack(_CallingPlayer, theAnswer);
			}
		}

		private class SimpleConfirmGump : Gump
		{
			private readonly YesNoCallback _CallBack;
			private readonly Mobile _CallingPlayer;

			public SimpleConfirmGump(YesNoCallback callback, Mobile callingPlayer, string msg, bool dragable)
				: base(300, 300)
			{
				_CallBack = callback;
				_CallingPlayer = callingPlayer;

				Disposable = false;
				Dragable = dragable;

				AddPage(0);

				AddImage(0, 0, 0x816);
				AddButton(20, 74, 0x81A, 0x81B, 1, GumpButtonType.Reply, 0); // OK
				AddButton(88, 74, 0x995, 0x996, 0, GumpButtonType.Reply, 0); // Cancel

				//string msg = "Are you sure?";
				AddLabel(42, 25, 63, msg);
			}

			//Handles button presses
			public override void OnResponse(NetState state, RelayInfo info)
			{
				var theAnswer = info.ButtonID == 1;

				_CallBack(_CallingPlayer, theAnswer);

			}
		}

		private class SimpleConfirmStateGump : Gump
		{
			private readonly YesNoCallbackState _CallBack;
			private readonly Mobile _CallingPlayer;
			private readonly object[] _State;

			public SimpleConfirmStateGump(YesNoCallbackState callback, Mobile callingPlayer, string msg, bool dragable, object[] state)
				: base(300, 300)
			{
				_CallBack = callback;
				_CallingPlayer = callingPlayer;
				_State = state;

				Disposable = false;
				Dragable = dragable;

				AddPage(0);

				AddImage(0, 0, 0x816);
				AddButton(20, 74, 0x81A, 0x81B, 1, GumpButtonType.Reply, 0); // OK
				AddButton(88, 74, 0x995, 0x996, 0, GumpButtonType.Reply, 0); // Cancel

				//string msg = "Are you sure?";
				AddLabel(42, 25, 63, msg);
			}

			//Handles button presses
			public override void OnResponse(NetState state, RelayInfo info)
			{
				var theAnswer = info.ButtonID == 1;

				_CallBack(_CallingPlayer, theAnswer, _State);
			}
		}
	}

	#region BaseBook Constructor

	public class BaseBook : Item, ISecurable
	{
		private string m_Title;
		private string m_Author;
		private SecureLevel m_SecureLevel;

		[CommandProperty(AccessLevel.GameMaster)]
		public string Title
		{
			get => m_Title;
			set
			{
				m_Title = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Author
		{
			get => m_Author;
			set
			{
				m_Author = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Writable { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int PagesCount => Pages.Length;

		public BookPageInfo[] Pages { get; private set; }

		[Constructable]
		public BaseBook(int itemID) : this(itemID, 20, true)
		{
		}

		[Constructable]
		public BaseBook(int itemID, int pageCount, bool writable) : this(itemID, null, null, pageCount, writable)
		{
		}

		[Constructable]
		public BaseBook(int itemID, string title, string author, int pageCount, bool writable) : base(itemID)
		{
			m_Title = title;
			m_Author = author;
			Writable = writable;

			var content = DefaultContent;

			if (content == null)
			{
				Pages = new BookPageInfo[pageCount];

				for (var i = 0; i < Pages.Length; ++i)
				{
					Pages[i] = new BookPageInfo();
				}
			}
			else
			{
				Pages = content.Copy();
			}
		}

		// Intended for defined books only
		public BaseBook(int itemID, bool writable) : base(itemID)
		{
			Writable = writable;

			var content = DefaultContent;

			if (content == null)
			{
				Pages = new BookPageInfo[0];
			}
			else
			{
				m_Title = content.Title;
				m_Author = content.Author;
				Pages = content.Copy();
			}
		}

		public virtual BookContent DefaultContent => null;

		public BaseBook(Serial serial) : base(serial)
		{
		}

		[Flags]
		private enum SaveFlags
		{
			None = 0x00,
			Title = 0x01,
			Author = 0x02,
			Writable = 0x04,
			Content = 0x08
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			SetSecureLevelEntry.AddTo(from, this, list);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			var content = DefaultContent;

			var flags = SaveFlags.None;

			if (m_Title != (content?.Title))
			{
				flags |= SaveFlags.Title;
			}

			if (m_Author != (content?.Author))
			{
				flags |= SaveFlags.Author;
			}

			if (Writable)
			{
				flags |= SaveFlags.Writable;
			}

			if (content == null || !content.IsMatch(Pages))
			{
				flags |= SaveFlags.Content;
			}

			writer.Write(4); // version

			writer.Write((int)m_SecureLevel);

			writer.Write((byte)flags);

			if ((flags & SaveFlags.Title) != 0)
			{
				writer.Write(m_Title);
			}

			if ((flags & SaveFlags.Author) != 0)
			{
				writer.Write(m_Author);
			}

			if ((flags & SaveFlags.Content) != 0)
			{
				writer.WriteEncodedInt(Pages.Length);

				for (var i = 0; i < Pages.Length; ++i)
				{
					Pages[i].Serialize(writer);
				}
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 4:
					{
						m_SecureLevel = (SecureLevel)reader.ReadInt();
						goto case 3;
					}
				case 3:
				case 2:
					{
						var content = DefaultContent;

						var flags = (SaveFlags)reader.ReadByte();

						if ((flags & SaveFlags.Title) != 0)
						{
							m_Title = Utility.Intern(reader.ReadString());
						}
						else if (content != null)
						{
							m_Title = content.Title;
						}

						if ((flags & SaveFlags.Author) != 0)
						{
							m_Author = reader.ReadString();
						}
						else if (content != null)
						{
							m_Author = content.Author;
						}

						Writable = (flags & SaveFlags.Writable) != 0;

						if ((flags & SaveFlags.Content) != 0)
						{
							Pages = new BookPageInfo[reader.ReadEncodedInt()];

							for (var i = 0; i < Pages.Length; ++i)
							{
								Pages[i] = new BookPageInfo(reader);
							}
						}
						else
						{
							if (content != null)
							{
								Pages = content.Copy();
							}
							else
							{
								Pages = new BookPageInfo[0];
							}
						}

						break;
					}
				case 1:
				case 0:
					{
						m_Title = reader.ReadString();
						m_Author = reader.ReadString();
						Writable = reader.ReadBool();

						if (version == 0 || reader.ReadBool())
						{
							Pages = new BookPageInfo[reader.ReadInt()];

							for (var i = 0; i < Pages.Length; ++i)
							{
								Pages[i] = new BookPageInfo(reader);
							}
						}
						else
						{
							var content = DefaultContent;

							if (content != null)
							{
								Pages = content.Copy();
							}
							else
							{
								Pages = new BookPageInfo[0];
							}
						}

						break;
					}
			}

			if (version < 3 && (Weight == 1 || Weight == 2))
			{
				Weight = -1;
			}
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			if (m_Title != null && m_Title.Length > 0)
			{
				list.Add(m_Title);
			}
			else
			{
				base.AddNameProperty(list);
			}
		}

		/*public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( m_Title != null && m_Title.Length > 0 )
				list.Add( 1060658, "Title\t{0}", m_Title ); // ~1_val~: ~2_val~

			if ( m_Author != null && m_Author.Length > 0 )
				list.Add( 1060659, "Author\t{0}", m_Author ); // ~1_val~: ~2_val~

			if ( m_Pages != null && m_Pages.Length > 0 )
				list.Add( 1060660, "Pages\t{0}", m_Pages.Length ); // ~1_val~: ~2_val~
		}*/

		public override void OnSingleClick(Mobile from)
		{
			LabelTo(from, "{0} by {1}", m_Title, m_Author);
			LabelTo(from, "[{0} pages]", Pages.Length);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (m_Title == null && m_Author == null && Writable == true)
			{
				Title = "a book";
				Author = from.Name;
			}

			_ = from.Send(new BookHeader(from, this));
			_ = from.Send(new BookPageDetails(this));
		}

		public string ContentAsString
		{
			get
			{
				var sb = new StringBuilder();

				foreach (var bpi in Pages)
				{
					foreach (var line in bpi.Lines)
					{
						_ = sb.AppendLine(line);
					}
				}

				return sb.ToString();
			}
		}

		public string[] ContentAsStringArray
		{
			get
			{
				var lines = new List<string>();

				foreach (var bpi in Pages)
				{
					lines.AddRange(bpi.Lines);
				}

				return lines.ToArray();
			}
		}

		public static void Initialize()
		{
			PacketHandlers.Register(0xD4, 0, true, new OnPacketReceive(HeaderChange));
			PacketHandlers.Register(0x66, 0, true, new OnPacketReceive(ContentChange));
			PacketHandlers.Register(0x93, 99, true, new OnPacketReceive(OldHeaderChange));
		}

		public static void OldHeaderChange(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			if (pvSrc.ReadItem() is not BaseBook book || !book.Writable || !from.InRange(book.GetWorldLocation(), 1) || !book.IsAccessibleTo(from))
			{
				return;
			}

			_ = pvSrc.Seek(4, SeekOrigin.Current); // Skip flags and page count

			var title = pvSrc.ReadStringSafe(60);
			var author = pvSrc.ReadStringSafe(30);

			book.Title = Utility.FixHtml(title);
			book.Author = Utility.FixHtml(author);
		}

		public static void HeaderChange(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			if (pvSrc.ReadItem() is not BaseBook book || !book.Writable || !from.InRange(book.GetWorldLocation(), 1) || !book.IsAccessibleTo(from))
			{
				return;
			}

			_ = pvSrc.Seek(4, SeekOrigin.Current); // Skip flags and page count

			int titleLength = pvSrc.ReadUInt16();

			if (titleLength > 60)
			{
				return;
			}

			var title = pvSrc.ReadUTF8StringSafe(titleLength);

			int authorLength = pvSrc.ReadUInt16();

			if (authorLength > 30)
			{
				return;
			}

			var author = pvSrc.ReadUTF8StringSafe(authorLength);

			book.Title = Utility.FixHtml(title);
			book.Author = Utility.FixHtml(author);
		}

		public static void ContentChange(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			if (pvSrc.ReadItem() is not BaseBook book || !book.Writable || !from.InRange(book.GetWorldLocation(), 1) || !book.IsAccessibleTo(from))
			{
				return;
			}

			int pageCount = pvSrc.ReadUInt16();

			if (pageCount > book.PagesCount)
			{
				return;
			}

			for (var i = 0; i < pageCount; ++i)
			{
				int index = pvSrc.ReadUInt16();

				if (index >= 1 && index <= book.PagesCount)
				{
					--index;

					int lineCount = pvSrc.ReadUInt16();

					if (lineCount <= 8)
					{
						var lines = new string[lineCount];

						for (var j = 0; j < lineCount; ++j)
						{
							if ((lines[j] = pvSrc.ReadUTF8StringSafe()).Length >= 80)
							{
								return;
							}
						}

						book.Pages[index].Lines = lines;
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
		}

		#region ISecurable Members

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level
		{
			get => m_SecureLevel;
			set => m_SecureLevel = value;
		}

		#endregion
	}

	public sealed class BookHeader : Packet
	{
		public BookHeader(Mobile from, BaseBook book) : base(0xD4)
		{
			var title = book.Title ?? "";
			var author = book.Author ?? "";

			var titleBuffer = Utility.UTF8.GetBytes(title);
			var authorBuffer = Utility.UTF8.GetBytes(author);

			EnsureCapacity(15 + titleBuffer.Length + authorBuffer.Length);

			m_Stream.Write(book.Serial);
			m_Stream.Write(true);
			m_Stream.Write(book.Writable && from.InRange(book.GetWorldLocation(), 1));
			m_Stream.Write((ushort)book.PagesCount);

			m_Stream.Write((ushort)(titleBuffer.Length + 1));
			m_Stream.Write(titleBuffer, 0, titleBuffer.Length);
			m_Stream.Write((byte)0); // terminate

			m_Stream.Write((ushort)(authorBuffer.Length + 1));
			m_Stream.Write(authorBuffer, 0, authorBuffer.Length);
			m_Stream.Write((byte)0); // terminate
		}
	}

	public class BookPageInfo
	{
		public string[] Lines { get; set; }

		public BookPageInfo()
		{
			Lines = new string[0];
		}

		public BookPageInfo(params string[] lines)
		{
			Lines = lines;
		}

		public BookPageInfo(GenericReader reader)
		{
			var length = reader.ReadInt();

			Lines = new string[length];

			for (var i = 0; i < Lines.Length; ++i)
			{
				Lines[i] = Utility.Intern(reader.ReadString());
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(Lines.Length);

			for (var i = 0; i < Lines.Length; ++i)
			{
				writer.Write(Lines[i]);
			}
		}
	}

	public sealed class BookPageDetails : Packet
	{
		public BookPageDetails(BaseBook book) : base(0x66)
		{
			EnsureCapacity(256);

			m_Stream.Write(book.Serial);
			m_Stream.Write((ushort)book.PagesCount);

			for (var i = 0; i < book.PagesCount; ++i)
			{
				var page = book.Pages[i];

				m_Stream.Write((ushort)(i + 1));
				m_Stream.Write((ushort)page.Lines.Length);

				for (var j = 0; j < page.Lines.Length; ++j)
				{
					var buffer = Utility.UTF8.GetBytes(page.Lines[j]);

					m_Stream.Write(buffer, 0, buffer.Length);
					m_Stream.Write((byte)0);
				}
			}
		}
	}

	public class BookContent
	{
		private readonly string m_Author;

		public string Title { get; }
		public string Author => m_Author;

		public BookPageInfo[] Pages { get; }

		public BookContent(string title, string author, params BookPageInfo[] pages)
		{
			Title = title;
			m_Author = author;
			Pages = pages;
		}

		public BookPageInfo[] Copy()
		{
			var copy = new BookPageInfo[Pages.Length];

			for (var i = 0; i < copy.Length; ++i)
			{
				copy[i] = new BookPageInfo(Pages[i].Lines);
			}

			return copy;
		}

		public bool IsMatch(BookPageInfo[] cmp)
		{
			if (cmp.Length != Pages.Length)
			{
				return false;
			}

			for (var i = 0; i < cmp.Length; ++i)
			{
				var a = Pages[i].Lines;
				var b = cmp[i].Lines;

				if (a.Length != b.Length)
				{
					return false;
				}
				else if (a != b)
				{
					for (var j = 0; j < a.Length; ++j)
					{
						if (a[j] != b[j])
						{
							return false;
						}
					}
				}
			}

			return true;
		}
	}

	#endregion
}