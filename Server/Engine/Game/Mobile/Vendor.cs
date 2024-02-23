using System;
using System.Collections.Generic;

using Server.Mobiles;

namespace Server
{
	public interface IVendor
	{
		bool OnBuyItems(Mobile from, List<BuyItemResponse> list);
		bool OnSellItems(Mobile from, List<SellItemResponse> list);

		[CommandProperty(AccessLevel.GameMaster)]
		DateTime LastRestock { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		TimeSpan RestockDelay { get; }

		void Restock();
	}
}

namespace Server.Mobiles
{
	public class BuyItemStateComparer : IComparer<BuyItemState>
	{
		public static BuyItemStateComparer Instance { get; } = new();

		private BuyItemStateComparer()
		{
		}

		public int Compare(BuyItemState l, BuyItemState r)
		{
			if (l == null && r == null)
			{
				return 0;
			}

			if (l == null)
			{
				return -1;
			}

			if (r == null)
			{
				return 1;
			}

			return l.MySerial.CompareTo(r.MySerial);
		}
	}

	public class BuyItemResponse
	{
		public Serial Serial { get; }
		public int Amount { get; }

		public BuyItemResponse(Serial serial, int amount)
		{
			Serial = serial;
			Amount = amount;
		}
	}

	public class BuyItemState
	{
		public string Description { get; }

		public Serial ContainerSerial { get; }
		public Serial MySerial { get; }

		public int Price { get; }
		public int Amount { get; }

		public int ItemID { get; }
		public int Hue { get; }

		public BuyItemState(string name, Serial cont, Serial serial, int price, int amount, int itemID, int hue)
		{
			Description = name;
			ContainerSerial = cont;
			MySerial = serial;
			Price = price;
			Amount = amount;
			ItemID = itemID;
			Hue = hue;
		}
	}

	public class SellItemResponse
	{
		public Item Item { get; }
		public int Amount { get; }

		public SellItemResponse(Item i, int amount)
		{
			Item = i;
			Amount = amount;
		}
	}

	public class SellItemState
	{
		public Item Item { get; }
		public int Price { get; }
		public string Name { get; }

		public SellItemState(Item item, int price, string name)
		{
			Item = item;
			Price = price;
			Name = name;
		}
	}
}