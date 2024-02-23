namespace Server.Items
{
	/// Facing South
	public class MediumStretchedHideSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new MediumStretchedHideSouthDeed();

		[Constructable]
		public MediumStretchedHideSouthAddon()
		{
			AddComponent(new AddonComponent(0x107C), 0, 0, 0);
		}

		public MediumStretchedHideSouthAddon(Serial serial) : base(serial)
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

	public class MediumStretchedHideSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new MediumStretchedHideSouthAddon();
		public override int LabelNumber => 1049404;  // a medium stretched hide deed facing south

		[Constructable]
		public MediumStretchedHideSouthDeed()
		{
		}

		public MediumStretchedHideSouthDeed(Serial serial) : base(serial)
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
	public class MediumStretchedHideEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new MediumStretchedHideEastDeed();

		[Constructable]
		public MediumStretchedHideEastAddon()
		{
			AddComponent(new AddonComponent(0x106B), 0, 0, 0);
		}

		public MediumStretchedHideEastAddon(Serial serial) : base(serial)
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

	public class MediumStretchedHideEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new MediumStretchedHideEastAddon();
		public override int LabelNumber => 1049403;  // a medium stretched hide deed facing east

		[Constructable]
		public MediumStretchedHideEastDeed()
		{
		}

		public MediumStretchedHideEastDeed(Serial serial) : base(serial)
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