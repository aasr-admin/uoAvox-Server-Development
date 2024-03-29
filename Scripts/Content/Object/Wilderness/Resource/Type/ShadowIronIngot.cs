﻿namespace Server.Items
{
	[FlipableAttribute(0x1BF2, 0x1BEF)]
	public class ShadowIronIngot : BaseIngot
	{
		[Constructable]
		public ShadowIronIngot() : this(1)
		{
		}

		[Constructable]
		public ShadowIronIngot(int amount) : base(CraftResource.ShadowIron, amount)
		{
		}

		public ShadowIronIngot(Serial serial) : base(serial)
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