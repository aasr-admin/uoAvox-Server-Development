namespace Server.Items
{
	public class FreshEggs : Item
	{
		[Constructable]
		public FreshEggs(int amount) : base(0x9B5)
		{
			Name = "Raw Eggs";
			Weight = 3.0;

			Stackable = true;
			Amount = amount;
		}

		public override void OnDoubleClick(Mobile from)
		{
			switch (Utility.Random(2))
			{
				case 1:
					{
						from.Emote("*tosses eggs*");
						from.SendAsciiMessage("These eggs look spoiled");
						Delete();
						break;
					}
				case 0:
					{
						from.AddToBackpack(new Eggs(1));
						from.AddToBackpack(new Eggshells());
						Delete();
						break;
					}
			}

			base.OnDoubleClick(from);
		}

		public FreshEggs(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	public class Eggs : CookableFood
	{
		[Constructable]
		public Eggs() : this(1)
		{
		}

		[Constructable]
		public Eggs(int amount) : base(0x9B5, 15)
		{
			Name = "Raw Eggs";
			Weight = 2.0;

			Stackable = true;
			Amount = amount;
		}

		public override Food Cook()
		{
			return new FriedEggs(1);
		}

		public Eggs(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			if (version < 1)
			{
				Stackable = true;
			}
		}
	}

	public class Eggshells : Item
	{
		[Constructable]
		public Eggshells() : base(0x9b4)
		{
			Name = "Egg Shells";
			Weight = 1.0;
		}

		public Eggshells(Serial serial) : base(serial)
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