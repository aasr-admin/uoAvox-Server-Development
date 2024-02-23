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
	/// Cedar Tree 1
	public class CedarTree : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3286 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3287 }; } }
		public static void Configure() { RegisterHarvestablePhase(new CedarTree()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3BE7; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenCedarTree); } }
    public override Type NextHarvestPhase { get { return typeof(FallenCedarTree); } }
    public override Type StartingGrowthPhase { get { return typeof(CedarTreeSapling1); } }
#endif
	}

	public class CedarTreeSapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CedarTreeSapling1()); }
		public override Type NextGrowthPhase { get { return typeof(CedarTreeSapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(CedarTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCedarTree); } }

		public CedarTreeSapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BE8, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BE9, 0, 0) });
		}
	}

	public class CedarTreeSapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CedarTreeSapling2()); }
		public override Type NextGrowthPhase { get { return typeof(CedarTree); } }
		public override Type StartingGrowthPhase { get { return typeof(CedarTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCedarTree); } }

		public CedarTreeSapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BEA, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BEB, 0, 0) });
		}
	}

	public class FallenCedarTree : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenCedarTree()); }

		public FallenCedarTree()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3BEC, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3BED, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3BEE, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3BEF, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3BF0, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3BF1, -6, 0);
			asset6.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3BF2, -4, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3BF3, -3, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3BF4, -2, -1);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3BF5, -3, 1);
			asset10.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3BF6, -2, 1);
			asset11.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11 });
		}
	}

	/// Cedar Tree 2
	public class CedarTree2 : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3288 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3289 }; } }
		public static void Configure() { RegisterHarvestablePhase(new CedarTree2()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3BF7; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenCedarTree2); } }
    public override Type NextHarvestPhase { get { return typeof(FallenCedarTree2); } }
    public override Type StartingGrowthPhase { get { return typeof(CedarTree2Sapling1); } }
#endif
	}

	public class CedarTree2Sapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CedarTree2Sapling1()); }
		public override Type NextGrowthPhase { get { return typeof(CedarTree2Sapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(CedarTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCedarTree2); } }

		public CedarTree2Sapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BF8, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BF9, 0, 0) });
		}
	}

	public class CedarTree2Sapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CedarTree2Sapling2()); }
		public override Type NextGrowthPhase { get { return typeof(CedarTree2); } }
		public override Type StartingGrowthPhase { get { return typeof(CedarTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCedarTree2); } }

		public CedarTree2Sapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BFA, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BFB, 0, 0) });
		}
	}

	public class FallenCedarTree2 : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenCedarTree2()); }

		public FallenCedarTree2()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3BFC, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3BFD, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3BFE, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3BFF, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3C00, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3C01, -6, 0);
			asset6.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3C02, -4, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3C03, -3, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3C04, -2, -1);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3C05, -3, 1);
			asset10.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3C06, -2, 1);
			asset11.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11 });
		}
	}
}