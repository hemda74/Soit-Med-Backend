using SoitMed.Models.Identity;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IUserImageRepository : IBaseRepository<UserImage>
    {
        Task<UserImage?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserImage>> GetByImageTypeAsync(string imageType, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<UserImage?> GetUserImageWithUserAsync(int id, CancellationToken cancellationToken = default);
    }
}
