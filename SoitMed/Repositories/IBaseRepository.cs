using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        // Get operations
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetFilteredAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<(IEnumerable<T> Items, int TotalCount)> GetPaginatedAsync(
            Expression<Func<T, bool>>? predicate = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        // Create operations
        Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> CreateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Update operations
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Delete operations
        Task<bool> DeleteAsync(object id, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Existence checks
        Task<bool> ExistsAsync(object id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        // Count operations
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        // Queryable for complex queries
        IQueryable<T> GetQueryable();
    }
}
