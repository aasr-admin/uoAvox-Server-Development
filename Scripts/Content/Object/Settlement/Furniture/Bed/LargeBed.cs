﻿namespace Server.Items
{
	/// Facing South
	public class LargeBedSouthAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new LargeBedSouthDeed();

		[Constructable]
		public LargeBedSouthAddon()
		{
			AddComponent(new AddonComponent(0xA83), 0, 0, 0);
			AddComponent(new AddonComponent(0xA7F), 0, 1, 0);
			AddComponent(new AddonComponent(0xA82), 1, 0, 0);
			AddComponent(new AddonComponent(0xA7E), 1, 1, 0);
		}

		public LargeBedSouthAddon(Serial serial) : base(serial)
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

	public class LargeBedSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new LargeBedSouthAddon();
		public override int LabelNumber => 1044323;  // large bed (south)

		[Constructable]
		public LargeBedSouthDeed()
		{
		}

		public LargeBedSouthDeed(Serial serial) : base(serial)
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
	public class LargeBedEastAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new LargeBedEastDeed();

		[Constructable]
		public LargeBedEastAddon()
		{
			AddComponent(new AddonComponent(0xA7D), 0, 0, 0);
			AddComponent(new AddonComponent(0xA7C), 0, 1, 0);
			AddComponent(new AddonComponent(0xA79), 1, 0, 0);
			AddComponent(new AddonComponent(0xA78), 1, 1, 0);
		}

		public LargeBedEastAddon(Serial serial) : base(serial)
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

	public class LargeBedEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new LargeBedEastAddon();
		public override int LabelNumber => 1044324;  // large bed (east)

		[Constructable]
		public LargeBedEastDeed()
		{
		}

		public LargeBedEastDeed(Serial serial) : base(serial)
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