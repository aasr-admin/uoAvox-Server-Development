using Server.Mobiles;
using Server.Multis;

using System;

namespace Server.Regions
{
	[Flags]
	public enum TeleportFlags : ulong
	{
		None = 0ul,

		Active = 1ul << 0,
		Relative = 1ul << 1,
		Combat = 1ul << 2,

		Players = 1ul << 10,
		Creatures = 1ul << 11,
		Vehicles = 1ul << 12,

		All = ~None,

		DefaultRules = Relative | Players | Creatures,
	}

	[NoSort, PropertyObject]
	public abstract class BaseTeleportRules
	{
		public TeleportFlags Flags { get; set; } = TeleportFlags.DefaultRules;

		public bool this[TeleportFlags flags] { get => GetFlag(flags); set => SetFlag(flags, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool SetDefaults { get => false; set => Flags = value ? TeleportFlags.DefaultRules : Flags; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool SetAllTrue { get => false; set => Flags = value ? TeleportFlags.All : Flags; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool SetAllFalse { get => false; set => Flags = value ? TeleportFlags.None : Flags; }

		public virtual void Defaults()
		{
			Flags = TeleportFlags.DefaultRules;
		}

		public virtual bool GetFlag(TeleportFlags flags)
		{
			return Flags.HasFlag(flags);
		}

		public virtual void SetFlag(TeleportFlags flags, bool state)
		{
			if (state)
			{
				Flags |= flags;
			}
			else
			{
				Flags &= ~flags;
			}
		}

		public override string ToString()
		{
			return Flags.ToString();
		}
	}

	[NoSort, PropertyObject]
	public class TeleportRules : BaseTeleportRules
	{
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Active { get => GetFlag(TeleportFlags.Active); set => SetFlag(TeleportFlags.Active, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Relative { get => GetFlag(TeleportFlags.Relative); set => SetFlag(TeleportFlags.Relative, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Players { get => GetFlag(TeleportFlags.Players); set => SetFlag(TeleportFlags.Players, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Creatures { get => GetFlag(TeleportFlags.Creatures); set => SetFlag(TeleportFlags.Creatures, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Vehicles { get => GetFlag(TeleportFlags.Vehicles); set => SetFlag(TeleportFlags.Vehicles, value); }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Combat { get => GetFlag(TeleportFlags.Combat); set => SetFlag(TeleportFlags.Combat, value); }
	}

	public sealed class TeleportRegion : Region
	{
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Poly2D TeleportLocation { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Map TeleportMap { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public TeleportRules TeleportRules { get; private set; } = new();

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Active { get => TeleportRules.Active; set => TeleportRules.Active = value; }

		public TeleportRegion(string name, Map map, int priority, params Rectangle2D[] area) 
			: base(name, map, priority, area)
		{
		}

		public TeleportRegion(string name, Map map, int priority, params Poly2D[] area) 
			: base(name, map, priority, area)
		{
		}

		public TeleportRegion(string name, Map map, int priority, params Rectangle3D[] area) 
			: base(name, map, priority, area)
		{
		}

		public TeleportRegion(string name, Map map, int priority, params Poly3D[] area) 
			: base(name, map, priority, area)
		{
		}

		public TeleportRegion(string name, Map map, Region parent, params Rectangle2D[] area) 
			: base(name, map, parent, area)
		{
		}

		public TeleportRegion(string name, Map map, Region parent, params Poly2D[] area) 
			: base(name, map, parent, area)
		{
		}

		public TeleportRegion(string name, Map map, Region parent, params Rectangle3D[] area) 
			: base(name, map, parent, area)
		{
		}

		public TeleportRegion(string name, Map map, Region parent, params Poly3D[] area) 
			: base(name, map, parent, area)
		{
		}

		public TeleportRegion(RegionDefinition def, Map map, Region parent) 
			: base(def, map, parent)
		{
		}

		public TeleportRegion(int id) 
			: base(id)
		{
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			TeleportMap = Map;
			TeleportLocation = Poly2D.Empty;

			TeleportRules.Defaults();
		}

		public override void OnVehicleLocationChanged(Item vehicle, Point3D oldLocation)
		{
			var moved = false;

			try
			{
				if (!Active || !TeleportRules.Vehicles)
				{
					return;
				}

				if (TeleportMap == null || TeleportMap == Map.Internal || TeleportLocation == Rectangle2D.Empty)
				{
					return;
				}

				if (vehicle is BaseBoat b)
				{
					if (b.Owner?.Deleted != false)
					{
						return;
					}

					if (!TeleportRules.Combat && b.Owner.Combatant != null)
					{
						return;
					}
				}

				var attempts = 5;

				while (!moved && --attempts >= 0)
				{
					TeleportLocation.GetRandomPoint(out var x, out var y);

					var dest = TeleportMap.GetTopSurface(x, y);

					if (vehicle is BaseBoat boat)
					{
						if (!CanVehicleTransition(boat, boat.Owner, dest, TeleportMap))
						{
							continue;
						}

						if (!boat.CanFit(dest, TeleportMap, boat.ItemID))
						{
							continue;
						}
					}
					else
					{
						if (!TeleportMap.CanFit(dest, vehicle.ItemData.CalcHeight))
						{
							continue;
						}
					}

					vehicle.MoveToWorld(dest, TeleportMap);

					moved = true;
				}
			}
			finally
			{
				if (!moved)
				{
					base.OnVehicleLocationChanged(vehicle, oldLocation);
				}
			}
		}

		public override void OnLocationChanged(Mobile m, Point3D oldLocation)
		{
			var moved = false;

			try
			{
				if (!Active || !(m.Player ? TeleportRules.Players : TeleportRules.Creatures))
				{
					return;
				}

				if (TeleportMap == null || TeleportMap == Map.Internal || TeleportLocation == Rectangle2D.Empty)
				{
					return;
				}

				if (!TeleportRules.Combat && m.Combatant != null)
				{
					return;
				}

				var attempts = 5;

				while (!moved && --attempts >= 0)
				{
					TeleportLocation.GetRandomPoint(out var x, out var y);

					var dest = TeleportMap.GetTopSurface(x, y);

					if (!CanTransition(m, dest, TeleportMap))
					{
						continue;
					}

					if (m is BaseCreature c && c.Spawner == null && c.GetMaster() == null)
					{
						if (!TeleportMap.CanSpawnMobile(dest))
						{
							continue;
						}
					}
					else
					{
						if (!TeleportMap.CanFit(dest, 16))
						{
							continue;
						}
					}

					m.MoveToWorld(dest, TeleportMap);

					moved = true;
				}
			}
			finally
			{
				if (!moved)
				{
					base.OnLocationChanged(m, oldLocation);
				}
			}
		}
		
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(TeleportMap);
			writer.Write(TeleportLocation);

			var rules = TeleportRules.Flags;

			if (rules == TeleportFlags.DefaultRules)
			{
				writer.Write(true);
			}
			else
			{
				writer.Write(false);
				writer.Write(rules);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();

			TeleportMap = reader.ReadMap();
			TeleportLocation = reader.ReadPoly2D();

			if (reader.ReadBool())
			{
				TeleportRules.Defaults();
			}
			else
			{
				TeleportRules.Flags = reader.ReadEnum<TeleportFlags>();
			}
		}
	}
}