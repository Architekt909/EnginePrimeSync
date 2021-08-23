using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnginePrimeSync.DB;

namespace EnginePrimeSync.Exporters
{
	public abstract class ExportCollection<T> : ExporterBase<T>
										where T : DbObject
	{
		protected string _objectNamePlural;
		
		public override void Run()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"All of these choices will result in a complete overwrite of the destination {_objectNamePlural}.\n");
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Choose an option:\n");
			Console.WriteLine($"1. EXPORT LOCAL {_objectNamePlural} to EXTERNAL DRIVE");
			Console.WriteLine($"2. IMPORT {_objectNamePlural} FROM EXTERNAL DRIVE");
			Console.WriteLine("3. Back (or anything invalid)");
			Console.ForegroundColor = ConsoleColor.White;

			var str = Console.ReadLine();
			if (!int.TryParse(str, out int choice))
				return;

			if (choice == 1)
				ImportExportObjects(true);
			else if (choice == 2)
				ImportExportObjects(false);
		}

		protected virtual void ImportExportObjects(bool export)
		{
			var localLibraryPath = GetLocalMusicLibraryPath();
			var externalDrive = GetDriveLetter(export);


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
			MainDb destinationDb;
			MainDb sourceDb;

			if (export)
			{
				destinationDb = externalMainDb;
				sourceDb = _mainDb = new MainDb(localLibraryPath);
			}
			else
			{
				sourceDb = externalMainDb;
				destinationDb = _mainDb = new MainDb(localLibraryPath);
			}

			sourceDb.OpenDb();
			if (!ReadSourceContent(sourceDb))
			{
				sourceDb.CloseDb();
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error reading {_objectNamePlural} from source database. Press enter to return to main menu.");
				Console.ForegroundColor = ConsoleColor.White;
				Console.ReadLine();
				return;
			}
			sourceDb.CloseDb();

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Press enter to wipe all {_objectNamePlural} from destination database.");
			Console.ForegroundColor = ConsoleColor.White;
			Console.ReadLine();


			destinationDb.OpenDb();

			if (!DeleteContent(destinationDb))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"There was an error trying to delete the destination {_objectNamePlural}.\nPlease restore your database and try again.\nPress enter to return to main menu.");
				destinationDb.CloseDb();
				Console.ReadLine();
				return;
			}

			if (!WriteContent(sourceDb, destinationDb))
			{
				Console.WriteLine("Press enter to return to main menu.");
				Console.ForegroundColor = ConsoleColor.White;
				destinationDb.CloseDb();
				Console.ReadLine();
			}

			destinationDb.CloseDb();

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine($"{(export ? "Export" : "Import")} complete. Press enter to return to main menu.");
			Console.ReadLine();
		}

		protected abstract bool DeleteContent(MainDb destinationDb);
		protected abstract bool WriteContent(MainDb sourceDb, MainDb destinationDb);
		protected abstract bool ReadSourceContent(MainDb sourceDb);
	}
}
