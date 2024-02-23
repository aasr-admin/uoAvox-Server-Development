namespace Server.Items
{
	public class Fish : Item, ICarvable
	{
		public void Carve(Mobile from, Item item)
		{
			base.ScissorHelper(from, new RawFreshwaterFishSteak(), 4);
		}

		[Constructable]
		public Fish() : this(1)
		{
		}

		[Constructable]
		public Fish(int amount) : base(Utility.Random(0x09CC, 4))
		{
			Name = "Fish";
			Weight = 1.0;
			Amount = amount;

			Stackable = true;
		}

		public Fish(Serial serial) : base(serial)
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
