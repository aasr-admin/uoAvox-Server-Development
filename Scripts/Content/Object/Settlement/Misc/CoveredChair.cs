namespace Server.Items
{
	[FlipableAttribute(0xC17, 0xC18)]
	public class CoveredChair : Item
	{
		[Constructable]
		public CoveredChair() : base(0xC17)
		{
			Movable = false;
		}

		public CoveredChair(Serial serial) : base(serial)
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