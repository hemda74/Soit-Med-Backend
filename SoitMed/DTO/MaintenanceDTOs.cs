using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;

namespace SoitMed.DTO
{
    // Maintenance Request DTOs
    public class CreateMaintenanceRequestDTO
    {
        [Required]
        public int EquipmentId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Symptoms { get; set; }
    }

    public class MaintenanceRequestResponseDTO
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
        public string? HospitalId { get; set; }
        public string? HospitalName { get; set; }
        public int EquipmentId { get; set; }
        public string EquipmentName { get; set; } = string.Empty;
        public string EquipmentQRCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Symptoms { get; set; }
        public MaintenanceRequestStatus Status { get; set; }
        public string? AssignedToEngineerId { get; set; }
        public string? AssignedToEngineerName { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<MaintenanceRequestAttachmentResponseDTO> Attachments { get; set; } = new();
        public List<MaintenanceVisitResponseDTO> Visits { get; set; } = new();
        public List<PaymentResponseDTO> Payments { get; set; } = new();
    }

    // Maintenance Visit DTOs
    public class CreateMaintenanceVisitDTO
    {
        [Required]
        public int MaintenanceRequestId { get; set; }

        [MaxLength(200)]
        public string? QRCode { get; set; }

        [MaxLength(100)]
        public string? SerialCode { get; set; }

        [MaxLength(2000)]
        public string? Report { get; set; }

        [MaxLength(1000)]
        public string? ActionsTaken { get; set; }

        [MaxLength(500)]
        public string? PartsUsed { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ServiceFee { get; set; }

        [Required]
        public MaintenanceVisitOutcome Outcome { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class MaintenanceVisitResponseDTO
    {
        public int Id { get; set; }
        public int MaintenanceRequestId { get; set; }
        public string EngineerId { get; set; } = string.Empty;
        public string EngineerName { get; set; } = string.Empty;
        public string? QRCode { get; set; }
        public string? SerialCode { get; set; }
        public string? Report { get; set; }
        public string? ActionsTaken { get; set; }
        public string? PartsUsed { get; set; }
        public decimal? ServiceFee { get; set; }
        public MaintenanceVisitOutcome Outcome { get; set; }
        public int? SparePartRequestId { get; set; }
        public DateTime VisitDate { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
    }

    // Spare Part Request DTOs
    public class CreateSparePartRequestDTO
    {
        [Required]
        public int MaintenanceRequestId { get; set; }

        public int? MaintenanceVisitId { get; set; }

        [Required]
        [MaxLength(200)]
        public string PartName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PartNumber { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }
    }

    public class SparePartRequestResponseDTO
    {
        public int Id { get; set; }
        public int MaintenanceRequestId { get; set; }
        public int? MaintenanceVisitId { get; set; }
        public string PartName { get; set; } = string.Empty;
        public string? PartNumber { get; set; }
        public string? Description { get; set; }
        public string? Manufacturer { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? CompanyPrice { get; set; }
        public decimal? CustomerPrice { get; set; }
        public SparePartAvailabilityStatus Status { get; set; }
        public string? AssignedToCoordinatorId { get; set; }
        public string? AssignedToCoordinatorName { get; set; }
        public string? AssignedToInventoryManagerId { get; set; }
        public string? AssignedToInventoryManagerName { get; set; }
        public string? PriceSetByManagerId { get; set; }
        public string? PriceSetByManagerName { get; set; }
        public bool? CustomerApproved { get; set; }
        public DateTime? CustomerApprovedAt { get; set; }
        public string? CustomerRejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CheckedAt { get; set; }
        public DateTime? PriceSetAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? DeliveredToEngineerAt { get; set; }
    }

    public class UpdateSparePartPriceDTO
    {
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CompanyPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CustomerPrice { get; set; }
    }

    public class CustomerSparePartDecisionDTO
    {
        [Required]
        public bool Approved { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }
    }

    // Maintenance Request Attachment DTOs
    public class MaintenanceRequestAttachmentResponseDTO
    {
        public int Id { get; set; }
        public int MaintenanceRequestId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
        public AttachmentType AttachmentType { get; set; }
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    // Maintenance Request Rating DTOs
    public class CreateMaintenanceRequestRatingDTO
    {
        [Required]
        public int MaintenanceRequestId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    public class MaintenanceRequestRatingResponseDTO
    {
        public int Id { get; set; }
        public int MaintenanceRequestId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string EngineerId { get; set; } = string.Empty;
        public string EngineerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Assignment DTOs
    public class AssignMaintenanceRequestDTO
    {
        [Required]
        [MaxLength(450)]
        public string EngineerId { get; set; } = string.Empty;
    }
}

