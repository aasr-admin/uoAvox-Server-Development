﻿namespace Server.Engines.Stealables
{
	public class DecoHorseDung : Item
	{

		[Constructable]
		public DecoHorseDung() : base(0xF3B)
		{
			Movable = true;
			Stackable = false;
		}

		public DecoHorseDung(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}
}