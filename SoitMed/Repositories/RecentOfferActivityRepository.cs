using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class RecentOfferActivityRepository : BaseRepository<RecentOfferActivity>, IRecentOfferActivityRepository
    {
        public RecentOfferActivityRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<RecentOfferActivity>> GetRecentActivitiesAsync(int limit, CancellationToken cancellationToken = default)
        {
            return await _context.RecentOfferActivities
                .OrderByDescending(a => a.ActivityTimestamp)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<RecentOfferActivity>> GetActivitiesByOfferIdAsync(long offerId, CancellationToken cancellationToken = default)
        {
            return await _context.RecentOfferActivities
                .Where(a => a.OfferId == offerId)
                .OrderByDescending(a => a.ActivityTimestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task MaintainMaxActivitiesAsync(int maxCount, CancellationToken cancellationToken = default)
        {
            var totalCount = await _context.RecentOfferActivities.CountAsync(cancellationToken);
            
            if (totalCount >= maxCount)
            {
                // Get IDs of activities to keep (the most recent ones)
                var activitiesToKeep = await _context.RecentOfferActivities
                    .OrderByDescending(a => a.ActivityTimestamp)
                    .Take(maxCount)
                    .Select(a => a.Id)
                    .ToListAsync(cancellationToken);

                // Delete all activities that are not in the keep list
                var activitiesToDelete = await _context.RecentOfferActivities
                    .Where(a => !activitiesToKeep.Contains(a.Id))
                    .ToListAsync(cancellationToken);

                if (activitiesToDelete.Any())
                {
                    _context.RecentOfferActivities.RemoveRange(activitiesToDelete);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}

