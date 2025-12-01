using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a product in the catalog that can be used in offers
    /// </summary>
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(100)]
        public string? Provider { get; set; } // Legacy text for backward compatibility

        [MaxLength(500)]
        public string? ProviderImagePath { get; set; } // Path to provider logo/image

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; } // e.g., "X-Ray", "Ultrasound", "CT Scanner"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BasePrice { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImagePath { get; set; }

        public int? Year { get; set; }

        public bool InStock { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(450)]
        public string? CreatedBy { get; set; } // User ID who added this product
    }
}



