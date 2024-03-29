﻿namespace Server.Items
{
	public class AgapiteOre : BaseOre
	{
		[Constructable]
		public AgapiteOre() : this(1)
		{
		}

		[Constructable]
		public AgapiteOre(int amount) : base(CraftResource.Agapite, amount)
		{
		}

		public AgapiteOre(Serial serial) : base(serial)
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
			return new AgapiteIngot();
		}
	}
}