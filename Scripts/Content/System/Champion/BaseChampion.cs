using Server.Engines.CannedEvil;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Regions;

using System;
using System.Collections.Generic;

namespace Server.Engines.CannedEvil
{
	/// Champion Spawn
	public class ChampionSpawn : Item
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public int SpawnSzMod
		{
			get => (m_SPawnSzMod < 1 || m_SPawnSzMod > 12) ? 12 : m_SPawnSzMod;
			set => m_SPawnSzMod = (value < 1 || value > 12) ? 12 : value;
		}
		private int m_SPawnSzMod;

		private bool m_Active;
		private bool m_RandomizeType;
		private ChampionSpawnType m_Type;
		private List<Mobile> m_Creatures;
		private List<Item> m_RedSkulls;
		private List<Item> m_WhiteSkulls;
		private ChampionPlatform m_Platform;
		private ChampionAltar m_Altar;
		private int m_Kills;
		private Mobile m_Champion;

		//private int m_SpawnRange;
		private Rectangle2D m_SpawnArea;
		private ChampionSpawnRegion m_Region;

		private TimeSpan m_ExpireDelay;
		private DateTime m_ExpireTime;

		private TimeSpan m_RestartDelay;
		private DateTime m_RestartTime;

		private Timer m_Timer, m_RestartTimer;

		private IdolOfTheChampion m_Idol;

		private bool m_HasBeenAdvanced;
		private bool m_ConfinedRoaming;

		private Dictionary<Mobile, int> m_DamageEntries;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ConfinedRoaming
		{
			get => m_ConfinedRoaming;
			set => m_ConfinedRoaming = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasBeenAdvanced
		{
			get => m_HasBeenAdvanced;
			set => m_HasBeenAdvanced = value;
		}

		[Constructable]
		public ChampionSpawn() : base(0xBD2)
		{
			Movable = false;
			Visible = false;

			m_Creatures = new List<Mobile>();
			m_RedSkulls = new List<Item>();
			m_WhiteSkulls = new List<Item>();

			m_Platform = new ChampionPlatform(this);
			m_Altar = new ChampionAltar(this);
			m_Idol = new IdolOfTheChampion(this);

			m_ExpireDelay = TimeSpan.FromMinutes(10.0);
			m_RestartDelay = TimeSpan.FromMinutes(10.0);

			m_DamageEntries = new Dictionary<Mobile, int>();

			Timer.DelayCall(TimeSpan.Zero, SetInitialSpawnArea);
		}

		public void SetInitialSpawnArea()
		{
			//Previous default used to be 24;
			SpawnArea = new Rectangle2D(new Point2D(X - 24, Y - 24), new Point2D(X + 24, Y + 24));
		}

		public void UpdateRegion()
		{
			if (m_Region != null)
			{
				m_Region.Delete();
				m_Region = null;
			}

			if (!Deleted && Map != Map.Internal)
			{
				m_Region = new ChampionSpawnRegion(this);
				m_Region.Register();
			}
		}

		public void UpdateRegionArea()
		{
			if (m_Region != null)
			{
				m_Region.Area = new Poly3D[] { SpawnArea };
			}
			else
			{
				UpdateRegion();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RandomizeType
		{
			get => m_RandomizeType;
			set => m_RandomizeType = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Kills
		{
			get => m_Kills;
			set
			{
				m_Kills = value;

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Rectangle2D SpawnArea
		{
			get => m_SpawnArea;
			set
			{
				m_SpawnArea = value;

				UpdateRegionArea();
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan RestartDelay
		{
			get => m_RestartDelay;
			set => m_RestartDelay = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime RestartTime => m_RestartTime;

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan ExpireDelay
		{
			get => m_ExpireDelay;
			set => m_ExpireDelay = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime ExpireTime
		{
			get => m_ExpireTime;
			set => m_ExpireTime = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public ChampionSpawnType Type
		{
			get => m_Type;
			set
			{
				m_Type = value;

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{
			get => m_Active;
			set
			{
				if (value)
				{
					Start();
				}
				else
				{
					Stop();
				}

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Champion
		{
			get => m_Champion;
			set => m_Champion = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Level
		{
			get => m_RedSkulls.Count;
			set
			{
				for (var i = m_RedSkulls.Count - 1; i >= value; --i)
				{
					m_RedSkulls[i].Delete();
					m_RedSkulls.RemoveAt(i);
				}

				for (var i = m_RedSkulls.Count; i < value; ++i)
				{
					var skull = new Item(0x1854) {
						Hue = 0x26,
						Movable = false,
						Light = LightType.Circle150
					};

					skull.MoveToWorld(GetRedSkullLocation(i), Map);

					m_RedSkulls.Add(skull);
				}

				InvalidateProperties();
			}
		}

		public int MaxKills => (m_SPawnSzMod * (250 / 12)) - (Level * m_SPawnSzMod);

		public bool IsChampionSpawn(Mobile m)
		{
			return m_Creatures.Contains(m);
		}

		public void SetWhiteSkullCount(int val)
		{
			for (var i = m_WhiteSkulls.Count - 1; i >= val; --i)
			{
				m_WhiteSkulls[i].Delete();
				m_WhiteSkulls.RemoveAt(i);
			}

			for (var i = m_WhiteSkulls.Count; i < val; ++i)
			{
				var skull = new Item(0x1854) {
					Movable = false,
					Light = LightType.Circle150
				};

				skull.MoveToWorld(GetWhiteSkullLocation(i), Map);

				m_WhiteSkulls.Add(skull);

				Effects.PlaySound(skull.Location, skull.Map, 0x29);
				Effects.SendLocationEffect(new Point3D(skull.X + 1, skull.Y + 1, skull.Z), skull.Map, 0x3728, 10);
			}
		}

		public void Start()
		{
			if (m_Active || Deleted)
			{
				return;
			}

			m_Active = true;
			m_HasBeenAdvanced = false;

			if (m_Timer != null)
			{
				m_Timer.Stop();
			}

			m_Timer = new SliceTimer(this);
			m_Timer.Start();

			if (m_RestartTimer != null)
			{
				m_RestartTimer.Stop();
			}

			m_RestartTimer = null;

			if (m_Altar != null)
			{
				if (m_Champion != null)
				{
					m_Altar.Hue = 0x26;
				}
				else
				{
					m_Altar.Hue = 0;
				}
			}

			if (m_Platform != null)
			{
				m_Platform.Hue = 0x452;
			}
		}

		public void Stop()
		{
			if (!m_Active || Deleted)
			{
				return;
			}

			m_Active = false;
			m_HasBeenAdvanced = false;

			if (m_Timer != null)
			{
				m_Timer.Stop();
			}

			m_Timer = null;

			if (m_RestartTimer != null)
			{
				m_RestartTimer.Stop();
			}

			m_RestartTimer = null;

			if (m_Altar != null)
			{
				m_Altar.Hue = 0;
			}

			if (m_Platform != null)
			{
				m_Platform.Hue = 0x497;
			}
		}

		public void BeginRestart(TimeSpan ts)
		{
			if (m_RestartTimer != null)
			{
				m_RestartTimer.Stop();
			}

			m_RestartTime = DateTime.UtcNow + ts;

			m_RestartTimer = new RestartTimer(this, ts);
			m_RestartTimer.Start();
		}

		public void EndRestart()
		{
			if (RandomizeType)
			{
				switch (Utility.Random(5))
				{
					case 0: Type = ChampionSpawnType.VerminHorde; break;
					case 1: Type = ChampionSpawnType.UnholyTerror; break;
					case 2: Type = ChampionSpawnType.ColdBlood; break;
					case 3: Type = ChampionSpawnType.Abyss; break;
					case 4: Type = ChampionSpawnType.Arachnid; break;
				}
			}

			m_HasBeenAdvanced = false;

			Start();
		}

		#region Scroll of Transcendence
		private ScrollofTranscendence CreateRandomSoT(bool felucca)
		{
			var level = Utility.RandomMinMax(1, 5);

			if (felucca)
			{
				level += 5;
			}

			return ScrollofTranscendence.CreateRandom(level, level);
		}
		#endregion

		public static void GiveScrollTo(Mobile killer, ScrollOfPower scroll)
		{
			if (scroll == null || killer == null)   //sanity
			{
				return;
			}

			if (scroll is ScrollofTranscendence)
			{
				killer.SendLocalizedMessage(1094936); // You have received a Scroll of Transcendence!
			}
			else
			{
				killer.SendLocalizedMessage(1049524); // You have received a scroll of power!
			}

			if (killer.Alive)
			{
				killer.AddToBackpack(scroll);
			}
			else
			{
				if (killer.Corpse != null && !killer.Corpse.Deleted)
				{
					killer.Corpse.DropItem(scroll);
				}
				else
				{
					killer.AddToBackpack(scroll);
				}
			}

			// Justice reward
			var pm = (PlayerMobile)killer;
			for (var j = 0; j < pm.JusticeProtectors.Count; ++j)
			{
				var prot = pm.JusticeProtectors[j];

				if (prot.Map != killer.Map || prot.Murderer || prot.Criminal || !JusticeVirtue.CheckMapRegion(killer, prot))
				{
					continue;
				}

				var chance = 0;

				switch (VirtueHelper.GetLevel(prot, VirtueName.Justice))
				{
					case VirtueLevel.Seeker: chance = 60; break;
					case VirtueLevel.Follower: chance = 80; break;
					case VirtueLevel.Knight: chance = 100; break;
				}

				if (chance > Utility.Random(100))
				{
					try
					{
						prot.SendLocalizedMessage(1049368); // You have been rewarded for your dedication to Justice!

						var scrollDupe = Activator.CreateInstance(scroll.GetType()) as ScrollOfPower;

						if (scrollDupe != null)
						{
							scrollDupe.Skill = scroll.Skill;
							scrollDupe.Value = scroll.Value;
							prot.AddToBackpack(scrollDupe);
						}
					}
					catch { }
				}
			}
		}

		public void OnSlice()
		{
			if (!m_Active || Deleted)
			{
				return;
			}

			if (m_Champion != null)
			{
				if (m_Champion.Deleted)
				{
					RegisterDamageTo(m_Champion);

					if (m_Champion is BaseChampion)
					{
						AwardArtifact(((BaseChampion)m_Champion).GetArtifact());
					}

					m_DamageEntries.Clear();

					if (m_Platform != null)
					{
						m_Platform.Hue = 0x497;
					}

					if (m_Altar != null)
					{
						m_Altar.Hue = 0;

						if (!Core.ML || Map == Map.Felucca)
						{
							new StarRoomGate(true, m_Altar.Location, m_Altar.Map);
						}
					}

					m_Champion = null;
					Stop();

					BeginRestart(m_RestartDelay);
				}
			}
			else
			{
				var kills = m_Kills;

				for (var i = 0; i < m_Creatures.Count; ++i)
				{
					var m = m_Creatures[i];

					if (m.Deleted)
					{
						if (m.Corpse != null && !m.Corpse.Deleted)
						{
							((Corpse)m.Corpse).BeginDecay(TimeSpan.FromMinutes(1));
						}
						m_Creatures.RemoveAt(i);
						--i;
						++m_Kills;

						var killer = m.FindMostRecentDamager(false);

						RegisterDamageTo(m);

						if (killer is BaseCreature)
						{
							killer = ((BaseCreature)killer).GetMaster();
						}

						if (killer is PlayerMobile)
						{
							#region Scroll of Transcendence
							if (Core.ML)
							{
								if (Map == Map.Felucca)
								{
									if (Utility.RandomDouble() < 0.001)
									{
										var pm = (PlayerMobile)killer;
										double random = Utility.Random(49);

										if (random <= 24)
										{
											var SoTF = CreateRandomSoT(true);
											GiveScrollTo(pm, SoTF);
										}
										else
										{
											var PS = PowerScroll.CreateRandomNoCraft(5, 5);
											GiveScrollTo(pm, PS);
										}
									}
								}

								if (Map == Map.Ilshenar || Map == Map.Tokuno || Map == Map.Malas)
								{
									if (Utility.RandomDouble() < 0.0015)
									{
										killer.SendLocalizedMessage(1094936); // You have received a Scroll of Transcendence!
										var SoTT = CreateRandomSoT(false);
										killer.AddToBackpack(SoTT);
									}
								}
							}
							#endregion

							var mobSubLevel = GetSubLevelFor(m) + 1;

							if (mobSubLevel >= 0)
							{
								var gainedPath = false;

								var pointsToGain = mobSubLevel * 40;

								if (VirtueHelper.Award(killer, VirtueName.Valor, pointsToGain, ref gainedPath))
								{
									if (gainedPath)
									{
										m.SendLocalizedMessage(1054032); // You have gained a path in Valor!
									}
									else
									{
										m.SendLocalizedMessage(1054030); // You have gained in Valor!
									}

									//No delay on Valor gains
								}

								var info = ((PlayerMobile)killer).ChampionTitles;

								info.Award(m_Type, mobSubLevel);
							}
						}
					}
				}

				// Only really needed once.
				if (m_Kills > kills)
				{
					InvalidateProperties();
				}

				var n = m_Kills / (double)MaxKills;
				var p = (int)(n * 100);

				if (p >= 90)
				{
					AdvanceLevel();
				}
				else if (p > 0)
				{
					SetWhiteSkullCount(p / 20);
				}

				if (DateTime.UtcNow >= m_ExpireTime)
				{
					Expire();
				}

				Respawn();
			}
		}

		public void AdvanceLevel()
		{
			m_ExpireTime = DateTime.UtcNow + m_ExpireDelay;

			if (Level < 16)
			{
				m_Kills = 0;
				++Level;
				InvalidateProperties();
				SetWhiteSkullCount(0);

				if (m_Altar != null)
				{
					Effects.PlaySound(m_Altar.Location, m_Altar.Map, 0x29);
					Effects.SendLocationEffect(new Point3D(m_Altar.X + 1, m_Altar.Y + 1, m_Altar.Z), m_Altar.Map, 0x3728, 10);
				}
			}
			else
			{
				SpawnChampion();
			}
		}

		public void SpawnChampion()
		{
			if (m_Altar != null)
			{
				m_Altar.Hue = 0x26;
			}

			if (m_Platform != null)
			{
				m_Platform.Hue = 0x452;
			}

			m_Kills = 0;
			Level = 0;
			InvalidateProperties();
			SetWhiteSkullCount(0);

			try
			{
				m_Champion = Activator.CreateInstance(ChampionSpawnInfo.GetInfo(m_Type).Champion) as Mobile;
			}
			catch { }

			if (m_Champion != null)
			{
				m_Champion.MoveToWorld(new Point3D(X, Y, Z - 15), Map);
			}
		}

		public void Respawn()
		{
			if (!m_Active || Deleted || m_Champion != null)
			{
				return;
			}

			while (m_Creatures.Count < ((m_SPawnSzMod * (200 / 12))) - (GetSubLevel() * (m_SPawnSzMod * (40 / 12))))
			{
				var m = Spawn();

				if (m == null)
				{
					return;
				}

				var loc = GetSpawnLocation();

				// Allow creatures to turn into Paragons at Ilshenar champions.
				m.OnBeforeSpawn(loc, Map);

				m_Creatures.Add(m);
				m.MoveToWorld(loc, Map);

				if (m is BaseCreature)
				{
					var bc = m as BaseCreature;
					bc.Tamable = false;

					if (!m_ConfinedRoaming)
					{
						bc.Home = Location;
						bc.RangeHome = (int)(Math.Sqrt(m_SpawnArea.Width * m_SpawnArea.Width + m_SpawnArea.Height * m_SpawnArea.Height) / 2);
					}
					else
					{
						bc.Home = bc.Location;

						var xWall1 = new Point2D(m_SpawnArea.X, bc.Y);
						var xWall2 = new Point2D(m_SpawnArea.X + m_SpawnArea.Width, bc.Y);
						var yWall1 = new Point2D(bc.X, m_SpawnArea.Y);
						var yWall2 = new Point2D(bc.X, m_SpawnArea.Y + m_SpawnArea.Height);

						var minXDist = Math.Min(bc.GetDistanceToSqrt(xWall1), bc.GetDistanceToSqrt(xWall2));
						var minYDist = Math.Min(bc.GetDistanceToSqrt(yWall1), bc.GetDistanceToSqrt(yWall2));

						bc.RangeHome = (int)Math.Min(minXDist, minYDist);
					}
				}
			}
		}

		public Point3D GetSpawnLocation()
		{
			var map = Map;

			if (map == null)
			{
				return Location;
			}

			// Try 20 times to find a spawnable location.
			for (var i = 0; i < 20; i++)
			{
				/*
				int x = Location.X + (Utility.Random( (m_SpawnRange * 2) + 1 ) - m_SpawnRange);
				int y = Location.Y + (Utility.Random( (m_SpawnRange * 2) + 1 ) - m_SpawnRange);
				*/

				var x = Utility.Random(m_SpawnArea.X, m_SpawnArea.Width);
				var y = Utility.Random(m_SpawnArea.Y, m_SpawnArea.Height);

				var z = Map.GetAverageZ(x, y);

				if (Map.CanSpawnMobile(new Point2D(x, y), z))
				{
					return new Point3D(x, y, z);
				}

				/* try @ platform Z if map z fails */
				else if (Map.CanSpawnMobile(new Point2D(x, y), m_Platform.Location.Z))
				{
					return new Point3D(x, y, m_Platform.Location.Z);
				}
			}

			return Location;
		}

		private const int Level1 = 4;  // First spawn level from 0-4 red skulls
		private const int Level2 = 8;  // Second spawn level from 5-8 red skulls
		private const int Level3 = 12; // Third spawn level from 9-12 red skulls

		public int GetSubLevel()
		{
			var level = Level;

			if (level <= Level1)
			{
				return 0;
			}
			else if (level <= Level2)
			{
				return 1;
			}
			else if (level <= Level3)
			{
				return 2;
			}

			return 3;
		}

		public int GetSubLevelFor(Mobile m)
		{
			var types = ChampionSpawnInfo.GetInfo(m_Type).SpawnTypes;
			var t = m.GetType();

			for (var i = 0; i < types.GetLength(0); i++)
			{
				var individualTypes = types[i];

				for (var j = 0; j < individualTypes.Length; j++)
				{
					if (t == individualTypes[j])
					{
						return i;
					}
				}
			}

			return -1;
		}

		public Mobile Spawn()
		{
			var types = ChampionSpawnInfo.GetInfo(m_Type).SpawnTypes;

			var v = GetSubLevel();

			if (v >= 0 && v < types.Length)
			{
				return Spawn(types[v]);
			}

			return null;
		}

		public Mobile Spawn(params Type[] types)
		{
			try
			{
				return Activator.CreateInstance(types[Utility.Random(types.Length)]) as Mobile;
			}
			catch
			{
				return null;
			}
		}

		public void Expire()
		{
			m_Kills = 0;

			if (m_WhiteSkulls.Count == 0)
			{
				// They didn't even get 20%, go back a level

				if (Level > 0)
				{
					--Level;
				}

				InvalidateProperties();
			}
			else
			{
				SetWhiteSkullCount(0);
			}

			m_ExpireTime = DateTime.UtcNow + m_ExpireDelay;
		}

		public Point3D GetRedSkullLocation(int index)
		{
			int x, y;

			if (index < 5)
			{
				x = index - 2;
				y = -2;
			}
			else if (index < 9)
			{
				x = 2;
				y = index - 6;
			}
			else if (index < 13)
			{
				x = 10 - index;
				y = 2;
			}
			else
			{
				x = -2;
				y = 14 - index;
			}

			return new Point3D(X + x, Y + y, Z - 15);
		}

		public Point3D GetWhiteSkullLocation(int index)
		{
			int x, y;

			switch (index)
			{
				default:
				case 0: x = -1; y = -1; break;
				case 1: x = 1; y = -1; break;
				case 2: x = 1; y = 1; break;
				case 3: x = -1; y = 1; break;
			}

			return new Point3D(X + x, Y + y, Z - 15);
		}

		public override void AddNameProperty(ObjectPropertyList list)
		{
			list.Add("champion spawn");
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Active)
			{
				list.Add(1060742); // active
				list.Add(1060658, "Type\t{0}", m_Type); // ~1_val~: ~2_val~
				list.Add(1060659, "Level\t{0}", Level); // ~1_val~: ~2_val~
				list.Add(1060660, "Kills\t{0} of {1} ({2:F1}%)", m_Kills, MaxKills, 100.0 * ((double)m_Kills / MaxKills)); // ~1_val~: ~2_val~
																														   //list.Add( 1060661, "Spawn Range\t{0}", m_SpawnRange ); // ~1_val~: ~2_val~
			}
			else
			{
				list.Add(1060743); // inactive
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			if (m_Active)
			{
				LabelTo(from, "{0} (Active; Level: {1}; Kills: {2}/{3})", m_Type, Level, m_Kills, MaxKills);
			}
			else
			{
				LabelTo(from, "{0} (Inactive)", m_Type);
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.SendGump(new PropertiesGump(from, this));
		}

		public override void OnLocationChange(Point3D oldLoc)
		{
			if (Deleted)
			{
				return;
			}

			if (m_Platform != null)
			{
				m_Platform.Location = new Point3D(X, Y, Z - 20);
			}

			if (m_Altar != null)
			{
				m_Altar.Location = new Point3D(X, Y, Z - 15);
			}

			if (m_Idol != null)
			{
				m_Idol.Location = new Point3D(X, Y, Z - 15);
			}

			if (m_RedSkulls != null)
			{
				for (var i = 0; i < m_RedSkulls.Count; ++i)
				{
					m_RedSkulls[i].Location = GetRedSkullLocation(i);
				}
			}

			if (m_WhiteSkulls != null)
			{
				for (var i = 0; i < m_WhiteSkulls.Count; ++i)
				{
					m_WhiteSkulls[i].Location = GetWhiteSkullLocation(i);
				}
			}

			m_SpawnArea.X += Location.X - oldLoc.X;
			m_SpawnArea.Y += Location.Y - oldLoc.Y;

			UpdateRegionArea();
		}

		public override void OnMapChange(Map oldMap)
		{
			if (Deleted)
			{
				return;
			}

			if (m_Platform != null)
			{
				m_Platform.Map = Map;
			}

			if (m_Altar != null)
			{
				m_Altar.Map = Map;
			}

			if (m_Idol != null)
			{
				m_Idol.Map = Map;
			}

			if (m_RedSkulls != null)
			{
				for (var i = 0; i < m_RedSkulls.Count; ++i)
				{
					m_RedSkulls[i].Map = Map;
				}
			}

			if (m_WhiteSkulls != null)
			{
				for (var i = 0; i < m_WhiteSkulls.Count; ++i)
				{
					m_WhiteSkulls[i].Map = Map;
				}
			}

			UpdateRegion();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_Platform != null)
			{
				m_Platform.Delete();
			}

			if (m_Altar != null)
			{
				m_Altar.Delete();
			}

			if (m_Idol != null)
			{
				m_Idol.Delete();
			}

			if (m_RedSkulls != null)
			{
				for (var i = 0; i < m_RedSkulls.Count; ++i)
				{
					m_RedSkulls[i].Delete();
				}

				m_RedSkulls.Clear();
			}

			if (m_WhiteSkulls != null)
			{
				for (var i = 0; i < m_WhiteSkulls.Count; ++i)
				{
					m_WhiteSkulls[i].Delete();
				}

				m_WhiteSkulls.Clear();
			}

			if (m_Creatures != null)
			{
				for (var i = 0; i < m_Creatures.Count; ++i)
				{
					var mob = m_Creatures[i];

					if (!mob.Player)
					{
						mob.Delete();
					}
				}

				m_Creatures.Clear();
			}

			if (m_Champion != null && !m_Champion.Player)
			{
				m_Champion.Delete();
			}

			Stop();

			UpdateRegion();
		}

		public ChampionSpawn(Serial serial) : base(serial)
		{
		}

		public virtual void RegisterDamageTo(Mobile m)
		{
			if (m == null)
			{
				return;
			}

			foreach (var de in m.DamageEntries)
			{
				if (de.HasExpired)
				{
					continue;
				}

				var damager = de.Damager;

				var master = damager.GetDamageMaster(m);

				if (master != null)
				{
					damager = master;
				}

				RegisterDamage(damager, de.DamageGiven);
			}
		}

		public void RegisterDamage(Mobile from, int amount)
		{
			if (from == null || !from.Player)
			{
				return;
			}

			if (m_DamageEntries.ContainsKey(from))
			{
				m_DamageEntries[from] += amount;
			}
			else
			{
				m_DamageEntries.Add(from, amount);
			}
		}

		public void AwardArtifact(Item artifact)
		{
			if (artifact == null)
			{
				return;
			}

			var totalDamage = 0;

			var validEntries = new Dictionary<Mobile, int>();

			foreach (var kvp in m_DamageEntries)
			{
				if (IsEligible(kvp.Key, artifact))
				{
					validEntries.Add(kvp.Key, kvp.Value);
					totalDamage += kvp.Value;
				}
			}

			var randomDamage = Utility.RandomMinMax(1, totalDamage);

			totalDamage = 0;

			foreach (var kvp in validEntries)
			{
				totalDamage += kvp.Value;

				if (totalDamage >= randomDamage)
				{
					GiveArtifact(kvp.Key, artifact);
					return;
				}
			}

			artifact.Delete();
		}

		public void GiveArtifact(Mobile to, Item artifact)
		{
			if (to == null || artifact == null)
			{
				return;
			}

			var pack = to.Backpack;

			if (pack == null || !pack.TryDropItem(to, artifact, false))
			{
				artifact.Delete();
			}
			else
			{
				to.SendLocalizedMessage(1062317); // For your valor in combating the fallen beast, a special artifact has been bestowed on you.
			}
		}

		public bool IsEligible(Mobile m, Item Artifact)
		{
			return m.Player && m.Alive && m.Region != null && m.Region == m_Region && m.Backpack != null && m.Backpack.CheckHold(m, Artifact, false);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(7); // version

			writer.Write(m_Region);
			writer.Write(m_SPawnSzMod);
			writer.Write(m_DamageEntries.Count);
			foreach (var kvp in m_DamageEntries)
			{
				writer.Write(kvp.Key);
				writer.Write(kvp.Value);
			}

			writer.Write(m_ConfinedRoaming);
			writer.Write(m_Idol);
			writer.Write(m_HasBeenAdvanced);
			writer.Write(m_SpawnArea);

			writer.Write(m_RandomizeType);

			writer.Write(m_Kills);

			writer.Write(m_Active);
			writer.Write((int)m_Type);
			writer.Write(m_Creatures, true);
			writer.Write(m_RedSkulls, true);
			writer.Write(m_WhiteSkulls, true);
			writer.Write(m_Platform);
			writer.Write(m_Altar);
			writer.Write(m_ExpireDelay);
			writer.WriteDeltaTime(m_ExpireTime);
			writer.Write(m_Champion);
			writer.Write(m_RestartDelay);

			writer.Write(m_RestartTimer != null);

			if (m_RestartTimer != null)
			{
				writer.WriteDeltaTime(m_RestartTime);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			m_DamageEntries = new Dictionary<Mobile, int>();

			var version = reader.ReadInt();

			switch (version)
			{
				case 7:
					{
						m_Region = reader.ReadRegion<ChampionSpawnRegion>();
						goto case 6;
					}
				case 6:
					{
						m_SPawnSzMod = reader.ReadInt();
						goto case 5;
					}
				case 5:
					{
						var entries = reader.ReadInt();
						Mobile m;
						int damage;
						for (var i = 0; i < entries; ++i)
						{
							m = reader.ReadMobile();
							damage = reader.ReadInt();

							if (m == null)
							{
								continue;
							}

							m_DamageEntries.Add(m, damage);
						}

						goto case 4;
					}
				case 4:
					{
						m_ConfinedRoaming = reader.ReadBool();
						m_Idol = reader.ReadItem<IdolOfTheChampion>();
						m_HasBeenAdvanced = reader.ReadBool();

						goto case 3;
					}
				case 3:
					{
						m_SpawnArea = reader.ReadRect2D();

						goto case 2;
					}
				case 2:
					{
						m_RandomizeType = reader.ReadBool();

						goto case 1;
					}
				case 1:
					{
						if (version < 3)
						{
							var oldRange = reader.ReadInt();

							m_SpawnArea = new Rectangle2D(new Point2D(X - oldRange, Y - oldRange), new Point2D(X + oldRange, Y + oldRange));
						}

						m_Kills = reader.ReadInt();

						goto case 0;
					}
				case 0:
					{
						if (version < 1)
						{
							m_SpawnArea = new Rectangle2D(new Point2D(X - 24, Y - 24), new Point2D(X + 24, Y + 24));    //Default was 24
						}

						var active = reader.ReadBool();
						m_Type = (ChampionSpawnType)reader.ReadInt();
						m_Creatures = reader.ReadStrongMobileList();
						m_RedSkulls = reader.ReadStrongItemList();
						m_WhiteSkulls = reader.ReadStrongItemList();
						m_Platform = reader.ReadItem<ChampionPlatform>();
						m_Altar = reader.ReadItem<ChampionAltar>();
						m_ExpireDelay = reader.ReadTimeSpan();
						m_ExpireTime = reader.ReadDeltaTime();
						m_Champion = reader.ReadMobile();
						m_RestartDelay = reader.ReadTimeSpan();

						if (reader.ReadBool())
						{
							m_RestartTime = reader.ReadDeltaTime();
							BeginRestart(m_RestartTime - DateTime.UtcNow);
						}

						if (version < 4)
						{
							m_Idol = new IdolOfTheChampion(this);
							m_Idol.MoveToWorld(new Point3D(X, Y, Z - 15), Map);
						}

						if (m_Platform == null || m_Altar == null || m_Idol == null)
						{
							Delete();
						}
						else if (active)
						{
							Start();
						}

						break;
					}
			}

			if (m_Region == null)
			{
				Timer.DelayCall(UpdateRegion);
			}
		}
	}

	public class ChampionSpawnRegion : BaseRegion
	{
		public ChampionSpawn ChampionSpawn { get; private set; }

		public override bool WeatherSupported => true;

		public ChampionSpawnRegion(ChampionSpawn spawn) : base(null, spawn.Map, Find(spawn.Location, spawn.Map), spawn.SpawnArea)
		{
			ChampionSpawn = spawn;
		}

		public ChampionSpawnRegion(int id) : base(id)
		{
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			Rules.AllowHouses = false;
			Rules.AllowVehicles = false;
			Rules.AllowYoungAggro = true;
		}

		public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
		{
			base.AlterLightLevel(m, ref global, ref personal);
			
			// This is a guesstimate.  TODO: Verify & get exact values 
			// OSI testing: at 2 red skulls, light = 0x3 ; 1 red = 0x3.; 3 = 8; 9 = 0xD 8 = 0xD 12 = 0x12 10 = 0xD
			global = Math.Max(global, 1 + (ChampionSpawn?.Level ?? 0));  
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(ChampionSpawn);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();

			ChampionSpawn = reader.ReadItem<ChampionSpawn>();
		}
	}

	public class IdolOfTheChampion : Item
	{
		private ChampionSpawn m_Spawn;

		public ChampionSpawn Spawn => m_Spawn;

		public override string DefaultName => "Idol of the Champion";


		public IdolOfTheChampion(ChampionSpawn spawn) : base(0x1F18)
		{
			m_Spawn = spawn;
			Movable = false;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_Spawn != null)
			{
				m_Spawn.Delete();
			}
		}

		public IdolOfTheChampion(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Spawn);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Spawn = reader.ReadItem() as ChampionSpawn;

						if (m_Spawn == null)
						{
							Delete();
						}

						break;
					}
			}
		}
	}


	/// Champion Timers
	public class SliceTimer : Timer
	{
		private readonly ChampionSpawn m_Spawn;

		public SliceTimer(ChampionSpawn spawn) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
		{
			m_Spawn = spawn;
			Priority = TimerPriority.OneSecond;
		}

		protected override void OnTick()
		{
			m_Spawn.OnSlice();
		}
	}

	public class RestartTimer : Timer
	{
		private readonly ChampionSpawn m_Spawn;

		public RestartTimer(ChampionSpawn spawn, TimeSpan delay) : base(delay)
		{
			m_Spawn = spawn;
			Priority = TimerPriority.FiveSeconds;
		}

		protected override void OnTick()
		{
			m_Spawn.EndRestart();
		}
	}
}

namespace Server.Mobiles
{
	public abstract class BaseChampion : BaseCreature
	{
		public override bool CanMoveOverObstacles => true;
		public override bool CanDestroyObstacles => true;

		public BaseChampion(AIType aiType) : this(aiType, FightMode.Closest)
		{
		}

		public BaseChampion(AIType aiType, FightMode mode) : base(aiType, mode, 18, 1, 0.1, 0.2)
		{
		}

		public BaseChampion(Serial serial) : base(serial)
		{
		}

		public abstract ChampionSkullType SkullType { get; }

		public abstract Type[] UniqueList { get; }
		public abstract Type[] SharedList { get; }
		public abstract Type[] DecorativeList { get; }
		public abstract MonsterStatuetteType[] StatueTypes { get; }

		public virtual bool NoGoodies => false;

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

		public Item GetArtifact()
		{
			var random = Utility.RandomDouble();
			if (0.05 >= random)
			{
				return CreateArtifact(UniqueList);
			}
			else if (0.15 >= random)
			{
				return CreateArtifact(SharedList);
			}
			else if (0.30 >= random)
			{
				return CreateArtifact(DecorativeList);
			}

			return null;
		}

		public Item CreateArtifact(Type[] list)
		{
			if (list.Length == 0)
			{
				return null;
			}

			var random = Utility.Random(list.Length);

			var type = list[random];

			var artifact = Loot.Construct(type);

			if (artifact is MonsterStatuette && StatueTypes.Length > 0)
			{
				((MonsterStatuette)artifact).Type = StatueTypes[Utility.Random(StatueTypes.Length)];
				((MonsterStatuette)artifact).LootType = LootType.Regular;
			}

			return artifact;
		}

		private PowerScroll CreateRandomPowerScroll()
		{
			int level;
			var random = Utility.RandomDouble();

			if (0.05 >= random)
			{
				level = 20;
			}
			else if (0.4 >= random)
			{
				level = 15;
			}
			else
			{
				level = 10;
			}

			return PowerScroll.CreateRandomNoCraft(level, level);
		}

		public void GivePowerScrolls()
		{
			if (Map != Map.Felucca)
			{
				return;
			}

			var toGive = new List<Mobile>();
			var rights = BaseCreature.GetLootingRights(DamageEntries, HitsMax);

			for (var i = rights.Count - 1; i >= 0; --i)
			{
				var ds = rights[i];

				if (ds.m_HasRight)
				{
					toGive.Add(ds.m_Mobile);
				}
			}

			if (toGive.Count == 0)
			{
				return;
			}

			for (var i = 0; i < toGive.Count; i++)
			{
				var m = toGive[i];

				if (!(m is PlayerMobile))
				{
					continue;
				}

				var gainedPath = false;

				var pointsToGain = 800;

				if (VirtueHelper.Award(m, VirtueName.Valor, pointsToGain, ref gainedPath))
				{
					if (gainedPath)
					{
						m.SendLocalizedMessage(1054032); // You have gained a path in Valor!
					}
					else
					{
						m.SendLocalizedMessage(1054030); // You have gained in Valor!
					}

					//No delay on Valor gains
				}
			}

			// Randomize
			for (var i = 0; i < toGive.Count; ++i)
			{
				var rand = Utility.Random(toGive.Count);
				var hold = toGive[i];
				toGive[i] = toGive[rand];
				toGive[rand] = hold;
			}

			for (var i = 0; i < 6; ++i)
			{
				var m = toGive[i % toGive.Count];

				var ps = CreateRandomPowerScroll();

				GivePowerScrollTo(m, ps);
			}
		}

		public static void GivePowerScrollTo(Mobile m, PowerScroll ps)
		{
			if (ps == null || m == null)    //sanity
			{
				return;
			}

			m.SendLocalizedMessage(1049524); // You have received a scroll of power!

			if (!Core.SE || m.Alive)
			{
				m.AddToBackpack(ps);
			}
			else
			{
				if (m.Corpse != null && !m.Corpse.Deleted)
				{
					m.Corpse.DropItem(ps);
				}
				else
				{
					m.AddToBackpack(ps);
				}
			}

			if (m is PlayerMobile)
			{
				var pm = (PlayerMobile)m;

				for (var j = 0; j < pm.JusticeProtectors.Count; ++j)
				{
					var prot = pm.JusticeProtectors[j];

					if (prot.Map != m.Map || prot.Murderer || prot.Criminal || !JusticeVirtue.CheckMapRegion(m, prot))
					{
						continue;
					}

					var chance = 0;

					switch (VirtueHelper.GetLevel(prot, VirtueName.Justice))
					{
						case VirtueLevel.Seeker: chance = 60; break;
						case VirtueLevel.Follower: chance = 80; break;
						case VirtueLevel.Knight: chance = 100; break;
					}

					if (chance > Utility.Random(100))
					{
						var powerScroll = new PowerScroll(ps.Skill, ps.Value);

						prot.SendLocalizedMessage(1049368); // You have been rewarded for your dedication to Justice!

						if (!Core.SE || prot.Alive)
						{
							prot.AddToBackpack(powerScroll);
						}
						else
						{
							if (prot.Corpse != null && !prot.Corpse.Deleted)
							{
								prot.Corpse.DropItem(powerScroll);
							}
							else
							{
								prot.AddToBackpack(powerScroll);
							}
						}
					}
				}
			}
		}

		public override bool OnBeforeDeath()
		{
			if (!NoKillAwards)
			{
				GivePowerScrolls();

				if (NoGoodies)
				{
					return base.OnBeforeDeath();
				}

				var map = Map;

				if (map != null)
				{
					for (var x = -12; x <= 12; ++x)
					{
						for (var y = -12; y <= 12; ++y)
						{
							var dist = Math.Sqrt(x * x + y * y);

							if (dist <= 12)
							{
								new GoodiesTimer(map, X + x, Y + y).Start();
							}
						}
					}
				}
			}

			return base.OnBeforeDeath();
		}

		public override void OnDeath(Container c)
		{
			if (Map == Map.Felucca)
			{
				//TODO: Confirm SE change or AoS one too?
				var rights = BaseCreature.GetLootingRights(DamageEntries, HitsMax);
				var toGive = new List<Mobile>();

				for (var i = rights.Count - 1; i >= 0; --i)
				{
					var ds = rights[i];

					if (ds.m_HasRight)
					{
						toGive.Add(ds.m_Mobile);
					}
				}

				if (toGive.Count > 0)
				{
					toGive[Utility.Random(toGive.Count)].AddToBackpack(new ChampionSkull(SkullType));
				}
				else
				{
					c.DropItem(new ChampionSkull(SkullType));
				}
			}

			base.OnDeath(c);
		}

		private class GoodiesTimer : Timer
		{
			private readonly Map m_Map;
			private readonly int m_X, m_Y;

			public GoodiesTimer(Map map, int x, int y) : base(TimeSpan.FromSeconds(Utility.RandomDouble() * 10.0))
			{
				m_Map = map;
				m_X = x;
				m_Y = y;
			}

			protected override void OnTick()
			{
				var z = m_Map.GetAverageZ(m_X, m_Y);
				var canFit = m_Map.CanFit(m_X, m_Y, z, 6, false, false);

				for (var i = -3; !canFit && i <= 3; ++i)
				{
					canFit = m_Map.CanFit(m_X, m_Y, z + i, 6, false, false);

					if (canFit)
					{
						z += i;
					}
				}

				if (!canFit)
				{
					return;
				}

				var g = new Gold(500, 1000);

				g.MoveToWorld(new Point3D(m_X, m_Y, z), m_Map);

				if (0.5 >= Utility.RandomDouble())
				{
					switch (Utility.Random(3))
					{
						case 0: // Fire column
							{
								Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
								Effects.PlaySound(g, g.Map, 0x208);

								break;
							}
						case 1: // Explosion
							{
								Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36BD, 20, 10, 5044);
								Effects.PlaySound(g, g.Map, 0x307);

								break;
							}
						case 2: // Ball of fire
							{
								Effects.SendLocationParticles(EffectItem.Create(g.Location, g.Map, EffectItem.DefaultDuration), 0x36FE, 10, 10, 5052);

								break;
							}
					}
				}
			}
		}
	}
}