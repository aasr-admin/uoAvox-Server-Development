using Server.Items;
using Server.Mobiles;

using System;

namespace Server.Engines.Events
{
	public abstract class BasePigmentsOfTokuno : Item, IUsesRemaining
	{
		private static readonly Type[] m_Glasses = new Type[]
		{
			typeof( MaritimeGlasses ),
			typeof( WizardsGlasses ),
			typeof( TradeGlasses ),
			typeof( LyricalGlasses ),
			typeof( NecromanticGlasses ),
			typeof( LightOfWayGlasses ),
			typeof( FoldedSteelGlasses ),
			typeof( PoisonedGlasses ),
			typeof( TreasureTrinketGlasses ),
			typeof( MaceShieldGlasses ),
			typeof( ArtsGlasses ),
			typeof( AnthropomorphistGlasses )
		};

		private static readonly Type[] m_Replicas = new Type[]
		{
			typeof( ANecromancerShroud ),
			typeof( BraveKnightOfTheBritannia ),
			typeof( CaptainJohnsHat ),
			typeof( DetectiveBoots ),
			typeof( DjinnisRing ),
			typeof( EmbroideredOakLeafCloak ),
			typeof( GuantletsOfAnger ),
			typeof( LieutenantOfTheBritannianRoyalGuard ),
			typeof( OblivionsNeedle ),
			typeof( RoyalGuardSurvivalKnife ),
			typeof( SamaritanRobe ),
			typeof( TheMostKnowledgePerson ),
			typeof( TheRobeOfBritanniaAri ),
			typeof( AcidProofRobe ),
			typeof( Calm ),
			typeof( CrownOfTalKeesh ),
			typeof( FangOfRactus ),
			typeof( GladiatorsCollar ),
			typeof( OrcChieftainHelm ),
			typeof( Pacify ),
			typeof( Quell ),
			typeof( ShroudOfDeciet ),
			typeof( Subdue )
		};

		private static readonly Type[] m_DyableHeritageItems = new Type[]
		{
			typeof( ChargerOfTheFallen ),
			typeof( AncientSamuraiHelm ),
			typeof( HolySword ),
			typeof( LeggingsOfEmbers ),
			typeof( ShaminoCrossbow )
		};

		public override int LabelNumber => 1070933;  // Pigments of Tokuno

		private int m_UsesRemaining;
		private TextDefinition m_Label;

		protected TextDefinition Label
		{
			get => m_Label;
			set { m_Label = value; InvalidateProperties(); }
		}

		#region Old Item Serialization Vars
		/* DO NOT USE! Only used in serialization of pigments that originally derived from Item */
		private bool m_InheritsItem;

		protected bool InheritsItem => m_InheritsItem;
		#endregion

		public BasePigmentsOfTokuno() : base(0xEFF)
		{
			Weight = 1.0;
			m_UsesRemaining = 1;
		}

		public BasePigmentsOfTokuno(int uses) : base(0xEFF)
		{
			Weight = 1.0;
			m_UsesRemaining = uses;
		}

		public BasePigmentsOfTokuno(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Label != null && m_Label > 0)
			{
				TextDefinition.AddTo(list, m_Label);
			}

			list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsAccessibleTo(from) && from.InRange(GetWorldLocation(), 3))
			{
				from.SendLocalizedMessage(1070929); // Select the artifact or enhanced magic item to dye.
				from.BeginTarget(3, false, Server.Targeting.TargetFlags.None, new TargetStateCallback(InternalCallback), this);
			}
			else
			{
				from.SendLocalizedMessage(502436); // That is not accessible.
			}
		}

		private void InternalCallback(Mobile from, object targeted, object state)
		{
			var pigment = (BasePigmentsOfTokuno)state;

			if (pigment.Deleted || pigment.UsesRemaining <= 0 || !from.InRange(pigment.GetWorldLocation(), 3) || !pigment.IsAccessibleTo(from))
			{
				return;
			}

			var i = targeted as Item;

			if (i == null)
			{
				from.SendLocalizedMessage(1070931); // You can only dye artifacts and enhanced magic items with this tub.
			}
			else if (!from.InRange(i.GetWorldLocation(), 3) || !IsAccessibleTo(from))
			{
				from.SendLocalizedMessage(502436); // That is not accessible.
			}
			else if (from.Items.Contains(i))
			{
				from.SendLocalizedMessage(1070930); // Can't dye artifacts or enhanced magic items that are being worn.
			}
			else if (i.IsLockedDown)
			{
				from.SendLocalizedMessage(1070932); // You may not dye artifacts and enhanced magic items which are locked down.
			}
			else if (i.QuestItem)
			{
				from.SendLocalizedMessage(1042417); // You cannot dye that.
			}
			else if (i is MetalPigmentsOfTokuno)
			{
				from.SendLocalizedMessage(1042417); // You cannot dye that.
			}
			else if (i is LesserPigmentsOfTokuno)
			{
				from.SendLocalizedMessage(1042417); // You cannot dye that.
			}
			else if (i is PigmentsOfTokuno)
			{
				from.SendLocalizedMessage(1042417); // You cannot dye that.
			}
			else if (!IsValidItem(i))
			{
				from.SendLocalizedMessage(1070931); // You can only dye artifacts and enhanced magic items with this tub.	//Yes, it says tub on OSI.  Don't ask me why ;p
			}
			else
			{
				//Notes: on OSI there IS no hue check to see if it's already hued.  and no messages on successful hue either
				i.Hue = Hue;

				if (--pigment.UsesRemaining <= 0)
				{
					pigment.Delete();
				}

				from.PlaySound(0x23E); // As per OSI TC1
			}
		}

		public static bool IsValidItem(Item i)
		{
			if (i is BasePigmentsOfTokuno)
			{
				return false;
			}

			var t = i.GetType();

			var resource = CraftResource.None;

			if (i is BaseWeapon)
			{
				resource = ((BaseWeapon)i).Resource;
			}
			else if (i is BaseArmor)
			{
				resource = ((BaseArmor)i).Resource;
			}
			else if (i is BaseClothing)
			{
				resource = ((BaseClothing)i).Resource;
			}

			if (!CraftResources.IsStandard(resource))
			{
				return true;
			}

			if (i is ITokunoDyable)
			{
				return true;
			}

			return (
				IsInTypeList(t, TreasuresOfTokuno.LesserArtifactsTotal)
				|| IsInTypeList(t, TreasuresOfTokuno.GreaterArtifacts)
				|| IsInTypeList(t, BaseTerraBossAward.ArtifactRarity10)
				|| IsInTypeList(t, BaseTerraBossAward.ArtifactRarity11)
				|| IsInTypeList(t, ValorSpawner.Artifacts)
				|| IsInTypeList(t, StealableSpawner.TypesOfEntires)
				|| IsInTypeList(t, Paragon.Artifacts)
				|| IsInTypeList(t, BaseAquaBossAward.SaltWaterArtifact)
				|| IsInTypeList(t, TreasureMapChest.Artifacts)
				|| IsInTypeList(t, m_Replicas)
				|| IsInTypeList(t, m_DyableHeritageItems)
				|| IsInTypeList(t, m_Glasses)
				);
		}

		private static bool IsInTypeList(Type t, Type[] list)
		{
			for (var i = 0; i < list.Length; i++)
			{
				if (list[i] == t)
				{
					return true;
				}
			}

			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1);

			writer.WriteEncodedInt(m_UsesRemaining);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_UsesRemaining = reader.ReadEncodedInt();
						break;
					}
				case 0: // Old pigments that inherited from item
					{
						m_InheritsItem = true;

						if (this is LesserPigmentsOfTokuno)
						{
							((LesserPigmentsOfTokuno)this).Type = (LesserPigmentType)reader.ReadEncodedInt();
						}
						else if (this is PigmentsOfTokuno)
						{
							((PigmentsOfTokuno)this).Type = (PigmentType)reader.ReadEncodedInt();
						}
						else if (this is MetalPigmentsOfTokuno)
						{
							reader.ReadEncodedInt();
						}

						m_UsesRemaining = reader.ReadEncodedInt();

						break;
					}
			}


		}

		#region IUsesRemaining Members

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get => m_UsesRemaining;
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		public bool ShowUsesRemaining
		{
			get => true;
			set { }
		}

		#endregion
	}

	public enum PigmentType
	{
		None,
		ParagonGold,
		VioletCouragePurple,
		InvulnerabilityBlue,
		LunaWhite,
		DryadGreen,
		ShadowDancerBlack,
		BerserkerRed,
		NoxGreen,
		RumRed,
		FireOrange,
		FadedCoal,
		Coal,
		FadedGold,
		StormBronze,
		Rose,
		MidnightCoal,
		FadedBronze,
		FadedRose,
		DeepRose
	}

	public class PigmentsOfTokuno : BasePigmentsOfTokuno
	{
		private static readonly int[][] m_Table = new int[][]
		{
			// Hue, Label
			new int[]{ /*PigmentType.None,*/ 0, -1 },
			new int[]{ /*PigmentType.ParagonGold,*/ 0x501, 1070987 },
			new int[]{ /*PigmentType.VioletCouragePurple,*/ 0x486, 1070988 },
			new int[]{ /*PigmentType.InvulnerabilityBlue,*/ 0x4F2, 1070989 },
			new int[]{ /*PigmentType.LunaWhite,*/ 0x47E, 1070990 },
			new int[]{ /*PigmentType.DryadGreen,*/ 0x48F, 1070991 },
			new int[]{ /*PigmentType.ShadowDancerBlack,*/ 0x455, 1070992 },
			new int[]{ /*PigmentType.BerserkerRed,*/ 0x21, 1070993 },
			new int[]{ /*PigmentType.NoxGreen,*/ 0x58C, 1070994 },
			new int[]{ /*PigmentType.RumRed,*/ 0x66C, 1070995 },
			new int[]{ /*PigmentType.FireOrange,*/ 0x54F, 1070996 },
			new int[]{ /*PigmentType.Fadedcoal,*/ 0x96A, 1079579 },
			new int[]{ /*PigmentType.Coal,*/ 0x96B, 1079580 },
			new int[]{ /*PigmentType.FadedGold,*/ 0x972, 1079581 },
			new int[]{ /*PigmentType.StormBronze,*/ 0x977, 1079582 },
			new int[]{ /*PigmentType.Rose,*/ 0x97C, 1079583 },
			new int[]{ /*PigmentType.MidnightCoal,*/ 0x96C, 1079584 },
			new int[]{ /*PigmentType.FadedBronze,*/ 0x975, 1079585 },
			new int[]{ /*PigmentType.FadedRose,*/ 0x97B, 1079586 },
			new int[]{ /*PigmentType.DeepRose,*/ 0x97E, 1079587 }
		};

		public static int[][] Table => m_Table;

		public static int[] GetInfo(PigmentType type)
		{
			var v = (int)type;

			if (v < 0 || v >= m_Table.Length)
			{
				v = 0;
			}

			return m_Table[v];
		}

		private PigmentType m_Type;

		[CommandProperty(AccessLevel.GameMaster)]
		public PigmentType Type
		{
			get => m_Type;
			set
			{
				m_Type = value;

				var v = (int)m_Type;

				if (v >= 0 && v < m_Table.Length)
				{
					Hue = m_Table[v][0];
					Label = m_Table[v][1];
				}
				else
				{
					Hue = 0;
					Label = -1;
				}
			}
		}

		public override int LabelNumber => 1070933;  // Pigments of Tokuno

		[Constructable]
		public PigmentsOfTokuno() : this(PigmentType.None, 10)
		{
		}

		[Constructable]
		public PigmentsOfTokuno(PigmentType type) : this(type, (type == PigmentType.None || type >= PigmentType.FadedCoal) ? 10 : 50)
		{
		}

		[Constructable]
		public PigmentsOfTokuno(PigmentType type, int uses) : base(uses)
		{
			Weight = 1.0;
			Type = type;
		}

		public PigmentsOfTokuno(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1);

			writer.WriteEncodedInt((int)m_Type);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = (InheritsItem ? 0 : reader.ReadInt()); // Required for BasePigmentsOfTokuno insertion

			switch (version)
			{
				case 1: Type = (PigmentType)reader.ReadEncodedInt(); break;
				case 0: break;
			}
		}
	}

	public enum LesserPigmentType
	{
		None,
		PaleOrange,
		FreshRose,
		ChaosBlue,
		Silver,
		NobleGold,
		LightGreen,
		PaleBlue,
		FreshPlum,
		DeepBrown,
		BurntBrown
	}

	public class LesserPigmentsOfTokuno : BasePigmentsOfTokuno
	{

		private static readonly int[][] m_Table = new int[][]
		{
			// Hue, Label
			new int[]{ /*PigmentType.None,*/ 0, -1 },
			new int[]{ /*PigmentType.PaleOrange,*/ 0x02E, 1071458 },
			new int[]{ /*PigmentType.FreshRose,*/ 0x4B9, 1071455 },
			new int[]{ /*PigmentType.ChaosBlue,*/ 0x005, 1071459 },
			new int[]{ /*PigmentType.Silver,*/ 0x3E9, 1071451 },
			new int[]{ /*PigmentType.NobleGold,*/ 0x227, 1071457 },
			new int[]{ /*PigmentType.LightGreen,*/ 0x1C8, 1071454 },
			new int[]{ /*PigmentType.PaleBlue,*/ 0x24F, 1071456 },
			new int[]{ /*PigmentType.FreshPlum,*/ 0x145, 1071450 },
			new int[]{ /*PigmentType.DeepBrown,*/ 0x3F0, 1071452 },
			new int[]{ /*PigmentType.BurntBrown,*/ 0x41A, 1071453 }
		};

		public static int[] GetInfo(LesserPigmentType type)
		{
			var v = (int)type;

			if (v < 0 || v >= m_Table.Length)
			{
				v = 0;
			}

			return m_Table[v];
		}

		private LesserPigmentType m_Type;

		[CommandProperty(AccessLevel.GameMaster)]
		public LesserPigmentType Type
		{
			get => m_Type;
			set
			{
				m_Type = value;

				var v = (int)m_Type;

				if (v >= 0 && v < m_Table.Length)
				{
					Hue = m_Table[v][0];
					Label = m_Table[v][1];
				}
				else
				{
					Hue = 0;
					Label = -1;
				}
			}
		}

		[Constructable]
		public LesserPigmentsOfTokuno() : this((LesserPigmentType)Utility.Random(0, 11))
		{
		}

		[Constructable]
		public LesserPigmentsOfTokuno(LesserPigmentType type) : base(1)
		{
			Weight = 1.0;
			Type = type;
		}

		public LesserPigmentsOfTokuno(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1);

			writer.WriteEncodedInt((int)m_Type);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = (InheritsItem ? 0 : reader.ReadInt()); // Required for BasePigmentsOfTokuno insertion

			switch (version)
			{
				case 1: Type = (LesserPigmentType)reader.ReadEncodedInt(); break;
				case 0: break;
			}
		}
	}

	public class MetalPigmentsOfTokuno : BasePigmentsOfTokuno
	{
		[Constructable]
		public MetalPigmentsOfTokuno() : base(1)
		{
			RandomHue();
			Label = -1;
		}

		public MetalPigmentsOfTokuno(Serial serial) : base(serial)
		{
		}

		public void RandomHue()
		{
			var a = Utility.Random(0, 30);
			if (a != 0)
			{
				Hue = a + 0x960;
			}
			else
			{
				Hue = 0;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = (InheritsItem ? 0 : reader.ReadInt()); // Required for BasePigmentsOfTokuno insertion
		}
	}
}