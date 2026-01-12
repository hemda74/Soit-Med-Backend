using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Equipment
{
    public class MaintenanceVisit
    {
        [Key]
        public int Id { get; set; }

        // Unique ticket number (auto-generated)
        [Required]
        [MaxLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        [Required]
        public int MaintenanceRequestId { get; set; }

        [ForeignKey("MaintenanceRequestId")]
        public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;

        // Customer information
        [Required]
        [MaxLength(450)]
        public string CustomerId { get; set; } = string.Empty; // ApplicationUser (Customer)

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; } = null!;

        // Device/Equipment information
        [Required]
        [MaxLength(50)]
        public string DeviceId { get; set; } = string.Empty; // FK to Equipment

        [ForeignKey("DeviceId")]
        public virtual Equipment Device { get; set; } = null!;

        // Scheduled date for the visit
        [Required]
        public DateTime ScheduledDate { get; set; }

        // Origin of the visit request
        [Required]
        public VisitOrigin Origin { get; set; }

        // Visit status (replaces MaintenanceVisitOutcome)
        [Required]
        public VisitStatus Status { get; set; } = VisitStatus.PendingApproval;

        // Self-reference for rescheduled visits
        public int? ParentVisitId { get; set; }

        [ForeignKey("ParentVisitId")]
        public virtual MaintenanceVisit? ParentVisit { get; set; }

        public virtual ICollection<MaintenanceVisit> ChildVisits { get; set; } = new List<MaintenanceVisit>();

        // Payment information
        public bool IsPaidVisit { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        // Engineer/Technician who performed the visit (primary engineer)
        [Required]
        [MaxLength(450)]
        public string EngineerId { get; set; } = string.Empty; // ApplicationUser (Engineer role)

        [ForeignKey("EngineerId")]
        public virtual ApplicationUser Engineer { get; set; } = null!;

        // Equipment identification
        [MaxLength(200)]
        public string? QRCode { get; set; } // Scanned QR code

        [MaxLength(100)]
        public string? SerialCode { get; set; } // Manual serial code entry

        // Visit details
        [MaxLength(2000)]
        public string? Report { get; set; }

        [MaxLength(1000)]
        public string? ActionsTaken { get; set; }

        [MaxLength(500)]
        public string? PartsUsed { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ServiceFee { get; set; }

        // Legacy field - kept for backward compatibility, but Status should be used instead
        [Required]
        public MaintenanceVisitOutcome Outcome { get; set; }

        // If needs spare part
        public int? SparePartRequestId { get; set; }

        [ForeignKey("SparePartRequestId")]
        public virtual SparePartRequest? SparePartRequest { get; set; }

        // Timestamps
        public DateTime VisitDate { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // Legacy system migration support
        public int? LegacyVisitId { get; set; } // Maps to old system visit ID

        // Navigation properties
        public virtual ICollection<VisitAssignees> Assignees { get; set; } = new List<VisitAssignees>();
        public virtual VisitReport? VisitReport { get; set; }
    }
}

