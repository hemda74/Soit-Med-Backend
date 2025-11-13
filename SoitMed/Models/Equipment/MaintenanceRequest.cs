using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Equipment
{
    public class MaintenanceRequest
    {
        [Key]
        public int Id { get; set; }

        // Customer information
        [Required]
        [MaxLength(450)]
        public string CustomerId { get; set; } = string.Empty; // ApplicationUser (Doctor/Technician/Manager)

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; } = null!;

        [MaxLength(50)]
        public string CustomerType { get; set; } = string.Empty; // "Doctor", "Technician", "Manager" - auto-detected from roles

        // Hospital link (if customer is linked to hospital)
        public string? HospitalId { get; set; }

        [ForeignKey("HospitalId")]
        public virtual Hospital.Hospital? Hospital { get; set; }

        // Equipment information
        [Required]
        public int EquipmentId { get; set; }

        [ForeignKey("EquipmentId")]
        public virtual Equipment Equipment { get; set; } = null!;

        // Request details
        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Symptoms { get; set; }

        public MaintenanceRequestStatus Status { get; set; } = MaintenanceRequestStatus.Pending;

        // Assignment
        [MaxLength(450)]
        public string? AssignedToEngineerId { get; set; } // Engineer or Technician

        [ForeignKey("AssignedToEngineerId")]
        public virtual ApplicationUser? AssignedToEngineer { get; set; }

        [MaxLength(450)]
        public string? AssignedByMaintenanceSupportId { get; set; }

        [ForeignKey("AssignedByMaintenanceSupportId")]
        public virtual ApplicationUser? AssignedByMaintenanceSupport { get; set; }

        public DateTime? AssignedAt { get; set; }

        // Payment information
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.NotRequired;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RemainingAmount { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<MaintenanceRequestAttachment> Attachments { get; set; } = new List<MaintenanceRequestAttachment>();
        public virtual ICollection<MaintenanceVisit> Visits { get; set; } = new List<MaintenanceVisit>();
        public virtual ICollection<Payment.Payment> Payments { get; set; } = new List<Payment.Payment>();
        public virtual ICollection<MaintenanceRequestRating> Ratings { get; set; } = new List<MaintenanceRequestRating>();
    }
}

