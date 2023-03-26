﻿using Server;
using Server.Engines.Facet;
using Server.Misc;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Server.Engine.Facet
{
	#region Developer Notations

    /// Map Change Format
    /// -----------------------
    /// 2 bytes      Map Index
    /// 2 bytes      X block number
    /// 2 bytes      Y block number
    /// 192 bytes    landblocks[land block count] 
    /// 
    /// Statics Change Format 
    /// 2 bytes      Map Index
    /// 4 bytes      X block number
    /// 4 bytes      Y block number
    /// 4 bytes      Number of statics
    /// 7 bytes      Static[Number of statics]

	#endregion

	public class MapChangeTracker
	{
		private static Hashtable[] m_LandChanges;
		private static Hashtable[] m_StaticsChanges;

		public static void Configure()
		{
			m_LandChanges = new Hashtable[256];
			m_StaticsChanges = new Hashtable[256];

			/// foreach (KeyValuePair<int, MapRegistry.MapDefinition> kvp in MapRegistry.Definitions)

			for (int i = 0; i < 256; i++)
			{
				m_LandChanges[i] = new Hashtable();
				m_StaticsChanges[i] = new Hashtable();
			}

			EventSink.WorldLoad += new WorldLoadEventHandler(OnLoad);
			EventSink.WorldSave += new WorldSaveEventHandler(OnSave);
		}

		public static void MarkStaticsBlockForSave(int map, Point2D block)
		{
			if (!m_StaticsChanges[map].ContainsKey(block))
			{
				m_StaticsChanges[map].Add(block, null);
			}
		}

		public static void MarkLandBlockForSave(int map, Point2D block)
		{
			if (!m_LandChanges[map].ContainsKey(block))
			{
				m_LandChanges[map].Add(block, null);
			}
		}

		public static void OnLoad()
		{
			Console.WriteLine("Loading Server Facet Map Changes...");

			if (!Directory.Exists(FacetEditingSettings.LiveRealTimeChangesSavePath))
			{
				Directory.CreateDirectory(FacetEditingSettings.LiveRealTimeChangesSavePath);
			}

			string[] filePaths = Directory.GetFiles(FacetEditingSettings.LiveRealTimeChangesSavePath, "*.live");

			List<string> staticsPaths = new List<string>();
			List<string> landPaths = new List<string>();

			foreach (string s in filePaths)
			{
				if (s.Contains("map"))
				{
					landPaths.Add(s);
				}
				else if (s.Contains("statics"))
				{
					staticsPaths.Add(s);
				}
			}

			landPaths.Sort();

			/// Read Map Blocks And Apply Them In Order
			foreach (string s in landPaths)
			{
				BinaryReader reader = new BinaryReader(File.Open(Path.Combine(Core.BaseDirectory, s), FileMode.Open));

				try
				{
					reader.BaseStream.Seek(0, SeekOrigin.Begin);

					int MapNumber = reader.ReadUInt16();

					while (reader.BaseStream.Position < reader.BaseStream.Length)
					{
						int x = (int)reader.ReadInt16();
						int y = (int)reader.ReadInt16();

						LandTile[] blocktiles = new LandTile[64];

						for (int j = 0; j < 64; j++)
						{
							short id = reader.ReadInt16();
							sbyte z = reader.ReadSByte();

							LandTile lt = new LandTile(id, z);

							blocktiles[j] = lt;
						}

						HashSet<int> associated;
						MapRegistry.MapAssociations.TryGetValue(MapNumber, out associated);

						foreach (int integer in associated)
						{
							Map map = Map.Maps[integer];
							TileMatrix tm = map.Tiles;

							tm.SetLandBlock(x, y, blocktiles);
						}
					}
				}
				catch
				{
					Console.WriteLine("An error occured reading land changes at " + reader.BaseStream.Position);
				}
				finally
				{
					reader.Close();
				}
			}

			staticsPaths.Sort();

			/// Read Static Blocks And Apply Them In Order
			foreach (string s in staticsPaths)
			{
				FileInfo mapFile = new FileInfo(Path.Combine(Core.BaseDirectory, s));
				BinaryReader reader = new BinaryReader(File.Open(Path.Combine(Core.BaseDirectory, s), FileMode.Open));

				try
				{
					reader.BaseStream.Seek(0, SeekOrigin.Begin);

					int MapNumber = reader.ReadUInt16();

					while (reader.BaseStream.Position < reader.BaseStream.Length)
					{
						int blockX = (int)reader.ReadInt16();
						int blockY = (int)reader.ReadInt16();
						int staticCount = reader.ReadInt32();

						Dictionary<Point2D, List<StaticTile>> blockStatics = new Dictionary<Point2D, List<StaticTile>>();

						for (int staticIndex = 0; staticIndex < staticCount; staticIndex++)
						{
							UInt16 id = reader.ReadUInt16();

							byte x = reader.ReadByte();
							byte y = reader.ReadByte();
							sbyte z = reader.ReadSByte();

							Int16 hue = reader.ReadInt16();

							StaticTile st = new StaticTile(id, x, y, z, hue);

							Point2D p = new Point2D(x, y);

							if (!(blockStatics.ContainsKey(p)))
							{
								blockStatics.Add(p, new List<StaticTile>());
							}

							blockStatics[p].Add(st);
						}

						StaticTile[][][] newblockOfTiles = new StaticTile[8][][];

						for (int i = 0; i < 8; i++)
						{
							newblockOfTiles[i] = new StaticTile[8][];

							for (int j = 0; j < 8; j++)
							{
								Point2D p = new Point2D(i, j);

								int length = 0;

								if (blockStatics.ContainsKey(p))
								{
									length = blockStatics[p].Count;
								}

								newblockOfTiles[i][j] = new StaticTile[length];

								for (int k = 0; k < length; k++)
								{
									if (blockStatics.ContainsKey(p))
									{
										newblockOfTiles[i][j][k] = blockStatics[p][k];
									}
								}
							}
						}

						HashSet<int> associated;
						MapRegistry.MapAssociations.TryGetValue(MapNumber, out associated);

						foreach (int integer in associated)
						{
							Map map = Map.Maps[integer];
							TileMatrix tm = map.Tiles;

							tm.SetStaticBlock(blockX, blockY, newblockOfTiles);
						}
					}
				}
				catch
				{
					Console.WriteLine("An error occured reading land changes.");
				}
				finally
				{
					reader.Close();
				}
			}
		}

		public static void OnSave(WorldSaveEventArgs e)
		{
			if (!Directory.Exists(FacetEditingSettings.LiveRealTimeChangesSavePath))
			{
				Directory.CreateDirectory(FacetEditingSettings.LiveRealTimeChangesSavePath);
			}

			DateTime now = DateTime.Now;
			string Stamp = string.Format("{0}-{1}-{2}-{3}-{4}-{5}", now.Year, now.Month.ToString("00"), now.Day.ToString("00"), now.Hour.ToString("00"), now.Minute.ToString("00"), now.Second.ToString("00"));
			
			foreach (KeyValuePair<int, MapRegistry.MapDefinition> kvp in MapRegistry.Definitions) //for (int mapIndex = 0; mapIndex < Live.NumberOfMapFiles; mapIndex++)
			{
				try
				{
					Map CurrentMap = Server.Map.Maps[kvp.Key];
					TileMatrix CurrentMatrix = CurrentMap.Tiles;

					ICollection keyColl = m_LandChanges[kvp.Key].Keys;

					if (keyColl.Count > 0)
					{
						string filename = string.Format("map{0}-{1}.live", kvp.Key, Stamp);

						Console.WriteLine(Path.Combine(FacetEditingSettings.LiveRealTimeChangesSavePath, filename));
						GenericWriter writer = new BinaryFileWriter(Path.Combine(FacetEditingSettings.LiveRealTimeChangesSavePath, filename), true);

						writer.Write((UInt16)kvp.Key);

						foreach (Point2D p in keyColl)
						{
							writer.Write((UInt16)p.X);
							writer.Write((UInt16)p.Y);

							LandTile[] blocktiles = CurrentMatrix.GetLandBlock(p.X, p.Y);

							for (int j = 0; j < 64; j++)
							{
								writer.Write((UInt16)blocktiles[j].ID);
								writer.Write((sbyte)blocktiles[j].Z);
							}
						}

						writer.Close();
					}

					m_LandChanges[kvp.Key].Clear();

					keyColl = m_StaticsChanges[kvp.Key].Keys;

					if (keyColl.Count > 0)
					{
						string filename = string.Format("statics{0}-{1}.live", kvp.Key, Stamp);
						GenericWriter writer = new BinaryFileWriter(Path.Combine(FacetEditingSettings.LiveRealTimeChangesSavePath, filename), true);
						writer.Write((UInt16)kvp.Key);

						foreach (Point2D p in keyColl)
						{
							StaticTile[][][] staticTiles = CurrentMatrix.GetStaticBlock(p.X, p.Y);

							int staticCount = 0;

							for (int i = 0; i < staticTiles.Length; i++)
								for (int j = 0; j < staticTiles[i].Length; j++)
								{
									staticCount += staticTiles[i][j].Length;
								}

							writer.Write((UInt16)p.X);
							writer.Write((UInt16)p.Y);
							writer.Write((int)staticCount);

							for (int i = 0; i < staticTiles.Length; i++)
								for (int j = 0; j < staticTiles[i].Length; j++)
								{
									for (int k = 0; k < staticTiles[i][j].Length; k++)
									{
										writer.Write((ushort)staticTiles[i][j][k].ID);
										writer.Write((byte)i);
										writer.Write((byte)j);
										writer.Write((sbyte)staticTiles[i][j][k].Z);
										writer.Write((short)staticTiles[i][j][k].Hue);
									}
								}
						}
						writer.Close();
					}
					m_StaticsChanges[kvp.Key].Clear();
				}
				catch
				{
					Console.WriteLine("Key: " + kvp.Key);
				}
			}
		}
	}
}