namespace Server.Items
{
	/// Facing South
	public class PolarBearRugSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new PolarBearRugSouthDeed();

		[Constructable]
		public PolarBearRugSouthAddon()
		{
			AddComponent(new AddonComponent(0x1E49), 1, 1, 0);
			AddComponent(new AddonComponent(0x1E4A), 0, 1, 0);
			AddComponent(new AddonComponent(0x1E4B), -1, 1, 0);
			AddComponent(new AddonComponent(0x1E4C), -1, 0, 0);
			AddComponent(new AddonComponent(0x1E4D), 0, 0, 0);
			AddComponent(new AddonComponent(0x1E4E), 1, 0, 0);
			AddComponent(new AddonComponent(0x1E4F), 1, -1, 0);
			AddComponent(new AddonComponent(0x1E50), 0, -1, 0);
			AddComponent(new AddonComponent(0x1E51), -1, -1, 0);
		}

		public PolarBearRugSouthAddon(Serial serial) : base(serial)
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

	public class PolarBearRugSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new PolarBearRugSouthAddon();
		public override int LabelNumber => 1049400;  // a polar bear rug deed facing south

		[Constructable]
		public PolarBearRugSouthDeed()
		{
		}

		public PolarBearRugSouthDeed(Serial serial) : base(serial)
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
	public class PolarBearRugEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new PolarBearRugEastDeed();

		[Constructable]
		public PolarBearRugEastAddon()
		{
			AddComponent(new AddonComponent(0x1E53), 1, 1, 0);
			AddComponent(new AddonComponent(0x1E54), 1, 0, 0);
			AddComponent(new AddonComponent(0x1E55), 1, -1, 0);
			AddComponent(new AddonComponent(0x1E56), 0, -1, 0);
			AddComponent(new AddonComponent(0x1E57), 0, 0, 0);
			AddComponent(new AddonComponent(0x1E58), 0, 1, 0);
			AddComponent(new AddonComponent(0x1E59), -1, 1, 0);
			AddComponent(new AddonComponent(0x1E5A), -1, 0, 0);
			AddComponent(new AddonComponent(0x1E5B), -1, -1, 0);
		}

		public PolarBearRugEastAddon(Serial serial) : base(serial)
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

	public class PolarBearRugEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new PolarBearRugEastAddon();
		public override int LabelNumber => 1049399;  // a polar bear rug deed facing east

		[Constructable]
		public PolarBearRugEastDeed()
		{
		}

		public PolarBearRugEastDeed(Serial serial) : base(serial)
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