using Server.ContextMenus;
using Server.Engines.PartySystem;
using Server.Gumps;
using Server.Network;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	/// Sunken Treasure Object
	public interface IShipwreckedItem
	{
		bool IsShipwreckedItem { get; set; }
	}

	public class ShipwreckedItem : Item, IDyable, IShipwreckedItem
	{
		public ShipwreckedItem(int itemID) : base(itemID)
		{
			var weight = ItemData.Weight;

			if (weight >= 255)
			{
				weight = 1;
			}

			Weight = weight;
		}

		public override void OnSingleClick(Mobile from)
		{
			LabelTo(from, 1050039, String.Format("#{0}\t#1041645", LabelNumber));
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			list.Add(1041645); // recovered from a shipwreck
		}

		public ShipwreckedItem(Serial serial) : base(serial)
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

		public bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
			{
				return false;
			}

			if (ItemID >= 0x13A4 && ItemID <= 0x13AE)
			{
				Hue = sender.DyedHue;
				return true;
			}

			from.SendLocalizedMessage(sender.FailMessage);
			return false;
		}

		#region IShipwreckedItem Members

		public bool IsShipwreckedItem
		{
			get => true;    //It's a ShipwreckedItem item.  'Course it's gonna be a Shipwreckeditem
			set
			{
			}
		}

		#endregion
	}


	/// Dungeon Treasure Chests
	public class BaseTreasureChest : LockableContainer
	{
		private TreasureLevel m_TreasureLevel;
		private short m_MaxSpawnTime = 60;
		private short m_MinSpawnTime = 10;
		private TreasureResetTimer m_ResetTimer;

		[CommandProperty(AccessLevel.GameMaster)]
		public TreasureLevel Level
		{
			get => m_TreasureLevel;
			set => m_TreasureLevel = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public short MaxSpawnTime
		{
			get => m_MaxSpawnTime;
			set => m_MaxSpawnTime = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public short MinSpawnTime
		{
			get => m_MinSpawnTime;
			set => m_MinSpawnTime = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override bool Locked
		{
			get => base.Locked;
			set
			{
				if (base.Locked != value)
				{
					base.Locked = value;

					if (!value)
					{
						StartResetTimer();
					}
				}
			}
		}

		public override bool IsDecoContainer => false;

		public BaseTreasureChest(int itemID) : this(itemID, TreasureLevel.Level2)
		{
		}

		public BaseTreasureChest(int itemID, TreasureLevel level) : base(itemID)
		{
			m_TreasureLevel = level;
			Locked = true;
			Movable = false;

			SetLockLevel();
			GenerateTreasure();
		}

		public BaseTreasureChest(Serial serial) : base(serial)
		{
		}

		public override string DefaultName
		{
			get
			{
				if (Locked)
				{
					return "a locked treasure chest";
				}

				return "a treasure chest";
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
			writer.Write((byte)m_TreasureLevel);
			writer.Write(m_MinSpawnTime);
			writer.Write(m_MaxSpawnTime);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			m_TreasureLevel = (TreasureLevel)reader.ReadByte();
			m_MinSpawnTime = reader.ReadShort();
			m_MaxSpawnTime = reader.ReadShort();

			if (!Locked)
			{
				StartResetTimer();
			}
		}

		protected virtual void SetLockLevel()
		{
			switch (m_TreasureLevel)
			{
				case TreasureLevel.Level1:
					RequiredSkill = LockLevel = 5;
					break;

				case TreasureLevel.Level2:
					RequiredSkill = LockLevel = 20;
					break;

				case TreasureLevel.Level3:
					RequiredSkill = LockLevel = 50;
					break;

				case TreasureLevel.Level4:
					RequiredSkill = LockLevel = 70;
					break;

				case TreasureLevel.Level5:
					RequiredSkill = LockLevel = 90;
					break;

				case TreasureLevel.Level6:
					RequiredSkill = LockLevel = 100;
					break;
			}
		}

		private void StartResetTimer()
		{
			if (m_ResetTimer == null)
			{
				m_ResetTimer = new TreasureResetTimer(this);
			}
			else
			{
				m_ResetTimer.Delay = TimeSpan.FromMinutes(Utility.Random(m_MinSpawnTime, m_MaxSpawnTime));
			}

			m_ResetTimer.Start();
		}

		protected virtual void GenerateTreasure()
		{
			var MinGold = 1;
			var MaxGold = 2;

			switch (m_TreasureLevel)
			{
				case TreasureLevel.Level1:
					MinGold = 100;
					MaxGold = 300;
					break;

				case TreasureLevel.Level2:
					MinGold = 300;
					MaxGold = 600;
					break;

				case TreasureLevel.Level3:
					MinGold = 600;
					MaxGold = 900;
					break;

				case TreasureLevel.Level4:
					MinGold = 900;
					MaxGold = 1200;
					break;

				case TreasureLevel.Level5:
					MinGold = 1200;
					MaxGold = 5000;
					break;

				case TreasureLevel.Level6:
					MinGold = 5000;
					MaxGold = 9000;
					break;
			}

			DropItem(new Gold(MinGold, MaxGold));
		}

		public void ClearContents()
		{
			for (var i = Items.Count - 1; i >= 0; --i)
			{
				if (i < Items.Count)
				{
					Items[i].Delete();
				}
			}
		}

		public void Reset()
		{
			if (m_ResetTimer != null)
			{
				if (m_ResetTimer.Running)
				{
					m_ResetTimer.Stop();
				}
			}

			Locked = true;
			ClearContents();
			GenerateTreasure();
		}

		public enum TreasureLevel
		{
			Level1,
			Level2,
			Level3,
			Level4,
			Level5,
			Level6,
		};

		private class TreasureResetTimer : Timer
		{
			private readonly BaseTreasureChest m_Chest;

			public TreasureResetTimer(BaseTreasureChest chest) : base(TimeSpan.FromMinutes(Utility.Random(chest.MinSpawnTime, chest.MaxSpawnTime)))
			{
				m_Chest = chest;
				Priority = TimerPriority.OneMinute;
			}

			protected override void OnTick()
			{
				m_Chest.Reset();
			}
		}
	}


	/// Burried Treasure Chests
	public class TreasureMapChest : LockableContainer
	{
		public override int LabelNumber => 3000541;

		public static Type[] Artifacts => m_Artifacts;

		private static readonly Type[] m_Artifacts = new Type[]
		{
			typeof( CandelabraOfSouls ), typeof( GoldBricks ), typeof( PhillipsWoodenSteed ),
			typeof( ArcticDeathDealer ), typeof( BlazeOfDeath ), typeof( BurglarsBandana ),
			typeof( CavortingClub ), typeof( DreadPirateHat ),
			typeof( EnchantedTitanLegBone ), typeof( GwennosHarp ), typeof( IolosLute ),
			typeof( LunaLance ), typeof( NightsKiss ), typeof( NoxRangersHeavyCrossbow ),
			typeof( PolarBearMask ), typeof( VioletCourage ), typeof( HeartOfTheLion ),
			typeof( ColdBlood ), typeof( AlchemistsBauble )
		};

		private int m_Level;
		private DateTime m_DeleteTime;
		private Timer m_Timer;
		private Mobile m_Owner;
		private bool m_Temporary;

		private List<Mobile> m_Guardians;

		[CommandProperty(AccessLevel.GameMaster)]
		public int Level { get => m_Level; set => m_Level = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get => m_Owner; set => m_Owner = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime DeleteTime => m_DeleteTime;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Temporary { get => m_Temporary; set => m_Temporary = value; }

		public List<Mobile> Guardians => m_Guardians;

		[Constructable]
		public TreasureMapChest(int level) : this(null, level, false)
		{
		}

		public TreasureMapChest(Mobile owner, int level, bool temporary) : base(0xE40)
		{
			m_Owner = owner;
			m_Level = level;
			m_DeleteTime = DateTime.UtcNow + TimeSpan.FromHours(3.0);

			m_Temporary = temporary;
			m_Guardians = new List<Mobile>();

			m_Timer = new DeleteTimer(this, m_DeleteTime);
			m_Timer.Start();

			Fill(this, level);
		}

		private static void GetRandomAOSStats(out int attributeCount, out int min, out int max)
		{
			var rnd = Utility.Random(15);

			if (Core.SE)
			{
				if (rnd < 1)
				{
					attributeCount = Utility.RandomMinMax(3, 5);
					min = 50; max = 100;
				}
				else if (rnd < 3)
				{
					attributeCount = Utility.RandomMinMax(2, 5);
					min = 40; max = 80;
				}
				else if (rnd < 6)
				{
					attributeCount = Utility.RandomMinMax(2, 4);
					min = 30; max = 60;
				}
				else if (rnd < 10)
				{
					attributeCount = Utility.RandomMinMax(1, 3);
					min = 20; max = 40;
				}
				else
				{
					attributeCount = 1;
					min = 10; max = 20;
				}
			}
			else
			{
				if (rnd < 1)
				{
					attributeCount = Utility.RandomMinMax(2, 5);
					min = 20; max = 70;
				}
				else if (rnd < 3)
				{
					attributeCount = Utility.RandomMinMax(2, 4);
					min = 20; max = 50;
				}
				else if (rnd < 6)
				{
					attributeCount = Utility.RandomMinMax(2, 3);
					min = 20; max = 40;
				}
				else if (rnd < 10)
				{
					attributeCount = Utility.RandomMinMax(1, 2);
					min = 10; max = 30;
				}
				else
				{
					attributeCount = 1;
					min = 10; max = 20;
				}
			}
		}

		public static void Fill(LockableContainer cont, int level)
		{
			cont.Movable = false;
			cont.Locked = true;
			int numberItems;

			if (level == 0)
			{
				cont.LockLevel = 0; // Can't be unlocked

				cont.DropItem(new Gold(Utility.RandomMinMax(50, 100)));

				if (Utility.RandomDouble() < 0.75)
				{
					cont.DropItem(new TreasureMap(0, Map.Trammel));
				}
			}
			else
			{
				cont.TrapType = TrapType.ExplosionTrap;
				cont.TrapPower = level * 25;
				cont.TrapLevel = level;

				switch (level)
				{
					case 1: cont.RequiredSkill = 36; break;
					case 2: cont.RequiredSkill = 76; break;
					case 3: cont.RequiredSkill = 84; break;
					case 4: cont.RequiredSkill = 92; break;
					case 5: cont.RequiredSkill = 100; break;
					case 6: cont.RequiredSkill = 100; break;
				}

				cont.LockLevel = cont.RequiredSkill - 10;
				cont.MaxLockLevel = cont.RequiredSkill + 40;

				//Publish 67 gold change
				//if ( Core.SA )
				//	cont.DropItem( new Gold( level * 5000 ) );
				//else					
				cont.DropItem(new Gold(level * 1000));

				for (var i = 0; i < level * 5; ++i)
				{
					cont.DropItem(Loot.RandomScroll(SpellSchool.Magery));
				}

				if (Core.SE)
				{
					switch (level)
					{
						case 1: numberItems = 5; break;
						case 2: numberItems = 10; break;
						case 3: numberItems = 15; break;
						case 4: numberItems = 38; break;
						case 5: numberItems = 50; break;
						case 6: numberItems = 60; break;
						default: numberItems = 0; break;
					};
				}
				else
				{
					numberItems = level * 6;
				}

				for (var i = 0; i < numberItems; ++i)
				{
					Item item;

					if (Core.AOS)
					{
						item = Loot.RandomArmorOrShieldOrWeaponOrJewelry();
					}
					else
					{
						item = Loot.RandomArmorOrShieldOrWeapon();
					}

					if (item is BaseWeapon)
					{
						var weapon = (BaseWeapon)item;

						if (Core.AOS)
						{
							int attributeCount;
							int min, max;

							GetRandomAOSStats(out attributeCount, out min, out max);

							BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);
						}
						else
						{
							weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(6);
							weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(6);
							weapon.DurabilityLevel = (WeaponDurabilityLevel)Utility.Random(6);
						}

						cont.DropItem(item);
					}
					else if (item is BaseArmor)
					{
						var armor = (BaseArmor)item;

						if (Core.AOS)
						{
							int attributeCount;
							int min, max;

							GetRandomAOSStats(out attributeCount, out min, out max);

							BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);
						}
						else
						{
							armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(6);
							armor.Durability = (ArmorDurabilityLevel)Utility.Random(6);
						}

						cont.DropItem(item);
					}
					else if (item is BaseHat)
					{
						var hat = (BaseHat)item;

						if (Core.AOS)
						{
							int attributeCount;
							int min, max;

							GetRandomAOSStats(out attributeCount, out min, out max);

							BaseRunicTool.ApplyAttributesTo(hat, attributeCount, min, max);
						}

						cont.DropItem(item);
					}
					else if (item is BaseJewel)
					{
						int attributeCount;
						int min, max;

						GetRandomAOSStats(out attributeCount, out min, out max);

						BaseRunicTool.ApplyAttributesTo((BaseJewel)item, attributeCount, min, max);

						cont.DropItem(item);
					}
				}
			}

			int reagents;
			if (level == 0)
			{
				reagents = 12;
			}
			else
			{
				reagents = level * 3;
			}

			for (var i = 0; i < reagents; i++)
			{
				var item = Loot.RandomPossibleReagent();
				item.Amount = Utility.RandomMinMax(40, 60);
				cont.DropItem(item);
			}

			int gems;
			if (level == 0)
			{
				gems = 2;
			}
			else
			{
				gems = level * 3;
			}

			for (var i = 0; i < gems; i++)
			{
				var item = Loot.RandomGem();
				cont.DropItem(item);
			}

			if (level == 6 && Core.AOS)
			{
				cont.DropItem((Item)Activator.CreateInstance(m_Artifacts[Utility.Random(m_Artifacts.Length)]));
			}
		}

		public override bool CheckLocked(Mobile from)
		{
			if (!Locked)
			{
				return false;
			}

			if (Level == 0 && from.AccessLevel < AccessLevel.GameMaster)
			{
				foreach (var m in Guardians)
				{
					if (m.Alive)
					{
						from.SendLocalizedMessage(1046448); // You must first kill the guardians before you may open this chest.
						return true;
					}
				}

				LockPick(from);
				return false;
			}
			else
			{
				return base.CheckLocked(from);
			}
		}

		private List<Item> m_Lifted = new List<Item>();

		private bool CheckLoot(Mobile m, bool criminalAction)
		{
			if (m_Temporary)
			{
				return false;
			}

			if (m.AccessLevel >= AccessLevel.GameMaster || m_Owner == null || m == m_Owner)
			{
				return true;
			}

			var p = Party.Get(m_Owner);

			if (p != null && p.Contains(m))
			{
				return true;
			}

			var map = Map;

			if (map != null && (map.Rules & MapRules.HarmfulRestrictions) == 0)
			{
				if (criminalAction)
				{
					m.CriminalAction(true);
				}
				else
				{
					m.SendLocalizedMessage(1010630); // Taking someone else's treasure is a criminal offense!
				}

				return true;
			}

			m.SendLocalizedMessage(1010631); // You did not discover this chest!
			return false;
		}

		public override bool IsDecoContainer => false;

		public override bool CheckItemUse(Mobile from, Item item)
		{
			return CheckLoot(from, item != this) && base.CheckItemUse(from, item);
		}

		public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			return CheckLoot(from, true) && base.CheckLift(from, item, ref reject);
		}

		public override void OnItemLifted(Mobile from, Item item)
		{
			var notYetLifted = !m_Lifted.Contains(item);

			from.RevealingAction();

			if (notYetLifted)
			{
				m_Lifted.Add(item);

				if (0.1 >= Utility.RandomDouble()) // 10% chance to spawn a new monster
				{
					TreasureMap.Spawn(m_Level, GetWorldLocation(), Map, from, false);
				}
			}

			base.OnItemLifted(from, item);
		}

		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			if (m.AccessLevel < AccessLevel.GameMaster)
			{
				m.SendLocalizedMessage(1048122, "", 0x8A5); // The chest refuses to be filled with treasure again.
				return false;
			}

			return base.CheckHold(m, item, message, checkItems, plusItems, plusWeight);
		}

		public TreasureMapChest(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(2); // version

			writer.Write(m_Guardians, true);
			writer.Write(m_Temporary);

			writer.Write(m_Owner);

			writer.Write(m_Level);
			writer.WriteDeltaTime(m_DeleteTime);
			writer.Write(m_Lifted, true);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 2:
					{
						m_Guardians = reader.ReadStrongMobileList();
						m_Temporary = reader.ReadBool();

						goto case 1;
					}
				case 1:
					{
						m_Owner = reader.ReadMobile();

						goto case 0;
					}
				case 0:
					{
						m_Level = reader.ReadInt();
						m_DeleteTime = reader.ReadDeltaTime();
						m_Lifted = reader.ReadStrongItemList();

						if (version < 2)
						{
							m_Guardians = new List<Mobile>();
						}

						break;
					}
			}

			if (!m_Temporary)
			{
				m_Timer = new DeleteTimer(this, m_DeleteTime);
				m_Timer.Start();
			}
			else
			{
				Delete();
			}
		}

		public override void OnAfterDelete()
		{
			if (m_Timer != null)
			{
				m_Timer.Stop();
			}

			m_Timer = null;

			base.OnAfterDelete();
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from.Alive)
			{
				list.Add(new RemoveEntry(from, this));
			}
		}

		public void BeginRemove(Mobile from)
		{
			if (!from.Alive)
			{
				return;
			}

			from.CloseGump(typeof(RemoveGump));
			from.SendGump(new RemoveGump(from, this));
		}

		public void EndRemove(Mobile from)
		{
			if (Deleted || from != m_Owner || !from.InRange(GetWorldLocation(), 3))
			{
				return;
			}

			from.SendLocalizedMessage(1048124, "", 0x8A5); // The old, rusted chest crumbles when you hit it.
			Delete();
		}

		private class RemoveGump : Gump
		{
			private readonly Mobile m_From;
			private readonly TreasureMapChest m_Chest;

			public RemoveGump(Mobile from, TreasureMapChest chest) : base(15, 15)
			{
				m_From = from;
				m_Chest = chest;

				Closable = false;
				Disposable = false;

				AddPage(0);

				AddBackground(30, 0, 240, 240, 2620);

				AddHtmlLocalized(45, 15, 200, 80, 1048125, 0x7FFF, false, false); // When this treasure chest is removed, any items still inside of it will be lost.
				AddHtmlLocalized(45, 95, 200, 60, 1048126, 0x7FFF, false, false); // Are you certain you're ready to remove this chest?

				AddButton(40, 153, 4005, 4007, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(75, 155, 180, 40, 1048127, 0x7FFF, false, false); // Remove the Treasure Chest

				AddButton(40, 195, 4005, 4007, 2, GumpButtonType.Reply, 0);
				AddHtmlLocalized(75, 197, 180, 35, 1006045, 0x7FFF, false, false); // Cancel
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (info.ButtonID == 1)
				{
					m_Chest.EndRemove(m_From);
				}
			}
		}

		private class RemoveEntry : ContextMenuEntry
		{
			private readonly Mobile m_From;
			private readonly TreasureMapChest m_Chest;

			public RemoveEntry(Mobile from, TreasureMapChest chest) : base(6149, 3)
			{
				m_From = from;
				m_Chest = chest;

				Enabled = (from == chest.Owner);
			}

			public override void OnClick()
			{
				if (m_Chest.Deleted || m_From != m_Chest.Owner || !m_From.CheckAlive())
				{
					return;
				}

				m_Chest.BeginRemove(m_From);
			}
		}

		private class DeleteTimer : Timer
		{
			private readonly Item m_Item;

			public DeleteTimer(Item item, DateTime time) : base(time - DateTime.UtcNow)
			{
				m_Item = item;
				Priority = TimerPriority.OneMinute;
			}

			protected override void OnTick()
			{
				m_Item.Delete();
			}
		}
	}
}