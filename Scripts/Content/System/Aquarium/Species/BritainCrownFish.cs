﻿namespace Server.Items
{
	public class BritainCrownFish : BaseAquaticLife
	{
		public override int LabelNumber => 1074589;  // Britain Crown Fish

		[Constructable]
		public BritainCrownFish() : base(0x3AFF)
		{
		}

		public BritainCrownFish(Serial serial) : base(serial)
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