using Server.Mobiles;

namespace Server.Engine.Facet
{
	public class MovementQuery : FacetEditingQuery
	{
		public static void Initialize()
		{
			PlayerMobile.BlockQuery = new MovementQuery();
		}

		public MovementQuery()
		{
		}

		public int QueryMobile(Mobile m, int previousMapBlock)
		{
			var blocknum = ((m.Location.X >> 3) * m.Map.Tiles.BlockHeight) + (m.Location.Y >> 3);

			if (blocknum != previousMapBlock)
			{
				_ = m.Send(new QueryClientHash(m));
			}

			return blocknum;
		}
	}
}