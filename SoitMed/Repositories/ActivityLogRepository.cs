using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ActivityLogRepository : BaseRepository<ActivityLog>, IActivityLogRepository
    {
        public ActivityLogRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<ActivityLog>> GetWithDealsAndOffersAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.ActivityLogs
                .Where(al => al.UserId == userId && al.CreatedAt >= startDate && al.CreatedAt <= endDate)
                .Include(al => al.Deal)
                .Include(al => al.Offer)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ActivityLog>> GetByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var query = _context.ActivityLogs
                .Where(al => al.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(al => al.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(al => al.CreatedAt <= endDate.Value);
            }

            return await query
                .Include(al => al.Deal)
                .Include(al => al.Offer)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
