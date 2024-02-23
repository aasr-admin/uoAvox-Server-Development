namespace Server.Items
{
	[FlipableAttribute(0xC2D, 0xC2F, 0xC2E, 0xC30)]
	public class WoodDebris : Item
	{
		[Constructable]
		public WoodDebris() : base(0xC2D)
		{
			Movable = false;
		}

		public WoodDebris(Serial serial) : base(serial)
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