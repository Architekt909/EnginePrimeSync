using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public abstract class ExporterBase
	{
		protected readonly TrackManager _trackManager = new TrackManager();
		protected MainDb _mainDb;
		protected PerformanceDb _perfDb;
		

		protected ExporterBase()
		{
		}

		public abstract void Run();

		protected string GetLocalMusicLibraryPath()
		{
			var musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + $"\\{EnginePrimeDb.ENGINE_FOLDER_SLASH}";
			bool useDefault = true;

			if (File.Exists(musicPath + MainDb.DB_NAME) && File.Exists(musicPath + PerformanceDb.DB_NAME))
			{
				Console.WriteLine($"Found local databases at: {musicPath}");
				Console.WriteLine("Would you like to use this default?");

				bool invalid = true;
				while (invalid)
				{
					Console.Write("[Y/n]: ");
					var str = Console.ReadLine();
					if (str == null)
						continue;

					str = str.Trim();

					if ((str.Length == 0) || str.Equals("y", StringComparison.CurrentCultureIgnoreCase))
					{
						useDefault = true;
						invalid = false;
					}
					else if (str.Equals("n", StringComparison.CurrentCultureIgnoreCase))
					{
						useDefault = false;
						invalid = false;
					}
				}

			}
			else
				useDefault = false;

			while (!useDefault)
			{
				Console.WriteLine($"Enter full path to the LOCAL PC FOLDER containing {MainDb.DB_NAME} and {PerformanceDb.DB_NAME}:");
				musicPath = Console.ReadLine();
				if (musicPath == null)
					continue;

				if (musicPath[^1] != '\\')
					musicPath += '\\';

				if (!File.Exists(musicPath + MainDb.DB_NAME) || !File.Exists(musicPath + PerformanceDb.DB_NAME))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Can't find {MainDb.DB_NAME} and/or {PerformanceDb.DB_NAME} at: {musicPath}");
					Console.ForegroundColor = ConsoleColor.White;
				}
				else
					useDefault = true;
			}

			return musicPath;
		}

		protected string GetDriveLetter(bool driveIsDestinationDbLocation)
		{
			string destDrive = null;
			while (destDrive == null)
			{
				Console.Write(driveIsDestinationDbLocation ? "Enter DESTINATION DRIVE LETTER! " : "Enter SOURCE DRIVE LETTER! ");
				Console.WriteLine("Just drive letter, i.e. F or F: or F:\\!");

				destDrive = Console.ReadLine();
				if (destDrive == null)
					continue;

				destDrive = destDrive.Trim();
				if (destDrive.Length == 1)
					destDrive += ":\\";
				else if (destDrive.Length == 2)
					destDrive += '\\';

				if (!Directory.Exists(destDrive))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Invalid destination directory: {destDrive}");
					Console.ForegroundColor = ConsoleColor.White;
					destDrive = null;
				}
			}

			return destDrive;
		}

		// returns true for yes false for no
		protected virtual bool PromptYesNoQuestion(string message, string choicePrompt = null)
		{
			string str = null;
			while (str == null)
			{
				Console.WriteLine(message);
				if (choicePrompt != null)
					Console.Write(choicePrompt);

				str = Console.ReadLine();
				if (string.IsNullOrEmpty(str))
				{
					str = null;
					continue;
				}

				str = str.Trim();
				if (str.Equals("y", StringComparison.CurrentCultureIgnoreCase))
					return true;
				else if (str.Equals("n", StringComparison.CurrentCultureIgnoreCase))
					return false;
			}

			return false;
		}

		// Just the top level folder with trailing slash
		protected virtual bool ParseMainDatabase(string folder)
		{
			try
			{
				_mainDb = new MainDb(folder);
				_mainDb.OpenDb();
				_mainDb.ReadTrackInfo(_trackManager);
				_mainDb.CloseDb();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"There was an error opening and/or parsing {_mainDb.GetDbPath()}.");
				Console.WriteLine("Please check that the following exists and isn't locked:");
				Console.WriteLine(_mainDb.GetDbPath());
				Console.ForegroundColor = ConsoleColor.White;

				return false;
			}

			return true;
		}

		// Just the top level folder with trailing slash
		protected virtual bool ParsePerformanceDatabase(string folder)
		{
			try
			{
				_perfDb = new PerformanceDb(folder);
				_perfDb.OpenDb();
				_perfDb.ReadPerformanceInfo(_trackManager);
				_perfDb.CloseDb();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"There was an error opening and/or parsing {_perfDb.GetDbPath()}.");
				Console.WriteLine("Please check that the following exists and isn't locked:");
				Console.WriteLine(_perfDb.GetDbPath());
				Console.ForegroundColor = ConsoleColor.White;

				return false;
			}

			return true;
		}
	}
}
