using Server.Targeting;

namespace Server.Items
{
	public class ArcaneGem : Item
	{
		public override string DefaultName => "arcane gem";

		[Constructable]
		public ArcaneGem() : base(0x1EA7)
		{
			Stackable = Core.ML;
			Weight = 1.0;
		}

		public ArcaneGem(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
			else
			{
				from.BeginTarget(2, false, TargetFlags.None, new TargetCallback(OnTarget));
				from.SendMessage("What do you wish to use the gem on?");
			}
		}

		public int GetChargesFor(Mobile m)
		{
			var v = (int)(m.Skills[SkillName.Tailoring].Value / 5);

			if (v < 16)
			{
				return 16;
			}
			else if (v > 24)
			{
				return 24;
			}

			return v;
		}

		public const int DefaultArcaneHue = 2117;

		public void OnTarget(Mobile from, object obj)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				return;
			}

			if (obj is IArcaneEquip && obj is Item)
			{
				var item = (Item)obj;
				var resource = CraftResource.None;

				if (item is BaseClothing)
				{
					resource = ((BaseClothing)item).Resource;
				}
				else if (item is BaseArmor)
				{
					resource = ((BaseArmor)item).Resource;
				}
				else if (item is BaseWeapon) // Sanity, weapons cannot recieve gems...
				{
					resource = ((BaseWeapon)item).Resource;
				}

				var eq = (IArcaneEquip)obj;

				if (!item.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
					return;
				}
				else if (item.LootType == LootType.Blessed)
				{
					from.SendMessage("You can only use this on exceptionally crafted robes, thigh boots, cloaks, or leather gloves.");
					return;
				}
				else if (resource != CraftResource.None && resource != CraftResource.RegularLeather)
				{
					from.SendLocalizedMessage(1049690); // Arcane gems can not be used on that type of leather.
					return;
				}

				var charges = GetChargesFor(from);

				if (eq.IsArcane)
				{
					if (eq.CurArcaneCharges >= eq.MaxArcaneCharges)
					{
						from.SendMessage("That item is already fully charged.");
					}
					else
					{
						if (eq.CurArcaneCharges <= 0)
						{
							item.Hue = DefaultArcaneHue;
						}

						if ((eq.CurArcaneCharges + charges) > eq.MaxArcaneCharges)
						{
							eq.CurArcaneCharges = eq.MaxArcaneCharges;
						}
						else
						{
							eq.CurArcaneCharges += charges;
						}

						from.SendMessage("You recharge the item.");
						if (Amount <= 1)
						{
							Delete();
						}
						else
						{
							Amount--;
						}
					}
				}
				else if (from.Skills[SkillName.Tailoring].Value >= 80.0)
				{
					var isExceptional = false;

					if (item is BaseClothing)
					{
						isExceptional = (((BaseClothing)item).Quality == ItemQuality.Exceptional);
					}
					else if (item is BaseArmor)
					{
						isExceptional = (((BaseArmor)item).Quality == ItemQuality.Exceptional);
					}
					else if (item is BaseWeapon)
					{
						isExceptional = (((BaseWeapon)item).Quality == ItemQuality.Exceptional);
					}

					if (isExceptional)
					{
						if (item is BaseClothing)
						{
							((BaseClothing)item).Quality = ItemQuality.Regular;
							((BaseClothing)item).Crafter = from;
						}
						else if (item is BaseArmor)
						{
							((BaseArmor)item).Quality = ItemQuality.Regular;
							((BaseArmor)item).Crafter = from;
							((BaseArmor)item).PhysicalBonus = ((BaseArmor)item).FireBonus = ((BaseArmor)item).ColdBonus = ((BaseArmor)item).PoisonBonus = ((BaseArmor)item).EnergyBonus = 0; // Is there a method to remove bonuses?
						}
						else if (item is BaseWeapon) // Sanity, weapons cannot recieve gems...
						{
							((BaseWeapon)item).Quality = ItemQuality.Regular;
							((BaseWeapon)item).Crafter = from;
						}

						eq.CurArcaneCharges = eq.MaxArcaneCharges = charges;

						item.Hue = DefaultArcaneHue;

						from.SendMessage("You enhance the item with your gem.");
						if (Amount <= 1)
						{
							Delete();
						}
						else
						{
							Amount--;
						}
					}
					else
					{
						from.SendMessage("Only exceptional items can be enhanced with the gem.");
					}
				}
				else
				{
					from.SendMessage("You do not have enough skill in tailoring to enhance the item.");
				}
			}
			else
			{
				from.SendMessage("You can only use this on exceptionally crafted robes, thigh boots, cloaks, or leather gloves.");
			}
		}

		public static bool ConsumeCharges(Mobile from, int amount)
		{
			var items = from.Items;
			var avail = 0;

			for (var i = 0; i < items.Count; ++i)
			{
				var obj = items[i];

				if (obj is IArcaneEquip)
				{
					var eq = (IArcaneEquip)obj;

					if (eq.IsArcane)
					{
						avail += eq.CurArcaneCharges;
					}
				}
			}

			if (avail < amount)
			{
				return false;
			}

			for (var i = 0; i < items.Count; ++i)
			{
				var obj = items[i];

				if (obj is IArcaneEquip)
				{
					var eq = (IArcaneEquip)obj;

					if (eq.IsArcane)
					{
						if (eq.CurArcaneCharges > amount)
						{
							eq.CurArcaneCharges -= amount;
							break;
						}
						else
						{
							amount -= eq.CurArcaneCharges;
							eq.CurArcaneCharges = 0;
						}
					}
				}
			}

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}
}