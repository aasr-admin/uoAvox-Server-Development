using Server;
using Server.Mobiles;
using System;

namespace Server
{
	public partial class OppositionGroup
	{
		private static OppositionGroup m_PiratesAndSeafarers = new OppositionGroup(new Type[][]
		{
				new Type[]
				{
					typeof( ActionAI_PirateCaptain ),
					typeof( ActionAI_PirateCrew )
				},
				new Type[]
				{
					typeof( ActionAI_PirateHunter ),
					typeof( ActionAI_Privateer ),
					typeof( ActionAI_DeepSeaFishermen ),
					typeof( ActionAI_DockedFishermen )
				}
		});

		public static OppositionGroup PiratesAndSeafarers
		{
			get { return m_PiratesAndSeafarers; }
		}


		private static OppositionGroup m_SeafarersAndPirates = new OppositionGroup(new Type[][]
		{
				new Type[]
				{
					typeof( ActionAI_PirateHunter ),
					typeof( ActionAI_Privateer )
				},
				new Type[]
				{
					typeof( ActionAI_PirateCaptain ),
					typeof( ActionAI_PirateCrew )
				}
		});

		public static OppositionGroup SeafarersAndPirates
		{
			get { return m_SeafarersAndPirates; }
		}
	}
}