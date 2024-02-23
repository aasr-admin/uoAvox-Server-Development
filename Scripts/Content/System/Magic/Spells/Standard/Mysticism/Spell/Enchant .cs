using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Spells.Spellweaving;

using System;
using System.Collections.Generic;

namespace Server.Spells.Mysticism
{
	public class EnchantSpell : MysticismSpell
	{
		private static readonly string ModName = "EnchantAttribute";

		public static readonly Dictionary<Mobile, EnchantmentTimer> Table = new();

		public override bool ClearHandsOnCast => false;

		public BaseWeapon Weapon { get; set; }
		public AosWeaponAttribute Attribute { get; set; }

		public EnchantSpell(Mobile caster, Item scroll)
			: this(caster, scroll, null, AosWeaponAttribute.HitLightning)
		{
		}

		public EnchantSpell(Mobile caster, Item scroll, BaseWeapon weapon, AosWeaponAttribute attribute)
			: base(caster, scroll, MysticismSpellName.Enchant)
		{
			Weapon = weapon;
			Attribute = attribute;
		}

		public override bool CheckCast()
		{
			if (Weapon == null)
			{
				if (Caster.Weapon is not BaseWeapon wep)
				{
					Caster.SendLocalizedMessage(501078); // You must be holding a weapon.
				}
				else
				{
					_ = Caster.CloseGump(typeof(EnchantSpellGump));

					var gump = new EnchantSpellGump(Caster, Scroll, wep);
					var serial = gump.Serial;

					_ = Caster.SendGump(gump);

					_ = Timer.DelayCall(TimeSpan.FromSeconds(30), () =>
					{
						var current = Caster.FindGump(typeof(EnchantSpellGump));

						if (current != null && current.Serial == serial)
						{
							_ = Caster.CloseGump(typeof(EnchantSpellGump));

							FinishSequence();
						}
					});
				}

				return false;
			}

			if (IsUnderSpellEffects(Caster, Weapon))
			{
				Caster.SendLocalizedMessage(501775); // This spell is already in effect.
				return false;
			}

			if (ImmolatingWeaponSpell.IsImmolating(Weapon) || Weapon.Consecrated)
			{
				Caster.SendLocalizedMessage(1080128); //You cannot use this ability while your weapon is enchanted.
				return false;
			}
			/*
			if (Weapon.FocusWeilder != null)
            {
                Caster.SendLocalizedMessage(1080446); // You cannot enchant an item that is under the effects of the ninjitsu focus attack ability.
                return false;
            }
            */
			if (Weapon.WeaponAttributes.HitLightning > 0 || Weapon.WeaponAttributes.HitFireball > 0 || Weapon.WeaponAttributes.HitHarm > 0 || Weapon.WeaponAttributes.HitMagicArrow > 0 || Weapon.WeaponAttributes.HitDispel > 0)
			{
				Caster.SendLocalizedMessage(1080127); // This weapon already has a hit spell effect and cannot be enchanted.
				return false;
			}

			return true;
		}

		public override void OnCast()
		{
			if (Caster.Weapon is not BaseWeapon wep || wep != Weapon)
			{
				Caster.SendLocalizedMessage(501078); // You must be holding a weapon.
			}
			else if (IsUnderSpellEffects(Caster, Weapon))
			{
				Caster.SendLocalizedMessage(501775); // This spell is already in effect.
			}
			else if (ImmolatingWeaponSpell.IsImmolating(Weapon) || Weapon.Consecrated)
			{
				Caster.SendLocalizedMessage(1080128); //You cannot use this ability while your weapon is enchanted.
			}
			/*else if (Weapon.FocusWeilder != null)
			{
				Caster.SendLocalizedMessage(1080446); // You cannot enchant an item that is under the effects of the ninjitsu focus attack ability.
			}*/
			else if (Weapon.WeaponAttributes.HitLightning > 0 || Weapon.WeaponAttributes.HitFireball > 0 || Weapon.WeaponAttributes.HitHarm > 0 || Weapon.WeaponAttributes.HitMagicArrow > 0 || Weapon.WeaponAttributes.HitDispel > 0)
			{
				Caster.SendLocalizedMessage(1080127); // This weapon already has a hit spell effect and cannot be enchanted.
			}
			else if (CheckSequence())
			{
				Caster.PlaySound(0x64E);
				Caster.FixedEffect(0x36CB, 1, 9, 1915, 0);

				var prim = Caster.Skills[CastSkill].Value;
				var sec = Caster.Skills[DamageSkill].Value;

				var duration = ((prim + sec) / 2.0) + 30.0;
				var value = (int)(60.0 * (prim + sec) / 240.0);

				var malus = 0;

				Enhancement.SetValue(Caster, Attribute, value, ModName);

				if (prim >= 80 && sec >= 80 && Weapon.Attributes.SpellChanneling == 0)
				{
					Enhancement.SetValue(Caster, AosAttribute.SpellChanneling, 1, ModName);
					Enhancement.SetValue(Caster, AosAttribute.CastSpeed, -1, ModName);

					malus = 1;
				}

				var t = new EnchantmentTimer(Caster, Weapon, Attribute, value, malus, duration);

				Table[Caster] = t;

				t.Start();

				var loc = Attribute switch
				{
					AosWeaponAttribute.HitFireball => 1060420,
					AosWeaponAttribute.HitHarm => 1060421,
					AosWeaponAttribute.HitMagicArrow => 1060426,
					AosWeaponAttribute.HitDispel => 1060417,
					_ => 1060423,
				};

				BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.Enchant, 1080126, loc, TimeSpan.FromSeconds(duration), Caster, value.ToString()));

				Weapon.InvalidateProperties();
			}

			FinishSequence();
		}

		public static bool IsUnderSpellEffects(Mobile from, BaseWeapon wep)
		{
			if (Table.TryGetValue(from, out var t))
			{
				return t?.Weapon == wep;
			}

			return false;
		}

		public static AosWeaponAttribute BonusAttribute(Mobile from)
		{
			if (Table.TryGetValue(from, out var t))
			{
				return t?.WeaponAttribute ?? 0;
			}

			return AosWeaponAttribute.HitLightning;
		}

		public static int BonusValue(Mobile from)
		{
			if (Table.TryGetValue(from, out var t))
			{
				return t?.AttributeValue ?? 0;
			}

			return 0;
		}

		public static bool CastingMalus(Mobile from, BaseWeapon weapon)
		{
			if (Table.TryGetValue(from, out var t))
			{
				return t?.CastingMalus > 0;
			}

			return false;
		}

		public static void RemoveEnchantment(Mobile caster)
		{
			if (Table.TryGetValue(caster, out var t))
			{
				t?.Stop();

				_ = Table.Remove(caster);

				caster.SendLocalizedMessage(1115273); // The enchantment on your weapon has expired.
				caster.PlaySound(0x1E6);

				Enhancement.RemoveMobile(caster);
			}
		}

		public static void OnWeaponRemoved(BaseWeapon wep, Mobile from)
		{
			if (IsUnderSpellEffects(from, wep))
			{
				RemoveEnchantment(from);
			}

			wep.InvalidateProperties();
		}
	}

	public class EnchantSpellGump : Gump
	{
		private readonly Mobile m_Caster;
		private readonly Item m_Scroll;
		private readonly BaseWeapon m_Weapon;

		public EnchantSpellGump(Mobile caster, Item scroll, BaseWeapon weapon)
			: base(20, 20)
		{
			m_Caster = caster;
			m_Scroll = scroll;
			m_Weapon = weapon;

			short color16 = 0x07FF;

			AddBackground(0, 0, 260, 187, 3600);
			AddAlphaRegion(5, 15, 242, 170);

			AddImageTiled(220, 15, 30, 162, 10464);

			AddItem(0, 3, 6882);
			AddItem(-8, 170, 6880);
			AddItem(185, 3, 6883);
			AddItem(192, 170, 6881);

			AddHtmlLocalized(20, 22, 150, 16, 1080133, color16, false, false); //Select Enchant

			AddButton(20, 50, 9702, 9703, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 50, 200, 16, 1079705, color16, false, false); //Hit Lighting

			AddButton(20, 75, 9702, 9703, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 75, 200, 16, 1079703, color16, false, false); //Hit Fireball

			AddButton(20, 100, 9702, 9703, 3, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 100, 200, 16, 1079704, color16, false, false); //Hit Harm

			AddButton(20, 125, 9702, 9703, 4, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 125, 200, 16, 1079706, color16, false, false); //Hit Magic Arrow

			AddButton(20, 150, 9702, 9703, 5, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 150, 200, 16, 1079702, color16, false, false); //Hit Dispel
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			AosWeaponAttribute attr;

			switch (info.ButtonID)
			{
				default:
					m_Caster.SendLocalizedMessage(1080132); //You decide not to enchant your weapon.
					return;
				case 1: //Hit Lightning
					{
						attr = AosWeaponAttribute.HitLightning;
						break;
					}
				case 2: //Hit Fireball
					{
						attr = AosWeaponAttribute.HitFireball;
						break;
					}
				case 3: //Hit Harm
					{
						attr = AosWeaponAttribute.HitHarm;
						break;
					}
				case 4: //Hit Magic Arrow
					{
						attr = AosWeaponAttribute.HitMagicArrow;
						break;
					}
				case 5: //Hit Dispel
					{
						attr = AosWeaponAttribute.HitDispel;
						break;
					}
			}

			var spell = new EnchantSpell(m_Caster, m_Scroll, m_Weapon, attr);

			_ = spell.Cast();
		}
	}

	public class EnchantmentTimer : Timer
	{
		public Mobile Owner { get; set; }
		public BaseWeapon Weapon { get; set; }
		public AosWeaponAttribute WeaponAttribute { get; set; }
		public int AttributeValue { get; set; }
		public int CastingMalus { get; set; }

		public EnchantmentTimer(Mobile owner, BaseWeapon wep, AosWeaponAttribute attribute, int value, int malus, double duration)
			: base(TimeSpan.FromSeconds(duration))
		{
			Owner = owner;
			Weapon = wep;
			WeaponAttribute = attribute;
			AttributeValue = value;
			CastingMalus = malus;
		}

		protected override void OnTick()
		{
			EnchantSpell.RemoveEnchantment(Owner);

			Weapon?.InvalidateProperties();
		}
	}
}