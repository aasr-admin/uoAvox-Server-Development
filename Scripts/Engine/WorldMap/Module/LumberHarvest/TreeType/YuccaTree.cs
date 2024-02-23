//#define CUSTOM_TREE_GRAPHICS

using Server;
using Server.Engine.Facet;
using Server.Engines.Harvest;
using Server.Items;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.Engine.Facet.Module.LumberHarvest
{
	/// Yucca Tree 1
	public class YuccaTree : BaseTreeHarvestPhase
	{
		public override int StumpGraphic { get { return 0x0DAC; } }
		public static void Configure() { RegisterHarvestablePhase(new YuccaTree()); }

		public override Type NextHarvestPhase { get { return typeof(SmallFallenTreeEastWest); } }
		public override Type StartingGrowthPhase { get { return typeof(SmallSaplingTreePhase1); } }
		public override Type FinalHarvestPhase { get { return typeof(SmallFallenTreeEastWest); } }


		public YuccaTree()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(3383, 0, 0) });
			PhaseResources.Add(0, new HarvestResource(00.0, 00.0, 100.0, 1072540, typeof(Log)));
		}
	}

	/// Yucca Tree 2
	public class YuccaTree2 : BaseTreeHarvestPhase
	{
		public override Type StartingGrowthPhase { get { return typeof(YuccaTree2); } }
		public override int StumpGraphic { get { return 0x0DAC; } }
		public static void Configure() { RegisterHarvestablePhase(new YuccaTree2()); }

		public YuccaTree2()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(3384, 0, 0) });
			PhaseResources.Add(0, new HarvestResource(00.0, 00.0, 100.0, 1072540, typeof(Log)));
		}
	}
}