using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class InstallmentPlanRepository : BaseRepository<InstallmentPlan>, IInstallmentPlanRepository
    {
        public InstallmentPlanRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<InstallmentPlan>> GetByOfferIdAsync(long offerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(i => i.OfferId == offerId)
                .OrderBy(i => i.InstallmentNumber)
                .ToListAsync(cancellationToken);
        }
    }
}



