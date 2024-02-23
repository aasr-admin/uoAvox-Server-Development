using Server.Network;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
	public static partial class Core
	{
		private static bool m_Crashed;
		private static Thread m_TimerThread;
		private static string m_BaseDirectory;
		private static string m_ExePath;
		private static bool m_Cache = true;
		private static bool m_Profiling;
		private static DateTime m_ProfileStart;
		private static TimeSpan m_ProfileTime;

		private static volatile MessagePump m_MessagePump = new();

		public static MessagePump MessagePump => m_MessagePump;

		public static event Action Slice;

		public static bool Profiling
		{
			get => m_Profiling;
			set
			{
				if (m_Profiling == value)
				{
					return;
				}

				m_Profiling = value;

				if (m_ProfileStart > DateTime.MinValue)
				{
					m_ProfileTime += DateTime.UtcNow - m_ProfileStart;
				}

				m_ProfileStart = m_Profiling ? DateTime.UtcNow : DateTime.MinValue;
			}
		}

		public static TimeSpan ProfileTime
		{
			get
			{
				if (m_ProfileStart > DateTime.MinValue)
				{
					return m_ProfileTime + (DateTime.UtcNow - m_ProfileStart);
				}

				return m_ProfileTime;
			}
		}

		public static bool Service { get; private set; }
		public static bool Debug { get; private set; }

		public static bool HaltOnWarning { get; private set; }

		public static Assembly Assembly { get; set; }
		public static Version Version => Assembly.GetName().Version;
		public static Process Process { get; private set; }
		public static Thread Thread { get; private set; }
		public static MultiTextWriter MultiConsoleOut { get; private set; }

		/* 
		 * DateTime.Now and DateTime.UtcNow are based on actual system clock time.
		 * The resolution is acceptable but large clock jumps are possible and cause issues.
		 * GetTickCount and GetTickCount64 have poor resolution.
		 * GetTickCount64 is unavailable on Windows XP and Windows Server 2003.
		 * Stopwatch.GetTimestamp() (QueryPerformanceCounter) is high resolution, but
		 * somewhat expensive to call because of its defference to DateTime.Now,
		 * which is why Stopwatch has been used to verify HRT before calling GetTimestamp(),
		 * enabling the usage of DateTime.UtcNow instead.
		 */

		private static readonly bool _HighRes = Stopwatch.IsHighResolution;

		private static readonly double _HighFrequency = 1000.0 / Stopwatch.Frequency;
		private static readonly double _LowFrequency = 1000.0 / TimeSpan.TicksPerSecond;

		private static bool _UseHRT;

		public static bool UsingHighResolutionTiming => _UseHRT && _HighRes && !Unix;

		public static long TickCount => (long)Ticks;

		public static double Ticks
		{
			get
			{
				if (_UseHRT && _HighRes && !Unix)
				{
					return Stopwatch.GetTimestamp() * _HighFrequency;
				}

				return DateTime.UtcNow.Ticks * _LowFrequency;
			}
		}

		public static readonly bool Is64Bit = Environment.Is64BitProcess;

		public static bool MultiProcessor { get; private set; }
		public static int ProcessorCount { get; private set; }

		public static bool Unix { get; private set; }

		public static HashSet<string> DataDirectories { get; } = new HashSet<string>();

		public static string DataDirectory
		{
			get => DataDirectories.FirstOrDefault();
			set
			{
				var old = DataDirectory;

				if (old != value)
				{
					if (value == null)
					{
						_ = DataDirectories.Remove(old);
					}
					else if (!String.IsNullOrWhiteSpace(value) && File.Exists(Path.Combine(value, "client.exe")))
					{
						_ = DataDirectories.Remove(old);
						_ = DataDirectories.Add(value);
					}
				}
			}
		}

		private static void InitDataDirectories()
		{
			var path = DataDirectory;

			if (!String.IsNullOrWhiteSpace(path) && File.Exists(Path.Combine(path, "client.exe")))
			{
				_ = DataDirectories.Add(path);
			}

			while (DataDirectories.Count == 0 && !Service)
			{
				Console.WriteLine("Core: Enter a path to Ultima Online:");

				path = Console.ReadLine();

				if (!String.IsNullOrWhiteSpace(path) && File.Exists(Path.Combine(path, "client.exe")))
				{
					if (DataDirectories.Add(path))
					{
						Console.WriteLine("Core: Ultima Online path has been updated...");
					}

					return;
				}

				Console.WriteLine("Core: Invalid path...");
			}
		}

		public static void DisplayDataDirectories()
		{
			Console.WriteLine();
			Console.WriteLine($"Core: Data Paths: {String.Join("\n > ", DataDirectories)}");
			Console.WriteLine();
		}

		public static string FindDataFile(string path)
		{
			foreach (var p in DataDirectories)
			{
				var fullPath = Path.Combine(p, path);

				if (File.Exists(fullPath))
				{
					return fullPath;
				}
			}

			var dataPath = Path.Combine(BaseDirectory, "Client");
			var fileName = Path.GetFileName(path);

			foreach (var file in Directory.EnumerateFiles(dataPath, fileName, SearchOption.AllDirectories))
			{
				if (Insensitive.Equals(Path.GetFileName(path), fileName))
				{
					return file;
				}
			}

			return null;
		}

		public static string FindDataFile(string format, params object[] args)
		{
			return FindDataFile(String.Format(format, args));
		}

		#region Expansions

		public static Expansion Expansion { get; set; }

		public static bool T2A => Expansion >= Expansion.T2A;

		public static bool UOR => Expansion >= Expansion.UOR;

		public static bool UOTD => Expansion >= Expansion.UOTD;

		public static bool LBR => Expansion >= Expansion.LBR;

		public static bool AOS => Expansion >= Expansion.AOS;

		public static bool SE => Expansion >= Expansion.SE;

		public static bool ML => Expansion >= Expansion.ML;

		public static bool SA => Expansion >= Expansion.SA;

		public static bool HS => Expansion >= Expansion.HS;

		public static bool TOL => Expansion >= Expansion.TOL;

		#endregion

		public static string ExePath => m_ExePath ??= Assembly.Location;

		public static string BaseDirectory
		{
			get
			{
				if (m_BaseDirectory == null)
				{
					try
					{
						m_BaseDirectory = ExePath;

						if (m_BaseDirectory.Length > 0)
						{
							m_BaseDirectory = Path.GetDirectoryName(m_BaseDirectory);
						}
					}
					catch
					{
						m_BaseDirectory = "";
					}
				}

				return m_BaseDirectory;
			}
		}

		public static string CurrentSavesDirectory => Path.Combine(BaseDirectory, "Saves");

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Console.WriteLine(e.IsTerminating ? "Error:" : "Warning:");
			Console.WriteLine(e.ExceptionObject);

			if (e.IsTerminating)
			{
				m_Crashed = true;

				var close = false;

				try
				{
					var args = new CrashedEventArgs(e.ExceptionObject as Exception);

					EventSink.InvokeCrashed(args);

					close = args.Close;
				}
				catch
				{
				}

				if (!close && !Service)
				{
					try
					{
						m_MessagePump?.Dispose();
						m_MessagePump = null;
					}
					catch
					{
					}

					Console.WriteLine("This exception is fatal, press return to exit");
					_ = Console.ReadLine();
				}

				Kill();
			}
		}

		internal enum ConsoleEventType
		{
			CTRL_C_EVENT,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

		internal delegate bool ConsoleEventHandler(ConsoleEventType type);
		internal static ConsoleEventHandler m_ConsoleEventHandler;

		internal partial class UnsafeNativeMethods
		{
			[LibraryImport("Kernel32")]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static partial bool SetConsoleCtrlHandler(ConsoleEventHandler callback, [MarshalAs(UnmanagedType.Bool)] bool add);
		}

		private static bool OnConsoleEvent(ConsoleEventType type)
		{
			if (World.Saving || (Service && type == ConsoleEventType.CTRL_LOGOFF_EVENT))
			{
				return true;
			}

			Kill(); //Kill -> HandleClosed will handle waiting for the completion of flushing to disk

			return true;
		}

		private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			HandleClosed();
		}

		public static bool Closing { get; private set; }

		private static int m_CycleIndex = 1;
		private static readonly float[] m_CyclesPerSecond = new float[100];

		public static float CyclesPerSecond => m_CyclesPerSecond[(m_CycleIndex - 1) % m_CyclesPerSecond.Length];

		public static float AverageCPS => m_CyclesPerSecond.Take(m_CycleIndex).Average();

		public static void Kill()
		{
			Kill(false);
		}

		public static void Kill(bool restart)
		{
			HandleClosed();

			if (restart)
			{
				_ = Process.Start(ExePath, Arguments);
			}

			Process.Kill();
		}

		private static void HandleClosed()
		{
			if (Closing)
			{
				return;
			}

			Closing = true;

			Console.WriteLine("Exiting...");

			World.WaitForWriteCompletion();

			if (!m_Crashed)
			{
				EventSink.InvokeShutdown(new ShutdownEventArgs());
			}

			Timer.TimerThread.Set();

			Console.WriteLine("done");
		}

		private static readonly AutoResetEvent m_Signal = new(true);

		public static void Set() { _ = m_Signal.Set(); }

		public static void Start(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

			foreach (var a in args)
			{
				if (Insensitive.Equals(a, "-debug"))
				{
					Debug = true;
				}
				else if (Insensitive.Equals(a, "-service"))
				{
					Service = true;
				}
				else if (Insensitive.Equals(a, "-profile"))
				{
					Profiling = true;
				}
				else if (Insensitive.Equals(a, "-nocache"))
				{
					m_Cache = false;
				}
				else if (Insensitive.Equals(a, "-haltonwarning"))
				{
					HaltOnWarning = true;
				}
				else if (Insensitive.Equals(a, "-usehrt"))
				{
					_UseHRT = true;
				}
			}

			try
			{
				if (Service)
				{
					var path = Path.Combine("Export", "Logs");

					if (!Directory.Exists(path))
					{
						_ = Directory.CreateDirectory(path);
					}

					path = Path.Combine(path, "Console.log");

					Console.SetOut(MultiConsoleOut = new MultiTextWriter(new FileLogger(path)));
				}
				else
				{
					Console.SetOut(MultiConsoleOut = new MultiTextWriter(Console.Out));
				}
			}
			catch
			{
			}

			Thread = Thread.CurrentThread;
			Process = Process.GetCurrentProcess();
			Assembly = Assembly.GetEntryAssembly();

			if (Thread != null)
			{
				Thread.Name = "Core Thread";
			}

			if (BaseDirectory.Length > 0)
			{
				Directory.SetCurrentDirectory(BaseDirectory);
			}

			m_TimerThread = new Thread(Timer.TimerThread.TimerMain)
			{
				Name = "Timer Thread"
			};

			var ver = Assembly.GetName().Version;

			#region uoAvos Console Application Window

			// Added to help future code support on forums, as a 'check' people can ask for to it see if they recompiled core or not
			Console.WriteLine("uoAvos - [https://github.com/aasr-admin/uoAvos-Development] Version {0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
			Console.WriteLine("Core: Running on .NET Version {0}.{1}.{2}", Environment.Version.Major, Environment.Version.Minor, Environment.Version.Build);

			#endregion

			var s = Arguments;

			if (s.Length > 0)
			{
				Console.WriteLine($"Core: Running with arguments: {s}");
			}

			ProcessorCount = Environment.ProcessorCount;

			if (ProcessorCount > 1)
			{
				MultiProcessor = true;
			}

			if (MultiProcessor || Is64Bit)
			{
				Console.WriteLine("Core: Optimizing for {0} {2}processor{1}", ProcessorCount, ProcessorCount == 1 ? "" : "s", Is64Bit ? "64-bit " : "");
			}

			var platform = (int)Environment.OSVersion.Platform;

			// MS 4, MONO 128
			if (platform is 4 or 128)
			{
				Unix = true;
				Console.WriteLine("Core: Unix environment detected");
			}
			else
			{
				m_ConsoleEventHandler = OnConsoleEvent;
				_ = UnsafeNativeMethods.SetConsoleCtrlHandler(m_ConsoleEventHandler, true);
			}

			if (GCSettings.IsServerGC)
			{
				Console.WriteLine("Core: Server garbage collection mode enabled");
			}

			if (_UseHRT)
			{
				Console.WriteLine("Core: Requested high resolution timing ({0})", UsingHighResolutionTiming ? "Supported" : "Unsupported");
			}

			Console.WriteLine("RandomImpl: {0} ({1})", RandomImpl.Type.Name, RandomImpl.IsHardwareRNG ? "Hardware" : "Software");

			while (!ScriptCompiler.Compile(Debug, m_Cache))
			{
				Console.WriteLine("Scripts: One or more scripts failed to compile or no script files were found.");

				if (Service)
				{
					return;
				}

				Console.WriteLine(" - Press return to exit, or R to try again.");

				if (Console.ReadKey(true).Key != ConsoleKey.R)
				{
					return;
				}
			}

			ScriptCompiler.Invoke("Prepare");

			InitDataDirectories();

			ScriptCompiler.Invoke("Configure");

			World.Load();

			ScriptCompiler.Invoke("Initialize");

			DisplayDataDirectories();

			m_MessagePump.Listen();

			m_TimerThread.Start();

			NetState.Initialize();

			EventSink.InvokeServerStarted();

			try
			{
				long now, last = TickCount;

				const int sampleInterval = 100;
				const float ticksPerSecond = 1000.0f * sampleInterval;

				long sample = 0;

				while (!Closing)
				{
					_ = m_Signal.WaitOne();

					Mobile.ProcessDeltaQueue();
					Item.ProcessDeltaQueue();

					Timer.Slice();

					m_MessagePump?.Slice();

					NetState.FlushAll();
					NetState.ProcessDisposedQueue();

					Slice?.Invoke();

					if (sample++ % sampleInterval != 0)
					{
						continue;
					}

					now = TickCount;
					m_CyclesPerSecond[m_CycleIndex++ % m_CyclesPerSecond.Length] = ticksPerSecond / (now - last);
					last = now;
				}
			}
			catch (Exception e)
			{
				CurrentDomain_UnhandledException(null, new UnhandledExceptionEventArgs(e, true));
			}
		}

		public static string Arguments
		{
			get
			{
				var sb = new StringBuilder();

				if (Debug)
				{
					Utility.Separate(sb, "-debug", " ");
				}

				if (Service)
				{
					Utility.Separate(sb, "-service", " ");
				}

				if (m_Profiling)
				{
					Utility.Separate(sb, "-profile", " ");
				}

				if (!m_Cache)
				{
					Utility.Separate(sb, "-nocache", " ");
				}

				if (HaltOnWarning)
				{
					Utility.Separate(sb, "-haltonwarning", " ");
				}

				if (_UseHRT)
				{
					Utility.Separate(sb, "-usehrt", " ");
				}

				return sb.ToString();
			}
		}

		public static int GlobalUpdateRange { get; set; } = 18;

		public static int GlobalMaxUpdateRange { get; set; } = 24;

		private static volatile int m_ItemCount, m_MobileCount;

		public static int ScriptItems => m_ItemCount;
		public static int ScriptMobiles => m_MobileCount;

		public static void VerifySerialization()
		{
			m_ItemCount = 0;
			m_MobileCount = 0;

			var ca = Assembly.GetCallingAssembly();

			VerifySerialization(ca);

			foreach (var a in ScriptCompiler.Assemblies.Where(a => a != ca))
			{
				VerifySerialization(a);
			}
		}

		private static readonly Type[] m_SerialTypeArray = { typeof(Serial) };

		private static void VerifyType(Type t)
		{
			var isItem = t.IsSubclassOf(typeof(Item));

			if (!isItem && !t.IsSubclassOf(typeof(Mobile)))
			{
				return;
			}

			if (isItem)
			{
				_ = Interlocked.Increment(ref m_ItemCount);
			}
			else
			{
				_ = Interlocked.Increment(ref m_MobileCount);
			}

			StringBuilder warningSb = null;

			try
			{
				if (t.GetConstructor(m_SerialTypeArray) == null)
				{
					warningSb ??= new StringBuilder();

					_ = warningSb.AppendLine("       - No serialization constructor");
				}

				if (t.GetMethod("Serialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
				{
					warningSb ??= new StringBuilder();

					_ = warningSb.AppendLine("       - No Serialize() method");
				}

				if (t.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) == null)
				{
					warningSb ??= new StringBuilder();

					_ = warningSb.AppendLine("       - No Deserialize() method");
				}

				if (warningSb != null && warningSb.Length > 0)
				{
					Console.WriteLine($"Warning: {t}\n{warningSb}");
				}
			}
			catch
			{
				Console.WriteLine($"Warning: Exception in serialization verification of type {t}");
			}
		}

		private static void VerifySerialization(Assembly a)
		{
			if (a != null)
			{
				_ = Parallel.ForEach(a.GetTypes(), VerifyType);
			}
		}
	}

	public class FileLogger : TextWriter
	{
		public const string DateFormat = "[MMMM dd hh:mm:ss.f tt]: ";

		private bool _NewLine;

		public string FileName { get; private set; }

		public FileLogger(string file)
			: this(file, false)
		{ }

		public FileLogger(string file, bool append)
		{
			FileName = file;

			using (
				var writer =
					new StreamWriter(
						new FileStream(FileName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read)))
			{
				writer.WriteLine(">>>Logging started on {0}.", DateTime.UtcNow.ToString("f"));
				//f = Tuesday, April 10, 2001 3:51 PM 
			}

			_NewLine = true;
		}

		public override void Write(char ch)
		{
			using var writer = new StreamWriter(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read));
			if (_NewLine)
			{
				writer.Write(DateTime.UtcNow.ToString(DateFormat));
				_NewLine = false;
			}

			writer.Write(ch);
		}

		public override void Write(string str)
		{
			using var writer = new StreamWriter(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read));
			if (_NewLine)
			{
				writer.Write(DateTime.UtcNow.ToString(DateFormat));
				_NewLine = false;
			}

			writer.Write(str);
		}

		public override void WriteLine(string line)
		{
			using var writer = new StreamWriter(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read));
			if (_NewLine)
			{
				writer.Write(DateTime.UtcNow.ToString(DateFormat));
			}

			writer.WriteLine(line);
			_NewLine = true;
		}

		public override Encoding Encoding => Encoding.Default;
	}

	public class MultiTextWriter : TextWriter
	{
		private readonly List<TextWriter> _Streams;

		public MultiTextWriter(params TextWriter[] streams)
		{
			_Streams = new List<TextWriter>(streams);

			if (_Streams.Count < 0)
			{
				throw new ArgumentException("You must specify at least one stream.");
			}
		}

		public void Add(TextWriter tw)
		{
			_Streams.Add(tw);
		}

		public void Remove(TextWriter tw)
		{
			_ = _Streams.Remove(tw);
		}

		public override void Write(char ch)
		{
			foreach (var t in _Streams)
			{
				t.Write(ch);
			}
		}

		public override void WriteLine(string line)
		{
			foreach (var t in _Streams)
			{
				t.WriteLine(line);
			}
		}

		public override void WriteLine(string line, params object[] args)
		{
			WriteLine(String.Format(line, args));
		}

		public override Encoding Encoding => Encoding.Default;
	}
}