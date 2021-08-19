using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public abstract class EnginePrimeDb : IDisposable
	{
		protected readonly string _dbPath;
		protected SqliteConnection _connection;
		protected bool _disposed;

		public const string EXTERNAL_MUSIC_FOLDER = @"Music";

		protected EnginePrimeDb(string dbPath)
		{
			_dbPath = dbPath;
		}

		public abstract string GetDbPath(); 

		public void OpenDb()
		{
			var connectionString = new SqliteConnectionStringBuilder()
			{
				Mode = SqliteOpenMode.ReadWrite,
				DataSource = _dbPath
			}.ToString();

			_connection = new SqliteConnection(connectionString);
			_connection.Open();
		}

		public void CloseDb() => _connection?.Close();

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					//dispose managed resources
					_connection?.Close();
				}
			}

			//dispose unmanaged resources
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

	}
}
