using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, bool unreadOnly = false, CancellationToken cancellationToken = default);
        Task<int> GetUnreadNotificationCountAsync(string userId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
    }
}

