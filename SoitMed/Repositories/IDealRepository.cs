using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IDealRepository : IBaseRepository<Deal>
    {
        Task<IEnumerable<Deal>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Deal>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    }
}