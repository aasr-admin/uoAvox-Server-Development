namespace Server.Items
{
	/// Facing South
	public class GozaMatSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new GozaMatSouthDeed();

		public override bool RetainDeedHue => true;

		[Constructable]
		public GozaMatSouthAddon() : this(0)
		{
		}

		[Constructable]
		public GozaMatSouthAddon(int hue)
		{
			AddComponent(new LocalizedAddonComponent(0x28a6, 1030688), 0, 1, 0);
			AddComponent(new LocalizedAddonComponent(0x28a7, 1030688), 0, 0, 0);
			Hue = hue;
		}

		public GozaMatSouthAddon(Serial serial) : base(serial)
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

	public class GozaMatSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new GozaMatSouthAddon(Hue);
		public override int LabelNumber => 1030405;  // goza (south)

		[Constructable]
		public GozaMatSouthDeed()
		{
		}

		public GozaMatSouthDeed(Serial serial) : base(serial)
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
	public class GozaMatEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new GozaMatEastDeed();

		public override bool RetainDeedHue => true;

		[Constructable]
		public GozaMatEastAddon() : this(0)
		{
		}

		[Constructable]
		public GozaMatEastAddon(int hue)
		{
			AddComponent(new LocalizedAddonComponent(0x28a4, 1030688), 1, 0, 0);
			AddComponent(new LocalizedAddonComponent(0x28a5, 1030688), 0, 0, 0);
			Hue = hue;
		}

		public GozaMatEastAddon(Serial serial) : base(serial)
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

	public class GozaMatEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new GozaMatEastAddon(Hue);
		public override int LabelNumber => 1030404;  // goza (east)

		[Constructable]
		public GozaMatEastDeed()
		{
		}

		public GozaMatEastDeed(Serial serial) : base(serial)
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