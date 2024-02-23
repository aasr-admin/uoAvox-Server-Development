using Server;
using Server.Commands;
using Server.Commands.Generic;
using Server.Engines.Facet;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Server.Engine.Facet
{
	public partial class FacetEditingCommands
	{
		[Usage("ExportClientFiles")]
		[Description("Increases the Z value of a static.")]
		private static void exportClientFiles_OnCommand(CommandEventArgs e)
		{
			ExportOnNextSave = true;
			Server.Misc.AutoSave.Save();
		}

		public static void Configure()
		{
			EventSink.WorldSave += new WorldSaveEventHandler(OnSave);
		}

		public static bool ExportOnNextSave = false;

		/// Path Used For Hashes
		private static Map m_WorkMap = null;

		public static Map WorkMap { get { return m_WorkMap; } }

		public static void OnSave(WorldSaveEventArgs e)
		{
			if (!ExportOnNextSave)
			{
				return;
			}

			ExportOnNextSave = false;

			if (!Directory.Exists(FacetEditingSettings.ModifiedClientFilesSavePath))
			{
				Directory.CreateDirectory(FacetEditingSettings.ModifiedClientFilesSavePath);
			}

			Console.Write("Exporting Client Files...");

			foreach (var kvp in MapRegistry.Associations)
			{
				string filename = string.Format("map{0}.mul", kvp.Key);

				GenericWriter writer = new BinaryFileWriter(Path.Combine(FacetEditingSettings.ModifiedClientFilesSavePath, filename), true);

				m_WorkMap = Server.Map.Maps[kvp.Key];
				TileMatrix CurrentMatrix = m_WorkMap.Tiles;

				int blocks = CurrentMatrix.BlockWidth * CurrentMatrix.BlockHeight;

				for (int xblock = 0; xblock < CurrentMatrix.BlockWidth; xblock++)
				{
					for (int yblock = 0; yblock < CurrentMatrix.BlockHeight; yblock++)
					{
						writer.Write((uint)0);

						LandTile[] blocktiles = CurrentMatrix.GetLandBlock(xblock, yblock);

						if (blocktiles.Length == 196)
						{
							Console.WriteLine("Invalid landblock! Save failed!");

							return;
						}
						else
						{
							for (int j = 0; j < 64; j++)
							{
								writer.Write((short)blocktiles[j].ID);
								writer.Write((sbyte)blocktiles[j].Z);
							}
						}
					}
				}

				writer.Close();
			}

			/* Statics */
			foreach (var kvp in MapRegistry.Associations)
			{
				string filename = string.Format("statics{0}.mul", kvp.Key);

				GenericWriter staticWriter = new BinaryFileWriter(Path.Combine(FacetEditingSettings.ModifiedClientFilesSavePath, filename), true);

				filename = string.Format("staidx{0}.mul", kvp.Key);

				GenericWriter staticIndexWriter = new BinaryFileWriter(Path.Combine(FacetEditingSettings.ModifiedClientFilesSavePath, filename), true);

				m_WorkMap = Server.Map.Maps[kvp.Key];
				TileMatrix CurrentMatrix = m_WorkMap.Tiles;

				int blocks = CurrentMatrix.BlockWidth * CurrentMatrix.BlockHeight;
				int startBlock = 0;
				int finishBlock = 0;

				for (int xblock = 0; xblock < CurrentMatrix.BlockWidth; xblock++)
				{
					for (int yblock = 0; yblock < CurrentMatrix.BlockHeight; yblock++)
					{
						StaticTile[][][] staticTiles = CurrentMatrix.GetStaticBlock(xblock, yblock);

						/// Static File
						for (int i = 0; i < staticTiles.Length; i++)
							for (int j = 0; j < staticTiles[i].Length; j++)
							{
								StaticTile[] sortedTiles = staticTiles[i][j];
								Array.Sort(sortedTiles, BlockUtility.CompareStaticTiles);

								for (int k = 0; k < sortedTiles.Length; k++)
								{
									staticWriter.Write((ushort)sortedTiles[k].ID);
									staticWriter.Write((byte)i);
									staticWriter.Write((byte)j);
									staticWriter.Write((sbyte)sortedTiles[k].Z);
									staticWriter.Write((short)sortedTiles[k].Hue);

									finishBlock += 7;
								}
							}

						/// Index File
						if (finishBlock != startBlock)
						{
							staticIndexWriter.Write((int)startBlock); //lookup
							staticIndexWriter.Write((int)(finishBlock - startBlock)); //length
							staticIndexWriter.Write((int)0); //extra

							startBlock = finishBlock;
						}
						else
						{
							staticIndexWriter.Write((uint)uint.MaxValue); //lookup
							staticIndexWriter.Write((uint)uint.MaxValue); //length
							staticIndexWriter.Write((uint)uint.MaxValue); //extra
						}
					}
				}

				staticWriter.Close();
				staticIndexWriter.Close();
			}
		}
	}
}