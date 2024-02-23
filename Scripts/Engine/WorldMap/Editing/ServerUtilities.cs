using System;
using System.Collections.Generic;

namespace Server.Engine.Facet
{
	public class FacetEditingUtility
	{
		public static List<Point2D> RasterCircle(Point2D center, int radius)
		{
			var x0 = center.X;
			var y0 = center.Y;

			var pointList = new List<Point2D>();

			var f = 1 - radius;
			var ddF_x = 1;
			var ddF_y = -2 * radius;
			var x = 0;
			var y = radius;

			pointList.Add(new Point2D(x0, y0 - radius));
			pointList.Add(new Point2D(x0, y0 + radius));
			pointList.Add(new Point2D(x0 - radius, y0));
			pointList.Add(new Point2D(x0 + radius, y0));

			while (x < y)
			{
				if (f >= 0)
				{
					y--;
					ddF_y += 2;
					f += ddF_y;
				}

				x++;
				ddF_x += 2;
				f += ddF_x;

				var p = new Point2D(x0 + x, y0 + y);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}

				p = new Point2D(x0 - x, y0 + y);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}

				p = new Point2D(x0 + x, y0 - y);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}

				p = new Point2D(x0 - x, y0 - y);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}

				p = new Point2D(x0 + y, y0 + x);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}

				p = new Point2D(x0 - y, y0 + x);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}

				p = new Point2D(x0 + y, y0 - x);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}

				p = new Point2D(x0 - y, y0 - x);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}
			}

			foreach (var p in pointList)
			{
				Console.WriteLine("" + p.X + "," + p.Y);
			}

			return pointList;
		}

		#region Developer Notations

		/// http://en.wikipedia.org/wiki/Midpoint_circle_algorithm

		#endregion

		public static List<Point2D> RasterFilledCircle(Point2D center, int radius)
		{
			var x0 = center.X;
			var y0 = center.Y;

			var pointList = new List<Point2D>();

			var f = 1 - radius;
			var ddF_x = 1;
			var ddF_y = -2 * radius;
			var x = 0;
			var y = radius;

			for (var h = y0 - radius; h <= y0 + radius; h++)
			{
				var p = new Point2D(x0, h);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}
			}

			for (var h = x0 - radius; h <= x0 + radius; h++)
			{
				var p = new Point2D(h, y0);

				if (!pointList.Contains(p))
				{
					pointList.Add(p);
				}
			}

			while (x < y)
			{
				if (f >= 0)
				{
					y--;
					ddF_y += 2;
					f += ddF_y;
				}

				x++;
				ddF_x += 2;
				f += ddF_x;

				for (var h = x0 - x; h <= x0 + x; h++)
				{
					var p = new Point2D(h, y0 + y);

					if (!pointList.Contains(p))
					{
						pointList.Add(p);
					}
				}

				for (var h = x0 - x; h <= x0 + x; h++)
				{
					var p = new Point2D(h, y0 - y);

					if (!pointList.Contains(p))
					{
						pointList.Add(p);
					}
				}

				for (var h = x0 - y; h <= x0 + y; h++)
				{
					var p = new Point2D(h, y0 + x);

					if (!pointList.Contains(p))
					{
						pointList.Add(p);
					}
				}

				for (var h = x0 - y; h <= x0 + y; h++)
				{
					var p = new Point2D(h, y0 - x);

					if (!pointList.Contains(p))
					{
						pointList.Add(p);
					}
				}
			}

			return pointList;
		}
	}
}