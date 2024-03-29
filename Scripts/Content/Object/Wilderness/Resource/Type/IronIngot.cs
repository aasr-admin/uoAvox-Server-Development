﻿namespace Server.Items
{
	[FlipableAttribute(0x1BF2, 0x1BEF)]
	public class IronIngot : BaseIngot
	{
		[Constructable]
		public IronIngot() : this(1)
		{
		}

		[Constructable]
		public IronIngot(int amount) : base(CraftResource.Iron, amount)
		{
		}

		public IronIngot(Serial serial) : base(serial)
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