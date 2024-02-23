namespace Server.Items
{
	/// Facing South
	public class StoneOvenSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new StoneOvenSouthDeed();

		[Constructable]
		public StoneOvenSouthAddon()
		{
			AddComponent(new AddonComponent(0x931), -1, 0, 0);
			AddComponent(new AddonComponent(0x930), 0, 0, 0);
		}

		public StoneOvenSouthAddon(Serial serial) : base(serial)
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

	public class StoneOvenSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new StoneOvenSouthAddon();
		public override int LabelNumber => 1044346;  // stone oven (south)

		[Constructable]
		public StoneOvenSouthDeed()
		{
		}

		public StoneOvenSouthDeed(Serial serial) : base(serial)
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
	public class StoneOvenEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new StoneOvenEastDeed();

		[Constructable]
		public StoneOvenEastAddon()
		{
			AddComponent(new AddonComponent(0x92C), 0, 0, 0);
			AddComponent(new AddonComponent(0x92B), 0, 1, 0);
		}

		public StoneOvenEastAddon(Serial serial) : base(serial)
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

	public class StoneOvenEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new StoneOvenEastAddon();
		public override int LabelNumber => 1044345;  // stone oven (east)

		[Constructable]
		public StoneOvenEastDeed()
		{
		}

		public StoneOvenEastDeed(Serial serial) : base(serial)
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