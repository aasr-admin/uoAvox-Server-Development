﻿using Server.Network;

using System;
using System.Collections.Generic;

namespace Server
{
	public class KeywordList
	{
		private int[] m_Keywords;
		private int m_Count;

		public KeywordList()
		{
			m_Keywords = new int[8];
			m_Count = 0;
		}

		public int Count => m_Count;

		public bool Contains(int keyword)
		{
			var contains = false;

			for (var i = 0; !contains && i < m_Count; ++i)
			{
				contains = (keyword == m_Keywords[i]);
			}

			return contains;
		}

		public void Add(int keyword)
		{
			if ((m_Count + 1) > m_Keywords.Length)
			{
				var old = m_Keywords;
				m_Keywords = new int[old.Length * 2];

				for (var i = 0; i < old.Length; ++i)
				{
					m_Keywords[i] = old[i];
				}
			}

			m_Keywords[m_Count++] = keyword;
		}

		private static readonly int[] m_EmptyInts = new int[0];

		public int[] ToArray()
		{
			if (m_Count == 0)
			{
				return m_EmptyInts;
			}

			var keywords = new int[m_Count];

			for (var i = 0; i < m_Count; ++i)
			{
				keywords[i] = m_Keywords[i];
			}

			m_Count = 0;

			return keywords;
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class CommandPropertyAttribute : Attribute
	{
		private readonly AccessLevel m_ReadLevel, m_WriteLevel;
		private readonly bool m_ReadOnly;

		public AccessLevel ReadLevel => m_ReadLevel;

		public AccessLevel WriteLevel => m_WriteLevel;

		public bool ReadOnly => m_ReadOnly;

		public CommandPropertyAttribute(AccessLevel level, bool readOnly)
		{
			m_ReadLevel = level;
			m_ReadOnly = readOnly;
		}

		public CommandPropertyAttribute(AccessLevel level) : this(level, level)
		{
		}

		public CommandPropertyAttribute(AccessLevel readLevel, AccessLevel writeLevel)
		{
			m_ReadLevel = readLevel;
			m_WriteLevel = writeLevel;
		}
	}
}

namespace Server.Commands
{
	public delegate void CommandEventHandler(CommandEventArgs e);

	public class CommandEventArgs : EventArgs
	{
		private readonly Mobile m_Mobile;
		private readonly string m_Command, m_ArgString;
		private readonly string[] m_Arguments;

		public Mobile Mobile => m_Mobile;

		public string Command => m_Command;

		public string ArgString => m_ArgString;

		public string[] Arguments => m_Arguments;

		public int Length => m_Arguments.Length;

		public string GetString(int index)
		{
			if (index < 0 || index >= m_Arguments.Length)
			{
				return "";
			}

			return m_Arguments[index];
		}

		public IEntity GetEntity(int index)
		{
			return World.FindEntity(GetSerial(index));
		}

		public Item GetItem(int index)
		{
			return World.FindItem(GetSerial(index));
		}

		public Mobile GetMobile(int index)
		{
			return World.FindMobile(GetSerial(index));
		}

		public Serial GetSerial(int index)
		{
			return new Serial(GetInt32(index));
		}

		public int GetInt32(int index)
		{
			if (index < 0 || index >= m_Arguments.Length)
			{
				return 0;
			}

			return Utility.ToInt32(m_Arguments[index]);
		}

		public bool GetBoolean(int index)
		{
			if (index < 0 || index >= m_Arguments.Length)
			{
				return false;
			}

			return Utility.ToBoolean(m_Arguments[index]);
		}

		public double GetDouble(int index)
		{
			if (index < 0 || index >= m_Arguments.Length)
			{
				return 0.0;
			}

			return Utility.ToDouble(m_Arguments[index]);
		}

		public TimeSpan GetTimeSpan(int index)
		{
			if (index < 0 || index >= m_Arguments.Length)
			{
				return TimeSpan.Zero;
			}

			return Utility.ToTimeSpan(m_Arguments[index]);
		}

		public CommandEventArgs(Mobile mobile, string command, string argString, string[] arguments)
		{
			m_Mobile = mobile;
			m_Command = command;
			m_ArgString = argString;
			m_Arguments = arguments;
		}
	}

	public class CommandEntry : IComparable
	{
		private readonly string m_Command;
		private readonly CommandEventHandler m_Handler;
		private readonly AccessLevel m_AccessLevel;

		public string Command => m_Command;

		public CommandEventHandler Handler => m_Handler;

		public AccessLevel AccessLevel => m_AccessLevel;

		public CommandEntry(string command, CommandEventHandler handler, AccessLevel accessLevel)
		{
			m_Command = command;
			m_Handler = handler;
			m_AccessLevel = accessLevel;
		}

		public int CompareTo(object obj)
		{
			if (obj == this)
			{
				return 0;
			}
			else if (obj == null)
			{
				return 1;
			}

			var e = obj as CommandEntry;

			if (e == null)
			{
				throw new ArgumentException();
			}

			return m_Command.CompareTo(e.m_Command);
		}
	}

	public static class CommandSystem
	{
		private static string m_Prefix = "[";

		public static string Prefix
		{
			get => m_Prefix;
			set => m_Prefix = value;
		}

		public static string[] Split(string value)
		{
			var array = value.ToCharArray();
			var list = new List<string>();

			int start = 0, end = 0;

			while (start < array.Length)
			{
				var c = array[start];

				if (c == '"')
				{
					++start;
					end = start;

					while (end < array.Length)
					{
						if (array[end] != '"' || array[end - 1] == '\\')
						{
							++end;
						}
						else
						{
							break;
						}
					}

					list.Add(value.Substring(start, end - start));

					start = end + 2;
				}
				else if (c != ' ')
				{
					end = start;

					while (end < array.Length)
					{
						if (array[end] != ' ')
						{
							++end;
						}
						else
						{
							break;
						}
					}

					list.Add(value.Substring(start, end - start));

					start = end + 1;
				}
				else
				{
					++start;
				}
			}

			return list.ToArray();
		}

		private static readonly Dictionary<string, CommandEntry> m_Entries;

		public static Dictionary<string, CommandEntry> Entries => m_Entries;

		static CommandSystem()
		{
			m_Entries = new Dictionary<string, CommandEntry>(StringComparer.OrdinalIgnoreCase);
		}

		public static void Register(string command, AccessLevel access, CommandEventHandler handler)
		{
			m_Entries[command] = new CommandEntry(command, handler, access);
		}

		private static AccessLevel m_BadCommandIngoreLevel = AccessLevel.Player;

		public static AccessLevel BadCommandIgnoreLevel { get => m_BadCommandIngoreLevel; set => m_BadCommandIngoreLevel = value; }

		public static bool Handle(Mobile from, string text)
		{
			return Handle(from, text, MessageType.Regular);
		}

		public static bool Handle(Mobile from, string text, MessageType type)
		{
			if (text.StartsWith(m_Prefix) || type == MessageType.Command)
			{
				if (type != MessageType.Command)
				{
					text = text.Substring(m_Prefix.Length);
				}

				var indexOf = text.IndexOf(' ');

				string command;
				string[] args;
				string argString;

				if (indexOf >= 0)
				{
					argString = text.Substring(indexOf + 1);

					command = text.Substring(0, indexOf);
					args = Split(argString);
				}
				else
				{
					argString = "";
					command = text.ToLower();
					args = new string[0];
				}

				CommandEntry entry = null;
				m_Entries.TryGetValue(command, out entry);

				if (entry != null)
				{
					if (from.AccessLevel >= entry.AccessLevel)
					{
						if (entry.Handler != null)
						{
							var e = new CommandEventArgs(from, command, argString, args);
							entry.Handler(e);
							EventSink.InvokeCommand(e);
						}
					}
					else
					{
						if (from.AccessLevel <= m_BadCommandIngoreLevel)
						{
							return false;
						}

						from.SendMessage("You do not have access to that command.");
					}
				}
				else
				{
					if (from.AccessLevel <= m_BadCommandIngoreLevel)
					{
						return false;
					}

					from.SendMessage("That is not a valid command.");
				}

				return true;
			}

			return false;
		}
	}
}