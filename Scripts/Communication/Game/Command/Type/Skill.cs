﻿using Server.Commands;
using Server.Gumps;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;

namespace Server.Commands
{
	public class SkillsCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register("SetSkill", AccessLevel.GameMaster, new CommandEventHandler(SetSkill_OnCommand));
			CommandSystem.Register("GetSkill", AccessLevel.GameMaster, new CommandEventHandler(GetSkill_OnCommand));
			CommandSystem.Register("SetAllSkills", AccessLevel.GameMaster, new CommandEventHandler(SetAllSkills_OnCommand));
		}

		[Usage("SetSkill <name> <value>")]
		[Description("Sets a skill value by name of a targeted mobile.")]
		public static void SetSkill_OnCommand(CommandEventArgs arg)
		{
			if (arg.Length != 2)
			{
				arg.Mobile.SendMessage("SetSkill <skill name> <value>");
			}
			else
			{
				SkillName skill;
				if (Enum.TryParse(arg.GetString(0), true, out skill))
				{
					arg.Mobile.Target = new SkillTarget(skill, arg.GetDouble(1));
				}
				else
				{
					arg.Mobile.SendLocalizedMessage(1005631); // You have specified an invalid skill to set.
				}
			}
		}

		[Usage("SetAllSkills <name> <value>")]
		[Description("Sets all skill values of a targeted mobile.")]
		public static void SetAllSkills_OnCommand(CommandEventArgs arg)
		{
			if (arg.Length != 1)
			{
				arg.Mobile.SendMessage("SetAllSkills <value>");
			}
			else
			{
				arg.Mobile.Target = new AllSkillsTarget(arg.GetDouble(0));
			}
		}

		[Usage("GetSkill <name>")]
		[Description("Gets a skill value by name of a targeted mobile.")]
		public static void GetSkill_OnCommand(CommandEventArgs arg)
		{
			if (arg.Length != 1)
			{
				arg.Mobile.SendMessage("GetSkill <skill name>");
			}
			else
			{
				SkillName skill;
				if (Enum.TryParse(arg.GetString(0), true, out skill))
				{
					arg.Mobile.Target = new SkillTarget(skill);
				}
				else
				{
					arg.Mobile.SendMessage("You have specified an invalid skill to get.");
				}
			}
		}

		public class AllSkillsTarget : Target
		{
			private readonly double m_Value;

			public AllSkillsTarget(double value) : base(-1, false, TargetFlags.None)
			{
				m_Value = value;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Mobile)
				{
					var targ = (Mobile)targeted;
					var skills = targ.Skills;

					for (var i = 0; i < skills.Length; ++i)
					{
						skills[i].Base = m_Value;
					}

					CommandLogging.LogChangeProperty(from, targ, "EverySkill.Base", m_Value.ToString());
				}
				else
				{
					from.SendMessage("That does not have skills!");
				}
			}
		}

		public class SkillTarget : Target
		{
			private readonly bool m_Set;
			private readonly SkillName m_Skill;
			private readonly double m_Value;

			public SkillTarget(SkillName skill, double value) : base(-1, false, TargetFlags.None)
			{
				m_Set = true;
				m_Skill = skill;
				m_Value = value;
			}

			public SkillTarget(SkillName skill) : base(-1, false, TargetFlags.None)
			{
				m_Set = false;
				m_Skill = skill;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Mobile)
				{
					var targ = (Mobile)targeted;
					var skill = targ.Skills[m_Skill];

					if (skill == null)
					{
						return;
					}

					if (m_Set)
					{
						skill.Base = m_Value;
						CommandLogging.LogChangeProperty(from, targ, String.Format("{0}.Base", m_Skill), m_Value.ToString());
					}

					from.SendMessage("{0} : {1} (Base: {2})", m_Skill, skill.Value, skill.Base);
				}
				else
				{
					from.SendMessage("That does not have skills!");
				}
			}
		}
	}

	public static class Skills
	{
		public static void Initialize()
		{
			Register();
		}

		public static void Register()
		{
			CommandSystem.Register("Skills", AccessLevel.Counselor, new CommandEventHandler(Skills_OnCommand));
		}

		private class SkillsTarget : Target
		{
			public SkillsTarget() : base(-1, true, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile)
				{
					from.SendGump(new SkillsGump(from, (Mobile)o));
				}
			}
		}

		[Usage("Skills")]
		[Description("Opens a menu where you can view or edit skills of a targeted mobile.")]
		private static void Skills_OnCommand(CommandEventArgs e)
		{
			e.Mobile.Target = new SkillsTarget();
		}
	}
}

namespace Server.Gumps
{
	public class EditSkillGump : Gump
	{
		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly int EntryWidth = 160;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + (2 * (EntryHeight + OffsetSize));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		private readonly Mobile m_From;
		private readonly Mobile m_Target;
		private readonly Skill m_Skill;

		private readonly SkillsGumpGroup m_Selected;

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1)
			{
				try
				{
					if (m_From.AccessLevel >= AccessLevel.GameMaster)
					{
						var text = info.GetTextEntry(0);

						if (text != null)
						{
							m_Skill.Base = Convert.ToDouble(text.Text);
							CommandLogging.LogChangeProperty(m_From, m_Target, String.Format("{0}.Base", m_Skill), m_Skill.Base.ToString());
						}
					}
					else
					{
						m_From.SendMessage("You may not change that.");
					}

					m_From.SendGump(new SkillsGump(m_From, m_Target, m_Selected));
				}
				catch
				{
					m_From.SendMessage("Bad format. ###.# expected.");
					m_From.SendGump(new EditSkillGump(m_From, m_Target, m_Skill, m_Selected));
				}
			}
			else
			{
				m_From.SendGump(new SkillsGump(m_From, m_Target, m_Selected));
			}
		}

		public EditSkillGump(Mobile from, Mobile target, Skill skill, SkillsGumpGroup selected) : base(GumpOffsetX, GumpOffsetY)
		{
			m_From = from;
			m_Target = target;
			m_Skill = skill;
			m_Selected = selected;

			var initialText = m_Skill.Base.ToString("F1");

			AddPage(0);

			AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, skill.Name);
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddTextEntry(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, 0, initialText);
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 1, GumpButtonType.Reply, 0);
		}
	}

	public class SkillsGump : Gump
	{
		public static bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		/*
		private static bool PrevLabel = OldStyle, NextLabel = OldStyle;

		private static readonly int PrevLabelOffsetX = PrevWidth + 1;
		
		private static readonly int PrevLabelOffsetY = 0;

		private static readonly int NextLabelOffsetX = -29;
		private static readonly int NextLabelOffsetY = 0;
		 * */

		private static readonly int NameWidth = 107;
		private static readonly int ValueWidth = 128;

		private static readonly int EntryCount = 15;

		private static readonly int TypeWidth = NameWidth + OffsetSize + ValueWidth;

		private static readonly int TotalWidth = OffsetSize + NameWidth + OffsetSize + ValueWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (EntryCount + 1));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		private static readonly int IndentWidth = 12;

		private readonly Mobile m_From;
		private readonly Mobile m_Target;

		private readonly SkillsGumpGroup[] m_Groups;
		private readonly SkillsGumpGroup m_Selected;

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var buttonID = info.ButtonID - 1;

			var index = buttonID / 3;
			var type = buttonID % 3;

			switch (type)
			{
				case 0:
					{
						if (index >= 0 && index < m_Groups.Length)
						{
							var newSelection = m_Groups[index];

							if (m_Selected != newSelection)
							{
								m_From.SendGump(new SkillsGump(m_From, m_Target, newSelection));
							}
							else
							{
								m_From.SendGump(new SkillsGump(m_From, m_Target, null));
							}
						}

						break;
					}
				case 1:
					{
						if (m_Selected != null && index >= 0 && index < m_Selected.Skills.Length)
						{
							var sk = m_Target.Skills[m_Selected.Skills[index]];

							if (sk != null)
							{
								if (m_From.AccessLevel >= AccessLevel.GameMaster)
								{
									m_From.SendGump(new EditSkillGump(m_From, m_Target, sk, m_Selected));
								}
								else
								{
									m_From.SendMessage("You may not change that.");
									m_From.SendGump(new SkillsGump(m_From, m_Target, m_Selected));
								}
							}
							else
							{
								m_From.SendGump(new SkillsGump(m_From, m_Target, m_Selected));
							}
						}

						break;
					}
				case 2:
					{
						if (m_Selected != null && index >= 0 && index < m_Selected.Skills.Length)
						{
							var sk = m_Target.Skills[m_Selected.Skills[index]];

							if (sk != null)
							{
								if (m_From.AccessLevel >= AccessLevel.GameMaster)
								{
									switch (sk.Lock)
									{
										case SkillLock.Up: sk.SetLockNoRelay(SkillLock.Down); sk.Update(); break;
										case SkillLock.Down: sk.SetLockNoRelay(SkillLock.Locked); sk.Update(); break;
										case SkillLock.Locked: sk.SetLockNoRelay(SkillLock.Up); sk.Update(); break;
									}
								}
								else
								{
									m_From.SendMessage("You may not change that.");
								}

								m_From.SendGump(new SkillsGump(m_From, m_Target, m_Selected));
							}
						}

						break;
					}
			}
		}

		public int GetButtonID(int type, int index)
		{
			return 1 + (index * 3) + type;
		}

		public SkillsGump(Mobile from, Mobile target) : this(from, target, null)
		{
		}

		public SkillsGump(Mobile from, Mobile target, SkillsGumpGroup selected) : base(GumpOffsetX, GumpOffsetY)
		{
			m_From = from;
			m_Target = target;

			m_Groups = SkillsGumpGroup.Groups;
			m_Selected = selected;

			var count = m_Groups.Length;

			if (selected != null)
			{
				count += selected.Skills.Length;
			}

			var totalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (count + 1));

			AddPage(0);

			AddBackground(0, 0, BackWidth, BorderSize + totalHeight + BorderSize, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);

			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize;

			var emptyWidth = TotalWidth - PrevWidth - NextWidth - (OffsetSize * 4) - (OldStyle ? SetWidth + OffsetSize : 0);

			if (OldStyle)
			{
				AddImageTiled(x, y, TotalWidth - (OffsetSize * 3) - SetWidth, EntryHeight, HeaderGumpID);
			}
			else
			{
				AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);
			}

			x += PrevWidth + OffsetSize;

			if (!OldStyle)
			{
				AddImageTiled(x - (OldStyle ? OffsetSize : 0), y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0), EntryHeight, HeaderGumpID);
			}

			x += emptyWidth + OffsetSize;

			if (!OldStyle)
			{
				AddImageTiled(x, y, NextWidth, EntryHeight, HeaderGumpID);
			}

			for (var i = 0; i < m_Groups.Length; ++i)
			{
				x = BorderSize + OffsetSize;
				y += EntryHeight + OffsetSize;

				var group = m_Groups[i];

				AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

				if (group == selected)
				{
					AddButton(x + PrevOffsetX, y + PrevOffsetY, 0x15E2, 0x15E6, GetButtonID(0, i), GumpButtonType.Reply, 0);
				}
				else
				{
					AddButton(x + PrevOffsetX, y + PrevOffsetY, 0x15E1, 0x15E5, GetButtonID(0, i), GumpButtonType.Reply, 0);
				}

				x += PrevWidth + OffsetSize;

				x -= (OldStyle ? OffsetSize : 0);

				AddImageTiled(x, y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0), EntryHeight, EntryGumpID);
				AddLabel(x + TextOffsetX, y, TextHue, group.Name);

				x += emptyWidth + (OldStyle ? OffsetSize * 2 : 0);
				x += OffsetSize;

				if (SetGumpID != 0)
				{
					AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
				}

				if (group == selected)
				{
					var indentMaskX = BorderSize;
					var indentMaskY = y + EntryHeight + OffsetSize;

					for (var j = 0; j < group.Skills.Length; ++j)
					{
						var sk = target.Skills[group.Skills[j]];

						x = BorderSize + OffsetSize;
						y += EntryHeight + OffsetSize;

						x += OffsetSize;
						x += IndentWidth;

						AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

						AddButton(x + PrevOffsetX, y + PrevOffsetY, 0x15E1, 0x15E5, GetButtonID(1, j), GumpButtonType.Reply, 0);

						x += PrevWidth + OffsetSize;

						x -= (OldStyle ? OffsetSize : 0);

						AddImageTiled(x, y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0) - OffsetSize - IndentWidth, EntryHeight, EntryGumpID);
						AddLabel(x + TextOffsetX, y, TextHue, sk == null ? "(null)" : sk.Name);

						x += emptyWidth + (OldStyle ? OffsetSize * 2 : 0) - OffsetSize - IndentWidth;
						x += OffsetSize;

						if (SetGumpID != 0)
						{
							AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
						}

						if (sk != null)
						{
							int buttonID1, buttonID2;
							int xOffset, yOffset;

							switch (sk.Lock)
							{
								default:
								case SkillLock.Up: buttonID1 = 0x983; buttonID2 = 0x983; xOffset = 6; yOffset = 4; break;
								case SkillLock.Down: buttonID1 = 0x985; buttonID2 = 0x985; xOffset = 6; yOffset = 4; break;
								case SkillLock.Locked: buttonID1 = 0x82C; buttonID2 = 0x82C; xOffset = 5; yOffset = 2; break;
							}

							AddButton(x + xOffset, y + yOffset, buttonID1, buttonID2, GetButtonID(2, j), GumpButtonType.Reply, 0);

							y += 1;
							x -= OffsetSize;
							x -= 1;
							x -= 50;

							AddImageTiled(x, y, 50, EntryHeight - 2, OffsetGumpID);

							x += 1;
							y += 1;

							AddImageTiled(x, y, 48, EntryHeight - 4, EntryGumpID);

							AddLabelCropped(x + TextOffsetX, y - 1, 48 - TextOffsetX, EntryHeight - 3, TextHue, sk.Base.ToString("F1"));

							y -= 2;
						}
					}

					AddImageTiled(indentMaskX, indentMaskY, IndentWidth + OffsetSize, (group.Skills.Length * (EntryHeight + OffsetSize)) - (i < (m_Groups.Length - 1) ? OffsetSize : 0), BackGumpID + 4);
				}
			}
		}
	}

	public class SkillsGumpGroup
	{
		private readonly string m_Name;
		private readonly SkillName[] m_Skills;

		public string Name => m_Name;
		public SkillName[] Skills => m_Skills;

		public SkillsGumpGroup(string name, SkillName[] skills)
		{
			m_Name = name;
			m_Skills = skills;

			Array.Sort(m_Skills, new SkillNameComparer());
		}

		private class SkillNameComparer : IComparer
		{
			public SkillNameComparer()
			{
			}

			public int Compare(object x, object y)
			{
				var a = (SkillName)x;
				var b = (SkillName)y;

				var aName = SkillInfo.Table[(int)a].Name;
				var bName = SkillInfo.Table[(int)b].Name;

				return aName.CompareTo(bName);
			}
		}

		private static readonly SkillsGumpGroup[] m_Groups = new SkillsGumpGroup[]
			{
				new SkillsGumpGroup( "Crafting", new SkillName[]
				{
					SkillName.Alchemy,
					SkillName.Blacksmith,
					SkillName.Cartography,
					SkillName.Carpentry,
					SkillName.Cooking,
					SkillName.Fletching,
					SkillName.Inscribe,
					SkillName.Tailoring,
					SkillName.Tinkering,
					SkillName.Imbuing
				} ),
				new SkillsGumpGroup( "Bardic", new SkillName[]
				{
					SkillName.Discordance,
					SkillName.Musicianship,
					SkillName.Peacemaking,
					SkillName.Provocation
				} ),
				new SkillsGumpGroup( "Magical", new SkillName[]
				{
					SkillName.Chivalry,
					SkillName.EvalInt,
					SkillName.Magery,
					SkillName.MagicResist,
					SkillName.Meditation,
					SkillName.Necromancy,
					SkillName.SpiritSpeak,
					SkillName.Ninjitsu,
					SkillName.Bushido,
					SkillName.Spellweaving,
					SkillName.Mysticism
				} ),
				new SkillsGumpGroup( "Miscellaneous", new SkillName[]
				{
					SkillName.Camping,
					SkillName.Fishing,
					SkillName.Focus,
					SkillName.Healing,
					SkillName.Herding,
					SkillName.Lockpicking,
					SkillName.Lumberjacking,
					SkillName.Mining,
					SkillName.Snooping,
					SkillName.Veterinary
				} ),
				new SkillsGumpGroup( "Combat Ratings", new SkillName[]
				{
					SkillName.Archery,
					SkillName.Fencing,
					SkillName.Macing,
					SkillName.Parry,
					SkillName.Swords,
					SkillName.Tactics,
					SkillName.Wrestling,
					SkillName.Throwing
				} ),
				new SkillsGumpGroup( "Actions", new SkillName[]
				{
					SkillName.AnimalTaming,
					SkillName.Begging,
					SkillName.DetectHidden,
					SkillName.Hiding,
					SkillName.RemoveTrap,
					SkillName.Poisoning,
					SkillName.Stealing,
					SkillName.Stealth,
					SkillName.Tracking
				} ),
				new SkillsGumpGroup( "Lore & Knowledge", new SkillName[]
				{
					SkillName.Anatomy,
					SkillName.AnimalLore,
					SkillName.ArmsLore,
					SkillName.Forensics,
					SkillName.ItemID,
					SkillName.TasteID
				} )
			};

		public static SkillsGumpGroup[] Groups => m_Groups;
	}
}