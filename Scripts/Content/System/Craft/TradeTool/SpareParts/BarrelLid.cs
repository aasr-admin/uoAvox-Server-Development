﻿namespace Server.Items
{
	public class BarrelLid : Item
	{
		[Constructable]
		public BarrelLid() : base(0x1DB8)
		{
			Weight = 2;
		}

		public BarrelLid(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}
}