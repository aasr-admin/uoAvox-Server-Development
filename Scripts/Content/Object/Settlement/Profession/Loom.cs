namespace Server.Items
{
	public interface ILoom
	{
		int Phase { get; set; }
	}

	/// Facing South
	public class LoomSouthAddon : BaseAddon, ILoom
	{
		public override BaseAddonDeed Deed => new LoomSouthDeed();

		private int m_Phase;

		public int Phase { get => m_Phase; set => m_Phase = value; }

		[Constructable]
		public LoomSouthAddon()
		{
			AddComponent(new AddonComponent(0x1061), 0, 0, 0);
			AddComponent(new AddonComponent(0x1062), 1, 0, 0);
		}

		public LoomSouthAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_Phase);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Phase = reader.ReadInt();
						break;
					}
			}
		}
	}

	public class LoomSouthDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new LoomSouthAddon();
		public override int LabelNumber => 1044344;  // loom (south)

		[Constructable]
		public LoomSouthDeed()
		{
		}

		public LoomSouthDeed(Serial serial) : base(serial)
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
	public class LoomEastAddon : BaseAddon, ILoom
	{
		public override BaseAddonDeed Deed => new LoomEastDeed();

		private int m_Phase;

		public int Phase { get => m_Phase; set => m_Phase = value; }

		[Constructable]
		public LoomEastAddon()
		{
			AddComponent(new AddonComponent(0x1060), 0, 0, 0);
			AddComponent(new AddonComponent(0x105F), 0, 1, 0);
		}

		public LoomEastAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_Phase);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Phase = reader.ReadInt();
						break;
					}
			}
		}
	}

	public class LoomEastDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new LoomEastAddon();
		public override int LabelNumber => 1044343;  // loom (east)

		[Constructable]
		public LoomEastDeed()
		{
		}

		public LoomEastDeed(Serial serial) : base(serial)
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