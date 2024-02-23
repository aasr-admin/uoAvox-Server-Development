using Server.Commands;
using Server.Commands.Generic;
using Server.Factions;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Prompts;
using Server.Targeting;

using System;
using System.Collections.Generic;

#region Developer Notations

/// This file contains two different guild systems that can be implemented on your server.
///
/// Paperdoll Guild System: this is the new guild system that is accessible using a button
///                         that is located on a players paperdoll. 
///
/// Monolith Stone System: this is the really old classic guild system which used an item
///                        called a Guildstone to control a guild and its members. 

#endregion

namespace Server.Guilds
{
	public class Guild : BaseGuild
	{
		public static void Configure()
		{
			EventSink.CreateGuild += new CreateGuildHandler(EventSink_CreateGuild);
			EventSink.GuildGumpRequest += new GuildGumpRequestHandler(EventSink_GuildGumpRequest);

			CommandSystem.Register("GuildProps", AccessLevel.Counselor, new CommandEventHandler(GuildProps_OnCommand));
		}

		#region GuildProps
		[Usage("GuildProps")]
		[Description("Opens a menu where you can view and edit guild properties of a targeted player or guild stone.  If the new Guild system is active, also brings up the guild gump.")]
		private static void GuildProps_OnCommand(CommandEventArgs e)
		{
			var arg = e.ArgString.Trim();
			var from = e.Mobile;

			if (arg.Length == 0)
			{
				e.Mobile.Target = new GuildPropsTarget();
			}
			else
			{
				Guild g = null;

				int id;

				if (Int32.TryParse(arg, out id))
				{
					g = Guild.Find(id) as Guild;
				}

				if (g == null)
				{
					g = Guild.FindByAbbrev(arg) as Guild;

					if (g == null)
					{
						g = Guild.FindByName(arg) as Guild;
					}
				}

				if (g != null)
				{
					from.SendGump(new PropertiesGump(from, g));

					if (NewGuildSystem && from.AccessLevel >= AccessLevel.GameMaster && from is PlayerMobile)
					{
						from.SendGump(new GuildInfoGump((PlayerMobile)from, g));
					}
				}
			}

		}

		private class GuildPropsTarget : Target
		{
			public GuildPropsTarget() : base(-1, true, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (!BaseCommand.IsAccessible(from, o))
				{
					from.SendMessage("That is not accessible.");
					return;
				}

				Guild g = null;

				if (o is Guildstone)
				{
					var stone = o as Guildstone;
					if (stone.Guild == null || stone.Guild.Disbanded)
					{
						from.SendMessage("The guild associated with that Guildstone no longer exists");
						return;
					}
					else
					{
						g = stone.Guild;
					}
				}
				else if (o is Mobile)
				{
					g = ((Mobile)o).Guild as Guild;
				}

				if (g != null)
				{
					from.SendGump(new PropertiesGump(from, g));

					if (NewGuildSystem && from.AccessLevel >= AccessLevel.GameMaster && from is PlayerMobile)
					{
						from.SendGump(new GuildInfoGump((PlayerMobile)from, g));
					}
				}
				else
				{
					from.SendMessage("That is not in a guild!");
				}
			}
		}
		#endregion

		#region EventSinks
		public static void EventSink_GuildGumpRequest(GuildGumpRequestEventArgs args)
		{
			var pm = args.Mobile as PlayerMobile;
			if (!NewGuildSystem || pm == null)
			{
				return;
			}

			if (pm.Guild == null)
			{
				pm.SendGump(new CreateGuildGump(pm));
			}
			else
			{
				pm.SendGump(new GuildInfoGump(pm, pm.Guild as Guild));
			}
		}

		public static void EventSink_CreateGuild(CreateGuildEventArgs args)
		{
			args.Guild = new Guild(args.Id);
		}
		#endregion

		public static bool NewGuildSystem => Core.SE;
		public static bool OrderChaos => !Core.SE;

		public static readonly int RegistrationFee = 25000;
		public static readonly int AbbrevLimit = 4;
		public static readonly int NameLimit = 40;
		public static readonly int MajorityPercentage = 66;
		public static readonly TimeSpan InactiveTime = TimeSpan.FromDays(30);


		#region New Alliances

		public AllianceInfo Alliance
		{
			get
			{
				if (m_AllianceInfo != null)
				{
					return m_AllianceInfo;
				}
				else if (m_AllianceLeader != null)
				{
					return m_AllianceLeader.m_AllianceInfo;
				}
				else
				{
					return null;
				}
			}
			set
			{
				var current = Alliance;

				if (value == current)
				{
					return;
				}

				if (current != null)
				{
					current.RemoveGuild(this);
				}

				if (value != null)
				{

					if (value.Leader == this)
					{
						m_AllianceInfo = value;
					}
					else
					{
						m_AllianceLeader = value.Leader;
					}

					value.AddPendingGuild(this);
				}
				else
				{
					m_AllianceInfo = null;
					m_AllianceLeader = null;
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public string AllianceName
		{
			get
			{
				var al = Alliance;
				if (al != null)
				{
					return al.Name;
				}

				return null;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public Guild AllianceLeader
		{
			get
			{
				var al = Alliance;

				if (al != null)
				{
					return al.Leader;
				}

				return null;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsAllianceMember
		{
			get
			{
				var al = Alliance;

				if (al != null)
				{
					return al.IsMember(this);
				}

				return false;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public bool IsAlliancePendingMember
		{
			get
			{
				var al = Alliance;

				if (al != null)
				{
					return al.IsPendingMember(this);
				}

				return false;
			}
		}

		public static Guild GetAllianceLeader(Guild g)
		{
			var alliance = g.Alliance;

			if (alliance != null && alliance.Leader != null && alliance.IsMember(g))
			{
				return alliance.Leader;
			}

			return g;
		}

		#endregion

		#region New Wars

		public List<WarDeclaration> PendingWars => m_PendingWars;
		public List<WarDeclaration> AcceptedWars => m_AcceptedWars;


		public WarDeclaration FindPendingWar(Guild g)
		{
			for (var i = 0; i < PendingWars.Count; i++)
			{
				var w = PendingWars[i];

				if (w.Opponent == g)
				{
					return w;
				}
			}

			return null;
		}

		public WarDeclaration FindActiveWar(Guild g)
		{
			for (var i = 0; i < AcceptedWars.Count; i++)
			{
				var w = AcceptedWars[i];

				if (w.Opponent == g)
				{
					return w;
				}
			}

			return null;
		}

		public void CheckExpiredWars()
		{
			for (var i = 0; i < AcceptedWars.Count; i++)
			{
				var w = AcceptedWars[i];
				var g = w.Opponent;

				var status = w.Status;

				if (status != WarStatus.InProgress)
				{
					var myAlliance = Alliance;
					var inAlliance = (myAlliance != null && myAlliance.IsMember(this));

					var otherAlliance = ((g != null) ? g.Alliance : null);
					var otherInAlliance = (otherAlliance != null && otherAlliance.IsMember(this));

					if (inAlliance)
					{
						myAlliance.AllianceMessage(1070739 + (int)status, (g == null) ? "a deleted opponent" : (otherInAlliance ? otherAlliance.Name : g.Name));
						myAlliance.InvalidateMemberProperties();
					}
					else
					{
						GuildMessage(1070739 + (int)status, (g == null) ? "a deleted opponent" : (otherInAlliance ? otherAlliance.Name : g.Name));
						InvalidateMemberProperties();
					}

					AcceptedWars.Remove(w);

					if (g != null)
					{
						if (status != WarStatus.Draw)
						{
							status = (WarStatus)((int)status + 1 % 2);
						}

						if (otherInAlliance)
						{
							otherAlliance.AllianceMessage(1070739 + (int)status, (inAlliance ? Alliance.Name : Name));
							otherAlliance.InvalidateMemberProperties();
						}
						else
						{
							g.GuildMessage(1070739 + (int)status, (inAlliance ? Alliance.Name : Name));
							g.InvalidateMemberProperties();
						}

						g.AcceptedWars.Remove(g.FindActiveWar(this));
					}
				}
			}

			for (var i = 0; i < PendingWars.Count; i++)
			{
				var w = PendingWars[i];
				var g = w.Opponent;

				if (w.Status != WarStatus.Pending)
				{
					//All sanity in here
					PendingWars.Remove(w);

					if (g != null)
					{
						g.PendingWars.Remove(g.FindPendingWar(this));
					}
				}
			}
		}

		public static void HandleDeath(Mobile victim)
		{
			HandleDeath(victim, null);
		}

		public static void HandleDeath(Mobile victim, Mobile killer)
		{
			if (!NewGuildSystem)
			{
				return;
			}

			if (killer == null)
			{
				killer = victim.FindMostRecentDamager(false);
			}

			if (killer == null || victim.Guild == null || killer.Guild == null)
			{
				return;
			}

			var victimGuild = GetAllianceLeader(victim.Guild as Guild);
			var killerGuild = GetAllianceLeader(killer.Guild as Guild);

			var war = killerGuild.FindActiveWar(victimGuild);

			if (war == null)
			{
				return;
			}

			war.Kills++;

			if (war.Opponent == victimGuild)
			{
				killerGuild.CheckExpiredWars();
			}
			else
			{
				victimGuild.CheckExpiredWars();
			}
		}
		#endregion

		#region Var declarations
		private Mobile m_Leader;

		private string m_Name;
		private string m_Abbreviation;

		private List<Guild> m_Allies;
		private List<Guild> m_Enemies;

		private List<Mobile> m_Members;

		private Item m_Guildstone;
		private Item m_Teleporter;

		private string m_Charter;
		private string m_Website;

		private DateTime m_LastFealty;

		private GuildType m_Type;
		private DateTime m_TypeLastChange;

		private List<Guild> m_AllyDeclarations, m_AllyInvitations;

		private List<Guild> m_WarDeclarations, m_WarInvitations;
		private List<Mobile> m_Candidates, m_Accepted;

		private List<WarDeclaration> m_PendingWars, m_AcceptedWars;

		private AllianceInfo m_AllianceInfo;
		private Guild m_AllianceLeader;
		#endregion

		public Guild(Mobile leader, string name, string abbreviation)
		{
			#region Ctor mumbo-jumbo
			m_Leader = leader;

			m_Members = new List<Mobile>();
			m_Allies = new List<Guild>();
			m_Enemies = new List<Guild>();
			m_WarDeclarations = new List<Guild>();
			m_WarInvitations = new List<Guild>();
			m_AllyDeclarations = new List<Guild>();
			m_AllyInvitations = new List<Guild>();
			m_Candidates = new List<Mobile>();
			m_Accepted = new List<Mobile>();

			m_LastFealty = DateTime.UtcNow;

			m_Name = name;
			m_Abbreviation = abbreviation;

			m_TypeLastChange = DateTime.MinValue;

			AddMember(m_Leader);

			if (m_Leader is PlayerMobile)
			{
				((PlayerMobile)m_Leader).GuildRank = RankDefinition.Leader;
			}

			m_AcceptedWars = new List<WarDeclaration>();
			m_PendingWars = new List<WarDeclaration>();
			#endregion
		}

		public Guild(int id) : base(id)//serialization ctor
		{
		}

		public void InvalidateMemberProperties()
		{
			InvalidateMemberProperties(false);
		}

		public void InvalidateMemberProperties(bool onlyOPL)
		{
			if (m_Members != null)
			{
				for (var i = 0; i < m_Members.Count; i++)
				{
					var m = m_Members[i];
					m.InvalidateProperties();

					if (!onlyOPL)
					{
						m.Delta(MobileDelta.Noto);
					}
				}
			}
		}

		public void InvalidateMemberNotoriety()
		{
			if (m_Members != null)
			{
				for (var i = 0; i < m_Members.Count; i++)
				{
					m_Members[i].Delta(MobileDelta.Noto);
				}
			}
		}

		public void InvalidateWarNotoriety()
		{
			var g = GetAllianceLeader(this);

			if (g.Alliance != null)
			{
				g.Alliance.InvalidateMemberNotoriety();
			}
			else
			{
				g.InvalidateMemberNotoriety();
			}

			if (g.AcceptedWars == null)
			{
				return;
			}

			foreach (var warDec in g.AcceptedWars)
			{
				var opponent = warDec.Opponent;

				if (opponent.Alliance != null)
				{
					opponent.Alliance.InvalidateMemberNotoriety();
				}
				else
				{
					opponent.InvalidateMemberNotoriety();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Leader
		{
			get
			{
				if (m_Leader == null || m_Leader.Deleted || m_Leader.Guild != this)
				{
					CalculateGuildmaster();
				}

				return m_Leader;
			}
			set
			{
				if (value != null)
				{
					AddMember(value); //Also removes from old guild.
				}

				if (m_Leader is PlayerMobile && m_Leader.Guild == this)
				{
					((PlayerMobile)m_Leader).GuildRank = RankDefinition.Member;
				}

				m_Leader = value;

				if (m_Leader is PlayerMobile)
				{
					((PlayerMobile)m_Leader).GuildRank = RankDefinition.Leader;
				}
			}
		}


		public override bool Disbanded => (m_Leader == null || m_Leader.Deleted);

		public override void OnDelete(Mobile mob)
		{
			RemoveMember(mob);
		}


		public void Disband()
		{
			m_Leader = null;

			BaseGuild.List.Remove(Id);

			foreach (var m in m_Members)
			{
				m.SendLocalizedMessage(502131); // Your guild has disbanded.

				if (m is PlayerMobile)
				{
					((PlayerMobile)m).GuildRank = RankDefinition.Lowest;
				}

				m.Guild = null;
			}

			m_Members.Clear();

			for (var i = m_Allies.Count - 1; i >= 0; --i)
			{
				if (i < m_Allies.Count)
				{
					RemoveAlly(m_Allies[i]);
				}
			}

			for (var i = m_Enemies.Count - 1; i >= 0; --i)
			{
				if (i < m_Enemies.Count)
				{
					RemoveEnemy(m_Enemies[i]);
				}
			}

			if (!NewGuildSystem && m_Guildstone != null)
			{
				m_Guildstone.Delete();
			}

			m_Guildstone = null;

			CheckExpiredWars();

			Alliance = null;
		}

		#region Is<something>(...)
		public bool IsMember(Mobile m)
		{
			return m_Members.Contains(m);
		}

		public bool IsAlly(Guild g)
		{
			if (NewGuildSystem)
			{
				return (Alliance != null && Alliance.IsMember(this) && Alliance.IsMember(g));
			}

			return m_Allies.Contains(g);
		}

		public bool IsEnemy(Guild g)
		{
			if (Type != GuildType.Regular && g.Type != GuildType.Regular && Type != g.Type)
			{
				return true;
			}

			return IsWar(g);
		}

		public bool IsWar(Guild g)
		{
			if (g == null)
			{
				return false;
			}

			if (NewGuildSystem)
			{
				var guild = GetAllianceLeader(this);
				var otherGuild = GetAllianceLeader(g);

				if (guild.FindActiveWar(otherGuild) != null)
				{
					return true;
				}

				return false;
			}

			return m_Enemies.Contains(g);
		}
		#endregion

		#region Serialization
		public override void Serialize(GenericWriter writer)
		{
			if (LastFealty + TimeSpan.FromDays(1.0) < DateTime.UtcNow)
			{
				CalculateGuildmaster();
			}

			CheckExpiredWars();

			if (Alliance != null)
			{
				Alliance.CheckLeader();
			}

			writer.Write(5);//version

			#region War Serialization
			writer.Write(m_PendingWars.Count);

			for (var i = 0; i < m_PendingWars.Count; i++)
			{
				m_PendingWars[i].Serialize(writer);
			}

			writer.Write(m_AcceptedWars.Count);

			for (var i = 0; i < m_AcceptedWars.Count; i++)
			{
				m_AcceptedWars[i].Serialize(writer);
			}
			#endregion

			#region Alliances

			var isAllianceLeader = (m_AllianceLeader == null && m_AllianceInfo != null);
			writer.Write(isAllianceLeader);

			if (isAllianceLeader)
			{
				m_AllianceInfo.Serialize(writer);
			}
			else
			{
				writer.Write(m_AllianceLeader);
			}

			#endregion

			//

			writer.WriteGuildList(m_AllyDeclarations, true);
			writer.WriteGuildList(m_AllyInvitations, true);

			writer.Write(m_TypeLastChange);

			writer.Write((int)m_Type);

			writer.Write(m_LastFealty);

			writer.Write(m_Leader);
			writer.Write(m_Name);
			writer.Write(m_Abbreviation);

			writer.WriteGuildList<Guild>(m_Allies, true);
			writer.WriteGuildList<Guild>(m_Enemies, true);
			writer.WriteGuildList(m_WarDeclarations, true);
			writer.WriteGuildList(m_WarInvitations, true);

			writer.Write(m_Members, true);
			writer.Write(m_Candidates, true);
			writer.Write(m_Accepted, true);

			writer.Write(m_Guildstone);
			writer.Write(m_Teleporter);

			writer.Write(m_Charter);
			writer.Write(m_Website);
		}

		public override void Deserialize(GenericReader reader)
		{
			var version = reader.ReadInt();

			switch (version)
			{
				case 5:
					{
						var count = reader.ReadInt();

						m_PendingWars = new List<WarDeclaration>();
						for (var i = 0; i < count; i++)
						{
							m_PendingWars.Add(new WarDeclaration(reader));
						}

						count = reader.ReadInt();
						m_AcceptedWars = new List<WarDeclaration>();
						for (var i = 0; i < count; i++)
						{
							m_AcceptedWars.Add(new WarDeclaration(reader));
						}

						var isAllianceLeader = reader.ReadBool();

						if (isAllianceLeader)
						{
							m_AllianceInfo = new AllianceInfo(reader);
						}
						else
						{
							m_AllianceLeader = reader.ReadGuild() as Guild;
						}

						goto case 4;
					}
				case 4:
					{
						m_AllyDeclarations = reader.ReadStrongGuildList<Guild>();
						m_AllyInvitations = reader.ReadStrongGuildList<Guild>();

						goto case 3;
					}
				case 3:
					{
						m_TypeLastChange = reader.ReadDateTime();

						goto case 2;
					}
				case 2:
					{
						m_Type = (GuildType)reader.ReadInt();

						goto case 1;
					}
				case 1:
					{
						m_LastFealty = reader.ReadDateTime();

						goto case 0;
					}
				case 0:
					{
						m_Leader = reader.ReadMobile();

						if (m_Leader is PlayerMobile)
						{
							((PlayerMobile)m_Leader).GuildRank = RankDefinition.Leader;
						}

						m_Name = reader.ReadString();
						m_Abbreviation = reader.ReadString();

						m_Allies = reader.ReadStrongGuildList<Guild>();
						m_Enemies = reader.ReadStrongGuildList<Guild>();
						m_WarDeclarations = reader.ReadStrongGuildList<Guild>();
						m_WarInvitations = reader.ReadStrongGuildList<Guild>();

						m_Members = reader.ReadStrongMobileList();
						m_Candidates = reader.ReadStrongMobileList();
						m_Accepted = reader.ReadStrongMobileList();

						m_Guildstone = reader.ReadItem();
						m_Teleporter = reader.ReadItem();

						m_Charter = reader.ReadString();
						m_Website = reader.ReadString();

						break;
					}
			}

			if (m_AllyDeclarations == null)
			{
				m_AllyDeclarations = new List<Guild>();
			}

			if (m_AllyInvitations == null)
			{
				m_AllyInvitations = new List<Guild>();
			}

			if (m_AcceptedWars == null)
			{
				m_AcceptedWars = new List<WarDeclaration>();
			}

			if (m_PendingWars == null)
			{
				m_PendingWars = new List<WarDeclaration>();
			}


			/*
			if ( ( !NewGuildSystem && m_Guildstone == null )|| m_Members.Count == 0 )
				Disband();
			*/

			Timer.DelayCall(TimeSpan.Zero, VerifyGuild_Callback);
		}

		private void VerifyGuild_Callback()
		{
			if ((!NewGuildSystem && m_Guildstone == null) || m_Members.Count == 0)
			{
				Disband();
			}

			CheckExpiredWars();

			var alliance = Alliance;

			if (alliance != null)
			{
				alliance.CheckLeader();
			}

			alliance = Alliance;   //CheckLeader could possibly change the value of this.Alliance

			if (alliance != null && !alliance.IsMember(this) && !alliance.IsPendingMember(this))    //This block is there to fix a bug in the code in an older version.  
			{
				Alliance = null;   //Will call Alliance.RemoveGuild which will set it null & perform all the pertient checks as far as alliacne disbanding
			}
		}

		#endregion

		#region Add/Remove Member/Old Ally/Old Enemy
		public void AddMember(Mobile m)
		{
			if (!m_Members.Contains(m))
			{
				if (m.Guild != null && m.Guild != this)
				{
					((Guild)m.Guild).RemoveMember(m);
				}

				m_Members.Add(m);
				m.Guild = this;

				if (!NewGuildSystem)
				{
					m.GuildFealty = m_Leader;
				}
				else
				{
					m.GuildFealty = null;
				}

				if (m is PlayerMobile)
				{
					((PlayerMobile)m).GuildRank = RankDefinition.Lowest;
				}

				var guild = m.Guild as Guild;

				if (guild != null)
				{
					guild.InvalidateWarNotoriety();
				}
			}
		}

		public void RemoveMember(Mobile m)
		{
			RemoveMember(m, 1018028); // You have been dismissed from your guild.
		}
		public void RemoveMember(Mobile m, int message)
		{
			if (m_Members.Contains(m))
			{
				m_Members.Remove(m);

				var guild = m.Guild as Guild;

				m.Guild = null;

				if (m is PlayerMobile)
				{
					((PlayerMobile)m).GuildRank = RankDefinition.Lowest;
				}

				if (message > 0)
				{
					m.SendLocalizedMessage(message);
				}

				if (m == m_Leader)
				{
					CalculateGuildmaster();

					if (m_Leader == null)
					{
						Disband();
					}
				}

				if (m_Members.Count == 0)
				{
					Disband();
				}

				if (guild != null)
				{
					guild.InvalidateWarNotoriety();
				}

				m.Delta(MobileDelta.Noto);
			}
		}

		public void AddAlly(Guild g)
		{
			if (!m_Allies.Contains(g))
			{
				m_Allies.Add(g);

				g.AddAlly(this);
			}
		}

		public void RemoveAlly(Guild g)
		{
			if (m_Allies.Contains(g))
			{
				m_Allies.Remove(g);

				g.RemoveAlly(this);
			}
		}

		public void AddEnemy(Guild g)
		{
			if (!m_Enemies.Contains(g))
			{
				m_Enemies.Add(g);

				g.AddEnemy(this);
			}
		}

		public void RemoveEnemy(Guild g)
		{
			if (m_Enemies != null && m_Enemies.Contains(g))
			{
				m_Enemies.Remove(g);

				g.RemoveEnemy(this);
			}
		}

		#endregion

		#region Guild[Text]Message(...)
		public void GuildMessage(int num, bool append, string format, params object[] args)
		{
			GuildMessage(num, append, String.Format(format, args));
		}
		public void GuildMessage(int number)
		{
			for (var i = 0; i < m_Members.Count; ++i)
			{
				m_Members[i].SendLocalizedMessage(number);
			}
		}
		public void GuildMessage(int number, string args)
		{
			GuildMessage(number, args, 0x3B2);
		}
		public void GuildMessage(int number, string args, int hue)
		{
			for (var i = 0; i < m_Members.Count; ++i)
			{
				m_Members[i].SendLocalizedMessage(number, args, hue);
			}
		}
		public void GuildMessage(int number, bool append, string affix)
		{
			GuildMessage(number, append, affix, "", 0x3B2);
		}
		public void GuildMessage(int number, bool append, string affix, string args)
		{
			GuildMessage(number, append, affix, args, 0x3B2);
		}
		public void GuildMessage(int number, bool append, string affix, string args, int hue)
		{
			for (var i = 0; i < m_Members.Count; ++i)
			{
				m_Members[i].SendLocalizedMessage(number, append, affix, args, hue);
			}
		}

		public void GuildTextMessage(string text)
		{
			GuildTextMessage(0x3B2, text);
		}
		public void GuildTextMessage(string format, params object[] args)
		{
			GuildTextMessage(0x3B2, String.Format(format, args));
		}
		public void GuildTextMessage(int hue, string text)
		{
			for (var i = 0; i < m_Members.Count; ++i)
			{
				m_Members[i].SendMessage(hue, text);
			}
		}
		public void GuildTextMessage(int hue, string format, params object[] args)
		{
			GuildTextMessage(hue, String.Format(format, args));
		}

		public void GuildChat(Mobile from, int hue, string text)
		{
			Packet p = null;
			for (var i = 0; i < m_Members.Count; i++)
			{
				var m = m_Members[i];

				var state = m.NetState;

				if (state != null)
				{
					if (p == null)
					{
						p = Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Guild, hue, 3, from.Language, from.Name, text));
					}

					state.Send(p);
				}
			}

			Packet.Release(p);
		}

		public void GuildChat(Mobile from, string text)
		{
			var pm = from as PlayerMobile;

			GuildChat(from, (pm == null) ? 0x3B2 : pm.GuildMessageHue, text);
		}
		#endregion

		#region Voting
		public bool CanVote(Mobile m)
		{
			if (NewGuildSystem)
			{
				var pm = m as PlayerMobile;
				if (pm == null || !pm.GuildRank.GetFlag(RankFlags.CanVote))
				{
					return false;
				}
			}

			return (m != null && !m.Deleted && m.Guild == this);
		}
		public bool CanBeVotedFor(Mobile m)
		{
			if (NewGuildSystem)
			{
				var pm = m as PlayerMobile;
				if (pm == null || pm.LastOnline + InactiveTime < DateTime.UtcNow)
				{
					return false;
				}
			}

			return (m != null && !m.Deleted && m.Guild == this);
		}

		public void CalculateGuildmaster()
		{
			var votes = new Dictionary<Mobile, int>();

			var votingMembers = 0;

			for (var i = 0; m_Members != null && i < m_Members.Count; ++i)
			{
				var memb = m_Members[i];

				if (!CanVote(memb))
				{
					continue;
				}

				var m = memb.GuildFealty;

				if (!CanBeVotedFor(m))
				{
					if (m_Leader != null && !m_Leader.Deleted && m_Leader.Guild == this)
					{
						m = m_Leader;
					}
					else
					{
						m = memb;
					}
				}

				if (m == null)
				{
					continue;
				}

				int v;

				if (!votes.TryGetValue(m, out v))
				{
					votes[m] = 1;
				}
				else
				{
					votes[m] = v + 1;
				}

				votingMembers++;
			}

			Mobile winner = null;
			var highVotes = 0;

			foreach (var kvp in votes)
			{
				var m = kvp.Key;
				var val = kvp.Value;

				if (winner == null || val > highVotes)
				{
					winner = m;
					highVotes = val;
				}
			}

			if (NewGuildSystem && (highVotes * 100) / Math.Max(votingMembers, 1) < MajorityPercentage && m_Leader != null && winner != m_Leader && !m_Leader.Deleted && m_Leader.Guild == this)
			{
				winner = m_Leader;
			}

			if (m_Leader != winner && winner != null)
			{
				GuildMessage(1018015, true, winner.Name); // Guild Message: Guildmaster changed to:
			}

			Leader = winner;
			m_LastFealty = DateTime.UtcNow;
		}

		#endregion

		#region Getters & Setters
		[CommandProperty(AccessLevel.GameMaster)]
		public Item Guildstone
		{
			get => m_Guildstone;
			set => m_Guildstone = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Item Teleporter
		{
			get => m_Teleporter;
			set => m_Teleporter = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override string Name
		{
			get => m_Name;
			set
			{
				m_Name = value;

				InvalidateMemberProperties(true);

				if (m_Guildstone != null)
				{
					m_Guildstone.InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Website
		{
			get => m_Website;
			set => m_Website = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override string Abbreviation
		{
			get => m_Abbreviation;
			set
			{
				m_Abbreviation = value;

				InvalidateMemberProperties(true);

				if (m_Guildstone != null)
				{
					m_Guildstone.InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Charter
		{
			get => m_Charter;
			set => m_Charter = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override GuildType Type
		{
			get => OrderChaos ? m_Type : GuildType.Regular;
			set
			{
				if (m_Type != value)
				{
					m_Type = value;
					m_TypeLastChange = DateTime.UtcNow;

					InvalidateMemberProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastFealty
		{
			get => m_LastFealty;
			set => m_LastFealty = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime TypeLastChange => m_TypeLastChange;

		public List<Guild> Allies => m_Allies;

		public List<Guild> Enemies => m_Enemies;

		public List<Guild> AllyDeclarations => m_AllyDeclarations;

		public List<Guild> AllyInvitations => m_AllyInvitations;

		public List<Guild> WarDeclarations => m_WarDeclarations;

		public List<Guild> WarInvitations => m_WarInvitations;

		public List<Mobile> Candidates => m_Candidates;

		public List<Mobile> Accepted => m_Accepted;

		public List<Mobile> Members => m_Members;

		#endregion
	}

	/// Guild Ranks
	[Flags]
	public enum RankFlags
	{
		None = 0x00000000,
		CanInvitePlayer = 0x00000001,
		AccessGuildItems = 0x00000002,
		RemoveLowestRank = 0x00000004,
		RemovePlayers = 0x00000008,
		CanPromoteDemote = 0x00000010,
		ControlWarStatus = 0x00000020,
		AllianceControl = 0x00000040,
		CanSetGuildTitle = 0x00000080,
		CanVote = 0x00000100,

		All = Member | CanInvitePlayer | RemovePlayers | CanPromoteDemote | ControlWarStatus | AllianceControl | CanSetGuildTitle,
		Member = RemoveLowestRank | AccessGuildItems | CanVote
	}

	public class RankDefinition
	{
		public static RankDefinition[] Ranks = new RankDefinition[]
			{
				new RankDefinition( 1062963, 0, RankFlags.None ),	//Ronin
				new RankDefinition( 1062962, 1, RankFlags.Member ),	//Member
				new RankDefinition( 1062961, 2, RankFlags.Member | RankFlags.RemovePlayers | RankFlags.CanInvitePlayer | RankFlags.CanSetGuildTitle | RankFlags.CanPromoteDemote ),	//Emmissary
				new RankDefinition( 1062960, 3, RankFlags.Member | RankFlags.ControlWarStatus ),	//Warlord
				new RankDefinition( 1062959, 4, RankFlags.All )	//Leader
			};
		public static RankDefinition Leader => Ranks[4];
		public static RankDefinition Member => Ranks[1];
		public static RankDefinition Lowest => Ranks[0];

		private readonly TextDefinition m_Name;
		private readonly int m_Rank;
		private RankFlags m_Flags;

		public TextDefinition Name => m_Name;
		public int Rank => m_Rank;
		public RankFlags Flags => m_Flags;

		public RankDefinition(TextDefinition name, int rank, RankFlags flags)
		{
			m_Name = name;
			m_Rank = rank;
			m_Flags = flags;
		}

		public bool GetFlag(RankFlags flag)
		{
			return ((m_Flags & flag) != 0);
		}

		public void SetFlag(RankFlags flag, bool value)
		{
			if (value)
			{
				m_Flags |= flag;
			}
			else
			{
				m_Flags &= ~flag;
			}
		}
	}

	/// Guild Alliances
	public class AllianceInfo
	{
		private static readonly Dictionary<string, AllianceInfo> m_Alliances = new Dictionary<string, AllianceInfo>();

		public static Dictionary<string, AllianceInfo> Alliances => m_Alliances;

		private readonly string m_Name;
		private Guild m_Leader;
		private readonly List<Guild> m_Members;
		private readonly List<Guild> m_PendingMembers;

		public string Name => m_Name;

		public void CalculateAllianceLeader()
		{
			m_Leader = ((m_Members.Count >= 2) ? m_Members[Utility.Random(m_Members.Count)] : null);
		}

		public void CheckLeader()
		{
			if (m_Leader == null || m_Leader.Disbanded)
			{
				CalculateAllianceLeader();

				if (m_Leader == null)
				{
					Disband();
				}
			}
		}

		public Guild Leader
		{
			get
			{
				CheckLeader();
				return m_Leader;
			}
			set
			{
				if (m_Leader != value && value != null)
				{
					AllianceMessage(1070765, value.Name); // Your Alliance is now led by ~1_GUILDNAME~
				}

				m_Leader = value;

				if (m_Leader == null)
				{
					CalculateAllianceLeader();
				}
			}
		}

		public bool IsPendingMember(Guild g)
		{
			if (g.Alliance != this)
			{
				return false;
			}

			return m_PendingMembers.Contains(g);
		}

		public bool IsMember(Guild g)
		{
			if (g.Alliance != this)
			{
				return false;
			}

			return m_Members.Contains(g);
		}

		public AllianceInfo(Guild leader, string name, Guild partner)
		{
			m_Leader = leader;
			m_Name = name;

			m_Members = new List<Guild>();
			m_PendingMembers = new List<Guild>();

			leader.Alliance = this;
			partner.Alliance = this;

			if (!m_Alliances.ContainsKey(m_Name.ToLower()))
			{
				m_Alliances.Add(m_Name.ToLower(), this);
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(0);   //Version

			writer.Write(m_Name);
			writer.Write(m_Leader);

			writer.WriteGuildList(m_Members, true);
			writer.WriteGuildList(m_PendingMembers, true);

			if (!m_Alliances.ContainsKey(m_Name.ToLower()))
			{
				m_Alliances.Add(m_Name.ToLower(), this);
			}
		}

		public AllianceInfo(GenericReader reader)
		{
			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Name = reader.ReadString();
						m_Leader = reader.ReadGuild() as Guild;

						m_Members = reader.ReadStrongGuildList<Guild>();
						m_PendingMembers = reader.ReadStrongGuildList<Guild>();

						break;
					}
			}
		}

		public void AddPendingGuild(Guild g)
		{
			if (g.Alliance != this || m_PendingMembers.Contains(g) || m_Members.Contains(g))
			{
				return;
			}

			m_PendingMembers.Add(g);
		}

		public void TurnToMember(Guild g)
		{
			if (g.Alliance != this || !m_PendingMembers.Contains(g) || m_Members.Contains(g))
			{
				return;
			}

			g.GuildMessage(1070760, Name); // Your Guild has joined the ~1_ALLIANCENAME~ Alliance.
			AllianceMessage(1070761, g.Name); // A new Guild has joined your Alliance: ~1_GUILDNAME~

			m_PendingMembers.Remove(g);
			m_Members.Add(g);
			g.Alliance.InvalidateMemberProperties();
		}

		public void RemoveGuild(Guild g)
		{
			if (m_PendingMembers.Contains(g))
			{
				m_PendingMembers.Remove(g);
			}

			if (m_Members.Contains(g))  //Sanity, just incase someone with a custom script adds a character to BOTH arrays
			{
				m_Members.Remove(g);
				g.InvalidateMemberProperties();

				g.GuildMessage(1070763, Name); // Your Guild has been removed from the ~1_ALLIANCENAME~ Alliance.
				AllianceMessage(1070764, g.Name); // A Guild has left your Alliance: ~1_GUILDNAME~
			}

			//g.Alliance = null;	//NO G.Alliance call here.  Set the Guild's Alliance to null, if you JUST use RemoveGuild, it removes it from the alliance, but doesn't remove the link from the guild to the alliance.  setting g.Alliance will call this method.
			//to check on OSI: have 3 guilds, make 2 of them a member, one pending.  remove one of the memebers.  alliance still exist?
			//ANSWER: NO

			if (g == m_Leader)
			{
				CalculateAllianceLeader();

				/*
				if( m_Leader == null ) //only when m_members.count < 2
					Disband();
				else
					AllianceMessage( 1070765, m_Leader.Name ); // Your Alliance is now led by ~1_GUILDNAME~
				*/
			}

			if (m_Members.Count < 2)
			{
				Disband();
			}
		}

		public void Disband()
		{
			AllianceMessage(1070762); // Your Alliance has dissolved.

			for (var i = 0; i < m_PendingMembers.Count; i++)
			{
				m_PendingMembers[i].Alliance = null;
			}

			for (var i = 0; i < m_Members.Count; i++)
			{
				m_Members[i].Alliance = null;
			}

			AllianceInfo aInfo = null;

			m_Alliances.TryGetValue(m_Name.ToLower(), out aInfo);

			if (aInfo == this)
			{
				m_Alliances.Remove(m_Name.ToLower());
			}
		}

		public void InvalidateMemberProperties()
		{
			InvalidateMemberProperties(false);
		}

		public void InvalidateMemberProperties(bool onlyOPL)
		{
			for (var i = 0; i < m_Members.Count; i++)
			{
				var g = m_Members[i];

				g.InvalidateMemberProperties(onlyOPL);
			}
		}

		public void InvalidateMemberNotoriety()
		{
			for (var i = 0; i < m_Members.Count; i++)
			{
				m_Members[i].InvalidateMemberNotoriety();
			}
		}

		#region Alliance[Text]Message(...)
		public void AllianceMessage(int num, bool append, string format, params object[] args)
		{
			AllianceMessage(num, append, String.Format(format, args));
		}
		public void AllianceMessage(int number)
		{
			for (var i = 0; i < m_Members.Count; ++i)
			{
				m_Members[i].GuildMessage(number);
			}
		}
		public void AllianceMessage(int number, string args)
		{
			AllianceMessage(number, args, 0x3B2);
		}
		public void AllianceMessage(int number, string args, int hue)
		{
			for (var i = 0; i < m_Members.Count; ++i)
			{
				m_Members[i].GuildMessage(number, args, hue);
			}
		}
		public void AllianceMessage(int number, bool append, string affix)
		{
			AllianceMessage(number, append, affix, "", 0x3B2);
		}
		public void AllianceMessage(int number, bool append, string affix, string args)
		{
			AllianceMessage(number, append, affix, args, 0x3B2);
		}
		public void AllianceMessage(int number, bool append, string affix, string args, int hue)
		{
			for (var i = 0; i < m_Members.Count; ++i)
			{
				m_Members[i].GuildMessage(number, append, affix, args, hue);
			}
		}

		public void AllianceTextMessage(string text)
		{
			AllianceTextMessage(0x3B2, text);
		}
		public void AllianceTextMessage(string format, params object[] args)
		{
			AllianceTextMessage(0x3B2, String.Format(format, args));
		}
		public void AllianceTextMessage(int hue, string text)
		{
			for (var i = 0; i < m_Members.Count; ++i)
			{
				m_Members[i].GuildTextMessage(hue, text);
			}
		}
		public void AllianceTextMessage(int hue, string format, params object[] args)
		{
			AllianceTextMessage(hue, String.Format(format, args));
		}

		public void AllianceChat(Mobile from, int hue, string text)
		{
			Packet p = null;
			for (var i = 0; i < m_Members.Count; i++)
			{
				var g = m_Members[i];

				for (var j = 0; j < g.Members.Count; j++)
				{
					var m = g.Members[j];

					var state = m.NetState;

					if (state != null)
					{
						if (p == null)
						{
							p = Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Alliance, hue, 3, from.Language, from.Name, text));
						}

						state.Send(p);
					}
				}
			}

			Packet.Release(p);
		}

		public void AllianceChat(Mobile from, string text)
		{
			var pm = from as PlayerMobile;

			AllianceChat(from, (pm == null) ? 0x3B2 : pm.AllianceMessageHue, text);
		}
		#endregion

		public class AllianceRosterGump : GuildDiplomacyGump
		{
			protected override bool AllowAdvancedSearch => false;

			private readonly AllianceInfo m_Alliance;

			public AllianceRosterGump(PlayerMobile pm, Guild g, AllianceInfo alliance) : base(pm, g, true, "", 0, alliance.m_Members, alliance.Name)
			{
				m_Alliance = alliance;
			}

			public AllianceRosterGump(PlayerMobile pm, Guild g, AllianceInfo alliance, IComparer<Guild> currentComparer, bool ascending, string filter, int startNumber) : base(pm, g, currentComparer, ascending, filter, startNumber, alliance.m_Members, alliance.Name)
			{
				m_Alliance = alliance;
			}

			public override Gump GetResentGump(PlayerMobile pm, Guild g, IComparer<Guild> comparer, bool ascending, string filter, int startNumber)
			{
				return new AllianceRosterGump(pm, g, m_Alliance, comparer, ascending, filter, startNumber);
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (info.ButtonID != 8) //So that they can't get to the AdvancedSearch button
				{
					base.OnResponse(sender, info);
				}
			}
		}
	}

	/// Guild Wars
	public enum WarStatus
	{
		InProgress = -1,
		Win,
		Lose,
		Draw,
		Pending
	}

	public class WarDeclaration
	{
		private int m_Kills;
		private int m_MaxKills;

		private TimeSpan m_WarLength;
		private DateTime m_WarBeginning;

		private readonly Guild m_Guild;
		private readonly Guild m_Opponent;

		private bool m_WarRequester;

		public int Kills
		{
			get => m_Kills;
			set => m_Kills = value;
		}
		public int MaxKills
		{
			get => m_MaxKills;
			set => m_MaxKills = value;
		}
		public TimeSpan WarLength
		{
			get => m_WarLength;
			set => m_WarLength = value;
		}
		public Guild Opponent => m_Opponent;
		public Guild Guild => m_Guild;
		public DateTime WarBeginning
		{
			get => m_WarBeginning;
			set => m_WarBeginning = value;
		}
		public bool WarRequester
		{
			get => m_WarRequester;
			set => m_WarRequester = value;
		}

		public WarDeclaration(Guild g, Guild opponent, int maxKills, TimeSpan warLength, bool warRequester)
		{
			m_Guild = g;
			m_MaxKills = maxKills;
			m_Opponent = opponent;
			m_WarLength = warLength;
			m_WarRequester = warRequester;
		}

		public WarDeclaration(GenericReader reader)
		{
			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Kills = reader.ReadInt();
						m_MaxKills = reader.ReadInt();

						m_WarLength = reader.ReadTimeSpan();
						m_WarBeginning = reader.ReadDateTime();

						m_Guild = reader.ReadGuild() as Guild;
						m_Opponent = reader.ReadGuild() as Guild;

						m_WarRequester = reader.ReadBool();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(0);   //version

			writer.Write(m_Kills);
			writer.Write(m_MaxKills);

			writer.Write(m_WarLength);
			writer.Write(m_WarBeginning);

			writer.Write(m_Guild);
			writer.Write(m_Opponent);

			writer.Write(m_WarRequester);
		}

		public WarStatus Status
		{
			get
			{
				if (m_Opponent == null || m_Opponent.Disbanded)
				{
					return WarStatus.Win;
				}

				if (m_Guild == null || m_Guild.Disbanded)
				{
					return WarStatus.Lose;
				}

				var w = m_Opponent.FindActiveWar(m_Guild);

				if (m_Opponent.FindPendingWar(m_Guild) != null && m_Guild.FindPendingWar(m_Opponent) != null)
				{
					return WarStatus.Pending;
				}

				if (w == null)
				{
					return WarStatus.Win;
				}

				if (m_WarLength != TimeSpan.Zero && (m_WarBeginning + m_WarLength) < DateTime.UtcNow)
				{
					if (m_Kills > w.m_Kills)
					{
						return WarStatus.Win;
					}
					else if (m_Kills < w.m_Kills)
					{
						return WarStatus.Lose;
					}
					else
					{
						return WarStatus.Draw;
					}
				}
				else if (m_MaxKills > 0)
				{
					if (m_Kills >= m_MaxKills)
					{
						return WarStatus.Win;
					}
					else if (w.m_Kills >= w.MaxKills)
					{
						return WarStatus.Lose;
					}
				}

				return WarStatus.InProgress;
			}
		}
	}

	public class WarTimer : Timer
	{
		private static readonly TimeSpan InternalDelay = TimeSpan.FromMinutes(1.0);

		public static void Initialize()
		{
			if (Guild.NewGuildSystem)
			{
				new WarTimer().Start();
			}
		}

		public WarTimer() : base(InternalDelay, InternalDelay)
		{
			Priority = TimerPriority.FiveSeconds;
		}

		protected override void OnTick()
		{
			foreach (Guild g in Guild.List.Values)
			{
				g.CheckExpiredWars();
			}
		}
	}

	#region Paperdoll Guild System

	public abstract class BaseGuildGump : Gump
	{
		private readonly Guild m_Guild;
		private readonly PlayerMobile m_Player;

		protected Guild guild => m_Guild;
		protected PlayerMobile player => m_Player;

		public BaseGuildGump(PlayerMobile pm, Guild g) : this(pm, g, 10, 10)
		{
		}

		public BaseGuildGump(PlayerMobile pm, Guild g, int x, int y) : base(x, y)
		{
			m_Guild = g;
			m_Player = pm;

			pm.CloseGump(typeof(BaseGuildGump));
		}

		//There's prolly a way to have all the vars set of inherited classes before something is called in the Ctor... but... I can't think of it right now, and I can't use Timer.DelayCall here :<

		public virtual void PopulateGump()
		{
			AddPage(0);

			AddBackground(0, 0, 600, 440, 0x24AE);
			AddBackground(66, 40, 150, 26, 0x2486);
			AddButton(71, 45, 0x845, 0x846, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(96, 43, 110, 26, 1063014, 0x0, false, false); // My Guild
			AddBackground(236, 40, 150, 26, 0x2486);
			AddButton(241, 45, 0x845, 0x846, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(266, 43, 110, 26, 1062974, 0x0, false, false); // Guild Roster
			AddBackground(401, 40, 150, 26, 0x2486);
			AddButton(406, 45, 0x845, 0x846, 3, GumpButtonType.Reply, 0);
			AddHtmlLocalized(431, 43, 110, 26, 1062978, 0x0, false, false); // Diplomacy
			AddPage(1);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var pm = sender.Mobile as PlayerMobile;

			if (!IsMember(pm, guild))
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 1:
					{
						pm.SendGump(new GuildInfoGump(pm, guild));
						break;
					}
				case 2:
					{
						pm.SendGump(new GuildRosterGump(pm, guild));
						break;
					}
				case 3:
					{
						pm.SendGump(new GuildDiplomacyGump(pm, guild));
						break;
					}
			}
		}

		public static bool IsLeader(Mobile m, Guild g)
		{
			return !(m.Deleted || g.Disbanded || !(m is PlayerMobile) || (m.AccessLevel < AccessLevel.GameMaster && g.Leader != m));
		}

		public static bool IsMember(Mobile m, Guild g)
		{
			return !(m.Deleted || g.Disbanded || !(m is PlayerMobile) || (m.AccessLevel < AccessLevel.GameMaster && !g.IsMember(m)));
		}

		public static bool CheckProfanity(string s)
		{
			return CheckProfanity(s, 50);
		}
		public static bool CheckProfanity(string s, int maxLength)
		{
			//return NameVerification.Validate( s, 1, 50, true, true, false, int.MaxValue, ProfanityProtection.Exceptions, ProfanityProtection.Disallowed, ProfanityProtection.StartDisallowed );	//What am I doing wrong, this still allows chars like the <3 symbol... 3 AM.  someone change this to use this

			//With testing on OSI, Guild stuff seems to follow a 'simpler' method of profanity protection
			if (s.Length < 1 || s.Length > maxLength)
			{
				return false;
			}

			var exceptions = ProfanityProtection.Exceptions;

			s = s.ToLower();

			for (var i = 0; i < s.Length; ++i)
			{
				var c = s[i];

				if ((c < 'a' || c > 'z') && (c < '0' || c > '9'))
				{
					var except = false;

					for (var j = 0; !except && j < exceptions.Length; j++)
					{
						if (c == exceptions[j])
						{
							except = true;
						}
					}

					if (!except)
					{
						return false;
					}
				}
			}

			var disallowed = ProfanityProtection.Disallowed;

			for (var i = 0; i < disallowed.Length; i++)
			{
				if (s.IndexOf(disallowed[i]) != -1)
				{
					return false;
				}
			}

			return true;
		}

		public void AddHtmlText(int x, int y, int width, int height, TextDefinition text, bool back, bool scroll)
		{
			if (text != null && text.Number > 0)
			{
				AddHtmlLocalized(x, y, width, height, text.Number, back, scroll);
			}
			else if (text != null && text.String != null)
			{
				AddHtml(x, y, width, height, text.String, back, scroll);
			}
		}

		public static string Color(string text, int color)
		{
			return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
		}
	}

	public abstract class BaseGuildListGump<T> : BaseGuildGump
	{
		private List<T> m_List;
		private readonly IComparer<T> m_Comparer;
		private readonly InfoField<T>[] m_Fields;
		private bool m_Ascending;
		private readonly string m_Filter;
		private int m_StartNumber;

		private const int itemsPerPage = 8;

		public BaseGuildListGump(PlayerMobile pm, Guild g, List<T> list, IComparer<T> currentComparer, bool ascending, string filter, int startNumber, InfoField<T>[] fields)
			: base(pm, g)
		{
			m_Filter = filter.Trim();

			m_Comparer = currentComparer;
			m_Fields = fields;
			m_Ascending = ascending;
			m_StartNumber = startNumber;
			m_List = list;
		}

		public virtual bool WillFilter => (m_Filter.Length >= 0);

		public override void PopulateGump()
		{
			base.PopulateGump();

			var list = m_List;
			if (WillFilter)
			{
				m_List = new List<T>();
				for (var i = 0; i < list.Count; i++)
				{
					if (!IsFiltered(list[i], m_Filter))
					{
						m_List.Add(list[i]);
					}
				}
			}
			else
			{
				m_List = new List<T>(list);
			}

			m_List.Sort(m_Comparer);
			m_StartNumber = Math.Max(Math.Min(m_StartNumber, m_List.Count - 1), 0);



			AddBackground(130, 75, 385, 30, 0xBB8);
			AddTextEntry(135, 80, 375, 30, 0x481, 1, m_Filter);
			AddButton(520, 75, 0x867, 0x868, 5, GumpButtonType.Reply, 0);   //Filter Button

			var width = 0;
			for (var i = 0; i < m_Fields.Length; i++)
			{
				var f = m_Fields[i];

				AddImageTiled(65 + width, 110, f.Width + 10, 26, 0xA40);
				AddImageTiled(67 + width, 112, f.Width + 6, 22, 0xBBC);
				AddHtmlText(70 + width, 113, f.Width, 20, f.Name, false, false);

				var isComparer = (m_Fields[i].Comparer.GetType() == m_Comparer.GetType());

				var ButtonID = (isComparer) ? (m_Ascending ? 0x983 : 0x985) : 0x2716;

				AddButton(59 + width + f.Width, 117, ButtonID, ButtonID + (isComparer ? 1 : 0), 100 + i, GumpButtonType.Reply, 0);

				width += (f.Width + 12);
			}

			if (m_StartNumber <= 0)
			{
				AddButton(65, 80, 0x15E3, 0x15E7, 0, GumpButtonType.Page, 0);
			}
			else
			{
				AddButton(65, 80, 0x15E3, 0x15E7, 6, GumpButtonType.Reply, 0);  // Back
			}

			if (m_StartNumber + itemsPerPage > m_List.Count)
			{
				AddButton(95, 80, 0x15E1, 0x15E5, 0, GumpButtonType.Page, 0);
			}
			else
			{
				AddButton(95, 80, 0x15E1, 0x15E5, 7, GumpButtonType.Reply, 0);  // Forward
			}

			var itemNumber = 0;

			if (m_Ascending)
			{
				for (var i = m_StartNumber; i < m_StartNumber + itemsPerPage && i < m_List.Count; i++)
				{
					DrawEntry(m_List[i], i, itemNumber++);
				}
			}
			else //descending, go from bottom of list to the top
			{
				for (var i = m_List.Count - 1 - m_StartNumber; i >= 0 && i >= (m_List.Count - itemsPerPage - m_StartNumber); i--)
				{
					DrawEntry(m_List[i], i, itemNumber++);
				}
			}

			DrawEndingEntry(itemNumber);
		}

		public virtual void DrawEndingEntry(int itemNumber)
		{
		}

		public virtual bool HasRelationship(T o)
		{
			return false;
		}

		public virtual void DrawEntry(T o, int index, int itemNumber)
		{
			var width = 0;
			for (var j = 0; j < m_Fields.Length; j++)
			{
				var f = m_Fields[j];

				AddImageTiled(65 + width, 138 + itemNumber * 28, f.Width + 10, 26, 0xA40);
				AddImageTiled(67 + width, 140 + itemNumber * 28, f.Width + 6, 22, 0xBBC);
				AddHtmlText(70 + width, 141 + itemNumber * 28, f.Width, 20, GetValuesFor(o, m_Fields.Length)[j], false, false);

				width += (f.Width + 12);
			}

			if (HasRelationship(o))
			{
				AddButton(40, 143 + itemNumber * 28, 0x8AF, 0x8AF, 200 + index, GumpButtonType.Reply, 0);   //Info Button
			}
			else
			{
				AddButton(40, 143 + itemNumber * 28, 0x4B9, 0x4BA, 200 + index, GumpButtonType.Reply, 0);   //Info Button
			}
		}

		protected abstract TextDefinition[] GetValuesFor(T o, int aryLength);
		protected abstract bool IsFiltered(T o, string filter);

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			base.OnResponse(sender, info);

			var pm = sender.Mobile as PlayerMobile;

			if (pm == null || !IsMember(pm, guild))
			{
				return;
			}

			var id = info.ButtonID;

			switch (id)
			{
				case 5: //Filter
					{
						var t = info.GetTextEntry(1);
						pm.SendGump(GetResentGump(player, guild, m_Comparer, m_Ascending, (t == null) ? "" : t.Text, 0));
						break;
					}
				case 6: //Back
					{
						pm.SendGump(GetResentGump(player, guild, m_Comparer, m_Ascending, m_Filter, m_StartNumber - itemsPerPage));
						break;
					}
				case 7: //Forward
					{
						pm.SendGump(GetResentGump(player, guild, m_Comparer, m_Ascending, m_Filter, m_StartNumber + itemsPerPage));
						break;
					}
			}

			if (id >= 100 && id < (100 + m_Fields.Length))
			{
				var comparer = m_Fields[id - 100].Comparer;

				if (m_Comparer.GetType() == comparer.GetType())
				{
					m_Ascending = !m_Ascending;
				}

				pm.SendGump(GetResentGump(player, guild, comparer, m_Ascending, m_Filter, 0));
			}
			else if (id >= 200 && id < (200 + m_List.Count))
			{
				pm.SendGump(GetObjectInfoGump(player, guild, m_List[id - 200]));
			}
		}


		public abstract Gump GetResentGump(PlayerMobile pm, Guild g, IComparer<T> comparer, bool ascending, string filter, int startNumber);
		public abstract Gump GetObjectInfoGump(PlayerMobile pm, Guild g, T o);

		public void ResendGump()
		{
			player.SendGump(GetResentGump(player, guild, m_Comparer, m_Ascending, m_Filter, m_StartNumber));
		}
	}

	public struct InfoField<T>
	{
		private readonly TextDefinition m_Name;
		private readonly int m_Width;
		private readonly IComparer<T> m_Comparer;

		public TextDefinition Name => m_Name;
		public int Width => m_Width;
		public IComparer<T> Comparer => m_Comparer;
		public InfoField(TextDefinition name, int width, IComparer<T> comparer)
		{
			m_Name = name;
			m_Width = width;
			m_Comparer = comparer;
		}
	}

	/// Guild Search
	public delegate void SearchSelectionCallback(GuildDisplayType display);

	public class GuildAdvancedSearchGump : BaseGuildGump
	{
		private readonly GuildDisplayType m_Display;
		private readonly SearchSelectionCallback m_Callback;

		public GuildAdvancedSearchGump(PlayerMobile pm, Guild g, GuildDisplayType display, SearchSelectionCallback callback) : base(pm, g)
		{
			m_Callback = callback;
			m_Display = display;
			PopulateGump();
		}

		public override void PopulateGump()
		{
			base.PopulateGump();

			AddHtmlLocalized(431, 43, 110, 26, 1062978, 0xF, false, false); // Diplomacy

			AddHtmlLocalized(65, 80, 480, 26, 1063124, 0xF, true, false); // <i>Advanced Search Options</i>

			AddHtmlLocalized(65, 110, 480, 26, 1063136 + (int)m_Display, 0xF, false, false); // Showing All Guilds/w/Relation/Waiting Relation

			AddGroup(1);
			AddRadio(75, 140, 0xD2, 0xD3, false, 2);
			AddHtmlLocalized(105, 140, 200, 26, 1063006, 0x0, false, false); // Show Guilds with Relationship
			AddRadio(75, 170, 0xD2, 0xD3, false, 1);
			AddHtmlLocalized(105, 170, 200, 26, 1063005, 0x0, false, false); // Show Guilds Awaiting Action
			AddRadio(75, 200, 0xD2, 0xD3, false, 0);
			AddHtmlLocalized(105, 200, 200, 26, 1063007, 0x0, false, false); // Show All Guilds

			AddBackground(450, 370, 100, 26, 0x2486);
			AddButton(455, 375, 0x845, 0x846, 5, GumpButtonType.Reply, 0);
			AddHtmlLocalized(480, 373, 60, 26, 1006044, 0x0, false, false); // OK
			AddBackground(340, 370, 100, 26, 0x2486);
			AddButton(345, 375, 0x845, 0x846, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(370, 373, 60, 26, 1006045, 0x0, false, false); // Cancel
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			base.OnResponse(sender, info);

			var pm = sender.Mobile as PlayerMobile;

			if (pm == null || !IsMember(pm, guild))
			{
				return;
			}

			var display = m_Display;

			if (info.ButtonID == 5)
			{
				for (var i = 0; i < 3; i++)
				{
					if (info.IsSwitched(i))
					{
						display = (GuildDisplayType)i;
						m_Callback(display);
						break;
					}
				}
			}
		}
	}

	/// Guild Create/ Join
	public class CreateGuildGump : Gump
	{
		public CreateGuildGump(PlayerMobile pm) : this(pm, "Guild Name", "")
		{
		}

		public CreateGuildGump(PlayerMobile pm, string guildName, string guildAbbrev) : base(10, 10)
		{
			pm.CloseGump(typeof(CreateGuildGump));
			pm.CloseGump(typeof(BaseGuildGump));

			AddPage(0);

			AddBackground(0, 0, 500, 300, 0x2422);
			AddHtmlLocalized(25, 20, 450, 25, 1062939, 0x0, true, false); // <center>GUILD MENU</center>
			AddHtmlLocalized(25, 60, 450, 60, 1062940, 0x0, false, false); // As you are not a member of any guild, you can create your own by providing a unique guild name and paying the standard guild registration fee.
			AddHtmlLocalized(25, 135, 120, 25, 1062941, 0x0, false, false); // Registration Fee:
			AddLabel(155, 135, 0x481, Guild.RegistrationFee.ToString());
			AddHtmlLocalized(25, 165, 120, 25, 1011140, 0x0, false, false); // Enter Guild Name: 
			AddBackground(155, 160, 320, 26, 0xBB8);
			AddTextEntry(160, 163, 315, 21, 0x481, 5, guildName);
			AddHtmlLocalized(25, 191, 120, 26, 1063035, 0x0, false, false); // Abbreviation:
			AddBackground(155, 186, 320, 26, 0xBB8);
			AddTextEntry(160, 189, 315, 21, 0x481, 6, guildAbbrev);
			AddButton(415, 217, 0xF7, 0xF8, 1, GumpButtonType.Reply, 0);
			AddButton(345, 217, 0xF2, 0xF1, 0, GumpButtonType.Reply, 0);

			if (pm.AcceptGuildInvites)
			{
				AddButton(20, 260, 0xD2, 0xD3, 2, GumpButtonType.Reply, 0);
			}
			else
			{
				AddButton(20, 260, 0xD3, 0xD2, 2, GumpButtonType.Reply, 0);
			}

			AddHtmlLocalized(45, 260, 200, 30, 1062943, 0x0, false, false); // <i>Ignore Guild Invites</i>
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var pm = sender.Mobile as PlayerMobile;

			if (pm == null || pm.Guild != null)
			{
				return;     //Sanity
			}

			switch (info.ButtonID)
			{
				case 1:
					{
						var tName = info.GetTextEntry(5);
						var tAbbrev = info.GetTextEntry(6);

						var guildName = (tName == null) ? "" : tName.Text;
						var guildAbbrev = (tAbbrev == null) ? "" : tAbbrev.Text;

						guildName = Utility.FixHtml(guildName.Trim());
						guildAbbrev = Utility.FixHtml(guildAbbrev.Trim());

						if (guildName.Length <= 0)
						{
							pm.SendLocalizedMessage(1070884); // Guild name cannot be blank.
						}
						else if (guildAbbrev.Length <= 0)
						{
							pm.SendLocalizedMessage(1070885); // You must provide a guild abbreviation.
						}
						else if (guildName.Length > Guild.NameLimit)
						{
							pm.SendLocalizedMessage(1063036, Guild.NameLimit.ToString()); // A guild name cannot be more than ~1_val~ characters in length.
						}
						else if (guildAbbrev.Length > Guild.AbbrevLimit)
						{
							pm.SendLocalizedMessage(1063037, Guild.AbbrevLimit.ToString()); // An abbreviation cannot exceed ~1_val~ characters in length.
						}
						else if (Guild.FindByAbbrev(guildAbbrev) != null || !BaseGuildGump.CheckProfanity(guildAbbrev))
						{
							pm.SendLocalizedMessage(501153); // That abbreviation is not available.
						}
						else if (Guild.FindByName(guildName) != null || !BaseGuildGump.CheckProfanity(guildName))
						{
							pm.SendLocalizedMessage(1063000); // That guild name is not available.
						}
						else if (!Banker.Withdraw(pm, Guild.RegistrationFee))
						{
							pm.SendLocalizedMessage(1063001, Guild.RegistrationFee.ToString()); // You do not possess the ~1_val~ gold piece fee required to create a guild.
						}
						else
						{
							pm.SendLocalizedMessage(1060398, Guild.RegistrationFee.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
							pm.SendLocalizedMessage(1063238); // Your new guild has been founded.
							pm.Guild = new Guild(pm, guildName, guildAbbrev);
						}

						break;
					}
				case 2:
					{
						pm.AcceptGuildInvites = !pm.AcceptGuildInvites;

						if (pm.AcceptGuildInvites)
						{
							pm.SendLocalizedMessage(1070699); // You are now accepting guild invitations.
						}
						else
						{
							pm.SendLocalizedMessage(1070698); // You are now ignoring guild invitations.
						}

						break;
					}
			}
		}
	}

	public class GuildInvitationRequest : BaseGuildGump
	{
		private readonly PlayerMobile m_Inviter;
		public GuildInvitationRequest(PlayerMobile pm, Guild g, PlayerMobile inviter) : base(pm, g)
		{
			m_Inviter = inviter;

			PopulateGump();
		}

		public override void PopulateGump()
		{
			AddPage(0);

			AddBackground(0, 0, 350, 170, 0x2422);
			AddHtmlLocalized(25, 20, 300, 45, 1062946, 0x0, true, false); // <center>You have been invited to join a guild! (Warning: Accepting will make you attackable!)</center>
			AddHtml(25, 75, 300, 25, String.Format("<center>{0}</center>", guild.Name), true, false);
			AddButton(265, 130, 0xF7, 0xF8, 1, GumpButtonType.Reply, 0);
			AddButton(195, 130, 0xF2, 0xF1, 0, GumpButtonType.Reply, 0);
			AddButton(20, 130, 0xD2, 0xD3, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 130, 150, 30, 1062943, 0x0, false, false); // <i>Ignore Guild Invites</i>
		}


		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (guild.Disbanded || player.Guild != null)
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 0:
					{
						m_Inviter.SendLocalizedMessage(1063250, String.Format("{0}\t{1}", player.Name, guild.Name)); // ~1_val~ has declined your invitation to join ~2_val~.
						break;
					}
				case 1:
					{
						guild.AddMember(player);
						player.SendLocalizedMessage(1063056, guild.Name); // You have joined ~1_val~.
						m_Inviter.SendLocalizedMessage(1063249, String.Format("{0}\t{1}", player.Name, guild.Name)); // ~1_val~ has accepted your invitation to join ~2_val~.

						break;
					}
				case 2:
					{
						player.AcceptGuildInvites = false;
						player.SendLocalizedMessage(1070698); // You are now ignoring guild invitations.

						break;
					}
			}
		}
	}

	/// Guild Information
	public class GuildInfoGump : BaseGuildGump
	{
		private readonly bool m_IsResigning;

		public GuildInfoGump(PlayerMobile pm, Guild g) : this(pm, g, false)
		{
		}
		public GuildInfoGump(PlayerMobile pm, Guild g, bool isResigning) : base(pm, g)
		{
			m_IsResigning = isResigning;
			PopulateGump();
		}

		public override void PopulateGump()
		{
			var isLeader = IsLeader(player, guild);
			base.PopulateGump();

			AddHtmlLocalized(96, 43, 110, 26, 1063014, 0xF, false, false); // My Guild

			AddImageTiled(65, 80, 160, 26, 0xA40);
			AddImageTiled(67, 82, 156, 22, 0xBBC);
			AddHtmlLocalized(70, 83, 150, 20, 1062954, 0x0, false, false); // <i>Guild Name</i>
			AddHtml(233, 84, 320, 26, guild.Name, false, false);

			AddImageTiled(65, 114, 160, 26, 0xA40);
			AddImageTiled(67, 116, 156, 22, 0xBBC);
			AddHtmlLocalized(70, 117, 150, 20, 1063025, 0x0, false, false); // <i>Alliance</i>

			if (guild.Alliance != null && guild.Alliance.IsMember(guild))
			{
				AddHtml(233, 118, 320, 26, guild.Alliance.Name, false, false);
				AddButton(40, 120, 0x4B9, 0x4BA, 6, GumpButtonType.Reply, 0);   //Alliance Roster
			}

			if (Guild.OrderChaos && isLeader)
			{
				AddButton(40, 154, 0x4B9, 0x4BA, 100, GumpButtonType.Reply, 0); // Guild Faction
			}

			AddImageTiled(65, 148, 160, 26, 0xA40);
			AddImageTiled(67, 150, 156, 22, 0xBBC);
			AddHtmlLocalized(70, 151, 150, 20, 1063084, 0x0, false, false); // <i>Guild Faction</i>

			GuildType gt;
			Faction f;

			if ((gt = guild.Type) != GuildType.Regular)
			{
				AddHtml(233, 152, 320, 26, gt.ToString(), false, false);
			}
			else if ((f = Faction.Find(guild.Leader)) != null)
			{
				AddHtml(233, 152, 320, 26, f.ToString(), false, false);
			}

			AddImageTiled(65, 196, 480, 4, 0x238D);


			var s = guild.Charter;
			if (String.IsNullOrEmpty(s))
			{
				s = "The guild leader has not yet set the guild charter.";
			}

			AddHtml(65, 216, 480, 80, s, true, true);
			if (isLeader)
			{
				AddButton(40, 251, 0x4B9, 0x4BA, 4, GumpButtonType.Reply, 0);   //Charter Edit button
			}

			s = guild.Website;
			if (String.IsNullOrEmpty(s))
			{
				s = "Guild website not yet set.";
			}

			AddHtml(65, 306, 480, 30, s, true, false);
			if (isLeader)
			{
				AddButton(40, 313, 0x4B9, 0x4BA, 5, GumpButtonType.Reply, 0);   //Website Edit button
			}

			AddCheck(65, 370, 0xD2, 0xD3, player.DisplayGuildTitle, 0);
			AddHtmlLocalized(95, 370, 150, 26, 1063085, 0x0, false, false); // Show Guild Title
			AddBackground(450, 370, 100, 26, 0x2486);

			AddButton(455, 375, 0x845, 0x846, 7, GumpButtonType.Reply, 0);
			AddHtmlLocalized(480, 373, 60, 26, 3006115, (short)(m_IsResigning ? 0x5000 : 0), false, false); // Resign
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			base.OnResponse(sender, info);

			var pm = sender.Mobile as PlayerMobile;

			if (!IsMember(pm, guild))
			{
				return;
			}

			pm.DisplayGuildTitle = info.IsSwitched(0);

			switch (info.ButtonID)
			{
				//1-3 handled by base.OnResponse
				case 4:
					{
						if (IsLeader(pm, guild))
						{
							pm.SendLocalizedMessage(1013071); // Enter the new guild charter (50 characters max):

							pm.BeginPrompt(new PromptCallback(SetCharter_Callback), true);  //Have the same callback handle both canceling and deletion cause the 2nd callback would just get a text of ""
						}
						break;
					}
				case 5:
					{
						if (IsLeader(pm, guild))
						{
							pm.SendLocalizedMessage(1013072); // Enter the new website for the guild (50 characters max):
							pm.BeginPrompt(new PromptCallback(SetWebsite_Callback), true);  //Have the same callback handle both canceling and deletion cause the 2nd callback would just get a text of ""
						}
						break;
					}
				case 6:
					{
						//Alliance Roster
						if (guild.Alliance != null && guild.Alliance.IsMember(guild))
						{
							pm.SendGump(new AllianceInfo.AllianceRosterGump(pm, guild, guild.Alliance));
						}

						break;
					}
				case 7:
					{
						//Resign
						if (!m_IsResigning)
						{
							pm.SendLocalizedMessage(1063332); // Are you sure you wish to resign from your guild?
							pm.SendGump(new GuildInfoGump(pm, guild, true));
						}
						else
						{
							guild.RemoveMember(pm, 1063411); // You resign from your guild.
						}
						break;
					}
				case 100: // Custom code to support Order/Chaos in the new guild system
					{
						// Guild Faction
						if (Guild.OrderChaos && IsLeader(pm, guild))
						{
							pm.CloseGump(typeof(GuildChangeTypeGump));
							pm.SendGump(new GuildChangeTypeGump(pm, guild));
						}
						break;
					}
			}
		}

		public void SetCharter_Callback(Mobile from, string text)
		{
			if (!IsLeader(from, guild))
			{
				return;
			}

			var charter = Utility.FixHtml(text.Trim());

			if (charter.Length > 50)
			{
				from.SendLocalizedMessage(1070774, "50"); // Your guild charter cannot exceed ~1_val~ characters.
			}
			else
			{
				guild.Charter = charter;
				from.SendLocalizedMessage(1070775); // You submit a new guild charter.
				return;
			}
		}

		public void SetWebsite_Callback(Mobile from, string text)
		{
			if (!IsLeader(from, guild))
			{
				return;
			}

			var site = Utility.FixHtml(text.Trim());

			if (site.Length > 50)
			{
				from.SendLocalizedMessage(1070777, "50"); // Your guild website cannot exceed ~1_val~ characters.
			}
			else
			{
				guild.Website = site;
				from.SendLocalizedMessage(1070778); // You submit a new guild website.
				return;
			}
		}
	}

	public class GuildMemberInfoGump : BaseGuildGump
	{
		private readonly PlayerMobile m_Member;
		private readonly bool m_ToLeader, m_toKick;

		public GuildMemberInfoGump(PlayerMobile pm, Guild g, PlayerMobile member, bool toKick, bool toPromoteToLeader) : base(pm, g, 10, 40)
		{
			m_ToLeader = toPromoteToLeader;
			m_toKick = toKick;
			m_Member = member;
			PopulateGump();
		}

		public override void PopulateGump()
		{
			AddPage(0);

			AddBackground(0, 0, 350, 255, 0x242C);
			AddHtmlLocalized(20, 15, 310, 26, 1063018, 0x0, false, false); // <div align=center><i>Guild Member Information</i></div>
			AddImageTiled(20, 40, 310, 2, 0x2711);

			AddHtmlLocalized(20, 50, 150, 26, 1062955, 0x0, true, false); // <i>Name</i>
			AddHtml(180, 53, 150, 26, m_Member.Name, false, false);

			AddHtmlLocalized(20, 80, 150, 26, 1062956, 0x0, true, false); // <i>Rank</i>
			AddHtmlLocalized(180, 83, 150, 26, m_Member.GuildRank.Name, 0x0, false, false);

			AddHtmlLocalized(20, 110, 150, 26, 1062953, 0x0, true, false); // <i>Guild Title</i>
			AddHtml(180, 113, 150, 26, m_Member.GuildTitle, false, false);
			AddImageTiled(20, 142, 310, 2, 0x2711);

			AddBackground(20, 150, 310, 26, 0x2486);
			AddButton(25, 155, 0x845, 0x846, 4, GumpButtonType.Reply, 0);
			AddHtmlLocalized(50, 153, 270, 26, (m_Member == player.GuildFealty && guild.Leader != m_Member) ? 1063082 : 1062996, 0x0, false, false); // Clear/Cast Vote For This Member

			AddBackground(20, 180, 150, 26, 0x2486);
			AddButton(25, 185, 0x845, 0x846, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(50, 183, 110, 26, 1062993, (short)(m_ToLeader ? 0x990000 : 0), false, false); // Promote

			AddBackground(180, 180, 150, 26, 0x2486);
			AddButton(185, 185, 0x845, 0x846, 3, GumpButtonType.Reply, 0);
			AddHtmlLocalized(210, 183, 110, 26, 1062995, 0x0, false, false); // Set Guild Title

			AddBackground(20, 210, 150, 26, 0x2486);
			AddButton(25, 215, 0x845, 0x846, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(50, 213, 110, 26, 1062994, 0x0, false, false); // Demote

			AddBackground(180, 210, 150, 26, 0x2486);
			AddButton(185, 215, 0x845, 0x846, 5, GumpButtonType.Reply, 0);
			AddHtmlLocalized(210, 213, 110, 26, 1062997, (short)(m_toKick ? 0x5000 : 0), false, false); // Kick
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var pm = sender.Mobile as PlayerMobile;

			if (pm == null || !IsMember(pm, guild) || !IsMember(m_Member, guild))
			{
				return;
			}

			var playerRank = pm.GuildRank;
			var targetRank = m_Member.GuildRank;

			switch (info.ButtonID)
			{
				case 1: //Promote
					{
						if (playerRank.GetFlag(RankFlags.CanPromoteDemote) && ((playerRank.Rank - 1) > targetRank.Rank || (playerRank == RankDefinition.Leader && playerRank.Rank > targetRank.Rank)))
						{
							targetRank = RankDefinition.Ranks[targetRank.Rank + 1];

							if (targetRank == RankDefinition.Leader)
							{
								if (m_ToLeader)
								{
									m_Member.GuildRank = targetRank;
									pm.SendLocalizedMessage(1063156, m_Member.Name); // The guild information for ~1_val~ has been updated.
									pm.SendLocalizedMessage(1063156, pm.Name); // The guild information for ~1_val~ has been updated.
									guild.Leader = m_Member;
								}
								else
								{
									pm.SendLocalizedMessage(1063144); // Are you sure you wish to make this member the new guild leader?
									pm.SendGump(new GuildMemberInfoGump(player, guild, m_Member, false, true));
								}
							}
							else
							{
								m_Member.GuildRank = targetRank;
								pm.SendLocalizedMessage(1063156, m_Member.Name); // The guild information for ~1_val~ has been updated.
							}
						}
						else
						{
							pm.SendLocalizedMessage(1063143); // You don't have permission to promote this member.
						}

						break;
					}
				case 2: //Demote
					{
						if (playerRank.GetFlag(RankFlags.CanPromoteDemote) && playerRank.Rank > targetRank.Rank)
						{
							if (targetRank == RankDefinition.Lowest)
							{
								if (RankDefinition.Lowest.Name.Number == 1062963)
								{
									pm.SendLocalizedMessage(1063333); // You can't demote a ronin.
								}
								else
								{
									pm.SendMessage("You can't demote a {0}.", RankDefinition.Lowest.Name);
								}
							}
							else
							{
								m_Member.GuildRank = RankDefinition.Ranks[targetRank.Rank - 1];
								pm.SendLocalizedMessage(1063156, m_Member.Name); // The guild information for ~1_val~ has been updated.
							}
						}
						else
						{
							pm.SendLocalizedMessage(1063146); // You don't have permission to demote this member.
						}

						break;
					}
				case 3: //Set Guild title
					{
						if (playerRank.GetFlag(RankFlags.CanSetGuildTitle) && (playerRank.Rank > targetRank.Rank || m_Member == player))
						{
							pm.SendLocalizedMessage(1011128); // Enter the new title for this guild member or 'none' to remove a title:

							pm.BeginPrompt(new PromptCallback(SetTitle_Callback));
						}
						else if (m_Member.GuildTitle == null || m_Member.GuildTitle.Length <= 0)
						{
							pm.SendLocalizedMessage(1070746); // You don't have the permission to set that member's guild title.
						}
						else
						{
							pm.SendLocalizedMessage(1063148); // You don't have permission to change this member's guild title.
						}

						break;
					}
				case 4: //Vote
					{
						if (m_Member == pm.GuildFealty && guild.Leader != m_Member)
						{
							pm.SendLocalizedMessage(1063158); // You have cleared your vote for guild leader.
						}
						else if (guild.CanVote(m_Member))//( playerRank.GetFlag( RankFlags.CanVote ) )
						{
							if (m_Member == guild.Leader)
							{
								pm.SendLocalizedMessage(1063424); // You can't vote for the current guild leader.
							}
							else if (!guild.CanBeVotedFor(m_Member))
							{
								pm.SendLocalizedMessage(1063425); // You can't vote for an inactive guild member.
							}
							else
							{
								pm.GuildFealty = m_Member;
								pm.SendLocalizedMessage(1063159, m_Member.Name); // You cast your vote for ~1_val~ for guild leader.
							}
						}
						else
						{
							pm.SendLocalizedMessage(1063149); // You don't have permission to vote.
						}

						break;
					}
				case 5: //Kick
					{
						if ((playerRank.GetFlag(RankFlags.RemovePlayers) && playerRank.Rank > targetRank.Rank) || (playerRank.GetFlag(RankFlags.RemoveLowestRank) && targetRank == RankDefinition.Lowest))
						{
							if (m_toKick)
							{
								guild.RemoveMember(m_Member);
								pm.SendLocalizedMessage(1063157); // The member has been removed from your guild.
							}
							else
							{
								pm.SendLocalizedMessage(1063152); // Are you sure you wish to kick this member from the guild?
								pm.SendGump(new GuildMemberInfoGump(player, guild, m_Member, true, false));
							}
						}
						else
						{
							pm.SendLocalizedMessage(1063151); // You don't have permission to remove this member.
						}

						break;
					}
			}
		}

		public void SetTitle_Callback(Mobile from, string text)
		{
			var pm = from as PlayerMobile;
			var targ = m_Member;

			if (pm == null || targ == null)
			{
				return;
			}

			var g = targ.Guild as Guild;

			if (g == null || !IsMember(pm, g) || !(pm.GuildRank.GetFlag(RankFlags.CanSetGuildTitle) && (pm.GuildRank.Rank > targ.GuildRank.Rank || pm == targ)))
			{
				if (m_Member.GuildTitle == null || m_Member.GuildTitle.Length <= 0)
				{
					pm.SendLocalizedMessage(1070746); // You don't have the permission to set that member's guild title.
				}
				else
				{
					pm.SendLocalizedMessage(1063148); // You don't have permission to change this member's guild title.
				}

				return;
			}


			var title = Utility.FixHtml(text.Trim());

			if (title.Length > 20)
			{
				from.SendLocalizedMessage(501178); // That title is too long.
			}
			else if (!BaseGuildGump.CheckProfanity(title))
			{
				from.SendLocalizedMessage(501179); // That title is disallowed.
			}
			else
			{
				if (Insensitive.Equals(title, "none"))
				{
					targ.GuildTitle = null;
				}
				else
				{
					targ.GuildTitle = title;
				}

				pm.SendLocalizedMessage(1063156, targ.Name); // The guild information for ~1_val~ has been updated.
			}
		}
	}

	public class GuildRosterGump : BaseGuildListGump<PlayerMobile>
	{
		#region Comparers
		private class NameComparer : IComparer<PlayerMobile>
		{
			public static readonly IComparer<PlayerMobile> Instance = new NameComparer();

			public NameComparer()
			{
			}

			public int Compare(PlayerMobile x, PlayerMobile y)
			{
				if (x == null && y == null)
				{
					return 0;
				}
				else if (x == null)
				{
					return -1;
				}
				else if (y == null)
				{
					return 1;
				}

				return Insensitive.Compare(x.Name, y.Name);
			}
		}

		private class LastOnComparer : IComparer<PlayerMobile>
		{
			public static readonly IComparer<PlayerMobile> Instance = new LastOnComparer();

			public LastOnComparer()
			{
			}

			public int Compare(PlayerMobile x, PlayerMobile y)
			{
				if (x == null && y == null)
				{
					return 0;
				}
				else if (x == null)
				{
					return -1;
				}
				else if (y == null)
				{
					return 1;
				}

				var aState = x.NetState;
				var bState = y.NetState;

				if (aState == null && bState == null)
				{
					return x.LastOnline.CompareTo(y.LastOnline);
				}
				else if (aState == null)
				{
					return -1;
				}
				else if (bState == null)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}
		private class TitleComparer : IComparer<PlayerMobile>
		{
			public static readonly IComparer<PlayerMobile> Instance = new TitleComparer();

			public TitleComparer()
			{
			}

			public int Compare(PlayerMobile x, PlayerMobile y)
			{
				if (x == null && y == null)
				{
					return 0;
				}
				else if (x == null)
				{
					return -1;
				}
				else if (y == null)
				{
					return 1;
				}

				return Insensitive.Compare(x.GuildTitle, y.GuildTitle);
			}
		}

		private class RankComparer : IComparer<PlayerMobile>
		{
			public static readonly IComparer<PlayerMobile> Instance = new RankComparer();

			public RankComparer()
			{
			}

			public int Compare(PlayerMobile x, PlayerMobile y)
			{
				if (x == null && y == null)
				{
					return 0;
				}
				else if (x == null)
				{
					return -1;
				}
				else if (y == null)
				{
					return 1;
				}

				return x.GuildRank.Rank.CompareTo(y.GuildRank.Rank);
			}
		}

		#endregion

		private static readonly InfoField<PlayerMobile>[] m_Fields =
			new InfoField<PlayerMobile>[]
			{
				new InfoField<PlayerMobile>( 1062955, 130, GuildRosterGump.NameComparer.Instance    ),	//Name
				new InfoField<PlayerMobile>( 1062956, 80,    GuildRosterGump.RankComparer.Instance  ),	//Rank
				new InfoField<PlayerMobile>( 1062952, 80,    GuildRosterGump.LastOnComparer.Instance),	//Last On
				new InfoField<PlayerMobile>( 1062953, 150, GuildRosterGump.TitleComparer.Instance   )	//Guild Title
			};

		public GuildRosterGump(PlayerMobile pm, Guild g) : this(pm, g, GuildRosterGump.LastOnComparer.Instance, false, "", 0)
		{
		}

		public GuildRosterGump(PlayerMobile pm, Guild g, IComparer<PlayerMobile> currentComparer, bool ascending, string filter, int startNumber)
			: base(pm, g, Utility.SafeConvertList<Mobile, PlayerMobile>(g.Members), currentComparer, ascending, filter, startNumber, m_Fields)
		{
			PopulateGump();
		}

		public override void PopulateGump()
		{
			base.PopulateGump();

			AddHtmlLocalized(266, 43, 110, 26, 1062974, 0xF, false, false); // Guild Roster
		}

		public override void DrawEndingEntry(int itemNumber)
		{
			AddBackground(225, 148 + itemNumber * 28, 150, 26, 0x2486);
			AddButton(230, 153 + itemNumber * 28, 0x845, 0x846, 8, GumpButtonType.Reply, 0);
			AddHtmlLocalized(255, 151 + itemNumber * 28, 110, 26, 1062992, 0x0, false, false); // Invite Player
		}

		protected override TextDefinition[] GetValuesFor(PlayerMobile pm, int aryLength)
		{
			var defs = new TextDefinition[aryLength];

			var name = String.Format("{0}{1}", pm.Name, (player.GuildFealty == pm && player.GuildFealty != guild.Leader) ? " *" : "");

			if (pm == player)
			{
				name = Color(name, 0x006600);
			}
			else if (pm.NetState != null)
			{
				name = Color(name, 0x000066);
			}

			defs[0] = name;
			defs[1] = pm.GuildRank.Name;
			defs[2] = (pm.NetState != null) ? new TextDefinition(1063015) : new TextDefinition(pm.LastOnline.ToString("yyyy-MM-dd"));
			defs[3] = (pm.GuildTitle == null) ? "" : pm.GuildTitle;

			return defs;
		}

		protected override bool IsFiltered(PlayerMobile pm, string filter)
		{
			if (pm == null)
			{
				return true;
			}

			return !Insensitive.Contains(pm.Name, filter);
		}

		public override Gump GetResentGump(PlayerMobile pm, Guild g, IComparer<PlayerMobile> comparer, bool ascending, string filter, int startNumber)
		{
			return new GuildRosterGump(pm, g, comparer, ascending, filter, startNumber);
		}

		public override Gump GetObjectInfoGump(PlayerMobile pm, Guild g, PlayerMobile o)
		{
			return new GuildMemberInfoGump(pm, g, o, false, false);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			base.OnResponse(sender, info);

			var pm = sender.Mobile as PlayerMobile;

			if (pm == null || !IsMember(pm, guild))
			{
				return;
			}

			if (info.ButtonID == 8)
			{
				if (pm.GuildRank.GetFlag(RankFlags.CanInvitePlayer))
				{
					pm.SendLocalizedMessage(1063048); // Whom do you wish to invite into your guild?
					pm.BeginTarget(-1, false, Targeting.TargetFlags.None, new TargetStateCallback(InvitePlayer_Callback), guild);
				}
				else
				{
					pm.SendLocalizedMessage(503301); // You don't have permission to do that.
				}
			}
		}

		public void InvitePlayer_Callback(Mobile from, object targeted, object state)
		{
			var pm = from as PlayerMobile;
			var targ = targeted as PlayerMobile;

			var g = state as Guild;

			var guildState = PlayerState.Find(g.Leader);
			var targetState = PlayerState.Find(targ);

			var guildFaction = (guildState == null ? null : guildState.Faction);
			var targetFaction = (targetState == null ? null : targetState.Faction);

			if (pm == null || !IsMember(pm, guild) || !pm.GuildRank.GetFlag(RankFlags.CanInvitePlayer))
			{
				pm.SendLocalizedMessage(503301); // You don't have permission to do that.
			}
			else if (targ == null)
			{
				pm.SendLocalizedMessage(1063334); // That isn't a valid player.
			}
			else if (!targ.AcceptGuildInvites)
			{
				pm.SendLocalizedMessage(1063049, targ.Name); // ~1_val~ is not accepting guild invitations.
			}
			else if (g.IsMember(targ))
			{
				pm.SendLocalizedMessage(1063050, targ.Name); // ~1_val~ is already a member of your guild!
			}
			else if (targ.Guild != null)
			{
				pm.SendLocalizedMessage(1063051, targ.Name); // ~1_val~ is already a member of a guild.
			}
			else if (targ.HasGump(typeof(BaseGuildGump)) || targ.HasGump(typeof(CreateGuildGump)))  //TODO: Check message if CreateGuildGump Open
			{
				pm.SendLocalizedMessage(1063052, targ.Name); // ~1_val~ is currently considering another guild invitation.
			}
			#region Factions
			else if (targ.Young && guildFaction != null)
			{
				pm.SendLocalizedMessage(1070766); // You cannot invite a young player to your faction-aligned guild.
			}
			else if (guildFaction != targetFaction)
			{
				if (guildFaction == null)
				{
					pm.SendLocalizedMessage(1013027); // That player cannot join a non-faction guild.
				}
				else if (targetFaction == null)
				{
					pm.SendLocalizedMessage(1013026); // That player must be in a faction before joining this guild.
				}
				else
				{
					pm.SendLocalizedMessage(1013028); // That person has a different faction affiliation.
				}
			}
			else if (targetState != null && targetState.IsLeaving)
			{
				// OSI does this quite strangely, so we'll just do it this way
				pm.SendMessage("That person is quitting their faction and so you may not recruit them.");
			}
			#endregion
			else
			{
				pm.SendLocalizedMessage(1063053, targ.Name); // You invite ~1_val~ to join your guild.
				targ.SendGump(new GuildInvitationRequest(targ, guild, pm));
			}
		}
	}

	public class OtherGuildInfo : BaseGuildGump
	{
		private readonly Guild m_Other;
		public OtherGuildInfo(PlayerMobile pm, Guild g, Guild otherGuild) : base(pm, g, 10, 40)
		{
			m_Other = otherGuild;

			g.CheckExpiredWars();

			PopulateGump();
		}

		public void AddButtonAndBackground(int x, int y, int buttonID, int locNum)
		{
			AddBackground(x, y, 225, 26, 0x2486);
			AddButton(x + 5, y + 5, 0x845, 0x846, buttonID, GumpButtonType.Reply, 0);
			AddHtmlLocalized(x + 30, y + 3, 185, 26, locNum, 0x0, false, false);
		}

		public override void PopulateGump()
		{
			var g = Guild.GetAllianceLeader(guild);
			var other = Guild.GetAllianceLeader(m_Other);

			var war = g.FindPendingWar(other);
			var activeWar = g.FindActiveWar(other);

			var alliance = guild.Alliance;
			var otherAlliance = m_Other.Alliance;
			//NOTE TO SELF: Only only alliance leader can see pending guild alliance statuses

			var PendingWar = (war != null);
			var ActiveWar = (activeWar != null);
			AddPage(0);

			AddBackground(0, 0, 520, 335, 0x242C);
			AddHtmlLocalized(20, 15, 480, 26, 1062975, 0x0, false, false); // <div align=center><i>Guild Relationship</i></div>
			AddImageTiled(20, 40, 480, 2, 0x2711);
			AddHtmlLocalized(20, 50, 120, 26, 1062954, 0x0, true, false); // <i>Guild Name</i>
			AddHtml(150, 53, 360, 26, m_Other.Name, false, false);

			AddHtmlLocalized(20, 80, 120, 26, 1063025, 0x0, true, false); // <i>Alliance</i>

			if (otherAlliance != null)
			{
				if (otherAlliance.IsMember(m_Other))
				{
					AddHtml(150, 83, 360, 26, otherAlliance.Name, false, false);
				}
				//else if( otherAlliance.Leader == guild && ( otherAlliance.IsPendingMember( m_Other ) || otherAlliance.IsPendingMember( guild ) ) )
				/*		else if( (otherAlliance.Leader == guild && otherAlliance.IsPendingMember( m_Other ) ) || ( otherAlliance.Leader == m_Other && otherAlliance.IsPendingMember( guild ) ) )
						{
							AddHtml( 150, 83, 360, 26, Color( alliance.Name, 0xF), false, false );
						}
						//AddHtml( 150, 83, 360, 26, ( alliance.PendingMembers.Contains( guild ) || alliance.PendingMembers.Contains( m_Other ) ) ? String.Format( "<basefont color=#blue>{0}</basefont>", alliance.Name ) : alliance.Name, false, false );
						//AddHtml( 150, 83, 360, 26, ( otherAlliance == alliance &&  otherAlliance.PendingMembers.Contains( guild ) || otherAlliance.PendingMembers.Contains( m_Other ) ) ? String.Format( "<basefont color=#blue>{0}</basefont>", otherAlliance.Name ) : otherAlliance.Name, false, false );
						*/
			}

			AddHtmlLocalized(20, 110, 120, 26, 1063139, 0x0, true, false); // <i>Abbreviation</i>
			AddHtml(150, 113, 120, 26, m_Other.Abbreviation, false, false);


			var kills = "0/0";
			var time = "00:00";
			var otherKills = "0/0";

			WarDeclaration otherWar;

			if (ActiveWar)
			{
				kills = String.Format("{0}/{1}", activeWar.Kills, activeWar.MaxKills);

				var timeRemaining = TimeSpan.Zero;

				if (activeWar.WarLength != TimeSpan.Zero && (activeWar.WarBeginning + activeWar.WarLength) > DateTime.UtcNow)
				{
					timeRemaining = (activeWar.WarBeginning + activeWar.WarLength) - DateTime.UtcNow;
				}

				//time = String.Format( "{0:D2}:{1:D2}", timeRemaining.Hours.ToString(), timeRemaining.Subtract( TimeSpan.FromHours( timeRemaining.Hours ) ).Minutes );	//Is there a formatter for htis? it's 2AM and I'm tired and can't find it
				time = String.Format("{0:D2}:{1:mm}", timeRemaining.Hours, DateTime.MinValue + timeRemaining);

				otherWar = m_Other.FindActiveWar(guild);
				if (otherWar != null)
				{
					otherKills = String.Format("{0}/{1}", otherWar.Kills, otherWar.MaxKills);
				}
			}
			else if (PendingWar)
			{
				kills = Color(String.Format("{0}/{1}", war.Kills, war.MaxKills), 0x990000);
				//time = Color( String.Format( "{0}:{1}", war.WarLength.Hours, ((TimeSpan)(war.WarLength - TimeSpan.FromHours( war.WarLength.Hours ))).Minutes ), 0xFF0000 );
				time = Color(String.Format("{0:D2}:{1:mm}", war.WarLength.Hours, DateTime.MinValue + war.WarLength), 0x990000);

				otherWar = m_Other.FindPendingWar(guild);
				if (otherWar != null)
				{
					otherKills = Color(String.Format("{0}/{1}", otherWar.Kills, otherWar.MaxKills), 0x990000);
				}
			}

			AddHtmlLocalized(280, 110, 120, 26, 1062966, 0x0, true, false); // <i>Your Kills</i>
			AddHtml(410, 113, 120, 26, kills, false, false);

			AddHtmlLocalized(20, 140, 120, 26, 1062968, 0x0, true, false); // <i>Time Remaining</i>
			AddHtml(150, 143, 120, 26, time, false, false);

			AddHtmlLocalized(280, 140, 120, 26, 1062967, 0x0, true, false); // <i>Their Kills</i>
			AddHtml(410, 143, 120, 26, otherKills, false, false);

			AddImageTiled(20, 172, 480, 2, 0x2711);

			var number = 1062973;// <div align=center>You are at peace with this guild.</div>


			if (PendingWar)
			{
				if (war.WarRequester)
				{
					number = 1063027; // <div align=center>You have challenged this guild to war!</div>
				}
				else
				{
					number = 1062969; // <div align=center>This guild has challenged you to war!</div>

					AddButtonAndBackground(20, 260, 5, 1062981); // Accept Challenge
					AddButtonAndBackground(275, 260, 6, 1062983); //Modify Terms
				}

				AddButtonAndBackground(20, 290, 7, 1062982); // Dismiss Challenge
			}
			else if (ActiveWar)
			{
				number = 1062965; // <div align=center>You are at war with this guild!</div>
				AddButtonAndBackground(20, 290, 8, 1062980); // Surrender
			}
			else if (alliance != null && alliance == otherAlliance) //alliance, Same Alliance
			{
				if (alliance.IsMember(guild) && alliance.IsMember(m_Other)) //Both in Same alliance, full members
				{
					number = 1062970; // <div align=center>You are allied with this guild.</div>

					if (alliance.Leader == guild)
					{
						AddButtonAndBackground(20, 260, 12, 1062984); // Remove Guild from Alliance
						AddButtonAndBackground(275, 260, 13, 1063433); // Promote to Alliance Leader	//Note: No 'confirmation' like the other leader guild promotion things
																	   //Remove guild from alliance	//Promote to Alliance Leader
					}

					//Show roster, Centered, up
					AddButtonAndBackground(148, 215, 10, 1063164); //Show Alliance Roster
																   //Leave Alliance
					AddButtonAndBackground(20, 290, 11, 1062985); // Leave Alliance
				}
				else if (alliance.Leader == guild && alliance.IsPendingMember(m_Other))
				{
					number = 1062971; // <div align=center>You have requested an alliance with this guild.</div>

					//Show Alliance Roster, Centered, down.
					AddButtonAndBackground(148, 245, 10, 1063164); //Show Alliance Roster
																   //Withdraw Request
					AddButtonAndBackground(20, 290, 14, 1062986); // Withdraw Request

					AddHtml(150, 83, 360, 26, Color(alliance.Name, 0x99), false, false);
				}
				else if (alliance.Leader == m_Other && alliance.IsPendingMember(guild))
				{
					number = 1062972; // <div align=center>This guild has requested an alliance.</div>

					//Show alliance Roster, top
					AddButtonAndBackground(148, 215, 10, 1063164); //Show Alliance Roster
																   //Deny Request
																   //Accept Request
					AddButtonAndBackground(20, 260, 15, 1062988); // Deny Request
					AddButtonAndBackground(20, 290, 16, 1062987); // Accept Request

					AddHtml(150, 83, 360, 26, Color(alliance.Name, 0x99), false, false);
				}
			}
			else
			{
				AddButtonAndBackground(20, 260, 2, 1062990); // Request Alliance 
				AddButtonAndBackground(20, 290, 1, 1062989); // Declare War!
			}

			AddButtonAndBackground(275, 290, 0, 3000091); //Cancel

			AddHtmlLocalized(20, 180, 480, 30, number, 0x0, true, false);
			AddImageTiled(20, 245, 480, 2, 0x2711);
		}


		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var pm = sender.Mobile as PlayerMobile;

			if (!IsMember(pm, guild))
			{
				return;
			}

			var playerRank = pm.GuildRank;

			var guildLeader = Guild.GetAllianceLeader(guild);
			var otherGuild = Guild.GetAllianceLeader(m_Other);

			var war = guildLeader.FindPendingWar(otherGuild);
			var activeWar = guildLeader.FindActiveWar(otherGuild);
			var otherWar = otherGuild.FindPendingWar(guildLeader);

			var alliance = guild.Alliance;
			var otherAlliance = otherGuild.Alliance;

			switch (info.ButtonID)
			{
				#region War
				case 5: //Accept the war
					{
						if (war != null && !war.WarRequester && activeWar == null)
						{
							if (!playerRank.GetFlag(RankFlags.ControlWarStatus))
							{
								pm.SendLocalizedMessage(1063440); // You don't have permission to negotiate wars.
							}
							else if (alliance != null && alliance.Leader != guild)
							{
								pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", guild.Name, alliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
								pm.SendLocalizedMessage(1070707, alliance.Leader.Name); // You need to negotiate via ~1_val~ instead.
							}
							else
							{
								//Accept the war
								guild.PendingWars.Remove(war);
								war.WarBeginning = DateTime.UtcNow;
								guild.AcceptedWars.Add(war);

								if (alliance != null && alliance.IsMember(guild))
								{
									alliance.AllianceMessage(1070769, ((otherAlliance != null) ? otherAlliance.Name : otherGuild.Name)); // Guild Message: Your guild is now at war with ~1_GUILDNAME~
									alliance.InvalidateMemberProperties();
								}
								else
								{
									guild.GuildMessage(1070769, ((otherAlliance != null) ? otherAlliance.Name : otherGuild.Name)); // Guild Message: Your guild is now at war with ~1_GUILDNAME~
									guild.InvalidateMemberProperties();
								}
								//Technically  SHOULD say Your guild is now at war w/out any info, intentional diff.

								otherGuild.PendingWars.Remove(otherWar);
								otherWar.WarBeginning = DateTime.UtcNow;
								otherGuild.AcceptedWars.Add(otherWar);

								if (otherAlliance != null && m_Other.Alliance.IsMember(m_Other))
								{
									otherAlliance.AllianceMessage(1070769, ((alliance != null) ? alliance.Name : guild.Name)); // Guild Message: Your guild is now at war with ~1_GUILDNAME~
									otherAlliance.InvalidateMemberProperties();
								}
								else
								{
									otherGuild.GuildMessage(1070769, ((alliance != null) ? alliance.Name : guild.Name)); // Guild Message: Your guild is now at war with ~1_GUILDNAME~
									otherGuild.InvalidateMemberProperties();
								}
							}
						}

						break;
					}
				case 6: //Modify war terms
					{
						if (war != null && !war.WarRequester && activeWar == null)
						{
							if (!playerRank.GetFlag(RankFlags.ControlWarStatus))
							{
								pm.SendLocalizedMessage(1063440); // You don't have permission to negotiate wars.
							}
							else if (alliance != null && alliance.Leader != guild)
							{
								pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", guild.Name, alliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
								pm.SendLocalizedMessage(1070707, alliance.Leader.Name); // You need to negotiate via ~1_val~ instead.
							}
							else
							{
								pm.SendGump(new WarDeclarationGump(pm, guild, otherGuild));
							}
						}
						break;
					}
				case 7: //Dismiss war
					{
						if (war != null)
						{
							if (!playerRank.GetFlag(RankFlags.ControlWarStatus))
							{
								pm.SendLocalizedMessage(1063440); // You don't have permission to negotiate wars.
							}
							else if (alliance != null && alliance.Leader != guild)
							{
								pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", guild.Name, alliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
								pm.SendLocalizedMessage(1070707, alliance.Leader.Name); // You need to negotiate via ~1_val~ instead.
							}
							else
							{
								//Dismiss the war
								guild.PendingWars.Remove(war);
								otherGuild.PendingWars.Remove(otherWar);
								pm.SendLocalizedMessage(1070752); // The proposal has been updated.
																  //Messages to opposing guild? (Testing on OSI says no)
							}
						}
						break;
					}
				case 8: //Surrender
					{
						if (!playerRank.GetFlag(RankFlags.ControlWarStatus))
						{
							pm.SendLocalizedMessage(1063440); // You don't have permission to negotiate wars.
						}
						else if (alliance != null && alliance.Leader != guild)
						{
							pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", guild.Name, alliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
							pm.SendLocalizedMessage(1070707, alliance.Leader.Name); // You need to negotiate via ~1_val~ instead.
						}
						else
						{
							if (activeWar != null)
							{
								if (alliance != null && alliance.IsMember(guild))
								{
									alliance.AllianceMessage(1070740, ((otherAlliance != null) ? otherAlliance.Name : otherGuild.Name));// You have lost the war with ~1_val~.
									alliance.InvalidateMemberProperties();
								}
								else
								{
									guild.GuildMessage(1070740, ((otherAlliance != null) ? otherAlliance.Name : otherGuild.Name));// You have lost the war with ~1_val~.
									guild.InvalidateMemberProperties();
								}

								guild.AcceptedWars.Remove(activeWar);

								if (otherAlliance != null && otherAlliance.IsMember(otherGuild))
								{
									otherAlliance.AllianceMessage(1070739, ((guild.Alliance != null) ? guild.Alliance.Name : guild.Name));// You have won the war against ~1_val~!
									otherAlliance.InvalidateMemberProperties();
								}
								else
								{
									otherGuild.GuildMessage(1070739, ((guild.Alliance != null) ? guild.Alliance.Name : guild.Name));// You have won the war against ~1_val~!
									otherGuild.InvalidateMemberProperties();
								}

								otherGuild.AcceptedWars.Remove(otherGuild.FindActiveWar(guild));
							}
						}
						break;
					}
				case 1: //Declare War
					{
						if (war == null && activeWar == null)
						{
							if (!playerRank.GetFlag(RankFlags.ControlWarStatus))
							{
								pm.SendLocalizedMessage(1063440); // You don't have permission to negotiate wars.
							}
							else if (alliance != null && alliance.Leader != guild)
							{
								pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", guild.Name, alliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
								pm.SendLocalizedMessage(1070707, alliance.Leader.Name); // You need to negotiate via ~1_val~ instead.
							}
							else if (otherAlliance != null && otherAlliance.Leader != m_Other)
							{
								pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", m_Other.Name, otherAlliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
								pm.SendLocalizedMessage(1070707, otherAlliance.Leader.Name); // You need to negotiate via ~1_val~ instead.
							}
							else
							{
								pm.SendGump(new WarDeclarationGump(pm, guild, m_Other));
							}
						}
						break;
					}
				#endregion
				case 2: //Request Alliance
					{
						#region New alliance
						if (alliance == null)
						{
							if (!playerRank.GetFlag(RankFlags.AllianceControl))
							{
								pm.SendLocalizedMessage(1070747); // You don't have permission to create an alliance.
							}
							else if (Faction.Find(guild.Leader) != Faction.Find(m_Other.Leader))
							{
								pm.SendLocalizedMessage(1070758); // You cannot propose an alliance to a guild with a different faction allegiance.
							}
							else if (otherAlliance != null)
							{
								if (otherAlliance.IsPendingMember(m_Other))
								{
									pm.SendLocalizedMessage(1063416, m_Other.Name); // ~1_val~ is currently considering another alliance proposal.
								}
								else
								{
									pm.SendLocalizedMessage(1063426, m_Other.Name); // ~1_val~ already belongs to an alliance.
								}
							}
							else if (m_Other.AcceptedWars.Count > 0 || m_Other.PendingWars.Count > 0)
							{
								pm.SendLocalizedMessage(1063427, m_Other.Name); // ~1_val~ is currently involved in a guild war.
							}
							else if (guild.AcceptedWars.Count > 0 || guild.PendingWars.Count > 0)
							{
								pm.SendLocalizedMessage(1063427, guild.Name); // ~1_val~ is currently involved in a guild war.
							}
							else
							{
								pm.SendLocalizedMessage(1063439); // Enter a name for the new alliance:
								pm.BeginPrompt(new PromptCallback(CreateAlliance_Callback));
							}
						}
						#endregion
						#region Existing Alliance
						else
						{
							if (!playerRank.GetFlag(RankFlags.AllianceControl))
							{
								pm.SendLocalizedMessage(1063436); // You don't have permission to negotiate an alliance.
							}
							else if (alliance.Leader != guild)
							{
								pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", guild.Name, alliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
							}
							else if (otherAlliance != null)
							{
								if (otherAlliance.IsPendingMember(m_Other))
								{
									pm.SendLocalizedMessage(1063416, m_Other.Name); // ~1_val~ is currently considering another alliance proposal.
								}
								else
								{
									pm.SendLocalizedMessage(1063426, m_Other.Name); // ~1_val~ already belongs to an alliance.
								}
							}
							else if (alliance.IsPendingMember(guild))
							{
								pm.SendLocalizedMessage(1063416, guild.Name); // ~1_val~ is currently considering another alliance proposal.
							}
							else if (m_Other.AcceptedWars.Count > 0 || m_Other.PendingWars.Count > 0)
							{
								pm.SendLocalizedMessage(1063427, m_Other.Name); // ~1_val~ is currently involved in a guild war.
							}
							else if (guild.AcceptedWars.Count > 0 || guild.PendingWars.Count > 0)
							{
								pm.SendLocalizedMessage(1063427, guild.Name); // ~1_val~ is currently involved in a guild war.
							}
							else if (Faction.Find(guild.Leader) != Faction.Find(m_Other.Leader))
							{
								pm.SendLocalizedMessage(1070758); // You cannot propose an alliance to a guild with a different faction allegiance.
							}
							else
							{
								pm.SendLocalizedMessage(1070750, m_Other.Name); // An invitation to join your alliance has been sent to ~1_val~.

								m_Other.GuildMessage(1070780, guild.Name); // ~1_val~ has proposed an alliance.

								m_Other.Alliance = alliance;    //Calls addPendingGuild
																//alliance.AddPendingGuild( m_Other );
							}
						}
						#endregion
						break;
					}
				case 10:    //Show Alliance Roster
					{
						if (alliance != null && alliance == otherAlliance)
						{
							pm.SendGump(new AllianceInfo.AllianceRosterGump(pm, guild, alliance));
						}

						break;
					}
				case 11:    //Leave Alliance
					{
						if (!playerRank.GetFlag(RankFlags.AllianceControl))
						{
							pm.SendLocalizedMessage(1063436); // You don't have permission to negotiate an alliance.
						}
						else if (alliance != null && alliance.IsMember(guild))
						{
							guild.Alliance = null;  //Calls alliance.Removeguild
													//						alliance.RemoveGuild( guild );

							m_Other.InvalidateWarNotoriety();

							guild.InvalidateMemberNotoriety();
						}
						break;
					}
				case 12:    //Remove Guild from alliance
					{
						if (!playerRank.GetFlag(RankFlags.AllianceControl))
						{
							pm.SendLocalizedMessage(1063436); // You don't have permission to negotiate an alliance.
						}
						else if (alliance != null && alliance.Leader != guild)
						{
							pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", guild.Name, alliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
						}
						else if (alliance != null && alliance.IsMember(guild) && alliance.IsMember(m_Other))
						{
							m_Other.Alliance = null;

							m_Other.InvalidateMemberNotoriety();

							guild.InvalidateWarNotoriety();
						}
						break;
					}
				case 13:    //Promote to Alliance leader
					{
						if (!playerRank.GetFlag(RankFlags.AllianceControl))
						{
							pm.SendLocalizedMessage(1063436); // You don't have permission to negotiate an alliance.
						}
						else if (alliance != null && alliance.Leader != guild)
						{
							pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", guild.Name, alliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
						}
						else if (alliance != null && alliance.IsMember(guild) && alliance.IsMember(m_Other))
						{
							pm.SendLocalizedMessage(1063434, String.Format("{0}\t{1}", m_Other.Name, alliance.Name)); // ~1_val~ is now the leader of ~2_val~.

							alliance.Leader = m_Other;
						}
						break;
					}
				case 14:    //Withdraw Request
					{
						if (!playerRank.GetFlag(RankFlags.AllianceControl))
						{
							pm.SendLocalizedMessage(1063436); // You don't have permission to negotiate an alliance.
						}
						else if (alliance != null && alliance.Leader == guild && alliance.IsPendingMember(m_Other))
						{
							m_Other.Alliance = null;
							pm.SendLocalizedMessage(1070752); // The proposal has been updated.
						}
						break;
					}
				case 15: //Deny Alliance Request
					{
						if (!playerRank.GetFlag(RankFlags.AllianceControl))
						{
							pm.SendLocalizedMessage(1063436); // You don't have permission to negotiate an alliance.
						}
						else if (alliance != null && otherAlliance != null && alliance.Leader == m_Other && otherAlliance.IsPendingMember(guild))
						{
							pm.SendLocalizedMessage(1070752); // The proposal has been updated.
															  //m_Other.GuildMessage( 1070782 ); // ~1_val~ has responded to your proposal.	//Per OSI commented out.

							guild.Alliance = null;
						}
						break;
					}
				case 16: //Accept Alliance Request
					{
						if (!playerRank.GetFlag(RankFlags.AllianceControl))
						{
							pm.SendLocalizedMessage(1063436); // You don't have permission to negotiate an alliance.
						}
						else if (otherAlliance != null && otherAlliance.Leader == m_Other && otherAlliance.IsPendingMember(guild))
						{
							pm.SendLocalizedMessage(1070752); // The proposal has been updated.

							otherAlliance.TurnToMember(m_Other); //No need to verify it's in the guild or already a member, the function does this

							otherAlliance.TurnToMember(guild);
						}
						break;
					}
			}
		}

		public void CreateAlliance_Callback(Mobile from, string text)
		{
			var pm = from as PlayerMobile;


			var alliance = guild.Alliance;
			var otherAlliance = m_Other.Alliance;

			if (!IsMember(from, guild) || alliance != null)
			{
				return;
			}

			var playerRank = pm.GuildRank;


			if (!playerRank.GetFlag(RankFlags.AllianceControl))
			{
				pm.SendLocalizedMessage(1070747); // You don't have permission to create an alliance.
			}
			else if (Faction.Find(guild.Leader) != Faction.Find(m_Other.Leader))
			{
				//Notes about this: OSI only cares/checks when proposing, you can change your faction all you want later.  
				pm.SendLocalizedMessage(1070758); // You cannot propose an alliance to a guild with a different faction allegiance.
			}
			else if (otherAlliance != null)
			{
				if (otherAlliance.IsPendingMember(m_Other))
				{
					pm.SendLocalizedMessage(1063416, m_Other.Name); // ~1_val~ is currently considering another alliance proposal.
				}
				else
				{
					pm.SendLocalizedMessage(1063426, m_Other.Name); // ~1_val~ already belongs to an alliance.
				}
			}
			else if (m_Other.AcceptedWars.Count > 0 || m_Other.PendingWars.Count > 0)
			{
				pm.SendLocalizedMessage(1063427, m_Other.Name); // ~1_val~ is currently involved in a guild war.
			}
			else if (guild.AcceptedWars.Count > 0 || guild.PendingWars.Count > 0)
			{
				pm.SendLocalizedMessage(1063427, guild.Name); // ~1_val~ is currently involved in a guild war.
			}
			else
			{
				var name = Utility.FixHtml(text.Trim());

				if (!BaseGuildGump.CheckProfanity(name))
				{
					pm.SendLocalizedMessage(1070886); // That alliance name is not allowed.
				}
				else if (name.Length > Guild.NameLimit)
				{
					pm.SendLocalizedMessage(1070887, Guild.NameLimit.ToString()); // An alliance name cannot exceed ~1_val~ characters in length.
				}
				else if (AllianceInfo.Alliances.ContainsKey(name.ToLower()))
				{
					pm.SendLocalizedMessage(1063428); // That alliance name is not available.
				}
				else
				{
					pm.SendLocalizedMessage(1070750, m_Other.Name); // An invitation to join your alliance has been sent to ~1_val~.

					m_Other.GuildMessage(1070780, guild.Name); // ~1_val~ has proposed an alliance.

					new AllianceInfo(guild, name, m_Other);
				}
			}
		}
	}

	/// Guild Diplomacy
	public enum GuildDisplayType
	{
		All,
		AwaitingAction,
		Relations
	}

	public class GuildDiplomacyGump : BaseGuildListGump<Guild>
	{
		protected virtual bool AllowAdvancedSearch => true;
		#region Comparers
		private class NameComparer : IComparer<Guild>
		{
			public static readonly IComparer<Guild> Instance = new NameComparer();

			public NameComparer()
			{
			}

			public int Compare(Guild x, Guild y)
			{
				if (x == null && y == null)
				{
					return 0;
				}
				else if (x == null)
				{
					return -1;
				}
				else if (y == null)
				{
					return 1;
				}

				return Insensitive.Compare(x.Name, y.Name);
			}
		}

		private class StatusComparer : IComparer<Guild>
		{
			private enum GuildCompareStatus
			{
				Peace,
				Ally,
				War
			}
			private readonly Guild m_Guild;
			public StatusComparer(Guild g)
			{
				m_Guild = g;
			}

			public int Compare(Guild x, Guild y)
			{
				if (x == null && y == null)
				{
					return 0;
				}
				else if (x == null)
				{
					return -1;
				}
				else if (y == null)
				{
					return 1;
				}

				var aStatus = GuildCompareStatus.Peace;
				var bStatus = GuildCompareStatus.Peace;

				if (m_Guild.IsAlly(x))
				{
					aStatus = GuildCompareStatus.Ally;
				}
				else if (m_Guild.IsWar(x))
				{
					aStatus = GuildCompareStatus.War;
				}

				if (m_Guild.IsAlly(y))
				{
					bStatus = GuildCompareStatus.Ally;
				}
				else if (m_Guild.IsWar(y))
				{
					bStatus = GuildCompareStatus.War;
				}

				return ((int)aStatus).CompareTo((int)bStatus);
			}
		}
		private class AbbrevComparer : IComparer<Guild>
		{
			public static readonly IComparer<Guild> Instance = new AbbrevComparer();

			public AbbrevComparer()
			{
			}

			public int Compare(Guild x, Guild y)
			{
				if (x == null && y == null)
				{
					return 0;
				}
				else if (x == null)
				{
					return -1;
				}
				else if (y == null)
				{
					return 1;
				}

				return Insensitive.Compare(x.Abbreviation, y.Abbreviation);
			}
		}

		#endregion

		private GuildDisplayType m_Display;
		private readonly TextDefinition m_LowerText;

		public GuildDiplomacyGump(PlayerMobile pm, Guild g)
			: this(pm, g, GuildDiplomacyGump.NameComparer.Instance, true, "", 0, GuildDisplayType.All, Utility.CastConvertList<BaseGuild, Guild>(new List<BaseGuild>(Guild.List.Values)), (1063136 + (int)GuildDisplayType.All))
		{
		}

		public GuildDiplomacyGump(PlayerMobile pm, Guild g, IComparer<Guild> currentComparer, bool ascending, string filter, int startNumber, GuildDisplayType display)
			: this(pm, g, currentComparer, ascending, filter, startNumber, display, Utility.CastConvertList<BaseGuild, Guild>(new List<BaseGuild>(Guild.List.Values)), (1063136 + (int)display))
		{
		}

		public GuildDiplomacyGump(PlayerMobile pm, Guild g, IComparer<Guild> currentComparer, bool ascending, string filter, int startNumber, List<Guild> list, TextDefinition lowerText)
			: this(pm, g, currentComparer, ascending, filter, startNumber, GuildDisplayType.All, list, lowerText)
		{
		}

		public GuildDiplomacyGump(PlayerMobile pm, Guild g, bool ascending, string filter, int startNumber, List<Guild> list, TextDefinition lowerText)
			: this(pm, g, GuildDiplomacyGump.NameComparer.Instance, ascending, filter, startNumber, GuildDisplayType.All, list, lowerText)
		{
		}

		public GuildDiplomacyGump(PlayerMobile pm, Guild g, IComparer<Guild> currentComparer, bool ascending, string filter, int startNumber, GuildDisplayType display, List<Guild> list, TextDefinition lowerText)
			: base(pm, g, list, currentComparer, ascending, filter, startNumber,
			new InfoField<Guild>[]
			{
				new InfoField<Guild>( 1062954, 280, GuildDiplomacyGump.NameComparer.Instance    ),	//Guild Name
				new InfoField<Guild>( 1062957, 50,  GuildDiplomacyGump.AbbrevComparer.Instance  ),	//Abbrev
				new InfoField<Guild>( 1062958, 120, new GuildDiplomacyGump.StatusComparer( g )  )	//Guild Title
			})
		{

			m_Display = display;
			m_LowerText = lowerText;
			PopulateGump();
		}

		public override void PopulateGump()
		{
			base.PopulateGump();

			AddHtmlLocalized(431, 43, 110, 26, 1062978, 0xF, false, false); // Diplomacy			
		}

		protected override TextDefinition[] GetValuesFor(Guild g, int aryLength)
		{
			var defs = new TextDefinition[aryLength];

			defs[0] = (g == guild) ? Color(g.Name, 0x006600) : g.Name;
			defs[1] = g.Abbreviation;

			defs[2] = 3000085; //Peace


			if (guild.IsAlly(g))
			{
				if (guild.Alliance.Leader == g)
				{
					defs[2] = 1063237; // Alliance Leader
				}
				else
				{
					defs[2] = 1062964; // Ally
				}
			}
			else if (guild.IsWar(g))
			{
				defs[2] = 3000086; // War
			}

			return defs;
		}

		public override bool HasRelationship(Guild g)
		{
			if (g == guild)
			{
				return false;
			}

			if (guild.FindPendingWar(g) != null)
			{
				return true;
			}

			var alliance = guild.Alliance;

			if (alliance != null)
			{
				var leader = alliance.Leader;

				if (leader != null)
				{
					if (guild == leader && alliance.IsPendingMember(g) || g == leader && alliance.IsPendingMember(guild))
					{
						return true;
					}
				}
				else if (alliance.IsPendingMember(g))
				{
					return true;
				}
			}

			return false;
		}

		public override void DrawEndingEntry(int itemNumber)
		{
			//AddHtmlLocalized( 66, 153 + itemNumber * 28, 280, 26, 1063136 + (int)m_Display, 0xF, false, false ); // Showing All Guilds/Awaiting Action/ w/Relation Ship
			//AddHtmlText( 66, 153 + itemNumber * 28, 280, 26, m_LowerText, false, false );

			if (m_LowerText != null && m_LowerText.Number > 0)
			{
				AddHtmlLocalized(66, 153 + itemNumber * 28, 280, 26, m_LowerText.Number, 0xF, false, false);
			}
			else if (m_LowerText != null && m_LowerText.String != null)
			{
				AddHtml(66, 153 + itemNumber * 28, 280, 26, Color(m_LowerText.String, 0x99), false, false);
			}

			if (AllowAdvancedSearch)
			{
				AddBackground(350, 148 + itemNumber * 28, 200, 26, 0x2486);
				AddButton(355, 153 + itemNumber * 28, 0x845, 0x846, 8, GumpButtonType.Reply, 0);
				AddHtmlLocalized(380, 151 + itemNumber * 28, 160, 26, 1063083, 0x0, false, false); // Advanced Search
			}
		}


		protected override bool IsFiltered(Guild g, string filter)
		{
			if (g == null)
			{
				return true;
			}

			switch (m_Display)
			{
				case GuildDisplayType.Relations:
					{
						//if( !( guild.IsWar( g ) || guild.IsAlly( g ) ) )

						if (!(guild.FindActiveWar(g) != null || guild.IsAlly(g)))   //As per OSI, only the guild leader wars show up under the sorting by relation
						{
							return true;
						}

						return false;
					}
				case GuildDisplayType.AwaitingAction:
					{
						return !HasRelationship(g);
					}
			}

			return !(Insensitive.Contains(g.Name, filter) || Insensitive.Contains(g.Abbreviation, filter));
		}

		public override bool WillFilter
		{
			get
			{
				if (m_Display == GuildDisplayType.All)
				{
					return base.WillFilter;
				}

				return true;
			}
		}


		public override Gump GetResentGump(PlayerMobile pm, Guild g, IComparer<Guild> comparer, bool ascending, string filter, int startNumber)
		{
			return new GuildDiplomacyGump(pm, g, comparer, ascending, filter, startNumber, m_Display);
		}

		public override Gump GetObjectInfoGump(PlayerMobile pm, Guild g, Guild o)
		{
			if (guild == o)
			{
				return new GuildInfoGump(pm, g);
			}

			return new OtherGuildInfo(pm, g, o);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			base.OnResponse(sender, info);

			var pm = sender.Mobile as PlayerMobile;

			if (pm == null || !IsMember(pm, guild))
			{
				return;
			}

			if (AllowAdvancedSearch && info.ButtonID == 8)
			{
				pm.SendGump(new GuildAdvancedSearchGump(pm, guild, m_Display, new SearchSelectionCallback(AdvancedSearch_Callback)));
			}
		}

		public void AdvancedSearch_Callback(GuildDisplayType display)
		{
			m_Display = display;
			ResendGump();
		}
	}

	public class WarDeclarationGump : BaseGuildGump
	{
		private readonly Guild m_Other;

		public WarDeclarationGump(PlayerMobile pm, Guild g, Guild otherGuild) : base(pm, g)
		{
			m_Other = otherGuild;
			var war = g.FindPendingWar(otherGuild);

			AddPage(0);

			AddBackground(0, 0, 500, 340, 0x24AE);
			AddBackground(65, 50, 370, 30, 0x2486);
			AddHtmlLocalized(75, 55, 370, 26, 1062979, 0x3C00, false, false); // <div align=center><i>Declaration of War</i></div>
			AddImage(410, 45, 0x232C);
			AddHtmlLocalized(65, 95, 200, 20, 1063009, 0x14AF, false, false); // <i>Duration of War</i>
			AddHtmlLocalized(65, 120, 400, 20, 1063010, 0x0, false, false); // Enter the number of hours the war will last.
			AddBackground(65, 150, 40, 30, 0x2486);
			AddTextEntry(70, 154, 50, 30, 0x481, 10, (war != null) ? war.WarLength.Hours.ToString() : "0");
			AddHtmlLocalized(65, 195, 200, 20, 1063011, 0x14AF, false, false); // <i>Victory Condition</i>
			AddHtmlLocalized(65, 220, 400, 20, 1063012, 0x0, false, false); // Enter the winning number of kills.
			AddBackground(65, 250, 40, 30, 0x2486);
			AddTextEntry(70, 254, 50, 30, 0x481, 11, (war != null) ? war.MaxKills.ToString() : "0");
			AddBackground(190, 270, 130, 26, 0x2486);
			AddButton(195, 275, 0x845, 0x846, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(220, 273, 90, 26, 1006045, 0x0, false, false); // Cancel
			AddBackground(330, 270, 130, 26, 0x2486);
			AddButton(335, 275, 0x845, 0x846, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(360, 273, 90, 26, 1062989, 0x5000, false, false); // Declare War!
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{

			var pm = sender.Mobile as PlayerMobile;

			if (!IsMember(pm, guild))
			{
				return;
			}

			var playerRank = pm.GuildRank;

			switch (info.ButtonID)
			{
				case 1:
					{
						var alliance = guild.Alliance;
						var otherAlliance = m_Other.Alliance;

						if (!playerRank.GetFlag(RankFlags.ControlWarStatus))
						{
							pm.SendLocalizedMessage(1063440); // You don't have permission to negotiate wars.
						}
						else if (alliance != null && alliance.Leader != guild)
						{
							pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", guild.Name, alliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
							pm.SendLocalizedMessage(1070707, alliance.Leader.Name); // You need to negotiate via ~1_val~ instead.
						}
						else if (otherAlliance != null && otherAlliance.Leader != m_Other)
						{
							pm.SendLocalizedMessage(1063239, String.Format("{0}\t{1}", m_Other.Name, otherAlliance.Name)); // ~1_val~ is not the leader of the ~2_val~ alliance.
							pm.SendLocalizedMessage(1070707, otherAlliance.Leader.Name); // You need to negotiate via ~1_val~ instead.
						}
						else
						{
							var activeWar = guild.FindActiveWar(m_Other);

							if (activeWar == null)
							{
								var war = guild.FindPendingWar(m_Other);
								var otherWar = m_Other.FindPendingWar(guild);

								//Note: OSI differs from what it says on website.  unlimited war = 0 kills/ 0 hrs.  Not > 999.  (sidenote: they both cap at 65535, 7.5 years, but, still.)
								var tKills = info.GetTextEntry(11);
								var tWarLength = info.GetTextEntry(10);

								var maxKills = (tKills == null) ? 0 : Math.Max(Math.Min(Utility.ToInt32(info.GetTextEntry(11).Text), 0xFFFF), 0);
								var warLength = TimeSpan.FromHours((tWarLength == null) ? 0 : Math.Max(Math.Min(Utility.ToInt32(info.GetTextEntry(10).Text), 0xFFFF), 0));

								if (war != null)
								{
									war.MaxKills = maxKills;
									war.WarLength = warLength;
									war.WarRequester = true;
								}
								else
								{
									guild.PendingWars.Add(new WarDeclaration(guild, m_Other, maxKills, warLength, true));
								}

								if (otherWar != null)
								{
									otherWar.MaxKills = maxKills;
									otherWar.WarLength = warLength;
									otherWar.WarRequester = false;
								}
								else
								{
									m_Other.PendingWars.Add(new WarDeclaration(m_Other, guild, maxKills, warLength, false));
								}

								if (war != null)
								{
									pm.SendLocalizedMessage(1070752); // The proposal has been updated.
																	  //m_Other.GuildMessage( 1070782 ); // ~1_val~ has responded to your proposal.
								}
								else
								{
									m_Other.GuildMessage(1070781, ((guild.Alliance != null) ? guild.Alliance.Name : guild.Name)); // ~1_val~ has proposed a war.
								}

								pm.SendLocalizedMessage(1070751, ((m_Other.Alliance != null) ? m_Other.Alliance.Name : m_Other.Name)); // War proposal has been sent to ~1_val~.
							}
						}
						break;
					}
				default:
					{
						pm.SendGump(new OtherGuildInfo(pm, guild, m_Other));
						break;
					}
			}
		}
	}

	#endregion
}

namespace Server.Gumps
{
	public abstract class GuildListGump : Gump
	{
		protected Mobile m_Mobile;
		protected Guild m_Guild;
		protected List<Guild> m_List;

		public GuildListGump(Mobile from, Guild guild, bool radio, List<Guild> list) : base(20, 30)
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage(0);
			AddBackground(0, 0, 550, 440, 5054);
			AddBackground(10, 10, 530, 420, 3000);

			Design();

			m_List = new List<Guild>(list);

			for (var i = 0; i < m_List.Count; ++i)
			{
				if ((i % 11) == 0)
				{
					if (i != 0)
					{
						AddButton(300, 370, 4005, 4007, 0, GumpButtonType.Page, (i / 11) + 1);
						AddHtmlLocalized(335, 370, 300, 35, 1011066, false, false); // Next page
					}

					AddPage((i / 11) + 1);

					if (i != 0)
					{
						AddButton(20, 370, 4014, 4016, 0, GumpButtonType.Page, (i / 11));
						AddHtmlLocalized(55, 370, 300, 35, 1011067, false, false); // Previous page
					}
				}

				if (radio)
				{
					AddRadio(20, 35 + ((i % 11) * 30), 208, 209, false, i);
				}

				var g = m_List[i];

				string name;

				if ((name = g.Name) != null && (name = name.Trim()).Length <= 0)
				{
					name = "(empty)";
				}

				AddLabel((radio ? 55 : 20), 35 + ((i % 11) * 30), 0, name);
			}
		}

		protected virtual void Design()
		{
		}
	}

	public abstract class GuildMobileListGump : Gump
	{
		protected Mobile m_Mobile;
		protected Guild m_Guild;
		protected List<Mobile> m_List;

		public GuildMobileListGump(Mobile from, Guild guild, bool radio, List<Mobile> list)
			: base(20, 30)
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage(0);
			AddBackground(0, 0, 550, 440, 5054);
			AddBackground(10, 10, 530, 420, 3000);

			Design();

			m_List = new List<Mobile>(list);

			for (var i = 0; i < m_List.Count; ++i)
			{
				if ((i % 11) == 0)
				{
					if (i != 0)
					{
						AddButton(300, 370, 4005, 4007, 0, GumpButtonType.Page, (i / 11) + 1);
						AddHtmlLocalized(335, 370, 300, 35, 1011066, false, false); // Next page
					}

					AddPage((i / 11) + 1);

					if (i != 0)
					{
						AddButton(20, 370, 4014, 4016, 0, GumpButtonType.Page, (i / 11));
						AddHtmlLocalized(55, 370, 300, 35, 1011067, false, false); // Previous page
					}
				}

				if (radio)
				{
					AddRadio(20, 35 + ((i % 11) * 30), 208, 209, false, i);
				}

				var m = m_List[i];

				string name;

				if ((name = m.Name) != null && (name = name.Trim()).Length <= 0)
				{
					name = "(empty)";
				}

				AddLabel((radio ? 55 : 20), 35 + ((i % 11) * 30), 0, name);
			}
		}

		protected virtual void Design()
		{
		}
	}

	#region GuildmasterGump Option

	public class GuildmasterGump : Gump
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildmasterGump(Mobile from, Guild guild) : base(20, 30)
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage(0);
			AddBackground(0, 0, 550, 400, 5054);
			AddBackground(10, 10, 530, 380, 3000);

			AddHtmlLocalized(20, 15, 510, 35, 1011121, false, false); // <center>GUILDMASTER FUNCTIONS</center>

			AddButton(20, 40, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 40, 470, 30, 1011107, false, false); // Set the guild name.

			AddButton(20, 70, 4005, 4007, 3, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 70, 470, 30, 1011109, false, false); // Set the guild's abbreviation.

			if (Guild.OrderChaos)
			{
				AddButton(20, 100, 4005, 4007, 4, GumpButtonType.Reply, 0);

				switch (m_Guild.Type)
				{
					case GuildType.Regular:
						AddHtmlLocalized(55, 100, 470, 30, 1013059, false, false); // Change guild type: Currently Standard
						break;
					case GuildType.Order:
						AddHtmlLocalized(55, 100, 470, 30, 1013057, false, false); // Change guild type: Currently Order
						break;
					case GuildType.Chaos:
						AddHtmlLocalized(55, 100, 470, 30, 1013058, false, false); // Change guild type: Currently Chaos
						break;
				}
			}

			AddButton(20, 130, 4005, 4007, 5, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 130, 470, 30, 1011112, false, false); // Set the guild's charter.

			AddButton(20, 160, 4005, 4007, 6, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 160, 470, 30, 1011113, false, false); // Dismiss a member.

			AddButton(20, 190, 4005, 4007, 7, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 190, 470, 30, 1011114, false, false); // Go to the WAR menu.

			if (m_Guild.Candidates.Count > 0)
			{
				AddButton(20, 220, 4005, 4007, 8, GumpButtonType.Reply, 0);
				AddHtmlLocalized(55, 220, 470, 30, 1013056, false, false); // Administer the list of candidates
			}
			else
			{
				AddImage(20, 220, 4020);
				AddHtmlLocalized(55, 220, 470, 30, 1013031, false, false); // There are currently no candidates for membership.
			}

			AddButton(20, 250, 4005, 4007, 9, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 250, 470, 30, 1011117, false, false); // Set the guildmaster's title.

			AddButton(20, 280, 4005, 4007, 10, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 280, 470, 30, 1011118, false, false); // Grant a title to another member.

			AddButton(20, 310, 4005, 4007, 11, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 310, 470, 30, 1011119, false, false); // Move this guildstone.

			AddButton(20, 360, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 360, 245, 30, 1011120, false, false); // Return to the main menu.

			AddButton(300, 360, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 360, 100, 30, 1011441, false, false); // EXIT
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Main menu
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildGump(m_Mobile, m_Guild));

						break;
					}
				case 2: // Set guild name
					{
						m_Mobile.SendLocalizedMessage(1013060); // Enter new guild name (40 characters max):
						m_Mobile.Prompt = new GuildNamePrompt(m_Mobile, m_Guild);

						break;
					}
				case 3: // Set guild abbreviation
					{
						m_Mobile.SendLocalizedMessage(1013061); // Enter new guild abbreviation (3 characters max):
						m_Mobile.Prompt = new GuildAbbrvPrompt(m_Mobile, m_Guild);

						break;
					}
				case 4: // Change guild type
					{
						if (!Guild.OrderChaos)
						{
							return;
						}

						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildChangeTypeGump(m_Mobile, m_Guild));

						break;
					}
				case 5: // Set charter
					{
						m_Mobile.SendLocalizedMessage(1013071); // Enter the new guild charter (50 characters max):
						m_Mobile.Prompt = new GuildCharterPrompt(m_Mobile, m_Guild);

						break;
					}
				case 6: // Dismiss member
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildDismissGump(m_Mobile, m_Guild));

						break;
					}
				case 7: // War menu
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildWarAdminGump(m_Mobile, m_Guild));

						break;
					}
				case 8: // Administer candidates
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildAdminCandidatesGump(m_Mobile, m_Guild));

						break;
					}
				case 9: // Set guildmaster's title
					{
						m_Mobile.SendLocalizedMessage(1013073); // Enter new guildmaster title (20 characters max):
						m_Mobile.Prompt = new GuildTitlePrompt(m_Mobile, m_Mobile, m_Guild);

						break;
					}
				case 10: // Grant title
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GrantGuildTitleGump(m_Mobile, m_Guild));

						break;
					}
				case 11: // Move guildstone
					{
						if (m_Guild.Guildstone != null)
						{
							var item = new GuildTeleporter(m_Guild.Guildstone);

							if (m_Guild.Teleporter != null)
							{
								m_Guild.Teleporter.Delete();
							}

							m_Mobile.SendLocalizedMessage(501133); // Use the teleporting object placed in your backpack to move this guildstone.

							m_Mobile.AddToBackpack(item);
							m_Guild.Teleporter = item;
						}

						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));

						break;
					}
			}
		}
	}

	/// GuildmasterGump: Case 1
	public class GuildGump : Gump
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildGump(Mobile beholder, Guild guild) : base(20, 30)
		{
			m_Mobile = beholder;
			m_Guild = guild;

			Dragable = false;

			AddPage(0);
			AddBackground(0, 0, 550, 400, 5054);
			AddBackground(10, 10, 530, 380, 3000);

			AddHtml(20, 15, 200, 35, guild.Name, false, false);

			var leader = guild.Leader;

			if (leader != null)
			{
				string leadTitle;

				if ((leadTitle = leader.GuildTitle) != null && (leadTitle = leadTitle.Trim()).Length > 0)
				{
					leadTitle += ": ";
				}
				else
				{
					leadTitle = "";
				}

				string leadName;

				if ((leadName = leader.Name) == null || (leadName = leadName.Trim()).Length <= 0)
				{
					leadName = "(empty)";
				}

				AddHtml(220, 15, 250, 35, leadTitle + leadName, false, false);
			}

			AddButton(20, 50, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 50, 100, 20, 1013022, false, false); // Loyal to

			var fealty = beholder.GuildFealty;

			if (fealty == null || !guild.IsMember(fealty))
			{
				fealty = leader;
			}

			if (fealty == null)
			{
				fealty = beholder;
			}

			string fealtyName;

			if (fealty == null || (fealtyName = fealty.Name) == null || (fealtyName = fealtyName.Trim()).Length <= 0)
			{
				fealtyName = "(empty)";
			}

			if (beholder == fealty)
			{
				AddHtmlLocalized(55, 70, 470, 20, 1018002, false, false); // yourself
			}
			else
			{
				AddHtml(55, 70, 470, 20, fealtyName, false, false);
			}

			AddButton(215, 50, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(250, 50, 170, 20, 1013023, false, false); // Display guild abbreviation
			AddHtmlLocalized(250, 70, 50, 20, beholder.DisplayGuildTitle ? 1011262 : 1011263, false, false); // on/off

			AddButton(20, 100, 4005, 4007, 3, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 100, 470, 30, 1011086, false, false); // View the current roster.

			AddButton(20, 130, 4005, 4007, 4, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 130, 470, 30, 1011085, false, false); // Recruit someone into the guild.

			if (guild.Candidates.Count > 0)
			{
				AddButton(20, 160, 4005, 4007, 5, GumpButtonType.Reply, 0);
				AddHtmlLocalized(55, 160, 470, 30, 1011093, false, false); // View list of candidates who have been sponsored to the guild.
			}
			else
			{
				AddImage(20, 160, 4020);
				AddHtmlLocalized(55, 160, 470, 30, 1013031, false, false); // There are currently no candidates for membership.
			}

			AddButton(20, 220, 4005, 4007, 6, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 220, 470, 30, 1011087, false, false); // View the guild's charter.

			AddButton(20, 250, 4005, 4007, 7, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 250, 470, 30, 1011092, false, false); // Resign from the guild.

			AddButton(20, 280, 4005, 4007, 8, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 280, 470, 30, 1011095, false, false); // View list of guilds you are at war with.

			if (beholder.AccessLevel >= AccessLevel.GameMaster || beholder == leader)
			{
				AddButton(20, 310, 4005, 4007, 9, GumpButtonType.Reply, 0);
				AddHtmlLocalized(55, 310, 470, 30, 1011094, false, false); // Access guildmaster functions.
			}
			else
			{
				AddImage(20, 310, 4020);
				AddHtmlLocalized(55, 310, 470, 30, 1018013, false, false); // Reserved for guildmaster
			}

			AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 360, 470, 30, 1011441, false, false); // EXIT
		}

		public static void EnsureClosed(Mobile m)
		{
			m.CloseGump(typeof(DeclareFealtyGump));
			m.CloseGump(typeof(GrantGuildTitleGump));
			m.CloseGump(typeof(GuildAdminCandidatesGump));
			m.CloseGump(typeof(GuildCandidatesGump));
			m.CloseGump(typeof(GuildChangeTypeGump));
			m.CloseGump(typeof(GuildCharterGump));
			m.CloseGump(typeof(GuildDismissGump));
			m.CloseGump(typeof(GuildGump));
			m.CloseGump(typeof(GuildmasterGump));
			m.CloseGump(typeof(GuildRosterGump));
			m.CloseGump(typeof(GuildWarGump));
		}

		public static bool BadLeader(Mobile m, Guild g)
		{
			if (m.Deleted || g.Disbanded || (m.AccessLevel < AccessLevel.GameMaster && g.Leader != m))
			{
				return true;
			}

			var stone = g.Guildstone;

			return (stone == null || stone.Deleted || !m.InRange(stone.GetWorldLocation(), 2));
		}

		public static bool BadMember(Mobile m, Guild g)
		{
			if (m.Deleted || g.Disbanded || (m.AccessLevel < AccessLevel.GameMaster && !g.IsMember(m)))
			{
				return true;
			}

			var stone = g.Guildstone;

			return (stone == null || stone.Deleted || !m.InRange(stone.GetWorldLocation(), 2));
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (BadMember(m_Mobile, m_Guild))
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Loyalty
					{
						EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new DeclareFealtyGump(m_Mobile, m_Guild));

						break;
					}
				case 2: // Toggle display abbreviation
					{
						m_Mobile.DisplayGuildTitle = !m_Mobile.DisplayGuildTitle;

						EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildGump(m_Mobile, m_Guild));

						break;
					}
				case 3: // View the current roster
					{
						EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildRosterGump(m_Mobile, m_Guild));

						break;
					}
				case 4: // Recruit
					{
						m_Mobile.Target = new GuildRecruitTarget(m_Mobile, m_Guild);

						break;
					}
				case 5: // Membership candidates
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildCandidatesGump(m_Mobile, m_Guild));

						break;
					}
				case 6: // View charter
					{
						EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildCharterGump(m_Mobile, m_Guild));

						break;
					}
				case 7: // Resign
					{
						m_Guild.RemoveMember(m_Mobile);

						break;
					}
				case 8: // View wars
					{
						EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildWarGump(m_Mobile, m_Guild));

						break;
					}
				case 9: // Guildmaster functions
					{
						if (m_Mobile.AccessLevel >= AccessLevel.GameMaster || m_Guild.Leader == m_Mobile)
						{
							EnsureClosed(m_Mobile);
							m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
						}

						break;
					}
			}
		}
	}

	/// GuildmasterGump: Case 2
	public class GuildNamePrompt : Prompt
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildNamePrompt(Mobile m, Guild g)
		{
			m_Mobile = m;
			m_Guild = g;
		}

		public override void OnCancel(Mobile from)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			text = text.Trim();

			if (text.Length > 40)
			{
				text = text.Substring(0, 40);
			}

			if (text.Length > 0)
			{
				if (Guild.FindByName(text) != null)
				{
					m_Mobile.SendMessage("{0} conflicts with the name of an existing guild.", text);
				}
				else
				{
					m_Guild.Name = text;
					m_Guild.GuildMessage(1018024, true, text); // The name of your guild has changed:
				}
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
		}
	}

	/// GuildmasterGump: Case 3
	public class GuildAbbrvPrompt : Prompt
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildAbbrvPrompt(Mobile m, Guild g)
		{
			m_Mobile = m;
			m_Guild = g;
		}

		public override void OnCancel(Mobile from)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			text = text.Trim();

			if (text.Length > 3)
			{
				text = text.Substring(0, 3);
			}

			if (text.Length > 0)
			{
				if (Guild.FindByAbbrev(text) != null)
				{
					m_Mobile.SendMessage("{0} conflicts with the abbreviation of an existing guild.", text);
				}
				else
				{
					m_Guild.Abbreviation = text;
					m_Guild.GuildMessage(1018025, true, text); // Your guild abbreviation has changed:
				}
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
		}
	}

	/// GuildmasterGump: Case 4
	public class GuildChangeTypeGump : Gump
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildChangeTypeGump(Mobile from, Guild guild) : base(20, 30)
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage(0);
			AddBackground(0, 0, 550, 400, 5054);
			AddBackground(10, 10, 530, 380, 3000);

			AddHtmlLocalized(20, 15, 510, 30, 1013062, false, false); // <center>Change Guild Type Menu</center>

			AddHtmlLocalized(50, 50, 450, 30, 1013066, false, false); // Please select the type of guild you would like to change to

			AddButton(20, 100, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(85, 100, 300, 30, 1013063, false, false); // Standard guild

			AddButton(20, 150, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddItem(50, 143, 7109);
			AddHtmlLocalized(85, 150, 300, 300, 1013064, false, false); // Order guild

			AddButton(20, 200, 4005, 4007, 3, GumpButtonType.Reply, 0);
			AddItem(45, 200, 7107);
			AddHtmlLocalized(85, 200, 300, 300, 1013065, false, false); // Chaos guild

			AddButton(300, 360, 4005, 4007, 4, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 360, 150, 30, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if ((Guild.NewGuildSystem && !BaseGuildGump.IsLeader(m_Mobile, m_Guild)) || (!Guild.NewGuildSystem && GuildGump.BadLeader(m_Mobile, m_Guild)))
			{
				return;
			}

			GuildType newType;

			switch (info.ButtonID)
			{
				default: newType = m_Guild.Type; break;
				case 1: newType = GuildType.Regular; break;
				case 2: newType = GuildType.Order; break;
				case 3: newType = GuildType.Chaos; break;
			}

			if (m_Guild.Type != newType)
			{
				var pl = PlayerState.Find(m_Mobile);

				if (pl != null)
				{
					m_Mobile.SendLocalizedMessage(1010405); // You cannot change guild types while in a Faction!
				}
				else if (m_Guild.TypeLastChange.AddDays(7) > DateTime.UtcNow)
				{
					m_Mobile.SendLocalizedMessage(1011142); // You have already changed your guild type recently.
															// TODO: Clilocs 1011142-1011145 suggest a timer for pending changes
				}
				else
				{
					m_Guild.Type = newType;
					m_Guild.GuildMessage(1018022, true, newType.ToString()); // Guild Message: Your guild type has changed:
				}
			}

			if (Guild.NewGuildSystem)
			{
				if (m_Mobile is PlayerMobile)
				{
					m_Mobile.SendGump(new GuildInfoGump((PlayerMobile)m_Mobile, m_Guild));
				}

				return;
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
		}
	}

	/// GuildmasterGump: Case 5
	public class GuildCharterPrompt : Prompt
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildCharterPrompt(Mobile m, Guild g)
		{
			m_Mobile = m;
			m_Guild = g;
		}

		public override void OnCancel(Mobile from)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			text = text.Trim();

			if (text.Length > 50)
			{
				text = text.Substring(0, 50);
			}

			if (text.Length > 0)
			{
				m_Guild.Charter = text;
			}

			m_Mobile.SendLocalizedMessage(1013072); // Enter the new website for the guild (50 characters max):
			m_Mobile.Prompt = new GuildWebsitePrompt(m_Mobile, m_Guild);

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
		}
	}

	/// GuildmasterGump: Case 6
	public class GuildDismissGump : GuildMobileListGump
	{
		public GuildDismissGump(Mobile from, Guild guild) : base(from, guild, true, guild.Members)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 400, 35, 1011124, false, false); // Whom do you wish to dismiss?

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 245, 30, 1011125, false, false); // Kick them out!

			AddButton(300, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 100, 35, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				var switches = info.Switches;

				if (switches.Length > 0)
				{
					var index = switches[0];

					if (index >= 0 && index < m_List.Count)
					{
						var m = m_List[index];

						if (m != null && !m.Deleted)
						{
							m_Guild.RemoveMember(m);

							if (m_Mobile.AccessLevel >= AccessLevel.GameMaster || m_Mobile == m_Guild.Leader)
							{
								GuildGump.EnsureClosed(m_Mobile);
								m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
							}
						}
					}
				}
			}
			else if (info.ButtonID == 2 && (m_Mobile.AccessLevel >= AccessLevel.GameMaster || m_Mobile == m_Guild.Leader))
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
			}
		}
	}

	/// GuildmasterGump: Case 7
	public class GuildWarAdminGump : Gump
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildWarAdminGump(Mobile from, Guild guild) : base(20, 30)
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage(0);
			AddBackground(0, 0, 550, 440, 5054);
			AddBackground(10, 10, 530, 420, 3000);

			AddHtmlLocalized(20, 10, 510, 35, 1011105, false, false); // <center>WAR FUNCTIONS</center>

			AddButton(20, 40, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 40, 400, 30, 1011099, false, false); // Declare war through guild name search.

			var count = 0;

			if (guild.Enemies.Count > 0)
			{
				AddButton(20, 160 + (count * 30), 4005, 4007, 2, GumpButtonType.Reply, 0);
				AddHtmlLocalized(55, 160 + (count++ * 30), 400, 30, 1011103, false, false); // Declare peace.
			}
			else
			{
				AddHtmlLocalized(20, 160 + (count++ * 30), 400, 30, 1013033, false, false); // No current wars
			}

			if (guild.WarInvitations.Count > 0)
			{
				AddButton(20, 160 + (count * 30), 4005, 4007, 3, GumpButtonType.Reply, 0);
				AddHtmlLocalized(55, 160 + (count++ * 30), 400, 30, 1011100, false, false); // Accept war invitations.

				AddButton(20, 160 + (count * 30), 4005, 4007, 4, GumpButtonType.Reply, 0);
				AddHtmlLocalized(55, 160 + (count++ * 30), 400, 30, 1011101, false, false); // Reject war invitations.
			}
			else
			{
				AddHtmlLocalized(20, 160 + (count++ * 30), 400, 30, 1018012, false, false); // No current invitations received for war.
			}

			if (guild.WarDeclarations.Count > 0)
			{
				AddButton(20, 160 + (count * 30), 4005, 4007, 5, GumpButtonType.Reply, 0);
				AddHtmlLocalized(55, 160 + (count++ * 30), 400, 30, 1011102, false, false); // Rescind your war declarations.
			}
			else
			{
				AddHtmlLocalized(20, 160 + (count++ * 30), 400, 30, 1013055, false, false); // No current war declarations
			}

			AddButton(20, 400, 4005, 4007, 6, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 400, 35, 1011104, false, false); // Return to the previous menu.
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Declare war
					{
						m_Mobile.SendLocalizedMessage(1018001); // Declare war through search - Enter Guild Name:  
						m_Mobile.Prompt = new GuildDeclareWarPrompt(m_Mobile, m_Guild);

						break;
					}
				case 2: // Declare peace
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildDeclarePeaceGump(m_Mobile, m_Guild));

						break;
					}
				case 3: // Accept war
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildAcceptWarGump(m_Mobile, m_Guild));

						break;
					}
				case 4: // Reject war
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildRejectWarGump(m_Mobile, m_Guild));

						break;
					}
				case 5: // Rescind declarations
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildRescindDeclarationGump(m_Mobile, m_Guild));

						break;
					}
				case 6: // Return
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));

						break;
					}
			}
		}
	}

	/// GuildmasterGump: Case 8
	public class GuildAdminCandidatesGump : GuildMobileListGump
	{
		public GuildAdminCandidatesGump(Mobile from, Guild guild) : base(from, guild, true, guild.Candidates)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 400, 35, 1013075, false, false); // Accept or Refuse candidates for membership

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 245, 30, 1013076, false, false); // Accept

			AddButton(300, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 100, 35, 1013077, false, false); // Refuse
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 0:
					{
						GuildGump.EnsureClosed(m_Mobile);
						m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));

						break;
					}
				case 1: // Accept
					{
						var switches = info.Switches;

						if (switches.Length > 0)
						{
							var index = switches[0];

							if (index >= 0 && index < m_List.Count)
							{
								var m = m_List[index];

								if (m != null && !m.Deleted)
								{
									#region Factions
									var guildState = PlayerState.Find(m_Guild.Leader);
									var targetState = PlayerState.Find(m);

									var guildFaction = (guildState == null ? null : guildState.Faction);
									var targetFaction = (targetState == null ? null : targetState.Faction);

									if (guildFaction != targetFaction)
									{
										if (guildFaction == null)
										{
											m_Mobile.SendLocalizedMessage(1013027); // That player cannot join a non-faction guild.
										}
										else if (targetFaction == null)
										{
											m_Mobile.SendLocalizedMessage(1013026); // That player must be in a faction before joining this guild.
										}
										else
										{
											m_Mobile.SendLocalizedMessage(1013028); // That person has a different faction affiliation.
										}

										break;
									}
									else if (targetState != null && targetState.IsLeaving)
									{
										// OSI does this quite strangely, so we'll just do it this way
										m_Mobile.SendMessage("That person is quitting their faction and so you may not recruit them.");
										break;
									}
									#endregion

									m_Guild.Candidates.Remove(m);
									m_Guild.Accepted.Add(m);

									GuildGump.EnsureClosed(m_Mobile);

									if (m_Guild.Candidates.Count > 0)
									{
										m_Mobile.SendGump(new GuildAdminCandidatesGump(m_Mobile, m_Guild));
									}
									else
									{
										m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
									}
								}
							}
						}

						break;
					}
				case 2: // Refuse
					{
						var switches = info.Switches;

						if (switches.Length > 0)
						{
							var index = switches[0];

							if (index >= 0 && index < m_List.Count)
							{
								var m = m_List[index];

								if (m != null && !m.Deleted)
								{
									m_Guild.Candidates.Remove(m);

									GuildGump.EnsureClosed(m_Mobile);

									if (m_Guild.Candidates.Count > 0)
									{
										m_Mobile.SendGump(new GuildAdminCandidatesGump(m_Mobile, m_Guild));
									}
									else
									{
										m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
									}
								}
							}
						}

						break;
					}
			}
		}
	}

	/// GuildmasterGump: Case 9
	public class GuildTitlePrompt : Prompt
	{
		private readonly Mobile m_Leader, m_Target;
		private readonly Guild m_Guild;

		public GuildTitlePrompt(Mobile leader, Mobile target, Guild g)
		{
			m_Leader = leader;
			m_Target = target;
			m_Guild = g;
		}

		public override void OnCancel(Mobile from)
		{
			if (GuildGump.BadLeader(m_Leader, m_Guild))
			{
				return;
			}
			else if (m_Target.Deleted || !m_Guild.IsMember(m_Target))
			{
				return;
			}

			GuildGump.EnsureClosed(m_Leader);
			m_Leader.SendGump(new GuildmasterGump(m_Leader, m_Guild));
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (GuildGump.BadLeader(m_Leader, m_Guild))
			{
				return;
			}
			else if (m_Target.Deleted || !m_Guild.IsMember(m_Target))
			{
				return;
			}

			text = text.Trim();

			if (text.Length > 20)
			{
				text = text.Substring(0, 20);
			}

			if (text.Length > 0)
			{
				m_Target.GuildTitle = text;
			}

			GuildGump.EnsureClosed(m_Leader);
			m_Leader.SendGump(new GuildmasterGump(m_Leader, m_Guild));
		}
	}

	/// GuildmasterGump: Case 10
	public class GrantGuildTitleGump : GuildMobileListGump
	{
		public GrantGuildTitleGump(Mobile from, Guild guild) : base(from, guild, true, guild.Members)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 400, 35, 1011118, false, false); // Grant a title to another member.

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 245, 30, 1011127, false, false); // I dub thee...

			AddButton(300, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 100, 35, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				var switches = info.Switches;

				if (switches.Length > 0)
				{
					var index = switches[0];

					if (index >= 0 && index < m_List.Count)
					{
						var m = m_List[index];

						if (m != null && !m.Deleted)
						{
							m_Mobile.SendLocalizedMessage(1013074); // New title (20 characters max):
							m_Mobile.Prompt = new GuildTitlePrompt(m_Mobile, m, m_Guild);
						}
					}
				}
			}
			else if (info.ButtonID == 2)
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
			}
		}
	}

	#endregion

	/// Guild Create/ Join
	public class DeclareFealtyGump : GuildMobileListGump
	{
		public DeclareFealtyGump(Mobile from, Guild guild) : base(from, guild, true, guild.Members)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 400, 35, 1011097, false, false); // Declare your fealty

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 250, 35, 1011098, false, false); // I have selected my new lord.

			AddButton(300, 400, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 100, 35, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadMember(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				var switches = info.Switches;

				if (switches.Length > 0)
				{
					var index = switches[0];

					if (index >= 0 && index < m_List.Count)
					{
						var m = m_List[index];

						if (m != null && !m.Deleted)
						{
							state.Mobile.GuildFealty = m;
						}
					}
				}
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildGump(m_Mobile, m_Guild));
		}
	}

	public class GuildCharterGump : Gump
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		private const string DefaultWebsite = "https://github.com/runuo/";

		public GuildCharterGump(Mobile from, Guild guild) : base(20, 30)
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage(0);
			AddBackground(0, 0, 550, 400, 5054);
			AddBackground(10, 10, 530, 380, 3000);

			AddButton(20, 360, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 360, 300, 35, 1011120, false, false); // Return to the main menu.

			string charter;

			if ((charter = guild.Charter) == null || (charter = charter.Trim()).Length <= 0)
			{
				AddHtmlLocalized(20, 20, 400, 35, 1013032, false, false); // No charter has been defined.
			}
			else
			{
				AddHtml(20, 20, 510, 75, charter, true, true);
			}

			AddButton(20, 200, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 200, 300, 20, 1011122, false, false); // Visit the guild website : 

			string website;

			if ((website = guild.Website) == null || (website = website.Trim()).Length <= 0)
			{
				website = DefaultWebsite;
			}

			AddHtml(55, 220, 300, 20, website, false, false);
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadMember(m_Mobile, m_Guild))
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 0: return; // Close
				case 1: break; // Return to main menu
				case 2:
					{
						string website;

						if ((website = m_Guild.Website) == null || (website = website.Trim()).Length <= 0)
						{
							website = DefaultWebsite;
						}

						m_Mobile.LaunchBrowser(website);
						break;
					}
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildGump(m_Mobile, m_Guild));
		}
	}

	public class GuildWebsitePrompt : Prompt
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildWebsitePrompt(Mobile m, Guild g)
		{
			m_Mobile = m;
			m_Guild = g;
		}

		public override void OnCancel(Mobile from)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			text = text.Trim();

			if (text.Length > 50)
			{
				text = text.Substring(0, 50);
			}

			if (text.Length > 0)
			{
				m_Guild.Website = text;
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
		}
	}

	public class GuildRecruitTarget : Target
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildRecruitTarget(Mobile m, Guild guild) : base(10, false, TargetFlags.None)
		{
			m_Mobile = m;
			m_Guild = guild;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (GuildGump.BadMember(m_Mobile, m_Guild))
			{
				return;
			}

			if (targeted is Mobile)
			{
				var m = (Mobile)targeted;

				var guildState = PlayerState.Find(m_Guild.Leader);
				var targetState = PlayerState.Find(m);

				var guildFaction = (guildState == null ? null : guildState.Faction);
				var targetFaction = (targetState == null ? null : targetState.Faction);

				if (!m.Player)
				{
					m_Mobile.SendLocalizedMessage(501161); // You may only recruit players into the guild.
				}
				else if (!m.Alive)
				{
					m_Mobile.SendLocalizedMessage(501162); // Only the living may be recruited.
				}
				else if (m_Guild.IsMember(m))
				{
					m_Mobile.SendLocalizedMessage(501163); // They are already a guildmember!
				}
				else if (m_Guild.Candidates.Contains(m))
				{
					m_Mobile.SendLocalizedMessage(501164); // They are already a candidate.
				}
				else if (m_Guild.Accepted.Contains(m))
				{
					m_Mobile.SendLocalizedMessage(501165); // They have already been accepted for membership, and merely need to use the Guildstone to gain full membership.
				}
				else if (m.Guild != null)
				{
					m_Mobile.SendLocalizedMessage(501166); // You can only recruit candidates who are not already in a guild.
				}
				#region Factions
				else if (guildFaction != targetFaction)
				{
					if (guildFaction == null)
					{
						m_Mobile.SendLocalizedMessage(1013027); // That player cannot join a non-faction guild.
					}
					else if (targetFaction == null)
					{
						m_Mobile.SendLocalizedMessage(1013026); // That player must be in a faction before joining this guild.
					}
					else
					{
						m_Mobile.SendLocalizedMessage(1013028); // That person has a different faction affiliation.
					}
				}
				else if (targetState != null && targetState.IsLeaving)
				{
					// OSI does this quite strangely, so we'll just do it this way
					m_Mobile.SendMessage("That person is quitting their faction and so you may not recruit them.");
				}
				#endregion
				else if (m_Mobile.AccessLevel >= AccessLevel.GameMaster || m_Guild.Leader == m_Mobile)
				{
					m_Guild.Accepted.Add(m);
				}
				else
				{
					m_Guild.Candidates.Add(m);
				}
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			if (GuildGump.BadMember(m_Mobile, m_Guild))
			{
				return;
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildGump(m_Mobile, m_Guild));
		}
	}

	/// Guild Information
	public class GuildCandidatesGump : GuildMobileListGump
	{
		public GuildCandidatesGump(Mobile from, Guild guild) : base(from, guild, false, guild.Candidates)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 500, 35, 1013030, false, false); // <center> Candidates </center>

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 300, 35, 1011120, false, false); // Return to the main menu.
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadMember(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildGump(m_Mobile, m_Guild));
			}
		}
	}

	public class GuildRosterGump : GuildMobileListGump
	{
		public GuildRosterGump(Mobile from, Guild guild) : base(from, guild, false, guild.Members)
		{
		}

		protected override void Design()
		{
			AddHtml(20, 10, 500, 35, String.Format("<center>{0}</center>", m_Guild.Name), false, false);

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 300, 35, 1011120, false, false); // Return to the main menu.
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadMember(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildGump(m_Mobile, m_Guild));
			}
		}
	}

	public class GuildWarGump : Gump
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildWarGump(Mobile from, Guild guild) : base(20, 30)
		{
			m_Mobile = from;
			m_Guild = guild;

			Dragable = false;

			AddPage(0);
			AddBackground(0, 0, 550, 440, 5054);
			AddBackground(10, 10, 530, 420, 3000);

			AddHtmlLocalized(20, 10, 500, 35, 1011133, false, false); // <center>WARFARE STATUS</center>

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 300, 35, 1011120, false, false); // Return to the main menu.

			AddPage(1);

			AddButton(375, 375, 5224, 5224, 0, GumpButtonType.Page, 2);
			AddHtmlLocalized(410, 373, 100, 25, 1011066, false, false); // Next page

			AddHtmlLocalized(20, 45, 400, 20, 1011134, false, false); // We are at war with:

			var enemies = guild.Enemies;

			if (enemies.Count == 0)
			{
				AddHtmlLocalized(20, 65, 400, 20, 1013033, false, false); // No current wars
			}
			else
			{
				for (var i = 0; i < enemies.Count; ++i)
				{
					var g = enemies[i];

					AddHtml(20, 65 + (i * 20), 300, 20, g.Name, false, false);
				}
			}

			AddPage(2);

			AddButton(375, 375, 5224, 5224, 0, GumpButtonType.Page, 3);
			AddHtmlLocalized(410, 373, 100, 25, 1011066, false, false); // Next page

			AddButton(30, 375, 5223, 5223, 0, GumpButtonType.Page, 1);
			AddHtmlLocalized(65, 373, 150, 25, 1011067, false, false); // Previous page

			AddHtmlLocalized(20, 45, 400, 20, 1011136, false, false); // Guilds that we have declared war on: 

			var declared = guild.WarDeclarations;

			if (declared.Count == 0)
			{
				AddHtmlLocalized(20, 65, 400, 20, 1018012, false, false); // No current invitations received for war.
			}
			else
			{
				for (var i = 0; i < declared.Count; ++i)
				{
					var g = declared[i];

					AddHtml(20, 65 + (i * 20), 300, 20, g.Name, false, false);
				}
			}

			AddPage(3);

			AddButton(30, 375, 5223, 5223, 0, GumpButtonType.Page, 2);
			AddHtmlLocalized(65, 373, 150, 25, 1011067, false, false); // Previous page

			AddHtmlLocalized(20, 45, 400, 20, 1011135, false, false); // Guilds that have declared war on us: 

			var invites = guild.WarInvitations;

			if (invites.Count == 0)
			{
				AddHtmlLocalized(20, 65, 400, 20, 1013055, false, false); // No current war declarations
			}
			else
			{
				for (var i = 0; i < invites.Count; ++i)
				{
					var g = invites[i];

					AddHtml(20, 65 + (i * 20), 300, 20, g.Name, false, false);
				}
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadMember(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildGump(m_Mobile, m_Guild));
			}
		}
	}

	/// Guild Diplomacy
	public class GuildDeclarePeaceGump : GuildListGump
	{
		public GuildDeclarePeaceGump(Mobile from, Guild guild) : base(from, guild, true, guild.Enemies)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 400, 35, 1011137, false, false); // Select the guild you wish to declare peace with.

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 245, 30, 1011138, false, false); // Send the olive branch.

			AddButton(300, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 100, 35, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				var switches = info.Switches;

				if (switches.Length > 0)
				{
					var index = switches[0];

					if (index >= 0 && index < m_List.Count)
					{
						var g = m_List[index];

						if (g != null)
						{
							m_Guild.RemoveEnemy(g);
							m_Guild.GuildMessage(1018018, true, "{0} ({1})", g.Name, g.Abbreviation); // Guild Message: You are now at peace with this guild:

							GuildGump.EnsureClosed(m_Mobile);

							if (m_Guild.Enemies.Count > 0)
							{
								m_Mobile.SendGump(new GuildDeclarePeaceGump(m_Mobile, m_Guild));
							}
							else
							{
								m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
							}
						}
					}
				}
			}
			else if (info.ButtonID == 2)
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
			}
		}
	}

	public class GuildDeclareWarGump : GuildListGump
	{
		public GuildDeclareWarGump(Mobile from, Guild guild, List<Guild> list)
			: base(from, guild, true, list)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 400, 35, 1011065, false, false); // Select the guild you wish to declare war on.

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 245, 30, 1011068, false, false); // Send the challenge!

			AddButton(300, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 100, 35, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				var switches = info.Switches;

				if (switches.Length > 0)
				{
					var index = switches[0];

					if (index >= 0 && index < m_List.Count)
					{
						var g = m_List[index];

						if (g != null)
						{
							if (g == m_Guild)
							{
								m_Mobile.SendLocalizedMessage(501184); // You cannot declare war against yourself!
							}
							else if ((g.WarInvitations.Contains(m_Guild) && m_Guild.WarDeclarations.Contains(g)) || m_Guild.IsWar(g))
							{
								m_Mobile.SendLocalizedMessage(501183); // You are already at war with that guild.
							}
							else if (Faction.Find(m_Guild.Leader) != null)
							{
								m_Mobile.SendLocalizedMessage(1005288); // You cannot declare war while you are in a faction
							}
							else
							{
								if (!m_Guild.WarDeclarations.Contains(g))
								{
									m_Guild.WarDeclarations.Add(g);
									m_Guild.GuildMessage(1018019, true, "{0} ({1})", g.Name, g.Abbreviation); // Guild Message: Your guild has sent an invitation for war:
								}

								if (!g.WarInvitations.Contains(m_Guild))
								{
									g.WarInvitations.Add(m_Guild);
									g.GuildMessage(1018021, true, "{0} ({1})", m_Guild.Name, m_Guild.Abbreviation); // Guild Message: Your guild has received an invitation to war:
								}
							}

							GuildGump.EnsureClosed(m_Mobile);
							m_Mobile.SendGump(new GuildWarAdminGump(m_Mobile, m_Guild));
						}
					}
				}
			}
			else if (info.ButtonID == 2)
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
			}
		}
	}

	public class GuildDeclareWarPrompt : Prompt
	{
		private readonly Mobile m_Mobile;
		private readonly Guild m_Guild;

		public GuildDeclareWarPrompt(Mobile m, Guild g)
		{
			m_Mobile = m;
			m_Guild = g;
		}

		public override void OnCancel(Mobile from)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			GuildGump.EnsureClosed(m_Mobile);
			m_Mobile.SendGump(new GuildWarAdminGump(m_Mobile, m_Guild));
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			text = text.Trim();

			if (text.Length >= 3)
			{
				var guilds = Utility.CastConvertList<BaseGuild, Guild>(Guild.Search(text));

				GuildGump.EnsureClosed(m_Mobile);

				if (guilds.Count > 0)
				{
					m_Mobile.SendGump(new GuildDeclareWarGump(m_Mobile, m_Guild, guilds));
				}
				else
				{
					m_Mobile.SendGump(new GuildWarAdminGump(m_Mobile, m_Guild));
					m_Mobile.SendLocalizedMessage(1018003); // No guilds found matching - try another name in the search
				}
			}
			else
			{
				m_Mobile.SendMessage("Search string must be at least three letters in length.");
			}
		}
	}

	public class GuildRescindDeclarationGump : GuildListGump
	{
		public GuildRescindDeclarationGump(Mobile from, Guild guild) : base(from, guild, true, guild.WarDeclarations)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 400, 35, 1011150, false, false); // Select the guild to rescind our invitations: 

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 245, 30, 1011102, false, false); // Rescind your war declarations.

			AddButton(300, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 100, 35, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				var switches = info.Switches;

				if (switches.Length > 0)
				{
					var index = switches[0];

					if (index >= 0 && index < m_List.Count)
					{
						var g = m_List[index];

						if (g != null)
						{
							m_Guild.WarDeclarations.Remove(g);
							g.WarInvitations.Remove(m_Guild);

							GuildGump.EnsureClosed(m_Mobile);

							if (m_Guild.WarDeclarations.Count > 0)
							{
								m_Mobile.SendGump(new GuildRescindDeclarationGump(m_Mobile, m_Guild));
							}
							else
							{
								m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
							}
						}
					}
				}
			}
			else if (info.ButtonID == 2)
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
			}
		}
	}

	public class GuildAcceptWarGump : GuildListGump
	{
		public GuildAcceptWarGump(Mobile from, Guild guild) : base(from, guild, true, guild.WarInvitations)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 400, 35, 1011147, false, false); // Select the guild to accept the invitations: 

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 245, 30, 1011100, false, false);  // Accept war invitations.

			AddButton(300, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 100, 35, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				var switches = info.Switches;

				if (switches.Length > 0)
				{
					var index = switches[0];

					if (index >= 0 && index < m_List.Count)
					{
						var g = m_List[index];

						if (g != null)
						{
							m_Guild.WarInvitations.Remove(g);
							g.WarDeclarations.Remove(m_Guild);

							m_Guild.AddEnemy(g);
							m_Guild.GuildMessage(1018020, true, "{0} ({1})", g.Name, g.Abbreviation);

							GuildGump.EnsureClosed(m_Mobile);

							if (m_Guild.WarInvitations.Count > 0)
							{
								m_Mobile.SendGump(new GuildAcceptWarGump(m_Mobile, m_Guild));
							}
							else
							{
								m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
							}
						}
					}
				}
			}
			else if (info.ButtonID == 2)
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
			}
		}
	}

	public class GuildRejectWarGump : GuildListGump
	{
		public GuildRejectWarGump(Mobile from, Guild guild) : base(from, guild, true, guild.WarInvitations)
		{
		}

		protected override void Design()
		{
			AddHtmlLocalized(20, 10, 400, 35, 1011148, false, false); // Select the guild to reject their invitations: 

			AddButton(20, 400, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 400, 245, 30, 1011101, false, false);  // Reject war invitations.

			AddButton(300, 400, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(335, 400, 100, 35, 1011012, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (GuildGump.BadLeader(m_Mobile, m_Guild))
			{
				return;
			}

			if (info.ButtonID == 1)
			{
				var switches = info.Switches;

				if (switches.Length > 0)
				{
					var index = switches[0];

					if (index >= 0 && index < m_List.Count)
					{
						var g = m_List[index];

						if (g != null)
						{
							m_Guild.WarInvitations.Remove(g);
							g.WarDeclarations.Remove(m_Guild);

							GuildGump.EnsureClosed(m_Mobile);

							if (m_Guild.WarInvitations.Count > 0)
							{
								m_Mobile.SendGump(new GuildRejectWarGump(m_Mobile, m_Guild));
							}
							else
							{
								m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
							}
						}
					}
				}
			}
			else if (info.ButtonID == 2)
			{
				GuildGump.EnsureClosed(m_Mobile);
				m_Mobile.SendGump(new GuildmasterGump(m_Mobile, m_Guild));
			}
		}
	}
}