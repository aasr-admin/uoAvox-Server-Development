﻿namespace Server.Items
{
	public class ShadowIronGranite : BaseGranite
	{
		[Constructable]
		public ShadowIronGranite() : base(CraftResource.ShadowIron)
		{
		}

		public ShadowIronGranite(Serial serial) : base(serial)
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