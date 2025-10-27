using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents an installment payment plan for an offer
    /// </summary>
    public class InstallmentPlan : BaseEntity
    {
        [Required]
        public long OfferId { get; set; }
        
        [Required]
        public int InstallmentNumber { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        // Navigation property
        public virtual SalesOffer Offer { get; set; } = null!;
    }
}


