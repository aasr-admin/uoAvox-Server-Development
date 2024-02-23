using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Network;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	public class SalvageBag : Bag
	{
		public override int LabelNumber => 1079931;  // Salvage Bag

		[Constructable]
		public SalvageBag()
			: this(Utility.RandomBlueHue())
		{
		}

		[Constructable]
		public SalvageBag(int hue)
		{
			Weight = 2.0;
			Hue = hue;
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from.Alive)
			{
				list.Add(new SalvageIngotsEntry(this, IsChildOf(from.Backpack) && Resmeltables()));
				list.Add(new SalvageClothEntry(this, IsChildOf(from.Backpack) && Scissorables()));
				list.Add(new SalvageAllEntry(this, IsChildOf(from.Backpack) && Resmeltables() && Scissorables()));
			}
		}

		#region Checks

		private bool Resmeltables() //Where context menu checks for metal items and dragon barding deeds
		{
			foreach (var i in Items)
			{
				if (i is BaseWeapon bw)
				{
					if (CraftResources.GetType(bw.Resource) == CraftResourceType.Metal)
					{
						return true;
					}
				}

				if (i is BaseArmor ba)
				{
					if (CraftResources.GetType(ba.Resource) == CraftResourceType.Metal)
					{
						return true;
					}
				}

				if (i is DragonBardingDeed)
				{
					return true;
				}
			}

			return false;
		}

		private bool Scissorables() //Where context menu checks for Leather items and cloth items
		{
			foreach (var i in Items)
			{
				if (i is IScissorable)
				{
					if (i is BaseClothing)
					{
						return true;
					}

					if (i is BaseArmor ba)
					{
						if (CraftResources.GetType(ba.Resource) == CraftResourceType.Leather)
						{
							return true;
						}
					}

					if (i is Cloth or BoltOfCloth or Hides or BonePile)
					{
						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region Resmelt

		private bool Resmelt(Mobile from, Item item, CraftResource resource)
		{
			try
			{
				if (CraftResources.GetType(resource) != CraftResourceType.Metal)
				{
					return false;
				}

				var info = CraftResources.GetInfo(resource);

				if (info == null || info.ResourceTypes.Length == 0)
				{
					return false;
				}

				var craftItem = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(item.GetType());

				if (craftItem == null || craftItem.Resources.Count == 0)
				{
					return false;
				}

				var craftResource = craftItem.Resources.GetAt(0);

				if (craftResource.Amount < 2)
				{
					return false; // Not enough metal to resmelt
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

				if (difficulty > from.Skills.Mining.Value)
				{
					return false;
				}

				var resourceType = info.ResourceTypes[0];

				var ingot = Utility.CreateInstance<Item>(resourceType);

				if (item is DragonBardingDeed || (item is BaseArmor ba && ba.PlayerConstructed) || (item is BaseWeapon bw && bw.PlayerConstructed) || (item is BaseClothing bc && bc.PlayerConstructed))
				{
					var mining = Math.Min(100.0, from.Skills.Mining.Value);

					var amount = ((4 + mining) * craftResource.Amount - 4) * 0.0068;

					if (amount >= 2)
					{
						ingot.Amount = (int)amount;
					}
					else
					{
						ingot.Amount = 2;
					}
				}
				else
				{
					ingot.Amount = 2;
				}
				
				item.Delete();

				from.AddToBackpack(ingot);

				from.PlaySound(0x2A);
				from.PlaySound(0x240);

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			return false;
		}

		#endregion

		#region Salvaging

		private void SalvageIngots(Mobile from)
		{
			var hasTool = from.Items.Exists(item => item is BaseTool tool && tool.CraftSystem == DefBlacksmithy.CraftSystem);

			if (!hasTool)
			{
				foreach (var tool in from.Backpack.FindItemsByType<BaseTool>())
				{
					if (tool.CraftSystem == DefBlacksmithy.CraftSystem)
					{
						hasTool = true;
						break;
					}
				}
			}

			if (!hasTool)
			{
				from.SendLocalizedMessage(1079822); // You need a blacksmithing tool in order to salvage ingots.
				return;
			}

			DefBlacksmithy.CheckAnvilAndForge(from, 2, out _, out var forge);

			if (!forge)
			{
				from.SendLocalizedMessage(1044265); // You must be near a forge.
				return;
			}

			var salvaged = 0;
			var notSalvaged = 0;

			foreach (var item in FindItemsByType<Item>())
			{
				if (item is BaseArmor ba)
				{
					if (Resmelt(from, item, ba.Resource))
					{
						salvaged++;
					}
					else
					{
						notSalvaged++;
					}
				}
				else if (item is BaseWeapon bw)
				{
					if (Resmelt(from, item, bw.Resource))
					{
						salvaged++;
					}
					else
					{
						notSalvaged++;
					}
				}
				else if (item is DragonBardingDeed bd)
				{
					if (Resmelt(from, item, bd.Resource))
					{
						salvaged++;
					}
					else
					{
						notSalvaged++;
					}
				}
			}

			from.SendLocalizedMessage(1079973, $"{salvaged}\t{salvaged + notSalvaged}"); // Salvaged: ~1_COUNT~/~2_NUM~ blacksmithed items

			if (notSalvaged > 0)
			{
				from.SendLocalizedMessage(1079975); // You failed to smelt some metal for lack of skill.
			}
		}

		private void SalvageCloth(Mobile from)
		{
			var scissors = from.Backpack.FindItemByType<Scissors>();

			if (scissors == null)
			{
				from.SendLocalizedMessage(1079823); // You need scissors in order to salvage cloth.
				return;
			}

			var salvaged = 0;
			var notSalvaged = 0;

			foreach (var item in FindItemsByType<Item>())
			{
				if (item is IScissorable scissorable)
				{
					if (Scissors.CanScissor(from, scissorable) && scissorable.Scissor(from, scissors))
					{
						++salvaged;
					}
					else
					{
						++notSalvaged;
					}
				}
			}

			from.SendLocalizedMessage(1079974, $"{salvaged:N0}\t{salvaged + notSalvaged:N0}"); // Salvaged: ~1_COUNT~/~2_NUM~ tailored items

			foreach (var i in FindItemsByType<Item>(true))
			{
				if (i is Leather or Cloth or SpinedLeather or HornedLeather or BarbedLeather or Bandage or Bone)
				{
					from.AddToBackpack(i);
				}
			}
		}

		private void SalvageAll(Mobile from)
		{
			SalvageIngots(from);
			SalvageCloth(from);
		}

		#endregion

		#region Context Menu

		private class SalvageAllEntry : ContextMenuEntry
		{
			private readonly SalvageBag m_Bag;

			public SalvageAllEntry(SalvageBag bag, bool enabled)
				: base(6276)
			{
				m_Bag = bag;

				if (!enabled)
				{
					Flags |= CMEFlags.Disabled;
				}
			}

			public override void OnClick()
			{
				if (m_Bag.Deleted)
				{
					return;
				}

				var from = Owner.From;

				if (from.CheckAlive())
				{
					m_Bag.SalvageAll(from);
				}
			}
		}

		private class SalvageIngotsEntry : ContextMenuEntry
		{
			private readonly SalvageBag m_Bag;

			public SalvageIngotsEntry(SalvageBag bag, bool enabled)
				: base(6277)
			{
				m_Bag = bag;

				if (!enabled)
				{
					Flags |= CMEFlags.Disabled;
				}
			}

			public override void OnClick()
			{
				if (m_Bag.Deleted)
				{
					return;
				}

				var from = Owner.From;

				if (from.CheckAlive())
				{
					m_Bag.SalvageIngots(from);
				}
			}
		}

		private class SalvageClothEntry : ContextMenuEntry
		{
			private readonly SalvageBag m_Bag;

			public SalvageClothEntry(SalvageBag bag, bool enabled)
				: base(6278)
			{
				m_Bag = bag;

				if (!enabled)
				{
					Flags |= CMEFlags.Disabled;
				}
			}

			public override void OnClick()
			{
				if (m_Bag.Deleted)
				{
					return;
				}

				var from = Owner.From;

				if (from.CheckAlive())
				{
					m_Bag.SalvageCloth(from);
				}
			}
		}

		#endregion

		#region Serialization

		public SalvageBag(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadEncodedInt();
		}

		#endregion
	}
}