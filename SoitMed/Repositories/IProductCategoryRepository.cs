using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IProductCategoryRepository : IBaseRepository<ProductCategory>
    {
        Task<List<ProductCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default);
        Task<List<ProductCategory>> GetMainCategoriesAsync(CancellationToken cancellationToken = default);
        Task<List<ProductCategory>> GetSubCategoriesAsync(long parentCategoryId, CancellationToken cancellationToken = default);
        Task<ProductCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<List<ProductCategory>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default);
    }
}

