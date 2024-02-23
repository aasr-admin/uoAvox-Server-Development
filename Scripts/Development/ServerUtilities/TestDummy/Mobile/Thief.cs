using Server.Items;

using System;

namespace Server.Mobiles
{
	[TypeAlias("Server.Mobiles.DummyTheif")]
	public class DummyThief : Dummy
	{
		[Constructable]
		public DummyThief() : base(AIType.AI_Thief, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Hybrid Thief
			var iHue = 20 + Team * 40;
			var jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(105, 105, 105);
			Skills[SkillName.Healing].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Stealing].Base = 120;
			Skills[SkillName.ArmsLore].Base = 100;
			Skills[SkillName.Meditation].Base = 120;
			Skills[SkillName.Wrestling].Base = 120;

			// Name
			Name = "Hybrid Thief";

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

			var band = new Bandage(50);
			AddToBackpack(band);
		}

		public DummyThief(Serial serial) : base(serial)
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