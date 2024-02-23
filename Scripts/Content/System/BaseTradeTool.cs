using Server.ContextMenus;
using Server.Engine.Facet;
using Server.Engine.Facet.Module.LumberHarvest;
using Server.Engines.Craft;
using Server.Engines.Harvest;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Server
{
	[AttributeUsage(AttributeTargets.Class)]
	public class FurnitureAttribute : Attribute
	{
		public static bool Check(Item item)
		{
			return item != null && IsDefined(item.GetType(), typeof(FurnitureAttribute), false);
		}
	}
}

namespace Server.Items
{
	public abstract class BaseTool : Item, IUsesRemaining, ICraftable, ICraftTool, IQuality
	{
		private Mobile m_Crafter;
		private ItemQuality m_Quality;
		private int m_UsesRemaining;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get => m_Crafter;
			set { m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public ItemQuality Quality
		{
			get => m_Quality;
			set { UnscaleUses(); m_Quality = value; InvalidateProperties(); ScaleUses(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get => m_UsesRemaining;
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		public virtual bool BreakOnDepletion => true;

		public void ScaleUses()
		{
			m_UsesRemaining = (m_UsesRemaining * GetUsesScalar()) / 100;
			InvalidateProperties();
		}

		public void UnscaleUses()
		{
			m_UsesRemaining = (m_UsesRemaining * 100) / GetUsesScalar();
		}

		public int GetUsesScalar()
		{
			if (m_Quality == ItemQuality.Exceptional)
			{
				return 200;
			}

			return 100;
		}

		public bool ShowUsesRemaining { get => true; set { } }

		public abstract CraftSystem CraftSystem { get; }

		ICraftSystem ICraftTool.CraftSystem => CraftSystem;

		public BaseTool(int itemID) : this(Utility.RandomMinMax(25, 75), itemID)
		{
		}

		public BaseTool(int uses, int itemID) : base(itemID)
		{
			m_UsesRemaining = uses;
			m_Quality = ItemQuality.Regular;
		}

		public BaseTool(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			// Makers mark not displayed on OSI
			//if ( m_Crafter != null )
			//	list.Add( 1050043, m_Crafter.Name ); // crafted by ~1_NAME~

			if (m_Quality == ItemQuality.Exceptional)
			{
				list.Add(1060636); // exceptional
			}

			list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~
		}

		public virtual void DisplayDurabilityTo(Mobile m)
		{
			LabelToAffix(m, 1017323, AffixType.Append, ": " + m_UsesRemaining.ToString()); // Durability
		}

		public static bool CheckAccessible(Item tool, Mobile m)
		{
			return (tool.IsChildOf(m) || tool.Parent == m);
		}

		public static bool CheckTool(Item tool, Mobile m)
		{
			var check = m.FindItemOnLayer(Layer.OneHanded);

			if (check is BaseTool && check != tool && !(check is AncientSmithyHammer))
			{
				return false;
			}

			check = m.FindItemOnLayer(Layer.TwoHanded);

			if (check is BaseTool && check != tool && !(check is AncientSmithyHammer))
			{
				return false;
			}

			return true;
		}

		public override void OnSingleClick(Mobile from)
		{
			DisplayDurabilityTo(from);

			base.OnSingleClick(from);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack) || Parent == from)
			{
				var system = CraftSystem;

				var num = system.CanCraft(from, this, null);

				if (num > 0 && (num != 1044267 || !Core.SE)) // Blacksmithing shows the gump regardless of proximity of an anvil and forge after SE
				{
					from.SendLocalizedMessage(num);
				}
				else
				{
					var context = system.GetContext(from);

					from.SendGump(new CraftGump(from, system, this, null));
				}
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_Crafter);
			writer.Write((int)m_Quality);

			writer.Write(m_UsesRemaining);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Crafter = reader.ReadMobile();
						m_Quality = (ItemQuality)reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						m_UsesRemaining = reader.ReadInt();
						break;
					}
			}
		}

		#region ICraftable

		public virtual int OnCraft(int quality, bool makersMark, Mobile from, ICraftSystem craftSystem, Type typeRes, ICraftTool tool, ICraftItem craftItem, int resHue)
		{
			Quality = (ItemQuality)quality;

			if (makersMark)
			{
				Crafter = from;
			}

			return quality;
		}

		#endregion
	}

	public abstract class BaseRunicTool : BaseTool
	{
		private CraftResource m_Resource;

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get => m_Resource;
			set { m_Resource = value; Hue = CraftResources.GetHue(m_Resource); InvalidateProperties(); }
		}

		public BaseRunicTool(CraftResource resource, int itemID) : base(itemID)
		{
			m_Resource = resource;
		}

		public BaseRunicTool(CraftResource resource, int uses, int itemID) : base(uses, itemID)
		{
			m_Resource = resource;
		}

		public BaseRunicTool(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
			writer.Write((int)m_Resource);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Resource = (CraftResource)reader.ReadInt();
						break;
					}
			}
		}

		private static bool m_IsRunicTool;
		private static int m_LuckChance;

		private static int Scale(int min, int max, int low, int high)
		{
			int percent;

			if (m_IsRunicTool)
			{
				percent = Utility.RandomMinMax(min, max);
			}
			else
			{
				// Behold, the worst system ever!
				var v = Utility.RandomMinMax(0, 10000);

				v = (int)Math.Sqrt(v);
				v = 100 - v;

				if (LootPack.CheckLuck(m_LuckChance))
				{
					v += 10;
				}

				if (v < min)
				{
					v = min;
				}
				else if (v > max)
				{
					v = max;
				}

				percent = v;
			}

			var scaledBy = Math.Abs(high - low) + 1;

			if (scaledBy != 0)
			{
				scaledBy = 10000 / scaledBy;
			}

			percent *= (10000 + scaledBy);

			return low + (((high - low) * percent) / 1000001);
		}

		private static void ApplyAttribute(AosAttributes attrs, int min, int max, AosAttribute attr, int low, int high)
		{
			ApplyAttribute(attrs, min, max, attr, low, high, 1);
		}

		private static void ApplyAttribute(AosAttributes attrs, int min, int max, AosAttribute attr, int low, int high, int scale)
		{
			if (attr == AosAttribute.CastSpeed)
			{
				attrs[attr] += Scale(min, max, low / scale, high / scale) * scale;
			}
			else
			{
				attrs[attr] = Scale(min, max, low / scale, high / scale) * scale;
			}

			if (attr == AosAttribute.SpellChanneling)
			{
				attrs[AosAttribute.CastSpeed] -= 1;
			}
		}

		private static void ApplyAttribute(AosArmorAttributes attrs, int min, int max, AosArmorAttribute attr, int low, int high)
		{
			attrs[attr] = Scale(min, max, low, high);
		}

		private static void ApplyAttribute(AosArmorAttributes attrs, int min, int max, AosArmorAttribute attr, int low, int high, int scale)
		{
			attrs[attr] = Scale(min, max, low / scale, high / scale) * scale;
		}

		private static void ApplyAttribute(AosWeaponAttributes attrs, int min, int max, AosWeaponAttribute attr, int low, int high)
		{
			attrs[attr] = Scale(min, max, low, high);
		}

		private static void ApplyAttribute(AosWeaponAttributes attrs, int min, int max, AosWeaponAttribute attr, int low, int high, int scale)
		{
			attrs[attr] = Scale(min, max, low / scale, high / scale) * scale;
		}

		private static void ApplyAttribute(AosElementAttributes attrs, int min, int max, AosElementAttribute attr, int low, int high)
		{
			attrs[attr] = Scale(min, max, low, high);
		}

		private static void ApplyAttribute(AosElementAttributes attrs, int min, int max, AosElementAttribute attr, int low, int high, int scale)
		{
			attrs[attr] = Scale(min, max, low / scale, high / scale) * scale;
		}

		private static readonly SkillName[] m_PossibleBonusSkills = new SkillName[]
			{
				SkillName.Swords,
				SkillName.Fencing,
				SkillName.Macing,
				SkillName.Archery,
				SkillName.Wrestling,
				SkillName.Parry,
				SkillName.Tactics,
				SkillName.Anatomy,
				SkillName.Healing,
				SkillName.Magery,
				SkillName.Meditation,
				SkillName.EvalInt,
				SkillName.MagicResist,
				SkillName.AnimalTaming,
				SkillName.AnimalLore,
				SkillName.Veterinary,
				SkillName.Musicianship,
				SkillName.Provocation,
				SkillName.Discordance,
				SkillName.Peacemaking,
				SkillName.Chivalry,
				SkillName.Focus,
				SkillName.Necromancy,
				SkillName.Stealing,
				SkillName.Stealth,
				SkillName.SpiritSpeak,
				SkillName.Bushido,
				SkillName.Ninjitsu
			};

		private static readonly SkillName[] m_PossibleSpellbookSkills = new SkillName[]
			{
				SkillName.Magery,
				SkillName.Meditation,
				SkillName.EvalInt,
				SkillName.MagicResist
			};

		private static void ApplySkillBonus(AosSkillBonuses attrs, int min, int max, int index, int low, int high)
		{
			var possibleSkills = new List<SkillName>(attrs.Owner is Spellbook ? m_PossibleSpellbookSkills : m_PossibleBonusSkills);
			var count = (Core.SE ? possibleSkills.Count : possibleSkills.Count - 2);

			SkillName sk, check;
			double bonus;
			bool found;

			do
			{
				found = false;
				sk = possibleSkills[Utility.Random(count--)];
				possibleSkills.Remove(sk);

				for (var i = 0; !found && i < 5; ++i)
				{
					found = (attrs.GetValues(i, out check, out bonus) && check == sk);
				}
			} while (found && count > 0);

			attrs.SetValues(index, sk, Scale(min, max, low, high));
		}

		private static void ApplyResistance(BaseArmor ar, int min, int max, ResistanceType res, int low, int high)
		{
			switch (res)
			{
				case ResistanceType.Physical: ar.PhysicalBonus += Scale(min, max, low, high); break;
				case ResistanceType.Fire: ar.FireBonus += Scale(min, max, low, high); break;
				case ResistanceType.Cold: ar.ColdBonus += Scale(min, max, low, high); break;
				case ResistanceType.Poison: ar.PoisonBonus += Scale(min, max, low, high); break;
				case ResistanceType.Energy: ar.EnergyBonus += Scale(min, max, low, high); break;
			}
		}

		private const int MaxProperties = 32;
		private static readonly BitArray m_Props = new BitArray(MaxProperties);
		private static readonly int[] m_Possible = new int[MaxProperties];

		public static int GetUniqueRandom(int count)
		{
			var avail = 0;

			for (var i = 0; i < count; ++i)
			{
				if (!m_Props[i])
				{
					m_Possible[avail++] = i;
				}
			}

			if (avail == 0)
			{
				return -1;
			}

			var v = m_Possible[Utility.Random(avail)];

			m_Props.Set(v, true);

			return v;
		}

		public void ApplyAttributesTo(BaseWeapon weapon)
		{
			var resInfo = CraftResources.GetInfo(m_Resource);

			if (resInfo == null)
			{
				return;
			}

			var attrs = resInfo.AttributeInfo;

			if (attrs == null)
			{
				return;
			}

			var attributeCount = Utility.RandomMinMax(attrs.RunicMinAttributes, attrs.RunicMaxAttributes);
			var min = attrs.RunicMinIntensity;
			var max = attrs.RunicMaxIntensity;

			ApplyAttributesTo(weapon, true, 0, attributeCount, min, max);
		}

		public static void ApplyAttributesTo(BaseWeapon weapon, int attributeCount, int min, int max)
		{
			ApplyAttributesTo(weapon, false, 0, attributeCount, min, max);
		}

		public static void ApplyAttributesTo(BaseWeapon weapon, bool isRunicTool, int luckChance, int attributeCount, int min, int max)
		{
			m_IsRunicTool = isRunicTool;
			m_LuckChance = luckChance;

			var primary = weapon.Attributes;
			var secondary = weapon.WeaponAttributes;

			m_Props.SetAll(false);

			if (weapon is BaseRanged)
			{
				m_Props.Set(2, true); // ranged weapons cannot be ubws or mageweapon
			}

			for (var i = 0; i < attributeCount; ++i)
			{
				var random = GetUniqueRandom(25);

				if (random == -1)
				{
					break;
				}

				switch (random)
				{
					case 0:
						{
							switch (Utility.Random(5))
							{
								case 0: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitPhysicalArea, 2, 50, 2); break;
								case 1: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitFireArea, 2, 50, 2); break;
								case 2: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitColdArea, 2, 50, 2); break;
								case 3: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitPoisonArea, 2, 50, 2); break;
								case 4: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitEnergyArea, 2, 50, 2); break;
							}

							break;
						}
					case 1:
						{
							switch (Utility.Random(4))
							{
								case 0: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitMagicArrow, 2, 50, 2); break;
								case 1: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitHarm, 2, 50, 2); break;
								case 2: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitFireball, 2, 50, 2); break;
								case 3: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitLightning, 2, 50, 2); break;
							}

							break;
						}
					case 2:
						{
							switch (Utility.Random(2))
							{
								case 0: ApplyAttribute(secondary, min, max, AosWeaponAttribute.UseBestSkill, 1, 1); break;
								case 1: ApplyAttribute(secondary, min, max, AosWeaponAttribute.MageWeapon, 1, 10); break;
							}

							break;
						}
					case 3: ApplyAttribute(primary, min, max, AosAttribute.WeaponDamage, 1, 50); break;
					case 4: ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 15); break;
					case 5: ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 1); break;
					case 6: ApplyAttribute(primary, min, max, AosAttribute.AttackChance, 1, 15); break;
					case 7: ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 100); break;
					case 8: ApplyAttribute(primary, min, max, AosAttribute.WeaponSpeed, 5, 30, 5); break;
					case 9: ApplyAttribute(primary, min, max, AosAttribute.SpellChanneling, 1, 1); break;
					case 10: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitDispel, 2, 50, 2); break;
					case 11: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitLeechHits, 2, 50, 2); break;
					case 12: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitLowerAttack, 2, 50, 2); break;
					case 13: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitLowerDefend, 2, 50, 2); break;
					case 14: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitLeechMana, 2, 50, 2); break;
					case 15: ApplyAttribute(secondary, min, max, AosWeaponAttribute.HitLeechStam, 2, 50, 2); break;
					case 16: ApplyAttribute(secondary, min, max, AosWeaponAttribute.LowerStatReq, 10, 100, 10); break;
					case 17: ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistPhysicalBonus, 1, 15); break;
					case 18: ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistFireBonus, 1, 15); break;
					case 19: ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistColdBonus, 1, 15); break;
					case 20: ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistPoisonBonus, 1, 15); break;
					case 21: ApplyAttribute(secondary, min, max, AosWeaponAttribute.ResistEnergyBonus, 1, 15); break;
					case 22: ApplyAttribute(secondary, min, max, AosWeaponAttribute.DurabilityBonus, 10, 100, 10); break;
					case 23: weapon.Slayer = GetRandomSlayer(); break;
					case 24: GetElementalDamages(weapon); break;
				}
			}
		}

		public static void GetElementalDamages(BaseWeapon weapon)
		{
			GetElementalDamages(weapon, true);
		}

		public static void GetElementalDamages(BaseWeapon weapon, bool randomizeOrder)
		{
			int fire, phys, cold, nrgy, pois, chaos, direct;

			weapon.GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

			var totalDamage = phys;

			var attrs = new AosElementAttribute[]
			{
				AosElementAttribute.Cold,
				AosElementAttribute.Energy,
				AosElementAttribute.Fire,
				AosElementAttribute.Poison
			};

			if (randomizeOrder)
			{
				for (var i = 0; i < attrs.Length; i++)
				{
					var rand = Utility.Random(attrs.Length);
					var temp = attrs[i];

					attrs[i] = attrs[rand];
					attrs[rand] = temp;
				}
			}


			/*
			totalDamage = AssignElementalDamage( weapon, AosElementAttribute.Cold,		totalDamage );
			totalDamage = AssignElementalDamage( weapon, AosElementAttribute.Energy,	totalDamage );
			totalDamage = AssignElementalDamage( weapon, AosElementAttribute.Fire,		totalDamage );
			totalDamage = AssignElementalDamage( weapon, AosElementAttribute.Poison,	totalDamage );

			weapon.AosElementDamages[AosElementAttribute.Physical] = 100 - totalDamage;
			 * */

			for (var i = 0; i < attrs.Length; i++)
			{
				totalDamage = AssignElementalDamage(weapon, attrs[i], totalDamage);
			}


			//Order is Cold, Energy, Fire, Poison -> Physical left
			//Cannot be looped, AoselementAttribute is 'out of order'

			weapon.Hue = weapon.GetElementalDamageHue();
		}

		private static int AssignElementalDamage(BaseWeapon weapon, AosElementAttribute attr, int totalDamage)
		{
			if (totalDamage <= 0)
			{
				return 0;
			}

			var random = Utility.Random(totalDamage / 10 + 1) * 10;
			weapon.AosElementDamages[attr] = random;

			return (totalDamage - random);
		}

		public static SlayerName GetRandomSlayer()
		{
			// TODO: Check random algorithm on OSI

			var groups = SlayerGroup.Groups;

			if (groups.Length == 0)
			{
				return SlayerName.None;
			}

			var group = groups[Utility.Random(groups.Length - 1)]; //-1 To Exclude the Fey Slayer which appears ONLY on a certain artifact.
			SlayerEntry entry;

			if (10 > Utility.Random(100)) // 10% chance to do super slayer
			{
				entry = group.Super;
			}
			else
			{
				var entries = group.Entries;

				if (entries.Length == 0)
				{
					return SlayerName.None;
				}

				entry = entries[Utility.Random(entries.Length)];
			}

			return entry.Name;
		}

		public void ApplyAttributesTo(BaseArmor armor)
		{
			var resInfo = CraftResources.GetInfo(m_Resource);

			if (resInfo == null)
			{
				return;
			}

			var attrs = resInfo.AttributeInfo;

			if (attrs == null)
			{
				return;
			}

			var attributeCount = Utility.RandomMinMax(attrs.RunicMinAttributes, attrs.RunicMaxAttributes);
			var min = attrs.RunicMinIntensity;
			var max = attrs.RunicMaxIntensity;

			ApplyAttributesTo(armor, true, 0, attributeCount, min, max);
		}

		public static void ApplyAttributesTo(BaseArmor armor, int attributeCount, int min, int max)
		{
			ApplyAttributesTo(armor, false, 0, attributeCount, min, max);
		}

		public static void ApplyAttributesTo(BaseArmor armor, bool isRunicTool, int luckChance, int attributeCount, int min, int max)
		{
			m_IsRunicTool = isRunicTool;
			m_LuckChance = luckChance;

			var primary = armor.Attributes;
			var secondary = armor.ArmorAttributes;

			m_Props.SetAll(false);

			var isShield = (armor is BaseShield);
			var baseCount = (isShield ? 7 : 20);
			var baseOffset = (isShield ? 0 : 4);

			if (!isShield && armor.MeditationAllowance == ArmorMeditationAllowance.All)
			{
				m_Props.Set(3, true); // remove mage armor from possible properties
			}

			if (armor.Resource >= CraftResource.RegularLeather && armor.Resource <= CraftResource.BarbedLeather)
			{
				m_Props.Set(0, true); // remove lower requirements from possible properties for leather armor
				m_Props.Set(2, true); // remove durability bonus from possible properties
			}
			if (armor.RequiredRace == Race.Elf)
			{
				m_Props.Set(7, true); // elves inherently have night sight and elf only armor doesn't get night sight as a mod
			}

			for (var i = 0; i < attributeCount; ++i)
			{
				var random = GetUniqueRandom(baseCount);

				if (random == -1)
				{
					break;
				}

				random += baseOffset;

				switch (random)
				{
					/* Begin Sheilds */
					case 0: ApplyAttribute(primary, min, max, AosAttribute.SpellChanneling, 1, 1); break;
					case 1: ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 15); break;
					case 2:
						if (Core.ML)
						{
							ApplyAttribute(primary, min, max, AosAttribute.ReflectPhysical, 1, 15);
						}
						else
						{
							ApplyAttribute(primary, min, max, AosAttribute.AttackChance, 1, 15);
						}
						break;
					case 3: ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 1); break;
					/* Begin Armor */
					case 4: ApplyAttribute(secondary, min, max, AosArmorAttribute.LowerStatReq, 10, 100, 10); break;
					case 5: ApplyAttribute(secondary, min, max, AosArmorAttribute.SelfRepair, 1, 5); break;
					case 6: ApplyAttribute(secondary, min, max, AosArmorAttribute.DurabilityBonus, 10, 100, 10); break;
					/* End Shields */
					case 7: ApplyAttribute(secondary, min, max, AosArmorAttribute.MageArmor, 1, 1); break;
					case 8: ApplyAttribute(primary, min, max, AosAttribute.RegenHits, 1, 2); break;
					case 9: ApplyAttribute(primary, min, max, AosAttribute.RegenStam, 1, 3); break;
					case 10: ApplyAttribute(primary, min, max, AosAttribute.RegenMana, 1, 2); break;
					case 11: ApplyAttribute(primary, min, max, AosAttribute.NightSight, 1, 1); break;
					case 12: ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 5); break;
					case 13: ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 8); break;
					case 14: ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 8); break;
					case 15: ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 8); break;
					case 16: ApplyAttribute(primary, min, max, AosAttribute.LowerRegCost, 1, 20); break;
					case 17: ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 100); break;
					case 18: ApplyAttribute(primary, min, max, AosAttribute.ReflectPhysical, 1, 15); break;
					case 19: ApplyResistance(armor, min, max, ResistanceType.Physical, 1, 15); break;
					case 20: ApplyResistance(armor, min, max, ResistanceType.Fire, 1, 15); break;
					case 21: ApplyResistance(armor, min, max, ResistanceType.Cold, 1, 15); break;
					case 22: ApplyResistance(armor, min, max, ResistanceType.Poison, 1, 15); break;
					case 23: ApplyResistance(armor, min, max, ResistanceType.Energy, 1, 15); break;
						/* End Armor */
				}
			}
		}

		public static void ApplyAttributesTo(BaseHat hat, int attributeCount, int min, int max)
		{
			ApplyAttributesTo(hat, false, 0, attributeCount, min, max);
		}

		public static void ApplyAttributesTo(BaseHat hat, bool isRunicTool, int luckChance, int attributeCount, int min, int max)
		{
			m_IsRunicTool = isRunicTool;
			m_LuckChance = luckChance;

			var primary = hat.Attributes;
			var secondary = hat.ClothingAttributes;
			var resists = hat.Resistances;

			m_Props.SetAll(false);

			for (var i = 0; i < attributeCount; ++i)
			{
				var random = GetUniqueRandom(19);

				if (random == -1)
				{
					break;
				}

				switch (random)
				{
					case 0: ApplyAttribute(primary, min, max, AosAttribute.ReflectPhysical, 1, 15); break;
					case 1: ApplyAttribute(primary, min, max, AosAttribute.RegenHits, 1, 2); break;
					case 2: ApplyAttribute(primary, min, max, AosAttribute.RegenStam, 1, 3); break;
					case 3: ApplyAttribute(primary, min, max, AosAttribute.RegenMana, 1, 2); break;
					case 4: ApplyAttribute(primary, min, max, AosAttribute.NightSight, 1, 1); break;
					case 5: ApplyAttribute(primary, min, max, AosAttribute.BonusHits, 1, 5); break;
					case 6: ApplyAttribute(primary, min, max, AosAttribute.BonusStam, 1, 8); break;
					case 7: ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 8); break;
					case 8: ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 8); break;
					case 9: ApplyAttribute(primary, min, max, AosAttribute.LowerRegCost, 1, 20); break;
					case 10: ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 100); break;
					case 11: ApplyAttribute(secondary, min, max, AosArmorAttribute.LowerStatReq, 10, 100, 10); break;
					case 12: ApplyAttribute(secondary, min, max, AosArmorAttribute.SelfRepair, 1, 5); break;
					case 13: ApplyAttribute(secondary, min, max, AosArmorAttribute.DurabilityBonus, 10, 100, 10); break;
					case 14: ApplyAttribute(resists, min, max, AosElementAttribute.Physical, 1, 15); break;
					case 15: ApplyAttribute(resists, min, max, AosElementAttribute.Fire, 1, 15); break;
					case 16: ApplyAttribute(resists, min, max, AosElementAttribute.Cold, 1, 15); break;
					case 17: ApplyAttribute(resists, min, max, AosElementAttribute.Poison, 1, 15); break;
					case 18: ApplyAttribute(resists, min, max, AosElementAttribute.Energy, 1, 15); break;
				}
			}
		}

		public static void ApplyAttributesTo(BaseJewel jewelry, int attributeCount, int min, int max)
		{
			ApplyAttributesTo(jewelry, false, 0, attributeCount, min, max);
		}

		public static void ApplyAttributesTo(BaseJewel jewelry, bool isRunicTool, int luckChance, int attributeCount, int min, int max)
		{
			m_IsRunicTool = isRunicTool;
			m_LuckChance = luckChance;

			var primary = jewelry.Attributes;
			var resists = jewelry.Resistances;
			var skills = jewelry.SkillBonuses;

			m_Props.SetAll(false);

			for (var i = 0; i < attributeCount; ++i)
			{
				var random = GetUniqueRandom(24);

				if (random == -1)
				{
					break;
				}

				switch (random)
				{
					case 0: ApplyAttribute(resists, min, max, AosElementAttribute.Physical, 1, 15); break;
					case 1: ApplyAttribute(resists, min, max, AosElementAttribute.Fire, 1, 15); break;
					case 2: ApplyAttribute(resists, min, max, AosElementAttribute.Cold, 1, 15); break;
					case 3: ApplyAttribute(resists, min, max, AosElementAttribute.Poison, 1, 15); break;
					case 4: ApplyAttribute(resists, min, max, AosElementAttribute.Energy, 1, 15); break;
					case 5: ApplyAttribute(primary, min, max, AosAttribute.WeaponDamage, 1, 25); break;
					case 6: ApplyAttribute(primary, min, max, AosAttribute.DefendChance, 1, 15); break;
					case 7: ApplyAttribute(primary, min, max, AosAttribute.AttackChance, 1, 15); break;
					case 8: ApplyAttribute(primary, min, max, AosAttribute.BonusStr, 1, 8); break;
					case 9: ApplyAttribute(primary, min, max, AosAttribute.BonusDex, 1, 8); break;
					case 10: ApplyAttribute(primary, min, max, AosAttribute.BonusInt, 1, 8); break;
					case 11: ApplyAttribute(primary, min, max, AosAttribute.EnhancePotions, 5, 25, 5); break;
					case 12: ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 1); break;
					case 13: ApplyAttribute(primary, min, max, AosAttribute.CastRecovery, 1, 3); break;
					case 14: ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 8); break;
					case 15: ApplyAttribute(primary, min, max, AosAttribute.LowerRegCost, 1, 20); break;
					case 16: ApplyAttribute(primary, min, max, AosAttribute.Luck, 1, 100); break;
					case 17: ApplyAttribute(primary, min, max, AosAttribute.SpellDamage, 1, 12); break;
					case 18: ApplyAttribute(primary, min, max, AosAttribute.NightSight, 1, 1); break;
					case 19: ApplySkillBonus(skills, min, max, 0, 1, 15); break;
					case 20: ApplySkillBonus(skills, min, max, 1, 1, 15); break;
					case 21: ApplySkillBonus(skills, min, max, 2, 1, 15); break;
					case 22: ApplySkillBonus(skills, min, max, 3, 1, 15); break;
					case 23: ApplySkillBonus(skills, min, max, 4, 1, 15); break;
				}
			}
		}

		public static void ApplyAttributesTo(Spellbook spellbook, int attributeCount, int min, int max)
		{
			ApplyAttributesTo(spellbook, false, 0, attributeCount, min, max);
		}

		public static void ApplyAttributesTo(Spellbook spellbook, bool isRunicTool, int luckChance, int attributeCount, int min, int max)
		{
			m_IsRunicTool = isRunicTool;
			m_LuckChance = luckChance;

			var primary = spellbook.Attributes;
			var skills = spellbook.SkillBonuses;

			m_Props.SetAll(false);

			for (var i = 0; i < attributeCount; ++i)
			{
				var random = GetUniqueRandom(16);

				if (random == -1)
				{
					break;
				}

				switch (random)
				{
					case 0:
					case 1:
					case 2:
					case 3:
						{
							ApplyAttribute(primary, min, max, AosAttribute.BonusInt, 1, 8);

							for (var j = 0; j < 4; ++j)
							{
								m_Props.Set(j, true);
							}

							break;
						}
					case 4: ApplyAttribute(primary, min, max, AosAttribute.BonusMana, 1, 8); break;
					case 5: ApplyAttribute(primary, min, max, AosAttribute.CastSpeed, 1, 1); break;
					case 6: ApplyAttribute(primary, min, max, AosAttribute.CastRecovery, 1, 3); break;
					case 7: ApplyAttribute(primary, min, max, AosAttribute.SpellDamage, 1, 12); break;
					case 8: ApplySkillBonus(skills, min, max, 0, 1, 15); break;
					case 9: ApplySkillBonus(skills, min, max, 1, 1, 15); break;
					case 10: ApplySkillBonus(skills, min, max, 2, 1, 15); break;
					case 11: ApplySkillBonus(skills, min, max, 3, 1, 15); break;
					case 12: ApplyAttribute(primary, min, max, AosAttribute.LowerRegCost, 1, 20); break;
					case 13: ApplyAttribute(primary, min, max, AosAttribute.LowerManaCost, 1, 8); break;
					case 14: ApplyAttribute(primary, min, max, AosAttribute.RegenMana, 1, 2); break;
					case 15: spellbook.Slayer = GetRandomSlayer(); break;
				}
			}
		}
	}

	public abstract class BaseHarvestTool : Item, IUsesRemaining, ICraftable, IHarvestTool, IQuality
	{
		private Mobile m_Crafter;
		private ItemQuality m_Quality;
		private int m_UsesRemaining;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get => m_Crafter;
			set { m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public ItemQuality Quality
		{
			get => m_Quality;
			set { UnscaleUses(); m_Quality = value; InvalidateProperties(); ScaleUses(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int UsesRemaining
		{
			get => m_UsesRemaining;
			set { m_UsesRemaining = value; InvalidateProperties(); }
		}

		public void ScaleUses()
		{
			m_UsesRemaining = (m_UsesRemaining * GetUsesScalar()) / 100;
			InvalidateProperties();
		}

		public void UnscaleUses()
		{
			m_UsesRemaining = (m_UsesRemaining * 100) / GetUsesScalar();
		}

		public int GetUsesScalar()
		{
			if (m_Quality == ItemQuality.Exceptional)
			{
				return 200;
			}

			return 100;
		}

		public bool ShowUsesRemaining { get => true; set { } }

		public abstract HarvestSystem HarvestSystem { get; }

		IHarvestSystem IHarvestTool.HarvestSystem => HarvestSystem;

		public BaseHarvestTool(int itemID) : this(50, itemID)
		{
		}

		public BaseHarvestTool(int usesRemaining, int itemID) : base(itemID)
		{
			m_UsesRemaining = usesRemaining;
			m_Quality = ItemQuality.Regular;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			// Makers mark not displayed on OSI
			//if ( m_Crafter != null )
			//	list.Add( 1050043, m_Crafter.Name ); // crafted by ~1_NAME~

			if (m_Quality == ItemQuality.Exceptional)
			{
				list.Add(1060636); // exceptional
			}

			list.Add(1060584, m_UsesRemaining.ToString()); // uses remaining: ~1_val~
		}

		public virtual void DisplayDurabilityTo(Mobile m)
		{
			LabelToAffix(m, 1017323, AffixType.Append, ": " + m_UsesRemaining.ToString()); // Durability
		}

		public override void OnSingleClick(Mobile from)
		{
			DisplayDurabilityTo(from);

			base.OnSingleClick(from);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack) || Parent == from)
			{
				HarvestSystem.BeginHarvesting(from, this);
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			AddContextMenuEntries(from, this, list, HarvestSystem);
		}

		public static void AddContextMenuEntries(Mobile from, Item item, List<ContextMenuEntry> list, HarvestSystem system)
		{
			if (system != Mining.System)
			{
				return;
			}

			if (!item.IsChildOf(from.Backpack) && item.Parent != from)
			{
				return;
			}

			var pm = from as PlayerMobile;

			if (pm == null)
			{
				return;
			}

			var miningEntry = new ContextMenuEntry(pm.ToggleMiningStone ? 6179 : 6178) {
				Color = 0x421F
			};
			list.Add(miningEntry);

			list.Add(new ToggleMiningStoneEntry(pm, false, 6176));
			list.Add(new ToggleMiningStoneEntry(pm, true, 6177));
		}

		private class ToggleMiningStoneEntry : ContextMenuEntry
		{
			private readonly PlayerMobile m_Mobile;
			private readonly bool m_Value;

			public ToggleMiningStoneEntry(PlayerMobile mobile, bool value, int number) : base(number)
			{
				m_Mobile = mobile;
				m_Value = value;

				var stoneMining = (mobile.StoneMining && mobile.Skills[SkillName.Mining].Base >= 100.0);

				if (mobile.ToggleMiningStone == value || (value && !stoneMining))
				{
					Flags |= CMEFlags.Disabled;
				}
			}

			public override void OnClick()
			{
				var oldValue = m_Mobile.ToggleMiningStone;

				if (m_Value)
				{
					if (oldValue)
					{
						m_Mobile.SendLocalizedMessage(1054023); // You are already set to mine both ore and stone!
					}
					else if (!m_Mobile.StoneMining || m_Mobile.Skills[SkillName.Mining].Base < 100.0)
					{
						m_Mobile.SendLocalizedMessage(1054024); // You have not learned how to mine stone or you do not have enough skill!
					}
					else
					{
						m_Mobile.ToggleMiningStone = true;
						m_Mobile.SendLocalizedMessage(1054022); // You are now set to mine both ore and stone.
					}
				}
				else
				{
					if (oldValue)
					{
						m_Mobile.ToggleMiningStone = false;
						m_Mobile.SendLocalizedMessage(1054020); // You are now set to mine only ore.
					}
					else
					{
						m_Mobile.SendLocalizedMessage(1054021); // You are already set to mine only ore!
					}
				}
			}
		}

		public BaseHarvestTool(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_Crafter);
			writer.Write((int)m_Quality);

			writer.Write(m_UsesRemaining);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Crafter = reader.ReadMobile();
						m_Quality = (ItemQuality)reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						m_UsesRemaining = reader.ReadInt();
						break;
					}
			}
		}

		#region ICraftable

		public virtual int OnCraft(int quality, bool makersMark, Mobile from, ICraftSystem craftSystem, Type typeRes, ICraftTool tool, ICraftItem craftItem, int resHue)
		{
			Quality = (ItemQuality)quality;

			if (makersMark)
			{
				Crafter = from;
			}

			return quality;
		}

		#endregion
	}
}

namespace Server.Targets
{
	public class BladedItemTarget : Target
	{
		private readonly Item m_Item;

		public BladedItemTarget(Item item) : base(2, false, TargetFlags.None)
		{
			m_Item = item;
		}

		protected override void OnTargetOutOfRange(Mobile from, object targeted)
		{
			if (targeted is UnholyBone && from.InRange(((UnholyBone)targeted), 12))
			{
				((UnholyBone)targeted).Carve(from, m_Item);
			}
			else
			{
				base.OnTargetOutOfRange(from, targeted);
			}
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (m_Item.Deleted)
			{
				return;
			}

			if (targeted is ICarvable)
			{
				((ICarvable)targeted).Carve(from, m_Item);
			}
			else if (targeted is SwampDragon && ((SwampDragon)targeted).HasBarding)
			{
				var pet = (SwampDragon)targeted;

				if (!pet.Controlled || pet.ControlMaster != from)
				{
					from.SendLocalizedMessage(1053022); // You cannot remove barding from a swamp dragon you do not own.
				}
				else
				{
					pet.HasBarding = false;
				}
			}
			else
			{
				var tool = m_Item as IHarvestTool;

				var system = tool?.HarvestSystem as HarvestSystem ?? Lumberjacking.System;

				if (system == null)
				{
					return;
				}

				if (!system.GetHarvestDetails(from, tool, targeted, out var tileID, out var map, out var loc))
				{
					from.SendLocalizedMessage(500494); // You can't use a bladed item on that!
					return;
				}

				var def = system.GetDefinition(tileID);

				if (def == null)
				{
					from.SendLocalizedMessage(500494); // You can't use a bladed item on that!
					return;
				}

				var bank = def.GetBank(map, loc.X, loc.Y);

				if (bank == null)
				{
					return;
				}

				if (bank.Current < 5)
				{
					from.SendLocalizedMessage(500493); // There's not enough wood here to harvest.
					return;
				}

				bank.Consume(5, from);

				var item = new Kindling();

				if (from.PlaceInBackpack(item))
				{
					from.SendLocalizedMessage(500491); // You put some kindling into your backpack.
					from.SendLocalizedMessage(500492); // An axe would probably get you more wood.
					return;
				}

				from.SendLocalizedMessage(500490); // You can't place any kindling into your backpack!

				item.Delete();
			}
		}
	}
}