namespace Server.Items
{
	/// Facing South
	public class SquareGozaMatSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new SquareGozaMatSouthDeed();

		public override bool RetainDeedHue => true;

		[Constructable]
		public SquareGozaMatSouthAddon() : this(0)
		{
		}

		[Constructable]
		public SquareGozaMatSouthAddon(int hue)
		{
			AddComponent(new LocalizedAddonComponent(0x28a9, 1030688), 0, 0, 0);
			Hue = hue;
		}

		public SquareGozaMatSouthAddon(Serial serial) : base(serial)
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

	public class SquareGozaMatSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new SquareGozaMatSouthAddon(Hue);
		public override int LabelNumber => 1030406;  // square goza (south)


		[Constructable]
		public SquareGozaMatSouthDeed()
		{
		}

		public SquareGozaMatSouthDeed(Serial serial) : base(serial)
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

	/// Facing East
	public class SquareGozaMatEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new SquareGozaMatEastDeed();
		public override int LabelNumber => 1030688;  // goza mat

		public override bool RetainDeedHue => true;

		[Constructable]
		public SquareGozaMatEastAddon() : this(0)
		{
		}

		[Constructable]
		public SquareGozaMatEastAddon(int hue)
		{
			AddComponent(new LocalizedAddonComponent(0x28a8, 1030688), 0, 0, 0);
			Hue = hue;
		}

		public SquareGozaMatEastAddon(Serial serial) : base(serial)
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

	public class SquareGozaMatEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new SquareGozaMatEastAddon(Hue);
		public override int LabelNumber => 1030407;  // square goza (east)

		[Constructable]
		public SquareGozaMatEastDeed()
		{
		}

		public SquareGozaMatEastDeed(Serial serial) : base(serial)
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