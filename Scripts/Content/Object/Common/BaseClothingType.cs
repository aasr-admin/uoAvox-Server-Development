using Server.Engines.Craft;
using Server.Network;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	/// BaseCloak
	public abstract class BaseCloak : BaseClothing
	{
		public BaseCloak(int itemID) : this(itemID, 0)
		{
		}

		public BaseCloak(int itemID, int hue) : base(itemID, Layer.Cloak, hue)
		{
		}

		public BaseCloak(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// BaseHat
	public abstract class BaseHat : BaseClothing, IShipwreckedItem
	{
		private bool m_IsShipwreckedItem;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsShipwreckedItem
		{
			get => m_IsShipwreckedItem;
			set => m_IsShipwreckedItem = value;
		}

		public BaseHat(int itemID) : this(itemID, 0)
		{
		}

		public BaseHat(int itemID, int hue) : base(itemID, Layer.Helm, hue)
		{
		}

		public BaseHat(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_IsShipwreckedItem);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_IsShipwreckedItem = reader.ReadBool();
						break;
					}
			}
		}

		public override void AddEquipInfoAttributes(Mobile from, List<EquipInfoAttribute> attrs)
		{
			base.AddEquipInfoAttributes(from, attrs);

			if (m_IsShipwreckedItem)
			{
				attrs.Add(new EquipInfoAttribute(1041645)); // recovered from a shipwreck
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if (m_IsShipwreckedItem)
			{
				list.Add(1041645); // recovered from a shipwreck
			}
		}

		public override int OnCraft(int quality, bool makersMark, Mobile from, ICraftSystem craftSystem, Type typeRes, ICraftTool tool, ICraftItem craftItem, int resHue)
		{
			Quality = (ItemQuality)quality;

			if (Quality == ItemQuality.Exceptional)
			{
				DistributeBonuses(tool is BaseRunicTool ? 6 : (Core.SE ? 15 : 14));   //BLAME OSI. (We can't confirm it's an OSI bug yet.)
			}

			return base.OnCraft(quality, makersMark, from, craftSystem, typeRes, tool, craftItem, resHue);
		}
	}

	/// BaseOuterTorso
	public abstract class BaseOuterTorso : BaseClothing
	{
		public BaseOuterTorso(int itemID) : this(itemID, 0)
		{
		}

		public BaseOuterTorso(int itemID, int hue) : base(itemID, Layer.OuterTorso, hue)
		{
		}

		public BaseOuterTorso(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// BaseMiddleTorso
	public abstract class BaseMiddleTorso : BaseClothing
	{
		public BaseMiddleTorso(int itemID) : this(itemID, 0)
		{
		}

		public BaseMiddleTorso(int itemID, int hue) : base(itemID, Layer.MiddleTorso, hue)
		{
		}

		public BaseMiddleTorso(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// BaseOuterLegs
	public abstract class BaseOuterLegs : BaseClothing
	{
		public BaseOuterLegs(int itemID) : this(itemID, 0)
		{
		}

		public BaseOuterLegs(int itemID, int hue) : base(itemID, Layer.OuterLegs, hue)
		{
		}

		public BaseOuterLegs(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// BasePants
	public abstract class BasePants : BaseClothing
	{
		public BasePants(int itemID) : this(itemID, 0)
		{
		}

		public BasePants(int itemID, int hue) : base(itemID, Layer.Pants, hue)
		{
		}

		public BasePants(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// BaseShirt
	public abstract class BaseShirt : BaseClothing
	{
		public BaseShirt(int itemID) : this(itemID, 0)
		{
		}

		public BaseShirt(int itemID, int hue) : base(itemID, Layer.Shirt, hue)
		{
		}

		public BaseShirt(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// BaseWaist
	public abstract class BaseWaist : BaseClothing
	{
		public BaseWaist(int itemID) : this(itemID, 0)
		{
		}

		public BaseWaist(int itemID, int hue) : base(itemID, Layer.Waist, hue)
		{
		}

		public BaseWaist(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
		}
	}

	/// BaseShoes
	public abstract class BaseShoes : BaseClothing
	{
		public BaseShoes(int itemID) : this(itemID, 0)
		{
		}

		public BaseShoes(int itemID, int hue) : base(itemID, Layer.Shoes, hue)
		{
		}

		public BaseShoes(Serial serial) : base(serial)
		{
		}

		public override bool Scissor(Mobile from, Scissors scissors)
		{
			if (DefaultResource == CraftResource.None)
			{
				return base.Scissor(from, scissors);
			}

			from.SendLocalizedMessage(502440); // Scissors can not be used on that to produce anything.
			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(2); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 2: break; // empty, resource removed
				case 1:
					{
						m_Resource = (CraftResource)reader.ReadInt();
						break;
					}
				case 0:
					{
						m_Resource = DefaultResource;
						break;
					}
			}
		}
	}
}