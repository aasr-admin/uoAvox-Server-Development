using Server.Commands;
using Server.Mobiles;
using Server.Spells;

using System;
using System.Collections.Generic;
using System.Xml;

namespace Server.Regions
{
	public class GuardedRegion : BaseRegion
	{
		public static void Initialize()
		{
			CommandSystem.Register("CheckGuarded", AccessLevel.GameMaster, new CommandEventHandler(CheckGuarded_OnCommand));
			CommandSystem.Register("SetGuarded", AccessLevel.Administrator, new CommandEventHandler(SetGuarded_OnCommand));
			CommandSystem.Register("ToggleGuarded", AccessLevel.Administrator, new CommandEventHandler(ToggleGuarded_OnCommand));
		}

		[Usage("CheckGuarded")]
		[Description("Returns a value indicating if the current region is guarded or not.")]
		private static void CheckGuarded_OnCommand(CommandEventArgs e)
		{
			var from = e.Mobile;
			var reg = from.Region.GetRegion<GuardedRegion>();

			if (reg == null)
			{
				from.SendMessage("You are not in a guardable region.");
			}
			else if (reg.Disabled)
			{
				from.SendMessage("The guards in this region have been disabled.");
			}
			else
			{
				from.SendMessage("This region is actively guarded.");
			}
		}

		[Usage("SetGuarded <true|false>")]
		[Description("Enables or disables guards for the current region.")]
		private static void SetGuarded_OnCommand(CommandEventArgs e)
		{
			var from = e.Mobile;

			if (e.Length == 1)
			{
				var reg = from.Region.GetRegion<GuardedRegion>();

				if (reg == null)
				{
					from.SendMessage("You are not in a guardable region.");
				}
				else
				{
					reg.Disabled = !e.GetBoolean(0);

					if (reg.Disabled)
					{
						from.SendMessage("The guards in this region have been disabled.");
					}
					else
					{
						from.SendMessage("The guards in this region have been enabled.");
					}
				}
			}
			else
			{
				from.SendMessage("Format: SetGuarded <true|false>");
			}
		}

		[Usage("ToggleGuarded")]
		[Description("Toggles the state of guards for the current region.")]
		private static void ToggleGuarded_OnCommand(CommandEventArgs e)
		{
			var from = e.Mobile;
			var reg = from.Region.GetRegion<GuardedRegion>();

			if (reg == null)
			{
				from.SendMessage("You are not in a guardable region.");
			}
			else
			{
				reg.Disabled = !reg.Disabled;

				if (reg.Disabled)
				{
					from.SendMessage("The guards in this region have been disabled.");
				}
				else
				{
					from.SendMessage("The guards in this region have been enabled.");
				}
			}
		}

		public static GuardedRegion Disable(GuardedRegion reg)
		{
			if (reg != null)
			{
				reg.Disabled = true;
			}

			return reg;
		}

		private readonly Dictionary<Mobile, GuardTimer> m_GuardCandidates = new();

		public virtual Type DefaultGuardType => (Map == Map.Ilshenar || Map == Map.Malas) ? typeof(ArcherGuard) : typeof(WarriorGuard);

		protected Type m_GuardType;

		[TypeFilter(true, true, typeof(BaseGuard))]
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Type GuardType
		{
			get => m_GuardType ?? DefaultGuardType;
			set
			{
				if (value == null || value == DefaultGuardType)
				{
					m_GuardType = null;
				}
				else if (!value.IsAbstract && value.IsSubclassOf(typeof(BaseGuard)))
				{
					m_GuardType = value;
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Disabled { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool MurderersAllowed { get; set; }

		public GuardedRegion(string name, Map map, int priority, params Rectangle2D[] area) : base(name, map, priority, area)
		{
		}

		public GuardedRegion(string name, Map map, int priority, params Poly2D[] area) : base(name, map, priority, area)
		{
		}

		public GuardedRegion(string name, Map map, int priority, params Rectangle3D[] area) : base(name, map, priority, area)
		{
		}

		public GuardedRegion(string name, Map map, int priority, params Poly3D[] area) : base(name, map, priority, area)
		{
		}

		public GuardedRegion(string name, Map map, Region parent, params Rectangle2D[] area) : base(name, map, parent, area)
		{
		}

		public GuardedRegion(string name, Map map, Region parent, params Poly2D[] area) : base(name, map, parent, area)
		{
		}

		public GuardedRegion(string name, Map map, Region parent, params Rectangle3D[] area) : base(name, map, parent, area)
		{
		}

		public GuardedRegion(string name, Map map, Region parent, params Poly3D[] area) : base(name, map, parent, area)
		{
		}

		public GuardedRegion(RegionDefinition def, Map map, Region parent) : base(def, map, parent)
		{
		}

		public GuardedRegion(int id) : base(id)
		{
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			GuardType = null;

			Disabled = false;
			MurderersAllowed = true;
			HousingAllowed = false;
		}

		public virtual bool CheckVendorAccess(BaseVendor vendor, Mobile from)
		{
			if (from.AccessLevel >= AccessLevel.GameMaster || Disabled)
			{
				return true;
			}

			if (!from.Murderer || MurderersAllowed)
			{
				return true;
			}

			return false;
		}

		public override bool OnBeginSpellCast(Mobile m, ISpell s)
		{
			return base.OnBeginSpellCast(m, s) && SpellHelper.CheckTown(s, this);
		}

		public virtual BaseGuard MakeGuard(Point3D location)
		{
			BaseGuard useGuard = null;

			var eable = Map.GetMobilesInRange(location, 8);

			foreach (var m in eable)
			{
				if (m is BaseGuard g && g.FocusMob?.Deleted != false)
				{
					useGuard = g;
					break;
				}
			}

			eable.Free();

			if (useGuard?.Deleted != false)
			{
				useGuard = null;

				var type = GuardType;

				if (type != null && !type.IsAbstract && type.IsSubclassOf(typeof(BaseGuard)))
				{
					try
					{
						useGuard = (BaseGuard)Activator.CreateInstance(type);
					}
					catch
					{
						m_GuardType = null;
					}
				}
				else 
				{
					GuardType = null;
				}
			}

			return useGuard;
		}

		public override void OnEnter(Mobile m)
		{
			if (!Disabled)
			{
				if (!MurderersAllowed && m.Murderer)
				{
					CheckGuardCandidate(m);
				}
			}

			base.OnEnter(m);
		}

		public override void OnSpeech(SpeechEventArgs args)
		{
			base.OnSpeech(args);

			if (!Disabled)
			{
				if (args.Mobile.Alive && args.HasKeyword(0x0007)) // *guards*
				{
					CallGuards(args.Mobile.Location);
				}
			}
		}

		public override void OnAggressed(Mobile aggressor, Mobile aggressed, bool criminal)
		{
			base.OnAggressed(aggressor, aggressed, criminal);

			if (!Disabled)
			{
				if (aggressor != aggressed && criminal)
				{
					CheckGuardCandidate(aggressor);
				}
			}
		}

		public override void OnGotBeneficialAction(Mobile helper, Mobile helped)
		{
			base.OnGotBeneficialAction(helper, helped);

			if (!Disabled)
			{
				var noto = Notoriety.Compute(helper, helped);

				if (helper != helped && (noto == Notoriety.Criminal || noto == Notoriety.Murderer))
				{
					CheckGuardCandidate(helper);
				}
			}
		}

		public override void OnCriminalAction(Mobile m, bool message)
		{
			base.OnCriminalAction(m, message);

			if (!Disabled)
			{
				CheckGuardCandidate(m);
			}
		}

		public void CheckGuardCandidate(Mobile m)
		{
			if (IsGuardCandidate(m))
			{
				m_GuardCandidates.TryGetValue(m, out var timer);

				if (timer == null)
				{
					timer = new GuardTimer(m, m_GuardCandidates);
					timer.Start();

					m_GuardCandidates[m] = timer;

					m.SendLocalizedMessage(502275); // Guards can now be called on you!

					var map = m.Map;

					if (map != null)
					{
						Mobile fakeCall = null;

						var prio = 0.0;

						var eable = m.GetMobilesInRange(8);

						foreach (var v in eable)
						{
							if (!v.Player && v != m && !IsGuardCandidate(v) && (v is BaseCreature c ? c.IsHumanInTown() : (v.Body.IsHuman && v.Region.IsPartOf(this))))
							{
								var dist = m.GetDistanceToSqrt(v);

								if (fakeCall == null || dist < prio)
								{
									fakeCall = v;
									prio = dist;
								}
							}
						}

						eable.Free();

						if (fakeCall != null)
						{
							fakeCall.Say(Utility.RandomList(1007037, 501603, 1013037, 1013038, 1013039, 1013041, 1013042, 1013043, 1013052));

							var g = MakeGuard(m.Location);

							if (g?.Deleted == false)
							{
								g.Attack(m, true);
							}

							timer.Stop();

							m_GuardCandidates.Remove(m);

							m.SendLocalizedMessage(502276); // Guards can no longer be called on you.
						}
					}
				}
				else
				{
					timer.Stop();
					timer.Start();
				}
			}
		}

		public void CallGuards(Point3D p)
		{
			if (!Disabled)
			{
				var eable = Map.GetMobilesInRange(p, 14);

				foreach (var m in eable)
				{
					if (IsGuardCandidate(m))
					{
						if (m_GuardCandidates.TryGetValue(m, out var timer) || (!MurderersAllowed && m.Murderer && m.Region.IsPartOf(this)))
						{
							if (timer != null)
							{
								timer.Stop();

								m_GuardCandidates.Remove(m);
							}

							var g = MakeGuard(m.Location);

							if (g?.Deleted == false)
							{
								g.Attack(m, true);
							}

							m.SendLocalizedMessage(502276); // Guards can no longer be called on you.

							break;
						}
					}
				}

				eable.Free();
			}
		}

		public bool IsGuardCandidate(Mobile m)
		{
			if (!Disabled)
			{
				if (m is not BaseGuard && (m is not BaseCreature c || !c.IsInvulnerable))
				{
					if (m.Alive && m.AccessLevel < AccessLevel.Counselor && !m.Blessed)
					{
						if (m.Criminal)
						{
							return true;
						}

						if (!MurderersAllowed && m.Murderer)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(Disabled);
			writer.Write(MurderersAllowed);
			writer.WriteObjectType(GuardType);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();

			Disabled = reader.ReadBool();
			MurderersAllowed = reader.ReadBool();
			GuardType = reader.ReadObjectType();
		}

		private class GuardTimer : Timer
		{
			private readonly Mobile m_Mobile;
			private readonly Dictionary<Mobile, GuardTimer> m_Table;

			public GuardTimer(Mobile m, Dictionary<Mobile, GuardTimer> table) 
				: base(TimeSpan.FromSeconds(15.0))
			{
				Priority = TimerPriority.TwoFiftyMS;

				m_Mobile = m;
				m_Table = table;
			}

			protected override void OnTick()
			{
				if (m_Table.Remove(m_Mobile))
				{
					m_Mobile.SendLocalizedMessage(502276); // Guards can no longer be called on you.
				}
			}
		}
	}
}