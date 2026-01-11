using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    // Alias classes for backward compatibility
    public class CreateCustomerRequest : CreateCustomerDTO { }
    public class UpdateCustomerRequest : UpdateCustomerDTO { }
    public class CreateEquipmentRequest : CreateEquipmentDTO { }
    public class UpdateEquipmentRequest : UpdateEquipmentDTO { }
    public class CreateVisitRequest : ScheduleVisitRequestDTO { }
    public class UpdateVisitRequest : UpdateVisitRequestDTO { }
    public class CompleteVisitRequest : CompleteVisitRequestDTO { }
    public class CustomerVisitStats : CustomerVisitStatsDTO { }
    public class CustomerSearchCriteria : CustomerSearchParametersDTO { }
    public class VisitDTO : MaintenanceVisitDTO { }
    public class PagedRequest : PagedRequestDTO { }
    public class VisitSearchCriteria : VisitSearchParametersDTO { }
    public class VisitCompletionResponse : VisitCompletionResponseDTO { }
    
    // Pagination DTOs
    public class PagedRequestDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SearchTerm { get; set; }
    }
    
    public class PagedResultDTO<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
    
    public class VisitSearchParametersDTO : PagedRequestDTO
    {
        public string CustomerId { get; set; }
        public string EngineerId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
    }
    
    public class CustomerSearchParametersDTO : PagedRequestDTO
    {
        public string IsActive { get; set; }
        public string Source { get; set; }
    }
    
    // Additional DTOs needed
    public class CustomerEquipmentVisitsDTO 
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<MaintenanceVisitDTO> Visits { get; set; }
        public int TotalVisits { get; set; }
    }
    
    public class EquipmentVisitsDTO 
    {
        public string EquipmentId { get; set; }
        public string EquipmentModel { get; set; }
        public List<MaintenanceVisitDTO> Visits { get; set; }
        public int TotalVisits { get; set; }
    }
    
    public class CreateContractRequest : CreateMaintenanceContractDTO { }
    public class UpdateContractRequest : UpdateMaintenanceContractDTO { }
    public class MaintenanceDashboardStats : MaintenanceDashboardStatsDTO { }
    
    // Payment DTOs
    public class PaymentFilters 
    {
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CustomerId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
    
    public class ConfirmPaymentDTO 
    {
        [Required]
        public string PaymentId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; }
        
        public string TransactionId { get; set; }
        public string Notes { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
    
    // Additional DTOs for missing interface methods
    public class UpdateVisitReportDTO 
    {
        public string Report { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public List<string> AttachmentIds { get; set; }
    }
    
    public class CreateSparePartDTO 
    {
        [Required]
        [StringLength(50)]
        public string PartNumber { get; set; }
        
        [Required]
        public string PartName { get; set; }
        
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public string Location { get; set; }
        public bool? IsActive { get; set; }
    }
    
    public class UpdateSparePartDTO 
    {
        public string PartName { get; set; }
        public string Description { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? StockQuantity { get; set; }
        public string Location { get; set; }
        public bool? IsActive { get; set; }
    }
    
    public class CreateInvoiceDTO 
    {
        [Required]
        public string CustomerId { get; set; }
        
        [Required]
        public List<InvoiceItemDTO> Items { get; set; }
        
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string Notes { get; set; }
    }
    
    public class UpdateInvoiceDTO 
    {
        public decimal? TaxAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }
    }
    
    public class InvoiceItemDTO 
    {
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    public class ProcessPaymentDTO 
    {
        [Required]
        public string InvoiceId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; }
        
        public string TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Notes { get; set; }
    }
    
    // Additional DTOs for interface methods
    public class MaintenanceDashboardDTO 
    {
        public int TotalCustomers { get; set; }
        public int TotalEquipment { get; set; }
        public int ActiveVisits { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<CustomerVisitStatsDTO> RecentCustomers { get; set; }
        public List<EquipmentVisitStatsDTO> TopEquipment { get; set; }
    }
    
    public class MonthlyVisitStatsDTO 
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public int TotalVisits { get; set; }
        public int CompletedVisits { get; set; }
        public decimal Revenue { get; set; }
    }
    
    public class RevenueReportDTO 
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AverageRevenuePerVisit { get; set; }
        public List<MonthlyRevenueDTO> MonthlyBreakdown { get; set; }
    }
    
    public class MonthlyRevenueDTO 
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
        public int VisitCount { get; set; }
    }
    
    public class MigrationStatusDTO 
    {
        public bool IsInProgress { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int FailedRecords { get; set; }
        public DateTime? LastMigrationDate { get; set; }
    }
    
    public class MigrationResultDTO 
    {
        public bool Success { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; }
        public TimeSpan Duration { get; set; }
    }
    
    public class SyncResultDTO 
    {
        public bool Success { get; set; }
        public int RecordsSynced { get; set; }
        public DateTime SyncDate { get; set; }
        public List<string> SyncErrors { get; set; }
    }
    
    public class ConsistencyReportDTO 
    {
        public bool IsConsistent { get; set; }
        public List<ConsistencyIssueDTO> Issues { get; set; }
        public DateTime CheckDate { get; set; }
    }
    
    public class ConsistencyIssueDTO 
    {
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Issue { get; set; }
        public string Severity { get; set; }
    }
    
    public class AuditLogDTO 
    {
        public string Id { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Action { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }
    
    public class InvoiceDTO 
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<InvoiceItemDTO> Items { get; set; }
    }
    
    public class PaymentDTO 
    {
        public string Id { get; set; }
        public string InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #region Customer DTOs
    public class CustomerDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Source { get; set; } // "New" or "Legacy"
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public int EquipmentCount { get; set; }
        public int VisitCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CreateCustomerDTO
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(500)]
        public string Address { get; set; }
    }

    public class UpdateCustomerDTO
    {
        [StringLength(200)]
        public string Name { get; set; }

        [Phone]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public bool? IsActive { get; set; }
    }
    #endregion

    #region Equipment DTOs
    public class EquipmentDTO
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string WarrantyStatus { get; set; }
        public string QRCode { get; set; }
        public string HospitalId { get; set; }
        public string Name { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int VisitCount { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        
        // Additional properties needed by services
        public List<string> RecentIssues { get; set; }
        public double UptimePercentage { get; set; }
        public string MaintenancePriority { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public List<string> AttachmentPaths { get; set; }
    }

    public class CreateEquipmentDTO
    {
        [Required]
        public string CustomerId { get; set; }

        [Required]
        public string ModelId { get; set; }

        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? InstallationDate { get; set; }
    }

    public class UpdateEquipmentDTO
    {
        public string ModelId { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public DateTime? InstallationDate { get; set; }
    }
    #endregion

    #region Visit DTOs
    public class MaintenanceVisitDTO
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string EquipmentId { get; set; }
        public string EquipmentModel { get; set; }
        public string EngineerId { get; set; }
        public string EngineerName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string VisitType { get; set; } // Preventive, Corrective, Installation, Inspection
        public string Status { get; set; } // Scheduled, InProgress, Completed, Cancelled
        public string IssueDescription { get; set; }
        public string Report { get; set; }
        public string ActionsTaken { get; set; }
        public string Outcome { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? PartsCost { get; set; }
        public decimal? TotalCost { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Source { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PartsUsageDTO> PartsUsed { get; set; }
        public List<MediaFileDTO> MediaFiles { get; set; }
    }

    public class ScheduleVisitRequestDTO
    {
        [Required]
        public string CustomerId { get; set; }

        [Required]
        public string EquipmentId { get; set; }

        [Required]
        public string EngineerId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [StringLength(20)]
        public string VisitType { get; set; }

        [StringLength(1000)]
        public string IssueDescription { get; set; }

        public decimal? EstimatedCost { get; set; }
    }

    public class UpdateVisitRequestDTO
    {
        public DateTime? ScheduledDate { get; set; }
        public string EngineerId { get; set; }
        public string Status { get; set; }
        public string IssueDescription { get; set; }
        public string Report { get; set; }
        public string ActionsTaken { get; set; }
        public string Outcome { get; set; }
        public decimal? ServiceFee { get; set; }
    }

    public class CompleteVisitRequestDTO
    {
        [Required]
        public string VisitId { get; set; }

        [Required]
        public string Source { get; set; }

        [StringLength(2000)]
        public string Report { get; set; }

        [StringLength(2000)]
        public string ActionsTaken { get; set; }

        [StringLength(1000)]
        public string PartsUsed { get; set; }

        public decimal? ServiceFee { get; set; }

        [StringLength(50)]
        public string Outcome { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        public List<PartsUsageDTO> PartsUsage { get; set; }
        public List<UploadMediaDTO> MediaFiles { get; set; }
    }

    public class VisitCompletionResponseDTO
    {
        public bool Success { get; set; }
        public string VisitId { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string InvoiceId { get; set; }
    }
    #endregion

    #region Contract DTOs
    public class MaintenanceContractDTO
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ContractNumber { get; set; }
        public string ContractType { get; set; } // Preventive, Comprehensive, Corrective
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ContractValue { get; set; }
        public int PlannedVisits { get; set; }
        public int CompletedVisits { get; set; }
        public string Status { get; set; } // Active, Expired, Terminated, Pending
        public bool IsAutoRenewal { get; set; }
        public string PaymentTerms { get; set; }
        public string ServiceLevel { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<EquipmentDTO> CoveredEquipment { get; set; }
        public List<MaintenanceVisitDTO> Visits { get; set; }
        public bool IsExpiringSoon { get; set; }
        public int DaysUntilExpiry { get; set; }
    }

    public class CreateMaintenanceContractDTO
    {
        [Required]
        public string CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string ContractNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string ContractType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ContractValue { get; set; }

        [Range(1, int.MaxValue)]
        public int PlannedVisits { get; set; }

        public bool IsAutoRenewal { get; set; }
        public string PaymentTerms { get; set; }
        public string ServiceLevel { get; set; }
        public List<string> EquipmentIds { get; set; }
    }

    public class UpdateMaintenanceContractDTO
    {
        public DateTime? EndDate { get; set; }
        public decimal? ContractValue { get; set; }
        public int? PlannedVisits { get; set; }
        public string Status { get; set; }
        public bool IsAutoRenewal { get; set; }
        public string PaymentTerms { get; set; }
        public string ServiceLevel { get; set; }
        public string Notes { get; set; }
    }

    public class RenewContractRequestDTO
    {
        [Required]
        public DateTime NewEndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? NewContractValue { get; set; }

        public int? AdditionalVisits { get; set; }
        public string UpdatedServiceLevel { get; set; }
        public string RenewalNotes { get; set; }
    }
    #endregion

    #region Visit Report DTOs
    public class VisitReportDTO
    {
        public string Id { get; set; }
        public string VisitId { get; set; }
        public string Report { get; set; }
        public DateTime? ReportDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string Outcome { get; set; }
        public decimal? TravelHours { get; set; }
        public int? Nights { get; set; }
        public decimal? Meals { get; set; }
        public bool RequiresParts { get; set; }
        public bool IsUnderWarranty { get; set; }
        public bool DeviceWorking { get; set; }
        public string TechnicalAssessment { get; set; }
        public List<MediaFileDTO> Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateVisitReportDTO
    {
        [Required]
        public string VisitId { get; set; }

        [StringLength(5000)]
        public string Report { get; set; }

        public DateTime? EffectiveDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Outcome { get; set; }
        public decimal? TravelHours { get; set; }
        public int? Nights { get; set; }
        public decimal? Meals { get; set; }
        public bool RequiresParts { get; set; }
        public bool IsUnderWarranty { get; set; }
        public bool DeviceWorking { get; set; }
        public string TechnicalAssessment { get; set; }
        public List<UploadMediaDTO> Attachments { get; set; }
    }
    #endregion

    #region Media File DTOs
    public class MediaFileDTO
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // Report, Image, Document, Video
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    public class UploadMediaDTO
    {
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public string ContentType { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
    #endregion

    #region Parts Management DTOs
    public class SparePartDTO
    {
        public string Id { get; set; }
        public string PartNumber { get; set; }
        public string PartName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Manufacturer { get; set; }
        public decimal UnitCost { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockLevel { get; set; }
        public int MaxStockLevel { get; set; }
        public string Location { get; set; }
        public bool? IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<EquipmentDTO> CompatibleEquipment { get; set; }
    }

    public class PartsUsageDTO
    {
        public string PartId { get; set; }
        public string PartName { get; set; }
        public string PartNumber { get; set; }
        public int QuantityUsed { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
    }
    #endregion

    #region Engineer DTOs
    public class EngineerDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Specialization { get; set; }
        public string Department { get; set; }
        public string JobTitle { get; set; }
        public bool IsActive { get; set; }
        public DateTime? HireDate { get; set; }
        public List<string> Certifications { get; set; }
        public List<string> EquipmentExpertise { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Additional properties needed by controllers
        public string UserId { get; set; }
        public List<string> GovernorateIds { get; set; }
    }

    public class EngineerWorkloadDTO
    {
        public string EngineerId { get; set; }
        public string EngineerName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalScheduledVisits { get; set; }
        public int CompletedVisits { get; set; }
        public int PendingVisits { get; set; }
        public int CancelledVisits { get; set; }
        public double CompletionRate { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<DailyWorkloadDTO> DailyWorkload { get; set; }
    }

    public class DailyWorkloadDTO
    {
        public DateTime Date { get; set; }
        public int ScheduledVisits { get; set; }
        public int CompletedVisits { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal Revenue { get; set; }
    }

    public class EngineerPerformanceDTO
    {
        public string EngineerId { get; set; }
        public string EngineerName { get; set; }
        public int TotalVisits { get; set; }
        public int CompletedVisits { get; set; }
        public double CompletionRate { get; set; }
        public decimal AverageVisitDuration { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerVisit { get; set; }
        public double CustomerRating { get; set; }
        public int OnTimeVisits { get; set; }
        public double OnTimeRate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }
    #endregion

    #region Statistics DTOs
    public class CustomerVisitStatsDTO
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalVisits { get; set; }
        public int CompletedVisits { get; set; }
        public int PendingVisits { get; set; }
        public int CancelledVisits { get; set; }
        public decimal TotalRevenue { get; set; }
        public double CompletionRate { get; set; }
        public decimal AverageVisitCost { get; set; }
        public List<MonthlyVisitCountDTO> MonthlyBreakdown { get; set; }
    }

    public class EquipmentVisitStatsDTO
    {
        public string EquipmentId { get; set; }
        public string EquipmentModel { get; set; }
        public string SerialNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public int TotalVisits { get; set; }
        public int CompletedVisits { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public decimal AverageVisitCost { get; set; }
        public int DowntimeHours { get; set; }
        public double UptimePercentage { get; set; }
        public List<VisitTypeStatsDTO> VisitTypeBreakdown { get; set; }
    }

    // Supporting statistics classes
    public class MonthlyVisitCountDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int VisitCount { get; set; }
    }

    public class VisitTypeStatsDTO
    {
        public string VisitType { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class TopCustomerDTO
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal Revenue { get; set; }
        public int VisitCount { get; set; }
    }

    public class EquipmentIssueDTO
    {
        public string EquipmentId { get; set; }
        public string EquipmentModel { get; set; }
        public string CustomerName { get; set; }
        public string IssueDescription { get; set; }
        public DateTime ReportedDate { get; set; }
        public string Status { get; set; }
        public int Priority { get; set; }
    }

    public class ExpiringContractDTO
    {
        public string ContractId { get; set; }
        public string CustomerName { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public decimal ContractValue { get; set; }
    }

    public class RevenueByCustomerDTO
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class RevenueByServiceTypeDTO
    {
        public string ServiceType { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; }
        public int VisitCount { get; set; }
    }
    #endregion

    #region Billing DTOs
    public class InvoiceLineItemDTO
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string ItemType { get; set; } // Service, Part, Labor
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal LineTotal { get; set; }
        public string RelatedId { get; set; } // VisitId, PartId, etc.
    }

    public class CreateInvoiceLineItemDTO
    {
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(50)]
        public string ItemType { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Discount { get; set; }

        public string RelatedId { get; set; }
    }

    public class UpdateInvoiceLineItemDTO
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
    }

    // Additional Payment DTOs
    public class PaymentResponseDTO
    {
        public bool Success { get; set; }
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public DateTime ProcessedAt { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Id { get; set; }
        public string MaintenanceRequestId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string PaymentMethodName { get; set; }
        public string StatusName { get; set; }
        public string PaymentReference { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }

    public class StripePaymentDTO
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string Token { get; set; }

        public string Description { get; set; }
        public string CustomerEmail { get; set; }
    }

    public class PayPalPaymentDTO
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string ReturnUrl { get; set; }

        [Required]
        public string CancelUrl { get; set; }

        public string Description { get; set; }
    }

    public class LocalGatewayPaymentDTO
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string CardNumber { get; set; }

        [Required]
        public string ExpiryDate { get; set; }

        [Required]
        public string CVV { get; set; }

        public string CardholderName { get; set; }
    }

    public class CashPaymentDTO
    {
        [Required]
        public decimal Amount { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string ReceivedBy { get; set; }

        public string Notes { get; set; }

        public string ReceiptNumber { get; set; }
    }

    public class BankTransferDTO
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string BankName { get; set; }

        [Required]
        public string AccountNumber { get; set; }

        [Required]
        public string TransactionReference { get; set; }

        public DateTime? TransferDate { get; set; }

        public string Notes { get; set; }
    }

    public class RefundDTO
    {
        [Required]
        public string PaymentId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string Reason { get; set; }

        public string RefundMethod { get; set; }

        public DateTime? ProcessedDate { get; set; }

        public string ProcessedBy { get; set; }
    }

    public class CreatePaymentDTO
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        public string InvoiceId { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string Notes { get; set; }

        public string TransactionId { get; set; }
    }

    public class EquipmentResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public EquipmentDTO Equipment { get; set; }
        public List<string> Errors { get; set; }
        
        // Additional properties needed by services
        public string Id { get; set; }
        public string Name { get; set; }
        public string QRCode { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public string Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public string HospitalId { get; set; }
        public string HospitalName { get; set; }
        public int RepairVisitCount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public bool IsActive { get; set; }
        public string QrToken { get; set; }
        public bool IsQrPrinted { get; set; }
        public string QRCodeImageData { get; set; }
        public string QRCodePdfPath { get; set; }
    }

    public class DataConsistencyReportDTO
    {
        public DateTime ReportDate { get; set; }
        public bool IsConsistent { get; set; }
        public List<ConsistencyIssueDTO> Issues { get; set; }
        public int TotalChecks { get; set; }
        public int PassedChecks { get; set; }
        public int FailedChecks { get; set; }
        public List<string> Warnings { get; set; }
    }
    
    // Additional DTOs needed by other services
    public class CompleteVisitDTO 
    {
        [Required]
        public string VisitId { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        public string Status { get; set; }
        
        public string Report { get; set; }
        
        public List<string> AttachmentIds { get; set; }
        
        public decimal? ServiceFee { get; set; }
        
        public List<PartsUsageDTO> PartsUsed { get; set; }
        
        public string Notes { get; set; }
        
        public string CompletedBy { get; set; }
        
        public string Outcome { get; set; }
        
        public string ActionsTaken { get; set; }
        
        public string PartsUsedString { get; set; }
        
        public string Source { get; set; }
    }
    
    public class VisitCompletionDTO 
    {
        public bool Success { get; set; }
        
        public string VisitId { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        public string Status { get; set; }
        
        public string Message { get; set; }
        
        public string InvoiceId { get; set; }
        
        public List<string> Errors { get; set; }
    }
    
    public class EnhancedEquipmentDTO : EquipmentDTO 
    {
        public int VisitCount { get; set; }
        
        public DateTime? LastVisitDate { get; set; }
        
        public decimal TotalMaintenanceCost { get; set; }
        
        public string WarrantyStatus { get; set; }
        
        public DateTime? NextMaintenanceDate { get; set; }
        
        public List<string> RecentIssues { get; set; }
        
        public double UptimePercentage { get; set; }
        
        public string MaintenancePriority { get; set; }
    }
    
    public class RepairRequestDTO 
    {
        public string Id { get; set; }
        
        public string CustomerId { get; set; }
        
        public string CustomerName { get; set; }
        
        public string EquipmentId { get; set; }
        
        public string EquipmentModel { get; set; }
        
        public string SerialNumber { get; set; }
        
        public string IssueDescription { get; set; }
        
        public DateTime RequestDate { get; set; }
        
        public string Status { get; set; } // Pending, Assigned, InProgress, Completed, Cancelled
        
        public string Priority { get; set; } // Low, Medium, High, Critical
        
        public string AssignedEngineerId { get; set; }
        
        public string AssignedEngineerName { get; set; }
        
        public DateTime? AssignedDate { get; set; }
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        public DateTime? ActualCompletionDate { get; set; }
        
        public decimal? EstimatedCost { get; set; }
        
        public decimal? ActualCost { get; set; }
        
        public string Notes { get; set; }
        
        public List<string> AttachmentIds { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public string CreatedBy { get; set; }
    }
    
    public class RepairRequestResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public RepairRequestDTO RepairRequest { get; set; }
        public List<string> Errors { get; set; }
    }
    
    public class UpdateRepairRequestDTO 
    {
        public string Status { get; set; }
        
        public string Priority { get; set; }
        
        public string AssignedEngineerId { get; set; }
        
        public DateTime? EstimatedCompletionDate { get; set; }
        
        public decimal? EstimatedCost { get; set; }
        
        public string Notes { get; set; }
        
        public List<string> AttachmentIds { get; set; }
    }
    
    public class GovernorateDTO 
    {
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public string NameEn { get; set; }
        
        public string CountryId { get; set; }
        
        public string CountryName { get; set; }
        
        public bool IsActive { get; set; }
        
        public int SortOrder { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public List<CityDTO> Cities { get; set; }
    }
    
    public class CityDTO 
    {
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public string NameEn { get; set; }
        
        public string GovernorateId { get; set; }
        
        public string GovernorateName { get; set; }
        
        public bool IsActive { get; set; }
        
        public int SortOrder { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
    }
    
    // Additional DTOs needed
    public class MaintenanceDashboardStatsDTO
    {
        public int TotalCustomers { get; set; }
        public int TotalEquipment { get; set; }
        public int ActiveContracts { get; set; }
        public int ExpiringContractsNext30Days { get; set; }
        public int ScheduledVisitsToday { get; set; }
        public int VisitsInProgress { get; set; }
        public int PendingVisits { get; set; }
        public int OverdueVisits { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public List<TopCustomerDTO> TopCustomers { get; set; }
        public List<EngineerPerformanceDTO> TopEngineers { get; set; }
        public List<EquipmentIssueDTO> RecentIssues { get; set; }
        public List<ExpiringContractDTO> ExpiringContracts { get; set; }
    }
    
    public class RejectPaymentDTO
    {
        [Required]
        public string PaymentId { get; set; }
        
        [Required]
        public string Reason { get; set; }
        
        public string Notes { get; set; }
        
        public string RejectedBy { get; set; }
        
        public DateTime? RejectionDate { get; set; }
    }
    
    // Accounting DTOs
    public class FinancialReportDTO
    {
        public DateTime ReportDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public List<PaymentMethodStatisticsDTO> PaymentMethodStats { get; set; }
        public List<MonthlyFinancialDTO> MonthlyBreakdown { get; set; }
    }
    
    public class PaymentMethodStatisticsDTO
    {
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public double Percentage { get; set; }
    }
    
    public class MonthlyFinancialDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit { get; set; }
    }
    
    // System Health DTOs
    public class SystemHealthDTO
    {
        public bool IsHealthy { get; set; }
        public DateTime CheckTime { get; set; }
        public DatabaseHealthDTO DatabaseHealth { get; set; }
        public FileSystemHealthDTO FileSystemHealth { get; set; }
        public MemoryHealthDTO MemoryHealth { get; set; }
        public List<ServiceHealthDTO> ServiceHealth { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Errors { get; set; }
    }
    
    public class DatabaseHealthDTO
    {
        public bool IsConnected { get; set; }
        public string ConnectionString { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int ActiveConnections { get; set; }
        public long DatabaseSize { get; set; }
        public DateTime LastBackup { get; set; }
        public bool IsBackupCurrent { get; set; }
    }
    
    public class FileSystemHealthDTO
    {
        public bool IsAccessible { get; set; }
        public string UploadPath { get; set; }
        public long AvailableSpace { get; set; }
        public long TotalSpace { get; set; }
        public double UsagePercentage { get; set; }
        public bool HasWritePermissions { get; set; }
    }
    
    public class MemoryHealthDTO
    {
        public long TotalMemory { get; set; }
        public long AvailableMemory { get; set; }
        public long UsedMemory { get; set; }
        public double UsagePercentage { get; set; }
        public bool IsMemoryPressure { get; set; }
    }
    
    public class ServiceHealthDTO
    {
        public string ServiceName { get; set; }
        public bool IsRunning { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastCheck { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    public class BackupStatusDTO
    {
        public string BackupId { get; set; }
        public DateTime BackupTime { get; set; }
        public bool Success { get; set; }
        public string BackupPath { get; set; }
        public long BackupSize { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> BackedUpTables { get; set; }
        public List<string> Errors { get; set; }
    }
    #endregion
}
