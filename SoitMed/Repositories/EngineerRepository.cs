using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Location;

namespace SoitMed.Repositories
{
    public class EngineerRepository : BaseRepository<Engineer>, IEngineerRepository
    {
        public EngineerRepository(Context context) : base(context)
        {
        }

        public async Task<Engineer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Engineer>> GetBySpecialtyAsync(string specialty, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.Specialty == specialty)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Engineer>> GetActiveEngineersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<Engineer?> GetEngineerWithUserAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EngineerId == id.ToString(), cancellationToken);
        }

        public async Task<Engineer?> GetEngineerWithGovernoratesAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.EngineerGovernorates)
                .ThenInclude(eg => eg.Governorate)
                .FirstOrDefaultAsync(e => e.EngineerId == id.ToString(), cancellationToken);
        }

        public async Task<Engineer?> GetEngineerWithAssignedRepairRequestsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.AssignedRepairRequests)
                .FirstOrDefaultAsync(e => e.EngineerId == id.ToString(), cancellationToken);
        }

        public async Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(e => e.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Engineer>> GetEngineersByGovernorateAsync(int governorateId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.EngineerGovernorates.Any(eg => eg.GovernorateId == governorateId.ToString()))
                .ToListAsync(cancellationToken);
        }
    }
}
