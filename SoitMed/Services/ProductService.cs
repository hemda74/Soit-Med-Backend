using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SoitMed.Services
{
    /// <summary>
    /// Service for managing product catalog
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<ProductResponseDTO>> GetAllProductsAsync(string? category = null, bool? inStock = null)
        {
            try
            {
                IEnumerable<Product> products;

                if (!string.IsNullOrWhiteSpace(category) && inStock.HasValue)
                {
                    products = await _unitOfWork.Products.GetByCategoryAsync(category);
                    products = products.Where(p => p.InStock == inStock.Value);
                }
                else if (!string.IsNullOrWhiteSpace(category))
                {
                    products = await _unitOfWork.Products.GetByCategoryAsync(category);
                }
                else if (inStock.HasValue && inStock.Value)
                {
                    products = await _unitOfWork.Products.GetInStockAsync();
                }
                else
                {
                    products = await _unitOfWork.Products.GetAllActiveAsync();
                }

                // OPTIMIZATION: Pre-load all related data in batches, then map synchronously to avoid DbContext concurrency issues
                var productsList = products.ToList();
                if (!productsList.Any())
                    return new List<ProductResponseDTO>();

                // Pre-load all user IDs that created products
                var userIds = productsList
                    .Where(p => !string.IsNullOrWhiteSpace(p.CreatedBy))
                    .Select(p => p.CreatedBy!)
                    .Distinct()
                    .ToList();

                var usersDict = userIds.Any()
                    ? (await _unitOfWork.Users.GetByIdsAsync(userIds)).ToDictionary(u => u.Id)
                    : new Dictionary<string, ApplicationUser>();

                // Map synchronously using pre-loaded data
                return productsList.Select(p => MapToProductResponseDTO(p, usersDict)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                throw;
            }
        }

        public async Task<ProductResponseDTO?> GetProductByIdAsync(long id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                    return null;

                return await MapToProductResponseDTO(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product. ProductId: {ProductId}", id);
                throw;
            }
        }

        public async Task<List<ProductResponseDTO>> GetProductsByCategoryAsync(string category)
        {
            try
            {
                var products = await _unitOfWork.Products.GetByCategoryAsync(category);
                
                // OPTIMIZATION: Pre-load all related data in batches, then map synchronously to avoid DbContext concurrency issues
                var productsList = products.ToList();
                if (!productsList.Any())
                    return new List<ProductResponseDTO>();

                // Pre-load all user IDs that created products
                var userIds = productsList
                    .Where(p => !string.IsNullOrWhiteSpace(p.CreatedBy))
                    .Select(p => p.CreatedBy!)
                    .Distinct()
                    .ToList();

                var usersDict = userIds.Any()
                    ? (await _unitOfWork.Users.GetByIdsAsync(userIds)).ToDictionary(u => u.Id)
                    : new Dictionary<string, ApplicationUser>();

                // Map synchronously using pre-loaded data
                return productsList.Select(p => MapToProductResponseDTO(p, usersDict)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category. Category: {Category}", category);
                throw;
            }
        }

        public async Task<List<ProductResponseDTO>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                var products = await _unitOfWork.Products.SearchAsync(searchTerm);
                
                // OPTIMIZATION: Pre-load all related data in batches, then map synchronously to avoid DbContext concurrency issues
                var productsList = products.ToList();
                if (!productsList.Any())
                    return new List<ProductResponseDTO>();

                // Pre-load all user IDs that created products
                var userIds = productsList
                    .Where(p => !string.IsNullOrWhiteSpace(p.CreatedBy))
                    .Select(p => p.CreatedBy!)
                    .Distinct()
                    .ToList();

                var usersDict = userIds.Any()
                    ? (await _unitOfWork.Users.GetByIdsAsync(userIds)).ToDictionary(u => u.Id)
                    : new Dictionary<string, ApplicationUser>();

                // Map synchronously using pre-loaded data
                return productsList.Select(p => MapToProductResponseDTO(p, usersDict)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products. SearchTerm: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<ProductResponseDTO> CreateProductAsync(CreateProductDTO createDto, string userId)
        {
            try
            {
                var product = new Product
                {
                    Name = createDto.Name,
                    Model = createDto.Model,
                    Provider = createDto.Provider,
                    ProviderImagePath = createDto.ProviderImagePath,
                    Country = createDto.Country,
                    Category = createDto.Category,
                    BasePrice = createDto.BasePrice,
                    Description = createDto.Description,
                    Year = createDto.Year,
                    InStock = createDto.InStock,
                    IsActive = true,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Products.CreateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product created successfully. ProductId: {ProductId}, Name: {Name}", product.Id, product.Name);

                return await MapToProductResponseDTO(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product. Name: {Name}", createDto.Name);
                throw;
            }
        }

        public async Task<ProductResponseDTO> UpdateProductAsync(long id, UpdateProductDTO updateDto, string userId)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                    throw new ArgumentException("Product not found", nameof(id));

                // Update only provided fields
                if (!string.IsNullOrWhiteSpace(updateDto.Name))
                    product.Name = updateDto.Name;

                if (updateDto.Model != null)
                    product.Model = updateDto.Model;

                if (updateDto.Provider != null)
                    product.Provider = updateDto.Provider;

                if (updateDto.ProviderImagePath != null)
                    product.ProviderImagePath = updateDto.ProviderImagePath;

                if (updateDto.Country != null)
                    product.Country = updateDto.Country;

                if (updateDto.Category != null)
                    product.Category = updateDto.Category;

                if (updateDto.BasePrice.HasValue)
                    product.BasePrice = updateDto.BasePrice.Value;

                if (updateDto.Description != null)
                    product.Description = updateDto.Description;

                if (updateDto.Year.HasValue)
                    product.Year = updateDto.Year;

                if (updateDto.InStock.HasValue)
                    product.InStock = updateDto.InStock.Value;

                if (updateDto.IsActive.HasValue)
                    product.IsActive = updateDto.IsActive.Value;

                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product updated successfully. ProductId: {ProductId}", id);

                return await MapToProductResponseDTO(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product. ProductId: {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(long id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                    return false;

                // Soft delete - set IsActive to false
                product.IsActive = false;
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product deleted (soft) successfully. ProductId: {ProductId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product. ProductId: {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductResponseDTO> UpdateProductImageAsync(long id, string imagePath)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                    throw new ArgumentException("Product not found", nameof(id));

                product.ImagePath = imagePath;
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product image updated. ProductId: {ProductId}, ImagePath: {ImagePath}", id, imagePath);

                return await MapToProductResponseDTO(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product image. ProductId: {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductResponseDTO> UpdateProviderImageAsync(long id, string imagePath)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                    throw new ArgumentException("Product not found", nameof(id));

                product.ProviderImagePath = imagePath;
                product.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Product provider image updated. ProductId: {ProductId}, ImagePath: {ImagePath}", id, imagePath);

                return await MapToProductResponseDTO(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider image. ProductId: {ProductId}", id);
                throw;
            }
        }

        #region Private Mapping Methods

        private async Task<ProductResponseDTO> MapToProductResponseDTO(Product product)
        {
            var dto = new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Model = product.Model,
                Provider = product.Provider,
                ProviderImagePath = product.ProviderImagePath,
                Country = product.Country,
                Category = product.Category,
                BasePrice = product.BasePrice,
                Description = product.Description,
                ImagePath = product.ImagePath,
                Year = product.Year,
                InStock = product.InStock,
                IsActive = product.IsActive,
                CreatedBy = product.CreatedBy,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            // Get creator name if available
            if (!string.IsNullOrEmpty(product.CreatedBy))
            {
                var creator = await _unitOfWork.Users.GetByIdAsync(product.CreatedBy);
                if (creator != null)
                {
                    dto.CreatedByName = $"{creator.FirstName} {creator.LastName}".Trim();
                }
            }

            return dto;
        }

        // Synchronous overload that uses pre-loaded data to avoid DbContext concurrency issues
        private ProductResponseDTO MapToProductResponseDTO(Product product, Dictionary<string, ApplicationUser> usersDict)
        {
            var dto = new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Model = product.Model,
                Provider = product.Provider,
                ProviderImagePath = product.ProviderImagePath,
                Country = product.Country,
                Category = product.Category,
                BasePrice = product.BasePrice,
                Description = product.Description,
                ImagePath = product.ImagePath,
                Year = product.Year,
                InStock = product.InStock,
                IsActive = product.IsActive,
                CreatedBy = product.CreatedBy,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            // Get creator name from pre-loaded dictionary
            if (!string.IsNullOrEmpty(product.CreatedBy) && usersDict.TryGetValue(product.CreatedBy, out var creator))
            {
                dto.CreatedByName = $"{creator.FirstName} {creator.LastName}".Trim();
            }

            return dto;
        }

        #endregion
    }
}



