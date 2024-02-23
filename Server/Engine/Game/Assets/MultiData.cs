
using Server.Network;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server
{
	public static class MultiData
	{
		public static Dictionary<int, MultiComponentList> Components { get; } = new();

		public static bool UsingUOPFormat { get; private set; }

		public static bool PostHSFormat { get; private set; }

		static MultiData()
		{
			try
			{
				if (LoadUOP() || LoadMUL())
				{
					Utility.PushColor(ConsoleColor.Green);
					Console.WriteLine($"MultiData: loaded ({Components.Count} entries)");
					Utility.PopColor();

					if (LoadVerdata())
					{
						Utility.PushColor(ConsoleColor.Blue);
						Console.WriteLine("MultiData: patched using verdata.mul");
						Utility.PopColor();
					}

					return;
				}
			}
			catch (Exception e)
			{
				if (Core.Debug)
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("MultiData: fatal exception");
					Console.WriteLine(e);
					Utility.PopColor();
				}
			}

			Utility.PushColor(ConsoleColor.Yellow);
			Console.WriteLine("MultiData: entries could not be loaded");
			Utility.PopColor();
		}

		private static bool LoadUOP()
		{
			var multicollectionPath = Core.FindDataFile("MultiCollection.uop");

			if (!File.Exists(multicollectionPath))
			{
				return false;
			}

			using var stream = new FileStream(multicollectionPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var streamReader = new BinaryReader(stream);

			// Head Information Start
			if (streamReader.ReadInt32() != 0x0050594D) // Not a UOP Files
			{
				return false;
			}

			if (streamReader.ReadInt32() > 5) // Bad Version
			{
				return false;
			}

			UsingUOPFormat = true;

			var chunkIds = new Dictionary<ulong, int>(0x10000);

			for (var i = 0; i < 0x10000; ++i)
			{
				chunkIds[UOPHash.Compute($"build/multicollection/{i:D6}.bin")] = i;
			}

			_ = streamReader.ReadUInt32(); // format timestamp? 0xFD23EC43

			var startAddress = streamReader.ReadInt64();

			_ = streamReader.ReadInt32(); // files in each block
			_ = streamReader.ReadInt32(); // Total File Count

			_ = stream.Seek(startAddress, SeekOrigin.Begin); // Head Information End

			long nextBlock;

			do
			{
				var blockFileCount = streamReader.ReadInt32();

				nextBlock = streamReader.ReadInt64();

				var index = 0;

				do
				{
					var offset = streamReader.ReadInt64();

					var headerSize = streamReader.ReadInt32(); // header length
					var compressedSize = streamReader.ReadInt32(); // compressed size
					var decompressedSize = streamReader.ReadInt32(); // decompressed size

					var filehash = streamReader.ReadUInt64(); // filename hash (HashLittle2)

					_ = streamReader.ReadUInt32(); // data hash (Adler32)

					var cmp = streamReader.ReadInt16(); // compression method (0 = none, 1 = zlib)

					index++;

					if (offset == 0 || decompressedSize == 0 || filehash == 0x126D1E99DDEDEE0A) // Exclude housing.bin
					{
						continue;
					}

					if (!chunkIds.TryGetValue(filehash, out var chunkID))
					{
						continue;
					}

					var positionpoint = stream.Position;  // save current position

					// Decompress Data Start
					_ = stream.Seek(offset + headerSize, SeekOrigin.Begin);

					var sourceData = new byte[compressedSize];

					if (stream.Read(sourceData, 0, compressedSize) != compressedSize)
					{
						continue;
					}

					byte[] data;

					if (cmp == 1)
					{
						var destData = new byte[decompressedSize];

						_ = Compression.Compressor.Decompress(destData, ref decompressedSize, sourceData, compressedSize);

						data = destData;
					}
					else
					{
						data = sourceData;
					}
					// End Decompress Data

					var tileList = new List<MultiTileEntry>();

					using var fs = new MemoryStream(data);
					using var reader = new BinaryReader(fs);

					var a = reader.ReadUInt32();
					var count = reader.ReadUInt32();

					for (uint i = 0; i < count; i++)
					{
						var id = reader.ReadUInt16();
						var x = reader.ReadInt16();
						var y = reader.ReadInt16();
						var z = reader.ReadInt16();

						var flag = reader.ReadUInt16();

						var clilocsCount = reader.ReadUInt32();

						if (clilocsCount != 0)
						{
							_ = fs.Seek(fs.Position + (clilocsCount * 4), SeekOrigin.Begin); // binary block bypass
						}

						tileList.Add(new MultiTileEntry(id, x, y, z, flag switch
						{
							0x001 => TileFlag.None,
							0x101 => TileFlag.Generic,
							_ => TileFlag.Background,
						}));
					}

					reader.Close();

					Components[chunkID] = new MultiComponentList(tileList);

					_ = stream.Seek(positionpoint, SeekOrigin.Begin); // back to position
				}
				while (index < blockFileCount);
			}
			while (stream.Seek(nextBlock, SeekOrigin.Begin) != 0);

			chunkIds.Clear();

			return true;
		}

		private static bool LoadMUL()
		{
			var idxPath = Core.FindDataFile("multi.idx");

			if (!File.Exists(idxPath))
			{
				return false;
			}

			var mulPath = Core.FindDataFile("multi.mul");

			if (!File.Exists(mulPath))
			{
				return false;
			}

			using var idx = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var idxReader = new BinaryReader(idx);

			using var mul = new FileStream(mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var mulReader = new BinaryReader(mul);

			PostHSFormat = idx.Length % 16 == 0;

			var count = idx.Length / 12;
			var size = PostHSFormat ? 16 : 12;

			for (var i = 0; i < count; i++)
			{
				var lookup = idxReader.ReadInt32();
				var length = idxReader.ReadInt32();

				_ = idxReader.ReadInt32();

				if (lookup >= 0 && length > 0)
				{
					mul.Seek(lookup, SeekOrigin.Begin);

					var entries = ReadEntries(mulReader, length / 12);

					Components[i] = new MultiComponentList(entries);
				}
			}

			return true;
		}

		private static bool LoadVerdata()
		{
			var vdPath = Core.FindDataFile("verdata.mul");

			if (!File.Exists(vdPath))
			{
				return false;
			}

			using var fs = new FileStream(vdPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var bin = new BinaryReader(fs);

			var count = bin.ReadInt32();

			for (var i = 0; i < count; ++i)
			{
				var file = bin.ReadInt32();
				var index = bin.ReadInt32();
				var lookup = bin.ReadInt32();
				var length = bin.ReadInt32();

				_ = bin.ReadInt32();

				if (file == 14 && index >= 0 && lookup >= 0 && length > 0)
				{
					_ = bin.BaseStream.Seek(lookup, SeekOrigin.Begin);

					var entries = ReadEntries(bin, length / 12);

					Components[index] = new MultiComponentList(entries);

					_ = bin.BaseStream.Seek(24 + (i * 20), SeekOrigin.Begin);
				}
			}

			bin.Close();

			return true;
		}

		private static IEnumerable<MultiTileEntry> ReadEntries(BinaryReader reader, int count)
		{
			while (--count >= 0)
			{
				yield return new MultiTileEntry(reader);
			}
		}

		public static MultiComponentList GetComponents(int multiID)
		{
			multiID &= 0x3FFF; // The value of the actual multi is shifted by 0x4000, so this is left alone.

			Components.TryGetValue(multiID, out var mcl);

			return mcl ?? MultiComponentList.Empty;
		}
	}

	public struct MultiTileEntry
	{
		public ushort ItemID;
		public short OffsetX, OffsetY, OffsetZ;
		public TileFlag Flags;

		public readonly int Height => TileData.ItemTable[ItemID & TileData.MaxItemValue].Height;

		public MultiTileEntry(ushort itemID, short xOffset, short yOffset, short zOffset, TileFlag flags)
		{
			ItemID = itemID;
			OffsetX = xOffset;
			OffsetY = yOffset;
			OffsetZ = zOffset;
			Flags = flags;
		}

		internal MultiTileEntry(BinaryReader reader)
		{
			ItemID = reader.ReadUInt16();
			OffsetX = reader.ReadInt16();
			OffsetY = reader.ReadInt16();
			OffsetZ = reader.ReadInt16();

			if (MultiData.PostHSFormat)
			{
				Flags = (TileFlag)reader.ReadUInt64();
			}
			else
			{
				Flags = (TileFlag)reader.ReadUInt32();
			}
		}

		public readonly void Serialize(GenericWriter writer)
		{
			writer.Write(0);

			writer.Write(ItemID);
			writer.Write(OffsetX);
			writer.Write(OffsetY);
			writer.Write(OffsetZ);
			writer.Write(Flags);
		}

		public void Deserialize(GenericReader reader)
		{
			_ = reader.ReadInt();

			ItemID = reader.ReadUShort();
			OffsetX = reader.ReadShort();
			OffsetY = reader.ReadShort();
			OffsetZ = reader.ReadShort();
			Flags = reader.ReadEnum<TileFlag>();
		}

		public override readonly string ToString()
		{
			return $"({ItemID}, {OffsetX}, {OffsetY}, {OffsetZ}, 0x{(ulong)Flags:X8})";
		}
	}

	public sealed class MultiComponentList
	{
		public static readonly MultiComponentList Empty = new();

		public static bool PostHSFormat => MultiData.PostHSFormat;

		private Point3D m_Min, m_Max, m_Center;

		public Point3D Min => m_Min;
		public Point3D Max => m_Max;

		public Point3D Center => m_Center;

		public int Width => m_Max.m_X - m_Min.m_X + 1;
		public int Height => m_Max.m_Y - m_Min.m_Y + 1;
		public int Depth => m_Max.m_Z - m_Min.m_Z + 1;

		public StaticTile[][][] Tiles { get; private set; }
		public MultiTileEntry[] List { get; private set; }

		private MultiComponentList()
		{
			List = new[]
			{
				new MultiTileEntry(0, 0, 0, 0, TileFlag.Background)
			};

			InvalidateTiles(false);
		}

		public MultiComponentList(IEnumerable<MultiTileEntry> list)
		{
			List = list?.ToArray();

			if (List?.Length > 0)
			{
				InvalidateTiles(true);
			}
			else
			{
				List = new[]
				{
					new MultiTileEntry(0, 0, 0, 0, 0)
				};

				InvalidateTiles(false);
			}
		}

		public MultiComponentList(MultiComponentList toCopy)
			: this(toCopy?.List)
		{
		}

		public MultiComponentList(GenericReader reader)
		{
			Deserialize(reader);
		}

		private void ResolveDimensions(int x, int y, int z, int h)
		{
			m_Min.m_X = Math.Min(m_Min.m_X, x);
			m_Min.m_Y = Math.Min(m_Min.m_Y, y);
			m_Min.m_Z = Math.Min(m_Min.m_Z, z);

			m_Max.m_X = Math.Max(m_Max.m_X, x);
			m_Max.m_Y = Math.Max(m_Max.m_Y, y);
			m_Max.m_Z = Math.Max(m_Max.m_Z, z + h);
		}

		private void ResolveDimensions(MultiTileEntry e)
		{
			ResolveDimensions(e.OffsetX, e.OffsetY, e.OffsetZ, e.Height);
		}

		private void InvalidateDimensions()
		{
			m_Min = m_Max = m_Center = Point3D.Zero;

			foreach (var e in List)
			{
				ResolveDimensions(e);
			}

			m_Center.m_X = -m_Min.m_X;// + ((Width - 1) / 2);
			m_Center.m_Y = -m_Min.m_Y;// + ((Height - 1) / 2);
			m_Center.m_Z = -m_Min.m_Z;// + ((Depth - 1) / 2);
		}

		private void InvalidateTiles(bool resize)
		{
			if (resize)
			{
				InvalidateDimensions();
			}

			var width = Width;
			var height = Height;

			var tiles = new TileList[width][];

			Tiles = new StaticTile[width][][];

			for (var x = 0; x < width; x++)
			{
				tiles[x] = new TileList[height];
				Tiles[x] = new StaticTile[height][];

				for (var y = 0; y < height; y++)
				{
					tiles[x][y] = new TileList();
				}
			}

			var i = -1;

			while (++i < List.Length)
			{
				var e = List[i];

				if (e.Flags != 0)
				{
					var xOffset = e.OffsetX + m_Center.m_X;
					var yOffset = e.OffsetY + m_Center.m_Y;
					var zOffset = e.OffsetZ + m_Center.m_Z;

					tiles[xOffset][yOffset].Add(e.ItemID, (byte)xOffset, (byte)yOffset, (sbyte)zOffset);
				}
			}

			for (var x = 0; x < width; x++)
			{
				for (var y = 0; y < height; y++)
				{
					Tiles[x][y] = tiles[x][y].ToArray();
				}
			}
		}

		public void Resize(int newWidth, int newHeight)
		{
			int oldWidth = Width, oldHeight = Height;
			var oldTiles = Tiles;

			if (newWidth < oldWidth || newHeight < oldHeight)
			{
				List = List.Where(e => e.OffsetX + m_Center.X < newWidth && e.OffsetY + m_Center.Y < newHeight).ToArray();

				InvalidateTiles(true);
			}
			else if (newWidth > oldWidth || newHeight > oldHeight)
			{
				m_Max.X += newWidth - oldWidth;
				m_Max.Y += newHeight - oldHeight;

				InvalidateTiles(false);
			}
		}

		public void Add(int itemID, int x, int y, int z)
		{
			itemID &= TileData.MaxItemValue;

			var vx = x + m_Center.m_X;
			var vy = y + m_Center.m_Y;

			if (vx < 0 || vx >= Width || vy < 0 || vy >= Height)
			{
				return;
			}

			var data = TileData.ItemTable[itemID];

			var oldTiles = Tiles[vx][vy];

			for (var i = oldTiles.Length - 1; i >= 0; i--)
			{
				var tile = oldTiles[i];

				if (tile.Z == z && (tile.Height > 0) == (data.Height > 0))
				{
					if (data.Flags.HasFlag(TileFlag.Roof) == tile.Flags.HasFlag(TileFlag.Roof))
					{
						Remove(tile.ID, x, y, z, false);
					}
				}
			}

			var list = List;

			Array.Resize(ref list, list.Length + 1);

			list[list.Length - 1] = new MultiTileEntry((ushort)itemID, (short)x, (short)y, (short)z, TileFlag.Background);

			List = list;

			InvalidateTiles(false);
		}

		public void RemoveXYZH(int x, int y, int z, int minHeight)
		{
			RemoveXYZH(x, y, z, minHeight, false);
		}

		public void RemoveXYZH(int x, int y, int z, int minHeight, bool resize)
		{
			var update = false;
			var oldList = List;

			for (var i = 0; i < oldList.Length; ++i)
			{
				var tile = oldList[i];

				if (tile.OffsetX == x && tile.OffsetY == y && tile.OffsetZ == z && tile.Height >= minHeight)
				{
					var newList = new MultiTileEntry[oldList.Length - 1];

					for (var j = 0; j < i; ++j)
					{
						newList[j] = oldList[j];
					}

					for (var j = i + 1; j < oldList.Length; ++j)
					{
						newList[j - 1] = oldList[j];
					}

					List = newList;
					update = true;

					break;
				}
			}

			if (update)
			{
				InvalidateTiles(resize);
			}
		}

		public void Remove(int itemID, int x, int y, int z) 
		{
			Remove(itemID, x, y, z, false);
		}

		public void Remove(int itemID, int x, int y, int z, bool resize)
		{
			var update = false;
			var oldList = List;

			for (var i = 0; i < oldList.Length; i++)
			{
				var tile = oldList[i];

				if (tile.ItemID == itemID && tile.OffsetX == x && tile.OffsetY == y && tile.OffsetZ == z)
				{
					var newList = new MultiTileEntry[oldList.Length - 1];

					for (var j = 0; j < i; j++)
					{
						newList[j] = oldList[j];
					}

					for (var j = i + 1; j < oldList.Length; j++)
					{
						newList[j - 1] = oldList[j];
					}

					List = newList;
					update = true;

					break;
				}
			}

			if (update)
			{
				InvalidateTiles(resize);
			}
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(m_Min);
			writer.Write(m_Max);
			writer.Write(m_Center);

			writer.Write(List.Length);

			for (var i = 0; i < List.Length; i++)
			{
				List[i].Serialize(writer);
			}
		}

		public void Deserialize(GenericReader reader)
		{
			_ = reader.ReadInt(); // version

			m_Min = reader.ReadPoint3D();
			m_Max = reader.ReadPoint3D();
			m_Center = reader.ReadPoint3D();

			var length = reader.ReadInt();

			List = new MultiTileEntry[length];

			for (var i = 0; i < length; i++)
			{
				List[i].Deserialize(reader);
			}

			InvalidateTiles(false);
		}
	}
}
