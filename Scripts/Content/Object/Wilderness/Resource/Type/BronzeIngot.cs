﻿namespace Server.Items
{
	[FlipableAttribute(0x1BF2, 0x1BEF)]
	public class BronzeIngot : BaseIngot
	{
		[Constructable]
		public BronzeIngot() : this(1)
		{
		}

		[Constructable]
		public BronzeIngot(int amount) : base(CraftResource.Bronze, amount)
		{
		}

		public BronzeIngot(Serial serial) : base(serial)
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