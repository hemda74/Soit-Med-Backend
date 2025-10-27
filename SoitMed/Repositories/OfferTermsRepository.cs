using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class OfferTermsRepository : BaseRepository<OfferTerms>, IOfferTermsRepository
    {
        public OfferTermsRepository(Context context) : base(context)
        {
        }

        public async Task<OfferTerms?> GetByOfferIdAsync(long offerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.OfferId == offerId, cancellationToken);
        }
    }
}


