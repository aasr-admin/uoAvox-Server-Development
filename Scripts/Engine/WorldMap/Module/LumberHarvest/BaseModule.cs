//#define CUSTOM_TREE_GRAPHICS

using Server.Engines.Harvest;
using Server.Items;
using Server.Targeting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#region Developer Notations

///    +---------------------------+              +---------------------------+
///    | GraphicAsset              +-------+      | BaseHarvestablePhase      |
///    |---------------------------|       |      |---------------------------|
///    | ItemId                    |       |      | FinalHarvestPhase         |
///    | xOffset                   |       |      | StartingGrowthPhase       |
///    | yOffset                   |       |      | NextHarvestPhase          |
///    | HarvestResourceBaseAmount |       |      | NextGrowthPhase           |
///    | BonusResourceBaseAmount   |       |      |                           |
///    | BonusResources            |       |      | HarvestResourceBaseAmount |
///    |                           |       |      | BonusResourceBaseAmount   |
///    | +ReapBonusResources       |       |      | HarvestSkill              |
///    +---------------------------+       |      |                           |
///                                        |      | PhaseResources            |
///                                        |      | BonusResources            |
///                                        +-----+| BaseAssetSets             |
///                                               |                           |
///                                               | +Construct                |
///                                               | +TearDown                 |
///                                               | +Harvest                  |
///                                               | +Grow                     |
///                                               |                           |
///                                               | +ReapResource             |
///                                               | +ReapBonusResources       |
///                                               |                           |
///                                               | +LookupOriginLocation     |
///                                               | +LookupAsset              |
///                                               | +LookupPhase              |
///                                               | +LookupStaticHue          |
///                                               | +LookupStaticTile         |
///                                               |                           |
///                                               +---------------------------+
///  
///                              Grow                     Grow                      Grow
///  
///           +----------------+       +----------------+       +-----------------+      +-----------------+
///           | SaplingPhase1  +------>| SaplingPhase2  +------>| GrownTreePhase1 +----->| GrownTreePhase2 |
///           +----------------+       +----------------+       +-----------------+      +-----------------+
///  
///  
///                             Harvest                   Harvest
///  
///           +-----------------+      +------------------+      +------------------+
///           | GrownTreePhase2 +----->| FallenTreePhase1 +----->| FallenTreePhase2 +-------+
///           +-----------------+      +------------------+      +------------------+       |
///                                                                       ^                 |
///                                                                       |     Harvest     |
///                                                                       |                 |
///                                                                       +-----------------+
///      
///                           +----------------------+
///                           | BaseHarvestablePhase |
///                           +----------------------+
///                                       ^ ^
///                                       | |
///                                       | +-----------------------------------------------------------------+
///                                       |                                                                   |
///                             +---------+--------+                                               +---------------------+
///                             | TreeHarvestPhase |                                               | RockMiningBasePhase |
///                             +------------------+                                               +---------------------+
///                                     ^ ^ ^                                                                 ^  ^
///                                     | | |                                                                 |  |
///                                     | | |                                                                 |  +--------------+
///                                     | | |                                                                 |                 |
///            +------------------------+ | +--------------------------+                               +------+------+  +-------+-------+
///            |                          |                            |                               | MoltenPhase |  | HardenedPhase |
///   +--------+---------+      +---------+----------+       +---------+-------+                       +-------------+  +---------------+
///   | SaplingTreePhase |      | FullGrownTreePhase |       | FallenTreePhase |                                              ^ ^
///   +------------------+      +--------------------+       +-----------------+                                              | |
///                                       ^                            ^  ^                                    +--------------+ |
///                                       |                            |  |                                    |                |
///                               +-------+--------+                   |  +--------------+                +----+-----+    +-----+------+
///                               | SmallTreePhase |                   |                 |                | GoldVein |    | CopperVein |
///                               +----------------+         +---------+-------+ +---------------+        +----------+    +------------+
///                                     ^ ^ ^                | SmallFallenTree | | FallenOakTree |
///                                     | | |                +-----------------+ +---------------+
///                                     | | |
///                         +-----------+ | +------------+
///                         |             |              |
///                   +-----+-----+ +-----+-----+ +------+-----+
///                   | OakTree   | | CedarTree | | WalnutTree |
///                   +-----------+ +-----------+ +------------+ 

#endregion

namespace Server.Engine.Facet.Module.LumberHarvest
{
	public static class FacetModule_Lumberjacking
	{
		//List contains an entry for each map, each entry contains all the locations where trees have been chopped down
		public static Dictionary<int, Dictionary<Point3D, int>> RegrowthMasterLookupTable { get; } = new();

		public static DateTime LastGrowth { get; private set; } = DateTime.UtcNow;

		//This is the minimum time between regrowths, but the regrowths will only happen on world saves
		public static TimeSpan TimeBetweenRegrowth { get; set; } = TimeSpan.FromMinutes(1);

		public static void Configure()
		{
			foreach (var m in Map.AllMaps)
			{
				RegrowthMasterLookupTable[m.MapID] = new();
			}

			EventSink.WorldSave += OnSave;
			EventSink.WorldLoad += OnLoad;

			if (FacetEditingSettings.LumberHarvestModuleEnabled)
			{
				ModifyLumberjacking();
			}
		}

		public static void OnLoad()
		{
			for (var mapId = 0; mapId < RegrowthMasterLookupTable.Count; ++mapId)
			{
				var filename = Path.Combine(FacetEditingSettings.LumberHarvestFallenTreeSaveLocation, $"TreeLocations.{mapId}");

				if (File.Exists(filename))
				{
					using var fs = new FileStream(filename, FileMode.Open);
					using var br = new BinaryReader(fs);

					var reader = new BinaryFileReader(br);

					while (!reader.End())
					{
						var p = reader.ReadPoint3D();
						var itemId = reader.ReadInt();

						if (!RegrowthMasterLookupTable.TryGetValue(mapId, out var list))
						{
							RegrowthMasterLookupTable[mapId] = list = new Dictionary<Point3D, int>();
						}

						list[p] = itemId;
					}

					reader.Close();
				}
			}
		}

		public static void OnSave(WorldSaveEventArgs e)
		{
			_ = Directory.CreateDirectory(FacetEditingSettings.LumberHarvestFallenTreeSaveLocation);

			var updateRegrowthTime = false;

			//foreach map in the lookup table
			foreach (var map in Map.AllMaps)
			{
				if (RegrowthMasterLookupTable.TryGetValue(map.MapID, out var mapLookupTable))
				{
					#region Regrowth
					if (DateTime.UtcNow > LastGrowth + TimeBetweenRegrowth)
					{
						updateRegrowthTime = true;

						MapOperationSeries mapOperations = null;

						var locationsToRemove = new HashSet<Point3D>();

						//for each tree location in the lookup table
						foreach (var treeLocationKvp in mapLookupTable)
						{
							if (BaseHarvestablePhase.MasterHarvestablePhaseLookupByItemIdList.ContainsKey(treeLocationKvp.Value))
							{
								//look up the current phase
								var existingTileFound = false;

								foreach (var tile in map.Tiles.GetStaticTiles(treeLocationKvp.Key.X, treeLocationKvp.Key.Y))
								{
									if (tile.Z == treeLocationKvp.Key.Z) // if the z altitude matches
									{
										if (BaseHarvestablePhase.MasterHarvestablePhaseLookupByItemIdList.TryGetValue(tile.ID, out var currentPhase)) //if the item id is linked to a phase
										{
											foreach (var assetSet in currentPhase.BaseAssetSets)
											{
												if (assetSet.Length > 0 && assetSet[0].ItemID == tile.ID)
												{
													existingTileFound = true;
												}
											}

											if (existingTileFound && !currentPhase.Grow(treeLocationKvp.Key, map, ref mapOperations))
											{
												locationsToRemove.Add(treeLocationKvp.Key);
											}

											break;
										}
									}
								}

								//nothing to grow at this location, so construct the starting phase
								if (!existingTileFound)
								{
									//lookup original phase that was saved
									var grownPhase = BaseHarvestablePhase.MasterHarvestablePhaseLookupByItemIdList[treeLocationKvp.Value];

									//cleanup final harvest graphics
									if (grownPhase.FinalHarvestPhase != null && BaseHarvestablePhase.MasterHarvestablePhaseLookupByTypeList.TryGetValue(grownPhase.FinalHarvestPhase, out var phase1))
									{
										phase1.Teardown(treeLocationKvp.Key, map, ref mapOperations);
									}

									if (grownPhase.StartingGrowthPhase != null && BaseHarvestablePhase.MasterHarvestablePhaseLookupByTypeList.TryGetValue(grownPhase.StartingGrowthPhase, out var phase2))
									{
										//remove stump
										if (grownPhase is BaseTreeHarvestPhase maturePhase)
										{
											if (mapOperations != null)
											{
												mapOperations.Add(new DeleteStatic(map.MapID, new StaticTarget(treeLocationKvp.Key, maturePhase.StumpGraphic)));
											}
											else
											{
												mapOperations = new MapOperationSeries(new DeleteStatic(map.MapID, new StaticTarget(treeLocationKvp.Key, maturePhase.StumpGraphic)), map.MapID);
											}
										}

										_ = phase2.Construct(treeLocationKvp.Key, map, ref mapOperations);
									}
								}
							}
						}

						mapOperations?.DoOperation();

						foreach (var p in locationsToRemove)
						{
							_ = RegrowthMasterLookupTable[map.MapID].Remove(p);
						}
					}
					#endregion

					using var writer = new BinaryFileWriter(Path.Combine(FacetEditingSettings.LumberHarvestFallenTreeSaveLocation, "TreeLocations." + map.MapID), true);

					foreach (var kvp in mapLookupTable)
					{
						writer.Write(kvp.Key);
						writer.Write(kvp.Value);
					}

					writer.Flush();
					writer.Close();
				}
			}

			if (updateRegrowthTime)
			{
				LastGrowth = DateTime.UtcNow;
			}
		}

		private static void ModifyLumberjacking()
		{
			var def = Lumberjacking.System.Definition;

			var tiles = BaseHarvestablePhase.MasterHarvestablePhaseLookupByItemIdList.Keys.ToArray();

			for (var i = 0; i < tiles.Length; ++i)
			{
				tiles[i] += 0x4000;
			}

			Array.Sort(tiles);

			def.Tiles = tiles;

			def.Resources = new[]
			{
				new HarvestResource(00.0, 00.0, 100.0, 500498, typeof(Log)),
			};

			def.BonusResources = new[]
			{
				new BonusHarvestResource(0, 83.9, null, null),
			};

			def.Veins = new[]
			{
				new HarvestVein(49.0, 0.0, def.Resources[0], null),
			};
		}

		public static bool SpecialHarvest(Mobile from, IHarvestTool tool, HarvestDefinition def, object toHarvest, int tileID, Map map, Point3D loc)
		{
			if (!FacetEditingSettings.LumberHarvestModuleEnabled)
			{
				return false;
			}

			if (from.CheckSkill(def.Skill, 0, 120))
			{
				var hTreePhase = BaseHarvestablePhase.LookupPhase(tileID);

				_ = hTreePhase.Harvest(from, tileID, tool, loc, map);

				if (tool is IUsesRemaining toolWithUses)
				{
					toolWithUses.ShowUsesRemaining = true;

					if (toolWithUses.UsesRemaining > 0)
					{
						--toolWithUses.UsesRemaining;
					}

					if (toolWithUses.UsesRemaining < 1)
					{
						tool.Delete();

						def.SendMessageTo(from, def.ToolBrokeMessage);
					}
				}
			}

			return true;
		}
	}

	public class HarvestGraphicAsset
	{
		public int ItemID;
		public short XOffset;
		public short YOffset;
		public int HarvestResourceBaseAmount = 0;
		public int BonusResourceBaseAmount = 0;

		//Graphics are linked to bonus resources only. Regular harvest resources should be assigned at the phase level.
		public Dictionary<int, BonusHarvestResource[]> BonusResources = new();

		public HarvestGraphicAsset(int itemId, short xOff, short yOff)
		{
			ItemID = itemId;
			XOffset = xOff;
			YOffset = yOff;
		}

		public virtual List<Item> ReapBonusResources(int hue, Mobile from)
		{
			return ReapBonusResources(hue, from, BonusResourceBaseAmount, BonusResources);
		}

		//The bonus resource list can be specified at the phase level by using this overloaded method.
		public static List<Item> ReapBonusResources(int hue, Mobile from, int bonusResourceAmount, Dictionary<int, BonusHarvestResource[]> bonusList)
		{
			var bonusItems = new List<Item>();
			BonusHarvestResource[] bonusResourceList = null;

			if (bonusResourceAmount > 0)
			{
				if (bonusList.ContainsKey(hue))
				{
					bonusResourceList = bonusList[hue];
				}
				else if (bonusList.ContainsKey(0))
				{
					bonusResourceList = bonusList[0];
				}

				if (bonusResourceList != null && bonusResourceList.Length > 0)
				{
					var skillBase = from.Skills[SkillName.Lumberjacking].Base;

					foreach (var resource in bonusResourceList)
					{
						if (skillBase >= resource.ReqSkill && Utility.RandomDouble() <= resource.Chance)
						{
							try
							{
								if (Activator.CreateInstance(resource.Type) is Item item)
								{
									item.Amount = bonusResourceAmount;

									bonusItems.Add(item);
								}
							}
							catch
							{
							}
						}
					}
				}
			}

			return bonusItems;
		}
	}

	/// Life Cycle Phases
	public class BaseLeafHandlingPhase : BaseTreeHarvestPhase
	{
		public const int FALLING_LEAF_HUE = 1436;

		public BaseLeafHandlingPhase()
		{
		}

		public override bool Harvest(Mobile from, int itemId, IHarvestTool tool, Point3D harvestTargetLocation, Map map)
		{
			var trunkLocation = LookupOriginLocation(harvestTargetLocation, itemId);

			//search for leaves
			MapOperationSeries mapActions = null;
			var leavesRemoved = RemoveAssets(map, trunkLocation, ref mapActions, LeafSets);

			if (mapActions != null)
			{
				//handle falling leaves

				var numLeaves = Utility.Random(5);
				Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(trunkLocation.X, trunkLocation.Y, trunkLocation.Z + 20), map),
						  new Entity(Serial.Zero, new Point3D(trunkLocation.X, trunkLocation.Y, trunkLocation.Z), map),
						  0x1B1F, 1, 0, false, false, FALLING_LEAF_HUE, 0, 0, 1, 0, EffectLayer.Waist, 0x486);

				for (var i = 0; i < numLeaves + 9; i++)
				{
					new FadingLeaf().MoveToWorld(new Point3D(trunkLocation.X + Utility.Random(4) - 2, trunkLocation.Y + Utility.Random(4) - 2, trunkLocation.Z), map);
					Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(trunkLocation.X + Utility.Random(4) - 2, trunkLocation.Y + Utility.Random(4) - 2, trunkLocation.Z + 20), map),
									  new Entity(Serial.Zero, new Point3D(trunkLocation.X + Utility.Random(4) - 2, trunkLocation.Y + Utility.Random(4) - 2, trunkLocation.Z), map),
									  0x1B1F, 1, 0, false, false, FALLING_LEAF_HUE, 0, 0, 1, 0, EffectLayer.Waist, 0x486);
				}

				//give out leaf bonus asset resources
				foreach (var assetPair in leavesRemoved)
				{
					foreach (var itm in assetPair.Value.ReapBonusResources(assetPair.Key, from))
					{
						_ = from.AddToBackpack(itm);
					}
				}
			}
			else //if there are no leaves left, then remove the trunk pieces and call nextPhase.construct
			{
				_ = Harvest(from, itemId, tool, harvestTargetLocation, map, ref mapActions);
			}

			mapActions?.DoOperation();

			return false;
		}
	}

	public class BaseTreeHarvestPhase : BaseHarvestablePhase
	{
		public const int LEAF_HUE = 0;

		public virtual int StumpGraphic => 0xE57;
		public virtual bool AddStump => true;

		public List<HarvestGraphicAsset[]> LeafSets = new();

		public override void RegisterBasicPhaseTiles()
		{
			base.RegisterBasicPhaseTiles();

			//Add all the leaf graphics for this phase
			foreach (var leafSet in LeafSets)
			{
				RegisterAssetSet(leafSet);
			}
		}

		public override Point3D LookupOriginLocation(Point3D harvestTargetLocation, int harvestTargetItemId)
		{
			var originLocation = base.LookupOriginLocation(harvestTargetLocation, harvestTargetItemId);

			if (originLocation == Point3D.Zero)
			{
				foreach (var leafSet in LeafSets)
				{
					foreach (var asset in leafSet)
					{
						if (asset.ItemID == harvestTargetItemId)
						{
							return new Point3D(harvestTargetLocation.X + asset.XOffset, harvestTargetLocation.Y + asset.YOffset, harvestTargetLocation.Z - TileData.ItemTable[harvestTargetItemId].CalcHeight);
						}
					}
				}
			}

			return originLocation;
		}

		public override int Construct(Point3D location, Map map, ref MapOperationSeries mapOperationSeries)
		{
			var trunkHue = base.Construct(location, map, ref mapOperationSeries);

			HarvestGraphicAsset[] leafSet = null;

			if (LeafSets != null && LeafSets.Count > 0)
			{
				//leafSet = LeafSets[Utility.Random(LeafSets.Count)];
				leafSet = LeafSets[0];
			}

			if (leafSet != null)
			{
				foreach (var asset in leafSet)
				{
					//come back and add a usestatic boolean to this class and add it into the constructor too
					var addStatic = new AddStatic(map.MapID, asset.ItemID, location.Z, location.X + asset.XOffset, location.Y + asset.YOffset, LEAF_HUE);

					if (mapOperationSeries != null)
					{
						mapOperationSeries.Add(addStatic);
					}
					else
					{
						mapOperationSeries = new MapOperationSeries(addStatic, map.MapID);
					}
				}
			}

			return trunkHue;
		}

		public override void Teardown(Point3D originLocation, Map map, ref MapOperationSeries mapActions)
		{
			base.Teardown(originLocation, map, ref mapActions);

			_ = RemoveAssets(map, originLocation, ref mapActions, LeafSets);
		}

		public override bool Harvest(Mobile from, int itemId, IHarvestTool tool, Point3D harvestTargetLocation, Map map)
		{
			MapOperationSeries mapActions = null;

			var successfulHarvest = Harvest(from, itemId, tool, harvestTargetLocation, map, ref mapActions);

			mapActions?.DoOperation();

			return successfulHarvest;
		}

		//This only gets called if the NextHarvestPhase is not null
		public virtual void RecordTreeLocationAndGraphic(int mapId, int itemId, Point3D trunkLocation)
		{
			if (!FacetModule_Lumberjacking.RegrowthMasterLookupTable.TryGetValue(mapId, out var table) || table == null)
			{
				FacetModule_Lumberjacking.RegrowthMasterLookupTable[mapId] = table = new Dictionary<Point3D, int>();
			}

			table[trunkLocation] = itemId;
		}

		public bool Harvest(Mobile from, int itemId, IHarvestTool tool, Point3D harvestTargetLocation, Map map, ref MapOperationSeries operationSeries)
		{
			var trunkLocation = LookupOriginLocation(harvestTargetLocation, itemId);

			List<KeyValuePair<int, HarvestGraphicAsset>> phasePiecesRemoved;

			//If the next phase is not null, tear it all down and construct the next phase
			if (NextHarvestPhase != null)
			{
				phasePiecesRemoved = RemoveAssets(map, trunkLocation, ref operationSeries, BaseAssetSets);

				var constructedTreeHue = 0;

				if (MasterHarvestablePhaseLookupByTypeList.TryGetValue(NextHarvestPhase, out var next))
				{
					constructedTreeHue = next.Construct(trunkLocation, map, ref operationSeries);
				}

				if (operationSeries != null)
				{
					RecordTreeLocationAndGraphic(map.MapID, BaseAssetSets[0][0].ItemID, trunkLocation);

					if (AddStump)
					{
						operationSeries.Add(new AddStatic(map.MapID, StumpGraphic, trunkLocation.Z, trunkLocation.X, trunkLocation.Y, constructedTreeHue));
					}
				}
			}
			else //the next phase is not null, so destroy one asset at a time
			{
				var asset = LookupAsset(itemId);

				var assetsToRemove = new List<HarvestGraphicAsset[]>
				{
					new HarvestGraphicAsset[] { asset }
				};

				phasePiecesRemoved = RemoveAssets(map, trunkLocation, ref operationSeries, assetsToRemove);
			}

			var system = tool?.HarvestSystem as HarvestSystem ?? Lumberjacking.System;

			var hue = 0;

			//give out phase resource for each graphic asset removed
			foreach (var assetPair in phasePiecesRemoved)
			{
				hue = assetPair.Key;

				var item = ReapResource(assetPair.Key, from, assetPair.Value.HarvestResourceBaseAmount, system, tool, harvestTargetLocation, map);

				if (item != null)
				{
					var itemAmount = item.Amount;

					if (system.Give(from, item, true))
					{
						EventSink.InvokeHarvestedItem(new HarvestedItemEventArgs(from, item, itemAmount, system, tool));
					}
					else
					{
						item.Delete();
					}
				}
			}

			//give out asset bonus resources
			foreach (var item in ReapBonusResources(hue, from))
			{
				var itemAmount = item.Amount;

				if (system.Give(from, item, true))
				{
					EventSink.InvokeHarvestedItem(new HarvestedItemEventArgs(from, item, itemAmount, system, tool));
				}
				else
				{
					item.Delete();
				}
			}

			return operationSeries != null;
		}
	}

	public abstract class BaseHarvestablePhase
	{
		public const int STATIC_TILE_SEARCH_Z_TOLERANCE = 5;

		public virtual Type FinalHarvestPhase => null;
		public virtual Type StartingGrowthPhase => null;
		public virtual Type NextHarvestPhase => null;
		public virtual Type NextGrowthPhase => null;
		public virtual bool ConstructUsingHues => false;
		public virtual int HarvestResourceBaseAmount => 0;
		public virtual int BonusResourceBaseAmount => 0;
		public virtual SkillName HarvestSkill => SkillName.Lumberjacking;

		public Dictionary<int, HarvestResource> PhaseResources { get; } = new();
		public Dictionary<int, BonusHarvestResource[]> BonusResources { get; } = new();
		public List<HarvestGraphicAsset[]> BaseAssetSets { get; } = new();

		public BaseHarvestablePhase()
		{
		}

		public delegate void UpdateTilesEventHandler(int[] newTiles);

		public static event UpdateTilesEventHandler UpdatedTilesEvent;

		/// Registration and Asset Lookup
		public static Dictionary<int, HarvestGraphicAsset> MasterHarvestableAssetlookup { get; } = new();
		public static Dictionary<int, BaseHarvestablePhase> MasterHarvestablePhaseLookupByItemIdList { get; } = new();
		public static Dictionary<Type, BaseHarvestablePhase> MasterHarvestablePhaseLookupByTypeList { get; } = new();

		public static bool RegisterHarvestablePhase(BaseHarvestablePhase phase)
		{
			var alreadyRegistered = true;

			var type = phase.GetType();

			if (!MasterHarvestablePhaseLookupByTypeList.TryGetValue(type, out var existingPhase) || existingPhase == null)
			{
				MasterHarvestablePhaseLookupByTypeList[type] = phase;

				alreadyRegistered = false;

				phase.RegisterBasicPhaseTiles();

				//send out update tiles event
				UpdatedTilesEvent?.Invoke(MasterHarvestablePhaseLookupByItemIdList.Keys.ToArray());
			}
			else
			{
				Console.WriteLine($"NOT Registering {type}");
			}

			return alreadyRegistered;
		}

		public virtual void RegisterAssetSet(HarvestGraphicAsset[] assetSet)
		{
			foreach (var treeAsset in assetSet)
			{
				MasterHarvestablePhaseLookupByItemIdList[treeAsset.ItemID] = this;
				MasterHarvestableAssetlookup[treeAsset.ItemID] = treeAsset;
			}
		}

		public virtual void RegisterBasicPhaseTiles()
		{
			//Add all the trunk graphics for this phase
			foreach (var assetSet in BaseAssetSets)
			{
				RegisterAssetSet(assetSet);
			}
		}

		/// Lookups
		public virtual Point3D LookupOriginLocation(Point3D harvestTargetLocation, int harvestTargetItemId)
		{
			foreach (var trunkSet in BaseAssetSets)
			{
				foreach (var asset in trunkSet)
				{
					if (asset.ItemID == harvestTargetItemId)
					{
						return new Point3D(harvestTargetLocation.X - asset.XOffset, harvestTargetLocation.Y - asset.YOffset, harvestTargetLocation.Z - TileData.ItemTable[harvestTargetItemId].CalcHeight);
					}
				}
			}

			return Point3D.Zero;
		}

		public static HarvestGraphicAsset LookupAsset(int itemId)
		{
			MasterHarvestableAssetlookup.TryGetValue(itemId, out var asset);

			return asset;
		}

		public static BaseHarvestablePhase LookupPhase(int itemId)
		{
			MasterHarvestablePhaseLookupByItemIdList.TryGetValue(itemId, out var phase);

			return phase;
		}

		public static int LookupStaticHue(Map map, Point3D staticLocation, int itemId)
		{
			var hue = 0;

			foreach (var tile in map.Tiles.GetStaticTiles(staticLocation.X, staticLocation.Y))
			{
				if (tile.Z >= staticLocation.Z - STATIC_TILE_SEARCH_Z_TOLERANCE && tile.Z <= staticLocation.Z + STATIC_TILE_SEARCH_Z_TOLERANCE)
				{
					if (tile.ID == itemId)
					{
						hue = tile.Hue;
					}
				}
			}

			return hue;
		}

		public static bool LookupStaticTile(Map map, Point3D staticLocation, int itemId, ref StaticTile tileResult)
		{
			foreach (var tile in map.Tiles.GetStaticTiles(staticLocation.X, staticLocation.Y))
			{
				if (tile.Z >= staticLocation.Z - STATIC_TILE_SEARCH_Z_TOLERANCE && tile.Z <= staticLocation.Z + STATIC_TILE_SEARCH_Z_TOLERANCE)
				{
					if (tile.ID == itemId)
					{
						tileResult = tile;
						return true;
					}
				}
			}

			return false;
		}

		/// Construction / Teardown
		public static List<KeyValuePair<int, HarvestGraphicAsset>> RemoveAssets(Map map, Point3D trunkLocation, ref MapOperationSeries deleteTreePartsOperationSeries, List<HarvestGraphicAsset[]> assetList)
		{
			var treePartsRemoved = new List<KeyValuePair<int, HarvestGraphicAsset>>();

			foreach (var assetSet in assetList)
			{
				foreach (var treeAsset in assetSet)
				{
					var leafX = trunkLocation.X + treeAsset.XOffset;
					var leafY = trunkLocation.Y + treeAsset.YOffset;

					foreach (var tile in map.Tiles.GetStaticTiles(leafX, leafY))
					{
						if (tile.Z >= trunkLocation.Z - STATIC_TILE_SEARCH_Z_TOLERANCE && tile.Z <= trunkLocation.Z + STATIC_TILE_SEARCH_Z_TOLERANCE)
						{
							if (tile.ID == treeAsset.ItemID)
							{
								var leafLocation = new Point3D(leafX, leafY, tile.Z);

								if (deleteTreePartsOperationSeries == null)
								{
									deleteTreePartsOperationSeries = new MapOperationSeries(new DeleteStatic(map.MapID, new StaticTarget(leafLocation, treeAsset.ItemID)), map.MapID);
								}
								else
								{
									deleteTreePartsOperationSeries.Add(new DeleteStatic(map.MapID, new StaticTarget(leafLocation, treeAsset.ItemID)));
								}

								treePartsRemoved.Add(new KeyValuePair<int, HarvestGraphicAsset>(tile.Hue, treeAsset));

								continue;
							}
						}
					}
				}
			}

			return treePartsRemoved;
		}

		public void Construct(Point3D location, Map map)
		{
			MapOperationSeries series = null;

			_ = Construct(location, map, ref series);

			series?.DoOperation();
		}

		public virtual void OnBeforeConstruct(Point3D location, Map map, ref MapOperationSeries mapOperationSeries)
		{
		}

		/*
		 * Return value is the HUE
		 */
		public virtual int Construct(Point3D location, Map map, ref MapOperationSeries mapOperationSeries)
		{
			OnBeforeConstruct(location, map, ref mapOperationSeries);

			HarvestGraphicAsset[] trunkSet = null;

			if (BaseAssetSets != null && BaseAssetSets.Count > 0)
			{
				trunkSet = BaseAssetSets[Utility.Random(BaseAssetSets.Count)];
			}

			var hue = 0;

			var possibleHues = new List<int>(PhaseResources.Keys);

			if (ConstructUsingHues && possibleHues.Count > 0)
			{
				hue = possibleHues[Utility.Random(possibleHues.Count)];
			}

			if (trunkSet != null)
			{
				foreach (var asset in trunkSet)
				{
					//come back and add a usestatic boolean to this class and add it into the constructor too
					var addStatic = new AddStatic(map.MapID, asset.ItemID, location.Z, location.X + asset.XOffset, location.Y + asset.YOffset, hue);

					if (mapOperationSeries == null)
					{
						mapOperationSeries = new MapOperationSeries(addStatic, map.MapID);
					}
					else
					{
						mapOperationSeries.Add(addStatic);
					}
				}
			}

			return hue;
		}

		public void Teardown(Point3D originLocation, Map map)
		{
			MapOperationSeries series = null;

			Teardown(originLocation, map, ref series);

			series?.DoOperation();
		}

		public virtual void Teardown(Point3D originLocation, Map map, ref MapOperationSeries mapActions)
		{
			_ = RemoveAssets(map, originLocation, ref mapActions, BaseAssetSets);
		}

		/// Resource Reap Methods
		public virtual Item ReapResource(int hue, Mobile from, int amount, HarvestSystem system, IHarvestTool tool, Point3D loc, Map map)
		{
			Item resourceItem = null;
			HarvestResource resource = null;

			if (amount > 0)
			{
				if (PhaseResources.ContainsKey(hue))
				{
					resource = PhaseResources[hue];
				}
				else if (PhaseResources.ContainsKey(0))
				{
					resource = PhaseResources[0];
				}

				if (resource != null)
				{
					var skillBase = from.Skills[HarvestSkill].Base;

					if (skillBase >= resource.ReqSkill)
					{
						try
						{
							var type = resource.Types[Utility.Random(resource.Types.Length)];

							type = from.Region.GetResource(from, tool, map, loc, system, type);

							if (Activator.CreateInstance(type) is Item item)
							{
								item.Amount = amount;
								resourceItem = item;
							}
						}
						catch
						{
						}
					}
					else
					{
						//TODO: Inform player they don't have enough skill using a cliloc to do it
						from.SendMessage("you don't have enough skill to harvest that!");
					}
				}
			}

			return resourceItem;
		}

		public virtual List<Item> ReapBonusResources(int hue, Mobile from)
		{
			return HarvestGraphicAsset.ReapBonusResources(hue, from, BonusResourceBaseAmount, BonusResources);
		}

		#region Developer Notations

		///     This is the harvest function for a phase.  If a next harvest phase is defined, this function tears down all the assets associated
		///     with the phase and constructs the next growth phase.  It also optionally gives any phase resources out according to the base 
		///     amount specified. It then optionally gives out any bonus resources according to the amount specified.
		///     
		///     If the next harvest phase has not been defined, it removes the asset that was targetted and gives out an amount of phase 
		///     resources specified in the graphic asset. It then gives out any bonus resources associated with the graphic.
		///     
		///                          Harvest                   Harvest
		///     
		///     +-----------------+      +------------------+      +------------------+
		///     | GrownTreePhase2 +----->| FallenTreePhase1 +----->| FallenTreePhase2 +-------+
		///     +-----------------+      +------------------+      +------------------+       |
		///                                                                 ^                 |
		///                                                                 |     Harvest     |
		///                                                                 |                 |
		///                                                                 +-----------------+
		///     
		///     

		#endregion

		public virtual bool Harvest(Mobile from, int itemId, IHarvestTool tool, Point3D location, Map map)
		{
			return false;
		}

		#region Developer Notations

		///		This is the growth function for a phase. If a next growth phase is defined, this function tears down all the assets associated with the
		///     phase and constructs the next growth phase. If the next phase has not been defined, this function assumes that the phase has finished  
		///     growing, and does nothing.
		///     
		///                           Grow                     Grow                      Grow                     Grow
		///     
		///        +----------------+       +----------------+       +-----------------+      +-----------------+
		///        | SaplingPhase1  +------>| SaplingPhase2  +------>| GrownTreePhase1 +----->| GrownTreePhase2 |---------->( Stop )
		///        +----------------+       +----------------+       +-----------------+      +-----------------+
		///        
		///     

		#endregion

		public virtual bool Grow(Point3D originLocation, Map map)
		{
			MapOperationSeries series = null;

			var grew = Grow(originLocation, map, ref series);

			series?.DoOperation();

			return grew;
		}

		public virtual bool Grow(Point3D originLocation, Map map, ref MapOperationSeries series)
		{
			var grew = false;

			if (NextGrowthPhase != null)
			{
				Teardown(originLocation, map, ref series);

				MasterHarvestablePhaseLookupByTypeList[NextGrowthPhase].Construct(originLocation, map); //this is the transition to the next phase

				grew = true;
			}

			return grew;
		}
	}

	/// Fallen Tree Leaves
	public class FadingLeaf : Item
	{
		[Constructable]
		public FadingLeaf() 
			: this(Utility.RandomList(0x1B1F, 0x1B20, 0x1B21, 0x1B22, 0x1B23, 0x1B24, 0x1B25))
		{
		}

		[Constructable]
		public FadingLeaf(int itemID) 
			: base(itemID)
		{
			Movable = false;
			Hue = 1436;

			Timer.DelayCall(TimeSpan.FromSeconds(5.0), Delete);
		}

		public FadingLeaf(Serial serial) 
			: base(serial)
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

			Timer.DelayCall(Delete);
		}
	}

	/// Chopped Fallen Tree
	public class SmallFallenTreeNorthSouth : BaseTreeHarvestPhase
	{
		public static void Configure()
		{
			_ = RegisterHarvestablePhase(new SmallFallenTreeNorthSouth());
		}

		private SmallFallenTreeNorthSouth()
		{
			var logResource = new HarvestResource(00.0, 00.0, 100.0, 1072540, typeof(Log));
			var oakResource = new HarvestResource(65.0, 25.0, 105.0, 1072541, typeof(OakLog));
			var ashResource = new HarvestResource(80.0, 40.0, 120.0, 1072542, typeof(AshLog));
			var yewResource = new HarvestResource(95.0, 55.0, 135.0, 1072543, typeof(YewLog));
			var heartwoodResource = new HarvestResource(100.0, 60.0, 140.0, 1072544, typeof(HeartwoodLog));
			var bloodwoodResource = new HarvestResource(100.0, 60.0, 140.0, 1072545, typeof(BloodwoodLog));
			var frostwoodResource = new HarvestResource(100.0, 60.0, 140.0, 1072546, typeof(FrostwoodLog));

			PhaseResources.Add(0, logResource);
			PhaseResources.Add(350, oakResource);
			PhaseResources.Add(751, ashResource);
			PhaseResources.Add(545, yewResource);
			PhaseResources.Add(436, heartwoodResource);
			PhaseResources.Add(339, bloodwoodResource);
			PhaseResources.Add(688, frostwoodResource);

			var asset1 = new HarvestGraphicAsset(0xCF4, 0, -1)
			{
				HarvestResourceBaseAmount = 10
			};

			var asset2 = new HarvestGraphicAsset(0xCF3, 0, -2)
			{
				HarvestResourceBaseAmount = 10
			};

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2 });
		}
	}

	public class SmallFallenTreeEastWest : BaseTreeHarvestPhase
	{
		public static void Configure()
		{
			_ = RegisterHarvestablePhase(new SmallFallenTreeEastWest());
		}

		private SmallFallenTreeEastWest()
		{
			var logResource = new HarvestResource(00.0, 00.0, 100.0, 1072540, typeof(Log));
			var oakResource = new HarvestResource(65.0, 25.0, 105.0, 1072541, typeof(OakLog));
			var ashResource = new HarvestResource(80.0, 40.0, 120.0, 1072542, typeof(AshLog));
			var yewResource = new HarvestResource(95.0, 55.0, 135.0, 1072543, typeof(YewLog));
			var heartwoodResource = new HarvestResource(100.0, 60.0, 140.0, 1072544, typeof(HeartwoodLog));
			var bloodwoodResource = new HarvestResource(100.0, 60.0, 140.0, 1072545, typeof(BloodwoodLog));
			var frostwoodResource = new HarvestResource(100.0, 60.0, 140.0, 1072546, typeof(FrostwoodLog));

			PhaseResources.Add(0, logResource);
			PhaseResources.Add(350, oakResource);
			PhaseResources.Add(751, ashResource);
			PhaseResources.Add(545, yewResource);
			PhaseResources.Add(436, heartwoodResource);
			PhaseResources.Add(339, bloodwoodResource);
			PhaseResources.Add(688, frostwoodResource);

			var asset1 = new HarvestGraphicAsset(0xCF5, -3, 0)
			{
				HarvestResourceBaseAmount = 10
			};

			var asset2 = new HarvestGraphicAsset(0xCF6, -2, 0)
			{
				HarvestResourceBaseAmount = 10
			};

			var asset3 = new HarvestGraphicAsset(0xCF7, -1, 0)
			{
				HarvestResourceBaseAmount = 10
			};

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3 });
		}
	}

	/// Tree ReGrowth Phases
	public class SmallSaplingTreePhase1 : BaseTreeHarvestPhase
	{
		public static void Configure()
		{
			_ = RegisterHarvestablePhase(new SmallSaplingTreePhase1());
		}

		public override bool AddStump => false;

		public override Type NextGrowthPhase => typeof(SmallSaplingTreePhase2);
		public override Type StartingGrowthPhase => typeof(SmallSaplingTreePhase1);

		private SmallSaplingTreePhase1()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0xCEA, 0, 0) });
		}

		public override void RecordTreeLocationAndGraphic(int mapId, int itemId, Point3D trunkLocation)
		{
			//don't record it
		}
	}

	public class SmallSaplingTreePhase2 : BaseTreeHarvestPhase
	{
		public static void Configure()
		{
			_ = RegisterHarvestablePhase(new SmallSaplingTreePhase2());
		}

		public override bool AddStump => false;

		public override Type StartingGrowthPhase => typeof(SmallSaplingTreePhase1);

		private SmallSaplingTreePhase2()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0xCE9, 0, 0) });
		}

		public override void RecordTreeLocationAndGraphic(int mapId, int itemId, Point3D trunkLocation)
		{
			//don't record it, if it gets harvested, we want to leave the original location that was written by the full grown tree
		}

		public override bool Grow(Point3D originLocation, Map map, ref MapOperationSeries series)
		{
			var grew = false;

			if (FacetModule_Lumberjacking.RegrowthMasterLookupTable.TryGetValue(map.MapID, out var mapRegrowthLookupTable))
			{
				//if we can't find the original lookup, then we don't grow
				if (mapRegrowthLookupTable.TryGetValue(originLocation, out var originalItemId))
				{
					Teardown(originLocation, map, ref series);

					MasterHarvestablePhaseLookupByItemIdList[originalItemId].Construct(originLocation, map);
				}
			}

			return grew;
		}
	}
}