using Server.Mobiles;

using System;

namespace Server.Spells.Spellweaving
{
	public class SummonFiendSpell : SpellweavingSummon<ArcaneFiend>
	{
		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(2.0);

		public override int Sound => 0x216;

		public SummonFiendSpell(Mobile caster, Item scroll)
			: base(caster, scroll, SpellweavingSpellName.SummonFiend)
		{
		}
	}
}