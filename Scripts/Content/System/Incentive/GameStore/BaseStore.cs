using Server.Accounting;
using Server.Commands;
using Server.Engines.Events;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#region Developer Notations

/// Recommend you use custom currency when using this system. EA uses sovreigns which come to about 5,000 for 49.95 US dollars. 
///
/// This is a good base on how to value the items. The item cost in the store are per EA, so careful attention should be made 
/// when balancing out your currency system, and how that currency will be obtained. Flooding your market with some of these 
/// items ill not be a good idea.
/// 
/// Also, if you're like me, and hate the flashy hues, you can comment out the store entries in UltimaStore.cs to your liking. 
/// 
/// Currently, the system supports Gold or points system currency. For a list of various points systems, go to PointsSystems.cs 
/// where a solid list can be viewed. If you use a custom currency [recommended], you will need to modify the following 
/// functions in the Configuration below:
/// 
/// [x] public static double GetCustomCurrency(Mobile m)
/// 
/// - this needs to return the currency for that player. This is to ensure they have sufficient currency before an item can be purchased.
/// 
/// [x] public static void DeductCustomCurrecy(Mobile m)
/// 
/// - This ensures that the currency points are actually deducted after the sale.  It's a good idea to get this right.
/// 
/// CurrencyType description:
/// 
/// None - disables the system
/// Sovereigns - Sovereigns, added as a seperate account currency, will be up to the shard owners how to be implemented.
/// Gold - uses standard gold currency. If you use this, I would suggest your increase the PointMultiplier, significantly 
///        so your not flooding your market with these items
/// PointsSystem - using this, you will have to designate PointsSystemCurrency as to which points system your going to use. 
///                Check PointsSystem.cs for a list of all the points systems, ie Despise Crystals, Void Pool points, etc.
/// Custom - see above on how to implement custom currency

#endregion

namespace Server.Engines.UOStore
{
	public enum CurrencyType
	{
		None,
		Sovereigns,
		Gold,
		Custom
	}

	public delegate int CustomCurrencyHandler(Mobile m, int consume);

	public static class Configuration
	{
		public static bool Enabled = true;

		public static string Website = "https://uo.com/ultima-store/";

		public static CurrencyType CurrencyImpl = CurrencyType.Sovereigns;

		public static string CurrencyName = "Sovereigns";

		public static bool CurrencyDisplay = true;

		public static double CostMultiplier = 1.0;

		public static int CartCapacity = 10;

		/// <summary>
		///     A hook to allow handling of custom currencies.
		///     This implementation should be treated as such;
		///     If 'consume' is less than zero, return the currency total.
		///     Else deduct from the currency total, return the amount consumed.
		/// </summary>
		public static CustomCurrencyHandler ResolveCurrency { get; set; }

		public static int GetCustomCurrency(Mobile m)
		{
			if (ResolveCurrency != null)
			{
				return ResolveCurrency(m, -1);
			}

			m.SendMessage(1174, "Currency is not set up for this system. Contact a shard administrator.");

			Utility.PushColor(ConsoleColor.Red);
			Console.WriteLine("[Ultima Store]: No custom currency method has been implemented.");
			Utility.PopColor();

			return 0;
		}

		public static int DeductCustomCurrecy(Mobile m, int amount)
		{
			if (ResolveCurrency != null)
			{
				return ResolveCurrency(m, amount);
			}

			m.SendMessage(1174, "Currency is not set up for this system. Contact a shard administrator.");

			Utility.PushColor(ConsoleColor.Red);
			Console.WriteLine("[Ultima Store]: No custom currency deduction method has been implemented.");
			Utility.PopColor();

			return 0;
		}
	}

	public class PlayerProfile
	{
		public const StoreCategory DefaultCategory = StoreCategory.Featured;
		public const SortBy DefaultSortBy = SortBy.Newest;

		public Dictionary<StoreEntry, int> Cart { get; private set; }

		public Mobile Player { get; private set; }

		public StoreCategory Category { get; set; }
		public SortBy SortBy { get; set; }

		public int VaultTokens { get; set; }

		public PlayerProfile(Mobile m)
		{
			Cart = new Dictionary<StoreEntry, int>();

			Player = m;

			Category = DefaultCategory;
			SortBy = DefaultSortBy;
		}

		public PlayerProfile(GenericReader reader)
		{
			Cart = new Dictionary<StoreEntry, int>();

			Deserialize(reader);
		}

		public void AddToCart(StoreEntry entry, int amount)
		{
			if (Cart.Count < Configuration.CartCapacity)
			{
				Cart[entry] = amount;
			}
		}

		public void RemoveFromCart(StoreEntry entry)
		{
			Cart.Remove(entry);
		}

		public void SetCartAmount(StoreEntry entry, int amount)
		{
			if (amount > 0)
			{
				AddToCart(entry, amount);
			}
			else
			{
				RemoveFromCart(entry);
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(1);

			writer.Write(VaultTokens);

			writer.Write(Player);

			writer.Write((int)Category);
			writer.Write((int)SortBy);
		}

		public void Deserialize(GenericReader reader)
		{
			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					VaultTokens = reader.ReadInt();
					goto case 0;
				case 0:
					Player = reader.ReadMobile();

					Category = (StoreCategory)reader.ReadInt();
					SortBy = (SortBy)reader.ReadInt();
					break;
			}
		}
	}

	#region Store Promo

	public static class PromoCodes
	{
		public static string FilePath => Path.Combine(Core.CurrentSavesDirectory, "Store", "Promo.bin");

		public static readonly Dictionary<string, PromoCodeHandler> Codes = new Dictionary<string, PromoCodeHandler>(StringComparer.InvariantCultureIgnoreCase);

		public static readonly Dictionary<string, HashSet<IAccount>> Redeemed = new Dictionary<string, HashSet<IAccount>>(StringComparer.InvariantCultureIgnoreCase);

		public static void Configure()
		{
			// TODO: Admin UI
			// CommandSystem.Register("Promos", AccessLevel.Administrator, e => OpenEditor(e.Mobile as PlayerMobile));

			EventSink.WorldSave += OnSave;
			EventSink.WorldLoad += OnLoad;

#if DEBUG
			Register("TEST-1234", TestCodeRedeem);
#endif
		}

#if DEBUG
		private static bool TestCodeRedeem(Mobile user, string code)
		{
			if (user.AccessLevel > AccessLevel.Counselor)
			{
				user.SendMessage($"The promo code test has passed for '{code}'");

				return true;
			}

			return false;
		}
#endif

		#region Register API

		public static void Unregister(string code, bool clearRedeemed)
		{
			Codes.Remove(code);

			if (clearRedeemed)
			{
				ClearRedeemed(code);
			}
		}

		public static void Register(string code, PromoCodeRedeemer redeemer)
		{
			Register(code, 0, DateTime.MinValue, DateTime.MaxValue, redeemer);
		}

		public static void Register(string code, int limited, PromoCodeRedeemer redeemer)
		{
			Register(code, limited, DateTime.MinValue, DateTime.MaxValue, redeemer);
		}

		public static void Register(string code, int endYear, int endMonth, int endDay, PromoCodeRedeemer redeemer)
		{
			Register(code, 0, endYear, endMonth, endDay, redeemer);
		}

		public static void Register(string code, DateTime ends, PromoCodeRedeemer redeemer)
		{
			Register(code, 0, DateTime.MinValue, ends, redeemer);
		}

		public static void Register(string code, int limited, int endYear, int endMonth, int endDay, PromoCodeRedeemer redeemer)
		{
			Register(code, limited, DateTime.MinValue, new DateTime(endYear, endMonth, endDay, 23, 59, 59, 999, DateTimeKind.Utc), redeemer);
		}

		public static void Register(string code, int beginYear, int beginMonth, int beginDay, int endYear, int endMonth, int endDay, PromoCodeRedeemer redeemer)
		{
			Register(code, 0, beginYear, beginMonth, beginDay, endYear, endMonth, endDay, redeemer);
		}

		public static void Register(string code, DateTime begins, DateTime ends, PromoCodeRedeemer redeemer)
		{
			Register(code, 0, begins, ends, redeemer);
		}

		public static void Register(string code, int limited, int beginYear, int beginMonth, int beginDay, int endYear, int endMonth, int endDay, PromoCodeRedeemer redeemer)
		{
			Register(code, limited, new DateTime(beginYear, beginMonth, beginDay, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(endYear, endMonth, endDay, 23, 59, 59, 999, DateTimeKind.Utc), redeemer);
		}

		public static void Register(string code, int limited, DateTime begins, DateTime ends, PromoCodeRedeemer redeemer)
		{
			Codes[code] = new PromoCodeHandler(limited, begins, ends, redeemer);
		}

		#endregion

		#region Redeem API

		public static void ClearRedeemed(string code)
		{
			HashSet<IAccount> users;

			if (Redeemed.TryGetValue(code, out users))
			{
				users.Clear();
				users.TrimExcess();

				Redeemed.Remove(code);
			}
		}

		public static bool TryRedeem(Mobile m, string code)
		{
			if (m == null || m.Deleted || m.Account == null)
			{
				return false;
			}

			if (m.Account.Age.TotalDays < 1)
			{
				m.SendMessage("Your account must be at least one day old to redeem promo codes!");

				return false;
			}

			if (String.IsNullOrWhiteSpace(code))
			{
				m.SendMessage("The promo code you provided is invalid.");

				return false;
			}

			code = code.Trim();

			PromoCodeHandler handler;

			if (!Codes.TryGetValue(code, out handler))
			{
				m.SendMessage("The promo code you provided is invalid.");

				return false;
			}

			if (handler == null || DateTime.UtcNow < handler.Begins)
			{
				m.SendMessage("The promo code you provided could not be processed at this time.");

				return false;
			}

			HashSet<IAccount> users;

			if (!Redeemed.TryGetValue(code, out users) || users == null)
			{
				Redeemed[code] = users = new HashSet<IAccount>();
			}

			if (users.Contains(m.Account) || users.Any(a => a.LoginIPs.Any(ip => Array.IndexOf(m.Account.LoginIPs, ip) != -1)))
			{
				m.SendMessage("You have already redeemed this promo code.");

				return false;
			}

			if (handler.Limited > 0 && users.Count >= handler.Limited)
			{
				m.SendMessage("The promo code you provded has reached its claim limit.");

				return false;
			}

			if (handler.Expires > DateTime.MinValue && DateTime.UtcNow > handler.Expires)
			{
				m.SendMessage("The promo code you provded has expired.");

				return false;
			}

			if (!users.Add(m.Account))
			{
				m.SendMessage("You have already redeemed this promo code.");

				return false;
			}

			if (!handler.Redeemer(m, code))
			{
				m.SendMessage("The promo code you provided could not be processed at this time.");

				return false;
			}

			return true;
		}

		#endregion

		#region Persistence API

		public static void OnSave(WorldSaveEventArgs e)
		{
			Persistence.Serialize(FilePath, OnSerialize);
		}

		private static void OnSerialize(GenericWriter writer)
		{
			writer.Write(0);

			writer.Write(Redeemed.Count);

			foreach (var kvp in Redeemed)
			{
				writer.Write(kvp.Key);

				writer.Write(kvp.Value.Count);

				foreach (var a in kvp.Value)
				{
					writer.Write(a.Username);
				}
			}
		}

		public static void OnLoad()
		{
			Persistence.Deserialize(FilePath, OnDeserialize);
		}

		private static void OnDeserialize(GenericReader reader)
		{
			reader.ReadInt();

			var count = reader.ReadInt();

			for (var i = 0; i < count; i++)
			{
				var code = reader.ReadString();

				var ac = reader.ReadInt();

				var list = new HashSet<IAccount>(ac);

				while (--ac >= 0)
				{
					var u = reader.ReadString();

					if (u != null)
					{
						var a = Accounts.GetAccount(u);

						if (a != null)
						{
							list.Add(a);
						}
					}
				}

				if (code != null && list.Count > 0 && Codes.ContainsKey(code))
				{
					Redeemed[code] = list;
				}
			}
		}

		#endregion
	}

	public delegate bool PromoCodeRedeemer(Mobile user, string code);

	[PropertyObject]
	public class PromoCodeHandler
	{
		[CommandProperty(AccessLevel.Administrator)]
		public DateTime Begins { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		public DateTime Expires { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		public int Limited { get; set; }

		public PromoCodeRedeemer Redeemer { get; set; }

		public PromoCodeHandler(int limited, DateTime begins, DateTime ends, PromoCodeRedeemer redeemer)
		{
			if (begins > ends)
			{
				Begins = ends;
				Expires = begins;
			}
			else
			{
				Begins = begins;
				Expires = ends;
			}

			Limited = limited;
			Redeemer = redeemer;
		}
	}

	#endregion

	#region Store Items

	public class StoreEntry
	{
		public Type ItemType { get; private set; }
		public TextDefinition[] Name { get; private set; }
		public int Tooltip { get; private set; }
		public int GumpID { get; private set; }
		public int ItemID { get; private set; }
		public int Hue { get; private set; }
		public int Price { get; private set; }
		public StoreCategory Category { get; private set; }
		public Func<Mobile, StoreEntry, Item> Constructor { get; private set; }

		public int Cost => (int)Math.Ceiling(Price * Configuration.CostMultiplier);

		public StoreEntry(Type itemType, TextDefinition name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
			: this(itemType, new[] { name }, tooltip, itemID, gumpID, hue, cost, cat, constructor)
		{ }

		public StoreEntry(Type itemType, TextDefinition[] name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
		{
			ItemType = itemType;
			Name = name;
			Tooltip = tooltip;
			ItemID = itemID;
			GumpID = gumpID;
			Hue = hue;
			Price = cost;
			Category = cat;
			Constructor = constructor;
		}

		public bool Construct(Mobile m, bool test = false)
		{
			Item item;

			if (Constructor != null)
			{
				item = Constructor(m, this);
			}
			else
			{
				item = Activator.CreateInstance(ItemType) as Item;
			}

			if (item != null)
			{
				if (m.Backpack == null || !m.Alive || !m.Backpack.TryDropItem(m, item, false))
				{
					UltimaStore.AddPendingItem(m, item);

					// Your purchased will be delivered to you once you free up room in your backpack.
					// Your purchased item will be delivered to you once you are resurrected.
					m.SendLocalizedMessage(m.Alive ? 1156846 : 1156848);
				}
				else if (item.LabelNumber > 0 || item.Name != null)
				{
					var name = item.LabelNumber > 0 ? ("#" + item.LabelNumber) : item.Name;

					// Your purchase of ~1_ITEM~ has been placed in your backpack.
					m.SendLocalizedMessage(1156844, name);
				}
				else
				{
					// Your purchased item has been placed in your backpack.
					m.SendLocalizedMessage(1156843);
				}

				if (test)
				{
					item.Delete();
				}

				return true;
			}

			Utility.PushColor(ConsoleColor.Red);
			Console.WriteLine("[Ultima Store Warning]: {0} failed to construct.", ItemType.Name);
			Utility.PopColor();

			return false;
		}
	}

	public enum StoreCategory
	{
		None,
		Featured,
		Character,
		Equipment,
		Decorations,
		Mounts,
		Misc,
		Cart
	}

	public enum SortBy
	{
		Name,
		PriceLower,
		PriceHigher,
		Newest,
		Oldest
	}

	public static partial class UltimaStore
	{
		public static string FilePath => Path.Combine(Core.CurrentSavesDirectory, "Store", "Store.bin");

		public static bool Enabled { get => Configuration.Enabled; set => Configuration.Enabled = value; }

		public static List<StoreEntry> Entries { get; private set; }
		public static Dictionary<Mobile, List<Item>> PendingItems { get; private set; }

		private static UltimaStoreContainer _UltimaStoreContainer;

		public static UltimaStoreContainer UltimaStoreContainer
		{
			get
			{
				if (_UltimaStoreContainer != null && _UltimaStoreContainer.Deleted)
				{
					_UltimaStoreContainer = null;
				}

				return _UltimaStoreContainer ?? (_UltimaStoreContainer = new UltimaStoreContainer());
			}
		}

		static UltimaStore()
		{
			Entries = new List<StoreEntry>();
			PendingItems = new Dictionary<Mobile, List<Item>>();
			PlayerProfiles = new Dictionary<Mobile, PlayerProfile>();
		}

		public static void Configure()
		{
			PacketHandlers.Register(0xFA, 1, true, UOStoreRequest);

			CommandSystem.Register("Store", AccessLevel.Player, e => OpenStore(e.Mobile as PlayerMobile));

			EventSink.WorldSave += OnSave;
			EventSink.WorldLoad += OnLoad;
		}

		public static void Register<T>(TextDefinition name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null) where T : Item
		{
			Register(typeof(T), name, tooltip, itemID, gumpID, hue, cost, cat, constructor);
		}

		public static void Register(Type itemType, TextDefinition name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
		{
			Register(new StoreEntry(itemType, name, tooltip, itemID, gumpID, hue, cost, cat, constructor));
		}

		public static void Register<T>(TextDefinition[] name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null) where T : Item
		{
			Register(typeof(T), name, tooltip, itemID, gumpID, hue, cost, cat, constructor);
		}

		public static void Register(Type itemType, TextDefinition[] name, int tooltip, int itemID, int gumpID, int hue, int cost, StoreCategory cat, Func<Mobile, StoreEntry, Item> constructor = null)
		{
			Register(new StoreEntry(itemType, name, tooltip, itemID, gumpID, hue, cost, cat, constructor));
		}

		public static StoreEntry GetEntry(Type t)
		{
			return Entries.FirstOrDefault(e => e.ItemType == t);
		}

		public static void Register(StoreEntry entry)
		{
			Entries.Add(entry);
		}

		public static bool CanSearch(Mobile m)
		{
			return m != null && m.Region.GetLogoutDelay(m) <= TimeSpan.Zero;
		}

		public static void UOStoreRequest(NetState state, PacketReader pvSrc)
		{
			OpenStore(state.Mobile as PlayerMobile);
		}

		public static void OpenStore(PlayerMobile user, StoreEntry forcedEntry = null)
		{
			if (user == null || user.NetState == null)
			{
				return;
			}

			if (!Enabled)
			{
				// The promo code redemption system is currently unavailable. Please try again later.
				user.SendLocalizedMessage(1062904);
				return;
			}

			if (Configuration.CurrencyImpl == CurrencyType.None)
			{
				// The promo code redemption system is currently unavailable. Please try again later.
				user.SendLocalizedMessage(1062904);
				return;
			}

			if (user.AccessLevel < AccessLevel.Counselor && !CanSearch(user))
			{
				// Before using the in game store, you must be in a safe log-out location
				// such as an inn or a house which has you on its Owner, Co-owner, or Friends list.
				user.SendLocalizedMessage(1156586);
				return;
			}

			if (!user.HasGump(typeof(UltimaStoreGump)))
			{
				BaseGump.SendGump(new UltimaStoreGump(user, forcedEntry));
			}
		}

		#region Constructors
		public static Item ConstructPigments(Mobile m, StoreEntry entry)
		{
			var type = PigmentType.None;

			for (var i = 0; i < PigmentsOfTokuno.Table.Length; i++)
			{
				if (PigmentsOfTokuno.Table[i][1] == entry.Name[1].Number)
				{
					type = (PigmentType)i;
					break;
				}
			}

			if (type != PigmentType.None)
			{
				return new PigmentsOfTokuno(type, 50);
			}

			return null;
		}

		public static Item ConstructEarrings(Mobile m, StoreEntry entry)
		{
			var ele = AosElementAttribute.Physical;

			switch (entry.Name[0].Number)
			{
				case 1071092: ele = AosElementAttribute.Fire; break;
				case 1071093: ele = AosElementAttribute.Cold; break;
				case 1071094: ele = AosElementAttribute.Poison; break;
				case 1071095: ele = AosElementAttribute.Energy; break;
			}

			return new EarringsOfProtection(ele);
		}

		public static Item ConstructMiniHouseDeed(Mobile m, StoreEntry entry)
		{
			var label = entry.Name[1].Number;

			switch (label)
			{
				default:
					for (var i = 0; i < MiniHouseInfo.Info.Length; i++)
					{
						if (MiniHouseInfo.Info[i].LabelNumber == entry.Name[1].Number)
						{
							var type = (MiniHouseType)i;

							return new MiniHouseDeed(type);
						}
					}
					return null;
				case 1157015: return new MiniHouseDeed(MiniHouseType.TwoStoryWoodAndPlaster);
				case 1157014: return new MiniHouseDeed(MiniHouseType.TwoStoryStoneAndPlaster);
			}
		}

		public static Item ConstructLampPost(Mobile m, StoreEntry entry)
		{
			var item = new LampPost2 {
				Movable = true,
				LootType = LootType.Blessed
			};

			return item;
		}
		#endregion

		public static void AddPendingItem(Mobile m, Item item)
		{
			List<Item> list;

			if (!PendingItems.TryGetValue(m, out list))
			{
				PendingItems[m] = list = new List<Item>();
			}

			if (!list.Contains(item))
			{
				list.Add(item);
			}

			UltimaStoreContainer.DropItem(item);
		}

		public static bool HasPendingItem(PlayerMobile pm)
		{
			return PendingItems.ContainsKey(pm);
		}

		public static void CheckPendingItem(Mobile m)
		{
			List<Item> list;

			if (PendingItems.TryGetValue(m, out list))
			{
				var index = list.Count;

				while (--index >= 0)
				{
					if (index >= list.Count)
					{
						continue;
					}

					var item = list[index];

					if (item != null)
					{
						if (m.Backpack != null && m.Alive && m.Backpack.TryDropItem(m, item, false))
						{
							if (item.LabelNumber > 0 || item.Name != null)
							{
								var name = item.LabelNumber > 0 ? ("#" + item.LabelNumber) : item.Name;

								// Your purchase of ~1_ITEM~ has been placed in your backpack.
								m.SendLocalizedMessage(1156844, name);
							}
							else
							{
								// Your purchased item has been placed in your backpack.
								m.SendLocalizedMessage(1156843);
							}

							list.RemoveAt(index);
						}
					}
					else
					{
						list.RemoveAt(index);
					}
				}

				if (list.Count == 0 && PendingItems.Remove(m))
				{
					list.TrimExcess();
				}
			}
		}

		public static List<StoreEntry> GetSortedList(string searchString)
		{
			var list = new List<StoreEntry>();

			list.AddRange(Entries.Where(e => Insensitive.Contains(GetStringName(e.Name), searchString)));

			return list;
		}

		public static string GetStringName(TextDefinition[] text)
		{
			var str = String.Empty;

			foreach (var td in text)
			{
				if (td.Number > 0 && StringList.Localization != null)
				{
					str += String.Format("{0} ", StringList.Localization.GetString(td.Number));
				}
				else if (!String.IsNullOrWhiteSpace(td.String))
				{
					str += String.Format("{0} ", td.String);
				}
			}

			return str;
		}

		public static string GetStringName(TextDefinition text)
		{
			var str = text.String;

			return str ?? String.Empty;
		}

		public static List<StoreEntry> GetList(StoreCategory cat, StoreEntry forcedEntry = null)
		{
			if (forcedEntry != null)
			{
				return new List<StoreEntry>() { forcedEntry };
			}

			return Entries.Where(e => e.Category == cat).ToList();
		}

		public static void SortList(List<StoreEntry> list, SortBy sort)
		{
			switch (sort)
			{
				case SortBy.Name:
					list.Sort((a, b) => String.CompareOrdinal(GetStringName(a.Name), GetStringName(b.Name)));
					break;
				case SortBy.PriceLower:
					list.Sort((a, b) => a.Price.CompareTo(b.Price));
					break;
				case SortBy.PriceHigher:
					list.Sort((a, b) => b.Price.CompareTo(a.Price));
					break;
				case SortBy.Newest:
					break;
				case SortBy.Oldest:
					list.Reverse();
					break;
			}
		}

		public static int CartCount(Mobile m)
		{
			var profile = GetProfile(m, false);

			if (profile != null)
			{
				return profile.Cart.Count;
			}

			return 0;
		}

		public static int GetSubTotal(Dictionary<StoreEntry, int> cart)
		{
			if (cart == null || cart.Count == 0)
			{
				return 0;
			}

			var sub = 0.0;

			foreach (var kvp in cart)
			{
				sub += kvp.Key.Cost * kvp.Value;
			}

			return (int)sub;
		}

		public static int GetCurrency(Mobile m, bool sendMessage = false)
		{
			switch (Configuration.CurrencyImpl)
			{
				case CurrencyType.Sovereigns:
					{
						if (m is PlayerMobile)
						{
							return ((PlayerMobile)m).AccountSovereigns;
						}
					}
					break;
				case CurrencyType.Gold:
					return Banker.GetBalance(m);
				case CurrencyType.Custom:
					return Configuration.GetCustomCurrency(m);
			}

			return 0;
		}

		public static void TryPurchase(Mobile m)
		{
			var cart = GetCart(m);
			var total = GetSubTotal(cart);

			if (cart == null || cart.Count == 0 || total == 0)
			{
				// Purchase failed due to your cart being empty.
				m.SendLocalizedMessage(1156842);
			}
			else if (total > GetCurrency(m, true))
			{
				if (m is PlayerMobile)
				{
					BaseGump.SendGump(new NoFundsGump((PlayerMobile)m));
				}
			}
			else
			{
				var subtotal = 0;
				var fail = false;

				var remove = new List<StoreEntry>();

				foreach (var entry in cart)
				{
					for (var i = 0; i < entry.Value; i++)
					{
						if (!entry.Key.Construct(m))
						{
							fail = true;

							try
							{
								using (var op = File.AppendText("UltimaStoreError.log"))
								{
									op.WriteLine("Bad Constructor: {0}", entry.Key.ItemType.Name);

									Utility.PushColor(ConsoleColor.Red);
									Console.WriteLine("[Ultima Store]: Bad Constructor: {0}", entry.Key.ItemType.Name);
									Utility.PopColor();
								}
							}
							catch (Exception e)
							{
								Utility.PushColor(ConsoleColor.Red);
								Console.WriteLine("[Ultima Store]: {0}", e);
								Utility.PopColor();
							}
						}
						else
						{
							remove.Add(entry.Key);

							subtotal += entry.Key.Cost;
						}
					}
				}

				if (subtotal > 0)
				{
					DeductCurrency(m, subtotal);
				}

				var profile = GetProfile(m);

				foreach (var entry in remove)
				{
					profile.RemoveFromCart(entry);
				}

				if (fail)
				{
					// Failed to process one of your items. Please check your cart and try again.
					m.SendLocalizedMessage(1156853);
				}
			}
		}

		/// <summary>
		/// Should have already passed GetCurrency
		/// </summary>
		/// <param name="m"></param>
		/// <param name="amount"></param>
		public static int DeductCurrency(Mobile m, int amount)
		{
			switch (Configuration.CurrencyImpl)
			{
				case CurrencyType.Sovereigns:
					{
						if (m is PlayerMobile && ((PlayerMobile)m).WithdrawSovereigns(amount))
						{
							return amount;
						}
					}
					break;
				case CurrencyType.Gold:
					{
						if (Banker.Withdraw(m, amount))
						{
							return amount;
						}
					}
					break;
				case CurrencyType.Custom:
					return Configuration.DeductCustomCurrecy(m, amount);
			}

			return 0;
		}

		#region Player Persistence
		public static Dictionary<Mobile, PlayerProfile> PlayerProfiles { get; private set; }

		public static PlayerProfile GetProfile(Mobile m, bool create = true)
		{
			PlayerProfile profile;

			if ((!PlayerProfiles.TryGetValue(m, out profile) || profile == null) && create)
			{
				PlayerProfiles[m] = profile = new PlayerProfile(m);
			}

			return profile;
		}

		public static Dictionary<StoreEntry, int> GetCart(Mobile m)
		{
			var profile = GetProfile(m, false);

			if (profile != null)
			{
				return profile.Cart;
			}

			return null;
		}

		public static void OnSave(WorldSaveEventArgs e)
		{
			Persistence.Serialize(FilePath, Serialize);
		}

		public static void OnLoad()
		{
			Persistence.Deserialize(FilePath, Deserialize);
		}

		private static void Serialize(GenericWriter writer)
		{
			writer.Write(0);

			writer.Write(_UltimaStoreContainer);

			writer.Write(PendingItems.Count);

			foreach (var kvp in PendingItems)
			{
				writer.Write(kvp.Key);
				writer.WriteItemList(kvp.Value, true);
			}

			writer.Write(PlayerProfiles.Count);

			foreach (var pe in PlayerProfiles)
			{
				pe.Value.Serialize(writer);
			}
		}

		private static void Deserialize(GenericReader reader)
		{
			reader.ReadInt();

			_UltimaStoreContainer = reader.ReadItem<UltimaStoreContainer>();

			var count = reader.ReadInt();

			for (var i = 0; i < count; i++)
			{
				var m = reader.ReadMobile();
				var list = reader.ReadStrongItemList<Item>();

				if (m != null && list.Count > 0)
				{
					PendingItems[m] = list;
				}
			}

			count = reader.ReadInt();

			for (var i = 0; i < count; i++)
			{
				var pe = new PlayerProfile(reader);

				if (pe.Player != null)
				{
					PlayerProfiles[pe.Player] = pe;
				}
			}
		}
		#endregion
	}

	public sealed class UltimaStoreContainer : Container
	{
		private static readonly List<Item> _DisplayItems = new List<Item>();

		public override bool Decays => false;

		public override string DefaultName => "Ultima Store Display Container";

		public UltimaStoreContainer()
			: base(0) // No Draw
		{
			Movable = false;
			Visible = false;

			Internalize();
		}

		public UltimaStoreContainer(Serial serial)
			: base(serial)
		{ }

		public void AddDisplayItem(Item item)
		{
			if (item == null)
			{
				return;
			}

			if (!_DisplayItems.Contains(item))
			{
				_DisplayItems.Add(item);
			}

			DropItem(item);
		}

		public Item FindDisplayItem(Type t)
		{
			var item = GetDisplayItem(t);

			if (item == null)
			{
				item = Loot.Construct(t);

				if (item != null)
				{
					AddDisplayItem(item);
				}
			}

			return item;
		}

		public Item GetDisplayItem(Type t)
		{
			return _DisplayItems.FirstOrDefault(x => x.GetType() == t);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.WriteItemList(_DisplayItems, true);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();

			var list = reader.ReadStrongItemList();

			if (list.Count > 0)
			{
				Timer.DelayCall(o => o.ForEach(AddDisplayItem), list);
			}
		}
	}

	#endregion

	#region Store Gumps

	public class UltimaStoreGump : BaseGump
	{
		private readonly int[][] _Offset =
		{
			new[] { 167, 74 },
			new[] { 354, 74 },
			new[] { 541, 74 },
			new[] { 167, 294 },
			new[] { 354, 294 },
			new[] { 541, 294 }
		};

		public StoreCategory Category
		{
			get
			{
				var profile = UltimaStore.GetProfile(User, false);

				if (profile != null)
				{
					return profile.Category;
				}

				return PlayerProfile.DefaultCategory;
			}
		}

		public SortBy SortBy
		{
			get
			{
				var profile = UltimaStore.GetProfile(User, false);

				if (profile != null)
				{
					return profile.SortBy;
				}

				return PlayerProfile.DefaultSortBy;
			}
		}

		public Dictionary<StoreEntry, int> Cart
		{
			get
			{
				var profile = UltimaStore.GetProfile(User, false);

				if (profile != null)
				{
					return profile.Cart;
				}

				return null;
			}
		}

		public int Page { get; private set; }
		public string SearchText { get; private set; }
		public List<StoreEntry> StoreList { get; private set; }

		public bool Search { get; private set; }

		public UltimaStoreGump(PlayerMobile pm, StoreEntry forcedEntry = null)
			: base(pm, 100, 200)
		{
			StoreList = UltimaStore.GetList(Category, forcedEntry);

			if (forcedEntry == null)
			{
				UltimaStore.SortList(StoreList, SortBy);
			}

			pm.Frozen = true;
			pm.Hidden = true;
			pm.Squelched = true;
		}

		public override void OnDispose()
		{
			StoreList.Clear();
			StoreList.TrimExcess();

			StoreList = null;
		}

		public override void AddGumpLayout()
		{
			AddPage(0);
			AddImage(0, 0, 0x9C49);

			AddECHandleInput();

			AddButton(36, 97, Category == StoreCategory.Featured ? 0x9C5F : 0x9C55, 0x9C5F, 100, GumpButtonType.Reply, 0);
			AddHtmlLocalized(36, 100, 125, 25, 1114513, "#1156587", 0x7FFF, false, false); // Featured

			AddECHandleInput();
			AddECHandleInput();

			AddButton(36, 126, Category == StoreCategory.Character ? 0x9C5F : 0x9C55, 0x9C5F, 101, GumpButtonType.Reply, 0);
			AddHtmlLocalized(36, 129, 125, 25, 1114513, "#1156588", 0x7FFF, false, false); // Character

			AddECHandleInput();
			AddECHandleInput();

			AddButton(36, 155, Category == StoreCategory.Equipment ? 0x9C5F : 0x9C55, 0x9C5F, 102, GumpButtonType.Reply, 0);
			AddHtmlLocalized(36, 158, 125, 25, 1114513, "#1078237", 0x7FFF, false, false); // Equipment

			AddECHandleInput();
			AddECHandleInput();

			AddButton(36, 184, Category == StoreCategory.Decorations ? 0x9C5F : 0x9C55, 0x9C5F, 103, GumpButtonType.Reply, 0);
			AddHtmlLocalized(36, 187, 125, 25, 1114513, "#1044501", 0x7FFF, false, false); // Decorations

			AddECHandleInput();
			AddECHandleInput();

			AddButton(36, 213, Category == StoreCategory.Mounts ? 0x9C5F : 0x9C55, 0x9C5F, 104, GumpButtonType.Reply, 0);
			AddHtmlLocalized(36, 216, 125, 25, 1114513, "#1154981", 0x7FFF, false, false); // Mounts

			AddECHandleInput();
			AddECHandleInput();

			AddButton(36, 242, Category == StoreCategory.Misc ? 0x9C5F : 0x9C55, 0x9C5F, 105, GumpButtonType.Reply, 0);
			AddHtmlLocalized(36, 245, 125, 25, 1114513, "#1011173", 0x7FFF, false, false); // Miscellaneous

			AddECHandleInput();
			AddECHandleInput();

			AddButton(36, 271, 0x9C55, 0x9C5F, 106, GumpButtonType.Reply, 0);
			AddHtmlLocalized(36, 274, 125, 25, 1114513, "#1156589", 0x7FFF, false, false); // Promotional Code

			AddECHandleInput();
			AddECHandleInput();

			AddButton(36, 300, 0x9C55, 0x9C5F, 107, GumpButtonType.Reply, 0);
			AddHtmlLocalized(36, 303, 125, 25, 1114513, "#1156875", 0x7FFF, false, false); // FAQ

			AddECHandleInput();

			AddImage(36, 331, 0x9C4A);

			AddHtmlLocalized(36, 334, 125, 25, 1114513, "#1044580", 0x2945, false, false); // Sort By

			AddButton(43, 360, SortBy == SortBy.Name ? 0x9C4F : 0x9C4E, SortBy == SortBy.Name ? 0x9C4F : 0x9C4E, 108, GumpButtonType.Reply, 0);
			AddHtmlLocalized(68, 360, 88, 25, 1037013, 0x6B55, false, false); // Name

			AddButton(43, 386, SortBy == SortBy.PriceLower ? 0x9C4F : 0x9C4E, SortBy == SortBy.PriceLower ? 0x9C4F : 0x9C4E, 109, GumpButtonType.Reply, 0);
			AddHtmlLocalized(68, 386, 88, 25, 1062218, 0x6B55, false, false); // Price Down
			AddImage(110, 386, 0x9C60);

			AddButton(43, 412, SortBy == SortBy.PriceHigher ? 0x9C4F : 0x9C4E, SortBy == SortBy.PriceHigher ? 0x9C4F : 0x9C4E, 110, GumpButtonType.Reply, 0);
			AddHtmlLocalized(68, 412, 88, 25, 1062218, 0x6B55, false, false); // Price Up
			AddImage(110, 412, 0x9C61);

			AddButton(43, 438, SortBy == SortBy.Newest ? 0x9C4F : 0x9C4E, SortBy == SortBy.Newest ? 0x9C4F : 0x9C4E, 111, GumpButtonType.Reply, 0);
			AddHtmlLocalized(68, 438, 88, 25, 1156590, 0x6B55, false, false); // Newest

			AddButton(43, 464, SortBy == SortBy.Oldest ? 0x9C4F : 0x9C4E, SortBy == SortBy.Oldest ? 0x9C4F : 0x9C4E, 112, GumpButtonType.Reply, 0);
			AddHtmlLocalized(68, 464, 88, 25, 1156591, 0x6B55, false, false); // Oldest

			AddECHandleInput();

			AddButton(598, 36, Category == StoreCategory.Cart ? 0x9C5E : 0x9C54, 0x9C5E, 113, GumpButtonType.Reply, 0);
			AddHtmlLocalized(628, 39, 123, 25, 1156593, String.Format("@{0}@{1}", UltimaStore.CartCount(User), Configuration.CartCapacity), 0x7FFF, false, false);

			AddECHandleInput();

			AddBackground(167, 516, 114, 22, 0x2486);
			AddTextEntry(169, 518, 110, 18, 0, 0, "", 169);

			AddECHandleInput();

			AddButton(286, 516, 0x9C52, 0x9C5C, 114, GumpButtonType.Reply, 0);
			AddHtmlLocalized(286, 519, 64, 22, 1114513, "#1154641", 0x7FFF, false, false); // Search

			AddECHandleInput();

			AddImage(36, 74, 0x9C56);
			AddLabelCropped(59, 74, 100, 14, 0x1C7, UltimaStore.GetCurrency(User).ToString("N0"));

			AddECHandleInput();

			if (!Search && Category == StoreCategory.Cart)
			{
				var profile = UltimaStore.GetProfile(User);

				AddImage(167, 74, 0x9C4C);

				if (profile != null && profile.Cart != null && profile.Cart.Count > 0)
				{
					var i = 0;

					foreach (var kvp in profile.Cart)
					{
						var entry = kvp.Key;
						var amount = kvp.Value;

						var index = UltimaStore.Entries.IndexOf(entry);

						if (entry.Name[0].Number > 0)
						{
							AddHtmlLocalized(175, 84 + (35 * i), 256, 25, entry.Name[0].Number, 0x6B55, false, false);
						}
						else
						{
							AddHtml(175, 84 + (35 * i), 256, 25, SetColor((short)0x6B55, entry.Name[0].String), false, false);
						}

						AddButton(431, 81 + (35 * i), 0x9C52, 0x9C5C, index + 2000, GumpButtonType.Reply, 0);

						AddLabelCropped(457, 82 + (35 * i), 38, 22, 0x9C2, amount.ToString());
						AddLabelCropped(531, 82 + (35 * i), 100, 14, 0x1C7, (entry.Cost * amount).ToString("N0"));

						AddButton(653, 81 + (35 * i), 0x9C52, 0x9C5C, index + 3000, GumpButtonType.Reply, 0);
						AddHtmlLocalized(653, 84 + (35 * i), 64, 22, 1114513, "#1011403", 0x7FFF, false, false); // Remove

						AddImage(175, 109 + (35 * i), 0x9C4D);

						++i;
					}
				}

				AddHtmlLocalized(508, 482, 125, 25, 1156594, 0x6B55, false, false); // Subtotal:
				AddImage(588, 482, 0x9C56);
				AddLabelCropped(611, 480, 100, 14, 0x1C7, UltimaStore.GetSubTotal(Cart).ToString("N0"));

				AddECHandleInput();
				AddECHandleInput();

				AddButton(653, 516, 0x9C52, 0x9C52, 115, GumpButtonType.Reply, 0);
				AddHtmlLocalized(653, 519, 64, 22, 1114513, "#1062219", 0x7FFF, false, false); // Buy
			}
			else
			{
				if (Search)
				{
					StoreList = UltimaStore.GetSortedList(SearchText);

					UltimaStore.SortList(StoreList, SortBy);

					if (StoreList.Count == 0)
					{
						User.SendLocalizedMessage(1154587, "", 1281); // No items matched your search.
						return;
					}
				}

				var listIndex = Page * 6;
				var pageIndex = 0;
				var pages = (int)Math.Ceiling((double)StoreList.Count / 6);

				for (var i = listIndex; i < StoreList.Count && pageIndex < 6; i++)
				{
					var entry = StoreList[i];
					var x = _Offset[pageIndex][0];
					var y = _Offset[pageIndex][1];

					AddButton(x, y, 0x9C4B, 0x9C4B, i + 1000, GumpButtonType.Reply, 0);

					if (entry.Tooltip > 0)
					{
						AddTooltip(entry.Tooltip);
					}
					else
					{
						var item = UltimaStore.UltimaStoreContainer.FindDisplayItem(entry.ItemType);

						if (item != null)
						{
							AddItemProperty(item);
						}
					}

					if (IsFeatured(entry))
					{
						AddImage(x, y + 189, 0x9C58);
					}

					for (var j = 0; j < entry.Name.Length; j++)
					{
						if (entry.Name[j].Number > 0)
						{
							AddHtmlLocalized(x, y + (j * 14) + 4, 183, 25, 1114513, String.Format("#{0}", entry.Name[j].Number.ToString()), 0x7FFF, false, false);
						}
						else
						{
							AddHtml(x, y + (j * 14) + 4, 183, 25, ColorAndCenter(0xFFFFFF, entry.Name[j].String), false, false);
						}
					}

					if (entry.ItemID > 0)
					{
						var b = ItemBounds.Table[entry.ItemID];

						AddItem((x + 91) - b.Width / 2 - b.X, (y + 108) - b.Height / 2 - b.Y, entry.ItemID, entry.Hue);
					}
					else
					{
						AddImage((x + 91) - 72, (y + 108) - 72, entry.GumpID);
					}

					AddImage(x + 60, y + 192, 0x9C56);
					AddLabelCropped(x + 80, y + 190, 143, 25, 0x9C2, entry.Cost.ToString("N0"));

					AddECHandleInput();
					AddECHandleInput();

					++pageIndex;
					++listIndex;
				}

				if (Page + 1 < pages)
				{
					AddButton(692, 516, 0x9C51, 0x9C5B, 116, GumpButtonType.Reply, 0);
				}

				if (Page > 0)
				{
					AddButton(648, 516, 0x9C50, 0x9C5A, 117, GumpButtonType.Reply, 0);
				}
			}

			if (Configuration.CurrencyDisplay)
			{
				AddHtml(43, 496, 120, 16, SetColor(0xFFFFFF, "Currency:"), false, false);
				AddHtml(43, 518, 120, 16, SetColor(0xFFFFFF, Configuration.CurrencyName), false, false);
			}
		}

		public bool IsFeatured(StoreEntry entry)
		{
			return entry.Category == StoreCategory.Featured ||
				UltimaStore.Entries.Any(e => e.ItemType == entry.ItemType && e.Category == StoreCategory.Featured);
		}

		public static void ReleaseHidden(PlayerMobile pm)
		{
			if (pm.HasGump(typeof(UltimaStoreGump)) || pm.HasGump(typeof(NoFundsGump)) ||
				pm.HasGump(typeof(ConfirmPurchaseGump)) || pm.HasGump(typeof(ConfirmCartGump)))
			{
				return;
			}

			pm.Frozen = false;
			pm.Squelched = false;
			pm.SendLocalizedMessage(501235, "", 0x35); // Help request aborted.

			if (pm.AccessLevel < AccessLevel.Counselor)
			{
				pm.RevealingAction();
			}
		}

		public override void OnServerClose(NetState owner)
		{
			if (owner.Mobile is PlayerMobile)
			{
				ReleaseHidden((PlayerMobile)owner.Mobile);
			}
		}

		public override void OnResponse(RelayInfo info)
		{
			var id = info.ButtonID;

			if (id == 0)
			{
				ReleaseHidden(User);
				return;
			}

			var profile = UltimaStore.GetProfile(User);

			switch (id)
			{
				// Change Category
				case 100:
				case 101:
				case 102:
				case 103:
				case 104:
				case 105:
					{
						Search = false;

						var oldCat = profile.Category;

						profile.Category = (StoreCategory)id - 99;

						if (oldCat != profile.Category)
						{
							StoreList = UltimaStore.GetList(Category);
							Page = 0;
						}

						Refresh();
						return;
					}

				// Promo Code
				case 106:
					{
						Refresh();
						SendGump(new PromoCodeGump(User, this));
						return;
					}

				// FAQ
				case 107:
					{
						if (!String.IsNullOrWhiteSpace(Configuration.Website))
						{
							User.LaunchBrowser(Configuration.Website);
						}
						else
						{
							User.LaunchBrowser("https://uo.com/ultima-store/");
						}

						Refresh();
						return;
					}

				// Change Sort Method
				case 108:
				case 109:
				case 110:
				case 111:
				case 112:
					{
						var oldSort = profile.SortBy;

						profile.SortBy = (SortBy)id - 108;

						if (oldSort != profile.SortBy)
						{
							// re-orders the list
							if (oldSort == SortBy.Newest || oldSort == SortBy.Oldest)
							{
								StoreList.Clear();
								StoreList.TrimExcess();

								StoreList = UltimaStore.GetList(Category);
							}

							UltimaStore.SortList(StoreList, profile.SortBy);

							Page = 0;
						}

						Refresh();
						return;
					}

				// Cart View
				case 113:
					{
						if (profile != null)
						{
							profile.Category = StoreCategory.Cart;
						}

						Refresh();
						return;
					}

				// Search
				case 114:
					{
						var searchTxt = info.GetTextEntry(0);

						if (searchTxt != null && !String.IsNullOrEmpty(searchTxt.Text))
						{
							Search = true;
							SearchText = searchTxt.Text;
						}
						else
						{
							User.SendLocalizedMessage(1150315); // That text is unacceptable.
						}

						Refresh();
						return;
					}

				// Buy
				case 115:
					{
						if (UltimaStore.CartCount(User) == 0)
						{
							if (profile != null)
							{
								profile.Category = StoreCategory.Cart;
							}

							Refresh();
							return;
						}

						var total = UltimaStore.GetSubTotal(Cart);

						if (total <= UltimaStore.GetCurrency(User, true))
						{
							SendGump(new ConfirmPurchaseGump(User));
						}
						else
						{
							SendGump(new NoFundsGump(User));
						}

						return;
					}

				// Next Page
				case 116:
					{
						++Page;

						Refresh();
						return;
					}

				// Previous Page
				case 117:
					{
						--Page;

						Refresh();
						return;
					}
			}

			if (id < 2000) // Add To Cart
			{
				Refresh();

				var entry = StoreList[id - 1000];

				if (Cart == null || Cart.Count < 10)
				{
					SendGump(new ConfirmCartGump(User, this, entry));
					return;
				}

				User.SendLocalizedMessage(1156745); // Your store cart is currently full.
			}
			else if (id < 3000) // Change Amount In Cart
			{
				Refresh();

				var entry = UltimaStore.Entries[id - 2000];

				SendGump(new ConfirmCartGump(User, this, entry, Cart != null && Cart.ContainsKey(entry) ? Cart[entry] : 0));
				return;
			}
			else if (id < 4000) // Remove From Cart
			{
				var entry = UltimaStore.Entries[id - 3000];

				if (profile != null)
				{
					profile.RemoveFromCart(entry);
				}

				Refresh();
				return;
			}

			ReleaseHidden(User);
		}
	}

	public class ConfirmCartGump : BaseGump
	{
		public UltimaStoreGump Gump { get; private set; }
		public StoreEntry Entry { get; private set; }
		public int Current { get; private set; }

		public ConfirmCartGump(PlayerMobile pm, UltimaStoreGump gump, StoreEntry entry, int current = 0)
			: base(pm, gump.X + (760 / 2) - 205, gump.Y + (574 / 2) - 100)
		{
			Gump = gump;
			Entry = entry;
			Current = current;

			pm.CloseGump(typeof(ConfirmCartGump));
		}

		public override void AddGumpLayout()
		{
			AddBackground(0, 0, 410, 200, 0x9C40);
			AddHtmlLocalized(10, 10, 400, 20, 1114513, "#1077826", 0x7FFF, false, false); // Quantity

			for (var i = 0; i < Entry.Name.Length; i++)
			{
				if (Entry.Name[i].Number > 0)
				{
					AddHtmlLocalized(10, 60 + (i * 14), 400, 20, 1114513, String.Format("#{0}", Entry.Name[i].Number), 0x6B45, false, false);
				}
				else
				{
					AddHtml(10, 60 + (i * 14), 400, 20, ColorAndCenter((short)0x6B45, Entry.Name[i].String), false, false);
				}
			}

			AddHtmlLocalized(30, 100, 200, 20, 1114514, "#1150152", 0x7FFF, false, false); // Quantity to Buy:

			AddBackground(233, 100, 50, 20, 0x2486);
			AddTextEntry(238, 100, 50, 20, 0, 0, Current > 0 ? Current.ToString() : "", 2);

			AddECHandleInput();

			AddButton(45, 150, 0x9C53, 0x9C5D, 195, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 153, 126, 25, 1114513, "#1156596", 0x7FFF, false, false); // Okay

			AddECHandleInput();
			AddECHandleInput();

			AddButton(240, 150, 0x9C53, 0x9C5D, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(240, 153, 126, 25, 1114513, "#1006045", 0x7FFF, false, false); // Cancel

			AddECHandleInput();
		}

		public override void OnServerClose(NetState owner)
		{
			if (owner.Mobile is PlayerMobile)
			{
				UltimaStoreGump.ReleaseHidden(User);
			}
		}

		public override void OnResponse(RelayInfo info)
		{
			if (info.ButtonID == 195)
			{
				var amtText = info.GetTextEntry(0);

				if (amtText != null && !String.IsNullOrWhiteSpace(amtText.Text))
				{
					var amount = Utility.ToInt32(amtText.Text);

					if (amount > 0)
					{
						if (amount <= 10)
						{
							UltimaStore.GetProfile(User).SetCartAmount(Entry, amount);
						}
						else
						{
							User.SendLocalizedMessage(1150315); // That text is unacceptable.
																//User.SendLocalizedMessage(1156836); // You can't exceed 125 items per purchase. 
						}

						Gump.Refresh();
					}
				}
				else
				{
					User.SendLocalizedMessage(1150315); // That text is unacceptable.
				}
			}

			UltimaStoreGump.ReleaseHidden(User);
		}
	}

	public class ConfirmPurchaseGump : BaseGump
	{
		public ConfirmPurchaseGump(PlayerMobile pm)
			: base(pm, 150, 150)
		{
			pm.CloseGump(typeof(ConfirmPurchaseGump));
		}

		public override void AddGumpLayout()
		{
			AddPage(0);

			AddBackground(0, 0, 410, 200, 0x9C40);
			AddHtmlLocalized(10, 10, 400, 20, 1114513, "#1156750", 0x7FFF, false, false); // Purchase Confirmation

			AddHtmlLocalized(30, 60, 350, 60, 1156749, 0x7FFF, false, false); // Are you sure you want to complete this purchase?

			AddECHandleInput();

			AddButton(45, 150, 0x9C53, 0x9C5D, 195, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 153, 126, 25, 1114513, "#1156596", 0x7FFF, false, false); // Okay

			AddECHandleInput();
			AddECHandleInput();

			AddButton(240, 150, 0x9C53, 0x9C5D, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(240, 153, 126, 25, 1114513, "#1006045", 0x7FFF, false, false); // Cancel

			AddECHandleInput();
		}

		public override void OnServerClose(NetState owner)
		{
			if (owner.Mobile is PlayerMobile)
			{
				UltimaStoreGump.ReleaseHidden(User);
			}
		}

		public override void OnResponse(RelayInfo info)
		{
			if (info.ButtonID == 195)
			{
				UltimaStore.TryPurchase(User);
			}

			UltimaStoreGump.ReleaseHidden(User);
		}
	}

	public class NoFundsGump : BaseGump
	{
		public NoFundsGump(PlayerMobile pm)
			: base(pm, 150, 150)
		{
			pm.CloseGump(typeof(NoFundsGump));
		}

		public override void AddGumpLayout()
		{
			AddPage(0);

			AddBackground(0, 0, 410, 200, 0x9C40);
			AddHtmlLocalized(10, 10, 400, 20, 1114513, "#1156747", 0x7FFF, false, false); // Insufficient Funds

			AddHtml(30, 60, 350, 60, SetColor(0xDA0000, String.Format("This transaction cannot be completed due to insufficient funds available. Visit your shards website for more information on how to obtain {0}.", Configuration.CurrencyName)), false, false);

			AddECHandleInput();

			AddButton(45, 150, 0x9C53, 0x9C5D, 195, GumpButtonType.Reply, 0);
			AddHtml(45, 153, 126, 25, ColorAndCenter(0xFFFFFF, "Information"), false, false); // Information

			AddECHandleInput();
			AddECHandleInput();

			AddButton(240, 150, 0x9C53, 0x9C5D, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(240, 153, 126, 25, 1114513, "#1006045", 0x7FFF, false, false); // Cancel

			AddECHandleInput();
		}

		public override void OnServerClose(NetState owner)
		{
			if (owner.Mobile is PlayerMobile)
			{
				UltimaStoreGump.ReleaseHidden(User);
			}
		}

		public override void OnResponse(RelayInfo info)
		{
			if (info.ButtonID == 195)
			{
				if (!String.IsNullOrEmpty(Configuration.Website))
				{
					User.LaunchBrowser(Configuration.Website);
				}
				else
				{
					User.LaunchBrowser("https://uo.com/ultima-store/");
				}
			}

			UltimaStoreGump.ReleaseHidden(User);
		}
	}

	public class PromoCodeGump : BaseGump
	{
		public BaseGump Gump { get; private set; }

		public PromoCodeGump(PlayerMobile pm, BaseGump gump)
			: base(pm, 10, 10)
		{
			Gump = gump;

			pm.CloseGump(typeof(PromoCodeGump));
		}

		public override void AddGumpLayout()
		{
			AddPage(0);

			AddBackground(0, 0, 400, 340, 0x9C40);

			AddHtmlLocalized(0, 10, 400, 20, 1114513, "#1062516", 0x7FFF, false, false); // Enter Promotional Code
			AddHtmlLocalized(20, 60, 355, 160, 1062869, 0x7FFF, false, true); // Enter your promotional code EXACTLY as it was given to you (including dashes). Enter no other text in the box aside from your promotional code.

			AddECHandleInput();

			AddBackground(80, 220, 240, 22, 0x2486);
			AddTextEntry(81, 220, 239, 20, 0, 0, "");

			AddButton(40, 260, 0x9C53, 0x9C5D, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(40, 262, 125, 25, 1114513, "#1156596", 0x7FFF, false, false);

			AddECHandleInput();
			AddECHandleInput();

			AddButton(234, 260, 0x9C53, 0x9C5D, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(234, 262, 126, 25, 1114513, "#1006045", 0x7FFF, false, false);

			AddECHandleInput();
		}

		public override void OnServerClose(NetState owner)
		{
			if (owner.Mobile is PlayerMobile)
			{
				UltimaStoreGump.ReleaseHidden(User);
			}
		}

		public override void OnResponse(RelayInfo info)
		{
			if (info.ButtonID == 1)
			{
				var text = info.GetTextEntry(0);

				if (text == null || !PromoCodes.TryRedeem(User, text.Text))
				{
					Refresh(true, false);
				}
			}
		}
	}

	public class PromoItemGump : BaseGump
	{
		public int Image { get; private set; }
		public StoreEntry Entry { get; private set; }

		public PromoItemGump(PlayerMobile pm, StoreEntry entry, int image)
			: base(pm, 150, 200)
		{
			Image = image;
			Entry = entry;
		}

		public override void AddGumpLayout()
		{
			AddImage(0, 0, 0x9CE1);

			AddECHandleInput();

			AddButton(76, 88, 0x9C4B, 0x9C4B, 1198, GumpButtonType.Reply, 0);

			if (Entry.Tooltip > 0)
			{
				AddTooltip(Entry.Tooltip);
			}
			else
			{
				var item = UltimaStore.UltimaStoreContainer.FindDisplayItem(Entry.ItemType);

				if (item != null)
				{
					AddItemProperty(item);
				}
			}

			for (var j = 0; j < Entry.Name.Length; j++)
			{
				if (Entry.Name[j].Number > 0)
				{
					AddHtmlLocalized(76, 92 + (j * 14) + 4, 183, 25, 1114513, String.Format("#{0}", Entry.Name[j].Number.ToString()), 0x7FFF, false, false);
				}
				else
				{
					AddHtml(76, 92 + (j * 14) + 4, 183, 25, ColorAndCenter(0xFFFFFF, Entry.Name[j].String), false, false);
				}
			}

			AddImage(94, 126, Image);
			AddImage(136, 280, 0x9C56);

			AddLabelCropped(156, 278, 143, 25, 0x9C2, Entry.Cost.ToString());

			AddECHandleInput();
		}

		public override void OnResponse(RelayInfo info)
		{
			if (info.ButtonID == 1198)
			{
				var profile = UltimaStore.GetProfile(User, false);

				if (profile != null)
				{
					profile.Category = StoreCategory.None;
				}

				UltimaStore.OpenStore(User, Entry);
			}
		}
	}

	#endregion
}