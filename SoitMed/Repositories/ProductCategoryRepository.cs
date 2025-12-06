using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ProductCategoryRepository : BaseRepository<ProductCategory>, IProductCategoryRepository
    {
        public ProductCategoryRepository(Context context) : base(context)
        {
        }

        public async Task<List<ProductCategory>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .Where(pc => pc.IsActive)
                .OrderBy(pc => pc.DisplayOrder)
                .ThenBy(pc => pc.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProductCategory>> GetMainCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .Where(pc => pc.IsActive && pc.ParentCategoryId == null)
                .OrderBy(pc => pc.DisplayOrder)
                .ThenBy(pc => pc.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProductCategory>> GetSubCategoriesAsync(long parentCategoryId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .Where(pc => pc.IsActive && pc.ParentCategoryId == parentCategoryId)
                .OrderBy(pc => pc.DisplayOrder)
                .ThenBy(pc => pc.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<ProductCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .FirstOrDefaultAsync(pc => pc.Name == name, cancellationToken);
        }

        public async Task<List<ProductCategory>> GetCategoryHierarchyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductCategories
                .Where(pc => pc.IsActive)
                .Include(pc => pc.SubCategories.Where(sc => sc.IsActive))
                .Include(pc => pc.Products)
                .OrderBy(pc => pc.DisplayOrder)
                .ThenBy(pc => pc.Name)
                .ToListAsync(cancellationToken);
        }
    }
}

