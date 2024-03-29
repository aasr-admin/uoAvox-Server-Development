﻿
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Commands.Generic
{
	public delegate BaseExtension ExtensionConstructor();

	public sealed class ExtensionInfo
	{
		private static readonly Dictionary<string, ExtensionInfo> m_Table = new Dictionary<string, ExtensionInfo>(StringComparer.InvariantCultureIgnoreCase);

		public static Dictionary<string, ExtensionInfo> Table => m_Table;

		public static void Register(ExtensionInfo ext)
		{
			m_Table[ext.m_Name] = ext;
		}

		private readonly int m_Order;

		private readonly string m_Name;
		private readonly int m_Size;

		private readonly ExtensionConstructor m_Constructor;

		public int Order => m_Order;

		public string Name => m_Name;

		public int Size => m_Size;

		public bool IsFixedSize => (m_Size >= 0);

		public ExtensionConstructor Constructor => m_Constructor;

		public ExtensionInfo(int order, string name, int size, ExtensionConstructor constructor)
		{
			m_Name = name;
			m_Size = size;

			m_Order = order;

			m_Constructor = constructor;
		}
	}

	public sealed class Extensions : List<BaseExtension>
	{
		public Extensions()
		{
		}

		public bool IsValid(object obj)
		{
			for (var i = 0; i < Count; ++i)
			{
				if (!this[i].IsValid(obj))
				{
					return false;
				}
			}

			return true;
		}

		public void Filter(ArrayList list)
		{
			for (var i = 0; i < Count; ++i)
			{
				this[i].Filter(list);
			}
		}

		public static Extensions Parse(Mobile from, ref string[] args)
		{
			var parsed = new Extensions();

			var size = args.Length;

			Type baseType = null;

			for (var i = args.Length - 1; i >= 0; --i)
			{
				ExtensionInfo extInfo = null;

				if (!ExtensionInfo.Table.TryGetValue(args[i], out extInfo))
				{
					continue;
				}

				if (extInfo.IsFixedSize && i != (size - extInfo.Size - 1))
				{
					throw new Exception("Invalid extended argument count.");
				}

				var ext = extInfo.Constructor();

				ext.Parse(from, args, i + 1, size - i - 1);

				if (ext is WhereExtension)
				{
					baseType = (ext as WhereExtension).Conditional.Type;
				}

				parsed.Add(ext);

				size = i;
			}

			parsed.Sort(delegate (BaseExtension a, BaseExtension b)
			{
				return (a.Order - b.Order);
			});

			AssemblyEmitter emitter = null;

			foreach (var update in parsed)
			{
				update.Optimize(from, baseType, ref emitter);
			}

			if (size != args.Length)
			{
				var old = args;
				args = new string[size];

				for (var i = 0; i < args.Length; ++i)
				{
					args[i] = old[i];
				}
			}

			return parsed;
		}
	}

	public abstract class BaseExtension
	{
		public abstract ExtensionInfo Info { get; }

		public string Name => Info.Name;

		public int Size => Info.Size;

		public bool IsFixedSize => Info.IsFixedSize;

		public int Order => Info.Order;

		public virtual void Optimize(Mobile from, Type baseType, ref AssemblyEmitter assembly)
		{
		}

		public virtual void Parse(Mobile from, string[] arguments, int offset, int size)
		{
		}

		public virtual bool IsValid(object obj)
		{
			return true;
		}

		public virtual void Filter(ArrayList list)
		{
		}
	}

	public sealed class DistinctExtension : BaseExtension
	{
		public static ExtensionInfo ExtInfo = new ExtensionInfo(30, "Distinct", -1, delegate () { return new DistinctExtension(); });

		public static void Initialize()
		{
			ExtensionInfo.Register(ExtInfo);
		}

		public override ExtensionInfo Info => ExtInfo;

		private readonly List<Property> m_Properties;

		private IComparer m_Comparer;

		public DistinctExtension()
		{
			m_Properties = new List<Property>();
		}

		public override void Optimize(Mobile from, Type baseType, ref AssemblyEmitter assembly)
		{
			if (baseType == null)
			{
				throw new Exception("Distinct extension may only be used in combination with an object conditional.");
			}

			foreach (var prop in m_Properties)
			{
				prop.BindTo(baseType, PropertyAccess.Read);
				prop.CheckAccess(from);
			}

			if (assembly == null)
			{
				assembly = new AssemblyEmitter("__dynamic");
			}

			m_Comparer = DistinctCompiler.Compile(assembly, baseType, m_Properties.ToArray());
		}

		public override void Parse(Mobile from, string[] arguments, int offset, int size)
		{
			if (size < 1)
			{
				throw new Exception("Invalid distinction syntax.");
			}

			var end = offset + size;

			while (offset < end)
			{
				var binding = arguments[offset++];

				m_Properties.Add(new Property(binding));
			}
		}

		public override void Filter(ArrayList list)
		{
			if (m_Comparer == null)
			{
				throw new InvalidOperationException("The extension must first be optimized.");
			}

			var copy = new ArrayList(list);

			copy.Sort(m_Comparer);

			list.Clear();

			object last = null;

			for (var i = 0; i < copy.Count; ++i)
			{
				var obj = copy[i];

				if (last == null || m_Comparer.Compare(obj, last) != 0)
				{
					list.Add(obj);
					last = obj;
				}
			}
		}
	}

	public sealed class LimitExtension : BaseExtension
	{
		public static ExtensionInfo ExtInfo = new ExtensionInfo(80, "Limit", 1, delegate () { return new LimitExtension(); });

		public static void Initialize()
		{
			ExtensionInfo.Register(ExtInfo);
		}

		public override ExtensionInfo Info => ExtInfo;

		private int m_Limit;

		public int Limit => m_Limit;

		public LimitExtension()
		{
		}

		public override void Parse(Mobile from, string[] arguments, int offset, int size)
		{
			m_Limit = Utility.ToInt32(arguments[offset]);

			if (m_Limit < 0)
			{
				throw new Exception("Limit cannot be less than zero.");
			}
		}

		public override void Filter(ArrayList list)
		{
			if (list.Count > m_Limit)
			{
				list.RemoveRange(m_Limit, list.Count - m_Limit);
			}
		}
	}

	public sealed class SortExtension : BaseExtension
	{
		public static ExtensionInfo ExtInfo = new ExtensionInfo(40, "Order", -1, delegate () { return new SortExtension(); });

		public static void Initialize()
		{
			ExtensionInfo.Register(ExtInfo);
		}

		public override ExtensionInfo Info => ExtInfo;

		private readonly List<OrderInfo> m_Orders;

		private IComparer m_Comparer;

		public SortExtension()
		{
			m_Orders = new List<OrderInfo>();
		}

		public override void Optimize(Mobile from, Type baseType, ref AssemblyEmitter assembly)
		{
			if (baseType == null)
			{
				throw new Exception("The ordering extension may only be used in combination with an object conditional.");
			}

			foreach (var order in m_Orders)
			{
				order.Property.BindTo(baseType, PropertyAccess.Read);
				order.Property.CheckAccess(from);
			}

			if (assembly == null)
			{
				assembly = new AssemblyEmitter("__dynamic");
			}

			m_Comparer = SortCompiler.Compile(assembly, baseType, m_Orders.ToArray());
		}

		public override void Parse(Mobile from, string[] arguments, int offset, int size)
		{
			if (size < 1)
			{
				throw new Exception("Invalid ordering syntax.");
			}

			if (Insensitive.Equals(arguments[offset], "by"))
			{
				++offset;
				--size;

				if (size < 1)
				{
					throw new Exception("Invalid ordering syntax.");
				}
			}

			var end = offset + size;

			while (offset < end)
			{
				var binding = arguments[offset++];

				var isAscending = true;

				if (offset < end)
				{
					var next = arguments[offset];

					switch (next.ToLower())
					{
						case "+":
						case "up":
						case "asc":
						case "ascending":
							isAscending = true;
							++offset;
							break;

						case "-":
						case "down":
						case "desc":
						case "descending":
							isAscending = false;
							++offset;
							break;
					}
				}

				var property = new Property(binding);

				m_Orders.Add(new OrderInfo(property, isAscending));
			}
		}

		public override void Filter(ArrayList list)
		{
			if (m_Comparer == null)
			{
				throw new InvalidOperationException("The extension must first be optimized.");
			}

			list.Sort(m_Comparer);
		}
	}

	public sealed class WhereExtension : BaseExtension
	{
		public static ExtensionInfo ExtInfo = new ExtensionInfo(20, "Where", -1, delegate () { return new WhereExtension(); });

		public static void Initialize()
		{
			ExtensionInfo.Register(ExtInfo);
		}

		public override ExtensionInfo Info => ExtInfo;

		private ObjectConditional m_Conditional;

		public ObjectConditional Conditional => m_Conditional;

		public WhereExtension()
		{
		}

		public override void Optimize(Mobile from, Type baseType, ref AssemblyEmitter assembly)
		{
			if (baseType == null)
			{
				throw new InvalidOperationException("Insanity.");
			}

			m_Conditional.Compile(ref assembly);
		}

		public override void Parse(Mobile from, string[] arguments, int offset, int size)
		{
			if (size < 1)
			{
				throw new Exception("Invalid condition syntax.");
			}

			m_Conditional = ObjectConditional.ParseDirect(from, arguments, offset, size);
		}

		public override bool IsValid(object obj)
		{
			return m_Conditional.CheckCondition(obj);
		}
	}
}