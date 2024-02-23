using Server.Mobiles;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Server.Quests
{

	public static class QuestRegistry
	{
		private static readonly Type[][] _VerifyArgs =
		{
			new[] { typeof(UID) },
			new[] { typeof(Quest) },
			new[] { typeof(GenericWriter) },
			new[] { typeof(GenericReader) },
		};

		private static readonly Dictionary<UID, Quest> _AddQueue = new();
		private static readonly Dictionary<UID, Quest> _RemoveQueue = new();

		public static Dictionary<UID, Quest> Quests { get; } = new();

		public static QuestStates States { get; } = new();

		public static ReadOnlyCollection<Type> QuestTypes { get; }
		public static ReadOnlyCollection<Type> ObjectiveTypes { get; }

		public static string QuestRootPath => Path.Combine(Core.CurrentSavesDirectory, "Quests");
		public static string QuestIndexPath => Path.Combine(QuestRootPath, "Quests.idx");
		public static string QuestDataPath => Path.Combine(QuestRootPath, "Quests.bin");

		static QuestRegistry()
		{
			var typeofQuest = typeof(Quest);
			var typeofObjective = typeof(QuestObjective);

			var questTypes = new List<Type>();
			var objTypes = new List<Type>();

			foreach (var type in typeofQuest.Assembly.DefinedTypes)
			{
				if (type.IsClass && !type.IsAbstract)
				{
					if (type.IsAssignableTo(typeofQuest))
					{
						questTypes.Add(type);
					}
					else if (type.IsAssignableTo(typeofObjective))
					{
						objTypes.Add(type);
					}
				}
			}

			QuestTypes = new(questTypes);
			ObjectiveTypes = new(objTypes);
		}

		public static void Prepare()
		{
			VerifySerialization();
		}

		[CallPriority(Int64.MinValue)]
		public static void Configure()
		{
			EventSink.WorldPreLoad += () =>
			{
				Persistence.Deserialize(QuestIndexPath, LoadIndex);
			};

			EventSink.WorldLoad += () =>
			{
				Persistence.Deserialize(QuestDataPath, LoadQuests);
			};

			EventSink.WorldPostLoad += () =>
			{
				ProcessSafetyQueues();
			};

			EventSink.WorldSave += e =>
			{
				Persistence.Serialize(QuestIndexPath, SaveIndex);
				Persistence.Serialize(QuestDataPath, SaveQuests);
			};

			EventSink.WorldPostSave += e =>
			{
				ProcessSafetyQueues();
			};

			EventSink.DeleteRequest += e =>
			{
				try
				{
					var m = e.State.Account[e.Index] as PlayerMobile;

					Timer.DelayCall(() =>
					{
						if (m?.Deleted == true)
						{
							foreach (var quest in States.Find(m).ToArray())
							{
								quest.Delete();
							}
						}
					});
				}
				catch
				{
				}
			};
		}

		private static void VerifySerialization()
		{
			foreach (var type in QuestTypes)
			{
				VerifyType(type);
			}

			foreach (var type in ObjectiveTypes)
			{
				VerifyType(type);
			}
		}

		private static void VerifyType(Type t)
		{
			try
			{
				StringBuilder warningSb = null;

				if (t.IsSubclassOf(typeof(Quest)))
				{
					if (t.GetConstructor(_VerifyArgs[0]) == null)
					{
						warningSb ??= new StringBuilder();
						warningSb.AppendLine("       - No serialization constructor");
					}
				}
				else if (t.IsSubclassOf(typeof(QuestObjective)))
				{
					if (t.GetConstructor(_VerifyArgs[1]) == null)
					{
						warningSb ??= new StringBuilder();
						warningSb.AppendLine("       - No serialization constructor");
					}
				}
				else
				{
					return;
				}

				var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

				if (t.GetMethod("Serialize", flags, _VerifyArgs[2]) == null)
				{
					warningSb ??= new StringBuilder();
					warningSb.AppendLine("       - No Serialize() method");
				}

				if (t.GetMethod("Deserialize", flags, _VerifyArgs[3]) == null)
				{
					warningSb ??= new StringBuilder();
					warningSb.AppendLine("       - No Deserialize() method");
				}

				if (warningSb?.Length > 0)
				{
					Console.WriteLine($"Warning: {t}\n{warningSb}");
				}
			}
			catch
			{
				Console.WriteLine($"Warning: Exception in serialization verification of type {t}");
			}
		}

		private static void ProcessSafetyQueues()
		{
			if (World.Volatile)
			{
				Timer.DelayCall(ProcessSafetyQueues);
				return;
			}
			
			if (_AddQueue.Count > 0)
			{
				foreach (var quest in _AddQueue.Values)
				{
					AddQuest(quest);
				}

				_AddQueue.Clear();
			}

			if (_RemoveQueue.Count > 0)
			{
				foreach (var quest in _RemoveQueue.Values)
				{
					RemoveQuest(quest);
				}

				_RemoveQueue.Clear();
			}
		}

		public static void AddQuest(Quest quest)
		{
			if (World.Volatile)
			{
				_AddQueue[quest.UID] = quest;
			}
			else if (States.Add(quest))
			{
				Quests[quest.UID] = quest;
			}
		}

		public static void RemoveQuest(Quest quest)
		{
			if (quest != null)
			{
				if (World.Volatile)
				{
					_RemoveQueue[quest.UID] = quest;
				}
				else
				{
					_ = Quests.Remove(quest.UID);
					_ = States.Remove(quest);
				}
			}
		}

		public static Quest FindQuest(UID uid)
		{
			Quest quest;

			if (World.Volatile)
			{
				_AddQueue.TryGetValue(uid, out quest);
			}
			else
			{
				Quests.TryGetValue(uid, out quest);
			}

			return quest;
		}

		public static void Write(this GenericWriter writer, Quest quest)
		{
			writer.Write(quest?.UID ?? UID.MinusOne);
		}

		public static Quest ReadQuest(this GenericReader reader)
		{
			return FindQuest(reader.ReadUID());
		}

		public static Q ReadQuest<Q>(this GenericReader reader) where Q : Quest
		{
			return ReadQuest(reader) as Q;
		}

		private static void SaveIndex(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			var queue = new Queue<IGrouping<Type, UID>>();

			foreach (var group in Quests.Values.GroupBy(q => q.GetType(), q => q.UID))
			{
				if (group.Key != null)
				{
					queue.Enqueue(group);
				}
			}

			writer.WriteEncodedInt(queue.Count);

			var uids = new HashSet<UID>();

			while (queue.TryDequeue(out var group))
			{
				writer.WriteObjectType(group.Key);

				uids.UnionWith(group);

				writer.WriteEncodedInt(uids.Count);

				foreach (var uid in uids)
				{
					writer.Write(uid);
				}

				uids.Clear();
			}
		}

		private static void LoadIndex(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();

			var errors = new Dictionary<Type, Exception>();

			var typeCount = reader.ReadEncodedInt();

			while (--typeCount >= 0)
			{
				var questType = reader.ReadObjectType();
				var questCount = reader.ReadEncodedInt();

				while (--questCount >= 0)
				{
					var uid = reader.ReadUID();

					if (questType != null)
					{
						try
						{
							var quest = (Quest)Activator.CreateInstance(questType, uid);

							AddQuest(quest);
						}
						catch (Exception error)
						{
							errors[questType] = error;

							questType = null;
						}
					}
				}
			}

			if (errors.Count > 0)
			{
				Utility.PushColor(ConsoleColor.Yellow);

				Console.WriteLine($"Deserialization failed for {errors.Count:N0} quest{(errors.Count != 1 ? "s" : String.Empty)}:");

				Utility.PopColor();
				Utility.PushColor(ConsoleColor.Red);

				foreach (var (questType, error) in errors)
				{
					Console.WriteLine($"{questType}:\n{error}");
				}

				Utility.PopColor();

				errors.Clear();
			}
		}

		private static void SaveQuests(GenericWriter writer)
		{
			States.Serialize(writer);
		}

		private static void LoadQuests(GenericReader reader)
		{
			States.Deserialize(reader);
		}
	}
}
