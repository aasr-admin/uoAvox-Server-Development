﻿using Server;
using Server.Engines.Harvest;
using Server.Items;
using Server.Misc;
using Server.Multis;

using System;
using System.Collections.Generic;

namespace Server
{
    /* CUSTOM MAP/FACET TEMPLATE */

    // 1) Create a new class for the Map which includes a List of Paths that includes each Path you want the mobile to select from when it's spawned.
    // 2) Create a list of points for each path you plot.
    // 3) Scroll down to private void SetPath() and create a new for loop for the same Map/Facet 

    /*

    #region Paths_<MAP NAME>
    public class ActionAI_MinerPaths_<MAP NAME>  
    {
        #region List of Points
        public static List<Tuple<Point3D, Direction>> path_miner_ONE 
            = new List<Tuple<Point3D, Direction>>
        {
            // add each point of the path here
        };
        #endregion

        // NOTE: List of paths MUST be after all possible path lists

        #region ListOfPaths_<MAP NAME>
        public static List<Tuple<Point3D, List<Tuple<Point3D, Direction>> >> Miner_ListOfPaths 
            = new List<Tuple<Point3D, List<Tuple<Point3D, Direction>> >>
        {
            // add each path you want the list to choose from here
        };
        #endregion

    }
    #endregion

    */

    #region Paths_Trammel
    public class ActionAI_MinerPaths_Trammel
    {
        public static List<Tuple<Point3D, Direction>> path_miner_ONE
            = new List<Tuple<Point3D, Direction>>
        {
            (new Tuple<Point3D, Direction> (new Point3D(3510, 2755, 7), Direction.West)),
            (new Tuple<Point3D, Direction> (new Point3D(3512, 2761, 4), Direction.West)),
            (new Tuple<Point3D, Direction> (new Point3D(3516, 2770, 9), Direction.West)),
            (new Tuple<Point3D, Direction> (new Point3D(3517, 2765, 14), Direction.East)),
            (new Tuple<Point3D, Direction> (new Point3D(3513, 2757, 9), Direction.East)),
            (new Tuple<Point3D, Direction> (new Point3D(3511, 2753, 7), Direction.North)),
        };

        public static List<Tuple<Point3D, Direction>> path_miner_TWO
            = new List<Tuple<Point3D, Direction>>
        {
            (new Tuple<Point3D, Direction> (new Point3D(3432, 2789, 0), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3441, 2788, 9), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3449, 2790, 12), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3456, 2792, 14), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3463, 2793, 20), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3471, 2791, 22), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3476, 2790, 7), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3484, 2785, 1), Direction.Down)),
            (new Tuple<Point3D, Direction> (new Point3D(3493, 2777, 0), Direction.East)),
            (new Tuple<Point3D, Direction> (new Point3D(3492, 2772, 3), Direction.Down))
        };


        #region ListOfPaths_Trammel
        public static List<Tuple<Point3D, List<Tuple<Point3D, Direction>>>> ListOfPaths
            = new List<Tuple<Point3D, List<Tuple<Point3D, Direction>>>>
        {
            (new Tuple<Point3D, List<Tuple<Point3D, Direction>> > (new Point3D(path_miner_ONE[0].Item1), path_miner_ONE)),
            (new Tuple<Point3D, List<Tuple<Point3D, Direction>> > (new Point3D(path_miner_TWO[0].Item1), path_miner_TWO))
        };
        #endregion
    }
    #endregion


    #region Paths_Fel
    public class ActionAI_MinerPaths_Felucca
    {
        public static List<Tuple<Point3D, Direction>> path_miner_ONE
            = new List<Tuple<Point3D, Direction>>
        {
            (new Tuple<Point3D, Direction> (new Point3D(3510, 2755, 7), Direction.West)),
            (new Tuple<Point3D, Direction> (new Point3D(3512, 2761, 4), Direction.West)),
            (new Tuple<Point3D, Direction> (new Point3D(3516, 2770, 9), Direction.West)),
            (new Tuple<Point3D, Direction> (new Point3D(3517, 2765, 14), Direction.East)),
            (new Tuple<Point3D, Direction> (new Point3D(3513, 2757, 9), Direction.East)),
            (new Tuple<Point3D, Direction> (new Point3D(3511, 2753, 7), Direction.North)),
        };

        public static List<Tuple<Point3D, Direction>> path_miner_TWO
            = new List<Tuple<Point3D, Direction>>
        {
            (new Tuple<Point3D, Direction> (new Point3D(3432, 2789, 0), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3441, 2788, 9), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3449, 2790, 12), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3456, 2792, 14), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3463, 2793, 20), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3471, 2791, 22), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3476, 2790, 7), Direction.South)),
            (new Tuple<Point3D, Direction> (new Point3D(3484, 2785, 1), Direction.Down)),
            (new Tuple<Point3D, Direction> (new Point3D(3493, 2777, 0), Direction.East)),
            (new Tuple<Point3D, Direction> (new Point3D(3492, 2772, 3), Direction.Down))
        };


        #region ListOfPaths_Felucca
        public static List<Tuple<Point3D, List<Tuple<Point3D, Direction>>>> ListOfPaths
            = new List<Tuple<Point3D, List<Tuple<Point3D, Direction>>>>
        {
            (new Tuple<Point3D, List<Tuple<Point3D, Direction>> > (new Point3D(path_miner_ONE[0].Item1), path_miner_ONE)),
            (new Tuple<Point3D, List<Tuple<Point3D, Direction>> > (new Point3D(path_miner_TWO[0].Item1), path_miner_TWO))
        };
        #endregion

    }
    #endregion


    #region Paths_Ilshenar
    public class ActionAI_MinerPaths_Ilshenar
    {
        //
        // REMOVE MAP FROM BLOCK COMMENT ON SETPATH() IF YOU ADD PATHS HERE
        //

        /*  
        public static List<Tuple<Point3D, Direction>> path_miner_ONE 
             = new List<Tuple<Point3D, Direction>>
         {
             (new Tuple<Point3D, Direction> (new Point3D(0, 0, 0), Direction.South)),
             (new Tuple<Point3D, Direction> (new Point3D(0, 0, 0), Direction.Left))
         };


         #region ListOfPaths_Ilshenar
         public static List<Tuple<Point3D, List<Tuple<Point3D, Direction>> >> ListOfPaths 
             = new List<Tuple<Point3D, List<Tuple<Point3D, Direction>> >>
         {
             (new Tuple<Point3D, List<Tuple<Point3D, Direction>> > (new Point3D(path_miner_ONE[0].Item1), path_miner_ONE))
         };
         #endregion 
         */
    }
    #endregion


    #region Paths_Malas
    public class ActionAI_MinerPaths_Malas
    {
        //
        // REMOVE MAP FROM BLOCK COMMENT ON SETPATH() IF YOU ADD PATHS HERE
        //

        /*  
        public static List<Tuple<Point3D, Direction>> path_miner_ONE 
             = new List<Tuple<Point3D, Direction>>
         {
             (new Tuple<Point3D, Direction> (new Point3D(0, 0, 0), Direction.South)),
             (new Tuple<Point3D, Direction> (new Point3D(0, 0, 0), Direction.Left))
         };


         #region ListOfPaths_Ilshenar
         public static List<Tuple<Point3D, List<Tuple<Point3D, Direction>> >> ListOfPaths 
             = new List<Tuple<Point3D, List<Tuple<Point3D, Direction>> >>
         {
             (new Tuple<Point3D, List<Tuple<Point3D, Direction>> > (new Point3D(path_miner_ONE[0].Item1), path_miner_ONE))
         };
         #endregion 
         */
    }
    #endregion


    #region Paths_Tokuno
    public class ActionAI_MinerPaths_Tokuno
    {
        //
        // REMOVE MAP FROM BLOCK COMMENT ON SETPATH() IF YOU ADD PATHS HERE
        //

        /*  
        public static List<Tuple<Point3D, Direction>> path_miner_ONE 
             = new List<Tuple<Point3D, Direction>>
         {
             (new Tuple<Point3D, Direction> (new Point3D(0, 0, 0), Direction.South)),
             (new Tuple<Point3D, Direction> (new Point3D(0, 0, 0), Direction.Left))
         };


         #region ListOfPaths_Ilshenar
         public static List<Tuple<Point3D, List<Tuple<Point3D, Direction>> >> ListOfPaths 
             = new List<Tuple<Point3D, List<Tuple<Point3D, Direction>> >>
         {
             (new Tuple<Point3D, List<Tuple<Point3D, Direction>> > (new Point3D(path_miner_ONE[0].Item1), path_miner_ONE))
         };
         #endregion 
         */
    }
    #endregion
}

namespace Server.Mobiles
{
    public class ActionAI_Miner : BaseCreature
    {
        private MinerCamp m_Camp;
        public PathFollower m_Path;
        private int m_Index;
        private WayPoint m_waypointFirst;
        private List<Tuple<Point3D, Direction>> m_MobilePath;

        public override HarvestDefinition harvestDefinition { get { return Mining.System.OreAndStone; } }
        public override HarvestSystem harvestSystem { get { return Mining.System; } }

        public override bool PlayerRangeSensitive { get { return false; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Index
        {
            get { return m_Index; }
            set { m_Index = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WayPoint waypointFirst
        {
            get { return m_waypointFirst; }
            set { m_waypointFirst = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public List<Tuple<Point3D, Direction>> MobilePath
        {
            get { return m_MobilePath; }
            set { m_MobilePath = value; }
        }


        [Constructable]
        public ActionAI_Miner()
            : base(AIType.AI_ActionAI, FightMode.None, 10, 1, 0.2, 1.6)
        {
            InitStats(31, 41, 51);

            SetSkill(SkillName.Healing, 36, 68);
            SetSkill(SkillName.Mining, 200, 300);

            RangeHome = 0;

            SpeechHue = Utility.RandomDyedHue();
            Title = "the MiningAiTestMobile";
            Hue = Utility.RandomSkinHue();


            if (this.Female = Utility.RandomBool())
            {
                this.Body = 0x191;
                this.Name = NameList.RandomName("female");
            }
            else
            {
                this.Body = 0x190;
                this.Name = NameList.RandomName("male");
            }
            AddItem(new Doublet(Utility.RandomDyedHue()));
            AddItem(new Sandals(Utility.RandomNeutralHue()));
            AddItem(new ShortPants(Utility.RandomNeutralHue()));
            AddItem(new HalfApron(Utility.RandomDyedHue()));

            AddItem(new Pickaxe());

            Utility.AssignRandomHair(this);

            Container pack = new Backpack();
            pack.Movable = false;
            AddItem(pack);

            RangeHome = 10;

            Timer.DelayCall(CreateCamp);
        }

        private void SetPath()
        {
            /* Custom Map/Facet for loop */

            // Please read instructions above before making edits here.
            // If we try to set a path that doesn't exist or a list that doesn't have any points it will cause issues or potentially crash the server.

            /*

            #region Felucca
            if(this.Map == Map.<MAP NAME>) // check for the name of the map the player is on
            {
                List<Tuple<Point3D,List<Tuple<Point3D,Direction>>>> MapPaths = ActionAI_MinerPaths_<MAP NAME>.Miner_ListOfPaths; // set the listofpaths for the map here


                for( int i = 0; i < MapPaths.Count; i++)
                {
                    if( Utility.InRange( this.Home, MapPaths[i].Item1, 25 ) )
                    {
                        m_MobilePath = MapPaths[i].Item2;
                        break;
                    }
                    else
                        continue;
                    
                }
            }
            #endregion

            */

            #region Felucca
            if (this.Map == Map.Felucca)
            {
                List<Tuple<Point3D, List<Tuple<Point3D, Direction>>>> MapPaths = ActionAI_MinerPaths_Felucca.ListOfPaths;


                for (int i = 0; i < MapPaths.Count; i++)
                {
                    if (Utility.InRange(this.Home, MapPaths[i].Item1, 25))
                    {
                        m_MobilePath = MapPaths[i].Item2;
                        break;
                    }
                    else
                        continue;

                }
            }
            #endregion

            #region Trammel
            else if (this.Map == Map.Trammel)
            {
                List<Tuple<Point3D, List<Tuple<Point3D, Direction>>>> MapPaths = ActionAI_MinerPaths_Trammel.ListOfPaths;


                for (int i = 0; i < MapPaths.Count; i++)
                {
                    if (Utility.InRange(this.Home, MapPaths[i].Item1, 25))
                    {
                        m_MobilePath = MapPaths[i].Item2;
                        break;
                    }
                    else
                        continue;

                }
            }
            #endregion

            /* 
            // THIS SECTION IS BLOCKED OUT UNTIL LISTS ARE FILLED OUT ABOVE

            #region Ilshenar
            if(this.Map == Map.Ilshenar)
            {
                List<Tuple<Point3D,List<Tuple<Point3D,Direction>>>> MapPaths = ActionAI_MinerPaths_Ilshenar.Miner_ListOfPaths;


                for( int i = 0; i < MapPaths.Count; i++)
                {
                    if( Utility.InRange( this.Home, MapPaths[i].Item1, 25 ) )
                    {
                        m_MobilePath = MapPaths[i].Item2;
                        break;
                    }
                    else
                        continue;
                    
                }
            }
            #endregion

            #region Malas
            if(this.Map == Map.Malas)
            {
                List<Tuple<Point3D,List<Tuple<Point3D,Direction>>>> MapPaths = ActionAI_MinerPaths_Malas.Miner_ListOfPaths;


                for( int i = 0; i < MapPaths.Count; i++)
                {
                    if( Utility.InRange( this.Home, MapPaths[i].Item1, 25 ) )
                    {
                        m_MobilePath = MapPaths[i].Item2;
                        break;
                    }
                    else
                        continue;
                    
                }
            }
            #endregion

            #region Tokuno
            if(this.Map == Map.Tokuno)
            {
                List<Tuple<Point3D,List<Tuple<Point3D,Direction>>>> MapPaths = ActionAI_MinerPaths_Tokuno.Miner_ListOfPaths;


                for( int i = 0; i < MapPaths.Count; i++)
                {
                    if( Utility.InRange( this.Home, MapPaths[i].Item1, 25 ) )
                    {
                        m_MobilePath = MapPaths[i].Item2;
                        break;
                    }
                    else
                        continue;
                    
                }
            }
            #endregion

            */

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
        }

        public void CreateCamp()
        {
            if (!Alive && Deleted)
            {
                return;
            }

            Home = this.Location;

            SetPath();

            MinerCamp camp = new MinerCamp();

            camp.MoveToWorld(this.Location, this.Map);

            m_Camp = camp;

            if (Backpack == null)
                AddItem(new Backpack());

            if (m_MobilePath == null)
            {
                return;
            }

            // Create the first Waypoint
            m_waypointFirst = new WayPoint();
            m_waypointFirst.MoveToWorld(m_MobilePath[0].Item1, Map);

            CurrentWayPoint = m_waypointFirst;
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
        }


        public override void OnThink()
        {
            if (!Alive && Deleted)
            {
                return;
            }

            if (m_MobilePath == null || m_waypointFirst == null)
            {
                return;
            }

            if (Alive && !Deleted && m_waypointFirst != null && m_MobilePath != null)
            {
                if (m_waypointFirst.Location == Home)
                {
                    CurrentSpeed = 2.0;

                    Timer.DelayCall(TimeSpan.FromMinutes(5.0), MoveWayPoint);
                }

                if (Location != Home && m_waypointFirst != null && (m_waypointFirst.X == Location.X & m_waypointFirst.Y == Location.Y))
                {
                    CantWalk = true;
                    CurrentSpeed = 2.0;

                    Direction = m_MobilePath[m_Index].Item2;
                    Animate(11, 5, 1, true, false, 0);

                    PlaySound(Utility.RandomList(harvestDefinition.EffectSounds));
                }
                else
                {
                    CurrentSpeed = 0.2;
                    CantWalk = false;
                }
            }
        }

        public void MoveWayPoint()
        {
            if (!Alive && Deleted)
            {
                return;
            }

            if (Alive && !Deleted)
            {
                if (waypointFirst != null && (waypointFirst.X == Location.X & waypointFirst.Y == Location.Y))
                {
                    CantWalk = false;

                    if ((m_Index + 1) < m_MobilePath.Count)
                    {
                        m_Index++;

                        waypointFirst.Location = m_MobilePath[m_Index].Item1;
                        CurrentWayPoint = waypointFirst;
                        Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
                    }
                    else
                    {
                        m_Index = 0;
                        waypointFirst.Location = Home;
                        CurrentWayPoint = waypointFirst;
                    }
                }
            }
        }

        // separate method in case mobile isn't on a way point at startup, this will force them to start moving again
        public void MoveWayPointOnDeserialize()
        {
            if (!Alive && Deleted)
            {
                return;
            }

            if (Alive && !Deleted)
            {
                SetPath();

                if (m_MobilePath == null)
                {
                    return;
                }

                CurrentWayPoint = waypointFirst;

                if (waypointFirst != null)
                {
                    CantWalk = false;

                    if ((m_Index + 1) < m_MobilePath.Count)
                    {
                        m_Index++;

                        waypointFirst.Location = m_MobilePath[m_Index].Item1;
                        CurrentWayPoint = waypointFirst;
                        Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
                    }
                    else
                    {
                        m_Index = 0;
                        waypointFirst.Location = Home;
                        CurrentWayPoint = waypointFirst;
                    }
                }
            }
        }

        public override void OnDelete()
        {
            if (m_Camp != null && !m_Camp.Deleted)
                m_Camp.Delete();

            base.OnDelete();
        }

        public ActionAI_Miner(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // version 

            //version 0
            writer.Write(m_Camp);
            writer.Write(m_waypointFirst);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Camp = reader.ReadItem() as MinerCamp;
            m_waypointFirst = reader.ReadItem() as WayPoint;

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPointOnDeserialize);
        }
    }
}