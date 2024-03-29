﻿namespace Server.Items
{
	[FlipableAttribute(0x1BF2, 0x1BEF)]
	public class ValoriteIngot : BaseIngot
	{
		[Constructable]
		public ValoriteIngot() : this(1)
		{
		}

		[Constructable]
		public ValoriteIngot(int amount) : base(CraftResource.Valorite, amount)
		{
		}

		public ValoriteIngot(Serial serial) : base(serial)
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