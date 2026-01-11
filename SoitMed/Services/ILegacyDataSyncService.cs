using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Legacy Data Sync Service Interface
    /// Syncs data from TBS to ITIWebApi44 using the same logic as MediaApi
    /// </summary>
    public interface ILegacyDataSyncService
    {
        /// <summary>
        /// Get customer machines using the same logic as MediaApi GetMachinesByCustomerIdAsync
        /// Returns customer with all related equipment and visits for each equipment
        /// </summary>
        Task<CustomerMachinesSyncDto?> GetCustomerMachinesFromTbsAsync(int legacyCustomerId);

        /// <summary>
        /// Sync customer machines from TBS to ITIWebApi44
        /// Creates/updates equipment and visits in the new system
        /// </summary>
        Task<SyncResultDto> SyncCustomerMachinesToNewSystemAsync(int legacyCustomerId);

        /// <summary>
        /// Get visits for a specific machine (OOI_ID) from TBS
        /// Same logic as MediaApi MachineService.GetMachineHistoryAsync
        /// </summary>
        Task<MachineVisitsSyncDto?> GetMachineVisitsFromTbsAsync(int ooiId);

        /// <summary>
        /// Sync visits for a machine from TBS to ITIWebApi44
        /// </summary>
        Task<SyncResultDto> SyncMachineVisitsToNewSystemAsync(int ooiId);

        /// <summary>
        /// Sync all customers with their machines and visits from TBS to ITIWebApi44
        /// </summary>
        Task<BulkSyncResultDto> SyncAllCustomersDataAsync(int? batchSize = null);

        /// <summary>
        /// Get all customers from TBS (for listing)
        /// </summary>
        Task<List<LegacyCustomerDto>> GetAllCustomersFromTbsAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// Sync all customers from TBS to ITIWebApi44 (creates clients if missing)
        /// </summary>
        Task<BulkSyncResultDto> SyncAllCustomersFromTbsAsync(int? batchSize = null);
    }
}

