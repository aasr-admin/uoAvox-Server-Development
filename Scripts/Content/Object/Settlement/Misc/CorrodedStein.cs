namespace Server.Items
{
	public class CorrodedStein : Item
	{
		public override int LabelNumber => 1072083;  // Paroxysmus' Corroded Stein

		[Constructable]
		public CorrodedStein() : base(0x9D6)
		{
		}

		public CorrodedStein(Serial serial) : base(serial)
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