using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Equipment
{
    public class Equipment
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string QRCode { get; set; } = string.Empty; // Unique QR code identifier

        // Store QR code image as base64 string
        public string? QRCodeImageData { get; set; }

        // Store QR code PDF file path
        [MaxLength(500)]
        public string? QRCodePdfPath { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }

        // Foreign Key to Hospital (nullable - equipment can be linked to hospital OR customer)
        public string? HospitalId { get; set; }

        [ForeignKey("HospitalId")]
        public virtual Hospital.Hospital? Hospital { get; set; }

        // Foreign Key to Customer (Doctor/Technician/Manager) - nullable
        [MaxLength(450)]
        public string? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Identity.ApplicationUser? Customer { get; set; }

        // Tracking repair visits
        public int RepairVisitCount { get; set; } = 0;

        // Equipment status
        public EquipmentStatus Status { get; set; } = EquipmentStatus.Operational;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMaintenanceDate { get; set; }
        public bool IsActive { get; set; } = true;

        // QR Code Token (unique identifier for QR code generation)
        [Required]
        public Guid QrToken { get; set; } = Guid.NewGuid();

        // QR Code printing tracking
        public bool IsQrPrinted { get; set; } = false;

        public DateTime? QrLastPrintedDate { get; set; }

        // Legacy system migration support
        [MaxLength(100)]
        public string? LegacySourceId { get; set; } // Maps to old system OOI_ID

        // Navigation property for repair requests
        public virtual ICollection<RepairRequest> RepairRequests { get; set; } = new List<RepairRequest>();
    }

    public enum EquipmentStatus
    {
        Operational = 1,
        UnderMaintenance = 2,
        OutOfOrder = 3,
        Retired = 4
    }
}
