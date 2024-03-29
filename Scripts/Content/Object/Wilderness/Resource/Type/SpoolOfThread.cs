﻿namespace Server.Items
{
	public class SpoolOfThread : BaseClothMaterial
	{
		[Constructable]
		public SpoolOfThread() : this(1)
		{
		}

		[Constructable]
		public SpoolOfThread(int amount) : base(0xFA0, amount)
		{
		}

		public SpoolOfThread(Serial serial) : base(serial)
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