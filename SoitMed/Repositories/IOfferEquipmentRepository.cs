using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IOfferEquipmentRepository : IBaseRepository<OfferEquipment>
    {
        Task<IEnumerable<OfferEquipment>> GetByOfferIdAsync(long offerId, CancellationToken cancellationToken = default);
    }
}


