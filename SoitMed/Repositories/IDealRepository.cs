using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IDealRepository : IBaseRepository<Deal>
    {
        Task<IEnumerable<Deal>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deal>> GetByStatusAsync(Models.Enums.DealStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deal>> GetByUserIdAndStatusAsync(string userId, Models.Enums.DealStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalValueByUserIdAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<decimal> GetWonValueByUserIdAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, decimal>> GetTotalValuesByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, decimal>> GetWonValuesByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
