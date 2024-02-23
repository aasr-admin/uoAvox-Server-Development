#define AVOS_MAPS

using System.Collections.Generic;

namespace Server.Engines.Facet
{
	public static class MapRegistry
	{
		public static Dictionary<int, HashSet<int>> Associations { get; } = new();

		private readonly record struct MapInfo(int Index, MapBounds Bounds, MapBounds Area, MapWrap Wrap, byte Season, string Name, MapRules Rules);

		private static readonly MapInfo[] _Info =
		{
			new MapInfo
			{
				Index = 0,
#if AVOS_MAPS
				Bounds = new MapBounds(7168, 4096),
				Area = new MapBounds(5120, 4096),
				Wrap = new MapWrap(new MapBounds(170, 370, 5130 - 190, 3733 - 379)),
				Season = 4,
				Name = "Felucca",
				Rules = MapRules.FeluccaRules,
#else
				Bounds = new MapBounds(7168, 4096),
				Area = new MapBounds(5120, 4096),
				Wrap = new MapWrap(new MapBounds(16, 16, 5120 - 32, 4096 - 32), new MapBounds(5136, 2320, 992, 1760)),
				Season = 4,
				Name = "Felucca",
				Rules = MapRules.FeluccaRules,
#endif
			},
			new MapInfo
			{
				Index = 1,
				Bounds = new MapBounds(7168, 4096),
				Area = new MapBounds(5120, 4096),
				Wrap = new MapWrap(new MapBounds(16, 16, 5120 - 32, 4096 - 32), new MapBounds(5136, 2320, 992, 1760)),
				Season = 0,
				Name = "Trammel",
				Rules = MapRules.TrammelRules,
			},
			new MapInfo
			{
				Index = 2,
				Bounds = new MapBounds(2304, 1600),
				Area = new MapBounds(2304, 1600),
				Wrap = new MapWrap(new MapBounds(16, 16, 2304 - 32, 1600 - 32)),
				Season = 1,
				Name = "Ilshenar",
				Rules = MapRules.TrammelRules,
			},
			new MapInfo
			{
				Index = 3,
				Bounds = new MapBounds(2560, 2048),
				Area = new MapBounds(2560, 2048),
				Wrap = new MapWrap(),
				Season = 1,
				Name = "Malas",
				Rules = MapRules.TrammelRules,
			},
			new MapInfo
			{
				Index = 4,
				Bounds = new MapBounds(1448, 1448),
				Area = new MapBounds(1448, 1448),
				Wrap = new MapWrap(new MapBounds(16, 16, 1448 - 32, 1448 - 32)),
				Season = 1,
				Name = "Tokuno",
				Rules = MapRules.TrammelRules,
			},
			new MapInfo
			{
				Index = 5,
				Bounds = new MapBounds(1280, 4096),
				Area = new MapBounds(1280, 4096),
				Wrap = new MapWrap(),
				Season = 1,
				Name = "TerMur",
				Rules = MapRules.TrammelRules,
			},
		};

		static MapRegistry()
		{
			foreach (var info in _Info)
			{
				if (info.Index != 0x7F)
				{
					RegisterMap(info.Index, info.Bounds, info.Area, info.Wrap, info.Season, info.Name, info.Rules);
				}
			}

			var internalBounds = new MapBounds(Map.SectorSize, Map.SectorSize);

			RegisterMap(0x7F, internalBounds, internalBounds, new(internalBounds), 1, "Internal", MapRules.Internal);
		}

		public static void Configure()
		{
			EventSink.ServerList += EventSink_OnServerList;
			EventSink.Login += EventSink_Login;
		}

		private static void EventSink_OnServerList(ServerListEventArgs args)
		{
			args.State.Send(new Engine.Facet.LoginComplete());
			args.State.Send(new Engine.Facet.MapDefinitions());
		}

		private static void EventSink_Login(LoginEventArgs args)
		{
			_ = args.Mobile.Send(new Engine.Facet.QueryClientHash(args.Mobile));
		}

		public static void RegisterMap(int index, MapBounds bounds, MapBounds area, MapWrap wrap, int season, string name, MapRules rules)
		{
			RegisterMap(index, index, index, bounds, area, wrap, season, name, rules);
		}

		public static void RegisterMap(int mapIndex, int mapID, int fileIndex, MapBounds bounds, MapBounds area, MapWrap wrap, int season, string name, MapRules rules)
		{
			var newMap = new Map(mapID, mapIndex, fileIndex, bounds, area, wrap, season, name, rules);

			Map.Maps[mapIndex] = newMap;

			Map.AllMaps.Add(newMap);

			if (mapIndex == 0x7F)
			{
				return;
			}

			if (!Associations.TryGetValue(fileIndex, out var value))
			{
				Associations[fileIndex] = value = new();
			}

			value.Add(mapIndex);
		}
	}
}