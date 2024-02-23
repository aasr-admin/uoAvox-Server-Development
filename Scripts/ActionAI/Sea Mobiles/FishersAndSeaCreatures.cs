using Server;
using Server.Mobiles;
using System;

namespace Server
{
	public partial class OppositionGroup
	{
		private static OppositionGroup m_FishersAndSeaCreatures = new OppositionGroup(new Type[][]
		{
				new Type[]
				{
					typeof( ActionAI_Fisher ),
					typeof( ActionAI_DeepSeaFishermen ),
					typeof( ActionAI_DockedFishermen )
				},
				new Type[]
				{
					typeof( SeaSerpent ),
					typeof( DeepSeaSerpent ),
					typeof( WaterElemental ),
					typeof( Kraken ),
					typeof( Leviathan )
				}
		});

		public static OppositionGroup FishersAndSeaCreatures
		{
			get { return m_FishersAndSeaCreatures; }
		}


		private static OppositionGroup m_SeaCreaturesAndFishers = new OppositionGroup(new Type[][]
		{
				new Type[]
				{
					typeof( SeaSerpent ),
					typeof( DeepSeaSerpent ),
					typeof( WaterElemental ),
					typeof( Kraken ),
					typeof( Leviathan )
				},
				new Type[]
				{
					typeof( ActionAI_Fisher ),
					typeof( ActionAI_DeepSeaFishermen ),
					typeof( ActionAI_DockedFishermen )
				}
		});

		public static OppositionGroup SeaCreaturesAndFishers
		{
			get { return m_SeaCreaturesAndFishers; }
		}
	}
}