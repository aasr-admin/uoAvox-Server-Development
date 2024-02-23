
using System;

namespace Server.Items
{
	/// Burried And Sunken Treasure Chests
	[FlipableAttribute(0xe43, 0xe42)]
	public class WoodenTreasureChest : BaseTreasureChest
	{
		[Constructable]
		public WoodenTreasureChest() : base(0xE43)
		{
		}

		public WoodenTreasureChest(Serial serial) : base(serial)
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

	[FlipableAttribute(0xe41, 0xe40)]
	public class MetalGoldenTreasureChest : BaseTreasureChest
	{
		[Constructable]
		public MetalGoldenTreasureChest() : base(0xE41)
		{
		}

		public MetalGoldenTreasureChest(Serial serial) : base(serial)
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

	[FlipableAttribute(0x9ab, 0xe7c)]
	public class MetalTreasureChest : BaseTreasureChest
	{
		[Constructable]
		public MetalTreasureChest() : base(0x9AB)
		{
		}

		public MetalTreasureChest(Serial serial) : base(serial)
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


	/// Treasure Chests Hidden In Dungeons
	public class TreasureChestLevel1 : LockableContainer
	{
		private const int m_Level = 1;

		public override bool Decays => true;

		public override bool IsDecoContainer => false;

		public override TimeSpan DecayTime => TimeSpan.FromMinutes(Utility.Random(15, 60));

		private void SetChestAppearance()
		{
			var UseFirstItemId = Utility.RandomBool();

			switch (Utility.RandomList(0, 1, 2))
			{
				case 0:// Large Crate
					ItemID = (UseFirstItemId ? 0xe3c : 0xe3d);
					GumpID = 0x44;
					break;

				case 1:// Medium Crate
					ItemID = (UseFirstItemId ? 0xe3e : 0xe3f);
					GumpID = 0x44;
					break;

				case 2:// Small Crate
					ItemID = (UseFirstItemId ? 0x9a9 : 0xe7e);
					GumpID = 0x44;
					break;
			}
		}

		public override int DefaultGumpID => 0x42;

		public override int DefaultDropSound => 0x42;

		public override Rectangle2D Bounds => new Rectangle2D(18, 105, 144, 73);

		[Constructable]
		public TreasureChestLevel1()
			: base(0xE41)
		{
			SetChestAppearance();
			Movable = false;

			TrapType = TrapType.DartTrap;
			TrapPower = m_Level * Utility.Random(1, 25);
			Locked = true;

			RequiredSkill = 57;
			LockLevel = RequiredSkill - Utility.Random(1, 10);
			MaxLockLevel = RequiredSkill + Utility.Random(1, 10);

			// According to OSI, loot in level 1 chest is:
			//  Gold 25 - 50
			//  Bolts 10
			//  Gems
			//  Normal weapon
			//  Normal armour
			//  Normal clothing
			//  Normal jewelry

			// Gold
			DropItem(new Gold(Utility.Random(30, 100)));

			// Drop bolts
			//DropItem( new Bolt( 10 ) );

			// Gems
			if (Utility.RandomBool() == true)
			{
				var GemLoot = Loot.RandomGem();
				GemLoot.Amount = Utility.Random(1, 3);
				DropItem(GemLoot);
			}

			// Weapon
			if (Utility.RandomBool() == true)
			{
				DropItem(Loot.RandomWeapon());
			}

			// Armour
			if (Utility.RandomBool() == true)
			{
				DropItem(Loot.RandomArmorOrShield());
			}

			// Clothing
			if (Utility.RandomBool() == true)
			{
				DropItem(Loot.RandomClothing());
			}

			// Jewelry
			if (Utility.RandomBool() == true)
			{
				DropItem(Loot.RandomJewelry());
			}
		}

		public TreasureChestLevel1(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(1); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			var version = reader.ReadInt();
		}
	}   // Level 01

	public class TreasureChestLevel2 : LockableContainer
	{
		private const int m_Level = 2;

		public override bool Decays => true;

		public override bool IsDecoContainer => false;

		public override TimeSpan DecayTime => TimeSpan.FromMinutes(Utility.Random(15, 60));

		private void SetChestAppearance()
		{
			var UseFirstItemId = Utility.RandomBool();

			switch (Utility.RandomList(0, 1, 2, 3, 4, 5, 6, 7))
			{
				case 0:// Large Crate
					ItemID = (UseFirstItemId ? 0xe3c : 0xe3d);
					GumpID = 0x44;
					break;

				case 1:// Medium Crate
					ItemID = (UseFirstItemId ? 0xe3e : 0xe3f);
					GumpID = 0x44;
					break;

				case 2:// Small Crate
					ItemID = (UseFirstItemId ? 0x9a9 : 0xe7e);
					GumpID = 0x44;
					break;

				case 3:// Wooden Chest
					ItemID = (UseFirstItemId ? 0xe42 : 0xe43);
					GumpID = 0x49;
					break;

				case 4:// Metal Chest
					ItemID = (UseFirstItemId ? 0x9ab : 0xe7c);
					GumpID = 0x4A;
					break;

				case 5:// Metal Golden Chest
					ItemID = (UseFirstItemId ? 0xe40 : 0xe41);
					GumpID = 0x42;
					break;

				case 6:// Keg
					ItemID = (UseFirstItemId ? 0xe7f : 0xe7f);
					GumpID = 0x3e;
					break;

				case 7:// Barrel
					ItemID = (UseFirstItemId ? 0xe77 : 0xe77);
					GumpID = 0x3e;
					break;
			}
		}

		public override int DefaultGumpID => 0x42;

		public override int DefaultDropSound => 0x42;

		public override Rectangle2D Bounds => new Rectangle2D(18, 105, 144, 73);

		[Constructable]
		public TreasureChestLevel2()
			: base(0xE41)
		{
			SetChestAppearance();
			Movable = false;

			TrapType = TrapType.ExplosionTrap;
			TrapPower = m_Level * Utility.Random(1, 25);
			Locked = true;

			RequiredSkill = 72;
			LockLevel = RequiredSkill - Utility.Random(1, 10);
			MaxLockLevel = RequiredSkill + Utility.Random(1, 10); ;

			// According to OSI, loot in level 2 chest is:
			//  Gold 80 - 150
			//  Arrows 10
			//  Reagents
			//  Scrolls
			//  Potions
			//  Gems

			// Gold
			DropItem(new Gold(Utility.Random(70, 100)));

			// Drop bolts
			//DropItem( new Arrow( 10 ) );

			// Reagents
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var ReagentLoot = Loot.RandomReagent();
				ReagentLoot.Amount = Utility.Random(1, m_Level);
				DropItem(ReagentLoot);
			}

			// Scrolls
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				Item ScrollLoot = Loot.RandomScroll(SpellCircle.First, SpellCircle.Fifth);
				ScrollLoot.Amount = Utility.Random(1, 8);
				DropItem(ScrollLoot);
			}

			// Potions
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var PotionLoot = Loot.RandomPotion();
				DropItem(PotionLoot);
			}

			// Gems
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var GemLoot = Loot.RandomGem();
				GemLoot.Amount = Utility.Random(1, 6);
				DropItem(GemLoot);
			}
		}

		public TreasureChestLevel2(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(1); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			var version = reader.ReadInt();
		}
	}    // Level 02

	public class TreasureChestLevel3 : LockableContainer
	{
		private const int m_Level = 3;

		public override bool Decays => true;

		public override bool IsDecoContainer => false;

		public override TimeSpan DecayTime => TimeSpan.FromMinutes(Utility.Random(15, 60));

		private void SetChestAppearance()
		{
			var UseFirstItemId = Utility.RandomBool();
			switch (Utility.RandomList(0, 1, 2))
			{
				case 0:// Wooden Chest
					ItemID = (UseFirstItemId ? 0xe42 : 0xe43);
					GumpID = 0x49;
					break;

				case 1:// Metal Chest
					ItemID = (UseFirstItemId ? 0x9ab : 0xe7c);
					GumpID = 0x4A;
					break;

				case 2:// Metal Golden Chest
					ItemID = (UseFirstItemId ? 0xe40 : 0xe41);
					GumpID = 0x42;
					break;
			}
		}

		public override int DefaultGumpID => 0x42;

		public override int DefaultDropSound => 0x42;

		public override Rectangle2D Bounds => new Rectangle2D(18, 105, 144, 73);

		[Constructable]
		public TreasureChestLevel3()
			: base(0xE41)
		{
			SetChestAppearance();
			Movable = false;

			TrapType = TrapType.PoisonTrap;
			TrapPower = m_Level * Utility.Random(1, 25);
			Locked = true;

			RequiredSkill = 84;
			LockLevel = RequiredSkill - Utility.Random(1, 10);
			MaxLockLevel = RequiredSkill + Utility.Random(1, 10); ;

			// According to OSI, loot in level 3 chest is:
			//  Gold 250 - 350
			//  Arrows 10
			//  Reagents
			//  Scrolls
			//  Potions
			//  Gems
			//  Magic Wand
			//  Magic weapon
			//  Magic armour
			//  Magic clothing  (not implemented)
			//  Magic jewelry  (not implemented)

			// Gold
			DropItem(new Gold(Utility.Random(180, 240)));

			// Drop bolts
			//DropItem( new Arrow( 10 ) );

			// Reagents
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var ReagentLoot = Loot.RandomReagent();
				ReagentLoot.Amount = Utility.Random(1, 9);
				DropItem(ReagentLoot);
			}

			// Scrolls
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				Item ScrollLoot = Loot.RandomScroll(SpellCircle.First, SpellCircle.Sixth);
				ScrollLoot.Amount = Utility.Random(1, 12);
				DropItem(ScrollLoot);
			}

			// Potions
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var PotionLoot = Loot.RandomPotion();
				DropItem(PotionLoot);
			}

			// Gems
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var GemLoot = Loot.RandomGem();
				GemLoot.Amount = Utility.Random(1, 9);
				DropItem(GemLoot);
			}

			// Magic Wand
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				DropItem(Loot.RandomWand());
			}

			// Equipment
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var item = Loot.RandomArmorOrShieldOrWeapon();

				if (item is BaseWeapon)
				{
					var weapon = (BaseWeapon)item;
					weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(m_Level);
					weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(m_Level);
					weapon.DurabilityLevel = (WeaponDurabilityLevel)Utility.Random(m_Level);
					weapon.Quality = ItemQuality.Regular;
				}
				else if (item is BaseArmor)
				{
					var armor = (BaseArmor)item;
					armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(m_Level);
					armor.Durability = (ArmorDurabilityLevel)Utility.Random(m_Level);
					armor.Quality = ItemQuality.Regular;
				}

				DropItem(item);
			}

			// Clothing
			for (var i = Utility.Random(1, 2); i > 1; i--)
			{
				DropItem(Loot.RandomClothing());
			}

			// Jewelry
			for (var i = Utility.Random(1, 2); i > 1; i--)
			{
				DropItem(Loot.RandomJewelry());
			}
		}

		public TreasureChestLevel3(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(1); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			var version = reader.ReadInt();
		}
	}    // Level 03

	public class TreasureChestLevel4 : LockableContainer
	{
		private const int m_Level = 4;

		public override bool Decays => true;

		public override bool IsDecoContainer => false;

		public override TimeSpan DecayTime => TimeSpan.FromMinutes(Utility.Random(15, 60));

		private void SetChestAppearance()
		{
			var UseFirstItemId = Utility.RandomBool();

			switch (Utility.Random(4))
			{
				case 0:// Wooden Chest
					ItemID = (UseFirstItemId ? 0xe42 : 0xe43);
					GumpID = 0x49;
					break;

				case 1:// Metal Chest
					ItemID = (UseFirstItemId ? 0x9ab : 0xe7c);
					GumpID = 0x4A;
					break;

				case 2:// Metal Golden Chest
					ItemID = (UseFirstItemId ? 0xe40 : 0xe41);
					GumpID = 0x42;
					break;

				case 3:// Keg
					ItemID = 0xe7f;
					GumpID = 0x3e;
					break;
			}
		}

		public override int DefaultGumpID => 0x42;

		public override int DefaultDropSound => 0x42;

		public override Rectangle2D Bounds => new Rectangle2D(18, 105, 144, 73);

		[Constructable]
		public TreasureChestLevel4()
			: base(0xE41)
		{
			SetChestAppearance();
			Movable = false;

			TrapType = TrapType.ExplosionTrap;
			TrapPower = m_Level * Utility.Random(10, 25);
			Locked = true;

			RequiredSkill = 92;
			LockLevel = RequiredSkill - Utility.Random(1, 10);
			MaxLockLevel = RequiredSkill + Utility.Random(1, 10); ;

			// According to OSI, loot in level 4 chest is:
			//  Gold 500 - 900
			//  Reagents
			//  Scrolls
			//  Blank scrolls
			//  Potions
			//  Gems
			//  Magic Wand
			//  Magic weapon
			//  Magic armour
			//  Magic clothing (not implemented)
			//  Magic jewelry (not implemented)
			//  Crystal ball (not implemented)

			// Gold
			DropItem(new Gold(Utility.Random(200, 400)));

			// Reagents
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var ReagentLoot = Loot.RandomReagent();
				ReagentLoot.Amount = 12;
				DropItem(ReagentLoot);
			}

			// Scrolls
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				Item ScrollLoot = Loot.RandomScroll(SpellCircle.First, SpellCircle.Sixth);
				ScrollLoot.Amount = 16;
				DropItem(ScrollLoot);
			}

			// Drop blank scrolls
			DropItem(new BlankScroll(Utility.Random(1, m_Level)));

			// Potions
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var PotionLoot = Loot.RandomPotion();
				DropItem(PotionLoot);
			}

			// Gems
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var GemLoot = Loot.RandomGem();
				GemLoot.Amount = 12;
				DropItem(GemLoot);
			}

			// Magic Wand
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				DropItem(Loot.RandomWand());
			}

			// Equipment
			for (var i = Utility.Random(1, m_Level); i > 1; i--)
			{
				var item = Loot.RandomArmorOrShieldOrWeapon();

				if (item is BaseWeapon)
				{
					var weapon = (BaseWeapon)item;
					weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(m_Level);
					weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(m_Level);
					weapon.DurabilityLevel = (WeaponDurabilityLevel)Utility.Random(m_Level);
					weapon.Quality = ItemQuality.Regular;
				}
				else if (item is BaseArmor)
				{
					var armor = (BaseArmor)item;
					armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(m_Level);
					armor.Durability = (ArmorDurabilityLevel)Utility.Random(m_Level);
					armor.Quality = ItemQuality.Regular;
				}

				DropItem(item);
			}

			// Clothing
			for (var i = Utility.Random(1, 2); i > 1; i--)
			{
				DropItem(Loot.RandomClothing());
			}

			// Jewelry
			for (var i = Utility.Random(1, 2); i > 1; i--)
			{
				DropItem(Loot.RandomJewelry());
			}

			// Crystal ball (not implemented)
		}

		public TreasureChestLevel4(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(1); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			var version = reader.ReadInt();
		}
	}    // Level 04
}