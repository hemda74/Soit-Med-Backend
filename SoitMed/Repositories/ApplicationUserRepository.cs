using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Identity;

namespace SoitMed.Repositories
{
    public class ApplicationUserRepository : BaseRepository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(Context context) : base(context)
        {
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<IEnumerable<ApplicationUser>> GetByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => u.DepartmentId == departmentId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ApplicationUser>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => u.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<ApplicationUser?> GetUserWithDepartmentAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(u => u.UserName == userName, cancellationToken);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default)
        {
            // This would require a more complex query with role checking
            // For now, returning all users - this can be enhanced based on your role management system
            return await _dbSet
                .Where(u => u.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
        {
            // This would require a more complex query with role checking
            // For now, returning all users - this can be enhanced based on your role management system
            return await _dbSet
                .Where(u => u.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersByLastLoginDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(u => u.LastLoginAt.HasValue && u.LastLoginAt.Value.Date == date.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ApplicationUser>> GetByIdsAsync(IEnumerable<string> ids)
        {
            var idList = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!idList.Any())
                return new List<ApplicationUser>();

            // Safety limit to prevent memory issues and connection pool exhaustion
            const int maxBatchSize = 1000;
            if (idList.Count > maxBatchSize)
            {
                idList = idList.Take(maxBatchSize).ToList();
            }

            return await _dbSet
                .AsNoTracking()
                .Where(u => idList.Contains(u.Id))
                .ToListAsync();
        }
    }
}
