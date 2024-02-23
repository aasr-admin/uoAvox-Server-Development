using Server;
using Server.Engines.Harvest;
using Server.Items;
using Server.Misc;
using Server.Multis;

using System;
using System.Collections.Generic;

using System.Linq;

#region Developer Notations

#endregion

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

namespace Server.Mobiles
{
    public class ActionAI_Lumberjack : BaseCreature
    {
        public PathFollower m_Path;

        private LumberjackCamp m_Camp;
        private int m_Index;
        private WayPoint m_waypoint;
        private List<Tuple<Point3D, Direction>> m_MobilePath;

        private HashSet<Point3D> points;
        private List<Point3D> pointsList;

        /// Active Harvest System Mobile Uses
        public override HarvestDefinition harvestDefinition { get { return Lumberjacking.System.Definition; } }

        public override HarvestSystem harvestSystem { get { return Lumberjacking.System; } }

        public override bool PlayerRangeSensitive { get { return false; } } // Mobile will continue to follow waypoints even if players aren't around

        #region Getter And Setters

        [CommandProperty(AccessLevel.GameMaster)]
        public int Index
        {
            get { return m_Index; }
            set { m_Index = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WayPoint waypoint
        {
            get { return m_waypoint; }
            set { m_waypoint = value; }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public List<Tuple<Point3D, Direction>> MobilePath
        {
            get { return m_MobilePath; }
            set { m_MobilePath = value; }
        }

        #endregion

        #region Mobile Constructor

        [Constructable]
        public ActionAI_Lumberjack() : base(AIType.AI_ActionAI, FightMode.None, 10, 1, 0.2, 1.6)
        {
            Title = "The Lumberjack";

            Hue = Utility.RandomSkinHue();
            Utility.AssignRandomHair(this);
            SpeechHue = Utility.RandomDyedHue();

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

            InitStats(31, 41, 51);

            SetSkill(SkillName.Healing, 36, 68);
            SetSkill(SkillName.Lumberjacking, 200, 300);

            RangeHome = 0;

            /// Clothing Mobile Will Wear
            AddItem(new Doublet(Utility.RandomDyedHue()));
            AddItem(new Sandals(Utility.RandomNeutralHue()));
            AddItem(new ShortPants(Utility.RandomNeutralHue()));
            AddItem(new HalfApron(Utility.RandomDyedHue()));

            /// Utility Mobile Will Equip
            AddItem(new Hatchet());

            /// Mobile Backpack And Items
            Container backpack = new StrongBackpack();
            backpack.Movable = false;
            AddItem(backpack);

            backpack.DropItem(new Gold(250, 300));

            /// ActionAI Mobile Labor Camp
            RangeHome = 10;
            Timer.DelayCall(CreateCamp);
        }

        #endregion

        public void CreateCamp()
        {
            if (!Alive && Deleted)
            {
                return;
            }

            Home = this.Location;

            LumberjackCamp camp = new LumberjackCamp();
            camp.MoveToWorld(this.Location, this.Map);
            m_Camp = camp;

            if (Backpack == null)
            {
                AddItem(new StrongBackpack());
            }

            SetPath();

            if (pointsList == null)
            {
                return;
            }

            /// Create The First Waypoint
            m_waypoint = new WayPoint();
            m_waypoint.MoveToWorld(pointsList[0], Map);

            CurrentWayPoint = m_waypoint;
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
        }

        /// NPC Moves Using Standard WayPoints
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

            for (var xx = this.X - range; xx <= this.X + range; xx++) 
            {
                for (var yy = this.Y - range; yy <= this.Y + range; yy++)
                {
                    StaticTile[] tiles = map.Tiles.GetStaticTiles(xx, yy, true);

                    if(tiles.Length == 0)
                        continue; 
                    else
                    {
                        if (m_TreeTiles.Contains(tiles[0].ID))
                        { 
                            points.Add(new Point3D(xx + 1, yy, tiles[0].Z ));
                        }
                    }
                } 
            }

            // convert hashset to list so we can use an indexer
            pointsList = points.ToList();

            Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
        }

		#region Tile lists
		private static readonly int[] m_TreeTiles = new int[]
		{
			0x0CCA, 0x0CCB, 0x0CCC, 0x0CCD, 0x0CD0, 0x0CD6, 0x0CD8, 0x0CDA,
			0x0CDD, 0x0CE0, 0x0CE3, 0x0CE6, 0x0CF8, 0x0CFE, 0x0D01, 0x0D43,
			0x0D59, 0x0D70, 0x0D85, 0x0D94, 0x0D98, 0x0D9C, 0x0DA4, 0x0DA8,

			0x0CC9, 0x0CC8, 0x0CCE, 0x0CCF, 0x0CD1, 0x0CD2, 0x0CD4, 0x0CD5,
			0x0CD7, 0x0CD9, 0x0CDB, 0x0CDC, 0x0CDE, 0x0CDF, 0x0CE1, 0x0CE2,
			0x0CE4, 0x0CE5, 0x0CE7, 0x0CE8, 0x0CE9, 0x0CEA, 0x0CF9, 0x0CFA,
			0x0CFC, 0x0CFD, 0x0CFF, 0x0D00, 0x0D02, 0x0D03, 0x0D46, 0x0D47,
			0x0D48, 0x0D4E, 0x0D4F, 0x0D50, 0x0D5D, 0x0D5E, 0x0D5F, 0x0D64,
			0x0D65, 0x0D66, 0x0D74, 0x0D75, 0x0D76, 0x0D7B, 0x0D7C, 0x0D7D,
			0x0D88, 0x0D89, 0x0D8A, 0x0D8D, 0x0D8E, 0x0D8F, 0x0D95, 0x0D96,
			0x0D97, 0x0D99, 0x0D9A, 0x0D9B, 0x0D9D, 0x0D9E, 0x0D9F, 0x0DA1,
			0x0DA2, 0x0DA3, 0x0DA5, 0x0DA6, 0x0DA7, 0x0DA9, 0x0DAA, 0x0DAB,
			0x0DB7, 0x0C9E, 0x0C99, 0x0C9A, 0x0C9B, 0x0C9C, 0x0C9D, 0x0D3F,
			0x0D40
		};
		#endregion

		public override void OnThink()
        {
            if (!Alive && Deleted)
            {
                return;
            }

            if (pointsList == null || m_waypoint == null)
            {
                return;
            }

            if (Alive && !Deleted)
            {
                if (m_waypoint.Location == Home)
                {
                    CurrentSpeed = 2.0;

                    Timer.DelayCall(TimeSpan.FromMinutes(5.0), MoveWayPoint);
                }

                if (Location != Home && m_waypoint != null && (m_waypoint.X == Location.X & m_waypoint.Y == Location.Y))
                {
                    CantWalk = true;
                    CurrentSpeed = 2.0;

                    /*********
                    TODO: MAKE MOBILE FACE TREE BASED ON CALCULATION OF POINTS
                    **********/
                    //Direction = m_MobilePath[m_Index].Item2;

                    Animate(Utility.RandomList(harvestDefinition.EffectActions), 5, 1, true, false, 0);
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
                if (waypoint != null && (waypoint.X == Location.X & waypoint.Y == Location.Y))
                {
                    CantWalk = false;

                    if ( (m_Index + 1) < pointsList.Count )
                    {
                        m_Index++;
                        //Emote("moving to next point"); // Debug
                        waypoint.Location = pointsList[m_Index];
                        CurrentWayPoint = waypoint;
                        Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
                    }
                    else
                    {
                        m_Index = 0;
                        //Emote("returning to camp"); // Debug
                        waypoint.Location = Home;
                        CurrentWayPoint = waypoint;
                    }
                }
            }
        }

        /// Force NPC To Move On Server Startup
        public void MoveWayPointOnDeserialize()
        {
            if (!Alive && Deleted)
            {
                return;
            }

            if (Alive && !Deleted)
            {
                SetPath();
                CurrentWayPoint = waypoint;

                if (pointsList == null)
                {
                    return;
                }

                if (waypoint != null)
                {
                    CantWalk = false;

                    if ((m_Index + 1) < pointsList.Count)
                    {
                        m_Index++;
                        // Emote("moving to next point"); // Debug
                        waypoint.Location = pointsList[m_Index];
                        CurrentWayPoint = waypoint;

                        Timer.DelayCall(TimeSpan.FromSeconds(10.0), MoveWayPoint);
                    }
                    else
                    {
                        m_Index = 0;
                        // Emote("returning to camp"); // Debug
                        waypoint.Location = Home;
                        CurrentWayPoint = waypoint;
                    }
                }
            }
        }

        public override void OnDelete()
        {
            if (m_Camp != null && !m_Camp.Deleted)
            {
                m_Camp.Delete();
            }

            if (m_waypoint != null && !m_waypoint.Deleted)
            {
                m_waypoint.Delete();
            }

            base.OnDelete();
        }

        public ActionAI_Lumberjack(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)1); // Current Version 

            writer.Write(m_Camp);
            writer.Write(m_waypoint);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            m_Camp = reader.ReadItem() as LumberjackCamp;
            m_waypoint = reader.ReadItem() as WayPoint;

            Timer.DelayCall(TimeSpan.FromSeconds(3.0), SetPath);
        }
    }
}