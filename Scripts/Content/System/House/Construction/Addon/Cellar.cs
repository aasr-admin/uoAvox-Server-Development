#region Header
//   Voxpire     _,-'/-'/
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2022  ` -'. -'
#endregion

#region References
using Server.Commands;
using Server.ContextMenus;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Targeting;

using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Server.Items
{
	// Initially separated by blocks of 100 to leave insertion spaces.
	// After adding a new entry, create the style info for it below and register it like the others.
	public enum CellarStyle
	{
		None = 0,
		Grass = 100,
		Garden = 101,
		Flagstone = 200,
		SandFlagstone = 201,
		Stone = 300,
		DarkStone = 301,
		SandStone = 302,
		Marble = 400,
		Brick = 401,
		Timber = 500,
		Crystal = 600,
		Jade = 601,
		Dungeon = 700,
		Blood = 701,
	}

	public static class CellarStyles
	{
		public static List<CellarStyleInfo> Styles { get; } = new();

		public static CellarStyleInfo GetInfo(this CellarStyle style)
		{
			return Styles.FirstOrDefault(s => s.Style == style);
		}

		static CellarStyles()
		{
			var grass = new CellarStyleInfo(CellarStyle.Grass, "Grass");
			grass.SetFloor(6013, 6014, 6015, 6016, 6017);
			grass.SetWall(3215, 3216, 3217, 3218);
			grass.SetStairs(17621, 2213);
			Styles.Add(grass);

			var garden = new CellarStyleInfo(CellarStyle.Garden, "Garden");
			garden.SetFloor(12788, 12789, 12790, 12791, 12792, 12793, 12794, 12795);
			garden.SetWall(3215, 3216, 3217, 3218);
			garden.SetStairs(17621, 2212);
			Styles.Add(garden);

			var flags = new CellarStyleInfo(CellarStyle.Flagstone, "Flagstone");
			flags.SetFloor(1276, 1277, 1278, 1279);
			flags.SetWall(1872);
			flags.SetStairs(17621, 1873);
			Styles.Add(flags);

			var sandFlags = new CellarStyleInfo(CellarStyle.SandFlagstone, "Sand Flagstone");
			sandFlags.SetFloor(1327, 1328, 1329, 1330);
			sandFlags.SetWall(1900);
			sandFlags.SetStairs(17621, 1901);
			Styles.Add(sandFlags);

			var stone = new CellarStyleInfo(CellarStyle.Stone, "Stone");
			stone.SetFloor(1305, 1306, 1307, 1308);
			stone.SetWall(1822);
			stone.SetStairs(17621, 1823);
			Styles.Add(stone);

			var darkStone = new CellarStyleInfo(CellarStyle.DarkStone, "Dark Stone");
			darkStone.SetFloor(1313, 1314, 1315, 1316);
			darkStone.SetWall(1928);
			darkStone.SetStairs(17621, 1929);
			Styles.Add(darkStone);

			var sand = new CellarStyleInfo(CellarStyle.SandStone, "Sand Stone");
			sand.SetFloor(1181, 1182, 1183, 1184);
			sand.SetWall(1900);
			sand.SetStairs(17621, 1901);
			Styles.Add(sand);

			var marble = new CellarStyleInfo(CellarStyle.Marble, "Marble");
			marble.SetFloor(1297, 1298, 1299, 1300);
			marble.SetWall(1801);
			marble.SetStairs(17621, 1802);
			Styles.Add(marble);

			var brick = new CellarStyleInfo(CellarStyle.Brick, "Brick");
			brick.SetFloor(1250, 1251, 1252, 1253, 1254, 1255, 1256, 1257);
			brick.SetWall(1822);
			brick.SetStairs(17621, 1823);
			Styles.Add(brick);

			var timber = new CellarStyleInfo(CellarStyle.Timber, "Timber");
			timber.SetFloor(1193, 1194, 1195, 1196);
			timber.SetWall(1848);
			timber.SetStairs(17621, 1849);
			Styles.Add(timber);

			var crystal = new CellarStyleInfo(CellarStyle.Crystal, "Crystal");
			crystal.SetFloor(13751, 13752, 13753, 13754, 13755, 13756, 13757);
			crystal.SetWall(13778);
			crystal.SetStairs(17621, 13780);
			Styles.Add(crystal);

			var jade = new CellarStyleInfo(CellarStyle.Jade, "Jade");
			jade.SetFloor(16815, 16815, 16816, 16817);
			jade.SetWall(19207);
			jade.SetStairs(17621, 19205);
			Styles.Add(jade);

			var dungeon = new CellarStyleInfo(CellarStyle.Dungeon, "Dungeon");
			dungeon.SetFloor(1339, 1340, 1341, 1342, 1343);
			dungeon.SetWall(1955);
			dungeon.SetStairs(17621, 1956);
			Styles.Add(dungeon);

			var blood = new CellarStyleInfo(CellarStyle.Blood, "Blood");
			blood.SetFloor(2760);
			blood.SetWall(8700);
			blood.SetStairs(17621, 1979);
			Styles.Add(blood);
		}
	}

	public sealed class CellarStyleInfo
	{
		public CellarStyle Style { get; private set; }
		public string Name { get; private set; }

		public int[] FloorTiles { get; set; }
		public int[] WallTiles { get; set; }

		public int StairsDown { get; set; }
		public int StairsUp { get; set; }

		public CellarStyleInfo(CellarStyle style, string name)
		{
			Style = style;
			Name = name;

			FloorTiles = Array.Empty<int>();
			WallTiles = Array.Empty<int>();
		}

		public void SetStairs(int down, int up)
		{
			StairsDown = down;
			StairsUp = up;
		}

		public void SetFloor(params int[] itemIDs)
		{
			FloorTiles = itemIDs ?? Array.Empty<int>();
		}

		public void SetWall(params int[] itemIDs)
		{
			WallTiles = itemIDs ?? Array.Empty<int>();
		}
	}

	public interface ICellarComponent : IEntity
	{
		Point3D Offset { get; set; }
	}

	public sealed class CellarStairs : AddonComponent, ICellarComponent, ISecurable
	{
		private SecureLevel _Level = SecureLevel.Anyone;

		[CommandProperty(AccessLevel.Counselor, true)]
		public CellarStairs Link { get; set; }

		[CommandProperty(AccessLevel.Counselor)]
		public SecureLevel Level
		{
			get
			{
				if (Link != null && _Level != Link._Level)
				{
					_Level = Link._Level;
				}

				return _Level;
			}
			set
			{
				if (_Level == value)
				{
					return;
				}

				_Level = value;

				if (Link != null)
				{
					Link._Level = _Level;
				}
			}
		}

		public CellarStairs(int itemID)
			: base(itemID)
		{
			Movable = false;
		}

		public CellarStairs(Serial serial)
			: base(serial)
		{
		}

		public void LinkWith(CellarStairs other)
		{
			if (Link == other || other == null || other == this)
			{
				return;
			}

			other.Link = this;
			Link = other;
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			SetSecureLevelEntry.AddTo(from, this, list);
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (Link == null || Link.Deleted || Link.Map == null || Link.Map == Map.Internal || Link.Location == Point3D.Zero)
			{
				return;
			}

			if (m.Region.IsPartOf(Region.Find(GetWorldLocation(), Map)) && Utility.CheckUse(this, m, true, true, 3))
			{
				m.MoveToWorld(Link.Location, Link.Map);
			}
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (Link == null || Link.Deleted || Link.Map == null || Link.Map == Map.Internal || Link.Location == Point3D.Zero || m == null || m.Deleted)
			{
				return false;
			}

			var house = BaseHouse.FindHouseAt(this);

			if (house == null || house.CheckSecureAccess(m, this) == SecureAccessResult.Inaccessible)
			{
				return false;
			}

			m.MoveToWorld(Link.Location, Link.Map);

			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(_Level);
			writer.Write(Link);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();

			_Level = reader.ReadEnum<SecureLevel>();

			var link = reader.ReadItem<CellarStairs>();

			if (Link == null || Link.Deleted)
			{
				Link = link;
			}
		}
	}

	public sealed class CellarFloor : AddonComponent, ICellarComponent
	{
		public CellarFloor(int itemID)
			: base(itemID)
		{
			Name = "Cellar Floor";
			Movable = false;
		}

		public CellarFloor(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();
		}
	}

	public sealed class CellarWall : AddonComponent, ICellarComponent
	{
		public CellarWall(int itemID)
			: base(itemID)
		{
			Name = "Cellar Wall";
			Movable = false;
		}

		public CellarWall(Serial serial)
			: base(serial)
		{
		}

		public override bool OnMoveOver(Mobile m)
		{
			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();
		}
	}

	public class CellarAddon : BaseAddon
	{
		protected sealed class CellarPlaceholder : AddonComponent, ICellarComponent
		{
			public CellarPlaceholder(int itemID)
				: base(itemID)
			{
				Name = "Cellar Placeholder";
				Movable = false;
			}

			public CellarPlaceholder(Serial serial)
				: base(serial)
			{ }

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				_ = reader.ReadInt();
			}
		}

		public static List<CellarAddon> Cellars { get; } = new();

		public static void Configure()
		{
			EventSink.ServerStarted += () =>
			{
				var i = Cellars.Count;

				while (--i >= 0)
				{
					Cellars[i]?.InvalidateComponents();
				}
			};

			CommandSystem.Register("Cellar", AccessLevel.Counselor, e =>
			{
				var house = BaseHouse.FindHouseAt(e.Mobile);

				if (house == null)
				{
					e.Mobile.SendMessage("You must be inside a house to use that command.");
					return;
				}

				var cellar = house.Addons.OfType<CellarAddon>().FirstOrDefault();

				if (cellar == null)
				{
					e.Mobile.SendMessage("This house does not have a cellar.");
					return;
				}

				e.Mobile.SendGump(new PropertiesGump(e.Mobile, cellar));
			});
		}

		public static bool CanPlace(Mobile m, Point3D p, out BaseHouse house, bool message = true)
		{
			house = null;

			if (m == null || p == Point3D.Zero)
			{
				return false;
			}

			var r = m.Region.GetRegion<HouseRegion>();

			if (r == null || r.House == null || r.House.Deleted || r.House.Map == null || r.House.Map == Map.Internal)
			{
				if (message)
				{
					m.SendMessage(0x22, "You must be inside a house to build a cellar.");
				}

				return false;
			}

			if (!r.Contains(p))
			{
				if (message)
				{
					m.SendMessage(0x22, "You may only build a cellar inside a house.");
				}

				return false;
			}

			house = r.House;

			if (house.Addons.Any(a => a is CellarAddon))
			{
				if (message)
				{
					m.SendMessage(0x22, "There is already a cellar in this house.");
				}

				return false;
			}

			// 10z allows for the ground floor tile height, house foundation surface tops are typically z + 7
			if (Math.Abs(house.Z - p.Z) > 10)
			{
				if (message)
				{
					m.SendMessage(0x22, "You may only build a cellar on the first floor of a house.");
				}

				return false;
			}

			if (m.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}

			if (!house.IsOwner(m))
			{
				if (message)
				{
					m.SendMessage(0x22, "Only the house owner may build a cellar.");
				}

				return false;
			}

			if (!house.Map.CanFit(p.X, p.Y, p.Z, 20, true, false, true))
			{
				if (message)
				{
					m.SendMessage(0x22, "The cellar can't be built here because something is blocking the location.");
				}

				return false;
			}

			return true;
		}

		public override BaseAddonDeed Deed => new CellarDeed(_Style);

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public CellarStyle Style
		{
			get => _Style;
			set
			{
				if (_Style == value)
				{
					return;
				}

				_Style = value;

				if (_Style != CellarStyle.None)
				{
					UpdateComponents();
				}
			}
		}

		public void UpdateComponents()
		{
			var info = _Style.GetInfo();

			if (info == null)
			{
				return;
			}

			_ = _FloorTiles.RemoveAll(o => o?.Deleted != false);
			_ = _WallTiles.RemoveAll(o => o?.Deleted != false);

			if (_FloorTiles.Count == 0 || _WallTiles.Count == 0)
			{
				House ??= BaseHouse.FindHouseAt(_Placeholder as Item ?? this);

				if (House != null)
				{
					ClearComponents(true);
					GenerateComponents();
				}
			}

			foreach (var s in _FloorTiles)
			{
				s.ItemID = Utility.RandomList(info.FloorTiles);
			}

			foreach (var s in _WallTiles)
			{
				s.ItemID = Utility.RandomList(info.WallTiles);
			}

			if (StairsDown != null)
			{
				StairsDown.ItemID = info.StairsDown;
			}

			if (StairsUp != null)
			{
				StairsUp.ItemID = info.StairsUp;
			}
		}

		[CommandProperty(AccessLevel.Counselor, true)]
		public BaseHouse House { get; private set; }

		[CommandProperty(AccessLevel.Counselor, true)]
		public CellarStairs StairsDown { get; private set; }

		[CommandProperty(AccessLevel.Counselor, true)]
		public CellarStairs StairsUp { get; private set; }

		private CellarPlaceholder _Placeholder;

		private List<CellarFloor> _FloorTiles;
		private List<CellarWall> _WallTiles;

		private CellarStyle _Style;

		private bool _Chopping;

		[Constructable]
		public CellarAddon()
			: this(CellarStyle.Garden)
		{ }

		[Constructable]
		public CellarAddon(CellarStyle style)
		{
			_Style = style;

			_FloorTiles = new List<CellarFloor>();
			_WallTiles = new List<CellarWall>();

			_Placeholder = AddComponent(new CellarPlaceholder(0xF39), Point3D.Zero);

			Cellars.Add(this);
		}

		public CellarAddon(Serial serial)
			: base(serial)
		{
			Cellars.Add(this);
		}

		public override void OnChop(Mobile m)
		{
			if (House == null || House.Deleted)
			{
				House = BaseHouse.FindHouseAt(this);
			}

			if (House == null || House.Deleted || _Chopping || m is not PlayerMobile pm)
			{
				return;
			}

			if (!House.IsOwner(m))
			{
				m.SendMessage(0x22, "Only the house owner may demolish the cellar.");
				return;
			}

			_Chopping = true;

			var where = House is HouseFoundation ? "the moving crate" : "the cellar entrance";

			var text = $"Demolishing this cellar will cause all contents to be moved to {where}.\n\n"
					 + $"Confirm cellar demolision?";

			DemolishConfirmGump g = null;

			g = new DemolishConfirmGump(text, (u, s, o) =>
			{
				_Chopping = false;

				if (s)
				{
					Demolish(u);
				}

				g = null;
			});

			_ = m.SendGump(g);

			_ = Timer.DelayCall(TimeSpan.FromSeconds(60.0), () =>
			{
				if (g == null)
				{
					return;
				}

				var ns = m.NetState;

				if (ns == null || !ns.Gumps.Contains(g))
				{
					return;
				}

				m.SendMessage(0x22, "Your time to select an option has expired and the demolition has been cancelled.");

				ns.Send(new CloseGump(g.TypeID, 0));

				ns.RemoveGump(g);

				g.OnServerClose(ns);

				_Chopping = false;
			});
		}

		public override void Delete()
		{
			Demolish(null);
		}

		public override void OnDelete()
		{
			base.OnDelete();

			_ = Cellars.Remove(this);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			_ = Cellars.Remove(this);
		}

		public void Demolish(Mobile m)
		{
			if (StairsDown == null || StairsUp == null)
			{
				return;
			}

			if (House == null || House.Deleted)
			{
				House = BaseHouse.FindHouseAt(this);
			}

			var loc = StairsDown.Location;

			if (House != null)
			{
				var map = House.Map;

				var entities = new HashSet<IEntity>();

				foreach (var r in House.Region.Area)
				{
					var eable = map.GetObjectsInBounds(r.Bounds);

					foreach (var e in eable)
					{
						if (e.Z >= House.Z - 20)
						{
							continue;
						}

						if (e == House || e == House.Sign || e == House.MovingCrate)
						{
							continue;
						}

						if (e == StairsDown || e == StairsUp || e == _Placeholder)
						{
							continue;
						}

						if (e is ICellarComponent || Components.Exists(c => c == e))
						{
							continue;
						}

						if (r.Contains(e.X, e.Y))
						{
							_ = entities.Add(e);
						}
					}

					eable.Free();
				}

				foreach (var ea in entities.OfType<IAddon>().ToArray())
				{
					var d = ea.Deed;

					if (d != null)
					{
						_ = entities.Add(d);
					}

					ea.Delete();
				}

				foreach (var e in entities)
				{
					if (e is Mobile em)
					{
						em.MoveToWorld(House.BanLocation, map);
					}
					else if (e is Item ei)
					{
						if (ei.Deleted || (!ei.Movable && !ei.IsLockedDown && !ei.IsSecure))
						{
							continue;
						}

						if (m != null)
						{
							if (ei.IsSecure)
							{
								House.ReleaseSecure(m, ei);
							}

							if (ei.IsLockedDown)
							{
								House.Release(m, ei);
							}
						}
						else
						{
							if (ei.IsSecure)
							{
								ei.IsSecure = ei.IsLockedDown = false;
								ei.Movable = ei is not BaseAddonContainer;
								ei.SetLastMoved();

								_ = House.Secures.RemoveWhere(si => si == null || si.Item == ei);
							}

							if (ei.IsLockedDown)
							{
								ei.IsLockedDown = false;
								ei.SetLastMoved();

								_ = House.LockDowns.Remove(ei);
							}
						}

						if (House is HouseFoundation hf)
						{
							hf.DropToMovingCrate(ei);
						}
						else
						{
							ei.MoveToWorld(loc, map);
						}
					}
				}
			}

			ClearComponents(false);

			if (m != null)
			{
				base.OnChop(m);
			}
			else
			{
				base.Delete();
			}

			_ = House?.Addons.Remove(this);

			House = null;
		}

		public override void OnMapChange(Map oldMap)
		{
			base.OnMapChange(oldMap);

			if (!World.Loading && Map != null && Map != Map.Internal)
			{
				GenerateComponents();
			}
		}

		public override void OnLocationChange(Point3D oldLoc)
		{
			base.OnLocationChange(oldLoc);

			if (!World.Loading && Map != null && Map != Map.Internal)
			{
				GenerateComponents();
			}
		}

		public void ClearComponents(bool holdPlace)
		{
			var comp = Components.Where(c => c != null && !c.Deleted).ToArray();

			_FloorTiles.Clear();
			_FloorTiles.TrimExcess();

			_WallTiles.Clear();
			_WallTiles.TrimExcess();

			Components.Clear();
			Components.TrimExcess();

			foreach (var c in comp)
			{
				c.Addon = null;
				c.Delete();
			}

			if (holdPlace)
			{
				_Placeholder ??= AddComponent(new CellarPlaceholder(0xF39), Point3D.Zero);
			}
		}

		protected TComponent AddComponent<TComponent>(TComponent c, Point3D o) where TComponent : AddonComponent
		{
			AddComponent(c, o.X, o.Y, o.Z);
			return c;
		}

		public void GenerateComponents()
		{
			if (Deleted || Map == null || Map == Map.Internal || Location == Point3D.Zero)
			{
				return;
			}

			if (House == null || House.Deleted)
			{
				House = BaseHouse.FindHouseAt(this);
			}

			if (House == null || House.Deleted || House.Area == null || House.Area.Length == 0)
			{
				House = null;
				return;
			}

			if (_Placeholder == null || _Placeholder.Deleted)
			{
				House = null;
				return;
			}

			if (_Style == CellarStyle.None)
			{
				_Style = CellarStyle.Garden;
			}

			var p = _Placeholder.Location;
			var o = _Placeholder.Offset;

			_ = Components.Remove(_Placeholder);

			_Placeholder.Addon = null;
			_Placeholder.Delete();
			_Placeholder = null;

			var style = Style.GetInfo();

			if (style != null)
			{
				StairsDown = AddComponent(new CellarStairs(style.StairsDown), o);

				var floorPoints = new HashSet<Point2D>();
				var wallPoints = new HashSet<Point2D>();

				var pAbs = Point2D.Zero;
				var pRel = Point2D.Zero;

				foreach (var r in House.Region.Area)
				{
					var b = r.Bounds;

					(pAbs.X, pAbs.Y, pRel.X, pRel.Y) = (0, 0, 0, 0);

					for (pAbs.X = b.Left; pAbs.X <= b.Right; pAbs.X++)
					{
						for (pAbs.Y = b.Top; pAbs.Y <= b.Bottom; pAbs.Y++)
						{
							if (r.Contains(pAbs.X, pAbs.Y))
							{
								(pRel.X, pRel.Y) = (pAbs.X - p.X, pAbs.Y - p.Y);

								_ = floorPoints.Add(pRel);
							}
						}
					}
				}

				int x = 0, y = 0;

				foreach (var pFloor in floorPoints)
				{
					for (var d = 0; d < 8; d++)
					{
						(x, y) = (pFloor.X, pFloor.Y);

						Movement.Movement.Offset((Direction)d, ref x, ref y);

						(pRel.X, pRel.Y) = (x, y);

						if (!floorPoints.Contains(pRel))
						{
							_ = wallPoints.Add(pFloor);

							break;
						}
					}
				}

				var z = Region.MinZ + 10 - p.Z;

				foreach (var loc in floorPoints)
				{
					_FloorTiles.Add(AddComponent(new CellarFloor(Utility.RandomList(style.FloorTiles)), new Point3D(loc, z)));
				}

				foreach (var loc in wallPoints)
				{
					_WallTiles.Add(AddComponent(new CellarWall(Utility.RandomList(style.WallTiles)), new Point3D(loc, z)));
				}

				StairsUp = AddComponent(new CellarStairs(style.StairsUp), new Point3D(0, 0, z));

				StairsDown.LinkWith(StairsUp);
			}

			_ = House.Addons.Add(this);
		}

		private void InvalidateComponents()
		{
			if (Deleted || Map == null || Map == Map.Internal)
			{
				return;
			}

			if (StairsDown == null || StairsUp == null)
			{
				var stairs = Components.OfType<CellarStairs>().OrderByDescending(s => s.Z).ToArray();

				if (stairs.Length > 1)
				{
					StairsDown = stairs.First();
					StairsUp = stairs.Last();

					StairsDown.Link = StairsUp;
					StairsUp.Link = StairsDown;
				}
			}
			else if (StairsDown.Link == null || StairsUp.Link == null)
			{
				StairsDown.Link = StairsUp;
				StairsUp.Link = StairsDown;
			}

			if (_Style == CellarStyle.None)
			{
				_Style = CellarStyle.Garden;
			}

			UpdateComponents();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(_Placeholder);
			writer.WriteItemList(_FloorTiles);
			writer.WriteItemList(_WallTiles);
			writer.Write(_Style);
			writer.Write(House);
			writer.Write(StairsDown);
			writer.Write(StairsUp);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();

			_Placeholder = reader.ReadItem<CellarPlaceholder>();
			_FloorTiles = reader.ReadStrongItemList<CellarFloor>();
			_WallTiles = reader.ReadStrongItemList<CellarWall>();
			_Style = reader.ReadEnum<CellarStyle>();
			House = reader.ReadItem<BaseHouse>();
			StairsDown = reader.ReadItem<CellarStairs>();
			StairsUp = reader.ReadItem<CellarStairs>();
		}

		private class DemolishConfirmGump : WarningGump
		{
			public DemolishConfirmGump(string text, WarningGumpCallback callback)
				: base(1060635, 0x7F00, text, 0xFFFFFF, 420, 280, callback, null)
			{
			}
		}
	}

	public class CellarDeed : BaseAddonDeed
	{
		private CellarStyle _Style;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public CellarStyle Style
		{
			get => _Style;
			set
			{
				_Style = value;

				InvalidateProperties();
			}
		}

		public override BaseAddon Addon => new CellarAddon(Style);

		[Constructable]
		public CellarDeed()
			: this(CellarStyle.None)
		{ }

		[Constructable]
		public CellarDeed(CellarStyle style)
		{
			_Style = style;

			Name = "Cellar Deed";
			LootType = LootType.Blessed;
		}

		public CellarDeed(Serial serial)
			: base(serial)
		{ }

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Style != CellarStyle.None)
			{
				list.Add($"[Style: {CellarStyles.GetInfo(Style)?.Name ?? Utility.FriendlyName(Style)}]");
			}
		}

		public override void OnSingleClick(Mobile m)
		{
			base.OnSingleClick(m);

			if (Style != CellarStyle.None)
			{
				LabelTo(m, $"[Style: {CellarStyles.GetInfo(Style)?.Name ?? Utility.FriendlyName(Style)}]");
			}
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (!Utility.CheckUse(this, m, true, false, 2, true) || m is not PlayerMobile pm)
			{
				return;
			}

			if (Style == CellarStyle.None)
			{
				_ = pm.SendGump(new CellarStyleGump(pm, this, 0, CellarStyles.Styles));
			}
			else
			{
				BeginTarget(m, Style);
			}
		}

		public void BeginTarget(Mobile m, CellarStyle style)
		{
			if (!Utility.CheckUse(this, m, false, false, 2, true))
			{
				return;
			}

			m.SendMessage("Pick a place for your cellar stairs.");

			_ = m.BeginTarget(-1, true, TargetFlags.None, (u, t, s) =>
			{
				if (t is IPoint3D p)
				{
					EndTarget(u, s, p);
				}
			}, style);
		}

		public void EndTarget(Mobile m, CellarStyle style, IPoint3D p)
		{
			if (!Utility.CheckUse(this, m, false, false, 2, true))
			{
				return;
			}

			var loc = new Point3D(p);

			if (!CellarAddon.CanPlace(m, loc, out var house))
			{
				return;
			}

			Style = style;

			var addon = Addon;

			addon.MoveToWorld(loc, m.Map);

			if (house?.Deleted != false)
			{
				/*
				if (house is TownHouse th)
				{
					if (th.ForSaleSign != null)
					{
						var z = addon.Components.Min(c => c.Z - 1);

						if (th.ForSaleSign.MinZ > z)
						{
							th.ForSaleSign.MinZ = z;
						}
					}
				}
				*/
			}

			Delete();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(_Style);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();

			_Style = reader.ReadEnum<CellarStyle>();
		}
	}

	public sealed class CellarStyleGump : BaseGridGump
	{
		private const int EntriesPerPage = 5;

		private readonly PlayerMobile m_Player;

		private readonly CellarDeed m_Deed;

		private readonly int m_Page;

		private readonly List<CellarStyleInfo> m_Styles;

		public override int BorderSize => 10;
		public override int OffsetSize => 1;

		private readonly bool m_BuildingList;

		public override int EntryHeight => m_BuildingList ? 64 : 20;

		public override int OffsetGumpID => 0x2430;
		public override int HeaderGumpID => 0x243A;
		public override int EntryGumpID => 0x2458;
		public override int BackGumpID => 0x2486;

		public override int TextHue => 0;
		public override int TextOffsetX => 2;

		public const int PageLeftID1 = 0x25EA;
		public const int PageLeftID2 = 0x25EB;
		public const int PageLeftWidth = 16;
		public const int PageLeftHeight = 16;

		public const int PageRightID1 = 0x25E6;
		public const int PageRightID2 = 0x25E7;
		public const int PageRightWidth = 16;
		public const int PageRightHeight = 16;

		public CellarStyleGump(PlayerMobile player, CellarDeed deed, int page, List<CellarStyleInfo> styles)
			: base(100, 100)
		{
			m_Player = player;
			m_Deed = deed;
			m_Page = page;
			m_Styles = styles;

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = true;

			m_BuildingList = true;

			var nameWidth = 100;

			var artWidth = EntryHeight;
			var artTotalWidth = (artWidth + OffsetSize) * 3;

			m_BuildingList = false;

			AddNewPage();

			AddEntryHtml(20 + nameWidth + artTotalWidth + 20, SetCenter("Cellar Style Selection"));

			AddNewLine();

			if (m_Page > 0)
			{
				AddEntryButton(20, PageLeftID1, PageLeftID2, 1, PageLeftWidth, PageLeftHeight);
			}
			else
			{
				AddEntryHeader(20);
			}

			AddEntryHtml(nameWidth + artTotalWidth - OffsetSize, SetCenter($"Page {m_Page + 1} of {(m_Styles.Count + EntriesPerPage - 1) / EntriesPerPage}"));

			if ((m_Page + 1) * EntriesPerPage < m_Styles.Count)
			{
				AddEntryButton(20, PageRightID1, PageRightID2, 2, PageRightWidth, PageRightHeight);
			}
			else
			{
				AddEntryHeader(20);
			}

			for (int i = m_Page * EntriesPerPage, line = 0; line < EntriesPerPage && i < m_Styles.Count; ++i, ++line)
			{
				if (line == 0)
				{
					AddNewLine();

					m_BuildingList = true;
				}
				else
				{
					AddNewLine();
				}

				AddEntryHtml(20 + nameWidth, m_Styles[i].Name);

				AddEntryItem(artWidth, Utility.RandomList(m_Styles[i].FloorTiles));
				AddEntryItem(artWidth, Utility.RandomList(m_Styles[i].WallTiles));
				AddEntryItem(artWidth, m_Styles[i].StairsUp);

				AddEntryButton(20, PageRightID1, PageRightID2, 3 + i, PageRightWidth, PageRightHeight);
			}

			FinishPage();

			m_BuildingList = false;
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			switch (info.ButtonID)
			{
				case 0:
					{
						_ = m_Player.CloseGump(typeof(CellarStyleGump));

						return;
					}
				case 1:
					{
						if (m_Page > 0)
						{
							_ = m_Player.SendGump(new CellarStyleGump(m_Player, m_Deed, m_Page - 1, m_Styles));
						}
						else
						{
							_ = m_Player.SendGump(new CellarStyleGump(m_Player, m_Deed, m_Page, m_Styles));
						}

						return;
					}
				case 2:
					{
						if ((m_Page + 1) * EntriesPerPage < m_Styles.Count)
						{
							_ = m_Player.SendGump(new CellarStyleGump(m_Player, m_Deed, m_Page + 1, m_Styles));
						}
						else
						{
							_ = m_Player.SendGump(new CellarStyleGump(m_Player, m_Deed, m_Page, m_Styles));
						}

						return;
					}
			}

			var v = info.ButtonID - 3;

			if (v >= 0 && v < m_Styles.Count)
			{
				var style = m_Styles[v];

				m_Player.SendMessage("You have chosen {0}.", style.Name);

				if (m_Deed != null && style.Style != CellarStyle.None)
				{
					m_Deed.BeginTarget(m_Player, style.Style);
				}
			}
			else
			{
				_ = m_Player.SendGump(new CellarStyleGump(m_Player, m_Deed, m_Page, m_Styles));
			}
		}
	}
}