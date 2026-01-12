using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class OfferEquipmentRepository : BaseRepository<OfferEquipment>, IOfferEquipmentRepository
    {
        public OfferEquipmentRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<OfferEquipment>> GetByOfferIdAsync(string offerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.OfferId == offerId)
                .ToListAsync(cancellationToken);
        }
    }
}



