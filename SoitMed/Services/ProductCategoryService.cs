using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductCategoryService> _logger;

        public ProductCategoryService(IUnitOfWork unitOfWork, ILogger<ProductCategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<ProductCategoryDTO>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _unitOfWork.ProductCategories.GetAllActiveAsync();
                return categories.Select(MapToCategoryDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                throw;
            }
        }

        public async Task<List<CategoryHierarchyDTO>> GetCategoryHierarchyAsync()
        {
            try
            {
                var allCategories = await _unitOfWork.ProductCategories.GetCategoryHierarchyAsync();
                
                // Get only main categories (no parent)
                var mainCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();
                
                return mainCategories.Select(c => MapToHierarchyDTO(c, allCategories)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category hierarchy");
                throw;
            }
        }

        public async Task<List<ProductCategoryDTO>> GetMainCategoriesAsync()
        {
            try
            {
                var categories = await _unitOfWork.ProductCategories.GetMainCategoriesAsync();
                return categories.Select(MapToCategoryDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting main categories");
                throw;
            }
        }

        public async Task<List<ProductCategoryDTO>> GetSubCategoriesAsync(long parentCategoryId)
        {
            try
            {
                var categories = await _unitOfWork.ProductCategories.GetSubCategoriesAsync(parentCategoryId);
                return categories.Select(MapToCategoryDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subcategories for parent {ParentId}", parentCategoryId);
                throw;
            }
        }

        public async Task<ProductCategoryDTO?> GetCategoryByIdAsync(long id)
        {
            try
            {
                var category = await _unitOfWork.ProductCategories.GetByIdAsync(id);
                if (category == null)
                    return null;

                return MapToCategoryDTO(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by id {CategoryId}", id);
                throw;
            }
        }

        public async Task<ProductCategoryDTO> CreateCategoryAsync(CreateProductCategoryDTO createDto)
        {
            try
            {
                // Validate parent category if specified
                if (createDto.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _unitOfWork.ProductCategories.GetByIdAsync(createDto.ParentCategoryId.Value);
                    if (parentCategory == null)
                        throw new ArgumentException("Parent category not found", nameof(createDto.ParentCategoryId));
                }

                var category = new ProductCategory
                {
                    Name = createDto.Name,
                    NameAr = createDto.NameAr,
                    Description = createDto.Description,
                    DescriptionAr = createDto.DescriptionAr,
                    ParentCategoryId = createDto.ParentCategoryId,
                    DisplayOrder = createDto.DisplayOrder,
                    IsActive = createDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ProductCategories.CreateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category created: {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

                return MapToCategoryDTO(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                throw;
            }
        }

        public async Task<ProductCategoryDTO> UpdateCategoryAsync(long id, UpdateProductCategoryDTO updateDto)
        {
            try
            {
                var category = await _unitOfWork.ProductCategories.GetByIdAsync(id);
                if (category == null)
                    throw new ArgumentException("Category not found", nameof(id));

                // Validate parent category if being changed
                if (updateDto.ParentCategoryId.HasValue)
                {
                    // Prevent circular reference
                    if (updateDto.ParentCategoryId.Value == id)
                        throw new ArgumentException("Category cannot be its own parent", nameof(updateDto.ParentCategoryId));

                    var parentCategory = await _unitOfWork.ProductCategories.GetByIdAsync(updateDto.ParentCategoryId.Value);
                    if (parentCategory == null)
                        throw new ArgumentException("Parent category not found", nameof(updateDto.ParentCategoryId));
                }

                // Update fields
                if (updateDto.Name != null)
                    category.Name = updateDto.Name;
                if (updateDto.NameAr != null)
                    category.NameAr = updateDto.NameAr;
                if (updateDto.Description != null)
                    category.Description = updateDto.Description;
                if (updateDto.DescriptionAr != null)
                    category.DescriptionAr = updateDto.DescriptionAr;
                if (updateDto.ParentCategoryId.HasValue)
                    category.ParentCategoryId = updateDto.ParentCategoryId.Value;
                if (updateDto.DisplayOrder.HasValue)
                    category.DisplayOrder = updateDto.DisplayOrder.Value;
                if (updateDto.IsActive.HasValue)
                    category.IsActive = updateDto.IsActive.Value;

                category.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProductCategories.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category updated: {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

                return MapToCategoryDTO(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(long id)
        {
            try
            {
                var category = await _unitOfWork.ProductCategories.GetByIdAsync(id);
                if (category == null)
                    return false;

                // Check if category has subcategories
                var subCategories = await _unitOfWork.ProductCategories.GetSubCategoriesAsync(id);
                if (subCategories.Any())
                    throw new InvalidOperationException("Cannot delete category with subcategories. Delete subcategories first.");

                // Check if category has products
                var context = _unitOfWork.GetContext();
                var hasProducts = await context.Products.AnyAsync(p => p.CategoryId == id);
                if (hasProducts)
                    throw new InvalidOperationException("Cannot delete category with products. Reassign products first.");

                await _unitOfWork.ProductCategories.DeleteAsync(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Category deleted: {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                throw;
            }
        }

        // Helper methods
        private ProductCategoryDTO MapToCategoryDTO(ProductCategory category)
        {
            var context = _unitOfWork.GetContext();
            var productCount = context.Products.Count(p => p.CategoryId == category.Id);

            return new ProductCategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                NameAr = category.NameAr,
                Description = category.Description,
                DescriptionAr = category.DescriptionAr,
                IconPath = category.IconPath,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.Name,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                ProductCount = productCount
            };
        }

        private CategoryHierarchyDTO MapToHierarchyDTO(ProductCategory category, List<ProductCategory> allCategories)
        {
            var context = _unitOfWork.GetContext();
            var productCount = context.Products.Count(p => p.CategoryId == category.Id);

            var subCategories = allCategories
                .Where(c => c.ParentCategoryId == category.Id)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => MapToHierarchyDTO(c, allCategories))
                .ToList();

            return new CategoryHierarchyDTO
            {
                Id = category.Id,
                Name = category.Name,
                NameAr = category.NameAr,
                IconPath = category.IconPath,
                ProductCount = productCount,
                SubCategories = subCategories
            };
        }
    }
}

