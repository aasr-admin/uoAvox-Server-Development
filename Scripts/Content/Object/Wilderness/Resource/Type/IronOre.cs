﻿namespace Server.Items
{
	public class IronOre : BaseOre
	{
		[Constructable]
		public IronOre() : this(1)
		{
		}

		[Constructable]
		public IronOre(int amount) : base(CraftResource.Iron, amount)
		{
		}

		public IronOre(bool fixedSize) : this(1)
		{
			if (fixedSize)
			{
				ItemID = 0x19B8;
			}
		}

		public IronOre(Serial serial) : base(serial)
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
			return new IronIngot();
		}
	}
}