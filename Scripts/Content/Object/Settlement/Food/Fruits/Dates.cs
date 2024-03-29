﻿namespace Server.Items
{
	public class Dates : Food
	{
		[Constructable]
		public Dates() : this(1)
		{
		}

		[Constructable]
		public Dates(int amount) : base(amount, 0x1727)
		{
			Weight = 1.0;
			FillFactor = 1;
		}

		public Dates(Serial serial) : base(serial)
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