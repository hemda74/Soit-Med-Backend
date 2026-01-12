using System.ComponentModel.DataAnnotations;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a product category with hierarchical structure
    /// </summary>
    public class ProductCategory : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameAr { get; set; } // Arabic name

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? DescriptionAr { get; set; }

        [MaxLength(500)]
        public string? IconPath { get; set; } // Path to category icon/image

        public string? ParentCategoryId { get; set; } // For hierarchical categories

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ProductCategory? ParentCategory { get; set; }
        public virtual ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

