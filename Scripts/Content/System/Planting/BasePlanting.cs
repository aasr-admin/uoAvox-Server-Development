using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.Engines.Plants
{
	public enum PlantType
	{
		CampionFlowers,
		Poppies,
		Snowdrops,
		Bulrushes,
		Lilies,
		PampasGrass,
		Rushes,
		ElephantEarPlant,
		Fern,
		PonytailPalm,
		SmallPalm,
		CenturyPlant,
		WaterPlant,
		SnakePlant,
		PricklyPearCactus,
		BarrelCactus,
		TribarrelCactus,
		CommonGreenBonsai,
		CommonPinkBonsai,
		UncommonGreenBonsai,
		UncommonPinkBonsai,
		RareGreenBonsai,
		RarePinkBonsai,
		ExceptionalBonsai,
		ExoticBonsai,
		Cactus,
		FlaxFlowers,
		FoxgloveFlowers,
		HopsEast,
		OrfluerFlowers,
		CypressTwisted,
		HedgeShort,
		JuniperBush,
		SnowdropPatch,
		Cattails,
		PoppyPatch,
		SpiderTree,
		WaterLily,
		CypressStraight,
		HedgeTall,
		HopsSouth,
		SugarCanes,
		CocoaTree
	}

	[Flags]
	public enum PlantHue
	{
		Plain = 0x1 | Crossable | Reproduces,

		Red = 0x2 | Crossable | Reproduces,
		Blue = 0x4 | Crossable | Reproduces,
		Yellow = 0x8 | Crossable | Reproduces,

		BrightRed = Red | Bright,
		BrightBlue = Blue | Bright,
		BrightYellow = Yellow | Bright,

		Purple = Red | Blue,
		Green = Blue | Yellow,
		Orange = Red | Yellow,

		BrightPurple = Purple | Bright,
		BrightGreen = Green | Bright,
		BrightOrange = Orange | Bright,

		Black = 0x10,
		White = 0x20,
		Pink = 0x40,
		Magenta = 0x80,
		Aqua = 0x100,
		FireRed = 0x200,

		None = 0,
		Reproduces = 0x2000000,
		Crossable = 0x4000000,
		Bright = 0x8000000
	}

	public enum PlantCategory
	{
		Default,
		Common = 1063335, //
		Uncommon = 1063336, //
		Rare = 1063337, // Bonsai
		Exceptional = 1063341, //
		Exotic = 1063342, //
		Peculiar = 1080528,
		Fragrant = 1080529
	}

	public enum PlantHealth
	{
		Dying,
		Wilted,
		Healthy,
		Vibrant
	}

	public enum PlantGrowthIndicator
	{
		None,
		InvalidLocation,
		NotHealthy,
		Delay,
		Grown,
		DoubleGrown
	}

	public enum PlantStatus
	{
		BowlOfDirt = 0,
		Seed = 1,
		Sapling = 2,
		Plant = 4,
		FullGrownPlant = 7,
		DecorativePlant = 10,
		DeadTwigs = 11,

		Stage1 = 1,
		Stage2 = 2,
		Stage3 = 3,
		Stage4 = 4,
		Stage5 = 5,
		Stage6 = 6,
		Stage7 = 7,
		Stage8 = 8,
		Stage9 = 9
	}


	/// Planting System
	public class PlantSystem
	{
		public static readonly TimeSpan CheckDelay = TimeSpan.FromHours(23.0);

		private readonly PlantItem m_Plant;
		private bool m_FertileDirt;

		private DateTime m_NextGrowth;
		private PlantGrowthIndicator m_GrowthIndicator;

		private int m_Water;

		private int m_Hits;
		private int m_Infestation;
		private int m_Fungus;
		private int m_Poison;
		private int m_Disease;
		private int m_PoisonPotion;
		private int m_CurePotion;
		private int m_HealPotion;
		private int m_StrengthPotion;

		private bool m_Pollinated;
		private PlantType m_SeedType;
		private PlantHue m_SeedHue;
		private int m_AvailableSeeds;
		private int m_LeftSeeds;

		private int m_AvailableResources;
		private int m_LeftResources;

		public PlantItem Plant => m_Plant;

		public bool FertileDirt
		{
			get => m_FertileDirt;
			set => m_FertileDirt = value;
		}

		public DateTime NextGrowth => m_NextGrowth;

		public PlantGrowthIndicator GrowthIndicator => m_GrowthIndicator;

		public bool IsFullWater => m_Water >= 4;
		public int Water
		{
			get => m_Water;
			set
			{
				if (value < 0)
				{
					m_Water = 0;
				}
				else if (value > 4)
				{
					m_Water = 4;
				}
				else
				{
					m_Water = value;
				}

				m_Plant.InvalidateProperties();
			}
		}

		public int Hits
		{
			get => m_Hits;
			set
			{
				if (m_Hits == value)
				{
					return;
				}

				if (value < 0)
				{
					m_Hits = 0;
				}
				else if (value > MaxHits)
				{
					m_Hits = MaxHits;
				}
				else
				{
					m_Hits = value;
				}

				if (m_Hits == 0)
				{
					m_Plant.Die();
				}

				m_Plant.InvalidateProperties();
			}
		}

		public int MaxHits => 10 + (int)m_Plant.PlantStatus * 2;

		public PlantHealth Health
		{
			get
			{
				var perc = m_Hits * 100 / MaxHits;

				if (perc < 33)
				{
					return PlantHealth.Dying;
				}
				else if (perc < 66)
				{
					return PlantHealth.Wilted;
				}
				else if (perc < 100)
				{
					return PlantHealth.Healthy;
				}
				else
				{
					return PlantHealth.Vibrant;
				}
			}
		}

		public int Infestation
		{
			get => m_Infestation;
			set
			{
				if (value < 0)
				{
					m_Infestation = 0;
				}
				else if (value > 2)
				{
					m_Infestation = 2;
				}
				else
				{
					m_Infestation = value;
				}
			}
		}

		public int Fungus
		{
			get => m_Fungus;
			set
			{
				if (value < 0)
				{
					m_Fungus = 0;
				}
				else if (value > 2)
				{
					m_Fungus = 2;
				}
				else
				{
					m_Fungus = value;
				}
			}
		}

		public int Poison
		{
			get => m_Poison;
			set
			{
				if (value < 0)
				{
					m_Poison = 0;
				}
				else if (value > 2)
				{
					m_Poison = 2;
				}
				else
				{
					m_Poison = value;
				}
			}
		}

		public int Disease
		{
			get => m_Disease;
			set
			{
				if (value < 0)
				{
					m_Disease = 0;
				}
				else if (value > 2)
				{
					m_Disease = 2;
				}
				else
				{
					m_Disease = value;
				}
			}
		}

		public bool IsFullPoisonPotion => m_PoisonPotion >= 2;
		public int PoisonPotion
		{
			get => m_PoisonPotion;
			set
			{
				if (value < 0)
				{
					m_PoisonPotion = 0;
				}
				else if (value > 2)
				{
					m_PoisonPotion = 2;
				}
				else
				{
					m_PoisonPotion = value;
				}
			}
		}

		public bool IsFullCurePotion => m_CurePotion >= 2;
		public int CurePotion
		{
			get => m_CurePotion;
			set
			{
				if (value < 0)
				{
					m_CurePotion = 0;
				}
				else if (value > 2)
				{
					m_CurePotion = 2;
				}
				else
				{
					m_CurePotion = value;
				}
			}
		}

		public bool IsFullHealPotion => m_HealPotion >= 2;
		public int HealPotion
		{
			get => m_HealPotion;
			set
			{
				if (value < 0)
				{
					m_HealPotion = 0;
				}
				else if (value > 2)
				{
					m_HealPotion = 2;
				}
				else
				{
					m_HealPotion = value;
				}
			}
		}

		public bool IsFullStrengthPotion => m_StrengthPotion >= 2;
		public int StrengthPotion
		{
			get => m_StrengthPotion;
			set
			{
				if (value < 0)
				{
					m_StrengthPotion = 0;
				}
				else if (value > 2)
				{
					m_StrengthPotion = 2;
				}
				else
				{
					m_StrengthPotion = value;
				}
			}
		}

		public bool HasMaladies => Infestation > 0 || Fungus > 0 || Poison > 0 || Disease > 0 || Water != 2;

		public bool PollenProducing => m_Plant.IsCrossable && m_Plant.PlantStatus >= PlantStatus.FullGrownPlant;

		public bool Pollinated
		{
			get => m_Pollinated;
			set => m_Pollinated = value;
		}

		public PlantType SeedType
		{
			get
			{
				if (m_Pollinated)
				{
					return m_SeedType;
				}
				else
				{
					return m_Plant.PlantType;
				}
			}
			set => m_SeedType = value;
		}

		public PlantHue SeedHue
		{
			get
			{
				if (m_Pollinated)
				{
					return m_SeedHue;
				}
				else
				{
					return m_Plant.PlantHue;
				}
			}
			set => m_SeedHue = value;
		}

		public int AvailableSeeds
		{
			get => m_AvailableSeeds;
			set
			{
				if (value >= 0)
				{
					m_AvailableSeeds = value;
				}
			}
		}

		public int LeftSeeds
		{
			get => m_LeftSeeds;
			set
			{
				if (value >= 0)
				{
					m_LeftSeeds = value;
				}
			}
		}

		public int AvailableResources
		{
			get => m_AvailableResources;
			set
			{
				if (value >= 0)
				{
					m_AvailableResources = value;
				}
			}
		}

		public int LeftResources
		{
			get => m_LeftResources;
			set
			{
				if (value >= 0)
				{
					m_LeftResources = value;
				}
			}
		}

		public PlantSystem(PlantItem plant, bool fertileDirt)
		{
			m_Plant = plant;
			m_FertileDirt = fertileDirt;

			m_NextGrowth = DateTime.UtcNow + CheckDelay;
			m_GrowthIndicator = PlantGrowthIndicator.None;
			m_Hits = MaxHits;
			m_LeftSeeds = 8;
			m_LeftResources = 8;
		}

		public void Reset(bool potions)
		{
			m_NextGrowth = DateTime.UtcNow + CheckDelay;
			m_GrowthIndicator = PlantGrowthIndicator.None;

			Hits = MaxHits;
			m_Infestation = 0;
			m_Fungus = 0;
			m_Poison = 0;
			m_Disease = 0;

			if (potions)
			{
				m_PoisonPotion = 0;
				m_CurePotion = 0;
				m_HealPotion = 0;
				m_StrengthPotion = 0;
			}

			m_Pollinated = false;
			m_AvailableSeeds = 0;
			m_LeftSeeds = 8;

			m_AvailableResources = 0;
			m_LeftResources = 8;
		}

		public int GetLocalizedDirtStatus()
		{
			if (Water <= 1)
			{
				return 1060826; // hard
			}
			else if (Water <= 2)
			{
				return 1060827; // soft
			}
			else if (Water <= 3)
			{
				return 1060828; // squishy
			}
			else
			{
				return 1060829; // sopping wet
			}
		}

		public int GetLocalizedHealth()
		{
			switch (Health)
			{
				case PlantHealth.Dying: return 1060825; // dying
				case PlantHealth.Wilted: return 1060824; // wilted
				case PlantHealth.Healthy: return 1060823; // healthy
				default: return 1060822; // vibrant
			}
		}

		public static void Configure()
		{
			EventSink.WorldLoad += new WorldLoadEventHandler(EventSink_WorldLoad);

			if (!Misc.AutoRestart.Enabled)
			{
				EventSink.WorldSave += new WorldSaveEventHandler(EventSink_WorldSave);
			}

			EventSink.Login += new LoginEventHandler(EventSink_Login);
		}

		private static void EventSink_Login(LoginEventArgs args)
		{
			var from = args.Mobile;

			if (from.Backpack != null)
			{
				var plants = from.Backpack.FindItemsByType<PlantItem>();

				foreach (var plant in plants)
				{
					if (plant.IsGrowable)
					{
						plant.PlantSystem.DoGrowthCheck();
					}
				}
			}

			var bank = from.FindBankNoCreate();

			if (bank != null)
			{
				var plants = bank.FindItemsByType<PlantItem>();

				foreach (var plant in plants)
				{
					if (plant.IsGrowable)
					{
						plant.PlantSystem.DoGrowthCheck();
					}
				}
			}
		}

		public static void GrowAll()
		{
			var plants = PlantItem.Plants;
			var now = DateTime.UtcNow;

			for (var i = plants.Count - 1; i >= 0; --i)
			{
				var plant = (PlantItem)plants[i];

				if (plant.IsGrowable && (plant.RootParent as Mobile) == null && now >= plant.PlantSystem.NextGrowth)
				{
					plant.PlantSystem.DoGrowthCheck();
				}
			}
		}

		private static void EventSink_WorldLoad()
		{
			GrowAll();
		}

		private static void EventSink_WorldSave(WorldSaveEventArgs args)
		{
			GrowAll();
		}

		public void DoGrowthCheck()
		{
			if (!m_Plant.IsGrowable)
			{
				return;
			}

			if (DateTime.UtcNow < m_NextGrowth)
			{
				m_GrowthIndicator = PlantGrowthIndicator.Delay;
				return;
			}

			m_NextGrowth = DateTime.UtcNow + CheckDelay;

			if (!m_Plant.ValidGrowthLocation)
			{
				m_GrowthIndicator = PlantGrowthIndicator.InvalidLocation;
				return;
			}

			if (m_Plant.PlantStatus == PlantStatus.BowlOfDirt)
			{
				if (Water > 2 || Utility.RandomDouble() < 0.9)
				{
					Water--;
				}

				return;
			}

			ApplyBeneficEffects();

			if (!ApplyMaladiesEffects()) // Dead
			{
				return;
			}

			Grow();

			UpdateMaladies();
		}

		private void ApplyBeneficEffects()
		{
			if (PoisonPotion >= Infestation)
			{
				PoisonPotion -= Infestation;
				Infestation = 0;
			}
			else
			{
				Infestation -= PoisonPotion;
				PoisonPotion = 0;
			}

			if (CurePotion >= Fungus)
			{
				CurePotion -= Fungus;
				Fungus = 0;
			}
			else
			{
				Fungus -= CurePotion;
				CurePotion = 0;
			}

			if (HealPotion >= Poison)
			{
				HealPotion -= Poison;
				Poison = 0;
			}
			else
			{
				Poison -= HealPotion;
				HealPotion = 0;
			}

			if (HealPotion >= Disease)
			{
				HealPotion -= Disease;
				Disease = 0;
			}
			else
			{
				Disease -= HealPotion;
				HealPotion = 0;
			}

			if (!HasMaladies)
			{
				if (HealPotion > 0)
				{
					Hits += HealPotion * 7;
				}
				else
				{
					Hits += 2;
				}
			}

			HealPotion = 0;
		}

		private bool ApplyMaladiesEffects()
		{
			var damage = 0;

			if (Infestation > 0)
			{
				damage += Infestation * Utility.RandomMinMax(3, 6);
			}

			if (Fungus > 0)
			{
				damage += Fungus * Utility.RandomMinMax(3, 6);
			}

			if (Poison > 0)
			{
				damage += Poison * Utility.RandomMinMax(3, 6);
			}

			if (Disease > 0)
			{
				damage += Disease * Utility.RandomMinMax(3, 6);
			}

			if (Water > 2)
			{
				damage += (Water - 2) * Utility.RandomMinMax(3, 6);
			}
			else if (Water < 2)
			{
				damage += (2 - Water) * Utility.RandomMinMax(3, 6);
			}

			Hits -= damage;

			return m_Plant.IsGrowable && m_Plant.PlantStatus != PlantStatus.BowlOfDirt;
		}

		private void Grow()
		{
			if (Health < PlantHealth.Healthy)
			{
				m_GrowthIndicator = PlantGrowthIndicator.NotHealthy;
			}
			else if (m_FertileDirt && m_Plant.PlantStatus <= PlantStatus.Stage5 && Utility.RandomDouble() < 0.1)
			{
				var curStage = (int)m_Plant.PlantStatus;
				m_Plant.PlantStatus = (PlantStatus)(curStage + 2);

				m_GrowthIndicator = PlantGrowthIndicator.DoubleGrown;
			}
			else if (m_Plant.PlantStatus < PlantStatus.Stage9)
			{
				var curStage = (int)m_Plant.PlantStatus;
				m_Plant.PlantStatus = (PlantStatus)(curStage + 1);

				m_GrowthIndicator = PlantGrowthIndicator.Grown;
			}
			else
			{
				if (Pollinated && LeftSeeds > 0 && m_Plant.Reproduces)
				{
					LeftSeeds--;
					AvailableSeeds++;
				}

				if (LeftResources > 0 && PlantResourceInfo.GetInfo(m_Plant.PlantType, m_Plant.PlantHue) != null)
				{
					LeftResources--;
					AvailableResources++;
				}

				m_GrowthIndicator = PlantGrowthIndicator.Grown;
			}

			if (m_Plant.PlantStatus >= PlantStatus.Stage9 && !Pollinated)
			{
				Pollinated = true;
				SeedType = m_Plant.PlantType;
				SeedHue = m_Plant.PlantHue;
			}
		}

		private void UpdateMaladies()
		{
			var infestationChance = 0.30 - StrengthPotion * 0.075 + (Water - 2) * 0.10;

			var typeInfo = PlantTypeInfo.GetInfo(m_Plant.PlantType);
			if (typeInfo.Flowery)
			{
				infestationChance += 0.10;
			}

			if (PlantHueInfo.IsBright(m_Plant.PlantHue))
			{
				infestationChance += 0.10;
			}

			if (Utility.RandomDouble() < infestationChance)
			{
				Infestation++;
			}

			var fungusChance = 0.15 - StrengthPotion * 0.075 + (Water - 2) * 0.10;

			if (Utility.RandomDouble() < fungusChance)
			{
				Fungus++;
			}

			if (Water > 2 || Utility.RandomDouble() < 0.9)
			{
				Water--;
			}

			if (PoisonPotion > 0)
			{
				Poison += PoisonPotion;
				PoisonPotion = 0;
			}

			if (CurePotion > 0)
			{
				Disease += CurePotion;
				CurePotion = 0;
			}

			StrengthPotion = 0;
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(2); // version

			writer.Write(m_FertileDirt);

			writer.Write(m_NextGrowth);
			writer.Write((int)m_GrowthIndicator);

			writer.Write(m_Water);

			writer.Write(m_Hits);
			writer.Write(m_Infestation);
			writer.Write(m_Fungus);
			writer.Write(m_Poison);
			writer.Write(m_Disease);
			writer.Write(m_PoisonPotion);
			writer.Write(m_CurePotion);
			writer.Write(m_HealPotion);
			writer.Write(m_StrengthPotion);

			writer.Write(m_Pollinated);
			writer.Write((int)m_SeedType);
			writer.Write((int)m_SeedHue);
			writer.Write(m_AvailableSeeds);
			writer.Write(m_LeftSeeds);

			writer.Write(m_AvailableResources);
			writer.Write(m_LeftResources);
		}

		public PlantSystem(PlantItem plant, GenericReader reader)
		{
			m_Plant = plant;

			var version = reader.ReadInt();

			m_FertileDirt = reader.ReadBool();

			if (version >= 1)
			{
				m_NextGrowth = reader.ReadDateTime();
			}
			else
			{
				m_NextGrowth = reader.ReadDeltaTime();
			}

			m_GrowthIndicator = (PlantGrowthIndicator)reader.ReadInt();

			m_Water = reader.ReadInt();

			m_Hits = reader.ReadInt();
			m_Infestation = reader.ReadInt();
			m_Fungus = reader.ReadInt();
			m_Poison = reader.ReadInt();
			m_Disease = reader.ReadInt();
			m_PoisonPotion = reader.ReadInt();
			m_CurePotion = reader.ReadInt();
			m_HealPotion = reader.ReadInt();
			m_StrengthPotion = reader.ReadInt();

			m_Pollinated = reader.ReadBool();
			m_SeedType = (PlantType)reader.ReadInt();
			m_SeedHue = (PlantHue)reader.ReadInt();
			m_AvailableSeeds = reader.ReadInt();
			m_LeftSeeds = reader.ReadInt();

			m_AvailableResources = reader.ReadInt();
			m_LeftResources = reader.ReadInt();

			if (version < 2 && PlantHueInfo.IsCrossable(m_SeedHue))
			{
				m_SeedHue |= PlantHue.Reproduces;
			}
		}
	}

	public partial class PlantTypeInfo
	{
		public static PlantTypeInfo GetInfo(PlantType plantType)
		{
			var index = (int)plantType;

			if (index >= 0 && index < m_Table.Length)
			{
				return m_Table[index];
			}
			else
			{
				return m_Table[0];
			}
		}

		public static PlantType RandomFirstGeneration()
		{
			switch (Utility.Random(3))
			{
				case 0: return PlantType.CampionFlowers;
				case 1: return PlantType.Fern;
				default: return PlantType.TribarrelCactus;
			}
		}

		public static PlantType RandomPeculiarGroupOne()
		{
			switch (Utility.Random(6))
			{
				case 0: return PlantType.Cactus;
				case 1: return PlantType.FlaxFlowers;
				case 2: return PlantType.FoxgloveFlowers;
				case 3: return PlantType.HopsEast;
				case 4: return PlantType.CocoaTree;
				default: return PlantType.OrfluerFlowers;
			}
		}

		public static PlantType RandomPeculiarGroupTwo()
		{
			switch (Utility.Random(5))
			{
				case 0: return PlantType.CypressTwisted;
				case 1: return PlantType.HedgeShort;
				case 2: return PlantType.JuniperBush;
				case 3: return PlantType.CocoaTree;
				default: return PlantType.SnowdropPatch;
			}
		}

		public static PlantType RandomPeculiarGroupThree()
		{
			switch (Utility.Random(5))
			{
				case 0: return PlantType.Cattails;
				case 1: return PlantType.PoppyPatch;
				case 2: return PlantType.SpiderTree;
				case 3: return PlantType.CocoaTree;
				default: return PlantType.WaterLily;
			}
		}

		public static PlantType RandomPeculiarGroupFour()
		{
			switch (Utility.Random(5))
			{
				case 0: return PlantType.CypressStraight;
				case 1: return PlantType.HedgeTall;
				case 2: return PlantType.HopsSouth;
				case 3: return PlantType.CocoaTree;
				default: return PlantType.SugarCanes;
			}
		}

		public static PlantType RandomBonsai(double increaseRatio)
		{
			/* Chances of each plant type are equal to the chances of the previous plant type * increaseRatio:
			 * E.g.:
			 *  chances_of_uncommon = chances_of_common * increaseRatio
			 *  chances_of_rare = chances_of_uncommon * increaseRatio
			 *  ...
			 *
			 * If increaseRatio < 1 -> rare plants are actually rarer than the others
			 * If increaseRatio > 1 -> rare plants are actually more common than the others (it might be the case with certain monsters)
			 *
			 * If a plant type (common, uncommon, ...) has 2 different colors, they have the same chances:
			 *  chances_of_green_common = chances_of_pink_common = chances_of_common / 2
			 *  ...
			 */

			var k1 = increaseRatio >= 0.0 ? increaseRatio : 0.0;
			var k2 = k1 * k1;
			var k3 = k2 * k1;
			var k4 = k3 * k1;

			var exp1 = k1 + 1.0;
			var exp2 = k2 + exp1;
			var exp3 = k3 + exp2;
			var exp4 = k4 + exp3;

			var rand = Utility.RandomDouble();

			if (rand < 0.5 / exp4)
			{
				return PlantType.CommonGreenBonsai;
			}
			else if (rand < 1.0 / exp4)
			{
				return PlantType.CommonPinkBonsai;
			}
			else if (rand < (k1 * 0.5 + 1.0) / exp4)
			{
				return PlantType.UncommonGreenBonsai;
			}
			else if (rand < exp1 / exp4)
			{
				return PlantType.UncommonPinkBonsai;
			}
			else if (rand < (k2 * 0.5 + exp1) / exp4)
			{
				return PlantType.RareGreenBonsai;
			}
			else if (rand < exp2 / exp4)
			{
				return PlantType.RarePinkBonsai;
			}
			else if (rand < exp3 / exp4)
			{
				return PlantType.ExceptionalBonsai;
			}
			else
			{
				return PlantType.ExoticBonsai;
			}
		}

		public static bool IsCrossable(PlantType plantType)
		{
			return GetInfo(plantType).Crossable;
		}

		public static PlantType Cross(PlantType first, PlantType second)
		{
			if (!IsCrossable(first) || !IsCrossable(second))
			{
				return PlantType.CampionFlowers;
			}

			var firstIndex = (int)first;
			var secondIndex = (int)second;

			if (firstIndex + 1 == secondIndex || firstIndex == secondIndex + 1)
			{
				return Utility.RandomBool() ? first : second;
			}
			else
			{
				return (PlantType)((firstIndex + secondIndex) / 2);
			}
		}

		public static bool CanReproduce(PlantType plantType)
		{
			return GetInfo(plantType).Reproduces;
		}

		public int GetPlantLabelSeed(PlantHueInfo hueInfo)
		{
			if (m_PlantLabelSeed != -1)
			{
				return m_PlantLabelSeed;
			}

			return hueInfo.IsBright() ? 1061887 : 1061888; // a ~1_val~ of ~2_val~ dirt with a ~3_val~ [bright] ~4_val~ ~5_val~ ~6_val~
		}

		public int GetPlantLabelPlant(PlantHueInfo hueInfo)
		{
			if (m_PlantLabelPlant != -1)
			{
				return m_PlantLabelPlant;
			}

			if (m_ContainsPlant)
			{
				return hueInfo.IsBright() ? 1060832 : 1060831; // a ~1_val~ of ~2_val~ dirt with a ~3_val~ [bright] ~4_val~ ~5_val~
			}
			else
			{
				return hueInfo.IsBright() ? 1061887 : 1061888; // a ~1_val~ of ~2_val~ dirt with a ~3_val~ [bright] ~4_val~ ~5_val~ ~6_val~
			}
		}

		public int GetPlantLabelFullGrown(PlantHueInfo hueInfo)
		{
			if (m_PlantLabelFullGrown != -1)
			{
				return m_PlantLabelFullGrown;
			}

			if (m_ContainsPlant)
			{
				return hueInfo.IsBright() ? 1061891 : 1061889; // a ~1_HEALTH~ [bright] ~2_COLOR~ ~3_NAME~
			}
			else
			{
				return hueInfo.IsBright() ? 1061892 : 1061890; // a ~1_HEALTH~ [bright] ~2_COLOR~ ~3_NAME~ plant
			}
		}

		public int GetPlantLabelDecorative(PlantHueInfo hueInfo)
		{
			if (m_PlantLabelDecorative != -1)
			{
				return m_PlantLabelDecorative;
			}

			return hueInfo.IsBright() ? 1074267 : 1070973; // a decorative [bright] ~1_COLOR~ ~2_TYPE~
		}

		public int GetSeedLabel(PlantHueInfo hueInfo)
		{
			if (m_SeedLabel != -1)
			{
				return m_SeedLabel;
			}

			return hueInfo.IsBright() ? 1061918 : 1061917; // [bright] ~1_COLOR~ ~2_TYPE~ seed
		}

		public int GetSeedLabelPlural(PlantHueInfo hueInfo)
		{
			if (m_SeedLabelPlural != -1)
			{
				return m_SeedLabelPlural;
			}

			return hueInfo.IsBright() ? 1113493 : 1113492; // ~1_amount~ [bright] ~2_color~ ~3_type~ seeds
		}

		private readonly int m_ItemID;
		private readonly int m_OffsetX;
		private readonly int m_OffsetY;
		private readonly PlantType m_PlantType;
		private readonly bool m_ContainsPlant;
		private readonly bool m_Flowery;
		private readonly bool m_Crossable;
		private readonly bool m_Reproduces;
		private readonly PlantCategory m_PlantCategory;

		// Cliloc overrides
		private readonly int m_PlantLabelSeed;
		private readonly int m_PlantLabelPlant;
		private readonly int m_PlantLabelFullGrown;
		private readonly int m_PlantLabelDecorative;
		private readonly int m_SeedLabel;
		private readonly int m_SeedLabelPlural;

		public int ItemID => m_ItemID;
		public int OffsetX => m_OffsetX;
		public int OffsetY => m_OffsetY;
		public PlantType PlantType => m_PlantType;
		public PlantCategory PlantCategory => m_PlantCategory;
		public int Name => (m_ItemID < 0x4000) ? 1020000 + m_ItemID : 1078872 + m_ItemID;

		public bool ContainsPlant => m_ContainsPlant;
		public bool Flowery => m_Flowery;
		public bool Crossable => m_Crossable;
		public bool Reproduces => m_Reproduces;

		private PlantTypeInfo(int itemID, int offsetX, int offsetY, PlantType plantType, bool containsPlant, bool flowery, bool crossable, bool reproduces, PlantCategory plantCategory)
			: this(itemID, offsetX, offsetY, plantType, containsPlant, flowery, crossable, reproduces, plantCategory, -1, -1, -1, -1, -1, -1)
		{
		}

		private PlantTypeInfo(int itemID, int offsetX, int offsetY, PlantType plantType, bool containsPlant, bool flowery, bool crossable, bool reproduces, PlantCategory plantCategory, int plantLabelSeed, int plantLabelPlant, int plantLabelFullGrown, int plantLabelDecorative, int seedLabel, int seedLabelPlural)
		{
			m_ItemID = itemID;
			m_OffsetX = offsetX;
			m_OffsetY = offsetY;
			m_PlantType = plantType;
			m_ContainsPlant = containsPlant;
			m_Flowery = flowery;
			m_Crossable = crossable;
			m_Reproduces = reproduces;
			m_PlantCategory = plantCategory;
			m_PlantLabelSeed = plantLabelSeed;
			m_PlantLabelPlant = plantLabelPlant;
			m_PlantLabelFullGrown = plantLabelFullGrown;
			m_PlantLabelDecorative = plantLabelDecorative;
			m_SeedLabel = seedLabel;
			m_SeedLabelPlural = seedLabelPlural;
		}
	}

	public class PlantHueInfo
	{
		private static readonly Dictionary<PlantHue, PlantHueInfo> m_Table;

		static PlantHueInfo()
		{
			m_Table = new Dictionary<PlantHue, PlantHueInfo> {
				[PlantHue.Plain] = new PlantHueInfo(0, 1060813, PlantHue.Plain, 0x835),
				[PlantHue.Red] = new PlantHueInfo(0x66D, 1060814, PlantHue.Red, 0x24),
				[PlantHue.Blue] = new PlantHueInfo(0x53D, 1060815, PlantHue.Blue, 0x6),
				[PlantHue.Yellow] = new PlantHueInfo(0x8A5, 1060818, PlantHue.Yellow, 0x38),
				[PlantHue.BrightRed] = new PlantHueInfo(0x21, 1060814, PlantHue.BrightRed, 0x21),
				[PlantHue.BrightBlue] = new PlantHueInfo(0x5, 1060815, PlantHue.BrightBlue, 0x6),
				[PlantHue.BrightYellow] = new PlantHueInfo(0x38, 1060818, PlantHue.BrightYellow, 0x35),
				[PlantHue.Purple] = new PlantHueInfo(0xD, 1060816, PlantHue.Purple, 0x10),
				[PlantHue.Green] = new PlantHueInfo(0x59B, 1060819, PlantHue.Green, 0x42),
				[PlantHue.Orange] = new PlantHueInfo(0x46F, 1060817, PlantHue.Orange, 0x2E),
				[PlantHue.BrightPurple] = new PlantHueInfo(0x10, 1060816, PlantHue.BrightPurple, 0xD),
				[PlantHue.BrightGreen] = new PlantHueInfo(0x42, 1060819, PlantHue.BrightGreen, 0x3F),
				[PlantHue.BrightOrange] = new PlantHueInfo(0x2B, 1060817, PlantHue.BrightOrange, 0x2B),
				[PlantHue.Black] = new PlantHueInfo(0x455, 1060820, PlantHue.Black, 0),
				[PlantHue.White] = new PlantHueInfo(0x481, 1060821, PlantHue.White, 0x481),
				[PlantHue.Pink] = new PlantHueInfo(0x48E, 1061854, PlantHue.Pink),
				[PlantHue.Magenta] = new PlantHueInfo(0x486, 1061852, PlantHue.Magenta),
				[PlantHue.Aqua] = new PlantHueInfo(0x495, 1061853, PlantHue.Aqua),
				[PlantHue.FireRed] = new PlantHueInfo(0x489, 1061855, PlantHue.FireRed)
			};
		}

		public static PlantHueInfo GetInfo(PlantHue plantHue)
		{
			PlantHueInfo info = null;

			if (m_Table.TryGetValue(plantHue, out info))
			{
				return info;
			}
			else
			{
				return m_Table[PlantHue.Plain];
			}
		}

		public static PlantHue RandomFirstGeneration()
		{
			switch (Utility.Random(4))
			{
				case 0: return PlantHue.Plain;
				case 1: return PlantHue.Red;
				case 2: return PlantHue.Blue;
				default: return PlantHue.Yellow;
			}
		}

		public static bool CanReproduce(PlantHue plantHue)
		{
			return (plantHue & PlantHue.Reproduces) != PlantHue.None;
		}

		public static bool IsCrossable(PlantHue plantHue)
		{
			return (plantHue & PlantHue.Crossable) != PlantHue.None;
		}

		public static bool IsBright(PlantHue plantHue)
		{
			return (plantHue & PlantHue.Bright) != PlantHue.None;
		}

		public static PlantHue GetNotBright(PlantHue plantHue)
		{
			return plantHue & ~PlantHue.Bright;
		}

		public static bool IsPrimary(PlantHue plantHue)
		{
			return plantHue == PlantHue.Red || plantHue == PlantHue.Blue || plantHue == PlantHue.Yellow;
		}

		public static PlantHue Cross(PlantHue first, PlantHue second)
		{
			if (!IsCrossable(first) || !IsCrossable(second))
			{
				return PlantHue.None;
			}

			if (Utility.RandomDouble() < 0.01)
			{
				return Utility.RandomBool() ? PlantHue.Black : PlantHue.White;
			}

			if (first == PlantHue.Plain || second == PlantHue.Plain)
			{
				return PlantHue.Plain;
			}

			var notBrightFirst = GetNotBright(first);
			var notBrightSecond = GetNotBright(second);

			if (notBrightFirst == notBrightSecond)
			{
				return first | PlantHue.Bright;
			}

			var firstPrimary = IsPrimary(notBrightFirst);
			var secondPrimary = IsPrimary(notBrightSecond);

			if (firstPrimary && secondPrimary)
			{
				return notBrightFirst | notBrightSecond;
			}

			if (firstPrimary && !secondPrimary)
			{
				return notBrightFirst;
			}

			if (!firstPrimary && secondPrimary)
			{
				return notBrightSecond;
			}

			return notBrightFirst & notBrightSecond;
		}

		private readonly int m_Hue;
		private readonly int m_Name;
		private readonly PlantHue m_PlantHue;
		private readonly int m_GumpHue;

		public int Hue => m_Hue;
		public int Name => m_Name;
		public PlantHue PlantHue => m_PlantHue;
		public int GumpHue => m_GumpHue;

		private PlantHueInfo(int hue, int name, PlantHue plantHue) : this(hue, name, plantHue, hue)
		{
		}

		private PlantHueInfo(int hue, int name, PlantHue plantHue, int gumpHue)
		{
			m_Hue = hue;
			m_Name = name;
			m_PlantHue = plantHue;
			m_GumpHue = gumpHue;
		}

		public bool IsCrossable()
		{
			return IsCrossable(m_PlantHue);
		}

		public bool IsBright()
		{
			return IsBright(m_PlantHue);
		}

		public PlantHue GetNotBright()
		{
			return GetNotBright(m_PlantHue);
		}

		public bool IsPrimary()
		{
			return IsPrimary(m_PlantHue);
		}
	}

	public partial class PlantResourceInfo
	{
		public static PlantResourceInfo GetInfo(PlantType plantType, PlantHue plantHue)
		{
			foreach (var info in m_ResourceList)
			{
				if (info.PlantType == plantType && info.PlantHue == plantHue)
				{
					return info;
				}
			}

			return null;
		}

		private readonly PlantType m_PlantType;
		private readonly PlantHue m_PlantHue;
		private readonly Type m_ResourceType;

		public PlantType PlantType => m_PlantType;
		public PlantHue PlantHue => m_PlantHue;
		public Type ResourceType => m_ResourceType;

		private PlantResourceInfo(PlantType plantType, PlantHue plantHue, Type resourceType)
		{
			m_PlantType = plantType;
			m_PlantHue = plantHue;
			m_ResourceType = resourceType;
		}

		public Item CreateResource()
		{
			return (Item)Activator.CreateInstance(m_ResourceType);
		}
	}


	/// Planting Gumps
	public class MainPlantGump : Gump
	{
		private readonly PlantItem m_Plant;

		public MainPlantGump(PlantItem plant) : base(20, 20)
		{
			m_Plant = plant;

			DrawBackground();

			DrawPlant();

			AddButton(71, 67, 0xD4, 0xD4, 1, GumpButtonType.Reply, 0); // Reproduction menu
			AddItem(59, 68, 0xD08);

			var system = plant.PlantSystem;

			AddButton(71, 91, 0xD4, 0xD4, 2, GumpButtonType.Reply, 0); // Infestation
			AddItem(8, 96, 0x372);
			AddPlus(95, 92, system.Infestation);

			AddButton(71, 115, 0xD4, 0xD4, 3, GumpButtonType.Reply, 0); // Fungus
			AddItem(58, 115, 0xD16);
			AddPlus(95, 116, system.Fungus);

			AddButton(71, 139, 0xD4, 0xD4, 4, GumpButtonType.Reply, 0); // Poison
			AddItem(59, 143, 0x1AE4);
			AddPlus(95, 140, system.Poison);

			AddButton(71, 163, 0xD4, 0xD4, 5, GumpButtonType.Reply, 0); // Disease
			AddItem(55, 167, 0x1727);
			AddPlus(95, 164, system.Disease);

			AddButton(209, 67, 0xD2, 0xD2, 6, GumpButtonType.Reply, 0); // Water
			AddItem(193, 67, 0x1F9D);
			AddPlusMinus(196, 67, system.Water);

			AddButton(209, 91, 0xD4, 0xD4, 7, GumpButtonType.Reply, 0); // Poison potion
			AddItem(201, 91, 0xF0A);
			AddLevel(196, 91, system.PoisonPotion);

			AddButton(209, 115, 0xD4, 0xD4, 8, GumpButtonType.Reply, 0); // Cure potion
			AddItem(201, 115, 0xF07);
			AddLevel(196, 115, system.CurePotion);

			AddButton(209, 139, 0xD4, 0xD4, 9, GumpButtonType.Reply, 0); // Heal potion
			AddItem(201, 139, 0xF0C);
			AddLevel(196, 139, system.HealPotion);

			AddButton(209, 163, 0xD4, 0xD4, 10, GumpButtonType.Reply, 0); // Strength potion
			AddItem(201, 163, 0xF09);
			AddLevel(196, 163, system.StrengthPotion);

			AddImage(48, 47, 0xD2);
			AddLevel(54, 47, (int)m_Plant.PlantStatus);

			AddImage(232, 47, 0xD2);
			AddGrowthIndicator(239, 47);

			AddButton(48, 183, 0xD2, 0xD2, 11, GumpButtonType.Reply, 0); // Help
			AddLabel(54, 183, 0x835, "?");

			AddButton(232, 183, 0xD4, 0xD4, 12, GumpButtonType.Reply, 0); // Empty the bowl
			AddItem(219, 180, 0x15FD);
		}

		private void DrawBackground()
		{
			AddBackground(50, 50, 200, 150, 0xE10);

			AddItem(45, 45, 0xCEF);
			AddItem(45, 118, 0xCF0);

			AddItem(211, 45, 0xCEB);
			AddItem(211, 118, 0xCEC);
		}

		private void DrawPlant()
		{
			var status = m_Plant.PlantStatus;

			if (status < PlantStatus.FullGrownPlant)
			{
				AddImage(110, 85, 0x589);

				AddItem(122, 94, 0x914);
				AddItem(135, 94, 0x914);
				AddItem(120, 112, 0x914);
				AddItem(135, 112, 0x914);

				if (status >= PlantStatus.Stage2)
				{
					AddItem(127, 112, 0xC62);
				}
				if (status == PlantStatus.Stage3 || status == PlantStatus.Stage4)
				{
					AddItem(129, 85, 0xC7E);
				}
				if (status >= PlantStatus.Stage4)
				{
					AddItem(121, 117, 0xC62);
					AddItem(133, 117, 0xC62);
				}
				if (status >= PlantStatus.Stage5)
				{
					AddItem(110, 100, 0xC62);
					AddItem(140, 100, 0xC62);
					AddItem(110, 130, 0xC62);
					AddItem(140, 130, 0xC62);
				}
				if (status >= PlantStatus.Stage6)
				{
					AddItem(105, 115, 0xC62);
					AddItem(145, 115, 0xC62);
					AddItem(125, 90, 0xC62);
					AddItem(125, 135, 0xC62);
				}
			}
			else
			{
				var typeInfo = PlantTypeInfo.GetInfo(m_Plant.PlantType);
				var hueInfo = PlantHueInfo.GetInfo(m_Plant.PlantHue);

				// The large images for these trees trigger a client crash, so use a smaller, generic tree.
				if (m_Plant.PlantType == PlantType.CypressTwisted || m_Plant.PlantType == PlantType.CypressStraight)
				{
					AddItem(130 + typeInfo.OffsetX, 96 + typeInfo.OffsetY, 0x0CCA, hueInfo.Hue);
				}
				else
				{
					AddItem(130 + typeInfo.OffsetX, 96 + typeInfo.OffsetY, typeInfo.ItemID, hueInfo.Hue);
				}
			}

			if (status != PlantStatus.BowlOfDirt)
			{
				var message = m_Plant.PlantSystem.GetLocalizedHealth();

				switch (m_Plant.PlantSystem.Health)
				{
					case PlantHealth.Dying:
						{
							AddItem(92, 167, 0x1B9D);
							AddItem(161, 167, 0x1B9D);

							AddHtmlLocalized(136, 167, 42, 20, message, 0x03E0, false, false);

							break;
						}
					case PlantHealth.Wilted:
						{
							AddItem(91, 164, 0x18E6);
							AddItem(161, 164, 0x18E6);

							AddHtmlLocalized(132, 167, 42, 20, message, 0x0300, false, false);

							break;
						}
					case PlantHealth.Healthy:
						{
							AddItem(96, 168, 0xC61);
							AddItem(162, 168, 0xC61);

							AddHtmlLocalized(129, 167, 42, 20, message, 0x0200, false, false);

							break;
						}
					case PlantHealth.Vibrant:
						{
							AddItem(93, 162, 0x1A99);
							AddItem(162, 162, 0x1A99);

							AddHtmlLocalized(129, 167, 42, 20, message, 0x021C, false, false);

							break;
						}
				}
			}
		}

		private void AddPlus(int x, int y, int value)
		{
			switch (value)
			{
				case 1: AddLabel(x, y, 0x35, "+"); break;
				case 2: AddLabel(x, y, 0x21, "+"); break;
			}
		}

		private void AddPlusMinus(int x, int y, int value)
		{
			switch (value)
			{
				case 0: AddLabel(x, y, 0x21, "-"); break;
				case 1: AddLabel(x, y, 0x35, "-"); break;
				case 3: AddLabel(x, y, 0x35, "+"); break;
				case 4: AddLabel(x, y, 0x21, "+"); break;
			}
		}

		private void AddLevel(int x, int y, int value)
		{
			AddLabel(x, y, 0x835, value.ToString());
		}

		private void AddGrowthIndicator(int x, int y)
		{
			if (!m_Plant.IsGrowable)
			{
				return;
			}

			switch (m_Plant.PlantSystem.GrowthIndicator)
			{
				case PlantGrowthIndicator.InvalidLocation: AddLabel(x, y, 0x21, "!"); break;
				case PlantGrowthIndicator.NotHealthy: AddLabel(x, y, 0x21, "-"); break;
				case PlantGrowthIndicator.Delay: AddLabel(x, y, 0x35, "-"); break;
				case PlantGrowthIndicator.Grown: AddLabel(x, y, 0x3, "+"); break;
				case PlantGrowthIndicator.DoubleGrown: AddLabel(x, y, 0x3F, "+"); break;
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var from = sender.Mobile;

			if (info.ButtonID == 0 || m_Plant.Deleted || m_Plant.PlantStatus >= PlantStatus.DecorativePlant)
			{
				return;
			}

			if (((info.ButtonID >= 6 && info.ButtonID <= 10) || info.ButtonID == 12) && !from.InRange(m_Plant.GetWorldLocation(), 3))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 500446); // That is too far away.
				return;
			}

			if (!m_Plant.IsUsableBy(from))
			{
				m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Reproduction menu
					{
						if (m_Plant.PlantStatus > PlantStatus.BowlOfDirt)
						{
							from.SendGump(new ReproductionGump(m_Plant));
						}
						else
						{
							from.SendLocalizedMessage(1061885); // You need to plant a seed in the bowl first.

							from.SendGump(new MainPlantGump(m_Plant));
						}

						break;
					}
				case 2: // Infestation
					{
						from.Send(new DisplayHelpTopic(54, true)); // INFESTATION LEVEL

						from.SendGump(new MainPlantGump(m_Plant));

						break;
					}
				case 3: // Fungus
					{
						from.Send(new DisplayHelpTopic(56, true)); // FUNGUS LEVEL

						from.SendGump(new MainPlantGump(m_Plant));

						break;
					}
				case 4: // Poison
					{
						from.Send(new DisplayHelpTopic(58, true)); // POISON LEVEL

						from.SendGump(new MainPlantGump(m_Plant));

						break;
					}
				case 5: // Disease
					{
						from.Send(new DisplayHelpTopic(60, true)); // DISEASE LEVEL

						from.SendGump(new MainPlantGump(m_Plant));

						break;
					}
				case 6: // Water
					{
						var item = from.Backpack.FindItemsByType(typeof(BaseBeverage));

						var foundUsableWater = false;

						if (item != null && item.Length > 0)
						{
							for (var i = 0; i < item.Length; ++i)
							{
								var beverage = (BaseBeverage)item[i];

								if (!beverage.IsEmpty && beverage.Pourable && beverage.Content == BeverageType.Water)
								{
									foundUsableWater = true;
									m_Plant.Pour(from, beverage);
									break;
								}
							}
						}

						if (!foundUsableWater)
						{
							from.Target = new PlantPourTarget(m_Plant);
							from.SendLocalizedMessage(1060808, "#" + m_Plant.GetLocalizedPlantStatus().ToString()); // Target the container you wish to use to water the ~1_val~.
						}

						from.SendGump(new MainPlantGump(m_Plant));

						break;
					}
				case 7: // Poison potion
					{
						AddPotion(from, PotionEffect.PoisonGreater, PotionEffect.PoisonDeadly);

						break;
					}
				case 8: // Cure potion
					{
						AddPotion(from, PotionEffect.CureGreater);

						break;
					}
				case 9: // Heal potion
					{
						AddPotion(from, PotionEffect.HealGreater);

						break;
					}
				case 10: // Strength potion
					{
						AddPotion(from, PotionEffect.StrengthGreater);

						break;
					}
				case 11: // Help
					{
						from.Send(new DisplayHelpTopic(48, true)); // PLANT GROWING

						from.SendGump(new MainPlantGump(m_Plant));

						break;
					}
				case 12: // Empty the bowl
					{
						from.SendGump(new EmptyTheBowlGump(m_Plant));

						break;
					}
			}
		}

		private void AddPotion(Mobile from, params PotionEffect[] effects)
		{
			var item = GetPotion(from, effects);

			if (item != null)
			{
				m_Plant.Pour(from, item);
			}
			else
			{
				int message;
				if (m_Plant.ApplyPotion(effects[0], true, out message))
				{
					from.SendLocalizedMessage(1061884); // You don't have any strong potions of that type in your pack.

					from.Target = new PlantPourTarget(m_Plant);
					from.SendLocalizedMessage(1060808, "#" + m_Plant.GetLocalizedPlantStatus().ToString()); // Target the container you wish to use to water the ~1_val~.

					return;
				}
				else
				{
					m_Plant.LabelTo(from, message);
				}
			}

			from.SendGump(new MainPlantGump(m_Plant));
		}

		public static Item GetPotion(Mobile from, PotionEffect[] effects)
		{
			if (from.Backpack == null)
			{
				return null;
			}

			var items = from.Backpack.FindItemsByType(new Type[] { typeof(BasePotion), typeof(PotionKeg) });

			foreach (var item in items)
			{
				if (item is BasePotion)
				{
					var potion = (BasePotion)item;

					if (Array.IndexOf(effects, potion.PotionEffect) >= 0)
					{
						return potion;
					}
				}
				else
				{
					var keg = (PotionKeg)item;

					if (keg.Held > 0 && Array.IndexOf(effects, keg.Type) >= 0)
					{
						return keg;
					}
				}
			}

			return null;
		}
	}

	public class SetToDecorativeGump : Gump
	{
		private readonly PlantItem m_Plant;

		public SetToDecorativeGump(PlantItem plant) : base(20, 20)
		{
			m_Plant = plant;

			DrawBackground();

			AddLabel(115, 85, 0x44, "Set plant");
			AddLabel(82, 105, 0x44, "to decorative mode?");

			AddButton(98, 140, 0x47E, 0x480, 1, GumpButtonType.Reply, 0); // Cancel

			AddButton(138, 141, 0xD2, 0xD2, 2, GumpButtonType.Reply, 0); // Help
			AddLabel(143, 141, 0x835, "?");

			AddButton(168, 140, 0x481, 0x483, 3, GumpButtonType.Reply, 0); // Ok
		}

		private void DrawBackground()
		{
			AddBackground(50, 50, 200, 150, 0xE10);

			AddItem(25, 45, 0xCEB);
			AddItem(25, 118, 0xCEC);

			AddItem(227, 45, 0xCEF);
			AddItem(227, 118, 0xCF0);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var from = sender.Mobile;

			if (info.ButtonID == 0 || m_Plant.Deleted || m_Plant.PlantStatus != PlantStatus.Stage9)
			{
				return;
			}

			if (info.ButtonID == 3 && !from.InRange(m_Plant.GetWorldLocation(), 3))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 500446); // That is too far away.
				return;
			}

			if (!m_Plant.IsUsableBy(from))
			{
				m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Cancel
					{
						from.SendGump(new ReproductionGump(m_Plant));

						break;
					}
				case 2: // Help
					{
						from.Send(new DisplayHelpTopic(70, true)); // DECORATIVE MODE

						from.SendGump(new SetToDecorativeGump(m_Plant));

						break;
					}
				case 3: // Ok
					{
						m_Plant.PlantStatus = PlantStatus.DecorativePlant;
						m_Plant.LabelTo(from, 1053077); // You prune the plant. This plant will no longer produce resources or seeds, but will require no upkeep.

						break;
					}
			}
		}
	}

	public class ReproductionGump : Gump
	{
		private readonly PlantItem m_Plant;

		public ReproductionGump(PlantItem plant) : base(20, 20)
		{
			m_Plant = plant;

			DrawBackground();

			AddButton(70, 67, 0xD4, 0xD4, 1, GumpButtonType.Reply, 0); // Main menu
			AddItem(57, 65, 0x1600);

			AddLabel(108, 67, 0x835, "Reproduction");

			if (m_Plant.PlantStatus == PlantStatus.Stage9)
			{
				AddButton(212, 67, 0xD4, 0xD4, 2, GumpButtonType.Reply, 0); // Set to decorative
				AddItem(202, 68, 0xC61);
				AddLabel(216, 66, 0x21, "/");
			}

			AddButton(80, 116, 0xD4, 0xD4, 3, GumpButtonType.Reply, 0); // Pollination
			AddItem(66, 117, 0x1AA2);
			AddPollinationState(106, 116);

			AddButton(128, 116, 0xD4, 0xD4, 4, GumpButtonType.Reply, 0); // Resources
			AddItem(113, 120, 0x1021);
			AddResourcesState(149, 116);

			AddButton(177, 116, 0xD4, 0xD4, 5, GumpButtonType.Reply, 0); // Seeds
			AddItem(160, 121, 0xDCF);
			AddSeedsState(199, 116);

			AddButton(70, 163, 0xD2, 0xD2, 6, GumpButtonType.Reply, 0); // Gather pollen
			AddItem(56, 164, 0x1AA2);

			AddButton(138, 163, 0xD2, 0xD2, 7, GumpButtonType.Reply, 0); // Gather resources
			AddItem(123, 167, 0x1021);

			AddButton(212, 163, 0xD2, 0xD2, 8, GumpButtonType.Reply, 0); // Gather seeds
			AddItem(195, 168, 0xDCF);
		}

		private void DrawBackground()
		{
			AddBackground(50, 50, 200, 150, 0xE10);

			AddImage(60, 90, 0xE17);
			AddImage(120, 90, 0xE17);

			AddImage(60, 145, 0xE17);
			AddImage(120, 145, 0xE17);

			AddItem(45, 45, 0xCEF);
			AddItem(45, 118, 0xCF0);

			AddItem(211, 45, 0xCEB);
			AddItem(211, 118, 0xCEC);
		}

		private void AddPollinationState(int x, int y)
		{
			var system = m_Plant.PlantSystem;

			if (!system.PollenProducing)
			{
				AddLabel(x, y, 0x35, "-");
			}
			else if (!system.Pollinated)
			{
				AddLabel(x, y, 0x21, "!");
			}
			else
			{
				AddLabel(x, y, 0x3F, "+");
			}
		}

		private void AddResourcesState(int x, int y)
		{
			var resInfo = PlantResourceInfo.GetInfo(m_Plant.PlantType, m_Plant.PlantHue);

			var system = m_Plant.PlantSystem;
			var totalResources = system.AvailableResources + system.LeftResources;

			if (resInfo == null || totalResources == 0)
			{
				AddLabel(x + 5, y, 0x21, "X");
			}
			else
			{
				AddLabel(x, y, PlantHueInfo.GetInfo(m_Plant.PlantHue).GumpHue,
					String.Format("{0}/{1}", system.AvailableResources, totalResources));
			}
		}

		private void AddSeedsState(int x, int y)
		{
			var system = m_Plant.PlantSystem;
			var totalSeeds = system.AvailableSeeds + system.LeftSeeds;

			if (!m_Plant.Reproduces || totalSeeds == 0)
			{
				AddLabel(x + 5, y, 0x21, "X");
			}
			else
			{
				AddLabel(x, y, PlantHueInfo.GetInfo(system.SeedHue).GumpHue,
					String.Format("{0}/{1}", system.AvailableSeeds, totalSeeds));
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var from = sender.Mobile;

			if (info.ButtonID == 0 || m_Plant.Deleted || m_Plant.PlantStatus >= PlantStatus.DecorativePlant || m_Plant.PlantStatus == PlantStatus.BowlOfDirt)
			{
				return;
			}

			if ((info.ButtonID >= 6 && info.ButtonID <= 8) && !from.InRange(m_Plant.GetWorldLocation(), 3))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 500446); // That is too far away.
				return;
			}

			if (!m_Plant.IsUsableBy(from))
			{
				m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Main menu
					{
						from.SendGump(new MainPlantGump(m_Plant));

						break;
					}
				case 2: // Set to decorative
					{
						if (m_Plant.PlantStatus == PlantStatus.Stage9)
						{
							from.SendGump(new SetToDecorativeGump(m_Plant));
						}

						break;
					}
				case 3: // Pollination
					{
						from.Send(new DisplayHelpTopic(67, true)); // POLLINATION STATE

						from.SendGump(new ReproductionGump(m_Plant));

						break;
					}
				case 4: // Resources
					{
						from.Send(new DisplayHelpTopic(69, true)); // RESOURCE PRODUCTION

						from.SendGump(new ReproductionGump(m_Plant));

						break;
					}
				case 5: // Seeds
					{
						from.Send(new DisplayHelpTopic(68, true)); // SEED PRODUCTION

						from.SendGump(new ReproductionGump(m_Plant));

						break;
					}
				case 6: // Gather pollen
					{
						if (!m_Plant.IsCrossable)
						{
							m_Plant.LabelTo(from, 1053050); // You cannot gather pollen from a mutated plant!
						}
						else if (!m_Plant.PlantSystem.PollenProducing)
						{
							m_Plant.LabelTo(from, 1053051); // You cannot gather pollen from a plant in this stage of development!
						}
						else if (m_Plant.PlantSystem.Health < PlantHealth.Healthy)
						{
							m_Plant.LabelTo(from, 1053052); // You cannot gather pollen from an unhealthy plant!
						}
						else
						{
							from.Target = new PollinateTarget(m_Plant);
							from.SendLocalizedMessage(1053054); // Target the plant you wish to cross-pollinate to.

							break;
						}

						from.SendGump(new ReproductionGump(m_Plant));

						break;
					}
				case 7: // Gather resources
					{
						var resInfo = PlantResourceInfo.GetInfo(m_Plant.PlantType, m_Plant.PlantHue);
						var system = m_Plant.PlantSystem;

						if (resInfo == null)
						{
							if (m_Plant.IsCrossable)
							{
								m_Plant.LabelTo(from, 1053056); // This plant has no resources to gather!
							}
							else
							{
								m_Plant.LabelTo(from, 1053055); // Mutated plants do not produce resources!
							}
						}
						else if (system.AvailableResources == 0)
						{
							m_Plant.LabelTo(from, 1053056); // This plant has no resources to gather!
						}
						else
						{
							var resource = resInfo.CreateResource();

							if (from.PlaceInBackpack(resource))
							{
								system.AvailableResources--;
								m_Plant.LabelTo(from, 1053059); // You gather resources from the plant.
							}
							else
							{
								resource.Delete();
								m_Plant.LabelTo(from, 1053058); // You attempt to gather as many resources as you can hold, but your backpack is full.
							}
						}

						from.SendGump(new ReproductionGump(m_Plant));

						break;
					}
				case 8: // Gather seeds
					{
						var system = m_Plant.PlantSystem;

						if (!m_Plant.Reproduces)
						{
							m_Plant.LabelTo(from, 1053060); // Mutated plants do not produce seeds!
						}
						else if (system.AvailableSeeds == 0)
						{
							m_Plant.LabelTo(from, 1053061); // This plant has no seeds to gather!
						}
						else
						{
							var seed = new Seed(system.SeedType, system.SeedHue, true);

							if (from.PlaceInBackpack(seed))
							{
								system.AvailableSeeds--;
								m_Plant.LabelTo(from, 1053063); // You gather seeds from the plant.
							}
							else
							{
								seed.Delete();
								m_Plant.LabelTo(from, 1053062); // You attempt to gather as many seeds as you can hold, but your backpack is full.
							}
						}

						from.SendGump(new ReproductionGump(m_Plant));

						break;
					}
			}
		}
	}

	public class EmptyTheBowlGump : Gump
	{
		private readonly PlantItem m_Plant;

		public EmptyTheBowlGump(PlantItem plant) : base(20, 20)
		{
			m_Plant = plant;

			DrawBackground();

			AddLabel(90, 70, 0x44, "Empty the bowl?");

			DrawPicture();

			AddButton(98, 150, 0x47E, 0x480, 1, GumpButtonType.Reply, 0); // Cancel

			AddButton(138, 151, 0xD2, 0xD2, 2, GumpButtonType.Reply, 0); // Help
			AddLabel(143, 151, 0x835, "?");

			AddButton(168, 150, 0x481, 0x483, 3, GumpButtonType.Reply, 0); // Ok
		}

		private void DrawBackground()
		{
			AddBackground(50, 50, 200, 150, 0xE10);

			AddItem(45, 45, 0xCEF);
			AddItem(45, 118, 0xCF0);

			AddItem(211, 45, 0xCEB);
			AddItem(211, 118, 0xCEC);
		}

		private void DrawPicture()
		{
			AddItem(90, 100, 0x1602);
			AddImage(140, 102, 0x15E1);
			AddItem(160, 100, 0x15FD);

			if (m_Plant.PlantStatus != PlantStatus.BowlOfDirt && m_Plant.PlantStatus < PlantStatus.Plant)
			{
				AddItem(156, 130, 0xDCF); // Seed
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var from = sender.Mobile;

			if (info.ButtonID == 0 || m_Plant.Deleted || m_Plant.PlantStatus >= PlantStatus.DecorativePlant)
			{
				return;
			}

			if (info.ButtonID == 3 && !from.InRange(m_Plant.GetWorldLocation(), 3))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 500446); // That is too far away.
				return;
			}

			if (!m_Plant.IsUsableBy(from))
			{
				m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Cancel
					{
						from.SendGump(new MainPlantGump(m_Plant));

						break;
					}
				case 2: // Help
					{
						from.Send(new DisplayHelpTopic(71, true)); // EMPTYING THE BOWL

						from.SendGump(new EmptyTheBowlGump(m_Plant));

						break;
					}
				case 3: // Ok
					{
						var bowl = new PlantBowl();

						if (!from.PlaceInBackpack(bowl))
						{
							bowl.Delete();

							m_Plant.LabelTo(from, 1053047); // You cannot empty a bowl with a full pack!
							from.SendGump(new MainPlantGump(m_Plant));

							break;
						}

						if (m_Plant.PlantStatus != PlantStatus.BowlOfDirt && m_Plant.PlantStatus < PlantStatus.Plant)
						{
							var seed = new Seed(m_Plant.PlantType, m_Plant.PlantHue, m_Plant.ShowType);

							if (!from.PlaceInBackpack(seed))
							{
								bowl.Delete();
								seed.Delete();

								m_Plant.LabelTo(from, 1053047); // You cannot empty a bowl with a full pack!
								from.SendGump(new MainPlantGump(m_Plant));

								break;
							}
						}

						m_Plant.Delete();

						break;
					}
			}
		}
	}


	/// Planting Targets
	public class PlantPourTarget : Target
	{
		private readonly PlantItem m_Plant;

		public PlantPourTarget(PlantItem plant) : base(3, true, TargetFlags.None)
		{
			m_Plant = plant;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!m_Plant.Deleted && from.InRange(m_Plant.GetWorldLocation(), 3) && targeted is Item)
			{
				m_Plant.Pour(from, (Item)targeted);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			if (!m_Plant.Deleted && m_Plant.PlantStatus < PlantStatus.DecorativePlant && from.InRange(m_Plant.GetWorldLocation(), 3) && m_Plant.IsUsableBy(from))
			{
				if (from.HasGump(typeof(MainPlantGump)))
				{
					from.CloseGump(typeof(MainPlantGump));
				}

				from.SendGump(new MainPlantGump(m_Plant));
			}
		}
	}

	public class PollinateTarget : Target
	{
		private readonly PlantItem m_Plant;

		public PollinateTarget(PlantItem plant) : base(3, true, TargetFlags.None)
		{
			m_Plant = plant;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!m_Plant.Deleted && m_Plant.PlantStatus < PlantStatus.DecorativePlant && from.InRange(m_Plant.GetWorldLocation(), 3))
			{
				if (!m_Plant.IsUsableBy(from))
				{
					m_Plant.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
				}
				else if (!m_Plant.IsCrossable)
				{
					m_Plant.LabelTo(from, 1053050); // You cannot gather pollen from a mutated plant!
				}
				else if (!m_Plant.PlantSystem.PollenProducing)
				{
					m_Plant.LabelTo(from, 1053051); // You cannot gather pollen from a plant in this stage of development!
				}
				else if (m_Plant.PlantSystem.Health < PlantHealth.Healthy)
				{
					m_Plant.LabelTo(from, 1053052); // You cannot gather pollen from an unhealthy plant!
				}
				else
				{
					var targ = targeted as PlantItem;

					if (targ == null || targ.PlantStatus >= PlantStatus.DecorativePlant || targ.PlantStatus <= PlantStatus.BowlOfDirt)
					{
						m_Plant.LabelTo(from, 1053070); // You can only pollinate other specially grown plants!
					}
					else if (!targ.IsUsableBy(from))
					{
						targ.LabelTo(from, 1061856); // You must have the item in your backpack or locked down in order to use it.
					}
					else if (!targ.IsCrossable)
					{
						targ.LabelTo(from, 1053073); // You cannot cross-pollinate with a mutated plant!
					}
					else if (!targ.PlantSystem.PollenProducing)
					{
						targ.LabelTo(from, 1053074); // This plant is not in the flowering stage. You cannot pollinate it!
					}
					else if (targ.PlantSystem.Health < PlantHealth.Healthy)
					{
						targ.LabelTo(from, 1053075); // You cannot pollinate an unhealthy plant!
					}
					else if (targ.PlantSystem.Pollinated)
					{
						targ.LabelTo(from, 1053072); // This plant has already been pollinated!
					}
					else if (targ == m_Plant)
					{
						targ.PlantSystem.Pollinated = true;
						targ.PlantSystem.SeedType = m_Plant.PlantType;
						targ.PlantSystem.SeedHue = m_Plant.PlantHue;

						targ.LabelTo(from, 1053071); // You pollinate the plant with its own pollen.
					}
					else
					{
						targ.PlantSystem.Pollinated = true;
						targ.PlantSystem.SeedType = PlantTypeInfo.Cross(m_Plant.PlantType, targ.PlantType);
						targ.PlantSystem.SeedHue = PlantHueInfo.Cross(m_Plant.PlantHue, targ.PlantHue);

						targ.LabelTo(from, 1053076); // You successfully cross-pollinate the plant.
					}
				}
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			if (!m_Plant.Deleted && m_Plant.PlantStatus < PlantStatus.DecorativePlant && m_Plant.PlantStatus != PlantStatus.BowlOfDirt && from.InRange(m_Plant.GetWorldLocation(), 3) && m_Plant.IsUsableBy(from))
			{
				from.SendGump(new ReproductionGump(m_Plant));
			}
		}
	}
}

namespace Server.Network
{
	public class DisplayHelpTopic : Packet
	{
		public DisplayHelpTopic(int topicID, bool display) : base(0xBF)
		{
			EnsureCapacity(11);

			m_Stream.Write((short)0x17);
			m_Stream.Write((byte)1);
			m_Stream.Write(topicID);
			m_Stream.Write(display);
		}
	}
}