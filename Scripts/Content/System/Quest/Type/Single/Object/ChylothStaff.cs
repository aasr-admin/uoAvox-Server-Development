﻿namespace Server.Items
{
	public class ChylothStaff : BlackStaff
	{
		public override int LabelNumber => 1041111;  // a magic staff

		[Constructable]
		public ChylothStaff()
		{
			Hue = 0x482;
		}

		public ChylothStaff(Serial serial) : base(serial)
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