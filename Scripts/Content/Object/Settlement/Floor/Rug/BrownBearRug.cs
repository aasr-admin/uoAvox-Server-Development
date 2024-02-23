namespace Server.Items
{
	/// Facing South
	public class BrownBearRugSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new BrownBearRugSouthDeed();

		[Constructable]
		public BrownBearRugSouthAddon()
		{
			AddComponent(new AddonComponent(0x1E36), 1, 1, 0);
			AddComponent(new AddonComponent(0x1E37), 0, 1, 0);
			AddComponent(new AddonComponent(0x1E38), -1, 1, 0);
			AddComponent(new AddonComponent(0x1E39), -1, 0, 0);
			AddComponent(new AddonComponent(0x1E3A), 0, 0, 0);
			AddComponent(new AddonComponent(0x1E3B), 1, 0, 0);
			AddComponent(new AddonComponent(0x1E3C), 1, -1, 0);
			AddComponent(new AddonComponent(0x1E3D), 0, -1, 0);
			AddComponent(new AddonComponent(0x1E3E), -1, -1, 0);
		}

		public BrownBearRugSouthAddon(Serial serial) : base(serial)
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

	public class BrownBearRugSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new BrownBearRugSouthAddon();
		public override int LabelNumber => 1049398;  // a brown bear rug deed facing south

		[Constructable]
		public BrownBearRugSouthDeed()
		{
		}

		public BrownBearRugSouthDeed(Serial serial) : base(serial)
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
	public class BrownBearRugEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new BrownBearRugEastDeed();

		[Constructable]
		public BrownBearRugEastAddon()
		{
			AddComponent(new AddonComponent(0x1E40), 1, 1, 0);
			AddComponent(new AddonComponent(0x1E41), 1, 0, 0);
			AddComponent(new AddonComponent(0x1E42), 1, -1, 0);
			AddComponent(new AddonComponent(0x1E43), 0, -1, 0);
			AddComponent(new AddonComponent(0x1E44), 0, 0, 0);
			AddComponent(new AddonComponent(0x1E45), 0, 1, 0);
			AddComponent(new AddonComponent(0x1E46), -1, 1, 0);
			AddComponent(new AddonComponent(0x1E47), -1, 0, 0);
			AddComponent(new AddonComponent(0x1E48), -1, -1, 0);
		}

		public BrownBearRugEastAddon(Serial serial) : base(serial)
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

	public class BrownBearRugEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new BrownBearRugEastAddon();
		public override int LabelNumber => 1049397;  // a brown bear rug deed facing east

		[Constructable]
		public BrownBearRugEastDeed()
		{
		}

		public BrownBearRugEastDeed(Serial serial) : base(serial)
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