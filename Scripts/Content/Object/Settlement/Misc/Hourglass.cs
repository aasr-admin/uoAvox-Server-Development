namespace Server.Items
{
	/// Hourglass
	public class Hourglass : Item
	{
		[Constructable]
		public Hourglass() : base(0x1810)
		{
			Weight = 1.0;
			Movable = true;
		}

		public Hourglass(Serial serial) : base(serial)
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

	/// HourglassAni
	public class HourglassAni : Item
	{
		[Constructable]
		public HourglassAni() : base(0x1811)
		{
			Weight = 1.0;
			Movable = true;
		}

		public HourglassAni(Serial serial) : base(serial)
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

	/// SpinningHourglass
	[Flipable(0x1810, 0x1811)]
	public class SpinningHourglass : Item
	{
		[Constructable]
		public SpinningHourglass() : base(0x1810)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SpinningHourglass(Serial serial) : base(serial)
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