using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IOfferEquipmentRepository : IBaseRepository<OfferEquipment>
    {
        Task<IEnumerable<OfferEquipment>> GetByOfferIdAsync(string offerId, CancellationToken cancellationToken = default);
    }
}



