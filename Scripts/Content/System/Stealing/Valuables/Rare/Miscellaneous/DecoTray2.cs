﻿namespace Server.Engines.Stealables
{
	public class DecoTray2 : Item
	{

		[Constructable]
		public DecoTray2() : base(0x991)
		{
			Movable = true;
			Stackable = false;
		}

		public DecoTray2(Serial serial) : base(serial)
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