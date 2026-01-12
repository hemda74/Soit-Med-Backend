using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IProductCategoryRepository : IBaseRepository<ProductCategory>
    {
        Task<List<ProductCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default);
        Task<List<ProductCategory>> GetMainCategoriesAsync(CancellationToken cancellationToken = default);
        Task<List<ProductCategory>> GetSubCategoriesAsync(string parentCategoryId, CancellationToken cancellationToken = default);
        Task<ProductCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<List<ProductCategory>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default);
        Task<List<ProductCategory>> GetByIdsAsync(IEnumerable<string> ids);
    }
}

