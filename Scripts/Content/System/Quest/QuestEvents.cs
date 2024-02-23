using Server.Engines.PartySystem;
using Server.Mobiles;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Quests
{
	public static class QuestEvents
	{
		public static event Action<Quest> QuestOffered;
		public static event Action<Quest> QuestAccepted;
		public static event Action<Quest> QuestDeclined;
		public static event Action<Quest> QuestCompleted;
		public static event Action<Quest> QuestRedeemed;
		public static event Action<Quest> QuestAbandoned;
		public static event Action<Quest, QuestObjective> QuestUpdated;

		public static void InvokeQuestOffered(Quest quest)
		{
			QuestOffered?.Invoke(quest);
		}

		public static void InvokeQuestAccepted(Quest quest)
		{
			QuestAccepted?.Invoke(quest);
		}

		public static void InvokeQuestDeclined(Quest quest)
		{
			QuestDeclined?.Invoke(quest);
		}

		public static void InvokeQuestCompleted(Quest quest)
		{
			QuestCompleted?.Invoke(quest);
		}

		public static void InvokeQuestRedeemed(Quest quest)
		{
			QuestRedeemed?.Invoke(quest);
		}

		public static void InvokeQuestAbandoned(Quest quest)
		{
			QuestAbandoned?.Invoke(quest);
		}

		public static void InvokeQuestUpdated(Quest quest, QuestObjective obj)
		{
			QuestUpdated?.Invoke(quest, obj);
		}

		[CallPriority(Int64.MaxValue)]
		public static void Initialize()
		{
			EventSink.PlayerDeath += OnPlayerDeath;
			EventSink.CreatureDeath += OnCreatureDeath;
			EventSink.Movement += OnMovement;
			EventSink.CastSpellSuccess += OnCastSpell;
			EventSink.AggressiveAction += OnAggressiveAction;
			EventSink.HungerChanged += OnHungerChanged;
			EventSink.Speech += OnSpeech;
			EventSink.Heard += OnHeard;
			EventSink.ParentChanged += OnParentChanged;
			EventSink.CraftedItem += OnCraftedItem;
			EventSink.HarvestedItem += OnHarvestedItem;
			EventSink.StolenItem += OnStolenItem;
			EventSink.FeedCreature += OnFeedCreature;
			EventSink.GiveGold += OnGiveGold;
			EventSink.MobileDamaged += OnMobileDamaged;
			EventSink.MobileHealed += OnMobileHealed;
			EventSink.CreatureTamed += OnCreatureTamed;
			EventSink.ContextMenuRequest += OnContextMenuRequest;
			EventSink.ContextMenuResponse += OnContextMenuResponse;
		}

		private static Mobile GetDamageMaster(Mobile m)
		{
			var o = m;

			while (m is BaseCreature c)
			{
				m = c.GetMaster();
			}

			return m ?? o;
		}

		private static IEnumerable<Mobile> GetKillers(Mobile dead)
		{
			return dead.DamageEntries.Where(de => !de.HasExpired).Select(de => GetDamageMaster(de.Damager)).Distinct();
		}

		private static IEnumerable<T> GetKillers<T>(Mobile dead) where T : Mobile
		{
			return GetKillers(dead).OfType<T>();
		}

		private static void OnPlayerDeath(PlayerDeathEventArgs e)
		{
			if (e.Mobile is PlayerMobile killed)
			{
				var killers = GetKillers(killed).ToArray();

				foreach (var o in killers.OfType<PlayerMobile>().GroupBy(Party.Get))
				{
					if (o.Key == null)
					{
						foreach (var killer in o)
						{
							ExecuteAction(killer, false, obj => obj.HandleKill(e.Mobile));
						}
					}
					else
					{
						foreach (var killer in o)
						{
							ExecuteAction(killer, true, obj => obj.HandleKill(e.Mobile));
							break;
						}
					}
				}

				ExecuteAction(killed, false, obj => obj.HandleDeath(killers));
			}
		}

		private static void OnCreatureDeath(CreatureDeathEventArgs e)
		{
			foreach (var o in GetKillers<PlayerMobile>(e.Mobile).GroupBy(Party.Get))
			{
				if (o.Key == null)
				{
					foreach (var killer in o)
					{
						ExecuteAction(killer, false, obj => obj.HandleKill(e.Mobile));
					}
				}
				else
				{
					foreach (var killer in o)
					{
						ExecuteAction(killer, true, obj => obj.HandleKill(e.Mobile));
						break;
					}
				}
			}
		}

		private static void OnMovement(MovementEventArgs e)
		{
			if (!e.Blocked && e.Mobile is PlayerMobile moving)
			{
				ExecuteAction(moving, false, obj => obj.HandleMoving(e.Direction));
			}
		}

		private static void OnCastSpell(CastSpellSuccessEventArgs e)
		{
			if (e.Mobile is PlayerMobile caster)
			{
				ExecuteAction(caster, false, obj => obj.HandleCastSpell(e.Spell));
			}
		}

		private static void OnAggressiveAction(AggressiveActionEventArgs e)
		{
			if (e.Aggressor is PlayerMobile aggressor)
			{
				ExecuteAction(aggressor, false, obj => obj.HandleAggressed(e.Aggressed, e.Criminal));
			}

			if (e.Aggressed is PlayerMobile aggressed)
			{
				ExecuteAction(aggressed, false, obj => obj.HandleAggressor(e.Aggressor, e.Criminal));
			}
		}

		private static void OnHungerChanged(HungerChangedEventArgs e)
		{
			if (e.Mobile is PlayerMobile player)
			{
				ExecuteAction(player, false, obj => obj.HandleHungerChanged(e.OldValue));
			}
		}

		private static void OnSpeech(SpeechEventArgs e)
		{
			if (!e.Blocked && e.Mobile is PlayerMobile speaker)
			{
				ExecuteAction(speaker, false, obj => obj.HandleSaidSpeech(e.Type, e.Hue, e.Speech, e.Keywords));
			}
		}

		private static void OnHeard(HeardEventArgs e)
		{
			if (!e.Blocked && e.Mobile is PlayerMobile listener)
			{
				ExecuteAction(listener, false, obj => obj.HandleHearSpeech(e.Speaker, e.Type, e.Hue, e.Speech, e.Keywords));
			}
		}

		private static void OnParentChanged(ParentChangedEventArgs e)
		{
			PlayerMobile oldRoot;

			if (e.OldParent is Item oldItemParent)
			{
				oldRoot = oldItemParent.RootParent as PlayerMobile;
			}
			else
			{
				oldRoot = e.OldParent as PlayerMobile;
			}

			PlayerMobile newRoot;

			if (e.NewParent is Item newItemParent)
			{
				newRoot = newItemParent.RootParent as PlayerMobile;
			}
			else
			{
				newRoot = e.NewParent as PlayerMobile;
			}

			if (oldRoot == newRoot)
			{
				return;
			}

			if (oldRoot == null)
			{
				ExecuteAction(newRoot, false, obj => obj.HandleObtainedItem(e.Item));
			}
			
			if (newRoot == null)
			{
				ExecuteAction(oldRoot, false, obj => obj.HandleDiscardedItem(e.Item));
			}
		}

		private static void OnCraftedItem(CraftedItemEventArgs e)
		{
			if (e.Mobile is PlayerMobile owner)
			{
				ExecuteAction(owner, false, obj => obj.HandleCraftedItem(e.System, e.Craft, e.Tool, e.Item, e.Amount));
			}
		}

		private static void OnHarvestedItem(HarvestedItemEventArgs e)
		{
			if (e.Mobile is PlayerMobile owner)
			{
				ExecuteAction(owner, false, obj => obj.HandleHarvestedItem(e.System, e.Tool, e.Item, e.Amount));
			}
		}

		private static void OnStolenItem(StolenItemEventArgs e)
		{
			if (e.Thief is PlayerMobile thief)
			{
				ExecuteAction(thief, false, obj => obj.HandleStealItem(e.Item, e.Amount, e.Victim));
			}

			if (e.Victim is PlayerMobile victim)
			{
				ExecuteAction(victim, false, obj => obj.HandleStolenItem(e.Item, e.Amount, e.Thief));
			}
		}

		private static void OnFeedCreature(FeedCreatureEventArgs e)
		{
			if (e.Mobile is PlayerMobile player)
			{
				ExecuteAction(player, false, obj => obj.HandleFeedCreature(e.Food, e.Amount, e.Creature));
			}
		}

		private static void OnGiveGold(GiveGoldEventArgs e)
		{
			if (e.Mobile is PlayerMobile player)
			{
				ExecuteAction(player, false, obj => obj.HandleGiveGold(e.Gold, e.Amount, e.Receiver));
			}
		}

		private static void OnMobileDamaged(MobileDamagedEventArgs e)
		{
			if (e.Source is PlayerMobile source)
			{
				ExecuteAction(source, false, obj => obj.HandleGiveDamage(e.Target, e.Amount));
			}

			if (e.Target is PlayerMobile target)
			{
				ExecuteAction(target, false, obj => obj.HandleTakeDamage(e.Source, e.Amount));
			}
		}

		private static void OnMobileHealed(MobileHealedEventArgs e)
		{
			if (e.Source is PlayerMobile source)
			{
				ExecuteAction(source, false, obj => obj.HandleGiveHeal(e.Target, e.Amount));
			}

			if (e.Target is PlayerMobile target)
			{
				ExecuteAction(target, false, obj => obj.HandleTakeHeal(e.Source, e.Amount));
			}
		}

		private static void OnCreatureTamed(CreatureTamedEventArgs e)
		{
			if (e.Tamer is PlayerMobile tamer)
			{
				ExecuteAction(tamer, false, obj => obj.HandleCreatureTamed(e.Creature));
			}
		}

		private static void OnContextMenuRequest(ContextMenuRequestEventArgs e)
		{
			if (e.Mobile is PlayerMobile player)
			{
				ExecuteAction(player, false, obj => obj.HandleContextMenuRequest(e.Owner, e.Entries));
			}
		}

		private static void OnContextMenuResponse(ContextMenuResponseEventArgs e)
		{
			if (!e.Blocked && e.Mobile is PlayerMobile player)
			{
				ExecuteAction(player, false, obj => obj.HandleContextMenuResponse(e.Owner, e.Entry));
			}
		}

		private static void ExecuteAction(PlayerMobile player, bool party, Func<QuestObjective, int> action)
		{
			if (player == null)
			{
				return;
			}

			foreach (var quest in QuestUtility.FindActiveQuests(player))
			{
				foreach (var obj in quest.FindActiveObjectives())
				{
					var progress = action(obj);

					if (progress != 0)
					{
						obj.UpdateProgress(progress);
					}
				}
			}

			if (party)
			{
				var p = Party.Get(player);

				if (p?.Active == true)
				{
					foreach (var m in p.Members)
					{
						if (m.Mobile is PlayerMobile member && member != player && member.InRange(player, Core.GlobalMaxUpdateRange))
						{
							ExecuteAction(member, false, action);
						}
					}
				}
			}
		}
	}
}
