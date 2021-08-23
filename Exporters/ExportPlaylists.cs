using System;
using System.IO;
using EnginePrimeSync.DB;

namespace EnginePrimeSync.Exporters
{
	// playlists are just ordered crates
	public class ExportPlaylists : ExportCollection<Playlist>
	{
		private readonly PlaylistManager _playlistManager;

		public ExportPlaylists()
		{
			_objectNamePlural = "playlists";
			_objectManager = new PlaylistManager();
			_playlistManager = _objectManager as PlaylistManager;
		}

		protected override bool ReadSourceContent(MainDb sourceDb) => sourceDb.ReadPlaylists(_playlistManager);
		protected override bool DeleteContent(MainDb destDb) => destDb.DeletePlaylists();
		protected override bool WriteContent(MainDb sourceDb, MainDb destDb) => destDb.WritePlaylists(_playlistManager);
	}
}
