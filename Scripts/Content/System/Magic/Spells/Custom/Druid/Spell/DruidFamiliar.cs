using Server.Gumps;
using Server.Mobiles;
using Server.Network;

using System;
using System.Collections.Generic;

namespace Server.Spells.Druid
{
	public class DruidFamiliarSpell : DruidSpell
	{
		public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(3.0);

		public static readonly DruidFamiliarEntry[] Entries =
		{
			new(typeof(SkitteringHopperFamiliar), "Skittering Hopper", 30.0,  30.0),
			new(typeof(PixieFamiliar), "Pixie", 50.0, 50.0),
			new(typeof(EagleFamiliar), "Spirit Eagle", 60.0, 60.0),
			new(typeof(QuagmireFamiliar), "Quagmire",  80.0, 80.0),
			new(typeof(TreefellowFamiliar), "Treefellow", 100.0, 100.0),
			new(typeof(DryadFamiliar), "Dryad", 115.0, 115.0),
		};

		public static Dictionary<Mobile, BaseCreature> Table { get; } = new();

		public DruidFamiliarSpell(Mobile caster, Item scroll)
			: base(caster, scroll, DruidSpellName.DruidFamiliar)
		{
		}

		public override bool CheckCast()
		{
			if (Table.TryGetValue(Caster, out var summon) && summon?.Deleted == false)
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
				_ = Caster.CloseGump(typeof(DruidFamiliarGump));
				_ = Caster.SendGump(new DruidFamiliarGump(Caster, Entries));
			}

			FinishSequence();
		}
	}

	public class DruidFamiliarEntry
	{
		public Type Type { get; }
		public object Name { get; }
		public double ReqAnimalLore { get; }
		public double ReqAnimalTaming { get; }

		public DruidFamiliarEntry(Type type, object name, double reqAnimalLore, double reqAnimalTaming)
		{
			Type = type;
			Name = name;
			ReqAnimalLore = reqAnimalLore;
			ReqAnimalTaming = reqAnimalTaming;
		}
	}

	public class DruidFamiliarGump : Gump
	{
		private readonly Mobile m_From;
		private readonly DruidFamiliarEntry[] m_Entries;

		private const short EnabledColor16 = 0x0F20;
		private const short DisabledColor16 = 0x262A;

		private const int EnabledColor32 = 0x18CD00;
		private const int DisabledColor32 = 0x4A8B52;

		public DruidFamiliarGump(Mobile from, DruidFamiliarEntry[] entries)
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

			AddHtmlLocalized(30, 26, 200, 20, 1060147, EnabledColor16, false, false); // Chose thy familiar...

			var lore = from.Skills[SkillName.AnimalLore].Base;
			var taming = from.Skills[SkillName.AnimalTaming].Base;

			for (var i = 0; i < entries.Length; ++i)
			{
				var name = entries[i].Name;

				var enabled = lore >= entries[i].ReqAnimalLore && taming >= entries[i].ReqAnimalTaming;

				AddButton(27, 53 + (i * 21), 9702, 9703, i + 1, GumpButtonType.Reply, 0);

				if (name is int ival)
				{
					AddHtmlLocalized(50, 51 + (i * 21), 150, 20, ival, enabled ? EnabledColor16 : DisabledColor16, false, false);
				}
				else if (name is string sval)
				{
					AddHtml(50, 51 + (i * 21), 150, 20, $"<BASEFONT COLOR=#{(enabled ? EnabledColor32 : DisabledColor32):X6}>{sval}", false, false);
				}
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var index = info.ButtonID - 1;

			if (index < 0 || index >= m_Entries.Length)
			{
				m_From.SendLocalizedMessage(1061825); // You decide not to summon a familiar.
				return;
			}

			var entry = m_Entries[index];

			var lore = m_From.Skills[SkillName.AnimalLore].Base;
			var taming = m_From.Skills[SkillName.AnimalTaming].Base;

			if (DruidFamiliarSpell.Table.TryGetValue(m_From, out var summon) && summon?.Deleted == false)
			{
				m_From.SendLocalizedMessage(1061605); // You already have a familiar.
				return;
			}

			if (lore < entry.ReqAnimalLore || taming < entry.ReqAnimalTaming)
			{
				m_From.SendMessage($"That familiar requires {entry.ReqAnimalLore:F1} Animal Lore and {entry.ReqAnimalTaming:F1} Animal Taming.");

				_ = m_From.CloseGump(typeof(DruidFamiliarGump));
				_ = m_From.SendGump(new DruidFamiliarGump(m_From, DruidFamiliarSpell.Entries));

				return;
			}

			if (entry.Type == null)
			{
				m_From.SendMessage("That familiar is currently unavailable.");

				_ = m_From.CloseGump(typeof(DruidFamiliarGump));
				_ = m_From.SendGump(new DruidFamiliarGump(m_From, DruidFamiliarSpell.Entries));

				return;
			}

			try
			{
				summon = (BaseCreature)Activator.CreateInstance(entry.Type);
			}
			catch
			{
				m_From.SendMessage("That familiar is currently unavailable.");

				_ = m_From.CloseGump(typeof(DruidFamiliarGump));
				_ = m_From.SendGump(new DruidFamiliarGump(m_From, DruidFamiliarSpell.Entries));

				return;
			}

			if (summon != null)
			{
				summon.Skills.MagicResist = m_From.Skills.MagicResist;

				if (BaseCreature.Summon(summon, m_From, m_From.Location, -1, TimeSpan.FromDays(1.0)))
				{
					DruidFamiliarSpell.Table[m_From] = summon;

					m_From.FixedParticles(0x3728, 1, 10, 9910, EffectLayer.Head);

					summon.PlaySound(summon.GetIdleSound());
				}
			}
		}
	}
}
