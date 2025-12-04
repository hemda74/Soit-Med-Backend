using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace SoitMed.Services
{
    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<VerificationCodeService> _logger;
        private const int CODE_LENGTH = 6;
        private const int CODE_EXPIRY_MINUTES = 15;

        public VerificationCodeService(IMemoryCache cache, ILogger<VerificationCodeService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<string> GenerateVerificationCodeAsync(string email)
        {
            try
            {
                // Generate a random 6-digit code
                var code = GenerateRandomCode();
                
                // Store the code in cache with expiry
                var cacheKey = $"verification_code_{email}";
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CODE_EXPIRY_MINUTES),
                    SlidingExpiration = TimeSpan.FromMinutes(5), // Reset expiry if accessed
                    Size = 1 // Required when SizeLimit is set on MemoryCache
                };

                _cache.Set(cacheKey, code, cacheOptions);
                
                _logger.LogInformation($"Generated verification code for email: {email}");
                return await Task.FromResult(code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to generate verification code for email: {email}");
                throw;
            }
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            try
            {
                var cacheKey = $"verification_code_{email}";
                var storedCode = _cache.Get<string>(cacheKey);

                if (string.IsNullOrEmpty(storedCode))
                {
                    _logger.LogWarning($"No verification code found for email: {email}");
                    return false;
                }

                var isValid = string.Equals(storedCode, code, StringComparison.OrdinalIgnoreCase);
                
                if (isValid)
                {
                    // Remove the code after successful verification
                    _cache.Remove(cacheKey);
                    _logger.LogInformation($"Verification code verified successfully for email: {email}");
                }
                else
                {
                    _logger.LogWarning($"Invalid verification code provided for email: {email}");
                }

                return await Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to verify code for email: {email}");
                return false;
            }
        }

        public async Task<bool> IsCodeValidAsync(string email, string code)
        {
            try
            {
                var cacheKey = $"verification_code_{email}";
                var storedCode = _cache.Get<string>(cacheKey);

                if (string.IsNullOrEmpty(storedCode))
                {
                    return false;
                }

                var isValid = string.Equals(storedCode, code, StringComparison.OrdinalIgnoreCase);
                return await Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to check code validity for email: {email}");
                return false;
            }
        }

        public async Task RemoveCodeAsync(string email)
        {
            try
            {
                var cacheKey = $"verification_code_{email}";
                _cache.Remove(cacheKey);
                _logger.LogInformation($"Removed verification code for email: {email}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to remove code for email: {email}");
            }
        }

        private string GenerateRandomCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = Math.Abs(BitConverter.ToInt32(bytes, 0));
            return (randomNumber % 1000000).ToString("D6");
        }
    }
}
