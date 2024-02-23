using Server.Accounting;
using Server.Guilds;
using Server.Network;

using System;
using System.IO;
using System.Net.Mail;

namespace Server.Misc
{
	public class CrashGuard
	{
		private static readonly bool Enabled = true;
		private static readonly bool SaveBackup = true;
		private static readonly bool RestartServer = true;
		private static readonly bool GenerateReport = true;

		public static void Initialize()
		{
			if (Enabled) // If enabled, register our crash event handler
			{
				EventSink.Crashed += CrashGuard_OnCrash;
			}
		}

		public static void CrashGuard_OnCrash(CrashedEventArgs e)
		{
			if (GenerateReport)
			{
				GenerateCrashReport(e);
			}

			World.WaitForWriteCompletion();

			if (SaveBackup)
			{
				Backup();
			}

			if (RestartServer)
			{
				Restart(e);
			}
		}

		private static void SendEmail(string filePath)
		{
			Console.Write("Crash: Sending email...");

			var message = new MailMessage(Email.FromAddress, Email.CrashAddresses) 
			{
				Subject = "Automated RunUO Crash Report",
				Body = "Automated RunUO Crash Report. See attachment for details."
			};

			message.Attachments.Add(new Attachment(filePath));

			if (Email.Send(message))
			{
				Console.WriteLine("done");
			}
			else
			{
				Console.WriteLine("failed");
			}
		}

		private static string GetRoot()
		{
			try
			{
				return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
			}
			catch
			{
				return "";
			}
		}

		private static void Restart(CrashedEventArgs e)
		{
			Console.Write("Crash: Restarting...");

			try
			{
				e.Close = true;

				Core.Kill(true);

				Console.WriteLine("done");
			}
			catch
			{
				Console.WriteLine("failed");
			}
		}

		private static void Backup()
		{
			Console.Write("Crash: Backing up...");

			try
			{
				if (World.Loading || World.Saving)
				{
					World.WaitForWriteCompletion();
				}

				var timeStamp = GetTimeStamp();

				var root = GetRoot();

				var rootBackup = Path.Combine(root, "Export", "Saves", "Crashed", timeStamp);
				var rootOrigin = Core.CurrentSavesDirectory;

				foreach (var file in Directory.EnumerateFiles(rootOrigin, "*", SearchOption.AllDirectories))
				{
					var path = file.Replace(rootOrigin, rootBackup);

					Directory.CreateDirectory(Path.GetDirectoryName(path));

					File.Copy(file, path, true);
				}

				Console.WriteLine("done");
			}
			catch
			{
				Console.WriteLine("failed");
			}
		}

		private static void GenerateCrashReport(CrashedEventArgs e)
		{
			Console.Write("Crash: Generating report...");

			try
			{
				var timeStamp = GetTimeStamp();
				var root = GetRoot();
				var filePath = Path.Combine(root, $"Crash {timeStamp}.log");

				using (var op = new StreamWriter(filePath))
				{
					var ver = Core.Assembly.GetName().Version;

					op.WriteLine("Server Crash Report");
					op.WriteLine("===================");
					op.WriteLine();
					op.WriteLine($"uoAvos Version {ver.Major}.{ver.Minor}, Build {ver.Build}.{ver.Revision}");
					op.WriteLine($"Operating System: {Environment.OSVersion}");
					op.WriteLine($".NET: {Environment.Version}");
					op.WriteLine($"Date: {timeStamp}");

					op.WriteLine($"Regions: {World.Regions.Count:N0}");
					op.WriteLine($"Mobiles: {World.Mobiles.Count:N0}");
					op.WriteLine($"Items: {World.Items.Count:N0}");
					op.WriteLine($"Guilds: {BaseGuild.List.Count:N0}");

					op.WriteLine("Exception:");
					op.WriteLine(e.Exception);
					op.WriteLine();

					op.WriteLine("Clients:");

					try
					{
						var states = NetState.Instances;

						op.WriteLine($"- Count: {states.Count:N0}");

						for (var i = 0; i < states.Count; ++i)
						{
							var state = states[i];

							op.Write($"+ {state}:");

							var a = state.Account as Account;

							if (a != null)
							{
								op.Write($" (account = {a.Username})");
							}

							var m = state.Mobile;

							if (m != null)
							{
								op.Write($" (mobile = 0x{m.Serial.Value:X} '{m.Name}')");
							}

							op.WriteLine();
						}
					}
					catch
					{
						op.WriteLine("- Failed");
					}
				}

				Console.WriteLine("done");

				if (Email.FromAddress != null && Email.CrashAddresses != null)
				{
					SendEmail(filePath);
				}
			}
			catch
			{
				Console.WriteLine("failed");
			}
		}

		private static string GetTimeStamp()
		{
			var now = DateTime.Now;

			var month = now.Month switch
			{
				1 => "JAN",
				2 => "FEB",
				3 => "MAR",
				4 => "APR",
				5 => "MAY",
				6 => "JUN",
				7 => "JUL",
				8 => "AUG",
				9 => "SEP",
				10 => "OCT",
				11 => "NOV",
				12 => "DEC",
				_ => "UNK",
			};

			return $"{now.DayOfWeek} {now.Day} {month} ({now.Hour:D2}-{now.Minute:D2}-{now.Second:D2})";
		}
	}
}