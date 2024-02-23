using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using Server.Targeting;

using System;
using System.Collections;

namespace Server.Items
{
	public class HousePlacementTool : Item
	{
		public override int LabelNumber => 1060651;  // a house placement tool

		[Constructable]
		public HousePlacementTool() : base(0x14F6)
		{
			Weight = 3.0;
			LootType = LootType.Blessed;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack))
			{
				_ = from.SendGump(new HousePlacementCategoryGump(from));
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public HousePlacementTool(Serial serial) : base(serial)
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
			_ = reader.ReadInt();

			if (Weight == 0.0)
			{
				Weight = 3.0;
			}
		}
	}

	public class HousePlacementCategoryGump : Gump
	{
		private readonly Mobile m_From;

		private const int LabelColor = 0x7FFF;
		private const int LabelColorDisabled = 0x4210;

		public HousePlacementCategoryGump(Mobile from) : base(50, 50)
		{
			m_From = from;

			_ = from.CloseGump(typeof(HousePlacementCategoryGump));
			_ = from.CloseGump(typeof(HousePlacementListGump));

			AddPage(0);

			AddBackground(0, 0, 270, 145, 5054);

			AddImageTiled(10, 10, 250, 125, 2624);
			AddAlphaRegion(10, 10, 250, 125);

			AddHtmlLocalized(10, 10, 250, 20, 1060239, LabelColor, false, false); // <CENTER>HOUSE PLACEMENT TOOL</CENTER>

			AddButton(10, 110, 4017, 4019, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 110, 150, 20, 3000363, LabelColor, false, false); // Close

			AddButton(10, 40, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 40, 200, 20, 1060390, LabelColor, false, false); // Classic Houses

			AddButton(10, 60, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 60, 200, 20, 1060391, LabelColor, false, false); // 2-Story Customizable Houses

			AddButton(10, 80, 4005, 4007, 3, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 80, 200, 20, 1060392, LabelColor, false, false); // 3-Story Customizable Houses
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_From.CheckAlive() || m_From.Backpack == null || m_From.Backpack.FindItemByType(typeof(HousePlacementTool)) == null)
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Classic Houses
					{
						_ = m_From.SendGump(new HousePlacementListGump(m_From, HousePlacementEntry.ClassicHouses));
						break;
					}
				case 2: // 2-Story Customizable Houses
					{
						_ = m_From.SendGump(new HousePlacementListGump(m_From, HousePlacementEntry.TwoStoryFoundations));
						break;
					}
				case 3: // 3-Story Customizable Houses
					{
						_ = m_From.SendGump(new HousePlacementListGump(m_From, HousePlacementEntry.ThreeStoryFoundations));
						break;
					}
			}
		}
	}

	public class HousePlacementListGump : Gump
	{
		private readonly Mobile m_From;
		private readonly HousePlacementEntry[] m_Entries;

		private const int LabelColor = 0x7FFF;
		private const int LabelHue = 0x480;

		public HousePlacementListGump(Mobile from, HousePlacementEntry[] entries) : base(50, 50)
		{
			m_From = from;
			m_Entries = entries;

			_ = from.CloseGump(typeof(HousePlacementCategoryGump));
			_ = from.CloseGump(typeof(HousePlacementListGump));

			AddPage(0);

			AddBackground(0, 0, 520, 420, 5054);

			AddImageTiled(10, 10, 500, 20, 2624);
			AddAlphaRegion(10, 10, 500, 20);

			AddHtmlLocalized(10, 10, 500, 20, 1060239, LabelColor, false, false); // <CENTER>HOUSE PLACEMENT TOOL</CENTER>

			AddImageTiled(10, 40, 500, 20, 2624);
			AddAlphaRegion(10, 40, 500, 20);

			AddHtmlLocalized(50, 40, 225, 20, 1060235, LabelColor, false, false); // House Description
			AddHtmlLocalized(275, 40, 75, 20, 1060236, LabelColor, false, false); // Storage
			AddHtmlLocalized(350, 40, 75, 20, 1060237, LabelColor, false, false); // Lockdowns
			AddHtmlLocalized(425, 40, 75, 20, 1060034, LabelColor, false, false); // Cost

			AddImageTiled(10, 70, 500, 280, 2624);
			AddAlphaRegion(10, 70, 500, 280);

			AddImageTiled(10, 360, 500, 20, 2624);
			AddAlphaRegion(10, 360, 500, 20);

			AddHtmlLocalized(10, 360, 250, 20, 1060645, LabelColor, false, false); // Bank Balance:
			AddLabel(250, 360, LabelHue, Banker.GetBalance(from).ToString());

			AddImageTiled(10, 390, 500, 20, 2624);
			AddAlphaRegion(10, 390, 500, 20);

			AddButton(10, 390, 4017, 4019, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(50, 390, 100, 20, 3000363, LabelColor, false, false); // Close

			for (var i = 0; i < entries.Length; ++i)
			{
				var page = 1 + (i / 14);
				var index = i % 14;

				if (index == 0)
				{
					if (page > 1)
					{
						AddButton(450, 390, 4005, 4007, 0, GumpButtonType.Page, page);
						AddHtmlLocalized(400, 390, 100, 20, 3000406, LabelColor, false, false); // Next
					}

					AddPage(page);

					if (page > 1)
					{
						AddButton(200, 390, 4014, 4016, 0, GumpButtonType.Page, page - 1);
						AddHtmlLocalized(250, 390, 100, 20, 3000405, LabelColor, false, false); // Previous
					}
				}

				var entry = entries[i];

				var y = 70 + (index * 20);

				AddButton(10, y, 4005, 4007, 1 + i, GumpButtonType.Reply, 0);
				AddHtmlLocalized(50, y, 225, 20, entry.Description, LabelColor, false, false);
				AddLabel(275, y, LabelHue, entry.Storage.ToString());
				AddLabel(350, y, LabelHue, entry.Lockdowns.ToString());
				AddLabel(425, y, LabelHue, entry.Cost.ToString());
			}
		}

		public override void OnResponse(Server.Network.NetState sender, RelayInfo info)
		{
			if (!m_From.CheckAlive() || m_From.Backpack == null || m_From.Backpack.FindItemByType(typeof(HousePlacementTool)) == null)
			{
				return;
			}

			var index = info.ButtonID - 1;

			if (index >= 0 && index < m_Entries.Length)
			{
				if (m_From.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse(m_From))
				{
					m_From.SendLocalizedMessage(501271); // You already own a house, you may not place another!
				}
				else
				{
					m_From.Target = new NewHousePlacementTarget(m_Entries, m_Entries[index]);
				}
			}
			else
			{
				_ = m_From.SendGump(new HousePlacementCategoryGump(m_From));
			}
		}
	}

	public class NewHousePlacementTarget : MultiTarget
	{
		private readonly HousePlacementEntry m_Entry;
		private readonly HousePlacementEntry[] m_Entries;

		private bool m_Placed;

		public NewHousePlacementTarget(HousePlacementEntry[] entries, HousePlacementEntry entry) : base(entry.MultiID, entry.Offset)
		{
			Range = 14;

			m_Entries = entries;
			m_Entry = entry;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (!from.CheckAlive() || from.Backpack == null || from.Backpack.FindItemByType(typeof(HousePlacementTool)) == null)
			{
				return;
			}

			if (o is IPoint3D ip)
			{
				if (ip is Item item)
				{
					ip = item.GetWorldTop();
				}

				var p = new Point3D(ip);

				var reg = Region.Find(p, from.Map);

				if (reg.IsPartOf(typeof(TempNoHousingRegion)))
				{
					from.SendLocalizedMessage(501270); // Lord British has decreed a 'no build' period, thus you cannot build this house at this time.
				}
				else if (reg.IsPartOf(typeof(TreasureRegion)) || reg.IsPartOf(typeof(HouseRegion)))
				{
					from.SendLocalizedMessage(1043287); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
				}
				else if (reg.IsPartOf(typeof(HouseRaffleRegion)))
				{
					from.SendLocalizedMessage(1150493); // You must have a deed for this plot of land in order to build here.
				}
				else if (!reg.AllowHousing(from, p))
				{
					from.SendLocalizedMessage(501265); // Housing can not be created in this area.
				}
				else
				{
					m_Placed = m_Entry.OnPlacement(from, p);
				}
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			if (!from.CheckAlive() || from.Backpack == null || from.Backpack.FindItemByType(typeof(HousePlacementTool)) == null)
			{
				return;
			}

			if (!m_Placed)
			{
				_ = from.SendGump(new HousePlacementListGump(from, m_Entries));
			}
		}
	}

	public partial class HousePlacementEntry
	{
		private readonly int m_Storage;
		private readonly int m_Lockdowns;
		private readonly int m_NewStorage;
		private readonly int m_NewLockdowns;
		private Point3D m_Offset;

		public Type Type { get; }

		public int Description { get; }
		public int Storage => BaseHouse.NewVendorSystem ? m_NewStorage : m_Storage;
		public int Lockdowns => BaseHouse.NewVendorSystem ? m_NewLockdowns : m_Lockdowns;
		public int Vendors { get; }
		public int Cost { get; }

		public int MultiID { get; }
		public Point3D Offset => m_Offset;

		public HousePlacementEntry(Type type, int description, int storage, int lockdowns, int newStorage, int newLockdowns, int vendors, int cost, int xOffset, int yOffset, int zOffset, int multiID)
		{
			Type = type;
			Description = description;
			m_Storage = storage;
			m_Lockdowns = lockdowns;
			m_NewStorage = newStorage;
			m_NewLockdowns = newLockdowns;
			Vendors = vendors;
			Cost = cost;

			m_Offset = new Point3D(xOffset, yOffset, zOffset);

			MultiID = multiID;
		}

		public BaseHouse ConstructHouse(Mobile from)
		{
			try
			{
				object[] args;

				if (Type == typeof(HouseFoundation))
				{
					args = new object[4] { from, MultiID, m_Storage, m_Lockdowns };
				}
				else if (Type == typeof(SmallOldHouse) || Type == typeof(SmallShop) || Type == typeof(TwoStoryHouse))
				{
					args = new object[2] { from, MultiID };
				}
				else
				{
					args = new object[1] { from };
				}

				return Activator.CreateInstance(Type, args) as BaseHouse;
			}
			catch
			{
			}

			return null;
		}

		public void PlacementWarning_Callback(Mobile from, bool okay, object state)
		{
			if (!from.CheckAlive() || from.Backpack == null || from.Backpack.FindItemByType(typeof(HousePlacementTool)) == null)
			{
				return;
			}

			var prevHouse = (PreviewHouse)state;

			if (!okay)
			{
				prevHouse.Delete();
				return;
			}

			if (prevHouse.Deleted)
			{
				/* Too much time has passed and the test house you created has been deleted.
				 * Please try again!
				 */
				_ = from.SendGump(new NoticeGump(1060637, 30720, 1060647, 32512, 320, 180, null, null));

				return;
			}

			var center = prevHouse.Location;

			prevHouse.Delete();

			//Point3D center = new Point3D( p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z );
			var res = HousePlacement.Check(from, MultiID, center, out var toMove);

			switch (res)
			{
				case HousePlacementResult.Valid:
					{
						if (from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse(from))
						{
							from.SendLocalizedMessage(501271); // You already own a house, you may not place another!
							break;
						}

						var house = ConstructHouse(from);

						if (house == null)
						{
							break;
						}

						house.Price = Cost;

						if (from.AccessLevel >= AccessLevel.GameMaster)
						{
							from.SendMessage("{0} gold would have been withdrawn from your bank if you were not a GM.", Cost.ToString());
						}
						else if (Banker.Withdraw(from, Cost))
						{
							from.SendLocalizedMessage(1060398, Cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
						}
						else
						{
							house.RemoveKeys(from);
							house.Delete();

							from.SendLocalizedMessage(1060646); // You do not have the funds available in your bank box to purchase this house.  Try placing a smaller house, or adding gold or checks to your bank box.

							break;
						}

						house.MoveToWorld(center, from.Map);

						if (toMove?.Count > 0)
						{
							foreach (var o in toMove)
							{
								o.Location = house.BanLocation;
							}
						}

						break;
					}
				case HousePlacementResult.BadItem:
				case HousePlacementResult.BadLand:
				case HousePlacementResult.BadStatic:
				case HousePlacementResult.BadRegionHidden:
				case HousePlacementResult.NoSurface:
					{
						from.SendLocalizedMessage(1043287); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
						break;
					}
				case HousePlacementResult.BadRegion:
					{
						from.SendLocalizedMessage(501265); // Housing cannot be created in this area.
						break;
					}
				case HousePlacementResult.BadRegionTemp:
					{
						from.SendLocalizedMessage(501270); // Lord British has decreed a 'no build' period, thus you cannot build this house at this time.
						break;
					}
				case HousePlacementResult.BadRegionRaffle:
					{
						from.SendLocalizedMessage(1150493); // You must have a deed for this plot of land in order to build here.
						break;
					}
				case HousePlacementResult.InvalidCastleKeep:
					{
						from.SendLocalizedMessage(1061122); // Castles and keeps cannot be created here.
						break;
					}
			}
		}

		public bool OnPlacement(Mobile from, Point3D p)
		{
			if (!from.CheckAlive() || from.Backpack == null || from.Backpack.FindItemByType(typeof(HousePlacementTool)) == null)
			{
				return false;
			}

			var center = new Point3D(p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z);
			var res = HousePlacement.Check(from, MultiID, center, out var toMove);

			switch (res)
			{
				case HousePlacementResult.Valid:
					{
						if (from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse(from))
						{
							from.SendLocalizedMessage(501271); // You already own a house, you may not place another!
							return false;
						}

						from.SendLocalizedMessage(1011576); // This is a valid location.

						var prev = new PreviewHouse(MultiID);

						var mcl = prev.Components;

						var banLoc = new Point3D(center.X + mcl.Min.X, center.Y + mcl.Max.Y + 1, center.Z);

						for (var i = 0; i < mcl.List.Length; ++i)
						{
							var entry = mcl.List[i];

							int itemID = entry.ItemID;

							if (itemID is >= 0xBA3 and <= 0xC0E)
							{
								banLoc = new Point3D(center.X + entry.OffsetX, center.Y + entry.OffsetY, center.Z);
								break;
							}
						}

						if (toMove?.Count > 0)
						{
							foreach (var o in toMove)
							{
								o.Location = banLoc;
							}
						}

						prev.MoveToWorld(center, from.Map);

						/* You are about to place a new house.
						 * Placing this house will condemn any and all of your other houses that you may have.
						 * All of your houses on all shards will be affected.
						 * 
						 * In addition, you will not be able to place another house or have one transferred to you for one (1) real-life week.
						 * 
						 * Once you accept these terms, these effects cannot be reversed.
						 * Re-deeding or transferring your new house will not uncondemn your other house(s) nor will the one week timer be removed.
						 * 
						 * If you are absolutely certain you wish to proceed, click the button next to OKAY below.
						 * If you do not wish to trade for this house, click CANCEL.
						 */
						_ = from.SendGump(new WarningGump(1060635, 30720, 1049583, 32512, 420, 280, new WarningGumpCallback(PlacementWarning_Callback), prev));

						return true;
					}
				case HousePlacementResult.BadItem:
				case HousePlacementResult.BadLand:
				case HousePlacementResult.BadStatic:
				case HousePlacementResult.BadRegionHidden:
					{
						from.SendLocalizedMessage(1043287); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
						return false;
					}
				case HousePlacementResult.NoSurface:
					{
						from.SendMessage("The house could not be created here.  Part of the foundation would not be on any surface.");
						return false;
					}
				case HousePlacementResult.BadRegion:
					{
						from.SendLocalizedMessage(501265); // Housing cannot be created in this area.
						return false;
					}
				case HousePlacementResult.BadRegionTemp:
					{
						from.SendLocalizedMessage(501270); //Lord British has decreed a 'no build' period, thus you cannot build this house at this time.
						return false;
					}
				case HousePlacementResult.BadRegionRaffle:
					{
						from.SendLocalizedMessage(1150493); // You must have a deed for this plot of land in order to build here.
						return false;
					}
				case HousePlacementResult.InvalidCastleKeep:
					{
						from.SendLocalizedMessage(1061122); // Castles and keeps cannot be created here.
						return false;
					}
			}

			return false;
		}

		private static readonly Hashtable m_Table;

		static HousePlacementEntry()
		{
			m_Table = new Hashtable();

			FillTable(m_ClassicHouses);
			FillTable(m_TwoStoryFoundations);
			FillTable(m_ThreeStoryFoundations);
		}

		public static HousePlacementEntry Find(BaseHouse house)
		{
			var obj = m_Table[house.GetType()];

			if (obj is HousePlacementEntry)
			{
				return (HousePlacementEntry)obj;
			}
			else if (obj is ArrayList list)
			{
				for (var i = 0; i < list.Count; ++i)
				{
					var e = (HousePlacementEntry)list[i];

					if (e.MultiID == house.ItemID)
					{
						return e;
					}
				}
			}
			else if (obj is Hashtable table)
			{
				obj = table[house.ItemID];

				if (obj is HousePlacementEntry)
				{
					return (HousePlacementEntry)obj;
				}
			}

			return null;
		}

		private static void FillTable(HousePlacementEntry[] entries)
		{
			for (var i = 0; i < entries.Length; ++i)
			{
				var e = entries[i];

				var obj = m_Table[e.Type];

				if (obj == null)
				{
					m_Table[e.Type] = e;
				}
				else if (obj is HousePlacementEntry)
				{
					var list = new ArrayList {
						obj,
						e
					};

					m_Table[e.Type] = list;
				}
				else if (obj is ArrayList list)
				{
					if (list.Count == 8)
					{
						var table = new Hashtable();

						for (var j = 0; j < list.Count; ++j)
						{
							table[((HousePlacementEntry)list[j]).MultiID] = list[j];
						}

						table[e.MultiID] = e;

						m_Table[e.Type] = table;
					}
					else
					{
						_ = list.Add(e);
					}
				}
				else if (obj is Hashtable)
				{
					((Hashtable)obj)[e.MultiID] = e;
				}
			}
		}
	}
}