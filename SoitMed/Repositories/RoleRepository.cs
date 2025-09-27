using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Core;

namespace SoitMed.Repositories
{
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        public RoleRepository(Context context) : base(context)
        {
        }

        public async Task<Role?> GetByRoleNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.RoleName == roleName, cancellationToken);
        }

        public async Task<bool> ExistsByRoleNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(r => r.RoleName == roleName, cancellationToken);
        }

        public async Task<bool> ExistsByRoleNameExcludingIdAsync(string roleName, int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(r => r.RoleName == roleName && r.RoleId != id, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(r => r.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
