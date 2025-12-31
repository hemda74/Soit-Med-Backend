using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SoitMed.Services
{
    /// <summary>
    /// Redis-based distributed caching service
    /// Falls back to memory cache if Redis is not available
    /// </summary>
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            
            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
                
                if (string.IsNullOrEmpty(cachedValue))
                    return default;

                return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
                };

                await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            // Note: Pattern-based removal is not natively supported by IDistributedCache
            // This would require Redis-specific implementation using StackExchange.Redis directly
            // For now, we'll log a warning and skip
            _logger.LogWarning("RemoveByPatternAsync is not fully supported with IDistributedCache. Pattern: {Pattern}", pattern);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to get from cache first
                var cachedValue = await GetAsync<T>(key, cancellationToken);
                if (cachedValue != null && !EqualityComparer<T>.Default.Equals(cachedValue, default))
                {
                    return cachedValue;
                }

                // If not in cache, execute factory
                var value = await factory();
                
                // Cache the result
                if (value != null && !EqualityComparer<T>.Default.Equals(value, default))
                {
                    await SetAsync(key, value, expiration, cancellationToken);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
                // If caching fails, still execute the factory
                return await factory();
            }
        }

        public async Task RefreshAsync(string key, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Refresh by getting and re-setting with new expiration
                var value = await _cache.GetStringAsync(key, cancellationToken);
                if (!string.IsNullOrEmpty(value))
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration ?? _defaultExpiration
                    };
                    await _cache.SetStringAsync(key, value, options, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache for key: {Key}", key);
            }
        }
    }
}

