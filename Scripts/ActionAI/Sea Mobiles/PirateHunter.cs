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

/// Reference: https://www.merriam-webster.com/dictionary/privateer

namespace Server.Mobiles
{
    [CorpseName("A Pirate Hunter Corpse")]
    public class ActionAI_PirateHunter : BaseCreature
    {
        private static string RandomCoastalTownList(params string[] list)
        {
            return list[RandomImpl.Next(list.Length)];
        }

        private string m_RandomCoastalTown = RandomCoastalTownList(
            "Britain", "Trinsic", "Jhelom", "Skara Brae", "Cove", "N'ujel'm",
            "Moonglow", "Magincia", "Ocllo", "Serpent's Hold"
        );

        private MediumBoat m_NavyBoat;

        #region Pirate Can Say Random Phrases From A [.txt] File

        public bool active;

        public static string path = "Data/Speech/PirateHunter.txt";

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
        public ActionAI_PirateHunter() : base(AIType.AI_Archer, FightMode.Closest, 15, 1, 0.2, 1.0)
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

            Title = ("[Naval Captain of " + m_RandomCoastalTown + "]");

            #region Pirate Can Say Random Phrases From A [.txt] File

            SpeechHue = Utility.RandomDyedHue();

            #endregion Pirate Can Say Random Phrases From A [.txt] File

            AddItem(new ThighBoots());
            AddItem(new Helmet());
            AddItem(new LeatherGloves());

            AddItem(new LongPants(Utility.RandomNeutralHue()));
            AddItem(new FancyShirt(Utility.RandomBlueHue()));

            AddItem(new BodySash(Utility.RandomYellowHue()));

            Item hair = new Item(Utility.RandomList(0x203B, 0x2049, 0x2048, 0x204A));
            hair.Hue = Utility.RandomNondyedHue();
            hair.Layer = Layer.Hair;
            hair.Movable = false;
            AddItem(hair);

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
            Karma = 5000;
            VirtualArmor = 5;

            switch (Utility.Random(5))
            {
                case 0: AddItem(new Bow()); break;
                case 1: AddItem(new CompositeBow()); break;
                case 2: AddItem(new Crossbow()); break;
                case 3: AddItem(new RepeatingCrossbow()); break;
                case 4: AddItem(new HeavyCrossbow()); break;
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

                m_NavyBoat = new MediumBoat();
                m_NavyBoat.Facing = m_Direction;
                Point3D loc = this.Location;
                m_NavyBoat.ShipName = "Navy Boat";

                m_NavyBoat.Owner = this;

                Point3D loccrew = this.Location;

                loc = new Point3D(this.X, this.Y - 1, this.Z);
                loccrew = new Point3D(this.X, this.Y - 1, this.Z + 1);

                m_NavyBoat.MoveToWorld(loc, map);
                boatspawn = true;

                for (int i = 0; i < 5; ++i)
                {
                    ActionAI_Privateer m_Crew = new ActionAI_Privateer();
                    m_Crew.MoveToWorld(loccrew, map);
                }
            }
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.SeafarersAndPirates; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich, 2);
        }

        public override bool IsScaredOfScaryThings { get { return false; } }
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

            if (m_NavyBoat == null)
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

            if (m_NavyBoat.Moving != Direction.South)
            {
                if (m_NavyBoat.Anchored == true)
                    m_NavyBoat.Anchored = false;

                m_NavyBoat.StartMove(Direction.North, false);
            }

            foreach (Item enemy in this.GetItemsInRange(50)) //200
            {
                if (enemy is BaseBoat && enemy != m_NavyBoat) // && !(enemy is PirateShip_Boat))
                {
                    Mobile owner = null;

                    if (((BaseBoat)enemy).Owner != null && ((BaseBoat)enemy).ShipName != "Navy Boat" && ((BaseBoat)enemy).Owner.Karma <= 0)
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

                                    m_NavyBoat.Anchored = true;

                                    if (plank.Locked)
                                    {
                                        this.MovingParticles(plank, 0x1C19, 1, 0, false, true, 0, 0, 9502, 6014, 0x11D, EffectLayer.Waist, 0);
                                        Effects.PlaySound(loc, m_enemyboat.Map, 0x307);
                                        Effects.SendLocationEffect(plank.Location, m_enemyboat.Map, 0x36BD, 20, 10);
                                        plank.Locked = false;
                                        plank.Open();
                                    }


                                    if (!m_enemyboat.Owner.Alive && !m_NavyBoat.Deleted)
                                    {
                                        this.Location = new Point3D(m_NavyBoat.X + 2, m_NavyBoat.Y + 2, m_NavyBoat.Z + 3);

                                        m_NavyBoat.Anchored = false;

                                        if (!m_NavyBoat.Move(Direction.North, 1, 0x4, false))
                                        {
                                            if (m_NavyBoat.Anchored == true)
                                                m_NavyBoat.Anchored = false;

                                            m_NavyBoat.StartMove(Direction.South, true);

                                            Timer.DelayCall(TimeSpan.FromSeconds(0.5),
                                                delegate
                                                {
                                                    m_NavyBoat.StopMove(true);

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

                                                    m_NavyBoat.SetFacing(m_Direction);

                                                    if (m_NavyBoat.Moving != Direction.South)
                                                    {
                                                        if (m_NavyBoat.Anchored == true)
                                                            m_NavyBoat.Anchored = false;

                                                        m_NavyBoat.StartMove(Direction.North, false);
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

                                                m_NavyBoat.Anchored = true;

                                                if (m_enemyboat != null)
                                                {
                                                    if (this.InRange(m_NavyBoat.GetWorldLocation(), 2) && m_enemyboat.Owner.Alive)
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

                            if (m_NavyBoat != null &&
                                (m_NavyBoat.Location.X >= (m_enemyboat.X + range) & m_NavyBoat.Location.Y >= (m_enemyboat.Y + range))
                                || (m_NavyBoat.Location.X <= (m_enemyboat.X - range) & m_NavyBoat.Location.Y <= (m_enemyboat.Y - range)))
                            {
                                m_enemyboat.Anchored = true;

                                if (m_NavyBoat != null && (m_enemyboat.Facing == Direction.North) && m_NavyBoat.Facing != Direction.North)
                                {
                                    m_NavyBoat.Facing = Direction.North;
                                }
                                else if (m_NavyBoat != null && (m_enemyboat.Facing == Direction.South) && m_NavyBoat.Facing != Direction.South)
                                {
                                    m_NavyBoat.Facing = Direction.South;
                                }
                                else if (m_NavyBoat != null && (m_enemyboat.Facing == Direction.East || m_enemyboat.Facing == Direction.Right /* NorthEast */ || m_enemyboat.Facing == Direction.Down /* SouthEast */) && m_NavyBoat.Facing != Direction.East)
                                {
                                    m_NavyBoat.Facing = Direction.East;
                                }
                                else if (m_NavyBoat != null && (m_enemyboat.Facing == Direction.West || m_enemyboat.Facing == Direction.Left /* SouthWest */ || m_enemyboat.Facing == Direction.Up) && m_NavyBoat.Facing != Direction.West)
                                {
                                    m_NavyBoat.Facing = Direction.West;
                                }

                            }
                            break;
                        }
                        else
                        {


                            if (m_NavyBoat != null && !m_NavyBoat.Deleted)
                            {
                                m_NavyBoat.Anchored = false;

                                Point3D oldPoint = m_NavyBoat.Location;

                                if (!m_NavyBoat.Move(Direction.North, 1, 0x4, false))
                                {
                                    if (m_NavyBoat.Anchored == true)
                                        m_NavyBoat.Anchored = false;

                                    m_NavyBoat.StartMove(Direction.South, true);

                                    Timer.DelayCall(TimeSpan.FromSeconds(0.5),
                                        delegate
                                        {
                                            m_NavyBoat.StopMove(true);

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

                                            m_NavyBoat.SetFacing(m_Direction);

                                            if (m_NavyBoat.Moving != Direction.South)
                                            {
                                                if (m_NavyBoat.Anchored == true)
                                                    m_NavyBoat.Anchored = false;

                                                m_NavyBoat.StartMove(Direction.North, false);
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

                if (m_NavyBoat != null && !m_NavyBoat.Deleted)
                {
                    m_NavyBoat.Anchored = false;

                    Point3D oldPoint = m_NavyBoat.Location;

                    if (!m_NavyBoat.Move(Direction.North, 1, 0x4, false))
                    {
                        if (m_NavyBoat.Anchored == true)
                            m_NavyBoat.Anchored = false;

                        m_NavyBoat.StartMove(Direction.South, true);

                        Timer.DelayCall(TimeSpan.FromSeconds(0.5),
                            delegate
                            {
                                m_NavyBoat.StopMove(true);

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

                                m_NavyBoat.SetFacing(m_Direction);

                                if (m_NavyBoat.Moving != Direction.South)
                                {
                                    if (m_NavyBoat.Anchored == true)
                                        m_NavyBoat.Anchored = false;

                                    m_NavyBoat.StartMove(Direction.North, false);
                                }

                            });
                    }
                }

                return;


            }

            if (m_NavyBoat != null && m_enemyboat != null)
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

                        m_NavyBoat.Anchored = true;

                        if (plank.Locked)
                        {
                            this.MovingParticles(plank, 0x1C19, 1, 0, false, true, 0, 0, 9502, 6014, 0x11D, EffectLayer.Waist, 0);
                            Effects.PlaySound(loc, m_enemyboat.Map, 0x307);
                            Effects.SendLocationEffect(plank.Location, m_enemyboat.Map, 0x36BD, 20, 10);
                            plank.Locked = false;
                            plank.Open();
                        }

                        if (!m_enemyboat.Owner.Alive && !m_NavyBoat.Deleted)
                        {
                            this.Location = new Point3D(m_NavyBoat.X + 2, m_NavyBoat.Y + 2, m_NavyBoat.Z + 3);

                            m_NavyBoat.Anchored = false;
                            m_enemyboat = null;

                            if (m_NavyBoat.Moving != Direction.South)
                            {
                                if (m_NavyBoat.Anchored == true)
                                    m_NavyBoat.Anchored = false;

                                m_NavyBoat.StartMove(Direction.North, false);

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
                                    m_NavyBoat.StopMove(true);
                                    m_NavyBoat.Anchored = true;

                                    if (m_enemyboat != null)
                                    {
                                        if (this.InRange(m_NavyBoat.GetWorldLocation(), 2) && m_enemyboat.Owner.Alive)
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
                    if (m_NavyBoat != null &&
                        (m_NavyBoat.Location.X >= (m_enemyboat.X + range) & m_NavyBoat.Location.Y >= (m_enemyboat.Y + range))
                        || (m_NavyBoat.Location.X <= (m_enemyboat.X - range) & m_NavyBoat.Location.Y <= (m_enemyboat.Y - range)))
                    {

                        m_enemyboat.Anchored = true;

                        if (m_NavyBoat != null && (m_enemyboat.Facing == Direction.North) && m_NavyBoat.Facing != Direction.North)
                        {
                            m_NavyBoat.Facing = Direction.North;
                        }
                        else if (m_NavyBoat != null && (m_enemyboat.Facing == Direction.South) && m_NavyBoat.Facing != Direction.South)
                        {
                            m_NavyBoat.Facing = Direction.South;
                        }
                        else if (m_NavyBoat != null && (m_enemyboat.Facing == Direction.East || m_enemyboat.Facing == Direction.Right /* NorthEast */ || m_enemyboat.Facing == Direction.Down /* SouthEast */) && m_NavyBoat.Facing != Direction.East)
                        {
                            m_NavyBoat.Facing = Direction.East;
                        }
                        else if (m_NavyBoat != null && (m_enemyboat.Facing == Direction.West || m_enemyboat.Facing == Direction.Left /* SouthWest */ || m_enemyboat.Facing == Direction.Up) && m_NavyBoat.Facing != Direction.West)
                        {
                            m_NavyBoat.Facing = Direction.West;
                        }
                    }

                    if (m_NavyBoat != null && (enemydirection == Direction.North) && m_NavyBoat.Facing != Direction.North)
                    {
                        m_NavyBoat.Facing = Direction.North;
                    }
                    else if (m_NavyBoat != null && (enemydirection == Direction.South) && m_NavyBoat.Facing != Direction.South)
                    {
                        m_NavyBoat.Facing = Direction.South;
                    }
                    else if (m_NavyBoat != null && (enemydirection == Direction.East || enemydirection == Direction.Right /* NorthEast */ || enemydirection == Direction.Down /* SouthEast */) && m_NavyBoat.Facing != Direction.East)
                    {
                        m_NavyBoat.Facing = Direction.East;
                    }
                    else if (m_NavyBoat != null && (enemydirection == Direction.West || enemydirection == Direction.Left /* SouthWest */ || enemydirection == Direction.Up) && m_NavyBoat.Facing != Direction.West)
                    {
                        m_NavyBoat.Facing = Direction.West;
                    }

                    if (m_NavyBoat.Moving != Direction.South)
                    {
                        if (m_NavyBoat.Anchored == true)
                            m_NavyBoat.Anchored = false;

                        m_NavyBoat.StartMove(Direction.North, false);
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
                this.LastKiller.SendGump(new BoatDecisionGump(this.LastKiller, m_NavyBoat));
            }
            else
            {
                new SinkTimer(m_NavyBoat, m_NavyBoat.Z).Start();
            }

            return true;
        }





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
                                if (m is ActionAI_Privateer || m is PlayerMobile)
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

        public ActionAI_PirateHunter(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((Item)m_NavyBoat);
            writer.Write((bool)boatspawn);
            writer.Write((int)0);

            #region Pirate Can Say Random Phrases From A [.txt] File

            writer.Write((bool)active);

            #endregion Pirate Can Say Random Phrases From A [.txt] File
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            m_NavyBoat = reader.ReadItem() as MediumBoat;
            boatspawn = reader.ReadBool();
            int version = reader.ReadInt();

            #region Pirate Can Say Random Phrases From A [.txt] File

            active = reader.ReadBool();

            #endregion Pirate Can Say Random Phrases From A [.txt] File

        }
    }

    [CorpseName("A Privateer Corpse")]
    public class ActionAI_Privateer : BaseCreature
    {
        [Constructable]
        public ActionAI_Privateer() : base(AIType.AI_Archer, FightMode.Closest, 15, 1, 0.2, 0.4)
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

            Title = "[Privateer]";

            AddItem(new ThighBoots());

            Item hair = new Item(Utility.RandomList(0x203B, 0x2049, 0x2048, 0x204A));
            hair.Hue = Utility.RandomNondyedHue();
            hair.Layer = Layer.Hair;
            hair.Movable = false;
            AddItem(hair);

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
            Karma = 2000;
            VirtualArmor = 2;


            switch (Utility.Random(2))
            {
                case 0: AddItem(new LongPants(Utility.RandomNeutralHue())); break;
                case 1: AddItem(new ShortPants(Utility.RandomNeutralHue())); break;
            }

            switch (Utility.Random(2))
            {
                case 0: AddItem(new Shirt(Utility.RandomBlueHue())); break;
                case 1: AddItem(new Doublet(Utility.RandomBlueHue())); break;
            }


            switch (Utility.Random(2))
            {
                case 0: AddItem(new Bonnet(Utility.RandomBlueHue())); break;
                case 1: AddItem(new Helmet()); break;
            }

            switch (Utility.Random(4))
            {
                case 0: AddItem(new Crossbow()); break;
                case 1: AddItem(new RepeatingCrossbow()); break;

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
            get { return OppositionGroup.SeafarersAndPirates; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich);
        }

        public override bool IsScaredOfScaryThings { get { return false; } }
        public override bool CanRummageCorpses { get { return true; } }
        public override bool PlayerRangeSensitive { get { return false; } }

        public ActionAI_Privateer(Serial serial) : base(serial)
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