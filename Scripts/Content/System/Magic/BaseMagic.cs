using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Spells.Bushido;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Racial;
using Server.Spells.Magery;
using Server.Spells.Spellweaving;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Server.Targeting;

namespace Server.Spells
{
	public abstract class Spell : ISpell
	{
		private static readonly TimeSpan m_NextSpellDelay = TimeSpan.FromSeconds(0.75);
		private static readonly TimeSpan m_AnimateDelay = TimeSpan.FromSeconds(1.5);

		public Mobile Caster { get; }
		public Item Scroll { get; }
		public SpellInfo Info { get; }

		public SpellName ID => Info.ID;
		public SpellSchool School => Info.School;
		public SpellCircle Circle => Info.Circle;

		public string Name => Info.Name ?? String.Empty;
		public string Desc => Info.Desc ?? String.Empty;
		public string Mantra => Info.Mantra ?? String.Empty;

		public SpellReagents Reagents => Info.Reagents;

		public SpellState State { get; set; }

		public long StartCastTime { get; private set; }

		public abstract SkillName CastSkill { get; }
		public abstract SkillName DamageSkill { get; }

		public virtual bool RevealOnCast => true;
		public virtual bool ClearHandsOnCast => true;
		public virtual bool ShowHandMovement => true;

		public virtual bool DelayedDamage => false;
		public virtual bool DelayedDamageStacking => true;

		//In reality, it's ANY delayed Damage spell Post-AoS that can't stack, but, only 
		//Expo & Magic Arrow have enough delay and a short enough cast time to bring up 
		//the possibility of stacking 'em.  Note that a MA & an Explosion will stack, but
		//of course, two MA's won't.

		private static readonly Dictionary<Type, DelayedDamageContextWrapper> m_ContextTable = new();

		private sealed class DelayedDamageContextWrapper
		{
			private readonly Dictionary<Mobile, Timer> m_Contexts = new();

			public void Add(Mobile m, Timer t)
			{
				if (m_Contexts.TryGetValue(m, out var oldTimer))
				{
					oldTimer?.Stop();

					m_Contexts.Remove(m);
				}

				m_Contexts.Add(m, t);
			}

			public void Remove(Mobile m)
			{
				m_Contexts.Remove(m);
			}
		}

		public void StartDelayedDamageContext(Mobile m, Timer t)
		{
			if (DelayedDamageStacking)
			{
				return;
			}

			DelayedDamageContextWrapper contexts;

			if (!m_ContextTable.TryGetValue(GetType(), out contexts))
			{
				m_ContextTable[GetType()] = contexts = new DelayedDamageContextWrapper();
			}

			contexts.Add(m, t);
		}

		public void RemoveDelayedDamageContext(Mobile m)
		{
			DelayedDamageContextWrapper contexts;

			if (m_ContextTable.TryGetValue(GetType(), out contexts))
			{
				contexts.Remove(m);
			}
		}

		public void HarmfulSpell(Mobile m)
		{
			if (m is BaseCreature c)
			{
				c.OnHarmfulSpell(Caster);
			}
		}

		public Spell(Mobile caster, Item scroll, SpellName id)
			: this(caster, scroll, SpellRegistry.GetInfo(id))
		{
		}

		public Spell(Mobile caster, Item scroll, SpellInfo info)
		{
			Caster = caster;
			Scroll = scroll;
			Info = info;

			Info ??= SpellRegistry.GetInfo(GetType()) ?? SpellInfo.CreateInvalid();
		}

		public virtual int GetNewAosDamage(int bonus, int dice, int sides, Mobile singleTarget)
		{
			if (singleTarget != null)
			{
				return GetNewAosDamage(bonus, dice, sides, Caster.Player && singleTarget.Player, GetDamageScalar(singleTarget));
			}

			return GetNewAosDamage(bonus, dice, sides, false);
		}

		public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer)
		{
			return GetNewAosDamage(bonus, dice, sides, playerVsPlayer, 1.0);
		}

		public virtual int GetNewAosDamage(int bonus, int dice, int sides, bool playerVsPlayer, double scalar)
		{
			var damage = Utility.Dice(dice, sides, bonus) * 100;
			var damageBonus = 0;

			var inscribeSkill = GetInscribeFixed(Caster);

			damageBonus += (inscribeSkill + (1000 * (inscribeSkill / 1000))) / 200;
			damageBonus += Caster.Int / 10;

			var sdiBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);

			// PvP spell damage increase cap of 15% from an item’s magic property
			if (playerVsPlayer && sdiBonus > 15)
			{
				sdiBonus = 15;
			}

			damageBonus += sdiBonus;

			var context = TransformationSpellHelper.GetContext(Caster);

			if (context != null && context.Spell is ReaperFormSpell rf)
			{
				damageBonus += rf.SpellDamageBonus;
			}

			damage = AOS.Scale(damage, 100 + damageBonus);

			var evalSkill = GetDamageFixed(Caster);
			var evalScale = 30 + (9 * evalSkill / 100);

			damage = AOS.Scale(damage, evalScale);
			damage = AOS.Scale(damage, (int)(scalar * 100));

			return damage / 100;
		}

		public virtual bool IsCasting => State == SpellState.Casting;

		public virtual void OnCasterHurt()
		{
			//Confirm: Monsters and pets cannot be disturbed.
			if (!Caster.Player)
			{
				return;
			}

			if (IsCasting)
			{
				var disturb = true;

				if (ProtectionSpell.Registry[Caster] is double d)
				{
					if (d > Utility.RandomDouble() * 100.0)
					{
						disturb = false;
					}
				}

				if (disturb)
				{
					Interrupt(SpellInterrupt.Hurt, true);
				}
			}
		}

		public virtual void OnCasterKilled()
		{
			Interrupt(SpellInterrupt.Kill);
		}

		public virtual void OnConnectionChanged()
		{
			FinishSequence();
		}

		public virtual bool OnCasterMoving(Direction d)
		{
			if (IsCasting && BlocksMovement)
			{
				Caster.SendLocalizedMessage(500111); // You are frozen and can not move.
				return false;
			}

			return true;
		}

		public virtual bool OnCasterEquiping(Item item)
		{
			if (IsCasting)
			{
				Interrupt(SpellInterrupt.EquipRequest);
			}

			return true;
		}

		public virtual bool OnCasterUsingObject(object o)
		{
			if (State == SpellState.Sequencing)
			{
				Interrupt(SpellInterrupt.UseRequest);
			}

			return true;
		}

		public virtual bool OnCastInTown(Region r)
		{
			return Info.AllowTown;
		}

		public virtual bool ConsumeReagents()
		{
			if (Scroll != null || !Caster.Player)
			{
				return true;
			}

			if (AosAttributes.GetValue(Caster, AosAttribute.LowerRegCost) > Utility.Random(100))
			{
				return true;
			}

			if (Engines.ConPVP.DuelContext.IsFreeConsume(Caster))
			{
				return true;
			}

			var pack = Caster.Backpack;

			if (pack == null)
			{
				return false;
			}

			var types = Info.Reagents.Types.ToArray();
			var amounts = Info.Reagents.Amounts.ToArray();

			if (pack.ConsumeTotal(types, amounts) == -1)
			{
				return true;
			}

			return false;
		}

		public virtual double GetInscribeSkill(Mobile m)
		{
			// There is no chance to gain
			//m.CheckSkill(SkillName.Inscribe, 0.0, 120.0);

			return m.Skills[SkillName.Inscribe].Value;
		}

		public virtual int GetInscribeFixed(Mobile m)
		{
			// There is no chance to gain
			//m.CheckSkill(SkillName.Inscribe, 0.0, 120.0);

			return m.Skills[SkillName.Inscribe].Fixed;
		}

		public virtual int GetDamageFixed(Mobile m)
		{
			//m.CheckSkill(DamageSkill, 0.0, m.Skills[DamageSkill].Cap);

			return m.Skills[DamageSkill].Fixed;
		}

		public virtual double GetDamageSkill(Mobile m)
		{
			//m.CheckSkill(DamageSkill, 0.0, m.Skills[DamageSkill].Cap);

			return m.Skills[DamageSkill].Value;
		}

		public virtual double GetResistSkill(Mobile m)
		{
			return m.Skills[SkillName.MagicResist].Value;
		}

		public virtual bool CheckResisted(Mobile target)
		{
			var n = GetResistPercent(target);

			n /= 100.0;

			if (n <= 0.0)
			{
				return false;
			}

			if (n >= 1.0)
			{
				return true;
			}

			var maxSkill = (1 + (int)Circle) * 10;

			maxSkill += (1 + ((int)Circle / 6)) * 25;

			if (target.Skills[SkillName.MagicResist].Value < maxSkill)
			{
				target.CheckSkill(SkillName.MagicResist, 0.0, target.Skills[SkillName.MagicResist].Cap);
			}

			return n >= Utility.RandomDouble();
		}

		public virtual double GetResistPercentForCircle(Mobile target, SpellCircle circle)
		{
			var firstPercent = target.Skills[SkillName.MagicResist].Value / 5.0;
			var secondPercent = target.Skills[SkillName.MagicResist].Value - (((Caster.Skills[CastSkill].Value - 20.0) / 5.0) + (1 + (int)circle) * 5.0);

			return (firstPercent > secondPercent ? firstPercent : secondPercent) / 2.0; // Seems should be about half of what stratics says.
		}

		public virtual double GetResistPercent(Mobile target)
		{
			return GetResistPercentForCircle(target, Circle);
		}

		public virtual double GetDamageScalar(Mobile target)
		{
			var scalar = 1.0;

			if (!Core.AOS) // EvalInt stuff for AoS is handled elsewhere
			{
				var casterEI = Caster.Skills[DamageSkill].Value;
				var targetRS = target.Skills[SkillName.MagicResist].Value;
				/*
				if(Core.AOS)
				{
					targetRS = 0;
				}
				
				m_Caster.CheckSkill(DamageSkill, 0.0, 120.0);
				*/
				if (casterEI > targetRS)
				{
					scalar = (1.0 + ((casterEI - targetRS) / 500.0));
				}
				else
				{
					scalar = (1.0 + ((casterEI - targetRS) / 200.0));
				}

				// magery damage bonus, -25% at 0 skill, +0% at 100 skill, +5% at 120 skill
				scalar += (Caster.Skills[CastSkill].Value - 100.0) / 400.0;

				if (!target.Player && !target.Body.IsHuman /*&& !Core.AOS*/ )
				{
					scalar *= 2.0; // Double magery damage to monsters/animals if not AOS
				}
			}

			if (target is BaseCreature tc)
			{
				tc.AlterDamageScalarFrom(Caster, ref scalar);
			}

			if (Caster is BaseCreature cc)
			{
				cc.AlterDamageScalarTo(target, ref scalar);
			}

			if (Core.SE)
			{
				scalar *= GetSlayerDamageScalar(target);
			}

			target.Region.SpellDamageScalar(Caster, target, ref scalar);

			if (EvasionSpell.CheckSpellEvasion(target))  //Only single target spells an be evaded
			{
				scalar = 0;
			}

			return scalar;
		}

		public virtual double GetSlayerDamageScalar(Mobile defender)
		{
			var atkBook = SpellbookHelper.FindEquippedSpellbook(Caster);

			var scalar = 1.0;

			if (atkBook is ISlayer slBook)
			{
				var atkSlayer = SlayerGroup.GetEntryByName(slBook.Slayer);
				var atkSlayer2 = SlayerGroup.GetEntryByName(slBook.Slayer2);

				if (atkSlayer != null && atkSlayer.Slays(defender) || atkSlayer2 != null && atkSlayer2.Slays(defender))
				{
					defender.FixedEffect(0x37B9, 10, 5);    //TODO: Confirm this displays on OSIs
					scalar = 2.0;
				}

				var context = TransformationSpellHelper.GetContext(defender);

				if ((slBook.Slayer == SlayerName.Silver || slBook.Slayer2 == SlayerName.Silver) && context != null && context.Type != typeof(HorrificBeastSpell))
				{
					scalar += .25; // Every necromancer transformation other than horrific beast take an additional 25% damage
				}

				if (scalar != 1.0)
				{
					return scalar;
				}
			}

			var defISlayer = SpellbookHelper.FindEquippedSpellbook(defender) as ISlayer;

			defISlayer ??= defender.Weapon as ISlayer;

			if (defISlayer != null)
			{
				var defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
				var defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);

				if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(Caster) || defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(Caster))
				{
					scalar = 2.0;
				}
			}

			return scalar;
		}

		public virtual void DoFizzle()
		{
			Caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502632); // The spell fizzles.

			if (Caster.Player)
			{
				if (Core.AOS)
				{
					Caster.FixedParticles(0x3735, 1, 30, 9503, EffectLayer.Waist);
				}
				else
				{
					Caster.FixedEffect(0x3735, 6, 30);
				}

				Caster.PlaySound(0x5C);
			}
		}

		private CastTimer m_CastTimer;
		private AnimTimer m_AnimTimer;

		public virtual bool CheckInterrupt(SpellInterrupt type, bool resistable)
		{
			if (resistable && Scroll is BaseWand)
			{
				return false;
			}

			return true;
		}

		public void Interrupt(SpellInterrupt type)
		{
			Interrupt(type, false);
		}

		public void Interrupt(SpellInterrupt type, bool resistable)
		{
			if (!CheckInterrupt(type, resistable))
			{
				return;
			}

			if (State == SpellState.Casting)
			{
				if (!Core.AOS && Circle == SpellCircle.First)
				{
					return;
				}

				State = SpellState.None;
				Caster.Spell = null;

				OnInterrupt(type, true);

				if (m_CastTimer != null)
				{
					m_CastTimer.Stop();
				}

				if (m_AnimTimer != null)
				{
					m_AnimTimer.Stop();
				}

				if (Core.AOS && Caster.Player && type == SpellInterrupt.Hurt)
				{
					DoHurtFizzle();
				}

				Caster.NextSpellTime = Core.TickCount + (int)GetInterruptRecovery().TotalMilliseconds;
			}
			else if (State == SpellState.Sequencing)
			{
				if (!Core.AOS && Circle == SpellCircle.First)
				{
					return;
				}

				State = SpellState.None;
				Caster.Spell = null;

				OnInterrupt(type, false);

				Target.Cancel(Caster);

				if (Core.AOS && Caster.Player && type == SpellInterrupt.Hurt)
				{
					DoHurtFizzle();
				}
			}
		}

		public virtual void DoHurtFizzle()
		{
			Caster.FixedEffect(0x3735, 6, 30);
			Caster.PlaySound(0x5C);
		}

		public virtual void OnInterrupt(SpellInterrupt type, bool message)
		{
			if (message)
			{
				Caster.SendLocalizedMessage(500641); // Your concentration is disturbed, thus ruining thy spell.
			}
		}

		public virtual bool CheckCast()
		{
			if (BlockedByHorrificBeast && TransformationSpellHelper.UnderTransformation(Caster, typeof(HorrificBeastSpell)))
			{
				Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
				return false;
			}

			if (BlockedByAnimalForm && AnimalFormSpell.UnderTransformation(Caster))
			{
				Caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
				return false;
			}

			if (Scroll is not BaseWand && (Caster.Paralyzed || Caster.Frozen))
			{
				Caster.SendLocalizedMessage(502643); // You can not cast a spell while frozen.
				return false;
			}

			if (Caster is PlayerMobile pm && pm.PeacedUntil > DateTime.UtcNow)
			{
				Caster.SendLocalizedMessage(1072060); // You cannot cast a spell while calmed.
				return false;
			}

			#region Dueling

			if (Caster is PlayerMobile pd && pd.DuelContext != null && !pd.DuelContext.AllowSpellCast(Caster, this))
			{
				return false;
			}

			#endregion

			return true;
		}

		public virtual void SayMantra()
		{
			if (Scroll is BaseWand)
			{
				return;
			}

			var mantra = Mantra;

			if (mantra?.Length > 0 && Caster.Player)
			{
				Caster.PublicOverheadMessage(MessageType.Spell, Caster.SpeechHue, true, mantra, false);
			}
		}

		public virtual bool BlockedByHorrificBeast => true;
		public virtual bool BlockedByAnimalForm => true;
		public virtual bool BlocksMovement => true;

		public virtual bool CheckNextSpellTime => Scroll is not BaseWand;

		public virtual bool Cast()
		{
			StartCastTime = Core.TickCount;

			if (Core.AOS && Caster.Spell is Spell s && s.State == SpellState.Sequencing)
			{
				s.Interrupt(SpellInterrupt.NewCast);
			}

			if (!Info.IsValid || !Info.Enabled || !Caster.CheckAlive())
			{
				return false;
			}

			if (CheckNextSpellTime && Core.TickCount - Caster.NextSpellTime < 0)
			{
				Caster.SendLocalizedMessage(502644); // You have not yet recovered from casting a spell.
				return false;
			}

			if (Scroll is BaseWand && Caster.Spell != null && Caster.Spell.IsCasting)
			{
				Caster.SendLocalizedMessage(502643); // You can not cast a spell while frozen.
				return false;
			}

			if (Caster.Spell != null && Caster.Spell.IsCasting)
			{
				Caster.SendLocalizedMessage(502642); // You are already casting a spell.
				return false;
			}

			if (Caster.Spell != null || !CheckCast() || !Caster.CheckSpellCast(this) || !Caster.Region.OnBeginSpellCast(Caster, this))
			{
				return false;
			}

			State = SpellState.Casting;
			Caster.Spell = this;

			if (Scroll is not BaseWand && RevealOnCast)
			{
				Caster.RevealingAction();
			}

			SayMantra();

			var castDelay = GetCastDelay();

			if (ShowHandMovement && (Caster.Body.IsHuman || (Caster.Player && Caster.Body.IsMonster)))
			{
				var count = (int)Math.Ceiling(castDelay.TotalSeconds / m_AnimateDelay.TotalSeconds);

				if (count != 0)
				{
					m_AnimTimer = new AnimTimer(this, count);
					m_AnimTimer.Start();
				}

				if (Info.LeftHandEffect > 0)
				{
					Caster.FixedParticles(0, 10, 5, Info.LeftHandEffect, EffectLayer.LeftHand);
				}

				if (Info.RightHandEffect > 0)
				{
					Caster.FixedParticles(0, 10, 5, Info.RightHandEffect, EffectLayer.RightHand);
				}
			}

			if (ClearHandsOnCast)
			{
				Caster.ClearHands();
			}

			if (Core.ML)
			{
				WeaponAbility.ClearCurrentAbility(Caster);
			}

			m_CastTimer = new CastTimer(this, castDelay);

			OnBeginCast();

			if (castDelay > TimeSpan.Zero)
			{
				m_CastTimer.Start();
			}
			else
			{
				m_CastTimer.Tick();
			}

			EventSink.InvokeCastSpellSuccess(new CastSpellSuccessEventArgs(Caster, this));

			return true;
		}

		public abstract void OnCast();

		public virtual void OnBeginCast()
		{
		}

		public virtual void GetCastSkills(ref double req, out double min, out double max)
		{
			min = max = 0; //Intended but not required for overriding.
		}

		public virtual bool CheckFizzle()
		{
			if (Scroll is BaseWand)
			{
				return true;
			}

			var skill = GetSkillRequirement();

			double minSkill, maxSkill;

			GetCastSkills(ref skill, out minSkill, out maxSkill);

			if (skill > 0 && Caster.Skills[CastSkill].Value < skill)
			{
				SendSkillRequirementMessage(skill, CastSkill);
				return false;
			}

			var mana = GetManaRequirement();

			if (mana > 0 && Caster.Mana < mana)
			{
				SendManaRequirementMessage(mana);
				return false;
			}

			var tithe = GetTitheRequirement();

			if (tithe > 0 && Caster.TithingPoints < tithe)
			{
				SendTitheRequirementMessage(tithe);
				return false;
			}

			if (!ConsumeReagents())
			{
				SendReagentRequirementMessage();
				return false;
			}

			if (DamageSkill != CastSkill)
			{
				Caster.CheckSkill(DamageSkill, 0.0, Caster.Skills[DamageSkill].Cap);
			}

			if (!Caster.CheckSkill(CastSkill, minSkill, maxSkill))
			{
				return false;
			}

			Caster.Mana -= mana;
			Caster.TithingPoints -= tithe;

			return true;
		}

		public virtual void SendSkillRequirementMessage(double value, SkillName skill)
		{
			Caster.SendMessage($"You must have at least {value:F1} {Caster.Skills[skill].Name} to use this ability.");
		}

		public virtual void SendManaRequirementMessage(int mana)
		{
			Caster.SendLocalizedMessage(1060174, mana.ToString()); // You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
		}

		public virtual void SendTitheRequirementMessage(int tithe)
		{
			Caster.SendLocalizedMessage(1060173, tithe.ToString()); // You must have at least ~1_TITHE_REQUIREMENT~ Tithing Points to use this ability.
		}

		public virtual void SendReagentRequirementMessage()
		{
			Caster.LocalOverheadMessage(MessageType.Regular, 0x22, 502630); // More reagents are needed for this spell.
		}

		public double GetSkillRequirement()
		{
			return GetSkillRequirement(Info.Skill);
		}

		public virtual double GetSkillRequirement(double value)
		{
			return value;
		}

		public int GetManaRequirement()
		{
			return GetManaRequirement(Info.Mana);
		}

		public virtual int GetManaRequirement(int mana)
		{
			var scalar = 1.0;

			if (!MindRotSpell.GetMindRotScalar(Caster, ref scalar))
			{
				scalar = 1.0;
			}

			// Lower Mana Cost = 40%
			var lmc = AosAttributes.GetValue(Caster, AosAttribute.LowerManaCost);

			if (lmc > 40)
			{
				lmc = 40;
			}

			scalar -= (double)lmc / 100;

			return (int)(mana * scalar);
		}

		public int GetTitheRequirement()
		{
			return GetTitheRequirement(Info.Tithe);
		}

		public virtual int GetTitheRequirement(int tithe)
		{
			var lrc = AosAttributes.GetValue(Caster, AosAttribute.LowerRegCost) / 100.0;

			if (Utility.RandomDouble() < lrc)
			{
				tithe = 0;
			}

			return tithe;
		}

		public virtual TimeSpan GetInterruptRecovery()
		{
			if (Core.AOS)
			{
				return TimeSpan.Zero;
			}

			var basedelay = GetCastDelay();

			var delay = 1.0 - Math.Sqrt((Core.TickCount - StartCastTime) / 1000.0 / basedelay.TotalSeconds);

			if (delay < 0.2)
			{
				delay = 0.2;
			}

			return TimeSpan.FromSeconds(delay);
		}

		public virtual int CastRecoveryBase => 6;
		public virtual int CastRecoveryFastScalar => 1;
		public virtual int CastRecoveryPerSecond => 4;
		public virtual int CastRecoveryMinimum => 0;

		public virtual bool UseCastRecoveryMin => false;
		public virtual bool UseCastRecoveryMods => true;

		public virtual TimeSpan GetCastRecovery()
		{
			if (!Core.AOS)
			{
				return m_NextSpellDelay;
			}

			if (UseCastRecoveryMin)
			{
				return TimeSpan.FromSeconds(CastRecoveryMinimum / (double)CastRecoveryPerSecond);
			}

			if (!UseCastRecoveryMods)
			{
				return TimeSpan.FromSeconds(CastRecoveryBase / (double)CastRecoveryPerSecond);
			}

			var fcr = AosAttributes.GetValue(Caster, AosAttribute.CastRecovery);

			fcr -= ThunderstormSpell.GetCastRecoveryMalus(Caster);

			var fcrDelay = CastRecoveryFastScalar * fcr;

			var delay = CastRecoveryBase - fcrDelay;

			if (delay < CastRecoveryMinimum)
			{
				delay = CastRecoveryMinimum;
			}

			return TimeSpan.FromSeconds(delay / (double)CastRecoveryPerSecond);
		}

		public abstract TimeSpan CastDelayBase { get; }

		public virtual double CastDelayFastScalar => 1;
		public virtual double CastDelaySecondsPerTick => 0.25;
		public virtual TimeSpan CastDelayMinimum => TimeSpan.FromSeconds(0.25);

		public virtual bool UseCastDelayMin => false;
		public virtual bool UseCastDelayMods => true;

		public virtual TimeSpan GetCastDelay()
		{
			if (Scroll is BaseWand) // TODO: Should FC apply to wands?
			{
				if (!Core.ML)
				{
					return TimeSpan.Zero;
				}

				if (UseCastDelayMin)
				{
					return CastDelayMinimum;
				}

				return CastDelayBase;
			}

			if (UseCastDelayMin)
			{
				return CastDelayMinimum;
			}

			if (!UseCastDelayMods)
			{
				return CastDelayBase;
			}

			// Faster casting cap of 2 (if not using the protection spell) 
			// Faster casting cap of 0 (if using the protection spell) 
			// Paladin spells are subject to a faster casting cap of 4 
			// Paladins with magery of 70.0 or above are subject to a faster casting cap of 2 
			var fcMax = 4;

			if (CastSkill == SkillName.Magery || CastSkill == SkillName.Necromancy || (CastSkill == SkillName.Chivalry && Caster.Skills[SkillName.Magery].Value >= 70.0))
			{
				fcMax = 2;
			}

			var fc = AosAttributes.GetValue(Caster, AosAttribute.CastSpeed);

			if (fc > fcMax)
			{
				fc = fcMax;
			}

			if (ProtectionSpell.Registry.Contains(Caster))
			{
				fc -= 2;
			}

			if (EssenceOfWindSpell.IsDebuffed(Caster))
			{
				fc -= EssenceOfWindSpell.GetFCMalus(Caster);
			}

			var baseDelay = CastDelayBase;

			var fcDelay = TimeSpan.FromSeconds(CastDelayFastScalar * fc * CastDelaySecondsPerTick);

			var delay = baseDelay - fcDelay;

			if (delay < CastDelayMinimum)
			{
				delay = CastDelayMinimum;
			}

			return delay;
		}

		public virtual void FinishSequence()
		{
			State = SpellState.None;

			if (Caster.Spell == this)
			{
				Caster.Spell = null;
			}
		}

		public virtual int ComputeKarmaAward()
		{
			return 0;
		}

		public virtual bool CheckSequence()
		{
			if (Caster.Deleted || !Caster.Alive || Caster.Spell != this || State != SpellState.Sequencing)
			{
				DoFizzle();
				return false;
			}

			if (Scroll != null && Scroll is not Runebook && (Scroll.Amount <= 0 || Scroll.Deleted || Scroll.RootParent != Caster || (Scroll is BaseWand bw && (bw.Charges <= 0 || Scroll.Parent != Caster))))
			{
				DoFizzle();
				return false;
			}

			if (Core.AOS && (Caster.Frozen || Caster.Paralyzed))
			{
				Caster.SendLocalizedMessage(502646); // You cannot cast a spell while frozen.
				DoFizzle();
				return false;
			}

			if (Caster is PlayerMobile pm && pm.PeacedUntil > DateTime.UtcNow)
			{
				Caster.SendLocalizedMessage(1072060); // You cannot cast a spell while calmed.
				DoFizzle();
				return false;
			}

			if (!CheckCast() || !CheckFizzle())
			{
				DoFizzle();
				return false;
			}

			if (Scroll is SpellScroll)
			{
				Scroll.Consume();
			}
			else if (Scroll is BaseWand wand)
			{
				wand.ConsumeCharge(Caster);

				Caster.RevealingAction();
			}

			if (Scroll is BaseWand)
			{
				var m = Scroll.Movable;

				Scroll.Movable = false;

				if (ClearHandsOnCast)
				{
					Caster.ClearHands();
				}

				Scroll.Movable = m;
			}
			else if (ClearHandsOnCast)
			{
				Caster.ClearHands();
			}

			var karma = ComputeKarmaAward();

			if (karma != 0)
			{
				Titles.AwardKarma(Caster, karma, true);
			}

			if (TransformationSpellHelper.UnderTransformation(Caster, typeof(VampiricEmbraceSpell)))
			{
				var garlic = false;

				for (var i = 0; !garlic && i < Info.Reagents.Count; ++i)
				{
					garlic = Info.Reagents[i] > 0 && Info.Reagents[i] == Reagent.Garlic;
				}

				if (garlic)
				{
					Caster.SendLocalizedMessage(1061651); // The garlic burns you!

					AOS.Damage(Caster, Utility.RandomMinMax(17, 23), 100, 0, 0, 0, 0);
				}
			}

			return true;
		}

		public bool CheckBSequence(Mobile target)
		{
			return CheckBSequence(target, false);
		}

		public bool CheckBSequence(Mobile target, bool allowDead)
		{
			if (!target.Alive && !allowDead)
			{
				Caster.SendLocalizedMessage(501857); // This spell won't work on that!
				return false;
			}
			else if (Caster.CanBeBeneficial(target, true, allowDead) && CheckSequence())
			{
				Caster.DoBeneficial(target);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool CheckHSequence(Mobile target)
		{
			if (!target.Alive)
			{
				Caster.SendLocalizedMessage(501857); // This spell won't work on that!
				return false;
			}
			else if (Caster.CanBeHarmful(target) && CheckSequence())
			{
				Caster.DoHarmful(target);
				return true;
			}
			else
			{
				return false;
			}
		}

		private class AnimTimer : Timer
		{
			private readonly Spell m_Spell;

			public AnimTimer(Spell spell, int count) : base(TimeSpan.Zero, m_AnimateDelay, count)
			{
				m_Spell = spell;

				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				if (m_Spell.State != SpellState.Casting || m_Spell.Caster.Spell != m_Spell)
				{
					Stop();
					return;
				}

				if (!m_Spell.Caster.Mounted && m_Spell.Info.Action >= 0)
				{
					if (m_Spell.Caster.Body.IsHuman)
					{
						m_Spell.Caster.Animate(m_Spell.Info.Action, 7, 1, true, false, 0);
					}
					else if (m_Spell.Caster.Player && m_Spell.Caster.Body.IsMonster)
					{
						m_Spell.Caster.Animate(12, 7, 1, true, false, 0);
					}
				}

				if (!Running)
				{
					m_Spell.m_AnimTimer = null;
				}
			}
		}

		private class CastTimer : Timer
		{
			private readonly Spell m_Spell;

			public CastTimer(Spell spell, TimeSpan castDelay) : base(castDelay)
			{
				m_Spell = spell;

				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				if (m_Spell == null || m_Spell.Caster == null)
				{
					return;
				}
				
				if (m_Spell.State == SpellState.Casting && m_Spell.Caster.Spell == m_Spell)
				{
					m_Spell.State = SpellState.Sequencing;
					m_Spell.m_CastTimer = null;

					m_Spell.Caster.OnSpellCast(m_Spell);

					if (m_Spell.Caster.Region != null)
					{
						m_Spell.Caster.Region.OnSpellCast(m_Spell.Caster, m_Spell);
					}

					m_Spell.Caster.NextSpellTime = Core.TickCount + (int)m_Spell.GetCastRecovery().TotalMilliseconds; // Spell.NextSpellDelay;

					var originalTarget = m_Spell.Caster.Target;

					m_Spell.OnCast();

					if (m_Spell.Caster.Player && m_Spell.Caster.Target != originalTarget && m_Spell.Caster.Target != null)
					{
						m_Spell.Caster.Target.BeginTimeout(m_Spell.Caster, TimeSpan.FromSeconds(30.0));
					}

					m_Spell.m_CastTimer = null;
				}
			}

			public void Tick()
			{
				OnTick();
			}
		}
	}

	public class SpellGumpIcons
	{
		public static string FilePath => Path.Combine(Core.CurrentSavesDirectory, "Spells", "Icons.bin");

		public static Dictionary<PlayerMobile, SpellGumpIcons> PlayerStates { get; } = new();

		[CallPriority(Int32.MinValue)]
		public static void Configure()
		{
			EventSink.WorldSave += OnSave;
			EventSink.WorldLoad += OnLoad;

			EventSink.Login += OnLogin;
		}

		public static SpellGumpIcons GetState(PlayerMobile player)
		{
			if (!PlayerStates.TryGetValue(player, out var state) || state == null)
			{
				if (!player.Deleted)
				{
					PlayerStates[player] = state = new SpellGumpIcons(player);
				}
			}
			else if (player.Deleted)
			{
				_ = PlayerStates.Remove(player);

				state = null;
			}

			return state;
		}

		public static SpellIconState GetIcon(PlayerMobile player, SpellName spell)
		{
			var state = GetState(player);

			if (state != null && state.Icons.TryGetValue(spell, out var info))
			{
				return info;
			}

			return null;
		}

		private static void OnLogin(LoginEventArgs args)
		{
			if (args.Mobile is PlayerMobile user)
			{
				var state = GetState(user);

				if (state != null)
				{
					var invalid = new Queue<SpellName>();

					foreach (var o in state.Icons)
					{
						if (o.Value != null && o.Value.Spell != SpellName.Invalid)
						{
							if (o.Value.Enabled)
							{
								new SpellIconGump(user, o.Value).SendGump();
							}
						}
						else
						{
							invalid.Enqueue(o.Key);
						}
					}

					while (invalid.TryDequeue(out var key))
					{
						state.Icons.Remove(key);
					}
				}
			}
		}

		private static void OnSave(WorldSaveEventArgs e)
		{
			Persistence.Serialize(FilePath, OnSerialize);
		}

		private static void OnSerialize(GenericWriter writer)
		{
			writer.Write(0);

			writer.Write(PlayerStates.Count);

			foreach (var state in PlayerStates.Values)
			{
				state.Serialize(writer);
			}
		}

		private static void OnLoad()
		{
			Persistence.Deserialize(FilePath, OnDeserialize);
		}

		private static void OnDeserialize(GenericReader reader)
		{
			_ = reader.ReadInt();

			var count = reader.ReadInt();

			while (--count >= 0)
			{
				var state = new SpellGumpIcons(reader);

				if (state.Owner?.Deleted == false)
				{
					PlayerStates[state.Owner] = state;
				}
			}
		}

		public PlayerMobile Owner { get; private set; }

		public Dictionary<SpellName, SpellIconState> Icons { get; } = new();

		public SpellGumpIcons(PlayerMobile owner)
		{
			Owner = owner;
		}

		public SpellGumpIcons(GenericReader reader)
		{
			Deserialize(reader);
		}

		public void Add(SpellIconState info)
		{
			if (info != null && info.Spell != SpellName.Invalid)
			{
				Icons[info.Spell] = info;
			}
		}

		public SpellIconState Get(SpellName id)
		{
			_ = Icons.TryGetValue(id, out var ii);

			return ii;
		}

		public bool Contains(SpellName id)
		{
			return Icons.ContainsKey(id);
		}

		public bool Remove(SpellName id)
		{
			return Icons.Remove(id);
		}

		public void Append(bool negatively)
		{
			var removeList = new Queue<SpellName>();

			foreach (var kvp in Icons)
			{
				if (!negatively)
				{
					Add(kvp.Value);
				}
				else
				{
					removeList.Enqueue(kvp.Value.Spell);
				}
			}

			while (removeList.TryDequeue(out var key))
			{
				Icons.Remove(key);
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(Owner);

			writer.Write(Icons.Count);

			foreach (var kvp in Icons)
			{
				kvp.Value.Serialize(writer);
			}
		}

		public void Deserialize(GenericReader reader)
		{
			_ = reader.ReadInt();

			Owner = reader.ReadMobile<PlayerMobile>();

			var count = reader.ReadInt();

			while (--count >= 0)
			{
				var icons = new SpellIconState(reader);

				if (icons.Spell != SpellName.Invalid)
				{
					Icons[icons.Spell] = icons;
				}
			}
		}
	}

	public sealed class SpellIconState
	{
		private SpellInfo m_SpellInfo;

		public SpellInfo SpellInfo => m_SpellInfo ??= SpellRegistry.GetInfo(Spell);

		public bool Enabled => SpellInfo?.Enabled == true;

		public string Name => SpellInfo?.Name ?? String.Empty;
		public string Mantra => SpellInfo?.Mantra ?? String.Empty;
		public string Desc => SpellInfo?.Desc ?? String.Empty;

		public int Icon => SpellInfo?.Icon ?? 0;
		public int Back => SpellInfo?.Back ?? 0;

		public Type Type => SpellInfo?.Type;

		public SpellName Spell { get; private set; }

		public int X { get; set; }
		public int Y { get; set; }

		public SpellIconState(SpellName spell, int x, int y)
		{
			Spell = spell;

			X = x;
			Y = y;
		}

		public SpellIconState(GenericReader reader)
		{
			Deserialize(reader);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(0);

			writer.Write(Spell);

			writer.WriteEncodedInt(X);
			writer.WriteEncodedInt(Y);
		}

		public void Deserialize(GenericReader reader)
		{
			_ = reader.ReadInt();

			Spell = reader.ReadEnum<SpellName>();

			X = reader.ReadEncodedInt();
			Y = reader.ReadEncodedInt();
		}
	}

	public class SpellIconGump : BaseGump
	{
		private readonly SpellIconState m_Icon;

		public SpellIconState Icon => m_Icon;

		public SpellIconGump(PlayerMobile user, SpellIconState info)
			: base(user, info.X, info.Y)
		{
			m_Icon = info;

			TypeID = GetTypeID(m_Icon.Type);

			Closable = false;
			Disposable = false;
			Dragable = false;
			Resizable = false;
		}

		public override void AddGumpLayout()
		{
			AddPage(0);

			AddBackground(0, 0, 54, 54, m_Icon.Back);

			AddButton(45, 0, 9008, 9010, () =>
			{
				var icons = SpellGumpIcons.GetState(User);

				if (icons != null && icons.Remove(m_Icon.Spell))
				{
					User.SendMessage("The icon has been removed.");
				}

				Close();
			});

			AddButton(5, 5, m_Icon.Icon, m_Icon.Icon, () =>
			{
				var result = SpellHelper.TryCast(User, m_Icon.SpellInfo, true);

				if (result != null)
				{
					Refresh(false, false);
				}
			});
		}
	}

	public class SpellIconPlacementGump : BaseGump
	{
		private static readonly SpellIconState m_InvalidIcon = new(SpellName.Invalid, 0, 0);

		private readonly SpellName m_Spell;

		private SpellIconState m_Icon;

		private RelayInfo m_Response;

		private int m_Diff = 50;

		public SpellIconPlacementGump(PlayerMobile user, SpellName spell)
			: base(user, 0, 0)
		{
			m_Spell = spell;

			Closable = true;
			Disposable = true;
			Dragable = false;
			Resizable = false;
		}

		public override void SendGump()
		{
			if (m_Icon == null && m_Spell != SpellName.Invalid)
			{
				var icons = SpellGumpIcons.GetState(User);

				if (icons != null)
				{
					m_Icon = icons.Get(m_Spell);

					if (m_Icon == null)
					{
						m_Icon = new(m_Spell, 300, 200);

						icons.Add(m_Icon);
					}
				}
			}

			m_Icon ??= m_InvalidIcon;

			base.SendGump();
		}

		public override void AddGumpLayout()
		{
			AddPage(0);

			AddImage(260, 160, 5011);
			AddLabel(353, 230, 1153, "Icon Placement");

			AddLabel(338, 350, 1153, m_Icon.Name);

			// Apply
			AddButton(412, 288, 2444, 2443, () =>
			{
				var ns = User.NetState;

				if (ns != null)
				{
					var gump = ns.Gumps.Find(g => g is SpellIconGump ig && ig.Icon?.Spell == m_Spell);

					if (gump != null)
					{
						ns.Send(new CloseGump(gump.TypeID, 0));

						ns.RemoveGump(gump);

						gump.OnServerClose(ns);
					}

					if (m_Icon.Enabled && m_Icon.Spell != SpellName.Invalid)
					{
						var ig = new SpellIconGump(User, m_Icon);

						ig.SendGump();
					}
				}

				Refresh(true, false);
			});

			// Move
			AddButton(325, 288, 2444, 2443, () =>
			{
				var x = m_Icon.X;
				var y = m_Icon.Y;

				ParseValue(1, ref x);
				ParseValue(2, ref y);

				m_Icon.X = Math.Clamp(x, 0, 4096);
				m_Icon.Y = Math.Clamp(y, 0, 2048);

				Refresh(true, false);
			});

			AddLabel(425, 290, 1153, "Apply");
			AddLabel(339, 290, 1153, "Move");

			// Left
			AddButton(278, 276, 4506, 4506, () =>
			{
				m_Icon.X = Math.Clamp(m_Icon.X - m_Diff, 0, 4096);

				Refresh(true, false);
			});

			// Right
			AddButton(474, 276, 4502, 4502, () =>
			{
				m_Icon.X = Math.Clamp(m_Icon.X + m_Diff, 0, 4096);

				Refresh(true, false);
			});

			// Up
			AddButton(377, 178, 4500, 4500, () =>
			{
				m_Icon.Y = Math.Clamp(m_Icon.Y - m_Diff, 0, 2048);

				Refresh(true, false);
			});

			// Down
			AddButton(377, 375, 4504, 4504, () =>
			{
				m_Icon.Y = Math.Clamp(m_Icon.Y + m_Diff, 0, 2048);

				Refresh(true, false);
			});

			AddBackground(348, 260, 105, 20, 9300);
			AddBackground(348, 318, 105, 20, 9300);
			AddBackground(388, 290, 25, 20, 9300);

			AddTextEntry(348, 260, 105, 20, 1153, 1, $"{m_Icon.X}");
			AddTextEntry(348, 318, 105, 20, 1153, 2, $"{m_Icon.Y}");

			AddTextEntry(388, 290, 25, 20, 1153, 0, $"{m_Diff}");

			AddBackground(m_Icon.X, m_Icon.Y, 54, 56, m_Icon.Back);
			AddImage(m_Icon.X + 45, m_Icon.Y, 9008);
			AddImage(m_Icon.X + 5, m_Icon.Y + 5, m_Icon.Icon);
		}

		public override void OnResponse(RelayInfo info)
		{
			m_Response = info;

			ParseValue(0, ref m_Diff);

			base.OnResponse(info);
		}

		private void ParseValue(int id, ref int diff)
		{
			var entry = m_Response?.GetTextEntry(id);
			var value = entry?.Text?.Trim();

			if (Int32.TryParse(value, out var val))
			{
				diff = val;
			}
		}
	}

	public class SpellbookGump : BaseGump
	{
		public bool Detailed { get; set; }

		public Spellbook Book { get; private set; }

		public SpellSchool School => Book?.School ?? SpellSchool.Invalid;

		public SpellbookTheme Theme => Book?.Theme ?? SpellbookTheme.Invalid;

		private readonly List<SpellInfo> _Info = new();

		private readonly Rectangle[] _Panels = new Rectangle[2];

		private int _Page, _PageCount, _EntryLimit;

		public SpellbookGump(PlayerMobile user, Spellbook book)
			: base(user)
		{
			Book = book;

			TypeID = Book.Serial;
		}

		public bool HasSpell(SpellName spell)
		{
			return Book?.HasSpell(spell) ?? false;
		}

		public override void SendGump()
		{
			if (Book == null || Book.Deleted)
			{
				Close();
				return;
			}

			if (!Book.CanDisplayTo(User, false))
			{
				Close();
				return;
			}

			_Info.Clear();

			if (Book.SpellCount > 0)
			{
				foreach (var spell in SpellRegistry.GetSpells(School))
				{
					if (HasSpell(spell))
					{
						var info = SpellRegistry.GetInfo(spell);

						if (info?.IsValid == true)
						{
							_Info.Add(info);
						}
					}
				}
			}

			base.SendGump();

			if (Book != null && !Book.Gumps.Contains(this))
			{
				Book.Gumps.Add(this);
			}
		}

		public override void AddGumpLayout()
		{
			var size = GetImageSize(Theme.BackgroundID);

			var center = size.Width / 2;

			_Panels[0].X = center - 150;
			_Panels[0].Y = 10;
			_Panels[0].Width = 150;
			_Panels[0].Height = 30 + 160;

			_Panels[1].X = center + 5;
			_Panels[1].Y = 10;
			_Panels[1].Width = 150;
			_Panels[1].Height = 30 + 160;

			if (Detailed)
			{
				_EntryLimit = 2;
			}
			else
			{
				_EntryLimit = (int)Math.Floor(_Panels.Sum(p => p.Height - 30) / 20.0);
			}

			_PageCount = Math.Max(1, (int)Math.Ceiling(_Info.Count / (double)_EntryLimit));
			_Page = Math.Clamp(_Page, 1, _PageCount);

			Build();
		}

		private void Build()
		{
			var theme = Theme;

			var color = theme.TextColor;

			var bgID = theme.BackgroundID;
			var bgHue = theme.BackgroundHue;

			var cb1 = theme.CastButtonID1;
			var cb2 = theme.CastButtonID2;

			var name = Center(theme.Name);
			var summary = Center(theme.Summary);

			if (bgHue <= 0 && Book?.Hue > 0)
			{
				bgHue = Book.Hue;
			}

			var perPage = (int)Math.Ceiling(_EntryLimit / (double)_Panels.Length);

			var buttonSize = GetImageSize(cb1);
			var buttonOffsetY = (20 - buttonSize.Height) / 2;

			buttonSize.Width += 5;

			SpellInfo[] spells = null;

			AddECHandleInput();

			if (_Info.Count > 0)
			{
				spells = _Info.Skip((_Page - 1) * _EntryLimit).Take(_EntryLimit).ToArray();

				for (var index = 0; index < spells.Length; index++)
				{
					var spell = spells[index];
					var bounds = _Panels[index / perPage];

					var offsetX = bounds.X + bounds.Width - 115;
					var offsetY = bounds.Y + 30 + (20 * (index % perPage)) + 2;

					AddButton(offsetX, offsetY + buttonOffsetY, 2062, 2061, b => ViewInfo(spell));
				}
			}

			if (bgHue > 0)
			{
				AddImage(0, 0, bgID, bgHue);
			}
			else
			{
				AddImage(0, 0, bgID);
			}

			AddECHandleInput();

			if (_PageCount > 1)
			{
				if (_Page > 1)
				{
					var bounds = _Panels[0];

					var offsetX = bounds.X - 8;
					var offsetY = bounds.Y - 5;

					AddECHandleInput();

					AddButton(offsetX, offsetY, 2205, 2205, b =>
					{
						--_Page;

						Refresh(true, false);
					});

					if (bgHue > 0)
					{
						AddImage(offsetX, offsetY, 2205, bgHue);
					}

					AddECHandleInput();
				}

				if (_Page < _PageCount)
				{
					var bounds = _Panels[1];

					var offsetX = bounds.X + bounds.Width - 37;
					var offsetY = bounds.Y - 6;

					AddECHandleInput();

					AddButton(offsetX, offsetY, 2206, 2206, b =>
					{
						++_Page;

						Refresh(true, false);
					});

					if (bgHue > 0)
					{
						AddImage(offsetX, offsetY, 2206, bgHue);
					}

					AddECHandleInput();
				}
			}

			AddHtml(_Panels[0].X, _Panels[0].Y, _Panels[0].Width, 40, name, false, false, color);
			AddHtml(_Panels[1].X, _Panels[1].Y, _Panels[1].Width, 40, summary, false, false, color);

			if (spells != null)
			{
				for (var index = 0; index < spells.Length; index++)
				{
					var spell = spells[index];
					var bounds = _Panels[index / perPage];

					var offsetX = bounds.X;
					var offsetY = bounds.Y + 30 + (20 * (index % perPage));

					AddECHandleInput();

					if (!spell.Type.IsAssignableTo(typeof(RacialPassiveAbility)))
					{
						AddButton(offsetX, offsetY + buttonOffsetY, cb1, cb2, b =>
						{
							var result = SpellHelper.TryCast(User, Book, spell, true);

							Refresh(result == null, false);
						});
					}
					else
					{
						AddImage(offsetX, offsetY + buttonOffsetY, cb1, 900);
					}

					AddHtml(offsetX + buttonSize.Width, offsetY, bounds.Width - buttonSize.Width, 40, spell.Name, false, false, color);

					AddECHandleInput();
				}
			}
		}

		public override void OnResponse(RelayInfo info)
		{
			if (Book != null && Book.CanDisplayTo(User, true))
			{
				base.OnResponse(info);
			}
			else
			{
				Close();
			}
		}

		private void ViewInfo(SpellInfo info)
		{
			Refresh();

			_ = User.CloseGump(typeof(ScrollGump));
			_ = SendGump(new ScrollGump(User, Book, info));
		}

		public override void OnClosed()
		{
			base.OnClosed();

			if (!Open)
			{
				_ = Book?.Gumps.Remove(this);
			}
		}

		public override void OnDispose()
		{
			base.OnDispose();

			_ = Book?.Gumps.Remove(this);

			Book = null;
		}

		public sealed class ScrollGump : BaseGump
		{
			private readonly ISpellbook m_Book;
			private readonly SpellInfo m_Info;

			private readonly bool m_IsRacialPassive;

			public ScrollGump(PlayerMobile user, ISpellbook book, SpellInfo info)
				: base(user, 485, 175)
			{
				if (user == null || book == null || info?.IsValid != true)
				{
					if (user != null)
					{
						_ = Timer.DelayCall((u, t) => u.CloseGump(t), user, GetType());
					}

					return;
				}

				m_Info = info;
				m_Book = book;

				m_IsRacialPassive = m_Info.Type.IsAssignableTo(typeof(RacialPassiveAbility));

				Closable = true;
				Disposable = true;
				Dragable = true;
				Resizable = false;
			}

			public override void AddGumpLayout()
			{
				var theme = m_Book.Theme;

				var textColor = theme.TextColor;

				var bgHue = theme.BackgroundHue;

				var cb1 = theme.CastButtonID1;
				var cb2 = theme.CastButtonID2;

				if (m_Book is IEntity e)
				{
					if (bgHue <= 0 && e.Hue > 0)
					{
						bgHue = e.Hue;
					}
				}

				AddPage(0);

				if (bgHue > 0)
				{
					AddImage(0, 0, 9390, bgHue);
					AddImage(86, 0, 9392, bgHue);

					AddImage(0, 125, 9396, bgHue);
					AddImage(86, 125, 9398, bgHue);
				}
				else
				{
					AddImage(0, 0, 9390);
					AddImage(86, 0, 9392);

					AddImage(0, 125, 9396);
					AddImage(86, 125, 9398);
				}

				if (!String.IsNullOrWhiteSpace(m_Info.Name))
				{
					AddHtml(30, 3, 140, 20, Center(m_Info.Name), false, false, textColor);
				}
				else
				{
					AddHtml(30, 3, 140, 20, Center(Utility.FriendlyName(m_Info.ID)), false, false, textColor);
				}

				if (m_IsRacialPassive)
				{
					AddImage(30, 40, m_Info.Icon);

					AddHtml(80, 40, 60, 40, AlignRight("Passive"), false, false, textColor);
					AddImage(145, 40, cb1, 900);
				}
				else
				{
					AddButton(30, 40, m_Info.Icon, m_Info.Icon, () =>
					{
						Refresh(false, false);

						_ = User.CloseGump(typeof(SpellIconPlacementGump));
						_ = SendGump(new SpellIconPlacementGump(User, m_Info.ID));
					});

					AddECHandleInput();

					AddHtml(80, 40, 60, 40, AlignRight("Cast"), false, false, textColor);
					AddButton(145, 40, cb1, cb2, () =>
					{
						var result = SpellHelper.TryCast(User, m_Book, m_Info, true);

						if (result != null)
						{
							Refresh(false, false);
						}
					});

					AddECHandleInput();
				}

				// Spell Info
				var infoString = new StringBuilder();

				if (!String.IsNullOrWhiteSpace(m_Info.Desc))
				{
					_ = infoString.AppendLine(m_Info.Desc.Replace(".  ", ".\n"));
				}

				if (m_Info.Mana > 0 || m_Info.Tithe > 0 || m_Info.Skill > 0.0)
				{
					_ = infoString.AppendLine();

					if (m_Info.Mana > 0)
					{
						_ = infoString.AppendLine($"{m_Info.Mana:N0} Mana");
					}

					if (m_Info.Tithe > 0)
					{
						_ = infoString.AppendLine($"{m_Info.Tithe:N0} Tithe");
					}

					if (m_Info.Skill > 0)
					{
						_ = infoString.AppendLine($"{m_Info.Skill:F1} Skill");
					}
				}

				if (m_Info.ReagentsCount > 0)
				{
					_ = infoString.AppendLine();
					_ = infoString.AppendLine($"Reagents:");
					_ = infoString.AppendLine();

					foreach (var reg in m_Info.Reagents)
					{
						if (reg.Amount > 0)
						{
							var name = Utility.FriendlyName(reg.Type);

							_ = infoString.AppendLine($"{reg.Amount:N0} {name}");
						}
					}
				}

				if (infoString.Length > 0)
				{
					AddHtml(30, 95, 140, 130, infoString.ToString(), false, true, textColor);
				}
			}
		}
	}
}