namespace Server.Regions
{
	public class Jail : BaseRegion
	{
		public Jail(string name, Map map, int priority, params Rectangle2D[] area) : base(name, map, priority, area)
		{
		}

		public Jail(string name, Map map, int priority, params Poly2D[] area) : base(name, map, priority, area)
		{
		}

		public Jail(string name, Map map, int priority, params Rectangle3D[] area) : base(name, map, priority, area)
		{
		}

		public Jail(string name, Map map, int priority, params Poly3D[] area) : base(name, map, priority, area)
		{
		}

		public Jail(string name, Map map, Region parent, params Rectangle2D[] area) : base(name, map, parent, area)
		{
		}

		public Jail(string name, Map map, Region parent, params Poly2D[] area) : base(name, map, parent, area)
		{
		}

		public Jail(string name, Map map, Region parent, params Rectangle3D[] area) : base(name, map, parent, area)
		{
		}

		public Jail(string name, Map map, Region parent, params Poly3D[] area) : base(name, map, parent, area)
		{
		}

		public Jail(RegionDefinition def, Map map, Region parent) : base(def, map, parent)
		{
		}

		public Jail(int id) : base(id)
		{
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			Rules.CanExit = false;
			Rules.CanEnter = false;

			Rules.AllowDelayLogout = false;

			Rules.AllowHouses = false;
			Rules.AllowVehicles = false;

			Rules.AllowMount = false;
			Rules.AllowEthereal = false;

			Rules.AllowSkills = false;
			Rules.AllowCombat = false;
			Rules.AllowMagic = false;
			Rules.AllowMelee = false;
			Rules.AllowRanged = false;

			Rules.AllowCreatureDeath = false;
			Rules.AllowCreatureHarm = false;
			Rules.AllowCreatureHeal = false;
			Rules.AllowCreatureLooting = false;

			Rules.AllowPlayerDeath = false;
			Rules.AllowPlayerHarm = false;
			Rules.AllowPlayerHeal = false;
			Rules.AllowPlayerLooting = false;
		}

		public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
		{
			base.AlterLightLevel(m, ref global, ref personal);

			global = LightCycle.JailLevel;
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