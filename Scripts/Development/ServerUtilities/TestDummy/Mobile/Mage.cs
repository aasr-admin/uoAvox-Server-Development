using Server.Items;

using System;

namespace Server.Mobiles
{
	public class DummyAssassin : Dummy
	{

		[Constructable]
		public DummyAssassin() : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Hybrid Assassin
			var iHue = 20 + Team * 40;
			var jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(105, 105, 105);
			Skills[SkillName.Magery].Base = 120;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Swords].Base = 120;
			Skills[SkillName.Tactics].Base = 120;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Poisoning].Base = 100;

			// Name
			Name = "Hybrid Assassin";

			// Equip
			var book = new BookOfMagery(UInt64.MaxValue) {
				Movable = false,
				LootType = LootType.Newbied,
				Content = 0xFFFFFFFFFFFFFFFF
			};
			AddToBackpack(book);

			var kat = new Katana {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Poison = Poison.Deadly,
				PoisonCharges = 12,
				Quality = ItemQuality.Regular
			};
			AddToBackpack(kat);

			var lea = new LeatherArms {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(lea);

			var lec = new LeatherChest {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(lec);

			var leg = new LeatherGorget {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(leg);

			var lel = new LeatherLegs {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(lel);

			var snd = new Sandals {
				Hue = iHue,
				LootType = LootType.Newbied
			};
			AddItem(snd);

			var cap = new Cap {
				Hue = iHue
			};
			AddItem(cap);

			var robe = new Robe {
				Hue = iHue
			};
			AddItem(robe);

			var pota = new DeadlyPoisonPotion {
				LootType = LootType.Newbied
			};
			AddToBackpack(pota);

			var potb = new DeadlyPoisonPotion {
				LootType = LootType.Newbied
			};
			AddToBackpack(potb);

			var potc = new DeadlyPoisonPotion {
				LootType = LootType.Newbied
			};
			AddToBackpack(potc);

			var potd = new DeadlyPoisonPotion {
				LootType = LootType.Newbied
			};
			AddToBackpack(potd);

			var band = new Bandage(50);
			AddToBackpack(band);

		}

		public DummyAssassin(Serial serial) : base(serial)
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

	public class DummyNox : Dummy
	{

		[Constructable]
		public DummyNox() : base(AIType.AI_Mage, FightMode.Closest, 15, 1, 0.2, 0.6)
		{

			// A Dummy Nox or Pure Mage
			var iHue = 20 + Team * 40;
			var jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(90, 90, 125);
			Skills[SkillName.Magery].Base = 120;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Inscribe].Base = 100;
			Skills[SkillName.Wrestling].Base = 120;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Poisoning].Base = 100;


			// Name
			Name = "Nox Mage";

			// Equip
			var book = new BookOfMagery(UInt64.MaxValue) {
				Movable = false,
				LootType = LootType.Newbied
			};
			AddItem(book);

			var kilt = new Kilt {
				Hue = jHue
			};
			AddItem(kilt);

			var snd = new Sandals {
				Hue = iHue,
				LootType = LootType.Newbied
			};
			AddItem(snd);

			var skc = new SkullCap {
				Hue = iHue
			};
			AddItem(skc);

			// Spells
			AddSpellAttack(typeof(Spells.Magery.MagicArrowSpell));
			AddSpellAttack(typeof(Spells.Magery.WeakenSpell));
			AddSpellAttack(typeof(Spells.Magery.FireballSpell));
			AddSpellDefense(typeof(Spells.Magery.WallOfStoneSpell));
			AddSpellDefense(typeof(Spells.Magery.HealSpell));
		}

		public DummyNox(Serial serial) : base(serial)
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

	public class DummySuper : Dummy
	{

		[Constructable]
		public DummySuper() : base(AIType.AI_Mage, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Super Mage
			var iHue = 20 + Team * 40;
			var jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(125, 125, 125);
			Skills[SkillName.Magery].Base = 120;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Wrestling].Base = 120;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Poisoning].Base = 100;
			Skills[SkillName.Inscribe].Base = 100;

			// Name
			Name = "Super Mage";

			// Equip
			var book = new BookOfMagery(UInt64.MaxValue) {
				Movable = false,
				LootType = LootType.Newbied
			};
			AddItem(book);

			var lea = new LeatherArms {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(lea);

			var lec = new LeatherChest {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(lec);

			var leg = new LeatherGorget {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(leg);

			var lel = new LeatherLegs {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(lel);

			var snd = new Sandals {
				Hue = iHue,
				LootType = LootType.Newbied
			};
			AddItem(snd);

			var jhat = new JesterHat {
				Hue = iHue
			};
			AddItem(jhat);

			var dblt = new Doublet {
				Hue = iHue
			};
			AddItem(dblt);

			// Spells
			AddSpellAttack(typeof(Spells.Magery.MagicArrowSpell));
			AddSpellAttack(typeof(Spells.Magery.WeakenSpell));
			AddSpellAttack(typeof(Spells.Magery.FireballSpell));
			AddSpellDefense(typeof(Spells.Magery.WallOfStoneSpell));
			AddSpellDefense(typeof(Spells.Magery.HealSpell));
		}

		public DummySuper(Serial serial) : base(serial)
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