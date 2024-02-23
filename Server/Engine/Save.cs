#if !MONO
using Microsoft.Win32.SafeHandles;
#endif

using Server.Guilds;
using Server.Network;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

#if !MONO
using System.Runtime.InteropServices;
#endif

using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Server
{
	public static class World
	{
		private static Dictionary<int, Region> m_Regions;

		private static Dictionary<Serial, Mobile> m_Mobiles;
		private static Dictionary<Serial, Item> m_Items;

		private static bool m_Loading;
		private static bool m_Loaded;

		private static bool m_Saving;
		private static readonly ManualResetEvent m_DiskWriteHandle = new ManualResetEvent(true);

		private static Queue<object> _addQueue, _deleteQueue;

		public static bool Saving => m_Saving;
		public static bool Loaded => m_Loaded;
		public static bool Loading => m_Loading;

		public static bool Volatile => m_Loading || m_Saving;

		public static string RegionRootPath => Path.Combine(Core.CurrentSavesDirectory, "Regions");
		public static string RegionIndexPath => Path.Combine(RegionRootPath, "Regions.idx");
		public static string RegionTypesPath => Path.Combine(RegionRootPath, "Regions.tdb");
		public static string RegionDataPath => Path.Combine(RegionRootPath, "Regions.bin");

		public static string MobileRootPath => Path.Combine(Core.CurrentSavesDirectory, "Mobiles");
		public static string MobileIndexPath => Path.Combine(MobileRootPath, "Mobiles.idx");
		public static string MobileTypesPath => Path.Combine(MobileRootPath, "Mobiles.tdb");
		public static string MobileDataPath => Path.Combine(MobileRootPath, "Mobiles.bin");

		public static string ItemRootPath => Path.Combine(Core.CurrentSavesDirectory, "Items");
		public static string ItemIndexPath => Path.Combine(ItemRootPath, "Items.idx");
		public static string ItemTypesPath => Path.Combine(ItemRootPath, "Items.tdb");
		public static string ItemDataPath => Path.Combine(ItemRootPath, "Items.bin");

		public static string GuildRootPath => Path.Combine(Core.CurrentSavesDirectory, "Guilds");
		public static string GuildIndexPath => Path.Combine(GuildRootPath, "Guilds.idx");
		public static string GuildDataPath => Path.Combine(GuildRootPath, "Guilds.bin");

		public static void NotifyDiskWriteComplete()
		{
			if (m_DiskWriteHandle.Set())
			{
				Console.WriteLine("Closing Save Files. ");
			}
		}

		public static void WaitForWriteCompletion()
		{
			m_DiskWriteHandle.WaitOne();
		}

		public static Dictionary<int, Region> Regions => m_Regions;

		public static Dictionary<Serial, Mobile> Mobiles => m_Mobiles;

		public static Dictionary<Serial, Item> Items => m_Items;

		public static bool OnDelete(IEntity entity)
		{
			if (m_Saving || m_Loading)
			{
				if (m_Saving)
				{
					AppendSafetyLog("delete", entity);
				}

				_deleteQueue.Enqueue(entity);

				return false;
			}

			return true;
		}

		public static void Broadcast(int hue, bool ascii, string text)
		{
			Packet p;

			if (ascii)
			{
				p = new AsciiMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "System", text);
			}
			else
			{
				p = new UnicodeMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "ENU", "System", text);
			}

			var list = NetState.Instances;

			p.Acquire();

			for (var i = 0; i < list.Count; ++i)
			{
				if (list[i].Mobile != null)
				{
					list[i].Send(p);
				}
			}

			p.Release();

			NetState.FlushAll();
		}

		public static void Broadcast(int hue, bool ascii, string format, params object[] args)
		{
			Broadcast(hue, ascii, String.Format(format, args));
		}

		private interface IEntityEntry
		{
			Serial Serial { get; }
			int TypeID { get; }
			string TypeName { get; }
			long Position { get; }
			int Length { get; }
		}

		private sealed class RegionEntry : IEntityEntry
		{
			private readonly Region m_Region;
			private readonly int m_TypeID;
			private readonly string m_TypeName;
			private readonly long m_Position;
			private readonly int m_Length;

			public Region Region => m_Region;

			public Serial Serial => m_Region?.m_Serial ?? Serial.Zero;

			public int TypeID => m_TypeID;

			public string TypeName => m_TypeName;

			public long Position => m_Position;

			public int Length => m_Length;

			public RegionEntry(Region r, int typeID, string typeName, long pos, int length)
			{
				m_Region = r;
				m_TypeID = typeID;
				m_TypeName = typeName;
				m_Position = pos;
				m_Length = length;
			}
		}

		private sealed class GuildEntry : IEntityEntry
		{
			private readonly BaseGuild m_Guild;
			private readonly long m_Position;
			private readonly int m_Length;

			public BaseGuild Guild => m_Guild;

			public Serial Serial => m_Guild?.m_Serial ?? Serial.Zero;

			public int TypeID => 0;

			public string TypeName => String.Empty;

			public long Position => m_Position;

			public int Length => m_Length;

			public GuildEntry(BaseGuild g, long pos, int length)
			{
				m_Guild = g;
				m_Position = pos;
				m_Length = length;
			}
		}

		private sealed class ItemEntry : IEntityEntry
		{
			private readonly Item m_Item;
			private readonly int m_TypeID;
			private readonly string m_TypeName;
			private readonly long m_Position;
			private readonly int m_Length;

			public Item Item => m_Item;

			public Serial Serial => m_Item?.Serial ?? Serial.MinusOne;

			public int TypeID => m_TypeID;

			public string TypeName => m_TypeName;

			public long Position => m_Position;

			public int Length => m_Length;

			public ItemEntry(Item item, int typeID, string typeName, long pos, int length)
			{
				m_Item = item;
				m_TypeID = typeID;
				m_TypeName = typeName;
				m_Position = pos;
				m_Length = length;
			}
		}

		private sealed class MobileEntry : IEntityEntry
		{
			private readonly Mobile m_Mobile;
			private readonly int m_TypeID;
			private readonly string m_TypeName;
			private readonly long m_Position;
			private readonly int m_Length;

			public Mobile Mobile => m_Mobile;

			public Serial Serial => m_Mobile?.Serial ?? Serial.MinusOne;

			public int TypeID => m_TypeID;

			public string TypeName => m_TypeName;

			public long Position => m_Position;

			public int Length => m_Length;

			public MobileEntry(Mobile mobile, int typeID, string typeName, long pos, int length)
			{
				m_Mobile = mobile;
				m_TypeID = typeID;
				m_TypeName = typeName;
				m_Position = pos;
				m_Length = length;
			}
		}

		private static string m_LoadingType;

		public static string LoadingType => m_LoadingType;

		private static readonly Type[] m_IdTypeArray = new Type[1] { typeof(int) };
		private static readonly Type[] m_SerialTypeArray = new Type[1] { typeof(Serial) };

		private static List<Tuple<ConstructorInfo, string>> ReadTypes(BinaryReader tdbReader)
		{
			var count = tdbReader.ReadInt32();

			var types = new List<Tuple<ConstructorInfo, string>>(count);

			for (var i = 0; i < count; ++i)
			{
				var typeName = tdbReader.ReadString();

				var t = ScriptCompiler.FindTypeByFullName(typeName);

				if (t == null)
				{
					Console.WriteLine("failed");

					if (!Core.Service)
					{
						Console.WriteLine($"Error: Type '{typeName}' was not found. Delete all of those types? (y/n)");

						if (Console.ReadKey(true).Key == ConsoleKey.Y)
						{
							types.Add(null);
							Console.Write("World: Loading...");
							continue;
						}

						Console.WriteLine("Types will not be deleted. An exception will be thrown.");
					}
					else
					{
						Console.WriteLine("Error: Type '{0}' was not found.", typeName);
					}

					throw new Exception($"Bad type '{typeName}'");
				}

				ConstructorInfo ctor;

				if (t.IsAssignableTo(typeof(Region)))
				{
					ctor = t.GetConstructor(m_IdTypeArray);
				}
				else
				{
					ctor = t.GetConstructor(m_SerialTypeArray);
				}

				if (ctor != null)
				{
					types.Add(new Tuple<ConstructorInfo, string>(ctor, typeName));
				}
				else
				{
					throw new Exception($"Type '{t}' does not have a serialization constructor");
				}
			}

			return types;
		}

		public static void Load()
		{
			if (m_Loaded)
			{
				return;
			}

			m_Loaded = true;
			m_LoadingType = null;

			Console.WriteLine("World: Loading...");

			var watch = Stopwatch.StartNew();

			m_Loading = true;

			_addQueue = new();
			_deleteQueue = new();

			EventSink.InvokeWorldPreLoad();

			int regionCount, mobileCount, itemCount, guildCount;

			var ctorArgs = new object[1];

			var regions = new List<RegionEntry>();
			var items = new List<ItemEntry>();
			var mobiles = new List<MobileEntry>();
			var guilds = new List<GuildEntry>();

			if (File.Exists(RegionIndexPath) && File.Exists(RegionTypesPath))
			{
				using var idx = new FileStream(RegionIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var idxReader = new BinaryReader(idx);

				using var tdb = new FileStream(RegionTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var tdbReader = new BinaryReader(tdb);

				var types = ReadTypes(tdbReader);

				regionCount = idxReader.ReadInt32();

				m_Regions = new Dictionary<int, Region>(regionCount);

				for (var i = 0; i < regionCount; ++i)
				{
					var typeID = idxReader.ReadInt32();
					var uid = idxReader.ReadInt32();
					var pos = idxReader.ReadInt64();
					var length = idxReader.ReadInt32();

					var objs = types[typeID];

					if (objs == null)
					{
						continue;
					}

					Region r = null;
					var ctor = objs.Item1;
					var typeName = objs.Item2;

					try
					{
						ctorArgs[0] = uid;

						r = (Region)ctor.Invoke(ctorArgs);
					}
					catch
					{
					}

					if (r != null)
					{
						regions.Add(new RegionEntry(r, typeID, typeName, pos, length));

						AddRegion(r);
					}
				}

				tdbReader.Close();
				idxReader.Close();
			}
			else
			{
				m_Regions = new Dictionary<int, Region>();
			}

			if (m_Regions.Count == 0)
			{
				Region.GenerateRegions();
			}

			if (File.Exists(MobileIndexPath) && File.Exists(MobileTypesPath))
			{
				using var idx = new FileStream(MobileIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var idxReader = new BinaryReader(idx);

				using var tdb = new FileStream(MobileTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var tdbReader = new BinaryReader(tdb);

				var types = ReadTypes(tdbReader);

				mobileCount = idxReader.ReadInt32();

				m_Mobiles = new Dictionary<Serial, Mobile>(mobileCount);

				for (var i = 0; i < mobileCount; ++i)
				{
					var typeID = idxReader.ReadInt32();
					var serial = idxReader.ReadInt32();
					var pos = idxReader.ReadInt64();
					var length = idxReader.ReadInt32();

					var objs = types[typeID];

					if (objs == null)
					{
						continue;
					}

					Mobile m = null;
					var ctor = objs.Item1;
					var typeName = objs.Item2;

					try
					{
						ctorArgs[0] = new Serial(serial);

						m = (Mobile)ctor.Invoke(ctorArgs);
					}
					catch
					{
					}

					if (m != null)
					{
						mobiles.Add(new MobileEntry(m, typeID, typeName, pos, length));

						AddMobile(m);
					}
				}

				tdbReader.Close();
				idxReader.Close();
			}
			else
			{
				m_Mobiles = new Dictionary<Serial, Mobile>();
			}

			if (File.Exists(ItemIndexPath) && File.Exists(ItemTypesPath))
			{
				using var idx = new FileStream(ItemIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var idxReader = new BinaryReader(idx);

				using var tdb = new FileStream(ItemTypesPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var tdbReader = new BinaryReader(tdb);

				var types = ReadTypes(tdbReader);

				itemCount = idxReader.ReadInt32();

				m_Items = new Dictionary<Serial, Item>(itemCount);

				for (var i = 0; i < itemCount; ++i)
				{
					var typeID = idxReader.ReadInt32();
					var serial = idxReader.ReadInt32();
					var pos = idxReader.ReadInt64();
					var length = idxReader.ReadInt32();

					var objs = types[typeID];

					if (objs == null)
					{
						continue;
					}

					Item item = null;
					var ctor = objs.Item1;
					var typeName = objs.Item2;

					try
					{
						ctorArgs[0] = new Serial(serial);

						item = (Item)ctor.Invoke(ctorArgs);
					}
					catch
					{
					}

					if (item != null)
					{
						items.Add(new ItemEntry(item, typeID, typeName, pos, length));

						AddItem(item);
					}
				}

				tdbReader.Close();

				idxReader.Close();
			}
			else
			{
				m_Items = new Dictionary<Serial, Item>();
			}

			if (File.Exists(GuildIndexPath))
			{
				using var idx = new FileStream(GuildIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var idxReader = new BinaryReader(idx);

				guildCount = idxReader.ReadInt32();

				var createEventArgs = new CreateGuildEventArgs(-1);

				for (var i = 0; i < guildCount; ++i)
				{
					idxReader.ReadInt32(); // typeid
					var id = idxReader.ReadInt32();
					var pos = idxReader.ReadInt64();
					var length = idxReader.ReadInt32();

					createEventArgs.Id = id;

					EventSink.InvokeCreateGuild(createEventArgs);

					var guild = createEventArgs.Guild;

					if (guild != null)
					{
						guilds.Add(new GuildEntry(guild, pos, length));
					}
				}

				idxReader.Close();
			}

			bool failedRegions = false, failedMobiles = false, failedItems = false, failedGuilds = false;

			Type failedType = null;
			var failedSerial = 0;

			Exception failed = null;
			var failedTypeID = 0;

			if (File.Exists(RegionDataPath))
			{
				using var bin = new FileStream(RegionDataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new BinaryFileReader(new BinaryReader(bin));

				for (var i = 0; i < regions.Count; ++i)
				{
					var entry = regions[i];
					var r = entry.Region;

					if (r != null)
					{
						reader.Seek(entry.Position, SeekOrigin.Begin);

						try
						{
							m_LoadingType = entry.TypeName;

							r.Deserialize(reader);

							if (reader.Position != (entry.Position + entry.Length))
							{
								throw new Exception($"***** Bad serialize on {r.GetType()} *****");
							}
						}
						catch (Exception e)
						{
							regions.RemoveAt(i);

							failed = e;
							failedRegions = true;
							failedType = r.GetType();
							failedTypeID = entry.TypeID;
							failedSerial = r.Id;

							break;
						}
					}
				}

				reader.Close();

				if (!failedRegions)
				{
					foreach (var entry in regions)
					{
						if (entry.Region != null && entry.Region.Parent == null)
						{
							entry.Region.Validate();
							entry.Region.Register();
						}
					}
				}
			}

			if (!failedRegions && File.Exists(MobileDataPath))
			{
				using var bin = new FileStream(MobileDataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new BinaryFileReader(new BinaryReader(bin));

				for (var i = 0; i < mobiles.Count; ++i)
				{
					var entry = mobiles[i];
					var m = entry.Mobile;

					if (m != null)
					{
						reader.Seek(entry.Position, SeekOrigin.Begin);

						try
						{
							m_LoadingType = entry.TypeName;

							m.Deserialize(reader);

							if (reader.Position != (entry.Position + entry.Length))
							{
								throw new Exception($"***** Bad serialize on {m.GetType()} *****");
							}
						}
						catch (Exception e)
						{
							mobiles.RemoveAt(i);

							failed = e;
							failedMobiles = true;
							failedType = m.GetType();
							failedTypeID = entry.TypeID;
							failedSerial = m.Serial;

							break;
						}
					}
				}

				reader.Close();
			}

			if (!failedRegions && !failedMobiles && File.Exists(ItemDataPath))
			{
				using var bin = new FileStream(ItemDataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new BinaryFileReader(new BinaryReader(bin));

				for (var i = 0; i < items.Count; ++i)
				{
					var entry = items[i];
					var item = entry.Item;

					if (item != null)
					{
						reader.Seek(entry.Position, SeekOrigin.Begin);

						try
						{
							m_LoadingType = entry.TypeName;

							item.Deserialize(reader);

							if (reader.Position != (entry.Position + entry.Length))
							{
								throw new Exception($"***** Bad serialize on {item.GetType()} *****");
							}
						}
						catch (Exception e)
						{
							items.RemoveAt(i);

							failed = e;
							failedItems = true;
							failedType = item.GetType();
							failedTypeID = entry.TypeID;
							failedSerial = item.Serial;

							break;
						}
					}
				}

				reader.Close();
			}

			m_LoadingType = null;

			if (!failedRegions && !failedMobiles && !failedItems && File.Exists(GuildDataPath))
			{
				using var bin = new FileStream(GuildDataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				using var reader = new BinaryFileReader(new BinaryReader(bin));

				for (var i = 0; i < guilds.Count; ++i)
				{
					var entry = guilds[i];
					var g = entry.Guild;

					if (g != null)
					{
						reader.Seek(entry.Position, SeekOrigin.Begin);

						try
						{
							g.Deserialize(reader);

							if (reader.Position != (entry.Position + entry.Length))
							{
								throw new Exception($"***** Bad serialize on Guild {g.Id} *****");
							}
						}
						catch (Exception e)
						{
							guilds.RemoveAt(i);

							failed = e;
							failedGuilds = true;
							failedType = typeof(BaseGuild);
							failedTypeID = g.Id;
							failedSerial = g.Id;

							break;
						}
					}
				}

				reader.Close();
			}

			if (failedRegions || failedItems || failedMobiles || failedGuilds)
			{
				Console.WriteLine("An error was encountered while loading a saved object");

				Console.WriteLine($" - Type: {failedType}");
				Console.WriteLine($" - Serial: {failedSerial}");

				if (!Core.Service)
				{
					Console.WriteLine("Delete the object? (y/n)");

					if (Console.ReadKey(true).Key == ConsoleKey.Y)
					{
						if (failedType != typeof(BaseGuild))
						{
							Console.WriteLine("Delete all objects of that type? (y/n)");

							if (Console.ReadKey(true).Key == ConsoleKey.Y)
							{
								if (failedMobiles)
								{
									for (var i = 0; i < mobiles.Count;)
									{
										if (mobiles[i].TypeID == failedTypeID)
										{
											mobiles.RemoveAt(i);
										}
										else
										{
											++i;
										}
									}
								}
								else if (failedItems)
								{
									for (var i = 0; i < items.Count;)
									{
										if (items[i].TypeID == failedTypeID)
										{
											items.RemoveAt(i);
										}
										else
										{
											++i;
										}
									}
								}
							}
						}

						SaveIndex(regions, RegionIndexPath);
						SaveIndex(mobiles, MobileIndexPath);
						SaveIndex(items, ItemIndexPath);
						SaveIndex(guilds, GuildIndexPath);
					}

					Console.WriteLine("After pressing return an exception will be thrown and the server will terminate.");
					Console.ReadLine();
				}
				else
				{
					Console.WriteLine("An exception will be thrown and the server will terminate.");
				}

				throw new Exception($"Load failed (regions={failedRegions}, items={failedItems}, mobiles={failedMobiles}, guilds={failedGuilds}, type={failedType}, serial={failedSerial})", failed);
			}

			EventSink.InvokeWorldLoad();

			m_Loading = false;

			ProcessSafetyQueues();

			foreach (var item in m_Items.Values)
			{
				if (item.Parent == null)
				{
					item.UpdateTotals();
				}

				item.ClearProperties();
			}

			foreach (var m in m_Mobiles.Values)
			{
				m.UpdateTotals();

				m.ClearProperties();
			}

			watch.Stop();

			Console.WriteLine($"World: Loaded ({m_Regions.Count} regions, {m_Items.Count} items, {m_Mobiles.Count} mobiles) ({watch.Elapsed.TotalSeconds:F2} seconds)");

			EventSink.InvokeWorldPostLoad();
		}

		private static void ProcessSafetyQueues()
		{
			while (_addQueue.Count > 0)
			{
				var entity = _addQueue.Dequeue();

				if (entity is Item item)
				{
					AddItem(item);
				}
				else if (entity is Mobile mob)
				{
					AddMobile(mob);
				}
				else if (entity is Region reg)
				{
					AddRegion(reg);
				}
			}

			while (_deleteQueue.Count > 0)
			{
				var entity = _deleteQueue.Dequeue();

				if (entity is Item item)
				{
					item.Delete();
				}
				else if (entity is Mobile mob)
				{
					mob.Delete();
				}
				else if (entity is Region reg)
				{
					reg.Delete();
				}
			}
		}

		private static void AppendSafetyLog(string action, object entity)
		{
			var message = $"Warning: Attempted to {action} {entity} during world save.{Environment.NewLine}This action could cause inconsistent state.{Environment.NewLine}It is strongly advised that the offending scripts be corrected.";

			Console.WriteLine(message);

			try
			{
				using var op = new StreamWriter("world-save-errors.log", true);

				op.WriteLine($"{DateTime.UtcNow}\t{message}");
				op.WriteLine(new StackTrace(2).ToString());
				op.WriteLine();
			}
			catch
			{
			}
		}

		private static void EnsureDirectories()
		{
			if (!Directory.Exists(RegionRootPath))
			{
				Directory.CreateDirectory(RegionRootPath);
			}

			if (!Directory.Exists(MobileRootPath))
			{
				Directory.CreateDirectory(MobileRootPath);
			}

			if (!Directory.Exists(ItemRootPath))
			{
				Directory.CreateDirectory(ItemRootPath);
			}

			if (!Directory.Exists(GuildRootPath))
			{
				Directory.CreateDirectory(GuildRootPath);
			}
		}

		private static void SaveIndex<T>(List<T> list, string path) where T : IEntityEntry
		{
			EnsureDirectories();

			using var idx = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			using var idxWriter = new BinaryWriter(idx);

			idxWriter.Write(list.Count);

			for (var i = 0; i < list.Count; ++i)
			{
				var e = list[i];

				idxWriter.Write(e.TypeID);
				idxWriter.Write(e.Serial);
				idxWriter.Write(e.Position);
				idxWriter.Write(e.Length);
			}

			idxWriter.Close();
		}

		internal static int m_Saves;

		public static void Save()
		{
			Save(true, false);
		}

		public static void Save(bool message, bool permitBackgroundWrite)
		{
			if (m_Saving)
			{
				return;
			}

			++m_Saves;

			NetState.FlushAll();
			NetState.Pause();

			// Block Save until current disk flush is done.
			WaitForWriteCompletion();

			m_Saving = true;

			m_DiskWriteHandle.Reset();

			if (message)
			{
				Broadcast(0x35, true, "The world is saving, please wait.");
			}

			var strategy = SaveStrategy.Acquire();

			Console.WriteLine($"Core: Using {strategy.Name.ToLowerInvariant()} save strategy");

			Console.Write("World: Saving...");

			var watch = Stopwatch.StartNew();

			EnsureDirectories();

			var args = new WorldSaveEventArgs(message);

			EventSink.InvokeWorldPreSave(args);

			strategy.Save(null, permitBackgroundWrite);

			EventSink.InvokeWorldSave(args);

			watch.Stop();

			m_Saving = false;

			if (!permitBackgroundWrite)
			{
				// Set the DiskWriteHandle.
				// If we allow background writes, we leave this up to the individual save strategies.
				NotifyDiskWriteComplete();
			}

			ProcessSafetyQueues();

			strategy.ProcessDecay();

			Console.WriteLine($"Save done in {watch.Elapsed.TotalSeconds:F2} seconds.");

			if (message)
			{
				Broadcast(0x35, true, $"World save complete. The entire process took {watch.Elapsed.TotalSeconds:F1} seconds.");
			}

			NetState.Resume();

			EventSink.InvokeWorldPostSave(args);
		}

		internal static List<Type> m_RegionTypes = new();
		internal static List<Type> m_ItemTypes = new();
		internal static List<Type> m_MobileTypes = new();

		public static IEntity FindEntity(Serial serial)
		{
			if (serial.IsItem)
			{
				return FindItem(serial);
			}

			if (serial.IsMobile)
			{
				return FindMobile(serial);
			}

			return null;
		}

		public static Mobile FindMobile(Serial serial)
		{
			m_Mobiles.TryGetValue(serial, out var mob);

			return mob;
		}

		public static Item FindItem(Serial serial)
		{
			m_Items.TryGetValue(serial, out var item);

			return item;
		}

		public static Region FindRegion(int uid)
		{
			m_Regions.TryGetValue(uid, out var region);

			return region;
		}

		public static void AddMobile(Mobile m)
		{
			if (m_Saving)
			{
				AppendSafetyLog("add", m);
				_addQueue.Enqueue(m);
			}
			else
			{
				m_Mobiles[m.Serial] = m;
			}
		}

		public static void AddItem(Item item)
		{
			if (m_Saving)
			{
				AppendSafetyLog("add", item);
				_addQueue.Enqueue(item);
			}
			else
			{
				m_Items[item.Serial] = item;
			}
		}

		public static void AddRegion(Region region)
		{
			if (m_Saving)
			{
				AppendSafetyLog("add", region);
				_addQueue.Enqueue(region);
			}
			else
			{
				m_Regions[region.Id] = region;
			}
		}

		public static void RemoveMobile(Mobile m)
		{
			m_Mobiles.Remove(m.Serial);
		}

		public static void RemoveItem(Item item)
		{
			m_Items.Remove(item.Serial);
		}

		public static void RemoveRegion(Region region)
		{
			m_Regions.Remove(region.Id);
		}
	}

	#region Persistence

	public static class Persistence
	{
		public static void SerializeBlock(GenericWriter writer, Action<GenericWriter> serializer)
		{
			if (serializer != null)
			{
				using var ms = new MemoryStream();
				using var w = new BinaryFileWriter(ms, true);

				try
				{
					serializer(w);

					w.Flush();

					writer.Write(ms);
				}
				catch
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("[Persistence]: An error was encountered while writing a persistent object");
					Utility.PopColor();

					throw;
				}
				finally
				{
					w.Close();
				}
			}
		}

		public static void DeserializeBlock(GenericReader reader, Action<GenericReader> deserializer)
		{
			using var ms = reader.ReadStream();

			if (deserializer != null)
			{
				using var r = new BinaryFileReader(new BinaryReader(ms));

				try
				{
					deserializer(r);
				}
				catch
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("[Persistence]: An error was encountered while reading a persistent object");
					Utility.PopColor();

					throw;
				}
				finally
				{
					r.Close();
				}
			}
		}

		public static void Serialize(string path, Action<GenericWriter> serializer)
		{
			Serialize(new FileInfo(path), serializer);
		}

		public static void Serialize(FileInfo file, Action<GenericWriter> serializer)
		{
			file.Refresh();

			if (file.Directory != null && !file.Directory.Exists)
			{
				file.Directory.Create();
			}

			if (!file.Exists)
			{
				file.Create().Close();
			}

			file.Refresh();

			using var fs = file.OpenWrite();
			using var writer = new BinaryFileWriter(fs, true);

			try
			{
				serializer(writer);
			}
			catch
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("[Persistence]: An error was encountered while writing a persistent object");
				Utility.PopColor();

				throw;
			}
			finally
			{
				writer.Flush();
				writer.Close();
			}
		}

		public static void Deserialize(string path, Action<GenericReader> deserializer)
		{
			Deserialize(path, deserializer, true);
		}

		public static void Deserialize(FileInfo file, Action<GenericReader> deserializer)
		{
			Deserialize(file, deserializer, true);
		}

		public static void Deserialize(string path, Action<GenericReader> deserializer, bool ensure)
		{
			Deserialize(new FileInfo(path), deserializer, ensure);
		}

		public static void Deserialize(FileInfo file, Action<GenericReader> deserializer, bool ensure)
		{
			file.Refresh();

			if (file.Directory != null && !file.Directory.Exists)
			{
				if (!ensure)
				{
					throw new DirectoryNotFoundException();
				}

				file.Directory.Create();
			}

			if (!file.Exists)
			{
				if (!ensure)
				{
					throw new FileNotFoundException
					{
						Source = file.FullName
					};
				}

				file.Create().Close();
			}

			file.Refresh();

			using var fs = file.OpenRead();
			using var reader = new BinaryFileReader(new BinaryReader(fs));

			try
			{
				deserializer(reader);
			}
			catch (EndOfStreamException)
			{
				if (file.Length > 0)
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("[Persistence]: An error was encountered while reading a persistent object");
					Utility.PopColor();

					throw;
				}
			}
			catch
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("[Persistence]: An error was encountered while reading a persistent object");
				Utility.PopColor();

				throw;
			}
			finally
			{
				reader.Close();
			}
		}

		public static void Save(string path, string root, Action<XmlElement> serializer)
		{
			Save(new FileInfo(path), root, serializer);
		}

		public static void Save(FileInfo file, string root, Action<XmlElement> serializer)
		{
			file.Refresh();

			if (file.Directory != null && !file.Directory.Exists)
			{
				file.Directory.Create();
			}

			if (!file.Exists)
			{
				file.Create().Close();
			}

			file.Refresh();

			var doc = new XmlDocument();

			doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", "yes"));

			var node = doc.CreateElement(root);

			serializer(node);

			doc.AppendChild(node);

			doc.Save(file.FullName);
		}

		public static void Load(string path, string root, Action<XmlElement> deserializer)
		{
			Load(path, root, deserializer, true);
		}

		public static void Load(FileInfo file, string root, Action<XmlElement> deserializer)
		{
			Load(file, root, deserializer, true);
		}

		public static void Load(string path, string root, Action<XmlElement> deserializer, bool ensure)
		{
			Load(new FileInfo(path), root, deserializer, ensure);
		}

		public static void Load(FileInfo file, string root, Action<XmlElement> deserializer, bool ensure)
		{
			file.Refresh();

			if (file.Directory != null && !file.Directory.Exists)
			{
				if (!ensure)
				{
					throw new DirectoryNotFoundException();
				}

				file.Directory.Create();
			}

			if (!file.Exists)
			{
				if (!ensure)
				{
					throw new FileNotFoundException
					{
						Source = file.FullName
					};
				}

				file.Create().Close();
			}

			file.Refresh();

			try
			{
				var doc = new XmlDocument();

				doc.Load(file.FullName);

				var node = doc[root];

				deserializer(node);
			}
			catch (EndOfStreamException)
			{
				if (file.Length > 0)
				{
					Utility.PushColor(ConsoleColor.Red);
					Console.WriteLine("[Persistence]: An error was encountered while reading a persistent object");
					Utility.PopColor();

					throw;
				}
			}
			catch
			{
				Utility.PushColor(ConsoleColor.Red);
				Console.WriteLine("[Persistence]: An error was encountered while reading a persistent object");
				Utility.PopColor();

				throw;
			}
		}
	}

	public sealed class BinaryMemoryWriter : BinaryFileWriter
	{
		private static readonly byte[] _indexBuffer = new byte[20];

		private MemoryStream _stream;

		protected override int BufferSize => 512;

		public BinaryMemoryWriter()
			: base(new MemoryStream(512), true)
		{
			_stream = UnderlyingStream as MemoryStream;
		}

		public override void Dispose()
		{
			base.Dispose();

			_stream = null;
		}

		public int CommitTo(SequentialFileWriter dataFile, SequentialFileWriter indexFile, int typeCode, int serial)
		{
			Flush();

			var buffer = _stream.GetBuffer();
			var length = (int)_stream.Length;

			var position = dataFile.Position;

			dataFile.Write(buffer, 0, length);

			_indexBuffer[0] = (byte)(typeCode >> 0);
			_indexBuffer[1] = (byte)(typeCode >> 8);
			_indexBuffer[2] = (byte)(typeCode >> 16);
			_indexBuffer[3] = (byte)(typeCode >> 24);

			_indexBuffer[4] = (byte)(serial >> 0);
			_indexBuffer[5] = (byte)(serial >> 8);
			_indexBuffer[6] = (byte)(serial >> 16);
			_indexBuffer[7] = (byte)(serial >> 24);

			_indexBuffer[8] = (byte)(position >> 0);
			_indexBuffer[9] = (byte)(position >> 8);
			_indexBuffer[10] = (byte)(position >> 16);
			_indexBuffer[11] = (byte)(position >> 24);
			_indexBuffer[12] = (byte)(position >> 32);
			_indexBuffer[13] = (byte)(position >> 40);
			_indexBuffer[14] = (byte)(position >> 48);
			_indexBuffer[15] = (byte)(position >> 56);

			_indexBuffer[16] = (byte)(length >> 0);
			_indexBuffer[17] = (byte)(length >> 8);
			_indexBuffer[18] = (byte)(length >> 16);
			_indexBuffer[19] = (byte)(length >> 24);

			indexFile.Write(_indexBuffer, 0, _indexBuffer.Length);

			_stream.SetLength(0);

			return length;
		}
	}

	public sealed class QueuedMemoryWriter : BinaryFileWriter
	{
		private struct IndexInfo
		{
			public int size;
			public int typeCode;
			public int serial;
		}

		private MemoryStream _memStream;
		private List<IndexInfo> _orderedIndexInfo = new();

		protected override int BufferSize => 512;

		public QueuedMemoryWriter()
			: base(new MemoryStream(1024 * 1024), true)
		{
			_memStream = UnderlyingStream as MemoryStream;
		}

		public override void Dispose()
		{
			base.Dispose();

			_memStream = null;

			_orderedIndexInfo.Clear();
			_orderedIndexInfo.TrimExcess();
			_orderedIndexInfo = null;
		}

		public void QueueForIndex(ISerializable serializable, int size)
		{
			IndexInfo info;

			info.size = size;

			info.typeCode = serializable.TypeReference;
			info.serial = serializable.SerialIdentity;

			_orderedIndexInfo.Add(info);
		}

		public void CommitTo(SequentialFileWriter dataFile, SequentialFileWriter indexFile)
		{
			Flush();

			var memLength = (int)_memStream.Position;

			if (memLength > 0)
			{
				var memBuffer = _memStream.GetBuffer();

				var actualPosition = dataFile.Position;

				dataFile.Write(memBuffer, 0, memLength);

				var indexBuffer = new byte[20];

				for (var i = 0; i < _orderedIndexInfo.Count; i++)
				{
					var info = _orderedIndexInfo[i];

					indexBuffer[0] = (byte)(info.typeCode >> 0);
					indexBuffer[1] = (byte)(info.typeCode >> 8);
					indexBuffer[2] = (byte)(info.typeCode >> 16);
					indexBuffer[3] = (byte)(info.typeCode >> 24);

					indexBuffer[4] = (byte)(info.serial >> 0);
					indexBuffer[5] = (byte)(info.serial >> 8);
					indexBuffer[6] = (byte)(info.serial >> 16);
					indexBuffer[7] = (byte)(info.serial >> 24);

					indexBuffer[8] = (byte)(actualPosition >> 0);
					indexBuffer[9] = (byte)(actualPosition >> 8);
					indexBuffer[10] = (byte)(actualPosition >> 16);
					indexBuffer[11] = (byte)(actualPosition >> 24);
					indexBuffer[12] = (byte)(actualPosition >> 32);
					indexBuffer[13] = (byte)(actualPosition >> 40);
					indexBuffer[14] = (byte)(actualPosition >> 48);
					indexBuffer[15] = (byte)(actualPosition >> 56);

					indexBuffer[16] = (byte)(info.size >> 0);
					indexBuffer[17] = (byte)(info.size >> 8);
					indexBuffer[18] = (byte)(info.size >> 16);
					indexBuffer[19] = (byte)(info.size >> 24);

					indexFile.Write(indexBuffer, 0, indexBuffer.Length);

					actualPosition += info.size;
				}
			}

			Close();
		}
	}

	public delegate void FileCommitCallback(FileQueue.Chunk chunk);

	public sealed class FileQueue : IDisposable
	{
		public sealed class Chunk
		{
			private readonly FileQueue _owner;
			private readonly int _slot;

			private readonly byte[] _buffer;

			private readonly int _offset;
			private readonly int _size;

			public byte[] Buffer => _buffer;

			public int Offset => _offset;
			public int Size => _size;

			public Chunk(FileQueue owner, int slot, byte[] buffer, int offset, int size)
			{
				_owner = owner;
				_slot = slot;

				_buffer = buffer;
				_offset = offset;
				_size = size;
			}

			public void Commit()
			{
				_owner.Commit(this, _slot);
			}
		}

		private struct Page
		{
			public byte[] buffer;
			public int length;
		}

		private static readonly int _bufferSize;
		private static readonly BufferPool _bufferPool;

		static FileQueue()
		{
			_bufferSize = FileOperations.BufferSize;
			_bufferPool = new BufferPool("File Buffers", 64, _bufferSize);
		}

		private readonly object _syncRoot;

		private readonly Chunk[] _active;
		private int _activeCount;

		private readonly Queue<Page> _pending;
		private Page _buffered;

		private readonly FileCommitCallback _callback;

		private ManualResetEvent _idle;

		private long _position;

		public long Position => _position;

		public FileQueue(int concurrentWrites, FileCommitCallback callback)
		{
			if (concurrentWrites < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(concurrentWrites));
			}

			_callback = callback ?? throw new ArgumentNullException(nameof(callback));

			_syncRoot = new object();

			_active = new Chunk[concurrentWrites];
			_pending = new Queue<Page>();

			_idle = new ManualResetEvent(true);
		}

		private void Append(Page page)
		{
			lock (_syncRoot)
			{
				if (_activeCount == 0)
				{
					_idle.Reset();
				}

				++_activeCount;

				for (var slot = 0; slot < _active.Length; ++slot)
				{
					if (_active[slot] == null)
					{
						_active[slot] = new Chunk(this, slot, page.buffer, 0, page.length);

						_callback(_active[slot]);

						return;
					}
				}

				_pending.Enqueue(page);
			}
		}

		public void Dispose()
		{
			if (_idle != null)
			{
				_idle.Close();
				_idle = null;
			}
		}

		public void Flush()
		{
			if (_buffered.buffer != null)
			{
				Append(_buffered);

				_buffered.buffer = null;
				_buffered.length = 0;
			}

			_idle.WaitOne();
		}

		private void Commit(Chunk chunk, int slot)
		{
			if (slot < 0 || slot >= _active.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(slot));
			}

			lock (_syncRoot)
			{
				if (_active[slot] != chunk)
				{
					throw new ArgumentException("current chunk is not active", nameof(chunk));
				}

				_bufferPool.ReleaseBuffer(chunk.Buffer);

				if (_pending.Count > 0)
				{
					var page = _pending.Dequeue();

					_active[slot] = new Chunk(this, slot, page.buffer, 0, page.length);

					_callback(_active[slot]);
				}
				else
				{
					_active[slot] = null;
				}

				--_activeCount;

				if (_activeCount == 0)
				{
					_idle.Set();
				}
			}
		}

		public void Enqueue(byte[] buffer, int offset, int size)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (size < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(size));
			}

			if (buffer.Length - offset < size)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			_position += size;

			while (size > 0)
			{
				if (_buffered.buffer == null)
				{
					_buffered.buffer = _bufferPool.AcquireBuffer();
				}

				var page = _buffered.buffer;
				var pageSpace = page.Length - _buffered.length;
				var byteCount = size > pageSpace ? pageSpace : size;

				Buffer.BlockCopy(buffer, offset, page, _buffered.length, byteCount);

				_buffered.length += byteCount;

				offset += byteCount;
				size -= byteCount;

				if (_buffered.length == page.Length)
				{
					Append(_buffered);

					_buffered.buffer = null;
					_buffered.length = 0;
				}
			}
		}
	}

	public static class FileOperations
	{
#if !MONO
		private const FileOptions NoBuffering = (FileOptions)0x20000000;

		internal static class UnsafeNativeMethods
		{
			[DllImport("Kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
			internal static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, IntPtr securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);
		}
#endif

		public const int KB = 1024;
		public const int MB = 1024 * KB;

		public static int BufferSize { get; set; } = 1 * MB;
		public static int Concurrency { get; set; } = 1;
		public static bool Unbuffered { get; set; } = true;

		public static bool AreSynchronous => Concurrency < 1;
		public static bool AreAsynchronous => Concurrency > 0;

		public static FileStream OpenSequentialStream(string path, FileMode mode, FileAccess access, FileShare share)
		{
			var options = FileOptions.SequentialScan;

			if (Concurrency > 0)
			{
				options |= FileOptions.Asynchronous;
			}

#if MONO
			return new FileStream(path, mode, access, share, _bufferSize, options);
#else
			if (!Unbuffered)
			{
				return new FileStream(path, mode, access, share, BufferSize, options);
			}

			options |= NoBuffering;

			var fileHandle = UnsafeNativeMethods.CreateFile(path, (int)access, share, IntPtr.Zero, mode, (int)options, IntPtr.Zero);

			if (fileHandle.IsInvalid)
			{
				throw new IOException();
			}

			return new UnbufferedFileStream(fileHandle, access, BufferSize, AreAsynchronous);
#endif
		}

#if !MONO
		private class UnbufferedFileStream : FileStream
		{
			private readonly SafeFileHandle _fileHandle;

			public UnbufferedFileStream(SafeFileHandle fileHandle, FileAccess access, int bufferSize, bool isAsync)
				: base(fileHandle, access, bufferSize, isAsync)
			{
				_fileHandle = fileHandle;
			}

			public override void Write(byte[] array, int offset, int count)
			{
				base.Write(array, offset, BufferSize);
			}

			public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
			{
				return base.BeginWrite(array, offset, BufferSize, userCallback, stateObject);
			}

			protected override void Dispose(bool disposing)
			{
				if (!_fileHandle.IsClosed)
				{
					_fileHandle.Close();
				}

				base.Dispose(disposing);
			}
		}
#endif
	}

	public sealed class SequentialFileWriter : Stream
	{
		private FileStream _fileStream;
		private FileQueue _fileQueue;

		private AsyncCallback _writeCallback;

		private readonly SaveMetrics _metrics;

		public override bool CanRead => false;
		public override bool CanSeek => false;
		public override bool CanWrite => true;

		public override long Length => Position;

		public override long Position { get => _fileQueue.Position; set => throw new InvalidOperationException(); }

		public SequentialFileWriter(string path, SaveMetrics metrics)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			_metrics = metrics;

			_fileStream = FileOperations.OpenSequentialStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

			_fileQueue = new FileQueue(Math.Max(1, FileOperations.Concurrency), FileCallback);
		}

		private void FileCallback(FileQueue.Chunk chunk)
		{
			if (FileOperations.AreSynchronous)
			{
				_fileStream.Write(chunk.Buffer, chunk.Offset, chunk.Size);

				if (_metrics != null)
				{
					_metrics.OnFileWritten(chunk.Size);
				}

				chunk.Commit();
			}
			else
			{
				if (_writeCallback == null)
				{
					_writeCallback = OnWrite;
				}

				_fileStream.BeginWrite(chunk.Buffer, chunk.Offset, chunk.Size, _writeCallback, chunk);
			}
		}

		private void OnWrite(IAsyncResult asyncResult)
		{
			var chunk = asyncResult.AsyncState as FileQueue.Chunk;

			_fileStream.EndWrite(asyncResult);

			if (_metrics != null)
			{
				_metrics.OnFileWritten(chunk.Size);
			}

			chunk.Commit();
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
			_fileQueue.Enqueue(buffer, offset, size);
		}

		public override void Flush()
		{
			_fileQueue.Flush();
			_fileStream.Flush();
		}

		protected override void Dispose(bool disposing)
		{
			if (_fileStream != null)
			{
				Flush();

				_fileQueue.Dispose();
				_fileQueue = null;

				_fileStream.Close();
				_fileStream = null;
			}

			base.Dispose(disposing);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new InvalidOperationException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException();
		}

		public override void SetLength(long value)
		{
			_fileStream.SetLength(value);
		}
	}

	public sealed class SaveMetrics : IDisposable
	{
		private const string _PerformanceCategoryName = "uoAvos";
		private const string _PerformanceCategoryDesc = "Performance counters for uoAvos";

#if !MONO
		private readonly PerformanceCounter _numberOfWorldSaves;

		private readonly PerformanceCounter _regionsPerSecond;
		private readonly PerformanceCounter _itemsPerSecond;
		private readonly PerformanceCounter _mobilesPerSecond;

		private readonly PerformanceCounter _serializedBytesPerSecond;
		private readonly PerformanceCounter _writtenBytesPerSecond;

		public SaveMetrics()
		{
			if (!PerformanceCounterCategory.Exists(_PerformanceCategoryName))
			{
				var counters = new CounterCreationDataCollection {
					new CounterCreationData(
						"Save - Count",
						"Number of world saves.",
						PerformanceCounterType.NumberOfItems32
					),

					new CounterCreationData(
						"Save - Regions/sec",
						"Number of regions saved per second.",
						PerformanceCounterType.RateOfCountsPerSecond32
					),

					new CounterCreationData(
						"Save - Items/sec",
						"Number of items saved per second.",
						PerformanceCounterType.RateOfCountsPerSecond32
					),

					new CounterCreationData(
						"Save - Mobiles/sec",
						"Number of mobiles saved per second.",
						PerformanceCounterType.RateOfCountsPerSecond32
					),

					new CounterCreationData(
						"Save - Serialized bytes/sec",
						"Amount of world-save bytes serialized per second.",
						PerformanceCounterType.RateOfCountsPerSecond32
					),

					new CounterCreationData(
						"Save - Written bytes/sec",
						"Amount of world-save bytes written to disk per second.",
						PerformanceCounterType.RateOfCountsPerSecond32
					)
				};

				PerformanceCounterCategory.Create(_PerformanceCategoryName, _PerformanceCategoryDesc, PerformanceCounterCategoryType.SingleInstance, counters);
			}

			_numberOfWorldSaves = new PerformanceCounter(_PerformanceCategoryName, "Save - Count", false);

			_regionsPerSecond = new PerformanceCounter(_PerformanceCategoryName, "Save - Regions/sec", false);
			_itemsPerSecond = new PerformanceCounter(_PerformanceCategoryName, "Save - Items/sec", false);
			_mobilesPerSecond = new PerformanceCounter(_PerformanceCategoryName, "Save - Mobiles/sec", false);

			_serializedBytesPerSecond = new PerformanceCounter(_PerformanceCategoryName, "Save - Serialized bytes/sec", false);
			_writtenBytesPerSecond = new PerformanceCounter(_PerformanceCategoryName, "Save - Written bytes/sec", false);

			_numberOfWorldSaves.Increment();
		}

		public void OnRegionSaved(int numberOfBytes)
		{
			_regionsPerSecond.Increment();

			_serializedBytesPerSecond.IncrementBy(numberOfBytes);
		}

		public void OnItemSaved(int numberOfBytes)
		{
			_itemsPerSecond.Increment();

			_serializedBytesPerSecond.IncrementBy(numberOfBytes);
		}

		public void OnMobileSaved(int numberOfBytes)
		{
			_mobilesPerSecond.Increment();

			_serializedBytesPerSecond.IncrementBy(numberOfBytes);
		}

		public void OnGuildSaved(int numberOfBytes)
		{
			_serializedBytesPerSecond.IncrementBy(numberOfBytes);
		}

		public void OnFileWritten(int numberOfBytes)
		{
			_writtenBytesPerSecond.IncrementBy(numberOfBytes);
		}

		private bool isDisposed;

		public void Dispose()
		{
			if (!isDisposed)
			{
				isDisposed = true;

				_numberOfWorldSaves.Dispose();

				_regionsPerSecond.Dispose();
				_itemsPerSecond.Dispose();
				_mobilesPerSecond.Dispose();

				_serializedBytesPerSecond.Dispose();
				_writtenBytesPerSecond.Dispose();
			}
		}
#else
		public void Dispose()
		{ }

		public void OnRegionSaved(int numberOfBytes)
		{ }

		public void OnItemSaved(int numberOfBytes)
		{ }

		public void OnMobileSaved(int numberOfBytes)
		{ }

		public void OnGuildSaved(int numberOfBytes)
		{ }

		public void OnFileWritten(int numberOfBytes)
		{ }
#endif
	}

	public abstract class SaveStrategy
	{
		public static SaveStrategy Acquire()
		{
			if (Core.MultiProcessor)
			{
				var processorCount = Core.ProcessorCount;

				if (processorCount > 2)
				{
					return new ParallelSaveStrategy(processorCount);
				}
			}

			return new StandardSaveStrategy();
		}

		public abstract string Name { get; }

		public abstract void Save(SaveMetrics metrics, bool permitBackgroundWrite);

		public abstract void ProcessDecay();
	}

	public class StandardSaveStrategy : SaveStrategy
	{
		public enum SaveOption
		{
			Normal,
			Threaded
		}

		public static SaveOption SaveType { get; set; } = SaveOption.Normal;

		public override string Name => "Standard";

		private readonly Queue<Item> _decayQueue;

		private bool _permitBackgroundWrite;

		public StandardSaveStrategy()
		{
			_decayQueue = new Queue<Item>();
		}

		protected bool PermitBackgroundWrite { get => _permitBackgroundWrite; set => _permitBackgroundWrite = value; }

		protected bool UseSequentialWriters => SaveType == SaveOption.Normal || !_permitBackgroundWrite;

		public override void Save(SaveMetrics metrics, bool permitBackgroundWrite)
		{
			_permitBackgroundWrite = permitBackgroundWrite;

			SaveRegions(metrics);
			SaveMobiles(metrics);
			SaveItems(metrics);
			SaveGuilds(metrics);

			if (permitBackgroundWrite && UseSequentialWriters)  //If we're permitted to write in the background, but we don't anyways, then notify.
			{
				World.NotifyDiskWriteComplete();
			}
		}

		protected void SaveRegions(SaveMetrics metrics)
		{
			var regions = World.Regions;

			GenericWriter idx, tdb, bin;

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter(World.RegionIndexPath, false);
				tdb = new BinaryFileWriter(World.RegionTypesPath, false);
				bin = new BinaryFileWriter(World.RegionDataPath, true);
			}
			else
			{
				idx = new AsyncWriter(World.RegionIndexPath, false);
				tdb = new AsyncWriter(World.RegionTypesPath, false);
				bin = new AsyncWriter(World.RegionDataPath, true);
			}

			idx.Write(regions.Count);

			foreach (var r in regions.Values)
			{
				var start = bin.Position;

				idx.Write(r.m_TypeRef);
				idx.Write(r.Id);
				idx.Write(start);

				r.Serialize(bin);

				if (metrics != null)
				{
					metrics.OnRegionSaved((int)(bin.Position - start));
				}

				idx.Write((int)(bin.Position - start));
			}

			tdb.Write(World.m_RegionTypes.Count);

			for (var i = 0; i < World.m_RegionTypes.Count; ++i)
			{
				tdb.Write(World.m_RegionTypes[i].FullName);
			}

			idx.Close();
			tdb.Close();
			bin.Close();
		}

		protected void SaveMobiles(SaveMetrics metrics)
		{
			var mobiles = World.Mobiles;

			GenericWriter idx, tdb, bin;

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter(World.MobileIndexPath, false);
				tdb = new BinaryFileWriter(World.MobileTypesPath, false);
				bin = new BinaryFileWriter(World.MobileDataPath, true);
			}
			else
			{
				idx = new AsyncWriter(World.MobileIndexPath, false);
				tdb = new AsyncWriter(World.MobileTypesPath, false);
				bin = new AsyncWriter(World.MobileDataPath, true);
			}

			idx.Write(mobiles.Count);

			foreach (var m in mobiles.Values)
			{
				var start = bin.Position;

				idx.Write(m.m_TypeRef);
				idx.Write(m.Serial);
				idx.Write(start);

				m.Serialize(bin);

				if (metrics != null)
				{
					metrics.OnMobileSaved((int)(bin.Position - start));
				}

				idx.Write((int)(bin.Position - start));

				m.FreeCache();
			}

			tdb.Write(World.m_MobileTypes.Count);

			for (var i = 0; i < World.m_MobileTypes.Count; ++i)
			{
				tdb.Write(World.m_MobileTypes[i].FullName);
			}

			idx.Close();
			tdb.Close();
			bin.Close();
		}

		protected void SaveItems(SaveMetrics metrics)
		{
			var items = World.Items;

			GenericWriter idx, tdb, bin;

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter(World.ItemIndexPath, false);
				tdb = new BinaryFileWriter(World.ItemTypesPath, false);
				bin = new BinaryFileWriter(World.ItemDataPath, true);
			}
			else
			{
				idx = new AsyncWriter(World.ItemIndexPath, false);
				tdb = new AsyncWriter(World.ItemTypesPath, false);
				bin = new AsyncWriter(World.ItemDataPath, true);
			}

			idx.Write(items.Count);

			var n = DateTime.UtcNow;

			foreach (var item in items.Values)
			{
				if (item.Decays && item.Parent == null && item.Map != Map.Internal && (item.LastMoved + item.DecayTime) <= n)
				{
					_decayQueue.Enqueue(item);
				}

				var start = bin.Position;

				idx.Write(item.m_TypeRef);
				idx.Write(item.Serial);
				idx.Write(start);

				item.Serialize(bin);

				if (metrics != null)
				{
					metrics.OnItemSaved((int)(bin.Position - start));
				}

				idx.Write((int)(bin.Position - start));

				item.FreeCache();
			}

			tdb.Write(World.m_ItemTypes.Count);

			for (var i = 0; i < World.m_ItemTypes.Count; ++i)
			{
				tdb.Write(World.m_ItemTypes[i].FullName);
			}

			idx.Close();
			tdb.Close();
			bin.Close();
		}

		protected void SaveGuilds(SaveMetrics metrics)
		{
			GenericWriter idx;
			GenericWriter bin;

			if (UseSequentialWriters)
			{
				idx = new BinaryFileWriter(World.GuildIndexPath, false);
				bin = new BinaryFileWriter(World.GuildDataPath, true);
			}
			else
			{
				idx = new AsyncWriter(World.GuildIndexPath, false);
				bin = new AsyncWriter(World.GuildDataPath, true);
			}

			idx.Write(BaseGuild.List.Count);

			foreach (var guild in BaseGuild.List.Values)
			{
				var start = bin.Position;

				idx.Write(0);//guilds have no typeid
				idx.Write(guild.Id);
				idx.Write(start);

				guild.Serialize(bin);

				if (metrics != null)
				{
					metrics.OnGuildSaved((int)(bin.Position - start));
				}

				idx.Write((int)(bin.Position - start));
			}

			idx.Close();
			bin.Close();
		}

		public override void ProcessDecay()
		{
			while (_decayQueue.Count > 0)
			{
				var item = _decayQueue.Dequeue();

				if (item.OnDecay())
				{
					item.Delete();
				}
			}
		}
	}

	public sealed class ParallelSaveStrategy : SaveStrategy
	{
		public override string Name => "Parallel";

		private readonly int _processorCount;

		private SaveMetrics _metrics;

		private SequentialFileWriter _regionData, _regionIndex;
		private SequentialFileWriter _itemData, _itemIndex;
		private SequentialFileWriter _mobileData, _mobileIndex;
		private SequentialFileWriter _guildData, _guildIndex;

		private readonly Queue<Item> _decayQueue = new();

		private Consumer[] _consumers;
		private int _cycle;

		private bool _finished;

		public ParallelSaveStrategy(int processorCount)
		{
			_processorCount = processorCount;
		}

		private int GetThreadCount()
		{
			return _processorCount - 1;
		}

		public override void Save(SaveMetrics metrics, bool permitBackgroundWrite)
		{
			_metrics = metrics;

			OpenFiles();

			_consumers = new Consumer[GetThreadCount()];

			for (var i = 0; i < _consumers.Length; ++i)
			{
				_consumers[i] = new Consumer(this, 256);
			}

			foreach (var value in Produce())
			{
				while (!Enqueue(value))
				{
					if (!Commit())
					{
						Thread.Sleep(0);
					}
				}
			}

			_finished = true;

			SaveTypeDatabases();

			WaitHandle.WaitAll(Array.ConvertAll<Consumer, WaitHandle>(_consumers, input => input._completionEvent));

			Commit();

			CloseFiles();

			static IEnumerable<ISerializable> Produce()
			{
				foreach (var reg in World.Regions.Values)
				{
					yield return reg;
				}

				foreach (var item in World.Items.Values)
				{
					yield return item;
				}

				foreach (var mob in World.Mobiles.Values)
				{
					yield return mob;
				}

				foreach (var guild in BaseGuild.List.Values)
				{
					yield return guild;
				}
			};
		}

		public override void ProcessDecay()
		{
			while (_decayQueue.Count > 0)
			{
				var item = _decayQueue.Dequeue();

				if (item.OnDecay())
				{
					item.Delete();
				}
			}
		}

		private static void SaveTypeDatabases()
		{
			SaveTypeDatabase(World.RegionTypesPath, World.m_RegionTypes);
			SaveTypeDatabase(World.ItemTypesPath, World.m_ItemTypes);
			SaveTypeDatabase(World.MobileTypesPath, World.m_MobileTypes);

			static void SaveTypeDatabase(string path, List<Type> types)
			{
				using var bfw = new BinaryFileWriter(path, false);

				bfw.Write(types.Count);

				foreach (var type in types)
				{
					bfw.Write(type.FullName);
				}

				bfw.Flush();
				bfw.Close();
			};
		}

		private void OpenFiles()
		{
			_regionData = new SequentialFileWriter(World.RegionDataPath, _metrics);
			_regionIndex = new SequentialFileWriter(World.RegionIndexPath, _metrics);

			_itemData = new SequentialFileWriter(World.ItemDataPath, _metrics);
			_itemIndex = new SequentialFileWriter(World.ItemIndexPath, _metrics);

			_mobileData = new SequentialFileWriter(World.MobileDataPath, _metrics);
			_mobileIndex = new SequentialFileWriter(World.MobileIndexPath, _metrics);

			_guildData = new SequentialFileWriter(World.GuildDataPath, _metrics);
			_guildIndex = new SequentialFileWriter(World.GuildIndexPath, _metrics);

			WriteCount(_regionIndex, World.Regions.Count);
			WriteCount(_itemIndex, World.Items.Count);
			WriteCount(_mobileIndex, World.Mobiles.Count);
			WriteCount(_guildIndex, BaseGuild.List.Count);

			static void WriteCount(SequentialFileWriter indexFile, int count)
			{
				var buffer = new byte[4];

				buffer[0] = (byte)(count >> 0);
				buffer[1] = (byte)(count >> 8);
				buffer[2] = (byte)(count >> 16);
				buffer[3] = (byte)(count >> 24);

				indexFile.Write(buffer, 0, buffer.Length);
			};
		}

		private void CloseFiles()
		{
			_regionData.Close();
			_regionIndex.Close();

			_itemData.Close();
			_itemIndex.Close();

			_mobileData.Close();
			_mobileIndex.Close();

			_guildData.Close();
			_guildIndex.Close();

			World.NotifyDiskWriteComplete();
		}

		private void OnSerialized(ConsumableEntry entry)
		{
			var value = entry.value;
			var writer = entry.writer;

			if (value is Region reg)
			{
				Save(reg, writer);
			}
			else if (value is Item item)
			{
				Save(item, writer);
			}
			else if (value is Mobile mob)
			{
				Save(mob, writer);
			}
			else if (value is BaseGuild guild)
			{
				Save(guild, writer);
			}
		}

		private void Save(Region reg, BinaryMemoryWriter writer)
		{
			var length = writer.CommitTo(_regionData, _regionIndex, reg.m_TypeRef, reg.Id);

			if (_metrics != null)
			{
				_metrics.OnRegionSaved(length);
			}
		}

		private void Save(Item item, BinaryMemoryWriter writer)
		{
			var length = writer.CommitTo(_itemData, _itemIndex, item.m_TypeRef, item.Serial);

			if (_metrics != null)
			{
				_metrics.OnItemSaved(length);
			}

			if (item.Decays && item.Parent == null && item.Map != Map.Internal && DateTime.UtcNow > (item.LastMoved + item.DecayTime))
			{
				_decayQueue.Enqueue(item);
			}
		}

		private void Save(Mobile mob, BinaryMemoryWriter writer)
		{
			var length = writer.CommitTo(_mobileData, _mobileIndex, mob.m_TypeRef, mob.Serial);

			if (_metrics != null)
			{
				_metrics.OnMobileSaved(length);
			}
		}

		private void Save(BaseGuild guild, BinaryMemoryWriter writer)
		{
			var length = writer.CommitTo(_guildData, _guildIndex, 0, guild.Id);

			if (_metrics != null)
			{
				_metrics.OnGuildSaved(length);
			}
		}

		private bool Enqueue(ISerializable value)
		{
			for (var i = 0; i < _consumers.Length; ++i)
			{
				var consumer = _consumers[_cycle++ % _consumers.Length];

				if ((consumer._tail - consumer._head) < consumer._buffer.Length)
				{
					consumer._buffer[consumer._tail % consumer._buffer.Length].value = value;
					consumer._tail++;

					return true;
				}
			}

			return false;
		}

		private bool Commit()
		{
			var committed = false;

			for (var i = 0; i < _consumers.Length; ++i)
			{
				var consumer = _consumers[i];

				while (consumer._head < consumer._done)
				{
					OnSerialized(consumer._buffer[consumer._head % consumer._buffer.Length]);
					consumer._head++;

					committed = true;
				}
			}

			return committed;
		}

		private struct ConsumableEntry
		{
			public ISerializable value;
			public BinaryMemoryWriter writer;
		}

		private sealed class Consumer
		{
			private readonly ParallelSaveStrategy _owner;

			public ManualResetEvent _completionEvent;

			public ConsumableEntry[] _buffer;

			public int _head, _done, _tail;

			private readonly Thread _thread;

			public Consumer(ParallelSaveStrategy owner, int bufferSize)
			{
				_owner = owner;

				_buffer = new ConsumableEntry[bufferSize];

				for (var i = 0; i < _buffer.Length; ++i)
				{
					_buffer[i].writer = new BinaryMemoryWriter();
				}

				_completionEvent = new ManualResetEvent(false);

				_thread = new Thread(Processor)
				{
					Name = "Parallel Serialization Thread"
				};

				_thread.Start();
			}

			private void Processor()
			{
				try
				{
					while (!_owner._finished)
					{
						Process();
						Thread.Sleep(0);
					}

					Process();

					_completionEvent.Set();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}

			private void Process()
			{
				ConsumableEntry entry;

				while (_done < _tail)
				{
					entry = _buffer[_done % _buffer.Length];

					entry.value.Serialize(entry.writer);

					++_done;
				}
			}
		}
	}

	public sealed class DynamicSaveStrategy : SaveStrategy
	{
		public override string Name => "Dynamic";

		private SaveMetrics _metrics;

		private SequentialFileWriter _regionData, _regionIndex;
		private SequentialFileWriter _itemData, _itemIndex;
		private SequentialFileWriter _mobileData, _mobileIndex;
		private SequentialFileWriter _guildData, _guildIndex;

		private readonly ConcurrentBag<Item> _decayBag = new();

		private readonly BlockingCollection<QueuedMemoryWriter> _regionThreadWriters = new();
		private readonly BlockingCollection<QueuedMemoryWriter> _itemThreadWriters = new();
		private readonly BlockingCollection<QueuedMemoryWriter> _mobileThreadWriters = new();
		private readonly BlockingCollection<QueuedMemoryWriter> _guildThreadWriters = new();

		public DynamicSaveStrategy()
		{
		}

		public override void Save(SaveMetrics metrics, bool permitBackgroundWrite)
		{
			_metrics = metrics;

			OpenFiles();

			var saveTasks = new[]
			{
				SaveRegions(),
				SaveItems(),
				SaveMobiles(),
				SaveGuilds()
			};

			SaveTypeDatabases();

			if (permitBackgroundWrite)
			{
				Task.Factory.ContinueWhenAll(saveTasks, _ =>
				{
					CloseFiles();

					World.NotifyDiskWriteComplete();
				});
			}
			else
			{
				Task.WaitAll(saveTasks);

				CloseFiles();
			}
		}

		private static Task StartCommitTask(BlockingCollection<QueuedMemoryWriter> threadWriter, SequentialFileWriter data, SequentialFileWriter index)
		{
			return Task.Factory.StartNew(() =>
			{
				while (!threadWriter.IsCompleted)
				{
					QueuedMemoryWriter writer;

					try
					{
						writer = threadWriter.Take();
					}
					catch (InvalidOperationException)
					{
						//Per MSDN, it's fine if we're here, successful completion of adding can rarely put us into this state.
						break;
					}

					writer.CommitTo(data, index);
				}
			});
		}

		private Task SaveRegions()
		{
			var commitTask = StartCommitTask(_regionThreadWriters, _regionData, _regionIndex);

			Parallel.ForEach(World.Regions.Values, () => new QueuedMemoryWriter(), (reg, state, writer) =>
			{
				var startPosition = writer.Position;

				reg.Serialize(writer);

				var size = (int)(writer.Position - startPosition);

				writer.QueueForIndex(reg, size);

				if (_metrics != null)
				{
					_metrics.OnRegionSaved(size);
				}

				return writer;
			},
			writer =>
			{
				writer.Flush();

				_regionThreadWriters.Add(writer);
			});

			_regionThreadWriters.CompleteAdding();

			return commitTask;
		}

		private Task SaveItems()
		{
			var commitTask = StartCommitTask(_itemThreadWriters, _itemData, _itemIndex);

			Parallel.ForEach(World.Items.Values, () => new QueuedMemoryWriter(), (item, state, writer) =>
			{
				var startPosition = writer.Position;

				item.Serialize(writer);

				var size = (int)(writer.Position - startPosition);

				writer.QueueForIndex(item, size);

				if (item.Decays && item.Parent == null && item.Map != Map.Internal && DateTime.UtcNow > (item.LastMoved + item.DecayTime))
				{
					_decayBag.Add(item);
				}

				if (_metrics != null)
				{
					_metrics.OnItemSaved(size);
				}

				return writer;
			},
			writer =>
			{
				writer.Flush();

				_itemThreadWriters.Add(writer);
			});

			_itemThreadWriters.CompleteAdding();

			return commitTask;
		}

		private Task SaveMobiles()
		{
			var commitTask = StartCommitTask(_mobileThreadWriters, _mobileData, _mobileIndex);

			Parallel.ForEach(World.Mobiles.Values, () => new QueuedMemoryWriter(), (mobile, state, writer) =>
			{
				var startPosition = writer.Position;

				mobile.Serialize(writer);

				var size = (int)(writer.Position - startPosition);

				writer.QueueForIndex(mobile, size);

				if (_metrics != null)
				{
					_metrics.OnMobileSaved(size);
				}

				return writer;
			},
			writer =>
			{
				writer.Flush();

				_mobileThreadWriters.Add(writer);
			});

			_mobileThreadWriters.CompleteAdding();

			return commitTask;
		}

		private Task SaveGuilds()
		{
			var commitTask = StartCommitTask(_guildThreadWriters, _guildData, _guildIndex);

			Parallel.ForEach(BaseGuild.List.Values, () => new QueuedMemoryWriter(), (guild, state, writer) =>
			{
				var startPosition = writer.Position;

				guild.Serialize(writer);

				var size = (int)(writer.Position - startPosition);

				writer.QueueForIndex(guild, size);

				if (_metrics != null)
				{
					_metrics.OnGuildSaved(size);
				}

				return writer;
			},
			writer =>
			{
				writer.Flush();

				_guildThreadWriters.Add(writer);
			});

			_guildThreadWriters.CompleteAdding();

			return commitTask;
		}

		public override void ProcessDecay()
		{
			while (_decayBag.TryTake(out var item))
			{
				if (item.OnDecay())
				{
					item.Delete();
				}
			}
		}

		private void OpenFiles()
		{
			_regionData = new SequentialFileWriter(World.RegionDataPath, _metrics);
			_regionIndex = new SequentialFileWriter(World.RegionIndexPath, _metrics);

			_itemData = new SequentialFileWriter(World.ItemDataPath, _metrics);
			_itemIndex = new SequentialFileWriter(World.ItemIndexPath, _metrics);

			_mobileData = new SequentialFileWriter(World.MobileDataPath, _metrics);
			_mobileIndex = new SequentialFileWriter(World.MobileIndexPath, _metrics);

			_guildData = new SequentialFileWriter(World.GuildDataPath, _metrics);
			_guildIndex = new SequentialFileWriter(World.GuildIndexPath, _metrics);

			WriteCount(_regionIndex, World.Regions.Count);
			WriteCount(_itemIndex, World.Items.Count);
			WriteCount(_mobileIndex, World.Mobiles.Count);
			WriteCount(_guildIndex, BaseGuild.List.Count);

			static void WriteCount(SequentialFileWriter indexFile, int count)
			{
				var buffer = new byte[4];

				buffer[0] = (byte)(count >> 0);
				buffer[1] = (byte)(count >> 8);
				buffer[2] = (byte)(count >> 16);
				buffer[3] = (byte)(count >> 24);

				indexFile.Write(buffer, 0, buffer.Length);
			};
		}

		private void CloseFiles()
		{
			_regionData.Close();
			_regionIndex.Close();

			_itemData.Close();
			_itemIndex.Close();

			_mobileData.Close();
			_mobileIndex.Close();

			_guildData.Close();
			_guildIndex.Close();
		}

		private static void SaveTypeDatabases()
		{
			SaveTypeDatabase(World.RegionTypesPath, World.m_RegionTypes);
			SaveTypeDatabase(World.ItemTypesPath, World.m_ItemTypes);
			SaveTypeDatabase(World.MobileTypesPath, World.m_MobileTypes);

			static void SaveTypeDatabase(string path, List<Type> types)
			{
				using var bfw = new BinaryFileWriter(path, false);

				bfw.Write(types.Count);

				foreach (var type in types)
				{
					bfw.Write(type.FullName);
				}

				bfw.Flush();
				bfw.Close();
			};
		}
	}

	#endregion
}