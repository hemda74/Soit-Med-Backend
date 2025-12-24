using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public class DealRepository : BaseRepository<Deal>, IDealRepository
    {
        public DealRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<Deal>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.ActivityLog)
                .Include(d => d.User)
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Deal>> GetByStatusAsync(Models.Enums.DealStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.ActivityLog)
                .Include(d => d.User)
                .Where(d => d.Status == status)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Deal>> GetByUserIdAndStatusAsync(string userId, Models.Enums.DealStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.ActivityLog)
                .Include(d => d.User)
                .Where(d => d.UserId == userId && d.Status == status)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Deal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.ActivityLog)
                .Include(d => d.User)
                .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalValueByUserIdAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.UserId == userId && d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                .SumAsync(d => d.DealValue, cancellationToken);
        }

        public async Task<decimal> GetWonValueByUserIdAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.UserId == userId && 
                           d.Status == Models.Enums.DealStatus.Completed && 
                           d.CreatedAt >= startDate && 
                           d.CreatedAt <= endDate)
                .SumAsync(d => d.DealValue, cancellationToken);
        }

        public async Task<Dictionary<string, decimal>> GetTotalValuesByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => userIds.Contains(d.UserId) && d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                .GroupBy(d => d.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(d => d.DealValue), cancellationToken);
        }

        public async Task<Dictionary<string, decimal>> GetWonValuesByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => userIds.Contains(d.UserId) && 
                           d.Status == Models.Enums.DealStatus.Completed && 
                           d.CreatedAt >= startDate && 
                           d.CreatedAt <= endDate)
                .GroupBy(d => d.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(d => d.DealValue), cancellationToken);
        }
    }
}
