﻿namespace Server.Items
{
	public class PainSpikeScroll : SpellScroll
	{
		[Constructable]
		public PainSpikeScroll() : this(1)
		{
		}

		[Constructable]
		public PainSpikeScroll(int amount) : base(SpellName.PainSpike, 0x2268, amount)
		{
		}

		public PainSpikeScroll(Serial serial) : base(serial)
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