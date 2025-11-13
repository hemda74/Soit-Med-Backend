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

        [Required]
        public int MaintenanceRequestId { get; set; }

        [ForeignKey("MaintenanceRequestId")]
        public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;

        // Engineer/Technician who performed the visit
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
    }
}

