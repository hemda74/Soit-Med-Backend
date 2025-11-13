using SoitMed.Models.Equipment;

namespace SoitMed.Repositories
{
    public interface IMaintenanceVisitRepository : IBaseRepository<MaintenanceVisit>
    {
        Task<IEnumerable<MaintenanceVisit>> GetByMaintenanceRequestIdAsync(int maintenanceRequestId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceVisit>> GetByEngineerIdAsync(string engineerId, CancellationToken cancellationToken = default);
        Task<MaintenanceVisit?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    }
}

