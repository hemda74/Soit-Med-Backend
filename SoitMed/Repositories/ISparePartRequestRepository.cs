using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface ISparePartRequestRepository : IBaseRepository<SparePartRequest>
    {
        Task<IEnumerable<SparePartRequest>> GetByMaintenanceRequestIdAsync(int maintenanceRequestId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SparePartRequest>> GetByStatusAsync(SparePartAvailabilityStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<SparePartRequest>> GetByCoordinatorIdAsync(string coordinatorId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SparePartRequest>> GetByInventoryManagerIdAsync(string inventoryManagerId, CancellationToken cancellationToken = default);
        Task<SparePartRequest?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    }
}

