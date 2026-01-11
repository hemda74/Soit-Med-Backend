using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Migration Service Interface - MCP-like service for Equipment-to-Client linking
    /// Provides comprehensive migration operations between TBS and ITIWebApi44 databases
    /// </summary>
    public interface IMigrationService
    {
        /// <summary>
        /// Link equipment to clients using all 4 methods (Visits, Contracts, Sales Invoices, Order Out)
        /// </summary>
        Task<EquipmentLinkingResultDto> LinkEquipmentToClientsAsync(string? adminUserId = null);

        /// <summary>
        /// Link equipment to clients via Visits (Method 1)
        /// </summary>
        Task<LinkingMethodResultDto> LinkViaVisitsAsync(string? adminUserId = null);

        /// <summary>
        /// Link equipment to clients via Maintenance Contracts (Method 2)
        /// </summary>
        Task<LinkingMethodResultDto> LinkViaMaintenanceContractsAsync(string? adminUserId = null);

        /// <summary>
        /// Link equipment to clients via Sales Invoices (Method 3)
        /// </summary>
        Task<LinkingMethodResultDto> LinkViaSalesInvoicesAsync(string? adminUserId = null);

        /// <summary>
        /// Link equipment to clients via Order Out (Method 4)
        /// </summary>
        Task<LinkingMethodResultDto> LinkViaOrderOutAsync(string? adminUserId = null);

        /// <summary>
        /// Get diagnostic statistics about equipment linking status
        /// </summary>
        Task<EquipmentLinkingDiagnosticsDto> GetDiagnosticsAsync();

        /// <summary>
        /// Get detailed report of unlinked equipment
        /// </summary>
        Task<UnlinkedEquipmentReportDto> GetUnlinkedEquipmentReportAsync(int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// Verify equipment linking for a specific client
        /// </summary>
        Task<ClientEquipmentVerificationDto> VerifyClientEquipmentAsync(long clientId);
    }
}

