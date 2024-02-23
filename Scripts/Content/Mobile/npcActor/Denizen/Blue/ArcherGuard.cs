using Server.Items;

using System;

namespace Server.Mobiles
{
	public class ArcherGuard : BaseTownGuard
	{
		[Constructable]
		public ArcherGuard() 
			: base(AIType.AI_Archer, FightMode.Weakest, 18, 1, 0.1, 0.2)
		{
			InitStats(100, 125, 25);

			Skills[SkillName.Archery].Base = 120.0;

			AddItem(new StuddedChest());
			AddItem(new StuddedArms());
			AddItem(new StuddedGloves());
			AddItem(new StuddedGorget());
			AddItem(new StuddedLegs());
			AddItem(new Boots());
			AddItem(new SkullCap());

			var pack = Backpack ?? new Backpack 
			{
				Movable = false
			};

			if (Utility.RandomBool())
			{
				AddItem(new Bow
				{
					Quality = ItemQuality.Exceptional
				});

				pack.DropItem(new Arrow(250)
				{
					LootType = LootType.Blessed
				});
			}
			else
			{
				AddItem(new Crossbow
				{
					Quality = ItemQuality.Exceptional
				});

				pack.DropItem(new Bolt(250)
				{
					LootType = LootType.Blessed
				});
			}

			AddItem(pack);
		}

		public ArcherGuard(Serial serial) 
			: base(serial)
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