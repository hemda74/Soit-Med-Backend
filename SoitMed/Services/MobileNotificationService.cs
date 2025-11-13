using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Repositories;
using System.Text;
using System.Text.Json;

namespace SoitMed.Services
{
    public class MobileNotificationService : IMobileNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MobileNotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private const string ExpoPushApiUrl = "https://exp.host/--/api/v2/push/send";

        public MobileNotificationService(
            IConfiguration configuration, 
            HttpClient httpClient, 
            ILogger<MobileNotificationService> logger,
            IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task SendPushNotificationAsync(string userId, string title, string message, Dictionary<string, object>? data = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Get all active device tokens for this user
                var deviceTokens = await _unitOfWork.GetContext().DeviceTokens
                    .Where(dt => dt.UserId == userId && dt.IsActive)
                    .ToListAsync(cancellationToken);

                if (!deviceTokens.Any())
                {
                    _logger.LogWarning("⚠️ No device tokens found for user {UserId}. Push notification not sent.", userId);
                    return;
                }

                _logger.LogInformation("📱 Sending push notification to {Count} device(s) for user {UserId}: {Title}", 
                    deviceTokens.Count, userId, title);

                // Prepare notification payload for Expo Push API
                var notifications = deviceTokens.Select(dt => new
                {
                    to = dt.Token,
                    sound = "default",
                    title = title,
                    body = message,
                    data = data ?? new Dictionary<string, object>(),
                    priority = "high",
                    channelId = "default"
                }).ToList();

                // Send to Expo Push API
                var jsonContent = JsonSerializer.Serialize(notifications, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Create request message with proper headers
                var request = new HttpRequestMessage(HttpMethod.Post, ExpoPushApiUrl)
                {
                    Content = httpContent
                };
                
                // Add required headers for Expo API (request headers, not content headers)
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
                request.Headers.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("deflate"));

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogInformation("✅ Push notification sent successfully. Response: {Response}", responseContent);
                    
                    // Update LastUsedAt for all tokens
                    foreach (var token in deviceTokens)
                    {
                        token.LastUsedAt = DateTime.UtcNow;
                    }
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("❌ Failed to send push notification. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending push notification to user {UserId}", userId);
                // Don't throw - allow SignalR to still work
            }
        }

        public async Task SendBulkPushNotificationAsync(List<string> userIds, string title, string message, Dictionary<string, object>? data = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("📱 Sending bulk push notification to {Count} users: {Title}", userIds.Count, title);
                
                // Send to each user
                var tasks = userIds.Select(userId => SendPushNotificationAsync(userId, title, message, data, cancellationToken));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending bulk push notification");
                // Don't throw - allow SignalR to still work
            }
        }

        public async Task RegisterDeviceTokenAsync(string userId, string deviceToken, string platform, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("📱 Registering device token for user {UserId} on platform {Platform}", userId, platform);
                
                // Check if token already exists for this user
                var existingToken = await _unitOfWork.GetContext().DeviceTokens
                    .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.Token == deviceToken, cancellationToken);

                if (existingToken != null)
                {
                    // Update existing token
                    existingToken.Platform = platform;
                    existingToken.LastUsedAt = DateTime.UtcNow;
                    existingToken.IsActive = true;
                    _unitOfWork.GetContext().DeviceTokens.Update(existingToken);
                    _logger.LogInformation("✅ Updated existing device token for user {UserId}", userId);
                }
                else
                {
                    // Create new token
                    var newToken = new DeviceToken
                    {
                        UserId = userId,
                        Token = deviceToken,
                        Platform = platform,
                        LastUsedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _unitOfWork.GetContext().DeviceTokens.AddAsync(newToken, cancellationToken);
                    _logger.LogInformation("✅ Created new device token for user {UserId}", userId);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error registering device token for user {UserId}", userId);
                throw;
            }
        }

        public async Task UnregisterDeviceTokenAsync(string userId, string deviceToken, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("📱 Unregistering device token for user {UserId}", userId);
                
                var token = await _unitOfWork.GetContext().DeviceTokens
                    .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.Token == deviceToken, cancellationToken);

                if (token != null)
                {
                    token.IsActive = false;
                    _unitOfWork.GetContext().DeviceTokens.Update(token);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("✅ Device token unregistered for user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("⚠️ Device token not found for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error unregistering device token for user {UserId}", userId);
                throw;
            }
        }
    }
}

