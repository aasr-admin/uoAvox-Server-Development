using Server.Accounting;
using Server.ContextMenus;
using Server.Guilds;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Prompts;
using Server.Regions;
using Server.Spells;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Server.Multis
{
	public abstract class BaseHouse : BaseMulti
	{
		public static bool NewVendorSystem => Core.AOS;  // Is new player vendor system enabled?

		public const int MaxCoOwners = 15;
		public static int MaxFriends => !Core.AOS ? 50 : 140;
		public static int MaxBans => !Core.AOS ? 50 : 140;

		#region Dynamic decay system
		private DecayLevel m_CurrentStage;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextDecayStage { get; set; }

		public void ResetDynamicDecay()
		{
			m_CurrentStage = DecayLevel.Ageless;
			NextDecayStage = DateTime.MinValue;
		}

		public void SetDynamicDecay(DecayLevel level)
		{
			m_CurrentStage = level;

			if (DynamicDecay.Decays(level))
			{
				NextDecayStage = DateTime.UtcNow + DynamicDecay.GetRandomDuration(level);
			}
			else
			{
				NextDecayStage = DateTime.MinValue;
			}
		}
		#endregion

		public const bool DecayEnabled = true;

		public static void Decay_OnTick()
		{
			for (var i = 0; i < AllHouses.Count; ++i)
			{
				_ = AllHouses[i].CheckDecay();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastRefreshed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RestrictDecay { get; set; }

		public virtual TimeSpan DecayPeriod => TimeSpan.FromDays(5.0);

		public virtual DecayType DecayType
		{
			get
			{
				if (RestrictDecay || !DecayEnabled || DecayPeriod == TimeSpan.Zero)
				{
					return DecayType.Ageless;
				}

				if (m_Owner == null)
				{
					return Core.AOS ? DecayType.Condemned : DecayType.ManualRefresh;
				}

				if (m_Owner.Account is not Account acct)
				{
					return Core.AOS ? DecayType.Condemned : DecayType.ManualRefresh;
				}

				if (acct.AccessLevel >= AccessLevel.GameMaster)
				{
					return DecayType.Ageless;
				}

				for (var i = 0; i < acct.Length; ++i)
				{
					var mob = acct[i];

					if (mob != null && mob.AccessLevel >= AccessLevel.GameMaster)
					{
						return DecayType.Ageless;
					}
				}

				if (!Core.AOS)
				{
					return DecayType.ManualRefresh;
				}

				if (acct.Inactive)
				{
					return DecayType.Condemned;
				}

				var allHouses = new List<BaseHouse>();

				for (var i = 0; i < acct.Length; ++i)
				{
					var mob = acct[i];

					if (mob != null)
					{
						allHouses.AddRange(GetHouses(mob));
					}
				}

				BaseHouse newest = null;

				for (var i = 0; i < allHouses.Count; ++i)
				{
					var check = allHouses[i];

					if (newest == null || IsNewer(check, newest))
					{
						newest = check;
					}
				}

				if (this == newest)
				{
					return DecayType.AutoRefresh;
				}

				return DecayType.ManualRefresh;
			}
		}

		public bool IsNewer(BaseHouse check, BaseHouse house)
		{
			var checkTime = check.LastTraded > check.BuiltOn ? check.LastTraded : check.BuiltOn;
			var houseTime = house.LastTraded > house.BuiltOn ? house.LastTraded : house.BuiltOn;

			return checkTime > houseTime;
		}

		public virtual bool CanDecay
		{
			get
			{
				var type = DecayType;

				return type is DecayType.Condemned or DecayType.ManualRefresh;
			}
		}

		private DecayLevel m_LastDecayLevel;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual DecayLevel DecayLevel
		{
			get
			{
				DecayLevel result;

				if (!CanDecay)
				{
					if (DynamicDecay.Enabled)
					{
						ResetDynamicDecay();
					}

					LastRefreshed = DateTime.UtcNow;
					result = DecayLevel.Ageless;
				}
				else if (DynamicDecay.Enabled)
				{
					var stage = m_CurrentStage;

					if (stage == DecayLevel.Ageless || (DynamicDecay.Decays(stage) && NextDecayStage <= DateTime.UtcNow))
					{
						SetDynamicDecay(++stage);
					}

					if (stage == DecayLevel.Collapsed && (HasRentedVendors || VendorInventories.Count > 0))
					{
						result = DecayLevel.DemolitionPending;
					}
					else
					{
						result = stage;
					}
				}
				else
				{
					result = GetOldDecayLevel();
				}

				if (result != m_LastDecayLevel)
				{
					m_LastDecayLevel = result;

					if (Sign != null && !Sign.GettingProperties)
					{
						Sign.InvalidateProperties();
					}
				}

				return result;
			}
		}

		public DecayLevel GetOldDecayLevel()
		{
			var timeAfterRefresh = DateTime.UtcNow - LastRefreshed;
			var percent = (int)(timeAfterRefresh.Ticks * 1000 / DecayPeriod.Ticks);

			if (percent >= 1000) // 100.0%
			{
				return (HasRentedVendors || VendorInventories.Count > 0) ? DecayLevel.DemolitionPending : DecayLevel.Collapsed;
			}
			else if (percent >= 950) // 95.0% - 99.9%
			{
				return DecayLevel.IDOC;
			}
			else if (percent >= 750) // 75.0% - 94.9%
			{
				return DecayLevel.Greatly;
			}
			else if (percent >= 500) // 50.0% - 74.9%
			{
				return DecayLevel.Fairly;
			}
			else if (percent >= 250) // 25.0% - 49.9%
			{
				return DecayLevel.Somewhat;
			}
			else if (percent >= 005) // 00.5% - 24.9%
			{
				return DecayLevel.Slightly;
			}

			return DecayLevel.LikeNew;
		}

		public virtual bool RefreshDecay()
		{
			if (DecayType == DecayType.Condemned)
			{
				return false;
			}

			var oldLevel = DecayLevel;

			LastRefreshed = DateTime.UtcNow;

			if (DynamicDecay.Enabled)
			{
				ResetDynamicDecay();
			}

			if (Sign != null)
			{
				Sign.InvalidateProperties();
			}

			return oldLevel > DecayLevel.LikeNew;
		}

		public virtual bool CheckDecay()
		{
			if (!Deleted && DecayLevel == DecayLevel.Collapsed)
			{
				_ = Timer.DelayCall(TimeSpan.Zero, Decay_Sandbox);
				return true;
			}

			return false;
		}

		public virtual void KillVendors()
		{
			foreach (var vendor in PlayerVendors.ToArray())
			{
				vendor.Destroy(true);
			}

			PlayerVendors.Clear();

			foreach (PlayerBarkeeper barkeeper in PlayerBarkeepers.ToArray())
			{
				barkeeper.Delete();
			}

			PlayerBarkeepers.Clear();
		}

		public virtual void Decay_Sandbox()
		{
			if (Deleted)
			{
				return;
			}

			if (Core.ML)
			{
				_ = new TempNoHousingRegion(this, null);
			}

			KillVendors();
			Delete();
		}

		public virtual TimeSpan RestrictedPlacingTime => TimeSpan.FromHours(1.0);

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual double BonusStorageScalar => Core.ML ? 1.2 : 1.0;

		private bool m_Public;

		private HouseRegion m_Region;
		private TrashBarrel m_Trash;

		private Mobile m_Owner;
		private Point3D m_RelativeBanLocation;

		private static readonly Dictionary<Mobile, List<BaseHouse>> m_Table = new();

		public virtual bool IsAosRules => Core.AOS;

		public virtual bool IsActive => true;

		public virtual HousePlacementEntry GetAosEntry()
		{
			return HousePlacementEntry.Find(this);
		}

		public virtual int GetAosMaxSecures()
		{
			var hpe = GetAosEntry();

			if (hpe == null)
			{
				return 0;
			}

			return (int)(hpe.Storage * BonusStorageScalar);
		}

		public virtual int GetAosMaxLockdowns()
		{
			var hpe = GetAosEntry();

			if (hpe == null)
			{
				return 0;
			}

			return (int)(hpe.Lockdowns * BonusStorageScalar);
		}

		public virtual int GetAosCurSecures(out int fromSecures, out int fromVendors, out int fromLockdowns, out int fromMovingCrate)
		{
			fromSecures = 0;
			fromVendors = 0;
			fromLockdowns = 0;
			fromMovingCrate = 0;

			var list = Secures;

			if (list != null)
			{
				foreach (var si in list)
				{
					fromSecures += si.Item.TotalItems;
				}

				fromLockdowns += list.Count;
			}

			fromLockdowns += GetLockdowns();

			if (!NewVendorSystem)
			{
				foreach (var vendor in PlayerVendors)
				{
					if (vendor.Backpack != null)
					{
						fromVendors += vendor.Backpack.TotalItems;
					}
				}
			}

			if (MovingCrate != null)
			{
				fromMovingCrate += MovingCrate.TotalItems;

				foreach (var item in MovingCrate.Items)
				{
					if (item is PackingBox)
					{
						fromMovingCrate--;
					}
				}
			}

			return fromSecures + fromVendors + fromLockdowns + fromMovingCrate;
		}

		public bool InRange(IPoint2D from, int range)
		{
			if (Region == null)
			{
				return false;
			}

			foreach (var rect in Region.Area)
			{
				if (from.X >= rect.Bounds.Start.X - range && from.Y >= rect.Bounds.Start.Y - range && from.X < rect.Bounds.End.X + range && from.Y < rect.Bounds.End.Y + range)
				{
					return true;
				}
			}

			return false;
		}

		public virtual int GetNewVendorSystemMaxVendors()
		{
			var hpe = GetAosEntry();

			if (hpe == null)
			{
				return 0;
			}

			return (int)(hpe.Vendors * BonusStorageScalar);
		}

		public virtual bool CanPlaceNewVendor()
		{
			if (!IsAosRules)
			{
				return true;
			}

			if (!NewVendorSystem)
			{
				return CheckAosLockdowns(10);
			}

			return (PlayerVendors.Count + VendorRentalContracts.Count) < GetNewVendorSystemMaxVendors();
		}

		public const int MaximumBarkeepCount = 2;

		public virtual bool CanPlaceNewBarkeep()
		{
			return PlayerBarkeepers.Count < MaximumBarkeepCount;
		}

		public static void IsThereVendor(Point3D location, Map map, out bool vendor, out bool rentalContract)
		{
			vendor = false;
			rentalContract = false;

			IPooledEnumerable eable = map.GetObjectsInRange(location, 0);

			foreach (IEntity entity in eable)
			{
				if (Math.Abs(location.Z - entity.Z) <= 16)
				{
					if (entity is PlayerVendor or PlayerBarkeeper or PlayerVendorPlaceholder)
					{
						vendor = true;
						break;
					}

					if (entity is VendorRentalContract)
					{
						rentalContract = true;
						break;
					}
				}
			}

			eable.Free();
		}

		public bool HasPersonalVendors
		{
			get
			{
				foreach (var vendor in PlayerVendors)
				{
					if (vendor is not RentedVendor)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool HasRentedVendors
		{
			get
			{
				foreach (var vendor in PlayerVendors)
				{
					if (vendor is RentedVendor)
					{
						return true;
					}
				}

				return false;
			}
		}

		#region Mondain's Legacy
		public bool HasAddonContainers
		{
			get
			{
				foreach (var item in Addons)
				{
					if (item is BaseAddonContainer)
					{
						return true;
					}
				}

				return false;
			}
		}
		#endregion

		public HashSet<PlayerVendor> AvailableVendorsFor(Mobile m)
		{
			var list = new HashSet<PlayerVendor>();

			foreach (var vendor in PlayerVendors)
			{
				if (vendor.CanInteractWith(m, false))
				{
					_ = list.Add(vendor);
				}
			}

			return list;
		}

		public bool AreThereAvailableVendorsFor(Mobile m)
		{
			foreach (var vendor in PlayerVendors)
			{
				if (vendor.CanInteractWith(m, false))
				{
					return true;
				}
			}

			return false;
		}

		public void MoveAllToCrate()
		{
			RelocatedEntities.Clear();

			if (MovingCrate != null)
			{
				MovingCrate.Hide();
			}

			if (m_Trash != null)
			{
				m_Trash.Delete();
				m_Trash = null;
			}

			foreach (var item in LockDowns)
			{
				if (!item.Deleted)
				{
					item.IsLockedDown = false;
					item.IsSecure = false;
					item.Movable = true;

					if (item.Parent == null)
					{
						DropToMovingCrate(item);
					}
				}
			}

			LockDowns.Clear();

			foreach (Item item in VendorRentalContracts)
			{
				if (!item.Deleted)
				{
					item.IsLockedDown = false;
					item.IsSecure = false;
					item.Movable = true;

					if (item.Parent == null)
					{
						DropToMovingCrate(item);
					}
				}
			}

			VendorRentalContracts.Clear();

			foreach (var info in Secures)
			{
				Item item = info.Item;

				if (!item.Deleted)
				{
					if (item is StrongBox)
					{
						item = ((StrongBox)item).ConvertToStandardContainer();
					}

					item.IsLockedDown = false;
					item.IsSecure = false;
					item.Movable = true;

					if (item.Parent == null)
					{
						DropToMovingCrate(item);
					}
				}
			}

			Secures.Clear();

			foreach (var addon in Addons)
			{
				if (!addon.Deleted)
				{
					Item deed = null;
					var retainDeedHue = false; //if the items aren't hued but the deed itself is
					var hue = 0;

					if (addon is IAddon)
					{
						deed = ((IAddon)addon).Deed;

						if (addon is BaseAddon && ((BaseAddon)addon).RetainDeedHue) //There are things that are IAddon which aren't BaseAddon
						{
							var ba = (BaseAddon)addon;
							retainDeedHue = true;

							for (var i = 0; hue == 0 && i < ba.Components.Count; ++i)
							{
								var c = ba.Components[i];

								if (c.Hue != 0)
								{
									hue = c.Hue;
								}
							}
						}
					}

					if (deed != null)
					{
						#region Mondain's Legacy
						if (deed is BaseAddonContainerDeed && addon is BaseAddonContainer)
						{
							var c = (BaseAddonContainer)addon;
							c.DropItemsToGround();

							((BaseAddonContainerDeed)deed).Resource = c.Resource;
						}
						else if (deed is BaseAddonDeed && addon is BaseAddon)
						{
							((BaseAddonDeed)deed).Resource = ((BaseAddon)addon).Resource;
						}
						#endregion

						addon.Delete();

						if (retainDeedHue)
						{
							deed.Hue = hue;
						}

						DropToMovingCrate(deed);
					}
					else
					{
						DropToMovingCrate(addon);
					}
				}
			}

			Addons.Clear();

			foreach (var mobile in PlayerVendors)
			{
				mobile.Return();
				mobile.Internalize();
				_ = InternalizedVendors.Add(mobile);
			}

			foreach (var mobile in PlayerBarkeepers)
			{
				mobile.Internalize();
				_ = InternalizedVendors.Add(mobile);
			}
		}

		public List<IEntity> GetHouseEntities()
		{
			var list = new List<IEntity>();

			if (MovingCrate != null)
			{
				MovingCrate.Hide();
			}

			if (m_Trash != null && m_Trash.Map != Map.Internal)
			{
				list.Add(m_Trash);
			}

			foreach (var item in LockDowns)
			{
				if (item.Parent == null && item.Map != Map.Internal)
				{
					list.Add(item);
				}
			}

			foreach (Item item in VendorRentalContracts)
			{
				if (item.Parent == null && item.Map != Map.Internal)
				{
					list.Add(item);
				}
			}

			foreach (var info in Secures)
			{
				Item item = info.Item;

				if (item.Parent == null && item.Map != Map.Internal)
				{
					list.Add(item);
				}
			}

			foreach (var item in Addons)
			{
				if (item.Parent == null && item.Map != Map.Internal)
				{
					list.Add(item);
				}
			}

			foreach (var mobile in PlayerVendors)
			{
				mobile.Return();

				if (mobile.Map != Map.Internal)
				{
					list.Add(mobile);
				}
			}

			foreach (var mobile in PlayerBarkeepers)
			{
				if (mobile.Map != Map.Internal)
				{
					list.Add(mobile);
				}
			}

			return list;
		}

		public void RelocateEntities()
		{
			foreach (var entity in GetHouseEntities())
			{
				var relLoc = new Point3D(entity.X - X, entity.Y - Y, entity.Z - Z);
				var relocEntity = new RelocatedEntity(entity, relLoc);

				_ = RelocatedEntities.Add(relocEntity);

				if (entity is Item)
				{
					((Item)entity).Internalize();
				}
				else
				{
					((Mobile)entity).Internalize();
				}
			}
		}

		public void RestoreRelocatedEntities()
		{
			foreach (var relocEntity in RelocatedEntities)
			{
				var relLoc = relocEntity.RelativeLocation;
				var location = new Point3D(relLoc.X + X, relLoc.Y + Y, relLoc.Z + Z);

				var entity = relocEntity.Entity;
				if (entity is Item item)
				{
					if (!item.Deleted)
					{
						if (item is IAddon)
						{
							if (((IAddon)item).CouldFit(location, Map))
							{
								item.MoveToWorld(location, Map);
								continue;
							}
						}
						else
						{
							int height;
							bool requireSurface;
							if (item is VendorRentalContract)
							{
								height = 16;
								requireSurface = true;
							}
							else
							{
								height = item.ItemData.Height;
								requireSurface = false;
							}

							if (Map.CanFit(location.X, location.Y, location.Z, height, false, false, requireSurface))
							{
								item.MoveToWorld(location, Map);
								continue;
							}
						}

						// The item can't fit

						if (item is TrashBarrel)
						{
							item.Delete(); // Trash barrels don't go to the moving crate
						}
						else
						{
							SetLockdown(item, false);
							item.IsSecure = false;
							item.Movable = true;

							var relocateItem = item;

							if (item is StrongBox)
							{
								relocateItem = ((StrongBox)item).ConvertToStandardContainer();
							}

							if (item is IAddon)
							{
								var deed = ((IAddon)item).Deed;
								var retainDeedHue = false; //if the items aren't hued but the deed itself is
								var hue = 0;

								if (item is BaseAddon && ((BaseAddon)item).RetainDeedHue)   //There are things that are IAddon which aren't BaseAddon
								{
									var ba = (BaseAddon)item;
									retainDeedHue = true;

									for (var i = 0; hue == 0 && i < ba.Components.Count; ++i)
									{
										var c = ba.Components[i];

										if (c.Hue != 0)
										{
											hue = c.Hue;
										}
									}
								}

								#region Mondain's Legacy
								if (deed != null)
								{
									if (deed is BaseAddonContainerDeed && item is BaseAddonContainer)
									{
										var c = (BaseAddonContainer)item;
										c.DropItemsToGround();

										((BaseAddonContainerDeed)deed).Resource = c.Resource;
									}
									else if (deed is BaseAddonDeed && item is BaseAddon)
									{
										((BaseAddonDeed)deed).Resource = ((BaseAddon)item).Resource;
									}

									if (retainDeedHue)
									{
										deed.Hue = hue;
									}
								}
								#endregion

								relocateItem = deed;
								item.Delete();
							}

							if (relocateItem != null)
							{
								DropToMovingCrate(relocateItem);
							}
						}
					}

					if (m_Trash == item)
					{
						m_Trash = null;
					}

					if (item is VendorRentalContract vrc)
					{
						_ = VendorRentalContracts.Remove(vrc);
					}

					_ = LockDowns.Remove(item);
					_ = Addons.Remove(item);

					_ = Secures.RemoveWhere(si => si.Item == item);
				}
				else
				{
					var mobile = (Mobile)entity;

					if (!mobile.Deleted)
					{
						if (Map.CanFit(location, 16, false, false))
						{
							mobile.MoveToWorld(location, Map);
						}
						else
						{
							_ = InternalizedVendors.Add(mobile);
						}
					}
				}
			}

			RelocatedEntities.Clear();
		}

		public void DropToMovingCrate(Item item)
		{
			MovingCrate ??= new MovingCrate(this);

			MovingCrate.DropItem(item);
		}

		public List<Item> GetItems()
		{
			if (Map == null || Map == Map.Internal)
			{
				return new List<Item>();
			}

			var start = new Point2D(X + Components.Min.X, Y + Components.Min.Y);
			var end = new Point2D(X + Components.Max.X + 1, Y + Components.Max.Y + 1);
			var rect = new Rectangle2D(start, end);

			var list = new List<Item>();

			IPooledEnumerable eable = Map.GetItemsInBounds(rect);

			foreach (Item item in eable)
			{
				if (item.Movable && IsInside(item))
				{
					list.Add(item);
				}
			}

			eable.Free();

			return list;
		}

		public List<Mobile> GetMobiles()
		{
			if (Map == null || Map == Map.Internal)
			{
				return new List<Mobile>();
			}

			var list = new List<Mobile>();

			foreach (var mobile in Region.GetMobiles())
			{
				if (IsInside(mobile))
				{
					list.Add(mobile);
				}
			}

			return list;
		}

		public virtual bool CheckAosLockdowns(int need)
		{
			return (GetAosCurLockdowns() + need) <= GetAosMaxLockdowns();
		}

		public virtual bool CheckAosStorage(int need)
		{
			int fromSecures, fromVendors, fromLockdowns, fromMovingCrate;

			return (GetAosCurSecures(out _, out _, out _, out _) + need) <= GetAosMaxSecures();
		}

		public static void Configure()
		{
			Item.LockedDownFlag = 1;
			Item.SecureFlag = 2;

			_ = Timer.DelayCall(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0), Decay_OnTick);
		}

		public virtual int GetAosCurLockdowns()
		{
			var v = 0;

			v += GetLockdowns();

			if (Secures != null)
			{
				v += Secures.Count;
			}

			if (!NewVendorSystem)
			{
				v += PlayerVendors.Count * 10;
			}

			return v;
		}

		public static bool CheckLockedDown(Item item)
		{
			var house = FindHouseAt(item);

			return house != null && house.IsLockedDown(item);
		}

		public static bool CheckSecured(Item item)
		{
			var house = FindHouseAt(item);

			return house != null && house.IsSecure(item);
		}

		public static bool CheckLockedDownOrSecured(Item item)
		{
			var house = FindHouseAt(item);

			return house != null && (house.IsSecure(item) || house.IsLockedDown(item));
		}

		public static List<BaseHouse> GetHouses(Mobile m)
		{
			var list = new List<BaseHouse>();

			if (m != null)
			{
				List<BaseHouse> exists;
				_ = m_Table.TryGetValue(m, out exists);

				if (exists != null)
				{
					for (var i = 0; i < exists.Count; ++i)
					{
						var house = exists[i];

						if (house != null && !house.Deleted && house.Owner == m)
						{
							list.Add(house);
						}
					}
				}
			}

			return list;
		}

		public static bool CheckHold(Mobile m, Container cont, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			var house = FindHouseAt(cont);

			if (house == null || !house.IsAosRules)
			{
				return true;
			}

			if (house.IsSecure(cont) && !house.CheckAosStorage(1 + item.TotalItems + plusItems))
			{
				if (message)
				{
					m.SendLocalizedMessage(1061839); // This action would exceed the secure storage limit of the house.
				}

				return false;
			}

			return true;
		}

		public static bool CheckAccessible(Mobile m, Item item)
		{
			if (m.AccessLevel >= AccessLevel.GameMaster)
			{
				return true; // Staff can access anything
			}

			var house = FindHouseAt(item);

			if (house == null)
			{
				return true;
			}

			var res = house.CheckSecureAccess(m, item);

			switch (res)
			{
				case SecureAccessResult.Insecure: break;
				case SecureAccessResult.Accessible: return true;
				case SecureAccessResult.Inaccessible: return false;
			}

			if (house.IsLockedDown(item))
			{
				return house.IsCoOwner(m) && (item is Container);
			}

			return true;
		}

		public static BaseHouse FindHouseAt(Mobile m)
		{
			if (m == null || m.Deleted)
			{
				return null;
			}

			return FindHouseAt(m.Location, m.Map, 16);
		}

		public static BaseHouse FindHouseAt(Item item)
		{
			if (item == null || item.Deleted)
			{
				return null;
			}

			return FindHouseAt(item.GetWorldLocation(), item.Map, item.ItemData.Height);
		}

		public static BaseHouse FindHouseAt(Point3D loc, Map map, int height)
		{
			if (map == null || map == Map.Internal)
			{
				return null;
			}

			var sector = map.GetSector(loc);

			for (var i = 0; i < sector.Multis.Count; ++i)
			{
				if (sector.Multis[i] is BaseHouse house && house.IsInside(loc, height))
				{
					return house;
				}
			}

			return null;
		}

		public bool IsInside(Mobile m)
		{
			if (m == null || m.Deleted || m.Map != Map)
			{
				return false;
			}

			return IsInside(m.Location, 16);
		}

		public bool IsInside(Item item)
		{
			if (item == null || item.Deleted || item.Map != Map)
			{
				return false;
			}

			return IsInside(item.Location, item.ItemData.Height);
		}

		public bool CheckAccessibility(Item item, Mobile from)
		{
			var res = CheckSecureAccess(from, item);

			switch (res)
			{
				case SecureAccessResult.Insecure: break;
				case SecureAccessResult.Accessible: return true;
				case SecureAccessResult.Inaccessible: return false;
			}

			if (!IsLockedDown(item))
			{
				return true;
			}
			else if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}
			else if (item is Runebook)
			{
				return true;
			}
			else if (item is ISecurable sec)
			{
				return HasSecureAccess(from, sec.Level);
			}
			else if (item is Container)
			{
				return IsCoOwner(from);
			}
			else if (item.Stackable)
			{
				return true;
			}
			else if (item is BaseLight)
			{
				return IsFriend(from);
			}
			else if (item is PotionKeg)
			{
				return IsFriend(from);
			}
			else if (item is BaseBoard)
			{
				return true;
			}
			else if (item is Dice)
			{
				return true;
			}
			else if (item is RecallRune)
			{
				return true;
			}
			else if (item is TreasureMap)
			{
				return true;
			}
			else if (item is Clock)
			{
				return true;
			}
			else if (item is BaseInstrument)
			{
				return true;
			}
			else if (item is Dyes or DyeTub)
			{
				return true;
			}
			else if (item is VendorRentalContract)
			{
				return true;
			}
			else if (item is RewardBrazier)
			{
				return true;
			}

			return false;
		}

		public virtual bool IsInside(Point3D p, int height)
		{
			if (Deleted)
			{
				return false;
			}

			var mcl = Components;

			var x = p.X - (X + mcl.Min.X);
			var y = p.Y - (Y + mcl.Min.Y);

			if (x < 0 || x >= mcl.Width || y < 0 || y >= mcl.Height)
			{
				return false;
			}

			if (p.Z < Z)
			{
				return Addons.Any(a => a is CellarAddon);
			}

			if (this is HouseFoundation && y < (mcl.Height - 1) && p.Z >= Z)
			{
				return true;
			}

			var tiles = mcl.Tiles[x][y];

			for (var j = 0; j < tiles.Length; ++j)
			{
				var tile = tiles[j];
				var id = tile.ID & TileData.MaxItemValue;
				var data = TileData.ItemTable[id];

				// Slanted roofs do not count; they overhang blocking south and east sides of the multi
				if ((data.Flags & TileFlag.Roof) != 0)
				{
					continue;
				}

				// Signs and signposts are not considered part of the multi
				if (id is (>= 0xB95 and <= 0xC0E) or (>= 0xC43 and <= 0xC44))
				{
					continue;
				}

				var tileZ = tile.Z + Z;

				if (p.Z == tileZ || (p.Z + height) > tileZ)
				{
					return true;
				}
			}

			return false;
		}

		public SecureAccessResult CheckSecureAccess(Mobile m, Item item)
		{
			var sec = GetSecurable(m, item);

			if (sec != null)
			{
				return HasSecureAccess(m, sec.Level) ? SecureAccessResult.Accessible : SecureAccessResult.Inaccessible;
			}

			return SecureAccessResult.Insecure;
		}

		public ISecurable GetSecurable(Mobile from, Item item)
		{
			if (!IsOwner(from) || !IsAosRules)
			{
				return null;
			}

			if (item is not Container && item is ISecurable sec)
			{
				var isOwned = Addons.Contains(item);

				if (!isOwned)
				{
					isOwned = sec is AddonComponent ac && ac.Addon != null && Addons.Contains(ac.Addon);
				}

				if (!isOwned)
				{
					isOwned = sec is BaseDoor door && Doors.Contains(door);
				}

				if (!isOwned)
				{
					isOwned = this is HouseFoundation hf && hf.IsFixture(item);
				}

				if (!isOwned)
				{
					isOwned = IsLockedDown(item);
				}

				if (isOwned)
				{
					return sec;
				}
			}
			else
			{
				var list = Secures;

				if (list != null)
				{
					foreach (var si in list)
					{
						if (si.Item == item)
						{
							return si;
						}
					}
				}
			}

			return null;
		}

		public static List<BaseHouse> AllHouses { get; } = new();

		public BaseHouse(int multiID, Mobile owner, int MaxLockDown, int MaxSecure) : base(multiID)
		{
			AllHouses.Add(this);

			LastRefreshed = DateTime.UtcNow;

			BuiltOn = DateTime.UtcNow;
			LastTraded = DateTime.MinValue;

			m_Owner = owner;

			MaxLockDowns = MaxLockDown;
			MaxSecures = MaxSecure;

			m_RelativeBanLocation = BaseBanLocation;

			UpdateRegion();

			if (owner != null)
			{
				_ = m_Table.TryGetValue(owner, out var list);

				if (list == null)
				{
					m_Table[owner] = list = new();
				}

				list.Add(this);
			}

			Movable = false;
		}

		public BaseHouse(Serial serial) : base(serial)
		{
			AllHouses.Add(this);
		}

		public override void OnMapChange(Map oldMap)
		{
			if (LockDowns == null)
			{
				return;
			}

			UpdateRegion();

			if (Sign != null && !Sign.Deleted)
			{
				Sign.Map = Map;
			}

			if (Doors != null)
			{
				foreach (Item item in Doors)
				{
					item.Map = Map;
				}
			}

			foreach (var entity in GetHouseEntities())
			{
				if (entity is Item)
				{
					((Item)entity).Map = Map;
				}
				else
				{
					((Mobile)entity).Map = Map;
				}
			}
		}

		public virtual void ChangeSignType(int itemID)
		{
			if (Sign != null)
			{
				Sign.ItemID = itemID;
			}
		}

		public abstract Rectangle2D[] Area { get; }
		public abstract Point3D BaseBanLocation { get; }

		public virtual void UpdateRegion()
		{
			if (m_Region != null)
			{
				m_Region.Delete();
				m_Region = null;
			}

			if (!Deleted && Map != null)
			{
				m_Region = new HouseRegion(this);
			}
		}

		public virtual void UpdateRegionArea()
		{
			if (m_Region?.Deleted == false)
			{
				m_Region.Area = HouseRegion.GetArea(this);
			}
			else
			{
				UpdateRegion();
			}
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			if (LockDowns == null)
			{
				return;
			}

			UpdateRegionArea();

			var x = base.Location.X - oldLocation.X;
			var y = base.Location.Y - oldLocation.Y;
			var z = base.Location.Z - oldLocation.Z;

			if (Sign != null && !Sign.Deleted)
			{
				Sign.Location = new Point3D(Sign.X + x, Sign.Y + y, Sign.Z + z);
			}

			if (Doors != null)
			{
				foreach (Item item in Doors)
				{
					if (!item.Deleted)
					{
						item.Location = new Point3D(item.X + x, item.Y + y, item.Z + z);
					}
				}
			}

			foreach (var entity in GetHouseEntities())
			{
				var newLocation = new Point3D(entity.X + x, entity.Y + y, entity.Z + z);

				if (entity is Item)
				{
					((Item)entity).Location = newLocation;
				}
				else
				{
					((Mobile)entity).Location = newLocation;
				}
			}
		}

		public BaseDoor AddEastDoor(int x, int y, int z)
		{
			return AddEastDoor(true, x, y, z);
		}

		public BaseDoor AddEastDoor(bool wood, int x, int y, int z)
		{
			var door = MakeDoor(wood, DoorFacing.SouthCW);

			AddDoor(door, x, y, z);

			return door;
		}

		public BaseDoor AddSouthDoor(int x, int y, int z)
		{
			return AddSouthDoor(true, x, y, z);
		}

		public BaseDoor AddSouthDoor(bool wood, int x, int y, int z)
		{
			var door = MakeDoor(wood, DoorFacing.WestCW);

			AddDoor(door, x, y, z);

			return door;
		}

		public BaseDoor AddEastDoor(int x, int y, int z, uint k)
		{
			return AddEastDoor(true, x, y, z, k);
		}

		public BaseDoor AddEastDoor(bool wood, int x, int y, int z, uint k)
		{
			var door = MakeDoor(wood, DoorFacing.SouthCW);

			door.Locked = true;
			door.KeyValue = k;

			AddDoor(door, x, y, z);

			return door;
		}

		public BaseDoor AddSouthDoor(int x, int y, int z, uint k)
		{
			return AddSouthDoor(true, x, y, z, k);
		}

		public BaseDoor AddSouthDoor(bool wood, int x, int y, int z, uint k)
		{
			var door = MakeDoor(wood, DoorFacing.WestCW);

			door.Locked = true;
			door.KeyValue = k;

			AddDoor(door, x, y, z);

			return door;
		}

		public BaseDoor[] AddSouthDoors(int x, int y, int z, uint k)
		{
			return AddSouthDoors(true, x, y, z, k);
		}

		public BaseDoor[] AddSouthDoors(bool wood, int x, int y, int z, uint k)
		{
			var westDoor = MakeDoor(wood, DoorFacing.WestCW);
			var eastDoor = MakeDoor(wood, DoorFacing.EastCCW);

			westDoor.Locked = true;
			eastDoor.Locked = true;

			westDoor.KeyValue = k;
			eastDoor.KeyValue = k;

			westDoor.Link = eastDoor;
			eastDoor.Link = westDoor;

			AddDoor(westDoor, x, y, z);
			AddDoor(eastDoor, x + 1, y, z);

			return new BaseDoor[2] { westDoor, eastDoor };
		}

		public uint CreateKeys(Mobile m)
		{
			var value = Key.RandomValue();

			if (!IsAosRules)
			{
				var packKey = new Key(KeyType.Gold);
				var bankKey = new Key(KeyType.Gold);

				packKey.KeyValue = value;
				bankKey.KeyValue = value;

				packKey.LootType = LootType.Newbied;
				bankKey.LootType = LootType.Newbied;

				var box = m.BankBox;

				if (!box.TryDropItem(m, bankKey, false))
				{
					bankKey.Delete();
				}

				_ = m.AddToBackpack(packKey);
			}

			return value;
		}

		public BaseDoor[] AddSouthDoors(int x, int y, int z)
		{
			return AddSouthDoors(true, x, y, z, false);
		}

		public BaseDoor[] AddSouthDoors(bool wood, int x, int y, int z, bool inv)
		{
			var westDoor = MakeDoor(wood, inv ? DoorFacing.WestCCW : DoorFacing.WestCW);
			var eastDoor = MakeDoor(wood, inv ? DoorFacing.EastCW : DoorFacing.EastCCW);

			westDoor.Link = eastDoor;
			eastDoor.Link = westDoor;

			AddDoor(westDoor, x, y, z);
			AddDoor(eastDoor, x + 1, y, z);

			return new BaseDoor[2] { westDoor, eastDoor };
		}

		public BaseDoor MakeDoor(bool wood, DoorFacing facing)
		{
			if (wood)
			{
				return new DarkWoodHouseDoor(facing);
			}
			else
			{
				return new MetalHouseDoor(facing);
			}
		}

		public void AddDoor(BaseDoor door, int xoff, int yoff, int zoff)
		{
			door.MoveToWorld(new Point3D(xoff + X, yoff + Y, zoff + Z), Map);
			_ = Doors.Add(door);
		}

		public void AddTrashBarrel(Mobile from)
		{
			if (!IsActive)
			{
				return;
			}

			foreach (var door in Doors)
			{
				var p = door.Location;

				if (door.Open)
				{
					p = new Point3D(p.X - door.Offset.X, p.Y - door.Offset.Y, p.Z - door.Offset.Z);
				}

				if ((from.Z + 16) >= p.Z && (p.Z + 16) >= from.Z)
				{
					if (from.InRange(p, 1))
					{
						from.SendLocalizedMessage(502120); // You cannot place a trash barrel near a door or near steps.
						return;
					}
				}
			}

			if (m_Trash == null || m_Trash.Deleted)
			{
				m_Trash = new TrashBarrel
				{
					Movable = false
				};
				m_Trash.MoveToWorld(from.Location, from.Map);

				from.SendLocalizedMessage(502121); /* You have a new trash barrel.
													  * Three minutes after you put something in the barrel, the trash will be emptied.
													  * Be forewarned, this is permanent! */
			}
			else
			{
				from.SendLocalizedMessage(502117); // You already have a trash barrel!
			}
		}

		public void SetSign(int xoff, int yoff, int zoff)
		{
			Sign = new HouseSign(this);
			Sign.MoveToWorld(new Point3D(X + xoff, Y + yoff, Z + zoff), Map);
		}

		public void SetLockdown(Item i, bool locked)
		{
			if (LockDowns == null)
			{
				return;
			}

			if (i is BaseAddonContainer)
			{
				i.Movable = false;
			}
			else
			{
				i.Movable = !locked;
			}

			i.IsLockedDown = locked;

			if (locked)
			{
				if (i is VendorRentalContract c)
				{
					_ = VendorRentalContracts.Add(c);
				}
				else
				{
					_ = LockDowns.Add(i);
				}
			}
			else
			{
				if (i is VendorRentalContract c)
				{
					_ = VendorRentalContracts.Remove(c);
				}
				else
				{
					_ = LockDowns.Remove(i);
				}
			}

			if (!locked)
			{
				i.SetLastMoved();
			}

			if ((i is Container) && (!locked || !(i is BaseBoard || i is Aquarium || i is FishBowl)))
			{
				foreach (var c in i.Items)
				{
					SetLockdown(c, locked);
				}
			}
		}

		public bool LockDown(Mobile m, Item item)
		{
			return LockDown(m, item, true);
		}

		public bool LockDown(Mobile m, Item item, bool checkIsInside)
		{
			if (!IsCoOwner(m) || !IsActive)
			{
				return false;
			}

			if (item is BaseAddonContainer || (item.Movable && !IsSecure(item)))
			{
				var amt = 1 + item.TotalItems;

				var rootItem = item.RootParent as Item;

				if (checkIsInside && item.RootParent is Mobile)
				{
					m.SendLocalizedMessage(1005525);//That is not in your house
				}
				else if (checkIsInside && !IsInside(item.GetWorldLocation(), item.ItemData.Height))
				{
					m.SendLocalizedMessage(1005525);//That is not in your house
				}
				else if (Ethics.Ethic.IsImbued(item))
				{
					m.SendLocalizedMessage(1005377);//You cannot lock that down
				}
				else if (IsSecure(rootItem))
				{
					m.SendLocalizedMessage(501737); // You need not lock down items in a secure container.
				}
				else if (item.Parent is Item parentItem && !IsLockedDown(parentItem))
				{
					m.SendLocalizedMessage(501736); // You must lockdown the container first!
				}
				else if (item is not VendorRentalContract && (IsAosRules ? (!CheckAosLockdowns(amt) || !CheckAosStorage(amt)) : (LockDownCount + amt) > MaxLockDowns))
				{
					m.SendLocalizedMessage(1005379);//That would exceed the maximum lock down limit for this house
				}
				else
				{
					SetLockdown(item, true);
					return true;
				}
			}
			else if (LockDowns.Contains(item))
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1005526); //That is already locked down
				return true;
			}
			else if (item is HouseSign or Static)
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1005526); // This is already locked down.
			}
			else
			{
				m.SendLocalizedMessage(1005377);//You cannot lock that down
			}

			return false;
		}

		private class TransferItem : Item
		{
			private readonly BaseHouse m_House;

			public override string DefaultName => "a house transfer contract";

			public TransferItem(BaseHouse house) : base(0x14F0)
			{
				m_House = house;

				Hue = 0x480;
				Movable = false;
			}

			public override void GetProperties(ObjectPropertyList list)
			{
				base.GetProperties(list);

				string houseName, owner, location;

				houseName = m_House == null ? "an unnamed house" : m_House.Sign.GetName();

				var houseOwner = m_House?.Owner;

				if (houseOwner == null)
				{
					owner = "nobody";
				}
				else
				{
					owner = houseOwner.Name;
				}

				int xLong = 0, yLat = 0, xMins = 0, yMins = 0;
				bool xEast = false, ySouth = false;

				var valid = m_House != null && Sextant.Format(m_House.Location, m_House.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth);

				if (valid)
				{
					location = String.Format("{0}° {1}'{2}, {3}° {4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
				}
				else
				{
					location = "unknown";
				}

				list.Add(1061112, Utility.FixHtml(houseName)); // House Name: ~1_val~
				list.Add(1061113, owner); // Owner: ~1_val~
				list.Add(1061114, location); // Location: ~1_val~
			}

			public TransferItem(Serial serial) : base(serial)
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
				_ = reader.ReadInt();

				Delete();
			}

			public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
			{
				if (!base.AllowSecureTrade(from, to, newOwner, accepted))
				{
					return false;
				}
				else if (!accepted)
				{
					return true;
				}

				if (Deleted || m_House == null || m_House.Deleted || !m_House.IsOwner(from) || !from.CheckAlive() || !to.CheckAlive())
				{
					return false;
				}

				if (BaseHouse.HasAccountHouse(to))
				{
					from.SendLocalizedMessage(501388); // You cannot transfer ownership to another house owner or co-owner!
					return false;
				}

				return m_House.CheckTransferPosition(from, to);
			}

			public override void OnSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
			{
				if (Deleted)
				{
					return;
				}

				Delete();

				if (m_House == null || m_House.Deleted || !m_House.IsOwner(from) || !from.CheckAlive() || !to.CheckAlive())
				{
					return;
				}

				if (!accepted)
				{
					return;
				}

				from.SendLocalizedMessage(501338); // You have transferred ownership of the house.
				to.SendLocalizedMessage(501339); /* You are now the owner of this house.
													* The house's co-owner, friend, ban, and access lists have been cleared.
													* You should double-check the security settings on any doors and teleporters in the house.
													*/

				m_House.RemoveKeys(from);
				m_House.Owner = to;
				m_House.Bans.Clear();
				m_House.Friends.Clear();
				m_House.CoOwners.Clear();
				m_House.ChangeLocks(to);
				m_House.LastTraded = DateTime.UtcNow;
			}
		}

		public bool CheckTransferPosition(Mobile from, Mobile to)
		{
			var isValid = true;
			Item sign = Sign;
			var p = sign == null ? Point3D.Zero : sign.GetWorldLocation();

			if (from.Map != Map || to.Map != Map)
			{
				isValid = false;
			}
			else if (sign == null)
			{
				isValid = false;
			}
			else if (from.Map != sign.Map || to.Map != sign.Map)
			{
				isValid = false;
			}
			else if (IsInside(from))
			{
				isValid = false;
			}
			else if (IsInside(to))
			{
				isValid = false;
			}
			else if (!from.InRange(p, 2))
			{
				isValid = false;
			}
			else if (!to.InRange(p, 2))
			{
				isValid = false;
			}

			if (!isValid)
			{
				from.SendLocalizedMessage(1062067); // In order to transfer the house, you and the recipient must both be outside the building and within two paces of the house sign.
			}

			return isValid;
		}

		public void BeginConfirmTransfer(Mobile from, Mobile to)
		{
			if (Deleted || !from.CheckAlive() || !IsOwner(from))
			{
				return;
			}

			if (NewVendorSystem && HasPersonalVendors)
			{
				from.SendLocalizedMessage(1062467); // You cannot trade this house while you still have personal vendors inside.
			}
			else if (DecayLevel == DecayLevel.DemolitionPending)
			{
				from.SendLocalizedMessage(1005321); // This house has been marked for demolition, and it cannot be transferred.
			}
			else if (from == to)
			{
				from.SendLocalizedMessage(1005330); // You cannot transfer a house to yourself, silly.
			}
			else if (to.Player)
			{
				if (BaseHouse.HasAccountHouse(to))
				{
					from.SendLocalizedMessage(501388); // You cannot transfer ownership to another house owner or co-owner!
				}
				else if (CheckTransferPosition(from, to))
				{
					from.SendLocalizedMessage(1005326); // Please wait while the other player verifies the transfer.

					if (HasRentedVendors)
					{
						/* You are about to be traded a home that has active vendor contracts.
						 * While there are active vendor contracts in this house, you
						 * <strong>cannot</strong> demolish <strong>OR</strong> customize the home.
						 * When you accept this house, you also accept landlordship for every
						 * contract vendor in the house.
						 */
						_ = to.SendGump(new WarningGump(1060635, 30720, 1062487, 32512, 420, 280, new WarningGumpCallback(ConfirmTransfer_Callback), from));
					}
					else
					{
						_ = to.CloseGump(typeof(HouseTransferGump));
						_ = to.SendGump(new HouseTransferGump(from, to, this));
					}
				}
			}
			else
			{
				from.SendLocalizedMessage(501384); // Only a player can own a house!
			}
		}

		private void ConfirmTransfer_Callback(Mobile to, bool ok, object state)
		{
			var from = (Mobile)state;

			if (!ok || Deleted || !from.CheckAlive() || !IsOwner(from))
			{
				return;
			}

			if (CheckTransferPosition(from, to))
			{
				_ = to.CloseGump(typeof(HouseTransferGump));
				_ = to.SendGump(new HouseTransferGump(from, to, this));
			}
		}

		public void EndConfirmTransfer(Mobile from, Mobile to)
		{
			if (Deleted || !from.CheckAlive() || !IsOwner(from))
			{
				return;
			}

			if (NewVendorSystem && HasPersonalVendors)
			{
				from.SendLocalizedMessage(1062467); // You cannot trade this house while you still have personal vendors inside.
			}
			else if (DecayLevel == DecayLevel.DemolitionPending)
			{
				from.SendLocalizedMessage(1005321); // This house has been marked for demolition, and it cannot be transferred.
			}
			else if (from == to)
			{
				from.SendLocalizedMessage(1005330); // You cannot transfer a house to yourself, silly.
			}
			else if (to.Player)
			{
				if (BaseHouse.HasAccountHouse(to))
				{
					from.SendLocalizedMessage(501388); // You cannot transfer ownership to another house owner or co-owner!
				}
				else if (CheckTransferPosition(from, to))
				{
					NetState fromState = from.NetState, toState = to.NetState;

					if (fromState != null && toState != null)
					{
						if (from.HasTrade)
						{
							from.SendLocalizedMessage(1062071); // You cannot trade a house while you have other trades pending.
						}
						else if (to.HasTrade)
						{
							to.SendLocalizedMessage(1062071); // You cannot trade a house while you have other trades pending.
						}
						else if (!to.Alive)
						{
							// TODO: Check if the message is correct.
							from.SendLocalizedMessage(1062069); // You cannot transfer this house to that person.
						}
						else
						{
							Container c = fromState.AddTrade(toState);

							c.DropItem(new TransferItem(this));
						}
					}
				}
			}
			else
			{
				from.SendLocalizedMessage(501384); // Only a player can own a house!
			}
		}

		public void Release(Mobile m, Item item)
		{
			if (!IsCoOwner(m) || !IsActive)
			{
				return;
			}

			if (IsLockedDown(item))
			{
				item.PublicOverheadMessage(Server.Network.MessageType.Label, 0x3B2, 501657);//[no longer locked down]
				SetLockdown(item, false);
				//TidyItemList( m_LockDowns );

				if (item is RewardBrazier)
				{
					((RewardBrazier)item).TurnOff();
				}
			}
			else if (IsSecure(item))
			{
				ReleaseSecure(m, item);
			}
			else
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1010416); // This is not locked down or secured.
			}
		}

		public void AddSecure(Mobile m, Item item)
		{
			if (Secures == null || !IsOwner(m) || !IsActive)
			{
				return;
			}

			if (!IsInside(item))
			{
				m.SendLocalizedMessage(1005525); // That is not in your house
			}
			else if (IsLockedDown(item))
			{
				m.SendLocalizedMessage(1010550); // This is already locked down and cannot be secured.
			}
			else if (item is not Container)
			{
				_ = LockDown(m, item);
			}
			else
			{
				SecureInfo info = null;

				foreach (var si in Secures)
				{
					if (si.Item == item)
					{
						info = si;
						break;
					}
				}

				if (info != null)
				{
					_ = m.CloseGump(typeof(SetSecureLevelGump));
					_ = m.SendGump(new SetSecureLevelGump(m_Owner, info, this));
				}
				else if (item.Parent != null)
				{
					m.SendLocalizedMessage(1010423); // You cannot secure this, place it on the ground first.
				}
				// Mondain's Legacy mod
				else if (item is not BaseAddonContainer && !item.Movable)
				{
					m.SendLocalizedMessage(1010424); // You cannot secure this.
				}
				else if (!IsAosRules && SecureCount >= MaxSecures)
				{
					// The maximum number of secure items has been reached :
					m.SendLocalizedMessage(1008142, true, MaxSecures.ToString());
				}
				else if (IsAosRules ? !CheckAosLockdowns(1) : ((LockDownCount + 125) >= MaxLockDowns))
				{
					m.SendLocalizedMessage(1005379); // That would exceed the maximum lock down limit for this house
				}
				else if (IsAosRules && !CheckAosStorage(item.TotalItems))
				{
					m.SendLocalizedMessage(1061839); // This action would exceed the secure storage limit of the house.
				}
				else
				{
					info = new SecureInfo((Container)item, SecureLevel.Owner);

					item.IsLockedDown = false;
					item.IsSecure = true;

					_ = Secures.Add(info);
					_ = LockDowns.Remove(item);
					item.Movable = false;

					_ = m.CloseGump(typeof(SetSecureLevelGump));
					_ = m.SendGump(new SetSecureLevelGump(m_Owner, info, this));
				}
			}
		}

		public virtual bool IsCombatRestricted(Mobile m)
		{
			if (m == null || !m.Player || m.AccessLevel >= AccessLevel.GameMaster || !IsAosRules || (m_Owner != null && m_Owner.AccessLevel >= AccessLevel.GameMaster))
			{
				return false;
			}

			for (var i = 0; i < m.Aggressed.Count; ++i)
			{
				var info = m.Aggressed[i];

				var attackerGuild = m.Guild as Guild;
				var defenderGuild = info.Defender.Guild as Guild;

				if (info.Defender.Player && info.Defender.Alive && (DateTime.UtcNow - info.LastCombatTime) < HouseRegion.CombatHeatDelay && (attackerGuild == null || defenderGuild == null || (defenderGuild != attackerGuild && !defenderGuild.IsEnemy(attackerGuild))))
				{
					return true;
				}
			}

			return false;
		}

		public bool HasSecureAccess(Mobile m, SecureLevel level)
		{
			if (m.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}

			if (IsCombatRestricted(m))
			{
				return false;
			}

			return level switch
			{
				SecureLevel.Owner => IsOwner(m),
				SecureLevel.CoOwners => IsCoOwner(m),
				SecureLevel.Friends => IsFriend(m),
				SecureLevel.Anyone => true,
				SecureLevel.Guild => IsGuildMember(m),
				_ => false,
			};
		}

		public void ReleaseSecure(Mobile m, Item item)
		{
			if (Secures == null || !IsOwner(m) || item is StrongBox || !IsActive)
			{
				return;
			}

			SecureInfo match = null;

			foreach (var info in Secures)
			{
				if (info.Item == item && HasSecureAccess(m, info.Level))
				{
					match = info;
					break;
				}
			}

			if (match != null)
			{
				item.IsLockedDown = false;
				item.IsSecure = false;

				item.Movable = item is not BaseAddonContainer;

				item.SetLastMoved();
				item.PublicOverheadMessage(Server.Network.MessageType.Label, 0x3B2, 501656);//[no longer secure]

				_ = Secures.Remove(match);

				return;
			}

			m.SendLocalizedMessage(501717);//This isn't secure...
		}

		public override bool Decays => false;

		public void AddStrongBox(Mobile from)
		{
			if (!IsCoOwner(from) || !IsActive)
			{
				return;
			}

			if (from == Owner)
			{
				from.SendLocalizedMessage(502109); // Owners don't get a strong box
				return;
			}

			if (IsAosRules ? !CheckAosLockdowns(1) : ((LockDownCount + 1) > MaxLockDowns))
			{
				from.SendLocalizedMessage(1005379);//That would exceed the maximum lock down limit for this house
				return;
			}

			foreach (var info in Secures)
			{
				var c = info.Item;

				if (!c.Deleted && c is StrongBox && ((StrongBox)c).Owner == from)
				{
					from.SendLocalizedMessage(502112);//You already have a strong box
					return;
				}
			}

			foreach (var door in Doors)
			{
				var p = door.Location;

				if (door.Open)
				{
					p = new Point3D(p.X - door.Offset.X, p.Y - door.Offset.Y, p.Z - door.Offset.Z);
				}

				if ((from.Z + 16) >= p.Z && (p.Z + 16) >= from.Z)
				{
					if (from.InRange(p, 1))
					{
						from.SendLocalizedMessage(502113); // You cannot place a strongbox near a door or near steps.
						return;
					}
				}
			}

			var sb = new StrongBox(from, this)
			{
				Movable = false,
				IsLockedDown = false,
				IsSecure = true
			};
			_ = Secures.Add(new SecureInfo(sb, SecureLevel.CoOwners));
			sb.MoveToWorld(from.Location, from.Map);
		}

		public void Kick(Mobile from, Mobile targ)
		{
			if (!IsFriend(from) || Friends == null)
			{
				return;
			}

			if (targ.AccessLevel > AccessLevel.Player && from.AccessLevel <= targ.AccessLevel)
			{
				from.SendLocalizedMessage(501346); // Uh oh...a bigger boot may be required!
			}
			else if (IsFriend(targ) && !Core.ML)
			{
				from.SendLocalizedMessage(501348); // You cannot eject a friend of the house!
			}
			else if (targ is PlayerVendor)
			{
				from.SendLocalizedMessage(501351); // You cannot eject a vendor.
			}
			else if (!IsInside(targ))
			{
				from.SendLocalizedMessage(501352); // You may not eject someone who is not in your house!
			}
			else if (targ is BaseCreature && ((BaseCreature)targ).NoHouseRestrictions)
			{
				from.SendLocalizedMessage(501347); // You cannot eject that from the house!
			}
			else
			{
				targ.MoveToWorld(BanLocation, Map);

				from.SendLocalizedMessage(1042840, targ.Name); // ~1_PLAYER NAME~ has been ejected from this house.
				targ.SendLocalizedMessage(501341); /* You have been ejected from this house.
													  * If you persist in entering, you may be banned from the house.
													  */
			}
		}

		public void RemoveAccess(Mobile from, Mobile targ)
		{
			if (!IsFriend(from) || Access == null)
			{
				return;
			}

			if (Access.Contains(targ))
			{
				_ = Access.Remove(targ);

				if (!HasAccess(targ) && IsInside(targ))
				{
					targ.Location = BanLocation;
					targ.SendLocalizedMessage(1060734); // Your access to this house has been revoked.
				}

				from.SendLocalizedMessage(1050051); // The invitation has been revoked.
			}
		}

		public void RemoveBan(Mobile from, Mobile targ)
		{
			if (!IsCoOwner(from) || Bans == null)
			{
				return;
			}

			if (Bans.Contains(targ))
			{
				_ = Bans.Remove(targ);

				from.SendLocalizedMessage(501297); // The ban is lifted.
			}
		}

		public void Ban(Mobile from, Mobile targ)
		{
			if (!IsFriend(from) || Bans == null)
			{
				return;
			}

			if (targ.AccessLevel > AccessLevel.Player && from.AccessLevel <= targ.AccessLevel)
			{
				from.SendLocalizedMessage(501354); // Uh oh...a bigger boot may be required.
			}
			else if (IsFriend(targ))
			{
				from.SendLocalizedMessage(501348); // You cannot eject a friend of the house!
			}
			else if (targ is PlayerVendor)
			{
				from.SendLocalizedMessage(501351); // You cannot eject a vendor.
			}
			else if (Bans.Count >= MaxBans)
			{
				from.SendLocalizedMessage(501355); // The ban limit for this house has been reached!
			}
			else if (IsBanned(targ))
			{
				from.SendLocalizedMessage(501356); // This person is already banned!
			}
			else if (!IsInside(targ))
			{
				from.SendLocalizedMessage(501352); // You may not eject someone who is not in your house!
			}
			else if (!Public && IsAosRules)
			{
				from.SendLocalizedMessage(1062521); // You cannot ban someone from a private house.  Revoke their access instead.
			}
			else if (targ is BaseCreature && ((BaseCreature)targ).NoHouseRestrictions)
			{
				from.SendLocalizedMessage(1062040); // You cannot ban that.
			}
			else
			{
				_ = Bans.Add(targ);

				from.SendLocalizedMessage(1042839, targ.Name); // ~1_PLAYER_NAME~ has been banned from this house.
				targ.SendLocalizedMessage(501340); // You have been banned from this house.

				targ.MoveToWorld(BanLocation, Map);
			}
		}

		public void GrantAccess(Mobile from, Mobile targ)
		{
			if (!IsFriend(from) || Access == null)
			{
				return;
			}

			if (HasAccess(targ))
			{
				from.SendLocalizedMessage(1060729); // That person already has access to this house.
			}
			else if (!targ.Player)
			{
				from.SendLocalizedMessage(1060712); // That is not a player.
			}
			else if (IsBanned(targ))
			{
				from.SendLocalizedMessage(501367); // This person is banned!  Unban them first.
			}
			else
			{
				_ = Access.Add(targ);

				targ.SendLocalizedMessage(1060735); // You have been granted access to this house.
			}
		}

		public void AddCoOwner(Mobile from, Mobile targ)
		{
			if (!IsOwner(from) || CoOwners == null || Friends == null)
			{
				return;
			}

			if (IsOwner(targ))
			{
				from.SendLocalizedMessage(501360); // This person is already the house owner!
			}
			else if (Friends.Contains(targ))
			{
				from.SendLocalizedMessage(501361); // This person is a friend of the house. Remove them first.
			}
			else if (!targ.Player)
			{
				from.SendLocalizedMessage(501362); // That can't be a co-owner of the house.
			}
			else if (!Core.AOS && HasAccountHouse(targ))
			{
				from.SendLocalizedMessage(501364); // That person is already a house owner.
			}
			else if (IsBanned(targ))
			{
				from.SendLocalizedMessage(501367); // This person is banned!  Unban them first.
			}
			else if (CoOwners.Count >= MaxCoOwners)
			{
				from.SendLocalizedMessage(501368); // Your co-owner list is full!
			}
			else if (CoOwners.Contains(targ))
			{
				from.SendLocalizedMessage(501369); // This person is already on your co-owner list!
			}
			else
			{
				_ = CoOwners.Add(targ);

				targ.Delta(MobileDelta.Noto);
				targ.SendLocalizedMessage(501343); // You have been made a co-owner of this house.
			}
		}

		public void RemoveCoOwner(Mobile from, Mobile targ)
		{
			if (!IsOwner(from) || CoOwners == null)
			{
				return;
			}

			if (CoOwners.Contains(targ))
			{
				_ = CoOwners.Remove(targ);

				targ.Delta(MobileDelta.Noto);

				from.SendLocalizedMessage(501299); // Co-owner removed from list.
				targ.SendLocalizedMessage(501300); // You have been removed as a house co-owner.

				foreach (var info in Secures)
				{
					var c = info.Item;

					if (c is StrongBox && ((StrongBox)c).Owner == targ)
					{
						c.IsLockedDown = false;
						c.IsSecure = false;
						_ = Secures.Remove(info);
						c.Destroy();
						break;
					}
				}
			}
		}

		public void AddFriend(Mobile from, Mobile targ)
		{
			if (!IsCoOwner(from) || Friends == null || CoOwners == null)
			{
				return;
			}

			if (IsOwner(targ))
			{
				from.SendLocalizedMessage(501370); // This person is already an owner of the house!
			}
			else if (CoOwners.Contains(targ))
			{
				from.SendLocalizedMessage(501369); // This person is already on your co-owner list!
			}
			else if (!targ.Player)
			{
				from.SendLocalizedMessage(501371); // That can't be a friend of the house.
			}
			else if (IsBanned(targ))
			{
				from.SendLocalizedMessage(501374); // This person is banned!  Unban them first.
			}
			else if (Friends.Count >= MaxFriends)
			{
				from.SendLocalizedMessage(501375); // Your friends list is full!
			}
			else if (Friends.Contains(targ))
			{
				from.SendLocalizedMessage(501376); // This person is already on your friends list!
			}
			else
			{
				_ = Friends.Add(targ);

				targ.Delta(MobileDelta.Noto);
				targ.SendLocalizedMessage(501337); // You have been made a friend of this house.
			}
		}

		public void RemoveFriend(Mobile from, Mobile targ)
		{
			if (!IsCoOwner(from) || Friends == null)
			{
				return;
			}

			if (Friends.Contains(targ))
			{
				_ = Friends.Remove(targ);

				targ.Delta(MobileDelta.Noto);

				from.SendLocalizedMessage(501298); // Friend removed from list.
				targ.SendLocalizedMessage(1060751); // You are no longer a friend of this house.
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(16); // version

			writer.Write(m_Region);

			if (!DynamicDecay.Enabled)
			{
				writer.Write(-1);
			}
			else
			{
				writer.Write((int)m_CurrentStage);
				writer.Write(NextDecayStage);
			}

			writer.Write(m_RelativeBanLocation);

			writer.WriteItemSet(VendorRentalContracts, true);
			writer.WriteMobileSet(InternalizedVendors, true);

			writer.WriteEncodedInt(RelocatedEntities.Count);
			foreach (var relEntity in RelocatedEntities)
			{
				writer.Write(relEntity.RelativeLocation);

				if (relEntity.Entity.Deleted)
				{
					writer.Write(Serial.MinusOne);
				}
				else
				{
					writer.Write(relEntity.Entity.Serial);
				}
			}

			writer.WriteEncodedInt(VendorInventories.Count);
			foreach (var inventory in VendorInventories)
			{
				inventory.Serialize(writer);
			}

			writer.Write(LastRefreshed);
			writer.Write(RestrictDecay);

			writer.Write(Visits);

			writer.Write(Price);

			writer.WriteMobileSet(Access);

			writer.Write(BuiltOn);
			writer.Write(LastTraded);

			writer.WriteItemSet(Addons, true);

			writer.Write(Secures.Count);

			foreach (var si in Secures)
			{
				si.Serialize(writer);
			}

			writer.Write(m_Public);

			writer.Write(m_Owner);

			writer.WriteMobileSet(CoOwners, true);
			writer.WriteMobileSet(Friends, true);
			writer.WriteMobileSet(Bans, true);

			writer.Write(Sign);
			writer.Write(m_Trash);

			writer.WriteItemSet(Doors, true);
			writer.WriteItemSet(LockDowns, true);

			writer.Write(MaxLockDowns);
			writer.Write(MaxSecures);

			// Items in locked down containers that aren't locked down themselves must decay!
			foreach (var item in LockDowns)
			{
				if (item is Container and not (BaseBoard or Aquarium or FishBowl))
				{
					var cont = (Container)item;
					var children = cont.Items;

					for (var j = 0; j < children.Count; ++j)
					{
						var child = children[j];

						if (child.Decays && !child.IsLockedDown && !child.IsSecure && (child.LastMoved + child.DecayTime) <= DateTime.UtcNow)
						{
							_ = Timer.DelayCall(child.Delete);
						}
					}
				}
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();
			int count;
			var loadedDynamicDecay = false;

			switch (version)
			{
				case 16:
					{
						m_Region = reader.ReadRegion<HouseRegion>();
						goto case 15;
					}
				case 15:
					{
						var stage = reader.ReadInt();

						if (stage != -1)
						{
							m_CurrentStage = (DecayLevel)stage;
							NextDecayStage = reader.ReadDateTime();
							loadedDynamicDecay = true;
						}

						goto case 14;
					}
				case 14:
					{
						m_RelativeBanLocation = reader.ReadPoint3D();
						goto case 13;
					}
				case 13: // removed ban location serialization
				case 12:
					{
						VendorRentalContracts = reader.ReadItemSet<VendorRentalContract>();
						InternalizedVendors = reader.ReadMobileSet();

						var relocatedCount = reader.ReadEncodedInt();
						for (var i = 0; i < relocatedCount; i++)
						{
							var relLocation = reader.ReadPoint3D();
							var entity = reader.ReadEntity();

							if (entity != null)
							{
								_ = RelocatedEntities.Add(new RelocatedEntity(entity, relLocation));
							}
						}

						var inventoryCount = reader.ReadEncodedInt();
						for (var i = 0; i < inventoryCount; i++)
						{
							var inventory = new VendorInventory(this, reader);
							_ = VendorInventories.Add(inventory);
						}

						goto case 11;
					}
				case 11:
					{
						LastRefreshed = reader.ReadDateTime();
						RestrictDecay = reader.ReadBool();
						goto case 10;
					}
				case 10: // just a signal for updates
				case 9:
					{
						Visits = reader.ReadInt();
						goto case 8;
					}
				case 8:
					{
						Price = reader.ReadInt();
						goto case 7;
					}
				case 7:
					{
						Access = reader.ReadMobileSet();
						goto case 6;
					}
				case 6:
					{
						BuiltOn = reader.ReadDateTime();
						LastTraded = reader.ReadDateTime();
						goto case 5;
					}
				case 5: // just removed fields
				case 4:
					{
						Addons = reader.ReadItemSet();
						goto case 3;
					}
				case 3:
					{
						count = reader.ReadInt();

						Secures = new(count);

						for (var i = 0; i < count; ++i)
						{
							var info = new SecureInfo(reader);

							if (info.Item != null)
							{
								info.Item.IsSecure = true;
								_ = Secures.Add(info);
							}
						}

						goto case 2;
					}
				case 2:
					{
						m_Public = reader.ReadBool();
						goto case 1;
					}
				case 1:
					{
						if (version < 13)
						{
							_ = reader.ReadPoint3D(); // house ban location
						}

						goto case 0;
					}
				case 0:
					{
						if (version < 14)
						{
							m_RelativeBanLocation = BaseBanLocation;
						}

						if (version < 8)
						{
							Price = DefaultPrice;
						}

						m_Owner = reader.ReadMobile();

						if (version < 5)
						{
							count = reader.ReadInt();

							for (var i = 0; i < count; i++)
							{
								_ = reader.ReadRect2D();
							}
						}

						CoOwners = reader.ReadMobileSet();
						Friends = reader.ReadMobileSet();
						Bans = reader.ReadMobileSet();

						Sign = reader.ReadItem<HouseSign>();
						m_Trash = reader.ReadItem<TrashBarrel>();

						Doors = reader.ReadItemSet<BaseDoor>();
						LockDowns = reader.ReadItemSet();

						if (version < 3)
						{
							var items = reader.ReadItemSet();

							Secures = new(items.Count);

							foreach (var o in items)
							{
								if (o is Container c)
								{
									c.IsSecure = true;
									_ = Secures.Add(new SecureInfo(c, SecureLevel.CoOwners));
								}
							}
						}

						MaxLockDowns = reader.ReadInt();
						MaxSecures = reader.ReadInt();
						break;
					}
			}

			foreach (var o in LockDowns)
			{
				o.IsLockedDown = true;
			}

			foreach (var o in VendorRentalContracts)
			{
				o.IsLockedDown = true;
			}

			if (m_Owner != null)
			{
				_ = m_Table.TryGetValue(m_Owner, out var list);

				if (list == null)
				{
					m_Table[m_Owner] = list = new List<BaseHouse>();
				}

				list.Add(this);
			}

			if (version <= 1)
			{
				ChangeSignType(0xBD2);//private house, plain brass sign
			}

			if (version < 10)
			{
				/* NOTE: This can exceed the house lockdown limit. It must be this way, because
				 * we do not want players' items to decay without them knowing. Or not even
				 * having a chance to fix it themselves.
				 */

				_ = Timer.DelayCall(FixLockdowns_Sandbox);
			}

			if (version < 11)
			{
				LastRefreshed = DateTime.UtcNow + TimeSpan.FromHours(24 * Utility.RandomDouble());
			}

			if (DynamicDecay.Enabled && !loadedDynamicDecay)
			{
				var old = GetOldDecayLevel();

				if (old == DecayLevel.DemolitionPending)
				{
					old = DecayLevel.Collapsed;
				}

				SetDynamicDecay(old);
			}

			if ((Map == null || Map == Map.Internal) && Location == Point3D.Zero)
			{
				Delete();
				return;
			}

			if (m_Region?.Deleted != false)
			{
				_ = Timer.DelayCall(UpdateRegion);
			}

			if (CheckDecay())
			{
				if (RelocatedEntities.Count > 0)
				{
					_ = Timer.DelayCall(TimeSpan.Zero, RestoreRelocatedEntities);
				}

				if (m_Owner == null && Friends.Count == 0 && CoOwners.Count == 0)
				{
					_ = Timer.DelayCall(TimeSpan.FromSeconds(10.0), Delete);
				}
			}
		}

		private void FixLockdowns_Sandbox()
		{
			var lockDowns = new HashSet<Item>();

			foreach (var item in LockDowns)
			{
				if (item is Container c)
				{
					_ = lockDowns.Add(c);
				}
			}

			foreach (var item in lockDowns)
			{
				SetLockdown(item, true);
			}
		}

		public static void HandleDeletion(Mobile mob)
		{
			var houses = GetHouses(mob);

			if (houses.Count == 0)
			{
				return;
			}

			var acct = mob.Account as Account;
			Mobile trans = null;

			for (var i = 0; i < acct.Length; ++i)
			{
				if (acct[i] != null && acct[i] != mob)
				{
					trans = acct[i];
				}
			}

			for (var i = 0; i < houses.Count; ++i)
			{
				var house = houses[i];

				var canClaim = false;

				if (trans == null)
				{
					canClaim = house.CoOwners.Count > 0;
				}
				/*{
	for ( int j = 0; j < house.CoOwners.Count; ++j )
	{
		Mobile check = house.CoOwners[j] as Mobile;

		if ( check != null && !check.Deleted && !HasAccountHouse( check ) )
		{
			canClaim = true;
			break;
		}
	}
}*/

				if (trans == null && !canClaim)
				{
					_ = Timer.DelayCall(TimeSpan.Zero, house.Delete);
				}
				else
				{
					house.Owner = trans;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner
		{
			get => m_Owner;
			set
			{
				if (m_Owner != null)
				{
					List<BaseHouse> list;
					_ = m_Table.TryGetValue(m_Owner, out list);

					if (list == null)
					{
						m_Table[m_Owner] = list = new List<BaseHouse>();
					}

					_ = list.Remove(this);
					m_Owner.Delta(MobileDelta.Noto);
				}

				m_Owner = value;

				if (m_Owner != null)
				{
					List<BaseHouse> list;
					_ = m_Table.TryGetValue(m_Owner, out list);

					if (list == null)
					{
						m_Table[m_Owner] = list = new List<BaseHouse>();
					}

					list.Add(this);
					m_Owner.Delta(MobileDelta.Noto);
				}

				if (Sign != null)
				{
					Sign.InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Visits { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Public
		{
			get => m_Public;
			set
			{
				if (m_Public != value)
				{
					m_Public = value;

					if (!m_Public) // Privatizing the house, change to brass sign
					{
						ChangeSignType(0xBD2);
					}

					if (Sign != null)
					{
						Sign.InvalidateProperties();
					}
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxSecures { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D BanLocation
		{
			get
			{
				if (m_Region != null)
				{
					return m_Region.GoLocation;
				}

				var rel = m_RelativeBanLocation;
				return new Point3D(X + rel.X, Y + rel.Y, Z + rel.Z);
			}
			set => RelativeBanLocation = new Point3D(value.X - X, value.Y - Y, value.Z - Z);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D RelativeBanLocation
		{
			get => m_RelativeBanLocation;
			set
			{
				m_RelativeBanLocation = value;

				if (m_Region != null)
				{
					m_Region.GoLocation = new Point3D(X + value.X, Y + value.Y, Z + value.Z);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MaxLockDowns { get; set; }

		public Region Region => m_Region;
		public HashSet<Mobile> CoOwners { get; set; } = new();
		public HashSet<Mobile> Friends { get; set; } = new();
		public HashSet<Mobile> Access { get; set; } = new();
		public HashSet<Mobile> Bans { get; set; } = new();
		public HashSet<BaseDoor> Doors { get; set; } = new();

		public int GetLockdowns()
		{
			var count = 0;

			if (LockDowns != null)
			{
				foreach (var item in LockDowns)
				{
					if (item is not Container)
					{
						count += item.TotalItems;
					}

					count++;
				}
			}

			return count;
		}

		public int LockDownCount
		{
			get
			{
				var count = 0;

				count += GetLockdowns();

				if (Secures != null)
				{
					foreach (var info in Secures)
					{
						if (info.Item.Deleted)
						{
							continue;
						}

						if (info.Item is StrongBox)
						{
							count += 1;
						}
						else
						{
							count += 125;
						}
					}
				}

				return count;
			}
		}

		public int SecureCount
		{
			get
			{
				var count = 0;

				if (Secures != null)
				{
					foreach (var info in Secures)
					{
						if (info.Item.Deleted)
						{
							continue;
						}

						if (info.Item is not StrongBox)
						{
							count += 1;
						}
					}
				}

				return count;
			}
		}

		public HashSet<Item> Addons { get; set; } = new();
		public HashSet<Item> LockDowns { get; private set; } = new();
		public HashSet<SecureInfo> Secures { get; private set; } = new();
		public HashSet<PlayerVendor> PlayerVendors { get; } = new();
		public HashSet<Mobile> PlayerBarkeepers { get; } = new();
		public HashSet<VendorRentalContract> VendorRentalContracts { get; private set; } = new();
		public HashSet<VendorInventory> VendorInventories { get; } = new();
		public HashSet<RelocatedEntity> RelocatedEntities { get; } = new();
		public HashSet<Mobile> InternalizedVendors { get; private set; } = new();

		public MovingCrate MovingCrate { get; set; }

		public HouseSign Sign { get; set; }

		public DateTime BuiltOn { get; set; }

		public DateTime LastTraded { get; set; }

		public override void OnDelete()
		{
			RestoreRelocatedEntities();

			var mcl = Components;

			_ = Timer.DelayCall((map, loc, min, max) =>
			{
				var startX = loc.X + min.X;
				var startY = loc.Y + min.Y;
				var endX = loc.X + max.X;
				var endY = loc.Y + max.Y;

				for (var x = startX; x <= endX; ++x)
				{
					for (var y = startY; y <= endY; ++y)
					{
						map.FixColumn(x, y);
					}
				}
			}, Map, Location, mcl.Min, mcl.Max);

			base.OnDelete();
		}

		private class FixColumnTimer : Timer
		{
			private readonly Map m_Map;
			private readonly int m_StartX, m_StartY, m_EndX, m_EndY;

			public FixColumnTimer(BaseMulti multi) : base(TimeSpan.Zero)
			{
				m_Map = multi.Map;

				var mcl = multi.Components;

				m_StartX = multi.X + mcl.Min.X;
				m_StartY = multi.Y + mcl.Min.Y;
				m_EndX = multi.X + mcl.Max.X;
				m_EndY = multi.Y + mcl.Max.Y;
			}

			protected override void OnTick()
			{
				if (m_Map == null)
				{
					return;
				}

				for (var x = m_StartX; x <= m_EndX; ++x)
				{
					for (var y = m_StartY; y <= m_EndY; ++y)
					{
						m_Map.FixColumn(x, y);
					}
				}
			}
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_Owner != null)
			{
				_ = m_Table.TryGetValue(m_Owner, out var list);

				if (list == null)
				{
					m_Table[m_Owner] = list = new List<BaseHouse>();
				}

				_ = list.Remove(this);
			}

			if (m_Region != null)
			{
				m_Region.Delete();
				m_Region = null;
			}

			if (Sign != null)
			{
				Sign.Delete();
				Sign = null;
			}

			if (m_Trash != null)
			{
				m_Trash.Delete();
				m_Trash = null;
			}

			if (Doors != null)
			{
				foreach (var door in Doors.ToArray())
				{
					door.Delete();
				}

				Doors.Clear();
			}

			if (LockDowns != null)
			{
				foreach (var item in LockDowns.ToArray())
				{
					item.IsLockedDown = false;
					item.IsSecure = false;
					item.Movable = true;
					item.SetLastMoved();
				}

				LockDowns.Clear();
			}

			if (VendorRentalContracts != null)
			{
				foreach (var item in VendorRentalContracts.ToArray())
				{
					item.IsLockedDown = false;
					item.IsSecure = false;
					item.Movable = true;
					item.SetLastMoved();
				}

				VendorRentalContracts.Clear();
			}

			if (Secures != null)
			{
				foreach (var info in Secures.ToArray())
				{
					if (info.Item is StrongBox)
					{
						info.Item.Destroy();
					}
					else
					{
						info.Item.IsLockedDown = false;
						info.Item.IsSecure = false;
						info.Item.Movable = true;
						info.Item.SetLastMoved();
					}
				}

				Secures.Clear();
			}

			if (Addons != null)
			{
				foreach (var item in Addons.ToArray())
				{
					if (!item.Deleted && item is IAddon addon)
					{
						var deed = addon.Deed;
						var retainDeedHue = false; //if the items aren't hued but the deed itself is
						var hue = 0;

						if (addon is BaseAddon ba && ba.RetainDeedHue)   //There are things that are IAddon which aren't BaseAddon
						{
							retainDeedHue = true;

							for (var j = 0; hue == 0 && j < ba.Components.Count; ++j)
							{
								var c = ba.Components[j];

								if (c.Hue != 0)
								{
									hue = c.Hue;
								}
							}
						}

						if (deed != null)
						{
							if (retainDeedHue)
							{
								deed.Hue = hue;
							}

							deed.MoveToWorld(item.Location, item.Map);
						}
					}

					item.Delete();
				}

				Addons.Clear();
			}

			if (VendorInventories != null)
			{
				foreach (var inventory in VendorInventories.ToArray())
				{
					inventory.Delete();
				}

				VendorInventories.Clear();
			}

			if (MovingCrate != null)
			{
				MovingCrate.Delete();
				MovingCrate = null;
			}

			KillVendors();

			_ = AllHouses.Remove(this);
		}

		public static bool HasHouse(Mobile m)
		{
			if (m == null)
			{
				return false;
			}

			_ = m_Table.TryGetValue(m, out var list);

			if (list == null)
			{
				return false;
			}

			for (var i = 0; i < list.Count; ++i)
			{
				var h = list[i];

				if (!h.Deleted)
				{
					return true;
				}
			}

			return false;
		}

		public static bool HasAccountHouse(Mobile m)
		{
			for (var i = 0; i < m.Account.Length; ++i)
			{
				if (HasHouse(m.Account[i]))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsOwner(Mobile m)
		{
			if (m == null)
			{
				return false;
			}

			if (m == m_Owner || m.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}

			return IsAosRules && AccountHandler.CheckAccount(m, m_Owner);
		}

		public bool IsCoOwner(Mobile m)
		{
			if (m == null || CoOwners == null)
			{
				return false;
			}

			if (IsOwner(m) || CoOwners.Contains(m))
			{
				return true;
			}

			return !IsAosRules && AccountHandler.CheckAccount(m, m_Owner);
		}

		public bool IsGuildMember(Mobile m)
		{
			if (m == null || Owner == null || Owner.Guild == null)
			{
				return false;
			}

			return m.Guild == Owner.Guild;
		}

		public void RemoveKeys(Mobile m)
		{
			if (Doors != null)
			{
				foreach (var door in Doors)
				{
					if (door.KeyValue != 0)
					{
						Key.RemoveKeys(m, door.KeyValue);
					}
				}
			}
		}

		public void ChangeLocks(Mobile m)
		{
			var keyValue = CreateKeys(m);

			if (Doors != null)
			{
				foreach (var door in Doors)
				{
					door.KeyValue = keyValue;
				}
			}
		}

		public void RemoveLocks()
		{
			if (Doors != null)
			{
				foreach (var door in Doors)
				{
					door.KeyValue = 0;
					door.Locked = false;
				}
			}
		}

		public virtual HousePlacementEntry ConvertEntry => null;
		public virtual int ConvertOffsetX => 0;
		public virtual int ConvertOffsetY => 0;
		public virtual int ConvertOffsetZ => 0;

		public virtual int DefaultPrice => 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public int Price { get; set; }

		public virtual HouseDeed GetDeed()
		{
			return null;
		}

		public bool IsFriend(Mobile m)
		{
			if (m == null || Friends == null)
			{
				return false;
			}

			return IsCoOwner(m) || Friends.Contains(m);
		}

		public bool IsBanned(Mobile m)
		{
			if (m == null || m == Owner || m.AccessLevel > AccessLevel.Player || Bans == null)
			{
				return false;
			}

			var theirAccount = m.Account as Account;

			foreach (var c in Bans)
			{
				if (c == m)
				{
					return true;
				}

				if (c.Account is Account bannedAccount && bannedAccount == theirAccount)
				{
					return true;
				}
			}

			return false;
		}

		public bool HasAccess(Mobile m)
		{
			if (m == null)
			{
				return false;
			}

			if (m.AccessLevel > AccessLevel.Player || IsFriend(m) || (Access != null && Access.Contains(m)))
			{
				return true;
			}

			if (m is BaseCreature bc)
			{
				if (bc.NoHouseRestrictions)
				{
					return true;
				}

				if (bc.Controlled || bc.Summoned)
				{
					m = bc.ControlMaster;

					m ??= bc.SummonMaster;

					if (m == null)
					{
						return false;
					}

					if (m.AccessLevel > AccessLevel.Player || IsFriend(m) || (Access != null && Access.Contains(m)))
					{
						return true;
					}
				}
			}

			return false;
		}

		public new bool IsLockedDown(Item check)
		{
			if (check == null)
			{
				return false;
			}

			if (LockDowns == null)
			{
				return false;
			}

			return LockDowns.Contains(check) || VendorRentalContracts.Contains(check);
		}

		public new bool IsSecure(Item item)
		{
			if (item == null)
			{
				return false;
			}

			if (Secures == null)
			{
				return false;
			}

			foreach (var si in Secures)
			{
				if (si.Item == item)
				{
					return true;
				}
			}

			return false;
		}

		public virtual Guildstone FindGuildstone()
		{
			var map = Map;

			if (map == null)
			{
				return null;
			}

			var mcl = Components;
			IPooledEnumerable eable = map.GetItemsInBounds(new Rectangle2D(X + mcl.Min.X, Y + mcl.Min.Y, mcl.Width, mcl.Height));

			foreach (Item item in eable)
			{
				if (item is Guildstone && Contains(item))
				{
					eable.Free();
					return (Guildstone)item;
				}
			}

			eable.Free();
			return null;
		}
	}

	public abstract class HouseDeed : Item
	{
		private Point3D m_Offset;

		[CommandProperty(AccessLevel.GameMaster)]
		public int MultiID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D Offset
		{
			get => m_Offset;
			set => m_Offset = value;
		}

		public HouseDeed(int id, Point3D offset) : base(0x14F0)
		{
			Weight = 1.0;
			LootType = LootType.Newbied;

			MultiID = id;
			m_Offset = offset;
		}

		public HouseDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(m_Offset);

			writer.Write(MultiID);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Offset = reader.ReadPoint3D();

						goto case 0;
					}
				case 0:
					{
						MultiID = reader.ReadInt();

						break;
					}
			}

			if (Weight == 0.0)
			{
				Weight = 1.0;
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
			else if (from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse(from))
			{
				from.SendLocalizedMessage(501271); // You already own a house, you may not place another!
			}
			else
			{
				from.SendLocalizedMessage(1010433); /* House placement cancellation could result in a
													   * 60 second delay in the return of your deed.
													   */

				from.Target = new HousePlacementTarget(this);
			}
		}

		public abstract BaseHouse GetHouse(Mobile owner);
		public abstract Rectangle2D[] Area { get; }

		public bool OnPlacement(Mobile from, Point3D p)
		{
			if (Deleted || !from.CheckAlive() || from.Backpack == null)
			{
				return false;
			}

			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				return false;
			}

			var center = new Point3D(p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z);
			var res = HousePlacement.Check(from, MultiID, center, out var toMove);

			switch (res)
			{
				case HousePlacementResult.Valid:
					{
						if (from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse(from))
						{
							from.SendLocalizedMessage(501271); // You already own a house, you may not place another!
							return false;
						}

						var house = GetHouse(from);

						house.MoveToWorld(center, from.Map);

						Delete();

						if (toMove?.Count > 0)
						{
							foreach (var o in toMove)
							{
								o.Location = house.BanLocation;
							}
						}

						return true;
					}
				case HousePlacementResult.BadItem:
				case HousePlacementResult.BadLand:
				case HousePlacementResult.BadStatic:
				case HousePlacementResult.BadRegionHidden:
					{
						from.SendLocalizedMessage(1043287); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
						return false;
					}
				case HousePlacementResult.NoSurface:
					{
						from.SendMessage("The house could not be created here.  Part of the foundation would not be on any surface.");
						return false;
					}
				case HousePlacementResult.BadRegion:
					{
						from.SendLocalizedMessage(501265); // Housing cannot be created in this area.
						return false;
					}
				case HousePlacementResult.BadRegionTemp:
					{
						from.SendLocalizedMessage(501270); //Lord British has decreed a 'no build' period, thus you cannot build this house at this time.
						return false;
					}
				case HousePlacementResult.BadRegionRaffle:
					{
						from.SendLocalizedMessage(1150493); // You must have a deed for this plot of land in order to build here.
						return false;
					}
			}

			return false;
		}
	}

	#region House Owner Targets

	public class HouseKickTarget : Target
	{
		private readonly BaseHouse m_House;

		public HouseKickTarget(BaseHouse house) : base(-1, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsFriend(from))
			{
				return;
			}

			if (targeted is Mobile)
			{
				m_House.Kick(from, (Mobile)targeted);
			}
			else
			{
				from.SendLocalizedMessage(501347);//You cannot eject that from the house!
			}
		}
	}

	public class HouseBanTarget : Target
	{
		private readonly BaseHouse m_House;
		private readonly bool m_Banning;

		public HouseBanTarget(bool ban, BaseHouse house) : base(-1, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
			m_Banning = ban;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsFriend(from))
			{
				return;
			}

			if (targeted is Mobile)
			{
				if (m_Banning)
				{
					m_House.Ban(from, (Mobile)targeted);
				}
				else
				{
					m_House.RemoveBan(from, (Mobile)targeted);
				}
			}
			else
			{
				from.SendLocalizedMessage(501347);//You cannot eject that from the house!
			}
		}
	}

	#endregion

	public class EjectPlayerEntry : ContextMenuEntry
	{
		private readonly Mobile m_From;
		private readonly Mobile m_Target;
		private readonly BaseHouse m_TargetHouse;

		public EjectPlayerEntry(Mobile from, Mobile target) : base(6206, 12)
		{
			m_From = from;
			m_Target = target;
			m_TargetHouse = BaseHouse.FindHouseAt(m_Target);
		}

		public override void OnClick()
		{
			if (!m_From.Alive || m_TargetHouse.Deleted || !m_TargetHouse.IsFriend(m_From))
			{
				return;
			}

			if (m_Target is not null)
			{
				m_TargetHouse.Kick(m_From, m_Target);
			}
		}
	}

	/// House Foundation
	public enum FoundationType
	{
		Stone,
		DarkWood,
		LightWood,
		Dungeon,
		Brick,
		ElvenGrey,
		ElvenNatural,
		Crystal,
		Shadow
	}

	public class HouseFoundation : BaseHouse
	{
		private DesignState m_Current; // State which is currently visible.
		private DesignState m_Design;  // State of current design.
		private DesignState m_Backup;  // State at last user backup.

		public FoundationType Type { get; set; }
		public int LastRevision { get; set; }
		public List<Item> Fixtures { get; private set; }
		public Item SignHanger { get; private set; }
		public Item Signpost { get; private set; }
		public int SignpostGraphic { get; set; }
		public Mobile Customizer { get; set; }

		public override bool IsAosRules => true;

		public override bool IsActive => Customizer == null;

		public virtual int CustomizationCost => Core.AOS ? 0 : 10000;

		public bool IsFixture(Item item)
		{
			return Fixtures != null && Fixtures.Contains(item);
		}

		public override MultiComponentList Components
		{
			get
			{
				if (m_Current == null)
				{
					SetInitialState();
				}

				return m_Current.Components;
			}
		}

		public override int GetMaxUpdateRange()
		{
			return 24;
		}

		public override int GetUpdateRange(Mobile m)
		{
			var w = CurrentState.Components.Width;
			var h = CurrentState.Components.Height - 1;
			var v = 18 + ((w > h ? w : h) / 2);

			if (v > 24)
			{
				v = 24;
			}
			else if (v < 18)
			{
				v = 18;
			}

			return v;
		}

		public DesignState CurrentState
		{
			get
			{
				if (m_Current == null)
				{
					SetInitialState();
				}

				return m_Current;
			}
			set => m_Current = value;
		}

		public DesignState DesignState
		{
			get
			{
				if (m_Design == null)
				{
					SetInitialState();
				}

				return m_Design;
			}
			set => m_Design = value;
		}

		public DesignState BackupState
		{
			get
			{
				if (m_Backup == null)
				{
					SetInitialState();
				}

				return m_Backup;
			}
			set => m_Backup = value;
		}

		public void SetInitialState()
		{
			// This is a new house, it has not yet loaded a design state
			m_Current = new DesignState(this, GetEmptyFoundation());
			m_Design = new DesignState(m_Current);
			m_Backup = new DesignState(m_Current);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (SignHanger != null)
			{
				SignHanger.Delete();
			}

			if (Signpost != null)
			{
				Signpost.Delete();
			}

			if (Fixtures == null)
			{
				return;
			}

			for (var i = 0; i < Fixtures.Count; ++i)
			{
				var item = Fixtures[i];

				if (item != null)
				{
					item.Delete();
				}
			}

			Fixtures.Clear();
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			base.OnLocationChange(oldLocation);

			var x = Location.X - oldLocation.X;
			var y = Location.Y - oldLocation.Y;
			var z = Location.Z - oldLocation.Z;

			if (SignHanger != null)
			{
				SignHanger.MoveToWorld(new Point3D(SignHanger.X + x, SignHanger.Y + y, SignHanger.Z + z), Map);
			}

			if (Signpost != null)
			{
				Signpost.MoveToWorld(new Point3D(Signpost.X + x, Signpost.Y + y, Signpost.Z + z), Map);
			}

			if (Fixtures == null)
			{
				return;
			}

			for (var i = 0; i < Fixtures.Count; ++i)
			{
				var item = Fixtures[i];

				if (Doors.Contains(item))
				{
					continue;
				}

				item.MoveToWorld(new Point3D(item.X + x, item.Y + y, item.Z + z), Map);
			}
		}

		public override void OnMapChange(Map oldMap)
		{
			base.OnMapChange(oldMap);

			if (SignHanger != null)
			{
				SignHanger.Map = Map;
			}

			if (Signpost != null)
			{
				Signpost.Map = Map;
			}

			if (Fixtures == null)
			{
				return;
			}

			for (var i = 0; i < Fixtures.Count; ++i)
			{
				Fixtures[i].Map = Map;
			}
		}

		public void ClearFixtures(Mobile from)
		{
			if (Fixtures == null)
			{
				return;
			}

			RemoveKeys(from);

			for (var i = 0; i < Fixtures.Count; ++i)
			{
				Fixtures[i].Delete();

				if (Doors != null && Fixtures[i] is BaseDoor door)
				{
					_ = Doors.Remove(door);
				}
			}

			Fixtures.Clear();
		}

		public void AddFixtures(Mobile from, MultiTileEntry[] list)
		{
			Fixtures ??= new();

			uint keyValue = 0;

			for (var i = 0; i < list.Length; ++i)
			{
				var mte = list[i];
				int itemID = mte.ItemID;

				if (itemID is >= 0x181D and < 0x1829)
				{
					var tp = new HouseTeleporter(itemID);

					AddFixture(tp, mte);
				}
				else
				{
					BaseDoor door = null;

					if (itemID is >= 0x675 and < 0x6F5)
					{
						var type = (itemID - 0x675) / 16;
						var facing = (DoorFacing)((itemID - 0x675) / 2 % 8);

						switch (type)
						{
							case 0: door = new GenericHouseDoor(facing, 0x675, 0xEC, 0xF3); break;
							case 1: door = new GenericHouseDoor(facing, 0x685, 0xEC, 0xF3); break;
							case 2: door = new GenericHouseDoor(facing, 0x695, 0xEB, 0xF2); break;
							case 3: door = new GenericHouseDoor(facing, 0x6A5, 0xEA, 0xF1); break;
							case 4: door = new GenericHouseDoor(facing, 0x6B5, 0xEA, 0xF1); break;
							case 5: door = new GenericHouseDoor(facing, 0x6C5, 0xEC, 0xF3); break;
							case 6: door = new GenericHouseDoor(facing, 0x6D5, 0xEA, 0xF1); break;
							case 7: door = new GenericHouseDoor(facing, 0x6E5, 0xEA, 0xF1); break;
						}
					}
					else if (itemID is >= 0x314 and < 0x364)
					{
						var type = (itemID - 0x314) / 16;
						var facing = (DoorFacing)((itemID - 0x314) / 2 % 8);
						door = new GenericHouseDoor(facing, 0x314 + (type * 16), 0xED, 0xF4);
					}
					else if (itemID is >= 0x824 and < 0x834)
					{
						var facing = (DoorFacing)((itemID - 0x824) / 2 % 8);
						door = new GenericHouseDoor(facing, 0x824, 0xEC, 0xF3);
					}
					else if (itemID is >= 0x839 and < 0x849)
					{
						var facing = (DoorFacing)((itemID - 0x839) / 2 % 8);
						door = new GenericHouseDoor(facing, 0x839, 0xEB, 0xF2);
					}
					else if (itemID is >= 0x84C and < 0x85C)
					{
						var facing = (DoorFacing)((itemID - 0x84C) / 2 % 8);
						door = new GenericHouseDoor(facing, 0x84C, 0xEC, 0xF3);
					}
					else if (itemID is >= 0x866 and < 0x876)
					{
						var facing = (DoorFacing)((itemID - 0x866) / 2 % 8);
						door = new GenericHouseDoor(facing, 0x866, 0xEB, 0xF2);
					}
					else if (itemID is >= 0xE8 and < 0xF8)
					{
						var facing = (DoorFacing)((itemID - 0xE8) / 2 % 8);
						door = new GenericHouseDoor(facing, 0xE8, 0xED, 0xF4);
					}
					else if (itemID is >= 0x1FED and < 0x1FFD)
					{
						var facing = (DoorFacing)((itemID - 0x1FED) / 2 % 8);
						door = new GenericHouseDoor(facing, 0x1FED, 0xEC, 0xF3);
					}
					else if (itemID is >= 0x241F and < 0x2421)
					{
						//DoorFacing facing = (DoorFacing)(((itemID - 0x241F) / 2) % 8);
						door = new GenericHouseDoor(DoorFacing.NorthCCW, 0x2415, -1, -1);
					}
					else if (itemID is >= 0x2423 and < 0x2425)
					{
						//DoorFacing facing = (DoorFacing)(((itemID - 0x241F) / 2) % 8);
						//This one and the above one are 'special' cases, ie: OSI had the ItemID pattern discombobulated for these
						door = new GenericHouseDoor(DoorFacing.WestCW, 0x2423, -1, -1);
					}
					else if (itemID is >= 0x2A05 and < 0x2A1D)
					{
						var facing = (DoorFacing)(((itemID - 0x2A05) / 2 % 4) + 8);

						var sound = (itemID is >= 0x2A0D and < 0x2a15) ? 0x539 : -1;

						door = new GenericHouseDoor(facing, 0x29F5 + (8 * ((itemID - 0x2A05) / 8)), sound, sound);
					}
					else if (itemID == 0x2D46)
					{
						door = new GenericHouseDoor(DoorFacing.NorthCW, 0x2D46, 0xEA, 0xF1, false);
					}
					else if (itemID is 0x2D48 or 0x2FE2)
					{
						door = new GenericHouseDoor(DoorFacing.SouthCCW, itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x2D63 and < 0x2D70)
					{
						var mod = (itemID - 0x2D63) / 2 % 2;
						var facing = (mod == 0) ? DoorFacing.SouthCCW : DoorFacing.WestCCW;

						var type = (itemID - 0x2D63) / 4;

						door = new GenericHouseDoor(facing, 0x2D63 + (4 * type) + (mod * 2), 0xEA, 0xF1, false);
					}
					else if (itemID is 0x2FE4 or 0x31AE)
					{
						door = new GenericHouseDoor(DoorFacing.WestCCW, itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x319C and < 0x31AE)
					{
						//special case for 0x31aa <-> 0x31a8 (a9)

						var mod = (itemID - 0x319C) / 2 % 2;
						_ = itemID is 0x31AA or 0x31A8;

						DoorFacing facing;

						if (itemID is 0x31AA or 0x31A8)
						{
							facing = (mod == 0) ? DoorFacing.NorthCW : DoorFacing.EastCW;
						}
						else
						{
							facing = (mod == 0) ? DoorFacing.EastCW : DoorFacing.NorthCW;
						}

						var type = (itemID - 0x319C) / 4;

						door = new GenericHouseDoor(facing, 0x319C + (4 * type) + (mod * 2), 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x367B and < 0x369B)
					{
						var type = (itemID - 0x367B) / 16;
						var facing = (DoorFacing)((itemID - 0x367B) / 2 % 8);

						switch (type)
						{
							case 0: door = new GenericHouseDoor(facing, 0x367B, 0xED, 0xF4); break; //crystal
							case 1: door = new GenericHouseDoor(facing, 0x368B, 0xEC, 0x3E7); break;    //shadow
						}
					}
					else if (itemID is >= 0x409B and < 0x40A3)
					{
						door = new GenericHouseDoor(GetSADoorFacing(itemID - 0x409B), itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x410C and < 0x4114)
					{
						door = new GenericHouseDoor(GetSADoorFacing(itemID - 0x410C), itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x41C2 and < 0x41CA)
					{
						door = new GenericHouseDoor(GetSADoorFacing(itemID - 0x41C2), itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x41CF and < 0x41D7)
					{
						door = new GenericHouseDoor(GetSADoorFacing(itemID - 0x41CF), itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x436E and < 0x437E)
					{
						/* These ones had to be different...
						 * Offset		0	2	4	6	8	10	12	14
						 * DoorFacing	2	3	2	3	6	7	6	7
						 */
						var offset = itemID - 0x436E;
						var facing = (DoorFacing)(((offset / 2) + (2 * ((1 + (offset / 4)) % 2))) % 8);
						door = new GenericHouseDoor(facing, itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x46DD and < 0x46E5)
					{
						door = new GenericHouseDoor(GetSADoorFacing(itemID - 0x46DD), itemID, 0xEB, 0xF2, false);
					}
					else if (itemID is >= 0x4D22 and < 0x4D2A)
					{
						door = new GenericHouseDoor(GetSADoorFacing(itemID - 0x4D22), itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x50C8 and < 0x50D0)
					{
						door = new GenericHouseDoor(GetSADoorFacing(itemID - 0x50C8), itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x50D0 and < 0x50D8)
					{
						door = new GenericHouseDoor(GetSADoorFacing(itemID - 0x50D0), itemID, 0xEA, 0xF1, false);
					}
					else if (itemID is >= 0x5142 and < 0x514A)
					{
						door = new GenericHouseDoor(GetSADoorFacing(itemID - 0x5142), itemID, 0xF0, 0xEF, false);
					}

					if (door != null)
					{
						if (keyValue == 0)
						{
							keyValue = CreateKeys(from);
						}

						door.Locked = true;
						door.KeyValue = keyValue;

						AddDoor(door, mte.OffsetX, mte.OffsetY, mte.OffsetZ);
						Fixtures.Add(door);
					}
				}
			}

			for (var i = 0; i < Fixtures.Count; ++i)
			{
				var fixture = Fixtures[i];

				if (fixture is HouseTeleporter tp)
				{
					for (var j = 1; j <= Fixtures.Count; ++j)
					{
						if (Fixtures[(i + j) % Fixtures.Count] is HouseTeleporter check && check.ItemID == tp.ItemID)
						{
							tp.Target = check;
							break;
						}
					}
				}
				else if (fixture is BaseHouseDoor door)
				{
					if (door.Link != null)
					{
						continue;
					}

					DoorFacing linkFacing;
					int xOffset, yOffset;

					switch (door.Facing)
					{
						default:
						case DoorFacing.WestCW: linkFacing = DoorFacing.EastCCW; xOffset = 1; yOffset = 0; break;
						case DoorFacing.EastCCW: linkFacing = DoorFacing.WestCW; xOffset = -1; yOffset = 0; break;
						case DoorFacing.WestCCW: linkFacing = DoorFacing.EastCW; xOffset = 1; yOffset = 0; break;
						case DoorFacing.EastCW: linkFacing = DoorFacing.WestCCW; xOffset = -1; yOffset = 0; break;
						case DoorFacing.SouthCW: linkFacing = DoorFacing.NorthCCW; xOffset = 0; yOffset = -1; break;
						case DoorFacing.NorthCCW: linkFacing = DoorFacing.SouthCW; xOffset = 0; yOffset = 1; break;
						case DoorFacing.SouthCCW: linkFacing = DoorFacing.NorthCW; xOffset = 0; yOffset = -1; break;
						case DoorFacing.NorthCW: linkFacing = DoorFacing.SouthCCW; xOffset = 0; yOffset = 1; break;
						case DoorFacing.SouthSW: linkFacing = DoorFacing.SouthSE; xOffset = 1; yOffset = 0; break;
						case DoorFacing.SouthSE: linkFacing = DoorFacing.SouthSW; xOffset = -1; yOffset = 0; break;
						case DoorFacing.WestSN: linkFacing = DoorFacing.WestSS; xOffset = 0; yOffset = 1; break;
						case DoorFacing.WestSS: linkFacing = DoorFacing.WestSN; xOffset = 0; yOffset = -1; break;
					}

					for (var j = i + 1; j < Fixtures.Count; ++j)
					{
						if (Fixtures[j] is BaseHouseDoor check && check.Link == null && check.Facing == linkFacing && (check.X - door.X) == xOffset && (check.Y - door.Y) == yOffset && (check.Z == door.Z))
						{
							check.Link = door;
							door.Link = check;
							break;
						}
					}
				}
			}
		}

		private static DoorFacing GetSADoorFacing(int offset)
		{
			/* Offset		0	2	4	6
			 * DoorFacing	2	3	6	7
			 */
			return (DoorFacing)(((offset / 2) + (2 * (1 + (offset / 4)))) % 8);
		}

		public void AddFixture(Item item, MultiTileEntry mte)
		{
			Fixtures.Add(item);
			item.MoveToWorld(new Point3D(X + mte.OffsetX, Y + mte.OffsetY, Z + mte.OffsetZ), Map);
		}

		public static void GetFoundationGraphics(FoundationType type, out int east, out int south, out int post, out int corner)
		{
			switch (type)
			{
				default:
				case FoundationType.DarkWood: corner = 0x0014; east = 0x0015; south = 0x0016; post = 0x0017; break;
				case FoundationType.LightWood: corner = 0x00BD; east = 0x00BE; south = 0x00BF; post = 0x00C0; break;
				case FoundationType.Dungeon: corner = 0x02FD; east = 0x02FF; south = 0x02FE; post = 0x0300; break;
				case FoundationType.Brick: corner = 0x0041; east = 0x0043; south = 0x0042; post = 0x0044; break;
				case FoundationType.Stone: corner = 0x0065; east = 0x0064; south = 0x0063; post = 0x0066; break;

				case FoundationType.ElvenGrey: corner = 0x2DF7; east = 0x2DF9; south = 0x2DFA; post = 0x2DF8; break;
				case FoundationType.ElvenNatural: corner = 0x2DFB; east = 0x2DFD; south = 0x2DFE; post = 0x2DFC; break;

				case FoundationType.Crystal: corner = 0x3672; east = 0x3671; south = 0x3670; post = 0x3673; break;
				case FoundationType.Shadow: corner = 0x3676; east = 0x3675; south = 0x3674; post = 0x3677; break;
			}
		}

		public static void ApplyFoundation(FoundationType type, MultiComponentList mcl)
		{
			int east, south, post, corner;

			GetFoundationGraphics(type, out east, out south, out post, out corner);

			var xCenter = mcl.Center.X;
			var yCenter = mcl.Center.Y;

			mcl.Add(post, 0 - xCenter, 0 - yCenter, 0);
			mcl.Add(corner, mcl.Width - 1 - xCenter, mcl.Height - 2 - yCenter, 0);

			for (var x = 1; x < mcl.Width; ++x)
			{
				mcl.Add(south, x - xCenter, 0 - yCenter, 0);

				if (x < mcl.Width - 1)
				{
					mcl.Add(south, x - xCenter, mcl.Height - 2 - yCenter, 0);
				}
			}

			for (var y = 1; y < mcl.Height - 1; ++y)
			{
				mcl.Add(east, 0 - xCenter, y - yCenter, 0);

				if (y < mcl.Height - 2)
				{
					mcl.Add(east, mcl.Width - 1 - xCenter, y - yCenter, 0);
				}
			}
		}

		public static void AddStairsTo(ref MultiComponentList mcl)
		{
			// copy the original..
			mcl = new MultiComponentList(mcl);

			mcl.Resize(mcl.Width, mcl.Height + 1);

			var xCenter = mcl.Center.X;
			var yCenter = mcl.Center.Y;
			var y = mcl.Height - 1;

			for (var x = 0; x < mcl.Width; ++x)
			{
				mcl.Add(0x63, x - xCenter, y - yCenter, 0);
			}
		}

		public MultiComponentList GetEmptyFoundation()
		{
			// Copy original foundation layout
			var mcl = new MultiComponentList(MultiData.GetComponents(ItemID));

			mcl.Resize(mcl.Width, mcl.Height + 1);

			var xCenter = mcl.Center.X;
			var yCenter = mcl.Center.Y;
			var y = mcl.Height - 1;

			ApplyFoundation(Type, mcl);

			for (var x = 1; x < mcl.Width; ++x)
			{
				mcl.Add(0x751, x - xCenter, y - yCenter, 0);
			}

			return mcl;
		}

		public override Rectangle2D[] Area
		{
			get
			{
				var mcl = Components;

				return new Rectangle2D[] { new Rectangle2D(mcl.Min.X, mcl.Min.Y, mcl.Width, mcl.Height) };
			}
		}

		public override Point3D BaseBanLocation => new(Components.Min.X, Components.Height - 1 - Components.Center.Y, 0);

		public void CheckSignpost()
		{
			var mcl = Components;

			var x = mcl.Min.X;
			var y = mcl.Height - 2 - mcl.Center.Y;

			if (CheckWall(mcl, x, y))
			{
				if (Signpost != null)
				{
					Signpost.Delete();
				}

				Signpost = null;
			}
			else if (Signpost == null)
			{
				Signpost = new Static(SignpostGraphic);
				Signpost.MoveToWorld(new Point3D(X + x, Y + y, Z + 7), Map);
			}
			else
			{
				Signpost.ItemID = SignpostGraphic;
				Signpost.MoveToWorld(new Point3D(X + x, Y + y, Z + 7), Map);
			}
		}

		public bool CheckWall(MultiComponentList mcl, int x, int y)
		{
			x += mcl.Center.X;
			y += mcl.Center.Y;

			if (x >= 0 && x < mcl.Width && y >= 0 && y < mcl.Height)
			{
				var tiles = mcl.Tiles[x][y];

				for (var i = 0; i < tiles.Length; ++i)
				{
					var tile = tiles[i];

					if (tile.Z == 7 && tile.Height == 20)
					{
						return true;
					}
				}
			}

			return false;
		}

		public HouseFoundation(Mobile owner, int multiID, int maxLockdowns, int maxSecures)
			: base(multiID, owner, maxLockdowns, maxSecures)
		{
			SignpostGraphic = 9;

			Fixtures = new List<Item>();

			var x = Components.Min.X;
			var y = Components.Height - 1 - Components.Center.Y;

			SignHanger = new Static(0xB98);
			SignHanger.MoveToWorld(new Point3D(X + x, Y + y, Z + 7), Map);

			CheckSignpost();

			SetSign(x, y, 7);
		}

		public HouseFoundation(Serial serial)
			: base(serial)
		{
		}

		public void BeginCustomize(Mobile m)
		{
			if (!m.CheckAlive())
			{
				return;
			}
			else if (SpellHelper.CheckCombat(m))
			{
				m.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
				return;
			}

			RelocateEntities();

			foreach (var item in GetItems())
			{
				item.Location = BanLocation;
			}

			foreach (var mobile in GetMobiles())
			{
				if (mobile != m)
				{
					mobile.Location = BanLocation;
				}
			}

			DesignContext.Add(m, this);
			_ = m.Send(new BeginHouseCustomization(this));

			var ns = m.NetState;
			if (ns != null)
			{
				SendInfoTo(ns);
			}

			DesignState.SendDetailedInfoTo(ns);
		}

		public override void SendInfoTo(NetState state, bool sendOplPacket)
		{
			base.SendInfoTo(state, sendOplPacket);

			var context = DesignContext.Find(state.Mobile);
			DesignState stateToSend;

			if (context != null && context.Foundation == this)
			{
				stateToSend = DesignState;
			}
			else
			{
				stateToSend = CurrentState;
			}

			stateToSend.SendGeneralInfoTo(state);
		}

		public override void Serialize(GenericWriter writer)
		{
			writer.Write(5); // version

			writer.Write(Signpost);
			writer.Write(SignpostGraphic);

			writer.Write((int)Type);

			writer.Write(SignHanger);

			writer.Write(LastRevision);
			writer.Write(Fixtures, true);

			CurrentState.Serialize(writer);
			DesignState.Serialize(writer);
			BackupState.Serialize(writer);

			base.Serialize(writer);
		}

		private int m_DefaultPrice;

		public override int DefaultPrice => m_DefaultPrice;

		public override void Deserialize(GenericReader reader)
		{
			var version = reader.ReadInt();

			switch (version)
			{
				case 5:
				case 4:
					{
						Signpost = reader.ReadItem();
						SignpostGraphic = reader.ReadInt();

						goto case 3;
					}
				case 3:
					{
						Type = (FoundationType)reader.ReadInt();

						goto case 2;
					}
				case 2:
					{
						SignHanger = reader.ReadItem();

						goto case 1;
					}
				case 1:
					{
						if (version < 5)
						{
							m_DefaultPrice = reader.ReadInt();
						}

						goto case 0;
					}
				case 0:
					{
						if (version < 3)
						{
							Type = FoundationType.Stone;
						}

						if (version < 4)
						{
							SignpostGraphic = 9;
						}

						LastRevision = reader.ReadInt();
						Fixtures = reader.ReadStrongItemList();

						m_Current = new DesignState(this, reader);
						m_Design = new DesignState(this, reader);
						m_Backup = new DesignState(this, reader);

						break;
					}
			}

			base.Deserialize(reader);
		}

		public bool IsHiddenToCustomizer(Item item)
		{
			return item == Signpost || item == SignHanger || item == Sign || IsFixture(item);
		}

		public static void Initialize()
		{
			PacketHandlers.RegisterExtended(0x1E, true, new OnPacketReceive(QueryDesignDetails));

			PacketHandlers.RegisterEncoded(0x02, true, new OnEncodedPacketReceive(Designer_Backup));
			PacketHandlers.RegisterEncoded(0x03, true, new OnEncodedPacketReceive(Designer_Restore));
			PacketHandlers.RegisterEncoded(0x04, true, new OnEncodedPacketReceive(Designer_Commit));
			PacketHandlers.RegisterEncoded(0x05, true, new OnEncodedPacketReceive(Designer_Delete));
			PacketHandlers.RegisterEncoded(0x06, true, new OnEncodedPacketReceive(Designer_Build));
			PacketHandlers.RegisterEncoded(0x0C, true, new OnEncodedPacketReceive(Designer_Close));
			PacketHandlers.RegisterEncoded(0x0D, true, new OnEncodedPacketReceive(Designer_Stairs));
			PacketHandlers.RegisterEncoded(0x0E, true, new OnEncodedPacketReceive(Designer_Sync));
			PacketHandlers.RegisterEncoded(0x10, true, new OnEncodedPacketReceive(Designer_Clear));
			PacketHandlers.RegisterEncoded(0x12, true, new OnEncodedPacketReceive(Designer_Level));

			PacketHandlers.RegisterEncoded(0x13, true, new OnEncodedPacketReceive(Designer_Roof)); // Samurai Empire roof
			PacketHandlers.RegisterEncoded(0x14, true, new OnEncodedPacketReceive(Designer_RoofDelete)); // Samurai Empire roof

			PacketHandlers.RegisterEncoded(0x1A, true, new OnEncodedPacketReceive(Designer_Revert));

			EventSink.Speech += new SpeechEventHandler(EventSink_Speech);
		}

		private static void EventSink_Speech(SpeechEventArgs e)
		{
			if (DesignContext.Find(e.Mobile) != null)
			{
				e.Mobile.SendLocalizedMessage(1061925); // You cannot speak while customizing your house.
				e.Blocked = true;
			}
		}

		public static void Designer_Sync(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client requested state synchronization
				 *  - Resend full house state
				 */

				var design = context.Foundation.DesignState;

				// Resend full house state
				design.SendDetailedInfoTo(state);
			}
		}

		public static void Designer_Clear(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client chose to clear the design
				 *  - Restore empty foundation
				 *     - Construct new design state from empty foundation
				 *     - Assign constructed state to foundation
				 *  - Update revision
				 *  - Update client with new state
				 */

				// Restore empty foundation : Construct new design state from empty foundation
				var newDesign = new DesignState(context.Foundation, context.Foundation.GetEmptyFoundation());

				// Restore empty foundation : Assign constructed state to foundation
				context.Foundation.DesignState = newDesign;

				// Update revision
				newDesign.OnRevised();

				// Update client with new state
				context.Foundation.SendInfoTo(state);
				newDesign.SendDetailedInfoTo(state);
			}
		}

		public static void Designer_Restore(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client chose to restore design to the last backup state
				 *  - Restore backup
				 *     - Construct new design state from backup state
				 *     - Assign constructed state to foundation
				 *  - Update revision
				 *  - Update client with new state
				 */

				// Restore backup : Construct new design state from backup state
				var backupDesign = new DesignState(context.Foundation.BackupState);

				// Restore backup : Assign constructed state to foundation
				context.Foundation.DesignState = backupDesign;

				// Update revision;
				backupDesign.OnRevised();

				// Update client with new state
				context.Foundation.SendInfoTo(state);
				backupDesign.SendDetailedInfoTo(state);
			}
		}

		public static void Designer_Backup(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client chose to backup design state
				 *  - Construct a copy of the current design state
				 *  - Assign constructed state to backup state field
				 */

				// Construct a copy of the current design state
				var copyState = new DesignState(context.Foundation.DesignState);

				// Assign constructed state to backup state field
				context.Foundation.BackupState = copyState;
			}
		}

		public static void Designer_Revert(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client chose to revert design state to currently visible state
				 *  - Revert design state
				 *     - Construct a copy of the current visible state
				 *     - Freeze fixtures in constructed state
				 *     - Assign constructed state to foundation
				 *     - If a signpost is needed, add it
				 *  - Update revision
				 *  - Update client with new state
				 */

				// Revert design state : Construct a copy of the current visible state
				var copyState = new DesignState(context.Foundation.CurrentState);

				// Revert design state : Freeze fixtures in constructed state
				copyState.FreezeFixtures();

				// Revert design state : Assign constructed state to foundation
				context.Foundation.DesignState = copyState;

				// Revert design state : If a signpost is needed, add it
				context.Foundation.CheckSignpost();

				// Update revision
				copyState.OnRevised();

				// Update client with new state
				context.Foundation.SendInfoTo(state);
				copyState.SendDetailedInfoTo(state);
			}
		}

		public void EndConfirmCommit(Mobile from)
		{
			var oldPrice = Price;
			var newPrice = oldPrice + CustomizationCost + ((DesignState.Components.List.Length - (CurrentState.Components.List.Length + CurrentState.Fixtures.Length)) * 500);
			var cost = newPrice - oldPrice;

			if (!Deleted)
			{ // Temporary Fix. We should be booting a client out of customization mode in the delete handler.
				if (from.AccessLevel >= AccessLevel.GameMaster && cost != 0)
				{
					from.SendMessage("{0} gold would have been {1} your bank if you were not a GM.", cost.ToString(), (cost > 0) ? "withdrawn from" : "deposited into");
				}
				else
				{
					if (cost > 0)
					{
						if (Banker.Withdraw(from, cost))
						{
							from.SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
						}
						else
						{
							from.SendLocalizedMessage(1061903); // You cannot commit this house design, because you do not have the necessary funds in your bank box to pay for the upgrade.  Please back up your design, obtain the required funds, and commit your design again.
							return;
						}
					}
					else if (cost < 0)
					{
						if (Banker.Deposit(from, -cost))
						{
							from.SendLocalizedMessage(1060397, (-cost).ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
						}
						else
						{
							return;
						}
					}
				}
			}

			/* Client chose to commit current design state
				 *  - Commit design state
				 *     - Construct a copy of the current design state
				 *     - Clear visible fixtures
				 *     - Melt fixtures from constructed state
				 *     - Add melted fixtures from constructed state
				 *     - Assign constructed state to foundation
				 *  - Update house price
				 *  - Remove design context
				 *  - Notify the client that customization has ended
				 *  - Notify the core that the foundation has changed and should be resent to all clients
				 *  - If a signpost is needed, add it
				 *  - Eject all from house
				 *  - Restore relocated entities
				 */

			// Commit design state : Construct a copy of the current design state
			var copyState = new DesignState(DesignState);

			// Commit design state : Clear visible fixtures
			ClearFixtures(from);

			// Commit design state : Melt fixtures from constructed state
			copyState.MeltFixtures();

			// Commit design state : Add melted fixtures from constructed state
			AddFixtures(from, copyState.Fixtures);

			// Commit design state : Assign constructed state to foundation
			CurrentState = copyState;

			// Update house price
			Price = newPrice - CustomizationCost;

			// Remove design context
			DesignContext.Remove(from);

			// Notify the client that customization has ended
			_ = from.Send(new EndHouseCustomization(this));

			// Notify the core that the foundation has changed and should be resent to all clients
			Delta(ItemDelta.Update);
			ProcessDelta();
			CurrentState.SendDetailedInfoTo(from.NetState);

			// If a signpost is needed, add it
			CheckSignpost();

			// Eject all from house
			from.RevealingAction();

			foreach (var item in GetItems())
			{
				item.Location = BanLocation;
			}

			foreach (var mobile in GetMobiles())
			{
				mobile.Location = BanLocation;
			}

			// Restore relocated entities
			RestoreRelocatedEntities();
		}

		public static void Designer_Commit(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				var oldPrice = context.Foundation.Price;
				var newPrice = oldPrice + context.Foundation.CustomizationCost + ((context.Foundation.DesignState.Components.List.Length - (context.Foundation.CurrentState.Components.List.Length + context.Foundation.Fixtures.Count)) * 500);
				var bankBalance = Banker.GetBalance(from);

				_ = from.SendGump(new ConfirmCommitGump(from, context.Foundation, bankBalance, oldPrice, newPrice));
			}
		}

		public int MaxLevels
		{
			get
			{
				var mcl = Components;

				if (mcl.Width >= 14 || mcl.Height >= 14)
				{
					return 4;
				}
				else
				{
					return 3;
				}
			}
		}

		public static int GetLevelZ(int level, HouseFoundation house)
		{
			if (level < 1 || level > house.MaxLevels)
			{
				level = 1;
			}

			return ((level - 1) * 20) + 7;

			/*
			switch( level )
			{
				default:
				case 1: return 07;
				case 2: return 27;
				case 3: return 47;
				case 4: return 67;
			}
			 * */
		}

		public static int GetZLevel(int z, HouseFoundation house)
		{
			var level = ((z - 7) / 20) + 1;

			if (level < 1 || level > house.MaxLevels)
			{
				level = 1;
			}

			return level;
		}

		private static ComponentVerification m_Verification;

		public static ComponentVerification Verification
		{
			get
			{
				m_Verification ??= new ComponentVerification();

				return m_Verification;
			}
		}

		public static bool ValidPiece(int itemID)
		{
			return ValidPiece(itemID, false);
		}

		public static bool ValidPiece(int itemID, bool roof)
		{
			itemID &= TileData.MaxItemValue;

			if (!roof && (TileData.ItemTable[itemID].Flags & TileFlag.Roof) != 0)
			{
				return false;
			}
			else if (roof && (TileData.ItemTable[itemID].Flags & TileFlag.Roof) == 0)
			{
				return false;
			}

			return Verification.IsItemValid(itemID);
		}

		public static readonly bool AllowStairSectioning = true;

		/* Stair block IDs
		 * (sorted ascending)
		 */
		private static readonly int[] m_BlockIDs = new int[]
			{
				0x3EE, 0x709, 0x71E, 0x721,
				0x738, 0x750, 0x76C, 0x788,
				0x7A3, 0x7BA, 0x35D2, 0x3609,
				0x4317, 0x4318, 0x4B07, 0x7807
			};

		/* Stair sequence IDs
		 * (sorted ascending)
		 * Use this for stairs in the proper N,W,S,E sequence
		 */
		private static readonly int[] m_StairSeqs = new int[]
			{
				0x3EF, 0x70A, 0x722, 0x739,
				0x751, 0x76D, 0x789, 0x7A4
			};

		/* Other stair IDs
		 * Listed in order: north, west, south, east
		 * Use this for stairs not in the proper sequence
		 */
		private static readonly int[] m_StairIDs = new int[]
			{
				0x71F, 0x736, 0x737, 0x749,
				0x35D4, 0x35D3, 0x35D6, 0x35D5,
				0x360B, 0x360A, 0x360D, 0x360C,
				0x4360, 0x435E, 0x435F, 0x4361,
				0x435C, 0x435A, 0x435B, 0x435D,
				0x4364, 0x4362, 0x4363, 0x4365,
				0x4B05, 0x4B04, 0x4B34, 0x4B33,
				0x7809, 0x7808, 0x780A, 0x780B,
				0x7BB, 0x7BC
			};

		public static bool IsStairBlock(int id)
		{
			var delta = -1;

			for (var i = 0; delta < 0 && i < m_BlockIDs.Length; ++i)
			{
				delta = m_BlockIDs[i] - id;
			}

			return delta == 0;
		}

		public static bool IsStair(int id, ref int dir)
		{
			//dir n=0 w=1 s=2 e=3
			var delta = -4;

			for (var i = 0; delta < -3 && i < m_StairSeqs.Length; ++i)
			{
				delta = m_StairSeqs[i] - id;
			}

			if (delta is >= (-3) and <= 0)
			{
				dir = -delta;
				return true;
			}

			for (var i = 0; i < m_StairIDs.Length; ++i)
			{
				if (m_StairIDs[i] == id)
				{
					dir = i % 4;
					return true;
				}
			}

			return false;
		}

		public static bool DeleteStairs(MultiComponentList mcl, int id, int x, int y, int z)
		{
			var ax = x + mcl.Center.X;
			var ay = y + mcl.Center.Y;

			if (ax < 0 || ay < 0 || ax >= mcl.Width || ay >= (mcl.Height - 1) || z < 7 || ((z - 7) % 5) != 0)
			{
				return false;
			}

			if (IsStairBlock(id))
			{
				var tiles = mcl.Tiles[ax][ay];

				for (var i = 0; i < tiles.Length; ++i)
				{
					var tile = tiles[i];

					if (tile.Z == (z + 5))
					{
						id = tile.ID;
						z = tile.Z;

						if (!IsStairBlock(id))
						{
							break;
						}
					}
				}
			}

			var dir = 0;

			if (!IsStair(id, ref dir))
			{
				return false;
			}

			if (AllowStairSectioning)
			{
				return true; // skip deletion
			}

			var height = (z - 7) % 20 / 5;

			int xStart, yStart;
			int xInc, yInc;

			switch (dir)
			{
				default:
				case 0: // North
					{
						xStart = x;
						yStart = y + height;
						xInc = 0;
						yInc = -1;
						break;
					}
				case 1: // West
					{
						xStart = x + height;
						yStart = y;
						xInc = -1;
						yInc = 0;
						break;
					}
				case 2: // South
					{
						xStart = x;
						yStart = y - height;
						xInc = 0;
						yInc = 1;
						break;
					}
				case 3: // East
					{
						xStart = x - height;
						yStart = y;
						xInc = 1;
						yInc = 0;
						break;
					}
			}

			var zStart = z - (height * 5);

			for (var i = 0; i < 4; ++i)
			{
				x = xStart + (i * xInc);
				y = yStart + (i * yInc);

				for (var j = 0; j <= i; ++j)
				{
					mcl.RemoveXYZH(x, y, zStart + (j * 5), 5);
				}

				ax = x + mcl.Center.X;
				ay = y + mcl.Center.Y;

				if (ax >= 1 && ax < mcl.Width && ay >= 1 && ay < mcl.Height - 1)
				{
					var tiles = mcl.Tiles[ax][ay];

					var hasBaseFloor = false;

					for (var j = 0; !hasBaseFloor && j < tiles.Length; ++j)
					{
						hasBaseFloor = tiles[j].Z == 7 && tiles[j].ID != 1;
					}

					if (!hasBaseFloor)
					{
						mcl.Add(0x31F4, x, y, 7);
					}
				}
			}

			return true;
		}

		public static void Designer_Delete(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client chose to delete a component
				 *  - Read data detailing which component to delete
				 *  - Verify component is deletable
				 *  - Remove the component
				 *  - If needed, replace removed component with a dirt tile
				 *  - Update revision
				 */

				// Read data detailing which component to delete
				var itemID = pvSrc.ReadInt32();
				var x = pvSrc.ReadInt32();
				var y = pvSrc.ReadInt32();
				var z = pvSrc.ReadInt32();

				// Verify component is deletable
				var design = context.Foundation.DesignState;
				var mcl = design.Components;

				var ax = x + mcl.Center.X;
				var ay = y + mcl.Center.Y;

				if (z == 0 && ax >= 0 && ax < mcl.Width && ay >= 0 && ay < (mcl.Height - 1))
				{
					/* Component is not deletable
					 *  - Resend design state
					 *  - Return without further processing
					 */

					design.SendDetailedInfoTo(state);
					return;
				}

				var fixState = false;

				// Remove the component
				if (AllowStairSectioning)
				{
					if (DeleteStairs(mcl, itemID, x, y, z))
					{
						fixState = true; // The client removes the entire set of stairs locally, resend state
					}

					mcl.Remove(itemID, x, y, z);
				}
				else
				{
					if (!DeleteStairs(mcl, itemID, x, y, z))
					{
						mcl.Remove(itemID, x, y, z);
					}
				}

				// If needed, replace removed component with a dirt tile
				if (ax >= 1 && ax < mcl.Width && ay >= 1 && ay < mcl.Height - 1)
				{
					var tiles = mcl.Tiles[ax][ay];

					var hasBaseFloor = false;

					for (var i = 0; !hasBaseFloor && i < tiles.Length; ++i)
					{
						hasBaseFloor = tiles[i].Z == 7 && tiles[i].ID != 1;
					}

					if (!hasBaseFloor)
					{
						// Replace with a dirt tile
						mcl.Add(0x31F4, x, y, 7);
					}
				}

				// Update revision
				design.OnRevised();

				// Resend design state
				if (fixState)
				{
					design.SendDetailedInfoTo(state);
				}
			}
		}

		public static void Designer_Stairs(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client chose to add stairs
				 *  - Read data detailing stair type and location
				 *  - Validate stair multi ID
				 *  - Add the stairs
				 *     - Load data describing the stair components
				 *     - Insert described components
				 *  - Update revision
				 */

				// Read data detailing stair type and location
				var itemID = pvSrc.ReadInt32();
				var x = pvSrc.ReadInt32();
				var y = pvSrc.ReadInt32();

				// Validate stair multi ID
				var design = context.Foundation.DesignState;

				if (!Verification.IsMultiValid(itemID))
				{
					/* Specified multi ID is not a stair
					 *  - Resend design state
					 *  - Return without further processing
					 */

					TraceValidity(state, itemID);
					design.SendDetailedInfoTo(state);
					return;
				}

				// Add the stairs
				var mcl = design.Components;

				// Add the stairs : Load data describing stair components
				var stairs = MultiData.GetComponents(itemID);

				// Add the stairs : Insert described components
				var z = GetLevelZ(context.Level, context.Foundation);

				for (var i = 0; i < stairs.List.Length; ++i)
				{
					var entry = stairs.List[i];

					if (entry.ItemID != 1)
					{
						mcl.Add(entry.ItemID, x + entry.OffsetX, y + entry.OffsetY, z + entry.OffsetZ);
					}
				}

				// Update revision
				design.OnRevised();
			}
		}

		private static void TraceValidity(NetState state, int itemID)
		{
			try
			{
				using var op = new StreamWriter("comp_val.log", true);
				op.WriteLine("{0}\t{1}\tInvalid ItemID 0x{2:X4}", state, state.Mobile, itemID);
			}
			catch
			{
			}
		}

		public static void Designer_Build(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client chose to add a component
				 *  - Read data detailing component graphic and location
				 *  - Add component
				 *  - Update revision
				 */

				// Read data detailing component graphic and location
				var itemID = pvSrc.ReadInt32();
				var x = pvSrc.ReadInt32();
				var y = pvSrc.ReadInt32();

				// Add component
				var design = context.Foundation.DesignState;

				if (from.AccessLevel < AccessLevel.GameMaster && !ValidPiece(itemID))
				{
					TraceValidity(state, itemID);
					design.SendDetailedInfoTo(state);
					return;
				}

				var mcl = design.Components;

				var z = GetLevelZ(context.Level, context.Foundation);

				if ((y + mcl.Center.Y) == (mcl.Height - 1))
				{
					z = 0; // Tiles placed on the far-south of the house are at 0 Z
				}

				mcl.Add(itemID, x, y, z);

				// Update revision
				design.OnRevised();
			}
		}

		public static void Designer_Close(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client closed his house design window
				 *  - Remove design context
				 *  - Notify the client that customization has ended
				 *  - Refresh client with current visible design state
				 *  - If a signpost is needed, add it
				 *  - Eject all from house
				 *  - Restore relocated entities
				 */

				// Remove design context
				DesignContext.Remove(from);

				// Notify the client that customization has ended
				_ = from.Send(new EndHouseCustomization(context.Foundation));

				// Refresh client with current visible design state
				context.Foundation.SendInfoTo(state);
				context.Foundation.CurrentState.SendDetailedInfoTo(state);

				// If a signpost is needed, add it
				context.Foundation.CheckSignpost();

				// Eject all from house
				from.RevealingAction();

				foreach (var item in context.Foundation.GetItems())
				{
					item.Location = context.Foundation.BanLocation;
				}

				foreach (var mobile in context.Foundation.GetMobiles())
				{
					mobile.Location = context.Foundation.BanLocation;
				}

				// Restore relocated entities
				context.Foundation.RestoreRelocatedEntities();
			}
		}

		public static void Designer_Level(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client is moving to a new floor level
				 *  - Read data detailing the target level
				 *  - Validate target level
				 *  - Update design context with new level
				 *  - Teleport mobile to new level
				 *  - Update client
				 *
				 */

				// Read data detailing the target level
				var newLevel = pvSrc.ReadInt32();

				// Validate target level
				if (newLevel < 1 || newLevel > context.MaxLevels)
				{
					newLevel = 1;
				}

				// Update design context with new level
				context.Level = newLevel;

				// Teleport mobile to new level
				from.Location = new Point3D(from.X, from.Y, context.Foundation.Z + GetLevelZ(newLevel, context.Foundation));

				// Update client
				context.Foundation.SendInfoTo(state);
			}
		}

		public static void QueryDesignDetails(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (pvSrc.ReadItem() is HouseFoundation foundation && from.Map == foundation.Map && from.InRange(foundation.GetWorldLocation(), 24) && from.CanSee(foundation))
			{
				DesignState stateToSend;

				if (context != null && context.Foundation == foundation)
				{
					stateToSend = foundation.DesignState;
				}
				else
				{
					stateToSend = foundation.CurrentState;
				}

				stateToSend.SendDetailedInfoTo(state);
			}
		}

		public static void Designer_Roof(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null && (Core.SE || from.AccessLevel >= AccessLevel.GameMaster))
			{
				// Read data detailing component graphic and location
				var itemID = pvSrc.ReadInt32();
				var x = pvSrc.ReadInt32();
				var y = pvSrc.ReadInt32();
				var z = pvSrc.ReadInt32();

				// Add component
				var design = context.Foundation.DesignState;

				if (from.AccessLevel < AccessLevel.GameMaster && !ValidPiece(itemID, true))
				{
					TraceValidity(state, itemID);
					design.SendDetailedInfoTo(state);
					return;
				}

				var mcl = design.Components;

				if (z < -3 || z > 12 || z % 3 != 0)
				{
					z = -3;
				}

				z += GetLevelZ(context.Level, context.Foundation);

				var list = mcl.List;
				for (var i = 0; i < list.Length; i++)
				{
					var mte = list[i];

					if (mte.OffsetX == x && mte.OffsetY == y && GetZLevel(mte.OffsetZ, context.Foundation) == context.Level && (TileData.ItemTable[mte.ItemID & TileData.MaxItemValue].Flags & TileFlag.Roof) != 0)
					{
						mcl.Remove(mte.ItemID, x, y, mte.OffsetZ);
					}
				}

				mcl.Add(itemID, x, y, z);

				// Update revision
				design.OnRevised();
			}
		}

		public static void Designer_RoofDelete(NetState state, IEntity e, EncodedReader pvSrc)
		{
			var from = state.Mobile;
			var context = DesignContext.Find(from);

			if (context != null)    // No need to check for Core.SE if trying to remove something that shouldn't be able to be placed anyways
			{
				// Read data detailing which component to delete
				var itemID = pvSrc.ReadInt32();
				var x = pvSrc.ReadInt32();
				var y = pvSrc.ReadInt32();
				var z = pvSrc.ReadInt32();

				// Verify component is deletable
				var design = context.Foundation.DesignState;
				var mcl = design.Components;

				if ((TileData.ItemTable[itemID & TileData.MaxItemValue].Flags & TileFlag.Roof) == 0)
				{
					design.SendDetailedInfoTo(state);
					return;
				}

				mcl.Remove(itemID, x, y, z);

				design.OnRevised();
			}
		}
	}

	public class DesignState
	{
		private Packet m_PacketCache;

		public Packet PacketCache
		{
			get => m_PacketCache;
			set
			{
				if (m_PacketCache == value)
				{
					return;
				}

				if (m_PacketCache != null)
				{
					m_PacketCache.Release();
				}

				m_PacketCache = value;
			}
		}

		public HouseFoundation Foundation { get; }
		public MultiComponentList Components { get; }
		public MultiTileEntry[] Fixtures { get; private set; }
		public int Revision { get; set; }

		public DesignState(HouseFoundation foundation, MultiComponentList components)
		{
			Foundation = foundation;
			Components = components;
			Fixtures = new MultiTileEntry[0];
		}

		public DesignState(DesignState toCopy)
		{
			Foundation = toCopy.Foundation;
			Components = new MultiComponentList(toCopy.Components);
			Revision = toCopy.Revision;
			Fixtures = new MultiTileEntry[toCopy.Fixtures.Length];

			for (var i = 0; i < Fixtures.Length; ++i)
			{
				Fixtures[i] = toCopy.Fixtures[i];
			}
		}

		public DesignState(HouseFoundation foundation, GenericReader reader)
		{
			Foundation = foundation;

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
				case 0:
					{
						Components = new MultiComponentList(reader);

						var length = reader.ReadInt();

						Fixtures = new MultiTileEntry[length];

						for (var i = 0; i < length; ++i)
						{
							Fixtures[i].ItemID = reader.ReadUShort();
							Fixtures[i].OffsetX = reader.ReadShort();
							Fixtures[i].OffsetY = reader.ReadShort();
							Fixtures[i].OffsetZ = reader.ReadShort();

							if (version > 0)
							{
								Fixtures[i].Flags = (TileFlag)reader.ReadULong();
							}
							else
							{
								Fixtures[i].Flags = (TileFlag)reader.ReadUInt();
							}
						}

						Revision = reader.ReadInt();

						break;
					}
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(1); // version

			Components.Serialize(writer);

			writer.Write(Fixtures.Length);

			for (var i = 0; i < Fixtures.Length; ++i)
			{
				var ent = Fixtures[i];

				writer.Write(ent.ItemID);
				writer.Write(ent.OffsetX);
				writer.Write(ent.OffsetY);
				writer.Write(ent.OffsetZ);

				writer.Write((ulong)ent.Flags);
			}

			writer.Write(Revision);
		}

		public void OnRevised()
		{
			lock (this)
			{
				Revision = ++Foundation.LastRevision;

				if (m_PacketCache != null)
				{
					m_PacketCache.Release();
				}

				m_PacketCache = null;
			}
		}

		public void SendGeneralInfoTo(NetState state)
		{
			if (state != null)
			{
				state.Send(new DesignStateGeneral(Foundation, this));
			}
		}

		public void SendDetailedInfoTo(NetState state)
		{
			if (state != null)
			{
				lock (this)
				{
					if (m_PacketCache == null)
					{
						DesignStateDetailed.SendDetails(state, Foundation, this);
					}
					else
					{
						state.Send(m_PacketCache);
					}
				}
			}
		}

		public void FreezeFixtures()
		{
			OnRevised();

			for (var i = 0; i < Fixtures.Length; ++i)
			{
				var mte = Fixtures[i];

				Components.Add(mte.ItemID, mte.OffsetX, mte.OffsetY, mte.OffsetZ);
			}

			Fixtures = new MultiTileEntry[0];
		}

		public void MeltFixtures()
		{
			OnRevised();

			var list = Components.List;
			var length = 0;

			for (var i = list.Length - 1; i >= 0; --i)
			{
				var mte = list[i];

				if (IsFixture(mte.ItemID))
				{
					++length;
				}
			}

			Fixtures = new MultiTileEntry[length];

			for (var i = list.Length - 1; i >= 0; --i)
			{
				var mte = list[i];

				if (IsFixture(mte.ItemID))
				{
					Fixtures[--length] = mte;
					Components.Remove(mte.ItemID, mte.OffsetX, mte.OffsetY, mte.OffsetZ);
				}
			}
		}

		public static bool IsFixture(int itemID)
		{
			if (itemID is >= 0x675 and < 0x6F5)
			{
				return true;
			}
			else if (itemID is >= 0x314 and < 0x364)
			{
				return true;
			}
			else if (itemID is >= 0x824 and < 0x834)
			{
				return true;
			}
			else if (itemID is >= 0x839 and < 0x849)
			{
				return true;
			}
			else if (itemID is >= 0x84C and < 0x85C)
			{
				return true;
			}
			else if (itemID is >= 0x866 and < 0x876)
			{
				return true;
			}
			else if (itemID is >= 0x0E8 and < 0x0F8)
			{
				return true;
			}
			else if (itemID is >= 0x1FED and < 0x1FFD)
			{
				return true;
			}
			else if (itemID is >= 0x181D and < 0x1829)
			{
				return true;
			}
			else if (itemID is >= 0x241F and < 0x2421)
			{
				return true;
			}
			else if (itemID is >= 0x2423 and < 0x2425)
			{
				return true;
			}
			else if (itemID is >= 0x2A05 and < 0x2A1D)
			{
				return true;
			}
			else if (itemID is >= 0x319C and < 0x31B0)
			{
				return true;
			}
			// ML doors
			else if (itemID is 0x2D46 or 0x2D48 or 0x2FE2 or 0x2FE4)
			{
				return true;
			}
			else if (itemID is >= 0x2D63 and < 0x2D70)
			{
				return true;
			}
			else if (itemID is >= 0x319C and < 0x31AF)
			{
				return true;
			}
			else if (itemID is >= 0x367B and < 0x369B)
			{
				return true;
			}
			// SA doors
			else if (itemID is >= 0x409B and < 0x40A3)
			{
				return true;
			}
			else if (itemID is >= 0x410C and < 0x4114)
			{
				return true;
			}
			else if (itemID is >= 0x41C2 and < 0x41CA)
			{
				return true;
			}
			else if (itemID is >= 0x41CF and < 0x41D7)
			{
				return true;
			}
			else if (itemID is >= 0x436E and < 0x437E)
			{
				return true;
			}
			else if (itemID is >= 0x46DD and < 0x46E5)
			{
				return true;
			}
			else if (itemID is >= 0x4D22 and < 0x4D2A)
			{
				return true;
			}
			else if (itemID is >= 0x50C8 and < 0x50D8)
			{
				return true;
			}
			else if (itemID is >= 0x5142 and < 0x514A)
			{
				return true;
			}
			// TOL doors
			else if (itemID is >= 0x9AD7 and < 0x9AE7)
			{
				return true;
			}
			else if (itemID is >= 0x9B3C and < 0x9B4C)
			{
				return true;
			}

			return false;
		}
	}

	public class ConfirmCommitGump : Gump
	{
		private readonly HouseFoundation m_Foundation;

		public ConfirmCommitGump(Mobile from, HouseFoundation foundation, int bankBalance, int oldPrice, int newPrice)
			: base(50, 50)
		{
			m_Foundation = foundation;

			AddPage(0);

			AddBackground(0, 0, 320, 320, 5054);

			AddImageTiled(10, 10, 300, 20, 2624);
			AddImageTiled(10, 40, 300, 240, 2624);
			AddImageTiled(10, 290, 300, 20, 2624);

			AddAlphaRegion(10, 10, 300, 300);

			AddHtmlLocalized(10, 10, 300, 20, 1062060, 32736, false, false); // <CENTER>COMMIT DESIGN</CENTER>

			AddHtmlLocalized(10, 40, 300, 140, (newPrice - oldPrice) <= bankBalance ? 1061898 : 1061903, 1023, false, true);

			AddHtmlLocalized(10, 190, 150, 20, 1061902, 32736, false, false); // Bank Balance:
			AddLabel(170, 190, 55, bankBalance.ToString());

			AddHtmlLocalized(10, 215, 150, 20, 1061899, 1023, false, false); // Old Value:
			AddLabel(170, 215, 90, oldPrice.ToString());

			AddHtmlLocalized(10, 235, 150, 20, 1061900, 1023, false, false); // Cost To Commit:
			AddLabel(170, 235, 90, newPrice.ToString());

			if (newPrice - oldPrice < 0)
			{
				AddHtmlLocalized(10, 260, 150, 20, 1062059, 992, false, false); // Your Refund:
				AddLabel(170, 260, 70, (oldPrice - newPrice).ToString());
			}
			else
			{
				AddHtmlLocalized(10, 260, 150, 20, 1061901, 31744, false, false); // Your Cost:
				AddLabel(170, 260, 40, (newPrice - oldPrice).ToString());
			}

			AddButton(10, 290, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 290, 55, 20, 1011036, 32767, false, false); // OKAY

			AddButton(170, 290, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(195, 290, 55, 20, 1011012, 32767, false, false); // CANCEL
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1)
			{
				m_Foundation.EndConfirmCommit(sender.Mobile);
			}
		}
	}

	public class DesignContext
	{
		public HouseFoundation Foundation { get; }
		public int Level { get; set; }
		public int MaxLevels => Foundation.MaxLevels;

		public DesignContext(HouseFoundation foundation)
		{
			Foundation = foundation;
			Level = 1;
		}

		public static Dictionary<Mobile, DesignContext> Table { get; } = new Dictionary<Mobile, DesignContext>();

		public static DesignContext Find(Mobile from)
		{
			if (from == null)
			{
				return null;
			}

			DesignContext d;
			_ = Table.TryGetValue(from, out d);

			return d;
		}

		public static bool Check(Mobile m)
		{
			if (Find(m) != null)
			{
				m.SendLocalizedMessage(1062206); // You cannot do that while customizing a house.
				return false;
			}

			return true;
		}

		public static void Add(Mobile from, HouseFoundation foundation)
		{
			if (from == null)
			{
				return;
			}

			var c = new DesignContext(foundation);

			Table[from] = c;

			if (from is PlayerMobile)
			{
				((PlayerMobile)from).DesignContext = c;
			}

			foundation.Customizer = from;

			from.Hidden = true;
			from.Location = new Point3D(foundation.X, foundation.Y, foundation.Z + 7);

			var state = from.NetState;

			if (state == null)
			{
				return;
			}

			var fixtures = foundation.Fixtures;

			for (var i = 0; fixtures != null && i < fixtures.Count; ++i)
			{
				var item = fixtures[i];

				state.Send(item.RemovePacket);
			}

			if (foundation.Signpost != null)
			{
				state.Send(foundation.Signpost.RemovePacket);
			}

			if (foundation.SignHanger != null)
			{
				state.Send(foundation.SignHanger.RemovePacket);
			}

			if (foundation.Sign != null)
			{
				state.Send(foundation.Sign.RemovePacket);
			}
		}

		public static void Remove(Mobile from)
		{
			var context = Find(from);

			if (context == null)
			{
				return;
			}

			_ = Table.Remove(from);

			if (from is PlayerMobile)
			{
				((PlayerMobile)from).DesignContext = null;
			}

			if (context == null)
			{
				return;
			}

			context.Foundation.Customizer = null;

			var state = from.NetState;

			if (state == null)
			{
				return;
			}

			var fixtures = context.Foundation.Fixtures;

			for (var i = 0; fixtures != null && i < fixtures.Count; ++i)
			{
				var item = fixtures[i];

				item.SendInfoTo(state);
			}

			if (context.Foundation.Signpost != null)
			{
				context.Foundation.Signpost.SendInfoTo(state);
			}

			if (context.Foundation.SignHanger != null)
			{
				context.Foundation.SignHanger.SendInfoTo(state);
			}

			if (context.Foundation.Sign != null)
			{
				context.Foundation.Sign.SendInfoTo(state);
			}
		}
	}

	public class BeginHouseCustomization : Packet
	{
		public BeginHouseCustomization(HouseFoundation house)
			: base(0xBF)
		{
			EnsureCapacity(17);

			m_Stream.Write((short)0x20);
			m_Stream.Write(house.Serial);
			m_Stream.Write((byte)0x04);
			m_Stream.Write((ushort)0x0000);
			m_Stream.Write((ushort)0xFFFF);
			m_Stream.Write((ushort)0xFFFF);
			m_Stream.Write((byte)0xFF);
		}
	}

	public class EndHouseCustomization : Packet
	{
		public EndHouseCustomization(HouseFoundation house)
			: base(0xBF)
		{
			EnsureCapacity(17);

			m_Stream.Write((short)0x20);
			m_Stream.Write(house.Serial);
			m_Stream.Write((byte)0x05);
			m_Stream.Write((ushort)0x0000);
			m_Stream.Write((ushort)0xFFFF);
			m_Stream.Write((ushort)0xFFFF);
			m_Stream.Write((byte)0xFF);
		}
	}

	public sealed class DesignStateGeneral : Packet
	{
		public DesignStateGeneral(HouseFoundation house, DesignState state)
			: base(0xBF)
		{
			EnsureCapacity(13);

			m_Stream.Write((short)0x1D);
			m_Stream.Write(house.Serial);
			m_Stream.Write(state.Revision);
		}
	}

	public sealed class DesignStateDetailed : Packet
	{
		public const int MaxItemsPerStairBuffer = 750;

		private static readonly BufferPool m_PlaneBufferPool = new("Housing Plane Buffers", 9, 0x400);
		private static readonly BufferPool m_StairBufferPool = new("Housing Stair Buffers", 6, MaxItemsPerStairBuffer * 5);
		private static readonly BufferPool m_DeflatedBufferPool = new("Housing Deflated Buffers", 1, 0x2000);

		private readonly byte[][] m_PlaneBuffers;
		private readonly byte[][] m_StairBuffers;

		private readonly bool[] m_PlaneUsed = new bool[9];
		private readonly byte[] m_PrimBuffer = new byte[4];

		public void Write(int value)
		{
			m_PrimBuffer[0] = (byte)(value >> 24);
			m_PrimBuffer[1] = (byte)(value >> 16);
			m_PrimBuffer[2] = (byte)(value >> 8);
			m_PrimBuffer[3] = (byte)value;

			m_Stream.UnderlyingStream.Write(m_PrimBuffer, 0, 4);
		}

		public void Write(short value)
		{
			m_PrimBuffer[0] = (byte)(value >> 8);
			m_PrimBuffer[1] = (byte)value;

			m_Stream.UnderlyingStream.Write(m_PrimBuffer, 0, 2);
		}

		public void Write(byte value)
		{
			m_Stream.UnderlyingStream.WriteByte(value);
		}

		public void Write(byte[] buffer, int offset, int size)
		{
			m_Stream.UnderlyingStream.Write(buffer, offset, size);
		}

		public static void Clear(byte[] buffer, int size)
		{
			for (var i = 0; i < size; ++i)
			{
				buffer[i] = 0;
			}
		}

		public DesignStateDetailed(int serial, int revision, int xMin, int yMin, int xMax, int yMax, MultiTileEntry[] tiles)
			: base(0xD8)
		{
			EnsureCapacity(17 + (tiles.Length * 5));

			Write((byte)0x03); // Compression Type
			Write((byte)0x00); // Unknown
			Write(serial);
			Write(revision);
			Write((short)tiles.Length);
			Write((short)0); // Buffer length : reserved
			Write((byte)0); // Plane count : reserved

			var totalLength = 1; // includes plane count

			var width = xMax - xMin + 1;
			var height = yMax - yMin + 1;

			m_PlaneBuffers = new byte[9][];

			lock (m_PlaneBufferPool)
			{
				for (var i = 0; i < m_PlaneBuffers.Length; ++i)
				{
					m_PlaneBuffers[i] = m_PlaneBufferPool.AcquireBuffer();
				}
			}

			m_StairBuffers = new byte[6][];

			lock (m_StairBufferPool)
			{
				for (var i = 0; i < m_StairBuffers.Length; ++i)
				{
					m_StairBuffers[i] = m_StairBufferPool.AcquireBuffer();
				}
			}

			Clear(m_PlaneBuffers[0], width * height * 2);

			for (var i = 0; i < 4; ++i)
			{
				Clear(m_PlaneBuffers[1 + i], (width - 1) * (height - 2) * 2);
				Clear(m_PlaneBuffers[5 + i], width * (height - 1) * 2);
			}

			var totalStairsUsed = 0;

			for (var i = 0; i < tiles.Length; ++i)
			{
				var mte = tiles[i];
				var x = mte.OffsetX - xMin;
				var y = mte.OffsetY - yMin;
				int z = mte.OffsetZ;
				var floor = TileData.ItemTable[mte.ItemID & TileData.MaxItemValue].Height <= 0;
				int plane, size;

				switch (z)
				{
					case 0: plane = 0; break;
					case 7: plane = 1; break;
					case 27: plane = 2; break;
					case 47: plane = 3; break;
					case 67: plane = 4; break;
					default:
						{
							var stairBufferIndex = totalStairsUsed / MaxItemsPerStairBuffer;
							var stairBuffer = m_StairBuffers[stairBufferIndex];

							var byteIndex = totalStairsUsed % MaxItemsPerStairBuffer * 5;

							stairBuffer[byteIndex++] = (byte)(mte.ItemID >> 8);
							stairBuffer[byteIndex++] = (byte)mte.ItemID;

							stairBuffer[byteIndex++] = (byte)mte.OffsetX;
							stairBuffer[byteIndex++] = (byte)mte.OffsetY;
							stairBuffer[byteIndex++] = (byte)mte.OffsetZ;

							++totalStairsUsed;

							continue;
						}
				}

				if (plane == 0)
				{
					size = height;
				}
				else if (floor)
				{
					size = height - 2;
					x -= 1;
					y -= 1;
				}
				else
				{
					size = height - 1;
					plane += 4;
				}

				var index = ((x * size) + y) * 2;

				if (x < 0 || y < 0 || y >= size || (index + 1) >= 0x400)
				{
					var stairBufferIndex = totalStairsUsed / MaxItemsPerStairBuffer;
					var stairBuffer = m_StairBuffers[stairBufferIndex];

					var byteIndex = totalStairsUsed % MaxItemsPerStairBuffer * 5;

					stairBuffer[byteIndex++] = (byte)(mte.ItemID >> 8);
					stairBuffer[byteIndex++] = (byte)mte.ItemID;

					stairBuffer[byteIndex++] = (byte)mte.OffsetX;
					stairBuffer[byteIndex++] = (byte)mte.OffsetY;
					stairBuffer[byteIndex++] = (byte)mte.OffsetZ;

					++totalStairsUsed;
				}
				else
				{
					m_PlaneUsed[plane] = true;
					m_PlaneBuffers[plane][index] = (byte)(mte.ItemID >> 8);
					m_PlaneBuffers[plane][index + 1] = (byte)mte.ItemID;
				}
			}

			var planeCount = 0;

			byte[] m_DeflatedBuffer = null;
			lock (m_DeflatedBufferPool)
			{
				m_DeflatedBuffer = m_DeflatedBufferPool.AcquireBuffer();
			}

			for (var i = 0; i < m_PlaneBuffers.Length; ++i)
			{
				if (!m_PlaneUsed[i])
				{
					m_PlaneBufferPool.ReleaseBuffer(m_PlaneBuffers[i]);
					continue;
				}

				++planeCount;

				int size;
				if (i == 0)
				{
					size = width * height * 2;
				}
				else if (i < 5)
				{
					size = (width - 1) * (height - 2) * 2;
				}
				else
				{
					size = width * (height - 1) * 2;
				}

				var inflatedBuffer = m_PlaneBuffers[i];

				var deflatedLength = m_DeflatedBuffer.Length;
				var ce = Compression.Pack(m_DeflatedBuffer, ref deflatedLength, inflatedBuffer, size, ZLibQuality.Default);

				if (ce != ZLibError.Okay)
				{
					Console.WriteLine("ZLib error: {0} (#{1})", ce, (int)ce);
					deflatedLength = 0;
					size = 0;
				}

				Write((byte)(0x20 | i));
				Write((byte)size);
				Write((byte)deflatedLength);
				Write((byte)(((size >> 4) & 0xF0) | ((deflatedLength >> 8) & 0xF)));
				Write(m_DeflatedBuffer, 0, deflatedLength);

				totalLength += 4 + deflatedLength;
				lock (m_PlaneBufferPool)
				{
					m_PlaneBufferPool.ReleaseBuffer(inflatedBuffer);
				}
			}

			var totalStairBuffersUsed = (totalStairsUsed + (MaxItemsPerStairBuffer - 1)) / MaxItemsPerStairBuffer;

			for (var i = 0; i < totalStairBuffersUsed; ++i)
			{
				++planeCount;

				var count = totalStairsUsed - (i * MaxItemsPerStairBuffer);

				if (count > MaxItemsPerStairBuffer)
				{
					count = MaxItemsPerStairBuffer;
				}

				var size = count * 5;

				var inflatedBuffer = m_StairBuffers[i];

				var deflatedLength = m_DeflatedBuffer.Length;
				var ce = Compression.Pack(m_DeflatedBuffer, ref deflatedLength, inflatedBuffer, size, ZLibQuality.Default);

				if (ce != ZLibError.Okay)
				{
					Console.WriteLine("ZLib error: {0} (#{1})", ce, (int)ce);
					deflatedLength = 0;
					size = 0;
				}

				Write((byte)(9 + i));
				Write((byte)size);
				Write((byte)deflatedLength);
				Write((byte)(((size >> 4) & 0xF0) | ((deflatedLength >> 8) & 0xF)));
				Write(m_DeflatedBuffer, 0, deflatedLength);

				totalLength += 4 + deflatedLength;
			}

			lock (m_StairBufferPool)
			{
				for (var i = 0; i < m_StairBuffers.Length; ++i)
				{
					m_StairBufferPool.ReleaseBuffer(m_StairBuffers[i]);
				}
			}

			lock (m_DeflatedBufferPool)
			{
				m_DeflatedBufferPool.ReleaseBuffer(m_DeflatedBuffer);
			}

			_ = m_Stream.Seek(15, System.IO.SeekOrigin.Begin);

			Write((short)totalLength); // Buffer length
			Write((byte)planeCount); // Plane count
		}

		private class SendQueueEntry
		{
			public NetState m_NetState;
			public int m_Serial, m_Revision;
			public int m_xMin, m_yMin, m_xMax, m_yMax;
			public DesignState m_Root;
			public MultiTileEntry[] m_Tiles;

			public SendQueueEntry(NetState ns, HouseFoundation foundation, DesignState state)
			{
				m_NetState = ns;
				m_Serial = foundation.Serial;
				m_Revision = state.Revision;
				m_Root = state;

				var mcl = state.Components;

				m_xMin = mcl.Min.X;
				m_yMin = mcl.Min.Y;
				m_xMax = mcl.Max.X;
				m_yMax = mcl.Max.Y;

				m_Tiles = mcl.List;
			}
		}

		private static readonly Queue<SendQueueEntry> m_SendQueue;
		private static readonly object m_SendQueueSyncRoot;
		private static readonly AutoResetEvent m_Sync;
		private static readonly Thread m_Thread;

		static DesignStateDetailed()
		{
			m_SendQueue = new Queue<SendQueueEntry>();
			m_SendQueueSyncRoot = ((ICollection)m_SendQueue).SyncRoot;
			m_Sync = new AutoResetEvent(false);

			m_Thread = new Thread(new ThreadStart(CompressionThread))
			{
				Name = "Housing Compression Thread"
			};
			m_Thread.Start();
		}

		public static void CompressionThread()
		{
			while (!Core.Closing)
			{
				_ = m_Sync.WaitOne();

				int count;

				lock (m_SendQueueSyncRoot)
				{
					count = m_SendQueue.Count;
				}

				while (count > 0)
				{
					SendQueueEntry sqe = null;

					lock (m_SendQueueSyncRoot)
					{
						sqe = m_SendQueue.Dequeue();
					}

					try
					{
						Packet p = null;

						lock (sqe.m_Root)
						{
							p = sqe.m_Root.PacketCache;
						}

						if (p == null)
						{
							p = new DesignStateDetailed(sqe.m_Serial, sqe.m_Revision, sqe.m_xMin, sqe.m_yMin, sqe.m_xMax, sqe.m_yMax, sqe.m_Tiles);
							p.SetStatic();

							lock (sqe.m_Root)
							{
								if (sqe.m_Revision == sqe.m_Root.Revision)
								{
									sqe.m_Root.PacketCache = p;
								}
							}
						}

						sqe.m_NetState.Send(p);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);

						try
						{
							using var op = new StreamWriter("dsd_exceptions.txt", true);
							op.WriteLine(e);
						}
						catch
						{
						}
					}
					finally
					{
						lock (m_SendQueueSyncRoot)
						{
							count = m_SendQueue.Count;
						}
					}

					//sqe.m_NetState.Send( new DesignStateDetailed( sqe.m_Serial, sqe.m_Revision, sqe.m_xMin, sqe.m_yMin, sqe.m_xMax, sqe.m_yMax, sqe.m_Tiles ) );
				}
			}
		}

		public static void SendDetails(NetState ns, HouseFoundation house, DesignState state)
		{
			lock (m_SendQueueSyncRoot)
			{
				m_SendQueue.Enqueue(new SendQueueEntry(ns, house, state));
			}

			_ = m_Sync.Set();
		}
	}

	/// House Component
	public class ComponentVerification
	{
		public int[] ItemTable { get; }
		public int[] MultiTable { get; }

		public ComponentVerification()
		{
			ItemTable = CreateTable(TileData.MaxItemValue);
			MultiTable = CreateTable(0x4000);

			LoadItems("walls.txt", "South1", "South2", "South3", "Corner", "East1", "East2", "East3", "Post", "WindowS", "AltWindowS", "WindowE", "AltWindowE", "SecondAltWindowS", "SecondAltWindowE");
			LoadItems("teleprts.txt", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14", "F15", "F16");
			LoadItems("stairs.txt", "Block", "North", "East", "South", "West", "Squared1", "Squared2", "Rounded1", "Rounded2");
			LoadItems("roof.txt", "North", "East", "South", "West", "NSCrosspiece", "EWCrosspiece", "NDent", "EDent", "SDent", "WDent", "NTPiece", "ETPiece", "STPiece", "WTPiece", "XPiece", "Extra Piece");
			LoadItems("floors.txt", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "F13", "F14", "F15", "F16");
			LoadItems("misc.txt", "Piece1", "Piece2", "Piece3", "Piece4", "Piece5", "Piece6", "Piece7", "Piece8");
			LoadItems("doors.txt", "Piece1", "Piece2", "Piece3", "Piece4", "Piece5", "Piece6", "Piece7", "Piece8");

			LoadMultis("stairs.txt", "MultiNorth", "MultiEast", "MultiSouth", "MultiWest");
		}

		public bool IsItemValid(int itemID)
		{
			if (itemID <= 0 || itemID >= ItemTable.Length)
			{
				return false;
			}

			return CheckValidity(ItemTable[itemID]);
		}

		public bool IsMultiValid(int multiID)
		{
			if (multiID <= 0 || multiID >= MultiTable.Length)
			{
				return false;
			}

			return CheckValidity(MultiTable[multiID]);
		}

		public bool CheckValidity(int val)
		{
			if (val == -1)
			{
				return false;
			}

			return val == 0 || ((int)ExpansionInfo.CoreExpansion.CustomHousingFlag & val) != 0;
		}

		private static int[] CreateTable(int length)
		{
			var table = new int[length];

			for (var i = 0; i < table.Length; ++i)
			{
				table[i] = -1;
			}

			return table;
		}

		private void LoadItems(string fileName, params string[] itemColumns)
		{
			var path = Core.FindDataFile(fileName);

			if (path != null)
			{
				LoadSpreadsheet(ItemTable, path, itemColumns);
			}
		}

		private void LoadMultis(string fileName, params string[] multiColumns)
		{
			var path = Core.FindDataFile(fileName);

			if (path != null)
			{
				LoadSpreadsheet(MultiTable, path, multiColumns);
			}
		}

		private static void LoadSpreadsheet(int[] table, string path, params string[] tileColumns)
		{
			var ss = new Spreadsheet(path);

			var tileCIDs = new int[tileColumns.Length];

			for (var i = 0; i < tileColumns.Length; ++i)
			{
				tileCIDs[i] = ss.GetColumnID(tileColumns[i]);
			}

			var featureCID = ss.GetColumnID("FeatureMask");

			foreach (var record in ss.Records)
			{
				var fid = record.GetInt32(featureCID);

				foreach (var itemID in tileCIDs.Select(v => record.GetInt32(v)).Where(id => id > 0 && id < table.Length))
				{
					table[itemID] = fid;
				}
			}
		}
	}

	public class Spreadsheet
	{
		private readonly ColumnInfo[] m_Columns;

		public Spreadsheet(string path)
		{
			using var ip = new StreamReader(path);
			var types = ReadLine(ip);
			var names = ReadLine(ip);

			m_Columns = new ColumnInfo[types.Length];

			for (var i = 0; i < m_Columns.Length; ++i)
			{
				m_Columns[i] = new ColumnInfo(i, types[i], names[i]);
			}

			var records = new List<DataRecord>();

			string[] values;

			while ((values = ReadLine(ip)) != null)
			{
				var data = new object[m_Columns.Length];

				for (var i = 0; i < m_Columns.Length; ++i)
				{
					var ci = m_Columns[i];

					switch (ci.m_Type)
					{
						case "int":
							data[i] = Utility.ToInt32(values[ci.m_DataIndex]);
							break;
						case "string":
							data[i] = values[ci.m_DataIndex];
							break;
					}
				}

				records.Add(new DataRecord(this, data));
			}

			Records = records.ToArray();
		}

		public DataRecord[] Records { get; }

		public int GetColumnID(string name)
		{
			for (var i = 0; i < m_Columns.Length; ++i)
			{
				if (m_Columns[i].m_Name == name)
				{
					return i;
				}
			}

			return -1;
		}

		private static string[] ReadLine(StreamReader ip)
		{
			string line;

			while ((line = ip.ReadLine()) != null)
			{
				if (line.Length == 0)
				{
					continue;
				}

				return line.Split('\t');
			}

			return null;
		}

		private class ColumnInfo
		{
			public readonly int m_DataIndex;
			public readonly string m_Type;
			public readonly string m_Name;

			public ColumnInfo(int dataIndex, string type, string name)
			{
				m_DataIndex = dataIndex;

				m_Type = type;
				m_Name = name;
			}
		}
	}

	public class DataRecord
	{
		public DataRecord(Spreadsheet ss, object[] data)
		{
			Spreadsheet = ss;
			Data = data;
		}

		public Spreadsheet Spreadsheet { get; }
		public object[] Data { get; }
		public object this[string name] => this[Spreadsheet.GetColumnID(name)];

		public object this[int id]
		{
			get
			{
				if (id < 0)
				{
					return null;
				}

				return Data[id];
			}
		}

		public int GetInt32(string name)
		{
			return GetInt32(this[name]);
		}

		public int GetInt32(int id)
		{
			return GetInt32(this[id]);
		}

		public int GetInt32(object obj)
		{
			if (obj is int)
			{
				return (int)obj;
			}

			return 0;
		}

		public string GetString(string name)
		{
			return this[name] as string;
		}
	}

	/// House Preview
	public class PreviewHouse : BaseMulti
	{
		private List<Item> m_Components;
		private Timer m_Timer;

		public PreviewHouse(int multiID) : base(multiID)
		{
			m_Components = new List<Item>();

			var mcl = Components;

			for (var i = 1; i < mcl.List.Length; ++i)
			{
				var entry = mcl.List[i];

				if (entry.Flags == 0)
				{
					Item item = new Static((int)entry.ItemID);

					item.MoveToWorld(new Point3D(X + entry.OffsetX, Y + entry.OffsetY, Z + entry.OffsetZ), Map);

					m_Components.Add(item);
				}
			}

			m_Timer = new DecayTimer(this);
			m_Timer.Start();
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			base.OnLocationChange(oldLocation);

			if (m_Components == null)
			{
				return;
			}

			var xOffset = X - oldLocation.X;
			var yOffset = Y - oldLocation.Y;
			var zOffset = Z - oldLocation.Z;

			for (var i = 0; i < m_Components.Count; ++i)
			{
				var item = m_Components[i];

				item.MoveToWorld(new Point3D(item.X + xOffset, item.Y + yOffset, item.Z + zOffset), Map);
			}
		}

		public override void OnMapChange(Map oldMap)
		{
			base.OnMapChange(oldMap);

			if (m_Components == null)
			{
				return;
			}

			for (var i = 0; i < m_Components.Count; ++i)
			{
				var item = m_Components[i];

				item.Map = Map;
			}
		}

		public override void OnDelete()
		{
			base.OnDelete();

			if (m_Components == null)
			{
				return;
			}

			for (var i = 0; i < m_Components.Count; ++i)
			{
				var item = m_Components[i];

				item.Delete();
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

		public PreviewHouse(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Components);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Components = reader.ReadStrongItemList();

						break;
					}
			}

			_ = Timer.DelayCall(TimeSpan.Zero, Delete);
		}

		private class DecayTimer : Timer
		{
			private readonly Item m_Item;

			public DecayTimer(Item item) : base(TimeSpan.FromSeconds(20.0))
			{
				m_Item = item;
				Priority = TimerPriority.OneSecond;
			}

			protected override void OnTick()
			{
				m_Item.Delete();
			}
		}
	}

	/// House Placement
	public enum HousePlacementResult
	{
		Valid,
		BadRegion,
		BadLand,
		BadStatic,
		BadItem,
		NoSurface,
		BadRegionHidden,
		BadRegionTemp,
		InvalidCastleKeep,
		BadRegionRaffle
	}

	public class HousePlacement
	{
		private const int YardSize = 5;

		// Any land tile which matches one of these ID numbers is considered a road and cannot be placed over.
		private static readonly int[] m_RoadIDs = new int[]
			{
				0x0071, 0x0078,
				0x00E8, 0x00EB,
				0x07AE, 0x07B1,
				0x3FF4, 0x3FF4,
				0x3FF8, 0x3FFB,
				0x0442, 0x0479, // Sand stones
				0x0501, 0x0510, // Sand stones
				0x0009, 0x0015, // Furrows
				0x0150, 0x015C  // Furrows
			};

		public static HousePlacementResult CheckRegion(Mobile from, Point3D loc, Map map)
		{
			var region = Region.Find(loc, map);

			if (!region.AllowHousing(from, loc)) // Cannot place houses in dungeons, towns, treasure map areas etc
			{
				if (region.IsPartOf(typeof(NoHousingRegion)))
				{
					return HousePlacementResult.BadRegion;
				}

				if (region.IsPartOf(typeof(TempNoHousingRegion)))
				{
					return HousePlacementResult.BadRegionTemp;
				}

				if (region.IsPartOf(typeof(TreasureRegion)) || region.IsPartOf(typeof(HouseRegion)))
				{
					return HousePlacementResult.BadRegionHidden;
				}

				if (region.IsPartOf(typeof(HouseRaffleRegion)))
				{
					return HousePlacementResult.BadRegionRaffle;
				}

				return HousePlacementResult.BadRegion;
			}

			return HousePlacementResult.Valid;
		}

		public static HousePlacementResult Check(Mobile from, int multiID, Point3D center, out HashSet<IEntity> toMove)
		{
			// If this spot is considered valid, every item and mobile in this list will be moved under the house sign
			toMove = null;

			var map = from.Map;

			if (map == null || map == Map.Internal)
			{
				return HousePlacementResult.BadLand; // A house cannot go here
			}

			if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				return HousePlacementResult.Valid; // Staff can place anywhere
			}

			if (map == Map.Ilshenar || SpellHelper.IsFeluccaT2A(map, center))
			{
				return HousePlacementResult.BadRegion; // No houses in Ilshenar/T2A
			}

			if (map == Map.Malas && (multiID == 0x007C || multiID == 0x007E))
			{
				return HousePlacementResult.InvalidCastleKeep;
			}

			// Cannot place houses in dungeons, towns, treasure map areas etc
			var result = CheckRegion(from, center, map);

			if (result != HousePlacementResult.Valid)
			{
				return result;
			}

			// This holds data describing the internal structure of the house
			var mcl = MultiData.GetComponents(multiID);

			if (multiID is >= 0x13EC and < 0x1D00)
			{
				HouseFoundation.AddStairsTo(ref mcl); // this is a AOS house, add the stairs
			}

			// Location of the nortwest-most corner of the house
			var start = new Point3D(center.X + mcl.Min.X, center.Y + mcl.Min.Y, center.Z);

			// These are storage lists. They hold items and mobiles found in the map for further processing
			var items = new HashSet<Item>();
			var mobiles = new HashSet<Mobile>();

			// These are also storage lists. They hold location values indicating the yard and border locations.
			var yard = new HashSet<Point2D>();
			var borders = new HashSet<Point2D>();

			/* RULES:
			 * 
			 * 1) All tiles which are around the -outside- of the foundation must not have anything impassable.
			 * 2) No impassable object or land tile may come in direct contact with any part of the house.
			 * 3) Five tiles from the front and back of the house must be completely clear of all house tiles.
			 * 4) The foundation must rest flatly on a surface. Any bumps around the foundation are not allowed.
			 * 5) No foundation tile may reside over terrain which is viewed as a road.
			 */

			for (var x = 0; x < mcl.Width; ++x)
			{
				for (var y = 0; y < mcl.Height; ++y)
				{
					var tileX = start.X + x;
					var tileY = start.Y + y;

					var addTiles = mcl.Tiles[x][y];

					if (addTiles.Length == 0)
					{
						continue; // There are no tiles here, continue checking somewhere else
					}

					var testPoint = new Point3D(tileX, tileY, center.Z);

					// Cannot place houses in dungeons, towns, treasure map areas etc
					var res = CheckRegion(from, testPoint, map);

					if (res != HousePlacementResult.Valid)
					{
						return res;
					}

					var landTile = map.Tiles.GetLandTile(tileX, tileY);
					var landID = landTile.ID & TileData.MaxLandValue;

					var oldTiles = map.Tiles.GetStaticTiles(tileX, tileY, true);

					var sector = map.GetSector(tileX, tileY);

					items.Clear();

					foreach (var item in sector.Items)
					{
						if (item.Visible && item.Movable && item.X == tileX && item.Y == tileY)
						{
							_ = items.Add(item);
						}
					}

					mobiles.Clear();

					foreach (var m in sector.Mobiles)
					{
						if (m.X == tileX && m.Y == tileY)
						{
							_ = mobiles.Add(m);
						}
					}

					int landStartZ = 0, landAvgZ = 0, landTopZ = 0;

					map.GetAverageZ(tileX, tileY, ref landStartZ, ref landAvgZ, ref landTopZ);

					var hasFoundation = false;

					foreach (var addTile in addTiles)
					{
						if (addTile.ID == 0x1) // Nodraw
						{
							continue;
						}

						var addTileFlags = TileData.ItemTable[addTile.ID & TileData.MaxItemValue].Flags;

						var isFoundation = addTile.Z == 0 && (addTileFlags & TileFlag.Wall) != 0;
						var hasSurface = false;

						if (isFoundation)
						{
							hasFoundation = true;
						}

						var addTileZ = center.Z + addTile.Z;
						var addTileTop = addTileZ + addTile.Height;

						if ((addTileFlags & TileFlag.Surface) != 0)
						{
							addTileTop += 16;
						}

						if (addTileTop > landStartZ && landAvgZ > addTileZ)
						{
							return HousePlacementResult.BadLand; // Broke rule #2
						}

						if (isFoundation && ((TileData.LandTable[landTile.ID & TileData.MaxLandValue].Flags & TileFlag.Impassable) == 0) && landAvgZ == center.Z)
						{
							hasSurface = true;
						}

						foreach (var oldTile in oldTiles)
						{
							var id = TileData.ItemTable[oldTile.ID & TileData.MaxItemValue];

							if ((id.Impassable || (id.Surface && (id.Flags & TileFlag.Background) == 0)) && addTileTop > oldTile.Z && (oldTile.Z + id.CalcHeight) > addTileZ)
							{
								return HousePlacementResult.BadStatic; // Broke rule #2
							}
						}

						foreach (var item in items)
						{
							var id = item.ItemData;

							if (addTileTop > item.Z && (item.Z + id.CalcHeight) > addTileZ)
							{
								if (item.Movable)
								{
									toMove ??= new();

									_ = toMove.Add(item);
								}
								else if (id.Impassable || (id.Surface && (id.Flags & TileFlag.Background) == 0))
								{
									return HousePlacementResult.BadItem; // Broke rule #2
								}
							}
						}

						if (isFoundation && !hasSurface)
						{
							return HousePlacementResult.NoSurface; // Broke rule #4
						}

						foreach (var m in mobiles)
						{
							if (addTileTop > m.Z && (m.Z + 16) > addTileZ)
							{
								toMove ??= new();

								_ = toMove.Add(m);
							}
						}
					}

					for (var i = 0; i < m_RoadIDs.Length; i += 2)
					{
						if (landID >= m_RoadIDs[i] && landID <= m_RoadIDs[i + 1])
						{
							return HousePlacementResult.BadLand; // Broke rule #5
						}
					}

					if (hasFoundation)
					{
						for (var xOffset = -1; xOffset <= 1; ++xOffset)
						{
							for (var yOffset = -YardSize; yOffset <= YardSize; ++yOffset)
							{
								_ = yard.Add(new Point2D(tileX + xOffset, tileY + yOffset));
							}
						}

						for (var xOffset = -1; xOffset <= 1; ++xOffset)
						{
							for (var yOffset = -1; yOffset <= 1; ++yOffset)
							{
								if (xOffset == 0 && yOffset == 0)
								{
									continue;
								}

								// To ease this rule, we will not add to the border list if the tile here is under a base floor (z<=8)

								var vx = x + xOffset;
								var vy = y + yOffset;

								if (vx >= 0 && vx < mcl.Width && vy >= 0 && vy < mcl.Height)
								{
									var breakTiles = mcl.Tiles[vx][vy];
									var shouldBreak = false;

									for (var i = 0; !shouldBreak && i < breakTiles.Length; ++i)
									{
										var breakTile = breakTiles[i];

										if (breakTile.Height == 0 && breakTile.Z <= 8 && TileData.ItemTable[breakTile.ID & TileData.MaxItemValue].Surface)
										{
											shouldBreak = true;
										}
									}

									if (shouldBreak)
									{
										continue;
									}
								}

								_ = borders.Add(new Point2D(tileX + xOffset, tileY + yOffset));
							}
						}
					}
				}
			}

			foreach (var borderPoint in borders)
			{
				var landTile = map.Tiles.GetLandTile(borderPoint.X, borderPoint.Y);
				var landID = landTile.ID & TileData.MaxLandValue;

				if ((TileData.LandTable[landID].Flags & TileFlag.Impassable) != 0)
				{
					return HousePlacementResult.BadLand;
				}

				for (var j = 0; j < m_RoadIDs.Length; j += 2)
				{
					if (landID >= m_RoadIDs[j] && landID <= m_RoadIDs[j + 1])
					{
						return HousePlacementResult.BadLand; // Broke rule #5
					}
				}

				var tiles = map.Tiles.GetStaticTiles(borderPoint.X, borderPoint.Y, true);

				foreach (var tile in tiles)
				{
					var id = TileData.ItemTable[tile.ID & TileData.MaxItemValue];

					if (id.Impassable || (id.Surface && (id.Flags & TileFlag.Background) == 0 && (tile.Z + id.CalcHeight) > (center.Z + 2)))
					{
						return HousePlacementResult.BadStatic; // Broke rule #1
					}
				}

				var sector = map.GetSector(borderPoint.X, borderPoint.Y);

				foreach (var item in sector.Items)
				{
					if (item.X != borderPoint.X || item.Y != borderPoint.Y || item.Movable)
					{
						continue;
					}

					var id = item.ItemData;

					if (id.Impassable || (id.Surface && (id.Flags & TileFlag.Background) == 0 && (item.Z + id.CalcHeight) > (center.Z + 2)))
					{
						return HousePlacementResult.BadItem; // Broke rule #1
					}
				}
			}

			var sectors = new HashSet<Sector>();
			var multis = new HashSet<BaseMulti>();

			foreach (var yardPoint in yard)
			{
				var sector = map.GetSector(yardPoint);

				if (sectors.Add(sector) && sector.Multis != null)
				{
					multis.UnionWith(sector.Multis);
				}
			}

			foreach (var yardPoint in yard)
			{
				foreach (var multi in multis)
				{
					if (multi.Contains(yardPoint))
					{
						return HousePlacementResult.BadStatic; // Broke rule #3
					}
				}
			}

			return HousePlacementResult.Valid;
		}
	}

	public partial class HousePlacementEntry
	{
		private readonly int m_Storage;
		private readonly int m_Lockdowns;
		private readonly int m_NewStorage;
		private readonly int m_NewLockdowns;
		private Point3D m_Offset;

		public Type Type { get; }

		public int Description { get; }
		public int Storage => BaseHouse.NewVendorSystem ? m_NewStorage : m_Storage;
		public int Lockdowns => BaseHouse.NewVendorSystem ? m_NewLockdowns : m_Lockdowns;
		public int Vendors { get; }
		public int Cost { get; }

		public int MultiID { get; }
		public Point3D Offset => m_Offset;

		public HousePlacementEntry(Type type, int description, int storage, int lockdowns, int newStorage, int newLockdowns, int vendors, int cost, int xOffset, int yOffset, int zOffset, int multiID)
		{
			Type = type;
			Description = description;
			m_Storage = storage;
			m_Lockdowns = lockdowns;
			m_NewStorage = newStorage;
			m_NewLockdowns = newLockdowns;
			Vendors = vendors;
			Cost = cost;

			m_Offset = new Point3D(xOffset, yOffset, zOffset);

			MultiID = multiID;
		}

		public BaseHouse ConstructHouse(Mobile from)
		{
			try
			{
				object[] args;

				if (Type == typeof(HouseFoundation))
				{
					args = new object[4] { from, MultiID, m_Storage, m_Lockdowns };
				}
				else if (Type == typeof(SmallOldHouse) || Type == typeof(SmallShop) || Type == typeof(TwoStoryHouse))
				{
					args = new object[2] { from, MultiID };
				}
				else
				{
					args = new object[1] { from };
				}

				return Activator.CreateInstance(Type, args) as BaseHouse;
			}
			catch
			{
			}

			return null;
		}

		public void PlacementWarning_Callback(Mobile from, bool okay, object state)
		{
			if (!from.CheckAlive() || from.Backpack == null || from.Backpack.FindItemByType(typeof(HousePlacementTool)) == null)
			{
				return;
			}

			var prevHouse = (PreviewHouse)state;

			if (!okay)
			{
				prevHouse.Delete();
				return;
			}

			if (prevHouse.Deleted)
			{
				/* Too much time has passed and the test house you created has been deleted.
				 * Please try again!
				 */
				_ = from.SendGump(new NoticeGump(1060637, 30720, 1060647, 32512, 320, 180, null, null));

				return;
			}

			var center = prevHouse.Location;

			prevHouse.Delete();

			var res = HousePlacement.Check(from, MultiID, center, out var toMove);

			switch (res)
			{
				case HousePlacementResult.Valid:
					{
						if (from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse(from))
						{
							from.SendLocalizedMessage(501271); // You already own a house, you may not place another!
							break;
						}

						var house = ConstructHouse(from);

						if (house == null)
						{
							break;
						}

						house.Price = Cost;

						if (from.AccessLevel >= AccessLevel.GameMaster)
						{
							from.SendMessage("{0} gold would have been withdrawn from your bank if you were not a GM.", Cost.ToString());
						}
						else if (Banker.Withdraw(from, Cost))
						{
							from.SendLocalizedMessage(1060398, Cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
						}
						else
						{
							house.RemoveKeys(from);
							house.Delete();

							from.SendLocalizedMessage(1060646); // You do not have the funds available in your bank box to purchase this house.  Try placing a smaller house, or adding gold or checks to your bank box.

							break;
						}

						house.MoveToWorld(center, from.Map);

						if (toMove?.Count > 0)
						{
							foreach (var o in toMove)
							{
								o.Location = house.BanLocation;
							}
						}

						break;
					}
				case HousePlacementResult.BadItem:
				case HousePlacementResult.BadLand:
				case HousePlacementResult.BadStatic:
				case HousePlacementResult.BadRegionHidden:
				case HousePlacementResult.NoSurface:
					{
						from.SendLocalizedMessage(1043287); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
						break;
					}
				case HousePlacementResult.BadRegion:
					{
						from.SendLocalizedMessage(501265); // Housing cannot be created in this area.
						break;
					}
				case HousePlacementResult.BadRegionTemp:
					{
						from.SendLocalizedMessage(501270); // Lord British has decreed a 'no build' period, thus you cannot build this house at this time.
						break;
					}
				case HousePlacementResult.BadRegionRaffle:
					{
						from.SendLocalizedMessage(1150493); // You must have a deed for this plot of land in order to build here.
						break;
					}
				case HousePlacementResult.InvalidCastleKeep:
					{
						from.SendLocalizedMessage(1061122); // Castles and keeps cannot be created here.
						break;
					}
			}
		}

		public bool OnPlacement(Mobile from, Point3D p)
		{
			if (!from.CheckAlive() || from.Backpack == null || from.Backpack.FindItemByType(typeof(HousePlacementTool)) == null)
			{
				return false;
			}

			var center = new Point3D(p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z);

			var res = HousePlacement.Check(from, MultiID, center, out var toMove);

			switch (res)
			{
				case HousePlacementResult.Valid:
					{
						if (from.AccessLevel < AccessLevel.GameMaster && BaseHouse.HasAccountHouse(from))
						{
							from.SendLocalizedMessage(501271); // You already own a house, you may not place another!
							return false;
						}

						from.SendLocalizedMessage(1011576); // This is a valid location.

						var prev = new PreviewHouse(MultiID);

						var mcl = prev.Components;

						var banLoc = new Point3D(center.X + mcl.Min.X, center.Y + mcl.Max.Y + 1, center.Z);

						for (var i = 0; i < mcl.List.Length; ++i)
						{
							var entry = mcl.List[i];

							int itemID = entry.ItemID;

							if (itemID is >= 0xBA3 and <= 0xC0E)
							{
								banLoc = new Point3D(center.X + entry.OffsetX, center.Y + entry.OffsetY, center.Z);
								break;
							}
						}

						if (toMove?.Count > 0)
						{
							foreach (var o in toMove)
							{
								o.Location = banLoc;
							}
						}

						prev.MoveToWorld(center, from.Map);

						/* You are about to place a new house.
						 * Placing this house will condemn any and all of your other houses that you may have.
						 * All of your houses on all shards will be affected.
						 * 
						 * In addition, you will not be able to place another house or have one transferred to you for one (1) real-life week.
						 * 
						 * Once you accept these terms, these effects cannot be reversed.
						 * Re-deeding or transferring your new house will not uncondemn your other house(s) nor will the one week timer be removed.
						 * 
						 * If you are absolutely certain you wish to proceed, click the button next to OKAY below.
						 * If you do not wish to trade for this house, click CANCEL.
						 */
						_ = from.SendGump(new WarningGump(1060635, 30720, 1049583, 32512, 420, 280, new WarningGumpCallback(PlacementWarning_Callback), prev));

						return true;
					}
				case HousePlacementResult.BadItem:
				case HousePlacementResult.BadLand:
				case HousePlacementResult.BadStatic:
				case HousePlacementResult.BadRegionHidden:
				case HousePlacementResult.NoSurface:
					{
						from.SendLocalizedMessage(1043287); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
						return false;
					}
				case HousePlacementResult.BadRegion:
					{
						from.SendLocalizedMessage(501265); // Housing cannot be created in this area.
						return false;
					}
				case HousePlacementResult.BadRegionTemp:
					{
						from.SendLocalizedMessage(501270); //Lord British has decreed a 'no build' period, thus you cannot build this house at this time.
						return false;
					}
				case HousePlacementResult.BadRegionRaffle:
					{
						from.SendLocalizedMessage(1150493); // You must have a deed for this plot of land in order to build here.
						return false;
					}
				case HousePlacementResult.InvalidCastleKeep:
					{
						from.SendLocalizedMessage(1061122); // Castles and keeps cannot be created here.
						return false;
					}
			}

			return false;
		}

		private static readonly Hashtable m_Table;

		static HousePlacementEntry()
		{
			m_Table = new Hashtable();

			FillTable(m_ClassicHouses);
			FillTable(m_TwoStoryFoundations);
			FillTable(m_ThreeStoryFoundations);
		}

		public static HousePlacementEntry Find(BaseHouse house)
		{
			var obj = m_Table[house.GetType()];

			if (obj is HousePlacementEntry)
			{
				return (HousePlacementEntry)obj;
			}
			else if (obj is ArrayList list)
			{
				for (var i = 0; i < list.Count; ++i)
				{
					var e = (HousePlacementEntry)list[i];

					if (e.MultiID == house.ItemID)
					{
						return e;
					}
				}
			}
			else if (obj is Hashtable table)
			{
				obj = table[house.ItemID];

				if (obj is HousePlacementEntry)
				{
					return (HousePlacementEntry)obj;
				}
			}

			return null;
		}

		private static void FillTable(HousePlacementEntry[] entries)
		{
			for (var i = 0; i < entries.Length; ++i)
			{
				var e = entries[i];

				var obj = m_Table[e.Type];

				if (obj == null)
				{
					m_Table[e.Type] = e;
				}
				else if (obj is HousePlacementEntry)
				{
					var list = new ArrayList {
						obj,
						e
					};

					m_Table[e.Type] = list;
				}
				else if (obj is ArrayList list)
				{
					if (list.Count == 8)
					{
						var table = new Hashtable();

						for (var j = 0; j < list.Count; ++j)
						{
							table[((HousePlacementEntry)list[j]).MultiID] = list[j];
						}

						table[e.MultiID] = e;

						m_Table[e.Type] = table;
					}
					else
					{
						_ = list.Add(e);
					}
				}
				else if (obj is Hashtable)
				{
					((Hashtable)obj)[e.MultiID] = e;
				}
			}
		}
	}

	#region House Owner Targets

	public class HousePlacementTarget : MultiTarget
	{
		private readonly HouseDeed m_Deed;

		public HousePlacementTarget(HouseDeed deed) : base(deed.MultiID, deed.Offset)
		{
			m_Deed = deed;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (!from.CheckAlive() || from.Backpack == null || !m_Deed.IsChildOf(from.Backpack))
			{
				return;
			}

			if (o is IPoint3D ip)
			{
				if (ip is Item item)
				{
					ip = item.GetWorldTop();
				}

				var p = new Point3D(ip);

				var reg = Region.Find(p, from.Map);

				if (reg.IsPartOf(typeof(TempNoHousingRegion)))
				{
					from.SendLocalizedMessage(501270); // Lord British has decreed a 'no build' period, thus you cannot build this house at this time.
				}
				else if (reg.IsPartOf(typeof(TreasureRegion)) || reg.IsPartOf(typeof(HouseRegion)))
				{
					from.SendLocalizedMessage(1043287); // The house could not be created here.  Either something is blocking the house, or the house would not be on valid terrain.
				}
				else if (reg.IsPartOf(typeof(HouseRaffleRegion)))
				{
					from.SendLocalizedMessage(1150493); // You must have a deed for this plot of land in order to build here.
				}
				else if (!reg.AllowHousing(from, p))
				{
					from.SendLocalizedMessage(501265); // Housing can not be created in this area.
				}
				else
				{
					_ = m_Deed.OnPlacement(from, p);
				}
			}
		}
	}

	#endregion

	public class RelocatedEntity
	{
		private Point3D m_RelativeLocation;

		public IEntity Entity { get; }

		public Point3D RelativeLocation => m_RelativeLocation;

		public RelocatedEntity(IEntity entity, Point3D relativeLocation)
		{
			Entity = entity;
			m_RelativeLocation = relativeLocation;
		}
	}

	/// House Access
	public class HouseAccessTarget : Target
	{
		private readonly BaseHouse m_House;

		public HouseAccessTarget(BaseHouse house) : base(-1, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsFriend(from))
			{
				return;
			}

			if (targeted is Mobile)
			{
				m_House.GrantAccess(from, (Mobile)targeted);
			}
			else
			{
				from.SendLocalizedMessage(1060712); // That is not a player.
			}
		}
	}

	public class CoOwnerTarget : Target
	{
		private readonly BaseHouse m_House;
		private readonly bool m_Add;

		public CoOwnerTarget(bool add, BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
			m_Add = add;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsOwner(from))
			{
				return;
			}

			if (targeted is Mobile)
			{
				if (m_Add)
				{
					m_House.AddCoOwner(from, (Mobile)targeted);
				}
				else
				{
					m_House.RemoveCoOwner(from, (Mobile)targeted);
				}
			}
			else
			{
				from.SendLocalizedMessage(501362);//That can't be a coowner
			}
		}
	}

	public class HouseFriendTarget : Target
	{
		private readonly BaseHouse m_House;
		private readonly bool m_Add;

		public HouseFriendTarget(bool add, BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
			m_Add = add;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsCoOwner(from))
			{
				return;
			}

			if (targeted is Mobile)
			{
				if (m_Add)
				{
					m_House.AddFriend(from, (Mobile)targeted);
				}
				else
				{
					m_House.RemoveFriend(from, (Mobile)targeted);
				}
			}
			else
			{
				from.SendLocalizedMessage(501371); // That can't be a friend
			}
		}
	}

	public class HouseOwnerTarget : Target
	{
		private readonly BaseHouse m_House;

		public HouseOwnerTarget(BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_House = house;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is Mobile)
			{
				m_House.BeginConfirmTransfer(from, (Mobile)targeted);
			}
			else
			{
				from.SendLocalizedMessage(501384); // Only a player can own a house!
			}
		}
	}

	/// House Secures
	public enum SecureLevel
	{
		Owner,
		CoOwners,
		Friends,
		Anyone,
		Guild
	}

	public enum SecureAccessResult
	{
		Insecure,
		Accessible,
		Inaccessible
	}

	public class SecureInfo : ISecurable
	{
		public Container Item { get; }
		public SecureLevel Level { get; set; }

		public SecureInfo(Container item, SecureLevel level)
		{
			Item = item;
			Level = level;
		}

		public SecureInfo(GenericReader reader)
		{
			Item = reader.ReadItem() as Container;
			Level = (SecureLevel)reader.ReadByte();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(Item);
			writer.Write((byte)Level);
		}
	}

	public class SetSecureLevelEntry : ContextMenuEntry
	{
		private readonly Item m_Item;

		public SetSecureLevelEntry(Item item) : base(6203, 6)
		{
			m_Item = item;
		}

		public override void OnClick()
		{
			var house = BaseHouse.FindHouseAt(m_Item);

			var sec = house?.GetSecurable(Owner.From, m_Item);

			if (sec != null)
			{
				_ = Owner.From.CloseGump(typeof(SetSecureLevelGump));
				_ = Owner.From.SendGump(new SetSecureLevelGump(Owner.From, sec, house));
			}
		}

		public static void AddTo(Mobile from, Item item, List<ContextMenuEntry> list)
		{
			var house = BaseHouse.FindHouseAt(item);

			var sec = house?.GetSecurable(from, item);

			if (sec != null)
			{
				list.Add(new SetSecureLevelEntry(item));
			}
		}
	}

	#region House Owner Targets

	public class LockdownTarget : Target
	{
		private readonly bool m_Release;
		private readonly BaseHouse m_House;

		public LockdownTarget(bool release, BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_Release = release;
			m_House = house;
		}

		protected override void OnTargetNotAccessible(Mobile from, object targeted)
		{
			OnTarget(from, targeted);
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsCoOwner(from))
			{
				return;
			}

			if (targeted is Item)
			{
				if (m_Release)
				{
					#region Mondain's legacy
					if (targeted is AddonContainerComponent component)
					{
						if (component.Addon != null)
						{
							m_House.Release(from, component.Addon);
						}
					}
					else
						#endregion

						m_House.Release(from, (Item)targeted);
				}
				else
				{
					if (targeted is VendorRentalContract)
					{
						from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1062392); // You must double click the contract in your pack to lock it down.
						from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501732); // I cannot lock this down!
					}
					else if ((Item)targeted is AddonComponent)
					{
						from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 501727); // You cannot lock that down!
						from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 501732); // I cannot lock this down!
					}
					else
					{
						#region Mondain's legacy
						if (targeted is AddonContainerComponent component)
						{
							if (component.Addon != null)
							{
								_ = m_House.LockDown(from, component.Addon);
							}
						}
						else
							#endregion

							_ = m_House.LockDown(from, (Item)targeted);
					}
				}
			}
			else if (targeted is StaticTarget)
			{
				return;
			}
			else
			{
				from.SendLocalizedMessage(1005377); //You cannot lock that down
			}
		}
	}

	public class SecureTarget : Target
	{
		private readonly bool m_Release;
		private readonly BaseHouse m_House;

		public SecureTarget(bool release, BaseHouse house) : base(12, false, TargetFlags.None)
		{
			CheckLOS = false;

			m_Release = release;
			m_House = house;
		}

		protected override void OnTargetNotAccessible(Mobile from, object targeted)
		{
			OnTarget(from, targeted);
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!from.Alive || m_House.Deleted || !m_House.IsCoOwner(from))
			{
				return;
			}

			if (targeted is Item)
			{
				if (m_Release)
				{
					#region Mondain's legacy
					if (targeted is AddonContainerComponent component)
					{
						if (component.Addon != null)
						{
							m_House.ReleaseSecure(from, component.Addon);
						}
					}
					else
						#endregion

						m_House.ReleaseSecure(from, (Item)targeted);
				}
				else
				{
					if (targeted is VendorRentalContract)
					{
						from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1062392); // You must double click the contract in your pack to lock it down.
						from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501732); // I cannot lock this down!
					}
					else
					{
						#region Mondain's legacy
						if (targeted is AddonContainerComponent component)
						{
							if (component.Addon != null)
							{
								m_House.AddSecure(from, component.Addon);
							}
						}
						else
							#endregion

							m_House.AddSecure(from, (Item)targeted);
					}
				}
			}
			else
			{
				from.SendLocalizedMessage(1010424);//You cannot secure this
			}
		}
	}

	#endregion

	/// House Decay
	public enum DecayType
	{
		Ageless,
		AutoRefresh,
		ManualRefresh,
		Condemned
	}

	public enum DecayLevel
	{
		Ageless,
		LikeNew,
		Slightly,
		Somewhat,
		Fairly,
		Greatly,
		IDOC,
		Collapsed,
		DemolitionPending
	}

	public class DynamicDecay
	{
		public static bool Enabled => Core.ML;

		private static readonly Dictionary<DecayLevel, DecayStageInfo> m_Stages;

		static DynamicDecay()
		{
			m_Stages = new Dictionary<DecayLevel, DecayStageInfo>();

			Register(DecayLevel.LikeNew, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
			Register(DecayLevel.Slightly, TimeSpan.FromDays(1), TimeSpan.FromDays(2));
			Register(DecayLevel.Somewhat, TimeSpan.FromDays(1), TimeSpan.FromDays(2));
			Register(DecayLevel.Fairly, TimeSpan.FromDays(1), TimeSpan.FromDays(2));
			Register(DecayLevel.Greatly, TimeSpan.FromDays(1), TimeSpan.FromDays(2));
			Register(DecayLevel.IDOC, TimeSpan.FromHours(12), TimeSpan.FromHours(24));
		}

		public static void Register(DecayLevel level, TimeSpan min, TimeSpan max)
		{
			var info = new DecayStageInfo(min, max);

			if (m_Stages.ContainsKey(level))
			{
				m_Stages[level] = info;
			}
			else
			{
				m_Stages.Add(level, info);
			}
		}

		public static bool Decays(DecayLevel level)
		{
			return m_Stages.ContainsKey(level);
		}

		public static TimeSpan GetRandomDuration(DecayLevel level)
		{
			if (!m_Stages.ContainsKey(level))
			{
				return TimeSpan.Zero;
			}

			var info = m_Stages[level];
			var min = info.MinDuration.Ticks;
			var max = info.MaxDuration.Ticks;

			return TimeSpan.FromTicks(min + (long)(Utility.RandomDouble() * (max - min)));
		}
	}

	public class DecayStageInfo
	{
		public TimeSpan MinDuration { get; }

		public TimeSpan MaxDuration { get; }

		public DecayStageInfo(TimeSpan min, TimeSpan max)
		{
			MinDuration = min;
			MaxDuration = max;
		}
	}

	/// House Gumps
	public class HouseGump : Gump
	{
		private readonly BaseHouse m_House;

		private ArrayList Wrap(string value)
		{
			if (value == null || (value = value.Trim()).Length <= 0)
			{
				return null;
			}

			var values = value.Split(' ');
			var list = new ArrayList();
			var current = "";

			for (var i = 0; i < values.Length; ++i)
			{
				var val = values[i];

				var v = current.Length == 0 ? val : current + ' ' + val;

				if (v.Length < 10)
				{
					current = v;
				}
				else if (v.Length == 10)
				{
					_ = list.Add(v);

					if (list.Count == 6)
					{
						return list;
					}

					current = "";
				}
				else if (val.Length <= 10)
				{
					_ = list.Add(current);

					if (list.Count == 6)
					{
						return list;
					}

					current = val;
				}
				else
				{
					while (v.Length >= 10)
					{
						_ = list.Add(v.Substring(0, 10));

						if (list.Count == 6)
						{
							return list;
						}

						v = v.Substring(10);
					}

					current = v;
				}
			}

			if (current.Length > 0)
			{
				_ = list.Add(current);
			}

			return list;
		}

		public HouseGump(Mobile from, BaseHouse house) : base(20, 30)
		{
			if (house.Deleted)
			{
				return;
			}

			m_House = house;

			_ = from.CloseGump(typeof(HouseGump));
			_ = from.CloseGump(typeof(HouseListGump));
			_ = from.CloseGump(typeof(HouseRemoveGump));

			var isCombatRestricted = house.IsCombatRestricted(from);

			var isOwner = m_House.IsOwner(from);
			var isCoOwner = isOwner || m_House.IsCoOwner(from);
			var isFriend = isCoOwner || m_House.IsFriend(from);

			if (isCombatRestricted)
			{
				isFriend = false;
			}

			AddPage(0);

			if (isFriend)
			{
				AddBackground(0, 0, 420, 430, 5054);
				AddBackground(10, 10, 400, 410, 3000);
			}

			AddImage(130, 0, 100);

			if (m_House.Sign != null)
			{
				var lines = Wrap(m_House.Sign.GetName());

				if (lines != null)
				{
					for (int i = 0, y = (101 - (lines.Count * 14)) / 2; i < lines.Count; ++i, y += 14)
					{
						var s = (string)lines[i];

						AddLabel(130 + ((143 - (s.Length * 8)) / 2), y, 0, s);
					}
				}
			}

			if (!isFriend)
			{
				return;
			}

			AddHtmlLocalized(55, 103, 75, 20, 1011233, false, false); // INFO
			AddButton(20, 103, 4005, 4007, 0, GumpButtonType.Page, 1);

			AddHtmlLocalized(170, 103, 75, 20, 1011234, false, false); // FRIENDS
			AddButton(135, 103, 4005, 4007, 0, GumpButtonType.Page, 2);

			AddHtmlLocalized(295, 103, 75, 20, 1011235, false, false); // OPTIONS
			AddButton(260, 103, 4005, 4007, 0, GumpButtonType.Page, 3);

			AddHtmlLocalized(295, 390, 75, 20, 1011441, false, false);  // EXIT
			AddButton(260, 390, 4005, 4007, 0, GumpButtonType.Reply, 0);

			AddHtmlLocalized(55, 390, 200, 20, 1011236, false, false); // Change this house's name!
			AddButton(20, 390, 4005, 4007, 1, GumpButtonType.Reply, 0);

			// Info page
			AddPage(1);

			AddHtmlLocalized(20, 135, 100, 20, 1011242, false, false); // Owned by:
			AddHtml(120, 135, 100, 20, GetOwnerName(), false, false);

			AddHtmlLocalized(20, 170, 275, 20, 1011237, false, false); // Number of locked down items:
			AddHtml(320, 170, 50, 20, m_House.LockDownCount.ToString(), false, false);

			AddHtmlLocalized(20, 190, 275, 20, 1011238, false, false); // Maximum locked down items:
			AddHtml(320, 190, 50, 20, m_House.MaxLockDowns.ToString(), false, false);

			AddHtmlLocalized(20, 210, 275, 20, 1011239, false, false); // Number of secure containers:
			AddHtml(320, 210, 50, 20, m_House.SecureCount.ToString(), false, false);

			AddHtmlLocalized(20, 230, 275, 20, 1011240, false, false); // Maximum number of secure containers:
			AddHtml(320, 230, 50, 20, m_House.MaxSecures.ToString(), false, false);

			AddHtmlLocalized(20, 260, 400, 20, 1018032, false, false); // This house is properly placed.
			AddHtmlLocalized(20, 280, 400, 20, 1018035, false, false); // This house is of modern design.

			if (m_House.Public)
			{
				// TODO: Validate exact placement
				AddHtmlLocalized(20, 305, 275, 20, 1011241, false, false); // Number of visits this building has had
				AddHtml(320, 305, 50, 20, m_House.Visits.ToString(), false, false);
			}

			// Friends page
			AddPage(2);

			AddHtmlLocalized(45, 130, 150, 20, 1011266, false, false); // List of co-owners
			AddButton(20, 130, 2714, 2715, 2, GumpButtonType.Reply, 0);

			AddHtmlLocalized(45, 150, 150, 20, 1011267, false, false); // Add a co-owner
			AddButton(20, 150, 2714, 2715, 3, GumpButtonType.Reply, 0);

			AddHtmlLocalized(45, 170, 150, 20, 1018036, false, false); // Remove a co-owner
			AddButton(20, 170, 2714, 2715, 4, GumpButtonType.Reply, 0);

			AddHtmlLocalized(45, 190, 150, 20, 1011268, false, false); // Clear co-owner list
			AddButton(20, 190, 2714, 2715, 5, GumpButtonType.Reply, 0);

			AddHtmlLocalized(225, 130, 155, 20, 1011243, false, false); // List of Friends
			AddButton(200, 130, 2714, 2715, 6, GumpButtonType.Reply, 0);

			AddHtmlLocalized(225, 150, 155, 20, 1011244, false, false); // Add a Friend
			AddButton(200, 150, 2714, 2715, 7, GumpButtonType.Reply, 0);

			AddHtmlLocalized(225, 170, 155, 20, 1018037, false, false); // Remove a Friend
			AddButton(200, 170, 2714, 2715, 8, GumpButtonType.Reply, 0);

			AddHtmlLocalized(225, 190, 155, 20, 1011245, false, false); // Clear Friends list
			AddButton(200, 190, 2714, 2715, 9, GumpButtonType.Reply, 0);

			AddHtmlLocalized(120, 215, 280, 20, 1011258, false, false); // Ban someone from the house
			AddButton(95, 215, 2714, 2715, 10, GumpButtonType.Reply, 0);

			AddHtmlLocalized(120, 235, 280, 20, 1011259, false, false); // Eject someone from the house
			AddButton(95, 235, 2714, 2715, 11, GumpButtonType.Reply, 0);

			AddHtmlLocalized(120, 255, 280, 20, 1011260, false, false); // View a list of banned people
			AddButton(95, 255, 2714, 2715, 12, GumpButtonType.Reply, 0);

			AddHtmlLocalized(120, 275, 280, 20, 1011261, false, false); // Lift a ban
			AddButton(95, 275, 2714, 2715, 13, GumpButtonType.Reply, 0);

			// Options page
			AddPage(3);

			AddHtmlLocalized(45, 150, 355, 30, 1011248, false, false); // Transfer ownership of the house
			AddButton(20, 150, 2714, 2715, 14, GumpButtonType.Reply, 0);

			AddHtmlLocalized(45, 180, 355, 30, 1011249, false, false); // Demolish house and get deed back
			AddButton(20, 180, 2714, 2715, 15, GumpButtonType.Reply, 0);

			if (!m_House.Public)
			{
				AddHtmlLocalized(45, 210, 355, 30, 1011247, false, false); // Change the house locks
				AddButton(20, 210, 2714, 2715, 16, GumpButtonType.Reply, 0);

				AddHtmlLocalized(45, 240, 350, 90, 1011253, false, false); // Declare this building to be public. This will make your front door unlockable.
				AddButton(20, 240, 2714, 2715, 17, GumpButtonType.Reply, 0);
			}
			else
			{
				//AddHtmlLocalized( 45, 280, 350, 30, 1011250, false, false ); // Change the sign type
				AddHtmlLocalized(45, 210, 350, 30, 1011250, false, false); // Change the sign type
				AddButton(20, 210, 2714, 2715, 0, GumpButtonType.Page, 4);

				AddHtmlLocalized(45, 240, 350, 30, 1011252, false, false); // Declare this building to be private.
				AddButton(20, 240, 2714, 2715, 17, GumpButtonType.Reply, 0);

				// Change the sign type
				AddPage(4);

				for (var i = 0; i < 24; ++i)
				{
					AddRadio(53 + (i / 4 * 50), 137 + (i % 4 * 35), 210, 211, false, i + 1);
					AddItem(60 + (i / 4 * 50), 130 + (i % 4 * 35), 2980 + (i * 2));
				}

				AddHtmlLocalized(200, 305, 129, 20, 1011254, false, false); // Guild sign choices
				AddButton(350, 305, 252, 253, 0, GumpButtonType.Page, 5);

				AddHtmlLocalized(200, 340, 355, 30, 1011277, false, false); // Okay that is fine.
				AddButton(350, 340, 4005, 4007, 18, GumpButtonType.Reply, 0);

				AddPage(5);

				for (var i = 0; i < 29; ++i)
				{
					AddRadio(53 + (i / 5 * 50), 137 + (i % 5 * 35), 210, 211, false, i + 25);
					AddItem(60 + (i / 5 * 50), 130 + (i % 5 * 35), 3028 + (i * 2));
				}

				AddHtmlLocalized(200, 305, 129, 20, 1011255, false, false); // Shop sign choices
				AddButton(350, 305, 250, 251, 0, GumpButtonType.Page, 4);

				AddHtmlLocalized(200, 340, 355, 30, 1011277, false, false); // Okay that is fine.
				AddButton(350, 340, 4005, 4007, 18, GumpButtonType.Reply, 0);
			}
		}

		private string GetOwnerName()
		{
			var m = m_House.Owner;

			if (m == null)
			{
				return "(unowned)";
			}

			string name;

			if ((name = m.Name) == null || (name = name.Trim()).Length <= 0)
			{
				name = "(no name)";
			}

			return name;
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_House.Deleted)
			{
				return;
			}

			var from = sender.Mobile;

			var isCombatRestricted = m_House.IsCombatRestricted(from);

			var isOwner = m_House.IsOwner(from);
			var isCoOwner = isOwner || m_House.IsCoOwner(from);
			var isFriend = isCoOwner || m_House.IsFriend(from);

			if (isCombatRestricted)
			{
				isFriend = isCoOwner = isOwner = false;
			}

			if (!isFriend || !from.Alive)
			{
				return;
			}

			Item sign = m_House.Sign;

			if (sign == null || from.Map != sign.Map || !from.InRange(sign.GetWorldLocation(), 18))
			{
				return;
			}

			switch (info.ButtonID)
			{
				case 1: // Rename sign
					{
						from.Prompt = new RenamePrompt(m_House);
						from.SendLocalizedMessage(501302); // What dost thou wish the sign to say?

						break;
					}
				case 2: // List of co-owners
					{
						_ = from.CloseGump(typeof(HouseGump));
						_ = from.CloseGump(typeof(HouseListGump));
						_ = from.CloseGump(typeof(HouseRemoveGump));
						_ = from.SendGump(new HouseListGump(1011275, m_House.CoOwners, m_House, false));

						break;
					}
				case 3: // Add co-owner
					{
						if (isOwner)
						{
							from.SendLocalizedMessage(501328); // Target the person you wish to name a co-owner of your household.
							from.Target = new CoOwnerTarget(true, m_House);
						}
						else
						{
							from.SendLocalizedMessage(501327); // Only the house owner may add Co-owners.
						}

						break;
					}
				case 4: // Remove co-owner
					{
						if (isOwner)
						{
							_ = from.CloseGump(typeof(HouseGump));
							_ = from.CloseGump(typeof(HouseListGump));
							_ = from.CloseGump(typeof(HouseRemoveGump));
							_ = from.SendGump(new HouseRemoveGump(1011274, m_House.CoOwners, m_House, false));
						}
						else
						{
							from.SendLocalizedMessage(501329); // Only the house owner may remove co-owners.
						}

						break;
					}
				case 5: // Clear co-owners
					{
						if (isOwner)
						{
							if (m_House.CoOwners != null)
							{
								m_House.CoOwners.Clear();
							}

							from.SendLocalizedMessage(501333); // All co-owners have been removed from this house.
						}
						else
						{
							from.SendLocalizedMessage(501330); // Only the house owner may remove co-owners.
						}

						break;
					}
				case 6: // List friends
					{
						_ = from.CloseGump(typeof(HouseGump));
						_ = from.CloseGump(typeof(HouseListGump));
						_ = from.CloseGump(typeof(HouseRemoveGump));
						_ = from.SendGump(new HouseListGump(1011273, m_House.Friends, m_House, false));

						break;
					}
				case 7: // Add friend
					{
						if (isCoOwner)
						{
							from.SendLocalizedMessage(501317); // Target the person you wish to name a friend of your household.
							from.Target = new HouseFriendTarget(true, m_House);
						}
						else
						{
							from.SendLocalizedMessage(501316); // Only the house owner may add friends.
						}

						break;
					}
				case 8: // Remove friend
					{
						if (isCoOwner)
						{
							_ = from.CloseGump(typeof(HouseGump));
							_ = from.CloseGump(typeof(HouseListGump));
							_ = from.CloseGump(typeof(HouseRemoveGump));
							_ = from.SendGump(new HouseRemoveGump(1011272, m_House.Friends, m_House, false));
						}
						else
						{
							from.SendLocalizedMessage(501318); // Only the house owner may remove friends.
						}

						break;
					}
				case 9: // Clear friends
					{
						if (isCoOwner)
						{
							if (m_House.Friends != null)
							{
								m_House.Friends.Clear();
							}

							from.SendLocalizedMessage(501332); // All friends have been removed from this house.
						}
						else
						{
							from.SendLocalizedMessage(501319); // Only the house owner may remove friends.
						}

						break;
					}
				case 10: // Ban
					{
						from.SendLocalizedMessage(501325); // Target the individual to ban from this house.
						from.Target = new HouseBanTarget(true, m_House);

						break;
					}
				case 11: // Eject
					{
						from.SendLocalizedMessage(501326); // Target the individual to eject from this house.
						from.Target = new HouseKickTarget(m_House);

						break;
					}
				case 12: // List bans
					{
						_ = from.CloseGump(typeof(HouseGump));
						_ = from.CloseGump(typeof(HouseListGump));
						_ = from.CloseGump(typeof(HouseRemoveGump));
						_ = from.SendGump(new HouseListGump(1011271, m_House.Bans, m_House, true));

						break;
					}
				case 13: // Remove ban
					{
						_ = from.CloseGump(typeof(HouseGump));
						_ = from.CloseGump(typeof(HouseListGump));
						_ = from.CloseGump(typeof(HouseRemoveGump));
						_ = from.SendGump(new HouseRemoveGump(1011269, m_House.Bans, m_House, true));

						break;
					}
				case 14: // Transfer ownership
					{
						if (isOwner)
						{
							from.SendLocalizedMessage(501309); // Target the person to whom you wish to give this house.
							from.Target = new HouseOwnerTarget(m_House);
						}
						else
						{
							from.SendLocalizedMessage(501310); // Only the house owner may do this.
						}

						break;
					}
				case 15: // Demolish house
					{
						if (isOwner)
						{
							if (!Guilds.Guild.NewGuildSystem && m_House.FindGuildstone() != null)
							{
								from.SendLocalizedMessage(501389); // You cannot redeed a house with a guildstone inside.
							}
							else
							{
								_ = from.CloseGump(typeof(HouseDemolishGump));
								_ = from.SendGump(new HouseDemolishGump(from, m_House));
							}
						}
						else
						{
							from.SendLocalizedMessage(501320); // Only the house owner may do this.
						}

						break;
					}
				case 16: // Change locks
					{
						if (m_House.Public)
						{
							from.SendLocalizedMessage(501669);// Public houses are always unlocked.
						}
						else
						{
							if (isOwner)
							{
								m_House.RemoveKeys(from);
								m_House.ChangeLocks(from);

								from.SendLocalizedMessage(501306); // The locks on your front door have been changed, and new master keys have been placed in your bank and your backpack.
							}
							else
							{
								from.SendLocalizedMessage(501303); // Only the house owner may change the house locks.
							}
						}

						break;
					}
				case 17: // Declare public/private
					{
						if (isOwner)
						{
							if (m_House.Public && m_House.PlayerVendors.Count > 0)
							{
								from.SendLocalizedMessage(501887); // You have vendors working out of this building. It cannot be declared private until there are no vendors in place.
								break;
							}

							m_House.Public = !m_House.Public;
							if (!m_House.Public)
							{
								m_House.ChangeLocks(from);

								from.SendLocalizedMessage(501888); // This house is now private.
								from.SendLocalizedMessage(501306); // The locks on your front door have been changed, and new master keys have been placed in your bank and your backpack.
							}
							else
							{
								m_House.RemoveKeys(from);
								m_House.RemoveLocks();
								from.SendLocalizedMessage(501886);//This house is now public. Friends of the house my now have vendors working out of this building.
							}
						}
						else
						{
							from.SendLocalizedMessage(501307); // Only the house owner may do this.
						}

						break;
					}
				case 18: // Change type
					{
						if (isOwner)
						{
							if (m_House.Public && info.Switches.Length > 0)
							{
								var index = info.Switches[0] - 1;

								if (index is >= 0 and < 53)
								{
									m_House.ChangeSignType(2980 + (index * 2));
								}
							}
						}
						else
						{
							from.SendLocalizedMessage(501307); // Only the house owner may do this.
						}

						break;
					}
			}
		}
	}

	public enum HouseGumpPageAOS
	{
		Information,
		Security,
		Storage,
		Customize,
		Ownership,
		ChangeHanger,
		ChangeFoundation,
		ChangeSign,
		RemoveCoOwner,
		ListCoOwner,
		RemoveFriend,
		ListFriend,
		RemoveBan,
		ListBan,
		RemoveAccess,
		ListAccess,
		ChangePost,
		Vendors
	}

	public class HouseGumpAOS : Gump
	{
		private readonly BaseHouse m_House;
		private readonly HouseGumpPageAOS m_Page;

		private const short LabelColor16 = 0x7FFF;
		private const short SelectedColor16 = 0x421F;
		private const short DisabledColor16 = 0x4210;
		private const short WarningColor16 = 0x7E10;

		private const int LabelHue = 0x481;
		private const int HighlightedLabelHue = 0x64;

		private List<Mobile> m_List;

		private string GetOwnerName()
		{
			var m = m_House.Owner;

			if (m == null || m.Deleted)
			{
				return "(unowned)";
			}

			string name;

			if ((name = m.Name) == null || (name = name.Trim()).Length <= 0)
			{
				name = "(no name)";
			}

			return name;
		}

		private string GetDateTime(DateTime val)
		{
			if (val == DateTime.MinValue)
			{
				return "";
			}

			return val.ToString("yyyy'-'MM'-'dd HH':'mm':'ss");
		}

		public void AddPageButton(int x, int y, int buttonID, int number, HouseGumpPageAOS page)
		{
			var isSelection = m_Page == page;

			AddButton(x, y, isSelection ? 4006 : 4005, 4007, buttonID, GumpButtonType.Reply, 0);
			AddHtmlLocalized(x + 45, y, 200, 20, number, isSelection ? SelectedColor16 : LabelColor16, false, false);
		}

		public void AddButtonLabeled(int x, int y, int buttonID, int number)
		{
			AddButtonLabeled(x, y, buttonID, number, true);
		}

		public void AddButtonLabeled(int x, int y, int buttonID, int number, bool enabled)
		{
			if (enabled)
			{
				AddButton(x, y, 4005, 4007, buttonID, GumpButtonType.Reply, 0);
			}

			AddHtmlLocalized(x + 35, y, 240, 20, number, enabled ? LabelColor16 : DisabledColor16, false, false);
		}

		public void AddList<T>(ICollection<T> list, int button, bool accountOf, bool leadingStar, Mobile from) where T : Mobile
		{
			if (list == null)
			{
				return;
			}

			m_List = new(list);

			var lastPage = 0;
			var index = 0;
			var i = -1;

			foreach (var m in list)
			{
				++i;

				var xoffset = index % 20 / 10 * 200;
				var yoffset = index % 10 * 20;
				var page = 1 + (index / 20);

				if (page != lastPage)
				{
					if (lastPage != 0)
					{
						AddButton(40, 360, 4005, 4007, 0, GumpButtonType.Page, page);
					}

					AddPage(page);

					if (lastPage != 0)
					{
						AddButton(10, 360, 4014, 4016, 0, GumpButtonType.Page, lastPage);
					}

					lastPage = page;
				}

				string name;
				var labelHue = LabelHue;

				if (m is PlayerVendor vendor)
				{
					name = vendor.ShopName;

					if (vendor.IsOwner(from))
					{
						labelHue = HighlightedLabelHue;
					}
				}
				else if (m != null)
				{
					name = m.Name;
				}
				else
				{
					continue;
				}

				if ((name = name.Trim()).Length <= 0)
				{
					continue;
				}

				if (button != -1)
				{
					AddButton(10 + xoffset, 150 + yoffset, 4005, 4007, GetButtonID(button, i), GumpButtonType.Reply, 0);
				}

				if (accountOf && m.Player && m.Account != null)
				{
					name = "Account of " + name;
				}

				if (leadingStar)
				{
					name = "* " + name;
				}

				AddLabel(button > 0 ? 45 + xoffset : 10 + xoffset, 150 + yoffset, labelHue, name);
				++index;
			}
		}

		public static int GetButtonID(int type, int index)
		{
			return 1 + (index * 15) + type;
		}

		private static readonly int[] m_HangerNumbers = { 2968, 2970, 2972, 2974, 2976, 2978 };

		private static readonly int[] m_FoundationNumbers = Core.ML ? new[] { 20, 189, 765, 65, 101, 0x2DF7, 0x2DFB, 0x3672, 0x3676 } : new[] { 20, 189, 765, 65, 101 };

		private static readonly int[] m_PostNumbers =
		{
			9, 29, 54, 90, 147, 169,
			177, 204, 251, 257, 263,
			298, 347, 424, 441, 466,
			514, 600, 601, 602, 603,
			660, 666, 672, 898, 970,
			974, 982
		};

		private static readonly List<int> _HouseSigns = new();

		public HouseGumpAOS(HouseGumpPageAOS page, Mobile from, BaseHouse house) : base(50, 40)
		{
			m_House = house;
			m_Page = page;

			_ = from.CloseGump(typeof(HouseGumpAOS));
			//from.CloseGump( typeof( HouseListGump ) );
			//from.CloseGump( typeof( HouseRemoveGump ) );

			var isCombatRestricted = house.IsCombatRestricted(from);

			var isOwner = house.IsOwner(from);
			var isCoOwner = isOwner || house.IsCoOwner(from);
			var isFriend = isCoOwner || house.IsFriend(from);

			if (isCombatRestricted)
			{
				isFriend = isCoOwner = isOwner = false;
			}

			AddPage(0);

			if (isFriend || page == HouseGumpPageAOS.Vendors)
			{
				AddBackground(0, 0, 420, page != HouseGumpPageAOS.Vendors ? 440 : 420, 5054);

				AddImageTiled(10, 10, 400, 100, 2624);
				AddAlphaRegion(10, 10, 400, 100);

				AddImageTiled(10, 120, 400, 260, 2624);
				AddAlphaRegion(10, 120, 400, 260);

				AddImageTiled(10, 390, 400, page != HouseGumpPageAOS.Vendors ? 40 : 20, 2624);
				AddAlphaRegion(10, 390, 400, page != HouseGumpPageAOS.Vendors ? 40 : 20);

				AddButtonLabeled(250, page != HouseGumpPageAOS.Vendors ? 410 : 390, 0, 1060675); // CLOSE
			}

			AddImage(10, 10, 100);

			if (m_House.Sign != null)
			{
				var lines = Wrap(m_House.Sign.GetName());

				if (lines != null)
				{
					for (int i = 0, y = (114 - (lines.Count * 14)) / 2; i < lines.Count; ++i, y += 14)
					{
						var s = (string)lines[i];

						AddLabel(10 + ((160 - (s.Length * 8)) / 2), y, 0, s);
					}
				}
			}

			if (page == HouseGumpPageAOS.Vendors)
			{
				AddHtmlLocalized(10, 120, 400, 20, 1062428, LabelColor16, false, false); // <CENTER>SHOPS</CENTER>

				AddList(house.AvailableVendorsFor(from), 1, false, false, from);
				return;
			}

			if (!isFriend)
			{
				return;
			}

			if (house.Public)
			{
				AddButtonLabeled(10, 390, GetButtonID(0, 0), 1060674); // Banish
				AddButtonLabeled(10, 410, GetButtonID(0, 1), 1011261); // Lift a Ban
			}
			else
			{
				AddButtonLabeled(10, 390, GetButtonID(0, 2), 1060676); // Grant Access
				AddButtonLabeled(10, 410, GetButtonID(0, 3), 1060677); // Revoke Access
			}

			AddPageButton(150, 10, GetButtonID(1, 0), 1060668, HouseGumpPageAOS.Information);
			AddPageButton(150, 30, GetButtonID(1, 1), 1060669, HouseGumpPageAOS.Security);
			AddPageButton(150, 50, GetButtonID(1, 2), 1060670, HouseGumpPageAOS.Storage);
			AddPageButton(150, 70, GetButtonID(1, 3), 1060671, HouseGumpPageAOS.Customize);
			AddPageButton(150, 90, GetButtonID(1, 4), 1060672, HouseGumpPageAOS.Ownership);

			switch (page)
			{
				case HouseGumpPageAOS.Information:
					{
						AddHtmlLocalized(20, 130, 200, 20, 1011242, LabelColor16, false, false); // Owned By: 
						AddLabel(210, 130, LabelHue, GetOwnerName());

						AddHtmlLocalized(20, 170, 380, 20, 1018032, SelectedColor16, false, false); // This house is properly placed.
						AddHtmlLocalized(20, 190, 380, 20, 1018035, SelectedColor16, false, false); // This house is of modern design.
						AddHtmlLocalized(20, 210, 380, 20, (house is HouseFoundation) ? 1060681 : 1060680, SelectedColor16, false, false); // This is a (pre | custom)-built house.
						AddHtmlLocalized(20, 230, 380, 20, house.Public ? 1060678 : 1060679, SelectedColor16, false, false); // This house is (private | open to the public).

						switch (house.DecayType)
						{
							case DecayType.Ageless:
							case DecayType.AutoRefresh:
								{
									AddHtmlLocalized(20, 250, 380, 20, 1062209, SelectedColor16, false, false); // This house is <a href = "?ForceTopic97">Automatically</a> refreshed.
									break;
								}
							case DecayType.ManualRefresh:
								{
									AddHtmlLocalized(20, 250, 380, 20, 1062208, SelectedColor16, false, false); // This house is <a href = "?ForceTopic97">Grandfathered</a>.
									break;
								}
							case DecayType.Condemned:
								{
									AddHtmlLocalized(20, 250, 380, 20, 1062207, WarningColor16, false, false); // This house is <a href = "?ForceTopic97">Condemned</a>.
									break;
								}
						}

						AddHtmlLocalized(20, 290, 200, 20, 1060692, SelectedColor16, false, false); // Built On:
						AddLabel(250, 290, LabelHue, GetDateTime(house.BuiltOn));

						AddHtmlLocalized(20, 310, 200, 20, 1060693, SelectedColor16, false, false); // Last Traded:
						AddLabel(250, 310, LabelHue, GetDateTime(house.LastTraded));

						AddHtmlLocalized(20, 330, 200, 20, 1061793, SelectedColor16, false, false); // House Value
						AddLabel(250, 330, LabelHue, house.Price.ToString());

						AddHtmlLocalized(20, 360, 300, 20, 1011241, SelectedColor16, false, false); // Number of visits this building has had: 
						AddLabel(350, 360, LabelHue, house.Visits.ToString());

						break;
					}
				case HouseGumpPageAOS.Security:
					{
						AddButtonLabeled(10, 130, GetButtonID(3, 0), 1011266, isCoOwner); // View Co-Owner List
						AddButtonLabeled(10, 150, GetButtonID(3, 1), 1011267, isOwner); // Add a Co-Owner
						AddButtonLabeled(10, 170, GetButtonID(3, 2), 1018036, isOwner); // Remove a Co-Owner
						AddButtonLabeled(10, 190, GetButtonID(3, 3), 1011268, isOwner); // Clear Co-Owner List

						AddButtonLabeled(10, 220, GetButtonID(3, 4), 1011243); // View Friends List
						AddButtonLabeled(10, 240, GetButtonID(3, 5), 1011244, isCoOwner); // Add a Friend
						AddButtonLabeled(10, 260, GetButtonID(3, 6), 1018037, isCoOwner); // Remove a Friend
						AddButtonLabeled(10, 280, GetButtonID(3, 7), 1011245, isCoOwner); // Clear Friend List

						if (house.Public)
						{
							AddButtonLabeled(10, 310, GetButtonID(3, 8), 1011260); // View Ban List
							AddButtonLabeled(10, 330, GetButtonID(3, 9), 1060698); // Clear Ban List

							AddButtonLabeled(210, 130, GetButtonID(3, 12), 1060695, isOwner); // Change to Private

							AddHtmlLocalized(245, 150, 240, 20, 1060694, SelectedColor16, false, false); // Change to Public
						}
						else
						{
							AddButtonLabeled(10, 310, GetButtonID(3, 10), 1060699); // View Access List
							AddButtonLabeled(10, 330, GetButtonID(3, 11), 1060700); // Clear Access List

							AddHtmlLocalized(245, 130, 240, 20, 1060695, SelectedColor16, false, false); // Change to Private

							AddButtonLabeled(210, 150, GetButtonID(3, 13), 1060694, isOwner); // Change to Public
						}

						break;
					}
				case HouseGumpPageAOS.Storage:
					{
						AddHtmlLocalized(10, 130, 400, 20, 1060682, LabelColor16, false, false); // <CENTER>HOUSE STORAGE SUMMARY</CENTER>

						int fromSecures, fromVendors, fromLockdowns, fromMovingCrate;

						var maxSecures = house.GetAosMaxSecures();
						var curSecures = house.GetAosCurSecures(out fromSecures, out fromVendors, out fromLockdowns, out fromMovingCrate);

						var maxLockdowns = house.GetAosMaxLockdowns();
						var curLockdowns = house.GetAosCurLockdowns();

						var bonusStorage = (int)((house.BonusStorageScalar * 100) - 100);

						if (bonusStorage > 0)
						{
							AddHtmlLocalized(10, 150, 300, 20, 1072519, LabelColor16, false, false); // Increased Storage
							AddLabel(310, 150, LabelHue, String.Format("{0}%", bonusStorage));
						}

						AddHtmlLocalized(10, 170, 300, 20, 1060683, LabelColor16, false, false); // Maximum Secure Storage
						AddLabel(310, 170, LabelHue, maxSecures.ToString());

						AddHtmlLocalized(10, 190, 300, 20, 1060685, LabelColor16, false, false); // Used by Moving Crate
						AddLabel(310, 190, LabelHue, fromMovingCrate.ToString());

						AddHtmlLocalized(10, 210, 300, 20, 1060686, LabelColor16, false, false); // Used by Lockdowns
						AddLabel(310, 210, LabelHue, fromLockdowns.ToString());

						if (BaseHouse.NewVendorSystem)
						{
							AddHtmlLocalized(10, 230, 300, 20, 1060688, LabelColor16, false, false); // Used by Secure Containers
							AddLabel(310, 230, LabelHue, fromSecures.ToString());

							AddHtmlLocalized(10, 250, 300, 20, 1060689, LabelColor16, false, false); // Available Storage
							AddLabel(310, 250, LabelHue, Math.Max(maxSecures - curSecures, 0).ToString());

							AddHtmlLocalized(10, 290, 300, 20, 1060690, LabelColor16, false, false); // Maximum Lockdowns
							AddLabel(310, 290, LabelHue, maxLockdowns.ToString());

							AddHtmlLocalized(10, 310, 300, 20, 1060691, LabelColor16, false, false); // Available Lockdowns
							AddLabel(310, 310, LabelHue, Math.Max(maxLockdowns - curLockdowns, 0).ToString());

							var maxVendors = house.GetNewVendorSystemMaxVendors();
							var vendors = house.PlayerVendors.Count + house.VendorRentalContracts.Count;

							AddHtmlLocalized(10, 350, 300, 20, 1062391, LabelColor16, false, false); // Vendor Count
							AddLabel(310, 350, LabelHue, vendors.ToString() + " / " + maxVendors.ToString());
						}
						else
						{
							AddHtmlLocalized(10, 230, 300, 20, 1060687, LabelColor16, false, false); // Used by Vendors
							AddLabel(310, 230, LabelHue, fromVendors.ToString());

							AddHtmlLocalized(10, 250, 300, 20, 1060688, LabelColor16, false, false); // Used by Secure Containers
							AddLabel(310, 250, LabelHue, fromSecures.ToString());

							AddHtmlLocalized(10, 270, 300, 20, 1060689, LabelColor16, false, false); // Available Storage
							AddLabel(310, 270, LabelHue, Math.Max(maxSecures - curSecures, 0).ToString());

							AddHtmlLocalized(10, 330, 300, 20, 1060690, LabelColor16, false, false); // Maximum Lockdowns
							AddLabel(310, 330, LabelHue, maxLockdowns.ToString());

							AddHtmlLocalized(10, 350, 300, 20, 1060691, LabelColor16, false, false); // Available Lockdowns
							AddLabel(310, 350, LabelHue, Math.Max(maxLockdowns - curLockdowns, 0).ToString());
						}

						break;
					}
				case HouseGumpPageAOS.Customize:
					{
						var isCustomizable = isOwner && (house is HouseFoundation);

						AddButtonLabeled(10, 120, GetButtonID(5, 0), 1060759, isOwner && !isCustomizable && (house.ConvertEntry != null)); // Convert Into Customizable House
						AddButtonLabeled(10, 160, GetButtonID(5, 1), 1060765, isOwner && isCustomizable); // Customize This House
						AddButtonLabeled(10, 180, GetButtonID(5, 2), 1060760, isOwner && house.MovingCrate != null); // Relocate Moving Crate
						AddButtonLabeled(10, 210, GetButtonID(5, 3), 1060761, isOwner && house.Public); // Change House Sign
						AddButtonLabeled(10, 230, GetButtonID(5, 4), 1060762, isOwner && isCustomizable); // Change House Sign Hanger
						AddButtonLabeled(10, 250, GetButtonID(5, 5), 1060763, isOwner && isCustomizable && (((HouseFoundation)house).Signpost != null)); // Change Signpost
						AddButtonLabeled(10, 280, GetButtonID(5, 6), 1062004, isOwner && isCustomizable); // Change Foundation Style
						AddButtonLabeled(10, 310, GetButtonID(5, 7), 1060764, isCoOwner); // Rename House

						break;
					}
				case HouseGumpPageAOS.Ownership:
					{
						AddButtonLabeled(10, 130, GetButtonID(6, 0), 1061794, isOwner && house.MovingCrate == null && house.InternalizedVendors.Count == 0); // Demolish House
						AddButtonLabeled(10, 150, GetButtonID(6, 1), 1061797, isOwner); // Trade House
						AddButtonLabeled(10, 190, GetButtonID(6, 2), 1061798, false); // Make Primary

						break;
					}
				case HouseGumpPageAOS.ChangeHanger:
					{
						for (var i = 0; i < m_HangerNumbers.Length; ++i)
						{
							var x = 50 + (i % 3 * 100);
							var y = 180 + (i / 3 * 80);

							AddButton(x, y, 4005, 4007, GetButtonID(7, i), GumpButtonType.Reply, 0);
							AddItem(x + 20, y, m_HangerNumbers[i]);
						}

						break;
					}
				case HouseGumpPageAOS.ChangeFoundation:
					{
						for (var i = 0; i < m_FoundationNumbers.Length; ++i)
						{
							var x = 15 + (i % 5 * 80);
							var y = 180 + (i / 5 * 100);

							AddButton(x, y, 4005, 4007, GetButtonID(8, i), GumpButtonType.Reply, 0);
							AddItem(x + 25, y, m_FoundationNumbers[i]);
						}

						break;
					}
				case HouseGumpPageAOS.ChangeSign:
					{
						var index = 0;

						if (_HouseSigns.Count == 0)
						{
							// Add standard signs
							for (var i = 0; i < 54; ++i)
							{
								_HouseSigns.Add(2980 + (i * 2));
							}

							// Add library and beekeeper signs ( ML )
							_HouseSigns.Add(2966);
							_HouseSigns.Add(3140);
						}

						var signsPerPage = Core.ML ? 24 : 18;
						var totalSigns = Core.ML ? 56 : 54;
						var pages = (int)Math.Ceiling((double)totalSigns / signsPerPage);

						for (var i = 0; i < pages; ++i)
						{
							AddPage(i + 1);

							AddButton(10, 360, 4005, 4007, 0, GumpButtonType.Page, ((i + 1) % pages) + 1);

							for (var j = 0; j < signsPerPage && totalSigns - (signsPerPage * i) - j > 0; ++j)
							{
								var x = 30 + (j % 6 * 60);
								var y = 130 + (j / 6 * 60);

								AddButton(x, y, 4005, 4007, GetButtonID(9, index), GumpButtonType.Reply, 0);
								AddItem(x + 20, y, _HouseSigns[index++]);
							}
						}

						break;
					}
				case HouseGumpPageAOS.RemoveCoOwner:
					{
						AddHtmlLocalized(10, 120, 400, 20, 1060730, LabelColor16, false, false); // <CENTER>CO-OWNER LIST</CENTER>
						AddList(house.CoOwners, 10, false, true, from);
						break;
					}
				case HouseGumpPageAOS.ListCoOwner:
					{
						AddHtmlLocalized(10, 120, 400, 20, 1060730, LabelColor16, false, false); // <CENTER>CO-OWNER LIST</CENTER>
						AddList(house.CoOwners, -1, false, true, from);
						break;
					}
				case HouseGumpPageAOS.RemoveFriend:
					{
						AddHtmlLocalized(10, 120, 400, 20, 1060731, LabelColor16, false, false); // <CENTER>FRIENDS LIST</CENTER>
						AddList(house.Friends, 11, false, true, from);
						break;
					}
				case HouseGumpPageAOS.ListFriend:
					{
						AddHtmlLocalized(10, 120, 400, 20, 1060731, LabelColor16, false, false); // <CENTER>FRIENDS LIST</CENTER>
						AddList(house.Friends, -1, false, true, from);
						break;
					}
				case HouseGumpPageAOS.RemoveBan:
					{
						AddHtmlLocalized(10, 120, 400, 20, 1060733, LabelColor16, false, false); // <CENTER>BAN LIST</CENTER>
						AddList(house.Bans, 12, true, true, from);
						break;
					}
				case HouseGumpPageAOS.ListBan:
					{
						AddHtmlLocalized(10, 120, 400, 20, 1060733, LabelColor16, false, false); // <CENTER>BAN LIST</CENTER>
						AddList(house.Bans, -1, true, true, from);
						break;
					}
				case HouseGumpPageAOS.RemoveAccess:
					{
						AddHtmlLocalized(10, 120, 400, 20, 1060732, LabelColor16, false, false); // <CENTER>ACCESS LIST</CENTER>
						AddList(house.Access, 13, false, true, from);
						break;
					}
				case HouseGumpPageAOS.ListAccess:
					{
						AddHtmlLocalized(10, 120, 400, 20, 1060732, LabelColor16, false, false); // <CENTER>ACCESS LIST</CENTER>
						AddList(house.Access, -1, false, true, from);
						break;
					}
				case HouseGumpPageAOS.ChangePost:
					{
						var index = 0;

						for (var i = 0; i < 2; ++i)
						{
							AddPage(i + 1);

							AddButton(10, 360, 4005, 4007, 0, GumpButtonType.Page, ((i + 1) % 2) + 1);

							for (var j = 0; j < 16 && index < m_PostNumbers.Length; ++j)
							{
								var x = 15 + (j % 8 * 50);
								var y = 130 + (j / 8 * 110);

								AddButton(x, y, 4005, 4007, GetButtonID(14, index), GumpButtonType.Reply, 0);
								AddItem(x + 10, y, m_PostNumbers[index++]);
							}
						}

						break;
					}
			}
		}

		public static void PublicPrivateNotice_Callback(Mobile from, BaseHouse house)
		{
			if (!house.Deleted)
			{
				_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, house));
			}
		}

		public static void CustomizeNotice_Callback(Mobile from, BaseHouse house)
		{
			if (!house.Deleted)
			{
				_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Customize, from, house));
			}
		}

		public static void ClearCoOwners_Callback(Mobile from, bool okay, BaseHouse house)
		{
			if (house.Deleted)
			{
				return;
			}

			if (okay && house.IsOwner(from))
			{
				if (house.CoOwners != null)
				{
					house.CoOwners.Clear();
				}

				from.SendLocalizedMessage(501333); // All co-owners have been removed from this house.
			}

			_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, house));
		}

		public static void ClearFriends_Callback(Mobile from, bool okay, BaseHouse house)
		{
			if (house.Deleted)
			{
				return;
			}

			if (okay && house.IsCoOwner(from))
			{
				if (house.Friends != null)
				{
					house.Friends.Clear();
				}

				from.SendLocalizedMessage(501332); // All friends have been removed from this house.
			}

			_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, house));
		}

		public static void ClearBans_Callback(Mobile from, bool okay, BaseHouse house)
		{
			if (house.Deleted)
			{
				return;
			}

			if (okay && house.IsFriend(from))
			{
				if (house.Bans != null)
				{
					house.Bans.Clear();
				}

				from.SendLocalizedMessage(1060754); // All bans for this house have been lifted.
			}

			_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, house));
		}

		public static void ClearAccess_Callback(Mobile from, bool okay, BaseHouse house)
		{
			if (house.Deleted)
			{
				return;
			}

			if (okay && house.IsFriend(from))
			{
				var list = house.Access.ToArray();

				if (house.Access != null)
				{
					house.Access.Clear();
				}

				foreach (var m in list)
				{
					if (!house.HasAccess(m) && house.IsInside(m))
					{
						m.Location = house.BanLocation;
						m.SendLocalizedMessage(1060734); // Your access to this house has been revoked.
					}
				}

				from.SendLocalizedMessage(1061843); // This house's Access List has been cleared.
			}

			_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, house));
		}

		public static void ConvertHouse_Callback(Mobile from, bool okay, BaseHouse house)
		{
			if (house.Deleted)
			{
				return;
			}

			if (okay && house.IsOwner(from) && !house.HasRentedVendors)
			{
				var e = house.ConvertEntry;

				if (e != null)
				{
					var cost = e.Cost - house.Price;

					if (cost > 0)
					{
						if (Banker.Withdraw(from, cost))
						{
							from.SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
						}
						else
						{
							from.SendLocalizedMessage(1061624); // You do not have enough funds in your bank to cover the difference between your old house and your new one.
							return;
						}
					}
					else if (cost < 0)
					{
						if (Banker.Deposit(from, -cost))
						{
							from.SendLocalizedMessage(1060397, (-cost).ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
						}
						else
						{
							return;
						}
					}

					var newHouse = e.ConstructHouse(from);

					if (newHouse != null)
					{
						newHouse.Price = e.Cost;

						house.MoveAllToCrate();

						newHouse.Friends = new(house.Friends);
						newHouse.CoOwners = new(house.CoOwners);
						newHouse.Bans = new(house.Bans);
						newHouse.Access = new(house.Access);
						newHouse.BuiltOn = house.BuiltOn;
						newHouse.LastTraded = house.LastTraded;
						newHouse.Public = house.Public;

						newHouse.VendorInventories.UnionWith(house.VendorInventories);
						house.VendorInventories.Clear();

						foreach (var inventory in newHouse.VendorInventories)
						{
							inventory.House = newHouse;
						}

						newHouse.InternalizedVendors.UnionWith(house.InternalizedVendors);
						house.InternalizedVendors.Clear();

						foreach (var mobile in newHouse.InternalizedVendors)
						{
							if (mobile is PlayerVendor)
							{
								((PlayerVendor)mobile).House = newHouse;
							}
							else if (mobile is PlayerBarkeeper)
							{
								((PlayerBarkeeper)mobile).House = newHouse;
							}
						}

						if (house.MovingCrate != null)
						{
							newHouse.MovingCrate = house.MovingCrate;
							newHouse.MovingCrate.House = newHouse;
							house.MovingCrate = null;
						}

						var items = house.GetItems();
						var mobiles = house.GetMobiles();

						newHouse.MoveToWorld(new Point3D(house.X + house.ConvertOffsetX, house.Y + house.ConvertOffsetY, house.Z + house.ConvertOffsetZ), house.Map);
						house.Delete();

						foreach (var item in items)
						{
							item.Location = newHouse.BanLocation;
						}

						foreach (var mobile in mobiles)
						{
							mobile.Location = newHouse.BanLocation;
						}

						/* You have successfully replaced your original house with a new house.
						 * The value of the replaced house has been deposited into your bank box.
						 * All of the items in your original house have been relocated to a Moving Crate in the new house.
						 * Any deed-based house add-ons have been converted back into deeds.
						 * Vendors and barkeeps in the house, if any, have been stored in the Moving Crate as well.
						 * Use the <B>Get Vendor</B> context-sensitive menu option on your character to retrieve them.
						 * These containers can be used to re-create the vendor in a new location.
						 * Any barkeepers have been converted into deeds.
						 */
						_ = from.SendGump(new NoticeGump(1060637, 30720, 1060012, 32512, 420, 280, null, null));
						return;
					}
				}
			}

			_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, house));
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_House.Deleted)
			{
				return;
			}

			var from = sender.Mobile;

			var isCombatRestricted = m_House.IsCombatRestricted(from);

			var isOwner = m_House.IsOwner(from);
			var isCoOwner = isOwner || m_House.IsCoOwner(from);
			var isFriend = isCoOwner || m_House.IsFriend(from);

			if (isCombatRestricted)
			{
				isCoOwner = isFriend = false;
			}

			if (!from.CheckAlive())
			{
				return;
			}

			Item sign = m_House.Sign;

			if (sign == null || from.Map != sign.Map || !from.InRange(sign.GetWorldLocation(), 18))
			{
				return;
			}

			var foundation = m_House as HouseFoundation;
			var isCustomizable = foundation != null;

			var val = info.ButtonID - 1;

			if (val < 0)
			{
				return;
			}

			var type = val % 15;
			var index = val / 15;

			if (m_Page == HouseGumpPageAOS.Vendors)
			{
				if (index >= 0 && index < m_List.Count)
				{
					if (m_List[index] is PlayerVendor vendor)
					{
						if (!vendor.CanInteractWith(from, false))
						{
							return;
						}

						if (from.Map != sign.Map || !from.InRange(sign, 5))
						{
							from.SendLocalizedMessage(1062429); // You must be within five paces of the house sign to use this option.
						}
						else if (vendor.IsOwner(from))
						{
							vendor.SendOwnerGump(from);
						}
						else
						{
							vendor.OpenBackpack(from);
						}
					}
				}

				return;
			}

			if (!isFriend)
			{
				return;
			}

			switch (type)
			{
				case 0:
					{
						switch (index)
						{
							case 0: // Banish
								{
									if (m_House.Public)
									{
										from.SendLocalizedMessage(501325); // Target the individual to ban from this house.
										from.Target = new HouseBanTarget(true, m_House);
									}

									break;
								}
							case 1: // Lift Ban
								{
									if (m_House.Public)
									{
										_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.RemoveBan, from, m_House));
									}

									break;
								}
							case 2: // Grant Access
								{
									if (!m_House.Public)
									{
										from.SendLocalizedMessage(1060711); // Target the person you would like to grant access to.
										from.Target = new HouseAccessTarget(m_House);
									}

									break;
								}
							case 3: // Revoke Access
								{
									if (!m_House.Public)
									{
										_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.RemoveAccess, from, m_House));
									}

									break;
								}
						}

						break;
					}
				case 1:
					{
						HouseGumpPageAOS page;

						switch (index)
						{
							case 0: page = HouseGumpPageAOS.Information; break;
							case 1: page = HouseGumpPageAOS.Security; break;
							case 2: page = HouseGumpPageAOS.Storage; break;
							case 3: page = HouseGumpPageAOS.Customize; break;
							case 4: page = HouseGumpPageAOS.Ownership; break;
							default: return;
						}

						_ = from.SendGump(new HouseGumpAOS(page, from, m_House));
						break;
					}
				case 3:
					{
						switch (index)
						{
							case 0: // View Co-Owner List
								{
									if (isCoOwner)
									{
										_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.ListCoOwner, from, m_House));
									}

									break;
								}
							case 1: // Add a Co-Owner
								{
									if (isOwner)
									{
										from.SendLocalizedMessage(501328); // Target the person you wish to name a co-owner of your household.
										from.Target = new CoOwnerTarget(true, m_House);
									}

									break;
								}
							case 2: // Remove a Co-Owner
								{
									if (isOwner)
									{
										_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.RemoveCoOwner, from, m_House));
									}

									break;
								}
							case 3: // Clear Co-Owner List
								{
									if (isOwner)
									{
										_ = from.SendGump(new WarningGump<BaseHouse>(1060635, 30720, 1060736, 32512, 420, 280, ClearCoOwners_Callback, m_House));
									}

									break;
								}
							case 4: // View Friends List
								{
									_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.ListFriend, from, m_House));

									break;
								}
							case 5: // Add a Friend
								{
									if (isCoOwner)
									{
										from.SendLocalizedMessage(501317); // Target the person you wish to name a friend of your household.
										from.Target = new HouseFriendTarget(true, m_House);
									}

									break;
								}
							case 6: // Remove a Friend
								{
									if (isCoOwner)
									{
										_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.RemoveFriend, from, m_House));
									}

									break;
								}
							case 7: // Clear Friend List
								{
									if (isCoOwner)
									{
										_ = from.SendGump(new WarningGump<BaseHouse>(1060635, 30720, 1018039, 32512, 420, 280, ClearFriends_Callback, m_House));
									}

									break;
								}
							case 8: // View Ban List
								{
									_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.ListBan, from, m_House));

									break;
								}
							case 9: // Clear Ban List
								{
									_ = from.SendGump(new WarningGump<BaseHouse>(1060635, 30720, 1060753, 32512, 420, 280, ClearBans_Callback, m_House));

									break;
								}
							case 10: // View Access List
								{
									_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.ListAccess, from, m_House));

									break;
								}
							case 11: // Clear Access List
								{
									_ = from.SendGump(new WarningGump<BaseHouse>(1060635, 30720, 1061842, 32512, 420, 280, ClearAccess_Callback, m_House));

									break;
								}
							case 12: // Make Private
								{
									if (isOwner)
									{
										if (m_House.PlayerVendors.Count > 0)
										{
											// You have vendors working out of this building. It cannot be declared private until there are no vendors in place.
											_ = from.SendGump(new NoticeGump<BaseHouse>(1060637, 30720, 501887, 32512, 320, 180, PublicPrivateNotice_Callback, m_House));
											break;
										}

										if (m_House.VendorRentalContracts.Count > 0)
										{
											// You cannot currently take this action because you have vendor contracts locked down in your home.  You must remove them first.
											_ = from.SendGump(new NoticeGump<BaseHouse>(1060637, 30720, 1062351, 32512, 320, 180, PublicPrivateNotice_Callback, m_House));
											break;
										}

										m_House.Public = false;

										m_House.ChangeLocks(from);

										// This house is now private.
										_ = from.SendGump(new NoticeGump<BaseHouse>(1060637, 30720, 501888, 32512, 320, 180, PublicPrivateNotice_Callback, m_House));

										var r = m_House.Region;
										var list = r.GetMobiles();

										for (var i = 0; i < list.Count; ++i)
										{
											var m = list[i];

											if (!m_House.HasAccess(m) && m_House.IsInside(m))
											{
												m.Location = m_House.BanLocation;
											}
										}
									}

									break;
								}
							case 13: // Make Public
								{
									if (isOwner)
									{
										m_House.Public = true;

										m_House.RemoveKeys(from);
										m_House.RemoveLocks();

										if (BaseHouse.NewVendorSystem)
										{
											// This house is now public. The owner may now place vendors and vendor rental contracts.
											_ = from.SendGump(new NoticeGump<BaseHouse>(1060637, 30720, 501886, 32512, 320, 180, PublicPrivateNotice_Callback, m_House));
										}
										else
										{
											_ = from.SendGump(new NoticeGump<BaseHouse>(1060637, 30720, "This house is now public. Friends of the house may now have vendors working out of this building.", 0xF8C000, 320, 180, PublicPrivateNotice_Callback, m_House));
										}

										var r = m_House.Region;
										var list = r.GetMobiles();

										for (var i = 0; i < list.Count; ++i)
										{
											var m = list[i];

											if (m_House.IsBanned(m) && m_House.IsInside(m))
											{
												m.Location = m_House.BanLocation;
											}
										}
									}

									break;
								}
						}

						break;
					}
				case 5:
					{
						switch (index)
						{
							case 0: // Convert Into Customizable House
								{
									if (isOwner && !isCustomizable)
									{
										if (m_House.HasRentedVendors)
										{
											// You cannot perform this action while you still have vendors rented out in this house.
											_ = from.SendGump(new NoticeGump<BaseHouse>(1060637, 30720, 1062395, 32512, 320, 180, CustomizeNotice_Callback, m_House));
										}
										else
										{
											var e = m_House.ConvertEntry;

											if (e != null)
											{
												/* You are about to turn your house into a customizable house.
												 * You will be refunded the value of this house, and then be charged the cost of the equivalent customizable dirt lot.
												 * All of your possessions in the house will be transported to a Moving Crate.
												 * Deed-based house add-ons will be converted back into deeds.
												 * Vendors and barkeeps will also be stored in the Moving Crate.
												 * Your house will be leveled to its foundation, and you will be able to build new walls, windows, doors, and stairs.
												 * Are you sure you wish to continue?
												 */
												_ = from.SendGump(new WarningGump<BaseHouse>(1060635, 30720, 1060013, 32512, 420, 280, ConvertHouse_Callback, m_House));
											}
										}
									}

									break;
								}
							case 1: // Customize This House
								{
									if (isOwner && isCustomizable)
									{
										if (m_House.HasRentedVendors)
										{
											// You cannot perform this action while you still have vendors rented out in this house.
											_ = from.SendGump(new NoticeGump<BaseHouse>(1060637, 30720, 1062395, 32512, 320, 180, CustomizeNotice_Callback, m_House));
										}
										else if (m_House.HasAddonContainers)
										{
											// The house can not be customized when add-on containers such as aquariums, elven furniture containers, vanities, and boiling cauldrons 
											// are present in the house.  Please re-deed the add-on containers before customizing the house.
											_ = from.SendGump(new NoticeGump<BaseHouse>(1060637, 30720, 1074863, 32512, 320, 180, CustomizeNotice_Callback, m_House));
										}
										else
										{
											foundation.BeginCustomize(from);
										}
									}

									break;
								}
							case 2: // Relocate Moving Crate
								{
									var crate = m_House.MovingCrate;

									if (isOwner && crate != null)
									{
										if (!m_House.IsInside(from))
										{
											from.SendLocalizedMessage(502092); // You must be in your house to do this.
										}
										else
										{
											crate.MoveToWorld(from.Location, from.Map);
											crate.RestartTimer();
										}
									}

									break;
								}
							case 3: // Change House Sign
								{
									if (isOwner && m_House.Public)
									{
										_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.ChangeSign, from, m_House));
									}

									break;
								}
							case 4: // Change House Sign Hanger
								{
									if (isOwner && isCustomizable)
									{
										_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.ChangeHanger, from, m_House));
									}

									break;
								}
							case 5: // Change Signpost
								{
									if (isOwner && isCustomizable && foundation.Signpost != null)
									{
										_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.ChangePost, from, m_House));
									}

									break;
								}
							case 6: // Change Foundation Style
								{
									if (isOwner && isCustomizable)
									{
										_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.ChangeFoundation, from, m_House));
									}

									break;
								}
							case 7: // Rename House
								{
									if (isCoOwner)
									{
										from.Prompt = new RenamePrompt(m_House);
										from.SendLocalizedMessage(501302); // What dost thou wish the sign to say?
									}

									break;
								}
						}

						break;
					}
				case 6:
					{
						switch (index)
						{
							case 0: // Demolish
								{
									if (isOwner && m_House.MovingCrate == null && m_House.InternalizedVendors.Count == 0)
									{
										if (!Guilds.Guild.NewGuildSystem && m_House.FindGuildstone() != null)
										{
											from.SendLocalizedMessage(501389); // You cannot redeed a house with a guildstone inside.
										}
										else if (Core.ML && from.AccessLevel < AccessLevel.GameMaster && DateTime.UtcNow <= m_House.BuiltOn.AddHours(1))
										{
											from.SendLocalizedMessage(1080178); // You must wait one hour between each house demolition.
										}
										else
										{
											_ = from.CloseGump(typeof(HouseDemolishGump));
											_ = from.SendGump(new HouseDemolishGump(from, m_House));
										}
									}

									break;
								}
							case 1: // Trade House
								{
									if (isOwner)
									{
										if (BaseHouse.NewVendorSystem && m_House.HasPersonalVendors)
										{
											from.SendLocalizedMessage(1062467); // You cannot trade this house while you still have personal vendors inside.
										}
										else if (m_House.DecayLevel == DecayLevel.DemolitionPending)
										{
											from.SendLocalizedMessage(1005321); // This house has been marked for demolition, and it cannot be transferred.
										}
										else
										{
											from.SendLocalizedMessage(501309); // Target the person to whom you wish to give this house.
											from.Target = new HouseOwnerTarget(m_House);
										}
									}

									break;
								}
							case 2: // Make Primary
								break;
						}

						break;
					}
				case 7:
					{
						if (isOwner && isCustomizable && index >= 0 && index < m_HangerNumbers.Length)
						{
							var hanger = foundation.SignHanger;

							if (hanger != null)
							{
								hanger.ItemID = m_HangerNumbers[index];
							}

							_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Customize, from, m_House));
						}

						break;
					}
				case 8:
					{
						if (isOwner && isCustomizable)
						{
							FoundationType newType;

							if (Core.ML && index >= 5)
							{
								switch (index)
								{
									case 5: newType = FoundationType.ElvenGrey; break;
									case 6: newType = FoundationType.ElvenNatural; break;
									case 7: newType = FoundationType.Crystal; break;
									case 8: newType = FoundationType.Shadow; break;
									default: return;
								}
							}
							else
							{
								switch (index)
								{
									case 0: newType = FoundationType.DarkWood; break;
									case 1: newType = FoundationType.LightWood; break;
									case 2: newType = FoundationType.Dungeon; break;
									case 3: newType = FoundationType.Brick; break;
									case 4: newType = FoundationType.Stone; break;
									default: return;
								}
							}

							foundation.Type = newType;

							var state = foundation.BackupState;
							HouseFoundation.ApplyFoundation(newType, state.Components);
							state.OnRevised();

							state = foundation.DesignState;
							HouseFoundation.ApplyFoundation(newType, state.Components);
							state.OnRevised();

							state = foundation.CurrentState;
							HouseFoundation.ApplyFoundation(newType, state.Components);
							state.OnRevised();

							foundation.Delta(ItemDelta.Update);

							_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Customize, from, m_House));
						}

						break;
					}
				case 9:
					{
						if (isOwner && m_House.Public && index >= 0 && index < _HouseSigns.Count)
						{
							m_House.ChangeSignType(_HouseSigns[index]);
							_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Customize, from, m_House));
						}

						break;
					}
				case 10:
					{
						if (isOwner && m_List != null && index >= 0 && index < m_List.Count)
						{
							m_House.RemoveCoOwner(from, m_List[index]);

							if (m_House.CoOwners.Count > 0)
							{
								_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.RemoveCoOwner, from, m_House));
							}
							else
							{
								_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, m_House));
							}
						}

						break;
					}
				case 11:
					{
						if (isCoOwner && m_List != null && index >= 0 && index < m_List.Count)
						{
							m_House.RemoveFriend(from, m_List[index]);

							if (m_House.Friends.Count > 0)
							{
								_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.RemoveFriend, from, m_House));
							}
							else
							{
								_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, m_House));
							}
						}

						break;
					}
				case 12:
					{
						if (m_List != null && index >= 0 && index < m_List.Count)
						{
							m_House.RemoveBan(from, m_List[index]);

							if (m_House.Bans.Count > 0)
							{
								_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.RemoveBan, from, m_House));
							}
							else
							{
								_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, m_House));
							}
						}

						break;
					}
				case 13:
					{
						if (m_List != null && index >= 0 && index < m_List.Count)
						{
							m_House.RemoveAccess(from, m_List[index]);

							if (m_House.Access.Count > 0)
							{
								_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.RemoveAccess, from, m_House));
							}
							else
							{
								_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Security, from, m_House));
							}
						}

						break;
					}
				case 14:
					{
						if (isOwner && isCustomizable && index >= 0 && index < m_PostNumbers.Length)
						{
							foundation.SignpostGraphic = m_PostNumbers[index];
							foundation.CheckSignpost();

							_ = from.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Customize, from, m_House));
						}

						break;
					}
			}
		}

		private ArrayList Wrap(string value)
		{
			if (value == null || (value = value.Trim()).Length <= 0)
			{
				return null;
			}

			var values = value.Split(' ');
			var list = new ArrayList();
			var current = "";

			for (var i = 0; i < values.Length; ++i)
			{
				var val = values[i];

				var v = current.Length == 0 ? val : current + ' ' + val;

				if (v.Length < 10)
				{
					current = v;
				}
				else if (v.Length == 10)
				{
					_ = list.Add(v);

					if (list.Count == 6)
					{
						return list;
					}

					current = "";
				}
				else if (val.Length <= 10)
				{
					_ = list.Add(current);

					if (list.Count == 6)
					{
						return list;
					}

					current = val;
				}
				else
				{
					while (v.Length >= 10)
					{
						_ = list.Add(v.Substring(0, 10));

						if (list.Count == 6)
						{
							return list;
						}

						v = v.Substring(10);
					}

					current = v;
				}
			}

			if (current.Length > 0)
			{
				_ = list.Add(current);
			}

			return list;
		}
	}

	public interface ISecurable
	{
		SecureLevel Level { get; set; }
	}

	public class SetSecureLevelGump : Gump
	{
		private readonly ISecurable m_Info;

		public SetSecureLevelGump(Mobile owner, ISecurable info, BaseHouse house) : base(50, 50)
		{
			m_Info = info;

			AddPage(0);

			var offset = Guild.NewGuildSystem ? 20 : 0;

			AddBackground(0, 0, 220, 160 + offset, 5054);

			AddImageTiled(10, 10, 200, 20, 5124);
			AddImageTiled(10, 40, 200, 20, 5124);
			AddImageTiled(10, 70, 200, 80 + offset, 5124);

			AddAlphaRegion(10, 10, 200, 140);

			AddHtmlLocalized(10, 10, 200, 20, 1061276, 32767, false, false); // <CENTER>SET ACCESS</CENTER>
			AddHtmlLocalized(10, 40, 100, 20, 1041474, 32767, false, false); // Owner:

			AddLabel(110, 40, 1152, owner == null ? "" : owner.Name);

			AddButton(10, 70, GetFirstID(SecureLevel.Owner), 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 70, 150, 20, 1061277, GetColor16(SecureLevel.Owner), false, false); // Owner Only

			AddButton(10, 90, GetFirstID(SecureLevel.CoOwners), 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 90, 150, 20, 1061278, GetColor16(SecureLevel.CoOwners), false, false); // Co-Owners

			AddButton(10, 110, GetFirstID(SecureLevel.Friends), 4007, 3, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 110, 150, 20, 1061279, GetColor16(SecureLevel.Friends), false, false); // Friends

			var houseOwner = house.Owner;
			if (Guild.NewGuildSystem && house != null && houseOwner != null && houseOwner.Guild != null && ((Guild)houseOwner.Guild).Leader == houseOwner)  //Only the actual House owner AND guild master can set guild secures
			{
				AddButton(10, 130, GetFirstID(SecureLevel.Guild), 4007, 5, GumpButtonType.Reply, 0);
				AddHtmlLocalized(45, 130, 150, 20, 1063455, GetColor16(SecureLevel.Guild), false, false); // Guild Members
			}

			AddButton(10, 130 + offset, GetFirstID(SecureLevel.Anyone), 4007, 4, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 130 + offset, 150, 20, 1061626, GetColor16(SecureLevel.Anyone), false, false); // Anyone
		}

		public short GetColor16(SecureLevel level)
		{
			return (short)(m_Info.Level == level ? 0x7F18 : 0x7FFF);
		}

		public int GetFirstID(SecureLevel level)
		{
			return (m_Info.Level == level) ? 4006 : 4005;
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			var level = m_Info.Level;

			switch (info.ButtonID)
			{
				case 1: level = SecureLevel.Owner; break;
				case 2: level = SecureLevel.CoOwners; break;
				case 3: level = SecureLevel.Friends; break;
				case 4: level = SecureLevel.Anyone; break;
				case 5: level = SecureLevel.Guild; break;
			}

			if (m_Info.Level == level)
			{
				state.Mobile.SendLocalizedMessage(1061281); // Access level unchanged.
			}
			else
			{
				m_Info.Level = level;
				state.Mobile.SendLocalizedMessage(1061280); // New access level set.
			}
		}
	}

	public class HouseListGump : Gump
	{
		private readonly BaseHouse m_House;

		public HouseListGump(int number, ICollection<Mobile> list, BaseHouse house, bool accountOf) : base(20, 30)
		{
			if (house.Deleted)
			{
				return;
			}

			m_House = house;

			AddPage(0);

			AddBackground(0, 0, 420, 430, 5054);
			AddBackground(10, 10, 400, 410, 3000);

			AddButton(20, 388, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 388, 300, 20, 1011104, false, false); // Return to previous menu

			AddHtmlLocalized(20, 20, 350, 20, number, false, false);

			if (list != null)
			{
				var i = -1;

				foreach (var m in list)
				{
					++i;

					if ((i % 16) == 0)
					{
						if (i != 0)
						{
							// Next button
							AddButton(370, 20, 4005, 4007, 0, GumpButtonType.Page, (i / 16) + 1);
						}

						AddPage((i / 16) + 1);

						if (i != 0)
						{
							// Previous button
							AddButton(340, 20, 4014, 4016, 0, GumpButtonType.Page, i / 16);
						}
					}

					string name;

					if (m == null || (name = m.Name) == null || (name = name.Trim()).Length <= 0)
					{
						continue;
					}

					AddLabel(55, 55 + (i % 16 * 20), 0, accountOf && m.Player && m.Account != null ? String.Format("Account of {0}", name) : name);
				}
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (m_House.Deleted)
			{
				return;
			}

			var from = state.Mobile;

			_ = from.SendGump(new HouseGump(from, m_House));
		}
	}

	public class ConfirmHouseResize : Gump
	{
		private readonly Mobile m_Mobile;
		private readonly BaseHouse m_House;

		public ConfirmHouseResize(Mobile mobile, BaseHouse house) : base(110, 100)
		{
			m_Mobile = mobile;
			m_House = house;

			_ = mobile.CloseGump(typeof(ConfirmHouseResize));

			Closable = false;

			AddPage(0);

			AddBackground(0, 0, 420, 280, 0x13BE);
			AddImageTiled(10, 10, 400, 20, 0xA40);
			AddAlphaRegion(10, 10, 400, 20);
			AddHtmlLocalized(10, 10, 400, 20, 1060635, 0x7800, false, false); // <CENTER>WARNING</CENTER>
			AddImageTiled(10, 40, 400, 200, 0xA40);
			AddAlphaRegion(10, 40, 400, 200);

			/* You are attempting to resize your house. You will be refunded the house's 
			value directly to your bank box. All items in the house will *remain behind* 
			and can be *freely picked up by anyone*. Once the house is demolished, however, 
			only this account will be able to place on the land for one hour. This *will* 
			circumvent the normal 7-day waiting period (if it applies to you). This action 
			will not un-condemn any other houses on your account. If you have other, 
			grandfathered houses, this action *WILL* condemn them. Are you sure you wish 
			to continue?*/
			AddHtmlLocalized(10, 40, 400, 200, 1080196, 0x7F00, false, true);

			AddImageTiled(10, 250, 400, 20, 0xA40);
			AddAlphaRegion(10, 250, 400, 20);
			AddButton(10, 250, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
			AddButton(210, 250, 0xFA5, 0xFA7, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(40, 250, 170, 20, 1011036, 0x7FFF, false, false); // OKAY
			AddHtmlLocalized(240, 250, 170, 20, 1011012, 0x7FFF, false, false); // CANCEL

		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info.ButtonID == 1 && !m_House.Deleted)
			{
				if (m_House.IsOwner(m_Mobile))
				{
					if (m_House.MovingCrate != null || m_House.InternalizedVendors.Count > 0)
					{
						m_Mobile.SendLocalizedMessage(1080455); // You can not resize your house at this time. Please remove all items fom the moving crate and try again.
						return;
					}
					else if (!Guilds.Guild.NewGuildSystem && m_House.FindGuildstone() != null)
					{
						m_Mobile.SendLocalizedMessage(501389); // You cannot redeed a house with a guildstone inside.
						return;
					}
					/*else if ( m_House.PlayerVendors.Count > 0 )
					{
						m_Mobile.SendLocalizedMessage( 503236 ); // You need to collect your vendor's belongings before moving.
						return;
					}*/
					else if (m_House.HasRentedVendors && m_House.VendorInventories.Count > 0)
					{
						m_Mobile.SendLocalizedMessage(1062679); // You cannot do that that while you still have contract vendors or unclaimed contract vendor inventory in your house.
						return;
					}
					else if (m_House.HasRentedVendors)
					{
						m_Mobile.SendLocalizedMessage(1062680); // You cannot do that that while you still have contract vendors in your house.
						return;
					}
					else if (m_House.VendorInventories.Count > 0)
					{
						m_Mobile.SendLocalizedMessage(1062681); // You cannot do that that while you still have unclaimed contract vendor inventory in your house.
						return;
					}

					if (m_Mobile.AccessLevel >= AccessLevel.GameMaster)
					{
						m_Mobile.SendMessage("You do not get a refund for your house as you are not a player");
						m_House.RemoveKeys(m_Mobile);
						_ = new TempNoHousingRegion(m_House, m_Mobile);
						m_House.Delete();
					}
					else
					{
						Item toGive;
						if (m_House.IsAosRules)
						{
							if (m_House.Price > 0)
							{
								toGive = new BankCheck(m_House.Price);
							}
							else
							{
								toGive = m_House.GetDeed();
							}
						}
						else
						{
							toGive = m_House.GetDeed();

							if (toGive == null && m_House.Price > 0)
							{
								toGive = new BankCheck(m_House.Price);
							}
						}

						if (toGive != null)
						{
							var box = m_Mobile.BankBox;

							if (box.TryDropItem(m_Mobile, toGive, false))
							{
								if (toGive is BankCheck)
								{
									m_Mobile.SendLocalizedMessage(1060397, ((BankCheck)toGive).Worth.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
								}

								m_House.RemoveKeys(m_Mobile);
								_ = new TempNoHousingRegion(m_House, m_Mobile);
								m_House.Delete();
							}
							else
							{
								toGive.Delete();
								m_Mobile.SendLocalizedMessage(500390); // Your bank box is full.
							}
						}
						else
						{
							m_Mobile.SendMessage("Unable to refund house.");
						}
					}
				}
				else
				{
					m_Mobile.SendLocalizedMessage(501320); // Only the house owner may do this.
				}
			}
			else if (info.ButtonID == 0)
			{
				_ = m_Mobile.CloseGump(typeof(ConfirmHouseResize));
				_ = m_Mobile.SendGump(new HouseGumpAOS(HouseGumpPageAOS.Customize, m_Mobile, m_House));
			}
		}
	}

	public class HouseTransferGump : Gump
	{
		private readonly Mobile m_From, m_To;
		private readonly BaseHouse m_House;

		public HouseTransferGump(Mobile from, Mobile to, BaseHouse house) : base(110, 100)
		{
			m_From = from;
			m_To = to;
			m_House = house;

			Closable = false;

			AddPage(0);

			AddBackground(0, 0, 420, 280, 5054);

			AddImageTiled(10, 10, 400, 20, 2624);
			AddAlphaRegion(10, 10, 400, 20);

			AddHtmlLocalized(10, 10, 400, 20, 1060635, 30720, false, false); // <CENTER>WARNING</CENTER>

			AddImageTiled(10, 40, 400, 200, 2624);
			AddAlphaRegion(10, 40, 400, 200);

			/* Another player is attempting to initiate a house trade with you.
			 * In order for you to see this window, both you and the other person are standing within two paces of the house to be traded.
			 * If you click OKAY below, a house trade scroll will appear in your trade window and you can complete the transaction.
			 * This scroll is a distinctive blue color and will show the name of the house, the name of the owner of that house, and the sextant coordinates of the center of the house when you hover your mouse over it.
			 * In order for the transaction to be successful, you both must accept the trade and you both must remain within two paces of the house sign.
			 * <BR><BR>Accepting this house in trade will <a href = "?ForceTopic97">condemn</a> any and all of your other houses that you may have.
			 * All of your houses on <U>all shards</U> will be affected.
			 * <BR><BR>In addition, you will not be able to place another house or have one transferred to you for one (1) real-life week.<BR><BR>
			 * Once you accept these terms, these effects cannot be reversed.
			 * Re-deeding or transferring your new house will <U>not</U> uncondemn your other house(s) nor will the one week timer be removed.<BR><BR>
			 * If you are absolutely certain you wish to proceed, click the button next to OKAY below.
			 * If you do not wish to trade for this house, click CANCEL.
			 */
			AddHtmlLocalized(10, 40, 400, 200, 1062086, 32512, false, true);

			AddImageTiled(10, 250, 400, 20, 2624);
			AddAlphaRegion(10, 250, 400, 20);

			AddButton(10, 250, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(40, 250, 170, 20, 1011036, 32767, false, false); // OKAY

			AddButton(210, 250, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(240, 250, 170, 20, 1011012, 32767, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info.ButtonID == 1 && !m_House.Deleted)
			{
				m_House.EndConfirmTransfer(m_From, m_To);
			}
		}
	}

	public class HouseRemoveGump : Gump
	{
		private readonly BaseHouse m_House;
		private readonly ICollection<Mobile> m_List;
		private readonly List<Mobile> m_Copy;
		private readonly int m_Number;
		private readonly bool m_AccountOf;

		public HouseRemoveGump(int number, ICollection<Mobile> list, BaseHouse house, bool accountOf) : base(20, 30)
		{
			if (house.Deleted)
			{
				return;
			}

			m_House = house;
			m_List = list;
			m_Number = number;
			m_AccountOf = accountOf;

			AddPage(0);

			AddBackground(0, 0, 420, 430, 5054);
			AddBackground(10, 10, 400, 410, 3000);

			AddButton(20, 388, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 388, 300, 20, 1011104, false, false); // Return to previous menu

			AddButton(20, 365, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(55, 365, 300, 20, 1011270, false, false); // Remove now!

			AddHtmlLocalized(20, 20, 350, 20, number, false, false);

			if (list != null)
			{
				m_Copy = new(list);

				var i = -1;

				foreach (var m in list)
				{
					++i;

					if ((i % 15) == 0)
					{
						if (i != 0)
						{
							// Next button
							AddButton(370, 20, 4005, 4007, 0, GumpButtonType.Page, (i / 15) + 1);
						}

						AddPage((i / 15) + 1);

						if (i != 0)
						{
							// Previous button
							AddButton(340, 20, 4014, 4016, 0, GumpButtonType.Page, i / 15);
						}
					}

					string name;

					if (m == null || (name = m.Name) == null || (name = name.Trim()).Length <= 0)
					{
						continue;
					}

					AddCheck(34, 52 + (i % 15 * 20), 0xD2, 0xD3, false, i);
					AddLabel(55, 52 + (i % 15 * 20), 0, accountOf && m.Player && m.Account != null ? String.Format("Account of {0}", name) : name);
				}
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (m_House.Deleted)
			{
				return;
			}

			var from = state.Mobile;

			if (m_List != null && info.ButtonID == 1) // Remove now
			{
				var switches = info.Switches;

				if (switches.Length > 0)
				{
					for (var i = 0; i < switches.Length; ++i)
					{
						var index = switches[i];

						if (index >= 0 && index < m_Copy.Count)
						{
							_ = m_List.Remove(m_Copy[index]);
						}
					}

					if (m_List.Count > 0)
					{
						_ = from.CloseGump(typeof(HouseGump));
						_ = from.CloseGump(typeof(HouseListGump));
						_ = from.CloseGump(typeof(HouseRemoveGump));
						_ = from.SendGump(new HouseRemoveGump(m_Number, m_List, m_House, m_AccountOf));
						return;
					}
				}
			}

			_ = from.SendGump(new HouseGump(from, m_House));
		}
	}

	public class HouseDemolishGump : Gump
	{
		private readonly Mobile m_Mobile;
		private readonly BaseHouse m_House;

		public HouseDemolishGump(Mobile mobile, BaseHouse house) : base(110, 100)
		{
			m_Mobile = mobile;
			m_House = house;

			_ = mobile.CloseGump(typeof(HouseDemolishGump));

			Closable = false;

			AddPage(0);

			AddBackground(0, 0, 420, 280, 5054);

			AddImageTiled(10, 10, 400, 20, 2624);
			AddAlphaRegion(10, 10, 400, 20);

			AddHtmlLocalized(10, 10, 400, 20, 1060635, 30720, false, false); // <CENTER>WARNING</CENTER>

			AddImageTiled(10, 40, 400, 200, 2624);
			AddAlphaRegion(10, 40, 400, 200);

			AddHtmlLocalized(10, 40, 400, 200, 1061795, 32512, false, true); /* You are about to demolish your house.
																				* You will be refunded the house's value directly to your bank box.
																				* All items in the house will remain behind and can be freely picked up by anyone.
																				* Once the house is demolished, anyone can attempt to place a new house on the vacant land.
																				* This action will not un-condemn any other houses on your account, nor will it end your 7-day waiting period (if it applies to you).
																				* Are you sure you wish to continue?
																				*/

			AddImageTiled(10, 250, 400, 20, 2624);
			AddAlphaRegion(10, 250, 400, 20);

			AddButton(10, 250, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(40, 250, 170, 20, 1011036, 32767, false, false); // OKAY

			AddButton(210, 250, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(240, 250, 170, 20, 1011012, 32767, false, false); // CANCEL
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info.ButtonID == 1 && !m_House.Deleted)
			{
				if (m_House.IsOwner(m_Mobile))
				{
					if (m_House.MovingCrate != null || m_House.InternalizedVendors.Count > 0)
					{
						return;
					}
					else if (!Guilds.Guild.NewGuildSystem && m_House.FindGuildstone() != null)
					{
						m_Mobile.SendLocalizedMessage(501389); // You cannot redeed a house with a guildstone inside.
						return;
					}
					/*else if ( m_House.PlayerVendors.Count > 0 )
					{
						m_Mobile.SendLocalizedMessage( 503236 ); // You need to collect your vendor's belongings before moving.
						return;
					}*/
					else if (m_House.HasRentedVendors && m_House.VendorInventories.Count > 0)
					{
						m_Mobile.SendLocalizedMessage(1062679); // You cannot do that that while you still have contract vendors or unclaimed contract vendor inventory in your house.
						return;
					}
					else if (m_House.HasRentedVendors)
					{
						m_Mobile.SendLocalizedMessage(1062680); // You cannot do that that while you still have contract vendors in your house.
						return;
					}
					else if (m_House.VendorInventories.Count > 0)
					{
						m_Mobile.SendLocalizedMessage(1062681); // You cannot do that that while you still have unclaimed contract vendor inventory in your house.
						return;
					}

					if (m_Mobile.AccessLevel >= AccessLevel.GameMaster)
					{
						m_Mobile.SendMessage("You do not get a refund for your house as you are not a player");
						m_House.RemoveKeys(m_Mobile);
						m_House.Delete();
					}
					else
					{
						Item toGive;
						if (m_House.IsAosRules)
						{
							if (m_House.Price > 0)
							{
								toGive = new BankCheck(m_House.Price);
							}
							else
							{
								toGive = m_House.GetDeed();
							}
						}
						else
						{
							toGive = m_House.GetDeed();

							if (toGive == null && m_House.Price > 0)
							{
								toGive = new BankCheck(m_House.Price);
							}
						}

						if (AccountGold.Enabled && toGive is BankCheck)
						{
							var worth = ((BankCheck)toGive).Worth;

							if (m_Mobile.Account != null && m_Mobile.Account.DepositGold(worth))
							{
								toGive.Delete();

								m_Mobile.SendLocalizedMessage(1060397, worth.ToString("#,0"));
								// ~1_AMOUNT~ gold has been deposited into your bank box.

								m_House.RemoveKeys(m_Mobile);
								m_House.Delete();
								return;
							}
						}

						if (toGive != null)
						{
							var box = m_Mobile.BankBox;

							if (box.TryDropItem(m_Mobile, toGive, false))
							{
								if (toGive is BankCheck)
								{
									m_Mobile.SendLocalizedMessage(1060397, ((BankCheck)toGive).Worth.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.
								}

								m_House.RemoveKeys(m_Mobile);
								m_House.Delete();
							}
							else
							{
								toGive.Delete();
								m_Mobile.SendLocalizedMessage(500390); // Your bank box is full.
							}
						}
						else
						{
							m_Mobile.SendMessage("Unable to refund house.");
						}
					}
				}
				else
				{
					m_Mobile.SendLocalizedMessage(501320); // Only the house owner may do this.
				}
			}
		}
	}

	// IDOC House Placement Delay
	public class TempNoHousingRegion : BaseRegion
	{
		private Mobile m_RegionOwner;
		private DateTime m_Expires;
		private Timer m_Timer;

		public TempNoHousingRegion(BaseHouse house, Mobile regionowner)
			: base(null, house.Map, DefaultPriority, house.Region.Area)
		{
			Register();

			m_RegionOwner = regionowner;

			m_Expires = DateTime.UtcNow.Add(house.RestrictedPlacingTime);
			m_Timer = Timer.DelayCall(house.RestrictedPlacingTime, Delete);
		}

		public TempNoHousingRegion(int id) : base(id)
		{
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			Rules.AllowHouses = false;
			Rules.AllowVehicles = false;
		}

		public override bool AllowHousing(Mobile from, Point3D p)
		{
			return from == m_RegionOwner || AccountHandler.CheckAccount(from, m_RegionOwner) || base.AllowHousing(from, p);
		}

		protected override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_Timer != null)
			{
				m_Timer.Stop();
				m_Timer = null;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(m_RegionOwner);
			writer.Write(m_Expires);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadInt();

			m_RegionOwner = reader.ReadMobile();
			m_Expires = reader.ReadDateTime();

			m_Timer?.Stop();
			m_Timer = Timer.DelayCall(TimeSpan.FromTicks(Math.Max(0, (m_Expires - DateTime.UtcNow).Ticks)), Delete);
		}
	}
}

namespace Server.Prompts
{
	// House Rename
	public class RenamePrompt : Prompt
	{
		private readonly BaseHouse m_House;

		public RenamePrompt(BaseHouse house)
		{
			m_House = house;
		}

		public override void OnResponse(Mobile from, string text)
		{
			if (m_House.IsFriend(from))
			{
				if (m_House.Sign != null)
				{
					m_House.Sign.Name = text;
				}

				from.SendMessage("Sign changed.");
			}
		}
	}
}