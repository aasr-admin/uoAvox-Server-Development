using Server.Engines.Craft;
using Server.Items;
using Server.Network;

using System;

namespace Server.Factions
{
	public enum AllowedPlacing
	{
		Everywhere,

		AnyFactionTown,
		ControlledFactionTown,
		FactionStronghold
	}

	public abstract class BaseFactionTrap : BaseTrap
	{
		private Faction m_Faction;
		private Mobile m_Placer;
		private DateTime m_TimeOfPlacement;

		private Timer m_Concealing;

		[CommandProperty(AccessLevel.GameMaster)]
		public Faction Faction
		{
			get => m_Faction;
			set => m_Faction = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Placer
		{
			get => m_Placer;
			set => m_Placer = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime TimeOfPlacement
		{
			get => m_TimeOfPlacement;
			set => m_TimeOfPlacement = value;
		}

		public virtual int EffectSound => 0;

		public virtual int SilverFromDisarm => 100;

		public virtual int MessageHue => 0;

		public virtual int AttackMessage => 0;
		public virtual int DisarmMessage => 0;

		public virtual AllowedPlacing AllowedPlacing => AllowedPlacing.Everywhere;

		public virtual TimeSpan ConcealPeriod => TimeSpan.FromMinutes(1.0);

		public virtual TimeSpan DecayPeriod
		{
			get
			{
				if (Core.AOS)
				{
					return TimeSpan.FromDays(1.0);
				}

				return TimeSpan.MaxValue; // no decay
			}
		}

		public override void OnTrigger(Mobile from)
		{
			if (!IsEnemy(from))
			{
				return;
			}

			Conceal();

			DoVisibleEffect();
			Effects.PlaySound(Location, Map, EffectSound);
			DoAttackEffect(from);

			var silverToAward = (from.Alive ? 20 : 40);

			if (silverToAward > 0 && m_Placer != null && m_Faction != null)
			{
				var victimState = PlayerState.Find(from);

				if (victimState != null && victimState.CanGiveSilverTo(m_Placer) && victimState.KillPoints > 0)
				{
					var silverGiven = m_Faction.AwardSilver(m_Placer, silverToAward);

					if (silverGiven > 0)
					{
						// TODO: Get real message
						if (from.Alive)
						{
							m_Placer.SendMessage("You have earned {0} silver pieces because {1} fell for your trap.", silverGiven, from.Name);
						}
						else
						{
							m_Placer.SendLocalizedMessage(1042736, String.Format("{0} silver\t{1}", silverGiven, from.Name)); // You have earned ~1_SILVER_AMOUNT~ pieces for vanquishing ~2_PLAYER_NAME~!
						}
					}

					victimState.OnGivenSilverTo(m_Placer);
				}
			}

			from.LocalOverheadMessage(MessageType.Regular, MessageHue, AttackMessage);
		}

		public abstract void DoVisibleEffect();
		public abstract void DoAttackEffect(Mobile m);

		public virtual int IsValidLocation()
		{
			return IsValidLocation(GetWorldLocation(), Map);
		}

		public virtual int IsValidLocation(Point3D p, Map m)
		{
			if (m == null)
			{
				return 502956; // You cannot place a trap on that.
			}

			if (Core.ML)
			{
				foreach (var item in m.GetItemsInRange(p, 0))
				{
					if (item is BaseFactionTrap && ((BaseFactionTrap)item).Faction == Faction)
					{
						return 1075263; // There is already a trap belonging to your faction at this location.;
					}
				}
			}

			switch (AllowedPlacing)
			{
				case AllowedPlacing.FactionStronghold:
					{
						var region = (StrongholdRegion)Region.Find(p, m).GetRegion(typeof(StrongholdRegion));

						if (region != null && region.Faction == m_Faction)
						{
							return 0;
						}

						return 1010355; // This trap can only be placed in your stronghold
					}
				case AllowedPlacing.AnyFactionTown:
					{
						var town = Town.FromRegion(Region.Find(p, m));

						if (town != null)
						{
							return 0;
						}

						return 1010356; // This trap can only be placed in a faction town
					}
				case AllowedPlacing.ControlledFactionTown:
					{
						var town = Town.FromRegion(Region.Find(p, m));

						if (town != null && town.Owner == m_Faction)
						{
							return 0;
						}

						return 1010357; // This trap can only be placed in a town your faction controls
					}
			}

			return 0;
		}

		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			base.OnMovement(m, oldLocation);

			if (!CheckDecay() && CheckRange(m.Location, oldLocation, 6))
			{
				if (Faction.Find(m) != null && ((m.Skills[SkillName.DetectHidden].Value - 80.0) / 20.0) > Utility.RandomDouble())
				{
					PrivateOverheadLocalizedMessage(m, 1010154, MessageHue, "", ""); // [Faction Trap]
				}
			}
		}

		public void PrivateOverheadLocalizedMessage(Mobile to, int number, int hue, string name, string args)
		{
			if (to == null)
			{
				return;
			}

			var ns = to.NetState;

			if (ns != null)
			{
				ns.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, hue, 3, number, name, args));
			}
		}

		public BaseFactionTrap(Faction f, Mobile m, int itemID) : base(itemID)
		{
			Visible = false;

			m_Faction = f;
			m_TimeOfPlacement = DateTime.UtcNow;
			m_Placer = m;
		}

		public BaseFactionTrap(Serial serial) : base(serial)
		{
		}

		public virtual bool CheckDecay()
		{
			var decayPeriod = DecayPeriod;

			if (decayPeriod == TimeSpan.MaxValue)
			{
				return false;
			}

			if ((m_TimeOfPlacement + decayPeriod) < DateTime.UtcNow)
			{
				Timer.DelayCall(TimeSpan.Zero, Delete);
				return true;
			}

			return false;
		}

		public virtual void BeginConceal()
		{
			if (m_Concealing != null)
			{
				m_Concealing.Stop();
			}

			m_Concealing = Timer.DelayCall(ConcealPeriod, Conceal);
		}

		public virtual void Conceal()
		{
			if (m_Concealing != null)
			{
				m_Concealing.Stop();
			}

			m_Concealing = null;

			if (!Deleted)
			{
				Visible = false;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			Faction.WriteReference(writer, m_Faction);
			writer.Write(m_Placer);
			writer.Write(m_TimeOfPlacement);

			if (Visible)
			{
				BeginConceal();
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			m_Faction = Faction.ReadReference(reader);
			m_Placer = reader.ReadMobile();
			m_TimeOfPlacement = reader.ReadDateTime();

			if (Visible)
			{
				BeginConceal();
			}

			CheckDecay();
		}

		public override void OnDelete()
		{
			if (m_Faction != null && m_Faction.Traps.Contains(this))
			{
				m_Faction.Traps.Remove(this);
			}

			base.OnDelete();
		}

		public virtual bool IsEnemy(Mobile mob)
		{
			if (mob.Hidden && mob.AccessLevel > AccessLevel.Player)
			{
				return false;
			}

			if (!mob.Alive || mob.IsDeadBondedPet)
			{
				return false;
			}

			var faction = Faction.Find(mob, true);

			if (faction == null && mob is BaseFactionGuard)
			{
				faction = ((BaseFactionGuard)mob).Faction;
			}

			if (faction == null)
			{
				return false;
			}

			return (faction != m_Faction);
		}
	}

	public abstract class BaseFactionTrapDeed : Item, ICraftable
	{
		public abstract Type TrapType { get; }

		private Faction m_Faction;

		[CommandProperty(AccessLevel.GameMaster)]
		public Faction Faction
		{
			get => m_Faction;
			set
			{
				m_Faction = value;

				if (m_Faction != null)
				{
					Hue = m_Faction.Definition.HuePrimary;
				}
			}
		}

		public BaseFactionTrapDeed(int itemID) : base(itemID)
		{
			Weight = 1.0;
			LootType = LootType.Blessed;
		}

		public BaseFactionTrapDeed(bool createdFromDeed) : this(0x14F0)
		{
		}

		public BaseFactionTrapDeed(Serial serial) : base(serial)
		{
		}

		public virtual BaseFactionTrap Construct(Mobile from)
		{
			try { return Activator.CreateInstance(TrapType, new object[] { m_Faction, from }) as BaseFactionTrap; }
			catch { return null; }
		}

		public override void OnDoubleClick(Mobile from)
		{
			var faction = Faction.Find(from);

			if (faction == null)
			{
				from.SendLocalizedMessage(1010353, "", 0x23); // Only faction members may place faction traps
			}
			else if (faction != m_Faction)
			{
				from.SendLocalizedMessage(1010354, "", 0x23); // You may only place faction traps created by your faction
			}
			else if (faction.Traps.Count >= faction.MaximumTraps)
			{
				from.SendLocalizedMessage(1010358, "", 0x23); // Your faction already has the maximum number of traps placed
			}
			else
			{
				var trap = Construct(from);

				if (trap == null)
				{
					return;
				}

				var message = trap.IsValidLocation(from.Location, from.Map);

				if (message > 0)
				{
					from.SendLocalizedMessage(message, "", 0x23);
					trap.Delete();
				}
				else
				{
					from.SendLocalizedMessage(1010360); // You arm the trap and carefully hide it from view
					trap.MoveToWorld(from.Location, from.Map);
					faction.Traps.Add(trap);
					Delete();
				}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			Faction.WriteReference(writer, m_Faction);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			m_Faction = Faction.ReadReference(reader);
		}

		#region ICraftable

		public virtual int OnCraft(int quality, bool makersMark, Mobile from, ICraftSystem craftSystem, Type typeRes, ICraftTool tool, ICraftItem craftItem, int resHue)
		{
			ItemID = 0x14F0;
			Faction = Faction.Find(from);

			return 1;
		}

		#endregion
	}
}