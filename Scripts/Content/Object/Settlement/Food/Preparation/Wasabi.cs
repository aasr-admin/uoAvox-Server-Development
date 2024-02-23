namespace Server.Items
{
	public class Wasabi : Item
	{
		[Constructable]
		public Wasabi() : base(0x24E8)
		{
			Weight = 1.0;
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.AddToBackpack(new WasabiClumps(2));

			Delete();

			base.OnDoubleClick(from);
		}

		public Wasabi(Serial serial) : base(serial)
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

	public class WasabiClumps : Food
	{
		[Constructable]
		public WasabiClumps() : this(2)
		{
		}

		[Constructable]
		public WasabiClumps(int fillFactor) : base(0x24EB)
		{
			Stackable = false;
			Weight = 1.0;
			FillFactor = fillFactor;
		}

		public WasabiClumps(Serial serial) : base(serial)
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