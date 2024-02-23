using Server.Items;
using Server.Network;
using Server.Prompts;
using Server.Regions;
using Server.Targeting;

using System;
using System.Collections.Generic;

#region Developer Notations

// new Rectangle2D(16, 16,...):

/// [x]  The 1st numeric value of 16 is a measurement, in pixels, subtracting from the playable portion of the maps edge, to create a wrap zone:
///		 this wrap zone comes in from the edge of your map 16 pixels into the ocean. When your players sail their boats from West to East 
///		 they will pass through a buffer zone and just before they can physically see the maps edge, the boat will teleport over the edge. 
///		 This prevents players from seeing the maps edge and makes traveling seem seamless.
/// 
/// [x]  The 2nd numeric value of 16 is a measurement, in pixels, subtracting from the playable portion of the maps edge, to create a wrap zone:
///		 this wrap zone comes in from the edge of your map 16 pixels into the ocean. When your players sail their boats from North to South 
///		 they will pass through a buffer zone and just before they can physically see the maps edge, the boat will teleport over the edge. 
///		 This prevents players from seeing the maps edge and makes traveling seem seamless.

// new Rectangle2D(..., 5120 - 32,...):

/// [x]  The numeric value of 5120 = a measurement, in pixels, of the playable portion (think: outdoor surface world area) of your facet:
///									 this is how large the the width of your map is, from the top left corner to the bottom right corner.
///									 
/// [x]  The numeric value of -32  = a measurement, in pixels, subtracting from the playable portion of the maps edge, to create a buffer zone:
///									 this buffer zone comes in from the edge of your map 32 pixels into the ocean. Your player boats serves
///									 to prevent your players from physically seeing the edge of the facet they are on.						 

// new Rectangle2D(..., 4096 - 32)						

/// [x] The numeric value of 4096  = a measurement, in pixels, of the playable portion (think: outdoor surface world area) of your facet:
///									 this is how large the the height of your map is, from the top left corner to the bottom right corner.
///									 
/// [x]  The numeric value of -32  = a measurement, in pixels, subtracting from the playable portion of the maps edge, to create a buffer zone:
///									 this buffer zone comes in from the edge of your map 32 pixels into the ocean. Your player boats serves
///									 to prevent your players from physically seeing the edge of the facet they are on.	

#endregion

namespace Server.Multis
{
	public enum BoatOrder
	{
		Move,
		Course,
		Single
	}

	public abstract class BaseBoat : BaseMulti
	{
		private static readonly TimeSpan BoatDecayDelay = TimeSpan.FromDays(9.0);

		public static BaseBoat FindBoatAt(IPoint2D loc, Map map)
		{
			var sector = map.GetSector(loc);

			for (var i = 0; i < sector.Multis.Count; i++)
			{
				if (sector.Multis[i] is BaseBoat boat && boat.Contains(loc.X, loc.Y))
				{
					return boat;
				}
			}

			return null;
		}

		private Region m_Region = Map.Internal.DefaultRegion;

		[CommandProperty(AccessLevel.GameMaster)]
		public Region Region
		{
			get
			{
				if (m_Region == null)
				{
					if (Map == null)
					{
						return Map.Internal.DefaultRegion;
					}

					return Map.DefaultRegion;
				}

				return m_Region;
			}
		}

		private Direction m_Facing;
		private int m_ClientSpeed;
		private string m_ShipName;
		private DateTime m_DecayTime;

		private Timer m_TurnTimer;
		private Timer m_MoveTimer;

		[CommandProperty(AccessLevel.GameMaster)]
		public Hold Hold { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TillerMan TillerMan { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Plank PPlank { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Plank SPlank { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Owner { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Direction Facing { get => m_Facing; set => SetFacing(value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public Direction Moving { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsMoving => m_MoveTimer != null;

		[CommandProperty(AccessLevel.GameMaster)]
		public int Speed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Anchored { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string ShipName
		{
			get => m_ShipName; set
			{
				m_ShipName = value;
				TillerMan?.InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public BoatOrder Order { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public MapItem MapItem { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int NextNavPoint { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime TimeOfDecay
		{
			get => m_DecayTime; set
			{
				m_DecayTime = value;
				TillerMan?.InvalidateProperties();
			}
		}

		public int Status
		{
			get
			{
				var start = TimeOfDecay - BoatDecayDelay;

				if (DateTime.UtcNow - start < TimeSpan.FromHours(1.0))
				{
					return 1043010; // This structure is like new.
				}

				if (DateTime.UtcNow - start < TimeSpan.FromDays(2.0))
				{
					return 1043011; // This structure is slightly worn.
				}

				if (DateTime.UtcNow - start < TimeSpan.FromDays(3.0))
				{
					return 1043012; // This structure is somewhat worn.
				}

				if (DateTime.UtcNow - start < TimeSpan.FromDays(4.0))
				{
					return 1043013; // This structure is fairly worn.
				}

				if (DateTime.UtcNow - start < TimeSpan.FromDays(5.0))
				{
					return 1043014; // This structure is greatly worn.
				}

				return 1043015; // This structure is in danger of collapsing.
			}
		}

		public virtual int NorthID => 0;
		public virtual int EastID => 0;
		public virtual int SouthID => 0;
		public virtual int WestID => 0;

		public virtual int HoldDistance => 0;
		public virtual int TillerManDistance => 0;
		public virtual Point2D StarboardOffset => Point2D.Zero;
		public virtual Point2D PortOffset => Point2D.Zero;
		public virtual Point3D MarkOffset => Point3D.Zero;

		public virtual BaseDockedBoat DockedBoat => null;

		public static List<BaseBoat> Boats { get; } = new List<BaseBoat>();

		public BaseBoat() : base(0x0)
		{
			m_DecayTime = DateTime.UtcNow + BoatDecayDelay;

			TillerMan = new TillerMan(this);
			Hold = new Hold(this);

			PPlank = new Plank(this, PlankSide.Port, 0);
			SPlank = new Plank(this, PlankSide.Starboard, 0);

			PPlank.MoveToWorld(new Point3D(X + PortOffset.X, Y + PortOffset.Y, Z), Map);
			SPlank.MoveToWorld(new Point3D(X + StarboardOffset.X, Y + StarboardOffset.Y, Z), Map);

			Facing = Direction.North;

			NextNavPoint = -1;

			Movable = false;

			Boats.Add(this);
		}

		public BaseBoat(Serial serial) : base(serial)
		{
		}

		public void UpdateRegion()
		{
			if (Deleted)
			{
				return;
			}

			var oldRegion = m_Region;
			var newRegion = Region.Find(Location, Map);

			if (oldRegion != newRegion)
			{
				m_Region = newRegion;

				Region.OnVehicleRegionChange(this, oldRegion, newRegion);

				OnRegionChange(oldRegion, newRegion);
			}
		}

		public virtual void OnRegionChange(Region Old, Region New)
		{
		}

		public Point3D GetRotatedLocation(int x, int y)
		{
			var p = new Point3D(X + x, Y + y, Z);

			return Rotate(p, (int)m_Facing / 2);
		}

		public void UpdateComponents()
		{
			if (PPlank != null)
			{
				PPlank.MoveToWorld(GetRotatedLocation(PortOffset.X, PortOffset.Y), Map);
				PPlank.SetFacing(m_Facing);
			}

			if (SPlank != null)
			{
				SPlank.MoveToWorld(GetRotatedLocation(StarboardOffset.X, StarboardOffset.Y), Map);
				SPlank.SetFacing(m_Facing);
			}

			int xOffset = 0, yOffset = 0;
			Movement.Movement.Offset(m_Facing, ref xOffset, ref yOffset);

			if (TillerMan != null)
			{
				TillerMan.Location = new Point3D(X + (xOffset * TillerManDistance) + (m_Facing == Direction.North ? 1 : 0), Y + (yOffset * TillerManDistance), TillerMan.Z);
				TillerMan.SetFacing(m_Facing);
				TillerMan.InvalidateProperties();
			}

			if (Hold != null)
			{
				Hold.Location = new Point3D(X + (xOffset * HoldDistance), Y + (yOffset * HoldDistance), Hold.Z);
				Hold.SetFacing(m_Facing);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(3);

			writer.Write(MapItem);
			writer.Write(NextNavPoint);

			writer.Write((int)m_Facing);

			writer.WriteDeltaTime(m_DecayTime);

			writer.Write(Owner);
			writer.Write(PPlank);
			writer.Write(SPlank);
			writer.Write(TillerMan);
			writer.Write(Hold);
			writer.Write(Anchored);
			writer.Write(m_ShipName);

			_ = CheckDecay();
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 3:
					{
						MapItem = (MapItem)reader.ReadItem();
						NextNavPoint = reader.ReadInt();

						goto case 2;
					}
				case 2:
					{
						m_Facing = (Direction)reader.ReadInt();

						goto case 1;
					}
				case 1:
					{
						m_DecayTime = reader.ReadDeltaTime();

						goto case 0;
					}
				case 0:
					{
						if (version < 3)
						{
							NextNavPoint = -1;
						}

						if (version < 2)
						{
							if (ItemID == NorthID)
							{
								m_Facing = Direction.North;
							}
							else if (ItemID == SouthID)
							{
								m_Facing = Direction.South;
							}
							else if (ItemID == EastID)
							{
								m_Facing = Direction.East;
							}
							else if (ItemID == WestID)
							{
								m_Facing = Direction.West;
							}
						}

						Owner = reader.ReadMobile();
						PPlank = reader.ReadItem() as Plank;
						SPlank = reader.ReadItem() as Plank;
						TillerMan = reader.ReadItem() as TillerMan;
						Hold = reader.ReadItem() as Hold;
						Anchored = reader.ReadBool();
						m_ShipName = reader.ReadString();

						if (version < 1)
						{
							Refresh();
						}

						break;
					}
			}

			Boats.Add(this);
		}

		public void RemoveKeys(Mobile m)
		{
			var keyValue = 0u;

			if (PPlank != null)
			{
				keyValue = PPlank.KeyValue;
			}

			if (keyValue == 0 && SPlank != null)
			{
				keyValue = SPlank.KeyValue;
			}

			Key.RemoveKeys(m, keyValue);
		}

		public uint CreateKeys(Mobile m)
		{
			var value = Key.RandomValue();

			var packKey = new Key(KeyType.Gold, value, this);
			var bankKey = new Key(KeyType.Gold, value, this);

			packKey.MaxRange = 10;
			bankKey.MaxRange = 10;

			packKey.Name = "a ship key";
			bankKey.Name = "a ship key";

			var box = m.BankBox;

			if (!box.TryDropItem(m, bankKey, false))
			{
				bankKey.Delete();
			}
			else
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502484); // A ship's key is now in my safety deposit box.
			}

			if (m.AddToBackpack(packKey))
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502485); // A ship's key is now in my backpack.
			}
			else
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502483); // A ship's key is now at my feet.
			}

			return value;
		}

		public override void OnAfterDelete()
		{
			TillerMan?.Delete();

			Hold?.Delete();

			PPlank?.Delete();

			SPlank?.Delete();

			m_TurnTimer?.Stop();

			m_MoveTimer?.Stop();

			_ = Boats.Remove(this);
		}

		public override void OnLocationChange(Point3D old)
		{
			base.OnLocationChange(old);

			if (TillerMan != null)
			{
				TillerMan.Location = new Point3D(X + (TillerMan.X - old.X), Y + (TillerMan.Y - old.Y), Z + (TillerMan.Z - old.Z));
			}

			if (Hold != null)
			{
				Hold.Location = new Point3D(X + (Hold.X - old.X), Y + (Hold.Y - old.Y), Z + (Hold.Z - old.Z));
			}

			if (PPlank != null)
			{
				PPlank.Location = new Point3D(X + (PPlank.X - old.X), Y + (PPlank.Y - old.Y), Z + (PPlank.Z - old.Z));
			}

			if (SPlank != null)
			{
				SPlank.Location = new Point3D(X + (SPlank.X - old.X), Y + (SPlank.Y - old.Y), Z + (SPlank.Z - old.Z));
			}

			UpdateRegion();

			Region?.OnVehicleLocationChanged(this, old);
		}

		public override void OnMapChange(Map oldMap)
		{
			base.OnMapChange(oldMap);

			if (TillerMan != null)
			{
				TillerMan.Map = Map;
			}

			if (Hold != null)
			{
				Hold.Map = Map;
			}

			if (PPlank != null)
			{
				PPlank.Map = Map;
			}

			if (SPlank != null)
			{
				SPlank.Map = Map;
			}

			if (!IsMoving)
			{
				UpdateRegion();
			}
		}

		public virtual bool CanCommand(Mobile m)
		{
			return true;
		}

		public virtual Point3D GetMarkedLocation()
		{
			var p = new Point3D(X + MarkOffset.X, Y + MarkOffset.Y, Z + MarkOffset.Z);

			return Rotate(p, (int)m_Facing / 2);
		}

		public virtual bool CheckKey(uint keyValue)
		{
			if (SPlank != null && SPlank.KeyValue == keyValue)
			{
				return true;
			}

			if (PPlank != null && PPlank.KeyValue == keyValue)
			{
				return true;
			}

			return false;
		}

		/*
		 * Intervals:
		 *       drift forward
		 * fast | 0.25|   0.25
		 * slow | 0.50|   0.50
		 *
		 * Speed:
		 *       drift forward
		 * fast |  0x4|    0x4
		 * slow |  0x3|    0x3
		 *
		 * Tiles (per interval):
		 *       drift forward
		 * fast |    1|      1
		 * slow |    1|      1
		 *
		 * 'walking' in piloting mode has a 1s interval, speed 0x2
		 */

		private static bool NewBoatMovement => Core.HS;

		private static readonly TimeSpan SlowInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.50 : 0.75);
		private static readonly TimeSpan FastInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.25 : 0.75);

		private static readonly int SlowSpeed = 1;
		private static readonly int FastSpeed = NewBoatMovement ? 1 : 3;

		private static readonly TimeSpan SlowDriftInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.50 : 1.50);
		private static readonly TimeSpan FastDriftInterval = TimeSpan.FromSeconds(NewBoatMovement ? 0.25 : 0.75);

		private static readonly int SlowDriftSpeed = 1;
		private static readonly int FastDriftSpeed = 1;

		private static readonly Direction Forward = Direction.North;
		private static readonly Direction ForwardLeft = Direction.Up;
		private static readonly Direction ForwardRight = Direction.Right;
		private static readonly Direction Backward = Direction.South;
		private static readonly Direction BackwardLeft = Direction.Left;
		private static readonly Direction BackwardRight = Direction.Down;
		private static readonly Direction Left = Direction.West;
		private static readonly Direction Right = Direction.East;

		private bool m_Decaying;

		public void Refresh()
		{
			m_DecayTime = DateTime.UtcNow + BoatDecayDelay;

			TillerMan?.InvalidateProperties();
		}

		private class DecayTimer : Timer
		{
			private readonly BaseBoat m_Boat;
			private int m_Count;

			public DecayTimer(BaseBoat boat) : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(5.0))
			{
				m_Boat = boat;

				Priority = TimerPriority.TwoFiftyMS;
			}

			protected override void OnTick()
			{
				if (m_Count == 5)
				{
					m_Boat.Delete();
					Stop();
				}
				else
				{
					m_Boat.Location = new Point3D(m_Boat.X, m_Boat.Y, m_Boat.Z - 1);

					m_Boat.TillerMan?.Say(1007168 + m_Count);

					++m_Count;
				}
			}
		}

		public bool CheckDecay()
		{
			if (m_Decaying)
			{
				return true;
			}

			if (!IsMoving && DateTime.UtcNow >= m_DecayTime)
			{
				new DecayTimer(this).Start();

				m_Decaying = true;

				return true;
			}

			return false;
		}

		public bool LowerAnchor(bool message)
		{
			if (CheckDecay())
			{
				return false;
			}

			if (Anchored)
			{
				if (message && TillerMan != null)
				{
					TillerMan.Say(501445); // Ar, the anchor was already dropped sir.
				}

				return false;
			}

			_ = StopMove(false);

			Anchored = true;

			if (message && TillerMan != null)
			{
				TillerMan.Say(501444); // Ar, anchor dropped sir.
			}

			return true;
		}

		public bool RaiseAnchor(bool message)
		{
			if (CheckDecay())
			{
				return false;
			}

			if (!Anchored)
			{
				if (message && TillerMan != null)
				{
					TillerMan.Say(501447); // Ar, the anchor has not been dropped sir.
				}

				return false;
			}

			Anchored = false;

			if (message && TillerMan != null)
			{
				TillerMan.Say(501446); // Ar, anchor raised sir.
			}

			return true;
		}

		public bool StartMove(Direction dir, bool fast)
		{
			if (CheckDecay())
			{
				return false;
			}

			var drift = dir != Forward && dir != ForwardLeft && dir != ForwardRight;
			var interval = fast ? (drift ? FastDriftInterval : FastInterval) : (drift ? SlowDriftInterval : SlowInterval);
			var speed = fast ? (drift ? FastDriftSpeed : FastSpeed) : (drift ? SlowDriftSpeed : SlowSpeed);
			var clientSpeed = fast ? 0x4 : 0x3;

			if (StartMove(dir, speed, clientSpeed, interval, false, true))
			{
				TillerMan?.Say(501429); // Aye aye sir.

				return true;
			}

			return false;
		}

		public bool OneMove(Direction dir)
		{
			if (CheckDecay())
			{
				return false;
			}

			var drift = dir != Forward;
			var interval = drift ? FastDriftInterval : FastInterval;
			var speed = drift ? FastDriftSpeed : FastSpeed;

			if (StartMove(dir, speed, 0x1, interval, true, true))
			{
				TillerMan?.Say(501429); // Aye aye sir.

				return true;
			}

			return false;
		}

		public void BeginRename(Mobile from)
		{
			if (CheckDecay())
			{
				return;
			}

			if (from.AccessLevel < AccessLevel.GameMaster && from != Owner)
			{
				TillerMan?.Say(Utility.Random(1042876, 4)); // Arr, don't do that! | Arr, leave me alone! | Arr, watch what thour'rt doing, matey! | Arr! Do that again and I’ll throw ye overhead!

				return;
			}

			TillerMan?.Say(502580); // What dost thou wish to name thy ship?

			from.Prompt = new RenameBoatPrompt(this);
		}

		public void EndRename(Mobile from, string newName)
		{
			if (Deleted || CheckDecay())
			{
				return;
			}

			if (from.AccessLevel < AccessLevel.GameMaster && from != Owner)
			{
				TillerMan?.Say(1042880); // Arr! Only the owner of the ship may change its name!

				return;
			}
			else if (!from.Alive)
			{
				TillerMan?.Say(502582); // You appear to be dead.

				return;
			}

			newName = newName.Trim();

			if (newName.Length == 0)
			{
				newName = null;
			}

			Rename(newName);
		}

		public enum DryDockResult { Valid, Dead, NoKey, NotAnchored, Mobiles, Items, Hold, Decaying }

		public DryDockResult CheckDryDock(Mobile from)
		{
			if (CheckDecay())
			{
				return DryDockResult.Decaying;
			}

			if (!from.Alive)
			{
				return DryDockResult.Dead;
			}

			var pack = from.Backpack;
			if ((SPlank == null || !Key.ContainsKey(pack, SPlank.KeyValue)) && (PPlank == null || !Key.ContainsKey(pack, PPlank.KeyValue)))
			{
				return DryDockResult.NoKey;
			}

			if (!Anchored)
			{
				return DryDockResult.NotAnchored;
			}

			if (Hold != null && Hold.Items.Count > 0)
			{
				return DryDockResult.Hold;
			}

			var map = Map;

			if (map == null || map == Map.Internal)
			{
				return DryDockResult.Items;
			}

			var ents = GetMovingEntities();

			if (ents.Count >= 1)
			{
				return (ents[0] is Mobile) ? DryDockResult.Mobiles : DryDockResult.Items;
			}

			return DryDockResult.Valid;
		}

		public void BeginDryDock(Mobile from)
		{
			if (CheckDecay())
			{
				return;
			}

			var result = CheckDryDock(from);

			if (result == DryDockResult.Dead)
			{
				from.SendLocalizedMessage(502493); // You appear to be dead.
			}
			else if (result == DryDockResult.NoKey)
			{
				from.SendLocalizedMessage(502494); // You must have a key to the ship to dock the boat.
			}
			else if (result == DryDockResult.NotAnchored)
			{
				from.SendLocalizedMessage(1010570); // You must lower the anchor to dock the boat.
			}
			else if (result == DryDockResult.Mobiles)
			{
				from.SendLocalizedMessage(502495); // You cannot dock the ship with beings on board!
			}
			else if (result == DryDockResult.Items)
			{
				from.SendLocalizedMessage(502496); // You cannot dock the ship with a cluttered deck.
			}
			else if (result == DryDockResult.Hold)
			{
				from.SendLocalizedMessage(502497); // Make sure your hold is empty, and try again!
			}
			else if (result == DryDockResult.Valid)
			{
				_ = from.SendGump(new ConfirmDryDockGump(from, this));
			}
		}

		public void EndDryDock(Mobile from)
		{
			if (Deleted || CheckDecay())
			{
				return;
			}

			var result = CheckDryDock(from);

			if (result == DryDockResult.Dead)
			{
				from.SendLocalizedMessage(502493); // You appear to be dead.
			}
			else if (result == DryDockResult.NoKey)
			{
				from.SendLocalizedMessage(502494); // You must have a key to the ship to dock the boat.
			}
			else if (result == DryDockResult.NotAnchored)
			{
				from.SendLocalizedMessage(1010570); // You must lower the anchor to dock the boat.
			}
			else if (result == DryDockResult.Mobiles)
			{
				from.SendLocalizedMessage(502495); // You cannot dock the ship with beings on board!
			}
			else if (result == DryDockResult.Items)
			{
				from.SendLocalizedMessage(502496); // You cannot dock the ship with a cluttered deck.
			}
			else if (result == DryDockResult.Hold)
			{
				from.SendLocalizedMessage(502497); // Make sure your hold is empty, and try again!
			}

			if (result != DryDockResult.Valid)
			{
				return;
			}

			var boat = DockedBoat;

			if (boat == null)
			{
				return;
			}

			RemoveKeys(from);

			_ = from.AddToBackpack(boat);
			Delete();
		}

		public void SetName(SpeechEventArgs e)
		{
			if (CheckDecay())
			{
				return;
			}

			if (e.Mobile.AccessLevel < AccessLevel.GameMaster && e.Mobile != Owner)
			{
				TillerMan?.Say(1042880); // Arr! Only the owner of the ship may change its name!

				return;
			}
			else if (!e.Mobile.Alive)
			{
				TillerMan?.Say(502582); // You appear to be dead.

				return;
			}

			if (e.Speech.Length > 8)
			{
				var newName = e.Speech.Substring(8).Trim();

				if (newName.Length == 0)
				{
					newName = null;
				}

				Rename(newName);
			}
		}

		public void Rename(string newName)
		{
			if (CheckDecay())
			{
				return;
			}

			if (newName != null && newName.Length > 40)
			{
				newName = newName.Substring(0, 40);
			}

			if (m_ShipName == newName)
			{
				TillerMan?.Say(502531); // Yes, sir.

				return;
			}

			ShipName = newName;

			if (TillerMan != null && m_ShipName != null)
			{
				TillerMan.Say(1042885, m_ShipName); // This ship is now called the ~1_NEW_SHIP_NAME~.
			}
			else
			{
				TillerMan?.Say(502534); // This ship now has no name.
			}
		}

		public void RemoveName(Mobile m)
		{
			if (CheckDecay())
			{
				return;
			}

			if (m.AccessLevel < AccessLevel.GameMaster && m != Owner)
			{
				TillerMan?.Say(1042880); // Arr! Only the owner of the ship may change its name!

				return;
			}
			else if (!m.Alive)
			{
				TillerMan?.Say(502582); // You appear to be dead.

				return;
			}

			if (m_ShipName == null)
			{
				TillerMan?.Say(502526); // Ar, this ship has no name.

				return;
			}

			ShipName = null;

			TillerMan?.Say(502534); // This ship now has no name.
		}

		public void GiveName(Mobile m)
		{
			if (TillerMan == null || CheckDecay())
			{
				return;
			}

			if (m_ShipName == null)
			{
				TillerMan.Say(502526); // Ar, this ship has no name.
			}
			else
			{
				TillerMan.Say(1042881, m_ShipName); // This is the ~1_BOAT_NAME~.
			}
		}

		public void GiveNavPoint()
		{
			if (TillerMan == null || CheckDecay())
			{
				return;
			}

			if (NextNavPoint < 0)
			{
				TillerMan.Say(1042882); // I have no current nav point.
			}
			else
			{
				TillerMan.Say(1042883, (NextNavPoint + 1).ToString()); // My current destination navpoint is nav ~1_NAV_POINT_NUM~.
			}
		}

		public void AssociateMap(MapItem map)
		{
			if (CheckDecay())
			{
				return;
			}

			if (map is BlankMap)
			{
				TillerMan?.Say(502575); // Ar, that is not a map, tis but a blank piece of paper!
			}
			else if (map.Pins.Count == 0)
			{
				TillerMan?.Say(502576); // Arrrr, this map has no course on it!
			}
			else
			{
				_ = StopMove(false);

				MapItem = map;
				NextNavPoint = -1;

				TillerMan?.Say(502577); // A map!
			}
		}

		public bool StartCourse(string navPoint, bool single, bool message)
		{
			var number = -1;

			var start = -1;
			for (var i = 0; i < navPoint.Length; i++)
			{
				if (Char.IsDigit(navPoint[i]))
				{
					start = i;
					break;
				}
			}

			if (start != -1)
			{
				var sNumber = navPoint.Substring(start);

				if (!Int32.TryParse(sNumber, out number))
				{
					number = -1;
				}

				if (number != -1)
				{
					number--;

					if (MapItem == null || number < 0 || number >= MapItem.Pins.Count)
					{
						number = -1;
					}
				}
			}

			if (number == -1)
			{
				if (message && TillerMan != null)
				{
					TillerMan.Say(1042551); // I don't see that navpoint, sir.
				}

				return false;
			}

			NextNavPoint = number;
			return StartCourse(single, message);
		}

		public bool StartCourse(bool single, bool message)
		{
			if (CheckDecay())
			{
				return false;
			}

			if (Anchored)
			{
				if (message && TillerMan != null)
				{
					TillerMan.Say(501419); // Ar, the anchor is down sir!
				}

				return false;
			}
			else if (MapItem == null || MapItem.Deleted)
			{
				if (message && TillerMan != null)
				{
					TillerMan.Say(502513); // I have seen no map, sir.
				}

				return false;
			}
			else if (Map != MapItem.Map || !Contains(MapItem.GetWorldLocation()))
			{
				if (message && TillerMan != null)
				{
					TillerMan.Say(502514); // The map is too far away from me, sir.
				}

				return false;
			}
			else if ((Map != Map.Trammel && Map != Map.Felucca) || NextNavPoint < 0 || NextNavPoint >= MapItem.Pins.Count)
			{
				if (message && TillerMan != null)
				{
					TillerMan.Say(1042551); // I don't see that navpoint, sir.
				}

				return false;
			}

			Speed = FastSpeed;
			Order = single ? BoatOrder.Single : BoatOrder.Course;

			m_MoveTimer?.Stop();

			m_MoveTimer = new MoveTimer(this, FastInterval, false);
			m_MoveTimer.Start();

			if (message && TillerMan != null)
			{
				TillerMan.Say(501429); // Aye aye sir.
			}

			return true;
		}

		public override bool HandlesOnSpeech => true;

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (CheckDecay())
			{
				return;
			}

			var from = e.Mobile;

			if (CanCommand(from) && Contains(from))
			{
				for (var i = 0; i < e.Keywords.Length; ++i)
				{
					var keyword = e.Keywords[i];

					if (keyword is >= 0x42 and <= 0x6B)
					{
						switch (keyword)
						{
							case 0x42: SetName(e); break;
							case 0x43: RemoveName(e.Mobile); break;
							case 0x44: GiveName(e.Mobile); break;
							case 0x45: _ = StartMove(Forward, true); break;
							case 0x46: _ = StartMove(Backward, true); break;
							case 0x47: _ = StartMove(Left, true); break;
							case 0x48: _ = StartMove(Right, true); break;
							case 0x4B: _ = StartMove(ForwardLeft, true); break;
							case 0x4C: _ = StartMove(ForwardRight, true); break;
							case 0x4D: _ = StartMove(BackwardLeft, true); break;
							case 0x4E: _ = StartMove(BackwardRight, true); break;
							case 0x4F: _ = StopMove(true); break;
							case 0x50: _ = StartMove(Left, false); break;
							case 0x51: _ = StartMove(Right, false); break;
							case 0x52: _ = StartMove(Forward, false); break;
							case 0x53: _ = StartMove(Backward, false); break;
							case 0x54: _ = StartMove(ForwardLeft, false); break;
							case 0x55: _ = StartMove(ForwardRight, false); break;
							case 0x56: _ = StartMove(BackwardRight, false); break;
							case 0x57: _ = StartMove(BackwardLeft, false); break;
							case 0x58: _ = OneMove(Left); break;
							case 0x59: _ = OneMove(Right); break;
							case 0x5A: _ = OneMove(Forward); break;
							case 0x5B: _ = OneMove(Backward); break;
							case 0x5C: _ = OneMove(ForwardLeft); break;
							case 0x5D: _ = OneMove(ForwardRight); break;
							case 0x5E: _ = OneMove(BackwardRight); break;
							case 0x5F: _ = OneMove(BackwardLeft); break;
							case 0x49: case 0x65: _ = StartTurn(2, true); break; // turn right
							case 0x4A: case 0x66: _ = StartTurn(-2, true); break; // turn left
							case 0x67: _ = StartTurn(-4, true); break; // turn around, come about
							case 0x68: _ = StartMove(Forward, true); break;
							case 0x69: _ = StopMove(true); break;
							case 0x6A: _ = LowerAnchor(true); break;
							case 0x6B: _ = RaiseAnchor(true); break;
							case 0x60: GiveNavPoint(); break; // nav
							case 0x61: NextNavPoint = 0; _ = StartCourse(false, true); break; // start
							case 0x62: _ = StartCourse(false, true); break; // continue
							case 0x63: _ = StartCourse(e.Speech, false, true); break; // goto*
							case 0x64: _ = StartCourse(e.Speech, true, true); break; // single*
						}

						break;
					}
				}
			}
		}

		public bool StartTurn(int offset, bool message)
		{
			if (CheckDecay())
			{
				return false;
			}

			if (Anchored)
			{
				if (message)
				{
					TillerMan.Say(501419); // Ar, the anchor is down sir!
				}

				return false;
			}
			else
			{
				if (m_MoveTimer != null && Order != BoatOrder.Move)
				{
					m_MoveTimer.Stop();
					m_MoveTimer = null;
				}

				m_TurnTimer?.Stop();

				m_TurnTimer = new TurnTimer(this, offset);
				m_TurnTimer.Start();

				if (message && TillerMan != null)
				{
					TillerMan.Say(501429); // Aye aye sir.
				}

				return true;
			}
		}

		public bool Turn(int offset, bool message)
		{
			if (m_TurnTimer != null)
			{
				m_TurnTimer.Stop();
				m_TurnTimer = null;
			}

			if (CheckDecay())
			{
				return false;
			}

			if (Anchored)
			{
				if (message)
				{
					TillerMan.Say(501419); // Ar, the anchor is down sir!
				}

				return false;
			}
			else if (SetFacing((Direction)(((int)m_Facing + offset) & 0x7)))
			{
				return true;
			}
			else
			{
				if (message)
				{
					TillerMan.Say(501423); // Ar, can't turn sir.
				}

				return false;
			}
		}

		private class TurnTimer : Timer
		{
			private readonly BaseBoat m_Boat;
			private readonly int m_Offset;

			public TurnTimer(BaseBoat boat, int offset) : base(TimeSpan.FromSeconds(0.5))
			{
				m_Boat = boat;
				m_Offset = offset;

				Priority = TimerPriority.TenMS;
			}

			protected override void OnTick()
			{
				if (!m_Boat.Deleted)
				{
					_ = m_Boat.Turn(m_Offset, true);
				}
			}
		}

		public bool StartMove(Direction dir, int speed, int clientSpeed, TimeSpan interval, bool single, bool message)
		{
			if (CheckDecay())
			{
				return false;
			}

			if (Anchored)
			{
				if (message && TillerMan != null)
				{
					TillerMan.Say(501419); // Ar, the anchor is down sir!
				}

				return false;
			}

			Moving = dir;
			Speed = speed;
			m_ClientSpeed = clientSpeed;
			Order = BoatOrder.Move;

			m_MoveTimer?.Stop();

			m_MoveTimer = new MoveTimer(this, interval, single);
			m_MoveTimer.Start();

			return true;
		}

		public bool StopMove(bool message)
		{
			if (CheckDecay())
			{
				return false;
			}

			if (m_MoveTimer == null)
			{
				if (message && TillerMan != null)
				{
					TillerMan.Say(501443); // Er, the ship is not moving sir.
				}

				return false;
			}

			Moving = Direction.North;
			Speed = 0;
			m_ClientSpeed = 0;
			m_MoveTimer.Stop();
			m_MoveTimer = null;

			if (message && TillerMan != null)
			{
				TillerMan.Say(501429); // Aye aye sir.
			}

			return true;
		}

		private static readonly Range[][] m_WaterTiles =
		{
			new[] // land 
			{
				168..171,
				310..311,
			},
			new[] // statics
			{
				6038..6066,
			}
		};

		private static bool IsWater(LandTile tile)
		{
			foreach (var range in m_WaterTiles[0])
			{
				if (tile.ID >= range.Start.Value && tile.ID <= range.End.Value)
				{
					return true;
				}
			}

			return false;
		}

		private static bool IsWater(StaticTile tile)
		{
			foreach (var range in m_WaterTiles[1])
			{
				if (tile.ID >= range.Start.Value && tile.ID <= range.End.Value)
				{
					return true;
				}
			}

			return false;
		}

		private static readonly bool DebugCanFit = false;

		public virtual int BoatHeight => 100;

		public bool CanFit(Point3D p, Map map, int itemID)
		{
			if (map == null || map == Map.Internal || Deleted || CheckDecay())
			{
				return false;
			}

			var newComponents = MultiData.GetComponents(itemID);

			for (var x = 0; x < newComponents.Width; ++x)
			{
				for (var y = 0; y < newComponents.Height; ++y)
				{
					var tx = p.X + newComponents.Min.X + x;
					var ty = p.Y + newComponents.Min.Y + y;

					if (tx >= 0 && tx < newComponents.Width && ty >= 0 && ty < newComponents.Height && newComponents.Tiles[tx][ty].Length == 0)
					{
						if (DebugCanFit)
						{
							Effects.SendLocationEffect(new Point3D(tx, ty, p.Z), map, 0x42CF, 30, 85, 0);
						}

						continue;
					}

					if (Contains(tx, ty))
					{
						if (DebugCanFit)
						{
							Effects.SendLocationEffect(new Point3D(tx, ty, p.Z), map, 0x42CF, 30, 85, 0);
						}

						continue;
					}

					var hasWater = false;

					var landTile = map.Tiles.GetLandTile(tx, ty);

					if (!landTile.Ignored && IsWater(landTile) && Geometry.Intersects(p.Z, BoatHeight, landTile))
					{
						hasWater = true;
					}

					var tiles = map.Tiles.GetStaticTiles(tx, ty, true);

					for (var i = 0; i < tiles.Length; ++i)
					{
						var tile = tiles[i];

						if (IsWater(tile))
						{
							if (!hasWater && Geometry.Intersects(p.Z, BoatHeight, tile))
							{
								hasWater = true;
							}
						}
						else
						{
							if (Geometry.Intersects(p.Z, BoatHeight, tile))
							{
								if (DebugCanFit)
								{
									Effects.SendLocationEffect(new Point3D(tx, ty, p.Z), map, 0x42CF, 30, 22, 0);
								}

								return false;
							}
						}
					}

					if (!hasWater)
					{
						if (DebugCanFit)
						{
							Effects.SendLocationEffect(new Point3D(tx, ty, p.Z), map, 0x42CF, 30, 22, 0);
						}

						return false;
					}
				}
			}

			var eable = map.GetItemsInBounds(new Rectangle2D(p.X + newComponents.Min.X, p.Y + newComponents.Min.Y, newComponents.Width, newComponents.Height));

			try
			{
				foreach (var item in eable)
				{
					if (item is BaseMulti || item.ItemID > TileData.MaxItemValue || !item.Visible)
					{
						if (DebugCanFit)
						{
							Effects.SendLocationEffect(item, map, 0x42CF, 30, 85, 0);
						}

						continue;
					}

					var tx = item.X - (p.X + newComponents.Min.X);
					var ty = item.Y - (p.Y + newComponents.Min.Y);

					if (tx >= 0 && tx < newComponents.Width && ty >= 0 && ty < newComponents.Height && newComponents.Tiles[tx][ty].Length == 0)
					{
						if (DebugCanFit)
						{
							Effects.SendLocationEffect(item, map, 0x42CF, 30, 85, 0);
						}

						continue;
					}

					if (Contains(item))
					{
						if (DebugCanFit)
						{
							Effects.SendLocationEffect(item, map, 0x42CF, 30, 85, 0);
						}

						continue;
					}

					if (Geometry.Intersects(p.Z, BoatHeight, item))
					{
						if (DebugCanFit)
						{
							Effects.SendLocationEffect(item, map, 0x42CF, 30, 22, 0);
						}

						return false;
					}
				}
			}
			finally
			{
				eable.Free();
			}

			return true;
		}

		public Point3D Rotate(Point3D p, int count)
		{
			var rx = p.X - X;
			var ry = p.Y - Y;

			for (var i = 0; i < count; ++i)
			{
				var temp = rx;
				rx = -ry;
				ry = temp;
			}

			return new Point3D(X + rx, Y + ry, p.Z);
		}

		public override bool Contains(int x, int y)
		{
			var mcl = Components;

			var tx = x - (X + mcl.Min.X);
			var ty = y - (Y + mcl.Min.Y);

			if (tx >= 0 && tx < mcl.Width && ty >= 0 && ty < mcl.Height && mcl.Tiles[tx][ty].Length > 0)
			{
				return true;
			}

			if (TillerMan?.AtPoint(x, y) == true)
			{
				return true;
			}

			if (Hold?.AtPoint(x, y) == true)
			{
				return true;
			}

			if (PPlank?.AtPoint(x, y) == true)
			{
				return true;
			}

			if (SPlank?.AtPoint(x, y) == true)
			{
				return true;
			}

			return false;
		}

		public override bool Contains(int x, int y, int z)
		{
			var mcl = Components;

			var tx = x - (X + mcl.Min.X);
			var ty = y - (Y + mcl.Min.Y);
			var tz = z - (Z + mcl.Min.Z);

			if (tx >= 0 && tx < mcl.Width && ty >= 0 && ty < mcl.Height && tz >= 0 && tz < mcl.Depth && mcl.Tiles[tx][ty].Length > 0)
			{
				return true;
			}

			if (Geometry.Intersects(x, y, z, 0, TillerMan))
			{
				return true;
			}

			if (Geometry.Intersects(x, y, z, 0, Hold))
			{
				return true;
			}

			if (Geometry.Intersects(x, y, z, 0, PPlank))
			{
				return true;
			}

			if (Geometry.Intersects(x, y, z, 0, SPlank))
			{
				return true;
			}

			return false;
		}

		public static bool IsValidLocation(Point3D p, Map map)
		{
			return map?.Wrap.Contains(p) == true;
		}

		public Direction GetMovementFor(int x, int y, out int maxSpeed)
		{
			var dx = x - X;
			var dy = y - Y;

			var adx = Math.Abs(dx);
			var ady = Math.Abs(dy);

			var dir = Utility.GetDirection(this, new Point2D(x, y));
			var iDir = (int)dir;

			// Compute the maximum distance we can travel without going too far away
			if (iDir % 2 == 0) // North, East, South and West
			{
				maxSpeed = Math.Abs(adx - ady);
			}
			else // Right, Down, Left and Up
			{
				maxSpeed = Math.Min(adx, ady);
			}

			return (Direction)((iDir - (int)Facing) & 0x7);
		}

		public bool DoMovement(bool message)
		{
			if (Order == BoatOrder.Move)
			{
				return Move(Moving, Speed, m_ClientSpeed, true);
			}

			if (MapItem == null || MapItem.Deleted)
			{
				if (message)
				{
					TillerMan?.Say(502513); // I have seen no map, sir.
				}

				return false;
			}

			if (Map != MapItem.Map || !Contains(MapItem.WorldLocation))
			{
				if (message)
				{
					TillerMan?.Say(502514); // The map is too far away from me, sir.
				}

				return false;
			}

			if ((Map != Map.Trammel && Map != Map.Felucca) || NextNavPoint < 0 || NextNavPoint >= MapItem.Pins.Count)
			{
				if (message)
				{
					TillerMan?.Say(1042551); // I don't see that navpoint, sir.
				}

				return false;
			}

			var dest = MapItem.Pins[NextNavPoint];

			MapItem.ConvertToWorld(dest.X, dest.Y, out var x, out var y);

			var dir = GetMovementFor(x, y, out var maxSpeed);

			if (maxSpeed == 0)
			{
				if (message && Order == BoatOrder.Single)
				{
					TillerMan?.Say(1042874, (NextNavPoint + 1).ToString()); // We have arrived at nav point ~1_POINT_NUM~ , sir.
				}

				if (NextNavPoint + 1 < MapItem.Pins.Count)
				{
					NextNavPoint++;

					if (Order == BoatOrder.Course)
					{
						if (message)
						{
							TillerMan?.Say(1042875, (NextNavPoint + 1).ToString()); // Heading to nav point ~1_POINT_NUM~, sir.
						}

						return true;
					}

					return false;
				}

				NextNavPoint = -1;

				if (message && Order == BoatOrder.Course)
				{
					TillerMan?.Say(502515); // The course is completed, sir.
				}

				return false;
			}

			if (dir == Left || dir == BackwardLeft || dir == Backward)
			{
				return Turn(-2, true);
			}

			if (dir == Right || dir == BackwardRight)
			{
				return Turn(2, true);
			}

			var speed = Math.Min(Speed, maxSpeed);

			return Move(dir, speed, 4, true);
		}

		public bool Move(Direction dir, int speed, int clientSpeed, bool message)
		{
			var map = Map;

			if (map == null || Deleted || CheckDecay())
			{
				return false;
			}

			if (Anchored)
			{
				if (message)
				{
					TillerMan?.Say(501419); // Ar, the anchor is down sir!
				}

				return false;
			}

			int rx = 0, ry = 0;
			var d = (Direction)(((int)m_Facing + (int)dir) & 0x7);

			Movement.Movement.Offset(d, ref rx, ref ry);

			for (var i = 1; i <= speed; ++i)
			{
				var loc = new Point3D(X + (i * rx), Y + (i * ry), Z);

				if (!CanFit(loc, map, ItemID) || !Region.CanVehicleMove(this, Owner, d, loc, Location, map))
				{
					speed = i - 1;
					break;
				}
			}

			if (speed <= 0)
			{
				if (message)
				{
					TillerMan?.Say(501424); // Ar, we've stopped sir.
				}

				return false;
			}

			var xOffset = speed * rx;
			var yOffset = speed * ry;
			var zOffset = 0;

			var newX = X + xOffset;
			var newY = Y + yOffset;
			var newZ = Z + zOffset;

			foreach (var rect in map.Wrap)
			{
				if (rect.Contains(this) && !rect.Contains(new Point2D(newX, newY)))
				{
					if (newX < rect.X)
					{
						newX = rect.X + rect.Width - 1;
					}
					else if (newX >= rect.X + rect.Width)
					{
						newX = rect.X;
					}

					if (newY < rect.Y)
					{
						newY = rect.Y + rect.Height - 1;
					}
					else if (newY >= rect.Y + rect.Height)
					{
						newY = rect.Y;
					}

					for (var j = 1; j <= speed; ++j)
					{
						var loc = new Point3D(newX + (j * rx), newY + (j * ry), newZ);

						if (!CanFit(loc, map, ItemID) || !Region.CanVehicleMove(this, Owner, d, loc, Location, map))
						{
							speed = j - 1;
							break;
						}
					}

					if (speed <= 0)
					{
						if (message)
						{
							TillerMan?.Say(501424); // Ar, we've stopped sir.
						}

						return false;
					}

					xOffset = newX - X;
					yOffset = newY - Y;
					zOffset = newZ - Z;
				}
			}

			Sail(xOffset, yOffset, zOffset, d, clientSpeed);

			return true;
		}

		private static void SafeAdd(Item item, List<IEntity> toMove)
		{
			if (item != null)
			{
				toMove.Add(item);
			}
		}

		protected void Sail(int xOffset, int yOffset, int zOffset, Direction d, int clientSpeed)
		{
			if (!NewBoatMovement || Math.Abs(xOffset) > 1 || Math.Abs(yOffset) > 1)
			{
				Teleport(xOffset, yOffset, zOffset);
				return;
			}

			var newLoc = new Point3D(X + xOffset, Y + yOffset, Z + zOffset);

			var toMove = GetMovingEntities();

			SafeAdd(TillerMan, toMove);
			SafeAdd(Hold, toMove);
			SafeAdd(PPlank, toMove);
			SafeAdd(SPlank, toMove);

			// Packet must be sent before actual locations are changed
			foreach (var ns in Map.GetClientsInRange(Location, GetMaxUpdateRange()))
			{
				var m = ns.Mobile;

				if (ns.HighSeas && m.CanSee(this) && m.InRange(newLoc, GetUpdateRange(m)))
				{
					ns.Send(new MoveBoatHS(m, this, d, clientSpeed, toMove, xOffset, yOffset, zOffset));
				}
			}

			foreach (var e in toMove)
			{
				e.NoMoveHS = true;

				if (e is TillerMan || e is Hold || e is Plank)
				{
					continue;
				}

				e.Location = new Point3D(e.X + xOffset, e.Y + yOffset, e.Z + zOffset);
			}

			NoMoveHS = true;
			Location = newLoc;

			foreach (var e in toMove)
			{
				e.NoMoveHS = false;
			}

			NoMoveHS = false;
		}

		protected void Teleport(int xOffset, int yOffset, int zOffset)
		{
			var newLoc = new Point3D(X + xOffset, Y + yOffset, Z + zOffset);

			var toMove = GetMovingEntities();

			for (var i = 0; i < toMove.Count; ++i)
			{
				var e = toMove[i];

				e.Location = new Point3D(e.X + xOffset, e.Y + yOffset, e.Z + zOffset);
			}

			Location = newLoc;
		}

		protected void Teleport(int xOffset, int yOffset, int zOffset, Map mapTo)
		{
			var newLoc = new Point3D(X + xOffset, Y + yOffset, Z + zOffset);

			var mcl = Components;

			var toMove = new List<IEntity>();

			var eable = Map.GetObjectsInBounds(new Rectangle2D(X + mcl.Min.X, Y + mcl.Min.Y, mcl.Width, mcl.Height));

			foreach (var o in eable)
			{
				if (o == this || o is TillerMan || o is Hold || o is Plank)
				{
					continue;
				}

				toMove.Add(o);
			}

			eable.Free();

			for (var i = 0; i < toMove.Count; ++i)
			{
				var e = toMove[i];

				if (Contains(e))
				{
					if (e is Item item && (!item.Visible || item.Z < Z))
					{
						continue;
					}

					e.Location = new Point3D(e.X + xOffset, e.Y + yOffset, e.Z + zOffset);
					e.Map = mapTo;
				}
			}

			Location = newLoc;
			Map = mapTo;
		}

		public override void MoveToWorld(Point3D location, Map map)
		{
			if (Map == null || Map == Map.Internal || map == null || map == Map.Internal)
			{
				base.MoveToWorld(location, map);

				return;
			}

			var xOffset = location.X - X;
			var yOffset = location.Y - Y;
			var zOffset = location.Z - Z;

			if (Map != map)
			{
				Teleport(xOffset, yOffset, zOffset, map);
			}
			else
			{
				Sail(xOffset, yOffset, zOffset, Moving & Direction.Mask, m_ClientSpeed);
			}
		}

		public List<IEntity> GetMovingEntities()
		{
			var list = new List<IEntity>();

			var map = Map;

			if (map == null || map == Map.Internal)
			{
				return list;
			}

			var mcl = Components;

			foreach (var o in map.GetObjectsInBounds(new Rectangle2D(X + mcl.Min.X, Y + mcl.Min.Y, mcl.Width, mcl.Height)))
			{
				if (o == this || o is TillerMan || o is Hold || o is Plank)
				{
					continue;
				}

				if (Contains(o))
				{
					if (o is Item item && (!item.Visible || item.Z < Z))
					{
						continue;
					}

					list.Add(o);
				}
			}

			return list;
		}

		public bool SetFacing(Direction facing)
		{
			if (Parent != null || Map == null)
			{
				return false;
			}

			if (CheckDecay())
			{
				return false;
			}

			if (Map != Map.Internal)
			{
				switch (facing)
				{
					case Direction.North:
						if (!CanFit(Location, Map, NorthID))
						{
							return false;
						}

						break;
					case Direction.East:
						if (!CanFit(Location, Map, EastID))
						{
							return false;
						}

						break;
					case Direction.South:
						if (!CanFit(Location, Map, SouthID))
						{
							return false;
						}

						break;
					case Direction.West:
						if (!CanFit(Location, Map, WestID))
						{
							return false;
						}

						break;
				}
			}

			var old = m_Facing;

			m_Facing = facing;

			TillerMan?.SetFacing(facing);

			Hold?.SetFacing(facing);

			PPlank?.SetFacing(facing);

			SPlank?.SetFacing(facing);

			var toMove = GetMovingEntities();

			toMove.Add(PPlank);
			toMove.Add(SPlank);

			int xOffset = 0, yOffset = 0;
			Movement.Movement.Offset(facing, ref xOffset, ref yOffset);

			if (TillerMan != null)
			{
				TillerMan.Location = new Point3D(X + (xOffset * TillerManDistance) + (facing == Direction.North ? 1 : 0), Y + (yOffset * TillerManDistance), TillerMan.Z);
			}

			if (Hold != null)
			{
				Hold.Location = new Point3D(X + (xOffset * HoldDistance), Y + (yOffset * HoldDistance), Hold.Z);
			}

			var count = (m_Facing - old) & 0x7;
			count /= 2;

			for (var i = 0; i < toMove.Count; ++i)
			{
				var e = toMove[i];

				if (e is Item item)
				{
					item.Location = Rotate(item.Location, count);
				}
				else if (e is Mobile m)
				{
					m.Direction = (m.Direction - old + facing) & Direction.Mask;
					m.Location = Rotate(m.Location, count);
				}
			}

			switch (facing)
			{
				case Direction.North: ItemID = NorthID; break;
				case Direction.East: ItemID = EastID; break;
				case Direction.South: ItemID = SouthID; break;
				case Direction.West: ItemID = WestID; break;
			}

			return true;
		}

		private class MoveTimer : Timer
		{
			private readonly BaseBoat m_Boat;

			public MoveTimer(BaseBoat boat, TimeSpan interval, bool single) : base(interval, interval, single ? 1 : 0)
			{
				m_Boat = boat;
				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				if (!m_Boat.DoMovement(true))
				{
					_ = m_Boat.StopMove(false);
				}
			}
		}

		public static void UpdateAllComponents()
		{
			for (var i = Boats.Count - 1; i >= 0; --i)
			{
				Boats[i].UpdateComponents();
			}
		}

		public static void Initialize()
		{
			_ = Timer.DelayCall(UpdateAllComponents);

			EventSink.WorldSave += EventSink_WorldSave;
		}

		private static void EventSink_WorldSave(WorldSaveEventArgs e)
		{
			_ = Timer.DelayCall(UpdateAllComponents);
		}

		#region High Seas

		public override bool AllowsRelativeDrop => true;

		/*
		 * OSI sends the 0xF7 packet instead, holding 0xF3 packets
		 * for every entity on the boat. Though, the regular 0xF3
		 * packets are still being sent as well as entities come
		 * into sight. Do we really need it?
		 */
		/*
		protected override Packet GetWorldPacketFor( NetState state )
		{
			if ( NewBoatMovement && state.HighSeas )
				return new DisplayBoatHS( state.Mobile, this );
			else
				return base.GetWorldPacketFor( state );
		}
		*/

		public sealed class MoveBoatHS : Packet
		{
			public MoveBoatHS(Mobile beholder, BaseBoat boat, Direction d, int speed, List<IEntity> ents, int xOffset, int yOffset, int zOffset)
				: base(0xF6)
			{
				EnsureCapacity(3 + 15 + (ents.Count * 10));

				m_Stream.Write(boat.Serial);
				m_Stream.Write((byte)speed);
				m_Stream.Write((byte)d);
				m_Stream.Write((byte)boat.Facing);
				m_Stream.Write((short)(boat.X + xOffset));
				m_Stream.Write((short)(boat.Y + yOffset));
				m_Stream.Write((short)(boat.Z + zOffset));
				m_Stream.Write((short)0); // count placeholder

				var count = 0;

				foreach (var ent in ents)
				{
					if (!beholder.CanSee(ent))
					{
						continue;
					}

					m_Stream.Write(ent.Serial);
					m_Stream.Write((short)(ent.X + xOffset));
					m_Stream.Write((short)(ent.Y + yOffset));
					m_Stream.Write((short)(ent.Z + zOffset));
					++count;
				}

				_ = m_Stream.Seek(16, System.IO.SeekOrigin.Begin);
				m_Stream.Write((short)count);
			}
		}

		public sealed class DisplayBoatHS : Packet
		{
			public DisplayBoatHS(Mobile beholder, BaseBoat boat)
				: base(0xF7)
			{
				var ents = boat.GetMovingEntities();

				SafeAdd(boat.TillerMan, ents);
				SafeAdd(boat.Hold, ents);
				SafeAdd(boat.PPlank, ents);
				SafeAdd(boat.SPlank, ents);

				ents.Add(boat);

				EnsureCapacity(3 + 2 + (ents.Count * 26));

				m_Stream.Write((short)0); // count placeholder

				var count = 0;

				foreach (var ent in ents)
				{
					if (!beholder.CanSee(ent))
					{
						continue;
					}

					// Embedded WorldItemHS packets
					m_Stream.Write((byte)0xF3);
					m_Stream.Write((short)0x1);

					if (ent is BaseMulti bm)
					{
						m_Stream.Write((byte)0x02);
						m_Stream.Write(bm.Serial);
						// TODO: Mask no longer needed, merge with Item case?
						m_Stream.Write((ushort)(bm.ItemID & 0x3FFF));
						m_Stream.Write((byte)0);

						m_Stream.Write((short)bm.Amount);
						m_Stream.Write((short)bm.Amount);

						m_Stream.Write((short)(bm.X & 0x7FFF));
						m_Stream.Write((short)(bm.Y & 0x3FFF));
						m_Stream.Write((sbyte)bm.Z);

						m_Stream.Write((byte)bm.Light);
						m_Stream.Write((short)bm.Hue);
						m_Stream.Write((byte)bm.GetPacketFlags());
					}
					else if (ent is Mobile m)
					{
						m_Stream.Write((byte)0x01);
						m_Stream.Write(m.Serial);
						m_Stream.Write((short)m.Body);
						m_Stream.Write((byte)0);

						m_Stream.Write((short)1);
						m_Stream.Write((short)1);

						m_Stream.Write((short)(m.X & 0x7FFF));
						m_Stream.Write((short)(m.Y & 0x3FFF));
						m_Stream.Write((sbyte)m.Z);

						m_Stream.Write((byte)m.Direction);
						m_Stream.Write((short)m.Hue);
						m_Stream.Write((byte)m.GetPacketFlags());
					}
					else if (ent is Item item)
					{
						m_Stream.Write((byte)0x00);
						m_Stream.Write(item.Serial);
						m_Stream.Write((ushort)(item.ItemID & 0xFFFF));
						m_Stream.Write((byte)0);

						m_Stream.Write((short)item.Amount);
						m_Stream.Write((short)item.Amount);

						m_Stream.Write((short)(item.X & 0x7FFF));
						m_Stream.Write((short)(item.Y & 0x3FFF));
						m_Stream.Write((sbyte)item.Z);

						m_Stream.Write((byte)item.Light);
						m_Stream.Write((short)item.Hue);
						m_Stream.Write((byte)item.GetPacketFlags());
					}

					m_Stream.Write((short)0x00);
					++count;
				}

				_ = m_Stream.Seek(3, System.IO.SeekOrigin.Begin);
				m_Stream.Write((short)count);
			}
		}

		#endregion
	}

	public class RenameBoatPrompt : Prompt
	{
		private readonly BaseBoat m_Boat;

		public RenameBoatPrompt(BaseBoat boat)
		{
			m_Boat = boat;
		}

		public override void OnResponse(Mobile from, string text)
		{
			m_Boat.EndRename(from, text);
		}
	}

	public abstract class BaseBoatDeed : Item
	{
		private Point3D m_Offset;

		[CommandProperty(AccessLevel.GameMaster)]
		public int MultiID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D Offset { get => m_Offset; set => m_Offset = value; }

		public BaseBoatDeed(int id, Point3D offset) : base(0x14F2)
		{
			Weight = 1.0;

			if (!Core.AOS)
			{
				LootType = LootType.Newbied;
			}

			MultiID = id;
			m_Offset = offset;
		}

		public BaseBoatDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(MultiID);
			writer.Write(m_Offset);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						MultiID = reader.ReadInt();
						m_Offset = reader.ReadPoint3D();

						break;
					}
			}

			if (Weight == 0.0)
			{
				Weight = 1.0;
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
			else if (from.AccessLevel < AccessLevel.GameMaster && (from.Map == Map.Ilshenar || from.Map == Map.Malas))
			{
				from.SendLocalizedMessage(1010567, null, 0x25); // You may not place a boat from this location.
			}
			else
			{
				if (Core.SE)
				{
					from.SendLocalizedMessage(502482); // Where do you wish to place the ship?
				}
				else
				{
					from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502482); // Where do you wish to place the ship?
				}

				from.Target = new InternalTarget(this);
			}
		}

		public abstract BaseBoat Boat { get; }

		public void OnPlacement(Mobile from, Point3D p)
		{
			if (Deleted)
			{
				return;
			}
			else if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
			else
			{
				var map = from.Map;

				if (map == null)
				{
					return;
				}

				if (from.AccessLevel < AccessLevel.GameMaster && (map == Map.Ilshenar || map == Map.Malas))
				{
					from.SendLocalizedMessage(1043284); // A ship can not be created here.
					return;
				}

				if (from.Region.IsPartOf<HouseRegion>() || BaseBoat.FindBoatAt(from, from.Map) != null)
				{
					from.SendLocalizedMessage(1010568, null, 0x25); // You may not place a ship while on another ship or inside a house.
					return;
				}

				var boat = Boat;

				if (boat == null)
				{
					return;
				}

				p = new Point3D(p.X - m_Offset.X, p.Y - m_Offset.Y, p.Z - m_Offset.Z);

				var reg = Region.Find(p, Map);

				if (!reg.AllowVehicles(from, p))
				{
					boat.Delete();
				}
				else if (BaseBoat.IsValidLocation(p, map) && boat.CanFit(p, map, boat.ItemID))
				{
					Delete();

					boat.Owner = from;
					boat.Anchored = true;

					var keyValue = boat.CreateKeys(from);

					if (boat.PPlank != null)
					{
						boat.PPlank.KeyValue = keyValue;
					}

					if (boat.SPlank != null)
					{
						boat.SPlank.KeyValue = keyValue;
					}

					boat.MoveToWorld(p, map);
				}
				else
				{
					boat.Delete();
					from.SendLocalizedMessage(1043284); // A ship can not be created here.
				}
			}
		}

		private class InternalTarget : MultiTarget
		{
			private readonly BaseBoatDeed m_Deed;

			public InternalTarget(BaseBoatDeed deed) : base(deed.MultiID, deed.Offset)
			{
				m_Deed = deed;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is IPoint3D ip)
				{
					if (ip is Item item)
					{
						ip = item.GetWorldTop();
					}

					var p = new Point3D(ip);

					var region = Region.Find(p, from.Map);

					if (region.AllowVehicles(from, p))
					{
						m_Deed.OnPlacement(from, p);
					}
				}
			}
		}
	}

	public class Strandedness
	{
		private static readonly Point2D[] m_Felucca = new Point2D[]
		{
			new Point2D( 2528, 3568 ), new Point2D( 2376, 3400 ), new Point2D( 2528, 3896 ),
			new Point2D( 2168, 3904 ), new Point2D( 1136, 3416 ), new Point2D( 1432, 3648 ),
			new Point2D( 1416, 4000 ), new Point2D( 4512, 3936 ), new Point2D( 4440, 3120 ),
			new Point2D( 4192, 3672 ), new Point2D( 4720, 3472 ), new Point2D( 3744, 2768 ),
			new Point2D( 3480, 2432 ), new Point2D( 3560, 2136 ), new Point2D( 3792, 2112 ),
			new Point2D( 2800, 2296 ), new Point2D( 2736, 2016 ), new Point2D( 4576, 1456 ),
			new Point2D( 4680, 1152 ), new Point2D( 4304, 1104 ), new Point2D( 4496, 984 ),
			new Point2D( 4248, 696 ), new Point2D( 4040, 616 ), new Point2D( 3896, 248 ),
			new Point2D( 4176, 384 ), new Point2D( 3672, 1104 ), new Point2D( 3520, 1152 ),
			new Point2D( 3720, 1360 ), new Point2D( 2184, 2152 ), new Point2D( 1952, 2088 ),
			new Point2D( 2056, 1936 ), new Point2D( 1720, 1992 ), new Point2D( 472, 2064 ),
			new Point2D( 656, 2096 ), new Point2D( 3008, 3592 ), new Point2D( 2784, 3472 ),
			new Point2D( 5456, 2400 ), new Point2D( 5976, 2424 ), new Point2D( 5328, 3112 ),
			new Point2D( 5792, 3152 ), new Point2D( 2120, 3616 ), new Point2D( 2136, 3128 ),
			new Point2D( 1632, 3528 ), new Point2D( 1328, 3160 ), new Point2D( 1072, 3136 ),
			new Point2D( 1128, 2976 ), new Point2D( 960, 2576 ), new Point2D( 752, 1832 ),
			new Point2D( 184, 1488 ), new Point2D( 592, 1440 ), new Point2D( 368, 1216 ),
			new Point2D( 232, 752 ), new Point2D( 696, 744 ), new Point2D( 304, 1000 ),
			new Point2D( 840, 376 ), new Point2D( 1192, 624 ), new Point2D( 1200, 192 ),
			new Point2D( 1512, 240 ), new Point2D( 1336, 456 ), new Point2D( 1536, 648 ),
			new Point2D( 1104, 952 ), new Point2D( 1864, 264 ), new Point2D( 2136, 200 ),
			new Point2D( 2160, 528 ), new Point2D( 1904, 512 ), new Point2D( 2240, 784 ),
			new Point2D( 2536, 776 ), new Point2D( 2488, 216 ), new Point2D( 2336, 72 ),
			new Point2D( 2648, 288 ), new Point2D( 2680, 576 ), new Point2D( 2896, 88 ),
			new Point2D( 2840, 344 ), new Point2D( 3136, 72 ), new Point2D( 2968, 520 ),
			new Point2D( 3192, 328 ), new Point2D( 3448, 208 ), new Point2D( 3432, 608 ),
			new Point2D( 3184, 752 ), new Point2D( 2800, 704 ), new Point2D( 2768, 1016 ),
			new Point2D( 2448, 1232 ), new Point2D( 2272, 920 ), new Point2D( 2072, 1080 ),
			new Point2D( 2048, 1264 ), new Point2D( 1808, 1528 ), new Point2D( 1496, 1880 ),
			new Point2D( 1656, 2168 ), new Point2D( 2096, 2320 ), new Point2D( 1816, 2528 ),
			new Point2D( 1840, 2640 ), new Point2D( 1928, 2952 ), new Point2D( 2120, 2712 )
		};

		private static readonly Point2D[] m_Trammel = m_Felucca;

		private static readonly Point2D[] m_Ilshenar = new Point2D[]
		{
			new Point2D( 1252, 1180 ), new Point2D( 1562, 1090 ), new Point2D( 1444, 1016 ),
			new Point2D( 1324, 968 ), new Point2D( 1418, 806 ), new Point2D( 1722, 874 ),
			new Point2D( 1456, 684 ), new Point2D( 1036, 866 ), new Point2D( 612, 476 ),
			new Point2D( 1476, 372 ), new Point2D( 762, 472 ), new Point2D( 812, 1162 ),
			new Point2D( 1422, 1144 ), new Point2D( 1254, 1066 ), new Point2D( 1598, 870 ),
			new Point2D( 1358, 866 ), new Point2D( 510, 302 ), new Point2D( 510, 392 )
		};

		private static readonly Point2D[] m_Tokuno = new Point2D[]
		{
			//Makoto-Jima
			new Point2D( 837, 1351 ), new Point2D( 941, 1241 ), new Point2D( 959, 1185 ),
			new Point2D( 923, 1091 ), new Point2D( 904, 983 ), new Point2D( 845, 944 ),
			new Point2D( 829, 896 ), new Point2D( 794, 852 ), new Point2D( 766, 821 ),
			new Point2D( 695, 814 ), new Point2D( 576, 835 ), new Point2D( 518, 840 ),
			new Point2D( 519, 902 ), new Point2D( 502, 950 ), new Point2D( 503, 1045 ),
			new Point2D( 547, 1131 ), new Point2D( 518, 1204 ), new Point2D( 506, 1243 ),
			new Point2D( 526, 1271 ), new Point2D( 562, 1295 ), new Point2D( 616, 1335 ),
			new Point2D( 789, 1347 ), new Point2D( 712, 1359 ),

			//Homare-Jima
			new Point2D( 202, 498 ), new Point2D( 116, 600 ), new Point2D( 107, 699 ),
			new Point2D( 162, 799 ), new Point2D( 158, 889 ), new Point2D( 169, 989 ),
			new Point2D( 194, 1101 ), new Point2D( 250, 1163 ), new Point2D( 295, 1176 ),
			new Point2D( 280, 1194 ), new Point2D( 286, 1102 ), new Point2D( 250, 1000 ),
			new Point2D( 260, 906 ), new Point2D( 360, 838 ), new Point2D( 389, 763 ),
			new Point2D( 415, 662 ), new Point2D( 500, 597 ), new Point2D( 570, 572 ),
			new Point2D( 631, 577 ), new Point2D( 692, 500 ), new Point2D( 723, 445 ),
			new Point2D( 672, 379 ), new Point2D( 626, 332 ), new Point2D( 494, 291 ),
			new Point2D( 371, 336 ), new Point2D( 324, 334 ), new Point2D( 270, 362 ),

			//Isamu-Jima
			new Point2D( 1240, 1076 ), new Point2D( 1189, 1115 ), new Point2D( 1046, 1039 ),
			new Point2D( 1025, 885 ), new Point2D( 907, 809 ), new Point2D( 840, 506 ),
			new Point2D( 799, 396 ), new Point2D( 720, 258 ), new Point2D( 744, 158 ),
			new Point2D( 904, 37 ), new Point2D( 974, 91 ), new Point2D( 1020, 187 ),
			new Point2D( 1035, 288 ), new Point2D( 1104, 395 ), new Point2D( 1215, 462 ),
			new Point2D( 1275, 488 ), new Point2D( 1348, 611 ), new Point2D( 1363, 739 ),
			new Point2D( 1364, 765 ), new Point2D( 1364, 876 ), new Point2D( 1300, 936 ),
			new Point2D( 1240, 1003 )
		};

		public static void Initialize()
		{
			EventSink.Login += EventSink_Login;
		}

		private static bool IsStranded(Mobile from)
		{
			var map = from.Map;

			if (map == null)
			{
				return false;
			}

			if (map == Map.Felucca || map == Map.Trammel)
			{
				if (from.X >= 5189 && from.Y >= 696 && from.X <= 5192 && from.Y <= 698)
				{
					return false;
				}
			}

			var surface = map.GetTopSurface(from.Location);

			if (surface is LandTile lt)
			{
				var id = lt.ID;

				return id is (>= 168 and <= 171) or (>= 310 and <= 311);
			}

			if (surface is StaticTile st)
			{
				var id = st.ID;

				return id is >= 0x1796 and <= 0x17B2;
			}

			return false;
		}

		public static void EventSink_Login(LoginEventArgs e)
		{
			var from = e.Mobile;

			if (!IsStranded(from))
			{
				return;
			}

			var map = from.Map;

			Point2D[] list;

			if (map == Map.Felucca)
			{
				list = m_Felucca;
			}
			else if (map == Map.Trammel)
			{
				list = m_Trammel;
			}
			else if (map == Map.Ilshenar)
			{
				list = m_Ilshenar;
			}
			else if (map == Map.Tokuno)
			{
				list = m_Tokuno;
			}
			else
			{
				return;
			}

			var p = Point2D.Zero;
			var pdist = Double.MaxValue;

			for (var i = 0; i < list.Length; ++i)
			{
				var dist = from.GetDistanceToSqrt(list[i]);

				if (dist < pdist)
				{
					p = list[i];
					pdist = dist;
				}
			}

			int x = p.X, y = p.Y;
			var z = map.GetAverageZ(x, y);

			var canFit = map.CanSpawnMobile(x, y, z);

			for (var i = 1; !canFit && i <= 40; i += 2)
			{
				for (var xo = -1; !canFit && xo <= 1; ++xo)
				{
					for (var yo = -1; !canFit && yo <= 1; ++yo)
					{
						if (xo == 0 && yo == 0)
						{
							continue;
						}

						x = p.X + (xo * i);
						y = p.Y + (yo * i);
						z = map.GetAverageZ(x, y);
						canFit = map.CanSpawnMobile(x, y, z);
					}
				}
			}

			if (canFit)
			{
				from.Location = new Point3D(x, y, z);
			}
		}
	}
}