using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace EnginePrimeSync.DB
{
	public class MainDb : EnginePrimeDb
	{
		public const string DB_NAME = "m.db";

		// Expects dbPath to contain a trailing slash
		public MainDb(string dbPath) : base(dbPath + DB_NAME)
		{
		}

		public override string GetDbPath() => _dbPath;
		
		// This assumes trackManager doesn't contain any data yet
		public bool ReadTrackInfo(TrackManager trackManager)
		{
			if (trackManager.Count() != 0)
				return false;

			if (!ParseTrackTable(trackManager))
				return false;

			if (!ParseMetadataTable(trackManager))
				return false;

			return true;
		}

		public bool DeleteCrates()
		{
			var views = new [] { "CrateTrackList", "CrateParentList", "CrateHierarchy", "Crate" };
			var ids = new[] { "crateId", "crateOriginId", "crateId", "id" };
			for (int i = 0; i < views.Length; i++)
			{
				var view = views[i];
				var id = ids[i];

				try
				{
					using var command = _connection.CreateCommand();
					command.CommandText = $"DELETE FROM {view} WHERE {id} >= 0";
					command.ExecuteNonQuery();
				}
				catch (Exception e)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Error trying to delete from {view}");
					Console.ForegroundColor = ConsoleColor.White;
					return false;
				}
			}

			return true;
		}

		public bool ReadCrates(CrateManager crateManager)
		{
			if (crateManager.Count() != 0)
				return false;

			if (!ParseCrateTable(crateManager))
				return false;

			if (!ParseCrateParentListTable(crateManager))
				return false;

			crateManager.FindParentCrateIds();

			return ParseCrateTrackListTable(crateManager);
		}

		private bool ParseCrateTable(CrateManager crateManager)
		{
			using var command = _connection.CreateCommand();
			command.CommandText = @"SELECT id,title,path FROM Crate ORDER BY id";
			try
			{
				using var reader = command.ExecuteReader();
				while (reader.Read())
				{
					Crate c = new Crate(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
					crateManager.Add(c);
				}
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		private bool ParseCrateParentListTable(CrateManager crateManager)
		{
			using var command = _connection.CreateCommand();
			command.CommandText = @"SELECT crateOriginId,crateParentId FROM CrateParentList ORDER BY crateOriginId";
			try
			{
				using var reader = command.ExecuteReader();
				while (reader.Read())
				{
					int crateId = reader.GetInt32(0);
					int immediateParentId = reader.GetInt32(1);

					var crate = crateManager.GetById(crateId);
					crate.ImmediateParentCrateId = immediateParentId;
				}
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		private bool ParseCrateTrackListTable(CrateManager crateManager)
		{
			var ids = crateManager.GetIds();
			foreach (var id in ids)
			{
				Crate crate = crateManager.GetById(id);

				try
				{
					using var command = _connection.CreateCommand();
					command.CommandText = @"SELECT trackId FROM CrateTrackList WHERE crateId = $id";
					command.Parameters.AddWithValue("$id", id);

					using var reader = command.ExecuteReader();

					while (reader.Read())
						crate.AddTrackId(reader.GetInt32(0));
				}
				catch (Exception e)
				{
					return false;
				}
			}

			return true;
		}

		//destDb must NOT be open! This just does a wholesale copy of each table from the source to the destination as
		//doing it via other methods is actually slower and more error prone.
		public bool WriteCrates(MainDb destDb)
		{
			var views = new[] { "Crate", "CrateHierarchy", "CrateParentList", "CrateTrackList" };

			try
			{
				using var attachCommand = _connection.CreateCommand();
				attachCommand.CommandText = $"ATTACH DATABASE '{destDb.GetDbPath()}' AS dest";
				attachCommand.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error copying crates.");
				Console.ForegroundColor = ConsoleColor.White;
				return false;
			}

			foreach (var view in views)
			{
				try
				{
					using var insertCommand = _connection.CreateCommand();
					insertCommand.CommandText = $"INSERT INTO dest.{view} SELECT * FROM {view}";
					insertCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Error copying DB table: {view} from source to destination.");
					Console.ForegroundColor = ConsoleColor.White;
					return false;
				}
			}

			try
			{
				using var detachCommand = _connection.CreateCommand();
				detachCommand.CommandText = "DETACH DATABASE dest";
				detachCommand.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error copying crates.");
				Console.ForegroundColor = ConsoleColor.White;
				return false;
			}

			return true;

		}
		public bool DeletePlaylists()
		{
			try
			{
				using var command = _connection.CreateCommand();
				command.CommandText = @"DELETE FROM PlaylistTrackList WHERE playlistId >= 0";
				command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				return false;
			}

			try
			{
				using var command = _connection.CreateCommand();
				command.CommandText = @"DELETE FROM Playlist WHERE id >= 0";
				command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				return false;
			}

			
			return true;
		}

		public bool ReadPlaylists(PlaylistManager playlistManager)
		{
			if (playlistManager.Count() != 0)
				return false;

			if (!ParsePlaylistTable(playlistManager))
				return false;

			return ParsePlaylistTrackListTable(playlistManager);
		}

		private bool ParsePlaylistTable(PlaylistManager playlistManager)
		{
			using var command = _connection.CreateCommand();
			command.CommandText = @"SELECT id,title FROM Playlist ORDER BY id";
			try
			{
				using var reader = command.ExecuteReader();
				while (reader.Read())
				{
					Playlist p = new Playlist(reader.GetInt32(0), reader.GetString(1));
					playlistManager.Add(p);
				}
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		private bool ParsePlaylistTrackListTable(PlaylistManager playlistManager)
		{
			var ids = playlistManager.GetIds();
			foreach (var id in ids)
			{
				var playlist = playlistManager.GetById(id);

				try
				{
					using var command = _connection.CreateCommand();
					command.CommandText = @"SELECT trackId,trackIdInOriginDatabase,databaseUuid,trackNumber FROM PlaylistTrackList WHERE playlistId = $id ORDER BY trackNumber";
					command.Parameters.AddWithValue("$id", id);
					using var reader = command.ExecuteReader();

					while (reader.Read())
						playlist.AddTrack(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3));
				}
				catch (Exception e)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Error getting track list for playlist ID: {playlist.Id}, title: {playlist.Name}");
					Console.ForegroundColor = ConsoleColor.White;
					return false;
				}
			}

			return true;
		}

		public bool WritePlaylists(PlaylistManager playlistManager)
		{
			var ids = playlistManager.GetIds();
			foreach (var id in ids)
			{
				var playlist = playlistManager.GetById(id);
				if (!WriteToPlaylistTable(playlist))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Error writing playlist ID: {playlist.Id}, title: {playlist.Name}");
					return false;
				}

				if (!WritePlaylistTracklistTable(playlist))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"Error writing playlist track list for playlist ID: {playlist.Id}, title: {playlist.Name}");
					return false;
				}
			}

			return true;
		}

		private bool WritePlaylistTracklistTable(Playlist p)
		{
			try
			{
				using var transaction = _connection.BeginTransaction();
				using var command = _connection.CreateCommand();
				command.CommandText = @"INSERT INTO PlaylistTrackList (playlistId, trackId, trackIdInOriginDatabase, databaseUuid, trackNumber)
										VALUES ($pId, $trackId, $originId, $uuid, $trackNum)";

				var paramPlaylistId = command.CreateParameter();
				paramPlaylistId.ParameterName = "$pId";
				command.Parameters.Add(paramPlaylistId);
				paramPlaylistId.Value = p.Id;

				var paramTrackId = command.CreateParameter();
				paramTrackId.ParameterName = "$trackId";
				command.Parameters.Add(paramTrackId);

				var paramOriginId = command.CreateParameter();
				paramOriginId.ParameterName = "$originId";
				command.Parameters.Add(paramOriginId);

				var paramUuid = command.CreateParameter();
				paramUuid.ParameterName = "$uuid";
				command.Parameters.Add(paramUuid);

				var paramTrackNum = command.CreateParameter();
				paramTrackNum.ParameterName = "$trackNum";
				command.Parameters.Add(paramTrackNum);

				foreach (var track in p.Tracks)
				{
					paramTrackId.Value = track.TrackId;
					paramOriginId.Value = track.TrackIdInOriginDb;
					paramUuid.Value = track.DatabaseUuid;
					paramTrackNum.Value = track.TrackOrder;

					command.ExecuteNonQuery();
				}

				transaction.Commit();

			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		private bool WriteToPlaylistTable(Playlist p)
		{
			try
			{
				using var command = _connection.CreateCommand();
				command.CommandText = @"INSERT INTO Playlist (id, title) VALUES ($id, $title)";
				command.Parameters.AddWithValue("$id", p.Id);
				command.Parameters.AddWithValue("$title", p.Name);
				command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		public bool DeleteCopiedTrackTable()
		{
			// This table records whether tracks have been exported to an external drive. It's what sets the "packed" value. It also causes
			// problems with 3rd party exporting apps, and is also not needed if you aren't using Engine Prime to handle syncing. This just
			// simply tells Engine Prime whether or not it needs to copy tracks over to the external drive, but our app handles that already.

			try
			{
				using var command = _connection.CreateCommand();
				command.CommandText = @"DELETE FROM CopiedTrack WHERE trackId >= 0";
				command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		public bool RemapTrackTablePathColumnForIds(Dictionary<int, string> trackIdToNewPathMap, string searchPrefix)
		{
			var trackIdToOldPathMap = new Dictionary<int, string>();
			foreach (var id in trackIdToNewPathMap.Keys)
				trackIdToOldPathMap[id] = searchPrefix;

			return RemapTrackTablePathColumnForIds(trackIdToNewPathMap, trackIdToOldPathMap);
		}

		public bool RemapTrackTablePathColumnForIds(Dictionary<int, string> trackIdToNewPathMap, Dictionary<int, string> trackIdToOldPathMap)
		{
			int count = 1;
			try
			{
				using var transaction = _connection.BeginTransaction();
				using var command = _connection.CreateCommand();

				command.CommandText = @"UPDATE Track SET path = $newP || SUBSTR(path, $oldPLength) WHERE id = $id";

				var paramNewP = command.CreateParameter();
				paramNewP.ParameterName = "$newP";
				command.Parameters.Add(paramNewP);

				var paramOldPLength = command.CreateParameter();
				paramOldPLength.ParameterName = "$oldPLength";
				command.Parameters.Add(paramOldPLength);

				var paramId = command.CreateParameter();
				paramId.ParameterName = "$id";
				command.Parameters.Add(paramId);

				foreach (var (key, value) in trackIdToNewPathMap)
				{
					Console.WriteLine($"Updating track {count}/{trackIdToNewPathMap.Count}");
					++count;

					paramNewP.Value = value;
					paramOldPLength.Value = trackIdToOldPathMap[key].Length + 1;
					paramId.Value = key;
					command.ExecuteNonQuery();
				}

				transaction.Commit();
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		private bool ParseTrackTable(TrackManager trackManager)
		{
			using var command = _connection.CreateCommand();
			command.CommandText = @"SELECT id,path,filename FROM Track ORDER BY id";

			try
			{
				using var reader = command.ExecuteReader();
				while (reader.Read())
				{
					Track t = new Track(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
					trackManager.Add(t);
				}
			}
			catch (SqliteException e)
			{
				return false;
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		
		private bool ParseMetadataTable(TrackManager trackManager)
		{
			using var command = _connection.CreateCommand();
			command.CommandText = @"SELECT id,type,text FROM MetaData WHERE type >= 1 AND type <= 5 ORDER BY id";
			
			try
			{
				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
					var id = reader.GetInt32(0);
					var dataType = reader.GetInt32(1);
					string text = null;

					if (!reader.IsDBNull(2))
						text = reader.GetString(2);

					Track track = trackManager.GetById(id);
					if (track == null)
						return false;	// Shouldn't happen as we'll read in all track data before calling this method

					switch (dataType)
					{
						case 1: track.Name = text; break;
						case 2: track.Artist = text; break;
						case 3: track.Album = text; break;
						case 4: track.Genre = text; break;
						case 5: track.Comment = text; break;
					}
				}
			}
			catch (SqliteException e)
			{
				return false;
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}
	}
}
