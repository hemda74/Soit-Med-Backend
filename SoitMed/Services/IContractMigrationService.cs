namespace SoitMed.Services
{
    /// <summary>
    /// Service for migrating contracts from legacy TBS database to new ITIWebApi44 database
    /// </summary>
    public interface IContractMigrationService
    {
        /// <summary>
        /// Migrates all contracts from TBS to ITIWebApi44
        /// </summary>
        Task<MigrationResult> MigrateAllContractsAsync(string adminUserId);

        /// <summary>
        /// Migrates a specific contract by legacy ID
        /// </summary>
        Task<MigrationResult> MigrateContractAsync(int legacyContractId, string adminUserId);

        /// <summary>
        /// Gets migration statistics
        /// </summary>
        Task<MigrationStatistics> GetMigrationStatisticsAsync();
    }

    public class MigrationResult
    {
        public bool Success { get; set; }
        public int ContractsMigrated { get; set; }
        public int InstallmentsMigrated { get; set; }
        public int NegotiationsCreated { get; set; }
        public int Errors { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class MigrationStatistics
    {
        public int TotalLegacyContracts { get; set; }
        public int MigratedContracts { get; set; }
        public int PendingContracts { get; set; }
        public int FailedContracts { get; set; }
    }
}

