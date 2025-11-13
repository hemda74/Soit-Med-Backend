using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Enums;

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

        [Required]
        [MaxLength(100)]
        public string TransactionType { get; set; } = string.Empty; // "Payment", "Refund", "PartialRefund"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string? GatewayTransactionId { get; set; }

        [MaxLength(500)]
        public string? GatewayResponse { get; set; } // JSON response from gateway

        public PaymentTransactionStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

