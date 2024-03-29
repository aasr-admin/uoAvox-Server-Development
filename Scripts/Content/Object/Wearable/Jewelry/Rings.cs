﻿namespace Server.Items
{
	/// GoldRing
	public class GoldRing : BaseRing
	{
		[Constructable]
		public GoldRing() : base(0x108a)
		{
			Weight = 0.1;
		}

		public GoldRing(Serial serial) : base(serial)
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

	/// SilverRing
	public class SilverRing : BaseRing
	{
		[Constructable]
		public SilverRing() : base(0x1F09)
		{
			Weight = 0.1;
		}

		public SilverRing(Serial serial) : base(serial)
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