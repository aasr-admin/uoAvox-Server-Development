namespace Server.Items
{
	/// SmallFlask
	[FlipableAttribute(0x182E, 0x182F, 0x1830, 0x1831)]
	public class SmallFlask : Item
	{
		[Constructable]
		public SmallFlask() : base(0x182E)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallFlask(Serial serial) : base(serial)
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

	/// SmallEmptyFlask
	public class SmallEmptyFlask : Item
	{
		[Constructable]
		public SmallEmptyFlask() : base(0x182D)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallEmptyFlask(Serial serial) : base(serial)
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

	/// SmallBlueFlask
	public class SmallBlueFlask : Item
	{
		[Constructable]
		public SmallBlueFlask() : base(0x182A)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallBlueFlask(Serial serial) : base(serial)
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

	/// AniSmallBlueFlask
	public class AniSmallBlueFlask : Item
	{
		[Constructable]
		public AniSmallBlueFlask() : base(0x1844)
		{
			Weight = 1.0;
			Movable = true;
		}

		public AniSmallBlueFlask(Serial serial) : base(serial)
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

	/// SmallYellowFlask
	public class SmallYellowFlask : Item
	{
		[Constructable]
		public SmallYellowFlask() : base(0x182B)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallYellowFlask(Serial serial) : base(serial)
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

	/// SmallRedFlask
	public class SmallRedFlask : Item
	{
		[Constructable]
		public SmallRedFlask() : base(0x182C)
		{
			Weight = 1.0;
			Movable = true;
		}

		public SmallRedFlask(Serial serial) : base(serial)
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

	/// MediumFlask
	[FlipableAttribute(0x182A, 0x182B, 0x182C, 0x182D)]
	public class MediumFlask : Item
	{
		[Constructable]
		public MediumFlask() : base(0x182A)
		{
			Weight = 1.0;
			Movable = true;
		}

		public MediumFlask(Serial serial) : base(serial)
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

	/// LargeFlask
	[FlipableAttribute(0x183B, 0x183C, 0x183D)]
	public class LargeFlask : Item
	{
		[Constructable]
		public LargeFlask() : base(0x183B)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LargeFlask(Serial serial) : base(serial)
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

	/// LargeEmptyFlask
	public class LargeEmptyFlask : Item
	{
		[Constructable]
		public LargeEmptyFlask() : base(0x183D)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LargeEmptyFlask(Serial serial) : base(serial)
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

	/// LargeYellowFlask
	public class LargeYellowFlask : Item
	{
		[Constructable]
		public LargeYellowFlask() : base(0x183B)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LargeYellowFlask(Serial serial) : base(serial)
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

	/// LargeVioletFlask
	public class LargeVioletFlask : Item
	{
		[Constructable]
		public LargeVioletFlask() : base(0x183C)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LargeVioletFlask(Serial serial) : base(serial)
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

	/// AniLargeVioletFlask
	public class AniLargeVioletFlask : Item
	{
		[Constructable]
		public AniLargeVioletFlask() : base(0x1841)
		{
			Weight = 1.0;
			Movable = true;
		}

		public AniLargeVioletFlask(Serial serial) : base(serial)
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

	/// LongFlask
	[FlipableAttribute(0x1838, 0x1839, 0x183A)]
	public class LongFlask : Item
	{
		[Constructable]
		public LongFlask() : base(0x1838)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LongFlask(Serial serial) : base(serial)
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

	/// CurvedFlask
	[FlipableAttribute(0x1832, 0x1833, 0x1834, 0x1835, 0x1836, 0x1837)]
	public class CurvedFlask : Item
	{
		[Constructable]
		public CurvedFlask() : base(0x1832)
		{
			Weight = 1.0;
			Movable = true;
		}

		public CurvedFlask(Serial serial) : base(serial)
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

	/// EmptyCurvedFlask
	public class EmptyCurvedFlaskE : Item
	{
		[Constructable]
		public EmptyCurvedFlaskE() : base(0x1835)
		{
			Weight = 1.0;
			Movable = true;
		}

		public EmptyCurvedFlaskE(Serial serial) : base(serial)
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

	public class EmptyCurvedFlaskW : Item
	{
		[Constructable]
		public EmptyCurvedFlaskW() : base(0x1832)
		{
			Weight = 1.0;
			Movable = true;
		}

		public EmptyCurvedFlaskW(Serial serial) : base(serial)
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

	/// RedCurvedFlask
	public class RedCurvedFlask : Item
	{
		[Constructable]
		public RedCurvedFlask() : base(0x1833)
		{
			Weight = 1.0;
			Movable = true;
		}

		public RedCurvedFlask(Serial serial) : base(serial)
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

	/// BlueCurvedFlask
	public class BlueCurvedFlask : Item
	{
		[Constructable]
		public BlueCurvedFlask() : base(0x1836)
		{
			Weight = 1.0;
			Movable = true;
		}

		public BlueCurvedFlask(Serial serial) : base(serial)
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

	/// LtBlueCurvedFlask
	public class LtBlueCurvedFlask : Item
	{
		[Constructable]
		public LtBlueCurvedFlask() : base(0x1834)
		{
			Weight = 1.0;
			Movable = true;
		}

		public LtBlueCurvedFlask(Serial serial) : base(serial)
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

	/// GreenCurvedFlask
	public class GreenCurvedFlask : Item
	{
		[Constructable]
		public GreenCurvedFlask() : base(0x1837)
		{
			Weight = 1.0;
			Movable = true;
		}

		public GreenCurvedFlask(Serial serial) : base(serial)
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

	/// EmptyRibbedFlask
	public class EmptyRibbedFlask : Item
	{
		[Constructable]
		public EmptyRibbedFlask() : base(0x183A)
		{
			Weight = 1.0;
			Movable = true;
		}

		public EmptyRibbedFlask(Serial serial) : base(serial)
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

	/// RedRibbedFlask
	public class RedRibbedFlask : Item
	{
		[Constructable]
		public RedRibbedFlask() : base(0x1838)
		{
			Weight = 1.0;
			Movable = true;
		}

		public RedRibbedFlask(Serial serial) : base(serial)
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

	/// AniRedRibbedFlask
	public class AniRedRibbedFlask : Item
	{
		[Constructable]
		public AniRedRibbedFlask() : base(0x183E)
		{
			Weight = 1.0;
			Movable = true;
		}

		public AniRedRibbedFlask(Serial serial) : base(serial)
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

	/// VioletRibbedFlask
	public class VioletRibbedFlask : Item
	{
		[Constructable]
		public VioletRibbedFlask() : base(0x1839)
		{
			Weight = 1.0;
			Movable = true;
		}

		public VioletRibbedFlask(Serial serial) : base(serial)
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