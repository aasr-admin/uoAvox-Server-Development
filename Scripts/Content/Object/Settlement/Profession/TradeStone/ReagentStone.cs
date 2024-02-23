namespace Server.Items
{
	public class ReagentStone : Item
	{
		public override string DefaultName => "a reagent stone";

		[Constructable]
		public ReagentStone() : base(0xED4)
		{
			Movable = false;
			Hue = 0x2D1;
		}

		public override void OnDoubleClick(Mobile from)
		{
			switch (Utility.Random(3))
			{
				case 2:
					{
						var regBag3 = new BagOfAllReagents(50);

						if (!from.AddToBackpack(regBag3))
						{
							regBag3.Delete();
						}

						break;
					}
				case 1:
					{
						var regBag2 = new BagOfNecroReagents(50);

						if (!from.AddToBackpack(regBag2))
						{
							regBag2.Delete();
						}

						break;
					}
				case 0:
					{
						var regBag1 = new BagOfReagents(50);

						if (!from.AddToBackpack(regBag1))
						{
							regBag1.Delete();
						}

						break;
					}
			}
		}

		public ReagentStone(Serial serial) : base(serial)
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

	public class BagOfReagents : Bag
	{
		[Constructable]
		public BagOfReagents() : this(50)
		{
		}

		[Constructable]
		public BagOfReagents(int amount)
		{
			DropItem(new BlackPearl(amount));
			DropItem(new Bloodmoss(amount));
			DropItem(new Garlic(amount));
			DropItem(new Ginseng(amount));
			DropItem(new MandrakeRoot(amount));
			DropItem(new Nightshade(amount));
			DropItem(new SulfurousAsh(amount));
			DropItem(new SpidersSilk(amount));
		}

		public BagOfReagents(Serial serial) : base(serial)
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

	public class BagOfNecroReagents : Bag
	{
		[Constructable]
		public BagOfNecroReagents() : this(50)
		{
		}

		[Constructable]
		public BagOfNecroReagents(int amount)
		{
			DropItem(new BatWing(amount));
			DropItem(new GraveDust(amount));
			DropItem(new DaemonBlood(amount));
			DropItem(new NoxCrystal(amount));
			DropItem(new PigIron(amount));
		}

		public BagOfNecroReagents(Serial serial) : base(serial)
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

	public class BagOfAllReagents : Bag
	{
		[Constructable]
		public BagOfAllReagents() : this(50)
		{
		}

		[Constructable]
		public BagOfAllReagents(int amount)
		{
			DropItem(new BlackPearl(amount));
			DropItem(new Bloodmoss(amount));
			DropItem(new Garlic(amount));
			DropItem(new Ginseng(amount));
			DropItem(new MandrakeRoot(amount));
			DropItem(new Nightshade(amount));
			DropItem(new SulfurousAsh(amount));
			DropItem(new SpidersSilk(amount));
			DropItem(new BatWing(amount));
			DropItem(new GraveDust(amount));
			DropItem(new DaemonBlood(amount));
			DropItem(new NoxCrystal(amount));
			DropItem(new PigIron(amount));
		}

		public BagOfAllReagents(Serial serial) : base(serial)
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