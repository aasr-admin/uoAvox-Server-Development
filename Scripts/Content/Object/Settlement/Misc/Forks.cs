namespace Server.Items
{
	[Flipable(0x9F4, 0x9F5, 0x9A3, 0x9A4)]
	public class Fork : Item
	{
		[Constructable]
		public Fork() : base(0x9F4)
		{
			Weight = 1.0;
		}

		public Fork(Serial serial) : base(serial)
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

	public class ForkLeft : Item
	{
		[Constructable]
		public ForkLeft() : base(0x9F4)
		{
			Weight = 1.0;
		}

		public ForkLeft(Serial serial) : base(serial)
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

	public class ForkRight : Item
	{
		[Constructable]
		public ForkRight() : base(0x9F5)
		{
			Weight = 1.0;
		}

		public ForkRight(Serial serial) : base(serial)
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