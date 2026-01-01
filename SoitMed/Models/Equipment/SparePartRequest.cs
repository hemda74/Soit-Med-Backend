using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Equipment
{
    public class SparePartRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaintenanceRequestId { get; set; }

        [ForeignKey("MaintenanceRequestId")]
        public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;

        public int? MaintenanceVisitId { get; set; }

        [ForeignKey("MaintenanceVisitId")]
        public virtual MaintenanceVisit? MaintenanceVisit { get; set; }

        // Visit ID for direct link (alternative to MaintenanceVisitId)
        public int? VisitId { get; set; }

        [ForeignKey("VisitId")]
        public virtual MaintenanceVisit? Visit { get; set; }

        // Part ID reference (if part catalog exists)
        public int? PartId { get; set; }

        // Spare part details
        [Required]
        [MaxLength(200)]
        public string PartName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? PartNumber { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        // Pricing
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CompanyPrice { get; set; } // Original + Revenue

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustomerPrice { get; set; } // Final price to customer

        // Availability and workflow
        [Required]
        public SparePartAvailabilityStatus Status { get; set; } = SparePartAvailabilityStatus.Checking;

        // Warehouse approval workflow
        [MaxLength(450)]
        public string? ApprovedByWarehouseKeeperId { get; set; } // WarehouseKeeper who approved/rejected

        [ForeignKey("ApprovedByWarehouseKeeperId")]
        public virtual ApplicationUser? ApprovedByWarehouseKeeper { get; set; }

        public DateTime? WarehouseApprovedAt { get; set; }

        [MaxLength(1000)]
        public string? WarehouseRejectionReason { get; set; }

        public bool? WarehouseApproved { get; set; } // null = pending, true = approved, false = rejected

        // Assignment
        [MaxLength(450)]
        public string? AssignedToCoordinatorId { get; set; } // SparePartsCoordinator

        [ForeignKey("AssignedToCoordinatorId")]
        public virtual ApplicationUser? AssignedToCoordinator { get; set; }

        [MaxLength(450)]
        public string? AssignedToInventoryManagerId { get; set; } // InventoryManager (for local parts)

        [ForeignKey("AssignedToInventoryManagerId")]
        public virtual ApplicationUser? AssignedToInventoryManager { get; set; }

        [MaxLength(450)]
        public string? PriceSetByManagerId { get; set; } // MaintenanceManager (for global parts)

        [ForeignKey("PriceSetByManagerId")]
        public virtual ApplicationUser? PriceSetByManager { get; set; }

        // Customer decision
        public bool? CustomerApproved { get; set; }

        public DateTime? CustomerApprovedAt { get; set; }

        [MaxLength(1000)]
        public string? CustomerRejectionReason { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CheckedAt { get; set; }
        public DateTime? PriceSetAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? DeliveredToEngineerAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Payment.Payment> Payments { get; set; } = new List<Payment.Payment>();
    }
}

