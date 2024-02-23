namespace Server.Items
{
	/// Facing South
	public class SmallStretchedHideSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new SmallStretchedHideSouthDeed();

		[Constructable]
		public SmallStretchedHideSouthAddon()
		{
			AddComponent(new AddonComponent(0x107A), 0, 0, 0);
		}

		public SmallStretchedHideSouthAddon(Serial serial) : base(serial)
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

	public class SmallStretchedHideSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new SmallStretchedHideSouthAddon();
		public override int LabelNumber => 1049402;  // a small stretched hide deed facing south

		[Constructable]
		public SmallStretchedHideSouthDeed()
		{
		}

		public SmallStretchedHideSouthDeed(Serial serial) : base(serial)
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
	public class SmallStretchedHideEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new SmallStretchedHideEastDeed();

		[Constructable]
		public SmallStretchedHideEastAddon()
		{
			AddComponent(new AddonComponent(0x1069), 0, 0, 0);
		}

		public SmallStretchedHideEastAddon(Serial serial) : base(serial)
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

	public class SmallStretchedHideEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new SmallStretchedHideEastAddon();
		public override int LabelNumber => 1049401;  // a small stretched hide deed facing east

		[Constructable]
		public SmallStretchedHideEastDeed()
		{
		}

		public SmallStretchedHideEastDeed(Serial serial) : base(serial)
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