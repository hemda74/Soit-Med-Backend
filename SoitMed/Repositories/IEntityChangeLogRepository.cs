using SoitMed.Models.Core;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Repository for EntityChangeLog (audit log)
    /// </summary>
    public interface IEntityChangeLogRepository : IBaseRepository<EntityChangeLog>
    {
        /// <summary>
        /// Gets change logs for a specific entity
        /// </summary>
        Task<IEnumerable<EntityChangeLog>> GetByEntityAsync(string entityName, int entityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets change logs for a specific user
        /// </summary>
        Task<IEnumerable<EntityChangeLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets change logs within a date range
        /// </summary>
        Task<IEnumerable<EntityChangeLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}

