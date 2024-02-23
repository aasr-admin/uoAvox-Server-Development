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
    public class LumberjackingGump : Gump
    {
        Mobile caller;
        LogCrate m_LogCrate;

        public LumberjackingGump(Mobile from, LogCrate crate) : base(0, 0)
        {
            caller = from;
            m_LogCrate = crate;

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);

            AddImage(471, 180, 1417);

			AddItem(553, 212, 3907); // axe Facing NS
			AddItem(556, 212, 3908); // axe Facing EW
			AddItem(423, 212, 3907); // axe Facing NS
			AddItem(427, 212, 3908); // axe Facing EW

            if( m_LogCrate.Active)
            {
                if(m_LogCrate.Automatic)
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
            
            AddButton(481, 190, 5559, 5560, (int)Buttons.MiningContainer, GumpButtonType.Reply, 0);
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
                    if ( from.Player && from.FindItemOnLayer(Layer.FirstValid) is BaseAxe || from.Player && from.FindItemOnLayer(Layer.TwoHanded) is BaseAxe )
                    {
                        m_LogCrate.Active = true;
                        m_LogCrate.Automatic = false;
                        Mine(from, m_LogCrate, false); 
                    }   

                    else
                        from.SendMessage("You must first equip a axe.");
                
                    from.SendGump(new LumberjackingGump(from, m_LogCrate));

                    break;
                }
                case (int)Buttons.AutoMine:
                {
                    m_LogCrate.Automatic = true;

                    from.Target = new LogCrate.LumberjackingGumpTarget(m_LogCrate);
                    
                    Timer.DelayCall(TimeSpan.FromSeconds(2.0),
                        delegate
                        {
                            Mine(from, m_LogCrate, true);
                        });
                    

                    from.SendGump(new LumberjackingGump(from, m_LogCrate));  

                    break;
                }
                case (int)Buttons.StartStop:
                {
                    if( m_LogCrate.Active )
                        m_LogCrate.Active = false;
                    else
                        m_LogCrate.Active = true;

                    from.SendGump(new LumberjackingGump(from, m_LogCrate));

                    break;
                }
                case (int)Buttons.MiningContainer:
                {
                    if(m_LogCrate != null && !m_LogCrate.Deleted && m_LogCrate.RootParent == from)
                        m_LogCrate.Open(from);

                    from.SendGump(new LumberjackingGump(from, m_LogCrate));

                    break;
                }
                case (int)Buttons.Close:
                {

                    break;
                }
            }
        }

        public void Mine(Mobile from, LogCrate crate, bool automatic)
        {
            // from.ShieldArmor is calling Server/Mobile.cs > [public Item ShieldArmor] which finds the item equipped on Layer.TwoHanded and returns that item
            // Since we have already confirmed the item equipped on that layer is a FishingPole, we can force double click it for targeting
            if(m_LogCrate.Deleted || m_LogCrate.RootParent != from)
            {
                return;
            }

            if( !automatic )
            {
                Item tool = from.FindItemOnLayer(Layer.FirstValid);
                
                if(tool is BaseAxe )
                    tool.OnDoubleClick(from); 
            }
            else
            {
                if ( from.Player && from.FindItemOnLayer(Layer.FirstValid) is BaseAxe || from.Player && from.FindItemOnLayer(Layer.TwoHanded) is BaseAxe )
                {
                    Item tool = from.ShieldArmor;
                    Map map = from.Map;
                    Point3D loc = new Point3D( from.X - crate.relativeLocation.X, from.Y - crate.relativeLocation.Y, crate.relativeLocation.Z);
                    from.SendMessage("{0}", loc);
                    HarvestBank bank = Lumberjacking.System.Definition.GetBank(map, loc.X, loc.Y);
                    bool available = (bank != null && bank.Current >= Lumberjacking.System.Definition.ConsumedPerHarvest);
                    LandTarget lt = new LandTarget(loc, from.Map);
                    StaticTarget st = new StaticTarget(loc, crate.staticTargetID);

                    if (!available)
                    {
                        from.SendLocalizedMessage(503172); // The fish don't seem to be biting here.
                    }
                    
                    else if (bank == null || bank.Current < Lumberjacking.System.Definition.ConsumedPerHarvest)
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
                        HarvestResource fallback = Lumberjacking.System.Definition.Resources[0];
                        HarvestResource resource = Lumberjacking.System.MutateResource(from, null, Lumberjacking.System.Definition, map, loc, vein, primary, fallback);
                        double skillBase = from.Skills[Lumberjacking.System.Definition.Skill].Base;

                        Type type = null;   

                        from.Direction = from.GetDirectionTo(loc);

                        if( crate.Active )
                        {
                            from.Animate(11, 5, 1, true, false, 0);
                            Effects.PlaySound(loc, from.Map, Utility.RandomList(Lumberjacking.System.Definition.EffectSounds));
                        }
                        

                        if (skillBase >= resource.ReqSkill && from.CheckSkill(Lumberjacking.System.Definition.Skill, resource.MinSkill, resource.MaxSkill))
                        {
                            type = Lumberjacking.System.GetResourceType(from, null, Lumberjacking.System.Definition, map, loc, resource);

                            if (type != null)
                                type = Lumberjacking.System.MutateType(type, from, null, Lumberjacking.System.Definition, map, loc, resource);

                            if (type != null)
                            {
                                Item itemHarvested = Lumberjacking.System.Construct(type, from);

                                if (itemHarvested == null)
                                {
                                    type = null;
                                }
                                else
                                {
                                    if (itemHarvested.Stackable)
                                    {
                                        int amount = Lumberjacking.System.Definition.ConsumedPerHarvest;
                                        int feluccaAmount = Lumberjacking.System.Definition.ConsumedPerFeluccaHarvest;

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
                                            if( m_LogCrate.Active && m_LogCrate.Automatic )
                                            {
                                                crate.TryDropItem(from, itemHarvested, false);
                                                Lumberjacking.System.SendSuccessTo(from, itemHarvested, resource);
                                            }
                                        });
                                    

                                    BonusHarvestResource bonus = Lumberjacking.System.Definition.GetBonusResource();

                                    if (bonus != null && bonus.Type != null && skillBase >= bonus.ReqSkill)
                                    {
                                        Item bonusItem = Lumberjacking.System.Construct(bonus.Type, from);

                                        Timer.DelayCall(TimeSpan.FromSeconds(2.0),
                                            delegate
                                            {
                                                if( m_LogCrate.Active && m_LogCrate.Automatic )
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
