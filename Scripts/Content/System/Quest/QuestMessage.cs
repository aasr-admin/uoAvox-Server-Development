using System;
using System.Collections.Generic;
using System.Drawing;

namespace Server.Quests
{
	public record QuestMessage : IComparable, IComparable<QuestMessage>
	{
		private sealed class InternalComparer : IComparer<QuestMessage>
		{
			public static InternalComparer Ascending { get; } = new(+1);
			public static InternalComparer Descending { get; } = new(-1);

			private readonly int _Modifier;

			private InternalComparer(int modifier)
			{
				_Modifier = modifier;
			}

			public int Compare(QuestMessage x, QuestMessage y)
			{
				var a = x?.Date.Ticks ?? 0;
				var b = y?.Date.Ticks ?? 0;

				return a.CompareTo(b) * _Modifier;
			}
		}

		public static IComparer<QuestMessage> AscendingComparer => InternalComparer.Ascending;
		public static IComparer<QuestMessage> DescendingComparer => InternalComparer.Descending;

		public DateTime Date { get; }
		public QuestMessageType Type { get; }
		public string Source { get; }
		public string Text { get; }

		public int TextHue { get; }
		public Color TextColor { get; }

		public QuestMessage(DateTime date, QuestMessageType type, int hue, string source, string text)
		{
			Date = date;
			Type = type;
			Source = source;
			Text = text;

			TextHue = hue;

			var data = HueData.GetHue(TextHue);

			if (data.Index > 0)
			{
				TextColor = data.Colors[^1];
			}
			else
			{
				TextColor = Color.White;
			}
		}

		public QuestMessage(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();

			Date = reader.ReadDateTime();
			Type = reader.ReadEnum<QuestMessageType>();
			Source = reader.ReadString();
			Text = reader.ReadString();
			TextHue = reader.ReadInt();
			TextColor = reader.ReadColor();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			writer.Write(Date);
			writer.Write(Type);
			writer.Write(Source);
			writer.Write(Text);
			writer.Write(TextHue);
			writer.Write(TextColor);
		}

		public override string ToString()
		{
			if (Source?.Length > 0)
			{
				return $"[{Source}]: {Text}";
			}

			return Text;
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj as QuestMessage);
		}

		public int CompareTo(QuestMessage other)
		{
			return Date.CompareTo(other?.Date);
		}
	}
}
