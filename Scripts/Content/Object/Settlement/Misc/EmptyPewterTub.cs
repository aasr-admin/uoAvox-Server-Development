namespace Server.Items
{
	[TypeAlias("Server.Items.EmptyLargePewterBowl")]
	public class EmptyPewterTub : Item
	{
		[Constructable]
		public EmptyPewterTub() : base(0x1603)
		{
			Weight = 2.0;
		}

		public EmptyPewterTub(Serial serial) : base(serial)
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