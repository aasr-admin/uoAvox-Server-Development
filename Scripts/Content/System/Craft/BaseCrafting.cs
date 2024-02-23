
using System;
using System.Collections.Generic;
using System.Reflection;

using Server.Commands;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Engines.Craft
{
	public enum CraftECA
	{
		ChanceMinusSixty,
		FiftyPercentChanceMinusTenPercent,
		ChanceMinusSixtyToFourtyFive
	}

	public abstract class CraftSystem : ICraftSystem
	{
		private readonly List<int> m_Recipes = new();
		private readonly List<int> m_RareRecipes = new();

		public int MinCraftEffect { get; }
		public int MaxCraftEffect { get; }

		public double Delay { get; }

		public CraftItemCol CraftItems { get; } = new();
		public CraftGroupCol CraftGroups { get; } = new();
		public CraftSubResCol CraftSubRes { get; } = new();
		public CraftSubResCol CraftSubRes2 { get; } = new();

		public abstract SkillName MainSkill { get; }

		public TextDefinition Title => new(GumpTitleNumber, GumpTitleString);

		public virtual int GumpTitleNumber => 0;
		public virtual string GumpTitleString => String.Empty;

		public virtual CraftECA ECA => CraftECA.ChanceMinusSixty;

		private readonly Dictionary<Mobile, CraftContext> m_ContextTable = new();

		public bool Resmelt { get; set; }
		public bool Repair { get; set; }
		public bool MarkOption { get; set; }
		public bool CanEnhance { get; set; }

		public CraftSystem(int minCraftEffect, int maxCraftEffect, double delay)
		{
			MinCraftEffect = minCraftEffect;
			MaxCraftEffect = maxCraftEffect;

			Delay = delay;

			InitCraftList();
		}

		public abstract double GetChanceAtMin(CraftItem item);

		public virtual bool RetainsColorFrom(CraftItem item, Type type)
		{
			return false;
		}

		public CraftContext GetContext(Mobile m)
		{
			if (m == null)
			{
				return null;
			}

			if (m.Deleted)
			{
				_ = m_ContextTable.Remove(m);

				return null;
			}

			_ = m_ContextTable.TryGetValue(m, out var c);

			if (c == null)
			{
				m_ContextTable[m] = c = new CraftContext();
			}

			return c;
		}

		public void OnMade(Mobile m, CraftItem item)
		{
			var c = GetContext(m);

			c?.OnMade(item);
		}

		public virtual bool ConsumeOnFailure(Mobile from, Type resourceType, CraftItem craftItem)
		{
			return true;
		}

		public void CreateItem(Mobile from, Type type, Type typeRes, BaseTool tool, CraftItem realCraftItem)
		{
			// Verify if the type is in the list of the craftable item
			if (CraftItems.SearchFor(type) != null)
			{
				// The item is in the list, try to create it
				// Test code: items like sextant parts can be crafted either directly from ingots, or from different parts
				realCraftItem.Craft(from, this, typeRes, tool);
			}
		}

		public int RandomRecipe()
		{
			if (m_Recipes.Count != 0)
			{
				return Utility.RandomList(m_Recipes);
			}

			return -1;
		}

		public int RandomRareRecipe()
		{
			if (m_RareRecipes.Count != 0)
			{
				return Utility.RandomList(m_RareRecipes);
			}

			return -1;
		}

		public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount)
		{
			return AddCraft(typeItem, group, name, MainSkill, minSkill, maxSkill, typeRes, nameRes, amount, String.Empty);
		}

		public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount, TextDefinition message)
		{
			return AddCraft(typeItem, group, name, MainSkill, minSkill, maxSkill, typeRes, nameRes, amount, message);
		}

		public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, SkillName skillToMake, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount)
		{
			return AddCraft(typeItem, group, name, skillToMake, minSkill, maxSkill, typeRes, nameRes, amount, String.Empty);
		}

		public int AddCraft(Type typeItem, TextDefinition group, TextDefinition name, SkillName skillToMake, double minSkill, double maxSkill, Type typeRes, TextDefinition nameRes, int amount, TextDefinition message)
		{
			var craftItem = new CraftItem(typeItem, group, name);

			craftItem.AddRes(typeRes, nameRes, amount, message);
			craftItem.AddSkill(skillToMake, minSkill, maxSkill);

			DoGroup(group, craftItem);

			return CraftItems.Add(craftItem);
		}

		private void DoGroup(TextDefinition groupName, CraftItem craftItem)
		{
			var index = CraftGroups.SearchFor(groupName);

			if (index == -1)
			{
				var craftGroup = new CraftGroup(groupName);

				craftGroup.AddCraftItem(craftItem);

				_ = CraftGroups.Add(craftGroup);
			}
			else
			{
				var craftGroup = CraftGroups.GetAt(index);
				
				craftGroup.AddCraftItem(craftItem);
			}
		}

		public void SetItemHue(int index, int hue)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.ItemHue = hue;
		}

		public void SetManaReq(int index, int mana)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.Mana = mana;
		}

		public void SetStamReq(int index, int stam)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.Stam = stam;
		}

		public void SetHitsReq(int index, int hits)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.Hits = hits;
		}

		public void SetUseAllRes(int index, bool useAll)
		{
			var craftItem = CraftItems.GetAt(index);
			craftItem.UseAllRes = useAll;
		}

		public void SetNeedHeat(int index, bool needHeat)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.NeedHeat = needHeat;
		}

		public void SetNeedOven(int index, bool needOven)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.NeedOven = needOven;
		}

		public void SetBeverageType(int index, BeverageType requiredBeverage)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.RequiredBeverage = requiredBeverage;
		}

		public void SetNeedMill(int index, bool needMill)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.NeedMill = needMill;
		}

		public void SetNeededExpansion(int index, Expansion expansion)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.RequiredExpansion = expansion;
		}

		public void AddRes(int index, Type type, TextDefinition name, int amount)
		{
			AddRes(index, type, name, amount, "");
		}

		public void AddRes(int index, Type type, TextDefinition name, int amount, TextDefinition message)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.AddRes(type, name, amount, message);
		}

		public void AddSkill(int index, SkillName skillToMake, double minSkill, double maxSkill)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.AddSkill(skillToMake, minSkill, maxSkill);
		}

		public void SetUseSubRes2(int index, bool val)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.UseSubRes2 = val;
		}

		private void AddRecipeBase(int index, int id)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.AddRecipe(id, this);
		}

		public void AddRecipe(int index, int id)
		{
			AddRecipeBase(index, id);

			m_Recipes.Add(id);
		}

		public void AddRareRecipe(int index, int id)
		{
			AddRecipeBase(index, id);

			m_RareRecipes.Add(id);
		}

		public void AddQuestRecipe(int index, int id)
		{
			AddRecipeBase(index, id);
		}

		public void ForceNonExceptional(int index)
		{
			var craftItem = CraftItems.GetAt(index);

			craftItem.ForceNonExceptional = true;
		}

		public void SetSubRes(Type type, string name)
		{
			CraftSubRes.ResType = type;
			CraftSubRes.NameString = name;
			CraftSubRes.Init = true;
		}

		public void SetSubRes(Type type, int name)
		{
			CraftSubRes.ResType = type;
			CraftSubRes.NameNumber = name;
			CraftSubRes.Init = true;
		}

		public void AddSubRes(Type type, int name, double reqSkill, object message)
		{
			CraftSubRes.Add(new CraftSubRes(type, name, reqSkill, message));
		}

		public void AddSubRes(Type type, int name, double reqSkill, int genericName, object message)
		{
			CraftSubRes.Add(new CraftSubRes(type, name, reqSkill, genericName, message));
		}

		public void AddSubRes(Type type, string name, double reqSkill, object message)
		{
			CraftSubRes.Add(new CraftSubRes(type, name, reqSkill, message));
		}

		public void SetSubRes2(Type type, string name)
		{
			CraftSubRes2.ResType = type;
			CraftSubRes2.NameString = name;
			CraftSubRes2.Init = true;
		}

		public void SetSubRes2(Type type, int name)
		{
			CraftSubRes2.ResType = type;
			CraftSubRes2.NameNumber = name;
			CraftSubRes2.Init = true;
		}

		public void AddSubRes2(Type type, int name, double reqSkill, object message)
		{
			CraftSubRes2.Add(new CraftSubRes(type, name, reqSkill, message));
		}

		public void AddSubRes2(Type type, int name, double reqSkill, int genericName, object message)
		{
			CraftSubRes2.Add(new CraftSubRes(type, name, reqSkill, genericName, message));
		}

		public void AddSubRes2(Type type, string name, double reqSkill, object message)
		{
			CraftSubRes2.Add(new CraftSubRes(type, name, reqSkill, message));
		}

		public abstract void InitCraftList();

		public abstract void PlayCraftEffect(Mobile from);
		public abstract int PlayEndingEffect(Mobile from, bool failed, bool lostMaterial, bool toolBroken, int quality, bool makersMark, CraftItem item);

		public abstract int CanCraft(Mobile from, BaseTool tool, Type itemType);
	}

	#region Recipe

	public class Recipe
	{
		public static Dictionary<int, Recipe> Recipes { get; } = new();

		public static int LargestRecipeID { get; private set; }

		public static void Initialize()
		{
			CommandSystem.Register("LearnAllRecipes", AccessLevel.GameMaster, LearnAllRecipes_OnCommand);
			CommandSystem.Register("ForgetAllRecipes", AccessLevel.GameMaster, ForgetAllRecipes_OnCommand);
		}

		[Usage("LearnAllRecipes")]
		[Description("Teaches a player all available recipes.")]
		private static void LearnAllRecipes_OnCommand(CommandEventArgs e)
		{
			var m = e.Mobile;
			m.SendMessage("Target a player to teach them all of the recipies.");

			_ = m.BeginTarget(-1, false, TargetFlags.None, (from, targeted) =>
			{
				if (targeted is PlayerMobile pm)
				{
					foreach (var kvp in Recipes)
					{
						pm.AcquireRecipe(kvp.Key);
					}

					m.SendMessage("You teach them all of the recipies.");
				}
				else
				{
					m.SendMessage("That is not a player!");
				}
			});
		}

		[Usage("ForgetAllRecipes")]
		[Description("Makes a player forget all the recipies they've learned.")]
		private static void ForgetAllRecipes_OnCommand(CommandEventArgs e)
		{
			var m = e.Mobile;

			m.SendMessage("Target a player to have them forget all of the recipies they've learned.");

			_ = m.BeginTarget(-1, false, TargetFlags.None, (from, targeted) =>
			{
				if (targeted is PlayerMobile pm)
				{
					pm.ResetRecipes();

					m.SendMessage("They forget all their recipies.");
				}
				else
				{
					m.SendMessage("That is not a player!");
				}
			});
		}

		public CraftSystem CraftSystem { get; set; }

		public CraftItem CraftItem { get; set; }

		public int ID { get; }

		private TextDefinition m_TD;

		public TextDefinition TextDefinition => m_TD ??= new TextDefinition(CraftItem.NameNumber, CraftItem.NameString);

		public Recipe(int id, CraftSystem system, CraftItem item)
		{
			ID = id;
			CraftSystem = system;
			CraftItem = item;

			if (Recipes.ContainsKey(id))
			{
				throw new Exception("Attempting to create recipe with pre-existing ID.");
			}

			Recipes.Add(id, this);

			LargestRecipeID = Math.Max(id, LargestRecipeID);
		}
	}

	#endregion

	public abstract class CustomCraft
	{
		public Mobile From { get; }
		public CraftItem CraftItem { get; }
		public CraftSystem CraftSystem { get; }
		public Type TypeRes { get; }
		public BaseTool Tool { get; }
		public int Quality { get; }

		public CustomCraft(Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, BaseTool tool, int quality)
		{
			From = from;
			CraftItem = craftItem;
			CraftSystem = craftSystem;
			TypeRes = typeRes;
			Tool = tool;
			Quality = quality;
		}

		public abstract void EndCraftAction();
		public abstract Item CompleteCraft(out int message);
	}

	#region Crafting Gump Menu

	public class CraftGump : Gump
	{
		private enum CraftPage
		{
			None,
			PickResource,
			PickResource2
		}

		private const int LabelHue = 0x480;

		private const short LabelColor16 = 0x7FFF;
		private const int LabelColor32 = 0xFFFFFF;

		private readonly Mobile m_From;
		private readonly CraftSystem m_CraftSystem;
		private readonly BaseTool m_Tool;

		private readonly CraftPage m_Page;

		public CraftGump(Mobile from, CraftSystem craftSystem, BaseTool tool, object notice) 
			: this(from, craftSystem, tool, notice, CraftPage.None)
		{
		}

		private CraftGump(Mobile from, CraftSystem craftSystem, BaseTool tool, object notice, CraftPage page) 
			: base(40, 40)
		{
			m_From = from;
			m_CraftSystem = craftSystem;
			m_Tool = tool;
			m_Page = page;

			var context = craftSystem.GetContext(from);

			_ = from.CloseGump(typeof(CraftGump));
			_ = from.CloseGump(typeof(CraftGumpItem));

			AddPage(0);

			AddBackground(0, 0, 530, 437, 5054);
			AddImageTiled(10, 10, 510, 22, 2624);
			AddImageTiled(10, 292, 150, 45, 2624);
			AddImageTiled(165, 292, 355, 45, 2624);
			AddImageTiled(10, 342, 510, 85, 2624);
			AddImageTiled(10, 37, 200, 250, 2624);
			AddImageTiled(215, 37, 305, 250, 2624);
			AddAlphaRegion(10, 10, 510, 417);

			if (craftSystem.GumpTitleNumber > 0)
			{
				AddHtmlLocalized(10, 12, 510, 20, craftSystem.GumpTitleNumber, LabelColor16, false, false);
			}
			else
			{
				AddHtml(10, 12, 510, 20, craftSystem.GumpTitleString, false, false);
			}

			AddHtmlLocalized(10, 37, 200, 22, 1044010, LabelColor16, false, false); // <CENTER>CATEGORIES</CENTER>
			AddHtmlLocalized(215, 37, 305, 22, 1044011, LabelColor16, false, false); // <CENTER>SELECTIONS</CENTER>
			AddHtmlLocalized(10, 302, 150, 25, 1044012, LabelColor16, false, false); // <CENTER>NOTICES</CENTER>

			AddButton(15, 402, 4017, 4019, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(50, 405, 150, 18, 1011441, LabelColor16, false, false); // EXIT

			AddButton(270, 402, 4005, 4007, GetButtonID(6, 2), GumpButtonType.Reply, 0);
			AddHtmlLocalized(305, 405, 150, 18, 1044013, LabelColor16, false, false); // MAKE LAST

			// Mark option
			if (craftSystem.MarkOption)
			{
				AddButton(270, 362, 4005, 4007, GetButtonID(6, 6), GumpButtonType.Reply, 0);
				AddHtmlLocalized(305, 365, 150, 18, 1044017 + (int)(context?.MarkOption ?? 0), LabelColor16, false, false);// MARK ITEM
			}
			// ****************************************

			// Resmelt option
			if (craftSystem.Resmelt)
			{
				AddButton(15, 342, 4005, 4007, GetButtonID(6, 1), GumpButtonType.Reply, 0);
				AddHtmlLocalized(50, 345, 150, 18, 1044259, LabelColor16, false, false); // SMELT ITEM
			}
			// ****************************************

			// Repair option
			if (craftSystem.Repair)
			{
				AddButton(270, 342, 4005, 4007, GetButtonID(6, 5), GumpButtonType.Reply, 0);
				AddHtmlLocalized(305, 345, 150, 18, 1044260, LabelColor16, false, false); // REPAIR ITEM
			}
			// ****************************************

			// Enhance option
			if (craftSystem.CanEnhance)
			{
				AddButton(270, 382, 4005, 4007, GetButtonID(6, 8), GumpButtonType.Reply, 0);
				AddHtmlLocalized(305, 385, 150, 18, 1061001, LabelColor16, false, false); // ENHANCE ITEM
			}
			// ****************************************

			if (notice is int num && num > 0)
			{
				AddHtmlLocalized(170, 295, 350, 40, num, LabelColor16, false, false);
			}
			else if (notice is string str)
			{
				AddHtml(170, 295, 350, 40, $"<BASEFONT COLOR=#{LabelColor32:X6}>{str}", false, false);
			}

			// If the system has more than one resource
			if (craftSystem.CraftSubRes.Init)
			{
				var nameString = craftSystem.CraftSubRes.NameString;
				var nameNumber = craftSystem.CraftSubRes.NameNumber;

				var resIndex = context == null ? -1 : context.LastResourceIndex;

				var resourceType = craftSystem.CraftSubRes.ResType;

				if (resIndex > -1)
				{
					var subResource = craftSystem.CraftSubRes.GetAt(resIndex);

					nameString = subResource.NameString;
					nameNumber = subResource.NameNumber;
					resourceType = subResource.ItemType;
				}

				var resourceCount = 0;

				if (from.Backpack != null)
				{
					resourceCount += from.Backpack.GetAmount(resourceType, true);
				}

				AddButton(15, 362, 4005, 4007, GetButtonID(6, 0), GumpButtonType.Reply, 0);

				if (nameNumber > 0)
				{
					AddHtmlLocalized(50, 365, 250, 18, nameNumber, $"{resourceCount:N0}", LabelColor16, false, false);
				}
				else
				{
					AddLabel(50, 362, LabelHue, $"{nameString} ({resourceCount:N0} Available)");
				}
			}
			// ****************************************

			// For dragon scales
			if (craftSystem.CraftSubRes2.Init)
			{
				var nameString = craftSystem.CraftSubRes2.NameString;
				var nameNumber = craftSystem.CraftSubRes2.NameNumber;

				var resIndex = context == null ? -1 : context.LastResourceIndex2;

				var resourceType = craftSystem.CraftSubRes2.ResType;

				if (resIndex > -1)
				{
					var subResource = craftSystem.CraftSubRes2.GetAt(resIndex);

					nameString = subResource.NameString;
					nameNumber = subResource.NameNumber;
					resourceType = subResource.ItemType;
				}

				var resourceCount = 0;

				if (from.Backpack != null)
				{
					resourceCount += from.Backpack.GetAmount(resourceType, true);
				}

				AddButton(15, 382, 4005, 4007, GetButtonID(6, 7), GumpButtonType.Reply, 0);

				if (nameNumber > 0)
				{
					AddHtmlLocalized(50, 385, 250, 18, nameNumber, $"{resourceCount:N0}", LabelColor16, false, false);
				}
				else
				{
					AddLabel(50, 385, LabelHue, $"{nameString} ({resourceCount:N0} Available)");
				}
			}
			// ****************************************

			_ = CreateGroupList();

			if (page == CraftPage.PickResource)
			{
				CreateResList(false, from);
			}
			else if (page == CraftPage.PickResource2)
			{
				CreateResList(true, from);
			}
			else if (context != null && context.LastGroupIndex > -1)
			{
				CreateItemList(context.LastGroupIndex);
			}
		}

		public void CreateResList(bool opt, Mobile from)
		{
			var res = opt ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes;

			for (var i = 0; i < res.Count; ++i)
			{
				var index = i % 10;

				var subResource = res.GetAt(i);

				if (index == 0)
				{
					if (i > 0)
					{
						AddButton(485, 260, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1);
					}

					AddPage((i / 10) + 1);

					if (i > 0)
					{
						AddButton(455, 260, 4014, 4015, 0, GumpButtonType.Page, i / 10);
					}

					var context = m_CraftSystem.GetContext(m_From);

					AddButton(220, 260, 4005, 4007, GetButtonID(6, 4), GumpButtonType.Reply, 0);
					AddHtmlLocalized(255, 263, 200, 18, (context == null || !context.DoNotColor) ? 1061591 : 1061590, LabelColor16, false, false);
				}

				var resourceCount = 0;

				if (from.Backpack != null)
				{
					resourceCount += from.Backpack.GetAmount(subResource.ItemType, true);
				}

				AddButton(220, 60 + (index * 20), 4005, 4007, GetButtonID(5, i), GumpButtonType.Reply, 0);

				if (subResource.NameNumber > 0)
				{
					AddHtmlLocalized(255, 63 + (index * 20), 250, 18, subResource.NameNumber, $"{resourceCount:N0}", LabelColor16, false, false);
				}
				else
				{
					AddLabel(255, 60 + (index * 20), LabelHue, $"{subResource.NameString} ({resourceCount:N0})");
				}
			}
		}

		public void CreateMakeLastList()
		{
			var context = m_CraftSystem.GetContext(m_From);

			if (context == null)
			{
				return;
			}

			var items = context.Items;

			if (items.Count > 0)
			{
				for (var i = 0; i < items.Count; ++i)
				{
					var index = i % 10;

					var craftItem = items[i];

					if (index == 0)
					{
						if (i > 0)
						{
							AddButton(370, 260, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1);
							AddHtmlLocalized(405, 263, 100, 18, 1044045, LabelColor16, false, false); // NEXT PAGE
						}

						AddPage((i / 10) + 1);

						if (i > 0)
						{
							AddButton(220, 260, 4014, 4015, 0, GumpButtonType.Page, i / 10);
							AddHtmlLocalized(255, 263, 100, 18, 1044044, LabelColor16, false, false); // PREV PAGE
						}
					}

					AddButton(220, 60 + (index * 20), 4005, 4007, GetButtonID(3, i), GumpButtonType.Reply, 0);

					if (craftItem.NameNumber > 0)
					{
						AddHtmlLocalized(255, 63 + (index * 20), 220, 18, craftItem.NameNumber, LabelColor16, false, false);
					}
					else
					{
						AddLabel(255, 60 + (index * 20), LabelHue, craftItem.NameString);
					}

					AddButton(480, 60 + (index * 20), 4011, 4012, GetButtonID(4, i), GumpButtonType.Reply, 0);
				}
			}
			else
			{
				// NOTE: This is not as OSI; it is an intentional difference

				AddHtmlLocalized(230, 62, 200, 22, 1044165, LabelColor16, false, false); // You haven't made anything yet.
			}
		}

		public void CreateItemList(int selectedGroup)
		{
			if (selectedGroup == 501) // 501 : Last 10
			{
				CreateMakeLastList();
				return;
			}

			var craftGroupCol = m_CraftSystem.CraftGroups;
			var craftGroup = craftGroupCol.GetAt(selectedGroup);
			var craftItemCol = craftGroup.CraftItems;

			for (var i = 0; i < craftItemCol.Count; ++i)
			{
				var index = i % 10;

				var craftItem = craftItemCol.GetAt(i);

				if (index == 0)
				{
					if (i > 0)
					{
						AddButton(370, 260, 4005, 4007, 0, GumpButtonType.Page, (i / 10) + 1);
						AddHtmlLocalized(405, 263, 100, 18, 1044045, LabelColor16, false, false); // NEXT PAGE
					}

					AddPage((i / 10) + 1);

					if (i > 0)
					{
						AddButton(220, 260, 4014, 4015, 0, GumpButtonType.Page, i / 10);
						AddHtmlLocalized(255, 263, 100, 18, 1044044, LabelColor16, false, false); // PREV PAGE
					}
				}

				AddButton(220, 60 + (index * 20), 4005, 4007, GetButtonID(1, i), GumpButtonType.Reply, 0);

				if (craftItem.NameNumber > 0)
				{
					AddHtmlLocalized(255, 63 + (index * 20), 220, 18, craftItem.NameNumber, LabelColor16, false, false);
				}
				else
				{
					AddLabel(255, 60 + (index * 20), LabelHue, craftItem.NameString);
				}

				AddButton(480, 60 + (index * 20), 4011, 4012, GetButtonID(2, i), GumpButtonType.Reply, 0);
			}
		}

		public int CreateGroupList()
		{
			var craftGroupCol = m_CraftSystem.CraftGroups;

			AddButton(15, 60, 4005, 4007, GetButtonID(6, 3), GumpButtonType.Reply, 0);
			AddHtmlLocalized(50, 63, 150, 18, 1044014, LabelColor16, false, false); // LAST TEN

			for (var i = 0; i < craftGroupCol.Count; i++)
			{
				var craftGroup = craftGroupCol.GetAt(i);

				AddButton(15, 80 + (i * 20), 4005, 4007, GetButtonID(0, i), GumpButtonType.Reply, 0);

				if (craftGroup.NameNumber > 0)
				{
					AddHtmlLocalized(50, 83 + (i * 20), 150, 18, craftGroup.NameNumber, LabelColor16, false, false);
				}
				else
				{
					AddLabel(50, 80 + (i * 20), LabelHue, craftGroup.NameString);
				}
			}

			return craftGroupCol.Count;
		}

		public static int GetButtonID(int type, int index)
		{
			return 1 + type + (index * 7);
		}

		public void CraftItem(CraftItem item)
		{
			var num = m_CraftSystem.CanCraft(m_From, m_Tool, item.ItemType);

			if (num > 0)
			{
				_ = m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, num));
			}
			else
			{
				Type type = null;

				var context = m_CraftSystem.GetContext(m_From);

				if (context != null)
				{
					var res = item.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes;
					var resIndex = item.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;

					if (resIndex >= 0 && resIndex < res.Count)
					{
						type = res.GetAt(resIndex).ItemType;
					}
				}

				m_CraftSystem.CreateItem(m_From, item.ItemType, type, m_Tool, item);
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID <= 0)
			{
				return; // Canceled
			}

			var buttonID = info.ButtonID - 1;
			var type = buttonID % 7;
			var index = buttonID / 7;

			var system = m_CraftSystem;
			var groups = system.CraftGroups;
			var context = system.GetContext(m_From);

			switch (type)
			{
				case 0: // Show group
					{
						if (context == null)
						{
							break;
						}

						if (index >= 0 && index < groups.Count)
						{
							context.LastGroupIndex = index;

							_ = m_From.SendGump(new CraftGump(m_From, system, m_Tool, null));
						}

						break;
					}
				case 1: // Create item
					{
						if (context == null)
						{
							break;
						}

						var groupIndex = context.LastGroupIndex;

						if (groupIndex >= 0 && groupIndex < groups.Count)
						{
							var group = groups.GetAt(groupIndex);

							if (index >= 0 && index < group.CraftItems.Count)
							{
								CraftItem(group.CraftItems.GetAt(index));
							}
						}

						break;
					}
				case 2: // Item details
					{
						if (context == null)
						{
							break;
						}

						var groupIndex = context.LastGroupIndex;

						if (groupIndex >= 0 && groupIndex < groups.Count)
						{
							var group = groups.GetAt(groupIndex);

							if (index >= 0 && index < group.CraftItems.Count)
							{
								_ = m_From.SendGump(new CraftGumpItem(m_From, system, group.CraftItems.GetAt(index), m_Tool));
							}
						}

						break;
					}
				case 3: // Create item (last 10)
					{
						if (context == null)
						{
							break;
						}

						var lastTen = context.Items;

						if (index >= 0 && index < lastTen.Count)
						{
							CraftItem(lastTen[index]);
						}

						break;
					}
				case 4: // Item details (last 10)
					{
						if (context == null)
						{
							break;
						}

						var lastTen = context.Items;

						if (index >= 0 && index < lastTen.Count)
						{
							_ = m_From.SendGump(new CraftGumpItem(m_From, system, lastTen[index], m_Tool));
						}

						break;
					}
				case 5: // Resource selected
					{
						if (m_Page == CraftPage.PickResource && index >= 0 && index < system.CraftSubRes.Count)
						{
							var res = system.CraftSubRes.GetAt(index);

							if (m_From.Skills[system.MainSkill].Base < res.RequiredSkill)
							{
								_ = m_From.SendGump(new CraftGump(m_From, system, m_Tool, res.Message));
							}
							else
							{
								if (context != null)
								{
									context.LastResourceIndex = index;
								}

								_ = m_From.SendGump(new CraftGump(m_From, system, m_Tool, null));
							}
						}
						else if (m_Page == CraftPage.PickResource2 && index >= 0 && index < system.CraftSubRes2.Count)
						{
							var res = system.CraftSubRes2.GetAt(index);

							if (m_From.Skills[system.MainSkill].Base < res.RequiredSkill)
							{
								_ = m_From.SendGump(new CraftGump(m_From, system, m_Tool, res.Message));
							}
							else
							{
								if (context != null)
								{
									context.LastResourceIndex2 = index;
								}

								_ = m_From.SendGump(new CraftGump(m_From, system, m_Tool, null));
							}
						}

						break;
					}
				case 6: // Misc. buttons
					{
						switch (index)
						{
							case 0: // Resource selection
								{
									if (system.CraftSubRes.Init)
									{
										_ = m_From.SendGump(new CraftGump(m_From, system, m_Tool, null, CraftPage.PickResource));
									}

									break;
								}
							case 1: // Smelt item
								{
									if (system.Resmelt)
									{
										Resmelt.Do(m_From, system, m_Tool);
									}

									break;
								}
							case 2: // Make last
								{
									if (context == null)
									{
										break;
									}

									var item = context.LastMade;

									if (item != null)
									{
										CraftItem(item);
									}
									else
									{
										_ = m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, 1044165, m_Page)); // You haven't made anything yet.
									}

									break;
								}
							case 3: // Last 10
								{
									if (context == null)
									{
										break;
									}

									context.LastGroupIndex = 501;

									_ = m_From.SendGump(new CraftGump(m_From, system, m_Tool, null));

									break;
								}
							case 4: // Toggle use resource hue
								{
									if (context == null)
									{
										break;
									}

									context.DoNotColor = !context.DoNotColor;

									_ = m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, null, m_Page));

									break;
								}
							case 5: // Repair item
								{
									if (system.Repair)
									{
										Repair.Do(m_From, system, m_Tool);
									}

									break;
								}
							case 6: // Toggle mark option
								{
									if (context == null || !system.MarkOption)
									{
										break;
									}

									switch (context.MarkOption)
									{
										case CraftMarkOption.MarkItem: context.MarkOption = CraftMarkOption.DoNotMark; break;
										case CraftMarkOption.DoNotMark: context.MarkOption = CraftMarkOption.PromptForMark; break;
										case CraftMarkOption.PromptForMark: context.MarkOption = CraftMarkOption.MarkItem; break;
									}

									_ = m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, null, m_Page));

									break;
								}
							case 7: // Resource selection 2
								{
									if (system.CraftSubRes2.Init)
									{
										_ = m_From.SendGump(new CraftGump(m_From, system, m_Tool, null, CraftPage.PickResource2));
									}

									break;
								}
							case 8: // Enhance item
								{
									if (system.CanEnhance)
									{
										Enhance.BeginTarget(m_From, system, m_Tool);
									}

									break;
								}
						}

						break;
					}
			}
		}
	}

	public class CraftGumpItem : Gump
	{
		private readonly Mobile m_From;
		private readonly CraftSystem m_CraftSystem;
		private readonly CraftItem m_CraftItem;
		private readonly BaseTool m_Tool;

		private const int LabelHue = 0x480;

		private const short LabelColor16 = 0x7FFF;
		private const short RedLabelColor16 = 0x6400;
		private const short GreyLabelColor16 = 0x3DEF;

		private const int LabelColor32 = 0xFFFFFF;
		private const int RedLabelColor32 = 0xC80000;
		//private const int GreyLabelColor32 = 0x787878;

		private int m_OtherCount;

		public CraftGumpItem(Mobile from, CraftSystem craftSystem, CraftItem craftItem, BaseTool tool) : base(40, 40)
		{
			m_From = from;
			m_CraftSystem = craftSystem;
			m_CraftItem = craftItem;
			m_Tool = tool;

			_ = from.CloseGump(typeof(CraftGump));
			_ = from.CloseGump(typeof(CraftGumpItem));

			AddPage(0);

			AddBackground(0, 0, 530, 417, 5054);
			AddImageTiled(10, 10, 510, 22, 2624);
			AddImageTiled(10, 37, 150, 148, 2624);
			AddImageTiled(165, 37, 355, 90, 2624);
			AddImageTiled(10, 190, 155, 22, 2624);
			AddImageTiled(10, 217, 150, 53, 2624);
			AddImageTiled(165, 132, 355, 80, 2624);
			AddImageTiled(10, 275, 155, 22, 2624);
			AddImageTiled(10, 302, 150, 53, 2624);
			AddImageTiled(165, 217, 355, 80, 2624);
			AddImageTiled(10, 360, 155, 22, 2624);
			AddImageTiled(165, 302, 355, 80, 2624);
			AddImageTiled(10, 387, 510, 22, 2624);

			AddAlphaRegion(10, 10, 510, 399);

			AddHtmlLocalized(170, 40, 150, 20, 1044053, LabelColor16, false, false); // ITEM
			AddHtmlLocalized(10, 192, 150, 22, 1044054, LabelColor16, false, false); // <CENTER>SKILLS</CENTER>
			AddHtmlLocalized(10, 277, 150, 22, 1044055, LabelColor16, false, false); // <CENTER>MATERIALS</CENTER>
			AddHtmlLocalized(10, 362, 150, 22, 1044056, LabelColor16, false, false); // <CENTER>OTHER</CENTER>

			if (craftSystem.GumpTitleNumber > 0)
			{
				AddHtmlLocalized(10, 12, 510, 20, craftSystem.GumpTitleNumber, LabelColor16, false, false);
			}
			else
			{
				AddHtml(10, 12, 510, 20, craftSystem.GumpTitleString, false, false);
			}

			AddButton(15, 387, 4014, 4016, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(50, 390, 150, 18, 1044150, LabelColor16, false, false); // BACK

			var needsRecipe = craftItem.Recipe != null && from is PlayerMobile pm && !pm.HasRecipe(craftItem.Recipe);

			if (needsRecipe)
			{
				AddButton(270, 387, 4005, 4007, 0, GumpButtonType.Page, 0);
				AddHtmlLocalized(305, 390, 150, 18, 1044151, GreyLabelColor16, false, false); // MAKE NOW
			}
			else
			{
				AddButton(270, 387, 4005, 4007, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(305, 390, 150, 18, 1044151, LabelColor16, false, false); // MAKE NOW
			}

			if (craftItem.NameNumber > 0)
			{
				AddHtmlLocalized(330, 40, 180, 18, craftItem.NameNumber, LabelColor16, false, false);
			}
			else
			{
				AddLabel(330, 40, LabelHue, craftItem.NameString);
			}

			if (craftItem.UseAllRes)
			{
				AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 310, 18, 1048176, LabelColor16, false, false); // Makes as many as possible at once
			}

			DrawItem();
			DrawSkill();
			DrawResource();

			if (craftItem.RequiredExpansion != Expansion.None)
			{
				var supportsEx = from.NetState != null && from.NetState.SupportsExpansion(craftItem.RequiredExpansion);

				var color16 = supportsEx ? LabelColor16 : RedLabelColor16;
				var color32 = supportsEx ? LabelColor32 : RedLabelColor32;

				TextDefinition.AddHtmlText(this, 170, 302 + (m_OtherCount++ * 20), 310, 18, RequiredExpansionMessage(craftItem.RequiredExpansion), false, false, color16, color32);
			}

			if (needsRecipe)
			{
				AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 310, 18, 1073620, RedLabelColor16, false, false); // You have not learned this recipe.
			}
		}

		private static TextDefinition RequiredExpansionMessage(Expansion expansion)
		{
			return expansion switch
			{
				Expansion.SE => 1063363, // * Requires the "Samurai Empire" expansion
				Expansion.ML => 1072651, // * Requires the "Mondain's Legacy" expansion
				_ => $"* Requires the \"{ExpansionInfo.GetInfo(expansion).Name}\" expansion",
			};
		}

		private bool m_ShowExceptionalChance;

		public void DrawItem()
		{
			var type = m_CraftItem.ItemType;

			AddItem(20, 50, CraftItem.ItemIDOf(type), m_CraftItem.ItemHue);

			if (m_CraftItem.IsMarkable(type))
			{
				// This item may hold its maker's mark
				AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 310, 18, 1044059, LabelColor16, false, false);

				m_ShowExceptionalChance = true;
			}
		}

		public void DrawSkill()
		{
			for (var i = 0; i < m_CraftItem.Skills.Count; i++)
			{
				var skill = m_CraftItem.Skills.GetAt(i);
				var minSkill = Math.Max(0, skill.MinSkill);

				AddHtmlLocalized(170, 132 + (i * 20), 200, 18, AosSkillBonuses.GetLabel(skill.SkillToMake), LabelColor16, false, false);
				AddLabel(430, 132 + (i * 20), LabelHue, $"{minSkill:F1}");
			}

			var res = m_CraftItem.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes;
			var resIndex = -1;

			var context = m_CraftSystem.GetContext(m_From);

			if (context != null)
			{
				resIndex = m_CraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;
			}

			var allRequiredSkills = true;

			var chance = m_CraftItem.GetSuccessChance(m_From, resIndex >= 0 ? res.GetAt(resIndex)?.ItemType : null, m_CraftSystem, false, ref allRequiredSkills);

			chance = Math.Clamp(chance, 0.0, 1.0);

			var excepChance = m_CraftItem.GetExceptionalChance(m_CraftSystem, chance, m_From);

			excepChance = Math.Clamp(excepChance, 0.0, 1.0);

			AddHtmlLocalized(170, 80, 250, 18, 1044057, LabelColor16, false, false); // Success Chance:
			AddLabel(430, 80, LabelHue, $"{chance * 100:F1}%");

			if (m_ShowExceptionalChance)
			{
				AddHtmlLocalized(170, 100, 250, 18, 1044058, LabelColor16, false, false); // Exceptional Chance:
				AddLabel(430, 100, LabelHue, $"{excepChance * 100:F1}%");
			}
		}

		private static readonly Type typeofBlankScroll = typeof(BlankScroll);
		private static readonly Type typeofSpellScroll = typeof(SpellScroll);

		public void DrawResource()
		{
			var retainedColor = false;

			var context = m_CraftSystem.GetContext(m_From);

			var res = m_CraftItem.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes;
			var resIndex = -1;

			if (context != null)
			{
				resIndex = m_CraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;
			}

			var cropScroll = m_CraftItem.Resources.Count > 1
						  && m_CraftItem.Resources.GetAt(m_CraftItem.Resources.Count - 1)?.ItemType == typeofBlankScroll
						  && typeofSpellScroll.IsAssignableFrom(m_CraftItem.ItemType);

			for (var i = 0; i < m_CraftItem.Resources.Count - (cropScroll ? 1 : 0) && i < 4; i++)
			{
				Type type;
				string nameString;
				int nameNumber;

				var craftResource = m_CraftItem.Resources.GetAt(i);

				type = craftResource.ItemType;
				nameString = craftResource.NameString;
				nameNumber = craftResource.NameNumber;

				// Resource Mutation
				if (type == res.ResType && resIndex > -1)
				{
					var subResource = res.GetAt(resIndex);

					type = subResource.ItemType;

					nameString = subResource.NameString;
					nameNumber = subResource.GenericNameNumber;

					if (nameNumber <= 0)
					{
						nameNumber = subResource.NameNumber;
					}
				}
				// ******************

				if (!retainedColor && m_CraftItem.RetainsColorFrom(m_CraftSystem, type))
				{
					retainedColor = true;

					AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 310, 18, 1044152, LabelColor16, false, false); // * The item retains the color of this material
					AddLabel(500, 219 + (i * 20), LabelHue, "*");
				}

				if (nameNumber > 0)
				{
					AddHtmlLocalized(170, 219 + (i * 20), 310, 18, nameNumber, LabelColor16, false, false);
				}
				else
				{
					AddLabel(170, 219 + (i * 20), LabelHue, nameString);
				}

				AddLabel(430, 219 + (i * 20), LabelHue, craftResource.Amount.ToString());
			}

			if (m_CraftItem.NameNumber == 1041267) // runebook
			{
				AddHtmlLocalized(170, 219 + (m_CraftItem.Resources.Count * 20), 310, 18, 1044447, LabelColor16, false, false);
				AddLabel(430, 219 + (m_CraftItem.Resources.Count * 20), LabelHue, "1");
			}

			if (cropScroll)
			{
				AddHtmlLocalized(170, 302 + (m_OtherCount++ * 20), 360, 18, 1044379, LabelColor16, false, false); // Inscribing scrolls also requires a blank scroll and mana.
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			// Back Button
			if (info.ButtonID == 0)
			{
				_ = m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, null));
			}
			else // Make Button
			{
				var num = m_CraftSystem.CanCraft(m_From, m_Tool, m_CraftItem.ItemType);

				if (num > 0)
				{
					_ = m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, num));
				}
				else
				{
					Type type = null;

					var context = m_CraftSystem.GetContext(m_From);

					if (context != null)
					{
						var res = m_CraftItem.UseSubRes2 ? m_CraftSystem.CraftSubRes2 : m_CraftSystem.CraftSubRes;
						var resIndex = m_CraftItem.UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex;

						if (resIndex > -1)
						{
							type = res.GetAt(resIndex)?.ItemType;
						}
					}

					m_CraftSystem.CreateItem(m_From, m_CraftItem.ItemType, type, m_Tool, m_CraftItem);
				}
			}
		}
	}

	/// Craft Group 
	public class CraftGroup
	{
		public CraftItemCol CraftItems { get; } = new();

		public string NameString { get; }
		public int NameNumber { get; }

		public CraftGroup(TextDefinition groupName)
		{
			NameNumber = groupName;
			NameString = groupName;
		}

		public void AddCraftItem(CraftItem craftItem)
		{
			_ = CraftItems.Add(craftItem);
		}
	}

	public class CraftGroupCol : System.Collections.CollectionBase
	{
		public CraftGroupCol()
		{
		}

		public int Add(CraftGroup craftGroup)
		{
			return List.Add(craftGroup);
		}

		public void Remove(int index)
		{
			if (index >= 0 && index < Count)
			{
				List.RemoveAt(index);
			}
		}

		public CraftGroup GetAt(int index)
		{
			return (CraftGroup)List[index];
		}

		public int SearchFor(TextDefinition groupName)
		{
			for (var i = 0; i < List.Count; i++)
			{
				var craftGroup = (CraftGroup)List[i];

				var nameNumber = craftGroup.NameNumber;
				var nameString = craftGroup.NameString;

				if ((nameNumber != 0 && nameNumber == groupName.Number) || (nameString != null && nameString == groupName.String))
				{
					return i;
				}
			}

			return -1;
		}
	}

	/// Craft Skill
	public class CraftSkill
	{
		public SkillName SkillToMake { get; }

		public double MinSkill { get; }
		public double MaxSkill { get; }

		public CraftSkill(SkillName skillToMake, double minSkill, double maxSkill)
		{
			SkillToMake = skillToMake;
			MinSkill = minSkill;
			MaxSkill = maxSkill;
		}
	}

	public class CraftSkillCol : System.Collections.CollectionBase
	{
		public CraftSkillCol()
		{
		}

		public void Add(CraftSkill craftSkill)
		{
			_ = List.Add(craftSkill);
		}

		public void Remove(int index)
		{
			if (index >= 0 && index < Count)
			{
				List.RemoveAt(index);
			}
		}

		public CraftSkill GetAt(int index)
		{
			return (CraftSkill)List[index];
		}
	}

	/// Craft Item
	[AttributeUsage(AttributeTargets.Class)]
	public class CraftItemIDAttribute : Attribute
	{
		public int ItemID { get; }

		public CraftItemIDAttribute(int itemID)
		{
			ItemID = itemID;
		}
	}

	public enum ConsumeType
	{
		All, Half, None
	}

	public class CraftItem : ICraftItem
	{
		private static readonly Dictionary<Type, int> _ItemIds = new();

		#region Tables

		private static readonly int[] m_HeatSources =
		{
			0x461, 0x48E, // Sandstone oven/fireplace
			0x92B, 0x96C, // Stone oven/fireplace
			0xDE3, 0xDE9, // Campfire
			0xFAC, 0xFAC, // Firepit
			0x184A, 0x184C, // Heating stand (left)
			0x184E, 0x1850, // Heating stand (right)
			0x398C, 0x399F,  // Fire field
			0x2DDB, 0x2DDC,	//Elven stove
			0x19AA, 0x19BB,	// Veteran Reward Brazier
			0x197A, 0x19A9, // Large Forge 
			0x0FB1, 0x0FB1, // Small Forge
			0x2DD8, 0x2DD8 // Elven Forge
		};

		private static readonly int[] m_Ovens =
		{
			0x461, 0x46F, // Sandstone oven
			0x92B, 0x93F,  // Stone oven
			0x2DDB, 0x2DDC	//Elven stove
		};

		private static readonly int[] m_Mills =
		{
			0x1920, 0x1921, 0x1922, 0x1923, 0x1924, 0x1295, 0x1926, 0x1928,
			0x192C, 0x192D, 0x192E, 0x129F, 0x1930, 0x1931, 0x1932, 0x1934
		};

		private static readonly Type[][] m_TypesTable =
		{
			new[]{ typeof( Log ), typeof( Board ) },
			new[]{ typeof( HeartwoodLog ), typeof( HeartwoodBoard ) },
			new[]{ typeof( BloodwoodLog ), typeof( BloodwoodBoard ) },
			new[]{ typeof( FrostwoodLog ), typeof( FrostwoodBoard ) },
			new[]{ typeof( OakLog ), typeof( OakBoard ) },
			new[]{ typeof( AshLog ), typeof( AshBoard ) },
			new[]{ typeof( YewLog ), typeof( YewBoard ) },
			new[]{ typeof( Leather ), typeof( Hides ) },
			new[]{ typeof( SpinedLeather ), typeof( SpinedHides ) },
			new[]{ typeof( HornedLeather ), typeof( HornedHides ) },
			new[]{ typeof( BarbedLeather ), typeof( BarbedHides ) },
			new[]{ typeof( BlankMap ), typeof( BlankScroll ) },
			new[]{ typeof( Cloth ), typeof( UncutCloth ) },
			new[]{ typeof( CheeseWheel ), typeof( CheeseWedge ) },
			new[]{ typeof( Pumpkin ), typeof( SmallPumpkin ) },
			new[]{ typeof( WoodenBowlOfPeas ), typeof( PewterBowlOfPeas ) }
		};

		private static readonly Type[] m_ColoredItemTable =
		{
			typeof( BaseWeapon ), typeof( BaseArmor ), typeof( BaseClothing ),
			typeof( BaseJewel ), typeof( DragonBardingDeed )
		};

		private static readonly Type[] m_ColoredResourceTable =
		{
			typeof( BaseIngot ), typeof( BaseOre ),
			typeof( BaseLeather ), typeof( BaseHides ),
			typeof( UncutCloth ), typeof( Cloth ),
			typeof( BaseGranite ), typeof( BaseScales )
		};

		private static readonly Type[] m_MarkableTable =
		{
			typeof( BaseArmor ),
			typeof( BaseWeapon ),
			typeof( BaseClothing ),
			typeof( BaseInstrument ),
			typeof( DragonBardingDeed ),
			typeof( BaseTool ),
			typeof( BaseHarvestTool ),
			typeof( FukiyaDarts ), typeof( Shuriken ),
			typeof( Spellbook ), typeof( Runebook ),
			typeof( BaseQuiver )
		};

		private static readonly Type[] m_NeverColorTable =
		{
			typeof( OrcHelm )
		};

		#endregion

		public static int LabelNumber(Type type)
		{
			var number = ItemIDOf(type);

			if (number >= 0x4000)
			{
				number += 1078872;
			}
			else
			{
				number += 1020000;
			}

			return number;
		}

		public static int ItemIDOf(Type type)
		{
			int itemId;

			if (!_ItemIds.TryGetValue(type, out itemId))
			{
				if (type == typeof(FactionExplosionTrap))
				{
					itemId = 14034;
				}
				else if (type == typeof(FactionGasTrap))
				{
					itemId = 4523;
				}
				else if (type == typeof(FactionSawTrap))
				{
					itemId = 4359;
				}
				else if (type == typeof(FactionSpikeTrap))
				{
					itemId = 4517;
				}

				if (itemId == 0)
				{
					var craftItemID = type.GetCustomAttribute<CraftItemIDAttribute>(false);

					if (craftItemID != null)
					{
						itemId = craftItemID.ItemID;
					}
				}

				if (itemId == 0)
				{
					var item = Utility.CreateInstance<Item>(type);

					if (item != null)
					{
						itemId = item.ItemID;
						item.Delete();
					}
				}

				_ItemIds[type] = itemId;
			}

			return itemId;
		}

		public static bool RetainsColor(Type type)
		{
			var neverColor = false;

			for (var i = 0; !neverColor && i < m_NeverColorTable.Length; ++i)
			{
				neverColor = type == m_NeverColorTable[i] || type.IsSubclassOf(m_NeverColorTable[i]);
			}

			if (neverColor)
			{
				return false;
			}

			var inItemTable = false;

			for (var i = 0; !inItemTable && i < m_ColoredItemTable.Length; ++i)
			{
				inItemTable = type == m_ColoredItemTable[i] || type.IsSubclassOf(m_ColoredItemTable[i]);
			}

			return inItemTable;
		}

		public static bool Find(Mobile from, int[] itemIDs)
		{
			var map = from.Map;

			if (map == null)
			{
				return false;
			}

			var eable = map.GetItemsInRange(from.Location, 2);

			foreach (var item in eable)
			{
				if (item.Z + 16 > from.Z && from.Z + 16 > item.Z && Find(item.ItemID, itemIDs))
				{
					eable.Free();
					return true;
				}
			}

			eable.Free();

			for (var x = -2; x <= 2; ++x)
			{
				for (var y = -2; y <= 2; ++y)
				{
					var vx = from.X + x;
					var vy = from.Y + y;

					var tiles = map.Tiles.GetStaticTiles(vx, vy, true);

					for (var i = 0; i < tiles.Length; ++i)
					{
						var z = tiles[i].Z;
						var id = tiles[i].ID;

						if (z + 16 > from.Z && from.Z + 16 > z && Find(id, itemIDs))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public static bool Find(int itemID, int[] itemIDs)
		{
			var contains = false;

			for (var i = 0; !contains && i < itemIDs.Length; i += 2)
			{
				contains = itemID >= itemIDs[i] && itemID <= itemIDs[i + 1];
			}

			return contains;
		}

		public static bool IsQuantityType(Type[][] types)
		{
			for (var i = 0; i < types.Length; ++i)
			{
				var check = types[i];

				for (var j = 0; j < check.Length; ++j)
				{
					if (typeof(IHasQuantity).IsAssignableFrom(check[j]))
					{
						return true;
					}
				}
			}

			return false;
		}

		private static TextDefinition RequiredExpansionMessage(Expansion expansion)
		{
			return expansion switch
			{
				Expansion.SE => 1063307, // The "Samurai Empire" expansion is required to attempt this item.
				Expansion.ML => 1072650, // The "Mondain's Legacy" expansion is required to attempt this item.
				_ => $"The \"{ExpansionInfo.GetInfo(expansion).Name}\" expansion is required to attempt this item."
			};
		}

		private int m_ResHue;
		private int m_ResAmount;

		private CraftSystem m_System;

		public Type ItemType { get; }
		public int ItemHue { get; set; }

		public string GroupNameString { get; }
		public int GroupNameNumber { get; }

		private TextDefinition _GroupName;

		public TextDefinition GroupName => _GroupName ??= new(GroupNameNumber, GroupNameString);

		public string NameString { get; }
		public int NameNumber { get; }

		private TextDefinition _Name;

		public TextDefinition Name => _Name ??= new(NameNumber, NameString);

		public int Hits { get; set; }
		public int Mana { get; set; }
		public int Stam { get; set; }

		public bool UseSubRes2 { get; set; }

		public bool UseAllRes { get; set; }

		public bool NeedHeat { get; set; }
		public bool NeedOven { get; set; }
		public bool NeedMill { get; set; }

		public CraftResCol Resources { get; }

		public CraftSkillCol Skills { get; }

		public bool ForceNonExceptional { get; set; }

		public Expansion RequiredExpansion { get; set; }

		public BeverageType RequiredBeverage { get; set; }

		public Recipe Recipe { get; private set; }

		public CraftItem(Type type, TextDefinition groupName, TextDefinition name)
		{
			Resources = new CraftResCol();
			Skills = new CraftSkillCol();

			ItemType = type;

			GroupNameString = groupName;
			NameString = name;

			GroupNameNumber = groupName;
			NameNumber = name;

			RequiredBeverage = BeverageType.Water;
		}

		public void AddRecipe(int id, CraftSystem system)
		{
			if (Recipe == null)
			{
				Recipe = new Recipe(id, system, this);
			}
			else
			{
				Console.WriteLine($"Warning: Attempted add of recipe #{id} to the crafting of {ItemType.Name} in CraftSystem {system}.");
			}
		}

		public void AddRes(Type type, TextDefinition name, int amount)
		{
			AddRes(type, name, amount, "");
		}

		public void AddRes(Type type, TextDefinition name, int amount, TextDefinition message)
		{
			Resources.Add(new CraftRes(type, name, amount, message));
		}

		public void AddSkill(SkillName skillToMake, double minSkill, double maxSkill)
		{
			Skills.Add(new CraftSkill(skillToMake, minSkill, maxSkill));
		}

		public bool ConsumeAttributes(Mobile from, ref object message, bool consume)
		{
			if (Hits > 0 && from.Hits < Hits)
			{
				message = "You lack the required hit points to make that.";
				return false;
			}

			if (Mana > 0 && from.Mana < Mana)
			{
				message = "You lack the required mana to make that.";
				return false;
			}

			if (Stam > 0 && from.Stam < Stam)
			{
				message = "You lack the required stamina to make that.";
				return false;
			}

			if (consume)
			{
				from.Hits -= Hits;
				from.Mana -= Mana;
				from.Stam -= Stam;
			}

			return true;
		}

		public bool IsMarkable(Type type)
		{
			if (ForceNonExceptional)  //Don't even display the stuff for marking if it can't ever be exceptional.
			{
				return false;
			}

			for (var i = 0; i < m_MarkableTable.Length; ++i)
			{
				if (type == m_MarkableTable[i] || type.IsSubclassOf(m_MarkableTable[i]))
				{
					return true;
				}
			}

			return false;
		}

		public bool RetainsColorFrom(CraftSystem system, Type type)
		{
			if (system.RetainsColorFrom(this, type))
			{
				return true;
			}

			var inItemTable = RetainsColor(ItemType);

			if (!inItemTable)
			{
				return false;
			}

			var inResourceTable = false;

			for (var i = 0; !inResourceTable && i < m_ColoredResourceTable.Length; ++i)
			{
				inResourceTable = type == m_ColoredResourceTable[i] || type.IsSubclassOf(m_ColoredResourceTable[i]);
			}

			return inResourceTable;
		}

		public int ConsumeQuantity(Container cont, Type[][] types, int[] amounts)
		{
			if (types.Length != amounts.Length)
			{
				throw new ArgumentException("Types and amounts must be equal length", nameof(types));
			}

			var items = new Item[types.Length][];
			var totals = new int[types.Length];

			for (var i = 0; i < types.Length; ++i)
			{
				items[i] = cont.FindItemsByType(types[i], true);

				for (var j = 0; j < items[i].Length; ++j)
				{
					if (items[i][j] is IHasQuantity hq)
					{
						if (hq is BaseBeverage bev && bev.Content != RequiredBeverage)
						{
							continue;
						}

						totals[i] += hq.Quantity;
					}
					else
					{
						totals[i] += items[i][j].Amount;
					}
				}

				if (totals[i] < amounts[i])
				{
					return i;
				}
			}

			for (var i = 0; i < types.Length; ++i)
			{
				var need = amounts[i];

				for (var j = 0; j < items[i].Length; ++j)
				{
					var item = items[i][j];

					if (item is IHasQuantity hq)
					{
						if (hq is BaseBeverage bev && bev.Content != RequiredBeverage)
						{
							continue;
						}

						var theirAmount = hq.Quantity;

						if (theirAmount >= need)
						{
							hq.Quantity -= need;

							break;
						}

						need -= theirAmount;

						hq.Quantity -= theirAmount;
					}
					else
					{
						var theirAmount = item.Amount;

						if (theirAmount >= need)
						{
							item.Consume(need);

							break;
						}

						need -= theirAmount;

						item.Delete();
					}
				}
			}

			return -1;
		}

		public int GetQuantity(Container cont, Type[] types)
		{
			var amount = 0;

			foreach (var item in cont.FindItemsByType(types, true))
			{
				if (item is IHasQuantity hq)
				{
					if (hq is BaseBeverage bev && bev.Content != RequiredBeverage)
					{
						continue;
					}

					amount += hq.Quantity;
				}
				else
				{
					amount += item.Amount;
				}
			}

			return amount;
		}

		public bool ConsumeRes(Mobile from, Type typeRes, CraftSystem craftSystem, ref int resHue, ref int maxAmount, ConsumeType consumeType, ref object message)
		{
			return ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, consumeType, ref message, false);
		}

		public bool ConsumeRes(Mobile from, Type typeRes, CraftSystem craftSystem, ref int resHue, ref int maxAmount, ConsumeType consumeType, ref object message, bool isFailure)
		{
			var ourPack = from.Backpack;

			if (ourPack == null)
			{
				return false;
			}

			if (NeedHeat && !Find(from, m_HeatSources))
			{
				message = 1044487; // You must be near a fire source to cook.
				return false;
			}

			if (NeedOven && !Find(from, m_Ovens))
			{
				message = 1044493; // You must be near an oven to bake that.
				return false;
			}

			if (NeedMill && !Find(from, m_Mills))
			{
				message = 1044491; // You must be near a flour mill to do that.
				return false;
			}

			var types = new Type[Resources.Count][];
			var amounts = new int[Resources.Count];

			maxAmount = Int32.MaxValue;

			var resCol = UseSubRes2 ? craftSystem.CraftSubRes2 : craftSystem.CraftSubRes;

			for (var i = 0; i < types.Length; ++i)
			{
				var craftRes = Resources.GetAt(i);
				var baseType = craftRes.ItemType;

				// Resource Mutation
				if (baseType == resCol.ResType && typeRes != null)
				{
					baseType = typeRes;

					var subResource = resCol.SearchFor(baseType);

					if (subResource != null && from.Skills[craftSystem.MainSkill].Base < subResource.RequiredSkill)
					{
						message = subResource.Message;
						return false;
					}
				}
				// ******************

				for (var j = 0; types[i] == null && j < m_TypesTable.Length; ++j)
				{
					if (m_TypesTable[j][0] == baseType)
					{
						types[i] = m_TypesTable[j];
					}
				}

				if (types[i] == null)
				{
					types[i] = new Type[] { baseType };
				}

				amounts[i] = craftRes.Amount;

				// For stackable items that can ben crafted more than one at a time
				if (UseAllRes)
				{
					var tempAmount = ourPack.GetAmount(types[i]);

					tempAmount /= amounts[i];

					if (tempAmount < maxAmount)
					{
						maxAmount = tempAmount;

						if (maxAmount == 0)
						{
							var res = Resources.GetAt(i);

							if (res.MessageNumber > 0)
							{
								message = res.MessageNumber;
							}
							else if (!String.IsNullOrEmpty(res.MessageString))
							{
								message = res.MessageString;
							}
							else
							{
								message = 502925; // You don't have the resources required to make that item.
							}

							return false;
						}
					}
				}
				// ****************************

				if (isFailure && !craftSystem.ConsumeOnFailure(from, types[i][0], this))
				{
					amounts[i] = 0;
				}
			}

			// We adjust the amount of each resource to consume the max posible
			if (UseAllRes)
			{
				for (var i = 0; i < amounts.Length; ++i)
				{
					amounts[i] *= maxAmount;
				}
			}
			else
			{
				maxAmount = -1;
			}

			Item consumeExtra = null;

			if (NameNumber == 1041267)
			{
				// Runebooks are a special case, they need a blank recall rune
				foreach (var rune in ourPack.FindItemsByType<RecallRune>())
				{
					if (rune != null && !rune.Marked)
					{
						consumeExtra = rune;
						break;
					}
				}

				if (consumeExtra == null)
				{
					message = 1044253; // You don't have the components needed to make that.
					return false;
				}
			}

			var index = 0;
						
			if (consumeType == ConsumeType.All) // Consume ALL
			{
				m_ResHue = 0;
				m_ResAmount = 0;
				m_System = craftSystem;

				if (IsQuantityType(types))
				{
					index = ConsumeQuantity(ourPack, types, amounts);
				}
				else
				{
					index = ourPack.ConsumeTotalGrouped(types, amounts, true, OnResourceConsumed, CheckHueGrouping);
				}

				resHue = m_ResHue;
			}			
			else if (consumeType == ConsumeType.Half) // Consume Half ( for use all resource craft type )
			{
				for (var i = 0; i < amounts.Length; i++)
				{
					amounts[i] = Math.Max(1, amounts[i] / 2);
				}

				m_ResHue = 0;
				m_ResAmount = 0;
				m_System = craftSystem;

				if (IsQuantityType(types))
				{
					index = ConsumeQuantity(ourPack, types, amounts);
				}
				else
				{
					index = ourPack.ConsumeTotalGrouped(types, amounts, true, OnResourceConsumed, CheckHueGrouping);
				}

				resHue = m_ResHue;
			}
			else // ConstumeType.None ( it's basicaly used to know if the crafter has enough resource before starting the process )
			{
				index = -1;

				if (IsQuantityType(types))
				{
					for (var i = 0; i < types.Length; i++)
					{
						if (GetQuantity(ourPack, types[i]) < amounts[i])
						{
							index = i;
							break;
						}
					}
				}
				else
				{
					for (var i = 0; i < types.Length; i++)
					{
						if (ourPack.GetBestGroupAmount(types[i], true, CheckHueGrouping) < amounts[i])
						{
							index = i;
							break;
						}
					}
				}
			}

			if (index == -1)
			{
				if (consumeType != ConsumeType.None)
				{
					consumeExtra?.Delete();
				}

				return true;
			}
			else
			{
				var res = Resources.GetAt(index);

				if (res.MessageNumber > 0)
				{
					message = res.MessageNumber;
				}
				else if (res.MessageString != null && res.MessageString != String.Empty)
				{
					message = res.MessageString;
				}
				else
				{
					message = 502925; // You don't have the resources required to make that item.
				}

				return false;
			}
		}

		private void OnResourceConsumed(Item item, int amount)
		{
			if (amount >= m_ResAmount && RetainsColorFrom(m_System, item.GetType()))
			{
				m_ResHue = item.Hue;
				m_ResAmount = amount;
			}
		}

		private int CheckHueGrouping(Item a, Item b)
		{
			return b.Hue.CompareTo(a.Hue);
		}

		public double GetExceptionalChance(CraftSystem system, double chance, Mobile from)
		{
			if (ForceNonExceptional)
			{
				return 0.0;
			}

			var bonus = 0.0;

			if (from.Talisman is BaseTalisman talisman)
			{
				if (talisman.Skill == system.MainSkill)
				{
					chance -= talisman.SuccessBonus / 100.0;
					bonus = talisman.ExceptionalBonus / 100.0;
				}
			}

			switch (system.ECA)
			{
				default:
				case CraftECA.ChanceMinusSixty:
					{
						chance -= 0.6;

						break;
					}
				case CraftECA.FiftyPercentChanceMinusTenPercent:
					{
						chance = (chance * 0.5) - 0.1; 
						
						break;
					}
				case CraftECA.ChanceMinusSixtyToFourtyFive:
					{
						var offset = 0.60 - ((from.Skills[system.MainSkill].Value - 95.0) * 0.03);

						if (offset < 0.45)
						{
							offset = 0.45;
						}
						else if (offset > 0.60)
						{
							offset = 0.60;
						}

						chance -= offset;

						break;
					}
			}

			if (chance > 0)
			{
				return chance + bonus;
			}

			return chance;
		}

		public bool CheckSkills(Mobile from, Type typeRes, CraftSystem craftSystem, ref int quality, ref bool allRequiredSkills)
		{
			return CheckSkills(from, typeRes, craftSystem, ref quality, ref allRequiredSkills, true);
		}

		public bool CheckSkills(Mobile from, Type typeRes, CraftSystem craftSystem, ref int quality, ref bool allRequiredSkills, bool gainSkills)
		{
			var chance = GetSuccessChance(from, typeRes, craftSystem, gainSkills, ref allRequiredSkills);

			if (GetExceptionalChance(craftSystem, chance, from) > Utility.RandomDouble())
			{
				quality = 2;
			}

			return chance > Utility.RandomDouble();
		}

		public double GetSuccessChance(Mobile from, Type typeRes, CraftSystem craftSystem, bool gainSkills, ref bool allRequiredSkills)
		{
			var minMainSkill = 0.0;
			var maxMainSkill = 0.0;
			var valMainSkill = 0.0;

			allRequiredSkills = true;

			for (var i = 0; i < Skills.Count; i++)
			{
				var craftSkill = Skills.GetAt(i);

				var minSkill = craftSkill.MinSkill;
				var maxSkill = craftSkill.MaxSkill;
				var valSkill = from.Skills[craftSkill.SkillToMake].Value;

				if (valSkill < minSkill)
				{
					allRequiredSkills = false;
				}

				if (craftSkill.SkillToMake == craftSystem.MainSkill)
				{
					minMainSkill = minSkill;
					maxMainSkill = maxSkill;
					valMainSkill = valSkill;
				}

				if (gainSkills) // This is a passive check. Success chance is entirely dependant on the main skill
				{
					_ = from.CheckSkill(craftSkill.SkillToMake, minSkill, maxSkill);
				}
			}

			double chance;

			if (allRequiredSkills)
			{
				chance = craftSystem.GetChanceAtMin(this) + ((valMainSkill - minMainSkill) / (maxMainSkill - minMainSkill) * (1.0 - craftSystem.GetChanceAtMin(this)));
			}
			else
			{
				chance = 0.0;
			}

			if (allRequiredSkills && from.Talisman is BaseTalisman talisman)
			{
				if (talisman.Skill == craftSystem.MainSkill)
				{
					chance += talisman.SuccessBonus / 100.0;
				}
			}

			if (allRequiredSkills && valMainSkill == maxMainSkill)
			{
				chance = 1.0;
			}

			return chance;
		}

		public void Craft(Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool)
		{
			if (!from.BeginAction(typeof(CraftSystem)))
			{
				from.SendLocalizedMessage(500119); // You must wait to perform another action
				return;
			}

			if (RequiredExpansion == Expansion.None || (from.NetState != null && from.NetState.SupportsExpansion(RequiredExpansion)))
			{
				var allRequiredSkills = true;
				var chance = GetSuccessChance(from, typeRes, craftSystem, false, ref allRequiredSkills);

				if (allRequiredSkills && chance >= 0.0)
				{
					if (Recipe == null || from is not PlayerMobile pm || pm.HasRecipe(Recipe))
					{
						var badCraft = craftSystem.CanCraft(from, tool, ItemType);

						if (badCraft <= 0)
						{
							var resHue = 0;
							var maxAmount = 0;

							object message = null;

							if (ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, ConsumeType.None, ref message))
							{
								message = null;

								if (ConsumeAttributes(from, ref message, false))
								{
									var context = craftSystem.GetContext(from);

									context?.OnMade(this);

									var iMin = craftSystem.MinCraftEffect;
									var iMax = craftSystem.MaxCraftEffect - iMin + 1;
									var iRandom = iMin + Utility.Random(iMax) + 1;

									var t = new InternalTimer(from, craftSystem, this, typeRes, tool, iRandom);

									t.Start();
								}
								else
								{
									from.EndAction(typeof(CraftSystem));

									_ = from.SendGump(new CraftGump(from, craftSystem, tool, message));
								}
							}
							else
							{
								from.EndAction(typeof(CraftSystem));

								_ = from.SendGump(new CraftGump(from, craftSystem, tool, message));
							}
						}
						else
						{
							from.EndAction(typeof(CraftSystem));

							_ = from.SendGump(new CraftGump(from, craftSystem, tool, badCraft));
						}
					}
					else
					{
						from.EndAction(typeof(CraftSystem));

						_ = from.SendGump(new CraftGump(from, craftSystem, tool, 1072847)); // You must learn that recipe from a scroll.
					}
				}
				else
				{
					from.EndAction(typeof(CraftSystem));

					_ = from.SendGump(new CraftGump(from, craftSystem, tool, 1044153)); // You don't have the required skills to attempt this item.
				}
			}
			else
			{
				from.EndAction(typeof(CraftSystem));

				_ = from.SendGump(new CraftGump(from, craftSystem, tool, RequiredExpansionMessage(RequiredExpansion))); //The {0} expansion is required to attempt this item.
			}
		}
		
		public void CompleteCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CustomCraft customCraft)
		{
			var badCraft = craftSystem.CanCraft(from, tool, ItemType);

			if (badCraft > 0)
			{
				if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
				{
					_ = from.SendGump(new CraftGump(from, craftSystem, tool, badCraft));
				}
				else
				{
					from.SendLocalizedMessage(badCraft);
				}

				return;
			}

			int checkResHue = 0, checkMaxAmount = 0;

			object checkMessage = null;

			// Not enough resource to craft it
			if (!ConsumeRes(from, typeRes, craftSystem, ref checkResHue, ref checkMaxAmount, ConsumeType.None, ref checkMessage))
			{
				if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
				{
					_ = from.SendGump(new CraftGump(from, craftSystem, tool, checkMessage));
				}
				else if (checkMessage is int num && num > 0)
				{
					from.SendLocalizedMessage(num);
				}
				else if (checkMessage is string str)
				{
					from.SendMessage(str);
				}

				return;
			}
			
			if (!ConsumeAttributes(from, ref checkMessage, false))
			{
				if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
				{
					_ = from.SendGump(new CraftGump(from, craftSystem, tool, checkMessage));
				}
				else if (checkMessage is int num && num > 0)
				{
					from.SendLocalizedMessage(num);
				}
				else if (checkMessage is string str)
				{
					from.SendMessage(str);
				}

				return;
			}

			var toolBroken = false;

			var ignored = 1;
			var endquality = 1;

			var allRequiredSkills = true;

			if (CheckSkills(from, typeRes, craftSystem, ref ignored, ref allRequiredSkills))
			{
				// Resource
				var resHue = 0;
				var maxAmount = 0;

				object message = null;

				// Not enough resource to craft it
				if (!ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, ConsumeType.All, ref message))
				{
					if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
					{
						_ = from.SendGump(new CraftGump(from, craftSystem, tool, message));
					}
					else if (message is int num && num > 0)
					{
						from.SendLocalizedMessage(num);
					}
					else if (message is string str)
					{
						from.SendMessage(str);
					}

					return;
				}
				
				if (!ConsumeAttributes(from, ref message, true))
				{
					if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
					{
						_ = from.SendGump(new CraftGump(from, craftSystem, tool, message));
					}
					else if (message is int num && num > 0)
					{
						from.SendLocalizedMessage(num);
					}
					else if (message is string str)
					{
						from.SendMessage(str);
					}

					return;
				}

				if (craftSystem is DefBlacksmithy)
				{
					if (from.FindItemOnLayer(Layer.OneHanded) is AncientSmithyHammer hammer && hammer != tool)
					{
						if (--hammer.UsesRemaining < 1)
						{
							hammer.Delete();
						}
					}
				}

				if (--tool.UsesRemaining < 1 && tool.BreakOnDepletion)
				{
					toolBroken = true;
				}

				if (toolBroken)
				{
					tool.Delete();
				}

				var result = 0;

				Item item;

				if (customCraft != null)
				{
					item = customCraft.CompleteCraft(out result);
				}
				else if (from.Map != Map.Trammel && from.Map != Map.Felucca && typeof(MapItem).IsAssignableFrom(ItemType))
				{
					item = new IndecipherableMap();

					from.SendLocalizedMessage(1070800); // The map you create becomes mysteriously indecipherable.
				}
				else
				{
					item = Utility.CreateInstance<Item>(ItemType);
				}

				if (item != null)
				{
					if (item is ICraftable craftable)
					{
						endquality = craftable.OnCraft(quality, makersMark, from, craftSystem, typeRes, tool, this, resHue);
					}
					else if (item.Hue == 0)
					{
						item.Hue = resHue;
					}

					if (maxAmount > 0)
					{
						if (!item.Stackable && item is IUsesRemaining u)
						{
							u.UsesRemaining *= maxAmount;
						}
						else
						{
							item.Amount = maxAmount;
						}
					}

					var itemAmount = item.Amount;

					_ = from.AddToBackpack(item);

					if (from.AccessLevel > AccessLevel.Player)
					{
						CommandLogging.WriteLine(from, $"Crafting {CommandLogging.Format(item)} with craft system {craftSystem.GetType().Name}");
					}

					EventSink.InvokeCraftedItem(new CraftedItemEventArgs(from, item, itemAmount, craftSystem, this, tool));
				}

				if (result == 0)
				{
					result = craftSystem.PlayEndingEffect(from, false, true, toolBroken, endquality, makersMark, this);
				}

				var queryFactionImbue = false;
				var availableSilver = 0;

				FactionItemDefinition def = null;
				Faction faction = null;

				if (item is IFactionItem)
				{
					def = FactionItemDefinition.Identify(item);

					if (def != null)
					{
						faction = Faction.Find(from);

						if (faction != null)
						{
							var town = Town.FromRegion(from.Region);

							if (town != null && town.Owner == faction)
							{
								var pack = from.Backpack;

								if (pack != null)
								{
									availableSilver = pack.GetAmount(typeof(Silver));

									if (availableSilver >= def.SilverCost)
									{
										queryFactionImbue = Faction.IsNearType(from, def.VendorType, 12);
									}
								}
							}
						}
					}
				}

				// TODO: Scroll imbuing

				if (queryFactionImbue)
				{
					_ = from.SendGump(new FactionImbueGump(quality, item, from, craftSystem, tool, result, availableSilver, faction, def));
				}
				else if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
				{
					_ = from.SendGump(new CraftGump(from, craftSystem, tool, result));
				}
				else if (result > 0)
				{
					from.SendLocalizedMessage(result);
				}
			}
			else if (!allRequiredSkills)
			{
				if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
				{
					_ = from.SendGump(new CraftGump(from, craftSystem, tool, 1044153));
				}
				else
				{
					from.SendLocalizedMessage(1044153); // You don't have the required skills to attempt this item.
				}
			}
			else
			{
				var consumeType = UseAllRes ? ConsumeType.Half : ConsumeType.All;
				var resHue = 0;
				var maxAmount = 0;

				object message = null;

				// Not enough resource to craft it
				if (!ConsumeRes(from, typeRes, craftSystem, ref resHue, ref maxAmount, consumeType, ref message, true))
				{
					if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
					{
						_ = from.SendGump(new CraftGump(from, craftSystem, tool, message));
					}
					else if (message is int num && num > 0)
					{
						from.SendLocalizedMessage(num);
					}
					else if (message is string str)
					{
						from.SendMessage(str);
					}

					return;
				}

				if (--tool.UsesRemaining < 1 && tool.BreakOnDepletion)
				{
					toolBroken = true;
				}

				if (toolBroken)
				{
					tool.Delete();
				}

				// SkillCheck failed.
				var result = craftSystem.PlayEndingEffect(from, true, true, toolBroken, endquality, false, this);

				if (tool != null && !tool.Deleted && tool.UsesRemaining > 0)
				{
					_ = from.SendGump(new CraftGump(from, craftSystem, tool, result));
				}
				else if (result > 0)
				{
					from.SendLocalizedMessage(result);
				}
			}
		}

		private class InternalTimer : Timer
		{
			private readonly Mobile m_From;
			private readonly int m_iCountMax;
			private readonly CraftItem m_CraftItem;
			private readonly CraftSystem m_CraftSystem;
			private readonly Type m_TypeRes;
			private readonly BaseTool m_Tool;

			private int m_iCount;

			public InternalTimer(Mobile from, CraftSystem craftSystem, CraftItem craftItem, Type typeRes, BaseTool tool, int iCountMax) 
				: base(TimeSpan.Zero, TimeSpan.FromSeconds(craftSystem.Delay), iCountMax)
			{
				m_From = from;
				m_CraftItem = craftItem;
				m_iCount = 0;
				m_iCountMax = iCountMax;
				m_CraftSystem = craftSystem;
				m_TypeRes = typeRes;
				m_Tool = tool;
			}

			protected override void OnTick()
			{
				m_iCount++;

				m_From.DisruptiveAction();

				if (m_iCount < m_iCountMax)
				{
					m_CraftSystem.PlayCraftEffect(m_From);
				}
				else
				{
					m_From.EndAction(typeof(CraftSystem));

					var badCraft = m_CraftSystem.CanCraft(m_From, m_Tool, m_CraftItem.ItemType);

					if (badCraft > 0)
					{
						if (m_Tool != null && !m_Tool.Deleted && m_Tool.UsesRemaining > 0)
						{
							_ = m_From.SendGump(new CraftGump(m_From, m_CraftSystem, m_Tool, badCraft));
						}
						else
						{
							m_From.SendLocalizedMessage(badCraft);
						}

						return;
					}

					var quality = 1;
					var allRequiredSkills = true;

					_ = m_CraftItem.CheckSkills(m_From, m_TypeRes, m_CraftSystem, ref quality, ref allRequiredSkills, false);

					var context = m_CraftSystem.GetContext(m_From);

					if (context == null)
					{
						return;
					}

					if (typeof(CustomCraft).IsAssignableFrom(m_CraftItem.ItemType))
					{
						var cc = Utility.CreateInstance<CustomCraft>(m_CraftItem.ItemType, m_From, m_CraftItem, m_CraftSystem, m_TypeRes, m_Tool, quality);

						cc?.EndCraftAction();

						return;
					}

					var makersMark = false;

					if (quality == 2 && m_From.Skills[m_CraftSystem.MainSkill].Base >= 100.0)
					{
						makersMark = m_CraftItem.IsMarkable(m_CraftItem.ItemType);
					}

					if (makersMark && context.MarkOption == CraftMarkOption.PromptForMark)
					{
						_ = m_From.SendGump(new QueryMakersMarkGump(quality, m_From, m_CraftItem, m_CraftSystem, m_TypeRes, m_Tool));
					}
					else
					{
						if (context.MarkOption == CraftMarkOption.DoNotMark)
						{
							makersMark = false;
						}

						m_CraftItem.CompleteCraft(quality, makersMark, m_From, m_CraftSystem, m_TypeRes, m_Tool, null);
					}
				}
			}
		}
	}

	public class CraftItemCol : System.Collections.CollectionBase
	{
		public CraftItemCol()
		{
		}

		public int Add(CraftItem craftItem)
		{
			return List.Add(craftItem);
		}

		public void Remove(int index)
		{
			if (index >= 0 && index < Count)
			{
				List.RemoveAt(index);
			}
		}

		public CraftItem GetAt(int index)
		{
			return (CraftItem)List[index];
		}

		public CraftItem SearchForSubclass(Type type)
		{
			for (var i = 0; i < List.Count; i++)
			{
				var craftItem = (CraftItem)List[i];

				if (craftItem.ItemType == type || type.IsSubclassOf(craftItem.ItemType))
				{
					return craftItem;
				}
			}

			return null;
		}

		public CraftItem SearchFor(Type type)
		{
			for (var i = 0; i < List.Count; i++)
			{
				var craftItem = (CraftItem)List[i];

				if (craftItem.ItemType == type)
				{
					return craftItem;
				}
			}

			return null;
		}
	}

	/// Craft Resource
	public class CraftRes
	{
		public Type ItemType { get; }

		public string MessageString { get; }

		public int MessageNumber { get; }

		public string NameString { get; }

		public int NameNumber { get; }

		public int Amount { get; }

		public CraftRes(Type type, int amount)
		{
			ItemType = type;
			Amount = amount;
		}

		public CraftRes(Type type, TextDefinition name, int amount, TextDefinition message) 
			: this(type, amount)
		{
			NameNumber = name;
			MessageNumber = message;

			NameString = name;
			MessageString = message;
		}

		public void SendMessage(Mobile from)
		{
			if (MessageNumber > 0)
			{
				from.SendLocalizedMessage(MessageNumber);
			}
			else if (!String.IsNullOrEmpty(MessageString))
			{
				from.SendMessage(MessageString);
			}
			else
			{
				from.SendLocalizedMessage(502925); // You don't have the resources required to make that item.
			}
		}
	}

	public class CraftResCol : System.Collections.CollectionBase
	{
		public CraftResCol()
		{
		}

		public void Add(CraftRes craftRes)
		{
			_ = List.Add(craftRes);
		}

		public void Remove(int index)
		{
			if (index >= 0 && index < Count)
			{
				List.RemoveAt(index);
			}
		}

		public CraftRes GetAt(int index)
		{
			return (CraftRes)List[index];
		}
	}

	public class CraftSubRes
	{
		public Type ItemType { get; }

		public string NameString { get; }

		public int NameNumber { get; }

		public int GenericNameNumber { get; }

		public object Message { get; }

		public double RequiredSkill { get; }

		public CraftSubRes(Type type, TextDefinition name, double reqSkill, object message) 
			: this(type, name, reqSkill, 0, message)
		{
		}

		public CraftSubRes(Type type, TextDefinition name, double reqSkill, int genericNameNumber, object message)
		{
			ItemType = type;
			NameNumber = name;
			NameString = name;
			RequiredSkill = reqSkill;
			GenericNameNumber = genericNameNumber;
			Message = message;
		}
	}

	public class CraftSubResCol : System.Collections.CollectionBase
	{
		public bool Init { get; set; }

		public Type ResType { get; set; }

		public string NameString { get; set; }

		public int NameNumber { get; set; }

		public CraftSubResCol()
		{
		}

		public void Add(CraftSubRes craftSubRes)
		{
			_ = List.Add(craftSubRes);
		}

		public void Remove(int index)
		{
			if (index >= 0 && index < Count)
			{
				List.RemoveAt(index);
			}
		}

		public CraftSubRes GetAt(int index)
		{
			return (CraftSubRes)List[index];
		}

		public CraftSubRes SearchFor(Type type)
		{
			for (var i = 0; i < List.Count; i++)
			{
				var craftSubRes = (CraftSubRes)List[i];

				if (craftSubRes.ItemType == type)
				{
					return craftSubRes;
				}
			}

			return null;
		}
	}

	#endregion

	#region Makers Mark Option

	public class QueryMakersMarkGump : Gump
	{
		private readonly int m_Quality;
		private readonly Mobile m_From;
		private readonly CraftItem m_CraftItem;
		private readonly CraftSystem m_CraftSystem;
		private readonly Type m_TypeRes;
		private readonly BaseTool m_Tool;

		public QueryMakersMarkGump(int quality, Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, BaseTool tool) 
			: base(100, 200)
		{
			_ = from.CloseGump(typeof(QueryMakersMarkGump));

			m_Quality = quality;
			m_From = from;
			m_CraftItem = craftItem;
			m_CraftSystem = craftSystem;
			m_TypeRes = typeRes;
			m_Tool = tool;

			AddPage(0);

			AddBackground(0, 0, 220, 170, 5054);
			AddBackground(10, 10, 200, 150, 3000);

			AddHtmlLocalized(20, 20, 180, 80, 1018317, false, false); // Do you wish to place your maker's mark on this item?

			AddHtmlLocalized(55, 100, 140, 25, 1011011, false, false); // CONTINUE
			AddButton(20, 100, 4005, 4007, 1, GumpButtonType.Reply, 0);

			AddHtmlLocalized(55, 125, 140, 25, 1011012, false, false); // CANCEL
			AddButton(20, 125, 4005, 4007, 0, GumpButtonType.Reply, 0);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var makersMark = info.ButtonID == 1;

			if (makersMark)
			{
				m_From.SendLocalizedMessage(501808); // You mark the item.
			}
			else
			{
				m_From.SendLocalizedMessage(501809); // Cancelled mark.
			}

			m_CraftItem.CompleteCraft(m_Quality, makersMark, m_From, m_CraftSystem, m_TypeRes, m_Tool, null);
		}
	}

	public enum CraftMarkOption
	{
		MarkItem,
		DoNotMark,
		PromptForMark
	}

	public class CraftContext
	{
		public List<CraftItem> Items { get; } = new();

		public int LastResourceIndex { get; set; } = -1;
		public int LastResourceIndex2 { get; set; } = -1;

		public int LastGroupIndex { get; set; } = -1;

		public bool DoNotColor { get; set; }

		public CraftMarkOption MarkOption { get; set; }

		public CraftContext()
		{
		}

		public CraftItem LastMade
		{
			get
			{
				if (Items?.Count > 0)
				{
					return Items[0];
				}

				return null;
			}
		}

		public void OnMade(CraftItem item)
		{
			_ = Items.Remove(item);

			while (Items.Count >= 10)
			{
				Items.RemoveAt(Items.Count - 1);
			}

			Items.Insert(0, item);
		}
	}

	#endregion

	#region Maintenance Option

	/// Enhance Items
	public enum EnhanceResult
	{
		None,
		NotInBackpack,
		BadItem,
		BadResource,
		AlreadyEnhanced,
		Success,
		Failure,
		Broken,
		NoResources,
		NoSkill
	}

	public static class Enhance
	{
		public static EnhanceResult Invoke(Mobile from, CraftSystem craftSystem, BaseTool tool, Item item, CraftResource resource, Type resType, ref object resMessage)
		{
			if (item == null)
			{
				return EnhanceResult.BadItem;
			}

			if (!item.IsChildOf(from.Backpack))
			{
				return EnhanceResult.NotInBackpack;
			}

			if (item is not BaseArmor and not BaseWeapon)
			{
				return EnhanceResult.BadItem;
			}

			if (item is IArcaneEquip eq && eq.IsArcane)
			{
				return EnhanceResult.BadItem;
			}

			if (CraftResources.IsStandard(resource))
			{
				return EnhanceResult.BadResource;
			}

			var num = craftSystem.CanCraft(from, tool, item.GetType());

			if (num > 0)
			{
				resMessage = num;

				return EnhanceResult.None;
			}

			var craftItem = craftSystem.CraftItems.SearchFor(item.GetType());

			if (craftItem == null || craftItem.Resources.Count == 0)
			{
				return EnhanceResult.BadItem;
			}

			var allRequiredSkills = false;

			if (craftItem.GetSuccessChance(from, resType, craftSystem, false, ref allRequiredSkills) <= 0.0)
			{
				return EnhanceResult.NoSkill;
			}

			var info = CraftResources.GetInfo(resource);

			if (info == null || info.ResourceTypes.Length == 0)
			{
				return EnhanceResult.BadResource;
			}

			var attributes = info.AttributeInfo;

			if (attributes == null)
			{
				return EnhanceResult.BadResource;
			}

			int resHue = 0, maxAmount = 0;

			if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.None, ref resMessage))
			{
				return EnhanceResult.NoResources;
			}

			if (craftSystem is DefBlacksmithy)
			{
				if (from.FindItemOnLayer(Layer.OneHanded) is AncientSmithyHammer hammer)
				{
					if (--hammer.UsesRemaining < 1)
					{
						hammer.Delete();
					}
				}
			}

			int phys = 0, fire = 0, cold = 0, pois = 0, nrgy = 0;
			int dinc = 0, dura, luck, lreq, baseChance;

			bool physBonus = false, fireBonus, coldBonus, nrgyBonus, poisBonus;
			bool dincBonus, duraBonus, luckBonus, lreqBonus;

			if (item is BaseWeapon weapon)
			{
				if (!CraftResources.IsStandard(weapon.Resource))
				{
					return EnhanceResult.AlreadyEnhanced;
				}

				baseChance = 20;

				dura = weapon.MaxHitPoints;
				luck = weapon.Attributes.Luck;
				lreq = weapon.WeaponAttributes.LowerStatReq;
				dinc = weapon.Attributes.WeaponDamage;

				fireBonus = attributes.WeaponFireDamage > 0;
				coldBonus = attributes.WeaponColdDamage > 0;
				nrgyBonus = attributes.WeaponEnergyDamage > 0;
				poisBonus = attributes.WeaponPoisonDamage > 0;

				duraBonus = attributes.WeaponDurability > 0;
				luckBonus = attributes.WeaponLuck > 0;
				lreqBonus = attributes.WeaponLowerRequirements > 0;
				dincBonus = dinc > 0;
			}
			else
			{
				var armor = (BaseArmor)item;

				if (!CraftResources.IsStandard(armor.Resource))
				{
					return EnhanceResult.AlreadyEnhanced;
				}

				baseChance = 20;

				phys = armor.PhysicalResistance;
				fire = armor.FireResistance;
				cold = armor.ColdResistance;
				pois = armor.PoisonResistance;
				nrgy = armor.EnergyResistance;

				dura = armor.MaxHitPoints;
				luck = armor.Attributes.Luck;
				lreq = armor.ArmorAttributes.LowerStatReq;

				physBonus = attributes.ArmorPhysicalResist > 0;
				fireBonus = attributes.ArmorFireResist > 0;
				coldBonus = attributes.ArmorColdResist > 0;
				nrgyBonus = attributes.ArmorEnergyResist > 0;
				poisBonus = attributes.ArmorPoisonResist > 0;

				duraBonus = attributes.ArmorDurability > 0;
				luckBonus = attributes.ArmorLuck > 0;
				lreqBonus = attributes.ArmorLowerRequirements > 0;
				dincBonus = false;
			}

			var skill = from.Skills[craftSystem.MainSkill].Fixed / 10;

			if (skill >= 100)
			{
				baseChance -= (skill - 90) / 10;
			}

			var res = EnhanceResult.Success;

			if (physBonus)
			{
				CheckResult(ref res, baseChance + phys);
			}

			if (fireBonus)
			{
				CheckResult(ref res, baseChance + fire);
			}

			if (coldBonus)
			{
				CheckResult(ref res, baseChance + cold);
			}

			if (nrgyBonus)
			{
				CheckResult(ref res, baseChance + nrgy);
			}

			if (poisBonus)
			{
				CheckResult(ref res, baseChance + pois);
			}

			if (duraBonus)
			{
				CheckResult(ref res, baseChance + (dura / 40));
			}

			if (luckBonus)
			{
				CheckResult(ref res, baseChance + 10 + (luck / 2));
			}

			if (lreqBonus)
			{
				CheckResult(ref res, baseChance + (lreq / 4));
			}

			if (dincBonus)
			{
				CheckResult(ref res, baseChance + (dinc / 4));
			}

			switch (res)
			{
				case EnhanceResult.Broken:
					{
						if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.Half, ref resMessage))
						{
							return EnhanceResult.NoResources;
						}

						item.Delete();

						break;
					}
				case EnhanceResult.Success:
					{
						if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.All, ref resMessage))
						{
							return EnhanceResult.NoResources;
						}

						if (item is BaseWeapon bw)
						{
							bw.Resource = resource;

							var hue = bw.GetElementalDamageHue();

							if (hue > 0)
							{
								bw.Hue = hue;
							}
						}
						else if (item is BaseArmor ba)
						{
							ba.Resource = resource;
						}

						break;
					}
				case EnhanceResult.Failure:
					{
						if (!craftItem.ConsumeRes(from, resType, craftSystem, ref resHue, ref maxAmount, ConsumeType.Half, ref resMessage))
						{
							return EnhanceResult.NoResources;
						}

						break;
					}
			}

			return res;
		}

		public static void CheckResult(ref EnhanceResult res, int chance)
		{
			if (res != EnhanceResult.Success)
			{
				return; // we've already failed..
			}

			var random = Utility.Random(100);

			if (10 > random)
			{
				res = EnhanceResult.Failure;
			}
			else if (chance > random)
			{
				res = EnhanceResult.Broken;
			}
		}

		public static void BeginTarget(Mobile from, CraftSystem craftSystem, BaseTool tool)
		{
			var context = craftSystem.GetContext(from);

			if (context == null)
			{
				return;
			}

			var lastRes = context.LastResourceIndex;
			var subRes = craftSystem.CraftSubRes;

			if (lastRes >= 0 && lastRes < subRes.Count)
			{
				var res = subRes.GetAt(lastRes);

				if (from.Skills[craftSystem.MainSkill].Value < res.RequiredSkill)
				{
					_ = from.SendGump(new CraftGump(from, craftSystem, tool, res.Message));
				}
				else
				{
					var resource = CraftResources.GetFromType(res.ItemType);

					if (resource != CraftResource.None)
					{
						from.SendLocalizedMessage(1061004); // Target an item to enhance with the properties of your selected material.
						from.Target = new InternalTarget(craftSystem, tool, res.ItemType, resource);
					}
					else
					{
						_ = from.SendGump(new CraftGump(from, craftSystem, tool, 1061010)); // You must select a special material in order to enhance an item with its properties.
					}
				}
			}
			else
			{
				_ = from.SendGump(new CraftGump(from, craftSystem, tool, 1061010)); // You must select a special material in order to enhance an item with its properties.
			}
		}

		private class InternalTarget : Target
		{
			private readonly CraftSystem m_CraftSystem;
			private readonly BaseTool m_Tool;
			private readonly Type m_ResourceType;
			private readonly CraftResource m_Resource;

			public InternalTarget(CraftSystem craftSystem, BaseTool tool, Type resourceType, CraftResource resource) 
				: base(2, false, TargetFlags.None)
			{
				m_CraftSystem = craftSystem;
				m_Tool = tool;
				m_ResourceType = resourceType;
				m_Resource = resource;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Item item)
				{
					object message = null;

					var res = Enhance.Invoke(from, m_CraftSystem, m_Tool, item, m_Resource, m_ResourceType, ref message);

					switch (res)
					{
						case EnhanceResult.NotInBackpack: message = 1061005; break;     // The item must be in your backpack to enhance it.
						case EnhanceResult.AlreadyEnhanced: message = 1061012; break;   // This item is already enhanced with the properties of a special material.
						case EnhanceResult.BadItem: message = 1061011; break;           // You cannot enhance this type of item with the properties of the selected special material.
						case EnhanceResult.BadResource: message = 1061010; break;       // You must select a special material in order to enhance an item with its properties.
						case EnhanceResult.Broken: message = 1061080; break;            // You attempt to enhance the item, but fail catastrophically. The item is lost.
						case EnhanceResult.Failure: message = 1061082; break;           // You attempt to enhance the item, but fail. Some material is lost in the process.
						case EnhanceResult.Success: message = 1061008; break;           // You enhance the item with the properties of the special material.
						case EnhanceResult.NoSkill: message = 1044153; break;           // You don't have the required skills to attempt this item.
					}

					_ = from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, message));
				}
			}
		}
	}

	/// Repair Items
	public static class Repair
	{
		public static void Do(Mobile from, CraftSystem craftSystem, BaseTool tool)
		{
			from.Target = new InternalTarget(craftSystem, tool);
			from.SendLocalizedMessage(1044276); // Target an item to repair.
		}

		public static void Do(Mobile from, CraftSystem craftSystem, RepairDeed deed)
		{
			from.Target = new InternalTarget(craftSystem, deed);
			from.SendLocalizedMessage(1044276); // Target an item to repair.
		}

		private class InternalTarget : Target
		{
			private readonly CraftSystem m_CraftSystem;
			private readonly BaseTool m_Tool;
			private readonly RepairDeed m_Deed;

			public InternalTarget(CraftSystem craftSystem, BaseTool tool) 
				: base(2, false, TargetFlags.None)
			{
				m_CraftSystem = craftSystem;
				m_Tool = tool;
			}

			public InternalTarget(CraftSystem craftSystem, RepairDeed deed) 
				: base(2, false, TargetFlags.None)
			{
				m_CraftSystem = craftSystem;
				m_Deed = deed;
			}

			private int GetWeakenChance(Mobile mob, SkillName skill, int curHits, int maxHits)
			{
				// 40% - (1% per hp lost) - (1% per 10 craft skill)
				return 40 + (maxHits - curHits) - (int)((m_Deed?.SkillLevel ?? mob.Skills[skill].Value) / 10);
			}

			private bool CheckWeaken(Mobile mob, SkillName skill, int curHits, int maxHits)
			{
				return GetWeakenChance(mob, skill, curHits, maxHits) > Utility.Random(100);
			}

			private bool CheckRepairDifficulty(Mobile mob, SkillName skill, int curHits, int maxHits)
			{
				var difficulty = (((maxHits - curHits) * 1250 / Math.Max(maxHits, 1)) - 250) * 0.1;

				if (m_Deed == null)
				{
					return mob.CheckSkill(skill, difficulty - 25.0, difficulty + 25.0);
				}

				var value = m_Deed.SkillLevel;
				var minSkill = difficulty - 25.0;
				var maxSkill = difficulty + 25;

				if (value < minSkill)
				{
					return false; // Too difficult
				}

				if (value >= maxSkill)
				{
					return true; // No challenge
				}

				var chance = (value - minSkill) / (maxSkill - minSkill);

				return chance >= Utility.RandomDouble();
			}

			private bool CheckDeed(Mobile from)
			{
				return m_Deed?.Check(from) != false;
			}

			private bool IsSpecialClothing(BaseClothing clothing)
			{
				// Clothing repairable but not craftable

				if (m_CraftSystem is DefTailoring)
				{
					return clothing is BearMask or DeerMask or TheMostKnowledgePerson or TheRobeOfBritanniaAri or EmbroideredOakLeafCloak;
				}

				return false;
			}

			private bool IsSpecialWeapon(BaseWeapon weapon)
			{
				// Weapons repairable but not craftable

				if (m_CraftSystem is DefTinkering)
				{
					return weapon is Cleaver or Hatchet or Pickaxe or ButcherKnife or SkinningKnife;
				}
				
				if (m_CraftSystem is DefCarpentry)
				{
					return weapon is Club or BlackStaff or MagicWand or WildStaff;
				}
				
				if (m_CraftSystem is DefBlacksmithy)
				{
					return weapon is RadiantScimitar or WarCleaver or ElvenSpellblade or AssassinSpike or Leafblade or RuneBlade or ElvenMachete or OrnateAxe or DiamondMace;
				}
				
				if (m_CraftSystem is DefBowFletching)
				{
					return weapon is ElvenCompositeLongbow or MagicalShortbow;
				}

				return false;
			}

			private bool IsSpecialArmor(BaseArmor armor)
			{
				// Armor repairable but not craftable
				if (m_CraftSystem is DefTailoring)
				{
					return armor is LeafTonlet or LeafArms or LeafChest or LeafGloves or LeafGorget or LeafLegs or HideChest or HideGloves or HideGorget or HidePants or HidePauldrons;
				}
				
				if (m_CraftSystem is DefCarpentry)
				{
					return armor is WingedHelm or RavenHelm or VultureHelm or WoodlandArms or WoodlandChest or WoodlandGloves or WoodlandGorget or WoodlandLegs;
				}
				
				if (m_CraftSystem is DefBlacksmithy)
				{
					return armor is Circlet or RoyalCirclet or GemmedCirclet;
				}

				return false;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				int number;

				if (!CheckDeed(from))
				{
					return;
				}

				var usingDeed = m_Deed != null;
				var toDelete = false;

				// TODO: Make an IRepairable

				if (m_CraftSystem.CanCraft(from, m_Tool, targeted.GetType()) == 1044267)
				{
					number = 1044282; // You must be near a forge and and anvil to repair items. * Yes, there are two and's *
				}
				else if (m_CraftSystem is DefTinkering && targeted is Golem g)
				{
					var damage = g.HitsMax - g.Hits;

					if (g.IsDeadBondedPet)
					{
						number = 500426; // You can't repair that.
					}
					else if (damage <= 0)
					{
						number = 500423; // That is already in full repair.
					}
					else
					{
						var skillValue = usingDeed ? m_Deed.SkillLevel : from.Skills.Tinkering.Value;

						if (skillValue < 60.0)
						{
							number = 1044153; // You don't have the required skills to attempt this item.
						}
						else if (!from.CanBeginAction(typeof(Golem)))
						{
							number = 501789; // You must wait before trying again.
						}
						else
						{
							damage = (int)Math.Min(damage, skillValue * 0.3);

							damage += 30;

							if (!from.CheckSkill(SkillName.Tinkering, 0.0, 100.0))
							{
								damage /= 2;
							}

							var pack = from.Backpack;

							if (pack != null)
							{
								var v = pack.ConsumeUpTo(typeof(IronIngot), (damage + 4) / 5);

								if (v > 0)
								{
									g.Hits += v * 5;

									number = 1044279; // You repair the item.
									toDelete = true;

									_ = from.BeginAction(typeof(Golem));

									_ = Timer.DelayCall(TimeSpan.FromSeconds(12.0), from.EndAction, typeof(Golem));
								}
								else
								{
									number = 1044037; // You do not have sufficient metal to make that.
								}
							}
							else
							{
								number = 1044037; // You do not have sufficient metal to make that.
							}
						}
					}
				}
				else if (targeted is BaseWeapon weapon)
				{
					var skill = m_CraftSystem.MainSkill;
					var toWeaken = 0;

					if (Core.AOS)
					{
						toWeaken = 1;
					}
					else if (skill != SkillName.Tailoring)
					{
						var skillLevel = usingDeed ? m_Deed.SkillLevel : from.Skills[skill].Base;

						if (skillLevel >= 90.0)
						{
							toWeaken = 1;
						}
						else if (skillLevel >= 70.0)
						{
							toWeaken = 2;
						}
						else
						{
							toWeaken = 3;
						}
					}

					if (m_CraftSystem.CraftItems.SearchForSubclass(weapon.GetType()) == null && !IsSpecialWeapon(weapon))
					{
						number = usingDeed ? 1061136 : 1044277; // That item cannot be repaired. : You cannot repair that item with this type of repair contract.
					}
					else if (!weapon.IsChildOf(from.Backpack) && (!Core.ML || weapon.Parent != from))
					{
						number = 1044275; // The item must be in your backpack to repair it.
					}
					else if (!Core.AOS && weapon.PoisonCharges != 0)
					{
						number = 1005012; // You cannot repair an item while a caustic substance is on it.
					}
					else if (weapon.MaxHitPoints <= 0 || weapon.HitPoints == weapon.MaxHitPoints)
					{
						number = 1044281; // That item is in full repair
					}
					else if (weapon.MaxHitPoints <= toWeaken)
					{
						number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
					}
					else
					{
						if (CheckWeaken(from, skill, weapon.HitPoints, weapon.MaxHitPoints))
						{
							weapon.MaxHitPoints -= toWeaken;
							weapon.HitPoints = Math.Max(0, weapon.HitPoints - toWeaken);
						}

						if (CheckRepairDifficulty(from, skill, weapon.HitPoints, weapon.MaxHitPoints))
						{
							number = 1044279; // You repair the item.

							m_CraftSystem.PlayCraftEffect(from);

							weapon.HitPoints = weapon.MaxHitPoints;
						}
						else
						{
							number = usingDeed ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]

							m_CraftSystem.PlayCraftEffect(from);
						}

						toDelete = true;
					}
				}
				else if (targeted is BaseArmor armor)
				{
					var skill = m_CraftSystem.MainSkill;
					var toWeaken = 0;

					if (Core.AOS)
					{
						toWeaken = 1;
					}
					else if (skill != SkillName.Tailoring)
					{
						var skillLevel = usingDeed ? m_Deed.SkillLevel : from.Skills[skill].Base;

						if (skillLevel >= 90.0)
						{
							toWeaken = 1;
						}
						else if (skillLevel >= 70.0)
						{
							toWeaken = 2;
						}
						else
						{
							toWeaken = 3;
						}
					}

					if (m_CraftSystem.CraftItems.SearchForSubclass(armor.GetType()) == null && !IsSpecialArmor(armor))
					{
						number = usingDeed ? 1061136 : 1044277; // That item cannot be repaired. : You cannot repair that item with this type of repair contract.
					}
					else if (!armor.IsChildOf(from.Backpack) && (!Core.ML || armor.Parent != from))
					{
						number = 1044275; // The item must be in your backpack to repair it.
					}
					else if (armor.MaxHitPoints <= 0 || armor.HitPoints == armor.MaxHitPoints)
					{
						number = 1044281; // That item is in full repair
					}
					else if (armor.MaxHitPoints <= toWeaken)
					{
						number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
					}
					else
					{
						if (CheckWeaken(from, skill, armor.HitPoints, armor.MaxHitPoints))
						{
							armor.MaxHitPoints -= toWeaken;
							armor.HitPoints = Math.Max(0, armor.HitPoints - toWeaken);
						}

						if (CheckRepairDifficulty(from, skill, armor.HitPoints, armor.MaxHitPoints))
						{
							number = 1044279; // You repair the item.

							m_CraftSystem.PlayCraftEffect(from);

							armor.HitPoints = armor.MaxHitPoints;
						}
						else
						{
							number = usingDeed ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]

							m_CraftSystem.PlayCraftEffect(from);
						}

						toDelete = true;
					}
				}
				else if (targeted is BaseClothing clothing)
				{
					var skill = m_CraftSystem.MainSkill;
					var toWeaken = 0;

					if (Core.AOS)
					{
						toWeaken = 1;
					}
					else if (skill != SkillName.Tailoring)
					{
						var skillLevel = usingDeed ? m_Deed.SkillLevel : from.Skills[skill].Base;

						if (skillLevel >= 90.0)
						{
							toWeaken = 1;
						}
						else if (skillLevel >= 70.0)
						{
							toWeaken = 2;
						}
						else
						{
							toWeaken = 3;
						}
					}

					if (m_CraftSystem.CraftItems.SearchForSubclass(clothing.GetType()) == null && !IsSpecialClothing(clothing) && targeted is not TribalMask or HornedTribalMask)
					{
						number = usingDeed ? 1061136 : 1044277; // That item cannot be repaired. : You cannot repair that item with this type of repair contract.
					}
					else if (!clothing.IsChildOf(from.Backpack) && (!Core.ML || clothing.Parent != from))
					{
						number = 1044275; // The item must be in your backpack to repair it.
					}
					else if (clothing.MaxHitPoints <= 0 || clothing.HitPoints == clothing.MaxHitPoints)
					{
						number = 1044281; // That item is in full repair
					}
					else if (clothing.MaxHitPoints <= toWeaken)
					{
						number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
					}
					else
					{
						if (CheckWeaken(from, skill, clothing.HitPoints, clothing.MaxHitPoints))
						{
							clothing.MaxHitPoints -= toWeaken;
							clothing.HitPoints = Math.Max(0, clothing.HitPoints - toWeaken);
						}

						if (CheckRepairDifficulty(from, skill, clothing.HitPoints, clothing.MaxHitPoints))
						{
							number = 1044279; // You repair the item.

							m_CraftSystem.PlayCraftEffect(from);

							clothing.HitPoints = clothing.MaxHitPoints;
						}
						else
						{
							number = usingDeed ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]

							m_CraftSystem.PlayCraftEffect(from);
						}

						toDelete = true;
					}
				}
				else if (!usingDeed && targeted is BlankScroll bs)
				{
					var skill = m_CraftSystem.MainSkill;

					if (from.Skills[skill].Value >= 50.0)
					{
						bs.Consume(1);

						_ = from.AddToBackpack(new RepairDeed(RepairDeed.GetTypeFor(m_CraftSystem), from.Skills[skill].Value, from));

						number = 500442; // You create the item and put it in your backpack.
					}
					else
					{
						number = 1047005; // You must be at least apprentice level to create a repair service contract.
					}
				}
				else if (targeted is Item)
				{
					number = usingDeed ? 1061136 : 1044277; // That item cannot be repaired. : You cannot repair that item with this type of repair contract.
				}
				else
				{
					number = 500426; // You can't repair that.
				}

				if (!usingDeed)
				{
					_ = from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, number));
				}
				else
				{
					from.SendLocalizedMessage(number);

					if (toDelete)
					{
						m_Deed.Delete();
					}
				}
			}
		}
	}

	/// Resmelt Items
	public enum SmeltResult
	{
		Success,
		Invalid,
		NoSkill
	}

	public class Resmelt
	{
		public Resmelt()
		{
		}

		public static void Do(Mobile from, CraftSystem craftSystem, BaseTool tool)
		{
			var num = craftSystem.CanCraft(from, tool, null);

			if (num > 0 && num != 1044267)
			{
				_ = from.SendGump(new CraftGump(from, craftSystem, tool, num));
			}
			else
			{
				from.SendLocalizedMessage(1044273); // Target an item to recycle.
				from.Target = new InternalTarget(craftSystem, tool);
			}
		}

		private class InternalTarget : Target
		{
			private readonly CraftSystem m_CraftSystem;
			private readonly BaseTool m_Tool;

			public InternalTarget(CraftSystem craftSystem, BaseTool tool) 
				: base(2, false, TargetFlags.None)
			{
				m_CraftSystem = craftSystem;
				m_Tool = tool;
			}

			private SmeltResult Resmelt(Mobile from, Item item, CraftResource resource)
			{
				try
				{
					if (Ethics.Ethic.IsImbued(item))
					{
						return SmeltResult.Invalid;
					}

					if (CraftResources.GetType(resource) != CraftResourceType.Metal)
					{
						return SmeltResult.Invalid;
					}

					var info = CraftResources.GetInfo(resource);

					if (info == null || info.ResourceTypes.Length == 0)
					{
						return SmeltResult.Invalid;
					}

					var craftItem = m_CraftSystem.CraftItems.SearchFor(item.GetType());

					if (craftItem == null || craftItem.Resources.Count == 0)
					{
						return SmeltResult.Invalid;
					}

					var craftResource = craftItem.Resources.GetAt(0);

					if (craftResource.Amount < 2)
					{
						return SmeltResult.Invalid; // Not enough metal to resmelt
					}

					var difficulty = 0.0;

					switch (resource)
					{
						case CraftResource.DullCopper: difficulty = 65.0; break;
						case CraftResource.ShadowIron: difficulty = 70.0; break;
						case CraftResource.Copper: difficulty = 75.0; break;
						case CraftResource.Bronze: difficulty = 80.0; break;
						case CraftResource.Gold: difficulty = 85.0; break;
						case CraftResource.Agapite: difficulty = 90.0; break;
						case CraftResource.Verite: difficulty = 95.0; break;
						case CraftResource.Valorite: difficulty = 99.0; break;
					}

					if (difficulty > from.Skills[SkillName.Mining].Value)
					{
						return SmeltResult.NoSkill;
					}

					var resourceType = info.ResourceTypes[0];

					var ingot = Utility.CreateInstance<Item>(resourceType);

					if (item is DragonBardingDeed || (item is BaseArmor ba && ba.PlayerConstructed) || (item is BaseWeapon bw && bw.PlayerConstructed) || (item is BaseClothing bc && bc.PlayerConstructed))
					{
						ingot.Amount = craftResource.Amount / 2;
					}
					else
					{
						ingot.Amount = 1;
					}

					item.Delete();

					_ = from.AddToBackpack(ingot);

					from.PlaySound(0x2A);
					from.PlaySound(0x240);

					return SmeltResult.Success;
				}
				catch
				{
				}

				return SmeltResult.Invalid;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				var num = m_CraftSystem.CanCraft(from, m_Tool, null);

				if (num > 0)
				{
					if (num == 1044267)
					{
						bool anvil, forge;

						DefBlacksmithy.CheckAnvilAndForge(from, 2, out anvil, out forge);

						if (!anvil)
						{
							num = 1044266; // You must be near an anvil
						}
						else if (!forge)
						{
							num = 1044265; // You must be near a forge.
						}
					}

					_ = from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, num));
				}
				else
				{
					var result = SmeltResult.Invalid;
					var isStoreBought = false;

					if (targeted is BaseArmor ba)
					{
						result = Resmelt(from, ba, ba.Resource);
						isStoreBought = !ba.PlayerConstructed;
					}
					else if (targeted is BaseWeapon bw)
					{
						result = Resmelt(from, bw, bw.Resource);
						isStoreBought = !bw.PlayerConstructed;
					}
					else if (targeted is DragonBardingDeed bd)
					{
						result = Resmelt(from, bd, bd.Resource);
						isStoreBought = false;
					}

					var message = result switch
					{
						SmeltResult.NoSkill => 1044269,
						SmeltResult.Success => isStoreBought ? 500418 : 1044270,
						_ => 1044272,
					};

					_ = from.SendGump(new CraftGump(from, m_CraftSystem, m_Tool, message));
				}
			}
		}
	}

	#endregion
}