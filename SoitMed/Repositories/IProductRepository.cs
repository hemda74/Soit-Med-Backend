using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllActiveAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        Task<IEnumerable<Product>> GetByCategoryIdAsync(string? categoryId);
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
        Task<IEnumerable<Product>> GetInStockAsync();
        /// <summary>
        /// Get products that match any of the provided names (optimized for equipment enrichment)
        /// This is much faster than loading all products when we only need to match specific equipment names
        /// </summary>
        Task<IEnumerable<Product>> GetByNamesAsync(IEnumerable<string> names);
    }
}



