using Server.Items;

using System;

namespace Server.Mobiles
{
	public class DummyStun : Dummy
	{

		[Constructable]
		public DummyStun() : base(AIType.AI_Mage, FightMode.Closest, 15, 1, 0.2, 0.6)
		{

			// A Dummy Stun Mage
			var iHue = 20 + Team * 40;
			var jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(90, 90, 125);
			Skills[SkillName.Magery].Base = 100;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Anatomy].Base = 80;
			Skills[SkillName.Wrestling].Base = 80;
			Skills[SkillName.Meditation].Base = 100;
			Skills[SkillName.Poisoning].Base = 100;


			// Name
			Name = "Stun Mage";

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

			var bts = new Boots {
				Hue = iHue
			};
			AddItem(bts);

			var cap = new Cap {
				Hue = iHue
			};
			AddItem(cap);

			// Spells
			AddSpellAttack(typeof(Spells.Magery.MagicArrowSpell));
			AddSpellAttack(typeof(Spells.Magery.WeakenSpell));
			AddSpellAttack(typeof(Spells.Magery.FireballSpell));
			AddSpellDefense(typeof(Spells.Magery.WallOfStoneSpell));
			AddSpellDefense(typeof(Spells.Magery.HealSpell));
		}

		public DummyStun(Serial serial) : base(serial)
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