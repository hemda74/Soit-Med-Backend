using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;
using SoitMed.Models.Equipment;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Payment
{
    /// <summary>
    /// Invoice entity for maintenance requests
    /// Supports future installment payments
    /// </summary>
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty; // Unique invoice number

        [Required]
        public int MaintenanceRequestId { get; set; }

        [ForeignKey("MaintenanceRequestId")]
        public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;

        // Cost breakdown
        [Column(TypeName = "decimal(18,2)")]
        public decimal LaborFees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SparePartsTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Required]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }

        // Payment information
        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [Required]
        public PaymentMethod Method { get; set; }

        // Future-proofing: Installment payment fields (reserved, not implemented yet)
        public PaymentPlan? PaymentPlan { get; set; } // Reserved for future
        public int? InstallmentMonths { get; set; } // Reserved for future
        [MaxLength(450)]
        public string? CollectionDelegateId { get; set; } // Reserved for future
        [ForeignKey("CollectionDelegateId")]
        public virtual ApplicationUser? CollectionDelegate { get; set; } // Reserved for future

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public DateTime? DueDate { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }
}


