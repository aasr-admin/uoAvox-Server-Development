﻿using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

using System;
using System.Collections;

#region Developer Notations

/// Pigments of tokuno do NOT check for if item is already hued 0;  APPARENTLY he still accepts it 
/// if it's < 10 charges. Chest of Heirlooms don't show if unlocked. Chest of heirlooms, locked, 
/// HARD to pick at 100 lock picking but not impossible.  had 95 health to 0, cause it's trapped.

#endregion

namespace Server.Engines.Events
{
	public enum TreasuresOfTokunoEra
	{
		None,
		ToTOne,
		ToTTwo,
		ToTThree
	}

	public class TreasuresOfTokuno
	{
		public const int ItemsPerReward = 10;

		private static readonly Type[] m_LesserArtifactsTotal = new Type[]
			{
				typeof( AncientFarmersKasa ), typeof( AncientSamuraiDo ), typeof( ArmsOfTacticalExcellence ), typeof( BlackLotusHood ),
 				typeof( DaimyosHelm ), typeof( DemonForks ), typeof( DragonNunchaku ), typeof( Exiler ), typeof( GlovesOfTheSun ),
 				typeof( HanzosBow ), typeof( LegsOfStability ), typeof( PeasantsBokuto ), typeof( PilferedDancerFans ), typeof( TheDestroyer ),
				typeof( TomeOfEnlightenment ), typeof( AncientUrn ), typeof( HonorableSwords ), typeof( PigmentsOfTokuno ), typeof( FluteOfRenewal ),
				typeof( LeurociansMempoOfFortune ), typeof( LesserPigmentsOfTokuno ), typeof( MetalPigmentsOfTokuno ), typeof( ChestOfHeirlooms )
 			};

		public static Type[] LesserArtifactsTotal => m_LesserArtifactsTotal;

		private static TreasuresOfTokunoEra _DropEra = TreasuresOfTokunoEra.None;
		private static TreasuresOfTokunoEra _RewardEra = TreasuresOfTokunoEra.ToTOne;

		public static TreasuresOfTokunoEra DropEra
		{
			get => _DropEra;
			set => _DropEra = value;
		}

		public static TreasuresOfTokunoEra RewardEra
		{
			get => _RewardEra;
			set => _RewardEra = value;
		}

		private static readonly Type[][] m_LesserArtifacts = new Type[][]
		{
			// ToT One Rewards
			new Type[] {
				typeof( AncientFarmersKasa ), typeof( AncientSamuraiDo ), typeof( ArmsOfTacticalExcellence ), typeof( BlackLotusHood ),
				typeof( DaimyosHelm ), typeof( DemonForks ), typeof( DragonNunchaku ), typeof( Exiler ), typeof( GlovesOfTheSun ),
				typeof( HanzosBow ), typeof( LegsOfStability ), typeof( PeasantsBokuto ), typeof( PilferedDancerFans ), typeof( TheDestroyer ),
				typeof( TomeOfEnlightenment ), typeof( AncientUrn ), typeof( HonorableSwords ), typeof( PigmentsOfTokuno ),
				typeof( FluteOfRenewal ), typeof( ChestOfHeirlooms )
			},
			// ToT Two Rewards
			new Type[] {
				typeof( MetalPigmentsOfTokuno ), typeof( AncientFarmersKasa ), typeof( AncientSamuraiDo ), typeof( ArmsOfTacticalExcellence ),
				typeof( MetalPigmentsOfTokuno ), typeof( BlackLotusHood ), typeof( DaimyosHelm ), typeof( DemonForks ),
				typeof( MetalPigmentsOfTokuno ), typeof( DragonNunchaku ), typeof( Exiler ), typeof( GlovesOfTheSun ), typeof( HanzosBow ),
				typeof( MetalPigmentsOfTokuno ), typeof( LegsOfStability ), typeof( PeasantsBokuto ), typeof( PilferedDancerFans ), typeof( TheDestroyer ),
				typeof( MetalPigmentsOfTokuno ), typeof( TomeOfEnlightenment ), typeof( AncientUrn ), typeof( HonorableSwords ),
				typeof( MetalPigmentsOfTokuno ), typeof( FluteOfRenewal ), typeof( ChestOfHeirlooms )
			},
			// ToT Three Rewards
			new Type[] {
				typeof( LesserPigmentsOfTokuno ), typeof( AncientFarmersKasa ), typeof( AncientSamuraiDo ), typeof( ArmsOfTacticalExcellence ),
				typeof( LesserPigmentsOfTokuno ), typeof( BlackLotusHood ), typeof( DaimyosHelm ), typeof( HanzosBow ),
				typeof( LesserPigmentsOfTokuno ), typeof( DemonForks ), typeof( DragonNunchaku ), typeof( Exiler ), typeof( GlovesOfTheSun ),
				typeof( LesserPigmentsOfTokuno ), typeof( LegsOfStability ), typeof( PeasantsBokuto ), typeof( PilferedDancerFans ), typeof( TheDestroyer ),
				typeof( LesserPigmentsOfTokuno ), typeof( TomeOfEnlightenment ), typeof( AncientUrn ), typeof( HonorableSwords ), typeof( FluteOfRenewal ),
				typeof( LesserPigmentsOfTokuno ), typeof( LeurociansMempoOfFortune ), typeof( ChestOfHeirlooms )
			}
		};

		public static Type[] LesserArtifacts => m_LesserArtifacts[(int)RewardEra - 1];

		private static Type[][] m_GreaterArtifacts = null;

		public static Type[] GreaterArtifacts
		{
			get
			{
				if (m_GreaterArtifacts == null)
				{
					m_GreaterArtifacts = new Type[ToTRedeemGump.NormalRewards.Length][];

					for (var i = 0; i < m_GreaterArtifacts.Length; i++)
					{
						m_GreaterArtifacts[i] = new Type[ToTRedeemGump.NormalRewards[i].Length];

						for (var j = 0; j < m_GreaterArtifacts[i].Length; j++)
						{
							m_GreaterArtifacts[i][j] = ToTRedeemGump.NormalRewards[i][j].Type;
						}
					}
				}

				return m_GreaterArtifacts[(int)RewardEra - 1];
			}
		}

		private static bool CheckLocation(Mobile m)
		{
			var r = m.Region;

			if (r.IsPartOf(typeof(Server.Regions.HouseRegion)) || Server.Multis.BaseBoat.FindBoatAt(m, m.Map) != null)
			{
				return false;
			}
			//TODO: a CanReach of something check as opposed to above?

			if (r.IsPartOf("Yomotsu Mines") || r.IsPartOf("Fan Dancer's Dojo"))
			{
				return true;
			}

			return (m.Map == Map.Tokuno);
		}

		public static void HandleKill(Mobile victim, Mobile killer)
		{
			var pm = killer as PlayerMobile;
			var bc = victim as BaseCreature;

			if (DropEra == TreasuresOfTokunoEra.None || pm == null || bc == null || !CheckLocation(bc) || !CheckLocation(pm) || !killer.InRange(victim, 18))
			{
				return;
			}

			if (bc.Controlled || bc.Owners.Count > 0 || bc.Fame <= 0)
			{
				return;
			}

			//25000 for 1/100 chance, 10 hyrus
			//1500, 1/1000 chance, 20 lizard men for that chance.

			pm.ToTTotalMonsterFame += (int)(bc.Fame * (1 + Math.Sqrt(pm.Luck) / 100));

			//This is the Exponentional regression with only 2 datapoints.
			//A log. func would also work, but it didn't make as much sense.
			//This function isn't OSI exact beign that I don't know OSI's func they used ;p
			var x = pm.ToTTotalMonsterFame;

			//const double A = 8.63316841 * Math.Pow( 10, -4 );
			const double A = 0.000863316841;
			//const double B = 4.25531915 * Math.Pow( 10, -6 );
			const double B = 0.00000425531915;

			var chance = A * Math.Pow(10, B * x);

			if (chance > Utility.RandomDouble())
			{
				Item i = null;

				try
				{
					i = Activator.CreateInstance(m_LesserArtifacts[(int)DropEra - 1][Utility.Random(m_LesserArtifacts[(int)DropEra - 1].Length)]) as Item;
				}
				catch
				{ }

				if (i != null)
				{
					pm.SendLocalizedMessage(1062317); // For your valor in combating the fallen beast, a special artifact has been bestowed on you.

					if (!pm.PlaceInBackpack(i))
					{
						if (pm.BankBox != null && pm.BankBox.TryDropItem(killer, i, false))
						{
							pm.SendLocalizedMessage(1079730); // The item has been placed into your bank box.
						}
						else
						{
							pm.SendLocalizedMessage(1072523); // You find an artifact, but your backpack and bank are too full to hold it.
							i.MoveToWorld(pm.Location, pm.Map);
						}
					}

					pm.ToTTotalMonsterFame = 0;
				}
			}
		}
	}

	public class TreasuresOfTokunoPersistance : Item
	{
		private static TreasuresOfTokunoPersistance m_Instance;

		public static TreasuresOfTokunoPersistance Instance => m_Instance;

		public override string DefaultName => "TreasuresOfTokuno Persistance - Internal";

		public static void Initialize()
		{
			if (m_Instance == null)
			{
				new TreasuresOfTokunoPersistance();
			}
		}

		public TreasuresOfTokunoPersistance() : base(1)
		{
			Movable = false;

			if (m_Instance == null || m_Instance.Deleted)
			{
				m_Instance = this;
			}
			else
			{
				base.Delete();
			}
		}

		public TreasuresOfTokunoPersistance(Serial serial) : base(serial)
		{
			m_Instance = this;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.WriteEncodedInt((int)TreasuresOfTokuno.RewardEra);
			writer.WriteEncodedInt((int)TreasuresOfTokuno.DropEra);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						TreasuresOfTokuno.RewardEra = (TreasuresOfTokunoEra)reader.ReadEncodedInt();
						TreasuresOfTokuno.DropEra = (TreasuresOfTokunoEra)reader.ReadEncodedInt();

						break;
					}
			}
		}

		public override void Delete()
		{
		}
	}

	public class ToTAdminGump : Gump
	{
		private readonly int m_ToTEras;
		private static readonly string[] m_ToTInfo =
		{
			//Opening Message
			"<center>Treasures of Tokuno Admin</center><br>" +
			"-Use the gems to switch eras<br>" +
			"-Drop era and Reward era can be changed seperately<br>" +
			"-Drop era can be deactivated, Reward era is always activated",
			//Treasures of Tokuno 1 message
			"<center>Treasures of Tokuno 1</center><br>" +
			"-10 charge Bleach Pigment can drop as a Lesser Artifact<br>" +
			"-50 charge Neon Pigments available as a reward<br>",
			//Treasures of Tokuno 2 message
			"<center>Treasures of Tokuno 2</center><br>" +
			"-30 types of 1 charge Metallic Pigments drop as Lesser Artifacts<br>" +
			"-1 charge Bleach Pigment can drop as a Lesser Artifact<br>" +
			"-10 charge Greater Metallic Pigments available as a reward",
			//Treasures of Tokuno 3 message
			"<center>Treasures of Tokuno 3</center><br>" +
			"-10 types of 1 charge Fresh Pigments drop as Lesser Artifacts<br>" +
			"-1 charge Bleach Pigment can drop as a Lesser Artifact<br>" +
			"-Leurocian's Mempo Of Fortune can drop as a Lesser Artifact"
		};

		public ToTAdminGump() : base(30, 50)
		{
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			m_ToTEras = Enum.GetValues(typeof(TreasuresOfTokunoEra)).Length - 1;

			AddPage(0);
			AddBackground(0, 0, 320, 75 + (m_ToTEras * 25), 9200);
			AddImageTiled(25, 18, 270, 10, 9267);
			AddLabel(75, 5, 54, "Treasures of Tokuno Admin");
			AddLabel(10, 25, 54, "ToT Era");
			AddLabel(90, 25, 54, "Drop Era");
			AddLabel(195, 25, 54, "Reward Era");
			AddLabel(287, 25, 54, "Info");

			AddBackground(320, 0, 200, 150, 9200);
			AddImageTiled(325, 5, 190, 140, 2624);
			AddAlphaRegion(325, 5, 190, 140);

			SetupToTEras();
		}

		public void SetupToTEras()
		{
			var isActivated = TreasuresOfTokuno.DropEra != TreasuresOfTokunoEra.None;
			AddButton(75, 50, isActivated ? 2361 : 2360, isActivated ? 2361 : 2360, 1, GumpButtonType.Reply, 0);
			AddLabel(90, 45, isActivated ? 167 : 137, isActivated ? "Activated" : "Deactivated");

			for (var i = 0; i < m_ToTEras; i++)
			{
				var yoffset = (i * 25);

				var isThisDropEra = ((int)TreasuresOfTokuno.DropEra - 1) == i;
				var isThisRewardEra = ((int)TreasuresOfTokuno.RewardEra - 1) == i;
				var dropButtonID = isThisDropEra ? 2361 : 2360;
				var rewardButtonID = isThisRewardEra ? 2361 : 2360;

				AddLabel(10, 70 + yoffset, 2100, "ToT " + (i + 1));
				AddButton(75, 75 + yoffset, dropButtonID, dropButtonID, 2 + (i * 2), GumpButtonType.Reply, 0);
				AddLabel(90, 70 + yoffset, isThisDropEra ? 167 : 137, isThisDropEra ? "Active" : "Inactive");
				AddButton(180, 75 + yoffset, rewardButtonID, rewardButtonID, 2 + (i * 2) + 1, GumpButtonType.Reply, 0);
				AddLabel(195, 70 + yoffset, isThisRewardEra ? 167 : 137, isThisRewardEra ? "Active" : "Inactive");

				AddButton(285, 70 + yoffset, 4005, 4006, i, GumpButtonType.Page, 2 + i);
			}

			for (var i = 0; i < m_ToTInfo.Length; i++)
			{
				AddPage(1 + i);
				AddHtml(330, 10, 180, 130, m_ToTInfo[i], false, true);
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var button = info.ButtonID;
			var from = sender.Mobile;

			if (button == 1)
			{
				TreasuresOfTokuno.DropEra = TreasuresOfTokunoEra.None;
				from.SendMessage("Treasures of Tokuno Drops have been deactivated");
			}
			else if (button >= 2)
			{
				int selectedToT;
				if (button % 2 == 0)
				{
					selectedToT = button / 2;
					TreasuresOfTokuno.DropEra = (TreasuresOfTokunoEra)(selectedToT);
					from.SendMessage("Treasures of Tokuno " + selectedToT + " Drops have been enabled");
				}
				else
				{
					selectedToT = (button - 1) / 2;
					TreasuresOfTokuno.RewardEra = (TreasuresOfTokunoEra)(selectedToT);
					from.SendMessage("Treasures of Tokuno " + selectedToT + " Rewards have been enabled");
				}
			}
		}

		public static void Initialize()
		{
			CommandSystem.Register("ToTAdmin", AccessLevel.Administrator, new CommandEventHandler(ToTAdmin_OnCommand));
		}

		[Usage("ToTAdmin")]
		[Description("Displays a menu to configure Treasures of Tokuno.")]
		public static void ToTAdmin_OnCommand(CommandEventArgs e)
		{
			ToTAdminGump tg;

			tg = new ToTAdminGump();
			e.Mobile.CloseGump(typeof(ToTAdminGump));
			e.Mobile.SendGump(tg);
		}
	}

	public class ItemTileButtonInfo : ImageTileButtonInfo
	{
		private Item m_Item;

		public Item Item
		{
			get => m_Item;
			set => m_Item = value;
		}

		public ItemTileButtonInfo(Item i) : base(i.ItemID, i.Hue, ((i.Name == null || i.Name.Length <= 0) ? (TextDefinition)i.LabelNumber : (TextDefinition)i.Name))
		{
			m_Item = i;
		}
	}

	public class ToTTurnInGump : BaseImageTileButtonsGump
	{
		public static ArrayList FindRedeemableItems(Mobile m)
		{
			var pack = (Backpack)m.Backpack;
			if (pack == null)
			{
				return new ArrayList();
			}

			var items = new ArrayList(pack.FindItemsByType(TreasuresOfTokuno.LesserArtifactsTotal));
			var buttons = new ArrayList();

			for (var i = 0; i < items.Count; i++)
			{
				var item = (Item)items[i];
				if (item is ChestOfHeirlooms && !((ChestOfHeirlooms)item).Locked)
				{
					continue;
				}

				if (item is ChestOfHeirlooms && ((ChestOfHeirlooms)item).TrapLevel != 10)
				{
					continue;
				}

				if (item is PigmentsOfTokuno && ((PigmentsOfTokuno)item).Type != PigmentType.None)
				{
					continue;
				}

				buttons.Add(new ItemTileButtonInfo(item));
			}

			return buttons;
		}

		private readonly Mobile m_Collector;

		public ToTTurnInGump(Mobile collector, ArrayList buttons) : base(1071012, buttons) // Click a minor artifact to give it to Ihara Soko.
		{
			m_Collector = collector;
		}

		public ToTTurnInGump(Mobile collector, ItemTileButtonInfo[] buttons) : base(1071012, buttons) // Click a minor artifact to give it to Ihara Soko.
		{
			m_Collector = collector;
		}

		public override void HandleButtonResponse(NetState sender, int adjustedButton, ImageTileButtonInfo buttonInfo)
		{
			var pm = sender.Mobile as PlayerMobile;

			var item = ((ItemTileButtonInfo)buttonInfo).Item;

			if (!(pm != null && item.IsChildOf(pm.Backpack) && pm.InRange(m_Collector.Location, 7)))
			{
				return;
			}

			item.Delete();

			if (++pm.ToTItemsTurnedIn >= TreasuresOfTokuno.ItemsPerReward)
			{
				m_Collector.SayTo(pm, 1070980); // Congratulations! You have turned in enough minor treasures to earn a greater reward.

				pm.CloseGump(typeof(ToTTurnInGump));    //Sanity

				if (!pm.HasGump(typeof(ToTRedeemGump)))
				{
					pm.SendGump(new ToTRedeemGump(m_Collector, false));
				}
			}
			else
			{
				m_Collector.SayTo(pm, 1070981, String.Format("{0}\t{1}", pm.ToTItemsTurnedIn, TreasuresOfTokuno.ItemsPerReward)); // You have turned in ~1_COUNT~ minor artifacts. Turn in ~2_NUM~ to receive a reward.

				var buttons = FindRedeemableItems(pm);

				pm.CloseGump(typeof(ToTTurnInGump)); //Sanity

				if (buttons.Count > 0)
				{
					pm.SendGump(new ToTTurnInGump(m_Collector, buttons));
				}
			}
		}

		public override void HandleCancel(NetState sender)
		{
			var pm = sender.Mobile as PlayerMobile;

			if (pm == null || !pm.InRange(m_Collector.Location, 7))
			{
				return;
			}

			if (pm.ToTItemsTurnedIn == 0)
			{
				m_Collector.SayTo(pm, 1071013); // Bring me 10 of the lost treasures of Tokuno and I will reward you with a valuable item.
			}
			else if (pm.ToTItemsTurnedIn < TreasuresOfTokuno.ItemsPerReward)    //This case should ALWAYS be true with this gump, jsut a sanity check
			{
				m_Collector.SayTo(pm, 1070981, String.Format("{0}\t{1}", pm.ToTItemsTurnedIn, TreasuresOfTokuno.ItemsPerReward)); // You have turned in ~1_COUNT~ minor artifacts. Turn in ~2_NUM~ to receive a reward.
			}
			else
			{
				m_Collector.SayTo(pm, 1070982); // When you wish to choose your reward, you have but to approach me again.
			}
		}

	}

	public class ToTRedeemGump : BaseImageTileButtonsGump
	{
		public class TypeTileButtonInfo : ImageTileButtonInfo
		{
			private readonly Type m_Type;

			public Type Type => m_Type;

			public TypeTileButtonInfo(Type type, int itemID, int hue, TextDefinition label, int localizedToolTip) : base(itemID, hue, label, localizedToolTip)
			{
				m_Type = type;
			}

			public TypeTileButtonInfo(Type type, int itemID, TextDefinition label) : this(type, itemID, 0, label, -1)
			{
			}

			public TypeTileButtonInfo(Type type, int itemID, TextDefinition label, int localizedToolTip) : this(type, itemID, 0, label, localizedToolTip)
			{
			}
		}

		public class PigmentsTileButtonInfo : ImageTileButtonInfo
		{
			private PigmentType m_Pigment;

			public PigmentType Pigment
			{
				get => m_Pigment;

				set => m_Pigment = value;
			}

			public PigmentsTileButtonInfo(PigmentType p) : base(0xEFF, PigmentsOfTokuno.GetInfo(p)[0], PigmentsOfTokuno.GetInfo(p)[1])
			{
				m_Pigment = p;
			}
		}

		#region ToT Normal Rewards Table
		private static readonly TypeTileButtonInfo[][] m_NormalRewards = new TypeTileButtonInfo[][]
		{
			// ToT One Rewards
			new TypeTileButtonInfo[] {
				new TypeTileButtonInfo( typeof( SwordsOfProsperity ),    0x27A9, 1070963, 1071002 ),
				new TypeTileButtonInfo( typeof( SwordOfTheStampede ),    0x27A2, 1070964, 1070978 ),
				new TypeTileButtonInfo( typeof( WindsEdge ),             0x27A3, 1070965, 1071003 ),
				new TypeTileButtonInfo( typeof( DarkenedSky ),           0x27AD, 1070966, 1071004 ),
				new TypeTileButtonInfo( typeof( TheHorselord ),          0x27A5, 1070967, 1071005 ),
				new TypeTileButtonInfo( typeof( RuneBeetleCarapace ),    0x277D, 1070968, 1071006 ),
				new TypeTileButtonInfo( typeof( KasaOfTheRajin ),        0x2798, 1070969, 1071007 ),
				new TypeTileButtonInfo( typeof( Stormgrip ),             0x2792, 1070970, 1071008 ),
				new TypeTileButtonInfo( typeof( TomeOfLostKnowledge ),   0x0EFA, 0x530, 1070971, 1071009 ),
				new TypeTileButtonInfo( typeof( PigmentsOfTokuno ),      0x0EFF, 1070933, 1071011 )
			},
			// ToT Two Rewards
			new TypeTileButtonInfo[] {
				new TypeTileButtonInfo( typeof( SwordsOfProsperity ),    0x27A9, 1070963, 1071002 ),
				new TypeTileButtonInfo( typeof( SwordOfTheStampede ),    0x27A2, 1070964, 1070978 ),
				new TypeTileButtonInfo( typeof( WindsEdge ),             0x27A3, 1070965, 1071003 ),
				new TypeTileButtonInfo( typeof( DarkenedSky ),           0x27AD, 1070966, 1071004 ),
				new TypeTileButtonInfo( typeof( TheHorselord ),          0x27A5, 1070967, 1071005 ),
				new TypeTileButtonInfo( typeof( RuneBeetleCarapace ),    0x277D, 1070968, 1071006 ),
				new TypeTileButtonInfo( typeof( KasaOfTheRajin ),        0x2798, 1070969, 1071007 ),
				new TypeTileButtonInfo( typeof( Stormgrip ),             0x2792, 1070970, 1071008 ),
				new TypeTileButtonInfo( typeof( TomeOfLostKnowledge ),   0x0EFA, 0x530, 1070971, 1071009 ),
				new TypeTileButtonInfo( typeof( PigmentsOfTokuno ),      0x0EFF, 1070933, 1071011 )
			},
			// ToT Three Rewards
			new TypeTileButtonInfo[] {
				new TypeTileButtonInfo( typeof( SwordsOfProsperity ),    0x27A9, 1070963, 1071002 ),
				new TypeTileButtonInfo( typeof( SwordOfTheStampede ),    0x27A2, 1070964, 1070978 ),
				new TypeTileButtonInfo( typeof( WindsEdge ),             0x27A3, 1070965, 1071003 ),
				new TypeTileButtonInfo( typeof( DarkenedSky ),           0x27AD, 1070966, 1071004 ),
				new TypeTileButtonInfo( typeof( TheHorselord ),          0x27A5, 1070967, 1071005 ),
				new TypeTileButtonInfo( typeof( RuneBeetleCarapace ),    0x277D, 1070968, 1071006 ),
				new TypeTileButtonInfo( typeof( KasaOfTheRajin ),        0x2798, 1070969, 1071007 ),
				new TypeTileButtonInfo( typeof( Stormgrip ),             0x2792, 1070970, 1071008 ),
				new TypeTileButtonInfo( typeof( TomeOfLostKnowledge ),   0x0EFA, 0x530, 1070971, 1071009 )
			}
		};
		#endregion

		public static TypeTileButtonInfo[][] NormalRewards => m_NormalRewards;

		#region ToT Pigment Rewards Table
		private static readonly PigmentsTileButtonInfo[][] m_PigmentRewards = new PigmentsTileButtonInfo[][]
		{
			// ToT One Pigment Rewards
			new PigmentsTileButtonInfo[] {
				new PigmentsTileButtonInfo( PigmentType.ParagonGold ),
				new PigmentsTileButtonInfo( PigmentType.VioletCouragePurple ),
				new PigmentsTileButtonInfo( PigmentType.InvulnerabilityBlue ),
				new PigmentsTileButtonInfo( PigmentType.LunaWhite ),
				new PigmentsTileButtonInfo( PigmentType.DryadGreen ),
				new PigmentsTileButtonInfo( PigmentType.ShadowDancerBlack ),
				new PigmentsTileButtonInfo( PigmentType.BerserkerRed ),
				new PigmentsTileButtonInfo( PigmentType.NoxGreen ),
				new PigmentsTileButtonInfo( PigmentType.RumRed ),
				new PigmentsTileButtonInfo( PigmentType.FireOrange )
			},
			// ToT Two Pigment Rewards
			new PigmentsTileButtonInfo[] {
				new PigmentsTileButtonInfo( PigmentType.FadedCoal ),
				new PigmentsTileButtonInfo( PigmentType.Coal ),
				new PigmentsTileButtonInfo( PigmentType.FadedGold ),
				new PigmentsTileButtonInfo( PigmentType.StormBronze ),
				new PigmentsTileButtonInfo( PigmentType.Rose ),
				new PigmentsTileButtonInfo( PigmentType.MidnightCoal ),
				new PigmentsTileButtonInfo( PigmentType.FadedBronze ),
				new PigmentsTileButtonInfo( PigmentType.FadedRose ),
				new PigmentsTileButtonInfo( PigmentType.DeepRose )
			},
			// ToT Three Pigment Rewards
			new PigmentsTileButtonInfo[] {
				new PigmentsTileButtonInfo( PigmentType.ParagonGold ),
				new PigmentsTileButtonInfo( PigmentType.VioletCouragePurple ),
				new PigmentsTileButtonInfo( PigmentType.InvulnerabilityBlue ),
				new PigmentsTileButtonInfo( PigmentType.LunaWhite ),
				new PigmentsTileButtonInfo( PigmentType.DryadGreen ),
				new PigmentsTileButtonInfo( PigmentType.ShadowDancerBlack ),
				new PigmentsTileButtonInfo( PigmentType.BerserkerRed ),
				new PigmentsTileButtonInfo( PigmentType.NoxGreen ),
				new PigmentsTileButtonInfo( PigmentType.RumRed ),
				new PigmentsTileButtonInfo( PigmentType.FireOrange )
			}
		};
		#endregion

		public static PigmentsTileButtonInfo[][] PigmentRewards => m_PigmentRewards;

		private readonly Mobile m_Collector;

		public ToTRedeemGump(Mobile collector, bool pigments) : base(pigments ? 1070986 : 1070985, pigments ? m_PigmentRewards[(int)TreasuresOfTokuno.RewardEra - 1] : m_NormalRewards[(int)TreasuresOfTokuno.RewardEra - 1])
		{
			m_Collector = collector;
		}

		public override void HandleButtonResponse(NetState sender, int adjustedButton, ImageTileButtonInfo buttonInfo)
		{
			var pm = sender.Mobile as PlayerMobile;

			if (pm == null || !pm.InRange(m_Collector.Location, 7) || !(pm.ToTItemsTurnedIn >= TreasuresOfTokuno.ItemsPerReward))
			{
				return;
			}

			var pigments = (buttonInfo is PigmentsTileButtonInfo);

			Item item = null;

			if (pigments)
			{
				var p = buttonInfo as PigmentsTileButtonInfo;

				item = new PigmentsOfTokuno(p.Pigment);
			}
			else
			{
				var t = buttonInfo as TypeTileButtonInfo;

				if (t.Type == typeof(PigmentsOfTokuno)) //Special case of course.
				{
					pm.CloseGump(typeof(ToTTurnInGump));    //Sanity
					pm.CloseGump(typeof(ToTRedeemGump));

					pm.SendGump(new ToTRedeemGump(m_Collector, true));

					return;
				}

				try
				{
					item = (Item)Activator.CreateInstance(t.Type);
				}
				catch { }
			}

			if (item == null)
			{
				return; //Sanity
			}

			if (pm.AddToBackpack(item))
			{
				pm.ToTItemsTurnedIn -= TreasuresOfTokuno.ItemsPerReward;
				m_Collector.SayTo(pm, 1070984, (item.Name == null || item.Name.Length <= 0) ? String.Format("#{0}", item.LabelNumber) : item.Name); // You have earned the gratitude of the Empire. I have placed the ~1_OBJTYPE~ in your backpack.
			}
			else
			{
				item.Delete();
				m_Collector.SayTo(pm, 500722); // You don't have enough room in your backpack!
				m_Collector.SayTo(pm, 1070982); // When you wish to choose your reward, you have but to approach me again.
			}
		}


		public override void HandleCancel(NetState sender)
		{
			var pm = sender.Mobile as PlayerMobile;

			if (pm == null || !pm.InRange(m_Collector.Location, 7))
			{
				return;
			}

			if (pm.ToTItemsTurnedIn == 0)
			{
				m_Collector.SayTo(pm, 1071013); // Bring me 10 of the lost treasures of Tokuno and I will reward you with a valuable item.
			}
			else if (pm.ToTItemsTurnedIn < TreasuresOfTokuno.ItemsPerReward)    //This and above case should ALWAYS be FALSE with this gump, jsut a sanity check
			{
				m_Collector.SayTo(pm, 1070981, String.Format("{0}\t{1}", pm.ToTItemsTurnedIn, TreasuresOfTokuno.ItemsPerReward)); // You have turned in ~1_COUNT~ minor artifacts. Turn in ~2_NUM~ to receive a reward.
			}
			else
			{
				m_Collector.SayTo(pm, 1070982); // When you wish to choose your reward, you have but to approach me again.
			}
		}
	}
}