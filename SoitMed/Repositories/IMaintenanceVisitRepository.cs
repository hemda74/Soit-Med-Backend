using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IMaintenanceVisitRepository : IBaseRepository<MaintenanceVisit>
    {
        Task<IEnumerable<MaintenanceVisit>> GetByMaintenanceRequestIdAsync(int maintenanceRequestId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceVisit>> GetByEngineerIdAsync(string EngineerId, CancellationToken cancellationToken = default);
        Task<MaintenanceVisit?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        
        // New methods for Maintenance Lifecycle Module
        Task<IEnumerable<MaintenanceVisit>> GetVisitsByStatusAsync(VisitStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceVisit>> GetVisitsByEngineerAsync(string engineerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceVisit>> GetVisitsPendingApprovalAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceVisit>> GetVisitsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<MaintenanceVisit?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    }
}

