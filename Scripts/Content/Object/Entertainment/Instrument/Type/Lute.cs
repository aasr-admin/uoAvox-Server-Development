﻿namespace Server.Items
{
	public class Lute : BaseInstrument
	{
		[Constructable]
		public Lute() : base(0xEB3, 0x4C, 0x4D)
		{
			Weight = 5.0;
		}

		public Lute(Serial serial) : base(serial)
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

			if (Weight == 3.0)
			{
				Weight = 5.0;
			}
		}
	}
}