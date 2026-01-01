using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Core;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Repository implementation for EntityChangeLog
    /// </summary>
    public class EntityChangeLogRepository : BaseRepository<EntityChangeLog>, IEntityChangeLogRepository
    {
        public EntityChangeLogRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<EntityChangeLog>> GetByEntityAsync(string entityName, int entityId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ecl => ecl.EntityName == entityName && ecl.EntityId == entityId)
                .Include(ecl => ecl.User)
                .OrderByDescending(ecl => ecl.ChangeDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<EntityChangeLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ecl => ecl.UserId == userId)
                .OrderByDescending(ecl => ecl.ChangeDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<EntityChangeLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ecl => ecl.ChangeDate >= startDate && ecl.ChangeDate <= endDate)
                .Include(ecl => ecl.User)
                .OrderByDescending(ecl => ecl.ChangeDate)
                .ToListAsync(cancellationToken);
        }
    }
}

