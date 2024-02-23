using Server.Items;
using Server.Regions;

using System;

namespace Server
{
	public sealed class TreasureRegion : BaseRegion
	{
		private const int Range = 5; // No house may be placed within 5 tiles of the treasure

		public static void Initialize()
		{
			for (var i = 0; i < TreasureMap.Locations.Length; i++)
			{
				var loc = TreasureMap.Locations[i];

				try
				{
					_ = new TreasureRegion(loc.X, loc.Y, Map.Felucca);
					_ = new TreasureRegion(loc.X, loc.Y, Map.Trammel);
				}
				catch (Exception e)
				{
					Console.WriteLine($"Warning: Invalid TreasureRegion: #{i}: {loc}");
					Console.WriteLine(e);
				}
			}
		}

		public TreasureRegion(int x, int y, Map map) : base(null, map, DefaultPriority, new Rectangle2D(x - Range, y - Range, 1 + (Range * 2), 1 + (Range * 2)))
		{
			GoLocation = new Point3D(x, y, map.GetAverageZ(x, y));

			Register();
		}

		public TreasureRegion(int id) : base(id)
		{ 
		}

		protected override void DefaultInit()
		{
			base.DefaultInit();

			Rules.AllowHouses = false;
			Rules.AllowVehicles = false;
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

			// tmap regions are not persisted
			Delete();
		}

		public override void OnEnter(Mobile m)
		{
			base.OnEnter(m);

			if (m.AccessLevel > AccessLevel.Player)
			{
				m.SendMessage("You have entered a protected treasure map area.");
			}
		}

		public override void OnExit(Mobile m)
		{
			base.OnExit(m);

			if (m.AccessLevel > AccessLevel.Player)
			{
				m.SendMessage("You have left a protected treasure map area.");
			}
		}
	}
}