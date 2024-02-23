using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Prompts;


using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.BulkOrders
{
	public enum BODType
	{
		Smith,
		Tailor
	}

	public class BulkOrderBook : Item, ISecurable
	{
		private ArrayList m_Entries;
		private BOBFilter m_Filter;
		private string m_BookName;
		private SecureLevel m_Level;
		private int m_ItemCount;

		[CommandProperty(AccessLevel.GameMaster)]
		public string BookName
		{
			get => m_BookName;
			set { m_BookName = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level
		{
			get => m_Level;
			set => m_Level = value;
		}

		public ArrayList Entries => m_Entries;

		public BOBFilter Filter => m_Filter;

		public int ItemCount
		{
			get => m_ItemCount;
			set => m_ItemCount = value;
		}

		[Constructable]
		public BulkOrderBook() : base(0x2259)
		{
			Weight = 1.0;
			LootType = LootType.Blessed;

			m_Entries = new ArrayList();
			m_Filter = new BOBFilter();

			m_Level = SecureLevel.CoOwners;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!from.InRange(GetWorldLocation(), 2))
			{
				from.LocalOverheadMessage(Network.MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
			else if (m_Entries.Count == 0)
			{
				from.SendLocalizedMessage(1062381); // The book is empty.
			}
			else if (from is PlayerMobile)
			{
				from.SendGump(new BOBGump((PlayerMobile)from, this));
			}
		}

		public override void OnDoubleClickSecureTrade(Mobile from)
		{
			if (!from.InRange(GetWorldLocation(), 2))
			{
				from.SendLocalizedMessage(500446); // That is too far away.
			}
			else if (m_Entries.Count == 0)
			{
				from.SendLocalizedMessage(1062381); // The book is empty.
			}
			else
			{
				from.SendGump(new BOBGump((PlayerMobile)from, this));

				var cont = GetSecureTradeCont();

				if (cont != null)
				{
					var trade = cont.Trade;

					if (trade != null && trade.From.Mobile == from)
					{
						trade.To.Mobile.SendGump(new BOBGump((PlayerMobile)(trade.To.Mobile), this));
					}
					else if (trade != null && trade.To.Mobile == from)
					{
						trade.From.Mobile.SendGump(new BOBGump((PlayerMobile)(trade.From.Mobile), this));
					}
				}
			}
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is LargeBOD || dropped is SmallBOD)
			{
				if (!IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1062385); // You must have the book in your backpack to add deeds to it.
					return false;
				}
				else if (!from.Backpack.CheckHold(from, dropped, true, true))
				{
					return false;
				}
				else if (m_Entries.Count < 500)
				{
					if (dropped is LargeBOD)
					{
						m_Entries.Add(new BOBLargeEntry((LargeBOD)dropped));
					}
					else if (dropped is SmallBOD) // Sanity
					{
						m_Entries.Add(new BOBSmallEntry((SmallBOD)dropped));
					}

					InvalidateProperties();

					if (m_Entries.Count / 5 > m_ItemCount)
					{
						m_ItemCount++;
						InvalidateItems();
					}

					from.SendSound(0x42, GetWorldLocation());
					from.SendLocalizedMessage(1062386); // Deed added to book.

					if (from is PlayerMobile)
					{
						from.SendGump(new BOBGump((PlayerMobile)from, this));
					}

					dropped.Delete();

					return true;
				}
				else
				{
					from.SendLocalizedMessage(1062387); // The book is full of deeds.
					return false;
				}
			}

			from.SendLocalizedMessage(1062388); // That is not a bulk order deed.
			return false;
		}

		public override int GetTotal(TotalType type)
		{
			var total = base.GetTotal(type);

			if (type == TotalType.Items)
			{
				total = m_ItemCount;
			}

			return total;
		}

		public void InvalidateItems()
		{
			if (RootParent is Mobile)
			{
				var m = (Mobile)RootParent;

				m.UpdateTotals();
				InvalidateContainers(Parent);
			}
		}

		public void InvalidateContainers(object parent)
		{
			if (parent != null && parent is Container)
			{
				var c = (Container)parent;

				c.InvalidateProperties();
				InvalidateContainers(c.Parent);
			}
		}

		public BulkOrderBook(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(2); // version

			writer.Write(m_ItemCount);

			writer.Write((int)m_Level);

			writer.Write(m_BookName);

			m_Filter.Serialize(writer);

			writer.WriteEncodedInt(m_Entries.Count);

			for (var i = 0; i < m_Entries.Count; ++i)
			{
				var obj = m_Entries[i];

				if (obj is BOBLargeEntry)
				{
					writer.WriteEncodedInt(0);
					((BOBLargeEntry)obj).Serialize(writer);
				}
				else if (obj is BOBSmallEntry)
				{
					writer.WriteEncodedInt(1);
					((BOBSmallEntry)obj).Serialize(writer);
				}
				else
				{
					writer.WriteEncodedInt(-1);
				}
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 2:
					{
						m_ItemCount = reader.ReadInt();
						goto case 1;
					}
				case 1:
					{
						m_Level = (SecureLevel)reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						m_BookName = reader.ReadString();

						m_Filter = new BOBFilter(reader);

						var count = reader.ReadEncodedInt();

						m_Entries = new ArrayList(count);

						for (var i = 0; i < count; ++i)
						{
							var v = reader.ReadEncodedInt();

							switch (v)
							{
								case 0: m_Entries.Add(new BOBLargeEntry(reader)); break;
								case 1: m_Entries.Add(new BOBSmallEntry(reader)); break;
							}
						}

						break;
					}
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add(1062344, m_Entries.Count.ToString()); // Deeds in book: ~1_val~

			if (m_BookName != null && m_BookName.Length > 0)
			{
				list.Add(1062481, m_BookName); // Book Name: ~1_val~
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			LabelTo(from, 1062344, m_Entries.Count.ToString()); // Deeds in book: ~1_val~

			if (!String.IsNullOrEmpty(m_BookName))
			{
				LabelTo(from, 1062481, m_BookName);
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from.CheckAlive() && IsChildOf(from.Backpack))
			{
				list.Add(new NameBookEntry(from, this));
			}

			SetSecureLevelEntry.AddTo(from, this, list);
		}

		private class NameBookEntry : ContextMenuEntry
		{
			private readonly Mobile m_From;
			private readonly BulkOrderBook m_Book;

			public NameBookEntry(Mobile from, BulkOrderBook book) : base(6216)
			{
				m_From = from;
				m_Book = book;
			}

			public override void OnClick()
			{
				if (m_From.CheckAlive() && m_Book.IsChildOf(m_From.Backpack))
				{
					m_From.Prompt = new NameBookPrompt(m_Book);
					m_From.SendLocalizedMessage(1062479); // Type in the new name of the book:
				}
			}
		}

		private class NameBookPrompt : Prompt
		{
			private readonly BulkOrderBook m_Book;

			public NameBookPrompt(BulkOrderBook book)
			{
				m_Book = book;
			}

			public override void OnResponse(Mobile from, string text)
			{
				if (text.Length > 40)
				{
					text = text.Substring(0, 40);
				}

				if (from.CheckAlive() && m_Book.IsChildOf(from.Backpack))
				{
					m_Book.BookName = Utility.FixHtml(text.Trim());

					from.SendLocalizedMessage(1062480); // The bulk order book's name has been changed.
				}
			}

			public override void OnCancel(Mobile from)
			{
			}
		}
	}

	public class BOBGump : Gump
	{
		private readonly PlayerMobile m_From;
		private readonly BulkOrderBook m_Book;
		private readonly ArrayList m_List;

		private int m_Page;

		private const int LabelColor = 0x7FFF;

		public Item Reconstruct(object obj)
		{
			Item item = null;

			if (obj is BOBLargeEntry)
			{
				item = ((BOBLargeEntry)obj).Reconstruct();
			}
			else if (obj is BOBSmallEntry)
			{
				item = ((BOBSmallEntry)obj).Reconstruct();
			}

			return item;
		}

		public bool CheckFilter(object obj)
		{
			if (obj is BOBLargeEntry)
			{
				var e = (BOBLargeEntry)obj;

				return CheckFilter(e.Material, e.AmountMax, true, e.RequireExceptional, e.DeedType, (e.Entries.Length > 0 ? e.Entries[0].ItemType : null));
			}
			else if (obj is BOBSmallEntry)
			{
				var e = (BOBSmallEntry)obj;

				return CheckFilter(e.Material, e.AmountMax, false, e.RequireExceptional, e.DeedType, e.ItemType);
			}

			return false;
		}

		public bool CheckFilter(BulkMaterialType mat, int amountMax, bool isLarge, bool reqExc, BODType deedType, Type itemType)
		{
			var f = (m_From.UseOwnFilter ? m_From.BOBFilter : m_Book.Filter);

			if (f.IsDefault)
			{
				return true;
			}

			if (f.Quality == 1 && reqExc)
			{
				return false;
			}
			else if (f.Quality == 2 && !reqExc)
			{
				return false;
			}

			if (f.Quantity == 1 && amountMax != 10)
			{
				return false;
			}
			else if (f.Quantity == 2 && amountMax != 15)
			{
				return false;
			}
			else if (f.Quantity == 3 && amountMax != 20)
			{
				return false;
			}

			if (f.Type == 1 && isLarge)
			{
				return false;
			}
			else if (f.Type == 2 && !isLarge)
			{
				return false;
			}

			switch (f.Material)
			{
				default:
				case 0: return true;
				case 1: return (deedType == BODType.Smith);
				case 2: return (deedType == BODType.Tailor);

				case 3: return (mat == BulkMaterialType.None && BGTClassifier.Classify(deedType, itemType) == BulkGenericType.Iron);
				case 4: return (mat == BulkMaterialType.DullCopper);
				case 5: return (mat == BulkMaterialType.ShadowIron);
				case 6: return (mat == BulkMaterialType.Copper);
				case 7: return (mat == BulkMaterialType.Bronze);
				case 8: return (mat == BulkMaterialType.Gold);
				case 9: return (mat == BulkMaterialType.Agapite);
				case 10: return (mat == BulkMaterialType.Verite);
				case 11: return (mat == BulkMaterialType.Valorite);

				case 12: return (mat == BulkMaterialType.None && BGTClassifier.Classify(deedType, itemType) == BulkGenericType.Cloth);
				case 13: return (mat == BulkMaterialType.None && BGTClassifier.Classify(deedType, itemType) == BulkGenericType.Leather);
				case 14: return (mat == BulkMaterialType.Spined);
				case 15: return (mat == BulkMaterialType.Horned);
				case 16: return (mat == BulkMaterialType.Barbed);
			}
		}

		public int GetIndexForPage(int page)
		{
			var index = 0;

			while (page-- > 0)
			{
				index += GetCountForIndex(index);
			}

			return index;
		}

		public int GetCountForIndex(int index)
		{
			var slots = 0;
			var count = 0;

			var list = m_List;

			for (var i = index; i >= 0 && i < list.Count; ++i)
			{
				var obj = list[i];

				if (CheckFilter(obj))
				{
					int add;

					if (obj is BOBLargeEntry)
					{
						add = ((BOBLargeEntry)obj).Entries.Length;
					}
					else
					{
						add = 1;
					}

					if ((slots + add) > 10)
					{
						break;
					}

					slots += add;
				}

				++count;
			}

			return count;
		}

		public int GetPageForIndex(int index, int sizeDropped)
		{
			if (index <= 0)
			{
				return 0;
			}

			var count = 0;
			var add = 0;
			var page = 0;
			var list = m_List;
			int i;
			object obj;

			for (i = 0; (i < index) && (i < list.Count); i++)
			{
				obj = list[i];
				if (CheckFilter(obj))
				{
					if (obj is BOBLargeEntry)
					{
						add = ((BOBLargeEntry)obj).Entries.Length;
					}
					else
					{
						add = 1;
					}

					count += add;
					if (count > 10)
					{
						page++;
						count = add;
					}
				}
			}
			/* now we are on the page of the bod preceeding the dropped one.
			 * next step: checking whether we have to remain where we are.
			 * The counter i needs to be incremented as the bod to this very moment
			 * has not yet been removed from m_List */
			i++;

			/* if, for instance, a big bod of size 6 has been removed, smaller bods
			 * might fall back into this page. Depending on their sizes, the page eeds
			 * to be adjusted accordingly. This is done now.
			 */
			if (count + sizeDropped > 10)
			{
				while ((i < list.Count) && (count <= 10))
				{
					obj = list[i];
					if (CheckFilter(obj))
					{
						if (obj is BOBLargeEntry)
						{
							count += ((BOBLargeEntry)obj).Entries.Length;
						}
						else
						{
							count += 1;
						}
					}
					i++;
				}
				if (count > 10)
				{
					page++;
				}
			}
			return page;
		}


		public object GetMaterialName(BulkMaterialType mat, BODType type, Type itemType)
		{
			switch (type)
			{
				case BODType.Smith:
					{
						switch (mat)
						{
							case BulkMaterialType.None: return 1062226;
							case BulkMaterialType.DullCopper: return 1018332;
							case BulkMaterialType.ShadowIron: return 1018333;
							case BulkMaterialType.Copper: return 1018334;
							case BulkMaterialType.Bronze: return 1018335;
							case BulkMaterialType.Gold: return 1018336;
							case BulkMaterialType.Agapite: return 1018337;
							case BulkMaterialType.Verite: return 1018338;
							case BulkMaterialType.Valorite: return 1018339;
						}

						break;
					}
				case BODType.Tailor:
					{
						switch (mat)
						{
							case BulkMaterialType.None:
								{
									if (itemType.IsSubclassOf(typeof(BaseArmor)) || itemType.IsSubclassOf(typeof(BaseShoes)))
									{
										return 1062235;
									}

									return 1044286;
								}
							case BulkMaterialType.Spined: return 1062236;
							case BulkMaterialType.Horned: return 1062237;
							case BulkMaterialType.Barbed: return 1062238;
						}

						break;
					}
			}

			return "Invalid";
		}

		public BOBGump(PlayerMobile from, BulkOrderBook book) : this(from, book, 0, null)
		{
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			var index = info.ButtonID;

			switch (index)
			{
				case 0: // EXIT
					{
						break;
					}
				case 1: // Set Filter
					{
						m_From.SendGump(new BOBFilterGump(m_From, m_Book));

						break;
					}
				case 2: // Previous page
					{
						if (m_Page > 0)
						{
							m_From.SendGump(new BOBGump(m_From, m_Book, m_Page - 1, m_List));
						}

						return;
					}
				case 3: // Next page
					{
						if (GetIndexForPage(m_Page + 1) < m_List.Count)
						{
							m_From.SendGump(new BOBGump(m_From, m_Book, m_Page + 1, m_List));
						}

						break;
					}
				case 4: // Price all
					{
						if (m_Book.IsChildOf(m_From.Backpack))
						{
							m_From.Prompt = new SetPricePrompt(m_Book, null, m_Page, m_List);
							m_From.SendMessage("Type in a price for all deeds in the book:");
						}

						break;
					}
				default:
					{
						var canDrop = m_Book.IsChildOf(m_From.Backpack);
						var canPrice = canDrop || (m_Book.RootParent is PlayerVendor);

						index -= 5;

						var type = index % 2;
						index /= 2;

						if (index < 0 || index >= m_List.Count)
						{
							break;
						}

						var obj = m_List[index];

						if (!m_Book.Entries.Contains(obj))
						{
							m_From.SendLocalizedMessage(1062382); // The deed selected is not available.
							break;
						}

						if (type == 0) // Drop
						{
							if (m_Book.IsChildOf(m_From.Backpack))
							{
								var item = Reconstruct(obj);

								if (item != null)
								{
									var pack = m_From.Backpack;
									if ((pack == null) || ((pack != null) && (!pack.CheckHold(m_From, item, true, true, 0, item.PileWeight + item.TotalWeight))))
									{
										m_From.SendLocalizedMessage(503204); // You do not have room in your backpack for this
										m_From.SendGump(new BOBGump(m_From, m_Book, m_Page, null));
									}
									else
									{
										if (m_Book.IsChildOf(m_From.Backpack))
										{
											int sizeOfDroppedBod;
											if (obj is BOBLargeEntry)
											{
												sizeOfDroppedBod = ((BOBLargeEntry)obj).Entries.Length;
											}
											else
											{
												sizeOfDroppedBod = 1;
											}

											m_From.AddToBackpack(item);
											m_From.SendLocalizedMessage(1045152); // The bulk order deed has been placed in your backpack.
											m_Book.Entries.Remove(obj);
											m_Book.InvalidateProperties();

											if (m_Book.Entries.Count / 5 < m_Book.ItemCount)
											{
												m_Book.ItemCount--;
												m_Book.InvalidateItems();
											}

											if (m_Book.Entries.Count > 0)
											{
												m_Page = GetPageForIndex(index, sizeOfDroppedBod);
												m_From.SendGump(new BOBGump(m_From, m_Book, m_Page, null));
											}
											else
											{
												m_From.SendLocalizedMessage(1062381); // The book is empty.
											}
										}
									}
								}
								else
								{
									m_From.SendMessage("Internal error. The bulk order deed could not be reconstructed.");
								}
							}
						}
						else // Set Price | Buy
						{
							if (m_Book.IsChildOf(m_From.Backpack))
							{
								m_From.Prompt = new SetPricePrompt(m_Book, obj, m_Page, m_List);
								m_From.SendLocalizedMessage(1062383); // Type in a price for the deed:
							}
							else if (m_Book.RootParent is PlayerVendor)
							{
								var pv = (PlayerVendor)m_Book.RootParent;
								var vi = pv.GetVendorItem(m_Book);

								if (vi != null && !vi.IsForSale)
								{
									int sizeOfDroppedBod;
									var price = 0;
									if (obj is BOBLargeEntry)
									{
										price = ((BOBLargeEntry)obj).Price;
										sizeOfDroppedBod = ((BOBLargeEntry)obj).Entries.Length;
									}
									else
									{
										price = ((BOBSmallEntry)obj).Price;
										sizeOfDroppedBod = 1;
									}
									if (price == 0)
									{
										m_From.SendLocalizedMessage(1062382); // The deed selected is not available.
									}
									else
									{
										if (m_Book.Entries.Count > 0)
										{
											m_Page = GetPageForIndex(index, sizeOfDroppedBod);
											m_From.SendGump(new BODBuyGump(m_From, m_Book, obj, m_Page, price));
										}
										else
										{
											m_From.SendLocalizedMessage(1062381); // The book is emptz
										}
									}
								}
							}
						}
						break;
					}
			}
		}

		private class SetPricePrompt : Prompt
		{
			private readonly BulkOrderBook m_Book;
			private readonly object m_Object;
			private readonly int m_Page;
			private readonly ArrayList m_List;

			public SetPricePrompt(BulkOrderBook book, object obj, int page, ArrayList list)
			{
				m_Book = book;
				m_Object = obj;
				m_Page = page;
				m_List = list;
			}

			public override void OnResponse(Mobile from, string text)
			{
				if (m_Object != null && !m_Book.Entries.Contains(m_Object))
				{
					from.SendLocalizedMessage(1062382); // The deed selected is not available.
					return;
				}

				var price = Utility.ToInt32(text);

				if (price < 0 || price > 250000000)
				{
					from.SendLocalizedMessage(1062390); // The price you requested is outrageous!
				}
				else if (m_Object == null)
				{
					for (var i = 0; i < m_List.Count; ++i)
					{
						var obj = m_List[i];

						if (!m_Book.Entries.Contains(obj))
						{
							continue;
						}

						if (obj is BOBLargeEntry)
						{
							((BOBLargeEntry)obj).Price = price;
						}
						else if (obj is BOBSmallEntry)
						{
							((BOBSmallEntry)obj).Price = price;
						}
					}

					from.SendMessage("Deed prices set.");

					if (from is PlayerMobile)
					{
						from.SendGump(new BOBGump((PlayerMobile)from, m_Book, m_Page, m_List));
					}
				}
				else if (m_Object is BOBLargeEntry)
				{
					((BOBLargeEntry)m_Object).Price = price;

					from.SendLocalizedMessage(1062384); // Deed price set.

					if (from is PlayerMobile)
					{
						from.SendGump(new BOBGump((PlayerMobile)from, m_Book, m_Page, m_List));
					}
				}
				else if (m_Object is BOBSmallEntry)
				{
					((BOBSmallEntry)m_Object).Price = price;

					from.SendLocalizedMessage(1062384); // Deed price set.

					if (from is PlayerMobile)
					{
						from.SendGump(new BOBGump((PlayerMobile)from, m_Book, m_Page, m_List));
					}
				}
			}
		}

		public BOBGump(PlayerMobile from, BulkOrderBook book, int page, ArrayList list) : base(12, 24)
		{
			from.CloseGump(typeof(BOBGump));
			from.CloseGump(typeof(BOBFilterGump));

			m_From = from;
			m_Book = book;
			m_Page = page;

			if (list == null)
			{
				list = new ArrayList(book.Entries.Count);

				for (var i = 0; i < book.Entries.Count; ++i)
				{
					var obj = book.Entries[i];

					if (CheckFilter(obj))
					{
						list.Add(obj);
					}
				}
			}

			m_List = list;

			var index = GetIndexForPage(page);
			var count = GetCountForIndex(index);

			var tableIndex = 0;

			var pv = book.RootParent as PlayerVendor;

			var canDrop = book.IsChildOf(from.Backpack);
			var canBuy = (pv != null);
			var canPrice = (canDrop || canBuy);

			if (canBuy)
			{
				var vi = pv.GetVendorItem(book);

				canBuy = (vi != null && !vi.IsForSale);
			}

			var width = 600;

			if (!canPrice)
			{
				width = 516;
			}

			X = (624 - width) / 2;

			AddPage(0);

			AddBackground(10, 10, width, 439, 5054);
			AddImageTiled(18, 20, width - 17, 420, 2624);

			if (canPrice)
			{
				AddImageTiled(573, 64, 24, 352, 200);
				AddImageTiled(493, 64, 78, 352, 1416);
			}

			if (canDrop)
			{
				AddImageTiled(24, 64, 32, 352, 1416);
			}

			AddImageTiled(58, 64, 36, 352, 200);
			AddImageTiled(96, 64, 133, 352, 1416);
			AddImageTiled(231, 64, 80, 352, 200);
			AddImageTiled(313, 64, 100, 352, 1416);
			AddImageTiled(415, 64, 76, 352, 200);

			for (var i = index; i < (index + count) && i >= 0 && i < list.Count; ++i)
			{
				var obj = list[i];

				if (!CheckFilter(obj))
				{
					continue;
				}

				AddImageTiled(24, 94 + (tableIndex * 32), canPrice ? 573 : 489, 2, 2624);

				if (obj is BOBLargeEntry)
				{
					tableIndex += ((BOBLargeEntry)obj).Entries.Length;
				}
				else if (obj is BOBSmallEntry)
				{
					++tableIndex;
				}
			}

			AddAlphaRegion(18, 20, width - 17, 420);
			AddImage(5, 5, 10460);
			AddImage(width - 15, 5, 10460);
			AddImage(5, 424, 10460);
			AddImage(width - 15, 424, 10460);

			AddHtmlLocalized(canPrice ? 266 : 224, 32, 200, 32, 1062220, LabelColor, false, false); // Bulk Order Book
			AddHtmlLocalized(63, 64, 200, 32, 1062213, LabelColor, false, false); // Type
			AddHtmlLocalized(147, 64, 200, 32, 1062214, LabelColor, false, false); // Item
			AddHtmlLocalized(246, 64, 200, 32, 1062215, LabelColor, false, false); // Quality
			AddHtmlLocalized(336, 64, 200, 32, 1062216, LabelColor, false, false); // Material
			AddHtmlLocalized(429, 64, 200, 32, 1062217, LabelColor, false, false); // Amount

			AddButton(35, 32, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(70, 32, 200, 32, 1062476, LabelColor, false, false); // Set Filter

			var f = (from.UseOwnFilter ? from.BOBFilter : book.Filter);

			if (f.IsDefault)
			{
				AddHtmlLocalized(canPrice ? 470 : 386, 32, 120, 32, 1062475, 16927, false, false); // Using No Filter
			}
			else if (from.UseOwnFilter)
			{
				AddHtmlLocalized(canPrice ? 470 : 386, 32, 120, 32, 1062451, 16927, false, false); // Using Your Filter
			}
			else
			{
				AddHtmlLocalized(canPrice ? 470 : 386, 32, 120, 32, 1062230, 16927, false, false); // Using Book Filter
			}

			AddButton(375, 416, 4017, 4018, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(410, 416, 120, 20, 1011441, LabelColor, false, false); // EXIT

			if (canDrop)
			{
				AddHtmlLocalized(26, 64, 50, 32, 1062212, LabelColor, false, false); // Drop
			}

			if (canPrice)
			{
				AddHtmlLocalized(516, 64, 200, 32, 1062218, LabelColor, false, false); // Price

				if (canBuy)
				{
					AddHtmlLocalized(576, 64, 200, 32, 1062219, LabelColor, false, false); // Buy
				}
				else
				{
					AddHtmlLocalized(576, 64, 200, 32, 1062227, LabelColor, false, false); // Set

					AddButton(450, 416, 4005, 4007, 4, GumpButtonType.Reply, 0);
					AddHtml(485, 416, 120, 20, "<BASEFONT COLOR=#FFFFFF>Price all</FONT>", false, false);
				}
			}

			tableIndex = 0;

			if (page > 0)
			{
				AddButton(75, 416, 4014, 4016, 2, GumpButtonType.Reply, 0);
				AddHtmlLocalized(110, 416, 150, 20, 1011067, LabelColor, false, false); // Previous page
			}

			if (GetIndexForPage(page + 1) < list.Count)
			{
				AddButton(225, 416, 4005, 4007, 3, GumpButtonType.Reply, 0);
				AddHtmlLocalized(260, 416, 150, 20, 1011066, LabelColor, false, false); // Next page
			}

			for (var i = index; i < (index + count) && i >= 0 && i < list.Count; ++i)
			{
				var obj = list[i];

				if (!CheckFilter(obj))
				{
					continue;
				}

				if (obj is BOBLargeEntry)
				{
					var e = (BOBLargeEntry)obj;

					var y = 96 + (tableIndex * 32);

					if (canDrop)
					{
						AddButton(35, y + 2, 5602, 5606, 5 + (i * 2), GumpButtonType.Reply, 0);
					}

					if (canDrop || (canBuy && e.Price > 0))
					{
						AddButton(579, y + 2, 2117, 2118, 6 + (i * 2), GumpButtonType.Reply, 0);
						AddLabel(495, y, 1152, e.Price.ToString());
					}

					AddHtmlLocalized(61, y, 50, 32, 1062225, LabelColor, false, false); // Large

					for (var j = 0; j < e.Entries.Length; ++j)
					{
						var sub = e.Entries[j];

						AddHtmlLocalized(103, y, 130, 32, sub.Number, LabelColor, false, false);

						if (e.RequireExceptional)
						{
							AddHtmlLocalized(235, y, 80, 20, 1060636, LabelColor, false, false); // exceptional
						}
						else
						{
							AddHtmlLocalized(235, y, 80, 20, 1011542, LabelColor, false, false); // normal
						}

						var name = GetMaterialName(e.Material, e.DeedType, sub.ItemType);

						if (name is int)
						{
							AddHtmlLocalized(316, y, 100, 20, (int)name, LabelColor, false, false);
						}
						else if (name is string)
						{
							AddLabel(316, y, 1152, (string)name);
						}

						AddLabel(421, y, 1152, String.Format("{0} / {1}", sub.AmountCur, e.AmountMax));

						++tableIndex;
						y += 32;
					}
				}
				else if (obj is BOBSmallEntry)
				{
					var e = (BOBSmallEntry)obj;

					var y = 96 + (tableIndex++ * 32);

					if (canDrop)
					{
						AddButton(35, y + 2, 5602, 5606, 5 + (i * 2), GumpButtonType.Reply, 0);
					}

					if (canDrop || (canBuy && e.Price > 0))
					{
						AddButton(579, y + 2, 2117, 2118, 6 + (i * 2), GumpButtonType.Reply, 0);
						AddLabel(495, y, 1152, e.Price.ToString());
					}

					AddHtmlLocalized(61, y, 50, 32, 1062224, LabelColor, false, false); // Small

					AddHtmlLocalized(103, y, 130, 32, e.Number, LabelColor, false, false);

					if (e.RequireExceptional)
					{
						AddHtmlLocalized(235, y, 80, 20, 1060636, LabelColor, false, false); // exceptional
					}
					else
					{
						AddHtmlLocalized(235, y, 80, 20, 1011542, LabelColor, false, false); // normal
					}

					var name = GetMaterialName(e.Material, e.DeedType, e.ItemType);

					if (name is int)
					{
						AddHtmlLocalized(316, y, 100, 20, (int)name, LabelColor, false, false);
					}
					else if (name is string)
					{
						AddLabel(316, y, 1152, (string)name);
					}

					AddLabel(421, y, 1152, String.Format("{0} / {1}", e.AmountCur, e.AmountMax));
				}
			}
		}
	}

	public class BOBFilter
	{
		private int m_Type;
		private int m_Quality;
		private int m_Material;
		private int m_Quantity;

		public bool IsDefault => (m_Type == 0 && m_Quality == 0 && m_Material == 0 && m_Quantity == 0);

		public void Clear()
		{
			m_Type = 0;
			m_Quality = 0;
			m_Material = 0;
			m_Quantity = 0;
		}

		public int Type
		{
			get => m_Type;
			set => m_Type = value;
		}

		public int Quality
		{
			get => m_Quality;
			set => m_Quality = value;
		}

		public int Material
		{
			get => m_Material;
			set => m_Material = value;
		}

		public int Quantity
		{
			get => m_Quantity;
			set => m_Quantity = value;
		}

		public BOBFilter()
		{
		}

		public BOBFilter(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 1:
					{
						m_Type = reader.ReadEncodedInt();
						m_Quality = reader.ReadEncodedInt();
						m_Material = reader.ReadEncodedInt();
						m_Quantity = reader.ReadEncodedInt();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			if (IsDefault)
			{
				writer.WriteEncodedInt(0); // version
			}
			else
			{
				writer.WriteEncodedInt(1); // version

				writer.WriteEncodedInt(m_Type);
				writer.WriteEncodedInt(m_Quality);
				writer.WriteEncodedInt(m_Material);
				writer.WriteEncodedInt(m_Quantity);
			}
		}
	}

	public class BOBFilterGump : Gump
	{
		private readonly PlayerMobile m_From;
		private readonly BulkOrderBook m_Book;

		private const short LabelColor16 = 0x7FFF;

		private static readonly int[,] m_MaterialFilters = new int[,]
			{
				{ 1044067,  1 }, // Blacksmithy
				{ 1062226,  3 }, // Iron
				{ 1018332,  4 }, // Dull Copper
				{ 1018333,  5 }, // Shadow Iron
				{ 1018334,  6 }, // Copper
				{ 1018335,  7 }, // Bronze

				{       0,  0 }, // --Blank--
				{ 1018336,  8 }, // Golden
				{ 1018337,  9 }, // Agapite
				{ 1018338, 10 }, // Verite
				{ 1018339, 11 }, // Valorite
				{       0,  0 }, // --Blank--

				{ 1044094,  2 }, // Tailoring
				{ 1044286, 12 }, // Cloth
				{ 1062235, 13 }, // Leather
				{ 1062236, 14 }, // Spined
				{ 1062237, 15 }, // Horned
				{ 1062238, 16 }  // Barbed
			};

		private static readonly int[,] m_TypeFilters = new int[,]
			{
				{ 1062229, 0 }, // All
				{ 1062224, 1 }, // Small
				{ 1062225, 2 }  // Large
			};

		private static readonly int[,] m_QualityFilters = new int[,]
			{
				{ 1062229, 0 }, // All
				{ 1011542, 1 }, // Normal
				{ 1060636, 2 }  // Exceptional
			};

		private static readonly int[,] m_AmountFilters = new int[,]
			{
				{ 1062229, 0 }, // All
				{ 1049706, 1 }, // 10
				{ 1016007, 2 }, // 15
				{ 1062239, 3 }  // 20
			};

		private static readonly int[][,] m_Filters = new int[][,]
			{
				m_TypeFilters,
				m_QualityFilters,
				m_MaterialFilters,
				m_AmountFilters
			};

		private static readonly int[] m_XOffsets_Type = new int[] { 0, 75, 170 };
		private static readonly int[] m_XOffsets_Quality = new int[] { 0, 75, 170 };
		private static readonly int[] m_XOffsets_Amount = new int[] { 0, 75, 180, 275 };
		private static readonly int[] m_XOffsets_Material = new int[] { 0, 105, 210, 305, 390, 485 };

		private static readonly int[] m_XWidths_Small = new int[] { 50, 50, 70, 50 };
		private static readonly int[] m_XWidths_Large = new int[] { 80, 50, 50, 50, 50, 50 };

		private void AddFilterList(int x, int y, int[] xOffsets, int yOffset, int[,] filters, int[] xWidths, int filterValue, int filterIndex)
		{
			for (var i = 0; i < filters.GetLength(0); ++i)
			{
				var number = filters[i, 0];

				if (number == 0)
				{
					continue;
				}

				var isSelected = (filters[i, 1] == filterValue);

				if (!isSelected && (i % xOffsets.Length) == 0)
				{
					isSelected = (filterValue == 0);
				}

				AddHtmlLocalized(x + 35 + xOffsets[i % xOffsets.Length], y + ((i / xOffsets.Length) * yOffset), xWidths[i % xOffsets.Length], 32, number, isSelected ? (short)0x421F : LabelColor16, false, false);
				AddButton(x + xOffsets[i % xOffsets.Length], y + ((i / xOffsets.Length) * yOffset), 4005, 4007, 4 + filterIndex + (i * 4), GumpButtonType.Reply, 0);
			}
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			var f = (m_From.UseOwnFilter ? m_From.BOBFilter : m_Book.Filter);

			var index = info.ButtonID;

			switch (index)
			{
				case 0: // Apply
					{
						m_From.SendGump(new BOBGump(m_From, m_Book));

						break;
					}
				case 1: // Set Book Filter
					{
						m_From.UseOwnFilter = false;
						m_From.SendGump(new BOBFilterGump(m_From, m_Book));

						break;
					}
				case 2: // Set Your Filter
					{
						m_From.UseOwnFilter = true;
						m_From.SendGump(new BOBFilterGump(m_From, m_Book));

						break;
					}
				case 3: // Clear Filter
					{
						f.Clear();
						m_From.SendGump(new BOBFilterGump(m_From, m_Book));

						break;
					}
				default:
					{
						index -= 4;

						var type = index % 4;
						index /= 4;

						if (type >= 0 && type < m_Filters.Length)
						{
							var filters = m_Filters[type];

							if (index >= 0 && index < filters.GetLength(0))
							{
								if (filters[index, 0] == 0)
								{
									break;
								}

								switch (type)
								{
									case 0: f.Type = filters[index, 1]; break;
									case 1: f.Quality = filters[index, 1]; break;
									case 2: f.Material = filters[index, 1]; break;
									case 3: f.Quantity = filters[index, 1]; break;
								}

								m_From.SendGump(new BOBFilterGump(m_From, m_Book));
							}
						}

						break;
					}
			}
		}

		public BOBFilterGump(PlayerMobile from, BulkOrderBook book) : base(12, 24)
		{
			from.CloseGump(typeof(BOBGump));
			from.CloseGump(typeof(BOBFilterGump));

			m_From = from;
			m_Book = book;

			var f = (from.UseOwnFilter ? from.BOBFilter : book.Filter);

			AddPage(0);

			AddBackground(10, 10, 600, 439, 5054);

			AddImageTiled(18, 20, 583, 420, 2624);
			AddAlphaRegion(18, 20, 583, 420);

			AddImage(5, 5, 10460);
			AddImage(585, 5, 10460);
			AddImage(5, 424, 10460);
			AddImage(585, 424, 10460);

			AddHtmlLocalized(270, 32, 200, 32, 1062223, LabelColor16, false, false); // Filter Preference

			AddHtmlLocalized(26, 64, 120, 32, 1062228, LabelColor16, false, false); // Bulk Order Type
			AddFilterList(25, 96, m_XOffsets_Type, 40, m_TypeFilters, m_XWidths_Small, f.Type, 0);

			AddHtmlLocalized(320, 64, 50, 32, 1062215, LabelColor16, false, false); // Quality
			AddFilterList(320, 96, m_XOffsets_Quality, 40, m_QualityFilters, m_XWidths_Small, f.Quality, 1);

			AddHtmlLocalized(26, 160, 120, 32, 1062232, LabelColor16, false, false); // Material Type
			AddFilterList(25, 192, m_XOffsets_Material, 40, m_MaterialFilters, m_XWidths_Large, f.Material, 2);

			AddHtmlLocalized(26, 320, 120, 32, 1062217, LabelColor16, false, false); // Amount
			AddFilterList(25, 352, m_XOffsets_Amount, 40, m_AmountFilters, m_XWidths_Small, f.Quantity, 3);

			AddHtmlLocalized(75, 416, 120, 32, 1062477, (from.UseOwnFilter ? LabelColor16 : (short)0x421F), false, false); // Set Book Filter
			AddButton(40, 416, 4005, 4007, 1, GumpButtonType.Reply, 0);

			AddHtmlLocalized(235, 416, 120, 32, 1062478, (from.UseOwnFilter ? (short)0x421F : LabelColor16), false, false); // Set Your Filter
			AddButton(200, 416, 4005, 4007, 2, GumpButtonType.Reply, 0);

			AddHtmlLocalized(405, 416, 120, 32, 1062231, LabelColor16, false, false); // Clear Filter
			AddButton(370, 416, 4005, 4007, 3, GumpButtonType.Reply, 0);

			AddHtmlLocalized(540, 416, 50, 32, 1011046, LabelColor16, false, false); // APPLY
			AddButton(505, 416, 4017, 4018, 0, GumpButtonType.Reply, 0);
		}
	}

	public class BOBSmallEntry
	{
		private readonly Type m_ItemType;
		private readonly bool m_RequireExceptional;
		private readonly BODType m_DeedType;
		private readonly BulkMaterialType m_Material;
		private readonly int m_AmountCur, m_AmountMax;
		private readonly int m_Number;
		private readonly int m_Graphic;
		private int m_Price;

		public Type ItemType => m_ItemType;
		public bool RequireExceptional => m_RequireExceptional;
		public BODType DeedType => m_DeedType;
		public BulkMaterialType Material => m_Material;
		public int AmountCur => m_AmountCur;
		public int AmountMax => m_AmountMax;
		public int Number => m_Number;
		public int Graphic => m_Graphic;
		public int Price { get => m_Price; set => m_Price = value; }

		public Item Reconstruct()
		{
			SmallBOD bod = null;

			if (m_DeedType == BODType.Smith)
			{
				bod = new SmallSmithBOD(m_AmountCur, m_AmountMax, m_ItemType, m_Number, m_Graphic, m_RequireExceptional, m_Material);
			}
			else if (m_DeedType == BODType.Tailor)
			{
				bod = new SmallTailorBOD(m_AmountCur, m_AmountMax, m_ItemType, m_Number, m_Graphic, m_RequireExceptional, m_Material);
			}

			return bod;
		}

		public BOBSmallEntry(SmallBOD bod)
		{
			m_ItemType = bod.Type;
			m_RequireExceptional = bod.RequireExceptional;

			if (bod is SmallTailorBOD)
			{
				m_DeedType = BODType.Tailor;
			}
			else if (bod is SmallSmithBOD)
			{
				m_DeedType = BODType.Smith;
			}

			m_Material = bod.Material;
			m_AmountCur = bod.AmountCur;
			m_AmountMax = bod.AmountMax;
			m_Number = bod.Number;
			m_Graphic = bod.Graphic;
		}

		public BOBSmallEntry(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						var type = reader.ReadString();

						if (type != null)
						{
							m_ItemType = ScriptCompiler.FindTypeByFullName(type);
						}

						m_RequireExceptional = reader.ReadBool();

						m_DeedType = (BODType)reader.ReadEncodedInt();

						m_Material = (BulkMaterialType)reader.ReadEncodedInt();
						m_AmountCur = reader.ReadEncodedInt();
						m_AmountMax = reader.ReadEncodedInt();
						m_Number = reader.ReadEncodedInt();
						m_Graphic = reader.ReadEncodedInt();
						m_Price = reader.ReadEncodedInt();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_ItemType == null ? null : m_ItemType.FullName);

			writer.Write(m_RequireExceptional);

			writer.WriteEncodedInt((int)m_DeedType);
			writer.WriteEncodedInt((int)m_Material);
			writer.WriteEncodedInt(m_AmountCur);
			writer.WriteEncodedInt(m_AmountMax);
			writer.WriteEncodedInt(m_Number);
			writer.WriteEncodedInt(m_Graphic);
			writer.WriteEncodedInt(m_Price);
		}
	}

	public class BOBLargeEntry
	{
		private readonly bool m_RequireExceptional;
		private readonly BODType m_DeedType;
		private readonly BulkMaterialType m_Material;
		private readonly int m_AmountMax;
		private int m_Price;
		private readonly BOBLargeSubEntry[] m_Entries;

		public bool RequireExceptional => m_RequireExceptional;
		public BODType DeedType => m_DeedType;
		public BulkMaterialType Material => m_Material;
		public int AmountMax => m_AmountMax;
		public int Price { get => m_Price; set => m_Price = value; }
		public BOBLargeSubEntry[] Entries => m_Entries;

		public Item Reconstruct()
		{
			LargeBOD bod = null;

			if (m_DeedType == BODType.Smith)
			{
				bod = new LargeSmithBOD(m_AmountMax, m_RequireExceptional, m_Material, ReconstructEntries());
			}
			else if (m_DeedType == BODType.Tailor)
			{
				bod = new LargeTailorBOD(m_AmountMax, m_RequireExceptional, m_Material, ReconstructEntries());
			}

			for (var i = 0; bod != null && i < bod.Entries.Length; ++i)
			{
				bod.Entries[i].Owner = bod;
			}

			return bod;
		}

		private LargeBulkEntry[] ReconstructEntries()
		{
			var entries = new LargeBulkEntry[m_Entries.Length];

			for (var i = 0; i < m_Entries.Length; ++i)
			{
				entries[i] = new LargeBulkEntry(null, new SmallBulkEntry(m_Entries[i].ItemType, m_Entries[i].Number, m_Entries[i].Graphic)) {
					Amount = m_Entries[i].AmountCur
				};
			}

			return entries;
		}

		public BOBLargeEntry(LargeBOD bod)
		{
			m_RequireExceptional = bod.RequireExceptional;

			if (bod is LargeTailorBOD)
			{
				m_DeedType = BODType.Tailor;
			}
			else if (bod is LargeSmithBOD)
			{
				m_DeedType = BODType.Smith;
			}

			m_Material = bod.Material;
			m_AmountMax = bod.AmountMax;

			m_Entries = new BOBLargeSubEntry[bod.Entries.Length];

			for (var i = 0; i < m_Entries.Length; ++i)
			{
				m_Entries[i] = new BOBLargeSubEntry(bod.Entries[i]);
			}
		}

		public BOBLargeEntry(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						m_RequireExceptional = reader.ReadBool();

						m_DeedType = (BODType)reader.ReadEncodedInt();

						m_Material = (BulkMaterialType)reader.ReadEncodedInt();
						m_AmountMax = reader.ReadEncodedInt();
						m_Price = reader.ReadEncodedInt();

						m_Entries = new BOBLargeSubEntry[reader.ReadEncodedInt()];

						for (var i = 0; i < m_Entries.Length; ++i)
						{
							m_Entries[i] = new BOBLargeSubEntry(reader);
						}

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_RequireExceptional);

			writer.WriteEncodedInt((int)m_DeedType);
			writer.WriteEncodedInt((int)m_Material);
			writer.WriteEncodedInt(m_AmountMax);
			writer.WriteEncodedInt(m_Price);

			writer.WriteEncodedInt(m_Entries.Length);

			for (var i = 0; i < m_Entries.Length; ++i)
			{
				m_Entries[i].Serialize(writer);
			}
		}
	}

	public class BOBLargeSubEntry
	{
		private readonly Type m_ItemType;
		private readonly int m_AmountCur;
		private readonly int m_Number;
		private readonly int m_Graphic;

		public Type ItemType => m_ItemType;
		public int AmountCur => m_AmountCur;
		public int Number => m_Number;
		public int Graphic => m_Graphic;

		public BOBLargeSubEntry(LargeBulkEntry lbe)
		{
			m_ItemType = lbe.Details.Type;
			m_AmountCur = lbe.Amount;
			m_Number = lbe.Details.Number;
			m_Graphic = lbe.Details.Graphic;
		}

		public BOBLargeSubEntry(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						var type = reader.ReadString();

						if (type != null)
						{
							m_ItemType = ScriptCompiler.FindTypeByFullName(type);
						}

						m_AmountCur = reader.ReadEncodedInt();
						m_Number = reader.ReadEncodedInt();
						m_Graphic = reader.ReadEncodedInt();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(m_ItemType == null ? null : m_ItemType.FullName);

			writer.WriteEncodedInt(m_AmountCur);
			writer.WriteEncodedInt(m_Number);
			writer.WriteEncodedInt(m_Graphic);
		}
	}

	public class BODBuyGump : Gump
	{
		private readonly PlayerMobile m_From;
		private readonly BulkOrderBook m_Book;
		private readonly object m_Object;
		private readonly int m_Price;
		private readonly int m_Page;

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 2)
			{
				var pv = m_Book.RootParent as PlayerVendor;

				if (m_Book.Entries.Contains(m_Object) && pv != null)
				{
					var price = 0;

					var vi = pv.GetVendorItem(m_Book);

					if (vi != null && !vi.IsForSale)
					{
						if (m_Object is BOBLargeEntry)
						{
							price = ((BOBLargeEntry)m_Object).Price;
						}
						else if (m_Object is BOBSmallEntry)
						{
							price = ((BOBSmallEntry)m_Object).Price;
						}
					}

					if (price != m_Price)
					{
						pv.SayTo(m_From, "The price has been been changed. If you like, you may offer to purchase the item again.");
					}
					else if (price == 0)
					{
						pv.SayTo(m_From, 1062382); // The deed selected is not available.
					}
					else
					{
						Item item = null;

						if (m_Object is BOBLargeEntry)
						{
							item = ((BOBLargeEntry)m_Object).Reconstruct();
						}
						else if (m_Object is BOBSmallEntry)
						{
							item = ((BOBSmallEntry)m_Object).Reconstruct();
						}

						if (item == null)
						{
							m_From.SendMessage("Internal error. The bulk order deed could not be reconstructed.");
						}
						else
						{
							pv.Say(m_From.Name);

							var pack = m_From.Backpack;

							if ((pack == null) || ((pack != null) && (!pack.CheckHold(m_From, item, true, true, 0, item.PileWeight + item.TotalWeight))))
							{
								pv.SayTo(m_From, 503204); // You do not have room in your backpack for this
								m_From.SendGump(new BOBGump(m_From, m_Book, m_Page, null));
							}
							else
							{
								if ((pack != null && pack.ConsumeTotal(typeof(Gold), price)) || Banker.Withdraw(m_From, price))
								{
									m_Book.Entries.Remove(m_Object);
									m_Book.InvalidateProperties();
									pv.HoldGold += price;
									m_From.AddToBackpack(item);
									m_From.SendLocalizedMessage(1045152); // The bulk order deed has been placed in your backpack.

									if (m_Book.Entries.Count / 5 < m_Book.ItemCount)
									{
										m_Book.ItemCount--;
										m_Book.InvalidateItems();
									}

									if (m_Book.Entries.Count > 0)
									{
										m_From.SendGump(new BOBGump(m_From, m_Book, m_Page, null));
									}
									else
									{
										m_From.SendLocalizedMessage(1062381); // The book is empty.
									}
								}
								else
								{
									pv.SayTo(m_From, 503205); // You cannot afford this item.
									item.Delete();
								}
							}
						}
					}
				}
				else
				{
					if (pv == null)
					{
						m_From.SendLocalizedMessage(1062382); // The deed selected is not available.
					}
					else
					{
						pv.SayTo(m_From, 1062382); // The deed selected is not available.
					}
				}
			}
			else
			{
				m_From.SendLocalizedMessage(503207); // Cancelled purchase.
			}
		}

		public BODBuyGump(PlayerMobile from, BulkOrderBook book, object obj, int page, int price) : base(100, 200)
		{
			m_From = from;
			m_Book = book;
			m_Object = obj;
			m_Price = price;
			m_Page = page;

			AddPage(0);

			AddBackground(100, 10, 300, 150, 5054);

			AddHtmlLocalized(125, 20, 250, 24, 1019070, false, false); // You have agreed to purchase:
			AddHtmlLocalized(125, 45, 250, 24, 1045151, false, false); // a bulk order deed

			AddHtmlLocalized(125, 70, 250, 24, 1019071, false, false); // for the amount of:
			AddLabel(125, 95, 0, price.ToString());

			AddButton(250, 130, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(282, 130, 100, 24, 1011012, false, false); // CANCEL

			AddButton(120, 130, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(152, 130, 100, 24, 1011036, false, false); // OKAY
		}
	}
}