﻿namespace Server.Items
{
	public class RawLambLeg : CookableFood
	{
		[Constructable]
		public RawLambLeg() : this(1)
		{
		}

		[Constructable]
		public RawLambLeg(int amount) : base(0x1609, 10)
		{
			Stackable = true;
			Amount = amount;
		}

		public override Food Cook()
		{
			return new LambLeg();
		}

		public RawLambLeg(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			if (version == 0 && Weight == 1)
			{
				Weight = -1;
			}
		}
	}
}