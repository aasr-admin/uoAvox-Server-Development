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
	public class OhiiTree : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 0xC9E }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { }; } }
		public static void Configure() { RegisterHarvestablePhase(new OhiiTree()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3C7E; } }
    public override Type NextHarvestPhase { get { return typeof(FallenOhiiTree); } }
    public override Type FinalHarvestPhase { get { return typeof(FallenOhiiTree); } }
    public override Type StartingGrowthPhase { get { return typeof(OhiiSapling1); } }
#endif
	}

	public class OhiiSapling1 : BaseTreeHarvestPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new OhiiSapling1()); }
		public override Type NextGrowthPhase { get { return typeof(OhiiSapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(OhiiSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenOhiiTree); } }

		public OhiiSapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C7F, 0, 0) });
		}
	}

	public class OhiiSapling2 : BaseTreeHarvestPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new OhiiSapling2()); }
		public override Type NextGrowthPhase { get { return typeof(OhiiTree); } }
		public override Type StartingGrowthPhase { get { return typeof(OhiiSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenOhiiTree); } }

		public OhiiSapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C80, 0, 0) });
		}
	}

	public class FallenOhiiTree : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenOhiiTree()); }

		public FallenOhiiTree()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3C83, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3C84, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3C85, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3C86, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3C87, -4, -1);
			asset5.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3C88, -3, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3C89, -2, -1);
			asset7.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3C8A, -4, 1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3C8B, -3, 1);
			asset9.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3C8C, -2, 1);
			asset10.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10 });
		}
	}
}