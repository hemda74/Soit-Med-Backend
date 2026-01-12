using SoitMed.DTO;

namespace SoitMed.Services
{
    /// <summary>
    /// Comprehensive Maintenance Service Interface
    /// Complete maintenance module with all business logic from legacy system
    /// </summary>
    public interface IComprehensiveMaintenanceService
    {
        #region Customer Management
        Task<CustomerEquipmentVisitsDTO> GetCustomerEquipmentVisitsAsync(string customerId, bool includeLegacy = true);
        Task<SoitMed.DTO.PagedResult<CustomerDTO>> SearchCustomersAsync(CustomerSearchCriteria criteria);
        Task<CustomerDTO> GetCustomerByIdAsync(string customerId);
        Task<CustomerDTO> CreateCustomerAsync(CreateCustomerDTO customer);
        Task<CustomerDTO> UpdateCustomerAsync(string customerId, UpdateCustomerDTO customer);
        Task<bool> DeleteCustomerAsync(string customerId);
        Task<CustomerVisitStatsDTO> GetCustomerVisitStatisticsAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null);
        #endregion

        #region Equipment Management
        Task<EquipmentVisitsDTO> GetEquipmentVisitsAsync(string equipmentIdentifier, bool includeLegacy = true);
        Task<EquipmentDTO> GetEquipmentByIdAsync(string equipmentId);
        Task<List<EquipmentDTO>> GetCustomerEquipmentAsync(string customerId);
        Task<List<EquipmentDTO>> GetEquipmentAsync(string customerId);
        Task<EquipmentDTO> CreateEquipmentAsync(CreateEquipmentDTO equipment);
        Task<EquipmentDTO> UpdateEquipmentAsync(string equipmentId, UpdateEquipmentDTO equipment);
        Task<EquipmentDTO> UpdateEquipmentStatusAsync(string equipmentId, string status);
        Task<bool> DeleteEquipmentAsync(string equipmentId);
        #endregion

        #region Visit Management
        Task<VisitCompletionResponseDTO> CompleteVisitAsync(CompleteVisitRequestDTO request);
        Task<MaintenanceVisitDTO> GetVisitByIdAsync(string visitId);
        Task<List<MaintenanceVisitDTO>> GetCustomerVisitsAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<MaintenanceVisitDTO>> GetEquipmentVisitsAsync(string equipmentId);
        Task<MaintenanceVisitDTO> ScheduleVisitAsync(ScheduleVisitRequestDTO request);
        Task<MaintenanceVisitDTO> UpdateVisitAsync(string visitId, UpdateVisitRequestDTO request);
        Task<MaintenanceVisitDTO> GetVisitAsync(string visitId);
        Task<MaintenanceVisitDTO> CreateVisitAsync(CreateVisitRequestDTO request);
        Task<bool> DeleteVisitAsync(string visitId);
        #endregion

        #region Contract Management
        Task<MaintenanceContractDTO> GetContractByIdAsync(string contractId);
        Task<List<MaintenanceContractDTO>> GetCustomerContractsAsync(string customerId);
        Task<MaintenanceContractDTO> CreateContractAsync(CreateMaintenanceContractDTO contract);
        Task<MaintenanceContractDTO> UpdateContractAsync(string contractId, UpdateMaintenanceContractDTO contract);
        Task<List<MaintenanceContractDTO>> GetExpiringContractsAsync(int daysThreshold = 30);
        Task<bool> RenewContractAsync(string contractId, RenewContractRequestDTO request);
        Task<MaintenanceContractDTO> GetContractAsync(string contractId);
        Task<bool> DeleteContractAsync(string contractId);
        #endregion

        #region Visit Reports & Media
        Task<VisitReportDTO> GetVisitReportAsync(string visitId);
        Task<VisitReportDTO> CreateVisitReportAsync(CreateVisitReportDTO report);
        Task<VisitReportDTO> UpdateVisitReportAsync(string reportId, UpdateVisitReportDTO report);
        Task<List<MediaFileDTO>> GetVisitMediaFilesAsync(string visitId);
        Task<MediaFileDTO> UploadVisitMediaAsync(string visitId, UploadMediaDTO media);
        Task<bool> DeleteVisitMediaAsync(string visitId, string mediaId);
        #endregion

        #region Statistics & Analytics
        Task<CustomerVisitStatsDTO> GetCustomerVisitStatsAsync(string customerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<EquipmentVisitStatsDTO> GetEquipmentVisitStatsAsync(string equipmentId);
        Task<MaintenanceDashboardDTO> GetMaintenanceDashboardAsync();
        Task<MaintenanceDashboardStatsDTO> GetMaintenanceDashboardStatsAsync();
        Task<List<MonthlyVisitStatsDTO>> GetMonthlyVisitStatsAsync(int months = 12);
        Task<RevenueReportDTO> GetRevenueReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        #endregion

        #region Engineer Management
        Task<List<EngineerDTO>> GetAvailableEngineersAsync(DateTime visitDate, string specialization = null);
        Task<EngineerWorkloadDTO> GetEngineerWorkloadAsync(string engineerId, DateTime startDate, DateTime endDate);
        Task<bool> AssignEngineerAsync(string visitId, string engineerId);
        Task<List<EngineerPerformanceDTO>> GetEngineerPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null);
        #endregion

        #region Inventory & Parts Management
        Task<List<SparePartDTO>> GetSparePartsAsync(string equipmentId = null);
        Task<SparePartDTO> CreateSparePartAsync(CreateSparePartDTO part);
        Task<SparePartDTO> UpdateSparePartAsync(string partId, UpdateSparePartDTO part);
        Task<bool> RecordPartsUsageAsync(string visitId, List<PartsUsageDTO> partsUsage);
        Task<List<PartsUsageDTO>> GetVisitPartsUsageAsync(string visitId);
        #endregion

        #region Billing & Invoicing
        Task<InvoiceDTO> GenerateVisitInvoiceAsync(string visitId);
        Task<InvoiceDTO> CreateInvoiceAsync(CreateInvoiceDTO invoice);
        Task<InvoiceDTO> UpdateInvoiceAsync(string invoiceId, UpdateInvoiceDTO invoice);
        Task<List<InvoiceDTO>> GetCustomerInvoicesAsync(string customerId);
        Task<bool> ProcessPaymentAsync(string invoiceId, ProcessPaymentDTO payment);
        #endregion

        #region Data Migration & Sync
        Task<MigrationStatusDTO> GetMigrationStatusAsync();
        Task<MigrationResultDTO> MigrateCustomersAsync(int batchSize = 100);
        Task<MigrationResultDTO> MigrateEquipmentAsync(int batchSize = 100);
        Task<MigrationResultDTO> MigrateVisitsAsync(int batchSize = 100);
        Task<MigrationResultDTO> MigrateContractsAsync(int batchSize = 100);
        Task<SyncResultDTO> SyncNewDataAsync();
        Task<DataConsistencyReportDTO> VerifyDataConsistencyAsync();
        #endregion

        #region Administrative
        Task<bool> ValidateBusinessRulesAsync(string entityType, object entity);
        Task<List<AuditLogDTO>> GetAuditLogsAsync(string entityId = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<SystemHealthDTO> GetSystemHealthAsync();
        Task<BackupStatusDTO> CreateBackupAsync();
        Task<bool> RestoreBackupAsync(string backupId);
        #endregion
    }
}
