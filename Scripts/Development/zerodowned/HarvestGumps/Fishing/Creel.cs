using Server;
using Server.ContextMenus;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Targeting;

namespace Server.Items
{
    [Flipable(0x24D5, 0x24D6)]
	public class Creel : BaseContainer
	{
		private int m_staticTargetID;

		[CommandProperty(AccessLevel.GameMaster)]
		public int staticTargetID
		{
			get { return m_staticTargetID; }
			set { m_staticTargetID = value; }
		}

        private Point3D m_point3D;

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D point3D
		{
			get { return m_point3D; }
			set { m_point3D = value; }
		}

		private bool m_Active;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{
			get { return m_Active; }
			set { m_Active = value; }
		}

		private bool m_Automatic;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Automatic
		{
			get { return m_Automatic; }
			set { m_Automatic = value; }
		}

		private Point3D m_relativeLocation;

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D relativeLocation
		{
			get { return m_relativeLocation; }
			set 
			{ 
				m_relativeLocation = value; 

				InvalidateProperties();
			}

			
		}

		[Constructable]
		public Creel() : base(0x24D5)
		{
			Weight = 0.0;
		}

		// This overrides the weight, weight = 0, regardless of contents
		public override int GetTotal(TotalType type)
		{
			switch (type)
			{
				case TotalType.Weight:
					return 0;
			}

			return base.GetTotal(type);
		}
		
		public override void OnDoubleClick(Mobile from)
		{
			if (from.AccessLevel > AccessLevel.Player || from.InRange(this.GetWorldLocation(), 2) || this.RootParent is PlayerVendor)
			{
				if (from.HasGump(typeof(FishingGump)))
                	from.CloseGump(typeof(FishingGump));

            	from.SendGump(new FishingGump(from, this));
			}	
			else
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
		}

		public override bool OnDragDropInto(Mobile from, Item item, Point3D p)
		{
			return false;
		}

		public override bool OnDragDrop(Mobile from, Item dropped)
		{
			return false;
		}

		public class FishingGumpTarget : Target
        {
			Creel m_creel;

            public FishingGumpTarget(Creel creel): base(10, true, TargetFlags.None)
            {
				m_creel = creel;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
				m_creel.staticTargetID = 0;
				
				if (targeted is LandTarget)
				{
					LandTarget obj = (LandTarget)targeted;

					if( m_Water.Contains(obj.TileID) )
                    {
						m_creel.point3D = obj.Location;

						if(m_creel.Automatic)
						{
							Point3D p = m_creel.point3D;
							m_creel.relativeLocation = new Point3D( (from.X - p.X), (from.Y - p.Y), p.Z );
						}

						m_creel.Active = true;
					}
					else
                    {
                        from.SendMessage("You feel as though fishing there would be a silly idea.");
                    }
				}

				if (targeted is StaticTarget)
				{
					StaticTarget obj = (StaticTarget)targeted;

					if( obj.Name == "water" )
                    {
						m_creel.staticTargetID = obj.ItemID;

						m_creel.point3D = obj.Location;

						if(m_creel.Automatic)
						{
							Point3D p = m_creel.point3D;
							m_creel.relativeLocation = new Point3D( (from.X - p.X), (from.Y - p.Y), p.Z );
						}

						m_creel.Active = true;
					} 
					else
                    {
                        from.SendMessage("You feel as though fishing there would be a silly idea.");
                    }
				}
                    
				if (from.HasGump(typeof(FishingGump)))
					from.CloseGump(typeof(FishingGump));

				from.SendGump(new FishingGump(from, m_creel));

            }

        }

		public Creel(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int)2); // version

			//2
			writer.Write((int)m_staticTargetID);

			//1
			writer.Write((bool)m_Active);
			writer.Write((bool)m_Automatic);
			writer.Write(m_relativeLocation);

			//0
			writer.Write(m_point3D);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch(version)
			{
				case 2:
					{
						m_staticTargetID = reader.ReadInt();
						goto case 1;
					}
				case 1:
					{
						m_Active = reader.ReadBool();
						m_Automatic = reader.ReadBool();
						m_relativeLocation = reader.ReadPoint3D();
						goto case 0;
					}
				case 0:
					{
						m_point3D = reader.ReadPoint3D();
						break;
					}
			}
			
		}

		public static readonly List <int> m_Water = new List<int>
        {
            0x00A8, 0x00A9,
			0x00AA, 0x00AB,
			0x0136, 0x0137,
			0x179A,
            0x5797, 0x579C,
            0x746E, 0x7485,
            0x7490, 0x74AB,
            0x74B5, 0x75D5,
			0x00A8, 0x00AB,
            0x0136, 0x0137,
            0x5797, 0x579C,
            0x746E, 0x7485,
            0x7490, 0x74AB,
            0x74B5, 0x75D5,
            0x1797, 0x1798,
            0x1799, 0x179A,
            0x179B, 0x179C
        };
	}
}