using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Prompts;
using Server.Spells.Chivalry;
using Server.Spells.Magery;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	public class Runebook : Item, ISecurable, ICraftable
	{
		public static readonly TimeSpan UseDelay = TimeSpan.FromSeconds(7.0);

		private BookQuality m_Quality;

		[CommandProperty(AccessLevel.GameMaster)]
		public BookQuality Quality
		{
			get => m_Quality;
			set
			{
				m_Quality = value;
				InvalidateProperties();
			}
		}

		private string m_Description;
		private int m_DefaultIndex;
		private Mobile m_Crafter;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextUse { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get => m_Crafter;
			set
			{
				m_Crafter = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Description
		{
			get => m_Description;
			set
			{
				m_Description = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int CurCharges { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxCharges { get; set; }

		public List<Mobile> Openers { get; set; } = new();

		public override int LabelNumber => 1041267;  // runebook

		[Constructable]
		public Runebook(int maxCharges) : base(Core.AOS ? 0x22C5 : 0xEFA)
		{
			Weight = Core.SE ? 1.0 : 3.0;
			LootType = LootType.Blessed;
			Hue = 0x461;

			Layer = Core.AOS ? Layer.Invalid : Layer.OneHanded;

			Entries = new List<RunebookEntry>();

			MaxCharges = maxCharges;

			m_DefaultIndex = -1;

			Level = SecureLevel.CoOwners;
		}

		[Constructable]
		public Runebook() : this(Core.SE ? 12 : 6)
		{
		}

		public List<RunebookEntry> Entries { get; private set; }

		public RunebookEntry Default
		{
			get
			{
				if (m_DefaultIndex >= 0 && m_DefaultIndex < Entries.Count)
				{
					return Entries[m_DefaultIndex];
				}

				return null;
			}
			set
			{
				if (value == null)
				{
					m_DefaultIndex = -1;
				}
				else
				{
					m_DefaultIndex = Entries.IndexOf(value);
				}
			}
		}

		public Runebook(Serial serial) : base(serial)
		{
		}

		public override bool AllowEquipedCast(Mobile from)
		{
			return true;
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			SetSecureLevelEntry.AddTo(from, this, list);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(3);

			writer.Write((byte)m_Quality);

			writer.Write(m_Crafter);

			writer.Write((int)Level);

			writer.Write(Entries.Count);

			for (var i = 0; i < Entries.Count; ++i)
			{
				Entries[i].Serialize(writer);
			}

			writer.Write(m_Description);
			writer.Write(CurCharges);
			writer.Write(MaxCharges);
			writer.Write(m_DefaultIndex);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			LootType = LootType.Blessed;

			if (Core.SE && Weight == 3.0)
			{
				Weight = 1.0;
			}

			var version = reader.ReadInt();

			switch (version)
			{
				case 3:
					{
						m_Quality = (BookQuality)reader.ReadByte();
						goto case 2;
					}
				case 2:
					{
						m_Crafter = reader.ReadMobile();
						goto case 1;
					}
				case 1:
					{
						Level = (SecureLevel)reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						var count = reader.ReadInt();

						Entries = new List<RunebookEntry>(count);

						for (var i = 0; i < count; ++i)
						{
							Entries.Add(new RunebookEntry(reader));
						}

						m_Description = reader.ReadString();
						CurCharges = reader.ReadInt();
						MaxCharges = reader.ReadInt();
						m_DefaultIndex = reader.ReadInt();

						break;
					}
			}
		}

		public void DropRune(Mobile from, RunebookEntry e, int index)
		{
			if (m_DefaultIndex > index)
			{
				m_DefaultIndex -= 1;
			}
			else if (m_DefaultIndex == index)
			{
				m_DefaultIndex = -1;
			}

			Entries.RemoveAt(index);

			var rune = new RecallRune
			{
				Target = e.Location,
				TargetMap = e.Map,
				Description = e.Description,
				House = e.House,
				Marked = true
			};

			_ = from.AddToBackpack(rune);

			from.SendLocalizedMessage(502421); // You have removed the rune.
		}

		public bool IsOpen(Mobile toCheck)
		{
			var ns = toCheck.NetState;

			if (ns != null)
			{
				foreach (var gump in ns.Gumps)
				{
					if (gump is RunebookGump bookGump && bookGump.Book == this)
					{
						return true;
					}
				}
			}

			return false;
		}

		public override bool DisplayLootType => Core.AOS;

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Quality == BookQuality.Exceptional)
			{
				list.Add(1063341); // exceptional
			}

			if (m_Crafter != null)
			{
				list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~
			}

			if (m_Description != null && m_Description.Length > 0)
			{
				list.Add(m_Description);
			}
		}

		public override bool OnDragLift(Mobile from)
		{
			if (from.HasGump(typeof(RunebookGump)))
			{
				from.SendLocalizedMessage(500169); // You cannot pick that up.
				return false;
			}

			foreach (var m in Openers)
			{
				if (IsOpen(m))
				{
					_ = m.CloseGump(typeof(RunebookGump));
				}
			}

			Openers.Clear();

			return true;
		}

		public override void OnSingleClick(Mobile from)
		{
			if (m_Description != null && m_Description.Length > 0)
			{
				LabelTo(from, m_Description);
			}

			base.OnSingleClick(from);

			if (m_Crafter != null)
			{
				LabelTo(from, 1050043, m_Crafter.Name);
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.InRange(GetWorldLocation(), Core.ML ? 3 : 1) && CheckAccess(from))
			{
				if (RootParent is BaseCreature)
				{
					from.SendLocalizedMessage(502402); // That is inaccessible.
					return;
				}

				if (DateTime.UtcNow < NextUse)
				{
					from.SendLocalizedMessage(502406); // This book needs time to recharge.
					return;
				}

				_ = from.CloseGump(typeof(RunebookGump));
				_ = from.SendGump(new RunebookGump(from, this));

				Openers.Add(from);
			}
		}

		public virtual void OnTravel()
		{
			if (!Core.SA)
			{
				NextUse = DateTime.UtcNow + UseDelay;
			}
		}

		public override void OnAfterDuped(Item newItem)
		{
			if (newItem is not Runebook book)
			{
				return;
			}

			book.Entries = new List<RunebookEntry>();

			for (var i = 0; i < Entries.Count; i++)
			{
				var entry = Entries[i];

				book.Entries.Add(new RunebookEntry(entry.Location, entry.Map, entry.Description, entry.House));
			}
		}

		public bool CheckAccess(Mobile m)
		{
			if (!IsLockedDown || m.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}

			var house = BaseHouse.FindHouseAt(this);

			if (house != null && house.IsAosRules && (house.Public ? house.IsBanned(m) : !house.HasAccess(m)))
			{
				return false;
			}

			return house != null && house.HasSecureAccess(m, Level);
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if (dropped is RecallRune rune)
			{
				if (IsLockedDown && from.AccessLevel < AccessLevel.GameMaster)
				{
					from.SendLocalizedMessage(502413, null, 0x35); // That cannot be done while the book is locked down.
				}
				else if (IsOpen(from))
				{
					from.SendLocalizedMessage(1005571); // You cannot place objects in the book while viewing the contents.
				}
				else if (Entries.Count < 16)
				{
					if (rune.Marked && rune.TargetMap != null)
					{
						Entries.Add(new RunebookEntry(rune.Target, rune.TargetMap, rune.Description, rune.House));

						dropped.Delete();

						_ = from.Send(new PlaySound(0x42, GetWorldLocation()));

						var desc = rune.Description;

						if (desc == null || (desc = desc.Trim()).Length == 0)
						{
							desc = "(indescript)";
						}

						from.SendMessage(desc);

						return true;
					}
					else
					{
						from.SendLocalizedMessage(502409); // This rune does not have a marked location.
					}
				}
				else
				{
					from.SendLocalizedMessage(502401); // This runebook is full.
				}
			}
			else if (dropped is RecallScroll)
			{
				if (CurCharges < MaxCharges)
				{
					_ = from.Send(new PlaySound(0x249, GetWorldLocation()));

					var amount = dropped.Amount;

					if (amount > (MaxCharges - CurCharges))
					{
						dropped.Consume(MaxCharges - CurCharges);
						CurCharges = MaxCharges;
					}
					else
					{
						CurCharges += amount;
						dropped.Delete();

						return true;
					}
				}
				else
				{
					from.SendLocalizedMessage(502410); // This book already has the maximum amount of charges.
				}
			}

			return false;
		}

		#region ICraftable

		public virtual int OnCraft(int quality, bool makersMark, Mobile from, ICraftSystem craftSystem, Type typeRes, ICraftTool tool, ICraftItem craftItem, int resHue)
		{
			var charges = 5 + quality + (int)(from.Skills[SkillName.Inscribe].Value / 30);

			if (charges > 10)
			{
				charges = 10;
			}

			MaxCharges = Core.SE ? charges * 2 : charges;

			if (makersMark)
			{
				Crafter = from;
			}

			m_Quality = (BookQuality)(quality - 1);

			return quality;
		}

		#endregion
	}

	public class RunebookEntry
	{
		private Point3D m_Location;

		public Point3D Location => m_Location;

		public Map Map { get; }

		public string Description { get; }

		public BaseHouse House { get; }

		public RunebookEntry(Point3D loc, Map map, string desc, BaseHouse house)
		{
			m_Location = loc;
			Map = map;
			Description = desc;
			House = house;
		}

		public RunebookEntry(GenericReader reader)
		{
			int version = reader.ReadByte();

			switch (version)
			{
				case 1:
					{
						House = reader.ReadItem() as BaseHouse;
						goto case 0;
					}
				case 0:
					{
						m_Location = reader.ReadPoint3D();
						Map = reader.ReadMap();
						Description = reader.ReadString();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			if (House != null && !House.Deleted)
			{
				writer.Write((byte)1); // version

				writer.Write(House);
			}
			else
			{
				writer.Write((byte)0); // version
			}

			writer.Write(m_Location);
			writer.Write(Map);
			writer.Write(Description);
		}
	}

	public class RunebookGump : Gump
	{
		public static int GetMapHue(Map map)
		{
			if (map == Map.Trammel)
			{
				return 10;
			}
			else if (map == Map.Felucca)
			{
				return 81;
			}
			else if (map == Map.Ilshenar)
			{
				return 1102;
			}
			else if (map == Map.Malas)
			{
				return 1102;
			}
			else if (map == Map.Tokuno)
			{
				return 1154;
			}

			return 0;
		}

		public static string GetName(string name)
		{
			if (name == null || (name = name.Trim()).Length <= 0)
			{
				return "(indescript)";
			}

			return name;
		}

		public static bool HasSpell(Mobile from, SpellName spell)
		{
			var book = SpellbookHelper.Find(from, spell);

			return book != null && book.HasSpell(spell);
		}

		public Runebook Book { get; }

		public RunebookGump(Mobile from, Runebook book) 
			: base(150, 200)
		{
			Book = book;

			AddBackground();
			AddIndex();

			for (var page = 0; page < 8; ++page)
			{
				AddPage(2 + page);

				AddButton(125, 14, 2205, 2205, 0, GumpButtonType.Page, 1 + page);

				if (page < 7)
				{
					AddButton(393, 14, 2206, 2206, 0, GumpButtonType.Page, 3 + page);
				}

				for (var half = 0; half < 2; ++half)
				{
					AddDetails((page * 2) + half, half);
				}
			}
		}

		private void AddBackground()
		{
			AddPage(0);

			// Background image
			AddImage(100, 10, 2200);

			// Two separators
			for (var i = 0; i < 2; ++i)
			{
				var xOffset = 125 + (i * 165);

				AddImage(xOffset, 50, 57);
				xOffset += 20;

				for (var j = 0; j < 6; ++j, xOffset += 15)
				{
					AddImage(xOffset, 50, 58);
				}

				AddImage(xOffset - 5, 50, 59);
			}

			// First four page buttons
			for (int i = 0, xOffset = 130, gumpID = 2225; i < 4; ++i, xOffset += 35, ++gumpID)
			{
				AddButton(xOffset, 187, gumpID, gumpID, 0, GumpButtonType.Page, 2 + i);
			}

			// Next four page buttons
			for (int i = 0, xOffset = 300, gumpID = 2229; i < 4; ++i, xOffset += 35, ++gumpID)
			{
				AddButton(xOffset, 187, gumpID, gumpID, 0, GumpButtonType.Page, 6 + i);
			}

			// Charges
			AddHtmlLocalized(140, 40, 80, 18, 1011296, false, false); // Charges:
			AddHtml(220, 40, 30, 18, Book.CurCharges.ToString(), false, false);

			// Max charges
			AddHtmlLocalized(300, 40, 100, 18, 1011297, false, false); // Max Charges:
			AddHtml(400, 40, 30, 18, Book.MaxCharges.ToString(), false, false);
		}

		private void AddIndex()
		{
			// Index
			AddPage(1);

			// Rename button
			AddButton(125, 15, 2472, 2473, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(158, 22, 100, 18, 1011299, false, false); // Rename book

			// List of entries
			var entries = Book.Entries;

			for (var i = 0; i < 16; ++i)
			{
				string desc;
				int hue;

				if (i < entries.Count)
				{
					desc = GetName(entries[i].Description);
					hue = GetMapHue(entries[i].Map);
				}
				else
				{
					desc = "Empty";
					hue = 0;
				}

				// Use charge button
				AddButton(130 + (i / 8 * 160), 65 + (i % 8 * 15), 2103, 2104, 2 + (i * 6) + 0, GumpButtonType.Reply, 0);

				// Description label
				AddLabelCropped(145 + (i / 8 * 160), 60 + (i % 8 * 15), 115, 17, hue, desc);
			}

			// Turn page button
			AddButton(393, 14, 2206, 2206, 0, GumpButtonType.Page, 2);
		}

		private void AddDetails(int index, int half)
		{
			// Use charge button
			AddButton(130 + (half * 160), 65, 2103, 2104, 2 + (index * 6) + 0, GumpButtonType.Reply, 0);

			string desc;
			int hue;

			if (index < Book.Entries.Count)
			{
				var e = Book.Entries[index];

				desc = GetName(e.Description);
				hue = GetMapHue(e.Map);

				// Location labels
				int xLong = 0, yLat = 0;
				int xMins = 0, yMins = 0;
				bool xEast = false, ySouth = false;

				if (Sextant.Format(e.Location, e.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
				{
					AddLabel(135 + (half * 160), 80, 0, String.Format("{0}° {1}'{2}", yLat, yMins, ySouth ? "S" : "N"));
					AddLabel(135 + (half * 160), 95, 0, String.Format("{0}° {1}'{2}", xLong, xMins, xEast ? "E" : "W"));
				}

				// Drop rune button
				AddButton(135 + (half * 160), 115, 2437, 2438, 2 + (index * 6) + 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(150 + (half * 160), 115, 100, 18, 1011298, false, false); // Drop rune

				// Set as default button
				var defButtonID = e != Book.Default ? 2361 : 2360;

				AddButton(160 + (half * 140), 20, defButtonID, defButtonID, 2 + (index * 6) + 2, GumpButtonType.Reply, 0);
				AddHtmlLocalized(175 + (half * 140), 15, 100, 18, 1011300, false, false); // Set default

				if (Core.AOS)
				{
					AddButton(135 + (half * 160), 140, 2103, 2104, 2 + (index * 6) + 3, GumpButtonType.Reply, 0);
					AddHtmlLocalized(150 + (half * 160), 136, 110, 20, 1062722, false, false); // Recall

					AddButton(135 + (half * 160), 158, 2103, 2104, 2 + (index * 6) + 4, GumpButtonType.Reply, 0);
					AddHtmlLocalized(150 + (half * 160), 154, 110, 20, 1062723, false, false); // Gate Travel

					AddButton(135 + (half * 160), 176, 2103, 2104, 2 + (index * 6) + 5, GumpButtonType.Reply, 0);
					AddHtmlLocalized(150 + (half * 160), 172, 110, 20, 1062724, false, false); // Sacred Journey
				}
				else
				{
					// Recall button
					AddButton(135 + (half * 160), 140, 2271, 2271, 2 + (index * 6) + 3, GumpButtonType.Reply, 0);

					// Gate button
					AddButton(205 + (half * 160), 140, 2291, 2291, 2 + (index * 6) + 4, GumpButtonType.Reply, 0);
				}
			}
			else
			{
				desc = "Empty";
				hue = 0;
			}

			// Description label
			AddLabelCropped(145 + (half * 160), 60, 115, 17, hue, desc);
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			var from = state.Mobile;

			if (Book.Deleted || !from.InRange(Book.GetWorldLocation(), Core.ML ? 3 : 1) || !Multis.DesignContext.Check(from))
			{
				_ = Book.Openers.Remove(from);
				return;
			}

			var buttonID = info.ButtonID;

			if (buttonID == 1) // Rename book
			{
				if (!Book.IsLockedDown || from.AccessLevel >= AccessLevel.GameMaster)
				{
					from.SendLocalizedMessage(502414); // Please enter a title for the runebook:
					from.Prompt = new InternalPrompt(Book);
				}
				else
				{
					_ = Book.Openers.Remove(from);

					from.SendLocalizedMessage(502413, null, 0x35); // That cannot be done while the book is locked down.
				}
			}
			else
			{
				buttonID -= 2;

				var index = buttonID / 6;
				var type = buttonID % 6;

				if (index >= 0 && index < Book.Entries.Count)
				{
					var e = Book.Entries[index];

					switch (type)
					{
						case 0: // Use charges
							{
								if (Book.CurCharges <= 0)
								{
									_ = from.CloseGump(typeof(RunebookGump));
									_ = from.SendGump(new RunebookGump(from, Book));

									from.SendLocalizedMessage(502412); // There are no charges left on that item.
								}
								else
								{
									int xLong = 0, yLat = 0;
									int xMins = 0, yMins = 0;
									bool xEast = false, ySouth = false;

									if (Sextant.Format(e.Location, e.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
									{
										var location = String.Format("{0}° {1}'{2}, {3}° {4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
										from.SendMessage(location);
									}

									Book.OnTravel();
									_ = new RecallSpell(from, Book, e, Book).Cast();

									_ = Book.Openers.Remove(from);
								}

								break;
							}
						case 1: // Drop rune
							{
								if (!Book.IsLockedDown || from.AccessLevel >= AccessLevel.GameMaster)
								{
									Book.DropRune(from, e, index);

									_ = from.CloseGump(typeof(RunebookGump));
									if (!Core.ML)
									{
										_ = from.SendGump(new RunebookGump(from, Book));
									}
								}
								else
								{
									_ = Book.Openers.Remove(from);

									from.SendLocalizedMessage(502413, null, 0x35); // That cannot be done while the book is locked down.
								}

								break;
							}
						case 2: // Set default
							{
								if (Book.CheckAccess(from))
								{
									Book.Default = e;

									_ = from.CloseGump(typeof(RunebookGump));
									_ = from.SendGump(new RunebookGump(from, Book));

									from.SendLocalizedMessage(502417); // New default location set.
								}

								break;
							}
						case 3: // Recall
							{
								if (HasSpell(from, SpellName.Recall))
								{
									int xLong = 0, yLat = 0;
									int xMins = 0, yMins = 0;
									bool xEast = false, ySouth = false;

									if (Sextant.Format(e.Location, e.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
									{
										var location = String.Format("{0}° {1}'{2}, {3}° {4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
										from.SendMessage(location);
									}

									Book.OnTravel();

									var s = new RecallSpell(from, null, e, null);

									_ = s.Cast();
								}
								else
								{
									from.SendLocalizedMessage(500015); // You do not have that spell!
								}

								_ = Book.Openers.Remove(from);

								break;
							}
						case 4: // Gate
							{
								if (HasSpell(from, SpellName.GateTravel))
								{
									int xLong = 0, yLat = 0;
									int xMins = 0, yMins = 0;
									bool xEast = false, ySouth = false;

									if (Sextant.Format(e.Location, e.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
									{
										var location = String.Format("{0}° {1}'{2}, {3}° {4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
										from.SendMessage(location);
									}

									Book.OnTravel();

									var s = new GateTravelSpell(from, null, e);

									_ = s.Cast();
								}
								else
								{
									from.SendLocalizedMessage(500015); // You do not have that spell!
								}

								_ = Book.Openers.Remove(from);

								break;
							}
						case 5: // Sacred Journey
							{
								if (Core.AOS)
								{
									if (HasSpell(from, SpellName.SacredJourney))
									{
										int xLong = 0, yLat = 0;
										int xMins = 0, yMins = 0;
										bool xEast = false, ySouth = false;

										if (Sextant.Format(e.Location, e.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
										{
											var location = String.Format("{0}° {1}'{2}, {3}° {4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
											from.SendMessage(location);
										}

										Book.OnTravel();

										var s = new SacredJourneySpell(from, null, e, null);

										_ = s.Cast();
									}
									else
									{
										from.SendLocalizedMessage(500015); // You do not have that spell!
									}
								}

								_ = Book.Openers.Remove(from);

								break;
							}
					}
				}
				else
				{
					_ = Book.Openers.Remove(from);
				}
			}
		}

		private class InternalPrompt : Prompt
		{
			private readonly Runebook m_Book;

			public InternalPrompt(Runebook book)
			{
				m_Book = book;
			}

			public override void OnResponse(Mobile from, string text)
			{
				if (m_Book.Deleted || !from.InRange(m_Book.GetWorldLocation(), Core.ML ? 3 : 1))
				{
					return;
				}

				if (m_Book.CheckAccess(from))
				{
					m_Book.Description = Utility.FixHtml(text.Trim());

					_ = from.CloseGump(typeof(RunebookGump));
					_ = from.SendGump(new RunebookGump(from, m_Book));

					from.SendMessage("The book's title has been changed.");
				}
				else
				{
					_ = m_Book.Openers.Remove(from);

					from.SendLocalizedMessage(502416); // That cannot be done while the book is locked down.
				}
			}

			public override void OnCancel(Mobile from)
			{
				from.SendLocalizedMessage(502415); // Request cancelled.

				if (!m_Book.Deleted && from.InRange(m_Book.GetWorldLocation(), Core.ML ? 3 : 1))
				{
					_ = from.CloseGump(typeof(RunebookGump));
					_ = from.SendGump(new RunebookGump(from, m_Book));
				}
			}
		}
	}
}