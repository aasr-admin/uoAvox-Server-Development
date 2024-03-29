﻿namespace Server.Items
{
	[FlipableAttribute(0x1BF2, 0x1BEF)]
	public class VeriteIngot : BaseIngot
	{
		[Constructable]
		public VeriteIngot() : this(1)
		{
		}

		[Constructable]
		public VeriteIngot(int amount) : base(CraftResource.Verite, amount)
		{
		}

		public VeriteIngot(Serial serial) : base(serial)
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