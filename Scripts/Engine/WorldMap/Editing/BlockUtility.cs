using System;

namespace Server.Engine.Facet
{
	internal class BlockUtility
	{
		public static byte[] GetLandData(int blockNumber, int mapNumber)
		{
			var m = Map.Maps[mapNumber];
			var tm = m.Tiles;

			return GetLandData(new Point2D(blockNumber / tm.BlockHeight, blockNumber % tm.BlockHeight), mapNumber);
		}

		public static byte[] GetLandData(Point2D blockCoordinates, int mapNumber)
		{
			var landData = new byte[192];

			var map = Map.Maps[mapNumber];
			var tm = map.Tiles;
			var land = tm.GetLandBlock(blockCoordinates.X, blockCoordinates.Y);

			for (var i = 0; i < land.Length; i++) //64 * 3 = 192 bytes
			{
				landData[i * 3] = (byte)(land[i].ID & 0x00FF);
				landData[(i * 3) + 1] = (byte)((land[i].ID & 0xFF00) >> 8);
				landData[(i * 3) + 2] = (byte)land[i].Z;
			}

			return landData;
		}

		public static void WriteLandDataToConsole(byte[] landData)
		{
			var outString = "Land Data:\n";

			for (var i = 0; i < landData.Length; i += 3)
			{
				if (i % 12 == 0 && i != 0)
				{
					outString += "\n";
				}

				outString += $"[{landData[i]:X2},{landData[i + 1]:X2},{landData[i + 2]:X2}]";
			}

			Console.WriteLine(outString);
		}

		public static void WriteStaticsDataToConsole(byte[] staticsData)
		{
			var outString = $"Statics Data ({staticsData.Length}):\n";

			for (var i = 0; i < staticsData.Length; i += 7)
			{
				if (i % 14 == 0 && i != 0)
				{
					outString += "\n";
				}

				outString += $"[{staticsData[i]:X2},{staticsData[i + 1]:X2},{staticsData[i + 2]:X2},{staticsData[i + 3]:X2},{staticsData[i + 4]:X2},{staticsData[i + 5]:X2},{staticsData[i + 6]:X2}]";
			}

			Console.WriteLine(outString);
		}

		public static void WriteDataToConsole(byte[] anyData)
		{
			var outString = $"Data ({anyData.Length}):\n";

			for (var i = 0; i < anyData.Length; i += 8)
			{
				if (i + 7 < anyData.Length)
				{
					if (i % 16 == 0 && i != 0)
					{
						outString += "\n";
					}

					outString += $"[{anyData[i]:X2},{anyData[i + 1]:X2},{anyData[i + 2]:X2},{anyData[i + 3]:X2},{anyData[i + 4]:X2},{anyData[i + 5]:X2},{anyData[i + 6]:X2},{anyData[i + 7]:X2}]";
				}
				else
				{
					outString += "\n[";

					for (var j = i; j < anyData.Length; j++)
					{
						outString += $"{anyData[j]:X2},";
					}

					outString += "]";
				}
			}

			Console.WriteLine(outString);
		}

		public static void WriteBlockDataToConsole(byte[] anyData)
		{
			Console.WriteLine($"Block Data ({anyData.Length}):\n");

			if (anyData.Length >= 192)
			{
				var landData = new byte[192];
				Array.Copy(anyData, 0, landData, 0, 192);

				//WriteLandDataToConsole(landData);
				if ((anyData.Length - 192) % 7 == 0)
				{
					var staticsData = new byte[anyData.Length - 192];
					Array.Copy(anyData, 192, staticsData, 0, anyData.Length - 192);

					WriteStaticsDataToConsole(staticsData);
				}
			}
		}

		public static byte[] GetRawStaticsData(int blockNumber, int mapNumber)
		{
			var m = Map.Maps[mapNumber];
			var tm = m.Tiles;

			return GetRawStaticsData(new Point2D(blockNumber / tm.BlockHeight, blockNumber % tm.BlockHeight), mapNumber);
		}

		public static byte[] GetRawStaticsData(Point2D blockCoordinates, int mapNumber)
		{
			var map = Map.Maps[mapNumber];
			var tm = map.Tiles;
			var staticTiles = tm.GetStaticBlock(blockCoordinates.X, blockCoordinates.Y);

			var staticCount = 0;

			for (var k = 0; k < staticTiles.Length; k++)
			{
				for (var l = 0; l < staticTiles[k].Length; l++)
				{
					staticCount += staticTiles[k][l].Length;
				}
			}

			var blockBytes = new byte[staticCount * 7];
			var blockByteIdx = 0;

			for (var i = 0; i < staticTiles.Length; i++)
			{
				for (var j = 0; j < staticTiles[i].Length; j++)
				{
					var sortedTiles = staticTiles[i][j];

					//Array.Sort(sortedTiles, CompareStaticTiles);
					for (var k = 0; k < sortedTiles.Length; k++)
					{
						blockBytes[blockByteIdx + 0] = (byte)(sortedTiles[k].ID & 0x00FF);
						blockBytes[blockByteIdx + 1] = (byte)((sortedTiles[k].ID & 0xFF00) >> 8);
						blockBytes[blockByteIdx + 2] = (byte)i;
						blockBytes[blockByteIdx + 3] = (byte)j;
						blockBytes[blockByteIdx + 4] = (byte)sortedTiles[k].Z;
						blockBytes[blockByteIdx + 5] = (byte)(sortedTiles[k].Hue & 0x00FF);
						blockBytes[blockByteIdx + 6] = (byte)((sortedTiles[k].Hue & 0xFF00) >> 8);
						blockByteIdx += 7;
					}
				}
			}

			return blockBytes;
		}

		public static int CompareStaticTiles(StaticTile b, StaticTile a)
		{
			var retVal = a.Z.CompareTo(b.Z);

			if (retVal == 0)//same Z, lower z has higher priority now, it's correct this way, tested locally
			{
				var sts = FacetEditingCommands.WorkMap.Tiles.GetStaticTiles(a.X, a.Y);

				for (var i = 0; i < sts.Length; i++)
				{
					//we compare hashcodes for easyness, instead of comparing a bunch of properties, order has been verified to work in exportclientfiles.
					var hash = sts[i].GetHashCode();
					if (hash == a.GetHashCode())
					{
						retVal = 1;
						break;
					}
					else if (hash == b.GetHashCode())
					{
						retVal = -1;
						break;
					}
				}
			}

			//We leave this as is, but it shouldn't happen anyway if we have same Z
			if (retVal == 0)
			{
				retVal = a.ID.CompareTo(b.ID);
			}

			if (retVal == 0)
			{
				retVal = a.Hue.CompareTo(b.Hue);
			}

			return retVal;
		}
	}
}