using System;
using EnginePrimeSync.DB;
using EnginePrimeSync.Exporters;

namespace EnginePrimeSync
{
	class Program
	{
		static void Main(string[] args)
		{
			int choice = 0;

			do
			{
				Console.Clear();
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("Choose from the following options:\n");
				Console.WriteLine("1. Import/Export entire database");
				Console.WriteLine("2. Import/Export playlists");
				Console.WriteLine("3. Import/Export crates");
				Console.WriteLine("4. Import/Export metadata (cues/loops/waveforms/etc)");
				Console.WriteLine("5. Fix file paths");
				Console.WriteLine("6. Exit (or just press enter or anything invalid)\n");
				Console.WriteLine("Choice: ");
				var str = Console.ReadLine();
				if (string.IsNullOrEmpty(str))
					break;

				if (!int.TryParse(str, out choice))
					break;

				if (choice == 1)
					ImportExportEntireDatabase();
				else if (choice == 2)
					ImportExportPlaylists();
				else if (choice == 3)
					ImportExportCrates();
				else if (choice == 5)
					FixPaths();

			}
			while (choice is >= 1 and <= 3);
		}

		private static void ImportExportEntireDatabase()
		{
			var exporter = new ExportDatabase();
			exporter.Run();
		}

		private static void ImportExportPlaylists()
		{
			var exporter = new ExportPlaylists();
			exporter.Run();
		}
		
		private static void ImportExportCrates()
		{
			var exporter = new ExportCrates();
			exporter.Run();
		}

		private static void FixPaths()
		{
			var exporter = new ExportFixedPaths();
			exporter.Run();
		}
	}
}
