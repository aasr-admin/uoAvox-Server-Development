namespace Server.Items
{
	/// TallMusicStand
	[Furniture]
	[Flipable(0xEBB, 0xEBC)]
	public class TallMusicStand : Item
	{
		[Constructable]
		public TallMusicStand() : base(0xEBB)
		{
			Weight = 10.0;
		}

		public TallMusicStand(Serial serial) : base(serial)
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

			if (Weight == 8.0)
			{
				Weight = 10.0;
			}
		}
	}

	/// ShortMusicStand
	[Furniture]
	[Flipable(0xEB6, 0xEB8)]
	public class ShortMusicStand : Item
	{
		[Constructable]
		public ShortMusicStand() : base(0xEB6)
		{
			Weight = 10.0;
		}

		public ShortMusicStand(Serial serial) : base(serial)
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

			if (Weight == 6.0)
			{
				Weight = 10.0;
			}
		}
	}
}
