using Server.Engines.Craft;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Items
{
	public enum PotionEffect
	{
		Nightsight,
		CureLesser,
		Cure,
		CureGreater,
		Agility,
		AgilityGreater,
		Strength,
		StrengthGreater,
		PoisonLesser,
		Poison,
		PoisonGreater,
		PoisonDeadly,
		Refresh,
		RefreshTotal,
		HealLesser,
		Heal,
		HealGreater,
		ExplosionLesser,
		Explosion,
		ExplosionGreater,
		Conflagration,
		ConflagrationGreater,
		MaskOfDeath,        // Mask of Death is not available in OSI but does exist in cliloc files
		MaskOfDeathGreater, // included in enumeration for compatability if later enabled by OSI
		ConfusionBlast,
		ConfusionBlastGreater,
		Invisibility,
		Parasitic,
		Darkglow,
	}

	public abstract class BasePotion : Item, ICraftable, ICommodity
	{
		private PotionEffect m_PotionEffect;

		public PotionEffect PotionEffect
		{
			get => m_PotionEffect;
			set
			{
				m_PotionEffect = value;
				InvalidateProperties();
			}
		}

		int ICommodity.DescriptionNumber => LabelNumber;
		bool ICommodity.IsDeedable => (Core.ML);

		public override int LabelNumber => 1041314 + (int)m_PotionEffect;

		public BasePotion(int itemID, PotionEffect effect) : base(itemID)
		{
			m_PotionEffect = effect;

			Stackable = Core.ML;
			Weight = 1.0;
		}

		public BasePotion(Serial serial) : base(serial)
		{
		}

		public virtual bool RequireFreeHand => true;

		public static bool HasFreeHand(Mobile m)
		{
			var handOne = m.FindItemOnLayer(Layer.OneHanded);
			var handTwo = m.FindItemOnLayer(Layer.TwoHanded);

			if (handTwo is BaseWeapon)
			{
				handOne = handTwo;
			}

			if (handTwo is BaseRanged)
			{
				var ranged = (BaseRanged)handTwo;

				if (ranged.Balanced)
				{
					return true;
				}
			}

			return (handOne == null || handTwo == null);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!Movable)
			{
				return;
			}

			if (from.InRange(GetWorldLocation(), 1))
			{
				if (!RequireFreeHand || HasFreeHand(from))
				{
					if (this is BaseExplosionPotion && Amount > 1)
					{
						var pot = (BasePotion)Activator.CreateInstance(GetType());

						if (pot != null)
						{
							Amount--;

							if (from.Backpack != null && !from.Backpack.Deleted)
							{
								from.Backpack.DropItem(pot);
							}
							else
							{
								pot.MoveToWorld(from.Location, from.Map);
							}
							pot.Drink(from);
						}
					}
					else
					{
						Drink(from);
					}
				}
				else
				{
					from.SendLocalizedMessage(502172); // You must have a free hand to drink a potion.
				}
			}
			else
			{
				from.SendLocalizedMessage(502138); // That is too far away for you to use
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write((int)m_PotionEffect);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
				case 0:
					{
						m_PotionEffect = (PotionEffect)reader.ReadInt();
						break;
					}
			}

			if (version == 0)
			{
				Stackable = Core.ML;
			}
		}

		public abstract void Drink(Mobile from);

		public static void PlayDrinkEffect(Mobile m)
		{
			m.RevealingAction();

			m.PlaySound(0x2D6);

			#region Dueling
			if (!Engines.ConPVP.DuelContext.IsFreeConsume(m))
			{
				m.AddToBackpack(new Bottle());
			}
			#endregion

			if (m.Body.IsHuman && !m.Mounted)
			{
				m.Animate(34, 5, 1, true, false, 0);
			}
		}

		public static int EnhancePotions(Mobile m)
		{
			var EP = AosAttributes.GetValue(m, AosAttribute.EnhancePotions);
			var skillBonus = m.Skills.Alchemy.Fixed / 330 * 10;

			if (Core.ML && EP > 50 && m.AccessLevel <= AccessLevel.Player)
			{
				EP = 50;
			}

			return (EP + skillBonus);
		}

		public static TimeSpan Scale(Mobile m, TimeSpan v)
		{
			if (!Core.AOS)
			{
				return v;
			}

			var scalar = 1.0 + (0.01 * EnhancePotions(m));

			return TimeSpan.FromSeconds(v.TotalSeconds * scalar);
		}

		public static double Scale(Mobile m, double v)
		{
			if (!Core.AOS)
			{
				return v;
			}

			var scalar = 1.0 + (0.01 * EnhancePotions(m));

			return v * scalar;
		}

		public static int Scale(Mobile m, int v)
		{
			if (!Core.AOS)
			{
				return v;
			}

			return AOS.Scale(v, 100 + EnhancePotions(m));
		}

		public override bool StackWith(Mobile from, Item dropped, bool playSound)
		{
			if (dropped is BasePotion && ((BasePotion)dropped).m_PotionEffect == m_PotionEffect)
			{
				return base.StackWith(from, dropped, playSound);
			}

			return false;
		}

		#region ICraftable

		public virtual int OnCraft(int quality, bool makersMark, Mobile from, ICraftSystem craftSystem, Type typeRes, ICraftTool tool, ICraftItem craftItem, int resHue)
		{
			if (craftSystem is DefAlchemy)
			{
				var pack = from.Backpack;

				if (pack != null)
				{
					if (PotionEffect >= PotionEffect.Invisibility)
					{
						return 1;
					}

					foreach (var keg in pack.FindItemsByType<PotionKeg>())
					{
						if (keg == null)
						{
							continue;
						}

						if (keg.Held <= 0 || keg.Held >= 100)
						{
							continue;
						}

						if (keg.Type != PotionEffect)
						{
							continue;
						}

						++keg.Held;

						Consume();
						from.AddToBackpack(new Bottle());

						return -1; // signal placed in keg
					}
				}
			}

			return 1;
		}

		#endregion
	}

	#region Potion Sub-Base Classes

	/// Agility Potion
	public abstract class BaseAgilityPotion : BasePotion
	{
		public abstract int DexOffset { get; }
		public abstract TimeSpan Duration { get; }

		public BaseAgilityPotion(PotionEffect effect) : base(0xF08, effect)
		{
		}

		public BaseAgilityPotion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}

		public bool DoAgility(Mobile from)
		{
			// TODO: Verify scaled; is it offset, duration, or both?
			if (Spells.SpellHelper.AddStatOffset(from, StatType.Dex, Scale(from, DexOffset), Duration))
			{
				from.FixedEffect(0x375A, 10, 15);
				from.PlaySound(0x1E7);
				return true;
			}

			from.SendLocalizedMessage(502173); // You are already under a similar effect.
			return false;
		}

		public override void Drink(Mobile from)
		{
			if (DoAgility(from))
			{
				BasePotion.PlayDrinkEffect(from);

				if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
				{
					Consume();
				}
			}
		}
	}

	/// Conflagration Potion
	public abstract class BaseConflagrationPotion : BasePotion
	{
		public abstract int MinDamage { get; }
		public abstract int MaxDamage { get; }

		public override bool RequireFreeHand => false;

		public BaseConflagrationPotion(PotionEffect effect) : base(0xF06, effect)
		{
			Hue = 0x489;
		}

		public BaseConflagrationPotion(Serial serial) : base(serial)
		{
		}

		public override void Drink(Mobile from)
		{
			if (Core.AOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)))
			{
				from.SendLocalizedMessage(1062725); // You can not use that potion while paralyzed.
				return;
			}

			var delay = GetDelay(from);

			if (delay > 0)
			{
				from.SendLocalizedMessage(1072529, String.Format("{0}\t{1}", delay, delay > 1 ? "seconds." : "second.")); // You cannot use that for another ~1_NUM~ ~2_TIMEUNITS~
				return;
			}

			var targ = from.Target as ThrowTarget;

			if (targ != null && targ.Potion == this)
			{
				return;
			}

			from.RevealingAction();

			if (!m_Users.Contains(from))
			{
				m_Users.Add(from);
			}

			from.Target = new ThrowTarget(this);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}

		private readonly List<Mobile> m_Users = new List<Mobile>();

		public void Explode_Callback(object state)
		{
			var states = (object[])state;

			Explode((Mobile)states[0], (Point3D)states[1], (Map)states[2]);
		}

		public virtual void Explode(Mobile from, Point3D loc, Map map)
		{
			if (Deleted || map == null)
			{
				return;
			}

			Consume();

			// Check if any other players are using this potion
			for (var i = 0; i < m_Users.Count; i++)
			{
				var targ = m_Users[i].Target as ThrowTarget;

				if (targ != null && targ.Potion == this)
				{
					Target.Cancel(from);
				}
			}

			// Effects
			Effects.PlaySound(loc, map, 0x20C);

			for (var i = -2; i <= 2; i++)
			{
				for (var j = -2; j <= 2; j++)
				{
					var p = new Point3D(loc.X + i, loc.Y + j, loc.Z);

					if (map.CanFit(p, 12, true, false) && from.InLOS(p))
					{
						new InternalItem(from, p, map, MinDamage, MaxDamage);
					}
				}
			}
		}

		#region Delay
		private static readonly Hashtable m_Delay = new Hashtable();

		public static void AddDelay(Mobile m)
		{
			var timer = m_Delay[m] as Timer;

			if (timer != null)
			{
				timer.Stop();
			}

			m_Delay[m] = Timer.DelayCall(TimeSpan.FromSeconds(30), EndDelay_Callback, m);
		}

		public static int GetDelay(Mobile m)
		{
			var timer = m_Delay[m] as Timer;

			if (timer != null && timer.Next > DateTime.UtcNow)
			{
				return (int)(timer.Next - DateTime.UtcNow).TotalSeconds;
			}

			return 0;
		}

		private static void EndDelay_Callback(object obj)
		{
			if (obj is Mobile)
			{
				EndDelay((Mobile)obj);
			}
		}

		public static void EndDelay(Mobile m)
		{
			var timer = m_Delay[m] as Timer;

			if (timer != null)
			{
				timer.Stop();
				m_Delay.Remove(m);
			}
		}
		#endregion

		private class ThrowTarget : Target
		{
			private readonly BaseConflagrationPotion m_Potion;

			public BaseConflagrationPotion Potion => m_Potion;

			public ThrowTarget(BaseConflagrationPotion potion) : base(12, true, TargetFlags.None)
			{
				m_Potion = potion;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Potion.Deleted || m_Potion.Map == Map.Internal)
				{
					return;
				}

				var p = targeted as IPoint3D;

				if (p == null || from.Map == null)
				{
					return;
				}

				// Add delay
				BaseConflagrationPotion.AddDelay(from);

				SpellHelper.GetSurfaceTop(ref p);

				from.RevealingAction();

				IEntity to;

				if (p is Mobile)
				{
					to = (Mobile)p;
				}
				else
				{
					to = new Entity(Serial.Zero, new Point3D(p), from.Map);
				}

				Effects.SendMovingEffect(from, to, 0xF0D, 7, 0, false, false, m_Potion.Hue, 0);
				Timer.DelayCall(TimeSpan.FromSeconds(1.5), m_Potion.Explode_Callback, new object[] { from, new Point3D(p), from.Map });
			}
		}

		public class InternalItem : Item
		{
			private Mobile m_From;
			private int m_MinDamage;
			private int m_MaxDamage;
			private DateTime m_End;
			private Timer m_Timer;

			public Mobile From => m_From;

			public override bool BlocksFit => true;

			public InternalItem(Mobile from, Point3D loc, Map map, int min, int max) : base(0x398C)
			{
				Movable = false;
				Light = LightType.Circle300;

				MoveToWorld(loc, map);

				m_From = from;
				m_End = DateTime.UtcNow + TimeSpan.FromSeconds(10);

				SetDamage(min, max);

				m_Timer = new InternalTimer(this, m_End);
				m_Timer.Start();
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if (m_Timer != null)
				{
					m_Timer.Stop();
				}
			}

			public InternalItem(Serial serial) : base(serial)
			{
			}

			public int GetDamage() { return Utility.RandomMinMax(m_MinDamage, m_MaxDamage); }

			private void SetDamage(int min, int max)
			{
				/* 	new way to apply alchemy bonus according to Stratics' calculator.
					this gives a mean to values 25, 50, 75 and 100. Stratics' calculator is outdated.
					Those goals will give 2 to alchemy bonus. It's not really OSI-like but it's an approximation. */

				m_MinDamage = min;
				m_MaxDamage = max;

				if (m_From == null)
				{
					return;
				}

				var alchemySkill = m_From.Skills.Alchemy.Fixed;
				var alchemyBonus = alchemySkill / 125 + alchemySkill / 250;

				m_MinDamage = Scale(m_From, m_MinDamage + alchemyBonus);
				m_MaxDamage = Scale(m_From, m_MaxDamage + alchemyBonus);
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0); // version

				writer.Write(m_From);
				writer.Write(m_End);
				writer.Write(m_MinDamage);
				writer.Write(m_MaxDamage);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				var version = reader.ReadInt();

				m_From = reader.ReadMobile();
				m_End = reader.ReadDateTime();
				m_MinDamage = reader.ReadInt();
				m_MaxDamage = reader.ReadInt();

				m_Timer = new InternalTimer(this, m_End);
				m_Timer.Start();
			}

			public override bool OnMoveOver(Mobile m)
			{
				if (Visible && m_From != null && (!Core.AOS || m != m_From) && SpellHelper.ValidIndirectTarget(m_From, m) && m_From.CanBeHarmful(m, false))
				{
					m_From.DoHarmful(m);

					AOS.Damage(m, m_From, GetDamage(), 0, 100, 0, 0, 0);
					m.PlaySound(0x208);
				}

				return true;
			}

			private class InternalTimer : Timer
			{
				private readonly InternalItem m_Item;
				private readonly DateTime m_End;

				public InternalTimer(InternalItem item, DateTime end) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1.0))
				{
					m_Item = item;
					m_End = end;

					Priority = TimerPriority.FiftyMS;
				}

				protected override void OnTick()
				{
					if (m_Item.Deleted)
					{
						return;
					}

					if (DateTime.UtcNow > m_End)
					{
						m_Item.Delete();
						Stop();
						return;
					}

					var from = m_Item.From;

					if (m_Item.Map == null || from == null)
					{
						return;
					}

					var mobiles = new List<Mobile>();

					foreach (var mobile in m_Item.GetMobilesInRange(0))
					{
						mobiles.Add(mobile);
					}

					for (var i = 0; i < mobiles.Count; i++)
					{
						var m = mobiles[i];

						if ((m.Z + 16) > m_Item.Z && (m_Item.Z + 12) > m.Z && (!Core.AOS || m != from) && SpellHelper.ValidIndirectTarget(from, m) && from.CanBeHarmful(m, false))
						{
							if (from != null)
							{
								from.DoHarmful(m);
							}

							AOS.Damage(m, from, m_Item.GetDamage(), 0, 100, 0, 0, 0);
							m.PlaySound(0x208);
						}
					}
				}
			}
		}
	}

	/// ConfusionBlast Potion
	public abstract class BaseConfusionBlastPotion : BasePotion
	{
		public abstract int Radius { get; }

		public override bool RequireFreeHand => false;

		public BaseConfusionBlastPotion(PotionEffect effect) : base(0xF06, effect)
		{
			Hue = 0x48D;
		}

		public BaseConfusionBlastPotion(Serial serial) : base(serial)
		{
		}

		public override void Drink(Mobile from)
		{
			if (Core.AOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)))
			{
				from.SendLocalizedMessage(1062725); // You can not use that potion while paralyzed.
				return;
			}

			var delay = GetDelay(from);

			if (delay > 0)
			{
				from.SendLocalizedMessage(1072529, String.Format("{0}\t{1}", delay, delay > 1 ? "seconds." : "second.")); // You cannot use that for another ~1_NUM~ ~2_TIMEUNITS~
				return;
			}

			var targ = from.Target as ThrowTarget;

			if (targ != null && targ.Potion == this)
			{
				return;
			}

			from.RevealingAction();

			if (!m_Users.Contains(from))
			{
				m_Users.Add(from);
			}

			from.Target = new ThrowTarget(this);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}

		private readonly List<Mobile> m_Users = new List<Mobile>();

		public void Explode_Callback(object state)
		{
			var states = (object[])state;

			Explode((Mobile)states[0], (Point3D)states[1], (Map)states[2]);
		}

		public virtual void Explode(Mobile from, Point3D loc, Map map)
		{
			if (Deleted || map == null)
			{
				return;
			}

			Consume();

			// Check if any other players are using this potion
			for (var i = 0; i < m_Users.Count; i++)
			{
				var targ = m_Users[i].Target as ThrowTarget;

				if (targ != null && targ.Potion == this)
				{
					Target.Cancel(from);
				}
			}

			// Effects
			Effects.PlaySound(loc, map, 0x207);

			Geometry.Circle2D(loc, map, Radius, BlastEffect, 270, 90);

			Timer.DelayCall(TimeSpan.FromSeconds(0.3), CircleEffect2, loc, map);

			foreach (var mobile in map.GetMobilesInRange(loc, Radius))
			{
				if (mobile is BaseCreature)
				{
					var mon = (BaseCreature)mobile;

					if (mon.Controlled || mon.Summoned)
					{
						continue;
					}

					mon.Pacify(from, DateTime.UtcNow + TimeSpan.FromSeconds(5.0)); // TODO check
				}
			}
		}

		#region Effects
		public virtual void BlastEffect(Point3D p, Map map)
		{
			if (map.CanFit(p, 12, true, false))
			{
				Effects.SendLocationEffect(p, map, 0x376A, 4, 9);
			}
		}

		public void CircleEffect2(Point3D p, Map map)
		{
			Geometry.Circle2D(p, map, Radius, BlastEffect, 90, 270);
		}
		#endregion

		#region Delay
		private static readonly Hashtable m_Delay = new Hashtable();

		public static void AddDelay(Mobile m)
		{
			var timer = m_Delay[m] as Timer;

			if (timer != null)
			{
				timer.Stop();
			}

			m_Delay[m] = Timer.DelayCall(TimeSpan.FromSeconds(60), EndDelay_Callback, m);
		}

		public static int GetDelay(Mobile m)
		{
			var timer = m_Delay[m] as Timer;

			if (timer != null && timer.Next > DateTime.UtcNow)
			{
				return (int)(timer.Next - DateTime.UtcNow).TotalSeconds;
			}

			return 0;
		}

		private static void EndDelay_Callback(object obj)
		{
			if (obj is Mobile)
			{
				EndDelay((Mobile)obj);
			}
		}

		public static void EndDelay(Mobile m)
		{
			var timer = m_Delay[m] as Timer;

			if (timer != null)
			{
				timer.Stop();
				m_Delay.Remove(m);
			}
		}
		#endregion

		private class ThrowTarget : Target
		{
			private readonly BaseConfusionBlastPotion m_Potion;

			public BaseConfusionBlastPotion Potion => m_Potion;

			public ThrowTarget(BaseConfusionBlastPotion potion) : base(12, true, TargetFlags.None)
			{
				m_Potion = potion;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Potion.Deleted || m_Potion.Map == Map.Internal)
				{
					return;
				}

				var p = targeted as IPoint3D;

				if (p == null || from.Map == null)
				{
					return;
				}

				// Add delay
				BaseConfusionBlastPotion.AddDelay(from);

				SpellHelper.GetSurfaceTop(ref p);

				from.RevealingAction();

				IEntity to;

				if (p is Mobile)
				{
					to = (Mobile)p;
				}
				else
				{
					to = new Entity(Serial.Zero, new Point3D(p), from.Map);
				}

				Effects.SendMovingEffect(from, to, 0xF0D, 7, 0, false, false, m_Potion.Hue, 0);
				Timer.DelayCall(TimeSpan.FromSeconds(1.0), m_Potion.Explode_Callback, new object[] { from, new Point3D(p), from.Map });
			}
		}
	}

	/// Cure Potion
	public class CureLevelInfo
	{
		private readonly Poison m_Poison;
		private readonly double m_Chance;

		public Poison Poison => m_Poison;

		public double Chance => m_Chance;

		public CureLevelInfo(Poison poison, double chance)
		{
			m_Poison = poison;
			m_Chance = chance;
		}
	}

	public abstract class BaseCurePotion : BasePotion
	{
		public abstract CureLevelInfo[] LevelInfo { get; }

		public BaseCurePotion(PotionEffect effect) : base(0xF07, effect)
		{
		}

		public BaseCurePotion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}

		public void DoCure(Mobile from)
		{
			var cure = false;

			var info = LevelInfo;

			for (var i = 0; i < info.Length; ++i)
			{
				var li = info[i];

				if (li.Poison == from.Poison && Scale(from, li.Chance) > Utility.RandomDouble())
				{
					cure = true;
					break;
				}
			}

			if (cure && from.CurePoison(from))
			{
				from.SendLocalizedMessage(500231); // You feel cured of poison!

				from.FixedEffect(0x373A, 10, 15);
				from.PlaySound(0x1E0);
			}
			else if (!cure)
			{
				from.SendLocalizedMessage(500232); // That potion was not strong enough to cure your ailment!
			}
		}

		public override void Drink(Mobile from)
		{
			if (TransformationSpellHelper.UnderTransformation(from, typeof(Spells.Necromancy.VampiricEmbraceSpell)))
			{
				from.SendLocalizedMessage(1061652); // The garlic in the potion would surely kill you.
			}
			else if (from.Poisoned)
			{
				DoCure(from);

				BasePotion.PlayDrinkEffect(from);

				from.FixedParticles(0x373A, 10, 15, 5012, EffectLayer.Waist);
				from.PlaySound(0x1E0);

				if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
				{
					Consume();
				}
			}
			else
			{
				from.SendLocalizedMessage(1042000); // You are not poisoned.
			}
		}
	}

	/// Explosion Potion
	public abstract class BaseExplosionPotion : BasePotion
	{
		public abstract int MinDamage { get; }
		public abstract int MaxDamage { get; }

		public override bool RequireFreeHand => false;

		private static readonly bool LeveledExplosion = false; // Should explosion potions explode other nearby potions?
		private static readonly bool InstantExplosion = false; // Should explosion potions explode on impact?
		private static readonly bool RelativeLocation = false; // Is the explosion target location relative for mobiles?
		private const int ExplosionRange = 2; // How long is the blast radius?

		public BaseExplosionPotion(PotionEffect effect) : base(0xF0D, effect)
		{
		}

		public BaseExplosionPotion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}

		public virtual object FindParent(Mobile from)
		{
			var m = HeldBy;

			if (m != null && m.Holding == this)
			{
				return m;
			}

			object obj = RootParent;

			if (obj != null)
			{
				return obj;
			}

			if (Map == Map.Internal)
			{
				return from;
			}

			return this;
		}

		private Timer m_Timer;

		public List<Mobile> Users => m_Users;

		private List<Mobile> m_Users;

		public override void Drink(Mobile from)
		{
			if (Core.AOS && (from.Paralyzed || from.Frozen || (from.Spell != null && from.Spell.IsCasting)))
			{
				from.SendLocalizedMessage(1062725); // You can not use a purple potion while paralyzed.
				return;
			}

			var targ = from.Target as ThrowTarget;
			Stackable = false; // Scavenged explosion potions won't stack with those ones in backpack, and still will explode.

			if (targ != null && targ.Potion == this)
			{
				return;
			}

			from.RevealingAction();

			if (m_Users == null)
			{
				m_Users = new List<Mobile>();
			}

			if (!m_Users.Contains(from))
			{
				m_Users.Add(from);
			}

			from.Target = new ThrowTarget(this);

			if (m_Timer == null)
			{
				from.SendLocalizedMessage(500236); // You should throw it now!

				if (Core.ML)
				{
					m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.25), 5, Detonate_OnTick, new object[] { from, 3 }); // 3.6 seconds explosion delay
				}
				else
				{
					m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(0.75), TimeSpan.FromSeconds(1.0), 4, Detonate_OnTick, new object[] { from, 3 }); // 2.6 seconds explosion delay
				}
			}
		}

		private void Detonate_OnTick(object state)
		{
			if (Deleted)
			{
				return;
			}

			var states = (object[])state;
			var from = (Mobile)states[0];
			var timer = (int)states[1];

			var parent = FindParent(from);

			if (timer == 0)
			{
				Point3D loc;
				Map map;

				if (parent is Item)
				{
					var item = (Item)parent;

					loc = item.GetWorldLocation();
					map = item.Map;
				}
				else if (parent is Mobile)
				{
					var m = (Mobile)parent;

					loc = m.Location;
					map = m.Map;
				}
				else
				{
					return;
				}

				Explode(from, true, loc, map);
				m_Timer = null;
			}
			else
			{
				if (parent is Item)
				{
					((Item)parent).PublicOverheadMessage(MessageType.Regular, 0x22, false, timer.ToString());
				}
				else if (parent is Mobile)
				{
					((Mobile)parent).PublicOverheadMessage(MessageType.Regular, 0x22, false, timer.ToString());
				}

				states[1] = timer - 1;
			}
		}

		private void Reposition_OnTick(object state)
		{
			if (Deleted)
			{
				return;
			}

			var states = (object[])state;
			var from = (Mobile)states[0];
			var p = (IPoint3D)states[1];
			var map = (Map)states[2];

			var loc = new Point3D(p);

			if (InstantExplosion)
			{
				Explode(from, true, loc, map);
			}
			else
			{
				MoveToWorld(loc, map);
			}
		}

		private class ThrowTarget : Target
		{
			private readonly BaseExplosionPotion m_Potion;

			public BaseExplosionPotion Potion => m_Potion;

			public ThrowTarget(BaseExplosionPotion potion) : base(12, true, TargetFlags.None)
			{
				m_Potion = potion;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Potion.Deleted || m_Potion.Map == Map.Internal)
				{
					return;
				}

				var p = targeted as IPoint3D;

				if (p == null)
				{
					return;
				}

				var map = from.Map;

				if (map == null)
				{
					return;
				}

				SpellHelper.GetSurfaceTop(ref p);

				from.RevealingAction();

				IEntity to;

				to = new Entity(Serial.Zero, new Point3D(p), map);

				if (p is Mobile)
				{
					if (!RelativeLocation) // explosion location = current mob location. 
					{
						p = ((Mobile)p).Location;
					}
					else
					{
						to = (Mobile)p;
					}
				}

				Effects.SendMovingEffect(from, to, m_Potion.ItemID, 7, 0, false, false, m_Potion.Hue, 0);

				if (m_Potion.Amount > 1)
				{
					Mobile.LiftItemDupe(m_Potion, 1);
				}

				m_Potion.Internalize();
				Timer.DelayCall(TimeSpan.FromSeconds(1.0), m_Potion.Reposition_OnTick, new object[] { from, p, map });
			}
		}

		public void Explode(Mobile from, bool direct, Point3D loc, Map map)
		{
			if (Deleted)
			{
				return;
			}

			Consume();

			for (var i = 0; m_Users != null && i < m_Users.Count; ++i)
			{
				var m = m_Users[i];
				var targ = m.Target as ThrowTarget;

				if (targ != null && targ.Potion == this)
				{
					Target.Cancel(m);
				}
			}

			if (map == null)
			{
				return;
			}

			Effects.PlaySound(loc, map, 0x307);

			Effects.SendLocationEffect(loc, map, 0x36B0, 9, 10, 0, 0);
			var alchemyBonus = 0;

			if (direct)
			{
				alchemyBonus = (int)(from.Skills.Alchemy.Value / (Core.AOS ? 5 : 10));
			}

			var eable = LeveledExplosion ? map.GetObjectsInRange(loc, ExplosionRange) : (IPooledEnumerable)map.GetMobilesInRange(loc, ExplosionRange);
			var toExplode = new ArrayList();

			var toDamage = 0;

			foreach (var o in eable)
			{
				if (o is Mobile && (from == null || (SpellHelper.ValidIndirectTarget(from, (Mobile)o) && from.CanBeHarmful((Mobile)o, false))))
				{
					toExplode.Add(o);
					++toDamage;
				}
				else if (o is BaseExplosionPotion && o != this)
				{
					toExplode.Add(o);
				}
			}

			eable.Free();

			var min = Scale(from, MinDamage);
			var max = Scale(from, MaxDamage);

			for (var i = 0; i < toExplode.Count; ++i)
			{
				var o = toExplode[i];

				if (o is Mobile)
				{
					var m = (Mobile)o;

					if (from != null)
					{
						from.DoHarmful(m);
					}

					var damage = Utility.RandomMinMax(min, max);

					damage += alchemyBonus;

					if (!Core.AOS && damage > 40)
					{
						damage = 40;
					}
					else if (Core.AOS && toDamage > 2)
					{
						damage /= toDamage - 1;
					}

					AOS.Damage(m, from, damage, 0, 100, 0, 0, 0);
				}
				else if (o is BaseExplosionPotion)
				{
					var pot = (BaseExplosionPotion)o;

					pot.Explode(from, false, pot.GetWorldLocation(), pot.Map);
				}
			}
		}
	}

	/// Heal Potion
	public abstract class BaseHealPotion : BasePotion
	{
		public abstract int MinHeal { get; }
		public abstract int MaxHeal { get; }
		public abstract double Delay { get; }

		public BaseHealPotion(PotionEffect effect) : base(0xF0C, effect)
		{
		}

		public BaseHealPotion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}

		public void DoHeal(Mobile from)
		{
			var min = Scale(from, MinHeal);
			var max = Scale(from, MaxHeal);

			from.Heal(Utility.RandomMinMax(min, max));
		}

		public override void Drink(Mobile from)
		{
			if (from.Hits < from.HitsMax)
			{
				if (from.Poisoned || MortalStrike.IsWounded(from))
				{
					from.LocalOverheadMessage(MessageType.Regular, 0x22, 1005000); // You can not heal yourself in your current state.
				}
				else
				{
					if (from.BeginAction(typeof(BaseHealPotion)))
					{
						DoHeal(from);

						BasePotion.PlayDrinkEffect(from);

						if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
						{
							Consume();
						}

						Timer.DelayCall(TimeSpan.FromSeconds(Delay), ReleaseHealLock, from);
					}
					else
					{
						from.LocalOverheadMessage(MessageType.Regular, 0x22, 500235); // You must wait 10 seconds before using another healing potion.
					}
				}
			}
			else
			{
				from.SendLocalizedMessage(1049547); // You decide against drinking this potion, as you are already at full health.
			}
		}

		private static void ReleaseHealLock(object state)
		{
			((Mobile)state).EndAction(typeof(BaseHealPotion));
		}
	}

	/// Poison Potion
	public abstract class BasePoisonPotion : BasePotion
	{
		public abstract Poison Poison { get; }

		public abstract double MinPoisoningSkill { get; }
		public abstract double MaxPoisoningSkill { get; }

		public BasePoisonPotion(PotionEffect effect) : base(0xF0A, effect)
		{
		}

		public BasePoisonPotion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}

		public void DoPoison(Mobile from)
		{
			from.ApplyPoison(from, Poison);
		}

		public override void Drink(Mobile from)
		{
			DoPoison(from);

			BasePotion.PlayDrinkEffect(from);

			if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
			{
				Consume();
			}
		}
	}

	/// Refresh Potion
	public abstract class BaseRefreshPotion : BasePotion
	{
		public abstract double Refresh { get; }

		public BaseRefreshPotion(PotionEffect effect) : base(0xF0B, effect)
		{
		}

		public BaseRefreshPotion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}

		public override void Drink(Mobile from)
		{
			if (from.Stam < from.StamMax)
			{
				from.Stam += Scale(from, (int)(Refresh * from.StamMax));

				BasePotion.PlayDrinkEffect(from);

				if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
				{
					Consume();
				}
			}
			else
			{
				from.SendMessage("You decide against drinking this potion, as you are already at full stamina.");
			}
		}
	}

	/// Strength Potion
	public abstract class BaseStrengthPotion : BasePotion
	{
		public abstract int StrOffset { get; }
		public abstract TimeSpan Duration { get; }

		public BaseStrengthPotion(PotionEffect effect) : base(0xF09, effect)
		{
		}

		public BaseStrengthPotion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}

		public bool DoStrength(Mobile from)
		{
			// TODO: Verify scaled; is it offset, duration, or both?
			if (Spells.SpellHelper.AddStatOffset(from, StatType.Str, Scale(from, StrOffset), Duration))
			{
				from.FixedEffect(0x375A, 10, 15);
				from.PlaySound(0x1E7);
				return true;
			}

			from.SendLocalizedMessage(502173); // You are already under a similar effect.
			return false;
		}

		public override void Drink(Mobile from)
		{
			if (DoStrength(from))
			{
				BasePotion.PlayDrinkEffect(from);

				if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
				{
					Consume();
				}
			}
		}
	}

	#endregion
}