﻿namespace Server.Items
{
	public class CocoaPulp : Item
	{
		public override int LabelNumber => 1080530;  // cocoa pulp
		public override double DefaultWeight => 1.0;

		[Constructable]
		public CocoaPulp()
			: this(1)
		{
		}

		[Constructable]
		public CocoaPulp(int amount)
			: base(0xF7C)
		{
			Hue = 0x219;
			Stackable = true;
			Amount = amount;
		}

		public CocoaPulp(Serial serial)
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