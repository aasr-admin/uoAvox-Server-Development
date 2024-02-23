namespace Server.Items
{
	/// AdventurersMachete
	public class AdventurersMachete : ElvenMachete
	{
		public override int LabelNumber => 1073533;  // adventurer's machete

		[Constructable]
		public AdventurersMachete()
		{
			Attributes.Luck = 20;
		}

		public AdventurersMachete(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// BoneMachete
	public class BoneMachete : ElvenMachete
	{
		public override WeaponAbility PrimaryAbility => null;
		public override WeaponAbility SecondaryAbility => null;

		public override int PhysicalResistance => 1;
		public override int FireResistance => 1;
		public override int ColdResistance => 1;
		public override int PoisonResistance => 1;
		public override int EnergyResistance => 1;

		public override int InitMinHits => 5;
		public override int InitMaxHits => 5;

		[Constructable]
		public BoneMachete()
		{
			ItemID = 0x20E;
		}

		public BoneMachete(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// Bokuto
	[FlipableAttribute(0x27A8, 0x27F3)]
	public class Bokuto : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Feint;
		public override WeaponAbility SecondaryAbility => WeaponAbility.NerveStrike;

		public override int AosStrengthReq => 20;
		public override int AosMinDamage => 9;
		public override int AosMaxDamage => 11;
		public override int AosSpeed => 53;
		public override float MlSpeed => 2.00f;

		public override int OldStrengthReq => 20;
		public override int OldMinDamage => 9;
		public override int OldMaxDamage => 11;
		public override int OldSpeed => 53;

		public override int DefHitSound => 0x536;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 25;
		public override int InitMaxHits => 50;

		[Constructable]
		public Bokuto() : base(0x27A8)
		{
			Weight = 7.0;
		}

		public Bokuto(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// BoneHarvester
	[FlipableAttribute(0x26BB, 0x26C5)]
	public class BoneHarvester : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ParalyzingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;

		public override int AosStrengthReq => 25;
		public override int AosMinDamage => 13;
		public override int AosMaxDamage => 15;
		public override int AosSpeed => 36;
		public override float MlSpeed => 3.00f;

		public override int OldStrengthReq => 25;
		public override int OldMinDamage => 13;
		public override int OldMaxDamage => 15;
		public override int OldSpeed => 36;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 70;

		[Constructable]
		public BoneHarvester() : base(0x26BB)
		{
			Weight = 3.0;
		}

		public BoneHarvester(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// Broadsword
	[FlipableAttribute(0xF5E, 0xF5F)]
	public class Broadsword : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ArmorIgnore;

		public override int AosStrengthReq => 30;
		public override int AosMinDamage => 14;
		public override int AosMaxDamage => 15;
		public override int AosSpeed => 33;
		public override float MlSpeed => 3.25f;

		public override int OldStrengthReq => 25;
		public override int OldMinDamage => 5;
		public override int OldMaxDamage => 29;
		public override int OldSpeed => 45;

		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 100;

		[Constructable]
		public Broadsword() : base(0xF5E)
		{
			Weight = 6.0;
		}

		public Broadsword(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// CorruptedRuneBlade
	public class CorruptedRuneBlade : RuneBlade
	{
		public override int LabelNumber => 1073540;  // Corrupted Rune Blade

		[Constructable]
		public CorruptedRuneBlade()
		{
			WeaponAttributes.ResistPhysicalBonus = -5;
			WeaponAttributes.ResistPoisonBonus = 12;
		}

		public CorruptedRuneBlade(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// CrescentBlade
	[FlipableAttribute(0x26C1, 0x26CB)]
	public class CrescentBlade : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;

		public override int AosStrengthReq => 55;
		public override int AosMinDamage => 11;
		public override int AosMaxDamage => 14;
		public override int AosSpeed => 47;
		public override float MlSpeed => 2.50f;

		public override int OldStrengthReq => 55;
		public override int OldMinDamage => 11;
		public override int OldMaxDamage => 14;
		public override int OldSpeed => 47;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 51;
		public override int InitMaxHits => 80;

		[Constructable]
		public CrescentBlade() : base(0x26C1)
		{
			Weight = 1.0;
		}

		public CrescentBlade(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// Cutlass
	[FlipableAttribute(0x1441, 0x1440)]
	public class Cutlass : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.BleedAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ShadowStrike;

		public override int AosStrengthReq => 25;
		public override int AosMinDamage => 11;
		public override int AosMaxDamage => 13;
		public override int AosSpeed => 44;
		public override float MlSpeed => 2.50f;

		public override int OldStrengthReq => 10;
		public override int OldMinDamage => 6;
		public override int OldMaxDamage => 28;
		public override int OldSpeed => 45;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 70;

		[Constructable]
		public Cutlass() : base(0x1441)
		{
			Weight = 8.0;
		}

		public Cutlass(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// DarkglowScimitar
	public class DarkglowScimitar : RadiantScimitar
	{
		public override int LabelNumber => 1073542;  // darkglow scimitar

		[Constructable]
		public DarkglowScimitar()
		{
			WeaponAttributes.HitDispel = 10;
		}

		public DarkglowScimitar(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// Daisho
	[FlipableAttribute(0x27A9, 0x27F4)]
	public class Daisho : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Feint;
		public override WeaponAbility SecondaryAbility => WeaponAbility.DoubleStrike;

		public override int AosStrengthReq => 40;
		public override int AosMinDamage => 13;
		public override int AosMaxDamage => 15;
		public override int AosSpeed => 40;
		public override float MlSpeed => 2.75f;

		public override int OldStrengthReq => 40;
		public override int OldMinDamage => 13;
		public override int OldMaxDamage => 15;
		public override int OldSpeed => 40;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 45;
		public override int InitMaxHits => 65;

		[Constructable]
		public Daisho() : base(0x27A9)
		{
			Weight = 8.0;
			Layer = Layer.TwoHanded;
		}

		public Daisho(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// DiseasedMachete
	public class DiseasedMachete : ElvenMachete
	{
		public override int LabelNumber => 1073536;  // Diseased Machete

		[Constructable]
		public DiseasedMachete()
		{
			WeaponAttributes.HitPoisonArea = 25;
		}

		public DiseasedMachete(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// ElvenMachete
	[FlipableAttribute(0x2D35, 0x2D29)]
	public class ElvenMachete : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DefenseMastery;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Bladeweave;

		public override int AosStrengthReq => 20;
		public override int AosMinDamage => 13;
		public override int AosMaxDamage => 15;
		public override int AosSpeed => 41;
		public override float MlSpeed => 2.75f;

		public override int OldStrengthReq => 20;
		public override int OldMinDamage => 13;
		public override int OldMaxDamage => 15;
		public override int OldSpeed => 41;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x239;

		public override int InitMinHits => 30;
		public override int InitMaxHits => 60;

		[Constructable]
		public ElvenMachete() : base(0x2D35)
		{
			Weight = 6.0;
		}

		public ElvenMachete(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// IcyScimitar
	public class IcyScimitar : RadiantScimitar
	{
		public override int LabelNumber => 1073543;  // icy scimitar

		[Constructable]
		public IcyScimitar()
		{
			WeaponAttributes.HitHarm = 15;
		}

		public IcyScimitar(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// Katana
	[FlipableAttribute(0x13FF, 0x13FE)]
	public class Katana : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ArmorIgnore;

		public override int AosStrengthReq => 25;
		public override int AosMinDamage => 11;
		public override int AosMaxDamage => 13;
		public override int AosSpeed => 46;
		public override float MlSpeed => 2.50f;

		public override int OldStrengthReq => 10;
		public override int OldMinDamage => 5;
		public override int OldMaxDamage => 26;
		public override int OldSpeed => 58;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 90;

		[Constructable]
		public Katana() : base(0x13FF)
		{
			Weight = 6.0;
		}

		public Katana(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// Kryss
	[FlipableAttribute(0x1401, 0x1400)]
	public class Kryss : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
		public override WeaponAbility SecondaryAbility => WeaponAbility.InfectiousStrike;

		public override int AosStrengthReq => 10;
		public override int AosMinDamage => 10;
		public override int AosMaxDamage => 12;
		public override int AosSpeed => 53;
		public override float MlSpeed => 2.00f;

		public override int OldStrengthReq => 10;
		public override int OldMinDamage => 3;
		public override int OldMaxDamage => 28;
		public override int OldSpeed => 53;

		public override int DefHitSound => 0x23C;
		public override int DefMissSound => 0x238;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 90;

		public override SkillName DefSkill => SkillName.Fencing;
		public override WeaponType DefType => WeaponType.Piercing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce1H;

		[Constructable]
		public Kryss() : base(0x1401)
		{
			Weight = 2.0;
		}

		public Kryss(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			if (Weight == 1.0)
			{
				Weight = 2.0;
			}
		}
	}

	/// Lance
	[FlipableAttribute(0x26C0, 0x26CA)]
	public class Lance : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Dismount;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ConcussionBlow;

		public override int AosStrengthReq => 95;
		public override int AosMinDamage => 17;
		public override int AosMaxDamage => 18;
		public override int AosSpeed => 24;
		public override float MlSpeed => 4.50f;

		public override int OldStrengthReq => 95;
		public override int OldMinDamage => 17;
		public override int OldMaxDamage => 18;
		public override int OldSpeed => 24;

		public override int DefHitSound => 0x23C;
		public override int DefMissSound => 0x238;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		public override SkillName DefSkill => SkillName.Fencing;
		public override WeaponType DefType => WeaponType.Piercing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce1H;

		[Constructable]
		public Lance() : base(0x26C0)
		{
			Weight = 12.0;
		}

		public Lance(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// Longsword
	[FlipableAttribute(0xF61, 0xF60)]
	public class Longsword : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ConcussionBlow;

		public override int AosStrengthReq => 35;
		public override int AosMinDamage => 15;
		public override int AosMaxDamage => 16;
		public override int AosSpeed => 30;
		public override float MlSpeed => 3.50f;

		public override int OldStrengthReq => 25;
		public override int OldMinDamage => 5;
		public override int OldMaxDamage => 33;
		public override int OldSpeed => 35;

		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		[Constructable]
		public Longsword() : base(0xF61)
		{
			Weight = 7.0;
		}

		public Longsword(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// MacheteOfDefense
	public class MacheteOfDefense : ElvenMachete
	{
		public override int LabelNumber => 1073535;  // machete of defense

		[Constructable]
		public MacheteOfDefense()
		{
			Attributes.DefendChance = 5;
		}

		public MacheteOfDefense(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// MagesRuneBlade
	public class MagesRuneBlade : RuneBlade
	{
		public override int LabelNumber => 1073538;  // mage's rune blade

		[Constructable]
		public MagesRuneBlade()
		{
			Attributes.CastSpeed = 1;
		}

		public MagesRuneBlade(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// OrcishMachete
	public class OrcishMachete : ElvenMachete
	{
		public override int LabelNumber => 1073534;  // Orcish Machete

		[Constructable]
		public OrcishMachete()
		{
			Attributes.BonusInt = -5;
			Attributes.WeaponDamage = 10;
		}

		public OrcishMachete(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// RadiantScimitar
	[FlipableAttribute(0x2D33, 0x2D27)]
	public class RadiantScimitar : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.WhirlwindAttack;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Bladeweave;

		public override int AosStrengthReq => 20;
		public override int AosMinDamage => 12;
		public override int AosMaxDamage => 14;
		public override int AosSpeed => 43;
		public override float MlSpeed => 2.50f;

		public override int OldStrengthReq => 20;
		public override int OldMinDamage => 12;
		public override int OldMaxDamage => 14;
		public override int OldSpeed => 43;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x239;

		public override int InitMinHits => 30;
		public override int InitMaxHits => 60;

		[Constructable]
		public RadiantScimitar() : base(0x2D33)
		{
			Weight = 9.0;
		}

		public RadiantScimitar(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// RuneBlade
	[FlipableAttribute(0x2D32, 0x2D26)]
	public class RuneBlade : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.Disarm;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Bladeweave;

		public override int AosStrengthReq => 30;
		public override int AosMinDamage => 15;
		public override int AosMaxDamage => 17;
		public override int AosSpeed => 35;
		public override float MlSpeed => 3.00f;

		public override int OldStrengthReq => 30;
		public override int OldMinDamage => 15;
		public override int OldMaxDamage => 17;
		public override int OldSpeed => 35;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x239;

		public override int InitMinHits => 30;
		public override int InitMaxHits => 60;

		[Constructable]
		public RuneBlade() : base(0x2D32)
		{
			Weight = 7.0;
			Layer = Layer.TwoHanded;
		}

		public RuneBlade(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// RuneBladeOfKnowledge
	public class RuneBladeOfKnowledge : RuneBlade
	{
		public override int LabelNumber => 1073539;  // rune blade of knowledge

		[Constructable]
		public RuneBladeOfKnowledge()
		{
			Attributes.SpellDamage = 5;
		}

		public RuneBladeOfKnowledge(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// Runesabre
	public class Runesabre : RuneBlade
	{
		public override int LabelNumber => 1073537;  // runesabre

		[Constructable]
		public Runesabre()
		{
			SkillBonuses.SetValues(0, SkillName.MagicResist, 5.0);
			WeaponAttributes.MageWeapon = -29;
		}

		public Runesabre(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// Scimitar
	[FlipableAttribute(0x13B6, 0x13B5)]
	public class Scimitar : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.DoubleStrike;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ParalyzingBlow;

		public override int AosStrengthReq => 25;
		public override int AosMinDamage => 13;
		public override int AosMaxDamage => 15;
		public override int AosSpeed => 37;
		public override float MlSpeed => 3.00f;

		public override int OldStrengthReq => 10;
		public override int OldMinDamage => 4;
		public override int OldMaxDamage => 30;
		public override int OldSpeed => 43;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 90;

		[Constructable]
		public Scimitar() : base(0x13B6)
		{
			Weight = 5.0;
		}

		public Scimitar(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// ThinLongsword
	[FlipableAttribute(0x13B8, 0x13B7)]
	public class ThinLongsword : BaseSword
	{
		public override int AosStrengthReq => 35;
		public override int AosMinDamage => 15;
		public override int AosMaxDamage => 16;
		public override int AosSpeed => 30;
		public override float MlSpeed => 3.50f;

		public override int OldStrengthReq => 25;
		public override int OldMinDamage => 5;
		public override int OldMaxDamage => 33;
		public override int OldSpeed => 35;

		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 110;

		[Constructable]
		public ThinLongsword() : base(0x13B8)
		{
			Weight = 1.0;
		}

		public ThinLongsword(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// TrueRadiantScimitar
	public class TrueRadiantScimitar : RadiantScimitar
	{
		public override int LabelNumber => 1073541;  // true radiant scimitar

		[Constructable]
		public TrueRadiantScimitar()
		{
			Attributes.NightSight = 1;
		}

		public TrueRadiantScimitar(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// TwinklingScimitar
	public class TwinklingScimitar : RadiantScimitar
	{
		public override int LabelNumber => 1073544;  // twinkling scimitar

		[Constructable]
		public TwinklingScimitar()
		{
			Attributes.DefendChance = 6;
		}

		public TwinklingScimitar(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// VikingSword
	[FlipableAttribute(0x13B9, 0x13Ba)]
	public class VikingSword : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ParalyzingBlow;

		public override int AosStrengthReq => 40;
		public override int AosMinDamage => 15;
		public override int AosMaxDamage => 17;
		public override int AosSpeed => 28;
		public override float MlSpeed => 3.75f;

		public override int OldStrengthReq => 40;
		public override int OldMinDamage => 6;
		public override int OldMaxDamage => 34;
		public override int OldSpeed => 30;

		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 100;

		[Constructable]
		public VikingSword() : base(0x13B9)
		{
			Weight = 6.0;
		}

		public VikingSword(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// Wakizashi
	[FlipableAttribute(0x27A4, 0x27EF)]
	public class Wakizashi : BaseSword
	{
		public override WeaponAbility PrimaryAbility => WeaponAbility.FrenziedWhirlwind;
		public override WeaponAbility SecondaryAbility => WeaponAbility.DoubleStrike;

		public override int AosStrengthReq => 20;
		public override int AosMinDamage => 11;
		public override int AosMaxDamage => 13;
		public override int AosSpeed => 44;
		public override float MlSpeed => 2.50f;

		public override int OldStrengthReq => 20;
		public override int OldMinDamage => 11;
		public override int OldMaxDamage => 13;
		public override int OldSpeed => 44;

		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x23A;

		public override int InitMinHits => 45;
		public override int InitMaxHits => 50;

		[Constructable]
		public Wakizashi() : base(0x27A4)
		{
			Weight = 5.0;
			Layer = Layer.OneHanded;
		}

		public Wakizashi(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}
}