using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IActivityLogRepository : IBaseRepository<ActivityLog>
    {
        Task<IEnumerable<ActivityLog>> GetWithDealsAndOffersAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<ActivityLog>> GetByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    }
}
