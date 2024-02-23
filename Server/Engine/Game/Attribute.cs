
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Server
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class NoSortAttribute : Attribute
	{
		public NoSortAttribute()
		{
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class CallPriorityAttribute : Attribute
	{
		public long Priority { get; set; }

		public CallPriorityAttribute(long priority)
		{
			Priority = priority;
		}
	}

	public sealed class CallPriorityComparer : IComparer<MethodInfo>
	{
		public static CallPriorityComparer Instance { get; } = new();

		private static long GetPriority(MethodInfo mi)
		{
			var attr = mi.GetCustomAttribute<CallPriorityAttribute>(true);

			return attr?.Priority ?? 0L;
		}

		private CallPriorityComparer()
		{
		}

		public int Compare(MethodInfo x, MethodInfo y)
		{
			if (x == null && y == null)
			{
				return 0;
			}

			if (x == null)
			{
				return 1;
			}

			if (y == null)
			{
				return -1;
			}

			var xPriority = GetPriority(x);
			var yPriority = GetPriority(y);

			if (xPriority > yPriority)
			{
				return 1;
			}

			if (xPriority < yPriority)
			{
				return -1;
			}

			return 0;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class TypeAliasAttribute : Attribute
	{
		public string[] Aliases { get; }

		public TypeAliasAttribute(params string[] aliases)
		{
			Aliases = aliases;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ParsableAttribute : Attribute
	{
		public ParsableAttribute()
		{
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
	public class CustomEnumAttribute : Attribute
	{
		public string[] Names { get; }

		public CustomEnumAttribute(params string[] names)
		{
			Names = names ?? new[] { "None" };
		}
	}

	[AttributeUsage(AttributeTargets.Constructor)]
	public class ConstructableAttribute : Attribute
	{
		public AccessLevel AccessLevel { get; set; }

		public ConstructableAttribute() : this(AccessLevel.Player)  //Lowest accesslevel for current functionality (Level determined by access to [add)
		{
		}

		public ConstructableAttribute(AccessLevel accessLevel)
		{
			AccessLevel = accessLevel;
		}
	}

	public enum TypeFilterResult
	{
		NoFilter,
		NotFound,
		Allowed,
		Disallowed,
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class TypeFilterAttribute : Attribute
	{
		public static TypeFilterResult CheckState(PropertyInfo prop, Type type)
		{
			int index, count = 0;

			foreach (var attr in prop.GetCustomAttributes<TypeFilterAttribute>())
			{
				++count;

				index = attr.AllowedTypes.Length;

				while (--index >= 0)
				{
					if (attr.AllowedTypes[index] == type || (attr.CheckChildren && type.IsSubclassOf(attr.AllowedTypes[index])))
					{
						return TypeFilterResult.Allowed;
					}
				}

				index = attr.DisallowedTypes.Length;

				while (--index >= 0)
				{
					if (attr.DisallowedTypes[index] == type || (attr.CheckChildren && type.IsSubclassOf(attr.DisallowedTypes[index])))
					{
						return TypeFilterResult.Disallowed;
					}
				}
			}

			if (count > 0)
			{
				return TypeFilterResult.NotFound;
			}

			return TypeFilterResult.NoFilter;
		}

		public bool CheckChildren { get; }

		public Type[] AllowedTypes { get; }
		public Type[] DisallowedTypes { get; }

		public TypeFilterAttribute(bool checkChildren, bool allowed, params Type[] types)
			: this(checkChildren, allowed ? types : null, !allowed ? types : null)
		{ }

		public TypeFilterAttribute(bool checkChildren, Type[] allowedTypes, Type[] disallowedTypes)
		{
			CheckChildren = checkChildren;

			AllowedTypes = allowedTypes ?? Type.EmptyTypes;
			DisallowedTypes = disallowedTypes ?? Type.EmptyTypes;
		}
	}
}