﻿using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Factions
{
	public class JoinStone : BaseSystemController
	{
		private Faction m_Faction;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public Faction Faction
		{
			get => m_Faction;
			set
			{
				m_Faction = value;

				Hue = (m_Faction == null ? 0 : m_Faction.Definition.HueJoin);
				AssignName(m_Faction == null ? null : m_Faction.Definition.SignupName);
			}
		}

		public override string DefaultName => "faction signup stone";

		[Constructable]
		public JoinStone() : this(null)
		{
		}

		[Constructable]
		public JoinStone(Faction faction) : base(0xEDC)
		{
			Movable = false;
			Faction = faction;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (m_Faction == null)
			{
				return;
			}

			if (!from.InRange(GetWorldLocation(), 2))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
			else if (FactionGump.Exists(from))
			{
				from.SendLocalizedMessage(1042160); // You already have a faction menu open.
			}
			else if (Faction.Find(from) == null && from is PlayerMobile)
			{
				from.SendGump(new JoinStoneGump((PlayerMobile)from, m_Faction));
			}
		}

		public JoinStone(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			Faction.WriteReference(writer, m_Faction);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Faction = Faction.ReadReference(reader);
						break;
					}
			}
		}
	}

	public class JoinStoneGump : FactionGump
	{
		private readonly PlayerMobile m_From;
		private readonly Faction m_Faction;

		public JoinStoneGump(PlayerMobile from, Faction faction) : base(20, 30)
		{
			m_From = from;
			m_Faction = faction;

			AddPage(0);

			AddBackground(0, 0, 550, 440, 5054);
			AddBackground(10, 10, 530, 420, 3000);


			AddHtmlText(20, 30, 510, 20, faction.Definition.Header, false, false);
			AddHtmlText(20, 130, 510, 100, faction.Definition.About, true, true);


			AddHtmlLocalized(20, 60, 100, 20, 1011429, false, false); // Led By : 
			AddHtml(125, 60, 200, 20, faction.Commander != null ? faction.Commander.Name : "Nobody", false, false);

			AddHtmlLocalized(20, 80, 100, 20, 1011457, false, false); // Tithe rate : 
			if (faction.Tithe >= 0 && faction.Tithe <= 100 && (faction.Tithe % 10) == 0)
			{
				AddHtmlLocalized(125, 80, 350, 20, 1011480 + (faction.Tithe / 10), false, false);
			}
			else
			{
				AddHtml(125, 80, 350, 20, faction.Tithe + "%", false, false);
			}

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 200, 20, 1011425, false, false); // JOIN THIS FACTION

			AddButton(300, 400, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 200, 20, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1)
			{
				m_Faction.OnJoinAccepted(m_From);
			}
		}
	}
}