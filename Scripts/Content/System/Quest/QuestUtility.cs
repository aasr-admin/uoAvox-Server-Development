using System;
using System.Collections.Generic;

using Server.Mobiles;

namespace Server.Quests
{
	public static class QuestUtility
	{
		public static QuestLogUI DisplayQuestLog(PlayerMobile player, Quest quest = null)
		{
			return QuestLogUI.DisplayTo(player, quest);
		}

		public static IEnumerable<QuestObjective> FindActiveObjectives(PlayerMobile player, Predicate<QuestObjective> predicate = null)
		{
			foreach (var quest in FindActiveQuests(player))
			{
				foreach (var obj in quest.FindActiveObjectives(predicate))
				{
					yield return obj;
				}
			}
		}

		public static IEnumerable<QuestObjective> FindObjectives(PlayerMobile player, Predicate<QuestObjective> predicate = null)
		{
			foreach (var quest in FindQuests(player))
			{
				foreach (var obj in quest.FindObjectives(predicate))
				{
					yield return obj;
				}
			}
		}

		public static IEnumerable<Quest> FindPendingQuests(PlayerMobile player, Predicate<Quest> predicate = null)
		{
			return QuestRegistry.States.FindPending(player, predicate);
		}

		public static IEnumerable<Quest> FindActiveQuests(PlayerMobile player, Predicate<Quest> predicate = null)
		{
			return QuestRegistry.States.FindActive(player, predicate);
		}

		public static IEnumerable<Quest> FindCompletedQuests(PlayerMobile player, Predicate<Quest> predicate = null)
		{
			return QuestRegistry.States.FindCompleted(player, predicate);
		}

		public static IEnumerable<Quest> FindQuests(PlayerMobile player, Predicate<Quest> predicate = null)
		{
			return QuestRegistry.States.Find(player, predicate);
		}

		public static IEnumerable<Q> FindQuests<Q>(PlayerMobile player, Predicate<Q> predicate = null)
		{
			return QuestRegistry.States.Find(player, predicate);
		}

		public static Quest GetQuest(PlayerMobile player, Type type, Predicate<Quest> predicate = null)
		{
			return QuestRegistry.States.Get(player, type, predicate);
		}

		public static Q GetQuest<Q>(PlayerMobile player, Predicate<Q> predicate = null) where Q : Quest
		{
			return QuestRegistry.States.Get(player, predicate);
		}

		public static bool HasQuest(PlayerMobile player, Type type, Predicate<Quest> predicate = null)
		{
			return QuestRegistry.States.Exists(player, type, predicate);
		}

		public static bool HasQuest<Q>(PlayerMobile player, Predicate<Q> predicate = null) where Q : Quest
		{
			return QuestRegistry.States.Exists(player, predicate);
		}

		public static Quest CreateQuest(PlayerMobile player, IQuestLauncher launcher)
		{
			return QuestRegistry.States.CreateQuest(player, launcher);
		}

		public static Quest CreateQuest(IEnumerable<Type> types, bool random, PlayerMobile player, IQuestLauncher launcher = null)
		{
			return QuestRegistry.States.CreateQuest(types, random, player, launcher);
		}

		public static Quest CreateQuest(Type type, PlayerMobile player, IQuestLauncher launcher = null)
		{
			return QuestRegistry.States.CreateQuest(type, player, launcher);
		}

		public static Q CreateQuest<Q>(PlayerMobile player, IQuestLauncher launcher = null) where Q : Quest
		{
			return QuestRegistry.States.CreateQuest<Q>(player, launcher);
		}
	}
}
