using Server.Commands;
using Server.Engines.Harvest;
using Server.Engines.Weather;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Server.Regions
{
	public enum SpawnZLevel
	{
		Lowest,
		Highest,
		Random
	}

	[Flags]
	public enum RegionFlags : ulong
	{
		None = 0ul,

		AllowCombat = 1ul << 0,
		AllowPvP = 1ul << 1,
		AllowPvM = 1ul << 2,
		AllowMvP = 1ul << 3,
		AllowMvM = 1ul << 4,

		AllowDelayLogout = 1ul << 5,
		AllowParentSpawns = 1ul << 6,
		AllowYoungAggro = 1ul << 7,
		AllowItemDecay = 1ul << 8,
		AllowLogout = 1ul << 9,
		AllowHouses = 1ul << 10,
		AllowVehicles = 1ul << 11,
		AllowSpawning = 1ul << 12,
		AllowFollowers = 1ul << 13,
		AllowEthereal = 1ul << 14,
		AllowMount = 1ul << 15,
		AllowMagic = 1ul << 16,
		AllowMelee = 1ul << 17,
		AllowRanged = 1ul << 18,
		AllowSkills = 1ul << 19,

		AllowPlayerDeath = 1ul << 20,
		AllowPlayerRes = 1ul << 21,
		AllowPlayerHeal = 1ul << 22,
		AllowPlayerHarm = 1ul << 23,
		AllowPlayerLooting = 1ul << 24,
		//PLACEHOLDER = 1ul << 25,
		//PLACEHOLDER = 1ul << 26,
		//PLACEHOLDER = 1ul << 27,
		//PLACEHOLDER = 1ul << 28,
		//PLACEHOLDER = 1ul << 29,

		AllowCreatureDeath = 1ul << 30,
		AllowCreatureRes = 1ul << 31,
		AllowCreatureHeal = 1ul << 32,
		AllowCreatureHarm = 1ul << 33,
		AllowCreatureLooting = 1ul << 34,
		//PLACEHOLDER = 1ul << 35,
		//PLACEHOLDER = 1ul << 36,
		//PLACEHOLDER = 1ul << 37,
		//PLACEHOLDER = 1ul << 38,
		//PLACEHOLDER = 1ul << 39,

		CanEnter = 1ul << 40,
		CanEnterAlive = 1ul << 41,
		CanEnterDead = 1ul << 42,
		CanEnterYoung = 1ul << 43,
		CanEnterInnocent = 1ul << 44,
		CanEnterCriminal = 1ul << 45,
		CanEnterMurderer = 1ul << 46,
		//PLACEHOLDER = 1ul << 47,
		//PLACEHOLDER = 1ul << 48,
		//PLACEHOLDER = 1ul << 49,

		CanExit = 1ul << 50,
		CanExitAlive = 1ul << 51,
		CanExitDead = 1ul << 52,
		CanExitYoung = 1ul << 53,
		CanExitInnocent = 1ul << 54,
		CanExitCriminal = 1ul << 55,
		CanExitMurderer = 1ul << 56,
		//PLACEHOLDER = 1ul << 57,
		//PLACEHOLDER = 1ul << 58,
		//PLACEHOLDER = 1ul << 59,

		FreeMovement = 1ul << 60,
		FreeReagents = 1ul << 61,
		FreeInsurance = 1ul << 62,
		//PLACEHOLDER = 1ul << 63,

		All = ~None,

		/// <summary>
		/// Default rules for new regions
		/// </summary>
		DefaultRules = AllowCombat
					 | AllowPvP
					 | AllowPvM
					 | AllowMvP
					 | AllowMvM
					 //
					 | AllowDelayLogout
					 | AllowItemDecay
					 | AllowLogout
					 | AllowHouses
					 | AllowVehicles
					 | AllowSpawning
					 | AllowFollowers
					 | AllowEthereal
					 | AllowMount
					 | AllowMagic
					 | AllowMelee
					 | AllowRanged
					 | AllowSkills
					 //
					 | AllowPlayerDeath
					 | AllowPlayerRes
					 | AllowPlayerHeal
					 | AllowPlayerHarm
					 | AllowPlayerLooting
					 //
					 | AllowCreatureDeath
					 | AllowCreatureRes
					 | AllowCreatureHeal
					 | AllowCreatureHarm
					 | AllowCreatureLooting
					 //
					 | CanEnter
					 | CanEnterAlive
					 | CanEnterDead
					 | CanEnterYoung
					 | CanEnterInnocent
					 | CanEnterCriminal
					 | CanEnterMurderer
					 //
					 | CanExit
					 | CanExitAlive
					 | CanExitDead
					 | CanExitYoung
					 | CanExitInnocent
					 | CanExitCriminal
					 | CanExitMurderer,
	}

	[NoSort, PropertyObject]
	public abstract class BaseRegionRules
	{
		public RegionFlags Flags { get; set; } = RegionFlags.DefaultRules;

		public bool this[RegionFlags flags] { get => GetFlag(flags); set => SetFlag(flags, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool SetDefaults { get => false; set => Flags = value ? RegionFlags.DefaultRules : Flags; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool SetAllTrue { get => false; set => Flags = value ? RegionFlags.All : Flags; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool SetAllFalse { get => false; set => Flags = value ? RegionFlags.None : Flags; }

		public virtual void Defaults()
		{
			Flags = RegionFlags.DefaultRules;
		}

		public virtual bool GetFlag(RegionFlags flags)
		{
			return Flags.HasFlag(flags);
		}

		public virtual void SetFlag(RegionFlags flags, bool state)
		{
			if (state)
			{
				Flags |= flags;
			}
			else
			{
				Flags &= ~flags;
			}
		}

		public override string ToString()
		{
			return Flags.ToString();
		}
	}

	[NoSort, PropertyObject]
	public class RegionRules : BaseRegionRules
	{
		#region General Combat

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowCombat { get => GetFlag(RegionFlags.AllowCombat); set => SetFlag(RegionFlags.AllowCombat, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowPvP { get => GetFlag(RegionFlags.AllowPvP); set => SetFlag(RegionFlags.AllowPvP, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowPvM { get => GetFlag(RegionFlags.AllowPvM); set => SetFlag(RegionFlags.AllowPvM, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowMvP { get => GetFlag(RegionFlags.AllowMvP); set => SetFlag(RegionFlags.AllowMvP, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowMvM { get => GetFlag(RegionFlags.AllowMvM); set => SetFlag(RegionFlags.AllowMvM, value); }

		#endregion

		#region General Rules

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowDelayLogout { get => GetFlag(RegionFlags.AllowDelayLogout); set => SetFlag(RegionFlags.AllowDelayLogout, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowParentSpawns { get => GetFlag(RegionFlags.AllowParentSpawns); set => SetFlag(RegionFlags.AllowParentSpawns, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowYoungAggro { get => GetFlag(RegionFlags.AllowYoungAggro); set => SetFlag(RegionFlags.AllowYoungAggro, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowItemDecay { get => GetFlag(RegionFlags.AllowItemDecay); set => SetFlag(RegionFlags.AllowItemDecay, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowLogout { get => GetFlag(RegionFlags.AllowLogout); set => SetFlag(RegionFlags.AllowLogout, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowHouses { get => GetFlag(RegionFlags.AllowHouses); set => SetFlag(RegionFlags.AllowHouses, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowVehicles { get => GetFlag(RegionFlags.AllowVehicles); set => SetFlag(RegionFlags.AllowVehicles, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowSpawning { get => GetFlag(RegionFlags.AllowSpawning); set => SetFlag(RegionFlags.AllowSpawning, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowFollowers { get => GetFlag(RegionFlags.AllowFollowers); set => SetFlag(RegionFlags.AllowFollowers, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowEthereal { get => GetFlag(RegionFlags.AllowEthereal); set => SetFlag(RegionFlags.AllowEthereal, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowMount { get => GetFlag(RegionFlags.AllowMount); set => SetFlag(RegionFlags.AllowMount, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowMagic { get => GetFlag(RegionFlags.AllowMagic); set => SetFlag(RegionFlags.AllowMagic, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowMelee { get => GetFlag(RegionFlags.AllowMelee); set => SetFlag(RegionFlags.AllowMelee, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowRanged { get => GetFlag(RegionFlags.AllowRanged); set => SetFlag(RegionFlags.AllowRanged, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowSkills { get => GetFlag(RegionFlags.AllowSkills); set => SetFlag(RegionFlags.AllowSkills, value); }

		#endregion

		#region Player Action Rules

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowPlayerDeath { get => GetFlag(RegionFlags.AllowPlayerDeath); set => SetFlag(RegionFlags.AllowPlayerDeath, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowPlayerRes { get => GetFlag(RegionFlags.AllowPlayerRes); set => SetFlag(RegionFlags.AllowPlayerRes, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowPlayerHeal { get => GetFlag(RegionFlags.AllowPlayerHeal); set => SetFlag(RegionFlags.AllowPlayerHeal, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowPlayerHarm { get => GetFlag(RegionFlags.AllowPlayerHarm); set => SetFlag(RegionFlags.AllowPlayerHarm, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowPlayerLooting { get => GetFlag(RegionFlags.AllowPlayerLooting); set => SetFlag(RegionFlags.AllowPlayerLooting, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		#endregion

		#region Creature Action Rules

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowCreatureDeath { get => GetFlag(RegionFlags.AllowCreatureDeath); set => SetFlag(RegionFlags.AllowCreatureDeath, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowCreatureRes { get => GetFlag(RegionFlags.AllowCreatureRes); set => SetFlag(RegionFlags.AllowCreatureRes, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowCreatureHeal { get => GetFlag(RegionFlags.AllowCreatureHeal); set => SetFlag(RegionFlags.AllowCreatureHeal, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowCreatureHarm { get => GetFlag(RegionFlags.AllowCreatureHarm); set => SetFlag(RegionFlags.AllowCreatureHarm, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool AllowCreatureLooting { get => GetFlag(RegionFlags.AllowCreatureLooting); set => SetFlag(RegionFlags.AllowCreatureLooting, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		#endregion

		#region Enter Rules

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanEnter { get => GetFlag(RegionFlags.CanEnter); set => SetFlag(RegionFlags.CanEnter, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanEnterAlive { get => GetFlag(RegionFlags.CanEnterAlive); set => SetFlag(RegionFlags.CanEnterAlive, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanEnterDead { get => GetFlag(RegionFlags.CanEnterDead); set => SetFlag(RegionFlags.CanEnterDead, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanEnterYoung { get => GetFlag(RegionFlags.CanEnterYoung); set => SetFlag(RegionFlags.CanEnterYoung, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanEnterInnocent { get => GetFlag(RegionFlags.CanEnterInnocent); set => SetFlag(RegionFlags.CanEnterInnocent, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanEnterCriminal { get => GetFlag(RegionFlags.CanEnterCriminal); set => SetFlag(RegionFlags.CanEnterCriminal, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanEnterMurderer { get => GetFlag(RegionFlags.CanEnterMurderer); set => SetFlag(RegionFlags.CanEnterMurderer, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		#endregion

		#region Exit Rules

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanExit { get => GetFlag(RegionFlags.CanExit); set => SetFlag(RegionFlags.CanExit, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanExitAlive { get => GetFlag(RegionFlags.CanExitAlive); set => SetFlag(RegionFlags.CanExitAlive, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanExitDead { get => GetFlag(RegionFlags.CanExitDead); set => SetFlag(RegionFlags.CanExitDead, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanExitYoung { get => GetFlag(RegionFlags.CanExitYoung); set => SetFlag(RegionFlags.CanExitYoung, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanExitInnocent { get => GetFlag(RegionFlags.CanExitInnocent); set => SetFlag(RegionFlags.CanExitInnocent, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanExitCriminal { get => GetFlag(RegionFlags.CanExitCriminal); set => SetFlag(RegionFlags.CanExitCriminal, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CanExitMurderer { get => GetFlag(RegionFlags.CanExitMurderer); set => SetFlag(RegionFlags.CanExitMurderer, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		#endregion

		#region Passive Rules

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool FreeMovement { get => GetFlag(RegionFlags.FreeMovement); set => SetFlag(RegionFlags.FreeMovement, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool FreeReagents { get => GetFlag(RegionFlags.FreeReagents); set => SetFlag(RegionFlags.FreeReagents, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool FreeInsurance { get => GetFlag(RegionFlags.FreeInsurance); set => SetFlag(RegionFlags.FreeInsurance, value); }

		//[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		//public bool PLACEHOLDER { get => GetFlag(RegionFlags.PLACEHOLDER); set => SetFlag(RegionFlags.PLACEHOLDER, value); }

		#endregion
	}

	public abstract class BaseRegion : Region
	{
		private static readonly List<Poly3D> m_RectBuffer1 = new();
		private static readonly List<Poly3D> m_RectBuffer2 = new();

		private static readonly List<int> m_SpawnBuffer1 = new();
		private static readonly List<Item> m_SpawnBuffer2 = new();

		public static void Configure()
		{
			DefaultRegionType = typeof(GenericRegion);
		}

		public static string GetRuneNameFor(Region region)
		{
			while (region != null)
			{
				if (region is BaseRegion br && br.RuneName != null)
				{
					return br.RuneName;
				}

				region = region.Parent;
			}

			return null;
		}

		public static bool CanSpawn(Region region, params Type[] types)
		{
			while (region != null)
			{
				if (!region.AllowSpawn())
				{
					return false;
				}

				if (region is BaseRegion br)
				{
					if (br.Spawns != null)
					{
						for (var i = 0; i < br.Spawns.Length; i++)
						{
							var entry = br.Spawns[i];

							if (entry.Definition.CanSpawn(types))
							{
								return true;
							}
						}
					}

					if (!br.Rules.AllowParentSpawns)
					{
						return false;
					}
				}

				region = region.Parent;
			}

			return false;
		}

		public static IEnumerable<TypeAmount> EnumerateHarvestNodes(Region region, Type harvest, bool active, bool inherited)
		{
			return internalEnumerate(region, harvest, active, inherited).DistinctBy(e => HashCode.Combine(e.Type, e.Amount)).OrderBy(e => e.Amount);

			static IEnumerable<TypeAmount> internalEnumerate(Region r, Type t, bool a, bool i)
			{
				if (r?.Deleted != false || t == null)
				{
					yield break;
				}

				while (r != null)
				{
					if (r is BaseRegion br)
					{
						var node = br.HarvestNodes[t];

						if (node?.Count > 0)
						{
							var entries = a ? node.ActiveEntries : node.ValidEntries;

							foreach (var entry in entries)
							{
								yield return entry;
							}
						}
					}

					if (!i)
					{
						break;
					}

					r = r.Parent;
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public string RuneName { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Currencies Currencies { get; protected set; } = new();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public HarvestNodes HarvestNodes { get; protected set; } = new();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public SkillPermissions SkillPermissions { get; protected set; } = new();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public SpellPermissions SpellPermissions { get; protected set; } = new();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public RegionRules Rules { get; protected set; } = new();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public AccessLevel RulesOverride { get; set; } = AccessLevel.GameMaster;

		#region Legacy Compatibility Properties

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool HousingAllowed { get => Rules.AllowHouses; set => Rules.AllowHouses = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool YoungProtected { get => !Rules.AllowYoungAggro; set => Rules.AllowYoungAggro = !value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool YoungMayEnter { get => Rules.CanEnterYoung; set => Rules.CanEnterYoung = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool MountsAllowed { get => Rules.AllowMount; set => Rules.AllowMount = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool DeadMayEnter { get => Rules.CanEnterDead; set => Rules.CanEnterDead = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool ResurrectionAllowed { get => Rules.AllowPlayerRes; set => Rules.AllowPlayerRes = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool LogoutAllowed { get => Rules.AllowLogout; set => Rules.AllowLogout = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoLogoutDelay { get => !Rules.AllowDelayLogout; set => Rules.AllowDelayLogout = !value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool ExcludeFromParentSpawns { get => !Rules.AllowParentSpawns; set => Rules.AllowParentSpawns = !value; }

		#endregion

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public SpawnZLevel SpawnZLevel { get; set; }

		private SpawnEntry[] m_Spawns;

		public SpawnEntry[] Spawns
		{
			get => m_Spawns;
			set
			{
				if (m_Spawns != null)
				{
					for (var i = 0; i < m_Spawns.Length; i++)
					{
						m_Spawns[i]?.Delete();
					}
				}

				m_Spawns = value;
			}
		}

		private Poly3D[] m_Areas;
		private int[] m_RectangleWeights;
		private int m_TotalWeight;

		public virtual bool WeatherSupported => Weather != null;

		public virtual int DefaultTemperature => 15;
		public virtual int DefaultPercipitationChance => 50;
		public virtual int DefaultExtremeTemperatureChance => 5;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public RegionalWeather Weather { get; set; }

		public BaseRegion(string name, Map map, int priority, params Rectangle2D[] area) : base(name, map, priority, area)
		{
		}

		public BaseRegion(string name, Map map, int priority, params Poly2D[] area) : base(name, map, priority, area)
		{
		}

		public BaseRegion(string name, Map map, int priority, params Rectangle3D[] area) : base(name, map, priority, area)
		{
		}

		public BaseRegion(string name, Map map, int priority, params Poly3D[] area) : base(name, map, priority, area)
		{
		}

		public BaseRegion(string name, Map map, Region parent, params Rectangle2D[] area) : base(name, map, parent, area)
		{
		}

		public BaseRegion(string name, Map map, Region parent, params Poly2D[] area) : base(name, map, parent, area)
		{
		}

		public BaseRegion(string name, Map map, Region parent, params Rectangle3D[] area) : base(name, map, parent, area)
		{
		}

		public BaseRegion(string name, Map map, Region parent, params Poly3D[] area) : base(name, map, parent, area)
		{
		}

		public BaseRegion(RegionDefinition def, Map map, Region parent) : base(def, map, parent)
		{
		}

		public BaseRegion(int id) : base(id)
		{ 
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			SpellPermissions.SetAll(true);
			SkillPermissions.SetAll(true);

			Rules.Defaults();

			SpawnZLevel = SpawnZLevel.Lowest;

			if (Weather != null)
			{
				Weather.Temperature = DefaultTemperature;
				Weather.ChanceOfPercipitation = DefaultPercipitationChance;
				Weather.ChanceOfExtremeTemperature = DefaultExtremeTemperatureChance;
			}
			else 
			{
				WeatherInit();
			}
		}

		protected virtual void WeatherInit()
		{
			if (WeatherSupported)
			{
				Weather ??= new RegionalWeather(this);
			}
		}

		protected virtual void WeatherUpdate()
		{
			WeatherInit();

			Weather?.Update();
		}

		public override void OnRegister()
		{
			base.OnRegister();

			WeatherUpdate();
		}

		public override TimeSpan GetLogoutDelay(Mobile m)
		{
			if (m.AccessLevel < RulesOverride)
			{
				if (!Rules.AllowDelayLogout && m.Aggressors.Count == 0 && m.Aggressed.Count == 0 && !m.Criminal)
				{
					if (!OnRuleEnforced(RegionFlags.AllowDelayLogout, m, m, false))
					{
						return TimeSpan.Zero;
					}
				}
			}

			return base.GetLogoutDelay(m);
		}

		public override bool AllowSpawn()
		{
			if (!Rules.AllowSpawning)
			{
				if (!OnRuleEnforced(RegionFlags.AllowSpawning, this, this, false))
				{
					return false;
				}
			}

			return base.AllowSpawn();
		}

		public virtual bool AllowInteraction(ref Mobile from, ref Mobile target)
		{
			while (from is BaseCreature fbc && fbc.ControlMaster != null)
			{
				from = fbc.ControlMaster;
			}

			while (target is BaseCreature fbt && fbt.ControlMaster != null)
			{
				target = fbt.ControlMaster;
			}

			if (from.AccessLevel >= RulesOverride)
			{
				return true;
			}

			if (!Rules.AllowCombat)
			{
				if (!OnRuleEnforced(RegionFlags.AllowCombat, from, target, false))
				{
					return false;
				}
			}

			if (from.Player && target.Player)
			{
				if (!Rules.AllowPvP)
				{
					if (!OnRuleEnforced(RegionFlags.AllowPvP, from, target, false))
					{
						return false;
					}
				}
			}
			else if (from.Player && !target.Player)
			{
				if (!Rules.AllowPvM)
				{
					if (!OnRuleEnforced(RegionFlags.AllowPvM, from, target, false))
					{
						return false;
					}
				}
			}
			else if (!from.Player && target.Player)
			{
				if (!Rules.AllowMvP)
				{
					if (!OnRuleEnforced(RegionFlags.AllowMvP, from, target, false))
					{
						return false;
					}
				}
			}
			else if (!from.Player && !target.Player)
			{
				if (!Rules.AllowMvM)
				{
					if (!OnRuleEnforced(RegionFlags.AllowMvM, from, target, false))
					{
						return false;
					}
				}
			}

			return true;
		}

		public override bool AllowBeneficial(Mobile from, Mobile target)
		{
			if (!AllowInteraction(ref from, ref target))
			{
				return false;
			}

			if (from.AccessLevel < RulesOverride)
			{
				if (target.Player)
				{
					if (!Rules.AllowPlayerHeal)
					{
						if (!OnRuleEnforced(RegionFlags.AllowPlayerHeal, from, target, false))
						{
							return false;
						}
					}
				}
				else
				{
					if (!Rules.AllowCreatureHeal)
					{
						if (!OnRuleEnforced(RegionFlags.AllowCreatureHeal, from, target, false))
						{
							return false;
						}
					}
				}
			}

			return base.AllowBeneficial(from, target);
		}

		public override bool AllowHarmful(Mobile from, Mobile target)
		{
			if (!AllowInteraction(ref from, ref target))
			{
				return false;
			}

			if (from.AccessLevel < RulesOverride)
			{
				if (target.Player)
				{
					if (!Rules.AllowPlayerHarm)
					{
						if (!OnRuleEnforced(RegionFlags.AllowPlayerHarm, from, target, false))
						{
							return false;
						}
					}
				}
				else
				{
					if (!Rules.AllowCreatureHarm)
					{
						if (!OnRuleEnforced(RegionFlags.AllowCreatureHarm, from, target, false))
						{
							return false;
						}
					}
				}
			}

			return base.AllowHarmful(from, target);
		}

		public override bool OnBeforeDeath(Mobile m)
		{
			if (m.AccessLevel < RulesOverride)
			{
				if (m.Player)
				{
					if (!Rules.AllowPlayerDeath)
					{
						if (!OnRuleEnforced(RegionFlags.AllowPlayerDeath, m, m, false))
						{
							return false;
						}
					}
				}
				else
				{
					if (!Rules.AllowCreatureDeath)
					{
						if (!OnRuleEnforced(RegionFlags.AllowCreatureDeath, m, m, false))
						{
							return false;
						}
					}
				}
			}

			return base.OnBeforeDeath(m);
		}

		public override bool OnResurrect(Mobile m)
		{
			if (m.AccessLevel < RulesOverride)
			{
				if (m.Player)
				{
					if (!Rules.AllowPlayerRes)
					{
						if (!OnRuleEnforced(RegionFlags.AllowPlayerRes, m, m, false))
						{
							return false;
						}
					}
				}
				else
				{
					if (!Rules.AllowCreatureRes)
					{
						if (!OnRuleEnforced(RegionFlags.AllowCreatureRes, m, m, false))
						{
							return false;
						}
					}
				}
			}

			return base.OnResurrect(m);
		}

		public override bool OnBeginSpellCast(Mobile m, ISpell s)
		{
			if (m.AccessLevel < RulesOverride)
			{
				if (!Rules.AllowMagic || !SpellPermissions[s.ID])
				{
					if (!OnRuleEnforced(RegionFlags.AllowMagic, m, s, false))
					{
						return false;
					}
				}
			}

			return base.OnBeginSpellCast(m, s);
		}

		public override bool OnDecay(Item item)
		{
			if (!Rules.AllowItemDecay)
			{
				if (!OnRuleEnforced(RegionFlags.AllowItemDecay, item, item, false))
				{
					return false;
				}
			}

			return base.OnDecay(item);
		}

		public override bool OnSkillUse(Mobile m, int skill)
		{
			if (m.AccessLevel < RulesOverride)
			{
				if (!Rules.AllowSkills || !SkillPermissions[skill])
				{
					if (!OnRuleEnforced(RegionFlags.AllowSkills, m, skill, false))
					{
						return false;
					}
				}
			}

			return base.OnSkillUse(m, skill);
		}

		public override bool OnDoubleClick(Mobile m, object o)
		{
			if (m.AccessLevel < RulesOverride)
			{
				if (o is Corpse c)
				{
					if (c.Owner != m)
					{
						if (c.Owner != null && c.Owner.Player)
						{
							if (!Rules.AllowPlayerLooting)
							{
								if (!OnRuleEnforced(RegionFlags.AllowPlayerLooting, m, o, false))
								{
									return false;
								}
							}
						}
						else
						{
							if (!Rules.AllowCreatureLooting)
							{
								if (!OnRuleEnforced(RegionFlags.AllowCreatureLooting, m, o, false))
								{
									return false;
								}
							}
						}
					}
				}
				else if (o is EtherealMount em)
				{
					if (!Rules.AllowEthereal)
					{
						if (!OnRuleEnforced(RegionFlags.AllowEthereal, m, em, false))
						{
							return false;
						}
					}
				}
				else if (o is BaseMount bm)
				{
					if (bm.Rider == null && (bm.GetMaster() == m || bm.IsPetFriend(m)))
					{
						if (!Rules.AllowMount)
						{
							if (!OnRuleEnforced(RegionFlags.AllowMount, m, bm, false))
							{
								return false;
							}
						}
					}
				}
				else if (o == m)
				{
					if (m.Mount is IMount im)
					{
						if (!Rules.AllowMount)
						{
							if (!OnRuleEnforced(RegionFlags.AllowMount, m, im, false))
							{
								return false;
							}
						}
					}
				}
			}

			return base.OnDoubleClick(m, o);
		}

		public override bool CheckAccessibility(Item item, Mobile from)
		{
			if (from.AccessLevel < RulesOverride)
			{
				if (item.RootParent is Corpse c && c.Owner != from)
				{
					if (c.Owner != null && c.Owner.Player)
					{
						if (!Rules.AllowPlayerLooting)
						{
							if (!OnRuleEnforced(RegionFlags.AllowPlayerLooting, from, item, false))
							{
								return false;
							}
						}
					}
					else
					{
						if (!Rules.AllowCreatureLooting)
						{
							if (!OnRuleEnforced(RegionFlags.AllowCreatureLooting, from, item, false))
							{
								return false;
							}
						}
					}
				}
			}

			return base.CheckAccessibility(item, from);
		}

		public override bool AcceptsSpawnsFrom(Mobile spawn, Region region)
		{
			if (region != this)
			{
				if (!Rules.AllowParentSpawns)
				{
					if (!OnRuleEnforced(RegionFlags.AllowParentSpawns, spawn, region, false))
					{
						return false;
					}
				}
			}

			return base.AcceptsSpawnsFrom(spawn, region);
		}

		public override bool AllowHousing(Mobile owner, Point3D p)
		{
			if (owner == null || owner.AccessLevel < RulesOverride)
			{
				if (!Rules.AllowHouses)
				{
					if (!OnRuleEnforced(RegionFlags.AllowHouses, owner, p, false))
					{
						return false;
					}
				}
			}

			return base.AllowHousing(owner, p);
		}

		public override bool AllowVehicles(Mobile owner, Point3D p)
		{
			if (owner == null || owner.AccessLevel < RulesOverride)
			{
				if (!Rules.AllowVehicles)
				{
					if (!OnRuleEnforced(RegionFlags.AllowVehicles, owner, p, false))
					{
						return false;
					}
				}
			}

			return base.AllowVehicles(owner, p);
		}

		public override bool CanEnter(Mobile m)
		{
			if (m.AccessLevel < RulesOverride)
			{
				if (!Rules.CanEnter)
				{
					if (!OnRuleEnforced(RegionFlags.CanEnter, m, m, false))
					{
						return false;
					}
				}

				if (m.Alive)
				{
					if (!Rules.CanEnterAlive)
					{
						if (!OnRuleEnforced(RegionFlags.CanEnterAlive, m, m, false))
						{
							return false;
						}
					}
				}
				else
				{
					if (!Rules.CanEnterDead)
					{
						if (!OnRuleEnforced(RegionFlags.CanEnterDead, m, m, false))
						{
							return false;
						}
					}
				}

				if (m.Murderer)
				{
					if (!Rules.CanEnterMurderer)
					{
						if (!OnRuleEnforced(RegionFlags.CanEnterMurderer, m, m, false))
						{
							return false;
						}
					}
				}
				else
				{
					if (!Rules.CanEnterInnocent)
					{
						if (!OnRuleEnforced(RegionFlags.CanEnterInnocent, m, m, false))
						{
							return false;
						}
					}
				}

				if (m.Criminal)
				{
					if (!Rules.CanEnterCriminal)
					{
						if (!OnRuleEnforced(RegionFlags.CanEnterCriminal, m, m, false))
						{
							return false;
						}
					}
				}

				if (m is PlayerMobile pm)
				{
					if (pm.Young)
					{
						if (!Rules.CanEnterYoung)
						{
							if (!OnRuleEnforced(RegionFlags.CanEnterYoung, pm, pm, false))
							{
								return false;
							}
						}
					}

					if (pm.Followers > 0)
					{
						if (!Rules.AllowFollowers)
						{
							if (!OnRuleEnforced(RegionFlags.AllowFollowers, pm, pm, false))
							{
								return false;
							}
						}
					}
				}

				if (m is BaseCreature bc)
				{
					if (bc.GetMaster() is PlayerMobile mpm)
					{
						if (!Rules.AllowFollowers)
						{
							if (!OnRuleEnforced(RegionFlags.AllowFollowers, mpm, bc, false))
							{
								return false;
							}
						}
					}
				}

				if (m.Mount is EtherealMount em)
				{
					if (!Rules.AllowEthereal)
					{
						if (!OnRuleEnforced(RegionFlags.AllowEthereal, m, em, false))
						{
							em.Rider = null;
							//return false;
						}
					}
				}
				else if (m.Mount is IMount im)
				{
					if (!Rules.AllowMount)
					{
						if (!OnRuleEnforced(RegionFlags.AllowMount, m, im, false))
						{
							im.Rider = null;
							//return false;
						}
					}
				}
			}

			return base.CanEnter(m);
		}

		public override bool CanExit(Mobile m)
		{
			if (m.AccessLevel < RulesOverride)
			{
				if (!Rules.CanExit)
				{
					if (!OnRuleEnforced(RegionFlags.CanExit, m, m, false))
					{
						return false;
					}
				}

				if (m.Alive)
				{
					if (!Rules.CanExitAlive)
					{
						if (!OnRuleEnforced(RegionFlags.CanExitAlive, m, m, false))
						{
							return false;
						}
					}
				}
				else
				{
					if (!Rules.CanExitDead)
					{
						if (!OnRuleEnforced(RegionFlags.CanExitDead, m, m, false))
						{
							return false;
						}
					}
				}

				if (m.Murderer)
				{
					if (!Rules.CanExitMurderer)
					{
						if (!OnRuleEnforced(RegionFlags.CanExitMurderer, m, m, false))
						{
							return false;
						}
					}
				}
				else
				{
					if (!Rules.CanExitInnocent)
					{
						if (!OnRuleEnforced(RegionFlags.CanExitInnocent, m, m, false))
						{
							return false;
						}
					}
				}

				if (m.Criminal)
				{
					if (!Rules.CanExitCriminal)
					{
						if (!OnRuleEnforced(RegionFlags.CanExitCriminal, m, m, false))
						{
							return false;
						}
					}
				}

				if (m is PlayerMobile pm && pm.Young)
				{
					if (!Rules.CanExitYoung)
					{
						if (!OnRuleEnforced(RegionFlags.CanExitYoung, pm, pm, false))
						{
							return false;
						}
					}
				}
			}

			return base.CanExit(m);
		}

		public override void OnEnter(Mobile m)
		{
			base.OnEnter(m);

			if (m.AccessLevel < RulesOverride)
			{
				if (m is PlayerMobile pm && pm.Young)
				{
					if (Rules.AllowYoungAggro)
					{
						if (OnRuleEnforced(RegionFlags.AllowYoungAggro, pm, pm, true))
						{
							pm.SendGump(new YoungDungeonWarning());
						}
					}
				}
			}
		}

		public virtual bool OnRuleEnforced(RegionFlags rule, object src, object trg, bool result)
		{
			if (!result && src is Mobile m && m.NetState != null)
			{
				var message = GetRuleEnforcementMessage(rule);

				if (!String.IsNullOrWhiteSpace(message))
				{
					m.PrivateOverheadMessage(Network.MessageType.Regular, 0x22, false, message, m.NetState);
				}
			}

			return result;
		}

		public virtual string GetRuleEnforcementMessage(RegionFlags rule)
		{
			return rule switch
			{
				RegionFlags.AllowLogout => "You cannot log-out here.",
				RegionFlags.AllowHouses => "You cannot place houses here.",
				RegionFlags.AllowVehicles => "You cannot place vehicles here.",
				RegionFlags.AllowSpawning => "You cannot spawn here.",
				RegionFlags.AllowFollowers => "You cannot bring followers here.",
				RegionFlags.AllowEthereal => "You cannot ride ethereal mounts here.",
				RegionFlags.AllowMount => "You cannot ride mounts here.",
				RegionFlags.AllowMagic => "You cannot use magic here.",
				RegionFlags.AllowMelee => "You cannot use melee weapons here.",
				RegionFlags.AllowRanged => "You cannot use ranged weapons here.",
				RegionFlags.AllowSkills => "You cannot directly use skills here.",

				RegionFlags.AllowPlayerDeath => "You cannot kill players here.",
				RegionFlags.AllowPlayerRes => "You cannot resurrect players here.",
				RegionFlags.AllowPlayerHeal => "You cannot heal players here.",
				RegionFlags.AllowPlayerHarm => "You cannot harm players here.",
				RegionFlags.AllowPlayerLooting => "You cannot loot players here.",

				RegionFlags.AllowCreatureDeath => "You cannot kill creatures here.",
				RegionFlags.AllowCreatureRes => "You cannot resurrect creatures here.",
				RegionFlags.AllowCreatureHeal => "You cannot heal creatures here.",
				RegionFlags.AllowCreatureHarm => "You cannot harm creatures here.",
				RegionFlags.AllowCreatureLooting => "You cannot loot creatures here.",

				RegionFlags.CanEnter => "You cannot enter.",
				RegionFlags.CanEnterAlive => "You cannot enter while alive.",
				RegionFlags.CanEnterDead => "You cannot enter while dead.",
				RegionFlags.CanEnterYoung => "You cannot enter while young.",
				RegionFlags.CanEnterInnocent => "You cannot enter while innocent.",
				RegionFlags.CanEnterCriminal => "You cannot enter while criminal.",
				RegionFlags.CanEnterMurderer => "You cannot enter while murderer.",

				RegionFlags.CanExit => "You cannot leave.",
				RegionFlags.CanExitAlive => "You cannot leave while alive.",
				RegionFlags.CanExitDead => "You cannot leave while dead.",
				RegionFlags.CanExitYoung => "You cannot leave while young.",
				RegionFlags.CanExitInnocent => "You cannot leave while innocent.",
				RegionFlags.CanExitCriminal => "You cannot leave while criminal.",
				RegionFlags.CanExitMurderer => "You cannot leave while murderer.",

				_ => null,
			};
		}

		private void InitArea()
		{
			if (m_Areas != null)
			{
				return;
			}

			// Test if area rectangles are overlapping, and in that case break them into smaller non overlapping rectangles
			for (var i = 0; i < Area.Length; i++)
			{
				m_RectBuffer2.Add(Area[i]);

				for (var j = 0; j < m_RectBuffer1.Count && m_RectBuffer2.Count > 0; j++)
				{
					var comp = m_RectBuffer1[j];

					for (var k = m_RectBuffer2.Count - 1; k >= 0; k--)
					{
						var rect = m_RectBuffer2[k];

						int l1 = rect.Bounds.Start.X, r1 = rect.Bounds.End.X, t1 = rect.Bounds.Start.Y, b1 = rect.Bounds.End.Y;
						int l2 = comp.Bounds.Start.X, r2 = comp.Bounds.End.X, t2 = comp.Bounds.Start.Y, b2 = comp.Bounds.End.Y;

						if (l1 < r2 && r1 > l2 && t1 < b2 && b1 > t2)
						{
							m_RectBuffer2.RemoveAt(k);

							var sz = rect.MinZ;
							var ez = rect.MaxZ;

							if (l1 < l2)
							{
								m_RectBuffer2.Add(new Rectangle3D(new Point3D(l1, t1, sz), new Point3D(l2, b1, ez)));
							}

							if (r1 > r2)
							{
								m_RectBuffer2.Add(new Rectangle3D(new Point3D(r2, t1, sz), new Point3D(r1, b1, ez)));
							}

							if (t1 < t2)
							{
								m_RectBuffer2.Add(new Rectangle3D(new Point3D(Math.Max(l1, l2), t1, sz), new Point3D(Math.Min(r1, r2), t2, ez)));
							}

							if (b1 > b2)
							{
								m_RectBuffer2.Add(new Rectangle3D(new Point3D(Math.Max(l1, l2), b2, sz), new Point3D(Math.Min(r1, r2), b1, ez)));
							}
						}
					}
				}

				m_RectBuffer1.AddRange(m_RectBuffer2);
				m_RectBuffer2.Clear();
			}

			m_Areas = m_RectBuffer1.ToArray();
			m_RectBuffer1.Clear();

			m_RectangleWeights = new int[m_Areas.Length];

			for (var i = 0; i < m_Areas.Length; i++)
			{
				var rect = m_Areas[i];
				var weight = rect.Bounds.Width * rect.Bounds.Height;

				m_RectangleWeights[i] = weight;
				m_TotalWeight += weight;
			}
		}

		public Point3D RandomSpawnLocation(int spawnHeight, bool land, bool water, Point3D home, int range)
		{
			var map = Map;

			if (map == Map.Internal)
			{
				return Point3D.Zero;
			}

			InitArea();

			if (m_TotalWeight <= 0)
			{
				return Point3D.Zero;
			}

			for (var i = 0; i < 100; i++)
			{
				int x, y, minZ, maxZ;

				if (home == Point3D.Zero)
				{
					var rand = Utility.Random(m_TotalWeight);

					x = Int32.MinValue; y = Int32.MinValue;
					minZ = Int32.MaxValue; maxZ = Int32.MinValue;

					for (var j = 0; j < m_RectangleWeights.Length; j++)
					{
						var curWeight = m_RectangleWeights[j];

						if (rand < curWeight)
						{
							var poly = m_Areas[j];
							var rect = poly.Bounds;

							x = rect.Start.X + rand % rect.Width;
							y = rect.Start.Y + rand / rect.Width;

							if (poly.Contains(x, y))
							{
								minZ = poly.MinZ;
								maxZ = poly.MaxZ;
								break;
							}
						}

						rand -= curWeight;
					}
				}
				else
				{
					x = Utility.RandomMinMax(home.X - range, home.X + range);
					y = Utility.RandomMinMax(home.Y - range, home.Y + range);

					minZ = Int32.MaxValue; maxZ = Int32.MinValue;

					for (var j = 0; j < Area.Length; j++)
					{
						var poly = Area[j];

						if (poly.Contains(x, y))
						{
							minZ = poly.MinZ;
							maxZ = poly.MaxZ;
							break;
						}
					}

					if (minZ == Int32.MaxValue)
					{
						continue;
					}
				}

				if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
				{
					continue;
				}

				var lt = map.Tiles.GetLandTile(x, y);

				int ltLowZ = 0, ltAvgZ = 0, ltTopZ = 0;

				map.GetAverageZ(x, y, ref ltLowZ, ref ltAvgZ, ref ltTopZ);

				var ltFlags = TileData.LandTable[lt.ID & TileData.MaxLandValue].Flags;
				var ltImpassable = ((ltFlags & TileFlag.Impassable) != 0);

				if (!lt.Ignored && ltAvgZ >= minZ && ltAvgZ < maxZ)
				{
					if ((ltFlags & TileFlag.Wet) != 0)
					{
						if (water)
						{
							m_SpawnBuffer1.Add(ltAvgZ);
						}
					}
					else if (land && !ltImpassable)
					{
						m_SpawnBuffer1.Add(ltAvgZ);
					}
				}

				var staticTiles = map.Tiles.GetStaticTiles(x, y, true);

				for (var j = 0; j < staticTiles.Length; j++)
				{
					var tile = staticTiles[j];
					var id = TileData.ItemTable[tile.ID & TileData.MaxItemValue];
					var tileZ = tile.Z + id.CalcHeight;

					if (tileZ >= minZ && tileZ < maxZ)
					{
						if ((id.Flags & TileFlag.Wet) != 0)
						{
							if (water)
							{
								m_SpawnBuffer1.Add(tileZ);
							}
						}
						else if (land && id.Surface && !id.Impassable)
						{
							m_SpawnBuffer1.Add(tileZ);
						}
					}
				}

				var sector = map.GetSector(x, y);

				for (var j = 0; j < sector.Items.Count; j++)
				{
					var item = sector.Items[j];

					if (item is not BaseMulti && item.ItemID <= TileData.MaxItemValue && item.AtWorldPoint(x, y))
					{
						m_SpawnBuffer2.Add(item);

						if (!item.Movable)
						{
							var id = item.ItemData;
							var itemZ = item.Z + id.CalcHeight;

							if (itemZ >= minZ && itemZ < maxZ)
							{
								if ((id.Flags & TileFlag.Wet) != 0)
								{
									if (water)
									{
										m_SpawnBuffer1.Add(itemZ);
									}
								}
								else if (land && id.Surface && !id.Impassable)
								{
									m_SpawnBuffer1.Add(itemZ);
								}
							}
						}
					}
				}

				if (m_SpawnBuffer1.Count == 0)
				{
					m_SpawnBuffer1.Clear();
					m_SpawnBuffer2.Clear();
					continue;
				}

				int z;

				switch (SpawnZLevel)
				{
					case SpawnZLevel.Lowest:
						{
							z = Int32.MaxValue;

							for (var j = 0; j < m_SpawnBuffer1.Count; j++)
							{
								var l = m_SpawnBuffer1[j];

								if (l < z)
								{
									z = l;
								}
							}

							break;
						}
					case SpawnZLevel.Highest:
						{
							z = Int32.MinValue;

							for (var j = 0; j < m_SpawnBuffer1.Count; j++)
							{
								var l = m_SpawnBuffer1[j];

								if (l > z)
								{
									z = l;
								}
							}

							break;
						}
					default: // SpawnZLevel.Random
						{
							var index = Utility.Random(m_SpawnBuffer1.Count);

							z = m_SpawnBuffer1[index];

							break;
						}
				}

				m_SpawnBuffer1.Clear();

				var r = Find(new Point3D(x, y, z), map);

				if (!r.AcceptsSpawnsFrom(null, this))
				{
					m_SpawnBuffer2.Clear();
					continue;
				}

				var top = z + spawnHeight;

				var ok = true;

				for (var j = 0; j < m_SpawnBuffer2.Count; j++)
				{
					var item = m_SpawnBuffer2[j];
					var id = item.ItemData;

					if ((id.Surface || id.Impassable) && item.Z + id.CalcHeight > z && item.Z < top)
					{
						ok = false;
						break;
					}
				}

				m_SpawnBuffer2.Clear();

				if (!ok)
				{
					continue;
				}

				if (ltImpassable && ltAvgZ > z && ltLowZ < top)
				{
					continue;
				}

				for (var j = 0; j < staticTiles.Length; j++)
				{
					var tile = staticTiles[j];
					var id = TileData.ItemTable[tile.ID & TileData.MaxItemValue];

					if ((id.Surface || id.Impassable) && tile.Z + id.CalcHeight > z && tile.Z < top)
					{
						ok = false;
						break;
					}
				}

				if (!ok)
				{
					continue;
				}

				for (var j = 0; j < sector.Mobiles.Count; j++)
				{
					var m = sector.Mobiles[j];

					if (m.X == x && m.Y == y && (m.AccessLevel < AccessLevel.Counselor || !m.Hidden))
					{
						if (m.Z + 16 > z && m.Z < top)
						{
							ok = false;
							break;
						}
					}
				}

				if (ok)
				{
					return new Point3D(x, y, z);
				}
			}

			return Point3D.Zero;
		}

		public override Type GetResource(Mobile from, IHarvestTool tool, Map map, Point3D loc, IHarvestSystem harvest, Type resource)
		{
			if (harvest != null)
			{
				var roll = Utility.RandomDouble();

				// group all entries that have the same chances, then pick one entry at random
				foreach (var g in EnumerateHarvestNodes(this, harvest.GetType(), true, false).GroupBy(e => e.Amount))
				{
					if (roll > g.Key / 100.0)
					{
						continue;
					}

					var entries = g.Select(e => e.Type).OrderBy(e => Utility.RandomDouble());

					var entry = entries.FirstOrDefault();

					if (entry != null)
					{
						return entry;
					}
				}
			}

			return base.GetResource(from, tool, map, loc, harvest, resource);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(RuneName);

			var rules = Rules.Flags;

			if (rules == RegionFlags.DefaultRules)
			{
				writer.Write(true);
			}
			else
			{
				writer.Write(false);
				writer.Write(rules);
			}

			writer.Write(SpawnZLevel);

			if (m_Spawns != null)
			{
				writer.Write(m_Spawns.Length);

				foreach (var se in m_Spawns)
				{
					writer.Write(se.ID);
				}
			}
			else
			{
				writer.Write(0);
			}

			if (Currencies != null)
			{
				writer.Write(true);

				Currencies.Serialize(writer);
			}
			else
			{
				writer.Write(false);
			}

			if (HarvestNodes != null)
			{
				writer.Write(true);

				HarvestNodes.Serialize(writer);
			}
			else
			{
				writer.Write(false);
			}

			if (SkillPermissions != null)
			{
				writer.Write(true);

				SkillPermissions.Serialize(writer);
			}
			else
			{
				writer.Write(false);
			}

			if (SpellPermissions != null)
			{
				writer.Write(true);

				SpellPermissions.Serialize(writer);
			}
			else
			{
				writer.Write(false);
			}

			if (Weather != null)
			{
				writer.Write(true);

				Weather.Serialize(writer);
			}
			else
			{
				writer.Write(false);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();

			RuneName = reader.ReadString();

			if (reader.ReadBool())
			{
				Rules.Defaults();
			}
			else
			{
				Rules.Flags = reader.ReadEnum<RegionFlags>();
			}

			SpawnZLevel = reader.ReadEnum<SpawnZLevel>();

			var count = reader.ReadInt();

			if (count > 0)
			{
				var spawns = new List<SpawnEntry>(count);

				while (--count >= 0)
				{
					var index = reader.ReadInt();

					if (SpawnEntry.Table[index] is SpawnEntry e)
					{
						spawns.Add(e);
					}
				}

				if (spawns.Count > 0)
				{
					m_Spawns = spawns.ToArray();

					spawns.Clear();
				}

				spawns.TrimExcess();
			}

			if (reader.ReadBool())
			{
				Currencies.Deserialize(reader);
			}

			if (reader.ReadBool())
			{
				HarvestNodes.Deserialize(reader);
			}

			if (reader.ReadBool())
			{
				SkillPermissions.Deserialize(reader);
			}

			if (reader.ReadBool())
			{
				SpellPermissions.Deserialize(reader);
			}

			if (reader.ReadBool())
			{
				if (Weather != null)
				{
					Weather.Deserialize(reader);
				}
				else
				{
					Weather = new RegionalWeather(reader);
				}
			}
			else
			{
				Timer.DelayCall(WeatherInit);
			}
		}
	}

	public class RegionEditorGump : BaseGump
	{
		private static Timer m_PreviewTimer;

		public static void Initialize()
		{
			CommandSystem.Register("RegionEditor", AccessLevel.GameMaster, e =>
			{
				if (e.Mobile is PlayerMobile pm && !pm.HasGump(typeof(RegionEditorGump)))
				{
					BaseGump.SendGump(new RegionEditorGump(pm));
				}
			});

			CommandSystem.Register("RegionPreview", AccessLevel.GameMaster, e =>
			{
				if (m_PreviewTimer?.Running == true)
				{
					m_PreviewTimer.Stop();
					m_PreviewTimer = null;
					return;
				}

				m_PreviewTimer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1), reg =>
				{
					foreach (var p in reg.Area)
					{
						for (var i = 0; i < p.Count; i++)
						{
							Geometry.Line2D(new Point3D(p[i], 0), new Point3D(p[(i + 1) % p.Count], 0), reg.Map, (loc, map) =>
							{
								loc.Z = map.GetAverageZ(loc.X, loc.Y);

								Effects.SendLocationEffect(loc, map, 0x50D, 10, 1, 0x22, 0);
							});
						}
					}
				}, e.Mobile.Region);
			});
		}

		private static void Resize(ref int x, ref int y, ref int w, ref int h, int delta)
		{
			x += delta * -1;
			y += delta * -1;
			w += delta * +2;
			h += delta * +2;
		}

		private Poly3D[] m_LastArea;

		public Map Facet { get; private set; }
		public Region Region { get; private set; }

		private int m_Width = 800, m_Height = 600;

		private int m_NavPage, m_FacetPage, m_RegionPage;

		public RegionEditorGump(PlayerMobile user) : base(user)
		{
			Region = user.Region;
			Facet = user.Map;
		}

		private void Slice()
		{
			if (Region?.IsDefault == true)
			{
				Region = null;
			}

			if (Region?.Map != null && Region.Map != Facet)
			{
				Facet = Region.Map;

				m_FacetPage = 0;
			}
		}

		public override void AddGumpLayout()
		{
			Slice();

			var x = 0;
			var y = 0;
			var w = m_Width;
			var h = m_Height;

			var mx = x;
			var my = y;
			var mw = w / 4;
			var mh = h;

			AddMenuPanel(mx, my, mw, mh);

			var tx = mx + mw;
			var ty = y;
			var tw = w - mw;
			var th = 40;

			AddTitlePanel(tx, ty, tw, th);

			var cx = tx;
			var cy = ty + th;
			var cw = tw;
			var ch = h - th;

			AddContentPanel(cx, cy, cw, ch);
		}

		private void AddMenuPanel(int x, int y, int w, int h)
		{
			AddBackground(x, y, w, h, 2620);

			Resize(ref x, ref y, ref w, ref h, -10);

			var per = (h - 30) / 22;
			var pages = (int)Math.Ceiling(Map.AllMaps.Count / (double)per);

			m_NavPage = Math.Clamp(m_NavPage, 0, Math.Max(0, pages - 1));

			foreach (var map in Map.AllMaps.Skip(m_NavPage * per).Take(per))
			{
				var color = Color.White;

				if (map != Map.Internal)
				{
					color = map != Facet ? Color.White : Color.Yellow;

					AddButton(x, y, map != Facet ? 4005 : 4006, 4007, () => SelectFacet(map));
				}
				else
				{
					color = Color.Red;

					AddImage(x, y, 4005, 900);
				}

				AddHtml(x + 35, y, w - 35, 40, map.Name ?? $"Facet {map.MapIndex}", false, false, color);

				y += 22;
				h -= 22;
			}

			y += h - 30;
			h = 30;

			y += 8;
			h -= 8;

			if (pages > 1)
			{
				AddButton(x, y, 4015, 4016, () =>
				{
					if (--m_NavPage < 0)
					{
						m_NavPage = pages - 1;
					}

					Refresh();
				});
				AddTooltip(1011067);

				AddButton(x + (w - 30), y, 4006, 4007, () =>
				{
					if (++m_NavPage >= pages)
					{
						m_NavPage = 0;
					}

					Refresh();
				});
				AddTooltip(1011066);
			}
			else
			{
				AddImage(x, y, 4014, 900);
				AddImage(x + (w - 30), y, 4005, 900);
			}

			AddHtml(x + 30, y, w - 60, 20, Center($"{m_NavPage + 1:N0} / {Math.Max(1, pages):N0}"), false, false, Color.Yellow);
		}

		private void AddTitlePanel(int x, int y, int w, int h)
		{
			AddBackground(x, y, w, h, 2620);

			Resize(ref x, ref y, ref w, ref h, -10);

			if (Facet != null)
			{
				if (Region != null)
				{
					AddHtml(x, y, w, h, $"{Facet?.Name ?? "Unnamed Facet"} : {Region}", false, false, Color.White);
				}
				else
				{
					AddHtml(x, y, w, h, $"{Facet?.Name ?? "Unnamed Facet"}", false, false, Color.White);
				}
			}
		}

		private void AddContentPanel(int x, int y, int w, int h)
		{
			AddBackground(x, y, w, h, 2620);

			Resize(ref x, ref y, ref w, ref h, -10);

			if (Region == null)
			{
				var per = (h - 30) / 22;
				var pages = (int)Math.Ceiling(Facet.Regions.Count / (double)per);

				m_FacetPage = Math.Clamp(m_FacetPage, 0, Math.Max(0, pages - 1));

				foreach (var reg in Facet.Regions.Skip(m_FacetPage * per).Take(per))
				{
					AddButton(x, y, 208, 209, () => SelectRegion(reg));

					var name = reg.ToString();

					if (reg.ChildLevel > 0)
					{
						name = $"{new string('\u25B6', reg.ChildLevel)}{name}";
					}

					AddHtml(x + 25, y, w - 25, 40, name, false, false, Color.White);

					y += 22;
					h -= 22;
				}

				y += h - 30;
				h = 30;

				y += 8;
				h -= 8;

				if (pages > 1)
				{
					AddButton(x, y, 4015, 4016, () =>
					{
						if (--m_FacetPage < 0)
						{
							m_FacetPage = pages - 1;
						}

						Refresh();
					});
					AddTooltip(1011067);

					AddButton(x + (w - 30), y, 4006, 4007, () =>
					{
						if (++m_FacetPage >= pages)
						{
							m_FacetPage = 0;
						}

						Refresh();
					});
					AddTooltip(1011066);
				}
				else
				{
					AddImage(x, y, 4014, 900);
					AddImage(x + (w - 30), y, 4005, 900);
				}

				AddHtml(x + 30, y, w - 60, 20, Center($"{m_FacetPage + 1:N0} / {Math.Max(1, pages):N0}"), false, false, Color.Yellow);
			}
			else
			{
				var xo = x;
				var yo = y;
				var wo = w;

				var per = (h - 30) / 22;
				var pages = (int)Math.Ceiling(Region.Area.Length / (double)per);

				m_RegionPage = Math.Clamp(m_RegionPage, 0, Math.Max(0, pages - 1));

				foreach (var poly in Region.Area.Skip(m_RegionPage * per).Take(per))
				{
					AddButton(xo, yo, 4018, 4019, () => RemovePoly(poly));

					xo += 32;
					wo -= 32;

					if (poly.Count > 0)
					{
						AddButton(xo, yo, 4012, 4013, () => EditPoly(poly));
					}
					else
					{
						AddImage(xo, yo, 4011, 900);
					}

					xo += 32;
					wo -= 32;

					if (poly.Count > 0)
					{
						AddButton(xo, yo, 4009, 4010, () => VisualPoly(poly));
					}
					else
					{
						AddImage(xo, yo, 4008, 900);
					}

					xo += 32;
					wo -= 32;

					if (poly.Count > 0)
					{
						AddButton(xo, yo, 4006, 4007, () => VisitPoly(poly));
					}
					else
					{
						AddImage(xo, yo, 4005, 900);
					}

					xo += 32;
					wo -= 32;

					if (poly.Count > 0)
					{
						AddHtml(xo, yo + 2, wo, 20, $"{poly[0]}..[{poly.Count - 1}], {poly.MinZ}..{poly.MaxZ}", false, false, Color.White);
					}
					else
					{
						AddHtml(xo, yo + 2, wo, 20, $"Empty, {poly.MinZ}\u261E{poly.MaxZ}...[{poly.Depth}]", false, false, Color.Red);
					}

					xo = x;
					yo += 22;
					wo = w;
				}

				y += h - 30;
				h = 30;

				y += 8;
				h -= 8;

				if (pages > 1)
				{
					AddButton(x, y, 4015, 4016, () =>
					{
						if (--m_RegionPage < 0)
						{
							m_RegionPage = pages - 1;
						}

						Refresh();
					});
					AddTooltip(1011067);

					AddButton(x + (w - 30), y, 4006, 4007, () =>
					{
						if (++m_RegionPage >= pages)
						{
							m_RegionPage = 0;
						}

						Refresh();
					});
					AddTooltip(1011066);
				}
				else
				{
					AddImage(x, y, 4014, 900);
					AddImage(x + (w - 30), y, 4005, 900);
				}

				AddHtml(x + 30, y, w - 60, 20, Center($"{m_RegionPage + 1:N0} / {Math.Max(1, pages):N0}"), false, false, Color.Yellow);
			}
		}

		private void SelectFacet(Map map)
		{
			if (Facet != map)
			{
				m_LastArea = null;

				m_FacetPage = 0;
				m_RegionPage = 0;
			}

			Facet = map;
			Region = null;

			Refresh();
		}

		private void SelectRegion(Region reg)
		{
			if (Region != reg)
			{
				m_LastArea = null;

				m_RegionPage = 0;
			}

			Region = reg;

			Refresh();
		}

		private void RemovePoly(Poly3D poly)
		{
			var oldArea = m_LastArea = Region.Area;
			var newArea = new Poly3D[oldArea.Length - 1];

			for (int i = 0, n = 0; i < oldArea.Length; i++)
			{
				if (oldArea[i] != poly)
				{
					newArea[n++] = oldArea[i];
				}
			}

			Region.Area = newArea;

			Refresh();
		}

		private void EditPoly(Poly3D poly)
		{
			/*var oldArea = Region.Area;
			var newArea = new Poly3D[oldArea.Length - 1];

			for (int i = 0, n = 0; i < oldArea.Length; i++)
			{
				if (oldArea[i] != poly)
				{
					newArea[n++] = oldArea[i];
				}
			}

			Region.Area = newArea;*/

			Refresh();
		}

		private void VisualPoly(Poly3D poly)
		{
			Refresh();
		}

		private void VisitPoly(Poly3D poly)
		{
			if (poly.Count > 0)
			{
				var p = poly[0];
				var z = Facet.GetAverageZ(p.X, p.Y);

				User.MoveToWorld(new Point3D(p, z), Facet);
			}

			Refresh();
		}

		private void UndoAreaEdit()
		{
			Region.Area = m_LastArea;

			m_LastArea = null;

			Refresh();
		}
	}
}