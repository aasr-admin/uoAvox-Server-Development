using Server;
using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;

#region Developer Notations

/// This Command Does Nothing At This Time
/// ToDo: Get This Command Working

#endregion

namespace Server.Engine.Facet
{
	public partial class FacetEditingCommands
	{
		[Usage("CircularIndent")]
		[Description("Makes a circular indent in the terrain.")]
		private static void circularIndent_OnCommand(CommandEventArgs e)
		{
			if (e.Length != 2)
			{
				e.Mobile.SendMessage("You must specify a radius and a depth.");

				return;
			}

			int radius = e.GetInt32(0);
			int depth = e.GetInt32(1);

			if (radius > 0)
			{
				e.Mobile.Target = new CircularIndentTarget(radius, depth);
			}
		}

		private class CircularIndentTarget : BaseLandRadialTarget
		{
			private int m_Radius;
			private int m_Depth;

			public CircularIndentTarget(int radius, int depth) : base(1, radius, 0)
			{
				m_Radius = radius;
				m_Depth = depth;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (base.SetupTarget(from, o))
				{
					List<Point2D> circle = FacetEditingUtility.RasterFilledCircle(new Point2D(m_Location.X, m_Location.Y), m_Radius);

					MapOperationSeries moveSeries = new MapOperationSeries(null, from.Map.MapID);

					bool first = true;

					foreach (Point2D p in circle)
					{
						if (first)
						{
							moveSeries = new MapOperationSeries(new IncLandAltitude(p.X, p.Y, from.Map.MapID, m_Depth), from.Map.MapID);
							first = false;
						}
						else
						{
							moveSeries.Add(new IncLandAltitude(p.X, p.Y, from.Map.MapID, m_Depth));
						}
					}

					moveSeries.DoOperation();
				}
			}
		}
	}
}