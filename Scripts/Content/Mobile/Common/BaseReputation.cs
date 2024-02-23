using Server.Commands;
using Server.Engines.ConPVP;
using Server.Engines.PartySystem;
using Server.Factions;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.SkillHandlers;
using Server.Spells.Magery;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.Misc
{
	public class NotorietyHandlers
	{
		public static void Initialize()
		{
			Notoriety.Hues[Notoriety.Innocent] = 0x59;
			Notoriety.Hues[Notoriety.Ally] = 0x3F;
			Notoriety.Hues[Notoriety.CanBeAttacked] = 0x3B2;
			Notoriety.Hues[Notoriety.Criminal] = 0x3B2;
			Notoriety.Hues[Notoriety.Enemy] = 0x90;
			Notoriety.Hues[Notoriety.Murderer] = 0x22;
			Notoriety.Hues[Notoriety.Invulnerable] = 0x35;

			Notoriety.Handler = new NotorietyHandler(MobileNotoriety);

			Mobile.AllowBeneficialHandler = new AllowBeneficialHandler(Mobile_AllowBeneficial);
			Mobile.AllowHarmfulHandler = new AllowHarmfulHandler(Mobile_AllowHarmful);
		}

		public static bool IsPeaceful(Guild guild)
		{
			if (guild == null)
			{
				return true;
			}

			if (guild.Enemies.Count == 0 && guild.Type == GuildType.Regular)
			{
				return true;
			}

			return false;
		}

		public static Guild GetGuildFor(Guild def, Mobile m)
		{
			if (m is BaseCreature c && c.ControlMaster?.Guild is Guild masterGuild)
			{
				return masterGuild;
			}

			return m?.Guild as Guild ?? def;
		}

		public static bool Mobile_AllowBeneficial(Mobile from, Mobile target)
		{
			if (from == null || target == null)
			{
				return true;
			}

			if (from.AccessLevel > AccessLevel.Player || target.AccessLevel > AccessLevel.Player)
			{
				return true;
			}

			var map = from.Map;

			var srcPlayer = from as PlayerMobile;
			var trgPlayer = target as PlayerMobile;

			var srcCreature = from as BaseCreature;
			var trgCreature = target as BaseCreature;

			#region Dueling

			var pmFrom = srcPlayer ?? srcCreature?.GetRootMaster<PlayerMobile>();
			var pmTarg = trgPlayer ?? trgCreature?.GetRootMaster<PlayerMobile>();

			if (pmFrom != null && pmTarg != null)
			{
				if (pmFrom.DuelContext != pmTarg.DuelContext)
				{
					if (pmFrom.DuelContext != null && pmFrom.DuelContext.Started)
					{
						return false;
					}

					if (pmTarg.DuelContext != null && pmTarg.DuelContext.Started)
					{
						return false;
					}
				}

				if (pmFrom.DuelContext != null)
				{
					if (pmFrom.DuelContext == pmTarg.DuelContext)
					{
						if (pmFrom.DuelContext.StartedReadyCountdown && !pmFrom.DuelContext.Started)
						{
							return false;
						}

						if (pmFrom.DuelContext.Tied || pmFrom.DuelPlayer.Eliminated || pmTarg.DuelPlayer.Eliminated)
						{
							return false;
						}
					}

					if (pmFrom.DuelPlayer != null && !pmFrom.DuelPlayer.Eliminated && pmFrom.DuelContext.IsSuddenDeath)
					{
						return false;
					}

					if (pmFrom.DuelContext == pmTarg.DuelContext)
					{
						if (pmFrom.DuelContext.m_Tournament != null && pmFrom.DuelContext.m_Tournament.IsNotoRestricted && pmFrom.DuelPlayer != null && pmTarg.DuelPlayer != null && pmFrom.DuelPlayer.Participant != pmTarg.DuelPlayer.Participant)
						{
							return false;
						}

						if (pmFrom.DuelContext.Started)
						{
							return true;
						}
					}
				}
			}

			if (pmFrom != null && pmFrom.DuelContext != null && pmFrom.DuelContext.Started)
			{
				return false;
			}

			if (pmTarg != null && pmTarg.DuelContext != null && pmTarg.DuelContext.Started)
			{
				return false;
			}

			if (from.Region.IsPartOf<SafeZone>() || target.Region.IsPartOf<SafeZone>())
			{
				return false;
			}

			#endregion

			#region Factions

			var targetFaction = Faction.Find(target, true);

			if (targetFaction != null)
			{
				if (!Core.ML || map == Faction.Facet)
				{
					if (Faction.Find(from, true) != targetFaction)
					{
						return false;
					}
				}
			}

			#endregion

			if (map != null && !map.Rules.HasFlag(MapRules.BeneficialRestrictions))
			{
				return true; // In felucca, anything goes
			}

			if (srcPlayer == null)
			{
				return true; // NPCs have no restrictions
			}

			if (Reputation.IsAlly(from, target))
			{
				return true;
			}

			if (trgCreature != null && !trgCreature.Controlled)
			{
				return false; // Players cannot heal uncontrolled mobiles
			}

			if (srcPlayer.Young && (trgPlayer == null || !trgPlayer.Young))
			{
				return false; // Young players cannot perform beneficial actions towards older players
			}

			var fromGuild = GetGuildFor(from.Guild as Guild, from);
			var targetGuild = GetGuildFor(target.Guild as Guild, target);

			if (fromGuild != null && targetGuild != null)
			{
				if (targetGuild == fromGuild || fromGuild.IsAlly(targetGuild))
				{
					return true; // Guild members can be beneficial
				}

				return IsPeaceful(fromGuild) && IsPeaceful(targetGuild);
			}

			return true;
		}

		public static bool Mobile_AllowHarmful(Mobile from, Mobile target)
		{
			if (from == null || target == null)
			{
				return true;
			}

			if (from.AccessLevel > AccessLevel.Player || target.AccessLevel > AccessLevel.Player)
			{
				return true;
			}

			var map = from.Map;

			var srcPlayer = from as PlayerMobile;
			var trgPlayer = target as PlayerMobile;

			var srcCreature = from as BaseCreature;
			var trgCreature = target as BaseCreature;

			#region Dueling

			var pmFrom = srcPlayer ?? srcCreature?.GetRootMaster<PlayerMobile>();
			var pmTarg = trgPlayer ?? trgCreature?.GetRootMaster<PlayerMobile>();

			if (pmFrom != null && pmTarg != null)
			{
				if (pmFrom.DuelContext != pmTarg.DuelContext)
				{
					if (pmFrom.DuelContext != null && pmFrom.DuelContext.Started)
					{
						return false;
					}

					if (pmTarg.DuelContext != null && pmTarg.DuelContext.Started)
					{
						return false;
					}
				}

				if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext)
				{
					if (pmFrom.DuelContext.StartedReadyCountdown && !pmFrom.DuelContext.Started)
					{
						return false;
					}

					if (pmFrom.DuelContext.Tied || pmFrom.DuelPlayer.Eliminated || pmTarg.DuelPlayer.Eliminated)
					{
						return false;
					}

					if (pmFrom.DuelContext.m_Tournament != null && pmFrom.DuelContext.m_Tournament.IsNotoRestricted && pmFrom.DuelPlayer != null && pmTarg.DuelPlayer != null && pmFrom.DuelPlayer.Participant == pmTarg.DuelPlayer.Participant)
					{
						return false;
					}

					if (pmFrom.DuelContext.Started)
					{
						return true;
					}
				}
			}

			if (pmFrom != null && pmFrom.DuelContext != null && pmFrom.DuelContext.Started)
			{
				return false;
			}

			if (pmTarg != null && pmTarg.DuelContext != null && pmTarg.DuelContext.Started)
			{
				return false;
			}

			if (from.Region.IsPartOf<SafeZone>() || target.Region.IsPartOf<SafeZone>())
			{
				return false;
			}

			#endregion

			if (map != null && !map.Rules.HasFlag(MapRules.HarmfulRestrictions))
			{
				return true; // In felucca, anything goes
			}

			if (srcPlayer == null)
			{
				var master = srcCreature?.GetMaster();

				if (master == null || master.AccessLevel > AccessLevel.Player)
				{
					if (trgPlayer == null || !trgPlayer.CheckYoungProtection(from))
					{
						return true; // Uncontrolled NPCs are only restricted by the young system
					}

					if (CheckAggressor(from.Aggressors, target) || CheckAggressed(from.Aggressed, target))
					{
						return true;
					}

					return false;
				}
			}

			if (Reputation.IsEnemy(from, target))
			{
				return true;
			}

			var fromGuild = GetGuildFor(from.Guild as Guild, from);
			var targetGuild = GetGuildFor(target.Guild as Guild, target);

			if (fromGuild != null && targetGuild != null && (fromGuild == targetGuild || fromGuild.IsAlly(targetGuild) || fromGuild.IsEnemy(targetGuild)))
			{
				return true; // Guild allies or enemies can be harmful
			}

			if (trgCreature != null && (trgCreature.Controlled || (trgCreature.Summoned && from != trgCreature.SummonMaster)))
			{
				return false; // Cannot harm other controlled mobiles
			}

			if (trgPlayer != null)
			{
				return false; // Cannot harm other players
			}

			if (trgCreature == null || !trgCreature.InitialInnocent)
			{
				if (Notoriety.Compute(from, target) == Notoriety.Innocent)
				{
					return false; // Cannot harm innocent mobiles
				}
			}

			return true;
		}

		public static int CorpseNotoriety(Mobile source, Corpse target)
		{
			if (target.AccessLevel > AccessLevel.Player)
			{
				return Notoriety.CanBeAttacked;
			}

			var srcPlayer = source as PlayerMobile;

			var srcCreature = source as BaseCreature;

			var body = (Body)target.Amount;

			if (target.Owner is BaseCreature trgCreature)
			{
				var sourceGuild = GetGuildFor(source.Guild as Guild, source);
				var targetGuild = GetGuildFor(target.Guild, trgCreature);

				if (sourceGuild != null && targetGuild != null)
				{
					if (sourceGuild == targetGuild || sourceGuild.IsAlly(targetGuild))
					{
						return Notoriety.Ally;
					}

					if (sourceGuild.IsEnemy(targetGuild))
					{
						return Notoriety.Enemy;
					}
				}

				var srcFaction = Faction.Find(source, true, true);
				var trgFaction = Faction.Find(trgCreature, true, true);

				if (srcFaction != null && trgFaction != null && srcFaction != trgFaction && source.Map == Faction.Facet)
				{
					return Notoriety.Enemy;
				}

				var srcRepPlayer = srcPlayer ?? srcCreature?.GetRootMaster<PlayerMobile>();

				if (srcRepPlayer != null)
				{
					var repNoto = Reputation.ComputeNotoriety(srcRepPlayer, trgCreature);

					if (repNoto >= 0)
					{
						return repNoto;
					}
				}

				if (CheckHouseFlag(source, trgCreature, trgCreature.Location, trgCreature.Map))
				{
					return Notoriety.CanBeAttacked;
				}

				var actual = Notoriety.CanBeAttacked;

				if (trgCreature.Murderer)
				{
					actual = Notoriety.Murderer;
				}
				else if (body.IsMonster && IsSummoned(trgCreature))
				{
					actual = Notoriety.Murderer;
				}
				else if (trgCreature.AlwaysMurderer || trgCreature.IsAnimatedDead)
				{
					actual = Notoriety.Murderer;
				}

				if (DateTime.UtcNow >= target.TimeOfDeath + Corpse.MonsterLootRightSacrifice)
				{
					return actual;
				}

				var sourceParty = Party.Get(source);

				var list = target.Aggressors;

				for (var i = 0; i < list.Count; ++i)
				{
					if (list[i] == source)
					{
						return actual;
					}

					if (sourceParty != null && Party.Get(list[i]) == sourceParty)
					{
						return actual;
					}
				}
			}
			else
			{
				if (target.Murderer)
				{
					return Notoriety.Murderer;
				}

				if (target.Criminal && target.Map != null && !target.Map.Rules.HasFlag(MapRules.HarmfulRestrictions))
				{
					return Notoriety.Criminal;
				}

				var sourceGuild = GetGuildFor(source.Guild as Guild, source);
				var targetGuild = GetGuildFor(target.Guild, target.Owner);

				if (sourceGuild != null && targetGuild != null)
				{
					if (sourceGuild == targetGuild || sourceGuild.IsAlly(targetGuild))
					{
						return Notoriety.Ally;
					}

					if (sourceGuild.IsEnemy(targetGuild))
					{
						return Notoriety.Enemy;
					}
				}

				var srcFaction = Faction.Find(source, true, true);
				var trgFaction = Faction.Find(target.Owner, true, true);

				if (srcFaction != null && trgFaction != null && srcFaction != trgFaction && source.Map == Faction.Facet)
				{
					var secondList = target.Aggressors;

					for (var i = 0; i < secondList.Count; ++i)
					{
						if (secondList[i] == source || secondList[i] is BaseFactionGuard)
						{
							return Notoriety.Enemy;
						}
					}
				}

				var srcRepPlayer = srcPlayer ?? srcCreature?.GetRootMaster<PlayerMobile>();

				if (srcRepPlayer != null)
				{
					var repNoto = Reputation.ComputeNotoriety(srcRepPlayer, target.Owner);

					if (repNoto >= 0)
					{
						return repNoto;
					}
				}

				if (CheckHouseFlag(source, target.Owner, target.Location, target.Map))
				{
					return Notoriety.CanBeAttacked;
				}

				if (target.Owner is not PlayerMobile)
				{
					return Notoriety.CanBeAttacked;
				}

				var list = target.Aggressors;

				for (var i = 0; i < list.Count; ++i)
				{
					if (list[i] == source)
					{
						return Notoriety.CanBeAttacked;
					}
				}
			}

			return Notoriety.Innocent;
		}

		public static int MobileNotoriety(Mobile source, Mobile target)
		{
			var srcPlayer = source as PlayerMobile;
			var trgPlayer = target as PlayerMobile;

			var srcCreature = source as BaseCreature;
			var trgCreature = target as BaseCreature;

			if (Core.AOS)
			{
				if (target.Blessed)
				{
					return Notoriety.Invulnerable;
				}

				if (trgCreature != null && trgCreature.IsInvulnerable)
				{
					return Notoriety.Invulnerable;
				}

				if (target is PlayerVendor or TownCrier)
				{
					return Notoriety.Invulnerable;
				}
			}

			#region Dueling

			var pmFrom = srcPlayer ?? srcCreature?.GetRootMaster<PlayerMobile>();
			var pmTarg = trgPlayer ?? trgCreature?.GetRootMaster<PlayerMobile>();

			if (pmFrom != null && pmTarg != null)
			{
				if (pmFrom.DuelContext != null && pmFrom.DuelContext.StartedBeginCountdown && !pmFrom.DuelContext.Finished && pmFrom.DuelContext == pmTarg.DuelContext)
				{
					if (pmFrom.DuelContext.IsAlly(pmFrom, pmTarg))
					{
						return Notoriety.Ally;
					}

					return Notoriety.Enemy;
				}
			}

			#endregion

			if (target.AccessLevel > AccessLevel.Player)
			{
				return Notoriety.CanBeAttacked;
			}

			if (srcPlayer != null && trgCreature != null)
			{
				var master = trgCreature.GetMaster();

				if (master != null && master.AccessLevel > AccessLevel.Player)
				{
					return Notoriety.CanBeAttacked;
				}

				master = trgCreature.ControlMaster;

				if (Core.ML && master != null)
				{
					if (source == master && CheckAggressor(target.Aggressors, source))
					{
						return Notoriety.CanBeAttacked;
					}

					if (CheckAggressor(source.Aggressors, trgCreature))
					{
						return Notoriety.CanBeAttacked;
					}

					return MobileNotoriety(source, master);
				}

				if (!trgCreature.Summoned && !trgCreature.Controlled && srcPlayer.EnemyOfOneType == target.GetType())
				{
					return Notoriety.Enemy;
				}
			}

			if (target.Murderer)
			{
				return Notoriety.Murderer;
			}

			if (target.Body.IsMonster && IsSummoned(trgCreature))
			{
				if (target is not BaseFamiliar and not ArcaneFey and not Golem)
				{
					return Notoriety.Murderer;
				}
			}

			if (trgCreature != null && (trgCreature.AlwaysMurderer || trgCreature.IsAnimatedDead))
			{
				return Notoriety.Murderer;
			}

			if (target.Criminal)
			{
				return Notoriety.Criminal;
			}

			var sourceGuild = GetGuildFor(source.Guild as Guild, source);
			var targetGuild = GetGuildFor(target.Guild as Guild, target);

			if (sourceGuild != null && targetGuild != null)
			{
				if (sourceGuild == targetGuild || sourceGuild.IsAlly(targetGuild))
				{
					return Notoriety.Ally;
				}

				if (sourceGuild.IsEnemy(targetGuild))
				{
					return Notoriety.Enemy;
				}
			}

			var srcFaction = Faction.Find(source, true, true);
			var trgFaction = Faction.Find(target, true, true);

			if (srcFaction != null && trgFaction != null && srcFaction != trgFaction && source.Map == Faction.Facet)
			{
				return Notoriety.Enemy;
			}

			if (Stealing.ClassicMode && trgPlayer != null && trgPlayer.PermaFlags.Contains(source))
			{
				return Notoriety.CanBeAttacked;
			}

			if (target is BaseCreature trgCreature1 && trgCreature1.AlwaysAttackable)
			{
				return Notoriety.CanBeAttacked;
			}

			var srcRepPlayer = srcPlayer ?? srcCreature?.GetRootMaster<PlayerMobile>();

			if (srcRepPlayer != null)
			{
				var repNoto = Reputation.ComputeNotoriety(srcRepPlayer, target);

				if (repNoto >= 0)
				{
					return repNoto;
				}
			}

			if (CheckHouseFlag(source, target, target.Location, target.Map))
			{
				return Notoriety.CanBeAttacked;
			}

			if (trgCreature == null || trgCreature.InitialInnocent)
			{
				if (trgPlayer == null && !target.Body.IsHuman && !target.Body.IsGhost && !IsPet(trgCreature))
				{
					return Notoriety.CanBeAttacked;
				}

				if (!Core.ML && PolymorphSpell.IsPolymorphed(target))
				{
					return Notoriety.CanBeAttacked;
				}
			}

			if (CheckAggressor(source.Aggressors, target))
			{
				return Notoriety.CanBeAttacked;
			}

			if (CheckAggressed(source.Aggressed, target))
			{
				return Notoriety.CanBeAttacked;
			}

			if (trgCreature != null && trgCreature.Controlled && trgCreature.ControlOrder == OrderType.Guard && trgCreature.ControlTarget == source)
			{
				return Notoriety.CanBeAttacked;
			}

			if (srcCreature != null)
			{
				var master = srcCreature.GetMaster();

				if (master != null)
				{
					if (trgCreature != null)
					{
						return Notoriety.CanBeAttacked;
					}

					if (CheckAggressor(master.Aggressors, target))
					{
						return Notoriety.CanBeAttacked;
					}

					if (MobileNotoriety(master, target) == Notoriety.CanBeAttacked)
					{
						return Notoriety.CanBeAttacked;
					}
				}
			}

			return Notoriety.Innocent;
		}

		public static bool CheckHouseFlag(Mobile from, Mobile m, Point3D p, Map map)
		{
			var house = BaseHouse.FindHouseAt(p, map, 16);

			if (house == null || house.Public || !house.IsFriend(from))
			{
				return false;
			}

			if (m != null && house.IsFriend(m))
			{
				return false;
			}

			if (m is BaseCreature c && !c.Deleted && c.Controlled && c.ControlMaster != null)
			{
				return !house.IsFriend(c.ControlMaster);
			}

			return true;
		}

		public static bool IsPet(BaseCreature c)
		{
			return c != null && c.Controlled;
		}

		public static bool IsSummoned(BaseCreature c)
		{
			return c != null && c.Summoned;
		}

		public static bool CheckAggressor(List<AggressorInfo> list, Mobile target)
		{
			for (var i = 0; i < list.Count; ++i)
			{
				if (list[i].Attacker == target)
				{
					return true;
				}
			}

			return false;
		}

		public static bool CheckAggressed(List<AggressorInfo> list, Mobile target)
		{
			for (var i = 0; i < list.Count; ++i)
			{
				var info = list[i];

				if (!info.CriminalAggression && info.Defender == target)
				{
					return true;
				}
			}

			return false;
		}
	}

	#region Reputation

	public enum ReputationLevel
	{
		Hated,
		Hostile,
		Unfriendly,
		Neutral,
		Friendly,
		Honored,
		Revered,
		Exalted,
	}

	public enum ReputationCategory
	{
		Townships,
		Vendors,
	}

	public static class Reputation
	{
		public static string FilePath => Path.Combine(Core.CurrentSavesDirectory, "Reputation", "Reputation.bin");

		public static ImmutableList<ReputationCategory> Categories { get; }

		public static ImmutableList<ReputationLevel> Levels { get; }

		public static ImmutableDictionary<ReputationLevel, Color> LevelColors { get; }

		public static ImmutableDictionary<ReputationCategory, ImmutableHashSet<ReputationDefinition>> Definitions { get; }

		public static Dictionary<PlayerMobile, ReputationState> PlayerStates { get; } = new();

		static Reputation()
		{
			Categories = ImmutableList.Create(Enum.GetValues<ReputationCategory>());

			Levels = ImmutableList.Create(Enum.GetValues<ReputationLevel>());

			static Color getColor(Color cmin, Color cmax, float p)
			{
				var r = (byte)(cmin.R + (cmax.R - cmin.R) * p);
				var g = (byte)(cmin.G + (cmax.G - cmin.G) * p);
				var b = (byte)(cmin.B + (cmax.B - cmin.B) * p);

				return Color.FromArgb(Byte.MaxValue, r, g, b);
			};

			var colors = ImmutableDictionary.CreateBuilder<ReputationLevel, Color>();

			var index = 0;

			var cmin = Color.OrangeRed;
			var cmax = Color.Goldenrod;

			var eidx = Levels.IndexOf(ReputationLevel.Neutral);
			var lcap = (float)(eidx - 1);

			while (index < eidx)
			{
				colors[Levels[index]] = getColor(cmin, cmax, index / lcap);

				++index;
			}

			cmin = Color.Goldenrod;
			cmax = Color.LawnGreen;

			eidx = Levels.Count;
			lcap = (float)(eidx - 1);

			while (index < eidx)
			{
				colors[Levels[index]] = getColor(cmin, cmax, index / lcap);

				++index;
			}

			LevelColors = colors.ToImmutable();

			var definitions = ImmutableDictionary.CreateBuilder<ReputationCategory, ImmutableHashSet<ReputationDefinition>>();

			definitions[ReputationCategory.Townships] = ImmutableHashSet.CreateRange(Town.Towns.Select(t => t.Reputation).Cast<ReputationDefinition>());
			definitions[ReputationCategory.Vendors] = ImmutableHashSet.CreateRange(NpcGuilds.Guilds.Select(g => g.Reputation).Cast<ReputationDefinition>());

			Definitions = definitions.ToImmutable();
		}

		[CallPriority(Int32.MinValue + 20)]
		public static void Configure()
		{
			CommandSystem.Register("Reputation", AccessLevel.Player, e =>
			{
				if (e.Mobile is PlayerMobile player)
				{
					ReputationGump.DisplayTo(player);
				}
			});

			EventSink.WorldSave += OnSave;
			EventSink.WorldLoad += OnLoad;
		}

		public static void HandleDeletion(Mobile mob)
		{
			if (mob is PlayerMobile p)
			{
				_ = PlayerStates.Remove(p);
			}
		}

		public static void HandleDeath(Mobile victim, PlayerMobile killer)
		{
			foreach (var victimRep in GetDefinitions(victim))
			{
				victimRep.HandleDeath(killer, victim);
			}
		}

		public static int DeltaPoints(PlayerMobile player, Mobile target, int delta, bool message)
		{
			var points = 0;

			foreach (var definition in GetDefinitions(target))
			{
				points += DeltaPoints(player, definition, delta, message);
			}

			return points;
		}

		public static int DeltaPoints(PlayerMobile player, ReputationDefinition definition, int delta, bool message)
		{
			return definition?.DeltaPoints(player, delta, message) ?? 0;
		}

		public static ReputationState GetState(PlayerMobile player)
		{
			if (!PlayerStates.TryGetValue(player, out var state) || state == null)
			{
				if (!player.Deleted)
				{
					PlayerStates[player] = state = new ReputationState(player);
				}
			}
			else if (player.Deleted)
			{
				_ = PlayerStates.Remove(player);

				state = null;
			}

			return state;
		}

		public static ReputationEntry GetEntry(PlayerMobile player, ReputationDefinition definition)
		{
			var state = GetState(player);

			if (state != null)
			{
				return state.GetEntry(definition);
			}

			return null;
		}

		public static ReputationLevel GetLevel(PlayerMobile player, ReputationDefinition definition)
		{
			var entry = GetEntry(player, definition);

			if (entry != null)
			{
				return definition.ComputeLevel(entry.Points);
			}

			return ReputationLevel.Neutral;
		}

		public static int GetPoints(PlayerMobile player, ReputationDefinition definition)
		{
			var entry = GetEntry(player, definition);

			if (entry != null)
			{
				return entry.Points;
			}

			return definition[ReputationLevel.Neutral];
		}

		public static bool HasDefinition<T>(Mobile mobile, T definition) where T : ReputationDefinition
		{
			foreach (var def in InternalGetDefinitions<T>(mobile))
			{
				if (def == definition)
				{
					return true;
				}
			}

			return false;
		}

		public static IEnumerable<ReputationDefinition> GetDefinitions(Mobile mobile)
		{
			return GetDefinitions<ReputationDefinition>(mobile);
		}

		public static IEnumerable<T> GetDefinitions<T>(Mobile mobile) where T : ReputationDefinition
		{
			return InternalGetDefinitions<T>(mobile).Distinct();
		}

		private static IEnumerable<T> InternalGetDefinitions<T>(Mobile mobile) where T : ReputationDefinition
		{
			if (mobile?.Deleted != false)
			{
				yield break;
			}

			if (mobile is PlayerMobile player)
			{
				if (player.HomeTown?.Definition.Reputation is T tdef)
				{
					yield return tdef;
				}
			}
			else if (mobile is BaseCreature creature)
			{
				if (creature.ControlMaster != null)
				{
					foreach (var def in InternalGetDefinitions<T>(creature.ControlMaster))
					{
						yield return def;
					}
				}
				else if (creature.SummonMaster != null)
				{
					foreach (var def in InternalGetDefinitions<T>(creature.SummonMaster))
					{
						yield return def;
					}
				}

				if (creature is BaseFactionGuard fg)
				{
					var town = fg.Town;

					if (town?.Definition.Reputation is T tdef)
					{
						yield return tdef;
					}
				}

				if (creature is BaseVendor vendor)
				{
					if (vendor.NpcGuild != NpcGuild.None)
					{
						var npcGuild = vendor.NpcGuildInfo;

						if (npcGuild?.Reputation is T gdef)
						{
							yield return gdef;
						}
					}

					var town = vendor.HomeTown ?? Town.FromRegion(vendor.Region);

					if (town?.Definition.Reputation is T tdef)
					{
						yield return tdef;
					}
				}

				if (creature.Home != Point3D.Zero)
				{
					var region = Region.Find(creature.Home, creature.Map);

					if (region != null)
					{
						var town = Town.FromRegion(region);

						if (town?.Definition.Reputation is T tdef)
						{
							yield return tdef;
						}
					}
				}

				if (creature.Spawner != null && creature.Spawner.HomeLocation != Point3D.Zero)
				{
					var region = Region.Find(creature.Spawner.HomeLocation, creature.Map);

					if (region != null)
					{
						var town = Town.FromRegion(region);

						if (town?.Definition.Reputation is T tdef)
						{
							yield return tdef;
						}
					}
				}

				if (creature.Spawner is Item s && s.RootParent is not Mobile)
				{
					var region = Region.Find(s.GetWorldLocation(), s.Map);

					if (region != null)
					{
						var town = Town.FromRegion(region);

						if (town?.Definition.Reputation is T tdef)
						{
							yield return tdef;
						}
					}
				}
			}
		}

		public static bool IsEnemy(Mobile source, Mobile target)
		{
			return ComputeNotoriety(source, target) == Notoriety.Enemy;
		}

		public static bool IsAlly(Mobile source, Mobile target)
		{
			return ComputeNotoriety(source, target) == Notoriety.Ally;
		}

		public static bool IsNeutral(Mobile source, Mobile target)
		{
			return ComputeNotoriety(source, target) <= 0;
		}

		public static int ComputeNotoriety(Mobile source, Mobile target)
		{
			var enemies = 0;
			var allies = 0;

			foreach (var rep in Reputation.GetDefinitions(source))
			{
				if (rep.IsEnemy(target))
				{
					++enemies;
				}
				else if (rep.IsAlly(target))
				{
					++allies;
				}
			}

			foreach (var rep in Reputation.GetDefinitions(target))
			{
				if (rep.IsEnemy(source))
				{
					++enemies;
				}
				else if (rep.IsAlly(source))
				{
					++allies;
				}
			}

			if (enemies > allies)
			{
				return Notoriety.Enemy;
			}

			if (allies > enemies)
			{
				return Notoriety.Ally;
			}

			return -1;
		}

		public static void AddProperties(Mobile source, ObjectPropertyList list)
		{
			StringBuilder sb = null;

			foreach (var rep in GetDefinitions(source))
			{
				var title = rep.Name;

				if (!String.IsNullOrWhiteSpace(title))
				{
					sb ??= new();

					_ = sb.AppendLine(title);
				}
			}

			if (sb?.Length > 0)
			{
				list.Add(1114057, sb.ToString()); // ~1_val~
			}
		}

		public static void WriteReference(GenericWriter writer, ReputationDefinition definition)
		{
			if (definition is TownReputationDefinition townDef)
			{
				writer.WriteEncodedInt(1);

				Town.WriteReference(writer, townDef.Owner);
			}
			else if (definition is NpcGuildReputationDefinition npcGuildDef)
			{
				writer.WriteEncodedInt(2);

				NpcGuilds.WriteReference(writer, npcGuildDef.Owner);
			}
			else
			{
				writer.WriteEncodedInt(-1);
			}
		}

		public static ReputationDefinition ReadReference(GenericReader reader)
		{
			var sub = reader.ReadEncodedInt();

			switch (sub)
			{
				case 1:
					{
						var town = Town.ReadReference(reader);

						return town?.Definition.Reputation;
					}
				case 2:
					{
						var npcGuild = NpcGuilds.ReadReference(reader);

						return npcGuild?.Reputation;
					}
			}

			return null;
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
				var state = new ReputationState(reader);

				if (state.Owner?.Deleted == false)
				{
					PlayerStates[state.Owner] = state;
				}
			}
		}
	}

	public abstract class ReputationDefinition<T> : ReputationDefinition
	{
		public T Owner { get; }

		public ReputationDefinition(T owner, ReputationCategory category, string name, string description, params int[] levels)
			: base(category, name, description, levels)
		{
			Owner = owner;
		}
	}

	[Parsable]
	public abstract class ReputationDefinition
	{
		private sealed class Empty : ReputationDefinition
		{
			public Empty(int[] levels)
				: base((ReputationCategory)(-1), String.Empty, String.Empty, levels)
			{ }
		}

		public static readonly ReputationDefinition Default;

		static ReputationDefinition()
		{
			var values = Enum.GetValues<ReputationLevel>();
			var levels = new int[values.Length];

			for (var i = 1; i < levels.Length; i++)
			{
				levels[i] = (1 << (i - 1)) * 1000;
			}

			Default = new Empty(levels);
		}

		public static bool TryParse(string input, out ReputationDefinition value)
		{
			try
			{
				value = Parse(input);
				return true;
			}
			catch
			{
				value = Default;
				return false;
			}
		}

		public static ReputationDefinition Parse(string input)
		{
			var index = input.IndexOf('|');

			var cats = input.Substring(0, index);
			var name = input.Substring(index + 1);

			var cat = Enum.Parse<ReputationCategory>(cats);
			var defs = Reputation.Definitions[cat];

			foreach (var def in defs)
			{
				if (Insensitive.Equals(def.Name, name))
				{
					return def;
				}
			}

			return null;
		}

		private readonly int[] m_Levels;

		public int this[ReputationLevel level] => m_Levels[(int)level];

		public int PointsMin => this[ReputationLevel.Hated];
		public int PointsMax => this[ReputationLevel.Exalted];
		public int PointsDef => this[ReputationLevel.Neutral];

		public ReputationCategory Category { get; }

		public string Name { get; }
		public string Description { get; }

		public ReputationDefinition(ReputationCategory cat, string name, string description, params int[] levels)
		{
			Category = cat;
			Name = name;
			Description = description;

			if (this is Empty)
			{
				m_Levels = levels;
			}
			else
			{
				m_Levels = Default.m_Levels.ToArray();

				if (levels?.Length > 0)
				{
					Array.Copy(levels, m_Levels, Math.Min(levels.Length, m_Levels.Length));
				}
			}
		}

		public override string ToString()
		{
			return $"{Category}|{Name}";
		}

		public ReputationLevel ComputeLevel(int points)
		{
			var index = -1;

			while (++index < m_Levels.Length)
			{
				if (points <= m_Levels[index])
				{
					break;
				}
			}

			return (ReputationLevel)index;
		}

		public Color ComputeLevelColor(int points)
		{
			var level = ComputeLevel(points);

			Reputation.LevelColors.TryGetValue(level, out var color);

			return color;
		}

		public virtual bool IsEnemy(Mobile mobile)
		{
			if (mobile is PlayerMobile p)
			{
				return Reputation.GetLevel(p, this) <= ReputationLevel.Hostile;
			}

			if (mobile is BaseCreature c)
			{
				var master = c.GetMaster();

				if (master != null)
				{
					return IsEnemy(master);
				}
			}

			return false;
		}

		public virtual bool IsAlly(Mobile mobile)
		{
			if (mobile is PlayerMobile p)
			{
				return Reputation.GetLevel(p, this) >= ReputationLevel.Revered;
			}

			if (mobile is BaseCreature c)
			{
				var master = c.GetMaster();

				if (master != null)
				{
					return IsAlly(master);
				}
			}

			if (Reputation.HasDefinition(mobile, this))
			{
				return true;
			}

			return false;
		}

		public virtual void HandleDeath(PlayerMobile killer, Mobile victim)
		{
			if (killer == null || victim == null)
			{
				return;
			}

			var points = -10;

			if (victim is PlayerMobile playerVictim)
			{
				var victimLevel = Reputation.GetLevel(playerVictim, this);

				if (victimLevel < ReputationLevel.Neutral)
				{
					points *= -1;
				}
				else if (victimLevel >= ReputationLevel.Honored)
				{
					points *= 1 + (ReputationLevel.Exalted - victimLevel);
				}
			}

			_ = DeltaPoints(killer, points, true);
		}

		public virtual int DeltaPoints(PlayerMobile player, int delta, bool message)
		{
			if (player == null || delta == 0)
			{
				return 0;
			}

			var entry = Reputation.GetEntry(player, this);

			if (entry == null)
			{
				return 0;
			}

			var oldLevel = entry.Level;
			var oldPoints = entry.Points;

			if (delta > 0)
			{
				entry.Points += delta;

				if (message && entry.Points > oldPoints)
				{
					player.SendMessage(0x55, $"You earned {entry.Points - oldPoints:N0} reputation with {entry.Definition.Name}");
				}
			}
			else if (delta < 0)
			{
				entry.Points -= delta;

				if (message && entry.Points < oldPoints)
				{
					player.SendMessage(0x22, $"You lost {oldPoints - entry.Points:N0} reputation with {entry.Definition.Name}");
				}
			}

			if (message)
			{
				var level = entry.Level;

				if (level > oldLevel)
				{
					player.SendMessage(0x55, $"You are now {level} with {entry.Definition.Name}");
				}
				else if (level < oldLevel)
				{
					player.SendMessage(0x22, $"You are now {level} with {entry.Definition.Name}");
				}
			}

			player.Delta(MobileDelta.Noto | MobileDelta.Properties);

			return delta;
		}
	}

	[PropertyObject]
	public class ReputationState : ICollection<ReputationEntry>
	{
		[CommandProperty(AccessLevel.Counselor, true)]
		public PlayerMobile Owner { get; private set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public HashSet<ReputationEntry> Entries { get; protected set; } = new();

		[CommandProperty(AccessLevel.Counselor, true)]
		public int Count => Entries.Count;

		public ReputationEntry this[ReputationDefinition definition] => GetEntry(definition);

		bool ICollection<ReputationEntry>.IsReadOnly => ((ICollection<ReputationEntry>)Entries).IsReadOnly;

		public ReputationState(PlayerMobile owner)
		{
			Owner = owner;
		}

		public ReputationState(GenericReader reader)
		{
			Deserialize(reader);
		}

		public ReputationEntry GetEntry(ReputationDefinition definition)
		{
			if (definition == null)
			{
				return null;
			}

			foreach (var entry in Entries)
			{
				if (entry.Definition == definition)
				{
					return entry;
				}
			}

			var add = new ReputationEntry(definition);

			_ = Entries.Add(add);

			return add;
		}

		void ICollection<ReputationEntry>.Add(ReputationEntry item)
		{
			_ = Entries.Add(item);
		}

		public void Clear()
		{
			Entries.Clear();
		}

		public bool Contains(ReputationEntry entry)
		{
			return Entries.Contains(entry);
		}

		public void CopyTo(ReputationEntry[] array, int arrayIndex)
		{
			Entries.CopyTo(array, arrayIndex);
		}

		public bool Remove(ReputationEntry item)
		{
			return Entries.Remove(item);
		}

		public IEnumerator<ReputationEntry> GetEnumerator()
		{
			return Entries.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.Write(Owner);

			writer.WriteEncodedInt(Entries.Count);

			foreach (var entry in Entries)
			{
				entry.Serialize(writer);
			}
		}

		public void Deserialize(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();

			Owner = reader.ReadMobile<PlayerMobile>();

			var count = reader.ReadEncodedInt();

			while (--count >= 0)
			{
				var entry = new ReputationEntry(reader);

				if (entry.Definition != null)
				{
					_ = Entries.Add(entry);
				}
			}
		}
	}

	[PropertyObject]
	public class ReputationEntry
	{
		[CommandProperty(AccessLevel.Counselor, true)]
		public ReputationDefinition Definition { get; private set; }

		[CommandProperty(AccessLevel.Counselor, true)]
		public string Name => Definition.Name;

		[CommandProperty(AccessLevel.Counselor, true)]
		public string Description => Definition.Description;

		private int m_Points;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Points
		{
			get => m_Points;
			set
			{
				value = Math.Clamp(value, PointsMin, PointsMax);

				if (m_Points != value)
				{
					m_Points = value;
					m_Level = null;
					m_LevelColor = null;
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, true)]
		public int PointsPrev
		{
			get
			{
				var level = Level - 1;

				if (Enum.IsDefined(level))
				{
					return Definition[level];
				}

				return PointsMin;
			}
		}

		[CommandProperty(AccessLevel.Counselor, true)]
		public int PointsPrevReq => PointsPrev - Points;

		[CommandProperty(AccessLevel.Counselor, true)]
		public int PointsNext
		{
			get
			{
				var level = Level + 1;

				if (Enum.IsDefined(level))
				{
					return Definition[level];
				}

				return PointsMax;
			}
		}

		[CommandProperty(AccessLevel.Counselor, true)]
		public int PointsNextReq => PointsNext - Points;

		[CommandProperty(AccessLevel.Counselor, true)]
		public int PointsMin => Definition.PointsMin;

		[CommandProperty(AccessLevel.Counselor, true)]
		public int PointsMax => Definition.PointsMax;

		[CommandProperty(AccessLevel.Counselor, true)]
		public int PointsDef => Definition.PointsDef;

		private ReputationLevel? m_Level = null;

		[CommandProperty(AccessLevel.Counselor, true)]
		public ReputationLevel Level => m_Level ??= Definition.ComputeLevel(Points);

		private Color? m_LevelColor = null;

		[CommandProperty(AccessLevel.Counselor, true)]
		public Color LevelColor => m_LevelColor ??= Definition.ComputeLevelColor(Points);

		public ReputationEntry(ReputationDefinition definition)
		{
			Definition = definition;

			m_Points = PointsDef;
		}

		public ReputationEntry(GenericReader reader)
		{
			Deserialize(reader);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			Reputation.WriteReference(writer, Definition);

			writer.WriteEncodedInt(m_Points);
		}

		public void Deserialize(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();

			Definition = Reputation.ReadReference(reader);

			m_Points = reader.ReadEncodedInt();
		}
	}

	public class ReputationGump : BaseGridGump
	{
		private const int EntriesPerPage = 15;

		private static readonly ImmutableList<ReputationEntry> m_Empty = ImmutableList.Create<ReputationEntry>();

		public static ReputationGump DisplayTo(PlayerMobile player)
		{
			var gump = new ReputationGump(player);

			_ = player.CloseGump(gump.GetType());
			_ = player.SendGump(gump);

			return gump;
		}

		private readonly PlayerMobile m_Player;

		private readonly ReputationCategory m_Category;

		private readonly int m_Page, m_PageCount;

		private readonly bool m_Staff;

		private readonly ImmutableList<ReputationEntry> m_View = m_Empty;

		public override int BorderSize => 10;
		public override int OffsetSize => 1;

		public override int EntryHeight => 20;

		public override int OffsetGumpID => 0x2430;
		public override int HeaderGumpID => 0x243A;
		public override int EntryGumpID => 0x2458;
		public override int BackGumpID => 0x2486;

		public override int TextHue => 0;
		public override int TextOffsetX => 2;

		public virtual int PageLeftID1 => 0x25EA;
		public virtual int PageLeftID2 => 0x25EB;
		public virtual int PageLeftWidth => 16;
		public virtual int PageLeftHeight => 16;

		public virtual int PageRightID1 => 0x25E6;
		public virtual int PageRightID2 => 0x25E7;
		public virtual int PageRightWidth => 16;
		public virtual int PageRightHeight => 16;

		public virtual int EntryInfoID1 => 0x5689;
		public virtual int EntryInfoID2 => 0x568B;
		public virtual int EntryInfoWidth => 16;
		public virtual int EntryInfoHeight => 16;

		public ReputationGump(PlayerMobile player)
			: this(player, default)
		{ }

		public ReputationGump(PlayerMobile player, ReputationCategory category)
			: this(player, category, 0)
		{ }

		public ReputationGump(PlayerMobile player, ReputationCategory category, int page)
			: this(player, category, page, null)
		{ }

		private ReputationGump(PlayerMobile player, ReputationCategory category, int page, ImmutableList<ReputationEntry> view)
			: base(50, 50)
		{
			m_Player = player;
			m_Category = category;

			if (view != null)
			{
				m_View = view;
				m_Page = page;
			}
			else if (Reputation.Definitions.TryGetValue(m_Category, out var defs))
			{
				var state = Reputation.GetState(m_Player);

				if (state != null)
				{
					m_View = ImmutableList.CreateRange(defs.Select(state.GetEntry));
					m_Page = page;
				}
			}

			m_PageCount = (m_View.Count + EntriesPerPage - 1) / EntriesPerPage;

			m_Staff = m_Player.AccessLevel >= AccessLevel.GameMaster;

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = true;

			var nameWidth = 160;
			var progWidth = 180;

			AddNewPage();

			AddEntryHtml(20 + nameWidth + progWidth + 20 + (m_Staff ? 20 : 0) + (OffsetSize * (m_Staff ? 3 : 2)), SetCenter($"{m_Category}"));

			AddNewLine();

			AddEntryButton(20, PageLeftID1, PageLeftID2, 1, PageLeftWidth, PageLeftHeight);

			AddEntryHtml(OffsetSize + nameWidth + progWidth + (m_Staff ? 20 : 0), SetCenter($"Page {m_Page + 1} of {m_PageCount}"));

			AddEntryButton(20, PageRightID1, PageRightID2, 2, PageRightWidth, PageRightHeight);

			for (int i = m_Page * EntriesPerPage, line = 0, bid = 5 + i; line < EntriesPerPage && i < m_View.Count; ++i, ++line)
			{
				AddNewLine();

				AddEntryProgress(20, nameWidth, progWidth, m_View[i]);

				AddEntryButton(20, EntryInfoID1, EntryInfoID2, bid++, EntryInfoWidth, EntryInfoHeight);

				if (m_Staff)
				{
					AddEntryButton(20, PageRightID1, PageRightID2, bid++, PageRightWidth, PageRightHeight);
				}
			}

			FinishPage();
		}

		public void AddEntryProgress(int padding, int nameWidth, int progWidth, ReputationEntry entry)
		{
			AddEntryHtml(padding + nameWidth, entry.Name);

			AddImageTiled(CurrentX, CurrentY, progWidth, EntryHeight, 2624);

			var pointsDelta = entry.Points - entry.PointsPrev;
			var pointsLimit = entry.PointsNext - entry.PointsPrev;

			var fillWidth = (int)(progWidth * (pointsDelta / (float)pointsLimit));

			if (fillWidth <= 0)
			{
				fillWidth = progWidth;
			}

			AddHtml(CurrentX, CurrentY, fillWidth, EntryHeight, SetBGColor(entry.LevelColor), false, false);

			if (fillWidth < progWidth && entry.Level > 0)
			{
				var levelColor = Reputation.LevelColors[entry.Level - 1];

				AddHtml(CurrentX + fillWidth, CurrentY, progWidth - fillWidth, EntryHeight, SetBGColor(levelColor), false, false);
			}

			var text = $"{entry.Level} [{pointsDelta:N0} / {pointsLimit:N0}]";

			text = SetSmall(SetCenter(SetColor(text, Color.White)));

			AddHtml(CurrentX + TextOffsetX, CurrentY, progWidth - TextOffsetX, EntryHeight, text, false, false);

			IncreaseX(progWidth);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			switch (info.ButtonID)
			{
				case 0:
					{
						_ = m_Player.CloseGump(typeof(ReputationGump));

						return;
					}
				case 1:
					{
						if (m_Page > 0)
						{
							_ = m_Player.SendGump(new ReputationGump(m_Player, m_Category, m_Page - 1, m_View));
						}
						else
						{
							var cat = m_Category - 1;

							if (!Enum.IsDefined(cat))
							{
								cat = Reputation.Categories[^1];
							}

							var page = 0;

							if (Reputation.Definitions.TryGetValue(cat, out var defs))
							{
								page = Math.Max(0, ((defs.Count + EntriesPerPage - 1) / EntriesPerPage) - 1);
							}

							_ = m_Player.SendGump(new ReputationGump(m_Player, cat, page));
						}

						return;
					}
				case 2:
					{
						if ((m_Page + 1) * EntriesPerPage < m_View.Count)
						{
							_ = m_Player.SendGump(new ReputationGump(m_Player, m_Category, m_Page + 1, m_View));
						}
						else
						{
							var cat = m_Category + 1;

							if (!Enum.IsDefined(cat))
							{
								cat = Reputation.Categories[0];
							}

							_ = m_Player.SendGump(new ReputationGump(m_Player, cat));
						}

						return;
					}
			}
			
			var bid = info.ButtonID - 5;
			var num = m_Staff ? 2 : 1;

			if (bid >= 0 && bid < m_View.Count * num)
			{
				var v = bid / num;
				var n = bid % num;

				var entry = m_View[v];

				if (m_Staff && n != 0)
				{
					_ = m_Player.SendGump(new ReputationGump(m_Player, m_Category, m_Page, m_View));

					if (n == 1)
					{
						_ = m_Player.SendGump(new PropertiesGump(m_Player, entry));
					}
				}
				else
				{
					_ = m_Player.SendGump(new NoticeGump<ReputationEntry>(entry.Name, 0xFFFFFF, entry.Description, 0xFFFFFF, 420, 420, (_, _) =>
					{
						_ = m_Player.SendGump(new ReputationGump(m_Player, m_Category, m_Page, m_View));
					}, entry));
				}
			}
			else
			{
				_ = m_Player.SendGump(new ReputationGump(m_Player, m_Category, m_Page, m_View));
			}
		}
	}

	#endregion
}