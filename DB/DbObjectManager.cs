using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public class DbObjectManager<T> where T : DbObject
	{
		protected Dictionary<int, T> _idToObjectMap;

		protected DbObjectManager()
		{

		}

		public virtual void Add(T t) => _idToObjectMap[t.Id] = t;
		public virtual T GetById(int id) => _idToObjectMap[id];
		public virtual int Count() => _idToObjectMap.Count;
		public virtual List<int> GetIds() => _idToObjectMap.Keys.ToList();
		public virtual void Clear() => _idToObjectMap.Clear();
	}
}
