using Server.Mobiles;

using System;

namespace Server.Spells.Spellweaving
{
	public class SummonFeySpell : SpellweavingSummon<ArcaneFey>
	{
		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);

		public override int Sound => 0x217;

		public SummonFeySpell(Mobile caster, Item scroll)
			: base(caster, scroll, SpellweavingSpellName.SummonFey)
		{
		}
	}
}