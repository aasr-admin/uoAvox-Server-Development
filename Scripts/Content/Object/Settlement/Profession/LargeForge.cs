namespace Server.Items
{
	/// Facing South
	public class LargeForgeSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new LargeForgeSouthDeed();

		[Constructable]
		public LargeForgeSouthAddon()
		{
			AddComponent(new ForgeComponent(0x197A), 0, 0, 0);
			AddComponent(new ForgeComponent(0x197E), 1, 0, 0);
			AddComponent(new ForgeComponent(0x19A2), 2, 0, 0);
			AddComponent(new ForgeComponent(0x199E), 3, 0, 0);
		}

		public LargeForgeSouthAddon(Serial serial) : base(serial)
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

	public class LargeForgeSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new LargeForgeSouthAddon();
		public override int LabelNumber => 1044332;  // large forge (south)

		[Constructable]
		public LargeForgeSouthDeed()
		{
		}

		public LargeForgeSouthDeed(Serial serial) : base(serial)
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
	public class LargeForgeEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new LargeForgeEastDeed();

		[Constructable]
		public LargeForgeEastAddon()
		{
			AddComponent(new ForgeComponent(0x1986), 0, 0, 0);
			AddComponent(new ForgeComponent(0x198A), 0, 1, 0);
			AddComponent(new ForgeComponent(0x1996), 0, 2, 0);
			AddComponent(new ForgeComponent(0x1992), 0, 3, 0);
		}

		public LargeForgeEastAddon(Serial serial) : base(serial)
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

	public class LargeForgeEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new LargeForgeEastAddon();
		public override int LabelNumber => 1044331;  // large forge (east)

		[Constructable]
		public LargeForgeEastDeed()
		{
		}

		public LargeForgeEastDeed(Serial serial) : base(serial)
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