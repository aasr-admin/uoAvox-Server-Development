using Server.Commands;
using Server.Engines.PartySystem;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Magery;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Spellweaving;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Server.Engines.ConPVP
{
	/// Participants
	public class Participant
	{
		private TournyParticipant m_TournyPart;

		public int Count => Players.Length;
		public DuelPlayer[] Players { get; private set; }
		public DuelContext Context { get; }
		public TournyParticipant TournyPart { get => m_TournyPart; set => m_TournyPart = value; }

		public DuelPlayer Find(Mobile mob)
		{
			if (mob is PlayerMobile pm)
			{
				if (pm.DuelContext == Context && pm.DuelPlayer.Participant == this)
				{
					return pm.DuelPlayer;
				}

				return null;
			}

			for (var i = 0; i < Players.Length; ++i)
			{
				if (Players[i] != null && Players[i].Mobile == mob)
				{
					return Players[i];
				}
			}

			return null;
		}

		public bool Contains(Mobile mob)
		{
			return Find(mob) != null;
		}

		public void Broadcast(int hue, string message, string nonLocalOverhead, string localOverhead)
		{
			for (var i = 0; i < Players.Length; ++i)
			{
				if (Players[i] != null)
				{
					if (message != null)
					{
						Players[i].Mobile.SendMessage(hue, message);
					}

					if (nonLocalOverhead != null)
					{
						Players[i].Mobile.NonlocalOverheadMessage(Network.MessageType.Regular, hue, false, String.Format(nonLocalOverhead, Players[i].Mobile.Name, Players[i].Mobile.Female ? "her" : "his"));
					}

					if (localOverhead != null)
					{
						Players[i].Mobile.LocalOverheadMessage(Network.MessageType.Regular, hue, false, localOverhead);
					}
				}
			}
		}

		public int FilledSlots
		{
			get
			{
				var count = 0;

				for (var i = 0; i < Players.Length; ++i)
				{
					if (Players[i] != null)
					{
						++count;
					}
				}

				return count;
			}
		}

		public bool HasOpenSlot
		{
			get
			{
				for (var i = 0; i < Players.Length; ++i)
				{
					if (Players[i] == null)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool Eliminated
		{
			get
			{
				for (var i = 0; i < Players.Length; ++i)
				{
					if (Players[i] != null && !Players[i].Eliminated)
					{
						return false;
					}
				}

				return true;
			}
		}

		public string NameList
		{
			get
			{
				var sb = new StringBuilder();

				for (var i = 0; i < Players.Length; ++i)
				{
					if (Players[i] == null)
					{
						continue;
					}

					var mob = Players[i].Mobile;

					if (sb.Length > 0)
					{
						_ = sb.Append(", ");
					}

					_ = sb.Append(mob.Name);
				}

				if (sb.Length == 0)
				{
					return "Empty";
				}

				return sb.ToString();
			}
		}

		public void Nullify(DuelPlayer player)
		{
			if (player == null)
			{
				return;
			}

			var index = Array.IndexOf(Players, player);

			if (index == -1)
			{
				return;
			}

			Players[index] = null;
		}

		public void Remove(DuelPlayer player)
		{
			if (player == null)
			{
				return;
			}

			var index = Array.IndexOf(Players, player);

			if (index == -1)
			{
				return;
			}

			var old = Players;
			Players = new DuelPlayer[old.Length - 1];

			for (var i = 0; i < index; ++i)
			{
				Players[i] = old[i];
			}

			for (var i = index + 1; i < old.Length; ++i)
			{
				Players[i - 1] = old[i];
			}
		}

		public void Remove(Mobile player)
		{
			Remove(Find(player));
		}

		public void Add(Mobile player)
		{
			if (Contains(player))
			{
				return;
			}

			for (var i = 0; i < Players.Length; ++i)
			{
				if (Players[i] == null)
				{
					Players[i] = new DuelPlayer(player, this);
					return;
				}
			}

			Resize(Players.Length + 1);
			Players[Players.Length - 1] = new DuelPlayer(player, this);
		}

		public void Resize(int count)
		{
			var old = Players;
			Players = new DuelPlayer[count];

			if (old != null)
			{
				var ct = 0;

				for (var i = 0; i < old.Length; ++i)
				{
					if (old[i] != null && ct < count)
					{
						Players[ct++] = old[i];
					}
				}
			}
		}

		public Participant(DuelContext context, int count)
		{
			Context = context;
			//m_Stakes = new StakesContainer( context, this );
			Resize(count);
		}
	}

	public class DuelPlayer
	{
		private bool m_Eliminated;
		private Participant m_Participant;

		public Mobile Mobile { get; }
		public bool Ready { get; set; }
		public bool Eliminated
		{
			get => m_Eliminated; set
			{
				m_Eliminated = value;
				if (m_Participant.Context.m_Tournament != null && m_Eliminated)
				{
					m_Participant.Context.m_Tournament.OnEliminated(this);
					Mobile.SendEverything();
				}
			}
		}
		public Participant Participant { get => m_Participant; set => m_Participant = value; }

		public DuelPlayer(Mobile mob, Participant p)
		{
			Mobile = mob;
			m_Participant = p;

			if (mob is PlayerMobile)
			{
				((PlayerMobile)mob).DuelPlayer = this;
			}
		}
	}

	public class ParticipantGump : Gump
	{
		private readonly Participant m_Participant;

		public Mobile From { get; }
		public DuelContext Context { get; }
		public Participant Participant => m_Participant;

		public string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public void AddGoldenButton(int x, int y, int bid)
		{
			AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
			AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
		}

		public void AddGoldenButtonLabeled(int x, int y, int bid, string text)
		{
			AddGoldenButton(x, y, bid);
			AddHtml(x + 25, y, 200, 20, text, false, false);
		}

		public ParticipantGump(Mobile from, DuelContext context, Participant p) : base(50, 50)
		{
			From = from;
			Context = context;
			m_Participant = p;

			_ = from.CloseGump(typeof(RulesetGump));
			_ = from.CloseGump(typeof(DuelContextGump));
			_ = from.CloseGump(typeof(ParticipantGump));

			var count = p.Players.Length;

			if (count < 4)
			{
				count = 4;
			}

			AddPage(0);

			var height = 35 + 10 + 22 + 22 + 30 + 22 + 2 + (count * 22) + 2 + 30;

			AddBackground(0, 0, 300, height, 9250);
			AddBackground(10, 10, 280, height - 20, 0xDAC);

			AddButton(240, 25, 0xFB1, 0xFB3, 3, GumpButtonType.Reply, 0);

			//AddButton( 223, 54, 0x265A, 0x265A, 4, GumpButtonType.Reply, 0 );

			AddHtml(35, 25, 230, 20, Center("Participant Setup"), false, false);

			var x = 35;
			var y = 47;

			AddHtml(x, y, 200, 20, String.Format("Team Size: {0}", p.Players.Length), false, false);
			y += 22;

			AddGoldenButtonLabeled(x + 20, y, 1, "Increase");
			y += 22;
			AddGoldenButtonLabeled(x + 20, y, 2, "Decrease");
			y += 30;

			AddHtml(35, y, 230, 20, Center("Players"), false, false);
			y += 22;

			for (var i = 0; i < p.Players.Length; ++i)
			{
				var pl = p.Players[i];

				AddGoldenButtonLabeled(x, y, 5 + i, String.Format("{0}: {1}", 1 + i, pl == null ? "Empty" : pl.Mobile.Name));
				y += 22;
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (!Context.Registered)
			{
				return;
			}

			var bid = info.ButtonID;

			if (bid == 0)
			{
				_ = From.SendGump(new DuelContextGump(From, Context));
			}
			else if (bid == 1)
			{
				if (m_Participant.Count < 8)
				{
					m_Participant.Resize(m_Participant.Count + 1);
				}
				else
				{
					From.SendMessage("You may not raise the team size any further.");
				}

				_ = From.SendGump(new ParticipantGump(From, Context, m_Participant));
			}
			else if (bid == 2)
			{
				if (m_Participant.Count > 1 && m_Participant.Count > m_Participant.FilledSlots)
				{
					m_Participant.Resize(m_Participant.Count - 1);
				}
				else
				{
					From.SendMessage("You may not lower the team size any further.");
				}

				_ = From.SendGump(new ParticipantGump(From, Context, m_Participant));
			}
			else if (bid == 3)
			{
				if (m_Participant.FilledSlots > 0)
				{
					From.SendMessage("There is at least one currently active player. You must remove them first.");
					_ = From.SendGump(new ParticipantGump(From, Context, m_Participant));
				}
				else if (Context.Participants.Count > 2)
				{
					/*Container cont = m_Participant.Stakes;

					if ( cont != null )
						cont.Delete();*/

					Context.Participants.Remove(m_Participant);
					_ = From.SendGump(new DuelContextGump(From, Context));
				}
				else
				{
					From.SendMessage("Duels must have at least two participating parties.");
					_ = From.SendGump(new ParticipantGump(From, Context, m_Participant));
				}
			}
			/*else if ( bid == 4 )
			{
				m_From.SendGump( new ParticipantGump( m_From, m_Context, m_Participant ) );

				Container cont = m_Participant.Stakes;

				if ( cont != null && !cont.Deleted )
				{
					cont.DisplayTo( m_From );

					Item[] checks = cont.FindItemsByType( typeof( BankCheck ) );

					int gold = cont.TotalGold;

					for ( int i = 0; i < checks.Length; ++i )
						gold += ((BankCheck)checks[i]).Worth;

					m_From.SendMessage( "This container has {0} item{1} and {2} stone{3}. In gold or check form there is a total of {4:D}gp.", cont.TotalItems, cont.TotalItems==1?"":"s", cont.TotalWeight, cont.TotalWeight==1?"":"s", gold );
				}
			}*/
			else
			{
				bid -= 5;

				if (bid >= 0 && bid < m_Participant.Players.Length)
				{
					if (m_Participant.Players[bid] == null)
					{
						From.Target = new ParticipantTarget(Context, m_Participant, bid);
						From.SendMessage("Target a player.");
					}
					else
					{
						m_Participant.Players[bid].Mobile.SendMessage("You have been removed from the duel.");

						if (m_Participant.Players[bid].Mobile is PlayerMobile)
						{
							((PlayerMobile)m_Participant.Players[bid].Mobile).DuelPlayer = null;
						}

						m_Participant.Players[bid] = null;
						From.SendMessage("They have been removed from the duel.");
						_ = From.SendGump(new ParticipantGump(From, Context, m_Participant));
					}
				}
			}
		}

		private class ParticipantTarget : Target
		{
			private readonly DuelContext m_Context;
			private readonly Participant m_Participant;
			private readonly int m_Index;

			public ParticipantTarget(DuelContext context, Participant p, int index) : base(12, false, TargetFlags.None)
			{
				m_Context = context;
				m_Participant = p;
				m_Index = index;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (!m_Context.Registered)
				{
					return;
				}

				var index = m_Index;

				if (index < 0 || index >= m_Participant.Players.Length)
				{
					return;
				}


				if (targeted is not Mobile mob)
				{
					from.SendMessage("That is not a player.");
				}
				else if (!mob.Player)
				{
					if (mob.Body.IsHuman)
					{
						mob.SayTo(from, 1005443); // Nay, I would rather stay here and watch a nail rust.
					}
					else
					{
						mob.SayTo(from, 1005444); // The creature ignores your offer.
					}
				}
				else if (AcceptDuelGump.IsIgnored(mob, from) || mob.Blessed)
				{
					from.SendMessage("They ignore your offer.");
				}
				else
				{
					if (mob is not PlayerMobile pm)
					{
						return;
					}

					if (pm.DuelContext != null)
					{
						from.SendMessage("{0} cannot fight because they are already assigned to another duel.", pm.Name);
					}
					else if (DuelContext.CheckCombat(pm))
					{
						from.SendMessage("{0} cannot fight because they have recently been in combat with another player.", pm.Name);
					}
					else if (mob.HasGump(typeof(AcceptDuelGump)))
					{
						from.SendMessage("{0} has already been offered a duel.");
					}
					else
					{
						from.SendMessage("You send {0} to {1}.", m_Participant.Find(from) == null ? "a challenge" : "an invitation", mob.Name);
						_ = mob.SendGump(new AcceptDuelGump(from, mob, m_Context, m_Participant, m_Index));
					}
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				_ = from.SendGump(new ParticipantGump(from, m_Context, m_Participant));
			}
		}
	}

	/// Duel Region
	public class SafeZone : GuardedRegion
	{
		public static readonly int SafeZonePriority = HouseRegion.HousePriority + 1;

		public SafeZone(Rectangle2D area, Point3D goloc, Map map, bool isGuarded) : this(null, map, SafeZonePriority, area)
		{
			GoLocation = goloc;

			Disabled = !isGuarded;

			Register();
		}

		public SafeZone(string name, Map map, int priority, params Rectangle2D[] area) : base(name, map, priority, area)
		{
		}

		public SafeZone(string name, Map map, int priority, params Poly2D[] area) : base(name, map, priority, area)
		{
		}

		public SafeZone(string name, Map map, int priority, params Rectangle3D[] area) : base(name, map, priority, area)
		{
		}

		public SafeZone(string name, Map map, int priority, params Poly3D[] area) : base(name, map, priority, area)
		{
		}

		public SafeZone(string name, Map map, Region parent, params Rectangle2D[] area) : base(name, map, parent, area)
		{
		}

		public SafeZone(string name, Map map, Region parent, params Poly2D[] area) : base(name, map, parent, area)
		{
		}

		public SafeZone(string name, Map map, Region parent, params Rectangle3D[] area) : base(name, map, parent, area)
		{
		}

		public SafeZone(string name, Map map, Region parent, params Poly3D[] area) : base(name, map, parent, area)
		{
		}

		public SafeZone(RegionDefinition def, Map map, Region parent) : base(def, map, parent)
		{
		}

		public SafeZone(int id) : base(id)
		{
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			Rules.AllowHouses = false;
			Rules.AllowVehicles = false;
		}

		public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
		{
			if (m.Player && Sigil.ExistsOn(m))
			{
				m.SendMessage(0x22, "You are holding a sigil and cannot enter this zone.");
				return false;
			}

			var pm = m as PlayerMobile;

			if (pm == null && m is BaseCreature bc && bc.Summoned)
			{
				pm = bc.SummonMaster as PlayerMobile;
			}

			if (pm != null && pm.DuelContext != null && pm.DuelContext.StartedBeginCountdown)
			{
				return true;
			}

			if (DuelContext.CheckCombat(m))
			{
				m.SendMessage(0x22, "You have recently been in combat and cannot enter this zone.");
				return false;
			}

			return base.OnMoveInto(m, d, newLocation, oldLocation);
		}

		public override void OnEnter(Mobile m)
		{
			m.SendMessage("You have entered a dueling safezone. No combat other than duels are allowed in this zone.");

			base.OnEnter(m);
		}

		public override void OnExit(Mobile m)
		{
			m.SendMessage("You have left a dueling safezone. Combat is now unrestricted.");

			base.OnExit(m);
		}

		public override bool CanUseStuckMenu(Mobile m)
		{
			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();
		}
	}

	/// Duel Ruleset
	public class PickRulesetGump : Gump
	{
		private readonly Mobile m_From;
		private readonly DuelContext m_Context;
		private readonly Ruleset m_Ruleset;
		private readonly Ruleset[] m_Defaults;
		private readonly Ruleset[] m_Flavors;

		public string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public PickRulesetGump(Mobile from, DuelContext context, Ruleset ruleset) : base(50, 50)
		{
			m_From = from;
			m_Context = context;
			m_Ruleset = ruleset;
			m_Defaults = ruleset.Layout.Defaults;
			m_Flavors = ruleset.Layout.Flavors;

			var height = 25 + 20 + ((m_Defaults.Length + 1) * 22) + 6 + 20 + (m_Flavors.Length * 22) + 25;

			AddPage(0);

			AddBackground(0, 0, 260, height, 9250);
			AddBackground(10, 10, 240, height - 20, 0xDAC);

			AddHtml(35, 25, 190, 20, Center("Rules"), false, false);

			var y = 25 + 20;

			for (var i = 0; i < m_Defaults.Length; ++i)
			{
				var cur = m_Defaults[i];

				AddHtml(35 + 14, y, 176, 20, cur.Title, false, false);

				if (ruleset.Base == cur && !ruleset.Changed)
				{
					AddImage(35, y + 4, 0x939);
				}
				else if (ruleset.Base == cur)
				{
					AddButton(35, y + 4, 0x93A, 0x939, 2 + i, GumpButtonType.Reply, 0);
				}
				else
				{
					AddButton(35, y + 4, 0x938, 0x939, 2 + i, GumpButtonType.Reply, 0);
				}

				y += 22;
			}

			AddHtml(35 + 14, y, 176, 20, "Custom", false, false);
			AddButton(35, y + 4, ruleset.Changed ? 0x939 : 0x938, 0x939, 1, GumpButtonType.Reply, 0);

			y += 22;
			y += 6;

			AddHtml(35, y, 190, 20, Center("Flavors"), false, false);
			y += 20;

			for (var i = 0; i < m_Flavors.Length; ++i)
			{
				var cur = m_Flavors[i];

				AddHtml(35 + 14, y, 176, 20, cur.Title, false, false);

				if (ruleset.Flavors.Contains(cur))
				{
					AddButton(35, y + 4, 0x939, 0x938, 2 + m_Defaults.Length + i, GumpButtonType.Reply, 0);
				}
				else
				{
					AddButton(35, y + 4, 0x938, 0x939, 2 + m_Defaults.Length + i, GumpButtonType.Reply, 0);
				}

				y += 22;
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_Context != null && !m_Context.Registered)
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 0: // closed
					{
						if (m_Context != null)
						{
							_ = m_From.SendGump(new DuelContextGump(m_From, m_Context));
						}

						break;
					}
				case 1: // customize
					{
						_ = m_From.SendGump(new RulesetGump(m_From, m_Ruleset, m_Ruleset.Layout, m_Context));
						break;
					}
				default:
					{
						var idx = info.ButtonID - 2;

						if (idx >= 0 && idx < m_Defaults.Length)
						{
							m_Ruleset.ApplyDefault(m_Defaults[idx]);
							_ = m_From.SendGump(new PickRulesetGump(m_From, m_Context, m_Ruleset));
						}
						else
						{
							idx -= m_Defaults.Length;

							if (idx >= 0 && idx < m_Flavors.Length)
							{
								if (m_Ruleset.Flavors.Contains(m_Flavors[idx]))
								{
									m_Ruleset.RemoveFlavor(m_Flavors[idx]);
								}
								else
								{
									m_Ruleset.AddFlavor(m_Flavors[idx]);
								}

								_ = m_From.SendGump(new PickRulesetGump(m_From, m_Context, m_Ruleset));
							}
						}

						break;
					}
			}
		}
	}

	public class Ruleset
	{
		private string m_Title;
		private bool m_Changed;

		public RulesetLayout Layout { get; }
		public BitArray Options { get; private set; }
		public string Title { get => m_Title; set => m_Title = value; }

		public Ruleset Base { get; private set; }
		public ArrayList Flavors { get; } = new ArrayList();
		public bool Changed { get => m_Changed; set => m_Changed = value; }

		public void ApplyDefault(Ruleset newDefault)
		{
			Base = newDefault;
			m_Changed = false;

			Options = new BitArray(newDefault.Options);

			ApplyFlavorsTo(this);
		}

		public void ApplyFlavorsTo(Ruleset ruleset)
		{
			for (var i = 0; i < Flavors.Count; ++i)
			{
				var flavor = (Ruleset)Flavors[i];

				_ = Options.Or(flavor.Options);
			}
		}

		public void AddFlavor(Ruleset flavor)
		{
			if (Flavors.Contains(flavor))
			{
				return;
			}

			_ = Flavors.Add(flavor);
			_ = Options.Or(flavor.Options);
		}

		public void RemoveFlavor(Ruleset flavor)
		{
			if (!Flavors.Contains(flavor))
			{
				return;
			}

			Flavors.Remove(flavor);
			_ = Options.And(flavor.Options.Not());
			_ = flavor.Options.Not();
		}

		public void SetOptionRange(string title, bool value)
		{
			var layout = Layout.FindByTitle(title);

			if (layout == null)
			{
				return;
			}

			for (var i = 0; i < layout.TotalLength; ++i)
			{
				Options[i + layout.Offset] = value;
			}

			m_Changed = true;
		}

		public bool GetOption(string title, string option)
		{
			var index = 0;
			var layout = Layout.FindByOption(title, option, ref index);

			if (layout == null)
			{
				return true;
			}

			return Options[layout.Offset + index];
		}

		public void SetOption(string title, string option, bool value)
		{
			var index = 0;
			var layout = Layout.FindByOption(title, option, ref index);

			if (layout == null)
			{
				return;
			}

			Options[layout.Offset + index] = value;

			m_Changed = true;
		}

		public Ruleset(RulesetLayout layout)
		{
			Layout = layout;
			Options = new BitArray(layout.TotalLength);
		}
	}

	public class RulesetGump : Gump
	{
		private readonly Mobile m_From;
		private readonly Ruleset m_Ruleset;
		private readonly RulesetLayout m_Page;
		private readonly DuelContext m_DuelContext;
		private readonly bool m_ReadOnly;

		public string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public void AddGoldenButton(int x, int y, int bid)
		{
			AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
			AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
		}

		public RulesetGump(Mobile from, Ruleset ruleset, RulesetLayout page, DuelContext duelContext) : this(from, ruleset, page, duelContext, false)
		{
		}

		public RulesetGump(Mobile from, Ruleset ruleset, RulesetLayout page, DuelContext duelContext, bool readOnly) : base(readOnly ? 310 : 50, 50)
		{
			m_From = from;
			m_Ruleset = ruleset;
			m_Page = page;
			m_DuelContext = duelContext;
			m_ReadOnly = readOnly;

			Dragable = !readOnly;

			_ = from.CloseGump(typeof(RulesetGump));
			_ = from.CloseGump(typeof(DuelContextGump));
			_ = from.CloseGump(typeof(ParticipantGump));

			var depthCounter = page;
			var depth = 0;

			while (depthCounter != null)
			{
				++depth;
				depthCounter = depthCounter.Parent;
			}

			var count = page.Children.Length + page.Options.Length;

			AddPage(0);

			var height = 35 + 10 + 2 + (count * 22) + 2 + 30;

			AddBackground(0, 0, 260, height, 9250);
			AddBackground(10, 10, 240, height - 20, 0xDAC);

			AddHtml(35, 25, 190, 20, Center(page.Title), false, false);

			var x = 35;
			var y = 47;

			for (var i = 0; i < page.Children.Length; ++i)
			{
				AddGoldenButton(x, y, 1 + i);
				AddHtml(x + 25, y, 250, 22, page.Children[i].Title, false, false);

				y += 22;
			}

			for (var i = 0; i < page.Options.Length; ++i)
			{
				var enabled = ruleset.Options[page.Offset + i];

				if (readOnly)
				{
					AddImage(x, y, enabled ? 0xD3 : 0xD2);
				}
				else
				{
					AddCheck(x, y, 0xD2, 0xD3, enabled, i);
				}

				AddHtml(x + 25, y, 250, 22, page.Options[i], false, false);

				y += 22;
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_DuelContext != null && !m_DuelContext.Registered)
			{
				return;
			}

			if (!m_ReadOnly)
			{
				var opts = new BitArray(m_Page.Options.Length);

				for (var i = 0; i < info.Switches.Length; ++i)
				{
					var sid = info.Switches[i];

					if (sid >= 0 && sid < m_Page.Options.Length)
					{
						opts[sid] = true;
					}
				}

				for (var i = 0; i < opts.Length; ++i)
				{
					if (m_Ruleset.Options[m_Page.Offset + i] != opts[i])
					{
						m_Ruleset.Options[m_Page.Offset + i] = opts[i];
						m_Ruleset.Changed = true;
					}
				}
			}

			var bid = info.ButtonID;

			if (bid == 0)
			{
				if (m_Page.Parent != null)
				{
					_ = m_From.SendGump(new RulesetGump(m_From, m_Ruleset, m_Page.Parent, m_DuelContext, m_ReadOnly));
				}
				else if (!m_ReadOnly)
				{
					_ = m_From.SendGump(new PickRulesetGump(m_From, m_DuelContext, m_Ruleset));
				}
			}
			else
			{
				bid -= 1;

				if (bid >= 0 && bid < m_Page.Children.Length)
				{
					_ = m_From.SendGump(new RulesetGump(m_From, m_Ruleset, m_Page.Children[bid], m_DuelContext, m_ReadOnly));
				}
			}
		}
	}

	public class RulesetLayout
	{
		private static RulesetLayout m_Root;

		public static RulesetLayout Root
		{
			get
			{
				if (m_Root == null)
				{
					var entries = new ArrayList {
						new RulesetLayout("Spells", new RulesetLayout[]
						{
							new RulesetLayout( "1st Circle", "Spells", new string[]
							{
								"Reactive Armor", "Clumsy",
								"Create Food", "Feeblemind",
								"Heal", "Magic Arrow",
								"Night Sight", "Weaken"
							} ),
							new RulesetLayout( "2nd Circle", "Spells", new string[]
							{
								"Agility", "Cunning",
								"Cure", "Harm",
								"Magic Trap", "Untrap",
								"Protection", "Strength"
							} ),
							new RulesetLayout( "3rd Circle", "Spells", new string[]
							{
								"Bless", "Fireball",
								"Magic Lock", "Poison",
								"Telekinesis", "Teleport",
								"Unlock Spell", "Wall of Stone"
							} ),
							new RulesetLayout( "4th Circle", "Spells", new string[]
							{
								"Arch Cure", "Arch Protection",
								"Curse", "Fire Field",
								"Greater Heal", "Lightning",
								"Mana Drain", "Recall"
							} ),
							new RulesetLayout( "5th Circle", "Spells", new string[]
							{
								"Blade Spirits", "Dispel Field",
								"Incognito", "Magic Reflection",
								"Mind Blast", "Paralyze",
								"Poison Field", "Summon Creature"
							} ),
							new RulesetLayout( "6th Circle", "Spells", new string[]
							{
								"Dispel", "Energy Bolt",
								"Explosion", "Invisibility",
								"Mark", "Mass Curse",
								"Paralyze Field", "Reveal"
							} ),
							new RulesetLayout( "7th Circle", "Spells", new string[]
							{
								"Chain Lightning", "Energy Field",
								"Flame Strike", "Gate Travel",
								"Mana Vampire", "Mass Dispel",
								"Meteor Swarm", "Polymorph"
							} ),
							new RulesetLayout( "8th Circle", "Spells", new string[]
							{
								"Earthquake", "Energy Vortex",
								"Resurrection", "Air Elemental",
								"Summon Daemon", "Earth Elemental",
								"Fire Elemental", "Water Elemental"
							} )
						})
					};

					if (Core.AOS)
					{
						_ = entries.Add(new RulesetLayout("Chivalry", new string[]
							{
								"Cleanse by Fire",
								"Close Wounds",
								"Consecrate Weapon",
								"Dispel Evil",
								"Divine Fury",
								"Enemy of One",
								"Holy Light",
								"Noble Sacrifice",
								"Remove Curse",
								"Sacred Journey"
							}));

						_ = entries.Add(new RulesetLayout("Necromancy", new string[]
							{
								"Animate Dead",
								"Blood Oath",
								"Corpse Skin",
								"Curse Weapon",
								"Evil Omen",
								"Horrific Beast",
								"Lich Form",
								"Mind Rot",
								"Pain Spike",
								"Poison Strike",
								"Strangle",
								"Summon Familiar",
								"Vampiric Embrace",
								"Vengeful Spirit",
								"Wither",
								"Wraith Form"
							}));

						if (Core.SE)
						{
							_ = entries.Add(new RulesetLayout("Bushido", new string[]
							{
								"Confidence",
								"Counter Attack",
								"Evasion",
								"Honorable Execution",
								"Lightning Strike",
								"Momentum Strike"
							}));

							_ = entries.Add(new RulesetLayout("Ninjitsu", new string[]
							{
								"Animal Form",
								"Backstab",
								"Death Strike",
								"Focus Attack",
								"Ki Attack",
								"Mirror Image",
								"Shadow Jump",
								"Suprise Attack"
							}));

							if (Core.ML)
							{
								_ = entries.Add(new RulesetLayout("Spellweaving", new string[]
									{
										"Arcane Circle",
										"Arcane Empowerment",
										"Attune Weapon",
										"Dryad Allure",
										"Essence of Wind",
										"Ethereal Voyage",
										"Gift of Life",
										"Gift of Renewal",
										"Immolating Weapon",
										"Nature's Fury",
										"Reaper Form",
										"Summon Fey",
										"Summon Fiend",
										"Thunderstorm",
										"Wildfire",
										"Word of Death"
									}));
							}
						}
					}

					if (Core.AOS)
					{
						if (Core.SE)
						{
							_ = entries.Add(new RulesetLayout("Combat Abilities", new string[]
							{
								"Stun",
								"Disarm",
								"Armor Ignore",
								"Bleed Attack",
								"Concussion Blow",
								"Crushing Blow",
								"Disarm",
								"Dismount",
								"Double Strike",
								"Infectious Strike",
								"Mortal Strike",
								"Moving Shot",
								"Paralyzing Blow",
								"Shadow Strike",
								"Whirlwind Attack",
								"Riding Swipe",
								"Frenzied Whirlwind",
								"Block",
								"Defense Mastery",
								"Nerve Strike",
								"Talon Strike",
								"Feint",
								"Dual Wield",
								"Double Shot",
								"Armor Pierce"
							}));

							//TODO: ADD ML ABILITIES
						}
						else
						{
							_ = entries.Add(new RulesetLayout("Combat Abilities", new string[]
							{
								"Stun",
								"Disarm",
								"Armor Ignore",
								"Bleed Attack",
								"Concussion Blow",
								"Crushing Blow",
								"Disarm",
								"Dismount",
								"Double Strike",
								"Infectious Strike",
								"Mortal Strike",
								"Moving Shot",
								"Paralyzing Blow",
								"Shadow Strike",
								"Whirlwind Attack"
							}));
						}
					}
					else
					{
						_ = entries.Add(new RulesetLayout("Combat Abilities", new string[]
							{
								"Stun",
								"Disarm",
								"Concussion Blow",
								"Crushing Blow",
								"Paralyzing Blow"
							}));
					}

					_ = entries.Add(new RulesetLayout("Skills", new string[]
						{
							"Anatomy",
							"Detect Hidden",
							"Evaluating Intelligence",
							"Hiding",
							"Poisoning",
							"Snooping",
							"Stealing",
							"Spirit Speak",
							"Stealth"
						}));

					if (Core.AOS)
					{
						_ = entries.Add(new RulesetLayout("Weapons", new string[]
						{
							"Magical",
							"Melee",
							"Ranged",
							"Poisoned",
							"Wrestling"
						}));

						_ = entries.Add(new RulesetLayout("Armor", new string[]
							{
								"Magical",
								"Shields"
							}));
					}
					else
					{
						_ = entries.Add(new RulesetLayout("Weapons", new string[]
						{
							"Magical",
							"Melee",
							"Ranged",
							"Poisoned",
							"Wrestling",
							"Runics"
						}));

						_ = entries.Add(new RulesetLayout("Armor", new string[]
							{
								"Magical",
								"Shields",
								"Colored"
							}));
					}

					if (Core.SE)
					{
						_ = entries.Add(new RulesetLayout("Items", new RulesetLayout[]
						{
							new RulesetLayout( "Potions", new string[]
							{
								"Agility",
								"Cure",
								"Explosion",
								"Heal",
								"Nightsight",
								"Poison",
								"Refresh",
								"Strength"
							} )
						},
						new string[]
						{
							"Bandages",
							"Wands",
							"Trapped Containers",
							"Bolas",
							"Mounts",
							"Orange Petals",
							"Shurikens",
							"Fukiya Darts",
							"Fire Horns"
						}));
					}
					else
					{
						_ = entries.Add(new RulesetLayout("Items", new RulesetLayout[]
						{
							new RulesetLayout( "Potions", new string[]
							{
								"Agility",
								"Cure",
								"Explosion",
								"Heal",
								"Nightsight",
								"Poison",
								"Refresh",
								"Strength"
							} )
						},
							new string[]
						{
							"Bandages",
							"Wands",
							"Trapped Containers",
							"Bolas",
							"Mounts",
							"Orange Petals",
							"Fire Horns"
						}));
					}

					m_Root = new RulesetLayout("Rules", (RulesetLayout[])entries.ToArray(typeof(RulesetLayout)));
					m_Root.ComputeOffsets();

					// Set up default rulesets

					if (!Core.AOS)
					{
						#region Mage 5x
						var m5x = new Ruleset(m_Root)
						{
							Title = "Mage 5x"
						};

						m5x.SetOptionRange("Spells", true);

						m5x.SetOption("Spells", "Wall of Stone", false);
						m5x.SetOption("Spells", "Fire Field", false);
						m5x.SetOption("Spells", "Poison Field", false);
						m5x.SetOption("Spells", "Energy Field", false);
						m5x.SetOption("Spells", "Reactive Armor", false);
						m5x.SetOption("Spells", "Protection", false);
						m5x.SetOption("Spells", "Teleport", false);
						m5x.SetOption("Spells", "Wall of Stone", false);
						m5x.SetOption("Spells", "Arch Protection", false);
						m5x.SetOption("Spells", "Recall", false);
						m5x.SetOption("Spells", "Blade Spirits", false);
						m5x.SetOption("Spells", "Incognito", false);
						m5x.SetOption("Spells", "Magic Reflection", false);
						m5x.SetOption("Spells", "Paralyze", false);
						m5x.SetOption("Spells", "Summon Creature", false);
						m5x.SetOption("Spells", "Invisibility", false);
						m5x.SetOption("Spells", "Mark", false);
						m5x.SetOption("Spells", "Paralyze Field", false);
						m5x.SetOption("Spells", "Energy Field", false);
						m5x.SetOption("Spells", "Gate Travel", false);
						m5x.SetOption("Spells", "Polymorph", false);
						m5x.SetOption("Spells", "Energy Vortex", false);
						m5x.SetOption("Spells", "Air Elemental", false);
						m5x.SetOption("Spells", "Summon Daemon", false);
						m5x.SetOption("Spells", "Earth Elemental", false);
						m5x.SetOption("Spells", "Fire Elemental", false);
						m5x.SetOption("Spells", "Water Elemental", false);
						m5x.SetOption("Spells", "Earthquake", false);
						m5x.SetOption("Spells", "Meteor Swarm", false);
						m5x.SetOption("Spells", "Chain Lightning", false);
						m5x.SetOption("Spells", "Resurrection", false);

						m5x.SetOption("Weapons", "Wrestling", true);

						m5x.SetOption("Skills", "Anatomy", true);
						m5x.SetOption("Skills", "Detect Hidden", true);
						m5x.SetOption("Skills", "Evaluating Intelligence", true);

						m5x.SetOption("Items", "Trapped Containers", true);
						#endregion

						#region Mage 7x
						var m7x = new Ruleset(m_Root)
						{
							Title = "Mage 7x"
						};

						m7x.SetOptionRange("Spells", true);

						m7x.SetOption("Spells", "Wall of Stone", false);
						m7x.SetOption("Spells", "Fire Field", false);
						m7x.SetOption("Spells", "Poison Field", false);
						m7x.SetOption("Spells", "Energy Field", false);
						m7x.SetOption("Spells", "Reactive Armor", false);
						m7x.SetOption("Spells", "Protection", false);
						m7x.SetOption("Spells", "Teleport", false);
						m7x.SetOption("Spells", "Wall of Stone", false);
						m7x.SetOption("Spells", "Arch Protection", false);
						m7x.SetOption("Spells", "Recall", false);
						m7x.SetOption("Spells", "Blade Spirits", false);
						m7x.SetOption("Spells", "Incognito", false);
						m7x.SetOption("Spells", "Magic Reflection", false);
						m7x.SetOption("Spells", "Paralyze", false);
						m7x.SetOption("Spells", "Summon Creature", false);
						m7x.SetOption("Spells", "Invisibility", false);
						m7x.SetOption("Spells", "Mark", false);
						m7x.SetOption("Spells", "Paralyze Field", false);
						m7x.SetOption("Spells", "Energy Field", false);
						m7x.SetOption("Spells", "Gate Travel", false);
						m7x.SetOption("Spells", "Polymorph", false);
						m7x.SetOption("Spells", "Energy Vortex", false);
						m7x.SetOption("Spells", "Air Elemental", false);
						m7x.SetOption("Spells", "Summon Daemon", false);
						m7x.SetOption("Spells", "Earth Elemental", false);
						m7x.SetOption("Spells", "Fire Elemental", false);
						m7x.SetOption("Spells", "Water Elemental", false);
						m7x.SetOption("Spells", "Earthquake", false);
						m7x.SetOption("Spells", "Meteor Swarm", false);
						m7x.SetOption("Spells", "Chain Lightning", false);
						m7x.SetOption("Spells", "Resurrection", false);

						m7x.SetOption("Combat Abilities", "Stun", true);

						m7x.SetOption("Skills", "Anatomy", true);
						m7x.SetOption("Skills", "Detect Hidden", true);
						m7x.SetOption("Skills", "Poisoning", true);
						m7x.SetOption("Skills", "Evaluating Intelligence", true);

						m7x.SetOption("Weapons", "Wrestling", true);

						m7x.SetOption("Potions", "Refresh", true);
						m7x.SetOption("Items", "Trapped Containers", true);
						m7x.SetOption("Items", "Bandages", true);
						#endregion

						#region Standard 7x
						var s7x = new Ruleset(m_Root)
						{
							Title = "Standard 7x"
						};

						s7x.SetOptionRange("Spells", true);

						s7x.SetOption("Spells", "Wall of Stone", false);
						s7x.SetOption("Spells", "Fire Field", false);
						s7x.SetOption("Spells", "Poison Field", false);
						s7x.SetOption("Spells", "Energy Field", false);
						s7x.SetOption("Spells", "Teleport", false);
						s7x.SetOption("Spells", "Wall of Stone", false);
						s7x.SetOption("Spells", "Arch Protection", false);
						s7x.SetOption("Spells", "Recall", false);
						s7x.SetOption("Spells", "Blade Spirits", false);
						s7x.SetOption("Spells", "Incognito", false);
						s7x.SetOption("Spells", "Magic Reflection", false);
						s7x.SetOption("Spells", "Paralyze", false);
						s7x.SetOption("Spells", "Summon Creature", false);
						s7x.SetOption("Spells", "Invisibility", false);
						s7x.SetOption("Spells", "Mark", false);
						s7x.SetOption("Spells", "Paralyze Field", false);
						s7x.SetOption("Spells", "Energy Field", false);
						s7x.SetOption("Spells", "Gate Travel", false);
						s7x.SetOption("Spells", "Polymorph", false);
						s7x.SetOption("Spells", "Energy Vortex", false);
						s7x.SetOption("Spells", "Air Elemental", false);
						s7x.SetOption("Spells", "Summon Daemon", false);
						s7x.SetOption("Spells", "Earth Elemental", false);
						s7x.SetOption("Spells", "Fire Elemental", false);
						s7x.SetOption("Spells", "Water Elemental", false);
						s7x.SetOption("Spells", "Earthquake", false);
						s7x.SetOption("Spells", "Meteor Swarm", false);
						s7x.SetOption("Spells", "Chain Lightning", false);
						s7x.SetOption("Spells", "Resurrection", false);

						s7x.SetOptionRange("Combat Abilities", true);

						s7x.SetOption("Skills", "Anatomy", true);
						s7x.SetOption("Skills", "Detect Hidden", true);
						s7x.SetOption("Skills", "Poisoning", true);
						s7x.SetOption("Skills", "Evaluating Intelligence", true);

						s7x.SetOptionRange("Weapons", true);
						s7x.SetOption("Weapons", "Runics", false);
						s7x.SetOptionRange("Armor", true);

						s7x.SetOption("Potions", "Refresh", true);
						s7x.SetOption("Items", "Bandages", true);
						s7x.SetOption("Items", "Trapped Containers", true);
						#endregion

						m_Root.Defaults = new Ruleset[] { m5x, m7x, s7x };
					}
					else
					{
						#region Standard All Skills

						var all = new Ruleset(m_Root)
						{
							Title = "Standard All Skills"
						};

						all.SetOptionRange("Spells", true);

						all.SetOption("Spells", "Wall of Stone", false);
						all.SetOption("Spells", "Fire Field", false);
						all.SetOption("Spells", "Poison Field", false);
						all.SetOption("Spells", "Energy Field", false);
						all.SetOption("Spells", "Teleport", false);
						all.SetOption("Spells", "Wall of Stone", false);
						all.SetOption("Spells", "Arch Protection", false);
						all.SetOption("Spells", "Recall", false);
						all.SetOption("Spells", "Blade Spirits", false);
						all.SetOption("Spells", "Incognito", false);
						all.SetOption("Spells", "Magic Reflection", false);
						all.SetOption("Spells", "Paralyze", false);
						all.SetOption("Spells", "Summon Creature", false);
						all.SetOption("Spells", "Invisibility", false);
						all.SetOption("Spells", "Mark", false);
						all.SetOption("Spells", "Paralyze Field", false);
						all.SetOption("Spells", "Energy Field", false);
						all.SetOption("Spells", "Gate Travel", false);
						all.SetOption("Spells", "Polymorph", false);
						all.SetOption("Spells", "Energy Vortex", false);
						all.SetOption("Spells", "Air Elemental", false);
						all.SetOption("Spells", "Summon Daemon", false);
						all.SetOption("Spells", "Earth Elemental", false);
						all.SetOption("Spells", "Fire Elemental", false);
						all.SetOption("Spells", "Water Elemental", false);
						all.SetOption("Spells", "Earthquake", false);
						all.SetOption("Spells", "Meteor Swarm", false);
						all.SetOption("Spells", "Chain Lightning", false);
						all.SetOption("Spells", "Resurrection", false);

						all.SetOptionRange("Necromancy", true);
						all.SetOption("Necromancy", "Summon Familiar", false);
						all.SetOption("Necromancy", "Vengeful Spirit", false);
						all.SetOption("Necromancy", "Animate Dead", false);
						all.SetOption("Necromancy", "Wither", false);
						all.SetOption("Necromancy", "Poison Strike", false);

						all.SetOptionRange("Chivalry", true);
						all.SetOption("Chivalry", "Sacred Journey", false);
						all.SetOption("Chivalry", "Enemy of One", false);
						all.SetOption("Chivalry", "Noble Sacrifice", false);

						all.SetOptionRange("Combat Abilities", true);
						all.SetOption("Combat Abilities", "Paralyzing Blow", false);
						all.SetOption("Combat Abilities", "Shadow Strike", false);

						all.SetOption("Skills", "Anatomy", true);
						all.SetOption("Skills", "Detect Hidden", true);
						all.SetOption("Skills", "Poisoning", true);
						all.SetOption("Skills", "Spirit Speak", true);
						all.SetOption("Skills", "Evaluating Intelligence", true);

						all.SetOptionRange("Weapons", true);
						all.SetOption("Weapons", "Poisoned", false);

						all.SetOptionRange("Armor", true);

						all.SetOptionRange("Ninjitsu", true);
						all.SetOption("Ninjitsu", "Animal Form", false);
						all.SetOption("Ninjitsu", "Mirror Image", false);
						all.SetOption("Ninjitsu", "Backstab", false);
						all.SetOption("Ninjitsu", "Suprise Attack", false);
						all.SetOption("Ninjitsu", "Shadow Jump", false);

						all.SetOptionRange("Bushido", true);

						all.SetOptionRange("Spellweaving", true);
						all.SetOption("Spellweaving", "Gift of Life", false);
						all.SetOption("Spellweaving", "Summon Fey", false);
						all.SetOption("Spellweaving", "Summon Fiend", false);
						all.SetOption("Spellweaving", "Nature's Fury", false);

						all.SetOption("Potions", "Refresh", true);
						all.SetOption("Items", "Bandages", true);
						all.SetOption("Items", "Trapped Containers", true);

						m_Root.Defaults = new Ruleset[] { all };
						#endregion
					}

					// Set up flavors

					var pots = new Ruleset(m_Root)
					{
						Title = "Potions"
					};

					pots.SetOptionRange("Potions", true);
					pots.SetOption("Potions", "Explosion", false);

					var para = new Ruleset(m_Root)
					{
						Title = "Paralyze"
					};
					para.SetOption("Spells", "Paralyze", true);
					para.SetOption("Spells", "Paralyze Field", true);
					para.SetOption("Combat Abilities", "Paralyzing Blow", true);

					var fields = new Ruleset(m_Root)
					{
						Title = "Fields"
					};
					fields.SetOption("Spells", "Wall of Stone", true);
					fields.SetOption("Spells", "Fire Field", true);
					fields.SetOption("Spells", "Poison Field", true);
					fields.SetOption("Spells", "Energy Field", true);
					fields.SetOption("Spells", "Wildfire", true);

					var area = new Ruleset(m_Root)
					{
						Title = "Area Effect"
					};
					area.SetOption("Spells", "Earthquake", true);
					area.SetOption("Spells", "Meteor Swarm", true);
					area.SetOption("Spells", "Chain Lightning", true);
					area.SetOption("Necromancy", "Wither", true);
					area.SetOption("Necromancy", "Poison Strike", true);

					var summons = new Ruleset(m_Root)
					{
						Title = "Summons"
					};
					summons.SetOption("Spells", "Blade Spirits", true);
					summons.SetOption("Spells", "Energy Vortex", true);
					summons.SetOption("Spells", "Air Elemental", true);
					summons.SetOption("Spells", "Summon Daemon", true);
					summons.SetOption("Spells", "Earth Elemental", true);
					summons.SetOption("Spells", "Fire Elemental", true);
					summons.SetOption("Spells", "Water Elemental", true);
					summons.SetOption("Necromancy", "Summon Familiar", true);
					summons.SetOption("Necromancy", "Vengeful Spirit", true);
					summons.SetOption("Necromancy", "Animate Dead", true);
					summons.SetOption("Ninjitsu", "Mirror Image", true);
					summons.SetOption("Spellweaving", "Summon Fey", true);
					summons.SetOption("Spellweaving", "Summon Fiend", true);
					summons.SetOption("Spellweaving", "Nature's Fury", true);

					m_Root.Flavors = new Ruleset[] { pots, para, fields, area, summons };
				}

				return m_Root;
			}
		}

		private readonly string m_Description;
		private int m_TotalLength;
		private RulesetLayout m_Parent;

		public string Title { get; }
		public string Description => m_Description;
		public string[] Options { get; }

		public int Offset { get; private set; }
		public int TotalLength => m_TotalLength;

		public RulesetLayout Parent => m_Parent;
		public RulesetLayout[] Children { get; }

		public Ruleset[] Defaults { get; set; }
		public Ruleset[] Flavors { get; set; }

		public RulesetLayout FindByTitle(string title)
		{
			if (Title == title)
			{
				return this;
			}

			for (var i = 0; i < Children.Length; ++i)
			{
				var layout = Children[i].FindByTitle(title);

				if (layout != null)
				{
					return layout;
				}
			}

			return null;
		}

		public string FindByIndex(int index)
		{
			if (index >= Offset && index < (Offset + Options.Length))
			{
				return m_Description + ": " + Options[index - Offset];
			}

			for (var i = 0; i < Children.Length; ++i)
			{
				var opt = Children[i].FindByIndex(index);

				if (opt != null)
				{
					return opt;
				}
			}

			return null;
		}

		public RulesetLayout FindByOption(string title, string option, ref int index)
		{
			if (title == null || Title == title)
			{
				index = GetOptionIndex(option);

				if (index >= 0)
				{
					return this;
				}

				title = null;
			}

			for (var i = 0; i < Children.Length; ++i)
			{
				var layout = Children[i].FindByOption(title, option, ref index);

				if (layout != null)
				{
					return layout;
				}
			}

			return null;
		}

		public int GetOptionIndex(string option)
		{
			return Array.IndexOf(Options, option);
		}

		public void ComputeOffsets()
		{
			var offset = 0;

			_ = RecurseComputeOffsets(ref offset);
		}

		private int RecurseComputeOffsets(ref int offset)
		{
			Offset = offset;

			offset += Options.Length;
			m_TotalLength += Options.Length;

			for (var i = 0; i < Children.Length; ++i)
			{
				m_TotalLength += Children[i].RecurseComputeOffsets(ref offset);
			}

			return m_TotalLength;
		}

		public RulesetLayout(string title, string[] options) : this(title, title, new RulesetLayout[0], options)
		{
		}

		public RulesetLayout(string title, string description, string[] options) : this(title, description, new RulesetLayout[0], options)
		{
		}

		public RulesetLayout(string title, RulesetLayout[] children) : this(title, title, children, new string[0])
		{
		}

		public RulesetLayout(string title, string description, RulesetLayout[] children) : this(title, description, children, new string[0])
		{
		}

		public RulesetLayout(string title, RulesetLayout[] children, string[] options) : this(title, title, children, options)
		{
		}

		public RulesetLayout(string title, string description, RulesetLayout[] children, string[] options)
		{
			Title = title;
			m_Description = description;
			Children = children;
			Options = options;

			for (var i = 0; i < children.Length; ++i)
			{
				children[i].m_Parent = this;
			}
		}
	}

	/// Duel Context
	public delegate void CountdownCallback(int count);

	public class DuelContext
	{
		private readonly ArrayList m_Participants;
		private bool m_Started;
		private bool m_Rematch;

		public bool Rematch => m_Rematch;

		public bool ReadyWait { get; private set; }
		public int ReadyCount { get; private set; }

		public bool Registered { get; private set; } = true;
		public bool Finished { get; private set; }
		public bool Started => m_Started;

		public Mobile Initiator { get; }
		public ArrayList Participants => m_Participants;
		public Ruleset Ruleset { get; private set; }
		public Arena Arena { get; private set; }

		private bool CantDoAnything(Mobile mob)
		{
			if (m_EventGame != null)
			{
				return m_EventGame.CantDoAnything(mob);
			}
			else
			{
				return false;
			}
		}

		public static bool IsFreeConsume(Mobile mob)
		{
			if (mob is not PlayerMobile pm || pm.DuelContext == null || pm.DuelContext.m_EventGame == null)
			{
				return false;
			}

			return pm.DuelContext.m_EventGame.FreeConsume;
		}

		public void DelayBounce(TimeSpan ts, Mobile mob, Container corpse)
		{
			_ = Timer.DelayCall(ts, DelayBounce_Callback, new object[] { mob, corpse });
		}

		public static bool AllowSpecialMove(Mobile from, string name, ISpecialMove move)
		{
			if (from is not PlayerMobile pm)
			{
				return true;
			}

			var dc = pm.DuelContext;

			return dc == null || dc.InstAllowSpecialMove(from, name, move);
		}

		public bool InstAllowSpecialMove(Mobile from, string name, ISpecialMove move)
		{
			if (!StartedBeginCountdown)
			{
				return true;
			}

			var pl = Find(from);

			if (pl == null || pl.Eliminated)
			{
				return true;
			}

			if (CantDoAnything(from))
			{
				return false;
			}

			string title = null;

			if (move is NinjitsuAbility)
			{
				title = "Bushido";
			}
			else if (move is BushidoAbility)
			{
				title = "Ninjitsu";
			}

			if (title == null || name == null || Ruleset.GetOption(title, name))
			{
				return true;
			}

			from.SendMessage("The dueling ruleset prevents you from using this move.");
			return false;
		}

		public bool AllowSpellCast(Mobile from, Spell spell)
		{
			if (!StartedBeginCountdown)
			{
				return true;
			}

			var pl = Find(from);

			if (pl == null || pl.Eliminated)
			{
				return true;
			}

			if (CantDoAnything(from))
			{
				return false;
			}

			if (spell is Server.Spells.Magery.RecallSpell)
			{
				from.SendMessage("You may not cast this spell.");
			}

			string title = null;
			string option;
			if (spell is SpellweavingSpell)
			{
				title = "Spellweaving";
				option = spell.Name;
			}
			else if (spell is ChivalrySpell)
			{
				title = "Chivalry";
				option = spell.Name;
			}
			else if (spell is NecromancySpell)
			{
				title = "Necromancy";
				option = spell.Name;
			}
			else if (spell is NinjitsuSpell)
			{
				title = "Ninjitsu";
				option = spell.Name;
			}
			else if (spell is BushidoSpell)
			{
				title = "Bushido";
				option = spell.Name;
			}
			else if (spell is MagerySpell)
			{
				switch (((MagerySpell)spell).Circle)
				{
					case SpellCircle.First: title = "1st Circle"; break;
					case SpellCircle.Second: title = "2nd Circle"; break;
					case SpellCircle.Third: title = "3rd Circle"; break;
					case SpellCircle.Fourth: title = "4th Circle"; break;
					case SpellCircle.Fifth: title = "5th Circle"; break;
					case SpellCircle.Sixth: title = "6th Circle"; break;
					case SpellCircle.Seventh: title = "7th Circle"; break;
					case SpellCircle.Eighth: title = "8th Circle"; break;
				}

				option = spell.Name;
			}
			else
			{
				title = "Other Spell";
				option = spell.Name;
			}

			if (title == null || option == null || Ruleset.GetOption(title, option))
			{
				return true;
			}

			from.SendMessage("The dueling ruleset prevents you from casting this spell.");
			return false;
		}

		public bool AllowItemEquip(Mobile from, Item item)
		{
			if (!StartedBeginCountdown)
			{
				return true;
			}

			var pl = Find(from);

			if (pl == null || pl.Eliminated)
			{
				return true;
			}

			if (item is Dagger || CheckItemEquip(from, item))
			{
				return true;
			}

			from.SendMessage("The dueling ruleset prevents you from equiping this item.");
			return false;
		}

		public static bool AllowSpecialAbility(Mobile from, string name, bool message)
		{
			if (from is not PlayerMobile pm)
			{
				return true;
			}

			var dc = pm.DuelContext;

			return dc == null || dc.InstAllowSpecialAbility(from, name, message);
		}

		public bool InstAllowSpecialAbility(Mobile from, string name, bool message)
		{
			if (!StartedBeginCountdown)
			{
				return true;
			}

			var pl = Find(from);

			if (pl == null || pl.Eliminated)
			{
				return true;
			}

			if (CantDoAnything(from))
			{
				return false;
			}

			if (Ruleset.GetOption("Combat Abilities", name))
			{
				return true;
			}

			if (message)
			{
				from.SendMessage("The dueling ruleset prevents you from using this combat ability.");
			}

			return false;
		}

		public bool CheckItemEquip(Mobile from, Item item)
		{
			if (item is Fists)
			{
				if (!Ruleset.GetOption("Weapons", "Wrestling"))
				{
					return false;
				}
			}
			else if (item is BaseArmor armor)
			{
				if (armor.ProtectionLevel > ArmorProtectionLevel.Regular && !Ruleset.GetOption("Armor", "Magical"))
				{
					return false;
				}

				if (!Core.AOS && armor.Resource != armor.DefaultResource && !Ruleset.GetOption("Armor", "Colored"))
				{
					return false;
				}

				if (armor is BaseShield && !Ruleset.GetOption("Armor", "Shields"))
				{
					return false;
				}
			}
			else if (item is BaseWeapon weapon)
			{
				if ((weapon.DamageLevel > WeaponDamageLevel.Regular || weapon.AccuracyLevel > WeaponAccuracyLevel.Regular) && !Ruleset.GetOption("Weapons", "Magical"))
				{
					return false;
				}

				if (!Core.AOS && weapon.Resource != CraftResource.Iron && weapon.Resource != CraftResource.None && !Ruleset.GetOption("Weapons", "Runics"))
				{
					return false;
				}

				if (weapon is BaseRanged && !Ruleset.GetOption("Weapons", "Ranged"))
				{
					return false;
				}

				if (weapon is not BaseRanged && !Ruleset.GetOption("Weapons", "Melee"))
				{
					return false;
				}

				if (weapon.PoisonCharges > 0 && weapon.Poison != null && !Ruleset.GetOption("Weapons", "Poisoned"))
				{
					return false;
				}

				if (weapon is BaseWand && !Ruleset.GetOption("Items", "Wands"))
				{
					return false;
				}
			}

			return true;
		}

		public bool AllowSkillUse(Mobile from, SkillName skill)
		{
			if (!StartedBeginCountdown)
			{
				return true;
			}

			var pl = Find(from);

			if (pl == null || pl.Eliminated)
			{
				return true;
			}

			if (CantDoAnything(from))
			{
				return false;
			}

			var id = (int)skill;

			if (id >= 0 && id < SkillInfo.Table.Length)
			{
				if (Ruleset.GetOption("Skills", SkillInfo.Table[id].Name))
				{
					return true;
				}
			}

			from.SendMessage("The dueling ruleset prevents you from using this skill.");
			return false;
		}

		public bool AllowItemUse(Mobile from, Item item)
		{
			if (!StartedBeginCountdown)
			{
				return true;
			}

			var pl = Find(from);

			if (pl == null || pl.Eliminated)
			{
				return true;
			}

			if (item is not BaseRefreshPotion)
			{
				if (CantDoAnything(from))
				{
					return false;
				}
			}

			string title = null, option = null;

			if (item is BasePotion)
			{
				title = "Potions";

				if (item is BaseAgilityPotion)
				{
					option = "Agility";
				}
				else if (item is BaseCurePotion)
				{
					option = "Cure";
				}
				else if (item is BaseHealPotion)
				{
					option = "Heal";
				}
				else if (item is NightSightPotion)
				{
					option = "Nightsight";
				}
				else if (item is BasePoisonPotion)
				{
					option = "Poison";
				}
				else if (item is BaseStrengthPotion)
				{
					option = "Strength";
				}
				else if (item is BaseExplosionPotion)
				{
					option = "Explosion";
				}
				else if (item is BaseRefreshPotion)
				{
					option = "Refresh";
				}
			}
			else if (item is Bandage)
			{
				title = "Items";
				option = "Bandages";
			}
			else if (item is TrapableContainer)
			{
				if (((TrapableContainer)item).TrapType != TrapType.None)
				{
					title = "Items";
					option = "Trapped Containers";
				}
			}
			else if (item is Bola)
			{
				title = "Items";
				option = "Bolas";
			}
			else if (item is OrangePetals)
			{
				title = "Items";
				option = "Orange Petals";
			}
			else if (item is EtherealMount || item.Layer == Layer.Mount)
			{
				title = "Items";
				option = "Mounts";
			}
			else if (item is LeatherNinjaBelt)
			{
				title = "Items";
				option = "Shurikens";
			}
			else if (item is Fukiya)
			{
				title = "Items";
				option = "Fukiya Darts";
			}
			else if (item is FireHorn)
			{
				title = "Items";
				option = "Fire Horns";
			}
			else if (item is BaseWand)
			{
				title = "Items";
				option = "Wands";
			}

			if (title != null && option != null && StartedBeginCountdown && !m_Started)
			{
				from.SendMessage("You may not use this item before the duel begins.");
				return false;
			}
			else if (item is BasePotion && item is not BaseExplosionPotion && item is not BaseRefreshPotion && IsSuddenDeath)
			{
				from.SendMessage(0x22, "You may not drink potions in sudden death.");
				return false;
			}
			else if (item is Bandage && IsSuddenDeath)
			{
				from.SendMessage(0x22, "You may not use bandages in sudden death.");
				return false;
			}

			if (title == null || option == null || Ruleset.GetOption(title, option))
			{
				return true;
			}

			from.SendMessage("The dueling ruleset prevents you from using this item.");
			return false;
		}

		private void DelayBounce_Callback(object state)
		{
			var states = (object[])state;
			var mob = (Mobile)states[0];
			var corpse = (Container)states[1];

			RemoveAggressions(mob);
			SendOutside(mob);
			Refresh(mob, corpse);
			Debuff(mob);
			CancelSpell(mob);
			mob.Frozen = false;
		}

		public void OnMapChanged(Mobile mob)
		{
			OnLocationChanged(mob);
		}

		public void OnLocationChanged(Mobile mob)
		{
			if (!Registered || !StartedBeginCountdown || Finished)
			{
				return;
			}

			var arena = Arena;

			if (arena == null)
			{
				return;
			}

			if (mob.Map == arena.Facet && arena.Bounds.Contains(mob.Location))
			{
				return;
			}

			var pl = Find(mob);

			if (pl == null || pl.Eliminated)
			{
				return;
			}

			if (mob.Map == Map.Internal)
			{
				// they've logged out

				if (mob.LogoutMap == arena.Facet && arena.Bounds.Contains(mob.LogoutLocation))
				{
					// they logged out inside the arena.. set them to eject on login

					mob.LogoutLocation = arena.Outside;
				}
			}

			pl.Eliminated = true;

			mob.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have forfeited your position in the duel.");
			mob.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, String.Format("{0} has forfeited by leaving the dueling arena.", mob.Name));

			var winner = CheckCompletion();

			if (winner != null)
			{
				Finish(winner);
			}
		}

		private bool m_Yielding;

		public void OnDeath(Mobile mob, Container corpse)
		{
			if (!Registered || !m_Started)
			{
				return;
			}

			var pl = Find(mob);

			if (pl != null && !pl.Eliminated)
			{
				if (m_EventGame != null && !m_EventGame.OnDeath(mob, corpse))
				{
					return;
				}

				pl.Eliminated = true;

				if (mob.Poison != null)
				{
					mob.Poison = null;
				}

				Requip(mob, corpse);
				DelayBounce(TimeSpan.FromSeconds(4.0), mob, corpse);

				var winner = CheckCompletion();

				if (winner != null)
				{
					Finish(winner);
				}
				else if (!m_Yielding)
				{
					mob.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have been defeated.");
					mob.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, String.Format("{0} has been defeated.", mob.Name));
				}
			}
		}

		public bool CheckFull()
		{
			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				if (p.HasOpenSlot)
				{
					return false;
				}
			}

			return true;
		}

		public void Requip(Mobile from, Container cont)
		{
			if (cont is not Corpse corpse)
			{
				return;
			}

			var items = new List<Item>(corpse.Items);

			var gathered = false;
			var didntFit = false;

			var pack = from.Backpack;

			for (var i = 0; !didntFit && i < items.Count; ++i)
			{
				var item = items[i];
				var loc = item.Location;

				if (item.Layer == Layer.Hair || item.Layer == Layer.FacialHair || !item.Movable)
				{
					continue;
				}

				if (pack != null)
				{
					pack.DropItem(item);
					gathered = true;
				}
				else
				{
					didntFit = true;
				}
			}

			corpse.Carved = true;

			if (corpse.ItemID == 0x2006)
			{
				corpse.ProcessDelta();
				corpse.SendRemovePacket();
				corpse.ItemID = Utility.Random(0xECA, 9); // bone graphic
				corpse.Hue = 0;
				corpse.ProcessDelta();

				var killer = from.FindMostRecentDamager(false);

				if (killer != null && killer.Player)
				{
					_ = killer.AddToBackpack(new Head(m_Tournament == null ? HeadType.Duel : HeadType.Tournament, from.Name));
				}
			}

			from.PlaySound(0x3E3);

			if (gathered && !didntFit)
			{
				from.SendLocalizedMessage(1062471); // You quickly gather all of your belongings.
			}
			else if (gathered && didntFit)
			{
				from.SendLocalizedMessage(1062472); // You gather some of your belongings. The rest remain on the corpse.
			}
		}

		public void Refresh(Mobile mob, Container cont)
		{
			if (!mob.Alive)
			{
				mob.Resurrect();


				if (mob.FindItemOnLayer(Layer.OuterTorso) is DeathRobe robe)
				{
					robe.Delete();
				}

				if (cont is Corpse corpse)
				{
					for (var i = 0; i < corpse.EquipItems.Count; ++i)
					{
						var item = corpse.EquipItems[i];

						if (item.Movable && item.Layer != Layer.Hair && item.Layer != Layer.FacialHair && item.IsChildOf(mob.Backpack))
						{
							_ = mob.EquipItem(item);
						}
					}
				}
			}

			mob.Hits = mob.HitsMax;
			mob.Stam = mob.StamMax;
			mob.Mana = mob.ManaMax;

			mob.Poison = null;
		}

		public void SendOutside(Mobile mob)
		{
			if (Arena == null)
			{
				return;
			}

			mob.Combatant = null;
			mob.MoveToWorld(Arena.Outside, Arena.Facet);
		}

		private Point3D m_GatePoint;
		private Map m_GateFacet;

		public void Finish(Participant winner)
		{
			if (Finished)
			{
				return;
			}

			EndAutoTie();
			StopSDTimers();

			Finished = true;

			for (var i = 0; i < winner.Players.Length; ++i)
			{
				var pl = winner.Players[i];

				if (pl != null && !pl.Eliminated)
				{
					DelayBounce(TimeSpan.FromSeconds(8.0), pl.Mobile, null);
				}
			}

			winner.Broadcast(0x59, null, winner.Players.Length == 1 ? "{0} has won the duel." : "{0} and {1} team have won the duel.", winner.Players.Length == 1 ? "You have won the duel." : "Your team has won the duel.");

			if (m_Tournament != null && winner.TournyPart != null)
			{
				m_Match.Winner = winner.TournyPart;
				winner.TournyPart.WonMatch(m_Match);
				m_Tournament.HandleWon(Arena, m_Match, winner.TournyPart);
			}

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var loser = (Participant)m_Participants[i];

				if (loser != winner)
				{
					loser.Broadcast(0x22, null, loser.Players.Length == 1 ? "{0} has lost the duel." : "{0} and {1} team have lost the duel.", loser.Players.Length == 1 ? "You have lost the duel." : "Your team has lost the duel.");

					if (m_Tournament != null && loser.TournyPart != null)
					{
						loser.TournyPart.LostMatch(m_Match);
					}
				}

				for (var j = 0; j < loser.Players.Length; ++j)
				{
					if (loser.Players[j] != null)
					{
						RemoveAggressions(loser.Players[j].Mobile);
						loser.Players[j].Mobile.Delta(MobileDelta.Noto);
						_ = loser.Players[j].Mobile.CloseGump(typeof(BeginGump));

						if (m_Tournament != null)
						{
							loser.Players[j].Mobile.SendEverything();
						}
					}
				}
			}

			if (IsOneVsOne)
			{
				var dp1 = ((Participant)m_Participants[0]).Players[0];
				var dp2 = ((Participant)m_Participants[1]).Players[0];

				if (dp1 != null && dp2 != null)
				{
					Award(dp1.Mobile, dp2.Mobile, dp1.Participant == winner);
					Award(dp2.Mobile, dp1.Mobile, dp2.Participant == winner);
				}
			}

			if (m_EventGame != null)
			{
				m_EventGame.OnStop();
			}

			_ = Timer.DelayCall(TimeSpan.FromSeconds(9.0), UnregisterRematch);
		}

		public void Award(Mobile us, Mobile them, bool won)
		{
			var ladder = Arena == null ? Ladder.Instance : Arena.AcquireLadder();

			if (ladder == null)
			{
				return;
			}

			var ourEntry = ladder.Find(us);
			var theirEntry = ladder.Find(them);

			if (ourEntry == null || theirEntry == null)
			{
				return;
			}

			var xpGain = Ladder.GetExperienceGain(ourEntry, theirEntry, won);

			if (xpGain == 0)
			{
				return;
			}

			if (m_Tournament != null)
			{
				xpGain *= xpGain > 0 ? 5 : 2;
			}

			if (won)
			{
				++ourEntry.Wins;
			}
			else
			{
				++ourEntry.Losses;
			}

			var oldLevel = Ladder.GetLevel(ourEntry.Experience);

			ourEntry.Experience += xpGain;

			if (ourEntry.Experience < 0)
			{
				ourEntry.Experience = 0;
			}

			ladder.UpdateEntry(ourEntry);

			var newLevel = Ladder.GetLevel(ourEntry.Experience);

			if (newLevel > oldLevel)
			{
				us.SendMessage(0x59, "You have achieved level {0}!", newLevel);
			}
			else if (newLevel < oldLevel)
			{
				us.SendMessage(0x22, "You have lost a level. You are now at {0}.", newLevel);
			}
		}

		public void UnregisterRematch()
		{
			Unregister(true);
		}

		public void Unregister()
		{
			Unregister(false);
		}

		public void Unregister(bool queryRematch)
		{
			DestroyWall();

			if (!Registered)
			{
				return;
			}

			Registered = false;

			if (Arena != null)
			{
				Arena.Evict();
			}

			StopSDTimers();

			var types = new Type[] { typeof(BeginGump), typeof(DuelContextGump), typeof(ParticipantGump), typeof(PickRulesetGump), typeof(ReadyGump), typeof(ReadyUpGump), typeof(RulesetGump) };

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl == null)
					{
						continue;
					}

					if (pl.Mobile is PlayerMobile)
					{
						((PlayerMobile)pl.Mobile).DuelPlayer = null;
					}

					for (var k = 0; k < types.Length; ++k)
					{
						_ = pl.Mobile.CloseGump(types[k]);
					}
				}
			}

			if (queryRematch && m_Tournament == null)
			{
				QueryRematch();
			}
		}

		public void QueryRematch()
		{
			var dc = new DuelContext(Initiator, Ruleset.Layout, false)
			{
				Ruleset = Ruleset,
				m_Rematch = true
			};

			dc.m_Participants.Clear();

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var oldPart = (Participant)m_Participants[i];
				var newPart = new Participant(dc, oldPart.Players.Length);

				for (var j = 0; j < oldPart.Players.Length; ++j)
				{
					var oldPlayer = oldPart.Players[j];

					if (oldPlayer != null)
					{
						newPart.Players[j] = new DuelPlayer(oldPlayer.Mobile, newPart);
					}
				}

				_ = dc.m_Participants.Add(newPart);
			}

			dc.CloseAllGumps();
			dc.SendReadyUpGump();
		}

		public DuelPlayer Find(Mobile mob)
		{
			if (mob is PlayerMobile pm)
			{
				if (pm.DuelContext == this)
				{
					return pm.DuelPlayer;
				}

				return null;
			}

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];
				var pl = p.Find(mob);

				if (pl != null)
				{
					return pl;
				}
			}

			return null;
		}

		public bool IsAlly(Mobile m1, Mobile m2)
		{
			var pl1 = Find(m1);
			var pl2 = Find(m2);

			return pl1 != null && pl2 != null && pl1.Participant == pl2.Participant;
		}

		public Participant CheckCompletion()
		{
			Participant winner = null;

			var hasWinner = false;
			var eliminated = 0;

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				if (p.Eliminated)
				{
					++eliminated;

					if (eliminated == (m_Participants.Count - 1))
					{
						hasWinner = true;
					}
				}
				else
				{
					winner = p;
				}
			}

			if (hasWinner)
			{
				return winner ?? (Participant)m_Participants[0];
			}

			return null;
		}

		private Timer m_Countdown;

		public void StartCountdown(int count, CountdownCallback cb)
		{
			cb(count);
			m_Countdown = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), count, Countdown_Callback, new object[] { count - 1, cb });
		}

		public void StopCountdown()
		{
			if (m_Countdown != null)
			{
				m_Countdown.Stop();
			}

			m_Countdown = null;
		}

		private void Countdown_Callback(object state)
		{
			var states = (object[])state;

			var count = (int)states[0];
			var cb = (CountdownCallback)states[1];

			if (count == 0)
			{
				if (m_Countdown != null)
				{
					m_Countdown.Stop();
				}

				m_Countdown = null;
			}

			cb(count);

			states[0] = count - 1;
		}

		private Timer m_AutoTieTimer;

		public bool Tied { get; private set; }

		public bool IsSuddenDeath { get; set; }

		private Timer m_SDWarnTimer, m_SDActivateTimer;

		public void StopSDTimers()
		{
			if (m_SDWarnTimer != null)
			{
				m_SDWarnTimer.Stop();
			}

			m_SDWarnTimer = null;

			if (m_SDActivateTimer != null)
			{
				m_SDActivateTimer.Stop();
			}

			m_SDActivateTimer = null;
		}

		public void StartSuddenDeath(TimeSpan timeUntilActive)
		{
			if (m_SDWarnTimer != null)
			{
				m_SDWarnTimer.Stop();
			}

			m_SDWarnTimer = Timer.DelayCall(TimeSpan.FromMinutes(timeUntilActive.TotalMinutes * 0.9), WarnSuddenDeath);

			if (m_SDActivateTimer != null)
			{
				m_SDActivateTimer.Stop();
			}

			m_SDActivateTimer = Timer.DelayCall(timeUntilActive, ActivateSuddenDeath);
		}

		public void WarnSuddenDeath()
		{
			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl == null || pl.Eliminated)
					{
						continue;
					}

					pl.Mobile.SendSound(0x1E1);
					pl.Mobile.SendMessage(0x22, "Warning! Warning! Warning!");
					pl.Mobile.SendMessage(0x22, "Sudden death will be active soon!");
				}
			}

			if (m_Tournament != null)
			{
				m_Tournament.Alert(Arena, "Sudden death will be active soon!");
			}

			if (m_SDWarnTimer != null)
			{
				m_SDWarnTimer.Stop();
			}

			m_SDWarnTimer = null;
		}

		public static bool CheckSuddenDeath(Mobile mob)
		{
			if (mob is PlayerMobile pm)
			{
				if (pm.DuelPlayer != null && !pm.DuelPlayer.Eliminated && pm.DuelContext != null && pm.DuelContext.IsSuddenDeath)
				{
					return true;
				}
			}

			return false;
		}

		public void ActivateSuddenDeath()
		{
			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl == null || pl.Eliminated)
					{
						continue;
					}

					pl.Mobile.SendSound(0x1E1);
					pl.Mobile.SendMessage(0x22, "Warning! Warning! Warning!");
					pl.Mobile.SendMessage(0x22, "Sudden death has ACTIVATED. You are now unable to perform any beneficial actions.");
				}
			}

			if (m_Tournament != null)
			{
				m_Tournament.Alert(Arena, "Sudden death has been activated!");
			}

			IsSuddenDeath = true;

			if (m_SDActivateTimer != null)
			{
				m_SDActivateTimer.Stop();
			}

			m_SDActivateTimer = null;
		}

		public void BeginAutoTie()
		{
			if (m_AutoTieTimer != null)
			{
				m_AutoTieTimer.Stop();
			}

			var ts = (m_Tournament == null || m_Tournament.TournyType == TournyType.Standard)
				? AutoTieDelay
				: TimeSpan.FromMinutes(90.0);

			m_AutoTieTimer = Timer.DelayCall(ts, InvokeAutoTie);
		}

		public void EndAutoTie()
		{
			if (m_AutoTieTimer != null)
			{
				m_AutoTieTimer.Stop();
			}

			m_AutoTieTimer = null;
		}

		public void InvokeAutoTie()
		{
			m_AutoTieTimer = null;

			if (!m_Started || Finished)
			{
				return;
			}

			Tied = true;
			Finished = true;

			StopSDTimers();

			var remaining = new ArrayList();

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				if (p.Eliminated)
				{
					p.Broadcast(0x22, null, p.Players.Length == 1 ? "{0} has lost the duel." : "{0} and {1} team have lost the duel.", p.Players.Length == 1 ? "You have lost the duel." : "Your team has lost the duel.");
				}
				else
				{
					p.Broadcast(0x59, null, p.Players.Length == 1 ? "{0} has tied the duel due to time expiration." : "{0} and {1} team have tied the duel due to time expiration.", p.Players.Length == 1 ? "You have tied the duel due to time expiration." : "Your team has tied the duel due to time expiration.");

					for (var j = 0; j < p.Players.Length; ++j)
					{
						var pl = p.Players[j];

						if (pl != null && !pl.Eliminated)
						{
							DelayBounce(TimeSpan.FromSeconds(8.0), pl.Mobile, null);
						}
					}

					if (p.TournyPart != null)
					{
						_ = remaining.Add(p.TournyPart);
					}
				}

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl != null)
					{
						pl.Mobile.Delta(MobileDelta.Noto);
						pl.Mobile.SendEverything();
					}
				}
			}

			if (m_Tournament != null)
			{
				m_Tournament.HandleTie(Arena, m_Match, remaining);
			}

			_ = Timer.DelayCall(TimeSpan.FromSeconds(10.0), Unregister);
		}

		public bool IsOneVsOne
		{
			get
			{
				if (m_Participants.Count != 2)
				{
					return false;
				}

				if (((Participant)m_Participants[0]).Players.Length != 1)
				{
					return false;
				}

				if (((Participant)m_Participants[1]).Players.Length != 1)
				{
					return false;
				}

				return true;
			}
		}

		public static void Initialize()
		{
			EventSink.Speech += new SpeechEventHandler(EventSink_Speech);
			EventSink.Login += new LoginEventHandler(EventSink_Login);

			CommandSystem.Register("vli", AccessLevel.GameMaster, new CommandEventHandler(vli_oc));
		}

		private static void vli_oc(CommandEventArgs e)
		{
			_ = e.Mobile.BeginTarget(-1, false, Targeting.TargetFlags.None, new TargetCallback(vli_ot));
		}

		private static void vli_ot(Mobile from, object obj)
		{
			if (obj is PlayerMobile pm)
			{
				var ladder = Ladder.Instance;

				if (ladder == null)
				{
					return;
				}

				var entry = ladder.Find(pm);

				if (entry != null)
				{
					_ = from.SendGump(new PropertiesGump(from, entry));
				}
			}
		}

		private static readonly TimeSpan CombatDelay = TimeSpan.FromSeconds(30.0);
		private static readonly TimeSpan AutoTieDelay = TimeSpan.FromMinutes(15.0);

		public static bool CheckCombat(Mobile m)
		{
			for (var i = 0; i < m.Aggressed.Count; ++i)
			{
				var info = m.Aggressed[i];

				if (info.Defender.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatDelay)
				{
					return true;
				}
			}

			for (var i = 0; i < m.Aggressors.Count; ++i)
			{
				var info = m.Aggressors[i];

				if (info.Attacker.Player && (DateTime.UtcNow - info.LastCombatTime) < CombatDelay)
				{
					return true;
				}
			}

			return false;
		}

		private static void EventSink_Login(LoginEventArgs e)
		{
			if (e.Mobile is not PlayerMobile pm)
			{
				return;
			}

			var dc = pm.DuelContext;

			if (dc == null)
			{
				return;
			}

			if (dc.ReadyWait && pm.DuelPlayer.Ready && !dc.Started && !dc.StartedBeginCountdown && !dc.Finished)
			{
				if (dc.m_Tournament == null)
				{
					_ = pm.SendGump(new ReadyGump(pm, dc, dc.ReadyCount));
				}
			}
			else if (dc.ReadyWait && !dc.StartedBeginCountdown && !dc.Started && !dc.Finished)
			{
				if (dc.m_Tournament == null)
				{
					_ = pm.SendGump(new ReadyUpGump(pm, dc));
				}
			}
			else if (dc.Initiator == pm && !dc.ReadyWait && !dc.StartedBeginCountdown && !dc.Started && !dc.Finished)
			{
				_ = pm.SendGump(new DuelContextGump(pm, dc));
			}
		}

		private static void ViewLadder_OnTarget(Mobile from, object obj, object state)
		{
			if (obj is PlayerMobile pm)
			{
				var ladder = (Ladder)state;

				var entry = ladder.Find(pm);

				if (entry == null)
				{
					return; // sanity
				}

				var text = String.Format("{{0}} are ranked {0} at level {1}.", LadderGump.Rank(entry.Index + 1), Ladder.GetLevel(entry.Experience));

				pm.PrivateOverheadMessage(MessageType.Regular, pm.SpeechHue, true, String.Format(text, from == pm ? "You" : "They"), from.NetState);
			}
			else if (obj is Mobile mob)
			{
				if (mob.Body.IsHuman)
				{
					mob.PrivateOverheadMessage(MessageType.Regular, mob.SpeechHue, false, "I'm not a duelist, and quite frankly, I resent the implication.", from.NetState);
				}
				else
				{
					mob.PrivateOverheadMessage(MessageType.Regular, 0x3B2, true, "It's probably better than you.", from.NetState);
				}
			}
			else
			{
				from.SendMessage("That's not a player.");
			}
		}

		private static void EventSink_Speech(SpeechEventArgs e)
		{
			if (e.Handled)
			{
				return;
			}


			if (e.Mobile is not PlayerMobile pm)
			{
				return;
			}

			if (Insensitive.Contains(e.Speech, "i wish to duel"))
			{
				if (!pm.CheckAlive())
				{
				}
				else if (pm.Region.IsPartOf(typeof(Regions.Jail)))
				{
				}
				else if (CheckCombat(pm))
				{
					e.Mobile.SendMessage(0x22, "You have recently been in combat with another player and must wait before starting a duel.");
				}
				else if (pm.DuelContext != null)
				{
					if (pm.DuelContext.Initiator == pm)
					{
						e.Mobile.SendMessage(0x22, "You have already started a duel.");
					}
					else
					{
						e.Mobile.SendMessage(0x22, "You have already been challenged in a duel.");
					}
				}
				else if (TournamentController.IsActive)
				{
					e.Mobile.SendMessage(0x22, "You may not start a duel while a tournament is active.");
				}
				else
				{
					_ = pm.SendGump(new DuelContextGump(pm, new DuelContext(pm, RulesetLayout.Root)));
					e.Handled = true;
				}
			}
			else if (Insensitive.Equals(e.Speech, "change arena preferences"))
			{
				if (!pm.CheckAlive())
				{
				}
				else
				{
					var prefs = Preferences.Instance;

					if (prefs != null)
					{
						_ = e.Mobile.CloseGump(typeof(PreferencesGump));
						_ = e.Mobile.SendGump(new PreferencesGump(e.Mobile, prefs));
					}
				}
			}
			else if (Insensitive.Equals(e.Speech, "showladder"))
			{
				e.Blocked = true;
				if (!pm.CheckAlive())
				{
				}
				else
				{
					var instance = Ladder.Instance;

					if (instance == null)
					{
						//pm.SendMessage( "Ladder not yet initialized." );
					}
					else
					{
						var entry = instance.Find(pm);

						if (entry == null)
						{
							return; // sanity
						}

						var text = String.Format("{{0}} {{1}} ranked {0} at level {1}.", LadderGump.Rank(entry.Index + 1), Ladder.GetLevel(entry.Experience));

						pm.LocalOverheadMessage(MessageType.Regular, pm.SpeechHue, true, String.Format(text, "You", "are"));
						pm.NonlocalOverheadMessage(MessageType.Regular, pm.SpeechHue, true, String.Format(text, pm.Name, "is"));

						//pm.PublicOverheadMessage( MessageType.Regular, pm.SpeechHue, true, String.Format( "Level {0} with {1} win{2} and {3} loss{4}.", Ladder.GetLevel( entry.Experience ), entry.Wins, entry.Wins==1?"":"s", entry.Losses, entry.Losses==1?"":"es" ) );
						//pm.PublicOverheadMessage( MessageType.Regular, pm.SpeechHue, true, String.Format( "Level {0} with {1} win{2} and {3} loss{4}.", Ladder.GetLevel( entry.Experience ), entry.Wins, entry.Wins==1?"":"s", entry.Losses, entry.Losses==1?"":"es" ) );
					}
				}
			}
			else if (Insensitive.Equals(e.Speech, "viewladder"))
			{
				e.Blocked = true;

				if (!pm.CheckAlive())
				{
				}
				else
				{
					var instance = Ladder.Instance;

					if (instance == null)
					{
						//pm.SendMessage( "Ladder not yet initialized." );
					}
					else
					{
						pm.SendMessage("Target a player to view their ranking and level.");
						_ = pm.BeginTarget(16, false, Targeting.TargetFlags.None, new TargetStateCallback(ViewLadder_OnTarget), instance);
					}
				}
			}
			else if (Insensitive.Contains(e.Speech, "i yield"))
			{
				if (!pm.CheckAlive())
				{
				}
				else if (pm.DuelContext == null)
				{
				}
				else if (pm.DuelContext.Finished)
				{
					e.Mobile.SendMessage(0x22, "The duel is already finished.");
				}
				else if (!pm.DuelContext.Started)
				{
					var dc = pm.DuelContext;
					var init = dc.Initiator;

					if (pm.DuelContext.StartedBeginCountdown)
					{
						e.Mobile.SendMessage(0x22, "The duel has not yet started.");
					}
					else
					{
						var pl = pm.DuelContext.Find(pm);

						if (pl == null)
						{
							return;
						}

						var p = pl.Participant;

						if (!pm.DuelContext.ReadyWait) // still setting stuff up
						{
							p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

							if (init == pm)
							{
								dc.Unregister();
							}
							else
							{
								p.Nullify(pl);
								pm.DuelPlayer = null;

								var ns = init.NetState;

								if (ns != null)
								{
									foreach (var g in ns.Gumps)
									{
										if (g is ParticipantGump pg)
										{
											if (pg.Participant == p)
											{
												_ = init.SendGump(new ParticipantGump(init, dc, p));
												break;
											}
										}
										else if (g is DuelContextGump dcg)
										{
											if (dcg.Context == dc)
											{
												_ = init.SendGump(new DuelContextGump(init, dc));
												break;
											}
										}
									}
								}
							}
						}
						else if (!pm.DuelContext.StartedReadyCountdown) // at ready stage
						{
							p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

							dc.m_Yielding = true;
							dc.RejectReady(pm, null);
							dc.m_Yielding = false;

							if (init == pm)
							{
								dc.Unregister();
							}
							else if (dc.Registered)
							{
								p.Nullify(pl);
								pm.DuelPlayer = null;

								var ns = init.NetState;

								if (ns != null)
								{
									var send = true;

									foreach (var g in ns.Gumps)
									{
										if (g is ParticipantGump pg)
										{
											if (pg.Participant == p)
											{
												_ = init.SendGump(new ParticipantGump(init, dc, p));
												send = false;
												break;
											}
										}
										else if (g is DuelContextGump dcg)
										{
											if (dcg.Context == dc)
											{
												_ = init.SendGump(new DuelContextGump(init, dc));
												send = false;
												break;
											}
										}
									}

									if (send)
									{
										_ = init.SendGump(new DuelContextGump(init, dc));
									}
								}
							}
						}
						else
						{
							if (pm.DuelContext.m_Countdown != null)
							{
								pm.DuelContext.m_Countdown.Stop();
							}

							pm.DuelContext.m_Countdown = null;

							pm.DuelContext.m_StartedReadyCountdown = false;
							p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

							dc.m_Yielding = true;
							dc.RejectReady(pm, null);
							dc.m_Yielding = false;

							if (init == pm)
							{
								dc.Unregister();
							}
							else if (dc.Registered)
							{
								p.Nullify(pl);
								pm.DuelPlayer = null;

								var ns = init.NetState;

								if (ns != null)
								{
									var send = true;

									foreach (var g in ns.Gumps)
									{
										if (g is ParticipantGump pg)
										{
											if (pg.Participant == p)
											{
												_ = init.SendGump(new ParticipantGump(init, dc, p));
												send = false;
												break;
											}
										}
										else if (g is DuelContextGump dcg)
										{
											if (dcg.Context == dc)
											{
												_ = init.SendGump(new DuelContextGump(init, dc));
												send = false;
												break;
											}
										}
									}

									if (send)
									{
										_ = init.SendGump(new DuelContextGump(init, dc));
									}
								}
							}
						}
					}
				}
				else
				{
					var pl = pm.DuelContext.Find(pm);

					if (pl != null)
					{
						if (pm.DuelContext.IsOneVsOne)
						{
							e.Mobile.SendMessage(0x22, "You may not yield a 1 on 1 match.");
						}
						else if (pl.Eliminated)
						{
							e.Mobile.SendMessage(0x22, "You have already been eliminated.");
						}
						else
						{
							pm.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have yielded.");
							pm.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, String.Format("{0} has yielded.", pm.Name));

							pm.DuelContext.m_Yielding = true;
							pm.Kill();
							pm.DuelContext.m_Yielding = false;

							if (pm.Alive) // invul, ...
							{
								pl.Eliminated = true;

								pm.DuelContext.RemoveAggressions(pm);
								pm.DuelContext.SendOutside(pm);
								pm.DuelContext.Refresh(pm, null);
								Debuff(pm);
								CancelSpell(pm);
								pm.Frozen = false;

								var winner = pm.DuelContext.CheckCompletion();

								if (winner != null)
								{
									pm.DuelContext.Finish(winner);
								}
							}
						}
					}
					else
					{
						e.Mobile.SendMessage(0x22, "BUG: Unable to find duel context.");
					}
				}
			}
		}

		public DuelContext(Mobile initiator, RulesetLayout layout) : this(initiator, layout, true)
		{
		}

		public DuelContext(Mobile initiator, RulesetLayout layout, bool addNew)
		{
			Initiator = initiator;
			m_Participants = new ArrayList();
			Ruleset = new Ruleset(layout);
			Ruleset.ApplyDefault(layout.Defaults[0]);

			if (addNew)
			{
				_ = m_Participants.Add(new Participant(this, 1));
				_ = m_Participants.Add(new Participant(this, 1));

				((Participant)m_Participants[0]).Add(initiator);
			}
		}

		public void CloseAllGumps()
		{
			var types = new Type[] { typeof(DuelContextGump), typeof(ParticipantGump), typeof(RulesetGump) };
			var defs = new int[] { -1, -1, -1 };

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl == null)
					{
						continue;
					}

					var mob = pl.Mobile;

					for (var k = 0; k < types.Length; ++k)
					{
						_ = mob.CloseGump(types[k]);
					}
					//mob.CloseGump( types[k], defs[k] );
				}
			}
		}

		public void RejectReady(Mobile rejector, string page)
		{
			if (m_StartedReadyCountdown)
			{
				return; // sanity
			}

			var types = new Type[] { typeof(DuelContextGump), typeof(ReadyUpGump), typeof(ReadyGump) };
			var defs = new int[] { -1, -1, -1 };

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl == null)
					{
						continue;
					}

					pl.Ready = false;

					var mob = pl.Mobile;

					if (page == null) // yield
					{
						if (mob != rejector)
						{
							mob.SendMessage(0x22, "{0} has yielded.", rejector.Name);
						}
					}
					else
					{
						if (mob == rejector)
						{
							mob.SendMessage(0x22, "You have rejected the {0}.", m_Rematch ? "rematch" : page);
						}
						else
						{
							mob.SendMessage(0x22, "{0} has rejected the {1}.", rejector.Name, m_Rematch ? "rematch" : page);
						}
					}

					for (var k = 0; k < types.Length; ++k)
					{
						_ = mob.CloseGump(types[k]);
					}
					//mob.CloseGump( types[k], defs[k] );
				}
			}

			if (m_Rematch)
			{
				Unregister();
			}
			else if (!m_Yielding)
			{
				_ = Initiator.SendGump(new DuelContextGump(Initiator, this));
			}

			ReadyWait = false;
			ReadyCount = 0;
		}

		public void SendReadyGump()
		{
			SendReadyGump(-1);
		}

		public static void Debuff(Mobile mob)
		{
			_ = mob.RemoveStatMod("[Magic] Str Offset");
			_ = mob.RemoveStatMod("[Magic] Dex Offset");
			_ = mob.RemoveStatMod("[Magic] Int Offset");
			_ = mob.RemoveStatMod("Concussion");
			_ = mob.RemoveStatMod("blood-rose");
			_ = mob.RemoveStatMod("clarity-potion");

			OrangePetals.RemoveContext(mob);

			mob.Paralyzed = false;
			mob.Hidden = false;

			if (!Core.AOS)
			{
				mob.MagicDamageAbsorb = 0;
				mob.MeleeDamageAbsorb = 0;

				ProtectionSpell.EndProtection(mob);
				ArchProtectionSpell.RemoveEntry(mob);

				DefensiveState.Nullify(mob, TimeSpan.Zero);
			}

			TransformationSpellHelper.RemoveContext(mob, true);
			AnimalFormSpell.RemoveContext(mob, true);

			if (DisguiseTimers.IsDisguised(mob))
			{
				_ = DisguiseTimers.StopTimer(mob);
			}

			PolymorphSpell.EndPolymorph(mob);

			BaseArmor.ValidateMobile(mob);
			BaseClothing.ValidateMobile(mob);

			mob.Hits = mob.HitsMax;
			mob.Stam = mob.StamMax;
			mob.Mana = mob.ManaMax;

			mob.Poison = null;
		}

		public static void CancelSpell(Mobile mob)
		{
			if (mob.Spell != null)
			{
				mob.Spell.Interrupt(SpellInterrupt.Kill);
			}

			Target.Cancel(mob);
		}

		private bool m_StartedReadyCountdown;

		public bool StartedBeginCountdown { get; private set; }
		public bool StartedReadyCountdown => m_StartedReadyCountdown;

		private class InternalWall : Item
		{
			public InternalWall() : base(0x80)
			{
				Movable = false;
			}

			public void Appear(Point3D loc, Map map)
			{
				MoveToWorld(loc, map);

				Effects.SendLocationParticles(this, 0x376A, 9, 10, 5025);
			}

			public InternalWall(Serial serial) : base(serial)
			{
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				var version = reader.ReadInt();

				Delete();
			}
		}

		private readonly ArrayList m_Walls = new();

		public void DestroyWall()
		{
			for (var i = 0; i < m_Walls.Count; ++i)
			{
				((Item)m_Walls[i]).Delete();
			}

			m_Walls.Clear();
		}

		public void CreateWall()
		{
			if (Arena == null)
			{
				return;
			}

			var start = Arena.Points.EdgeWest;
			var wall = Arena.Wall;

			var dx = start.X - wall.X;
			var dy = start.Y - wall.Y;
			var rx = dx - dy;
			var ry = dx + dy;

			bool eastToWest;

			if (rx >= 0 && ry >= 0)
			{
				eastToWest = false;
			}
			else if (rx >= 0)
			{
				eastToWest = true;
			}
			else if (ry >= 0)
			{
				eastToWest = true;
			}
			else
			{
				eastToWest = false;
			}

			Effects.PlaySound(wall, Arena.Facet, 0x1F6);

			for (var i = -1; i <= 1; ++i)
			{
				var loc = new Point3D(eastToWest ? wall.X + i : wall.X, eastToWest ? wall.Y : wall.Y + i, wall.Z);

				var created = new InternalWall();

				created.Appear(loc, Arena.Facet);

				_ = m_Walls.Add(created);
			}
		}

		public void BuildParties()
		{
			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				if (p.Players.Length > 1)
				{
					var players = new ArrayList();

					for (var j = 0; j < p.Players.Length; ++j)
					{
						var dp = p.Players[j];

						if (dp == null)
						{
							continue;
						}

						_ = players.Add(dp.Mobile);
					}

					if (players.Count > 1)
					{
						for (var leaderIndex = 0; (leaderIndex + 1) < players.Count; leaderIndex += Party.Capacity)
						{
							var leader = (Mobile)players[leaderIndex];
							var party = Party.Get(leader);

							if (party == null)
							{
								leader.Party = party = new Party(leader);
							}
							else if (party.Leader != leader)
							{
								party.SendPublicMessage(leader, "I leave this party to fight in a duel.");
								party.Remove(leader);
								leader.Party = party = new Party(leader);
							}

							for (var j = leaderIndex + 1; j < players.Count && j < leaderIndex + Party.Capacity; ++j)
							{
								var player = (Mobile)players[j];
								var existing = Party.Get(player);

								if (existing == party)
								{
									continue;
								}

								if ((party.Members.Count + party.Candidates.Count) >= Party.Capacity)
								{
									player.SendMessage("You could not be added to the team party because it is at full capacity.");
									leader.SendMessage("{0} could not be added to the team party because it is at full capacity.");
								}
								else
								{
									if (existing != null)
									{
										existing.SendPublicMessage(player, "I leave this party to fight in a duel.");
										existing.Remove(player);
									}

									party.OnAccept(player, true);
								}
							}
						}
					}
				}
			}
		}

		public void ClearIllegalItems()
		{
			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl == null)
					{
						continue;
					}

					ClearIllegalItems(pl.Mobile);
				}
			}
		}

		public void ClearIllegalItems(Mobile mob)
		{
			if (mob.StunReady && !AllowSpecialAbility(mob, "Stun", false))
			{
				mob.StunReady = false;
			}

			if (mob.DisarmReady && !AllowSpecialAbility(mob, "Disarm", false))
			{
				mob.DisarmReady = false;
			}

			var pack = mob.Backpack;

			if (pack == null)
			{
				return;
			}

			for (var i = mob.Items.Count - 1; i >= 0; --i)
			{
				if (i >= mob.Items.Count)
				{
					continue; // sanity
				}

				var item = mob.Items[i];

				if (!CheckItemEquip(mob, item))
				{
					pack.DropItem(item);

					if (item is BaseWeapon)
					{
						mob.SendLocalizedMessage(1062001, item.Name ?? "#" + item.LabelNumber.ToString()); // You can no longer wield your ~1_WEAPON~
					}
					else if (item is BaseArmor and not BaseShield)
					{
						mob.SendLocalizedMessage(1062002, item.Name ?? "#" + item.LabelNumber.ToString()); // You can no longer wear your ~1_ARMOR~
					}
					else
					{
						mob.SendLocalizedMessage(1062003, item.Name ?? "#" + item.LabelNumber.ToString()); // You can no longer equip your ~1_SHIELD~
					}
				}
			}

			var inHand = mob.Holding;

			if (inHand != null && !CheckItemEquip(mob, inHand))
			{
				mob.Holding = null;

				var bi = inHand.GetBounce();

				if (bi.m_Parent == mob)
				{
					pack.DropItem(inHand);
				}
				else
				{
					inHand.Bounce(mob);
				}

				inHand.ClearBounce();
			}
		}

		private void MessageRuleset(Mobile mob)
		{
			if (Ruleset == null)
			{
				return;
			}

			var ruleset = Ruleset;
			var basedef = ruleset.Base;

			mob.SendMessage("Ruleset: {0}", basedef.Title);

			BitArray defs;

			if (ruleset.Flavors.Count > 0)
			{
				defs = new BitArray(basedef.Options);

				for (var i = 0; i < ruleset.Flavors.Count; ++i)
				{
					_ = defs.Or(((Ruleset)ruleset.Flavors[i]).Options);

					mob.SendMessage(" + {0}", ((Ruleset)ruleset.Flavors[i]).Title);
				}
			}
			else
			{
				defs = basedef.Options;
			}

			var changes = 0;

			var opts = ruleset.Options;

			for (var i = 0; i < opts.Length; ++i)
			{
				if (defs[i] != opts[i])
				{
					var name = ruleset.Layout.FindByIndex(i);

					if (name != null) // sanity
					{
						++changes;

						if (changes == 1)
						{
							mob.SendMessage("Modifications:");
						}

						mob.SendMessage("{0}: {1}", name, opts[i] ? "enabled" : "disabled");
					}
				}
			}
		}

		public void SendBeginGump(int count)
		{
			if (!Registered || Finished)
			{
				return;
			}

			if (count == 10)
			{
				CreateWall();
				BuildParties();
				ClearIllegalItems();
			}
			else if (count == 0)
			{
				DestroyWall();
			}

			StartedBeginCountdown = true;

			if (count == 0)
			{
				m_Started = true;
				BeginAutoTie();
			}

			var types = new Type[] { typeof(ReadyGump), typeof(ReadyUpGump), typeof(BeginGump) };

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl == null)
					{
						continue;
					}

					var mob = pl.Mobile;

					if (count > 0)
					{
						if (count == 10)
						{
							CloseAndSendGump(mob, new BeginGump(count), types);
						}

						mob.Frozen = true;
					}
					else
					{
						_ = mob.CloseGump(typeof(BeginGump));
						mob.Frozen = false;
					}
				}
			}
		}

		private readonly ArrayList m_Entered = new();

		private class ReturnEntry
		{
			private Point3D m_Location;
			private DateTime m_Expire;

			public Mobile Mobile { get; }
			public Point3D Location => m_Location;
			public Map Facet { get; private set; }

			public void Return()
			{
				if (Facet == Map.Internal || Facet == null)
				{
					return;
				}

				if (Mobile.Map == Map.Internal)
				{
					Mobile.LogoutLocation = m_Location;
					Mobile.LogoutMap = Facet;
				}
				else
				{
					Mobile.Location = m_Location;
					Mobile.Map = Facet;
				}
			}

			public ReturnEntry(Mobile mob)
			{
				Mobile = mob;

				Update();
			}

			public ReturnEntry(Mobile mob, Point3D loc, Map facet)
			{
				Mobile = mob;
				m_Location = loc;
				Facet = facet;
				m_Expire = DateTime.UtcNow + TimeSpan.FromMinutes(30.0);
			}

			public bool Expired => DateTime.UtcNow >= m_Expire;

			public void Update()
			{
				m_Expire = DateTime.UtcNow + TimeSpan.FromMinutes(30.0);

				if (Mobile.Map == Map.Internal)
				{
					Facet = Mobile.LogoutMap;
					m_Location = Mobile.LogoutLocation;
				}
				else
				{
					Facet = Mobile.Map;
					m_Location = Mobile.Location;
				}
			}
		}

		private class ExitTeleporter : Item
		{
			private ArrayList m_Entries;

			public override string DefaultName => "return teleporter";

			public ExitTeleporter() : base(0x1822)
			{
				m_Entries = new ArrayList();

				Hue = 0x482;
				Movable = false;
			}

			public void Register(Mobile mob)
			{
				var entry = Find(mob);

				if (entry != null)
				{
					entry.Update();
					return;
				}

				_ = m_Entries.Add(new ReturnEntry(mob));
			}

			private ReturnEntry Find(Mobile mob)
			{
				for (var i = 0; i < m_Entries.Count; ++i)
				{
					var entry = (ReturnEntry)m_Entries[i];

					if (entry.Mobile == mob)
					{
						return entry;
					}
					else if (entry.Expired)
					{
						m_Entries.RemoveAt(i--);
					}
				}

				return null;
			}

			public override bool OnMoveOver(Mobile m)
			{
				if (!base.OnMoveOver(m))
				{
					return false;
				}

				var entry = Find(m);

				if (entry != null)
				{
					entry.Return();

					Effects.PlaySound(GetWorldLocation(), Map, 0x1FE);
					Effects.PlaySound(m.Location, m.Map, 0x1FE);

					m_Entries.Remove(entry);

					return false;
				}
				else
				{
					m.SendLocalizedMessage(1049383); // The teleporter doesn't seem to work for you.
					return true;
				}
			}

			public ExitTeleporter(Serial serial) : base(serial)
			{
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0);

				writer.WriteEncodedInt(m_Entries.Count);

				for (var i = 0; i < m_Entries.Count; ++i)
				{
					var entry = (ReturnEntry)m_Entries[i];

					writer.Write(entry.Mobile);
					writer.Write(entry.Location);
					writer.Write(entry.Facet);

					if (entry.Expired)
					{
						m_Entries.RemoveAt(i--);
					}
				}
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				var version = reader.ReadInt();

				switch (version)
				{
					case 0:
						{
							var count = reader.ReadEncodedInt();

							m_Entries = new ArrayList(count);

							for (var i = 0; i < count; ++i)
							{
								var mob = reader.ReadMobile();
								var loc = reader.ReadPoint3D();
								var map = reader.ReadMap();

								_ = m_Entries.Add(new ReturnEntry(mob, loc, map));
							}

							break;
						}
				}
			}
		}

		private class ArenaMoongate : ConfirmationMoongate
		{
			private readonly ExitTeleporter m_Teleporter;

			public override string DefaultName => "spectator moongate";

			public ArenaMoongate(Point3D target, Map map, ExitTeleporter tp) : base(target, map)
			{
				m_Teleporter = tp;

				ItemID = 0x1FD4;
				Dispellable = false;

				GumpWidth = 300;
				GumpHeight = 150;
				MessageColor = Color.White;
				MessageString = "Are you sure you wish to spectate this duel?";
				TitleColor = Color.OrangeRed;
				TitleNumber = 1062051; // Gate Warning

				_ = Timer.DelayCall(TimeSpan.FromSeconds(10.0), Delete);
			}

			public override void CheckGate(Mobile m, int range)
			{
				if (DuelContext.CheckCombat(m))
				{
					m.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
				}
				else
				{
					base.CheckGate(m, range);
				}
			}

			public override void UseGate(Mobile m)
			{
				if (DuelContext.CheckCombat(m))
				{
					m.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
				}
				else
				{
					if (m_Teleporter != null && !m_Teleporter.Deleted)
					{
						m_Teleporter.Register(m);
					}

					base.UseGate(m);
				}
			}

			public void Appear(Point3D loc, Map map)
			{
				Effects.PlaySound(loc, map, 0x20E);
				MoveToWorld(loc, map);
			}

			public ArenaMoongate(Serial serial) : base(serial)
			{
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				var version = reader.ReadInt();

				Delete();
			}
		}

		public void RemoveAggressions(Mobile mob)
		{
			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var dp = p.Players[j];

					if (dp == null || dp.Mobile == mob)
					{
						continue;
					}

					mob.RemoveAggressed(dp.Mobile);
					mob.RemoveAggressor(dp.Mobile);
					dp.Mobile.RemoveAggressed(mob);
					dp.Mobile.RemoveAggressor(mob);
				}
			}
		}

		public void SendReadyUpGump()
		{
			if (!Registered)
			{
				return;
			}

			ReadyWait = true;
			ReadyCount = -1;

			var types = new Type[] { typeof(ReadyUpGump) };

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl == null)
					{
						continue;
					}

					var mob = pl.Mobile;

					if (mob != null)
					{
						if (m_Tournament == null)
						{
							CloseAndSendGump(mob, new ReadyUpGump(mob, this), types);
						}
					}
				}
			}
		}

		public string ValidateStart()
		{
			if (m_Tournament == null && TournamentController.IsActive)
			{
				return "a tournament is active";
			}

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var dp = p.Players[j];

					if (dp == null)
					{
						return "a slot is empty";
					}

					if (dp.Mobile.Region.IsPartOf(typeof(Regions.Jail)))
					{
						return String.Format("{0} is in jail", dp.Mobile.Name);
					}

					if (Sigil.ExistsOn(dp.Mobile))
					{
						return String.Format("{0} is holding a sigil", dp.Mobile.Name);
					}

					if (!dp.Mobile.Alive)
					{
						if (m_Tournament == null)
						{
							return String.Format("{0} is dead", dp.Mobile.Name);
						}
						else
						{
							dp.Mobile.Resurrect();
						}
					}

					if (m_Tournament == null && CheckCombat(dp.Mobile))
					{
						return String.Format("{0} is in combat", dp.Mobile.Name);
					}

					if (dp.Mobile.Mounted)
					{
						var mount = dp.Mobile.Mount;

						if (m_Tournament != null && mount != null)
						{
							mount.Rider = null;
						}
						else
						{
							return String.Format("{0} is mounted", dp.Mobile.Name);
						}
					}
				}
			}

			return null;
		}

		public Arena m_OverrideArena;
		public Tournament m_Tournament;
		public TournyMatch m_Match;
		public EventGame m_EventGame;

		public Tournament Tournament => m_Tournament;

		public void SendReadyGump(int count)
		{
			if (!Registered)
			{
				return;
			}

			if (count != -1)
			{
				m_StartedReadyCountdown = true;
			}

			ReadyCount = count;

			if (count == 0)
			{
				var error = ValidateStart();

				if (error != null)
				{
					for (var i = 0; i < m_Participants.Count; ++i)
					{
						var p = (Participant)m_Participants[i];

						for (var j = 0; j < p.Players.Length; ++j)
						{
							var dp = p.Players[j];

							if (dp != null)
							{
								dp.Mobile.SendMessage("The duel could not be started because {0}.", error);
							}
						}
					}

					StartCountdown(10, new CountdownCallback(SendReadyGump));

					return;
				}

				ReadyWait = false;

				var players = new List<Mobile>();

				for (var i = 0; i < m_Participants.Count; ++i)
				{
					var p = (Participant)m_Participants[i];

					for (var j = 0; j < p.Players.Length; ++j)
					{
						var dp = p.Players[j];

						if (dp != null)
						{
							players.Add(dp.Mobile);
						}
					}
				}

				var arena = m_OverrideArena;

				arena ??= Arena.FindArena(players);

				if (arena == null)
				{
					for (var i = 0; i < m_Participants.Count; ++i)
					{
						var p = (Participant)m_Participants[i];

						for (var j = 0; j < p.Players.Length; ++j)
						{
							var dp = p.Players[j];

							if (dp != null)
							{
								dp.Mobile.SendMessage("The duel could not be started because there are no arenas. If you want to stop waiting for a free arena, yield the duel.");
							}
						}
					}

					StartCountdown(10, new CountdownCallback(SendReadyGump));
					return;
				}

				if (!arena.IsOccupied)
				{
					Arena = arena;

					if (Initiator.Map == Map.Internal)
					{
						m_GatePoint = Initiator.LogoutLocation;
						m_GateFacet = Initiator.LogoutMap;
					}
					else
					{
						m_GatePoint = Initiator.Location;
						m_GateFacet = Initiator.Map;
					}


					if (arena.Teleporter is not ExitTeleporter tp)
					{
						arena.Teleporter = tp = new ExitTeleporter();
						tp.MoveToWorld(arena.GateOut == Point3D.Zero ? arena.Outside : arena.GateOut, arena.Facet);
					}

					var mg = new ArenaMoongate(arena.GateIn == Point3D.Zero ? arena.Outside : arena.GateIn, arena.Facet, tp);

					StartedBeginCountdown = true;

					for (var i = 0; i < m_Participants.Count; ++i)
					{
						var p = (Participant)m_Participants[i];

						for (var j = 0; j < p.Players.Length; ++j)
						{
							var pl = p.Players[j];

							if (pl == null)
							{
								continue;
							}

							tp.Register(pl.Mobile);

							pl.Mobile.Frozen = false; // reset timer just in case
							pl.Mobile.Frozen = true;

							Debuff(pl.Mobile);
							CancelSpell(pl.Mobile);

							pl.Mobile.Delta(MobileDelta.Noto);
						}

						arena.MoveInside(p.Players, i);
					}

					if (m_EventGame != null)
					{
						m_EventGame.OnStart();
					}

					StartCountdown(10, new CountdownCallback(SendBeginGump));

					mg.Appear(m_GatePoint, m_GateFacet);
				}
				else
				{
					for (var i = 0; i < m_Participants.Count; ++i)
					{
						var p = (Participant)m_Participants[i];

						for (var j = 0; j < p.Players.Length; ++j)
						{
							var dp = p.Players[j];

							if (dp != null)
							{
								dp.Mobile.SendMessage("The duel could not be started because all arenas are full. If you want to stop waiting for a free arena, yield the duel.");
							}
						}
					}

					StartCountdown(10, new CountdownCallback(SendReadyGump));
				}

				return;
			}

			ReadyWait = true;

			var isAllReady = true;

			var types = new Type[] { typeof(ReadyGump) };

			for (var i = 0; i < m_Participants.Count; ++i)
			{
				var p = (Participant)m_Participants[i];

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl == null)
					{
						continue;
					}

					var mob = pl.Mobile;

					if (pl.Ready)
					{
						if (m_Tournament == null)
						{
							CloseAndSendGump(mob, new ReadyGump(mob, this, count), types);
						}
					}
					else
					{
						isAllReady = false;
					}
				}
			}

			if (count == -1 && isAllReady)
			{
				StartCountdown(3, new CountdownCallback(SendReadyGump));
			}
		}

		public static void CloseAndSendGump(Mobile mob, Gump g, params Type[] types)
		{
			CloseAndSendGump(mob.NetState, g, types);
		}

		public static void CloseAndSendGump(NetState ns, Gump g, params Type[] types)
		{
			if (ns != null)
			{
				var mob = ns.Mobile;

				if (mob != null)
				{
					foreach (var type in types)
					{
						_ = mob.CloseGump(type);
					}

					_ = mob.SendGump(g);
				}
			}

			/*if ( ns == null )
				return;

			for ( int i = 0; i < types.Length; ++i )
				ns.Send( new CloseGump( Gump.GetTypeID( types[i] ), 0 ) );

			g.SendTo( ns );

			ns.AddGump( g );

			Packet[] packets = new Packet[types.Length + 1];

			for ( int i = 0; i < types.Length; ++i )
				packets[i] = new CloseGump( Gump.GetTypeID( types[i] ), 0 );

			packets[types.Length] = (Packet) typeof( Gump ).InvokeMember( "Compile", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, g, null, null );

			bool compress = ns.CompressionEnabled;
			ns.CompressionEnabled = false;
			ns.Send( BindPackets( compress, packets ) );
			ns.CompressionEnabled = compress;*/
		}

		/*public static Packet BindPackets( bool compress, params Packet[] packets )
		{
			if ( packets.Length == 0 )
				throw new ArgumentException( "No packets to bind", "packets" );

			byte[][] compiled = new byte[packets.Length][];
			int[] lengths = new int[packets.Length];

			int length = 0;

			for ( int i = 0; i < packets.Length; ++i )
			{
				compiled[i] = packets[i].Compile( compress, out lengths[i] );
				length += lengths[i];
			}

			return new BoundPackets( length, compiled, lengths );
		}

		private class BoundPackets : Packet
		{
			public BoundPackets( int length, byte[][] compiled, int[] lengths ) : base( 0, length )
			{
				m_Stream.Seek( 0, System.IO.SeekOrigin.Begin );

				for ( int i = 0; i < compiled.Length; ++i )
					m_Stream.Write( compiled[i], 0, lengths[i] );
			}
		}*/
	}

	public class DuelContextGump : Gump
	{
		private readonly DuelContext m_Context;

		public Mobile From { get; }
		public DuelContext Context => m_Context;

		public string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public void AddGoldenButton(int x, int y, int bid)
		{
			AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
			AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
		}

		public void AddGoldenButtonLabeled(int x, int y, int bid, string text)
		{
			AddGoldenButton(x, y, bid);
			AddHtml(x + 25, y, 200, 20, text, false, false);
		}

		public DuelContextGump(Mobile from, DuelContext context) : base(50, 50)
		{
			From = from;
			m_Context = context;

			_ = from.CloseGump(typeof(RulesetGump));
			_ = from.CloseGump(typeof(DuelContextGump));
			_ = from.CloseGump(typeof(ParticipantGump));

			var count = context.Participants.Count;

			if (count < 3)
			{
				count = 3;
			}

			var height = 35 + 10 + 22 + 30 + 22 + 22 + 2 + (count * 22) + 2 + 30;

			AddPage(0);

			AddBackground(0, 0, 300, height, 9250);
			AddBackground(10, 10, 280, height - 20, 0xDAC);

			AddHtml(35, 25, 230, 20, Center("Duel Setup"), false, false);

			var x = 35;
			var y = 47;

			AddGoldenButtonLabeled(x, y, 1, "Rules");
			y += 22;
			AddGoldenButtonLabeled(x, y, 2, "Start");
			y += 22;
			AddGoldenButtonLabeled(x, y, 3, "Add Participant");
			y += 30;

			AddHtml(35, y, 230, 20, Center("Participants"), false, false);
			y += 22;

			for (var i = 0; i < context.Participants.Count; ++i)
			{
				var p = (Participant)context.Participants[i];

				AddGoldenButtonLabeled(x, y, 4 + i, String.Format(p.Count == 1 ? "Player {0}: {3}" : "Team {0}: {1}/{2}: {3}", 1 + i, p.FilledSlots, p.Count, p.NameList));
				y += 22;
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (!m_Context.Registered)
			{
				return;
			}

			var index = info.ButtonID;

			switch (index)
			{
				case -1: // CloseGump
					{
						break;
					}
				case 0: // closed
					{
						m_Context.Unregister();
						break;
					}
				case 1: // Rules
					{
						//m_From.SendGump( new RulesetGump( m_From, m_Context.Ruleset, m_Context.Ruleset.Layout, m_Context ) );
						_ = From.SendGump(new PickRulesetGump(From, m_Context, m_Context.Ruleset));
						break;
					}
				case 2: // Start
					{
						if (m_Context.CheckFull())
						{
							m_Context.CloseAllGumps();
							m_Context.SendReadyUpGump();
							//m_Context.SendReadyGump();
						}
						else
						{
							From.SendMessage("You cannot start the duel before all participating players have been assigned.");
							_ = From.SendGump(new DuelContextGump(From, m_Context));
						}

						break;
					}
				case 3: // New Participant
					{
						if (m_Context.Participants.Count < 10)
						{
							_ = m_Context.Participants.Add(new Participant(m_Context, 1));
						}
						else
						{
							From.SendMessage("The number of participating parties may not be increased further.");
						}

						_ = From.SendGump(new DuelContextGump(From, m_Context));

						break;
					}
				default: // Participant
					{
						index -= 4;

						if (index >= 0 && index < m_Context.Participants.Count)
						{
							_ = From.SendGump(new ParticipantGump(From, m_Context, (Participant)m_Context.Participants[index]));
						}

						break;
					}
			}
		}
	}

	/// Duel Accepted
	public class AcceptDuelGump : Gump
	{
		private readonly Mobile m_Challenger, m_Challenged;
		private readonly DuelContext m_Context;
		private readonly Participant m_Participant;
		private readonly int m_Slot;

		public string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public string Color(string text, int color)
		{
			return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
		}

		private const int LabelColor32 = 0xFFFFFF;
		private const int BlackColor32 = 0x000008;

		private bool m_Active = true;

		public AcceptDuelGump(Mobile challenger, Mobile challenged, DuelContext context, Participant p, int slot) : base(50, 50)
		{
			m_Challenger = challenger;
			m_Challenged = challenged;
			m_Context = context;
			m_Participant = p;
			m_Slot = slot;

			_ = challenged.CloseGump(typeof(AcceptDuelGump));

			Closable = false;

			AddPage(0);

			//AddBackground( 0, 0, 400, 220, 9150 );
			AddBackground(1, 1, 398, 218, 3600);
			//AddBackground( 16, 15, 369, 189, 9100 );

			AddImageTiled(16, 15, 369, 189, 3604);
			AddAlphaRegion(16, 15, 369, 189);

			AddImage(215, -43, 0xEE40);
			//AddImage( 330, 141, 0x8BA );

			AddHtml(22 - 1, 22, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
			AddHtml(22 + 1, 22, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
			AddHtml(22, 22 - 1, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
			AddHtml(22, 22 + 1, 294, 20, Color(Center("Duel Challenge"), BlackColor32), false, false);
			AddHtml(22, 22, 294, 20, Color(Center("Duel Challenge"), LabelColor32), false, false);

			string fmt;

			if (p.Contains(challenger))
			{
				fmt = "You have been asked to join sides with {0} in a duel. Do you accept?";
			}
			else
			{
				fmt = "You have been challenged to a duel from {0}. Do you accept?";
			}

			AddHtml(22 - 1, 50, 294, 40, Color(String.Format(fmt, challenger.Name), BlackColor32), false, false);
			AddHtml(22 + 1, 50, 294, 40, Color(String.Format(fmt, challenger.Name), BlackColor32), false, false);
			AddHtml(22, 50 - 1, 294, 40, Color(String.Format(fmt, challenger.Name), BlackColor32), false, false);
			AddHtml(22, 50 + 1, 294, 40, Color(String.Format(fmt, challenger.Name), BlackColor32), false, false);
			AddHtml(22, 50, 294, 40, Color(String.Format(fmt, challenger.Name), 0xB0C868), false, false);

			AddImageTiled(32, 88, 264, 1, 9107);
			AddImageTiled(42, 90, 264, 1, 9157);

			AddRadio(24, 100, 9727, 9730, true, 1);
			AddHtml(60 - 1, 105, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
			AddHtml(60 + 1, 105, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
			AddHtml(60, 105 - 1, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
			AddHtml(60, 105 + 1, 250, 20, Color("Yes, I will fight this duel.", BlackColor32), false, false);
			AddHtml(60, 105, 250, 20, Color("Yes, I will fight this duel.", LabelColor32), false, false);

			AddRadio(24, 135, 9727, 9730, false, 2);
			AddHtml(60 - 1, 140, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
			AddHtml(60 + 1, 140, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
			AddHtml(60, 140 - 1, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
			AddHtml(60, 140 + 1, 250, 20, Color("No, I do not wish to fight.", BlackColor32), false, false);
			AddHtml(60, 140, 250, 20, Color("No, I do not wish to fight.", LabelColor32), false, false);

			AddRadio(24, 170, 9727, 9730, false, 3);
			AddHtml(60 - 1, 175, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
			AddHtml(60 + 1, 175, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
			AddHtml(60, 175 - 1, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
			AddHtml(60, 175 + 1, 250, 20, Color("No, knave. Do not ask again.", BlackColor32), false, false);
			AddHtml(60, 175, 250, 20, Color("No, knave. Do not ask again.", LabelColor32), false, false);

			AddButton(314, 173, 247, 248, 1, GumpButtonType.Reply, 0);

			_ = Timer.DelayCall(TimeSpan.FromSeconds(15.0), AutoReject);
		}

		public void AutoReject()
		{
			if (!m_Active)
			{
				return;
			}

			m_Active = false;

			_ = m_Challenged.CloseGump(typeof(AcceptDuelGump));

			m_Challenger.SendMessage("{0} seems unresponsive.", m_Challenged.Name);
			m_Challenged.SendMessage("You decline the challenge.");
		}

		private static readonly Hashtable m_IgnoreLists = new();

		private class IgnoreEntry
		{
			public Mobile m_Ignored;
			public DateTime m_Expire;

			public Mobile Ignored => m_Ignored;
			public bool Expired => DateTime.UtcNow >= m_Expire;

			private static readonly TimeSpan ExpireDelay = TimeSpan.FromMinutes(15.0);

			public void Refresh()
			{
				m_Expire = DateTime.UtcNow + ExpireDelay;
			}

			public IgnoreEntry(Mobile ignored)
			{
				m_Ignored = ignored;
				Refresh();
			}
		}

		public static void BeginIgnore(Mobile source, Mobile toIgnore)
		{
			var list = (ArrayList)m_IgnoreLists[source];

			if (list == null)
			{
				m_IgnoreLists[source] = list = new ArrayList();
			}

			for (var i = 0; i < list.Count; ++i)
			{
				var ie = (IgnoreEntry)list[i];

				if (ie.Ignored == toIgnore)
				{
					ie.Refresh();
					return;
				}
				else if (ie.Expired)
				{
					list.RemoveAt(i--);
				}
			}

			_ = list.Add(new IgnoreEntry(toIgnore));
		}

		public static bool IsIgnored(Mobile source, Mobile check)
		{
			var list = (ArrayList)m_IgnoreLists[source];

			if (list == null)
			{
				return false;
			}

			for (var i = 0; i < list.Count; ++i)
			{
				var ie = (IgnoreEntry)list[i];

				if (ie.Expired)
				{
					list.RemoveAt(i--);
				}
				else if (ie.Ignored == check)
				{
					return true;
				}
			}

			return false;
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID != 1 || !m_Active || !m_Context.Registered)
			{
				return;
			}

			m_Active = false;

			if (!m_Context.Participants.Contains(m_Participant))
			{
				return;
			}

			if (info.IsSwitched(1))
			{
				if (m_Challenged is not PlayerMobile pm)
				{
					return;
				}

				if (pm.DuelContext != null)
				{
					if (pm.DuelContext.Initiator == pm)
					{
						pm.SendMessage(0x22, "You have already started a duel.");
					}
					else
					{
						pm.SendMessage(0x22, "You have already been challenged in a duel.");
					}

					m_Challenger.SendMessage("{0} cannot fight because they are already assigned to another duel.", pm.Name);
				}
				else if (DuelContext.CheckCombat(pm))
				{
					pm.SendMessage(0x22, "You have recently been in combat with another player and must wait before starting a duel.");
					m_Challenger.SendMessage("{0} cannot fight because they have recently been in combat with another player.", pm.Name);
				}
				else if (TournamentController.IsActive)
				{
					pm.SendMessage(0x22, "A tournament is currently active and you may not duel.");
					m_Challenger.SendMessage(0x22, "A tournament is currently active and you may not duel.");
				}
				else
				{
					var added = false;

					if (m_Slot >= 0 && m_Slot < m_Participant.Players.Length && m_Participant.Players[m_Slot] == null)
					{
						added = true;
						m_Participant.Players[m_Slot] = new DuelPlayer(m_Challenged, m_Participant);
					}
					else
					{
						for (var i = 0; i < m_Participant.Players.Length; ++i)
						{
							if (m_Participant.Players[i] == null)
							{
								added = true;
								m_Participant.Players[i] = new DuelPlayer(m_Challenged, m_Participant);
								break;
							}
						}
					}

					if (added)
					{
						m_Challenger.SendMessage("{0} has accepted the request.", m_Challenged.Name);
						m_Challenged.SendMessage("You have accepted the request from {0}.", m_Challenger.Name);

						var ns = m_Challenger.NetState;

						if (ns != null)
						{
							foreach (var g in ns.Gumps)
							{
								if (g is ParticipantGump pg)
								{
									if (pg.Participant == m_Participant)
									{
										_ = m_Challenger.SendGump(new ParticipantGump(m_Challenger, m_Context, m_Participant));
										break;
									}
								}
								else if (g is DuelContextGump dcg)
								{
									if (dcg.Context == m_Context)
									{
										_ = m_Challenger.SendGump(new DuelContextGump(m_Challenger, m_Context));
										break;
									}
								}
							}
						}
					}
					else
					{
						m_Challenger.SendMessage("The participant list was full and so {0} could not join.", m_Challenged.Name);
						m_Challenged.SendMessage("The participant list was full and so you could not join the fight {1} {0}.", m_Challenger.Name, m_Participant.Contains(m_Challenger) ? "with" : "against");
					}
				}
			}
			else
			{
				if (info.IsSwitched(3))
				{
					BeginIgnore(m_Challenged, m_Challenger);
				}

				m_Challenger.SendMessage("{0} does not wish to fight.", m_Challenged.Name);
				m_Challenged.SendMessage("You chose not to fight {1} {0}.", m_Challenger.Name, m_Participant.Contains(m_Challenger) ? "with" : "against");
			}
		}
	}

	public class BeginGump : Gump
	{
		public string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public string Color(string text, int color)
		{
			return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
		}

		private const int LabelColor32 = 0xFFFFFF;
		private const int BlackColor32 = 0x000008;

		public BeginGump(int count) : base(50, 50)
		{
			AddPage(0);

			const int offset = 50;

			AddBackground(1, 1, 398, 202 - offset, 3600);

			AddImageTiled(16, 15, 369, 173 - offset, 3604);
			AddAlphaRegion(16, 15, 369, 173 - offset);

			AddImage(215, -43, 0xEE40);

			AddHtml(22 - 1, 22, 294, 20, Color(Center("Duel Countdown"), BlackColor32), false, false);
			AddHtml(22 + 1, 22, 294, 20, Color(Center("Duel Countdown"), BlackColor32), false, false);
			AddHtml(22, 22 - 1, 294, 20, Color(Center("Duel Countdown"), BlackColor32), false, false);
			AddHtml(22, 22 + 1, 294, 20, Color(Center("Duel Countdown"), BlackColor32), false, false);
			AddHtml(22, 22, 294, 20, Color(Center("Duel Countdown"), LabelColor32), false, false);

			AddHtml(22 - 1, 50, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", BlackColor32), false, false);
			AddHtml(22 + 1, 50, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", BlackColor32), false, false);
			AddHtml(22, 50 - 1, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", BlackColor32), false, false);
			AddHtml(22, 50 + 1, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", BlackColor32), false, false);
			AddHtml(22, 50, 294, 80, Color("The arranged duel is about to begin. During this countdown period you may not cast spells and you may not move. This message will close automatically when the period ends.", 0xFFCC66), false, false);

			/*AddImageTiled( 32, 128, 264, 1, 9107 );
			AddImageTiled( 42, 130, 264, 1, 9157 );

			AddHtml( 60-1, 140, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#{2:X6}>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", BlackColor32 ), BlackColor32 ), false, false );
			AddHtml( 60+1, 140, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#{2:X6}>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", BlackColor32 ), BlackColor32 ), false, false );
			AddHtml( 60, 140-1, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#{2:X6}>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", BlackColor32 ), BlackColor32 ), false, false );
			AddHtml( 60, 140+1, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#{2:X6}>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", BlackColor32 ), BlackColor32 ), false, false );
			AddHtml( 60, 140, 250, 20, Color( String.Format( "Duel will begin in <BASEFONT COLOR=#FF6666>{0} <BASEFONT COLOR=#{2:X6}>second{1}.", count, count==1?"":"s", 0x66AACC ), 0x66AACC ), false, false );*/

			AddButton(314 - 50, 157 - offset, 247, 248, 1, GumpButtonType.Reply, 0);
		}
	}

	/// Players Ready?
	public class ReadyGump : Gump
	{
		private readonly Mobile m_From;
		private readonly DuelContext m_Context;
		private readonly int m_Count;

		public string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public ReadyGump(Mobile from, DuelContext context, int count) : base(50, 50)
		{
			m_From = from;
			m_Context = context;
			m_Count = count;

			var parts = context.Participants;

			var height = 25 + 20;

			for (var i = 0; i < parts.Count; ++i)
			{
				var p = (Participant)parts[i];

				height += 4;

				if (p.Players.Length > 1)
				{
					height += 22;
				}

				height += p.Players.Length * 22;
			}

			height += 25;

			Closable = false;
			Dragable = false;

			AddPage(0);

			AddBackground(0, 0, 260, height, 9250);
			AddBackground(10, 10, 240, height - 20, 0xDAC);

			if (count == -1)
			{
				AddHtml(35, 25, 190, 20, Center("Ready"), false, false);
			}
			else
			{
				AddHtml(35, 25, 190, 20, Center("Starting"), false, false);
				AddHtml(35, 25, 190, 20, "<DIV ALIGN=RIGHT>" + count.ToString(), false, false);
			}

			var y = 25 + 20;

			for (var i = 0; i < parts.Count; ++i)
			{
				var p = (Participant)parts[i];

				y += 4;

				var isAllReady = true;
				var yStore = y;
				var offset = 0;

				if (p.Players.Length > 1)
				{
					AddHtml(35 + 14, y, 176, 20, String.Format("Participant #{0}", i + 1), false, false);
					y += 22;
					offset = 10;
				}

				for (var j = 0; j < p.Players.Length; ++j)
				{
					var pl = p.Players[j];

					if (pl != null && pl.Ready)
					{
						AddImage(35 + offset, y + 4, 0x939);
					}
					else
					{
						AddImage(35 + offset, y + 4, 0x938);
						isAllReady = false;
					}

					var name = pl == null ? "(Empty)" : pl.Mobile.Name;

					AddHtml(35 + offset + 14, y, 166, 20, name, false, false);

					y += 22;
				}

				if (p.Players.Length > 1)
				{
					AddImage(35, yStore + 4, isAllReady ? 0x939 : 0x938);
				}
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
		}
	}

	public class ReadyUpGump : Gump
	{
		private readonly Mobile m_From;
		private readonly DuelContext m_Context;

		public string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public void AddGoldenButton(int x, int y, int bid)
		{
			AddButton(x, y, 0xD2, 0xD2, bid, GumpButtonType.Reply, 0);
			AddButton(x + 3, y + 3, 0xD8, 0xD8, bid, GumpButtonType.Reply, 0);
		}

		public ReadyUpGump(Mobile from, DuelContext context) : base(50, 50)
		{
			m_From = from;
			m_Context = context;

			Closable = false;
			AddPage(0);

			if (context.Rematch)
			{
				var height = 25 + 20 + 10 + 22 + 25;

				AddBackground(0, 0, 210, height, 9250);
				AddBackground(10, 10, 190, height - 20, 0xDAC);

				AddHtml(35, 25, 140, 20, Center("Rematch?"), false, false);

				AddButton(35, 55, 247, 248, 1, GumpButtonType.Reply, 0);
				AddButton(115, 55, 242, 241, 2, GumpButtonType.Reply, 0);
			}
			else
			{
				#region Participants
				AddPage(1);

				var parts = context.Participants;

				var height = 25 + 20;

				for (var i = 0; i < parts.Count; ++i)
				{
					var p = (Participant)parts[i];

					height += 4;

					if (p.Players.Length > 1)
					{
						height += 22;
					}

					height += p.Players.Length * 22;
				}

				height += 10 + 22 + 25;

				AddBackground(0, 0, 260, height, 9250);
				AddBackground(10, 10, 240, height - 20, 0xDAC);

				AddHtml(35, 25, 190, 20, Center("Participants"), false, false);

				var y = 20 + 25;

				for (var i = 0; i < parts.Count; ++i)
				{
					var p = (Participant)parts[i];

					y += 4;

					var offset = 0;

					if (p.Players.Length > 1)
					{
						AddHtml(35, y, 176, 20, String.Format("Team #{0}", i + 1), false, false);
						y += 22;
						offset = 10;
					}

					for (var j = 0; j < p.Players.Length; ++j)
					{
						var pl = p.Players[j];

						var name = pl == null ? "(Empty)" : pl.Mobile.Name;

						AddHtml(35 + offset, y, 166, 20, name, false, false);

						y += 22;
					}
				}

				y += 8;

				AddHtml(35, y, 176, 20, "Continue?", false, false);

				y -= 2;

				AddButton(102, y, 247, 248, 0, GumpButtonType.Page, 2);
				AddButton(169, y, 242, 241, 2, GumpButtonType.Reply, 0);
				#endregion

				#region Rules
				AddPage(2);

				var ruleset = context.Ruleset;
				var basedef = ruleset.Base;

				height = 25 + 20 + 5 + 20 + 20 + 4;

				var changes = 0;

				BitArray defs;

				if (ruleset.Flavors.Count > 0)
				{
					defs = new BitArray(basedef.Options);

					for (var i = 0; i < ruleset.Flavors.Count; ++i)
					{
						_ = defs.Or(((Ruleset)ruleset.Flavors[i]).Options);
					}

					height += ruleset.Flavors.Count * 18;
				}
				else
				{
					defs = basedef.Options;
				}

				var opts = ruleset.Options;

				for (var i = 0; i < opts.Length; ++i)
				{
					if (defs[i] != opts[i])
					{
						++changes;
					}
				}

				height += changes * 22;

				height += 10 + 22 + 25;

				AddBackground(0, 0, 260, height, 9250);
				AddBackground(10, 10, 240, height - 20, 0xDAC);

				AddHtml(35, 25, 190, 20, Center("Rules"), false, false);

				AddHtml(35, 50, 190, 20, String.Format("Set: {0}", basedef.Title), false, false);

				y = 70;

				for (var i = 0; i < ruleset.Flavors.Count; ++i, y += 18)
				{
					AddHtml(35, y, 190, 20, String.Format(" + {0}", ((Ruleset)ruleset.Flavors[i]).Title), false, false);
				}

				y += 4;

				if (changes > 0)
				{
					AddHtml(35, y, 190, 20, "Modifications:", false, false);
					y += 20;

					for (var i = 0; i < opts.Length; ++i)
					{
						if (defs[i] != opts[i])
						{
							var name = ruleset.Layout.FindByIndex(i);

							if (name != null) // sanity
							{
								AddImage(35, y, opts[i] ? 0xD3 : 0xD2);
								AddHtml(60, y, 165, 22, name, false, false);
							}

							y += 22;
						}
					}
				}
				else
				{
					AddHtml(35, y, 190, 20, "Modifications: None", false, false);
					y += 20;
				}

				y += 8;

				AddHtml(35, y, 176, 20, "Continue?", false, false);

				y -= 2;

				AddButton(102, y, 247, 248, 1, GumpButtonType.Reply, 0);
				AddButton(169, y, 242, 241, 3, GumpButtonType.Reply, 0);
				#endregion
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (!m_Context.Registered || !m_Context.ReadyWait)
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // okay
					{
						if (m_From is not PlayerMobile pm)
						{
							break;
						}

						pm.DuelPlayer.Ready = true;
						m_Context.SendReadyGump();

						break;
					}
				case 2: // reject participants
					{
						m_Context.RejectReady(m_From, "participants");
						break;
					}
				case 3: // reject rules
					{
						m_Context.RejectReady(m_From, "rules");
						break;
					}
			}
		}
	}
}