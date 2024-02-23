using System.Collections.Generic;

namespace Server
{
	public partial class Region
	{
		private static readonly Dictionary<string, HashSet<RegionDefinition>> m_Definitions = new()
		{
			#region Felucca

			["Felucca"] = new()
			{
				#region Moongates

				new("GuardedRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Moongates" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1336, 1997, 5) },
					{ "Music", "Invalid" },

					{ 1330, 1991, -128, 13, 13, 256 },
					{ 1494, 3767, -128, 12, 11, 256 },
					{ 2694, 685, -128, 15, 16, 256 },
					{ 1823, 2943, -128, 11, 11, 256 },
					{ 761, 741, -128, 19, 21, 256 },
					{ 638, 2062, -128, 12, 11, 256 },
					{ 4459, 1276, -128, 16, 16, 256 },
					{ 3554, 2132, -128, 18, 18, 256 },
				},

				#endregion

				#region Heartwood

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Heartwood" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6984, 337, 0) },
					{ "Music", "ElfCity" },

					{ 6911, 255, -128, 257, 257, 256 },
				},

				#endregion

				#region Sanctuary

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(764, 1646, 0) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Sanctuary" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6173, 23, 0) },
					{ "Music", "Dungeon9" },

					{ 6157, 4, -128, 242, 251, 256 },
				},

				#endregion

				#region Painted Caves

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1716, 892, 0) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Painted Caves" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6308, 892, -1) },
					{ "Music", "Dungeon9" },

					{ 6240, 860, -128, 70, 60, 256 },
				},

				#endregion

				#region Prism of Light

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(3784, 1097, 14) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Prism of Light" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6474, 188, 0) },
					{ "Music", "Dungeon9" },

					{ 6400, 0, -128, 221, 255, 256 },

					#region 

					new("CrystalField")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6509, 86, -4) },
						{ "Music", "Dungeon9" },

						{ 6506, 83, -4, 7, 7, 132 },
					},

					#endregion
					#region 

					new("IcyRiver")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6581, 88, 0) },
						{ "Music", "Dungeon9" },

						{ 6576, 73, 0, 10, 31, 128 },
						{ 6572, 83, 0, 4, 21, 128 },
						{ 6567, 90, 0, 5, 16, 128 },
						{ 6563, 94, 0, 4, 14, 128 },
						{ 6561, 95, 0, 2, 14, 128 },
						{ 6558, 98, 0, 7, 12, 128 },
						{ 6554, 101, 0, 4, 9, 128 },
					},

					#endregion
				},

				#endregion

				#region Blighted Grove

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(587, 1641, -1) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Blighted Grove" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6478, 863, 11) },
					{ "Music", "MelisandesLair" },

					{ 6440, 820, -128, 160, 150, 256 },

					#region 

					new("PoisonedTree")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6585, 875, 0) },
						{ "Music", "MelisandesLair" },

						{ 6570, 860, 0, 30, 30, 128 },
					},

					#endregion
				},

				#endregion

				#region Palace of Paroxysmus

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(5574, 3024, 31) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Palace of Paroxysmus" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6222, 335, 60) },
					{ "Music", "ParoxysmusLair" },

					{ 6191, 311, -128, 370, 360, 256 },

					#region Acid River

					new("DungeonRegion")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6219, 342, 60) },
						{ "Music", "ParoxysmusLair" },

						{ 6214, 340, -50, 11, 4, 178 },
						{ 6215, 337, -50, 5, 10, 178 },
						{ 6216, 335, -50, 3, 13, 178 },
						{ 6222, 340, -50, 3, 8, 178 },
						{ 6226, 342, -50, 1, 7, 178 },
						{ 6228, 345, -50, 2, 5, 178 },
						{ 6231, 346, -50, 1, 4, 178 },
						{ 6232, 347, -50, 3, 5, 178 },
						{ 6234, 349, -50, 2, 4, 178 },
						{ 6237, 350, -50, 0, 1, 178 },
						{ 6238, 353, -50, 2, 3, 178 },
						{ 6241, 354, -50, 2, 4, 178 },
						{ 6244, 355, -50, 3, 3, 178 },
						{ 6248, 356, -50, 3, 2, 178 },
						{ 6252, 355, -50, 1, 3, 178 },
						{ 6254, 354, -50, 10, 2, 178 },
						{ 6265, 353, -50, 3, 4, 178 },
						{ 6269, 354, -50, 2, 1, 178 },
						{ 6272, 353, -50, 9, 3, 178 },
						{ 6282, 354, -50, 2, 4, 178 },
						{ 6283, 355, -50, 2, 4, 178 },
						{ 6284, 357, -50, 3, 3, 178 },
						{ 6285, 359, -50, 3, 2, 178 },
						{ 6286, 360, -50, 3, 2, 178 },
						{ 6288, 361, -50, 2, 5, 178 },
						{ 6290, 362, -50, 2, 5, 178 },
						{ 6291, 363, -50, 2, 5, 178 },
						{ 6293, 365, -50, 1, 4, 178 },
						{ 6294, 366, -50, 1, 4, 178 },
						{ 6296, 367, -50, 1, 3, 178 },
						{ 6297, 369, -50, 3, 3, 178 },
						{ 6301, 370, -50, 3, 3, 178 },
						{ 6302, 371, -50, 8, 3, 178 },
						{ 6309, 369, -50, 5, 1, 178 },
						{ 6313, 366, -50, 2, 2, 178 },
						{ 6314, 364, -50, 5, 2, 178 },
						{ 6316, 362, -50, 3, 2, 178 },
						{ 6319, 361, -50, 11, 5, 178 },
						{ 6321, 360, -50, 7, 1, 178 },
						{ 6331, 363, -50, 4, 3, 178 },
						{ 6335, 364, -50, 13, 4, 178 },
						{ 6345, 362, -50, 5, 2, 178 },
						{ 6353, 358, -50, 7, 3, 178 },
						{ 6361, 359, -50, 16, 3, 178 },
						{ 6378, 357, -50, 14, 6, 178 },
						{ 6399, 352, -50, 14, 10, 178 },
						{ 6405, 363, -50, 9, 1, 178 },
						{ 6408, 365, -50, 7, 1, 178 },
						{ 6411, 367, -50, 3, 8, 178 },
						{ 6413, 375, -50, 5, 8, 178 },
						{ 6411, 383, -50, 6, 2, 178 },
						{ 6411, 386, -50, 5, 1, 178 },
						{ 6411, 388, -50, 3, 1, 178 },
						{ 6409, 390, -50, 3, 4, 178 },
						{ 6408, 395, -50, 4, 3, 178 },
						{ 6409, 398, -50, 5, 3, 178 },
						{ 6412, 401, -50, 4, 2, 178 },
						{ 6414, 402, -50, 3, 3, 178 },
						{ 6414, 406, -50, 2, 2, 178 },
						{ 6414, 408, -50, 1, 0, 178 },
						{ 6416, 410, -50, 6, 4, 178 },
						{ 6418, 407, -50, 1, 2, 178 },
						{ 6416, 415, -50, 4, 8, 178 },
						{ 6417, 422, -50, 4, 5, 178 },
						{ 6419, 426, -50, 3, 6, 178 },
						{ 6420, 432, -50, 4, 6, 178 },
						{ 6418, 439, -50, 3, 6, 178 },
						{ 6416, 444, -50, 3, 2, 178 },
						{ 6414, 447, -50, 5, 6, 178 },
						{ 6411, 454, -50, 7, 21, 178 },
						{ 6400, 474, -50, 15, 5, 178 },
						{ 6398, 470, -50, 4, 6, 178 },
						{ 6396, 468, -50, 1, 6, 178 },
						{ 6394, 463, -50, 1, 8, 178 },
						{ 6392, 462, -50, 1, 4, 178 },
						{ 6391, 452, -50, 2, 9, 178 },
						{ 6389, 454, -50, 2, 5, 178 },
						{ 6387, 456, -50, 2, 3, 178 },
						{ 6386, 445, -50, 1, 9, 178 },
						{ 6387, 444, -50, 2, 8, 178 },
						{ 6390, 445, -50, 2, 5, 178 },
						{ 6383, 445, -50, 4, 6, 178 },
						{ 6372, 444, -50, 10, 4, 178 },
						{ 6364, 443, -50, 9, 4, 178 },
						{ 6362, 446, -50, 4, 3, 178 },
						{ 6359, 448, -50, 4, 3, 178 },
						{ 6351, 449, -50, 10, 3, 178 },
						{ 6349, 453, -50, 7, 1, 178 },
						{ 6348, 454, -50, 3, 3, 178 },
						{ 6346, 458, -50, 2, 2, 178 },
						{ 6345, 460, -50, 2, 4, 178 },
						{ 6340, 461, -50, 3, 3, 178 },
						{ 6338, 463, -50, 1, 3, 178 },
						{ 6336, 465, -50, 2, 2, 178 },
						{ 6333, 466, -50, 4, 4, 178 },
						{ 6330, 469, -50, 2, 4, 178 },
						{ 6328, 473, -50, 5, 10, 178 },
						{ 6329, 484, -50, 5, 33, 178 },
						{ 6331, 518, -50, 4, 5, 178 },
						{ 6333, 522, -50, 5, 5, 178 },
						{ 6335, 528, -50, 3, 3, 178 },
						{ 6336, 531, -50, 4, 4, 178 },
						{ 6338, 536, -50, 4, 2, 178 },
						{ 6340, 539, -50, 4, 2, 178 },
						{ 6342, 542, -50, 4, 3, 178 },
						{ 6344, 546, -50, 4, 1, 178 },
						{ 6345, 547, -50, 4, 2, 178 },
						{ 6347, 549, -50, 5, 5, 178 },
						{ 6349, 555, -50, 4, 2, 178 },
						{ 6351, 558, -50, 4, 3, 178 },
						{ 6353, 562, -50, 3, 3, 178 },
						{ 6355, 564, -50, 3, 5, 178 },
						{ 6358, 568, -50, 4, 7, 178 },
						{ 6363, 568, -50, 2, 4, 178 },
						{ 6366, 567, -50, 10, 4, 178 },
						{ 6377, 569, -50, 3, 3, 178 },
						{ 6378, 570, -50, 5, 3, 178 },
						{ 6383, 569, -50, 3, 3, 178 },
						{ 6387, 569, -50, 8, 4, 178 },
						{ 6396, 569, -50, 7, 3, 178 },
						{ 6403, 571, -50, 2, 3, 178 },
						{ 6405, 573, -50, 2, 2, 178 },
						{ 6406, 576, -50, 0, 1, 178 },
						{ 6409, 576, -50, 1, 3, 178 },
						{ 6408, 577, -50, 0, 1, 178 },
						{ 6410, 577, -50, 2, 3, 178 },
						{ 6413, 578, -50, 2, 3, 178 },
						{ 6416, 579, -50, 1, 2, 178 },
						{ 6418, 578, -50, 16, 3, 178 },
						{ 6430, 576, -50, 4, 1, 178 },
						{ 6433, 565, -50, 2, 10, 178 },
						{ 6434, 564, -50, 13, 2, 178 },
						{ 6444, 563, -50, 3, 2, 178 },
						{ 6447, 562, -50, 4, 2, 178 },
						{ 6452, 561, -50, 12, 3, 178 },
						{ 6458, 559, -50, 6, 1, 178 },
						{ 6461, 555, -50, 3, 3, 178 },
						{ 6462, 551, -50, 2, 3, 178 },
						{ 6464, 548, -50, 2, 2, 178 },
						{ 6466, 546, -50, 16, 3, 178 },
						{ 6475, 544, -50, 1, 1, 178 },
						{ 6480, 540, -50, 6, 5, 178 },
						{ 6484, 538, -50, 6, 1, 178 },
						{ 6487, 536, -50, 3, 1, 178 },
						{ 6489, 534, -50, 2, 1, 178 },
						{ 6490, 532, -50, 2, 1, 178 },
						{ 6491, 530, -50, 7, 2, 178 },
						{ 6493, 529, -50, 6, 0, 178 },
						{ 6496, 527, -50, 4, 1, 178 },
						{ 6499, 524, -50, 3, 2, 178 },
						{ 6500, 522, -50, 3, 2, 178 },
						{ 6501, 520, -50, 3, 2, 178 },
						{ 6499, 514, -50, 3, 4, 178 },
						{ 6498, 510, -50, 5, 3, 178 },
						{ 6497, 500, -50, 8, 8, 178 },
						{ 6484, 448, -50, 65, 51, 178 },
						{ 6508, 500, -50, 5, 3, 178 },
						{ 6523, 500, -50, 17, 3, 178 },
						{ 6528, 504, -50, 4, 1, 178 },
					},

					#endregion
					#region Lair

					new("DungeonRegion")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6320, 476, -50) },
						{ "Music", "ParoxysmusLair" },

						{ 6316, 472, -128, 8, 8, 256 },
						{ 6303, 509, -128, 11, 7, 256 },
					},

					#endregion
				},

				#endregion

				#region Huntsman's Forest

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Huntsman's Forest" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1655, 610, 22) },
					{ "Music", "Invalid" },

					{ 1595, 550, -128, 120, 120, 256 },
				},

				#endregion

				#region Cove

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cove" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2275, 1210, 0) },
					{ "Music", "Cove" },

					{ 2200, 1110, -128, 50, 50, 256 },
					{ 2200, 1160, -128, 86, 86, 256 },
				},

				#endregion

				#region Britain

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Britain" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1495, 1629, 10) },
					{ "Music", "Britain1" },

					{ 1416, 1498, -10, 324, 279, 138 },
					{ 1385, 1538, -10, 31, 239, 138 },
					{ 1416, 1777, 0, 324, 60, 128 },
					{ 1385, 1777, 0, 31, 130, 128 },
					{ 1093, 1538, 0, 292, 369, 128 },

					#region Blackthorn Castle

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Blackthorn Castle" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1523, 1443, 15) },
						{ "Music", "LBCastle" },

						{ 1500, 1408, -128, 46, 90, 256 },
					},

					#endregion
					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1496, 1611, 10) },
						{ "Music", "Britain1" },

						{ 1492, 1602, 0, 8, 19, 128 },
						{ 1500, 1610, 0, 8, 16, 128 },
						{ 1576, 1584, 0, 24, 16, 128 },
						{ 1456, 1512, 0, 16, 16, 128 },
						{ 1472, 1512, 0, 8, 8, 128 },
						{ 1486, 1684, 0, 8, 8, 128 },
						{ 1494, 1676, 0, 8, 24, 128 },
						{ 1424, 1712, 0, 8, 24, 128 },
						{ 1432, 1712, 0, 8, 12, 128 },
						{ 1608, 1584, 0, 24, 8, 128 },
						{ 1616, 1576, 0, 8, 8, 128 },
						{ 1544, 1760, 0, 16, 16, 128 },
						{ 1560, 1760, 0, 8, 8, 128 },
					},

					#endregion
					#region A Wheatfield in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1135, 1791, 0) },
						{ "Music", "Britain1" },

						{ 1120, 1776, -128, 32, 32, 256 },
					},

					#endregion
					#region A Wheatfield in Britain 2

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1199, 1823, 0) },
						{ "Music", "Britain1" },

						{ 1184, 1808, -128, 32, 32, 256 },
					},

					#endregion
					#region A Wheatfield in Britain 3

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 3" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1231, 1887, 0) },
						{ "Music", "Britain1" },

						{ 1216, 1872, -128, 32, 32, 256 },
					},

					#endregion
					#region A Carrot Field in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Carrot Field in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1215, 1723, 0) },
						{ "Music", "Britain1" },

						{ 1208, 1712, -128, 16, 24, 256 },
					},

					#endregion
					#region An Onion Field in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "An Onion Field in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1231, 1723, 0) },
						{ "Music", "Britain1" },

						{ 1224, 1712, -128, 16, 24, 256 },
					},

					#endregion
					#region A Cabbage Field in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Cabbage Field in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1183, 1683, 0) },
						{ "Music", "Britain1" },

						{ 1176, 1672, -128, 16, 23, 256 },
					},

					#endregion
					#region A Turnip Field in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Turnip Field in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1199, 1683, 0) },
						{ "Music", "Britain1" },

						{ 1192, 1672, -128, 16, 24, 256 },
					},

					#endregion
					#region A Wheatfield in Britain 4

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 4" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1120, 1624, 0) },
						{ "Music", "Britain1" },

						{ 1104, 1608, -128, 32, 32, 256 },
					},

					#endregion
					#region A Wheatfield in Britain 5

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 5" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1152, 1576, 0) },
						{ "Music", "Britain1" },

						{ 1136, 1560, -128, 32, 32, 256 },
					},

					#endregion
					#region A Turnip Field in Britain 2

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Turnip Field in Britain 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1216, 1604, 0) },
						{ "Music", "Britain1" },

						{ 1208, 1592, -128, 16, 24, 256 },
					},

					#endregion
					#region A Carrot Field in Britain 2

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Carrot Field in Britain 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1232, 1604, 0) },
						{ "Music", "Britain1" },

						{ 1224, 1592, -128, 16, 24, 256 },
					},

					#endregion
				},

				#endregion

				#region Britain Graveyard

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Britain Graveyard" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1384, 1492, 10) },
					{ "Music", "Invalid" },

					{ 1333, 1441, -128, 84, 82, 256 },
				},

				#endregion

				#region Jhelom Islands

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Jhelom Islands" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1383, 3815, 0) },
					{ "Music", "Jhelom" },

					{ 1111, 3567, -128, 33, 21, 256 },
					{ 1078, 3588, -128, 124, 121, 256 },
					{ 1224, 3592, -128, 309, 473, 256 },

					#region Jhelom

					new("TownRegion")
					{
						{ "Disabled", false },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Jhelom" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1383, 3815, 0) },
						{ "Music", "Jhelom" },

						{ 1303, 3670, -20, 189, 225, 148 },
						{ 1338, 3895, -20, 74, 28, 148 },
						{ 1383, 3951, -20, 109, 94, 148 },

						#region 

						new("GenericRegion")
						{
							{ "RuneName", "" },
							{ "NoLogoutDelay", true },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(1360, 3816, 0) },
							{ "Music", "Jhelom" },

							{ 1352, 3800, -20, 16, 32, 148 },
							{ 1368, 3808, -20, 8, 16, 148 },
							{ 1432, 3768, -20, 32, 8, 148 },
							{ 1440, 3776, -20, 24, 8, 148 },
						},

						#endregion
					},

					#endregion
				},

				#endregion

				#region Minoc

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2466, 544, 0) },
					{ "Music", "Minoc" },

					{ 2411, 366, -128, 135, 241, 256 },
					{ 2548, 495, -128, 72, 55, 256 },
					{ 2564, 585, -128, 3, 42, 256 },
					{ 2567, 585, -128, 61, 61, 256 },
					{ 2499, 627, -128, 68, 63, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2477, 401, 15) },
						{ "Music", "Minoc" },

						{ 2457, 397, -128, 40, 8, 256 },
						{ 2465, 405, -128, 8, 8, 256 },
						{ 2481, 405, -128, 8, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Ocllo

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ocllo" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3650, 2519, 0) },
					{ "Music", "Ocllo" },

					{ 3587, 2456, -128, 119, 99, 256 },
					{ 3706, 2460, -128, 2, 95, 256 },
					{ 3587, 2555, -128, 106, 73, 256 },
					{ 3590, 2628, -128, 103, 58, 256 },
					{ 3693, 2555, -128, 61, 144, 256 },
					{ 3754, 2558, -128, 7, 141, 256 },
					{ 3761, 2555, -128, 7, 144, 178 },
					{ 3695, 2699, -128, 66, 13, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(3672, 2616, 0) },
						{ "Music", "Ocllo" },

						{ 3664, 2608, -128, 16, 16, 256 },
						{ 3664, 2640, -128, 8, 16, 256 },
						{ 3672, 2648, -128, 8, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Trinsic

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Trinsic" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1867, 2780, 0) },
					{ "Music", "Trinsic" },

					{ 1856, 2636, -128, 75, 28, 256 },
					{ 1816, 2664, -128, 283, 231, 256 },
					{ 2099, 2782, -128, 18, 25, 256 },
					{ 1970, 2895, -128, 47, 32, 256 },
					{ 1796, 2696, 0, 20, 67, 128 },
					{ 1800, 2796, 0, 16, 52, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1845, 2736, 0) },
						{ "Music", "Trinsic" },

						{ 1834, 2728, -128, 22, 16, 256 },
						{ 2024, 2784, -128, 16, 20, 256 },
						{ 2026, 2804, -128, 14, 2, 256 },
						{ 2024, 2806, -128, 16, 7, 256 },
						{ 1923, 2786, -128, 12, 22, 256 },
						{ 1935, 2786, -128, 7, 14, 256 },
					},

					#endregion
				},

				#endregion

				#region Vesper

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Vesper" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2899, 676, 0) },
					{ "Music", "Vesper" },

					{ 2893, 598, -128, 121, 50, 256 },
					{ 2816, 648, -128, 249, 365, 256 },
					{ 2734, 944, -128, 82, 4, 256 },
					{ 2728, 948, -128, 88, 53, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2960, 880, 0) },
						{ "Music", "Vesper" },

						{ 2952, 864, -128, 16, 32, 256 },
						{ 2968, 872, -128, 8, 16, 256 },
						{ 2776, 952, -128, 16, 32, 256 },
						{ 2768, 960, -128, 8, 16, 256 },
						{ 2892, 901, -128, 16, 19, 256 },
						{ 2908, 904, -128, 8, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Yew

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Yew" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(546, 992, 0) },
					{ "Music", "Yew" },

					{ 92, 656, 0, 349, 225, 39 },
					{ 441, 746, 0, 216, 135, 39 },
					{ 258, 881, 0, 399, 380, 39 },
					{ 657, 922, 0, 42, 307, 39 },
					{ 657, 806, 0, 17, 28, 39 },
					{ 718, 874, 0, 38, 22, 39 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(612, 820, 0) },
						{ "Music", "Yew" },

						{ 600, 808, 0, 24, 24, 39 },
					},

					#endregion
					#region A Field of Sheep in Yew 1

					new("GenericRegion")
					{
						{ "RuneName", "A Field of Sheep in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Field of Sheep in Yew 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(675, 939, 0) },
						{ "Music", "Yew" },

						{ 664, 928, -128, 22, 22, 256 },
					},

					#endregion
					#region A Field of Sheep in Yew 2

					new("GenericRegion")
					{
						{ "RuneName", "A Field of Sheep in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Field of Sheep in Yew 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(675, 1179, 0) },
						{ "Music", "Yew" },

						{ 664, 1168, -128, 22, 22, 256 },
					},

					#endregion
					#region A Farm in Yew

					new("GenericRegion")
					{
						{ "RuneName", "A Farm in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Farm in Yew" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(571, 1099, 0) },
						{ "Music", "Yew" },

						{ 560, 1088, -128, 22, 22, 256 },
					},

					#endregion
					#region A Wheatfield in Yew 1

					new("GenericRegion")
					{
						{ "RuneName", "A Wheatfield in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Yew 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(567, 1239, 0) },
						{ "Music", "Yew" },

						{ 560, 1232, -128, 16, 16, 256 },
					},

					#endregion
					#region A Wheatfield in Yew 2

					new("GenericRegion")
					{
						{ "RuneName", "A Wheatfield in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Yew 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(375, 1191, 0) },
						{ "Music", "Yew" },

						{ 368, 1176, -128, 14, 32, 256 },
					},

					#endregion
				},

				#endregion

				#region Wind

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Wind" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5223, 190, 5) },
					{ "Music", "Wind" },

					{ 5294, 19, -128, 72, 120, 256 },
					{ 5132, 58, -128, 81, 68, 256 },
					{ 5197, 126, -128, 55, 78, 256 },
					{ 5132, 3, -128, 70, 55, 256 },
					{ 5252, 112, -128, 42, 58, 256 },
					{ 5213, 98, -128, 39, 28, 256 },
					{ 5279, 57, -128, 15, 55, 256 },
					{ 5252, 170, -128, 32, 8, 256 },
					{ 5286, 25, -128, 8, 32, 256 },
					{ 5252, 178, -128, 20, 5, 256 },
					{ 5252, 183, -128, 10, 10, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5171, 19, 27) },
						{ "Music", "Wind" },

						{ 5159, 15, -128, 25, 9, 256 },
						{ 5159, 24, -128, 9, 16, 256 },
						{ 5175, 24, -128, 9, 8, 256 },
						{ 5212, 159, -128, 9, 24, 256 },
						{ 5221, 171, -128, 7, 12, 256 },
						{ 5206, 164, -128, 6, 15, 256 },
						{ 5303, 28, -128, 16, 14, 256 },
					},

					#endregion
				},

				#endregion

				#region Serpent's Hold

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Serpent's Hold" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3010, 3371, 15) },
					{ "Music", "Serpents" },

					{ 2868, 3324, 0, 205, 195, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2968, 3408, 15) },
						{ "Music", "Serpents" },

						{ 2960, 3400, 0, 16, 16, 128 },
						{ 2968, 3416, 0, 8, 16, 128 },
						{ 3008, 3450, 0, 14, 14, 128 },
					},

					#endregion
				},

				#endregion

				#region Skara Brae

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Skara Brae" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(632, 2233, 0) },
					{ "Music", "Skarabra" },

					{ 538, 2107, -128, 150, 190, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(608, 2244, 0) },
						{ "Music", "Skarabra" },

						{ 600, 2232, -128, 16, 24, 256 },
						{ 592, 2240, -128, 8, 16, 256 },
						{ 616, 2240, -128, 8, 16, 256 },
						{ 552, 2168, -128, 16, 24, 256 },
						{ 568, 2168, -128, 8, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Nujel'm

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Nujel'm" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3732, 1279, 0) },
					{ "Music", "Nujelm" },

					{ 3475, 1000, 0, 360, 435, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(3744, 1195, 0) },
						{ "Music", "Nujelm" },

						{ 3736, 1184, 0, 16, 23, 128 },
						{ 3728, 1192, 0, 8, 15, 128 },
						{ 3728, 1288, 0, 23, 15, 128 },
						{ 3728, 1303, 0, 16, 9, 128 },
						{ 3728, 1312, 0, 12, 8, 128 },
						{ 3728, 1320, 0, 16, 23, 128 },
						{ 3744, 1328, 0, 7, 15, 128 },
						{ 3760, 1216, 0, 12, 24, 128 },
						{ 3772, 1220, 0, 4, 16, 128 },
						{ 3776, 1224, 0, 8, 8, 128 },
						{ 3728, 1248, 0, 16, 24, 128 },
						{ 3744, 1264, 0, 8, 8, 128 },
						{ 3744, 1248, 0, 8, 8, 128 },
					},

					#endregion
				},

				#endregion

				#region Moonglow

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Moonglow" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4442, 1172, 0) },
					{ "Music", "Moonglow" },

					{ 4535, 844, 0, 20, 3, 128 },
					{ 4530, 847, 0, 31, 61, 128 },
					{ 4521, 914, 0, 56, 49, 128 },
					{ 4278, 915, 0, 54, 19, 128 },
					{ 4283, 944, 0, 53, 73, 128 },
					{ 4377, 1015, -10, 59, 37, 138 },
					{ 4367, 1050, 0, 142, 145, 128 },
					{ 4539, 1036, 0, 27, 18, 128 },
					{ 4517, 1053, 0, 23, 22, 128 },
					{ 4389, 1198, 0, 47, 39, 128 },
					{ 4466, 1211, 0, 32, 25, 128 },
					{ 4700, 1108, 0, 17, 18, 128 },
					{ 4656, 1127, 0, 26, 13, 128 },
					{ 4678, 1162, 0, 25, 25, 128 },
					{ 4613, 1196, 0, 23, 22, 128 },
					{ 4646, 1212, 0, 14, 17, 128 },
					{ 4677, 1214, 0, 26, 22, 128 },
					{ 4622, 1316, 0, 22, 24, 128 },
					{ 4487, 1353, 0, 59, 21, 128 },
					{ 4477, 1374, 0, 69, 35, 128 },
					{ 4659, 1387, 0, 40, 40, 128 },
					{ 4549, 1482, 0, 29, 27, 128 },
					{ 4405, 1451, 0, 23, 23, 128 },
					{ 4483, 1468, 0, 21, 13, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(4388, 1164, 0) },
						{ "Music", "Moonglow" },

						{ 4384, 1152, 0, 8, 24, 128 },
						{ 4392, 1160, 0, 16, 8, 128 },
						{ 4400, 1152, 0, 8, 8, 128 },
						{ 4480, 1056, 0, 8, 16, 128 },
						{ 4488, 1060, 0, 4, 8, 128 },
						{ 4476, 1060, 0, 4, 8, 128 },
					},

					#endregion
				},

				#endregion

				#region Magincia

				new("NewMaginciaRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Magincia" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3714, 2220, 20) },
					{ "Music", "Magincia" },

					{ 3632, 2032, -128, 50, 70, 256 },
					{ 3624, 2162, -128, 95, 70, 256 },
					{ 3752, 2046, -128, 52, 48, 256 },
					{ 3680, 2045, -128, 72, 49, 256 },
					{ 3652, 2094, -128, 160, 180, 256 },
					{ 3649, 2256, -128, 54, 47, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(3693, 2254, 20) },
						{ "Music", "Magincia" },

						{ 3687, 2246, -128, 12, 16, 256 },
					},

					#endregion
				},

				#endregion

				#region Buccaneer's Den

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Buccaneer's Den" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2706, 2163, 0) },
					{ "Music", "Bucsden" },

					{ 2612, 2057, -128, 164, 210, 256 },
					{ 2604, 2065, 0, 8, 189, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2720, 2088, 0) },
						{ "Music", "Bucsden" },

						{ 2712, 2080, -128, 16, 16, 256 },
						{ 2712, 2096, -128, 8, 8, 256 },
						{ 2664, 2232, -128, 24, 8, 256 },
						{ 2672, 2240, -128, 16, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Covetous

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(2499, 916, 8) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Covetous" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5456, 1862, 0) },
					{ "Music", "Dungeon9" },

					{ 5376, 1793, -128, 201, 255, 256 },
					{ 5576, 1791, -128, 57, 257, 256 },
				},

				#endregion

				#region Deceit

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(4111, 429, 34) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Deceit" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5187, 635, 0) },
					{ "Music", "Dungeon9" },

					{ 5122, 518, -128, 248, 252, 256 },
				},

				#endregion

				#region Despise

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1296, 1082, 9) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Despise" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5501, 570, 59) },
					{ "Music", "Dungeon9" },

					{ 5377, 516, -128, 254, 506, 256 },
				},

				#endregion

				#region Destard

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1176, 2635, 9) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Destard" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5243, 1004, 0) },
					{ "Music", "Dungeon9" },

					{ 5120, 770, -128, 251, 258, 256 },
				},

				#endregion

				#region Hythloth

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(4722, 3814, 0) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Hythloth" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5905, 22, 44) },
					{ "Music", "Dungeon9" },

					{ 5898, 2, -128, 238, 244, 256 },
				},

				#endregion

				#region Khaldun

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(5882, 3819, -1) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Khaldun" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5571, 1302, 0) },
					{ "Music", "Dungeon9" },

					{ 5381, 1284, -128, 247, 225, 256 },
				},

				#endregion

				#region Jail

				new("Jail")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Jail" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5275, 1163, 0) },
					{ "Music", "Invalid" },

					{ 5271, 1159, -128, 41, 33, 256 },
				},

				#endregion

				#region Green Acres

				new("GreenAcres")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Green Acres" },
					{ "Priority", 1 },
					{ "GoLocation", new Point3D(5445, 1153, 0) },
					{ "Music", "Invalid" },

					{ 5376, 512, -128, 767, 767, 256 },
				},

				#endregion

				#region Shame

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(512, 1559, 10) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shame" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5395, 126, 0) },
					{ "Music", "Dungeon9" },

					{ 5377, 2, -128, 257, 260, 256 },
					{ 5635, 2, -128, 260, 124, 256 },
				},

				#endregion

				#region Wrong

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(2042, 226, 14) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Wrong" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5825, 599, 0) },
					{ "Music", "Dungeon9" },

					{ 5633, 511, -128, 253, 510, 256 },

					#region 

					new("WrongLevel3")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5703, 645, 0) },
						{ "Music", "Dungeon9" },

						{ 5687, 623, -128, 33, 44, 256 },

						#region 

						new("WrongJail")
						{
							{ "EntranceLocation", new Point3D(0, 0, 0) },
							{ "EntranceMap", "" },
							{ "RuneName", "" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(5703, 639, 0) },
							{ "Music", "Dungeon9" },

							{ 5700, 636, -128, 7, 7, 256 },
						},

						#endregion
					},

					#endregion
				},

				#endregion

				#region Cave 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cave 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2367, 942, 0) },
					{ "Music", "Invalid" },

					{ 2373, 900, -128, 22, 28, 256 },
					{ 2395, 903, -128, 14, 16, 256 },
					{ 2373, 928, -128, 10, 9, 256 },
					{ 2359, 927, -128, 14, 18, 256 },
				},

				#endregion

				#region Cave 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cave 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1903, 365, 0) },
					{ "Music", "Invalid" },

					{ 1887, 354, -128, 33, 23, 256 },
				},

				#endregion

				#region Cave 3

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cave 3" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1934, 316, 0) },
					{ "Music", "Invalid" },

					{ 1925, 307, -128, 18, 18, 256 },
				},

				#endregion

				#region Cave 4

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cave 4" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2345, 830, 0) },
					{ "Music", "Invalid" },

					{ 2323, 809, -128, 45, 42, 256 },
				},

				#endregion

				#region Britain Mine 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Britain Mine 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1443, 1228, 0) },
					{ "Music", "Invalid" },

					{ 1436, 1215, -128, 29, 37, 256 },
				},

				#endregion

				#region Britain Mine 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Britain Mine 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1629, 1189, 0) },
					{ "Music", "Invalid" },

					{ 1611, 1175, -128, 51, 29, 256 },
				},

				#endregion

				#region Minoc Cave 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc Cave 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2426, 177, 0) },
					{ "Music", "Invalid" },

					{ 2406, 168, -128, 22, 16, 256 },
				},

				#endregion

				#region Minoc Cave 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc Cave 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2440, 94, 0) },
					{ "Music", "Invalid" },

					{ 2418, 81, -128, 24, 32, 256 },
				},

				#endregion

				#region Minoc Cave 3

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc Cave 3" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2471, 64, 0) },
					{ "Music", "Invalid" },

					{ 2447, 39, -128, 42, 28, 256 },
				},

				#endregion

				#region Minoc Mine

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc Mine" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2558, 499, 0) },
					{ "Music", "Invalid" },

					{ 2556, 501, -128, 6, 3, 256 },
					{ 2556, 474, -128, 26, 27, 256 },
				},

				#endregion

				#region Avatar Isle Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Avatar Isle Cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4605, 3815, 0) },
					{ "Music", "Invalid" },

					{ 4594, 3807, -128, 23, 17, 256 },
				},

				#endregion

				#region Ice Isle Cave 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ice Isle Cave 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4054, 440, 3) },
					{ "Music", "Invalid" },

					{ 4018, 421, -128, 49, 49, 256 },
				},

				#endregion

				#region Ice Isle Cave 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ice Isle Cave 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4030, 325, 0) },
					{ "Music", "Invalid" },

					{ 4002, 310, -128, 43, 41, 256 },
					{ 4005, 298, -128, 22, 12, 256 },
				},

				#endregion

				#region North Territory Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "North Territory Cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1984, 262, 8) },
					{ "Music", "Invalid" },

					{ 1973, 251, -128, 21, 23, 256 },
				},

				#endregion

				#region Yew Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Yew Cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(773, 1684, 0) },
					{ "Music", "Invalid" },

					{ 766, 1683, -128, 12, 14, 256 },
				},

				#endregion

				#region North Territory Mine 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "North Territory Mine 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1723, 1065, 0) },
					{ "Music", "Invalid" },

					{ 1713, 1055, -128, 40, 25, 256 },
				},

				#endregion

				#region North Territory Mine 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "North Territory Mine 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1638, 974, 0) },
					{ "Music", "Invalid" },

					{ 1604, 958, -128, 46, 51, 256 },
				},

				#endregion

				#region Mt Kendall

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Mt Kendall" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2593, 477, 60) },
					{ "Music", "Invalid" },

					{ 2552, 448, -128, 71, 59, 256 },
					{ 2547, 380, -128, 43, 68, 256 },
					{ 2590, 413, -128, 33, 35, 256 },
				},

				#endregion

				#region Covetous Mine

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Covetous Mine" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2445, 880, 0) },
					{ "Music", "Invalid" },

					{ 2429, 866, -128, 19, 45, 256 },
					{ 2448, 879, -128, 27, 28, 256 },
					{ 2456, 907, -128, 19, 34, 256 },
				},

				#endregion

				#region Terathan Keep

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(5426, 3120, -60) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Terathan Keep" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5451, 3143, -60) },
					{ "Music", "Dungeon9" },

					{ 5404, 3099, -128, 77, 68, 256 },
					{ 5120, 1530, -128, 254, 258, 256 },
				},

				#endregion

				#region Fire

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(2922, 3402, 21) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Fire" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5760, 2908, 15) },
					{ "Music", "Dungeon9" },

					{ 5635, 1285, -128, 245, 235, 256 },
				},

				#endregion

				#region Ice

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1996, 80, 15) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ice" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5210, 2322, 30) },
					{ "Music", "Dungeon9" },

					{ 5668, 130, -128, 220, 138, 256 },
					{ 5800, 319, -128, 63, 65, 256 },
					{ 5654, 300, -128, 54, 40, 256 },
				},

				#endregion

				#region Blackthorn Dungeon

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(6430, 2678, 0) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Blackthorn Dungeon" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6430, 2678, 0) },
					{ "Music", "Dungeon9" },

					{ 6151, 2301, -128, 413, 539, 256 },
				},

				#endregion

				#region Delucia

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Delucia" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5228, 3978, 37) },
					{ "Music", "Invalid" },

					{ 5123, 3942, -128, 192, 122, 256 },
					{ 5147, 4064, -128, 125, 20, 256 },
					{ 5235, 3930, -128, 80, 12, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5199, 4063, 37) },
						{ "Music", "Invalid" },

						{ 5194, 4053, -128, 10, 20, 256 },
					},

					#endregion
				},

				#endregion

				#region Papua

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Papua" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5769, 3176, 0) },
					{ "Music", "Invalid" },

					{ 5639, 3095, -128, 192, 223, 256 },
					{ 5831, 3237, -128, 20, 30, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5769, 3162, 14) },
						{ "Music", "Invalid" },

						{ 5757, 3150, -128, 24, 24, 256 },
					},

					#endregion
				},

				#endregion

				#region Wrong Entrance

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Wrong Entrance" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2043, 236, 13) },
					{ "Music", "Mountn_a" },

					{ 1939, 215, -128, 134, 137, 256 },
				},

				#endregion

				#region Covetous Entrance

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Covetous Entrance" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2499, 918, 0) },
					{ "Music", "Vesper" },

					{ 2433, 846, -128, 128, 128, 256 },
				},

				#endregion

				#region Despise Entrance

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Despise Entrance" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1298, 1081, 0) },
					{ "Music", "Invalid" },

					{ 1289, 1064, -128, 32, 37, 256 },
				},

				#endregion

				#region Despise Passage

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Despise Passage" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1380, 1114, 0) },
					{ "Music", "Invalid" },

					{ 1338, 1060, -128, 51, 62, 256 },
					{ 1354, 1122, -128, 42, 121, 256 },
					{ 1349, 1122, -128, 5, 102, 256 },
				},

				#endregion

				#region Misc Dungeons

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1492, 1641, 20) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Misc Dungeons" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6032, 1499, 0) },
					{ "Music", "Dungeon9" },

					{ 5886, 1281, -128, 257, 254, 256 },
				},

				#endregion

				#region Orc Cave

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1014, 1434, 0) },
					{ "EntranceMap", "Felucca" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Orc Cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5137, 2015, 0) },
					{ "Music", "Dungeon9" },

					{ 5281, 1283, -128, 92, 103, 256 },
					{ 5267, 1955, -128, 97, 91, 256 },
					{ 5127, 1941, -128, 37, 83, 256 },
				},

				#endregion

				#region A Cotton Field in Moonglow

				new("GenericRegion")
				{
					{ "RuneName", "A Cotton Field in Moonglow" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Cotton Field in Moonglow" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4567, 1475, 0) },
					{ "Music", "Invalid" },

					{ 4557, 1471, -128, 20, 10, 256 },
				},

				#endregion

				#region A Wheatfield in Skara Brae 1

				new("GenericRegion")
				{
					{ "RuneName", "A Wheatfield in Skara Brae" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Wheatfield in Skara Brae 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(813, 2163, 0) },
					{ "Music", "Invalid" },

					{ 796, 2152, -128, 36, 24, 256 },
				},

				#endregion

				#region A Carrot Field in Skara Brae

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Carrot Field in Skara Brae" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2254, 0) },
					{ "Music", "Invalid" },

					{ 816, 2251, -128, 16, 8, 256 },
				},

				#endregion

				#region An Onion Field in Skara Brae

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "An Onion Field in Skara Brae" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2264, 0) },
					{ "Music", "Invalid" },

					{ 816, 2261, -128, 16, 8, 256 },
				},

				#endregion

				#region A Cabbage Field in Skara Brae 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Cabbage Field in Skara Brae 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2264, 0) },
					{ "Music", "Invalid" },

					{ 816, 2271, -128, 16, 8, 256 },
				},

				#endregion

				#region A Cabbage Field in Skara Brae 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Cabbage Field in Skara Brae 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2274, 0) },
					{ "Music", "Invalid" },

					{ 816, 2281, -128, 16, 8, 256 },
				},

				#endregion

				#region A Wheatfield in Skara Brae 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Wheatfield in Skara Brae 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(843, 2352, 0) },
					{ "Music", "Invalid" },

					{ 835, 2344, -128, 16, 16, 256 },
				},

				#endregion

				#region A Cotton Field in Skara Brae

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Cotton Field in Skara Brae" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2355, 0) },
					{ "Music", "Invalid" },

					{ 816, 2344, -128, 16, 24, 256 },
				},

				#endregion

				#region Shrine of Compassion

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Compassion" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1856, 872, -1) },
					{ "Music", "Invalid" },

					{ 1851, 867, -128, 14, 14, 256 },
				},

				#endregion

				#region Chaos Shrine

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Chaos Shrine" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1456, 854, 0) },
					{ "Music", "Invalid" },

					{ 1456, 840, -128, 4, 7, 256 },
				},

				#endregion

				#region Shrine of Honesty

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Honesty" },
					{ "Priority", 37 },
					{ "GoLocation", new Point3D(4217, 564, 36) },
					{ "Music", "Invalid" },

					{ 4209, 560, -128, 7, 8, 256 },
				},

				#endregion

				#region Shrine of Honor

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Honor" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1730, 3528, 3) },
					{ "Music", "Invalid" },

					{ 1721, 3525, -128, 8, 6, 256 },
				},

				#endregion

				#region Shrine of Humility

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Humility" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4276, 3699, 0) },
					{ "Music", "Invalid" },

					{ 4270, 3694, -128, 9, 9, 256 },
				},

				#endregion

				#region Shrine of Justice

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Justice" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1301, 639, 16) },
					{ "Music", "Invalid" },

					{ 1297, 629, -128, 9, 9, 256 },
				},

				#endregion

				#region Shrine of Sacrifice

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Sacrifice" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3355, 299, 9) },
					{ "Music", "Invalid" },

					{ 3352, 286, -128, 6, 7, 256 },
				},

				#endregion

				#region Shrine of Spirituality

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Spirituality" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1589, 2485, 5) },
					{ "Music", "Invalid" },

					{ 1590, 2485, -128, 10, 11, 256 },
					{ 1599, 2488, -128, 10, 5, 256 },
				},

				#endregion

				#region Shrine of Valor

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Valor" },
					{ "Priority", 37 },
					{ "GoLocation", new Point3D(2496, 3932, 0) },
					{ "Music", "Invalid" },

					{ 2488, 3928, -128, 9, 11, 256 },
				},

				#endregion

				#region South Britannian Sea

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "South Britannian Sea" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1394, 3890, 0) },
					{ "Music", "Invalid" },

					{ 688, 3700, -128, 1412, 380, 256 },
				},

				#endregion

				#region Paroxysmus Exit

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Paroxysmus Exit" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5623, 3038, 15) },
					{ "Music", "Invalid" },

					{ 5620, 3032, -128, 10, 10, 256 },
				},

				#endregion

				#region Sanctuary Entrance

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Sanctuary Entrance" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(762, 1645, 0) },
					{ "Music", "Invalid" },

					{ 759, 1635, -128, 12, 12, 256 },
				},

				#endregion

				#region Solen Hives

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Solen Hives" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5774, 1896, 20) },
					{ "Music", "Invalid" },

					{ 5640, 1776, -128, 295, 263, 256 },
				},

				#endregion

				#region Sea Market

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Sea Market" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4550, 2317, -2) },
					{ "Music", "Invalid" },

					{ 4529, 2296, -128, 45, 112, 256 },
				},

				#endregion
			},

			#endregion

			#region Trammel

			["Trammel"] = new()
			{
				#region Khaldun

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(5882, 3819, -1) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Khaldun" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5571, 1302, 0) },
					{ "Music", "Dungeon9" },

					{ 5381, 1284, -128, 247, 225, 256 },
				},

				#endregion

				#region Underwater World

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Underwater World" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6362, 1713, -10) },
					{ "Music", "Invalid" },

					{ 6251, 1613, -128, 222, 200, 256 },
				},

				#endregion

				#region Moongates

				new("GuardedRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Moongates" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1336, 1997, 5) },
					{ "Music", "Invalid" },

					{ 1330, 1991, -128, 13, 13, 256 },
					{ 1494, 3767, -128, 12, 11, 256 },
					{ 2694, 685, -128, 15, 16, 256 },
					{ 1823, 2943, -128, 11, 11, 256 },
					{ 761, 741, -128, 19, 21, 256 },
					{ 638, 2062, -128, 12, 11, 256 },
					{ 4459, 1276, -128, 16, 16, 256 },
					{ 3554, 2132, -128, 18, 18, 256 },
				},

				#endregion

				#region Heartwood

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Heartwood" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6984, 337, 0) },
					{ "Music", "ElfCity" },

					{ 6911, 255, -128, 257, 257, 256 },
				},

				#endregion

				#region Sanctuary

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(764, 1646, 0) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Sanctuary" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6173, 23, 0) },
					{ "Music", "Dungeon9" },

					{ 6157, 4, -128, 242, 251, 256 },
				},

				#endregion

				#region Painted Caves

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1716, 892, 0) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Painted Caves" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6308, 892, -1) },
					{ "Music", "Dungeon9" },

					{ 6240, 860, -128, 70, 60, 256 },
				},

				#endregion

				#region Prism of Light

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(3784, 1097, 14) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Prism of Light" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6474, 188, 0) },
					{ "Music", "Dungeon9" },

					{ 6400, 0, -128, 221, 255, 256 },

					#region 

					new("CrystalField")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6509, 86, -4) },
						{ "Music", "Dungeon9" },

						{ 6506, 83, -4, 7, 7, 132 },
					},

					#endregion
					#region 

					new("IcyRiver")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6581, 88, 0) },
						{ "Music", "Dungeon9" },

						{ 6576, 73, 0, 10, 31, 128 },
						{ 6572, 83, 0, 4, 21, 128 },
						{ 6567, 90, 0, 5, 16, 128 },
						{ 6563, 94, 0, 4, 14, 128 },
						{ 6561, 95, 0, 2, 14, 128 },
						{ 6558, 98, 0, 7, 12, 128 },
						{ 6554, 101, 0, 4, 9, 128 },
					},

					#endregion
				},

				#endregion

				#region Blighted Grove

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(587, 1641, -1) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Blighted Grove" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6478, 863, 11) },
					{ "Music", "MelisandesLair" },

					{ 6440, 820, -128, 160, 150, 256 },

					#region 

					new("PoisonedTree")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6585, 875, 0) },
						{ "Music", "MelisandesLair" },

						{ 6570, 860, 0, 30, 30, 128 },
					},

					#endregion
				},

				#endregion

				#region Palace of Paroxysmus

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(5574, 3024, 31) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Palace of Paroxysmus" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6222, 335, 60) },
					{ "Music", "ParoxysmusLair" },

					{ 6191, 311, -128, 370, 360, 256 },

					#region 

					new("AcidRiver")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6219, 342, 60) },
						{ "Music", "ParoxysmusLair" },

						{ 6214, 340, -50, 11, 4, 178 },
						{ 6215, 337, -50, 5, 10, 178 },
						{ 6216, 335, -50, 3, 13, 178 },
						{ 6222, 340, -50, 3, 8, 178 },
						{ 6226, 342, -50, 1, 7, 178 },
						{ 6228, 345, -50, 2, 5, 178 },
						{ 6231, 346, -50, 1, 4, 178 },
						{ 6232, 347, -50, 3, 5, 178 },
						{ 6234, 349, -50, 2, 4, 178 },
						{ 6237, 350, -50, 0, 1, 178 },
						{ 6238, 353, -50, 2, 3, 178 },
						{ 6241, 354, -50, 2, 4, 178 },
						{ 6244, 355, -50, 3, 3, 178 },
						{ 6248, 356, -50, 3, 2, 178 },
						{ 6252, 355, -50, 1, 3, 178 },
						{ 6254, 354, -50, 10, 2, 178 },
						{ 6265, 353, -50, 3, 4, 178 },
						{ 6269, 354, -50, 2, 1, 178 },
						{ 6272, 353, -50, 9, 3, 178 },
						{ 6282, 354, -50, 2, 4, 178 },
						{ 6283, 355, -50, 2, 4, 178 },
						{ 6284, 357, -50, 3, 3, 178 },
						{ 6285, 359, -50, 3, 2, 178 },
						{ 6286, 360, -50, 3, 2, 178 },
						{ 6288, 361, -50, 2, 5, 178 },
						{ 6290, 362, -50, 2, 5, 178 },
						{ 6291, 363, -50, 2, 5, 178 },
						{ 6293, 365, -50, 1, 4, 178 },
						{ 6294, 366, -50, 1, 4, 178 },
						{ 6296, 367, -50, 1, 3, 178 },
						{ 6297, 369, -50, 3, 3, 178 },
						{ 6301, 370, -50, 3, 3, 178 },
						{ 6302, 371, -50, 8, 3, 178 },
						{ 6309, 369, -50, 5, 1, 178 },
						{ 6313, 366, -50, 2, 2, 178 },
						{ 6314, 364, -50, 5, 2, 178 },
						{ 6316, 362, -50, 3, 2, 178 },
						{ 6319, 361, -50, 11, 5, 178 },
						{ 6321, 360, -50, 7, 1, 178 },
						{ 6331, 363, -50, 4, 3, 178 },
						{ 6335, 364, -50, 13, 4, 178 },
						{ 6345, 362, -50, 5, 2, 178 },
						{ 6353, 358, -50, 7, 3, 178 },
						{ 6361, 359, -50, 16, 3, 178 },
						{ 6378, 357, -50, 14, 6, 178 },
						{ 6399, 352, -50, 14, 10, 178 },
						{ 6405, 363, -50, 9, 1, 178 },
						{ 6408, 365, -50, 7, 1, 178 },
						{ 6411, 367, -50, 3, 8, 178 },
						{ 6413, 375, -50, 5, 8, 178 },
						{ 6411, 383, -50, 6, 2, 178 },
						{ 6411, 386, -50, 5, 1, 178 },
						{ 6411, 388, -50, 3, 1, 178 },
						{ 6409, 390, -50, 3, 4, 178 },
						{ 6408, 395, -50, 4, 3, 178 },
						{ 6409, 398, -50, 5, 3, 178 },
						{ 6412, 401, -50, 4, 2, 178 },
						{ 6414, 402, -50, 3, 3, 178 },
						{ 6414, 406, -50, 2, 2, 178 },
						{ 6414, 408, -50, 1, 0, 178 },
						{ 6416, 410, -50, 6, 4, 178 },
						{ 6418, 407, -50, 1, 2, 178 },
						{ 6416, 415, -50, 4, 8, 178 },
						{ 6417, 422, -50, 4, 5, 178 },
						{ 6419, 426, -50, 3, 6, 178 },
						{ 6420, 432, -50, 4, 6, 178 },
						{ 6418, 439, -50, 3, 6, 178 },
						{ 6416, 444, -50, 3, 2, 178 },
						{ 6414, 447, -50, 5, 6, 178 },
						{ 6411, 454, -50, 7, 21, 178 },
						{ 6400, 474, -50, 15, 5, 178 },
						{ 6398, 470, -50, 4, 6, 178 },
						{ 6396, 468, -50, 1, 6, 178 },
						{ 6394, 463, -50, 1, 8, 178 },
						{ 6392, 462, -50, 1, 4, 178 },
						{ 6391, 452, -50, 2, 9, 178 },
						{ 6389, 454, -50, 2, 5, 178 },
						{ 6387, 456, -50, 2, 3, 178 },
						{ 6386, 445, -50, 1, 9, 178 },
						{ 6387, 444, -50, 2, 8, 178 },
						{ 6390, 445, -50, 2, 5, 178 },
						{ 6383, 445, -50, 4, 6, 178 },
						{ 6372, 444, -50, 10, 4, 178 },
						{ 6364, 443, -50, 9, 4, 178 },
						{ 6362, 446, -50, 4, 3, 178 },
						{ 6359, 448, -50, 4, 3, 178 },
						{ 6351, 449, -50, 10, 3, 178 },
						{ 6349, 453, -50, 7, 1, 178 },
						{ 6348, 454, -50, 3, 3, 178 },
						{ 6346, 458, -50, 2, 2, 178 },
						{ 6345, 460, -50, 2, 4, 178 },
						{ 6340, 461, -50, 3, 3, 178 },
						{ 6338, 463, -50, 1, 3, 178 },
						{ 6336, 465, -50, 2, 2, 178 },
						{ 6333, 466, -50, 4, 4, 178 },
						{ 6330, 469, -50, 2, 4, 178 },
						{ 6328, 473, -50, 5, 10, 178 },
						{ 6329, 484, -50, 5, 33, 178 },
						{ 6331, 518, -50, 4, 5, 178 },
						{ 6333, 522, -50, 5, 5, 178 },
						{ 6335, 528, -50, 3, 3, 178 },
						{ 6336, 531, -50, 4, 4, 178 },
						{ 6338, 536, -50, 4, 2, 178 },
						{ 6340, 539, -50, 4, 2, 178 },
						{ 6342, 542, -50, 4, 3, 178 },
						{ 6344, 546, -50, 4, 1, 178 },
						{ 6345, 547, -50, 4, 2, 178 },
						{ 6347, 549, -50, 5, 5, 178 },
						{ 6349, 555, -50, 4, 2, 178 },
						{ 6351, 558, -50, 4, 3, 178 },
						{ 6353, 562, -50, 3, 3, 178 },
						{ 6355, 564, -50, 3, 5, 178 },
						{ 6358, 568, -50, 4, 7, 178 },
						{ 6363, 568, -50, 2, 4, 178 },
						{ 6366, 567, -50, 10, 4, 178 },
						{ 6377, 569, -50, 3, 3, 178 },
						{ 6378, 570, -50, 5, 3, 178 },
						{ 6383, 569, -50, 3, 3, 178 },
						{ 6387, 569, -50, 8, 4, 178 },
						{ 6396, 569, -50, 7, 3, 178 },
						{ 6403, 571, -50, 2, 3, 178 },
						{ 6405, 573, -50, 2, 2, 178 },
						{ 6406, 576, -50, 0, 1, 178 },
						{ 6409, 576, -50, 1, 3, 178 },
						{ 6408, 577, -50, 0, 1, 178 },
						{ 6410, 577, -50, 2, 3, 178 },
						{ 6413, 578, -50, 2, 3, 178 },
						{ 6416, 579, -50, 1, 2, 178 },
						{ 6418, 578, -50, 16, 3, 178 },
						{ 6430, 576, -50, 4, 1, 178 },
						{ 6433, 565, -50, 2, 10, 178 },
						{ 6434, 564, -50, 13, 2, 178 },
						{ 6444, 563, -50, 3, 2, 178 },
						{ 6447, 562, -50, 4, 2, 178 },
						{ 6452, 561, -50, 12, 3, 178 },
						{ 6458, 559, -50, 6, 1, 178 },
						{ 6461, 555, -50, 3, 3, 178 },
						{ 6462, 551, -50, 2, 3, 178 },
						{ 6464, 548, -50, 2, 2, 178 },
						{ 6466, 546, -50, 16, 3, 178 },
						{ 6475, 544, -50, 1, 1, 178 },
						{ 6480, 540, -50, 6, 5, 178 },
						{ 6484, 538, -50, 6, 1, 178 },
						{ 6487, 536, -50, 3, 1, 178 },
						{ 6489, 534, -50, 2, 1, 178 },
						{ 6490, 532, -50, 2, 1, 178 },
						{ 6491, 530, -50, 7, 2, 178 },
						{ 6493, 529, -50, 6, 0, 178 },
						{ 6496, 527, -50, 4, 1, 178 },
						{ 6499, 524, -50, 3, 2, 178 },
						{ 6500, 522, -50, 3, 2, 178 },
						{ 6501, 520, -50, 3, 2, 178 },
						{ 6499, 514, -50, 3, 4, 178 },
						{ 6498, 510, -50, 5, 3, 178 },
						{ 6497, 500, -50, 8, 8, 178 },
						{ 6484, 448, -50, 65, 51, 178 },
						{ 6508, 500, -50, 5, 3, 178 },
						{ 6523, 500, -50, 17, 3, 178 },
						{ 6528, 504, -50, 4, 1, 178 },
					},

					#endregion
					#region 

					new("TheLostCityEntry")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(6320, 476, -50) },
						{ "Music", "ParoxysmusLair" },

						{ 6316, 472, -128, 8, 8, 256 },
						{ 6303, 509, -128, 11, 7, 256 },
					},

					#endregion
				},

				#endregion

				#region Bravehorn's drinking pool

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Bravehorn's drinking pool" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1321, 2342, -15) },
					{ "Music", "Invalid" },

					{ 1302, 2331, -128, 38, 22, 256 },
				},

				#endregion

				#region Huntsman's Forest

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Huntsman's Forest" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1655, 610, 22) },
					{ "Music", "Invalid" },

					{ 1595, 550, -128, 120, 120, 256 },
				},

				#endregion

				#region Cove

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cove" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2275, 1210, 0) },
					{ "Music", "Cove" },

					{ 2200, 1110, 0, 50, 50, 128 },
					{ 2200, 1160, 0, 86, 86, 128 },
				},

				#endregion

				#region Britain

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Britain" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1495, 1629, 10) },
					{ "Music", "Britain1" },

					{ 1416, 1498, -10, 324, 279, 138 },
					{ 1500, 1408, 0, 46, 90, 128 },
					{ 1385, 1538, -10, 31, 239, 138 },
					{ 1416, 1777, 0, 324, 60, 128 },
					{ 1385, 1777, 0, 31, 130, 128 },
					{ 1093, 1538, 0, 292, 369, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1496, 1611, 10) },
						{ "Music", "Britain1" },

						{ 1492, 1602, 0, 8, 19, 128 },
						{ 1500, 1610, 0, 8, 16, 128 },
						{ 1576, 1584, 0, 24, 16, 128 },
						{ 1456, 1512, 0, 16, 16, 128 },
						{ 1472, 1512, 0, 8, 8, 128 },
						{ 1424, 1712, 0, 8, 24, 128 },
						{ 1432, 1712, 0, 8, 12, 128 },
						{ 1486, 1684, 0, 8, 8, 128 },
						{ 1494, 1676, 0, 8, 24, 128 },
						{ 1608, 1584, 0, 24, 8, 128 },
						{ 1616, 1576, 0, 8, 8, 128 },
						{ 1544, 1760, 0, 16, 16, 128 },
						{ 1560, 1760, 0, 8, 8, 128 },

						#region Custeau Perron House

						new("CusteauPerronHouseRegion")
						{
							{ "Disabled", false },
							{ "RuneName", "" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "Custeau Perron House" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(1652, 1548, 20) },
							{ "Music", "Britain1" },

							{ 1648, 1544, -128, 8, 8, 256 },
						},

						#endregion
					},

					#endregion
					#region A Wheatfield in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1135, 1791, 0) },
						{ "Music", "Britain1" },

						{ 1120, 1776, -128, 32, 32, 256 },
					},

					#endregion
					#region A Wheatfield in Britain 2

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1199, 1823, 0) },
						{ "Music", "Britain1" },

						{ 1184, 1808, -128, 32, 32, 256 },
					},

					#endregion
					#region A Wheatfield in Britain 3

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 3" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1231, 1887, 0) },
						{ "Music", "Britain1" },

						{ 1216, 1872, -128, 32, 32, 256 },
					},

					#endregion
					#region A Carrot Field in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Carrot Field in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1215, 1723, 0) },
						{ "Music", "Britain1" },

						{ 1208, 1712, -128, 16, 24, 256 },
					},

					#endregion
					#region An Onion Field in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "An Onion Field in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1231, 1723, 0) },
						{ "Music", "Britain1" },

						{ 1224, 1712, -128, 16, 24, 256 },
					},

					#endregion
					#region A Cabbage Field in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Cabbage Field in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1183, 1683, 0) },
						{ "Music", "Britain1" },

						{ 1176, 1672, -128, 16, 23, 256 },
					},

					#endregion
					#region A Turnip Field in Britain 1

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Turnip Field in Britain 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1199, 1683, 0) },
						{ "Music", "Britain1" },

						{ 1192, 1672, -128, 16, 24, 256 },
					},

					#endregion
					#region A Wheatfield in Britain 4

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 4" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1120, 1624, 0) },
						{ "Music", "Britain1" },

						{ 1104, 1608, -128, 32, 32, 256 },
					},

					#endregion
					#region A Wheatfield in Britain 5

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Britain 5" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1152, 1576, 0) },
						{ "Music", "Britain1" },

						{ 1136, 1560, -128, 32, 32, 256 },
					},

					#endregion
					#region A Turnip Field in Britain 2

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Turnip Field in Britain 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1216, 1604, 0) },
						{ "Music", "Britain1" },

						{ 1208, 1592, -128, 16, 24, 256 },
					},

					#endregion
					#region A Carrot Field in Britain 2

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Carrot Field in Britain 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1232, 1604, 0) },
						{ "Music", "Britain1" },

						{ 1224, 1592, -128, 16, 24, 256 },
					},

					#endregion
				},

				#endregion

				#region Britain Graveyard

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Britain Graveyard" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1384, 1492, 10) },
					{ "Music", "Invalid" },

					{ 1333, 1441, -128, 84, 82, 256 },
				},

				#endregion

				#region Jhelom Islands

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Jhelom Islands" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1383, 3815, 0) },
					{ "Music", "Jhelom" },

					{ 1111, 3567, -128, 33, 21, 256 },
					{ 1078, 3588, -128, 124, 121, 256 },
					{ 1224, 3592, -128, 309, 473, 256 },

					#region Jhelom

					new("TownRegion")
					{
						{ "Disabled", false },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Jhelom" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1383, 3815, 0) },
						{ "Music", "Jhelom" },

						{ 1303, 3670, -20, 189, 225, 148 },
						{ 1338, 3895, -20, 74, 28, 148 },
						{ 1383, 3951, -20, 109, 94, 148 },

						#region 

						new("GenericRegion")
						{
							{ "RuneName", "" },
							{ "NoLogoutDelay", true },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(1360, 3816, 0) },
							{ "Music", "Jhelom" },

							{ 1352, 3800, -20, 16, 32, 148 },
							{ 1368, 3808, -20, 8, 16, 148 },
							{ 1432, 3768, -20, 32, 8, 148 },
							{ 1440, 3776, -20, 24, 8, 148 },
						},

						#endregion
					},

					#endregion
				},

				#endregion

				#region Minoc

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2466, 544, 0) },
					{ "Music", "Minoc" },

					{ 2411, 366, -128, 135, 241, 256 },
					{ 2548, 495, -128, 72, 55, 256 },
					{ 2564, 585, -128, 3, 42, 256 },
					{ 2567, 585, -128, 61, 61, 256 },
					{ 2499, 627, -128, 68, 63, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2477, 401, 15) },
						{ "Music", "Minoc" },

						{ 2457, 397, -128, 40, 8, 256 },
						{ 2465, 405, -128, 8, 8, 256 },
						{ 2481, 405, -128, 8, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Haven Island

				new("NoHousingRegion")
				{
					{ "SmartChecking", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Haven Island" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3627, 2606, 0) },
					{ "Music", "Invalid" },

					{ 3314, 2345, -128, 500, 750, 256 },

					#region Enhanced Tracking Skill

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Enhanced Tracking Skill" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(3564, 2720, -5) },
						{ "Music", "Invalid" },

						{ 3314, 2345, -128, 500, 750, 256 },

						#region the New Haven Bard

						new("GenericRegion")
						{
							{ "RuneName", "" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "the New Haven Bard" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(3415, 2602, 50) },
							{ "Music", "Invalid" },

							{ 3410, 2598, -128, 11, 9, 256 },
							{ 3408, 2607, -128, 7, 5, 256 },
						},

						#endregion
						#region the New Haven Carpenter

						new("GenericRegion")
						{
							{ "RuneName", "" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "the New Haven Carpenter" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(3443, 2640, 22) },
							{ "Music", "Invalid" },

							{ 3439, 2633, -128, 9, 15, 256 },
						},

						#endregion
						#region the New Haven Farm

						new("GenericRegion")
						{
							{ "RuneName", "" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "the New Haven Farm" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(3723, 2622, 36) },
							{ "Music", "Invalid" },

							{ 3703, 2567, -128, 40, 110, 256 },
						},

						#endregion
						#region Old Haven Training

						new("GenericRegion")
						{
							{ "RuneName", "" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "Old Haven Training" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(3646, 2493, 0) },
							{ "Music", "Invalid" },

							{ 3589, 2443, -128, 115, 100, 256 },
						},

						#endregion
						#region Haven Mountains

						new("GenericRegion")
						{
							{ "RuneName", "" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "Haven Mountains" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(3481, 2757, 26) },
							{ "Music", "Invalid" },

							{ 3426, 2732, -128, 110, 50, 256 },
						},

						#endregion
						#region Haven Moongate

						new("GuardedRegion")
						{
							{ "Disabled", false },
							{ "RuneName", "" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "Haven Moongate" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(3763, 2769, 5) },
							{ "Music", "Invalid" },

							{ 3755, 2763, -128, 17, 13, 256 },
						},

						#endregion
						#region New Haven

						new("TownRegion")
						{
							{ "Disabled", false },
							{ "RuneName", "New Haven City" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "New Haven" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(3503, 2574, 14) },
							{ "Music", "InTown01" },

							{ 3423, 2480, -128, 111, 143, 256 },

							#region the New Haven Alchemist

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "the New Haven Alchemist" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3461, 2568, 30) },
								{ "Music", "InTown01" },

								{ 3457, 2564, -128, 9, 9, 256 },
							},

							#endregion
							#region the New Haven Warrior

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "the New Haven Warrior" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3527, 2520, 20) },
								{ "Music", "InTown01" },

								{ 3518, 2510, -128, 19, 21, 256 },
								{ 3521, 2531, -128, 22, 22, 256 },
								{ 3521, 2553, -128, 15, 11, 256 },

								#region the New Haven Bowyer

								new("GenericRegion")
								{
									{ "RuneName", "" },
									{ "NoLogoutDelay", false },
									{ "SpawnZLevel", "Lowest" },
									{ "ExcludeFromParentSpawns", false },
									{ "Name", "the New Haven Bowyer" },
									{ "Priority", 50 },
									{ "GoLocation", new Point3D(3534, 2539, 20) },
									{ "Music", "InTown01" },

									{ 3529, 2534, -128, 10, 10, 256 },
								},

								#endregion
							},

							#endregion
							#region the New Haven Tailor

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "the New Haven Tailor" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3497, 2551, 15) },
								{ "Music", "InTown01" },

								{ 3492, 2546, -128, 10, 11, 256 },
							},

							#endregion
							#region the New Haven Mapmaker

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "the New Haven Mapmaker" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3493, 2591, 7) },
								{ "Music", "InTown01" },

								{ 3490, 2588, -128, 7, 7, 256 },
							},

							#endregion
							#region the New Haven Mage

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "the New Haven Mage" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3477, 2496, 66) },
								{ "Music", "InTown01" },

								{ 3464, 2488, -128, 27, 16, 256 },
							},

							#endregion
							#region the New Haven Inn

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", true },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "the New Haven Inn" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3501, 2517, 22) },
								{ "Music", "InTown01" },

								{ 3493, 2513, -128, 17, 9, 256 },
								{ 3503, 2523, -128, 7, 7, 256 },
							},

							#endregion
							#region the New Haven Docks

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "the New Haven Docks" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3514, 2602, -5) },
								{ "Music", "InTown01" },

								{ 3501, 2587, -128, 26, 30, 256 },
								{ 3512, 2581, -128, 7, 12, 256 },
							},

							#endregion
							#region the New Haven Bank

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "the New Haven Bank" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3485, 2572, 15) },
								{ "Music", "InTown01" },

								{ 3479, 2565, -128, 13, 15, 256 },
							},

							#endregion
							#region Springs And Things Workshop

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "Springs And Things Workshop" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3459, 2528, 48) },
								{ "Music", "InTown01" },

								{ 3455, 2523, -128, 9, 10, 256 },
							},

							#endregion
							#region Haven Dojo

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "Haven Dojo" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3420, 2518, 15) },
								{ "Music", "InTown01" },

								{ 3415, 2513, -128, 11, 11, 256 },
							},

							#endregion
							#region Gorge's Shop

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "Gorge's Shop" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3468, 2539, 35) },
								{ "Music", "InTown01" },

								{ 3465, 2535, -128, 7, 8, 256 },
							},

							#endregion
							#region Haven Library

							new("GenericRegion")
							{
								{ "RuneName", "" },
								{ "NoLogoutDelay", false },
								{ "SpawnZLevel", "Lowest" },
								{ "ExcludeFromParentSpawns", false },
								{ "Name", "Haven Library" },
								{ "Priority", 50 },
								{ "GoLocation", new Point3D(3472, 2493, 66) },
								{ "Music", "InTown01" },

								{ 3464, 2488, -128, 16, 11, 256 },
							},

							#endregion
						},

						#endregion
					},

					#endregion
				},

				#endregion

				#region Trinsic

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Trinsic" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1867, 2780, 0) },
					{ "Music", "Trinsic" },

					{ 1856, 2636, -128, 75, 28, 256 },
					{ 1816, 2664, -128, 283, 231, 256 },
					{ 2099, 2782, -128, 18, 25, 256 },
					{ 1970, 2895, -128, 47, 32, 256 },
					{ 1796, 2696, 0, 20, 67, 128 },
					{ 1800, 2796, 0, 16, 52, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1845, 2736, 0) },
						{ "Music", "Trinsic" },

						{ 1834, 2728, -128, 22, 16, 256 },
						{ 2024, 2784, -128, 16, 20, 256 },
						{ 2026, 2804, -128, 14, 2, 256 },
						{ 2024, 2806, -128, 16, 7, 256 },
						{ 1923, 2786, -128, 12, 22, 256 },
						{ 1935, 2786, -128, 7, 14, 256 },
					},

					#endregion
				},

				#endregion

				#region Vesper

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Vesper" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2899, 676, 0) },
					{ "Music", "Vesper" },

					{ 2893, 598, -128, 121, 50, 256 },
					{ 2816, 648, -128, 249, 365, 256 },
					{ 2734, 944, -128, 82, 4, 256 },
					{ 2728, 948, -128, 88, 53, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2960, 880, 0) },
						{ "Music", "Vesper" },

						{ 2952, 864, -128, 16, 32, 256 },
						{ 2968, 872, -128, 8, 16, 256 },
						{ 2776, 952, -128, 16, 32, 256 },
						{ 2768, 960, -128, 8, 16, 256 },
						{ 2892, 901, -128, 16, 19, 256 },
						{ 2908, 904, -128, 8, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Yew

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Yew" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(546, 992, 0) },
					{ "Music", "Yew" },

					{ 92, 656, 0, 349, 225, 128 },
					{ 441, 746, 0, 216, 135, 128 },
					{ 258, 881, 0, 399, 380, 128 },
					{ 657, 922, 0, 42, 307, 128 },
					{ 657, 806, 0, 17, 28, 128 },
					{ 718, 874, 0, 38, 22, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(612, 820, 0) },
						{ "Music", "Yew" },

						{ 600, 808, 0, 24, 24, 128 },
					},

					#endregion
					#region A Field of Sheep in Yew 1

					new("GenericRegion")
					{
						{ "RuneName", "A Field of Sheep in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Field of Sheep in Yew 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(675, 939, 0) },
						{ "Music", "Yew" },

						{ 664, 928, -128, 22, 22, 256 },
					},

					#endregion
					#region A Field of Sheep in Yew 2

					new("GenericRegion")
					{
						{ "RuneName", "A Field of Sheep in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Field of Sheep in Yew 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(675, 1179, 0) },
						{ "Music", "Yew" },

						{ 664, 1168, -128, 22, 22, 256 },
					},

					#endregion
					#region A Farm in Yew

					new("GenericRegion")
					{
						{ "RuneName", "A Farm in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Farm in Yew" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(571, 1099, 0) },
						{ "Music", "Yew" },

						{ 560, 1088, -128, 22, 22, 256 },
					},

					#endregion
					#region A Wheatfield in Yew 1

					new("GenericRegion")
					{
						{ "RuneName", "A Wheatfield in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Yew 1" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(567, 1239, 0) },
						{ "Music", "Yew" },

						{ 560, 1232, -128, 16, 16, 256 },
					},

					#endregion
					#region A Wheatfield in Yew 2

					new("GenericRegion")
					{
						{ "RuneName", "A Wheatfield in Yew" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "A Wheatfield in Yew 2" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(375, 1191, 0) },
						{ "Music", "Yew" },

						{ 368, 1176, -128, 14, 32, 256 },
					},

					#endregion
				},

				#endregion

				#region Wind

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Wind" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5223, 190, 5) },
					{ "Music", "Wind" },

					{ 5294, 19, -128, 72, 120, 256 },
					{ 5132, 58, -128, 81, 68, 256 },
					{ 5197, 126, -128, 55, 78, 256 },
					{ 5132, 3, -128, 70, 55, 256 },
					{ 5252, 112, -128, 42, 58, 256 },
					{ 5213, 98, -128, 39, 28, 256 },
					{ 5279, 57, -128, 15, 55, 256 },
					{ 5252, 170, -128, 32, 8, 256 },
					{ 5286, 25, -128, 8, 32, 256 },
					{ 5252, 178, -128, 20, 5, 256 },
					{ 5252, 183, -128, 10, 10, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5171, 19, 27) },
						{ "Music", "Wind" },

						{ 5159, 15, -128, 25, 9, 256 },
						{ 5159, 24, -128, 9, 16, 256 },
						{ 5175, 24, -128, 9, 8, 256 },
						{ 5212, 159, -128, 9, 24, 256 },
						{ 5221, 171, -128, 7, 12, 256 },
						{ 5206, 164, -128, 6, 15, 256 },
						{ 5303, 28, -128, 16, 14, 256 },
					},

					#endregion
				},

				#endregion

				#region Serpent's Hold

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Serpent's Hold" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3010, 3371, 15) },
					{ "Music", "Serpents" },

					{ 2868, 3324, 0, 205, 195, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2968, 3408, 15) },
						{ "Music", "Serpents" },

						{ 2960, 3400, 0, 16, 16, 128 },
						{ 2968, 3416, 0, 8, 16, 128 },
						{ 3008, 3450, 0, 14, 14, 128 },
					},

					#endregion
				},

				#endregion

				#region Skara Brae

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Skara Brae" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(632, 2233, 0) },
					{ "Music", "Skarabra" },

					{ 538, 2107, -128, 150, 190, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(608, 2244, 0) },
						{ "Music", "Skarabra" },

						{ 600, 2232, -128, 16, 24, 256 },
						{ 592, 2240, -128, 8, 16, 256 },
						{ 616, 2240, -128, 8, 16, 256 },
						{ 552, 2168, -128, 16, 24, 256 },
						{ 568, 2168, -128, 8, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Nujel'm

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Nujel'm" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3732, 1279, 0) },
					{ "Music", "Nujelm" },

					{ 3475, 1000, 0, 360, 435, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(3744, 1195, 0) },
						{ "Music", "Nujelm" },

						{ 3736, 1184, 0, 16, 23, 128 },
						{ 3728, 1192, 0, 8, 15, 128 },
						{ 3728, 1288, 0, 23, 15, 128 },
						{ 3728, 1303, 0, 16, 9, 128 },
						{ 3728, 1312, 0, 12, 8, 128 },
						{ 3728, 1320, 0, 16, 23, 128 },
						{ 3744, 1328, 0, 7, 15, 128 },
						{ 3760, 1216, 0, 12, 24, 128 },
						{ 3772, 1220, 0, 4, 16, 128 },
						{ 3776, 1224, 0, 8, 8, 128 },
						{ 3728, 1248, 0, 16, 24, 128 },
						{ 3744, 1248, 0, 8, 8, 128 },
						{ 3744, 1264, 0, 8, 8, 128 },
					},

					#endregion
				},

				#endregion

				#region Moonglow

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Moonglow" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4442, 1172, 0) },
					{ "Music", "Moonglow" },

					{ 4535, 844, 0, 20, 3, 128 },
					{ 4530, 847, 0, 31, 61, 128 },
					{ 4521, 914, 0, 56, 49, 128 },
					{ 4278, 915, 0, 54, 19, 128 },
					{ 4283, 944, 0, 53, 73, 128 },
					{ 4377, 1015, -10, 59, 37, 138 },
					{ 4367, 1050, 0, 142, 145, 128 },
					{ 4539, 1036, 0, 27, 18, 128 },
					{ 4517, 1053, 0, 23, 22, 128 },
					{ 4389, 1198, 0, 47, 39, 128 },
					{ 4466, 1211, 0, 32, 25, 128 },
					{ 4700, 1108, 0, 17, 18, 128 },
					{ 4656, 1127, 0, 26, 13, 128 },
					{ 4678, 1162, 0, 25, 25, 128 },
					{ 4613, 1196, 0, 23, 22, 128 },
					{ 4646, 1212, 0, 14, 17, 128 },
					{ 4677, 1214, 0, 26, 22, 128 },
					{ 4622, 1316, 0, 22, 24, 128 },
					{ 4487, 1353, 0, 59, 21, 128 },
					{ 4477, 1374, 0, 69, 35, 128 },
					{ 4659, 1387, 0, 40, 40, 128 },
					{ 4549, 1482, 0, 29, 27, 128 },
					{ 4405, 1451, 0, 23, 23, 128 },
					{ 4483, 1468, 0, 21, 13, 128 },

					#region The Scholar's Inn

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "The Scholar's Inn" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(4388, 1164, 0) },
						{ "Music", "Moonglow" },

						{ 4384, 1152, 0, 8, 24, 128 },
						{ 4392, 1160, 0, 16, 8, 128 },
						{ 4400, 1152, 0, 8, 8, 128 },
						{ 4480, 1056, 0, 8, 16, 128 },
						{ 4488, 1060, 0, 4, 8, 128 },
						{ 4476, 1060, 0, 4, 8, 128 },
					},

					#endregion
				},

				#endregion

				#region Magincia

				new("NewMaginciaRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Magincia" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3714, 2220, 20) },
					{ "Music", "Magincia" },

					{ 3632, 2032, -128, 50, 70, 256 },
					{ 3624, 2162, -128, 95, 70, 256 },
					{ 3752, 2046, -128, 52, 48, 256 },
					{ 3680, 2045, -128, 72, 49, 256 },
					{ 3652, 2094, -128, 160, 180, 256 },
					{ 3649, 2256, -128, 54, 47, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(3693, 2254, 20) },
						{ "Music", "Magincia" },

						{ 3687, 2246, -128, 12, 16, 256 },
					},

					#endregion
				},

				#endregion

				#region Buccaneer's Den

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Buccaneer's Den" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2706, 2163, 0) },
					{ "Music", "Bucsden" },

					{ 2612, 2057, -128, 164, 210, 256 },
					{ 2604, 2065, 0, 8, 189, 128 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2720, 2088, 0) },
						{ "Music", "Bucsden" },

						{ 2712, 2080, -128, 16, 16, 256 },
						{ 2712, 2096, -128, 8, 8, 256 },
						{ 2664, 2232, -128, 24, 8, 256 },
						{ 2672, 2240, -128, 16, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Covetous

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(2499, 916, 10) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Covetous" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5456, 1862, 0) },
					{ "Music", "Dungeon9" },

					{ 5376, 1793, -128, 201, 255, 256 },
					{ 5576, 1791, -128, 57, 257, 256 },
				},

				#endregion

				#region Deceit

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(4111, 429, 31) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Deceit" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5187, 635, 0) },
					{ "Music", "Dungeon9" },

					{ 5122, 518, -128, 248, 252, 256 },
				},

				#endregion

				#region Despise

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1296, 1082, 9) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Despise" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5501, 570, 59) },
					{ "Music", "Dungeon9" },

					{ 5377, 516, -128, 254, 506, 256 },
				},

				#endregion

				#region Destard

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1176, 2635, 12) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Destard" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5243, 1004, 0) },
					{ "Music", "Dungeon9" },

					{ 5120, 770, -128, 251, 258, 256 },

					#region Obsidian Wyvern

					new("ExploringDeepCreaturesRegion")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Obsidian Wyvern" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5138, 965, 0) },
						{ "Music", "Dungeon9" },

						{ 5133, 959, -128, 10, 12, 256 },
					},

					#endregion
				},

				#endregion

				#region Hythloth

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(4722, 3814, 0) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Hythloth" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5905, 22, 44) },
					{ "Music", "Dungeon9" },

					{ 5898, 2, -128, 238, 244, 256 },
				},

				#endregion

				#region Jail

				new("Jail")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Jail" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5275, 1163, 0) },
					{ "Music", "Invalid" },

					{ 5271, 1159, -128, 41, 33, 256 },
				},

				#endregion

				#region Green Acres

				new("GreenAcres")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Green Acres" },
					{ "Priority", 1 },
					{ "GoLocation", new Point3D(5445, 1153, 0) },
					{ "Music", "Invalid" },

					{ 5376, 512, -128, 767, 767, 256 },
				},

				#endregion

				#region Shame

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(512, 1559, 9) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shame" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5395, 126, 0) },
					{ "Music", "Dungeon9" },

					{ 5377, 2, -128, 257, 260, 256 },
					{ 5635, 2, -128, 260, 124, 256 },
				},

				#endregion

				#region Wrong

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(2042, 226, 14) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Wrong" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5825, 599, 0) },
					{ "Music", "Dungeon9" },

					{ 5633, 511, -128, 253, 510, 256 },

					#region 

					new("WrongLevel3")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5703, 645, 0) },
						{ "Music", "Dungeon9" },

						{ 5687, 623, -128, 33, 44, 256 },

						#region 

						new("WrongJail")
						{
							{ "EntranceLocation", new Point3D(0, 0, 0) },
							{ "EntranceMap", "" },
							{ "RuneName", "" },
							{ "NoLogoutDelay", false },
							{ "SpawnZLevel", "Lowest" },
							{ "ExcludeFromParentSpawns", false },
							{ "Name", "" },
							{ "Priority", 50 },
							{ "GoLocation", new Point3D(5703, 639, 0) },
							{ "Music", "Dungeon9" },

							{ 5700, 636, -128, 7, 7, 256 },
						},

						#endregion
					},

					#endregion
				},

				#endregion

				#region Cave 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cave 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2367, 942, 0) },
					{ "Music", "Invalid" },

					{ 2373, 900, -128, 22, 28, 256 },
					{ 2395, 903, -128, 14, 16, 256 },
					{ 2373, 928, -128, 10, 9, 256 },
					{ 2359, 927, -128, 14, 18, 256 },
				},

				#endregion

				#region Cave 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cave 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1903, 365, 0) },
					{ "Music", "Invalid" },

					{ 1887, 354, -128, 33, 23, 256 },
				},

				#endregion

				#region Cave 3

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cave 3" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1934, 316, 0) },
					{ "Music", "Invalid" },

					{ 1925, 307, -128, 18, 18, 256 },
				},

				#endregion

				#region Cave 4

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cave 4" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2345, 830, 0) },
					{ "Music", "Invalid" },

					{ 2323, 809, -128, 45, 42, 256 },
				},

				#endregion

				#region Britain Mine 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Britain Mine 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1443, 1228, 0) },
					{ "Music", "Invalid" },

					{ 1436, 1215, -128, 29, 37, 256 },
				},

				#endregion

				#region Britain Mine 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Britain Mine 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1629, 1189, 0) },
					{ "Music", "Invalid" },

					{ 1611, 1175, -128, 51, 29, 256 },
				},

				#endregion

				#region Minoc Cave 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc Cave 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2426, 177, 0) },
					{ "Music", "Invalid" },

					{ 2406, 168, -128, 22, 16, 256 },
				},

				#endregion

				#region Minoc Cave 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc Cave 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2440, 94, 0) },
					{ "Music", "Invalid" },

					{ 2418, 81, -128, 24, 32, 256 },
				},

				#endregion

				#region Minoc Cave 3

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc Cave 3" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2471, 64, 0) },
					{ "Music", "Invalid" },

					{ 2447, 39, -128, 42, 28, 256 },
				},

				#endregion

				#region Minoc Mine

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Minoc Mine" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2558, 499, 0) },
					{ "Music", "Invalid" },

					{ 2556, 501, -128, 6, 3, 256 },
					{ 2556, 474, -128, 26, 27, 256 },
				},

				#endregion

				#region Avatar Isle Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Avatar Isle Cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4605, 3815, 0) },
					{ "Music", "Invalid" },

					{ 4594, 3807, -128, 23, 17, 256 },
				},

				#endregion

				#region Ice Isle Cave 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ice Isle Cave 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4054, 440, 3) },
					{ "Music", "Invalid" },

					{ 4018, 421, -128, 49, 49, 256 },
				},

				#endregion

				#region Ice Isle Cave 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ice Isle Cave 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4030, 325, 0) },
					{ "Music", "Invalid" },

					{ 4002, 310, -128, 43, 41, 256 },
					{ 4005, 298, -128, 22, 12, 256 },
				},

				#endregion

				#region North Territory Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "North Territory Cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1984, 262, 8) },
					{ "Music", "Invalid" },

					{ 1973, 251, -128, 21, 23, 256 },
				},

				#endregion

				#region Yew Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Yew Cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(773, 1684, 0) },
					{ "Music", "Invalid" },

					{ 766, 1683, -128, 12, 14, 256 },
				},

				#endregion

				#region North Territory Mine 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "North Territory Mine 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1723, 1065, 0) },
					{ "Music", "Invalid" },

					{ 1713, 1055, -128, 40, 25, 256 },
				},

				#endregion

				#region North Territory Mine 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "North Territory Mine 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1638, 974, 0) },
					{ "Music", "Invalid" },

					{ 1604, 958, -128, 46, 51, 256 },
				},

				#endregion

				#region Mt Kendall

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Mt Kendall" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2593, 477, 60) },
					{ "Music", "Invalid" },

					{ 2552, 448, -128, 71, 59, 256 },
					{ 2547, 380, -128, 43, 68, 256 },
					{ 2590, 413, -128, 33, 35, 256 },
				},

				#endregion

				#region Covetous Mine

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Covetous Mine" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2445, 880, 0) },
					{ "Music", "Invalid" },

					{ 2429, 866, -128, 19, 45, 256 },
					{ 2448, 879, -128, 27, 28, 256 },
					{ 2456, 907, -128, 19, 34, 256 },
				},

				#endregion

				#region Terathan Keep

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(5426, 3120, -60) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Terathan Keep" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5451, 3143, -60) },
					{ "Music", "Dungeon9" },

					{ 5404, 3099, -128, 77, 68, 256 },
					{ 5120, 1530, -128, 254, 258, 256 },
				},

				#endregion

				#region Fire

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(2922, 3402, 25) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Fire" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5760, 2908, 15) },
					{ "Music", "Dungeon9" },

					{ 5635, 1285, -128, 245, 235, 256 },
				},

				#endregion

				#region Ice

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1996, 80, 15) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ice" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5210, 2322, 30) },
					{ "Music", "Dungeon9" },

					{ 5668, 130, -128, 220, 138, 256 },
					{ 5800, 319, -128, 63, 65, 256 },
					{ 5654, 300, -128, 54, 40, 256 },

					#region Ice Wyrm

					new("ExploringDeepCreaturesRegion")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Ice Wyrm" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5806, 239, -3) },
						{ "Music", "Dungeon9" },

						{ 5799, 232, -128, 15, 15, 256 },
					},

					#endregion
				},

				#endregion

				#region Mercutio The Unsavory

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Mercutio The Unsavory" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2584, 1118, 0) },
					{ "Music", "Invalid" },

					{ 2580, 1112, -128, 8, 13, 256 },
				},

				#endregion

				#region Blackthorn Dungeon

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(6430, 2678, 0) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Blackthorn Dungeon" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6430, 2678, 0) },
					{ "Music", "Dungeon9" },

					{ 6151, 2301, -128, 413, 539, 256 },
				},

				#endregion

				#region Blackthorn Castle

				new("GuardedRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Blackthorn Castle" },
					{ "Priority", 51 },
					{ "GoLocation", new Point3D(1529, 1477, 0) },
					{ "Music", "LBCastle" },

					{ 1503, 1377, -128, 79, 120, 256 },
					{ 1520, 1482, -128, 6, 23, 256 },
					{ 1466, 1407, -128, 40, 88, 256 },
				},

				#endregion

				#region Delucia

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Delucia" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5228, 3978, 37) },
					{ "Music", "Invalid" },

					{ 5123, 3942, -128, 192, 122, 256 },
					{ 5147, 4064, -128, 125, 20, 256 },
					{ 5235, 3930, -128, 80, 12, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5199, 4063, 37) },
						{ "Music", "Invalid" },

						{ 5194, 4053, -128, 10, 20, 256 },
					},

					#endregion
				},

				#endregion

				#region Papua

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Papua" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5769, 3176, 0) },
					{ "Music", "Invalid" },

					{ 5639, 3095, -128, 192, 223, 256 },
					{ 5831, 3237, -128, 20, 30, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5769, 3162, 14) },
						{ "Music", "Invalid" },

						{ 5757, 3150, -128, 24, 24, 256 },
					},

					#endregion
				},

				#endregion

				#region Wrong Entrance

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Wrong Entrance" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2043, 236, 13) },
					{ "Music", "Mountn_a" },

					{ 1939, 215, -128, 134, 137, 256 },
				},

				#endregion

				#region Covetous Entrance

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Covetous Entrance" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2499, 918, 0) },
					{ "Music", "Vesper" },

					{ 2433, 846, -128, 128, 128, 256 },
				},

				#endregion

				#region Despise Entrance

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Despise Entrance" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1298, 1081, 0) },
					{ "Music", "Invalid" },

					{ 1289, 1064, -128, 32, 37, 256 },
				},

				#endregion

				#region Despise Passage

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Despise Passage" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1380, 1114, 0) },
					{ "Music", "Invalid" },

					{ 1338, 1060, -128, 51, 62, 256 },
					{ 1354, 1122, -128, 42, 121, 256 },
					{ 1349, 1122, -128, 5, 102, 256 },
				},

				#endregion

				#region Misc Dungeons

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1492, 1641, 20) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Misc Dungeons" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6032, 1499, 0) },
					{ "Music", "Dungeon9" },

					{ 5886, 1281, -128, 257, 254, 256 },
				},

				#endregion

				#region Orc Cave

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1014, 1434, 0) },
					{ "EntranceMap", "Trammel" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Orc Cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5137, 2015, 0) },
					{ "Music", "Dungeon9" },

					{ 5281, 1283, -128, 92, 103, 256 },
					{ 5267, 1955, -128, 97, 91, 256 },
					{ 5127, 1941, -128, 37, 83, 256 },

					#region Orc Engineer

					new("ExploringDeepCreaturesRegion")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Orc Engineer" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(5313, 1969, 0) },
						{ "Music", "Dungeon9" },

						{ 5308, 1964, -128, 11, 11, 256 },
					},

					#endregion
				},

				#endregion

				#region A Cotton Field in Moonglow

				new("GenericRegion")
				{
					{ "RuneName", "A Cotton Field in Moonglow" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Cotton Field in Moonglow" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4567, 1475, 0) },
					{ "Music", "Invalid" },

					{ 4557, 1471, -128, 20, 10, 256 },
				},

				#endregion

				#region A Wheatfield in Skara Brae 1

				new("GenericRegion")
				{
					{ "RuneName", "A Wheatfield in Skara Brae" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Wheatfield in Skara Brae 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(813, 2163, 0) },
					{ "Music", "Invalid" },

					{ 796, 2152, -128, 36, 24, 256 },
				},

				#endregion

				#region A Carrot Field in Skara Brae

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Carrot Field in Skara Brae" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2254, 0) },
					{ "Music", "Invalid" },

					{ 816, 2251, -128, 16, 8, 256 },
				},

				#endregion

				#region An Onion Field in Skara Brae

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "An Onion Field in Skara Brae" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2264, 0) },
					{ "Music", "Invalid" },

					{ 816, 2261, -128, 16, 8, 256 },
				},

				#endregion

				#region A Cabbage Field in Skara Brae 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Cabbage Field in Skara Brae 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2264, 0) },
					{ "Music", "Invalid" },

					{ 816, 2271, -128, 16, 8, 256 },
				},

				#endregion

				#region A Cabbage Field in Skara Brae 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Cabbage Field in Skara Brae 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2274, 0) },
					{ "Music", "Invalid" },

					{ 816, 2281, -128, 16, 8, 256 },
				},

				#endregion

				#region A Wheatfield in Skara Brae 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Wheatfield in Skara Brae 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(843, 2352, 0) },
					{ "Music", "Invalid" },

					{ 835, 2344, -128, 16, 16, 256 },
				},

				#endregion

				#region A Cotton Field in Skara Brae

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "A Cotton Field in Skara Brae" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(823, 2355, 0) },
					{ "Music", "Invalid" },

					{ 816, 2344, -128, 16, 24, 256 },
				},

				#endregion

				#region FireIsleCasino

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "FireIsleCasino" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4047, 3371, 0) },
					{ "Music", "Invalid" },

					{ 4038, 3303, -128, 60, 64, 256 },
				},

				#endregion

				#region Shrine of Compassion

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Compassion" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1856, 872, -1) },
					{ "Music", "Invalid" },

					{ 1851, 867, -128, 14, 14, 256 },
				},

				#endregion

				#region Chaos Shrine

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Chaos Shrine" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1456, 854, 0) },
					{ "Music", "Invalid" },

					{ 1456, 840, -128, 4, 7, 256 },
				},

				#endregion

				#region Shrine of Honesty

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Honesty" },
					{ "Priority", 37 },
					{ "GoLocation", new Point3D(4217, 564, 36) },
					{ "Music", "Invalid" },

					{ 4209, 560, -128, 7, 8, 256 },
				},

				#endregion

				#region Shrine of Honor

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Honor" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1730, 3528, 3) },
					{ "Music", "Invalid" },

					{ 1721, 3525, -128, 8, 6, 256 },
				},

				#endregion

				#region Shrine of Humility

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Humility" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4276, 3699, 0) },
					{ "Music", "Invalid" },

					{ 4270, 3694, -128, 9, 9, 256 },
				},

				#endregion

				#region Shrine of Justice

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Justice" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1301, 639, 16) },
					{ "Music", "Invalid" },

					{ 1297, 629, -128, 9, 9, 256 },
				},

				#endregion

				#region Shrine of Sacrifice

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Sacrifice" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(3355, 299, 9) },
					{ "Music", "Invalid" },

					{ 3352, 286, -128, 6, 7, 256 },
				},

				#endregion

				#region Shrine of Spirituality

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Spirituality" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1589, 2485, 5) },
					{ "Music", "Invalid" },

					{ 1590, 2485, -128, 10, 11, 256 },
					{ 1599, 2488, -128, 10, 5, 256 },
				},

				#endregion

				#region Shrine of Valor

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Valor" },
					{ "Priority", 37 },
					{ "GoLocation", new Point3D(2496, 3932, 0) },
					{ "Music", "Invalid" },

					{ 2488, 3928, -128, 9, 11, 256 },
				},

				#endregion

				#region South Britannian Sea

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "South Britannian Sea" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1394, 3890, 0) },
					{ "Music", "Invalid" },

					{ 688, 3700, -128, 1412, 380, 256 },
				},

				#endregion

				#region Paroxysmus Exit

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Paroxysmus Exit" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5623, 3038, 15) },
					{ "Music", "Invalid" },

					{ 5620, 3032, -128, 10, 10, 256 },
				},

				#endregion

				#region Sanctuary Entrance

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Sanctuary Entrance" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(762, 1645, 0) },
					{ "Music", "Invalid" },

					{ 759, 1635, -128, 12, 12, 256 },
				},

				#endregion

				#region Solen Hives

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Solen Hives" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(5774, 1896, 20) },
					{ "Music", "Invalid" },

					{ 5640, 1776, -128, 295, 263, 256 },
				},

				#endregion

				#region Sea Market

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Sea Market" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(4550, 2317, -2) },
					{ "Music", "Invalid" },

					{ 4529, 2296, -128, 45, 112, 256 },
				},

				#endregion
			},

			#endregion

			#region Ilshenar

			["Ilshenar"] = new()
			{
				#region Djinn

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Djinn" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1733, 519, 8) },
					{ "Music", "Invalid" },

					{ 1724, 508, -128, 18, 22, 256 },
				},

				#endregion

				#region Twisted Weald

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1450, 1473, -23) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Twisted Weald" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2189, 1253, 0) },
					{ "Music", "DreadHornArea" },

					{ 2107, 1145, -128, 147, 135, 256 },

					#region Twisted Weald Desert

					new("DungeonRegion")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Twisted Weald Desert" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2145, 1190, -58) },
						{ "Music", "DreadHornArea" },

						{ 2139, 1187, -128, 13, 7, 256 },
						{ 2152, 1164, -128, 38, 30, 256 },
						{ 2190, 1164, -128, 35, 48, 256 },
					},

					#endregion
				},

				#endregion

				#region Sheep Farm

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Sheep Farm" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1318, 1327, -14) },
					{ "Music", "Invalid" },

					{ 1303, 1312, -128, 30, 30, 256 },
				},

				#endregion

				#region Shrine of Valor

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Valor" },
					{ "Priority", 37 },
					{ "GoLocation", new Point3D(539, 221, -36) },
					{ "Music", "Invalid" },

					{ 512, 200, -128, 32, 32, 256 },
				},

				#endregion

				#region Chaos Shrine

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Chaos Shrine" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1748, 236, 0) },
					{ "Music", "Invalid" },

					{ 1736, 224, -128, 24, 24, 256 },
				},

				#endregion

				#region Shrine of Sacrifice

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Sacrifice" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1196, 1290, -25) },
					{ "Music", "Invalid" },

					{ 1160, 1280, -128, 32, 16, 256 },
				},

				#endregion

				#region Shrine of Honesty

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Honesty" },
					{ "Priority", 37 },
					{ "GoLocation", new Point3D(725, 1355, -61) },
					{ "Music", "Invalid" },

					{ 712, 1344, -128, 24, 32, 256 },
				},

				#endregion

				#region Shrine of Compassion

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Compassion" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1223, 475, -16) },
					{ "Music", "Invalid" },

					{ 1200, 448, -128, 32, 40, 256 },
				},

				#endregion

				#region Shrine of Spirituality

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Spirituality" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1528, 1344, 0) },
					{ "Music", "Invalid" },

					{ 1520, 1336, -128, 16, 16, 256 },
				},

				#endregion

				#region Shrine of Humility

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Humility" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(284, 1016, 0) },
					{ "Music", "Invalid" },

					{ 272, 1008, -128, 24, 16, 256 },
				},

				#endregion

				#region Shrine of Honor

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Honor" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(748, 728, 0) },
					{ "Music", "Invalid" },

					{ 736, 712, -128, 24, 32, 256 },
				},

				#endregion

				#region Terort Skitas

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Terort Skitas" },
					{ "Priority", 11 },
					{ "GoLocation", new Point3D(568, 430, 41) },
					{ "Music", "Invalid" },

					{ 528, 376, -128, 96, 88, 256 },
				},

				#endregion

				#region Ver Lor Reg

				new("VerLorRegCity")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ver Lor Reg" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(840, 571, 0) },
					{ "Music", "Invalid" },

					{ 824, 552, -128, 32, 176, 256 },
					{ 752, 576, -128, 72, 128, 256 },
					{ 856, 576, -128, 48, 128, 256 },
					{ 904, 624, -128, 24, 32, 256 },
				},

				#endregion

				#region Pormir Harm

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Pormir Harm" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(745, 492, -66) },
					{ "Music", "Stones2" },

					{ 736, 480, -128, 32, 24, 256 },
				},

				#endregion

				#region Pormir Felwis

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Pormir Felwis" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(472, 352, -78) },
					{ "Music", "Invalid" },

					{ 472, 336, -128, 40, 32, 256 },
				},

				#endregion

				#region Cyclops Temple

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Cyclops Temple" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(901, 1297, -71) },
					{ "Music", "Invalid" },

					{ 880, 1256, -128, 56, 112, 256 },
				},

				#endregion

				#region Rat Fort

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Rat Fort" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(642, 845, -58) },
					{ "Music", "Invalid" },

					{ 616, 792, -128, 56, 56, 256 },
				},

				#endregion

				#region Rat Fort Cellar

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Rat Fort Cellar" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(171, 750, -28) },
					{ "Music", "Invalid" },

					{ 160, 736, -128, 32, 32, 256 },
				},

				#endregion

				#region Entrance to Central Ilshenar 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance to Central Ilshenar 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1141, 594, -80) },
					{ "Music", "Invalid" },

					{ 1136, 584, -128, 16, 16, 256 },
				},

				#endregion

				#region Entrance to Central Ilshenar 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance to Central Ilshenar 2" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1238, 583, -19) },
					{ "Music", "Invalid" },

					{ 1232, 576, -128, 8, 16, 256 },
				},

				#endregion

				#region Montor

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Montor" },
					{ "Priority", 41 },
					{ "GoLocation", new Point3D(1646, 319, 48) },
					{ "Music", "Invalid" },

					{ 1576, 272, -128, 144, 96, 256 },
					{ 1616, 200, -128, 80, 72, 256 },
					{ 1696, 144, -128, 40, 128, 256 },
					{ 1736, 144, -128, 32, 64, 256 },
				},

				#endregion

				#region Termir Flam

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Termir Flam" },
					{ "Priority", 40 },
					{ "GoLocation", new Point3D(1636, 432, 0) },
					{ "Music", "Invalid" },

					{ 1520, 368, -128, 232, 128, 256 },
					{ 1520, 296, -128, 56, 72, 256 },
					{ 1752, 440, -128, 24, 40, 256 },
				},

				#endregion

				#region Ancient Citadel

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ancient Citadel" },
					{ "Priority", 11 },
					{ "GoLocation", new Point3D(1516, 542, 85) },
					{ "Music", "Invalid" },

					{ 1448, 496, -128, 104, 104, 256 },
				},

				#endregion

				#region Alexandretta's Bowl

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Alexandretta's Bowl" },
					{ "Priority", 41 },
					{ "GoLocation", new Point3D(1396, 432, -17) },
					{ "Music", "Invalid" },

					{ 1272, 352, -128, 168, 240, 256 },
				},

				#endregion

				#region Entrance to Rock Dungeon

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance to Rock Dungeon" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(1784, 568, 100) },
					{ "Music", "Invalid" },

					{ 1784, 560, -128, 8, 16, 256 },
				},

				#endregion

				#region Rock Dungeon

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1788, 571, 70) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Rock Dungeon" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(2187, 316, -7) },
					{ "Music", "Invalid" },

					{ 2176, 288, -128, 24, 40, 256 },
					{ 2088, 8, -128, 160, 176, 256 },
				},

				#endregion

				#region Lenmir Anfinmotas

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Lenmir Anfinmotas" },
					{ "Priority", 5 },
					{ "GoLocation", new Point3D(1673, 876, -25) },
					{ "Music", "Invalid" },

					{ 1560, 792, -128, 232, 120, 256 },
				},

				#endregion

				#region Entrance to Spider Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance to Spider Cave" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(1418, 915, -19) },
					{ "Music", "Invalid" },

					{ 1400, 896, -128, 40, 24, 256 },
				},

				#endregion

				#region Spider Cave

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1420, 910, -10) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Spider Cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1785, 991, -28) },
					{ "Music", "Invalid" },

					{ 1752, 952, -128, 112, 48, 256 },
					{ 1480, 864, -128, 48, 32, 256 },
				},

				#endregion

				#region Twin Oaks

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Twin Oaks" },
					{ "Priority", 48 },
					{ "GoLocation", new Point3D(1560, 1052, 0) },
					{ "Music", "Invalid" },

					{ 1536, 1032, -128, 48, 40, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 48 },
						{ "GoLocation", new Point3D(1556, 1047, -27) },
						{ "Music", "Invalid" },

						{ 1546, 1037, -128, 21, 20, 256 },
					},

					#endregion
				},

				#endregion

				#region Reg Volon

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Reg Volon" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1363, 1055, -13) },
					{ "Music", "Invalid" },

					{ 1328, 1008, -128, 80, 112, 256 },
				},

				#endregion

				#region Entrance Spectre Dungeon

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance Spectre Dungeon" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(1363, 1033, -8) },
					{ "Music", "Invalid" },

					{ 1360, 1030, -128, 6, 4, 256 },
				},

				#endregion

				#region Spectre Dungeon

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1362, 1031, -13) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Spectre Dungeon" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(1983, 1107, -18) },
					{ "Music", "Invalid" },

					{ 1936, 1000, -128, 96, 120, 256 },
				},

				#endregion

				#region Entrance Blood Dungeon

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance Blood Dungeon" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(1747, 1227, -1) },
					{ "Music", "Invalid" },

					{ 1736, 1224, -128, 24, 16, 256 },
				},

				#endregion

				#region Blood Dungeon

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1745, 1236, -30) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Blood Dungeon" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(2114, 839, -28) },
					{ "Music", "Invalid" },

					{ 2032, 808, -128, 168, 256, 256 },
				},

				#endregion

				#region Entrance Mushroom Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance Mushroom Cave" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(1460, 1329, -25) },
					{ "Music", "Invalid" },

					{ 1448, 1320, -128, 16, 16, 256 },
				},

				#endregion

				#region Mushroom Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Mushroom Cave" },
					{ "Priority", 11 },
					{ "GoLocation", new Point3D(1476, 1494, -28) },
					{ "Music", "Invalid" },

					{ 1392, 1448, -128, 104, 120, 256 },
				},

				#endregion

				#region Lake Shire

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Lake Shire" },
					{ "Priority", 48 },
					{ "GoLocation", new Point3D(1212, 1144, -25) },
					{ "Music", "Invalid" },

					{ 1144, 1072, -128, 120, 128, 256 },
				},

				#endregion

				#region Entrance Rat Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance Rat Cave" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(1031, 1154, -24) },
					{ "Music", "Invalid" },

					{ 1024, 1152, -128, 8, 8, 256 },
				},

				#endregion

				#region Rat Cave Territory

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Rat Cave Territory" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(1345, 1511, -3) },
					{ "Music", "Invalid" },

					{ 1144, 1440, -128, 224, 144, 256 },
				},

				#endregion

				#region Ratman Cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ratman Cave" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(1348, 1511, -3) },
					{ "Music", "Invalid" },

					{ 1264, 1448, -128, 96, 128, 256 },
					{ 1152, 1456, -128, 104, 104, 256 },
				},

				#endregion

				#region Bet-Lem Reg

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Bet-Lem Reg" },
					{ "Priority", 48 },
					{ "GoLocation", new Point3D(1238, 974, -34) },
					{ "Music", "Invalid" },

					{ 1232, 936, -128, 40, 48, 256 },
				},

				#endregion

				#region Mistas

				new("TownRegion")
				{
					{ "Disabled", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Mistas" },
					{ "Priority", 48 },
					{ "GoLocation", new Point3D(818, 1073, -30) },
					{ "Music", "Invalid" },

					{ 744, 984, -128, 168, 168, 256 },
					{ 744, 1152, -128, 48, 24, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 48 },
						{ "GoLocation", new Point3D(760, 1141, -30) },
						{ "Music", "Invalid" },

						{ 752, 1131, -128, 16, 21, 256 },
					},

					#endregion
				},

				#endregion

				#region Entrance Serpentine Passage

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance Serpentine Passage" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(808, 872, 5) },
					{ "Music", "Invalid" },

					{ 800, 864, -128, 16, 16, 256 },
				},

				#endregion

				#region Serpentine Passage

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Serpentine Passage" },
					{ "Priority", 11 },
					{ "GoLocation", new Point3D(533, 1526, -25) },
					{ "Music", "Invalid" },

					{ 368, 1488, -128, 192, 104, 256 },
				},

				#endregion

				#region Ankh Dungeon

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(668, 928, -84) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ankh Dungeon" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(156, 1484, -28) },
					{ "Music", "Invalid" },

					{ 0, 1248, -128, 176, 344, 256 },
					{ 568, 1152, -128, 16, 8, 256 },
				},

				#endregion

				#region Kirin passage

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Kirin passage" },
					{ "Priority", 11 },
					{ "GoLocation", new Point3D(11, 881, -29) },
					{ "Music", "Invalid" },

					{ 0, 800, -128, 192, 400, 256 },
				},

				#endregion

				#region Entrance Lizards Passage

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance Lizards Passage" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(314, 1331, -37) },
					{ "Music", "Invalid" },

					{ 312, 1328, -128, 8, 8, 256 },
				},

				#endregion

				#region Wisp Dungeon

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(652, 1301, -60) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Wisp Dungeon" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(628, 1524, -28) },
					{ "Music", "Invalid" },

					{ 616, 1480, -128, 88, 96, 256 },
					{ 704, 1544, -128, 24, 32, 256 },
					{ 704, 1480, -128, 40, 40, 256 },
					{ 816, 1448, -128, 96, 136, 256 },
					{ 944, 1416, -128, 56, 40, 256 },
					{ 912, 1456, -128, 112, 128, 256 },
					{ 832, 1424, -128, 48, 24, 256 },
					{ 744, 1504, -128, 72, 80, 256 },
					{ 744, 1456, -128, 56, 48, 256 },
				},

				#endregion

				#region Lizard Passage

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Lizard Passage" },
					{ "Priority", 11 },
					{ "GoLocation", new Point3D(285, 1585, -27) },
					{ "Music", "Invalid" },

					{ 256, 1560, -128, 80, 32, 256 },
				},

				#endregion

				#region Exodus Dungeon

				new("ExodusDungeonRegion")
				{
					{ "EntranceLocation", new Point3D(827, 777, -80) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Exodus Dungeon" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1966, 117, -28) },
					{ "Music", "Invalid" },

					{ 1832, 16, -128, 248, 200, 256 },
				},

				#endregion

				#region Lizard Man's Huts

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Lizard Man's Huts" },
					{ "Priority", 48 },
					{ "GoLocation", new Point3D(298, 1336, -24) },
					{ "Music", "Invalid" },

					{ 264, 1280, -128, 56, 96, 256 },
				},

				#endregion

				#region Nox Tereg

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Nox Tereg" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(416, 1176, 0) },
					{ "Music", "Invalid" },

					{ 344, 1112, -128, 144, 128, 256 },
				},

				#endregion

				#region Sorcerer's Dungeon

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(546, 455, -40) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Sorcerer's Dungeon" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(429, 108, -28) },
					{ "Music", "Invalid" },

					{ 368, 0, -128, 120, 120, 256 },
					{ 200, 0, -128, 168, 112, 256 },
					{ 256, 120, -128, 32, 32, 256 },
					{ 48, 0, -128, 136, 136, 256 },
					{ 224, 112, -128, 24, 32, 256 },
				},

				#endregion

				#region Entrance Ancient Lair

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Entrance Ancient Lair" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(940, 503, -30) },
					{ "Music", "Invalid" },

					{ 936, 488, -128, 8, 8, 256 },
				},

				#endregion

				#region Ancient Lair

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(938, 494, 0) },
					{ "EntranceMap", "Ilshenar" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ancient Lair" },
					{ "Priority", 2 },
					{ "GoLocation", new Point3D(86, 744, -28) },
					{ "Music", "Invalid" },

					{ 24, 664, -128, 112, 96, 256 },
				},

				#endregion

				#region Gypsy Camp

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gypsy Camp" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(504, 536, -60) },
					{ "Music", "Invalid" },

					{ 480, 520, -128, 56, 48, 256 },
				},

				#endregion

				#region Gypsy Camp 1

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gypsy Camp 1" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1508, 620, 0) },
					{ "Music", "Invalid" },

					{ 1480, 600, -128, 56, 40, 256 },
				},

				#endregion

				#region Gypsy Camp 2

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gypsy Camp 2" },
					{ "Priority", 48 },
					{ "GoLocation", new Point3D(1612, 548, 0) },
					{ "Music", "Invalid" },

					{ 1592, 528, -128, 40, 40, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 48 },
						{ "GoLocation", new Point3D(1623, 550, -19) },
						{ "Music", "Invalid" },

						{ 1617, 546, -128, 12, 8, 256 },
					},

					#endregion
				},

				#endregion

				#region Juka Camp

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Juka Camp" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1248, 724, 0) },
					{ "Music", "Invalid" },

					{ 1192, 656, -128, 112, 136, 256 },
				},

				#endregion

				#region Gypsy Camp 3

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gypsy Camp 3" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(948, 428, 0) },
					{ "Music", "Invalid" },

					{ 928, 408, -128, 40, 40, 256 },
				},

				#endregion

				#region Gypsy Camp 4

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gypsy Camp 4" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(860, 464, 0) },
					{ "Music", "Invalid" },

					{ 840, 440, -128, 40, 48, 256 },
				},

				#endregion

				#region Gypsy Camp 5

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gypsy Camp 5" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1228, 544, 0) },
					{ "Music", "Invalid" },

					{ 1216, 520, -128, 24, 48, 256 },
					{ 1248, 568, -128, 16, 24, 256 },
				},

				#endregion

				#region Gypsy Camp 6

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gypsy Camp 6" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1392, 432, 0) },
					{ "Music", "Invalid" },

					{ 1376, 416, -128, 32, 32, 256 },
				},

				#endregion

				#region Lord Blackthorn's Ilshenar Castle

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Lord Blackthorn's Ilshenar Castle" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1084, 652, 0) },
					{ "Music", "Invalid" },

					{ 1048, 616, -128, 72, 72, 256 },
					{ 1080, 688, -128, 24, 24, 256 },
				},

				#endregion

				#region Shrine of the Virtues

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of the Virtues" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1104, 404, 0) },
					{ "Music", "Invalid" },

					{ 1072, 376, -128, 64, 56, 256 },
				},

				#endregion

				#region Pass of Karnaugh

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Pass of Karnaugh" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(908, 312, 0) },
					{ "Music", "Invalid" },

					{ 848, 288, -128, 120, 48, 256 },
					{ 936, 232, -128, 32, 56, 256 },
					{ 968, 248, -128, 120, 56, 256 },
					{ 1000, 304, -128, 24, 16, 256 },
					{ 1048, 304, -128, 136, 56, 256 },
					{ 1184, 336, -128, 64, 56, 256 },
				},

				#endregion

				#region Vinculum Inn

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Vinculum Inn" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(656, 666, -36) },
					{ "Music", "Invalid" },

					{ 658, 654, -128, 13, 24, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(664, 666, -35) },
						{ "Music", "Invalid" },

						{ 658, 654, -128, 13, 24, 256 },
					},

					#endregion
				},

				#endregion
			},

			#endregion

			#region Malas

			["Malas"] = new()
			{
				#region Gravewater Lake [Underwater]

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gravewater Lake [Underwater]" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1700, 1597, -115) },
					{ "Music", "Invalid" },

					{ 1698, 1567, -128, 5, 60, 256 },
					{ 1687, 1627, -128, 26, 26, 256 },
				},

				#endregion

				#region Gravewater Dock

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gravewater Dock" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1700, 1559, -115) },
					{ "Music", "Invalid" },

					{ 1692, 1551, -128, 16, 16, 256 },
				},

				#endregion

				#region Bedlam

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(2068, 1372, -75) },
					{ "EntranceMap", "Malas" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Bedlam" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(117, 1681, 0) },
					{ "Music", "GrizzleDungeon" },

					{ 80, 1590, -128, 130, 100, 256 },

					#region 

					new("DungeonRegion")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(86, 1674, 0) },
						{ "Music", "GrizzleDungeon" },

						{ 79, 1663, 0, 15, 22, 128 },
						{ 94, 1668, 0, 7, 14, 128 },
					},

					#endregion
				},

				#endregion

				#region Labyrinth

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1733, 978, -80) },
					{ "EntranceMap", "Malas" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Labyrinth" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(331, 1973, 0) },
					{ "Music", "Dungeon9" },

					{ 255, 1791, -128, 256, 256, 256 },
				},

				#endregion

				#region TheCitadel

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1349, 769, -95) },
					{ "EntranceMap", "Malas" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "TheCitadel" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(106, 1884, 0) },
					{ "Music", "Dungeon9" },

					{ 65, 1865, -128, 130, 125, 256 },
				},

				#endregion

				#region Luna

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Luna" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(989, 520, -50) },
					{ "Music", "Tavern04" },

					{ 945, 490, -128, 91, 61, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(995, 519, -50) },
						{ "Music", "Tavern04" },

						{ 986, 509, -128, 18, 21, 256 },
					},

					#endregion
				},

				#endregion

				#region Umbra

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Umbra" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2049, 1344, -85) },
					{ "Music", "Invalid" },

					{ 2042, 1265, -128, 48, 13, 256 },
					{ 1960, 1278, -128, 146, 135, 256 },
					{ 2004, 1413, -128, 36, 6, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(2038, 1318, -90) },
						{ "Music", "Invalid" },

						{ 2031, 1311, -128, 15, 14, 256 },
					},

					#endregion
				},

				#endregion

				#region Doom

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(2357, 1268, -102) },
					{ "EntranceMap", "Malas" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Doom" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2366, 1268, -85) },
					{ "Music", "Dungeon9" },

					{ 256, 0, -128, 256, 304, 256 },

					#region Doom Guardian Room

					new("DungeonRegion")
					{
						{ "EntranceLocation", new Point3D(0, 0, 0) },
						{ "EntranceMap", "" },
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Doom Guardian Room" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(365, 15, -1) },
						{ "Music", "Dungeon9" },

						{ 355, 5, -128, 20, 20, 256 },
					},

					#endregion
				},

				#endregion

				#region Doom Gauntlet

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(2357, 1268, -102) },
					{ "EntranceMap", "Malas" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Doom Gauntlet" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(429, 340, -1) },
					{ "Music", "Dungeon9" },

					{ 256, 304, -128, 256, 256, 256 },
				},

				#endregion

				#region Orc Fortress

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Orc Fortress" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1340, 1226, -90) },
					{ "Music", "Invalid" },

					{ 1295, 1189, -128, 100, 105, 256 },
				},

				#endregion

				#region Crystal Cave Entrance

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Crystal Cave Entrance" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1195, 515, -90) },
					{ "Music", "Invalid" },

					{ 1176, 509, -128, 43, 13, 256 },
				},

				#endregion

				#region Protected Island

				new("NoHousingRegion")
				{
					{ "SmartChecking", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Protected Island" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(2104, 380, -90) },
					{ "Music", "Invalid" },

					{ 1976, 101, -128, 472, 244, 256 },
					{ 2051, 345, -128, 305, 37, 256 },
					{ 2026, 345, -128, 25, 25, 256 },
					{ 2069, 382, -128, 75, 14, 256 },
					{ 2254, 382, -128, 98, 38, 256 },
					{ 2222, 382, -128, 32, 20, 256 },
				},

				#endregion

				#region Grand Arena

				new("NoHousingRegion")
				{
					{ "SmartChecking", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Grand Arena" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(961, 637, -90) },
					{ "Music", "Invalid" },

					{ 919, 622, -128, 43, 30, 256 },
				},

				#endregion

				#region Hanse's Hostel

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", true },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Hanse's Hostel" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1063, 1435, -90) },
					{ "Music", "Invalid" },

					{ 1049, 1420, -128, 13, 25, 256 },
					{ 1062, 1420, -128, 3, 11, 256 },
					{ 1062, 1439, -128, 3, 6, 256 },
				},

				#endregion

				#region Yomotsu Mines

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(259, 783, -1) },
					{ "EntranceMap", "Malas" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Yomotsu Mines" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(6, 118, 0) },
					{ "Music", "TokunoDungeon" },

					{ 0, 0, -128, 129, 129, 256 },
				},

				#endregion

				#region Fan Dancer's Dojo

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(977, 223, -77) },
					{ "EntranceMap", "Malas" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Fan Dancer's Dojo" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(71, 337, 0) },
					{ "Music", "TokunoDungeon" },

					{ 40, 320, -128, 170, 400, 256 },
				},

				#endregion

				#region Samurai start location

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Samurai start location" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(377, 740, 1) },
					{ "Music", "Zento" },

					{ 320, 690, -128, 115, 100, 256 },
				},

				#endregion

				#region Ninja start location

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ninja start location" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(400, 815, -1) },
					{ "Music", "Zento" },

					{ 360, 795, -128, 80, 40, 256 },
				},

				#endregion

				#region Ninja cave

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Ninja cave" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(410, 1067, 0) },
					{ "Music", "Invalid" },

					{ 360, 960, -128, 100, 215, 256 },
				},

				#endregion

				#region Isle of the Divide

				new("NoHousingRegion")
				{
					{ "SmartChecking", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Isle of the Divide" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1731, 982, -80) },
					{ "Music", "Invalid" },

					{ 1533, 849, -128, 295, 195, 256 },
				},

				#endregion
			},

			#endregion

			#region Tokuno

			["Tokuno"] = new()
			{
				#region Moongates

				new("GuardedRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Moongates" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1169, 998, 41) },
					{ "Music", "Invalid" },

					{ 1167, 996, -128, 4, 4, 256 },
					{ 799, 1201, -128, 6, 6, 256 },
					{ 267, 625, -128, 7, 7, 256 },
				},

				#endregion

				#region Zento

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Zento" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(736, 1256, 30) },
					{ "Music", "Zento" },

					{ 656, 1192, -128, 160, 128, 256 },

					#region 

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(686, 1211, 25) },
						{ "Music", "Zento" },

						{ 674, 1203, -128, 25, 16, 256 },
					},

					#endregion
				},

				#endregion

				#region Fan Dancer's Dojo

				new("NoHousingRegion")
				{
					{ "SmartChecking", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Fan Dancer's Dojo" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(977, 223, 23) },
					{ "Music", "Invalid" },

					{ 969, 194, -128, 23, 30, 256 },
				},

				#endregion

				#region Bushido Dojo

				new("NoHousingRegion")
				{
					{ "SmartChecking", true },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Bushido Dojo" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(320, 408, 32) },
					{ "Music", "Invalid" },

					{ 283, 361, -128, 68, 102, 256 },
				},

				#endregion

				#region Tokuno Docks

				new("GenericRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Tokuno Docks" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(713, 1360, 26) },
					{ "Music", "Invalid" },

					{ 650, 1350, -128, 100, 50, 256 },
				},

				#endregion
			},

			#endregion

			#region TerMur

			["TerMur"] = new()
			{
				#region Abyss

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Abyss" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(946, 72, 72) },
					{ "Music", "StygianAbyss" },

					{ 301, 328, -128, 775, 474, 256 },
					{ 780, 32, -128, 296, 296, 256 },
					{ 436, 802, -128, 304, 178, 256 },
					{ 754, 881, -128, 108, 93, 256 },
					{ 756, 1049, -128, 104, 93, 256 },
					{ 756, 1214, -128, 105, 99, 256 },
					{ 0, 684, -128, 134, 176, 256 },
					{ 0, 485, -128, 134, 162, 256 },
					{ 0, 286, -128, 137, 159, 256 },
					{ 254, 70, -128, 246, 183, 256 },

					#region Abyssal Lair

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Abyssal Lair" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(985, 366, -11) },
						{ "Music", "StygianAbyss" },

						{ 931, 314, -128, 100, 140, 256 },
					},

					#endregion
					#region Medusa's Lair

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Medusa's Lair" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(771, 928, 0) },
						{ "Music", "StygianAbyss" },

						{ 764, 894, -128, 69, 89, 256 },
						{ 816, 749, -128, 13, 16, 256 },
					},

					#endregion
					#region NPC Encampment

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "NPC Encampment" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(1090, 1130, -42) },
						{ "Music", "StygianAbyss" },

						{ 1087, 1127, -42, 9, 6, 170 },
					},

					#endregion
					#region Cavern of the Discarded

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Cavern of the Discarded" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(912, 501, -12) },
						{ "Music", "StygianAbyss" },

						{ 887, 475, -128, 113, 100, 256 },
						{ 926, 533, -128, 45, 20, 256 },
						{ 928, 543, -128, 42, 30, 256 },
						{ 929, 536, -128, 41, 28, 256 },
						{ 936, 559, -128, 36, 14, 256 },
						{ 938, 541, -128, 20, 31, 256 },
						{ 961, 527, -128, 22, 32, 256 },
					},

					#endregion
					#region Chamber of Virtue

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Chamber of Virtue" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(747, 474, -17) },
						{ "Music", "StygianAbyss" },

						{ 739, 465, -128, 21, 21, 256 },
					},

					#endregion
					#region Crimson Veins

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Crimson Veins" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(974, 164, -11) },
						{ "Music", "StygianAbyss" },

						{ 931, 135, -128, 65, 65, 256 },
					},

					#endregion
					#region Enslaved Goblins

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Enslaved Goblins" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(581, 815, -45) },
						{ "Music", "StygianAbyss" },

						{ 538, 806, -128, 75, 50, 256 },
						{ 539, 790, -128, 75, 50, 256 },
						{ 553, 791, -128, 60, 50, 256 },
						{ 560, 796, -128, 21, 21, 256 },
						{ 570, 773, -128, 43, 84, 256 },
						{ 571, 788, -128, 25, 25, 256 },
					},

					#endregion
					#region Fairy Dragon Lair

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Fairy Dragon Lair" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(888, 277, 3) },
						{ "Music", "StygianAbyss" },

						{ 849, 254, -128, 67, 43, 256 },
					},

					#endregion
					#region Fire Temple Ruins

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Fire Temple Ruins" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(519, 765, -92) },
						{ "Music", "StygianAbyss" },

						{ 474, 766, -128, 89, 23, 256 },
						{ 485, 760, -128, 68, 29, 256 },
						{ 495, 750, -128, 65, 29, 256 },
						{ 506, 750, -128, 66, 32, 256 },
						{ 520, 739, -128, 64, 32, 256 },
					},

					#endregion
					#region Fractured City

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Fractured City" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(780, 445, -15) },
						{ "Music", "StygianAbyss" },

						{ 664, 439, -128, 67, 88, 256 },
						{ 726, 505, -128, 45, 22, 256 },
						{ 712, 402, -128, 108, 58, 256 },
						{ 720, 504, -128, 26, 23, 256 },
						{ 771, 460, -128, 39, 58, 256 },
						{ 808, 480, -128, 12, 32, 256 },
					},

					#endregion
					#region Lands of the Lich

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Lands of the Lich" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(538, 656, 8) },
						{ "Music", "StygianAbyss" },

						{ 499, 575, -128, 71, 120, 256 },
						{ 554, 600, -128, 74, 42, 256 },
					},

					#endregion
					#region Lava Caldera

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Lava Caldera" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(587, 895, -73) },
						{ "Music", "StygianAbyss" },

						{ 553, 860, -128, 86, 88, 256 },
					},

					#endregion
					#region Passage of Tears

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Passage of Tears" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(685, 579, -15) },
						{ "Music", "StygianAbyss" },

						{ 638, 530, -128, 100, 105, 256 },
					},

					#endregion
					#region Secret Garden

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Secret Garden" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(462, 719, 22) },
						{ "Music", "StygianAbyss" },

						{ 413, 678, -128, 76, 71, 256 },
					},

					#endregion
					#region Serpents Lair

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Serpents Lair" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(711, 720, -11) },
						{ "Music", "StygianAbyss" },

						{ 681, 679, -128, 168, 111, 256 },
						{ 703, 656, -128, 147, 133, 256 },
						{ 785, 640, -128, 65, 135, 256 },
					},

					#endregion
					#region Silver Sapling

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Silver Sapling" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(341, 619, 26) },
						{ "Music", "StygianAbyss" },

						{ 311, 582, -128, 68, 68, 256 },
					},

					#endregion
					#region Skeletal Dragon

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Skeletal Dragon" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(675, 828, -109) },
						{ "Music", "StygianAbyss" },

						{ 643, 809, -128, 55, 63, 256 },
					},

					#endregion
					#region Sutek the Mage

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Sutek the Mage" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(924, 595, -14) },
						{ "Music", "StygianAbyss" },

						{ 901, 574, -128, 53, 40, 256 },
					},

					#endregion
				},

				#endregion

				#region Underworld

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Underworld" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1128, 1207, -2) },
					{ "Music", "HumanLevel" },

					{ 898, 808, -128, 382, 423, 256 },
				},

				#endregion

				#region Atoll Bend

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Atoll Bend" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1118, 3408, -42) },
					{ "Music", "Invalid" },

					{ 1027, 3311, -128, 197, 203, 256 },
				},

				#endregion

				#region Chicken Chase

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "Chicken Chase" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Chicken Chase" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(560, 3412, 37) },
					{ "Music", "Invalid" },

					{ 448, 3352, -128, 165, 128, 256 },
				},

				#endregion

				#region Fishermans Reach

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Fishermans Reach" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(631, 3035, 36) },
					{ "Music", "Invalid" },

					{ 547, 2835, -128, 198, 267, 256 },
				},

				#endregion

				#region Gated Isle

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Gated Isle" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(703, 3934, -31) },
					{ "Music", "Invalid" },

					{ 603, 3836, -128, 212, 207, 256 },
				},

				#endregion

				#region High Plain

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "High Plain" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(863, 2931, 38) },
					{ "Music", "Invalid" },

					{ 748, 2843, -128, 233, 207, 256 },
				},

				#endregion

				#region Holy City

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Holy City" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(997, 3869, -42) },
					{ "Music", "Holycity" },

					{ 922, 3838, -128, 149, 127, 256 },
				},

				#endregion

				#region Kepetch Waste

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Kepetch Waste" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(447, 3188, 20) },
					{ "Music", "Invalid" },

					{ 356, 3143, -128, 167, 81, 256 },
				},

				#endregion

				#region Lost Settlement

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Lost Settlement" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(526, 3822, -44) },
					{ "Music", "Invalid" },

					{ 454, 3708, -128, 194, 249, 256 },
				},

				#endregion

				#region Moongates

				new("GuardedRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Moongates" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(859, 3532, -43) },
					{ "Music", "Invalid" },

					{ 852, 3525, -128, 15, 15, 256 },
					{ 925, 3988, -128, 15, 15, 256 },
				},

				#endregion

				#region Northern Steppes

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Northern Steppes" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(822, 3063, 61) },
					{ "Music", "Invalid" },

					{ 694, 3020, -128, 260, 128, 256 },
				},

				#endregion

				#region Raptor Island

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Raptor Island" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(816, 3778, -42) },
					{ "Music", "Invalid" },

					{ 712, 3765, -128, 167, 64, 256 },
					{ 728, 3809, -128, 111, 114, 256 },
					{ 769, 3721, -128, 203, 47, 256 },
					{ 808, 3692, -128, 122, 40, 256 },
				},

				#endregion

				#region Royal City

				new("TownRegion")
				{
					{ "Disabled", false },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Royal City" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(750, 3440, -20) },
					{ "Music", "RoyalCity" },

					{ 624, 3296, -128, 303, 287, 256 },

					#region Queen's Palace

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Queen's Palace" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(748, 3357, 55) },
						{ "Music", "QueenPalace" },

						{ 717, 3339, -128, 68, 73, 256 },
					},

					#endregion
					#region Royal City Inn

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", true },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Royal City Inn" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(742, 3473, -20) },
						{ "Music", "RoyalCity" },

						{ 724, 3463, -128, 37, 20, 256 },
						{ 747, 3480, -128, 13, 13, 256 },
					},

					#endregion
				},

				#endregion

				#region Royal Park

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Royal Park" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(711, 3255, -42) },
					{ "Music", "Invalid" },

					{ 632, 3233, -128, 176, 35, 256 },
					{ 753, 3161, -128, 237, 131, 256 },
				},

				#endregion

				#region Slith Valley

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Slith Valley" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1078, 3331, -42) },
					{ "Music", "Invalid" },

					{ 1028, 3263, -128, 193, 141, 256 },
				},

				#endregion

				#region Spider Island

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Spider Island" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1115, 3730, -42) },
					{ "Music", "Invalid" },

					{ 1063, 3695, -128, 118, 84, 256 },
				},

				#endregion

				#region Stygian Dragon Lair

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Stygian Dragon Lair" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(367, 155, 0) },
					{ "Music", "StygianDragon" },

					{ 258, 90, -128, 231, 155, 256 },
				},

				#endregion

				#region Talon Point

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Talon Point" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(676, 3831, -39) },
					{ "Music", "Invalid" },

					{ 632, 3830, -128, 98, 55, 256 },
					{ 629, 3786, -128, 76, 40, 256 },
					{ 656, 3750, -128, 51, 41, 256 },
				},

				#endregion

				#region Toxic Desert

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(1051, 3022, 37) },
					{ "EntranceMap", "TerMur" },
					{ "RuneName", "Toxic Desert" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Toxic Desert" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(1047, 2980, 62) },
					{ "Music", "Invalid" },

					{ 1010, 2929, 37, 146, 120, 91 },
				},

				#endregion

				#region Void Island

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Void Island" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(440, 3577, 38) },
					{ "Music", "Invalid" },

					{ 314, 3473, -128, 267, 226, 256 },
				},

				#endregion

				#region Volcano

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Volcano" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(407, 3010, -24) },
					{ "Music", "Invalid" },

					{ 298, 2895, -128, 201, 222, 256 },
				},

				#endregion

				#region Walled Circus

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Walled Circus" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(370, 3273, 0) },
					{ "Music", "Invalid" },

					{ 291, 3207, -128, 154, 147, 256 },
				},

				#endregion

				#region Waterfall Point

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Waterfall Point" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(661, 2894, 39) },
					{ "Music", "Invalid" },

					{ 635, 2862, -128, 96, 64, 256 },
				},

				#endregion

				#region Shrine of Singularity

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Shrine of Singularity" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(995, 3806, 0) },
					{ "Music", "CodexShrine" },

					{ 978, 3786, -128, 40, 52, 256 },
				},

				#endregion

				#region Tomb of Kings

				new("GenericRegion")
				{
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Tomb of Kings" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(35, 236, -5) },
					{ "Music", "Invalid" },

					{ 0, 14, -128, 182, 246, 256 },

					#region Bridge

					new("GenericRegion")
					{
						{ "RuneName", "" },
						{ "NoLogoutDelay", false },
						{ "SpawnZLevel", "Lowest" },
						{ "ExcludeFromParentSpawns", false },
						{ "Name", "Tomb of Kings Bridge" },
						{ "Priority", 50 },
						{ "GoLocation", new Point3D(37, 33, -5) },
						{ "Music", "Invalid" },

						{ 32, 29, -128, 11, 9, 256 },
					},

					#endregion
				},

				#endregion

				#region Great Ape Lair

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Great Ape Lair" },
					{ "Priority", 100 },
					{ "GoLocation", new Point3D(926, 1464, 0) },
					{ "Music", "Invalid" },

					{ 832, 1363, -25, 106, 111, 153 },
				},

				#endregion

				#region Zipactriotl Lair

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Zipactriotl Lair" },
					{ "Priority", 100 },
					{ "GoLocation", new Point3D(896, 2304, -19) },
					{ "Music", "Invalid" },

					{ 831, 2239, -25, 128, 128, 153 },
				},

				#endregion

				#region Myrmidex Queen Lair

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Myrmidex Queen Lair" },
					{ "Priority", 100 },
					{ "GoLocation", new Point3D(766, 2306, 0) },
					{ "Music", "Invalid" },

					{ 704, 2248, -25, 124, 115, 153 },
				},

				#endregion

				#region Myrmidex Battleground

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Myrmidex Battleground" },
					{ "Priority", 100 },
					{ "GoLocation", new Point3D(853, 1784, 0) },
					{ "Music", "Invalid" },

					{ 828, 1762, -25, 195, 158, 153 },
				},

				#endregion

				#region Kotl City

				new("DungeonRegion")
				{
					{ "EntranceLocation", new Point3D(0, 0, 0) },
					{ "EntranceMap", "" },
					{ "RuneName", "" },
					{ "NoLogoutDelay", false },
					{ "SpawnZLevel", "Lowest" },
					{ "ExcludeFromParentSpawns", false },
					{ "Name", "Kotl City" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(542, 2473, 0) },
					{ "Music", "Invalid" },

					{ 435, 2270, -25, 240, 220, 153 },
				},

				#endregion

				#region Crystal Lotus Puzzle Region

				new("GenericRegion")
				{
					{ "Name", "Crystal Lotus Puzzle Region" },
					{ "Priority", 50 },
					{ "GoLocation", new Point3D(0, 0, 0) },
					{ "Music", "Invalid" },

					{ 945, 2858, -128, 66, 62, 256 },
				},

				#endregion
			},

			#endregion
		};
	}
}
