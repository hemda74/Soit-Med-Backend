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

        public async Task<IEnumerable<Deal>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Deals
                .Where(d => d.ActivityLog.UserId == userId)
                .Include(d => d.ActivityLog)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Deal>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
        {
            return await _context.Deals
                .Where(d => d.Status == status)
                .Include(d => d.ActivityLog)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}