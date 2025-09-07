using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Equipment
{
    public class Equipment
    {
        [Key]
        public int Id { get; set; }

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

        // Foreign Key to Hospital
        [Required]
        public string HospitalId { get; set; } = string.Empty;

        [ForeignKey("HospitalId")]
        public virtual Hospital.Hospital Hospital { get; set; } = null!;

        // Tracking repair visits
        public int RepairVisitCount { get; set; } = 0;

        // Equipment status
        public EquipmentStatus Status { get; set; } = EquipmentStatus.Operational;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMaintenanceDate { get; set; }
        public bool IsActive { get; set; } = true;

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
