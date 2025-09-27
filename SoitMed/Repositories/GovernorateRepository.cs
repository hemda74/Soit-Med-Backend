using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Location;

namespace SoitMed.Repositories
{
    public class GovernorateRepository : BaseRepository<Governorate>, IGovernorateRepository
    {
        public GovernorateRepository(Context context) : base(context)
        {
        }

        public async Task<Governorate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(g => g.Name == name, cancellationToken);
        }

        public async Task<bool> ExistsByNameExcludingIdAsync(string name, int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(g => g.Name == name && g.GovernorateId != id, cancellationToken);
        }

        public async Task<IEnumerable<Governorate>> GetActiveGovernoratesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(g => g.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<Governorate?> GetGovernorateWithEngineersAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(g => g.EngineerGovernorates)
                .ThenInclude(eg => eg.Engineer)
                .FirstOrDefaultAsync(g => g.GovernorateId == id, cancellationToken);
        }
    }
}
