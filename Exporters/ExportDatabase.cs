using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnginePrimeSync.DB;

namespace EnginePrimeSync.Exporters
{
	public class ExportDatabase : ExporterBase
	{
		
		
		public ExportDatabase()
		{

		}

		private bool ParseDatabases(string folder)
		{
			if (!ParseMainDatabase(folder))
				return false;

			return ParsePerformanceDatabase(folder);
		}

		public override void Run()
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
			var sourceFolder = sourceDrive + EnginePrimeDb.ENGINE_FOLDER_SLASH;

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

			string destLibraryPath = destDrive + EnginePrimeDb.ENGINE_FOLDER_SLASH;

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
