using Server.Commands;
using Server.Engines.BulkOrders;
using Server.Engines.Publishing;
using Server.Engines.Stealables;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Server.Commands
{
	public class Categorization
	{
		public static CategoryNode Objects { get; private set; }
		public static CategoryNode Items { get; private set; }
		public static CategoryNode Mobiles { get; private set; }

		public static void Initialize()
		{
			#region Items
			Items = new("Items")
			{
				typeof(Item),

				#region Bods & Rewards
				new CategoryNode("Bods & Rewards")
				{
					typeof(SmallBOD),
					typeof(LargeBOD),
					typeof(BulkOrderBook),

					#region Smithing
					new CategoryNode("Smithing")
					{
						typeof(SmallSmithBOD),
						typeof(LargeSmithBOD),

						#region Rewards
						new CategoryNode("Rewards")
						{
							typeof(SturdyPickaxe),
							typeof(SturdyShovel),
							typeof(BaseGlovesOfMining),
							typeof(GargoylesPickaxe),
							typeof(ProspectorsTool),
							typeof(PowderOfTemperament),
							typeof(ColoredAnvil),
							typeof(RunicHammer),
							typeof(AncientSmithyHammer),
						},
						#endregion
					},
					#endregion
					#region Tailoring
					new CategoryNode("Tailoring")
					{
						typeof(SmallTailorBOD),
						typeof(LargeTailorBOD),

						#region Rewards
						new CategoryNode("Rewards")
						{
							typeof(ClothingBlessDeed),
							typeof(RunicSewingKit),

							#region Addons
							new CategoryNode("Addons")
							{
								#region Constructed
								new CategoryNode("Constructed")
								{
									typeof(SmallStretchedHideEastAddon),
									typeof(SmallStretchedHideSouthAddon),
									typeof(MediumStretchedHideEastAddon),
									typeof(MediumStretchedHideSouthAddon),
									typeof(LightFlowerTapestryEastAddon),
									typeof(LightFlowerTapestrySouthAddon),
									typeof(DarkFlowerTapestryEastAddon),
									typeof(DarkFlowerTapestrySouthAddon),
									typeof(BrownBearRugEastAddon),
									typeof(BrownBearRugSouthAddon),
									typeof(PolarBearRugEastAddon),
									typeof(PolarBearRugSouthAddon),
								},
								#endregion
								#region Deeds
								new CategoryNode("Deeds")
								{
									typeof(SmallStretchedHideEastDeed),
									typeof(SmallStretchedHideSouthDeed),
									typeof(MediumStretchedHideEastDeed),
									typeof(MediumStretchedHideSouthDeed),
									typeof(LightFlowerTapestryEastDeed),
									typeof(LightFlowerTapestrySouthDeed),
									typeof(DarkFlowerTapestryEastDeed),
									typeof(DarkFlowerTapestrySouthDeed),
									typeof(BrownBearRugEastDeed),
									typeof(BrownBearRugSouthDeed),
									typeof(PolarBearRugEastDeed),
									typeof(PolarBearRugSouthDeed),
								},
								#endregion
							},
							#endregion
						},
						#endregion
					},
					#endregion
				},
				#endregion
				#region Wearables
				new CategoryNode("Wearables")
				{
					#region Clothing
					new CategoryNode("Clothing")
					{
						typeof(BaseClothing),
						
						#region Cloaks
						new CategoryNode("Cloaks")
						{
							typeof(BaseCloak),
						},
						#endregion
						#region Hats
						new CategoryNode("Hats")
						{
							typeof(BaseHat),
						},
						#endregion
						#region Robes
						new CategoryNode("Robes")
						{
							typeof(BaseOuterTorso)
						},
						#endregion
						#region Shirts
						new CategoryNode("Shirts")
						{
							typeof(BaseShirt),
						},
						#endregion
						#region Pants
						new CategoryNode("Pants")
						{
							typeof(BasePants),
							typeof(BaseOuterLegs),
						},
						#endregion
						#region Shoes
						new CategoryNode("Shoes")
						{
							typeof(BaseShoes),
						},
						#endregion
						#region Misc
						new CategoryNode("Misc")
						{
							typeof(BaseWaist),
							typeof(BaseMiddleTorso),
						},
						#endregion
					},
					#endregion
					#region Weapons
					new CategoryNode("Weapons")
					{
						typeof(BaseWeapon),
						
						#region Wands
						new CategoryNode("Wands")
						{
							typeof(BaseWand),
						},
						#endregion
						#region Axes
						new CategoryNode("Axes")
						{
							typeof(BaseAxe),
						},
						#endregion
						#region Knives
						new CategoryNode("Knives")
						{
							typeof(BaseKnife),
						},
						#endregion
						#region Bashing
						new CategoryNode("Bashing")
						{
							typeof(BaseBashing),
						},
						#endregion
						#region Pole Arms
						new CategoryNode("Pole Arms")
						{
							typeof(BasePoleArm),
						},
						#endregion
						#region Ranged
						new CategoryNode("Ranged")
						{
							typeof(BaseRanged),
						},
						#endregion
						#region Spears
						new CategoryNode("Spears")
						{
							typeof(BaseSpear),
						},
						#endregion
						#region Staves
						new CategoryNode("Staves")
						{
							typeof(BaseStaff),
						},
						#endregion
						#region Swords
						new CategoryNode("Swords")
						{
							typeof(BaseSword),
						},
						#endregion
						#region Artifacts
						new CategoryNode("Artifacts")
						{
							typeof(AxeOfTheHeavens),
							typeof(BladeOfInsanity),
							typeof(BladeOfTheRighteous),
							typeof(BoneCrusher),
							typeof(BreathOfTheDead),
							typeof(Frostbringer),
							typeof(LegacyOfTheDreadLord),
							typeof(SerpentsFang),
							typeof(StaffOfTheMagi),
							typeof(TheBeserkersMaul),
							typeof(TheDragonSlayer),
							typeof(TheTaskmaster),
							typeof(TitansHammer),
							typeof(ZyronicClaw),
						},
						#endregion
					},
					#endregion
					#region Armor
					new CategoryNode("Armor")
					{
						typeof(BaseArmor),
						
						#region Bone
						new CategoryNode("Bone")
						{
							typeof(BoneArms),
							typeof(BoneChest),
							typeof(BoneGloves),
							typeof(BoneLegs),
							typeof(BoneHelm),
						},
						#endregion
						#region Chain
						new CategoryNode("Chain")
						{
							typeof(ChainChest),
							typeof(ChainLegs),
							typeof(ChainCoif),
						},
						#endregion
						#region Daemon Bone
						new CategoryNode("Daemon Bone")
						{
							typeof(DaemonArms),
							typeof(DaemonChest),
							typeof(DaemonGloves),
							typeof(DaemonLegs),
							typeof(DaemonHelm),
						},
						#endregion
						#region Dragon
						new CategoryNode("Dragon")
						{
							typeof(DragonArms),
							typeof(DragonChest),
							typeof(DragonGloves),
							typeof(DragonLegs),
							typeof(DragonHelm),
						},
						#endregion
						#region Helmets
						new CategoryNode("Helmets")
						{
							typeof(Bascinet),
							typeof(CloseHelm),
							typeof(Helmet),
							typeof(NorseHelm),
							typeof(OrcHelm),
						},
						#endregion
						#region Leather
						new CategoryNode("Leather")
						{
							typeof(FemaleLeatherChest),
							typeof(LeatherArms),
							typeof(LeatherBustierArms),
							typeof(LeatherChest),
							typeof(LeatherGloves),
							typeof(LeatherGorget),
							typeof(LeatherLegs),
							typeof(LeatherShorts),
							typeof(LeatherSkirt),
							typeof(LeatherCap),
						},
						#endregion
						#region Plate
						new CategoryNode("Plate")
						{
							typeof(FemalePlateChest),
							typeof(PlateArms),
							typeof(PlateChest),
							typeof(PlateGloves),
							typeof(PlateGorget),
							typeof(PlateLegs),
							typeof(PlateHelm),
						},
						#endregion
						#region Ranger
						new CategoryNode("Ranger")
						{
							typeof(RangerArms),
							typeof(RangerChest),
							typeof(RangerGloves),
							typeof(RangerGorget),
							typeof(RangerLegs),
						},
						#endregion
						#region Ringmail
						new CategoryNode("Ringmail")
						{
							typeof(RingmailArms),
							typeof(RingmailChest),
							typeof(RingmailGloves),
							typeof(RingmailLegs),
						},
						#endregion
						#region Studded Leather
						new CategoryNode("Studded Leather")
						{
							typeof(FemaleStuddedChest),
							typeof(StuddedArms),
							typeof(StuddedBustierArms),
							typeof(StuddedChest),
							typeof(StuddedGloves),
							typeof(StuddedGorget),
							typeof(StuddedLegs),
						},
						#endregion
						#region Shields
						new CategoryNode("Shields")
						{
							typeof(BaseShield),

							#region Artifacts
							new CategoryNode("Artifacts")
							{
								typeof(Aegis),
								typeof(ArcaneShield),
							},
							#endregion
						},
						#endregion
						#region Artifacts
						new CategoryNode("Artifacts")
						{
							typeof(ArmorOfFortune),
							typeof(GauntletsOfNobility),
							typeof(HelmOfInsight),
							typeof(HolyKnightsBreastplate),
							typeof(InquisitorsResolution),
							typeof(JackalsCollar),
							typeof(LeggingsOfBane),
							typeof(MidnightBracers),
							typeof(OrnateCrownOfTheHarrower),
							typeof(ShadowDancerLeggings),
							typeof(TunicOfFire),
							typeof(VoiceOfTheFallenKing),
						},
						#endregion
					},
					#endregion
					#region Jewelry
					new CategoryNode("Jewelry")
					{
						typeof(BaseJewel),

						new CategoryNode("Artifacts")
						{
							typeof(BraceletOfHealth),
							typeof(OrnamentOfTheMagician),
							typeof(RingOfTheElements),
							typeof(RingOfTheVile),
						},
					},
					#endregion
					#region Quivers
					new CategoryNode("Quivers")
					{
						typeof(BaseQuiver),
					},
					#endregion
					#region Lights
					new CategoryNode("Lights")
					{
						typeof(BaseEquipableLight),
					},
					#endregion
				},
				#endregion
				#region Addons
				new CategoryNode("Addons")
				{
					#region Constructed
					new CategoryNode("Constructed")
					{
						typeof(BaseAddon),

						#region Smithing
						new CategoryNode("Smithing")
						{
							typeof(LargeForgeEastAddon),
							typeof(LargeForgeSouthAddon),
							typeof(SmallForgeAddon),
							typeof(AnvilEastAddon),
							typeof(AnvilSouthAddon),
						},
						#endregion
						#region Tailoring
						new CategoryNode("Tailoring")
						{
							typeof(LoomEastAddon),
							typeof(LoomSouthAddon),
							typeof(SpinningwheelEastAddon),
							typeof(SpinningwheelSouthAddon),
						},
						#endregion
						#region Cooking
						new CategoryNode("Cooking")
						{
							typeof(FlourMillEastAddon),
							typeof(FlourMillSouthAddon),
							typeof(StoneOvenEastAddon),
							typeof(StoneOvenSouthAddon),
						},
						#endregion
						#region Bedding
						new CategoryNode("Bedding")
						{
							typeof(LargeBedEastAddon),
							typeof(LargeBedSouthAddon),
							typeof(SmallBedSouthAddon),
							typeof(SmallBedEastAddon),
						},
						#endregion
						#region Training
						new CategoryNode("Training")
						{
							typeof(TrainingDummyEastAddon),
							typeof(TrainingDummySouthAddon),
							typeof(PickpocketDipEastAddon),
							typeof(PickpocketDipSouthAddon),
							typeof(ArcheryButteAddon),
						},
						#endregion
						#region Spiritual
						new CategoryNode("Spiritual")
						{
							typeof(PentagramAddon),
							typeof(AbbatoirAddon),
							typeof(BloodPentagram),
						},
						#endregion
						#region Furniture
						new CategoryNode("Furniture")
						{
							typeof(LargeStoneTableEastAddon),
							typeof(LargeStoneTableSouthAddon),
							typeof(MediumStoneTableEastAddon),
							typeof(MediumStoneTableSouthAddon),
						},
						#endregion
					},
					#endregion
					#region Deeds
					new CategoryNode("Deeds")
					{
						typeof(BaseAddonDeed),

						#region Smithing
						new CategoryNode("Smithing")
						{
							typeof(LargeForgeEastDeed),
							typeof(LargeForgeSouthDeed),
							typeof(SmallForgeDeed),
							typeof(AnvilEastDeed),
							typeof(AnvilSouthDeed),
						},
						#endregion
						#region Tailoring
						new CategoryNode("Tailoring")
						{
							typeof(LoomEastDeed),
							typeof(LoomSouthDeed),
							typeof(SpinningwheelEastDeed),
							typeof(SpinningwheelSouthDeed),
						},
						#endregion
						#region Cooking
						new CategoryNode("Cooking")
						{
							typeof(FlourMillEastDeed),
							typeof(FlourMillSouthDeed),
							typeof(StoneOvenEastDeed),
							typeof(StoneOvenSouthDeed),
						},
						#endregion
						#region Bedding
						new CategoryNode("Bedding")
						{
							typeof(LargeBedEastDeed),
							typeof(LargeBedSouthDeed),
							typeof(SmallBedSouthDeed),
							typeof(SmallBedEastDeed),
						},
						#endregion
						#region Training
						new CategoryNode("Training")
						{
							typeof(TrainingDummyEastDeed),
							typeof(TrainingDummySouthDeed),
							typeof(PickpocketDipEastDeed),
							typeof(PickpocketDipSouthDeed),
							typeof(ArcheryButteDeed),
						},
						#endregion
						#region Spiritual
						new CategoryNode("Spiritual")
						{
							typeof(PentagramDeed),
							typeof(AbbatoirDeed),
						},
						#endregion
						#region Furniture
						new CategoryNode("Furniture")
						{
							typeof(LargeStoneTableEastDeed),
							typeof(LargeStoneTableSouthDeed),
							typeof(MediumStoneTableEastDeed),
							typeof(MediumStoneTableSouthDeed),
						},
						#endregion
					},
					#endregion
				},
				#endregion
				#region Books
				new CategoryNode("Books")
				{
					typeof(BaseBook),
				},
				#endregion
				#region Resources
				new CategoryNode("Resources")
				{
					#region Ingots
					new CategoryNode("Ingots")
					{
						typeof(BaseIngot),
					},
					#endregion
					#region Ore
					new CategoryNode("Ore")
					{
						typeof(BaseOre),
					},
					#endregion
					#region Granite
					new CategoryNode("Granite")
					{
						typeof(BaseGranite),
					},
					#endregion
					#region Scales
					new CategoryNode("Scales")
					{
						typeof(BaseScales),
					},
					#endregion
					#region Reagents
					new CategoryNode("Reagents")
					{
						typeof(BaseReagent),

						#region Standard
						new CategoryNode("Standard")
						{
							typeof(BlackPearl),
							typeof(Bloodmoss),
							typeof(Garlic),
							typeof(Ginseng),
							typeof(MandrakeRoot),
							typeof(Nightshade),
							typeof(SpidersSilk),
							typeof(SulfurousAsh),
						},
						#endregion
						#region Necromancy
						new CategoryNode("Necromancy")
						{
							typeof(BatWing),
							typeof(DaemonBlood),
							typeof(GraveDust),
							typeof(NoxCrystal),
							typeof(PigIron),
						},
						#endregion
					},
					#endregion
					#region Wood
					new CategoryNode("Wood")
					{
						typeof(Log),
						typeof(Board),
					},
					#endregion
					#region Fish
					new CategoryNode("Fish")
					{
						typeof(BaseAquaticLife),
						typeof(BigFish),
					},
					#endregion
					#region Tailoring
					new CategoryNode("Tailoring")
					{
						#region Hides
						new CategoryNode("Hides")
						{
							typeof(BaseHides),
						},
						#endregion
						#region Leather
						new CategoryNode("Leather")
						{
							typeof(BaseLeather),
						},
						#endregion
						#region Cloth
						new CategoryNode("Cloth")
						{
							typeof(BoltOfCloth),
							typeof(Cloth),
							typeof(UncutCloth),
						},
						#endregion
						#region Materials
						new CategoryNode("Materials")
						{
							typeof(BaseClothMaterial),
							typeof(Wool),
							typeof(Flax),
							typeof(Cotton),
							typeof(Bone),
						},
						#endregion
					},
					#endregion
					#region Fletching
					new CategoryNode("Fletching")
					{
						typeof(Arrow),
						typeof(Bolt),
						typeof(Feather),
						typeof(Shaft),
					},
					#endregion
					#region Bags
					new CategoryNode("Bags")
					{
						typeof(BagOfAllReagents),
						typeof(BagOfNecroReagents),
						typeof(BagOfReagents),
						typeof(AlchemyBag),
						typeof(BagOfingots),
						typeof(ScribeBag),
						typeof(SmithBag),
						typeof(TailorBag),
					},
					#endregion
					#region Stones
					new CategoryNode("Stones")
					{
						typeof(AlchemyStone),
						typeof(IngotStone),
						typeof(ReagentStone),
						typeof(ScribeStone),
						typeof(SmithStone),
						typeof(TailorStone),
					},
					#endregion
				},
				#endregion
				#region Entertainment
				new CategoryNode("Entertainment")
				{
					#region Instruments
					new CategoryNode("Instruments")
					{
						typeof(BaseInstrument),
					},
					#endregion
					#region Games
					new CategoryNode("Games")
					{
						typeof(BaseBoard),
						typeof(Dice),
					},
					#endregion
				},
				#endregion
				#region Construction
				new CategoryNode("Construction")
				{
					#region Decoration
					new CategoryNode("Decoration")
					{
						#region Shields
						new CategoryNode("Shields")
						{
							typeof(DecorativeShield1),
							typeof(DecorativeShield2),
							typeof(DecorativeShield3),
							typeof(DecorativeShield4),
							typeof(DecorativeShield5),
							typeof(DecorativeShield6),
							typeof(DecorativeShield7),
							typeof(DecorativeShield8),
							typeof(DecorativeShield9),
							typeof(DecorativeShield10),
							typeof(DecorativeShield11),
							typeof(DecorativeShieldSword1North),
							typeof(DecorativeShieldSword1West),
							typeof(DecorativeShieldSword2North),
							typeof(DecorativeShieldSword2West),
						},
						#endregion
						#region Weapons
						new CategoryNode("Weapons")
						{
							typeof(DecorativeBowWest),
							typeof(DecorativeBowNorth),
							typeof(DecorativeAxeNorth),
							typeof(DecorativeAxeWest),
							typeof(DecorativeSwordNorth),
							typeof(DecorativeSwordWest),
							typeof(DecorativeDAxeNorth),
							typeof(DecorativeDAxeWest),
						},
						#endregion
						#region Paintings
						new CategoryNode("Paintings")
						{
							typeof(LargePainting),
							typeof(WomanPortrait1),
							typeof(WomanPortrait2),
							typeof(ManPortrait1),
							typeof(ManPortrait2),
							typeof(LadyPortrait1),
							typeof(LadyPortrait2),
						},
						#endregion
						#region Tapestries
						new CategoryNode("Tapestries")
						{
							typeof(Tapestry1N),
							typeof(Tapestry2N),
							typeof(Tapestry2W),
							typeof(Tapestry3N),
							typeof(Tapestry3W),
							typeof(Tapestry4N),
							typeof(Tapestry4W),
							typeof(Tapestry5N),
							typeof(Tapestry5W),
							typeof(Tapestry6N),
							typeof(Tapestry6W),
						},
						#endregion
					},
					#endregion
					#region Broken
					new CategoryNode("Broken")
					{
						typeof(RuinedFallenChairA),
						typeof(RuinedArmoire),
						typeof(RuinedBookcase),
						typeof(RuinedBooks),
						typeof(CoveredChair),
						typeof(RuinedFallenChairB),
						typeof(RuinedChair),
						typeof(RuinedClock),
						typeof(RuinedDrawers),
						typeof(RuinedPainting),
						typeof(WoodDebris),
					},
					#endregion
					#region Rares
					new CategoryNode("Rares")
					{
						typeof(Rope),
						typeof(IronWire),
						typeof(SilverWire),
						typeof(GoldWire),
						typeof(CopperWire),
						typeof(Whip),
						typeof(PaintsAndBrush),
					},
					#endregion
					#region Doors
					new CategoryNode("Doors")
					{
						typeof(BaseDoor),
					},
					#endregion
					#region Floors
					new CategoryNode("Floors")
					{
						typeof(BaseFloor),
					},
					#endregion
					#region Containers
					new CategoryNode("Containers")
					{
						typeof(Container),
					},
					#endregion
					#region Furniture
					new CategoryNode("Furniture")
					{
						#region Chairs
						new CategoryNode("Chairs")
						{
							typeof(WoodenBench),
							typeof(FancyWoodenChairCushion),
							typeof(WoodenChairCushion),
							typeof(WoodenChair),
							typeof(BambooChair),
							typeof(Stool),
							typeof(FootStool),
							typeof(Throne),
							typeof(WoodenThrone),
						},
						#endregion
						#region Tables
						new CategoryNode("Tables")
						{
							typeof(LargeTable),
							typeof(Nightstand),
							typeof(YewWoodTable),
							typeof(WritingTable),
						},
						#endregion
						#region Misc
						new CategoryNode("Misc")
						{
							typeof(Easle),
							typeof(TallMusicStand),
							typeof(ShortMusicStand),
						},
						#endregion
					},
					#endregion
				},
				#endregion
				#region Lights
				new CategoryNode("Lights")
				{
					typeof(BaseLight),
				},
				#endregion
				#region Food & Drink
				new CategoryNode("Food & Drink")
				{
					#region Prepared
					new CategoryNode("Prepared")
					{
						typeof(Food),
					},
					#endregion
					#region Uncooked
					new CategoryNode("Uncooked")
					{
						typeof(CookableFood),
					},
					#endregion
					#region Beverages
					new CategoryNode("Beverages")
					{
						typeof(BaseBeverage),
						typeof(Glass),
						typeof(ShortMusicStand),
					},
					#endregion
				},
				#endregion
				#region Cartography
				new CategoryNode("Cartography")
				{
					typeof(MapItem),
				},
				#endregion
				#region Hair & Beards
				new CategoryNode("Hair & Beards")
				{
					#region Hair
					new CategoryNode("Hair")
					{
						typeof(Hair),
					},
					#endregion
					#region Beards
					new CategoryNode("Beards")
					{
						typeof(Beard),
					},
					#endregion
				},
				#endregion
				#region Stonecraft
				new CategoryNode("Stonecraft")
				{
					typeof(Vase),
					typeof(LargeVase),
					typeof(StatueSouth),
					typeof(StatueNorth),
					typeof(StatueEast),
					typeof(StatuePegasus),
					typeof(StoneChair),
				},
				#endregion
				#region Glassblowing
				new CategoryNode("Glassblowing")
				{
					typeof(SmallFlask),
					typeof(MediumFlask),
					typeof(LargeFlask),
					typeof(CurvedFlask),
					typeof(LongFlask),
					typeof(SpinningHourglass),
					typeof(GreenBottle),
					typeof(RedBottle),
					typeof(SmallBrownBottle),
					typeof(SmallVioletBottle),
					typeof(TinyYellowBottle),
					typeof(SmallBlueFlask),
					typeof(SmallYellowFlask),
					typeof(SmallRedFlask),
					typeof(SmallEmptyFlask),
					typeof(YellowBeaker),
					typeof(RedBeaker),
					typeof(BlueBeaker),
					typeof(GreenBeaker),
					typeof(EmptyCurvedFlaskW),
					typeof(RedCurvedFlask),
					typeof(LtBlueCurvedFlask),
					typeof(EmptyCurvedFlaskE),
					typeof(BlueCurvedFlask),
					typeof(GreenCurvedFlask),
					typeof(RedRibbedFlask),
					typeof(VioletRibbedFlask),
					typeof(EmptyRibbedFlask),
					typeof(LargeYellowFlask),
					typeof(LargeVioletFlask),
					typeof(LargeEmptyFlask),
					typeof(AniRedRibbedFlask),
					typeof(AniLargeVioletFlask),
					typeof(AniSmallBlueFlask),
					typeof(SmallBlueBottle),
					typeof(SmallGreenBottle),
					typeof(EmptyVialsWRack),
					typeof(FullVialsWRack),
					typeof(EmptyJar),
					typeof(DecoFullJar),
					typeof(HalfEmptyJar),
					typeof(EmptyJars3),
					typeof(EmptyJars4),
					typeof(DecoFullJars3),
					typeof(DecoFullJars4),
					typeof(EmptyJars2),
					typeof(EmptyVial),
					typeof(HourglassAni),
					typeof(Hourglass),
					typeof(TinyRedBottle),
					typeof(SmallGreenBottle2),
					typeof(Glass),
				},
				#endregion
				#region Pots & Plants
				new CategoryNode("Pots & Plants")
				{
					#region Pots
					new CategoryNode("Pots")
					{
						typeof(SmallEmptyPot),
						typeof(LargeEmptyPot),
					},
					#endregion
					#region Plants
					new CategoryNode("Plants")
					{
						typeof(PottedCactus),
						typeof(PottedCactus1),
						typeof(PottedCactus2),
						typeof(PottedCactus3),
						typeof(PottedCactus4),
						typeof(PottedCactus5),
						typeof(PottedPlant),
						typeof(PottedPlant1),
						typeof(PottedPlant2),
						typeof(PottedTree),
						typeof(PottedTree1),
					},
					#endregion
				},
				#endregion
				#region Components
				new CategoryNode("Components")
				{
					typeof(BarrelLid),
					typeof(BarrelStaves),
					typeof(BarrelHoops),
					typeof(BarrelTap),
					typeof(Gears),
					typeof(Springs),
					typeof(Hinge),
					typeof(Axle),
					typeof(AxleGears),
					typeof(SextantParts),
					typeof(ClockParts),
					typeof(ClockFrame),
					typeof(Clock),
					typeof(ClockRight),
					typeof(ClockLeft),
				},
				#endregion
				#region Tools
				new CategoryNode("Tools")
				{
					#region Crafting
					new CategoryNode("Crafting")
					{
						typeof(BaseTool),
					},
					#endregion
					#region Harvesting
					new CategoryNode("Harvesting")
					{
						typeof(BaseHarvestTool),
					},
					#endregion
				},
				#endregion
				#region Traps
				new CategoryNode("Traps")
				{
					typeof(BaseTrap),
				},
				#endregion
				#region Gems
				new CategoryNode("Gems")
				{
					typeof(Amber),
					typeof(Amethyst),
					typeof(Citrine),
					typeof(Diamond),
					typeof(Emerald),
					typeof(Ruby),
					typeof(Sapphire),
					typeof(StarSapphire),
					typeof(Tourmaline),
				},
				#endregion
				#region Boats
				new CategoryNode("Boats")
				{
					#region Constructed
					new CategoryNode("Constructed")
					{
						typeof(BaseBoat),
					},
					#endregion
					#region Deeds
					new CategoryNode("Deeds")
					{
						typeof(BaseBoatDeed),
					},
					#endregion
				},
				#endregion
				#region Housing
				new CategoryNode("Housing")
				{
					typeof(HousePlacementTool),

					#region Constructed
					new CategoryNode("Constructed")
					{
						typeof(BaseHouse),
					},
					#endregion
					#region Deeds
					new CategoryNode("Deeds")
					{
						typeof(HouseDeed),
					},
					#endregion
				},
				#endregion
				#region Camps
				new CategoryNode("Camps")
				{
					typeof(BaseCamp),
				},
				#endregion
				#region Magical
				new CategoryNode("Magical")
				{
					#region Potions
					new CategoryNode("Potions")
					{
						typeof(BasePotion),

						#region Agility
						new CategoryNode("Agility")
						{
							typeof(BaseAgilityPotion),
						},
						#endregion
						#region Cure
						new CategoryNode("Cure")
						{
							typeof(BaseCurePotion),
						},
						#endregion
						#region Explosion
						new CategoryNode("Explosion")
						{
							typeof(BaseExplosionPotion),
						},
						#endregion
						#region Heal
						new CategoryNode("Heal")
						{
							typeof(BaseHealPotion),
						},
						#endregion
						#region Poison
						new CategoryNode("Poison")
						{
							typeof(BasePoisonPotion),
						},
						#endregion
						#region Refresh
						new CategoryNode("Refresh")
						{
							typeof(BaseRefreshPotion),
						},
						#endregion
						#region Strength
						new CategoryNode("Strength")
						{
							typeof(BaseStrengthPotion),
						},
						#endregion
						#region Conflagration
						new CategoryNode("Conflagration")
						{
							typeof(BaseConflagrationPotion),
						},
						#endregion
						#region Confusion
						new CategoryNode("Confusion")
						{
							typeof(BaseConfusionBlastPotion),
						},
						#endregion
					},
					#endregion
					#region Scrolls
					new CategoryNode("Scrolls")
					{
						typeof(BlankScroll),
						typeof(SpellScroll),

						#region Magery
						new CategoryNode("Magery")
						{
							#region Circle 1
							new CategoryNode("Circle 1")
							{
								typeof(ClumsyScroll),
								typeof(CreateFoodScroll),
								typeof(FeeblemindScroll),
								typeof(HealScroll),
								typeof(MagicArrowScroll),
								typeof(NightSightScroll),
								typeof(ReactiveArmorScroll),
								typeof(WeakenScroll),
							},
							#endregion
							#region Circle 2
							new CategoryNode("Circle 2")
							{
								typeof(AgilityScroll),
								typeof(CunningScroll),
								typeof(CureScroll),
								typeof(HarmScroll),
								typeof(MagicTrapScroll),
								typeof(RemoveTrapScroll),
								typeof(ProtectionScroll),
								typeof(StrengthScroll),
							},
							#endregion
							#region Circle 3
							new CategoryNode("Circle 3")
							{
								typeof(BlessScroll),
								typeof(FireballScroll),
								typeof(MagicLockScroll),
								typeof(PoisonScroll),
								typeof(TelekinesisScroll),
								typeof(TeleportScroll),
								typeof(UnlockScroll),
								typeof(WallOfStoneScroll),
							},
							#endregion
							#region Circle 4
							new CategoryNode("Circle 4")
							{
								typeof(ArchCureScroll),
								typeof(ArchProtectionScroll),
								typeof(CurseScroll),
								typeof(FireFieldScroll),
								typeof(GreaterHealScroll),
								typeof(LightningScroll),
								typeof(ManaDrainScroll),
								typeof(RecallScroll),
							},
							#endregion
							#region Circle 5
							new CategoryNode("Circle 5")
							{
								typeof(BladeSpiritsScroll),
								typeof(DispelFieldScroll),
								typeof(IncognitoScroll),
								typeof(MagicReflectScroll),
								typeof(MindBlastScroll),
								typeof(ParalyzeScroll),
								typeof(PoisonFieldScroll),
								typeof(SummonCreatureScroll),
							},
							#endregion
							#region Circle 6
							new CategoryNode("Circle 6")
							{
								typeof(DispelScroll),
								typeof(EnergyBoltScroll),
								typeof(ExplosionScroll),
								typeof(InvisibilityScroll),
								typeof(MarkScroll),
								typeof(MassCurseScroll),
								typeof(ParalyzeFieldScroll),
								typeof(RevealScroll),
							},
							#endregion
							#region Circle 7
							new CategoryNode("Circle 7")
							{
								typeof(ChainLightningScroll),
								typeof(EnergyFieldScroll),
								typeof(FlameStrikeScroll),
								typeof(GateTravelScroll),
								typeof(ManaVampireScroll),
								typeof(MassDispelScroll),
								typeof(MeteorSwarmScroll),
								typeof(PolymorphScroll),
							},
							#endregion
							#region Circle 8
							new CategoryNode("Circle 8")
							{
								typeof(EarthquakeScroll),
								typeof(EnergyVortexScroll),
								typeof(ResurrectionScroll),
								typeof(SummonAirElementalScroll),
								typeof(SummonDaemonScroll),
								typeof(SummonEarthElementalScroll),
								typeof(SummonFireElementalScroll),
								typeof(SummonWaterElementalScroll),
							},
							#endregion
						},
						#endregion
						#region Necromancy
						new CategoryNode("Necromancy")
						{
							typeof(AnimateDeadScroll),
							typeof(BloodOathScroll),
							typeof(CorpseSkinScroll),
							typeof(CurseWeaponScroll),
							typeof(EvilOmenScroll),
							typeof(HorrificBeastScroll),
							typeof(LichFormScroll),
							typeof(MindRotScroll),
							typeof(PainSpikeScroll),
							typeof(PoisonStrikeScroll),
							typeof(StrangleScroll),
							typeof(SummonFamiliarScroll),
							typeof(VampiricEmbraceScroll),
							typeof(VengefulSpiritScroll),
							typeof(WitherScroll),
							typeof(WraithFormScroll),
						},
						#endregion
					},
					#endregion
					#region Ethereal Mounts
					new CategoryNode("Ethereal Mounts")
					{
						typeof(EtherealMount),
					},
					#endregion
				},
				#endregion
			};
			#endregion

			#region Mobiles
			Mobiles = new("Mobiles")
			{
				typeof(Mobile),

				#region Healers
				new CategoryNode("Healers")
				{
					typeof(BaseHealer),
				},
				#endregion
				#region Champions
				new CategoryNode("Champions")
				{
					typeof(BaseChampion),
					typeof(Silvani),
					typeof(Harrower),
					typeof(HarrowerTentacles),
				},
				#endregion
				#region Guards
				new CategoryNode("Guards")
				{
					typeof(BaseGuard),
					typeof(BaseShieldGuard),
				},
				#endregion
				#region Vendors
				new CategoryNode("Vendors")
				{
					typeof(BaseVendor),

					#region Guildmasters
					new CategoryNode("Guildmasters")
					{
						typeof(BaseGuildmaster),
					},
					#endregion
					#region Factions
					new CategoryNode("Factions")
					{
						typeof(BaseFactionVendor),
					},
					#endregion
				},
				#endregion
				#region Dummies
				new CategoryNode("Dummies")
				{
					typeof(DummyMace),
					typeof(DummyFence),
					typeof(DummySword),
					typeof(DummyNox),
					typeof(DummyStun),
					typeof(DummySuper),
					typeof(DummyHealer),
					typeof(DummyAssassin),
					typeof(DummyThief),
				},
				#endregion
				#region Creatures
				new CategoryNode("Creatures")
				{
					typeof(BaseCreature),

					#region Animals
					new CategoryNode("Animals")
					{
						#region Mounts
						new CategoryNode("Mounts")
						{
							typeof(BaseMount),
							
							#region War Horses
							new CategoryNode("War Horses")
							{
								typeof(BaseWarHorse),
								typeof(FactionWarHorse),
							},
							#endregion
						},
						#endregion
						#region Birds & Fowl
						new CategoryNode("Birds & Fowl")
						{
							typeof(Bird),
							typeof(Eagle),
							typeof(Chicken),
						},
						#endregion
						#region Bovines
						new CategoryNode("Bovines")
						{
							typeof(Bull),
							typeof(Cow),
						},
						#endregion
						#region Bruins
						new CategoryNode("Bruins")
						{
							typeof(GrizzlyBear),
							typeof(PolarBear),
							typeof(BlackBear),
							typeof(BrownBear),
						},
						#endregion
						#region Canines
						new CategoryNode("Canines")
						{
							typeof(DireWolf),
							typeof(GreyWolf),
							typeof(TimberWolf),
							typeof(WhiteWolf),
							typeof(HellHound),
							typeof(Dog),
						},
						#endregion
						#region Deer
						new CategoryNode("Deer")
						{
							typeof(Hind),
							typeof(GreatHart),
						},
						#endregion
						#region Felines
						new CategoryNode("Felines")
						{
							typeof(Panther),
							typeof(HellCat),
							typeof(PredatorHellCat),
							typeof(SnowLeopard),
							typeof(Cougar),
							typeof(Cat),
						},
						#endregion
						#region Frogs & Toads
						new CategoryNode("Frogs & Toads")
						{
							typeof(BullFrog),
							typeof(GiantToad),
						},
						#endregion
						#region Lizards
						new CategoryNode("Lizards")
						{
							typeof(Alligator),
						},
						#endregion
						#region Pack Animals
						new CategoryNode("Pack Animals")
						{
							typeof(PackLlama),
							typeof(PackHorse),
						},
						#endregion
						#region Rodents
						new CategoryNode("Rodents")
						{
							typeof(Rabbit),
							typeof(JackRabbit),
							typeof(Rat),
							typeof(Sewerrat),
							typeof(GiantRat),
						},
						#endregion
						#region Porcines
						new CategoryNode("Porcines")
						{
							typeof(Boar),
							typeof(Pig),
						},
						#endregion
						#region Ruminants
						new CategoryNode("Ruminants")
						{
							typeof(Goat),
							typeof(MountainGoat),
							typeof(Sheep),
						},
						#endregion
						#region Misc
						new CategoryNode("Misc")
						{
							typeof(Llama),
							typeof(Gorilla),
							typeof(Walrus),
						},
						#endregion
					},
					#endregion
					#region Ants
					new CategoryNode("Ants")
					{
						#region Red
						new CategoryNode("Red")
						{
							typeof(RedSolenWorker),
							typeof(RedSolenWarrior),
							typeof(RedSolenQueen),
							typeof(RedSolenInfiltratorQueen),
							typeof(RedSolenInfiltratorWarrior),
						},
						#endregion
						#region Black
						new CategoryNode("Black")
						{
							typeof(BlackSolenWorker),
							typeof(BlackSolenWarrior),
							typeof(BlackSolenQueen),
							typeof(BlackSolenInfiltratorQueen),
							typeof(BlackSolenInfiltratorWarrior),
						},
						#endregion
					},
					#endregion
					#region Elementals
					new CategoryNode("Elementals")
					{
						typeof(AirElemental),
						typeof(BloodElemental),
						typeof(FireElemental),
						typeof(IceElemental),
						typeof(WaterElemental),
						typeof(PoisonElemental),
						typeof(EarthElemental),
						typeof(SnowElemental),

						#region Ore Elementals
						new CategoryNode("Ore Elementals")
						{
							typeof(DullCopperElemental),
							typeof(ShadowIronElemental),
							typeof(CopperElemental),
							typeof(BronzeElemental),
							typeof(GoldenElemental),
							typeof(AgapiteElemental),
							typeof(VeriteElemental),
							typeof(ValoriteElemental),
						},
						#endregion
					},
					#endregion
					#region Marine
					new CategoryNode("Marine")
					{
						typeof(Dolphin),
						typeof(SeaSerpent),
						typeof(DeepSeaSerpent),
					},
					#endregion
					#region Good
					new CategoryNode("Good")
					{
						typeof(Pixie),
						typeof(Wisp),
						typeof(EtherealWarrior),
						typeof(Centaur),
					},
					#endregion
					#region Evil
					new CategoryNode("Evil")
					{
						#region Age of Shadows
						new CategoryNode("Age of Shadows")
						{
							typeof(FleshGolem),
							typeof(Gibberling),
							typeof(GoreFiend),
							typeof(Ravager),
							typeof(SkitteringHopper),
							typeof(Treefellow),
							typeof(VampireBat),
							typeof(WailingBanshee),
							typeof(AbysmalHorror),
							typeof(BoneDemon),
							typeof(CrystalElemental),
							typeof(DarknightCreeper),
							typeof(DemonKnight),
							typeof(Devourer),
							typeof(FleshRenderer),
							typeof(Impaler),
							typeof(MoundOfMaggots),
							typeof(PatchworkSkeleton),
							typeof(ShadowKnight),
							typeof(WandererOfTheVoid),
							typeof(ChaosDaemon),
							typeof(HordeMinion),
						},
						#endregion
						#region Daemons
						new CategoryNode("Daemons")
						{
							typeof(IceFiend),
							typeof(Balron),
							typeof(Daemon),
						},
						#endregion
						#region Dragons & Wyrms
						new CategoryNode("Dragons & Wyrms")
						{
							typeof(Dragon),
							typeof(WhiteWyrm),
							typeof(SerpentineDragon),
							typeof(AncientWyrm),
							typeof(ShadowWyrm),
							typeof(SkeletalDragon),
							typeof(Drake),
							typeof(Wyvern),
						},
						#endregion
						#region Gargoyles
						new CategoryNode("Gargoyles")
						{
							typeof(Gargoyle),
							typeof(FireGargoyle),
							typeof(StoneGargoyle),
						},
						#endregion
						#region Harpies
						new CategoryNode("Harpies")
						{
							typeof(Harpy),
							typeof(StoneHarpy),
						},
						#endregion
						#region Humans
						new CategoryNode("Humans")
						{
							typeof(EvilMage),
							typeof(EvilMageLord),
							typeof(Betrayer),
							typeof(Brigand),
							typeof(Executioner),
							typeof(Guardian),
							typeof(KhaldunSummoner),
							typeof(KhaldunZealot),
						},
						#endregion
						#region Lizards
						new CategoryNode("Lizards")
						{
							typeof(LavaLizard),
							typeof(Lizardman),
						},
						#endregion
						#region Common
						new CategoryNode("Common")
						{
							typeof(Mongbat),
							typeof(StrongMongbat),
							typeof(Imp),
							typeof(HeadlessOne),
							typeof(Titan),
							typeof(Cyclops),
							typeof(Ettin),
							typeof(FrostTroll),
							typeof(Ogre),
							typeof(Troll),
							typeof(ArcticOgreLord),
							typeof(OgreLord),
						},
						#endregion
						#region Ophidians
						new CategoryNode("Ophidians")
						{
							typeof(OphidianMage),
							typeof(OphidianArchmage),
							typeof(OphidianMatriarch),
							typeof(OphidianWarrior),
							typeof(OphidianKnight),
						},
						#endregion
						#region Orcs
						new CategoryNode("Orcs")
						{
							typeof(OrcishMage),
							typeof(Orc),
							typeof(OrcishLord),
							typeof(OrcBomber),
							typeof(OrcCaptain),
							typeof(OrcishLord),
							typeof(OrcBrute),
						},
						#endregion
						#region Plague & Slime
						new CategoryNode("Plague & Slime")
						{
							typeof(Jwilson),
							typeof(FrostOoze),
							typeof(Slime),
							typeof(PlagueSpawn),
							typeof(PlagueBeast),
							typeof(BogThing),
							typeof(Bogling),
						},
						#endregion
						#region Plants
						new CategoryNode("Plants")
						{
							typeof(Corpser),
							typeof(Quagmire),
							typeof(WhippingVine),
							typeof(SwampTentacle),
							typeof(Reaper),
						},
						#endregion
						#region Ratmen
						new CategoryNode("Ratmen")
						{
							typeof(Ratman),
							typeof(RatmanArcher),
							typeof(RatmanMage),
						},
						#endregion
						#region Serpents
						new CategoryNode("Serpents")
						{
							typeof(IceSerpent),
							typeof(IceSnake),
							typeof(LavaSerpent),
							typeof(LavaSnake),
							typeof(GiantSerpent),
							typeof(SilverSerpent),
							typeof(Snake),
						},
						#endregion
						#region Spiders
						new CategoryNode("Spiders")
						{
							typeof(DreadSpider),
							typeof(FrostSpider),
							typeof(GiantSpider),
						},
						#endregion
						#region Terathans
						new CategoryNode("Terathans")
						{
							typeof(TerathanMatriarch),
							typeof(TerathanAvenger),
							typeof(TerathanDrone),
							typeof(TerathanWarrior),
						},
						#endregion
						#region Undead
						new CategoryNode("Undead")
						{
							#region Ghosts
							new CategoryNode("Ghosts")
							{
								typeof(Bogle),
								typeof(Shade),
								typeof(Spectre),
								typeof(Wraith),
								typeof(Ghoul),
							},
							#endregion
							#region Skeletons
							new CategoryNode("Skeletons")
							{
								typeof(BoneMagi),
								typeof(SkeletalMage),
								typeof(BoneKnight),
								typeof(SkeletalKnight),
								typeof(Skeleton),
							},
							#endregion
							#region Undead
							new CategoryNode("Undead")
							{
								typeof(RottingCorpse),
								typeof(Zombie),
								typeof(Mummy),
							},
							#endregion
							#region Liches
							new CategoryNode("Liches")
							{
								typeof(AncientLich),
								typeof(Lich),
								typeof(LichLord),
							},
							#endregion
						},
						#endregion
						#region Magical
						new CategoryNode("Magical")
						{
							typeof(EnergyVortex),
							typeof(SandVortex),
							typeof(BladeSpirits),
						},
						#endregion
					},
					#endregion
				},
				#endregion
			};
			#endregion

			#region Objects
			Objects = new("Objects")
			{
				Items,
				Mobiles,
			};
			#endregion

			var types = new HashSet<Type>();

			AddTypes(Core.Assembly, types);

			foreach (var asm in ScriptCompiler.Assemblies)
			{
				AddTypes(asm, types);
			}

			foreach (var type in types)
			{
				var match = GetDeepestMatch(Objects, type);

				if (match == null)
				{
					continue;
				}

				IEntity obj;

				try
				{
					obj = (IEntity)Activator.CreateInstance(type);
				}
				catch
				{
					continue;
				}

				if (obj == null)
				{
					continue;
				}

				match.Add(obj);

				try
				{
					obj.Delete();
				}
				catch
				{ }
			}
		}

		private static CategoryNode GetDeepestMatch(CategoryNode node, Type type)
		{
			foreach (var n in node.Nodes)
			{
				var s = GetDeepestMatch(n, type);

				if (s != null)
				{
					return s;
				}
			}

			if (node.IsMatch(type))
			{
				return node;
			}

			return null;
		}

		private static void AddTypes(Assembly asm, HashSet<Type> types)
		{
			foreach (var type in asm.GetTypes())
			{
				if (IsConstructable(type))
				{
					types.Add(type);
				}
			}
		}

		private static bool IsConstructable(Type type)
		{
			if (type.IsAbstract)
			{
				return false;
			}

			if (!type.IsSubclassOf(typeof(Item)) && !type.IsSubclassOf(typeof(Mobile)))
			{
				return false;
			}

			var ctor = type.GetConstructor(Type.EmptyTypes);

			return ctor != null && ctor.IsDefined(typeof(ConstructableAttribute), false);
		}
	}

	public interface ICategoryEntry : IComparable<ICategoryEntry>
	{
		string Name { get; }

		CategoryNode Parent { get; }

		void Click(Mobile from, CategoryNode category, int page);
	}

	public sealed class CategoryNode : ICategoryEntry, IEnumerable<ICategoryEntry>
	{
		public string Name { get; } = "(empty)";

		public HashSet<Type> Types { get; } = new();

		public SortedSet<CategoryNode> Nodes { get; } = new(CategoryComparer.Instance);
		public SortedSet<CategoryObject> Objects { get; } = new(CategoryComparer.Instance);

		public int NodeCount => Nodes.Count;
		public int ObjectCount => Objects.Count;

		public int Count => NodeCount + ObjectCount;

		public CategoryNode Parent { get; private set; }

		public CategoryNode(string name)
		{
			Name = name;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Name);
		}

		public bool IsMatch(Type type)
		{
			foreach (var match in Types)
			{
				if (type == match || type.IsSubclassOf(match))
				{
					return true;
				}
			}

			return false;
		}

		public void Add(Type type)
		{
			Types.Add(type);
		}

		public void Add(CategoryNode child)
		{
			if (Nodes.Add(child))
			{
				child.Parent = this;
			}
		}

		public void Add(IEntity obj)
		{
			Objects.Add(new CategoryObject(this, obj));
		}

		public int CompareTo(ICategoryEntry other)
		{
			return Insensitive.Compare(Name, other?.Name);
		}

		public void Click(Mobile from, CategoryNode category, int page)
		{
			from.SendGump(new CategorizedAddGump(from, this, 0));
		}

		public IEnumerator<ICategoryEntry> GetEnumerator()
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

	public sealed class CategoryObject : ICategoryEntry
	{
		public CategoryNode Parent { get; }

		public Type Type { get; }

		public int ItemID { get; }
		public int Hue { get; }

		public string Name { get; } = "(invalid)";

		public CategoryObject(CategoryNode parent, IEntity e)
		{
			Parent = parent;

			if (e is Item item)
			{
				Type = item.GetType();
				Name = Type.Name;

				if (item is not BaseAddon && item is not BaseMulti)
				{
					ItemID = item.ItemID;
					Hue = item.Hue;
				}
			}
			else if (e is Mobile mobile)
			{
				Type = mobile.GetType();
				Name = Type.Name;

				ItemID = ShrinkTable.Lookup(mobile, 0);
				Hue = mobile.Hue;
			}
			else if (e != null)
			{
				Type = e.GetType();
				Name = Type.Name;
			}
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type, Name);
		}

		public int CompareTo(ICategoryEntry other)
		{
			return Insensitive.Compare(Name, other?.Name);
		}

		public void Click(Mobile from, CategoryNode category, int page)
		{
			if (Type == null)
			{
				from.SendMessage("That is an invalid type name.");
			}
			else
			{
				CommandSystem.Handle(from, $"{CommandSystem.Prefix}Add {Type.Name}");
			}

			from.SendGump(new CategorizedAddGump(from, category, page));
		}
	}

	public sealed class CategoryComparer : IComparer<ICategoryEntry>
	{
		public static CategoryComparer Instance { get; } = new();

		private CategoryComparer()
		{ }

		public int Compare(ICategoryEntry x, ICategoryEntry y)
		{
			return Insensitive.Compare(x?.Name, y?.Name);
		}
	}
}

namespace Server.Commands.Generic
{
	public class AddCommand : BaseCommand
	{
		public AddCommand()
		{
			AccessLevel = AccessLevel.GameMaster;
			Supports = CommandSupport.Simple | CommandSupport.Self;
			Commands = new string[] { "Add" };
			ObjectTypes = ObjectTypes.All;
			Usage = "Add [<name> [params] [set {<propertyName> <value> ...}]]";
			Description = "Adds an item or npc by name to a targeted location. Optional constructor parameters. Optional set property list. If no arguments are specified, this brings up a categorized add menu.";
		}

		public override bool ValidateArgs(BaseCommandImplementor impl, CommandEventArgs e)
		{
			if (e.Length >= 1)
			{
				var t = ScriptCompiler.FindTypeByName(e.GetString(0));

				if (t == null)
				{
					e.Mobile.SendMessage("No type with that name was found.");

					var match = e.GetString(0).Trim();

					if (match.Length < 3)
					{
						e.Mobile.SendMessage("Invalid search string.");
						e.Mobile.SendGump(new AddGump(e.Mobile, match, 0, Type.EmptyTypes, false));
					}
					else
					{
						e.Mobile.SendGump(new AddGump(e.Mobile, match, 0, AddGump.Match(match).ToArray(), true));
					}
				}
				else
				{
					return true;
				}
			}
			else
			{
				e.Mobile.SendGump(new CategorizedAddGump(e.Mobile));
			}

			return false;
		}

		public override void Execute(CommandEventArgs e, object obj)
		{
			var p = obj as IPoint3D;

			if (p == null)
			{
				return;
			}

			if (p is Item)
			{
				p = ((Item)p).GetWorldTop();
			}
			else if (p is Mobile)
			{
				p = ((Mobile)p).Location;
			}

			Add.Invoke(e.Mobile, new Point3D(p), new Point3D(p), e.Arguments);
		}
	}
}

namespace Server.Gumps
{
	public class AddGump : Gump
	{
		private readonly string m_SearchString;
		private readonly Type[] m_SearchResults;
		private readonly int m_Page;

		public static void Initialize()
		{
			CommandSystem.Register("AddMenu", AccessLevel.GameMaster, new CommandEventHandler(AddMenu_OnCommand));
		}

		[Usage("AddMenu [searchString]")]
		[Description("Opens an add menu, with an optional initial search string. This menu allows you to search for Items or Mobiles and add them interactively.")]
		private static void AddMenu_OnCommand(CommandEventArgs e)
		{
			var val = e.ArgString.Trim();
			Type[] types;
			var explicitSearch = false;

			if (val.Length == 0)
			{
				types = Type.EmptyTypes;
			}
			else if (val.Length < 3)
			{
				e.Mobile.SendMessage("Invalid search string.");
				types = Type.EmptyTypes;
			}
			else
			{
				types = Match(val).ToArray();
				explicitSearch = true;
			}

			e.Mobile.SendGump(new AddGump(e.Mobile, val, 0, types, explicitSearch));
		}

		public AddGump(Mobile from, string searchString, int page, Type[] searchResults, bool explicitSearch) : base(50, 50)
		{
			m_SearchString = searchString;
			m_SearchResults = searchResults;
			m_Page = page;

			from.CloseGump(typeof(AddGump));

			AddPage(0);

			AddBackground(0, 0, 420, 280, 5054);

			AddImageTiled(10, 10, 400, 20, 2624);
			AddAlphaRegion(10, 10, 400, 20);
			AddImageTiled(41, 11, 184, 18, 0xBBC);
			AddImageTiled(42, 12, 182, 16, 2624);
			AddAlphaRegion(42, 12, 182, 16);

			AddButton(10, 9, 4011, 4013, 1, GumpButtonType.Reply, 0);
			AddTextEntry(44, 10, 180, 20, 0x480, 0, searchString);

			AddHtmlLocalized(230, 10, 100, 20, 3010005, 0x7FFF, false, false);

			AddImageTiled(10, 40, 400, 200, 2624);
			AddAlphaRegion(10, 40, 400, 200);

			if (searchResults.Length > 0)
			{
				for (var i = (page * 10); i < ((page + 1) * 10) && i < searchResults.Length; ++i)
				{
					var index = i % 10;

					AddLabel(44, 39 + (index * 20), 0x480, searchResults[i].Name);
					AddButton(10, 39 + (index * 20), 4023, 4025, 4 + i, GumpButtonType.Reply, 0);
				}
			}
			else
			{
				AddLabel(15, 44, 0x480, explicitSearch ? "Nothing matched your search terms." : "No results to display.");
			}

			AddImageTiled(10, 250, 400, 20, 2624);
			AddAlphaRegion(10, 250, 400, 20);

			if (m_Page > 0)
			{
				AddButton(10, 249, 4014, 4016, 2, GumpButtonType.Reply, 0);
			}
			else
			{
				AddImage(10, 249, 4014);
			}

			AddHtmlLocalized(44, 250, 170, 20, 1061028, (short)(m_Page > 0 ? 0x7FFF : 0x5EF7), false, false); // Previous page

			if (((m_Page + 1) * 10) < searchResults.Length)
			{
				AddButton(210, 249, 4005, 4007, 3, GumpButtonType.Reply, 0);
			}
			else
			{
				AddImage(210, 249, 4005);
			}

			AddHtmlLocalized(244, 250, 170, 20, 1061027, (short)((m_Page + 1) * 10 < searchResults.Length ? 0x7FFF : 0x5EF7), false, false); // Next page
		}

		private static readonly Type typeofItem = typeof(Item), typeofMobile = typeof(Mobile);

		private static void Match(string match, Type[] types, List<Type> results)
		{
			if (match.Length == 0)
			{
				return;
			}

			match = match.ToLower();

			for (var i = 0; i < types.Length; ++i)
			{
				var t = types[i];

				if ((typeofMobile.IsAssignableFrom(t) || typeofItem.IsAssignableFrom(t)) && t.Name.ToLower().IndexOf(match) >= 0 && !results.Contains(t))
				{
					var ctors = t.GetConstructors();

					for (var j = 0; j < ctors.Length; ++j)
					{
						if (ctors[j].GetParameters().Length == 0 && ctors[j].IsDefined(typeof(ConstructableAttribute), false))
						{
							results.Add(t);
							break;
						}
					}
				}
			}
		}

		public static List<Type> Match(string match)
		{
			var results = new List<Type>();
			Type[] types;

			var asms = ScriptCompiler.Assemblies;

			for (var i = 0; i < asms.Length; ++i)
			{
				types = ScriptCompiler.GetTypeCache(asms[i]).Types;
				Match(match, types, results);
			}

			types = ScriptCompiler.GetTypeCache(Core.Assembly).Types;
			Match(match, types, results);

			results.Sort(new TypeNameComparer());

			return results;
		}

		private class TypeNameComparer : IComparer<Type>
		{
			public int Compare(Type x, Type y)
			{
				return x.Name.CompareTo(y.Name);
			}
		}

		public class InternalTarget : Target
		{
			private readonly Type m_Type;
			private readonly Type[] m_SearchResults;
			private readonly string m_SearchString;
			private readonly int m_Page;

			public InternalTarget(Type type, Type[] searchResults, string searchString, int page) : base(-1, true, TargetFlags.None)
			{
				m_Type = type;
				m_SearchResults = searchResults;
				m_SearchString = searchString;
				m_Page = page;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				var p = o as IPoint3D;

				if (p != null)
				{
					if (p is Item)
					{
						p = ((Item)p).GetWorldTop();
					}
					else if (p is Mobile)
					{
						p = ((Mobile)p).Location;
					}

					Server.Commands.Add.Invoke(from, new Point3D(p), new Point3D(p), new string[] { m_Type.Name });

					from.Target = new InternalTarget(m_Type, m_SearchResults, m_SearchString, m_Page);
				}
			}

			protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
			{
				if (cancelType == TargetCancelType.Canceled)
				{
					from.SendGump(new AddGump(from, m_SearchString, m_Page, m_SearchResults, true));
				}
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var from = sender.Mobile;

			switch (info.ButtonID)
			{
				case 1: // Search
					{
						var te = info.GetTextEntry(0);
						var match = (te == null ? "" : te.Text.Trim());

						if (match.Length < 3)
						{
							from.SendMessage("Invalid search string.");
							from.SendGump(new AddGump(from, match, m_Page, m_SearchResults, false));
						}
						else
						{
							from.SendGump(new AddGump(from, match, 0, Match(match).ToArray(), true));
						}

						break;
					}
				case 2: // Previous page
					{
						if (m_Page > 0)
						{
							from.SendGump(new AddGump(from, m_SearchString, m_Page - 1, m_SearchResults, true));
						}

						break;
					}
				case 3: // Next page
					{
						if ((m_Page + 1) * 10 < m_SearchResults.Length)
						{
							from.SendGump(new AddGump(from, m_SearchString, m_Page + 1, m_SearchResults, true));
						}

						break;
					}
				default:
					{
						var index = info.ButtonID - 4;

						if (index >= 0 && index < m_SearchResults.Length)
						{
							from.SendMessage("Where do you wish to place this object? <ESC> to cancel.");
							from.Target = new InternalTarget(m_SearchResults[index], m_SearchResults, m_SearchString, m_Page);
						}

						break;
					}
			}
		}
	}

	public class CategorizedAddGump : Gump
	{
		public static bool OldStyle = PropsConfig.OldStyle;

		public static readonly int EntryHeight = 24;//PropsConfig.EntryHeight;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;
		public static readonly int BorderSize = PropsConfig.BorderSize;

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
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY + (((EntryHeight - 20) / 2) / 2);
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY + (((EntryHeight - 20) / 2) / 2);
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY + (((EntryHeight - 20) / 2) / 2);
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		private static readonly bool PrevLabel = false, NextLabel = false;

		private static readonly int PrevLabelOffsetX = PrevWidth + 1;
		private static readonly int PrevLabelOffsetY = 0;

		private static readonly int NextLabelOffsetX = -29;
		private static readonly int NextLabelOffsetY = 0;

		private static readonly int EntryWidth = 250;
		private static readonly int EntryCount = 30;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (EntryCount + 1));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		private readonly Dictionary<int, ICategoryEntry> m_ButtonHandlers = new();

		private readonly Mobile m_Owner;
		private readonly CategoryNode m_Category;
		private int m_Page;

		public CategorizedAddGump(Mobile owner)
			: this(owner, Categorization.Objects, 0)
		{ }

		public CategorizedAddGump(Mobile owner, CategoryNode category, int page)
			: base(GumpOffsetX, GumpOffsetY)
		{
			owner.CloseGump(typeof(CategorizedAddGump));

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

			AddHtml(x + TextOffsetX, y + ((EntryHeight - 20) / 2), emptyWidth - TextOffsetX, EntryHeight, String.Format("<center>{0}</center>", m_Category.Name), false, false);

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
				AddLabelCropped(x + TextOffsetX, y + ((EntryHeight - 20) / 2), EntryWidth - TextOffsetX, EntryHeight, TextHue, entry.Name);

				x += EntryWidth + OffsetSize;

				if (SetGumpID != 0)
				{
					AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
				}

				AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, index + 4, GumpButtonType.Reply, 0, entry);

				if (entry is CategoryObject obj)
				{
					var bounds = ItemBounds.Table[obj.ItemID];

					if (obj.ItemID > 1 && bounds.Height < (EntryHeight * 2))
					{
						if (bounds.Height < EntryHeight)
						{
							if (obj.Hue > 0)
							{
								AddItem(x - OffsetSize - 22 - (index % 2 * 44) - (bounds.Width / 2) - bounds.X, y + (EntryHeight / 2) - (bounds.Height / 2) - bounds.Y, obj.ItemID, obj.Hue);
							}
							else
							{
								AddItem(x - OffsetSize - 22 - (index % 2 * 44) - (bounds.Width / 2) - bounds.X, y + (EntryHeight / 2) - (bounds.Height / 2) - bounds.Y, obj.ItemID);
							}
						}
						else
						{
							if (obj.Hue > 0)
							{
								AddItem(x - OffsetSize - 22 - (index % 2 * 44) - (bounds.Width / 2) - bounds.X, y + EntryHeight - 1 - bounds.Height - bounds.Y, obj.ItemID, obj.Hue);
							}
							else
							{
								AddItem(x - OffsetSize - 22 - (index % 2 * 44) - (bounds.Width / 2) - bounds.X, y + EntryHeight - 1 - bounds.Height - bounds.Y, obj.ItemID);
							}
						}
					}
				}
			}
		}

		public void AddButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, ICategoryEntry entry)
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

							m_Owner.SendGump(new CategorizedAddGump(m_Owner, m_Category.Parent, index));
						}

						break;
					}
				case 2: // Previous
					{
						if (m_Page > 0)
						{
							m_Owner.SendGump(new CategorizedAddGump(m_Owner, m_Category, m_Page - 1));
						}

						break;
					}
				case 3: // Next
					{
						if ((m_Page + 1) * EntryCount < m_Category.Count)
						{
							m_Owner.SendGump(new CategorizedAddGump(m_Owner, m_Category, m_Page + 1));
						}

						break;
					}
				default:
					{
						if (m_ButtonHandlers.TryGetValue(info.ButtonID, out var entry))
						{
							entry.Click(m_Owner, m_Category, m_Page);
						}
						else
						{
							m_Owner.SendGump(new CategorizedAddGump(m_Owner, m_Category, m_Page));
						}

						break;
					}
			}

			m_ButtonHandlers.Clear();
		}
	}
}