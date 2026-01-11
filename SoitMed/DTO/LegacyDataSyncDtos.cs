namespace SoitMed.DTO
{
    /// <summary>
    /// Customer with machines and visits (same structure as MediaApi CustomerMachinesDto)
    /// </summary>
    public class CustomerMachinesSyncDto
    {
        public int LegacyCustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
        public int MachineCount { get; set; }
        public List<MachineSyncDto> Machines { get; set; } = new();
    }

    /// <summary>
    /// Machine data from TBS (same structure as MediaApi MachineDto)
    /// </summary>
    public class MachineSyncDto
    {
        public int MachineId { get; set; } // OOI_ID
        public int OoId { get; set; }
        public int ItemId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string? DevicePlace { get; set; }
        public string? ModelName { get; set; }
        public string? ModelNameEn { get; set; }
        public string? ItemCode { get; set; }
        public decimal Quantity { get; set; }
        public int UId { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? OoiMain { get; set; }
        public string? LotNum { get; set; }
        public bool IsReturned { get; set; }
        public bool IsDeliveryForReplacement { get; set; }
        public int VisitCount { get; set; }
        public List<VisitSyncDto> Visits { get; set; } = new();
    }

    /// <summary>
    /// Visit data from TBS
    /// </summary>
    public class VisitSyncDto
    {
        public int VisitingId { get; set; }
        public int VisitingTypeId { get; set; }
        public int? EmpCode { get; set; }
        public string? EmployeeName { get; set; }
        public int? ContractId { get; set; }
        public DateTime? VisitingDate { get; set; }
        public int LegacyCustomerId { get; set; }
        public string? Notes { get; set; }
        public decimal? VisitingValue { get; set; }
        public decimal? BillValue { get; set; }
        public string? DefectDescription { get; set; }
        public string? DevicePlace { get; set; }
        public bool? IsCancelled { get; set; }
        public string? Files { get; set; }
        
        // Visit Report data
        public int? VisitingReportId { get; set; }
        public string? ReportDescription { get; set; }
        public DateTime? ReportDate { get; set; }
        public string? ReportFiles { get; set; }
        public int? RepVisitStatusId { get; set; }
    }

    /// <summary>
    /// Machine visits data (same structure as MediaApi MachineHistoryDto)
    /// </summary>
    public class MachineVisitsSyncDto
    {
        public int MachineId { get; set; } // OOI_ID
        public string SerialNumber { get; set; } = string.Empty;
        public string? Model { get; set; }
        public int? CurrentClientId { get; set; }
        public string? CurrentClientName { get; set; }
        public List<VisitHistoryItemSyncDto> VisitsHistory { get; set; } = new();
    }

    /// <summary>
    /// Visit history item
    /// </summary>
    public class VisitHistoryItemSyncDto
    {
        public int VisitId { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Engineer { get; set; } = string.Empty;
        public string? ReportSummary { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> MediaFileNames { get; set; } = new();
    }

    /// <summary>
    /// Sync result
    /// </summary>
    public class SyncResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EquipmentCreated { get; set; }
        public int EquipmentUpdated { get; set; }
        public int VisitsCreated { get; set; }
        public int VisitsUpdated { get; set; }
        public int Errors { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
        public DateTime SyncTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Bulk sync result
    /// </summary>
    public class BulkSyncResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalCustomers { get; set; }
        public int CustomersProcessed { get; set; }
        public int CustomersSucceeded { get; set; }
        public int CustomersSkipped { get; set; }
        public int CustomersFailed { get; set; }
        public int TotalEquipmentCreated { get; set; }
        public int TotalEquipmentUpdated { get; set; }
        public int TotalVisitsCreated { get; set; }
        public int TotalVisitsUpdated { get; set; }
        public int TotalErrors { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
    }

    /// <summary>
    /// Legacy customer DTO
    /// </summary>
    public class LegacyCustomerDto
    {
        public int CusId { get; set; }
        public string? CusName { get; set; }
        public string? CusTel { get; set; }
        public string? CusMobile { get; set; }
        public string? CusAddress { get; set; }
    }
}

