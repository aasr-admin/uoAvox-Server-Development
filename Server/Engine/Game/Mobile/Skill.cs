using Server.Network;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
	public delegate TimeSpan SkillUseCallback(Mobile user);

	public enum SkillLock : byte
	{
		Up = 0,
		Down = 1,
		Locked = 2
	}

	public enum SkillName
	{
		Alchemy = 0,
		Anatomy = 1,
		AnimalLore = 2,
		ItemID = 3,
		ArmsLore = 4,
		Parry = 5,
		Begging = 6,
		Blacksmith = 7,
		Fletching = 8,
		Peacemaking = 9,
		Camping = 10,
		Carpentry = 11,
		Cartography = 12,
		Cooking = 13,
		DetectHidden = 14,
		Discordance = 15,
		EvalInt = 16,
		Healing = 17,
		Fishing = 18,
		Forensics = 19,
		Herding = 20,
		Hiding = 21,
		Provocation = 22,
		Inscribe = 23,
		Lockpicking = 24,
		Magery = 25,
		MagicResist = 26,
		Tactics = 27,
		Snooping = 28,
		Musicianship = 29,
		Poisoning = 30,
		Archery = 31,
		SpiritSpeak = 32,
		Stealing = 33,
		Tailoring = 34,
		AnimalTaming = 35,
		TasteID = 36,
		Tinkering = 37,
		Tracking = 38,
		Veterinary = 39,
		Swords = 40,
		Macing = 41,
		Fencing = 42,
		Wrestling = 43,
		Lumberjacking = 44,
		Mining = 45,
		Meditation = 46,
		Stealth = 47,
		RemoveTrap = 48,
		Necromancy = 49,
		Focus = 50,
		Chivalry = 51,
		Bushido = 52,
		Ninjitsu = 53,
		Spellweaving = 54,
		Mysticism = 55,
		Imbuing = 56,
		Throwing = 57,
	}

	[PropertyObject]
	public class Skill
	{
		private ushort m_Base;
		private ushort m_Cap;

		public Skills Owner { get; }

		public SkillInfo Info { get; }

		public SkillName SkillName => (SkillName)Info.SkillID;

		public int SkillID => Info.SkillID;

		[CommandProperty(AccessLevel.Counselor)]
		public string Name => Info.Name;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public SkillLock Lock { get; private set; }

		public int BaseFixedPoint
		{
			get => m_Base;
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				else if (value >= 0x10000)
				{
					value = 0xFFFF;
				}

				var sv = (ushort)value;

				int oldBase = m_Base;

				if (m_Base != sv)
				{
					Owner.Total = (Owner.Total - m_Base) + sv;

					m_Base = sv;

					Owner.OnSkillChange(this);

					if (Owner.Owner != null)
					{
						Owner.Owner.OnSkillChange(SkillName, oldBase / 10.0);

						EventSink.InvokeSkillChanged(new SkillChangedEventArgs(Owner.Owner, SkillName, (m_Base - oldBase) / 10.0));
					}
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public double Base
		{
			get => m_Base / 10.0;
			set => BaseFixedPoint = (int)(value * 10.0);
		}

		public int CapFixedPoint
		{
			get => m_Cap;
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				else if (value >= 0x10000)
				{
					value = 0xFFFF;
				}

				var sv = (ushort)value;

				if (m_Cap != sv)
				{
					m_Cap = sv;

					Owner.OnSkillChange(this);
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public double Cap
		{
			get => m_Cap / 10.0;
			set => CapFixedPoint = (int)(value * 10.0);
		}

		public static bool UseStatMods { get; set; }

		public int Fixed => (int)(Value * 10);

		[CommandProperty(AccessLevel.Counselor)]
		public double Value
		{
			get
			{
				//There has to be this distinction between the racial values and not to account for gaining skills and these skills aren't displayed nor Totaled up.
				var value = NonRacialValue;

				var raceBonus = Owner.Owner.RacialSkillBonus;

				if (raceBonus > value)
				{
					value = raceBonus;
				}

				return value;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public double NonRacialValue
		{
			get
			{
				var baseValue = Base;
				var inv = 100.0 - baseValue;

				if (inv < 0.0)
				{
					inv = 0.0;
				}

				inv /= 100.0;

				var statsOffset = ((UseStatMods ? Owner.Owner.Str : Owner.Owner.RawStr) * Info.StrScale) + ((UseStatMods ? Owner.Owner.Dex : Owner.Owner.RawDex) * Info.DexScale) + ((UseStatMods ? Owner.Owner.Int : Owner.Owner.RawInt) * Info.IntScale);
				var statTotal = Info.StatTotal * inv;

				statsOffset *= inv;

				if (statsOffset > statTotal)
				{
					statsOffset = statTotal;
				}

				var value = baseValue + statsOffset;

				Owner.Owner.ValidateSkillMods();

				var mods = Owner.Owner.SkillMods;

				double bonusObey = 0.0, bonusNotObey = 0.0;

				for (var i = 0; i < mods.Count; ++i)
				{
					var mod = mods[i];

					if (mod.Skill == (SkillName)Info.SkillID)
					{
						if (mod.Relative)
						{
							if (mod.ObeyCap)
							{
								bonusObey += mod.Value;
							}
							else
							{
								bonusNotObey += mod.Value;
							}
						}
						else
						{
							bonusObey = 0.0;
							bonusNotObey = 0.0;
							value = mod.Value;
						}
					}
				}

				value += bonusNotObey;

				if (value < Cap)
				{
					value += bonusObey;

					if (value > Cap)
					{
						value = Cap;
					}
				}

				return value;
			}
		}

		public Skill(Skills owner, SkillInfo info, GenericReader reader)
		{
			Owner = owner;
			Info = info;

			Deserialize(reader);

			if (!Enum.IsDefined(Lock))
			{
				Console.WriteLine($"Bad skill lock -> {owner.Owner}.{Lock}");
				Lock = SkillLock.Up;
			}
		}

		public Skill(Skills owner, SkillInfo info, int baseValue, int cap, SkillLock skillLock)
		{
			Owner = owner;
			Info = info;
			m_Base = (ushort)baseValue;
			m_Cap = (ushort)cap;
			Lock = skillLock;
		}

		public override string ToString()
		{
			return $"[{Name}: {Base}]";
		}

		public void SetLockNoRelay(SkillLock skillLock)
		{
			if (Enum.IsDefined(skillLock))
			{
				Lock = skillLock;
			}
		}

		public void Update()
		{
			Owner.OnSkillChange(this);
		}

		public void Serialize(GenericWriter writer)
		{
			if (m_Base == 0 && m_Cap == 1000 && Lock == SkillLock.Up)
			{
				writer.Write((byte)0xFF); // default
			}
			else
			{
				var flags = 0x0;

				if (m_Base != 0)
				{
					flags |= 0x1;
				}

				if (m_Cap != 1000)
				{
					flags |= 0x2;
				}

				if (Lock != SkillLock.Up)
				{
					flags |= 0x4;
				}

				writer.Write((byte)flags); // version

				if (m_Base != 0)
				{
					writer.Write((short)m_Base);
				}

				if (m_Cap != 1000)
				{
					writer.Write((short)m_Cap);
				}

				if (Lock != SkillLock.Up)
				{
					writer.Write((byte)Lock);
				}
			}
		}

		public void Deserialize(GenericReader reader)
		{
			int version = reader.ReadByte();

			switch (version)
			{
				case 0:
					{
						m_Base = reader.ReadUShort();
						m_Cap = reader.ReadUShort();
						Lock = (SkillLock)reader.ReadByte();

						break;
					}
				case 0xFF:
					{
						m_Base = 0;
						m_Cap = 1000;
						Lock = SkillLock.Up;

						break;
					}
				default:
					{
						if ((version & 0xC0) == 0x00)
						{
							if ((version & 0x1) != 0)
							{
								m_Base = reader.ReadUShort();
							}

							if ((version & 0x2) != 0)
							{
								m_Cap = reader.ReadUShort();
							}
							else
							{
								m_Cap = 1000;
							}

							if ((version & 0x4) != 0)
							{
								Lock = (SkillLock)reader.ReadByte();
							}
						}

						break;
					}
			}
		}
	}

	public class SkillInfo
	{
		public static SkillInfo[] Table { get; set; } =
		{
			new SkillInfo(  0, "Alchemy",                   0.0,    5.0,    5.0,    "Alchemist",        0.0,    0.5,    0.5,    1.0 ),
			new SkillInfo(  1, "Anatomy",                   0.0,    0.0,    0.0,    "Biologist",        0.15,   0.15,   0.7,    1.0 ),
			new SkillInfo(  2, "Animal Lore",               0.0,    0.0,    0.0,    "Naturalist",       0.0,    0.0,    1.0,    1.0 ),
			new SkillInfo(  3, "Item Identification",       0.0,    0.0,    0.0,    "Merchant",         0.0,    0.0,    1.0,    1.0 ),
			new SkillInfo(  4, "Arms Lore",                 0.0,    0.0,    0.0,    "Weapon Master",    0.75,   0.15,   0.1,    1.0 ),
			new SkillInfo(  5, "Parrying",                  7.5,    2.5,    0.0,    "Duelist",          0.75,   0.25,   0.0,    1.0 ),
			new SkillInfo(  6, "Begging",                   0.0,    0.0,    0.0,    "Beggar",           0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo(  7, "Blacksmithy",               10.0,   0.0,    0.0,    "Blacksmith",       1.0,    0.0,    0.0,    1.0 ),
			new SkillInfo(  8, "Bowcraft/Fletching",        6.0,    16.0,   0.0,    "Bowyer",           0.6,    1.6,    0.0,    1.0 ),
			new SkillInfo(  9, "Peacemaking",               0.0,    0.0,    0.0,    "Pacifier",         0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 10, "Camping",                   20.0,   15.0,   15.0,   "Explorer",         2.0,    1.5,    1.5,    1.0 ),
			new SkillInfo( 11, "Carpentry",                 20.0,   5.0,    0.0,    "Carpenter",        2.0,    0.5,    0.0,    1.0 ),
			new SkillInfo( 12, "Cartography",               0.0,    7.5,    7.5,    "Cartographer",     0.0,    0.75,   0.75,   1.0 ),
			new SkillInfo( 13, "Cooking",                   0.0,    20.0,   30.0,   "Chef",             0.0,    2.0,    3.0,    1.0 ),
			new SkillInfo( 14, "Detecting Hidden",          0.0,    0.0,    0.0,    "Scout",            0.0,    0.4,    0.6,    1.0 ),
			new SkillInfo( 15, "Discordance",               0.0,    2.5,    2.5,    "Demoralizer",      0.0,    0.25,   0.25,   1.0 ),
			new SkillInfo( 16, "Evaluating Intelligence",   0.0,    0.0,    0.0,    "Scholar",          0.0,    0.0,    1.0,    1.0 ),
			new SkillInfo( 17, "Healing",                   6.0,    6.0,    8.0,    "Healer",           0.6,    0.6,    0.8,    1.0 ),
			new SkillInfo( 18, "Fishing",                   0.0,    0.0,    0.0,    "Fisherman",        0.5,    0.5,    0.0,    1.0 ),
			new SkillInfo( 19, "Forensic Evaluation",       0.0,    0.0,    0.0,    "Detective",        0.0,    0.2,    0.8,    1.0 ),
			new SkillInfo( 20, "Herding",                   16.25,  6.25,   2.5,    "Shepherd",         1.625,  0.625,  0.25,   1.0 ),
			new SkillInfo( 21, "Hiding",                    0.0,    0.0,    0.0,    "Shade",            0.0,    0.8,    0.2,    1.0 ),
			new SkillInfo( 22, "Provocation",               0.0,    4.5,    0.5,    "Rouser",           0.0,    0.45,   0.05,   1.0 ),
			new SkillInfo( 23, "Inscription",               0.0,    2.0,    8.0,    "Scribe",           0.0,    0.2,    0.8,    1.0 ),
			new SkillInfo( 24, "Lockpicking",               0.0,    25.0,   0.0,    "Infiltrator",      0.0,    2.0,    0.0,    1.0 ),
			new SkillInfo( 25, "Magery",                    0.0,    0.0,    15.0,   "Mage",             0.0,    0.0,    1.5,    1.0 ),
			new SkillInfo( 26, "Resisting Spells",          0.0,    0.0,    0.0,    "Warder",           0.25,   0.25,   0.5,    1.0 ),
			new SkillInfo( 27, "Tactics",                   0.0,    0.0,    0.0,    "Tactician",        0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 28, "Snooping",                  0.0,    25.0,   0.0,    "Spy",              0.0,    2.5,    0.0,    1.0 ),
			new SkillInfo( 29, "Musicianship",              0.0,    0.0,    0.0,    "Bard",             0.0,    0.8,    0.2,    1.0 ),
			new SkillInfo( 30, "Poisoning",                 0.0,    4.0,    16.0,   "Assassin",         0.0,    0.4,    1.6,    1.0 ),
			new SkillInfo( 31, "Archery",                   2.5,    7.5,    0.0,    "Archer",           0.25,   0.75,   0.0,    1.0 ),
			new SkillInfo( 32, "Spirit Speak",              0.0,    0.0,    0.0,    "Medium",           0.0,    0.0,    1.0,    1.0 ),
			new SkillInfo( 33, "Stealing",                  0.0,    10.0,   0.0,    "Pickpocket",       0.0,    1.0,    0.0,    1.0 ),
			new SkillInfo( 34, "Tailoring",                 3.75,   16.25,  5.0,    "Tailor",           0.38,   1.63,   0.5,    1.0 ),
			new SkillInfo( 35, "Animal Taming",             14.0,   2.0,    4.0,    "Tamer",            1.4,    0.2,    0.4,    1.0 ),
			new SkillInfo( 36, "Taste Identification",      0.0,    0.0,    0.0,    "Praegustator",     0.2,    0.0,    0.8,    1.0 ),
			new SkillInfo( 37, "Tinkering",                 5.0,    2.0,    3.0,    "Tinker",           0.5,    0.2,    0.3,    1.0 ),
			new SkillInfo( 38, "Tracking",                  0.0,    12.5,   12.5,   "Ranger",           0.0,    1.25,   1.25,   1.0 ),
			new SkillInfo( 39, "Veterinary",                8.0,    4.0,    8.0,    "Veterinarian",     0.8,    0.4,    0.8,    1.0 ),
			new SkillInfo( 40, "Swordsmanship",             7.5,    2.5,    0.0,    "Swordsman",        0.75,   0.25,   0.0,    1.0 ),
			new SkillInfo( 41, "Mace Fighting",             9.0,    1.0,    0.0,    "Armsman",          0.9,    0.1,    0.0,    1.0 ),
			new SkillInfo( 42, "Fencing",                   4.5,    5.5,    0.0,    "Fencer",           0.45,   0.55,   0.0,    1.0 ),
			new SkillInfo( 43, "Wrestling",                 9.0,    1.0,    0.0,    "Wrestler",         0.9,    0.1,    0.0,    1.0 ),
			new SkillInfo( 44, "Lumberjacking",             20.0,   0.0,    0.0,    "Lumberjack",       2.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 45, "Mining",                    20.0,   0.0,    0.0,    "Miner",            2.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 46, "Meditation",                0.0,    0.0,    0.0,    "Stoic",            0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 47, "Stealth",                   0.0,    0.0,    0.0,    "Rogue",            0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 48, "Remove Trap",               0.0,    0.0,    0.0,    "Trap Specialist",  0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 49, "Necromancy",                0.0,    0.0,    0.0,    "Necromancer",      0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 50, "Focus",                     0.0,    0.0,    0.0,    "Driven",           0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 51, "Chivalry",                  0.0,    0.0,    0.0,    "Paladin",          0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 52, "Bushido",                   0.0,    0.0,    0.0,    "Samurai",          0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 53, "Ninjitsu",                  0.0,    0.0,    0.0,    "Ninja",            0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 54, "Spellweaving",              0.0,    0.0,    0.0,    "Arcanist",         0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 55, "Mysticism",                 0.0,    0.0,    0.0,    "Mystic",           0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 56, "Imbuing",                   0.0,    0.0,    0.0,    "Artificer",        0.0,    0.0,    0.0,    1.0 ),
			new SkillInfo( 57, "Throwing",                  0.0,    0.0,    0.0,    "Bladeweaver",      0.0,    0.0,    0.0,    1.0 ),
		};

		public SkillUseCallback Callback { get; set; }

		public int SkillID { get; }

		public string Name { get; set; }
		public string Title { get; set; }

		public double StrScale { get; set; }
		public double DexScale { get; set; }
		public double IntScale { get; set; }

		public double StatTotal { get; set; }

		public double StrGain { get; set; }
		public double DexGain { get; set; }
		public double IntGain { get; set; }

		public double GainFactor { get; set; }

		public SkillInfo(int skillID, string name, double strScale, double dexScale, double intScale, string title, double strGain, double dexGain, double intGain, double gainFactor)
			: this(skillID, name, strScale, dexScale, intScale, title, null, strGain, dexGain, intGain, gainFactor)
		{ }

		public SkillInfo(int skillID, string name, double strScale, double dexScale, double intScale, string title, SkillUseCallback callback, double strGain, double dexGain, double intGain, double gainFactor)
		{
			Name = name;
			Title = title;
			SkillID = skillID;
			StrScale = strScale / 100.0;
			DexScale = dexScale / 100.0;
			IntScale = intScale / 100.0;
			Callback = callback;
			StrGain = strGain;
			DexGain = dexGain;
			IntGain = intGain;
			GainFactor = gainFactor;

			StatTotal = strScale + dexScale + intScale;
		}
	}

	public class Skills : SkillStates<Skill>
	{
		public static bool UseSkill(Mobile from, SkillName name)
		{
			return UseSkill(from, (int)name);
		}

		public static bool UseSkill(Mobile from, int skillID)
		{
			if (!from.CheckAlive())
			{
				return false;
			}

			if (!from.Region.OnSkillUse(from, skillID))
			{
				return false;
			}

			if (!from.AllowSkillUse((SkillName)skillID))
			{
				return false;
			}

			if (skillID >= 0 && skillID < SkillInfo.Table.Length)
			{
				var info = SkillInfo.Table[skillID];

				if (info.Callback != null)
				{
					if (Core.TickCount - from.NextSkillTime >= 0 && from.Spell == null)
					{
						from.DisruptiveAction();

						from.NextSkillTime = Core.TickCount + (int)info.Callback(from).TotalMilliseconds;

						return true;
					}

					from.SendSkillMessage();
				}
				else
				{
					from.SendLocalizedMessage(500014); // That skill cannot be used directly.
				}
			}

			return false;
		}

		[CommandProperty(AccessLevel.Counselor, true)]
		public Mobile Owner { get; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Cap { get; set; }

		[CommandProperty(AccessLevel.Counselor, true)]
		public int Total { get; set; }

		private Skill m_Highest;

		public Skill Highest
		{
			get
			{
				if (m_Highest == null)
				{
					Skill highest = null;

					var value = Int32.MinValue;

					foreach (var sk in this)
					{
						if (sk != null && sk.BaseFixedPoint > value)
						{
							value = sk.BaseFixedPoint;
							highest = sk;
						}
					}

					if (highest == null && Length > 0)
					{
						highest = this[0];
					}

					m_Highest = highest;
				}

				return m_Highest;
			}
		}

		public Skills(Mobile owner)
		{
			Owner = owner;
			Cap = 7000;
		}

		public Skills(Mobile owner, GenericReader reader)
			: base(reader)
		{
			Owner = owner;
		}

		protected override Skill Get(SkillName skill)
		{
			var sk = base.Get(skill);

			if (sk?.Owner != this)
			{
				var index = Convert.ToInt32(skill);

				Set(skill, sk = new Skill(this, SkillInfo.Table[index], 0, 1000, SkillLock.Up));
			}

			return sk;
		}

		protected override void Set(SkillName skill, Skill value)
		{
			if (value?.Owner == this)
			{
				base.Set(skill, value);
			}
		}

		public void OnSkillChange(Skill skill)
		{
			if (skill == m_Highest) // could be downgrading the skill, force a recalc
			{
				m_Highest = null;
			}
			else if (m_Highest != null && skill.BaseFixedPoint > m_Highest.BaseFixedPoint)
			{
				m_Highest = skill;
			}

			Owner.OnSkillInvalidated(skill);

			Owner.NetState?.Send(new SkillChange(skill));
		}

		public override void Serialize(GenericWriter writer)
		{
			Total = this.Sum(sk => sk.BaseFixedPoint);

			writer.Write(4); // version

			writer.Write(Cap);

			base.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			var version = reader.ReadInt();

			switch (version)
			{
				case 4:
				case 3:
				case 2:
					{
						Cap = reader.ReadInt();

						goto case 1;
					}
				case 1:
					{
						if (version < 2)
						{
							Cap = 7000;
						}

						if (version < 3)
						{
							reader.ReadInt(); // Total
						}

						if (version < 4)
						{
							var count = reader.ReadInt();

							for (var i = 0; i < count; i++)
							{
								var key = (SkillName)i;

								this[key] = ReadData(reader, key);
							}
						}

						break;
					}
				case 0:
					{
						reader.ReadInt();

						goto case 1;
					}
			}

			if (version >= 4)
			{
				base.Deserialize(reader);
			}

			Total = this.Sum(sk => sk.BaseFixedPoint);
		}

		protected override void WriteData(GenericWriter writer, SkillName key, Skill value)
		{
			value.Serialize(writer);
		}

		protected override Skill ReadData(GenericReader reader, SkillName key)
		{
			return new Skill(this, SkillInfo.Table[(int)key], reader);
		}
	}

	[NoSort, PropertyObject]
	public abstract class SkillStates<T> : BaseStates<SkillName, T>
	{
		#region Skill Getters & Setters

		[CommandProperty(AccessLevel.Counselor)]
		public T Alchemy { get => this[SkillName.Alchemy]; set => this[SkillName.Alchemy] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Anatomy { get => this[SkillName.Anatomy]; set => this[SkillName.Anatomy] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T AnimalLore { get => this[SkillName.AnimalLore]; set => this[SkillName.AnimalLore] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T ItemID { get => this[SkillName.ItemID]; set => this[SkillName.ItemID] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T ArmsLore { get => this[SkillName.ArmsLore]; set => this[SkillName.ArmsLore] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Parry { get => this[SkillName.Parry]; set => this[SkillName.Parry] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Begging { get => this[SkillName.Begging]; set => this[SkillName.Begging] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Blacksmith { get => this[SkillName.Blacksmith]; set => this[SkillName.Blacksmith] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Fletching { get => this[SkillName.Fletching]; set => this[SkillName.Fletching] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Peacemaking { get => this[SkillName.Peacemaking]; set => this[SkillName.Peacemaking] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Camping { get => this[SkillName.Camping]; set => this[SkillName.Camping] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Carpentry { get => this[SkillName.Carpentry]; set => this[SkillName.Carpentry] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Cartography { get => this[SkillName.Cartography]; set => this[SkillName.Cartography] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Cooking { get => this[SkillName.Cooking]; set => this[SkillName.Cooking] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T DetectHidden { get => this[SkillName.DetectHidden]; set => this[SkillName.DetectHidden] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Discordance { get => this[SkillName.Discordance]; set => this[SkillName.Discordance] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T EvalInt { get => this[SkillName.EvalInt]; set => this[SkillName.EvalInt] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Healing { get => this[SkillName.Healing]; set => this[SkillName.Healing] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Fishing { get => this[SkillName.Fishing]; set => this[SkillName.Fishing] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Forensics { get => this[SkillName.Forensics]; set => this[SkillName.Forensics] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Herding { get => this[SkillName.Herding]; set => this[SkillName.Herding] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Hiding { get => this[SkillName.Hiding]; set => this[SkillName.Hiding] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Provocation { get => this[SkillName.Provocation]; set => this[SkillName.Provocation] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Inscribe { get => this[SkillName.Inscribe]; set => this[SkillName.Inscribe] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Lockpicking { get => this[SkillName.Lockpicking]; set => this[SkillName.Lockpicking] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Magery { get => this[SkillName.Magery]; set => this[SkillName.Magery] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T MagicResist { get => this[SkillName.MagicResist]; set => this[SkillName.MagicResist] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Tactics { get => this[SkillName.Tactics]; set => this[SkillName.Tactics] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Snooping { get => this[SkillName.Snooping]; set => this[SkillName.Snooping] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Musicianship { get => this[SkillName.Musicianship]; set => this[SkillName.Musicianship] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Poisoning { get => this[SkillName.Poisoning]; set => this[SkillName.Poisoning] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Archery { get => this[SkillName.Archery]; set => this[SkillName.Archery] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T SpiritSpeak { get => this[SkillName.SpiritSpeak]; set => this[SkillName.SpiritSpeak] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Stealing { get => this[SkillName.Stealing]; set => this[SkillName.Stealing] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Tailoring { get => this[SkillName.Tailoring]; set => this[SkillName.Tailoring] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T AnimalTaming { get => this[SkillName.AnimalTaming]; set => this[SkillName.AnimalTaming] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T TasteID { get => this[SkillName.TasteID]; set => this[SkillName.TasteID] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Tinkering { get => this[SkillName.Tinkering]; set => this[SkillName.Tinkering] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Tracking { get => this[SkillName.Tracking]; set => this[SkillName.Tracking] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Veterinary { get => this[SkillName.Veterinary]; set => this[SkillName.Veterinary] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Swords { get => this[SkillName.Swords]; set => this[SkillName.Swords] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Macing { get => this[SkillName.Macing]; set => this[SkillName.Macing] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Fencing { get => this[SkillName.Fencing]; set => this[SkillName.Fencing] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Wrestling { get => this[SkillName.Wrestling]; set => this[SkillName.Wrestling] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Lumberjacking { get => this[SkillName.Lumberjacking]; set => this[SkillName.Lumberjacking] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Mining { get => this[SkillName.Mining]; set => this[SkillName.Mining] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Meditation { get => this[SkillName.Meditation]; set => this[SkillName.Meditation] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Stealth { get => this[SkillName.Stealth]; set => this[SkillName.Stealth] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T RemoveTrap { get => this[SkillName.RemoveTrap]; set => this[SkillName.RemoveTrap] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Necromancy { get => this[SkillName.Necromancy]; set => this[SkillName.Necromancy] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Focus { get => this[SkillName.Focus]; set => this[SkillName.Focus] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Chivalry { get => this[SkillName.Chivalry]; set => this[SkillName.Chivalry] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Bushido { get => this[SkillName.Bushido]; set => this[SkillName.Bushido] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Ninjitsu { get => this[SkillName.Ninjitsu]; set => this[SkillName.Ninjitsu] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Spellweaving { get => this[SkillName.Spellweaving]; set => this[SkillName.Spellweaving] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Mysticism { get => this[SkillName.Mysticism]; set => this[SkillName.Mysticism] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Imbuing { get => this[SkillName.Imbuing]; set => this[SkillName.Imbuing] = value; }

		[CommandProperty(AccessLevel.Counselor)]
		public T Throwing { get => this[SkillName.Throwing]; set => this[SkillName.Throwing] = value; }

		#endregion

		public T this[int skill] { get => this[(SkillName)skill]; set => this[(SkillName)skill] = value; }

		public SkillStates()
		{
		}

		public SkillStates(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}
	}

	[NoSort]
	public class SkillPermissions : SkillStates<bool>
	{
		public SkillPermissions()
		{
			SetAll(true);
		}

		public SkillPermissions(GenericReader reader)
			: base(reader)
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

			reader.ReadInt();
		}

		protected override void WriteData(GenericWriter writer, SkillName key, bool value)
		{
			writer.Write(value);
		}

		protected override bool ReadData(GenericReader reader, SkillName key)
		{
			return reader.ReadBool();
		}
	}
}