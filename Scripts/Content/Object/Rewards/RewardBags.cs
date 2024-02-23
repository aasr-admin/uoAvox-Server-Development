namespace Server.Items
{
	public static class RewardBag
	{
		public static void Fill(Container c, int itemCount, double talismanChance)
		{
			c.Hue = Utility.RandomNondyedHue();

			var done = 0;

			if (Utility.RandomDouble() < talismanChance)
			{
				c.DropItem(new RandomTalisman());
				++done;
			}

			for (; done < itemCount; ++done)
			{
				Item loot = null;

				switch (Utility.Random(5))
				{
					case 0: loot = Loot.RandomWeapon(); break;
					case 1: loot = Loot.RandomArmor(); break;
					case 2: loot = Loot.RandomRangedWeapon(); break;
					case 3: loot = Loot.RandomJewelry(); break;
					case 4: loot = Loot.RandomHat(); break;
				}

				if (loot == null)
				{
					continue;
				}

				Enhance(loot);

				c.DropItem(loot);
			}
		}

		public static void Enhance(Item loot)
		{
			if (loot is BaseWeapon w)
			{
				BaseRunicTool.ApplyAttributesTo(w, Utility.RandomMinMax(1, 5), 10, 80);
			}
			else if (loot is BaseArmor a)
			{
				BaseRunicTool.ApplyAttributesTo(a, Utility.RandomMinMax(1, 5), 10, 80);
			}
			else if (loot is BaseShield s)
			{
				BaseRunicTool.ApplyAttributesTo(s, Utility.RandomMinMax(1, 5), 10, 80);
			}
			else if (loot is BaseJewel j)
			{
				BaseRunicTool.ApplyAttributesTo(j, Utility.RandomMinMax(1, 5), 10, 80);
			}
		}
	}

	public class SmallBagOfTrinkets : Bag
	{
		[Constructable]
		public SmallBagOfTrinkets()
		{
			RewardBag.Fill(this, 1, 0.0);
		}

		public SmallBagOfTrinkets(Serial serial)
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
			_ = reader.ReadInt();
		}
	}

	public class BagOfTrinkets : Bag
	{
		[Constructable]
		public BagOfTrinkets()
		{
			RewardBag.Fill(this, 2, 0.05);
		}

		public BagOfTrinkets(Serial serial)
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
			_ = reader.ReadInt();
		}
	}

	public class BagOfTreasure : Bag
	{
		[Constructable]
		public BagOfTreasure()
		{
			RewardBag.Fill(this, 3, 0.20);
		}

		public BagOfTreasure(Serial serial)
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
			_ = reader.ReadInt();
		}
	}

	public class LargeBagOfTreasure : Bag
	{
		[Constructable]
		public LargeBagOfTreasure()
		{
			RewardBag.Fill(this, 4, 0.50);
		}

		public LargeBagOfTreasure(Serial serial)
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
			_ = reader.ReadInt();
		}
	}

	public class RewardStrongbox : WoodenBox
	{
		[Constructable]
		public RewardStrongbox()
		{
			RewardBag.Fill(this, 5, 1.0);
		}

		public RewardStrongbox(Serial serial)
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
			_ = reader.ReadInt();
		}
	}
}