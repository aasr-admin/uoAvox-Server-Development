using Server.Items;

namespace Server.Mobiles
{
	[CorpseName("a Saliva corpse")]
	public class Saliva : Harpy
	{
		[Constructable]
		public Saliva()
		{
			IsParagon = true;

			Name = "Saliva";
			Hue = 0x11E;

			SetStr(110, 206);
			SetDex(123, 222);
			SetInt(80, 127);

			SetHits(409, 842);

			SetDamage(20, 22);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 46, 48);
			SetResistance(ResistanceType.Fire, 32, 40);
			SetResistance(ResistanceType.Cold, 34, 49);
			SetResistance(ResistanceType.Poison, 40, 48);
			SetResistance(ResistanceType.Energy, 35, 39);

			SetSkill(SkillName.Wrestling, 106.4, 128.8);
			SetSkill(SkillName.Tactics, 129.9, 141.0);
			SetSkill(SkillName.MagicResist, 84.3, 105.0);

			// TODO: Fame/Karma?
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.UltraRich, 2);
		}

		public Saliva(Serial serial)
			: base(serial)
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