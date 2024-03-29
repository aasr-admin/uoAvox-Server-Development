﻿namespace Server.Engines.Stealables
{
	public class DecoPumice : Item
	{

		[Constructable]
		public DecoPumice() : base(0xF8B)
		{
			Movable = true;
			Stackable = false;
		}

		public DecoPumice(Serial serial) : base(serial)
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