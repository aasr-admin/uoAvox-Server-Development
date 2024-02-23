using Server.Items;
using Server.Multis;

using System;
using System.Collections.Generic;

namespace Server.Misc
{
	public class Cleanup
	{
		public static void Initialize()
		{
			_ = Timer.DelayCall(TimeSpan.FromSeconds(2.5), Run);
		}

		public static void Run()
		{
			var items = new HashSet<Item>();
			var validItems = new HashSet<Item>();
			var hairCleanup = new HashSet<Mobile>();

			var boxes = 0;

			foreach (var item in World.Items.Values)
			{
				if (item.Map == null)
				{
					_ = items.Add(item);
					continue;
				}

				if (item is CommodityDeed deed)
				{
					if (deed.Commodity != null)
					{
						_ = validItems.Add(deed.Commodity);
					}

					continue;
				}

				if (item is BaseHouse house)
				{
					foreach (var relEntity in house.RelocatedEntities)
					{
						if (relEntity.Entity is Item relItem)
						{
							_ = validItems.Add(relItem);
						}
					}

					foreach (var inventory in house.VendorInventories)
					{
						foreach (var subItem in inventory.Items)
						{
							_ = validItems.Add(subItem);
						}
					}

					continue;
				}

				if (item is BankBox box)
				{
					if (box.Owner == null || box.Items.Count == 0)
					{
						if (items.Add(box))
						{
							++boxes;
						}
					}

					continue;
				}

				if (item.Layer is Layer.Hair or Layer.FacialHair)
				{
					if (item.RootParent is Mobile rootMobile)
					{
						if (item.Parent != rootMobile && rootMobile.AccessLevel == AccessLevel.Player)
						{
							_ = items.Add(item);
							continue;
						}

						if (item.Parent == rootMobile)
						{
							_ = hairCleanup.Add(rootMobile);
							continue;
						}
					}
				}

				if (item.Parent != null || item.Map != Map.Internal || item.HeldBy != null || item.Location != Point3D.Zero)
				{
					continue;
				}

				if (IsBuggable(item))
				{
					_ = items.Add(item);
				}
			}

			foreach (var validItem in validItems)
			{
				_ = items.Remove(validItem);
			}

			if (items.Count > 0)
			{
				if (boxes > 0)
				{
					Console.WriteLine("Cleanup: Detected {0} inaccessible items, including {1} bank boxes, removing..", items.Count, boxes);
				}
				else
				{
					Console.WriteLine("Cleanup: Detected {0} inaccessible items, removing..", items.Count);
				}

				foreach (var item in items)
				{
					item.Delete();
				}
			}

			if (hairCleanup.Count > 0)
			{
				Console.WriteLine("Cleanup: Detected {0} hair and facial hair items being worn, converting to their virtual counterparts..", hairCleanup.Count);

				foreach (var mob in hairCleanup)
				{
					mob.ConvertHair();
				}
			}
		}

		public static HashSet<Type> BuggableTypes { get; } = new();

		[CallPriority(Int32.MinValue + 1)]
		public static void Configure()
		{
			SetBuggable<ICommodity>();

			SetBuggable<SaltwaterFish>();
			SetBuggable<BigFish>();
			SetBuggable<BasePotion>();
			SetBuggable<Food>();
			SetBuggable<CookableFood>();
			SetBuggable<SpecialFishingNet>();
			SetBuggable<BaseMagicFish>();
			SetBuggable<Shoes>();
			SetBuggable<Sandals>();
			SetBuggable<Boots>();
			SetBuggable<ThighBoots>();
			SetBuggable<TreasureMap>();
			SetBuggable<MessageInABottle>();
			//SetBuggable<BaseBoat>();

			SetBuggable<BaseArmor>();
			SetBuggable<BaseWeapon>();
			SetBuggable<BaseClothing>();

			if (Core.AOS)
			{
				SetBuggable<BaseJewel>();
			}

			if (Core.ML)
			{
				SetBuggable<BasePotion>();
			}

			#region Champion Artifacts

			SetBuggable<SkullPole>();
			SetBuggable<EvilIdolSkull>();
			SetBuggable<MonsterStatuette>();
			SetBuggable<Pier>();
			SetBuggable<ArtifactLargeVase>();
			SetBuggable<ArtifactVase>();
			SetBuggable<MinotaurStatueDeed>();
			SetBuggable<SwampTile>();
			SetBuggable<WallBlood>();
			SetBuggable<TatteredAncientMummyWrapping>();
			SetBuggable<LavaTile>();
			SetBuggable<DemonSkull>();
			SetBuggable<Web>();
			SetBuggable<WaterTile>();
			SetBuggable<WindSpirit>();
			SetBuggable<DirtPatch>();
			SetBuggable<Futon>();

			#endregion
		}

		public static void SetBuggable<T>()
		{
			_ = BuggableTypes.Add(typeof(T));
		}

		public static bool IsBuggable(Item item)
		{
			if (item is Fists)
			{
				return false;
			}

			var type = item.GetType();

			if (BuggableTypes.Contains(type))
			{
				return true;
			}

			foreach (var t in BuggableTypes)
			{
				if (type.IsAssignableTo(t))
				{
					return true;
				}
			}

			return false;
		}
	}
}