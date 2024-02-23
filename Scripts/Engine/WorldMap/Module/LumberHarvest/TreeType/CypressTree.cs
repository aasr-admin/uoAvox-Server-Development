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
	/// Cypress Tree 1
	public class CypressTree : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3320 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3321, 3322 }; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3C8D; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree); } }
    public override Type NextHarvestPhase { get { return typeof(FallenCypressTree); } }
    public override Type StartingGrowthPhase { get { return typeof(CypressTreeSapling1); } }
#endif
	}

	public class CypressTreeSapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTreeSapling1()); }
		public override Type NextGrowthPhase { get { return typeof(CypressTreeSapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(CypressTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree); } }

		public CypressTreeSapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C8E, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C8F, 0, 0) });
		}
	}

	public class CypressTreeSapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTreeSapling2()); }
		public override Type NextGrowthPhase { get { return typeof(CypressTree); } }
		public override Type StartingGrowthPhase { get { return typeof(CypressTreeSapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree); } }

		public CypressTreeSapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C90, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C91, 0, 0) });
		}
	}

	public class FallenCypressTree : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenCypressTree()); }

		public FallenCypressTree()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3C92, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3C93, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3C94, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3C95, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3C96, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3C97, -5, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3C98, -4, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3C99, -3, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3C9A, -2, -1);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3C9B, -2, -2);
			asset10.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3C9C, -4, 1);
			asset11.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3C9D, -3, 1);
			asset12.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset13 = new HarvestGraphicAsset(0x3C9E, -2, 1);
			asset13.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12, asset13 });
		}
	}

	/// Cypress Tree 2
	public class CypressTree2 : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3323 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3324, 3325 }; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree2()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3C9F; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree2); } }
    public override Type NextHarvestPhase { get { return typeof(FallenCypressTree2); } }
    public override Type StartingGrowthPhase { get { return typeof(CypressTree2Sapling1); } }
#endif
	}

	public class CypressTree2Sapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree2Sapling1()); }
		public override Type NextGrowthPhase { get { return typeof(CypressTree2Sapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(CypressTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree2); } }

		public CypressTree2Sapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CA0, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CA1, 0, 0) });
		}
	}

	public class CypressTree2Sapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree2Sapling2()); }
		public override Type NextGrowthPhase { get { return typeof(CypressTree2); } }
		public override Type StartingGrowthPhase { get { return typeof(CypressTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree2); } }

		public CypressTree2Sapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CA2, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CA3, 0, 0) });
		}
	}

	public class FallenCypressTree2 : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenCypressTree2()); }

		public FallenCypressTree2()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3CA4, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3CA5, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3CA6, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3CA7, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3CA8, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3CA9, -4, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3CAA, -3, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3CAB, -2, -1);
			asset8.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3CAC, -2, -2);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3CAD, -4, 1);
			asset10.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3CAE, -3, 1);
			asset11.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11 });
		}
	}

	/// Cypress Tree 3
	public class CypressTree3 : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3326 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3327, 3328 }; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree3()); }
#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3CAF; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree3); } }
    public override Type NextHarvestPhase { get { return typeof(FallenCypressTree3); } }
    public override Type StartingGrowthPhase { get { return typeof(CypressTree3Sapling1); } }
#endif
	}

	public class CypressTree3Sapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree3Sapling1()); }
		public override Type NextGrowthPhase { get { return typeof(CypressTree3Sapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(CypressTree3Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree3); } }

		public CypressTree3Sapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CB0, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CB1, 0, 0) });
		}
	}

	public class CypressTree3Sapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree3Sapling2()); }
		public override Type NextGrowthPhase { get { return typeof(CypressTree3); } }
		public override Type StartingGrowthPhase { get { return typeof(CypressTree3Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree3); } }

		public CypressTree3Sapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CB2, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CB3, 0, 0) });
		}
	}

	public class FallenCypressTree3 : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenCypressTree3()); }

		public FallenCypressTree3()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3CB4, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3CB5, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3CB6, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3CB7, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3CB8, -5, -1);
			asset5.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3CB9, -4, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3CBA, -3, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3CBB, -2, -1);
			asset8.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3CBC, -4, -2);
			asset9.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3CBD, -3, -2);
			asset10.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3CBE, -2, -2);
			asset11.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3CBF, -4, 1);
			asset12.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset13 = new HarvestGraphicAsset(0x3CC0, -3, 1);
			asset13.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset14 = new HarvestGraphicAsset(0x3CC1, -2, 1);
			asset14.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12, asset13, asset14 });
		}
	}

	/// Cypress Tree 4
	public class CypressTree4 : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3329 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3330, 3331 }; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree4()); }
#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3CC2; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree4); } }
    public override Type NextHarvestPhase { get { return typeof(FallenCypressTree4); } }
    public override Type StartingGrowthPhase { get { return typeof(CypressTree4Sapling1); } }
#endif
	}

	public class CypressTree4Sapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree4Sapling1()); }
		public override Type NextGrowthPhase { get { return typeof(CypressTree4Sapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(CypressTree4Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree4); } }

		public CypressTree4Sapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CC3, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CC4, 0, 0) });
		}
	}

	public class CypressTree4Sapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new CypressTree4Sapling2()); }
		public override Type NextGrowthPhase { get { return typeof(CypressTree4); } }
		public override Type StartingGrowthPhase { get { return typeof(CypressTree4Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenCypressTree4); } }

		public CypressTree4Sapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CC5, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3CC6, 0, 0) });
		}
	}

	public class FallenCypressTree4 : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenCypressTree4()); }

		public FallenCypressTree4()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3CC7, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3CC8, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3CC9, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3CCA, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3CCB, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3CCC, -5, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3CCD, -4, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3CCE, -3, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3CCF, -2, -1);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3CD0, -3, -2);
			asset10.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3CD1, -5, 1);
			asset11.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3CD2, -4, 1);
			asset12.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset13 = new HarvestGraphicAsset(0x3CD3, -3, 1);
			asset13.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset14 = new HarvestGraphicAsset(0x3CD4, -2, 1);
			asset14.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset15 = new HarvestGraphicAsset(0x3CD5, -1, 1);
			asset15.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12, asset13, asset14, asset15 });
		}
	}
}