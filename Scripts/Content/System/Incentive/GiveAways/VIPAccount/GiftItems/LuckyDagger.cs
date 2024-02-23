namespace Server.Items
{
	public class LuckyDagger : Dagger
	{
		[Constructable]
		public LuckyDagger()
		{
			Hue = 0x8A5;
			Weight = 1.0;
		}

		public LuckyDagger(Serial serial) : base(serial)
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