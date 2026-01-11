using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Enhanced Maintenance Service Interface
    /// Provides comprehensive customer → equipment → visits workflow
    /// Integrates legacy TBS database with new itiwebapi44 database
    /// </summary>
    public interface IEnhancedMaintenanceService
    {
        #region Customer Management

        /// <summary>
        /// Get customer with their equipment and visit history
        /// Merges data from both legacy and new databases
        /// </summary>
        /// <param name="customerId">Customer ID, phone, or email</param>
        /// <param name="includeLegacy">Whether to include legacy data</param>
        /// <returns>Customer with equipment and visits</returns>
        Task<CustomerEquipmentVisitsDTO> GetCustomerEquipmentVisitsAsync(string customerId, bool includeLegacy = true);

        /// <summary>
        /// Search customers across both databases
        /// </summary>
        /// <param name="criteria">Search criteria</param>
        /// <returns>Paged customer results</returns>
        Task<SoitMed.DTO.PagedResult<CustomerDTO>> SearchCustomersAsync(CustomerSearchCriteria criteria);

        #endregion

        #region Equipment Management

        /// <summary>
        /// Get equipment details with complete visit history
        /// </summary>
        /// <param name="equipmentIdentifier">Equipment ID or serial number</param>
        /// <param name="includeLegacy">Whether to include legacy data</param>
        /// <returns>Equipment with visit history</returns>
        Task<EquipmentVisitsDTO> GetEquipmentVisitsAsync(string equipmentIdentifier, bool includeLegacy = true);

        #endregion

        #region Visit Completion Logic

        /// <summary>
        /// Complete a maintenance visit with comprehensive tracking
        /// Updates both legacy and new databases as needed
        /// </summary>
        /// <param name="dto">Visit completion data</param>
        /// <returns>Visit completion result</returns>
        Task<VisitCompletionDTO> CompleteVisitAsync(CompleteVisitDTO dto);

        /// <summary>
        /// Get visit completion statistics for a customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="startDate">Start date filter (optional)</param>
        /// <param name="endDate">End date filter (optional)</param>
        /// <returns>Customer visit statistics</returns>
        Task<CustomerVisitStatsDTO> GetCustomerVisitStatsAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null);

        #endregion
    }
}
