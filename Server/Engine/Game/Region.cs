using Server.Items;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Server
{
	public enum MusicName
	{
		Invalid = -1,
		OldUlt01 = 0,
		Create1,
		DragFlit,
		OldUlt02,
		OldUlt03,
		OldUlt04,
		OldUlt05,
		OldUlt06,
		Stones2,
		Britain1,
		Britain2,
		Bucsden,
		Jhelom,
		LBCastle,
		Linelle,
		Magincia,
		Minoc,
		Ocllo,
		Samlethe,
		Serpents,
		Skarabra,
		Trinsic,
		Vesper,
		Wind,
		Yew,
		Cave01,
		Dungeon9,
		Forest_a,
		InTown01,
		Jungle_a,
		Mountn_a,
		Plains_a,
		Sailing,
		Swamp_a,
		Tavern01,
		Tavern02,
		Tavern03,
		Tavern04,
		Combat1,
		Combat2,
		Combat3,
		Approach,
		Death,
		Victory,
		BTCastle,
		Nujelm,
		Dungeon2,
		Cove,
		Moonglow,
		Zento,
		TokunoDungeon,
		Taiko,
		DreadHornArea,
		ElfCity,
		GrizzleDungeon,
		MelisandesLair,
		ParoxysmusLair,
		GwennoConversation,
		GoodEndGame,
		GoodVsEvil,
		GreatEarthSerpents,
		Humanoids_U9,
		MinocNegative,
		Paws,
		SelimsBar,
		SerpentIsleCombat_U7,
		ValoriaShips,
		TheWanderer,
		Castle,
		Festival,
		Honor,
		Medieval,
		BattleOnStones,
		Docktown,
		GargoyleQueen,
		GenericCombat,
		Holycity,
		HumanLevel,
		LoginLoop,
		NorthernForestBattleonStones,
		PrimevalLich,
		QueenPalace,
		RoyalCity,
		SlasherVeil,
		StygianAbyss,
		StygianDragon,
		Void,
		CodexShrine,
		AnvilStrikeInMinoc,
		ASkaranLullaby,
		BlackthornsMarch,
		DupresNightInTrinsic,
		FayaxionAndTheSix,
		FlightOfTheNexus,
		GalehavenJaunt,
		JhelomToArms,
		MidnightInYew,
		MoonglowSonata,
		NewMaginciaMarch,
		NujelmWaltz,
		SherrysSong,
		StarlightInBritain,
		TheVesperMist
	}

	[PropertyObject]
	public partial class Region : IComparable, IComparable<Region>, ISerializable
	{
		private static int m_NextID = 1;

		public static int NextID => m_NextID;

		public static Type DefaultRegionType { get; set; } = typeof(Region);

		public static TimeSpan StaffLogoutDelay { get; set; } = TimeSpan.Zero;
		public static TimeSpan DefaultLogoutDelay { get; set; } = TimeSpan.FromMinutes(5.0);

		public static readonly int DefaultPriority = 50;

		public static readonly int MinZ = SByte.MinValue;
		public static readonly int MaxZ = SByte.MaxValue + 1;

		public static event Action<Region, Mobile, Region> OnTransition;
		public static event Action<Region, Item, Region> OnVehicleTransition;

		public static event Action<Region> OnRegistered, OnUnregistered;

		public static event Action<Region, string> OnNameChanged;

		public static Region Find(int id)
		{
			return World.FindRegion(id);
		}

		public static Region Find(Point2D p, Map map)
		{
			if (map == null)
			{
				return Map.Internal.DefaultRegion;
			}

			var sector = map.GetSector(p);

			foreach (var o in sector.RegionRects)
			{
				foreach (var bound in o.Value)
				{
					if (bound.Contains(p))
					{
						return o.Key;
					}
				}
			}

			return map.DefaultRegion;
		}

		public static Region Find(IPoint2D p, Map map)
		{
			if (map == null)
			{
				return Map.Internal.DefaultRegion;
			}

			var sector = map.GetSector(p);

			foreach (var o in sector.RegionRects)
			{
				foreach (var bound in o.Value)
				{
					if (bound.Contains(p))
					{
						return o.Key;
					}
				}
			}

			return map.DefaultRegion;
		}

		public static Region Find(Point3D p, Map map)
		{
			if (map == null)
			{
				return Map.Internal.DefaultRegion;
			}

			var sector = map.GetSector(p);

			foreach (var o in sector.RegionRects)
			{
				foreach (var bound in o.Value)
				{
					if (bound.Contains(p))
					{
						return o.Key;
					}
				}
			}

			return map.DefaultRegion;
		}

		public static Region Find(IPoint3D p, Map map)
		{
			if (map == null)
			{
				return Map.Internal.DefaultRegion;
			}

			if (p is Item item)
			{
				p = item.GetWorldLocation();
			}

			var sector = map.GetSector(p);

			foreach (var o in sector.RegionRects)
			{
				foreach (var bound in o.Value)
				{
					if (bound.Contains(p))
					{
						return o.Key;
					}
				}
			}

			return map.DefaultRegion;
		}

		public static Region Find(int x, int y, Map map)
		{
			if (map == null)
			{
				return Map.Internal.DefaultRegion;
			}

			var sector = map.GetSector(x, y);

			foreach (var o in sector.RegionRects)
			{
				foreach (var bound in o.Value)
				{
					if (bound.Contains(x, y))
					{
						return o.Key;
					}
				}
			}

			return map.DefaultRegion;
		}

		public static Region Find(int x, int y, int z, Map map)
		{
			if (map == null)
			{
				return Map.Internal.DefaultRegion;
			}

			var sector = map.GetSector(x, y);

			foreach (var o in sector.RegionRects)
			{
				foreach (var bound in o.Value)
				{
					if (bound.Contains(x, y, z))
					{
						return o.Key;
					}
				}
			}

			return map.DefaultRegion;
		}

		public static Poly3D[] Convert(Rectangle2D[] rects)
		{
			var ret = new Poly3D[rects.Length];

			for (var i = 0; i < ret.Length; i++)
			{
				ret[i] = rects[i];
			}

			return ret;
		}

		public static Poly3D[] Convert(Rectangle3D[] rects)
		{
			var ret = new Poly3D[rects.Length];

			for (var i = 0; i < ret.Length; i++)
			{
				ret[i] = rects[i];
			}

			return ret;
		}

		public static Poly3D[] Convert(Poly2D[] rects)
		{
			var ret = new Poly3D[rects.Length];

			for (var i = 0; i < ret.Length; i++)
			{
				ret[i] = rects[i];
			}

			return ret;
		}

		private bool m_RequiresRegistration;

		internal int m_TypeRef;
		internal Serial m_Serial;

		int ISerializable.TypeReference => m_TypeRef;
		int ISerializable.SerialIdentity => m_Serial;

		[CommandProperty(AccessLevel.Counselor, true)]
		public int Id { get => m_Serial; private set => m_Serial = new Serial(value); }

		public bool Deleted { get; private set; }

		private string m_Name;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public string Name
		{
			get => m_Name;
			set
			{
				if (String.IsNullOrWhiteSpace(value))
				{
					value = null;
				}

				if (m_Name != value)
				{
					var old = m_Name;

					m_Name = value;

					OnNameChanged?.Invoke(this, old);
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, true)]
		public Map Map { get; private set; }

		private Region m_Parent;

		[CommandProperty(AccessLevel.Counselor, true)]
		public Region Parent
		{
			get => m_Parent;
			set
			{
				if (!World.Loading && value != null)
				{
					if (Map == null)
					{
						Map = value.Map;
					}
					else if (value.Map == null)
					{
						value.Map = Map;
					}
					else if (Map != value.Map)
					{
						return;
					}
				}

				if (m_Parent?.Children.Remove(this) == true)
				{
					m_Parent.OnChildRemoved(this);
				}

				var old = m_Parent;

				m_Parent = value;

				if (m_Parent?.Children.Add(this) == true)
				{
					m_Parent.OnChildAdded(this);
				}

				OnParentChanged(old);
			}
		}

		[CommandProperty(AccessLevel.Counselor, true)]
		public Region RootParent
		{
			get
			{
				var p = Parent;

				while (p != null)
				{
					if (p.Parent == null)
					{
						break;
					}

					p = p.Parent;
				}

				return p;
			}
		}

		[CommandProperty(AccessLevel.Counselor, true)]
		public int ChildLevel
		{
			get
			{
				var level = 0;

				var p = Parent;

				while (p != null)
				{
					++level;

					if (p.Parent == null)
					{
						break;
					}

					p = p.Parent;
				}

				return level;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public HashSet<Region> Children { get; private set; }

		[CommandProperty(AccessLevel.Counselor, true)]
		public bool Dynamic { get; private set; }

		private int m_Priority;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Priority { get => m_Priority; set => Delta(ref m_Priority, value); }

		[CommandProperty(AccessLevel.Counselor, true)]
		public bool Registered { get; private set; }

		//[CommandProperty(AccessLevel.Counselor, true)]
		public Sector[] Sectors { get; private set; }

		private Poly3D[] m_Area;

		public Poly3D[] Area { get => m_Area ??= Array.Empty<Poly3D>(); set => Delta(ref m_Area, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Point3D GoLocation { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public MusicName Music { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public AccessLevel LockdownLevel { get; set; }

		[CommandProperty(AccessLevel.Counselor, true)]
		public bool IsDefault => Map?.DefaultRegion == this;

		public virtual MusicName DefaultMusic => Parent?.Music ?? MusicName.Invalid;

		public Region(string name, Map map, int priority, params Rectangle2D[] area) : this(name, map, priority, Convert(area))
		{
		}

		public Region(string name, Map map, int priority, params Poly2D[] area) : this(name, map, priority, Convert(area))
		{
		}

		public Region(string name, Map map, int priority, params Rectangle3D[] area) : this(name, map, priority, Convert(area))
		{
		}

		public Region(string name, Map map, int priority, params Poly3D[] area) : this(name, map, null, area)
		{
			Priority = priority;
		}

		public Region(string name, Map map, Region parent, params Rectangle2D[] area) : this(name, map, parent, Convert(area))
		{
		}

		public Region(string name, Map map, Region parent, params Poly2D[] area) : this(name, map, parent, Convert(area))
		{
		}

		public Region(string name, Map map, Region parent, params Rectangle3D[] area) : this(name, map, parent, Convert(area))
		{
		}

		public Region(string name, Map map, Region parent, params Poly3D[] area)
		{
			Children = new();

			Id = m_NextID++;

			Dynamic = true;

			Name = name;
			Map = map;
			Parent = parent;
			Area = area;

			DefaultInit();
			Validate();

			World.AddRegion(this);

			ValidateTypeRef();
		}

		public Region(int id)
		{
			Id = id;

			if (++id > m_NextID)
			{
				m_NextID = id;
			}

			ValidateTypeRef();
		}

		private void ValidateTypeRef()
		{
			var ourType = GetType();

			m_TypeRef = World.m_RegionTypes.IndexOf(ourType);

			if (m_TypeRef == -1)
			{
				World.m_RegionTypes.Add(ourType);

				m_TypeRef = World.m_RegionTypes.Count - 1;
			}
		}

		protected virtual void DefaultInit()
		{
			Music = DefaultMusic;

			Priority = Parent?.Priority ?? DefaultPriority;
		}

		public virtual void Validate()
		{
			if (String.IsNullOrWhiteSpace(Name))
			{
				Name = null;
			}

			if ((GoLocation == Point3D.Zero || !Contains(GoLocation)) && Map != Map.Internal && Area.Length > 0 && Area[0].Count > 0)
			{
				var p = Area[0][0];

				GoLocation = new Point3D(p, Map.GetAverageZ(p.X, p.Y));
			}
		}

		private void Delta<T>(ref T val, T value)
		{
			var registered = Registered;

			if (registered)
			{
				Unregister();
			}

			val = value;

			Validate();

			if (registered)
			{
				Register();
			}
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write(0);

			writer.Write(IsDefault);
			writer.Write(Registered);

			writer.Write(m_Name);

			writer.Write(Map);

			writer.Write(m_Parent);

			writer.Write(Dynamic);
			writer.Write(m_Priority);
			writer.Write(Music);

			writer.Write(LockdownLevel);

			if (m_Area != null)
			{
				writer.Write(m_Area.Length);

				for (var i = 0; i < m_Area.Length; i++)
				{
					writer.Write(m_Area[i]);
				}
			}
			else
			{
				writer.Write(0);
			}

			writer.Write(Children);
		}

		public virtual void Deserialize(GenericReader reader)
		{
			_ = reader.ReadInt();

			var isDefault = reader.ReadBool();
			var isRegistered = reader.ReadBool();

			m_Name = reader.ReadString();

			Map = reader.ReadMap();

			m_Parent = reader.ReadRegion();

			Dynamic = reader.ReadBool();
			m_Priority = reader.ReadInt();
			Music = reader.ReadEnum<MusicName>();

			LockdownLevel = reader.ReadEnum<AccessLevel>();

			var count = reader.ReadInt();

			if (count > 0)
			{
				m_Area = new Poly3D[count];

				for (var i = 0; i < count; i++)
				{
					m_Area[i] = reader.ReadPoly3D();
				}
			}

			Children = reader.ReadRegionSet();

			m_RequiresRegistration = Registered = isRegistered;

			if (isDefault && Map != null)
			{
				Map.DefaultRegion = this;
			}
		}

		public virtual void Delete()
		{
			if (Deleted)
			{
				return;
			}

			var children = Children.ToDictionary(c => c, c => c.Registered);

			Unregister();

			Children.Clear();

			foreach (var o in children)
			{
				if (!o.Key.Deleted)
				{
					o.Key.Parent = Parent;

					if (o.Value)
					{
						o.Key.Register();
					}
				}
			}

			children.Clear();
			children.TrimExcess();

			OnDelete();

			Deleted = true;

			Parent = null;
			Map = null;
			Area = null;

			World.RemoveRegion(this);

			OnAfterDelete();
		}

		protected virtual void OnDelete()
		{
		}

		protected virtual void OnAfterDelete()
		{
		}

		protected virtual void OnParentChanged(Region oldParent)
		{
		}

		public void Register()
		{
			if (Deleted || (Registered && !m_RequiresRegistration))
			{
				return;
			}

			Validate();

			Registered = true;
			
			Map.RegisterRegion(this);

			if (!Registered)
			{
				return;
			}

			m_RequiresRegistration = false;

			if (Area.Length > 0)
			{
				var sectors = new HashSet<Sector>();

				for (var i = 0; i < Area.Length; i++)
				{
					var rect = Area[i];

					var start = Map.Bound(rect.Bounds.Start);
					var end = Map.Bound(rect.Bounds.End);

					var startSector = Map.GetSector(start);
					var endSector = Map.GetSector(end);

					for (var x = startSector.X; x <= endSector.X; x++)
					{
						for (var y = startSector.Y; y <= endSector.Y; y++)
						{
							var sector = Map.GetRealSector(x, y);

							sectors.Add(sector);

							sector.OnEnter(this, rect);
						}
					}
				}

				Sectors = sectors.ToArray();

				sectors.Clear();
				sectors.TrimExcess();
			}
			else
			{
				Sectors = Array.Empty<Sector>();
			}

			foreach (var c in Children)
			{
				c.Register();
			}

			OnRegister();

			OnRegistered?.Invoke(this);
		}

		public void Unregister()
		{
			if (Deleted || !Registered)
			{
				return;
			}

			Registered = false;

			Map.UnregisterRegion(this);

			if (Registered)
			{
				return;
			}

			if (Sectors != null)
			{
				for (var i = 0; i < Sectors.Length; i++)
				{
					Sectors[i].OnLeave(this);
				}
			}

			Sectors = null;

			foreach (var c in Children)
			{
				c.Unregister();
			}

			OnUnregister();

			OnUnregistered?.Invoke(this);
		}

		public bool Contains(Point2D p)
		{
			if (Deleted)
			{
				return false;
			}

			for (var i = 0; i < Area.Length; i++)
			{
				if (Area[i].Contains(p))
				{
					return true;
				}
			}

			return false;
		}

		public bool Contains(IPoint2D p)
		{
			if (Deleted)
			{
				return false;
			}

			for (var i = 0; i < Area.Length; i++)
			{
				if (Area[i].Contains(p))
				{
					return true;
				}
			}

			return false;
		}

		public bool Contains(Point3D p)
		{
			if (Deleted)
			{
				return false;
			}

			for (var i = 0; i < Area.Length; i++)
			{
				if (Area[i].Contains(p))
				{
					return true;
				}
			}

			return false;
		}

		public bool Contains(IPoint3D p)
		{
			if (Deleted)
			{
				return false;
			}

			for (var i = 0; i < Area.Length; i++)
			{
				if (Area[i].Contains(p))
				{
					return true;
				}
			}

			return false;
		}

		public bool Contains(int x, int y)
		{
			if (Deleted)
			{
				return false;
			}

			for (var i = 0; i < Area.Length; i++)
			{
				if (Area[i].Contains(x, y))
				{
					return true;
				}
			}

			return false;
		}

		public bool Contains(int x, int y, int z)
		{
			if (Deleted)
			{
				return false;
			}

			for (var i = 0; i < Area.Length; i++)
			{
				if (Area[i].Contains(x, y, z))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsChildOf(Region region)
		{
			if (Deleted || region?.Deleted != false)
			{
				return false;
			}

			var p = m_Parent;

			while (p != null)
			{
				if (p == region)
				{
					return true;
				}

				p = p.m_Parent;
			}

			return false;
		}

		public T GetRegion<T>() where T : Region
		{
			return (T)GetRegion(typeof(T));
		}

		public Region GetRegion(Type regionType)
		{
			if (Deleted || regionType == null)
			{
				return null;
			}

			var r = this;

			do
			{
				if (regionType.IsAssignableFrom(r.GetType()))
				{
					return r;
				}

				r = r.m_Parent;
			}
			while (r != null);

			return null;
		}

		public Region GetRegion(string regionName)
		{
			if (Deleted || regionName == null)
			{
				return null;
			}

			var r = this;

			do
			{
				if (r.Name == regionName)
				{
					return r;
				}

				r = r.m_Parent;
			}
			while (r != null);

			return null;
		}

		public bool IsPartOf(Region region)
		{
			if (Deleted || region?.Deleted != false)
			{
				return false;
			}

			if (this == region)
			{
				return true;
			}

			return IsChildOf(region);
		}

		public bool IsPartOf<T>() where T : Region
		{
			return IsPartOf(typeof(T));
		}

		public bool IsPartOf(Type regionType)
		{
			return GetRegion(regionType) != null;
		}

		public bool IsPartOf(string regionName)
		{
			return GetRegion(regionName) != null;
		}

		public virtual bool AcceptsSpawnsFrom(Mobile spawn, Region region)
		{
			if (Deleted || region?.Deleted != false)
			{
				return false;
			}

			if (spawn?.Player == true)
			{
				return true;
			}

			if (!AllowSpawn())
			{
				return false;
			}

			if (region == this)
			{
				return true;
			}

			if (m_Parent != null)
			{
				return m_Parent.AcceptsSpawnsFrom(spawn, region);
			}

			return false;
		}

		public List<Mobile> GetPlayers()
		{
			var list = new List<Mobile>();

			if (!Deleted && Sectors != null)
			{
				for (var i = 0; i < Sectors.Length; i++)
				{
					var sector = Sectors[i];

					foreach (var player in sector.Players)
					{
						if (player.Region.IsPartOf(this))
						{
							list.Add(player);
						}
					}
				}
			}

			return list;
		}

		public int GetPlayerCount()
		{
			var count = 0;

			if (!Deleted && Sectors != null)
			{
				for (var i = 0; i < Sectors.Length; i++)
				{
					var sector = Sectors[i];

					foreach (var player in sector.Players)
					{
						if (player.Region.IsPartOf(this))
						{
							count++;
						}
					}
				}
			}

			return count;
		}

		public List<Mobile> GetMobiles()
		{
			var list = new List<Mobile>();

			if (!Deleted && Sectors != null)
			{
				for (var i = 0; i < Sectors.Length; i++)
				{
					var sector = Sectors[i];

					foreach (var mobile in sector.Mobiles)
					{
						if (mobile.Region.IsPartOf(this))
						{
							list.Add(mobile);
						}
					}
				}
			}

			return list;
		}

		public int GetMobileCount()
		{
			var count = 0;

			if (!Deleted && Sectors != null)
			{
				for (var i = 0; i < Sectors.Length; i++)
				{
					var sector = Sectors[i];

					foreach (var mobile in sector.Mobiles)
					{
						if (mobile.Region.IsPartOf(this))
						{
							count++;
						}
					}
				}
			}

			return count;
		}

		public virtual int CompareTo(Region reg)
		{
			if (reg == null)
			{
				return -1;
			}

			var res = Deleted.CompareTo(reg.Deleted);

			if (res == 0)
			{
				res = Dynamic.CompareTo(reg.Dynamic) * -1;

				if (res == 0)
				{
					res = Priority.CompareTo(reg.Priority) * -1;

					if (res == 0)
					{
						res = ChildLevel.CompareTo(reg.ChildLevel) * -1;
					}
				}
			}
			
			return res;
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as Region);
		}

		public override sealed string ToString()
		{
			return String.IsNullOrWhiteSpace(Name) ? $"{GetType().Name} 0x{Id:X}" : Name;
		}

		public virtual void OnRegister()
		{
		}

		public virtual void OnUnregister()
		{
		}

		public virtual void OnChildAdded(Region child)
		{
		}

		public virtual void OnChildRemoved(Region child)
		{
		}

		public virtual bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
		{
			return m.WalkRegion == null || AcceptsSpawnsFrom(m, m.WalkRegion);
		}

		public virtual bool CanEnter(Mobile m)
		{
			if (Parent != null)
			{
				return Parent.CanEnter(m);
			}

			if (LockdownLevel > AccessLevel.Player && m.AccessLevel < LockdownLevel)
			{
				return false;
			}

			return true;
		}

		public virtual bool CanExit(Mobile m)
		{
			if (Parent != null)
			{
				return Parent.CanExit(m);
			}

			return true;
		}

		public virtual void OnEnter(Mobile m)
		{
		}

		public virtual void OnExit(Mobile m)
		{
		}

		public virtual bool OnVehicleMoveInto(Item vehicle, Direction d, Point3D newLocation, Point3D oldLocation)
		{
			return true;
		}

		public virtual bool CanVehicleEnter(Item vehicle)
		{
			if (Parent != null)
			{
				return Parent.CanVehicleEnter(vehicle);
			}

			return true;
		}

		public virtual bool CanVehicleExit(Item vehicle)
		{
			if (Parent != null)
			{
				return Parent.CanVehicleExit(vehicle);
			}

			return true;
		}

		public virtual void OnVehicleEnter(Item vehicle)
		{
		}

		public virtual void OnVehicleExit(Item vehicle)
		{
		}

		public virtual Type GetResource(Mobile from, IHarvestTool tool, Map map, Point3D loc, IHarvestSystem harvest, Type resource)
		{
			if (Parent != null)
			{
				return Parent.GetResource(from, tool, map, loc, harvest, resource);
			}

			return resource;
		}

		public virtual bool CanUseStuckMenu(Mobile m)
		{
			if (Parent != null)
			{
				return Parent.CanUseStuckMenu(m);
			}

			return true;
		}

		public virtual void OnAggressed(Mobile aggressor, Mobile aggressed, bool criminal)
		{
			if (Parent != null)
			{
				Parent.OnAggressed(aggressor, aggressed, criminal);
			}
		}

		public virtual void OnDidHarmful(Mobile harmer, Mobile harmed)
		{
			if (Parent != null)
			{
				Parent.OnDidHarmful(harmer, harmed);
			}
		}

		public virtual void OnGotHarmful(Mobile harmer, Mobile harmed)
		{
			if (Parent != null)
			{
				Parent.OnGotHarmful(harmer, harmed);
			}
		}

		public virtual void OnLocationChanged(Mobile m, Point3D oldLocation)
		{
			if (Parent != null)
			{
				Parent.OnLocationChanged(m, oldLocation);
			}
		}

		public virtual void OnVehicleLocationChanged(Item vehicle, Point3D oldLocation)
		{
			if (Parent != null)
			{
				Parent.OnVehicleLocationChanged(vehicle, oldLocation);
			}
		}

		public virtual bool OnTarget(Mobile m, Target t, object o)
		{
			if (Parent != null)
			{
				return Parent.OnTarget(m, t, o);
			}

			return true;
		}

		public virtual bool OnCombatantChange(Mobile m, Mobile Old, Mobile New)
		{
			if (Parent != null)
			{
				return Parent.OnCombatantChange(m, Old, New);
			}

			return true;
		}

		public virtual bool AllowHousing(Mobile from, Point3D p)
		{
			if (Parent != null)
			{
				return Parent.AllowHousing(from, p);
			}

			return true;
		}

		public virtual bool AllowVehicles(Mobile from, Point3D p)
		{
			if (Parent != null)
			{
				return Parent.AllowVehicles(from, p);
			}

			return true;
		}

		public virtual bool SendInaccessibleMessage(Item item, Mobile from)
		{
			if (Parent != null)
			{
				return Parent.SendInaccessibleMessage(item, from);
			}

			return false;
		}

		public virtual bool CheckAccessibility(Item item, Mobile from)
		{
			if (Parent != null)
			{
				return Parent.CheckAccessibility(item, from);
			}

			return true;
		}

		public virtual bool OnDecay(Item item)
		{
			if (Parent != null)
			{
				return Parent.OnDecay(item);
			}

			return true;
		}

		public virtual bool AllowHarmful(Mobile from, Mobile target)
		{
			if (Parent != null)
			{
				return Parent.AllowHarmful(from, target);
			}

			if (Mobile.AllowHarmfulHandler != null)
			{
				return Mobile.AllowHarmfulHandler(from, target);
			}

			return true;
		}

		public virtual void OnCriminalAction(Mobile m, bool message)
		{
			if (Parent != null)
			{
				Parent.OnCriminalAction(m, message);
			}
			else if (message)
			{
				m.SendLocalizedMessage(1005040); // You've committed a criminal act!!
			}
		}

		public virtual bool AllowBeneficial(Mobile from, Mobile target)
		{
			if (Parent != null)
			{
				return Parent.AllowBeneficial(from, target);
			}

			if (Mobile.AllowBeneficialHandler != null)
			{
				return Mobile.AllowBeneficialHandler(from, target);
			}

			return true;
		}

		public virtual void OnBeneficialAction(Mobile helper, Mobile target)
		{
			if (Parent != null)
			{
				Parent.OnBeneficialAction(helper, target);
			}
		}

		public virtual void OnGotBeneficialAction(Mobile helper, Mobile target)
		{
			if (Parent != null)
			{
				Parent.OnGotBeneficialAction(helper, target);
			}
		}

		public virtual void SpellDamageScalar(Mobile caster, Mobile target, ref double damage)
		{
			if (Parent != null)
			{
				Parent.SpellDamageScalar(caster, target, ref damage);
			}
		}

		public virtual void OnSpeech(SpeechEventArgs args)
		{
			if (Parent != null)
			{
				Parent.OnSpeech(args);
			}
		}

		public virtual bool OnSkillUse(Mobile m, int Skill)
		{
			if (Parent != null)
			{
				return Parent.OnSkillUse(m, Skill);
			}

			return true;
		}

		public virtual bool OnBeginSpellCast(Mobile m, ISpell s)
		{
			if (Parent != null)
			{
				return Parent.OnBeginSpellCast(m, s);
			}

			return true;
		}

		public virtual void OnSpellCast(Mobile m, ISpell s)
		{
			if (Parent != null)
			{
				Parent.OnSpellCast(m, s);
			}
		}

		public virtual bool OnResurrect(Mobile m)
		{
			if (Parent != null)
			{
				return Parent.OnResurrect(m);
			}

			return true;
		}

		public virtual bool OnBeforeDeath(Mobile m)
		{
			if (Parent != null)
			{
				return Parent.OnBeforeDeath(m);
			}

			return true;
		}

		public virtual void OnDeath(Mobile m)
		{
			if (Parent != null)
			{
				Parent.OnDeath(m);
			}
		}

		public virtual bool OnDamage(Mobile m, ref int Damage)
		{
			if (Parent != null)
			{
				return Parent.OnDamage(m, ref Damage);
			}

			return true;
		}

		public virtual bool OnHeal(Mobile m, ref int Heal)
		{
			if (Parent != null)
			{
				return Parent.OnHeal(m, ref Heal);
			}

			return true;
		}

		public virtual bool OnDoubleClick(Mobile m, object o)
		{
			if (Parent != null)
			{
				return Parent.OnDoubleClick(m, o);
			}

			return true;
		}

		public virtual bool OnSingleClick(Mobile m, object o)
		{
			if (Parent != null)
			{
				return Parent.OnSingleClick(m, o);
			}

			return true;
		}

		public virtual bool AllowSpawn()
		{
			if (Parent != null)
			{
				return Parent.AllowSpawn();
			}

			return true;
		}

		public virtual void AlterLightLevel(Mobile m, ref int global, ref int personal)
		{
			if (Parent != null)
			{
				Parent.AlterLightLevel(m, ref global, ref personal);
			}
		}

		public virtual TimeSpan GetLogoutDelay(Mobile m)
		{
			if (Parent != null)
			{
				return Parent.GetLogoutDelay(m);
			}

			if (m.AccessLevel >= AccessLevel.Counselor)
			{
				return StaffLogoutDelay;
			}

			return DefaultLogoutDelay;
		}

		public void PlayMusic(Mobile m)
		{
			if (m?.NetState != null)
			{
				var music = Music;

				if (OnPlayMusic(m, ref music))
				{
					m.Send(new PlayMusic(music));
				}
			}
		}

		protected virtual bool OnPlayMusic(Mobile m, ref MusicName music)
		{
			if (Parent != null)
			{
				return Parent.OnPlayMusic(m, ref music);
			}

			return true;
		}

		public static bool CanTransition(Mobile m, Point3D location, Map map)
		{
			var oldRegion = m.Region;
			var newRegion = Find(location, map);

			while (oldRegion != newRegion)
			{
				if (!oldRegion.CanExit(m))
				{
					return false;
				}

				if (!newRegion.CanEnter(m))
				{
					return false;
				}

				if (newRegion.Parent == null)
				{
					return true;
				}

				newRegion = newRegion.Parent;
			}

			return true;
		}

		public static bool CanMove(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation, Map map)
		{
			var oldRegion = m.Region;
			var newRegion = Find(newLocation, map);

			while (oldRegion != newRegion)
			{
				if (!oldRegion.CanExit(m))
				{
					return false;
				}

				if (!newRegion.CanEnter(m))
				{
					return false;
				}

				if (!newRegion.OnMoveInto(m, d, newLocation, oldLocation))
				{
					return false;
				}

				if (newRegion.Parent == null)
				{
					return true;
				}

				newRegion = newRegion.Parent;
			}

			return true;
		}

		public static void OnRegionChange(Mobile m, Region oldRegion, Region newRegion)
		{
			var oldR = oldRegion;
			var newR = newRegion;

			while (oldR != newR)
			{
				var oldRChild = oldR?.ChildLevel ?? -1;
				var newRChild = newR?.ChildLevel ?? -1;

				if (oldR != null && oldRChild >= newRChild)
				{
					oldR.OnExit(m);

					//EventSink.InvokeOnExitRegion(new OnExitRegionEventArgs(m, oldR, newR));

					oldR = oldR.Parent;
				}

				if (newR != null && newRChild >= oldRChild)
				{
					newR.OnEnter(m);

					//EventSink.InvokeOnEnterRegion(new OnEnterRegionEventArgs(m, oldR, newR));

					newR = newR.Parent;
				}
			}

			if (newRegion != null && m.NetState != null)
			{
				m.CheckLightLevels(false);

				if (oldRegion == null || oldRegion.Music != newRegion.Music)
				{
					newRegion.PlayMusic(m);
				}
			}

			OnTransition?.Invoke(oldRegion, m, newRegion);
		}

		public static bool CanVehicleMove(Item vehicle, Mobile owner, Direction d, Point3D newLocation, Point3D oldLocation, Map map)
		{
			var oldRegion = Find(vehicle.Location, vehicle.Map);
			var newRegion = Find(newLocation, map);

			while (oldRegion != newRegion)
			{
				if (!oldRegion.CanVehicleExit(vehicle))
				{
					return false;
				}

				if (!newRegion.AllowVehicles(owner, newLocation))
				{
					return false;
				}

				if (!newRegion.CanVehicleEnter(vehicle))
				{
					return false;
				}

				if (!newRegion.OnVehicleMoveInto(vehicle, d, newLocation, oldLocation))
				{
					return false;
				}

				if (newRegion.Parent == null)
				{
					return true;
				}

				newRegion = newRegion.Parent;
			}

			return true;
		}

		public static void OnVehicleRegionChange(Item vehicle, Region oldRegion, Region newRegion)
		{
			var oldR = oldRegion;
			var newR = newRegion;

			while (oldR != newR)
			{
				var oldRChild = oldR?.ChildLevel ?? -1;
				var newRChild = newR?.ChildLevel ?? -1;

				if (oldR != null && oldRChild >= newRChild)
				{
					oldR.OnVehicleExit(vehicle);

					//EventSink.InvokeOnVehicleExitRegion(new OnVehicleExitRegionEventArgs(vehicle, oldR, newR));

					oldR = oldR.Parent;
				}

				if (newR != null && newRChild >= oldRChild)
				{
					newR.OnVehicleEnter(vehicle);

					//EventSink.InvokeOnVehicleEnterRegion(new OnVehicleEnterRegionEventArgs(vehicle, oldR, newR));

					newR = newR.Parent;
				}
			}

			OnVehicleTransition?.Invoke(oldRegion, vehicle, newRegion);
		}

		public static bool CanVehicleTransition(Item vehicle, Mobile owner, Point3D location, Map map)
		{
			var oldRegion = Find(vehicle.Location, vehicle.Map);
			var newRegion = Find(location, map);

			while (oldRegion != newRegion)
			{
				if (!oldRegion.CanVehicleExit(vehicle))
				{
					return false;
				}

				if (!newRegion.AllowVehicles(owner, location))
				{
					return false;
				}

				if (!newRegion.CanVehicleEnter(vehicle))
				{
					return false;
				}

				if (newRegion.Parent == null)
				{
					return true;
				}

				newRegion = newRegion.Parent;
			}

			return true;
		}

		public static bool Generating { get; private set; }

		internal static void GenerateRegions()
		{
			if (Generating)
			{
				return;
			}

			Generating = true;

			try
			{
				Console.WriteLine("Regions: Generating...");

				var count = 0;

				foreach (var entry in m_Definitions)
				{
					var map = Map.Parse(entry.Key);

					if (map == Map.Internal)
					{
						Console.WriteLine("Invalid internal map in a facet element");
					}
					else
					{
						count += GenerateRegions(entry.Value, map, null);
					}
				}

				Console.WriteLine("Regions: Registering...");

				foreach (var region in World.Regions.Values)
				{
					if (region.Parent == null)
					{
						region.Register();
					}
				}

				Console.WriteLine($"Regions: done ({count:N0} regions)");
			}
			finally
			{
				Generating = false;
			}
		}

		private static int GenerateRegions(HashSet<RegionDefinition> defs, Map map, Region parent)
		{
			var count = 0;

			foreach (var def in defs)
			{
				var region = CreateInstance(def, map, parent);

				if (region != null)
				{
					count += 1 + GenerateRegions(def.Children, map, region);
				}
			}

			return count;
		}

		public static Region CreateInstance(RegionDefinition def, Map map, Region parent)
		{
			var type = ScriptCompiler.FindTypeByName(def.Type, true);

			if (type == null || !typeof(Region).IsAssignableFrom(type))
			{
				Utility.PushColor(ConsoleColor.Yellow);
				Console.WriteLine($"Invalid region type '{def.Type}', defaulting to '{DefaultRegionType.FullName}'");
				Utility.PopColor();

				type = DefaultRegionType;
			}

			if (!typeof(Region).IsAssignableFrom(type))
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine($"Invalid region type '{type.FullName}'");
				Utility.PopColor();

				return null;
			}

			try
			{
				return (Region)Activator.CreateInstance(type, def, map, parent);
			}
			catch (Exception ex)
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine($"Error during the creation of region type '{type.FullName}':");
				Console.WriteLine(ex);
				Utility.PopColor();

				return null;
			}
		}

		public Region(RegionDefinition def, Map map, Region parent)
		{
			Children = new();

			Id = m_NextID++;

			Dynamic = false;

			Map = map;
			Parent = parent;

			DefaultInit();

			Area = def.Bounds.ToArray();

			var type = GetType();

			foreach (var entry in def.Props)
			{
				object val;

				try
				{
					var prop = type.GetProperty(entry.Key);

					if (prop != null && prop.SetMethod != null)
					{
						if (prop.PropertyType == typeof(Map))
						{
							val = Map.Parse(entry.Value.ToString());
						}
						else if (prop.PropertyType == typeof(Type))
						{
							val = ScriptCompiler.FindTypeByName(entry.Value.ToString());
						}
						else if (prop.PropertyType.IsEnum)
						{
							if (!Enum.TryParse(prop.PropertyType, entry.Value.ToString(), true, out val))
							{
								val = Enum.ToObject(prop.PropertyType, 0);
							}
						}
						else
						{
							val = entry.Value;
						}

						prop.SetValue(this, val);
					}
					else
					{
						var t = type;

						while (t != null)
						{
							var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
							var field = t.GetField($"m_{entry.Key}", flags) ?? t.GetField($"_{entry.Key}", flags);

							if (field != null)
							{
								if (field.FieldType == typeof(Map))
								{
									val = Map.Parse(entry.Value.ToString());
								}
								else if (field.FieldType == typeof(Type))
								{
									val = ScriptCompiler.FindTypeByName(entry.Value.ToString());
								}
								else if (field.FieldType.IsEnum)
								{
									if (!Enum.TryParse(field.FieldType, entry.Value.ToString(), true, out val))
									{
										val = Enum.ToObject(field.FieldType, 0);
									}
								}
								else
								{
									val = entry.Value;
								}

								field.SetValue(this, val);

								break;
							}
							else
							{
								t = t.BaseType;
							}
						}
					}
				}
				catch
				{
					Utility.PushColor(ConsoleColor.Yellow);
					Console.WriteLine($"Warning: Could not set '{entry.Key}' for '{type.Name}'");
					Utility.PopColor();
				}
			}

			Validate();

			World.AddRegion(this);

			ValidateTypeRef();
		}
	}

	public class Sector
	{
		public List<BaseMulti> Multis { get; } = new();
		public List<Mobile> Mobiles { get; } = new();
		public List<Item> Items { get; } = new();
		public List<NetState> Clients { get; } = new();
		public List<Mobile> Players { get; } = new();

		public SortedDictionary<Region, HashSet<Poly3D>> RegionRects { get; } = new();

		public Map Owner { get; }

		public int X { get; }
		public int Y { get; }

		private bool m_Active;

		public bool Active => m_Active && Owner != Map.Internal;

		public Sector(int x, int y, Map owner)
		{
			X = x;
			Y = y;
			Owner = owner;
		}

		private static void Replace<T>(IList<T> list, T oldValue, T newValue)
		{
			if (oldValue != null && newValue != null)
			{
				var index = list.IndexOf(oldValue);

				if (index >= 0)
				{
					list[index] = newValue;
				}
				else
				{
					list.Add(newValue);
				}
			}
			else if (oldValue != null)
			{
				list.Remove(oldValue);
			}
			else if (newValue != null)
			{
				list.Add(newValue);
			}
		}

		public void OnClientChange(NetState oldState, NetState newState)
		{
			Replace(Clients, oldState, newState);
		}

		public void OnEnter(Item item)
		{
			Items.Add(item);
		}

		public void OnLeave(Item item)
		{
			Items.Remove(item);
		}

		public void OnEnter(Mobile mob)
		{
			Mobiles.Add(mob);

			if (mob.NetState != null)
			{
				Clients.Add(mob.NetState);
			}

			if (mob.Player)
			{
				if (Players.Count == 0)
				{
					Owner.ActivateSectors(X, Y);
				}

				Players.Add(mob);
			}
		}

		public void OnLeave(Mobile mob)
		{
			Mobiles.Remove(mob);

			if (mob.NetState != null)
			{
				Clients.Remove(mob.NetState);
			}

			if (mob.Player && Players != null)
			{
				Players.Remove(mob);

				if (Players.Count == 0)
				{
					Owner.DeactivateSectors(X, Y);
				}
			}
		}

		public void OnEnter(Region region, Poly3D rect)
		{
			if (!RegionRects.TryGetValue(region, out var rects))
			{
				RegionRects[region] = rects = new();
			}

			if (rects.Add(rect))
			{
				UpdateMobileRegions();
			}
		}

		public void OnLeave(Region region)
		{
			if (RegionRects.Remove(region))
			{
				UpdateMobileRegions();
			}
		}

		private void UpdateMobileRegions()
		{
			var index = Mobiles.Count;

			while (--index >= 0)
			{
				if (index < Mobiles.Count)
				{
					Mobiles[index].UpdateRegion();
				}
			}
		}

		public void OnMultiEnter(BaseMulti multi)
		{
			Multis.Add(multi);
		}

		public void OnMultiLeave(BaseMulti multi)
		{
			Multis.Remove(multi);
		}

		public void Activate()
		{
			if (!Active && Owner != Map.Internal)
			{
				foreach (var item in Items)
				{
					item.OnSectorActivate();
				}

				foreach (var mob in Mobiles)
				{
					mob.OnSectorActivate();
				}

				m_Active = true;
			}
		}

		public void Deactivate()
		{
			if (Active)
			{
				foreach (var item in Items)
				{
					item.OnSectorDeactivate();
				}

				foreach (var mob in Mobiles)
				{
					mob.OnSectorDeactivate();
				}

				m_Active = false;
			}
		}
	}

	public sealed class RegionDefinition : IEnumerable<object>
	{
		public static void Generate()
		{
			var fs = File.OpenWrite(Path.Combine(Core.BaseDirectory, "Server", "Engine", "Game", "Regions.cs"));
			var fw = new StreamWriter(fs);

			void write(ref int tabs, string line)
			{
				if (line == "{")
				{
					for (var i = 0; i < tabs; i++)
					{
						fw.Write('\t');
					}

					fw.WriteLine(line);

					++tabs;
				}
				else
				{
					if (line == "}" || line == "}," || line == "};")
					{
						--tabs;
					}

					if (line != "")
					{
						for (var i = 0; i < tabs; i++)
						{
							fw.Write('\t');
						}
					}

					fw.WriteLine(line);
				}

				fw.Flush();
			};

			void writeRegion(ref int tabs, Region reg)
			{
				var type = reg.GetType();

				write(ref tabs, $"#region {reg.Name}");
				write(ref tabs, $"");
				write(ref tabs, $"new(\"{type.Name}\")");
				write(ref tabs, $"{{");

				var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

				foreach (var prop in props)
				{
					if (prop.GetMethod.IsVirtual)
					{
						continue;
					}

					if (prop.Name.Contains("Default") || prop.Name == "ChildLevel" || prop.Name == "Dynamic" || prop.Name == "Map" || prop.Name == "Registered")
					{
						continue;
					}

					var val = prop.GetValue(reg, null);

					if (prop.PropertyType.IsEnum || prop.PropertyType == typeof(string) || prop.PropertyType == typeof(Map))
					{
						write(ref tabs, $"{{ \"{prop.Name}\", \"{val}\" }},");
					}
					else if (prop.PropertyType == typeof(bool))
					{
						write(ref tabs, $"{{ \"{prop.Name}\", {(val?.ToString() ?? Boolean.FalseString).ToLower()} }},");
					}
					else if (prop.PropertyType == typeof(Type))
					{
						write(ref tabs, $"{{ \"{prop.Name}\", \"{(val as Type)?.Name}\" }},");
					}
					else if (prop.PropertyType.GetCustomAttributes(typeof(ParsableAttribute), true)?.Length > 0)
					{
						write(ref tabs, $"{{ \"{prop.Name}\", new {prop.PropertyType.Name}{val} }},");
					}
					else if (prop.PropertyType.IsValueType)
					{
						write(ref tabs, $"{{ \"{prop.Name}\", {val} }},");
					}
				}

				if (reg.Area.Length > 0)
				{
					write(ref tabs, $"");

					foreach (var rect in reg.Area)
					{
						var points = String.Join(", ", rect.Points.Select(p => $"new{p}"));

						write(ref tabs, $"{{ {rect.MinZ}, {rect.MaxZ}, {points} }},");
					}
				}

				if (reg.Children.Count > 0)
				{
					write(ref tabs, $"");

					var firstChild = true;

					foreach (var child in reg.Children)
					{
						if (!firstChild)
						{
							write(ref tabs, $"");
						}

						writeRegion(ref tabs, child);

						firstChild = false;
					}
				}

				write(ref tabs, $"}},");
				write(ref tabs, $"");
				write(ref tabs, $"#endregion");
			};

			var indent = 0;

			write(ref indent, $"using System.Collections.Generic;");
			write(ref indent, $"");
			write(ref indent, $"namespace Server");
			write(ref indent, $"{{");
			{
				write(ref indent, $"public partial class Region");
				write(ref indent, $"{{");
				{
					write(ref indent, "private static readonly Dictionary<string, HashSet<RegionDefinition>> m_Definitions = new()");
					write(ref indent, $"{{");
					{
						var firstMap = true;

						foreach (var map in Map.Maps)
						{
							if (map == null || map == Map.Internal)
							{
								continue;
							}

							if (!firstMap)
							{
								write(ref indent, $"");
							}

							write(ref indent, $"#region {map.Name}");
							write(ref indent, $"");
							write(ref indent, $"[\"{map.Name}\"] = new()");
							write(ref indent, $"{{");

							var firstReg = true;

							foreach (var reg in map.Regions)
							{
								if (reg.Parent != null)
								{
									continue;
								}

								if (!firstReg)
								{
									write(ref indent, $"");
								}

								writeRegion(ref indent, reg);

								firstReg = false;
							}

							write(ref indent, $"}},");
							write(ref indent, $"");
							write(ref indent, $"#endregion");

							firstMap = false;
						}
					}
					write(ref indent, $"}};");
				}
				write(ref indent, $"}}");
			}
			write(ref indent, $"}}");

			fw.Flush();
			fw.Close();
			fs.Close();
		}

		public string Type { get; }

		public Dictionary<string, object> Props { get; } = new Dictionary<string, object>();

		public HashSet<Poly3D> Bounds { get; } = new HashSet<Poly3D>();

		public HashSet<RegionDefinition> Children { get; } = new HashSet<RegionDefinition>();

		public RegionDefinition(string type)
		{
			Type = type;
		}

		public void Add(RegionDefinition child)
		{
			Children.Add(child);
		}

		public void Add(string prop, object value)
		{
			Props[prop] = value;
		}

		public void Add(int x, int y, int z, int width, int height, int depth)
		{
			Bounds.Add(new Rectangle3D(x, y, z, width, height, depth));
		}

		public IEnumerator<object> GetEnumerator()
		{
			foreach (var rect in Bounds)
			{
				yield return rect;
			}

			foreach (var child in Children)
			{
				yield return child;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}