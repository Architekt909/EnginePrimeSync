using System;
using System.Collections.Generic;
using System.IO;

namespace EnginePrimeSync.DB
{
	
	public class TrackManager : DbObjectManager<Track>
	{
		public TrackManager()
		{
			_idToObjectMap = new Dictionary<int, Track>();
		}

		// Doesn't copy any files. Returns map of track ID to new path string that should be set. Should include trailing slash
		public Dictionary<int, string> RemapPrefixesForImporting(string destLibraryFolder)
		{
			var trackIdToNewPathMap = new Dictionary<int, string>();

			bool requireRemap = true;
			string oldPrefix = EnginePrimeDb.EXTERNAL_MUSIC_FOLDER;
			string newPrefix = null;

			foreach (var kvp in _idToObjectMap)
			{
				var track = kvp.Value;
				bool done = false;

				while (!done)
				{
					if (requireRemap)
						newPrefix = RemapPrefixes(track, false, ref oldPrefix);
					
					// Verify this file exists locally
					var newFullFilePath = destLibraryFolder + newPrefix + track.Path.Substring(oldPrefix.Length);
					if (!File.Exists(newFullFilePath))
					{
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine($"\nRemapping source file: {track.Path}");
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine($"Couldn't find file at destination: {newFullFilePath}");
						Console.WriteLine("You will be prompted to try again to fix any prefix problems. Nothing has been modified at this point.");
						Console.ForegroundColor = ConsoleColor.White;
						requireRemap = true;
					}
					else
					{
						requireRemap = false;
						done = true;

						trackIdToNewPathMap[track.Id] = newPrefix;
					}
				}
			}

			return trackIdToNewPathMap;
		}

		/*
			Returns a dictionary where the key is the old prefix to strip out and the value is a list of IDs this applies to
			destFolder should NOT contain trailing slash, while sourceLibraryPath SHOULD contain trailing slash
		 */
		public Dictionary<string, List<int>> CopyMusicFiles(string destFolder, string sourceLibraryPath)
		{
			var oldPrefixForTrackIdsMap = new Dictionary<string, List<int>>();
			var oldPrefixes = new List<string>();

			int count = 1;

			foreach (var kvp in _idToObjectMap)
			{
				var track = kvp.Value;

				string fullFilePath = sourceLibraryPath + track.Path;
				string oldPrefix = null;

				bool foundPrefix = false;
				foreach (var old in oldPrefixes)
				{
					if (track.Path.IndexOf(old) == 0)
					{
						oldPrefix = old;
						foundPrefix = true;
						break;
					}
				}

				if (!foundPrefix)
				{
					_ = RemapPrefixes(track, true, ref oldPrefix);
					oldPrefixes.Add(oldPrefix);
				}

				List<int> ids;
				if (oldPrefixForTrackIdsMap.ContainsKey(oldPrefix))
					ids = oldPrefixForTrackIdsMap[oldPrefix];
				else
				{
					ids = new List<int>();
					oldPrefixForTrackIdsMap[oldPrefix] = ids;
				}

				ids.Add(track.Id);

				var strippedPath = track.Path.Substring(oldPrefix.Length + 1); // +1 to remove trailing slash. This now just contains the folder the file is in without the root.
				var lastSlash = strippedPath.LastIndexOf('/');
				var trackDir = strippedPath.Substring(0, lastSlash);

				var fullDestDir = destFolder + "/" + trackDir;
				if (!Directory.Exists(fullDestDir))
				{
					try
					{
						Directory.CreateDirectory(fullDestDir);
					}
					catch (Exception e)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"There was an error trying to create this directory on the target drive:\n{fullDestDir}\nAborting and cleaning up destination music folder.");
						Console.ForegroundColor = ConsoleColor.White;
						return null;
					}
				}

				var fullDestFilePath = destFolder + "/" + strippedPath;
				try
				{
					Console.WriteLine($"Copying file {count}/{_idToObjectMap.Count}: {fullDestFilePath}");
					File.Copy(fullFilePath, fullDestFilePath);
					++count;
				}
				catch (Exception e)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"There was an error trying to copy from:\n{fullFilePath}\nto: {fullDestFilePath}\n");
					Console.ForegroundColor = ConsoleColor.White;
					return null;
				}
			}

			return oldPrefixForTrackIdsMap;
		}

		// New prefix is returned.
		private string RemapPrefixes(Track track, bool exporting, ref string oldPrefix)
		{
			bool done = false;
			string newPrefix = null;

			while (!done)
			{
				if (exporting)
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("Please enter the prefix you wish to replace from the string below (DON'T include trailing slash): ");
					Console.WriteLine(track.Path);
					Console.ForegroundColor = ConsoleColor.White;
					oldPrefix = Console.ReadLine();
					if (oldPrefix == null)
						continue;
				}

				string newPath = string.Empty;

				if (!exporting)
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine("Please enter the new prefix that the tracks should be remapped to.");
					Console.WriteLine("This path must be RELATIVE to the location of your destination database folder!");
					Console.WriteLine(@"For example, if your destination folder is D:\Music\Engine Library\ and the");
					Console.WriteLine(@"toplevel folder containing your music is D:\Music\Archived Vinyl\ you would want to enter:");
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine(@"../ArchivedVinyl (DO NOT INCLUDE TRAILING SLASH, and yes, you need to use / not \\)");
					Console.ForegroundColor = ConsoleColor.Magenta;
					Console.WriteLine($"The current track is: {track.Path}");
					Console.ForegroundColor = ConsoleColor.White;
					newPrefix = Console.ReadLine();
					if (newPrefix == null)
						continue;

					newPrefix = newPrefix.Trim();
					newPath = newPrefix + track.Path.Substring(oldPrefix.Length);
				}

				oldPrefix = oldPrefix.Trim();
				if (track.Path.IndexOf(oldPrefix, StringComparison.CurrentCultureIgnoreCase) != 0)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"The path we need to remap: {track.Path}");
					Console.WriteLine($"Does not start with: {oldPrefix}");
					Console.ForegroundColor = ConsoleColor.White;
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					if (exporting)
					{
						newPrefix = EnginePrimeDb.EXTERNAL_MUSIC_FOLDER;
						newPath = newPrefix + track.Path.Substring(oldPrefix.Length);
					}
					

					Console.Write("The path from the source database: ");
					Console.ForegroundColor = ConsoleColor.Magenta;
					Console.WriteLine(track.Path);
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Write("Will be remapped to: ");
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine($"{newPath} in the destination database.");
					Console.ForegroundColor = ConsoleColor.White;
					Console.WriteLine("Is this OK? Enter 'n' to redo prefix mapping.");
					Console.Write("[y/N]: ");

					string choice = Console.ReadLine();
					if (choice == null)
						continue;

					choice = choice.Trim();
					if (!choice.Equals("y", StringComparison.CurrentCultureIgnoreCase))
						continue;

					done = true;
				}
			}

			return newPrefix;
		}
	}
}
