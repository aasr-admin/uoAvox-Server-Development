namespace Server.Items
{
	[Flipable(0x9F8, 0x9F9, 0x9C2, 0x9C3)]
	public class Spoon : Item
	{
		[Constructable]
		public Spoon() : base(0x9F8)
		{
			Weight = 1.0;
		}

		public Spoon(Serial serial) : base(serial)
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

	public class SpoonLeft : Item
	{
		[Constructable]
		public SpoonLeft() : base(0x9F8)
		{
			Weight = 1.0;
		}

		public SpoonLeft(Serial serial) : base(serial)
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

	public class SpoonRight : Item
	{
		[Constructable]
		public SpoonRight() : base(0x9F9)
		{
			Weight = 1.0;
		}

		public SpoonRight(Serial serial) : base(serial)
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