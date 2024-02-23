using System;
using System.Collections;
using System.Text;

namespace Server
{
	public enum ClientType
	{
		Regular,
		UOTD,
		God,
		SA
	}

	public class ClientVersion : IComparable, IComparer
	{
		public int Major { get; }
		public int Minor { get; }
		public int Revision { get; }
		public int Patch { get; }

		public ClientType Type { get; }

		public string SourceString { get; }

		public ClientVersion(int maj, int min, int rev, int pat) 
			: this(maj, min, rev, pat, ClientType.Regular)
		{
		}

		public ClientVersion(int maj, int min, int rev, int pat, ClientType type)
		{
			Major = maj;
			Minor = min;
			Revision = rev;
			Patch = pat;
			Type = type;

			SourceString = ToStringImpl();
		}

		public ClientVersion(string fmt)
		{
			SourceString = fmt;

			try
			{
				fmt = fmt.ToLower();

				var br1 = fmt.IndexOf('.');
				var br2 = fmt.IndexOf('.', br1 + 1);

				var br3 = br2 + 1;
				while (br3 < fmt.Length && Char.IsDigit(fmt, br3))
				{
					br3++;
				}

				Major = Utility.ToInt32(fmt.Substring(0, br1));
				Minor = Utility.ToInt32(fmt.Substring(br1 + 1, br2 - br1 - 1));
				Revision = Utility.ToInt32(fmt.Substring(br2 + 1, br3 - br2 - 1));

				if (br3 < fmt.Length)
				{
					if (Major <= 5 && Minor <= 0 && Revision <= 6)    //Anything before 5.0.7
					{
						if (!Char.IsWhiteSpace(fmt, br3))
						{
							Patch = fmt[br3] - 'a' + 1;
						}
					}
					else
					{
						Patch = Utility.ToInt32(fmt.Substring(br3 + 1, fmt.Length - br3 - 1));
					}
				}

				var cmp = StringComparison.InvariantCultureIgnoreCase;

				if (fmt.Contains("god", cmp) || fmt.Contains("gq", cmp))
				{
					Type = ClientType.God;
				}
				else if (fmt.Contains("third dawn", cmp) || fmt.Contains("uo:td", cmp) || fmt.Contains("uotd", cmp) || fmt.Contains("uo3d", cmp) || fmt.Contains("uo:3d", cmp))
				{
					Type = ClientType.UOTD;
				}
				else
				{
					Type = ClientType.Regular;
				}
			}
			catch
			{
				Major = 0;
				Minor = 0;
				Revision = 0;
				Patch = 0;
				Type = ClientType.Regular;
			}
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Major, Minor, Revision, Patch, Type);
		}

		public override bool Equals(object obj)
		{
			var v = obj as ClientVersion;

			if (IsNull(v))
			{
				return false;
			}

			return Major == v.Major && Minor == v.Minor && Revision == v.Revision && Patch == v.Patch && Type == v.Type;
		}

		private string ToStringImpl()
		{
			var builder = new StringBuilder(16);

			_ = builder.Append(Major);
			_ = builder.Append('.');
			_ = builder.Append(Minor);
			_ = builder.Append('.');
			_ = builder.Append(Revision);

			if (Major <= 5 && Minor <= 0 && Revision <= 6) // Anything before 5.0.7
			{
				if (Patch > 0)
				{
					_ = builder.Append((char)('a' + (Patch - 1)));
				}
			}
			else
			{
				_ = builder.Append('.');
				_ = builder.Append(Patch);
			}

			if (Type != ClientType.Regular)
			{
				_ = builder.Append(' ');
				_ = builder.Append(Type.ToString());
			}

			return builder.ToString();
		}

		public override string ToString()
		{
			return SourceString;
		}

		public int CompareTo(object obj)
		{
			var o = obj as ClientVersion;

			if (IsNull(o))
			{
				return 1;
			}

			if (Major > o.Major)
			{
				return 1;
			}

			if (Major < o.Major)
			{
				return -1;
			}

			if (Minor > o.Minor)
			{
				return 1;
			}

			if (Minor < o.Minor)
			{
				return -1;
			}

			if (Revision > o.Revision)
			{
				return 1;
			}

			if (Revision < o.Revision)
			{
				return -1;
			}

			if (Patch > o.Patch)
			{
				return 1;
			}

			if (Patch < o.Patch)
			{
				return -1;
			}

			return 0;
		}

		public int Compare(object x, object y)
		{
			return Compare(x as ClientVersion, y as ClientVersion);
		}

		public static int Compare(ClientVersion a, ClientVersion b)
		{
			if (IsNull(a) && IsNull(b))
			{
				return 0;
			}
			
			if (IsNull(a))
			{
				return -1;
			}
			
			if (IsNull(b))
			{
				return 1;
			}

			return a.CompareTo(b);
		}

		public static bool IsNull(object x)
		{
			return x is null;
		}

		public static bool operator ==(ClientVersion l, ClientVersion r)
		{
			return (Compare(l, r) == 0);
		}

		public static bool operator !=(ClientVersion l, ClientVersion r)
		{
			return (Compare(l, r) != 0);
		}

		public static bool operator >=(ClientVersion l, ClientVersion r)
		{
			return (Compare(l, r) >= 0);
		}

		public static bool operator >(ClientVersion l, ClientVersion r)
		{
			return (Compare(l, r) > 0);
		}

		public static bool operator <=(ClientVersion l, ClientVersion r)
		{
			return (Compare(l, r) <= 0);
		}

		public static bool operator <(ClientVersion l, ClientVersion r)
		{
			return (Compare(l, r) < 0);
		}
	}

	public sealed class CityInfo
	{
		private Point3D m_Location;

		public Point3D Location { get => m_Location; set => m_Location = value; }

		public int X { get => m_Location.X; set => m_Location.X = value; }
		public int Y { get => m_Location.Y; set => m_Location.Y = value; }
		public int Z { get => m_Location.Z; set => m_Location.Z = value; }

		public Map Map { get; set; }

		public string City { get; set; }
		public string Building { get; set; }
		public int Description { get; set; }

		public CityInfo(string city, string building, int x, int y, int z, Map m) 
			: this(city, building, 0, x, y, z, m)
		{
		}

		public CityInfo(string city, string building, int description, int x, int y, int z) 
			: this(city, building, description, x, y, z, Map.Trammel)
		{
		}

		public CityInfo(string city, string building, int x, int y, int z) 
			: this(city, building, 0, x, y, z, Map.Trammel)
		{
		}

		public CityInfo(string city, string building, int description, int x, int y, int z, Map map)
		{
			m_Location = new Point3D(x, y, z);
			Map = map;

			City = city;
			Building = building;
			Description = description;
		}
	}

	public readonly struct SkillNameValue
	{
		public SkillName Name { get; }
		public int Value { get; }

		public SkillNameValue(SkillName name, int value)
		{
			Name = name;
			Value = value;
		}
	}
}