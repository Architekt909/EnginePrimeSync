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
				Console.WriteLine("4. Exit (or just press enter or anything invalid)\n");
				Console.WriteLine("Choice: ");
				var str = Console.ReadLine();
				if (string.IsNullOrEmpty(str))
					break;

				if (!int.TryParse(str, out choice))
					break;

				if (choice == 1)
					ImportExportEntireDatabase();
			}
			while (choice is >= 1 and <= 3);
		}

		private static void ImportExportEntireDatabase()
		{
			var exporter = new ExportDatabase();
			exporter.Run();
		}
		
	}
}
