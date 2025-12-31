using System.Threading;
using System.Threading.Tasks;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for caching service - supports both in-memory and distributed caching (Redis)
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value from cache by key
        /// </summary>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets a value in cache with optional expiration
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a value from cache by key
        /// </summary>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes multiple values from cache by pattern (e.g., "users:*")
        /// </summary>
        Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a key exists in cache
        /// </summary>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets or creates a cache entry - if not in cache, executes factory method and caches result
        /// </summary>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes the expiration time of a cached item
        /// </summary>
        Task RefreshAsync(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    }
}

