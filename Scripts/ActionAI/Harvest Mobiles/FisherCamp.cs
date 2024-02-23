using Server;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

using System;

namespace Server.Items
{
	public class FisherCamp : BaseAddon
	{
		[Constructable]
		public FisherCamp()
		{
			/// Water Barrel
			AddComponent(new AddonComponent(0x154D), 0, 1, 0);

			/// Large Log
			AddComponent(new AddonComponent(0x1E19), -2, 3, 0);
			AddComponent(new AddonComponent(0x1E1B), -1, 3, 0);
			AddComponent(new AddonComponent(0x1E17), 1, 3, 0);
		}

		public FisherCamp(Serial serial) : base(serial)
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