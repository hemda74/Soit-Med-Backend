using System.ComponentModel.DataAnnotations;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents general terms and conditions for an offer
    /// </summary>
    public class OfferTerms : BaseEntity
    {
        [Required]
        public long OfferId { get; set; }
        
        [MaxLength(500)]
        public string? WarrantyPeriod { get; set; }
        
        [MaxLength(500)]
        public string? DeliveryTime { get; set; }
        
        [MaxLength(2000)]
        public string? MaintenanceTerms { get; set; }
        
        [MaxLength(2000)]
        public string? OtherTerms { get; set; }
        
        // Navigation property
        public virtual SalesOffer Offer { get; set; } = null!;
    }
}

