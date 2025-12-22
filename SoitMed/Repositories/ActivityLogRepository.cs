using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ActivityLogRepository : BaseRepository<ActivityLog>, IActivityLogRepository
    {
        public ActivityLogRepository(Context context) : base(context)
        {
        }

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
                .OrderByDescending(al => al.CreatedAt)
                .ToListAsync(cancellationToken);
        }

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
