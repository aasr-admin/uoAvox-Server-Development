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
	/// Oak Tree 1
	public class OakTree : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3290 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3291, 3292 }; } }
		public static void Configure() { RegisterHarvestablePhase(new OakTree()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3BBD; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenOakTree); } }
    public override Type NextHarvestPhase { get { return typeof(FallenOakTree); } }
    public override Type StartingGrowthPhase { get { return typeof(OakTreeSapling1); } }
#endif
	}

	public class OakTreeSapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new OakTreeSapling1()); }
		public override Type NextGrowthPhase { get { return typeof(OakTreeSapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(OakTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenOakTree); } }

		public OakTreeSapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BBE, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BBF, 0, 0) });
		}
	}

	public class OakTreeSapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new OakTreeSapling2()); }
		public override Type NextGrowthPhase { get { return typeof(OakTree); } }
		public override Type StartingGrowthPhase { get { return typeof(OakTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenOakTree); } }

		public OakTreeSapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BC0, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BC1, 0, 0) });
		}
	}

	public class FallenOakTree : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenOakTree()); }

		public FallenOakTree()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3BC2, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3BC3, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3BC4, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3BC5, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3BC6, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3BC7, -5, -1);
			asset6.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3BC8, -4, -1);
			asset7.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3BC9, -3, -1);
			asset8.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3BCA, -2, -1);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3BCB, -3, -2);
			asset10.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3BCC, -5, 1);
			asset11.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3BCD, -4, 1);
			asset12.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset13 = new HarvestGraphicAsset(0x3BCE, -3, 1);
			asset13.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset14 = new HarvestGraphicAsset(0x3BCF, -2, 1);
			asset14.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12, asset13, asset14 });
		}
	}

	/// Oak Tree 2
	public class OakTree2 : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 0x0CDD }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3294, 3295 }; } }
		public static void Configure() { RegisterHarvestablePhase(new OakTree2()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3BD0; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenOakTree2); } }
    public override Type NextHarvestPhase { get { return typeof(FallenOakTree2); } }
    public override Type StartingGrowthPhase { get { return typeof(OakTree2Sapling1); } }
#endif
	}

	public class OakTree2Sapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new OakTree2Sapling1()); }
		public override Type NextGrowthPhase { get { return typeof(OakTree2Sapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(OakTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenOakTree2); } }

		public OakTree2Sapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BD1, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BD2, 0, 0) });
		}
	}

	public class OakTree2Sapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new OakTree2Sapling2()); }
		public override Type NextGrowthPhase { get { return typeof(OakTree2); } }
		public override Type StartingGrowthPhase { get { return typeof(OakTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenOakTree2); } }

		public OakTree2Sapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BD3, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3BD4, 0, 0) });
		}
	}

	public class FallenOakTree2 : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenOakTree2()); }

		public FallenOakTree2()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3BD5, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3BD6, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3BD7, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3BD8, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3BD9, -4, -1);
			asset5.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3BDA, -3, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3BDB, -2, -1);
			asset7.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3BDC, -4, -2);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3BDD, -3, -2);
			asset9.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3BDE, -2, -2);
			asset10.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3BDF, -1, 1);
			asset11.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3BE0, -2, 1);
			asset12.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset13 = new HarvestGraphicAsset(0x3BE1, -3, 1);
			asset13.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset14 = new HarvestGraphicAsset(0x3BE2, -4, 1);
			asset14.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset15 = new HarvestGraphicAsset(0x3BE3, -4, 2);
			asset15.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset16 = new HarvestGraphicAsset(0x3BE4, -3, 2);
			asset16.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset17 = new HarvestGraphicAsset(0x3BE5, -2, 2);
			asset17.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset18 = new HarvestGraphicAsset(0x3BE6, -1, 2);
			asset18.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12, asset13, asset14, asset15, asset16, asset17, asset18 });
		}
	}
}