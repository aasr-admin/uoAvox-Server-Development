using Server.ContextMenus;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	public enum BagOfSendingHue
	{
		Yellow,
		Blue,
		Red
	}

	public class BagOfSending : Item, ITranslocationItem
	{
		public static BagOfSendingHue RandomHue()
		{
			return Utility.Random(3) switch
			{
				0 => BagOfSendingHue.Yellow,
				1 => BagOfSendingHue.Blue,
				_ => BagOfSendingHue.Red,
			};
		}

		private int m_Charges;
		private int m_Recharges;
		private BagOfSendingHue m_BagOfSendingHue;

		[CommandProperty(AccessLevel.GameMaster)]
		public int Charges
		{
			get => m_Charges;
			set
			{
				if (value > MaxCharges)
				{
					m_Charges = MaxCharges;
				}
				else if (value < 0)
				{
					m_Charges = 0;
				}
				else
				{
					m_Charges = value;
				}

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Recharges
		{
			get => m_Recharges;
			set
			{
				if (value > MaxRecharges)
				{
					m_Recharges = MaxRecharges;
				}
				else if (value < 0)
				{
					m_Recharges = 0;
				}
				else
				{
					m_Recharges = value;
				}

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxCharges => 30;

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxRecharges => 255;

		public string TranslocationItemName => "bag of sending";

		public override int LabelNumber => 1054104;  // a bag of sending

		[CommandProperty(AccessLevel.GameMaster)]
		public BagOfSendingHue BagOfSendingHue
		{
			get => m_BagOfSendingHue;
			set
			{
				m_BagOfSendingHue = value;

				switch (value)
				{
					case BagOfSendingHue.Yellow: Hue = 0x8A5; break;
					case BagOfSendingHue.Blue: Hue = 0x8AD; break;
					case BagOfSendingHue.Red: Hue = 0x89B; break;
				}
			}
		}

		[Constructable]
		public BagOfSending() : this(RandomHue())
		{
		}

		[Constructable]
		public BagOfSending(BagOfSendingHue hue) : base(0xE76)
		{
			Weight = 2.0;

			BagOfSendingHue = hue;

			m_Charges = Utility.RandomMinMax(3, 9);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add(1060741, m_Charges.ToString()); // charges: ~1_val~
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			LabelTo(from, 1060741, m_Charges.ToString()); // charges: ~1_val~
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from.Alive)
			{
				list.Add(new UseBagEntry(this, Charges > 0 && IsChildOf(from.Backpack)));
			}
		}

		private class UseBagEntry : ContextMenuEntry
		{
			private readonly BagOfSending m_Bag;

			public UseBagEntry(BagOfSending bag, bool enabled) : base(6189)
			{
				m_Bag = bag;

				if (!enabled)
				{
					Flags |= CMEFlags.Disabled;
				}
			}

			public override void OnClick()
			{
				if (m_Bag.Deleted)
				{
					return;
				}

				var from = Owner.From;

				if (from.CheckAlive())
				{
					m_Bag.OnDoubleClick(from);
				}
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.Region.IsPartOf(typeof(Regions.Jail)))
			{
				from.SendMessage("You may not do that in jail.");
			}
			else if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1062334, String.Empty, 0x59); // The bag of sending must be in your backpack.
			}
			else if (Charges == 0)
			{
				from.SendLocalizedMessage(1042544, String.Empty, 0x59); // This item is out of charges.
			}
			else
			{
				from.Target = new SendTarget(this);
			}
		}

		private class SendTarget : Target
		{
			private readonly BagOfSending m_Bag;

			public SendTarget(BagOfSending bag) : base(-1, false, TargetFlags.None)
			{
				m_Bag = bag;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Bag.Deleted)
				{
					return;
				}

				if (from.Region.IsPartOf(typeof(Regions.Jail)))
				{
					from.SendMessage("You may not do that in jail.");
				}
				else if (!m_Bag.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1062334, String.Empty, 0x59); // The bag of sending must be in your backpack. 1054107 is gone from client, using generic response
				}
				else if (m_Bag.Charges == 0)
				{
					from.SendLocalizedMessage(1042544, String.Empty, 0x59); // This item is out of charges.
				}
				else if (targeted is Item item)
				{
					var reqCharges = (int)Math.Max(1, Math.Ceiling(item.TotalWeight / 10.0));

					if (!item.IsChildOf(from.Backpack))
					{
						from.SendLocalizedMessage(1054152, String.Empty, 0x59); // You may only send items from your backpack to your bank box.
					}
					else if (item is BagOfSending or Container)
					{
						_ = from.Send(new AsciiMessage(m_Bag.Serial, m_Bag.ItemID, MessageType.Regular, 0x3B2, 3, "", "You cannot send a container through the bag of sending."));
					}
					else if (item.LootType == LootType.Cursed)
					{
						from.SendLocalizedMessage(1054108, String.Empty, 0x59); // The bag of sending rejects the cursed item.
					}
					else if (!item.VerifyMove(from) || item.QuestItem || item.Nontransferable)
					{
						from.SendLocalizedMessage(1054109, String.Empty, 0x59); // The bag of sending rejects that item.
					}
					else if (Spells.SpellHelper.IsDoomGauntlet(from.Map, from.Location))
					{
						from.SendLocalizedMessage(1062089); // You cannot use that here.
					}
					else if (!from.BankBox.TryDropItem(from, item, false))
					{
						from.SendLocalizedMessage(1054110, String.Empty, 0x59); // Your bank box is full.
					}
					else if (Core.ML && reqCharges > m_Bag.Charges)
					{
						from.SendLocalizedMessage(1079932); //You don't have enough charges to send that much weight
					}
					else
					{
						m_Bag.Charges -= Core.ML ? reqCharges : 1;

						from.SendLocalizedMessage(1054150, String.Empty, 0x59); // The item was placed in your bank box.
					}
				}
			}
		}

		public BagOfSending(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(1); // version

			writer.WriteEncodedInt(m_Recharges);

			writer.WriteEncodedInt(m_Charges);
			writer.WriteEncodedInt((int)m_BagOfSendingHue);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 1:
					{
						m_Recharges = reader.ReadEncodedInt();
						goto case 0;
					}
				case 0:
					{
						m_Charges = Math.Min(reader.ReadEncodedInt(), MaxCharges);
						m_BagOfSendingHue = (BagOfSendingHue)reader.ReadEncodedInt();
						break;
					}
			}
		}
	}
}