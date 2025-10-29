using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IOfferRepository : IBaseRepository<Offer>
    {
<<<<<<< HEAD
        Task<IEnumerable<Offer>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Offer>> GetByStatusAsync(OfferStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Offer>> GetByUserIdAndStatusAsync(string userId, OfferStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Offer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetOfferCountsByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetAcceptedOfferCountsByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
=======
        Task<IEnumerable<Offer>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Offer>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    }
}
>>>>>>> dev
