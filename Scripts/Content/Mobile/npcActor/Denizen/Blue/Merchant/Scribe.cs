﻿
using System;
using System.Collections.Generic;

#region Developer Notations

/// In Select Shops There Should ALWAYS Be One Merchant That Sells Every Resources For Their Trade
/// In Select Shops There Should ALWAYS Be One Merchant That Sells Every TradeTools For Their Trade
/// In Select Shops There Should ALWAYS Be One Merchant That Sells Products Created From Their Trade

#endregion

namespace Server.Mobiles
{
	public class Scribe : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos => m_SBInfos;

		public override NpcGuild NpcGuild => NpcGuild.MagesGuild;

		private DateTime m_NextShush;
		public static readonly TimeSpan ShushDelay = TimeSpan.FromMinutes(1);

		[Constructable]
		public Scribe() : base("the scribe")
		{
			SetSkill(SkillName.EvalInt, 60.0, 83.0);
			SetSkill(SkillName.Inscribe, 90.0, 100.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBScribe());
		}

		public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(new Server.Items.Robe(Utility.RandomNeutralHue()));
		}

		public override bool HandlesOnSpeech(Mobile from)
		{
			return from.Player;
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			base.OnSpeech(e);

			if (!e.Handled && m_NextShush <= DateTime.UtcNow && InLOS(e.Mobile))
			{
				Direction = GetDirectionTo(e.Mobile);

				PlaySound(Female ? 0x32F : 0x441);
				PublicOverheadMessage(Network.MessageType.Regular, 0x3B2, 1073990); // Shhhh!

				m_NextShush = DateTime.UtcNow + ShushDelay;
				e.Handled = true;
			}
		}

		public Scribe(Serial serial) : base(serial)
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