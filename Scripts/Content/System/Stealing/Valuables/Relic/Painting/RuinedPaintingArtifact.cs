﻿namespace Server.Items
{
	public class RuinedPaintingArtifact : BaseDecorationArtifact
	{
		public override int ArtifactRarity => 12;

		[Constructable]
		public RuinedPaintingArtifact() : base(0xC2C)
		{
		}

		public RuinedPaintingArtifact(Serial serial) : base(serial)
		{
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