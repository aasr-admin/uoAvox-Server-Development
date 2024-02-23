using Server.Items;
using Server.Mobiles;

namespace Server.Spells.Spellweaving
{
	public abstract class SpellweavingSpell : Spell
	{
		public new SpellweavingSpellName ID => (SpellweavingSpellName)base.ID;

		public override SkillName CastSkill => SkillName.Spellweaving;
		public override SkillName DamageSkill => SkillName.Spellweaving;

		public override bool ClearHandsOnCast => false;

		private int m_CastTimeFocusLevel;

		public virtual int FocusLevel => m_CastTimeFocusLevel;

		public SpellweavingSpell(Mobile caster, Item scroll, SpellweavingSpellName id)
			: base(caster, scroll, (SpellName)id)
		{
		}

		public SpellweavingSpell(Mobile caster, Item scroll, SpellInfo info)
			: base(caster, scroll, info)
		{
		}

		public override void GetCastSkills(ref double req, out double min, out double max)
		{
			min = req - 12.5; //per 5 on friday, 2/16/07
			max = req + 37.5;
		}

		public override void DoFizzle()
		{
			Caster.PlaySound(0x1D6);
			Caster.NextSpellTime = Core.TickCount;
		}

		public override void DoHurtFizzle()
		{
			Caster.PlaySound(0x1D6);
		}

		public override void OnInterrupt(SpellInterrupt type, bool message)
		{
			base.OnInterrupt(type, message);

			if (message)
			{
				Caster.PlaySound(0x1D6);
			}
		}

		public override void OnBeginCast()
		{
			base.OnBeginCast();

			m_CastTimeFocusLevel = GetFocusLevel(Caster);

			SendCastEffect();
		}

		public virtual void SendCastEffect()
		{
			var delay = GetCastDelay();

			Caster.FixedEffect(0x37C4, 10, (int)(delay.TotalSeconds * 28), 4, 3);
		}

		public override bool CheckResisted(Mobile m)
		{
			//TODO: According to the guide this is it.. but.. is it correct per OSI?
			var percent = (50.0 + 2.0 * (GetResistSkill(m) - GetDamageSkill(Caster))) / 100.0; 

			if (percent <= 0)
			{
				return false;
			}

			if (percent >= 1.0)
			{
				return true;
			}

			return percent >= Utility.RandomDouble();
		}

		public static int GetFocusLevel(Mobile from)
		{
			var focus = FindArcaneFocus(from);

			if (focus == null || focus.Deleted)
			{
				return 0;
			}

			return focus.StrengthBonus;
		}

		public static ArcaneFocus FindArcaneFocus(Mobile from)
		{
			if (from == null || from.Backpack == null)
			{
				return null;
			}

			if (from.Holding is ArcaneFocus af)
			{
				return af;
			}

			return from.Backpack.FindItemByType<ArcaneFocus>();
		}
	}
}