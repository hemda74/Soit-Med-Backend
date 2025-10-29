using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public class OfferRepository : BaseRepository<Offer>, IOfferRepository
    {
        public OfferRepository(Context context) : base(context)
        {
        }

<<<<<<< HEAD
        public async Task<IEnumerable<Offer>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.ActivityLog)
                .Include(o => o.User)
                .Where(o => o.UserId == userId)
=======
        public async Task<IEnumerable<Offer>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Offers
                .Where(o => o.ActivityLog.UserId == userId)
                .Include(o => o.ActivityLog)
>>>>>>> dev
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

<<<<<<< HEAD
        public async Task<IEnumerable<Offer>> GetByStatusAsync(OfferStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.ActivityLog)
                .Include(o => o.User)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Offer>> GetByUserIdAndStatusAsync(string userId, OfferStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.ActivityLog)
                .Include(o => o.User)
                .Where(o => o.UserId == userId && o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Offer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.ActivityLog)
                .Include(o => o.User)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetOfferCountsByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => userIds.Contains(o.UserId) && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .GroupBy(o => o.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetAcceptedOfferCountsByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(o => userIds.Contains(o.UserId) && 
                           o.Status == OfferStatus.Accepted && 
                           o.CreatedAt >= startDate && 
                           o.CreatedAt <= endDate)
                .GroupBy(o => o.UserId)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }
    }
}
=======
        public async Task<IEnumerable<Offer>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            return await _context.Offers
                .Where(o => o.Status == status)
                .Include(o => o.ActivityLog)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
>>>>>>> dev
