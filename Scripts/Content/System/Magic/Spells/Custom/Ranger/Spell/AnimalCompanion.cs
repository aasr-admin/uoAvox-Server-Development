using Server.Gumps;
using Server.Mobiles;
using Server.Network;

using System;
using System.Collections.Generic;

namespace Server.Spells.Ranger
{
	public class AnimalCompanionSpell : RangerSpell
	{
		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(3.0);

		public static Dictionary<Mobile, BaseCreature> Table { get; } = new();

		public AnimalCompanionSpell(Mobile caster, Item scroll) 
			: base(caster, scroll, RangerSpellName.AnimalCompanion)
		{
		}

		public override bool CheckCast()
		{
			if (Table.TryGetValue(Caster, out var check) && check?.Deleted == false)
			{
				Caster.SendLocalizedMessage(1061605); // You already have a familiar.
				return false;
			}

			return base.CheckCast();
		}

		public override void OnCast()
		{
			if (CheckSequence())
			{
				_ = Caster.CloseGump(typeof(RangerFamiliarGump));
				_ = Caster.SendGump(new RangerFamiliarGump(Caster, RangerFamiliarEntry.Entries));
			}

			FinishSequence();
		}
	}

	public class RangerFamiliarEntry
	{
		public static readonly RangerFamiliarEntry[] Entries =
		{
			new(typeof(PackRatFamiliar), "Pack Rat", 30.0, 30.0),
			new(typeof(IceHoundFamiliar), "Ice Hound", 50.0, 50.0),
			new(typeof(ThunderHoundFamiliar), "Thunder Hound", 60.0, 60.0),
			new(typeof(HellHoundFamiliar), "Hell Hound", 80.0, 80.0),
			new(typeof(VampireWolfFamiliar), "Vampire Wolf", 100.0, 100.0),
			new(typeof(TigerFamiliar), "Saber Tooth Tiger", 115.0, 115.0),
		};

		public Type Type { get; }
		public TextDefinition Name { get; }
		public double ReqAnimalLore { get; }
		public double ReqAnimalTaming { get; }

		public RangerFamiliarEntry(Type type, TextDefinition name, double reqAnimalLore, double reqAnimalTaming)
		{
			Type = type;
			Name = name;
			ReqAnimalLore = reqAnimalLore;
			ReqAnimalTaming = reqAnimalTaming;
		}
	}

	public class RangerFamiliarGump : Gump
	{
		private readonly Mobile m_From;
		private readonly RangerFamiliarEntry[] m_Entries;

		private const short m_EnabledColor16 = 0x0F20;
		private const short m_DisabledColor16 = 0x262A;

		private const int m_EnabledColor32 = 0x18CD00;
		private const int m_DisabledColor32 = 0x4A8B52;

		public RangerFamiliarGump(Mobile from, RangerFamiliarEntry[] entries) 
			: base(200, 100)
		{
			m_From = from;
			m_Entries = entries;

			AddPage(0);

			AddBackground(10, 10, 250, 178, 9270);
			AddAlphaRegion(20, 20, 230, 158);

			AddImage(220, 20, 10464);
			AddImage(220, 72, 10464);
			AddImage(220, 124, 10464);

			AddItem(188, 16, 6883);
			AddItem(198, 168, 6881);
			AddItem(8, 15, 6882);
			AddItem(2, 168, 6880);

			AddHtmlLocalized(30, 26, 200, 20, 1060147, m_EnabledColor16, false, false); // Chose thy familiar...

			var lore = from.Skills[SkillName.AnimalLore].Base;
			var taming = from.Skills[SkillName.AnimalTaming].Base;

			for (var i = 0; i < entries.Length; ++i)
			{
				var entry = entries[i];
				var enabled = lore >= entry.ReqAnimalLore && taming >= entry.ReqAnimalTaming;
				var color16 = enabled ? m_EnabledColor16 : m_DisabledColor16;
				var color32 = enabled ? m_EnabledColor32 : m_DisabledColor32;

				AddButton(27, 53 + (i * 21), 9702, 9703, i + 1, GumpButtonType.Reply, 0);

				TextDefinition.AddHtmlText(this, 50, 51 + (i * 21), 150, 20, entry.Name, false, false, color16, color32);
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var index = info.ButtonID - 1;

			if (index >= 0 && index < m_Entries.Length)
			{
				var entry = m_Entries[index];

				var lore = m_From.Skills[SkillName.AnimalLore].Base;
				var taming = m_From.Skills[SkillName.AnimalTaming].Base;

				if (AnimalCompanionSpell.Table.TryGetValue(m_From, out var check) && check?.Deleted == false)
				{
					m_From.SendLocalizedMessage(1061605); // You already have a familiar.
				}
				else if (lore < entry.ReqAnimalLore || taming < entry.ReqAnimalTaming)
				{
					// That familiar requires ~1_NECROMANCY~ Necromancy and ~2_SPIRIT~ Spirit Speak.
					m_From.SendMessage($"That familiar requires {entry.ReqAnimalLore:F1} Animal Lore and {entry.ReqAnimalTaming:F1} Animal Taming.");

					_ = m_From.CloseGump(typeof(RangerFamiliarGump));
					_ = m_From.SendGump(new RangerFamiliarGump(m_From, m_Entries));
				}
				else if (entry.Type == null)
				{
					m_From.SendMessage("That familiar has not yet been defined.");

					_ = m_From.CloseGump(typeof(RangerFamiliarGump));
					_ = m_From.SendGump(new RangerFamiliarGump(m_From, m_Entries));
				}
				else
				{
					try
					{
						var bc = (BaseCreature)Activator.CreateInstance(entry.Type);

						bc.Skills.MagicResist = m_From.Skills.MagicResist;

						if (BaseCreature.Summon(bc, m_From, m_From.Location, -1, TimeSpan.FromDays(1.0)))
						{
							m_From.FixedParticles(0x3728, 1, 10, 9910, EffectLayer.Head);

							bc.PlaySound(bc.GetIdleSound());

							AnimalCompanionSpell.Table[m_From] = bc;
						}
					}
					catch
					{
					}
				}
			}
			else
			{
				m_From.SendLocalizedMessage(1061825); // You decide not to summon a familiar.
			}
		}
	}
}
