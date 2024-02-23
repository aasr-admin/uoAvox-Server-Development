using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Server.Engines.Facet;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Quests
{
	public sealed class QuestLogUI : BaseGump
	{
		// These values are based on gump art borders
		// These values shouldn't be changed unless it is to compensate for using different background art
		// The currently used background has a 4 pixel hard border (the corner piece is 4 x 4 pixels)
		// The row height is based on the height of the image used for the header backgrounds
		private const int PADDING = 4, PADDING2 = PADDING * 2, ROW_HEIGHT = 22 + PADDING2;

		// These values are based on gump art borders
		// These values shouldn't be changed unless it is to compensate for using different background art
		// The currently used background has an 11 pixel hard border (the corner piece is 50 x 50 pixels)
		private const int COL_PADDING = 11, COL_PADDING2 = COL_PADDING * 2;

		// These values are arbitrary and can be changed any time to increase the area of the overall layout
		// Everything within the layout will automatically expand or contract to fit the constraints
		// Making the height taller will allow more rows to be displayed on a single page
		// Padding is applied to the external edges of the main layout;
		// The total width is COL_PADDING + COL1_WIDTH + COL_PADDING2 + COL2_WIDTH + COL_PADDING
		// The total height is COL_PADDING + COL_HEIGHT + COL_PADDING
		// The default internal size for the layout is 800 x 600 with the width split between the columns
		// The left column must be able to display the facet image (383 x 383)
		// The right column must be able to display at least 4 rows (4 x ROW_HEIGHT)
		private const int COL1_WIDTH = 500, COL2_WIDTH = 300, COL_HEIGHT = 600;

		public static QuestLogUI DisplayTo(PlayerMobile player, Quest quest = null)
		{
			var ui = player.FindGump<QuestLogUI>();

			if (ui != null)
			{
				ui.Quest = quest;

				ui.Refresh();
			}
			else
			{
				ui = new QuestLogUI(player, quest);

				ui.SendGump();
			}

			return ui;
		}

		public Quest Quest { get; private set; }

		public List<Quest> States { get; } = new();

		public QuestLogUI(PlayerMobile user, Quest quest = null)
			: base(user)
		{
			Quest = quest;
		}

		private void Validate()
		{
			if (Quest != null && (Quest.Owner != User || Quest.Deleted || Quest.IsAbandoned))
			{
				Quest = null;
			}

			States.Clear();
			States.AddRange(QuestRegistry.States.Find(User, null, q => q.IsActive || q.IsCompleted));
		}

		public override void SendGump()
		{
			Validate();

			base.SendGump();
		}

		public override void OnBeforeRefresh()
		{
			Validate();

			base.OnBeforeRefresh();
		}

		public override void AddGumpLayout()
		{
			var pad = COL_PADDING;
			var pad2 = COL_PADDING2;

			var col1w = COL1_WIDTH + COL_PADDING2;
			var col1h = COL_HEIGHT + COL_PADDING2;

			AddBackground(0, 0, col1w, col1h, 1460);
			AddImageTiled(pad, pad, col1w - pad2, col1h - pad2, 2624);
			RenderColumn1(pad, pad, col1w - pad2, col1h - pad2);

			if (Quest == null || (!Quest.IsPending && !Quest.IsCompleted))
			{
				var col2w = COL2_WIDTH + COL_PADDING2;
				var col2h = COL_HEIGHT + COL_PADDING2;

				AddBackground(col1w - (pad + 2), 0, col2w + 2, col2h, 1460);
				AddImageTiled(col1w - 2, pad, col2w - pad2 + 2, col2h - pad2, 2624);
				RenderColumn2(col1w - 2, pad, col2w - pad2 + 2, col2h - pad2);
			}
		}

		private void AddHeader(int x, int y, int w, int indent, bool highlight, Action clicked = null)
		{
			var qyo = (ROW_HEIGHT - 25) / 2;

			if (clicked != null)
			{
				AddButton(x + (w - 126), y + qyo, highlight ? 1589 : 1588, highlight ? 1589 : 1588, clicked);
			}

			AddImage(x + (w - 126), y + qyo, highlight ? 1589 : 1588);

			var bgw = w - indent;

			if (clicked != null)
			{
				while (bgw > 0)
				{
					var iw = Math.Min(bgw, 126);

					AddButton(x + indent + (bgw - iw), y + qyo, highlight ? 1589 : 1588, highlight ? 1589 : 1588, clicked);

					bgw -= iw;
				}

				bgw = w - indent;
			}

			var first = true;

			while (bgw > 0)
			{
				var iw = Math.Min(bgw, 100);

				AddImageTiled(x + indent + (bgw - (iw + (first ? 26 : 0))), y + qyo, iw + (first ? 26 : 16), 25, highlight ? 1589 : 1588);

				bgw -= iw;

				first = false;
			}
		}

		private void RenderColumn1(int x, int y, int w, int h)
		{
			if (Quest != null)
			{
				var info = new StringBuilder();

				h -= ROW_HEIGHT;

				var h2 = h / 2;

				RenderQuestDetails(Quest, x, y, w, h2, true, info);

				y += h2;
				h -= h2;

				AddBackground(x, y, w - 16, h, 1579);

				foreach (var message in Quest.Messages)
				{
					var date = SetColor(Color.Maroon, Utility.FormatDate(message.Date, message.Date.Kind == DateTimeKind.Utc, false, true, false));

					info.AppendLine($"{date}\u250A {SetColor(message.TextColor, $"{message}")}");
				}

				AddHtml(x + PADDING2, y + PADDING, w - (PADDING2 + 1), h - PADDING2, $"<B><SMALL>{info}</SMALL></B>", false, true);

				info.Clear();

				y += h;
				h = ROW_HEIGHT;

				if (Quest.IsPending)
				{
					var w2 = w / 2;

					AddECHandleInput();

					AddHeader(x, y, w2, 0, false, () => Accept(Quest));
					AddHtml(x + PADDING2, y + PADDING, w2 - PADDING2, h - PADDING2, Center("<B>ACCEPT</B>"), false, false, Color.Green);

					AddECHandleInput();

					AddECHandleInput();

					AddHeader(x + w2, y, w2, 0, false, () => Decline(Quest));
					AddHtml(x + w2 + PADDING2, y + PADDING, w2 - PADDING2, h - PADDING2, Center("<B>DECLINE</B>"), false, false, Color.Red);

					AddECHandleInput();
				}
				else if (Quest.IsCompleted)
				{
					var w2 = w / 2;

					AddECHandleInput();

					AddHeader(x, y, w2, 0, false, () => Redeem(Quest));
					AddHtml(x + PADDING2, y + PADDING, w2 - PADDING2, h - PADDING2, Center("<B>REDEEM</B>"), false, false, Color.Green);

					AddECHandleInput();

					AddECHandleInput();

					AddHeader(x + w2, y, w2, 0, false, () => Abandon(Quest));
					AddHtml(x + w2 + PADDING2, y + PADDING, w2 - PADDING2, h - PADDING2, Center("<B>ABANDON</B>"), false, false, Color.Red);

					AddECHandleInput();
				}
				else if (Quest.IsActive)
				{
					var w2 = w / 2;

					AddECHandleInput();

					AddHeader(x + w2, y, w2, 0, false, () => Abandon(Quest));
					AddHtml(x + w2 + PADDING2, y + PADDING, w2 - PADDING2, h - PADDING2, Center("<B>ABANDON</B>"), false, false, Color.Red);

					AddECHandleInput();
				}
				else
				{
					AddHeader(x, y, w, 0, false);
				}
			}
			else
			{

			}
		}

		private void RenderColumn2(int x, int y, int w, int h)
		{
			AddVerticalScroll(x + (w - (10 + PADDING2)), y, h, 2, States.Count);

			w -= 18;

			var scrollIndex = GetVerticalScroll(2);

			var selectedRowCount = 4;

			var rowCount = (int)Math.Floor(h / (double)ROW_HEIGHT);

			if (Quest != null)
			{
				var questIndex = States.IndexOf(Quest);

				if (questIndex >= scrollIndex && questIndex < scrollIndex + rowCount)
				{
					rowCount -= selectedRowCount - 1;
				}
			}

			var pages = Math.Max(1, (int)Math.Ceiling(States.Count / (double)rowCount));

			scrollIndex = Math.Clamp(scrollIndex, 0, pages - 1);

			SetVerticalScroll(2, scrollIndex);

			var info = new StringBuilder();

			var index = 0;

			foreach (var quest in States.Skip(scrollIndex).Take(rowCount))
			{
				var qx = x;
				var qy = y + (ROW_HEIGHT * index);
				var qw = w;
				var qh = ROW_HEIGHT;

				var selected = quest == Quest;

				if (selected)
				{
					qh *= selectedRowCount;
				}

				qx += PADDING;
				qy += PADDING;
				qw -= PADDING2;
				qh -= PADDING2;

				RenderQuestDetails(quest, qx, qy, qw, qh, false, info);

				if (selected)
				{
					index += 4;
				}
				else
				{
					++index;
				}
			}
		}

		public void Select(Quest quest)
		{
			Quest = quest;

			Refresh();
		}

		private void Accept(Quest quest)
		{
			_ = quest.Accept();

			Refresh();
		}

		private void Decline(Quest quest)
		{
			_ = quest.Decline();

			Refresh();
		}

		private void Redeem(Quest quest)
		{
			_ = quest.Redeem();

			Refresh();
		}

		private void Abandon(Quest quest)
		{
			_ = quest.Abandon();

			Refresh();
		}

		private void RenderQuestDetails(Quest quest, int x, int y, int w, int h, bool expanded, StringBuilder info)
		{
			info ??= new();

			var selected = quest == Quest;

			var qx = x;
			var qy = y;
			var qw = w;
			var qh = h;

			AddECHandleInput();

			if (selected || expanded)
			{
				AddHeader(qx, qy, qw, ROW_HEIGHT, true);
				AddImage(qx - PADDING + 2, qy, 9781);
			}
			else
			{
				AddHeader(qx, qy, qw, ROW_HEIGHT, false, () => Select(quest));
				AddButton(qx - PADDING + 2, qy, 9781, 9780, () => Select(quest));
			}

			qx += 35;
			qw -= 35;

			if (quest.IsCompleted)
			{
				_ = info.Append(SetColor(Color.Maroon, "["));
				_ = info.Append(SetColor(Color.Green, "\u2605")); // Filled Star
				_ = info.Append(SetColor(Color.Maroon, "] "));
			}
			else if (quest.IsPending)
			{
				_ = info.Append(SetColor(Color.Maroon, "["));
				_ = info.Append(SetColor(Color.Green, "\u25C7")); // Empty Diamond
				_ = info.Append(SetColor(Color.Maroon, "] "));
			}
			else
			{
				_ = Utility.Interpolate(quest.ObjectivesCountCompleted, quest.ObjectivesCount, Color.Red, Color.Green, out var rgb);

				_ = info.Append(SetColor(Color.Maroon, "["));
				_ = info.Append(SetColor(rgb, $"{quest.ObjectivesCountCompleted:N0} / {quest.ObjectivesCount:N0}"));
				_ = info.Append(SetColor(Color.Maroon, "] "));
				_ = info.Append(SetColor(Color.DimGray, String.Empty));
			}

			if (!TextDefinition.IsNullOrEmpty(quest.Name))
			{
				_ = info.Append(quest.Name.Combine());
			}
			else
			{
				_ = info.Append(Utility.FriendlyName(quest.GetType()));
			}

			var single = !selected && (!expanded || !quest.IsPending);

			AddHtml(qx, qy + PADDING, qw, ROW_HEIGHT, $"<B><SMALL>{info}</SMALL></B>", false, false);

			AddECHandleInput();

			_ = info.Clear();

			if (single)
			{
				return;
			}

			if (expanded && !TextDefinition.IsNullOrEmpty(quest.Lore))
			{
				_ = info.Append(SetColor(Color.DimGray, String.Empty));
				_ = info.Append(quest.Lore.Combine());
				_ = info.AppendLine();
				_ = info.AppendLine();
			}

			foreach (var obj in Quest.Objectives)
			{
				if (obj.IsComplete)
				{
					_ = info.Append(SetColor(Color.Maroon, "["));
					_ = info.Append(SetColor(Color.Green, "\u2605")); // Filled Star
					_ = info.Append(SetColor(Color.Maroon, "] "));
				}
				else
				{
					_ = Utility.Interpolate(obj.Progress, obj.ProgressRequired, Color.Red, Color.Green, out var rgb);

					_ = info.Append(SetColor(Color.Maroon, "["));
					_ = info.Append(SetColor(rgb, $"{obj.Progress:N0} / {obj.ProgressRequired:N0}"));
					_ = info.Append(SetColor(Color.Maroon, "] "));
					_ = info.Append(SetColor(Color.DimGray, String.Empty));
				}

				if (!TextDefinition.IsNullOrEmpty(obj.Title))
				{
					_ = info.AppendLine(obj.Title.Combine());
				}
				else
				{
					_ = info.AppendLine(Utility.FriendlyName(obj.GetType()));
				}

				if (expanded && !TextDefinition.IsNullOrEmpty(obj.Summary))
				{
					if (obj.IsComplete)
					{
						_ = info.Append(SetColor(Color.DimGray, "\u25C8 ")); // Filled Diamond
					}
					else
					{
						_ = info.Append(SetColor(Color.DimGray, "\u25C7 ")); // Empty Diamond
					}

					_ = info.Append(obj.Summary.Combine());
					_ = info.AppendLine();
					_ = info.AppendLine();
				}
			}

			qx = x;
			qy += ROW_HEIGHT;
			qw = w;
			qh -= ROW_HEIGHT;

			AddBackground(qx, qy, qw - 16, qh, 1579);

			qx += PADDING2;
			qy += PADDING;
			qw -= PADDING2 + 1;
			qh -= PADDING2;

			AddHtml(qx, qy, qw, qh, $"<B><SMALL>{info}</SMALL></B>", false, true);

			_ = info.Clear();
		}
		/*
		private void AddMaps(int x, int y, out int width, out int height)
		{
			width = height = 0;

			var results = Quest.Objectives.SelectMany(o => o.Locations).GroupBy(o => o.Map).ToDictionary(o => o.Key, o => o.ToList());

			var w = 0;
			var h = 0;

			var tabs = AddTabs(x, y, 383, 383 + 40, 0, 0, -1, -1, Color.White, Color.Yellow, null, results.Keys, (xx, yy, ww, hh, map) =>
			{
				AddMap(xx, yy, map.MapID, -1, (mx, my, mw, mh) =>
				{
					w = Math.Max(mw, w);
					h = Math.Max(mh, h);

					foreach (var area in results[map])
					{
						AddPoly(mx, my, area.Bounds, Color.White, p =>
						{
							p.X = Math.Clamp((int)(mw * ((p.X - map.Wrap.X) / (double)map.Wrap.Width)), 0, mw);
							p.Y = Math.Clamp((int)(mh * ((p.Y - map.Wrap.Y) / (double)map.Wrap.Height)), 0, mh);

							return p;
						});
					}
				});
			});

			width = Math.Max(width, w);
			height = Math.Max(height, h);

			height = Math.Max(height, tabs.TotalHeight);
		}
		*/
	}
}
