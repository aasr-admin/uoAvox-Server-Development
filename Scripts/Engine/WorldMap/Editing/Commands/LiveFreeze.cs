using Server;
using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engine.Facet
{
	public partial class FacetEditingCommands
	{
		[Usage("LiveFreeze")]
		[Description("Makes a targeted area of dynamic items static.")]
		public static void LiveFreeze_OnCommand(CommandEventArgs e)
		{
			BoundingBoxPicker.Begin(e.Mobile, new BoundingBoxCallback(LiveFreezeBox_Callback), null);
		}

		private static Point3D NullP3D = new Point3D(int.MinValue, int.MinValue, int.MinValue);

		private class StateInfo
		{
			public Map m_Map;
			public Point3D m_Start, m_End;

			public StateInfo(Map map, Point3D start, Point3D end)
			{
				m_Map = map;
				m_Start = start;
				m_End = end;
			}
		}

		private class DeltaState
		{
			public int m_X, m_Y;
			public List<Item> m_List;
			public DeltaState(Point2D p)
			{
				m_X = p.X;
				m_Y = p.Y;

				m_List = new List<Item>();
			}
		}

		private const string LiveFreezeWarning = "Those items will be frozen into the map. Do you wish to proceed?";

		private static void LiveFreezeBox_Callback(Mobile from, Map map, Point3D start, Point3D end, object state)
		{
			SendWarning(from, "You are about to freeze a section of items.", LiveFreezeWarning, map, start, end, new WarningGumpCallback(LiveFreezeWarning_Callback));
		}

		private static void LiveFreezeWarning_Callback(Mobile from, bool okay, object state)
		{
			if (!okay)
			{
				return;
			}

			StateInfo si = (StateInfo)state;

			LiveFreeze(from, si.m_Map, si.m_Start, si.m_End);
		}

		public static void LiveFreeze(Mobile from, Map targetMap, Point3D start3d, Point3D end3d)
		{
			Dictionary<Point2D, List<Item>> ItemsByBlockLocation = new Dictionary<Point2D, List<Item>>();

			if (targetMap != null && start3d != NullP3D && end3d != NullP3D)
			{
				Point2D start = targetMap.Bound(new Point2D(start3d));
				Point2D end = targetMap.Bound(new Point2D(end3d));

				IPooledEnumerable eable = targetMap.GetItemsInBounds(new Rectangle2D(start.X, start.Y, end.X - start.X + 1, end.Y - start.Y + 1));

				Console.WriteLine(string.Format("Invoking live freeze from {0},{1} to {2},{3}", start.X, start.Y, end.X, end.Y));

				foreach (Item item in eable)
				{
					if (item is Static || item is BaseFloor || item is BaseWall)
					{
						Map itemMap = item.Map;

						if (itemMap == null || itemMap == Map.Internal)
						{
							continue;
						}

						Point2D p = new Point2D(item.X >> 3, item.Y >> 3);

						if (!(ItemsByBlockLocation.ContainsKey(p)))
						{
							ItemsByBlockLocation.Add(p, new List<Item>());
						}

						ItemsByBlockLocation[p].Add(item);
					}
				}

				eable.Free();
			}
			else
			{
				from.SendMessage("That was not a proper area. Please retarget and reissue the command.");

				return;
			}

			TileMatrix matrix = targetMap.Tiles;

			foreach (KeyValuePair<Point2D, List<Item>> kvp in ItemsByBlockLocation)
			{
				StaticTile[][][] blockOfTiles = matrix.GetStaticBlock(kvp.Key.X, kvp.Key.Y);
				Dictionary<Point2D, List<StaticTile>> newBlockStatics = new Dictionary<Point2D, List<StaticTile>>();

				foreach (Item item in kvp.Value)
				{
					int xOffset = item.X - (kvp.Key.X * 8);
					int yOffset = item.Y - (kvp.Key.Y * 8);

					if (xOffset < 0 || xOffset >= 8 || yOffset < 0 || yOffset >= 8)
					{
						continue;
					}

					StaticTile newTile = new StaticTile((ushort)item.ItemID, (byte)xOffset, (byte)yOffset, (sbyte)item.Z, (ushort)item.Hue);
					Point2D refPoint = new Point2D(xOffset, yOffset);

					if (!(newBlockStatics.ContainsKey(refPoint)))
					{
						newBlockStatics.Add(refPoint, new List<StaticTile>());
					}

					newBlockStatics[refPoint].Add(newTile);
					item.Delete();
				}

				for (int i = 0; i < blockOfTiles.Length; i++)
					for (int j = 0; j < blockOfTiles[i].Length; j++)
						for (int k = 0; k < blockOfTiles[i][j].Length; k++)
						{
							Point2D refPoint = new Point2D(i, j);

							if (!(newBlockStatics.ContainsKey(refPoint)))
							{
								newBlockStatics.Add(refPoint, new List<StaticTile>());
							}

							newBlockStatics[refPoint].Add(blockOfTiles[i][j][k]);
						}

				StaticTile[][][] newblockOfTiles = new StaticTile[8][][];

				for (int i = 0; i < 8; i++)
				{
					newblockOfTiles[i] = new StaticTile[8][];

					for (int j = 0; j < 8; j++)
					{
						Point2D p = new Point2D(i, j);

						int length = 0;

						if (newBlockStatics.ContainsKey(p))
						{
							length = newBlockStatics[p].Count;
						}

						newblockOfTiles[i][j] = new StaticTile[length];

						for (int k = 0; k < length; k++)
						{
							if (newBlockStatics.ContainsKey(p))
							{
								newblockOfTiles[i][j][k] = newBlockStatics[p][k];
							}
						}
					}
				}

				matrix.SetStaticBlock(kvp.Key.X, kvp.Key.Y, newblockOfTiles);
				int blockNum = ((kvp.Key.X * matrix.BlockHeight) + kvp.Key.Y);

				List<Mobile> candidates = new List<Mobile>();

				int bX = kvp.Key.X * 8;
				int bY = kvp.Key.Y * 8;

				IPooledEnumerable eable = targetMap.GetMobilesInRange(new Point3D(bX, bY, 0));

				foreach (Mobile m in eable)
				{
					if (m.Player)
					{
						candidates.Add(m);
					}
				}

				eable.Free();

				CRC.InvalidateBlockCRC(targetMap.MapID, blockNum);

				foreach (Mobile m in candidates)
				{
					m.Send(new UpdateStaticsPacket(new Point2D(kvp.Key.X, kvp.Key.Y), m));
				}

				MapChangeTracker.MarkStaticsBlockForSave(targetMap.MapID, kvp.Key);
			}
		}

		public static void SendWarning(Mobile m, string header, string baseWarning, Map map, Point3D start, Point3D end, WarningGumpCallback callback)
		{
			m.SendGump(new WarningGump(1060635, 30720, String.Format(baseWarning, String.Format(header, map)), 0xFFC000, 420, 400, callback, new StateInfo(map, start, end)));
		}
	}
}