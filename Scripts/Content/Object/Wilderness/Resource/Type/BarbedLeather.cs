﻿namespace Server.Items
{
	[FlipableAttribute(0x1081, 0x1082)]
	public class BarbedLeather : BaseLeather
	{
		[Constructable]
		public BarbedLeather() : this(1)
		{
		}

		[Constructable]
		public BarbedLeather(int amount) : base(CraftResource.BarbedLeather, amount)
		{
		}

		public BarbedLeather(Serial serial) : base(serial)
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