using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    /// <summary>
    /// DTO for product category response
    /// </summary>
    public class ProductCategoryDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameAr { get; set; }
        public string? Description { get; set; }
        public string? DescriptionAr { get; set; }
        public string? IconPath { get; set; }
        public string? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public List<ProductCategoryDTO> SubCategories { get; set; } = new List<ProductCategoryDTO>();
        public int ProductCount { get; set; } // Number of products in this category
    }

    /// <summary>
    /// DTO for creating a product category
    /// </summary>
    public class CreateProductCategoryDTO
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameAr { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? DescriptionAr { get; set; }

        public string? ParentCategoryId { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating a product category
    /// </summary>
    public class UpdateProductCategoryDTO
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(100)]
        public string? NameAr { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? DescriptionAr { get; set; }

        public string? ParentCategoryId { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for category hierarchy (tree structure)
    /// </summary>
    public class CategoryHierarchyDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? NameAr { get; set; }
        public string? IconPath { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryHierarchyDTO> SubCategories { get; set; } = new List<CategoryHierarchyDTO>();
    }
}

