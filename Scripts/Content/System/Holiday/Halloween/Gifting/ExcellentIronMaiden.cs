﻿namespace Server.Items
{
	public class ExcellentIronMaiden : Item
	{
		public override double DefaultWeight => 5;

		[Constructable]
		public ExcellentIronMaiden()
			: base(0x3f15)
		{
		}

		public ExcellentIronMaiden(Serial serial)
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

			var version = reader.ReadInt();
		}
	}
}