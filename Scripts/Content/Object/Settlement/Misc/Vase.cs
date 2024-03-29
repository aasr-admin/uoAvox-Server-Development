﻿namespace Server.Items
{
	public class Vase : Item
	{
		[Constructable]
		public Vase() : base(0xB46)
		{
			Weight = 10;
		}

		public Vase(Serial serial) : base(serial)
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

	public class LargeVase : Item
	{
		[Constructable]
		public LargeVase() : base(0xB45)
		{
			Weight = 15;
		}

		public LargeVase(Serial serial) : base(serial)
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