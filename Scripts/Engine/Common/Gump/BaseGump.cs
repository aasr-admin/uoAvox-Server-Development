using Server.Mobiles;
using Server.Network;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Server.Gumps
{
	public abstract class BaseGump : Gump, IDisposable
	{
		public const int CenterLoc = 1114513;     // <center>~1_val~</center>
		public const int AlignRightLoc = 1114514; // <DIV ALIGN=RIGHT>~1_TOKEN~</DIV>

		public static T SendGump<T>(T gump) where T : BaseGump
		{
			if (gump == null)
			{
				return null;
			}

			var g = gump.User.FindGump<T>();

			if (g == gump)
			{
				gump.Refresh();
			}
			else
			{
				gump.SendGump();
			}

			return gump;
		}

		public static T GetGump<T>(PlayerMobile pm, Func<T, bool> predicate) where T : Gump
		{
			return EnumerateGumps<T>(pm).FirstOrDefault(x => predicate == null || predicate(x));
		}

		public static IEnumerable<T> EnumerateGumps<T>(PlayerMobile pm, Func<T, bool> predicate = null) where T : Gump
		{
			return pm?.NetState?.Gumps?.OfType<T>().Where(g => predicate == null || predicate(g)) ?? Enumerable.Empty<T>();
		}

		public static List<Gump> GetGumps(PlayerMobile pm)
		{
			return new(EnumerateGumps<Gump>(pm));
		}

		public static List<T> GetGumps<T>(PlayerMobile pm) where T : Gump
		{
			return new(EnumerateGumps<T>(pm));
		}

		public static List<BaseGump> GetGumps(PlayerMobile pm, bool checkOpen)
		{
			return new(EnumerateGumps<BaseGump>(pm, g => !checkOpen || g.Open));
		}

		public static List<T> GetGumps<T>(PlayerMobile pm, bool checkOpen) where T : BaseGump
		{
			return new(EnumerateGumps<T>(pm, g => !checkOpen || g.Open));
		}

		public static void CheckCloseGumps(PlayerMobile pm, bool checkOpen = false)
		{
			var ns = pm.NetState;

			if (ns != null)
			{
				var gumps = GetGumps(pm, checkOpen);

				foreach (var gump in gumps)
				{
					if (gump.CloseOnMapChange)
					{
						_ = pm.CloseGump(gump.GetType());
					}
				}

				gumps.Clear();
				gumps.TrimExcess();
			}
		}

		private Gump _Parent;

		private int _ButtonID;

		private Dictionary<int, Action<int>> _Buttons;

		public PlayerMobile User { get; private set; }

		public bool Open { get; protected set; }

		public bool CloseOnMapChange { get; set; }

		public Gump Parent
		{
			get => _Parent;
			set
			{
				if (_Parent == value)
				{
					return;
				}

				if (_Parent is BaseGump opg)
				{
					_ = opg.Children.Remove(this);
				}

				_Parent = value;

				if (_Parent is BaseGump npg)
				{
					_ = npg.Children.Add(this);
				}
			}
		}

		public HashSet<BaseGump> Children { get; private set; } = new();

		public BaseGump(PlayerMobile user, int x = 50, int y = 50, BaseGump parent = null)
			: base(x, y)
		{
			User = user;
			Parent = parent;
		}

		~BaseGump()
		{
			Dispose();
		}

		public virtual void SendGump()
		{
			AddGumpLayout();

			Open = User.SendGump(this);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);

			Children.Clear();
			Children = null;

			User = null;
			Parent = null;

			if (_TextTooltips != null)
			{
				foreach (var kvp in _TextTooltips)
				{
					kvp.Value.Free();
				}

				_TextTooltips.Clear();
				_TextTooltips.TrimExcess();
			}

			if (_ClilocTooltips != null)
			{
				foreach (var kvp in _ClilocTooltips)
				{
					kvp.Value.Free();
				}

				_ClilocTooltips.Clear();
				_ClilocTooltips.TrimExcess();
			}

			if (_VScrollIndex != null)
			{
				_VScrollIndex.Clear();
				_VScrollIndex.TrimExcess();
				_VScrollIndex = null;
			}

			if (_Controls != null)
			{
				_Controls.Clear();
				_Controls.TrimExcess();
				_Controls = null;
			}

			if (_Buttons != null)
			{
				_Buttons.Clear();
				_Buttons.TrimExcess();
				_Buttons = null;
			}

			OnDispose();
		}

		public virtual void OnDispose()
		{
		}

		public abstract void AddGumpLayout();

		public virtual void Refresh(bool recompile = true, bool close = true)
		{
			OnBeforeRefresh();

			if (User == null || User.NetState == null)
			{
				return;
			}

			if (close)
			{
				User.NetState.Send(new CloseGump(TypeID, 0));
				User.NetState.RemoveGump(this);
			}
			else
			{
				User.NetState.RemoveGump(this);
			}

			if (recompile)
			{
				Entries.Clear();
				AddGumpLayout();
			}

			Open = User.SendGump(this);

			OnAfterRefresh();
		}

		public void RefreshParent(bool resend = false)
		{
			if (Parent is BaseGump pg)
			{
				pg.Refresh();
			}

			if (resend)
			{
				Refresh();
			}
		}

		public virtual void OnBeforeRefresh()
		{
		}

		public virtual void OnAfterRefresh()
		{
		}

		public virtual void OnClosed()
		{
			foreach (var child in Children)
			{
				child.Close();
			}

			Children.Clear();

			Open = false;

			if (Parent is BaseGump pg)
			{
				pg.OnChildClosed(this);
			}

			Parent = null;
		}

		public virtual void OnChildClosed(BaseGump gump)
		{
		}

		public override sealed void OnResponse(NetState state, RelayInfo info)
		{
			OnResponse(info);

			if (info.ButtonID == 0)
			{
				OnClosed();
			}
		}

		public virtual void OnResponse(RelayInfo info)
		{
			if (_Buttons != null && _Buttons.TryGetValue(info.ButtonID, out var callback) && callback != null)
			{
				callback(info.ButtonID);
			}
			else if (info.ButtonID != 0)
			{
				Refresh();
			}
			else
			{
				Close();
			}
		}

		public virtual void OnServerClosed(NetState state)
		{
			OnClosed();
		}

		public virtual void Close()
		{
			if (User == null || User.NetState == null)
			{
				return;
			}

			OnServerClose(User.NetState);

			_ = User.Send(new CloseGump(TypeID, 0));

			User.NetState.RemoveGump(this);

			OnClosed();

			_Buttons?.Clear();
		}

		private Dictionary<int, int> _VScrollIndex;

		public void SetVerticalScroll(int id, int index)
		{
			if (index > 0)
			{
				_VScrollIndex ??= new();

				_VScrollIndex[id] = index;
			}
			else if (_VScrollIndex != null)
			{
				_VScrollIndex.Remove(id);
			}
		}

		public int GetVerticalScroll(int id)
		{
			if (_VScrollIndex == null)
			{
				return 0;
			}

			_VScrollIndex.TryGetValue(id, out var index);

			return index;
		}

		/// <summary>
		/// The total width of this element is 18 (4 + 10 + 4)<br />
		/// The minimum height of this element is 48 (4 + 11 + 4 + 10~ + 4 + 11 + 4)
		/// </summary>
		public void AddVerticalScroll(int x, int y, int height, int id, int count)
		{
			var padding = 4;
			var padding2 = padding * 2;
			var width = 10 + padding2;

			height = Math.Max(height, padding + 11 + padding + 10 + padding + 11 + padding);

			x += padding;
			y += padding;
			width -= padding2;
			height -= padding2;

			var index = GetVerticalScroll(id);

			if (count > 0 && index > 0)
			{
				var page = Math.Clamp(index - 1, 0, count - 1);

				AddButton(x, y, 2435, 2436, () =>
				{
					_VScrollIndex ??= new();

					_VScrollIndex[id] = page;

					Refresh();
				});
			}
			else
			{
				AddImage(x, y, 2435, 900);
			}

			if (count > 0 && index + 1 < count)
			{
				var page = Math.Clamp(index + 1, 0, count - 1);

				AddButton(x, y + (height - 11), 2437, 2438, () =>
				{
					_VScrollIndex ??= new();

					_VScrollIndex[id] = page;

					Refresh();
				});
			}
			else
			{
				AddImage(x, y + (height - 11), 2437, 900);
			}

			y += 11 + padding;

			var bx = x - 1;
			var by = y;
			var bw = width;
			var bh = height;

			var si = 0;
			var sc = Math.Floor(height / 10.0);

			while (by < bh)
			{
				if (by + 10 > bh)
				{
					by = bh - 10;
				}

				if (count > 0)
				{
					var page = Math.Clamp((int)(count * (si / sc)), 0, count - 1);

					if (page != si)
					{
						AddButton(bx, by, 30550, 30550, () =>
						{
							_VScrollIndex ??= new();

							_VScrollIndex[id] = page;

							Refresh();
						});
					}
				}

				++si;

				by += 10;
			}

			var fx = x - 1;
			var fy = y;
			var fw = width;
			var fh = (int)(height / sc);

			if (fh > 0)
			{
				var fo = height - (fy + fh);

				fy += Math.Clamp((int)(fo * (index / sc)), 0, fo);

				AddImageTiled(fx, fy, fw, fh, 30556);
			}
		}

		public void AddButton(int x, int y, int normalID, int pressedID, Action callback)
		{
			AddButton(x, y, normalID, pressedID, _ => callback());
		}

		public void AddButton(int x, int y, int normalID, int pressedID, Action<int> callback)
		{
			var buttonID = ++_ButtonID;

			AddButton(x, y, normalID, pressedID, buttonID, GumpButtonType.Reply, buttonID);

			_Buttons ??= new Dictionary<int, Action<int>>();

			_Buttons[buttonID] = callback;
		}

		public void AddHtml(int x, int y, int width, int height, string text, bool background, bool scrollbar, Color color)
		{
			text = SetColor(color, text);

			AddHtml(x, y, width, height, text, background, scrollbar);
		}

		public void AddItemProperty(Item item)
		{
			item.SendPropertiesTo(User);

			AddItemProperty(item.Serial);
		}

		public void AddMobileProperty(Mobile mob)
		{
			mob.SendPropertiesTo(User);

			AddItemProperty(mob.Serial.Value);
		}

		#region Controls

		private Dictionary<int, object> _Controls;

		public bool HasTabs<T>(Action<int, int, int, int, T> renderer)
		{
			return _Controls?.ContainsKey(renderer.GetHashCode()) == true;
		}

		public T GetTabs<T>(Action<int, int, int, int, T> renderer)
		{
			object o = null;

			_Controls?.TryGetValue(renderer.GetHashCode(), out o);

			if (o is T t)
			{
				return t;
			}

			return default;
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(Rectangle o, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(o.X, o.Y, o.Width, o.Height, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(int x, int y, int w, int h, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(x, y, w, h, null, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(Rectangle o, T initValue, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(o.X, o.Y, o.Width, o.Height, initValue, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(int x, int y, int w, int h, T initValue, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(x, y, w, h, initValue, null, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(Rectangle o, int pad, int bgID, int btnNormal, int btnSelected, Color txtNormal, Color txtSelected, T initValue, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(o.X, o.Y, o.Width, o.Height, pad, bgID, btnNormal, btnSelected, txtNormal, txtSelected, initValue, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(int x, int y, int w, int h, int pad, int bgID, int btnNormal, int btnSelected, Color txtNormal, Color txtSelected, T initValue, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(x, y, w, h, pad, bgID, btnNormal, btnSelected, txtNormal, txtSelected, initValue, null, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(Rectangle o, ICollection<T> values, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(o.X, o.Y, o.Width, o.Height, values, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(int x, int y, int w, int h, ICollection<T> values, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(x, y, w, h, default(T), values, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(Rectangle o, T initValue, ICollection<T> values, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(o.X, o.Y, o.Width, o.Height, initValue, values, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(int x, int y, int w, int h, T initValue, ICollection<T> values, Action<int, int, int, int, T> onRender)
		{
			var sup = false;
			var pad = sup ? 15 : 10;
			var bgID = sup ? 40000 : 9270;
			var btnNormal = sup ? 40016 : 9909;
			var btnSelected = sup ? 40027 : 9904;

			return AddTabs(x, y, w, h, pad, bgID, btnNormal, btnSelected, Color.White, Color.Gold, initValue, values, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(Rectangle o, int pad, int bgID, int btnNormal, int btnSelected, Color txtNormal, Color txtSelected, T initValue, ICollection<T> values, Action<int, int, int, int, T> onRender)
		{
			return AddTabs(o.X, o.Y, o.Width, o.Height, pad, bgID, btnNormal, btnSelected, txtNormal, txtSelected, initValue, values, onRender);
		}

		/// <summary>
		///     onRender: (x, y, w, h, obj)
		/// </summary>
		public (T Selected, int TotalHeight) AddTabs<T>(int x, int y, int w, int h, int pad, int bgID, int btnNormal, int btnSelected, Color txtNormal, Color txtSelected, T initValue, ICollection<T> values, Action<int, int, int, int, T> onRender)
		{
			if (w * h <= 0 || pad * 2 >= w || pad * 2 >= h || onRender == null)
			{
				return (default, -1);
			}

			if (values == null && typeof(T).IsEnum)
			{
				values = (T[])Enum.GetValues(typeof(T));
			}

			if (values == null || values.Count == 0)
			{
				return (default, -1);
			}

			var hash = onRender.GetHashCode();

			object value = null;

			_ = _Controls?.TryGetValue(hash, out value);

			if (value is not T vt || !values.Contains(vt))
			{
				value = initValue;
			}

			if (btnNormal < 0)
			{
				btnNormal = 9909;
			}

			if (btnSelected < 0)
			{
				btnSelected = 9904;
			}

			var btnSize = GetImageSize(btnNormal);

			var titleX = pad + btnSize.Width + 5;
			var titleH = btnSize.Height + 5;

			var titleC = values.Count(v => v != null);

			var font = FontData.GetUnicode(0);

			var maxW = values.Select(v => Utility.SpaceWords($"{v}")).Aggregate(pad, (c, o) => Math.Max(c, font.GetWidth(o)));

			var tabW = titleX + maxW;
			var tabH = titleH;

			var tabC = w / tabW;
			var tabR = (int)Math.Ceiling(titleC / (double)tabC);

			if (tabW * tabC < w - (pad * 2))
			{
				tabW += (int)((w - (pad * 2) - (tabW * tabC)) / (double)tabC);
			}

			var tabsH = (pad * 2) + (tabR * tabH);
			var panelH = h - tabsH;

			var txtWidth = tabW - titleX;

			var px = x;
			var py = y + tabsH;
			var hh = 0;
			var i = 0;

			if (bgID > 0)
			{
				AddBackground(x, y, w, tabsH, bgID);
			}

			foreach (var val in values.Where(v => v != null))
			{
				var v = val;
				var s = Equals(value, v);
				var l = SetColor(s ? txtSelected : txtNormal, $"<BIG>{ResolveLabel(v)}</BIG>");

				AddECHandleInput();

				AddButton(x + pad, y + pad, s ? btnSelected : btnNormal, s ? btnNormal : btnSelected, b =>
				{
					_Controls ??= new();

					_Controls[hash] = s ? initValue : v;

					Refresh(true);
				});

				AddHtml(x + titleX, ComputeCenter(y + pad, tabH) - 10, txtWidth, 40, l, false, false);

				AddECHandleInput();

				if (s)
				{
					if (bgID > 0)
					{
						AddBackground(px, py, w, panelH, bgID);
					}

					onRender(px + pad, py + pad, w - (pad * 2), panelH - (pad * 2), v);

					hh += panelH;
				}

				if (++i % tabC == 0)
				{
					x = px;
					y += tabH;
					hh += tabH;
				}
				else
				{
					x += tabW;
				}
			}

			return ((T)value, hh);
		}

		#endregion

		#region Images

		public static Size GetImageSize(int gumpID)
		{
			var asset = GumpData.GetGump(gumpID);

			return asset?.Size ?? Size.Empty;
		}

		#endregion

		#region Formatting

		public static string SetColor(Color color, string str)
		{
			return SetColor(color.ToArgb(), str);
		}

		public static string SetColor(short color16, string str)
		{
			Utility.ConvertColor(color16, out var color32); 

			return SetColor(color32, str);
		}

		public static string SetColor(int color32, string str)
		{
			var r = Math.Max(0x08, (color32 >> 16) & 0xFF);
			var g = Math.Max(0x08, (color32 >> 08) & 0xFF);
			var b = Math.Max(0x08, (color32 >> 00) & 0xFF);

			color32 = (r << 16) | (g << 08) | (b << 00);

			return $"<BASEFONT COLOR=#{color32:X6}>{str}";
		}

		public static string ColorAndCenter(Color color, string str)
		{
			return ColorAndCenter(color.ToArgb(), str);
		}

		public static string ColorAndCenter(short color16, string str)
		{
			return SetColor(color16, Center(str));
		}

		public static string ColorAndCenter(int color32, string str)
		{
			return SetColor(color32, Center(str));
		}

		public static string Center(string str)
		{
			return $"<CENTER>{str}</CENTER>";
		}

		public static string ColorAndAlignRight(Color color, string str)
		{
			return ColorAndAlignRight(color.ToArgb(), str);
		}

		public static string ColorAndAlignRight(short color16, string str)
		{
			return SetColor(color16, AlignRight(str));
		}

		public static string ColorAndAlignRight(int color32, string str)
		{
			return SetColor(color32, AlignRight(str));
		}

		public static string AlignRight(string str)
		{
			return $"<DIV ALIGN=RIGHT>{str}</DIV>";
		}

		#endregion

		#region Tooltips

		private Dictionary<int, Spoof> _TextTooltips, _ClilocTooltips;

		public void AddTooltip(TextDefinition text)
		{
			if (text.Number > 0)
			{
				AddTooltip(text.Number);
			}
			else if (!String.IsNullOrEmpty(text.String))
			{
				AddTooltip(text.String);
			}
		}

		public void AddTooltip(string text, Color color)
		{
			AddTooltip(String.Empty, text, Color.Empty, color);
		}

		public void AddTooltip(string title, string text)
		{
			AddTooltip(title, text, Color.Empty, Color.Empty);
		}

		public void AddTooltip(int cliloc, string format, params string[] args)
		{
			base.AddTooltip(cliloc, String.Format(format, args));
		}

		public void AddTooltip(int[] clilocs)
		{
			AddTooltip(clilocs, new string[clilocs.Length]);
		}

		public void AddTooltip(string[] args)
		{
			var clilocs = new int[Math.Min(Spoof.EmptyClilocs.Length, args.Length)];

			for (var i = 0; i < args.Length; i++)
			{
				if (i >= Spoof.EmptyClilocs.Length)
				{
					break;
				}

				clilocs[i] = Spoof.EmptyClilocs[i];
			}

			AddTooltip(clilocs, args);
		}

		public void AddTooltip(int[] clilocs, string[] args)
		{
			var h = new HashCode();

			for (var i = 0; i < clilocs.Length; i++)
			{
				h.Add(clilocs[i]);

				if (args != null && i < args.Length && args[i] != null)
				{
					h.Add(args[i]);
				}
			}

			var hash = h.ToHashCode();

			Spoof spoof;

			if (_ClilocTooltips?.TryGetValue(hash, out spoof) != true || spoof?.Deleted != false)
			{
				_ClilocTooltips ??= new();

				_ClilocTooltips[hash] = spoof = Spoof.Acquire();

				var dictionary = spoof.ClilocTable ??= new();

				dictionary.Clear();

				var emptyIndex = 0;

				for (var i = 0; i < clilocs.Length; i++)
				{
					var str = String.Empty;

					if (i < args.Length)
					{
						str = args[i] ?? String.Empty;
					}

					var cliloc = clilocs[i];

					if (cliloc <= 0)
					{
						if (emptyIndex <= Spoof.EmptyClilocs.Length)
						{
							cliloc = Spoof.EmptyClilocs[emptyIndex];

							emptyIndex++;
						}
					}

					if (cliloc > 0)
					{
						dictionary[cliloc] = str;
					}
				}
			}

			AddItemProperty(spoof);
		}

		public void AddTooltip(string title, string text, Color titleColor, Color textColor)
		{
			title ??= String.Empty;
			text ??= String.Empty;

			if (titleColor.IsEmpty || titleColor == Color.Transparent)
			{
				titleColor = Color.White;
			}

			if (textColor.IsEmpty || textColor == Color.Transparent)
			{
				textColor = Color.White;
			}

			var hash = HashCode.Combine(title, text, titleColor, textColor);

			Spoof spoof;

			if (_TextTooltips?.TryGetValue(hash, out spoof) != true || spoof?.Deleted != false)
			{
				_TextTooltips ??= new();

				_TextTooltips[hash] = spoof = Spoof.Acquire();

				if (!String.IsNullOrWhiteSpace(title))
				{
					spoof.Text = $"{SetColor(titleColor, title)}\n{SetColor(textColor, text)}";
				}
				else
				{
					spoof.Text = SetColor(textColor, text);
				}
			}

			AddItemProperty(spoof);
		}

		public static string StripHtmlBreaks(string str, bool preserve)
		{
			return Regex.Replace(str, @"<br[^>]?>", preserve ? "\n" : " ", RegexOptions.IgnoreCase);
		}

		public static string ResolveLabel(object o)
		{
			if (o is Enum en)
			{
				return Utility.FriendlyName(en);
			}

			if (o is IEntity e)
			{
				return Utility.FriendlyName(e);
			}

			return o.ToString();
		}

		public static Point ComputeCenter(Point p, Size s)
		{
			return new Point(ComputeCenter(p.X, s.Width), ComputeCenter(p.Y, s.Height));
		}

		public static void ComputeCenter(ref Point p, Size s)
		{
			p = new Point(ComputeCenter(p.X, s.Width), ComputeCenter(p.Y, s.Height));
		}

		public static Point ComputeCenter(int x, int y, int w, int h)
		{
			ComputeCenter(ref x, ref y, w, h);

			return new Point(x, y);
		}

		public static void ComputeCenter(ref int x, ref int y, int w, int h)
		{
			x = ComputeCenter(x, w);
			y = ComputeCenter(y, h);
		}

		public static void ComputeCenter(ref int o, int s)
		{
			o = ComputeCenter(o, s);
		}

		public static int ComputeCenter(int o, int s)
		{
			return o + (s / 2);
		}

		private sealed class Spoof : Item
		{
			public static readonly int[] EmptyClilocs =
			{
				1042971, 1070722, // ~1_NOTHING~
			    1114057, 1114778, 1114779, // ~1_val~
			    1150541, // ~1_TOKEN~
			    1153153, // ~1_year~
            };

			private static readonly char[] _Split = { '\n' };

			private static readonly Queue<Spoof> _SpoofPool = new();

			public static Spoof Acquire()
			{
				Spoof spoof = null;

				while (spoof?.Deleted != false)
				{
					if (_SpoofPool.Count == 0)
					{
						return new Spoof();
					}

					spoof = _SpoofPool.Dequeue();
				}

				return spoof;
			}

			private string _Text = String.Empty;

			public string Text
			{
				get => _Text ?? String.Empty;
				set
				{
					if (_Text != value)
					{
						_Text = value;

						ClearProperties();
					}
				}
			}

			private Dictionary<int, string> _ClilocTable;

			public Dictionary<int, string> ClilocTable
			{
				get => _ClilocTable;
				set
				{
					if (_ClilocTable != value)
					{
						_ClilocTable = value;

						ClearProperties();
					}
				}
			}

			public override bool Decays => false;

			public override string DefaultName => " ";

			public Spoof()
				: base(1)
			{
				Visible = false;
				Movable = false;

				Internalize();
			}

			public Spoof(Serial serial)
				: base(serial)
			{
			}

			public void Free()
			{
				ClearProperties();

				_Text = String.Empty;

				_ClilocTable = null;

				if (!Deleted && _SpoofPool.Count < 128)
				{
					_SpoofPool.Enqueue(this);
				}
				else
				{
					Delete();
				}
			}

			public override void AddNameProperty(ObjectPropertyList list)
			{
			}

			public override void AddNameProperties(ObjectPropertyList list)
			{
			}

			public override void GetProperties(ObjectPropertyList list)
			{
				if (!String.IsNullOrEmpty(Text))
				{
					var text = StripHtmlBreaks(Text, true);

					if (text.Contains('\n'))
					{
						var lines = text.Split(_Split);

						foreach (var str in lines)
						{
							list.Add(str);
						}
					}
					else
					{
						list.Add(text);
					}
				}
				else if (_ClilocTable?.Count > 0)
				{
					foreach (var kvp in _ClilocTable)
					{
						var cliloc = kvp.Key;
						var args = kvp.Value;

						if (cliloc <= 0 && !String.IsNullOrEmpty(args))
						{
							list.Add(args);
						}
						else if (String.IsNullOrEmpty(args))
						{
							list.Add(cliloc);
						}
						else
						{
							list.Add(cliloc, args);
						}
					}
				}
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.WriteEncodedInt(0);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				_ = reader.ReadEncodedInt();

				Timer.DelayCall(Free);
			}
		}

		#endregion
	}
}