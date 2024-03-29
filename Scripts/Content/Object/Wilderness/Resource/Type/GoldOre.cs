﻿namespace Server.Items
{
	public class GoldOre : BaseOre
	{
		[Constructable]
		public GoldOre() : this(1)
		{
		}

		[Constructable]
		public GoldOre(int amount) : base(CraftResource.Gold, amount)
		{
		}

		public GoldOre(Serial serial) : base(serial)
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

		public override BaseIngot GetIngot()
		{
			return new GoldIngot();
		}
	}
}