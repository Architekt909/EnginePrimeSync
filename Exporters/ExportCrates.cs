using EnginePrimeSync.DB;
using System;
using System.IO;

namespace EnginePrimeSync.Exporters
{
	public class ExportCrates : ExportCollection<Crate>
	{
		private readonly CrateManager _crateManager;

		public ExportCrates()
		{
			_objectManager = new CrateManager();
			_crateManager = _objectManager as CrateManager;
			_objectNamePlural = "crates";
		}

		protected override bool DeleteContent(MainDb destinationDb) => destinationDb.DeleteCrates();
		
		protected override bool WriteContent(MainDb destinationDb)
		{
			return false;
		}

		protected override bool ReadSourceContent(MainDb sourceDb) => sourceDb.ReadCrates(_crateManager);
		
	}
}
