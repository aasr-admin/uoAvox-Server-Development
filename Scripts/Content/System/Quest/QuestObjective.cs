using System;
using System.Collections.Generic;

using Server.ContextMenus;
using Server.Mobiles;
using Server.Network;

namespace Server.Quests
{
	public abstract class QuestObjective
	{
		public Quest Quest { get; private set; }

		public PlayerMobile Owner => Quest?.Owner;

		public abstract TextDefinition Title { get; }
		public abstract TextDefinition Summary { get; }

		public abstract QuestArea[] Locations { get; }

		public abstract double ProgressRequired { get; }

		public double Progress { get; private set; }

		public bool IsComplete => Progress >= ProgressRequired;

		public QuestObjective()
		{
		}

		public QuestObjective(Quest quest)
		{
			Quest = quest;
		}

		public void ForceCompletion()
		{
			UpdateProgress(ProgressRequired, false);
		}

		public void UpdateProgress(double offset)
		{
			UpdateProgress(offset, true);
		}

		public void UpdateProgress(double offset, bool relative)
		{
			var old = Progress;

			Progress = Math.Clamp(relative ? (Progress + offset) : offset, 0.0, ProgressRequired);

			if (Progress != old)
			{
				Quest?.ProgressUpdated(this);
			}
		}

		public bool InBounds(IEntity entity)
		{
			return Array.Exists(Locations, loc => loc.InBounds(entity));
		}

		public bool InBounds(Map map, IPoint3D point)
		{
			return Array.Exists(Locations, loc => loc.InBounds(map, point));
		}

		#region Handlers

		public virtual int HandleContextMenuRequest(IEntity owner, List<ContextMenuEntry> entries)
		{
			return 0;
		}

		public virtual int HandleContextMenuResponse(IEntity owner, ContextMenuEntry entry)
		{
			return 0;
		}

		public virtual int HandleDeath(Mobile[] killers)
		{
			return 0;
		}

		public virtual int HandleKill(Mobile killed)
		{
			return 0;
		}

		public virtual int HandleSaidSpeech(MessageType type, int hue, string said, int[] keywords)
		{
			return 0;
		}

		public virtual int HandleHearSpeech(Mobile speaker, MessageType type, int hue, string said, int[] keywords)
		{
			return 0;
		}

		public virtual int HandleMoving(Direction dir)
		{
			return 0;
		}

		public virtual int HandleObtainedItem(Item item)
		{
			return 0;
		}

		public virtual int HandleDiscardedItem(Item item)
		{
			return 0;
		}

		public virtual int HandleCraftedItem(ICraftSystem system, ICraftItem craft, ICraftTool tool, Item item, int amount)
		{
			return 0;
		}

		public virtual int HandleHarvestedItem(IHarvestSystem system, IHarvestTool tool, Item item, int amount)
		{
			return 0;
		}

		public virtual int HandleStealItem(Item item, int amount, Mobile victim)
		{
			return 0;
		}

		public virtual int HandleStolenItem(Item item, int amount, Mobile thief)
		{
			return 0;
		}

		public virtual int HandleFeedCreature(Item food, int amount, Mobile creature)
		{
			return 0;
		}

		public virtual int HandleGiveGold(Item gold, int amount, Mobile receiver)
		{
			return 0;
		}

		public virtual int HandleCastSpell(ISpell spell)
		{
			return 0;
		}

		public virtual int HandleAggressed(Mobile aggressed, bool criminal)
		{
			return 0;
		}

		public virtual int HandleAggressor(Mobile aggressor, bool criminal)
		{
			return 0;
		}

		public virtual int HandleHungerChanged(int oldAmount)
		{
			return 0;
		}

		public virtual int HandleTakeDamage(Mobile source, int amount)
		{
			return 0;
		}

		public virtual int HandleGiveDamage(Mobile target, int amount)
		{
			return 0;
		}

		public virtual int HandleTakeHeal(Mobile source, int amount)
		{
			return 0;
		}

		public virtual int HandleGiveHeal(Mobile target, int amount)
		{
			return 0;
		}

		public virtual int HandleCreatureTamed(Mobile tamed)
		{
			return 0;
		}

		#endregion

		public virtual void OnAdded(Quest owner)
		{
			Quest = owner;
		}

		public virtual void OnRemoved(Quest owner)
		{
			Quest = null;
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.Write(Quest);

			writer.Write(Progress);
		}

		public virtual void Deserialize(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();

			Quest = reader.ReadQuest();

			Progress = reader.ReadDouble();
		}
	}
}
