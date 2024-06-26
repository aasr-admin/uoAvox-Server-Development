﻿namespace Server.Items
{
	public class NetherCycloneScroll : SpellScroll
	{
		[Constructable]
		public NetherCycloneScroll()
			: this(1)
		{
		}

		[Constructable]
		public NetherCycloneScroll(int amount) : base(SpellName.NetherCyclone, 0x2DAC, amount)
		{
		}

		public NetherCycloneScroll(Serial serial)
			: base(serial)
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

			/*int version = */
			reader.ReadInt();
		}
	}
}