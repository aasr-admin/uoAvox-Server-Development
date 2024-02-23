using Server;
using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Multis;
using Server.Network;

using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Mobiles
{
    public class ActionAI_PirateCaptain : BaseCreature
    {
        private MediumBoat m_PirateShip_Boat;

        #region Pirate Can Say Random Phrases From A [.txt] File

        public bool active;

        public static string path = "Data/Speech/PirateCaptain.txt";

        private DateTime nextAbilityTime;

        private StreamReader text;

        private string curspeech;

        public override bool InitialInnocent { get { return true; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                if (!value)
                {
                    CloseStream();
                }

                active = value;
            }
        }

        #endregion Pirate Can Say Random Phrases From A [.txt] File

        [Constructable]
        public ActionAI_PirateCaptain() : base(AIType.AI_Archer, FightMode.Closest, 15, 1, 0.2, 2.0)
        {

            Hue = Utility.RandomSkinHue();

            if (this.Female = Utility.RandomBool())
            {
                Body = 0x191;
                Name = NameList.RandomName("female");
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("male");
            }

            Title = "[Pirate Captain]";

            #region Pirate Can Say Random Phrases From A [.txt] File

            SpeechHue = Utility.RandomDyedHue();

            #endregion Pirate Can Say Random Phrases From A [.txt] File

            AddItem(new Sandals());
            AddItem(new TricorneHat(Utility.RandomRedHue()));

            Item hair = new Item(Utility.RandomList(0x203B, 0x2049, 0x2048, 0x204A));
            hair.Hue = Utility.RandomNondyedHue();
            hair.Layer = Layer.Hair;
            hair.Movable = false;
            AddItem(hair);

            if (Utility.RandomBool() && !this.Female)
            {
                Item beard = new Item(Utility.RandomList(0x203E, 0x203F, 0x2040, 0x2041, 0x204B, 0x204C, 0x204D));

                beard.Hue = hair.Hue;
                beard.Layer = Layer.FacialHair;
                beard.Movable = false;

                AddItem(beard);

                Item necklace = new Necklace();
                necklace.Name = "A Pirates Medallion";
                necklace.Movable = false;
                necklace.Hue = 38;

                AddItem(necklace);
            }

            SetStr(195, 200);
            SetDex(181, 195);
            SetInt(61, 75);
            SetHits(288, 308);

            SetDamage(20, 40);

            SetDamageType(ResistanceType.Physical, 100, 100);
            SetDamageType(ResistanceType.Fire, 25, 50);
            SetDamageType(ResistanceType.Cold, 25, 50);
            SetDamageType(ResistanceType.Energy, 25, 50);
            SetDamageType(ResistanceType.Poison, 25, 50);

            SetResistance(ResistanceType.Physical, 100, 100);
            SetResistance(ResistanceType.Fire, 25, 50);
            SetResistance(ResistanceType.Cold, 25, 50);
            SetResistance(ResistanceType.Energy, 25, 50);
            SetResistance(ResistanceType.Poison, 25, 50);

            SetSkill(SkillName.Fencing, 86.0, 97.5);
            SetSkill(SkillName.Macing, 85.0, 87.5);
            SetSkill(SkillName.MagicResist, 55.0, 67.5);
            SetSkill(SkillName.Swords, 85.0, 87.5);
            SetSkill(SkillName.Tactics, 85.0, 87.5);
            SetSkill(SkillName.Wrestling, 35.0, 37.5);
            SetSkill(SkillName.Archery, 85.0, 87.5);

            #region Pirate Can Say Random Phrases From A [.txt] File

            active = true;

            #endregion Pirate Can Say Random Phrases From A [.txt] File

            CantWalk = false;

            Fame = 5000;
            Karma = -5000;
            VirtualArmor = 5;

            {
                switch (Utility.Random(1))
                {
                    case 0: AddItem(new LongPants(Utility.RandomRedHue())); break;
                    case 1: AddItem(new ShortPants(Utility.RandomRedHue())); break;
                }

                switch (Utility.Random(3))
                {
                    case 0: AddItem(new FancyShirt(Utility.RandomRedHue())); break;
                    case 1: AddItem(new Shirt(Utility.RandomRedHue())); break;
                    case 2: AddItem(new Doublet(Utility.RandomRedHue())); break;
                }

                switch (Utility.Random(5))
                {
                    case 0: AddItem(new Bow()); break;
                    case 1: AddItem(new CompositeBow()); break;
                    case 2: AddItem(new Crossbow()); break;
                    case 3: AddItem(new RepeatingCrossbow()); break;
                    case 4: AddItem(new HeavyCrossbow()); break;
                }
            }

            Timer.DelayCall(Create_BoatAndCrew);
        }

        private void Create_BoatAndCrew()
        {
            Direction m_Direction = Direction.North;

            switch (Utility.Random(4))
            {
                case 0:
                    m_Direction = Direction.North;
                    break;
                case 1:
                    m_Direction = Direction.South;
                    break;
                case 2:
                    m_Direction = Direction.West;
                    break;
                case 3:
                    m_Direction = Direction.East;
                    break;
            }

            if (boatspawn == false)
            {
                Map map = this.Map;
                if (map == null)
                    return;

                m_PirateShip_Boat = new MediumBoat();
                m_PirateShip_Boat.Facing = m_Direction;
                Point3D loc = this.Location;
                m_PirateShip_Boat.ShipName = "Pirate Boat";

                m_PirateShip_Boat.Owner = this;

                Point3D loccrew = this.Location;

                loc = new Point3D(this.X, this.Y - 1, this.Z);
                loccrew = new Point3D(this.X, this.Y - 1, this.Z + 1);

                m_PirateShip_Boat.MoveToWorld(loc, map);
                boatspawn = true;

                for (int i = 0; i < 5; ++i)
                {
                    ActionAI_PirateCrew m_PirateCrew = new ActionAI_PirateCrew();
                    m_PirateCrew.MoveToWorld(loccrew, map);
                }
            }
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.PiratesAndSeafarers; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich, 2);
        }

        public override bool IsScaredOfScaryThings { get { return false; } }
        public override bool AlwaysMurderer { get { return true; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override bool AutoDispel { get { return true; } }
        public override bool CanRummageCorpses { get { return true; } }

        #region Pirate Can Say Random Phrases From A [.txt] File

        public void Emote()
        {
            switch (Utility.Random(85))
            {
                case 1:
                    PlaySound(Female ? 785 : 1056);
                    Say("*cough!*");
                    break;
                case 2:
                    PlaySound(Female ? 818 : 1092);
                    Say("*sniff*");
                    break;
                default:
                    break;
            }
        }

        public void CloseStream()
        {
            if (text != null)
            {
                try { text.Close(); text = null; }
                catch { };
            }
        }

        public void Talk()
        {
            if (text == null) return;

            try
            {
                curspeech = text.ReadLine();

                if (curspeech == null) throw (new ArgumentNullException());

                Say(curspeech);
            }
            catch
            {
                CloseStream();
            }
        }

        public override void OnDeath(Container c)
        {
            CloseStream();
            base.OnDeath(c);
        }

        #endregion Pirate Can Say Random Phrases From A [.txt] File

        public override bool PlayerRangeSensitive { get { return false; } }

        private bool boatspawn;
        private DateTime m_NextPickup;
        private Mobile m_Mobile;
        private BaseBoat m_enemyboat;
        private ArrayList list;
        private Direction enemydirection;

        public override void OnThink()
        {

            Direction m_Direction = Direction.North;

            switch (Utility.Random(4))
            {
                case 0:
                    m_Direction = Direction.North;
                    break;
                case 1:
                    m_Direction = Direction.South;
                    break;
                case 2:
                    m_Direction = Direction.West;
                    break;
                case 3:
                    m_Direction = Direction.East;
                    break;
            }

            #region Pirate Can Say Random Phrases From A [.txt] File

            if (DateTime.Now >= nextAbilityTime && Combatant == null && active == true)
            {
                nextAbilityTime = DateTime.Now + TimeSpan.FromSeconds(Utility.RandomMinMax(4, 6));

                if (text == null)
                {
                    try
                    {
                        text = new StreamReader(path, System.Text.Encoding.Default, false);
                    }
                    catch { }
                }

                Talk();
                Emote();
            }

            #endregion Pirate Can Say Random Phrases From A [.txt] File

            int range = 15;

            if (DateTime.Now < m_NextPickup)
                return;

            if (m_PirateShip_Boat == null)
            {
                return;
            }

            m_NextPickup = DateTime.Now + TimeSpan.FromSeconds(Utility.RandomMinMax(1, 2));

            enemydirection = Direction.North;


            switch (4)
            {
                case 0:
                    m_Direction = Direction.North;
                    break;
                case 1:
                    m_Direction = Direction.South;
                    break;
                case 2:
                    m_Direction = Direction.West;
                    break;
                case 3:
                    m_Direction = Direction.East;
                    break;
            }

            if (m_PirateShip_Boat.Moving != Direction.South)
            {
                if (m_PirateShip_Boat.Anchored == true)
                    m_PirateShip_Boat.Anchored = false;

                m_PirateShip_Boat.StartMove(Direction.North, false);
            }

            foreach (Item enemy in this.GetItemsInRange(50)) //200
            {
                if (enemy is BaseBoat && enemy != m_PirateShip_Boat) // && !(enemy is PirateShip_Boat))
                {
                    Mobile owner = null;

                    if (((BaseBoat)enemy).Owner != null && ((BaseBoat)enemy).ShipName != "Pirate Boat" && ((BaseBoat)enemy).Owner.Karma >= 0)
                    {
                        owner = ((BaseBoat)enemy).Owner;

                        List<Mobile> targets = new List<Mobile>();
                        IPooledEnumerable eable = enemy.GetMobilesInRange(16);

                        foreach (Mobile m in eable)
                        {
                            if (m is PlayerMobile)
                                targets.Add(m);
                        }

                        eable.Free();

                        if (targets.Count > 0)
                        {
                            m_enemyboat = enemy as BaseBoat;

                            enemydirection = this.GetDirectionTo(m_enemyboat);

                            if (this.Location == m_enemyboat.Location)
                                return;

                            if (this.InRange(m_enemyboat.GetWorldLocation(), 10))
                            {
                                Plank plank = null;

                                if (this.InRange(m_enemyboat.PPlank.GetWorldLocation(), 10))
                                    plank = m_enemyboat.PPlank;
                                else if (this.InRange(m_enemyboat.SPlank.GetWorldLocation(), 10))
                                    plank = m_enemyboat.SPlank;

                                if (plank != null)
                                {
                                    Point3D loc = new Point3D(m_enemyboat.X, m_enemyboat.Y, m_enemyboat.Z);

                                    m_PirateShip_Boat.Anchored = true;

                                    if (plank.Locked)
                                    {
                                        this.MovingParticles(plank, 0x1C19, 1, 0, false, true, 0, 0, 9502, 6014, 0x11D, EffectLayer.Waist, 0);
                                        Effects.PlaySound(loc, m_enemyboat.Map, 0x307);
                                        Effects.SendLocationEffect(plank.Location, m_enemyboat.Map, 0x36BD, 20, 10);
                                        plank.Locked = false;
                                        plank.Open();
                                    }


                                    if (!m_enemyboat.Owner.Alive && !m_PirateShip_Boat.Deleted)
                                    {
                                        this.Location = new Point3D(m_PirateShip_Boat.X + 2, m_PirateShip_Boat.Y + 2, m_PirateShip_Boat.Z + 3);

                                        m_PirateShip_Boat.Anchored = false;

                                        if (!m_PirateShip_Boat.Move(Direction.North, 1, 0x4, false))
                                        {
                                            if (m_PirateShip_Boat.Anchored == true)
                                                m_PirateShip_Boat.Anchored = false;

                                            m_PirateShip_Boat.StartMove(Direction.South, true);

                                            Timer.DelayCall(TimeSpan.FromSeconds(0.5),
                                                delegate
                                                {
                                                    m_PirateShip_Boat.StopMove(true);

                                                    switch (Utility.Random(4))
                                                    {
                                                        case 0:
                                                            m_Direction = Direction.North;
                                                            break;
                                                        case 1:
                                                            m_Direction = Direction.South;
                                                            break;
                                                        case 2:
                                                            m_Direction = Direction.West;
                                                            break;
                                                        case 3:
                                                            m_Direction = Direction.East;
                                                            break;
                                                    }

                                                    m_PirateShip_Boat.SetFacing(m_Direction);

                                                    if (m_PirateShip_Boat.Moving != Direction.South)
                                                    {
                                                        if (m_PirateShip_Boat.Anchored == true)
                                                            m_PirateShip_Boat.Anchored = false;

                                                        m_PirateShip_Boat.StartMove(Direction.North, false);
                                                    }

                                                });
                                        }
                                    }

                                    if (m_enemyboat != null && !m_enemyboat.Deleted)
                                    {
                                        if (m_enemyboat.Owner != null && !m_enemyboat.Owner.Alive)
                                            m_enemyboat.Anchored = false;

                                        Timer.DelayCall(TimeSpan.FromSeconds(0.3),
                                            delegate
                                            {

                                                m_PirateShip_Boat.Anchored = true;

                                                if (m_enemyboat != null)
                                                {
                                                    if (this.InRange(m_PirateShip_Boat.GetWorldLocation(), 2) && m_enemyboat.Owner.Alive)
                                                    {
                                                        this.Location = new Point3D(m_enemyboat.X + 2, m_enemyboat.Y + 2, m_enemyboat.Z + 3);
                                                    }
                                                }
                                            });
                                    }

                                    Timer.DelayCall(TimeSpan.FromSeconds(3.0),
                                        delegate
                                        {
                                            if (m_enemyboat != null)
                                            {
                                                if (m_enemyboat != null && m_enemyboat.Owner != null && !this.InRange(m_enemyboat.Owner.Location, 8))
                                                {
                                                    this.Location = new Point3D(m_enemyboat.Owner.X + 1, m_enemyboat.Owner.Y + 1, m_enemyboat.Owner.Z);
                                                }
                                            }
                                        });



                                }
                            }

                            if (m_PirateShip_Boat != null &&
                                (m_PirateShip_Boat.Location.X >= (m_enemyboat.X + range) & m_PirateShip_Boat.Location.Y >= (m_enemyboat.Y + range))
                                || (m_PirateShip_Boat.Location.X <= (m_enemyboat.X - range) & m_PirateShip_Boat.Location.Y <= (m_enemyboat.Y - range)))
                            {
                                m_enemyboat.Anchored = true;

                                if (m_PirateShip_Boat != null && (m_enemyboat.Facing == Direction.North) && m_PirateShip_Boat.Facing != Direction.North)
                                {
                                    m_PirateShip_Boat.Facing = Direction.North;
                                }
                                else if (m_PirateShip_Boat != null && (m_enemyboat.Facing == Direction.South) && m_PirateShip_Boat.Facing != Direction.South)
                                {
                                    m_PirateShip_Boat.Facing = Direction.South;
                                }
                                else if (m_PirateShip_Boat != null && (m_enemyboat.Facing == Direction.East || m_enemyboat.Facing == Direction.Right /* NorthEast */ || m_enemyboat.Facing == Direction.Down /* SouthEast */) && m_PirateShip_Boat.Facing != Direction.East)
                                {
                                    m_PirateShip_Boat.Facing = Direction.East;
                                }
                                else if (m_PirateShip_Boat != null && (m_enemyboat.Facing == Direction.West || m_enemyboat.Facing == Direction.Left /* SouthWest */ || m_enemyboat.Facing == Direction.Up) && m_PirateShip_Boat.Facing != Direction.West)
                                {
                                    m_PirateShip_Boat.Facing = Direction.West;
                                }

                            }
                            break;
                        }
                        else
                        {


                            if (m_PirateShip_Boat != null && !m_PirateShip_Boat.Deleted)
                            {
                                m_PirateShip_Boat.Anchored = false;

                                Point3D oldPoint = m_PirateShip_Boat.Location;

                                if (!m_PirateShip_Boat.Move(Direction.North, 1, 0x4, false))
                                {
                                    if (m_PirateShip_Boat.Anchored == true)
                                        m_PirateShip_Boat.Anchored = false;

                                    m_PirateShip_Boat.StartMove(Direction.South, true);

                                    Timer.DelayCall(TimeSpan.FromSeconds(0.5),
                                        delegate
                                        {
                                            m_PirateShip_Boat.StopMove(true);

                                            switch (Utility.Random(4))
                                            {
                                                case 0:
                                                    m_Direction = Direction.North;
                                                    break;
                                                case 1:
                                                    m_Direction = Direction.South;
                                                    break;
                                                case 2:
                                                    m_Direction = Direction.West;
                                                    break;
                                                case 3:
                                                    m_Direction = Direction.East;
                                                    break;
                                            }

                                            m_PirateShip_Boat.SetFacing(m_Direction);

                                            if (m_PirateShip_Boat.Moving != Direction.South)
                                            {
                                                if (m_PirateShip_Boat.Anchored == true)
                                                    m_PirateShip_Boat.Anchored = false;

                                                m_PirateShip_Boat.StartMove(Direction.North, false);
                                            }

                                        });
                                }
                            }

                            return;


                        }
                    }
                }
            }
            if (m_enemyboat == null)
            {

                if (m_PirateShip_Boat != null && !m_PirateShip_Boat.Deleted)
                {
                    m_PirateShip_Boat.Anchored = false;

                    Point3D oldPoint = m_PirateShip_Boat.Location;

                    if (!m_PirateShip_Boat.Move(Direction.North, 1, 0x4, false))
                    {
                        if (m_PirateShip_Boat.Anchored == true)
                            m_PirateShip_Boat.Anchored = false;

                        m_PirateShip_Boat.StartMove(Direction.South, true);

                        Timer.DelayCall(TimeSpan.FromSeconds(0.5),
                            delegate
                            {
                                m_PirateShip_Boat.StopMove(true);

                                switch (Utility.Random(4))
                                {
                                    case 0:
                                        m_Direction = Direction.North;
                                        break;
                                    case 1:
                                        m_Direction = Direction.South;
                                        break;
                                    case 2:
                                        m_Direction = Direction.West;
                                        break;
                                    case 3:
                                        m_Direction = Direction.East;
                                        break;
                                }

                                m_PirateShip_Boat.SetFacing(m_Direction);

                                if (m_PirateShip_Boat.Moving != Direction.South)
                                {
                                    if (m_PirateShip_Boat.Anchored == true)
                                        m_PirateShip_Boat.Anchored = false;

                                    m_PirateShip_Boat.StartMove(Direction.North, false);
                                }

                            });
                    }
                }

                return;


            }

            if (m_PirateShip_Boat != null && m_enemyboat != null)
            {
                if (this.Location == m_enemyboat.Location)
                    return;

                if (this.InRange(m_enemyboat.GetWorldLocation(), 10))
                {
                    Plank plank = null;

                    if (this.InRange(m_enemyboat.PPlank.GetWorldLocation(), 10))
                        plank = m_enemyboat.PPlank;
                    else if (this.InRange(m_enemyboat.SPlank.GetWorldLocation(), 10))
                        plank = m_enemyboat.SPlank;

                    if (plank != null)
                    {
                        Point3D loc = new Point3D(m_enemyboat.X, m_enemyboat.Y, m_enemyboat.Z);

                        m_PirateShip_Boat.Anchored = true;

                        if (plank.Locked)
                        {
                            this.MovingParticles(plank, 0x1C19, 1, 0, false, true, 0, 0, 9502, 6014, 0x11D, EffectLayer.Waist, 0);
                            Effects.PlaySound(loc, m_enemyboat.Map, 0x307);
                            Effects.SendLocationEffect(plank.Location, m_enemyboat.Map, 0x36BD, 20, 10);
                            plank.Locked = false;
                            plank.Open();
                        }

                        if (!m_enemyboat.Owner.Alive && !m_PirateShip_Boat.Deleted)
                        {
                            this.Location = new Point3D(m_PirateShip_Boat.X + 2, m_PirateShip_Boat.Y + 2, m_PirateShip_Boat.Z + 3);

                            m_PirateShip_Boat.Anchored = false;
                            m_enemyboat = null;

                            if (m_PirateShip_Boat.Moving != Direction.South)
                            {
                                if (m_PirateShip_Boat.Anchored == true)
                                    m_PirateShip_Boat.Anchored = false;

                                m_PirateShip_Boat.StartMove(Direction.North, false);

                            }

                            return;
                        }

                        if (m_enemyboat != null && m_enemyboat.Deleted)
                        {
                            if (m_enemyboat.Owner != null && !m_enemyboat.Owner.Alive)
                                m_enemyboat.Anchored = false;

                            Timer.DelayCall(TimeSpan.FromSeconds(0.3),
                                delegate
                                {
                                    m_PirateShip_Boat.StopMove(true);
                                    m_PirateShip_Boat.Anchored = true;

                                    if (m_enemyboat != null)
                                    {
                                        if (this.InRange(m_PirateShip_Boat.GetWorldLocation(), 2) && m_enemyboat.Owner.Alive)
                                        {
                                            this.Location = new Point3D(m_enemyboat.X + 2, m_enemyboat.Y + 2, m_enemyboat.Z + 3);
                                        }
                                    }
                                });
                        }

                        Timer.DelayCall(TimeSpan.FromSeconds(3.0),
                            delegate
                            {
                                if (m_enemyboat != null)
                                {
                                    if (m_enemyboat != null && m_enemyboat.Owner != null && !this.InRange(m_enemyboat.Owner.Location, 8))
                                    {
                                        this.Location = new Point3D(m_enemyboat.Owner.X + 1, m_enemyboat.Owner.Y + 1, m_enemyboat.Owner.Z);
                                    }
                                }
                            });



                    }
                }

                if (m_enemyboat != null)
                {
                    if (m_PirateShip_Boat != null &&
                        (m_PirateShip_Boat.Location.X >= (m_enemyboat.X + range) & m_PirateShip_Boat.Location.Y >= (m_enemyboat.Y + range))
                        || (m_PirateShip_Boat.Location.X <= (m_enemyboat.X - range) & m_PirateShip_Boat.Location.Y <= (m_enemyboat.Y - range)))
                    {

                        m_enemyboat.Anchored = true;

                        if (m_PirateShip_Boat != null && (m_enemyboat.Facing == Direction.North) && m_PirateShip_Boat.Facing != Direction.North)
                        {
                            m_PirateShip_Boat.Facing = Direction.North;
                        }
                        else if (m_PirateShip_Boat != null && (m_enemyboat.Facing == Direction.South) && m_PirateShip_Boat.Facing != Direction.South)
                        {
                            m_PirateShip_Boat.Facing = Direction.South;
                        }
                        else if (m_PirateShip_Boat != null && (m_enemyboat.Facing == Direction.East || m_enemyboat.Facing == Direction.Right /* NorthEast */ || m_enemyboat.Facing == Direction.Down /* SouthEast */) && m_PirateShip_Boat.Facing != Direction.East)
                        {
                            m_PirateShip_Boat.Facing = Direction.East;
                        }
                        else if (m_PirateShip_Boat != null && (m_enemyboat.Facing == Direction.West || m_enemyboat.Facing == Direction.Left /* SouthWest */ || m_enemyboat.Facing == Direction.Up) && m_PirateShip_Boat.Facing != Direction.West)
                        {
                            m_PirateShip_Boat.Facing = Direction.West;
                        }
                    }

                    if (m_PirateShip_Boat != null && (enemydirection == Direction.North) && m_PirateShip_Boat.Facing != Direction.North)
                    {
                        m_PirateShip_Boat.Facing = Direction.North;
                    }
                    else if (m_PirateShip_Boat != null && (enemydirection == Direction.South) && m_PirateShip_Boat.Facing != Direction.South)
                    {
                        m_PirateShip_Boat.Facing = Direction.South;
                    }
                    else if (m_PirateShip_Boat != null && (enemydirection == Direction.East || enemydirection == Direction.Right /* NorthEast */ || enemydirection == Direction.Down /* SouthEast */) && m_PirateShip_Boat.Facing != Direction.East)
                    {
                        m_PirateShip_Boat.Facing = Direction.East;
                    }
                    else if (m_PirateShip_Boat != null && (enemydirection == Direction.West || enemydirection == Direction.Left /* SouthWest */ || enemydirection == Direction.Up) && m_PirateShip_Boat.Facing != Direction.West)
                    {
                        m_PirateShip_Boat.Facing = Direction.West;
                    }

                    if (m_PirateShip_Boat.Moving != Direction.South)
                    {
                        if (m_PirateShip_Boat.Anchored == true)
                            m_PirateShip_Boat.Anchored = false;

                        m_PirateShip_Boat.StartMove(Direction.North, false);
                    }
                }

            }

            base.OnThink();
        }


        public override void OnDelete()
        {
        }

        public override void OnDamagedBySpell(Mobile caster)
        {
            if (caster == this)
                return;
        }

        public void SpawnPirate(Mobile target)
        {
            Map map = target.Map;

            if (map == null)
                return;

            int pirates = 0;

            foreach (Mobile m in this.GetMobilesInRange(10))
            {
                if (m is ActionAI_PirateCrew)
                    ++pirates;
            }

            if (pirates < 10 && Utility.RandomDouble() <= 0.25)
            {
                BaseCreature PirateCrew = new ActionAI_PirateCrew();

                Point3D loc = target.Location;
                bool validLocation = false;

                for (int j = 0; !validLocation && j < 10; ++j)
                {
                    int x = target.X + Utility.Random(3) - 1;
                    int y = target.Y + Utility.Random(3) - 1;
                    int z = map.GetAverageZ(x, y);

                    if (validLocation = map.CanFit(x, y, this.Z, 16, false, false))
                        loc = new Point3D(x, y, Z);
                    else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                        loc = new Point3D(x, y, z);
                }

                PirateCrew.MoveToWorld(loc, map);

                PirateCrew.Combatant = target;
            }
        }

        public override bool OnBeforeDeath()
        {
            if (this.LastKiller != null && this.LastKiller is PlayerMobile)
            {
                this.LastKiller.SendGump(new BoatDecisionGump(this.LastKiller, m_PirateShip_Boat));
            }
            else
            {
                new SinkTimer(m_PirateShip_Boat, m_PirateShip_Boat.Z).Start();
            }

            return true;
        }





        /* public class SinkTimer : Timer
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
        } */

        public ActionAI_PirateCaptain(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((Item)m_PirateShip_Boat);
            writer.Write((bool)boatspawn);
            writer.Write((int)0);

            #region Pirate Can Say Random Phrases From A [.txt] File

            writer.Write((bool)active);

            #endregion Pirate Can Say Random Phrases From A [.txt] File

        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            m_PirateShip_Boat = reader.ReadItem() as MediumBoat;
            boatspawn = reader.ReadBool();
            int version = reader.ReadInt();

            #region Pirate Can Say Random Phrases From A [.txt] File

            active = reader.ReadBool();

            #endregion Pirate Can Say Random Phrases From A [.txt] File

        }
    }

    [CorpseName("A Pirate's Corpse")]
    public class ActionAI_PirateCrew : BaseCreature
    {
        [Constructable]
        public ActionAI_PirateCrew() : base(AIType.AI_Archer, FightMode.Closest, 15, 1, 0.2, 0.4)
        {
            SpeechHue = Utility.RandomDyedHue();
            Hue = Utility.RandomSkinHue();

            if (this.Female = Utility.RandomBool())
            {
                Body = 0x191;
                Name = NameList.RandomName("female");
            }
            else
            {
                Body = 0x190;
                Name = NameList.RandomName("male");
            }

            Title = "[Crew]";

            AddItem(new ThighBoots());

            Item hair = new Item(Utility.RandomList(0x203B, 0x2049, 0x2048, 0x204A));
            hair.Hue = Utility.RandomNondyedHue();
            hair.Layer = Layer.Hair;
            hair.Movable = false;
            AddItem(hair);

            if (Utility.RandomBool() && !this.Female)
            {
                Item beard = new Item(Utility.RandomList(0x203E, 0x203F, 0x2040, 0x2041, 0x204B, 0x204C, 0x204D));

                beard.Hue = hair.Hue;
                beard.Layer = Layer.FacialHair;
                beard.Movable = false;

                AddItem(beard);
            }

            SetStr(195, 200);
            SetDex(181, 195);
            SetInt(61, 75);
            SetHits(288, 308);

            SetDamage(20, 40);

            SetSkill(SkillName.Fencing, 86.0, 97.5);
            SetSkill(SkillName.Macing, 85.0, 87.5);
            SetSkill(SkillName.MagicResist, 55.0, 67.5);
            SetSkill(SkillName.Swords, 85.0, 87.5);
            SetSkill(SkillName.Tactics, 85.0, 87.5);
            SetSkill(SkillName.Wrestling, 35.0, 37.5);
            SetSkill(SkillName.Archery, 85.0, 87.5);

            CantWalk = false;

            Fame = 2000;
            Karma = -2000;
            VirtualArmor = 66;


            switch (Utility.Random(1))
            {
                case 0: AddItem(new LongPants(Utility.RandomRedHue())); break;
                case 1: AddItem(new ShortPants(Utility.RandomRedHue())); break;
            }

            switch (Utility.Random(3))
            {
                case 0: AddItem(new FancyShirt(Utility.RandomRedHue())); break;
                case 1: AddItem(new Shirt(Utility.RandomRedHue())); break;
                case 2: AddItem(new Doublet(Utility.RandomRedHue())); break;
            }


            switch (Utility.Random(3))
            {
                case 0: AddItem(new Bandana(Utility.RandomRedHue())); break;
                case 1: AddItem(new SkullCap(Utility.RandomRedHue())); break;
            }

            switch (Utility.Random(4))
            {
                case 0: AddItem(new Bow()); break;
                case 1: AddItem(new CompositeBow()); break;

                case 2:
                    AddItem(new Cutlass());
                    ChangeAIType(AIType.AI_Melee);
                    break;
                case 3:
                    AddItem(new Broadsword());
                    ChangeAIType(AIType.AI_Melee);
                    break;
            }
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.PiratesAndSeafarers; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
        }

        public override bool IsScaredOfScaryThings { get { return false; } }
        public override bool AlwaysMurderer { get { return true; } }
        public override bool CanRummageCorpses { get { return true; } }
        public override bool PlayerRangeSensitive { get { return false; } }

        public ActionAI_PirateCrew(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}