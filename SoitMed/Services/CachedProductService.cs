using Microsoft.Extensions.Logging;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models;
using SoitMed.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoitMed.Services
{
    /// <summary>
    /// Example implementation of a cached service
    /// Demonstrates how to use ICacheService to cache frequently accessed data
    /// </summary>
    public class CachedProductService : ICachedProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<CachedProductService> _logger;

        // Cache expiration times
        private readonly TimeSpan _productCacheExpiration = TimeSpan.FromHours(12);
        private readonly TimeSpan _categoryCacheExpiration = TimeSpan.FromHours(24);

        public CachedProductService(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<CachedProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all active products with caching
        /// </summary>
        public async Task<IEnumerable<Product>> GetAllActiveProductsAsync(CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrCreateAsync(
                CacheKeys.Products.InStock,
                async () =>
                {
                    _logger.LogInformation("Cache miss: Loading active products from database");
                    var products = await _unitOfWork.Products.GetAllActiveAsync(cancellationToken);
                    return products.ToList();
                },
                _productCacheExpiration,
                cancellationToken
            );
        }

        /// <summary>
        /// Gets a product by ID with caching
        /// </summary>
        public async Task<Product?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrCreateAsync(
                CacheKeys.Products.ById(id),
                async () =>
                {
                    _logger.LogInformation("Cache miss: Loading product {ProductId} from database", id);
                    return await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
                },
                _productCacheExpiration,
                cancellationToken
            );
        }

        /// <summary>
        /// Gets products by category with caching
        /// </summary>
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId, CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrCreateAsync(
                CacheKeys.Products.ByCategory(categoryId),
                async () =>
                {
                    _logger.LogInformation("Cache miss: Loading products for category {CategoryId} from database", categoryId);
                    var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId, cancellationToken);
                    return products.ToList();
                },
                _productCacheExpiration,
                cancellationToken
            );
        }

        /// <summary>
        /// Gets all product categories with caching
        /// </summary>
        public async Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrCreateAsync(
                CacheKeys.Products.Categories,
                async () =>
                {
                    _logger.LogInformation("Cache miss: Loading product categories from database");
                    var categories = await _unitOfWork.ProductCategories.GetAllAsync(cancellationToken);
                    return categories.ToList();
                },
                _categoryCacheExpiration,
                cancellationToken
            );
        }

        /// <summary>
        /// Creates or updates a product and invalidates cache
        /// </summary>
        public async Task<Product> SaveProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            Product savedProduct;

            if (product.Id == 0)
            {
                // Create new product
                savedProduct = await _unitOfWork.Products.AddAsync(product, cancellationToken);
                _logger.LogInformation("Created new product: {ProductId}", savedProduct.Id);
            }
            else
            {
                // Update existing product
                await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
                savedProduct = product;
                _logger.LogInformation("Updated product: {ProductId}", product.Id);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Invalidate relevant caches
            await InvalidateProductCachesAsync(savedProduct, cancellationToken);

            return savedProduct;
        }

        /// <summary>
        /// Deletes a product and invalidates cache
        /// </summary>
        public async Task DeleteProductAsync(long id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted product: {ProductId}", id);

            // Invalidate relevant caches
            await InvalidateProductCachesAsync(product, cancellationToken);
        }

        /// <summary>
        /// Invalidates all caches related to a product
        /// </summary>
        private async Task InvalidateProductCachesAsync(Product product, CancellationToken cancellationToken)
        {
            // Remove specific product cache
            await _cacheService.RemoveAsync(CacheKeys.Products.ById(product.Id), cancellationToken);

            // Remove category cache if product has category
            if (product.CategoryId.HasValue)
            {
                await _cacheService.RemoveAsync(
                    CacheKeys.Products.ByCategory(product.CategoryId.Value),
                    cancellationToken
                );
            }

            // Remove all products cache
            await _cacheService.RemoveAsync(CacheKeys.Products.All, cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.Products.InStock, cancellationToken);

            _logger.LogInformation("Invalidated caches for product: {ProductId}", product.Id);
        }

        /// <summary>
        /// Clears all product-related caches (use sparingly)
        /// </summary>
        public async Task ClearAllProductCachesAsync(CancellationToken cancellationToken = default)
        {
            await _cacheService.RemoveByPatternAsync(CacheKeys.Patterns.AllProducts, cancellationToken);
            _logger.LogWarning("Cleared all product caches");
        }
    }

    /// <summary>
    /// Interface for cached product service
    /// </summary>
    public interface ICachedProductService
    {
        Task<IEnumerable<Product>> GetAllActiveProductsAsync(CancellationToken cancellationToken = default);
        Task<Product?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(long categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
        Task<Product> SaveProductAsync(Product product, CancellationToken cancellationToken = default);
        Task DeleteProductAsync(long id, CancellationToken cancellationToken = default);
        Task ClearAllProductCachesAsync(CancellationToken cancellationToken = default);
    }
}

