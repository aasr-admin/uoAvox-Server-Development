
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Server
{
	public static class Utility
	{
		private static Encoding m_UTF8, m_UTF8WithEncoding;

		public static Encoding UTF8 => m_UTF8 ??= new UTF8Encoding(false, false);
		public static Encoding UTF8WithEncoding => m_UTF8WithEncoding ??= new UTF8Encoding(true, false);

		public static void Separate(StringBuilder sb, string value, string separator)
		{
			if (sb.Length > 0)
			{
				_ = sb.Append(separator);
			}

			_ = sb.Append(value);
		}

		public static string Intern(string str)
		{
			if (!String.IsNullOrWhiteSpace(str))
			{
				return String.Intern(str);
			}

			return str;
		}

		public static void Intern(ref string str)
		{
			str = Intern(str);
		}

		public static void SpaceWords(ref string str)
		{
			if (!String.IsNullOrWhiteSpace(str))
			{
				str = Regex.Replace(str, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
			}
		}

		public static string SpaceWords(string str)
		{
			SpaceWords(ref str);

			return str;
		}

		private static readonly Dictionary<Enum, string> m_FriendlyEnumNames = new();

		public static string FriendlyName(Enum value)
		{
			var type = value.GetType();

			var flags = type.GetCustomAttribute<FlagsAttribute>(true) != null;

			if (flags || !m_FriendlyEnumNames.TryGetValue(value, out var name))
			{
				name = value.ToString();

				SpaceWords(ref name);

				if (!flags && !String.IsNullOrWhiteSpace(name))
				{
					m_FriendlyTypeNames[type] = name = Intern(name);
				}
			}

			return name;
		}

		private static readonly Dictionary<Type, string> m_FriendlyTypeNames = new();

		public static string FriendlyName(Type type)
		{
			if (!m_FriendlyTypeNames.TryGetValue(type, out var name))
			{
				if (type.IsAssignableTo(typeof(Item)) && Activator.CreateInstance(type) is Item item)
				{
					name = item.Name;

					if (String.IsNullOrWhiteSpace(name))
					{
						name = StringList.Localization.GetString(item.LabelNumber);
					}

					item.Delete();
				}

				if (String.IsNullOrWhiteSpace(name))
				{
					name = SpaceWords(type.Name);
				}

				if (!String.IsNullOrWhiteSpace(name))
				{
					m_FriendlyTypeNames[type] = name = Intern(name);
				}
			}

			return name;
		}

		public static string FriendlyName(IEntity entity)
		{
			var name = entity.Name;

			if (entity is Item item)
			{
				name = item.Name;

				if (String.IsNullOrWhiteSpace(name))
				{
					name = StringList.Localization.GetString(item.LabelNumber);
				}
			}

			if (String.IsNullOrWhiteSpace(name))
			{
				name = FriendlyName(entity.GetType());
			}

			return name;
		}

		private static readonly Dictionary<Type, int> m_ItemIDs = new();

		public static int GetDefaultAssetID(Type type)
		{
			if (!m_ItemIDs.TryGetValue(type, out var assetID))
			{
				assetID = -1;

				if (type.IsAssignableTo(typeof(Item)))
				{
					if (Activator.CreateInstance(type) is Item item)
					{
						assetID = item.ItemID;

						item.Delete();
					}
				}
				else if (type.IsAssignableTo(typeof(Mobile)))
				{
					if (Activator.CreateInstance(type) is Mobile mobile)
					{
						assetID = mobile.Body;

						mobile.Delete();
					}
				}

				if (assetID >= 0)
				{
					m_ItemIDs[type] = assetID;
				}
			}

			return assetID;
		}

		private static Dictionary<IPAddress, IPAddress> _ipAddressTable;

		public static IPAddress Intern(IPAddress ipAddress)
		{
			_ipAddressTable ??= new Dictionary<IPAddress, IPAddress>();

			IPAddress interned;

			if (!_ipAddressTable.TryGetValue(ipAddress, out interned))
			{
				interned = ipAddress;
				_ipAddressTable[ipAddress] = interned;
			}

			return interned;
		}

		public static void Intern(ref IPAddress ipAddress)
		{
			ipAddress = Intern(ipAddress);
		}

		public static bool IsValidIP(string text)
		{
			var valid = true;

			_ = IPMatch(text, IPAddress.None, ref valid);

			return valid;
		}

		public static bool IPMatch(string val, IPAddress ip)
		{
			var valid = true;

			return IPMatch(val, ip, ref valid);
		}

		public static string FixHtml(string str)
		{
			if (str == null)
			{
				return "";
			}

			var hasOpen = str.Contains('<');
			var hasClose = str.Contains('>');
			var hasPound = str.Contains('#');

			if (!hasOpen && !hasClose && !hasPound)
			{
				return str;
			}

			var sb = new StringBuilder(str);

			if (hasOpen)
			{
				_ = sb.Replace('<', '(');
			}

			if (hasClose)
			{
				_ = sb.Replace('>', ')');
			}

			if (hasPound)
			{
				_ = sb.Replace('#', '-');
			}

			return sb.ToString();
		}

		public static bool IPMatchCIDR(string cidr, IPAddress ip)
		{
			if (ip == null || ip.AddressFamily == AddressFamily.InterNetworkV6)
			{
				return false;   //Just worry about IPv4 for now
			}

			/*
			string[] str = cidr.Split( '/' );

			if ( str.Length != 2 )
				return false;

			/* **************************************************
			IPAddress cidrPrefix;

			if ( !IPAddress.TryParse( str[0], out cidrPrefix ) )
				return false;
			 * */

			/*
			string[] dotSplit = str[0].Split( '.' );

			if ( dotSplit.Length != 4 )		//At this point and time, and for speed sake, we'll only worry about IPv4
				return false;

			byte[] bytes = new byte[4];

			for ( int i = 0; i < 4; i++ )
			{
				byte.TryParse( dotSplit[i], out bytes[i] );
			}

			uint cidrPrefix = OrderedAddressValue( bytes );

			int cidrLength = Utility.ToInt32( str[1] );
			//The below solution is the fastest solution of the three

			*/

			var bytes = new byte[4];
			var split = cidr.Split('.');
			var cidrBits = false;
			var cidrLength = 0;

			for (var i = 0; i < 4; i++)
			{
				var part = 0;

				var partBase = 10;

				var pattern = split[i];

				for (var j = 0; j < pattern.Length; j++)
				{
					var c = pattern[j];

					if (c is 'x' or 'X')
					{
						partBase = 16;
					}
					else if (c is >= '0' and <= '9')
					{
						var offset = c - '0';

						if (cidrBits)
						{
							cidrLength *= partBase;
							cidrLength += offset;
						}
						else
						{
							part *= partBase;
							part += offset;
						}
					}
					else if (c is >= 'a' and <= 'f')
					{
						var offset = 10 + (c - 'a');

						if (cidrBits)
						{
							cidrLength *= partBase;
							cidrLength += offset;
						}
						else
						{
							part *= partBase;
							part += offset;
						}
					}
					else if (c is >= 'A' and <= 'F')
					{
						var offset = 10 + (c - 'A');

						if (cidrBits)
						{
							cidrLength *= partBase;
							cidrLength += offset;
						}
						else
						{
							part *= partBase;
							part += offset;
						}
					}
					else if (c == '/')
					{
						if (cidrBits || i != 3) //If there's two '/' or the '/' isn't in the last byte
						{
							return false;
						}

						partBase = 10;
						cidrBits = true;
					}
					else
					{
						return false;
					}
				}

				bytes[i] = (byte)part;
			}

			var cidrPrefix = OrderedAddressValue(bytes);

			return IPMatchCIDR(cidrPrefix, ip, cidrLength);
		}

		public static bool IPMatchCIDR(IPAddress cidrPrefix, IPAddress ip, int cidrLength)
		{
			if (cidrPrefix == null || ip == null || cidrPrefix.AddressFamily == AddressFamily.InterNetworkV6)   //Ignore IPv6 for now
			{
				return false;
			}

			var cidrValue = SwapUnsignedInt((uint)GetLongAddressValue(cidrPrefix));
			var ipValue = SwapUnsignedInt((uint)GetLongAddressValue(ip));

			return IPMatchCIDR(cidrValue, ipValue, cidrLength);
		}

		public static bool IPMatchCIDR(uint cidrPrefixValue, IPAddress ip, int cidrLength)
		{
			if (ip == null || ip.AddressFamily == AddressFamily.InterNetworkV6)
			{
				return false;
			}

			var ipValue = SwapUnsignedInt((uint)GetLongAddressValue(ip));

			return IPMatchCIDR(cidrPrefixValue, ipValue, cidrLength);
		}

		public static bool IPMatchCIDR(uint cidrPrefixValue, uint ipValue, int cidrLength)
		{
			if (cidrLength is <= 0 or >= 32)   //if invalid cidr Length, just compare IPs
			{
				return cidrPrefixValue == ipValue;
			}

			var mask = UInt32.MaxValue << (32 - cidrLength);

			return (cidrPrefixValue & mask) == (ipValue & mask);
		}

		private static uint OrderedAddressValue(byte[] bytes)
		{
			if (bytes.Length != 4)
			{
				return 0;
			}

			return (uint)((bytes[0] << 0x18) | (bytes[1] << 0x10) | (bytes[2] << 8) | bytes[3]) & 0xffffffff;
		}

		private static uint SwapUnsignedInt(uint source)
		{
			return ((source & 0x000000FF) << 0x18)
			| ((source & 0x0000FF00) << 8)
			| ((source & 0x00FF0000) >> 8)
			| ((source & 0xFF000000) >> 0x18);
		}

		public static bool TryConvertIPv6toIPv4(ref IPAddress address)
		{
			if (!Socket.OSSupportsIPv6 || address.AddressFamily == AddressFamily.InterNetwork)
			{
				return true;
			}

			var addr = address.GetAddressBytes();
			if (addr.Length == 16)  //sanity 0 - 15 //10 11 //12 13 14 15
			{
				if (addr[10] != 0xFF || addr[11] != 0xFF)
				{
					return false;
				}

				for (var i = 0; i < 10; i++)
				{
					if (addr[i] != 0)
					{
						return false;
					}
				}

				var v4Addr = new byte[4];

				for (var i = 0; i < 4; i++)
				{
					v4Addr[i] = addr[12 + i];
				}

				address = new IPAddress(v4Addr);
				return true;
			}

			return false;
		}

		public static bool IPMatch(string val, IPAddress ip, ref bool valid)
		{
			valid = true;

			var split = val.Split('.');

			for (var i = 0; i < 4; ++i)
			{
				int lowPart, highPart;

				if (i >= split.Length)
				{
					lowPart = 0;
					highPart = 255;
				}
				else
				{
					var pattern = split[i];

					if (pattern == "*")
					{
						lowPart = 0;
						highPart = 255;
					}
					else
					{
						lowPart = 0;
						highPart = 0;

						var highOnly = false;
						var lowBase = 10;
						var highBase = 10;

						for (var j = 0; j < pattern.Length; ++j)
						{
							var c = pattern[j];

							if (c == '?')
							{
								if (!highOnly)
								{
									lowPart *= lowBase;
									lowPart += 0;
								}

								highPart *= highBase;
								highPart += highBase - 1;
							}
							else if (c == '-')
							{
								highOnly = true;
								highPart = 0;
							}
							else if (c is 'x' or 'X')
							{
								lowBase = 16;
								highBase = 16;
							}
							else if (c is >= '0' and <= '9')
							{
								var offset = c - '0';

								if (!highOnly)
								{
									lowPart *= lowBase;
									lowPart += offset;
								}

								highPart *= highBase;
								highPart += offset;
							}
							else if (c is >= 'a' and <= 'f')
							{
								var offset = 10 + (c - 'a');

								if (!highOnly)
								{
									lowPart *= lowBase;
									lowPart += offset;
								}

								highPart *= highBase;
								highPart += offset;
							}
							else if (c is >= 'A' and <= 'F')
							{
								var offset = 10 + (c - 'A');

								if (!highOnly)
								{
									lowPart *= lowBase;
									lowPart += offset;
								}

								highPart *= highBase;
								highPart += offset;
							}
							else
							{
								valid = false;  //high & lowpart would be 0 if it got to here.
							}
						}
					}
				}

				int b = (byte)(Utility.GetAddressValue(ip) >> (i * 8));

				if (b < lowPart || b > highPart)
				{
					return false;
				}
			}

			return true;
		}

		public static bool IPMatchClassC(IPAddress ip1, IPAddress ip2)
		{
			return (Utility.GetAddressValue(ip1) & 0xFFFFFF) == (Utility.GetAddressValue(ip2) & 0xFFFFFF);
		}

		public static int InsensitiveCompare(string first, string second)
		{
			return Insensitive.Compare(first, second);
		}

		public static bool InsensitiveStartsWith(string first, string second)
		{
			return Insensitive.StartsWith(first, second);
		}

		#region To[Something]
		public static bool ToBoolean(string value)
		{
			_ = Boolean.TryParse(value, out var b);

			return b;
		}

		public static double ToDouble(string value)
		{
			_ = Double.TryParse(value, out var d);

			return d;
		}

		public static TimeSpan ToTimeSpan(string value)
		{
			_ = TimeSpan.TryParse(value, out var t);

			return t;
		}

		public static Serial ToSerial(string value)
		{
			return new Serial(ToInt32(value));
		}

		public static int ToInt32(string value)
		{
			int i;

			if (value.StartsWith("0x"))
			{
				_ = Int32.TryParse(value.AsSpan(2), NumberStyles.HexNumber, null, out i);
			}
			else
			{
				_ = Int32.TryParse(value, out i);
			}

			return i;
		}
		#endregion

		#region Get[Something]
		public static double GetXMLDouble(string doubleString, double defaultValue)
		{
			try
			{
				return XmlConvert.ToDouble(doubleString);
			}
			catch
			{
				double val;
				if (Double.TryParse(doubleString, out val))
				{
					return val;
				}

				return defaultValue;
			}
		}

		public static Serial GetXMLSerial(string intString, int defaultValue)
		{
			return new Serial(GetXMLInt32(intString, defaultValue));
		}

		public static int GetXMLInt32(string intString, int defaultValue)
		{
			try
			{
				return XmlConvert.ToInt32(intString);
			}
			catch
			{
				int val;
				if (Int32.TryParse(intString, out val))
				{
					return val;
				}

				return defaultValue;
			}
		}

		public static DateTime GetXMLDateTime(string dateTimeString, DateTime defaultValue)
		{
			try
			{
				return XmlConvert.ToDateTime(dateTimeString, XmlDateTimeSerializationMode.Utc);
			}
			catch
			{
				DateTime d;

				if (DateTime.TryParse(dateTimeString, out d))
				{
					return d;
				}

				return defaultValue;
			}
		}

		public static DateTimeOffset GetXMLDateTimeOffset(string dateTimeOffsetString, DateTimeOffset defaultValue)
		{
			try
			{
				return XmlConvert.ToDateTimeOffset(dateTimeOffsetString);
			}
			catch
			{
				DateTimeOffset d;

				if (DateTimeOffset.TryParse(dateTimeOffsetString, out d))
				{
					return d;
				}

				return defaultValue;
			}
		}

		public static TimeSpan GetXMLTimeSpan(string timeSpanString, TimeSpan defaultValue)
		{
			try
			{
				return XmlConvert.ToTimeSpan(timeSpanString);
			}
			catch
			{
				return defaultValue;
			}
		}

		public static string GetAttribute(XmlElement node, string attributeName)
		{
			return GetAttribute(node, attributeName, null);
		}

		public static string GetAttribute(XmlElement node, string attributeName, string defaultValue)
		{
			if (node == null)
			{
				return defaultValue;
			}

			var attr = node.Attributes[attributeName];

			if (attr == null)
			{
				return defaultValue;
			}

			return attr.Value;
		}

		public static string GetText(XmlElement node, string defaultValue)
		{
			if (node == null)
			{
				return defaultValue;
			}

			return node.InnerText;
		}

		public static int GetAddressValue(IPAddress address)
		{
#pragma warning disable 618
			return (int)address.Address;
#pragma warning restore 618
		}

		public static long GetLongAddressValue(IPAddress address)
		{
#pragma warning disable 618
			return address.Address;
#pragma warning restore 618
		}
		#endregion

		#region In[...]Range
		public static bool InRange(Point3D p1, Point3D p2, int range)
		{
			return (p1.m_X >= (p2.m_X - range))
				&& (p1.m_X <= (p2.m_X + range))
				&& (p1.m_Y >= (p2.m_Y - range))
				&& (p1.m_Y <= (p2.m_Y + range));
		}

		public static bool InUpdateRange(Point3D p1, Point3D p2)
		{
			return (p1.m_X >= (p2.m_X - 18))
				&& (p1.m_X <= (p2.m_X + 18))
				&& (p1.m_Y >= (p2.m_Y - 18))
				&& (p1.m_Y <= (p2.m_Y + 18));
		}

		public static bool InUpdateRange(Point2D p1, Point2D p2)
		{
			return (p1.m_X >= (p2.m_X - 18))
				&& (p1.m_X <= (p2.m_X + 18))
				&& (p1.m_Y >= (p2.m_Y - 18))
				&& (p1.m_Y <= (p2.m_Y + 18));
		}

		public static bool InUpdateRange(IPoint2D p1, IPoint2D p2)
		{
			return (p1.X >= (p2.X - 18))
				&& (p1.X <= (p2.X + 18))
				&& (p1.Y >= (p2.Y - 18))
				&& (p1.Y <= (p2.Y + 18));
		}
		#endregion

		public static double GetDistanceToSqrt(IPoint2D p1, IPoint2D p2)
		{
			var xDelta = p1.X - p2.X;
			var yDelta = p1.Y - p2.Y;

			return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
		}

		public static Direction GetDirection(IPoint2D from, IPoint2D to)
		{
			var dx = to.X - from.X;
			var dy = to.Y - from.Y;

			var adx = Math.Abs(dx);
			var ady = Math.Abs(dy);

			if (adx >= ady * 3)
			{
				if (dx > 0)
				{
					return Direction.East;
				}
				else
				{
					return Direction.West;
				}
			}
			else if (ady >= adx * 3)
			{
				if (dy > 0)
				{
					return Direction.South;
				}
				else
				{
					return Direction.North;
				}
			}
			else if (dx > 0)
			{
				if (dy > 0)
				{
					return Direction.Down;
				}
				else
				{
					return Direction.Right;
				}
			}
			else
			{
				if (dy > 0)
				{
					return Direction.Left;
				}
				else
				{
					return Direction.Up;
				}
			}
		}

		public static object GetArrayCap(Array array, int index)
		{
			return GetArrayCap(array, index, null);
		}

		public static object GetArrayCap(Array array, int index, object emptyValue)
		{
			if (array.Length > 0)
			{
				if (index < 0)
				{
					index = 0;
				}
				else if (index >= array.Length)
				{
					index = array.Length - 1;
				}

				return array.GetValue(index);
			}
			else
			{
				return emptyValue;
			}
		}

		#region Random

		//4d6+8 would be: Utility.Dice( 4, 6, 8 )
		public static int Dice(int numDice, int numSides, int bonus)
		{
			var total = 0;

			for (var i = 0; i < numDice; ++i)
			{
				total += Random(numSides) + 1;
			}

			total += bonus;
			return total;
		}

		public static T RandomList<T>(List<T> list)
		{
			if (list.Count > 0)
			{
				if (list.Count > 1)
				{
					return list[Random(list.Count)];
				}

				return list[0];
			}

			return default;
		}

		public static T RandomList<T>(params T[] list)
		{
			if (list.Length > 0)
			{
				if (list.Length > 1)
				{
					return list[Random(list.Length)];
				}

				return list[0];
			}

			return default;
		}

		public static bool RandomBool()
		{
			return RandomImpl.NextBool();
		}

#if MONO
		private static class EnumCache<T> where T : struct, IConvertible
#else
		private static class EnumCache<T> where T : struct, Enum
#endif
		{
			public static T[] Values = (T[])Enum.GetValues(typeof(T));
		}

#if MONO
		public static TEnum RandomEnum<TEnum>() where TEnum : struct, IConvertible            
#else
		public static TEnum RandomEnum<TEnum>() where TEnum : struct, Enum
#endif
		{
			return RandomList(EnumCache<TEnum>.Values);
		}

#if MONO
		public static TEnum RandomMinMax<TEnum>(TEnum min, TEnum max) where TEnum : struct, IConvertible            
#else
		public static TEnum RandomMinMax<TEnum>(TEnum min, TEnum max) where TEnum : struct, Enum
#endif
		{
			var values = EnumCache<TEnum>.Values;

			if (values.Length == 0)
			{
				return default;
			}

			int curIdx = -1, minIdx = -1, maxIdx = -1;

			while (++curIdx < values.Length)
			{
				if (Equals(values[curIdx], min))
				{
					minIdx = curIdx;
				}
				else if (Equals(values[curIdx], max))
				{
					maxIdx = curIdx;
				}
			}

			if (minIdx == 0 && maxIdx == values.Length - 1)
			{
				return RandomList(values);
			}

			curIdx = -1;

			if (minIdx >= 0)
			{
				if (minIdx == maxIdx)
				{
					curIdx = minIdx;
				}
				else if (maxIdx > minIdx)
				{
					curIdx = RandomMinMax(minIdx, maxIdx);
				}
			}

			if (curIdx >= 0 && curIdx < values.Length)
			{
				return values[curIdx];
			}

			return RandomList(min, max);
		}

		public static TimeSpan RandomMinMax(TimeSpan min, TimeSpan max)
		{
			return TimeSpan.FromTicks(RandomMinMax(min.Ticks, max.Ticks));
		}

		public static int RandomMinMax(int min, int max)
		{
			if (min > max)
			{
				(max, min) = (min, max);
			}
			else if (min == max)
			{
				return min;
			}

			return min + Random(max - min + 1);
		}

		public static long RandomMinMax(long min, long max)
		{
			if (min > max)
			{
				(max, min) = (min, max);
			}
			else if (min == max)
			{
				return min;
			}

			return min + (long)(RandomImpl.NextDouble() * (max - min));
		}

		public static double RandomMinMax(double min, double max)
		{
			if (min > max)
			{
				(max, min) = (min, max);
			}
			else if (min == max)
			{
				return min;
			}

			return min + (RandomDouble() * (max - min + 1));
		}

		public static int Random(int from, int count)
		{
			if (count > 0)
			{
				return from + Random(count);
			}

			if (count < 0)
			{
				return from - Random(-count);
			}

			return from;
		}

		public static int Random(int count)
		{
			return RandomImpl.Next(count);
		}

		public static void RandomBytes(byte[] buffer)
		{
			RandomImpl.NextBytes(buffer);
		}

		public static double RandomDouble()
		{
			return RandomImpl.NextDouble();
		}

		#endregion

		#region Random Hues

		/// <summary>
		/// Random pink, blue, green, orange, red or yellow hue
		/// </summary>
		public static int RandomNondyedHue()
		{
			return Random(6) switch
			{
				0 => RandomPinkHue(),
				1 => RandomBlueHue(),
				2 => RandomGreenHue(),
				3 => RandomOrangeHue(),
				4 => RandomRedHue(),
				5 => RandomYellowHue(),
				_ => 0,
			};
		}

		/// <summary>
		/// Random hue in the range 1201-1254
		/// </summary>
		public static int RandomPinkHue()
		{
			return Random(1201, 54);
		}

		/// <summary>
		/// Random hue in the range 1301-1354
		/// </summary>
		public static int RandomBlueHue()
		{
			return Random(1301, 54);
		}

		/// <summary>
		/// Random hue in the range 1401-1454
		/// </summary>
		public static int RandomGreenHue()
		{
			return Random(1401, 54);
		}

		/// <summary>
		/// Random hue in the range 1501-1554
		/// </summary>
		public static int RandomOrangeHue()
		{
			return Random(1501, 54);
		}

		/// <summary>
		/// Random hue in the range 1601-1654
		/// </summary>
		public static int RandomRedHue()
		{
			return Random(1601, 54);
		}

		/// <summary>
		/// Random hue in the range 1701-1754
		/// </summary>
		public static int RandomYellowHue()
		{
			return Random(1701, 54);
		}

		/// <summary>
		/// Random hue in the range 1801-1908
		/// </summary>
		public static int RandomNeutralHue()
		{
			return Random(1801, 108);
		}

		/// <summary>
		/// Random hue in the range 2001-2018
		/// </summary>
		public static int RandomSnakeHue()
		{
			return Random(2001, 18);
		}

		/// <summary>
		/// Random hue in the range 2101-2130
		/// </summary>
		public static int RandomBirdHue()
		{
			return Random(2101, 30);
		}

		/// <summary>
		/// Random hue in the range 2201-2224
		/// </summary>
		public static int RandomSlimeHue()
		{
			return Random(2201, 24);
		}

		/// <summary>
		/// Random hue in the range 2301-2318
		/// </summary>
		public static int RandomAnimalHue()
		{
			return Random(2301, 18);
		}

		/// <summary>
		/// Random hue in the range 2401-2430
		/// </summary>
		public static int RandomMetalHue()
		{
			return Random(2401, 30);
		}

		public static int ClipDyedHue(int hue)
		{
			if (hue < 2)
			{
				return 2;
			}
			else if (hue > 1001)
			{
				return 1001;
			}
			else
			{
				return hue;
			}
		}

		/// <summary>
		/// Random hue in the range 2-1001
		/// </summary>
		public static int RandomDyedHue()
		{
			return Random(2, 1000);
		}

		/// <summary>
		/// Random hue from 0x62, 0x71, 0x03, 0x0D, 0x13, 0x1C, 0x21, 0x30, 0x37, 0x3A, 0x44, 0x59
		/// </summary>
		public static int RandomBrightHue()
		{
			if (Utility.RandomDouble() < 0.1)
			{
				return Utility.RandomList(0x62, 0x71);
			}

			return Utility.RandomList(0x03, 0x0D, 0x13, 0x1C, 0x21, 0x30, 0x37, 0x3A, 0x44, 0x59);
		}

		//[Obsolete( "Depreciated, use the methods for the Mobile's race", false )]
		public static int ClipSkinHue(int hue)
		{
			if (hue < 1002)
			{
				return 1002;
			}
			else if (hue > 1058)
			{
				return 1058;
			}
			else
			{
				return hue;
			}
		}

		//[Obsolete( "Depreciated, use the methods for the Mobile's race", false )]
		public static int RandomSkinHue()
		{
			return Random(1002, 57) | 0x8000;
		}

		//[Obsolete( "Depreciated, use the methods for the Mobile's race", false )]
		public static int ClipHairHue(int hue)
		{
			if (hue < 1102)
			{
				return 1102;
			}
			else if (hue > 1149)
			{
				return 1149;
			}
			else
			{
				return hue;
			}
		}

		//[Obsolete( "Depreciated, use the methods for the Mobile's race", false )]
		public static int RandomHairHue()
		{
			return Random(1102, 48);
		}

		#endregion

		private static readonly SkillName[] m_AllSkills = EnumCache<SkillName>.Values;

		private static readonly SkillName[] m_CombatSkills = new SkillName[]
		{
			SkillName.Archery,
			SkillName.Swords,
			SkillName.Macing,
			SkillName.Fencing,
			SkillName.Wrestling
		};

		private static readonly SkillName[] m_CraftSkills = new SkillName[]
		{
			SkillName.Alchemy,
			SkillName.Blacksmith,
			SkillName.Fletching,
			SkillName.Carpentry,
			SkillName.Cartography,
			SkillName.Cooking,
			SkillName.Inscribe,
			SkillName.Tailoring,
			SkillName.Tinkering
		};

		public static SkillName RandomSkill()
		{
			return m_AllSkills[Random(m_AllSkills.Length - (Core.ML ? 0 : Core.SE ? 1 : Core.AOS ? 3 : 6))];
		}

		public static SkillName RandomCombatSkill()
		{
			return m_CombatSkills[Random(m_CombatSkills.Length)];
		}

		public static SkillName RandomCraftSkill()
		{
			return m_CraftSkills[Random(m_CraftSkills.Length)];
		}

		public static void FixPoints(ref Point2D top, ref Point2D bottom)
		{
			if (bottom.m_X < top.m_X)
			{
				(bottom.m_X, top.m_X) = (top.m_X, bottom.m_X);
			}

			if (bottom.m_Y < top.m_Y)
			{
				(bottom.m_Y, top.m_Y) = (top.m_Y, bottom.m_Y);
			}
		}

		public static void FixPoints(ref Point3D top, ref Point3D bottom)
		{
			if (bottom.m_X < top.m_X)
			{
				(bottom.m_X, top.m_X) = (top.m_X, bottom.m_X);
			}

			if (bottom.m_Y < top.m_Y)
			{
				(bottom.m_Y, top.m_Y) = (top.m_Y, bottom.m_Y);
			}

			if (bottom.m_Z < top.m_Z)
			{
				(bottom.m_Z, top.m_Z) = (top.m_Z, bottom.m_Z);
			}
		}

		public static ArrayList BuildArrayList(IEnumerable enumerable)
		{
			var e = enumerable.GetEnumerator();

			var list = new ArrayList();

			while (e.MoveNext())
			{
				_ = list.Add(e.Current);
			}

			return list;
		}

		public static bool RangeCheck(IPoint2D p1, IPoint2D p2, int range)
		{
			return (p1.X >= (p2.X - range))
				&& (p1.X <= (p2.X + range))
				&& (p1.Y >= (p2.Y - range))
				&& (p2.Y <= (p2.Y + range));
		}

		public static void FormatBuffer(TextWriter output, Stream input, int length)
		{
			output.WriteLine("        0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
			output.WriteLine("       -- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");

			var byteIndex = 0;

			var whole = length >> 4;
			var rem = length & 0xF;

			for (var i = 0; i < whole; ++i, byteIndex += 16)
			{
				var bytes = new StringBuilder(49);
				var chars = new StringBuilder(16);

				for (var j = 0; j < 16; ++j)
				{
					var c = input.ReadByte();

					_ = bytes.Append(c.ToString("X2"));

					if (j != 7)
					{
						_ = bytes.Append(' ');
					}
					else
					{
						_ = bytes.Append("  ");
					}

					if (c is >= 0x20 and < 0x7F)
					{
						_ = chars.Append((char)c);
					}
					else
					{
						_ = chars.Append('.');
					}
				}

				output.Write(byteIndex.ToString("X4"));
				output.Write("   ");
				output.Write(bytes.ToString());
				output.Write("  ");
				output.WriteLine(chars.ToString());
			}

			if (rem != 0)
			{
				var bytes = new StringBuilder(49);
				var chars = new StringBuilder(rem);

				for (var j = 0; j < 16; ++j)
				{
					if (j < rem)
					{
						var c = input.ReadByte();

						_ = bytes.Append(c.ToString("X2"));

						if (j != 7)
						{
							_ = bytes.Append(' ');
						}
						else
						{
							_ = bytes.Append("  ");
						}

						if (c is >= 0x20 and < 0x7F)
						{
							_ = chars.Append((char)c);
						}
						else
						{
							_ = chars.Append('.');
						}
					}
					else
					{
						_ = bytes.Append("   ");
					}
				}

				output.Write(byteIndex.ToString("X4"));
				output.Write("   ");
				output.Write(bytes.ToString());
				output.Write("  ");
				output.WriteLine(chars.ToString());
			}
		}

		public static string FormatDate(DateTime date)
		{
			return FormatDate(date, date.Kind == DateTimeKind.Utc);
		}

		public static string FormatDate(DateTime date, bool utc)
		{
			return FormatDate(date, utc, false);
		}

		public static string FormatDate(DateTime date, bool utc, bool absolute)
		{
			return FormatDate(date, utc, absolute, true, true);
		}

		public static string FormatDate(DateTime date, bool utc, bool absolute, bool prefixed, bool suffixed)
		{
			DateTime now;

			if (utc)
			{
				now = DateTime.UtcNow;

				if (date.Kind != DateTimeKind.Utc)
				{
					date = date.ToUniversalTime();
				}
			}
			else
			{
				now = DateTime.Now;

				if (date.Kind != DateTimeKind.Local)
				{
					date = date.ToLocalTime();
				}
			}

			string prefix = null, format = suffixed ? "h:mmtt" : "HH:mm";

			if (absolute || date > now)
			{
				format = $"YYYY-MM-dd {format}";
			}
			else if (prefixed)
			{
				switch (now.DayOfYear - date.DayOfYear)
				{
					case 0: prefix = "Today at "; break;
					case 1: prefix = "Yesterday at "; break;
					default:
						{
							if (date.Year < now.Year)
							{
								format = $"YYYY-MM-dd {format}";
							}
							else
							{
								format = $"MM-dd {format}";
							}
						}
						break;
				}
			}
			else
			{
				if (date.Year < now.Year)
				{
					format = $"YYYY-MM-dd {format}";
				}
				else if (date.Month < now.Month)
				{
					format = $"MM-dd {format}";
				}
				else if (date.Day < now.Day)
				{
					format = $"MM-dd {format}";
				}
			}

			format = date.ToString(format, CultureInfo.InvariantCulture);
			format = format.Replace("YYYY", $"{date.Year:D4}");

			if (prefixed)
			{
				format = format.Replace('-', '/');
				format = format.Replace("    ", " at ");
			}

			if (suffixed)
			{
				if (date.TimeOfDay.Hours < 12)
				{
					format = format.Replace("AM", "am");
				}
				else
				{
					format = format.Replace("PM", "pm");
				}
			}

			return $"{prefix}{format}";
		}

		private static readonly Stack<ConsoleColor> m_ConsoleColors = new();

		public static void PushColor(ConsoleColor color)
		{
			try
			{
				m_ConsoleColors.Push(Console.ForegroundColor);
				Console.ForegroundColor = color;
			}
			catch
			{
			}
		}

		public static void PopColor()
		{
			try
			{
				Console.ForegroundColor = m_ConsoleColors.Pop();
			}
			catch
			{
			}
		}

		public static double Interpolate(double min, double max, double from, double to)
		{
			var prog = min / max;

			if (Double.IsNaN(prog) || Double.IsInfinity(prog))
			{
				if (Double.IsNegativeInfinity(prog))
				{
					return from;
				}

				if (Double.IsPositiveInfinity(prog))
				{
					return to;
				}

				return 0;
			}

			return from + ((to - from) * (min / max));
		}

		public static int Interpolate(double min, double max, Color from, Color to, out Color color)
		{
			var prog = min / max;

			if (Double.IsNaN(prog) || Double.IsInfinity(prog))
			{
				if (Double.IsNegativeInfinity(prog))
				{
					color = from;
				}
				else if (Double.IsPositiveInfinity(prog))
				{
					color = to;
				}
				else
				{
					color = Color.Empty;
				}
			}
			else
			{
				color = Color.FromArgb
				(
					Math.Clamp((int)Interpolate(min, max, from.A, to.A), 0, 255),
					Math.Clamp((int)Interpolate(min, max, from.R, to.R), 0, 255),
					Math.Clamp((int)Interpolate(min, max, from.G, to.G), 0, 255),
					Math.Clamp((int)Interpolate(min, max, from.B, to.B), 0, 255)
				);
			}

			return color.ToArgb();
		}

		public static void ConvertColor(Color color, out short out16)
		{
			ConvertColor(color.ToArgb(), out out16);
		}

		public static void ConvertColor(Color color, out int out32)
		{
			out32 = color.ToArgb();
		}

		public static void ConvertColor(int in32, out short out16)
		{
			in32 &= 0x00FFFFFF;

			var r = ((in32 >> 16) & 0xFF) >> 3;
			var g = ((in32 >> 08) & 0xFF) >> 3;
			var b = ((in32 >> 00) & 0xFF) >> 3;

			out16 = (short)((r << 16) | (g << 08) | (b << 00));
		}

		public static void ConvertColor(short in16, out int out32)
		{
			in16 &= 0x7FFF;

			var r = ((in16 >> 10) & 0x1F) << 3;
			var g = ((in16 >> 05) & 0x1F) << 3;
			var b = ((in16 >> 00) & 0x1F) << 3;

			out32 = (r << 16) | (g << 08) | (b << 00);
		}

		public static bool NumberBetween(double num, int bound1, int bound2, double allowance)
		{
			if (bound1 > bound2)
			{
				(bound2, bound1) = (bound1, bound2);
			}

			return num < bound2 + allowance && num > bound1 - allowance;
		}

		public static void AssignRandomHair(Mobile m)
		{
			AssignRandomHair(m, true);
		}

		public static void AssignRandomHair(Mobile m, int hue)
		{
			m.HairItemID = m.Race.RandomHair(m);
			m.HairHue = hue;
		}

		public static void AssignRandomHair(Mobile m, bool randomHue)
		{
			m.HairItemID = m.Race.RandomHair(m);

			if (randomHue)
			{
				m.HairHue = m.Race.RandomHairHue();
			}
		}

		public static void AssignRandomFacialHair(Mobile m)
		{
			AssignRandomFacialHair(m, true);
		}

		public static void AssignRandomFacialHair(Mobile m, int hue)
		{
			m.FacialHairItemID = m.Race.RandomFacialHair(m);
			m.FacialHairHue = hue;
		}

		public static void AssignRandomFacialHair(Mobile m, bool randomHue)
		{
			m.FacialHairItemID = m.Race.RandomFacialHair(m);

			if (randomHue)
			{
				m.FacialHairHue = m.Race.RandomHairHue();
			}
		}

		public static List<TOutput> CastConvertList<TInput, TOutput>(List<TInput> list) where TOutput : TInput
		{
			return list.ConvertAll(value => (TOutput)value);
		}

		public static List<TOutput> SafeConvertList<TInput, TOutput>(List<TInput> list) where TOutput : class
		{
			var output = new List<TOutput>(list.Capacity);

			for (var i = 0; i < list.Count; i++)
			{
				if (list[i] is TOutput t)
				{
					output.Add(t);
				}
			}

			return output;
		}

		public static bool CheckUse(Item item, Mobile from, bool handle = true, bool allowDead = false, int range = -1, bool packOnly = false, bool inTrade = false, bool inDisplay = true, AccessLevel access = AccessLevel.Player)
		{
			if (item == null || item.Deleted || from == null || from.Deleted)
			{
				return false;
			}

			if (from.AccessLevel < access)
			{
				if (handle)
				{
					from.SendMessage("You do not have sufficient access to use this item.");
				}

				return false;
			}

			if (!from.CanSee(item))
			{
				if (handle)
				{
					from.SendMessage("This item can't be seen.");
					item.OnDoubleClickCantSee(from);
				}

				return false;
			}

			if (!item.IsAccessibleTo(from))
			{
				if (handle)
				{
					item.OnDoubleClickNotAccessible(from);
				}

				return false;
			}

			if (item.InSecureTrade && !inTrade)
			{
				if (handle)
				{
					item.OnDoubleClickSecureTrade(from);
				}

				return false;
			}

			if (item.Parent == null && !item.Movable && !item.IsLockedDown && !item.IsSecure && !item.InSecureTrade && !inDisplay)
			{
				if (handle)
				{
					from.SendMessage("This item can not be accessed because it is part of a display.");
				}

				return false;
			}

			if (!from.Alive && !allowDead)
			{
				if (handle)
				{
					item.OnDoubleClickDead(from);
				}

				return false;
			}

			if (range >= 0 && !packOnly && !from.InRange(item, range))
			{
				if (handle)
				{
					if (range > 0)
					{
						from.SendMessage("You must be within {0:#,0} paces to use this item.", range);
					}
					else
					{
						from.SendMessage("You must be standing on this item to use it.");
					}

					item.OnDoubleClickOutOfRange(from);
				}

				return false;
			}

			if (packOnly && item.RootParent != from)
			{
				if (handle)
				{
					// This item must be in your backpack.
					from.SendLocalizedMessage(1054107);
				}

				return false;
			}

			return true;
		}

		private static BindingFlags GetBindingFlags(object obj, out Type type)
		{
			type = obj as Type ?? obj.GetType();

			var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

			if (type.IsSealed && type.IsAbstract)
			{
				flags &= ~BindingFlags.Instance;
			}

			return flags;
		}

		public static FieldInfo GetEventField(object obj, EventInfo eventInfo)
		{
			var flags = GetBindingFlags(obj, out var type);

			return type.GetField(eventInfo.Name, flags) ?? type.GetField($"EVENT_{eventInfo.Name.ToUpper()}", flags);
		}

		public static FieldInfo GetEventField(object obj, string eventName)
		{
			var flags = GetBindingFlags(obj, out var type);

			return GetEventField(obj, type.GetEvent(eventName, flags));
		}

		public static void ClearAllEventDelegates(object obj)
		{
			var flags = GetBindingFlags(obj, out var type);

			foreach (var e in type.GetEvents(flags))
			{
				ClearEventDelegates(type, e);
			}
		}

		public static void ClearEventDelegates(object obj, EventInfo eventInfo)
		{
			var eventField = GetEventField(obj, eventInfo);

			eventField?.SetValue(obj is Type ? null : obj, null);
		}

		public static void ClearEventDelegates(object obj, string eventName)
		{
			var flags = GetBindingFlags(obj, out var type);

			ClearEventDelegates(obj, type.GetEvent(eventName, flags));
		}

		public static Delegate[] GetEventDelegates(object obj, EventInfo eventInfo)
		{
			var eventField = GetEventField(obj, eventInfo);

			if (eventField?.GetValue(obj is Type ? null : obj) is Delegate eventHandler)
			{
				return eventHandler.GetInvocationList();
			}

			return Array.Empty<Delegate>();
		}

		public static Delegate[] GetEventDelegates(object obj, string eventName)
		{
			var flags = GetBindingFlags(obj, out var type);

			return GetEventDelegates(obj, type.GetEvent(eventName, flags));
		}

		public static MethodInfo[] GetEventMethods(object obj, EventInfo eventInfo)
		{
			var delegates = GetEventDelegates(obj, eventInfo);

			if (delegates.Length > 0)
			{
				var methods = new MethodInfo[delegates.Length];

				for (var i = 0; i < methods.Length; i++)
				{
					methods[i] = delegates[i].Method;
				}

				return methods;
			}

			return Array.Empty<MethodInfo>();
		}

		public static MethodInfo[] GetEventMethods(object obj, string eventName)
		{
			var flags = GetBindingFlags(obj, out var type);

			return GetEventMethods(obj, type.GetEvent(eventName, flags));
		}

		public static object CreateInstance(Type type, params object[] args)
		{
			try { return Activator.CreateInstance(type, args); }
			catch { return null; }
		}

		public static T CreateInstance<T>(Type type, params object[] args)
		{
			if (type?.IsAssignableTo(typeof(T)) == true)
			{
				return (T)CreateInstance(type, args);
			}

			return default;
		}

		public static Range[] ConvertToRangedArray(int[] ranges)
		{
			var output = new Range[ranges.Length / 2];

			var i = -1;
			var j = -1;

			while (++i < output.Length)
			{
				output[i] = ranges[++j]..ranges[++j];
			}

			return output;
		}

		public static int[] ConvertToRangedArray(Range[] ranges)
		{
			var output = new int[ranges.Length * 2];

			var i = -1;
			var j = -1;

			while (++i < ranges.Length)
			{
				output[++j] = ranges[i].Start.Value;
				output[++j] = ranges[i].End.Value;
			}

			return output;
		}

		public static int[] ConvertToSequentialArray(Range[] ranges)
		{
			var output = new SortedSet<int>();

			foreach (var r in ranges)
			{
				var id = r.Start.Value;

				while (id <= r.End.Value)
				{
					output.Add(id++);
				}
			}

			return output.ToArray();
		}
	}

	public static class WaterUtility
	{
		public static Range[] AllWaterTiles { get; set; } =
		{
			0x00A8..0x00AB,
			0x0136..0x0137,

			// static tiles require the 0x4000 offset
			// 0x5797 == 0x1798 | 0x4000

			0x5797..0x579C,
			0x746E..0x7485,
			0x7490..0x74AB,
			0x74B5..0x75D5,
		};

		public static Range[] DeepWaterLandTiles { get; set; } =
		{
			0x00A8..0x00AB,
			0x0136..0x0137,
		};

		public static Range[] DeepWaterStaticTiles { get; set; } =
		{
		};

		public static Range[] ShallowWaterLandTiles { get; set; } =
		{
		};

		public static Range[] ShallowWaterStaticTiles { get; set; } =
		{
			0x1797..0x179C,
		};

		public static Range[] FreshWaterLandTiles { get; set; } =
		{
		};

		public static Range[] FreshWaterStaticTiles { get; set; } =
		{
		};

		public static bool ValidateWater(Range[] waterValidation, Range[] freshValidation, Map map, Point3D target, int tileID, ref int z, out bool fresh)
		{
			var water = fresh = false;

			for (var i = 0; !water && i < waterValidation.Length; i++)
			{
				water = tileID >= waterValidation[i].Start.Value && tileID <= waterValidation[i].End.Value;
			}

			for (var i = 0; !fresh && i < freshValidation.Length; i++)
			{
				fresh = tileID >= freshValidation[i].Start.Value && tileID <= freshValidation[i].End.Value;
			}

			if (!water && fresh)
			{
				water = true;
			}

			if (water)
			{
				z = target.Z;
			}

			return water;
		}

		public static bool ValidateWater(Map map, object target, ref int z, out bool fresh)
		{
			return ValidateDeepWater(map, target, ref z, out fresh) || ValidateShallowWater(map, target, ref z, out fresh);
		}

		public static bool ValidateDeepWater(Map map, object target, ref int z, out bool fresh)
		{
			fresh = false;

			if (target is IEntity e)
			{
				if (e is Item i)
				{
					target = new LandTarget(i.WorldLocation, map);
				}
				else
				{
					target = new LandTarget(e.Location, map);
				}
			}

			if (target is LandTarget lt)
			{
				return ValidateDeepWater(map, lt, ref z, out fresh);
			}

			if (target is StaticTarget st)
			{
				return ValidateDeepWater(map, st, ref z, out fresh);
			}

			return false;
		}

		public static bool ValidateShallowWater(Map map, object target, ref int z, out bool fresh)
		{
			fresh = false;

			if (target is IEntity e)
			{
				if (e is Item i)
				{
					target = new LandTarget(i.WorldLocation, map);
				}
				else
				{
					target = new LandTarget(e.Location, map);
				}
			}

			if (target is LandTarget lt)
			{
				return ValidateShallowWater(map, lt, ref z, out fresh);
			}

			if (target is StaticTarget st)
			{
				return ValidateShallowWater(map, st, ref z, out fresh);
			}

			return false;
		}

		public static bool ValidateDeepWater(Map map, LandTarget target, ref int z, out bool fresh)
		{
			return ValidateWater(DeepWaterLandTiles, FreshWaterLandTiles, map, target.Location, target.TileID, ref z, out fresh);
		}

		public static bool ValidateShallowWater(Map map, LandTarget target, ref int z, out bool fresh)
		{
			return ValidateWater(ShallowWaterLandTiles, FreshWaterLandTiles, map, target.Location, target.TileID, ref z, out fresh);
		}

		public static bool ValidateDeepWater(Map map, StaticTarget target, ref int z, out bool fresh)
		{
			return ValidateWater(DeepWaterStaticTiles, FreshWaterStaticTiles, map, target.Location, target.ItemID, ref z, out fresh);
		}

		public static bool ValidateShallowWater(Map map, StaticTarget target, ref int z, out bool fresh)
		{
			return ValidateWater(ShallowWaterStaticTiles, FreshWaterStaticTiles, map, target.Location, target.ItemID, ref z, out fresh);
		}

		public static bool FullDeepValidation(Map map, Point3D loc, ref int z, out bool fresh)
		{
			return FullDeepValidation(map, loc, ref z, 5, out fresh);
		}

		public static bool FullDeepValidation(Map map, Point3D loc, ref int z, int range, out bool fresh)
		{
			var land = new LandTarget(loc, map);

			var valid = ValidateDeepWater(map, land, ref z, out fresh);

			loc.X -= range;
			loc.Y -= range;

			var endX = loc.X + (range * 2);
			var endY = loc.Y + (range * 2);
			var endZ = loc.Z;

			while (valid && loc.X <= endX)
			{
				while (valid && loc.Y <= endY)
				{
					loc.Z = map.GetAverageZ(loc.X, loc.Y);

					land = new LandTarget(loc, map);

					valid = ValidateDeepWater(map, land, ref endZ, out _);

					++loc.Y;
				}

				++loc.X;
			}

			return valid;
		}
	}
}