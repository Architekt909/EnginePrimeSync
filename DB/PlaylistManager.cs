using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public class PlaylistManager
	{
		private readonly Dictionary<int, Playlist> _idToPlaylistMap = new Dictionary<int, Playlist>();

		public PlaylistManager()
		{

		}

		public void AddPlaylist(Playlist p) => _idToPlaylistMap[p.Id] = p;
		public Playlist GetPlaylistById(int id) => _idToPlaylistMap[id];
		public int NumPlaylists() => _idToPlaylistMap.Count;
		public List<int> GetPlaylistIds() => _idToPlaylistMap.Keys.ToList();
	}
}
