﻿namespace Server.Items
{
	public class CleansingWindsScroll : SpellScroll
	{
		[Constructable]
		public CleansingWindsScroll()
			: this(1)
		{
		}

		[Constructable]
		public CleansingWindsScroll(int amount) : base(SpellName.CleansingWinds, 0x2DA8, amount)
		{
		}

		public CleansingWindsScroll(Serial serial)
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

			/*int version = */
			reader.ReadInt();
		}
	}
}