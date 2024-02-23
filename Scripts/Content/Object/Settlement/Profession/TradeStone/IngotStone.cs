namespace Server.Items
{
	public class IngotStone : Item
	{
		public override string DefaultName => "an Ingot stone";

		[Constructable]
		public IngotStone() : base(0xED4)
		{
			Movable = false;
			Hue = 0x480;
		}

		public override void OnDoubleClick(Mobile from)
		{
			var ingotBag = new BagOfingots(5000);

			if (!from.AddToBackpack(ingotBag))
			{
				ingotBag.Delete();
			}
		}

		public IngotStone(Serial serial) : base(serial)
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

	public class BagOfingots : Bag
	{
		[Constructable]
		public BagOfingots() : this(5000)
		{
		}

		[Constructable]
		public BagOfingots(int amount)
		{
			DropItem(new DullCopperIngot(amount));
			DropItem(new ShadowIronIngot(amount));
			DropItem(new CopperIngot(amount));
			DropItem(new BronzeIngot(amount));
			DropItem(new GoldIngot(amount));
			DropItem(new AgapiteIngot(amount));
			DropItem(new VeriteIngot(amount));
			DropItem(new ValoriteIngot(amount));
			DropItem(new IronIngot(amount));
			DropItem(new Tongs());
			DropItem(new TinkerTools());

		}

		public BagOfingots(Serial serial) : base(serial)
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