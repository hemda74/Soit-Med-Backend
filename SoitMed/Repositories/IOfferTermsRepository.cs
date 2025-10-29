using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IOfferTermsRepository : IBaseRepository<OfferTerms>
    {
        Task<OfferTerms?> GetByOfferIdAsync(long offerId, CancellationToken cancellationToken = default);
    }
}



