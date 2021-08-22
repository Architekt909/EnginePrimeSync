using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public class DbObject
	{
		public int Id { get; protected set; }
		public string Name { get; set; }

		protected DbObject(int id)
		{
			Id = id;
		}
	}
}
