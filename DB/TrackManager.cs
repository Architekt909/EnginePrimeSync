using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	
	public class TrackManager
	{
		private Dictionary<int, Track> _idToTrackMap = new Dictionary<int, Track>();

		public TrackManager()
		{
		}

		public void AddTrack(Track track) => _idToTrackMap[track.Id] = track;
		public Track GetTrack(int id) => _idToTrackMap.ContainsKey(id) ? _idToTrackMap[id] : null;
		public int NumTracks() => _idToTrackMap.Count;
		public void Clear() => _idToTrackMap.Clear();

		// Returns a list of prefixes to strip out of the source database and replace with the standard destination drive music folder
		// destFolder should NOT contain trailing slash, while sourceLibraryPath SHOULD contain trailing slash
		public List<string> CopyMusicFiles(string destFolder, string sourceLibraryPath)
		{
			var oldPrefixes = new List<string>();
			int count = 1;

			foreach (var kvp in _idToTrackMap)
			{
				var track = kvp.Value;

				string fullFilePath = sourceLibraryPath + track.Path;
				string prefix = null;

				bool foundPrefix = false;
				foreach (var oldPrefix in oldPrefixes)
				{
					if (track.Path.IndexOf(oldPrefix) == 0)
					{
						prefix = oldPrefix;
						foundPrefix = true;
						break;
					}
				}

				if (!foundPrefix)
					prefix = RemapPrefixes(oldPrefixes, track);
				
				var strippedPath = track.Path.Substring(prefix.Length + 1); // +1 to remove trailing slash. This now just contains the folder the file is in without the root.
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
					Console.WriteLine($"Copying file {count}/{_idToTrackMap.Count}: {fullDestFilePath}");
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

			return oldPrefixes;
		}

		private string RemapPrefixes(List<string> oldPrefixes, Track track)
		{
			bool done = false;
			string prefix = null;

			while (!done)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("Please enter the prefix you wish to replace from the string below (DON'T include trailing slash): ");
				Console.WriteLine(track.Path);
				Console.ForegroundColor = ConsoleColor.White;
				prefix = Console.ReadLine();
				if (prefix == null)
					continue;

				prefix = prefix.Trim();
				if (track.Path.IndexOf(prefix, StringComparison.CurrentCultureIgnoreCase) != 0)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"The path we need to remap: {track.Path}");
					Console.WriteLine($"Does not start with: {prefix}");
					Console.ForegroundColor = ConsoleColor.White;
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Cyan;
					var newPath = EnginePrimeDb.EXTERNAL_MUSIC_FOLDER + track.Path.Substring(prefix.Length);
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

					oldPrefixes.Add(prefix);
					done = true;
				}
			}

			return prefix;
		}
	}
}
