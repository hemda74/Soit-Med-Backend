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
            // OPTIMIZATION: Use AsNoTracking for read-only queries
            return await _context.ProductCategories
                .AsNoTracking()
                .Where(pc => pc.IsActive)
                .OrderBy(pc => pc.DisplayOrder)
                .ThenBy(pc => pc.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProductCategory>> GetMainCategoriesAsync(CancellationToken cancellationToken = default)
        {
            // OPTIMIZATION: Use AsNoTracking for read-only queries and explicit comparison for SQL Server BIT type
            return await _context.ProductCategories
                .AsNoTracking()
                .Where(pc => pc.IsActive == true && pc.ParentCategoryId == null)
                .OrderBy(pc => pc.DisplayOrder)
                .ThenBy(pc => pc.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProductCategory>> GetSubCategoriesAsync(string parentCategoryId, CancellationToken cancellationToken = default)
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
            // OPTIMIZATION: Remove Products Include - it loads ALL products which causes timeout
            // Product counts should be calculated separately if needed
            return await _context.ProductCategories
                .AsNoTracking()
                .Where(pc => pc.IsActive)
                .Include(pc => pc.SubCategories.Where(sc => sc.IsActive))
                .OrderBy(pc => pc.DisplayOrder)
                .ThenBy(pc => pc.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProductCategory>> GetByIdsAsync(IEnumerable<string> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any())
                return new List<ProductCategory>();

            return await _context.ProductCategories
                .AsNoTracking()
                .Where(pc => idList.Contains(pc.Id))
                .ToListAsync();
        }
    }
}

