﻿namespace Server.Mobiles
{
	public class SummonedBakeKitsune : BaseTalismanSummon
	{
		[Constructable]
		public SummonedBakeKitsune() : base()
		{
			Name = "a bake kitsune";
			Body = 246;
			BaseSoundID = 0x4DD;
		}

		public SummonedBakeKitsune(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}
}