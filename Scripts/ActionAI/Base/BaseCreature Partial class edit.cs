using System;
using Server.Engines.Harvest;

namespace Server.Mobiles
{
    public partial class BaseCreature : Mobile
	{
        public virtual HarvestDefinition harvestDefinition { get { return null; } }

		public virtual HarvestSystem harvestSystem { get { return null; } }
    }
}