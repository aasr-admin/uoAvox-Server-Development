using Server.Mobiles;
using Server.Network;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Engine.Facet
{
	public static class FacetEditingPacketHandlers
	{
		public static void Configure()
		{
			EventSink.Disconnected += EventSink_Disconnected;
		}

		private static void EventSink_Disconnected(DisconnectedEventArgs e)
		{
			if (e.Mobile is PlayerMobile player)
			{
				Console.WriteLine($"Server Facet Settings Configured For {player.Name}");

				//player.FacetEditingMajorVersion = 0;
				//player.FacetEditingMinorVersion = 0;
			}
		}

		public static void Initialize()
		{
			PacketHandlers.Register(0x3F, 0, true, ReceiveFacetEditingCommand);
		}

		public static void ReceiveFacetEditingCommand(NetState state, PacketReader pvSrc)
		{
			_ = pvSrc.Seek(13, SeekOrigin.Begin);

			var facetEditingCommand = pvSrc.ReadByte();

			switch (facetEditingCommand)
			{
				case 0xFF: // block query response
					{
						HandleBlockQueryReply(state, pvSrc);
					}

					break;

				case 0xFE: // read client version of Server Facet
					{
						_ = pvSrc.Seek(15, SeekOrigin.Begin);

						var majorVersion = pvSrc.ReadUInt16();
						var minorVersion = pvSrc.ReadUInt16();

						Console.WriteLine(String.Format("Received Server Facet Version Packet: {0}.{1} From {2}", majorVersion, minorVersion, state.Mobile.Name));

						//if (state != null && state.Mobile is PlayerMobile player)
						//{
						//	player.FacetEditingMajorVersion = majorVersion;
						//	player.FacetEditingMinorVersion = minorVersion;
						//}
					}

					break;

				#region Developer Notations

				/// Need to write in functionality for direct server edit commands
				/// from players with accesslevel that is high enough.
				/// This will be for future enhancements on the client end (Client Overlay Editor,
				/// Pandora's Box Plugin, etc)

				#endregion

				default:
					{
					}

					break;
			}
		}

		#region Developer Notations

		/// Whenever the client moves out of a block, we will ask the client to 
		/// provide us with a list of blocks around it and their corresponding
		/// block versions.  If any of them are different than the server's 
		/// block versions, we'll know the client needs to be updated, and 
		/// we'll send the appropriate blocks.

		#endregion

		public static void HandleBlockQueryReply(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			// byte 000              -  cmd
			// byte 001 through 002  -  packet size

			_ = pvSrc.Seek(3, SeekOrigin.Begin); // byte 003 through 006  -  central block number for the query (block that player is standing in)

			var blocknum = pvSrc.ReadUInt32();

			_ = pvSrc.ReadUInt32(); // byte 007 through 010  -  number of statics in the packet (8 for a query response)

			// byte 011 through 012  -  Server Facet sequence number - we sent one out, did we get it back?
			// byte 013              -  Server Facet command (0xFF is a block Query Response)

			_ = pvSrc.Seek(14, SeekOrigin.Begin); // byte 014              -  Server Facet mapnumber

			var mapID = (int)pvSrc.ReadByte();

			if (mapID != from.Map.MapID)
			{
				Console.WriteLine($"Received a block query response from {from.Name} for map {mapID} but that player is on map {from.Map.MapID}");
				return;
			}

			var receivedCRCs = new ushort[25]; // byte 015 through 64   -  25 block CRCs

			for (var i = 0; i < 25; i++)
			{
				receivedCRCs[i] = pvSrc.ReadUInt16();
			}

			/// TODO: see if sequence numbers are valid

			PushBlockUpdates((int)blocknum, mapID, receivedCRCs, from);
		}

		public static ushort GetBlockCrc(Point2D blockCoords, int mapID, ref byte[] landDataOut, ref byte[] staticsDataOut)
		{
			if (blockCoords.X < 0 || blockCoords.Y < 0)
			{
				return 0;
			}

			var map = Map.Maps[mapID];

			if (map == null || blockCoords.X >= map.Tiles.BlockWidth || blockCoords.Y >= map.Tiles.BlockHeight)
			{
				return 0;
			}

			landDataOut = BlockUtility.GetLandData(blockCoords, mapID);
			staticsDataOut = BlockUtility.GetRawStaticsData(blockCoords, mapID);

			var blockData = new byte[landDataOut.Length + staticsDataOut.Length];

			Array.Copy(landDataOut, 0, blockData, 0, landDataOut.Length);
			Array.Copy(staticsDataOut, 0, blockData, landDataOut.Length, staticsDataOut.Length);

			return CRC.Fletcher16(blockData);
		}

		public static void PushBlockUpdates(int block, int mapID, ushort[] recievedCRCs, Mobile from)
		{
			//Console.WriteLine("------------------------------------------Push Block Updates----------------------------------------");
			//Console.WriteLine("Map: " + mapID);
			//Console.WriteLine("Block: " + block);
			//Console.WriteLine("----------------------------------------------------------------------------------------------------");

			var map = Map.Maps[mapID];
			var tm = map.Tiles;

			var blockX = block / tm.BlockHeight;
			var blockY = block % tm.BlockHeight;

			var wrapWidthInBlocks = map.Area.Width >> 3;
			var wrapHeightInBlocks = map.Area.Height >> 3;
			var mapWidthInBlocks = map.Bounds.Width >> 3;
			var mapHeightInBlocks = map.Bounds.Height >> 3;

			//Console.WriteLine("BlockX: " + blockX + " BlockY: " + blockY);
			//Console.WriteLine("Map Height in blocks: " + mapHeightInBlocks);
			//Console.WriteLine("Map Width in blocks: " + mapWidthInBlocks);
			//Console.WriteLine("Wrap Height in blocks: " + wrapHeightInBlocks);
			//Console.WriteLine("Wrap Width in blocks: " + wrapWidthInBlocks);

			if (block < 0 || block >= mapWidthInBlocks * mapHeightInBlocks)
			{
				return;
			}

			for (var x = -2; x <= 2; x++)
			{
				int xBlockItr;

				if (blockX < wrapWidthInBlocks)
				{
					xBlockItr = (blockX + x) % wrapWidthInBlocks;

					if (xBlockItr < 0)
					{
						xBlockItr += wrapWidthInBlocks;
					}
				}
				else
				{
					xBlockItr = (blockX + x) % mapWidthInBlocks;

					if (xBlockItr < 0)
					{
						xBlockItr += mapWidthInBlocks;
					}
				}

				for (var y = -2; y <= 2; y++)
				{
					int yBlockItr;

					if (blockY < wrapHeightInBlocks)
					{
						yBlockItr = (blockY + y) % wrapHeightInBlocks;

						if (yBlockItr < 0)
						{
							yBlockItr += wrapHeightInBlocks;
						}
					}
					else
					{
						yBlockItr = (blockY + y) % mapHeightInBlocks;

						if (yBlockItr < 0)
						{
							yBlockItr += mapHeightInBlocks;
						}
					}

					var blocknum = (xBlockItr * mapHeightInBlocks) + yBlockItr;

					// CRC Caching
					var crc = CRC.MapCRCs[mapID][blocknum];

					var landData = Array.Empty<byte>();
					var staticsData = Array.Empty<byte>();

					var blockPosition = new Point2D(xBlockItr, yBlockItr);

					if (crc == UInt16.MaxValue)
					{
						CRC.MapCRCs[mapID][blocknum] = crc = GetBlockCrc(blockPosition, mapID, ref landData, ref staticsData);
					}

					//Console.WriteLine(crc.ToString("X4") + " vs " + recievedCRCs[((x + 2) * 5) + y + 2].ToString("X4"));
					//Console.WriteLine(String.Format("({0},{1})", blockPosition.X, blockPosition.Y));

					if (crc != recievedCRCs[((x + 2) * 5) + y + 2])
					{
						if (landData.Length < 1)
						{
							_ = from.Send(new UpdateTerrainPacket(blockPosition, from));
							_ = from.Send(new UpdateStaticsPacket(blockPosition, from));
						}
						else
						{
							_ = from.Send(new UpdateTerrainPacket(landData, blocknum, from.Map.MapID));
							_ = from.Send(new UpdateStaticsPacket(staticsData, blocknum, from.Map.MapID));
						}
					}
				}
			}

			//if (refreshClientView)
			//{
			//  from.Send(new RefreshClientView());
			//}
		}
	}

	#region Update Statics Packet

	#region Developer Notations

	/// Update Statics Packet 
	/// 
	/// 
	/// Original Packet: 
	/// ------------------------------
	/// Source: http://necrotoolz.sourceforge.net/kairpacketguide/packet3f.htm
	/// 
	/// 0x3f      Command 
	/// ushort    Size of packet
	/// uint      Block Number
	/// uint      Number of statics
	/// uint      Extra value for the index file
	/// 
	/// Static[number of statics]    7 bytes
	///       ushort      ArtID
	///       byte        X offset in the block
	///       byte        Y offset in the block
	///       sbyte       Z axis position of the static
	///       ushort      Hue Number
	///       
	/// 
	/// 
	/// New Format:
	/// ------------------------------
	/// byte         0x3f
	/// ushort       size of the packet
	/// int          block_number
	/// uint         number of statics
	/// uint         extra value that we're splitting
	///              byte  hash
	///              byte  Server Facet Command
	///              ushort  mapnumber - if this is 0xFF it means its a query
	/// struct
	/// [number of statics]
	///              ushort      ItemId      
	///              byte        X           // Not actually stored in runuo
	///              byte        Y           // Not actually stored in runuo
	///              sbyte       Z           
	///              ushort      Hue         //no longer used
	///              
	/// 
	/// We're going to use this as a dual purpose packet. The extra field 
	/// will tell us if the packet should actually be used as a hash. The
	/// extra field is split into two padding bytes (0x00), 
	/// and an unsigned short that we're using as our map number.
	/// 
	/// If unsigned short representing the map has a maxvalue for a ushort 
	/// (0xffff), then we'll know its a packet that we're using to request 
	/// a set of 25 CRCs from the client.

	#endregion

	public class UpdateStaticsPacket : Packet
	{
		public UpdateStaticsPacket(Point2D blockCoords, Mobile m) : base(0x3F)
		{
			var playerMap = m.Map;
			var tm = playerMap.Tiles;

			var blockNum = (blockCoords.X * tm.BlockHeight) + blockCoords.Y;
			var staticsData = BlockUtility.GetRawStaticsData(blockCoords, playerMap.MapID);

			WriteDataToStream(staticsData, blockNum, playerMap.MapID);
		}

		public UpdateStaticsPacket(byte[] staticsData, int blockNumber, int mapID) : base(0x3F)
		{
			WriteDataToStream(staticsData, blockNumber, mapID);
		}

		public void WriteDataToStream(byte[] staticsData, int blockNumber, int mapID)
		{
			// byte 000         -  cmd

			EnsureCapacity(15 + staticsData.Length); //byte 001 to 002  -  packet size

			m_Stream.Write((uint)blockNumber);                  // byte 003 to 006  -  block number
			m_Stream.Write(staticsData.Length / 7);             // byte 007 to 010  -  number of statics in packet
			m_Stream.Write((ushort)0);                          // byte 011 to 012  -  Server Facet sequence number
			m_Stream.Write((byte)0);                            // byte 013         -  Server Facet command (0x00 is a statics update)
			m_Stream.Write((byte)mapID);                        // byte 014         -  Server Facet mapnumber
			m_Stream.Write(staticsData, 0, staticsData.Length); // byte 015 to ???  -  statics data

			//Console.WriteLine(string.Format("Sending statics data for block: {0}, Map: {1}", blockNum, playerMap.MapID));
			//BlockUtility.WriteStaticsDataToConsole(staticsData);
		}
	}

	#endregion

	#region Query Client Hash Packet

	/// This Is Now Using A CRC Check Instead Of Versions
	public class QueryClientHash : Packet
	{
		public QueryClientHash(Mobile m) : base(0x3F)
		{
			var playerMap = m.Map;
			var tm = playerMap.Tiles;

			var blocknum = ((m.Location.X >> 3) * tm.BlockHeight) + (m.Location.Y >> 3);

			//Console.WriteLine(String.Format("Block Query Hash: {0}", blocknum));

			// byte 000         -  cmd

			EnsureCapacity(15); // byte 001 to 002  -  packet size

			m_Stream.Write(blocknum);               // byte 003 to 006  -  central block number for the query (block that player is standing in)
			m_Stream.Write(0);                      // byte 007 to 010  -  number of statics in the packet (0 for a query)
			m_Stream.Write((ushort)0);              // byte 011 to 012  -  Server Facet sequence number
			m_Stream.Write((byte)0xFF);             // byte 013         -  Server Facet command (0xFF is a block Query)
			m_Stream.Write((byte)playerMap.MapID);  // byte 014         -  Server Facet mapnumber
		}
	}

	#endregion

	#region Update Map Definition Packet

	/// This Is Sent To The Client So The Client Knows The Dimension Of Extra Maps
	public class MapDefinitions : Packet
	{
		public MapDefinitions() : base(0x3F)
		{
			var maps = Map.AllMaps;

			var length = maps.Count(m => m != null && m.FileIndex != 0x7F) * 9; // 12 * 9 = 108
			var count = length / 7; // 108 / 7 = 15
			var padding = 0;

			if (length - (count * 7) > 0)
			{
				padding = (++count * 7) - length;
			}

			// byte 000 to 015  -  The first 15 bytes of this packet are always the same
			// byte 000         -  cmd

			EnsureCapacity(15 + length); // byte 001 to 002  -  packet size

			m_Stream.Write(0);          // byte 003 to 006  -  block number, doesn't really apply in this case
			m_Stream.Write(count);      // byte 007 to 010  -  number of statics in the packet, in this case its calculated to hold enough space for all the map definitions
			m_Stream.Write((ushort)0);  // byte 011 to 012  -  Server Facet sequence number, doesn't apply in this case
			m_Stream.Write((byte)1);    // byte 013         -  Server Facet command (0x01 is Update Map Definitions)
			m_Stream.Write((byte)0);    // byte 014         -  Server Facet mapnumber, doesn't apply in this case

			// byte 015 to end  -  Map Definitions
			foreach (var map in maps)
			{
				if (map == null || map.FileIndex == 0x7F)
				{
					continue;
				}

				m_Stream.Write((byte)map.FileIndex);    // iteration byte 000         -  map file index number
				m_Stream.Write(map.Bounds.Width);   // iteration byte 001 to 002  -  map width
				m_Stream.Write(map.Bounds.Height);  // iteration byte 003 to 004  -  map height
				m_Stream.Write(map.Area.Width);     // iteration byte 005 to 006  -  wrap around dimension X
				m_Stream.Write(map.Area.Height);        // iteration byte 007 to 008  -  wrap around dimension Y
			}

			for (var i = 0; i < padding; i++)
			{
				m_Stream.Write((byte)0x00);
			}
		}
	}

	#endregion

	#region Update Terrain Packet

	#region Developer Notations

	/// Update Terrain Packet 
	/// Source: http://necrotoolz.sourceforge.net/kairpacketguide/packet40.htm
	/// 
	/// Format:
	/// 
	/// byte         0x40
	/// uint         block number
	/// 
	/// struct[64]   maptiles
	///              ushort      Tile Number 
	///              sbyte       Z    
	/// uint         map grid header -> we're splitting this into two ushorts
	///      ushort  padding
	///      ushort  mapnumber

	#endregion

	public class UpdateTerrainPacket : Packet
	{
		public UpdateTerrainPacket(Point2D blockCoords, Mobile m) : base(0x40, 0xC9)
		{
			var playerMap = m.Map;
			var tm = playerMap.Tiles;

			var blockNumber = (blockCoords.X * tm.BlockHeight) + blockCoords.Y;
			var landData = BlockUtility.GetLandData(blockCoords, playerMap.MapID);

			WriteDataToStream(landData, blockNumber, playerMap.MapID);
		}

		public UpdateTerrainPacket(byte[] landData, int blockNumber, int mapID) : base(0x40, 0xC9)
		{
			WriteDataToStream(landData, blockNumber, mapID);
		}

		public void WriteDataToStream(byte[] landData, int blockNumber, int mapID)
		{
			// Console.WriteLine(string.Format("Packet Constructor land block coords ({0},{1})", blockCoords.X, blockCoords.Y));

			// byte 000              -  cmd

			m_Stream.Write(blockNumber);                    // byte 001 through 004  -  blocknum
			m_Stream.Write(landData, 0, landData.Length);   // byte 005 through 196  -  land data
			m_Stream.Write((byte)0);                        // byte 197              -  padding
			m_Stream.Write((byte)0);                        // byte 198              -  padding
			m_Stream.Write((byte)0);                        // byte 199              -  padding
			m_Stream.Write((byte)mapID);                    // byte 200              -  map number

			//Console.WriteLine(string.Format("Sending land data for block: {0} Map: {1}", blocknum, playerMap.MapID));
			//BlockUtility.WriteLandDataToConsole(landData);
		}
	}

	#endregion

	#region TargetObjectList Packets

	public struct TargetObject
	{
		public int ItemID;
		public int Hue;
		public int xOffset;
		public int yOffset;
		public int zOffset;
	}

	#region Developer Notations

	///Target Object List Packet
	///Thank you -hash- from RunUO.com for providing the definition for this

	#endregion

	public sealed class TargetObjectList : Packet
	{
		public TargetObjectList(List<TargetObject> targetObjects, Mobile m, bool allowGround) : base(0xB4)
		{
			byte allowGroundByte = 0;

			if (allowGround == true)
			{
				allowGroundByte = 1;
			}

			var packetSize = 16 + (targetObjects.Count * 10);

			// byte 000              -  cmd

			EnsureCapacity(packetSize); // byte 001 through 002  -  packet size

			m_Stream.Write(allowGroundByte);                // byte 003              -  Allow Ground
			m_Stream.Write((int)m.Serial);                  // byte 004 through 007  -  target serial
			m_Stream.Write((ushort)0);                      // byte 008 through 009  -  x
			m_Stream.Write((ushort)0);                      // byte 010 through 011  -  y
			m_Stream.Write((ushort)0);                      // byte 012 through 013  -  z
			m_Stream.Write((ushort)targetObjects.Count);    // byte 014 through 015  -  Number of Entries

			// byte 016 through end  -  target object entries (10 bytes each)
			foreach (var t in targetObjects)
			{
				m_Stream.Write((ushort)t.ItemID);   // entry byte 000 through 001  -  Number of Entries
				m_Stream.Write((ushort)t.Hue);      // entry byte 002 through 003  -  Number of Entries
				m_Stream.Write((ushort)t.xOffset);  // entry byte 004 through 005  -  Number of Entries
				m_Stream.Write((ushort)t.yOffset);  // entry byte 006 through 007  -  Number of Entries
				m_Stream.Write((ushort)t.zOffset);  // entry byte 008 through 009  -  Number of Entries
			}
		}
	}

	#endregion

	#region Login Complete Packet

	public class LoginComplete : Packet
	{
		public LoginComplete() : base(0x3F)
		{
			// 1 byte packet number (0x3F)
			// 2 bytes size of packet (15)
			// 4 byte block num (0x01)
			// 4 byte statics count
			// 4 byte extra

			// byte 000 to 015  -  The first 15 bytes of this packet are always the same
			// byte 000         -  cmd

			EnsureCapacity(43); // byte 001 to 002  -  packet size

			m_Stream.Write(1U);         // byte 003 to 006  -  block number, doesn't really apply in this case
			m_Stream.Write(4U);         // byte 007 to 010  -  number of statics in the packet - 0 in this case
			m_Stream.Write((ushort)0);  // byte 011 to 012  -  Server Facet sequence number, doesn't apply in this case
			m_Stream.Write((byte)2);    // byte 013         -  Server Facet command (0x02 is Login Confirmation)
			m_Stream.Write((byte)0);    // byte 014         -  Server Facet mapnumber, doesn't apply in this case

			if (FacetEditingSettings.UNIQUE_SERVER_IDENTIFIER.Length < 28)
			{
				m_Stream.WriteAsciiFixed(FacetEditingSettings.UNIQUE_SERVER_IDENTIFIER, FacetEditingSettings.UNIQUE_SERVER_IDENTIFIER.Length);

				// byte 015 to 042  -  shard identifier
				var remainingLength = 28 - FacetEditingSettings.UNIQUE_SERVER_IDENTIFIER.Length;

				for (var i = 0; i < remainingLength; ++i)
				{
					m_Stream.Write((byte)0);
				}
			}
			else
			{
				m_Stream.WriteAsciiFixed(FacetEditingSettings.UNIQUE_SERVER_IDENTIFIER.Substring(0, 28), 28);
			}
		}
	}

	#endregion

	#region Refresh Client View Packet

	///This Is Now Using A CRC Check Instead Of Versions
	public class RefreshClientView : Packet
	{
		public RefreshClientView() : base(0x3F)
		{
			// byte 000         -  cmd

			EnsureCapacity(15); // byte 001 to 002  -  packet size

			m_Stream.Write(0);          // byte 003 to 006  -  central block number for the query (block that player is standing in)
			m_Stream.Write(0);          // byte 007 to 010  -  number of statics in the packet (0 for a query)
			m_Stream.Write((ushort)0);  // byte 011 to 012  -  Server Facet sequence number
			m_Stream.Write((byte)3);    // byte 013         -  Server Facet command (0x03 is a REFRESH_CLIENT)
			m_Stream.Write((byte)0);    // byte 014         -  Server Facet mapnumber
		}
	}

	#endregion
}