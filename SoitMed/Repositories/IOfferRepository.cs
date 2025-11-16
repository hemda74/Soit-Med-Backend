using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IOfferRepository : IBaseRepository<Offer>
    {
        Task<IEnumerable<Offer>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Offer>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    }
}
