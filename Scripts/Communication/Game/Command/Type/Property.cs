using Server.Accounting;
using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.HuePickers;
using Server.Items;
using Server.Network;
using Server.Prompts;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Server
{
	public abstract class PropertyException : ApplicationException
	{
		public Property Property { get; }

		public PropertyException(Property property, string message)
			: base(message)
		{
			Property = property;
		}
	}

	public abstract class BindingException : PropertyException
	{
		public BindingException(Property property, string message)
			: base(property, message)
		{
		}
	}

	public sealed class NotYetBoundException : BindingException
	{
		public NotYetBoundException(Property property)
			: base(property, "Property has not yet been bound.")
		{
		}
	}

	public sealed class AlreadyBoundException : BindingException
	{
		public AlreadyBoundException(Property property)
			: base(property, "Property has already been bound.")
		{
		}
	}

	public sealed class UnknownPropertyException : BindingException
	{
		public UnknownPropertyException(Property property, string current)
			: base(property, $"Property '{current}' not found.")
		{
		}
	}

	public sealed class ReadOnlyException : BindingException
	{
		public ReadOnlyException(Property property)
			: base(property, "Property is read-only.")
		{
		}
	}

	public sealed class WriteOnlyException : BindingException
	{
		public WriteOnlyException(Property property)
			: base(property, "Property is write-only.")
		{
		}
	}

	public abstract class AccessException : PropertyException
	{
		public AccessException(Property property, string message)
			: base(property, message)
		{
		}
	}

	public sealed class InternalAccessException : AccessException
	{
		public InternalAccessException(Property property)
			: base(property, "Property is internal.")
		{
		}
	}

	public abstract class ClearanceException : AccessException
	{
		public AccessLevel PlayerAccess { get; }
		public AccessLevel NeededAccess { get; }

		public ClearanceException(Property property, AccessLevel playerAccess, AccessLevel neededAccess, string accessType)
			: base(property, $"You must be at least {Mobile.GetAccessLevelName(neededAccess)} to {accessType} this property.")
		{
			PlayerAccess = playerAccess;
			NeededAccess = neededAccess;
		}
	}

	public sealed class ReadAccessException : ClearanceException
	{
		public ReadAccessException(Property property, AccessLevel playerAccess, AccessLevel neededAccess)
			: base(property, playerAccess, neededAccess, "read")
		{
		}
	}

	public sealed class WriteAccessException : ClearanceException
	{
		public WriteAccessException(Property property, AccessLevel playerAccess, AccessLevel neededAccess)
			: base(property, playerAccess, neededAccess, "write")
		{
		}
	}

	public sealed class Property
	{
		private PropertyInfo[] m_Chain;

		public string Binding { get; }

		public bool IsBound => m_Chain != null;

		public PropertyAccess Access { get; private set; }

		public PropertyInfo[] Chain
		{
			get
			{
				if (!IsBound)
				{
					throw new NotYetBoundException(this);
				}

				return m_Chain;
			}
		}

		public Type Type
		{
			get
			{
				if (!IsBound)
				{
					throw new NotYetBoundException(this);
				}

				return m_Chain[m_Chain.Length - 1].PropertyType;
			}
		}

		public bool CheckAccess(Mobile from)
		{
			if (!IsBound)
			{
				throw new NotYetBoundException(this);
			}

			for (var i = 0; i < m_Chain.Length; ++i)
			{
				var prop = m_Chain[i];

				var isFinal = i == (m_Chain.Length - 1);

				var access = Access;

				if (!isFinal)
				{
					access |= PropertyAccess.Read;
				}

				var security = Props.GetCPA(prop);

				if (security == null)
				{
					throw new InternalAccessException(this);
				}

				if (access.HasFlag(PropertyAccess.Read) && from.AccessLevel < security.ReadLevel)
				{
					throw new ReadAccessException(this, from.AccessLevel, security.ReadLevel);
				}

				if (access.HasFlag(PropertyAccess.Write) && (from.AccessLevel < security.WriteLevel || security.ReadOnly))
				{
					throw new WriteAccessException(this, from.AccessLevel, security.ReadLevel);
				}
			}

			return true;
		}

		public void BindTo(Type objectType, PropertyAccess desiredAccess)
		{
			if (IsBound)
			{
				throw new AlreadyBoundException(this);
			}

			var split = Binding.Split('.');

			var chain = new PropertyInfo[split.Length];

			for (var i = 0; i < split.Length; ++i)
			{
				var isFinal = i == (chain.Length - 1);

				chain[i] = objectType.GetProperty(split[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

				if (chain[i] == null)
				{
					throw new UnknownPropertyException(this, split[i]);
				}

				objectType = chain[i].PropertyType;

				var access = desiredAccess;

				if (!isFinal)
				{
					access |= PropertyAccess.Read;
				}

				if (access.HasFlag(PropertyAccess.Read) && !chain[i].CanRead)
				{
					throw new WriteOnlyException(this);
				}

				if (access.HasFlag(PropertyAccess.Write) && !chain[i].CanWrite)
				{
					throw new ReadOnlyException(this);
				}
			}

			Access = desiredAccess;
			m_Chain = chain;
		}

		public Property(string binding)
		{
			Binding = binding;
		}

		public Property(PropertyInfo[] chain)
		{
			m_Chain = chain;
		}

		public override string ToString()
		{
			if (!IsBound)
			{
				return Binding;
			}

			var toJoin = new string[m_Chain.Length];

			for (var i = 0; i < toJoin.Length; ++i)
			{
				toJoin[i] = m_Chain[i].Name;
			}

			return String.Join(".", toJoin);
		}

		public static Property Parse(Type type, string binding, PropertyAccess access)
		{
			var prop = new Property(binding);

			prop.BindTo(type, access);

			return prop;
		}
	}
}

namespace Server.Commands
{
	public enum PropertyAccess
	{
		Read = 0x01,
		Write = 0x02,
		ReadWrite = Read | Write
	}

	public class Props
	{
		private static readonly Type m_TypeOfSerial = typeof(Serial);
		private static readonly Type m_TypeOfType = typeof(Type);
		private static readonly Type m_TypeOfChar = typeof(char);
		private static readonly Type m_TypeOfString = typeof(string);
		private static readonly Type m_TypeOfText = typeof(TextDefinition);
		private static readonly Type m_TypeOfTimeSpan = typeof(TimeSpan);
		private static readonly Type m_TypeOfDateTime = typeof(DateTime);

		private static readonly Type m_TypeOfParsable = typeof(ParsableAttribute);
		private static readonly Type m_TypeOfCPA = typeof(CommandPropertyAttribute);

		private static readonly Type[] m_NumericTypes =
		{
			typeof(sbyte), typeof(byte),
			typeof(short), typeof(ushort),
			typeof(int), typeof(uint),
			typeof(long), typeof(ulong)
		};

		private static readonly Type[] m_ParseTypes = new Type[] { typeof(string) };
		private static readonly object[] m_ParseParams = new object[1];

		public static void Initialize()
		{
			CommandSystem.Register("Props", AccessLevel.Counselor, Props_OnCommand);
			CommandSystem.Register("PropsRegion", AccessLevel.Counselor, PropsRegion_OnCommand);
		}

		private class PropsTarget : Target
		{
			public PropsTarget() : base(-1, true, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (!BaseCommand.IsAccessible(from, o))
				{
					from.SendMessage("That is not accessible.");
				}
				else
				{
					_ = from.SendGump(new PropertiesGump(from, o));
				}
			}
		}

		private class PropsRegionTarget : Target
		{
			public PropsRegionTarget() : base(-1, true, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object o)
			{
				var loc = from.Location;
				var map = from.Map;

				if (o is Item item)
				{
					loc = item.GetWorldLocation();
					map = item.Map;
				}
				else if (o is IEntity e)
				{
					loc = e.Location;
					map = e.Map;
				}
				else if (o is IPoint3D p3d)
				{
					loc = new Point3D(p3d);
				}
				else if (o is IPoint2D p2d)
				{
					loc = new Point3D(p2d, from.Z);
				}

				o = Region.Find(loc, map);

				if (!BaseCommand.IsAccessible(from, o))
				{
					from.SendMessage("That is not accessible.");
				}
				else
				{
					_ = from.SendGump(new PropertiesGump(from, o));
				}
			}
		}

		[Usage("Props [serial]")]
		[Description("Opens a menu where you can view and edit all properties of a targeted (or specified) object.")]
		private static void Props_OnCommand(CommandEventArgs e)
		{
			if (e.Length == 1)
			{
				var ent = e.GetEntity(0);

				if (ent == null)
				{
					e.Mobile.SendMessage("No object with that serial was found.");
				}
				else if (!BaseCommand.IsAccessible(e.Mobile, ent))
				{
					e.Mobile.SendMessage("That is not accessible.");
				}
				else
				{
					_ = e.Mobile.SendGump(new PropertiesGump(e.Mobile, ent));
				}
			}
			else
			{
				e.Mobile.Target = new PropsTarget();
			}
		}

		[Usage("PropsRegion [name]")]
		[Description("Opens a menu where you can view and edit all properties of a targeted (or specified) region.")]
		private static void PropsRegion_OnCommand(CommandEventArgs e)
		{
			if (e.Length == 1)
			{
				var name = e.GetString(0);

				var count = 0;

				foreach (var reg in e.Mobile.Map.Regions)
				{
					if (Insensitive.Equals(reg.Name, name) && BaseCommand.IsAccessible(e.Mobile, reg))
					{
						++count;

						_ = e.Mobile.SendGump(new PropertiesGump(e.Mobile, reg));
					}
				}

				if (count == 0)
				{
					e.Mobile.SendMessage($"No region with that name was found in {e.Mobile.Map.Name}.");
				}
			}
			else
			{
				e.Mobile.Target = new PropsRegionTarget();
			}
		}

		private static bool CIEqual(string l, string r)
		{
			return Insensitive.Equals(l, r);
		}

		public static CommandPropertyAttribute GetCPA(PropertyInfo p)
		{
			var attrs = p.GetCustomAttributes(m_TypeOfCPA, false);

			if (attrs.Length == 0)
			{
				return null;
			}

			return attrs[0] as CommandPropertyAttribute;
		}

		public static PropertyInfo[] GetPropertyInfoChain(Mobile from, Type type, string propertyString, PropertyAccess endAccess, ref string failReason)
		{
			var split = propertyString.Split('.');

			if (split.Length == 0)
			{
				return null;
			}

			var info = new PropertyInfo[split.Length];

			for (var i = 0; i < info.Length; ++i)
			{
				var propertyName = split[i];

				if (CIEqual(propertyName, "current"))
				{
					continue;
				}

				var props = type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);

				var isFinal = i == (info.Length - 1);

				var access = endAccess;

				if (!isFinal)
				{
					access |= PropertyAccess.Read;
				}

				for (var j = 0; j < props.Length; ++j)
				{
					var p = props[j];

					if (CIEqual(p.Name, propertyName))
					{
						var attr = GetCPA(p);

						if (attr == null)
						{
							failReason = $"Property '{propertyName}' not found.";
							return null;
						}

						if (access.HasFlag(PropertyAccess.Read) && from.AccessLevel < attr.ReadLevel)
						{
							failReason = $"You must be at least {Mobile.GetAccessLevelName(attr.ReadLevel)} to get the property '{propertyName}'.";
							return null;
						}

						if (access.HasFlag(PropertyAccess.Write) && from.AccessLevel < attr.WriteLevel)
						{
							failReason = $"You must be at least {Mobile.GetAccessLevelName(attr.WriteLevel)} to set the property '{propertyName}'.";
							return null;
						}

						if (access.HasFlag(PropertyAccess.Read) && !p.CanRead)
						{
							failReason = $"Property '{propertyName}' is write only.";
							return null;
						}

						if (access.HasFlag(PropertyAccess.Write) && (!p.CanWrite || attr.ReadOnly) && isFinal)
						{
							failReason = $"Property '{propertyName}' is read only.";
							return null;
						}

						info[i] = p;
						type = p.PropertyType;
						break;
					}
				}

				if (info[i] == null)
				{
					failReason = $"Property '{propertyName}' not found.";
					return null;
				}
			}

			return info;
		}

		public static PropertyInfo GetPropertyInfo(Mobile from, ref object obj, string propertyName, PropertyAccess access, ref string failReason)
		{
			var chain = GetPropertyInfoChain(from, obj.GetType(), propertyName, access, ref failReason);

			if (chain != null)
			{
				return GetPropertyInfo(ref obj, chain, ref failReason);
			}

			return null;
		}

		public static PropertyInfo GetPropertyInfo(ref object obj, PropertyInfo[] chain, ref string failReason)
		{
			if (chain == null || chain.Length == 0)
			{
				failReason = "Property chain is empty.";
				return null;
			}

			for (var i = 0; i < chain.Length - 1; ++i)
			{
				if (chain[i] == null)
				{
					continue;
				}

				obj = chain[i].GetValue(obj, null);

				if (obj == null)
				{
					failReason = $"Property '{chain[i]}' is null.";
					return null;
				}
			}

			return chain[chain.Length - 1];
		}

		public static string GetValue(Mobile from, object o, string name)
		{
			var failReason = "";

			var chain = GetPropertyInfoChain(from, o.GetType(), name, PropertyAccess.Read, ref failReason);

			if (chain == null || chain.Length == 0)
			{
				return failReason;
			}

			var p = GetPropertyInfo(ref o, chain, ref failReason);

			if (p == null)
			{
				return failReason;
			}

			return InternalGetValue(o, p, chain);
		}

		public static string IncreaseValue(Mobile from, object o, string[] args)
		{
			var realObjs = new object[args.Length / 2];
			var realProps = new PropertyInfo[args.Length / 2];
			var realValues = new int[args.Length / 2];

			bool positive = false, negative = false;

			for (var i = 0; i < realProps.Length; ++i)
			{
				var name = args[i * 2];

				try
				{
					var valueString = args[1 + (i * 2)];

					if (valueString.StartsWith("0x"))
					{
						realValues[i] = Convert.ToInt32(valueString.Substring(2), 16);
					}
					else
					{
						realValues[i] = Convert.ToInt32(valueString);
					}
				}
				catch
				{
					return "Offset value could not be parsed.";
				}

				if (realValues[i] > 0)
				{
					positive = true;
				}
				else if (realValues[i] < 0)
				{
					negative = true;
				}
				else
				{
					return "Zero is not a valid value to offset.";
				}

				string failReason = null;
				realObjs[i] = o;
				realProps[i] = GetPropertyInfo(from, ref realObjs[i], name, PropertyAccess.ReadWrite, ref failReason);

				if (failReason != null)
				{
					return failReason;
				}

				if (realProps[i] == null)
				{
					return "Property not found.";
				}
			}

			for (var i = 0; i < realProps.Length; ++i)
			{
				var obj = realProps[i].GetValue(realObjs[i], null);

				if (obj is not IConvertible)
				{
					return "Property is not IConvertable.";
				}

				try
				{

					var v = (long)Convert.ChangeType(obj, TypeCode.Int64) + realValues[i];

					realProps[i].SetValue(realObjs[i], Convert.ChangeType(v, realProps[i].PropertyType), null);
				}
				catch
				{
					return "Value could not be converted";
				}
			}

			if (realProps.Length == 1)
			{
				if (positive)
				{
					return "The property has been increased.";
				}

				return "The property has been decreased.";
			}

			if (positive && negative)
			{
				return "The properties have been changed.";
			}

			if (positive)
			{
				return "The properties have been increased.";
			}

			return "The properties have been decreased.";
		}

		private static string InternalGetValue(object o, PropertyInfo p, PropertyInfo[] chain)
		{
			var type = p.PropertyType;
			var value = p.GetValue(o, null);

			string toString;

			if (value == null)
			{
				toString = "null";
			}
			else if (IsNumeric(type))
			{
				toString = $"{value} (0x{value:X})";
			}
			else if (IsChar(type))
			{
				toString = $"'{value}' ({(int)value} [0x{(int)value:X}])";
			}
			else if (IsString(type))
			{
				toString = (string)value == "null" ? @"@""null""" : $"\"{value}\"";
			}
			else if (IsText(type))
			{
				toString = ((TextDefinition)value).Format(false);
			}
			else
			{
				toString = value.ToString();
			}

			if (chain == null)
			{
				return $"{p.Name} = {toString}";
			}

			var concat = new string[(chain.Length * 2) + 1];

			for (var i = 0; i < chain.Length; ++i)
			{
				concat[(i * 2) + 0] = chain[i].Name;
				concat[(i * 2) + 1] = (i < (chain.Length - 1)) ? "." : " = ";
			}

			concat[concat.Length - 1] = toString;

			return String.Concat(concat);
		}

		public static string SetValue(Mobile from, object o, string name, string value)
		{
			var logObject = o;
			var failReason = "";

			var p = GetPropertyInfo(from, ref o, name, PropertyAccess.Write, ref failReason);

			if (p == null)
			{
				return failReason;
			}

			return InternalSetValue(from, logObject, o, p, name, value, true);
		}

		private static bool IsSerial(Type t)
		{
			return t == m_TypeOfSerial;
		}


		private static bool IsType(Type t)
		{
			return t == m_TypeOfType;
		}


		private static bool IsChar(Type t)
		{
			return t == m_TypeOfChar;
		}


		private static bool IsString(Type t)
		{
			return t == m_TypeOfString;
		}


		private static bool IsText(Type t)
		{
			return t == m_TypeOfText;
		}

		private static bool IsEnum(Type t)
		{
			return t.IsEnum;
		}

		private static bool IsParsable(Type t)
		{
			return t == m_TypeOfTimeSpan || t == m_TypeOfDateTime || t.IsDefined(m_TypeOfParsable, false);
		}

		private static object Parse(object o, Type t, string value)
		{
			var method = t.GetMethod("Parse", m_ParseTypes);

			m_ParseParams[0] = value;

			return method.Invoke(o, m_ParseParams);
		}

		private static bool IsNumeric(Type t)
		{
			return Array.IndexOf(m_NumericTypes, t) >= 0;
		}

		public static string ConstructFromString(Type type, object obj, string value, ref object constructed)
		{
			object toSet;
			var isSerial = IsSerial(type);

			if (isSerial) // mutate into int32
			{
				type = m_NumericTypes[4];
			}

			if (value == "(-null-)" && !type.IsValueType)
			{
				value = null;
			}

			if (IsEnum(type))
			{
				try
				{
					toSet = Enum.Parse(type, value, true);
				}
				catch
				{
					return "That is not a valid enumeration member.";
				}
			}
			else if (IsType(type))
			{
				try
				{
					toSet = ScriptCompiler.FindTypeByName(value);

					if (toSet == null)
					{
						return "No type with that name was found.";
					}
				}
				catch
				{
					return "No type with that name was found.";
				}
			}
			else if (IsParsable(type))
			{
				try
				{
					toSet = Parse(obj, type, value);
				}
				catch
				{
					return "That is not properly formatted.";
				}
			}
			else if (value == null)
			{
				toSet = null;
			}
			else if (value.StartsWith("0x") && IsNumeric(type))
			{
				try
				{
					toSet = Convert.ChangeType(Convert.ToUInt64(value.Substring(2), 16), type);
				}
				catch
				{
					return "That is not properly formatted.";
				}
			}
			else
			{
				try
				{
					toSet = Convert.ChangeType(value, type);
				}
				catch
				{
					return "That is not properly formatted.";
				}
			}

			if (isSerial) // mutate back
			{
				toSet = new Serial((int)toSet);
			}

			constructed = toSet;
			return null;
		}

		public static string SetDirect(Mobile from, object logObject, object obj, PropertyInfo prop, string givenName, object toSet, bool shouldLog)
		{
			try
			{
				if (toSet is AccessLevel newLevel)
				{
					var reqLevel = AccessLevel.Administrator;

					if (newLevel == AccessLevel.Administrator)
					{
						reqLevel = AccessLevel.Developer;
					}
					else if (newLevel >= AccessLevel.Developer)
					{
						reqLevel = AccessLevel.Owner;
					}

					if (from.AccessLevel < reqLevel)
					{
						return "You do not have access to that level.";
					}
				}

				if (shouldLog)
				{
					CommandLogging.LogChangeProperty(from, logObject, givenName, toSet == null ? "(-null-)" : toSet.ToString());
				}

				prop.SetValue(obj, toSet, null);
				return "Property has been set.";
			}
			catch
			{
				return "An exception was caught, the property may not be set.";
			}
		}

		public static string SetDirect(object obj, PropertyInfo prop, object toSet)
		{
			try
			{
				if (toSet is AccessLevel)
				{
					return "You do not have access to that level.";
				}

				prop.SetValue(obj, toSet, null);
				return "Property has been set.";
			}
			catch
			{
				return "An exception was caught, the property may not be set.";
			}
		}

		public static string InternalSetValue(Mobile from, object logobj, object o, PropertyInfo p, string pname, string value, bool shouldLog)
		{
			object toSet = null;
			var result = ConstructFromString(p.PropertyType, o, value, ref toSet);

			if (result != null)
			{
				return result;
			}

			return SetDirect(from, logobj, o, p, pname, toSet, shouldLog);
		}

		public static string InternalSetValue(object o, PropertyInfo p, string value)
		{
			object toSet = null;
			var result = ConstructFromString(p.PropertyType, o, value, ref toSet);

			if (result != null)
			{
				return result;
			}

			return SetDirect(o, p, toSet);
		}
	}
}

namespace Server.Gumps
{
	public class PropsConfig
	{
		public static readonly bool OldStyle = false;

		public static readonly int GumpOffsetX = 30;
		public static readonly int GumpOffsetY = 30;

		public static readonly int TextHue = 0;
		public static readonly int TextOffsetX = 2;

		public static readonly int OffsetGumpID = 0x0A40; // Pure black
		public static readonly int HeaderGumpID = OldStyle ? 0x0BBC : 0x0E14; // Light offwhite, textured : Dark navy blue, textured
		public static readonly int EntryGumpID = 0x0BBC; // Light offwhite, textured
		public static readonly int BackGumpID = 0x13BE; // Gray slate/stoney
		public static readonly int SetGumpID = OldStyle ? 0x0000 : 0x0E14; // Empty : Dark navy blue, textured

		public static readonly int SetWidth = 20;
		public static readonly int SetOffsetX = OldStyle ? 4 : 2, SetOffsetY = 2;
		public static readonly int SetButtonID1 = 0x15E1; // Arrow pointing right
		public static readonly int SetButtonID2 = 0x15E5; // " pressed

		public static readonly int PrevWidth = 20;
		public static readonly int PrevOffsetX = 2, PrevOffsetY = 2;
		public static readonly int PrevButtonID1 = 0x15E3; // Arrow pointing left
		public static readonly int PrevButtonID2 = 0x15E7; // " pressed

		public static readonly int NextWidth = 20;
		public static readonly int NextOffsetX = 2, NextOffsetY = 2;
		public static readonly int NextButtonID1 = 0x15E1; // Arrow pointing right
		public static readonly int NextButtonID2 = 0x15E5; // " pressed

		public static readonly int OffsetSize = 1;

		public static readonly int EntryHeight = 20;
		public static readonly int BorderSize = 10;
	}

	public class StackEntry
	{
		public object m_Object;
		public PropertyInfo m_Property;

		public StackEntry(object obj, PropertyInfo prop)
		{
			m_Object = obj;
			m_Property = prop;
		}
	}

	public class PropertiesGump : Gump
	{
		private static HashSet<PropertiesGump> m_Buffer = new();

		public static Dictionary<object, HashSet<PropertiesGump>> Instances { get; } = new();

		public static void Configure()
		{
			PropertyNotifier.PropertyChanged += OnPropertyChanged;
		}

		private static void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (sender != null && Instances.TryGetValue(sender, out var gumps))
			{
				Instances[sender] = m_Buffer;

				m_Buffer = gumps;

				foreach (var gump in m_Buffer)
				{
					var ns = gump.m_Mobile?.NetState;

					if (ns == null)
					{
						continue;
					}

					ns.Send(new CloseGump(gump.TypeID, 0));

					ns.RemoveGump(gump);

					gump.OnServerClose(ns);

					var g = new PropertiesGump(gump.m_Mobile, gump.m_Object, gump.m_Stack, gump.m_List, gump.m_Page);

					g.SendTo(ns);
				}

				m_Buffer.Clear();
			}
		}

		private static readonly Type m_TypeOfMobile = typeof(Mobile);
		private static readonly Type m_TypeOfItem = typeof(Item);
		private static readonly Type m_TypeOfIEntity = typeof(IEntity);
		private static readonly Type m_TypeOfType = typeof(Type);
		private static readonly Type m_TypeOfPoint3D = typeof(Point3D);
		private static readonly Type m_TypeOfPoint2D = typeof(Point2D);
		private static readonly Type m_TypeOfTimeSpan = typeof(TimeSpan);
		private static readonly Type m_TypeOfDateTime = typeof(DateTime);
		private static readonly Type m_TypeOfCustomEnum = typeof(CustomEnumAttribute);
		private static readonly Type m_TypeOfEnum = typeof(Enum);
		private static readonly Type m_TypeOfBool = typeof(bool);
		private static readonly Type m_TypeOfString = typeof(string);
		private static readonly Type m_TypeOfText = typeof(TextDefinition);
		private static readonly Type m_TypeOfPoison = typeof(Poison);
		private static readonly Type m_TypeOfMap = typeof(Map);
		private static readonly Type m_TypeOfSkills = typeof(Skills);
		private static readonly Type m_TypeOfColor = typeof(Color);
		private static readonly Type m_TypeOfICollection = typeof(ICollection);
		private static readonly Type m_TypeOfICollectionT = typeof(ICollection<>);

		private static readonly Type m_TypeOfNoSort = typeof(NoSortAttribute);
		private static readonly Type m_TypeOfPropertyObject = typeof(PropertyObjectAttribute);
		private static readonly Type m_TypeOfObject = typeof(object);

		private static readonly Type[] m_TypeOfReal = new Type[]
		{
			typeof(float), typeof(double), typeof(decimal),
		};

		private static readonly Type[] m_TypeOfNumeric =
		{
			typeof(byte), typeof(short), typeof(int), typeof(long), typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong)
		};

		public static readonly string[] m_BoolNames = new string[] { "True", "False" };
		public static readonly object[] m_BoolValues = new object[] { true, false };

		public static readonly string[] m_PoisonNames = new string[] { "None", "Lesser", "Regular", "Greater", "Deadly", "Lethal" };
		public static readonly object[] m_PoisonValues = new object[] { null, Poison.Lesser, Poison.Regular, Poison.Greater, Poison.Deadly, Poison.Lethal };

		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly bool PrevLabel = OldStyle, NextLabel = OldStyle;
		private static readonly bool TypeLabel = !OldStyle;

		private static readonly int PrevLabelOffsetX = PrevWidth + 1;
		private static readonly int PrevLabelOffsetY = 0;

		private static readonly int NextLabelOffsetX = -29;
		private static readonly int NextLabelOffsetY = 0;

		private static readonly int NameWidth = 150;
		private static readonly int ValueWidth = 150;

		private static readonly int EntryCount = 20;

		private static readonly int TypeWidth = NameWidth + OffsetSize + ValueWidth;

		private static readonly int TotalWidth = OffsetSize + NameWidth + OffsetSize + ValueWidth + OffsetSize + SetWidth + OffsetSize;
		//private static readonly int TotalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (EntryCount + 1));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		//private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		private int m_Page;

		private readonly ArrayList m_List;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Type m_Type;
		private readonly Stack<StackEntry> m_Stack;

		public PropertiesGump(Mobile mobile, object o) : base(GumpOffsetX, GumpOffsetY)
		{
			m_Mobile = mobile;
			m_Object = o;

			m_Type = m_Object?.GetType();

			m_List = BuildList();

			Initialize(0);
		}

		public PropertiesGump(Mobile mobile, object o, Stack<StackEntry> stack, StackEntry parent) : base(GumpOffsetX, GumpOffsetY)
		{
			m_Mobile = mobile;
			m_Object = o;

			m_Type = m_Object?.GetType();

			m_Stack = stack;
			m_List = BuildList();

			if (parent != null)
			{
				m_Stack ??= new Stack<StackEntry>();

				m_Stack.Push(parent);
			}

			Initialize(0);
		}

		public PropertiesGump(Mobile mobile, object o, Stack<StackEntry> stack, ArrayList list, int page) : base(GumpOffsetX, GumpOffsetY)
		{
			m_Mobile = mobile;
			m_Object = o;

			m_Type = m_Object?.GetType();

			m_List = list;
			m_Stack = stack;

			Initialize(page);
		}

		private void Initialize(int page)
		{
			if (!m_Type.IsPrimitive)
			{
				if (!Instances.TryGetValue(m_Object, out var gumps))
				{
					Instances[m_Object] = gumps = new HashSet<PropertiesGump>();
				}
				else
				{
					_ = gumps.RemoveWhere(g => g.m_Mobile == m_Mobile);
				}

				_ = gumps.Add(this);
			}

			foreach (var o in m_List)
			{
				if (o == null)
				{
					continue;
				}

				var t = o.GetType();

				if (!t.IsPrimitive)
				{
					if (!Instances.TryGetValue(o, out var gumps))
					{
						Instances[o] = gumps = new HashSet<PropertiesGump>();
					}

					_ = gumps.Add(this);
				}
			}

			m_Page = page;

			var count = m_List.Count - (page * EntryCount);

			if (count < 0)
			{
				count = 0;
			}
			else if (count > EntryCount)
			{
				count = EntryCount;
			}

			var lastIndex = (page * EntryCount) + count - 1;

			if (lastIndex >= 0 && lastIndex < m_List.Count && m_List[lastIndex] == null)
			{
				--count;
			}

			var totalHeight = OffsetSize + ((EntryHeight + OffsetSize) * (count + 1));

			AddPage(0);

			AddBackground(0, 0, BackWidth, BorderSize + totalHeight + BorderSize, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);

			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize;

			var emptyWidth = TotalWidth - PrevWidth - NextWidth - (OffsetSize * 4) - (OldStyle ? SetWidth + OffsetSize : 0);

			if (OldStyle)
			{
				AddImageTiled(x, y, TotalWidth - (OffsetSize * 3) - SetWidth, EntryHeight, HeaderGumpID);
			}
			else
			{
				AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);
			}

			if (page > 0)
			{
				AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 1, GumpButtonType.Reply, 0);

				if (PrevLabel)
				{
					AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
				}
			}

			x += PrevWidth + OffsetSize;

			if (!OldStyle)
			{
				AddImageTiled(x, y, emptyWidth, EntryHeight, HeaderGumpID);
			}

			if (TypeLabel && m_Type != null)
			{
				AddHtml(x, y, emptyWidth, EntryHeight, $"<BASEFONT COLOR=#FAFAFA><CENTER>{m_Type.Name}</CENTER>", false, false);
			}

			x += emptyWidth + OffsetSize;

			if (!OldStyle)
			{
				AddImageTiled(x, y, NextWidth, EntryHeight, HeaderGumpID);
			}

			if ((page + 1) * EntryCount < m_List.Count)
			{
				AddButton(x + NextOffsetX, y + NextOffsetY, NextButtonID1, NextButtonID2, 2, GumpButtonType.Reply, 1);

				if (NextLabel)
				{
					AddLabel(x + NextLabelOffsetX, y + NextLabelOffsetY, TextHue, "Next");
				}
			}

			for (int i = 0, index = page * EntryCount; i < count && index < m_List.Count; ++i, ++index)
			{
				x = BorderSize + OffsetSize;
				y += EntryHeight + OffsetSize;

				var o = m_List[index];

				if (o == null)
				{
					AddImageTiled(x - OffsetSize, y, TotalWidth, EntryHeight, BackGumpID + 4);
				}
				else if (o == m_Object)
				{
					AddImageTiled(x, y, NameWidth, EntryHeight, EntryGumpID);
					AddLabelCropped(x + TextOffsetX, y, NameWidth - TextOffsetX, EntryHeight, TextHue, "Entries");
					x += NameWidth + OffsetSize;

					var subcount = 0;

					if (o is ICollection col)
					{
						subcount = col.Count;
					}
					else if (o is IEnumerable eable)
					{
						foreach (var obj in eable)
						{
							++subcount;
						}
					}

					AddImageTiled(x, y, ValueWidth, EntryHeight, EntryGumpID);
					AddLabelCropped(x + TextOffsetX, y, ValueWidth - TextOffsetX, EntryHeight, TextHue, $"[{subcount:N0}]...");
					x += ValueWidth + OffsetSize;

					if (SetGumpID != 0)
					{
						AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
					}

					AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, i + 3, GumpButtonType.Reply, 0);
				}
				else if (o is Type type)
				{
					AddImageTiled(x, y, TypeWidth, EntryHeight, EntryGumpID);
					AddLabelCropped(x + TextOffsetX, y, TypeWidth - TextOffsetX, EntryHeight, TextHue, type.Name);
					x += TypeWidth + OffsetSize;

					if (SetGumpID != 0)
					{
						AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
					}
				}
				else if (o is PropertyInfo prop)
				{
					AddImageTiled(x, y, NameWidth, EntryHeight, EntryGumpID);
					AddLabelCropped(x + TextOffsetX, y, NameWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
					x += NameWidth + OffsetSize;

					var value = ValueToString(prop);

					if (value.Length * 6 >= ValueWidth - TextOffsetX)
					{
						value = $"{value[..((ValueWidth - TextOffsetX) / 6)]}";
					}

					AddImageTiled(x, y, ValueWidth, EntryHeight, EntryGumpID);
					AddLabelCropped(x + TextOffsetX, y, ValueWidth - TextOffsetX, EntryHeight, TextHue, value);
					x += ValueWidth + OffsetSize;

					if (SetGumpID != 0)
					{
						AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
					}

					var cpa = GetCPA(prop);

					if ((prop.CanWrite || prop.SetMethod != null) && cpa != null && m_Mobile.AccessLevel >= cpa.WriteLevel && !cpa.ReadOnly)
					{
						AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, i + 3, GumpButtonType.Reply, 0);
					}
				}
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (!m_Type.IsPrimitive && Instances.TryGetValue(m_Object, out var gumps))
			{
				_ = gumps.Remove(this);

				if (gumps.Count == 0)
				{
					_ = Instances.Remove(m_Object);
				}
			}

			var from = state.Mobile;

			if (!BaseCommand.IsAccessible(from, m_Object))
			{
				from.SendMessage("You may no longer access their properties.");
				return;
			}

			switch (info.ButtonID)
			{
				case 0: // Closed
					{
						if (m_Stack != null && m_Stack.Count > 0)
						{
							var entry = m_Stack.Pop();

							_ = from.SendGump(new PropertiesGump(from, entry.m_Object, m_Stack, null));
						}

						break;
					}
				case 1: // Previous
					{
						if (m_Page > 0)
						{
							_ = from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page - 1));
						}

						break;
					}
				case 2: // Next
					{
						if ((m_Page + 1) * EntryCount < m_List.Count)
						{
							_ = from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page + 1));
						}

						break;
					}
				default:
					{
						var index = (m_Page * EntryCount) + (info.ButtonID - 3);

						if (index >= 0 && index < m_List.Count)
						{
							var entry = m_List[index];

							if (entry == m_Object)
							{
								if (entry is ICollection col && col.Count > 0)
								{
									_ = from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page));
									_ = from.SendGump(new InterfaceGump(from, col));
								}
								else if (entry is IEnumerable eable)
								{
									var arr = new ArrayList();

									foreach (var o in eable)
									{
										arr.Add(o);
									}

									if (arr.Count > 0)
									{
										_ = from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page));
										_ = from.SendGump(new InterfaceGump(from, arr));
									}
								}
							}

							if (entry is not PropertyInfo prop)
							{
								return;
							}

							var attr = GetCPA(prop);

							if (!prop.CanWrite || attr == null || from.AccessLevel < attr.WriteLevel || attr.ReadOnly)
							{
								return;
							}

							var type = prop.PropertyType;

							if (IsType(type, m_TypeOfMobile) || IsType(type, m_TypeOfItem) || IsType(type, m_TypeOfIEntity))
							{
								_ = from.SendGump(new SetObjectGump(prop, from, m_Object, m_Stack, type, m_Page, m_List));
							}
							else if (IsType(type, m_TypeOfType))
							{
								from.SendMessage($"Target an object to use its type reference for {prop.Name}...");
								from.Target = new SetObjectTarget(prop, from, m_Object, m_Stack, type, m_Page, m_List);
							}
							else if (IsType(type, m_TypeOfPoint3D))
							{
								_ = from.SendGump(new SetPoint3DGump(prop, from, m_Object, m_Stack, m_Page, m_List));
							}
							else if (IsType(type, m_TypeOfPoint2D))
							{
								_ = from.SendGump(new SetPoint2DGump(prop, from, m_Object, m_Stack, m_Page, m_List));
							}
							else if (IsType(type, m_TypeOfTimeSpan))
							{
								_ = from.SendGump(new SetTimeSpanGump(prop, from, m_Object, m_Stack, m_Page, m_List));
							}
							else if (IsType(type, m_TypeOfDateTime))
							{
								_ = from.SendGump(new SetDateTimeGump(prop, from, m_Object, m_Stack, m_Page, m_List));
							}
							else if (IsCustomEnum(type))
							{
								_ = from.SendGump(new SetCustomEnumGump(prop, from, m_Object, m_Stack, m_Page, m_List, GetCustomEnumNames(type)));
							}
							else if (IsType(type, m_TypeOfEnum))
							{
								_ = from.SendGump(new SetListOptionGump(prop, from, m_Object, m_Stack, m_Page, m_List, Enum.GetNames(type), GetObjects(Enum.GetValues(type))));
							}
							else if (IsType(type, m_TypeOfBool))
							{
								_ = from.SendGump(new SetListOptionGump(prop, from, m_Object, m_Stack, m_Page, m_List, m_BoolNames, m_BoolValues));
							}
							else if (IsType(type, m_TypeOfString) || IsType(type, m_TypeOfReal) || IsType(type, m_TypeOfNumeric) || IsType(type, m_TypeOfText))
							{
								_ = from.SendGump(new SetGump(prop, from, m_Object, m_Stack, m_Page, m_List));
							}
							else if (IsType(type, m_TypeOfPoison))
							{
								_ = from.SendGump(new SetListOptionGump(prop, from, m_Object, m_Stack, m_Page, m_List, m_PoisonNames, m_PoisonValues));
							}
							else if (IsType(type, m_TypeOfMap))
							{
								_ = from.SendGump(new SetListOptionGump(prop, from, m_Object, m_Stack, m_Page, m_List, Map.GetMapNames(), Map.GetMapValues()));
							}
							else if (IsType(type, m_TypeOfSkills) && m_Object is Mobile mob)
							{
								_ = from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page));
								_ = from.SendGump(new SkillsGump(from, mob));
							}
							else if (IsType(type, m_TypeOfColor))
							{
								_ = from.SendGump(new SetColorGump(prop, from, m_Object, m_Stack, m_Page, m_List));
							}
							else if (HasAttribute(type, m_TypeOfPropertyObject, true))
							{
								var obj = prop.GetValue(m_Object, null);

								if (obj != null)
								{
									_ = from.SendGump(new PropertiesGump(from, obj, m_Stack, new StackEntry(m_Object, prop)));
								}
								else
								{
									_ = from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page));
								}
								/*
								if (IsType(type, m_TypeOfICollection) || IsType(type, m_TypeOfICollectionT))
								{
									if (obj is ICollection col && col.Count > 0)
									{
										_ = from.SendGump(new InterfaceGump(from, col));
									}
									else if (obj is IEnumerable eable)
									{
										var arr = new ArrayList();

										foreach (var o in eable)
										{
											arr.Add(o);
										}

										if (arr.Count > 0)
										{
											_ = from.SendGump(new InterfaceGump(from, arr));
										}
									}
								}
								*/
							}
							else if (IsType(type, m_TypeOfICollection) || IsType(type, m_TypeOfICollectionT))
							{
								_ = from.SendGump(new PropertiesGump(from, m_Object, m_Stack, m_List, m_Page));
								/*
								var subval = prop.GetValue(m_Object, null);

								if (subval is ICollection col && col.Count > 0)
								{
									_ = from.SendGump(new InterfaceGump(from, col));
								}
								else if (subval is IEnumerable eable)
								{
									var arr = new ArrayList();

									foreach (var o in eable)
									{
										arr.Add(o);
									}

									if (arr.Count > 0)
									{
										_ = from.SendGump(new InterfaceGump(from, arr));
									}
								}
								*/
							}
						}

						break;
					}
			}
		}

		private static object[] GetObjects(Array a)
		{
			var list = new object[a.Length];

			for (var i = 0; i < list.Length; ++i)
			{
				list[i] = a.GetValue(i);
			}

			return list;
		}

		private static bool IsCustomEnum(Type type)
		{
			return type.IsDefined(m_TypeOfCustomEnum, false);
		}

		private static string[] GetCustomEnumNames(Type type)
		{
			var attrs = type.GetCustomAttributes(m_TypeOfCustomEnum, false);

			if (attrs.Length == 0)
			{
				return Array.Empty<string>();
			}

			if (attrs[0] is not CustomEnumAttribute ce)
			{
				return Array.Empty<string>();
			}

			return ce.Names;
		}

		private static bool HasAttribute(Type type, Type check, bool inherit)
		{
			var objs = type.GetCustomAttributes(check, inherit);

			return objs?.Length > 0;
		}

		private static bool IsType(Type type, Type check)
		{
			if (type == check)
			{
				return true;
			}

			if (check.IsInterface)
			{
				return type.GetInterface(check.Name) != null;
			}

			return type.IsSubclassOf(check);
		}

		private static bool IsType(Type type, Type[] check)
		{
			for (var i = 0; i < check.Length; ++i)
			{
				if (IsType(type, check[i]))
				{
					return true;
				}
			}

			return false;
		}

		private string ValueToString(PropertyInfo prop)
		{
			return ValueToString(m_Object, prop);
		}

		public static string ValueToString(object obj, PropertyInfo prop)
		{
			try
			{
				return ValueToString(prop.GetValue(obj, null));
			}
			catch (Exception e)
			{
				return $"!{e.GetType()}!";
			}
		}

		public static string ValueToString(object o)
		{
			if (o == null)
			{
				return "-null-";
			}

			if (o is string str)
			{
				return $"\"{str}\"";
			}

			if (o is bool)
			{
				return o.ToString();
			}

			if (o is char c)
			{
				return $"0x{(short)c:X} '{c}'";
			}

			if (o is Serial s)
			{
				if (s.IsValid)
				{
					if (s.IsItem)
					{
						return $"(I) 0x{s.Value:X}";
					}

					if (s.IsMobile)
					{
						return $"(M) 0x{s.Value:X}";
					}
				}

				return $"(?) 0x{s.Value:X}";
			}

			if (o is sbyte or byte or short or ushort or int or uint or long or ulong)
			{
				return $"{o} (0x{o:X})";
			}

			if (o is Mobile mobile)
			{
				return $"(M) 0x{mobile.Serial.Value:X} \"{mobile.Name}\"";
			}

			if (o is Item item)
			{
				return $"(I) 0x{item.Serial.Value:X}";
			}

			if (o is IEntity entity)
			{
				return $"(?) 0x{entity.Serial.Value:X}";
			}

			if (o is Type type)
			{
				return GetRealTypeName(type);
			}

			if (o is TextDefinition def)
			{
				return def.Format(true);
			}

			if (o is Color color)
			{
				if (color.IsEmpty)
				{
					return "---";
				}

				if (color.IsNamedColor)
				{
					return color.Name;
				}

				return $"{color.ToArgb() & 0x00FFFFFF:X6}";
			}

			if (o is Array arr)
			{
				return $"{GetRealTypeName(arr)}";
			}

			if (o is ICollection col)
			{
				return $"{GetRealTypeName(col)}[{col.Count}]";
			}

			return o.ToString();
		}

		public static string GetRealTypeName(object obj)
		{
			if (obj is Array arr)
			{
				return GetRealTypeName(arr);
			}

			return GetRealTypeName(obj?.GetType());
		}

		public static string GetRealTypeName(Array arr)
		{
			if (arr == null)
			{
				return "-null-";
			}

			var name = GetRealTypeName(arr.GetType());

			var index = name.IndexOf('[');

			if (index >= 0)
			{
				var ranks = String.Join(", ", Enumerable.Range(0, arr.Rank).Select(i => arr.GetLength(i)));

				name = name.Insert(++index, ranks);

				index += ranks.Length;

				var close = name.IndexOf(']', index);

				if (close > index)
				{
					name = name.Remove(index, close - index);
				}

				name = name.Remove(close + 1);
			}

			return name;
		}

		public static string GetRealTypeName(Type type)
		{
			if (type == null)
			{
				return "-null-";
			}

			var name = type.Name;

			if (!type.IsGenericType)
			{
				return name;
			}

			name = name.Substring(0, name.IndexOf('`'));

			return $"{name}<{String.Join(", ", type.GenericTypeArguments.Select(GetRealTypeName))}>";
		}

		public static void OnValueChanged(Stack<StackEntry> stack, object obj, PropertyInfo prop, object oldValue, object newValue)
		{
			if (stack != null && stack.Count != 0 && prop.PropertyType.IsValueType)
			{
				var o = obj;

				foreach (var e in stack)
				{
					if (!e.m_Property.PropertyType.IsValueType)
					{
						break;
					}

					if (e.m_Property.CanWrite)
					{
						e.m_Property.SetValue(e.m_Object, o, null);
					}

					o = e.m_Object;
				}
			}

			PropertyNotifier.Notify(obj, prop, oldValue, newValue);
		}

		private ArrayList BuildList()
		{
			var list = new ArrayList();

			if (m_Type == null)
			{
				return list;
			}

			var props = m_Type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);

			var groups = GetGroups(m_Type, props);

			for (var i = 0; i < groups.Count; ++i)
			{
				var de = (DictionaryEntry)groups[i];
				var groupList = (ArrayList)de.Value;

				if (!HasAttribute((Type)de.Key, m_TypeOfNoSort, false))
				{
					groupList.Sort(PropertySorter.Instance);
				}

				if (i != 0)
				{
					_ = list.Add(null);
				}

				_ = list.Add(de.Key);
				list.AddRange(groupList);
			}

			if (IsType(m_Type, m_TypeOfICollection) || IsType(m_Type, m_TypeOfICollectionT))
			{
				list.Add(m_Object);
			}

			return list;
		}

		private static CommandPropertyAttribute GetCPA(PropertyInfo prop)
		{
			foreach (var attr in prop.GetCustomAttributes<CommandPropertyAttribute>(false))
			{
				return attr;
			}

			return null;
		}

		private ArrayList GetGroups(Type objectType, IEnumerable<PropertyInfo> props)
		{
			var groups = new Hashtable();

			foreach (var prop in props)
			{
				if (prop.CanRead)
				{
					var attr = GetCPA(prop);

					if (attr != null && m_Mobile.AccessLevel >= attr.ReadLevel)
					{
						var type = prop.DeclaringType;

						while (true)
						{
							var baseType = type.BaseType;

							if (baseType == null || baseType == m_TypeOfObject)
							{
								break;
							}

							if (baseType.GetProperty(prop.Name, prop.PropertyType) != null)
							{
								type = baseType;
							}
							else
							{
								break;
							}
						}

						var list = (ArrayList)groups[type];

						if (list == null)
						{
							groups[type] = list = new ArrayList();
						}

						_ = list.Add(prop);
					}
				}
			}

			var sorted = new ArrayList(groups);

			sorted.Sort(new GroupComparer(objectType));

			return sorted;
		}

		public static object GetObjectFromString(Type t, string s)
		{
			if (t == typeof(string))
			{
				return s;
			}

			if (t == typeof(sbyte) || t == typeof(byte) || t == typeof(short) || t == typeof(ushort) || t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong))
			{
				if (s.StartsWith("0x"))
				{
					if (t == typeof(ulong) || t == typeof(uint) || t == typeof(ushort) || t == typeof(byte))
					{
						return Convert.ChangeType(Convert.ToUInt64(s.Substring(2), 16), t);
					}

					return Convert.ChangeType(Convert.ToInt64(s.Substring(2), 16), t);
				}

				return Convert.ChangeType(s, t);
			}

			if (t == typeof(double) || t == typeof(float) || t == typeof(decimal))
			{
				return Convert.ChangeType(s, t);
			}

			if (t == typeof(IAccount) || t == typeof(Account))
			{
				return Accounts.GetAccount(s);
			}

			if (t == typeof(Color))
			{
				if (String.IsNullOrWhiteSpace(s) || s == "---")
				{
					return Color.Empty;
				}

				if (Insensitive.Equals(s, "None") || Insensitive.Equals(s, "Empty"))
				{
					return Color.Empty;
				}

				if (Insensitive.StartsWith(s, "0x"))
				{
					return Color.FromArgb(Convert.ToInt32(s.Substring(2), 16));
				}

				if (Insensitive.StartsWith(s, "#"))
				{
					return Color.FromArgb(Convert.ToInt32(s.Substring(1), 16));
				}

				if (Int32.TryParse(s, out var val))
				{
					return Color.FromArgb(val);
				}

				var rgb = s.Split(',');

				if (rgb.Length >= 3)
				{
					if (Byte.TryParse(rgb[0], out var r) && Byte.TryParse(rgb[1], out var g) && Byte.TryParse(rgb[2], out var b))
					{
						return Color.FromArgb(r, g, b);
					}
				}

				return Color.FromName(s);
			}

			if (t.IsDefined(typeof(ParsableAttribute), false))
			{
				var parseMethod = t.GetMethod("Parse", new[] { typeof(string) });

				return parseMethod.Invoke(null, new object[] { s });
			}

			throw new FormatException();
		}

		private class PropertySorter : IComparer
		{
			public static readonly PropertySorter Instance = new();

			private PropertySorter()
			{
			}

			public int Compare(object x, object y)
			{
				if (x == null && y == null)
				{
					return 0;
				}

				if (x == null)
				{
					return -1;
				}

				if (y == null)
				{
					return 1;
				}

				if (x is not PropertyInfo)
				{
					throw new ArgumentException($"Comparison was not a {typeof(PropertyInfo)}", nameof(x));
				}

				if (y is not PropertyInfo)
				{
					throw new ArgumentException($"Comparison was not a {typeof(PropertyInfo)}", nameof(y));
				}

				var a = (PropertyInfo)x;
				var b = (PropertyInfo)y;

				return a.Name.CompareTo(b.Name);
			}
		}

		private class GroupComparer : IComparer
		{
			private readonly Type m_Start;

			public GroupComparer(Type start)
			{
				m_Start = start;
			}

			private int GetDistance(Type type)
			{
				var current = m_Start;

				int dist;

				for (dist = 0; current != null && current != m_TypeOfObject && current != type; ++dist)
				{
					current = current.BaseType;
				}

				return dist;
			}

			public int Compare(object x, object y)
			{
				if (x == null && y == null)
				{
					return 0;
				}

				if (x == null)
				{
					return -1;
				}

				if (y == null)
				{
					return 1;
				}

				if (x is not DictionaryEntry de1)
				{
					throw new ArgumentException($"Comparison was not a {typeof(DictionaryEntry)}", nameof(x));
				}

				if (y is not DictionaryEntry de2)
				{
					throw new ArgumentException($"Comparison was not a {typeof(DictionaryEntry)}", nameof(y));
				}

				var a = (Type)de1.Key;
				var b = (Type)de2.Key;

				return GetDistance(a).CompareTo(GetDistance(b));
			}
		}
	}

	public class SetBodyGump : Gump
	{
		public static string Center(string text)
		{
			return $"<CENTER>{text}</CENTER>";
		}

		public static string Color(string text, int color)
		{
			return $"<BASEFONT COLOR=#{color:X6}>{text}</BASEFONT>";
		}

		private readonly PropertyInfo m_Property;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Stack<StackEntry> m_Stack;
		private readonly int m_Page;
		private readonly ArrayList m_List;
		private readonly int m_OurPage;
		private readonly ArrayList m_OurList;
		private readonly BodyType m_OurType;

		private const int LabelColor32 = 0xFFFFFF;
		private const int SelectedColor32 = 0x8080FF;
		private const int TextColor32 = 0xFFFFFF;

		public SetBodyGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
			: this(prop, mobile, o, stack, page, list, 0, null, BodyType.Empty)
		{
		}

		public SetBodyGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list, int ourPage, ArrayList ourList, BodyType ourType)
			: base(20, 30)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Page = page;
			m_List = list;
			m_OurPage = ourPage;
			m_OurList = ourList;
			m_OurType = ourType;

			AddPage(0);

			AddBackground(0, 0, 525, 328, 5054);

			AddImageTiled(10, 10, 505, 20, 0xA40);
			AddAlphaRegion(10, 10, 505, 20);

			AddImageTiled(10, 35, 505, 283, 0xA40);
			AddAlphaRegion(10, 35, 505, 283);

			AddTypeButton(10, 10, 1, "Monster", BodyType.Monster);
			AddTypeButton(130, 10, 2, "Animal", BodyType.Animal);
			AddTypeButton(250, 10, 3, "Marine", BodyType.Sea);
			AddTypeButton(370, 10, 4, "Human", BodyType.Human);

			AddImage(480, 12, 0x25EA);
			AddImage(497, 12, 0x25E6);

			if (ourList == null)
			{
				AddLabel(15, 40, 0x480, "Choose a body type above.");
			}
			else if (ourList.Count == 0)
			{
				AddLabel(15, 40, 0x480, "The server must have UO:3D installed to use this feature.");
			}
			else
			{
				for (int i = 0, index = ourPage * 12; i < 12 && index >= 0 && index < ourList.Count; ++i, ++index)
				{
					var entry = (InternalEntry)ourList[index];
					var itemID = entry.ItemID;

					var bounds = ItemBounds.Table[itemID & 0x3FFF];

					var x = 15 + (i % 4 * 125);
					var y = 40 + (i / 4 * 93);

					AddItem(x + ((120 - bounds.Width) / 2) - bounds.X, y + ((69 - bounds.Height) / 2) - bounds.Y, itemID);
					AddButton(x + 6, y + 66, 0x98D, 0x98D, 7 + index, GumpButtonType.Reply, 0);

					x += 6;
					y += 67;

					AddHtml(x + 0, y - 1, 108, 21, Center(entry.DisplayName), false, false);
					AddHtml(x + 0, y + 1, 108, 21, Center(entry.DisplayName), false, false);
					AddHtml(x - 1, y + 0, 108, 21, Center(entry.DisplayName), false, false);
					AddHtml(x + 1, y + 0, 108, 21, Center(entry.DisplayName), false, false);
					AddHtml(x + 0, y + 0, 108, 21, Color(Center(entry.DisplayName), TextColor32), false, false);
				}

				if (ourPage > 0)
				{
					AddButton(480, 12, 0x15E3, 0x15E7, 5, GumpButtonType.Reply, 0);
				}

				if ((ourPage + 1) * 12 < ourList.Count)
				{
					AddButton(497, 12, 0x15E1, 0x15E5, 6, GumpButtonType.Reply, 0);
				}
			}
		}

		public void AddTypeButton(int x, int y, int buttonID, string text, BodyType type)
		{
			var isSelection = m_OurType == type;

			AddButton(x, y - 1, isSelection ? 4006 : 4005, 4007, buttonID, GumpButtonType.Reply, 0);
			AddHtml(x + 35, y, 200, 20, Color(text, isSelection ? SelectedColor32 : LabelColor32), false, false);
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var index = info.ButtonID - 1;

			if (index == -1)
			{
				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
			else if (index is >= 0 and < 4)
			{
				if (m_Monster == null)
				{
					LoadLists();
				}

				BodyType type;
				ArrayList list;

				switch (index)
				{
					default:
					case 0: type = BodyType.Monster; list = m_Monster; break;
					case 1: type = BodyType.Animal; list = m_Animal; break;
					case 2: type = BodyType.Sea; list = m_Sea; break;
					case 3: type = BodyType.Human; list = m_Human; break;
				}

				_ = m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List, 0, list, type));
			}
			else if (m_OurList != null)
			{
				index -= 4;

				if (index == 0 && m_OurPage > 0)
				{
					_ = m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List, m_OurPage - 1, m_OurList, m_OurType));
				}
				else if (index == 1 && ((m_OurPage + 1) * 12) < m_OurList.Count)
				{
					_ = m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List, m_OurPage + 1, m_OurList, m_OurType));
				}
				else
				{
					index -= 2;

					if (index >= 0 && index < m_OurList.Count)
					{
						try
						{
							var entry = (InternalEntry)m_OurList[index];

							CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, entry.Body.ToString());

							var oldValue = m_Property.GetValue(m_Object, null);

							m_Property.SetValue(m_Object, entry.Body, null);

							PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, entry.Body);
						}
						catch
						{
							m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
						}

						_ = m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List, m_OurPage, m_OurList, m_OurType));
					}
				}
			}
		}

		private static ArrayList m_Monster, m_Animal, m_Sea, m_Human;

		private static void LoadLists()
		{
			m_Monster = new ArrayList();
			m_Animal = new ArrayList();
			m_Sea = new ArrayList();
			m_Human = new ArrayList();

			var entries = Docs.LoadBodies();

			foreach (var entry in entries)
			{
				var body = entry.Body;

				if (body.IsEmpty)
				{
					continue;
				}

				ArrayList list = null;

				switch (entry.BodyType)
				{
					case BodyType.Monster: list = m_Monster; break;
					case BodyType.Animal: list = m_Animal; break;
					case BodyType.Sea: list = m_Sea; break;
					case BodyType.Human: list = m_Human; break;
				}

				if (list == null)
				{
					continue;
				}

				var itemID = ShrinkTable.Lookup(body, -1);

				if (itemID != -1)
				{
					_ = list.Add(new InternalEntry(body, itemID, entry.Name));
				}
			}

			m_Monster.Sort();
			m_Animal.Sort();
			m_Sea.Sort();
			m_Human.Sort();
		}

		private class InternalEntry : IComparable
		{
			public int Body { get; }
			public int ItemID { get; }
			public string Name { get; }
			public string DisplayName { get; }

			private static readonly string[] m_GroupNames = new string[]
			{
				"ogres_", "ettins_", "walking_dead_", "gargoyles_",
				"orcs_", "flails_", "daemons_", "arachnids_",
				"dragons_", "elementals_", "serpents_", "gazers_",
				"liche_", "spirits_", "harpies_", "headless_",
				"lizard_race_", "mongbat_", "rat_race_", "scorpions_",
				"trolls_", "slimes_", "skeletons_", "ethereals_",
				"terathan_", "imps_", "cyclops_", "krakens_",
				"frogs_", "ophidians_", "centaurs_", "mages_",
				"fey_race_", "genies_", "paladins_", "shadowlords_",
				"succubi_", "lizards_", "rodents_", "birds_",
				"bovines_", "bruins_", "canines_", "deer_",
				"equines_", "felines_", "fowl_", "gorillas_",
				"kirin_", "llamas_", "ostards_", "porcines_",
				"ruminants_", "walrus_", "dolphins_", "sea_horse_",
				"sea_serpents_", "character_", "h_", "titans_"
			};

			public InternalEntry(int body, int itemID, string name)
			{
				Body = body;
				ItemID = itemID;
				Name = name;

				DisplayName = name.ToLower();

				for (var i = 0; i < m_GroupNames.Length; ++i)
				{
					if (DisplayName.StartsWith(m_GroupNames[i]))
					{
						DisplayName = DisplayName.Substring(m_GroupNames[i].Length);
						break;
					}
				}

				DisplayName = DisplayName.Replace('_', ' ');
			}

			public int CompareTo(object obj)
			{
				var comp = (InternalEntry)obj;

				var v = Name.CompareTo(comp.Name);

				if (v == 0)
				{
					_ = Body.CompareTo(comp.Body);
				}

				return v;
			}
		}
	}

	public class SetCustomEnumGump : SetListOptionGump
	{
		private readonly string[] m_Names;

		public SetCustomEnumGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int propspage, ArrayList list, string[] names) : base(prop, mobile, o, stack, propspage, list, names, null)
		{
			m_Names = names;
		}

		public override void OnResponse(NetState sender, RelayInfo relayInfo)
		{
			var index = relayInfo.ButtonID - 1;

			if (index >= 0 && index < m_Names.Length)
			{
				try
				{
					var info = m_Property.PropertyType.GetMethod("Parse", new Type[] { typeof(string) });

					var result = "";

					var oldValue = m_Property.GetValue(m_Object, null);
					object newValue = null;

					if (info != null)
					{
						result = Props.SetDirect(m_Mobile, m_Object, m_Object, m_Property, m_Property.Name, newValue = info.Invoke(null, new object[] { m_Names[index] }), true);
					}
					else if (m_Property.PropertyType.IsEnum)
					{
						result = Props.SetDirect(m_Mobile, m_Object, m_Object, m_Property, m_Property.Name, newValue = Enum.Parse(m_Property.PropertyType, m_Names[index], false), true);
					}

					m_Mobile.SendMessage(result);

					if (result == "Property has been set.")
					{
						PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, newValue);
					}
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}
			}

			_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
		}
	}

	public class SetGump : Gump
	{
		private readonly PropertyInfo m_Property;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Stack<StackEntry> m_Stack;
		private readonly int m_Page;
		private readonly ArrayList m_List;

		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly int EntryWidth = 212;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + (2 * (EntryHeight + OffsetSize));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		public SetGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list) : base(GumpOffsetX, GumpOffsetY)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Page = page;
			m_List = list;

			var canNull = !prop.PropertyType.IsValueType;
			var canDye = prop.IsDefined(typeof(HueAttribute), false);
			var isBody = prop.IsDefined(typeof(BodyAttribute), false);

			var val = prop.GetValue(m_Object, null);

			string initialText;

			if (val == null)
			{
				initialText = "";
			}
			else if (val is TextDefinition td)
			{
				initialText = td.GetValue();
			}
			else
			{
				initialText = val.ToString();
			}

			AddPage(0);

			AddBackground(0, 0, BackWidth, BackHeight + (canNull ? (EntryHeight + OffsetSize) : 0) + (canDye ? (EntryHeight + OffsetSize) : 0) + (isBody ? (EntryHeight + OffsetSize) : 0), BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight + (canNull ? (EntryHeight + OffsetSize) : 0) + (canDye ? (EntryHeight + OffsetSize) : 0) + (isBody ? (EntryHeight + OffsetSize) : 0), OffsetGumpID);

			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddTextEntry(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, 0, initialText);
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 1, GumpButtonType.Reply, 0);

			if (canNull)
			{
				x = BorderSize + OffsetSize;
				y += EntryHeight + OffsetSize;

				AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
				AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Null");
				x += EntryWidth + OffsetSize;

				if (SetGumpID != 0)
				{
					AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
				}

				AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 2, GumpButtonType.Reply, 0);
			}

			if (canDye)
			{
				x = BorderSize + OffsetSize;
				y += EntryHeight + OffsetSize;

				AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
				AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Hue Picker");
				x += EntryWidth + OffsetSize;

				if (SetGumpID != 0)
				{
					AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
				}

				AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 3, GumpButtonType.Reply, 0);
			}

			if (isBody)
			{
				x = BorderSize + OffsetSize;
				y += EntryHeight + OffsetSize;

				AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
				AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Body Picker");
				x += EntryWidth + OffsetSize;

				if (SetGumpID != 0)
				{
					AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
				}

				AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 4, GumpButtonType.Reply, 0);
			}
		}

		private class InternalPicker : HuePicker
		{
			private readonly PropertyInfo m_Property;
			private readonly Mobile m_Mobile;
			private readonly object m_Object;
			private readonly Stack<StackEntry> m_Stack;
			private readonly int m_Page;
			private readonly ArrayList m_List;

			public InternalPicker(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list) : base((o as IHued)?.HuedItemID ?? 0x0FAB)
			{
				m_Property = prop;
				m_Mobile = mobile;
				m_Object = o;
				m_Stack = stack;
				m_Page = page;
				m_List = list;
			}

			public override void OnResponse(int hue)
			{
				try
				{
					CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, hue.ToString());

					var oldValue = m_Property.GetValue(m_Object, null);

					m_Property.SetValue(m_Object, hue, null);

					PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, hue);
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}

				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			object toSet;
			bool shouldSet, shouldSend = true;

			switch (info.ButtonID)
			{
				case 1:
					{
						var text = info.GetTextEntry(0);

						if (text != null)
						{
							try
							{
								toSet = PropertiesGump.GetObjectFromString(m_Property.PropertyType, text.Text);

								shouldSet = true;
							}
							catch
							{
								toSet = null;

								shouldSet = false;

								m_Mobile.SendMessage("Bad format");
							}
						}
						else
						{
							toSet = null;

							shouldSet = false;
						}

						break;
					}
				case 2: // Null
					{
						toSet = null;

						shouldSet = true;

						break;
					}
				case 3: // Hue Picker
					{
						toSet = null;

						shouldSet = false;
						shouldSend = false;

						_ = m_Mobile.SendHuePicker(new InternalPicker(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List));

						break;
					}
				case 4: // Body Picker
					{
						toSet = null;

						shouldSet = false;
						shouldSend = false;

						_ = m_Mobile.SendGump(new SetBodyGump(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List));

						break;
					}
				default:
					{
						toSet = null;

						shouldSet = false;

						break;
					}
			}

			if (shouldSet)
			{
				try
				{
					CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet == null ? "(null)" : toSet.ToString());

					var oldValue = m_Property.GetValue(m_Object, null);

					m_Property.SetValue(m_Object, toSet, null);

					PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}
			}

			if (shouldSend)
			{
				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
		}
	}

	public class SetListOptionGump : Gump
	{
		protected PropertyInfo m_Property;
		protected Mobile m_Mobile;
		protected object m_Object;
		protected Stack<StackEntry> m_Stack;
		protected int m_Page;
		protected ArrayList m_List;

		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly int EntryWidth = 212;
		private static readonly int EntryCount = 13;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;

		private static readonly bool PrevLabel = OldStyle, NextLabel = OldStyle;

		private static readonly int PrevLabelOffsetX = PrevWidth + 1;
		private static readonly int PrevLabelOffsetY = 0;

		private static readonly int NextLabelOffsetX = -29;
		private static readonly int NextLabelOffsetY = 0;

		protected object[] m_Values;

		public SetListOptionGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int propspage, ArrayList list, string[] names, object[] values) : base(GumpOffsetX, GumpOffsetY)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Page = propspage;
			m_List = list;

			m_Values = values;

			var pages = (names.Length + EntryCount - 1) / EntryCount;
			var index = 0;

			for (var page = 1; page <= pages; ++page)
			{
				AddPage(page);

				var start = (page - 1) * EntryCount;
				var count = names.Length - start;

				if (count > EntryCount)
				{
					count = EntryCount;
				}

				var totalHeight = OffsetSize + ((count + 2) * (EntryHeight + OffsetSize));
				var backHeight = BorderSize + totalHeight + BorderSize;

				AddBackground(0, 0, BackWidth, backHeight, BackGumpID);
				AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), totalHeight, OffsetGumpID);

				var x = BorderSize + OffsetSize;
				var y = BorderSize + OffsetSize;

				var emptyWidth = TotalWidth - PrevWidth - NextWidth - (OffsetSize * 4) - (OldStyle ? SetWidth + OffsetSize : 0);

				AddImageTiled(x, y, PrevWidth, EntryHeight, HeaderGumpID);

				if (page > 1)
				{
					AddButton(x + PrevOffsetX, y + PrevOffsetY, PrevButtonID1, PrevButtonID2, 0, GumpButtonType.Page, page - 1);

					if (PrevLabel)
					{
						AddLabel(x + PrevLabelOffsetX, y + PrevLabelOffsetY, TextHue, "Previous");
					}
				}

				x += PrevWidth + OffsetSize;

				if (!OldStyle)
				{
					AddImageTiled(x - (OldStyle ? OffsetSize : 0), y, emptyWidth + (OldStyle ? OffsetSize * 2 : 0), EntryHeight, HeaderGumpID);
				}

				x += emptyWidth + OffsetSize;

				if (!OldStyle)
				{
					AddImageTiled(x, y, NextWidth, EntryHeight, HeaderGumpID);
				}

				if (page < pages)
				{
					AddButton(x + NextOffsetX, y + NextOffsetY, NextButtonID1, NextButtonID2, 0, GumpButtonType.Page, page + 1);

					if (NextLabel)
					{
						AddLabel(x + NextLabelOffsetX, y + NextLabelOffsetY, TextHue, "Next");
					}
				}

				AddRect(0, prop.Name, 0);

				for (var i = 0; i < count; ++i)
				{
					AddRect(i + 1, names[index], ++index);
				}
			}
		}

		private void AddRect(int index, string str, int button)
		{
			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize + ((index + 1) * (EntryHeight + OffsetSize));

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, str);

			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			if (button != 0)
			{
				AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, button, GumpButtonType.Reply, 0);
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var index = info.ButtonID - 1;

			if (index >= 0 && index < m_Values.Length)
			{
				try
				{
					var oldValue = m_Property.GetValue(m_Object, null);

					var toSet = m_Values[index];

					var result = Props.SetDirect(m_Mobile, m_Object, m_Object, m_Property, m_Property.Name, toSet, true);

					m_Mobile.SendMessage(result);

					if (result == "Property has been set.")
					{
						PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
					}
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}
			}

			_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
		}
	}

	public class SetObjectGump : Gump
	{
		private readonly PropertyInfo m_Property;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Stack<StackEntry> m_Stack;
		private readonly Type m_Type;
		private readonly int m_Page;
		private readonly ArrayList m_List;

		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly int EntryWidth = 212;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + (5 * (EntryHeight + OffsetSize));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		public SetObjectGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, Type type, int page, ArrayList list) : base(GumpOffsetX, GumpOffsetY)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Type = type;
			m_Page = page;
			m_List = list;

			var initialText = PropertiesGump.ValueToString(o, prop);

			AddPage(0);

			AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, initialText);
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 1, GumpButtonType.Reply, 0);

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Change by Serial");
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 2, GumpButtonType.Reply, 0);

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Nullify");
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 3, GumpButtonType.Reply, 0);

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "View Properties");
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 4, GumpButtonType.Reply, 0);
		}

		private class InternalPrompt : Prompt
		{
			private readonly PropertyInfo m_Property;
			private readonly Mobile m_Mobile;
			private readonly object m_Object;
			private readonly Stack<StackEntry> m_Stack;
			private readonly Type m_Type;
			private readonly int m_Page;
			private readonly ArrayList m_List;

			public InternalPrompt(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, Type type, int page, ArrayList list)
			{
				m_Property = prop;
				m_Mobile = mobile;
				m_Object = o;
				m_Stack = stack;
				m_Type = type;
				m_Page = page;
				m_List = list;
			}

			public override void OnCancel(Mobile from)
			{
				_ = m_Mobile.SendGump(new SetObjectGump(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List));
			}

			public override void OnResponse(Mobile from, string text)
			{
				object toSet;
				bool shouldSet;

				try
				{
					var serial = Utility.ToSerial(text);

					toSet = World.FindEntity(serial);

					if (toSet == null)
					{
						shouldSet = false;

						m_Mobile.SendMessage("No object with that serial was found.");
					}
					else if (!m_Type.IsAssignableFrom(toSet.GetType()))
					{
						toSet = null;

						shouldSet = false;

						m_Mobile.SendMessage("The object with that serial could not be assigned to a property of type : {0}", m_Type.Name);
					}
					else
					{
						shouldSet = true;
					}
				}
				catch
				{
					toSet = null;

					shouldSet = false;

					m_Mobile.SendMessage("Bad format");
				}

				if (shouldSet)
				{
					try
					{
						CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet == null ? "(null)" : toSet.ToString());

						var oldValue = m_Property.GetValue(m_Object, null);

						m_Property.SetValue(m_Object, toSet, null);

						PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
					}
					catch
					{
						m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
					}
				}

				_ = m_Mobile.SendGump(new SetObjectGump(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List));
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			object toSet, viewProps = null;
			bool shouldSet, shouldSend = true;

			switch (info.ButtonID)
			{
				case 0: // closed
					{
						_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));

						toSet = null;

						shouldSet = false;
						shouldSend = false;

						break;
					}
				case 1: // Change by Target
					{
						m_Mobile.Target = new SetObjectTarget(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List);

						toSet = null;

						shouldSet = false;
						shouldSend = false;

						break;
					}
				case 2: // Change by Serial
					{
						toSet = null;

						shouldSet = false;
						shouldSend = false;

						m_Mobile.SendMessage("Enter the serial you wish to find:");
						m_Mobile.Prompt = new InternalPrompt(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List);

						break;
					}
				case 3: // Nullify
					{
						toSet = null;

						shouldSet = true;

						break;
					}
				case 4: // View Properties
					{
						toSet = null;

						shouldSet = false;

						var obj = m_Property.GetValue(m_Object, null);

						if (obj == null)
						{
							m_Mobile.SendMessage("The property is null and so you cannot view its properties.");
						}
						else if (!BaseCommand.IsAccessible(m_Mobile, obj))
						{
							m_Mobile.SendMessage("You may not view their properties.");
						}
						else
						{
							viewProps = obj;
						}

						break;
					}
				default:
					{
						toSet = null;

						shouldSet = false;

						break;
					}
			}

			if (shouldSet)
			{
				try
				{
					CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet == null ? "(null)" : toSet.ToString());

					var oldValue = m_Property.GetValue(m_Object, null);

					m_Property.SetValue(m_Object, toSet, null);

					PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}
			}

			if (shouldSend)
			{
				_ = m_Mobile.SendGump(new SetObjectGump(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List));
			}

			if (viewProps != null)
			{
				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, viewProps));
			}
		}
	}

	public class SetObjectTarget : Target
	{
		private readonly PropertyInfo m_Property;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Stack<StackEntry> m_Stack;
		private readonly Type m_Type;
		private readonly int m_Page;
		private readonly ArrayList m_List;

		public SetObjectTarget(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, Type type, int page, ArrayList list) : base(-1, false, TargetFlags.None)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Type = type;
			m_Page = page;
			m_List = list;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			try
			{
				if (m_Type == typeof(Type))
				{
					targeted = targeted.GetType();
				}
				else if ((m_Type == typeof(BaseAddon) || m_Type.IsAssignableFrom(typeof(BaseAddon))) && targeted is AddonComponent ac)
				{
					targeted = ac.Addon;
				}

				var type = targeted.GetType();

				if (m_Type.IsAssignableFrom(type))
				{
					var state = TypeFilterAttribute.CheckState(m_Property, type);

					if (state == TypeFilterResult.NoFilter || state == TypeFilterResult.Allowed)
					{
						CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, targeted.ToString());

						var oldValue = m_Property.GetValue(m_Object, null);

						m_Property.SetValue(m_Object, targeted, null);

						PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, targeted);
					}
					else
					{
						m_Mobile.SendMessage("That is not a valid type for this property.");
					}
				}
				else
				{
					m_Mobile.SendMessage($"That cannot be assigned to a property of type : {m_Type.Name}");
				}
			}
			catch
			{
				m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			if (m_Type == typeof(Type))
			{
				_ = from.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
			else
			{
				_ = from.SendGump(new SetObjectGump(m_Property, m_Mobile, m_Object, m_Stack, m_Type, m_Page, m_List));
			}
		}
	}

	public class SetPoint2DGump : Gump
	{
		private readonly PropertyInfo m_Property;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Stack<StackEntry> m_Stack;
		private readonly int m_Page;
		private readonly ArrayList m_List;

		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly int CoordWidth = 105;
		private static readonly int EntryWidth = CoordWidth + OffsetSize + CoordWidth;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + (4 * (EntryHeight + OffsetSize));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		public SetPoint2DGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list) : base(GumpOffsetX, GumpOffsetY)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Page = page;
			m_List = list;

			var p = (Point2D)prop.GetValue(o, null);

			AddPage(0);

			AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Use your location");
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 1, GumpButtonType.Reply, 0);

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Target a location");
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 2, GumpButtonType.Reply, 0);

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "X:");
			AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 0, p.X.ToString());
			x += CoordWidth + OffsetSize;

			AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "Y:");
			AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 1, p.Y.ToString());
			x += CoordWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 3, GumpButtonType.Reply, 0);
		}

		private class InternalTarget : Target
		{
			private readonly PropertyInfo m_Property;
			private readonly Mobile m_Mobile;
			private readonly object m_Object;
			private readonly Stack<StackEntry> m_Stack;
			private readonly int m_Page;
			private readonly ArrayList m_List;

			public InternalTarget(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list) : base(-1, true, TargetFlags.None)
			{
				m_Property = prop;
				m_Mobile = mobile;
				m_Object = o;
				m_Stack = stack;
				m_Page = page;
				m_List = list;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is IPoint3D p)
				{
					try
					{
						var toSet = new Point2D(p);

						CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString());

						var oldValue = m_Property.GetValue(m_Object, null);

						m_Property.SetValue(m_Object, toSet, null);

						PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
					}
					catch
					{
						m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
					}
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Point2D toSet;
			bool shouldSet, shouldSend;

			switch (info.ButtonID)
			{
				case 1: // Current location
					{
						toSet = new Point2D(m_Mobile.Location);

						shouldSet = true;
						shouldSend = true;

						break;
					}
				case 2: // Pick location
					{
						m_Mobile.Target = new InternalTarget(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List);

						toSet = Point2D.Zero;

						shouldSet = false;
						shouldSend = false;

						break;
					}
				case 3: // Use values
					{
						var x = info.GetTextEntry(0);
						var y = info.GetTextEntry(1);

						toSet = new Point2D(x == null ? 0 : Utility.ToInt32(x.Text), y == null ? 0 : Utility.ToInt32(y.Text));

						shouldSet = true;
						shouldSend = true;

						break;
					}
				default:
					{
						toSet = Point2D.Zero;

						shouldSet = false;
						shouldSend = true;

						break;
					}
			}

			if (shouldSet)
			{
				try
				{
					CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString());

					var oldValue = m_Property.GetValue(m_Object, null);

					m_Property.SetValue(m_Object, toSet, null);

					PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}
			}

			if (shouldSend)
			{
				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
		}
	}

	public class SetPoint3DGump : Gump
	{
		private readonly PropertyInfo m_Property;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Stack<StackEntry> m_Stack;
		private readonly int m_Page;
		private readonly ArrayList m_List;

		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly int CoordWidth = 70;
		private static readonly int EntryWidth = CoordWidth + OffsetSize + CoordWidth + OffsetSize + CoordWidth;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + (4 * (EntryHeight + OffsetSize));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		public SetPoint3DGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list) : base(GumpOffsetX, GumpOffsetY)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Page = page;
			m_List = list;

			var p = (Point3D)prop.GetValue(o, null);

			AddPage(0);

			AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, prop.Name);
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Use your location");
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 1, GumpButtonType.Reply, 0);

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, "Target a location");
			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 2, GumpButtonType.Reply, 0);

			x = BorderSize + OffsetSize;
			y += EntryHeight + OffsetSize;

			AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "X:");
			AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 0, p.X.ToString());
			x += CoordWidth + OffsetSize;

			AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "Y:");
			AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 1, p.Y.ToString());
			x += CoordWidth + OffsetSize;

			AddImageTiled(x, y, CoordWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, CoordWidth - TextOffsetX, EntryHeight, TextHue, "Z:");
			AddTextEntry(x + 16, y, CoordWidth - 16, EntryHeight, TextHue, 2, p.Z.ToString());
			x += CoordWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, 3, GumpButtonType.Reply, 0);
		}

		private class InternalTarget : Target
		{
			private readonly PropertyInfo m_Property;
			private readonly Mobile m_Mobile;
			private readonly object m_Object;
			private readonly Stack<StackEntry> m_Stack;
			private readonly int m_Page;
			private readonly ArrayList m_List;

			public InternalTarget(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list) : base(-1, true, TargetFlags.None)
			{
				m_Property = prop;
				m_Mobile = mobile;
				m_Object = o;
				m_Stack = stack;
				m_Page = page;
				m_List = list;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is IPoint3D p)
				{
					try
					{
						var toSet = new Point3D(p);

						CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString());

						var oldValue = m_Property.GetValue(m_Object, null);

						m_Property.SetValue(m_Object, toSet, null);

						PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
					}
					catch
					{
						m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
					}
				}
			}

			protected override void OnTargetFinish(Mobile from)
			{
				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Point3D toSet;
			bool shouldSet, shouldSend;

			switch (info.ButtonID)
			{
				case 1: // Current location
					{
						toSet = m_Mobile.Location;

						shouldSet = true;
						shouldSend = true;

						break;
					}
				case 2: // Pick location
					{
						m_Mobile.Target = new InternalTarget(m_Property, m_Mobile, m_Object, m_Stack, m_Page, m_List);

						toSet = Point3D.Zero;

						shouldSet = false;
						shouldSend = false;

						break;
					}
				case 3: // Use values
					{
						var x = info.GetTextEntry(0);
						var y = info.GetTextEntry(1);
						var z = info.GetTextEntry(2);

						toSet = new Point3D(x == null ? 0 : Utility.ToInt32(x.Text), y == null ? 0 : Utility.ToInt32(y.Text), z == null ? 0 : Utility.ToInt32(z.Text));

						shouldSet = true;
						shouldSend = true;

						break;
					}
				default:
					{
						toSet = Point3D.Zero;

						shouldSet = false;
						shouldSend = true;

						break;
					}
			}

			if (shouldSet)
			{
				try
				{
					CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString());

					var oldValue = m_Property.GetValue(m_Object, null);

					m_Property.SetValue(m_Object, toSet, null);

					PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}
			}

			if (shouldSend)
			{
				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
		}
	}

	public class SetTimeSpanGump : Gump
	{
		private readonly PropertyInfo m_Property;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Stack<StackEntry> m_Stack;
		private readonly int m_Page;
		private readonly ArrayList m_List;

		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly int EntryWidth = 212;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + (7 * (EntryHeight + OffsetSize));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		public SetTimeSpanGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list) : base(GumpOffsetX, GumpOffsetY)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Page = page;
			m_List = list;

			var ts = (TimeSpan)prop.GetValue(o, null);

			AddPage(0);

			AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

			AddRect(0, prop.Name, 0, -1);
			AddRect(1, ts.ToString(), 0, -1);
			AddRect(2, "Zero", 1, -1);
			AddRect(3, "From H:M:S", 2, -1);
			AddRect(4, "H:", 3, 0);
			AddRect(5, "M:", 4, 1);
			AddRect(6, "S:", 5, 2);
		}

		private void AddRect(int index, string str, int button, int text)
		{
			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize + (index * (EntryHeight + OffsetSize));

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, str);

			if (text != -1)
			{
				AddTextEntry(x + 16 + TextOffsetX, y, EntryWidth - TextOffsetX - 16, EntryHeight, TextHue, text, "");
			}

			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			if (button != 0)
			{
				AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, button, GumpButtonType.Reply, 0);
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			TimeSpan toSet;
			bool shouldSet, shouldSend;

			var h = info.GetTextEntry(0);
			var m = info.GetTextEntry(1);
			var s = info.GetTextEntry(2);

			switch (info.ButtonID)
			{
				case 1: // Zero
					{
						toSet = TimeSpan.Zero;

						shouldSet = true;
						shouldSend = true;

						break;
					}
				case 2: // From H:M:S
					{
						var successfulParse = false;

						if (h != null && m != null && s != null)
						{
							successfulParse = TimeSpan.TryParse(h.Text + ":" + m.Text + ":" + s.Text, out toSet);
						}
						else
						{
							toSet = TimeSpan.Zero;
						}

						shouldSet = successfulParse;
						shouldSend = successfulParse;

						break;
					}
				case 3: // From H
					{
						if (h != null)
						{
							try
							{
								toSet = TimeSpan.FromHours(Utility.ToDouble(h.Text));

								shouldSet = true;
								shouldSend = true;

								break;
							}
							catch
							{
							}
						}

						toSet = TimeSpan.Zero;

						shouldSet = false;
						shouldSend = false;

						break;
					}
				case 4: // From M
					{
						if (m != null)
						{
							try
							{
								toSet = TimeSpan.FromMinutes(Utility.ToDouble(m.Text));

								shouldSet = true;
								shouldSend = true;

								break;
							}
							catch
							{
							}
						}

						toSet = TimeSpan.Zero;

						shouldSet = false;
						shouldSend = false;

						break;
					}
				case 5: // From S
					{
						if (s != null)
						{
							try
							{
								toSet = TimeSpan.FromSeconds(Utility.ToDouble(s.Text));

								shouldSet = true;
								shouldSend = true;

								break;
							}
							catch
							{
							}
						}

						toSet = TimeSpan.Zero;

						shouldSet = false;
						shouldSend = false;

						break;
					}
				default:
					{
						toSet = TimeSpan.Zero;

						shouldSet = false;
						shouldSend = true;

						break;
					}
			}

			if (shouldSet)
			{
				try
				{
					CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString());

					var oldValue = m_Property.GetValue(m_Object, null);

					m_Property.SetValue(m_Object, toSet, null);

					PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}
			}

			if (shouldSend)
			{
				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
		}
	}

	public class SetDateTimeGump : Gump
	{
		private readonly DateTime m_OldDT;
		private readonly PropertyInfo m_Property;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Stack<StackEntry> m_Stack;
		private readonly int m_Page;
		private readonly ArrayList m_List;

		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly int EntryWidth = 212;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + (12 * (EntryHeight + OffsetSize));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		public SetDateTimeGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
			: base(GumpOffsetX, GumpOffsetY)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Page = page;
			m_List = list;

			m_OldDT = (DateTime)prop.GetValue(o, null);

			AddPage(0);

			AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

			AddRect(0, prop.Name, 0, -1);
			AddRect(1, $"{m_OldDT:u}", 0, -1);
			AddRect(2, "MinValue", 1, -1);
			AddRect(3, "From YYYY:MM:DD hh:mm", 2, -1);
			AddRect(4, "From YYYY:MM:DD", 3, -1);
			AddRect(5, "From hh:mm", 4, -1);
			AddRect(6, "Year:", 5, 0);
			AddRect(7, "Month:", 6, 1);
			AddRect(8, "Day:", 7, 2);
			AddRect(9, "Hour:", 8, 3);
			AddRect(10, "Minute:", 9, 4);
			AddRect(11, "MaxValue", 10, -1);
		}

		private void AddRect(int index, string str, int button, int text)
		{
			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize + (index * (EntryHeight + OffsetSize));

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, str);

			if (text != -1)
			{
				AddTextEntry(x + 40 + TextOffsetX, y, EntryWidth - TextOffsetX - 16, EntryHeight, TextHue, text, "");
			}

			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			if (button != 0)
			{
				AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, button, GumpButtonType.Reply, 0);
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			DateTime toSet;
			bool shouldSet, shouldSend;

			var year = "";

			if (info.ButtonID is 2 or 3 or 5)
			{
				year = info.GetTextEntry(0).Text;
			}

			var month = "";

			if (info.ButtonID is 2 or 3 or 6)
			{
				month = info.GetTextEntry(1).Text;
			}

			var day = "";

			if (info.ButtonID is 2 or 3 or 7)
			{
				day = info.GetTextEntry(2).Text;
			}

			var hour = "";

			if (info.ButtonID is 2 or 4 or 8)
			{
				hour = info.GetTextEntry(3).Text;
			}

			var min = "";

			if (info.ButtonID is 2 or 4 or 9)
			{
				min = info.GetTextEntry(4).Text;
			}

			switch (info.ButtonID)
			{
				case 1: // MinValue
					{
						toSet = DateTime.MinValue;

						shouldSet = true;
						shouldSend = true;

						break;
					}
				case 2: // From YYYY MM DD H:M
					{
						var toapply = $"{(year.Length > 0 ? year : $"{m_OldDT:yyyy}")}/{(month.Length > 0 ? month : $"{m_OldDT:MM}")}/{(day.Length > 0 ? day : $"{m_OldDT:dd}")} {(hour.Length > 0 ? hour : $"{m_OldDT:HH}")}:{(min.Length > 0 ? min : $"{m_OldDT:mm}")}:00";

						var successfulParse = DateTime.TryParse(toapply, out toSet);

						shouldSet = successfulParse;
						shouldSend = successfulParse;

						break;
					}
				case 3:
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
					goto case 2;
				case 10:
					{
						toSet = DateTime.MaxValue;

						shouldSet = true;
						shouldSend = true;

						break;
					}
				default:
					{
						toSet = DateTime.MinValue;

						shouldSet = false;
						shouldSend = true;

						break;
					}
			}

			if (shouldSet)
			{
				try
				{
					CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString(CultureInfo.InvariantCulture));

					var oldValue = m_Property.GetValue(m_Object, null);

					m_Property.SetValue(m_Object, toSet, null);

					PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}
			}

			if (shouldSend)
			{
				_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
			}
		}
	}

	public class SetColorGump : Gump
	{
		private readonly Color m_OldColor;
		private readonly PropertyInfo m_Property;
		private readonly Mobile m_Mobile;
		private readonly object m_Object;
		private readonly Stack<StackEntry> m_Stack;
		private readonly int m_Page;
		private readonly ArrayList m_List;

		public static readonly bool OldStyle = PropsConfig.OldStyle;

		public static readonly int GumpOffsetX = PropsConfig.GumpOffsetX;
		public static readonly int GumpOffsetY = PropsConfig.GumpOffsetY;

		public static readonly int TextHue = PropsConfig.TextHue;
		public static readonly int TextOffsetX = PropsConfig.TextOffsetX;

		public static readonly int OffsetGumpID = PropsConfig.OffsetGumpID;
		public static readonly int HeaderGumpID = PropsConfig.HeaderGumpID;
		public static readonly int EntryGumpID = PropsConfig.EntryGumpID;
		public static readonly int BackGumpID = PropsConfig.BackGumpID;
		public static readonly int SetGumpID = PropsConfig.SetGumpID;

		public static readonly int SetWidth = PropsConfig.SetWidth;
		public static readonly int SetOffsetX = PropsConfig.SetOffsetX, SetOffsetY = PropsConfig.SetOffsetY;
		public static readonly int SetButtonID1 = PropsConfig.SetButtonID1;
		public static readonly int SetButtonID2 = PropsConfig.SetButtonID2;

		public static readonly int PrevWidth = PropsConfig.PrevWidth;
		public static readonly int PrevOffsetX = PropsConfig.PrevOffsetX, PrevOffsetY = PropsConfig.PrevOffsetY;
		public static readonly int PrevButtonID1 = PropsConfig.PrevButtonID1;
		public static readonly int PrevButtonID2 = PropsConfig.PrevButtonID2;

		public static readonly int NextWidth = PropsConfig.NextWidth;
		public static readonly int NextOffsetX = PropsConfig.NextOffsetX, NextOffsetY = PropsConfig.NextOffsetY;
		public static readonly int NextButtonID1 = PropsConfig.NextButtonID1;
		public static readonly int NextButtonID2 = PropsConfig.NextButtonID2;

		public static readonly int OffsetSize = PropsConfig.OffsetSize;

		public static readonly int EntryHeight = PropsConfig.EntryHeight;
		public static readonly int BorderSize = PropsConfig.BorderSize;

		private static readonly int EntryWidth = 212;

		private static readonly int TotalWidth = OffsetSize + EntryWidth + OffsetSize + SetWidth + OffsetSize;
		private static readonly int TotalHeight = OffsetSize + (6 * (EntryHeight + OffsetSize));

		private static readonly int BackWidth = BorderSize + TotalWidth + BorderSize;
		private static readonly int BackHeight = BorderSize + TotalHeight + BorderSize;

		public SetColorGump(PropertyInfo prop, Mobile mobile, object o, Stack<StackEntry> stack, int page, ArrayList list)
			: base(GumpOffsetX, GumpOffsetY)
		{
			m_Property = prop;
			m_Mobile = mobile;
			m_Object = o;
			m_Stack = stack;
			m_Page = page;
			m_List = list;

			m_OldColor = (Color)prop.GetValue(o, null);

			AddPage(0);

			AddBackground(0, 0, BackWidth, BackHeight, BackGumpID);
			AddImageTiled(BorderSize, BorderSize, TotalWidth - (OldStyle ? SetWidth + OffsetSize : 0), TotalHeight, OffsetGumpID);

			var name = m_OldColor.IsNamedColor ? m_OldColor.Name : m_OldColor.IsEmpty ? "Empty" : "";

			var rgb = $"#{m_OldColor.ToArgb() & 0x00FFFFFF:X6}";

			var val = $"{name} ({rgb}) ({m_OldColor.R},{m_OldColor.G},{m_OldColor.B})";

			AddRect(0, prop.Name, 0, -1);
			AddRect(1, val, 0, -1);
			AddRect(2, "Name:", 1, 0);
			AddRect(3, "RGB:", 2, 1);
			AddRect(4, "Hex:", 3, 2);
			AddRect(5, "Empty", 4, -1);
		}

		private void AddRect(int index, string str, int button, int text)
		{
			var x = BorderSize + OffsetSize;
			var y = BorderSize + OffsetSize + (index * (EntryHeight + OffsetSize));

			AddImageTiled(x, y, EntryWidth, EntryHeight, EntryGumpID);
			AddLabelCropped(x + TextOffsetX, y, EntryWidth - TextOffsetX, EntryHeight, TextHue, str);

			if (text != -1)
			{
				AddTextEntry(x + 40 + TextOffsetX, y, EntryWidth - TextOffsetX - 16, EntryHeight, TextHue, text, "");
			}

			x += EntryWidth + OffsetSize;

			if (SetGumpID != 0)
			{
				AddImageTiled(x, y, SetWidth, EntryHeight, SetGumpID);
			}

			if (button != 0)
			{
				AddButton(x + SetOffsetX, y + SetOffsetY, SetButtonID1, SetButtonID2, button, GumpButtonType.Reply, 0);
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			var toSet = Color.Empty;
			var shouldSet = false;

			var name = "";

			if (info.ButtonID == 1)
			{
				name = info.GetTextEntry(0).Text;
			}

			var rgb = "";

			if (info.ButtonID == 2)
			{
				rgb = info.GetTextEntry(1).Text;
			}

			var hex = "";

			if (info.ButtonID == 3)
			{
				hex = info.GetTextEntry(2).Text;
			}

			switch (info.ButtonID)
			{
				case 1: // Name
					{
						var toapply = name.Length > 0 ? name : m_OldColor.IsNamedColor ? m_OldColor.Name : m_OldColor.IsEmpty ? "Empty" : "";

						toSet = Color.FromName(toapply);

						shouldSet = true;
					}
					break;
				case 2: // RGB
					{
						var toapply = rgb.Length > 0 ? rgb : $"{m_OldColor.R},{m_OldColor.G},{m_OldColor.B}";

						var args = toapply.Split(',');

						if (args.Length >= 3)
						{
							byte r, g, b;

							if (Byte.TryParse(args[0], out r) && Byte.TryParse(args[1], out g) && Byte.TryParse(args[2], out b))
							{
								toSet = Color.FromArgb(r, g, b);
								shouldSet = true;
							}
						}
					}
					break;
				case 3: // Hex
					{
						var toapply = hex.Length > 0 ? hex : $"#{m_OldColor.ToArgb() & 0x00FFFFFF:X6}";

						int val;

						if (Int32.TryParse(toapply.TrimStart('#'), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out val))
						{
							toSet = Color.FromArgb(val);
							shouldSet = true;
						}
					}
					break;
				case 4: // Empty
					{
						toSet = Color.Empty;
						shouldSet = true;
					}
					break;
			}

			if (shouldSet)
			{
				try
				{
					CommandLogging.LogChangeProperty(m_Mobile, m_Object, m_Property.Name, toSet.ToString());

					var oldValue = m_Property.GetValue(m_Object, null);

					m_Property.SetValue(m_Object, toSet, null);

					PropertiesGump.OnValueChanged(m_Stack, m_Object, m_Property, oldValue, toSet);
				}
				catch
				{
					m_Mobile.SendMessage("An exception was caught. The property may not have changed.");
				}
			}

			_ = m_Mobile.SendGump(new PropertiesGump(m_Mobile, m_Object, m_Stack, m_List, m_Page));
		}
	}
}