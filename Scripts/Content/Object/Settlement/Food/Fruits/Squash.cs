﻿namespace Server.Items
{
	[FlipableAttribute(0xc72, 0xc73)]
	public class Squash : Food
	{
		[Constructable]
		public Squash() : this(1)
		{
		}

		[Constructable]
		public Squash(int amount) : base(amount, 0xc72)
		{
			Weight = 1.0;
			FillFactor = 1;
		}

		public Squash(Serial serial) : base(serial)
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