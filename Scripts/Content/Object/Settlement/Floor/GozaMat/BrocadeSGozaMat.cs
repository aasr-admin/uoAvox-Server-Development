namespace Server.Items
{
	/// Facing South
	public class BrocadeSquareGozaMatSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new BrocadeSquareGozaMatSouthDeed();

		public override bool RetainDeedHue => true;

		[Constructable]
		public BrocadeSquareGozaMatSouthAddon() : this(0)
		{
		}

		[Constructable]
		public BrocadeSquareGozaMatSouthAddon(int hue)
		{
			AddComponent(new LocalizedAddonComponent(0x28AF, 1030688), 0, 0, 0);
			Hue = hue;
		}

		public BrocadeSquareGozaMatSouthAddon(Serial serial) : base(serial)
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

	public class BrocadeSquareGozaMatSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new BrocadeSquareGozaMatSouthAddon(Hue);
		public override int LabelNumber => 1030410;  // brocade square goza (south)


		[Constructable]
		public BrocadeSquareGozaMatSouthDeed()
		{
		}

		public BrocadeSquareGozaMatSouthDeed(Serial serial) : base(serial)
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
	public class BrocadeSquareGozaMatEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new BrocadeSquareGozaMatEastDeed();

		public override bool RetainDeedHue => true;

		[Constructable]
		public BrocadeSquareGozaMatEastAddon() : this(0)
		{
		}

		[Constructable]
		public BrocadeSquareGozaMatEastAddon(int hue)
		{
			AddComponent(new LocalizedAddonComponent(0x28AE, 1030688), 0, 0, 0);
			Hue = hue;
		}

		public BrocadeSquareGozaMatEastAddon(Serial serial) : base(serial)
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

	public class BrocadeSquareGozaMatEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new BrocadeSquareGozaMatEastAddon(Hue);
		public override int LabelNumber => 1030411;  // brocade square goza (east)

		[Constructable]
		public BrocadeSquareGozaMatEastDeed()
		{
		}

		public BrocadeSquareGozaMatEastDeed(Serial serial) : base(serial)
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