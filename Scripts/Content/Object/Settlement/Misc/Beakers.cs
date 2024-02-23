namespace Server.Items
{
	/// YellowBeaker
	public class YellowBeaker : Item
	{
		[Constructable]
		public YellowBeaker() : base(0x182E)
		{
			Weight = 1.0;
			Movable = true;
		}

		public YellowBeaker(Serial serial) : base(serial)
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

	/// RedBeaker
	public class RedBeaker : Item
	{
		[Constructable]
		public RedBeaker() : base(0x182F)
		{
			Weight = 1.0;
			Movable = true;
		}

		public RedBeaker(Serial serial) : base(serial)
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

	/// BlueBeaker
	public class BlueBeaker : Item
	{
		[Constructable]
		public BlueBeaker() : base(0x1830)
		{
			Weight = 1.0;
			Movable = true;
		}

		public BlueBeaker(Serial serial) : base(serial)
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

	/// GreenBeaker
	public class GreenBeaker : Item
	{
		[Constructable]
		public GreenBeaker() : base(0x1831)
		{
			Weight = 1.0;
			Movable = true;
		}

		public GreenBeaker(Serial serial) : base(serial)
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