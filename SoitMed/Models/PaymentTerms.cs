using System.ComponentModel.DataAnnotations;

namespace SoitMed.Models
{
    /// <summary>
    /// Payment terms and conditions for offers and deals
    /// </summary>
    public class PaymentTerms : BaseEntity
    {

        [Required]
        [MaxLength(100)]
        public string PaymentMethod { get; set; } = string.Empty; // e.g., "Cash", "Bank Transfer", "Credit Card", "Installments"

        [Required]
        public decimal TotalAmount { get; set; }

        public decimal? DownPayment { get; set; }

        public int? InstallmentCount { get; set; }

        public decimal? InstallmentAmount { get; set; }

        public int? PaymentDueDays { get; set; } // Days from delivery/invoice date

        [MaxLength(200)]
        public string? BankName { get; set; }

        [MaxLength(50)]
        public string? AccountNumber { get; set; }

        [MaxLength(50)]
        public string? IBAN { get; set; }

        [MaxLength(100)]
        public string? SwiftCode { get; set; }

        [MaxLength(1000)]
        public string? PaymentInstructions { get; set; }

        public bool RequiresAdvancePayment { get; set; } = false;

        public decimal? AdvancePaymentPercentage { get; set; }

        [MaxLength(200)]
        public string? Currency { get; set; } = "EGP";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
