using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    /// <summary>
    /// DTO for creating a new product in the catalog
    /// </summary>
    public class CreateProductDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(100)]
        public string? Provider { get; set; }

        [MaxLength(500)]
        public string? ProviderImagePath { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
        public decimal BasePrice { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public int? Year { get; set; }

        public bool InStock { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating an existing product
    /// </summary>
    public class UpdateProductDTO
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(100)]
        public string? Provider { get; set; }

        [MaxLength(500)]
        public string? ProviderImagePath { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
        public decimal? BasePrice { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public int? Year { get; set; }

        public bool? InStock { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for product response
    /// </summary>
    public class ProductResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Provider { get; set; }
        public string? ProviderImagePath { get; set; }
        public string? Country { get; set; }
        public string? Category { get; set; }
        public decimal BasePrice { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public int? Year { get; set; }
        public bool InStock { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for product summary (used in lists)
    /// </summary>
    public class ProductSummaryDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Provider { get; set; }
        public string? ProviderImagePath { get; set; }
        public string? Category { get; set; }
        public decimal BasePrice { get; set; }
        public bool InStock { get; set; }
        public string? ImagePath { get; set; }
    }
}



