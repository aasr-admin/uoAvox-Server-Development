using System;
using System.Collections.Generic;
using System.Linq;

using Server.Engines.PartySystem;
using Server.Mobiles;
using Server.Network;

namespace Server.Quests
{
	public abstract class Quest
	{
		private static UID _UID = UID.Zero;

		public static UID NewUID
		{
			get
			{
				do
				{
					++_UID;
				}
				while (QuestRegistry.FindQuest(_UID) != null);

				return _UID;
			}
		}

		#region Pending Expire

		private class ExpireTimer : Timer
		{
			public ExpireTimer()
				: base(TimeSpan.Zero, TimeSpan.FromMinutes(1.0))
			{ }

			protected override void OnTick()
			{
				var index = _Pending.Count;

				if (index > 0)
				{
					var now = DateTime.UtcNow;

					while (--index >= 0)
					{
						var quest = _Pending[index];

						if (quest.Deleted || !quest.IsPending)
						{
							_Pending.RemoveAt(index);
						}
						else if (quest.DateCreated.Add(_PendingExpire) <= now)
						{
							var gump = quest.Owner?.FindGump<QuestLogUI>();

							if (gump?.Quest == quest)
							{
								continue;
							}

							quest.Delete();

							_Pending.RemoveAt(index);
						}
					}
				}
			}
		}

		private static readonly TimeSpan _PendingExpire = TimeSpan.FromMinutes(10.0);

		private static readonly List<Quest> _Pending = new();

		private static readonly ExpireTimer _ExpireTimer = new();

		[CallPriority(Int64.MaxValue)]
		public static void Initialize()
		{
			_ExpireTimer.Start();
		}

		#endregion

		public abstract TextDefinition Name { get; }
		public abstract TextDefinition Lore { get; }

		public abstract TextDefinition MessageOffered { get; }
		public abstract TextDefinition MessageAccepted { get; }
		public abstract TextDefinition MessageDeclined { get; }
		public abstract TextDefinition MessageCompleted { get; }
		public abstract TextDefinition MessageRedeemed { get; }
		public abstract TextDefinition MessageAbandoned { get; }

		private Timer _SliceTimer;

		public virtual TimeSpan SliceInterval { get; } = TimeSpan.Zero;

		public virtual bool Unique { get; } = true;

		public virtual Type[] RequiredQuests { get; } = Type.EmptyTypes;

		public virtual Type NextQuest { get; }

		public UID UID { get; }

		public PlayerMobile Owner { get; private set; }
		public IQuestLauncher Launcher { get; private set; }

		public HashSet<QuestObjective> Objectives { get; } = new();

		public SortedSet<QuestMessage> Messages { get; } = new(QuestMessage.DescendingComparer);

		public int ObjectivesCount => Objectives.Count;
		public int ObjectivesCountCompleted => Objectives.Count(o => o.IsComplete);

		public QuestState State { get; private set; }

		public bool IsPending => State == QuestState.Pending;
		public bool IsActive => State == QuestState.Active;
		public bool IsCompleted => State == QuestState.Completed;
		public bool IsRedeemed => State == QuestState.Redeemed;
		public bool IsAbandoned => State == QuestState.Abandoned;

		public DateTime DateCreated { get; private set; }
		public DateTime DateUpdated { get; private set; }

		public bool Deleted { get; private set; }

		public Quest(PlayerMobile owner, IQuestLauncher launcher)
		{
			UID = NewUID;

			Owner = owner;
			Launcher = launcher;

			DateCreated = DateUpdated = DateTime.UtcNow;

			QuestRegistry.AddQuest(this);
		}

		public Quest(UID uid)
		{
			UID = uid;
		}

		public bool Generate()
		{
			Objectives.Clear();

			OnGenerate();

			return Objectives.Count > 0;
		}

		protected abstract void OnGenerate();

		protected void AddObjective(QuestObjective obj)
		{
			if (!IsPending)
			{
				return;
			}

			if (Objectives.Add(obj))
			{
				obj.OnAdded(this);
			}
		}

		protected void RemoveObjective(QuestObjective obj)
		{
			if (!IsPending)
			{
				return;
			}

			if (Objectives.Remove(obj))
			{
				obj.OnRemoved(this);
			}
		}

		public virtual bool CanOffer()
		{
			return true;
		}

		public bool Offer()
		{
			if (IsPending && CanOffer())
			{
				OnOffered();

				Launcher?.QuestOffered(this);

				QuestEvents.InvokeQuestOffered(this);

				return true;
			}

			return false;
		}

		protected virtual void OnOffered()
		{
			Message(Messages.Count == 0, QuestMessageType.PrivateTalk, MessageOffered);
		}

		public virtual bool CanAccept()
		{
			return true;
		}

		public bool Accept()
		{
			if (IsPending && CanAccept())
			{
				State = QuestState.Active;

				DateUpdated = DateTime.UtcNow;

				_Pending.Remove(this);

				StartSliceTimer();

				OnAccepted();

				Launcher?.QuestAccepted(this);

				QuestEvents.InvokeQuestAccepted(this);

				return true;
			}

			return false;
		}

		protected virtual void OnAccepted()
		{
			Message(true, QuestMessageType.PrivateTalk, MessageAccepted);
		}

		public virtual bool CanDecline()
		{
			return true;
		}

		public bool Decline()
		{
			if (IsPending && CanDecline())
			{
				State = QuestState.Abandoned;

				DateUpdated = DateTime.UtcNow;

				_Pending.Remove(this);

				OnDeclined();

				Launcher?.QuestDeclined(this);

				QuestEvents.InvokeQuestDeclined(this);

				Delete();

				return true;
			}

			return false;
		}

		protected virtual void OnDeclined()
		{
			Message(true, QuestMessageType.PrivateTalk, MessageDeclined);
		}

		public virtual bool CanRedeem()
		{
			return true;
		}

		public bool Redeem()
		{
			if (IsCompleted && CanRedeem())
			{
				State = QuestState.Redeemed;

				DateUpdated = DateTime.UtcNow;

				OnRedeemed();

				Launcher?.QuestRedeemed(this);

				QuestEvents.InvokeQuestRedeemed(this);

				Cleanup();

				return true;
			}

			return false;
		}

		protected virtual void OnRedeemed()
		{
			Message(true, QuestMessageType.PrivateTalk, MessageRedeemed);
		}

		public virtual bool CanAbandon()
		{
			return true;
		}

		public bool Abandon()
		{
			if ((IsActive || IsCompleted) && CanAbandon())
			{
				State = QuestState.Abandoned;

				DateUpdated = DateTime.UtcNow;

				StopSliceTimer();

				OnAbandoned();

				Launcher?.QuestAbandoned(this);

				QuestEvents.InvokeQuestAbandoned(this);

				Delete();

				return true;
			}

			return false;
		}

		protected virtual void OnAbandoned()
		{
			Message(true, QuestMessageType.PrivateTalk, MessageAbandoned);
		}

		public void CheckCompletion()
		{
			if (IsActive && Objectives.All(o => o.IsComplete))
			{
				State = QuestState.Completed;

				DateUpdated = DateTime.UtcNow;

				StopSliceTimer();

				OnCompleted();

				Launcher?.QuestCompleted(this);

				QuestEvents.InvokeQuestCompleted(this);
			}
		}

		protected virtual void OnCompleted()
		{
			Message(true, QuestMessageType.PrivateTalk, MessageCompleted);
		}

		public void ProgressUpdated(QuestObjective obj)
		{
			if (IsActive)
			{
				OnProgressUpdated(obj);

				Launcher?.QuestProgressUpdated(this, obj);

				QuestEvents.InvokeQuestUpdated(this, obj);

				CheckCompletion();
			}
		}

		protected virtual void OnProgressUpdated(QuestObjective obj)
		{
		}

		protected void StartSliceTimer()
		{
			StopSliceTimer();

			var slice = SliceInterval;

			if (slice > TimeSpan.Zero)
			{
				_SliceTimer = Timer.DelayCall(slice, slice, OnSlice);
			}
		}

		protected void StopSliceTimer()
		{
			_SliceTimer?.Stop();
			_SliceTimer = null;
		}

		protected virtual void OnSlice()
		{
		}

		#region Messages

		public void Message(bool log, QuestMessageType type, TextDefinition message)
		{
			Message(log, Owner, type, message);
		}

		public void Message(bool log, QuestMessageType type, int hue, TextDefinition message)
		{
			Message(log, Owner, type, hue, message);
		}

		public void Message(bool log, QuestMessageType type, IEntity source, TextDefinition message)
		{
			Message(log, Owner, type, source, message);
		}

		public void Message(bool log, QuestMessageType type, IEntity source, int hue, TextDefinition message)
		{
			Message(log, Owner, type, source, hue, message);
		}

		public void Message(bool log, Mobile to, QuestMessageType type, TextDefinition message)
		{
			Message(log, to, type, Launcher, message);
		}

		public void Message(bool log, Mobile to, QuestMessageType type, int hue, TextDefinition message)
		{
			Message(log, to, type, Launcher, hue, message);
		}

		public void Message(bool log, Mobile to, QuestMessageType type, IEntity source, TextDefinition message)
		{
			var hue = 0x3B2;

			if (type.HasFlag(QuestMessageType.Talk))
			{
				hue = 0x3B2;
			}
			else if (type.HasFlag(QuestMessageType.Whisper))
			{
				hue = 0x59;
			}
			else if (type.HasFlag(QuestMessageType.Yell))
			{
				hue = 0x35;
			}
			else if (type.HasFlag(QuestMessageType.Emote))
			{
				hue = 0x22;
			}

			Message(log, to, type, source, hue, message);
		}

		public void Message(bool log, Mobile to, QuestMessageType type, IEntity source, int hue, TextDefinition message)
		{
			if (!TextDefinition.IsNullOrEmpty(message))
			{
				Message(log, to, type, source, hue, message.Combine());
			}
		}

		public void Message(bool log, Mobile to, QuestMessageType type, IEntity source, int hue, string message)
		{
			if (to == Owner && type.HasFlag(QuestMessageType.Party))
			{
				type &= ~QuestMessageType.Party;
				type |= QuestMessageType.Private;

				var party = Party.Get(to);

				if (party?.Active == true)
				{
					foreach (var member in party.Members)
					{
						if (member.Mobile != to)
						{
							Message(log, member.Mobile, type, source, hue, message);
						}
					}
				}
			}

			var sys = type.HasFlag(QuestMessageType.System);

			if (sys)
			{
				if (source != null && source != to)
				{
					to.SendMessage(hue, $"[{source.Name}] {message}");
				}
				else
				{
					to.SendMessage(hue, message);
				}

				if (log)
				{
					Messages.Add(new QuestMessage(DateTime.UtcNow, type, hue, source?.Name ?? String.Empty, message));
				}

				return;
			}

			var typ = MessageType.Regular;

			if (type.HasFlag(QuestMessageType.Talk))
			{
				typ = MessageType.Regular;
			}
			else if (type.HasFlag(QuestMessageType.Whisper))
			{
				typ = MessageType.Whisper;
			}
			else if (type.HasFlag(QuestMessageType.Yell))
			{
				typ = MessageType.Yell;
			}
			else if (type.HasFlag(QuestMessageType.Emote))
			{
				typ = MessageType.Emote;
			}

			if (type.HasFlag(QuestMessageType.Private))
			{
				if (source != null && source != to)
				{
					if (source is Mobile sm)
					{
						sm.PrivateOverheadMessage(typ, hue, false, message, to.NetState);
					}
					else if (source is Item si)
					{
						si.PrivateOverheadMessage(typ, hue, false, message, to.NetState);
					}
					else
					{
						to.PrivateOverheadMessage(typ, hue, false, message, to.NetState);
					}
				}
				else
				{
					to.PrivateOverheadMessage(typ, hue, false, message, to.NetState);
				}
			}
			else
			{
				if (source != null && source != to)
				{
					if (source is Mobile sm)
					{
						sm.PublicOverheadMessage(typ, hue, false, message);
					}
					else if (source is Item si)
					{
						si.PublicOverheadMessage(typ, hue, false, message);
					}
					else
					{
						to.PublicOverheadMessage(typ, hue, false, message);
					}
				}
				else
				{
					to.PublicOverheadMessage(typ, hue, false, message);
				}
			}

			if (log)
			{
				Messages.Add(new QuestMessage(DateTime.UtcNow, type, hue, source?.Name ?? String.Empty, message));
			}
		}

		#endregion

		public IEnumerable<QuestObjective> FindActiveObjectives(Predicate<QuestObjective> predicate = null)
		{
			return FindObjectives(o => !o.IsComplete && predicate?.Invoke(o) != false);
		}

		public IEnumerable<QuestObjective> FindObjectives(Predicate<QuestObjective> predicate = null)
		{
			foreach (var obj in Objectives)
			{
				if (predicate?.Invoke(obj) != false)
				{
					yield return obj;
				}
			}
		}

		private readonly HashSet<IEntity> _Cleanup = new();

		protected bool SetCleanup(IEntity e, bool state)
		{
			if (e != Launcher)
			{
				if (!state)
				{
					if (e is Item item)
					{
						item.QuestItem = false;
					}

					return _Cleanup.Remove(e);
				}

				if (e is not PlayerMobile)
				{
					if (e is Item item)
					{
						item.QuestItem = true;
					}

					_ = _Cleanup.Add(e);

					return true;
				}
			}

			return false;
		}

		protected void Cleanup()
		{
			foreach (var e in _Cleanup)
			{
				if (e is not Item o || o.QuestItem)
				{
					e.Delete();
				}
			}

			_Cleanup.Clear();

			if (IsRedeemed)
			{
				if (Launcher is Item item)
				{
					if (item.Movable)
					{
						item.Consume();
					}
					else
					{
						var root = item.RootParent;

						if ((root is Item rootItem && rootItem.Movable) || root is Mobile || item.IsLockedDown || item.IsSecure)
						{
							item.Consume();
						}
					}
				}
				else if (Launcher is Mobile mob)
				{
					if (mob.Spawner == null && (mob is not BaseCreature pet || pet.GetMaster() is not PlayerMobile))
					{
						mob.Delete();
					}
				}
			}

			Launcher = null;
		}

		protected Item ProvideItem(Type type, params object[] args)
		{
			if (IsPending || IsActive)
			{
				var item = Utility.CreateInstance<Item>(type, args);

				if (item?.Deleted == false)
				{
					_ = SetCleanup(item, true);

					return item;
				}
			}

			return null;
		}

		public void Delete()
		{
			if (Deleted)
			{
				return;
			}

			Deleted = true;

			if (IsPending)
			{
				_Pending.Remove(this);
			}

			OnDelete();

			Cleanup();

			QuestRegistry.RemoveQuest(this);
		}

		protected virtual void OnDelete()
		{
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.Write(Owner);
			writer.Write(Launcher);

			writer.Write(State);

			writer.WriteEncodedInt(Objectives.Count);

			foreach (var obj in Objectives)
			{
				writer.WriteObjectType(obj);

				Persistence.SerializeBlock(writer, w => obj?.Serialize(w));
			}

			writer.WriteEncodedInt(Messages.Count);

			foreach (var msg in Messages)
			{
				msg.Serialize(writer);
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();

			Owner = reader.ReadMobile<PlayerMobile>();
			Launcher = reader.ReadEntity<IQuestLauncher>();

			State = reader.ReadEnum<QuestState>();

			var count = reader.ReadEncodedInt();

			while (--count >= 0)
			{
				var type = reader.ReadObjectType();
				var obj = Utility.CreateInstance<QuestObjective>(type, this);

				Persistence.DeserializeBlock(reader, r => obj?.Deserialize(r));

				if (obj != null)
				{
					Objectives.Add(obj);
				}
			}

			var msgs = reader.ReadEncodedInt();

			while (--msgs >= 0)
			{
				Messages.Add(new QuestMessage(reader));
			}
		}
	}
}
