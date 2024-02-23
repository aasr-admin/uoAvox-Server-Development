using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

using System;

namespace Server.Items
{
	public class MinerCamp : BaseAddon
	{
		[Constructable]
		public MinerCamp()
		{
			// Campfire
			AddComponent(new AddonComponent(0xDE3), -2, 1, 0);

			// Mining Cart
			AddComponent(new AddonComponent(0x1A8B), -1, 3, 0);
			AddComponent(new AddonComponent(0x1A88), 0, 3, 0);
			AddComponent(new AddonComponent(0x1A87), 1, 3, 0);
		}

		public MinerCamp(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}