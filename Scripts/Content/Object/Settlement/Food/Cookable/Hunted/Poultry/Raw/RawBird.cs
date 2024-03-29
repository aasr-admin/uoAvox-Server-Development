﻿namespace Server.Items
{
	public class RawBird : CookableFood
	{
		[Constructable]
		public RawBird() : this(1)
		{
		}

		[Constructable]
		public RawBird(int amount) : base(0x9B9, 10)
		{
			Weight = 1.0;
			Stackable = true;
			Amount = amount;
		}

		public override Food Cook()
		{
			return new CookedBird();
		}

		public RawBird(Serial serial) : base(serial)
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