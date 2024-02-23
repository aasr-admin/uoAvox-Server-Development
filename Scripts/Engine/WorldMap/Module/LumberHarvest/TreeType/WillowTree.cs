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
	public class WillowTree : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3302 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3303, 3304 }; } }
		public static void Configure() { RegisterHarvestablePhase(new WillowTree()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3C6A; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenWillowTree); } }
    public override Type NextHarvestPhase { get { return typeof(FallenWillowTree); } }
    public override Type StartingGrowthPhase { get { return typeof(WillowTreeSapling1); } }
#endif
	}

	public class WillowTreeSapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new WillowTreeSapling1()); }
		public override Type NextGrowthPhase { get { return typeof(WillowTreeSapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(WillowTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenWillowTree); } }

		public WillowTreeSapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C6B, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C6C, 0, 0) });
		}
	}

	public class WillowTreeSapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new WillowTreeSapling2()); }
		public override Type NextGrowthPhase { get { return typeof(WillowTree); } }
		public override Type StartingGrowthPhase { get { return typeof(WillowTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenWillowTree); } }

		public WillowTreeSapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C6D, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C6E, 0, 0) });
		}
	}

	public class FallenWillowTree : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenWillowTree()); }

		public FallenWillowTree()
		{
			HarvestResource logResource = new HarvestResource(00.0, 00.0, 100.0, 1072540, typeof(Log));
			HarvestResource oakResource = new HarvestResource(65.0, 25.0, 105.0, 1072541, typeof(OakLog));
			HarvestResource ashResource = new HarvestResource(80.0, 40.0, 120.0, 1072542, typeof(AshLog));
			HarvestResource yewResource = new HarvestResource(95.0, 55.0, 135.0, 1072543, typeof(YewLog));
			HarvestResource heartwoodResource = new HarvestResource(100.0, 60.0, 140.0, 1072544, typeof(HeartwoodLog));
			HarvestResource bloodwoodResource = new HarvestResource(100.0, 60.0, 140.0, 1072545, typeof(BloodwoodLog));
			HarvestResource frostwoodResource = new HarvestResource(100.0, 60.0, 140.0, 1072546, typeof(FrostwoodLog));

			PhaseResources.Add(0, logResource);
			PhaseResources.Add(350, oakResource);
			PhaseResources.Add(751, ashResource);
			PhaseResources.Add(545, yewResource);
			PhaseResources.Add(436, heartwoodResource);
			PhaseResources.Add(339, bloodwoodResource);
			PhaseResources.Add(688, frostwoodResource);

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3C6F, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3C70, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3C71, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3C72, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3C73, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3C74, -4, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3C75, -3, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3C76, -2, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3C77, -1, -1);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3C78, -4, -2);
			asset10.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3C79, -3, -2);
			asset11.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3C7A, -2, -2);
			asset12.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset13 = new HarvestGraphicAsset(0x3C7B, -4, 1);
			asset13.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset14 = new HarvestGraphicAsset(0x3C7C, -3, 1);
			asset14.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset15 = new HarvestGraphicAsset(0x3C7D, -2, 1);
			asset15.HarvestResourceBaseAmount = 10;


			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12, asset13, asset14, asset15 });
		}
	}
}