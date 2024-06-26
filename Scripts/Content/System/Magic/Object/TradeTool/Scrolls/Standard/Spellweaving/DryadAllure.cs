﻿namespace Server.Items
{
	public class DryadAllureScroll : SpellScroll
	{
		[Constructable]
		public DryadAllureScroll()
			: this(1)
		{
		}

		[Constructable]
		public DryadAllureScroll(int amount) : base(SpellName.DryadAllure, 0x2D5C, amount)
		{
		}

		public DryadAllureScroll(Serial serial)
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