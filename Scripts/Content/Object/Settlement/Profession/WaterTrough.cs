namespace Server.Items
{
	/// Facing South
	public class WaterTroughSouthAddon : BaseAddon, IWaterSource
	{
		public override BaseAddonDeed Deed => new WaterTroughSouthDeed();

		[Constructable]
		public WaterTroughSouthAddon()
		{
			AddComponent(new AddonComponent(0xB43), 0, 0, 0);
			AddComponent(new AddonComponent(0xB44), 1, 0, 0);
		}

		public WaterTroughSouthAddon(Serial serial) : base(serial)
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

		public int Quantity
		{
			get => 500;
			set { }
		}
	}

	public class WaterTroughSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new WaterTroughSouthAddon();
		public override int LabelNumber => 1044350;  // water trough (south)

		[Constructable]
		public WaterTroughSouthDeed()
		{
		}

		public WaterTroughSouthDeed(Serial serial) : base(serial)
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
	public class WaterTroughEastAddon : BaseAddon, IWaterSource
	{
		public override BaseAddonDeed Deed => new WaterTroughEastDeed();

		[Constructable]
		public WaterTroughEastAddon()
		{
			AddComponent(new AddonComponent(0xB41), 0, 0, 0);
			AddComponent(new AddonComponent(0xB42), 0, 1, 0);
		}

		public WaterTroughEastAddon(Serial serial) : base(serial)
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

		public int Quantity
		{
			get => 500;
			set { }
		}
	}

	public class WaterTroughEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new WaterTroughEastAddon();
		public override int LabelNumber => 1044349;  // water trough (east)

		[Constructable]
		public WaterTroughEastDeed()
		{
		}

		public WaterTroughEastDeed(Serial serial) : base(serial)
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