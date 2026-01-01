namespace SoitMed.Services
{
    /// <summary>
    /// Service for importing legacy data from old SOIT system
    /// </summary>
    public interface ILegacyImporterService
    {
        /// <summary>
        /// Import all legacy data (Clients, Equipment, MaintenanceVisits)
        /// </summary>
        Task<ImportResult> ImportAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Import only clients from legacy system
        /// </summary>
        Task<ImportResult> ImportClientsAsync(CancellationToken cancellationToken = default, System.IO.StreamWriter? logWriter = null);

        /// <summary>
        /// Import only equipment from legacy system
        /// </summary>
        Task<ImportResult> ImportEquipmentAsync(CancellationToken cancellationToken = default, System.IO.StreamWriter? logWriter = null);

        /// <summary>
        /// Import only maintenance visits from legacy system
        /// </summary>
        Task<ImportResult> ImportMaintenanceVisitsAsync(CancellationToken cancellationToken = default, System.IO.StreamWriter? logWriter = null);
    }

    /// <summary>
    /// Result of an import operation
    /// </summary>
    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string LogFilePath { get; set; } = string.Empty;
    }
}

