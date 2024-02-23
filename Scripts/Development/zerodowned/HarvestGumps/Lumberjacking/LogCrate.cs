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
	public class LogCrate : BaseContainer
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
		public LogCrate() : base(0x09A9)
		{
			Weight = 0.0;
            Hue = 1191; // standin for something better?
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
				if (from.HasGump(typeof(LumberjackingGump)))
                	from.CloseGump(typeof(LumberjackingGump));

            	from.SendGump(new LumberjackingGump(from, this));
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

		public class LumberjackingGumpTarget : Target
        {
			LogCrate m_crate;

            public LumberjackingGumpTarget(LogCrate crate): base(10, true, TargetFlags.None)
            {
				m_crate = crate;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                m_crate.staticTargetID = 0;

				if (targeted is LandTarget)
				{
					LandTarget obj = (LandTarget)targeted;

                    if( m_MountainAndCaveTiles.Contains(obj.TileID) )
                    {
                        m_crate.point3D = obj.Location;

                        if(m_crate.Automatic)
                        {
                            Point3D p = m_crate.point3D;
                            m_crate.relativeLocation = new Point3D( (from.X - p.X), (from.Y - p.Y), p.Z );
                        }

                        m_crate.Active = true;
                    }
                    else
                    {
                        from.SendMessage("You feel as though mining there would be a silly idea.");
                    }
				}

                if (targeted is StaticTarget)
				{
					StaticTarget obj = (StaticTarget)targeted;

					if( obj.Name == "cave floor" )
                    {
						m_crate.staticTargetID = obj.ItemID;

						m_crate.point3D = obj.Location;

						if(m_crate.Automatic)
						{
							Point3D p = m_crate.point3D;
							m_crate.relativeLocation = new Point3D( (from.X - p.X), (from.Y - p.Y), p.Z );
						}

						m_crate.Active = true;
					} 
					else
                    {
                        from.SendMessage("You feel as though mining there would be a silly idea.");
                    }
				}    
                    
				if (from.HasGump(typeof(LumberjackingGump)))
					from.CloseGump(typeof(LumberjackingGump));

				from.SendGump(new LumberjackingGump(from, m_crate));

            }

        }

		public LogCrate(Serial serial) : base(serial)
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


        #region Tile lists
		//public static int[] m_MountainAndCaveTiles = new int[]
        public static readonly List <int> m_MountainAndCaveTiles = new List<int>
			{
				220, 221, 222, 223, 224, 225, 226, 227, 228, 229,
				230, 231, 236, 237, 238, 239, 240, 241, 242, 243,
				244, 245, 246, 247, 252, 253, 254, 255, 256, 257,
				258, 259, 260, 261, 262, 263, 268, 269, 270, 271,
				272, 273, 274, 275, 276, 277, 278, 279, 286, 287,
				288, 289, 290, 291, 292, 293, 294, 296, 296, 297,
				321, 322, 323, 324, 467, 468, 469, 470, 471, 472,
				473, 474, 476, 477, 478, 479, 480, 481, 482, 483,
				484, 485, 486, 487, 492, 493, 494, 495, 543, 544,
				545, 546, 547, 548, 549, 550, 551, 552, 553, 554,
				555, 556, 557, 558, 559, 560, 561, 562, 563, 564,
				565, 566, 567, 568, 569, 570, 571, 572, 573, 574,
				575, 576, 577, 578, 579, 581, 582, 583, 584, 585,
				586, 587, 588, 589, 590, 591, 592, 593, 594, 595,
				596, 597, 598, 599, 600, 601, 610, 611, 612, 613,

				1010, 1741, 1742, 1743, 1744, 1745, 1746, 1747, 1748, 1749,
				1750, 1751, 1752, 1753, 1754, 1755, 1756, 1757, 1771, 1772,
				1773, 1774, 1775, 1776, 1777, 1778, 1779, 1780, 1781, 1782,
				1783, 1784, 1785, 1786, 1787, 1788, 1789, 1790, 1801, 1802,
				1803, 1804, 1805, 1806, 1807, 1808, 1809, 1811, 1812, 1813,
				1814, 1815, 1816, 1817, 1818, 1819, 1820, 1821, 1822, 1823,
				1824, 1831, 1832, 1833, 1834, 1835, 1836, 1837, 1838, 1839,
				1840, 1841, 1842, 1843, 1844, 1845, 1846, 1847, 1848, 1849,
				1850, 1851, 1852, 1853, 1854, 1861, 1862, 1863, 1864, 1865,
				1866, 1867, 1868, 1869, 1870, 1871, 1872, 1873, 1874, 1875,
				1876, 1877, 1878, 1879, 1880, 1881, 1882, 1883, 1884, 1981,
				1982, 1983, 1984, 1985, 1986, 1987, 1988, 1989, 1990, 1991,
				1992, 1993, 1994, 1995, 1996, 1997, 1998, 1999, 2000, 2001,
				2002, 2003, 2004, 2028, 2029, 2030, 2031, 2032, 2033, 2100,
				2101, 2102, 2103, 2104, 2105,

				0x453B, 0x453C, 0x453D, 0x453E, 0x453F, 0x4540, 0x4541,
				0x4542, 0x4543, 0x4544, 0x4545, 0x4546, 0x4547, 0x4548,
				0x4549, 0x454A, 0x454B, 0x454C, 0x454D, 0x454E, 0x454F
			};

		private static int[] m_SandTiles = new int[]
			{
				22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
				32, 33, 34, 35, 36, 37, 38, 39, 40, 41,
				42, 43, 44, 45, 46, 47, 48, 49, 50, 51,
				52, 53, 54, 55, 56, 57, 58, 59, 60, 61,
				62, 68, 69, 70, 71, 72, 73, 74, 75,

				286, 287, 288, 289, 290, 291, 292, 293, 294, 295,
				296, 297, 298, 299, 300, 301, 402, 424, 425, 426,
				427, 441, 442, 443, 444, 445, 446, 447, 448, 449,
				450, 451, 452, 453, 454, 455, 456, 457, 458, 459,
				460, 461, 462, 463, 464, 465, 642, 643, 644, 645,
				650, 651, 652, 653, 654, 655, 656, 657, 821, 822,
				823, 824, 825, 826, 827, 828, 833, 834, 835, 836,
				845, 846, 847, 848, 849, 850, 851, 852, 857, 858,
				859, 860, 951, 952, 953, 954, 955, 956, 957, 958,
				967, 968, 969, 970,

				1447, 1448, 1449, 1450, 1451, 1452, 1453, 1454, 1455,
				1456, 1457, 1458, 1611, 1612, 1613, 1614, 1615, 1616,
				1617, 1618, 1623, 1624, 1625, 1626, 1635, 1636, 1637,
				1638, 1639, 1640, 1641, 1642, 1647, 1648, 1649, 1650
			};
		#endregion
	}
}