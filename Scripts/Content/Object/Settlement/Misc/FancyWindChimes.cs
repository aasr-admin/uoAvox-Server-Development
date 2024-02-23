namespace Server.Items
{
	public class FancyWindChimes : BaseWindChimes
	{
		public override int LabelNumber => 1030291;

		[Constructable]
		public FancyWindChimes() : base(0x2833)
		{
		}

		public FancyWindChimes(Serial serial) : base(serial)
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