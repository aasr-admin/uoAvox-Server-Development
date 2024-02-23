using Server.Accounting;
using Server.Commands;
using Server.Commands.Generic;
using Server.Engines.Craft;
using Server.Ethics;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Prompts;
using Server.Regions;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Server.Factions
{
	public class Generator
	{
		public static void Initialize()
		{
			CommandSystem.Register("GenerateFactions", AccessLevel.Administrator, new CommandEventHandler(GenerateFactions_OnCommand));
		}

		public static void GenerateFactions_OnCommand(CommandEventArgs e)
		{
			_ = new FactionPersistance();

			var factions = Faction.Factions;

			foreach (var faction in factions)
			{
				Generate(faction);
			}

			var towns = Town.Towns;

			foreach (var town in towns)
			{
				Generate(town);
			}
		}

		public static void Generate(Town town)
		{
			var facet = Faction.Facet;

			var def = town.Definition;

			if (!CheckExistance(def.Monolith, facet, typeof(TownMonolith)))
			{
				var mono = new TownMonolith(town);
				mono.MoveToWorld(def.Monolith, facet);
				mono.Sigil = new Sigil(town);
			}

			if (!CheckExistance(def.TownStone, facet, typeof(TownStone)))
			{
				new TownStone(town).MoveToWorld(def.TownStone, facet);
			}
		}

		public static void Generate(Faction faction)
		{
			var facet = Faction.Facet;

			var towns = Town.Towns;

			var stronghold = faction.Definition.Stronghold;

			if (!CheckExistance(stronghold.JoinStone, facet, typeof(JoinStone)))
			{
				new JoinStone(faction).MoveToWorld(stronghold.JoinStone, facet);
			}

			if (!CheckExistance(stronghold.FactionStone, facet, typeof(FactionStone)))
			{
				new FactionStone(faction).MoveToWorld(stronghold.FactionStone, facet);
			}

			for (var i = 0; i < stronghold.Monoliths.Length; ++i)
			{
				var monolith = stronghold.Monoliths[i];

				if (!CheckExistance(monolith, facet, typeof(StrongholdMonolith)))
				{
					new StrongholdMonolith(towns[i], faction).MoveToWorld(monolith, facet);
				}
			}
		}

		private static bool CheckExistance(Point3D loc, Map facet, Type type)
		{
			foreach (var item in facet.GetItemsInRange(loc, 0))
			{
				if (type.IsAssignableFrom(item.GetType()))
				{
					return true;
				}
			}

			return false;
		}
	}


	/// Faction Engine
	public class RankDefinition
	{
		private readonly int m_Rank;
		private readonly int m_Required;
		private readonly int m_MaxWearables;
		private readonly TextDefinition m_Title;

		public int Rank => m_Rank;
		public int Required => m_Required;
		public int MaxWearables => m_MaxWearables;
		public TextDefinition Title => m_Title;

		public RankDefinition(int rank, int required, int maxWearables, TextDefinition title)
		{
			m_Rank = rank;
			m_Required = required;
			m_Title = title;
			m_MaxWearables = maxWearables;
		}
	}

	public enum FactionEthic
	{
		None,
		Good,
		Evil,
	}

	public class FactionDefinition
	{
		private readonly int m_Sort;

		private readonly FactionEthic m_Ethic;

		private readonly int m_HuePrimary;
		private readonly int m_HueSecondary;
		private readonly int m_HueJoin;
		private readonly int m_HueBroadcast;

		private readonly int m_WarHorseBody;
		private readonly int m_WarHorseItem;

		private readonly string m_FriendlyName;
		private readonly string m_Keyword;
		private readonly string m_Abbreviation;

		private readonly TextDefinition m_Name;
		private readonly TextDefinition m_PropName;
		private readonly TextDefinition m_Header;
		private readonly TextDefinition m_About;
		private readonly TextDefinition m_CityControl;
		private readonly TextDefinition m_SigilControl;
		private readonly TextDefinition m_SignupName;
		private readonly TextDefinition m_FactionStoneName;
		private readonly TextDefinition m_OwnerLabel;

		private readonly TextDefinition m_GuardIgnore, m_GuardWarn, m_GuardAttack;

		private readonly StrongholdDefinition m_Stronghold;

		private readonly RankDefinition[] m_Ranks;
		private readonly GuardDefinition[] m_Guards;

		public int Sort => m_Sort;

		public FactionEthic Ethic => m_Ethic;

		public int HuePrimary => m_HuePrimary;
		public int HueSecondary => m_HueSecondary;
		public int HueJoin => m_HueJoin;
		public int HueBroadcast => m_HueBroadcast;

		public int WarHorseBody => m_WarHorseBody;
		public int WarHorseItem => m_WarHorseItem;

		public string FriendlyName => m_FriendlyName;
		public string Keyword => m_Keyword;
		public string Abbreviation => m_Abbreviation;

		public TextDefinition Name => m_Name;
		public TextDefinition PropName => m_PropName;
		public TextDefinition Header => m_Header;
		public TextDefinition About => m_About;
		public TextDefinition CityControl => m_CityControl;
		public TextDefinition SigilControl => m_SigilControl;
		public TextDefinition SignupName => m_SignupName;
		public TextDefinition FactionStoneName => m_FactionStoneName;
		public TextDefinition OwnerLabel => m_OwnerLabel;

		public TextDefinition GuardIgnore => m_GuardIgnore;
		public TextDefinition GuardWarn => m_GuardWarn;
		public TextDefinition GuardAttack => m_GuardAttack;

		public StrongholdDefinition Stronghold => m_Stronghold;

		public RankDefinition[] Ranks => m_Ranks;
		public GuardDefinition[] Guards => m_Guards;

		public FactionDefinition(int sort, FactionEthic ethic, int huePrimary, int hueSecondary, int hueJoin, int hueBroadcast, int warHorseBody, int warHorseItem, string friendlyName, string keyword, string abbreviation, TextDefinition name, TextDefinition propName, TextDefinition header, TextDefinition about, TextDefinition cityControl, TextDefinition sigilControl, TextDefinition signupName, TextDefinition factionStoneName, TextDefinition ownerLabel, TextDefinition guardIgnore, TextDefinition guardWarn, TextDefinition guardAttack, StrongholdDefinition stronghold, RankDefinition[] ranks, GuardDefinition[] guards)
		{
			m_Sort = sort;
			m_Ethic = ethic;
			m_HuePrimary = huePrimary;
			m_HueSecondary = hueSecondary;
			m_HueJoin = hueJoin;
			m_HueBroadcast = hueBroadcast;
			m_WarHorseBody = warHorseBody;
			m_WarHorseItem = warHorseItem;
			m_FriendlyName = friendlyName;
			m_Keyword = keyword;
			m_Abbreviation = abbreviation;
			m_Name = name;
			m_PropName = propName;
			m_Header = header;
			m_About = about;
			m_CityControl = cityControl;
			m_SigilControl = sigilControl;
			m_SignupName = signupName;
			m_FactionStoneName = factionStoneName;
			m_OwnerLabel = ownerLabel;
			m_GuardIgnore = guardIgnore;
			m_GuardWarn = guardWarn;
			m_GuardAttack = guardAttack;
			m_Stronghold = stronghold;
			m_Ranks = ranks;
			m_Guards = guards;
		}
	}

	[Parsable]
	public abstract class Faction : IComparable<Faction>
	{
		public static readonly Map Facet = Map.Felucca;

		public static readonly TimeSpan LeavePeriod = TimeSpan.FromDays(3.0);

		public const int StabilityFactor = 300; // 300% greater (3 times) than smallest faction
		public const int StabilityActivation = 200; // Stablity code goes into effect when largest faction has > 200 people

		public abstract FactionDefinition Definition { get; }

		public FactionState State { get; set; }

		public Election Election => State.Election;
		public StrongholdRegion StrongholdRegion => State.StrongholdRegion;

		public Mobile Commander { get => State.Commander; set => State.Commander = value; }

		public int Tithe { get => State.Tithe; set => State.Tithe = value; }
		public int Silver { get => State.Silver; set => State.Silver = value; }

		public List<PlayerState> Members => State.Members;
		public List<FactionItem> Items => State.Items;
		public List<BaseFactionTrap> Traps => State.Traps;

		public bool FactionMessageReady => State.FactionMessageReady;

		public int MaximumTraps { get; set; } = 15;

		public int ZeroRankOffset { get; set; }

		public Faction()
		{
			State = new FactionState(this);
		}

		public void Broadcast(string text)
		{
			Broadcast(0x3B2, text);
		}

		public void Broadcast(int hue, string text)
		{
			var members = Members;

			for (var i = 0; i < members.Count; ++i)
			{
				members[i].Mobile.SendMessage(hue, text);
			}
		}

		public void Broadcast(int number)
		{
			var members = Members;

			for (var i = 0; i < members.Count; ++i)
			{
				members[i].Mobile.SendLocalizedMessage(number);
			}
		}

		public void Broadcast(string format, params object[] args)
		{
			Broadcast(String.Format(format, args));
		}

		public void Broadcast(int hue, string format, params object[] args)
		{
			Broadcast(hue, String.Format(format, args));
		}

		public void BeginBroadcast(Mobile from)
		{
			from.SendLocalizedMessage(1010265); // Enter Faction Message
			from.Prompt = new BroadcastPrompt(this);
		}

		public void EndBroadcast(Mobile from, string text)
		{
			if (from.AccessLevel == AccessLevel.Player)
			{
				State.RegisterBroadcast();
			}

			Broadcast(Definition.HueBroadcast, "{0} [Commander] {1} : {2}", from.Name, Definition.FriendlyName, text);
		}

		private class BroadcastPrompt : Prompt
		{
			private readonly Faction m_Faction;

			public BroadcastPrompt(Faction faction)
			{
				m_Faction = faction;
			}

			public override void OnResponse(Mobile from, string text)
			{
				m_Faction.EndBroadcast(from, text);
			}
		}

		public static void HandleAtrophy()
		{
			foreach (var f in Factions)
			{
				if (!f.State.IsAtrophyReady)
				{
					return;
				}
			}

			var activePlayers = new List<PlayerState>();

			foreach (var f in Factions)
			{
				foreach (var ps in f.Members)
				{
					if (ps.KillPoints > 0 && ps.IsActive)
					{
						activePlayers.Add(ps);
					}
				}
			}

			var distrib = 0;

			foreach (var f in Factions)
			{
				distrib += f.State.CheckAtrophy();
			}

			if (activePlayers.Count == 0)
			{
				return;
			}

			for (var i = 0; i < distrib; ++i)
			{
				activePlayers[Utility.Random(activePlayers.Count)].KillPoints++;
			}
		}

		public static void DistributePoints(int distrib)
		{
			var activePlayers = new List<PlayerState>();

			foreach (var f in Factions)
			{
				foreach (var ps in f.Members)
				{
					if (ps.KillPoints > 0 && ps.IsActive)
					{
						activePlayers.Add(ps);
					}
				}
			}

			if (activePlayers.Count > 0)
			{
				for (var i = 0; i < distrib; ++i)
				{
					activePlayers[Utility.Random(activePlayers.Count)].KillPoints++;
				}
			}
		}

		public static void BeginHonorLeadership(Mobile from)
		{
			from.SendLocalizedMessage(502090); // Click on the player whom you wish to honor.
			from.BeginTarget(12, false, TargetFlags.None, HonorLeadership_OnTarget);
		}

		public static void HonorLeadership_OnTarget(Mobile from, object obj)
		{
			if (obj is Mobile recv)
			{
				var giveState = PlayerState.Find(from);
				var recvState = PlayerState.Find(recv);

				if (giveState == null)
				{
					return;
				}

				if (recvState == null || recvState.Faction != giveState.Faction)
				{
					from.SendLocalizedMessage(1042497); // Only faction mates can be honored this way.
				}
				else if (giveState.KillPoints < 5)
				{
					from.SendLocalizedMessage(1042499); // You must have at least five kill points to honor them.
				}
				else
				{
					recvState.LastHonorTime = DateTime.UtcNow;

					giveState.KillPoints -= 5;
					recvState.KillPoints += 4;

					// TODO: Confirm no message sent to giver
					recv.SendLocalizedMessage(1042500); // You have been honored with four kill points.
				}
			}
			else
			{
				from.SendLocalizedMessage(1042496); // You may only honor another player.
			}
		}

		public virtual void AddMember(Mobile mob)
		{
			Members.Insert(ZeroRankOffset, new PlayerState(mob, this, Members));

			mob.AddToBackpack(FactionItem.Imbue(new Robe(), this, false, Definition.HuePrimary));
			mob.SendLocalizedMessage(1010374); // You have been granted a robe which signifies your faction

			mob.InvalidateProperties();
			mob.Delta(MobileDelta.Noto);

			mob.FixedEffect(0x373A, 10, 30);
			mob.PlaySound(0x209);
		}

		public static bool IsNearType(Mobile mob, Type type, int range)
		{
			var mobs = type.IsSubclassOf(typeof(Mobile));
			var items = type.IsSubclassOf(typeof(Item));

			IPooledEnumerable eable;

			if (mobs)
			{
				eable = mob.GetMobilesInRange(range);
			}
			else if (items)
			{
				eable = mob.GetItemsInRange(range);
			}
			else
			{
				return false;
			}

			foreach (var obj in eable)
			{
				if (type.IsAssignableFrom(obj.GetType()))
				{
					eable.Free();
					return true;
				}
			}

			eable.Free();
			return false;
		}

		public static bool IsNearType(Mobile mob, Type[] types, int range)
		{
			IPooledEnumerable eable = mob.GetObjectsInRange(range);

			foreach (var obj in eable)
			{
				var objType = obj.GetType();

				for (var i = 0; i < types.Length; i++)
				{
					if (types[i].IsAssignableFrom(objType))
					{
						eable.Free();
						return true;
					}
				}
			}

			eable.Free();
			return false;
		}

		public void RemovePlayerState(PlayerState pl)
		{
			if (pl == null || !Members.Contains(pl))
			{
				return;
			}

			var killPoints = pl.KillPoints;

			if (pl.RankIndex != -1)
			{
				while ((pl.RankIndex + 1) < ZeroRankOffset)
				{
					var pNext = Members[pl.RankIndex + 1];
					Members[pl.RankIndex + 1] = pl;
					Members[pl.RankIndex] = pNext;
					pl.RankIndex++;
					pNext.RankIndex--;
				}

				ZeroRankOffset--;
			}

			Members.Remove(pl);

			var pm = (PlayerMobile)pl.Mobile;
			if (pm == null)
			{
				return;
			}

			var mob = pl.Mobile;
			if (pm.FactionPlayerState == pl)
			{
				pm.FactionPlayerState = null;

				mob.InvalidateProperties();
				mob.Delta(MobileDelta.Noto);

				if (Election.IsCandidate(mob))
				{
					Election.RemoveCandidate(mob);
				}

				if (pl.Finance != null)
				{
					pl.Finance.Finance = null;
				}

				if (pl.Sheriff != null)
				{
					pl.Sheriff.Sheriff = null;
				}

				Election.RemoveVoter(mob);

				if (Commander == mob)
				{
					Commander = null;
				}

				pm.ValidateEquipment();
			}

			if (killPoints > 0)
			{
				DistributePoints(killPoints);
			}
		}

		public void RemoveMember(Mobile mob)
		{
			var pl = PlayerState.Find(mob);

			if (pl == null || !Members.Contains(pl))
			{
				return;
			}

			var killPoints = pl.KillPoints;

			var pack = mob.Backpack;

			if (pack != null)
			{
				//Ordinarily, through normal faction removal, this will never find any sigils.
				//Only with a leave delay less than the ReturnPeriod or a Faction Kick/Ban, will this ever do anything
				foreach (var sigil in pack.FindItemsByType<Sigil>())
				{
					sigil.ReturnHome();
				}
			}

			if (pl.RankIndex != -1)
			{
				while (pl.RankIndex + 1 < ZeroRankOffset)
				{
					var pNext = Members[pl.RankIndex + 1];

					Members[pl.RankIndex + 1] = pl;
					Members[pl.RankIndex] = pNext;

					pl.RankIndex++;
					pNext.RankIndex--;
				}

				ZeroRankOffset--;
			}

			Members.Remove(pl);

			if (mob is PlayerMobile pms)
			{
				pms.FactionPlayerState = null;
			}

			mob.InvalidateProperties();
			mob.Delta(MobileDelta.Noto);

			if (Election.IsCandidate(mob))
			{
				Election.RemoveCandidate(mob);
			}

			Election.RemoveVoter(mob);

			if (pl.Finance != null)
			{
				pl.Finance.Finance = null;
			}

			if (pl.Sheriff != null)
			{
				pl.Sheriff.Sheriff = null;
			}

			if (Commander == mob)
			{
				Commander = null;
			}

			if (mob is PlayerMobile pmv)
			{
				pmv.ValidateEquipment();
			}

			if (killPoints > 0)
			{
				DistributePoints(killPoints);
			}
		}

		public void JoinGuilded(PlayerMobile mob, Guild guild)
		{
			if (mob.Young)
			{
				guild.RemoveMember(mob);
				mob.SendLocalizedMessage(1042283); // You have been kicked out of your guild!  Young players may not remain in a guild which is allied with a faction.
			}
			else if (AlreadyHasCharInFaction(mob))
			{
				guild.RemoveMember(mob);
				mob.SendLocalizedMessage(1005281); // You have been kicked out of your guild due to factional overlap
			}
			else if (IsFactionBanned(mob))
			{
				guild.RemoveMember(mob);
				mob.SendLocalizedMessage(1005052); // You are currently banned from the faction system
			}
			else
			{
				AddMember(mob);
				mob.SendLocalizedMessage(1042756, true, " " + Definition.FriendlyName); // You are now joining a faction:
			}
		}

		public void JoinAlone(Mobile mob)
		{
			AddMember(mob);
			mob.SendLocalizedMessage(1005058); // You have joined the faction
		}

		private static bool AlreadyHasCharInFaction(Mobile mob)
		{
			if (mob.Account != null)
			{
				for (var i = 0; i < mob.Account.Length; ++i)
				{
					if (Find(mob.Account[i]) != null)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool IsFactionBanned(Mobile mob)
		{
			if (mob.Account is not Account acct)
			{
				return false;
			}

			return acct.GetTag("FactionBanned") != null;
		}

		public void OnJoinAccepted(Mobile mob)
		{
			if (mob is not PlayerMobile pm)
			{
				return; // sanity
			}

			var pl = PlayerState.Find(pm);

			if (pm.Young)
			{
				pm.SendLocalizedMessage(1010104); // You cannot join a faction as a young player
			}
			else if (pl != null && pl.IsLeaving)
			{
				pm.SendLocalizedMessage(1005051); // You cannot use the faction stone until you have finished quitting your current faction
			}
			else if (AlreadyHasCharInFaction(pm))
			{
				pm.SendLocalizedMessage(1005059); // You cannot join a faction because you already declared your allegiance with another character
			}
			else if (IsFactionBanned(mob))
			{
				pm.SendLocalizedMessage(1005052); // You are currently banned from the faction system
			}
			else if (pm.Guild is Guild guild)
			{
				if (guild.Leader != pm)
				{
					pm.SendLocalizedMessage(1005057); // You cannot join a faction because you are in a guild and not the guildmaster
				}
				else if (guild.Type != GuildType.Regular)
				{
					pm.SendLocalizedMessage(1042161); // You cannot join a faction because your guild is an Order or Chaos type.
				}
				else if (!Guild.NewGuildSystem && guild.Enemies != null && guild.Enemies.Count > 0) //CAN join w/wars in new system
				{
					pm.SendLocalizedMessage(1005056); // You cannot join a faction with active Wars
				}
				else if (Guild.NewGuildSystem && guild.Alliance != null)
				{
					pm.SendLocalizedMessage(1080454); // Your guild cannot join a faction while in alliance with non-factioned guilds.
				}
				else if (!CanHandleInflux(guild.Members.Count))
				{
					pm.SendLocalizedMessage(1018031); // In the interest of faction stability, this faction declines to accept new members for now.
				}
				else
				{
					var members = new List<Mobile>(guild.Members);

					for (var i = 0; i < members.Count; ++i)
					{
						if (members[i] is PlayerMobile member)
						{
							JoinGuilded(member, guild);
						}
					}

					members.Clear();
					members.TrimExcess();
				}
			}
			else if (!CanHandleInflux(1))
			{
				pm.SendLocalizedMessage(1018031); // In the interest of faction stability, this faction declines to accept new members for now.
			}
			else
			{
				JoinAlone(mob);
			}
		}

		public bool IsCommander(Mobile mob)
		{
			if (mob == null)
			{
				return false;
			}

			return mob.AccessLevel >= AccessLevel.GameMaster || mob == Commander;
		}

		public override string ToString()
		{
			return Definition.FriendlyName;
		}

		public int CompareTo(Faction f)
		{
			return Definition.Sort - f.Definition.Sort;
		}

		public static bool CheckLeaveTimer(Mobile mob)
		{
			var pl = PlayerState.Find(mob);

			if (pl == null || !pl.IsLeaving)
			{
				return false;
			}

			if ((pl.Leaving + LeavePeriod) >= DateTime.UtcNow)
			{
				return false;
			}

			mob.SendLocalizedMessage(1005163); // You have now quit your faction

			pl.Faction.RemoveMember(mob);

			return true;
		}

		public static void Initialize()
		{
			EventSink.Login += new LoginEventHandler(EventSink_Login);
			EventSink.Logout += new LogoutEventHandler(EventSink_Logout);

			Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(10.0), HandleAtrophy);

			Timer.DelayCall(TimeSpan.FromSeconds(30.0), TimeSpan.FromSeconds(30.0), ProcessTick);

			CommandSystem.Register("FactionElection", AccessLevel.GameMaster, new CommandEventHandler(FactionElection_OnCommand));
			CommandSystem.Register("FactionCommander", AccessLevel.Administrator, new CommandEventHandler(FactionCommander_OnCommand));
			CommandSystem.Register("FactionItemReset", AccessLevel.Administrator, new CommandEventHandler(FactionItemReset_OnCommand));
			CommandSystem.Register("FactionReset", AccessLevel.Administrator, new CommandEventHandler(FactionReset_OnCommand));
			CommandSystem.Register("FactionTownReset", AccessLevel.Administrator, new CommandEventHandler(FactionTownReset_OnCommand));
		}

		public static void FactionTownReset_OnCommand(CommandEventArgs e)
		{
			var monoliths = BaseMonolith.Monoliths;

			for (var i = 0; i < monoliths.Count; ++i)
			{
				monoliths[i].Sigil = null;
			}

			var towns = Town.Towns;

			for (var i = 0; i < towns.Count; ++i)
			{
				towns[i].Silver = 0;
				towns[i].Sheriff = null;
				towns[i].Finance = null;
				towns[i].Tax = 0;
				towns[i].Owner = null;
			}

			var sigils = Sigil.Sigils;

			for (var i = 0; i < sigils.Count; ++i)
			{
				sigils[i].Corrupted = null;
				sigils[i].Corrupting = null;
				sigils[i].LastStolen = DateTime.MinValue;
				sigils[i].GraceStart = DateTime.MinValue;
				sigils[i].CorruptionStart = DateTime.MinValue;
				sigils[i].PurificationStart = DateTime.MinValue;
				sigils[i].LastMonolith = null;
				sigils[i].ReturnHome();
			}

			var factions = Faction.Factions;

			for (var i = 0; i < factions.Count; ++i)
			{
				var f = factions[i];

				var list = new List<FactionItem>(f.State.Items);

				for (var j = 0; j < list.Count; ++j)
				{
					var fi = list[j];

					if (fi.Expiration == DateTime.MinValue)
					{
						fi.Item.Delete();
					}
					else
					{
						fi.Detach();
					}
				}
			}
		}

		public static void FactionReset_OnCommand(CommandEventArgs e)
		{
			var monoliths = BaseMonolith.Monoliths;

			for (var i = 0; i < monoliths.Count; ++i)
			{
				monoliths[i].Sigil = null;
			}

			var towns = Town.Towns;

			for (var i = 0; i < towns.Count; ++i)
			{
				towns[i].Silver = 0;
				towns[i].Sheriff = null;
				towns[i].Finance = null;
				towns[i].Tax = 0;
				towns[i].Owner = null;
			}

			var sigils = Sigil.Sigils;

			for (var i = 0; i < sigils.Count; ++i)
			{
				sigils[i].Corrupted = null;
				sigils[i].Corrupting = null;
				sigils[i].LastStolen = DateTime.MinValue;
				sigils[i].GraceStart = DateTime.MinValue;
				sigils[i].CorruptionStart = DateTime.MinValue;
				sigils[i].PurificationStart = DateTime.MinValue;
				sigils[i].LastMonolith = null;
				sigils[i].ReturnHome();
			}

			var factions = Faction.Factions;

			for (var i = 0; i < factions.Count; ++i)
			{
				var f = factions[i];

				var playerStateList = new List<PlayerState>(f.Members);

				for (var j = 0; j < playerStateList.Count; ++j)
				{
					f.RemoveMember(playerStateList[j].Mobile);
				}

				var factionItemList = new List<FactionItem>(f.State.Items);

				for (var j = 0; j < factionItemList.Count; ++j)
				{
					var fi = factionItemList[j];

					if (fi.Expiration == DateTime.MinValue)
					{
						fi.Item.Delete();
					}
					else
					{
						fi.Detach();
					}
				}

				var factionTrapList = new List<BaseFactionTrap>(f.Traps);

				for (var j = 0; j < factionTrapList.Count; ++j)
				{
					factionTrapList[j].Delete();
				}
			}
		}

		public static void FactionItemReset_OnCommand(CommandEventArgs e)
		{
			var pots = new ArrayList();

			foreach (var item in World.Items.Values)
			{
				if (item is IFactionItem && item is not HoodedShroudOfShadows)
				{
					pots.Add(item);
				}
			}

			var hues = new int[Factions.Count * 2];

			for (var i = 0; i < Factions.Count; ++i)
			{
				hues[0 + (i * 2)] = Factions[i].Definition.HuePrimary;
				hues[1 + (i * 2)] = Factions[i].Definition.HueSecondary;
			}

			var count = 0;

			for (var i = 0; i < pots.Count; ++i)
			{
				var item = (Item)pots[i];
				var fci = (IFactionItem)item;

				if (fci.FactionItemState != null || item.LootType != LootType.Blessed)
				{
					continue;
				}

				var isHued = false;

				for (var j = 0; j < hues.Length; ++j)
				{
					if (item.Hue == hues[j])
					{
						isHued = true;
						break;
					}
				}

				if (isHued)
				{
					fci.FactionItemState = null;
					++count;
				}
			}

			e.Mobile.SendMessage("{0} items reset", count);
		}

		public static void FactionCommander_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendMessage("Target a player to make them the faction commander.");
			e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(FactionCommander_OnTarget));
		}

		public static void FactionCommander_OnTarget(Mobile from, object obj)
		{
			if (obj is PlayerMobile)
			{
				var targ = (Mobile)obj;
				var pl = PlayerState.Find(targ);

				if (pl != null)
				{
					pl.Faction.Commander = targ;
					from.SendMessage("You have appointed them as the faction commander.");
				}
				else
				{
					from.SendMessage("They are not in a faction.");
				}
			}
			else
			{
				from.SendMessage("That is not a player.");
			}
		}

		public static void FactionElection_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendMessage("Target a faction stone to open its election properties.");
			e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(FactionElection_OnTarget));
		}

		public static void FactionElection_OnTarget(Mobile from, object obj)
		{
			if (obj is FactionStone fs)
			{
				var faction = fs.Faction;

				if (faction != null)
				{
					from.SendGump(new ElectionManagementGump(faction.Election));
				}
				else
				{
					from.SendMessage("That stone has no faction assigned.");
				}
			}
			else
			{
				from.SendMessage("That is not a faction stone.");
			}
		}

		public static void FactionKick_OnCommand(CommandEventArgs e)
		{
			e.Mobile.SendMessage("Target a player to remove them from their faction.");
			e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(FactionKick_OnTarget));
		}

		public static void FactionKick_OnTarget(Mobile from, object obj)
		{
			if (obj is Mobile mob)
			{
				var pl = PlayerState.Find(mob);

				if (pl != null)
				{
					pl.Faction.RemoveMember(mob);

					mob.SendMessage("You have been kicked from your faction.");
					from.SendMessage("They have been kicked from their faction.");
				}
				else
				{
					from.SendMessage("They are not in a faction.");
				}
			}
			else
			{
				from.SendMessage("That is not a player.");
			}
		}

		public static void ProcessTick()
		{
			var sigils = Sigil.Sigils;

			for (var i = 0; i < sigils.Count; ++i)
			{
				var sigil = sigils[i];

				if (!sigil.IsBeingCorrupted && sigil.GraceStart != DateTime.MinValue && sigil.GraceStart + Sigil.CorruptionGrace < DateTime.UtcNow)
				{
					if (sigil.LastMonolith is StrongholdMonolith && (sigil.Corrupted == null || sigil.LastMonolith.Faction != sigil.Corrupted))
					{
						sigil.Corrupting = sigil.LastMonolith.Faction;
						sigil.CorruptionStart = DateTime.UtcNow;
					}
					else
					{
						sigil.Corrupting = null;
						sigil.CorruptionStart = DateTime.MinValue;
					}

					sigil.GraceStart = DateTime.MinValue;
				}

				if (sigil.LastMonolith == null || sigil.LastMonolith.Sigil == null)
				{
					if (sigil.LastStolen + Sigil.ReturnPeriod < DateTime.UtcNow)
					{
						sigil.ReturnHome();
					}
				}
				else
				{
					if (sigil.IsBeingCorrupted && sigil.CorruptionStart + Sigil.CorruptionPeriod < DateTime.UtcNow)
					{
						sigil.Corrupted = sigil.Corrupting;
						sigil.Corrupting = null;
						sigil.CorruptionStart = DateTime.MinValue;
						sigil.GraceStart = DateTime.MinValue;
					}
					else if (sigil.IsPurifying && sigil.PurificationStart + Sigil.PurificationPeriod < DateTime.UtcNow)
					{
						sigil.PurificationStart = DateTime.MinValue;
						sigil.Corrupted = null;
						sigil.Corrupting = null;
						sigil.CorruptionStart = DateTime.MinValue;
						sigil.GraceStart = DateTime.MinValue;
					}
				}
			}
		}

		public static void HandleDeath(Mobile mob)
		{
			HandleDeath(mob, null);
		}

		#region Skill Loss

		public const double SkillLossFactor = 1.0 / 3;

		public static readonly TimeSpan SkillLossPeriod = TimeSpan.FromMinutes(20.0);

		private static readonly Dictionary<Mobile, SkillLossContext> m_SkillLoss = new();

		private class SkillLossContext
		{
			public Timer m_Timer;
			public List<SkillMod> m_Mods;
		}

		public static bool InSkillLoss(Mobile mob)
		{
			return m_SkillLoss.ContainsKey(mob);
		}

		public static void ApplySkillLoss(Mobile mob)
		{
			if (InSkillLoss(mob))
			{
				return;
			}

			var context = new SkillLossContext();

			m_SkillLoss[mob] = context;

			var mods = context.m_Mods = new List<SkillMod>();

			for (var i = 0; i < mob.Skills.Length; ++i)
			{
				var sk = mob.Skills[i];
				var baseValue = sk.Base;

				if (baseValue > 0)
				{
					var mod = new DefaultSkillMod(sk.SkillName, true, -(baseValue * SkillLossFactor));

					mods.Add(mod);
					mob.AddSkillMod(mod);
				}
			}

			context.m_Timer = Timer.DelayCall(SkillLossPeriod, ClearSkillLoss_Callback, mob);
		}

		private static void ClearSkillLoss_Callback(object state)
		{
			ClearSkillLoss((Mobile)state);
		}

		public static bool ClearSkillLoss(Mobile mob)
		{
			if (!m_SkillLoss.TryGetValue(mob, out var context))
			{
				return false;
			}

			m_SkillLoss.Remove(mob);

			var mods = context.m_Mods;

			for (var i = 0; i < mods.Count; ++i)
			{
				mob.RemoveSkillMod(mods[i]);
			}

			context.m_Timer.Stop();

			return true;
		}
		#endregion

		public int AwardSilver(Mobile mob, int silver)
		{
			if (silver <= 0)
			{
				return 0;
			}

			var tithed = silver * Tithe / 100;

			Silver += tithed;
			silver -= tithed;

			if (silver > 0)
			{
				mob.AddToBackpack(new Silver(silver));
			}

			return silver;
		}

		public static Faction FindSmallestFaction()
		{
			var factions = Factions;

			Faction smallest = null;

			for (var i = 0; i < factions.Count; ++i)
			{
				var faction = factions[i];

				if (smallest == null || faction.Members.Count < smallest.Members.Count)
				{
					smallest = faction;
				}
			}

			return smallest;
		}

		public static bool StabilityActive()
		{
			var factions = Factions;

			for (var i = 0; i < factions.Count; ++i)
			{
				var faction = factions[i];

				if (faction.Members.Count > StabilityActivation)
				{
					return true;
				}
			}

			return false;
		}

		public bool CanHandleInflux(int influx)
		{
			if (!StabilityActive())
			{
				return true;
			}

			var smallest = FindSmallestFaction();

			if (smallest == null)
			{
				return true; // sanity
			}

			if (StabilityFactor > 0 && (Members.Count + influx) * 100 / StabilityFactor > smallest.Members.Count)
			{
				return false;
			}

			return true;
		}

		public static void HandleDeath(Mobile victim, Mobile killer)
		{
			killer ??= victim.FindMostRecentDamager(true);

			var killerState = PlayerState.Find(killer);

			var pack = victim.Backpack;

			if (pack != null)
			{
				var killerPack = killer?.Backpack;

				foreach (var sigil in pack.FindItemsByType<Sigil>())
				{
					if (killerState != null && killerPack != null)
					{
						if (killer.GetDistanceToSqrt(victim) > 64)
						{
							sigil.ReturnHome();
							killer.SendLocalizedMessage(1042230); // The sigil has gone back to its home location.
						}
						else if (Sigil.ExistsOn(killer))
						{
							sigil.ReturnHome();
							killer.SendLocalizedMessage(1010258); // The sigil has gone back to its home location because you already have a sigil.
						}
						else if (!killerPack.TryDropItem(killer, sigil, false))
						{
							sigil.ReturnHome();
							killer.SendLocalizedMessage(1010259); // The sigil has gone home because your backpack is full.
						}
					}
					else
					{
						sigil.ReturnHome();
					}
				}
			}

			if (killerState == null)
			{
				return;
			}

			if (victim is BaseCreature bc)
			{
				var victimFaction = bc.FactionAllegiance;

				if (bc.Map == Facet && victimFaction != null && killerState.Faction != victimFaction)
				{
					var silver = killerState.Faction.AwardSilver(killer, bc.FactionSilverWorth);

					if (silver > 0)
					{
						killer.SendLocalizedMessage(1042748, silver.ToString("N0")); // Thou hast earned ~1_AMOUNT~ silver for vanquishing the vile creature.
					}
				}

				#region Ethics
				if (bc.Map == Facet && bc.GetEthicAllegiance(killer) == BaseCreature.Allegiance.Enemy)
				{
					var killerEPL = Ethics.Player.Find(killer);

					if (killerEPL != null && 100 - killerEPL.Power > Utility.Random(100))
					{
						++killerEPL.Power;
						++killerEPL.History;
					}
				}
				#endregion

				return;
			}

			var victimState = PlayerState.Find(victim);

			if (victimState == null)
			{
				return;
			}

			#region Dueling
			if (victim.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)))
			{
				return;
			}
			#endregion

			if (killer == victim || killerState.Faction != victimState.Faction)
			{
				ApplySkillLoss(victim);
			}

			if (killerState.Faction != victimState.Faction)
			{
				if (victimState.KillPoints <= -6)
				{
					killer.SendLocalizedMessage(501693); // This victim is not worth enough to get kill points from. 

					#region Ethics
					var killerEPL = Ethics.Player.Find(killer);
					var victimEPL = Ethics.Player.Find(victim);

					if (killerEPL != null && victimEPL != null && victimEPL.Power > 0 && victimState.CanGiveSilverTo(killer))
					{
						var powerTransfer = Math.Max(1, victimEPL.Power / 5);

						if (powerTransfer > 100 - killerEPL.Power)
						{
							powerTransfer = 100 - killerEPL.Power;
						}

						if (powerTransfer > 0)
						{
							victimEPL.Power -= (powerTransfer + 1) / 2;
							killerEPL.Power += powerTransfer;

							killerEPL.History += powerTransfer;

							victimState.OnGivenSilverTo(killer);
						}
					}
					#endregion
				}
				else
				{
					var award = Math.Max(victimState.KillPoints / 10, 1);

					if (award > 40)
					{
						award = 40;
					}

					if (victimState.CanGiveSilverTo(killer))
					{
						PowerFactionItem.CheckSpawn(killer, victim);

						if (victimState.KillPoints > 0)
						{
							victimState.IsActive = true;

							if (1 > Utility.Random(3))
							{
								killerState.IsActive = true;
							}

							var silver = killerState.Faction.AwardSilver(killer, award * 40);

							if (silver > 0)
							{
								killer.SendLocalizedMessage(1042736, String.Format("{0:N0} silver\t{1}", silver, victim.Name)); // You have earned ~1_SILVER_AMOUNT~ pieces for vanquishing ~2_PLAYER_NAME~!
							}
						}

						victimState.KillPoints -= award;
						killerState.KillPoints += award;

						var offset = award != 1 ? 0 : 2; // for pluralization

						var args = String.Format("{0}\t{1}\t{2}", award, victim.Name, killer.Name);

						killer.SendLocalizedMessage(1042737 + offset, args); // Thou hast been honored with ~1_KILL_POINTS~ kill point(s) for vanquishing ~2_DEAD_PLAYER~!
						victim.SendLocalizedMessage(1042738 + offset, args); // Thou has lost ~1_KILL_POINTS~ kill point(s) to ~3_ATTACKER_NAME~ for being vanquished!

						#region Ethics
						var killerEPL = Ethics.Player.Find(killer);
						var victimEPL = Ethics.Player.Find(victim);

						if (killerEPL != null && victimEPL != null && victimEPL.Power > 0)
						{
							var powerTransfer = Math.Max(1, victimEPL.Power / 5);

							if (powerTransfer > 100 - killerEPL.Power)
							{
								powerTransfer = 100 - killerEPL.Power;
							}

							if (powerTransfer > 0)
							{
								victimEPL.Power -= (powerTransfer + 1) / 2;
								killerEPL.Power += powerTransfer;

								killerEPL.History += powerTransfer;
							}
						}
						#endregion

						victimState.OnGivenSilverTo(killer);
					}
					else
					{
						killer.SendLocalizedMessage(1042231); // You have recently defeated this enemy and thus their death brings you no honor.
					}
				}
			}
		}

		private static void EventSink_Logout(LogoutEventArgs e)
		{
			var mob = e.Mobile;

			var pack = mob.Backpack;

			if (pack == null)
			{
				return;
			}

			foreach (var sigil in pack.FindItemsByType<Sigil>())
			{
				sigil.ReturnHome();
			}
		}

		private static void EventSink_Login(LoginEventArgs e)
		{
			CheckLeaveTimer(e.Mobile);
		}

		public static void WriteReference(GenericWriter writer, Faction fact)
		{
			var idx = Factions.IndexOf(fact);

			writer.WriteEncodedInt(idx + 1);
		}

		public static List<Faction> Factions => Reflector.Factions;

		public static Faction ReadReference(GenericReader reader)
		{
			var idx = reader.ReadEncodedInt() - 1;

			if (idx >= 0 && idx < Factions.Count)
			{
				return Factions[idx];
			}

			return null;
		}

		public static void HandleDeletion(Mobile mob)
		{
			var faction = Find(mob);

			faction?.RemoveMember(mob);
		}

		public static Faction Find(Mobile mob)
		{
			return Find(mob, false, false);
		}

		public static Faction Find(Mobile mob, bool inherit)
		{
			return Find(mob, inherit, false);
		}

		public static Faction Find(Mobile mob, bool inherit, bool creatureAllegiances)
		{
			var pl = PlayerState.Find(mob);

			if (pl != null)
			{
				return pl.Faction;
			}

			if (inherit && mob is BaseCreature bc)
			{
				if (bc.Controlled)
				{
					return Find(bc.ControlMaster, false);
				}
				
				if (bc.Summoned)
				{
					return Find(bc.SummonMaster, false);
				}
				
				if (creatureAllegiances && bc is BaseFactionGuard fg)
				{
					return fg.Faction;
				}
				
				if (creatureAllegiances)
				{
					return bc.FactionAllegiance;
				}
			}

			return null;
		}

		public static Faction Parse(string input)
		{
			if (input == null)
			{
				throw new ArgumentNullException(nameof(input));
			}

			return Factions.Find(fac => Insensitive.Equals(fac.Definition.Abbreviation, input) || Insensitive.Equals(fac.Definition.FriendlyName, input));
		}

		public static bool TryParse(string input, out Faction faction)
		{
			try
			{
				faction = Parse(input);

				return true;
			}
			catch
			{
				faction = null;

				return false;
			}
		}
	}

	public class FactionState
	{
		private const int BroadcastsPerPeriod = 2;

		private static readonly TimeSpan BroadcastPeriod = TimeSpan.FromHours(1.0);

		private readonly DateTime[] m_LastBroadcasts = new DateTime[BroadcastsPerPeriod];

		public Faction Faction { get; private set; }

		private Mobile m_Commander;

		public Mobile Commander
		{
			get => m_Commander;
			set
			{
				m_Commander?.InvalidateProperties();

				m_Commander = value;

				if (m_Commander != null)
				{
					m_Commander.SendLocalizedMessage(1042227); // You have been elected Commander of your faction

					m_Commander.InvalidateProperties();

					var pl = PlayerState.Find(m_Commander);

					if (pl != null && pl.Finance != null)
					{
						pl.Finance.Finance = null;
					}

					if (pl != null && pl.Sheriff != null)
					{
						pl.Sheriff.Sheriff = null;
					}
				}
			}
		}

		public int Tithe { get; set; } = 50;
		public int Silver { get; set; } = 0;

		public List<PlayerState> Members { get; } = new();
		public List<FactionItem> Items { get; } = new();
		public List<BaseFactionTrap> Traps { get; } = new();

		private Election m_Election;

		public Election Election => m_Election ??= (World.Loaded ? new Election(Faction) : null);

		private StrongholdRegion m_StrongholdRegion;

		public StrongholdRegion StrongholdRegion => m_StrongholdRegion ??= (World.Loaded ? new StrongholdRegion(Faction) : null);

		public DateTime LastAtrophy { get; private set; }

		public bool IsAtrophyReady => DateTime.UtcNow >= LastAtrophy.AddHours(47.0);
		
		public bool FactionMessageReady
		{
			get
			{
				for (var i = 0; i < m_LastBroadcasts.Length; ++i)
				{
					if (DateTime.UtcNow >= m_LastBroadcasts[i] + BroadcastPeriod)
					{
						return true;
					}
				}

				return false;
			}
		}

		public FactionState(Faction faction)
		{
			Faction = faction;
		}

		public FactionState(GenericReader reader)
		{
			Deserialize(reader);
		}

		public int CheckAtrophy()
		{
			if (DateTime.UtcNow < LastAtrophy.AddHours(47.0))
			{
				return 0;
			}

			var distrib = 0;

			LastAtrophy = DateTime.UtcNow;

			var members = new List<PlayerState>(Members);

			for (var i = 0; i < members.Count; ++i)
			{
				var ps = members[i];

				if (ps.IsActive)
				{
					ps.IsActive = false;
					continue;
				}
				
				if (ps.KillPoints > 0)
				{
					var atrophy = (ps.KillPoints + 9) / 10;

					ps.KillPoints -= atrophy;
					distrib += atrophy;
				}
			}

			return distrib;
		}

		public void RegisterBroadcast()
		{
			for (var i = 0; i < m_LastBroadcasts.Length; ++i)
			{
				if (DateTime.UtcNow >= m_LastBroadcasts[i] + BroadcastPeriod)
				{
					m_LastBroadcasts[i] = DateTime.UtcNow;
					break;
				}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(6); // version

			writer.Write(StrongholdRegion);

			writer.Write(LastAtrophy);

			writer.WriteEncodedInt(m_LastBroadcasts.Length);

			for (var i = 0; i < m_LastBroadcasts.Length; ++i)
			{
				writer.Write(m_LastBroadcasts[i]);
			}

			Election.Serialize(writer);

			Faction.WriteReference(writer, Faction);

			writer.Write(m_Commander);

			writer.WriteEncodedInt(Tithe);
			writer.WriteEncodedInt(Silver);

			writer.WriteEncodedInt(Members.Count);

			for (var i = 0; i < Members.Count; ++i)
			{
				var pl = Members[i];

				pl.Serialize(writer);
			}

			writer.WriteEncodedInt(Items.Count);

			for (var i = 0; i < Items.Count; ++i)
			{
				Items[i].Serialize(writer);
			}

			writer.WriteEncodedInt(Traps.Count);

			for (var i = 0; i < Traps.Count; ++i)
			{
				writer.Write(Traps[i]);
			}
		}

		public void Deserialize(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 6:
					{
						m_StrongholdRegion = reader.ReadRegion<StrongholdRegion>();
						goto case 5;
					}
				case 5:
					{
						LastAtrophy = reader.ReadDateTime();
						goto case 4;
					}
				case 4:
					{
						var count = reader.ReadEncodedInt();

						for (var i = 0; i < count; ++i)
						{
							var time = reader.ReadDateTime();

							if (i < m_LastBroadcasts.Length)
							{
								m_LastBroadcasts[i] = time;
							}
						}

						goto case 3;
					}
				case 3:
				case 2:
				case 1:
					{
						m_Election = new Election(reader);

						goto case 0;
					}
				case 0:
					{
						Faction = Faction.ReadReference(reader);

						Faction.State = this;

						m_Commander = reader.ReadMobile();

						if (version < 5)
						{
							LastAtrophy = DateTime.UtcNow;
						}

						if (version < 4)
						{
							var time = reader.ReadDateTime();

							if (m_LastBroadcasts.Length > 0)
							{
								m_LastBroadcasts[0] = time;
							}
						}

						Tithe = reader.ReadEncodedInt();
						Silver = reader.ReadEncodedInt();

						var memberCount = reader.ReadEncodedInt();

						for (var i = 0; i < memberCount; ++i)
						{
							var pl = new PlayerState(reader, Faction, Members);

							if (pl.Mobile != null)
							{
								Members.Add(pl);
							}
						}

						Faction.ZeroRankOffset = Members.Count;

						Members.Sort();

						for (var i = Members.Count - 1; i >= 0; i--)
						{
							var player = Members[i];

							if (player.KillPoints <= 0)
							{
								Faction.ZeroRankOffset = i;
							}
							else
							{
								player.RankIndex = i;
							}
						}

						if (version >= 2)
						{
							var factionItemCount = reader.ReadEncodedInt();

							for (var i = 0; i < factionItemCount; ++i)
							{
								var factionItem = new FactionItem(reader, Faction);

								Timer.DelayCall(factionItem.CheckAttach); // sandbox attachment
							}
						}

						if (version >= 3)
						{
							var factionTrapCount = reader.ReadEncodedInt();

							for (var i = 0; i < factionTrapCount; ++i)
							{
								var trap = reader.ReadItem<BaseFactionTrap>();

								if (trap != null && !trap.CheckDecay())
								{
									Traps.Add(trap);
								}
							}
						}

						break;
					}
			}
		}
	}

	public class FactionPersistance : Item
	{
		private static FactionPersistance m_Instance;

		public static FactionPersistance Instance => m_Instance;

		public override string DefaultName => "Faction Persistance - Internal";

		public FactionPersistance() : base(1)
		{
			Movable = false;

			if (m_Instance == null || m_Instance.Deleted)
			{
				m_Instance = this;
			}
			else
			{
				base.Delete();
			}
		}

		private enum PersistedType
		{
			Terminator,
			Faction,
			Town
		}

		public FactionPersistance(Serial serial) : base(serial)
		{
			m_Instance = this;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			var factions = Faction.Factions;

			for (var i = 0; i < factions.Count; ++i)
			{
				writer.WriteEncodedInt((int)PersistedType.Faction);

				factions[i].State.Serialize(writer);
			}

			var towns = Town.Towns;

			for (var i = 0; i < towns.Count; ++i)
			{
				writer.WriteEncodedInt((int)PersistedType.Town);

				towns[i].State.Serialize(writer);
			}

			writer.WriteEncodedInt((int)PersistedType.Terminator);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						PersistedType type;

						while ((type = (PersistedType)reader.ReadEncodedInt()) != PersistedType.Terminator)
						{
							switch (type)
							{
								case PersistedType.Faction: _ = new FactionState(reader); break;
								case PersistedType.Town: _ = new TownState(reader); break;
							}
						}

						break;
					}
			}
		}

		public override void Delete()
		{
		}
	}

	public class PlayerState : IComparable<PlayerState>
	{
		private readonly Mobile m_Mobile;
		private readonly Faction m_Faction;
		private readonly List<PlayerState> m_Owner;
		private int m_KillPoints;
		private DateTime m_Leaving;
		private MerchantTitle m_MerchantTitle;
		private RankDefinition m_Rank;
		private List<SilverGivenEntry> m_SilverGiven;
		private bool m_IsActive;

		private Town m_Sheriff;
		private Town m_Finance;

		private DateTime m_LastHonorTime;

		public Mobile Mobile => m_Mobile;
		public Faction Faction => m_Faction;
		public List<PlayerState> Owner => m_Owner;
		public MerchantTitle MerchantTitle { get => m_MerchantTitle; set { m_MerchantTitle = value; Invalidate(); } }
		public Town Sheriff { get => m_Sheriff; set { m_Sheriff = value; Invalidate(); } }
		public Town Finance { get => m_Finance; set { m_Finance = value; Invalidate(); } }
		public List<SilverGivenEntry> SilverGiven => m_SilverGiven;

		public int KillPoints
		{
			get => m_KillPoints;
			set
			{
				if (m_KillPoints != value)
				{
					if (value > m_KillPoints)
					{
						if (m_KillPoints <= 0)
						{
							if (value <= 0)
							{
								m_KillPoints = value;
								Invalidate();
								return;
							}

							m_Owner.Remove(this);
							m_Owner.Insert(m_Faction.ZeroRankOffset, this);

							m_RankIndex = m_Faction.ZeroRankOffset;
							m_Faction.ZeroRankOffset++;
						}
						while ((m_RankIndex - 1) >= 0)
						{
							var p = m_Owner[m_RankIndex - 1];
							if (value > p.KillPoints)
							{
								m_Owner[m_RankIndex] = p;
								m_Owner[m_RankIndex - 1] = this;
								RankIndex--;
								p.RankIndex++;
							}
							else
							{
								break;
							}
						}
					}
					else
					{
						if (value <= 0)
						{
							if (m_KillPoints <= 0)
							{
								m_KillPoints = value;
								Invalidate();
								return;
							}

							while ((m_RankIndex + 1) < m_Faction.ZeroRankOffset)
							{
								var p = m_Owner[m_RankIndex + 1];
								m_Owner[m_RankIndex + 1] = this;
								m_Owner[m_RankIndex] = p;
								RankIndex++;
								p.RankIndex--;
							}

							m_RankIndex = -1;
							m_Faction.ZeroRankOffset--;
						}
						else
						{
							while ((m_RankIndex + 1) < m_Faction.ZeroRankOffset)
							{
								var p = m_Owner[m_RankIndex + 1];
								if (value < p.KillPoints)
								{
									m_Owner[m_RankIndex + 1] = this;
									m_Owner[m_RankIndex] = p;
									RankIndex++;
									p.RankIndex--;
								}
								else
								{
									break;
								}
							}
						}
					}

					m_KillPoints = value;
					Invalidate();
				}
			}
		}

		private bool m_InvalidateRank = true;
		private int m_RankIndex = -1;

		public int RankIndex { get => m_RankIndex; set { if (m_RankIndex != value) { m_RankIndex = value; m_InvalidateRank = true; } } }

		public RankDefinition Rank
		{
			get
			{
				if (m_InvalidateRank)
				{
					var ranks = m_Faction.Definition.Ranks;
					int percent;

					if (m_Owner.Count == 1)
					{
						percent = 1000;
					}
					else if (m_RankIndex == -1)
					{
						percent = 0;
					}
					else
					{
						percent = ((m_Faction.ZeroRankOffset - m_RankIndex) * 1000) / m_Faction.ZeroRankOffset;
					}

					for (var i = 0; i < ranks.Length; i++)
					{
						var check = ranks[i];

						if (percent >= check.Required)
						{
							m_Rank = check;
							m_InvalidateRank = false;
							break;
						}
					}

					Invalidate();
				}

				return m_Rank;
			}
		}

		public DateTime LastHonorTime { get => m_LastHonorTime; set => m_LastHonorTime = value; }
		public DateTime Leaving { get => m_Leaving; set => m_Leaving = value; }
		public bool IsLeaving => (m_Leaving > DateTime.MinValue);

		public bool IsActive { get => m_IsActive; set => m_IsActive = value; }

		public bool CanGiveSilverTo(Mobile mob)
		{
			if (m_SilverGiven == null)
			{
				return true;
			}

			for (var i = 0; i < m_SilverGiven.Count; ++i)
			{
				var sge = m_SilverGiven[i];

				if (sge.IsExpired)
				{
					m_SilverGiven.RemoveAt(i--);
				}
				else if (sge.GivenTo == mob)
				{
					return false;
				}
			}

			return true;
		}

		public void OnGivenSilverTo(Mobile mob)
		{
			m_SilverGiven ??= new List<SilverGivenEntry>();

			m_SilverGiven.Add(new SilverGivenEntry(mob));
		}

		public void Invalidate()
		{
			if (m_Mobile is PlayerMobile pm)
			{
				pm.InvalidateProperties();
				pm.InvalidateMyRunUO();
			}
		}

		public void Attach()
		{
			if (m_Mobile is PlayerMobile pm)
			{
				pm.FactionPlayerState = this;
			}
		}

		public PlayerState(Mobile mob, Faction faction, List<PlayerState> owner)
		{
			m_Mobile = mob;
			m_Faction = faction;
			m_Owner = owner;

			Attach();
			Invalidate();
		}

		public PlayerState(GenericReader reader, Faction faction, List<PlayerState> owner)
		{
			m_Faction = faction;
			m_Owner = owner;

			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 1:
					{
						m_IsActive = reader.ReadBool();
						m_LastHonorTime = reader.ReadDateTime();
						goto case 0;
					}
				case 0:
					{
						m_Mobile = reader.ReadMobile();

						m_KillPoints = reader.ReadEncodedInt();
						m_MerchantTitle = (MerchantTitle)reader.ReadEncodedInt();

						m_Leaving = reader.ReadDateTime();

						break;
					}
			}

			Attach();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(1); // version

			writer.Write(m_IsActive);
			writer.Write(m_LastHonorTime);

			writer.Write(m_Mobile);

			writer.WriteEncodedInt(m_KillPoints);
			writer.WriteEncodedInt((int)m_MerchantTitle);

			writer.Write(m_Leaving);
		}

		public static PlayerState Find(Mobile mob)
		{
			if (mob is PlayerMobile pm)
			{
				return pm.FactionPlayerState;
			}

			return null;
		}

		public int CompareTo(PlayerState ps)
		{
			return ps.m_KillPoints - m_KillPoints;
		}
	}

	public abstract class FactionGump : Gump
	{
		public virtual int ButtonTypes => 10;

		public int ToButtonID(int type, int index)
		{
			return 1 + (index * ButtonTypes) + type;
		}

		public bool FromButtonID(int buttonID, out int type, out int index)
		{
			var offset = buttonID - 1;

			if (offset >= 0)
			{
				type = offset % ButtonTypes;
				index = offset / ButtonTypes;
				return true;
			}
			else
			{
				type = index = 0;
				return false;
			}
		}

		public static bool Exists(Mobile mob)
		{
			return (mob.FindGump(typeof(FactionGump)) != null);
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

		public FactionGump(int x, int y) : base(x, y)
		{
		}
	}


	/// Faction Election
	public enum ElectionState
	{
		Pending,
		Campaign,
		Election
	}

	public class Election
	{
		public static readonly TimeSpan PendingPeriod = TimeSpan.FromDays(5.0);
		public static readonly TimeSpan CampaignPeriod = TimeSpan.FromDays(1.0);
		public static readonly TimeSpan VotingPeriod = TimeSpan.FromDays(3.0);

		public const int MaxCandidates = 10;
		public const int CandidateRank = 5;

		private readonly Faction m_Faction;
		private readonly List<Candidate> m_Candidates;

		private ElectionState m_State;
		private DateTime m_LastStateTime;

		public Faction Faction => m_Faction;

		public List<Candidate> Candidates => m_Candidates;

		public ElectionState State { get => m_State; set { m_State = value; m_LastStateTime = DateTime.UtcNow; } }
		public DateTime LastStateTime => m_LastStateTime;

		[CommandProperty(AccessLevel.GameMaster)]
		public ElectionState CurrentState => m_State;

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public TimeSpan NextStateTime
		{
			get
			{
				var period = m_State switch
				{
					ElectionState.Election => VotingPeriod,
					ElectionState.Campaign => CampaignPeriod,
					_ => PendingPeriod,
				};

				var until = m_LastStateTime + period - DateTime.UtcNow;

				if (until < TimeSpan.Zero)
				{
					until = TimeSpan.Zero;
				}

				return until;
			}
			set
			{
				var period = m_State switch
				{
					ElectionState.Election => VotingPeriod,
					ElectionState.Campaign => CampaignPeriod,
					_ => PendingPeriod,
				};

				m_LastStateTime = DateTime.UtcNow - period + value;
			}
		}

		private Timer m_Timer;

		public void StartTimer()
		{
			m_Timer = Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), Slice);
		}

		public Election(Faction faction)
		{
			m_Faction = faction;
			m_Candidates = new List<Candidate>();

			StartTimer();
		}

		public Election(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
					{
						m_Faction = Faction.ReadReference(reader);

						m_LastStateTime = reader.ReadDateTime();
						m_State = (ElectionState)reader.ReadEncodedInt();

						m_Candidates = new List<Candidate>();

						var count = reader.ReadEncodedInt();

						for (var i = 0; i < count; ++i)
						{
							var cd = new Candidate(reader);

							if (cd.Mobile != null)
							{
								m_Candidates.Add(cd);
							}
						}

						break;
					}
			}

			StartTimer();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			Faction.WriteReference(writer, m_Faction);

			writer.Write(m_LastStateTime);
			writer.WriteEncodedInt((int)m_State);

			writer.WriteEncodedInt(m_Candidates.Count);

			for (var i = 0; i < m_Candidates.Count; ++i)
			{
				m_Candidates[i].Serialize(writer);
			}
		}

		public void AddCandidate(Mobile mob)
		{
			if (IsCandidate(mob))
			{
				return;
			}

			m_Candidates.Add(new Candidate(mob));
			mob.SendLocalizedMessage(1010117); // You are now running for office.
		}

		public void RemoveVoter(Mobile mob)
		{
			if (m_State == ElectionState.Election)
			{
				for (var i = 0; i < m_Candidates.Count; ++i)
				{
					var voters = m_Candidates[i].Voters;

					for (var j = 0; j < voters.Count; ++j)
					{
						var voter = voters[j];

						if (voter.From == mob)
						{
							voters.RemoveAt(j--);
						}
					}
				}
			}
		}

		public void RemoveCandidate(Mobile mob)
		{
			var cd = FindCandidate(mob);

			if (cd == null)
			{
				return;
			}

			m_Candidates.Remove(cd);
			mob.SendLocalizedMessage(1038031);

			if (m_State == ElectionState.Election)
			{
				if (m_Candidates.Count == 1)
				{
					m_Faction.Broadcast(1038031); // There are no longer any valid candidates in the Faction Commander election.

					var winner = m_Candidates[0];

					var winMob = winner.Mobile;
					var pl = PlayerState.Find(winMob);

					if (pl == null || pl.Faction != m_Faction || winMob == m_Faction.Commander)
					{
						m_Faction.Broadcast(1038026); // Faction leadership has not changed.
					}
					else
					{
						m_Faction.Broadcast(1038028); // The faction has a new commander.
						m_Faction.Commander = winMob;
					}

					m_Candidates.Clear();
					State = ElectionState.Pending;
				}
				else if (m_Candidates.Count == 0) // well, I guess this'll never happen
				{
					m_Faction.Broadcast(1038031); // There are no longer any valid candidates in the Faction Commander election.

					m_Candidates.Clear();
					State = ElectionState.Pending;
				}
			}
		}

		public bool IsCandidate(Mobile mob)
		{
			return (FindCandidate(mob) != null);
		}

		public bool CanVote(Mobile mob)
		{
			return (m_State == ElectionState.Election && !HasVoted(mob));
		}

		public bool HasVoted(Mobile mob)
		{
			return (FindVoter(mob) != null);
		}

		public Candidate FindCandidate(Mobile mob)
		{
			for (var i = 0; i < m_Candidates.Count; ++i)
			{
				if (m_Candidates[i].Mobile == mob)
				{
					return m_Candidates[i];
				}
			}

			return null;
		}

		public Candidate FindVoter(Mobile mob)
		{
			for (var i = 0; i < m_Candidates.Count; ++i)
			{
				var voters = m_Candidates[i].Voters;

				for (var j = 0; j < voters.Count; ++j)
				{
					var voter = voters[j];

					if (voter.From == mob)
					{
						return m_Candidates[i];
					}
				}
			}

			return null;
		}

		public bool CanBeCandidate(Mobile mob)
		{
			if (IsCandidate(mob))
			{
				return false;
			}

			if (m_Candidates.Count >= MaxCandidates)
			{
				return false;
			}

			if (m_State != ElectionState.Campaign)
			{
				return false; // sanity..
			}

			var pl = PlayerState.Find(mob);

			return pl != null && pl.Faction == m_Faction && pl.Rank.Rank >= CandidateRank;
		}

		public void Slice()
		{
			if (m_Faction.Election != this)
			{
				m_Timer?.Stop();
				m_Timer = null;

				return;
			}

			switch (m_State)
			{
				case ElectionState.Pending:
					{
						if ((m_LastStateTime + PendingPeriod) > DateTime.UtcNow)
						{
							break;
						}

						m_Faction.Broadcast(1038023); // Campaigning for the Faction Commander election has begun.

						m_Candidates.Clear();
						State = ElectionState.Campaign;

						break;
					}
				case ElectionState.Campaign:
					{
						if ((m_LastStateTime + CampaignPeriod) > DateTime.UtcNow)
						{
							break;
						}

						if (m_Candidates.Count == 0)
						{
							m_Faction.Broadcast(1038025); // Nobody ran for office.
							State = ElectionState.Pending;
						}
						else if (m_Candidates.Count == 1)
						{
							m_Faction.Broadcast(1038029); // Only one member ran for office.

							var winner = m_Candidates[0];

							var mob = winner.Mobile;
							var pl = PlayerState.Find(mob);

							if (pl == null || pl.Faction != m_Faction || mob == m_Faction.Commander)
							{
								m_Faction.Broadcast(1038026); // Faction leadership has not changed.
							}
							else
							{
								m_Faction.Broadcast(1038028); // The faction has a new commander.
								m_Faction.Commander = mob;
							}

							m_Candidates.Clear();
							State = ElectionState.Pending;
						}
						else
						{
							m_Faction.Broadcast(1038030);
							State = ElectionState.Election;
						}

						break;
					}
				case ElectionState.Election:
					{
						if ((m_LastStateTime + VotingPeriod) > DateTime.UtcNow)
						{
							break;
						}

						m_Faction.Broadcast(1038024); // The results for the Faction Commander election are in

						Candidate winner = null;

						for (var i = 0; i < m_Candidates.Count; ++i)
						{
							var cd = m_Candidates[i];

							var pl = PlayerState.Find(cd.Mobile);

							if (pl == null || pl.Faction != m_Faction)
							{
								continue;
							}

							//cd.CleanMuleVotes();

							if (winner == null || cd.Votes > winner.Votes)
							{
								winner = cd;
							}
						}

						if (winner == null)
						{
							m_Faction.Broadcast(1038026); // Faction leadership has not changed.
						}
						else if (winner.Mobile == m_Faction.Commander)
						{
							m_Faction.Broadcast(1038027); // The incumbent won the election.
						}
						else
						{
							m_Faction.Broadcast(1038028); // The faction has a new commander.
							m_Faction.Commander = winner.Mobile;
						}

						m_Candidates.Clear();
						State = ElectionState.Pending;

						break;
					}
			}
		}
	}

	public class Voter
	{
		public Mobile Candidate { get; private set; }
		public Mobile From { get; private set; }
		public IPAddress Address { get; private set; }
		public DateTime Time { get; private set; }

		public object[] AcquireFields()
		{
			var gameTime = TimeSpan.Zero;

			if (From is PlayerMobile pm)
			{
				gameTime = pm.GameTime;
			}

			var kp = 0;

			var pl = PlayerState.Find(From);

			if (pl != null)
			{
				kp = pl.KillPoints;
			}

			var sk = From.Skills.Total;

			var factorSkills = 50 + (sk * 100 / 10000);
			var factorKillPts = 100 + (kp * 2);
			var factorGameTime = 50 + (int)(gameTime.Ticks * 100 / TimeSpan.TicksPerDay);

			var totalFactor = factorSkills * factorKillPts * Math.Max(factorGameTime, 100) / 10000;

			if (totalFactor > 100)
			{
				totalFactor = 100;
			}
			else if (totalFactor < 0)
			{
				totalFactor = 0;
			}

			return new object[] { From, Address, Time, totalFactor };
		}

		public Voter(Mobile from, Mobile candidate)
		{
			From = from;
			Candidate = candidate;

			if (From.NetState != null)
			{
				Address = From.NetState.Address;
			}
			else
			{
				Address = IPAddress.None;
			}

			Time = DateTime.UtcNow;
		}

		public Voter(GenericReader reader, Mobile candidate)
		{
			Candidate = candidate;

			Deserialize(reader);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.Write(From);
			writer.Write(Address);
			writer.Write(Time);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.ReadEncodedInt();

			From = reader.ReadMobile();
			Address = Utility.Intern(reader.ReadIPAddress());
			Time = reader.ReadDateTime();
		}
	}

	public class Candidate
	{
		public Mobile Mobile { get; }
		public List<Voter> Voters { get; }

		public int Votes => Voters.Count;

		public void CleanMuleVotes()
		{
			for (var i = 0; i < Voters.Count; ++i)
			{
				var voter = Voters[i];

				if ((int)voter.AcquireFields()[3] < 90)
				{
					Voters.RemoveAt(i--);
				}
			}
		}

		public Candidate(Mobile mob)
		{
			Mobile = mob;
			Voters = new List<Voter>();
		}

		public Candidate(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 1:
					{
						Mobile = reader.ReadMobile();

						var count = reader.ReadEncodedInt();
						Voters = new List<Voter>(count);

						for (var i = 0; i < count; ++i)
						{
							var voter = new Voter(reader, Mobile);

							if (voter.From != null)
							{
								Voters.Add(voter);
							}
						}

						break;
					}
				case 0:
					{
						Mobile = reader.ReadMobile();

						var mobs = reader.ReadStrongMobileList();
						Voters = new List<Voter>(mobs.Count);

						for (var i = 0; i < mobs.Count; ++i)
						{
							Voters.Add(new Voter(mobs[i], Mobile));
						}

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(1); // version

			writer.Write(Mobile);

			writer.WriteEncodedInt(Voters.Count);

			for (var i = 0; i < Voters.Count; ++i)
			{
				Voters[i].Serialize(writer);
			}
		}
	}

	public class ElectionGump : FactionGump
	{
		private readonly PlayerMobile m_From;
		private readonly Election m_Election;

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			switch (info.ButtonID)
			{
				case 0: // back
					{
						m_From.SendGump(new FactionStoneGump(m_From, m_Election.Faction));
						break;
					}
				case 1: // vote
					{
						if (m_Election.State == ElectionState.Election)
						{
							m_From.SendGump(new VoteGump(m_From, m_Election));
						}

						break;
					}
				case 2: // campaign
					{
						if (m_Election.CanBeCandidate(m_From))
						{
							m_Election.AddCandidate(m_From);
						}

						break;
					}
			}
		}

		public ElectionGump(PlayerMobile from, Election election) : base(50, 50)
		{
			m_From = from;
			m_Election = election;

			AddPage(0);

			AddBackground(0, 0, 420, 180, 5054);
			AddBackground(10, 10, 400, 160, 3000);

			AddHtmlText(20, 20, 380, 20, election.Faction.Definition.Header, false, false);

			// NOTE: Gump not entirely OSI-accurate, intentionally so

			switch (election.State)
			{
				case ElectionState.Pending:
					{
						var toGo = (election.LastStateTime + Election.PendingPeriod) - DateTime.UtcNow;
						var days = (int)(toGo.TotalDays + 0.5);

						AddHtmlLocalized(20, 40, 380, 20, 1038034, false, false); // A new election campaign is pending

						if (days > 0)
						{
							AddHtmlLocalized(20, 60, 280, 20, 1018062, false, false); // Days until next election :
							AddLabel(300, 60, 0, days.ToString());
						}
						else
						{
							AddHtmlLocalized(20, 60, 280, 20, 1018059, false, false); // Election campaigning begins tonight.
						}

						break;
					}
				case ElectionState.Campaign:
					{
						var toGo = (election.LastStateTime + Election.CampaignPeriod) - DateTime.UtcNow;
						var days = (int)(toGo.TotalDays + 0.5);

						AddHtmlLocalized(20, 40, 380, 20, 1018058, false, false); // There is an election campaign in progress.

						if (days > 0)
						{
							AddHtmlLocalized(20, 60, 280, 20, 1038033, false, false); // Days to go:
							AddLabel(300, 60, 0, days.ToString());
						}
						else
						{
							AddHtmlLocalized(20, 60, 280, 20, 1018061, false, false); // Campaign in progress. Voting begins tonight.
						}

						if (m_Election.CanBeCandidate(m_From))
						{
							AddButton(20, 110, 4005, 4007, 2, GumpButtonType.Reply, 0);
							AddHtmlLocalized(55, 110, 350, 20, 1011427, false, false); // CAMPAIGN FOR LEADERSHIP
						}
						else
						{
							var pl = PlayerState.Find(m_From);

							if (pl == null || pl.Rank.Rank < Election.CandidateRank)
							{
								AddHtmlLocalized(20, 100, 380, 20, 1010118, false, false); // You must have a higher rank to run for office
							}
						}

						break;
					}
				case ElectionState.Election:
					{
						var toGo = (election.LastStateTime + Election.VotingPeriod) - DateTime.UtcNow;
						var days = (int)Math.Ceiling(toGo.TotalDays);

						AddHtmlLocalized(20, 40, 380, 20, 1018060, false, false); // There is an election vote in progress.

						AddHtmlLocalized(20, 60, 280, 20, 1038033, false, false);
						AddLabel(300, 60, 0, days.ToString());

						AddHtmlLocalized(55, 100, 380, 20, 1011428, false, false); // VOTE FOR LEADERSHIP
						AddButton(20, 100, 4005, 4007, 1, GumpButtonType.Reply, 0);

						break;
					}
			}

			AddButton(20, 140, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 140, 350, 20, 1011012, false, false); // CANCEL
		}
	}

	public class ElectionManagementGump : Gump
	{
		public const int LabelColor = 0xFFFFFF;

		public static string Right(string text)
		{
			return String.Format("<DIV ALIGN=RIGHT>{0}</DIV>", text);
		}

		public static string Center(string text)
		{
			return String.Format("<CENTER>{0}</CENTER>", text);
		}

		public static string Color(string text, int color)
		{
			return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
		}

		public static string FormatTimeSpan(TimeSpan ts)
		{
			return String.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", ts.Days, ts.Hours % 24, ts.Minutes % 60, ts.Seconds % 60);
		}

		private readonly Election m_Election;
		private readonly Candidate m_Candidate;
		private readonly int m_Page;

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var from = sender.Mobile;
			var bid = info.ButtonID;

			if (m_Candidate == null)
			{
				if (bid == 0)
				{
				}
				else if (bid == 1)
				{
				}
				else
				{
					bid -= 2;

					if (bid >= 0 && bid < m_Election.Candidates.Count)
					{
						from.SendGump(new ElectionManagementGump(m_Election, m_Election.Candidates[bid], 0));
					}
				}
			}
			else
			{
				if (bid == 0)
				{
					from.SendGump(new ElectionManagementGump(m_Election));
				}
				else if (bid == 1)
				{
					m_Election.RemoveCandidate(m_Candidate.Mobile);
					from.SendGump(new ElectionManagementGump(m_Election));
				}
				else if (bid == 2 && m_Page > 0)
				{
					from.SendGump(new ElectionManagementGump(m_Election, m_Candidate, m_Page - 1));
				}
				else if (bid == 3 && (m_Page + 1) * 10 < m_Candidate.Voters.Count)
				{
					from.SendGump(new ElectionManagementGump(m_Election, m_Candidate, m_Page + 1));
				}
				else
				{
					bid -= 4;

					if (bid >= 0 && bid < m_Candidate.Voters.Count)
					{
						m_Candidate.Voters.RemoveAt(bid);
						from.SendGump(new ElectionManagementGump(m_Election, m_Candidate, m_Page));
					}
				}
			}
		}

		public ElectionManagementGump(Election election) : this(election, null, 0)
		{
		}

		public ElectionManagementGump(Election election, Candidate candidate, int page) : base(40, 40)
		{
			m_Election = election;
			m_Candidate = candidate;
			m_Page = page;

			AddPage(0);

			if (candidate != null)
			{
				AddBackground(0, 0, 448, 354, 9270);
				AddAlphaRegion(10, 10, 428, 334);

				AddHtml(10, 10, 428, 20, Color(Center("Candidate Management"), LabelColor), false, false);

				AddHtml(45, 35, 100, 20, Color("Player Name:", LabelColor), false, false);
				AddHtml(145, 35, 100, 20, Color(candidate.Mobile == null ? "null" : candidate.Mobile.Name, LabelColor), false, false);

				AddHtml(45, 55, 100, 20, Color("Vote Count:", LabelColor), false, false);
				AddHtml(145, 55, 100, 20, Color(candidate.Votes.ToString(), LabelColor), false, false);

				AddButton(12, 73, 4005, 4007, 1, GumpButtonType.Reply, 0);
				AddHtml(45, 75, 100, 20, Color("Drop Candidate", LabelColor), false, false);

				AddImageTiled(13, 99, 422, 242, 9264);
				AddImageTiled(14, 100, 420, 240, 9274);
				AddAlphaRegion(14, 100, 420, 240);

				AddHtml(14, 100, 420, 20, Color(Center("Voters"), LabelColor), false, false);

				if (page > 0)
				{
					AddButton(397, 104, 0x15E3, 0x15E7, 2, GumpButtonType.Reply, 0);
				}
				else
				{
					AddImage(397, 104, 0x25EA);
				}

				if ((page + 1) * 10 < candidate.Voters.Count)
				{
					AddButton(414, 104, 0x15E1, 0x15E5, 3, GumpButtonType.Reply, 0);
				}
				else
				{
					AddImage(414, 104, 0x25E6);
				}

				AddHtml(14, 120, 30, 20, Color(Center("DEL"), LabelColor), false, false);
				AddHtml(47, 120, 150, 20, Color("Name", LabelColor), false, false);
				AddHtml(195, 120, 100, 20, Color(Center("Address"), LabelColor), false, false);
				AddHtml(295, 120, 80, 20, Color(Center("Time"), LabelColor), false, false);
				AddHtml(355, 120, 60, 20, Color(Center("Legit"), LabelColor), false, false);

				var idx = 0;

				for (var i = page * 10; i >= 0 && i < candidate.Voters.Count && i < (page + 1) * 10; ++i, ++idx)
				{
					var voter = candidate.Voters[i];

					AddButton(13, 138 + (idx * 20), 4002, 4004, 4 + i, GumpButtonType.Reply, 0);

					var fields = voter.AcquireFields();

					var x = 45;

					for (var j = 0; j < fields.Length; ++j)
					{
						var obj = fields[j];

						if (obj is Mobile mob)
						{
							AddHtml(x + 2, 140 + (idx * 20), 150, 20, Color(mob.Name, LabelColor), false, false);
							x += 150;
						}
						else if (obj is IPAddress ip)
						{
							AddHtml(x, 140 + (idx * 20), 100, 20, Color(Center(ip.ToString()), LabelColor), false, false);
							x += 100;
						}
						else if (obj is DateTime dt)
						{
							AddHtml(x, 140 + (idx * 20), 80, 20, Color(Center(FormatTimeSpan(dt - election.LastStateTime)), LabelColor), false, false);
							x += 80;
						}
						else if (obj is int n)
						{
							AddHtml(x, 140 + (idx * 20), 60, 20, Color(Center(n + "%"), LabelColor), false, false);
							x += 60;
						}
					}
				}
			}
			else
			{
				AddBackground(0, 0, 288, 334, 9270);
				AddAlphaRegion(10, 10, 268, 314);

				AddHtml(10, 10, 268, 20, Color(Center("Election Management"), LabelColor), false, false);

				AddHtml(45, 35, 100, 20, Color("Current State:", LabelColor), false, false);
				AddHtml(145, 35, 100, 20, Color(election.State.ToString(), LabelColor), false, false);

				AddButton(12, 53, 4005, 4007, 1, GumpButtonType.Reply, 0);
				AddHtml(45, 55, 100, 20, Color("Transition Time:", LabelColor), false, false);
				AddHtml(145, 55, 100, 20, Color(FormatTimeSpan(election.NextStateTime), LabelColor), false, false);

				AddImageTiled(13, 79, 262, 242, 9264);
				AddImageTiled(14, 80, 260, 240, 9274);
				AddAlphaRegion(14, 80, 260, 240);

				AddHtml(14, 80, 260, 20, Color(Center("Candidates"), LabelColor), false, false);
				AddHtml(14, 100, 30, 20, Color(Center("-->"), LabelColor), false, false);
				AddHtml(47, 100, 150, 20, Color("Name", LabelColor), false, false);
				AddHtml(195, 100, 80, 20, Color(Center("Votes"), LabelColor), false, false);

				for (var i = 0; i < election.Candidates.Count; ++i)
				{
					var cd = election.Candidates[i];
					var mob = cd.Mobile;

					if (mob == null)
					{
						continue;
					}

					AddButton(13, 118 + (i * 20), 4005, 4007, 2 + i, GumpButtonType.Reply, 0);
					AddHtml(47, 120 + (i * 20), 150, 20, Color(mob.Name, LabelColor), false, false);
					AddHtml(195, 120 + (i * 20), 80, 20, Color(Center(cd.Votes.ToString()), LabelColor), false, false);
				}
			}
		}
	}

	public class VoteGump : FactionGump
	{
		private readonly PlayerMobile m_From;
		private readonly Election m_Election;

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 0)
			{
				m_From.SendGump(new FactionStoneGump(m_From, m_Election.Faction));
			}
			else
			{
				if (!m_Election.CanVote(m_From))
				{
					return;
				}

				var index = info.ButtonID - 1;

				if (index >= 0 && index < m_Election.Candidates.Count)
				{
					m_Election.Candidates[index].Voters.Add(new Voter(m_From, m_Election.Candidates[index].Mobile));
				}

				m_From.SendGump(new VoteGump(m_From, m_Election));
			}
		}

		public VoteGump(PlayerMobile from, Election election) : base(50, 50)
		{
			m_From = from;
			m_Election = election;

			var canVote = election.CanVote(from);

			AddPage(0);

			AddBackground(0, 0, 420, 350, 5054);
			AddBackground(10, 10, 400, 330, 3000);

			AddHtmlText(20, 20, 380, 20, election.Faction.Definition.Header, false, false);

			if (canVote)
			{
				AddHtmlLocalized(20, 60, 380, 20, 1011428, false, false); // VOTE FOR LEADERSHIP
			}
			else
			{
				AddHtmlLocalized(20, 60, 380, 20, 1038032, false, false); // You have already voted in this election.
			}

			for (var i = 0; i < election.Candidates.Count; ++i)
			{
				var cd = election.Candidates[i];

				if (canVote)
				{
					AddButton(20, 100 + (i * 20), 4005, 4007, i + 1, GumpButtonType.Reply, 0);
				}

				AddLabel(55, 100 + (i * 20), 0, cd.Mobile.Name);
				AddLabel(300, 100 + (i * 20), 0, cd.Votes.ToString());
			}

			AddButton(20, 310, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 310, 100, 20, 1011012, false, false); // CANCEL
		}
	}

	public class TownReputationDefinition : ReputationDefinition<Town>
	{
		public TownReputationDefinition(Town owner, string name, string description, params int[] levels)
			: base(owner, ReputationCategory.Townships, name, description, levels)
		{ }
	}

	/// Faction Town
	public class TownDefinition
	{
		private readonly int m_Sort;
		private readonly int m_SigilID;

		private readonly string m_Region;

		private readonly string m_FriendlyName;

		private readonly TextDefinition m_TownName;
		private readonly TextDefinition m_TownStoneHeader;
		private readonly TextDefinition m_StrongholdMonolithName;
		private readonly TextDefinition m_TownMonolithName;
		private readonly TextDefinition m_TownStoneName;
		private readonly TextDefinition m_SigilName;
		private readonly TextDefinition m_CorruptedSigilName;

		private readonly Point3D m_Monolith;
		private readonly Point3D m_TownStone;

		private readonly TownReputationDefinition m_Reputation;

		public int Sort => m_Sort;
		public int SigilID => m_SigilID;

		public string Region => m_Region;
		public string FriendlyName => m_FriendlyName;

		public TextDefinition TownName => m_TownName;
		public TextDefinition TownStoneHeader => m_TownStoneHeader;
		public TextDefinition StrongholdMonolithName => m_StrongholdMonolithName;
		public TextDefinition TownMonolithName => m_TownMonolithName;
		public TextDefinition TownStoneName => m_TownStoneName;
		public TextDefinition SigilName => m_SigilName;
		public TextDefinition CorruptedSigilName => m_CorruptedSigilName;

		public Point3D Monolith => m_Monolith;
		public Point3D TownStone => m_TownStone;

		public TownReputationDefinition Reputation => m_Reputation;

		public TownDefinition(int sort, int sigilID, string region, string friendlyName, TextDefinition townName, TextDefinition townStoneHeader, TextDefinition strongholdMonolithName, TextDefinition townMonolithName, TextDefinition townStoneName, TextDefinition sigilName, TextDefinition corruptedSigilName, Point3D monolith, Point3D townStone, TownReputationDefinition reputation)
		{
			m_Sort = sort;
			m_SigilID = sigilID;
			m_Region = region;
			m_FriendlyName = friendlyName;
			m_TownName = townName;
			m_TownStoneHeader = townStoneHeader;
			m_StrongholdMonolithName = strongholdMonolithName;
			m_TownMonolithName = townMonolithName;
			m_TownStoneName = townStoneName;
			m_SigilName = sigilName;
			m_CorruptedSigilName = corruptedSigilName;
			m_Monolith = monolith;
			m_TownStone = townStone;
			m_Reputation = reputation;
		}
	}

	[Parsable, CustomEnum(new string[] { "Britain", "Magincia", "Minoc", "Moonglow", "Skara Brae", "Trinsic", "Vesper", "Yew" })]
	public abstract class Town : IComparable, IComparable<Town>
	{
		private TownDefinition m_Definition;
		private TownState m_State;

		public TownDefinition Definition
		{
			get => m_Definition;
			set => m_Definition = value;
		}

		public TownState State
		{
			get => m_State;
			set
			{
				m_State = value;
				ConstructGuardLists();
			}
		}

		public int Silver
		{
			get => m_State.Silver;
			set => m_State.Silver = value;
		}

		public Faction Owner
		{
			get => m_State.Owner;
			set => Capture(value);
		}

		public Mobile Sheriff
		{
			get => m_State.Sheriff;
			set => m_State.Sheriff = value;
		}

		public Mobile Finance
		{
			get => m_State.Finance;
			set => m_State.Finance = value;
		}

		public int Tax
		{
			get => m_State.Tax;
			set => m_State.Tax = value;
		}

		public DateTime LastTaxChange
		{
			get => m_State.LastTaxChange;
			set => m_State.LastTaxChange = value;
		}

		public TownReputationDefinition Reputation => m_Definition?.Reputation;

		public static readonly TimeSpan TaxChangePeriod = TimeSpan.FromHours(12.0);
		public static readonly TimeSpan IncomePeriod = TimeSpan.FromDays(1.0);

		public bool TaxChangeReady => (m_State.LastTaxChange + TaxChangePeriod) < DateTime.UtcNow;

		public static Town FromRegion(Region reg)
		{
			if (reg.Map != Faction.Facet)
			{
				return null;
			}

			var towns = Towns;

			for (var i = 0; i < towns.Count; ++i)
			{
				var town = towns[i];

				if (reg.IsPartOf(town.Definition.Region))
				{
					return town;
				}
			}

			return null;
		}

		public int FinanceUpkeep
		{
			get
			{
				var vendorLists = VendorLists;
				var upkeep = 0;

				for (var i = 0; i < vendorLists.Count; ++i)
				{
					upkeep += vendorLists[i].Vendors.Count * vendorLists[i].Definition.Upkeep;
				}

				return upkeep;
			}
		}

		public int SheriffUpkeep
		{
			get
			{
				var guardLists = GuardLists;
				var upkeep = 0;

				for (var i = 0; i < guardLists.Count; ++i)
				{
					upkeep += guardLists[i].Guards.Count * guardLists[i].Definition.Upkeep;
				}

				return upkeep;
			}
		}

		public int DailyIncome => (10000 * (100 + m_State.Tax)) / 100;

		public int NetCashFlow => DailyIncome - FinanceUpkeep - SheriffUpkeep;

		public TownMonolith Monolith
		{
			get
			{
				var monoliths = BaseMonolith.Monoliths;

				foreach (var monolith in monoliths)
				{
					if (monolith is TownMonolith townMonolith && townMonolith.Town == this)
					{
						return townMonolith;
					}
				}

				return null;
			}
		}

		public DateTime LastIncome
		{
			get => m_State.LastIncome;
			set => m_State.LastIncome = value;
		}

		public void BeginOrderFiring(Mobile from)
		{
			var isFinance = IsFinance(from);
			var isSheriff = IsSheriff(from);
			string type = null;

			// NOTE: Messages not OSI-accurate, intentional
			if (isFinance && isSheriff) // GM only
			{
				type = "vendor or guard";
			}
			else if (isFinance)
			{
				type = "vendor";
			}
			else if (isSheriff)
			{
				type = "guard";
			}

			from.SendMessage("Target the {0} you wish to dismiss.", type);
			from.BeginTarget(12, false, TargetFlags.None, new TargetCallback(EndOrderFiring));
		}

		public void EndOrderFiring(Mobile from, object obj)
		{
			var isFinance = IsFinance(from);
			var isSheriff = IsSheriff(from);

			string type = null;

			if (isFinance && isSheriff) // GM only
			{
				type = "vendor or guard";
			}
			else if (isFinance)
			{
				type = "vendor";
			}
			else if (isSheriff)
			{
				type = "guard";
			}

			if (obj is BaseFactionVendor vendor)
			{
				if (vendor.Town == this && isFinance)
				{
					vendor.Delete();
				}
			}
			else if (obj is BaseFactionGuard guard)
			{
				if (guard.Town == this && isSheriff)
				{
					guard.Delete();
				}
			}
			else
			{
				from.SendMessage("That is not a {0}!", type);
			}
		}

		private Timer m_IncomeTimer;

		public void StartIncomeTimer()
		{
			m_IncomeTimer?.Stop();

			m_IncomeTimer = Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), CheckIncome);
		}

		public void StopIncomeTimer()
		{
			m_IncomeTimer?.Stop();

			m_IncomeTimer = null;
		}

		public void CheckIncome()
		{
			if (LastIncome + IncomePeriod > DateTime.UtcNow || Owner == null)
			{
				return;
			}

			ProcessIncome();
		}

		public void ProcessIncome()
		{
			LastIncome = DateTime.UtcNow;

			var flow = NetCashFlow;

			if (Silver + flow < 0)
			{
				var toDelete = BuildFinanceList();

				while (Silver + flow < 0 && toDelete.Count > 0)
				{
					var index = Utility.Random(toDelete.Count);
					var mob = toDelete[index];

					mob.Delete();

					toDelete.RemoveAt(index);
					flow = NetCashFlow;
				}
			}

			Silver += flow;
		}

		public List<Mobile> BuildFinanceList()
		{
			var list = new List<Mobile>();

			var vendorLists = VendorLists;

			for (var i = 0; i < vendorLists.Count; ++i)
			{
				list.AddRange(vendorLists[i].Vendors);
			}

			var guardLists = GuardLists;

			for (var i = 0; i < guardLists.Count; ++i)
			{
				list.AddRange(guardLists[i].Guards);
			}

			return list;
		}

		public List<VendorList> VendorLists { get; set; }
		public List<GuardList> GuardLists { get; set; }

		public void ConstructGuardLists()
		{
			var defs = Owner?.Definition?.Guards ?? Array.Empty<GuardDefinition>();

			GuardLists = new List<GuardList>();

			for (var i = 0; i < defs.Length; ++i)
			{
				GuardLists.Add(new GuardList(defs[i]));
			}
		}

		public GuardList FindGuardList(Type type)
		{
			var guardLists = GuardLists;

			for (var i = 0; i < guardLists.Count; ++i)
			{
				var guardList = guardLists[i];

				if (guardList.Definition.Type == type)
				{
					return guardList;
				}
			}

			return null;
		}

		public void ConstructVendorLists()
		{
			var defs = VendorDefinition.Definitions;

			VendorLists = new List<VendorList>();

			for (var i = 0; i < defs.Length; ++i)
			{
				VendorLists.Add(new VendorList(defs[i]));
			}
		}

		public VendorList FindVendorList(Type type)
		{
			var vendorLists = VendorLists;

			for (var i = 0; i < vendorLists.Count; ++i)
			{
				var vendorList = vendorLists[i];

				if (vendorList.Definition.Type == type)
				{
					return vendorList;
				}
			}

			return null;
		}

		public bool RegisterGuard(BaseFactionGuard guard)
		{
			if (guard == null)
			{
				return false;
			}

			var guardList = FindGuardList(guard.GetType());

			if (guardList == null)
			{
				return false;
			}

			guardList.Guards.Add(guard);
			return true;
		}

		public bool UnregisterGuard(BaseFactionGuard guard)
		{
			if (guard == null)
			{
				return false;
			}

			var guardList = FindGuardList(guard.GetType());

			if (guardList == null)
			{
				return false;
			}

			if (!guardList.Guards.Remove(guard))
			{
				return false;
			}

			return true;
		}

		public bool RegisterVendor(BaseFactionVendor vendor)
		{
			if (vendor == null)
			{
				return false;
			}

			var vendorList = FindVendorList(vendor.GetType());

			if (vendorList == null)
			{
				return false;
			}

			vendorList.Vendors.Add(vendor);
			return true;
		}

		public bool UnregisterVendor(BaseFactionVendor vendor)
		{
			if (vendor == null)
			{
				return false;
			}

			var vendorList = FindVendorList(vendor.GetType());

			if (vendorList == null)
			{
				return false;
			}

			if (!vendorList.Vendors.Remove(vendor))
			{
				return false;
			}

			return true;
		}

		public static void Initialize()
		{
			var towns = Towns;

			for (var i = 0; i < towns.Count; ++i)
			{
				towns[i].Sheriff = towns[i].Sheriff;
				towns[i].Finance = towns[i].Finance;
			}

			CommandSystem.Register("GrantTownSilver", AccessLevel.Administrator, GrantTownSilver_OnCommand);
		}

		public Town()
		{
			m_State = new TownState(this);

			ConstructVendorLists();
			ConstructGuardLists();
			StartIncomeTimer();
		}

		public bool IsSheriff(Mobile mob)
		{
			if (mob == null || mob.Deleted)
			{
				return false;
			}

			return mob.AccessLevel >= AccessLevel.GameMaster || mob == Sheriff;
		}

		public bool IsFinance(Mobile mob)
		{
			if (mob == null || mob.Deleted)
			{
				return false;
			}

			return mob.AccessLevel >= AccessLevel.GameMaster || mob == Finance;
		}

		public static List<Town> Towns => Reflector.Towns;

		public const int SilverCaptureBonus = 10000;

		public void Capture(Faction f)
		{
			if (m_State.Owner == f)
			{
				return;
			}

			if (m_State.Owner == null) // going from unowned to owned
			{
				LastIncome = DateTime.UtcNow;
				f.Silver += SilverCaptureBonus;
			}
			else if (f == null) // going from owned to unowned
			{
				LastIncome = DateTime.MinValue;
			}
			else // otherwise changing hands, income timer doesn't change
			{
				f.Silver += SilverCaptureBonus;
			}

			m_State.Owner = f;

			Sheriff = null;
			Finance = null;

			var monolith = Monolith;

			if (monolith != null)
			{
				monolith.Faction = f;
			}

			var vendorLists = VendorLists;

			for (var i = 0; i < vendorLists.Count; ++i)
			{
				var vendorList = vendorLists[i];
				var vendors = vendorList.Vendors;

				for (var j = vendors.Count - 1; j >= 0; --j)
				{
					vendors[j].Delete();
				}
			}

			var guardLists = GuardLists;

			for (var i = 0; i < guardLists.Count; ++i)
			{
				var guardList = guardLists[i];
				var guards = guardList.Guards;

				foreach (var g in guards.ToArray())
				{
					g.Delete();

					guards.Remove(g);
				}
			}

			ConstructGuardLists();
		}

		public int CompareTo(object obj)
		{
			if (obj is Town town)
			{
				return CompareTo(town);
			}

			return -1;
		}

		public int CompareTo(Town town)
		{
			return m_Definition.Sort - town.m_Definition.Sort;
		}

		public override string ToString()
		{
			return m_Definition.FriendlyName;
		}

		public static void WriteReference(GenericWriter writer, Town town)
		{
			var idx = Towns.IndexOf(town);

			writer.WriteEncodedInt(idx + 1);
		}

		public static Town ReadReference(GenericReader reader)
		{
			var idx = reader.ReadEncodedInt() - 1;

			if (idx >= 0 && idx < Towns.Count)
			{
				return Towns[idx];
			}

			return null;
		}

		public static Town Parse(string input)
		{
			if (input == null)
			{
				throw new ArgumentNullException(nameof(input));
			}

			return Towns.Find(t => Insensitive.Equals(t.Definition.FriendlyName, input));
		}

		public static bool TryParse(string input, out Town town)
		{
			try
			{
				town = Parse(input);

				return true;
			}
			catch
			{
				town = null;

				return false;
			}
		}

		public static void GrantTownSilver_OnCommand(CommandEventArgs e)
		{
			var town = FromRegion(e.Mobile.Region);

			if (town == null)
			{
				e.Mobile.SendMessage("You are not in a faction town.");
			}
			else if (e.Length == 0)
			{
				e.Mobile.SendMessage("Format: GrantTownSilver <amount>");
			}
			else
			{
				town.Silver += e.GetInt32(0);
				e.Mobile.SendMessage("You have granted {0:N0} silver to the town. It now has {1:N0} silver.", e.GetInt32(0), town.Silver);
			}
		}
	}

	public class TownState
	{
		private Town m_Town;
		private Faction m_Owner;

		private Mobile m_Sheriff;
		private Mobile m_Finance;

		private int m_Silver;
		private int m_Tax;

		private DateTime m_LastTaxChange;
		private DateTime m_LastIncome;

		public Town Town
		{
			get => m_Town;
			set => m_Town = value;
		}

		public Faction Owner
		{
			get => m_Owner;
			set => m_Owner = value;
		}

		public Mobile Sheriff
		{
			get => m_Sheriff;
			set
			{
				if (m_Sheriff != null)
				{
					var pl = PlayerState.Find(m_Sheriff);

					if (pl != null)
					{
						pl.Sheriff = null;
					}
				}

				m_Sheriff = value;

				if (m_Sheriff != null)
				{
					var pl = PlayerState.Find(m_Sheriff);

					if (pl != null)
					{
						pl.Sheriff = m_Town;
					}
				}
			}
		}

		public Mobile Finance
		{
			get => m_Finance;
			set
			{
				if (m_Finance != null)
				{
					var pl = PlayerState.Find(m_Finance);

					if (pl != null)
					{
						pl.Finance = null;
					}
				}

				m_Finance = value;

				if (m_Finance != null)
				{
					var pl = PlayerState.Find(m_Finance);

					if (pl != null)
					{
						pl.Finance = m_Town;
					}
				}
			}
		}

		public int Silver
		{
			get => m_Silver;
			set => m_Silver = value;
		}

		public int Tax
		{
			get => m_Tax;
			set => m_Tax = value;
		}

		public DateTime LastTaxChange
		{
			get => m_LastTaxChange;
			set => m_LastTaxChange = value;
		}

		public DateTime LastIncome
		{
			get => m_LastIncome;
			set => m_LastIncome = value;
		}

		public TownState(Town town)
		{
			m_Town = town;
		}

		public TownState(GenericReader reader)
		{
			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 3:
					{
						m_LastIncome = reader.ReadDateTime();

						goto case 2;
					}
				case 2:
					{
						m_Tax = reader.ReadEncodedInt();
						m_LastTaxChange = reader.ReadDateTime();

						goto case 1;
					}
				case 1:
					{
						m_Silver = reader.ReadEncodedInt();

						goto case 0;
					}
				case 0:
					{
						m_Town = Town.ReadReference(reader);
						m_Owner = Faction.ReadReference(reader);

						m_Sheriff = reader.ReadMobile();
						m_Finance = reader.ReadMobile();

						m_Town.State = this;

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(3); // version

			writer.Write(m_LastIncome);

			writer.WriteEncodedInt(m_Tax);
			writer.Write(m_LastTaxChange);

			writer.WriteEncodedInt(m_Silver);

			Town.WriteReference(writer, m_Town);
			Faction.WriteReference(writer, m_Owner);

			writer.Write(m_Sheriff);
			writer.Write(m_Finance);
		}
	}

	public class Reflector
	{
		private static List<Town> m_Towns;

		public static List<Town> Towns
		{
			get
			{
				if (m_Towns == null)
				{
					ProcessTypes();
				}

				return m_Towns;
			}
		}

		private static List<Faction> m_Factions;

		public static List<Faction> Factions
		{
			get
			{
				if (m_Factions == null)
				{
					ProcessTypes();
				}

				return m_Factions;
			}
		}

		private static object Construct(Type type)
		{
			try { return Activator.CreateInstance(type); }
			catch { return null; }
		}

		private static void ProcessTypes()
		{
			m_Factions = new List<Faction>();
			m_Towns = new List<Town>();

			var asms = ScriptCompiler.Assemblies;

			for (var i = 0; i < asms.Length; ++i)
			{
				var asm = asms[i];
				var tc = ScriptCompiler.GetTypeCache(asm);
				var types = tc.Types;

				for (var j = 0; j < types.Length; ++j)
				{
					var type = types[j];

					if (type.IsSubclassOf(typeof(Faction)))
					{
						if (Construct(type) is Faction faction)
						{
							Faction.Factions.Add(faction);
						}
					}
					else if (type.IsSubclassOf(typeof(Town)))
					{
						if (Construct(type) is Town town)
						{
							Town.Towns.Add(town);
						}
					}
				}
			}
		}
	}


	/// Faction Stronghold
	public class StrongholdDefinition
	{
		private readonly Poly2D[] m_Area;
		private Point3D m_JoinStone;
		private Point3D m_FactionStone;
		private readonly Point3D[] m_Monoliths;

		public Poly2D[] Area => m_Area;

		public Point3D JoinStone => m_JoinStone;
		public Point3D FactionStone => m_FactionStone;

		public Point3D[] Monoliths => m_Monoliths;

		public StrongholdDefinition(Poly2D[] area, Point3D joinStone, Point3D factionStone, Point3D[] monoliths)
		{
			m_Area = area;
			m_JoinStone = joinStone;
			m_FactionStone = factionStone;
			m_Monoliths = monoliths;
		}
	}

	public class StrongholdRegion : BaseRegion
	{
		public Faction Faction { get; set; }

		public StrongholdRegion(Faction faction) : base(faction.Definition.FriendlyName, Faction.Facet, DefaultPriority, faction.Definition.Stronghold.Area)
		{
			Faction = faction;

			Register();
		}

		public StrongholdRegion(int id) : base(id)
		{
		}

		public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
		{
			if (!base.OnMoveInto(m, d, newLocation, oldLocation))
			{
				return false;
			}

			if (m.AccessLevel >= AccessLevel.Counselor || Contains(oldLocation))
			{
				return true;
			}

			if (m is PlayerMobile pm && pm.DuelContext != null)
			{
				m.SendMessage("You may not enter this area while participating in a duel or a tournament.");
				return false;
			}

			return Faction.Find(m, true, true) != null;
		}

		public override bool AllowHousing(Mobile from, Point3D p)
		{
			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			Faction.WriteReference(writer, Faction);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();

			Faction = Faction.ReadReference(reader);
		}
	}


	/// Faction Mobile
	public class GuardDefinition
	{
		private readonly Type m_Type;

		private readonly int m_Price;
		private readonly int m_Upkeep;
		private readonly int m_Maximum;

		private readonly int m_ItemID;

		private readonly TextDefinition m_Header;
		private readonly TextDefinition m_Label;

		public Type Type => m_Type;

		public int Price => m_Price;
		public int Upkeep => m_Upkeep;
		public int Maximum => m_Maximum;
		public int ItemID => m_ItemID;

		public TextDefinition Header => m_Header;
		public TextDefinition Label => m_Label;

		public GuardDefinition(Type type, int itemID, int price, int upkeep, int maximum, TextDefinition header, TextDefinition label)
		{
			m_Type = type;

			m_Price = price;
			m_Upkeep = upkeep;
			m_Maximum = maximum;
			m_ItemID = itemID;

			m_Header = header;
			m_Label = label;
		}
	}

	public class GuardList
	{
		private readonly GuardDefinition m_Definition;
		private readonly HashSet<BaseFactionGuard> m_Guards;

		public GuardDefinition Definition => m_Definition;
		public HashSet<BaseFactionGuard> Guards => m_Guards;

		public BaseFactionGuard Construct()
		{
			try { return Activator.CreateInstance(m_Definition.Type) as BaseFactionGuard; }
			catch { return null; }
		}

		public GuardList(GuardDefinition definition)
		{
			m_Definition = definition;
			m_Guards = new HashSet<BaseFactionGuard>();
		}
	}

	public class VendorDefinition
	{
		private readonly Type m_Type;

		private readonly int m_Price;
		private readonly int m_Upkeep;
		private readonly int m_Maximum;

		private readonly int m_ItemID;

		private readonly TextDefinition m_Header;
		private readonly TextDefinition m_Label;

		public Type Type => m_Type;

		public int Price => m_Price;
		public int Upkeep => m_Upkeep;
		public int Maximum => m_Maximum;
		public int ItemID => m_ItemID;

		public TextDefinition Header => m_Header;
		public TextDefinition Label => m_Label;

		public VendorDefinition(Type type, int itemID, int price, int upkeep, int maximum, TextDefinition header, TextDefinition label)
		{
			m_Type = type;

			m_Price = price;
			m_Upkeep = upkeep;
			m_Maximum = maximum;
			m_ItemID = itemID;

			m_Header = header;
			m_Label = label;
		}

		private static readonly VendorDefinition[] m_Definitions = new VendorDefinition[]
		{
			new VendorDefinition( typeof( FactionBottleVendor ), 0xF0E,
				5000,
				1000,
				10,
				new TextDefinition( 1011549, "POTION BOTTLE VENDOR" ),
				new TextDefinition( 1011544, "Buy Potion Bottle Vendor" )
			),
			new VendorDefinition( typeof( FactionBoardVendor ), 0x1BD7,
				3000,
				500,
				10,
				new TextDefinition( 1011552, "WOOD VENDOR" ),
				new TextDefinition( 1011545, "Buy Wooden Board Vendor" )
			),
			new VendorDefinition( typeof( FactionOreVendor ), 0x19B8,
				3000,
				500,
				10,
				new TextDefinition( 1011553, "IRON ORE VENDOR" ),
				new TextDefinition( 1011546, "Buy Iron Ore Vendor" )
			),
			new VendorDefinition( typeof( FactionReagentVendor ), 0xF86,
				5000,
				1000,
				10,
				new TextDefinition( 1011554, "REAGENT VENDOR" ),
				new TextDefinition( 1011547, "Buy Reagent Vendor" )
			),
			new VendorDefinition( typeof( FactionHorseVendor ), 0x20DD,
				5000,
				1000,
				1,
				new TextDefinition( 1011556, "HORSE BREEDER" ),
				new TextDefinition( 1011555, "Buy Horse Breeder" )
			)
		};

		public static VendorDefinition[] Definitions => m_Definitions;
	}

	public class VendorList
	{
		private readonly VendorDefinition m_Definition;
		private readonly List<BaseFactionVendor> m_Vendors;

		public VendorDefinition Definition => m_Definition;
		public List<BaseFactionVendor> Vendors => m_Vendors;

		public BaseFactionVendor Construct(Town town, Faction faction)
		{
			try { return Activator.CreateInstance(m_Definition.Type, new object[] { town, faction }) as BaseFactionVendor; }
			catch { return null; }
		}

		public VendorList(VendorDefinition definition)
		{
			m_Definition = definition;
			m_Vendors = new List<BaseFactionVendor>();
		}
	}


	/// Faction Object
	public interface IFactionItem
	{
		FactionItem FactionItemState { get; set; }
	}

	public class FactionItemDefinition
	{
		private readonly int m_SilverCost;
		private readonly Type m_VendorType;

		public int SilverCost => m_SilverCost;
		public Type VendorType => m_VendorType;

		public FactionItemDefinition(int silverCost, Type vendorType)
		{
			m_SilverCost = silverCost;
			m_VendorType = vendorType;
		}

		private static readonly FactionItemDefinition m_MetalArmor = new(1000, typeof(Blacksmith));
		private static readonly FactionItemDefinition m_Weapon = new(1000, typeof(Blacksmith));
		private static readonly FactionItemDefinition m_RangedWeapon = new(1000, typeof(Bowyer));
		private static readonly FactionItemDefinition m_LeatherArmor = new(750, typeof(Tailor));
		private static readonly FactionItemDefinition m_Clothing = new(200, typeof(Tailor));
		private static readonly FactionItemDefinition m_Scroll = new(500, typeof(Mage));

		public static FactionItemDefinition Identify(Item item)
		{
			if (item is BaseArmor ba)
			{
				if (CraftResources.GetType(ba.Resource) == CraftResourceType.Leather)
				{
					return m_LeatherArmor;
				}

				return m_MetalArmor;
			}

			if (item is BaseRanged)
			{
				return m_RangedWeapon;
			}
			
			if (item is BaseWeapon)
			{
				return m_Weapon;
			}
			
			if (item is BaseClothing)
			{
				return m_Clothing;
			}
			
			if (item is SpellScroll)
			{
				return m_Scroll;
			}

			return null;
		}
	}

	public class FactionItem
	{
		public static readonly TimeSpan ExpirationPeriod = TimeSpan.FromDays(21.0);

		public Faction Faction { get; }

		public Item Item { get; private set; }
		public DateTime Expiration { get; private set; }

		public bool HasExpired
		{
			get
			{
				if (Item == null || Item.Deleted)
				{
					return true;
				}

				return Expiration != DateTime.MinValue && DateTime.UtcNow >= Expiration;
			}
		}

		public void StartExpiration()
		{
			Expiration = DateTime.UtcNow + ExpirationPeriod;
		}

		public void CheckAttach()
		{
			if (!HasExpired)
			{
				Attach();
			}
			else
			{
				Detach();
			}
		}

		public void Attach()
		{
			if (Item is IFactionItem fi)
			{
				fi.FactionItemState = this;
			}

			if (Faction != null && !Faction.State.Items.Contains(this))
			{
				Faction.State.Items.Add(this);
			}
		}

		public void Detach()
		{
			if (Item is IFactionItem fi)
			{
				fi.FactionItemState = null;
			}

			Faction?.State.Items.Remove(this);
		}

		public FactionItem(Item item, Faction faction)
		{
			Item = item;
			Faction = faction;
		}

		public FactionItem(GenericReader reader, Faction faction)
		{
			Faction = faction;

			Deserialize(reader);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.Write(Item);
			writer.Write(Expiration);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.ReadEncodedInt();

			Item = reader.ReadItem();
			Expiration = reader.ReadDateTime();
		}

		public static int GetMaxWearables(Mobile mob)
		{
			var pl = PlayerState.Find(mob);

			if (pl == null)
			{
				return 0;
			}

			if (pl.Faction.IsCommander(mob))
			{
				return 9;
			}

			return pl.Rank.MaxWearables;
		}

		public static FactionItem Find(Item item)
		{
			if (item is IFactionItem fi)
			{
				var state = fi.FactionItemState;

				if (state != null && state.HasExpired)
				{
					state.Detach();
					state = null;
				}

				return state;
			}

			return null;
		}

		public static Item Imbue(Item item, Faction faction, bool expire, int hue)
		{
			if (item is not IFactionItem)
			{
				return item;
			}

			var state = Find(item);

			if (state == null)
			{
				state = new FactionItem(item, faction);
				state.Attach();
			}

			if (expire)
			{
				state.StartExpiration();
			}

			item.Hue = hue;

			return item;
		}
	}


	/// Faction Currency
	public class SilverGivenEntry
	{
		public static readonly TimeSpan ExpirePeriod = TimeSpan.FromHours(3.0);

		public Mobile GivenTo { get; }
		public DateTime TimeOfGift { get; }

		public bool IsExpired => TimeOfGift + ExpirePeriod < DateTime.UtcNow;

		public SilverGivenEntry(Mobile givenTo)
		{
			GivenTo = givenTo;
			TimeOfGift = DateTime.UtcNow;
		}
	}


	/// Faction Title
	public enum MerchantTitle
	{
		None,
		Scribe,
		Carpenter,
		Blacksmith,
		Bowyer,
		Tailor
	}

	public class MerchantTitleInfo
	{
		private readonly SkillName m_Skill;
		private readonly double m_Requirement;
		private readonly TextDefinition m_Title;
		private readonly TextDefinition m_Label;
		private readonly TextDefinition m_Assigned;

		public SkillName Skill => m_Skill;
		public double Requirement => m_Requirement;
		public TextDefinition Title => m_Title;
		public TextDefinition Label => m_Label;
		public TextDefinition Assigned => m_Assigned;

		public MerchantTitleInfo(SkillName skill, double requirement, TextDefinition title, TextDefinition label, TextDefinition assigned)
		{
			m_Skill = skill;
			m_Requirement = requirement;
			m_Title = title;
			m_Label = label;
			m_Assigned = assigned;
		}
	}

	public class MerchantTitles
	{
		private static readonly MerchantTitleInfo[] m_Info = new MerchantTitleInfo[]
		{
			new MerchantTitleInfo( SkillName.Inscribe,      90.0,   new TextDefinition( 1060773, "Scribe" ),        new TextDefinition( 1011468, "SCRIBE" ),        new TextDefinition( 1010121, "You now have the faction title of scribe" ) ),
			new MerchantTitleInfo( SkillName.Carpentry,     90.0,   new TextDefinition( 1060774, "Carpenter" ),     new TextDefinition( 1011469, "CARPENTER" ),     new TextDefinition( 1010122, "You now have the faction title of carpenter" ) ),
			new MerchantTitleInfo( SkillName.Tinkering,     90.0,   new TextDefinition( 1022984, "Tinker" ),        new TextDefinition( 1011470, "TINKER" ),        new TextDefinition( 1010123, "You now have the faction title of tinker" ) ),
			new MerchantTitleInfo( SkillName.Blacksmith,    90.0,   new TextDefinition( 1023016, "Blacksmith" ),    new TextDefinition( 1011471, "BLACKSMITH" ),    new TextDefinition( 1010124, "You now have the faction title of blacksmith" ) ),
			new MerchantTitleInfo( SkillName.Fletching,     90.0,   new TextDefinition( 1023022, "Bowyer" ),        new TextDefinition( 1011472, "BOWYER" ),        new TextDefinition( 1010125, "You now have the faction title of Bowyer" ) ),
			new MerchantTitleInfo( SkillName.Tailoring,     90.0,   new TextDefinition( 1022982, "Tailor" ),        new TextDefinition( 1018300, "TAILOR" ),        new TextDefinition( 1042162, "You now have the faction title of Tailor" ) ),
		};

		public static MerchantTitleInfo[] Info => m_Info;

		public static MerchantTitleInfo GetInfo(MerchantTitle title)
		{
			var idx = (int)title - 1;

			if (idx >= 0 && idx < m_Info.Length)
			{
				return m_Info[idx];
			}

			return null;
		}

		public static bool HasMerchantQualifications(Mobile mob)
		{
			for (var i = 0; i < m_Info.Length; ++i)
			{
				if (IsQualified(mob, m_Info[i]))
				{
					return true;
				}
			}

			return false;
		}

		public static bool IsQualified(Mobile mob, MerchantTitle title)
		{
			return IsQualified(mob, GetInfo(title));
		}

		public static bool IsQualified(Mobile mob, MerchantTitleInfo info)
		{
			if (mob == null || info == null)
			{
				return false;
			}

			return mob.Skills[info.Skill].Value >= info.Requirement;
		}
	}


	/// Faction Speech
	public enum FactionKickType
	{
		Kick,
		Ban,
		Unban
	}

	public class FactionKickCommand : BaseCommand
	{
		private readonly FactionKickType m_KickType;

		public FactionKickCommand(FactionKickType kickType)
		{
			m_KickType = kickType;

			AccessLevel = AccessLevel.GameMaster;
			Supports = CommandSupport.AllMobiles;
			ObjectTypes = ObjectTypes.Mobiles;

			switch (m_KickType)
			{
				case FactionKickType.Kick:
					{
						Commands = new string[] { "FactionKick" };
						Usage = "FactionKick";
						Description = "Kicks the targeted player out of his current faction. This does not prevent them from rejoining.";
						break;
					}
				case FactionKickType.Ban:
					{
						Commands = new string[] { "FactionBan" };
						Usage = "FactionBan";
						Description = "Bans the account of a targeted player from joining factions. All players on the account are removed from their current faction, if any.";
						break;
					}
				case FactionKickType.Unban:
					{
						Commands = new string[] { "FactionUnban" };
						Usage = "FactionUnban";
						Description = "Unbans the account of a targeted player from joining factions.";
						break;
					}
			}
		}

		public override void Execute(CommandEventArgs e, object obj)
		{
			var mob = (Mobile)obj;

			switch (m_KickType)
			{
				case FactionKickType.Kick:
					{
						var pl = PlayerState.Find(mob);

						if (pl != null)
						{
							pl.Faction.RemoveMember(mob);
							mob.SendMessage("You have been kicked from your faction.");
							AddResponse("They have been kicked from their faction.");
						}
						else
						{
							LogFailure("They are not in a faction.");
						}

						break;
					}
				case FactionKickType.Ban:
					{
						if (mob.Account is Account acct)
						{
							if (acct.GetTag("FactionBanned") == null)
							{
								acct.SetTag("FactionBanned", "true");
								AddResponse("The account has been banned from joining factions.");
							}
							else
							{
								AddResponse("The account is already banned from joining factions.");
							}

							for (var i = 0; i < acct.Length; ++i)
							{
								mob = acct[i];

								if (mob != null)
								{
									var pl = PlayerState.Find(mob);

									if (pl != null)
									{
										pl.Faction.RemoveMember(mob);

										mob.SendMessage("You have been kicked from your faction.");
										AddResponse("They have been kicked from their faction.");
									}
								}
							}
						}
						else
						{
							LogFailure("They have no assigned account.");
						}

						break;
					}
				case FactionKickType.Unban:
					{
						if (mob.Account is Account acct)
						{
							if (acct.GetTag("FactionBanned") == null)
							{
								AddResponse("The account is not already banned from joining factions.");
							}
							else
							{
								acct.RemoveTag("FactionBanned");
								AddResponse("The account may now freely join factions.");
							}
						}
						else
						{
							LogFailure("They have no assigned account.");
						}

						break;
					}
			}
		}
	}

	public class Keywords
	{
		public static void Initialize()
		{
			EventSink.Speech += EventSink_Speech;
		}

		private static void ShowScore_Sandbox(PlayerState state)
		{
			state?.Mobile.PublicOverheadMessage(MessageType.Regular, state.Mobile.SpeechHue, true, state.KillPoints.ToString("N0"));
		}

		private static void EventSink_Speech(SpeechEventArgs e)
		{
			var from = e.Mobile;
			var keywords = e.Keywords;

			for (var i = 0; i < keywords.Length; ++i)
			{
				switch (keywords[i])
				{
					case 0x00E4: // *i wish to access the city treasury*
						{
							var town = Town.FromRegion(from.Region);

							if (town == null || !town.IsFinance(from) || !from.Alive)
							{
								break;
							}

							if (FactionGump.Exists(from))
							{
								from.SendLocalizedMessage(1042160); // You already have a faction menu open.
							}
							else if (town.Owner != null && from is PlayerMobile pm)
							{
								from.SendGump(new FinanceGump(pm, town.Owner, town));
							}

							break;
						}
					case 0x0ED: // *i am sheriff*
						{
							var town = Town.FromRegion(from.Region);

							if (town == null || !town.IsSheriff(from) || !from.Alive)
							{
								break;
							}

							if (FactionGump.Exists(from))
							{
								from.SendLocalizedMessage(1042160); // You already have a faction menu open.
							}
							else if (town.Owner != null && from is PlayerMobile pm)
							{
								from.SendGump(new SheriffGump(pm, town.Owner, town));
							}

							break;
						}
					case 0x00EF: // *you are fired*
						{
							var town = Town.FromRegion(from.Region);

							if (town == null)
							{
								break;
							}

							if (town.IsFinance(from) || town.IsSheriff(from))
							{
								town.BeginOrderFiring(from);
							}

							break;
						}
					case 0x00E5: // *i wish to resign as finance minister*
						{
							var pl = PlayerState.Find(from);

							if (pl != null && pl.Finance != null)
							{
								pl.Finance.Finance = null;
								from.SendLocalizedMessage(1005081); // You have been fired as Finance Minister
							}

							break;
						}
					case 0x00EE: // *i wish to resign as sheriff*
						{
							var pl = PlayerState.Find(from);

							if (pl != null && pl.Sheriff != null)
							{
								pl.Sheriff.Sheriff = null;
								from.SendLocalizedMessage(1010270); // You have been fired as Sheriff
							}

							break;
						}
					case 0x00E9: // *what is my faction term status*
						{
							var pl = PlayerState.Find(from);

							if (pl != null && pl.IsLeaving)
							{
								if (Faction.CheckLeaveTimer(from))
								{
									break;
								}

								var remaining = pl.Leaving + Faction.LeavePeriod - DateTime.UtcNow;

								if (remaining.TotalDays >= 1)
								{
									from.SendLocalizedMessage(1042743, remaining.TotalDays.ToString("N0"));// Your term of service will come to an end in ~1_DAYS~ days.
								}
								else if (remaining.TotalHours >= 1)
								{
									from.SendLocalizedMessage(1042741, remaining.TotalHours.ToString("N0")); // Your term of service will come to an end in ~1_HOURS~ hours.
								}
								else
								{
									from.SendLocalizedMessage(1042742); // Your term of service will come to an end in less than one hour.
								}
							}
							else if (pl != null)
							{
								from.SendLocalizedMessage(1042233); // You are not in the process of quitting the faction.
							}

							break;
						}
					case 0x00EA: // *message faction*
						{
							var faction = Faction.Find(from);

							if (faction == null || !faction.IsCommander(from))
							{
								break;
							}

							if (from.AccessLevel == AccessLevel.Player && !faction.FactionMessageReady)
							{
								from.SendLocalizedMessage(1010264); // The required time has not yet passed since the last message was sent
							}
							else
							{
								faction.BeginBroadcast(from);
							}

							break;
						}
					case 0x00EC: // *showscore*
						{
							var pl = PlayerState.Find(from);

							if (pl != null)
							{
								Timer.DelayCall(ShowScore_Sandbox, pl);
							}

							break;
						}
					case 0x0178: // i honor your leadership
						{
							var faction = Faction.Find(from);

							if (faction != null)
							{
								Faction.BeginHonorLeadership(from);
							}

							break;
						}
				}
			}
		}
	}


	/// Faction Gumps
	public class FinanceGump : FactionGump
	{
		private readonly PlayerMobile m_From;
		private readonly Faction m_Faction;
		private readonly Town m_Town;

		private static readonly int[] m_PriceOffsets = new int[]
			{
				-30, -25, -20, -15, -10, -5,
				+50, +100, +150, +200, +250, +300
			};

		public override int ButtonTypes => 2;

		public FinanceGump(PlayerMobile from, Faction faction, Town town) : base(50, 50)
		{
			m_From = from;
			m_Faction = faction;
			m_Town = town;


			AddPage(0);

			AddBackground(0, 0, 320, 410, 5054);
			AddBackground(10, 10, 300, 390, 3000);

			#region General
			AddPage(1);

			AddHtmlLocalized(20, 30, 260, 25, 1011541, false, false); // FINANCE MINISTER


			AddHtmlLocalized(55, 90, 200, 25, 1011539, false, false); // CHANGE PRICES
			AddButton(20, 90, 4005, 4007, 0, GumpButtonType.Page, 2);

			AddHtmlLocalized(55, 120, 200, 25, 1011540, false, false); // BUY SHOPKEEPERS	
			AddButton(20, 120, 4005, 4007, 0, GumpButtonType.Page, 3);

			AddHtmlLocalized(55, 150, 200, 25, 1011495, false, false); // VIEW FINANCES
			AddButton(20, 150, 4005, 4007, 0, GumpButtonType.Page, 4);

			AddHtmlLocalized(55, 360, 200, 25, 1011441, false, false); // EXIT
			AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Reply, 0);
			#endregion

			#region Change Prices
			AddPage(2);

			AddHtmlLocalized(20, 30, 200, 25, 1011539, false, false); // CHANGE PRICES

			for (var i = 0; i < m_PriceOffsets.Length; ++i)
			{
				var ofs = m_PriceOffsets[i];

				var x = 20 + ((i / 6) * 150);
				var y = 90 + ((i % 6) * 30);

				AddRadio(x, y, 208, 209, (town.Tax == ofs), i + 1);

				if (ofs < 0)
				{
					AddLabel(x + 35, y, 0x26, String.Concat("- ", -ofs, "%"));
				}
				else
				{
					AddLabel(x + 35, y, 0x12A, String.Concat("+ ", ofs, "%"));
				}
			}

			AddRadio(20, 270, 208, 209, (town.Tax == 0), 0);
			AddHtmlLocalized(55, 270, 90, 25, 1011542, false, false); // normal

			AddHtmlLocalized(55, 330, 200, 25, 1011509, false, false); // Set Prices
			AddButton(20, 330, 4005, 4007, ToButtonID(0, 0), GumpButtonType.Reply, 0);

			AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
			AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
			#endregion

			#region Buy Shopkeepers
			AddPage(3);

			AddHtmlLocalized(20, 30, 200, 25, 1011540, false, false); // BUY SHOPKEEPERS

			var vendorLists = town.VendorLists;

			for (var i = 0; i < vendorLists.Count; ++i)
			{
				var list = vendorLists[i];

				AddButton(20, 90 + (i * 40), 4005, 4007, 0, GumpButtonType.Page, 5 + i);
				AddItem(55, 90 + (i * 40), list.Definition.ItemID);
				AddHtmlText(100, 90 + (i * 40), 200, 25, list.Definition.Label, false, false);
			}

			AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false);  //	Previous page
			AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
			#endregion

			#region View Finances
			AddPage(4);

			var financeUpkeep = town.FinanceUpkeep;
			var sheriffUpkeep = town.SheriffUpkeep;
			var dailyIncome = town.DailyIncome;
			var netCashFlow = town.NetCashFlow;


			AddHtmlLocalized(20, 30, 300, 25, 1011524, false, false); // FINANCE STATEMENT

			AddHtmlLocalized(20, 80, 300, 25, 1011538, false, false); // Current total money for town : 
			AddLabel(20, 100, 0x44, town.Silver.ToString());

			AddHtmlLocalized(20, 130, 300, 25, 1011520, false, false); // Finance Minister Upkeep : 
			AddLabel(20, 150, 0x44, financeUpkeep.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(20, 180, 300, 25, 1011521, false, false); // Sheriff Upkeep : 
			AddLabel(20, 200, 0x44, sheriffUpkeep.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(20, 230, 300, 25, 1011522, false, false); // Town Income : 
			AddLabel(20, 250, 0x44, dailyIncome.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(20, 280, 300, 25, 1011523, false, false); // Net Cash flow per day : 
			AddLabel(20, 300, 0x44, netCashFlow.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
			AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
			#endregion

			#region Shopkeeper Pages
			for (var i = 0; i < vendorLists.Count; ++i)
			{
				var vendorList = vendorLists[i];

				AddPage(5 + i);

				AddHtmlText(60, 30, 300, 25, vendorList.Definition.Header, false, false);
				AddItem(20, 30, vendorList.Definition.ItemID);

				AddHtmlLocalized(20, 90, 200, 25, 1011514, false, false); // You have : 
				AddLabel(230, 90, 0x26, vendorList.Vendors.Count.ToString());

				AddHtmlLocalized(20, 120, 200, 25, 1011515, false, false); // Maximum : 
				AddLabel(230, 120, 0x256, vendorList.Definition.Maximum.ToString());

				AddHtmlLocalized(20, 150, 200, 25, 1011516, false, false); // Cost :
				AddLabel(230, 150, 0x44, vendorList.Definition.Price.ToString("N0")); // NOTE: Added 'N0'

				AddHtmlLocalized(20, 180, 200, 25, 1011517, false, false); // Daily Pay :
				AddLabel(230, 180, 0x37, vendorList.Definition.Upkeep.ToString("N0")); // NOTE: Added 'N0'

				AddHtmlLocalized(20, 210, 200, 25, 1011518, false, false); // Current Silver :
				AddLabel(230, 210, 0x44, town.Silver.ToString("N0")); // NOTE: Added 'N0'

				AddHtmlLocalized(20, 240, 200, 25, 1011519, false, false); // Current Payroll :
				AddLabel(230, 240, 0x44, financeUpkeep.ToString("N0")); // NOTE: Added 'N0'

				AddHtmlText(55, 300, 200, 25, vendorList.Definition.Label, false, false);
				if (town.Silver >= vendorList.Definition.Price)
				{
					AddButton(20, 300, 4005, 4007, ToButtonID(1, i), GumpButtonType.Reply, 0);
				}
				else
				{
					AddImage(20, 300, 4020);
				}

				AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
				AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 3);
			}
			#endregion
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (!m_Town.IsFinance(m_From) || m_Town.Owner != m_Faction)
			{
				m_From.SendLocalizedMessage(1010339); // You no longer control this city
				return;
			}

			int type, index;

			if (!FromButtonID(info.ButtonID, out type, out index))
			{
				return;
			}

			switch (type)
			{
				case 0: // general
					{
						switch (index)
						{
							case 0: // set price
								{
									var switches = info.Switches;

									if (switches.Length == 0)
									{
										break;
									}

									var opt = switches[0];
									var newTax = 0;

									if (opt >= 1 && opt <= m_PriceOffsets.Length)
									{
										newTax = m_PriceOffsets[opt - 1];
									}

									if (m_Town.Tax == newTax)
									{
										break;
									}

									if (m_From.AccessLevel == AccessLevel.Player && !m_Town.TaxChangeReady)
									{
										var remaining = DateTime.UtcNow - (m_Town.LastTaxChange + Town.TaxChangePeriod);

										if (remaining.TotalMinutes < 4)
										{
											m_From.SendLocalizedMessage(1042165); // You must wait a short while before changing prices again.
										}
										else if (remaining.TotalMinutes < 10)
										{
											m_From.SendLocalizedMessage(1042166); // You must wait several minutes before changing prices again.
										}
										else if (remaining.TotalHours < 1)
										{
											m_From.SendLocalizedMessage(1042167); // You must wait up to an hour before changing prices again.
										}
										else if (remaining.TotalHours < 4)
										{
											m_From.SendLocalizedMessage(1042168); // You must wait a few hours before changing prices again.
										}
										else
										{
											m_From.SendLocalizedMessage(1042169); // You must wait several hours before changing prices again.
										}
									}
									else
									{
										m_Town.Tax = newTax;

										if (m_From.AccessLevel == AccessLevel.Player)
										{
											m_Town.LastTaxChange = DateTime.UtcNow;
										}
									}

									break;
								}
						}

						break;
					}
				case 1: // make vendor
					{
						var vendorLists = m_Town.VendorLists;

						if (index >= 0 && index < vendorLists.Count)
						{
							var vendorList = vendorLists[index];

							if (Town.FromRegion(m_From.Region) != m_Town)
							{
								m_From.SendLocalizedMessage(1010305); // You must be in your controlled city to buy Items
							}
							else if (vendorList.Vendors.Count >= vendorList.Definition.Maximum)
							{
								m_From.SendLocalizedMessage(1010306); // You currently have too many of this enhancement type to place another
							}
							else if (BaseBoat.FindBoatAt(m_From.Location, m_From.Map) != null)
							{
								m_From.SendMessage("You cannot place a vendor here");
							}
							else if (m_Town.Silver >= vendorList.Definition.Price)
							{
								var vendor = vendorList.Construct(m_Town, m_Faction);

								if (vendor != null)
								{
									m_Town.Silver -= vendorList.Definition.Price;

									vendor.MoveToWorld(m_From.Location, m_From.Map);
									vendor.Home = vendor.Location;
								}
							}
						}

						break;
					}
			}
		}
	}

	#region Faction Crafting

	public class FactionImbueGump : FactionGump
	{
		private readonly Item m_Item;
		private readonly Mobile m_Mobile;
		private readonly Faction m_Faction;
		private readonly CraftSystem m_CraftSystem;
		private readonly BaseTool m_Tool;
		private readonly object m_Notice;
		private readonly int m_Quality;

		private readonly FactionItemDefinition m_Definition;

		public FactionImbueGump(int quality, Item item, Mobile from, CraftSystem craftSystem, BaseTool tool, object notice, int availableSilver, Faction faction, FactionItemDefinition def) : base(100, 200)
		{
			m_Item = item;
			m_Mobile = from;
			m_Faction = faction;
			m_CraftSystem = craftSystem;
			m_Tool = tool;
			m_Notice = notice;
			m_Quality = quality;
			m_Definition = def;

			AddPage(0);

			AddBackground(0, 0, 320, 270, 5054);
			AddBackground(10, 10, 300, 250, 3000);

			AddHtmlLocalized(20, 20, 210, 25, 1011569, false, false); // Imbue with Faction properties?


			AddHtmlLocalized(20, 60, 170, 25, 1018302, false, false); // Item quality: 
			AddHtmlLocalized(175, 60, 100, 25, 1018305 - m_Quality, false, false); //	Exceptional, Average, Low

			AddHtmlLocalized(20, 80, 170, 25, 1011572, false, false); // Item Cost : 
			AddLabel(175, 80, 0x34, def.SilverCost.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(20, 100, 170, 25, 1011573, false, false); // Your Silver : 
			AddLabel(175, 100, 0x34, availableSilver.ToString("N0")); // NOTE: Added 'N0'


			AddRadio(20, 140, 210, 211, true, 1);
			AddLabel(55, 140, m_Faction.Definition.HuePrimary - 1, "*****");
			AddHtmlLocalized(150, 140, 150, 25, 1011570, false, false); // Primary Color

			AddRadio(20, 160, 210, 211, false, 2);
			AddLabel(55, 160, m_Faction.Definition.HueSecondary - 1, "*****");
			AddHtmlLocalized(150, 160, 150, 25, 1011571, false, false); // Secondary Color


			AddHtmlLocalized(55, 200, 200, 25, 1011011, false, false); // CONTINUE
			AddButton(20, 200, 4005, 4007, 1, GumpButtonType.Reply, 0);

			AddHtmlLocalized(55, 230, 200, 25, 1011012, false, false); // CANCEL
			AddButton(20, 230, 4005, 4007, 0, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1)
			{
				var pack = m_Mobile.Backpack;

				if (pack != null && m_Item.IsChildOf(pack))
				{
					if (pack.ConsumeTotal(typeof(Silver), m_Definition.SilverCost))
					{
						int hue;

						if (m_Item is SpellScroll)
						{
							hue = 0;
						}
						else if (info.IsSwitched(1))
						{
							hue = m_Faction.Definition.HuePrimary;
						}
						else
						{
							hue = m_Faction.Definition.HueSecondary;
						}

						FactionItem.Imbue(m_Item, m_Faction, true, hue);
					}
					else
					{
						m_Mobile.SendLocalizedMessage(1042204); // You do not have enough silver.
					}
				}
			}

			if (m_Tool != null && !m_Tool.Deleted && m_Tool.UsesRemaining > 0)
			{
				m_Mobile.SendGump(new CraftGump(m_Mobile, m_CraftSystem, m_Tool, m_Notice));
			}
			else if (m_Notice is string s)
			{
				m_Mobile.SendMessage(s);
			}
			else if (m_Notice is int n && n > 0)
			{
				m_Mobile.SendLocalizedMessage(n);
			}
		}
	}

	#endregion

	public class SheriffGump : FactionGump
	{
		private readonly PlayerMobile m_From;
		private readonly Faction m_Faction;
		private readonly Town m_Town;

		private void CenterItem(int itemID, int x, int y, int w, int h)
		{
			var rc = ItemBounds.Table[itemID];
			AddItem(x + ((w - rc.Width) / 2) - rc.X, y + ((h - rc.Height) / 2) - rc.Y, itemID);
		}

		public SheriffGump(PlayerMobile from, Faction faction, Town town) : base(50, 50)
		{
			m_From = from;
			m_Faction = faction;
			m_Town = town;

			AddPage(0);

			AddBackground(0, 0, 320, 410, 5054);
			AddBackground(10, 10, 300, 390, 3000);

			#region General
			AddPage(1);

			AddHtmlLocalized(20, 30, 260, 25, 1011431, false, false); // Sheriff

			AddHtmlLocalized(55, 90, 200, 25, 1011494, false, false); // HIRE GUARDS
			AddButton(20, 90, 4005, 4007, 0, GumpButtonType.Page, 3);

			AddHtmlLocalized(55, 120, 200, 25, 1011495, false, false); // VIEW FINANCES
			AddButton(20, 120, 4005, 4007, 0, GumpButtonType.Page, 2);

			AddHtmlLocalized(55, 360, 200, 25, 1011441, false, false); // Exit
			AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Reply, 0);
			#endregion

			#region Finances
			AddPage(2);

			var financeUpkeep = town.FinanceUpkeep;
			var sheriffUpkeep = town.SheriffUpkeep;
			var dailyIncome = town.DailyIncome;
			var netCashFlow = town.NetCashFlow;

			AddHtmlLocalized(20, 30, 300, 25, 1011524, false, false); // FINANCE STATEMENT

			AddHtmlLocalized(20, 80, 300, 25, 1011538, false, false); // Current total money for town : 
			AddLabel(20, 100, 0x44, town.Silver.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(20, 130, 300, 25, 1011520, false, false); // Finance Minister Upkeep : 
			AddLabel(20, 150, 0x44, financeUpkeep.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(20, 180, 300, 25, 1011521, false, false); // Sheriff Upkeep : 
			AddLabel(20, 200, 0x44, sheriffUpkeep.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(20, 230, 300, 25, 1011522, false, false); // Town Income : 
			AddLabel(20, 250, 0x44, dailyIncome.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(20, 280, 300, 25, 1011523, false, false); // Net Cash flow per day : 
			AddLabel(20, 300, 0x44, netCashFlow.ToString("N0")); // NOTE: Added 'N0'

			AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
			AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
			#endregion

			#region Hire Guards
			AddPage(3);

			AddHtmlLocalized(20, 30, 300, 25, 1011494, false, false); // HIRE GUARDS

			var guardLists = town.GuardLists;

			for (var i = 0; i < guardLists.Count; ++i)
			{
				var guardList = guardLists[i];
				var y = 90 + (i * 60);

				AddButton(20, y, 4005, 4007, 0, GumpButtonType.Page, 4 + i);
				CenterItem(guardList.Definition.ItemID, 50, y - 20, 70, 60);
				AddHtmlText(120, y, 200, 25, guardList.Definition.Header, false, false);
			}

			AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
			AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 1);
			#endregion

			#region Guard Pages
			for (var i = 0; i < guardLists.Count; ++i)
			{
				var guardList = guardLists[i];

				AddPage(4 + i);

				AddHtmlText(90, 30, 300, 25, guardList.Definition.Header, false, false);
				CenterItem(guardList.Definition.ItemID, 10, 10, 80, 80);

				AddHtmlLocalized(20, 90, 200, 25, 1011514, false, false); // You have : 
				AddLabel(230, 90, 0x26, guardList.Guards.Count.ToString());

				AddHtmlLocalized(20, 120, 200, 25, 1011515, false, false); // Maximum : 
				AddLabel(230, 120, 0x12A, guardList.Definition.Maximum.ToString());

				AddHtmlLocalized(20, 150, 200, 25, 1011516, false, false); // Cost : 
				AddLabel(230, 150, 0x44, guardList.Definition.Price.ToString("N0")); // NOTE: Added 'N0'

				AddHtmlLocalized(20, 180, 200, 25, 1011517, false, false); // Daily Pay :
				AddLabel(230, 180, 0x37, guardList.Definition.Upkeep.ToString("N0")); // NOTE: Added 'N0'

				AddHtmlLocalized(20, 210, 200, 25, 1011518, false, false); // Current Silver : 
				AddLabel(230, 210, 0x44, town.Silver.ToString("N0")); // NOTE: Added 'N0'

				AddHtmlLocalized(20, 240, 200, 25, 1011519, false, false); // Current Payroll : 
				AddLabel(230, 240, 0x44, sheriffUpkeep.ToString("N0")); // NOTE: Added 'N0'

				AddHtmlText(55, 300, 200, 25, guardList.Definition.Label, false, false);
				AddButton(20, 300, 4005, 4007, 1 + i, GumpButtonType.Reply, 0);

				AddHtmlLocalized(55, 360, 200, 25, 1011067, false, false); // Previous page
				AddButton(20, 360, 4005, 4007, 0, GumpButtonType.Page, 3);
			}
			#endregion
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (!m_Town.IsSheriff(m_From) || m_Town.Owner != m_Faction)
			{
				m_From.SendLocalizedMessage(1010339); // You no longer control this city
				return;
			}

			var index = info.ButtonID - 1;

			if (index >= 0 && index < m_Town.GuardLists.Count)
			{
				var guardList = m_Town.GuardLists[index];

				if (Town.FromRegion(m_From.Region) != m_Town)
				{
					m_From.SendLocalizedMessage(1010305); // You must be in your controlled city to buy Items
				}
				else if (guardList.Guards.Count >= guardList.Definition.Maximum)
				{
					m_From.SendLocalizedMessage(1010306); // You currently have too many of this enhancement type to place another
				}
				else if (BaseBoat.FindBoatAt(m_From.Location, m_From.Map) != null)
				{
					m_From.SendMessage("You cannot place a guard here");
				}
				else if (m_Town.Silver >= guardList.Definition.Price)
				{
					var guard = guardList.Construct();

					if (guard != null)
					{
						guard.Faction = m_Faction;
						guard.Town = m_Town;

						m_Town.Silver -= guardList.Definition.Price;

						guard.MoveToWorld(m_From.Location, m_From.Map);
						guard.Home = guard.Location;
					}
				}
			}
		}
	}
}