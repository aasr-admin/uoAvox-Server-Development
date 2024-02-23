using System;
using System.Collections;
using System.Collections.Generic;

using Server.Mobiles;

namespace Server.Quests
{
	public sealed class QuestStates
	{
		private readonly Dictionary<PlayerMobile, HashSet<Quest>> _Quests = new();

		public IEnumerable<Quest> this[PlayerMobile player] => Find(player);

		public QuestStates()
		{
		}

		public bool Add(Quest quest)
		{
			if (quest?.Deleted != false || quest?.Owner == null)
			{
				return false;
			}

			if (!_Quests.TryGetValue(quest.Owner, out var quests))
			{
				_Quests[quest.Owner] = quests = new();
			}

			return quests.Add(quest);
		}

		public bool Remove(Quest quest)
		{
			HashSet<Quest> quests = null;

			var owner = quest?.Owner;

			if (owner == null)
			{
				foreach (var o in _Quests)
				{
					if (o.Value.Contains(quest))
					{
						owner = o.Key;
						quests = o.Value;

						break;
					}
				}
			}

			if (owner == null)
			{
				return false;
			}

			if (quests == null && !_Quests.TryGetValue(owner, out quests))
			{
				return false;
			}

			var removed = quests.Remove(quest);

			if (removed && quests.Count == 0)
			{
				_ = _Quests.Remove(owner);
			}

			return removed;
		}

		public bool Remove(PlayerMobile player)
		{
			return player != null && _Quests.Remove(player);
		}

		public bool Contains(PlayerMobile player)
		{
			return player != null && _Quests.ContainsKey(player);
		}

		public IEnumerable<Quest> FindPending(PlayerMobile player, Predicate<Quest> predicate = null)
		{
			return Find(player, null, q => q.IsPending && predicate?.Invoke(q) != false);
		}

		public IEnumerable<Quest> FindActive(PlayerMobile player, Predicate<Quest> predicate = null)
		{
			return Find(player, null, q => q.IsActive && predicate?.Invoke(q) != false);
		}

		public IEnumerable<Quest> FindCompleted(PlayerMobile player, Predicate<Quest> predicate = null)
		{
			return Find(player, null, q => q.IsCompleted && predicate?.Invoke(q) != false);
		}

		public IEnumerable<Quest> Find(PlayerMobile player, Type type = null, Predicate<Quest> predicate = null)
		{
			if (player != null && _Quests.TryGetValue(player, out var quests))
			{
				foreach (var quest in quests)
				{
					if (type?.IsInstanceOfType(quest) != false && predicate?.Invoke(quest) != false)
					{
						yield return quest;
					}
				}
			}
		}

		public Quest Get(PlayerMobile player, Type type = null, Predicate<Quest> predicate = null)
		{
			if (player != null)
			{
				foreach (var quest in Find(player, type, predicate))
				{
					return quest;
				}
			}

			return null;
		}

		public bool Exists(PlayerMobile player, Type type = null, Predicate<Quest> predicate = null)
		{
			return Get(player, type, predicate) != null;
		}

		public IEnumerable<Q> Find<Q>(PlayerMobile player, Predicate<Q> predicate = null)
		{
			if (player != null && _Quests.TryGetValue(player, out var quests))
			{
				foreach (var quest in quests)
				{
					if (quest is Q q && predicate?.Invoke(q) != false)
					{
						yield return q;
					}
				}
			}
		}

		public Q Get<Q>(PlayerMobile player, Predicate<Q> predicate = null) where Q : Quest
		{
			if (player != null)
			{
				foreach (var quest in Find(player, predicate))
				{
					return quest;
				}
			}

			return null;
		}

		public bool Exists<Q>(PlayerMobile player, Predicate<Q> predicate = null) where Q : Quest
		{
			return Get(player, predicate) != null;
		}

		public Quest CreateQuest(Type type, PlayerMobile player, IQuestLauncher launcher = null)
		{
			if (type == null || type.IsAbstract)
			{
				return null;
			}

			var quest = Get(player, type);

			if (quest != null)
			{
				if (quest.IsPending || quest.IsActive || quest.IsCompleted || quest.Unique)
				{
					return quest;
				}

				quest.Delete();
			}

			quest = Utility.CreateInstance<Quest>(type, player, launcher);

			if (quest != null)
			{
				if (quest.RequiredQuests?.Length > 0)
				{
					foreach (var req in quest.RequiredQuests)
					{
						if (!Exists(player, req))
						{
							quest.Delete();

							break;
						}
					}
				}

				if (!quest.Deleted && !quest.Generate())
				{
					quest.Delete();
				}
			}

			if (quest?.Deleted == false)
			{
				return quest;
			}

			return null;
		}

		public Q CreateQuest<Q>(PlayerMobile player, IQuestLauncher launcher = null) where Q : Quest
		{
			var type = typeof(Q);

			if (type.IsAbstract)
			{
				return null;
			}

			var quest = Get<Q>(player, null);

			if (quest != null)
			{
				if (quest.IsPending || quest.IsActive || quest.IsCompleted || quest.Unique)
				{
					return quest;
				}

				quest.Delete();
			}

			quest = Utility.CreateInstance<Q>(typeof(Q), player, launcher);

			if (quest != null)
			{
				if (quest.RequiredQuests?.Length > 0)
				{
					foreach (var req in quest.RequiredQuests)
					{
						if (!Exists(player, req))
						{
							quest.Delete();

							break;
						}
					}
				}

				if (!quest.Deleted && !quest.Generate())
				{
					quest.Delete();
				}
			}

			if (quest?.Deleted == false)
			{
				return quest;
			}

			return null;
		}

		public Quest CreateQuest(IEnumerable<Type> types, bool random, PlayerMobile player, IQuestLauncher launcher = null)
		{
			if (types == null || (types is ICollection col && col.Count == 0))
			{
				return null;
			}

			var set = new List<Type>(types);

			try
			{
				while (set.Count > 0)
				{
					var index = random ? Utility.Random(set.Count) : set.Count - 1;

					var quest = CreateQuest(set[index], player, launcher);

					if (quest?.Deleted == false)
					{
						return quest;
					}

					set.RemoveAt(index);
				}

				return null;
			}
			finally
			{
				set.Clear();
				set.TrimExcess();
			}
		}

		public Quest CreateQuest(PlayerMobile player, IQuestLauncher launcher)
		{
			if (player != null && launcher != null)
			{
				return CreateQuest(launcher.Quests, launcher.QuestRandomize, player, launcher);
			}

			return null;
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.WriteEncodedInt(_Quests.Count);

			foreach (var (player, quests) in _Quests)
			{
				writer.Write(player);

				writer.WriteEncodedInt(quests.Count);

				foreach (var quest in quests)
				{
					writer.Write(quest);

					Persistence.SerializeBlock(writer, w => quest?.Serialize(w));
				}
			}
		}

		public void Deserialize(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();

			var count = reader.ReadEncodedInt();

			while (--count >= 0)
			{
				var player = reader.ReadMobile<PlayerMobile>();

				var states = reader.ReadEncodedInt();

				if (states > 0)
				{
					var quests = new HashSet<Quest>(states);

					while (--states >= 0)
					{
						var quest = reader.ReadQuest();

						Persistence.DeserializeBlock(reader, r => quest?.Deserialize(r));

						if (player?.Deleted != false)
						{
							quest?.Delete();
						}

						if (quest?.Deleted == false)
						{
							_ = quests.Add(quest);
						}
					}

					if (player?.Deleted == false && quests.Count > 0)
					{
						_Quests[player] = quests;
					}
				}
			}
		}
	}
}
