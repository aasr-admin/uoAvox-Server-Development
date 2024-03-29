﻿using Server.Spells.Magery;
using Server.Targeting;

namespace Server.Mobiles
{
	public class HealerAI : BaseAI
	{
		private static readonly NeedDelegate m_Cure = new NeedDelegate(NeedCure);
		private static readonly NeedDelegate m_GHeal = new NeedDelegate(NeedGHeal);
		private static readonly NeedDelegate m_LHeal = new NeedDelegate(NeedLHeal);
		private static readonly NeedDelegate[] m_ACure = new NeedDelegate[] { m_Cure };
		private static readonly NeedDelegate[] m_AGHeal = new NeedDelegate[] { m_GHeal };
		private static readonly NeedDelegate[] m_ALHeal = new NeedDelegate[] { m_LHeal };
		private static readonly NeedDelegate[] m_All = new NeedDelegate[] { m_Cure, m_GHeal, m_LHeal };

		public HealerAI(BaseCreature m) : base(m)
		{
		}

		public override bool Think()
		{
			if (m_Mobile.Deleted)
			{
				return false;
			}

			var targ = m_Mobile.Target;

			if (targ != null)
			{
				if (targ is CureSpell.InternalTarget)
				{
					ProcessTarget(targ, m_ACure);
				}
				else if (targ is GreaterHealSpell.InternalTarget)
				{
					ProcessTarget(targ, m_AGHeal);
				}
				else if (targ is HealSpell.InternalTarget)
				{
					ProcessTarget(targ, m_ALHeal);
				}
				else
				{
					targ.Cancel(m_Mobile, TargetCancelType.Canceled);
				}
			}
			else
			{
				var toHelp = Find(m_All);

				if (toHelp != null)
				{
					if (NeedCure(toHelp))
					{
						if (m_Mobile.Debug)
						{
							m_Mobile.DebugSay("{0} needs a cure", toHelp.Name);
						}

						if (!(new CureSpell(m_Mobile, null)).Cast())
						{
							new CureSpell(m_Mobile, null).Cast();
						}
					}
					else if (NeedGHeal(toHelp))
					{
						if (m_Mobile.Debug)
						{
							m_Mobile.DebugSay("{0} needs a greater heal", toHelp.Name);
						}

						if (!(new GreaterHealSpell(m_Mobile, null)).Cast())
						{
							new HealSpell(m_Mobile, null).Cast();
						}
					}
					else if (NeedLHeal(toHelp))
					{
						if (m_Mobile.Debug)
						{
							m_Mobile.DebugSay("{0} needs a lesser heal", toHelp.Name);
						}

						new HealSpell(m_Mobile, null).Cast();
					}
				}
				else
				{
					if (AcquireFocusMob(m_Mobile.RangePerception, FightMode.Weakest, false, true, false))
					{
						WalkMobileRange(m_Mobile.FocusMob, 1, false, 4, 7);
					}
					else
					{
						WalkRandomInHome(3, 2, 1);
					}
				}
			}

			return true;
		}

		private delegate bool NeedDelegate(Mobile m);

		private void ProcessTarget(Target targ, NeedDelegate[] func)
		{
			var toHelp = Find(func);

			if (toHelp != null)
			{
				if (targ.Range != -1 && !m_Mobile.InRange(toHelp, targ.Range))
				{
					DoMove(m_Mobile.GetDirectionTo(toHelp) | Direction.Running);
				}
				else
				{
					targ.Invoke(m_Mobile, toHelp);
				}
			}
			else
			{
				targ.Cancel(m_Mobile, TargetCancelType.Canceled);
			}
		}

		private Mobile Find(params NeedDelegate[] funcs)
		{
			if (m_Mobile.Deleted)
			{
				return null;
			}

			var map = m_Mobile.Map;

			if (map != null)
			{
				var prio = 0.0;
				Mobile found = null;

				foreach (var m in m_Mobile.GetMobilesInRange(m_Mobile.RangePerception))
				{
					if (!m_Mobile.CanSee(m) || !(m is BaseCreature) || ((BaseCreature)m).Team != m_Mobile.Team)
					{
						continue;
					}

					for (var i = 0; i < funcs.Length; ++i)
					{
						if (funcs[i](m))
						{
							var val = -m_Mobile.GetDistanceToSqrt(m);

							if (found == null || val > prio)
							{
								prio = val;
								found = m;
							}

							break;
						}
					}
				}

				return found;
			}

			return null;
		}

		private static bool NeedCure(Mobile m)
		{
			return m.Poisoned;
		}

		private static bool NeedGHeal(Mobile m)
		{
			return m.Hits < m.HitsMax - 40;
		}

		private static bool NeedLHeal(Mobile m)
		{
			return m.Hits < m.HitsMax - 10;
		}
	}
}