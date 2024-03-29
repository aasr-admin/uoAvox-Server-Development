﻿namespace Server.Items
{
	[FlipableAttribute(0x1BF2, 0x1BEF)]
	public class CopperIngot : BaseIngot
	{
		[Constructable]
		public CopperIngot() : this(1)
		{
		}

		[Constructable]
		public CopperIngot(int amount) : base(CraftResource.Copper, amount)
		{
		}

		public CopperIngot(Serial serial) : base(serial)
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