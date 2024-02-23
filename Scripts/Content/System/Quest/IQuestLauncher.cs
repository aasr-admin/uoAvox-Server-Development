using System;

namespace Server.Quests
{
	public interface IQuestLauncher : IEntity
	{
		Type[] Quests { get; }

		bool QuestRandomize { get; }

		void QuestOffered(Quest quest);
		void QuestAccepted(Quest quest);
		void QuestDeclined(Quest quest);
		void QuestCompleted(Quest quest);
		void QuestRedeemed(Quest quest);
		void QuestAbandoned(Quest quest);

		void QuestProgressUpdated(Quest quest, QuestObjective obj);
	}
}
