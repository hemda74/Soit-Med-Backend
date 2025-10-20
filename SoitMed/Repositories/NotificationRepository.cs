using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, bool unreadOnly = false, CancellationToken cancellationToken = default)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
        }

        public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            if (notifications.Any())
            {
                _context.Notifications.UpdateRange(notifications);
            }
        }
    }
}

