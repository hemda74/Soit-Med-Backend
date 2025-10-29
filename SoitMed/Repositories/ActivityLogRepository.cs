using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ActivityLogRepository : BaseRepository<ActivityLog>, IActivityLogRepository
    {
        public ActivityLogRepository(Context context) : base(context)
        {
        }

<<<<<<< HEAD
        public async Task<IEnumerable<ActivityLog>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.UserId == userId)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ActivityLog>> GetByPlanTaskIdAsync(long planTaskId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.PlanTaskId == planTaskId)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ActivityLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.CreatedAt >= startDate && al.CreatedAt <= endDate)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ActivityLog>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => al.UserId == userId && al.CreatedAt >= startDate && al.CreatedAt <= endDate)
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ActivityLog>> GetWithDealsAndOffersAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(al => al.Deal)
                .Include(al => al.Offer)
                .Include(al => al.User)
                .Where(al => al.UserId == userId && al.CreatedAt >= startDate && al.CreatedAt <= endDate)
=======
        public async Task<IEnumerable<ActivityLog>> GetWithDealsAndOffersAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.ActivityLogs
                .Where(al => al.UserId == userId && al.CreatedAt >= startDate && al.CreatedAt <= endDate)
                .Include(al => al.Deal)
                .Include(al => al.Offer)
>>>>>>> dev
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

<<<<<<< HEAD
        public async Task<Dictionary<string, int>> GetActivityCountsByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(al => userIds.Contains(al.UserId) && al.CreatedAt >= startDate && al.CreatedAt <= endDate)
                .GroupBy(al => al.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }

        public async Task<Dictionary<string, decimal>> GetDealValuesByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(al => al.Deal)
                .Where(al => userIds.Contains(al.UserId) && 
                           al.CreatedAt >= startDate && 
                           al.CreatedAt <= endDate && 
                           al.Deal != null)
                .GroupBy(al => al.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(al => al.Deal!.DealValue), cancellationToken);
        }
    }
}
=======
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
>>>>>>> dev
