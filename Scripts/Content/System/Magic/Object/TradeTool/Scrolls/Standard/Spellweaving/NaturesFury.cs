﻿namespace Server.Items
{
	public class NaturesFuryScroll : SpellScroll
	{
		[Constructable]
		public NaturesFuryScroll()
			: this(1)
		{
		}

		[Constructable]
		public NaturesFuryScroll(int amount) : base(SpellName.NaturesFury, 0x2D56, amount)
		{
		}

		public NaturesFuryScroll(Serial serial)
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