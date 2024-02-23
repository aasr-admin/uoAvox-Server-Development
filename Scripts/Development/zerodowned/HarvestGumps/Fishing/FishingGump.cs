using Server;
using Server.Commands;
using Server.Gumps;
using Server.Engines.Harvest;
using Server.Network;
using Server.Targeting;
using System;
using Server.Items;
using Server.Mobiles;
using Server.Engines.Harvest;

namespace Server.Gumps
{
    public class FishingGump : Gump
    {
        Mobile caller;
        Creel m_creel;

        public FishingGump(Mobile from, Creel creel) : base(0, 0)
        {
            caller = from;
            m_creel = creel;

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);

            //AddImageTiled(410, 175, 195, 140, 2624);
            //AddAlphaRegion(410, 175, 195, 140);
            
            AddImage(471, 180, 1417);

            AddItem(499, 218, 3519); // FishingPole Facing NS
            AddItem(401, 218, 3520); // FishingPole Facing EW

            if( m_creel.Active)
            {
                if(m_creel.Automatic)
                {
                    AddButton(450, 270, 2113, 2111, (int)Buttons.StartStop, GumpButtonType.Reply, 0);
                    AddImage(450, 270, 2113, 70);

                    AddImage(520, 270, 2116, 929);
                }
                else
                {
                    AddButton(520, 270, 2116, 2114, (int)Buttons.StartStop, GumpButtonType.Reply, 0);
                    AddImage(520, 270, 2116, 70);

                    AddImage(450, 270, 2113, 929);
                }
            }
            else
            {
                AddButton(520, 270, 2116, 2114, (int)Buttons.ManualFish, GumpButtonType.Reply, 0);
                AddButton(450, 270, 2113, 2111, (int)Buttons.AutoFish, GumpButtonType.Reply, 0);
            }
            
            AddButton(481, 190, 5565, 5566, (int)Buttons.FishingContainer, GumpButtonType.Reply, 0);
            AddButton(420, 250, 22150, 22151, (int)Buttons.Close, GumpButtonType.Reply, 0);
        }

        public enum Buttons
        {
            ManualFish,
            AutoFish,
            StartStop,
            FishingContainer,
            Close
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case (int)Buttons.ManualFish:
                {
                    if ( from.Player && from.FindItemOnLayer(Layer.TwoHanded) is FishingPole )
                    {
                        m_creel.Active = true;
                        m_creel.Automatic = false;
                        Fish(from, m_creel, false); 
                    }   

                    else
                        from.SendMessage("You must first equip a fishing pole.");
                
                    from.SendGump(new FishingGump(from, m_creel));

                    break;
                }
                case (int)Buttons.AutoFish:
                {
                    m_creel.Automatic = true;

                    from.Target = new Creel.FishingGumpTarget(m_creel);
                    
                    Timer.DelayCall(TimeSpan.FromSeconds(2.0),
                        delegate
                        {
                            Fish(from, m_creel, true);
                        });
                    

                    from.SendGump(new FishingGump(from, m_creel));  

                    break;
                }
                case (int)Buttons.StartStop:
                {
                    if( m_creel.Active )
                        m_creel.Active = false;
                    else
                        m_creel.Active = true;

                    from.SendGump(new FishingGump(from, m_creel));

                    break;
                }
                case (int)Buttons.FishingContainer:
                {
                    if(m_creel != null && !m_creel.Deleted && m_creel.RootParent == from)
                        m_creel.Open(from);

                    from.SendGump(new FishingGump(from, m_creel));

                    break;
                }
                case (int)Buttons.Close:
                {

                    break;
                }
            }
        }

        public void Fish(Mobile from, Creel creel, bool automatic)
        {
            // from.ShieldArmor is calling Server/Mobile.cs > [public Item ShieldArmor] which finds the item equipped on Layer.TwoHanded and returns that item
            // Since we have already confirmed the item equipped on that layer is a FishingPole, we can force double click it for targeting
            if(m_creel.Deleted || m_creel.RootParent != from)
            {
                return;
            }

            if( !automatic )
                from.ShieldArmor.OnDoubleClick(from); 
            else
            {
                if ( from.Player && from.FindItemOnLayer(Layer.TwoHanded) is FishingPole )
                {
                    Item tool = from.ShieldArmor;
                    Map map = from.Map;
                    Point3D loc = new Point3D( from.X - creel.relativeLocation.X, from.Y - creel.relativeLocation.Y, creel.relativeLocation.Z);
                    from.SendMessage("{0}", loc);
                    HarvestBank bank = Fishing.System.Definition.GetBank(map, loc.X, loc.Y);
                    bool available = (bank != null && bank.Current >= Fishing.System.Definition.ConsumedPerHarvest);
                    LandTarget lt = new LandTarget(loc, from.Map);
                    StaticTarget st = new StaticTarget(loc, creel.staticTargetID);

                    if (!available)
                    {
                        
                        from.SendLocalizedMessage(503172); // The fish don't seem to be biting here.
                    }
                    
                    
                    else if (bank == null || bank.Current < Fishing.System.Definition.ConsumedPerHarvest)
                    {   
                        from.SendLocalizedMessage(503171); // You fish a while, but fail to catch anything.
                    }
                    else
                    {
                        HarvestVein vein = bank.Vein;

                        if (vein == null)
                        {    
                            from.SendLocalizedMessage(500976); // You need to be closer to the water to fish!
                        }

                        HarvestResource primary = vein.PrimaryResource;
                        HarvestResource fallback = Fishing.System.Definition.Resources[0];
                        HarvestResource resource = Fishing.System.MutateResource(from, null, Fishing.System.Definition, map, loc, vein, primary, fallback);
                        double skillBase = from.Skills[Fishing.System.Definition.Skill].Base;

                        Type type = null;   

                        from.Direction = from.GetDirectionTo(loc);

                        if(m_creel.Active)
                            from.Animate(11, 5, 1, true, false, 0);
                        

                        Timer.DelayCall(TimeSpan.FromSeconds(0.7),
                                delegate
                                {
                                    if( m_creel.Active && m_creel.Automatic )
                                    {
                                        Effects.SendLocationEffect(loc, from.Map, 0x352D, 16, 4); //water splash
                                        Effects.PlaySound(loc, from.Map, 0x364);
                                    }
                                });
                        

                        if (skillBase >= resource.ReqSkill && from.CheckSkill(Fishing.System.Definition.Skill, resource.MinSkill, resource.MaxSkill))
                        {
                            type = Fishing.System.GetResourceType(from, null, Fishing.System.Definition, map, loc, resource);

                            if (type != null)
                                type = Fishing.System.MutateType(type, from, null, Fishing.System.Definition, map, loc, resource);

                            if (type != null)
                            {
                                Item itemHarvested = Fishing.System.Construct(type, from);

                                if (itemHarvested == null)
                                {
                                    type = null;
                                }
                                else
                                {
                                    if (itemHarvested.Stackable)
                                    {
                                        int amount = Fishing.System.Definition.ConsumedPerHarvest;
                                        int feluccaAmount = Fishing.System.Definition.ConsumedPerFeluccaHarvest;

                                        bool inFelucca = (map == Map.Felucca);

                                        if (inFelucca)
                                            itemHarvested.Amount = feluccaAmount;
                                        else
                                            itemHarvested.Amount = amount;
                                    }

                                    bank.Consume(itemHarvested.Amount, from);

                                    if (creel == null || creel.Deleted || creel.RootParent != from )
                                    {
                                        from.SendMessage("Hmm, you seem to have lost your Creel");
                                        return;
                                    }

                                    Container pack = from.Backpack;

                                    Timer.DelayCall(TimeSpan.FromSeconds(4.0),
                                        delegate
                                        {
                                            if( m_creel.Active && m_creel.Automatic )
                                            {
                                                if( !(itemHarvested is BaseContainer || itemHarvested is ShipwreckedItem) )
                                                {
                                                    creel.TryDropItem(from, itemHarvested, false);
                                                }
                                                else
                                                {
                                                    pack.TryDropItem(from, itemHarvested, false);
                                                }
                                                
                                                Fishing.System.SendSuccessTo(from, itemHarvested, resource);
                                            }
                                        });
                                    

                                    BonusHarvestResource bonus = Fishing.System.Definition.GetBonusResource();

                                    if (bonus != null && bonus.Type != null && skillBase >= bonus.ReqSkill)
                                    {
                                        Item bonusItem = Fishing.System.Construct(bonus.Type, from);

                                        Timer.DelayCall(TimeSpan.FromSeconds(4.0),
                                            delegate
                                            {
                                                if( m_creel.Active && m_creel.Automatic )
                                                {
                                                    if( !(bonusItem is BaseContainer || bonusItem is ShipwreckedItem) )
                                                    {
                                                        creel.TryDropItem(from, bonusItem, false);
                                                    }
                                                    else
                                                    {
                                                        pack.TryDropItem(from, bonusItem, false);
                                                    }
                                                    
                                                    bonus.SendSuccessTo(from);
                                                }
                                            });

                                        
                                    }

                                /* if (tool is IUsesRemaining)
                                {
                                    IUsesRemaining toolWithUses = (IUsesRemaining)tool;

                                    toolWithUses.ShowUsesRemaining = true;

                                    if (toolWithUses.UsesRemaining > 0)
                                        --toolWithUses.UsesRemaining;

                                    if (toolWithUses.UsesRemaining < 1)
                                    {
                                        tool.Delete();
                                        def.SendMessageTo(from, def.ToolBrokeMessage);
                                    }
                                } */
                                }
                            }
                        }
                       
                    }
                }
                         
                

            }
                

            if(creel.Active)
            {
                Timer.DelayCall( TimeSpan.FromSeconds( 7.0 ), 
                    delegate 
                    {
                        if(creel.Active)
                            Fish(from, creel, automatic);
                    } );
            }
        }
    }
}