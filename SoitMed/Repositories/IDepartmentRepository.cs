using SoitMed.Models.Core;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IDepartmentRepository : IBaseRepository<Department>
    {
        Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameExcludingIdAsync(string name, int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Department>> GetDepartmentsWithUsersAsync(CancellationToken cancellationToken = default);
        Task<Department?> GetDepartmentWithUsersAsync(int id, CancellationToken cancellationToken = default);
    }
}
