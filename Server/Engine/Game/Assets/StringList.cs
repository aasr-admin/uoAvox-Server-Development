
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server
{
	public class StringList
	{
		private static readonly char[] m_Tab = { '\t' };

		//C# argument support
		public static readonly Regex FormatExpression = new(@"~(\d)+_.*?~", RegexOptions.IgnoreCase);

		public static StringList Localization { get; }

		static StringList()
		{
			Localization = new StringList();
		}

		public static string MatchComparison(Match m)
		{
			return $"{{{Utility.ToInt32(m.Groups[1].Value) - 1}}}";
		}

		public static string FormatArguments(string entry)
		{
			return FormatExpression.Replace(entry, MatchComparison);
		}

		public static string CombineArguments(string str, string args)
		{
			if (!String.IsNullOrEmpty(args))
			{
				return CombineArguments(str, args.Split(m_Tab));
			}

			return str;
		}

		public static string CombineArguments(string str, params object[] args)
		{
			return String.Format(str, args);
		}

		public static string CombineArguments(int number, string args)
		{
			return CombineArguments(number, args.Split(m_Tab));
		}

		public static string CombineArguments(int number, params object[] args)
		{
			return String.Format(Localization[number], args);
		}

		public Dictionary<int, StringEntry> EntryTable { get; } = new Dictionary<int, StringEntry>();

		public Dictionary<int, string> StringTable { get; } = new Dictionary<int, string>();

		public string Language { get; private set; }

		public string this[int number] => GetString(number);

		public StringList()
			: this("enu")
		{ }

		public StringList(string language)
			: this(language, true)
		{ }

		public StringList(string language, bool format)
		{
			Language = language;

			var path = Core.FindDataFile($"Cliloc.{language}");

			if (path == null)
			{
				Console.WriteLine($"Cliloc.{language} not found");
				return;
			}

			using (var bin = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				var buffer = new byte[1024];

				bin.ReadInt32();
				bin.ReadInt16();

				while (bin.BaseStream.Length != bin.BaseStream.Position)
				{
					var number = bin.ReadInt32();

					bin.ReadByte(); // flag

					int length = bin.ReadInt16();

					if (length > buffer.Length)
					{
						buffer = new byte[(length + 1023) & ~1023];
					}

					bin.Read(buffer, 0, length);

					var text = Encoding.UTF8.GetString(buffer, 0, length);

					var se = new StringEntry(number, text);

					EntryTable[number] = se;
					StringTable[number] = text;
				}
			}
		}

		public StringEntry GetEntry(int number)
		{
			if (EntryTable.TryGetValue(number, out var entry))
			{
				return entry;
			}

			return null;
		}

		public string GetString(int number)
		{
			if (StringTable.TryGetValue(number, out var entry))
			{
				return entry;
			}

			return null;
		}
	}

	public class StringEntry
	{
		private static readonly Regex m_RegEx = new(@"~(\d+)[_\w]+~", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

		private string m_Format;
		private int m_ArgCount;

		public int Number { get; private set; }
		public string Text { get; private set; }

		public StringEntry(int number, string text)
		{
			Number = number;
			Text = text;
		}

		public override string ToString()
		{
			return Text;
		}

		public string ToString(string tsv)
		{
			if (tsv == null)
			{
				return ToString();
			}

			return ToString(tsv.Split('\t'));
		}

		public string ToString(params object[] args)
		{
			if (m_Format == null)
			{
				m_ArgCount = m_RegEx.Count(Text);
				m_Format = m_ArgCount > 0 ? m_RegEx.Replace(Text, @"{$1}") : String.Empty;
			}

			if (m_ArgCount <= 0 || args == null || args.Length == 0)
			{
				return m_Format;
			}

			if (args.Length < m_ArgCount)
			{
				Array.Resize(ref args, m_ArgCount);
			}
			else if (args.Length > m_ArgCount)
			{
				args[m_ArgCount - 1] = String.Join(' ', args[(m_ArgCount - 1)..]);
			}

			return String.Format(m_Format, args);
		}
	}
}
