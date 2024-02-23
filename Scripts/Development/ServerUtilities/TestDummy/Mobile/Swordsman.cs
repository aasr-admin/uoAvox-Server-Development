using Server.Items;

namespace Server.Mobiles
{
	public class DummySword : Dummy
	{

		[Constructable]
		public DummySword() : base(AIType.AI_Melee, FightMode.Closest, 15, 1, 0.2, 0.6)
		{
			// A Dummy Swordsman
			var iHue = 20 + Team * 40;
			var jHue = 25 + Team * 40;

			// Skills and Stats
			InitStats(125, 125, 90);
			Skills[SkillName.Swords].Base = 120;
			Skills[SkillName.Anatomy].Base = 120;
			Skills[SkillName.Healing].Base = 120;
			Skills[SkillName.Tactics].Base = 120;
			Skills[SkillName.Parry].Base = 120;


			// Name
			Name = "Swordsman";

			// Equip
			var kat = new Katana {
				Crafter = this,
				Movable = true,
				Quality = ItemQuality.Regular
			};
			AddItem(kat);

			var bts = new Boots {
				Hue = iHue
			};
			AddItem(bts);

			var cht = new ChainChest {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(cht);

			var chl = new ChainLegs {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(chl);

			var pla = new PlateArms {
				Movable = false,
				LootType = LootType.Newbied,
				Crafter = this,
				Quality = ItemQuality.Regular
			};
			AddItem(pla);

			var band = new Bandage(50);
			AddToBackpack(band);
		}

		public DummySword(Serial serial) : base(serial)
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