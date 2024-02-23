using System.IO;

namespace Server.Engine.Facet
{
	public class FacetEditingSettings
	{
		public const string UNIQUE_SERVER_IDENTIFIER = "uoAvos"; //Must be 28 characters or less

		#region Manually Defined Save Directories

		public static string FileExportRootFolderPath => Path.Combine(Core.BaseDirectory, "Export", "Facet");

		public static string ModifiedClientFilesSavePath => Path.Combine(FileExportRootFolderPath, "MapFiles");

		public static string LiveRealTimeChangesSavePath => Path.Combine(FileExportRootFolderPath, "Changes");

		#endregion

		#region Server Facet Module: LumberHarvest

		public static bool LumberHarvestModuleEnabled { get; set; } = true;

		public static string LumberHarvestFallenTreeSaveLocation => Path.Combine(FileExportRootFolderPath, "Modules", "LumberHarvest");

		#endregion
	}
}