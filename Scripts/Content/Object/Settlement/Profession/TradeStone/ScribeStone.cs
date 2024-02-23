namespace Server.Items
{
	public class ScribeStone : Item
	{
		public override string DefaultName => "a Scribe Supply Stone";

		[Constructable]
		public ScribeStone() : base(0xED4)
		{
			Movable = false;
			Hue = 0x105;
		}

		public override void OnDoubleClick(Mobile from)
		{
			var scribeBag = new ScribeBag();

			if (!from.AddToBackpack(scribeBag))
			{
				scribeBag.Delete();
			}
		}

		public ScribeStone(Serial serial) : base(serial)
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

	public class ScribeBag : Bag
	{
		public override string DefaultName => "a Scribe Kit";

		[Constructable]
		public ScribeBag() : this(1)
		{
			Movable = true;
			Hue = 0x105;
		}

		[Constructable]
		public ScribeBag(int amount)
		{
			DropItem(new BagOfReagents(5000));
			DropItem(new BlankScroll(500));
		}

		public ScribeBag(Serial serial) : base(serial)
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