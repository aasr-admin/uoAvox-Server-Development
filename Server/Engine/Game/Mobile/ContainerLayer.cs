﻿using Server.Accounting;
using Server.Network;

using System;

namespace Server.Items
{
	public class BankBox : Container
	{
		private Mobile m_Owner;
		private bool m_Open;

		public override int DefaultMaxWeight => 0;

		public override bool IsVirtualItem => true;

		public BankBox(Serial serial) : base(serial)
		{
		}

		public Mobile Owner => m_Owner;

		public bool Opened => m_Open;

		public void Open()
		{
			m_Open = true;

			if (m_Owner != null)
			{
				m_Owner.PrivateOverheadMessage(MessageType.Regular, 0x3B2, true, String.Format("Bank container has {0} items, {1} stones", TotalItems, TotalWeight), m_Owner.NetState);
				m_Owner.Send(new EquipUpdate(this));
				DisplayTo(m_Owner);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Owner);
			writer.Write(m_Open);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Owner = reader.ReadMobile();
						m_Open = reader.ReadBool();

						if (m_Owner == null)
						{
							Delete();
						}

						break;
					}
			}

			if (ItemID == 0xE41)
			{
				ItemID = 0xE7C;
			}
		}

		private static bool m_SendRemovePacket;

		public static bool SendDeleteOnClose { get => m_SendRemovePacket; set => m_SendRemovePacket = value; }

		public void Close()
		{
			m_Open = false;

			if (m_Owner != null && m_SendRemovePacket)
			{
				m_Owner.Send(RemovePacket);
			}
		}

		public override void OnSingleClick(Mobile from)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
		}

		public override DeathMoveResult OnParentDeath(Mobile parent)
		{
			return DeathMoveResult.RemainEquiped;
		}

		public BankBox(Mobile owner) : base(0xE7C)
		{
			Layer = Layer.Bank;
			Movable = false;
			m_Owner = owner;
		}

		public override bool IsAccessibleTo(Mobile check)
		{
			if ((check == m_Owner && m_Open) || check.AccessLevel >= AccessLevel.GameMaster)
			{
				return base.IsAccessibleTo(check);
			}
			else
			{
				return false;
			}
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			if ((from == m_Owner && m_Open) || from.AccessLevel >= AccessLevel.GameMaster)
			{
				return base.OnDragDrop(from, dropped);
			}
			else
			{
				return false;
			}
		}

		public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
		{
			if ((from == m_Owner && m_Open) || from.AccessLevel >= AccessLevel.GameMaster)
			{
				return base.OnDragDropInto(from, item, p);
			}
			else
			{
				return false;
			}
		}

		public override int GetTotal(TotalType type)
		{
			if (AccountGold.Enabled && Owner != null && Owner.Account != null && type == TotalType.Gold)
			{
				return Owner.Account.TotalGold;
			}

			return base.GetTotal(type);
		}
	}
}