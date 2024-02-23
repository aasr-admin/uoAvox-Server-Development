namespace Server.Items
{
	public class CheeseWheel : Food
	{
		public override double DefaultWeight => 0.1;

		[Constructable]
		public CheeseWheel() : this(1)
		{
		}

		[Constructable]
		public CheeseWheel(int amount) : base(amount, 0x97E)
		{
			Name = "Cheese Wheel";
			FillFactor = 3;
		}

		public CheeseWheel(Serial serial) : base(serial)
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

	public class CheeseWheelItem : Item
	{
		[Constructable]
		public CheeseWheelItem() : base(0x97E)
		{
			Name = "Cheese Wheel";
			Weight = 3.0;
		}

		public override void OnDoubleClick(Mobile from)
		{
			switch (Utility.Random(2))
			{
				case 1:
					{
						from.SendAsciiMessage("You create a wedge from the block of cheese");
						from.AddToBackpack(new CheeseWedge());
						Consume();
						break;
					}
				case 0:
					{
						from.SendAsciiMessage("You create a wedge from the block of cheese");
						from.AddToBackpack(new CheeseWedgeItem());
						Consume();
						break;
					}
			}

			base.OnDoubleClick(from);
		}

		public CheeseWheelItem(Serial serial) : base(serial)
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