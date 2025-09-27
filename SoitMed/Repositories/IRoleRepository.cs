using SoitMed.Models.Core;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IRoleRepository : IBaseRepository<Role>
    {
        Task<Role?> GetByRoleNameAsync(string roleName, CancellationToken cancellationToken = default);
        Task<bool> ExistsByRoleNameAsync(string roleName, CancellationToken cancellationToken = default);
        Task<bool> ExistsByRoleNameExcludingIdAsync(string roleName, int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default);
    }
}
