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
	/// Walnut Tree 1
	public class WalnutTree : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3296 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3297, 3298 }; } }
		public static void Configure() { RegisterHarvestablePhase(new WalnutTree()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3C43; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenWalnutTree); } }
    public override Type NextHarvestPhase { get { return typeof(FallenWalnutTree); } }
    public override Type StartingGrowthPhase { get { return typeof(WalnutTreeSapling1); } }
#endif
	}

	public class WalnutTreeSapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new WalnutTreeSapling1()); }
		public override Type NextGrowthPhase { get { return typeof(WalnutTreeSapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(WalnutTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenWalnutTree); } }

		public WalnutTreeSapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C44, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C45, 0, 0) });
		}
	}

	public class WalnutTreeSapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new WalnutTreeSapling2()); }
		public override Type NextGrowthPhase { get { return typeof(WalnutTree); } }
		public override Type StartingGrowthPhase { get { return typeof(WalnutTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenWalnutTree); } }

		public WalnutTreeSapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C46, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C47, 0, 0) });
		}
	}

	public class FallenWalnutTree : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenWalnutTree()); }

		public FallenWalnutTree()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3C48, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3C49, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3C4A, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3C4B, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3C4C, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3C4D, -6, 0);
			asset6.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3C4E, -5, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3C4F, -4, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3C50, -3, -1);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3C51, -5, 1);
			asset10.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3C52, -4, 1);
			asset11.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3C53, -3, 1);
			asset12.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12 });
		}
	}

	/// Walnut Tree 2
	public class WalnutTree2 : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3299 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3300, 3301 }; } }
		public static void Configure() { RegisterHarvestablePhase(new WalnutTree2()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3C54; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenWalnutTree2); } }
    public override Type NextHarvestPhase { get { return typeof(FallenWalnutTree2); } }
    public override Type StartingGrowthPhase { get { return typeof(WalnutTree2Sapling1); } }
#endif
	}

	public class WalnutTree2Sapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new WalnutTree2Sapling1()); }
		public override Type NextGrowthPhase { get { return typeof(WalnutTree2Sapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(WalnutTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenWalnutTree2); } }

		public WalnutTree2Sapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C55, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C56, 0, 0) });
		}
	}

	public class WalnutTree2Sapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new WalnutTree2Sapling2()); }
		public override Type NextGrowthPhase { get { return typeof(WalnutTree2); } }
		public override Type StartingGrowthPhase { get { return typeof(WalnutTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenWalnutTree2); } }

		public WalnutTree2Sapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C57, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C58, 0, 0) });
		}
	}

	public class FallenWalnutTree2 : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenWalnutTree2()); }

		public FallenWalnutTree2()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3C59, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3C5A, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3C5B, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3C5C, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3C5D, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3C5E, -5, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3C5F, -4, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3C60, -3, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3C61, -2, -1);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3C62, -5, -2);
			asset10.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3C63, -4, -2);
			asset11.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3C64, -3, -2);
			asset12.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset13 = new HarvestGraphicAsset(0x3C65, -2, -2);
			asset13.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset14 = new HarvestGraphicAsset(0x3C66, -5, 1);
			asset14.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset15 = new HarvestGraphicAsset(0x3C67, -4, 1);
			asset15.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset16 = new HarvestGraphicAsset(0x3C68, -3, 1);
			asset16.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset17 = new HarvestGraphicAsset(0x3C69, -2, 1);
			asset17.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12, asset13, asset14, asset15, asset16, asset17 });
		}
	}
}