using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IOfferTermsRepository : IBaseRepository<OfferTerms>
    {
        Task<OfferTerms?> GetByOfferIdAsync(string offerId, CancellationToken cancellationToken = default);
    }
}



