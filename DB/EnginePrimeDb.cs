using Microsoft.Data.Sqlite;
using System;

namespace EnginePrimeSync.DB
{
	public abstract class EnginePrimeDb : IDisposable
	{
		protected readonly string _dbPath;
		protected SqliteConnection _connection;
		protected bool _disposed;
		protected bool _opened;

		public const string EXTERNAL_MUSIC_FOLDER = @"Music";
		public const string ENGINE_FOLDER = @"Engine Library";
		public const string ENGINE_FOLDER_SLASH = ENGINE_FOLDER + @"\";

		protected EnginePrimeDb(string dbPath)
		{
			_dbPath = dbPath;
		}

		public abstract string GetDbPath(); 

		public void OpenDb()
		{
			if (_opened)
				return;

			var connectionString = new SqliteConnectionStringBuilder()
			{
				Mode = SqliteOpenMode.ReadWrite,
				DataSource = _dbPath
			}.ToString();

			_connection = new SqliteConnection(connectionString);
			_connection.Open();
			_opened = true;
		}

		public void CloseDb()
		{
			_connection?.Close();
			_opened = false;
		} 

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
