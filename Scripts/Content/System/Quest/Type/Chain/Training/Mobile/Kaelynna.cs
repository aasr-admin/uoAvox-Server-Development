﻿using Server.Items;
using Server.Mobiles;

namespace Server.Engines.ChainQuests.Mobiles
{
	public class Kaelynna : BaseCreature
	{
		public override bool IsInvulnerable => true;
		public override bool CanTeach => true;

		public override bool CanShout => true;
		public override void Shout(PlayerMobile pm)
		{
			ChainQuestSystem.Tell(this, pm, 1078125); // Want to unlock the secrets of magery?
		}

		[Constructable]
		public Kaelynna()
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			Name = "Kaelynna";
			Title = "the Magery Instructor";
			BodyValue = 0x191;
			Female = true;
			Hue = 0x83EA;
			HairItemID = 0x203C;
			HairHue = 0x47D;

			InitStats(100, 100, 25);

			SetSkill(SkillName.EvalInt, 120.0);
			SetSkill(SkillName.Inscribe, 120.0);
			SetSkill(SkillName.Magery, 120.0);
			SetSkill(SkillName.MagicResist, 120.0);
			SetSkill(SkillName.Wrestling, 120.0);
			SetSkill(SkillName.Meditation, 120.0);

			AddItem(new Backpack());
			AddItem(new Robe(0x592));
			AddItem(new Sandals());
		}

		public Kaelynna(Serial serial)
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