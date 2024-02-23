namespace Server.Items
{
	public class WindChimes : BaseWindChimes
	{
		public override int LabelNumber => 1030290;

		[Constructable]
		public WindChimes() : base(0x2832)
		{
		}

		public WindChimes(Serial serial) : base(serial)
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