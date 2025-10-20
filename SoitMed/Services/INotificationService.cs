using SoitMed.Models;

namespace SoitMed.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(string userId, string title, string message, string type, string? priority = null, long? requestWorkflowId = null, long? activityLogId = null, bool isMobilePush = false, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, bool unreadOnly = false, CancellationToken cancellationToken = default);
        
        Task MarkNotificationAsReadAsync(long notificationId, string userId, CancellationToken cancellationToken = default);
        
        Task MarkAllNotificationsAsReadAsync(string userId, CancellationToken cancellationToken = default);
        
        Task<int> GetUnreadNotificationCountAsync(string userId, CancellationToken cancellationToken = default);
        
        Task SendRequestNotificationAsync(string fromUserId, string toRole, string requestType, long activityLogId, long? offerId, long? dealId, string clientName, string clientAddress, string equipmentDetails, int? deliveryTermsId, int? paymentTermsId, CancellationToken cancellationToken = default);
        
        Task SendAssignmentNotificationAsync(string fromUserId, string toUserId, long requestWorkflowId, CancellationToken cancellationToken = default);
        
        Task SendStatusUpdateNotificationAsync(long requestWorkflowId, string status, string? comments = null, CancellationToken cancellationToken = default);
    }
}

