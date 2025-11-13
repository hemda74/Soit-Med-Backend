using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IMaintenanceRequestRepository : IBaseRepository<MaintenanceRequest>
    {
        Task<IEnumerable<MaintenanceRequest>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceRequest>> GetByEngineerIdAsync(string engineerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceRequest>> GetByStatusAsync(MaintenanceRequestStatus status, CancellationToken cancellationToken = default);
        Task<MaintenanceRequest?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<MaintenanceRequest?> GetWithAttachmentsAsync(int id, CancellationToken cancellationToken = default);
        Task<MaintenanceRequest?> GetWithVisitsAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    }
}

