using Server.Items;
using Server.Mobiles;

using System;

#region Developer Notations

/// SHIELDS ARE COMBINED WITH ARMOR LIST AND HATS ARE COMBINED WITH CLOTHING LIST. 
/// NAMES HAVE BEEN KEPT THE SAME, SUCH AS "RANDOMHAT" WHEN IT GIVES ALL CLOTHING TO MAKE EVERYTHING MORE BACKWARDS COMPATIBILE

#endregion

namespace Server
{
	public partial class Loot
	{
		public static Item Construct(Type type)
		{
			try
			{
				return Activator.CreateInstance(type) as Item;
			}
			catch
			{
				return null;
			}
		}

		public static Item Construct(Type[] types)
		{
			if (types.Length > 0)
			{
				return Construct(types, Utility.Random(types.Length));
			}

			return null;
		}

		public static Item Construct(Type[] types, int index)
		{
			if (index >= 0 && index < types.Length)
			{
				return Construct(types[index]);
			}

			return null;
		}

		public static Item Construct(params Type[][] types)
		{
			var totalLength = 0;

			for (var i = 0; i < types.Length; ++i)
			{
				totalLength += types[i].Length;
			}

			if (totalLength > 0)
			{
				var index = Utility.Random(totalLength);

				for (var i = 0; i < types.Length; ++i)
				{
					if (index >= 0 && index < types[i].Length)
					{
						return Construct(types[i][index]);
					}

					index -= types[i].Length;
				}
			}

			return null;
		}
	}

	public partial class LootPack
	{
		public static int GetLuckChance(Mobile killer, Mobile victim)
		{
			if (!Core.AOS)
			{
				return 0;
			}

			var luck = killer.Luck;

			var pmKiller = killer as PlayerMobile;
			if (pmKiller != null && pmKiller.SentHonorContext != null && pmKiller.SentHonorContext.Target == victim)
			{
				luck += pmKiller.SentHonorContext.PerfectionLuckBonus;
			}

			if (luck < 0)
			{
				return 0;
			}

			if (!Core.SE && luck > 1200)
			{
				luck = 1200;
			}

			return (int)(Math.Pow(luck, 1 / 1.8) * 100);
		}

		public static int GetLuckChanceForKiller(Mobile dead)
		{
			var list = BaseCreature.GetLootingRights(dead.DamageEntries, dead.HitsMax);

			DamageStore highest = null;

			for (var i = 0; i < list.Count; ++i)
			{
				var ds = list[i];

				if (ds.m_HasRight && (highest == null || ds.m_Damage > highest.m_Damage))
				{
					highest = ds;
				}
			}

			if (highest == null)
			{
				return 0;
			}

			return GetLuckChance(highest.m_Mobile, dead);
		}

		public static bool CheckLuck(int chance)
		{
			return (chance > Utility.Random(10000));
		}

		private readonly LootPackEntry[] m_Entries;

		public LootPack(LootPackEntry[] entries)
		{
			m_Entries = entries;
		}

		public void Generate(Mobile from, Container cont, bool spawning, int luckChance)
		{
			if (cont == null)
			{
				return;
			}

			var checkLuck = Core.AOS;

			for (var i = 0; i < m_Entries.Length; ++i)
			{
				var entry = m_Entries[i];

				var shouldAdd = (entry.Chance > Utility.Random(10000));

				if (!shouldAdd && checkLuck)
				{
					checkLuck = false;

					if (LootPack.CheckLuck(luckChance))
					{
						shouldAdd = (entry.Chance > Utility.Random(10000));
					}
				}

				if (!shouldAdd)
				{
					continue;
				}

				var item = entry.Construct(from, luckChance, spawning);

				if (item != null)
				{
					if (!item.Stackable || !cont.TryDropItem(from, item, false))
					{
						cont.DropItem(item);
					}
				}
			}
		}
	}

	public class LootPackEntry
	{
		private int m_Chance;
		private LootPackDice m_Quantity;

		private int m_MaxProps, m_MinIntensity, m_MaxIntensity;

		private readonly bool m_AtSpawnTime;

		private LootPackItem[] m_Items;

		public int Chance
		{
			get => m_Chance;
			set => m_Chance = value;
		}

		public LootPackDice Quantity
		{
			get => m_Quantity;
			set => m_Quantity = value;
		}

		public int MaxProps
		{
			get => m_MaxProps;
			set => m_MaxProps = value;
		}

		public int MinIntensity
		{
			get => m_MinIntensity;
			set => m_MinIntensity = value;
		}

		public int MaxIntensity
		{
			get => m_MaxIntensity;
			set => m_MaxIntensity = value;
		}

		public LootPackItem[] Items
		{
			get => m_Items;
			set => m_Items = value;
		}

		private static bool IsInTokuno(Mobile m)
		{
			if (m.Region.IsPartOf("Fan Dancer's Dojo"))
			{
				return true;
			}

			if (m.Region.IsPartOf("Yomotsu Mines"))
			{
				return true;
			}

			return (m.Map == Map.Tokuno);
		}

		#region Mondain's Legacy
		private static bool IsMondain(Mobile m)
		{
			return ValorSpawner.IsValorRegion(m.Region);
		}
		#endregion

		public Item Construct(Mobile from, int luckChance, bool spawning)
		{
			if (m_AtSpawnTime != spawning)
			{
				return null;
			}

			var totalChance = 0;

			for (var i = 0; i < m_Items.Length; ++i)
			{
				totalChance += m_Items[i].Chance;
			}

			var rnd = Utility.Random(totalChance);

			for (var i = 0; i < m_Items.Length; ++i)
			{
				var item = m_Items[i];

				if (rnd < item.Chance)
				{
					return Mutate(from, luckChance, item.Construct(IsInTokuno(from), IsMondain(from)));
				}

				rnd -= item.Chance;
			}

			return null;
		}

		private int GetRandomOldBonus()
		{
			var rnd = Utility.RandomMinMax(m_MinIntensity, m_MaxIntensity);

			if (50 > rnd)
			{
				return 1;
			}
			else
			{
				rnd -= 50;
			}

			if (25 > rnd)
			{
				return 2;
			}
			else
			{
				rnd -= 25;
			}

			if (14 > rnd)
			{
				return 3;
			}
			else
			{
				rnd -= 14;
			}

			if (8 > rnd)
			{
				return 4;
			}

			return 5;
		}

		public Item Mutate(Mobile from, int luckChance, Item item)
		{
			if (item != null)
			{
				if (item is BaseWeapon && 1 > Utility.Random(100))
				{
					item.Delete();
					item = new FireHorn();
					return item;
				}

				if (item is BaseWeapon || item is BaseArmor || item is BaseJewel || item is BaseHat)
				{
					if (Core.AOS)
					{
						var bonusProps = GetBonusProperties();
						var min = m_MinIntensity;
						var max = m_MaxIntensity;

						if (bonusProps < m_MaxProps && LootPack.CheckLuck(luckChance))
						{
							++bonusProps;
						}

						var props = 1 + bonusProps;

						// Make sure we're not spawning items with 6 properties.
						if (props > m_MaxProps)
						{
							props = m_MaxProps;
						}

						if (item is BaseWeapon)
						{
							BaseRunicTool.ApplyAttributesTo((BaseWeapon)item, false, luckChance, props, m_MinIntensity, m_MaxIntensity);
						}
						else if (item is BaseArmor)
						{
							BaseRunicTool.ApplyAttributesTo((BaseArmor)item, false, luckChance, props, m_MinIntensity, m_MaxIntensity);
						}
						else if (item is BaseJewel)
						{
							BaseRunicTool.ApplyAttributesTo((BaseJewel)item, false, luckChance, props, m_MinIntensity, m_MaxIntensity);
						}
						else if (item is BaseHat)
						{
							BaseRunicTool.ApplyAttributesTo((BaseHat)item, false, luckChance, props, m_MinIntensity, m_MaxIntensity);
						}
					}
					else // not aos
					{
						if (item is BaseWeapon)
						{
							var weapon = (BaseWeapon)item;

							if (80 > Utility.Random(100))
							{
								weapon.AccuracyLevel = (WeaponAccuracyLevel)GetRandomOldBonus();
							}

							if (60 > Utility.Random(100))
							{
								weapon.DamageLevel = (WeaponDamageLevel)GetRandomOldBonus();
							}

							if (40 > Utility.Random(100))
							{
								weapon.DurabilityLevel = (WeaponDurabilityLevel)GetRandomOldBonus();
							}

							if (5 > Utility.Random(100))
							{
								weapon.Slayer = SlayerName.Silver;
							}

							if (from != null && weapon.AccuracyLevel == 0 && weapon.DamageLevel == 0 && weapon.DurabilityLevel == 0 && weapon.Slayer == SlayerName.None && 5 > Utility.Random(100))
							{
								weapon.Slayer = SlayerGroup.GetLootSlayerType(from.GetType());
							}
						}
						else if (item is BaseArmor)
						{
							var armor = (BaseArmor)item;

							if (80 > Utility.Random(100))
							{
								armor.ProtectionLevel = (ArmorProtectionLevel)GetRandomOldBonus();
							}

							if (40 > Utility.Random(100))
							{
								armor.Durability = (ArmorDurabilityLevel)GetRandomOldBonus();
							}
						}
					}
				}
				else if (item is BaseInstrument)
				{
					var slayer = SlayerName.None;

					if (Core.AOS)
					{
						slayer = BaseRunicTool.GetRandomSlayer();
					}
					else
					{
						slayer = SlayerGroup.GetLootSlayerType(from.GetType());
					}

					if (slayer == SlayerName.None)
					{
						item.Delete();
						return null;
					}

					var instr = (BaseInstrument)item;

					instr.Quality = ItemQuality.Regular;
					instr.Slayer = slayer;
				}

				if (item.Stackable)
				{
					item.Amount = m_Quantity.Roll();
				}
			}

			return item;
		}

		public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, string quantity) : this(atSpawnTime, items, chance, new LootPackDice(quantity), 0, 0, 0)
		{
		}

		public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, int quantity) : this(atSpawnTime, items, chance, new LootPackDice(0, 0, quantity), 0, 0, 0)
		{
		}

		public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, string quantity, int maxProps, int minIntensity, int maxIntensity) : this(atSpawnTime, items, chance, new LootPackDice(quantity), maxProps, minIntensity, maxIntensity)
		{
		}

		public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, int quantity, int maxProps, int minIntensity, int maxIntensity) : this(atSpawnTime, items, chance, new LootPackDice(0, 0, quantity), maxProps, minIntensity, maxIntensity)
		{
		}

		public LootPackEntry(bool atSpawnTime, LootPackItem[] items, double chance, LootPackDice quantity, int maxProps, int minIntensity, int maxIntensity)
		{
			m_AtSpawnTime = atSpawnTime;
			m_Items = items;
			m_Chance = (int)(100 * chance);
			m_Quantity = quantity;
			m_MaxProps = maxProps;
			m_MinIntensity = minIntensity;
			m_MaxIntensity = maxIntensity;
		}

		public int GetBonusProperties()
		{
			int p0 = 0, p1 = 0, p2 = 0, p3 = 0, p4 = 0, p5 = 0;

			switch (m_MaxProps)
			{
				case 1: p0 = 3; p1 = 1; break;
				case 2: p0 = 6; p1 = 3; p2 = 1; break;
				case 3: p0 = 10; p1 = 6; p2 = 3; p3 = 1; break;
				case 4: p0 = 16; p1 = 12; p2 = 6; p3 = 5; p4 = 1; break;
				case 5: p0 = 30; p1 = 25; p2 = 20; p3 = 15; p4 = 9; p5 = 1; break;
			}

			var pc = p0 + p1 + p2 + p3 + p4 + p5;

			var rnd = Utility.Random(pc);

			if (rnd < p5)
			{
				return 5;
			}
			else
			{
				rnd -= p5;
			}

			if (rnd < p4)
			{
				return 4;
			}
			else
			{
				rnd -= p4;
			}

			if (rnd < p3)
			{
				return 3;
			}
			else
			{
				rnd -= p3;
			}

			if (rnd < p2)
			{
				return 2;
			}
			else
			{
				rnd -= p2;
			}

			if (rnd < p1)
			{
				return 1;
			}

			return 0;
		}
	}

	public partial class LootPackItem
	{
		private Type m_Type;
		private int m_Chance;

		public Type Type
		{
			get => m_Type;
			set => m_Type = value;
		}

		public int Chance
		{
			get => m_Chance;
			set => m_Chance = value;
		}

		public LootPackItem(Type type, int chance)
		{
			m_Type = type;
			m_Chance = chance;
		}
	}

	public class LootPackDice
	{
		private int m_Count, m_Sides, m_Bonus;

		public int Count
		{
			get => m_Count;
			set => m_Count = value;
		}

		public int Sides
		{
			get => m_Sides;
			set => m_Sides = value;
		}

		public int Bonus
		{
			get => m_Bonus;
			set => m_Bonus = value;
		}

		public int Roll()
		{
			var v = m_Bonus;

			for (var i = 0; i < m_Count; ++i)
			{
				v += Utility.Random(1, m_Sides);
			}

			return v;
		}

		public LootPackDice(string str)
		{
			var start = 0;
			var index = str.IndexOf('d', start);

			if (index < start)
			{
				return;
			}

			m_Count = Utility.ToInt32(str.Substring(start, index - start));

			bool negative;

			start = index + 1;
			index = str.IndexOf('+', start);

			if (negative = (index < start))
			{
				index = str.IndexOf('-', start);
			}

			if (index < start)
			{
				index = str.Length;
			}

			m_Sides = Utility.ToInt32(str.Substring(start, index - start));

			if (index == str.Length)
			{
				return;
			}

			start = index + 1;
			index = str.Length;

			m_Bonus = Utility.ToInt32(str.Substring(start, index - start));

			if (negative)
			{
				m_Bonus *= -1;
			}
		}

		public LootPackDice(int count, int sides, int bonus)
		{
			m_Count = count;
			m_Sides = sides;
			m_Bonus = bonus;
		}
	}
}