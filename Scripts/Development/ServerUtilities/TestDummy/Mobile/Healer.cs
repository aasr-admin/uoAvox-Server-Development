using Server.Items;

using System;

namespace Server.Mobiles
{
	public class DummyHealer : Dummy
	{

		[Constructable]
		public DummyHealer() : base(AIType.AI_Healer, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Healer Mage
			var iHue = 20 + Team * 40;
			var jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(125, 125, 125);
			Skills[SkillName.Magery].Base = 120;
			Skills[SkillName.EvalInt].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Wrestling].Base = 120;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Healing].Base = 100;

			// Name
			Name = "Healer";

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

			var cap = new Cap {
				Hue = iHue
			};
			AddItem(cap);

			var robe = new Robe {
				Hue = iHue
			};
			AddItem(robe);

		}

		public DummyHealer(Serial serial) : base(serial)
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