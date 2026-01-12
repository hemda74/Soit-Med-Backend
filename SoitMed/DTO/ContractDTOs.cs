using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Enums;

namespace SoitMed.DTO
{
    /// <summary>
    /// Contract type filter for maintenance contracts
    /// </summary>
    public enum ContractTypeFilter
    {
        All = 0,
        MainContracts = 1,      // عقود الصيانة الرئيسية (MainContract = null)
        ActiveContracts = 2,    // عقود الصيانة السارية (Signed and not expired)
        ExpiredContracts = 3,   // عقود الصيانة المنتهية (Expired or past end date)
        CancelledContracts = 4  // عقود الصيانة الملغاة (Cancelled)
    }

    /// <summary>
    /// DTO for filtering and searching contracts
    /// </summary>
    public class ContractFilterDTO
    {
        public string? SearchTerm { get; set; } // Search in ContractNumber, Title, Client Name
        public ContractStatus? Status { get; set; } // Filter by contract status
        public ContractTypeFilter? ContractType { get; set; } // Filter by contract type (Main, Active, Expired, Cancelled)
        public long? ClientId { get; set; } // Filter by client
        public long? DealId { get; set; } // Filter by deal
        public DateTime? StartDateFrom { get; set; } // Filter by start date range
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; } // Filter by end date range
        public DateTime? EndDateTo { get; set; }
        public bool? HasInstallments { get; set; } // Filter contracts with installments
        public bool? IsLegacy { get; set; } // Filter migrated contracts (LegacyContractId != null)
        public string? SortBy { get; set; } = "CreatedAt"; // Sort field: ContractNumber, Title, CreatedAt, SignedAt, Status
        public string? SortOrder { get; set; } = "desc"; // Sort order: asc, desc
        public int Page { get; set; } = 1; // Pagination
        public int PageSize { get; set; } = 20; // Pagination
    }

    /// <summary>
    /// DTO for contract response
    /// </summary>
    public class ContractResponseDTO
    {
        public long Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? ContractContent { get; set; }
        public string? DocumentUrl { get; set; }
        public ContractStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public DateTime? DraftedAt { get; set; }
        public DateTime? SentToCustomerAt { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        
        // Client information
        public string ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPhone { get; set; }
        public string? ClientEmail { get; set; }
        
        // Deal information
        public long? DealId { get; set; }
        public string? DealTitle { get; set; }
        
        // Financial information
        public decimal? CashAmount { get; set; }
        public decimal? InstallmentAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? LatePenaltyRate { get; set; }
        public int? InstallmentDurationMonths { get; set; }
        public bool HasInstallments { get; set; }
        public int InstallmentCount { get; set; }
        public int PaidInstallmentCount { get; set; }
        public int OverdueInstallmentCount { get; set; }
        
        // User information
        public string DraftedBy { get; set; } = string.Empty;
        public string? DrafterName { get; set; }
        public string? CustomerSignedBy { get; set; }
        public string? CustomerSignerName { get; set; }
        
        // Legacy migration
        public int? LegacyContractId { get; set; }
        public bool IsLegacy { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Additional computed fields
        public bool IsExpired { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public decimal? TotalAmount { get; set; } // CashAmount or InstallmentAmount
        
        // Contract type information (for legacy contracts)
        public bool IsMainContract { get; set; } // Main contract (MainContract = null in TBS)
        public int? MainContractId { get; set; } // Reference to main contract (if sub-contract)
        public string ContractTypeDisplay { get; set; } = string.Empty; // "Main", "Active", "Expired", "Cancelled"
    }

    /// <summary>
    /// DTO for paginated contract response
    /// </summary>
    public class PaginatedContractResponseDTO
    {
        public List<ContractResponseDTO> Contracts { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// DTO for contract equipment (from legacy system)
    /// </summary>
    public class ContractEquipmentDTO
    {
        public int MachineId { get; set; }
        public int ItemId { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public string? ModelName { get; set; }
        public string? ModelNameEn { get; set; }
        public string? DevicePlace { get; set; }
        public string? ItemCode { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int VisitCount { get; set; }
    }

    /// <summary>
    /// DTO for contract media file (from legacy system)
    /// </summary>
    public class ContractMediaFileDTO
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public string? MimeType { get; set; }
        public long? FileSize { get; set; }
        public string? FileSizeFormatted { get; set; }
        public bool IsImage { get; set; }
        public bool IsPdf { get; set; }
        public bool CanPreview { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? AvailabilityMessage { get; set; }
    }

    /// <summary>
    /// DTO for detailed contract response with equipment and media
    /// </summary>
    public class DetailedContractResponseDTO : ContractResponseDTO
    {
        public List<ContractEquipmentDTO> Equipment { get; set; } = new();
        public int EquipmentCount => Equipment.Count;
        public List<ContractMediaFileDTO> MediaFiles { get; set; } = new();
        public int MediaFileCount => MediaFiles.Count;
    }
}

