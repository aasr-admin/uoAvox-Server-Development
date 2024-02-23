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
	/// Generic Tree 1
	public class GenericTree : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 0xCCD }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 0xCCE, 0xCCF }; } }
		public static void Configure() { RegisterHarvestablePhase(new GenericTree()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3C09; } }
    public override Type NextHarvestPhase { get { return typeof(FallenGenericTree); } }
    public override Type FinalHarvestPhase { get { return typeof(FallenGenericTree); } }
    public override Type StartingGrowthPhase { get { return typeof(GenericTreeSapling); } }
#endif
	}

	public class GenericTreeSapling : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new GenericTreeSapling()); }
		public override Type NextGrowthPhase { get { return typeof(GenericTreeSapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(GenericTreeSapling); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenGenericTree); } }

		public GenericTreeSapling()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C0A, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C0B, 0, 0) });
		}
	}

	public class GenericTreeSapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new GenericTreeSapling2()); }
		public override Type NextGrowthPhase { get { return typeof(GenericTree); } }
		public override Type StartingGrowthPhase { get { return typeof(GenericTreeSapling); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenGenericTree); } }

		public GenericTreeSapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C0C, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C0D, 0, 0) });
		}
	}

	public class FallenGenericTree : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenGenericTree()); }

		public FallenGenericTree()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3C0E, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3C0F, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3C10, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3C11, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3C12, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3C13, -5, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3C14, -4, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3C15, -3, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3C16, -2, -1);
			asset9.HarvestResourceBaseAmount = 10;


			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3C17, -3, -3);
			asset10.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3C18, -4, 1);
			asset11.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3C19, -3, 1);
			asset12.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset13 = new HarvestGraphicAsset(0x3C1A, -2, 1);
			asset13.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset14 = new HarvestGraphicAsset(0x3C1B, -3, 2);
			asset14.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12, asset13, asset14 });
		}
	}


	/// Generic Tree 2
	public class GenericTree2 : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3280 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3281, 3282 }; } }
		public static void Configure() { RegisterHarvestablePhase(new GenericTree2()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3C1C; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenGenericTree2); } }
    public override Type NextHarvestPhase { get { return typeof(FallenGenericTree2); } }
    public override Type StartingGrowthPhase { get { return typeof(GenericTree2Sapling1); } }
#endif
	}

	public class GenericTree2Sapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new GenericTree2Sapling1()); }
		public override Type NextGrowthPhase { get { return typeof(GenericTree2Sapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(GenericTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenGenericTree2); } }

		public GenericTree2Sapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C1D, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C1E, 0, 0) });
		}
	}

	public class GenericTree2Sapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new GenericTree2Sapling2()); }
		public override Type NextGrowthPhase { get { return typeof(GenericTree2); } }
		public override Type StartingGrowthPhase { get { return typeof(GenericTree2Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenGenericTree2); } }

		public GenericTree2Sapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C1F, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C20, 0, 0) });
		}
	}

	public class FallenGenericTree2 : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenGenericTree2()); }

		public FallenGenericTree2()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3C21, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3C22, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3C23, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3C24, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3C25, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3C26, -5, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3C27, -4, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3C28, -3, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3C29, -2, -1);
			asset9.HarvestResourceBaseAmount = 10;


			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3C2A, -5, 1);
			asset10.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3C2B, -4, 1);
			asset11.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3C2C, -3, 1);
			asset12.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12 });
		}
	}


	/// Generic Tree 3
	public class GenericTree3 : SmallTree
	{
		public override int[] SmallTreeTrunkGraphics { get { return new int[] { 3283 }; } }
		public override int[] SmallTreeLeafGraphics { get { return new int[] { 3284, 3285 }; } }
		public static void Configure() { RegisterHarvestablePhase(new GenericTree3()); }

#if (CUSTOM_TREE_GRAPHICS)
    public override int StumpGraphic { get { return 0x3C2D; } }
    public override Type FinalHarvestPhase { get { return typeof(FallenGenericTree3); } }
    public override Type NextHarvestPhase { get { return typeof(FallenGenericTree3); } }
    public override Type StartingGrowthPhase { get { return typeof(GenericTree3Sapling1); } }
#endif
	}

	public class GenericTree3Sapling1 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new GenericTree3Sapling1()); }
		public override Type NextGrowthPhase { get { return typeof(GenericTree3Sapling2); } }
		public override Type StartingGrowthPhase { get { return typeof(GenericTree3Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenGenericTree3); } }

		public GenericTree3Sapling1()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C2E, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C2F, 0, 0) });
		}
	}

	public class GenericTree3Sapling2 : BaseLeafHandlingPhase
	{
		public override bool AddStump { get { return false; } }
		public static void Configure() { RegisterHarvestablePhase(new GenericTree3Sapling2()); }
		public override Type NextGrowthPhase { get { return typeof(GenericTree3); } }
		public override Type StartingGrowthPhase { get { return typeof(GenericTree3Sapling1); } }
		public override Type FinalHarvestPhase { get { return typeof(FallenGenericTree3); } }

		public GenericTree3Sapling2()
		  : base()
		{
			BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C30, 0, 0) });
			LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(0x3C31, 0, 0) });
		}
	}

	public class FallenGenericTree3 : BaseTreeHarvestPhase
	{
		public static void Configure() { RegisterHarvestablePhase(new FallenGenericTree3()); }

		public FallenGenericTree3()
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

			HarvestGraphicAsset asset1 = new HarvestGraphicAsset(0x3C32, -1, 0);
			asset1.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset2 = new HarvestGraphicAsset(0x3C33, -2, 0);
			asset2.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset3 = new HarvestGraphicAsset(0x3C34, -3, 0);
			asset3.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset4 = new HarvestGraphicAsset(0x3C35, -4, 0);
			asset4.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset5 = new HarvestGraphicAsset(0x3C36, -5, 0);
			asset5.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset6 = new HarvestGraphicAsset(0x3C37, -5, -1);
			asset6.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset7 = new HarvestGraphicAsset(0x3C38, -4, -1);
			asset7.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset8 = new HarvestGraphicAsset(0x3C39, -3, -1);
			asset8.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset9 = new HarvestGraphicAsset(0x3C3A, -2, -1);
			asset9.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset10 = new HarvestGraphicAsset(0x3C3B, -4, -2);
			asset10.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset11 = new HarvestGraphicAsset(0x3C3C, -3, -2);
			asset11.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset12 = new HarvestGraphicAsset(0x3C3D, -2, -2);
			asset12.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset13 = new HarvestGraphicAsset(0x3C3E, -5, 1);
			asset13.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset14 = new HarvestGraphicAsset(0x3C3F, -4, 1);
			asset14.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset15 = new HarvestGraphicAsset(0x3C40, -3, 1);
			asset15.HarvestResourceBaseAmount = 10;
			HarvestGraphicAsset asset16 = new HarvestGraphicAsset(0x3C41, -2, 1);
			asset16.HarvestResourceBaseAmount = 10;

			HarvestGraphicAsset asset17 = new HarvestGraphicAsset(0x3C42, -2, 2);
			asset17.HarvestResourceBaseAmount = 10;

			BaseAssetSets.Add(new HarvestGraphicAsset[] { asset1, asset2, asset3, asset4, asset5, asset6, asset7, asset8, asset9, asset10, asset11, asset12, asset13, asset14, asset15, asset16, asset17 });
		}
	}
}