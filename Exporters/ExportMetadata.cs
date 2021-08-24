using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnginePrimeSync.DB;

namespace EnginePrimeSync.Exporters
{
	public class ExportMetadata : ExporterBase<Track>
	{
		private readonly TrackManager _trackManager;

		public ExportMetadata()
		{
			_objectManager = new TrackManager();
			_trackManager = _objectManager as TrackManager;
		}

		public override void Run()
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Choose an option:\n");
			Console.WriteLine("1. EXPORT LOCAL metadata to EXTERNAL DRIVE");
			Console.WriteLine("2. IMPORT metadata FROM EXTERNAL DRIVE");
			Console.WriteLine("3. Back (or anything invalid)");
			Console.ForegroundColor = ConsoleColor.White;

			var str = Console.ReadLine();
			if (!int.TryParse(str, out int choice))
				return;

			ImportExportMetadata(choice == 1);

			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine("Press enter to return to main menu");
			Console.ReadLine();
		}

		private void ImportExportMetadata(bool export)
		{
			bool exportCues = false;
			bool exportLoops = false;
			bool everything = false;

			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("Which metadata do you wish to " + (export ? "export?" : "import?"));
			Console.WriteLine("1. Just cue points");
			Console.WriteLine("2. Just loop info");
			Console.WriteLine("3. Both cues AND loops");
			Console.WriteLine("4. Everything (includes beat data, waveform analysis, cues, loops, etc)");
			Console.WriteLine("5. Back (or anything invalid)");

			var str = Console.ReadLine();
			if (!int.TryParse(str, out int choice))
				return;

			if (choice == 1)
				exportCues = true;
			else if (choice == 2)
				exportLoops = true;
			else if (choice == 3)
				exportCues = exportLoops = true;
			else if (choice == 4)
				everything = true;
			else
				return;

			var localLibraryFolder = GetLocalMusicLibraryPath();
			var externalDrivePath = GetDriveLetter(export);

			var externalDbPath = externalDrivePath + EnginePrimeDb.ENGINE_FOLDER_SLASH;

			if (!File.Exists(externalDbPath + PerformanceDb.DB_NAME) || !File.Exists(externalDbPath + MainDb.DB_NAME))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Couldn't find databases on external drive at: ");
				Console.WriteLine(externalDbPath);
				Console.WriteLine("Please ensure this file exists. If need be, do a full export of your local database.");
				Console.WriteLine("Or maybe you typed the wrong path.");
				return;
			}

			_perfDb = new PerformanceDb(localLibraryFolder);
			PerformanceDb externalPerfDb = new PerformanceDb(externalDbPath);
			List<int> nonMatchingIds;

			PerformanceDb sourceDb, destDb;
			if (export)
			{
				sourceDb = _perfDb;
				destDb = externalPerfDb;

				_mainDb = new MainDb(localLibraryFolder);
				_mainDb.OpenDb();
				_mainDb.ReadTrackInfo(_trackManager);
				_mainDb.CloseDb();

				var otherManager = new TrackManager();
				var otherMainDb = new MainDb(externalDbPath);
				otherMainDb.OpenDb();
				otherMainDb.ReadTrackInfo(otherManager);
				otherMainDb.CloseDb();

				nonMatchingIds = _trackManager.VerifyTrackIdsAreTheSame(otherManager);
			}
			else
			{
				sourceDb = externalPerfDb;
				destDb = _perfDb;

				_mainDb = new MainDb(externalDbPath);
				_mainDb.OpenDb();
				_mainDb.ReadTrackInfo(_trackManager);
				_mainDb.CloseDb();

				var otherManager = new TrackManager();
				var otherMainDb = new MainDb(localLibraryFolder);
				otherMainDb.OpenDb();
				otherMainDb.ReadTrackInfo(otherManager);
				otherMainDb.CloseDb();

				nonMatchingIds = _trackManager.VerifyTrackIdsAreTheSame(otherManager);
			}

			if (nonMatchingIds.Count > 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"There are {nonMatchingIds.Count} tracks whose IDs don't match up.");
				Console.WriteLine("This makes it impossible to accurately copy metadata for tracks.");
				Console.WriteLine("To ensure that the track IDs match up, please run a full import or export");
				Console.WriteLine("which will ensure that the databases match up. This usually happens if you");
				Console.WriteLine("have only used the Engine Prime sync feature and/or have never used this tool's");
				Console.WriteLine("import/export full database option. Engine Prime doesn't always mirror track IDs.");
				return;
			}

			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine($"Updating {_trackManager.Count()} tracks. This may take anywhere from a few seconds to minutes depending on size.");
			Console.ForegroundColor = ConsoleColor.White;

			try
			{
				sourceDb.OpenDb();
				_ = sourceDb.CopyMetadataToOtherDb(destDb, everything, exportCues, exportLoops);
			}
			finally
			{
				sourceDb.CloseDb();
			}
		}
	}
}
