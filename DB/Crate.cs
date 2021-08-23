using System;
using System.Collections.Generic;


namespace EnginePrimeSync.DB
{
	public class Crate : DbObject
	{
		public string Path { get; private set; }
		public int ImmediateParentCrateId { get; set; }
		public int TopLevelParentId { get; set; }
		
		public List<int> TrackList { get; } = new();

		public Crate(int id, string name, string path) : base(id)
		{
			Name = name;
			Path = path;
			ImmediateParentCrateId = TopLevelParentId = id;
		}

		public void AddTrackId(int id) => TrackList.Add(id);
	}
}
