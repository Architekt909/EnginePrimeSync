using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public class PlaylistManager : DbObjectManager<Playlist>
	{
		public PlaylistManager()
		{
			_idToObjectMap = new Dictionary<int, Playlist>();
		}
	}
}
