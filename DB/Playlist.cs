using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public class Playlist : DbObject
	{
		public class TrackListItem
		{
			public int TrackId { get; }
			public int TrackIdInOriginDb { get; }
			public string DatabaseUuid { get; }
			public int TrackOrder { get; }

			public TrackListItem(int trackId, int trackIdInOriginDb, string databaseUuid, int trackOrder)
			{
				TrackId = trackId;
				TrackIdInOriginDb = trackIdInOriginDb;
				DatabaseUuid = databaseUuid;
				TrackOrder = trackOrder;
			}
		}
		
		// Sorted by track order
		public List<TrackListItem> Tracks { get; } = new();

		public Playlist(int id, string title) : base(id)
		{
			Name = title;
		}

		public void AddTrack(int trackId, int trackIdInOriginDb, string databaseUuid, int trackOrder)
		{
			var item = new TrackListItem(trackId, trackIdInOriginDb, databaseUuid, trackOrder);
			Tracks.Add(item);
			Tracks.Sort((t1, t2) => t1.TrackOrder < t2.TrackOrder ? -1 : 1);
		}

	}
}
