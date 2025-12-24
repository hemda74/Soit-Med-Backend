using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IOfferRepository : IBaseRepository<Offer>
    {
        Task<IEnumerable<Offer>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Offer>> GetByStatusAsync(Models.Enums.OfferStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Offer>> GetByUserIdAndStatusAsync(string userId, Models.Enums.OfferStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Offer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetOfferCountsByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetAcceptedOfferCountsByUserAsync(IEnumerable<string> userIds, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
