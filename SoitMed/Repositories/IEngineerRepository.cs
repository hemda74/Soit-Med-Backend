using SoitMed.Models.Location;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IEngineerRepository : IBaseRepository<Engineer>
    {
        Task<Engineer?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Engineer>> GetBySpecialtyAsync(string specialty, CancellationToken cancellationToken = default);
        Task<IEnumerable<Engineer>> GetActiveEngineersAsync(CancellationToken cancellationToken = default);
        Task<Engineer?> GetEngineerWithUserAsync(int id, CancellationToken cancellationToken = default);
        Task<Engineer?> GetEngineerWithGovernoratesAsync(int id, CancellationToken cancellationToken = default);
        Task<Engineer?> GetEngineerWithAssignedRepairRequestsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Engineer>> GetEngineersByGovernorateAsync(int governorateId, CancellationToken cancellationToken = default);
    }
}
