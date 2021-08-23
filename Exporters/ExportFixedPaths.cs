using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnginePrimeSync.DB;

namespace EnginePrimeSync.Exporters
{
	public class ExportFixedPaths : ExporterBase<Track>
	{
		private readonly TrackManager _trackManager;

		public ExportFixedPaths()
		{
			_objectManager = new TrackManager();
			_trackManager = _objectManager as TrackManager;
		}

		public override void Run()
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			var libraryPath = GetDefaultMusicLibraryPath();

			bool valid = true;
			var mDbPath = libraryPath + MainDb.DB_NAME;
			if (File.Exists(mDbPath))
			{
				Console.WriteLine("Found default library path on local machine at:");
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(libraryPath);
				Console.ForegroundColor = ConsoleColor.White;

				bool result = PromptYesNoQuestion("Would you like to fix paths for this database?", "[y/n]");
				if (!result)
					valid = false;
			}
			else
				valid = false;
			
			while (!valid)
			{
				Console.WriteLine("\nEnter FULL PATH to the engine library folder that contains your databases.");
				Console.WriteLine(@"For example, if your database to fix resides at C:\users\myname\Music\Engine Library");
				Console.WriteLine(@"You'd simply enter: C:\users\myname\Music\Engine Library");
				Console.WriteLine(@"You can also enter the path to a library on an external drive, or any drive at all such as network shares.");
				libraryPath = Console.ReadLine();

				if (string.IsNullOrEmpty(libraryPath))
					continue;

				libraryPath = libraryPath.Trim();
				if ((libraryPath[^1] != '/') && (libraryPath[^1] != '\\'))
					libraryPath += "\\";

				mDbPath = libraryPath + MainDb.DB_NAME;
				if (!File.Exists(mDbPath))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Can't find file: {mDbPath}\nPlease try again.\n");
					Console.ForegroundColor = ConsoleColor.White;
				}
				else
					valid = true;
			}

			if (!ParseMainDatabase(libraryPath))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"There was an error opening and/or parsing {_mainDb.GetDbPath()}.");
				Console.WriteLine("Please check that the following exists and isn't locked:");
				Console.WriteLine(_mainDb.GetDbPath());
				Console.WriteLine("Press enter to return to main menu.");
				Console.ForegroundColor = ConsoleColor.White;
				Console.ReadLine();
				return;
			}

			var trackIdToOldPathMap = new Dictionary<int, string>();
			var trackIdToNewPathMap = _trackManager.RemapPrefixesForImportingOrFixing(libraryPath, true, trackIdToOldPathMap);

			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine($"Updating paths for {trackIdToNewPathMap.Count} tracks. This may take a while.");
			Console.ForegroundColor = ConsoleColor.White;

			_mainDb.OpenDb();
			if (!_mainDb.RemapTrackTablePathColumnForIds(trackIdToNewPathMap, trackIdToOldPathMap))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"There was an error remapping paths for database:\n{_mainDb.GetDbPath()}\nPress enter to return to main menu.");
				Console.ForegroundColor = ConsoleColor.White;
				_mainDb.CloseDb();
				Console.ReadLine();
				return;
			}

			_mainDb.CloseDb();
		}

		private bool ParseMainDatabase(string folder)
		{
			try
			{
				_mainDb = new MainDb(folder);
				_mainDb.OpenDb();
				if (!_mainDb.ReadTrackInfo(_trackManager))
					throw new Exception();
				
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
	}
}
