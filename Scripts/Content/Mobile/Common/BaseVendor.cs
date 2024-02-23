using Server.ContextMenus;
using Server.Engines.BulkOrders;
using Server.Factions;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Quests;
using Server.Regions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
	public interface IShopSellInfo
	{
		//get display name for an item
		string GetNameFor(Item item);

		//get price for an item which the player is selling
		int GetSellPriceFor(Item item);

		//get price for an item which the player is buying
		int GetBuyPriceFor(Item item);

		//can we sell this item to this vendor?
		bool IsSellable(Item item);

		//What do we sell?
		Type[] Types { get; }

		//does the vendor resell this item?
		bool IsResellable(Item item);
	}

	public interface IBuyItemInfo
	{
		//get a new instance of an object (we just bought it)
		IEntity GetEntity();

		int ControlSlots { get; }

		int PriceScalar { get; set; }

		//display price of the item
		int Price { get; }

		//display name of the item
		string Name { get; }

		//display hue
		int Hue { get; }

		//display id
		int ItemID { get; }

		//amount in stock
		int Amount { get; set; }

		//max amount in stock
		int MaxAmount { get; }

		//Attempt to restock with item, (return true if restock sucessful)
		bool Restock(Item item, int amount);

		//called when its time for the whole shop to restock
		void OnRestock();

		IEntity GetDisplayEntity();

		void DeleteDisplayEntity();
	}
}

namespace Server.ContextMenus
{
	public class VendorSellEntry : ContextMenuEntry
	{
		private readonly BaseVendor m_Vendor;

		public VendorSellEntry(Mobile from, BaseVendor vendor)
			: base(6104, 8)
		{
			m_Vendor = vendor;
			Enabled = vendor.CheckVendorAccess(from);
		}

		public override void OnClick()
		{
			m_Vendor.VendorSell(Owner.From);
		}
	}

	public class VendorBuyEntry : ContextMenuEntry
	{
		private readonly BaseVendor m_Vendor;

		public VendorBuyEntry(Mobile from, BaseVendor vendor)
			: base(6103, 8)
		{
			m_Vendor = vendor;
			Enabled = vendor.CheckVendorAccess(from);
		}

		public override void OnClick()
		{
			m_Vendor.VendorBuy(Owner.From);
		}
	}
}

namespace Server.Mobiles
{
	public enum VendorShoeType
	{
		None,
		Shoes,
		Boots,
		Sandals,
		ThighBoots
	}

	public abstract class BaseVendor : BaseCreature, IVendor, IQuestLauncher
	{
		#region Quests

		public virtual Type[] Quests { get; } = Type.EmptyTypes;

		public virtual bool QuestRandomize { get; } = false;

		public virtual void QuestOffered(Quest quest)
		{
		}

		public virtual void QuestAccepted(Quest quest)
		{
		}

		public virtual void QuestDeclined(Quest quest)
		{
		}

		public virtual void QuestCompleted(Quest quest)
		{
		}

		public virtual void QuestRedeemed(Quest quest)
		{
		}

		public virtual void QuestAbandoned(Quest quest)
		{
		}

		public virtual void QuestProgressUpdated(Quest quest, QuestObjective obj)
		{
		}

		#endregion

		public static List<BaseVendor> AllVendors { get; } = new(0x400);

		private static readonly Serial _COFFEE = new(0x7FC0FFEE);

		private const int MaxSell = 500;

		protected abstract List<SBInfo> SBInfos { get; }

		private readonly List<IBuyItemInfo> m_ArmorBuyInfo = new();
		private readonly List<IShopSellInfo> m_ArmorSellInfo = new();

		#region Holiday Season Events
				
		public DateTime NextNaughtyOrNice { get; set; } // Christmas		
		public DateTime NextEggfest { get; set; } // Easter		
		public DateTime NextTrickOrTreat { get; set; } // Halloween		
		public DateTime NextFatherTime { get; set; } // New Year		
		public DateTime NextPotsOGold { get; set; } // Saint Patrick		
		public DateTime NextCupidsKiss { get; set; } // Valentine

		#endregion

		#region Bulk Orders

		public virtual bool IsValidBulkOrder(Item item)
		{
			return false;
		}

		public virtual Item CreateBulkOrder(Mobile from, bool fromContextMenu)
		{
			return null;
		}

		public virtual bool SupportsBulkOrders(Mobile from)
		{
			return false;
		}

		public virtual TimeSpan GetNextBulkOrder(Mobile from)
		{
			return TimeSpan.Zero;
		}

		public virtual void OnSuccessfulBulkOrderReceive(Mobile from)
		{
		}

		private class BulkOrderInfoEntry : ContextMenuEntry
		{
			private readonly Mobile m_From;
			private readonly BaseVendor m_Vendor;

			public BulkOrderInfoEntry(Mobile from, BaseVendor vendor)
				: base(6152)
			{
				m_From = from;
				m_Vendor = vendor;
			}

			public override void OnClick()
			{
				if (m_Vendor.SupportsBulkOrders(m_From))
				{
					var ts = m_Vendor.GetNextBulkOrder(m_From);

					var totalSeconds = (int)ts.TotalSeconds;
					var totalHours = (totalSeconds + 3599) / 3600;
					var totalMinutes = (totalSeconds + 59) / 60;

					if ((Core.SE ? totalMinutes : totalHours) == 0)
					{
						m_From.SendLocalizedMessage(1049038); // You can get an order now.

						if (Core.AOS)
						{
							var bulkOrder = m_Vendor.CreateBulkOrder(m_From, true);

							if (bulkOrder is LargeBOD lbod)
							{
								m_From.SendGump(new LargeBODAcceptGump(m_From, lbod));
							}
							else if (bulkOrder is SmallBOD sbod)
							{
								m_From.SendGump(new SmallBODAcceptGump(m_From, sbod));
							}
						}
					}
					else
					{
						var oldSpeechHue = m_Vendor.SpeechHue;

						m_Vendor.SpeechHue = 0x3B2;

						if (Core.SE)
						{
							m_Vendor.SayTo(m_From, 1072058, totalMinutes.ToString()); // An offer may be available in about ~1_minutes~ minutes.
						}
						else
						{
							m_Vendor.SayTo(m_From, 1049039, totalHours.ToString()); // An offer may be available in about ~1_hours~ hours.
						}

						m_Vendor.SpeechHue = oldSpeechHue;
					}
				}
			}
		}

		#endregion

		private Town m_HomeTown;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Town HomeTown
		{
			get => m_HomeTown;
			set
			{
				m_HomeTown = value;

				InvalidateProperties();
			}
		}

		#region Currencies

		protected readonly Dictionary<Mobile, TypeAmount> m_SelectedCurrencies = new();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Currencies Currencies { get; protected set; } = new();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CurrenciesLocal { get; set; } = true;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CurrenciesInherit { get; set; } = true;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool CurrenciesUseGold { get; set; } = true;

		public IEnumerable<TypeAmount> CurrenciesSupported
		{
			get
			{
				if (CurrenciesInherit)
				{
					var reg = Region;

					while (reg != null)
					{
						if (reg is BaseRegion br && br.Currencies?.Count > 0)
						{
							foreach (var entry in br.Currencies.ActiveEntries)
							{
								if (entry == Currencies.GoldEntry && !CurrenciesUseGold)
								{
									continue;
								}

								yield return entry;
							}
						}

						reg = reg.Parent;
					}
				}

				if (CurrenciesLocal)
				{
					foreach (var entry in Currencies.ActiveEntries)
					{
						if (entry == Currencies.GoldEntry && !CurrenciesUseGold)
						{
							continue;
						}

						yield return entry;
					}
				}
			}
		}

		#endregion

		[CommandProperty(AccessLevel.GameMaster)]
		public AccessLevel VendorAccessLevel { get; set; } = AccessLevel.Player;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public DateTime LastRestock { get; set; }

		public virtual TimeSpan RestockDelay => TimeSpan.FromHours(1);

		public virtual bool IsTokunoVendor => Map?.MapID == 4;

		public override bool ShowFameTitle => false;

		public override bool IsInvulnerable => true;

		public override bool CanTeach => true;

		public override bool BardImmune => true;

		public override bool PlayerRangeSensitive => true;

		public virtual bool IsActiveVendor => true;
		public virtual bool IsActiveBuyer => IsActiveVendor;  // response to vendor SELL
		public virtual bool IsActiveSeller => IsActiveVendor;  // repsonse to vendor BUY

		public virtual NpcGuild NpcGuild => NpcGuild.None;

		public NpcGuildInfo NpcGuildInfo => NpcGuilds.Find(this);

		public Container BuyPack
		{
			get
			{
				if (FindItemOnLayer(Layer.ShopBuy) is not Container pack)
				{
					pack = new Backpack
					{
						Layer = Layer.ShopBuy,
						Visible = false
					};

					AddItem(pack);
				}

				return pack;
			}
		}

		public BaseVendor(string title)
			: base(AIType.AI_Vendor, FightMode.None, 2, 1, 0.5, 2)
		{
			AllVendors.Add(this);

			Title = title;

			LoadSBInfo();

			InitBody();
			InitOutfit();

			AddItem(new Backpack
			{
				Layer = Layer.ShopBuy,
				Movable = false,
				Visible = false
			});

			AddItem(new Backpack
			{
				Layer = Layer.ShopResale,
				Movable = false,
				Visible = false
			});

			LastRestock = DateTime.UtcNow;
		}

		public BaseVendor(Serial serial)
			: base(serial)
		{
			AllVendors.Add(this);
		}

		public abstract void InitSBInfo();

		protected void LoadSBInfo()
		{
			LastRestock = DateTime.UtcNow;

			foreach (var buy in m_ArmorBuyInfo)
			{
				buy?.DeleteDisplayEntity();
			}

			m_ArmorBuyInfo.Clear();
			m_ArmorSellInfo.Clear();

			SBInfos.Clear();

			InitSBInfo();

			for (var i = 0; i < SBInfos.Count; i++)
			{
				var sbInfo = SBInfos[i];

				m_ArmorBuyInfo.AddRange(sbInfo.BuyInfo);
				m_ArmorSellInfo.Add(sbInfo.SellInfo);
			}
		}

		public virtual int GetPriceScalar(Mobile buyer)
		{
			var scalar = 100;

			if (NpcGuild != NpcGuild.None && buyer is PlayerMobile player && player.NpcGuild == NpcGuild)
			{
				scalar -= NpcGuildInfo.VendorDiscount;
			}

			var town = HomeTown ?? Town.FromRegion(Region);

			if (town != null)
			{
				scalar += town.Tax;
			}

			return Math.Max(0, scalar);
		}

		public void UpdateBuyInfo(Mobile buyer)
		{
			var priceScalar = GetPriceScalar(buyer);

			foreach (var info in m_ArmorBuyInfo)
			{
				info.PriceScalar = priceScalar;
			}
		}

		public virtual bool GetGender()
		{
			return Utility.RandomBool();
		}

		public virtual void InitBody()
		{
			InitStats(100, 100, 25);

			SpeechHue = Utility.RandomDyedHue();
			Hue = Utility.RandomSkinHue();

			if (Female = GetGender())
			{
				Body = 0x191;
				Name = NameList.RandomName("female");
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName("male");
			}
		}

		public virtual int GetRandomHue()
		{
			return Utility.Random(5) switch
			{
				1 => Utility.RandomGreenHue(),
				2 => Utility.RandomRedHue(),
				3 => Utility.RandomYellowHue(),
				4 => Utility.RandomNeutralHue(),
				_ => Utility.RandomBlueHue(),
			};
		}

		public virtual int GetShoeHue()
		{
			if (0.1 > Utility.RandomDouble())
			{
				return 0;
			}

			return Utility.RandomNeutralHue();
		}

		public virtual VendorShoeType ShoeType => VendorShoeType.Shoes;

		public virtual void CheckMorph()
		{
			if (CheckGargoyle())
			{
				return;
			}

			if (CheckNecromancer())
			{
				return;
			}

			CheckTokuno();
		}

		public virtual bool CheckTokuno()
		{
			if (Map != Map.Tokuno)
			{
				return false;
			}

			NameList n;

			if (Female)
			{
				n = NameList.GetNameList("tokuno female");
			}
			else
			{
				n = NameList.GetNameList("tokuno male");
			}

			if (!n.ContainsName(Name))
			{
				TurnToTokuno();
			}

			return true;
		}

		public virtual void TurnToTokuno()
		{
			if (Female)
			{
				Name = NameList.RandomName("tokuno female");
			}
			else
			{
				Name = NameList.RandomName("tokuno male");
			}
		}

		public virtual bool CheckGargoyle()
		{
			var map = Map;

			if (map != Map.Ilshenar)
			{
				return false;
			}

			if (!Region.IsPartOf("Gargoyle City"))
			{
				return false;
			}

			if (Body != 0x2F6 || (Hue & 0x8000) == 0)
			{
				TurnToGargoyle();
			}

			return true;
		}

		public virtual bool CheckNecromancer()
		{
			var map = Map;

			if (map != Map.Malas)
			{
				return false;
			}

			if (!Region.IsPartOf("Umbra"))
			{
				return false;
			}

			if (Hue != 0x83E8)
			{
				TurnToNecromancer();
			}

			return true;
		}

		public override void OnAfterSpawn()
		{
			CheckMorph();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			m_SelectedCurrencies.Clear();

			AllVendors.Remove(this);
		}

		protected override void OnMapChange(Map oldMap)
		{
			base.OnMapChange(oldMap);

			CheckMorph();

			LoadSBInfo();
		}

		public virtual int GetRandomNecromancerHue()
		{
			return Utility.Random(20) switch
			{
				0 => 0,
				1 => 0x4E9,
				_ => Utility.RandomList(0x485, 0x497),
			};
		}

		public virtual void TurnToNecromancer()
		{
			for (var i = 0; i < Items.Count; ++i)
			{
				var item = Items[i];

				if (item is Hair || item is Beard)
				{
					item.Hue = 0;
				}
				else if (item is BaseClothing || item is BaseWeapon || item is BaseArmor || item is BaseTool)
				{
					item.Hue = GetRandomNecromancerHue();
				}
			}

			HairHue = 0;
			FacialHairHue = 0;

			Hue = 0x83E8;
		}

		public virtual void TurnToGargoyle()
		{
			for (var i = 0; i < Items.Count; ++i)
			{
				var item = Items[i];

				if (item is BaseClothing || item is Hair || item is Beard)
				{
					item.Delete();
				}
			}

			HairItemID = 0;
			FacialHairItemID = 0;

			Body = 0x2F6;
			Hue = Utility.RandomBrightHue() | 0x8000;
			Name = NameList.RandomName("gargoyle vendor");

			CapitalizeTitle();
		}

		public virtual void CapitalizeTitle()
		{
			var title = Title;

			if (title == null)
			{
				return;
			}

			var split = title.Split(' ');

			for (var i = 0; i < split.Length; ++i)
			{
				if (Insensitive.Equals(split[i], "the"))
				{
					continue;
				}

				if (split[i].Length > 1)
				{
					split[i] = Char.ToUpper(split[i][0]) + split[i].Substring(1);
				}
				else if (split[i].Length > 0)
				{
					split[i] = Char.ToUpper(split[i][0]).ToString();
				}
			}

			Title = String.Join(" ", split);
		}

		public virtual int GetHairHue()
		{
			return Utility.RandomHairHue();
		}

		public virtual void InitOutfit()
		{
			switch (Utility.Random(3))
			{
				case 0: AddItem(new FancyShirt(GetRandomHue())); break;
				case 1: AddItem(new Doublet(GetRandomHue())); break;
				case 2: AddItem(new Shirt(GetRandomHue())); break;
			}

			switch (ShoeType)
			{
				case VendorShoeType.Shoes: AddItem(new Shoes(GetShoeHue())); break;
				case VendorShoeType.Boots: AddItem(new Boots(GetShoeHue())); break;
				case VendorShoeType.Sandals: AddItem(new Sandals(GetShoeHue())); break;
				case VendorShoeType.ThighBoots: AddItem(new ThighBoots(GetShoeHue())); break;
			}

			var hairHue = GetHairHue();

			Utility.AssignRandomHair(this, hairHue);
			Utility.AssignRandomFacialHair(this, hairHue);

			if (Female)
			{
				switch (Utility.Random(6))
				{
					case 0: AddItem(new ShortPants(GetRandomHue())); break;
					case 1:
					case 2: AddItem(new Kilt(GetRandomHue())); break;
					case 3:
					case 4:
					case 5: AddItem(new Skirt(GetRandomHue())); break;
				}
			}
			else
			{
				switch (Utility.Random(2))
				{
					case 0: AddItem(new LongPants(GetRandomHue())); break;
					case 1: AddItem(new ShortPants(GetRandomHue())); break;
				}
			}

			PackGold(100, 200);
		}

		public virtual void Restock()
		{
			LastRestock = DateTime.UtcNow;

			var buyInfo = GetBuyInfo();

			foreach (var bii in buyInfo)
			{
				bii.OnRestock();
			}
		}

		private static readonly TimeSpan InventoryDecayTime = TimeSpan.FromHours(1.0);

		public virtual bool AllowBuyer(Mobile buyer, bool message)
		{
			if (!IsActiveSeller)
			{
				return false;
			}

			if (!buyer.CheckAlive(message))
			{
				return false;
			}

			if (!CheckVendorAccess(buyer))
			{
				if (message)
				{
					Say(501522); // I shall not treat with scum like thee!
				}

				return false;
			}

			return true;
		}

		public void VendorBuy(Mobile from)
		{
			if (!AllowBuyer(from, true))
			{
				return;
			}

			if (from.Player)
			{
				var gump = CurrencyUtility.BeginSelectCurrency(from, CurrenciesUseGold, CurrenciesSupported, (p, t) =>
				{
					m_SelectedCurrencies[from] = t;

					OnVendorBuy(from);
				});

				if (gump != null)
				{
					return;
				}
			}

			OnVendorBuy(from);
		}

		protected virtual void OnVendorBuy(Mobile from)
		{
			if (!AllowBuyer(from, true))
			{
				m_SelectedCurrencies.Remove(from);
				return;
			}

			if (m_SelectedCurrencies.TryGetValue(from, out var currency))
			{
			}
			else if (CurrenciesUseGold)
			{
				currency = Currencies.GoldEntry;
			}
			else
			{
				return;
			}

			if (DateTime.UtcNow - LastRestock > RestockDelay)
			{
				Restock();
			}

			UpdateBuyInfo(from);

			var count = 0;
			List<BuyItemState> list;
			var buyInfo = GetBuyInfo();
			var sellInfo = GetSellInfo();

			list = new List<BuyItemState>(buyInfo.Length);
			var cont = BuyPack;

			List<ObjectPropertyList> opls = null;

			for (var idx = 0; idx < buyInfo.Length; idx++)
			{
				var buyItem = buyInfo[idx];

				if (buyItem.Amount <= 0 || list.Count >= 250)
				{
					continue;
				}

				var disp = buyItem.GetDisplayEntity();

				if (!CurrencyUtility.ConvertGoldToCurrency(buyItem.Price, currency, out var price))
				{
					continue;
				}

				list.Add(new BuyItemState(buyItem.Name, cont.Serial, disp == null ? _COFFEE : disp.Serial, price, buyItem.Amount, buyItem.ItemID, buyItem.Hue));
				
				++count;

				opls ??= new List<ObjectPropertyList>();
				
				if (disp is Item item)
				{
					opls.Add(item.PropertyList);
				}
				else if (disp is Mobile mobile)
				{
					opls.Add(mobile.PropertyList);
				}
			}

			var playerItems = cont.Items;

			for (var i = playerItems.Count - 1; i >= 0; --i)
			{
				if (i >= playerItems.Count)
				{
					continue;
				}

				var item = playerItems[i];

				if (item.LastMoved + InventoryDecayTime <= DateTime.UtcNow)
				{
					item.Delete();
				}
			}

			for (var i = 0; i < playerItems.Count; ++i)
			{
				var item = playerItems[i];

				var price = 0;
				string name = null;

				foreach (var ssi in sellInfo)
				{
					if (ssi.IsSellable(item))
					{
						price = ssi.GetBuyPriceFor(item);
						name = ssi.GetNameFor(item);
						break;
					}
				}

				if (name != null && list.Count < 250)
				{
					if (!CurrencyUtility.ConvertGoldToCurrency(price, currency, out price))
					{
						continue;
					}

					list.Add(new BuyItemState(name, cont.Serial, item.Serial, price, item.Amount, item.ItemID, item.Hue));

					++count;

					opls ??= new List<ObjectPropertyList>();
					
					opls.Add(item.PropertyList);
				}
			}

			if (list.Count > 0)
			{
				list.Sort(BuyItemStateComparer.Instance);

				SendPacksTo(from);

				var ns = from.NetState;

				if (ns == null)
				{
					return;
				}

				if (ns.ContainerGridLines)
				{
					from.Send(new VendorBuyContent6017(list));
				}
				else
				{
					from.Send(new VendorBuyContent(list));
				}

				from.Send(new VendorBuyList(this, list));

				if (ns.HighSeas)
				{
					from.Send(new DisplayBuyListHS(this));
				}
				else
				{
					from.Send(new DisplayBuyList(this));
				}

				from.Send(new MobileStatusExtended(from));//make sure their gold amount is sent

				if (opls != null)
				{
					for (var i = 0; i < opls.Count; ++i)
					{
						from.Send(opls[i]);
					}
				}

				SayTo(from, 500186); // Greetings.  Have a look around.
			}
		}

		public virtual void SendPacksTo(Mobile from)
		{
			var pack = FindItemOnLayer(Layer.ShopBuy);

			if (pack == null)
			{
				pack = new Backpack 
				{
					Layer = Layer.ShopBuy,
					Movable = false,
					Visible = false
				};

				AddItem(pack);
			}

			from.Send(new EquipUpdate(pack));

			pack = FindItemOnLayer(Layer.ShopSell);

			if (pack != null)
			{
				from.Send(new EquipUpdate(pack));
			}

			pack = FindItemOnLayer(Layer.ShopResale);

			if (pack == null)
			{
				pack = new Backpack 
				{
					Layer = Layer.ShopResale,
					Movable = false,
					Visible = false
				};

				AddItem(pack);
			}

			from.Send(new EquipUpdate(pack));
		}

		public virtual bool AllowSeller(Mobile seller, bool message)
		{
			if (!IsActiveBuyer)
			{
				return false;
			}

			if (!seller.CheckAlive(message))
			{
				return false;
			}

			if (!CheckVendorAccess(seller))
			{
				if (message)
				{
					Say(501522); // I shall not treat with scum like thee!
				}

				return false;
			}

			return true;
		}

		public void VendorSell(Mobile from)
		{
			if (!AllowSeller(from, true))
			{
				return;
			}

			if (from.Player)
			{
				var gump = CurrencyUtility.BeginSelectCurrency(from, CurrenciesUseGold, CurrenciesSupported, (p, t) =>
				{
					m_SelectedCurrencies[from] = t;

					OnVendorSell(from);
				});

				if (gump != null)
				{
					return;
				}
			}

			OnVendorSell(from);
		}

		protected virtual void OnVendorSell(Mobile from)
		{
			if (!AllowSeller(from, true))
			{
				m_SelectedCurrencies.Remove(from);
				return;
			}

			if (m_SelectedCurrencies.TryGetValue(from, out var currency))
			{
			}
			else if (CurrenciesUseGold)
			{
				currency = Currencies.GoldEntry;
			}
			else
			{
				m_SelectedCurrencies.Remove(from);
				return;
			}

			var pack = from.Backpack;

			if (pack != null)
			{
				var info = GetSellInfo();

				var table = new List<SellItemState>();

				foreach (var ssi in info)
				{
					var items = pack.FindItemsByType(ssi.Types);

					foreach (var item in items)
					{
						if (item is Container c && c.Items.Count != 0)
						{
							continue;
						}

						if (item.Parent is LockableContainer lockable && lockable.Locked)
						{
							continue;
						}

						if (item.Movable && item.IsStandardLoot() && ssi.IsSellable(item))
						{
							if (CurrencyUtility.ConvertGoldToCurrency(ssi.GetSellPriceFor(item), currency, out var price))
							{
								table.Add(new SellItemState(item, price, ssi.GetNameFor(item)));
							}
						}
					}
				}

				if (table.Count > 0)
				{
					SendPacksTo(from);

					from.Send(new VendorSellList(this, table));
				}
				else
				{
					Say(true, "You have nothing I would be interested in.");
				}
			}
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			/* TODO: Thou art giving me? and fame/karma for gold gifts */

			if (dropped is SmallBOD || dropped is LargeBOD)
			{
				var pm = from as PlayerMobile;

				if (Core.ML && pm != null && pm.NextBODTurnInTime > DateTime.UtcNow)
				{
					SayTo(from, 1079976); // You'll have to wait a few seconds while I inspect the last order.
					return false;
				}
				else if (!IsValidBulkOrder(dropped))
				{
					SayTo(from, 1045130); // That order is for some other shopkeeper.
					return false;
				}
				else if ((dropped is SmallBOD sb && !sb.Complete) || (dropped is LargeBOD lb && !lb.Complete))
				{
					SayTo(from, 1045131); // You have not completed the order yet.
					return false;
				}

				Item reward = null;
				int gold = 0, fame = 0;

				if (dropped is SmallBOD sbod)
				{
					sbod.GetRewards(out reward, out gold, out fame);
				}
				else if(dropped is LargeBOD lbod)
				{
					lbod.GetRewards(out reward, out gold, out fame);
				}

				from.SendSound(0x3D);

				SayTo(from, 1045132); // Thank you so much!  Here is a reward for your effort.

				if (reward != null)
				{
					from.AddToBackpack(reward);
				}

				if (gold > 1000)
				{
					from.AddToBackpack(new BankCheck(gold));
				}
				else if (gold > 0)
				{
					from.AddToBackpack(new Gold(gold));
				}

				Titles.AwardFame(from, fame, true);

				OnSuccessfulBulkOrderReceive(from);

				if (Core.ML && pm != null)
				{
					pm.NextBODTurnInTime = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
				}

				dropped.Delete();
				return true;
			}

			return base.OnDragDrop(from, dropped);
		}

		private GenericBuyInfo LookupDisplayObject(object obj)
		{
			var buyInfo = GetBuyInfo();

			for (var i = 0; i < buyInfo.Length; ++i)
			{
				var gbi = (GenericBuyInfo)buyInfo[i];

				if (gbi.GetDisplayEntity() == obj)
				{
					return gbi;
				}
			}

			return null;
		}

		private static void ProcessSinglePurchase(BuyItemResponse buy, IBuyItemInfo bii, List<BuyItemResponse> validBuy, ref int controlSlots, ref bool fullPurchase, ref int totalCost)
		{
			var amount = buy.Amount;

			if (amount > bii.Amount)
			{
				amount = bii.Amount;
			}

			if (amount <= 0)
			{
				return;
			}

			var slots = bii.ControlSlots * amount;

			if (controlSlots >= slots)
			{
				controlSlots -= slots;
			}
			else
			{
				fullPurchase = false;
				return;
			}

			totalCost += bii.Price * amount;
			validBuy.Add(buy);
		}

		private void ProcessValidPurchase(int amount, IBuyItemInfo bii, Mobile buyer, Container cont)
		{
			if (amount > bii.Amount)
			{
				amount = bii.Amount;
			}

			if (amount < 1)
			{
				return;
			}

			bii.Amount -= amount;

			var o = bii.GetEntity();

			if (o is Item item)
			{
				if (item.Stackable)
				{
					item.Amount = amount;

					if (cont == null || !cont.TryDropItem(buyer, item, false))
					{
						item.MoveToWorld(buyer.Location, buyer.Map);
					}

					EventSink.InvokeBuyFromVendor(new BuyFromVendorEventArgs(buyer, this, item, amount));
				}
				else
				{
					item.Amount = 1;

					if (cont == null || !cont.TryDropItem(buyer, item, false))
					{
						item.MoveToWorld(buyer.Location, buyer.Map);
					}

					EventSink.InvokeBuyFromVendor(new BuyFromVendorEventArgs(buyer, this, item, 1));

					for (var i = 1; i < amount; i++)
					{
						if (bii.GetEntity() is Item obj)
						{
							obj.Amount = 1;

							if (cont == null || !cont.TryDropItem(buyer, obj, false))
							{
								obj.MoveToWorld(buyer.Location, buyer.Map);
							}

							EventSink.InvokeBuyFromVendor(new BuyFromVendorEventArgs(buyer, this, item, 1));
						}
					}
				}
			}
			else if (o is Mobile mobile)
			{
				mobile.Direction = (Direction)Utility.Random(8);
				mobile.MoveToWorld(buyer.Location, buyer.Map);
				mobile.PlaySound(mobile.GetIdleSound());

				if (mobile is BaseCreature creature)
				{
					creature.SetControlMaster(buyer);
					creature.ControlOrder = OrderType.Stop;
				}

				EventSink.InvokeBuyFromVendor(new BuyFromVendorEventArgs(buyer, this, mobile, 1));

				for (var i = 1; i < amount; ++i)
				{
					if (bii.GetEntity() is Mobile obj)
					{
						obj.Direction = (Direction)Utility.Random(8);
						obj.MoveToWorld(buyer.Location, buyer.Map);
						obj.PlaySound(obj.GetIdleSound());

						if (obj is BaseCreature pet)
						{
							pet.SetControlMaster(buyer);
							pet.ControlOrder = OrderType.Stop;
						}

						EventSink.InvokeBuyFromVendor(new BuyFromVendorEventArgs(buyer, this, obj, 1));
					}
				}
			}
		}

		public virtual bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
		{
			if (m_SelectedCurrencies.TryGetValue(buyer, out var currency))
			{
				m_SelectedCurrencies.Remove(buyer);
			}
			else if (CurrenciesUseGold)
			{
				currency = Currencies.GoldEntry;
			}
			else
			{
				return false;
			}

			if (!AllowBuyer(buyer, true))
			{
				return false;
			}

			UpdateBuyInfo(buyer);

			var info = GetSellInfo();

			var totalCost = 0;
			var validBuy = new List<BuyItemResponse>(list.Count);

			Container cont;
			bool bought;

			var fromBank = false;
			var fullPurchase = true;

			var controlSlots = buyer.FollowersMax - buyer.Followers;

			foreach (var buy in list)
			{
				var ser = buy.Serial;
				var amount = buy.Amount;

				if (ser.IsItem)
				{
					var item = World.FindItem(ser);

					if (item == null)
					{
						continue;
					}

					var gbi = LookupDisplayObject(item);

					if (gbi != null)
					{
						ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref totalCost);
					}
					else if (item != BuyPack && item.IsChildOf(BuyPack))
					{
						if (amount > item.Amount)
						{
							amount = item.Amount;
						}

						if (amount <= 0)
						{
							continue;
						}

						foreach (var ssi in info)
						{
							if (ssi.IsSellable(item) && ssi.IsResellable(item))
							{
								if (CurrencyUtility.ConvertGoldToCurrency(ssi.GetBuyPriceFor(item), currency, out var price))
								{
									totalCost += price * amount;

									validBuy.Add(buy);
								}

								break;
							}
						}
					}
				}
				else if (ser.IsMobile)
				{
					var mob = World.FindMobile(ser);

					if (mob == null)
					{
						continue;
					}

					var gbi = LookupDisplayObject(mob);

					if (gbi != null)
					{
						ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref totalCost);
					}
				}
			}

			if (fullPurchase && validBuy.Count == 0)
			{
				SayTo(buyer, 500190); // Thou hast bought nothing!
			}
			else if (validBuy.Count == 0)
			{
				SayTo(buyer, 500187); // Your order cannot be fulfilled, please try again.
			}

			if (validBuy.Count == 0)
			{
				return false;
			}

			bought = buyer.AccessLevel >= AccessLevel.GameMaster;
			cont = buyer.Backpack;

			if (!bought && cont != null)
			{
				bought = cont.ConsumeTotal(currency, totalCost);
			}

			if (!bought && CurrencyUtility.ConvertGoldToCurrency(2000, currency, out var petty) && totalCost > petty)
			{
				fromBank = true;

				bought = CurrencyUtility.WithdrawFromBank(buyer, currency, totalCost, false);
			}

			if (!bought)
			{
				if (fromBank)
				{
					SayTo(buyer, 500191); // Begging thy pardon, but thy bank account lacks these funds.
				}
				else
				{
					SayTo(buyer, 500192); // Begging thy pardon, but thou canst not afford that.
				}

				return false;
			}
			
			buyer.PlaySound(0x32);

			cont = buyer.Backpack ?? buyer.BankBox;

			foreach (var buy in validBuy)
			{
				var ser = buy.Serial;
				var amount = buy.Amount;

				if (amount < 1)
				{
					continue;
				}

				if (ser.IsItem)
				{
					var item = World.FindItem(ser);

					if (item == null)
					{
						continue;
					}

					var gbi = LookupDisplayObject(item);

					if (gbi != null)
					{
						ProcessValidPurchase(amount, gbi, buyer, cont);
						continue;
					}

					if (amount > item.Amount)
					{
						amount = item.Amount;
					}

					foreach (var ssi in info)
					{
						if (ssi.IsSellable(item) && ssi.IsResellable(item))
						{
							Item buyItem;

							if (amount >= item.Amount)
							{
								buyItem = item;
							}
							else
							{
								buyItem = LiftItemDupe(item, item.Amount - amount);

								buyItem ??= item;
							}

							EventSink.InvokeBuyFromVendor(new BuyFromVendorEventArgs(buyer, this, buyItem, amount));

							if (cont == null || !cont.TryDropItem(buyer, buyItem, false))
							{
								buyItem.MoveToWorld(buyer.Location, buyer.Map);
							}

							break;
						}
					}
				}
				else if (ser.IsMobile)
				{
					var mob = World.FindMobile(ser);

					if (mob == null)
					{
						continue;
					}

					var gbi = LookupDisplayObject(mob);

					if (gbi != null)
					{
						ProcessValidPurchase(amount, gbi, buyer, cont);
					}
				}
			}

			var charged = $"{totalCost:N0} {Utility.FriendlyName(currency)}";

			if (fullPurchase)
			{
				if (buyer.AccessLevel >= AccessLevel.GameMaster)
				{
					SayTo(buyer, true, $"I would not presume to charge thee anything, master {buyer.RawName}.  Here are the goods you requested.");
				}
				else if (fromBank)
				{
					SayTo(buyer, true, $"The total of your purchase is {charged}, which has been drawn from your bank account.  My thanks for the patronage.");
				}
				else
				{
					SayTo(buyer, true, $"The total of your purchase is {charged}.  My thanks for the patronage.");
				}
			}
			else
			{
				if (buyer.AccessLevel >= AccessLevel.GameMaster)
				{
					SayTo(buyer, true, $"I would not presume to charge thee anything, master {buyer.RawName}.  Unfortunately, I could not sell you all the goods you requested.");
				}
				else if (fromBank)
				{
					SayTo(buyer, true, $"The total of thy purchase is {charged}, which has been withdrawn from your bank account.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.");
				}
				else
				{
					SayTo(buyer, true, $"The total of thy purchase is {charged}.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.");
				}
			}

			return true;
		}

		public virtual bool OnSellItems(Mobile seller, List<SellItemResponse> list)
		{
			if (m_SelectedCurrencies.TryGetValue(seller, out var currency))
			{
				m_SelectedCurrencies.Remove(seller);
			}
			else if (CurrenciesUseGold)
			{
				currency = Currencies.GoldEntry;
			}
			else
			{
				return false;
			}

			if (!AllowSeller(seller, true))
			{
				return false;
			}

			seller.PlaySound(0x32);

			var info = GetSellInfo();
			var buyInfo = GetBuyInfo();
			var give = 0;
			var sold = 0;
			Container cont;

			foreach (var resp in list)
			{
				if (resp.Item.RootParent != seller || resp.Amount <= 0 || !resp.Item.IsStandardLoot() || !resp.Item.Movable || (resp.Item is Container c && c.Items.Count != 0))
				{
					continue;
				}

				foreach (var ssi in info)
				{
					if (!ssi.IsSellable(resp.Item))
					{
						continue;
					}

					var amount = Math.Min(resp.Amount, resp.Item.Amount);

					if (amount <= 0)
					{
						continue;
					}

					if (!CurrencyUtility.ConvertGoldToCurrency(ssi.GetSellPriceFor(resp.Item), currency, out _))
					{
						continue;
					}

					sold++;
					break;
				}
			}

			if (sold > MaxSell)
			{
				SayTo(seller, true, $"You may only sell {MaxSell:N0} items at a time!");
				return false;
			}

			if (sold == 0)
			{
				return true;
			}

			var index = list.Count;

			while (--index >= 0)
			{
				if (index >= list.Count)
				{
					continue;
				}

				var resp = list[index];

				if (resp.Item.RootParent != seller || resp.Amount <= 0 || !resp.Item.Movable || !resp.Item.IsStandardLoot() || (resp.Item is Container c && c.Items.Count != 0))
				{
					list.RemoveAt(index);
					continue;
				}

				foreach (var ssi in info)
				{
					if (!ssi.IsSellable(resp.Item))
					{
						list.RemoveAt(index);
						continue;
					}

					var amount = Math.Min(resp.Amount, resp.Item.Amount);

					if (amount <= 0)
					{
						list.RemoveAt(index);
						continue;
					}

					if (!CurrencyUtility.ConvertGoldToCurrency(ssi.GetSellPriceFor(resp.Item), currency, out var worth))
					{
						list.RemoveAt(index);
						continue;
					}

					give += worth * amount;
					break;
				}
			}

			if (give > 0 && CurrencyUtility.DepositToBank(seller, currency, give, true))
			{
				seller.PlaySound(0x0037); //Gold dropping sound

				foreach (var resp in list)
				{
					foreach (var ssi in info)
					{
						var amount = Math.Min(resp.Amount, resp.Item.Amount);

						if (ssi.IsResellable(resp.Item))
						{
							var found = false;

							foreach (var bii in buyInfo)
							{
								if (bii.Restock(resp.Item, amount))
								{
									resp.Item.Consume(amount);

									EventSink.InvokeSellToVendor(new SellToVendorEventArgs(seller, this, resp.Item, amount));

									found = true;

									break;
								}
							}

							if (!found)
							{
								cont = BuyPack;

								if (amount < resp.Item.Amount)
								{
									var item = LiftItemDupe(resp.Item, resp.Item.Amount - amount);

									if (item != null)
									{
										item.SetLastMoved();
										cont.DropItem(item);

										EventSink.InvokeSellToVendor(new SellToVendorEventArgs(seller, this, item, amount));
									}
									else
									{
										resp.Item.SetLastMoved();
										cont.DropItem(resp.Item);

										EventSink.InvokeSellToVendor(new SellToVendorEventArgs(seller, this, resp.Item, amount));
									}
								}
								else
								{
									resp.Item.SetLastMoved();
									cont.DropItem(resp.Item);

									EventSink.InvokeSellToVendor(new SellToVendorEventArgs(seller, this, resp.Item, amount));
								}
							}
						}
						else
						{
							resp.Item.Consume(amount);

							EventSink.InvokeSellToVendor(new SellToVendorEventArgs(seller, this, resp.Item, amount));
						}

						break;
					}
				}

				if (SupportsBulkOrders(seller))
				{
					var bulkOrder = CreateBulkOrder(seller, false);

					if (bulkOrder is LargeBOD lbod)
					{
						seller.SendGump(new LargeBODAcceptGump(seller, lbod));
					}
					else if (bulkOrder is SmallBOD sbod)
					{
						seller.SendGump(new SmallBODAcceptGump(seller, sbod));
					}
				}
			}

			return true;
		}

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			if (from.Alive && IsActiveVendor)
			{
				if (SupportsBulkOrders(from))
				{
					list.Add(new BulkOrderInfoEntry(from, this));
				}

				if (IsActiveSeller)
				{
					list.Add(new VendorBuyEntry(from, this));
				}

				if (IsActiveBuyer)
				{
					list.Add(new VendorSellEntry(from, this));
				}
			}

			base.AddCustomContextEntries(from, list);
		}

		public virtual bool CheckVendorAccess(Mobile from)
		{
			if (from.AccessLevel < VendorAccessLevel)
			{
				SayTo(from, true, "My apologies, I cannot trade with you at this time");

				return false;
			}

			var reg = Region.GetRegion<GuardedRegion>();

			if (reg != null && !reg.CheckVendorAccess(this, from))
			{
				return false;
			}

			if (Region != from.Region)
			{
				reg = from.Region.GetRegion<GuardedRegion>();

				if (reg != null && !reg.CheckVendorAccess(this, from))
				{
					return false;
				}
			}

			if (Reputation.IsEnemy(this, from))
			{
				return false;
			}

			return true;
		}

		public virtual IShopSellInfo[] GetSellInfo()
		{
			return m_ArmorSellInfo.ToArray();
		}

		public virtual IBuyItemInfo[] GetBuyInfo()
		{
			return m_ArmorBuyInfo.ToArray();
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);

			if (Quests?.Length > 0)
			{
				list.Add(1060847, $"Quest\tGiver");
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.Write(VendorAccessLevel);

			Town.WriteReference(writer, m_HomeTown);

			SerializeCurrencies(writer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadEncodedInt(); // version

			VendorAccessLevel = reader.ReadEnum<AccessLevel>();

			m_HomeTown = Town.ReadReference(reader);

			DeserializeCurrencies(reader);

			LoadSBInfo();
		}

		protected virtual void SerializeCurrencies(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			Currencies.Serialize(writer);

			var cFlags = 0U;

			if (CurrenciesLocal)
			{
				cFlags |= 1U << 0;
			}

			if (CurrenciesInherit)
			{
				cFlags |= 1U << 1;
			}

			if (CurrenciesUseGold)
			{
				cFlags |= 1U << 2;
			}

			writer.WriteEncodedUInt(cFlags);
		}

		protected virtual void DeserializeCurrencies(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();

			Currencies.Deserialize(reader);

			var cFlags = reader.ReadEncodedUInt();

			CurrenciesLocal = (cFlags & (1U << 0)) != 0;
			CurrenciesInherit = (cFlags & (1U << 1)) != 0;
			CurrenciesUseGold = (cFlags & (1U << 2)) != 0;
		}
	}

	public class VendorInventory
	{
		public static readonly TimeSpan GracePeriod = TimeSpan.FromDays(7.0);

		public BaseHouse House { get; set; }

		public string VendorName { get; set; }

		public string ShopName { get; set; }

		public Mobile Owner { get; set; }

		public int Gold { get; set; }

		public DateTime ExpireTime { get; }

		public List<Item> Items { get; }

		private readonly Timer m_ExpireTimer;

		public VendorInventory(BaseHouse house, Mobile owner, string vendorName, string shopName)
		{
			House = house;
			Owner = owner;
			VendorName = vendorName;
			ShopName = shopName;

			Items = new();

			ExpireTime = DateTime.UtcNow + GracePeriod;

			m_ExpireTimer = new ExpireTimer(this, GracePeriod);
			m_ExpireTimer.Start();
		}

		public VendorInventory(BaseHouse house, GenericReader reader)
		{
			House = house;

			var version = reader.ReadEncodedInt();

			Owner = reader.ReadMobile();
			VendorName = reader.ReadString();
			ShopName = reader.ReadString();

			Items = reader.ReadStrongItemList();

			Gold = reader.ReadInt();

			ExpireTime = reader.ReadDeltaTime();

			if (Items.Count == 0 && Gold == 0)
			{
				Timer.DelayCall(Delete);
			}
			else
			{
				var delay = ExpireTime - DateTime.UtcNow;

				m_ExpireTimer = new ExpireTimer(this, delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
				m_ExpireTimer.Start();
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0); // version

			writer.Write(Owner);
			writer.Write(VendorName);
			writer.Write(ShopName);

			writer.Write(Items, true);

			writer.Write(Gold);

			writer.WriteDeltaTime(ExpireTime);
		}

		public void AddItem(Item item)
		{
			item.Internalize();

			Items.Add(item);
		}

		public void Delete()
		{
			foreach (var item in Items)
			{
				item.Delete();
			}

			Items.Clear();

			Gold = 0;

			House?.VendorInventories.Remove(this);

			m_ExpireTimer.Stop();
		}

		private class ExpireTimer : Timer
		{
			private readonly VendorInventory m_Inventory;

			public ExpireTimer(VendorInventory inventory, TimeSpan delay) : base(delay)
			{
				m_Inventory = inventory;

				Priority = TimerPriority.OneMinute;
			}

			protected override void OnTick()
			{
				var house = m_Inventory.House;

				if (house != null)
				{
					if (m_Inventory.Gold > 0)
					{
						house.MovingCrate ??= new MovingCrate(house);
						
						Banker.Deposit(house.MovingCrate, m_Inventory.Gold);
					}

					foreach (var item in m_Inventory.Items)
					{
						if (!item.Deleted)
						{
							house.DropToMovingCrate(item);
						}
					}

					m_Inventory.Gold = 0;
					m_Inventory.Items.Clear();
				}

				m_Inventory.Delete();
			}
		}
	}

	#region Server Vendor Sell And Buy Info

	public abstract class SBInfo
	{
		public static readonly List<SBInfo> Empty = new();

		public SBInfo()
		{
		}

		public abstract IShopSellInfo SellInfo { get; }
		public abstract List<GenericBuyInfo> BuyInfo { get; }
	}

	public class GenericSellInfo : IShopSellInfo
	{
		private readonly Dictionary<Type, int> m_Table = new();

		private Type[] m_Types;

		public GenericSellInfo()
		{
		}

		public void Add(Type type, int price)
		{
			m_Table[type] = price;
			m_Types = null;
		}

		public int GetSellPriceFor(Item item)
		{
			m_Table.TryGetValue(item.GetType(), out var price);

			if (item is IQuality q)
			{
				switch (q.Quality)
				{
					case ItemQuality.Low: price = (int)(price * 0.60); break;
					case ItemQuality.Exceptional: price = (int)(price * 1.25); break;
				}
			}

			if (item is BaseArmor armor)
			{
				price += 100 * (int)armor.Durability;
				price += 100 * (int)armor.ProtectionLevel;
			}
			else if (item is BaseWeapon weapon)
			{
				price += 100 * (int)weapon.DurabilityLevel;
				price += 100 * (int)weapon.DamageLevel;
			}
			else if (item is BaseBeverage bev)
			{
				var prices = (price, price);

				switch (bev)
				{
					case Pitcher: prices = (3, 5); break;
					case BeverageBottle: prices = (3, 3); break;
					case Jug: prices = (6, 6); break;
				}

				if (bev.IsEmpty || bev.Content == BeverageType.Milk)
				{
					price = prices.Item1;
				}
				else
				{
					price = prices.Item2;
				}
			}

			return Math.Max(1, price);
		}

		public int GetBuyPriceFor(Item item)
		{
			return (int)(1.90 * GetSellPriceFor(item));
		}

		public Type[] Types
		{
			get
			{
				if (m_Types == null)
				{
					m_Types = new Type[m_Table.Keys.Count];
					m_Table.Keys.CopyTo(m_Types, 0);
				}

				return m_Types;
			}
		}

		public string GetNameFor(Item item)
		{
			if (item.Name != null)
			{
				return item.Name;
			}

			return item.LabelNumber.ToString();
		}

		public bool IsSellable(Item item)
		{
			if (item.Nontransferable)
			{
				return false;
			}

			return IsInList(item.GetType());
		}

		public bool IsResellable(Item item)
		{
			if (item.Nontransferable)
			{
				return false;
			}

			return IsInList(item.GetType());
		}

		public bool IsInList(Type type)
		{
			return m_Table.ContainsKey(type);
		}
	}

	public class GenericBuyInfo : IBuyItemInfo
	{
		private class DisplayCache : Container
		{
			private static DisplayCache m_Cache;

			public static DisplayCache Cache
			{
				get
				{
					if (m_Cache?.Deleted != false)
					{
						m_Cache = new DisplayCache();
					}

					return m_Cache;
				}
			}

			private readonly Dictionary<Type, IEntity> m_Table = new();

			public List<Mobile> Mobiles { get; private set; }

			public DisplayCache() 
				: base(0)
			{
				Mobiles = new List<Mobile>();
			}

			public DisplayCache(Serial serial) 
				: base(serial)
			{
			}

			public IEntity Lookup(Type key)
			{
				m_Table.TryGetValue(key, out var e);

				return e;
			}

			public void Store(Type key, IEntity obj, bool cache)
			{
				if (cache)
				{
					m_Table[key] = obj;
				}

				if (obj is Item i)
				{
					AddItem(i);
				}
				else if (obj is Mobile m)
				{
					Mobiles.Add(m);
				}
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				DeleteEntities();

				if (m_Cache == this)
				{
					m_Cache = null;
				}
			}

			public void DeleteEntities()
			{
				DeleteMobiles();
				DeleteItems();
			}

			public void DeleteMobiles()
			{
				var mIndex = Mobiles.Count;

				while (--mIndex >= 0)
				{
					if (mIndex < Mobiles.Count)
					{
						Mobiles[mIndex]?.Delete();
					}
				}

				Mobiles.Clear();
			}

			public void DeleteItems()
			{
				var iIndex = Items.Count;

				while (--iIndex >= 0)
				{
					if (iIndex < Items.Count)
					{
						Items[iIndex]?.Delete();
					}
				}

				Items.Clear();
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0); // version

				writer.Write(Mobiles);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				_ = reader.ReadInt(); // version

				Mobiles = reader.ReadStrongMobileList();

				DeleteEntities();

				if (m_Cache == null)
				{
					m_Cache = this;
				}
				else
				{
					Delete();
				}
			}
		}

		public Type Type { get; }
		public object[] Args { get; }

		public string Name { get; set; }

		public int ItemID { get; set; }
		public int Hue { get; set; }

		public int Amount { get; set; }
		public int MaxAmount { get; set; }

		public int ControlSlots { get; set; }

		public bool CanCacheDisplay { get; set; }

		public int PriceScalar { get; set; }

		private int m_Price;

		public int Price
		{
			get
			{
				if (PriceScalar > 0)
				{
					if (m_Price > 5000000)
					{
						long price = m_Price;

						price *= PriceScalar;
						price += 50;
						price /= 100;

						if (price > Int32.MaxValue)
						{
							price = Int32.MaxValue;
						}

						return (int)price;
					}

					return ((m_Price * PriceScalar) + 50) / 100;
				}

				return m_Price;
			}
			set => m_Price = value;
		}

		private IEntity m_DisplayEntity;

		public GenericBuyInfo(Type type, int price, int amount, int itemID, int hue) 
			: this(null, type, price, amount, itemID, hue, null)
		{
		}

		public GenericBuyInfo(string name, Type type, int price, int amount, int itemID, int hue) 
			: this(name, type, price, amount, itemID, hue, null)
		{
		}

		public GenericBuyInfo(Type type, int price, int amount, int itemID, int hue, object[] args) 
			: this(null, type, price, amount, itemID, hue, args)
		{
		}

		public GenericBuyInfo(string name, Type type, int price, int amount, int itemID, int hue, object[] args)
		{
			Type = type;
			Args = args;

			Price = price;

			Amount = amount;
			MaxAmount = amount;

			ItemID = itemID;
			Hue = hue;

			if (name == null)
			{
				Name = $"{(itemID < 0x4000 ? 1020000 : 1078872) + itemID}";
			}
			else
			{
				Name = name;
			}

			if (Args == null || Args.Length == 0)
			{
				CanCacheDisplay = true;
			}
		}

		public void DeleteDisplayEntity()
		{
			m_DisplayEntity?.Delete();
			m_DisplayEntity = null;
		}

		public IEntity GetDisplayEntity()
		{
			if (m_DisplayEntity?.Deleted == false)
			{
				return m_DisplayEntity;
			}

			if (CanCacheDisplay)
			{
				m_DisplayEntity = DisplayCache.Cache.Lookup(Type);
			}

			if (m_DisplayEntity?.Deleted != false)
			{
				m_DisplayEntity = GetEntity();
			}

			DisplayCache.Cache.Store(Type, m_DisplayEntity, CanCacheDisplay);

			return m_DisplayEntity;
		}

		public virtual IEntity GetEntity()
		{
			return Activator.CreateInstance(Type, Args) as IEntity;
		}

		public bool Restock(Item item, int amount)
		{
			if (amount <= 0 || item == null || item.GetType() != Type)
			{
				return false;
			}

			if (item is IQuality q && q.Quality != ItemQuality.Regular)
			{
				return false;
			}

			if (item is BaseWeapon weapon)
			{
				if (weapon.DurabilityLevel > 0 || weapon.DamageLevel > 0 || weapon.AccuracyLevel > 0)
				{
					return false;
				}
			}
			else if (item is BaseArmor armor)
			{
				if (armor.Durability > 0 || armor.ProtectionLevel > 0)
				{
					return false;
				}
			}

			Amount += amount;

			return true;
		}

		public void OnRestock()
		{
			if (Amount <= 0)
			{
				/*
				 * Core.ML using this vendor system is undefined behavior, so being
				 * as it lends itself to an abusable exploit to cause ingame havok
				 * and the stackable items are not found to be over 20 items, this is
				 * changed until there is a better solution.
				 */

				var display = GetDisplayEntity();

				if (Core.ML && display is Item displayItem && !displayItem.Stackable)
				{
					MaxAmount = Math.Min(20, MaxAmount);
				}
				else
				{
					MaxAmount = Math.Min(999, MaxAmount * 2);
				}
			}
			else
			{
				/* NOTE: According to UO.com, the quantity is halved if the item does not reach 0
				 * Here we implement differently: the quantity is halved only if less than half
				 * of the maximum quantity was bought. That is, if more than half is sold, then
				 * there's clearly a demand and we should not cut down on the stock.
				 */

				var halfQuantity = MaxAmount;

				if (halfQuantity >= 999)
				{
					halfQuantity = 640;
				}
				else if (halfQuantity > 20)
				{
					halfQuantity /= 2;
				}

				if (Amount >= halfQuantity)
				{
					MaxAmount = halfQuantity;
				}
			}

			Amount = MaxAmount;
		}
	}

	public class AnimalBuyInfo : GenericBuyInfo
	{
		public AnimalBuyInfo(int controlSlots, Type type, int price, int amount, int itemID, int hue) 
			: this(controlSlots, null, type, price, amount, itemID, hue)
		{
		}

		public AnimalBuyInfo(int controlSlots, string name, Type type, int price, int amount, int itemID, int hue) 
			: base(name, type, price, amount, itemID, hue)
		{
			ControlSlots = controlSlots;
		}
	}

	public class BeverageBuyInfo : GenericBuyInfo
	{
		public BeverageBuyInfo(Type type, BeverageType content, int price, int amount, int itemID, int hue)
			: this(null, type, content, price, amount, itemID, hue)
		{
		}

		public BeverageBuyInfo(string name, Type type, BeverageType content, int price, int amount, int itemID, int hue)
			: base(name, type, price, amount, itemID, hue, new object[] { content })
		{
			var label = 0;

			if (type == typeof(Pitcher))
			{
				label = 1048128;
			}
			else if (type == typeof(BeverageBottle))
			{
				label = 1042959;
			}
			else if (type == typeof(Jug))
			{
				label = 1042965;
			}

			if (label > 0)
			{
				Name = $"#{label + (int)content}";
			}
		}
	}

	public class PresetMapBuyInfo : GenericBuyInfo
	{
		public PresetMapBuyInfo(PresetMapEntry entry, int price, int amount)
			: base(entry.Name.ToString(), typeof(PresetMap), price, amount, 0x14EC, 0, new object[] { entry })
		{
		}
	}

	#endregion
}