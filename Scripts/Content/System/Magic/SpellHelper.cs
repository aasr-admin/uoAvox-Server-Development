using Server.Engines.PartySystem;
using Server.Factions;
using Server.Guilds;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.Spells.Bushido;
using Server.Spells.Magery;
using Server.Spells.Mysticism;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.Spells
{
	public static class SpellHelper
	{
		public static bool? TryCast(Mobile caster, SpellName id, bool message)
		{
			var book = SpellbookHelper.Find(caster, id);

			return TryCast(caster, book, id, message);
		}

		public static bool? TryCast(Mobile caster, SpellInfo info, bool message)
		{
			var book = SpellbookHelper.Find(caster, info.ID);

			return TryCast(caster, book, info, message);
		}

		public static bool? TryCast(Mobile caster, ISpellbook book, SpellName id, bool message)
		{
			var info = SpellRegistry.GetInfo(id);

			return TryCast(caster, book, info, message);
		}

		public static bool? TryCast(Mobile caster, ISpellbook book, SpellInfo info, bool message)
		{
			if (caster?.Deleted != false)
			{
				return null;
			}

			if (info == null || (book?.HasSpell(info.ID)) != true)
			{
				if (message)
				{
					caster.SendMessage("You do not have that spell.");
				}

				return null;
			}

			if (!info.IsValid || !info.Enabled)
			{
				if (message)
				{
					caster.SendLocalizedMessage(502345); // This spell has been temporarily disabled.				
				}

				return null;
			}

			var spell = SpellRegistry.NewSpell(info.ID, caster, null);

			if (spell == null)
			{
				if (message)
				{
					caster.SendLocalizedMessage(502345); // This spell has been temporarily disabled.				
				}

				return null;
			}

			return spell.Cast();
		}

		public static TimeSpan AosDamageDelay { get; set; } = TimeSpan.FromSeconds(1.0);
		public static TimeSpan OldDamageDelay { get; set; } = TimeSpan.FromSeconds(0.5);

		public static TimeSpan GetDamageDelayForSpell(Spell sp)
		{
			if (!sp.DelayedDamage)
			{
				return TimeSpan.Zero;
			}

			if (Core.AOS)
			{
				return AosDamageDelay;
			}

			return OldDamageDelay;
		}

		public static bool CheckMulti(Point3D p, Map map)
		{
			return CheckMulti(p, map, true, 0);
		}

		public static bool CheckMulti(Point3D p, Map map, bool houses)
		{
			return CheckMulti(p, map, houses, 0);
		}

		public static bool CheckMulti(Point3D p, Map map, bool houses, int housingrange)
		{
			if (map == null || map == Map.Internal)
			{
				return false;
			}

			var sector = map.GetSector(p.X, p.Y);

			for (var i = 0; i < sector.Multis.Count; ++i)
			{
				var multi = sector.Multis[i];

				if (multi is BaseHouse bh)
				{
					if (houses && bh.IsInside(p, 16))
					{
						return true;
					}

					if (housingrange > 0 && bh.InRange(p, housingrange))
					{
						return true;
					}
				}
				else if (multi.Contains(p))
				{
					return true;
				}
			}

			return false;
		}

		public static void Turn(Mobile from, object to)
		{
			if (to is not IPoint3D target)
			{
				return;
			}

			if (target is Item item)
			{
				if (item.RootParent != from)
				{
					from.Direction = from.GetDirectionTo(item.GetWorldLocation());
				}
			}
			else if (from != target)
			{
				from.Direction = from.GetDirectionTo(target);
			}
		}

		public static TimeSpan CombatHeatDelay { get; set; } = TimeSpan.FromSeconds(30.0);
		public static bool RestrictTravelCombat { get; set; } = true;

		public static bool CheckCombat(Mobile m)
		{
			if (!RestrictTravelCombat)
			{
				return false;
			}

			var now = DateTime.UtcNow;

			for (var i = 0; i < m.Aggressed.Count; ++i)
			{
				var info = m.Aggressed[i];

				if (info.Defender.Player && (now - info.LastCombatTime) < CombatHeatDelay)
				{
					return true;
				}
			}

			if (Core.Expansion == Expansion.AOS)
			{
				for (var i = 0; i < m.Aggressors.Count; ++i)
				{
					var info = m.Aggressors[i];

					if (info.Attacker.Player && (now - info.LastCombatTime) < CombatHeatDelay)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static bool AdjustField(ref Point3D p, Map map, int height, bool mobsBlock)
		{
			if (map == null)
			{
				return false;
			}

			var loc = new Point3D(p.X, p.Y, p.Z);

			for (var offset = 0; offset < 10; ++offset)
			{
				loc.Z -= offset;

				if (map.CanFit(loc, height, true, mobsBlock))
				{
					p = loc;

					return true;
				}
			}

			return false;
		}

		public static bool CanRevealCaster(Mobile m)
		{
			return m is BaseCreature c && !c.Controlled;
		}

		public static void GetSurfaceTop(ref IPoint3D p)
		{
			if (p is Item item)
			{
				p = item.GetSurfaceTop();
			}
			else if (p is StaticTarget t)
			{
				var z = t.Z;

				if ((t.Flags & TileFlag.Surface) == 0)
				{
					z -= TileData.ItemTable[t.ItemID & TileData.MaxItemValue].CalcHeight;
				}

				p = new Point3D(t.X, t.Y, z);
			}
		}

		public static bool AddStatOffset(Mobile m, StatType type, int offset, TimeSpan duration)
		{
			if (offset > 0)
			{
				return AddStatBonus(m, m, type, offset, duration);
			}

			if (offset < 0)
			{
				return AddStatCurse(m, m, type, -offset, duration);
			}

			return true;
		}

		public static bool AddStatBonus(Mobile caster, Mobile target, StatType type)
		{
			return AddStatBonus(caster, target, type, GetOffset(caster, target, type, false), GetDuration(caster, target));
		}

		public static bool AddStatBonus(Mobile caster, Mobile target, StatType type, int bonus, TimeSpan duration)
		{
			var offset = bonus;
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = target.GetStatMod(name);

			if (mod != null && mod.Offset < 0)
			{
				target.AddStatMod(new StatMod(type, name, mod.Offset + offset, duration));
				return true;
			}

			if (mod == null || mod.Offset < offset)
			{
				target.AddStatMod(new StatMod(type, name, offset, duration));
				return true;
			}

			return false;
		}

		public static bool AddStatCurse(Mobile caster, Mobile target, StatType type)
		{
			return AddStatCurse(caster, target, type, GetOffset(caster, target, type, true), GetDuration(caster, target));
		}

		public static bool AddStatCurse(Mobile caster, Mobile target, StatType type, int curse, TimeSpan duration)
		{
			var offset = -curse;
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = target.GetStatMod(name);

			if (mod != null && mod.Offset > 0)
			{
				target.AddStatMod(new StatMod(type, name, mod.Offset + offset, duration));
				return true;
			}

			if (mod == null || mod.Offset > offset)
			{
				target.AddStatMod(new StatMod(type, name, offset, duration));
				return true;
			}

			return false;
		}

		public static bool RemoveStatBonus(Mobile m, StatType type)
		{
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = m.GetStatMod(name);

			return mod?.Offset > 0 && m.RemoveStatMod(name);
		}

		public static bool RemoveStatCurse(Mobile m, StatType type)
		{
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = m.GetStatMod(name);

			return mod?.Offset < 0 && m.RemoveStatMod(name);
		}

		public static bool RemoveStatOffset(Mobile m, StatType type)
		{
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = m.GetStatMod(name);

			return mod?.Offset != 0 && m.RemoveStatMod(name);
		}

		public static bool RemoveStatOffset(Mobile m, StatType type, int value)
		{
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = m.GetStatMod(name);

			return mod?.Offset == value && m.RemoveStatMod(name);
		}

		public static bool HasStatBonus(Mobile m, StatType type)
		{
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = m.GetStatMod(name);

			return mod?.Offset > 0;
		}

		public static bool HasStatCurse(Mobile m, StatType type)
		{
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = m.GetStatMod(name);

			return mod?.Offset < 0;
		}

		public static bool HasStatOffset(Mobile m, StatType type)
		{
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = m.GetStatMod(name);

			return mod?.Offset != 0;
		}

		public static bool HasStatOffset(Mobile m, StatType type, int value)
		{
			var name = String.Format("[Magic] {0} Offset", type);

			var mod = m.GetStatMod(name);

			return mod?.Offset == value;
		}

		public static TimeSpan GetDuration(Mobile caster, Mobile target)
		{
			if (Core.AOS)
			{
				return TimeSpan.FromSeconds((6 * caster.Skills.EvalInt.Fixed / 50) + 1);
			}

			return TimeSpan.FromSeconds(caster.Skills[SkillName.Magery].Value * 1.2);
		}

		public static bool DisableSkillCheck { get; set; }

		public static double GetOffsetScalar(Mobile caster, Mobile target, bool curse)
		{
			double percent;

			if (curse)
			{
				percent = 8 + (caster.Skills.EvalInt.Fixed / 100) - (target.Skills.MagicResist.Fixed / 100);
			}
			else
			{
				percent = 1 + (caster.Skills.EvalInt.Fixed / 100);
			}

			percent *= 0.01;

			if (percent < 0)
			{
				percent = 0;
			}

			return percent;
		}

		public static int GetOffset(Mobile caster, Mobile target, StatType type, bool curse)
		{
			if (Core.AOS)
			{
				if (!DisableSkillCheck)
				{
					_ = caster.CheckSkill(SkillName.EvalInt, 0.0, 120.0);

					if (curse)
					{
						_ = target.CheckSkill(SkillName.MagicResist, 0.0, 120.0);
					}
				}

				var percent = GetOffsetScalar(caster, target, curse);

				switch (type)
				{
					case StatType.Str: return (int)(target.RawStr * percent);
					case StatType.Dex: return (int)(target.RawDex * percent);
					case StatType.Int: return (int)(target.RawInt * percent);
				}
			}

			return 1 + (int)(caster.Skills[SkillName.Magery].Value * 0.1);
		}

		public static Guild GetGuildFor(Mobile m)
		{
			var g = m.Guild as Guild;

			if (g == null && m is BaseCreature c)
			{
				m = c.ControlMaster;

				if (m != null)
				{
					g = m.Guild as Guild;
				}

				if (g == null)
				{
					m = c.SummonMaster;

					if (m != null)
					{
						g = m.Guild as Guild;
					}
				}
			}

			return g;
		}

		public static bool ValidIndirectTarget(Mobile from, Mobile to)
		{
			if (from == to)
			{
				return true;
			}

			if (to.Hidden && to.AccessLevel > from.AccessLevel)
			{
				return false;
			}

			#region Dueling
			var pmFrom = from as PlayerMobile;
			var pmTarg = to as PlayerMobile;

			if (pmFrom == null && from is BaseCreature bcFrom && bcFrom.Summoned)
			{
				pmFrom = bcFrom.SummonMaster as PlayerMobile;
			}

			if (pmTarg == null && to is BaseCreature bcTarg && bcTarg.Summoned)
			{
				pmTarg = bcTarg.SummonMaster as PlayerMobile;
			}

			if (pmFrom != null && pmTarg != null)
			{
				if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && pmFrom.DuelContext.Started && pmFrom.DuelPlayer != null && pmTarg.DuelPlayer != null)
				{
					return pmFrom.DuelPlayer.Participant != pmTarg.DuelPlayer.Participant;
				}
			}
			#endregion

			var fromGuild = GetGuildFor(from);
			var toGuild = GetGuildFor(to);

			if (fromGuild != null && toGuild != null && (fromGuild == toGuild || fromGuild.IsAlly(toGuild)))
			{
				return false;
			}

			var fp = Party.Get(from);

			if (fp != null && fp.Contains(to))
			{
				return false;
			}

			if (to is BaseCreature tc)
			{
				if (tc.Controlled || tc.Summoned)
				{
					var master = tc.GetMaster();

					if (master == from)
					{
						return false;
					}

					if (fp != null && fp.Contains(master))
					{
						return false;
					}
				}
			}

			if (from is BaseCreature fc)
			{
				if (fc.Controlled || fc.Summoned)
				{
					var master = fc.GetMaster();

					if (master == to)
					{
						return false;
					}

					var tp = Party.Get(to);

					if (tp != null && tp.Contains(master))
					{
						return false;
					}
				}
			}

			if (to is BaseCreature c && !c.Controlled && c.InitialInnocent)
			{
				return true;
			}

			var noto = Notoriety.Compute(from, to);

			return noto != Notoriety.Innocent || from.Murderer;
		}

		private static readonly int[] m_Offsets =
		{
			-1, -1,
			-1, 00,
			-1, +1,
			00, -1,
			00, +1,
			+1, -1,
			+1, 00,
			+1, +1
		};

		public static bool Summon(BaseCreature creature, Mobile caster, int sound, TimeSpan duration, bool scaleDuration, bool scaleStats)
		{
			var map = caster.Map;

			if (map == null || map == Map.Internal)
			{
				return false;
			}

			var scale = 1.0 + ((caster.Skills.Magery.Value - 100.0) / 200.0);

			if (scaleDuration)
			{
				duration = TimeSpan.FromSeconds(duration.TotalSeconds * scale);
			}

			if (scaleStats)
			{
				creature.RawStr = (int)(creature.RawStr * scale);
				creature.Hits = creature.HitsMax;

				creature.RawDex = (int)(creature.RawDex * scale);
				creature.Stam = creature.StamMax;

				creature.RawInt = (int)(creature.RawInt * scale);
				creature.Mana = creature.ManaMax;
			}

			var p = caster.Location;

			if (!FindValidSpawnLocation(map, ref p, true))
			{
				caster.SendLocalizedMessage(501942); // That location is blocked.
				creature.Delete();
				return false;
			}

			return BaseCreature.Summon(creature, caster, p, sound, duration);
		}

		public static bool FindValidSpawnLocation(Map map, ref Point3D p, bool surroundingsOnly)
		{
			if (map == null || map == Map.Internal)
			{
				return false;
			}

			if (!surroundingsOnly)
			{
				if (map.CanSpawnMobile(p))
				{
					return true;
				}

				var z = map.GetAverageZ(p.X, p.Y);

				if (map.CanSpawnMobile(p.X, p.Y, z))
				{
					p = new Point3D(p.X, p.Y, z);
					return true;
				}
			}

			var offset = Utility.Random(8) * 2;

			for (var i = 0; i < m_Offsets.Length; i += 2)
			{
				var x = p.X + m_Offsets[(offset + i) % m_Offsets.Length];
				var y = p.Y + m_Offsets[(offset + i + 1) % m_Offsets.Length];

				if (map.CanSpawnMobile(x, y, p.Z))
				{
					p = new Point3D(x, y, p.Z);
					return true;
				}

				var z = map.GetAverageZ(x, y);

				if (map.CanSpawnMobile(x, y, z))
				{
					p = new Point3D(x, y, z);
					return true;
				}
			}

			return false;
		}

		private delegate bool TravelValidator(Map map, Point3D loc);

		private static readonly TravelValidator[] m_Validators =
		{
			IsFeluccaT2A,
			IsKhaldun,
			IsIlshenar,
			IsTrammelWind,
			IsFeluccaWind,
			IsFeluccaDungeon,
			IsTrammelSolenHive,
			IsFeluccaSolenHive,
			IsCrystalCave,
			IsDoomGauntlet,
			IsDoomFerry,
			IsSafeZone,
			IsFactionStronghold,
			IsChampionSpawn,
			IsTokunoDungeon,
			IsLampRoom,
			IsGuardianRoom,
			IsHeartwood,
			IsMLDungeon
		};

		private static readonly bool[,] m_Rules =
		{
					/*T2A(Fel),	Khaldun,	Ilshenar,	Wind(Tram),	Wind(Fel),	Dungeons(Fel),	Solen(Tram),	Solen(Fel),	CrystalCave(Malas),	Gauntlet(Malas),	Gauntlet(Ferry),	SafeZone,	Stronghold,	ChampionSpawn,	Dungeons(Tokuno[Malas]),	LampRoom(Doom),	GuardianRoom(Doom),	Heartwood,	MLDungeons */
/* Recall From */	{ false,    false,      true,       true,       false,      false,          true,           false,      false,              false,              false,              true,       true,       false,          true,                       false,          false,              false,      false },
/* Recall To */		{ false,    false,      false,      false,      false,      false,          false,          false,      false,              false,              false,              false,      false,      false,          false,                      false,          false,              false,      false },
/* Gate From */		{ false,    false,      false,      false,      false,      false,          false,          false,      false,              false,              false,              false,      false,      false,          false,                      false,          false,              false,      false },
/* Gate To */		{ false,    false,      false,      false,      false,      false,          false,          false,      false,              false,              false,              false,      false,      false,          false,                      false,          false,              false,      false },
/* Mark In */		{ false,    false,      false,      false,      false,      false,          false,          false,      false,              false,              false,              false,      false,      false,          false,                      false,          false,              false,      false },
/* Tele From */		{ true,     true,       true,       true,       true,       true,           true,           true,       false,              true,               true,               true,       false,      true,           true,                       true,           true,               false,      true },
/* Tele To */		{ true,     true,       true,       true,       true,       true,           true,           true,       false,              true,               false,              false,      false,      true,           true,                       true,           true,               false,      false },
		};

		public static void SendInvalidMessage(Mobile caster, TravelCheckType type)
		{
			if (type is TravelCheckType.RecallTo or TravelCheckType.GateTo)
			{
				caster.SendLocalizedMessage(1019004); // You are not allowed to travel there.
			}
			else if (type == TravelCheckType.TeleportTo)
			{
				caster.SendLocalizedMessage(501035); // You cannot teleport from here to the destination.
			}
			else
			{
				caster.SendLocalizedMessage(501802); // Thy spell doth not appear to work...
			}
		}

		public static bool CheckTravel(Mobile caster, TravelCheckType type)
		{
			return CheckTravel(caster, caster.Map, caster.Location, type);
		}

		public static bool CheckTravel(Map map, Point3D loc, TravelCheckType type)
		{
			return CheckTravel(null, map, loc, type);
		}

		private static Mobile m_TravelCaster;
		private static TravelCheckType m_TravelType;

		public static bool CheckTravel(Mobile caster, Map map, Point3D loc, TravelCheckType type)
		{
			if (IsInvalid(map, loc)) // null, internal, out of bounds
			{
				if (caster != null)
				{
					SendInvalidMessage(caster, type);
				}

				return false;
			}

			if (caster != null)
			{
				if (caster.AccessLevel < AccessLevel.Counselor && caster.Region.IsPartOf<Jail>())
				{
					caster.SendLocalizedMessage(1114345); // You'll need a better jailbreak plan than that!
					return false;
				}

				if (type is TravelCheckType.RecallFrom or TravelCheckType.RecallTo or TravelCheckType.TeleportFrom or TravelCheckType.TeleportTo)
				{
					if (!Region.CanTransition(caster, loc, map))
					{
						SendInvalidMessage(caster, type);
						return false;
					}
				}

				// Always allow monsters to teleport
				if (caster is BaseCreature bc && type is TravelCheckType.TeleportTo or TravelCheckType.TeleportFrom)
				{
					if (!bc.Controlled && !bc.Summoned)
					{
						return true;
					}
				}
			}

			m_TravelCaster = caster;
			m_TravelType = type;

			var v = (int)type;
			var isValid = true;

			for (var i = 0; isValid && i < m_Validators.Length; ++i)
			{
				isValid = m_Rules[v, i] || !m_Validators[i](map, loc);
			}

			if (!isValid && caster != null)
			{
				SendInvalidMessage(caster, type);
			}

			return isValid;
		}

		public static bool IsWindLoc(Point3D loc)
		{
			int x = loc.X, y = loc.Y;

			return x >= 5120 && y >= 0 && x < 5376 && y < 256;
		}

		public static bool IsFeluccaWind(Map map, Point3D loc)
		{
			if (map == Map.Felucca)
			{
				return IsWindLoc(loc);
			}

			return false;
		}

		public static bool IsTrammelWind(Map map, Point3D loc)
		{
			if (map == Map.Trammel)
			{
				return IsWindLoc(loc);
			}

			return false;
		}

		public static bool IsIlshenar(Map map, Point3D loc)
		{
			if (map == Map.Ilshenar)
			{
				return true;
			}

			return false;
		}

		public static bool IsSolenHiveLoc(Point3D loc)
		{
			int x = loc.X, y = loc.Y;

			return x >= 5640 && y >= 1776 && x < 5935 && y < 2039;
		}

		public static bool IsTrammelSolenHive(Map map, Point3D loc)
		{
			if (map == Map.Trammel)
			{
				return IsSolenHiveLoc(loc);
			}

			return false;
		}

		public static bool IsFeluccaSolenHive(Map map, Point3D loc)
		{
			if (map == Map.Felucca)
			{
				return IsSolenHiveLoc(loc);
			}

			return false;
		}

		public static bool IsFeluccaT2A(Map map, Point3D loc)
		{
			if (map == Map.Felucca)
			{
				int x = loc.X, y = loc.Y;

				return x >= 5120 && y >= 2304 && x < 6144 && y < 4096;
			}

			return false;
		}

		public static bool IsAnyT2A(Map map, Point3D loc)
		{
			if (map == Map.Trammel || map == Map.Felucca)
			{
				int x = loc.X, y = loc.Y;

				return x >= 5120 && y >= 2304 && x < 6144 && y < 4096;
			}

			return false;
		}

		public static bool IsFeluccaDungeon(Map map, Point3D loc)
		{
			if (map == Map.Felucca)
			{
				var reg = Region.Find(loc, map);

				return reg.IsPartOf<DungeonRegion>();
			}

			return false;
		}

		public static bool IsKhaldun(Map map, Point3D loc)
		{
			var reg = Region.Find(loc, map);

			return reg.Name == "Khaldun";
		}

		public static bool IsCrystalCave(Map map, Point3D loc)
		{
			if (map == Map.Malas && loc.Z < -80)
			{
				int x = loc.X, y = loc.Y;

				return (x >= 1182 && y >= 437 && x < 1211 && y < 470)
					|| (x >= 1156 && y >= 470 && x < 1211 && y < 503)
					|| (x >= 1176 && y >= 503 && x < 1208 && y < 509)
					|| (x >= 1188 && y >= 509 && x < 1201 && y < 513);
			}

			return false;
		}

		public static bool IsSafeZone(Map map, Point3D loc)
		{
			var reg = Region.Find(loc, map);

			#region Duels
			if (reg.IsPartOf<Engines.ConPVP.SafeZone>())
			{
				if (m_TravelType is TravelCheckType.TeleportTo or TravelCheckType.TeleportFrom)
				{
					if (m_TravelCaster is PlayerMobile pm && pm.DuelPlayer != null && !pm.DuelPlayer.Eliminated)
					{
						return false;
					}
				}

				return true;
			}
			#endregion

			return false;
		}

		public static bool IsFactionStronghold(Map map, Point3D loc)
		{
			var reg = Region.Find(loc, map);
			/*
			// Teleporting is allowed, but only for faction members
			if (!Core.AOS && m_TravelCaster != null && m_TravelType is TravelCheckType.TeleportTo or TravelCheckType.TeleportFrom)
			{
				if (Factions.Faction.Find(m_TravelCaster, true, true) != null)
				{
					return false;
				}
			}
			*/
			return reg.IsPartOf<Factions.StrongholdRegion>();
		}

		public static bool IsChampionSpawn(Map map, Point3D loc)
		{
			var reg = Region.Find(loc, map);

			return reg.IsPartOf<Engines.CannedEvil.ChampionSpawnRegion>();
		}

		public static bool IsDoomFerry(Map map, Point3D loc)
		{
			if (map == Map.Malas)
			{
				int x = loc.X, y = loc.Y;

				if (x >= 426 && y >= 314 && x <= 430 && y <= 331)
				{
					return true;
				}

				if (x >= 406 && y >= 247 && x <= 410 && y <= 264)
				{
					return true;
				}

				return false;
			}

			return false;
		}

		public static bool IsTokunoDungeon(Map map, Point3D loc)
		{
			//The tokuno dungeons are really inside malas
			if (map == Map.Malas)
			{
				int x = loc.X, y = loc.Y;

				var r1 = x >= 0 && y >= 0 && x <= 128 && y <= 128;
				var r2 = x >= 45 && y >= 320 && x < 195 && y < 710;

				return r1 || r2;
			}

			return false;
		}

		public static bool IsDoomGauntlet(Map map, Point3D loc)
		{
			if (map == Map.Malas)
			{
				int x = loc.X - 256, y = loc.Y - 304;

				return x >= 0 && y >= 0 && x < 256 && y < 256;
			}

			return false;
		}

		public static bool IsLampRoom(Map map, Point3D loc)
		{
			if (map == Map.Malas)
			{
				int x = loc.X, y = loc.Y;

				return x >= 465 && y >= 92 && x < 474 && y < 102;
			}

			return false;
		}

		public static bool IsGuardianRoom(Map map, Point3D loc)
		{
			if (map == Map.Malas)
			{
				int x = loc.X, y = loc.Y;

				return x >= 356 && y >= 5 && x < 375 && y < 25;
			}

			return false;
		}

		public static bool IsHeartwood(Map map, Point3D loc)
		{
			if (map == Map.Trammel || map == Map.Felucca)
			{
				int x = loc.X, y = loc.Y;

				return x >= 6911 && y >= 254 && x < 7167 && y < 511;
			}

			return false;
		}

		public static bool IsMLDungeon(Map map, Point3D loc)
		{
			return ValorSpawner.IsValorRegion(Region.Find(loc, map));
		}

		public static bool IsInvalid(Map map, Point3D loc)
		{
			if (map == null || map == Map.Internal)
			{
				return true;
			}

			int x = loc.X, y = loc.Y;

			return x < 0 || y < 0 || x >= map.Width || y >= map.Height;
		}

		//towns
		public static bool CheckTown(ISpell spell, GuardedRegion region)
		{
			if ((spell.Caster == null || spell.Caster.AccessLevel < AccessLevel.GameMaster) && !region.Disabled && !spell.OnCastInTown(region))
			{
				spell.Caster?.SendMessage($"You cannot cast {spell.Name} in town!");

				return false;
			}

			return true;
		}

        public static bool CheckTown(ISpell spell, IPoint3D loc)
		{
			if (Region.Find(loc, spell.Caster?.Map) is GuardedRegion reg)
			{
				return CheckTown(spell, reg);
			}

			return true;
		}

		//magic reflection
		public static bool CheckReflect(int circle, Mobile caster, ref Mobile target)
		{
			return CheckReflect(circle, ref caster, ref target);
		}

		public static bool CheckReflect(int circle, ref Mobile caster, ref Mobile target)
		{
			if (target.MagicDamageAbsorb > 0)
			{
				++circle;

				target.MagicDamageAbsorb -= circle;

				// This order isn't very intuitive, but you have to nullify reflect before target gets switched

				var reflect = target.MagicDamageAbsorb >= 0;

				if (target is BaseCreature c)
				{
					c.CheckReflect(caster, ref reflect);
				}

				if (target.MagicDamageAbsorb <= 0)
				{
					target.MagicDamageAbsorb = 0;

					DefensiveState.Nullify(target);
				}

				if (reflect)
				{
					target.FixedEffect(0x37B9, 10, 5);

					(target, caster) = (caster, target);
				}

				return reflect;
			}
			
			if (target is BaseCreature tc)
			{
				var reflect = false;

				tc.CheckReflect(caster, ref reflect);

				if (reflect)
				{
					target.FixedEffect(0x37B9, 10, 5);

					(target, caster) = (caster, target);
				}

				return reflect;
			}

			return false;
		}

		public static void Damage(Spell spell, Mobile target, double damage)
		{
			var ts = GetDamageDelayForSpell(spell);

			Damage(spell, ts, target, spell.Caster, damage);
		}

		public static void Damage(TimeSpan delay, Mobile target, double damage)
		{
			Damage(delay, target, null, damage);
		}

		public static void Damage(TimeSpan delay, Mobile target, Mobile from, double damage)
		{
			Damage(null, delay, target, from, damage);
		}

		public static void Damage(Spell spell, TimeSpan delay, Mobile target, Mobile from, double damage)
		{
			var iDamage = (int)damage;

			if (delay <= TimeSpan.Zero)
			{
				if (from is BaseCreature fc)
				{
					fc.AlterSpellDamageTo(target, ref iDamage);
				}

				if (target is BaseCreature tc)
				{
					tc.AlterSpellDamageFrom(from, ref iDamage);
				}

				target.Damage(iDamage, from);
			}
			else
			{
				var t = new SpellDamageTimer(spell, target, from, iDamage, delay);

				t.Start();
			}

			if (target is BaseCreature c && from != null && delay == TimeSpan.Zero)
			{
				c.OnHarmfulSpell(from);
				c.OnDamagedBySpell(from);
			}
		}

		public static void Damage(Spell spell, Mobile target, double damage, int phys, int fire, int cold, int pois, int nrgy)
		{
			var ts = GetDamageDelayForSpell(spell);

			Damage(spell, ts, target, spell.Caster, damage, phys, fire, cold, pois, nrgy, DFAlgorithm.Standard);
		}

		public static void Damage(Spell spell, Mobile target, double damage, int phys, int fire, int cold, int pois, int nrgy, DFAlgorithm dfa)
		{
			var ts = GetDamageDelayForSpell(spell);

			Damage(spell, ts, target, spell.Caster, damage, phys, fire, cold, pois, nrgy, dfa);
		}

		public static void Damage(TimeSpan delay, Mobile target, double damage, int phys, int fire, int cold, int pois, int nrgy)
		{
			Damage(delay, target, null, damage, phys, fire, cold, pois, nrgy);
		}

		public static void Damage(TimeSpan delay, Mobile target, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy)
		{
			Damage(delay, target, from, damage, phys, fire, cold, pois, nrgy, DFAlgorithm.Standard);
		}

		public static void Damage(TimeSpan delay, Mobile target, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy, DFAlgorithm dfa)
		{
			Damage(null, delay, target, from, damage, phys, fire, cold, pois, nrgy, dfa);
		}

		public static void Damage(Spell spell, TimeSpan delay, Mobile target, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy, DFAlgorithm dfa)
		{
			var iDamage = (int)damage;

			if (delay <= TimeSpan.Zero)
			{
				if (from is BaseCreature fc)
				{
					fc.AlterSpellDamageTo(target, ref iDamage);
				}

				if (target is BaseCreature tc)
				{
					tc.AlterSpellDamageFrom(from, ref iDamage);
				}

				WeightOverloading.DFA = dfa;

				var damageGiven = AOS.Damage(target, from, iDamage, phys, fire, cold, pois, nrgy);

				if (from != null) // sanity check
				{
					DoLeech(damageGiven, from, target);
				}

				WeightOverloading.DFA = DFAlgorithm.Standard;
			}
			else
			{
				var t = new SpellDamageTimerAOS(spell, target, from, iDamage, phys, fire, cold, pois, nrgy, delay, dfa);

				t.Start();
			}

			if (target is BaseCreature c && from != null && delay == TimeSpan.Zero)
			{
				c.OnHarmfulSpell(from);
				c.OnDamagedBySpell(from);
			}
		}

		public static void DoLeech(int damageGiven, Mobile from, Mobile target)
		{
			var context = TransformationSpellHelper.GetContext(from);

			if (context != null) /* cleanup */
			{
				if (context.Type == typeof(WraithFormSpell))
				{
					var wraithLeech = 5 + (int)(15 * from.Skills.SpiritSpeak.Value / 100); // Wraith form gives 5-20% mana leech
					var manaLeech = AOS.Scale(damageGiven, wraithLeech);

					if (manaLeech != 0)
					{
						from.Mana += manaLeech;

						from.PlaySound(0x44D);
					}
				}
				else if (context.Type == typeof(VampiricEmbraceSpell))
				{
					from.Hits += AOS.Scale(damageGiven, 20);

					from.PlaySound(0x44D);
				}
			}
		}

		public static void Heal(int amount, Mobile target, Mobile from)
		{
			Heal(amount, target, from, true);
		}

		public static void Heal(int amount, Mobile target, Mobile from, bool message)
		{
			//TODO: All Healing *spells* go through ArcaneEmpowerment
			target.Heal(amount, from, message);
		}

		private class SpellDamageTimer : Timer
		{
			private readonly Mobile m_Target, m_From;
			private readonly Spell m_Spell;

			private int m_Damage;

			public SpellDamageTimer(Spell s, Mobile target, Mobile from, int damage, TimeSpan delay)
				: base(delay)
			{
				m_Target = target;
				m_From = from;
				m_Damage = damage;
				m_Spell = s;

				if (m_Spell != null && m_Spell.DelayedDamage && !m_Spell.DelayedDamageStacking)
				{
					m_Spell.StartDelayedDamageContext(target, this);
				}

				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				if (m_From is BaseCreature fc)
				{
					fc.AlterSpellDamageTo(m_Target, ref m_Damage);
				}

				if (m_Target is BaseCreature tc)
				{
					tc.AlterSpellDamageFrom(m_From, ref m_Damage);
				}

				m_Target.Damage(m_Damage);

				m_Spell?.RemoveDelayedDamageContext(m_Target);
			}
		}

		private class SpellDamageTimerAOS : Timer
		{
			private readonly Mobile m_Target, m_From;
			private int m_Damage;
			private readonly int m_Phys, m_Fire, m_Cold, m_Pois, m_Nrgy;
			private readonly DFAlgorithm m_DFA;
			private readonly Spell m_Spell;

			public SpellDamageTimerAOS(Spell s, Mobile target, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, TimeSpan delay, DFAlgorithm dfa)
				: base(delay)
			{
				m_Target = target;
				m_From = from;
				m_Damage = damage;
				m_Phys = phys;
				m_Fire = fire;
				m_Cold = cold;
				m_Pois = pois;
				m_Nrgy = nrgy;
				m_DFA = dfa;
				m_Spell = s;

				if (m_Spell != null && m_Spell.DelayedDamage && !m_Spell.DelayedDamageStacking)
				{
					m_Spell.StartDelayedDamageContext(target, this);
				}

				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				if (m_From is BaseCreature fc && m_Target != null)
				{
					fc.AlterSpellDamageTo(m_Target, ref m_Damage);
				}

				if (m_Target is BaseCreature tc && m_From != null)
				{
					tc.AlterSpellDamageFrom(m_From, ref m_Damage);
				}

				WeightOverloading.DFA = m_DFA;

				var damageGiven = AOS.Damage(m_Target, m_From, m_Damage, m_Phys, m_Fire, m_Cold, m_Pois, m_Nrgy);

				if (m_From != null) // sanity check
				{
					DoLeech(damageGiven, m_From, m_Target);
				}

				WeightOverloading.DFA = DFAlgorithm.Standard;

				if (m_Target is BaseCreature c && m_From != null)
				{
					c.OnHarmfulSpell(m_From);
					c.OnDamagedBySpell(m_From);
				}

				m_Spell?.RemoveDelayedDamageContext(m_Target);
			}
		}
	}

	/// Travel Spell Checks
	public enum TravelCheckType
	{
		RecallFrom,
		RecallTo,
		GateFrom,
		GateTo,
		Mark,
		TeleportFrom,
		TeleportTo
	}

	/// Transformation Spell
	public class TransformationSpellHelper
	{
		#region Context Stuff
		private static readonly Dictionary<Mobile, TransformContext> m_Table = new();

		public static void AddContext(Mobile m, TransformContext context)
		{
			if (context != null)
			{
				m_Table[m] = context;
			}
		}

		public static void RemoveContext<T>(Mobile m, bool resetGraphics) where T : ITransformationSpell
		{
			if (m_Table.TryGetValue(m, out var context) && context.Spell is T)
			{
				RemoveContext(m, context, resetGraphics);
			}
		}

		public static void RemoveContext(Mobile m, bool resetGraphics)
		{
			if (m_Table.TryGetValue(m, out var context))
			{
				RemoveContext(m, context, resetGraphics);
			}
		}

		public static void RemoveContext(Mobile m, TransformContext context, bool resetGraphics)
		{
			if (m_Table.Remove(m))
			{
				var mods = context.Mods;

				for (var i = 0; i < mods.Count; ++i)
				{
					m.RemoveResistanceMod(mods[i]);
				}

				if (resetGraphics)
				{
					m.HueMod = -1;
					m.BodyMod = 0;
				}

				context.Timer.Stop();
				context.Spell.RemoveEffect(m);
			}
		}

		public static TransformContext GetContext(Mobile m)
		{
			_ = m_Table.TryGetValue(m, out var context);

			return context;
		}

		public static bool UnderTransformation(Mobile m)
		{
			return m_Table.ContainsKey(m);
		}

		public static bool UnderTransformation(Mobile m, Type type)
		{
			_ = m_Table.TryGetValue(m, out var context);

			return context?.Type == type;
		}
		#endregion

		public static bool CheckCast(Mobile caster, Spell spell)
		{
			if (caster.Flying)
			{
				caster.SendLocalizedMessage(1113415); // You cannot use this ability while flying.
				return false;
			}

			if (Sigil.ExistsOn(caster))
			{
				caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
				return false;
			}

			if (PolymorphSpell.IsPolymorphed(caster))
			{
				caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
				return false;
			}

			if (SleepSpell.IsUnderSleepEffects(caster))
			{
				caster.SendMessage("You can't do that while fatigued.");
				return false;
			}

			return true;
		}

		public static bool OnCast(Mobile caster, Spell spell)
		{
			if (spell is not ITransformationSpell transformSpell)
			{
				return false;
			}

			if (caster.Flying)
			{
				caster.SendLocalizedMessage(1113415); // You cannot use this ability while flying.
				return false;
			}

			if (Sigil.ExistsOn(caster))
			{
				caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
				return false;
			}

			if (DisguiseTimers.IsDisguised(caster))
			{
				caster.SendLocalizedMessage(1061631); // You can't do that while disguised.
				return false;
			}

			if (PolymorphSpell.IsPolymorphed(caster))
			{
				caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
				return false;
			}

			if (AnimalFormSpell.UnderTransformation(caster))
			{
				caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
				return false;
			}

			if (IncognitoSpell.IsIncognito(caster) || (caster.IsBodyMod && GetContext(caster) == null))
			{
				spell.DoFizzle();
				return false;
			}

			if (!spell.CheckSequence())
			{
				return false;
			}

			var context = GetContext(caster);
			var ourType = spell.GetType();

			var wasTransformed = context != null;
			var ourTransform = wasTransformed && context.Type == ourType;

			if (wasTransformed)
			{
				RemoveContext(caster, context, ourTransform);

				if (ourTransform)
				{
					caster.PlaySound(0xFA);
					caster.FixedParticles(0x3728, 1, 13, 5042, EffectLayer.Waist);
				}
			}

			if (!ourTransform)
			{
				var mods = new List<ResistanceMod>();

				if (transformSpell.PhysResistOffset != 0)
				{
					mods.Add(new ResistanceMod(ResistanceType.Physical, transformSpell.PhysResistOffset));
				}

				if (transformSpell.FireResistOffset != 0)
				{
					mods.Add(new ResistanceMod(ResistanceType.Fire, transformSpell.FireResistOffset));
				}

				if (transformSpell.ColdResistOffset != 0)
				{
					mods.Add(new ResistanceMod(ResistanceType.Cold, transformSpell.ColdResistOffset));
				}

				if (transformSpell.PoisResistOffset != 0)
				{
					mods.Add(new ResistanceMod(ResistanceType.Poison, transformSpell.PoisResistOffset));
				}

				if (transformSpell.NrgyResistOffset != 0)
				{
					mods.Add(new ResistanceMod(ResistanceType.Energy, transformSpell.NrgyResistOffset));
				}

				if (!transformSpell.Body.IsHuman)
				{
					var mt = caster.Mount;

					if (mt != null)
					{
						mt.Rider = null;
					}
				}

				caster.BodyMod = transformSpell.Body;
				caster.HueMod = transformSpell.Hue;

				for (var i = 0; i < mods.Count; ++i)
				{
					caster.AddResistanceMod(mods[i]);
				}

				transformSpell.DoEffect(caster);

				Timer timer = new TransformTimer(caster, transformSpell);

				timer.Start();

				AddContext(caster, new TransformContext(timer, mods, ourType, transformSpell));
				return true;
			}

			return false;
		}
	}

	public interface ITransformationSpell : ISpell
	{
		Body Body { get; }
		int Hue { get; }

		int PhysResistOffset { get; }
		int FireResistOffset { get; }
		int ColdResistOffset { get; }
		int PoisResistOffset { get; }
		int NrgyResistOffset { get; }

		double TickRate { get; }
		void OnTick(Mobile m);

		void DoEffect(Mobile m);
		void RemoveEffect(Mobile m);
	}

	public class TransformContext
	{
		public Timer Timer { get; }
		public List<ResistanceMod> Mods { get; }
		public Type Type { get; }
		public ITransformationSpell Spell { get; }

		public TransformContext(Timer timer, List<ResistanceMod> mods, Type type, ITransformationSpell spell)
		{
			Timer = timer;
			Mods = mods;
			Type = type;
			Spell = spell;
		}
	}

	public class TransformTimer : Timer
	{
		private readonly Mobile m_Mobile;
		private readonly ITransformationSpell m_Spell;

		public TransformTimer(Mobile from, ITransformationSpell spell)
			: base(TimeSpan.FromSeconds(spell.TickRate), TimeSpan.FromSeconds(spell.TickRate))
		{
			m_Mobile = from;
			m_Spell = spell;

			Priority = TimerPriority.TwoFiftyMS;
		}

		protected override void OnTick()
		{
			if (m_Mobile.Deleted || !m_Mobile.Alive || m_Mobile.Body != m_Spell.Body || m_Mobile.Hue != m_Spell.Hue)
			{
				TransformationSpellHelper.RemoveContext(m_Mobile, true);
				Stop();
			}
			else
			{
				m_Spell.OnTick(m_Mobile);
			}
		}
	}

	/// Summoned Creatures
	public sealed class UnsummonTimer : Timer
	{
		private readonly BaseCreature m_Creature;

		public UnsummonTimer(BaseCreature creature, TimeSpan delay)
			: base(delay)
		{
			m_Creature = creature;
		}

		protected override void OnTick()
		{
			m_Creature?.Delete();
		}
	}

	/// Dispel Spells
	[AttributeUsage(AttributeTargets.Class)]
	public class DispellableAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class DispellableFieldAttribute : Attribute
	{
	}

	/// Bushido | Ninjitsu
	public abstract class SpecialMove : ISpecialMove
	{
		public abstract SpellName ID { get; }

		private SpellInfo m_Info;

		public SpellInfo Info => m_Info ??= SpellRegistry.GetInfo(ID) ?? SpellInfo.CreateInvalid();

		public SpellSchool School => Info.School;

		public string Name => Info.Name;
		public string Desc => Info.Desc;

		public int BaseMana => Info.Mana;

		public double RequiredSkill => Info.Skill;

		public virtual SkillName MoveSkill => SkillName.Bushido;

		public virtual TextDefinition AbilityMessage => 0;

		public virtual bool BlockedByAnimalForm => true;
		public virtual bool DelayedContext => false;

		public virtual int GetAccuracyBonus(Mobile attacker)
		{
			return 0;
		}

		public virtual double GetDamageScalar(Mobile attacker, Mobile defender)
		{
			return 1.0;
		}

		// Called before swinging, to make sure the accuracy scalar is to be computed.
		public virtual bool OnBeforeSwing(Mobile attacker, Mobile defender)
		{
			return true;
		}

		// Called when a hit connects, but before damage is calculated.
		public virtual bool OnBeforeDamage(Mobile attacker, Mobile defender)
		{
			return true;
		}

		// Called as soon as the ability is used.
		public virtual void OnUse(Mobile from)
		{
		}

		// Called when a hit connects, at the end of the weapon.OnHit() method.
		public virtual void OnHit(Mobile attacker, Mobile defender, int damage)
		{
		}

		// Called when a hit misses.
		public virtual void OnMiss(Mobile attacker, Mobile defender)
		{
		}

		// Called when the move is cleared.
		public virtual void OnClearMove(Mobile from)
		{
		}

		public virtual bool IgnoreArmor(Mobile attacker)
		{
			return false;
		}

		public virtual double GetPropertyBonus(Mobile attacker)
		{
			return 1.0;
		}

		public virtual bool CheckSkills(Mobile m)
		{
			if (m.Skills[MoveSkill].Value < RequiredSkill)
			{
				m.SendLocalizedMessage(1063013, $"{RequiredSkill:F1}\t{MoveSkill}"); // You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
				return false;
			}

			return true;
		}

		public virtual int ScaleMana(Mobile m, int mana)
		{
			var scalar = 1.0;

			if (!MindRotSpell.GetMindRotScalar(m, ref scalar))
			{
				scalar = 1.0;
			}

			// Lower Mana Cost = 40%
			var lmc = Math.Min(AosAttributes.GetValue(m, AosAttribute.LowerManaCost), 40);

			scalar -= (double)lmc / 100;

			var total = (int)(mana * scalar);

			if (m.Skills[MoveSkill].Value < 50.0 && GetContext(m) != null)
			{
				total *= 2;
			}

			return total;
		}

		public virtual bool CheckMana(Mobile from, bool consume)
		{
			var mana = ScaleMana(from, BaseMana);

			if (from.Mana < mana)
			{
				from.SendLocalizedMessage(1060181, mana.ToString()); // You need ~1_MANA_REQUIREMENT~ mana to perform that attack
				return false;
			}

			if (consume)
			{
				if (!DelayedContext)
				{
					SetContext(from);
				}

				from.Mana -= mana;
			}

			return true;
		}

		public virtual void SetContext(Mobile from)
		{
			if (GetContext(from) == null)
			{
				if (DelayedContext || from.Skills[MoveSkill].Value < 50.0)
				{
					var timer = new SpecialMoveTimer(from);

					timer.Start();

					AddContext(from, new SpecialMoveContext(timer, GetType()));
				}
			}
		}

		public virtual bool Validate(Mobile from)
		{
			if (!from.Player)
			{
				return true;
			}

			if (HonorableExecutionAbility.IsUnderPenalty(from))
			{
				from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
				return false;
			}

			if (AnimalFormSpell.UnderTransformation(from))
			{
				from.SendLocalizedMessage(1063024); // You cannot perform this special move right now.
				return false;
			}

			#region Dueling
			string option = null;

			if (this is BackstabAbility)
			{
				option = "Backstab";
			}
			else if (this is DeathStrikeAbility)
			{
				option = "Death Strike";
			}
			else if (this is FocusAttackAbility)
			{
				option = "Focus Attack";
			}
			else if (this is KiAttackAbility)
			{
				option = "Ki Attack";
			}
			else if (this is SurpriseAttackAbility)
			{
				option = "Surprise Attack";
			}
			else if (this is HonorableExecutionAbility)
			{
				option = "Honorable Execution";
			}
			else if (this is LightningStrikeAbility)
			{
				option = "Lightning Strike";
			}
			else if (this is MomentumStrikeAbility)
			{
				option = "Momentum Strike";
			}

			if (option != null && !Engines.ConPVP.DuelContext.AllowSpecialMove(from, option, this))
			{
				return false;
			}
			#endregion

			return CheckSkills(from) && CheckMana(from, false);
		}

		public virtual void CheckGain(Mobile m)
		{
			_ = m.CheckSkill(MoveSkill, RequiredSkill, RequiredSkill + 37.5);
		}

		public static Dictionary<Mobile, ISpecialMove> Table { get; } = new();

		public static void ClearAllMoves(Mobile m)
		{
			foreach (var id in SpellRegistry.SpecialIds)
			{
				if (id != SpellName.Invalid)
				{
					_ = m.Send(new ToggleSpecialAbility((int)id + 1, false));
				}
			}
		}

		public virtual bool ValidatesDuringHit => true;

		public static ISpecialMove GetCurrentMove(Mobile m)
		{
			if (m == null)
			{
				return null;
			}

			if (!Core.SE)
			{
				ClearCurrentMove(m);
				return null;
			}

			_ = Table.TryGetValue(m, out var move);

			if (move != null && move.ValidatesDuringHit && !move.Validate(m))
			{
				ClearCurrentMove(m);
				return null;
			}

			return move;
		}

		public static bool SetCurrentMove(Mobile m, ISpecialMove move)
		{
			if (!Core.SE)
			{
				ClearCurrentMove(m);
				return false;
			}

			if (move != null && !move.Validate(m))
			{
				ClearCurrentMove(m);
				return false;
			}

			var sameMove = move == GetCurrentMove(m);

			ClearCurrentMove(m);

			if (sameMove)
			{
				return true;
			}

			if (move != null)
			{
				WeaponAbility.ClearCurrentAbility(m);

				Table[m] = move;

				move.OnUse(m);

				var moveID = SpellRegistry.GetID(move);

				if (moveID > 0)
				{
					_ = m.Send(new ToggleSpecialAbility((int)moveID + 1, true));
				}

				TextDefinition.SendMessageTo(m, move.AbilityMessage);
			}

			return true;
		}

		public static void ClearCurrentMove(Mobile m)
		{
			_ = Table.TryGetValue(m, out var move);

			if (move != null)
			{
				move.OnClearMove(m);

				var moveID = SpellRegistry.GetID(move);

				if (moveID > 0)
				{
					_ = m.Send(new ToggleSpecialAbility((int)moveID + 1, false));
				}
			}

			_ = Table.Remove(m);
		}

		public SpecialMove()
		{
		}

		private static readonly Dictionary<Mobile, SpecialMoveContext> m_PlayersTable = new();

		private static void AddContext(Mobile m, SpecialMoveContext context)
		{
			m_PlayersTable[m] = context;
		}

		private static void RemoveContext(Mobile m)
		{
			var context = GetContext(m);

			if (context != null)
			{
				_ = m_PlayersTable.Remove(m);

				context.Timer.Stop();
			}
		}

		private static SpecialMoveContext GetContext(Mobile m)
		{
			_ = m_PlayersTable.TryGetValue(m, out var context);

			return context;
		}

		public static bool GetContext(Mobile m, Type type)
		{
			var context = GetContext(m);

			return context?.Type == type;
		}

		private class SpecialMoveTimer : Timer
		{
			private readonly Mobile m_Mobile;

			public SpecialMoveTimer(Mobile from)
				: base(TimeSpan.FromSeconds(3.0))
			{
				m_Mobile = from;

				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				RemoveContext(m_Mobile);
			}
		}

		public class SpecialMoveContext
		{
			public Timer Timer { get; }
			public Type Type { get; }

			public SpecialMoveContext(Timer timer, Type type)
			{
				Timer = timer;
				Type = type;
			}
		}
	}

	/// Defensive State
	public static class DefensiveState
	{
		private static readonly Type m_Locker = typeof(DefensiveState);

		public static bool IsActive(Mobile from)
		{
			return from?.CanBeginAction(m_Locker) == false;
		}

		public static bool Activate(Mobile from)
		{
			return from?.BeginAction(m_Locker) == true;
		}

		public static void Nullify(Mobile from)
		{
			Nullify(from, TimeSpan.FromMinutes(1.0));
		}

		public static void Nullify(Mobile from, TimeSpan delay)
		{
			if (IsActive(from))
			{
				if (delay > TimeSpan.Zero)
				{
					Timer.DelayCall(delay, from.EndAction, m_Locker);
				}
				else
				{
					from.EndAction(m_Locker);
				}
			}
		}
	}
}