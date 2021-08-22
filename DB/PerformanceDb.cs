using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;



namespace EnginePrimeSync.DB
{
	// Use this to read track metadata AFTER you've read basic track data from the main database class
	public class PerformanceDb : EnginePrimeDb
	{
		public const string DB_NAME = "p.db";

		// Expects dbPath to contain a trailing slash
		public PerformanceDb(string dbPath) : base(dbPath + DB_NAME)
		{

		}

		public override string GetDbPath() => _dbPath;

		// Assumes trackManager has already been populated by the main db
		public bool ReadPerformanceInfo(TrackManager trackManager)
		{
			if (trackManager.Count() == 0)
				return false;

			using var command = _connection.CreateCommand();
			command.CommandText = @"SELECT id,trackData,quickCues,loops FROM PerformanceData ORDER BY id";

			try
			{
				using var reader = command.ExecuteReader();
				while (reader.Read())
				{
					int id = reader.GetInt32(0);
					Track t = trackManager.GetById(id);
					if (t == null)
						return false;   //shouldn't happen

					var bytes = GetBytes(reader, 1, true);
					t.TrackDataBlob = bytes;

					bytes = GetBytes(reader, 2, true);
					t.CueBlob = bytes;

					bytes = GetBytes(reader, 3, false); // loops aren't compressed
					t.LoopBlob = bytes;
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

		private byte[] GetBytes(SqliteDataReader reader, int index, bool isCompressed)
		{
			using Stream readStream = reader.GetStream(index);
			if (readStream is SqliteBlob blob)
			{
				var bytes = new byte[(int)blob.Length];
				_ = blob.Read(bytes, 0, (int)blob.Length);

				return isCompressed ? Util.DecompressBytes(bytes) : bytes;
			}

			return null;

		}
	}
}
