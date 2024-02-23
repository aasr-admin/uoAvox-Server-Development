using Server.Engines.Publishing;

using System.Collections.Generic;

namespace Server.Mobiles
{
	public class Publisher : Scribe
	{
		private static List<BookContent> m_Books;

		public static List<BookContent> Books
		{
			get
			{
				if (m_Books == null)
				{
					m_Books = new List<BookContent>();
				}

				return m_Books;
			}
			set => m_Books = value;
		}

		[Constructable]
		public Publisher()
		{
			Title = "the publisher";
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is BaseBook book)
			{
				YesNo.SimpleConfirmMsg(new YesNoCallbackState(PublishConfirm), from, "Publish This Book?", true, book);
			}

			return base.OnDragDrop(from, dropped);
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.SendGump(new BookPubGump(from));
		}

		public void PublishConfirm(Mobile from, bool yesNo, object[] arg)
		{
			if (!yesNo)
			{
				return;
			}

			var book = arg[0] as BaseBook;
			if (book == null)
			{
				return;
			}

			var bc = new BookContent(book.Title, book.Author, book.Pages);

			if (m_Books == null)
			{
				m_Books = new List<BookContent>();
			}

			if (!m_Books.Contains(bc))
			{
				m_Books.Add(bc);
			}
		}

		public Publisher(Serial serial) : base(serial)
		{ }

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
}