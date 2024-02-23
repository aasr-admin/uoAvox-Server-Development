using Server.ContextMenus;
using Server.Engine.Facet;
using Server.Engine.Facet.Module.LumberHarvest;
using Server.Engines.Harvest;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Targeting;
using Server.Targets;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	public enum WeaponType
	{
		Axe,
		Slashing,
		Staff,
		Bashing,
		Piercing,
		Polearm,
		Ranged,
		Fists
	}

	/// BaseAxe
	public interface IAxe
	{
		bool Axe(Mobile from, BaseAxe axe);
	}

	public abstract class BaseAxe : BaseMeleeWeapon, IUsesRemaining, IHarvestTool
	{
		public override int DefHitSound => 0x232;
		public override int DefMissSound => 0x23A;

		public override SkillName DefSkill => SkillName.Swords;
		public override WeaponType DefType => WeaponType.Axe;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Slash2H;

		public virtual HarvestSystem HarvestSystem => Lumberjacking.System;

		IHarvestSystem IHarvestTool.HarvestSystem => HarvestSystem;

		private int m_UsesRemaining;
		private bool m_ShowUsesRemaining;

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get => m_UsesRemaining;
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ShowUsesRemaining
		{
			get => m_ShowUsesRemaining;
			set { m_ShowUsesRemaining = value; InvalidateProperties(); }
		}

		public virtual int GetUsesScalar()
		{
			if (Quality == ItemQuality.Exceptional)
			{
				return 200;
			}

			return 100;
		}

		public override void UnscaleDurability()
		{
			base.UnscaleDurability();

			var scale = GetUsesScalar();

			m_UsesRemaining = ((m_UsesRemaining * 100) + (scale - 1)) / scale;
			InvalidateProperties();
		}

		public override void ScaleDurability()
		{
			base.ScaleDurability();

			var scale = GetUsesScalar();

			m_UsesRemaining = ((m_UsesRemaining * scale) + 99) / 100;
			InvalidateProperties();
		}

		public BaseAxe(int itemID) : base(itemID)
		{
			m_UsesRemaining = 150;
		}

		public BaseAxe(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (HarvestSystem == null || Deleted)
			{
				return;
			}

			var loc = GetWorldLocation();

			if (!from.InLOS(loc) || !from.InRange(loc, 2))
			{
				from.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3E9, 1019045); // I can't reach that
				return;
			}
			else if (!IsAccessibleTo(from))
			{
				PublicOverheadMessage(MessageType.Regular, 0x3E9, 1061637); // You are not allowed to access this.
				return;
			}

			if (!(HarvestSystem is Mining))
			{
				from.SendLocalizedMessage(1010018); // What do you want to use this item on?
			}

			HarvestSystem.BeginHarvesting(from, this);
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (HarvestSystem != null)
			{
				BaseHarvestTool.AddContextMenuEntries(from, this, list, HarvestSystem);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(2); // version

			writer.Write(m_ShowUsesRemaining);

			writer.Write(m_UsesRemaining);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 2:
					{
						m_ShowUsesRemaining = reader.ReadBool();
						goto case 1;
					}
				case 1:
					{
						m_UsesRemaining = reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						if (m_UsesRemaining < 1)
						{
							m_UsesRemaining = 150;
						}

						break;
					}
			}
		}

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			base.OnHit(attacker, defender, damageBonus);

			if (!Core.AOS && (attacker.Player || attacker.Body.IsHuman) && Layer == Layer.TwoHanded && attacker.Skills[SkillName.Anatomy].Value >= 80 && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() && Engines.ConPVP.DuelContext.AllowSpecialAbility(attacker, "Concussion Blow", false))
			{
				var mod = defender.GetStatMod("Concussion");

				if (mod == null)
				{
					defender.SendMessage("You receive a concussion blow!");
					defender.AddStatMod(new StatMod(StatType.Int, "Concussion", -(defender.RawInt / 2), TimeSpan.FromSeconds(30.0)));

					attacker.SendMessage("You deliver a concussion blow!");
					attacker.PlaySound(0x308);
				}
			}
		}
	}

	/// BaseKnife
	public abstract class BaseKnife : BaseMeleeWeapon
	{
		public override int DefHitSound => 0x23B;
		public override int DefMissSound => 0x238;

		public override SkillName DefSkill => SkillName.Swords;
		public override WeaponType DefType => WeaponType.Slashing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Slash1H;

		public BaseKnife(int itemID) : base(itemID)
		{
		}

		public BaseKnife(Serial serial) : base(serial)
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

		public override void OnDoubleClick(Mobile from)
		{
			from.SendLocalizedMessage(1010018); // What do you want to use this item on?

			from.Target = new BladedItemTarget(this);
		}

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			base.OnHit(attacker, defender, damageBonus);

			if (!Core.AOS && Poison != null && PoisonCharges > 0)
			{
				--PoisonCharges;

				if (Utility.RandomDouble() >= 0.5) // 50% chance to poison
				{
					defender.ApplyPoison(attacker, Poison);
				}
			}
		}
	}

	/// BaseSword
	public abstract class BaseSword : BaseMeleeWeapon
	{
		public override SkillName DefSkill => SkillName.Swords;
		public override WeaponType DefType => WeaponType.Slashing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Slash1H;

		public BaseSword(int itemID) : base(itemID)
		{
		}

		public BaseSword(Serial serial) : base(serial)
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

		public override void OnDoubleClick(Mobile from)
		{
			from.SendLocalizedMessage(1010018); // What do you want to use this item on?

			from.Target = new BladedItemTarget(this);
		}

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			base.OnHit(attacker, defender, damageBonus);

			if (!Core.AOS && Poison != null && PoisonCharges > 0)
			{
				--PoisonCharges;

				if (Utility.RandomDouble() >= 0.5) // 50% chance to poison
				{
					defender.ApplyPoison(attacker, Poison);
				}
			}
		}
	}

	/// BaseStaff
	public abstract class BaseStaff : BaseMeleeWeapon
	{
		public override int DefHitSound => 0x233;
		public override int DefMissSound => 0x239;

		public override SkillName DefSkill => SkillName.Macing;
		public override WeaponType DefType => WeaponType.Staff;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Bash2H;

		public BaseStaff(int itemID) : base(itemID)
		{
		}

		public BaseStaff(Serial serial) : base(serial)
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

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			base.OnHit(attacker, defender, damageBonus);

			defender.Stam -= Utility.Random(3, 3); // 3-5 points of stamina loss
		}
	}

	/// BaseBashing
	public abstract class BaseBashing : BaseMeleeWeapon
	{
		public override int DefHitSound => 0x233;
		public override int DefMissSound => 0x239;

		public override SkillName DefSkill => SkillName.Macing;
		public override WeaponType DefType => WeaponType.Bashing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Bash1H;

		public BaseBashing(int itemID) : base(itemID)
		{
		}

		public BaseBashing(Serial serial) : base(serial)
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

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			base.OnHit(attacker, defender, damageBonus);

			defender.Stam -= Utility.Random(3, 3); // 3-5 points of stamina loss
		}

		public override double GetBaseDamage(Mobile attacker)
		{
			var damage = base.GetBaseDamage(attacker);

			if (!Core.AOS && (attacker.Player || attacker.Body.IsHuman) && Layer == Layer.TwoHanded && attacker.Skills[SkillName.Anatomy].Value >= 80 && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() && Engines.ConPVP.DuelContext.AllowSpecialAbility(attacker, "Crushing Blow", false))
			{
				damage *= 1.5;

				attacker.SendMessage("You deliver a crushing blow!"); // Is this not localized?
				attacker.PlaySound(0x11C);
			}

			return damage;
		}
	}

	/// BaseSpear
	public abstract class BaseSpear : BaseMeleeWeapon
	{
		public override int DefHitSound => 0x23C;
		public override int DefMissSound => 0x238;

		public override SkillName DefSkill => SkillName.Fencing;
		public override WeaponType DefType => WeaponType.Piercing;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Pierce2H;

		public BaseSpear(int itemID) : base(itemID)
		{
		}

		public BaseSpear(Serial serial) : base(serial)
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

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			base.OnHit(attacker, defender, damageBonus);

			if (!Core.AOS && Layer == Layer.TwoHanded && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() && Engines.ConPVP.DuelContext.AllowSpecialAbility(attacker, "Paralyzing Blow", false))
			{
				defender.SendMessage("You receive a paralyzing blow!"); // Is this not localized?
				defender.Freeze(TimeSpan.FromSeconds(2.0));

				attacker.SendMessage("You deliver a paralyzing blow!"); // Is this not localized?
				attacker.PlaySound(0x11C);
			}

			if (!Core.AOS && Poison != null && PoisonCharges > 0)
			{
				--PoisonCharges;

				if (Utility.RandomDouble() >= 0.5) // 50% chance to poison
				{
					defender.ApplyPoison(attacker, Poison);
				}
			}
		}
	}

	/// BasePoleArm
	public abstract class BasePoleArm : BaseMeleeWeapon, IUsesRemaining, IHarvestTool
	{
		public override int DefHitSound => 0x237;
		public override int DefMissSound => 0x238;

		public override SkillName DefSkill => SkillName.Swords;
		public override WeaponType DefType => WeaponType.Polearm;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Slash2H;

		public virtual HarvestSystem HarvestSystem => Lumberjacking.System;

		IHarvestSystem IHarvestTool.HarvestSystem => HarvestSystem;

		private int m_UsesRemaining;
		private bool m_ShowUsesRemaining;

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get => m_UsesRemaining;
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ShowUsesRemaining
		{
			get => m_ShowUsesRemaining;
			set { m_ShowUsesRemaining = value; InvalidateProperties(); }
		}

		public BasePoleArm(int itemID) : base(itemID)
		{
			m_UsesRemaining = 150;
		}

		public BasePoleArm(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (HarvestSystem == null)
			{
				return;
			}

			if (IsChildOf(from.Backpack) || Parent == from)
			{
				HarvestSystem.BeginHarvesting(from, this);
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (HarvestSystem != null)
			{
				BaseHarvestTool.AddContextMenuEntries(from, this, list, HarvestSystem);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(2); // version

			writer.Write(m_ShowUsesRemaining);

			writer.Write(m_UsesRemaining);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 2:
					{
						m_ShowUsesRemaining = reader.ReadBool();
						goto case 1;
					}
				case 1:
					{
						m_UsesRemaining = reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						if (m_UsesRemaining < 1)
						{
							m_UsesRemaining = 150;
						}

						break;
					}
			}
		}

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			base.OnHit(attacker, defender, damageBonus);

			if (!Core.AOS && (attacker.Player || attacker.Body.IsHuman) && Layer == Layer.TwoHanded && attacker.Skills[SkillName.Anatomy].Value >= 80 && (attacker.Skills[SkillName.Anatomy].Value / 400.0) >= Utility.RandomDouble() && Engines.ConPVP.DuelContext.AllowSpecialAbility(attacker, "Concussion Blow", false))
			{
				var mod = defender.GetStatMod("Concussion");

				if (mod == null)
				{
					defender.SendMessage("You receive a concussion blow!");
					defender.AddStatMod(new StatMod(StatType.Int, "Concussion", -(defender.RawInt / 2), TimeSpan.FromSeconds(30.0)));

					attacker.SendMessage("You deliver a concussion blow!");
					attacker.PlaySound(0x11C);
				}
			}
		}
	}

	/// BaseRanged
	public abstract class BaseRanged : BaseMeleeWeapon
	{
		public abstract int EffectID { get; }
		public abstract Type AmmoType { get; }
		public abstract Item Ammo { get; }

		public override int DefHitSound => 0x234;
		public override int DefMissSound => 0x238;

		public override SkillName DefSkill => SkillName.Archery;
		public override WeaponType DefType => WeaponType.Ranged;
		public override WeaponAnimation DefAnimation => WeaponAnimation.ShootXBow;

		public override SkillName AccuracySkill => SkillName.Archery;

		private Timer m_RecoveryTimer; // so we don't start too many timers
		private bool m_Balanced;
		private int m_Velocity;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Balanced
		{
			get => m_Balanced;
			set { m_Balanced = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Velocity
		{
			get => m_Velocity;
			set { m_Velocity = value; InvalidateProperties(); }
		}

		public BaseRanged(int itemID) : base(itemID)
		{
		}

		public BaseRanged(Serial serial) : base(serial)
		{
		}

		public override TimeSpan OnSwing(Mobile attacker, Mobile defender, double damageBonus)
		{
			var a = WeaponAbility.GetCurrentAbility(attacker);

			// Make sure we've been standing still for .25/.5/1 second depending on Era
			if (Core.TickCount - attacker.LastMoveTime >= (Core.SE ? 250 : Core.AOS ? 500 : 1000) || (Core.AOS && a is MovingShot))
			{
				var canSwing = true;

				if (Core.AOS)
				{
					canSwing = !attacker.Paralyzed && !attacker.Frozen;

					if (canSwing)
					{
						canSwing = attacker.Spell is not Spell sp || !sp.IsCasting || !sp.BlocksMovement;
					}
				}

				#region Dueling

				if (attacker is PlayerMobile pm && pm.DuelContext != null && !pm.DuelContext.CheckItemEquip(attacker, this))
				{
					canSwing = false;
				}

				#endregion

				if (canSwing && attacker.HarmfulCheck(defender))
				{
					attacker.DisruptiveAction();
					attacker.Send(new Swing(0, attacker, defender));

					if (OnFired(attacker, defender))
					{
						if (CheckHit(attacker, defender))
						{
							OnHit(attacker, defender, damageBonus);
						}
						else
						{
							OnMiss(attacker, defender);
						}
					}
				}

				attacker.RevealingAction();

				return GetDelay(attacker);
			}
			else
			{
				attacker.RevealingAction();

				return TimeSpan.FromSeconds(0.25);
			}
		}

		public override void OnHit(Mobile attacker, Mobile defender, double damageBonus)
		{
			if (attacker.Player && !defender.Player && (defender.Body.IsAnimal || defender.Body.IsMonster) && 0.4 >= Utility.RandomDouble())
			{
				defender.AddToBackpack(Ammo);
			}

			if (Core.ML && m_Velocity > 0)
			{
				var bonus = (int)attacker.GetDistanceToSqrt(defender);

				if (bonus > 0 && m_Velocity > Utility.Random(100))
				{
					AOS.Damage(defender, attacker, bonus * 3, 100, 0, 0, 0, 0);

					if (attacker.Player)
					{
						attacker.SendLocalizedMessage(1072794); // Your arrow hits its mark with velocity!
					}

					if (defender.Player)
					{
						defender.SendLocalizedMessage(1072795); // You have been hit by an arrow with velocity!
					}
				}
			}

			base.OnHit(attacker, defender, damageBonus);
		}

		public override void OnMiss(Mobile attacker, Mobile defender)
		{
			if (attacker.Player && 0.4 >= Utility.RandomDouble())
			{
				if (Core.SE)
				{
					var p = attacker as PlayerMobile;

					if (p != null)
					{
						var ammo = AmmoType;

						if (p.RecoverableAmmo.ContainsKey(ammo))
						{
							p.RecoverableAmmo[ammo]++;
						}
						else
						{
							p.RecoverableAmmo.Add(ammo, 1);
						}

						if (!p.Warmode)
						{
							if (m_RecoveryTimer == null)
							{
								m_RecoveryTimer = Timer.DelayCall(TimeSpan.FromSeconds(10), p.RecoverAmmo);
							}

							if (!m_RecoveryTimer.Running)
							{
								m_RecoveryTimer.Start();
							}
						}
					}
				}
				else
				{
					Ammo.MoveToWorld(new Point3D(defender.X + Utility.RandomMinMax(-1, 1), defender.Y + Utility.RandomMinMax(-1, 1), defender.Z), defender.Map);
				}
			}

			base.OnMiss(attacker, defender);
		}

		public virtual bool OnFired(Mobile attacker, Mobile defender)
		{
			if (attacker.Player)
			{
				var quiver = attacker.FindItemOnLayer(Layer.Cloak) as BaseQuiver;
				var pack = attacker.Backpack;

				if (quiver == null || Utility.Random(100) >= quiver.LowerAmmoCost)
				{
					// consume ammo
					if (quiver != null && quiver.ConsumeTotal(AmmoType, 1))
					{
						quiver.InvalidateWeight();
					}
					else if (pack == null || !pack.ConsumeTotal(AmmoType, 1))
					{
						return false;
					}
				}
				else if (quiver.FindItemByType(AmmoType) == null && (pack == null || pack.FindItemByType(AmmoType) == null))
				{
					// lower ammo cost should not work when we have no ammo at all
					return false;
				}
			}

			attacker.MovingEffect(defender, EffectID, 18, 1, false, false);

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(3); // version

			writer.Write(m_Balanced);
			writer.Write(m_Velocity);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 3:
					{
						m_Balanced = reader.ReadBool();
						m_Velocity = reader.ReadInt();

						goto case 2;
					}
				case 2:
				case 1:
					{
						break;
					}
				case 0:
					{
						/*m_EffectID =*/
						reader.ReadInt();
						break;
					}
			}

			if (version < 2)
			{
				WeaponAttributes.MageWeapon = 0;
				WeaponAttributes.UseBestSkill = 0;
			}
		}
	}

	/// Fists
	public class Fists : BaseMeleeWeapon
	{
		public static void Initialize()
		{
			Mobile.DefaultWeapon = new Fists();

			EventSink.DisarmRequest += new DisarmRequestEventHandler(EventSink_DisarmRequest);
			EventSink.StunRequest += new StunRequestEventHandler(EventSink_StunRequest);
		}

		public override WeaponAbility PrimaryAbility => WeaponAbility.Disarm;
		public override WeaponAbility SecondaryAbility => WeaponAbility.ParalyzingBlow;

		public override int AosStrengthReq => 0;
		public override int AosMinDamage => 1;
		public override int AosMaxDamage => 4;
		public override int AosSpeed => 50;
		public override float MlSpeed => 2.50f;

		public override int OldStrengthReq => 0;
		public override int OldMinDamage => 1;
		public override int OldMaxDamage => 8;
		public override int OldSpeed => 30;

		public override int DefHitSound => -1;
		public override int DefMissSound => -1;

		public override SkillName DefSkill => SkillName.Wrestling;
		public override WeaponType DefType => WeaponType.Fists;
		public override WeaponAnimation DefAnimation => WeaponAnimation.Wrestle;

		public Fists() : base(0)
		{
			Visible = false;
			Movable = false;
			Quality = ItemQuality.Regular;
		}

		public Fists(Serial serial) : base(serial)
		{
		}

		public override double GetDefendSkillValue(Mobile attacker, Mobile defender)
		{
			var wresValue = defender.Skills[SkillName.Wrestling].Value;
			var anatValue = defender.Skills[SkillName.Anatomy].Value;
			var evalValue = defender.Skills[SkillName.EvalInt].Value;
			var incrValue = (anatValue + evalValue + 20.0) * 0.5;

			if (incrValue > 120.0)
			{
				incrValue = 120.0;
			}

			if (wresValue > incrValue)
			{
				return wresValue;
			}
			else
			{
				return incrValue;
			}
		}

		private void CheckPreAOSMoves(Mobile attacker, Mobile defender)
		{
			if (attacker.StunReady)
			{
				if (attacker.CanBeginAction(typeof(Fists)))
				{
					if (attacker.Skills[SkillName.Anatomy].Value >= 80.0 && attacker.Skills[SkillName.Wrestling].Value >= 80.0)
					{
						if (attacker.Stam >= 15)
						{
							attacker.Stam -= 15;

							if (CheckMove(attacker, SkillName.Anatomy))
							{
								StartMoveDelay(attacker);

								attacker.StunReady = false;

								attacker.SendLocalizedMessage(1004013); // You successfully stun your opponent!
								defender.SendLocalizedMessage(1004014); // You have been stunned!

								defender.Freeze(TimeSpan.FromSeconds(4.0));
							}
							else
							{
								attacker.SendLocalizedMessage(1004010); // You failed in your attempt to stun.
								defender.SendLocalizedMessage(1004011); // Your opponent tried to stun you and failed.
							}
						}
						else
						{
							attacker.SendLocalizedMessage(1004009); // You are too fatigued to attempt anything.
						}
					}
					else
					{
						attacker.SendLocalizedMessage(1004008); // You are not skilled enough to stun your opponent.
						attacker.StunReady = false;
					}
				}
			}
			else if (attacker.DisarmReady)
			{
				if (attacker.CanBeginAction(typeof(Fists)))
				{
					if (defender.Player || defender.Body.IsHuman)
					{
						if (attacker.Skills[SkillName.ArmsLore].Value >= 80.0 && attacker.Skills[SkillName.Wrestling].Value >= 80.0)
						{
							if (attacker.Stam >= 15)
							{
								var toDisarm = defender.FindItemOnLayer(Layer.OneHanded);

								if (toDisarm == null || !toDisarm.Movable)
								{
									toDisarm = defender.FindItemOnLayer(Layer.TwoHanded);
								}

								var pack = defender.Backpack;

								if (pack == null || toDisarm == null || !toDisarm.Movable)
								{
									attacker.SendLocalizedMessage(1004001); // You cannot disarm your opponent.
								}
								else if (CheckMove(attacker, SkillName.ArmsLore))
								{
									StartMoveDelay(attacker);

									attacker.Stam -= 15;
									attacker.DisarmReady = false;

									attacker.SendLocalizedMessage(1004006); // You successfully disarm your opponent!
									defender.SendLocalizedMessage(1004007); // You have been disarmed!

									pack.DropItem(toDisarm);
								}
								else
								{
									attacker.Stam -= 15;

									attacker.SendLocalizedMessage(1004004); // You failed in your attempt to disarm.
									defender.SendLocalizedMessage(1004005); // Your opponent tried to disarm you but failed.
								}
							}
							else
							{
								attacker.SendLocalizedMessage(1004003); // You are too fatigued to attempt anything.
							}
						}
						else
						{
							attacker.SendLocalizedMessage(1004002); // You are not skilled enough to disarm your opponent.
							attacker.DisarmReady = false;
						}
					}
					else
					{
						attacker.SendLocalizedMessage(1004001); // You cannot disarm your opponent.
					}
				}
			}
		}

		public override TimeSpan OnSwing(Mobile attacker, Mobile defender, double damageBonus)
		{
			if (!Core.AOS)
			{
				CheckPreAOSMoves(attacker, defender);
			}

			return base.OnSwing(attacker, defender, damageBonus);
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

			Delete();
		}

		/* Wrestling moves */

		private static bool CheckMove(Mobile m, SkillName other)
		{
			var wresValue = m.Skills[SkillName.Wrestling].Value;
			var scndValue = m.Skills[other].Value;

			/* 40% chance at 80, 80
			 * 50% chance at 100, 100
			 * 60% chance at 120, 120
			 */

			var chance = (wresValue + scndValue) / 400.0;

			return (chance >= Utility.RandomDouble());
		}

		private static bool HasFreeHands(Mobile m)
		{
			var item = m.FindItemOnLayer(Layer.OneHanded);

			if (item != null && !(item is Spellbook))
			{
				return false;
			}

			return m.FindItemOnLayer(Layer.TwoHanded) == null;
		}

		private static void EventSink_DisarmRequest(DisarmRequestEventArgs e)
		{
			if (Core.AOS)
			{
				return;
			}

			var m = e.Mobile;

			#region Dueling
			if (!Engines.ConPVP.DuelContext.AllowSpecialAbility(m, "Disarm", true))
			{
				return;
			}
			#endregion

			var armsValue = m.Skills[SkillName.ArmsLore].Value;
			var wresValue = m.Skills[SkillName.Wrestling].Value;

			if (!HasFreeHands(m))
			{
				m.SendLocalizedMessage(1004029); // You must have your hands free to attempt to disarm your opponent.
				m.DisarmReady = false;
			}
			else if (armsValue >= 80.0 && wresValue >= 80.0)
			{
				m.DisruptiveAction();
				m.DisarmReady = !m.DisarmReady;
				m.SendLocalizedMessage(m.DisarmReady ? 1019013 : 1019014);
			}
			else
			{
				m.SendLocalizedMessage(1004002); // You are not skilled enough to disarm your opponent.
				m.DisarmReady = false;
			}
		}

		private static void EventSink_StunRequest(StunRequestEventArgs e)
		{
			if (Core.AOS)
			{
				return;
			}

			var m = e.Mobile;

			#region Dueling
			if (!Engines.ConPVP.DuelContext.AllowSpecialAbility(m, "Stun", true))
			{
				return;
			}
			#endregion

			var anatValue = m.Skills[SkillName.Anatomy].Value;
			var wresValue = m.Skills[SkillName.Wrestling].Value;

			if (!HasFreeHands(m))
			{
				m.SendLocalizedMessage(1004031); // You must have your hands free to attempt to stun your opponent.
				m.StunReady = false;
			}
			else if (anatValue >= 80.0 && wresValue >= 80.0)
			{
				m.DisruptiveAction();
				m.StunReady = !m.StunReady;
				m.SendLocalizedMessage(m.StunReady ? 1019011 : 1019012);
			}
			else
			{
				m.SendLocalizedMessage(1004008); // You are not skilled enough to stun your opponent.
				m.StunReady = false;
			}
		}

		private class MoveDelayTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public MoveDelayTimer(Mobile m) : base(TimeSpan.FromSeconds(10.0))
			{
				m_Mobile = m;

				Priority = TimerPriority.TwoFiftyMS;

				m_Mobile.BeginAction(typeof(Fists));
			}

			protected override void OnTick()
			{
				m_Mobile.EndAction(typeof(Fists));
			}
		}

		private static void StartMoveDelay(Mobile m)
		{
			new MoveDelayTimer(m).Start();
		}
	}

	/// NinjaWeapon
	public interface INinjaAmmo : IUsesRemaining
	{
		int PoisonCharges { get; set; }
		Poison Poison { get; set; }
	}

	public interface INinjaWeapon : IUsesRemaining
	{
		int NoFreeHandMessage { get; }
		int EmptyWeaponMessage { get; }
		int RecentlyUsedMessage { get; }
		int FullWeaponMessage { get; }
		int WrongAmmoMessage { get; }
		Type AmmoType { get; }
		int PoisonCharges { get; set; }
		Poison Poison { get; set; }
		int WeaponDamage { get; }
		int WeaponMinRange { get; }
		int WeaponMaxRange { get; }

		void AttackAnimation(Mobile from, Mobile to);
	}

	public class NinjaWeapon
	{
		private const int MaxUses = 10;

		public static void AttemptShoot(PlayerMobile from, INinjaWeapon weapon)
		{
			if (CanUseWeapon(from, weapon))
			{
				from.BeginTarget(weapon.WeaponMaxRange, false, TargetFlags.Harmful, new TargetStateCallback<INinjaWeapon>(OnTarget), weapon);
			}
		}

		private static void Shoot(PlayerMobile from, Mobile target, INinjaWeapon weapon)
		{
			if (from != target && CanUseWeapon(from, weapon) && from.CanBeHarmful(target))
			{
				if (weapon.WeaponMinRange == 0 || !from.InRange(target, weapon.WeaponMinRange))
				{
					from.NinjaWepCooldown = true;

					from.Direction = from.GetDirectionTo(target);

					from.RevealingAction();

					weapon.AttackAnimation(from, target);

					ConsumeUse(weapon);

					if (CombatCheck(from, target))
					{
						Timer.DelayCall(TimeSpan.FromSeconds(1.0), OnHit, new object[] { from, target, weapon });
					}

					Timer.DelayCall(TimeSpan.FromSeconds(2.5), ResetUsing, from);
				}
				else
				{
					from.SendLocalizedMessage(1063303); // Your target is too close!
				}
			}
		}

		private static void ResetUsing(PlayerMobile from)
		{
			from.NinjaWepCooldown = false;
		}

		private static void Unload(Mobile from, INinjaWeapon weapon)
		{
			if (weapon.UsesRemaining > 0)
			{
				var ammo = Activator.CreateInstance(weapon.AmmoType, new object[] { weapon.UsesRemaining }) as INinjaAmmo;

				ammo.Poison = weapon.Poison;
				ammo.PoisonCharges = weapon.PoisonCharges;

				from.AddToBackpack((Item)ammo);

				weapon.UsesRemaining = 0;
				weapon.PoisonCharges = 0;
				weapon.Poison = null;
			}
		}

		private static void Reload(PlayerMobile from, INinjaWeapon weapon, INinjaAmmo ammo)
		{
			if (weapon.UsesRemaining < MaxUses)
			{
				var need = Math.Min((MaxUses - weapon.UsesRemaining), ammo.UsesRemaining);

				if (need > 0)
				{
					if (weapon.Poison != null && (ammo.Poison == null || weapon.Poison.Level > ammo.Poison.Level))
					{
						from.SendLocalizedMessage(1070767); // Loaded projectile is stronger, unload it first
					}
					else
					{
						if (weapon.UsesRemaining > 0)
						{
							if ((weapon.Poison == null && ammo.Poison != null)
								|| ((weapon.Poison != null && ammo.Poison != null) && weapon.Poison.Level != ammo.Poison.Level))
							{
								Unload(from, weapon);
								need = Math.Min(MaxUses, ammo.UsesRemaining);
							}
						}
						var poisonneeded = Math.Min((MaxUses - weapon.PoisonCharges), ammo.PoisonCharges);

						weapon.UsesRemaining += need;
						weapon.PoisonCharges += poisonneeded;

						if (weapon.PoisonCharges > 0)
						{
							weapon.Poison = ammo.Poison;
						}

						ammo.PoisonCharges -= poisonneeded;
						ammo.UsesRemaining -= need;

						if (ammo.UsesRemaining < 1)
						{
							((Item)ammo).Delete();
						}
						else if (ammo.PoisonCharges < 1)
						{
							ammo.Poison = null;
						}
					}
				} // "else" here would mean they targeted "ammo" with 0 uses.  undefined behavior.
			}
			else
			{
				from.SendLocalizedMessage(weapon.FullWeaponMessage);
			}
		}

		private static void ConsumeUse(INinjaWeapon weapon)
		{
			if (weapon.UsesRemaining > 0)
			{
				weapon.UsesRemaining--;

				if (weapon.UsesRemaining < 1)
				{
					weapon.PoisonCharges = 0;
					weapon.Poison = null;
				}
			}
		}

		private static bool CanUseWeapon(PlayerMobile from, INinjaWeapon weapon)
		{
			if (WeaponIsValid(weapon, from))
			{
				if (weapon.UsesRemaining > 0)
				{
					if (!from.NinjaWepCooldown)
					{
						if (BasePotion.HasFreeHand(from))
						{
							return true;
						}
						else
						{
							from.SendLocalizedMessage(weapon.NoFreeHandMessage);
						}
					}
					else
					{
						from.SendLocalizedMessage(weapon.RecentlyUsedMessage);
					}
				}
				else
				{
					from.SendLocalizedMessage(weapon.EmptyWeaponMessage);
				}
			}
			return false;
		}

		private static bool CombatCheck(Mobile attacker, Mobile defender) /* mod'd from baseweapon */
		{
			var defWeapon = defender.Weapon as BaseWeapon;

			var atkSkill = defender.Skills.Ninjitsu;
			var defSkill = defender.Skills[defWeapon.Skill];

			var atSkillValue = attacker.Skills.Ninjitsu.Value;
			var defSkillValue = defWeapon.GetDefendSkillValue(attacker, defender);

			double attackValue = AosAttributes.GetValue(attacker, AosAttribute.AttackChance);

			if (defSkillValue <= -20.0)
			{
				defSkillValue = -19.9;
			}

			if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
			{
				attackValue += 10;
			}

			if (AnimalFormSpell.UnderTransformation(attacker, typeof(GreyWolf)) || AnimalFormSpell.UnderTransformation(attacker, typeof(BakeKitsune)))
			{
				attackValue += 20;
			}

			if (HitLower.IsUnderAttackEffect(attacker))
			{
				attackValue -= 25;
			}

			if (attackValue > 45)
			{
				attackValue = 45;
			}

			attackValue = (atSkillValue + 20.0) * (100 + attackValue);

			double defenseValue = AosAttributes.GetValue(defender, AosAttribute.DefendChance);

			if (Spells.Chivalry.DivineFurySpell.UnderEffect(defender))
			{
				defenseValue -= 20;
			}

			if (HitLower.IsUnderDefenseEffect(defender))
			{
				defenseValue -= 25;
			}

			var refBonus = 0;

			if (Block.GetBonus(defender, ref refBonus))
			{
				defenseValue += refBonus;
			}

			if (SkillHandlers.Discordance.GetEffect(attacker, ref refBonus))
			{
				defenseValue -= refBonus;
			}

			if (defenseValue > 45)
			{
				defenseValue = 45;
			}

			defenseValue = (defSkillValue + 20.0) * (100 + defenseValue);

			var chance = attackValue / (defenseValue * 2.0);

			if (chance < 0.02)
			{
				chance = 0.02;
			}

			return attacker.CheckSkill(atkSkill.SkillName, chance);
		}

		private static void OnHit(object[] states)
		{
			var from = states[0] as Mobile;
			var target = states[1] as Mobile;
			var weapon = states[2] as INinjaWeapon;

			if (from.CanBeHarmful(target))
			{
				from.DoHarmful(target);

				AOS.Damage(target, from, weapon.WeaponDamage, 100, 0, 0, 0, 0);

				if (weapon.Poison != null && weapon.PoisonCharges > 0)
				{
					if (EvilOmenSpell.TryEndEffect(target))
					{
						target.ApplyPoison(from, Poison.GetPoison(weapon.Poison.Level + 1));
					}
					else
					{
						target.ApplyPoison(from, weapon.Poison);
					}

					weapon.PoisonCharges--;

					if (weapon.PoisonCharges < 1)
					{
						weapon.Poison = null;
					}
				}
			}
		}

		private static void OnTarget(Mobile from, object targeted, INinjaWeapon weapon)
		{
			var player = from as PlayerMobile;

			if (WeaponIsValid(weapon, from))
			{
				if (targeted is Mobile)
				{
					Shoot(player, (Mobile)targeted, weapon);
				}
				else if (targeted.GetType() == weapon.AmmoType)
				{
					Reload(player, weapon, (INinjaAmmo)targeted);
				}
				else
				{
					player.SendLocalizedMessage(weapon.WrongAmmoMessage);
				}
			}
		}

		private static bool WeaponIsValid(INinjaWeapon weapon, Mobile from)
		{
			var item = weapon as Item;

			if (!item.Deleted && item.RootParent == from)
			{
				return true;
			}
			return false;
		}

		public class LoadEntry : ContextMenuEntry
		{
			private readonly INinjaWeapon weapon;

			public LoadEntry(INinjaWeapon wep, int entry)
				: base(entry, 0)
			{
				weapon = wep;
			}

			public override void OnClick()
			{
				if (WeaponIsValid(weapon, Owner.From))
				{
					Owner.From.BeginTarget(10, false, TargetFlags.Harmful, new TargetStateCallback<INinjaWeapon>(OnTarget), weapon);
				}
			}
		}

		public class UnloadEntry : ContextMenuEntry
		{
			private readonly INinjaWeapon weapon;

			public UnloadEntry(INinjaWeapon wep, int entry)
				: base(entry, 0)
			{
				weapon = wep;

				Enabled = (weapon.UsesRemaining > 0);
			}

			public override void OnClick()
			{
				if (WeaponIsValid(weapon, Owner.From))
				{
					Unload(Owner.From, weapon);
				}
			}
		}
	}
}