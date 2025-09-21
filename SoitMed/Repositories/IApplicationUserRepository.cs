using SoitMed.Models.Identity;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IApplicationUserRepository : IBaseRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<IEnumerable<ApplicationUser>> GetByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
        Task<ApplicationUser?> GetUserWithDepartmentAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default);
        Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default);
        Task<IEnumerable<ApplicationUser>> GetUsersByLastLoginDateAsync(DateTime date, CancellationToken cancellationToken = default);
    }
}
