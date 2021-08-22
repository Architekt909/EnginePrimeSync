using System;
using System.IO;
using EnginePrimeSync.DB;

namespace EnginePrimeSync.Exporters
{
	public class ExportPlaylists : ExporterBase
	{
		private readonly PlaylistManager _playlistManager = new PlaylistManager();

		public ExportPlaylists()
		{

		}

		public override void Run()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("All of these choices will result in a complete overwrite of the destination playlists.\n");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Choose an option:\n");
			Console.WriteLine("1. EXPORT LOCAL playlists to EXTERNAL DRIVE");
			Console.WriteLine("2. IMPORT playlists FROM EXTERNAL DRIVE");
			Console.WriteLine("3. Back (or anything invalid)");
			Console.ForegroundColor = ConsoleColor.White;

			var str = Console.ReadLine();
			if (!int.TryParse(str, out int choice))
				return;

			if (choice == 1)
				ImportExportPlaylists(true);
			else if (choice == 2)
				ImportExportPlaylists(false);

		}
		
		private void ImportExportPlaylists(bool export)
		{
			var localLibraryPath = GetLocalMusicLibraryPath();
			var externalDrive = GetDriveLetter(export);
			
			if (!ParseMainDatabase(localLibraryPath))
			{
				Console.WriteLine("Press enter to return to the main menu.");
				Console.ReadLine();
				return;
			}

			Console.WriteLine("This process assumes that you already have a valid main database at the destination location.");
			Console.WriteLine("If you do not, you should do a full export from the main menu first.");
			Console.WriteLine("Otherwise your track IDs might not match up and your playlists may be inaccurately referencing other tracks.");
			if (PromptYesNoQuestion("Return to main menu?", "[y/n]: "))
				return;

			var externalLibraryRoot = externalDrive + EnginePrimeDb.ENGINE_FOLDER_SLASH;
			var externalMainDbPath = externalLibraryRoot + MainDb.DB_NAME;
			
			if (!File.Exists(externalMainDbPath))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Can't find main database on external drive: {externalMainDbPath}");
				Console.WriteLine("Press enter to return to main menu.");
				Console.ForegroundColor = ConsoleColor.White;

				Console.ReadLine();
				return;
			}

			using var externalMainDb = new MainDb(externalLibraryRoot);

			var destinationDb = export ? externalMainDb : _mainDb;
			var sourceDb = export ? _mainDb : externalMainDb;

			sourceDb.OpenDb();
			if (!sourceDb.ReadPlaylists(_playlistManager))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error reading playlists from source database. Press enter to return to main menu.");
				Console.ForegroundColor = ConsoleColor.White;
			}
			sourceDb.CloseDb();

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("Press enter to wipe all playlists from destination database.");
			Console.ForegroundColor = ConsoleColor.White;
			Console.ReadLine();


			destinationDb.OpenDb();

			if (!destinationDb.DeletePlaylists())
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("There was an error trying to delete the destination playlists.\nPlease restore your database and try again.\nPress enter to return to main menu.");
				destinationDb.CloseDb();
				Console.ReadLine();
				return;
			}

			if (!destinationDb.WritePlaylists(_playlistManager))
			{
				Console.WriteLine("Press enter to return to main menu.");
				Console.ForegroundColor = ConsoleColor.White;
				destinationDb.CloseDb();
				Console.ReadLine();
			}

			destinationDb.CloseDb();

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"Playlist {(export ? "export" : "import")} complete. Press enter to return to main menu.");
			Console.ReadLine();
		}

	}
}
