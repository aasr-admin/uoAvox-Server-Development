
/* using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Misc; */
using Server.Multis;
/* using Server.Network; */
using Server.Mobiles;
using System;
using System.Collections;
using System.Collections.Generic;
/* using System.IO;
using System.Text; */

namespace Server
{        
        public class SinkTimer : Timer
        {
            private BaseBoat m_Boat;
            private int m_Count;
            private int m_waterZ;

            public SinkTimer(BaseBoat boat, int boatZ) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(2.0))
            {
                m_Boat = boat;
                m_waterZ = boatZ;

                Priority = TimerPriority.TwoFiftyMS;
            }

            protected override void OnTick()
            {

                int random = Utility.RandomMinMax(-2, 2);
                Point3D loc = new Point3D(m_Boat.X + random, m_Boat.Y + random, m_Boat.Z);


                switch (m_Count)
                {
                    case 1:
                    case 2:
                    case 3:
                        {
                            Effects.SendLocationEffect(loc, m_Boat.Map, 0x36BD, 20, 10);
                            Effects.PlaySound(loc, m_Boat.Map, 0x307);

                            if (m_Boat.Facing == Direction.North || m_Boat.Facing == Direction.South)
                            {
                                Point3D pLeft = new Point3D(m_Boat.X - 3, m_Boat.Y, m_waterZ);
                                Effects.SendLocationEffect(pLeft, m_Boat.Map, 8104, 20, 10);

                                Point3D pRight = new Point3D(m_Boat.X + 3, m_Boat.Y, m_waterZ);
                                Effects.SendLocationEffect(pRight, m_Boat.Map, 8109, 20, 10);
                            }

                            if (m_Boat.Facing == Direction.East || m_Boat.Facing == Direction.West)
                            {
                                Point3D pLeft = new Point3D(m_Boat.X, m_Boat.Y - 3, m_waterZ);
                                Effects.SendLocationEffect(pLeft, m_Boat.Map, 8099, 20, 10);

                                Point3D pRight = new Point3D(m_Boat.X, m_Boat.Y + 3, m_waterZ);
                                Effects.SendLocationEffect(pRight, m_Boat.Map, 8114, 20, 10);
                            }

                            m_Boat.Z -= 1; //= new Point3D( m_Boat.X, m_Boat.Y, m_Boat.Z - 1 );
                            Effects.SendLocationEffect(loc, m_Boat.Map, 0x398C, 20, 2000);

                            if (m_Boat.TillerMan != null && m_Count < 5)
                                m_Boat.TillerMan.Say(1007168 + m_Count);



                            m_Boat.Hue = m_Boat.Hold.Hue = m_Boat.TillerMan.Hue = m_Boat.PPlank.Hue = m_Boat.SPlank.Hue = 906;

                        }

                        break;
                    case 4:
                        {
                            Effects.SendLocationEffect(loc, m_Boat.Map, 0x398C, 20, 2000);

                            Effects.SendLocationEffect(loc, m_Boat.Map, 0x36BD, 20, 10);
                            Effects.PlaySound(loc, m_Boat.Map, 0x307);



                            List<Mobile> targets = new List<Mobile>();
                            IPooledEnumerable eable = m_Boat.GetMobilesInRange(16);

                            foreach (Mobile m in eable)
                            {
                                if (m is ActionAI_PirateCrew || m is PlayerMobile)
                                    targets.Add(m);
                            }

                            eable.Free();

                            if (targets.Count > 0)
                            {
                                for (int i = 0; i < targets.Count; ++i)
                                {
                                    Mobile m = targets[i];
                                    m.Kill();
                                }
                            }


                        }

                        break;
                    case 5:
                        Effects.PlaySound(loc, m_Boat.Map, 0x117);
                        Effects.SendLocationEffect(loc, m_Boat.Map, 0x36BD, 20, 10);

                        if (m_Boat.Facing == Direction.North || m_Boat.Facing == Direction.South)
                        {
                            Point3D pLeft = new Point3D(m_Boat.X - 3, m_Boat.Y, m_waterZ);
                            Effects.SendLocationEffect(pLeft, m_Boat.Map, 8104, 20, 10);

                            Point3D pRight = new Point3D(m_Boat.X + 3, m_Boat.Y, m_waterZ);
                            Effects.SendLocationEffect(pRight, m_Boat.Map, 8109, 20, 10);
                        }

                        if (m_Boat.Facing == Direction.East || m_Boat.Facing == Direction.West)
                        {
                            Point3D pLeft = new Point3D(m_Boat.X, m_Boat.Y - 3, m_waterZ);
                            Effects.SendLocationEffect(pLeft, m_Boat.Map, 8099, 20, 10);

                            Point3D pRight = new Point3D(m_Boat.X, m_Boat.Y + 3, m_waterZ);
                            Effects.SendLocationEffect(pRight, m_Boat.Map, 8114, 20, 10);
                        }

                        m_Boat.Z -= 1;
                        break;
                    case 6:
                        break;
                    case 7:
                        break;
                    case 8:
                        Effects.PlaySound(loc, m_Boat.Map, 0x117);

                        if (m_Boat.Facing == Direction.North || m_Boat.Facing == Direction.South)
                        {
                            Point3D pLeft = new Point3D(m_Boat.X - 3, m_Boat.Y, m_waterZ);
                            Effects.SendLocationEffect(pLeft, m_Boat.Map, 8104, 20, 10);

                            Point3D pRight = new Point3D(m_Boat.X + 3, m_Boat.Y, m_waterZ);
                            Effects.SendLocationEffect(pRight, m_Boat.Map, 8109, 20, 10);
                        }

                        if (m_Boat.Facing == Direction.East || m_Boat.Facing == Direction.West)
                        {
                            Point3D pLeft = new Point3D(m_Boat.X, m_Boat.Y - 3, m_waterZ);
                            Effects.SendLocationEffect(pLeft, m_Boat.Map, 8099, 20, 10);

                            Point3D pRight = new Point3D(m_Boat.X, m_Boat.Y + 3, m_waterZ);
                            Effects.SendLocationEffect(pRight, m_Boat.Map, 8114, 20, 10);
                        }

                        m_Boat.Z -= 1;
                        break;
                    case 9:
                        if (m_Boat.Facing == Direction.North || m_Boat.Facing == Direction.South)
                        {
                            Point3D pLeft = new Point3D(m_Boat.X - 3, m_Boat.Y, m_waterZ);
                            Effects.SendLocationEffect(pLeft, m_Boat.Map, 8104, 20, 10);

                            Point3D pRight = new Point3D(m_Boat.X + 3, m_Boat.Y, m_waterZ);
                            Effects.SendLocationEffect(pRight, m_Boat.Map, 8109, 20, 10);
                        }

                        if (m_Boat.Facing == Direction.East || m_Boat.Facing == Direction.West)
                        {
                            Point3D pLeft = new Point3D(m_Boat.X, m_Boat.Y - 3, m_waterZ);
                            Effects.SendLocationEffect(pLeft, m_Boat.Map, 8099, 20, 10);

                            Point3D pRight = new Point3D(m_Boat.X, m_Boat.Y + 3, m_waterZ);
                            Effects.SendLocationEffect(pRight, m_Boat.Map, 8114, 20, 10);
                        }

                        m_Boat.Z -= 2;
                        m_Boat.Hue = 904;
                        break;
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                        {
                            if (m_Boat.Facing == Direction.North || m_Boat.Facing == Direction.South)
                            {
                                Point3D pLeft = new Point3D(m_Boat.X - 3, m_Boat.Y, m_waterZ);
                                Effects.SendLocationEffect(pLeft, m_Boat.Map, 8104, 20, 10);

                                Point3D pRight = new Point3D(m_Boat.X + 3, m_Boat.Y, m_waterZ);
                                Effects.SendLocationEffect(pRight, m_Boat.Map, 8109, 20, 10);
                            }

                            if (m_Boat.Facing == Direction.East || m_Boat.Facing == Direction.West)
                            {
                                Point3D pLeft = new Point3D(m_Boat.X, m_Boat.Y - 3, m_waterZ);
                                Effects.SendLocationEffect(pLeft, m_Boat.Map, 8099, 20, 10);

                                Point3D pRight = new Point3D(m_Boat.X, m_Boat.Y + 3, m_waterZ);
                                Effects.SendLocationEffect(pRight, m_Boat.Map, 8114, 20, 10);
                            }

                            m_Boat.Z -= 3;
                            Effects.SendLocationEffect(loc, m_Boat.Map, 0x398C, 20, 2000);
                            m_Boat.Hue = 902;
                        }

                        break;

                    case 15:
                        {
                            m_Boat.Delete();

                            Stop();
                        }

                        break;
                }

                ++m_Count;
            }
        }
}