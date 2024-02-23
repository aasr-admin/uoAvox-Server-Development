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
	public abstract class SmallTree : BaseLeafHandlingPhase
	{
		public virtual int[] SmallTreeTrunkGraphics { get { return new int[] { }; } }

		public virtual int[] SmallTreeLeafGraphics { get { return new int[] { }; } }

		public override Type NextHarvestPhase { get { return typeof(SmallFallenTreeEastWest); } }

		public override Type StartingGrowthPhase { get { return typeof(SmallSaplingTreePhase1); } }

		public override Type FinalHarvestPhase { get { return typeof(SmallFallenTreeEastWest); } }

		public SmallTree() : base()
		{
			/// This Is A Simple Tree, So There Is Only A Main Tree Phase
			PhaseResources.Add(0, new HarvestResource(00.0, 00.0, 100.0, 1072540, typeof(Log)));
			PhaseResources.Add(350, new HarvestResource(65.0, 25.0, 105.0, 1072541, typeof(OakLog)));
			PhaseResources.Add(751, new HarvestResource(80.0, 40.0, 120.0, 1072542, typeof(AshLog)));
			PhaseResources.Add(545, new HarvestResource(95.0, 55.0, 135.0, 1072543, typeof(YewLog)));
			PhaseResources.Add(436, new HarvestResource(100.0, 60.0, 140.0, 1072544, typeof(HeartwoodLog)));
			PhaseResources.Add(339, new HarvestResource(100.0, 60.0, 140.0, 1072545, typeof(BloodwoodLog)));
			PhaseResources.Add(688, new HarvestResource(100.0, 60.0, 140.0, 1072546, typeof(FrostwoodLog)));

			/// This Tree Has Two Sets Of Leaves That Are Possible
			foreach (int treeTrunkId in SmallTreeTrunkGraphics)
			{
				BaseAssetSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(treeTrunkId, 0, 0) });
			}

			foreach (int leafId in SmallTreeLeafGraphics)
			{
				LeafSets.Add(new HarvestGraphicAsset[] { new HarvestGraphicAsset(leafId, 0, 0) });
			}
		}
	}
}