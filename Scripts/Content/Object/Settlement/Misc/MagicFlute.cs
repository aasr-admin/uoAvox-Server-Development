namespace Server.Items
{
	public class MagicFlute : Item
	{
		public override int LabelNumber => 1055051;  // magic flute

		[Constructable]
		public MagicFlute() : base(0x1421)
		{
			Hue = 0x8AB;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				SendLocalizedMessageTo(from, 1042292); // You must have the object in your backpack to use it.
				return;
			}

			from.PlaySound(0x3D);
		}

		public MagicFlute(Serial serial) : base(serial)
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