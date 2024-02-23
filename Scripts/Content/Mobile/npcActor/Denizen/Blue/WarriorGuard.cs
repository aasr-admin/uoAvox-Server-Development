using Server.Items;

using System;

namespace Server.Mobiles
{
	public class WarriorGuard : BaseTownGuard
	{
		[Constructable]
		public WarriorGuard()
			: base(AIType.AI_Melee, FightMode.Weakest, 18, 1, 0.1, 0.2)
		{
			InitStats(125, 100, 25);

			Skills[SkillName.Swords].Base = 120.0;

			if (Female)
			{
				switch (Utility.Random(2))
				{
					case 0: AddItem(new LeatherSkirt()); break;
					case 1: AddItem(new LeatherShorts()); break;
				}

				switch (Utility.Random(5))
				{
					case 0: AddItem(new FemaleLeatherChest()); break;
					case 1: AddItem(new FemaleStuddedChest()); break;
					case 2: AddItem(new LeatherBustierArms()); break;
					case 3: AddItem(new StuddedBustierArms()); break;
					case 4: AddItem(new FemalePlateChest()); break;
				}
			}
			else
			{
				AddItem(new PlateChest());
				AddItem(new PlateArms());
				AddItem(new PlateLegs());

				switch (Utility.Random(3))
				{
					case 0: AddItem(new Doublet(Utility.RandomNondyedHue())); break;
					case 1: AddItem(new Tunic(Utility.RandomNondyedHue())); break;
					case 2: AddItem(new BodySash(Utility.RandomNondyedHue())); break;
				}
			}

			if (Utility.RandomBool())
			{
				AddItem(new Halberd
				{
					Quality = ItemQuality.Exceptional
				});
			}
			else
			{
				AddItem(new Bardiche
				{
					Quality = ItemQuality.Exceptional
				});
			}
		}

		public WarriorGuard(Serial serial) 
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