﻿using Server.Items;
using Server.Mobiles;

using System;
using System.Collections.Generic;

namespace Server.Spells.Necromancy
{
	public class WitherSpell : NecromancySpell
	{
		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.5);

		public WitherSpell(Mobile caster, Item scroll)
			: base(caster, scroll, NecromancySpellName.Wither)
		{
		}

		public override bool DelayedDamage => false;

		public override void OnCast()
		{
			if (CheckSequence())
			{
				/* Creates a withering frost around the Caster,
				 * which deals Cold Damage to all valid targets in a radius of 5 tiles.
				 */

				var map = Caster.Map;

				if (map != null)
				{
					var targets = new List<Mobile>();

					var cbc = Caster as BaseCreature;
					var isMonster = (cbc != null && !cbc.Controlled && !cbc.Summoned);

					foreach (var m in Caster.GetMobilesInRange(Core.ML ? 4 : 5))
					{
						if (Caster != m && Caster.InLOS(m) && (isMonster || SpellHelper.ValidIndirectTarget(Caster, m)) && Caster.CanBeHarmful(m, false))
						{
							if (isMonster)
							{
								if (m is BaseCreature)
								{
									var bc = (BaseCreature)m;

									if (!bc.Controlled && !bc.Summoned && bc.Team == cbc.Team)
									{
										continue;
									}
								}
								else if (!m.Player)
								{
									continue;
								}
							}

							targets.Add(m);
						}
					}

					Effects.PlaySound(Caster.Location, map, 0x1FB);
					Effects.PlaySound(Caster.Location, map, 0x10B);
					Effects.SendLocationParticles(EffectItem.Create(Caster.Location, map, EffectItem.DefaultDuration), 0x37CC, 1, 40, 97, 3, 9917, 0);

					for (var i = 0; i < targets.Count; ++i)
					{
						var m = targets[i];

						Caster.DoHarmful(m);
						m.FixedParticles(0x374A, 1, 15, 9502, 97, 3, (EffectLayer)255);

						double damage = Utility.RandomMinMax(30, 35);

						damage *= (300 + (m.Karma / 100) + (GetDamageSkill(Caster) * 10));
						damage /= 1000;

						var sdiBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);

						// PvP spell damage increase cap of 15% from an item’s magic property in Publish 33(SE)
						if (Core.SE && m.Player && Caster.Player && sdiBonus > 15)
						{
							sdiBonus = 15;
						}

						damage *= (100 + sdiBonus);
						damage /= 100;

						// TODO: cap?
						//if ( damage > 40 )
						//	damage = 40;

						SpellHelper.Damage(this, m, damage, 0, 0, 100, 0, 0);
					}
				}
			}

			FinishSequence();
		}
	}
}