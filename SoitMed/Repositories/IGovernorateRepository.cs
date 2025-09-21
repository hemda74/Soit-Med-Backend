using SoitMed.Models.Location;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IGovernorateRepository : IBaseRepository<Governorate>
    {
        Task<Governorate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameExcludingIdAsync(string name, int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Governorate>> GetActiveGovernoratesAsync(CancellationToken cancellationToken = default);
        Task<Governorate?> GetGovernorateWithEngineersAsync(int id, CancellationToken cancellationToken = default);
    }
}
