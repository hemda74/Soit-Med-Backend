using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IRecentOfferActivityRepository : IBaseRepository<RecentOfferActivity>
    {
        Task<IEnumerable<RecentOfferActivity>> GetRecentActivitiesAsync(int limit, CancellationToken cancellationToken = default);
        Task<IEnumerable<RecentOfferActivity>> GetActivitiesByOfferIdAsync(long offerId, CancellationToken cancellationToken = default);
        Task MaintainMaxActivitiesAsync(int maxCount, CancellationToken cancellationToken = default);
    }
}

