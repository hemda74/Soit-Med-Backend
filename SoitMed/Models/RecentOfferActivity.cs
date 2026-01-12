using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models
{
    /// <summary>
    /// Stores recent offer activities for dashboard display
    /// Only the last 20 activities are kept in the database
    /// </summary>
    public class RecentOfferActivity : BaseEntity
    {
        [Required]
        public string OfferId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Accepted, Completed, Sent, Rejected
        
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string? ClientName { get; set; }
        
        [MaxLength(200)]
        public string? SalesManName { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        public DateTime ActivityTimestamp { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual SalesOffer Offer { get; set; } = null!;
    }
}

