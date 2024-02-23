using System;

using Server.Items;
using Server.Mobiles;

namespace Server.Quests
{
	public abstract class QuestLauncherItem : Item, IQuestLauncher
	{
		public abstract Type[] Quests { get; }

		public virtual bool QuestRandomize { get; }

		public override sealed bool Nontransferable => true;

		public override sealed bool IsVirtualItem => true;

		public override sealed bool DisplayWeight => false;

		public override sealed double DefaultWeight => 0;

		public virtual string DefaultDescription => null;

		private string _Description;

		[CommandProperty(AccessLevel.GameMaster)]
		public string Description
		{
			get => _Description ?? DefaultDescription;
			set
			{
				if (value == null || value != DefaultDescription)
				{
					_Description = value;

					InvalidateProperties();
				}
			}
		}

		public QuestLauncherItem(int itemID)
			: base(itemID)
		{
			QuestItem = true;
		}

		public QuestLauncherItem(Serial serial)
			: base(serial)
		{
		}

		public override bool IsAccessibleTo(Mobile check)
		{
			if (!base.IsAccessibleTo(check))
			{
				return false;
			}

			var root = RootParent;

			if (root == check)
			{
				return true;
			}

			if (Movable)
			{
				var parent = Parent as Container;

				while (parent != null)
				{
					if (!parent.IsAccessibleTo(check))
					{
						return false;
					}

					parent = parent.Parent as Container;
				}
			}
			else if (IsLockedDown || IsSecure)
			{
				return false;
			}

			return true;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from is not PlayerMobile player)
			{
				return;
			}

			if (!IsAccessibleTo(from))
			{
				if (Movable || IsLockedDown || IsSecure)
				{
					player.SendMessage(0x22, $"{Name} must be in your possession to access it.");
				}
				else
				{
					player.SendMessage(0x22, $"{Name} is inaccessible.");
				}

				return;
			}

			var quest = QuestUtility.CreateQuest(player, this);

			if (quest?.Offer() != null)
			{
				QuestUtility.DisplayQuestLog(player, quest);
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			AddDescriptionProperty(list);
		}

		public override void AddQuestItemProperty(ObjectPropertyList list)
		{
			list.Add(1060847, $"Quest\tLauncher");
		}

		public virtual void AddDescriptionProperty(ObjectPropertyList list)
		{
			var desc = Description;

			if (!String.IsNullOrWhiteSpace(desc))
			{
				list.Add(1116690, $"<basefont color=#FFD700>\"\t{desc}\t\"<basefont color=#FFFFFF>");
			}
		}

		public virtual void QuestOffered(Quest quest)
		{
		}

		public virtual void QuestAccepted(Quest quest)
		{
		}

		public virtual void QuestDeclined(Quest quest)
		{
		}

		public virtual void QuestCompleted(Quest quest)
		{
		}

		public virtual void QuestRedeemed(Quest quest)
		{
		}

		public virtual void QuestAbandoned(Quest quest)
		{
		}

		public virtual void QuestProgressUpdated(Quest quest, QuestObjective obj)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0);

			writer.Write(_Description);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadEncodedInt();

			_Description = reader.ReadString();
		}
	}
}
