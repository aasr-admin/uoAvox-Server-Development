﻿using Server;
using Server.Engines.Harvest;
using Server.Items;
using Server.Misc;
using Server.Multis;

using System;
using System.Collections.Generic;

using System.Linq;

/// Description:
/// this mobile will harvest resources just as if they were real players on your server. They will appear
/// to place a camp, work (harvest their designated resource), move to the next waypoint, and then when
/// their work is done they will return to their camp (or home location) and drop off their harvest into a
/// crate which will self-delete (with its contents) after a duration; players can loot these crates and
/// obtain free resources should they stumble upon these mobiles.

/// the path they follow will be auto-generated after spawning and will be re-generated with each loop after
/// returnting to their home location and waiting for a predefined amount of time.
/// the list will first be created via a hashset to prevent duplicating locations, it will then be 
/// converted to a List so an indexer can be used to move the waypoint to each "point" in the List.

/*
    STILL TODO: LOS CHECK FOR MOVING WAYPOINT
*/

namespace Server.Mobiles
{
    public class ActionAI_Miner : BaseCreature
    {
        private MinerCamp m_Camp;
        public PathFollower m_Path;
        private int m_Index;
        private WayPoint m_waypointFirst;
        private List<Tuple<Point3D, Direction>> m_MobilePath;

        private HashSet<Point3D> points;
        private List<Point3D> pointsList;

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
            if (!Alive && Deleted && (Map == Map.Internal))
            {
                return;
            }

            int range = 10;
			Map map = this.Map;
			// use a hashset as an easy way to prevent duplicates
			points = new HashSet<Point3D>();
			HashSet<Point3D> obstacles = new HashSet<Point3D>();

			for (var xx = this.X - range; xx <= this.X + range; xx++) 
            {
                for (var yy = this.Y - range; yy <= this.Y + range; yy++)
                {
                    StaticTile[] tiles = map.Tiles.GetStaticTiles(xx, yy, true);

                    if(tiles.Length == 0)
                        continue; 
                    else
                    {
						if (m_Obstacles.Contains(tiles[0].ID))
							continue;

						else if (m_MiningTiles.Contains(tiles[0].ID))
                        { 
                            points.Add(new Point3D(xx, yy, tiles[0].Z ));
                        }
                    }
                } 
            }

            // convert hashset to list so we can use an indexer
            pointsList = points.ToList();

			// remove every other entry point so mobile is going through each tile 1 by 1
			for (int i = (pointsList.Count - 1); i > 0; i--)
			{
				if (i % 2 == 0)
					pointsList.RemoveAt(i);
			}

			// cause... 1 isn't divisible by 2, is it?
			// plus this will keep the "order" the mob's movement looking more uniform, like a real player using a macro
			pointsList.RemoveAt(1);

			Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
        }

        public void CreateCamp()
        {
            if (!Alive && Deleted)
            {
                return;
            }

            Home = this.Location;

            MinerCamp camp = new MinerCamp();
            camp.MoveToWorld(this.Location, this.Map);
            m_Camp = camp;

            if (Backpack == null)
            {
                AddItem(new Backpack());
            }

            SetPath();

            //if (m_MobilePath == null)
            if (pointsList == null)
            {
                return;
            }

            // Create the first Waypoint
            m_waypointFirst = new WayPoint();
            //m_waypointFirst.MoveToWorld(m_MobilePath[0].Item1, Map);
            m_waypointFirst.MoveToWorld(pointsList[0], Map);

            CurrentWayPoint = m_waypointFirst;
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
        }

        private static readonly int[] m_MiningTiles = new int[]
        {
            1339, 
            1340, 1341, 1342, 1343, 1344, 1345, 1346, 1347, 1348, 1349,
            /*1350, 1351, 1352, 1353, 1354, 1355, 1356, 1357, 1358, 1359, */
            /*1361, 1362, 1363,*/
            1386
        };

		private static readonly int[] m_Obstacles = new int[]
		{
			/* stalagtites, mites, SilverEtchedMace */
			2272, 2273, 2274, 2275, 2276, 2277, 2278, 2279, 2280, 2281, 2282, 

			/* anvil and forge */
			4015, 4016, 4017,
		};


		public override void OnThink()
        {
            if (!Alive && Deleted)
            {
                return;
            }

            //if (m_MobilePath == null || m_waypointFirst == null)
            if (pointsList == null || m_waypointFirst == null)
            {
                return;
            }

            if (pointsList == null || m_waypointFirst == null)
            {
                return;
            }

            if (Alive && !Deleted /* && m_waypointFirst != null && m_MobilePath != null */)
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

                     /*********
                    TODO: MAKE MOBILE FACE TREE BASED ON CALCULATION OF POINTS
                    **********/
                    //Direction = m_MobilePath[m_Index].Item2;
                    
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

                    //if ((m_Index + 1) < m_MobilePath.Count)
                    if ( (m_Index + 1) < pointsList.Count )
                    {
                        m_Index++;
						//waypointFirst.Location = m_MobilePath[m_Index].Item1;
						waypointFirst.Location = pointsList[m_Index];
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

                //if (m_MobilePath == null)
                if (pointsList == null)
                {
                    return;
                }

                CurrentWayPoint = waypointFirst;

                if (waypointFirst != null)
                {
                    CantWalk = false;

                    //if ((m_Index + 1) < m_MobilePath.Count)
                    if ((m_Index + 1) < pointsList.Count)
                    {
                        m_Index++;

                        //waypointFirst.Location = m_MobilePath[m_Index].Item1;
						waypointFirst.Location = pointsList[m_Index];
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