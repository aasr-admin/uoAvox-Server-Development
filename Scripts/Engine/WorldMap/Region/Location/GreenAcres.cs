namespace Server.Regions
{
	public sealed class GreenAcres : BaseRegion
	{
		public override bool WeatherSupported => true;

		public GreenAcres(string name, Map map, int priority, params Rectangle2D[] area) : base(name, map, priority, area)
		{
		}

		public GreenAcres(string name, Map map, int priority, params Poly2D[] area) : base(name, map, priority, area)
		{
		}

		public GreenAcres(string name, Map map, int priority, params Rectangle3D[] area) : base(name, map, priority, area)
		{
		}

		public GreenAcres(string name, Map map, int priority, params Poly3D[] area) : base(name, map, priority, area)
		{
		}

		public GreenAcres(string name, Map map, Region parent, params Rectangle2D[] area) : base(name, map, parent, area)
		{
		}

		public GreenAcres(string name, Map map, Region parent, params Poly2D[] area) : base(name, map, parent, area)
		{
		}

		public GreenAcres(string name, Map map, Region parent, params Rectangle3D[] area) : base(name, map, parent, area)
		{
		}

		public GreenAcres(string name, Map map, Region parent, params Poly3D[] area) : base(name, map, parent, area)
		{
		}

		public GreenAcres(RegionDefinition def, Map map, Region parent) : base(def, map, parent)
		{
		}

		public GreenAcres(int id) : base(id)
		{
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			Rules.AllowHouses = false;
			Rules.AllowVehicles = false;

			SpellPermissions[SpellName.Mark] = false;
			SpellPermissions[SpellName.Recall] = false;
			SpellPermissions[SpellName.GateTravel] = false;
			SpellPermissions[SpellName.SacredJourney] = false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
		}
	}
}