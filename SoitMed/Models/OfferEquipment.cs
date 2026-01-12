using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents an equipment item in an offer
    /// </summary>
    public class OfferEquipment : BaseEntity
    {
        [Required]
        public string OfferId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Model { get; set; }
        
        [MaxLength(100)]
        public string? Provider { get; set; } // Legacy text - kept for backward compatibility
        
        [MaxLength(500)]
        public string? ProviderImagePath { get; set; } // Provider logo/image path
        
        [MaxLength(100)]
        public string? Country { get; set; }
        
        [MaxLength(500)]
        public string? ImagePath { get; set; }
        
        public int? Year { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool InStock { get; set; } = true;
        
        // Navigation property
        public virtual SalesOffer Offer { get; set; } = null!;
    }
}

