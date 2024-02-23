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
    public class MiningGump : Gump
    {
        Mobile caller;
        OreCrate m_OreCrate;

        public MiningGump(Mobile from, OreCrate crate) : base(0, 0)
        {
            caller = from;
            m_OreCrate = crate;

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);

            AddImage(471, 180, 1417);

			AddItem(553, 212, 3718); // Pickaxe Facing NS
			AddItem(556, 212, 3717); // Pickaxe Facing EW
			AddItem(423, 212, 3718); // Pickaxe Facing NS
			AddItem(427, 212, 3717); // Pickaxe Facing EW

            if( m_OreCrate.Active)
            {
                if(m_OreCrate.Automatic)
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
                AddButton(520, 270, 2116, 2114, (int)Buttons.ManualMine, GumpButtonType.Reply, 0);
                AddButton(450, 270, 2113, 2111, (int)Buttons.AutoMine, GumpButtonType.Reply, 0);
            }
            
            AddButton(481, 190, 5573, 5574, (int)Buttons.MiningContainer, GumpButtonType.Reply, 0);
            AddButton(420, 250, 22150, 22151, (int)Buttons.Close, GumpButtonType.Reply, 0);
        }

        public enum Buttons
		{
			ManualMine,
			AutoMine,
			StartStop,
			MiningContainer,
			Close
		}

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case (int)Buttons.ManualMine:
                {
                    if ( from.Player && from.FindItemOnLayer(Layer.FirstValid) is Pickaxe )
                    {
                        m_OreCrate.Active = true;
                        m_OreCrate.Automatic = false;
                        Mine(from, m_OreCrate, false); 
                    }   

                    else
                        from.SendMessage("You must first equip a pickaxe.");
                
                    from.SendGump(new MiningGump(from, m_OreCrate));

                    break;
                }
                case (int)Buttons.AutoMine:
                {
                    m_OreCrate.Automatic = true;

                    from.Target = new OreCrate.MiningGumpTarget(m_OreCrate);
                    
                    Timer.DelayCall(TimeSpan.FromSeconds(2.0),
                        delegate
                        {
                            Mine(from, m_OreCrate, true);
                        });
                    

                    from.SendGump(new MiningGump(from, m_OreCrate));  

                    break;
                }
                case (int)Buttons.StartStop:
                {
                    if( m_OreCrate.Active )
                        m_OreCrate.Active = false;
                    else
                        m_OreCrate.Active = true;

                    from.SendGump(new MiningGump(from, m_OreCrate));

                    break;
                }
                case (int)Buttons.MiningContainer:
                {
                    if(m_OreCrate != null && !m_OreCrate.Deleted && m_OreCrate.RootParent == from)
                        m_OreCrate.Open(from);

                    from.SendGump(new MiningGump(from, m_OreCrate));

                    break;
                }
                case (int)Buttons.Close:
                {

                    break;
                }
            }
        }

        public void Mine(Mobile from, OreCrate crate, bool automatic)
        {
            // from.ShieldArmor is calling Server/Mobile.cs > [public Item ShieldArmor] which finds the item equipped on Layer.TwoHanded and returns that item
            // Since we have already confirmed the item equipped on that layer is a FishingPole, we can force double click it for targeting
            if(m_OreCrate.Deleted || m_OreCrate.RootParent != from)
            {
                return;
            }

            if( !automatic )
            {
                Item tool = from.FindItemOnLayer(Layer.FirstValid);
                
                if(tool is Pickaxe)
                    tool.OnDoubleClick(from); 
            }
            else
            {
                if ( from.Player && from.FindItemOnLayer(Layer.FirstValid) is Pickaxe )
                {
                    Item tool = from.ShieldArmor;
                    Map map = from.Map;
                    Point3D loc = new Point3D( from.X - crate.relativeLocation.X, from.Y - crate.relativeLocation.Y, crate.relativeLocation.Z);
                    from.SendMessage("{0}", loc);
                    HarvestBank bank = Mining.System.OreAndStone.GetBank(map, loc.X, loc.Y);
                    bool available = (bank != null && bank.Current >= Mining.System.OreAndStone.ConsumedPerHarvest);
                    LandTarget lt = new LandTarget(loc, from.Map);
                    StaticTarget st = new StaticTarget(loc, crate.staticTargetID);

                    if (!available)
                    {
                        from.SendLocalizedMessage(503172); // The fish don't seem to be biting here.
                    }
                    
                    else if (bank == null || bank.Current < Mining.System.OreAndStone.ConsumedPerHarvest)
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
                        HarvestResource fallback = Mining.System.OreAndStone.Resources[0];
                        HarvestResource resource = Mining.System.MutateResource(from, null, Mining.System.OreAndStone, map, loc, vein, primary, fallback);
                        double skillBase = from.Skills[Mining.System.OreAndStone.Skill].Base;

                        Type type = null;   

                        from.Direction = from.GetDirectionTo(loc);

                        if( crate.Active )
                        {
                            from.Animate(11, 5, 1, true, false, 0);
                            Effects.PlaySound(loc, from.Map, Utility.RandomList(Mining.System.OreAndStone.EffectSounds));
                        }
                        

                        if (skillBase >= resource.ReqSkill && from.CheckSkill(Mining.System.OreAndStone.Skill, resource.MinSkill, resource.MaxSkill))
                        {
                            type = Mining.System.GetResourceType(from, null, Mining.System.OreAndStone, map, loc, resource);

                            if (type != null)
                                type = Mining.System.MutateType(type, from, null, Mining.System.OreAndStone, map, loc, resource);

                            if (type != null)
                            {
                                Item itemHarvested = Mining.System.Construct(type, from);

                                if (itemHarvested == null)
                                {
                                    type = null;
                                }
                                else
                                {
                                    if (itemHarvested.Stackable)
                                    {
                                        int amount = Mining.System.OreAndStone.ConsumedPerHarvest;
                                        int feluccaAmount = Mining.System.OreAndStone.ConsumedPerFeluccaHarvest;

                                        bool inFelucca = (map == Map.Felucca);

                                        if (inFelucca)
                                            itemHarvested.Amount = feluccaAmount;
                                        else
                                            itemHarvested.Amount = amount;
                                    }

                                    bank.Consume(itemHarvested.Amount, from);

                                    if (crate == null || crate.Deleted || crate.RootParent != from )
                                    {
                                        from.SendMessage("Hmm, you seem to have lost your Ore Crate");
                                        return;
                                    }

                                    Container pack = from.Backpack;

                                    Timer.DelayCall(TimeSpan.FromSeconds(2.0),
                                        delegate
                                        {
                                            if( m_OreCrate.Active && m_OreCrate.Automatic )
                                            {
                                                crate.TryDropItem(from, itemHarvested, false);
                                                Mining.System.SendSuccessTo(from, itemHarvested, resource);
                                            }
                                        });
                                    

                                    BonusHarvestResource bonus = Mining.System.OreAndStone.GetBonusResource();

                                    if (bonus != null && bonus.Type != null && skillBase >= bonus.ReqSkill)
                                    {
                                        Item bonusItem = Mining.System.Construct(bonus.Type, from);

                                        Timer.DelayCall(TimeSpan.FromSeconds(2.0),
                                            delegate
                                            {
                                                if( m_OreCrate.Active && m_OreCrate.Automatic )
                                                {
                                                    crate.TryDropItem(from, bonusItem, false);
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
                

            if(crate.Active)
            {
                Timer.DelayCall( TimeSpan.FromSeconds( 5.0 ), 
                    delegate 
                    {
                        if(crate.Active)
                            Mine(from, crate, automatic);
                    } );
            }
        }
    }
}