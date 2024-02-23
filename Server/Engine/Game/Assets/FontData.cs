#region References
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using PF = System.Drawing.Imaging.PixelFormat;
#endregion

namespace Server
{
	public sealed class FontData
	{
		public const PF PixelFormat = PF.Format16bppArgb1555;

		public static readonly Size DefaultCharSize = new(8, 10);

		public static FontCache Ascii { get; private set; }
		public static FontCache Unicode { get; private set; }

		public static void Configure()
		{
			Ascii ??= new FontCache(FontEncoding.Ascii);
			Unicode ??= new FontCache(FontEncoding.Unicode);
		}

		public static FontInfo GetFont(FontEncoding enc, byte id)
		{
			return FontCache.GetFont(enc, id);
		}

		public static FontInfo GetAscii(byte id)
		{
			return FontCache.GetFont(FontEncoding.Ascii, id);
		}

		public static FontInfo GetUnicode(byte id)
		{
			return FontCache.GetFont(FontEncoding.Unicode, id);
		}

		public static Bitmap GetImage(FontInfo font, char c)
		{
			return FontCache.GetImage(font, c);
		}

		public static Bitmap GetAsciiImage(byte font, char c)
		{
			return Ascii.GetImage(font, c);
		}

		public static Bitmap GetUnicodeImage(byte font, char c)
		{
			return Unicode.GetImage(font, c);
		}

		public static int GetAsciiWidth(byte font, string text)
		{
			return Ascii.GetWidth(font, text);
		}

		public static int GetAsciiHeight(byte font, string text)
		{
			return Ascii.GetHeight(font, text);
		}

		public static Size GetAsciiSize(byte font, string text)
		{
			return Ascii.GetSize(font, text);
		}

		public static int GetAsciiWidth(byte font, params string[] lines)
		{
			return Ascii.GetWidth(font, lines);
		}

		public static int GetAsciiHeight(byte font, params string[] lines)
		{
			return Ascii.GetHeight(font, lines);
		}

		public static Size GetAsciiSize(byte font, params string[] lines)
		{
			return Ascii.GetSize(font, lines);
		}

		public static int GetUnicodeWidth(byte font, string text)
		{
			return Unicode.GetWidth(font, text);
		}

		public static int GetUnicodeHeight(byte font, string text)
		{
			return Unicode.GetHeight(font, text);
		}

		public static Size GetUnicodeSize(byte font, string text)
		{
			return Unicode.GetSize(font, text);
		}

		public static int GetUnicodeWidth(byte font, params string[] lines)
		{
			return Unicode.GetWidth(font, lines);
		}

		public static int GetUnicodeHeight(byte font, params string[] lines)
		{
			return Unicode.GetHeight(font, lines);
		}

		public static Size GetUnicodeSize(byte font, params string[] lines)
		{
			return Unicode.GetSize(font, lines);
		}
	}

	public sealed class FontCache
	{
		private static readonly byte[] _EmptyBuffer = Array.Empty<byte>();

		private static readonly FontInfo[] _Ascii = new FontInfo[10];
		private static readonly FontInfo[] _Unicode = new FontInfo[13];

		private static readonly FontChar[][][] _Chars = new FontChar[2][][];

		private static string FindFontFile(string file, params object[] args)
		{
			if (args?.Length > 0)
			{
				file = String.Format(file, args);
			}

			var path = Path.Combine(Core.BaseDirectory, "Data", "Fonts", file);

			if (!File.Exists(path))
			{
				path = Core.FindDataFile(file);
			}

			return path;
		}

		private static Bitmap NewEmptyImage()
		{
			return new Bitmap(FontData.DefaultCharSize.Width, FontData.DefaultCharSize.Height, FontData.PixelFormat);
		}

		private static FontChar NewEmptyChar(FontEncoding enc)
		{
			return new FontChar(enc, 0, 0, NewEmptyImage());
		}

		private static FontInfo Instantiate(FontEncoding enc, byte id)
		{
			int charsWidth = 0, charsHeight = 0;

			var list = _Chars[(byte)enc][id];

			var i = list.Length;

			while (--i >= 0)
			{
				charsWidth = Math.Max(charsWidth, list[i].XOffset + list[i].Width);
				charsHeight = Math.Max(charsHeight, list[i].YOffset + list[i].Height);
			}

			return new FontInfo(enc, id, 1, 4, (byte)charsWidth, (byte)charsHeight, list);
		}

		private static FontInfo LoadAscii(byte id)
		{
			if (id >= _Ascii.Length)
			{
				return null;
			}

			const FontEncoding enc = FontEncoding.Ascii;

			var idx = (byte)enc;

			var fonts = _Chars[idx] ??= new FontChar[_Ascii.Length][];

			if (fonts[id] != null)
			{
				return _Ascii[id] ??= Instantiate(enc, id);
			}

			var chars = fonts[id] = new FontChar[Byte.MaxValue + 1];

			var path = FindFontFile("fonts.mul");

			if (!File.Exists(path))
			{
				Array.Fill(chars, NewEmptyChar(enc));

				return _Ascii[id] ??= Instantiate(enc, id);
			}

			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (var bin = new BinaryReader(fs))
				{
					for (var i = 0; i <= id; i++)
					{
						bin.ReadByte(); // header

						if (i == id)
						{
							for (var c = 0; c < 32; c++)
							{
								chars[c] = NewEmptyChar(enc);
							}
						}

						for (var c = 32; c < chars.Length; c++)
						{
							var width = bin.ReadByte();
							var height = bin.ReadByte();

							bin.ReadByte(); // unk

							if (i == id)
							{
								var buffer = _EmptyBuffer;

								if (width * height > 0)
								{
									buffer = bin.ReadBytes(width * height * 2);
								}

								chars[c] = new FontChar(enc, 0, 0, GetImage(width, height, buffer, enc));
							}
							else
							{
								bin.BaseStream.Seek(width * height * 2, SeekOrigin.Current);
							}
						}
					}
				}
			}

			return _Ascii[id] ??= Instantiate(enc, id);
		}

		private static FontInfo LoadUnicode(byte id)
		{
			if (id >= _Unicode.Length)
			{
				return null;
			}

			const FontEncoding enc = FontEncoding.Unicode;

			var idx = (byte)enc;

			var fonts = _Chars[idx] ??= new FontChar[_Unicode.Length][];

			if (fonts[id] != null)
			{
				return _Unicode[id] ??= Instantiate(enc, id);
			}

			var chars = fonts[id] = new FontChar[UInt16.MaxValue + 1];

			var path = FindFontFile("unifont{0:#}.mul", id);

			if (!File.Exists(path))
			{
				Array.Fill(chars, NewEmptyChar(enc));

				return _Unicode[id] ??= Instantiate(enc, id);
			}

			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (var bin = new BinaryReader(fs))
				{
					for (int c = 0, o; c < chars.Length; c++)
					{
						fs.Seek(c * 4, SeekOrigin.Begin);

						o = bin.ReadInt32();

						if (o <= 0 || o >= fs.Length)
						{
							chars[c] = NewEmptyChar(enc);
							continue;
						}

						fs.Seek(o, SeekOrigin.Begin);

						var x = bin.ReadSByte(); // x-offset
						var y = bin.ReadSByte(); // y-offset

						var width = bin.ReadByte();
						var height = bin.ReadByte();

						var buffer = _EmptyBuffer;

						if (width * height > 0)
						{
							buffer = bin.ReadBytes(height * (((width - 1) / 8) + 1));
						}

						chars[c] = new FontChar(enc, x, y, GetImage(width, height, buffer, enc));
					}
				}
			}

			return _Unicode[id] ??= Instantiate(enc, id);
		}

		private static unsafe Bitmap GetImage(int width, int height, byte[] buffer, FontEncoding enc)
		{
			if (width * height <= 0 || buffer == null || buffer.Length == 0)
			{
				return NewEmptyImage();
			}

			var image = new Bitmap(width, height, FontData.PixelFormat);
			var bound = new Rectangle(0, 0, width, height);
			var data = image.LockBits(bound, ImageLockMode.WriteOnly, FontData.PixelFormat);

			var index = 0;

			var line = (ushort*)data.Scan0;
			var delta = data.Stride >> 1;

			int x, y;
			ushort pixel;

			for (y = 0; y < height; y++, line += delta)
			{
				var cur = line;

				if (cur == null)
				{
					continue;
				}

				for (x = 0; x < width; x++)
				{
					pixel = 0;

					if (enc > 0)
					{
						index = x / 8 + y * ((width + 7) / 8);
					}

					if (index < buffer.Length)
					{
						if (enc > 0)
						{
							pixel = buffer[index];
						}
						else
						{
							pixel = (ushort)(buffer[index++] | (buffer[index++] << 8));
						}
					}

					if (enc > 0)
					{
						pixel &= (ushort)(1 << (7 - (x % 8)));
					}

					if (pixel == 0)
					{
						cur[x] = 0;
					}
					else if (enc > 0)
					{
						cur[x] = 0x8000;
					}
					else
					{
						cur[x] = (ushort)(pixel ^ 0x8000);
					}
				}
			}

			image.UnlockBits(data);

			return image;
		}

		public static FontInfo GetFont(FontEncoding enc, byte id)
		{
			switch (enc)
			{
				case FontEncoding.Ascii: return LoadAscii(id);
				case FontEncoding.Unicode: return LoadUnicode(id);
			}

			return null;
		}

		public static FontInfo GetAscii(byte id)
		{
			return GetFont(FontEncoding.Ascii, id);
		}

		public static FontInfo GetUnicode(byte id)
		{
			return GetFont(FontEncoding.Unicode, id);
		}

		public static Bitmap GetImage(FontInfo font, char c)
		{
			return font[c].GetImage();
		}

		public FontEncoding Encoding { get; private set; }

		public FontInfo this[int id] => GetFont(Encoding, (byte)id);

		public int Count { get; private set; }

		public byte DefaultID { get; private set; }

		public FontCache(FontEncoding enc)
		{
			Encoding = enc;

			switch (Encoding)
			{
				case FontEncoding.Ascii:
					{
						Count = _Ascii.Length;
						DefaultID = 3;
					}
					break;
				case FontEncoding.Unicode:
					{
						Count = _Unicode.Length;
						DefaultID = 1;
					}
					break;
			}
		}

		public Bitmap GetImage(byte font, char c)
		{
			if (font >= Count)
			{
				font = DefaultID;
			}

			return GetImage(this[font], c);
		}

		public int GetWidth(byte font, string text)
		{
			if (font >= Count)
			{
				font = DefaultID;
			}

			return this[font].GetWidth(text);
		}

		public int GetHeight(byte font, string text)
		{
			if (font >= Count)
			{
				font = DefaultID;
			}

			return this[font].GetHeight(text);
		}

		public Size GetSize(byte font, string text)
		{
			if (font >= Count)
			{
				font = DefaultID;
			}

			return this[font].GetSize(text);
		}

		public int GetWidth(byte font, params string[] lines)
		{
			if (font >= Count)
			{
				font = DefaultID;
			}

			return this[font].GetWidth(lines);
		}

		public int GetHeight(byte font, params string[] lines)
		{
			if (font >= Count)
			{
				font = DefaultID;
			}

			return this[font].GetHeight(lines);
		}

		public Size GetSize(byte font, params string[] lines)
		{
			if (font >= Count)
			{
				font = DefaultID;
			}

			return this[font].GetSize(lines);
		}
	}

	public sealed class FontInfo
	{
		public FontEncoding Encoding { get; private set; }

		public byte ID { get; private set; }

		public byte MaxCharWidth { get; private set; }
		public byte MaxCharHeight { get; private set; }

		public byte CharSpacing { get; private set; }
		public byte LineSpacing { get; private set; }

		public byte LineHeight { get; private set; }

		public FontChar[] Chars { get; private set; }

		public int Length => Chars.Length;

		public FontChar this[char c] => Chars[c % Length];
		public FontChar this[int i] => Chars[i % Length];

		public FontInfo(FontEncoding enc, byte id, byte charSpacing, byte lineSpacing, byte charsWidth, byte charsHeight, FontChar[] chars)
		{
			Encoding = enc;

			ID = id;

			CharSpacing = charSpacing;
			LineSpacing = lineSpacing;
			MaxCharWidth = charsWidth;
			MaxCharHeight = charsHeight;

			Chars = chars;
		}

		public int GetWidth(string value)
		{
			return GetSize(value).Width;
		}

		public int GetHeight(string value)
		{
			return GetSize(value).Height;
		}

		public Size GetSize(string value)
		{
			var lines = value.Split('\n');

			if (lines.Length == 0)
			{
				lines = new[] { value };
			}

			return GetSize(lines);
		}

		public int GetWidth(params string[] lines)
		{
			return GetSize(lines).Width;
		}

		public int GetHeight(params string[] lines)
		{
			return GetSize(lines).Height;
		}

		public Size GetSize(params string[] lines)
		{
			var w = 0;
			var h = 0;

			var space = Chars[' '];

			FontChar ci;

			foreach (var line in lines.SelectMany(o => o.Contains('\n') ? o.Split('\n') : Enumerable.Repeat(o, 1)))
			{
				var lw = 0;
				var lh = 0;

				foreach (var c in line)
				{
					if (c == '\t')
					{
						lw += (CharSpacing + space.Width) * 4;
						continue;
					}

					ci = this[c];

					if (ci == null)
					{
						lw += CharSpacing + space.Width;
						continue;
					}

					lw += CharSpacing + ci.XOffset + ci.Width;
					lh = Math.Max(lh, ci.YOffset + ci.Height);
				}

				w = Math.Max(w, lw);
				h += lh + LineSpacing;
			}

			return new Size(w, h);
		}

		public override string ToString()
		{
			return String.Format("({0}, {1}, {2})", Encoding, ID, Length);
		}
	}

	public sealed class FontChar
	{
		public Bitmap Image { get; private set; }

		public FontEncoding Encoding { get; private set; }

		public sbyte XOffset { get; private set; }
		public sbyte YOffset { get; private set; }

		public byte Width { get; private set; }
		public byte Height { get; private set; }

		public FontChar(FontEncoding enc, sbyte ox, sbyte oy, Bitmap image)
		{
			Encoding = enc;

			XOffset = ox;
			YOffset = oy;

			Image = image;

			Width = (byte)Image.Width;
			Height = (byte)Image.Height;
		}

		public Bitmap GetImage()
		{
			return GetImage(false);
		}

		public Bitmap GetImage(bool fill)
		{
			return GetImage(fill, 0x7FFF);
		}

		public Bitmap GetImage(bool fill, ushort bgColor)
		{
			return GetImage(fill, bgColor, 0x0000);
		}

		public unsafe Bitmap GetImage(bool fill, ushort bgColor, ushort textColor)
		{
			if (Width * Height <= 0)
			{
				return null;
			}

			var image = new Bitmap(Width, Height, FontData.PixelFormat);

			var bound = new Rectangle(0, 0, Width, Height);

			var dataSrc = Image.LockBits(bound, ImageLockMode.ReadOnly, FontData.PixelFormat);
			var lineSrc = (ushort*)dataSrc.Scan0;
			var deltaSrc = dataSrc.Stride >> 1;

			var dataTrg = image.LockBits(bound, ImageLockMode.WriteOnly, FontData.PixelFormat);
			var lineTrg = (ushort*)dataTrg.Scan0;
			var deltaTrg = dataTrg.Stride >> 1;

			int x, y;

			for (y = 0; y < Height; y++, lineSrc += deltaSrc, lineTrg += deltaTrg)
			{
				var source = lineSrc;
				var target = lineTrg;

				if (source == null || target == null)
				{
					continue;
				}

				for (x = 0; x < Width; x++)
				{
					if (source[x] != 0)
					{
						target[x] = textColor;
					}
					else if (fill)
					{
						target[x] = bgColor;
					}
				}
			}

			Image.UnlockBits(dataSrc);
			image.UnlockBits(dataTrg);

			return image;
		}
	}

	public enum FontEncoding : byte
	{
		Ascii,
		Unicode
	}

	public enum FontStyle
	{
		Big = 0,
		Normal = 1,
		Small = 2
	}
}