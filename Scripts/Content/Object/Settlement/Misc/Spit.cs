namespace Server.Items
{
	/// RabbitOnASpit
	public class RabbitOnASpitSouth : Item
	{
		[Constructable]
		public RabbitOnASpitSouth() : base(0x1E95)
		{
		}

		public RabbitOnASpitSouth(Serial serial) : base(serial)
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
	} // Facing: South

	public class RabbitOnASpitEast : Item
	{
		[Constructable]
		public RabbitOnASpitEast() : base(0x1E98)
		{
		}

		public RabbitOnASpitEast(Serial serial) : base(serial)
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
	} // Facint East


	/// ChickenOnASpit
	public class ChickenOnASpitSouth : Item
	{
		[Constructable]
		public ChickenOnASpitSouth() : base(0x1E94)
		{
		}

		public ChickenOnASpitSouth(Serial serial) : base(serial)
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
	} // Facing: South

	public class ChickenOnASpitEast : Item
	{
		[Constructable]
		public ChickenOnASpitEast() : base(0x1E97)
		{
		}

		public ChickenOnASpitEast(Serial serial) : base(serial)
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
	} // Facint East
}