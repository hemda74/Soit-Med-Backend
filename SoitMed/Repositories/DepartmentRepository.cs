using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Core;

namespace SoitMed.Repositories
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(Context context) : base(context)
        {
        }

        public async Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.Name == name, cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(d => d.Name == name, cancellationToken);
        }

        public async Task<bool> ExistsByNameExcludingIdAsync(string name, int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(d => d.Name == name && d.Id != id, cancellationToken);
        }

        public async Task<IEnumerable<Department>> GetDepartmentsWithUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.Users)
                .ToListAsync(cancellationToken);
        }

        public async Task<Department?> GetDepartmentWithUsersAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.Users)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }
    }
}
