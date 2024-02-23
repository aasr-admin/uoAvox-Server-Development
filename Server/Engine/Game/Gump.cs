using Server.Network;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Server.Gumps
{
	public class Gump
	{
		public static IPersistentHashGenerator HashGenerator { get; set; } = PersistentHash.Default;

		public static int GetTypeID(Type type)
		{
			return HashGenerator.Generate(type.FullName, true);
		}

		private readonly List<GumpEntry> m_Entries;
		private readonly List<string> m_Strings;

		internal int m_TextEntries, m_Switches;

		private static int m_NextSerial = 1;

		private int m_Serial;
		private int m_X, m_Y;

		private bool m_Dragable = true;
		private bool m_Closable = true;
		private bool m_Resizable = true;
		private bool m_Disposable = true;

		public Gump(int x, int y)
		{
			do
			{
				m_Serial = m_NextSerial++;
			} while (m_Serial == 0); // standard client apparently doesn't send a gump response packet if serial == 0

			m_X = x;
			m_Y = y;

			TypeID = GetTypeID(GetType());

			m_Entries = new();
			m_Strings = new();
		}

		public virtual void Invalidate()
		{
		}

		public int TypeID { get; protected set; }

		public List<GumpEntry> Entries => m_Entries;

		public int Serial
		{
			get => m_Serial;
			set
			{
				if (m_Serial != value)
				{
					m_Serial = value;
					Invalidate();
				}
			}
		}

		public int X
		{
			get => m_X;
			set
			{
				if (m_X != value)
				{
					m_X = value;
					Invalidate();
				}
			}
		}

		public int Y
		{
			get => m_Y;
			set
			{
				if (m_Y != value)
				{
					m_Y = value;
					Invalidate();
				}
			}
		}

		public bool Disposable
		{
			get => m_Disposable;
			set
			{
				if (m_Disposable != value)
				{
					m_Disposable = value;
					Invalidate();
				}
			}
		}

		public bool Resizable
		{
			get => m_Resizable;
			set
			{
				if (m_Resizable != value)
				{
					m_Resizable = value;
					Invalidate();
				}
			}
		}

		public bool Dragable
		{
			get => m_Dragable;
			set
			{
				if (m_Dragable != value)
				{
					m_Dragable = value;
					Invalidate();
				}
			}
		}

		public bool Closable
		{
			get => m_Closable;
			set
			{
				if (m_Closable != value)
				{
					m_Closable = value;
					Invalidate();
				}
			}
		}

		public void AddPage(int page)
		{
			Add(new GumpPage(page));
		}

		public void AddAlphaRegion(int x, int y, int width, int height)
		{
			Add(new GumpAlphaRegion(x, y, width, height));
		}

		public void AddBackground(int x, int y, int width, int height, int gumpID)
		{
			Add(new GumpBackground(x, y, width, height, gumpID));
		}

		public void AddButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
		{
			Add(new GumpButton(x, y, normalID, pressedID, buttonID, type, param));
		}

		public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
		{
			Add(new GumpCheck(x, y, inactiveID, activeID, initialState, switchID));
		}

		public void AddGroup(int group)
		{
			Add(new GumpGroup(group));
		}

		public void AddTooltip(int number)
		{
			Add(new GumpTooltip(number));
		}

		public void AddTooltip(int number, string args)
		{
			Add(new GumpTooltip(number, args));
		}

		public void AddTooltip(string text)
		{
			Add(new GumpTooltip(1042971, text));
		}

		public void AddHtml(int x, int y, int width, int height, string text, bool background, bool scrollbar)
		{
			Add(new GumpHtml(x, y, width, height, text, background, scrollbar));
		}

		public void AddHtmlLocalized(int x, int y, int width, int height, int number, bool background, bool scrollbar)
		{
			Add(new GumpHtmlLocalized(x, y, width, height, number, background, scrollbar));
		}

		public void AddHtmlLocalized(int x, int y, int width, int height, int number, short color, bool background, bool scrollbar)
		{
			Add(new GumpHtmlLocalized(x, y, width, height, number, color, background, scrollbar));
		}

		public void AddHtmlLocalized(int x, int y, int width, int height, int number, string args, short color, bool background, bool scrollbar)
		{
			Add(new GumpHtmlLocalized(x, y, width, height, number, args, color, background, scrollbar));
		}

		public void AddSpriteImage(int x, int y, int gumpID, int width, int height, int sx, int sy)
		{
			Add(new GumpSpriteImage(x, y, gumpID, width, height, sx, sy));
		}

		public void AddImage(int x, int y, int gumpID)
		{
			Add(new GumpImage(x, y, gumpID));
		}

		public void AddImage(int x, int y, int gumpID, int hue)
		{
			Add(new GumpImage(x, y, gumpID, hue));
		}

		public void AddImageTiled(int x, int y, int width, int height, int gumpID)
		{
			Add(new GumpImageTiled(x, y, width, height, gumpID));
		}

		public void AddImageTiledButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height)
		{
			Add(new GumpImageTileButton(x, y, normalID, pressedID, buttonID, type, param, itemID, hue, width, height));
		}

		public void AddImageTiledButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height, int localizedTooltip)
		{
			Add(new GumpImageTileButton(x, y, normalID, pressedID, buttonID, type, param, itemID, hue, width, height, localizedTooltip));
		}

		public void AddItem(int x, int y, int itemID)
		{
			Add(new GumpItem(x, y, itemID));
		}

		public void AddItem(int x, int y, int itemID, int hue)
		{
			Add(new GumpItem(x, y, itemID, hue));
		}

		public void AddLabel(int x, int y, int hue, string text)
		{
			Add(new GumpLabel(x, y, hue, text));
		}

		public void AddLabelCropped(int x, int y, int width, int height, int hue, string text)
		{
			Add(new GumpLabelCropped(x, y, width, height, hue, text));
		}

		public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
		{
			Add(new GumpRadio(x, y, inactiveID, activeID, initialState, switchID));
		}

		public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText)
		{
			Add(new GumpTextEntry(x, y, width, height, hue, entryID, initialText));
		}

		public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, int size)
		{
			Add(new GumpTextEntryLimited(x, y, width, height, hue, entryID, initialText, size));
		}

		public void AddPoly(int x, int y, Poly3D poly, Color color, Func<Point2D, Point2D> mutator = null)
		{
			AddPoly(x, y, poly.m_Poly, color, mutator);
		}

		public void AddPoly(int x, int y, Poly2D poly, Color color, Func<Point2D, Point2D> mutator = null) 
		{
			AddPoly(x, y, poly, color, color, mutator);
		}

		public void AddPoly(int x, int y, Poly2D poly, Color colorStart, Color colorEnd, Func<Point2D, Point2D> mutator = null)
		{
			if (poly.Count > 0)
			{
				for (int i = 0, n = 1; i < poly.Count; i = n++)
				{
					var rgb = Utility.Interpolate(n, poly.Count, colorStart, colorEnd, out _);

					var cur = poly.m_Points[i % poly.Count];
					var next = poly.m_Points[n % poly.Count];

					if (mutator != null)
					{
						cur = mutator(cur);
						next = mutator(next);
					}

					if (cur == next)
					{
						AddHtml(x + cur.X, y + cur.Y, 1, 1, $"<BODYBGCOLOR=#{rgb:X6}> ", false, false);

						break;
					}

					if (cur.m_X == next.m_X)
					{
						var x1 = Math.Min(cur.m_X, next.m_X);
						var y1 = Math.Min(cur.m_Y, next.m_Y);

						var w = Math.Abs(next.m_X - cur.m_X);

						AddHtml(x + x1, y + y1, w, 1, $"<BODYBGCOLOR=#{rgb:X6}> ", false, false);
					}
					else if (cur.m_Y == next.m_Y)
					{
						var x1 = Math.Min(cur.m_X, next.m_X);
						var y1 = Math.Min(cur.m_Y, next.m_Y);

						var h = Math.Abs(next.m_Y - cur.m_Y);

						AddHtml(x + x1, y + y1, 1, h, $"<BODYBGCOLOR=#{rgb:X6}> ", false, false);
					}
					else
					{
						foreach (var p in Geometry.TraceLine2D(cur, next))
						{
							AddHtml(x + p.X, y + p.Y, 1, 1, $"<BODYBGCOLOR=#{rgb:X6}> ", false, false);
						}
					}
				}
			}
		}

		/// <summary>
		/// onRender(imageX, imageY, imageWidth, imageHeight)
		/// </summary>
		public void AddMap(int x, int y, int mapID, int frameHue = -1, Action<int, int, int, int> onRender = null)
		{
			var framed = frameHue >= 0;

			var ox = framed ? x + 5 : x;
			var oy = framed ? y + 5 : y;

			AddImage(ox, oy, 8013 + mapID);

			onRender?.Invoke(ox, oy, 383, 383);

			if (framed)
			{
				AddImage(x, y, 5599, frameHue);
			}
		}

		public void AddItemProperty(int serial)
		{
			Add(new GumpItemProperty(serial));
		}

		public void AddECHandleInput()
		{
			Add(new ECHandleInput());
		}

		public void Add(GumpEntry g)
		{
			if (g.Parent != this)
			{
				g.Parent = this;
			}
			else if (!m_Entries.Contains(g))
			{
				Invalidate();
				m_Entries.Add(g);
			}
		}

		public void Remove(GumpEntry g)
		{
			if (g == null || !m_Entries.Contains(g))
			{
				return;
			}

			Invalidate();
			m_Entries.Remove(g);
			g.Parent = null;
		}

		public int Intern(string value)
		{
			var indexOf = m_Strings.IndexOf(value);

			if (indexOf >= 0)
			{
				return indexOf;
			}
			else
			{
				Invalidate();
				m_Strings.Add(value);
				return m_Strings.Count - 1;
			}
		}

		public void SendTo(NetState state)
		{
			state.AddGump(this);
			state.Send(Compile(state));
		}

		public static byte[] StringToBuffer(string str)
		{
			return Encoding.ASCII.GetBytes(str);
		}

		private static readonly byte[] m_BeginLayout = StringToBuffer("{ ");
		private static readonly byte[] m_EndLayout = StringToBuffer(" }");

		private static readonly byte[] m_NoMove = StringToBuffer("{ nomove }");
		private static readonly byte[] m_NoClose = StringToBuffer("{ noclose }");
		private static readonly byte[] m_NoDispose = StringToBuffer("{ nodispose }");
		private static readonly byte[] m_NoResize = StringToBuffer("{ noresize }");

		private Packet Compile()
		{
			return Compile(null);
		}

		private Packet Compile(NetState ns)
		{
			IGumpWriter disp;

			if (ns != null && ns.Unpack)
			{
				disp = new DisplayGumpPacked(this);
			}
			else
			{
				disp = new DisplayGumpFast(this);
			}

			if (!m_Dragable)
			{
				disp.AppendLayout(m_NoMove);
			}

			if (!m_Closable)
			{
				disp.AppendLayout(m_NoClose);
			}

			if (!m_Disposable)
			{
				disp.AppendLayout(m_NoDispose);
			}

			if (!m_Resizable)
			{
				disp.AppendLayout(m_NoResize);
			}

			var count = m_Entries.Count;
			GumpEntry e;

			for (var i = 0; i < count; ++i)
			{
				e = m_Entries[i];

				disp.AppendLayout(m_BeginLayout);
				e.AppendTo(disp);
				disp.AppendLayout(m_EndLayout);
			}

			disp.WriteStrings(m_Strings);

			disp.Flush();

			m_TextEntries = disp.TextEntries;
			m_Switches = disp.Switches;

			return disp as Packet;
		}

		public virtual void OnResponse(NetState sender, RelayInfo info)
		{
		}

		public virtual void OnServerClose(NetState owner)
		{
		}
	}

	public abstract class GumpEntry
	{
		private Gump m_Parent;

		protected GumpEntry()
		{
		}

		protected void Delta(ref int var, int val)
		{
			if (var != val)
			{
				var = val;

				if (m_Parent != null)
				{
					m_Parent.Invalidate();
				}
			}
		}

		protected void Delta(ref bool var, bool val)
		{
			if (var != val)
			{
				var = val;

				if (m_Parent != null)
				{
					m_Parent.Invalidate();
				}
			}
		}

		protected void Delta(ref string var, string val)
		{
			if (var != val)
			{
				var = val;

				if (m_Parent != null)
				{
					m_Parent.Invalidate();
				}
			}
		}

		public Gump Parent
		{
			get => m_Parent;
			set
			{
				if (m_Parent != value)
				{
					if (m_Parent != null)
					{
						m_Parent.Remove(this);
					}

					m_Parent = value;

					if (m_Parent != null)
					{
						m_Parent.Add(this);
					}
				}
			}
		}

		public abstract string Compile();
		public abstract void AppendTo(IGumpWriter disp);
	}

	public class GumpPage : GumpEntry
	{
		private int m_Page;

		public GumpPage(int page)
		{
			m_Page = page;
		}

		public int Page
		{
			get => m_Page;
			set => Delta(ref m_Page, value);
		}

		public override string Compile()
		{
			return String.Format("{{ page {0} }}", m_Page);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("page");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_Page);
		}
	}

	public class GumpBackground : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Width, m_Height;
		private int m_GumpID;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public int GumpID
		{
			get => m_GumpID;
			set => Delta(ref m_GumpID, value);
		}

		public GumpBackground(int x, int y, int width, int height, int gumpID)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_GumpID = gumpID;
		}

		public override string Compile()
		{
			return String.Format("{{ resizepic {0} {1} {2} {3} {4} }}", m_X, m_Y, m_GumpID, m_Width, m_Height);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("resizepic");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_GumpID);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);
		}
	}

	public class GumpAlphaRegion : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Width, m_Height;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public GumpAlphaRegion(int x, int y, int width, int height)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
		}

		public override string Compile()
		{
			return String.Format("{{ checkertrans {0} {1} {2} {3} }}", m_X, m_Y, m_Width, m_Height);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("checkertrans");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);
		}
	}

	public class GumpItem : GumpEntry
	{
		private int m_X, m_Y;
		private int m_ItemID;
		private int m_Hue;

		public GumpItem(int x, int y, int itemID) : this(x, y, itemID, 0)
		{
		}

		public GumpItem(int x, int y, int itemID, int hue)
		{
			m_X = x;
			m_Y = y;
			m_ItemID = itemID;
			m_Hue = hue;
		}

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int ItemID
		{
			get => m_ItemID;
			set => Delta(ref m_ItemID, value);
		}

		public int Hue
		{
			get => m_Hue;
			set => Delta(ref m_Hue, value);
		}

		public override string Compile()
		{
			if (m_Hue == 0)
			{
				return String.Format("{{ tilepic {0} {1} {2} }}", m_X, m_Y, m_ItemID);
			}
			else
			{
				return String.Format("{{ tilepichue {0} {1} {2} {3} }}", m_X, m_Y, m_ItemID, m_Hue);
			}
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("tilepic");
		private static readonly byte[] m_LayoutNameHue = Gump.StringToBuffer("tilepichue");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_Hue == 0 ? m_LayoutName : m_LayoutNameHue);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_ItemID);

			if (m_Hue != 0)
			{
				disp.AppendLayout(m_Hue);
			}
		}
	}

	public class GumpItemProperty : GumpEntry
	{
		private int m_Serial;

		public GumpItemProperty(int serial)
		{
			m_Serial = serial;
		}

		public int Serial
		{
			get => m_Serial;
			set => Delta(ref m_Serial, value);
		}

		public override string Compile()
		{
			return String.Format("{{ itemproperty {0} }}", m_Serial);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("itemproperty");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_Serial);
		}
	}

	public class GumpImage : GumpEntry
	{
		private int m_X, m_Y;
		private int m_GumpID;
		private int m_Hue;

		public GumpImage(int x, int y, int gumpID) : this(x, y, gumpID, 0)
		{
		}

		public GumpImage(int x, int y, int gumpID, int hue)
		{
			m_X = x;
			m_Y = y;
			m_GumpID = gumpID;
			m_Hue = hue;
		}

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int GumpID
		{
			get => m_GumpID;
			set => Delta(ref m_GumpID, value);
		}

		public int Hue
		{
			get => m_Hue;
			set => Delta(ref m_Hue, value);
		}

		public override string Compile()
		{
			if (m_Hue == 0)
			{
				return String.Format("{{ gumppic {0} {1} {2} }}", m_X, m_Y, m_GumpID);
			}
			else
			{
				return String.Format("{{ gumppic {0} {1} {2} hue={3} }}", m_X, m_Y, m_GumpID, m_Hue);
			}
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("gumppic");
		private static readonly byte[] m_HueEquals = Gump.StringToBuffer(" hue=");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_GumpID);

			if (m_Hue != 0)
			{
				disp.AppendLayout(m_HueEquals);
				disp.AppendLayoutNS(m_Hue);
			}
		}
	}

	public class GumpImageTileButton : GumpEntry
	{
		//Note, on OSI, The tooltip supports ONLY clilocs as far as I can figure out, and the tooltip ONLY works after the buttonTileArt (as far as I can tell from testing)
		private int m_X, m_Y;
		private int m_ID1, m_ID2;
		private int m_ButtonID;
		private GumpButtonType m_Type;
		private int m_Param;

		private int m_ItemID;
		private int m_Hue;
		private int m_Width;
		private int m_Height;

		private int m_LocalizedTooltip;

		public GumpImageTileButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height) : this(x, y, normalID, pressedID, buttonID, type, param, itemID, hue, width, height, -1)
		{
		}
		public GumpImageTileButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height, int localizedTooltip)
		{
			m_X = x;
			m_Y = y;
			m_ID1 = normalID;
			m_ID2 = pressedID;
			m_ButtonID = buttonID;
			m_Type = type;
			m_Param = param;

			m_ItemID = itemID;
			m_Hue = hue;
			m_Width = width;
			m_Height = height;

			m_LocalizedTooltip = localizedTooltip;
		}

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int NormalID
		{
			get => m_ID1;
			set => Delta(ref m_ID1, value);
		}

		public int PressedID
		{
			get => m_ID2;
			set => Delta(ref m_ID2, value);
		}

		public int ButtonID
		{
			get => m_ButtonID;
			set => Delta(ref m_ButtonID, value);
		}

		public GumpButtonType Type
		{
			get => m_Type;
			set
			{
				if (m_Type != value)
				{
					m_Type = value;

					var parent = Parent;

					if (parent != null)
					{
						parent.Invalidate();
					}
				}
			}
		}

		public int Param
		{
			get => m_Param;
			set => Delta(ref m_Param, value);
		}

		public int ItemID
		{
			get => m_ItemID;
			set => Delta(ref m_ItemID, value);
		}

		public int Hue
		{
			get => m_Hue;
			set => Delta(ref m_Hue, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public int LocalizedTooltip
		{
			get => m_LocalizedTooltip;
			set => m_LocalizedTooltip = value;
		}

		public override string Compile()
		{
			if (m_LocalizedTooltip > 0)
			{
				return String.Format("{{ buttontileart {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} }}{{ tooltip {11} }}", m_X, m_Y, m_ID1, m_ID2, (int)m_Type, m_Param, m_ButtonID, m_ItemID, m_Hue, m_Width, m_Height, m_LocalizedTooltip);
			}
			else
			{
				return String.Format("{{ buttontileart {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} }}", m_X, m_Y, m_ID1, m_ID2, (int)m_Type, m_Param, m_ButtonID, m_ItemID, m_Hue, m_Width, m_Height);
			}
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("buttontileart");
		private static readonly byte[] m_LayoutTooltip = Gump.StringToBuffer(" }{ tooltip");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_ID1);
			disp.AppendLayout(m_ID2);
			disp.AppendLayout((int)m_Type);
			disp.AppendLayout(m_Param);
			disp.AppendLayout(m_ButtonID);

			disp.AppendLayout(m_ItemID);
			disp.AppendLayout(m_Hue);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);

			if (m_LocalizedTooltip > 0)
			{
				disp.AppendLayout(m_LayoutTooltip);
				disp.AppendLayout(m_LocalizedTooltip);
			}
		}
	}

	public class GumpImageTiled : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Width, m_Height;
		private int m_GumpID;

		public GumpImageTiled(int x, int y, int width, int height, int gumpID)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_GumpID = gumpID;
		}

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public int GumpID
		{
			get => m_GumpID;
			set => Delta(ref m_GumpID, value);
		}

		public override string Compile()
		{
			return String.Format("{{ gumppictiled {0} {1} {2} {3} {4} }}", m_X, m_Y, m_Width, m_Height, m_GumpID);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("gumppictiled");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);
			disp.AppendLayout(m_GumpID);
		}
	}

	public class GumpSpriteImage : GumpEntry
	{
		private int m_X, m_Y, m_SX, m_SY;
		private int m_Width, m_Height;
		private int m_GumpID;

		public GumpSpriteImage(int x, int y, int gumpID, int width, int height, int sx, int sy)
		{
			m_X = x;
			m_Y = y;
			m_GumpID = gumpID;
			m_Width = width;
			m_Height = height;
			m_SX = sx;
			m_SY = sy;
		}

		public int X
		{
			get => m_X;

			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;

			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;

			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;

			set => Delta(ref m_Height, value);
		}

		public int GumpID
		{
			get => m_GumpID;

			set => Delta(ref m_GumpID, value);
		}

		public int SX
		{
			get => m_SX;

			set => Delta(ref m_SX, value);
		}

		public int SY
		{
			get => m_SY;

			set => Delta(ref m_SY, value);
		}

		public override string Compile()
		{
			return $"{{ picinpic {m_X} {m_Y} {m_GumpID} {m_Width} {m_Height} {m_SX} {m_SY} }}";
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("picinpic");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_GumpID);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);
			disp.AppendLayout(m_SX);
			disp.AppendLayout(m_SY);
		}
	}

	public class GumpHtml : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Width, m_Height;
		private string m_Text;
		private bool m_Background, m_Scrollbar;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public string Text
		{
			get => m_Text;
			set => Delta(ref m_Text, value);
		}

		public bool Background
		{
			get => m_Background;
			set => Delta(ref m_Background, value);
		}

		public bool Scrollbar
		{
			get => m_Scrollbar;
			set => Delta(ref m_Scrollbar, value);
		}

		public GumpHtml(int x, int y, int width, int height, string text, bool background, bool scrollbar)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_Text = text;
			m_Background = background;
			m_Scrollbar = scrollbar;
		}

		public override string Compile()
		{
			return String.Format("{{ htmlgump {0} {1} {2} {3} {4} {5} {6} }}", m_X, m_Y, m_Width, m_Height, Parent.Intern(m_Text), m_Background ? 1 : 0, m_Scrollbar ? 1 : 0);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("htmlgump");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);
			disp.AppendLayout(Parent.Intern(m_Text));
			disp.AppendLayout(m_Background);
			disp.AppendLayout(m_Scrollbar);
		}
	}

	public enum GumpHtmlLocalizedType
	{
		Plain,
		Color,
		Args
	}

	public class GumpHtmlLocalized : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Width, m_Height;
		private int m_Number;
		private string m_Args;
		private int m_Color;
		private bool m_Background, m_Scrollbar;

		private GumpHtmlLocalizedType m_Type;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public int Number
		{
			get => m_Number;
			set => Delta(ref m_Number, value);
		}

		public string Args
		{
			get => m_Args;
			set => Delta(ref m_Args, value);
		}

		public int Color
		{
			get => m_Color;
			set => Delta(ref m_Color, value);
		}

		public bool Background
		{
			get => m_Background;
			set => Delta(ref m_Background, value);
		}

		public bool Scrollbar
		{
			get => m_Scrollbar;
			set => Delta(ref m_Scrollbar, value);
		}

		public GumpHtmlLocalizedType Type
		{
			get => m_Type;
			set
			{
				if (m_Type != value)
				{
					m_Type = value;

					if (Parent != null)
					{
						Parent.Invalidate();
					}
				}
			}
		}

		public GumpHtmlLocalized(int x, int y, int width, int height, int number, bool background, bool scrollbar)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_Number = number;
			m_Background = background;
			m_Scrollbar = scrollbar;

			m_Type = GumpHtmlLocalizedType.Plain;
		}

		public GumpHtmlLocalized(int x, int y, int width, int height, int number, int color, bool background, bool scrollbar)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_Number = number;
			m_Color = color;
			m_Background = background;
			m_Scrollbar = scrollbar;

			m_Type = GumpHtmlLocalizedType.Color;
		}

		public GumpHtmlLocalized(int x, int y, int width, int height, int number, string args, int color, bool background, bool scrollbar)
		{
			// Are multiple arguments unsupported? And what about non ASCII arguments?

			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_Number = number;
			m_Args = args;
			m_Color = color;
			m_Background = background;
			m_Scrollbar = scrollbar;

			m_Type = GumpHtmlLocalizedType.Args;
		}

		public override string Compile()
		{
			switch (m_Type)
			{
				case GumpHtmlLocalizedType.Plain:
					return String.Format("{{ xmfhtmlgump {0} {1} {2} {3} {4} {5} {6} }}", m_X, m_Y, m_Width, m_Height, m_Number, m_Background ? 1 : 0, m_Scrollbar ? 1 : 0);

				case GumpHtmlLocalizedType.Color:
					return String.Format("{{ xmfhtmlgumpcolor {0} {1} {2} {3} {4} {5} {6} {7} }}", m_X, m_Y, m_Width, m_Height, m_Number, m_Background ? 1 : 0, m_Scrollbar ? 1 : 0, m_Color);

				default: // GumpHtmlLocalizedType.Args
					return String.Format("{{ xmfhtmltok {0} {1} {2} {3} {4} {5} {6} {7} @{8}@ }}", m_X, m_Y, m_Width, m_Height, m_Background ? 1 : 0, m_Scrollbar ? 1 : 0, m_Color, m_Number, m_Args);
			}
		}

		private static readonly byte[] m_LayoutNamePlain = Gump.StringToBuffer("xmfhtmlgump");
		private static readonly byte[] m_LayoutNameColor = Gump.StringToBuffer("xmfhtmlgumpcolor");
		private static readonly byte[] m_LayoutNameArgs = Gump.StringToBuffer("xmfhtmltok");

		public override void AppendTo(IGumpWriter disp)
		{
			switch (m_Type)
			{
				case GumpHtmlLocalizedType.Plain:
					{
						disp.AppendLayout(m_LayoutNamePlain);

						disp.AppendLayout(m_X);
						disp.AppendLayout(m_Y);
						disp.AppendLayout(m_Width);
						disp.AppendLayout(m_Height);
						disp.AppendLayout(m_Number);
						disp.AppendLayout(m_Background);
						disp.AppendLayout(m_Scrollbar);

						break;
					}

				case GumpHtmlLocalizedType.Color:
					{
						disp.AppendLayout(m_LayoutNameColor);

						disp.AppendLayout(m_X);
						disp.AppendLayout(m_Y);
						disp.AppendLayout(m_Width);
						disp.AppendLayout(m_Height);
						disp.AppendLayout(m_Number);
						disp.AppendLayout(m_Background);
						disp.AppendLayout(m_Scrollbar);
						disp.AppendLayout(m_Color);

						break;
					}

				case GumpHtmlLocalizedType.Args:
					{
						disp.AppendLayout(m_LayoutNameArgs);

						disp.AppendLayout(m_X);
						disp.AppendLayout(m_Y);
						disp.AppendLayout(m_Width);
						disp.AppendLayout(m_Height);
						disp.AppendLayout(m_Background);
						disp.AppendLayout(m_Scrollbar);
						disp.AppendLayout(m_Color);
						disp.AppendLayout(m_Number);
						disp.AppendLayout(m_Args);

						break;
					}
			}
		}
	}

	public class GumpCheck : GumpEntry
	{
		private int m_X, m_Y;
		private int m_ID1, m_ID2;
		private bool m_InitialState;
		private int m_SwitchID;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int InactiveID
		{
			get => m_ID1;
			set => Delta(ref m_ID1, value);
		}

		public int ActiveID
		{
			get => m_ID2;
			set => Delta(ref m_ID2, value);
		}

		public bool InitialState
		{
			get => m_InitialState;
			set => Delta(ref m_InitialState, value);
		}

		public int SwitchID
		{
			get => m_SwitchID;
			set => Delta(ref m_SwitchID, value);
		}

		public GumpCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
		{
			m_X = x;
			m_Y = y;
			m_ID1 = inactiveID;
			m_ID2 = activeID;
			m_InitialState = initialState;
			m_SwitchID = switchID;
		}

		public override string Compile()
		{
			return String.Format("{{ checkbox {0} {1} {2} {3} {4} {5} }}", m_X, m_Y, m_ID1, m_ID2, m_InitialState ? 1 : 0, m_SwitchID);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("checkbox");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_ID1);
			disp.AppendLayout(m_ID2);
			disp.AppendLayout(m_InitialState);
			disp.AppendLayout(m_SwitchID);

			disp.Switches++;
		}
	}

	public class GumpRadio : GumpEntry
	{
		private int m_X, m_Y;
		private int m_ID1, m_ID2;
		private bool m_InitialState;
		private int m_SwitchID;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int InactiveID
		{
			get => m_ID1;
			set => Delta(ref m_ID1, value);
		}

		public int ActiveID
		{
			get => m_ID2;
			set => Delta(ref m_ID2, value);
		}

		public bool InitialState
		{
			get => m_InitialState;
			set => Delta(ref m_InitialState, value);
		}

		public int SwitchID
		{
			get => m_SwitchID;
			set => Delta(ref m_SwitchID, value);
		}

		public GumpRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
		{
			m_X = x;
			m_Y = y;
			m_ID1 = inactiveID;
			m_ID2 = activeID;
			m_InitialState = initialState;
			m_SwitchID = switchID;
		}

		public override string Compile()
		{
			return String.Format("{{ radio {0} {1} {2} {3} {4} {5} }}", m_X, m_Y, m_ID1, m_ID2, m_InitialState ? 1 : 0, m_SwitchID);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("radio");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_ID1);
			disp.AppendLayout(m_ID2);
			disp.AppendLayout(m_InitialState);
			disp.AppendLayout(m_SwitchID);

			disp.Switches++;
		}
	}

	public class GumpLabel : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Hue;
		private string m_Text;

		public GumpLabel(int x, int y, int hue, string text)
		{
			m_X = x;
			m_Y = y;
			m_Hue = hue;
			m_Text = text;
		}

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Hue
		{
			get => m_Hue;
			set => Delta(ref m_Hue, value);
		}

		public string Text
		{
			get => m_Text;
			set => Delta(ref m_Text, value);
		}

		public override string Compile()
		{
			return String.Format("{{ text {0} {1} {2} {3} }}", m_X, m_Y, m_Hue, Parent.Intern(m_Text));
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("text");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_Hue);
			disp.AppendLayout(Parent.Intern(m_Text));
		}
	}

	public class GumpLabelCropped : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Width, m_Height;
		private int m_Hue;
		private string m_Text;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public int Hue
		{
			get => m_Hue;
			set => Delta(ref m_Hue, value);
		}

		public string Text
		{
			get => m_Text;
			set => Delta(ref m_Text, value);
		}

		public GumpLabelCropped(int x, int y, int width, int height, int hue, string text)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_Hue = hue;
			m_Text = text;
		}

		public override string Compile()
		{
			return String.Format("{{ croppedtext {0} {1} {2} {3} {4} {5} }}", m_X, m_Y, m_Width, m_Height, m_Hue, Parent.Intern(m_Text));
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("croppedtext");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);
			disp.AppendLayout(m_Hue);
			disp.AppendLayout(Parent.Intern(m_Text));
		}
	}

	public class GumpTextEntry : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Width, m_Height;
		private int m_Hue;
		private int m_EntryID;
		private string m_InitialText;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public int Hue
		{
			get => m_Hue;
			set => Delta(ref m_Hue, value);
		}

		public int EntryID
		{
			get => m_EntryID;
			set => Delta(ref m_EntryID, value);
		}

		public string InitialText
		{
			get => m_InitialText;
			set => Delta(ref m_InitialText, value);
		}

		public GumpTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_Hue = hue;
			m_EntryID = entryID;
			m_InitialText = initialText;
		}

		public override string Compile()
		{
			return String.Format("{{ textentry {0} {1} {2} {3} {4} {5} {6} }}", m_X, m_Y, m_Width, m_Height, m_Hue, m_EntryID, Parent.Intern(m_InitialText));
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("textentry");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);
			disp.AppendLayout(m_Hue);
			disp.AppendLayout(m_EntryID);
			disp.AppendLayout(Parent.Intern(m_InitialText));

			disp.TextEntries++;
		}
	}

	public class GumpTextEntryLimited : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Width, m_Height;
		private int m_Hue;
		private int m_EntryID;
		private string m_InitialText;
		private int m_Size;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public int Hue
		{
			get => m_Hue;
			set => Delta(ref m_Hue, value);
		}

		public int EntryID
		{
			get => m_EntryID;
			set => Delta(ref m_EntryID, value);
		}

		public string InitialText
		{
			get => m_InitialText;
			set => Delta(ref m_InitialText, value);
		}

		public int Size
		{
			get => m_Size;
			set => Delta(ref m_Size, value);
		}

		public GumpTextEntryLimited(int x, int y, int width, int height, int hue, int entryID, string initialText, int size)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_Hue = hue;
			m_EntryID = entryID;
			m_InitialText = initialText;
			m_Size = size;
		}

		public override string Compile()
		{
			return String.Format("{{ textentrylimited {0} {1} {2} {3} {4} {5} {6} {7} }}", m_X, m_Y, m_Width, m_Height, m_Hue, m_EntryID, Parent.Intern(m_InitialText), m_Size);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("textentrylimited");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);
			disp.AppendLayout(m_Hue);
			disp.AppendLayout(m_EntryID);
			disp.AppendLayout(Parent.Intern(m_InitialText));
			disp.AppendLayout(m_Size);

			disp.TextEntries++;
		}
	}

	public class TextRelay
	{
		private readonly int m_EntryID;
		private readonly string m_Text;

		public TextRelay(int entryID, string text)
		{
			m_EntryID = entryID;
			m_Text = text;
		}

		public int EntryID => m_EntryID;

		public string Text => m_Text;
	}

	public class RelayInfo
	{
		private readonly int m_ButtonID;
		private readonly int[] m_Switches;
		private readonly TextRelay[] m_TextEntries;

		public RelayInfo(int buttonID, int[] switches, TextRelay[] textEntries)
		{
			m_ButtonID = buttonID;
			m_Switches = switches;
			m_TextEntries = textEntries;
		}

		public int ButtonID => m_ButtonID;

		public int[] Switches => m_Switches;

		public TextRelay[] TextEntries => m_TextEntries;

		public bool IsSwitched(int switchID)
		{
			for (var i = 0; i < m_Switches.Length; ++i)
			{
				if (m_Switches[i] == switchID)
				{
					return true;
				}
			}

			return false;
		}

		public TextRelay GetTextEntry(int entryID)
		{
			for (var i = 0; i < m_TextEntries.Length; ++i)
			{
				if (m_TextEntries[i].EntryID == entryID)
				{
					return m_TextEntries[i];
				}
			}

			return null;
		}
	}

	public enum GumpButtonType
	{
		Page = 0,
		Reply = 1
	}

	public class GumpButton : GumpEntry
	{
		private int m_X, m_Y;
		private int m_ID1, m_ID2;
		private int m_ButtonID;
		private GumpButtonType m_Type;
		private int m_Param;

		public GumpButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
		{
			m_X = x;
			m_Y = y;
			m_ID1 = normalID;
			m_ID2 = pressedID;
			m_ButtonID = buttonID;
			m_Type = type;
			m_Param = param;
		}

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int NormalID
		{
			get => m_ID1;
			set => Delta(ref m_ID1, value);
		}

		public int PressedID
		{
			get => m_ID2;
			set => Delta(ref m_ID2, value);
		}

		public int ButtonID
		{
			get => m_ButtonID;
			set => Delta(ref m_ButtonID, value);
		}

		public GumpButtonType Type
		{
			get => m_Type;
			set
			{
				if (m_Type != value)
				{
					m_Type = value;

					var parent = Parent;

					if (parent != null)
					{
						parent.Invalidate();
					}
				}
			}
		}

		public int Param
		{
			get => m_Param;
			set => Delta(ref m_Param, value);
		}

		public override string Compile()
		{
			return String.Format("{{ button {0} {1} {2} {3} {4} {5} {6} }}", m_X, m_Y, m_ID1, m_ID2, (int)m_Type, m_Param, m_ButtonID);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("button");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_ID1);
			disp.AppendLayout(m_ID2);
			disp.AppendLayout((int)m_Type);
			disp.AppendLayout(m_Param);
			disp.AppendLayout(m_ButtonID);
		}
	}

	public class GumpGroup : GumpEntry
	{
		private int m_Group;

		public GumpGroup(int group)
		{
			m_Group = group;
		}

		public int Group
		{
			get => m_Group;
			set => Delta(ref m_Group, value);
		}

		public override string Compile()
		{
			return String.Format("{{ group {0} }}", m_Group);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("group");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_Group);
		}
	}

	public class GumpTooltip : GumpEntry
	{
		private int m_Number;
		private string m_Args;

		public GumpTooltip(int number)
			: this(number, null)
		{
		}

		public GumpTooltip(int number, string args)
		{
			m_Number = number;
			m_Args = args;
		}

		public int Number
		{
			get => m_Number;
			set => Delta(ref m_Number, value);
		}

		public string Args
		{
			get => m_Args;
			set => Delta(ref m_Args, value);
		}

		public override string Compile()
		{
			if (String.IsNullOrEmpty(m_Args))
			{
				return $"{{ tooltip {m_Number} }}";
			}

			return $"{{ tooltip {m_Number} @{m_Args}@ }}";
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("tooltip");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_Number);

			if (!String.IsNullOrEmpty(m_Args))
			{
				disp.AppendLayout(m_Args);
			}
		}
	}

	public class ECHandleInput : GumpEntry
	{
		public ECHandleInput()
		{
		}

		public override string Compile()
		{
			return $"{{ echandleinput }}";
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("echandleinput");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
		}
	}
}