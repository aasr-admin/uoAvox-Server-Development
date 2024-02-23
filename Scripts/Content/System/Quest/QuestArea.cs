namespace Server.Quests
{
	public record QuestArea
	{
		public Map Map { get; }
		public Point3D Location { get; }
		public Poly3D Bounds { get; }

		public QuestArea(Map map, Point3D location, Poly3D bounds)
		{
			Map = map;
			Location = location;
			Bounds = bounds;

			if (Bounds.Count == 0)
			{
				var points = new Point2D[8];

				var slice = 360 / points.Length;

				for (var i = 0; i < points.Length; i++)
				{
					points[i] = Angle.GetPoint2D(location.X, location.Y, i * slice, 5);
				}

				Bounds = new Poly3D(Region.MinZ, Region.MaxZ, points);
			}
		}

		public bool InBounds(IEntity entity)
		{
			if (entity?.Deleted == false)
			{
				return InBounds(entity.Map, entity is Item item ? item.WorldLocation : entity.Location);
			}

			return false;
		}

		public bool InBounds(Map map, IPoint3D point)
		{
			return map == Map && (Location == point || Bounds.Contains(point));
		}
	}
}
