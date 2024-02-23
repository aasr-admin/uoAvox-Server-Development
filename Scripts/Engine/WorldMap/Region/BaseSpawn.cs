using Server.Commands;
using Server.Items;
using Server.Mobiles;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Server.Regions
{
	public class SpawnEntry : ISpawner
	{
		public static readonly TimeSpan DefaultMinSpawnTime = TimeSpan.FromMinutes(2.0);
		public static readonly TimeSpan DefaultMaxSpawnTime = TimeSpan.FromMinutes(5.0);

		public static readonly Direction InvalidDirection = Direction.Running;

		private static readonly Hashtable m_Table = new Hashtable();

		public static Hashtable Table => m_Table;

		// When a creature's AI is deactivated (PlayerRangeSensitive optimization) does it return home?
		public virtual bool ReturnOnDeactivate => true;

		// Are creatures unlinked on taming (true) or should they also go out of the region (false)?
		public virtual bool UnlinkOnTaming => false;

		// Are unlinked and untamed creatures removed after 20 hours?
		public virtual bool RemoveIfUntamed => true;

		private readonly int m_ID;
		private readonly BaseRegion m_Region;
		private Point3D m_Home;
		private readonly int m_Range;
		private readonly Direction m_Direction;
		private readonly SpawnDefinition m_Definition;
		private readonly List<ISpawnable> m_SpawnedObjects;
		private int m_Max;
		private readonly TimeSpan m_MinSpawnTime;
		private readonly TimeSpan m_MaxSpawnTime;
		private bool m_Running;

		private DateTime m_NextSpawn;
		private Timer m_SpawnTimer;

		public int ID => m_ID;
		public BaseRegion Region => m_Region;
		public Point3D HomeLocation => m_Home;
		public int HomeRange => m_Range;
		public Direction Direction => m_Direction;
		public SpawnDefinition Definition => m_Definition;
		public List<ISpawnable> SpawnedObjects => m_SpawnedObjects;
		public int Max => m_Max;
		public TimeSpan MinSpawnTime => m_MinSpawnTime;
		public TimeSpan MaxSpawnTime => m_MaxSpawnTime;
		public bool Running => m_Running;

		public bool Complete => m_SpawnedObjects.Count >= m_Max;
		public bool Spawning => m_Running && !Complete;

		public SpawnEntry(int id, BaseRegion region, Point3D home, int range, Direction direction, SpawnDefinition definition, int max, TimeSpan minSpawnTime, TimeSpan maxSpawnTime)
		{
			m_ID = id;
			m_Region = region;
			m_Home = home;
			m_Range = range;
			m_Direction = direction;
			m_Definition = definition;
			m_SpawnedObjects = new List<ISpawnable>();
			m_Max = max;
			m_MinSpawnTime = minSpawnTime;
			m_MaxSpawnTime = maxSpawnTime;
			m_Running = false;

			if (m_Table.Contains(id))
			{
				Console.WriteLine("Warning: double SpawnEntry ID '{0}'", id);
			}
			else
			{
				m_Table[id] = this;
			}
		}

		public Point3D RandomSpawnLocation(int spawnHeight, bool land, bool water)
		{
			if (m_Region?.Registered != true)
			{
				return Point3D.Zero;
			}
			
			return m_Region.RandomSpawnLocation(spawnHeight, land, water, m_Home, m_Range);
		}

		public void Start()
		{
			if (m_Running)
			{
				return;
			}

			m_Running = true;
			CheckTimer();
		}

		public void Stop()
		{
			if (!m_Running)
			{
				return;
			}

			m_Running = false;
			CheckTimer();
		}

		private void Spawn()
		{
			var spawn = m_Definition.Spawn(this);

			if (spawn != null)
			{
				Add(spawn);
			}
		}

		private void Add(ISpawnable spawn)
		{
			m_SpawnedObjects.Add(spawn);

			spawn.Spawner = this;

			if (spawn is BaseCreature c)
			{
				c.RemoveIfUntamed = RemoveIfUntamed;
			}
		}

		void ISpawner.Remove(ISpawnable spawn)
		{
			m_SpawnedObjects.Remove(spawn);

			CheckTimer();
		}

		private TimeSpan RandomTime()
		{
			var min = (int)m_MinSpawnTime.TotalSeconds;
			var max = (int)m_MaxSpawnTime.TotalSeconds;

			var rand = Utility.RandomMinMax(min, max);
			return TimeSpan.FromSeconds(rand);
		}

		private void CheckTimer()
		{
			if (Spawning)
			{
				if (m_SpawnTimer == null)
				{
					var time = RandomTime();
					m_SpawnTimer = Timer.DelayCall(time, TimerCallback);
					m_NextSpawn = DateTime.UtcNow + time;
				}
			}
			else if (m_SpawnTimer != null)
			{
				m_SpawnTimer.Stop();
				m_SpawnTimer = null;
			}
		}

		private void TimerCallback()
		{
			if (m_Region?.Deleted != false)
			{
				Delete();
				return;
			}

			var amount = Math.Max((m_Max - m_SpawnedObjects.Count) / 3, 1);

			for (var i = 0; i < amount; i++)
			{
				Spawn();
			}

			m_SpawnTimer = null;
			CheckTimer();
		}

		public void DeleteSpawnedObjects()
		{
			InternalDeleteSpawnedObjects();

			m_Running = false;
			CheckTimer();
		}

		private void InternalDeleteSpawnedObjects()
		{
			foreach (var spawnable in m_SpawnedObjects)
			{
				spawnable.Spawner = null;

				var uncontrolled = spawnable is not BaseCreature c || !c.Controlled;

				if (uncontrolled)
				{
					spawnable.Delete();
				}
			}

			m_SpawnedObjects.Clear();
		}

		public void Respawn()
		{
			if (m_Region?.Deleted != false)
			{
				Delete();
				return;
			}

			InternalDeleteSpawnedObjects();

			for (var i = 0; !Complete && i < m_Max; i++)
			{
				Spawn();
			}

			m_Running = true;
			CheckTimer();
		}

		public void Delete()
		{
			Stop();

			m_Max = 0;

			InternalDeleteSpawnedObjects();

			if (m_SpawnTimer != null)
			{
				m_SpawnTimer.Stop();
				m_SpawnTimer = null;
			}

			if (m_Table[m_ID] == this)
			{
				m_Table.Remove(m_ID);
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(m_SpawnedObjects.Count);

			for (var i = 0; i < m_SpawnedObjects.Count; i++)
			{
				writer.Write(m_SpawnedObjects[i].Serial);
			}

			writer.Write(m_Running);

			if (m_SpawnTimer != null)
			{
				writer.Write(true);
				writer.WriteDeltaTime(m_NextSpawn);
			}
			else
			{
				writer.Write(false);
			}
		}

		public void Deserialize(GenericReader reader, int version)
		{
			var count = reader.ReadInt();

			for (var i = 0; i < count; i++)
			{
				if (World.FindEntity(reader.ReadSerial()) is ISpawnable spawnableEntity)
				{
					Add(spawnableEntity);
				}
			}

			m_Running = reader.ReadBool();

			if (reader.ReadBool())
			{
				m_NextSpawn = reader.ReadDeltaTime();

				if (Spawning)
				{
					if (m_SpawnTimer != null)
					{
						m_SpawnTimer.Stop();
					}

					var delay = m_NextSpawn - DateTime.UtcNow;

					m_SpawnTimer = Timer.DelayCall(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, TimerCallback);
				}
			}

			CheckTimer();
		}

		private static List<IEntity> m_RemoveList;

		public static void Remove(GenericReader reader, int version)
		{
			var count = reader.ReadInt();

			for (var i = 0; i < count; i++)
			{
				var entity = World.FindEntity(reader.ReadSerial());

				if (entity != null)
				{
					m_RemoveList ??= new List<IEntity>();
					m_RemoveList.Add(entity);
				}
			}

			reader.ReadBool(); // m_Running

			if (reader.ReadBool())
			{
				reader.ReadDeltaTime(); // m_NextSpawn
			}
		}

		public static void Initialize()
		{
			if (m_RemoveList != null)
			{
				foreach (var ent in m_RemoveList)
				{
					ent.Delete();
				}

				m_RemoveList = null;
			}

			SpawnPersistence.EnsureExistence();

			CommandSystem.Register("RespawnAllRegions", AccessLevel.Administrator, new CommandEventHandler(RespawnAllRegions_OnCommand));
			CommandSystem.Register("RespawnRegion", AccessLevel.GameMaster, new CommandEventHandler(RespawnRegion_OnCommand));
			CommandSystem.Register("DelAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler(DelAllRegionSpawns_OnCommand));
			CommandSystem.Register("DelRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler(DelRegionSpawns_OnCommand));
			CommandSystem.Register("StartAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler(StartAllRegionSpawns_OnCommand));
			CommandSystem.Register("StartRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler(StartRegionSpawns_OnCommand));
			CommandSystem.Register("StopAllRegionSpawns", AccessLevel.Administrator, new CommandEventHandler(StopAllRegionSpawns_OnCommand));
			CommandSystem.Register("StopRegionSpawns", AccessLevel.GameMaster, new CommandEventHandler(StopRegionSpawns_OnCommand));
		}

		private static IEnumerable<SpawnEntry> GetCommandData(CommandEventArgs args)
		{
			var from = args.Mobile;

			var count = 0;

			if (args.Length == 0)
			{
				var br = from.Region.GetRegion<BaseRegion>();

				foreach (var s in br?.Spawns)
				{
					++count;

					yield return s;
				}

				if (count == 0)
				{
					from.SendMessage("There are no spawners in your region.");
				}
			}
			else
			{
				var name = args.GetString(0);

				foreach (var r in from.Map.Regions)
				{
					if (r is BaseRegion br && br.Spawns?.Length > 0)
					{
						foreach (var s in br?.Spawns)
						{
							++count;

							yield return s;
						}
					}
				}

				if (count == 0)
				{
					from.SendMessage("There are no spawners in regions named '{0}'.", name);
				}
			}
		}

		[Usage("RespawnAllRegions")]
		[Description("Respawns all regions and sets the spawners as running.")]
		private static void RespawnAllRegions_OnCommand(CommandEventArgs args)
		{
			foreach (SpawnEntry entry in m_Table.Values)
			{
				entry.Respawn();
			}

			args.Mobile.SendMessage("All regions have respawned.");
		}

		[Usage("RespawnRegion [<region name>]")]
		[Description("Respawns the region in which you are (or that you provided) and sets the spawners as running.")]
		private static void RespawnRegion_OnCommand(CommandEventArgs args)
		{
			foreach (var g in GetCommandData(args).GroupBy(s => s.Region))
			{
				foreach (var s in g)
				{
					s.Respawn();
				}

				args.Mobile.SendMessage("Region '{0}' has respawned.", g.Key);
			}
		}

		[Usage("DelAllRegionSpawns")]
		[Description("Deletes all spawned objects of every regions and sets the spawners as not running.")]
		private static void DelAllRegionSpawns_OnCommand(CommandEventArgs args)
		{
			foreach (SpawnEntry entry in m_Table.Values)
			{
				entry.DeleteSpawnedObjects();
			}

			args.Mobile.SendMessage("All region spawned objects have been deleted.");
		}

		[Usage("DelRegionSpawns [<region name>]")]
		[Description("Deletes all spawned objects of the region in which you are (or that you provided) and sets the spawners as not running.")]
		private static void DelRegionSpawns_OnCommand(CommandEventArgs args)
		{
			foreach (var g in GetCommandData(args).GroupBy(s => s.Region))
			{
				foreach (var s in g)
				{
					s.DeleteSpawnedObjects();
				}

				args.Mobile.SendMessage("Spawned objects of region '{0}' have been deleted.", g.Key);
			}
		}

		[Usage("StartAllRegionSpawns")]
		[Description("Sets the region spawners of all regions as running.")]
		private static void StartAllRegionSpawns_OnCommand(CommandEventArgs args)
		{
			foreach (SpawnEntry entry in m_Table.Values)
			{
				entry.Start();
			}

			args.Mobile.SendMessage("All region spawners have started.");
		}

		[Usage("StartRegionSpawns [<region name>]")]
		[Description("Sets the region spawners of the region in which you are (or that you provided) as running.")]
		private static void StartRegionSpawns_OnCommand(CommandEventArgs args)
		{
			foreach (var g in GetCommandData(args).GroupBy(s => s.Region))
			{
				foreach (var s in g)
				{
					s.Start();
				}

				args.Mobile.SendMessage("Spawners of region '{0}' have started.", g.Key);
			}
		}

		[Usage("StopAllRegionSpawns")]
		[Description("Sets the region spawners of all regions as not running.")]
		private static void StopAllRegionSpawns_OnCommand(CommandEventArgs args)
		{
			foreach (SpawnEntry entry in m_Table.Values)
			{
				entry.Stop();
			}

			args.Mobile.SendMessage("All region spawners have stopped.");
		}

		[Usage("StopRegionSpawns [<region name>]")]
		[Description("Sets the region spawners of the region in which you are (or that you provided) as not running.")]
		private static void StopRegionSpawns_OnCommand(CommandEventArgs args)
		{
			foreach (var g in GetCommandData(args).GroupBy(s => s.Region))
			{
				foreach (var s in g)
				{
					s.Stop();
				}

				args.Mobile.SendMessage("Spawners of region '{0}' have stopped.", g.Key);
			}
		}
	}

	public abstract class SpawnDefinition
	{
		protected SpawnDefinition()
		{
		}

		public abstract ISpawnable Spawn(SpawnEntry entry);

		public abstract bool CanSpawn(params Type[] types);
	}

	public abstract class SpawnType : SpawnDefinition
	{
		private readonly Type m_Type;
		private bool m_Init;

		public Type Type => m_Type;

		public abstract int Height { get; }
		public abstract bool Land { get; }
		public abstract bool Water { get; }

		protected SpawnType(Type type)
		{
			m_Type = type;
			m_Init = false;
		}

		protected void EnsureInit()
		{
			if (m_Init)
			{
				return;
			}

			Init();
			m_Init = true;
		}

		protected virtual void Init()
		{
		}

		public override ISpawnable Spawn(SpawnEntry entry)
		{
			var region = entry.Region;
			var map = region.Map;

			var loc = entry.RandomSpawnLocation(Height, Land, Water);

			if (loc == Point3D.Zero)
			{
				return null;
			}

			return Construct(entry, loc, map);
		}

		protected abstract ISpawnable Construct(SpawnEntry entry, Point3D loc, Map map);

		public override bool CanSpawn(params Type[] types)
		{
			for (var i = 0; i < types.Length; i++)
			{
				if (types[i] == m_Type)
				{
					return true;
				}
			}

			return false;
		}
	}

	public class SpawnMobile : SpawnType
	{
		private static readonly Hashtable m_Table = new Hashtable();

		public static SpawnMobile Get(Type type)
		{
			var sm = (SpawnMobile)m_Table[type];

			if (sm == null)
			{
				sm = new SpawnMobile(type);
				m_Table[type] = sm;
			}

			return sm;
		}

		protected bool m_Land;
		protected bool m_Water;

		public override int Height => 16;
		public override bool Land { get { EnsureInit(); return m_Land; } }
		public override bool Water { get { EnsureInit(); return m_Water; } }

		protected SpawnMobile(Type type) : base(type)
		{
		}

		protected override void Init()
		{
			var mob = (Mobile)Activator.CreateInstance(Type);

			m_Land = !mob.CantWalk;
			m_Water = mob.CanSwim;

			mob.Delete();
		}

		protected override ISpawnable Construct(SpawnEntry entry, Point3D loc, Map map)
		{
			var mobile = CreateMobile();

			var creature = mobile as BaseCreature;

			if (creature != null)
			{
				creature.Home = entry.HomeLocation;
				creature.RangeHome = entry.HomeRange;
			}

			if (entry.Direction != SpawnEntry.InvalidDirection)
			{
				mobile.Direction = entry.Direction;
			}

			mobile.OnBeforeSpawn(loc, map);
			mobile.MoveToWorld(loc, map);
			mobile.OnAfterSpawn();

			return mobile;
		}

		protected virtual Mobile CreateMobile()
		{
			return (Mobile)Activator.CreateInstance(Type);
		}
	}

	public class SpawnItem : SpawnType
	{
		private static readonly Hashtable m_Table = new Hashtable();

		public static SpawnItem Get(Type type)
		{
			var si = (SpawnItem)m_Table[type];

			if (si == null)
			{
				si = new SpawnItem(type);
				m_Table[type] = si;
			}

			return si;
		}

		protected int m_Height;

		public override int Height { get { EnsureInit(); return m_Height; } }
		public override bool Land => true;
		public override bool Water => false;

		protected SpawnItem(Type type) : base(type)
		{
		}

		protected override void Init()
		{
			var item = (Item)Activator.CreateInstance(Type);

			m_Height = item.ItemData.Height;

			item.Delete();
		}

		protected override ISpawnable Construct(SpawnEntry entry, Point3D loc, Map map)
		{
			var item = CreateItem();

			item.OnBeforeSpawn(loc, map);
			item.MoveToWorld(loc, map);
			item.OnAfterSpawn();

			return item;
		}

		protected virtual Item CreateItem()
		{
			return (Item)Activator.CreateInstance(Type);
		}
	}

	public class SpawnTreasureChest : SpawnItem
	{
		private readonly int m_ItemID;
		private readonly BaseTreasureChest.TreasureLevel m_Level;

		public int ItemID => m_ItemID;
		public BaseTreasureChest.TreasureLevel Level => m_Level;

		public SpawnTreasureChest(int itemID, BaseTreasureChest.TreasureLevel level) : base(typeof(BaseTreasureChest))
		{
			m_ItemID = itemID;
			m_Level = level;
		}

		protected override void Init()
		{
			m_Height = TileData.ItemTable[m_ItemID & TileData.MaxItemValue].Height;
		}

		protected override Item CreateItem()
		{
			return new BaseTreasureChest(m_ItemID, m_Level);
		}
	}

	public class SpawnGroupElement
	{
		private readonly SpawnDefinition m_SpawnDefinition;
		private readonly int m_Weight;

		public SpawnDefinition SpawnDefinition => m_SpawnDefinition;
		public int Weight => m_Weight;

		public SpawnGroupElement(SpawnDefinition spawnDefinition, int weight)
		{
			m_SpawnDefinition = spawnDefinition;
			m_Weight = weight;
		}
	}

	public class SpawnGroup : SpawnDefinition
	{
		private static readonly (string, Type[])[] m_DefaultGroups =
		{
			#region Groups
						
			#region Town Animals

			("TownAnimals", new[]
			{
				typeof(Cat),
				typeof(Dog),
				typeof(Rat),
			}),

			#endregion
			
			#region Reagents

			("Reagents", new[]
			{
				typeof(BlackPearl),
				typeof(Bloodmoss),
				typeof(Garlic),
				typeof(Ginseng),
				typeof(MandrakeRoot),
				typeof(Nightshade),
				typeof(SulfurousAsh),
				typeof(SpidersSilk),
			}),

			#endregion

			#endregion
		};

		private static readonly Dictionary<string, SpawnGroup> m_Table = new(StringComparer.InvariantCultureIgnoreCase);

		public static Dictionary<string, SpawnGroup> Table => m_Table;

		public static void Register(SpawnGroup group)
		{
			if (m_Table.ContainsKey(group.Name))
			{
				Console.WriteLine("Warning: Double SpawnGroup name '{0}'", group.Name);
			}
			else
			{
				m_Table[group.Name] = group;
			}
		}

		static SpawnGroup()
		{
			var elements = new List<SpawnGroupElement>();

			foreach (var (name, types) in m_DefaultGroups)
			{
				foreach (var type in types)
				{
					SpawnDefinition def = null;

					if (typeof(Mobile).IsAssignableFrom(type))
					{
						def = SpawnMobile.Get(type);
					}
					else if (typeof(Item).IsAssignableFrom(type))
					{
						def = SpawnItem.Get(type);
					}

					if (def != null)
					{
						elements.Add(new SpawnGroupElement(def, 1));
					}
				}

				m_Table[name] = new SpawnGroup(name, elements.ToArray());

				elements.Clear();
			}
			
			elements.TrimExcess();
		}

		private readonly string m_Name;
		private readonly SpawnGroupElement[] m_Elements;
		private readonly int m_TotalWeight;

		public string Name => m_Name;
		public SpawnGroupElement[] Elements => m_Elements;

		public SpawnGroup(string name, SpawnGroupElement[] elements)
		{
			m_Name = name;
			m_Elements = elements;

			m_TotalWeight = 0;
			for (var i = 0; i < elements.Length; i++)
			{
				m_TotalWeight += elements[i].Weight;
			}
		}

		public override ISpawnable Spawn(SpawnEntry entry)
		{
			var index = Utility.Random(m_TotalWeight);

			for (var i = 0; i < m_Elements.Length; i++)
			{
				var element = m_Elements[i];

				if (index < element.Weight)
				{
					return element.SpawnDefinition.Spawn(entry);
				}

				index -= element.Weight;
			}

			return null;
		}

		public override bool CanSpawn(params Type[] types)
		{
			for (var i = 0; i < m_Elements.Length; i++)
			{
				if (m_Elements[i].SpawnDefinition.CanSpawn(types))
				{
					return true;
				}
			}

			return false;
		}
	}

	public class SpawnPersistence : Item
	{
		private static SpawnPersistence m_Instance;

		public SpawnPersistence Instance => m_Instance;

		public static void EnsureExistence()
		{
			if (m_Instance == null)
			{
				m_Instance = new SpawnPersistence();
			}
		}

		public override string DefaultName => "Region spawn persistence - Internal";

		private SpawnPersistence() : base(1)
		{
			Movable = false;
		}

		public SpawnPersistence(Serial serial) : base(serial)
		{
			m_Instance = this;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.Write(SpawnEntry.Table.Values.Count);
			foreach (SpawnEntry entry in SpawnEntry.Table.Values)
			{
				writer.Write(entry.ID);

				entry.Serialize(writer);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();

			var count = reader.ReadInt();
			for (var i = 0; i < count; i++)
			{
				var id = reader.ReadInt();

				var entry = (SpawnEntry)SpawnEntry.Table[id];

				if (entry != null)
				{
					entry.Deserialize(reader, version);
				}
				else
				{
					SpawnEntry.Remove(reader, version);
				}
			}
		}
	}
}