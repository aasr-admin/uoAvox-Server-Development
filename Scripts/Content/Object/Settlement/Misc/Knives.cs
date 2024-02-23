namespace Server.Items
{
	[Flipable(0x9F6, 0x9F7, 0x9A5, 0x9A6)]
	public class Knife : Item
	{
		[Constructable]
		public Knife() : base(0x9F6)
		{
			Weight = 1.0;
		}

		public Knife(Serial serial) : base(serial)
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

	public class KnifeLeft : Item
	{
		[Constructable]
		public KnifeLeft() : base(0x9F6)
		{
			Weight = 1.0;
		}

		public KnifeLeft(Serial serial) : base(serial)
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

	public class KnifeRight : Item
	{
		[Constructable]
		public KnifeRight() : base(0x9F7)
		{
			Weight = 1.0;
		}

		public KnifeRight(Serial serial) : base(serial)
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