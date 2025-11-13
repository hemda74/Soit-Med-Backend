using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Models.Equipment;

namespace SoitMed.Models.Payment
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        // Related entities
        public int? MaintenanceRequestId { get; set; }
        [ForeignKey("MaintenanceRequestId")]
        public virtual MaintenanceRequest? MaintenanceRequest { get; set; }

        public int? SparePartRequestId { get; set; }
        [ForeignKey("SparePartRequestId")]
        public virtual SparePartRequest? SparePartRequest { get; set; }

        // Customer information
        [Required]
        [MaxLength(450)]
        public string CustomerId { get; set; } = string.Empty; // ApplicationUser
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; } = null!;

        // Payment details
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(100)]
        public string? TransactionId { get; set; } // Gateway transaction ID

        [MaxLength(500)]
        public string? PaymentReference { get; set; } // Bank reference, receipt number, etc.

        // Payment metadata (JSON for gateway-specific data)
        [Column(TypeName = "nvarchar(max)")]
        public string? PaymentMetadata { get; set; }

        // Accounting
        [MaxLength(450)]
        public string? ProcessedByAccountantId { get; set; } // Accounting role user
        [ForeignKey("ProcessedByAccountantId")]
        public virtual ApplicationUser? ProcessedByAccountant { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [MaxLength(1000)]
        public string? AccountingNotes { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }
}

