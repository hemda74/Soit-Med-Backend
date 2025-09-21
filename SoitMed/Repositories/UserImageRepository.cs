using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Identity;

namespace SoitMed.Repositories
{
    public class UserImageRepository : BaseRepository<UserImage>, IUserImageRepository
    {
        public UserImageRepository(Context context) : base(context)
        {
        }

        public async Task<UserImage?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ui => ui.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<UserImage>> GetByImageTypeAsync(string imageType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(ui => ui.ImageType == imageType)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(ui => ui.UserId == userId, cancellationToken);
        }

        public async Task<UserImage?> GetUserImageWithUserAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(ui => ui.User)
                .FirstOrDefaultAsync(ui => ui.Id == id, cancellationToken);
        }
    }
}
