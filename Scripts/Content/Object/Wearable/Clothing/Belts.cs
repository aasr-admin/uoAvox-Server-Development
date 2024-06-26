﻿namespace Server.Items
{
	/// Obi
	[Flipable(0x27A0, 0x27EB)]
	public class Obi : BaseWaist
	{
		[Constructable]
		public Obi() : this(0)
		{
		}

		[Constructable]
		public Obi(int hue) : base(0x27A0, hue)
		{
			Weight = 1.0;
		}

		public Obi(Serial serial) : base(serial)
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

	/// WoodlandBelt
	[FlipableAttribute(0x2B68, 0x315F)]
	public class WoodlandBelt : BaseWaist
	{
		public override Race RequiredRace => Race.Elf;

		[Constructable]
		public WoodlandBelt() : this(0)
		{
		}

		[Constructable]
		public WoodlandBelt(int hue) : base(0x2B68, hue)
		{
			Weight = 4.0;
		}

		public WoodlandBelt(Serial serial) : base(serial)
		{
		}

		public override bool Dye(Mobile from, DyeTub sender)
		{
			from.SendLocalizedMessage(sender.FailMessage);
			return false;
		}

		public override bool Scissor(Mobile from, Scissors scissors)
		{
			from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}
}
