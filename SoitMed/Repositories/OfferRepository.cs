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

        public async Task<IEnumerable<Offer>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Offers
                .Where(o => o.ActivityLog.UserId == userId)
                .Include(o => o.ActivityLog)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Offer>> GetByStatusAsync(OfferStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Offers
                .Where(o => o.Status == status)
                .Include(o => o.ActivityLog)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}