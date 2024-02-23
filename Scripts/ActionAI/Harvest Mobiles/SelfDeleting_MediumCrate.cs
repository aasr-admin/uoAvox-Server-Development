using Server;
using Server.ContextMenus;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Multis
{
    [Furniture]
	[Flipable(0xE3F, 0xE3E)]
	public class SelfDeleting_MediumCrate : MediumCrate
	{
		[Constructable]
		public SelfDeleting_MediumCrate() : base()
		{
			Weight = 2.0;
			LiftOverride = true;

			Timer.DelayCall(TimeSpan.FromMinutes(10.0), SelfDelete);
		}

		private void SelfDelete()
		{
			Delete();
		}

		public SelfDeleting_MediumCrate(Serial serial) : base(serial)
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

			if (Weight == 6.0)
				Weight = 2.0;
		}
	}
}