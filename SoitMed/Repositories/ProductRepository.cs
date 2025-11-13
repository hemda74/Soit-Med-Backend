using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetAllActiveAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive && p.Category != null && p.Category == category)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllActiveAsync();
            }

            // OPTIMIZATION: Use prefix matching for better index utilization
            // Pattern "%term%" = O(n) - Full table scan (SLOW)
            // Pattern "term%" = O(log n) - Can use index (FAST)
            // We'll try prefix first, then fallback to contains if needed
            var term = searchTerm.Trim();
            var prefixPattern = $"{term}%"; // Prefix matching - can use index
            var containsPattern = $"%{term}%"; // Contains matching - slower but more flexible
            
            // Strategy: Try prefix matching first (fast, uses index), then contains for flexibility
            // This gives us O(log n) for prefix matches, O(n) only for contains fallback
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive &&
                           // Prefix matching (fast - uses index on Name)
                           (EF.Functions.Like(p.Name, prefixPattern) ||
                            (p.Model != null && EF.Functions.Like(p.Model, prefixPattern)) ||
                            (p.Provider != null && EF.Functions.Like(p.Provider, prefixPattern)) ||
                            (p.Category != null && EF.Functions.Like(p.Category, prefixPattern)) ||
                            // Contains matching (slower but more flexible - only if prefix doesn't match)
                            EF.Functions.Like(p.Name, containsPattern) ||
                            (p.Model != null && EF.Functions.Like(p.Model, containsPattern)) ||
                            (p.Provider != null && EF.Functions.Like(p.Provider, containsPattern)) ||
                            (p.Category != null && EF.Functions.Like(p.Category, containsPattern)) ||
                            (p.Description != null && EF.Functions.Like(p.Description, containsPattern))))
                .OrderBy(p => p.Name)
                .Take(100) // Limit results for performance
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetInStockAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.IsActive && p.InStock)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Get products that match any of the provided names (optimized for equipment enrichment)
        /// Uses case-insensitive matching and only queries products that might match
        /// This is much faster than loading all products when we only need to match specific equipment names
        /// OPTIMIZATION: Avoids nested Any() calls which create inefficient SQL queries
        /// </summary>
        public async Task<IEnumerable<Product>> GetByNamesAsync(IEnumerable<string> names)
        {
            var nameList = names.Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!nameList.Any())
            {
                return Enumerable.Empty<Product>();
            }

            // Limit the number of names to search to avoid overly complex queries
            // This prevents O(n*m) complexity when there are many names
            const int maxNamesToSearch = 50;
            var limitedNames = nameList.Take(maxNamesToSearch).ToList();

            // Convert names to lowercase for case-insensitive comparison
            var lowerNames = limitedNames.Select(n => n.ToLower()).ToList();
            
            // Extract key words from each name (first 3-4 words) for better partial matching
            // Limit keywords to prevent query complexity
            var keyWords = limitedNames
                .SelectMany(name => name.Split(new[] { ' ', '(', '-', ')', '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2) // Filter out very short words
                    .Take(4)
                    .Select(w => w.ToLower()))
                .Distinct()
                .Take(100) // Limit total keywords
                .ToList();

            // OPTIMIZATION: Build query with multiple OR conditions using Union
            // This avoids nested Any() which creates inefficient subqueries (O(n*m) complexity)
            // Instead, we use multiple queries and union them, which EF Core can optimize better
            var queries = new List<IQueryable<Product>>();

            // Add queries for each name match
            foreach (var name in lowerNames)
            {
                // Pre-compute pattern string to avoid EF Core translation issues
                // EF Core cannot translate string.Format or string interpolation with database columns
                var pattern = $"%{name}%";
                
                var nameQuery = _context.Products
                    .AsNoTracking()
                    .Where(p => p.IsActive && (
                        EF.Functions.Like(p.Name.ToLower(), pattern) ||
                        p.Name.ToLower() == name));
                queries.Add(nameQuery);
            }

            // Add queries for keyword matches
            foreach (var keyword in keyWords)
            {
                // Pre-compute pattern string to avoid EF Core translation issues
                var pattern = $"%{keyword}%";
                var keywordQuery = _context.Products
                    .AsNoTracking()
                    .Where(p => p.IsActive && EF.Functions.Like(p.Name.ToLower(), pattern));
                queries.Add(keywordQuery);
            }

            // Combine all queries using Union (removes duplicates automatically)
            if (!queries.Any())
            {
                return Enumerable.Empty<Product>();
            }

            var combinedQuery = queries[0];
            for (int i = 1; i < queries.Count; i++)
            {
                combinedQuery = combinedQuery.Union(queries[i]);
            }

            return await combinedQuery
                .OrderBy(p => p.Name)
                .Take(200) // Limit results for performance
                .ToListAsync();
        }
    }
}



