using Server.Network;

namespace Server.Items
{
	/// MountedPixie: Blue
	[Flipable(0x2A75, 0x2A76)]
	public class MountedPixieBlueComponent : AddonComponent
	{
		public override int LabelNumber => 1074482;  // Mounted pixie

		public MountedPixieBlueComponent() : base(0x2A75)
		{
		}

		public MountedPixieBlueComponent(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Utility.InRange(Location, from.Location, 2))
			{
				Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x55C, 0x55E));
			}
			else
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieBlueAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new MountedPixieBlueDeed();

		public MountedPixieBlueAddon() : base()
		{
			AddComponent(new MountedPixieBlueComponent(), 0, 0, 0);
		}

		public MountedPixieBlueAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieBlueDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new MountedPixieBlueAddon();
		public override int LabelNumber => 1074482;  // Mounted pixie

		[Constructable]
		public MountedPixieBlueDeed() : base()
		{
			LootType = LootType.Blessed;
		}

		public MountedPixieBlueDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// MountedPixie: Green
	[Flipable(0x2A71, 0x2A72)]
	public class MountedPixieGreenComponent : AddonComponent
	{
		public override int LabelNumber => 1074482;  // Mounted pixie

		public MountedPixieGreenComponent() : base(0x2A71)
		{
		}

		public MountedPixieGreenComponent(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Utility.InRange(Location, from.Location, 2))
			{
				Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x554, 0x557));
			}
			else
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieGreenAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new MountedPixieGreenDeed();

		public MountedPixieGreenAddon() : base()
		{
			AddComponent(new MountedPixieGreenComponent(), 0, 0, 0);
		}

		public MountedPixieGreenAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieGreenDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new MountedPixieGreenAddon();
		public override int LabelNumber => 1074482;  // Mounted pixie

		[Constructable]
		public MountedPixieGreenDeed() : base()
		{
			LootType = LootType.Blessed;
		}

		public MountedPixieGreenDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// MountedPixie: Lime
	[Flipable(0x2A77, 0x2A78)]
	public class MountedPixieLimeComponent : AddonComponent
	{
		public override int LabelNumber => 1074482;  // Mounted pixie

		public MountedPixieLimeComponent() : base(0x2A77)
		{
		}

		public MountedPixieLimeComponent(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Utility.InRange(Location, from.Location, 2))
			{
				Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x55F, 0x561));
			}
			else
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieLimeAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new MountedPixieLimeDeed();

		public MountedPixieLimeAddon() : base()
		{
			AddComponent(new MountedPixieLimeComponent(), 0, 0, 0);
		}

		public MountedPixieLimeAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieLimeDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new MountedPixieLimeAddon();
		public override int LabelNumber => 1074482;  // Mounted pixie

		[Constructable]
		public MountedPixieLimeDeed() : base()
		{
			LootType = LootType.Blessed;
		}

		public MountedPixieLimeDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// MountedPixie: Orange
	[Flipable(0x2A73, 0x2A74)]
	public class MountedPixieOrangeComponent : AddonComponent
	{
		public override int LabelNumber => 1074482;  // Mounted pixie

		public MountedPixieOrangeComponent() : base(0x2A73)
		{
		}

		public MountedPixieOrangeComponent(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Utility.InRange(Location, from.Location, 2))
			{
				Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x558, 0x55B));
			}
			else
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieOrangeAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new MountedPixieOrangeDeed();

		public MountedPixieOrangeAddon() : base()
		{
			AddComponent(new MountedPixieOrangeComponent(), 0, 0, 0);
		}

		public MountedPixieOrangeAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieOrangeDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new MountedPixieOrangeAddon();
		public override int LabelNumber => 1074482;  // Mounted pixie

		[Constructable]
		public MountedPixieOrangeDeed() : base()
		{
			LootType = LootType.Blessed;
		}

		public MountedPixieOrangeDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// MountedPixie: White
	[Flipable(0x2A79, 0x2A7A)]
	public class MountedPixieWhiteComponent : AddonComponent
	{
		public override int LabelNumber => 1074482;  // Mounted pixie

		public MountedPixieWhiteComponent() : base(0x2A79)
		{
		}

		public MountedPixieWhiteComponent(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Utility.InRange(Location, from.Location, 2))
			{
				Effects.PlaySound(Location, Map, Utility.RandomMinMax(0x562, 0x564));
			}
			else
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieWhiteAddon : BaseAddon
	{
		public override BaseAddonDeed Deed => new MountedPixieWhiteDeed();

		public MountedPixieWhiteAddon() : base()
		{
			AddComponent(new MountedPixieWhiteComponent(), 0, 0, 0);
		}

		public MountedPixieWhiteAddon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class MountedPixieWhiteDeed : BaseAddonDeed
	{
		public override BaseAddon Addon => new MountedPixieWhiteAddon();
		public override int LabelNumber => 1074482;  // Mounted pixie

		[Constructable]
		public MountedPixieWhiteDeed() : base()
		{
			LootType = LootType.Blessed;
		}

		public MountedPixieWhiteDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}
}