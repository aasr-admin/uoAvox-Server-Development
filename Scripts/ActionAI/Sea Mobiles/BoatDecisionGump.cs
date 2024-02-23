using Server;
using Server.Engines.CannedEvil;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Movement;
using Server.Multis;
using Server.Network;
using Server.Prompts;
using Server.Regions;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Gumps
{
	
	public class BoatDecisionGump : Gump
	{
		public static void Initialize()
		{
			EventSink.PlayerDeath += new PlayerDeathEventHandler( EventSink_PlayerDeath );
		}

		public static void EventSink_PlayerDeath( PlayerDeathEventArgs e )
		{
			Mobile m = e.Mobile;
			Mobile lastKiller = m.LastKiller;
			
			foreach (Item item in m.GetItemsInRange(20))
			{
				if (item is BaseBoat && ((BaseBoat)item).Owner == m)
				{
					if (lastKiller is PlayerMobile)
					{
						lastKiller.SendGump(new BoatDecisionGump(lastKiller, ((BaseBoat)item)));
					}
					else
					{
						Timer.DelayCall(TimeSpan.FromSeconds(7.5),
							delegate {

								if (!((BaseBoat)item).Owner.Alive)
									new SinkTimer(((BaseBoat)item), ((BaseBoat)item).Z).Start();
							});
					}
				}
			}
		}

		Mobile caller;
		private BaseBoat m_boat;

		public BoatDecisionGump(Mobile from, BaseBoat boat) : base(0, 0)
		{
			m_boat = boat;

			this.Closable = true;
			this.Disposable = true;
			this.Dragable = true;
			this.Resizable = false;

			AddPage(0);

			AddBackground(0, 0, 205, 180, 5170);

			AddButton(175, 6, 3, 3, 0, GumpButtonType.Reply, 0); // close button

			AddButton(23, 45, 2151, 2153, 1, GumpButtonType.Reply, 0);
			AddHtml(58, 47, 200, 25, "<big>Capture Ship</big>", false, false);

			AddButton(23, 102, 2151, 2153, 2, GumpButtonType.Reply, 0);
			AddHtml(58, 102, 200, 25, "<big>Destroy Ship</big>", false, false);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;

			if (info.ButtonID == 0)
			{
			}
			else if (info.ButtonID == 1)
			{
				CaptureBoat(from);
			}
			else if (info.ButtonID == 2)
			{
				if (m_boat != null)
				{
					new SinkTimer(m_boat, m_boat.Z).Start();
				}
			}
		}

		public void CaptureBoat(Mobile from)
		{
			if (m_boat != null && m_boat.Deleted)
				return;

			/* DryDockResult result = CheckDryDock( from );

			if ( result == DryDockResult.Dead )
				from.SendLocalizedMessage( 502493 ); // You appear to be dead.
			else if ( result == DryDockResult.NoKey )
				from.SendLocalizedMessage( 502494 ); // You must have a key to the ship to dock the boat.
			else if ( result == DryDockResult.NotAnchored )
				from.SendLocalizedMessage( 1010570 ); // You must lower the anchor to dock the boat.
			else if ( result == DryDockResult.Mobiles )
				from.SendLocalizedMessage( 502495 ); // You cannot dock the ship with beings on board!
			else if ( result == DryDockResult.Items )
				from.SendLocalizedMessage( 502496 ); // You cannot dock the ship with a cluttered deck.
			else if ( result == DryDockResult.Hold )
				from.SendLocalizedMessage( 502497 ); // Make sure your hold is empty, and try again!

			if ( result != DryDockResult.Valid )
				return; */

			BaseDockedBoat boat = m_boat.DockedBoat;

			if (boat == null)
				return;

			//RemoveKeys( from );

			from.AddToBackpack(boat);
			m_boat.Delete();
		}
	}
}