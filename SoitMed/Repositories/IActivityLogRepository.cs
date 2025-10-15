using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IActivityLogRepository : IBaseRepository<ActivityLog>
    {
        Task<IEnumerable<ActivityLog>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ActivityLog>> GetByPlanTaskIdAsync(long planTaskId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ActivityLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<ActivityLog>> GetByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IEnumerable<ActivityLog>> GetWithDealsAndOffersAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetActivityCountsByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, decimal>> GetDealValuesByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
