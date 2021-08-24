using Microsoft.Data.Sqlite;
using System;
using System.IO;

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

		//destDb must NOT be open!
		public bool CopyMetadataToOtherDb(PerformanceDb destDb, bool copyEverything, bool copyCues = false, bool copyLoops = false)
		{

			try
			{
				using var attachCommand = _connection.CreateCommand();
				attachCommand.CommandText = $"ATTACH DATABASE '{destDb.GetDbPath()}' AS dest";
				attachCommand.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Error attaching destination dabase: {destDb.GetDbPath()}.");
				Console.ForegroundColor = ConsoleColor.White;
				return false;
			}

			try
			{
				using var updateCommand = _connection.CreateCommand();
				if (copyEverything)
				{
					var columns = new[] { "isAnalyzed", "isRendered", "trackData", "highResolutionWaveFormData", "overviewWaveFormData", "beatData", "quickCues", "loops", "hasSeratoValues", "hasRekordboxValues", "hasTraktorValues" };
					updateCommand.CommandText = @"UPDATE dest.PerformanceData ";
					for (int i = 0; i < columns.Length; i++)
					{
						if (i == 0)
							updateCommand.CommandText += "SET ";

						var col = columns[i];
						updateCommand.CommandText += $"{col}=(SELECT {col} FROM PerformanceData WHERE dest.PerformanceData.id=PerformanceData.id)";

						if (i < columns.Length - 1)
							updateCommand.CommandText += ",";
					}

					updateCommand.CommandText += " WHERE EXISTS(SELECT 1 FROM dest.PerformanceData WHERE dest.PerformanceData.id=PerformanceData.id)";
				}
				else if (copyCues && copyLoops)
				{
					updateCommand.CommandText = @"UPDATE dest.PerformanceData 
												SET quickCues=(SELECT quickCues FROM PerformanceData WHERE dest.PerformanceData.id=PerformanceData.id),
													loops=(SELECT loops FROM PerformanceData WHERE dest.PerformanceData.id=PerformanceData.id)
												WHERE EXISTS(SELECT 1 FROM dest.PerformanceData WHERE dest.PerformanceData.id=PerformanceData.id)";
				}
				else if (copyCues)
				{
					updateCommand.CommandText = @"UPDATE dest.PerformanceData 
												SET quickCues=(SELECT quickCues FROM PerformanceData WHERE dest.PerformanceData.id=PerformanceData.id) 
												WHERE EXISTS(SELECT 1 FROM dest.PerformanceData WHERE dest.PerformanceData.id=PerformanceData.id)";
				}
				else
				{
					updateCommand.CommandText = @"UPDATE dest.PerformanceData 
												SET loops=(SELECT loops FROM PerformanceData WHERE dest.PerformanceData.id=PerformanceData.id) 
												WHERE EXISTS(SELECT 1 FROM dest.PerformanceData WHERE dest.PerformanceData.id=PerformanceData.id)";
				}

				updateCommand.ExecuteNonQuery();
			}
			catch (Exception e)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Error copying metadata from source to destination.");
				Console.ForegroundColor = ConsoleColor.White;
				return false;
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
				Console.WriteLine("Error detaching destination database.");
				Console.ForegroundColor = ConsoleColor.White;
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
