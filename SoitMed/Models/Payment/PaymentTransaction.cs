using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Models.Equipment;

namespace SoitMed.Models.Payment
{
    public class PaymentTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PaymentId { get; set; }
        [ForeignKey("PaymentId")]
        public virtual Payment Payment { get; set; } = null!;

        // Link to maintenance visit (nullable - payments can be for other purposes)
        public int? VisitId { get; set; }

        [ForeignKey("VisitId")]
        public virtual MaintenanceVisit? Visit { get; set; }

        [Required]
        [MaxLength(100)]
        public string TransactionType { get; set; } = string.Empty; // "Payment", "Refund", "PartialRefund"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Payment method (using enum)
        [Required]
        public PaymentMethod Method { get; set; }

        [MaxLength(100)]
        public string? GatewayTransactionId { get; set; }

        [MaxLength(500)]
        public string? GatewayResponse { get; set; } // JSON response from gateway

        // Payment status (using enum)
        [Required]
        public PaymentStatus Status { get; set; }

        // Collection delegate (for Delegate payment method)
        [MaxLength(450)]
        public string? CollectionDelegateId { get; set; }

        [ForeignKey("CollectionDelegateId")]
        public virtual ApplicationUser? CollectionDelegate { get; set; }

        // Accounts approver (for confirming payments)
        [MaxLength(450)]
        public string? AccountsApproverId { get; set; }

        [ForeignKey("AccountsApproverId")]
        public virtual ApplicationUser? AccountsApprover { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

