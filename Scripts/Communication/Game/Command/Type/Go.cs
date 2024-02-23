using Server.Commands.Generic;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Network;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Server.Commands
{
	public partial class CommandHandlers
	{
		private static bool FixMap(ref Map map, ref Point3D loc, Item item)
		{
			if (map == null || map == Map.Internal)
			{
				var m = item.RootParent as Mobile;

				return (m != null && FixMap(ref map, ref loc, m));
			}

			return true;
		}

		private static bool FixMap(ref Map map, ref Point3D loc, Mobile m)
		{
			if (map == null || map == Map.Internal)
			{
				map = m.LogoutMap;
				loc = m.LogoutLocation;
			}

			return (map != null && map != Map.Internal);
		}

		[Usage("Go [name | serial | (x y [z]) | (deg min (N | S) deg min (E | W))]")]
		[Description("With no arguments, this command brings up the go menu. With one argument, (name), you are moved to that regions \"go location.\" Or, if a numerical value is specified for one argument, (serial), you are moved to that object. Two or three arguments, (x y [z]), will move your character to that location. When six arguments are specified, (deg min (N | S) deg min (E | W)), your character will go to an approximate of those sextant coordinates.")]
		private static void Go_OnCommand(CommandEventArgs e)
		{
			var from = e.Mobile;

			if (e.Length == 0)
			{
				GoGump.DisplayTo(from);
				return;
			}

			if (e.Length == 1)
			{
				try
				{
					var ser = e.GetSerial(0);

					var ent = World.FindEntity(ser);

					if (ent is Item)
					{
						var item = (Item)ent;

						var map = item.Map;
						var loc = item.GetWorldLocation();

						var owner = item.RootParent as Mobile;

						if (owner != null && (owner.Map != null && owner.Map != Map.Internal) && !BaseCommand.IsAccessible(from, owner) /* !from.CanSee( owner )*/ )
						{
							from.SendMessage("You can not go to what you can not see.");
							return;
						}
						else if (owner != null && (owner.Map == null || owner.Map == Map.Internal) && owner.Hidden && owner.AccessLevel >= from.AccessLevel)
						{
							from.SendMessage("You can not go to what you can not see.");
							return;
						}
						else if (!FixMap(ref map, ref loc, item))
						{
							from.SendMessage("That is an internal item and you cannot go to it.");
							return;
						}

						from.MoveToWorld(loc, map);

						return;
					}
					else if (ent is Mobile)
					{
						var m = (Mobile)ent;

						var map = m.Map;
						var loc = m.Location;

						var owner = m;

						if (owner != null && (owner.Map != null && owner.Map != Map.Internal) && !BaseCommand.IsAccessible(from, owner) /* !from.CanSee( owner )*/ )
						{
							from.SendMessage("You can not go to what you can not see.");
							return;
						}
						else if (owner != null && (owner.Map == null || owner.Map == Map.Internal) && owner.Hidden && owner.AccessLevel >= from.AccessLevel)
						{
							from.SendMessage("You can not go to what you can not see.");
							return;
						}
						else if (!FixMap(ref map, ref loc, m))
						{
							from.SendMessage("That is an internal mobile and you cannot go to it.");
							return;
						}

						from.MoveToWorld(loc, map);

						return;
					}
					else
					{
						var name = e.GetString(0);

						foreach (var map in Map.AllMaps)
						{
							if (map.MapIndex == 0x7F || map.MapIndex == 0xFF)
							{
								continue;
							}

							if (Insensitive.Equals(name, map.Name))
							{
								from.Map = map;
								return;
							}
						}

						foreach (var r in from.Map.Regions)
						{
							if (Insensitive.Equals(r.Name, name))
							{
								from.MoveToWorld(r.GoLocation, r.Map);
								return;
							}
						}

						foreach (var map in Map.AllMaps)
						{
							if (map.MapIndex == 0x7F || map.MapIndex == 0xFF || from.Map == map)
							{
								continue;
							}

							foreach (var r in map.Regions)
							{
								if (Insensitive.Equals(r.Name, name))
								{
									if (r.Map != from.Map || !r.Contains(from))
									{
										from.MoveToWorld(r.GoLocation, r.Map);
										return;
									}
								}
							}
						}

						if (ser != 0)
						{
							from.SendMessage("No object with that serial was found.");
						}
						else
						{
							from.SendMessage("No region with that name was found.");
						}

						return;
					}
				}
				catch
				{
				}

				from.SendMessage("Region name not found");
			}
			else if (e.Length == 2 || e.Length == 3)
			{
				var map = from.Map;

				if (map != null)
				{
					try
					{
						/*
						 * This to avoid being teleported to (0,0) if trying to teleport
						 * to a region with spaces in its name.
						 */
						var x = Int32.Parse(e.GetString(0));
						var y = Int32.Parse(e.GetString(1));
						var z = (e.Length == 3) ? Int32.Parse(e.GetString(2)) : map.GetAverageZ(x, y);

						from.Location = new Point3D(x, y, z);
					}
					catch
					{
						from.SendMessage("Region name not found.");
					}
				}
			}
			else if (e.Length == 6)
			{
				var map = from.Map;

				if (map != null)
				{
					var p = Sextant.ReverseLookup(map, e.GetInt32(3), e.GetInt32(0), e.GetInt32(4), e.GetInt32(1), Insensitive.Equals(e.GetString(5), "E"), Insensitive.Equals(e.GetString(2), "S"));

					if (p != Point3D.Zero)
					{
						from.Location = p;
					}
					else
					{
						from.SendMessage("Sextant reverse lookup failed.");
					}
				}
			}
			else
			{
				from.SendMessage("Format: Go [name | serial | (x y [z]) | (deg min (N | S) deg min (E | W)]");
			}
		}
	}
}

namespace Server.Gumps
{
	public class GoGump : Gump
	{
		public static bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly bool PrevLabel = false, NextLabel = false;

		private static readonly int PrevLabelOffsetX = PrevWidth + 1;
		private static readonly int PrevLabelOffsetY = 0;

		private static readonly int NextLabelOffsetX = -29;
		private static readonly int NextLabelOffsetY = 0;

		private static readonly int EntryWidth = 180;
		private static readonly int EntryCount = 15;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (EntryCount + 1));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		public static void DisplayTo(Mobile from)
		{
			var category = WorldLocations.Global[from.Map.Name] ?? WorldLocations.Global;

			from.SendGump(new GoGump(from, category, 0));
		}

		private readonly Mobile m_Owner;
		private readonly WorldLocations.Category m_Category;
		private int m_Page;

		private readonly Dictionary<int, WorldLocations.Node> m_ButtonHandlers = new();

		public GoGump(Mobile owner)
			: this(owner, WorldLocations.Global, 0)
		{ }

		public GoGump(Mobile owner, WorldLocations.Category category, int page) : base(50, 50)
		{
			owner.CloseGump(typeof(GoGump));

			m_Owner = owner;
			m_Category = category;

			Initialize(page);
		}

		public void Initialize(int page)
		{
			m_Page = page;

			var count = Math.Clamp(m_Category.Count - (page * EntryCount), 0, EntryCount);
			var totalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (count + 1));

			AddPage(0);

			AddBackground(0, 0, BackWidth, BorderSize + totalHeight + BorderSize, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);

			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize;

			if (OldStyle)
			{
				AddImageTiled(x, y, TotalWidth - (OffsetSize * 3) - SetWidth, EntryHeight, HeaderGumpID);
			}
			else
			{
				AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);
			}

			if (m_Category.Parent != null)
			{
				AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 1, GumpButtonType.Reply, 0);

				if (PrevLabel)
				{
					AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
				}
			}

			x += PrevWidth + OffsetSize;

			var emptyWidth = TotalWidth - (PrevWidth * 2) - NextWidth - (OffsetSize * 5) - (OldStyle ? SetWidth + OffsetSize : 0);

			if (!OldStyle)
			{
				AddImageTiled(x - (OldStyle ? OffsetSize : 0), y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0), EntryHeight, EntryGumpID);
			}

			AddHtml(x + TextOffsetX, y, emptyWidth - TextOffsetX, EntryHeight, String.Format("<center>{0}</center>", m_Category.Name), false, false);

			x += emptyWidth + OffsetSize;

			if (OldStyle)
			{
				AddImageTiled(x, y, TotalWidth - (OffsetSize * 3) - SetWidth, EntryHeight, HeaderGumpID);
			}
			else
			{
				AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);
			}

			if (page > 0)
			{
				AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 2, GumpButtonType.Reply, 0);

				if (PrevLabel)
				{
					AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
				}
			}

			x += PrevWidth + OffsetSize;

			if (!OldStyle)
			{
				AddImageTiled(x, y, NextWidth, EntryHeight, HeaderGumpID);
			}

			if ((page + 1) * EntryCount < m_Category.Count)
			{
				AddButton(x + NextOffsetX, y + NextOffsetY, NextButtonID1, NextButtonID2, 3, GumpButtonType.Reply, 1);

				if (NextLabel)
				{
					AddLabel(x + NextLabelOffsetX, y + NextLabelOffsetY, TextHue, "Next");
				}
			}

			var index = -1;

			foreach (var entry in m_Category.Skip(page * EntryCount).Take(EntryCount))
			{
				++index;

				x = BorderSize + OffsetSize;
				y += EntryHeight + OffsetSize;

				AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
				AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, entry.Name);

				x += EntryWidth + OffsetSize;

				if (SetGumpID != 0)
				{
					AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
				}

				AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, index + 4, GumpButtonType.Reply, 0, entry);
			}
		}

		public void AddButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, WorldLocations.Node entry)
		{
			AddButton(x, y, normalID, pressedID, buttonID, type, param);

			m_ButtonHandlers[buttonID] = entry;
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			switch (info.ButtonID)
			{
				case 0: // Closed
				case 1: // Up
					{
						if (m_Category.Parent != null)
						{
							var index = -1;
							var search = -1;

							foreach (var node in m_Category.Parent)
							{
								++search;

								if (node == m_Category)
								{
									index = search / EntryCount;
									break;
								}
							}

							if (index < 0)
							{
								index = 0;
							}

							m_Owner.SendGump(new GoGump(m_Owner, m_Category.Parent, index));
						}

						break;
					}
				case 2: // Previous
					{
						if (m_Page > 0)
						{
							m_Owner.SendGump(new GoGump(m_Owner, m_Category, m_Page - 1));
						}

						break;
					}
				case 3: // Next
					{
						if ((m_Page + 1) * EntryCount < m_Category.Count)
						{
							m_Owner.SendGump(new GoGump(m_Owner, m_Category, m_Page + 1));
						}

						break;
					}
				default:
					{
						if (m_ButtonHandlers.TryGetValue(info.ButtonID, out var node))
						{
							if (node is WorldLocations.Category cat)
							{
								m_Owner.SendGump(new GoGump(m_Owner, cat, 0));
							}
							else if (node is WorldLocations.Entry entry)
							{
								if (entry.Location != Point3D.Zero)
								{
									Map map = null;

									var p = entry.Parent;

									while (p != null && (map == null || map == Map.Internal))
									{
										map = Map.Parse(p.Name);

										p = p.Parent;
									}

									if (map != null && map != Map.Internal)
									{
										m_Owner.MoveToWorld(entry.Location, map);
									}
									else
									{
										m_Owner.SendMessage("That is an invalid facet.");
									}
								}
								else
								{
									m_Owner.SendMessage("That is an invalid location.");
								}

								m_Owner.SendGump(new GoGump(m_Owner, m_Category, m_Page));
							}
						}
						else
						{
							m_Owner.SendGump(new GoGump(m_Owner, m_Category, m_Page));
						}

						break;
					}
			}
		}
	}
}

namespace Server.Misc
{
	public static class WorldLocations
	{
		public static Category Global { get; set; } = new("Sosaria")
		{
			#region Felucca

			new("Felucca")
			{
				#region Factions

				new("Factions")
				{
					{ "Council of Mages", 3750, 2241, 20 },
					{ "Minax", 1172, 2593, 0 },
					{ "Shadowlords", 969, 768, 0 },
					{ "True Britannians", 1419, 1622, 20 },

					#region Towns

					new("Towns")
					{
						{ "Britain", 1592, 1680, 10 },
						{ "Magincia", 3714, 2235, 20 },
						{ "Minoc", 2471, 439, 15 },
						{ "Moonglow", 4436, 1083, 0 },
						{ "Skara Brae", 576, 2200, 0 },
						{ "Trinsic", 1914, 2717, 20 },
						{ "Vesper", 2982, 818, 0 },
						{ "Yew", 548, 979, 0 },
					},

					#endregion
				},

				#endregion

				#region Dungeons

				new("Dungeons")
				{
					new("Covetous")
					{
						{ "Entrance", 2499, 919, 0 },
						{ "Level 1", 5456, 1863, 0 },
						{ "Level 2", 5614, 1997, 0 },
						{ "Level 3", 5579, 1924, 0 },
						{ "Lake Cave", 5467, 1805, 7 },
						{ "Torture Chambers", 5552, 1807, 0 },
					},
					new("Deceit")
					{
						{ "Entrance", 4111, 432, 5 },
						{ "Level 1", 5188, 638, 0 },
						{ "Level 2", 5305, 533, 2 },
						{ "Level 3", 5137, 650, 5 },
						{ "Level 4", 5306, 652, 2 },
					},
					new("Despise")
					{
						{ "Entrance", 1298, 1080, 0 },
						{ "Entryway", 5587, 631, 30 },
						{ "Level 1", 5501, 570, 59 },
						{ "Level 2", 5519, 673, 20 },
						{ "Level 3", 5407, 859, 45 },
					},
					new("Destard")
					{
						{ "Entrance", 1176, 2637, 0 },
						{ "Level 1", 5243, 1006, 0 },
						{ "Level 2", 5143, 801, 4 },
						{ "Level 3", 5137, 986, 5 },
					},
					new("Hythloth")
					{
						{ "Entrance", 4721, 3822, 0 },
						{ "Level 1", 5905, 20, 46 },
						{ "Level 2", 5976, 169, 0 },
						{ "Level 3", 6083, 145, -20 },
						{ "Level 4", 6059, 89, 24 },
					},
					new("Shame")
					{
						{ "Entrance", 514, 1561, 0 },
						{ "Level 1", 5395, 126, 0 },
						{ "Level 2", 5515, 11, 5 },
						{ "Level 3", 5514, 148, 25 },
						{ "Level 4", 5875, 20, -5 },
					},
					new("Wrong")
					{
						{ "Entrance", 2043, 238, 10 },
						{ "Level 1", 5825, 630, 0 },
						{ "Level 2", 5690, 569, 25 },
						{ "Level 3", 5703, 639, 0 },
					},
					new("Miscellaneous")
					{
						{ "Hyloth Fire Pit", 4596, 3630, 30 },
						{ "Yew-Britain Brigand Camp", 885, 1682, 0 },
						{ "Yew Fort of the Damned", 972, 768, 0 },
					},
					new("Terathan Keep")
					{
						{ "Entrance", 5451, 3143, -60 },
						{ "Level 1", 5342, 1601, 0 },
						{ "Champion Room", 5205, 1585, 0 },
						{ "Starroom", 5139, 1767, 0 },
					},
					new("Fire")
					{
						{ "Entrance", 5760, 2908, 15 },
						{ "Brit Entrance", 2923, 3407, 8 },
						{ "Level 1", 5790, 1416, 40 },
						{ "Level 2", 5702, 1316, 1 },
					},
					new("Ice")
					{
						{ "Entrance", 5210, 2322, 30 },
						{ "Brit Entrance", 1999, 81, 4 },
						{ "Level 1", 5875, 150, 15 },
						{ "Ratman Room", 5834, 327, 18 },
						{ "Ice Demon Lair", 5700, 305, 0 },
					},
					new("Orc Cave")
					{
						{ "Entrance", 1019, 1431, 0 },
						{ "Level 1", 5137, 2014, 0 },
						{ "Level 2", 5332, 1376, 0 },
						{ "Level 3", 5272, 2036, 0 },
					},
					new("Khaldun")
					{
						{ "Entrance 1", 6009, 3775, 19 },
						{ "Entrance 2", 5882, 3819, -1 },
						{ "Level 1", 5571, 1302, 0 },
					},

					#region Solen Hives

					new("Solen Hives")
					{
						{ "Central Area", 5774, 1896, 20 },

						new("Area A")
						{
							{ "Entrance", 729, 1451, 0 },
							{ "Level 1", 5658, 1795, 3 },
							{ "Level 2", 5726, 1891, 1 },
						},
						new("Area B")
						{
							{ "Entrance", 2607, 763, 0 },
							{ "Level 1", 5921, 1797, 1 },
							{ "Level 2", 5875, 1866, 2 },
						},
						new("Area C")
						{
							{ "Entrance", 1689, 2789, 0 },
							{ "Level 1", 5659, 2022, 0 },
							{ "Level 2", 5708, 1955, 0 },
						},
						new("Area D")
						{
							{ "Entrance", 1723, 814, 0 },
							{ "Level 1", 5916, 2019, 3 },
							{ "Level 2", 5813, 2012, -1 },
						},
						new("Area E")
						{
							{ "Entrance", 5732, 1858, 0 },
						},
					},

					#endregion
				},

				#endregion

				#region Internal

				new("Internal")
				{
					{ "Green Acres", 5445, 1153, 0 },

					new("Jail Cells")
					{
						{ "Cell 1", 5276, 1164, 0 },
						{ "Cell 2", 5286, 1164, 0 },
						{ "Cell 3", 5296, 1164, 0 },
						{ "Cell 4", 5306, 1164, 0 },
						{ "Cell 5", 5276, 1174, 0 },
						{ "Cell 6", 5286, 1174, 0 },
						{ "Cell 7", 5286, 1174, 0 },
						{ "Cell 8", 5306, 1174, 0 },
						{ "Cell 9", 5283, 1184, 0 },
						{ "Cell 10", 5304, 1184, 0 },
					},
				},

				#endregion

				#region Shrines

				new("Shrines")
				{
					{ "Chaos", 1456, 854, 0 },
					{ "Compassion", 1856, 872, -1 },
					{ "Honesty", 4217, 564, 36 },
					{ "Honor", 1730, 3528, 3 },
					{ "Humility", 4276, 3699, 0 },
					{ "Justice", 1301, 639, 16 },
					{ "Sacrifice", 3355, 299, 9 },
					{ "Spirituality", 1589, 2485, 5 },
					{ "Valor", 2496, 3932, 0 },
				},

				#endregion

				#region Towns

				new("Towns")
				{
					new("Britain")
					{
						{ "Blackthorn Castle", 1533, 1415, 56 },
						{ "Blackthorn Entrance", 1523, 1456, 15 },
						{ "British Castle", 1323, 1624, 55 },
						{ "British Entrance", 1401, 1625, 28 },
						{ "Cemetery", 1384, 1497, 10 },
						{ "Center", 1475, 1645, 20 },
						{ "Farmlands", 1228, 1705, 0 },
						{ "Park", 1614, 1641, 35 },
						{ "Suburbs", 1662, 1572, 5 },
					},
					new("Buccaneers Den")
					{
						{ "Bathhouse", 2667, 2084, 5 },
						{ "Docks", 2736, 2166, 0 },
						{ "Tunnels", 2667, 2069, -20 },
					},
					new("Cove")
					{
						{ "Cemetery", 2443, 1123, 5 },
						{ "Gates", 2285, 1209, 0 },
						{ "Guard Post", 2218, 1116, 19 },
						{ "Orc Fort", 2171, 1332, 0 },
					},
					new("Jhelom")
					{
						{ "Cemetery", 1296, 3719, 0 },
						{ "East Docks", 1492, 3696, -3 },
						{ "Fighting Pit", 1398, 3742, -21 },
						{ "Main Island", 1414, 3816, 0 },
						{ "Medium Island", 1124, 3623, 5 },
						{ "Small Island", 1466, 4015, 5 },
					},
					new("Magincia")
					{
						{ "Bank", 3730, 2161, 20 },
						{ "Docks", 3675, 2259, 20 },
						{ "Park", 3719, 2063, 25 },
						{ "Parliament", 3792, 2248, 20 },
					},
					new("Minoc")
					{
						{ "Bridge", 2539, 501, 30 },
						{ "Gypsy Camp", 2540, 651, 0 },
						{ "Mining Camp", 2583, 528, 15 },
						{ "North", 2475, 417, 15 },
						{ "South", 2526, 583, 0 },
					},
					new("Moonglow")
					{
						{ "Cemetery", 4546, 1338, 8 },
						{ "Center", 4442, 1122, 5 },
						{ "Docks", 4406, 1045, -2 },
						{ "Telescope", 4707, 1124, 0 },
						{ "Zoo", 4549, 1378, 8 },
					},
					new("Nujel'm")
					{
						{ "Cemetery", 3536, 1156, 20 },
						{ "Chess Board", 3728, 1360, 5 },
						{ "Docks", 3803, 1279, 5 },
						{ "East", 3755, 1227, 0 },
						{ "North", 3668, 1116, 0 },
						{ "Palace", 3698, 1279, 20 },
						{ "West", 3572, 1211, 0 },
					},
					new("Ocllo")
					{
						{ "Docks", 3650, 2653, 0 },
						{ "Farmlands", 3722, 2647, 20 },
						{ "North", 3650, 2516, 0 },
					},
					new("Serpents Hold")
					{
						{ "North", 3023, 3417, 15 },
						{ "South", 2906, 3505, 10 },
						{ "Guard Post", 3011, 3526, 15 },
					},
					new("Skara Brae")
					{
						{ "East", 811, 2243, 0 },
						{ "East Docks", 716, 2233, -3 },
						{ "North", 746, 2165, 0 },
						{ "South", 899, 2381, 0 },
						{ "West", 601, 2171, 0 },
						{ "West Docks", 639, 2236, -3 },
					},
					new("Trinsic")
					{
						{ "Center", 1927, 2779, 0 },
						{ "East Docks", 2071, 2855, -3 },
						{ "Island Park", 2108, 2793, 2 },
						{ "North", 1894, 2666, 0 },
						{ "South", 1891, 2850, 20 },
						{ "South Gate", 2000, 2930, 0 },
						{ "West Gate", 1832, 2779, 0 },
					},
					new("Vesper")
					{
						{ "Cemetery", 2786, 867, 0 },
						{ "Center", 2882, 788, 0 },
						{ "Docks", 3013, 828, -3 },
						{ "East", 2760, 981, 0 },
						{ "North", 2907, 603, 0 },
					},
					new("Wind")
					{
						{ "Caves", 5166, 244, 15 },
						{ "East", 5336, 88, 15 },
						{ "Entrance", 1362, 896, 0 },
						{ "Park", 5211, 22, 15 },
						{ "South", 5223, 189, 5 },
						{ "West", 5180, 90, 25 },
					},
					new("Yew")
					{
						{ "Cemetery", 724, 1138, 0 },
						{ "Center", 535, 992, 0 },
						{ "Courts and Prisons", 354, 836, 20 },
						{ "Empath Abbey", 635, 860, 0 },
						{ "Hidden Cave", 313, 787, -24 },
						{ "Orc Fort", 633, 1499, 0 },
					},
					new("Delucia")
					{
						{ "Watch Tower", 5276, 3945, 37 },
						{ "Center", 5228, 3978, 37 },
						{ "Orc Fort", 5210, 3636, 3 },
					},
					new("Papua")
					{
						{ "The Just Inn", 5769, 3176, 0 },
						{ "Center", 5730, 3208, -4 },
						{ "Docks", 5825, 3256, 2 },
					},
				},

				#endregion
			},

			#endregion

			#region Trammel

			new("Trammel")
			{
				#region Dungeons

				new("Dungeons")
				{
					new("Covetous")
					{
						{ "Entrance", 2499, 919, 0 },
						{ "Level 1", 5456, 1863, 0 },
						{ "Level 2", 5614, 1997, 0 },
						{ "Level 3", 5579, 1924, 0 },
						{ "Lake Cave", 5467, 1805, 7 },
						{ "Torture Chambers", 5552, 1807, 0 },
					},
					new("Deceit")
					{
						{ "Entrance", 4111, 432, 5 },
						{ "Level 1", 5188, 638, 0 },
						{ "Level 2", 5305, 533, 2 },
						{ "Level 3", 5137, 650, 5 },
						{ "Level 4", 5306, 652, 2 },
					},
					new("Despise")
					{
						{ "Entrance", 1298, 1080, 0 },
						{ "Entryway", 5587, 631, 30 },
						{ "Level 1", 5501, 570, 59 },
						{ "Level 2", 5519, 673, 20 },
						{ "Level 3", 5407, 859, 45 },
					},
					new("Destard")
					{
						{ "Entrance", 1176, 2637, 0 },
						{ "Level 1", 5243, 1006, 0 },
						{ "Level 2", 5143, 801, 4 },
						{ "Level 3", 5137, 986, 5 },
					},
					new("Hythloth")
					{
						{ "Entrance", 4721, 3822, 0 },
						{ "Level 1", 5905, 20, 46 },
						{ "Level 2", 5976, 169, 0 },
						{ "Level 3", 6083, 145, -20 },
						{ "Level 4", 6059, 89, 24 },
					},
					new("Shame")
					{
						{ "Entrance", 514, 1561, 0 },
						{ "Level 1", 5395, 126, 0 },
						{ "Level 2", 5515, 11, 5 },
						{ "Level 3", 5514, 148, 25 },
						{ "Level 4", 5875, 20, -5 },
					},
					new("Wrong")
					{
						{ "Entrance", 2043, 238, 10 },
						{ "Level 1", 5825, 630, 0 },
						{ "Level 2", 5690, 569, 25 },
						{ "Level 3", 5703, 639, 0 },
					},
					new("Miscellaneous")
					{
						{ "Hyloth Fire Pit", 4596, 3630, 30 },
						{ "Yew-Britain Brigand Camp", 885, 1682, 0 },
						{ "Yew Fort of the Damned", 972, 768, 0 },
					},
					new("Terathan Keep")
					{
						{ "Entrance", 5451, 3143, -60 },
						{ "Level 1", 5342, 1601, 0 },
						{ "Champion Room", 5205, 1585, 0 },
						{ "Starroom", 5139, 1767, 0 },
					},
					new("Fire")
					{
						{ "Entrance", 5760, 2908, 15 },
						{ "Brit Entrance", 2923, 3407, 8 },
						{ "Level 1", 5790, 1416, 40 },
						{ "Level 2", 5702, 1316, 1 },
					},
					new("Ice")
					{
						{ "Entrance", 5210, 2322, 30 },
						{ "Brit Entrance", 1999, 81, 4 },
						{ "Level 1", 5875, 150, 15 },
						{ "Ratman Room", 5834, 327, 18 },
						{ "Ice Demon Lair", 5700, 305, 0 },
					},
					new("Orc Cave")
					{
						{ "Entrance", 1019, 1431, 0 },
						{ "Level 1", 5137, 2014, 0 },
						{ "Level 2", 5332, 1376, 0 },
						{ "Level 3", 5272, 2036, 0 },
					},

					#region Solen Hives

					new("Solen Hives")
					{
						{ "Central Area", 5774, 1896, 20 },

						new("Area A")
						{
							{ "Entrance", 729, 1451, 0 },
							{ "Level 1", 5658, 1795, 3 },
							{ "Level 2", 5726, 1891, 1 },
						},
						new("Area B")
						{
							{ "Entrance", 2607, 763, 0 },
							{ "Level 1", 5921, 1797, 1 },
							{ "Level 2", 5875, 1866, 2 },
						},
						new("Area C")
						{
							{ "Entrance", 1689, 2789, 0 },
							{ "Level 1", 5659, 2022, 0 },
							{ "Level 2", 5708, 1955, 0 },
						},
						new("Area D")
						{
							{ "Entrance", 1723, 814, 0 },
							{ "Level 1", 5916, 2019, 3 },
							{ "Level 2", 5813, 2012, -1 },
						},
						new("Area E")
						{
							{ "Entrance", 5732, 1858, 0 },
						},
					},

					#endregion
				},

				#endregion

				#region Internal

				new("Internal")
				{
					{ "Green Acres", 5445, 1153, 0 },

					new("Jail Cells")
					{
						{ "Cell 1", 5276, 1164, 0 },
						{ "Cell 2", 5286, 1164, 0 },
						{ "Cell 3", 5296, 1164, 0 },
						{ "Cell 4", 5306, 1164, 0 },
						{ "Cell 5", 5276, 1174, 0 },
						{ "Cell 6", 5286, 1174, 0 },
						{ "Cell 7", 5286, 1174, 0 },
						{ "Cell 8", 5306, 1174, 0 },
						{ "Cell 9", 5283, 1184, 0 },
						{ "Cell 10", 5304, 1184, 0 },
					},
				},

				#endregion

				#region Shrines

				new("Shrines")
				{
					{ "Chaos", 1456, 854, 0 },
					{ "Compassion", 1856, 872, -1 },
					{ "Honesty", 4217, 564, 36 },
					{ "Honor", 1730, 3528, 3 },
					{ "Humility", 4276, 3699, 0 },
					{ "Justice", 1301, 639, 16 },
					{ "Sacrifice", 3355, 299, 9 },
					{ "Spirituality", 1589, 2485, 5 },
					{ "Valor", 2496, 3932, 0 },
				},

				#endregion

				#region Towns

				new("Towns")
				{
					new("Britain")
					{
						{ "Blackthorn Castle", 1533, 1415, 56 },
						{ "Blackthorn Entrance", 1523, 1456, 15 },
						{ "British Castle", 1323, 1624, 55 },
						{ "British Entrance", 1401, 1625, 28 },
						{ "Cemetery", 1384, 1497, 10 },
						{ "Center", 1475, 1645, 20 },
						{ "Farmlands", 1228, 1705, 0 },
						{ "Park", 1614, 1641, 35 },
						{ "Suburbs", 1662, 1572, 5 },
					},
					new("Buccaneers Den")
					{
						{ "Bathhouse", 2667, 2084, 5 },
						{ "Docks", 2736, 2166, 0 },
						{ "Tunnels", 2667, 2069, -20 },
					},
					new("Cove")
					{
						{ "Cemetery", 2443, 1123, 5 },
						{ "Gates", 2285, 1209, 0 },
						{ "Guard Post", 2218, 1116, 19 },
						{ "Orc Fort", 2171, 1332, 0 },
					},
					new("Jhelom")
					{
						{ "Cemetery", 1296, 3719, 0 },
						{ "East Docks", 1492, 3696, -3 },
						{ "Fighting Pit", 1398, 3742, -21 },
						{ "Main Island", 1414, 3816, 0 },
						{ "Medium Island", 1124, 3623, 5 },
						{ "Small Island", 1466, 4015, 5 },
					},
					new("Magincia")
					{
						{ "Bank", 3730, 2161, 20 },
						{ "Docks", 3675, 2259, 20 },
						{ "Park", 3719, 2063, 25 },
						{ "Parliament", 3792, 2248, 20 },
					},
					new("Minoc")
					{
						{ "Bridge", 2539, 501, 30 },
						{ "Gypsy Camp", 2540, 651, 0 },
						{ "Mining Camp", 2583, 528, 15 },
						{ "North", 2475, 417, 15 },
						{ "South", 2526, 583, 0 },
					},
					new("Moonglow")
					{
						{ "Cemetery", 4546, 1338, 8 },
						{ "Center", 4442, 1122, 5 },
						{ "Docks", 4406, 1045, -2 },
						{ "Telescope", 4707, 1124, 0 },
						{ "Zoo", 4549, 1378, 8 },
					},
					new("Nujel'm")
					{
						{ "Cemetery", 3536, 1156, 20 },
						{ "Chess Board", 3728, 1360, 5 },
						{ "Docks", 3803, 1279, 5 },
						{ "East", 3755, 1227, 0 },
						{ "North", 3668, 1116, 0 },
						{ "Palace", 3698, 1279, 20 },
						{ "West", 3572, 1211, 0 },
					},
					new("Haven") // TODO: Update
					{
						{ "Docks", 3650, 2653, 0 },
						{ "Farmlands", 3722, 2647, 20 },
						{ "North", 3650, 2516, 0 },
					},
					new("Serpents Hold")
					{
						{ "North", 3023, 3417, 15 },
						{ "South", 2906, 3505, 10 },
						{ "Guard Post", 3011, 3526, 15 },
					},
					new("Skara Brae")
					{
						{ "East", 811, 2243, 0 },
						{ "East Docks", 716, 2233, -3 },
						{ "North", 746, 2165, 0 },
						{ "South", 899, 2381, 0 },
						{ "West", 601, 2171, 0 },
						{ "West Docks", 639, 2236, -3 },
					},
					new("Trinsic")
					{
						{ "Center", 1927, 2779, 0 },
						{ "East Docks", 2071, 2855, -3 },
						{ "Island Park", 2108, 2793, 2 },
						{ "North", 1894, 2666, 0 },
						{ "South", 1891, 2850, 20 },
						{ "South Gate", 2000, 2930, 0 },
						{ "West Gate", 1832, 2779, 0 },
					},
					new("Vesper")
					{
						{ "Cemetery", 2786, 867, 0 },
						{ "Center", 2882, 788, 0 },
						{ "Docks", 3013, 828, -3 },
						{ "East", 2760, 981, 0 },
						{ "North", 2907, 603, 0 },
					},
					new("Wind")
					{
						{ "Caves", 5166, 244, 15 },
						{ "East", 5336, 88, 15 },
						{ "Entrance", 1362, 896, 0 },
						{ "Park", 5211, 22, 15 },
						{ "South", 5223, 189, 5 },
						{ "West", 5180, 90, 25 },
					},
					new("Yew")
					{
						{ "Cemetery", 724, 1138, 0 },
						{ "Center", 535, 992, 0 },
						{ "Courts and Prisons", 354, 836, 20 },
						{ "Empath Abbey", 635, 860, 0 },
						{ "Hidden Cave", 313, 787, -24 },
						{ "Orc Fort", 633, 1499, 0 },
					},
					new("Delucia")
					{
						{ "Watch Tower", 5276, 3945, 37 },
						{ "Center", 5228, 3978, 37 },
						{ "Orc Fort", 5210, 3636, 3 },
					},
					new("Papua")
					{
						{ "The Just Inn", 5769, 3176, 0 },
						{ "Center", 5730, 3208, -4 },
						{ "Docks", 5825, 3256, 2 },
					},
				},

				#endregion
			},

			#endregion

			#region Ilshenar

			new("Ilshenar")
			{
				#region Cities

				new("Cities")
				{
					new("Ancient Citadel")
					{
						{ "Entrance", 1518, 568, -14 },
					},
					new("Gargoyle City")
					{
						{ "Bank", 852, 602, -40 },
						{ "Central Area", 836, 641, -20 },
					},
					new("Lakeshire")
					{
						{ "Central Area", 1203, 1124, -25 },
					},
					new("Mistas")
					{
						{ "Central Area", 820, 1073, -30 },
					},
					new("Montor")
					{
						{ "Central Area", 1643, 310, 48 },
					},
					new("Req Volon")
					{
						{ "Central Area", 1362, 1073, -13 },
					},
					new("Savage Camp")
					{
						{ "Central Area", 1251, 743, -80 },
					},
					new("Terort Skitas")
					{
						{ "Entrance", 567, 437, 21 },
					},
				},

				#endregion

				#region Dungeons

				new("Dungeons")
				{
					new("Ankh")
					{
						{ "Entrance", 576, 1150, -100 },
						{ "Level 1", 155, 1482, -28 },
						{ "Kirin passage", 11, 874, -28 },
						{ "Serpentine Passage", 396, 1578, -28 },
					},
					new("Blood")
					{
						{ "Entrance", 1747, 1228, -1 },
						{ "Level 1", 2114, 834, -28 },
					},
					new("Exodus")
					{
						{ "Entrance", 835, 777, -80 },
						{ "Level 1", 1974, 115, -28 },
					},
					new("Rock")
					{
						{ "Entrance", 1788, 573, 70 },
						{ "Level 1", 2188, 318, -7 },
						{ "Level 2", 2188, 28, -27 },
					},
					new("Sorcerers")
					{
						{ "Entrance", 548, 462, -53 },
						{ "Level 1", 428, 109, -28 },
						{ "Level 2", 240, 23, -18 },
						{ "Level 3", 272, 136, -28 },
						{ "Level 4", 132, 124, -28 },
						{ "Level 5", 236, 121, -28 },
					},
					new("Spectre")
					{
						{ "Entrance", 1363, 1033, -8 },
						{ "Level 1", 1982, 1103, -28 },
					},
					new("Wisp")
					{
						{ "Entrance", 651, 1302, -58 },
						{ "Level 1", 627, 1525, -28 },
						{ "Level 2", 718, 1492, -28 },
						{ "Level 3", 832, 1551, -28 },
						{ "Level 4", 960, 1428, -28 },
						{ "Level 5", 942, 1465, -33 },
						{ "Level 6", 866, 1434, -28 },
						{ "Level 7", 807, 1548, -28 },
						{ "Level 8", 754, 1479, -28 },
					},
				},

				#endregion

				#region Caves

				new("Caves")
				{
					new("Ancient Lair")
					{
						{ "Entrance", 940, 504, -30 },
						{ "Level 1", 85, 746, -28 },
					},
					new("Lizard Passage")
					{
						{ "Entrance", 314, 1334, -36 },
						{ "Level 1", 329, 1589, -18 },
						{ "Level 2", 227, 1337, -18 },
					},
					new("Mushroom Cave")
					{
						{ "Entrance", 1459, 1329, -24 },
					},
					new("Rat Cave")
					{
						{ "Entrance", 1034, 1153, -23 },
						{ "Level 1", 1348, 1510, -3 },
						{ "Level 2", 1247, 1511, -28 },
					},
					new("Spider Cave")
					{
						{ "Entrance", 1421, 913, -19 },
						{ "Level 1", 1786, 991, -28 },
						{ "Level 2", 1517, 877, 10 },
						{ "Ethereal Keep", 1365, 1102, -26 },
					},
				},

				#endregion

				#region Shrines

				new("Shrines")
				{
					{ "Compassion", 1217, 469, -13 },
					{ "Honesty", 720, 1356, -60 },
					{ "Honor", 748, 728, -29 },
					{ "Humility", 287, 1016, 0 },
					{ "Justice", 987, 1007, -35 },
					{ "Sacrifice", 1175, 1287, -30 },
					{ "Spirituality", 1532, 1341, -3 },
					{ "Valor", 527, 218, -44 },
				},

				#endregion
			},

			#endregion

			#region Malas

			new("Malas")
			{
				#region Towns

				new("Towns")
				{
					new("Luna")
					{
						{ "Luna Moongate", 1015, 527, -65 },
						{ "Luna Bank", 991, 519, -50 },
						{ "Clothier's Colors", 976, 527, -50 },
						{ "Grand Arena", 961, 638, -90 },
						{ "Hardwoods and More", 1002, 512, -50 },
						{ "Paladin's Stopover", 1005, 519, -30 },
						{ "Proud Bridle", 1023, 503, -70 },
						{ "Open Market", 992, 512, -50 },
						{ "Sage's Refuge", 1002, 527, -50 },
						{ "Shining Blades", 976, 512, -50 },
						{ "Shrine of Wisdom", 976, 510, -30 },
						{ "Vault of Secrets", 1072, 457, -90 },
					},
					new("Umbra")
					{
						{ "Umbra Moongate", 1998, 1387, -85 },
						{ "Umbra Bridge", 1941, 1321, -88 },
						{ "Armoury of Souls", 1984, 1367, -80 },
						{ "Bloodletter's Guild", 2084, 1372, -75 },
						{ "Critter Pens", 1992, 1326, -90 },
						{ "Darkweave", 2079, 1338, -81 },
						{ "Ghast Refectory", 2017, 1354, -90 },
						{ "Gravedigger's Apparatus", 2016, 1328, -80 },
						{ "Lich's Hoard", 2051, 1398, -90 },
						{ "Necromancer Ampitheater", 2024, 1383, -80 },
						{ "Skeleton Swill", 2030, 1350, -90 },
						{ "Wailing Banshee Inn", 2039, 1317, -84 },
						{ "Warped Woodworks", 2066, 1288, -80 },
					},
				},

				#endregion

				#region Dungeons

				new("Dungeons")
				{
					new("Doom")
					{
						{ "Entrance", 2367, 1268, -85 },
						{ "Tunnel", 2352, 1267, -110 },
						{ "Inside", 381, 133, 33 },
						{ "Gauntlet", 433, 331, -2 },
						{ "Guardian's Room", 365, 15, -1 },
						{ "Lamp Room", 470, 95, -1 },
					},
				},

				#endregion

				#region Sites

				new("Sites")
				{
					#region Points Of Interest

					new("Points of Interest")
					{
						{ "Broken Mountains", 1111, 1461, -90 },
						{ "Corrupted Forest", 2172, 1144, -87 },
						{ "Crumbling Continent", 736, 1180, -95 },
						{ "Crystal Fens", 1388, 616, -85 },
						{ "Divide of the Abyss", 1545, 877, -85 },
						{ "Dry Highlands", 2128, 1668, -90 },
						{ "Forgotten Pyramid", 1825, 1799, -50 },
						{ "Gravewater Lake", 1596, 1656, -115 },
						{ "Grimswind Ruins", 2192, 330, -90 },
						{ "Northern Crags", 1580, 384, -50 },
						{ "Orc Fortress", 1340, 1226, -90 },
						{ "Hanse's Hostel", 1065, 1435, -90 },
					},

					#endregion

					#region Caves & Mines

					new("Caves & Mines")
					{
						{ "Mine 1", 2082, 574, -90 },
						{ "Mine 2", 2157, 456, -90 },
						{ "Mine 3", 1811, 359, -90 },
						{ "Mine 4", 1790, 475, -90 },
						{ "Mine 5", 1628, 432, -86 },
						{ "Mine 6", 1658, 304, -90 },
						{ "Mine 7", 1071, 247, -90 },
						{ "Mine 8", 1095, 197, -90 },
						{ "Mine 9", 1195, 511, -90 },
					},

					#endregion

					#region Orc Forts

					new("Orc Forts")
					{
						{ "Fort 1", 912, 193, -79 },
						{ "Fort 2", 1666, 362, -50 },
						{ "Fort 3", 1364, 599, -86 },
						{ "Fort 4", 1205, 705, -88 },
						{ "Fort 5", 1257, 1331, -90 },
						{ "Fort 6", 1598, 1825, -110 },
					},

					#endregion
				},

				#endregion
			},

			#endregion

			#region Tokuno

			new("Tokuno Islands")
			{
				#region Towns

				new("Towns")
				{
					new("Zento")
					{
						{ "Zento Moongate", 802, 1204, 25 },
						{ "Zento Bank", 735, 1255, 30 },
					},
				},

				#endregion

				#region Dungeons

				new("Dungeons")
				{
					new("Fan Dancer's Dojo")
					{
						{ "Entrance", 977, 218, 23 },
					},
					new("Yomotsu Mines")
					{
						{ "Entrance", 259, 785, 64 },
					},
				},

				#endregion

				#region Sites

				new("Sites")
				{
					new("Makoto-Jima")
					{
						{ "Moongate", 802, 1204, 25 },
						{ "Shrine", 718, 1161, 25 },
						{ "The Waste", 724, 943, 42 },
					},
					new("Isamu-Jima")
					{
						{ "Moongate", 1169, 998, 41 },
						{ "Shrine", 1047, 512, 15 },
						{ "Lotus Lakes", 1066, 817, 20 },
						{ "Mount Sho", 1159, 696, 70 },
						{ "Dragon Valley", 948, 432, 8 },
						{ "Winter Spur", 926, 162, 34 },
					},
					new("Homare-Jima")
					{
						{ "Moongate", 270, 628, 15 },
						{ "Shrine", 295, 712, 55 },
						{ "Field of Echoes", 160, 654, 36 },
						{ "Crane Marsh", 232, 1107, 10 },
						{ "Bushido Dojo", 334, 407, 32 },
						{ "Kitsune Woods", 523, 520, 32 },
					},
				},

				#endregion
			},

			#endregion

			#region TerMur

			new("TerMur")
			{
				#region Towns

				new("Towns")
				{
					new("Royal City")
					{
						{ "Royal City Moongate", 855, 3526, -43 },
					},
					new("Holy City")
					{
						{ "Holy City Moongate", 928, 3989, -40 },
					},
					new("Dugan")
					{
						{ "Dugan", 1107, 1131, -52 },
					},
				},

				#endregion

				#region Dungeons

				new("Dungeons")
				{
					new("Tomb of Kings")
					{
						{ "Entrance", 997, 3843, -41 },
						{ "Gate to Stygian Abyss", 38, 29, 0 },
					},
					new("Stygian Abyss")
					{
						{ "Exit to Tomb of Kings", 946, 71, 72 },
						{ "Exit to Underworld", 521, 921, 36 },
						{ "Abyssal Lair Entrance", 985, 366, -11 },
						{ "Cavern of the Discarded", 912, 501, -12 },
						{ "Clan Scratch", 959, 548, -14 },
						{ "Crimson Veins", 974, 164, -11 },
						{ "Enslaved Goblins", 581, 815, -45 },
						{ "Fairy Dragon Lair", 888, 277, 3 },
						{ "Fire Temple Ruins", 519, 765, -92 },
						{ "Hydra", 793, 753, 53 },
						{ "Lands of the Lich", 538, 656, 8 },
						{ "Lava Caldera", 587, 895, -73 },
						{ "Medusa's Lair", 818, 755, 50 },
						{ "Passage of Tears", 685, 579, -15 },
						{ "Secret Garden", 462, 719, 22 },
						{ "Serpent Lair", 711, 720, -11 },
						{ "Silver Sapling", 341, 619, 26 },
						{ "Stygian Dragon Lair Entrance", 851, 279, -12 },
						{ "Sutek the Mage", 924, 595, -14 },
					},
					new("Underworld")
					{
						{ "Entrance", 1128, 1211, -2 },
					},
				},

				#endregion

				#region Sites

				new("Sites")
				{
					{ "Atoll Bend", 1118, 3408, -42 },
					{ "Chicken Chase", 560, 3412, 37 },
					{ "City Residential", 952, 3531, -43 },
					{ "Coral Desert", 1047, 2980, 62 },
					{ "Fishermans Reach", 631, 3035, 36 },
					{ "Gated Isle", 703, 3934, -31 },
					{ "High Plain", 863, 2931, 38 },
					{ "Holy City Island", 996, 3905, -42 },
					{ "Kepetch Waste", 447, 3188, 20 },
					{ "Lava Lake", 407, 3010, -24 },
					{ "Lavapit Pyramid", 740, 3691, -43 },
					{ "Lost Settlement", 526, 3822, -44 },
					{ "Northern Steppes", 822, 3063, 61 },
					{ "Raptor Island", 816, 3778, -42 },
					{ "Royal Park", 711, 3255, 38 },
					{ "Shrine of Singularity", 995, 3814, -23 },
					{ "Slith Valley", 1078, 3331, -42 },
					{ "Spider Island", 1115, 3730, -42 },
					{ "Spiders Guarde", 1109, 3606, -45 },
					{ "Talon Point", 676, 3831, -39 },
					{ "Treefellow Course", 570, 2936, 39 },
					{ "Void Isle", 440, 3577, 38 },
					{ "Walled Circus", 370, 3273, 0 },
					{ "Waterfall Point", 661, 2894, 39 },
				},

				#endregion
			},

			#endregion
		};

		public static Category Felucca => Global["Felucca"];
		public static Category Trammel => Global["Trammel"];
		public static Category Ilshenar => Global["Ilshenar"];
		public static Category Malas => Global["Malas"];
		public static Category Tokuno => Global["Tokuno"];
		public static Category TerMur => Global["TerMur"];

		public abstract class Node : IComparable<Node>
		{
			public Category Parent { get; protected set; }

			public string Name { get; protected set; }

			public Node(string name) 
				: this(null, name)
			{
			}

			public Node(Category parent, string name)
			{
				Parent = parent;
				Name = name;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Name);
			}

			public int CompareTo(Node other)
			{
				return Insensitive.Compare(Name, other?.Name);
			}
		}

		public sealed class Category : Node, IEnumerable<Node>
		{
			public SortedSet<Category> Nodes { get; } = new(Comparer.Instance);
			public SortedSet<Entry> Objects { get; } = new(Comparer.Instance);

			public int NodeCount => Nodes.Count;
			public int ObjectCount => Objects.Count;

			public int Count => NodeCount + ObjectCount;

			public Category this[string name]
			{
				get
				{
					foreach (var n in Nodes)
					{
						if (Insensitive.Equals(n.Name, name))
						{
							return n;
						}
					}

					return null;
				}
			}

			public Category(string name)
				: base(name)
			{ }

			public Category(Category parent, string name)
				: base(parent, name)
			{ }

			public void Add(Category child)
			{
				if (Nodes.Add(child))
				{
					child.Parent = this;
				}
			}

			public void Add(string name, int x, int y, int z)
			{
				Objects.Add(new Entry(this, name, x, y, z));
			}

			public IEnumerator<Node> GetEnumerator()
			{
				foreach (var node in Nodes)
				{
					yield return node;
				}

				foreach (var entry in Objects)
				{
					yield return entry;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public class Entry : Node
		{
			public Point3D Location { get; }

			public Entry(Category parent, string name, int x, int y, int z)
				: base(parent, name)
			{
				Location = new Point3D(x, y, z);
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Location, Name);
			}
		}

		public sealed class Comparer : IComparer<Node>
		{
			public static Comparer Instance { get; } = new();

			private Comparer()
			{ }

			public int Compare(Node x, Node y)
			{
				return Insensitive.Compare(x?.Name, y?.Name);
			}
		}
	}
}