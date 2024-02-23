using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Server
{
	public interface ISpecialMove
	{
		SpellInfo Info { get; }

		SpellName ID => Info.ID;
		SpellSchool School => Info.School;

		string Name => Info.Name;
		string Mantra => Info.Mantra;
		string Desc => Info.Desc;

		int BaseMana => Info.Mana;

		double RequiredSkill { get; }

		SkillName MoveSkill { get; }

		bool ValidatesDuringHit { get; }
		
		TextDefinition AbilityMessage { get; }

		bool IgnoreArmor(Mobile attacker);

		int GetAccuracyBonus(Mobile attacker);

		double GetPropertyBonus(Mobile attacker);
		double GetDamageScalar(Mobile attacker, Mobile defender);

		bool OnBeforeDamage(Mobile attacker, Mobile defender);
		bool OnBeforeSwing(Mobile attacker, Mobile defender);

		void OnHit(Mobile attacker, Mobile defender, int damage);
		void OnMiss(Mobile attacker, Mobile defender);

		void OnUse(Mobile from);

		void OnClearMove(Mobile from);

		bool Validate(Mobile from);
	}

	public interface ISpell
	{
		SpellInfo Info { get; }

		SpellName ID => Info.ID;
		SpellSchool School => Info.School;
		SpellCircle Circle => Info.Circle;

		string Name => Info.Name;
		string Mantra => Info.Mantra;
		string Desc => Info.Desc;

		Mobile Caster => null;

		bool IsCasting { get; }

		SkillName CastSkill { get; }
		SkillName DamageSkill => CastSkill;

		bool Cast();

		void SayMantra();

		void OnCasterHurt();
		void OnCasterKilled();
		void OnConnectionChanged();

		bool OnCasterMoving(Direction d);
		bool OnCasterEquiping(Item item);
		bool OnCasterUsingObject(object o);
		bool OnCastInTown(Region r);

		void GetCastSkills(ref double req, out double min, out double max);

		double GetSkillRequirement();
		int GetManaRequirement();
		int GetTitheRequirement();

		void Interrupt(SpellInterrupt type)
		{
			Interrupt(type, false);
		}

		void Interrupt(SpellInterrupt type, bool resistable);
	}

	public interface ISpellbook
	{
		SpellSchool School { get; }

		int BookOffset => SpellbookHelper.GetSchoolOffset(School);
		int BookCount => SpellbookHelper.GetSchoolCount(School);

		SpellbookTheme Theme => SpellbookTheme.GetTheme(School);

		int SpellCount { get; }

		ulong Content { get; set; }

		void DisplayTo(Mobile to);

		bool HasSpell(SpellName spell);
		bool AddSpell(SpellName spell);

		void Fill();
	}

	public enum SpellState
	{
		None = 0,
		Casting = 1,    // We are in the process of casting (that is, waiting GetCastTime() and doing animations). Spell casting may be interupted in this state.
		Sequencing = 2  // Casting completed, but the full spell sequence isn't. Usually waiting for a target response. Some actions are restricted in this state (using skills for example).
	}

	public enum SpellInterrupt
	{
		Unspecified,
		EquipRequest,
		UseRequest,
		Hurt,
		Kill,
		NewCast
	}

	public enum SpellSchool
	{
		Invalid = -1,

		// Base
		Magery = 0,
		Necromancy = 100,
		Chivalry = 200,
		Bushido = 400,
		Ninjitsu = 500,
		Spellweaving = 600,
		Mysticism = 677,

		// Custom
		Avatar = 1000,
		Cleric = 1100,
		Druid = 1200,
		Ranger = 1300,
		Rogue = 1400,

		// Racial
		Human = 50000,
		Elf = 50100,
		Gargoyle = 50200,
	}

	public enum SpellCircle
	{
		Invalid = -1,

		First,
		Second,
		Third,
		Fourth,
		Fifth,
		Sixth,
		Seventh,
		Eighth
	}

	public enum SpellName
	{
		Invalid = -1,

		#region Magery

		#region First

		Clumsy = 0,
		CreateFood = 1,
		Feeblemind = 2,
		Heal = 3,
		MagicArrow = 4,
		NightSight = 5,
		ReactiveArmor = 6,
		Weaken = 7,

		#endregion

		#region Second

		Agility = 8,
		Cunning = 9,
		Cure = 10,
		Harm = 11,
		MagicTrap = 12,
		RemoveTrap = 13,
		Protection = 14,
		Strength = 15,

		#endregion

		#region Third

		Bless = 16,
		Fireball = 17,
		MagicLock = 18,
		Poison = 19,
		Telekinesis = 20,
		Teleport = 21,
		Unlock = 22,
		WallOfStone = 23,

		#endregion

		#region Fourth

		ArchCure = 24,
		ArchProtection = 25,
		Curse = 26,
		FireField = 27,
		GreaterHeal = 28,
		Lightning = 29,
		ManaDrain = 30,
		Recall = 31,

		#endregion

		#region Fifth

		BladeSpirits = 32,
		DispelField = 33,
		Incognito = 34,
		MagicReflect = 35,
		MindBlast = 36,
		Paralyze = 37,
		PoisonField = 38,
		SummonCreature = 39,

		#endregion

		#region Sixth

		Dispel = 40,
		EnergyBolt = 41,
		Explosion = 42,
		Invisibility = 43,
		Mark = 44,
		MassCurse = 45,
		ParalyzeField = 46,
		Reveal = 47,

		#endregion

		#region Seventh

		ChainLightning = 48,
		EnergyField = 49,
		FlameStrike = 50,
		GateTravel = 51,
		ManaVampire = 52,
		MassDispel = 53,
		MeteorSwarm = 54,
		Polymorph = 55,

		#endregion

		#region Eighth

		Earthquake = 56,
		EnergyVortex = 57,
		Resurrection = 58,
		AirElemental = 59,
		SummonDaemon = 60,
		EarthElemental = 61,
		FireElemental = 62,
		WaterElemental = 63,

		#endregion

		#endregion

		#region Necromancy

		AnimateDead = 100,
		BloodOath = 101,
		CorpseSkin = 102,
		CurseWeapon = 103,
		EvilOmen = 104,
		HorrificBeast = 105,
		LichForm = 106,
		MindRot = 107,
		PainSpike = 108,
		PoisonStrike = 109,
		Strangle = 110,
		SummonFamiliar = 111,
		VampiricEmbrace = 112,
		VengefulSpirit = 113,
		Wither = 114,
		WraithForm = 115,
		Exorcism = 116,

		#endregion

		#region Chivalry

		CleanseByFire = 200,
		CloseWounds = 201,
		ConsecrateWeapon = 202,
		DispelEvil = 203,
		DivineFury = 204,
		EnemyOfOne = 205,
		HolyLight = 206,
		NobleSacrifice = 207,
		RemoveCurse = 208,
		SacredJourney = 209,

		#endregion

		#region Bushido

		HonorableExecution = 400,
		Confidence = 401,
		Evasion = 402,
		CounterAttack = 403,
		LightningStrike = 404,
		MomentumStrike = 405,

		#endregion

		#region Ninjitsu

		FocusAttack = 500,
		DeathStrike = 501,
		AnimalForm = 502,
		KiAttack = 503,
		SurpriseAttack = 504,
		Backstab = 505,
		ShadowJump = 506,
		MirrorImage = 507,

		#endregion

		#region Spellweaving

		ArcaneCircle = 600,
		GiftOfRenewal = 601,
		ImmolatingWeapon = 602,
		Attunement = 603,
		Thunderstorm = 604,
		NaturesFury = 605,
		SummonFey = 606,
		SummonFiend = 607,
		ReaperForm = 608,
		Wildfire = 609,
		EssenceOfWind = 610,
		DryadAllure = 611,
		EtherealVoyage = 612,
		WordOfDeath = 613,
		GiftOfLife = 614,
		ArcaneEmpowerment = 615,

		#endregion

		#region Mysticism

		NetherBolt = 677,
		HealingStone = 678,
		PurgeMagic = 679,
		Enchant = 680,
		Sleep = 681,
		EagleStrike = 682,
		AnimatedWeapon = 683,
		StoneForm = 684,
		SpellTrigger = 685,
		MassSleep = 686,
		CleansingWinds = 687,
		Bombard = 688,
		SpellPlague = 689,
		HailStorm = 690,
		NetherCyclone = 691,
		RisingColossus = 692,

		#endregion

		// Custom

		#region Avatar

		DivineLight = 1000,
		DivineGateway = 1001,
		MarkOfGods = 1002,

		#endregion

		#region Cleric

		AngelicFaith = 1100,
		BanishEvil = 1101,
		DampenSpirit = 1102,
		DivineFocus = 1103,
		HammerOfFaith = 1104,
		Purge = 1105,
		Restoration = 1106,
		SacredBoon = 1107,
		Sacrifice = 1108,
		Smite = 1109,
		TouchOfLife = 1110,
		TrialByFire = 1111,

		#endregion

		#region Druid

		BeastPack = 1200,
		BlendWithForest = 1201,
		DruidFamiliar = 1202,
		EnchantedGrove = 1203,
		GraspingRoots = 1204,
		HollowReed = 1205,
		LeafWhirlwind = 1206,
		LureStone = 1207,
		MushroomGateway = 1208,
		NaturesPassage = 1209,
		RestorativeSoil = 1210,
		ShieldOfEarth = 1211,
		SpringOfLife = 1212,
		StoneCircle = 1213,
		SwarmOfInsects = 1214,
		VolcanicEruption = 1215,

		#endregion

		#region Ranger

		HuntersAim = 1300,
		PhoenixFlight = 1301,
		AnimalCompanion = 1302,
		CallMount = 1303,
		FireBow = 1304,
		IceBow = 1305,
		LightningBow = 1306,
		NoxBow = 1307,

		#endregion

		#region Rogue

		Intimidation = 1400,
		ShadowBlend = 1401,
		SlyFox = 1402,

		#endregion

		// Racial

		#region Human

		StrongBack = 50000,
		Toughness = 50001,
		WorkHorse = 50002,
		JackOfAllTrades = 50003,
		MasterArtisan = 50004,

		#endregion

		#region Elf

		NightVision = 50100,
		InfusedWithMagic = 50101,
		KnowledgeOfNature = 50102,
		Evasive = 50103,
		Perceptive = 50104,
		Wisdom = 50105,

		#endregion

		#region Gargoyle

		Flying = 50200,
		Berserk = 50201,
		DeadlyAim = 50202,
		MysticInsight = 50203,

		#endregion
	}

	#region [School]SpellName

	#region Standard

	public enum MagerySpellName
	{
		Clumsy = SpellName.Clumsy,
		CreateFood = SpellName.CreateFood,
		Feeblemind = SpellName.Feeblemind,
		Heal = SpellName.Heal,
		MagicArrow = SpellName.MagicArrow,
		NightSight = SpellName.NightSight,
		ReactiveArmor = SpellName.ReactiveArmor,
		Weaken = SpellName.Weaken,

		Agility = SpellName.Agility,
		Cunning = SpellName.Cunning,
		Cure = SpellName.Cure,
		Harm = SpellName.Harm,
		MagicTrap = SpellName.MagicTrap,
		RemoveTrap = SpellName.RemoveTrap,
		Protection = SpellName.Protection,
		Strength = SpellName.Strength,

		Bless = SpellName.Bless,
		Fireball = SpellName.Fireball,
		MagicLock = SpellName.MagicLock,
		Poison = SpellName.Poison,
		Telekinesis = SpellName.Telekinesis,
		Teleport = SpellName.Teleport,
		Unlock = SpellName.Unlock,
		WallOfStone = SpellName.WallOfStone,

		ArchCure = SpellName.ArchCure,
		ArchProtection = SpellName.ArchProtection,
		Curse = SpellName.Curse,
		FireField = SpellName.FireField,
		GreaterHeal = SpellName.GreaterHeal,
		Lightning = SpellName.Lightning,
		ManaDrain = SpellName.ManaDrain,
		Recall = SpellName.Recall,

		BladeSpirits = SpellName.BladeSpirits,
		DispelField = SpellName.DispelField,
		Incognito = SpellName.Incognito,
		MagicReflect = SpellName.MagicReflect,
		MindBlast = SpellName.MindBlast,
		Paralyze = SpellName.Paralyze,
		PoisonField = SpellName.PoisonField,
		SummonCreature = SpellName.SummonCreature,

		Dispel = SpellName.Dispel,
		EnergyBolt = SpellName.EnergyBolt,
		Explosion = SpellName.Explosion,
		Invisibility = SpellName.Invisibility,
		Mark = SpellName.Mark,
		MassCurse = SpellName.MassCurse,
		ParalyzeField = SpellName.ParalyzeField,
		Reveal = SpellName.Reveal,

		ChainLightning = SpellName.ChainLightning,
		EnergyField = SpellName.EnergyField,
		FlameStrike = SpellName.FlameStrike,
		GateTravel = SpellName.GateTravel,
		ManaVampire = SpellName.ManaVampire,
		MassDispel = SpellName.MassDispel,
		MeteorSwarm = SpellName.MeteorSwarm,
		Polymorph = SpellName.Polymorph,

		Earthquake = SpellName.Earthquake,
		EnergyVortex = SpellName.EnergyVortex,
		Resurrection = SpellName.Resurrection,
		AirElemental = SpellName.AirElemental,
		SummonDaemon = SpellName.SummonDaemon,
		EarthElemental = SpellName.EarthElemental,
		FireElemental = SpellName.FireElemental,
		WaterElemental = SpellName.WaterElemental,
	}

	public enum NecromancySpellName
	{
		AnimateDead = SpellName.AnimateDead,
		BloodOath = SpellName.BloodOath,
		CorpseSkin = SpellName.CorpseSkin,
		CurseWeapon = SpellName.CurseWeapon,
		EvilOmen = SpellName.EvilOmen,
		HorrificBeast = SpellName.HorrificBeast,
		LichForm = SpellName.LichForm,
		MindRot = SpellName.MindRot,
		PainSpike = SpellName.PainSpike,
		PoisonStrike = SpellName.PoisonStrike,
		Strangle = SpellName.Strangle,
		SummonFamiliar = SpellName.SummonFamiliar,
		VampiricEmbrace = SpellName.VampiricEmbrace,
		VengefulSpirit = SpellName.VengefulSpirit,
		Wither = SpellName.Wither,
		WraithForm = SpellName.WraithForm,
		Exorcism = SpellName.Exorcism,
	}

	public enum ChivalrySpellName
	{
		CleanseByFire = SpellName.CleanseByFire,
		CloseWounds = SpellName.CloseWounds,
		ConsecrateWeapon = SpellName.ConsecrateWeapon,
		DispelEvil = SpellName.DispelEvil,
		DivineFury = SpellName.DivineFury,
		EnemyOfOne = SpellName.EnemyOfOne,
		HolyLight = SpellName.HolyLight,
		NobleSacrifice = SpellName.NobleSacrifice,
		RemoveCurse = SpellName.RemoveCurse,
		SacredJourney = SpellName.SacredJourney,
	}

	public enum BushidoSpellName
	{
		HonorableExecution = SpellName.HonorableExecution,
		Confidence = SpellName.Confidence,
		Evasion = SpellName.Evasion,
		CounterAttack = SpellName.CounterAttack,
		LightningStrike = SpellName.LightningStrike,
		MomentumStrike = SpellName.MomentumStrike,
	}

	public enum NinjitsuSpellName
	{
		FocusAttack = SpellName.FocusAttack,
		DeathStrike = SpellName.DeathStrike,
		AnimalForm = SpellName.AnimalForm,
		KiAttack = SpellName.KiAttack,
		SurpriseAttack = SpellName.SurpriseAttack,
		Backstab = SpellName.Backstab,
		ShadowJump = SpellName.ShadowJump,
		MirrorImage = SpellName.MirrorImage,
	}

	public enum SpellweavingSpellName
	{
		ArcaneCircle = SpellName.ArcaneCircle,
		GiftOfRenewal = SpellName.GiftOfRenewal,
		ImmolatingWeapon = SpellName.ImmolatingWeapon,
		Attunement = SpellName.Attunement,
		Thunderstorm = SpellName.Thunderstorm,
		NaturesFury = SpellName.NaturesFury,
		SummonFey = SpellName.SummonFey,
		SummonFiend = SpellName.SummonFiend,
		ReaperForm = SpellName.ReaperForm,
		Wildfire = SpellName.Wildfire,
		EssenceOfWind = SpellName.EssenceOfWind,
		DryadAllure = SpellName.DryadAllure,
		EtherealVoyage = SpellName.EtherealVoyage,
		WordOfDeath = SpellName.WordOfDeath,
		GiftOfLife = SpellName.GiftOfLife,
		ArcaneEmpowerment = SpellName.ArcaneEmpowerment,
	}

	public enum MysticismSpellName
	{
		NetherBolt = SpellName.NetherBolt,
		HealingStone = SpellName.HealingStone,
		PurgeMagic = SpellName.PurgeMagic,
		Enchant = SpellName.Enchant,
		Sleep = SpellName.Sleep,
		EagleStrike = SpellName.EagleStrike,
		AnimatedWeapon = SpellName.AnimatedWeapon,
		StoneForm = SpellName.StoneForm,
		SpellTrigger = SpellName.SpellTrigger,
		MassSleep = SpellName.MassSleep,
		CleansingWinds = SpellName.CleansingWinds,
		Bombard = SpellName.Bombard,
		SpellPlague = SpellName.SpellPlague,
		HailStorm = SpellName.HailStorm,
		NetherCyclone = SpellName.NetherCyclone,
		RisingColossus = SpellName.RisingColossus,
	}

	#endregion

	#region Custom

	public enum AvatarSpellName
	{
		DivineLight = SpellName.DivineLight,
		DivineGateway = SpellName.DivineGateway,
		MarkOfGods = SpellName.MarkOfGods,
	}

	public enum ClericSpellName
	{
		AngelicFaith = SpellName.AngelicFaith,
		BanishEvil = SpellName.BanishEvil,
		DampenSpirit = SpellName.DampenSpirit,
		DivineFocus = SpellName.DivineFocus,
		HammerOfFaith = SpellName.HammerOfFaith,
		Purge = SpellName.Purge,
		Restoration = SpellName.Restoration,
		SacredBoon = SpellName.SacredBoon,
		Sacrifice = SpellName.Sacrifice,
		Smite = SpellName.Smite,
		TouchOfLife = SpellName.TouchOfLife,
		TrialByFire = SpellName.TrialByFire,
	}

	public enum DruidSpellName
	{
		BeastPack = SpellName.BeastPack,
		BlendWithForest = SpellName.BlendWithForest,
		DruidFamiliar = SpellName.DruidFamiliar,
		EnchantedGrove = SpellName.EnchantedGrove,
		GraspingRoots = SpellName.GraspingRoots,
		HollowReed = SpellName.HollowReed,
		LeafWhirlwind = SpellName.LeafWhirlwind,
		LureStone = SpellName.LureStone,
		MushroomGateway = SpellName.MushroomGateway,
		NaturesPassage = SpellName.NaturesPassage,
		RestorativeSoil = SpellName.RestorativeSoil,
		ShieldOfEarth = SpellName.ShieldOfEarth,
		SpringOfLife = SpellName.SpringOfLife,
		StoneCircle = SpellName.StoneCircle,
		SwarmOfInsects = SpellName.SwarmOfInsects,
		VolcanicEruption = SpellName.VolcanicEruption,
	}

	public enum RangerSpellName
	{
		HuntersAim = SpellName.HuntersAim,
		PhoenixFlight = SpellName.PhoenixFlight,
		AnimalCompanion = SpellName.AnimalCompanion,
		CallMount = SpellName.CallMount,
		FireBow = SpellName.FireBow,
		IceBow = SpellName.IceBow,
		LightningBow = SpellName.LightningBow,
		NoxBow = SpellName.NoxBow,
	}

	public enum RogueSpellName
	{
		Intimidation = SpellName.Intimidation,
		ShadowBlend = SpellName.ShadowBlend,
		SlyFox = SpellName.SlyFox,
	}

	#endregion

	#region Racial

	public enum HumanAbilityName
	{
		StrongBack = SpellName.StrongBack,
		Toughness = SpellName.Toughness,
		WorkHorse = SpellName.WorkHorse,
		JackOfAllTrades = SpellName.JackOfAllTrades,
		MasterArtisan = SpellName.MasterArtisan,
	}

	public enum ElfAbilityName
	{
		NightVision = SpellName.NightVision,
		InfusedWithMagic = SpellName.InfusedWithMagic,
		KnowledgeOfNature = SpellName.KnowledgeOfNature,
		Evasive = SpellName.Evasive,
		Perceptive = SpellName.Perceptive,
		Wisdom = SpellName.Wisdom,
	}

	public enum GargoyleAbilityName
	{
		Flying = SpellName.Flying,
		Berserk = SpellName.Berserk,
		DeadlyAim = SpellName.DeadlyAim,
		MysticInsight = SpellName.MysticInsight,
	}

	public enum RacialAbilityName
	{
		StrongBack = HumanAbilityName.StrongBack,
		Toughness = HumanAbilityName.Toughness,
		WorkHorse = HumanAbilityName.WorkHorse,
		JackOfAllTrades = HumanAbilityName.JackOfAllTrades,
		MasterArtisan = HumanAbilityName.MasterArtisan,

		NightVision = ElfAbilityName.NightVision,
		InfusedWithMagic = ElfAbilityName.InfusedWithMagic,
		KnowledgeOfNature = ElfAbilityName.KnowledgeOfNature,
		Evasive = ElfAbilityName.Evasive,
		Perceptive = ElfAbilityName.Perceptive,
		Wisdom = ElfAbilityName.Wisdom,

		Flying = GargoyleAbilityName.Flying,
		Berserk = GargoyleAbilityName.Berserk,
		DeadlyAim = GargoyleAbilityName.DeadlyAim,
		MysticInsight = GargoyleAbilityName.MysticInsight,
	}

	#endregion

	#endregion

	public static class SpellNames
	{
		private static SpellName[] m_Instances;

		public static IReadOnlyCollection<SpellName> Instances => InternalGetInstances(ref m_Instances);

		#region Schools

		#region Standard

		private static MagerySpellName[] m_Magery;

		public static IReadOnlyCollection<MagerySpellName> Magery => InternalGetInstances(ref m_Magery);

		private static NecromancySpellName[] m_Necromancy;

		public static IReadOnlyCollection<NecromancySpellName> Necromancy => InternalGetInstances(ref m_Necromancy);

		private static ChivalrySpellName[] m_Chivalry;

		public static IReadOnlyCollection<ChivalrySpellName> Chivalry => InternalGetInstances(ref m_Chivalry);

		private static BushidoSpellName[] m_Bushido;

		public static IReadOnlyCollection<BushidoSpellName> Bushido => InternalGetInstances(ref m_Bushido);

		private static NinjitsuSpellName[] m_Ninjitsu;

		public static IReadOnlyCollection<NinjitsuSpellName> Ninjitsu => InternalGetInstances(ref m_Ninjitsu);

		private static SpellweavingSpellName[] m_Spellweaving;

		public static IReadOnlyCollection<SpellweavingSpellName> Spellweaving => InternalGetInstances(ref m_Spellweaving);

		private static MysticismSpellName[] m_Mysticism;

		public static IReadOnlyCollection<MysticismSpellName> Mysticism => InternalGetInstances(ref m_Mysticism);

		#endregion

		#region Custom

		private static AvatarSpellName[] m_Avatar;

		public static IReadOnlyCollection<AvatarSpellName> Avatar => InternalGetInstances(ref m_Avatar);

		private static ClericSpellName[] m_Cleric;

		public static IReadOnlyCollection<ClericSpellName> Cleric => InternalGetInstances(ref m_Cleric);

		private static DruidSpellName[] m_Druid;

		public static IReadOnlyCollection<DruidSpellName> Druid => InternalGetInstances(ref m_Druid);

		private static RangerSpellName[] m_Ranger;

		public static IReadOnlyCollection<RangerSpellName> Ranger => InternalGetInstances(ref m_Ranger);

		private static RogueSpellName[] m_Rogue;

		public static IReadOnlyCollection<RogueSpellName> Rogue => InternalGetInstances(ref m_Rogue);

		#endregion

		#region Racial

		private static HumanAbilityName[] m_Human;

		public static IReadOnlyCollection<HumanAbilityName> Human => InternalGetInstances(ref m_Human);

		private static ElfAbilityName[] m_Elf;

		public static IReadOnlyCollection<ElfAbilityName> Elf => InternalGetInstances(ref m_Elf);

		private static GargoyleAbilityName[] m_Gargoyle;

		public static IReadOnlyCollection<GargoyleAbilityName> Gargoyle => InternalGetInstances(ref m_Gargoyle);

		#endregion

		#endregion

		[MethodImpl(MethodImplOptions.Synchronized)]
		private static T[] InternalGetInstances<T>(ref T[] list) where T : struct, Enum
		{
			if (list != null)
			{
				return list;
			}

			var values = Enum.GetValues<T>();

			var offset = typeof(T) == typeof(SpellName) ? 1 : 0;

			list = new T[values.Length - offset];

			Array.Copy(values, offset, list, 0, list.Length);

			return list;
		}
	}

	public sealed class SpellReagents : TypeAmounts
	{
		public override int DefaultAmountMin => 0;

		public SpellReagents()
		{
		}

		public SpellReagents(params TypeAmount[] entries) 
			: base(entries)
		{
		}

		public SpellReagents(IEnumerable<TypeAmount> entries) 
			: base(entries)
		{
		}

		public SpellReagents(GenericReader reader) 
			: base(reader)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadEncodedInt();
		}
	}

	public sealed class SpellInfo
	{
		public static SpellInfo CreateInvalid()
		{
			return new SpellInfo(typeof(ISpell))
			{
				Enabled = false,
				Action = -1,
				AllowTown = false,
			};
		}

		public Type Type { get; }

		public SpellName ID { get; }
		public SpellSchool School { get; }
		public SpellCircle Circle { get; }

		public string Name { get; set; } = String.Empty;
		public string Mantra { get; set; } = String.Empty;
		public string Desc { get; set; } = String.Empty;

		public bool Enabled { get; set; } = true;

		public int Icon { get; set; } = 0;
		public int Back { get; set; } = 0;

		public int Action { get; set; } = 16;

		public int LeftHandEffect { get; set; } = 0;
		public int RightHandEffect { get; set; } = 0;

		public bool AllowTown { get; set; } = true;

		public int Mana { get; set; } = 0;
		public int Tithe { get; set; } = 0;
		public double Skill { get; set; } = 0.0;

		public SpellReagents Reagents { get; } = new();

		public int ReagentsCount => Reagents.Count;

		public IEnumerable<Type> ReagentTypes => Reagents.Types;
		public IEnumerable<int> ReagentAmounts => Reagents.Amounts;

		private bool? m_IsValid;

		public bool IsValid => m_IsValid ??= Type != null && ID != SpellName.Invalid && School != SpellSchool.Invalid;

		private bool? m_IsSpecial;

		public bool IsSpecial => m_IsSpecial ??= Type != null && !Type.IsAssignableTo(typeof(ISpell));

		public SpellInfo(Type type)
			: this(type, SpellName.Invalid, SpellSchool.Invalid)
		{ }

        public SpellInfo(Type type, SpellName id, SpellSchool school)
			: this(type, id, school, SpellCircle.Invalid)
		{ }

		public SpellInfo(Type type, SpellName id, SpellSchool school, SpellCircle circle)
		{
			Type = type;
			ID = id;
			School = school;
			Circle = circle;
		}
	}

	public readonly record struct SpellbookTheme : IEquatable<SpellbookTheme>
	{
		public static readonly SpellbookTheme Invalid = new(SpellSchool.Invalid, Color.DarkSlateGray, 2203, 0, 2362, 2361, 0, 0, "Invalid", "Concepts");

		static SpellbookTheme()
		{
			// Base
			Register(SpellSchool.Magery, Color.DarkSlateGray, 2203, 0, 2362, 2361, 0, 0, "Magery", "Spells");
			Register(SpellSchool.Necromancy, Color.DarkSlateGray, 2203, 0, 2362, 2361, 0, 0, "Necromancy", "Spells");
			Register(SpellSchool.Chivalry, Color.DarkSlateGray, 2203, 0, 2362, 2361, 0, 0, "Chivalry", "Spells");
			Register(SpellSchool.Bushido, Color.DarkSlateGray, 2203, 0, 2362, 2361, 0, 0, "Bushido", "Abilities");
			Register(SpellSchool.Ninjitsu, Color.DarkSlateGray, 2203, 0, 2362, 2361, 0, 0, "Ninjitsu", "Abilities");
			Register(SpellSchool.Spellweaving, Color.DarkSlateGray, 2203, 0, 2362, 2361, 0x8A2, 0x8FD, "Spellweaving", "Spells");
			Register(SpellSchool.Mysticism, Color.DarkSlateGray, 2203, 0, 2362, 2361, 0, 0, "Mysticism", "Spells");

			// Class
			Register(SpellSchool.Avatar, Color.Goldenrod, 2202, 0, 2362, 2361, 0x8A7, 0x8A7, "Avatar", "Spells");
			Register(SpellSchool.Cleric, Color.DarkSlateGray, 2202, 0, 2362, 2361, 0xAA8, 0xAA8, "Cleric", "Spells");
			Register(SpellSchool.Druid, Color.RosyBrown, 2202, 0, 2362, 2361, 0x7D3, 0x7D3, "Druid", "Spells");
			Register(SpellSchool.Ranger, Color.DarkSlateGray, 2202, 0, 2362, 2361, 0x9F6, 0x9F6, "Ranger", "Abilities");
			Register(SpellSchool.Rogue, Color.IndianRed, 2202, 0, 2362, 2361, 0x89D, 0x89D, "Rogue", "Abilities");

			// Racial
			Register(SpellSchool.Human, Color.DarkSlateGray, 2200, 0, 2224, 2224, 0, 0, "Human", "Abilities");
			Register(SpellSchool.Elf, Color.DarkSlateGray, 2200, 0, 2224, 2224, 0, 0, "Elven", "Abilities");
			Register(SpellSchool.Gargoyle, Color.DarkSlateGray, 2200, 0, 2224, 2224, 0, 0, "Gargish", "Abilities");
		}

		public static void Register(SpellSchool school, Color textColor, int backgroundID, int backgroundHue, int castButtonID1, int castButtonID2, int bookHue, int scrollHue, string name, string summary)
		{
			if (school != SpellSchool.Invalid)
			{
				_Themes[school] = new(school, textColor, backgroundID, backgroundHue, castButtonID1, castButtonID2, bookHue, scrollHue, name, summary);
			}
		}

		private static readonly Dictionary<SpellSchool, SpellbookTheme> _Themes = new()
		{
			[SpellSchool.Invalid] = Invalid
		};

		public static SpellbookTheme GetTheme(SpellSchool school)
		{
			if (!_Themes.TryGetValue(school, out var theme))
			{
				theme = Invalid;
			}

			return theme;
		}

		public readonly SpellSchool School;

		public readonly Color TextColor;

		public readonly int BackgroundID;
		public readonly int BackgroundHue;

		public readonly int CastButtonID1;
		public readonly int CastButtonID2;

		public readonly int BookHue;
		public readonly int ScrollHue;

		public readonly string Name;
		public readonly string Summary;

		public SpellbookTheme(SpellSchool school, Color textColor, int backgroundID, int backgroundHue, int castButtonID1, int castButtonID2, int bookHue, int scrollHue, string name, string summary)
		{
			School = school;

			TextColor = textColor;

			BackgroundID = backgroundID;
			BackgroundHue = backgroundHue;

			CastButtonID1 = castButtonID1;
			CastButtonID2 = castButtonID2;

			BookHue = bookHue;
			ScrollHue = scrollHue;

			Name = name;
			Summary = summary;
		}

		public override string ToString()
		{
			return $"{Name} {Summary}";
		}
	}

	public static class SpellbookHelper
	{
		private static readonly Dictionary<Mobile, HashSet<ISpellbook>> m_Table = new();

		public static int GetSpellIndex(SpellName spell)
		{
			return (int)spell - GetSchoolOffset(SpellRegistry.GetSchool(spell));
		}

		public static int GetSchoolOffset(SpellSchool school)
		{
			return (int)school;
		}

		public static int GetSchoolCount(SpellSchool school)
		{
			return school switch
			{
				// Standard
				SpellSchool.Magery => SpellNames.Magery.Count,
				SpellSchool.Necromancy => SpellNames.Necromancy.Count,
				SpellSchool.Chivalry => SpellNames.Chivalry.Count,
				SpellSchool.Bushido => SpellNames.Bushido.Count,
				SpellSchool.Ninjitsu => SpellNames.Ninjitsu.Count,
				SpellSchool.Spellweaving => SpellNames.Spellweaving.Count,
				SpellSchool.Mysticism => SpellNames.Mysticism.Count,

				// Custom
				SpellSchool.Avatar => SpellNames.Avatar.Count,
				SpellSchool.Cleric => SpellNames.Cleric.Count,
				SpellSchool.Druid => SpellNames.Druid.Count,
				SpellSchool.Ranger => SpellNames.Ranger.Count,
				SpellSchool.Rogue => SpellNames.Rogue.Count,

				// Racial
				SpellSchool.Human => SpellNames.Human.Count,
				SpellSchool.Elf => SpellNames.Elf.Count,
				SpellSchool.Gargoyle => SpellNames.Gargoyle.Count,

				// Default
				_ => 0,
			};
		}

		public static ISpellbook Find(Mobile from, SpellName spell)
		{
			return Find(from, spell, SpellRegistry.GetSchool(spell));
		}

		public static ISpellbook Find(Mobile from, SpellName spell, SpellSchool school)
		{
			if (from == null)
			{
				return null;
			}

			if (from.Deleted)
			{
				m_Table.Remove(from);
				return null;
			}

			var searchAgain = false;

			if (!m_Table.TryGetValue(from, out var list) || list == null)
			{
				m_Table[from] = list = FindAllSpellbooks(from);
			}
			else
			{
				searchAgain = true;
			}

			var book = FindSpellbookInList(list, from, spell, school);

			if (book == null && searchAgain)
			{
				m_Table[from] = list = FindAllSpellbooks(from);

				book = FindSpellbookInList(list, from, spell, school);
			}

			return book;
		}

		private static ISpellbook FindSpellbookInList(HashSet<ISpellbook> list, Mobile from, SpellName spell, SpellSchool school)
		{
			var pack = from.Backpack;

			list.RemoveWhere(book => book is not Item item || item.Deleted || (item.Parent != from && (pack == null || item.Parent != pack)));

			foreach (var book in list)
			{
				if (ValidateSpellbook(book, spell, school))
				{
					return book;
				}
			}

			return null;
		}

		private static HashSet<ISpellbook> FindAllSpellbooks(Mobile from)
		{
			var list = new HashSet<ISpellbook>();

			var item = from.FindItemOnLayer(Layer.OneHanded);

			if (item is ISpellbook book)
			{
				list.Add(book);
			}

			var pack = from.Backpack;

			if (pack == null)
			{
				return list;
			}

			for (var i = 0; i < pack.Items.Count; ++i)
			{
				item = pack.Items[i];

				if (item is ISpellbook sb)
				{
					list.Add(sb);
				}
			}

			return list;
		}

		public static ISpellbook FindEquippedSpellbook(Mobile from)
		{
			return from.FindItemOnLayer(Layer.OneHanded) as ISpellbook;
		}

		public static bool ValidateSpellbook(ISpellbook book, SpellName spell, SpellSchool school)
		{
			return book.School == school && (spell == SpellName.Invalid || book.HasSpell(spell));
		}
	}

	public static class SpellRegistry
	{
		private static readonly Dictionary<Type, SpellName> m_Types = new();
		private static readonly Dictionary<SpellName, SpellInfo> m_Info = new();
		private static readonly Dictionary<SpellName, ISpecialMove> m_Specials = new();
		private static readonly Dictionary<SpellSchool, SortedSet<SpellName>> m_Schools = new();

		public static IReadOnlyCollection<Type> Types => m_Types.Keys;
		public static IReadOnlyCollection<SpellName> IDs => m_Info.Keys;
		public static IReadOnlyCollection<SpellInfo> Info => m_Info.Values;
		public static IReadOnlyCollection<SpellSchool> Schools => m_Schools.Keys;

		public static IReadOnlyCollection<SpellName> SpecialIds => m_Specials.Keys;
		public static IReadOnlyCollection<ISpecialMove> SpecialMoves => m_Specials.Values;

		public static void Register(SpellInfo info)
		{
			if (info?.IsValid != true)
			{
				return;
			}

			m_Info[info.ID] = info;
			m_Types[info.Type] = info.ID;

			if (!m_Schools.TryGetValue(info.School, out var ids))
			{
				m_Schools[info.School] = ids = new();
			}

			ids.Add(info.ID);

			if (info.IsSpecial && !m_Specials.TryGetValue(info.ID, out var spm))
			{
				try
				{
					spm = (ISpecialMove)Activator.CreateInstance(info.Type);
				}
				catch
				{
				}

				if (spm != null)
				{
					m_Specials[info.ID] = spm;
				}
			}
		}

		public static int CountSpells(this SpellSchool school)
		{
			if (m_Schools.TryGetValue(school, out var ids))
			{
				return ids.Count;
			}

			return 0;
		}

		public static IEnumerable<SpellName> GetSpells(this SpellSchool school)
		{
			if (m_Schools.TryGetValue(school, out var ids))
			{
				foreach (var id in ids)
				{
					yield return id;
				}
			}
		}

		public static IEnumerable<SpellInfo> GetInfo(this SpellSchool school)
		{
			foreach (var id in GetSpells(school))
			{
				yield return GetInfo(id);
			}
		}

		public static SpellInfo GetInfo(SpellName id)
		{
			if (m_Info.TryGetValue(id, out var info))
			{
				return info;
			}

			return null;
		}

		public static SpellInfo GetInfo(Type type)
		{
			if (m_Types.TryGetValue(type, out var id))
			{
				return GetInfo(id);
			}

			return null;
		}

		public static bool IsEnabled(SpellName id)
		{
			var info = GetInfo(id);

			return info?.Enabled == true;
		}

		public static string GetName(SpellName id)
		{
			var info = GetInfo(id);

			return info?.Name;
		}

		public static string GetMantra(SpellName id)
		{
			var info = GetInfo(id);

			return info?.Mantra;
		}

		public static Type GetType(SpellName id)
		{
			var info = GetInfo(id);

			return info?.Type;
		}

		public static SpellName GetID(Type type)
		{
			var info = GetInfo(type);

			return info?.ID ?? SpellName.Invalid;
		}

		public static SpellSchool GetSchool(SpellName id)
		{
			var info = GetInfo(id);

			return info?.School ?? SpellSchool.Invalid;
		}

		public static SpellCircle GetCircle(SpellName id)
		{
			var info = GetInfo(id);

			return info?.Circle ?? SpellCircle.Invalid;
		}

		public static SpellName GetID(ISpecialMove s)
		{
			return GetID(s?.GetType());
		}

		public static ISpecialMove GetSpecialMove(Type type)
		{
			var info = GetInfo(type);

			if (info?.IsValid == true)
			{
				return GetSpecialMove(info.ID);
			}

			return null;
		}

		public static ISpecialMove GetSpecialMove(SpellName id)
		{
			if (id >= 0 && m_Specials.TryGetValue(id, out var sm))
			{
				return sm;
			}

			return null;
		}

		public static ISpell NewSpell(SpellName id, Mobile caster, Item scroll)
		{
			var info = GetInfo(id);

			if (info?.IsValid == true && !info.IsSpecial && info.Enabled)
			{
				try
				{
					return (ISpell)Activator.CreateInstance(info.Type, caster, scroll);
				}
				catch
				{
				}
			}

			return null;
		}

		public static ISpell NewSpell(string name, Mobile caster, Item scroll)
		{
			if (Enum.TryParse<SpellName>(name, out var id))
			{
				return NewSpell(id, caster, scroll);
			}

			foreach (var spell in m_Info.Values)
			{
				if (Insensitive.StartsWith(spell.Name, name))
				{
					return NewSpell(spell.ID, caster, scroll);
				}
			}

			return null;
		}
	}

	[NoSort, PropertyObject]
	public abstract class SpellStates<T> : BaseStates<SpellName, T>
	{
		#region Schools

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract MagerySpellStates<T> Magery { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract NecromancySpellStates<T> Necromancy { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract ChivalrySpellStates<T> Chivalry { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract BushidoSpellStates<T> Bushido { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract NinjitsuSpellStates<T> Ninjitsu { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract SpellweavingSpellStates<T> Spellweaving { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract MysticismSpellStates<T> Mysticism { get; protected set; }

		// Custom

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract AvatarSpellStates<T> Avatar { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract ClericSpellStates<T> Cleric { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract DruidSpellStates<T> Druid { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract RangerSpellStates<T> Ranger { get; protected set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public abstract RogueSpellStates<T> Rogue { get; protected set; }

		#endregion

		protected IEnumerable<IStates<T>> SubStates
		{
			get
			{
				yield return Magery;
				yield return Necromancy;
				yield return Chivalry;
				yield return Bushido;
				yield return Ninjitsu;
				yield return Spellweaving;
				yield return Mysticism;

				// Custom

				yield return Avatar;
				yield return Cleric;
				yield return Druid;
				yield return Ranger;
				yield return Rogue;
			}
		}

		public T this[MagerySpellName spell] { get => Magery[spell]; set => Magery[spell] = value; }
		public T this[NecromancySpellName spell] { get => Necromancy[spell]; set => Necromancy[spell] = value; }
		public T this[ChivalrySpellName spell] { get => Chivalry[spell]; set => Chivalry[spell] = value; }
		public T this[BushidoSpellName spell] { get => Bushido[spell]; set => Bushido[spell] = value; }
		public T this[NinjitsuSpellName spell] { get => Ninjitsu[spell]; set => Ninjitsu[spell] = value; }
		public T this[SpellweavingSpellName spell] { get => Spellweaving[spell]; set => Spellweaving[spell] = value; }
		public T this[MysticismSpellName spell] { get => Mysticism[spell]; set => Mysticism[spell] = value; }

		// Custom

		public T this[AvatarSpellName spell] { get => Avatar[spell]; set => Avatar[spell] = value; }
		public T this[ClericSpellName spell] { get => Cleric[spell]; set => Cleric[spell] = value; }
		public T this[DruidSpellName spell] { get => Druid[spell]; set => Druid[spell] = value; }
		public T this[RangerSpellName spell] { get => Ranger[spell]; set => Ranger[spell] = value; }
		public T this[RogueSpellName spell] { get => Rogue[spell]; set => Rogue[spell] = value; }

		public T this[int spell] { get => this[(SpellName)spell]; set => this[(SpellName)spell] = value; }

		public override sealed int Length { get; } = EnumValues.Length;

		public SpellStates()
			: base(0)
		{
		}

		public SpellStates(GenericReader reader)
			: base(0, reader)
		{
		}

		public override void Clear()
		{
			base.Clear();

			foreach (var states in SubStates)
			{
				states.Clear();
			}
		}

		protected override T Get(SpellName spell)
		{
			var index = Array.IndexOf(EnumValues, spell);

			if (index < 0)
			{
				return default;
			}

			var skipped = 0;

			foreach (var states in SubStates)
			{
				if (index >= skipped + states.Length)
				{
					skipped += states.Length;
					continue;
				}

				return states.Data[index - skipped];
			}

			return default;
		}

		protected override void Set(SpellName spell, T value)
		{
			var index = Array.IndexOf(EnumValues, spell);

			if (index < 0)
			{
				return;
			}

			var skipped = 0;

			foreach (var states in SubStates)
			{
				if (index >= skipped + states.Length)
				{
					skipped += states.Length;
					continue;
				}

				states.Data[index - skipped] = value;
				return;
			}
		}

		public override IEnumerator<T> GetEnumerator()
		{
			foreach (var states in SubStates)
			{
				foreach (var value in states)
				{
					yield return value;
				}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			Magery.Serialize(writer);
			Necromancy.Serialize(writer);
			Chivalry.Serialize(writer);
			Bushido.Serialize(writer);
			Ninjitsu.Serialize(writer);
			Spellweaving.Serialize(writer);
			Mysticism.Serialize(writer);

			// Custom

			Avatar.Serialize(writer);
			Cleric.Serialize(writer);
			Druid.Serialize(writer);
			Ranger.Serialize(writer);
			Rogue.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();

			Magery.Deserialize(reader);
			Necromancy.Deserialize(reader);
			Chivalry.Deserialize(reader);
			Bushido.Deserialize(reader);
			Ninjitsu.Deserialize(reader);
			Spellweaving.Deserialize(reader);
			Mysticism.Deserialize(reader);

			// Custom

			Avatar.Deserialize(reader);
			Cleric.Deserialize(reader);
			Druid.Deserialize(reader);
			Ranger.Deserialize(reader);
			Rogue.Deserialize(reader);
		}
	}

	[NoSort, PropertyObject]
	public abstract class MagerySpellStates<T> : BaseStates<MagerySpellName, T>
	{
		#region Magery

		#region First

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Clumsy { get => this[MagerySpellName.Clumsy]; set => this[MagerySpellName.Clumsy] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T CreateFood { get => this[MagerySpellName.CreateFood]; set => this[MagerySpellName.CreateFood] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Feeblemind { get => this[MagerySpellName.Feeblemind]; set => this[MagerySpellName.Feeblemind] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Heal { get => this[MagerySpellName.Heal]; set => this[MagerySpellName.Heal] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MagicArrow { get => this[MagerySpellName.MagicArrow]; set => this[MagerySpellName.MagicArrow] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T NightSight { get => this[MagerySpellName.NightSight]; set => this[MagerySpellName.NightSight] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ReactiveArmor { get => this[MagerySpellName.ReactiveArmor]; set => this[MagerySpellName.ReactiveArmor] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Weaken { get => this[MagerySpellName.Weaken]; set => this[MagerySpellName.Weaken] = value; }

		#endregion

		#region Second

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Agility { get => this[MagerySpellName.Agility]; set => this[MagerySpellName.Agility] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Cunning { get => this[MagerySpellName.Cunning]; set => this[MagerySpellName.Cunning] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Cure { get => this[MagerySpellName.Cure]; set => this[MagerySpellName.Cure] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Harm { get => this[MagerySpellName.Harm]; set => this[MagerySpellName.Harm] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MagicTrap { get => this[MagerySpellName.MagicTrap]; set => this[MagerySpellName.MagicTrap] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T RemoveTrap { get => this[MagerySpellName.RemoveTrap]; set => this[MagerySpellName.RemoveTrap] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Protection { get => this[MagerySpellName.Protection]; set => this[MagerySpellName.Protection] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Strength { get => this[MagerySpellName.Strength]; set => this[MagerySpellName.Strength] = value; }

		#endregion

		#region Third

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Bless { get => this[MagerySpellName.Bless]; set => this[MagerySpellName.Bless] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Fireball { get => this[MagerySpellName.Fireball]; set => this[MagerySpellName.Fireball] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MagicLock { get => this[MagerySpellName.MagicLock]; set => this[MagerySpellName.MagicLock] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Poison { get => this[MagerySpellName.Poison]; set => this[MagerySpellName.Poison] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Telekinesis { get => this[MagerySpellName.Telekinesis]; set => this[MagerySpellName.Telekinesis] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Teleport { get => this[MagerySpellName.Teleport]; set => this[MagerySpellName.Teleport] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Unlock { get => this[MagerySpellName.Unlock]; set => this[MagerySpellName.Unlock] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T WallOfStone { get => this[MagerySpellName.WallOfStone]; set => this[MagerySpellName.WallOfStone] = value; }

		#endregion

		#region Fourth

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ArchCure { get => this[MagerySpellName.ArchCure]; set => this[MagerySpellName.ArchCure] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ArchProtection { get => this[MagerySpellName.ArchProtection]; set => this[MagerySpellName.ArchProtection] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Curse { get => this[MagerySpellName.Curse]; set => this[MagerySpellName.Curse] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T FireField { get => this[MagerySpellName.FireField]; set => this[MagerySpellName.FireField] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T GreaterHeal { get => this[MagerySpellName.GreaterHeal]; set => this[MagerySpellName.GreaterHeal] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Lightning { get => this[MagerySpellName.Lightning]; set => this[MagerySpellName.Lightning] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ManaDrain { get => this[MagerySpellName.ManaDrain]; set => this[MagerySpellName.ManaDrain] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Recall { get => this[MagerySpellName.Recall]; set => this[MagerySpellName.Recall] = value; }

		#endregion

		#region Fifth

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T BladeSpirits { get => this[MagerySpellName.BladeSpirits]; set => this[MagerySpellName.BladeSpirits] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DispelField { get => this[MagerySpellName.DispelField]; set => this[MagerySpellName.DispelField] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Incognito { get => this[MagerySpellName.Incognito]; set => this[MagerySpellName.Incognito] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MagicReflect { get => this[MagerySpellName.MagicReflect]; set => this[MagerySpellName.MagicReflect] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MindBlast { get => this[MagerySpellName.MindBlast]; set => this[MagerySpellName.MindBlast] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Paralyze { get => this[MagerySpellName.Paralyze]; set => this[MagerySpellName.Paralyze] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T PoisonField { get => this[MagerySpellName.PoisonField]; set => this[MagerySpellName.PoisonField] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SummonCreature { get => this[MagerySpellName.SummonCreature]; set => this[MagerySpellName.SummonCreature] = value; }

		#endregion

		#region Sixth

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Dispel { get => this[MagerySpellName.Dispel]; set => this[MagerySpellName.Dispel] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EnergyBolt { get => this[MagerySpellName.EnergyBolt]; set => this[MagerySpellName.EnergyBolt] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Explosion { get => this[MagerySpellName.Explosion]; set => this[MagerySpellName.Explosion] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Invisibility { get => this[MagerySpellName.Invisibility]; set => this[MagerySpellName.Invisibility] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Mark { get => this[MagerySpellName.Mark]; set => this[MagerySpellName.Mark] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MassCurse { get => this[MagerySpellName.MassCurse]; set => this[MagerySpellName.MassCurse] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ParalyzeField { get => this[MagerySpellName.ParalyzeField]; set => this[MagerySpellName.ParalyzeField] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Reveal { get => this[MagerySpellName.Reveal]; set => this[MagerySpellName.Reveal] = value; }

		#endregion

		#region Seventh

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ChainLightning { get => this[MagerySpellName.ChainLightning]; set => this[MagerySpellName.ChainLightning] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EnergyField { get => this[MagerySpellName.EnergyField]; set => this[MagerySpellName.EnergyField] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T FlameStrike { get => this[MagerySpellName.FlameStrike]; set => this[MagerySpellName.FlameStrike] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T GateTravel { get => this[MagerySpellName.GateTravel]; set => this[MagerySpellName.GateTravel] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ManaVampire { get => this[MagerySpellName.ManaVampire]; set => this[MagerySpellName.ManaVampire] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MassDispel { get => this[MagerySpellName.MassDispel]; set => this[MagerySpellName.MassDispel] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MeteorSwarm { get => this[MagerySpellName.MeteorSwarm]; set => this[MagerySpellName.MeteorSwarm] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Polymorph { get => this[MagerySpellName.Polymorph]; set => this[MagerySpellName.Polymorph] = value; }

		#endregion

		#region Eighth

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Earthquake { get => this[MagerySpellName.Earthquake]; set => this[MagerySpellName.Earthquake] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EnergyVortex { get => this[MagerySpellName.EnergyVortex]; set => this[MagerySpellName.EnergyVortex] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Resurrection { get => this[MagerySpellName.Resurrection]; set => this[MagerySpellName.Resurrection] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T AirElemental { get => this[MagerySpellName.AirElemental]; set => this[MagerySpellName.AirElemental] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SummonDaemon { get => this[MagerySpellName.SummonDaemon]; set => this[MagerySpellName.SummonDaemon] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EarthElemental { get => this[MagerySpellName.EarthElemental]; set => this[MagerySpellName.EarthElemental] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T FireElemental { get => this[MagerySpellName.FireElemental]; set => this[MagerySpellName.FireElemental] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T WaterElemental { get => this[MagerySpellName.WaterElemental]; set => this[MagerySpellName.WaterElemental] = value; }

		#endregion

		#endregion

		//public T this[int spell] { get => this[(MagerySpellName)spell]; set => this[(MagerySpellName)spell] = value; }

		public MagerySpellStates()
		{
		}

		public MagerySpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class NecromancySpellStates<T> : BaseStates<NecromancySpellName, T>
	{
		#region Necromancy

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T AnimateDead { get => this[NecromancySpellName.AnimateDead]; set => this[NecromancySpellName.AnimateDead] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T BloodOath { get => this[NecromancySpellName.BloodOath]; set => this[NecromancySpellName.BloodOath] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T CorpseSkin { get => this[NecromancySpellName.CorpseSkin]; set => this[NecromancySpellName.CorpseSkin] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T CurseWeapon { get => this[NecromancySpellName.CurseWeapon]; set => this[NecromancySpellName.CurseWeapon] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EvilOmen { get => this[NecromancySpellName.EvilOmen]; set => this[NecromancySpellName.EvilOmen] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T HorrificBeast { get => this[NecromancySpellName.HorrificBeast]; set => this[NecromancySpellName.HorrificBeast] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T LichForm { get => this[NecromancySpellName.LichForm]; set => this[NecromancySpellName.LichForm] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MindRot { get => this[NecromancySpellName.MindRot]; set => this[NecromancySpellName.MindRot] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T PainSpike { get => this[NecromancySpellName.PainSpike]; set => this[NecromancySpellName.PainSpike] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T PoisonStrike { get => this[NecromancySpellName.PoisonStrike]; set => this[NecromancySpellName.PoisonStrike] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Strangle { get => this[NecromancySpellName.Strangle]; set => this[NecromancySpellName.Strangle] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SummonFamiliar { get => this[NecromancySpellName.SummonFamiliar]; set => this[NecromancySpellName.SummonFamiliar] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T VampiricEmbrace { get => this[NecromancySpellName.VampiricEmbrace]; set => this[NecromancySpellName.VampiricEmbrace] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T VengefulSpirit { get => this[NecromancySpellName.VengefulSpirit]; set => this[NecromancySpellName.VengefulSpirit] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Wither { get => this[NecromancySpellName.Wither]; set => this[NecromancySpellName.Wither] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T WraithForm { get => this[NecromancySpellName.WraithForm]; set => this[NecromancySpellName.WraithForm] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Exorcism { get => this[NecromancySpellName.Exorcism]; set => this[NecromancySpellName.Exorcism] = value; }

		#endregion

		//public T this[int spell] { get => this[(NecromancySpellName)spell]; set => this[(NecromancySpellName)spell] = value; }

		public NecromancySpellStates()
		{
		}

		public NecromancySpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class ChivalrySpellStates<T> : BaseStates<ChivalrySpellName, T>
	{
		#region Chivalry

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T CleanseByFire { get => this[ChivalrySpellName.CleanseByFire]; set => this[ChivalrySpellName.CleanseByFire] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T CloseWounds { get => this[ChivalrySpellName.CloseWounds]; set => this[ChivalrySpellName.CloseWounds] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ConsecrateWeapon { get => this[ChivalrySpellName.ConsecrateWeapon]; set => this[ChivalrySpellName.ConsecrateWeapon] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DispelEvil { get => this[ChivalrySpellName.DispelEvil]; set => this[ChivalrySpellName.DispelEvil] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DivineFury { get => this[ChivalrySpellName.DivineFury]; set => this[ChivalrySpellName.DivineFury] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EnemyOfOne { get => this[ChivalrySpellName.EnemyOfOne]; set => this[ChivalrySpellName.EnemyOfOne] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T HolyLight { get => this[ChivalrySpellName.HolyLight]; set => this[ChivalrySpellName.HolyLight] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T NobleSacrifice { get => this[ChivalrySpellName.NobleSacrifice]; set => this[ChivalrySpellName.NobleSacrifice] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T RemoveCurse { get => this[ChivalrySpellName.RemoveCurse]; set => this[ChivalrySpellName.RemoveCurse] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SacredJourney { get => this[ChivalrySpellName.SacredJourney]; set => this[ChivalrySpellName.SacredJourney] = value; }

		#endregion

		//public T this[int spell] { get => this[(ChivalrySpellName)spell]; set => this[(ChivalrySpellName)spell] = value; }

		public ChivalrySpellStates()
		{
		}

		public ChivalrySpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class BushidoSpellStates<T> : BaseStates<BushidoSpellName, T>
	{
		#region Bushido

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T HonorableExecution { get => this[BushidoSpellName.HonorableExecution]; set => this[BushidoSpellName.HonorableExecution] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Confidence { get => this[BushidoSpellName.Confidence]; set => this[BushidoSpellName.Confidence] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Evasion { get => this[BushidoSpellName.Evasion]; set => this[BushidoSpellName.Evasion] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T CounterAttack { get => this[BushidoSpellName.CounterAttack]; set => this[BushidoSpellName.CounterAttack] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T LightningStrike { get => this[BushidoSpellName.LightningStrike]; set => this[BushidoSpellName.LightningStrike] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MomentumStrike { get => this[BushidoSpellName.MomentumStrike]; set => this[BushidoSpellName.MomentumStrike] = value; }

		#endregion

		//public T this[int spell] { get => this[(BushidoSpellName)spell]; set => this[(BushidoSpellName)spell] = value; }

		public BushidoSpellStates()
		{
		}

		public BushidoSpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class NinjitsuSpellStates<T> : BaseStates<NinjitsuSpellName, T>
	{
		#region Ninjitsu

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T FocusAttack { get => this[NinjitsuSpellName.FocusAttack]; set => this[NinjitsuSpellName.FocusAttack] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DeathStrike { get => this[NinjitsuSpellName.DeathStrike]; set => this[NinjitsuSpellName.DeathStrike] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T AnimalForm { get => this[NinjitsuSpellName.AnimalForm]; set => this[NinjitsuSpellName.AnimalForm] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T KiAttack { get => this[NinjitsuSpellName.KiAttack]; set => this[NinjitsuSpellName.KiAttack] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SurpriseAttack { get => this[NinjitsuSpellName.SurpriseAttack]; set => this[NinjitsuSpellName.SurpriseAttack] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Backstab { get => this[NinjitsuSpellName.Backstab]; set => this[NinjitsuSpellName.Backstab] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ShadowJump { get => this[NinjitsuSpellName.ShadowJump]; set => this[NinjitsuSpellName.ShadowJump] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MirrorImage { get => this[NinjitsuSpellName.MirrorImage]; set => this[NinjitsuSpellName.MirrorImage] = value; }

		#endregion

		//public T this[int spell] { get => this[(NinjitsuSpellName)spell]; set => this[(NinjitsuSpellName)spell] = value; }

		public NinjitsuSpellStates()
		{
		}

		public NinjitsuSpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class SpellweavingSpellStates<T> : BaseStates<SpellweavingSpellName, T>
	{
		#region Spellweaving

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ArcaneCircle { get => this[SpellweavingSpellName.ArcaneCircle]; set => this[SpellweavingSpellName.ArcaneCircle] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T GiftOfRenewal { get => this[SpellweavingSpellName.GiftOfRenewal]; set => this[SpellweavingSpellName.GiftOfRenewal] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ImmolatingWeapon { get => this[SpellweavingSpellName.ImmolatingWeapon]; set => this[SpellweavingSpellName.ImmolatingWeapon] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Attunement { get => this[SpellweavingSpellName.Attunement]; set => this[SpellweavingSpellName.Attunement] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Thunderstorm { get => this[SpellweavingSpellName.Thunderstorm]; set => this[SpellweavingSpellName.Thunderstorm] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T NaturesFury { get => this[SpellweavingSpellName.NaturesFury]; set => this[SpellweavingSpellName.NaturesFury] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SummonFey { get => this[SpellweavingSpellName.SummonFey]; set => this[SpellweavingSpellName.SummonFey] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SummonFiend { get => this[SpellweavingSpellName.SummonFiend]; set => this[SpellweavingSpellName.SummonFiend] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ReaperForm { get => this[SpellweavingSpellName.ReaperForm]; set => this[SpellweavingSpellName.ReaperForm] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Wildfire { get => this[SpellweavingSpellName.Wildfire]; set => this[SpellweavingSpellName.Wildfire] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EssenceOfWind { get => this[SpellweavingSpellName.EssenceOfWind]; set => this[SpellweavingSpellName.EssenceOfWind] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DryadAllure { get => this[SpellweavingSpellName.DryadAllure]; set => this[SpellweavingSpellName.DryadAllure] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EtherealVoyage { get => this[SpellweavingSpellName.EtherealVoyage]; set => this[SpellweavingSpellName.EtherealVoyage] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T WordOfDeath { get => this[SpellweavingSpellName.WordOfDeath]; set => this[SpellweavingSpellName.WordOfDeath] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T GiftOfLife { get => this[SpellweavingSpellName.GiftOfLife]; set => this[SpellweavingSpellName.GiftOfLife] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ArcaneEmpowerment { get => this[SpellweavingSpellName.ArcaneEmpowerment]; set => this[SpellweavingSpellName.ArcaneEmpowerment] = value; }

		#endregion

		//public T this[int spell] { get => this[(SpellweavingSpellName)spell]; set => this[(SpellweavingSpellName)spell] = value; }

		public SpellweavingSpellStates()
		{
		}

		public SpellweavingSpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class MysticismSpellStates<T> : BaseStates<MysticismSpellName, T>
	{
		#region Mysticism

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T NetherBolt { get => this[MysticismSpellName.NetherBolt]; set => this[MysticismSpellName.NetherBolt] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T HealingStone { get => this[MysticismSpellName.HealingStone]; set => this[MysticismSpellName.HealingStone] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T PurgeMagic { get => this[MysticismSpellName.PurgeMagic]; set => this[MysticismSpellName.PurgeMagic] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Enchant { get => this[MysticismSpellName.Enchant]; set => this[MysticismSpellName.Enchant] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Sleep { get => this[MysticismSpellName.Sleep]; set => this[MysticismSpellName.Sleep] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EagleStrike { get => this[MysticismSpellName.EagleStrike]; set => this[MysticismSpellName.EagleStrike] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T AnimatedWeapon { get => this[MysticismSpellName.AnimatedWeapon]; set => this[MysticismSpellName.AnimatedWeapon] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T StoneForm { get => this[MysticismSpellName.StoneForm]; set => this[MysticismSpellName.StoneForm] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SpellTrigger { get => this[MysticismSpellName.SpellTrigger]; set => this[MysticismSpellName.SpellTrigger] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MassSleep { get => this[MysticismSpellName.MassSleep]; set => this[MysticismSpellName.MassSleep] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T CleansingWinds { get => this[MysticismSpellName.CleansingWinds]; set => this[MysticismSpellName.CleansingWinds] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Bombard { get => this[MysticismSpellName.Bombard]; set => this[MysticismSpellName.Bombard] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SpellPlague { get => this[MysticismSpellName.SpellPlague]; set => this[MysticismSpellName.SpellPlague] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T HailStorm { get => this[MysticismSpellName.HailStorm]; set => this[MysticismSpellName.HailStorm] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T NetherCyclone { get => this[MysticismSpellName.NetherCyclone]; set => this[MysticismSpellName.NetherCyclone] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T RisingColossus { get => this[MysticismSpellName.RisingColossus]; set => this[MysticismSpellName.RisingColossus] = value; }

		#endregion

		//public T this[int spell] { get => this[(MysticismSpellName)spell]; set => this[(MysticismSpellName)spell] = value; }

		public MysticismSpellStates()
		{
		}

		public MysticismSpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class AvatarSpellStates<T> : BaseStates<AvatarSpellName, T>
	{
		#region Avatar

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DivineLight { get => this[AvatarSpellName.DivineLight]; set => this[AvatarSpellName.DivineLight] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DivineGateway { get => this[AvatarSpellName.DivineGateway]; set => this[AvatarSpellName.DivineGateway] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MarkOfGods { get => this[AvatarSpellName.MarkOfGods]; set => this[AvatarSpellName.MarkOfGods] = value; }

		#endregion

		//public T this[int spell] { get => this[(AvatarSpellName)spell]; set => this[(AvatarSpellName)spell] = value; }

		public AvatarSpellStates()
		{
		}

		public AvatarSpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class ClericSpellStates<T> : BaseStates<ClericSpellName, T>
	{
		#region Cleric

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T AngelicFaith { get => this[ClericSpellName.AngelicFaith]; set => this[ClericSpellName.AngelicFaith] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T BanishEvil { get => this[ClericSpellName.BanishEvil]; set => this[ClericSpellName.BanishEvil] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DampenSpirit { get => this[ClericSpellName.DampenSpirit]; set => this[ClericSpellName.DampenSpirit] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DivineFocus { get => this[ClericSpellName.DivineFocus]; set => this[ClericSpellName.DivineFocus] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T HammerOfFaith { get => this[ClericSpellName.HammerOfFaith]; set => this[ClericSpellName.HammerOfFaith] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Purge { get => this[ClericSpellName.Purge]; set => this[ClericSpellName.Purge] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Restoration { get => this[ClericSpellName.Restoration]; set => this[ClericSpellName.Restoration] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SacredBoon { get => this[ClericSpellName.SacredBoon]; set => this[ClericSpellName.SacredBoon] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Sacrifice { get => this[ClericSpellName.Sacrifice]; set => this[ClericSpellName.Sacrifice] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Smite { get => this[ClericSpellName.Smite]; set => this[ClericSpellName.Smite] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T TouchOfLife { get => this[ClericSpellName.TouchOfLife]; set => this[ClericSpellName.TouchOfLife] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T TiralByFire { get => this[ClericSpellName.TrialByFire]; set => this[ClericSpellName.TrialByFire] = value; }

		#endregion

		//public T this[int spell] { get => this[(ClericSpellName)spell]; set => this[(ClericSpellName)spell] = value; }

		public ClericSpellStates()
		{
		}

		public ClericSpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class DruidSpellStates<T> : BaseStates<DruidSpellName, T>
	{
		#region Druid

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T BeastPack { get => this[DruidSpellName.BeastPack]; set => this[DruidSpellName.BeastPack] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T BlendWithForest { get => this[DruidSpellName.BlendWithForest]; set => this[DruidSpellName.BlendWithForest] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T DruidFamiliar { get => this[DruidSpellName.DruidFamiliar]; set => this[DruidSpellName.DruidFamiliar] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T EnchantedGrove { get => this[DruidSpellName.EnchantedGrove]; set => this[DruidSpellName.EnchantedGrove] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T GraspingRoots { get => this[DruidSpellName.GraspingRoots]; set => this[DruidSpellName.GraspingRoots] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T HollowReed { get => this[DruidSpellName.HollowReed]; set => this[DruidSpellName.HollowReed] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T LeafWhirlwind { get => this[DruidSpellName.LeafWhirlwind]; set => this[DruidSpellName.LeafWhirlwind] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T LureStone { get => this[DruidSpellName.LureStone]; set => this[DruidSpellName.LureStone] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T MushroomGateway { get => this[DruidSpellName.MushroomGateway]; set => this[DruidSpellName.MushroomGateway] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T NaturesPassage { get => this[DruidSpellName.NaturesPassage]; set => this[DruidSpellName.NaturesPassage] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T RestorativeSoil { get => this[DruidSpellName.RestorativeSoil]; set => this[DruidSpellName.RestorativeSoil] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ShieldOfEarth { get => this[DruidSpellName.ShieldOfEarth]; set => this[DruidSpellName.ShieldOfEarth] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SpringOfLife { get => this[DruidSpellName.SpringOfLife]; set => this[DruidSpellName.SpringOfLife] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T StoneCircle { get => this[DruidSpellName.StoneCircle]; set => this[DruidSpellName.StoneCircle] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SwarmOfInsects { get => this[DruidSpellName.SwarmOfInsects]; set => this[DruidSpellName.SwarmOfInsects] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T VolcanicEruption { get => this[DruidSpellName.VolcanicEruption]; set => this[DruidSpellName.VolcanicEruption] = value; }

		#endregion

		//public T this[int spell] { get => this[(DruidSpellName)spell]; set => this[(DruidSpellName)spell] = value; }

		public DruidSpellStates()
		{
		}

		public DruidSpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class RangerSpellStates<T> : BaseStates<RangerSpellName, T>
	{
		#region Ranger

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T HuntersAim { get => this[RangerSpellName.HuntersAim]; set => this[RangerSpellName.HuntersAim] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T PhoenixFlight { get => this[RangerSpellName.PhoenixFlight]; set => this[RangerSpellName.PhoenixFlight] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T AnimalCompanion { get => this[RangerSpellName.AnimalCompanion]; set => this[RangerSpellName.AnimalCompanion] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T CallMount { get => this[RangerSpellName.CallMount]; set => this[RangerSpellName.CallMount] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T FireBow { get => this[RangerSpellName.FireBow]; set => this[RangerSpellName.FireBow] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T IceBow { get => this[RangerSpellName.IceBow]; set => this[RangerSpellName.IceBow] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T LightningBow { get => this[RangerSpellName.LightningBow]; set => this[RangerSpellName.LightningBow] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T NoxBow { get => this[RangerSpellName.NoxBow]; set => this[RangerSpellName.NoxBow] = value; }

		#endregion

		//public T this[int spell] { get => this[(RangerSpellName)spell]; set => this[(RangerSpellName)spell] = value; }

		public RangerSpellStates()
		{
		}

		public RangerSpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort, PropertyObject]
	public abstract class RogueSpellStates<T> : BaseStates<RogueSpellName, T>
	{
		#region Rogue

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T Intimidation { get => this[RogueSpellName.Intimidation]; set => this[RogueSpellName.Intimidation] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T ShadowBlend { get => this[RogueSpellName.ShadowBlend]; set => this[RogueSpellName.ShadowBlend] = value; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public T SlyFox { get => this[RogueSpellName.SlyFox]; set => this[RogueSpellName.SlyFox] = value; }

		#endregion

		//public T this[int spell] { get => this[(RogueSpellName)spell]; set => this[(RogueSpellName)spell] = value; }

		public RogueSpellStates()
		{
		}

		public RogueSpellStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort]
	public class SpellPermissions : SpellStates<bool>
	{
		#region Schools

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override MagerySpellStates<bool> Magery { get; protected set; } = new MagerySpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override NecromancySpellStates<bool> Necromancy { get; protected set; } = new NecromancySpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override ChivalrySpellStates<bool> Chivalry { get; protected set; } = new ChivalrySpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override BushidoSpellStates<bool> Bushido { get; protected set; } = new BushidoSpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override NinjitsuSpellStates<bool> Ninjitsu { get; protected set; } = new NinjitsuSpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override SpellweavingSpellStates<bool> Spellweaving { get; protected set; } = new SpellweavingSpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override MysticismSpellStates<bool> Mysticism { get; protected set; } = new MysticismSpellPermissions();

		// Custom

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override AvatarSpellStates<bool> Avatar { get; protected set; } = new AvatarSpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override ClericSpellStates<bool> Cleric { get; protected set; } = new ClericSpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override DruidSpellStates<bool> Druid { get; protected set; } = new DruidSpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override RangerSpellStates<bool> Ranger { get; protected set; } = new RangerSpellPermissions();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public override RogueSpellStates<bool> Rogue { get; protected set; } = new RogueSpellPermissions();

		#endregion

		public SpellPermissions()
		{
			SetAll(true);
		}

		public SpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, SpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, SpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class MagerySpellPermissions : MagerySpellStates<bool>
	{
		public MagerySpellPermissions()
		{
			SetAll(true);
		}

		public MagerySpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, MagerySpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, MagerySpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class NecromancySpellPermissions : NecromancySpellStates<bool>
	{
		public NecromancySpellPermissions()
		{
			SetAll(true);
		}

		public NecromancySpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, NecromancySpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, NecromancySpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class ChivalrySpellPermissions : ChivalrySpellStates<bool>
	{
		public ChivalrySpellPermissions()
		{
			SetAll(true);
		}

		public ChivalrySpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, ChivalrySpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, ChivalrySpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class BushidoSpellPermissions : BushidoSpellStates<bool>
	{
		public BushidoSpellPermissions()
		{
			SetAll(true);
		}

		public BushidoSpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, BushidoSpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, BushidoSpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class NinjitsuSpellPermissions : NinjitsuSpellStates<bool>
	{
		public NinjitsuSpellPermissions()
		{
			SetAll(true);
		}

		public NinjitsuSpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, NinjitsuSpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, NinjitsuSpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class SpellweavingSpellPermissions : SpellweavingSpellStates<bool>
	{
		public SpellweavingSpellPermissions()
		{
			SetAll(true);
		}

		public SpellweavingSpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, SpellweavingSpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, SpellweavingSpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class MysticismSpellPermissions : MysticismSpellStates<bool>
	{
		public MysticismSpellPermissions()
		{
			SetAll(true);
		}

		public MysticismSpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, MysticismSpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, MysticismSpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class AvatarSpellPermissions : AvatarSpellStates<bool>
	{
		public AvatarSpellPermissions()
		{
			SetAll(true);
		}

		public AvatarSpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, AvatarSpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, AvatarSpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class ClericSpellPermissions : ClericSpellStates<bool>
	{
		public ClericSpellPermissions()
		{
			SetAll(true);
		}

		public ClericSpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, ClericSpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, ClericSpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class DruidSpellPermissions : DruidSpellStates<bool>
	{
		public DruidSpellPermissions()
		{
			SetAll(true);
		}

		public DruidSpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, DruidSpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, DruidSpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class RangerSpellPermissions : RangerSpellStates<bool>
	{
		public RangerSpellPermissions()
		{
			SetAll(true);
		}

		public RangerSpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, RangerSpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, RangerSpellName key)
		{
			return reader.ReadBool();
		}
	}

	public class RogueSpellPermissions : RogueSpellStates<bool>
	{
		public RogueSpellPermissions()
		{
			SetAll(true);
		}

		public RogueSpellPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, RogueSpellName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, RogueSpellName key)
		{
			return reader.ReadBool();
		}
	}
}