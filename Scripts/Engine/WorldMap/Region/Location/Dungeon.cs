namespace Server.Regions
{
	public class DungeonRegion : BaseRegion
	{
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Point3D EntranceLocation { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Map EntranceMap { get; set; }

		public override bool WeatherSupported => true;

		public DungeonRegion(string name, Map map, int priority, params Rectangle2D[] area) : base(name, map, priority, area)
		{
		}

		public DungeonRegion(string name, Map map, int priority, params Poly2D[] area) : base(name, map, priority, area)
		{
		}

		public DungeonRegion(string name, Map map, int priority, params Rectangle3D[] area) : base(name, map, priority, area)
		{
		}

		public DungeonRegion(string name, Map map, int priority, params Poly3D[] area) : base(name, map, priority, area)
		{
		}

		public DungeonRegion(string name, Map map, Region parent, params Rectangle2D[] area) : base(name, map, parent, area)
		{
		}

		public DungeonRegion(string name, Map map, Region parent, params Poly2D[] area) : base(name, map, parent, area)
		{
		}

		public DungeonRegion(string name, Map map, Region parent, params Rectangle3D[] area) : base(name, map, parent, area)
		{
		}

		public DungeonRegion(string name, Map map, Region parent, params Poly3D[] area) : base(name, map, parent, area)
		{
		}

		public DungeonRegion(RegionDefinition def, Map map, Region parent) : base(def, map, parent)
		{
		}

		public DungeonRegion(int id) : base(id)
		{
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			Rules.AllowHouses = false;
			Rules.AllowVehicles = false;
			Rules.AllowYoungAggro = true;
		}

		public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
		{
			base.AlterLightLevel(m, ref global, ref personal);

			global = LightCycle.DungeonLevel;
		}

		public override bool CanUseStuckMenu(Mobile m)
		{
			if (Map == Map.Felucca)
			{
				return false;
			}

			return base.CanUseStuckMenu(m);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(EntranceLocation);
			writer.Write(EntranceMap);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();

			EntranceLocation = reader.ReadPoint3D();
			EntranceMap = reader.ReadMap();
		}
	}
}