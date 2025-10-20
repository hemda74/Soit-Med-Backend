using System.ComponentModel.DataAnnotations;

namespace SoitMed.Models
{
    /// <summary>
    /// Delivery terms and conditions for offers and deals
    /// </summary>
    public class DeliveryTerms : BaseEntity
    {

        [Required]
        [MaxLength(200)]
        public string DeliveryMethod { get; set; } = string.Empty; // e.g., "Express", "Standard", "Freight"

        [Required]
        [MaxLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        public int? EstimatedDeliveryDays { get; set; }

        [MaxLength(1000)]
        public string? SpecialInstructions { get; set; }

        public bool IsUrgent { get; set; } = false;

        public DateTime? PreferredDeliveryDate { get; set; }

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        [MaxLength(100)]
        public string? ContactEmail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
