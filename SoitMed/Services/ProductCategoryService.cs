using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;
using SoitMed.Common;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductCategoryService> _logger;
        private readonly ICacheService _cacheService;

        public ProductCategoryService(IUnitOfWork unitOfWork, ILogger<ProductCategoryService> logger, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<List<ProductCategoryDTO>> GetAllCategoriesAsync()
        {
            try
            {
                return await _cacheService.GetOrCreateAsync(
                    CacheKeys.Products.Categories,
                    async () =>
                    {
                        var categories = await _unitOfWork.ProductCategories.GetAllActiveAsync();
                        
                        // OPTIMIZATION: Pre-load product counts to avoid N+1 queries
                        var context = _unitOfWork.GetContext();
                        var categoryIds = categories.Select(c => c.Id).ToList();
                        
                        var productCounts = categoryIds.Any()
                            ? await context.Products
                                .AsNoTracking()
                                .Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value))
                                .GroupBy(p => p.CategoryId.Value)
                                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                                .ToDictionaryAsync(x => x.CategoryId, x => x.Count)
                            : new Dictionary<long, int>();
                        
                        return categories.Select(c => MapToCategoryDTO(c, productCounts.GetValueOrDefault(c.Id, 0))).ToList();
                    },
                    TimeSpan.FromHours(24)
                );
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
                return await _cacheService.GetOrCreateAsync(
                    "ProductCategories:Hierarchy",
                    async () =>
                    {
                        var allCategories = await _unitOfWork.ProductCategories.GetCategoryHierarchyAsync();
                        
                        // Get only main categories (no parent)
                        var mainCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();
                        
                        // OPTIMIZATION: Pre-load all product counts to avoid N+1 queries
                        var context = _unitOfWork.GetContext();
                        var allCategoryIds = allCategories.Select(c => c.Id).ToList();
                        
                        var productCounts = allCategoryIds.Any()
                            ? await context.Products
                                .AsNoTracking()
                                .Where(p => p.CategoryId.HasValue && allCategoryIds.Contains(p.CategoryId.Value))
                                .GroupBy(p => p.CategoryId.Value)
                                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                                .ToDictionaryAsync(x => x.CategoryId, x => x.Count)
                            : new Dictionary<long, int>();
                        
                        return mainCategories.Select(c => MapToHierarchyDTO(c, allCategories, productCounts)).ToList();
                    },
                    TimeSpan.FromHours(24)
                );
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
                return await _cacheService.GetOrCreateAsync(
                    "ProductCategories:Main",
                    async () =>
                    {
                        // OPTIMIZATION: Query directly from repository (avoids loading all categories)
                        var categories = await _unitOfWork.ProductCategories.GetMainCategoriesAsync();
                        _logger.LogInformation("✅ [ProductCategoryService] GetMainCategoriesAsync returned: {Count} categories", categories.Count);
                        
                        if (categories.Count == 0)
                        {
                            _logger.LogWarning("⚠️ [ProductCategoryService] No main categories found!");
                            return new List<ProductCategoryDTO>();
                        }
                        
                        // OPTIMIZATION: Pre-load product counts in a single query to avoid N+1
                        var context = _unitOfWork.GetContext();
                        var categoryIds = categories.Select(c => c.Id).ToList();
                        
                        // Get product counts for all categories in one query
                        var productCounts = await context.Products
                            .AsNoTracking()
                            .Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value))
                            .GroupBy(p => p.CategoryId.Value)
                            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);
                        
                        // Map to DTOs using pre-loaded counts
                        return categories.Select(c => MapToCategoryDTO(c, productCounts.GetValueOrDefault(c.Id, 0))).ToList();
                    },
                    TimeSpan.FromHours(24)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [ProductCategoryService] Error getting main categories");
                throw;
            }
        }

        public async Task<List<ProductCategoryDTO>> GetSubCategoriesAsync(long parentCategoryId)
        {
            try
            {
                var categories = await _unitOfWork.ProductCategories.GetSubCategoriesAsync(parentCategoryId);
                
                // OPTIMIZATION: Pre-load product counts to avoid N+1 queries
                var context = _unitOfWork.GetContext();
                var categoryIds = categories.Select(c => c.Id).ToList();
                
                var productCounts = categoryIds.Any()
                    ? await context.Products
                        .AsNoTracking()
                        .Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value))
                        .GroupBy(p => p.CategoryId.Value)
                        .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                        .ToDictionaryAsync(x => x.CategoryId, x => x.Count)
                    : new Dictionary<long, int>();
                
                return categories.Select(c => MapToCategoryDTO(c, productCounts.GetValueOrDefault(c.Id, 0))).ToList();
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

                // Invalidate cache
                await _cacheService.RemoveAsync(CacheKeys.Products.Categories);
                await _cacheService.RemoveAsync("ProductCategories:Hierarchy");
                await _cacheService.RemoveAsync("ProductCategories:Main");

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

                // Invalidate cache
                await _cacheService.RemoveAsync(CacheKeys.Products.Categories);
                await _cacheService.RemoveAsync("ProductCategories:Hierarchy");
                await _cacheService.RemoveAsync("ProductCategories:Main");

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

                // Invalidate cache
                await _cacheService.RemoveAsync(CacheKeys.Products.Categories);
                await _cacheService.RemoveAsync("ProductCategories:Hierarchy");
                await _cacheService.RemoveAsync("ProductCategories:Main");

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
        private ProductCategoryDTO MapToCategoryDTO(ProductCategory category, int productCount = 0)
        {
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
        
        // Overload for backward compatibility (but should avoid using this)
        private ProductCategoryDTO MapToCategoryDTO(ProductCategory category)
        {
            // OPTIMIZATION: Only count if not provided (for backward compatibility)
            // But this should be avoided in hot paths
            var context = _unitOfWork.GetContext();
            var productCount = context.Products.Count(p => p.CategoryId == category.Id);
            return MapToCategoryDTO(category, productCount);
        }

        private CategoryHierarchyDTO MapToHierarchyDTO(ProductCategory category, List<ProductCategory> allCategories, Dictionary<long, int> productCounts)
        {
            var productCount = productCounts.GetValueOrDefault(category.Id, 0);

            var subCategories = allCategories
                .Where(c => c.ParentCategoryId == category.Id)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => MapToHierarchyDTO(c, allCategories, productCounts))
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

