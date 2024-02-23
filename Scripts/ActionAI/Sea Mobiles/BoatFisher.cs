using Server;
using Server.Engines.Harvest;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;

using Server.Spells;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Mobiles
{
    public class ActionAI_DeepSeaFishermen : BaseCreature
    {
        private MediumBoat m_DeepSeaBoat;

        private int m_FishingCount;

        private SpecialFishingNet m_Net;

        [CommandProperty(AccessLevel.GameMaster)]
        public int FishingCount
        {
            get
            {
                return m_FishingCount;
            }
            set
            {
                m_FishingCount = value;
            }
        }

        public override bool InitialInnocent { get { return true; } }

        public override HarvestDefinition harvestDefinition { get { return Fishing.System.Definition; } }
        public override HarvestSystem harvestSystem { get { return Fishing.System; } }

        [Constructable]
        public ActionAI_DeepSeaFishermen() : base(AIType.AI_Archer, FightMode.Closest, 15, 1, 0.2, 2.0)
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

            Title = "[Deep Sea Fisherman]";

            AddItem(new FloppyHat(Utility.RandomOrangeHue()));
            AddItem(new FishingPole());
            AddItem(new ShortPants(Utility.RandomOrangeHue()));
            AddItem(new Shirt(Utility.RandomOrangeHue()));

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

            SetSkill(SkillName.Fishing, 200, 300);

            CantWalk = false;

            Fame = 1000;
            Karma = 1000;
            VirtualArmor = 5;





            /* switch (Utility.Random(5))
            {
                case 0: AddItem(new Bow()); break;
                case 1: AddItem(new CompositeBow()); break;
                case 2: AddItem(new Crossbow()); break;
                case 3: AddItem(new RepeatingCrossbow()); break;
                case 4: AddItem(new HeavyCrossbow()); break;
            } */


            Timer.DelayCall(Create_BoatAndCrew);
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.FishersAndSeaCreatures; }
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

                m_DeepSeaBoat = new MediumBoat();
                m_DeepSeaBoat.Facing = m_Direction;
                Point3D loc = this.Location;
                //m_DeepSeaBoat.ShipName = "Pirate Boat";

                m_DeepSeaBoat.Owner = this;

                Point3D loccrew = this.Location;

                loc = new Point3D(this.X, this.Y - 1, this.Z);
                loccrew = new Point3D(this.X, this.Y - 1, this.Z + 1);

                m_DeepSeaBoat.MoveToWorld(loc, map);
                this.Z += 1;
                boatspawn = true;

                /* for(int i = 0; i < 5; ++i)
                {
                    PirateCrew m_PirateCrew = new PirateCrew();
                    m_PirateCrew.MoveToWorld(loccrew, map); 
                } */


            }

            Timer.DelayCall(MoveBoat);
        }

        private void MoveBoat()
        {
            if (CheckForEnemies())
                return;

            //Warmode = false;
            EquipWeapon(false);

            if (m_DeepSeaBoat.Moving != Direction.South && m_FishingCount < 4)
            {
                if (m_DeepSeaBoat.Anchored == true)
                    m_DeepSeaBoat.Anchored = false;

                m_DeepSeaBoat.StartMove(Direction.North, false);
            }

            Timer.DelayCall(TimeSpan.FromSeconds(5.0), StopAndFish);
        }

        private void PlaceFishingnet()
        {
            if (m_Net == null || m_Net.Deleted)
                m_Net = new SpecialFishingNet();

            m_Net.MoveToWorld(new Point3D(m_DeepSeaBoat.X, m_DeepSeaBoat.Y, m_DeepSeaBoat.Z - 10), m_DeepSeaBoat.Map);
        }



        private void StopAndFish()
        {
            m_DeepSeaBoat.StopMove(true);
            m_DeepSeaBoat.Anchored = true;



            CurrentSpeed = 2.0;

            Point3D loc = new Point3D(this.X, this.Y, this.Z);

            switch (Direction)
            {
                case Direction.North:
                    loc = new Point3D(this.X, this.Y - 2, this.Z);
                    break;
                case Direction.East:
                    loc = new Point3D(this.X + 2, this.Y, this.Z);
                    break;
                case Direction.South:
                    loc = new Point3D(this.X, this.Y + 2, this.Z);
                    break;
                case Direction.West:
                    loc = new Point3D(this.X - 2, this.Y, this.Z);
                    break;
                case Direction.Right: // NorthEast = Right
                    loc = new Point3D(this.X - 2, this.Y + 2, this.Z);
                    break;
                case Direction.Down: // SouthEast = Down
                    loc = new Point3D(this.X - 2, this.Y - 2, this.Z);
                    break;
                case Direction.Left: // SouthWest = Left
                    loc = new Point3D(this.X + 2, this.Y - 2, this.Z);
                    break;
                case Direction.Up: // NorthWest = UP
                    loc = new Point3D(this.X + 2, this.Y + 2, this.Z);
                    break;
            }

            Point3D fishingPoint = m_DeepSeaBoat.Location;

            int random = Utility.RandomMinMax(-2, 2);

            switch (m_DeepSeaBoat.Facing)
            {
                case Direction.North:
                    this.Direction = Direction.East;
                    fishingPoint = new Point3D(m_DeepSeaBoat.X + 5, m_DeepSeaBoat.Y + random, m_DeepSeaBoat.Z);
                    break;
                case Direction.South:
                    this.Direction = Direction.West;
                    fishingPoint = new Point3D(m_DeepSeaBoat.X - 5, m_DeepSeaBoat.Y + random, m_DeepSeaBoat.Z);
                    break;
                case Direction.East:
                    this.Direction = Direction.South;
                    fishingPoint = new Point3D(m_DeepSeaBoat.X + random, m_DeepSeaBoat.Y + 5, m_DeepSeaBoat.Z);
                    break;
                case Direction.West:
                    this.Direction = Direction.North;
                    fishingPoint = new Point3D(m_DeepSeaBoat.X + random, m_DeepSeaBoat.Y - 5, m_DeepSeaBoat.Z);
                    break;
            }

            if (0.20 >= Utility.RandomDouble() && !CheckForEnemies())
            {
                FishingNet(fishingPoint);
                m_DeepSeaBoat.StopMove(true);
                m_DeepSeaBoat.Anchored = true;
            }
            else /* if( !CheckForEnemies() && FishingCount <= 5 ) */
            {
                PlaceFishingnet();

                Animate(11, 5, 1, true, false, 0); //  Animate( int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay )    

                DoHarvest();

                Timer.DelayCall(TimeSpan.FromSeconds(0.7),
                    delegate
                    {
                        Effects.SendLocationEffect(fishingPoint, this.Map, 0x352D, 16, 4); //water splash //  SendLocationEffect( IPoint3D p, Map map, int itemID, int duration, int speed )
                        Effects.PlaySound(fishingPoint, this.Map, 0x364);
                    });

                FishingCount += 1;

                Container pack = Backpack;

                if (pack == null)
                    return;
                else if (pack.TotalWeight >= 300)
                    EmptyPack();

                if (FishingCount <= 4)
                {
                    Timer.DelayCall(TimeSpan.FromSeconds(2.0), StopAndFish);
                    //Emote(m_FishingCount);
                }
                else
                {


                    if (m_Net != null && !m_Net.Deleted)
                        m_Net.Delete();

                    Timer.DelayCall(TimeSpan.FromSeconds(4.0), MoveBoat);

                    FishingCount = 0;

                }
            }

        }

        private void FishingNet(Point3D p)
        {
			var newZ = p.Z;

            if (WaterUtility.FullDeepValidation(Map, p, ref newZ, out var fresh))
            {
				p.Z = newZ;

                //this.Emote("Deep Sea Valid");

                if (m_Net == null || m_Net.Deleted)
                    m_Net = new SpecialFishingNet();

                m_Net.MoveToWorld(p, this.Map);

                SpellHelper.Turn(this, p);
                this.Animate(12, 5, 1, true, false, 0);

                Effects.SendLocationEffect(p, this.Map, 0x352D, 16, 4);
                Effects.PlaySound(p, this.Map, 0x364);

                Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.25), 14, DoFishnetEffect, p, 0, this, m_Net );
            }
            else
            {
                //this.Emote("Deep Sea Invalid");
            }
        }

        private void DoFishnetEffect(Point3D point3D, int indexID, Mobile m, SpecialFishingNet fishnet)
        {
            if (Deleted)
                return;

            //object[] states = (object[])state;

			Point3D p = point3D; // (Point3D)states[0];
			int index = indexID; // (int)states[1];
			Mobile from = m; // (Mobile)states[2];
			SpecialFishingNet net = fishnet; // (SpecialFishingNet)states[3];

			// states[1] = ++index;

            if (index == 1)
            {
                Effects.SendLocationEffect(p, Map, 0x352D, 16, 4);
                Effects.PlaySound(p, Map, 0x364);
            }
            else if (index <= 7 || index == 14)
            {
                for (int i = 0; i < 3; ++i)
                {
                    int x, y;

                    switch (Utility.Random(8))
                    {
                        default:
                        case 0: x = -1; y = -1; break;
                        case 1: x = -1; y = 0; break;
                        case 2: x = -1; y = +1; break;
                        case 3: x = 0; y = -1; break;
                        case 4: x = 0; y = +1; break;
                        case 5: x = +1; y = -1; break;
                        case 6: x = +1; y = 0; break;
                        case 7: x = +1; y = +1; break;
                    }

                    Effects.SendLocationEffect(new Point3D(p.X + x, p.Y + y, p.Z), Map, 0x352D, 16, 4);
                }

                if (Utility.RandomBool())
                    Effects.PlaySound(p, Map, 0x364);

                if (index == 14)
                {
                    FinishEffect(p, Map, from, m_Net);



                    //net.Delete();
                }
                else
                {
                    if (m_Net != null && !m_Net.Deleted)
                        m_Net.Z -= 1;
                }
            }


        }

        private void FinishEffect(Point3D p, Map map, Mobile from, SpecialFishingNet net)
        {
            int count = GetSpawnCount();

            for (int i = 0; map != null && i < count; ++i)
            {
                BaseCreature spawn;

                switch (Utility.Random(4))
                {
                    default:
                    case 0: spawn = new SeaSerpent(); break;
                    case 1: spawn = new DeepSeaSerpent(); break;
                    case 2: spawn = new WaterElemental(); break;
                    case 3: spawn = new Kraken(); break;
                }

                Spawn(p, this.Map, spawn);

                //Warmode = true;
                EquipWeapon(true);

                spawn.Combatant = this;

                Timer.DelayCall(TimeSpan.FromSeconds(10.0),
                    delegate
                    {
                        if (net != null && !net.Deleted)
                            net.Delete();
                    });




            }
        }

        private void CheckAgain()
        {
            /*  Timer.DelayCall( TimeSpan.FromMinutes( 1.0 ), 
                    delegate {
                         */
            if (CheckForEnemies())
                CheckAgain();
            else
            {
                DelayStopAndFish();
                return;
            }
            //} );
        }

        private void DelayStopAndFish()
        {
            //Timer.DelayCall( TimeSpan.FromMinutes( 1.0 ), StopAndFish);
            StopAndFish();
            return;
        }

        /*
        int EnemyCount = 0;

            foreach ( Mobile mobile in this.GetMobilesInRange( 8 ) ) //200
            {
                if( IsEnemy(mobile) )
                {
                    EnemyCount++;
                    Warmode = true;
                    EquipWeapon(true);
                }
            }

        */

        protected virtual int GetSpawnCount()
        {
            int count = Utility.RandomMinMax(1, 3);

            /* if ( Hue != 0x8A0 )
				count += Utility.RandomMinMax( 1, 2 ); */

            return count;
        }

        protected void Spawn(Point3D p, Map map, BaseCreature spawn)
        {
            if (map == null)
            {
                spawn.Delete();
                return;
            }

            int x = p.X, y = p.Y;

            for (int j = 0; j < 20; ++j)
            {
                int tx = p.X - 2 + Utility.Random(5);
                int ty = p.Y - 2 + Utility.Random(5);

                LandTile t = map.Tiles.GetLandTile(tx, ty);

                if (t.Z == p.Z && ((t.ID >= 0xA8 && t.ID <= 0xAB) || (t.ID >= 0x136 && t.ID <= 0x137)) && !Spells.SpellHelper.CheckMulti(new Point3D(tx, ty, p.Z), map))
                {
                    x = tx;
                    y = ty;
                    break;
                }
            }

            spawn.MoveToWorld(new Point3D(x, y, p.Z), map);

            if (spawn is Kraken && 0.2 > Utility.RandomDouble())
                spawn.PackItem(new MessageInABottle(map == Map.Felucca ? Map.Felucca : Map.Trammel));

            if (m_Net != null && !m_Net.Deleted)
                m_Net.Delete();
        }

        private void EmptyPack()
        {
            if (m_DeepSeaBoat == null && m_DeepSeaBoat.Deleted)
                return;

            SelfDeleting_MediumCrate container = null;
            List<Item> items = Backpack.Items;
            Point3D p = new Point3D(X, Y, Z);

            if (items.Count > 0)
            {
                foreach (Item item in Map.GetItemsInRange(p, 3))
                {
                    if (item is SelfDeleting_MediumCrate)
                        container = (SelfDeleting_MediumCrate)item;
                }

                if (container == null || container.Weight > 300)
                {
                    container = new SelfDeleting_MediumCrate();
                    Point3D newPoint = new Point3D(m_DeepSeaBoat.X, m_DeepSeaBoat.Y, m_DeepSeaBoat.Z);
                    container.MoveToWorld(newPoint, Map);
                }

                int randX = 0;
                int randY = 0;

                for (int i = 0; i < items.Count; i++)
                {
                    //randomize placement of items in container so they're not all stacked on stop of eachother
                    randX = Utility.RandomMinMax(0, 100);
                    randY = Utility.RandomMinMax(0, 100);

                    //items in Containers do not have a Z coordinate
                    if (container != null || !container.Deleted)
                        container.OnDragDropInto(this, items[i], new Point3D(randX, randY, 0));
                }
            }
        }

        public void DoHarvest()
        {
            if (Map == Map.Internal)
            {
                return;
            }

            if (Backpack == null)
                AddItem(new Backpack());

            Map map = Map;
            Point3D loc = Location;
            int x = 0, y = 0;
            loc.X += x;
            loc.Y += y;

            HarvestBank bank = harvestDefinition.GetBank(map, loc.X, loc.Y);

            bool available = (bank != null && bank.Current >= harvestDefinition.ConsumedPerHarvest);

            if (!available)
            {
                //Emote( String.Format("There is no wood here to harvest") );
                return;
            }

            if (bank == null || bank.Current < harvestDefinition.ConsumedPerHarvest)
                return;

            HarvestVein vein = bank.Vein;

            if (vein == null)
                return;

            HarvestResource primary = vein.PrimaryResource;
            HarvestResource fallback = harvestDefinition.Resources[0];

            HarvestResource resource = harvestSystem.MutateResource(this, null, harvestDefinition, map, loc, vein, primary, fallback);

            double skillBase = Skills[harvestDefinition.Skill].Base;

            Type type = null;

            if (skillBase >= resource.ReqSkill && CheckSkill(harvestDefinition.Skill, resource.MinSkill, resource.MaxSkill))
            {
                type = harvestSystem.GetResourceType(this, null, harvestDefinition, map, loc, resource);

                if (type != null)
                    type = harvestSystem.MutateType(type, this, null, harvestDefinition, map, loc, resource);

                if (type != null)
                {
                    Item itemHarvested = harvestSystem.Construct(type, this);

                    if (itemHarvested == null)
                    {
                        type = null;
                    }
                    else
                    {
                        if (itemHarvested.Stackable)
                        {
                            int amount = harvestDefinition.ConsumedPerHarvest;
                            int feluccaAmount = harvestDefinition.ConsumedPerFeluccaHarvest;

                            bool inFelucca = (map == Map.Felucca);

                            if (inFelucca)
                                itemHarvested.Amount = feluccaAmount;
                            else
                                itemHarvested.Amount = amount;
                        }

                        bank.Consume(itemHarvested.Amount, this);

                        Container pack = Backpack;

                        if (pack == null)
                        {
                            return;
                        }

                        pack.TryDropItem(this, itemHarvested, false);

                        // Harvest bark fragment, amber, etc
                        BonusHarvestResource bonus = harvestDefinition.GetBonusResource();

                        if (bonus != null && bonus.Type != null && skillBase >= bonus.ReqSkill)
                        {
                            Item bonusItem = harvestSystem.Construct(bonus.Type, this);

                            pack.TryDropItem(this, bonusItem, false);
                        }
                    }
                }
            }
        }

        private void EquipWeapon(bool equipWeapon)
        {
            if (Backpack == null)
                AddItem(new Backpack());

            Item itemEquipped = this.FindItemOnLayer(Layer.TwoHanded);

            if (itemEquipped != null)
                RemoveItem(itemEquipped);

            if (equipWeapon)
            {
                Item item = Backpack.FindItemByType(typeof(Bow));

                if (item == null)
                {
                    Bow bow = new Bow();
                    Backpack.DropItem(bow);
                    AddItem(bow);
                }
                else
                {
                    AddItem(item);
                }
            }
            else
            {
                Item item = Backpack.FindItemByType(typeof(FishingPole));

                if (item == null)
                {
                    FishingPole pole = new FishingPole();
                    Backpack.DropItem(pole);
                    AddItem(pole);
                }
                else
                {
                    AddItem(item);
                }
            }
        }

        private bool CheckForEnemies()
        {
            foreach (Mobile mobile in this.GetMobilesInRange(10)) //200
            {
                if (mobile is SeaSerpent || mobile is WaterElemental || mobile is DeepSeaSerpent || mobile is Kraken)
                    return true;
            }

            foreach (Item item in this.GetItemsInRange(10)) //200
            {
                if (item is SpecialFishingNet)
                    return true;

                if (item is Corpse)
                {
                    CheckForCorpse(((Corpse)item));

                    if (m_Net != null && !m_Net.Deleted)
                        m_Net.Delete();

                    return true;
                }
            }

            m_FishingCount = 0;

            return false;

        }

        private void CheckForCorpse(Corpse c)
        {
            if (c.Killer == this)
            {
                c.Carve(this, c);

                List<Item> items = new List<Item>(c.Items.Count);

                foreach (Item item in c.Items)
                {
                    if (item != null)
                        items.Add(item);
                }



                foreach (Item item in items)
                {
                    Container pack = Backpack;

                    if (pack == null)
                    {
                        return;
                    }

                    pack.TryDropItem(this, item, false);

                    if (pack.TotalWeight >= 300)
                        EmptyPack();
                }

                Timer.DelayCall(TimeSpan.FromSeconds(3.0), delegate
                {
                    c.Delete();
                });



            }


        }

        /*       private bool lootContainer(PlayerMobile player, Container container)
              {
                  List<Item> items = new List<Item>(container.Items.Count);

                  foreach (Item item in container.Items)
                  {
                      if (item != null && isItemLootable(item))
                          items.Add(item);
                  }

                  foreach (Item item in items)
                  {
                      if (!TryDropItem(player, item, false))
                          return false;
                  }

                  return true;
              } */

        public override void OnThink()
        {
            if (m_DeepSeaBoat == null)
                return;

            if (!this.InRange(m_DeepSeaBoat.GetWorldLocation(), 15))
            {
                this.Location = new Point3D(m_DeepSeaBoat.X + 1, m_DeepSeaBoat.Y + 1, m_DeepSeaBoat.Z + 1);
            }

            if (m_FishingCount == 0 & !CheckForEnemies() & !m_DeepSeaBoat.IsMoving)
                Timer.DelayCall(TimeSpan.FromSeconds(5.0), MoveBoat);

            if (m_DeepSeaBoat.Anchored == true)
            {
                if (CheckForEnemies())
                {
                    return;
                }
            }

            if (CheckForEnemies())
            {
                m_DeepSeaBoat.StopMove(true);
                m_DeepSeaBoat.Anchored = true;
                EquipWeapon(true);
            }


            if (!CheckForEnemies() && !m_DeepSeaBoat.Move(Direction.North, 1, 0x4, false))
            {
                m_DeepSeaBoat.Anchored = false;
                m_DeepSeaBoat.StartMove(Direction.South, true);

                Timer.DelayCall(TimeSpan.FromSeconds(0.5),
                    delegate
                    {
                        m_DeepSeaBoat.StopMove(true);

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

                        m_DeepSeaBoat.SetFacing(m_Direction);

                        if (m_DeepSeaBoat.Moving != Direction.South)
                        {
                            if (m_DeepSeaBoat.Anchored == true)
                                m_DeepSeaBoat.Anchored = false;

                            m_DeepSeaBoat.StartMove(Direction.North, false);
                        }

                    });
            }



            base.OnThink();
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich, 2);
        }

        public override bool IsScaredOfScaryThings { get { return false; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override bool AutoDispel { get { return false; } }
        public override bool CanRummageCorpses { get { return false; } }

        public override bool PlayerRangeSensitive { get { return false; } }

        private bool boatspawn;



        /* public void SpawnPirate( Mobile target )
		{
			Map map = target.Map;

			if ( map == null )
				return;

			int pirates = 0;

			foreach ( Mobile m in this.GetMobilesInRange( 10 ) )
			{
                if (m is PirateCrew)
                    ++pirates;
			}

			if ( pirates < 10 && Utility.RandomDouble() <= 0.25)
			{
                BaseCreature PirateCrew = new PirateCrew();

				Point3D loc = target.Location;
				bool validLocation = false;

				for ( int j = 0; !validLocation && j < 10; ++j )
				{
					int x = target.X + Utility.Random( 3 ) - 1;
					int y = target.Y + Utility.Random( 3 ) - 1;
					int z = map.GetAverageZ( x, y );

					if ( validLocation = map.CanFit( x, y, this.Z, 16, false, false ) )
						loc = new Point3D( x, y, Z );
					else if ( validLocation = map.CanFit( x, y, z, 16, false, false ) )
						loc = new Point3D( x, y, z );
				}

                PirateCrew.MoveToWorld(loc, map);

                PirateCrew.Combatant = target;
			}
		} */

        public override bool OnBeforeDeath()
        {
            if (this.LastKiller != null && this.LastKiller is PlayerMobile)
            {
                this.LastKiller.SendGump(new BoatDecisionGump(this.LastKiller, m_DeepSeaBoat));
            }
            else
            {
                new SinkTimer(m_DeepSeaBoat, m_DeepSeaBoat.Z).Start();
            }

            return true;
        }

        public ActionAI_DeepSeaFishermen(Serial serial) : base(serial)
        {
            if (m_DeepSeaBoat != null && !m_DeepSeaBoat.Deleted)
                Timer.DelayCall(MoveBoat);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);

            writer.Write((Item)m_DeepSeaBoat);
            writer.Write((bool)boatspawn);
            writer.Write((int)m_FishingCount);
            writer.Write(m_Net);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_DeepSeaBoat = reader.ReadItem() as MediumBoat;
            boatspawn = reader.ReadBool();
            m_FishingCount = reader.ReadInt();
            m_Net = reader.ReadItem() as SpecialFishingNet;

            /*  if( m_DeepSeaBoat != null && !m_DeepSeaBoat.Deleted )
                 Timer.DelayCall( TimeSpan.FromSeconds( 2.0 ), MoveBoat ); */
        }
    }

    public class ActionAI_DockedFishermen : BaseCreature
    {
        private MediumBoat m_DeepSeaBoat;

        private int m_FishingCount;

        private SpecialFishingNet m_Net;

        [CommandProperty(AccessLevel.GameMaster)]
        public int FishingCount
        {
            get
            {
                return m_FishingCount;
            }
            set
            {
                m_FishingCount = value;
            }
        }

        public override bool InitialInnocent { get { return true; } }

        public override HarvestDefinition harvestDefinition { get { return Fishing.System.Definition; } }
        public override HarvestSystem harvestSystem { get { return Fishing.System; } }

        [Constructable]
        public ActionAI_DockedFishermen() : base(AIType.AI_Archer, FightMode.Closest, 15, 1, 0.2, 2.0)
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

            Title = "[Deep Sea Fisherman]";

            AddItem(new FloppyHat(Utility.RandomOrangeHue()));
            AddItem(new FishingPole());
            AddItem(new ShortPants(Utility.RandomOrangeHue()));
            AddItem(new Shirt(Utility.RandomOrangeHue()));

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

            SetSkill(SkillName.Fishing, 200, 300);

            CantWalk = false;

            Fame = 1000;
            Karma = 1000;
            VirtualArmor = 5;

            Timer.DelayCall(Create_BoatAndCrew);
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.FishersAndSeaCreatures; }
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

                m_DeepSeaBoat = new MediumBoat();
                m_DeepSeaBoat.Facing = m_Direction;
                Point3D loc = this.Location;
                //m_DeepSeaBoat.ShipName = "Pirate Boat";

                m_DeepSeaBoat.Owner = this;

                Point3D loccrew = this.Location;

                loc = new Point3D(this.X, this.Y - 1, this.Z);
                loccrew = new Point3D(this.X, this.Y - 1, this.Z + 1);

                m_DeepSeaBoat.MoveToWorld(loc, map);
                this.Z += 1;
                boatspawn = true;
            }

            Timer.DelayCall(TimeSpan.FromSeconds(5.0), StopAndFish);
        }

        private void StopAndFish()
        {
            CurrentSpeed = 2.0;

            Point3D loc = new Point3D(this.X, this.Y, this.Z);

            switch (Direction)
            {
                case Direction.North:
                    loc = new Point3D(this.X, this.Y - 2, this.Z);
                    break;
                case Direction.East:
                    loc = new Point3D(this.X + 2, this.Y, this.Z);
                    break;
                case Direction.South:
                    loc = new Point3D(this.X, this.Y + 2, this.Z);
                    break;
                case Direction.West:
                    loc = new Point3D(this.X - 2, this.Y, this.Z);
                    break;
                case Direction.Right: // NorthEast
                    loc = new Point3D(this.X - 2, this.Y + 2, this.Z);
                    break;
                case Direction.Down: // SouthEast
                    loc = new Point3D(this.X - 2, this.Y - 2, this.Z);
                    break;
                case Direction.Left: // SouthWest
                    loc = new Point3D(this.X + 2, this.Y - 2, this.Z);
                    break;
                case Direction.Up: // NorthWest
                    loc = new Point3D(this.X + 2, this.Y + 2, this.Z);
                    break;
            }

            Point3D fishingPoint = m_DeepSeaBoat.Location;

            int random = Utility.RandomMinMax(-2, 2);

            switch (m_DeepSeaBoat.Facing)
            {
                case Direction.North:
                    this.Direction = Direction.East;
                    fishingPoint = new Point3D(m_DeepSeaBoat.X + 5, m_DeepSeaBoat.Y + random, m_DeepSeaBoat.Z);
                    break;
                case Direction.South:
                    this.Direction = Direction.West;
                    fishingPoint = new Point3D(m_DeepSeaBoat.X - 5, m_DeepSeaBoat.Y + random, m_DeepSeaBoat.Z);
                    break;
                case Direction.East:
                    this.Direction = Direction.South;
                    fishingPoint = new Point3D(m_DeepSeaBoat.X + random, m_DeepSeaBoat.Y + 5, m_DeepSeaBoat.Z);
                    break;
                case Direction.West:
                    this.Direction = Direction.North;
                    fishingPoint = new Point3D(m_DeepSeaBoat.X + random, m_DeepSeaBoat.Y - 5, m_DeepSeaBoat.Z);
                    break;
            }

            Animate(11, 5, 1, true, false, 0); //  Animate( int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay )    

            Timer.DelayCall(TimeSpan.FromSeconds(0.7),
                delegate {
                    Effects.SendLocationEffect(fishingPoint, this.Map, 0x352D, 16, 4); //water splash //  SendLocationEffect( IPoint3D p, Map map, int itemID, int duration, int speed )
                    Effects.PlaySound(fishingPoint, this.Map, 0x364);
                });

            FishingCount += 1;

            DoHarvest();

            Container pack = Backpack;

            if (pack == null)
                return;
            else if (pack.TotalWeight >= 300)
                EmptyPack();

            if (FishingCount <= 4)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(2.0), StopAndFish);
                //Emote(m_FishingCount);
            }
            else
            {
                FishingCount = 0;
                Timer.DelayCall(TimeSpan.FromMinutes(5.0), StopAndFish);
            }
        }

        public void DoHarvest()
        {
            if (Map == Map.Internal)
            {
                return;
            }

            if (Backpack == null)
                AddItem(new Backpack());

            Map map = Map;
            Point3D loc = Location;
            int x = 0, y = 0;
            loc.X += x;
            loc.Y += y;

            HarvestBank bank = harvestDefinition.GetBank(map, loc.X, loc.Y);

            bool available = (bank != null && bank.Current >= harvestDefinition.ConsumedPerHarvest);

            if (!available)
            {
                //Emote( String.Format("There is no wood here to harvest") );
                return;
            }

            if (bank == null || bank.Current < harvestDefinition.ConsumedPerHarvest)
                return;

            HarvestVein vein = bank.Vein;

            if (vein == null)
                return;

            HarvestResource primary = vein.PrimaryResource;
            HarvestResource fallback = harvestDefinition.Resources[0];

            HarvestResource resource = harvestSystem.MutateResource(this, null, harvestDefinition, map, loc, vein, primary, fallback);

            double skillBase = Skills[harvestDefinition.Skill].Base;

            Type type = null;

            if (skillBase >= resource.ReqSkill && CheckSkill(harvestDefinition.Skill, resource.MinSkill, resource.MaxSkill))
            {
                type = harvestSystem.GetResourceType(this, null, harvestDefinition, map, loc, resource);

                if (type != null)
                    type = harvestSystem.MutateType(type, this, null, harvestDefinition, map, loc, resource);

                if (type != null)
                {
                    Item itemHarvested = harvestSystem.Construct(type, this);

                    if (itemHarvested == null)
                    {
                        type = null;
                    }
                    else
                    {
                        if (itemHarvested.Stackable)
                        {
                            int amount = harvestDefinition.ConsumedPerHarvest;
                            int feluccaAmount = harvestDefinition.ConsumedPerFeluccaHarvest;

                            bool inFelucca = (map == Map.Felucca);

                            if (inFelucca)
                                itemHarvested.Amount = feluccaAmount;
                            else
                                itemHarvested.Amount = amount;
                        }

                        bank.Consume(itemHarvested.Amount, this);

                        Container pack = Backpack;

                        if (pack == null)
                        {
                            return;
                        }

                        pack.TryDropItem(this, itemHarvested, false);

                        // Harvest bark fragment, amber, etc
                        BonusHarvestResource bonus = harvestDefinition.GetBonusResource();

                        if (bonus != null && bonus.Type != null && skillBase >= bonus.ReqSkill)
                        {
                            Item bonusItem = harvestSystem.Construct(bonus.Type, this);

                            pack.TryDropItem(this, bonusItem, false);
                        }
                    }
                }
            }
        }

        private void EmptyPack()
        {
            if (m_DeepSeaBoat == null && m_DeepSeaBoat.Deleted)
                return;

            SelfDeleting_MediumCrate container = null;
            List<Item> items = Backpack.Items;
            Point3D p = new Point3D(X, Y, Z);

            if (items.Count > 0)
            {
                foreach (Item item in Map.GetItemsInRange(p, 3))
                {
                    if (item is SelfDeleting_MediumCrate)
                        container = (SelfDeleting_MediumCrate)item;
                }

                if (container == null || container.Weight > 300)
                {
                    container = new SelfDeleting_MediumCrate();
                    Point3D newPoint = new Point3D(m_DeepSeaBoat.X, m_DeepSeaBoat.Y, m_DeepSeaBoat.Z);
                    container.MoveToWorld(newPoint, Map);
                }

                int randX = 0;
                int randY = 0;

                for (int i = 0; i < items.Count; i++)
                {
                    //randomize placement of items in container so they're not all stacked on stop of eachother
                    randX = Utility.RandomMinMax(0, 100);
                    randY = Utility.RandomMinMax(0, 100);

                    //items in Containers do not have a Z coordinate
                    if (container != null || !container.Deleted)
                        container.OnDragDropInto(this, items[i], new Point3D(randX, randY, 0));
                }
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.UltraRich, 2);
        }

        public override bool IsScaredOfScaryThings { get { return false; } }
        public override Poison PoisonImmune { get { return Poison.Regular; } }
        public override bool AutoDispel { get { return false; } }
        public override bool CanRummageCorpses { get { return false; } }

        public override bool PlayerRangeSensitive { get { return false; } }

        private bool boatspawn;

        public override bool OnBeforeDeath()
        {
            if (this.LastKiller != null && this.LastKiller is PlayerMobile)
            {
                this.LastKiller.SendGump(new BoatDecisionGump(this.LastKiller, m_DeepSeaBoat));
            }
            else
            {
                new SinkTimer(m_DeepSeaBoat, m_DeepSeaBoat.Z).Start();
            }

            return true;
        }

        public ActionAI_DockedFishermen(Serial serial) : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);

            writer.Write((Item)m_DeepSeaBoat);
            writer.Write((bool)boatspawn);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            m_DeepSeaBoat = reader.ReadItem() as MediumBoat;
            boatspawn = reader.ReadBool();


            Timer.DelayCall(TimeSpan.FromSeconds((Utility.RandomDouble() * 100.0)), StopAndFish);
        }
    }
}