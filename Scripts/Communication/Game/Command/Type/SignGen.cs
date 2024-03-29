﻿using Server.Items;

using System.Collections.Generic;

namespace Server.Commands
{
	public static class SignGen
	{
		private class SignEntry
		{
			public TextDefinition m_Text;
			public Point3D m_Location;
			public int m_ItemID;
			public int m_Map;

			public SignEntry(TextDefinition text, Point3D pt, int itemID, int mapLoc)
			{
				m_Text = text;
				m_Location = pt;
				m_ItemID = itemID;
				m_Map = mapLoc;
			}
		}

		private static readonly SignEntry[] m_Entries =
		{
			#region Felucca & Trammel

			new(1016093, new(373, 904, -1), 3032, 0),
			new(1016315, new(2769, 976, 0), 2996, 0),
			new(1016095, new(2634, 2088, 10), 3016, 0),
			new(1016417, new(2632, 2102, 10), 2991, 0),
			new(1016082, new(2666, 2096, 5), 3026, 0),
			new(1016216, new(2664, 2192, 4), 3025, 0),
			new(1016138, new(2706, 2152, 0), 2988, 0),
			new(1016345, new(2714, 2104, 0), 2996, 0),
			new(1016346, new(2688, 2232, -4), 3011, 0),
			new(1016083, new(2712, 2178, 0), 3023, 0),
			new(1016217, new(2744, 2248, -4), 3019, 0),
			new(1023083, new(2732, 2192, 0), 3084, 0),
			new(1016274, new(2840, 873, 0), 3019, 0),
			new(1016361, new(2840, 878, 0), 2981, 0),
			new(1016421, new(2840, 801, 0), 3007, 0),
			new(1016342, new(2864, 731, 0), 3053, 0),
			new(1016316, new(2857, 856, 0), 3016, 0),
			new(1016347, new(2860, 801, 0), 3021, 0),
			new(1016091, new(2848, 929, 0), 3025, 0),
			new(1016293, new(2856, 910, 0), 3075, 0),
			new(1016333, new(2896, 673, 0), 3083, 0),
			new(1016286, new(2894, 664, 0), 2990, 0),
			new(1016356, new(2888, 721, 0), 3009, 0),
			new(1016296, new(2905, 720, 0), 3018, 0),
			new(1016335, new(2904, 737, 0), 3035, 0),
			new(1016308, new(2904, 794, 0), 2983, 0),
			new(1016294, new(2919, 680, 0), 3050, 0),
			new(1016325, new(2912, 680, 0), 2990, 0),
			new(1016312, new(2913, 808, 0), 2992, 0),
			new(1016142, new(2928, 852, 0), 2987, 0),
			new(1016328, new(2916, 905, 0), 3011, 0),
			new(1016361, new(2969, 624, 0), 2982, 0),
			new(1016288, new(2958, 712, 0), 3140, 0),
			new(1016414, new(2952, 929, 0), 2993, 0),
			new(1016275, new(2977, 640, 0), 3020, 0),
			new(1016416, new(2976, 875, 0), 2995, 0),
			new(1016305, new(2976, 817, 0), 3057, 0),
			new(1016326, new(3000, 833, 0), 3013, 0),
			new(1016327, new(2993, 824, 0), 2994, 0),
			new(1016373, new(2995, 768, 0), 2980, 0),
			new(1016289, new(3000, 777, 0), 2985, 0),
			new(1016120, new(3024, 777, 0), 3025, 0),
			new(1023083, new(2890, 3479, 15), 3084, 0),
			new(1016256, new(2888, 3502, 10), 2981, 0),
			new(1016244, new(2909, 3496, 10), 2986, 0),
			new(1016249, new(2912, 3517, 10), 2999, 0),
			new(1016390, new(2931, 3512, 10), 2984, 0),
			new(1016285, new(2976, 3406, 15), 2995, 0),
			new(1016246, new(2994, 3440, 15), 2988, 0),
			new(1016247, new(3005, 3360, 15), 2990, 0),
			new(1016220, new(2977, 3360, 15), 2980, 0),
			new(1016248, new(3024, 3430, 15), 3007, 0),
			new(1016268, new(3016, 3386, 15), 3019, 0),
			new(1016077, new(3009, 3360, 15), 3014, 0),
			new(1016245, new(3054, 3408, 15), 3053, 0),
			new(1016255, new(3053, 3376, 15), 3006, 0),
			new(1016199, new(3561, 1174, 9), 3025, 0),
			new(1016204, new(3554, 1175, 0), 2985, 0),
			new(1016203, new(3551, 1194, 0), 3006, 0),
			new(1016205, new(3551, 1178, 0), 3026, 0),
			new(1016202, new(3551, 1186, 0), 3016, 0),
			new(1016224, new(3554, 1199, 0), 3015, 0),
			new(1016239, new(3704, 1213, 0), 2997, 0),
			new(1016057, new(3743, 1207, 0), 3004, 0),
			new(1022993, new(3720, 1224, 0), 2993, 0),
			new(1016232, new(3740, 1314, 0), 2995, 0),
			new(1016099, new(3720, 1380, 0), 2987, 0),
			new(1016198, new(3732, 1394, 0), 3025, 0),
			new(1016201, new(3753, 1240, 14), 3085, 0),
			new(1016357, new(3748, 1256, 0), 3012, 0),
			new(1016271, new(3784, 1264, 0), 2981, 0),
			new(1016192, new(3784, 1231, 0), 3011, 0),
			new(1016091, new(3776, 1196, 0), 3026, 0),
			new(1016153, new(3778, 1176, 0), 3010, 0),
			new(1016171, new(3673, 2138, 24), 3053, 0),
			new(1016300, new(3673, 2184, 30), 3009, 0),
			new(1016263, new(3672, 2232, 20), 2981, 0),
			new(1016307, new(3669, 2261, 20), 2998, 0),
			new(1016362, new(3704, 2167, 20), 2995, 0),
			new(1016284, new(3681, 2176, 20), 2980, 0),
			new(1016139, new(3685, 2231, 20), 2988, 0),
			new(1016121, new(3682, 2261, 20), 3060, 0),
			new(1016051, new(3728, 2160, 20), 3084, 0),
			new(1016091, new(3720, 2184, 20), 3026, 0),
			new(1016173, new(3712, 2215, 20), 2989, 0),
			new(1016128, new(3736, 2230, 20), 3011, 0),
			new(1016178, new(3712, 2242, 20), 3081, 0),
			new(1016049, new(3753, 2232, 20), 2980, 0),
			new(1016107, new(4548, 873, 37), 3048, 0),
			new(1016502, new(4414, 1096, 0), 2992, 0),
			new(1016165, new(4390, 1072, 0), 2980, 0),
			new(1016185, new(4396, 1088, 0), 2988, 0),
			new(1016331, new(4400, 1115, 0), 3007, 0),
			new(1016497, new(4400, 1132, 0), 2985, 0),
			new(1016354, new(4407, 1168, 0), 2996, 0),
			new(1016146, new(4419, 1136, 0), 3014, 0),
			new(1016186, new(4424, 1108, 0), 3013, 0),
			new(1016506, new(4424, 1062, 0), 3019, 0),
			new(1016241, new(4436, 1168, 0), 3008, 0),
			new(1016091, new(4477, 1130, 0), 3026, 0),
			new(1016111, new(4478, 1092, 0), 3026, 0),
			new(1016238, new(4459, 1072, 0), 2982, 0),
			new(1016188, new(4451, 1096, 0), 2990, 0),
			new(1016116, new(4469, 1176, 0), 3084, 0),
			new(1016187, new(4483, 1072, 0), 2996, 0),
			new(1016184, new(4528, 1069, 0), 3017, 0),
			new(1016076, new(4537, 1376, 23), 3025, 0),
			new(1016076, new(4518, 1402, 21), 3026, 0),
			new(1016324, new(4308, 1012, 0), 3026, 0),
			new(1016149, new(4632, 1200, 0), 3051, 0),
			new(1016367, new(520, 986, 0), 3025, 0),
			new(1016434, new(547, 832, 0), 3026, 0),
			new(1016143, new(544, 970, 0), 2987, 0),
			new(1016436, new(544, 1008, 0), 2985, 0),
			new(1016317, new(557, 1000, 0), 2979, 0),
			new(1016364, new(563, 976, 0), 3022, 0),
			new(1016067, new(576, 1009, 0), 2991, 0),
			new(1016106, new(636, 856, 5), 3026, 0),
			new(1016432, new(624, 886, 2), 3025, 0),
			new(1016091, new(616, 984, 0), 3026, 0),
			new(1016355, new(568, 2177, 0), 3011, 0),
			new(1016055, new(595, 2152, 0), 3084, 0),
			new(1016229, new(576, 2145, 0), 3055, 0),
			new(1016189, new(592, 2170, 0), 3007, 0),
			new(1016060, new(578, 2128, 0), 3024, 0),
			new(1016069, new(582, 2192, 0), 2986, 0),
			new(1016066, new(593, 2216, 0), 3022, 0),
			new(1016266, new(592, 2225, 0), 3019, 0),
			new(1016066, new(600, 2209, 0), 3021, 0),
			new(1016069, new(592, 2185, 0), 2985, 0),
			new(1016258, new(576, 2209, 0), 3023, 0),
			new(1016084, new(633, 2168, 0), 2992, 0),
			new(1016091, new(617, 2144, 0), 3026, 0),
			new(1016111, new(616, 2154, 0), 3023, 0),
			new(1016111, new(610, 2168, 0), 3024, 0),
			new(1016267, new(608, 2273, 0), 2997, 0),
			new(1016299, new(616, 2232, 0), 2995, 0),
			new(1016141, new(621, 2224, 0), 2988, 0),
			new(1016208, new(632, 2185, 0), 3015, 0),
			new(1016168, new(609, 2184, 0), 2990, 0),
			new(1016127, new(648, 2168, 0), 3008, 0),
			new(1016191, new(656, 2144, 0), 2990, 0),
			new(1016251, new(648, 2192, 0), 2982, 0),
			new(1016277, new(5305, 94, 19), 2989, 0),
			new(1016278, new(5204, 4060, 37), 2995, 0),
			new(1016365, new(5217, 4019, 48), 3020, 0),
			new(1016439, new(5228, 4008, 37), 3016, 0),
			new(1016430, new(5270, 3988, 36), 3084, 0),
			new(1016148, new(5291, 3982, 37), 2989, 0),
			new(1016262, new(5304, 3997, 37), 3000, 0),
			new(1016431, new(5674, 3139, 10), 3084, 0),
			new(1016264, new(5671, 3148, 21), 3009, 0),
			new(1016261, new(5678, 3286, 10), 2999, 0),
			new(1016290, new(5703, 3210, 10), 2991, 0),
			new(1016348, new(5720, 3205, 19), 3013, 0),
			new(1016360, new(5698, 3289, 24), 2986, 0),
			new(1016193, new(5744, 3209, 16), 2980, 0),
			new(1016313, new(5740, 3216, 6), 2987, 0),
			new(1016403, new(5730, 3204, 19), 2990, 0),
			new(1016391, new(5729, 3250, 26), 2984, 0),
			new(1016036, new(5740, 3267, 13), 3019, 0),
			new(1016318, new(5774, 3175, 20), 2996, 0),
			new(1016215, new(5804, 3282, 16), 2994, 0),
			new(1016349, new(5802, 3287, 13), 3007, 0),
			new(1016260, new(1306, 1761, 25), 2999, 0),
			new(1023075, new(1342, 1744, 20), 3076, 0),
			new(1016322, new(1370, 1584, 30), 3008, 0),
			new(1016072, new(1360, 1776, 15), 3053, 0),
			new(1016262, new(1386, 1664, 30), 3000, 0),
			new(1016369, new(1432, 1656, 10), 3073, 0),
			new(1016353, new(1433, 1600, 20), 2992, 0),
			new(1016071, new(1420, 1595, 30), 3025, 0),
			new(1016332, new(1425, 1584, 30), 3054, 0),
			new(1016221, new(1421, 1638, 50), 7977, 0),
			new(1016503, new(1470, 1643, 34), 7977, 0),
			new(1016503, new(1480, 1643, 34), 7977, 0),
			new(1016499, new(1570, 1526, 46), 7977, 0),
			new(1016505, new(1539, 1527, 45), 7977, 0),
			new(1016419, new(1459, 1622, 35), 4762, 0),
			new(1016419, new(1428, 1622, 32), 4764, 0),
			new(1016419, new(1489, 1627, 34), 4762, 0),
			new(1016504, new(1428, 1622, 37), 4765, 0),
			new(1016311, new(1428, 1546, 30), 3015, 0),
			new(1016302, new(1436, 1693, 0), 3084, 0),
			new(1016340, new(1424, 1747, 10), 2997, 0),
			new(1016291, new(1432, 1730, 20), 3011, 0),
			new(1016145, new(1446, 1654, 10), 3008, 0),
			new(1016126, new(1460, 1610, 20), 2979, 0),
			new(1016320, new(1454, 1600, 20), 3086, 0),
			new(1016320, new(1441, 1584, 20), 3086, 0),
			new(1016280, new(1440, 1611, 20), 3023, 0),
			new(1016163, new(1454, 1561, 30), 3004, 0),
			new(1016222, new(1458, 1683, 0), 3009, 0),
			new(1016323, new(1470, 1696, 0), 2982, 0),
			new(1016073, new(1468, 1676, 0), 3020, 0),
			new(1016295, new(1449, 1728, 1), 2986, 0),
			new(1016046, new(1444, 1670, 10), 3070, 0),
			new(1016338, new(1474, 1520, 20), 2996, 0),
			new(1016137, new(1480, 1610, 20), 2987, 0),
			new(1016269, new(1501, 1627, 25), 2996, 0),
			new(1016265, new(1489, 1587, 32), 3007, 0),
			new(1016359, new(1488, 1572, 30), 2990, 0),
			new(1016225, new(1472, 1584, 20), 3022, 0),
			new(1016178, new(1477, 1600, 20), 3082, 0),
			new(1016281, new(1502, 1689, 20), 3011, 0),
			new(1016304, new(1493, 1724, 5), 2966, 0),
			new(1022993, new(1480, 1746, 0), 2993, 0),
			new(1016110, new(1508, 1662, 20), 3013, 0),
			new(1016074, new(1505, 1576, 20), 3026, 0),
			new(1016287, new(1516, 1553, 36), 3000, 0),
			new(1016155, new(1328, 3771, 0), 3083, 0),
			new(1016334, new(1368, 3829, 0), 2995, 0),
			new(1016038, new(1360, 3786, 0), 2981, 0),
			new(1016098, new(1368, 3752, 0), 3007, 0),
			new(1016098, new(1362, 3768, 0), 3008, 0),
			new(1016080, new(1368, 3713, 0), 3033, 0),
			new(1016049, new(1368, 3733, 0), 2979, 0),
			new(1016111, new(1391, 3832, 0), 3026, 0),
			new(1016111, new(1400, 3825, 0), 3025, 0),
			new(1016334, new(1376, 3814, 0), 2995, 0),
			new(1016158, new(1400, 3769, 0), 2965, 0),
			new(1016117, new(1385, 3712, 0), 3016, 0),
			new(1016091, new(1425, 3704, 0), 3026, 0),
			new(1016122, new(1425, 3824, 0), 2992, 0),
			new(1016123, new(1411, 3808, 0), 2984, 0),
			new(1016194, new(1434, 3800, 0), 3020, 0),
			new(1016314, new(1433, 3776, 0), 3012, 0),
			new(1016157, new(1409, 3792, 0), 2988, 0),
			new(1016420, new(1424, 3859, 0), 3015, 0),
			new(1016159, new(1427, 3993, 10), 2990, 0),
			new(1016237, new(1449, 3752, 0), 2998, 0),
			new(1016242, new(1448, 3721, 0), 3007, 0),
			new(1016206, new(1440, 3746, 0), 3059, 0),
			new(1016242, new(1448, 3721, 0), 3007, 0),
			new(1016133, new(1440, 3864, 0), 3029, 0),
			new(1016154, new(1449, 3864, 0), 3008, 0),
			new(1016154, new(1464, 3849, 0), 3007, 0),
			new(1016343, new(1443, 3993, 10), 3010, 0),
			new(1016114, new(1464, 4024, 0), 2985, 0),
			new(1016038, new(1458, 4008, 0), 2982, 0),
			new(1016214, new(1441, 4010, 10), 3085, 0),
			new(1016085, new(1472, 3872, 0), 3008, 0),
			new(1016350, new(1555, 1665, 37), 2982, 0),
			new(1016374, new(1549, 1776, 10), 3012, 0),
			new(1016089, new(1592, 1562, 20), 3077, 0),
			new(1016151, new(1590, 1665, 22), 2990, 0),
			new(1016236, new(1575, 1721, 46), 3024, 0),
			new(1016045, new(1640, 1688, 32), 3007, 0),
			new(1016091, new(1505, 1528, 40), 3026, 0),
			new(1016156, new(1376, 3752, 0), 3026, 0),
			new(1016091, new(624, 2112, 0), 3025, 0),
			new(1016067, new(561, 1016, 0), 2992, 0),
			new(1016298, new(2970, 3432, 15), 3012, 0),
			new(1016111, new(3019, 768, 0), 3026, 0),
			new(1016272, new(2863, 989, 0), 3025, 0),
			new(1023050, new(4686, 1423, 0), 3050, 0),
			new(1016249, new(3043, 3463, 25), 2999, 0),
			new(1016119, new(3011, 3464, 15), 3012, 0),
			new(1016091, new(2968, 3367, 15), 3025, 0),
			new(1016368, new(3712, 2136, 20), 2984, 0),
			new(1016104, new(1614, 1636, 40), 7977, 0),
			new(1016088, new(1400, 1622, 50), 7976, 0),
			new(1016047, new(1400, 1630, 50), 7976, 0),
			new(1016250, new(1495, 1643, 35), 7976, 0),
			new(1016226, new(1495, 1640, 35), 7976, 0),
			new(1016500, new(1584, 1527, 56), 4764, 0),
			new(1016233, new(1584, 1527, 52), 4765, 0),
			new(1016233, new(1546, 1624, 23), 4765, 0),
			new(1016233, new(1546, 1624, 27), 4764, 0),
			new(1016233, new(1565, 1584, 43), 4759, 0),
			new(1016233, new(1532, 1705, 35), 4766, 0),
			new(1016233, new(1537, 1668, 35), 4761, 0),
			new(1016233, new(1561, 1670, 36), 4762, 0),
			new(1016233, new(1566, 1678, 35), 4765, 0),
			new(1016233, new(1559, 1695, 45), 4759, 0),
			new(1016233, new(1599, 1571, 36), 4766, 0),
			new(1016233, new(1599, 1571, 33), 4761, 0),
			new(1016233, new(1572, 1600, 29), 4761, 0),
			new(1016059, new(1595, 1678, 22), 4765, 0),
			new(1016059, new(1595, 1678, 25), 4759, 0),
			new(1016059, new(1617, 1679, 35), 4762, 0),
			new(1016059, new(1562, 1720, 50), 4764, 0),
			new(1016059, new(1664, 1592, 15), 4765, 0),
			new(1016059, new(1676, 1625, 10), 4765, 0),
			new(1016059, new(1640, 1680, 33), 4759, 0),
			new(1016059, new(1667, 1559, 38), 4765, 0),
			new(1016059, new(1664, 1574, 20), 4765, 0),
			new(1016059, new(1558, 1753, 30), 4766, 0),
			new(1016310, new(1552, 1728, 37), 4762, 0),
			new(1016310, new(1552, 1728, 34), 4765, 0),
			new(1016310, new(1532, 1705, 33), 4765, 0),
			new(1016310, new(1485, 1709, 15), 4765, 0),
			new(1016310, new(1488, 1731, 15), 4761, 0),
			new(1016310, new(1468, 1732, 15), 4764, 0),
			new(1016310, new(1441, 1732, 19), 4764, 0),
			new(1016310, new(1415, 1736, 25), 4764, 0),
			new(1016504, new(1415, 1736, 31), 4761, 0),
			new(1016504, new(1424, 1701, 18), 4765, 0),
			new(1016504, new(1422, 1672, 25), 4765, 0),
			new(1016504, new(1439, 1585, 35), 4766, 0),
			new(1016504, new(1435, 1570, 45), 4765, 0),
			new(1016504, new(1432, 1538, 47), 4765, 0),
			new(1016500, new(1432, 1538, 42), 4764, 0),
			new(1016500, new(1498, 1536, 46), 4765, 0),
			new(1016500, new(1519, 1525, 55), 4764, 0),
			new(1016396, new(1362, 1756, 30), 4762, 0),
			new(1016393, new(1362, 1756, 23), 4764, 0),
			new(1016393, new(1375, 1811, 18), 4761, 0),
			new(1016393, new(1355, 1835, 16), 4759, 0),
			new(1016393, new(1392, 1892, 9), 4761, 0),
			new(1016393, new(1314, 1749, 26), 4764, 0),
			new(1016393, new(1262, 1742, 9), 4764, 0),
			new("Only a mind at peace can approach the shrine", new(1580, 2484, 4), 7976, 0),
			new("A calm mind may travel where others may not tread", new(1604, 2484, 7), 7976, 0),
			new(1016401, new(1262, 1742, 15), 4762, 0),
			new(1016398, new(1262, 1742, 12), 4766, 0),
			new(1016402, new(1392, 1892, 13), 4764, 0),
			new(1016398, new(1392, 1892, 18), 4765, 0),
			new(1016398, new(1355, 1835, 16), 4765, 0),
			new(1016398, new(1375, 1811, 14), 4766, 0),
			new(1016398, new(1362, 1756, 27), 4765, 0),
			new(1016394, new(1643, 1521, 50), 4764, 0),
			new(1016292, new(1557, 1613, 21), 3025, 0),
			new(1016377, new(1600, 1589, 20), 2995, 0),
			new(1016352, new(1610, 1592, 0), 3012, 0),
			new(1016376, new(1619, 1769, 70), 3024, 0),
			new(1016223, new(1602, 1720, 20), 3020, 0),
			new(1016018, new(1660, 1648, 0), 3010, 0),
			new(1016352, new(1632, 1586, 0), 3011, 0),
			new(1016091, new(1634, 1673, 26), 3026, 0),
			new(1016050, new(1819, 2825, 0), 3083, 0),
			new(1016152, new(1850, 2777, 0), 3024, 0),
			new(1016371, new(1841, 2744, 0), 2996, 0),
			new(1016408, new(1828, 2743, 0), 2999, 0),
			new(1016107, new(1852, 2709, 10), 2989, 0),
			new(1016392, new(1855, 2688, 0), 3074, 0),
			new(1016392, new(1844, 2688, 0), 3074, 0),
			new(1016078, new(1851, 2800, -8), 3020, 0),
			new(1016048, new(1881, 2814, 6), 2980, 0),
			new(1016252, new(1899, 2664, 0), 3008, 0),
			new(1016407, new(1904, 2686, 10), 3083, 0),
			new(1016406, new(1912, 2713, 20), 3025, 0),
			new(1016405, new(1914, 2813, 0), 2988, 0),
			new(1016344, new(1901, 2814, 0), 3010, 0),
			new(1016091, new(1896, 2842, 20), 3026, 0),
			new(1016147, new(1933, 2776, 10), 3016, 0),
			new(1016079, new(1942, 2736, 10), 3075, 0),
			new(1016179, new(1939, 2701, 30), 3023, 0),
			new(1016058, new(1938, 2694, 20), 3024, 0),
			new(1016058, new(1947, 2691, 30), 3024, 0),
			new(1016319, new(1942, 2791, 0), 3011, 0),
			new(1016058, new(1959, 2695, 20), 3024, 0),
			new(1016212, new(2002, 2728, 30), 2966, 0),
			new(1016118, new(2005, 2813, -1), 3000, 0),
			new(1016037, new(1986, 2846, 15), 2982, 0),
			new(1016404, new(1997, 2864, 10), 3023, 0),
			new(1016372, new(2000, 2888, 5), 2985, 0),
			new(1016351, new(2030, 2813, 9), 2996, 0),
			new(1016259, new(2037, 2843, 0), 3059, 0),
			new(1016112, new(2221, 1193, 4), 3023, 0),
			new(1016375, new(2224, 1165, 0), 3007, 0),
			new("Bank of Cove", new(2232, 1199, 0), 3083, 0),
			new(1016313, new(2248, 1227, 0), 2987, 0),
			new(1016181, new(2428, 536, 0), 3024, 0),
			new(1016329, new(2459, 492, 15), 3053, 0),
			new(1016341, new(2459, 432, 15), 3020, 0),
			new(1016358, new(2442, 416, 15), 2986, 0),
			new(1016336, new(2432, 555, 0), 3003, 0),
			new(1016124, new(2472, 458, 15), 2984, 0),
			new(1016337, new(2482, 440, 15), 3076, 0),
			new(1016279, new(2474, 405, 15), 3012, 0),
			new(1016422, new(2525, 578, 0), 3008, 0),
			new(1016262, new(2521, 379, 23), 3000, 0),
			new(1016339, new(2510, 482, 15), 2992, 0),
			new(1016309, new(2500, 440, 15), 3028, 0),
			new(1016363, new(2523, 536, 0), 3026, 0),
			new(1016313, new(2576, 603, 0), 2988, 0),
			new(1016052, new(2505, 560, 0), 3084, 0),
			new(1016366, new(2534, 560, 0), 3020, 0),
			new(1016306, new(2470, 569, 5), 3016, 0),
			new(1016409, new(2016, 2753, 30), 3076, 0),
			new(1016129, new(618, 1152, 0), 3022, 0),
			new(1016130, new(626, 1040, 0), 2998, 0),
			new(1016053, new(3768, 1313, 0), 3083, 0),
			new(1016164, new(5351, 64, 15), 3024, 0),
			new(1016425, new(5267, 131, 20), 2987, 0),
			new(1016240, new(5250, 184, 22), 2965, 0),
			new(1016321, new(5236, 155, 15), 2966, 0),
			new(1016427, new(5222, 183, 8), 2995, 0),
			new(1016170, new(5217, 124, 0), 3014, 0),
			new(1016426, new(5200, 98, 5), 2982, 0),
			new(1016243, new(5168, 35, 22), 2995, 0),
			new(1016169, new(5158, 107, 5), 3019, 0),
			new(1016424, new(5152, 77, 28), 2990, 0),
			new(1016229, new(745, 2160, 0), 3056, 0),

			#endregion

			#region Felucca

			new(1016061, new(3608, 2609, 0), 3025, 1),
			new(1016197, new(3602, 2584, 0), 2980, 1),
			new(1023049, new(3632, 2537, 0), 3049, 1),
			new(1016211, new(3646, 2512, 0), 3018, 1),
			new(1016207, new(3617, 2480, 0), 2966, 1),
			new(1016042, new(3634, 2648, 0), 2994, 1),
			new(1016140, new(3625, 2616, 0), 2988, 1),
			new(1016162, new(3634, 2584, 0), 3020, 1),
			new(1016057, new(3658, 2536, 0), 3036, 1),
			new(1016115, new(3675, 2480, 0), 3004, 1),
			new(1016276, new(3665, 2657, 4), 3012, 1),
			new(1016283, new(3670, 2624, 0), 2996, 1),
			new(1016132, new(3647, 2600, 0), 3016, 1),
			new(1016022, new(3656, 2592, 0), 2982, 1),
			new(1016054, new(3690, 2520, 0), 3084, 1),
			new(1016270, new(3720, 2650, 20), 2985, 1),

			#endregion

			#region Trammel

			new("The Shakin' Bakery", new(3632, 2537, 0), 2979, 2),
			new("A Stitch in Time Tailor Shop", new(3680, 2480, 0), 2982, 2),
			new("The Prime Cut Butcher Shop", new(3720, 2650, 20), 2985, 2),
			new("The Healers of Haven", new(3658, 2585, 0), 2988, 2),
			new("The Little Shop of Magic", new(3632, 2577, 19), 2989, 2),
			new("Carpenters of Haven", new(3646, 2512, 0), 2992, 2),
			new("The Bountiful Harvest Inn", new(3670, 2624, 0), 2996, 2),
			new("Mapmakers of Haven", new(3634, 2640, 0), 2998, 2),
			new("The Albatross Bar and Grill", new(3665, 2657, 4), 3012, 2),
			new("The Haven Blacksmith", new(3645, 2609, 0), 3015, 2),
			new(1016162, new(3666, 2600, 0), 3020, 2),
			new("Uzeraan's mansion", new(3613, 2585, 0), 3025, 2),
			new("Haven Clockworks and Tinker Shop", new(3666, 2512, 0), 3026, 2),
			new("The Haven Thieves' Guild", new(3690, 2520, 0), 3026, 2),
			new(1016057, new(3658, 2536, 0), 3036, 2),
			new("The Second Bank of Haven", new(3624, 2610, 2), 3083, 2),
			new("Haven Public Library", new(3617, 2480, 0), 2966, 2),

			#endregion

			#region Ilshenar

			new("Librum", new(860, 654, -39), 2965, 3),
			new("Librum", new(877, 654, -39), 2965, 3),
			new("Librum", new(861, 632, -34), 2965, 3),
			new("Librum", new(865, 632, -34), 2965, 3),
			new("Librum", new(873, 632, -34), 2965, 3),
			new("Librum", new(877, 632, -34), 2965, 3),
			new("Skis-In-Lem", new(882, 616, -34), 2981, 3),
			new("Skis-In-Lem", new(886, 616, -34), 2981, 3),
			new("Atri-Ben-In-Ailem", new(780, 638, 1), 2984, 3),
			new("Atri-Ben-In-Ailem", new(780, 646, 1), 2984, 3),
			new("In-Mani-Lem", new(858, 592, -33), 2987, 3),
			new("In-Mani-Lem", new(862, 592, -33), 2987, 3),
			new("Ter-Ort-Lem", new(837, 582, 1), 2990, 3),
			new("Ter-Zu", new(836, 703, 1), 2995, 3),
			new("Ter-Zu", new(845, 703, 1), 2995, 3),
			new("Aglo-In-Lem", new(817, 694, -38), 3007, 3),
			new("Aglo-In-Lem", new(822, 694, -38), 3007, 3),
			new("Agra-Char-In-Lem", new(819, 592, -34), 3007, 3),
			new("Agra-Char-In-Lem", new(823, 592, -34), 3007, 3),
			new("Agra-Char-In-Lem", new(814, 596, -34), 3008, 3),
			new("Agra-Char-In-Lem", new(814, 600, -34), 3008, 3),
			new("Aglo-In-Lem", new(814, 685, -34), 3008, 3),
			new("Aglo-In-Lem", new(814, 689, -34), 3008, 3),
			new("Klar-Lap-In-Lem", new(858, 694, -39), 3009, 3),
			new("Klar-Lap-In-Lem", new(863, 694, -39), 3009, 3),
			new("Zhel-In-Lem", new(788, 655, 1), 3015, 3),
			new("Zhel-In-Lem", new(796, 655, 1), 3015, 3),
			new("Ben-In-Lem", new(830, 678, -39), 3019, 3),
			new("Ben-In-Lem", new(835, 678, -39), 3019, 3),
			new("Ben-In-Lem", new(831, 608, -34), 3019, 3),
			new("Ben-In-Lem", new(835, 608, -34), 3019, 3),
			new("Lap-In-Lem", new(788, 630, 1), 3025, 3),
			new("Lap-In-Lem", new(796, 630, 1), 3025, 3),
			new("Ter-An-Eks-Por", new(881, 670, -39), 3025, 3),
			new("Ter-An-Eks-Por", new(886, 670, -39), 3025, 3),
			new("Atri-Aur", new(845, 678, -39), 3083, 3),
			new("Atri-Aur", new(850, 678, -39), 3083, 3),
			new("Atri-Aur", new(846, 608, -34), 3083, 3),
			new("Atri-Aur", new(850, 608, -34), 3083, 3),
			new("Lap-In-Lem", new(868, 684, -39), 3140, 3),
			new("Lap-In-Lem", new(868, 689, -39), 3140, 3),
			new(1016183, new(787, 1137, -13), 2996, 3),

			#endregion

			#region Malas

			new(1061816, new(996, 517, -50), 3083, 4),
			new(1061813, new(981, 511, -50), 3015, 4),
			new(1061814, new(981, 526, -50), 2981, 4),
			new(1061818, new(1000, 523, -50), 2990, 4),
			new(1061817, new(1001, 518, -50), 2992, 4),
			new(1061815, new(994, 509, -50), 3020, 4),
			new(1061819, new(1005, 522, -30), 2995, 4),
			new(1061820, new(1021, 505, -70), 3000, 4),
			new(1061800, new(1997, 1369, -87), 3015, 4),
			new(1061804, new(1994, 1326, -91), 3000, 4),
			new(1061809, new(2014, 1338, -76), 3020, 4),
			new(1061810, new(2023, 1347, -90), 2979, 4),
			new(1061807, new(2030, 1348, -89), 3011, 4),
			new(1061799, new(2038, 1385, -88), 2989, 4),
			new(1061802, new(2055, 1390, -90), 3009, 4),
			new(1061803, new(2056, 1338, -85), 3083, 4),
			new(1061808, new(2047, 1312, -90), 2995, 4),
			new(1061805, new(2069, 1298, -81), 2992, 4),
			new(1061801, new(2075, 1339, -82), 2982, 4),
			new(1061806, new(2085, 1379, -90), 2988, 4),
			new(1061616, new(1067, 1431, -88), 2995, 4),
			new(1061835, new(962, 641, -90), 3011, 4),
			new(1061836, new(962, 633, -90), 2979, 4),

			#endregion

			#region Tokuno

			new(1063398, new(677, 1220, 25), 2996, 5),
			new(1063399, new(694, 1229, 25), 3012, 5),
			new(1063400, new(675, 1280, 32), 2966, 5),
			new(1063400, new(680, 1280, 32), 2966, 5),
			new(1063401, new(707, 1300, 25), 2989, 5),
			new(1063402, new(718, 1301, 25), 3010, 5),
			new(1063403, new(736, 1281, 25), 5019, 5),
			new(1063404, new(768, 1298, 25), 3025, 5),
			new(1063405, new(789, 1302, 25), 3021, 5),
			new(1063406, new(790, 1284, 25), 3000, 5),
			new(1063407, new(764, 1255, 25), 3016, 5),
			new(1063408, new(780, 1227, 25), 2988, 5),
			new(1063409, new(729, 1229, 25), 3024, 5),
			new(1063410, new(710, 1251, 25), 3025, 5),
			new(1063410, new(710, 1257, 25), 3025, 5),
			new(1063410, new(710, 1270, 25), 3025, 5),

			#endregion

			#region TerMur

			#endregion
		};

		public static void Initialize()
		{
			CommandSystem.Register("SignGen", AccessLevel.Administrator, new CommandEventHandler(SignGen_OnCommand));
		}

		[Usage("SignGen")]
		[Description("Generates world/shop signs on all facets.")]
		public static void SignGen_OnCommand(CommandEventArgs c)
		{
			Generate(c.Mobile);
		}

		public static void Generate(Mobile from)
		{
			from?.SendMessage("Generating signs, please wait.");

			var brit = new Map[] { Map.Felucca, Map.Trammel };
			var fel = new Map[] { Map.Felucca };
			var tram = new Map[] { Map.Trammel };
			var ilsh = new Map[] { Map.Ilshenar };
			var malas = new Map[] { Map.Malas };
			var tokuno = new Map[] { Map.Tokuno };
			var termur = new Map[] { Map.TerMur };

			foreach (var e in m_Entries)
			{
				Map[] maps = null;

				switch (e.m_Map)
				{
					case 0: maps = brit; break; // Trammel and Felucca
					case 1: maps = fel; break;  // Felucca
					case 2: maps = tram; break; // Trammel
					case 3: maps = ilsh; break; // Ilshenar
					case 4: maps = malas; break; // Malas
					case 5: maps = tokuno; break; // Tokuno
					case 6: maps = termur; break; // TerMur
				}

				for (var j = 0; maps != null && j < maps.Length; ++j)
				{
					Add_Static(e.m_ItemID, e.m_Location, maps[j], e.m_Text);
				}
			}

			from?.SendMessage("Sign generating complete.");
		}

		private static readonly Queue<Item> m_ToDelete = new Queue<Item>();

		public static void Add_Static(int itemID, Point3D location, Map map, TextDefinition name)
		{
			var eable = map.GetItemsInRange(location, 0);

			foreach (var item in eable)
			{
				if (item is Sign && item.Z == location.Z && item.ItemID == itemID)
				{
					m_ToDelete.Enqueue(item);
				}
			}

			eable.Free();

			while (m_ToDelete.Count > 0)
			{
				m_ToDelete.Dequeue().Delete();
			}

			Item sign;

			if (name.Number > 0)
			{
				sign = new LocalizedSign(itemID, name.Number);
			}
			else
			{
				sign = new Sign(itemID)
				{
					Name = name
				};
			}

			if (map == Map.Malas)
			{
				if (location.X >= 965 && location.Y >= 502 && location.X <= 1012 && location.Y <= 537)
				{
					sign.Hue = 0x47E;
				}
				else if (location.X >= 1960 && location.Y >= 1278 && location.X < 2106 && location.Y < 1413)
				{
					sign.Hue = 0x44E;
				}
			}

			sign.MoveToWorld(location, map);
		}
	}
}