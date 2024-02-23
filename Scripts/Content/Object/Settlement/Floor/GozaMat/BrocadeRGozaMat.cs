namespace Server.Items
{
	/// Facing South
	public class BrocadeGozaMatSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new BrocadeGozaMatSouthDeed();

		public override bool RetainDeedHue => true;

		[Constructable]
		public BrocadeGozaMatSouthAddon() : this(0)
		{
		}

		[Constructable]
		public BrocadeGozaMatSouthAddon(int hue)
		{
			AddComponent(new LocalizedAddonComponent(0x28AD, 1030688), 0, 0, 0);
			AddComponent(new LocalizedAddonComponent(0x28AC, 1030688), 0, 1, 0);
			Hue = hue;
		}

		public BrocadeGozaMatSouthAddon(Serial serial) : base(serial)
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

	public class BrocadeGozaMatSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new BrocadeGozaMatSouthAddon(Hue);
		public override int LabelNumber => 1030409;  // brocade goza (south)

		[Constructable]
		public BrocadeGozaMatSouthDeed()
		{
		}

		public BrocadeGozaMatSouthDeed(Serial serial) : base(serial)
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
	public class BrocadeGozaMatEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new BrocadeGozaMatEastDeed();

		public override bool RetainDeedHue => true;

		[Constructable]
		public BrocadeGozaMatEastAddon() : this(0)
		{
		}

		[Constructable]
		public BrocadeGozaMatEastAddon(int hue)
		{
			AddComponent(new LocalizedAddonComponent(0x28AB, 1030688), 0, 0, 0);
			AddComponent(new LocalizedAddonComponent(0x28AA, 1030688), 1, 0, 0);
			Hue = hue;
		}

		public BrocadeGozaMatEastAddon(Serial serial) : base(serial)
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

	public class BrocadeGozaMatEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new BrocadeGozaMatEastAddon(Hue);
		public override int LabelNumber => 1030408;  // brocade goza (east)

		[Constructable]
		public BrocadeGozaMatEastDeed()
		{
		}

		public BrocadeGozaMatEastDeed(Serial serial) : base(serial)
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