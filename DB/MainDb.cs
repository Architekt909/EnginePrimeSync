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
			if (trackManager.NumTracks() != 0)
				return false;

			if (!ParseTrackTable(trackManager))
				return false;

			if (!ParseMetadataTable(trackManager))
				return false;

			return true;
		}

		public bool RemapTrackTablePathColumn(string searchPrefix, string replacementPrefix)
		{
			try
			{
				//NOTE: DO NOT PUT QUOTES OR SINGLE QUOTES AROUND THE PATH PARAMETERS EVEN IF THEY CONTAIN SPACES, OTHERWISE THE UPDATE METHOD WON'T WORK BUT WON'T THROW AN ERROR EITHER.
				using var command = _connection.CreateCommand();
				command.CommandText = @"UPDATE Track SET path = REPLACE(path, $oldP, $newP)";
				command.Parameters.AddWithValue("$oldP", searchPrefix);
				command.Parameters.AddWithValue("$newP", replacementPrefix);
				command.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		public bool RemapTrackTablePathColumnForSingleId(int trackId, string searchPrefix, string replacementPrefix)
		{
			try
			{
				//NOTE: DO NOT PUT QUOTES OR SINGLE QUOTES AROUND THE PATH PARAMETERS EVEN IF THEY CONTAIN SPACES, OTHERWISE THE UPDATE METHOD WON'T WORK BUT WON'T THROW AN ERROR EITHER.
				using var command = _connection.CreateCommand();
				command.CommandText = @"UPDATE Track SET path = REPLACE(path, $oldP, $newP) WHERE id = $id";
				command.Parameters.AddWithValue("$id", trackId);
				command.Parameters.AddWithValue("$oldP", searchPrefix);
				command.Parameters.AddWithValue("$newP", replacementPrefix);
				command.ExecuteNonQuery();
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
					trackManager.AddTrack(t);
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

					Track track = trackManager.GetTrack(id);
					if (track == null)
						return false;	// Shouldn't happen as we'll read in all track data before calling this method

					switch (dataType)
					{
						case 1: track.Title = text; break;
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
