using Server.Spells;
using Server.Commands;
using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Ethics;
using Server.Gumps;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells;
using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	public enum BookQuality
	{
		Regular,
		Exceptional,
	}

	public abstract class Spellbook : Item, ICraftable, ISlayer, ISpellbook
	{
		private static readonly int[] m_LegendPropertyCounts = new int[]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,	// 0 properties : 21/52 : 40%
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,					// 1 property   : 15/52 : 29%
			2, 2, 2, 2, 2, 2, 2, 2, 2, 2,									// 2 properties : 10/52 : 19%
			3, 3, 3, 3, 3, 3,												// 3 properties :  6/52 : 12%
		};

		private static readonly int[] m_ElderPropertyCounts = new int[]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,	// 0 properties : 15/34 : 44%
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 					// 1 property   : 10/34 : 29%
			2, 2, 2, 2, 2, 2,								// 2 properties :  6/34 : 18%
			3, 3, 3,										// 3 properties :  3/34 :  9%
		};

		private static readonly int[] m_GrandPropertyCounts = new int[]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,	// 0 properties : 10/20 : 50%
			1, 1, 1, 1, 1, 1,				// 1 property   :  6/20 : 30%
			2, 2, 2,						// 2 properties :  3/20 : 15%
			3,								// 3 properties :  1/20 :  5%
		};

		private static readonly int[] m_MasterPropertyCounts = new int[]
		{
			0, 0, 0, 0, 0, 0,	// 0 properties : 6/10 : 60%
			1, 1, 1,			// 1 property   : 3/10 : 30%
			2,					// 2 properties : 1/10 : 10%
		};

		private static readonly int[] m_AdeptPropertyCounts = new int[]
		{
			0, 0, 0,	// 0 properties : 3/4 : 75%
			1,			// 1 property   : 1/4 : 25%
		};

		public static void Initialize()
		{
			EventSink.OpenSpellbookRequest += EventSink_OpenSpellbookRequest;
			EventSink.CastSpellRequest += EventSink_CastSpellRequest;

			CommandSystem.Register("AllSpells", AccessLevel.GameMaster, AllSpells_OnCommand);
		}

		[Usage("AllSpells")]
		[Description("Completely fills a targeted spellbook with scrolls.")]
		private static void AllSpells_OnCommand(CommandEventArgs e)
		{
			e.Mobile.BeginTarget(-1, false, TargetFlags.None, AllSpells_OnTarget);
			e.Mobile.SendMessage("Target the spellbook to fill.");
		}

		public static void AllSpells_OnTarget(Mobile from, object obj)
		{
			if (obj is ISpellbook book)
			{
				book.Fill();

				from.SendMessage("The spellbook has been filled.");

				CommandLogging.WriteLine(from, "{0} {1} filling spellbook {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(book));
			}
			else
			{
				from.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(AllSpells_OnTarget));
				from.SendMessage("That is not a spellbook. Try again.");
			}
		}

		public static void EventSink_OpenSpellbookRequest(OpenSpellbookRequestEventArgs e)
		{
			var from = e.Mobile;

			if (!DesignContext.Check(from))
			{
				return; // They are customizing
			}

			var book = SpellbookHelper.Find(from, SpellName.Invalid, (SpellSchool)(e.Type - 1));

			book?.DisplayTo(from);
		}

		public static void EventSink_CastSpellRequest(CastSpellRequestEventArgs e)
		{
			var from = e.Mobile;

			if (!DesignContext.Check(from))
			{
				return; // They are customizing
			}

			var book = e.Spellbook;

			if (book == null || !book.HasSpell(e.SpellID))
			{
				book = SpellbookHelper.Find(from, e.SpellID);
			}

			if (book != null && book.HasSpell(e.SpellID))
			{
				var move = SpellRegistry.GetSpecialMove(e.SpellID);

				if (move != null)
				{
					SpecialMove.SetCurrentMove(from, move);
				}
				else
				{
					var spell = SpellRegistry.NewSpell(e.SpellID, from, null);

					if (spell != null)
					{
						spell.Cast();
					}
					else
					{
						from.SendLocalizedMessage(502345); // This spell has been temporarily disabled.
					}
				}
			}
			else
			{
				from.SendLocalizedMessage(500015); // You do not have that spell!
			}
		}

		public List<SpellbookGump> Gumps { get; } = new();

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public override int Hue
		{
			get => base.Hue;
			set
			{
				if (value <= 0)
				{
					value = Theme.BookHue;
				}

				if (base.Hue != value)
				{
					base.Hue = value;

					_ = UpdateGumps();
				}
			}
		}

		private AosAttributes m_AosAttributes;

		[CommandProperty(AccessLevel.GameMaster)]
		public AosAttributes Attributes
		{
			get => m_AosAttributes;
			set { }
		}

		private AosSkillBonuses m_AosSkillBonuses;

		[CommandProperty(AccessLevel.GameMaster)]
		public AosSkillBonuses SkillBonuses
		{
			get => m_AosSkillBonuses;
			set { }
		}

		private string m_EngravedText;

		[CommandProperty(AccessLevel.GameMaster)]
		public string EngravedText
		{
			get => m_EngravedText;
			set
			{
				m_EngravedText = value;

				InvalidateProperties();
			}
		}

		private BookQuality m_Quality;

		[CommandProperty(AccessLevel.GameMaster)]
		public BookQuality Quality
		{
			get => m_Quality;
			set
			{
				m_Quality = value;

				InvalidateProperties();
			}
		}

		private Mobile m_Crafter;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get => m_Crafter;
			set
			{
				m_Crafter = value;

				InvalidateProperties();
			}
		}

		private SlayerName m_Slayer;

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName Slayer
		{
			get => m_Slayer;
			set
			{
				m_Slayer = value;

				InvalidateProperties();
			}
		}

		private SlayerName m_Slayer2;

		[CommandProperty(AccessLevel.GameMaster)]
		public SlayerName Slayer2
		{
			get => m_Slayer2;
			set
			{
				m_Slayer2 = value;

				InvalidateProperties();
			}
		}

		public override bool DisplayWeight => false;

		public override bool DisplayLootType => Core.AOS;

		public virtual bool DisplaySpellCount => true;

		[CommandProperty(AccessLevel.GameMaster)]
		public abstract SpellSchool School { get; }

		protected ulong m_Content;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual ulong Content
		{
			get => m_Content;
			set
			{
				if (m_Content != value)
				{
					m_Content = value;

					SpellCount = 0;

					while (value > 0)
					{
						SpellCount += (int)(value & 0x1);

						value >>= 1;
					}

					InvalidateProperties();

					_ = UpdateGumps();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster, true)]
		public int SpellCount { get; protected set; }

		public int BookOffset => SpellbookHelper.GetSchoolOffset(School);
		public int BookCount => SpellbookHelper.GetSchoolCount(School);

		[CommandProperty(AccessLevel.GameMaster)]
		public SpellbookTheme Theme => SpellbookTheme.GetTheme(School);

		public override string DefaultName => $"{Theme.Name} {Theme.Summary}";

		public virtual bool UseGumps => true;
		public virtual bool FillSpells => false;
		public virtual bool AllowEquip => Core.ML;

		[Constructable]
		public Spellbook() 
			: this(0UL)
		{
		}

		[Constructable]
		public Spellbook(ulong content) 
			: this(content, 0xEFA)
		{
		}

		[Constructable]
		public Spellbook(ulong content, int itemID) 
			: base(itemID)
		{
			m_AosAttributes = new AosAttributes(this);
			m_AosSkillBonuses = new AosSkillBonuses(this);

			Weight = 3.0;
			Layer = Layer.OneHanded;
			LootType = LootType.Blessed;

			Content = content;

			Hue = 0;

			if (FillSpells)
			{
				Fill();
			}
		}

		public Spellbook(Serial serial) 
			: base(serial)
		{
		}

		public virtual void Fill()
		{
			if (Deleted)
			{
				return;
			}

			foreach (var spell in SpellRegistry.GetSpells(School))
			{
				_ = AddSpell(spell);
			}
		}

		public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
		{
			if (!Ethic.CheckTrade(from, to, newOwner, this))
			{
				return false;
			}

			return base.AllowSecureTrade(from, to, newOwner, accepted);
		}

		public override bool CanEquip(Mobile from)
		{
			if (!AllowEquip)
			{
				return false;
			}

			if (!Ethic.CheckEquip(from, this))
			{
				return false;
			}

			if (!from.CanBeginAction(typeof(BaseWeapon)))
			{
				return false;
			}

			return base.CanEquip(from);
		}

		public override bool AllowEquipedCast(Mobile from)
		{
			return true;
		}

		public override void OnAfterDuped(Item newItem)
		{
			base.OnAfterDuped(newItem);

			if (newItem is Spellbook book)
			{
				book.m_AosAttributes = new AosAttributes(newItem, m_AosAttributes);
				book.m_AosSkillBonuses = new AosSkillBonuses(newItem, m_AosSkillBonuses);
			}
		}

		public override void OnAdded(IEntity parent)
		{
			base.OnAdded(parent);

			if (Core.AOS && parent is Mobile from)
			{
				m_AosSkillBonuses.AddTo(from);

				var strBonus = m_AosAttributes.BonusStr;
				var dexBonus = m_AosAttributes.BonusDex;
				var intBonus = m_AosAttributes.BonusInt;

				if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
				{
					var modName = Serial.ToString();

					if (strBonus != 0)
					{
						from.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));
					}

					if (dexBonus != 0)
					{
						from.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));
					}

					if (intBonus != 0)
					{
						from.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
					}
				}

				from.CheckStatTimers();
			}
		}

		public override void OnRemoved(IEntity parent)
		{
			base.OnRemoved(parent);

			if (Core.AOS && parent is Mobile from)
			{
				m_AosSkillBonuses.Remove();

				var modName = Serial.ToString();

				from.RemoveStatMod(modName + "Str");
				from.RemoveStatMod(modName + "Dex");
				from.RemoveStatMod(modName + "Int");

				from.CheckStatTimers();
			}
		}

		public override void OnDelete()
		{
			base.OnDelete();

			_ = CloseGumps();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			_ = CloseGumps();
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			return dropped is SpellScroll scroll && AddScroll(from, scroll);
		}

		public virtual bool HasSpell(SpellName spell)
		{
			var spellID = SpellbookHelper.GetSpellIndex(spell);

			return spellID >= 0 && spellID < BookCount && (m_Content & (1UL << spellID)) != 0;
		}

		public virtual bool AddSpell(SpellName spell)
		{
			if (Deleted || spell == SpellName.Invalid)
			{
				return false;
			}

			if (HasSpell(spell))
			{
				return false;
			}

			var index = SpellbookHelper.GetSpellIndex(spell);

			if (index >= 0)
			{
				Content |= 1ul << index;

				return true;
			}

			return false;
		}

		public virtual bool AddScroll(Mobile from, SpellScroll scroll)
		{
			if (scroll?.Deleted != false || !scroll.Movable || scroll.SpellID == SpellName.Invalid || !scroll.CheckItemUse(from))
			{
				return false;
			}

			var school = SpellRegistry.GetSchool(scroll.SpellID);

			if (school != School)
			{
				from?.SendMessage("That spell does not belong in that spellbook.");
				return false;
			}

			if (HasSpell(scroll.SpellID))
			{
				from?.SendLocalizedMessage(500179); // That spell is already present in that spellbook.
				return false;
			}

			if (!AddSpell(scroll.SpellID))
			{
				return false;
			}

			scroll.Delete();

			from?.SendSound(0x249, GetWorldLocation());

			return true;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Quality == BookQuality.Exceptional)
			{
				list.Add(1063341); // exceptional
			}

			if (m_EngravedText != null)
			{
				list.Add(1072305, m_EngravedText); // Engraved: ~1_INSCRIPTION~
			}

			if (m_Crafter != null)
			{
				list.Add(1050043, m_Crafter.Name); // crafted by ~1_NAME~
			}

			m_AosSkillBonuses.GetProperties(list);

			if (m_Slayer != SlayerName.None)
			{
				var entry = SlayerGroup.GetEntryByName(m_Slayer);

				if (entry != null)
				{
					list.Add(entry.Title);
				}
			}

			if (m_Slayer2 != SlayerName.None)
			{
				var entry = SlayerGroup.GetEntryByName(m_Slayer2);

				if (entry != null)
				{
					list.Add(entry.Title);
				}
			}

			int prop;

			if ((prop = m_AosAttributes.WeaponDamage) != 0)
			{
				list.Add(1060401, prop.ToString()); // damage increase ~1_val~%
			}

			if ((prop = m_AosAttributes.DefendChance) != 0)
			{
				list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%
			}

			if ((prop = m_AosAttributes.BonusDex) != 0)
			{
				list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~
			}

			if ((prop = m_AosAttributes.EnhancePotions) != 0)
			{
				list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%
			}

			if ((prop = m_AosAttributes.CastRecovery) != 0)
			{
				list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~
			}

			if ((prop = m_AosAttributes.CastSpeed) != 0)
			{
				list.Add(1060413, prop.ToString()); // faster casting ~1_val~
			}

			if ((prop = m_AosAttributes.AttackChance) != 0)
			{
				list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%
			}

			if ((prop = m_AosAttributes.BonusHits) != 0)
			{
				list.Add(1060431, prop.ToString()); // hit point increase ~1_val~
			}

			if ((prop = m_AosAttributes.BonusInt) != 0)
			{
				list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~
			}

			if ((prop = m_AosAttributes.LowerManaCost) != 0)
			{
				list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%
			}

			if ((prop = m_AosAttributes.LowerRegCost) != 0)
			{
				list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%
			}

			if ((prop = m_AosAttributes.Luck) != 0)
			{
				list.Add(1060436, prop.ToString()); // luck ~1_val~
			}

			if ((prop = m_AosAttributes.BonusMana) != 0)
			{
				list.Add(1060439, prop.ToString()); // mana increase ~1_val~
			}

			if ((prop = m_AosAttributes.RegenMana) != 0)
			{
				list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~
			}

			if (m_AosAttributes.NightSight != 0)
			{
				list.Add(1060441); // night sight
			}

			if ((prop = m_AosAttributes.ReflectPhysical) != 0)
			{
				list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%
			}

			if ((prop = m_AosAttributes.RegenStam) != 0)
			{
				list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~
			}

			if ((prop = m_AosAttributes.RegenHits) != 0)
			{
				list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~
			}

			if (m_AosAttributes.SpellChanneling != 0)
			{
				list.Add(1060482); // spell channeling
			}

			if ((prop = m_AosAttributes.SpellDamage) != 0)
			{
				list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%
			}

			if ((prop = m_AosAttributes.BonusStam) != 0)
			{
				list.Add(1060484, prop.ToString()); // stamina increase ~1_val~
			}

			if ((prop = m_AosAttributes.BonusStr) != 0)
			{
				list.Add(1060485, prop.ToString()); // strength bonus ~1_val~
			}

			if ((prop = m_AosAttributes.WeaponSpeed) != 0)
			{
				list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%
			}

			if (Core.ML && (prop = m_AosAttributes.IncreasedKarmaLoss) != 0)
			{
				list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%
			}

			if (DisplaySpellCount)
			{
				list.Add(1042886, SpellCount.ToString()); // ~1_NUMBERS_OF_SPELLS~ Spells
			}
		}

		public virtual int OnCraft(int quality, bool makersMark, Mobile from, ICraftSystem craftSystem, Type typeRes, ICraftTool tool, ICraftItem craftItem, int resHue)
		{
			var magery = from.Skills.Magery.BaseFixedPoint;

			if (magery >= 800)
			{
				int[] propertyCounts;
				int minIntensity;
				int maxIntensity;

				if (magery >= 1000)
				{
					if (magery >= 1200)
					{
						propertyCounts = m_LegendPropertyCounts;
					}
					else if (magery >= 1100)
					{
						propertyCounts = m_ElderPropertyCounts;
					}
					else
					{
						propertyCounts = m_GrandPropertyCounts;
					}

					minIntensity = 55;
					maxIntensity = 75;
				}
				else if (magery >= 900)
				{
					propertyCounts = m_MasterPropertyCounts;
					minIntensity = 25;
					maxIntensity = 45;
				}
				else
				{
					propertyCounts = m_AdeptPropertyCounts;
					minIntensity = 0;
					maxIntensity = 15;
				}

				var propertyCount = propertyCounts[Utility.Random(propertyCounts.Length)];

				BaseRunicTool.ApplyAttributesTo(this, true, 0, propertyCount, minIntensity, maxIntensity);
			}

			if (makersMark)
			{
				Crafter = from;
			}

			m_Quality = (BookQuality)(quality - 1);

			return quality;
		}

		protected override void OnParentChanged(IEntity oldParent)
		{
			base.OnParentChanged(oldParent);

			_ = UpdateGumps();
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (m_Crafter != null)
			{
				LabelTo(from, 1050043, m_Crafter.Name); // crafted by ~1_NAME~
			}

			LabelTo(from, 1042886, SpellCount.ToString());
		}

		public override void OnDoubleClick(Mobile from)
		{
			var pack = from.Backpack;

			if (Parent == from || (pack != null && Parent == pack))
			{
				DisplayTo(from);
			}
			else
			{
				from.SendLocalizedMessage(500207); // The spellbook must be in your backpack (and not in a container within) to open.
			}
		}

		public virtual bool CanDisplayTo(Mobile user, bool message)
		{
			if (Deleted || user == null || user.Deleted || user.NetState?.Running != true)
			{
				return false;
			}

			if (Parent != user && Parent != user.Backpack)
			{
				if (message)
				{
					user.SendMessage("The spellbook must be in your backpack [and not in a container within] to open.");
				}

				return false;
			}

			if (!DesignContext.Check(user))
			{
				if (message)
				{
					user.SendMessage("You cannot do that while customizing.");
				}

				return false;
			}

			return true;
		}

		public void DisplayTo(Mobile user)
		{
			if (user == null)
			{
				return;
			}

			var ns = user.NetState;

			if (Deleted || user.Deleted || !user.Player || ns == null)
			{
				_ = CloseGump(user);
				return;
			}

			if (!CanDisplayTo(user, false))
			{
				_ = CloseGump(user);
				return;
			}

			OnDisplayTo(user, false);
		}

		protected virtual void OnDisplayTo(Mobile user, bool refreshOnly)
		{
			if (UseGumps && user is PlayerMobile pm)
			{
				if (!UpdateGump(user) && !refreshOnly)
				{
					_ = BaseGump.SendGump(new SpellbookGump(pm, this));
				}
			}
			else
			{
				_ = CloseGump(user);

				var ns = user.NetState;

				if (ns == null)
				{
					return;
				}

				if (Parent == null)
				{
					ns.Send(WorldPacket);
				}
				else if (Parent is Item)
				{
					// What will happen if the client doesn't know about our parent?
					if (ns.ContainerGridLines)
					{
						ns.Send(new ContainerContentUpdate6017(this));
					}
					else
					{
						ns.Send(new ContainerContentUpdate(this));
					}
				}
				else if (Parent is Mobile)
				{
					// What will happen if the client doesn't know about our parent?
					ns.Send(new EquipUpdate(this));
				}

				if (ns.HighSeas)
				{
					ns.Send(new DisplaySpellbookHS(this));
				}
				else
				{
					ns.Send(new DisplaySpellbook(this));
				}

				if (ObjectPropertyList.Enabled)
				{
					if (ns.NewSpellbook)
					{
						ns.Send(new NewSpellbookContent(this, ItemID, BookOffset + 1, m_Content));
					}
					else if (ns.ContainerGridLines)
					{
						ns.Send(new SpellbookContent6017(SpellCount, BookOffset + 1, m_Content, this));
					}
					else
					{
						ns.Send(new SpellbookContent(SpellCount, BookOffset + 1, m_Content, this));
					}
				}
				else if (ns.ContainerGridLines)
				{
					ns.Send(new SpellbookContent6017(SpellCount, BookOffset + 1, m_Content, this));
				}
				else
				{
					ns.Send(new SpellbookContent(SpellCount, BookOffset + 1, m_Content, this));
				}
			}
		}

		public bool UpdateGump(Mobile user)
		{
			if (user == null)
			{
				return false;
			}

			if (!CanDisplayTo(user, false))
			{
				return CloseGump(user);
			}

			var index = Gumps.Count;
			var affected = 0;

			while (--index >= 0)
			{
				if (index < Gumps.Count && Gumps[index].User == user)
				{
					if (affected == 0)
					{
						Gumps[index].Refresh();
					}
					else
					{
						Gumps[index].Close();
					}

					++affected;
				}
			}

			return affected > 0;
		}

		public int UpdateGumps()
		{
			if (Deleted || Parent == null)
			{
				return CloseGumps();
			}

			var index = Gumps.Count;
			var affected = 0;

			while (--index >= 0)
			{
				if (index < Gumps.Count)
				{
					if (CanDisplayTo(Gumps[index].User, false))
					{
						Gumps[index].Refresh();
					}
					else
					{
						Gumps[index].Close();
					}

					++affected;
				}
			}

			return affected;
		}

		public bool CloseGump(Mobile user)
		{
			if (user == null)
			{
				return false;
			}

			var index = Gumps.Count;
			var affected = 0;

			while (--index >= 0)
			{
				if (index < Gumps.Count && Gumps[index].User == user)
				{
					Gumps[index].Close();

					++affected;
				}
			}

			return affected > 0;
		}

		public int CloseGumps()
		{
			var index = Gumps.Count;
			var affected = 0;

			while (--index >= 0)
			{
				if (index < Gumps.Count)
				{
					Gumps[index].Close();

					++affected;
				}
			}

			return affected;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(5); // version

			writer.Write((byte)m_Quality);

			writer.Write(m_EngravedText);

			writer.Write(m_Crafter);

			writer.Write((int)m_Slayer);
			writer.Write((int)m_Slayer2);

			m_AosAttributes.Serialize(writer);
			m_AosSkillBonuses.Serialize(writer);

			writer.Write(m_Content);
			writer.Write(SpellCount);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 5:
					{
						m_Quality = (BookQuality)reader.ReadByte();

						goto case 4;
					}
				case 4:
					{
						m_EngravedText = reader.ReadString();

						goto case 3;
					}
				case 3:
					{
						m_Crafter = reader.ReadMobile();

						goto case 2;
					}
				case 2:
					{
						m_Slayer = (SlayerName)reader.ReadInt();
						m_Slayer2 = (SlayerName)reader.ReadInt();

						goto case 1;
					}
				case 1:
					{
						m_AosAttributes = new AosAttributes(this, reader);
						m_AosSkillBonuses = new AosSkillBonuses(this, reader);

						goto case 0;
					}
				case 0:
					{
						m_Content = reader.ReadULong();
						SpellCount = reader.ReadInt();

						break;
					}
			}

			m_AosAttributes ??= new AosAttributes(this);
			m_AosSkillBonuses ??= new AosSkillBonuses(this);
			
			if (Parent is Mobile m)
			{
				if (Core.AOS)
				{
					m_AosSkillBonuses.AddTo(m);
				}

				var strBonus = m_AosAttributes.BonusStr;
				var dexBonus = m_AosAttributes.BonusDex;
				var intBonus = m_AosAttributes.BonusInt;

				if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
				{
					var modName = Serial.ToString();

					if (strBonus != 0)
					{
						m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));
					}

					if (dexBonus != 0)
					{
						m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));
					}

					if (intBonus != 0)
					{
						m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
					}
				}

				m.CheckStatTimers();
			}

			if (FillSpells && SpellCount != BookCount)
			{
				Timer.DelayCall(Fill);
			}
		}
	}

	public class AddToSpellbookEntry : ContextMenuEntry
	{
		public AddToSpellbookEntry() 
			: base(6144, 3)
		{
		}

		public override void OnClick()
		{
			if (Owner.From.CheckAlive() && Owner.Target is SpellScroll ss)
			{
				Owner.From.Target = new InternalTarget(ss);
			}
		}

		private class InternalTarget : Target
		{
			private readonly SpellScroll m_Scroll;

			public InternalTarget(SpellScroll scroll) 
				: base(3, false, TargetFlags.None)
			{
				m_Scroll = scroll;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (!from.CheckAlive())
				{
					return;
				}

				if (targeted is Spellbook book)
				{
					book.AddScroll(from, m_Scroll);
				}
			}
		}
	}
}