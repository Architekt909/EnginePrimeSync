using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnginePrimeSync.DB;

namespace EnginePrimeSync.Exporters
{
	public class ExportDatabase
	{
		private readonly TrackManager _trackManager = new TrackManager();
		private MainDb _mainDb;
		private PerformanceDb _perfDb;


		public ExportDatabase()
		{

		}

		private bool ParseDatabases(string folder)
		{
			try
			{
				_mainDb = new MainDb(folder);
				_mainDb.OpenDb();
				_mainDb.ReadTrackInfo(_trackManager);
				_mainDb.CloseDb();

				_perfDb = new PerformanceDb(folder);
				_perfDb.OpenDb();
				_perfDb.ReadPerformanceInfo(_trackManager);
				_perfDb.CloseDb();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("There was an error opening and/or parsing one or both of the databases.");
				Console.WriteLine("Please check that the following exist and aren't locked:");
				Console.WriteLine(_mainDb.GetDbPath());
				Console.WriteLine(_perfDb.GetDbPath());
				Console.ForegroundColor = ConsoleColor.White;

				return false;
			}

			return true;
		}
		
		public void Run()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("All of these choices will result in a complete overwrite of the destination db.\n");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Choose an option:\n");
			Console.WriteLine("1. EXPORT LOCAL db to EXTERNAL DRIVE (will copy music files too)");
			Console.WriteLine("2. IMPORT database FROM EXTERNAL DRIVE (doesn't copy music)");
			Console.WriteLine("3. Back (or anything invalid)");
			Console.ForegroundColor = ConsoleColor.White;

			var str = Console.ReadLine();
			if (!int.TryParse(str, out int choice))
				return;

			if (choice == 1)
				ExportLocalDb();
			else if (choice == 2)
				ImportExternalDb();
		}

		private void ImportExternalDb()
		{
			var musicPath = GetLocalMusicLibraryPath();
			var sourceDrive = GetDriveLetter(false);

			// Verify m.db and p.db exist on the destination drive
			var sourceFolder = sourceDrive + @"Engine Library\";

			while (!File.Exists(sourceFolder + MainDb.DB_NAME) || !File.Exists(sourceFolder + PerformanceDb.DB_NAME))
			{
				Console.WriteLine($"Enter the folder on drive {sourceDrive} that contains the files {MainDb.DB_NAME} and {PerformanceDb.DB_NAME}");
				Console.WriteLine($"For example: {sourceDrive}Engine Library\nTrailing slash is optional");

				var str = Console.ReadLine();
				if (str == null)
					continue;

				str = str.Trim();
				if ((str[^1] != '/') || (str[^1] != '\\'))
					str += '/';

				sourceFolder = str;
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"We will now copy the databases from {sourceFolder} to {musicPath}");
			Console.WriteLine("Press enter to continue.");
			Console.ForegroundColor = ConsoleColor.White;
			Console.ReadLine();

			var sourceMainDb = sourceFolder + MainDb.DB_NAME;
			var sourcePerfDb = sourceFolder + PerformanceDb.DB_NAME;
			var destMainDb = musicPath + MainDb.DB_NAME;
			var destPerfDb = musicPath + PerformanceDb.DB_NAME;

			try
			{
				File.Copy(sourceMainDb, destMainDb, true);
				File.Copy(sourcePerfDb, destPerfDb, true);
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("There was an error copying either or both of the databases. Aborting.\nI'd advise restoring your destination databases just to be safe.\nPress Enter to return to main menu.");
				Console.ReadLine();
				return;
			}

			// Now we need to remap paths in the destination main db
			using var mainDb = new MainDb(musicPath);
			var trackManager = new TrackManager();

			try
			{
				mainDb.OpenDb();
				if (!mainDb.ReadTrackInfo(trackManager))
					throw new Exception();
			}
			catch (Exception e)
			{
				mainDb.CloseDb();
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("There was an error trying to read the destination databases. Aborting.\nI'd advise restoring your destination databases.\nPress enter to return to main menu.");
				Console.ReadLine();
				return;
			}

			var oldPrefixToNewPrefixMap = trackManager.RemapPrefixesForImporting(musicPath);

			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine($"Updating paths for {oldPrefixToNewPrefixMap.Count} tracks. This may take a while.");
			Console.ForegroundColor = ConsoleColor.White;

			int count = 1;
			foreach (var kvp in oldPrefixToNewPrefixMap)
			{
				Console.WriteLine($"Updating track {count}/{oldPrefixToNewPrefixMap.Count}");
				++count;
				
				if (!mainDb.RemapTrackTablePathColumnForSingleId(kvp.Key, EnginePrimeDb.EXTERNAL_MUSIC_FOLDER, kvp.Value))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"There was an error remapping paths for database:\n{mainDb.GetDbPath()}\nOld prefix: {kvp.Key}\nNew prefix: {kvp.Value}\nPress enter to return to main menu.");
					Console.ForegroundColor = ConsoleColor.White;
					mainDb.CloseDb();
					Console.ReadLine();
					return;
				}
			}

			mainDb.CloseDb();

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Database import and path remapping complete. Press enter to return to main menu.");
			Console.ReadLine();
		}

		private string GetLocalMusicLibraryPath()
		{
			var musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + @"\Engine Library\";
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

		private string GetDriveLetter(bool driveIsDestinationDbLocation)
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

		private void ExportLocalDb()
		{
			var musicPath = GetLocalMusicLibraryPath();
			string destDrive = GetDriveLetter(true);
			
			if (!ParseDatabases(musicPath))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("There was an error trying to parse your database(s). Aborting.");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Press enter to return to main menu.");
				Console.ReadLine();
				return;
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Press enter to erase all music files on destination drive.");
			Console.ForegroundColor = ConsoleColor.White;
			Console.ReadLine();

			string destLibraryPath = destDrive + @"Engine Library\";

			if (Directory.Exists(destLibraryPath + EnginePrimeDb.EXTERNAL_MUSIC_FOLDER))
			{
				try
				{
					Directory.Delete(destLibraryPath + EnginePrimeDb.EXTERNAL_MUSIC_FOLDER, true);
				}
				catch (Exception e)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("There was an error trying to recursively delete the old folder:");
					Console.WriteLine(destLibraryPath + EnginePrimeDb.EXTERNAL_MUSIC_FOLDER);
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("Press enter to return to main menu.");
					Console.ReadLine();
					return;
				}
			}

			if (!Directory.Exists(destLibraryPath))
				Directory.CreateDirectory(destLibraryPath);
			if (!Directory.Exists(destLibraryPath + EnginePrimeDb.EXTERNAL_MUSIC_FOLDER))
				Directory.CreateDirectory(destLibraryPath + EnginePrimeDb.EXTERNAL_MUSIC_FOLDER);

			List<string> oldPrefixes = CopyMusicToDestDrive(destLibraryPath + EnginePrimeDb.EXTERNAL_MUSIC_FOLDER, musicPath);

			if (oldPrefixes == null)
			{
				try
				{
					Directory.Delete(destLibraryPath + EnginePrimeDb.EXTERNAL_MUSIC_FOLDER, true);
				}
				catch (Exception e)
				{
					//
				}

				Console.WriteLine("Press enter to return to main menu.");
				Console.ReadLine();

				return;
			}

			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("File copying completed successfully!. Copying databases over.");
			try
			{
				File.Copy(_mainDb.GetDbPath(), destLibraryPath + MainDb.DB_NAME, true);
				File.Copy(_perfDb.GetDbPath(), destLibraryPath + PerformanceDb.DB_NAME, true);
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error copying one or both databases.\nSource: {_mainDb.GetDbPath()} to {destLibraryPath}{MainDb.DB_NAME}\nSource: {_perfDb.GetDbPath()} to {destLibraryPath}{PerformanceDb.DB_NAME}");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Press enter to return to main menu.");
				Console.ReadLine();
				return;
			}

			if (RemapDestinationDatabasePaths(oldPrefixes, destLibraryPath))
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("\n\nEXPORT COMPLETE.");
			}

			Console.WriteLine("PRESS ENTER TO RETURN TO MENU.");
			Console.ReadLine();


		}

		private bool RemapDestinationDatabasePaths(List<string> oldPrefixes, string destLibraryPath)
		{
			using var destDb = new MainDb(destLibraryPath);
			destDb.OpenDb();

			foreach (var oldPrefix in oldPrefixes)
			{
				if (!destDb.RemapTrackTablePathColumn(oldPrefix, EnginePrimeDb.EXTERNAL_MUSIC_FOLDER))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"There was an error remapping paths for database:\n{destDb.GetDbPath()}\nOld prefix: {oldPrefix}\nNew prefix: {EnginePrimeDb.EXTERNAL_MUSIC_FOLDER}\n");
					Console.ForegroundColor = ConsoleColor.White;
					destDb.CloseDb();
					return false;
				}
			}

			destDb.CloseDb();
			return true;
		}

		private List<string> CopyMusicToDestDrive(string destLibraryPath, string sourceLibraryPath)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine($"Copying {_trackManager.NumTracks()} tracks to {destLibraryPath}");
			Console.ForegroundColor = ConsoleColor.White;

			var oldPrefixes = _trackManager.CopyMusicFiles(destLibraryPath, sourceLibraryPath);
			if (oldPrefixes == null)
			{
				// An error ocurred
				return null;
			}

			return oldPrefixes;
		}

		
	}
}
