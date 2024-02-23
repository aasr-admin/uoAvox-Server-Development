using Server.ContextMenus;
using Server.Engines.Craft;
using Server.Engines.Publishing;
using Server.Engines.Stealables;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

using System;
using System.Collections.Generic;

namespace Server.Items
{
	/// BaseContainer
	public abstract class BaseContainer : Container
	{
		public override int DefaultMaxWeight
		{
			get
			{
				if (IsSecure)
				{
					return 0;
				}

				return base.DefaultMaxWeight;
			}
		}

		public BaseContainer(int itemID) : base(itemID)
		{
		}

		public override bool IsAccessibleTo(Mobile m)
		{
			if (!BaseHouse.CheckAccessible(m, this))
			{
				return false;
			}

			return base.IsAccessibleTo(m);
		}

		public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			if (IsSecure && !BaseHouse.CheckHold(m, this, item, message, checkItems, plusItems, plusWeight))
			{
				return false;
			}

			return base.CheckHold(m, item, message, checkItems, plusItems, plusWeight);
		}

		public override bool CheckItemUse(Mobile from, Item item)
		{
			if (IsDecoContainer && item is BaseBook)
			{
				return true;
			}

			return base.CheckItemUse(from, item);
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			SetSecureLevelEntry.AddTo(from, this, list);
		}

		public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
		{
			if (!CheckHold(from, dropped, sendFullMessage, true))
			{
				return false;
			}

			var house = BaseHouse.FindHouseAt(this);

			if (house != null && house.IsLockedDown(this))
			{
				if (dropped is VendorRentalContract || (dropped is Container && ((Container)dropped).FindItemByType(typeof(VendorRentalContract)) != null))
				{
					from.SendLocalizedMessage(1062492); // You cannot place a rental contract in a locked down container.
					return false;
				}

				if (!house.LockDown(from, dropped, false))
				{
					return false;
				}
			}

			var list = Items;

			for (var i = 0; i < list.Count; ++i)
			{
				var item = list[i];

				if (!(item is Container) && item.StackWith(from, dropped, false))
				{
					return true;
				}
			}

			DropItem(dropped);

			return true;
		}

		public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
		{
			if (!CheckHold(from, item, true, true))
			{
				return false;
			}

			var house = BaseHouse.FindHouseAt(this);

			if (house != null && house.IsLockedDown(this))
			{
				if (item is VendorRentalContract || (item is Container && ((Container)item).FindItemByType(typeof(VendorRentalContract)) != null))
				{
					from.SendLocalizedMessage(1062492); // You cannot place a rental contract in a locked down container.
					return false;
				}

				if (!house.LockDown(from, item, false))
				{
					return false;
				}
			}

			item.Location = new Point3D(p.X, p.Y, 0);
			AddItem(item);

			from.SendSound(GetDroppedSound(item), GetWorldLocation());

			return true;
		}

		public override void UpdateTotal(Item sender, TotalType type, int delta)
		{
			base.UpdateTotal(sender, type, delta);

			if (type == TotalType.Weight && RootParent is Mobile)
			{
				((Mobile)RootParent).InvalidateProperties();
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.AccessLevel > AccessLevel.Player || from.InRange(GetWorldLocation(), 2) || RootParent is PlayerVendor)
			{
				Open(from);
			}
			else
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			}
		}

		public virtual void Open(Mobile from)
		{
			DisplayTo(from);
		}

		public BaseContainer(Serial serial) : base(serial)
		{
		}

		/* Note: base class insertion; we cannot serialize anything here */
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
		}
	}

	/// Containers With Opening And Closing Doors
	public class DynamicFurniture
	{
		private static readonly Dictionary<Container, Timer> m_Table = new Dictionary<Container, Timer>();

		public static bool Open(Container c, Mobile m)
		{
			if (m_Table.ContainsKey(c))
			{
				c.SendRemovePacket();
				Close(c);
				c.Delta(ItemDelta.Update);
				c.ProcessDelta();
				return false;
			}

			if (c is Armoire || c is FancyArmoire)
			{
				Timer t = new FurnitureTimer(c, m);
				t.Start();
				m_Table[c] = t;

				switch (c.ItemID)
				{
					case 0xA4D: c.ItemID = 0xA4C; break;
					case 0xA4F: c.ItemID = 0xA4E; break;
					case 0xA51: c.ItemID = 0xA50; break;
					case 0xA53: c.ItemID = 0xA52; break;
				}
			}

			return true;
		}

		public static void Close(Container c)
		{
			Timer t = null;

			m_Table.TryGetValue(c, out t);

			if (t != null)
			{
				t.Stop();
				m_Table.Remove(c);
			}

			if (c is Armoire || c is FancyArmoire)
			{
				switch (c.ItemID)
				{
					case 0xA4C: c.ItemID = 0xA4D; break;
					case 0xA4E: c.ItemID = 0xA4F; break;
					case 0xA50: c.ItemID = 0xA51; break;
					case 0xA52: c.ItemID = 0xA53; break;
				}
			}
		}
	}

	public class FurnitureTimer : Timer
	{
		private readonly Container m_Container;
		private readonly Mobile m_Mobile;

		public FurnitureTimer(Container c, Mobile m) : base(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5))
		{
			Priority = TimerPriority.TwoFiftyMS;

			m_Container = c;
			m_Mobile = m;
		}

		protected override void OnTick()
		{
			if (m_Mobile.Map != m_Container.Map || !m_Mobile.InRange(m_Container.GetWorldLocation(), 3))
			{
				DynamicFurniture.Close(m_Container);
			}
		}
	}

	/// TrapableContainer
	public abstract partial class TrapableContainer : BaseContainer, ITelekinesisable
	{
		private TrapType m_TrapType;
		private int m_TrapPower;
		private int m_TrapLevel;

		[CommandProperty(AccessLevel.GameMaster)]
		public TrapType TrapType
		{
			get => m_TrapType;
			set => m_TrapType = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int TrapPower
		{
			get => m_TrapPower;
			set => m_TrapPower = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int TrapLevel
		{
			get => m_TrapLevel;
			set => m_TrapLevel = value;
		}

		public virtual bool TrapOnOpen => true;

		public TrapableContainer(int itemID) : base(itemID)
		{
		}

		public TrapableContainer(Serial serial) : base(serial)
		{
		}

		private void SendMessageTo(Mobile to, int number, int hue)
		{
			if (Deleted || !to.CanSee(this))
			{
				return;
			}

			to.Send(new Network.MessageLocalized(Serial, ItemID, Network.MessageType.Regular, hue, 3, number, "", ""));
		}

		private void SendMessageTo(Mobile to, string text, int hue)
		{
			if (Deleted || !to.CanSee(this))
			{
				return;
			}

			to.Send(new Network.UnicodeMessage(Serial, ItemID, Network.MessageType.Regular, hue, 3, "ENU", "", text));
		}

		public virtual void OnTelekinesis(Mobile from)
		{
			Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
			Effects.PlaySound(Location, Map, 0x1F5);

			if (TrapOnOpen)
			{
				ExecuteTrap(from);
			}
		}

		public override void Open(Mobile from)
		{
			if (!TrapOnOpen || !ExecuteTrap(from))
			{
				base.Open(from);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(2); // version

			writer.Write(m_TrapLevel);

			writer.Write(m_TrapPower);
			writer.Write((int)m_TrapType);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 2:
					{
						m_TrapLevel = reader.ReadInt();
						goto case 1;
					}
				case 1:
					{
						m_TrapPower = reader.ReadInt();
						goto case 0;
					}
				case 0:
					{
						m_TrapType = (TrapType)reader.ReadInt();
						break;
					}
			}
		}
	}

	/// LockableContainer
	public abstract class LockableContainer : TrapableContainer, ILockable, ILockpickable, ICraftable, IShipwreckedItem
	{
		private bool m_Locked;
		private int m_LockLevel, m_MaxLockLevel, m_RequiredSkill;
		private uint m_KeyValue;
		private Mobile m_Picker;
		private bool m_TrapOnLockpick;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Picker
		{
			get => m_Picker;
			set => m_Picker = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxLockLevel
		{
			get => m_MaxLockLevel;
			set => m_MaxLockLevel = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int LockLevel
		{
			get => m_LockLevel;
			set => m_LockLevel = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int RequiredSkill
		{
			get => m_RequiredSkill;
			set => m_RequiredSkill = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool Locked
		{
			get => m_Locked;
			set
			{
				m_Locked = value;

				if (m_Locked)
				{
					m_Picker = null;
				}

				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public uint KeyValue
		{
			get => m_KeyValue;
			set => m_KeyValue = value;
		}

		public override bool TrapOnOpen => !m_TrapOnLockpick;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool TrapOnLockpick
		{
			get => m_TrapOnLockpick;
			set => m_TrapOnLockpick = value;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(6); // version

			writer.Write(m_IsShipwreckedItem);

			writer.Write(m_TrapOnLockpick);

			writer.Write(m_RequiredSkill);

			writer.Write(m_MaxLockLevel);

			writer.Write(m_KeyValue);
			writer.Write(m_LockLevel);
			writer.Write(m_Locked);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 6:
					{
						m_IsShipwreckedItem = reader.ReadBool();

						goto case 5;
					}
				case 5:
					{
						m_TrapOnLockpick = reader.ReadBool();

						goto case 4;
					}
				case 4:
					{
						m_RequiredSkill = reader.ReadInt();

						goto case 3;
					}
				case 3:
					{
						m_MaxLockLevel = reader.ReadInt();

						goto case 2;
					}
				case 2:
					{
						m_KeyValue = reader.ReadUInt();

						goto case 1;
					}
				case 1:
					{
						m_LockLevel = reader.ReadInt();

						goto case 0;
					}
				case 0:
					{
						if (version < 3)
						{
							m_MaxLockLevel = 100;
						}

						if (version < 4)
						{
							if ((m_MaxLockLevel - m_LockLevel) == 40)
							{
								m_RequiredSkill = m_LockLevel + 6;
								m_LockLevel = m_RequiredSkill - 10;
								m_MaxLockLevel = m_RequiredSkill + 39;
							}
							else
							{
								m_RequiredSkill = m_LockLevel;
							}
						}

						m_Locked = reader.ReadBool();

						break;
					}
			}
		}

		public LockableContainer(int itemID) : base(itemID)
		{
			m_MaxLockLevel = 100;
		}

		public LockableContainer(Serial serial) : base(serial)
		{
		}

		public override bool CheckContentDisplay(Mobile from)
		{
			return !m_Locked && base.CheckContentDisplay(from);
		}

		public override bool TryDropItem(Mobile from, Item dropped, bool sendFullMessage)
		{
			if (from.AccessLevel < AccessLevel.GameMaster && m_Locked)
			{
				from.SendLocalizedMessage(501747); // It appears to be locked.
				return false;
			}

			return base.TryDropItem(from, dropped, sendFullMessage);
		}

		public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
		{
			if (from.AccessLevel < AccessLevel.GameMaster && m_Locked)
			{
				from.SendLocalizedMessage(501747); // It appears to be locked.
				return false;
			}

			return base.OnDragDropInto(from, item, p);
		}

		public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			if (!base.CheckLift(from, item, ref reject))
			{
				return false;
			}

			if (item != this && from.AccessLevel < AccessLevel.GameMaster && m_Locked)
			{
				return false;
			}

			return true;
		}

		public override bool CheckItemUse(Mobile from, Item item)
		{
			if (!base.CheckItemUse(from, item))
			{
				return false;
			}

			if (item != this && from.AccessLevel < AccessLevel.GameMaster && m_Locked)
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
				return false;
			}

			return true;
		}

		public override bool DisplaysContent => !m_Locked;

		public virtual bool CheckLocked(Mobile from)
		{
			var inaccessible = false;

			if (m_Locked)
			{
				int number;

				if (from.AccessLevel >= AccessLevel.GameMaster)
				{
					number = 502502; // That is locked, but you open it with your godly powers.
				}
				else
				{
					number = 501747; // It appears to be locked.
					inaccessible = true;
				}

				from.Send(new MessageLocalized(Serial, ItemID, MessageType.Regular, 0x3B2, 3, number, "", ""));
			}

			return inaccessible;
		}

		public override void OnTelekinesis(Mobile from)
		{
			if (CheckLocked(from))
			{
				Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
				Effects.PlaySound(Location, Map, 0x1F5);
				return;
			}

			base.OnTelekinesis(from);
		}

		public override void OnDoubleClickSecureTrade(Mobile from)
		{
			if (CheckLocked(from))
			{
				return;
			}

			base.OnDoubleClickSecureTrade(from);
		}

		public override void Open(Mobile from)
		{
			if (CheckLocked(from))
			{
				return;
			}

			base.Open(from);
		}

		public override void OnSnoop(Mobile from)
		{
			if (CheckLocked(from))
			{
				return;
			}

			base.OnSnoop(from);
		}

		public virtual void LockPick(Mobile from)
		{
			Locked = false;
			Picker = from;

			if (TrapOnLockpick && ExecuteTrap(from))
			{
				TrapOnLockpick = false;
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

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (m_IsShipwreckedItem)
			{
				LabelTo(from, 1041645); //recovered from a shipwreck
			}
		}

		#region ICraftable Members

		public virtual int OnCraft(int quality, bool makersMark, Mobile from, ICraftSystem craftSystem, Type typeRes, ICraftTool tool, ICraftItem craftItem, int resHue)
		{
			if (from.CheckSkill(SkillName.Tinkering, -5.0, 15.0))
			{
				from.SendLocalizedMessage(500636); // Your tinker skill was sufficient to make the item lockable.

				var key = new Key(KeyType.Copper, Key.RandomValue());

				KeyValue = key.KeyValue;
				DropItem(key);

				var tinkering = from.Skills[SkillName.Tinkering].Value;
				var level = (int)(tinkering * 0.8);

				RequiredSkill = level - 4;
				LockLevel = level - 14;
				MaxLockLevel = level + 35;

				if (LockLevel == 0)
				{
					LockLevel = -1;
				}
				else if (LockLevel > 95)
				{
					LockLevel = 95;
				}

				if (RequiredSkill > 95)
				{
					RequiredSkill = 95;
				}

				if (MaxLockLevel > 95)
				{
					MaxLockLevel = 95;
				}
			}
			else
			{
				from.SendLocalizedMessage(500637); // Your tinker skill was insufficient to make the item lockable.
			}

			return 1;
		}

		#endregion

		#region IShipwreckedItem Members

		private bool m_IsShipwreckedItem;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsShipwreckedItem
		{
			get => m_IsShipwreckedItem;
			set => m_IsShipwreckedItem = value;
		}
		#endregion
	}

	/// FillableContainer
	public enum FillableContentType
	{
		None = -1,
		Alchemist,
		Armorer,
		ArtisanGuild,
		Baker,
		Bard,
		Blacksmith,
		Bowyer,
		Butcher,
		Carpenter,
		Clothier,
		Cobbler,
		Docks,
		Farm,
		FighterGuild,
		FruitBasket,
		Guard,
		Healer,
		Herbalist,
		Inn,
		Jeweler,
		Library,
		Mage,
		Merchant,
		Mill,
		Mine,
		Observatory,
		Painter,
		Provisioner,
		Ranger,
		Stables,
		Tanner,
		Tavern,
		ThiefGuild,
		Tinker,
		Veterinarian,
		Weaponsmith
	}

	public abstract class FillableContainer : LockableContainer
	{
		public virtual int MinRespawnMinutes => 60;
		public virtual int MaxRespawnMinutes => 90;

		public virtual bool IsLockable => true;
		public virtual bool IsTrapable => IsLockable;

		public virtual int SpawnThreshold => 2;

		public virtual FillableContentType DefaultContent => FillableContentType.None;

		protected FillableContent m_Content;

		protected DateTime m_NextRespawnTime;
		protected Timer m_RespawnTimer;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextRespawnTime => m_NextRespawnTime;

		[CommandProperty(AccessLevel.GameMaster)]
		public FillableContentType ContentType
		{
			get
			{
				if (m_Content == null)
				{
					return DefaultContent;
				}

				var content = FillableContent.Lookup(m_Content);

				if (content == FillableContentType.None)
				{
					return DefaultContent;
				}

				return content;
			}
			set => Content = FillableContent.Lookup(value);
		}

		public FillableContent Content
		{
			get => m_Content;
			set
			{
				if (m_Content == value)
				{
					return;
				}

				m_Content = value;

				var items = Items;
				var index = items.Count;

				while (--index >= 0)
				{
					if (index < items.Count)
					{
						items[index].Delete();
					}
				}

				Respawn();
			}
		}

		public FillableContainer(int itemID)
			: base(itemID)
		{
			Movable = false;
		}

		public override void OnMapChange(Map oldMap)
		{
			base.OnMapChange(oldMap);

			AcquireContent();
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			base.OnLocationChange(oldLocation);

			AcquireContent();
		}

		public virtual void AcquireContent()
		{
			if (m_Content?.IsEmpty != false)
			{
				return;
			}

			var type = ContentType;

			if (type != FillableContentType.None)
			{
				m_Content = FillableContent.Lookup(type);
			}
			else
			{
				m_Content = FillableContent.Acquire(GetWorldLocation(), Map);
			}

			if (m_Content != null)
			{
				Respawn();
			}
		}

		public override void OnItemRemoved(Item item)
		{
			CheckRespawn();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_RespawnTimer != null)
			{
				m_RespawnTimer.Stop();
				m_RespawnTimer = null;
			}
		}

		public int GetItemsCount()
		{
			var count = 0;

			foreach (var item in Items)
			{
				count += item.Amount;
			}

			return count;
		}

		public void CheckRespawn()
		{
			var canSpawn = (m_Content != null && !Deleted && GetItemsCount() <= SpawnThreshold && !Movable && Parent == null && !IsLockedDown && !IsSecure);

			if (canSpawn)
			{
				if (m_RespawnTimer == null)
				{
					var mins = Utility.RandomMinMax(MinRespawnMinutes, MaxRespawnMinutes);
					var delay = TimeSpan.FromSeconds(mins);

					m_NextRespawnTime = DateTime.UtcNow + delay;
					m_RespawnTimer = Timer.DelayCall(delay, Respawn);
				}
			}
			else if (m_RespawnTimer != null)
			{
				m_RespawnTimer.Stop();
				m_RespawnTimer = null;
			}
		}

		public void Respawn()
		{
			if (m_RespawnTimer != null)
			{
				m_RespawnTimer.Stop();
				m_RespawnTimer = null;
			}

			if (m_Content == null || Deleted)
			{
				return;
			}

			GenerateContent();

			if (IsLockable)
			{
				Locked = true;

				var difficulty = (m_Content.Level - 1) * 30;

				LockLevel = difficulty - 10;
				MaxLockLevel = difficulty + 30;
				RequiredSkill = difficulty;
			}

			if (IsTrapable && (m_Content.Level > 1 || 4 > Utility.Random(5)))
			{
				if (m_Content.Level > Utility.Random(5))
				{
					TrapType = TrapType.PoisonTrap;
				}
				else
				{
					TrapType = TrapType.ExplosionTrap;
				}

				TrapPower = m_Content.Level * Utility.RandomMinMax(10, 30);
				TrapLevel = m_Content.Level;
			}
			else
			{
				TrapType = TrapType.None;
				TrapPower = 0;
				TrapLevel = 0;
			}

			CheckRespawn();
		}

		protected virtual int GetSpawnCount()
		{
			var itemsCount = GetItemsCount();

			if (itemsCount > SpawnThreshold)
			{
				return 0;
			}

			var maxSpawnCount = (1 + SpawnThreshold - itemsCount) * 2;

			return Utility.RandomMinMax(0, maxSpawnCount);
		}

		public virtual void GenerateContent()
		{
			if (m_Content == null || Deleted)
			{
				return;
			}

			var toSpawn = GetSpawnCount();

			for (var i = 0; i < toSpawn; ++i)
			{
				var item = m_Content.Construct();

				if (item != null)
				{
					var list = Items;

					for (var j = 0; j < list.Count; ++j)
					{
						var subItem = list[j];

						if (!(subItem is Container) && subItem.StackWith(null, item, false))
						{
							break;
						}
					}

					if (item != null && !item.Deleted)
					{
						DropItem(item);
					}
				}
			}
		}

		public FillableContainer(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(1); // version

			writer.Write((int)ContentType);

			if (m_RespawnTimer != null)
			{
				writer.Write(true);
				writer.WriteDeltaTime(m_NextRespawnTime);
			}
			else
			{
				writer.Write(false);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();

			switch (version)
			{
				case 1:
					{
						m_Content = FillableContent.Lookup((FillableContentType)reader.ReadInt());
						goto case 0;
					}
				case 0:
					{
						if (reader.ReadBool())
						{
							m_NextRespawnTime = reader.ReadDeltaTime();

							var delay = m_NextRespawnTime - DateTime.UtcNow;
							m_RespawnTimer = Timer.DelayCall(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, Respawn);
						}
						else
						{
							CheckRespawn();
						}

						break;
					}
			}
		}
	}

	public class FillableEntry
	{
		protected Type[] m_Types;
		protected int m_Weight;

		public Type[] Types => m_Types;
		public int Weight => m_Weight;

		public FillableEntry(Type type)
			: this(1, new Type[] { type })
		{
		}

		public FillableEntry(int weight, Type type)
			: this(weight, new Type[] { type })
		{
		}

		public FillableEntry(Type[] types)
			: this(1, types)
		{
		}

		public FillableEntry(int weight, Type[] types)
		{
			m_Weight = weight;
			m_Types = types;
		}

		public FillableEntry(int weight, Type[] types, int offset, int count)
		{
			m_Weight = weight;
			m_Types = new Type[count];

			for (var i = 0; i < m_Types.Length; ++i)
			{
				m_Types[i] = types[offset + i];
			}
		}

		public virtual Item Construct()
		{
			var item = Loot.Construct(m_Types);

			if (item is Key)
			{
				((Key)item).ItemID = Utility.RandomList((int)KeyType.Copper, (int)KeyType.Gold, (int)KeyType.Iron, (int)KeyType.Rusty);
			}
			else if (item is Arrow || item is Bolt)
			{
				item.Amount = Utility.RandomMinMax(2, 6);
			}
			else if (item is Bandage || item is Lockpick)
			{
				item.Amount = Utility.RandomMinMax(1, 3);
			}

			return item;
		}
	}

	public class FillableBvrge : FillableEntry
	{
		private readonly BeverageType m_Content;

		public BeverageType Content => m_Content;

		public FillableBvrge(Type type, BeverageType content)
			: this(1, type, content)
		{
		}

		public FillableBvrge(int weight, Type type, BeverageType content)
			: base(weight, type)
		{
			m_Content = content;
		}

		public override Item Construct()
		{
			Item item;

			var index = Utility.Random(m_Types.Length);

			if (m_Types[index] == typeof(BeverageBottle))
			{
				item = new BeverageBottle(m_Content);
			}
			else if (m_Types[index] == typeof(Jug))
			{
				item = new Jug(m_Content);
			}
			else
			{
				item = base.Construct();

				if (item is BaseBeverage)
				{
					var bev = (BaseBeverage)item;

					bev.Content = m_Content;
					bev.Quantity = bev.MaxQuantity;
				}
			}

			return item;
		}
	}

	public class FillableContent
	{
		private static readonly SortedDictionary<FillableContentType, FillableContent> m_ContentTypes = new()
		{
			#region Definitions
			[FillableContentType.Alchemist] = new(
				1,
				new[]
				{
					typeof( Alchemist ),
				},
				new FillableEntry(typeof(NightSightPotion)),
				new FillableEntry(typeof(LesserCurePotion)),
				new FillableEntry(typeof(AgilityPotion)),
				new FillableEntry(typeof(StrengthPotion)),
				new FillableEntry(typeof(LesserPoisonPotion)),
				new FillableEntry(typeof(RefreshPotion)),
				new FillableEntry(typeof(LesserHealPotion)),
				new FillableEntry(typeof(LesserExplosionPotion)),
				new FillableEntry(typeof(MortarPestle))
			),

			[FillableContentType.Armorer] = new(
				2,
				new[]
				{
					typeof( Armorer )
				},
				new FillableEntry(2, typeof(ChainCoif)),
				new FillableEntry(1, typeof(PlateGorget)),
				new FillableEntry(1, typeof(BronzeShield)),
				new FillableEntry(1, typeof(Buckler)),
				new FillableEntry(2, typeof(MetalKiteShield)),
				new FillableEntry(2, typeof(HeaterShield)),
				new FillableEntry(1, typeof(WoodenShield)),
				new FillableEntry(1, typeof(MetalShield))
			),

			[FillableContentType.ArtisanGuild] = new(
				1,
				new FillableEntry(1, typeof(PaintsAndBrush)),
				new FillableEntry(1, typeof(SledgeHammer)),
				new FillableEntry(2, typeof(SmithHammer)),
				new FillableEntry(2, typeof(Tongs)),
				new FillableEntry(4, typeof(Lockpick)),
				new FillableEntry(4, typeof(TinkerTools)),
				new FillableEntry(1, typeof(MalletAndChisel)),
				new FillableEntry(1, typeof(StatueEast2)),
				new FillableEntry(1, typeof(StatueSouth)),
				new FillableEntry(1, typeof(StatueSouthEast)),
				new FillableEntry(1, typeof(StatueWest)),
				new FillableEntry(1, typeof(StatueNorth)),
				new FillableEntry(1, typeof(StatueEast)),
				new FillableEntry(1, typeof(BustEast)),
				new FillableEntry(1, typeof(BustSouth)),
				new FillableEntry(1, typeof(BearMask)),
				new FillableEntry(1, typeof(DeerMask)),
				new FillableEntry(4, typeof(OrcHelm)),
				new FillableEntry(1, typeof(TribalMask)),
				new FillableEntry(1, typeof(HornedTribalMask))
			),

			[FillableContentType.Baker] = new(
				1,
				new[]
				{
					typeof( Baker ),
				},
				new FillableEntry(1, typeof(RollingPin)),
				new FillableEntry(2, typeof(SackFlour)),
				new FillableEntry(2, typeof(BreadLoaf)),
				new FillableEntry(1, typeof(FrenchBread))
			),

			[FillableContentType.Bard] = new(
				1,
				new[]
				{
					typeof( Bard ),
					typeof( BardGuildmaster )
				},
				new FillableEntry(1, typeof(LapHarp)),
				new FillableEntry(2, typeof(Lute)),
				new FillableEntry(1, typeof(Drums)),
				new FillableEntry(1, typeof(Tambourine)),
				new FillableEntry(1, typeof(TambourineTassel))
			),

			[FillableContentType.Blacksmith] = new(
				2,
				new[]
				{
					typeof( Blacksmith ),
					typeof( BlacksmithGuildmaster )
				},
				new FillableEntry(8, typeof(SmithHammer)),
				new FillableEntry(8, typeof(Tongs)),
				new FillableEntry(8, typeof(SledgeHammer)),
				//new FillableEntry( 8, typeof( IronOre ) ), // TODO: Smaller ore
				new FillableEntry(8, typeof(IronIngot)),
				new FillableEntry(1, typeof(IronWire)),
				new FillableEntry(1, typeof(SilverWire)),
				new FillableEntry(1, typeof(GoldWire)),
				new FillableEntry(1, typeof(CopperWire)),
				new FillableEntry(1, typeof(HorseShoes)),
				new FillableEntry(1, typeof(ForgedMetal))
			),

			[FillableContentType.Bowyer] = new(
				2,
				new[]
				{
					typeof( Bowyer )
				},
				new FillableEntry(2, typeof(Bow)),
				new FillableEntry(2, typeof(Crossbow)),
				new FillableEntry(1, typeof(Arrow))
			),

			[FillableContentType.Butcher] = new(
				1,
				new[]
				{
					typeof( Butcher ),
				},
				new FillableEntry(2, typeof(Cleaver)),
				new FillableEntry(2, typeof(SlabOfBacon)),
				new FillableEntry(2, typeof(Bacon)),
				new FillableEntry(1, typeof(RawSaltwaterFishSteak)),
				new FillableEntry(1, typeof(SaltwaterFishSteak)),
				new FillableEntry(2, typeof(CookedBird)),
				new FillableEntry(2, typeof(RawBird)),
				new FillableEntry(2, typeof(Ham)),
				new FillableEntry(1, typeof(RawLambLeg)),
				new FillableEntry(1, typeof(LambLeg)),
				new FillableEntry(1, typeof(Ribs)),
				new FillableEntry(1, typeof(RawRibs)),
				new FillableEntry(2, typeof(Sausage)),
				new FillableEntry(1, typeof(RawChickenLeg)),
				new FillableEntry(1, typeof(ChickenLeg))
			),

			[FillableContentType.Carpenter] = new(
				1,
				new[]
				{
					typeof( Carpenter ),
					typeof( Architect ),
					typeof( RealEstateBroker )
				},
				new FillableEntry(1, typeof(ChiselsNorth)),
				new FillableEntry(1, typeof(ChiselsWest)),
				new FillableEntry(2, typeof(DovetailSaw)),
				new FillableEntry(2, typeof(Hammer)),
				new FillableEntry(2, typeof(MouldingPlane)),
				new FillableEntry(2, typeof(Nails)),
				new FillableEntry(2, typeof(JointingPlane)),
				new FillableEntry(2, typeof(SmoothingPlane)),
				new FillableEntry(2, typeof(Saw)),
				new FillableEntry(2, typeof(DrawKnife)),
				new FillableEntry(1, typeof(Log)),
				new FillableEntry(1, typeof(Froe)),
				new FillableEntry(1, typeof(Inshave)),
				new FillableEntry(1, typeof(Scorp))
			),

			[FillableContentType.Clothier] = new(
				1,
				new[]
				{
					typeof( Tailor ),
					typeof( Weaver ),
					typeof( TailorGuildmaster )
				},
				new FillableEntry(1, typeof(Cotton)),
				new FillableEntry(1, typeof(Wool)),
				new FillableEntry(1, typeof(DarkYarn)),
				new FillableEntry(1, typeof(LightYarn)),
				new FillableEntry(1, typeof(LightYarnUnraveled)),
				new FillableEntry(1, typeof(SpoolOfThread)),
				//Four different types
				//new FillableEntry( 1, typeof( FoldedCloth ) ),
				//new FillableEntry( 1, typeof( FoldedCloth ) ),
				//new FillableEntry( 1, typeof( FoldedCloth ) ),
				//new FillableEntry( 1, typeof( FoldedCloth ) ),
				new FillableEntry(1, typeof(Dyes)),
				new FillableEntry(2, typeof(Leather))
			),

			[FillableContentType.Cobbler] = new(
				1,
				new[]
				{
					typeof( Cobbler )
				},
				new FillableEntry(1, typeof(Boots)),
				new FillableEntry(2, typeof(Shoes)),
				new FillableEntry(2, typeof(Sandals)),
				new FillableEntry(1, typeof(ThighBoots))
			),

			[FillableContentType.Docks] = new(
				1,
				new[]
				{
					typeof( Fisherman ),
					typeof( FisherGuildmaster )
				},
				new FillableEntry(1, typeof(FishingPole)),
				//Two different types
				//new FillableEntry( 1, typeof( SmallFish ) ),
				//new FillableEntry( 1, typeof( SmallFish ) ),
				new FillableEntry(4, typeof(SaltwaterFish))
			),

			[FillableContentType.Farm] = new(
				1,
				new[]
				{
					typeof( Farmer ),
					typeof( Rancher )
				},
				new FillableEntry(1, typeof(Shirt)),
				new FillableEntry(1, typeof(ShortPants)),
				new FillableEntry(1, typeof(Skirt)),
				new FillableEntry(1, typeof(PlainDress)),
				new FillableEntry(1, typeof(Cap)),
				new FillableEntry(2, typeof(Sandals)),
				new FillableEntry(2, typeof(GnarledStaff)),
				new FillableEntry(2, typeof(Pitchfork)),
				new FillableEntry(1, typeof(Bag)),
				new FillableEntry(1, typeof(Kindling)),
				new FillableEntry(1, typeof(Lettuce)),
				new FillableEntry(1, typeof(Onion)),
				new FillableEntry(1, typeof(Turnip)),
				new FillableEntry(1, typeof(Ham)),
				new FillableEntry(1, typeof(Bacon)),
				new FillableEntry(1, typeof(RawLambLeg)),
				new FillableEntry(1, typeof(SheafOfHay)),
				new FillableBvrge(1, typeof(Pitcher), BeverageType.Milk)
			),

			[FillableContentType.FighterGuild] = new(
				3,
				new[]
				{
					typeof( WarriorGuildmaster )
				},
				new FillableEntry(12, Loot.ArmorTypes),
				new FillableEntry(8, Loot.WeaponTypes),
				new FillableEntry(3, Loot.ShieldTypes),
				new FillableEntry(1, typeof(Arrow))
			),

			[FillableContentType.FruitBasket] = new(
				1,
				new FillableEntry(typeof(Apple)),
				new FillableEntry(typeof(Pear)),
				new FillableEntry(typeof(Peach)),
				new FillableEntry(typeof(Grapes)),
				new FillableEntry(typeof(Bananas))
			),

			[FillableContentType.Guard] = new(
				3,
				new FillableEntry(12, Loot.ArmorTypes),
				new FillableEntry(8, Loot.WeaponTypes),
				new FillableEntry(3, Loot.ShieldTypes),
				new FillableEntry(1, typeof(Arrow))
			),

			[FillableContentType.Healer] = new(
				1,
				new[]
				{
					typeof( Healer ),
					typeof( HealerGuildmaster )
				},
				new FillableEntry(1, typeof(Bandage)),
				new FillableEntry(1, typeof(MortarPestle)),
				new FillableEntry(1, typeof(LesserHealPotion))
			),

			[FillableContentType.Herbalist] = new(
				1,
				new[]
				{
					typeof( Herbalist )
				},
				new FillableEntry(10, typeof(Garlic)),
				new FillableEntry(10, typeof(Ginseng)),
				new FillableEntry(10, typeof(MandrakeRoot)),
				new FillableEntry(1, typeof(DeadWood)),
				new FillableEntry(1, typeof(WhiteDriedFlowers)),
				new FillableEntry(1, typeof(GreenDriedFlowers)),
				new FillableEntry(1, typeof(DriedOnions)),
				new FillableEntry(1, typeof(DriedHerbs))
			),

			[FillableContentType.Inn] = new(
				1,
				new FillableEntry(1, typeof(Candle)),
				new FillableEntry(1, typeof(Torch)),
				new FillableEntry(1, typeof(Lantern))
			),

			[FillableContentType.Jeweler] = new(
				2,
				new[]
				{
					typeof( Jeweler )
				},
				new FillableEntry(1, typeof(GoldRing)),
				new FillableEntry(1, typeof(GoldBracelet)),
				new FillableEntry(1, typeof(GoldEarrings)),
				new FillableEntry(1, typeof(GoldNecklace)),
				new FillableEntry(1, typeof(GoldBeadNecklace)),
				new FillableEntry(1, typeof(Necklace)),
				new FillableEntry(1, typeof(Beads)),
				new FillableEntry(9, Loot.GemTypes)
			),

			[FillableContentType.Library] = new(
				1,
				new[]
				{
					typeof( Scribe )
				},
				new FillableEntry(8, Loot.LibraryBookTypes),
				new FillableEntry(1, typeof(RedBook)),
				new FillableEntry(1, typeof(BlueBook))
			),

			[FillableContentType.Mage] = new(
				2,
				new[]
				{
					typeof( Mage ),
					typeof( HolyMage ),
					typeof( MageGuildmaster )
				},
				new FillableEntry(16, typeof(BlankScroll)),
				new FillableEntry(14, typeof(Spellbook)),
				new FillableEntry(12, Loot.MageryScrollTypes, 0, 8),
				new FillableEntry(11, Loot.MageryScrollTypes, 8, 8),
				new FillableEntry(10, Loot.MageryScrollTypes, 16, 8),
				new FillableEntry(9, Loot.MageryScrollTypes, 24, 8),
				new FillableEntry(8, Loot.MageryScrollTypes, 32, 8),
				new FillableEntry(7, Loot.MageryScrollTypes, 40, 8),
				new FillableEntry(6, Loot.MageryScrollTypes, 48, 8),
				new FillableEntry(5, Loot.MageryScrollTypes, 56, 8)
			),

			[FillableContentType.Merchant] = new(
				1,
				new[]
				{
					typeof( MerchantGuildmaster )
				},
				new FillableEntry(1, typeof(CheeseWheel)),
				new FillableEntry(1, typeof(CheeseWedge)),
				new FillableEntry(1, typeof(CheeseSlice)),
				new FillableEntry(1, typeof(Eggs)),
				new FillableEntry(4, typeof(SaltwaterFish)),
				new FillableEntry(2, typeof(RawSaltwaterFishSteak)),
				new FillableEntry(2, typeof(SaltwaterFishSteak)),
				new FillableEntry(1, typeof(Apple)),
				new FillableEntry(2, typeof(Banana)),
				new FillableEntry(2, typeof(Bananas)),
				new FillableEntry(2, typeof(OpenCoconut)),
				new FillableEntry(1, typeof(SplitCoconut)),
				new FillableEntry(1, typeof(Coconut)),
				new FillableEntry(1, typeof(Dates)),
				new FillableEntry(1, typeof(Grapes)),
				new FillableEntry(1, typeof(Lemon)),
				new FillableEntry(1, typeof(Lemons)),
				new FillableEntry(1, typeof(Lime)),
				new FillableEntry(1, typeof(Limes)),
				new FillableEntry(1, typeof(Peach)),
				new FillableEntry(1, typeof(Pear)),
				new FillableEntry(2, typeof(SlabOfBacon)),
				new FillableEntry(2, typeof(Bacon)),
				new FillableEntry(2, typeof(CookedBird)),
				new FillableEntry(2, typeof(RawBird)),
				new FillableEntry(2, typeof(Ham)),
				new FillableEntry(1, typeof(RawLambLeg)),
				new FillableEntry(1, typeof(LambLeg)),
				new FillableEntry(1, typeof(Ribs)),
				new FillableEntry(1, typeof(RawRibs)),
				new FillableEntry(2, typeof(Sausage)),
				new FillableEntry(1, typeof(RawChickenLeg)),
				new FillableEntry(1, typeof(ChickenLeg)),
				new FillableEntry(1, typeof(Watermelon)),
				new FillableEntry(1, typeof(SmallWatermelon)),
				new FillableEntry(3, typeof(Turnip)),
				new FillableEntry(2, typeof(YellowGourd)),
				new FillableEntry(2, typeof(GreenGourd)),
				new FillableEntry(2, typeof(Pumpkin)),
				new FillableEntry(1, typeof(SmallPumpkin)),
				new FillableEntry(2, typeof(Onion)),
				new FillableEntry(2, typeof(Lettuce)),
				new FillableEntry(2, typeof(Squash)),
				new FillableEntry(2, typeof(HoneydewMelon)),
				new FillableEntry(1, typeof(Carrot)),
				new FillableEntry(2, typeof(Cantaloupe)),
				new FillableEntry(2, typeof(Cabbage)),
				new FillableEntry(4, typeof(EarOfCorn))
			),

			[FillableContentType.Mill] = new(
				1,
				new FillableEntry(1, typeof(SackFlour))
			),

			[FillableContentType.Mine] = new(
				1,
				new[]
				{
					typeof( Miner )
				},
				new FillableEntry(2, typeof(Pickaxe)),
				new FillableEntry(2, typeof(Shovel)),
				new FillableEntry(2, typeof(IronIngot)),
				//new FillableEntry( 2, typeof( IronOre ) ), // TODO: Smaller Ore
				new FillableEntry(1, typeof(ForgedMetal))
			),

			[FillableContentType.Observatory] = new(
				1,
				new FillableEntry(2, typeof(Sextant)),
				new FillableEntry(2, typeof(Clock)),
				new FillableEntry(1, typeof(Spyglass))
			),

			[FillableContentType.Painter] = new(
				1,
				new FillableEntry(1, typeof(PaintsAndBrush)),
				new FillableEntry(2, typeof(PenAndInk))
			),

			[FillableContentType.Provisioner] = new(
				1,
				new[]
				{
					typeof( Provisioner )
				},
				new FillableEntry(1, typeof(CheeseWheel)),
				new FillableEntry(1, typeof(CheeseWedge)),
				new FillableEntry(1, typeof(CheeseSlice)),
				new FillableEntry(1, typeof(Eggs)),
				new FillableEntry(4, typeof(SaltwaterFish)),
				new FillableEntry(1, typeof(DirtyFrypan)),
				new FillableEntry(1, typeof(DirtyPan)),
				new FillableEntry(1, typeof(DirtyKettle)),
				new FillableEntry(1, typeof(DirtySmallRoundPot)),
				new FillableEntry(1, typeof(DirtyRoundPot)),
				new FillableEntry(1, typeof(DirtySmallPot)),
				new FillableEntry(1, typeof(DirtyPot)),
				new FillableEntry(1, typeof(Apple)),
				new FillableEntry(2, typeof(Banana)),
				new FillableEntry(2, typeof(Bananas)),
				new FillableEntry(2, typeof(OpenCoconut)),
				new FillableEntry(1, typeof(SplitCoconut)),
				new FillableEntry(1, typeof(Coconut)),
				new FillableEntry(1, typeof(Dates)),
				new FillableEntry(1, typeof(Grapes)),
				new FillableEntry(1, typeof(Lemon)),
				new FillableEntry(1, typeof(Lemons)),
				new FillableEntry(1, typeof(Lime)),
				new FillableEntry(1, typeof(Limes)),
				new FillableEntry(1, typeof(Peach)),
				new FillableEntry(1, typeof(Pear)),
				new FillableEntry(2, typeof(SlabOfBacon)),
				new FillableEntry(2, typeof(Bacon)),
				new FillableEntry(1, typeof(RawSaltwaterFishSteak)),
				new FillableEntry(1, typeof(SaltwaterFishSteak)),
				new FillableEntry(2, typeof(CookedBird)),
				new FillableEntry(2, typeof(RawBird)),
				new FillableEntry(2, typeof(Ham)),
				new FillableEntry(1, typeof(RawLambLeg)),
				new FillableEntry(1, typeof(LambLeg)),
				new FillableEntry(1, typeof(Ribs)),
				new FillableEntry(1, typeof(RawRibs)),
				new FillableEntry(2, typeof(Sausage)),
				new FillableEntry(1, typeof(RawChickenLeg)),
				new FillableEntry(1, typeof(ChickenLeg)),
				new FillableEntry(1, typeof(Watermelon)),
				new FillableEntry(1, typeof(SmallWatermelon)),
				new FillableEntry(3, typeof(Turnip)),
				new FillableEntry(2, typeof(YellowGourd)),
				new FillableEntry(2, typeof(GreenGourd)),
				new FillableEntry(2, typeof(Pumpkin)),
				new FillableEntry(1, typeof(SmallPumpkin)),
				new FillableEntry(2, typeof(Onion)),
				new FillableEntry(2, typeof(Lettuce)),
				new FillableEntry(2, typeof(Squash)),
				new FillableEntry(2, typeof(HoneydewMelon)),
				new FillableEntry(1, typeof(Carrot)),
				new FillableEntry(2, typeof(Cantaloupe)),
				new FillableEntry(2, typeof(Cabbage)),
				new FillableEntry(4, typeof(EarOfCorn))
			),

			[FillableContentType.Ranger] = new(
				2,
				new[]
				{
					typeof( Ranger ),
					typeof( RangerGuildmaster )
				},
				new FillableEntry(2, typeof(StuddedChest)),
				new FillableEntry(2, typeof(StuddedLegs)),
				new FillableEntry(2, typeof(StuddedArms)),
				new FillableEntry(2, typeof(StuddedGloves)),
				new FillableEntry(1, typeof(StuddedGorget)),

				new FillableEntry(2, typeof(LeatherChest)),
				new FillableEntry(2, typeof(LeatherLegs)),
				new FillableEntry(2, typeof(LeatherArms)),
				new FillableEntry(2, typeof(LeatherGloves)),
				new FillableEntry(1, typeof(LeatherGorget)),

				new FillableEntry(2, typeof(FeatheredHat)),
				new FillableEntry(1, typeof(CloseHelm)),
				new FillableEntry(1, typeof(TallStrawHat)),
				new FillableEntry(1, typeof(Bandana)),
				new FillableEntry(1, typeof(Cloak)),
				new FillableEntry(2, typeof(Boots)),
				new FillableEntry(2, typeof(ThighBoots)),

				new FillableEntry(2, typeof(GnarledStaff)),
				new FillableEntry(1, typeof(Whip)),

				new FillableEntry(2, typeof(Bow)),
				new FillableEntry(2, typeof(Crossbow)),
				new FillableEntry(2, typeof(HeavyCrossbow)),
				new FillableEntry(4, typeof(Arrow))
			),

			[FillableContentType.Stables] = new(
				1,
				new[]
				{
					typeof( AnimalTrainer ),
					typeof( GypsyAnimalTrainer )
				},
				//new FillableEntry( 1, typeof( Wheat ) ),
				new FillableEntry(1, typeof(Carrot))
			),

			[FillableContentType.Tanner] = new(
				2,
				new[]
				{
					typeof( Tanner ),
					typeof( LeatherWorker ),
					typeof( Furtrader )
				},
				new FillableEntry(1, typeof(FeatheredHat)),
				new FillableEntry(1, typeof(LeatherArms)),
				new FillableEntry(2, typeof(LeatherLegs)),
				new FillableEntry(2, typeof(LeatherChest)),
				new FillableEntry(2, typeof(LeatherGloves)),
				new FillableEntry(1, typeof(LeatherGorget)),
				new FillableEntry(2, typeof(Leather))
			),

			[FillableContentType.Tavern] = new(
				1,
				new[]
				{
					typeof( TavernKeeper ),
					typeof( Barkeeper ),
					typeof( Waiter ),
					typeof( Cook )
				},
				new FillableBvrge(1, typeof(BeverageBottle), BeverageType.Ale),
				new FillableBvrge(1, typeof(BeverageBottle), BeverageType.Wine),
				new FillableBvrge(1, typeof(BeverageBottle), BeverageType.Liquor),
				new FillableBvrge(1, typeof(Jug), BeverageType.Cider)
			),

			[FillableContentType.ThiefGuild] = new(
				1,
				new[]
				{
					typeof( Thief ),
					typeof( ThiefGuildmaster )
				},
				new FillableEntry(1, typeof(Lockpick)),
				new FillableEntry(1, typeof(BearMask)),
				new FillableEntry(1, typeof(DeerMask)),
				new FillableEntry(1, typeof(TribalMask)),
				new FillableEntry(1, typeof(HornedTribalMask)),
				new FillableEntry(4, typeof(OrcHelm))
			),

			[FillableContentType.Tinker] = new(
				1,
				new[]
				{
					typeof( Tinker ),
					typeof( TinkerGuildmaster )
				},
				new FillableEntry(1, typeof(Lockpick)),
				//new FillableEntry( 1, typeof( KeyRing ) ),
				new FillableEntry(2, typeof(Clock)),
				new FillableEntry(2, typeof(ClockParts)),
				new FillableEntry(2, typeof(AxleGears)),
				new FillableEntry(2, typeof(Gears)),
				new FillableEntry(2, typeof(Hinge)),
				//new FillableEntry( 1, typeof( ArrowShafts ) ),
				new FillableEntry(2, typeof(Sextant)),
				new FillableEntry(2, typeof(SextantParts)),
				new FillableEntry(2, typeof(Axle)),
				new FillableEntry(2, typeof(Springs)),
				new FillableEntry(5, typeof(TinkerTools)),
				new FillableEntry(4, typeof(Key)),
				new FillableEntry(1, typeof(DecoArrowShafts)),
				new FillableEntry(1, typeof(Lockpicks)),
				new FillableEntry(1, typeof(ToolKit))
			),

			[FillableContentType.Veterinarian] = new(
				1,
				new[]
				{
					typeof( Veterinarian )
				},
				new FillableEntry(1, typeof(Bandage)),
				new FillableEntry(1, typeof(MortarPestle)),
				new FillableEntry(1, typeof(LesserHealPotion)),
				//new FillableEntry( 1, typeof( Wheat ) ),
				new FillableEntry(1, typeof(Carrot))
			),

			[FillableContentType.Weaponsmith] = new(
				2,
				new[]
				{
					typeof( Weaponsmith )
				},
				new FillableEntry(8, Loot.WeaponTypes),
				new FillableEntry(1, typeof(Arrow))
			),
			#endregion
		};

		private static readonly Dictionary<FillableContent, FillableContentType> m_ReverseLookup = new();
		private static readonly Dictionary<Type, FillableContent> m_AcquireLookup = new();

		public static FillableContent Empty { get; } = new FillableContent(0);

		public static FillableContentType[] ContentTypes { get; }

		static FillableContent()
		{
			foreach (var kv in m_ContentTypes)
			{
				m_ReverseLookup[kv.Value] = kv.Key;

				foreach (var v in kv.Value.m_Vendors)
				{
					m_AcquireLookup[v] = kv.Value;
				}
			}

			var types = Enum.GetValues<FillableContentType>();

			ContentTypes = new FillableContentType[types.Length - 1];

			foreach (var type in types)
			{
				if (type == FillableContentType.None)
				{
					m_ContentTypes[type] = Empty;
					m_ReverseLookup[Empty] = type;
				}
				else
				{
					ContentTypes[(int)type] = type;

					if (!m_ContentTypes.ContainsKey(type))
					{
						Utility.PushColor(ConsoleColor.Yellow);
						Console.WriteLine($"Warning: FillableContent definition missing for '{type}'");
						Utility.PopColor();

						m_ContentTypes[type] = Empty;
					}
				}
			}
		}

		public static FillableContent Lookup(FillableContentType type)
		{
			if (type == FillableContentType.None)
			{
				return Empty;
			}

			if (m_ContentTypes.TryGetValue(type, out var content))
			{
				return content;
			}

			return Empty;
		}

		public static FillableContentType Lookup(FillableContent content)
		{
			if (content == null)
			{
				return FillableContentType.None;
			}

			if (m_ReverseLookup.TryGetValue(content, out var type))
			{
				return type;
			}

			return FillableContentType.None;
		}

		public static FillableContent Acquire(Point3D loc, Map map)
		{
			if (map == null || map == Map.Internal)
			{
				return null;
			}

			Mobile nearest = null;
			FillableContent content = null;

			var eable = map.GetMobilesInRange(loc, 20);

			foreach (var mob in eable)
			{
				if (nearest != null && mob.GetDistanceToSqrt(loc) > nearest.GetDistanceToSqrt(loc))
				{
					if (nearest is not Cobbler || mob is not Provisioner)
					{
						continue;
					}
				}

				if (!m_AcquireLookup.TryGetValue(mob.GetType(), out var check))
				{
					continue;
				}

				if (check?.IsEmpty == false)
				{
					nearest = mob;
					content = check;
				}
			}

			eable.Free();

			return content;
		}

		private readonly int m_Level;
		private readonly Type[] m_Vendors;

		private readonly FillableEntry[] m_Entries;
		private readonly int m_Weight;

		public int Level => m_Level;
		public Type[] Vendors => m_Vendors;

		public FillableContentType TypeID => Lookup(this);

		public bool IsEmpty => m_Entries == null || m_Entries.Length == 0;

		public FillableContent(int level, params FillableEntry[] entries)
			: this(level, Type.EmptyTypes, entries)
		{ }

		public FillableContent(int level, Type[] vendors, params FillableEntry[] entries)
		{
			m_Level = level;
			m_Vendors = vendors;
			m_Entries = entries;

			for (var i = 0; i < entries.Length; ++i)
			{
				m_Weight += entries[i].Weight;
			}
		}

		public virtual Item Construct()
		{
			var index = Utility.Random(m_Weight);

			foreach (var entry in m_Entries)
			{
				if (index < entry.Weight)
				{
					return entry.Construct();
				}

				index -= entry.Weight;
			}

			return null;
		}
	}

	/// BaseBagBall
	public abstract class BaseBagBall : BaseContainer, IDyable
	{
		public BaseBagBall(int itemID) : base(itemID)
		{
			Weight = 1.0;
		}

		public BaseBagBall(Serial serial) : base(serial)
		{
		}

		public bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
			{
				return false;
			}

			Hue = sender.DyedHue;

			return true;
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
}