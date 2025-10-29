using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IDealRepository : IBaseRepository<Deal>
    {
<<<<<<< HEAD
        Task<IEnumerable<Deal>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deal>> GetByStatusAsync(DealStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deal>> GetByUserIdAndStatusAsync(string userId, DealStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalValueByUserIdAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<decimal> GetWonValueByUserIdAsync(string userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, decimal>> GetTotalValuesByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, decimal>> GetWonValuesByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
=======
        Task<IEnumerable<Deal>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deal>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    }
}
>>>>>>> dev
