using SoitMed.Models;

namespace SoitMed.Services
{
    public interface IMobileNotificationService
    {
        Task SendPushNotificationAsync(string userId, string title, string message, Dictionary<string, object>? data = null, CancellationToken cancellationToken = default);
        
        Task SendBulkPushNotificationAsync(List<string> userIds, string title, string message, Dictionary<string, object>? data = null, CancellationToken cancellationToken = default);
        
        Task RegisterDeviceTokenAsync(string userId, string deviceToken, string platform, CancellationToken cancellationToken = default);
        
        Task UnregisterDeviceTokenAsync(string userId, string deviceToken, CancellationToken cancellationToken = default);
    }
}

