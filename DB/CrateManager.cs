
using System.Collections.Generic;

namespace EnginePrimeSync.DB
{
	public class CrateManager : DbObjectManager<Crate>
	{
		public CrateManager()
		{
			_idToObjectMap = new Dictionary<int, Crate>();
		}

		public void FindParentCrateIds()
		{
			foreach (var kvp in _idToObjectMap)
			{
				var crate = kvp.Value;
				crate.TopLevelParentId = RecursivelyFindParentId(crate);
			}
		}

		private int RecursivelyFindParentId(Crate c)
		{
			if (c.Id == c.ImmediateParentCrateId)
				return c.Id;

			var parentCrate = _idToObjectMap[c.ImmediateParentCrateId];
			return RecursivelyFindParentId(parentCrate);
		}
	}
}
