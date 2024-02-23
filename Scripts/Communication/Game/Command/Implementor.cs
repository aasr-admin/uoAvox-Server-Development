using Server.Items;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Server.Commands.Generic
{
	[Flags]
	public enum CommandSupport
	{
		Single = 0x0001,
		Global = 0x0002,
		Online = 0x0004,
		Multi = 0x0008,
		Area = 0x0010,
		Self = 0x0020,
		Region = 0x0040,
		Contained = 0x0080,
		IPAddress = 0x0100,

		All = Single | Global | Online | Multi | Area | Self | Region | Contained | IPAddress,
		AllMobiles = All & ~Contained,
		AllNPCs = All & ~(IPAddress | Online | Self | Contained),
		AllItems = All & ~(IPAddress | Online | Self | Region),

		Simple = Single | Multi,
		Complex = Global | Online | Area | Region | Contained | IPAddress
	}

	public abstract class BaseCommandImplementor
	{
		public static void RegisterImplementors()
		{
			Register(new RegionCommandImplementor());
			Register(new GlobalCommandImplementor());
			Register(new OnlineCommandImplementor());
			Register(new SingleCommandImplementor());
			Register(new SerialCommandImplementor());
			Register(new MultiCommandImplementor());
			Register(new AreaCommandImplementor());
			Register(new SelfCommandImplementor());
			Register(new ContainedCommandImplementor());
			Register(new IPAddressCommandImplementor());

			Register(new RangeCommandImplementor());
			Register(new ScreenCommandImplementor());
			Register(new FacetCommandImplementor());
		}

		private string[] m_Accessors;
		private AccessLevel m_AccessLevel;
		private CommandSupport m_SupportRequirement;
		private readonly Dictionary<string, BaseCommand> m_Commands;
		private string m_Usage;
		private string m_Description;
		private bool m_SupportsConditionals;

		public bool SupportsConditionals
		{
			get => m_SupportsConditionals;
			set => m_SupportsConditionals = value;
		}

		public string[] Accessors
		{
			get => m_Accessors;
			set => m_Accessors = value;
		}

		public string Usage
		{
			get => m_Usage;
			set => m_Usage = value;
		}

		public string Description
		{
			get => m_Description;
			set => m_Description = value;
		}

		public AccessLevel AccessLevel
		{
			get => m_AccessLevel;
			set => m_AccessLevel = value;
		}

		public CommandSupport SupportRequirement
		{
			get => m_SupportRequirement;
			set => m_SupportRequirement = value;
		}

		public Dictionary<string, BaseCommand> Commands => m_Commands;

		public BaseCommandImplementor()
		{
			m_Commands = new Dictionary<string, BaseCommand>(StringComparer.OrdinalIgnoreCase);
		}

		public virtual void Compile(Mobile from, BaseCommand command, ref string[] args, ref object obj)
		{
			obj = null;
		}

		public virtual void Register(BaseCommand command)
		{
			for (var i = 0; i < command.Commands.Length; ++i)
			{
				m_Commands[command.Commands[i]] = command;
			}
		}

		public bool CheckObjectTypes(Mobile from, BaseCommand command, Extensions ext, out bool items, out bool mobiles)
		{
			items = mobiles = false;

			var cond = ObjectConditional.Empty;

			foreach (var check in ext)
			{
				if (check is WhereExtension)
				{
					cond = (check as WhereExtension).Conditional;

					break;
				}
			}

			var condIsItem = cond.IsItem;
			var condIsMobile = cond.IsMobile;

			switch (command.ObjectTypes)
			{
				case ObjectTypes.All:
				case ObjectTypes.Both:
					{
						if (condIsItem)
						{
							items = true;
						}

						if (condIsMobile)
						{
							mobiles = true;
						}

						break;
					}
				case ObjectTypes.Items:
					{
						if (condIsItem)
						{
							items = true;
						}
						else if (condIsMobile)
						{
							from.SendMessage("You may not use a mobile type condition for this command.");
							return false;
						}

						break;
					}
				case ObjectTypes.Mobiles:
					{
						if (condIsMobile)
						{
							mobiles = true;
						}
						else if (condIsItem)
						{
							from.SendMessage("You may not use an item type condition for this command.");
							return false;
						}

						break;
					}
			}

			return true;
		}

		public void RunCommand(Mobile from, BaseCommand command, string[] args)
		{
			try
			{
				object obj = null;

				Compile(from, command, ref args, ref obj);

				RunCommand(from, obj, command, args);
			}
			catch (Exception ex)
			{
				from.SendMessage(ex.Message);
			}
		}

		public string GenerateArgString(string[] args)
		{
			if (args.Length == 0)
			{
				return "";
			}

			// NOTE: this does not preserve the case where quotation marks are used on a single word

			var sb = new StringBuilder();

			for (var i = 0; i < args.Length; ++i)
			{
				if (i > 0)
				{
					sb.Append(' ');
				}

				if (args[i].IndexOf(' ') >= 0)
				{
					sb.Append('"');
					sb.Append(args[i]);
					sb.Append('"');
				}
				else
				{
					sb.Append(args[i]);
				}
			}

			return sb.ToString();
		}

		public void RunCommand(Mobile from, object obj, BaseCommand command, string[] args)
		{
			//	try
			//	{
			var e = new CommandEventArgs(from, command.Commands[0], GenerateArgString(args), args);

			if (!command.ValidateArgs(this, e))
			{
				return;
			}

			var flushToLog = false;

			if (obj is ArrayList)
			{
				var list = (ArrayList)obj;

				if (list.Count > 20)
				{
					CommandLogging.Enabled = false;
				}
				else if (list.Count == 0)
				{
					command.LogFailure("Nothing was found to use this command on.");
				}

				command.ExecuteList(e, list);

				if (list.Count > 20)
				{
					flushToLog = true;
					CommandLogging.Enabled = true;
				}
			}
			else if (obj != null)
			{
				if (command.ListOptimized)
				{
					var list = new ArrayList {
						obj
					};
					command.ExecuteList(e, list);
				}
				else
				{
					command.Execute(e, obj);
				}
			}

			command.Flush(from, flushToLog);
			//	}
			//	catch ( Exception ex )
			//	{
			//		from.SendMessage( ex.Message );
			//	}
		}

		public virtual void Process(Mobile from, BaseCommand command, string[] args)
		{
			RunCommand(from, command, args);
		}

		public virtual void Execute(CommandEventArgs e)
		{
			if (e.Length >= 1)
			{
				BaseCommand command = null;
				m_Commands.TryGetValue(e.GetString(0), out command);

				if (command == null)
				{
					e.Mobile.SendMessage("That is either an invalid command name or one that does not support this modifier.");
				}
				else if (e.Mobile.AccessLevel < command.AccessLevel)
				{
					e.Mobile.SendMessage("You do not have access to that command.");
				}
				else
				{
					var oldArgs = e.Arguments;
					var args = new string[oldArgs.Length - 1];

					for (var i = 0; i < args.Length; ++i)
					{
						args[i] = oldArgs[i + 1];
					}

					Process(e.Mobile, command, args);
				}
			}
			else
			{
				e.Mobile.SendMessage("You must supply a command name.");
			}
		}

		public void Register()
		{
			if (m_Accessors == null)
			{
				return;
			}

			for (var i = 0; i < m_Accessors.Length; ++i)
			{
				CommandSystem.Register(m_Accessors[i], m_AccessLevel, new CommandEventHandler(Execute));
			}
		}

		public static void Register(BaseCommandImplementor impl)
		{
			m_Implementors.Add(impl);
			impl.Register();
		}

		private static List<BaseCommandImplementor> m_Implementors;

		public static List<BaseCommandImplementor> Implementors
		{
			get
			{
				if (m_Implementors == null)
				{
					m_Implementors = new List<BaseCommandImplementor>();
					RegisterImplementors();
				}

				return m_Implementors;
			}
		}
	}

	public class AreaCommandImplementor : BaseCommandImplementor
	{
		private static AreaCommandImplementor m_Instance;

		public static AreaCommandImplementor Instance => m_Instance;

		public AreaCommandImplementor()
		{
			Accessors = new string[] { "Area", "Group" };
			SupportRequirement = CommandSupport.Area;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Area <command> [condition]";
			Description = "Invokes the command on all appropriate objects in a targeted area. Optional condition arguments can further restrict the set of objects.";

			m_Instance = this;
		}

		public override void Process(Mobile from, BaseCommand command, string[] args)
		{
			BoundingBoxPicker.Begin(from, new BoundingBoxCallback(OnTarget), new object[] { command, args });
		}

		public void OnTarget(Mobile from, Map map, Point3D start, Point3D end, object state)
		{
			try
			{
				var states = (object[])state;
				var command = (BaseCommand)states[0];
				var args = (string[])states[1];

				var rect = new Rectangle2D(start.X, start.Y, end.X - start.X + 1, end.Y - start.Y + 1);

				var ext = Extensions.Parse(from, ref args);

				bool items, mobiles;

				if (!CheckObjectTypes(from, command, ext, out items, out mobiles))
				{
					return;
				}

				IPooledEnumerable eable;

				if (items && mobiles)
				{
					eable = map.GetObjectsInBounds(rect);
				}
				else if (items)
				{
					eable = map.GetItemsInBounds(rect);
				}
				else if (mobiles)
				{
					eable = map.GetMobilesInBounds(rect);
				}
				else
				{
					return;
				}

				var objs = new ArrayList();

				foreach (var obj in eable)
				{
					if (mobiles && obj is Mobile && !BaseCommand.IsAccessible(from, obj))
					{
						continue;
					}

					if (ext.IsValid(obj))
					{
						objs.Add(obj);
					}
				}

				eable.Free();

				ext.Filter(objs);

				RunCommand(from, objs, command, args);
			}
			catch (Exception ex)
			{
				from.SendMessage(ex.Message);
			}
		}
	}

	public class ContainedCommandImplementor : BaseCommandImplementor
	{
		public ContainedCommandImplementor()
		{
			Accessors = new string[] { "Contained" };
			SupportRequirement = CommandSupport.Contained;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Contained <command> [condition]";
			Description = "Invokes the command on all child items in a targeted container. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Process(Mobile from, BaseCommand command, string[] args)
		{
			if (command.ValidateArgs(this, new CommandEventArgs(from, command.Commands[0], GenerateArgString(args), args)))
			{
				from.BeginTarget(-1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback(OnTarget), new object[] { command, args });
			}
		}

		public void OnTarget(Mobile from, object targeted, object state)
		{
			if (!BaseCommand.IsAccessible(from, targeted))
			{
				from.SendMessage("That is not accessible.");
				return;
			}

			var states = (object[])state;
			var command = (BaseCommand)states[0];
			var args = (string[])states[1];

			if (command.ObjectTypes == ObjectTypes.Mobiles)
			{
				return; // sanity check
			}

			if (!(targeted is Container))
			{
				from.SendMessage("That is not a container.");
			}
			else
			{
				try
				{
					var ext = Extensions.Parse(from, ref args);

					bool items, mobiles;

					if (!CheckObjectTypes(from, command, ext, out items, out mobiles))
					{
						return;
					}

					if (!items)
					{
						from.SendMessage("This command only works on items.");
						return;
					}

					var cont = (Container)targeted;

					var found = cont.FindItemsByType(typeof(Item), true);

					var list = new ArrayList();

					for (var i = 0; i < found.Length; ++i)
					{
						if (ext.IsValid(found[i]))
						{
							list.Add(found[i]);
						}
					}

					ext.Filter(list);

					RunCommand(from, list, command, args);
				}
				catch (Exception e)
				{
					from.SendMessage(e.Message);
				}
			}
		}
	}

	public class FacetCommandImplementor : BaseCommandImplementor
	{
		public FacetCommandImplementor()
		{
			Accessors = new string[] { "Facet" };
			SupportRequirement = CommandSupport.Area;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Facet <command> [condition]";
			Description = "Invokes the command on all appropriate objects within your facet's map bounds. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Process(Mobile from, BaseCommand command, string[] args)
		{
			var impl = AreaCommandImplementor.Instance;

			if (impl == null)
			{
				return;
			}

			var map = from.Map;

			if (map == null || map == Map.Internal)
			{
				return;
			}

			impl.OnTarget(from, map, Point3D.Zero, new Point3D(map.Width - 1, map.Height - 1, 0), new object[] { command, args });
		}
	}

	public class GlobalCommandImplementor : BaseCommandImplementor
	{
		public GlobalCommandImplementor()
		{
			Accessors = new string[] { "Global" };
			SupportRequirement = CommandSupport.Global;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.Administrator;
			Usage = "Global <command> [condition]";
			Description = "Invokes the command on all appropriate objects in the world. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Compile(Mobile from, BaseCommand command, ref string[] args, ref object obj)
		{
			try
			{
				var ext = Extensions.Parse(from, ref args);

				bool items, mobiles;

				if (!CheckObjectTypes(from, command, ext, out items, out mobiles))
				{
					return;
				}

				var list = new ArrayList();

				if (items)
				{
					foreach (var item in World.Items.Values)
					{
						if (ext.IsValid(item))
						{
							list.Add(item);
						}
					}
				}

				if (mobiles)
				{
					foreach (var mob in World.Mobiles.Values)
					{
						if (ext.IsValid(mob))
						{
							list.Add(mob);
						}
					}
				}

				ext.Filter(list);

				obj = list;
			}
			catch (Exception ex)
			{
				from.SendMessage(ex.Message);
			}
		}
	}

	public class IPAddressCommandImplementor : BaseCommandImplementor
	{
		public IPAddressCommandImplementor()
		{
			Accessors = new string[] { "IPAddress" };
			SupportRequirement = CommandSupport.IPAddress;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.Administrator;
			Usage = "IPAddress <command> [condition]";
			Description = "Invokes the command on one mobile from each IP address that is logged in. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Compile(Mobile from, BaseCommand command, ref string[] args, ref object obj)
		{
			try
			{
				var ext = Extensions.Parse(from, ref args);

				bool items, mobiles;

				if (!CheckObjectTypes(from, command, ext, out items, out mobiles))
				{
					return;
				}

				if (!mobiles) // sanity check
				{
					command.LogFailure("This command does not support items.");
					return;
				}

				var list = new ArrayList();
				var addresses = new ArrayList();

				var states = NetState.Instances;

				for (var i = 0; i < states.Count; ++i)
				{
					var ns = states[i];
					var mob = ns.Mobile;

					if (mob != null && !addresses.Contains(ns.Address) && ext.IsValid(mob))
					{
						list.Add(mob);
						addresses.Add(ns.Address);
					}
				}

				ext.Filter(list);

				obj = list;
			}
			catch (Exception ex)
			{
				from.SendMessage(ex.Message);
			}
		}
	}

	public class MultiCommandImplementor : BaseCommandImplementor
	{
		public MultiCommandImplementor()
		{
			Accessors = new string[] { "Multi", "m" };
			SupportRequirement = CommandSupport.Multi;
			AccessLevel = AccessLevel.Counselor;
			Usage = "Multi <command>";
			Description = "Invokes the command on multiple targeted objects.";
		}

		public override void Process(Mobile from, BaseCommand command, string[] args)
		{
			if (command.ValidateArgs(this, new CommandEventArgs(from, command.Commands[0], GenerateArgString(args), args)))
			{
				from.BeginTarget(-1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback(OnTarget), new object[] { command, args });
			}
		}

		public void OnTarget(Mobile from, object targeted, object state)
		{
			var states = (object[])state;
			var command = (BaseCommand)states[0];
			var args = (string[])states[1];

			if (!BaseCommand.IsAccessible(from, targeted))
			{
				from.SendMessage("That is not accessible.");
				from.BeginTarget(-1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback(OnTarget), new object[] { command, args });
				return;
			}

			switch (command.ObjectTypes)
			{
				case ObjectTypes.Both:
					{
						if (!(targeted is Item) && !(targeted is Mobile))
						{
							from.SendMessage("This command does not work on that.");
							return;
						}

						break;
					}
				case ObjectTypes.Items:
					{
						if (!(targeted is Item))
						{
							from.SendMessage("This command only works on items.");
							return;
						}

						break;
					}
				case ObjectTypes.Mobiles:
					{
						if (!(targeted is Mobile))
						{
							from.SendMessage("This command only works on mobiles.");
							return;
						}

						break;
					}
			}

			RunCommand(from, targeted, command, args);

			from.BeginTarget(-1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback(OnTarget), new object[] { command, args });
		}
	}

	public sealed class ObjectConditional
	{
		private static readonly Type typeofItem = typeof(Item);
		private static readonly Type typeofMobile = typeof(Mobile);

		private readonly Type m_ObjectType;

		private readonly ICondition[][] m_Conditions;

		private IConditional[] m_Conditionals;

		public Type Type => m_ObjectType;

		public bool IsItem => (m_ObjectType == null || m_ObjectType == typeofItem || m_ObjectType.IsSubclassOf(typeofItem));

		public bool IsMobile => (m_ObjectType == null || m_ObjectType == typeofMobile || m_ObjectType.IsSubclassOf(typeofMobile));

		public static readonly ObjectConditional Empty = new ObjectConditional(null, null);

		public bool HasCompiled => (m_Conditionals != null);

		public void Compile(ref AssemblyEmitter emitter)
		{
			if (emitter == null)
			{
				emitter = new AssemblyEmitter("__dynamic");
			}

			m_Conditionals = new IConditional[m_Conditions.Length];

			for (var i = 0; i < m_Conditionals.Length; ++i)
			{
				m_Conditionals[i] = ConditionalCompiler.Compile(emitter, m_ObjectType, m_Conditions[i], i);
			}
		}

		public bool CheckCondition(object obj)
		{
			if (m_ObjectType == null)
			{
				return true; // null type means no condition
			}

			if (!HasCompiled)
			{
				AssemblyEmitter emitter = null;

				Compile(ref emitter);
			}

			for (var i = 0; i < m_Conditionals.Length; ++i)
			{
				if (m_Conditionals[i].Verify(obj))
				{
					return true;
				}
			}

			return false; // all conditions false
		}

		public static ObjectConditional Parse(Mobile from, ref string[] args)
		{
			string[] conditionArgs = null;

			for (var i = 0; i < args.Length; ++i)
			{
				if (Insensitive.Equals(args[i], "where"))
				{
					var origArgs = args;

					args = new string[i];

					for (var j = 0; j < args.Length; ++j)
					{
						args[j] = origArgs[j];
					}

					conditionArgs = new string[origArgs.Length - i - 1];

					for (var j = 0; j < conditionArgs.Length; ++j)
					{
						conditionArgs[j] = origArgs[i + j + 1];
					}

					break;
				}
			}

			return ParseDirect(from, conditionArgs, 0, conditionArgs.Length);
		}

		public static ObjectConditional ParseDirect(Mobile from, string[] args, int offset, int size)
		{
			if (args == null || size == 0)
			{
				return ObjectConditional.Empty;
			}

			var index = 0;

			var objectType = ScriptCompiler.FindTypeByName(args[offset + index], true);

			if (objectType == null)
			{
				throw new Exception(String.Format("No type with that name ({0}) was found.", args[offset + index]));
			}

			++index;

			var conditions = new List<ICondition[]>();
			var current = new List<ICondition> {
				TypeCondition.Default
			};

			while (index < size)
			{
				var cur = args[offset + index];

				var inverse = false;

				if (Insensitive.Equals(cur, "not") || cur == "!")
				{
					inverse = true;
					++index;

					if (index >= size)
					{
						throw new Exception("Improperly formatted object conditional.");
					}
				}
				else if (Insensitive.Equals(cur, "or") || cur == "||")
				{
					if (current.Count > 1)
					{
						conditions.Add(current.ToArray());

						current.Clear();
						current.Add(TypeCondition.Default);
					}

					++index;

					continue;
				}

				var binding = args[offset + index];
				index++;

				if (index >= size)
				{
					throw new Exception("Improperly formatted object conditional.");
				}

				var oper = args[offset + index];
				index++;

				if (index >= size)
				{
					throw new Exception("Improperly formatted object conditional.");
				}

				var val = args[offset + index];
				index++;

				var prop = new Property(binding);

				prop.BindTo(objectType, PropertyAccess.Read);
				prop.CheckAccess(from);

				ICondition condition = null;

				switch (oper)
				{
					#region Equality
					case "=":
					case "==":
					case "is":
						condition = new ComparisonCondition(prop, inverse, ComparisonOperator.Equal, val);
						break;

					case "!=":
						condition = new ComparisonCondition(prop, inverse, ComparisonOperator.NotEqual, val);
						break;
					#endregion

					#region Relational
					case ">":
						condition = new ComparisonCondition(prop, inverse, ComparisonOperator.Greater, val);
						break;

					case "<":
						condition = new ComparisonCondition(prop, inverse, ComparisonOperator.Lesser, val);
						break;

					case ">=":
						condition = new ComparisonCondition(prop, inverse, ComparisonOperator.GreaterEqual, val);
						break;

					case "<=":
						condition = new ComparisonCondition(prop, inverse, ComparisonOperator.LesserEqual, val);
						break;
					#endregion

					#region Strings
					case "==~":
					case "~==":
					case "=~":
					case "~=":
					case "is~":
					case "~is":
						condition = new StringCondition(prop, inverse, StringOperator.Equal, val, true);
						break;

					case "!=~":
					case "~!=":
						condition = new StringCondition(prop, inverse, StringOperator.NotEqual, val, true);
						break;

					case "starts":
						condition = new StringCondition(prop, inverse, StringOperator.StartsWith, val, false);
						break;

					case "starts~":
					case "~starts":
						condition = new StringCondition(prop, inverse, StringOperator.StartsWith, val, true);
						break;

					case "ends":
						condition = new StringCondition(prop, inverse, StringOperator.EndsWith, val, false);
						break;

					case "ends~":
					case "~ends":
						condition = new StringCondition(prop, inverse, StringOperator.EndsWith, val, true);
						break;

					case "contains":
						condition = new StringCondition(prop, inverse, StringOperator.Contains, val, false);
						break;

					case "contains~":
					case "~contains":
						condition = new StringCondition(prop, inverse, StringOperator.Contains, val, true);
						break;
						#endregion
				}

				if (condition == null)
				{
					throw new InvalidOperationException(String.Format("Unrecognized operator (\"{0}\").", oper));
				}

				current.Add(condition);
			}

			conditions.Add(current.ToArray());

			return new ObjectConditional(objectType, conditions.ToArray());
		}

		public ObjectConditional(Type objectType, ICondition[][] conditions)
		{
			m_ObjectType = objectType;
			m_Conditions = conditions;
		}
	}

	public class OnlineCommandImplementor : BaseCommandImplementor
	{
		public OnlineCommandImplementor()
		{
			Accessors = new string[] { "Online" };
			SupportRequirement = CommandSupport.Online;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Online <command> [condition]";
			Description = "Invokes the command on all mobiles that are currently logged in. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Compile(Mobile from, BaseCommand command, ref string[] args, ref object obj)
		{
			try
			{
				var ext = Extensions.Parse(from, ref args);

				bool items, mobiles;

				if (!CheckObjectTypes(from, command, ext, out items, out mobiles))
				{
					return;
				}

				if (!mobiles) // sanity check
				{
					command.LogFailure("This command does not support items.");
					return;
				}

				var list = new ArrayList();

				var states = NetState.Instances;

				for (var i = 0; i < states.Count; ++i)
				{
					var ns = states[i];
					var mob = ns.Mobile;

					if (mob == null)
					{
						continue;
					}

					if (!BaseCommand.IsAccessible(from, mob))
					{
						continue;
					}

					if (ext.IsValid(mob))
					{
						list.Add(mob);
					}
				}

				ext.Filter(list);

				obj = list;
			}
			catch (Exception ex)
			{
				from.SendMessage(ex.Message);
			}
		}
	}

	public class RangeCommandImplementor : BaseCommandImplementor
	{
		private static RangeCommandImplementor m_Instance;

		public static RangeCommandImplementor Instance => m_Instance;

		public RangeCommandImplementor()
		{
			Accessors = new string[] { "Range" };
			SupportRequirement = CommandSupport.Area;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Range <range> <command> [condition]";
			Description = "Invokes the command on all appropriate objects within a specified range of you. Optional condition arguments can further restrict the set of objects.";

			m_Instance = this;
		}

		public override void Execute(CommandEventArgs e)
		{
			if (e.Length >= 2)
			{
				var range = e.GetInt32(0);

				if (range < 0)
				{
					e.Mobile.SendMessage("The range must not be negative.");
				}
				else
				{
					BaseCommand command = null;
					Commands.TryGetValue(e.GetString(1), out command);

					if (command == null)
					{
						e.Mobile.SendMessage("That is either an invalid command name or one that does not support this modifier.");
					}
					else if (e.Mobile.AccessLevel < command.AccessLevel)
					{
						e.Mobile.SendMessage("You do not have access to that command.");
					}
					else
					{
						var oldArgs = e.Arguments;
						var args = new string[oldArgs.Length - 2];

						for (var i = 0; i < args.Length; ++i)
						{
							args[i] = oldArgs[i + 2];
						}

						Process(range, e.Mobile, command, args);
					}
				}
			}
			else
			{
				e.Mobile.SendMessage("You must supply a range and a command name.");
			}
		}

		public void Process(int range, Mobile from, BaseCommand command, string[] args)
		{
			var impl = AreaCommandImplementor.Instance;

			if (impl == null)
			{
				return;
			}

			var map = from.Map;

			if (map == null || map == Map.Internal)
			{
				return;
			}

			var start = new Point3D(from.X - range, from.Y - range, from.Z);
			var end = new Point3D(from.X + range, from.Y + range, from.Z);

			impl.OnTarget(from, map, start, end, new object[] { command, args });
		}
	}

	public class RegionCommandImplementor : BaseCommandImplementor
	{
		public RegionCommandImplementor()
		{
			Accessors = new string[] { "Region" };
			SupportRequirement = CommandSupport.Region;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Region <command> [condition]";
			Description = "Invokes the command on all appropriate mobiles in your current region. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Compile(Mobile from, BaseCommand command, ref string[] args, ref object obj)
		{
			try
			{
				var ext = Extensions.Parse(from, ref args);

				bool items, mobiles;

				if (!CheckObjectTypes(from, command, ext, out items, out mobiles))
				{
					return;
				}

				var reg = from.Region;

				var list = new ArrayList();

				if (mobiles)
				{
					foreach (var mob in reg.GetMobiles())
					{
						if (!BaseCommand.IsAccessible(from, mob))
						{
							continue;
						}

						if (ext.IsValid(mob))
						{
							list.Add(mob);
						}
					}
				}
				else
				{
					command.LogFailure("This command does not support items.");
					return;
				}

				ext.Filter(list);

				obj = list;
			}
			catch (Exception ex)
			{
				from.SendMessage(ex.Message);
			}
		}
	}

	public class ScreenCommandImplementor : BaseCommandImplementor
	{
		public ScreenCommandImplementor()
		{
			Accessors = new string[] { "Screen" };
			SupportRequirement = CommandSupport.Area;
			SupportsConditionals = true;
			AccessLevel = AccessLevel.GameMaster;
			Usage = "Screen <command> [condition]";
			Description = "Invokes the command on all appropriate objects in your screen. Optional condition arguments can further restrict the set of objects.";
		}

		public override void Process(Mobile from, BaseCommand command, string[] args)
		{
			var impl = RangeCommandImplementor.Instance;

			if (impl == null)
			{
				return;
			}

			impl.Process(18, from, command, args);
		}
	}

	public class SelfCommandImplementor : BaseCommandImplementor
	{
		public SelfCommandImplementor()
		{
			Accessors = new string[] { "Self" };
			SupportRequirement = CommandSupport.Self;
			AccessLevel = AccessLevel.Counselor;
			Usage = "Self <command>";
			Description = "Invokes the command on the commanding player.";
		}

		public override void Compile(Mobile from, BaseCommand command, ref string[] args, ref object obj)
		{
			if (command.ObjectTypes == ObjectTypes.Items)
			{
				return; // sanity check
			}

			obj = from;
		}
	}

	public class SerialCommandImplementor : BaseCommandImplementor
	{
		public SerialCommandImplementor()
		{
			Accessors = new string[] { "Serial" };
			SupportRequirement = CommandSupport.Single;
			AccessLevel = AccessLevel.Counselor;
			Usage = "Serial <serial> <command>";
			Description = "Invokes the command on a single object by serial.";
		}

		public override void Execute(CommandEventArgs e)
		{
			if (e.Length >= 2)
			{
				Serial serial = e.GetSerial(0);

				object obj = null;

				if (serial.IsItem)
				{
					obj = World.FindItem(serial);
				}
				else if (serial.IsMobile)
				{
					obj = World.FindMobile(serial);
				}

				if (obj == null)
				{
					e.Mobile.SendMessage("That is not a valid serial.");
				}
				else
				{
					BaseCommand command = null;
					Commands.TryGetValue(e.GetString(1), out command);

					if (command == null)
					{
						e.Mobile.SendMessage("That is either an invalid command name or one that does not support this modifier.");
					}
					else if (e.Mobile.AccessLevel < command.AccessLevel)
					{
						e.Mobile.SendMessage("You do not have access to that command.");
					}
					else
					{
						switch (command.ObjectTypes)
						{
							case ObjectTypes.Both:
								{
									if (!(obj is Item) && !(obj is Mobile))
									{
										e.Mobile.SendMessage("This command does not work on that.");
										return;
									}

									break;
								}
							case ObjectTypes.Items:
								{
									if (!(obj is Item))
									{
										e.Mobile.SendMessage("This command only works on items.");
										return;
									}

									break;
								}
							case ObjectTypes.Mobiles:
								{
									if (!(obj is Mobile))
									{
										e.Mobile.SendMessage("This command only works on mobiles.");
										return;
									}

									break;
								}
						}

						var oldArgs = e.Arguments;
						var args = new string[oldArgs.Length - 2];

						for (var i = 0; i < args.Length; ++i)
						{
							args[i] = oldArgs[i + 2];
						}

						RunCommand(e.Mobile, obj, command, args);
					}
				}
			}
			else
			{
				e.Mobile.SendMessage("You must supply an object serial and a command name.");
			}
		}
	}

	public class SingleCommandImplementor : BaseCommandImplementor
	{
		public SingleCommandImplementor()
		{
			Accessors = new string[] { "Single" };
			SupportRequirement = CommandSupport.Single;
			AccessLevel = AccessLevel.Counselor;
			Usage = "Single <command>";
			Description = "Invokes the command on a single targeted object. This is the same as just invoking the command directly.";
		}

		public override void Register(BaseCommand command)
		{
			base.Register(command);

			for (var i = 0; i < command.Commands.Length; ++i)
			{
				CommandSystem.Register(command.Commands[i], command.AccessLevel, new CommandEventHandler(Redirect));
			}
		}

		public void Redirect(CommandEventArgs e)
		{
			BaseCommand command = null;

			Commands.TryGetValue(e.Command, out command);

			if (command == null)
			{
				e.Mobile.SendMessage("That is either an invalid command name or one that does not support this modifier.");
			}
			else if (e.Mobile.AccessLevel < command.AccessLevel)
			{
				e.Mobile.SendMessage("You do not have access to that command.");
			}
			else if (command.ValidateArgs(this, e))
			{
				Process(e.Mobile, command, e.Arguments);
			}
		}

		public override void Process(Mobile from, BaseCommand command, string[] args)
		{
			if (command.ValidateArgs(this, new CommandEventArgs(from, command.Commands[0], GenerateArgString(args), args)))
			{
				from.BeginTarget(-1, command.ObjectTypes == ObjectTypes.All, TargetFlags.None, new TargetStateCallback(OnTarget), new object[] { command, args });
			}
		}

		public void OnTarget(Mobile from, object targeted, object state)
		{
			if (!BaseCommand.IsAccessible(from, targeted))
			{
				from.SendMessage("That is not accessible.");
				return;
			}

			var states = (object[])state;
			var command = (BaseCommand)states[0];
			var args = (string[])states[1];

			switch (command.ObjectTypes)
			{
				case ObjectTypes.Both:
					{
						if (!(targeted is Item) && !(targeted is Mobile))
						{
							from.SendMessage("This command does not work on that.");
							return;
						}

						break;
					}
				case ObjectTypes.Items:
					{
						if (!(targeted is Item))
						{
							from.SendMessage("This command only works on items.");
							return;
						}

						break;
					}
				case ObjectTypes.Mobiles:
					{
						if (!(targeted is Mobile))
						{
							from.SendMessage("This command only works on mobiles.");
							return;
						}

						break;
					}
			}

			RunCommand(from, targeted, command, args);
		}
	}
}