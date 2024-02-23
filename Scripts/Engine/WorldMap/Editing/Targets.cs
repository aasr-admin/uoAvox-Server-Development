using Server.Commands.Generic;
using Server.Network;
using Server.Targeting;

using System.Collections.Generic;

namespace Server.Engine.Facet
{
	public class BaseLandTarget : Target
	{
		protected IPoint3D m_Location;
		public BaseLandTarget() : base(-1, true, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object o)
		{
		}

		protected bool SetupTarget(Mobile from, object o)
		{
			if (!BaseCommand.IsAccessible(from, o))
			{
				from.SendMessage("That is not accessible.");

				return false;
			}

			if (o is not IPoint3D)
			{
				return false;
			}

			m_Location = (IPoint3D)o;

			return true;
		}
	}

	public class RadialTarget : Target
	{
		public int Height { get; set; }

		public int TType { get; set; }

		public int Radius { get; set; }

		public RadialTarget(int TType, int Radius, int Height) : base(-1, true, TargetFlags.None)
		{
			this.TType = TType;
			this.Radius = Radius;
			this.Height = Height;
		}

		public override Packet GetPacketFor(NetState ns)
		{
			var objs = new List<TargetObject>();
			var circle = FacetEditingUtility.RasterCircle(new Point2D(0, 0), Radius);

			foreach (var p in circle)
			{
				var t = new TargetObject
				{
					ItemID = 0xA12,
					Hue = 35,
					xOffset = p.X,
					yOffset = p.Y,
					zOffset = 0
				};

				objs.Add(t);
			}

			return new TargetObjectList(objs, ns.Mobile, true);
		}
	}

	public class BaseLandRadialTarget : RadialTarget
	{
		protected IPoint3D m_Location;
		public BaseLandRadialTarget(int TType, int Radius, int Height) : base(TType, Radius, Height)
		{
		}

		protected override void OnTarget(Mobile from, object o)
		{
		}

		protected bool SetupTarget(Mobile from, object o)
		{
			if (!BaseCommand.IsAccessible(from, o))
			{
				from.SendMessage("That is not accessible.");

				return false;
			}

			if (o is not IPoint3D)
			{
				return false;
			}

			m_Location = (IPoint3D)o;

			return true;
		}
	}
}