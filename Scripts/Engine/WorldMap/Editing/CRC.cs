using Server.Engines.Facet;

using System;

namespace Server.Engine.Facet
{
	public class CRC
	{
		public static ushort[][] MapCRCs { get; private set; } // CRC Caching: [map][block]

		public static void InvalidateBlockCRC(int map, int block)
		{
			MapCRCs[map][block] = UInt16.MaxValue;
		}

		public static void Configure()
		{
			EventSink.WorldLoad += new WorldLoadEventHandler(OnLoad);
		}

		public static void OnLoad()
		{
			MapCRCs = new ushort[256][];

			/// We Need CRCs For Every Block In Every Map
			foreach (var kvp in MapRegistry.Associations)
			{
				var blocks = Map.Maps[kvp.Key].Tiles.BlockWidth * Map.Maps[kvp.Key].Tiles.BlockHeight;

				MapCRCs[kvp.Key] = new ushort[blocks];

				for (var j = 0; j < blocks; j++)
				{
					MapCRCs[kvp.Key][j] = UInt16.MaxValue;
				}
			}
		}

		#region Developer Notations

		/// Thank you http://en.wikipedia.org/wiki/Fletcher%27s_checksum
		/// Each sum is computed modulo 255 and thus remains less than 
		/// 0xFF at all times. This implementation will thus never 
		/// produce the checksum results 0x00FF, 0xFF00 or 0xFFFF.

		#endregion

		public static ushort Fletcher16(byte[] data)
		{
			ushort sum1 = 0;
			ushort sum2 = 0;

			int index;

			for (index = 0; index < data.Length; ++index)
			{
				sum1 = (ushort)((sum1 + data[index]) % 255);
				sum2 = (ushort)((sum2 + sum1) % 255);
			}

			return (ushort)((sum2 << 8) | sum1);
		}
	}
}