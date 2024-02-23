using Server.Guilds;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Server
{
	public interface ISerializable
	{
		int TypeReference { get; }
		int SerialIdentity { get; }

		void Serialize(GenericWriter writer);
		void Deserialize(GenericReader reader);
	}

	public readonly struct UID : IComparable, IComparable<UID>
	{
		private readonly long m_UID;

		public static readonly UID MinusOne = new(-1L);
		public static readonly UID Zero = new(0L);

		public UID(long uid)
		{
			m_UID = uid;
		}

		public long Value => m_UID;

		public bool IsValid => m_UID > 0;

		public override int GetHashCode()
		{
			return m_UID.GetHashCode();
		}

		public int CompareTo(UID other)
		{
			return m_UID.CompareTo(other.m_UID);
		}

		public int CompareTo(object other)
		{
			if (other is UID u)
			{
				return CompareTo(u);
			}

			if (other == null)
			{
				return -1;
			}

			return 0;
		}

		public override bool Equals(object o)
		{
			if (o == null || o is not UID s)
			{
				return false;
			}

			return s.m_UID == m_UID;
		}

		public static bool operator ==(UID l, UID r)
		{
			return l.m_UID == r.m_UID;
		}

		public static bool operator !=(UID l, UID r)
		{
			return l.m_UID != r.m_UID;
		}

		public static bool operator >(UID l, UID r)
		{
			return l.m_UID > r.m_UID;
		}

		public static bool operator <(UID l, UID r)
		{
			return l.m_UID < r.m_UID;
		}

		public static bool operator >=(UID l, UID r)
		{
			return l.m_UID >= r.m_UID;
		}

		public static bool operator <=(UID l, UID r)
		{
			return l.m_UID <= r.m_UID;
		}

		public override string ToString()
		{
			return $"0x{m_UID:X8}";
		}

		public static implicit operator long(UID uid)
		{
			return uid.m_UID;
		}

		public static implicit operator UID(long uid)
		{
			return new UID(uid);
		}
	}

	public struct Serial : IComparable, IComparable<Serial>
	{
		public static readonly Serial MinusOne = new(-1);
		public static readonly Serial Zero = new(0);

		public static readonly Serial MinValue = new(0x00000000);
		public static readonly Serial MaxValue = new(0x7FFFFFFF);

		public static readonly Serial FirstMobile = new(0x00000001);
		public static readonly Serial FirstItem = new(0x40000001);

		private static Serial m_LastMobile = new(0x00000000);
		private static Serial m_LastItem = new(0x40000000);

		public static Serial NewMobile
		{
			get
			{
				do
				{
					++m_LastMobile.m_Serial;
				}
				while (World.FindMobile(m_LastMobile) != null);

				return m_LastMobile;
			}
		}

		public static Serial NewItem
		{
			get
			{
				do
				{
					++m_LastItem.m_Serial;
				}
				while (World.FindItem(m_LastItem) != null);

				return m_LastItem;
			}
		}

		private int m_Serial;

		public readonly int Value => m_Serial;

		public readonly bool IsMobile => m_Serial > 0 && m_Serial < 0x40000000;

		public readonly bool IsItem => m_Serial >= 0x40000000 && m_Serial <= 0x7FFFFFFF;

		public readonly bool IsValid => m_Serial > 0;

		public Serial(int serial)
		{
			m_Serial = serial;
		}

		public override readonly string ToString()
		{
			return $"0x{m_Serial:X8}";
		}

		public override readonly int GetHashCode()
		{
			return m_Serial;
		}

		public readonly int CompareTo(Serial other)
		{
			return m_Serial.CompareTo(other.m_Serial);
		}

		public readonly int CompareTo(object other)
		{
			if (other is Serial s)
			{
				return CompareTo(s);
			}
			
			if (other == null)
			{
				return -1;
			}

			return 0;
		}

		public override readonly bool Equals(object o)
		{
			if (o == null || o is not Serial s)
			{
				return false;
			}

			return s.m_Serial == m_Serial;
		}

		public static bool operator ==(Serial l, Serial r)
		{
			return l.m_Serial == r.m_Serial;
		}

		public static bool operator !=(Serial l, Serial r)
		{
			return l.m_Serial != r.m_Serial;
		}

		public static bool operator >(Serial l, Serial r)
		{
			return l.m_Serial > r.m_Serial;
		}

		public static bool operator <(Serial l, Serial r)
		{
			return l.m_Serial < r.m_Serial;
		}

		public static bool operator >=(Serial l, Serial r)
		{
			return l.m_Serial >= r.m_Serial;
		}

		public static bool operator <=(Serial l, Serial r)
		{
			return l.m_Serial <= r.m_Serial;
		}

		public static implicit operator int(Serial serial)
		{
			return serial.m_Serial;
		}
	}

	public abstract class GenericReader
	{
		protected GenericReader() { }

		public abstract int PeekInt();

		public abstract int ReadEncodedInt();
		public abstract uint ReadEncodedUInt();
		public abstract long ReadEncodedLong();
		public abstract ulong ReadEncodedULong();

		public abstract string ReadString();

		public abstract Type ReadObjectType();

		public abstract DateTime ReadDateTime();
		public abstract DateTimeOffset ReadDateTimeOffset();
		public abstract TimeSpan ReadTimeSpan();
		public abstract DateTime ReadDeltaTime();
		public abstract IPAddress ReadIPAddress();

		public abstract Enum ReadEnum();
		public abstract T ReadEnum<T>() where T : struct, Enum;

		public abstract Color ReadColor();

		public abstract decimal ReadDecimal();
		public abstract long ReadLong();
		public abstract ulong ReadULong();
		public abstract int ReadInt();
		public abstract uint ReadUInt();
		public abstract short ReadShort();
		public abstract ushort ReadUShort();
		public abstract double ReadDouble();
		public abstract float ReadFloat();
		public abstract char ReadChar();
		public abstract byte ReadByte();
		public abstract sbyte ReadSByte();
		public abstract bool ReadBool();

		public abstract byte[] ReadBytes();
		public abstract MemoryStream ReadStream();

		public abstract Point3D ReadPoint3D();
		public abstract Point2D ReadPoint2D();
		public abstract Rectangle2D ReadRect2D();
		public abstract Rectangle3D ReadRect3D();
		public abstract Poly2D ReadPoly2D();
		public abstract Poly3D ReadPoly3D();
		public abstract Map ReadMap();

		public abstract UID ReadUID();
		public abstract Serial ReadSerial();

		public abstract IEntity ReadEntity();
		public abstract Item ReadItem();
		public abstract Mobile ReadMobile();
		public abstract BaseGuild ReadGuild();
		public abstract Region ReadRegion();

		public abstract T ReadEntity<T>() where T : class, IEntity;
		public abstract T ReadItem<T>() where T : Item;
		public abstract T ReadMobile<T>() where T : Mobile;
		public abstract T ReadGuild<T>() where T : BaseGuild;
		public abstract T ReadRegion<T>() where T : Region;

		public abstract List<Item> ReadStrongItemList();
		public abstract List<T> ReadStrongItemList<T>() where T : Item;

		public abstract List<Mobile> ReadStrongMobileList();
		public abstract List<T> ReadStrongMobileList<T>() where T : Mobile;

		public abstract List<BaseGuild> ReadStrongGuildList();
		public abstract List<T> ReadStrongGuildList<T>() where T : BaseGuild;

		public abstract List<Region> ReadStrongRegionList();
		public abstract List<T> ReadStrongRegionList<T>() where T : Region;

		public abstract HashSet<Item> ReadItemSet();
		public abstract HashSet<T> ReadItemSet<T>() where T : Item;

		public abstract HashSet<Mobile> ReadMobileSet();
		public abstract HashSet<T> ReadMobileSet<T>() where T : Mobile;

		public abstract HashSet<BaseGuild> ReadGuildSet();
		public abstract HashSet<T> ReadGuildSet<T>() where T : BaseGuild;

		public abstract HashSet<Region> ReadRegionSet();
		public abstract HashSet<T> ReadRegionSet<T>() where T : Region;

		public abstract Race ReadRace();

		public abstract bool End();
	}

	public sealed class BinaryFileReader : GenericReader, IDisposable
	{
		private BinaryReader m_File;

		public BinaryFileReader(BinaryReader br)
		{
			m_File = br;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);

			Close();

			m_File = null;
		}

		public void Close()
		{
			m_File.Close();
		}

		public long Position => m_File.BaseStream.Position;

		public long Seek(long offset, SeekOrigin origin)
		{
			return m_File.BaseStream.Seek(offset, origin);
		}

		public override int PeekInt()
		{
			var peek = m_File.ReadInt32();

			m_File.BaseStream.Seek(-4, SeekOrigin.Current);

			return peek;
		}

		public override int ReadEncodedInt()
		{
			return (int)ReadEncodedUInt();
		}

		public override uint ReadEncodedUInt()
		{
			uint v = 0, b;
			var shift = 0;

			do
			{
				b = m_File.ReadByte();
				v |= (b & 0x7F) << shift;
				shift += 7;
			}
			while (b >= 0x80);

			return v;
		}

		public override long ReadEncodedLong()
		{
			return (long)ReadEncodedULong();
		}

		public override ulong ReadEncodedULong()
		{
			ulong v = 0, b;
			var shift = 0;

			do
			{
				b = m_File.ReadByte();
				v |= (b & 0x7F) << shift;
				shift += 7;
			}
			while (b >= 0x80);

			return v;
		}

		public override string ReadString()
		{
			if (ReadByte() != 0)
			{
				return m_File.ReadString();
			}
			
			return null;
		}

		public override Type ReadObjectType()
		{
			var hash = ReadEncodedInt();

			return ScriptCompiler.FindTypeByFullNameHash(hash);
		}

		public override DateTime ReadDeltaTime()
		{
			var ticks = m_File.ReadInt64();
			var now = DateTime.UtcNow.Ticks;

			if (ticks > 0 && (ticks + now) < 0)
			{
				return DateTime.MaxValue;
			}
			
			if (ticks < 0 && (ticks + now) < 0)
			{
				return DateTime.MinValue;
			}

			try
			{
				return new DateTime(now + ticks);
			}
			catch
			{
				if (ticks > 0)
				{
					return DateTime.MaxValue;
				}
				
				return DateTime.MinValue;
			}
		}

		public override DateTime ReadDateTime()
		{
			return new DateTime(m_File.ReadInt64());
		}

		public override DateTimeOffset ReadDateTimeOffset()
		{
			var ticks = m_File.ReadInt64();
			var offset = new TimeSpan(m_File.ReadInt64());

			return new DateTimeOffset(ticks, offset);
		}

		public override TimeSpan ReadTimeSpan()
		{
			return new TimeSpan(m_File.ReadInt64());
		}

		public override IPAddress ReadIPAddress()
		{
			return new IPAddress(m_File.ReadInt64());
		}

		public override Enum ReadEnum()
		{
			var type = ReadObjectType();
			var value = ReadEncodedULong();

			if (type == null)
			{
				_ = ReadEncodedULong();

				return default;
			}

			if (type?.IsEnum == true)
			{
				if ((int)Type.GetTypeCode(type) % 2 == 1)
				{
					return (Enum)Enum.ToObject(type, unchecked((long)value));
				}

				return (Enum)Enum.ToObject(type, value);
			}

			return default;
		}

		public override T ReadEnum<T>()
		{
			try { return (T)ReadEnum(); }
			catch { return default; }
		}

		public override Color ReadColor()
		{
			return ReadByte() switch
			{
				0 => Color.Empty,
				1 => Color.FromKnownColor(ReadEnum<KnownColor>()),
				2 => Color.FromName(ReadString()),
				3 => Color.FromArgb(ReadEncodedInt()),
				_ => Color.Empty,
			};
		}

		public override decimal ReadDecimal()
		{
			return m_File.ReadDecimal();
		}

		public override long ReadLong()
		{
			return m_File.ReadInt64();
		}

		public override ulong ReadULong()
		{
			return m_File.ReadUInt64();
		}

		public override int ReadInt()
		{
			return m_File.ReadInt32();
		}

		public override uint ReadUInt()
		{
			return m_File.ReadUInt32();
		}

		public override short ReadShort()
		{
			return m_File.ReadInt16();
		}

		public override ushort ReadUShort()
		{
			return m_File.ReadUInt16();
		}

		public override double ReadDouble()
		{
			return m_File.ReadDouble();
		}

		public override float ReadFloat()
		{
			return m_File.ReadSingle();
		}

		public override char ReadChar()
		{
			return m_File.ReadChar();
		}

		public override byte ReadByte()
		{
			return m_File.ReadByte();
		}

		public override sbyte ReadSByte()
		{
			return m_File.ReadSByte();
		}

		public override bool ReadBool()
		{
			return m_File.ReadBoolean();
		}

		public override byte[] ReadBytes()
		{
			var length = ReadEncodedInt();

			if (length >= 0)
			{
				return m_File.ReadBytes(length);
			}

			return null;
		}

		public override MemoryStream ReadStream()
		{
			var length = ReadEncodedInt();

			if (length >= 0)
			{
				return new MemoryStream(m_File.ReadBytes(length));
			}

			return null;
		}

		public override Point3D ReadPoint3D()
		{
			return new Point3D(ReadInt(), ReadInt(), ReadInt());
		}

		public override Point2D ReadPoint2D()
		{
			return new Point2D(ReadInt(), ReadInt());
		}

		public override Rectangle2D ReadRect2D()
		{
			return new Rectangle2D(ReadPoint2D(), ReadPoint2D());
		}

		public override Rectangle3D ReadRect3D()
		{
			return new Rectangle3D(ReadPoint3D(), ReadPoint3D());
		}

		public override Poly2D ReadPoly2D()
		{
			var points = new Point2D[ReadInt()];

			for (var i = 0; i < points.Length; i++)
			{
				points[i] = ReadPoint2D();
			}

			return new Poly2D(points);
		}

		public override Poly3D ReadPoly3D()
		{
			var minZ = ReadInt();
			var maxZ = ReadInt();

			var points = new Point2D[ReadInt()];

			for (var i = 0; i < points.Length; i++)
			{
				points[i] = ReadPoint2D();
			}

			return new Poly3D(minZ, maxZ, points);
		}

		public override Map ReadMap()
		{
			return Map.Maps[ReadByte()];
		}

		public override UID ReadUID()
		{
			return ReadLong();
		}

		public override Serial ReadSerial()
		{
			return new Serial(ReadInt());
		}

		public override IEntity ReadEntity()
		{
			return World.FindEntity(ReadSerial());
		}

		public override Item ReadItem()
		{
			return World.FindItem(ReadSerial());
		}

		public override Mobile ReadMobile()
		{
			return World.FindMobile(ReadSerial());
		}

		public override BaseGuild ReadGuild()
		{
			return BaseGuild.Find(ReadInt());
		}

		public override Region ReadRegion()
		{
			return Region.Find(ReadInt());
		}

		public override T ReadEntity<T>()
		{
			return ReadEntity() as T;
		}

		public override T ReadItem<T>()
		{
			return ReadItem() as T;
		}

		public override T ReadMobile<T>()
		{
			return ReadMobile() as T;
		}

		public override T ReadGuild<T>()
		{
			return ReadGuild() as T;
		}

		public override T ReadRegion<T>()
		{
			return ReadRegion() as T;
		}

		public override List<Item> ReadStrongItemList()
		{
			return ReadStrongItemList<Item>();
		}

		public override List<T> ReadStrongItemList<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new List<T>(count);

				for (var i = 0; i < count; ++i)
				{
					var item = ReadItem<T>();

					if (item != null)
					{
						list.Add(item);
					}
				}

				return list;
			}
			else
			{
				return new List<T>();
			}
		}

		public override HashSet<Item> ReadItemSet()
		{
			return ReadItemSet<Item>();
		}

		public override HashSet<T> ReadItemSet<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var set = new HashSet<T>();

				for (var i = 0; i < count; ++i)
				{
					var item = ReadItem<T>();

					if (item != null)
					{
						set.Add(item);
					}
				}

				return set;
			}
			else
			{
				return new HashSet<T>();
			}
		}

		public override List<Mobile> ReadStrongMobileList()
		{
			return ReadStrongMobileList<Mobile>();
		}

		public override List<T> ReadStrongMobileList<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new List<T>(count);

				for (var i = 0; i < count; ++i)
				{
					var m = ReadMobile<T>();

					if (m != null)
					{
						list.Add(m);
					}
				}

				return list;
			}
			else
			{
				return new List<T>();
			}
		}

		public override HashSet<Mobile> ReadMobileSet()
		{
			return ReadMobileSet<Mobile>();
		}

		public override HashSet<T> ReadMobileSet<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var set = new HashSet<T>();

				for (var i = 0; i < count; ++i)
				{
					var item = ReadMobile<T>();

					if (item != null)
					{
						set.Add(item);
					}
				}

				return set;
			}
			else
			{
				return new HashSet<T>();
			}
		}

		public override List<BaseGuild> ReadStrongGuildList()
		{
			return ReadStrongGuildList<BaseGuild>();
		}

		public override List<T> ReadStrongGuildList<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new List<T>(count);

				for (var i = 0; i < count; ++i)
				{
					var g = ReadGuild<T>();

					if (g != null)
					{
						list.Add(g);
					}
				}

				return list;
			}
			else
			{
				return new List<T>();
			}
		}

		public override HashSet<BaseGuild> ReadGuildSet()
		{
			return ReadGuildSet<BaseGuild>();
		}

		public override HashSet<T> ReadGuildSet<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var set = new HashSet<T>();

				for (var i = 0; i < count; ++i)
				{
					var item = ReadGuild<T>();

					if (item != null)
					{
						set.Add(item);
					}
				}

				return set;
			}
			else
			{
				return new HashSet<T>();
			}
		}

		public override List<Region> ReadStrongRegionList()
		{
			return ReadStrongRegionList<Region>();
		}

		public override List<T> ReadStrongRegionList<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var list = new List<T>(count);

				for (var i = 0; i < count; ++i)
				{
					var r = ReadRegion<T>();

					if (r != null)
					{
						list.Add(r);
					}
				}

				return list;
			}
			else
			{
				return new List<T>();
			}
		}

		public override HashSet<Region> ReadRegionSet()
		{
			return ReadRegionSet<Region>();
		}

		public override HashSet<T> ReadRegionSet<T>()
		{
			var count = ReadInt();

			if (count > 0)
			{
				var set = new HashSet<T>();

				for (var i = 0; i < count; ++i)
				{
					var r = ReadRegion<T>();

					if (r != null)
					{
						set.Add(r);
					}
				}

				return set;
			}
			else
			{
				return new HashSet<T>();
			}
		}

		public override Race ReadRace()
		{
			return Race.Races[ReadByte()];
		}

		public override bool End()
		{
			return m_File.PeekChar() == -1;
		}
	}

	public abstract class GenericWriter
	{
		protected GenericWriter() { }

		public abstract void Close();

		public abstract long Position { get; }

		public abstract void Write(string value);

		public abstract void WriteEncodedInt(int value);
		public abstract void WriteEncodedUInt(uint value);
		public abstract void WriteEncodedLong(long value);
		public abstract void WriteEncodedULong(ulong value);

		public abstract void WriteObjectType(object value);
		public abstract void WriteObjectType(Type value);

		public abstract void WriteDeltaTime(DateTime value);
		public abstract void Write(DateTime value);
		public abstract void Write(DateTimeOffset value);
		public abstract void Write(TimeSpan value);
		public abstract void Write(IPAddress value);
		public abstract void Write(Enum value);
		public abstract void Write(Color value);

		public abstract void Write(decimal value);
		public abstract void Write(long value);
		public abstract void Write(ulong value);
		public abstract void Write(int value);
		public abstract void Write(uint value);
		public abstract void Write(short value);
		public abstract void Write(ushort value);
		public abstract void Write(double value);
		public abstract void Write(float value);
		public abstract void Write(char value);
		public abstract void Write(byte value);
		public abstract void Write(sbyte value);
		public abstract void Write(bool value);

		public abstract void Write(byte[] value);
		public abstract void Write(byte[] value, int offset, int length);

		public abstract void Write(MemoryStream stream);
		public abstract void Write(MemoryStream stream, int offset, int length);

		public abstract void Write(Point3D value);
		public abstract void Write(Point2D value);
		public abstract void Write(Rectangle2D value);
		public abstract void Write(Rectangle3D value);
		public abstract void Write(Poly2D value);
		public abstract void Write(Poly3D value);
		public abstract void Write(Map value);

		public abstract void Write(UID value);
		public abstract void Write(Serial value);

		public abstract void Write(IEntity value);
		public abstract void Write(Item value);
		public abstract void Write(Mobile value);
		public abstract void Write(BaseGuild value);
		public abstract void Write(Region value);

		public abstract void Write(Race value);

		public abstract void Write(List<Item> list);
		public abstract void Write(List<Item> list, bool tidy);

		public abstract void WriteItemList<T>(List<T> list) where T : Item;
		public abstract void WriteItemList<T>(List<T> list, bool tidy) where T : Item;

		public abstract void Write(HashSet<Item> list);
		public abstract void Write(HashSet<Item> list, bool tidy);

		public abstract void WriteItemSet<T>(HashSet<T> set) where T : Item;
		public abstract void WriteItemSet<T>(HashSet<T> set, bool tidy) where T : Item;

		public abstract void Write(List<Mobile> list);
		public abstract void Write(List<Mobile> list, bool tidy);

		public abstract void WriteMobileList<T>(List<T> list) where T : Mobile;
		public abstract void WriteMobileList<T>(List<T> list, bool tidy) where T : Mobile;

		public abstract void Write(HashSet<Mobile> list);
		public abstract void Write(HashSet<Mobile> list, bool tidy);

		public abstract void WriteMobileSet<T>(HashSet<T> set) where T : Mobile;
		public abstract void WriteMobileSet<T>(HashSet<T> set, bool tidy) where T : Mobile;

		public abstract void Write(List<BaseGuild> list);
		public abstract void Write(List<BaseGuild> list, bool tidy);

		public abstract void WriteGuildList<T>(List<T> list) where T : BaseGuild;
		public abstract void WriteGuildList<T>(List<T> list, bool tidy) where T : BaseGuild;

		public abstract void Write(HashSet<BaseGuild> list);
		public abstract void Write(HashSet<BaseGuild> list, bool tidy);

		public abstract void WriteGuildSet<T>(HashSet<T> set) where T : BaseGuild;
		public abstract void WriteGuildSet<T>(HashSet<T> set, bool tidy) where T : BaseGuild;

		public abstract void Write(List<Region> list);
		public abstract void Write(List<Region> list, bool tidy);

		public abstract void WriteRegionList<T>(List<T> list) where T : Region;
		public abstract void WriteRegionList<T>(List<T> list, bool tidy) where T : Region;

		public abstract void Write(HashSet<Region> list);
		public abstract void Write(HashSet<Region> list, bool tidy);

		public abstract void WriteRegionSet<T>(HashSet<T> set) where T : Region;
		public abstract void WriteRegionSet<T>(HashSet<T> set, bool tidy) where T : Region;
	}

	public class BinaryFileWriter : GenericWriter, IDisposable
	{
		private readonly bool m_PrefixStrings;

		private Stream m_File;

		protected virtual int BufferSize => 64 * 1024;

		private byte[] m_Buffer;

		private int m_Index;

		private Encoding m_Encoding;

		public BinaryFileWriter(Stream strm, bool prefixStr)
		{
			m_Encoding = Utility.UTF8;

			m_Buffer = new byte[BufferSize];

			m_File = strm;

			m_PrefixStrings = prefixStr;
		}

		public BinaryFileWriter(string filename, bool prefixStr)
		{
			m_Buffer = new byte[BufferSize];

			m_File = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);

			m_Encoding = Utility.UTF8WithEncoding;

			m_PrefixStrings = prefixStr;
		}

		public virtual void Dispose()
		{
			GC.SuppressFinalize(this);

			Close();

			m_File = null;
			m_Buffer = null;
			m_Encoding = null;
		}

		public void Flush()
		{
			if (m_Index > 0)
			{
				m_Position += m_Index;

				m_File.Write(m_Buffer, 0, m_Index);
				m_Index = 0;
			}
		}

		private long m_Position;

		public override long Position => m_Position + m_Index;

		public Stream UnderlyingStream
		{
			get
			{
				if (m_Index > 0)
				{
					Flush();
				}

				return m_File;
			}
		}

		public long Seek(long offset, SeekOrigin origin)
		{
			return UnderlyingStream.Seek(offset, origin);
		}

		public override void Close()
		{
			if (m_Index > 0)
			{
				Flush();
			}

			m_File.Close();
		}

		public override void WriteEncodedInt(int value)
		{
			WriteEncodedUInt((uint)value);
		}

		public override void WriteEncodedUInt(uint value)
		{
			while (value >= 0x80)
			{
				if (m_Index + 1 > m_Buffer.Length)
				{
					Flush();
				}

				m_Buffer[m_Index++] = (byte)(value | 0x80);

				value >>= 7;
			}

			if (m_Index + 1 > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index++] = (byte)value;
		}

		public override void WriteEncodedLong(long value)
		{
			WriteEncodedULong((ulong)value);
		}

		public override void WriteEncodedULong(ulong value)
		{
			while (value >= 0x80)
			{
				if (m_Index + 1 > m_Buffer.Length)
				{
					Flush();
				}

				m_Buffer[m_Index++] = (byte)(value | 0x80);

				value >>= 7;
			}

			if (m_Index + 1 > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index++] = (byte)value;
		}

		private byte[] m_CharacterBuffer;
		private int m_MaxBufferChars;
		private const int LargeByteBufferSize = 256;

		internal void InternalWriteString(string value)
		{
			var length = m_Encoding.GetByteCount(value);

			WriteEncodedInt(length);

			if (m_CharacterBuffer == null)
			{
				m_CharacterBuffer = new byte[LargeByteBufferSize];
				m_MaxBufferChars = LargeByteBufferSize / m_Encoding.GetMaxByteCount(1);
			}

			if (length > LargeByteBufferSize)
			{
				var current = 0;
				var charsLeft = value.Length;

				while (charsLeft > 0)
				{
					var charCount = (charsLeft > m_MaxBufferChars) ? m_MaxBufferChars : charsLeft;
					var byteLength = m_Encoding.GetBytes(value, current, charCount, m_CharacterBuffer, 0);

					if ((m_Index + byteLength) > m_Buffer.Length)
					{
						Flush();
					}

					Buffer.BlockCopy(m_CharacterBuffer, 0, m_Buffer, m_Index, byteLength);
					m_Index += byteLength;

					current += charCount;
					charsLeft -= charCount;
				}
			}
			else
			{
				var byteLength = m_Encoding.GetBytes(value, 0, value.Length, m_CharacterBuffer, 0);

				if ((m_Index + byteLength) > m_Buffer.Length)
				{
					Flush();
				}

				Buffer.BlockCopy(m_CharacterBuffer, 0, m_Buffer, m_Index, byteLength);
				m_Index += byteLength;
			}
		}

		public override void Write(string value)
		{
			if (m_PrefixStrings)
			{
				if (value == null)
				{
					if ((m_Index + 1) > m_Buffer.Length)
					{
						Flush();
					}

					m_Buffer[m_Index++] = 0;
				}
				else
				{
					if ((m_Index + 1) > m_Buffer.Length)
					{
						Flush();
					}

					m_Buffer[m_Index++] = 1;

					InternalWriteString(value);
				}
			}
			else
			{
				InternalWriteString(value);
			}
		}

		public override void WriteObjectType(object value)
		{
			WriteObjectType(value?.GetType());
		}

		public override void WriteObjectType(Type value)
		{
			var hash = ScriptCompiler.FindHashByFullName(value?.FullName);

			WriteEncodedInt(hash);
		}

		public override void WriteDeltaTime(DateTime value)
		{
			var ticks = value.Ticks;
			var now = (value.Kind == DateTimeKind.Local ? DateTime.Now : DateTime.UtcNow).Ticks;

			TimeSpan d;

			try
			{
				d = new TimeSpan(ticks - now);
			}
			catch
			{
				d = TimeSpan.MinValue;
			}

			Write(d);
		}

		public override void Write(DateTime value)
		{
			Write(value.Ticks);
		}

		public override void Write(DateTimeOffset value)
		{
			Write(value.Ticks);
			Write(value.Offset.Ticks);
		}

		public override void Write(TimeSpan value)
		{
			Write(value.Ticks);
		}

		public override void Write(IPAddress value)
		{
			Write(Utility.GetLongAddressValue(value));
		}

		public override void Write(Enum value)
		{
			WriteObjectType(value);

			if (value != null)
			{
				if ((int)value.GetTypeCode() % 2 == 1)
				{
					WriteEncodedLong(Convert.ToInt64(value));
				}
				else
				{
					WriteEncodedULong(Convert.ToUInt64(value));
				}
			}
		}

		public override void Write(Color value)
		{
			if (value.IsEmpty)
			{
				Write((byte)0);
			}
			else if (value.IsKnownColor)
			{
				Write((byte)1);
				Write(value.ToKnownColor());
			}
			else if (value.IsNamedColor)
			{
				Write((byte)2);
				Write(value.Name);
			}
			else
			{
				Write((byte)3);
				WriteEncodedInt(value.ToArgb());
			}
		}

		public override void Write(decimal value)
		{
			var bits = Decimal.GetBits(value);

			for (var i = 0; i < bits.Length; ++i)
			{
				Write(bits[i]);
			}
		}

		public override void Write(long value)
		{
			if ((m_Index + 8) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Buffer[m_Index + 2] = (byte)(value >> 16);
			m_Buffer[m_Index + 3] = (byte)(value >> 24);
			m_Buffer[m_Index + 4] = (byte)(value >> 32);
			m_Buffer[m_Index + 5] = (byte)(value >> 40);
			m_Buffer[m_Index + 6] = (byte)(value >> 48);
			m_Buffer[m_Index + 7] = (byte)(value >> 56);
			m_Index += 8;
		}

		public override void Write(ulong value)
		{
			if ((m_Index + 8) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Buffer[m_Index + 2] = (byte)(value >> 16);
			m_Buffer[m_Index + 3] = (byte)(value >> 24);
			m_Buffer[m_Index + 4] = (byte)(value >> 32);
			m_Buffer[m_Index + 5] = (byte)(value >> 40);
			m_Buffer[m_Index + 6] = (byte)(value >> 48);
			m_Buffer[m_Index + 7] = (byte)(value >> 56);
			m_Index += 8;
		}

		public override void Write(int value)
		{
			if ((m_Index + 4) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Buffer[m_Index + 2] = (byte)(value >> 16);
			m_Buffer[m_Index + 3] = (byte)(value >> 24);
			m_Index += 4;
		}

		public override void Write(uint value)
		{
			if ((m_Index + 4) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Buffer[m_Index + 2] = (byte)(value >> 16);
			m_Buffer[m_Index + 3] = (byte)(value >> 24);
			m_Index += 4;
		}

		public override void Write(short value)
		{
			if ((m_Index + 2) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Index += 2;
		}

		public override void Write(ushort value)
		{
			if ((m_Index + 2) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index] = (byte)value;
			m_Buffer[m_Index + 1] = (byte)(value >> 8);
			m_Index += 2;
		}

		public override unsafe void Write(double value)
		{
			if ((m_Index + 8) > m_Buffer.Length)
			{
				Flush();
			}

#if MONO
			byte[] bytes = BitConverter.GetBytes(value);

			for(int i = 0; i < bytes.Length; i++)
			{
				m_Buffer[m_Index++] = bytes[i];
			}
#else
			fixed (byte* pBuffer = m_Buffer)
			{
				*(double*)(pBuffer + m_Index) = value;
			}

			m_Index += 8;
#endif
		}

		public override unsafe void Write(float value)
		{
			if ((m_Index + 4) > m_Buffer.Length)
			{
				Flush();
			}

#if MONO
			byte[] bytes = BitConverter.GetBytes(value);

			for(int i = 0; i < bytes.Length; i++)
			{
				m_Buffer[m_Index++] = bytes[i];
			}
#else
			fixed (byte* pBuffer = m_Buffer)
			{
				*(float*)(pBuffer + m_Index) = value;
			}

			m_Index += 4;
#endif
		}

		private readonly char[] m_SingleCharBuffer = new char[1];

		public override void Write(char value)
		{
			if ((m_Index + 8) > m_Buffer.Length)
			{
				Flush();
			}

			m_SingleCharBuffer[0] = value;

			var byteCount = m_Encoding.GetBytes(m_SingleCharBuffer, 0, 1, m_Buffer, m_Index);
			m_Index += byteCount;
		}

		public override void Write(byte value)
		{
			if ((m_Index + 1) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index++] = value;
		}

		public override void Write(sbyte value)
		{
			if ((m_Index + 1) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index++] = (byte)value;
		}

		public override void Write(bool value)
		{
			if ((m_Index + 1) > m_Buffer.Length)
			{
				Flush();
			}

			m_Buffer[m_Index++] = (byte)(value ? 1 : 0);
		}

		public override void Write(byte[] value)
		{
			Write(value, 0, value?.Length ?? 0);
		}

		public override void Write(byte[] value, int offset, int length)
		{
			if (value != null)
			{
				WriteEncodedInt(length);

				if (length > 0)
				{
					length += offset;

					for (var i = offset; i < length; i++)
					{
						Write(value[i]);
					}
				}
			}
			else
			{
				WriteEncodedInt(-1);
			}
		}

		public override void Write(MemoryStream value)
		{
			Write(value, 0, (int)(value?.Length ?? 0));
		}

		public override void Write(MemoryStream value, int offset, int length)
		{
			if (value != null)
			{
				WriteEncodedInt(length);

				if (length > 0)
				{
					var pos = value.Position;

					value.Position = offset;

					var count = 0;

					while (count < length)
					{
						var read = value.ReadByte();

						if (read >= 0)
						{
							Write((byte)read);

							++count;
						}
						else
						{
							break;
						}
					}

					if (count > 0)
					{
						Flush();
					}

					value.Position = pos;
				}
			}
			else
			{
				WriteEncodedInt(-1);
			}
		}

		public override void Write(Point3D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
			Write(value.m_Z);
		}

		public override void Write(Point2D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
		}

		public override void Write(Rectangle2D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Rectangle3D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Poly2D value)
		{
			Write(value.Count);

			for (var i = 0; i < value.Count; i++)
			{
				Write(value[i]);
			}
		}

		public override void Write(Poly3D value)
		{
			Write(value.MinZ);
			Write(value.MaxZ);

			Write(value.Count);

			for (var i = 0; i < value.Count; i++)
			{
				Write(value[i]);
			}
		}

		public override void Write(Map value)
		{
			if (value != null)
			{
				Write((byte)value.MapIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(Race value)
		{
			if (value != null)
			{
				Write((byte)value.RaceIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(UID value)
		{
			Write(value.Value);
		}

		public override void Write(Serial value)
		{
			Write(value.Value);
		}

		public override void Write(IEntity value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(Item value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(Mobile value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(BaseGuild value)
		{
			if (value == null)
			{
				Write(0);
			}
			else
			{
				Write(value.Id);
			}
		}

		public override void Write(Region value)
		{
			if (value == null)
			{
				Write(0);
			}
			else
			{
				Write(value.Id);
			}
		}

		public override void Write(List<Item> list)
		{
			Write(list, false);
		}

		public override void Write(List<Item> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(item => item.Deleted);
			}

			WriteCollection(list, Write);
		}

		public override void WriteItemList<T>(List<T> list)
		{
			WriteItemList<T>(list, false);
		}

		public override void WriteItemList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(item => item.Deleted);
			}

			WriteCollection(list, Write);
		}

		public override void Write(HashSet<Item> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Item> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(item => item.Deleted);
			}

			WriteCollection(set, Write);
		}

		public override void WriteItemSet<T>(HashSet<T> set)
		{
			WriteItemSet(set, false);
		}

		public override void WriteItemSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(item => item.Deleted);
			}

			WriteCollection(set, Write);
		}

		public override void Write(List<Mobile> list)
		{
			Write(list, false);
		}

		public override void Write(List<Mobile> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(mob => mob.Deleted);
			}

			WriteCollection(list, Write);
		}

		public override void WriteMobileList<T>(List<T> list)
		{
			WriteMobileList<T>(list, false);
		}

		public override void WriteMobileList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(mob => mob.Deleted);
			}

			WriteCollection(list, Write);
		}

		public override void Write(HashSet<Mobile> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Mobile> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(mobile => mobile.Deleted);
			}

			WriteCollection(set, Write);
		}

		public override void WriteMobileSet<T>(HashSet<T> set)
		{
			WriteMobileSet(set, false);
		}

		public override void WriteMobileSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(mob => mob.Deleted);
			}

			WriteCollection(set, Write);
		}

		public override void Write(List<BaseGuild> list)
		{
			Write(list, false);
		}

		public override void Write(List<BaseGuild> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(guild => guild.Disbanded);
			}

			WriteCollection(list, Write);
		}

		public override void WriteGuildList<T>(List<T> list)
		{
			WriteGuildList<T>(list, false);
		}

		public override void WriteGuildList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(guild => guild.Disbanded);
			}

			WriteCollection(list, Write);
		}

		public override void Write(HashSet<BaseGuild> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<BaseGuild> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(guild => guild.Disbanded);
			}

			WriteCollection(set, Write);
		}

		public override void WriteGuildSet<T>(HashSet<T> set)
		{
			WriteGuildSet(set, false);
		}

		public override void WriteGuildSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(guild => guild.Disbanded);
			}

			WriteCollection(set, Write);
		}

		public override void Write(List<Region> list)
		{
			Write(list, false);
		}

		public override void Write(List<Region> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(reg => !reg.Registered);
			}

			WriteCollection(list, Write);
		}

		public override void WriteRegionList<T>(List<T> list)
		{
			WriteRegionList(list, false);
		}

		public override void WriteRegionList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(reg => !reg.Registered);
			}

			WriteCollection(list, Write);
		}

		public override void Write(HashSet<Region> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Region> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(reg => !reg.Registered);
			}

			WriteCollection(set, Write);
		}

		public override void WriteRegionSet<T>(HashSet<T> set)
		{
			WriteRegionSet(set, false);
		}

		public override void WriteRegionSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(reg => !reg.Registered);
			}

			WriteCollection(set, Write);
		}

		private void WriteCollection<T>(ICollection col, Action<T> write)
		{
			Write(col.Count);

			foreach (T o in col)
			{
				write(o);
			}
		}

		private void WriteCollection<T>(ICollection<T> col, Action<T> write)
		{
			Write(col.Count);

			foreach (var o in col)
			{
				write(o);
			}
		}
	}

	public sealed class AsyncWriter : GenericWriter
	{
		private static int m_ThreadCount = 0;
		public static int ThreadCount => m_ThreadCount;

		private readonly int BufferSize;

		private long m_LastPos, m_CurPos;
		private bool m_Closed;
		private readonly bool PrefixStrings;

		private MemoryStream m_Mem;
		private BinaryWriter m_Bin;
		private readonly FileStream m_File;

		private readonly Queue<MemoryStream> m_WriteQueue;
		private Thread m_WorkerThread;

		public MemoryStream MemStream
		{
			get => m_Mem;
			set
			{
				if (m_Mem.Length > 0)
				{
					Enqueue(m_Mem);
				}

				m_Mem = value;
				m_Bin = new BinaryWriter(m_Mem, Utility.UTF8WithEncoding);
				m_LastPos = 0;
				m_CurPos = m_Mem.Length;
				m_Mem.Seek(0, SeekOrigin.End);
			}
		}

		public override long Position => m_CurPos;

		public AsyncWriter(string filename, bool prefix)
			: this(filename, 1048576, prefix)//1 mb buffer
		{
		}

		public AsyncWriter(string filename, int buffSize, bool prefix)
		{
			PrefixStrings = prefix;
			m_Closed = false;
			m_WriteQueue = new();
			BufferSize = buffSize;

			m_File = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
			m_Mem = new MemoryStream(BufferSize + 1024);
			m_Bin = new BinaryWriter(m_Mem, Utility.UTF8WithEncoding);
		}

		private void Enqueue(MemoryStream mem)
		{
			lock (m_WriteQueue)
			{
				m_WriteQueue.Enqueue(mem);
			}

			if (m_WorkerThread == null || !m_WorkerThread.IsAlive)
			{
				m_WorkerThread = new Thread(new ThreadStart(new WorkerThread(this).Worker))
				{
					Priority = ThreadPriority.BelowNormal
				};

				m_WorkerThread.Start();
			}
		}

		private void OnWrite()
		{
			var curlen = m_Mem.Length;

			m_CurPos += curlen - m_LastPos;
			m_LastPos = curlen;

			if (curlen >= BufferSize)
			{
				Enqueue(m_Mem);
				m_Mem = new MemoryStream(BufferSize + 1024);
				m_Bin = new BinaryWriter(m_Mem, Utility.UTF8WithEncoding);
				m_LastPos = 0;
			}
		}

		public override void Close()
		{
			Enqueue(m_Mem);

			m_Closed = true;
		}

		public override void WriteEncodedInt(int value)
		{
			WriteEncodedUInt((uint)value);
		}

		public override void WriteEncodedUInt(uint value)
		{
			while (value >= 0x80)
			{
				m_Bin.Write((byte)(value | 0x80));

				value >>= 7;
			}

			m_Bin.Write((byte)value);

			OnWrite();
		}

		public override void WriteEncodedLong(long value)
		{
			WriteEncodedULong((ulong)value);
		}

		public override void WriteEncodedULong(ulong value)
		{
			while (value >= 0x80)
			{
				m_Bin.Write((byte)(value | 0x80));

				value >>= 7;
			}

			m_Bin.Write((byte)value);

			OnWrite();
		}

		public override void Write(string value)
		{
			if (PrefixStrings)
			{
				if (value == null)
				{
					m_Bin.Write((byte)0);
				}
				else
				{
					m_Bin.Write((byte)1);
					m_Bin.Write(value);
				}
			}
			else
			{
				m_Bin.Write(value);
			}

			OnWrite();
		}

		public override void WriteObjectType(object value)
		{
			WriteObjectType(value?.GetType());
		}

		public override void WriteObjectType(Type value)
		{
			var hash = ScriptCompiler.FindHashByFullName(value?.FullName);

			WriteEncodedInt(hash);
		}

		public override void WriteDeltaTime(DateTime value)
		{
			var ticks =  value.Ticks;
			var now = (value.Kind == DateTimeKind.Local ? DateTime.Now : DateTime.UtcNow).Ticks;

			TimeSpan d;

			try
			{
				d = new TimeSpan(ticks - now);
			}
			catch
			{
				d = TimeSpan.MinValue;
			}

			Write(d);
		}

		public override void Write(DateTime value)
		{
			m_Bin.Write(value.Ticks);
			OnWrite();
		}

		public override void Write(DateTimeOffset value)
		{
			m_Bin.Write(value.Ticks);
			m_Bin.Write(value.Offset.Ticks);
			OnWrite();
		}

		public override void Write(TimeSpan value)
		{
			m_Bin.Write(value.Ticks);
			OnWrite();
		}

		public override void Write(IPAddress value)
		{
			m_Bin.Write(Utility.GetLongAddressValue(value));
			OnWrite();
		}

		public override void Write(Enum value)
		{
			WriteObjectType(value);

			if (value != null)
			{
				if ((byte)value.GetTypeCode() % 2 == 1)
				{
					WriteEncodedLong(Convert.ToInt64(value));
				}
				else
				{
					WriteEncodedULong(Convert.ToUInt64(value));
				}
			}
		}

		public override void Write(Color value)
		{
			if (value.IsEmpty)
			{
				Write((byte)0);
			}
			else if (value.IsKnownColor)
			{
				Write((byte)1);
				Write(value.ToKnownColor());
			}
			else if (value.IsNamedColor)
			{
				Write((byte)2);
				Write(value.Name);
			}
			else
			{
				Write((byte)3);
				WriteEncodedInt(value.ToArgb());
			}
		}

		public override void Write(decimal value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(long value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(ulong value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(int value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(uint value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(short value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(ushort value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(double value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(float value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(char value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(byte value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(sbyte value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(bool value)
		{
			m_Bin.Write(value);
			OnWrite();
		}

		public override void Write(byte[] value)
		{
			Write(value, 0, value?.Length ?? 0);
		}

		public override void Write(byte[] value, int offset, int length)
		{
			if (value != null)
			{
				WriteEncodedInt(length);

				m_Bin.Write(value, offset, length);
				OnWrite();
			}
			else
			{
				WriteEncodedInt(-1);
			}
		}

		public override void Write(MemoryStream value)
		{
			Write(value, 0, (int)(value?.Length ?? 0));
		}

		public override void Write(MemoryStream value, int offset, int length)
		{
			if (value != null)
			{
				WriteEncodedInt(length);

				if (length > 0)
				{
					var pos = value.Position;

					value.Position = offset;

					var count = 0;

					while (count < length)
					{
						var read = value.ReadByte();

						if (read >= 0)
						{
							m_Bin.Write((byte)read);

							++count;
						}
						else
						{
							break;
						}
					}

					if (count > 0)
					{
						OnWrite();
					}

					value.Position = pos;
				}
			}
			else
			{
				WriteEncodedInt(-1);
			}
		}

		public override void Write(Point3D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
			Write(value.m_Z);
		}

		public override void Write(Point2D value)
		{
			Write(value.m_X);
			Write(value.m_Y);
		}

		public override void Write(Rectangle2D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Rectangle3D value)
		{
			Write(value.Start);
			Write(value.End);
		}

		public override void Write(Poly2D value)
		{
			Write(value.Count);

			for (var i = 0; i < value.Count; i++)
			{
				Write(value[i]);
			}
		}

		public override void Write(Poly3D value)
		{
			Write(value.MinZ);
			Write(value.MaxZ);

			Write(value.Count);

			for (var i = 0; i < value.Count; i++)
			{
				Write(value[i]);
			}
		}

		public override void Write(Map value)
		{
			if (value != null)
			{
				Write((byte)value.MapIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(Race value)
		{
			if (value != null)
			{
				Write((byte)value.RaceIndex);
			}
			else
			{
				Write((byte)0xFF);
			}
		}

		public override void Write(UID value)
		{
			Write(value.Value);
		}

		public override void Write(Serial value)
		{
			Write(value.Value);
		}

		public override void Write(IEntity value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(Item value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(Mobile value)
		{
			if (value == null || value.Deleted)
			{
				Write(Serial.MinusOne);
			}
			else
			{
				Write(value.Serial);
			}
		}

		public override void Write(BaseGuild value)
		{
			if (value == null)
			{
				Write(0);
			}
			else
			{
				Write(value.Id);
			}
		}

		public override void Write(Region value)
		{
			if (value == null)
			{
				Write(0);
			}
			else
			{
				Write(value.Id);
			}
		}

		public override void Write(List<Item> list)
		{
			Write(list, false);
		}

		public override void Write(List<Item> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(item => item.Deleted);
			}

			WriteCollection(list, Write);
		}

		public override void WriteItemList<T>(List<T> list)
		{
			WriteItemList(list, false);
		}

		public override void WriteItemList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(item => item.Deleted);
			}

			WriteCollection(list, Write);
		}

		public override void Write(HashSet<Item> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Item> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(item => item.Deleted);
			}

			WriteCollection(set, Write);
		}

		public override void WriteItemSet<T>(HashSet<T> set)
		{
			WriteItemSet(set, false);
		}

		public override void WriteItemSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(item => item.Deleted);
			}

			WriteCollection(set, Write);
		}

		public override void Write(List<Mobile> list)
		{
			Write(list, false);
		}

		public override void Write(List<Mobile> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(mob => mob.Deleted);
			}

			WriteCollection(list, Write);
		}

		public override void WriteMobileList<T>(List<T> list)
		{
			WriteMobileList(list, false);
		}

		public override void WriteMobileList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(mob => mob.Deleted);
			}

			WriteCollection(list, Write);
		}

		public override void Write(HashSet<Mobile> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Mobile> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(mobile => mobile.Deleted);
			}

			WriteCollection(set, Write);
		}

		public override void WriteMobileSet<T>(HashSet<T> set)
		{
			WriteMobileSet(set, false);
		}

		public override void WriteMobileSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(mob => mob.Deleted);
			}

			WriteCollection(set, Write);
		}

		public override void Write(List<BaseGuild> list)
		{
			Write(list, false);
		}

		public override void Write(List<BaseGuild> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(guild => guild.Disbanded);
			}

			WriteCollection(list, Write);
		}

		public override void WriteGuildList<T>(List<T> list)
		{
			WriteGuildList(list, false);
		}

		public override void WriteGuildList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(guild => guild.Disbanded);
			}

			WriteCollection(list, Write);
		}

		public override void Write(HashSet<BaseGuild> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<BaseGuild> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(guild => guild.Disbanded);
			}

			WriteCollection(set, Write);
		}

		public override void WriteGuildSet<T>(HashSet<T> set)
		{
			WriteGuildSet(set, false);
		}

		public override void WriteGuildSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(guild => guild.Disbanded);
			}

			WriteCollection(set, Write);
		}

		public override void Write(List<Region> list)
		{
			Write(list, false);
		}

		public override void Write(List<Region> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(reg => !reg.Registered);
			}

			WriteCollection(list, Write);
		}

		public override void WriteRegionList<T>(List<T> list)
		{
			WriteRegionList(list, false);
		}

		public override void WriteRegionList<T>(List<T> list, bool tidy)
		{
			if (tidy)
			{
				list.RemoveAll(reg => !reg.Registered);
			}

			WriteCollection(list, Write);
		}

		public override void Write(HashSet<Region> set)
		{
			Write(set, false);
		}

		public override void Write(HashSet<Region> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(reg => !reg.Registered);
			}

			WriteCollection(set, Write);
		}

		public override void WriteRegionSet<T>(HashSet<T> set)
		{
			WriteRegionSet(set, false);
		}

		public override void WriteRegionSet<T>(HashSet<T> set, bool tidy)
		{
			if (tidy)
			{
				set.RemoveWhere(reg => !reg.Registered);
			}

			WriteCollection(set, Write);
		}

		private void WriteCollection<T>(ICollection<T> col, Action<T> write)
		{
			Write(col.Count);

			foreach (var o in col)
			{
				write(o);
			}
		}

		private class WorkerThread
		{
			private readonly AsyncWriter m_Owner;

			public WorkerThread(AsyncWriter owner)
			{
				m_Owner = owner;
			}

			public void Worker()
			{
				m_ThreadCount++;

				var lastCount = 0;

				do
				{
					MemoryStream mem = null;

					lock (m_Owner.m_WriteQueue)
					{
						if ((lastCount = m_Owner.m_WriteQueue.Count) > 0)
						{
							mem = m_Owner.m_WriteQueue.Dequeue();
						}
					}

					if (mem != null && mem.Length > 0)
					{
						mem.WriteTo(m_Owner.m_File);
					}
				} while (lastCount > 1);

				if (m_Owner.m_Closed)
				{
					m_Owner.m_File.Close();
				}

				m_ThreadCount--;

				if (m_ThreadCount <= 0)
				{
					World.NotifyDiskWriteComplete();
				}
			}
		}
	}
}