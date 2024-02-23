using Server.Factions;
using Server.Items;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Magery;
using Server.Spells.Ninjitsu;
using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.SkillHandlers
{
	public class Stealing
	{
		public static readonly bool ClassicMode = false;
		public static readonly bool SuspendOnMurder = false;

		public static void Initialize()
		{
			SkillInfo.Table[33].Callback = OnUse;
		}

		public static TimeSpan OnUse(Mobile m)
		{
			if (!IsEmptyHanded(m))
			{
				m.SendLocalizedMessage(1005584); // Both hands must be free to steal.
			}
			else if (m.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)))
			{
				m.SendMessage("You may not steal in this area.");
			}
			else
			{
				m.SendLocalizedMessage(502698); // Which item do you want to steal?
				m.Target = new StealingTarget();
				m.RevealingAction();
			}

			return TimeSpan.FromSeconds(10.0);
		}

		public static bool IsInGuild(Mobile m)
		{
			return m is PlayerMobile pm && pm.NpcGuild == NpcGuild.ThievesGuild;
		}

		public static bool IsInnocentTo(Mobile from, Mobile to)
		{
			return Notoriety.Compute(from, to) == Notoriety.Innocent;
		}

		private static Item TryStealItem(Mobile thief, Item toSteal, ref bool caught)
		{
			Item stolen = null;

			object root = toSteal.RootParent;

			StealableSpawner.StealableInstance si = null;
			if (toSteal.Parent == null || !toSteal.Movable)
			{
				si = StealableSpawner.GetStealableInstance(toSteal);
			}

			if (!IsEmptyHanded(thief))
			{
				thief.SendLocalizedMessage(1005584); // Both hands must be free to steal.
			}
			else if (thief.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)))
			{
				thief.SendMessage("You may not steal in this area.");
			}
			else if (root is PlayerMobile && !IsInGuild(thief))
			{
				thief.SendLocalizedMessage(1005596); // You must be in the thieves guild to steal from other players.
			}
			else if (SuspendOnMurder && root is PlayerMobile && IsInGuild(thief) && thief.Kills > 0)
			{
				thief.SendLocalizedMessage(502706); // You are currently suspended from the thieves guild.
			}
			else if (root is BaseVendor vend && vend.IsInvulnerable)
			{
				thief.SendLocalizedMessage(1005598); // You can't steal from shopkeepers.
			}
			else if (root is PlayerVendor)
			{
				thief.SendLocalizedMessage(502709); // You can't steal from vendors.
			}
			else if (!thief.CanSee(toSteal))
			{
				thief.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (thief.Backpack == null || !thief.Backpack.CheckHold(thief, toSteal, false, true))
			{
				thief.SendLocalizedMessage(1048147); // Your backpack can't hold anything else.
			}
			#region Sigils
			else if (toSteal is Sigil sig)
			{
				var pl = PlayerState.Find(thief);
				var faction = pl?.Faction;

				if (!thief.InRange(sig.GetWorldLocation(), 1))
				{
					thief.SendLocalizedMessage(502703); // You must be standing next to an item to steal it.
				}
				else if (root != null) // not on the ground
				{
					thief.SendLocalizedMessage(502710); // You can't steal that!
				}
				else if (faction != null)
				{
					if (IncognitoSpell.IsIncognito(thief))
					{
						thief.SendLocalizedMessage(1010581); //	You cannot steal the sigil when you are incognito
					}
					else if (DisguiseTimers.IsDisguised(thief))
					{
						thief.SendLocalizedMessage(1010583); //	You cannot steal the sigil while disguised
					}
					else if (PolymorphSpell.IsPolymorphed(thief))
					{
						thief.SendLocalizedMessage(1010582); //	You cannot steal the sigil while polymorphed				
					}
					else if (TransformationSpellHelper.UnderTransformation(thief))
					{
						thief.SendLocalizedMessage(1061622); // You cannot steal the sigil while in that form.
					}
					else if (AnimalFormSpell.UnderTransformation(thief))
					{
						thief.SendLocalizedMessage(1063222); // You cannot steal the sigil while mimicking an animal.
					}
					else if (pl.IsLeaving)
					{
						thief.SendLocalizedMessage(1005589); // You are currently quitting a faction and cannot steal the town sigil
					}
					else if (sig.IsBeingCorrupted && sig.LastMonolith.Faction == faction)
					{
						thief.SendLocalizedMessage(1005590); //	You cannot steal your own sigil
					}
					else if (sig.IsPurifying)
					{
						thief.SendLocalizedMessage(1005592); // You cannot steal this sigil until it has been purified
					}
					else if (thief.CheckTargetSkill(SkillName.Stealing, sig, 80.0, 80.0))
					{
						if (Sigil.ExistsOn(thief))
						{
							thief.SendLocalizedMessage(1010258); //	The sigil has gone back to its home location because you already have a sigil.
						}
						else if (thief.Backpack == null || !thief.Backpack.CheckHold(thief, sig, false, true))
						{
							thief.SendLocalizedMessage(1010259); //	The sigil has gone home because your backpack is full
						}
						else
						{
							if (sig.IsBeingCorrupted)
							{
								sig.GraceStart = DateTime.UtcNow; // begin grace period
							}

							thief.SendLocalizedMessage(1010586); // YOU STOLE THE SIGIL!!!   (woah, calm down now)

							if (sig.LastMonolith != null && sig.LastMonolith.Sigil != null)
							{
								sig.LastMonolith.Sigil = null;
								sig.LastStolen = DateTime.UtcNow;
							}

							return sig;
						}
					}
					else
					{
						thief.SendLocalizedMessage(1005594); //	You do not have enough skill to steal the sigil
					}
				}
				else
				{
					thief.SendLocalizedMessage(1005588); //	You must join a faction to do that
				}
			}
			#endregion
			else if (si == null && (toSteal.Parent == null || !toSteal.Movable))
			{
				thief.SendLocalizedMessage(502710); // You can't steal that!
			}
			else if (toSteal.LootType == LootType.Newbied || toSteal.CheckBlessed(root))
			{
				thief.SendLocalizedMessage(502710); // You can't steal that!
			}
			else if (Core.AOS && si == null && toSteal is Container)
			{
				thief.SendLocalizedMessage(502710); // You can't steal that!
			}
			else if (!thief.InRange(toSteal.GetWorldLocation(), 1))
			{
				thief.SendLocalizedMessage(502703); // You must be standing next to an item to steal it.
			}
			else if (si != null && thief.Skills[SkillName.Stealing].Value < 100.0)
			{
				thief.SendLocalizedMessage(1060025, "", 0x66D); // You're not skilled enough to attempt the theft of this item.
			}
			else if (toSteal.Parent is Mobile)
			{
				thief.SendLocalizedMessage(1005585); // You cannot steal items which are equiped.
			}
			else if (root == thief)
			{
				thief.SendLocalizedMessage(502704); // You catch yourself red-handed.
			}
			else if (root is Mobile rm && (!thief.CanBeHarmful(rm) || rm.AccessLevel > AccessLevel.Player))
			{
				thief.SendLocalizedMessage(502710); // You can't steal that!
			}
			else if (root is Corpse)
			{
				thief.SendLocalizedMessage(502710); // You can't steal that!
			}
			else
			{
				var w = toSteal.Weight + toSteal.TotalWeight;

				if (w > 10)
				{
					thief.SendMessage("That is too heavy to steal.");
				}
				else
				{
					if (toSteal.Stackable && toSteal.Amount > 1)
					{
						var maxAmount = (int)(thief.Skills[SkillName.Stealing].Value / 10.0 / toSteal.Weight);

						if (maxAmount < 1)
						{
							maxAmount = 1;
						}
						else if (maxAmount > toSteal.Amount)
						{
							maxAmount = toSteal.Amount;
						}

						var amount = Utility.RandomMinMax(1, maxAmount);

						if (amount >= toSteal.Amount)
						{
							var pileWeight = (int)Math.Ceiling(toSteal.Weight * toSteal.Amount);
							pileWeight *= 10;

							if (thief.CheckTargetSkill(SkillName.Stealing, toSteal, pileWeight - 22.5, pileWeight + 27.5))
							{
								stolen = toSteal;
							}
						}
						else
						{
							var pileWeight = (int)Math.Ceiling(toSteal.Weight * amount);
							pileWeight *= 10;

							if (thief.CheckTargetSkill(SkillName.Stealing, toSteal, pileWeight - 22.5, pileWeight + 27.5))
							{
								stolen = Mobile.LiftItemDupe(toSteal, toSteal.Amount - amount);

								stolen ??= toSteal;
							}
						}
					}
					else
					{
						var iw = (int)Math.Ceiling(w);
						iw *= 10;

						if (thief.CheckTargetSkill(SkillName.Stealing, toSteal, iw - 22.5, iw + 27.5))
						{
							stolen = toSteal;
						}
					}

					if (stolen != null)
					{
						thief.SendLocalizedMessage(502724); // You succesfully steal the item.

						if (si != null)
						{
							toSteal.Movable = true;
							si.Item = null;
						}
					}
					else
					{
						thief.SendLocalizedMessage(502723); // You fail to steal the item.
					}

					caught = thief.Skills[SkillName.Stealing].Value < Utility.Random(150);
				}
			}

			return stolen;
		}

		public static bool IsEmptyHanded(Mobile from)
		{
			if (from.FindItemOnLayer(Layer.OneHanded) != null)
			{
				return false;
			}

			if (from.FindItemOnLayer(Layer.TwoHanded) != null)
			{
				return false;
			}

			return true;
		}

		private class StealingTarget : Target
		{

			public StealingTarget()
				: base(1, false, TargetFlags.None)
			{
				AllowNonlocal = true;
			}

			protected override void OnTarget(Mobile from, object target)
			{
				from.RevealingAction();

				Item stolen = null;
				object root = null;
				var caught = false;

				if (target is Item item)
				{
					root = item.RootParent;
					stolen = TryStealItem(from, item, ref caught);
				}
				else if (target is Mobile mob)
				{
					var pack = mob.Backpack;

					if (pack != null && pack.Items.Count > 0)
					{
						var randomIndex = Utility.Random(pack.Items.Count);

						root = target;
						stolen = TryStealItem(from, pack.Items[randomIndex], ref caught);
					}
				}
				else
				{
					from.SendLocalizedMessage(502710); // You can't steal that!
				}

				if (stolen != null)
				{
					if (root is Mobile victim)
					{
						EventSink.InvokeStolenItem(new StolenItemEventArgs(from, victim, stolen, stolen.Amount));
					}

					_ = from.AddToBackpack(stolen);

					if (stolen is not Container && !stolen.Stackable)
					{
						// do not return stolen containers or stackable items
						StolenItem.Add(stolen, from, root as Mobile);
					}
				}

				if (caught)
				{
					if (root == null)
					{
						from.CriminalAction(false);
					}
					else if (root is Corpse cRoot && cRoot.IsCriminalAction(from))
					{
						from.CriminalAction(false);
					}
					else if (root is Mobile mobRoot)
					{
						if (!IsInGuild(mobRoot) && IsInnocentTo(from, mobRoot))
						{
							from.CriminalAction(false);
						}

						var message = String.Format("You notice {0} trying to steal from {1}.", from.Name, mobRoot.Name);

						foreach (var ns in from.GetClientsInRange(8))
						{
							if (ns.Mobile != from)
							{
								ns.Mobile.SendMessage(message);
							}
						}
					}
				}
				else if (root is Corpse cRoot && cRoot.IsCriminalAction(from))
				{
					from.CriminalAction(false);
				}

				if (root is PlayerMobile rm && from is PlayerMobile pm && IsInnocentTo(pm, rm) && !IsInGuild(rm))
				{
					pm.PermaFlags.Add(rm);
					pm.Delta(MobileDelta.Noto);
				}
			}
		}
	}

	public class StolenItem
	{
		public static readonly TimeSpan StealTime = TimeSpan.FromMinutes(2.0);

		public Item Stolen { get; }
		public Mobile Thief { get; }
		public Mobile Victim { get; }
		public DateTime Expires { get; private set; }

		public bool IsExpired => DateTime.UtcNow >= Expires;

		public StolenItem(Item stolen, Mobile thief, Mobile victim)
		{
			Stolen = stolen;
			Thief = thief;
			Victim = victim;

			Expires = DateTime.UtcNow + StealTime;
		}

		private static readonly Queue<StolenItem> m_Queue = new();

		public static void Add(Item item, Mobile thief, Mobile victim)
		{
			Clean();

			m_Queue.Enqueue(new StolenItem(item, thief, victim));
		}

		public static bool IsStolen(Item item)
		{
			Mobile victim = null;

			return IsStolen(item, ref victim);
		}

		public static bool IsStolen(Item item, ref Mobile victim)
		{
			Clean();

			foreach (var si in m_Queue)
			{
				if (si.Stolen == item && !si.IsExpired)
				{
					victim = si.Victim;
					return true;
				}
			}

			return false;
		}

		public static void ReturnOnDeath(Mobile killed, Container corpse)
		{
			Clean();

			foreach (var si in m_Queue)
			{
				if (si.Stolen.RootParent == corpse && si.Victim != null && !si.IsExpired)
				{
					if (si.Victim.AddToBackpack(si.Stolen))
					{
						si.Victim.SendLocalizedMessage(1010464); // the item that was stolen is returned to you.
					}
					else
					{
						si.Victim.SendLocalizedMessage(1010463); // the item that was stolen from you falls to the ground.
					}

					si.Expires = DateTime.UtcNow; // such a hack
				}
			}
		}

		public static void Clean()
		{
			var slice = m_Queue.Count;
			
			while (--slice >= 0 && m_Queue.Count > 0)
			{
				var si = m_Queue.Dequeue();

				if (!si.IsExpired)
				{
					m_Queue.Enqueue(si);
				}
			}
		}
	}
}