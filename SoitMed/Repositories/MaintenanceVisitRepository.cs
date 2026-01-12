using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public class MaintenanceVisitRepository : BaseRepository<MaintenanceVisit>, IMaintenanceVisitRepository
    {
        public MaintenanceVisitRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<MaintenanceVisit>> GetByMaintenanceRequestIdAsync(int maintenanceRequestId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mv => mv.MaintenanceRequestId == maintenanceRequestId && mv.IsActive)
                .Include(mv => mv.Engineer)
                .Include(mv => mv.SparePartRequest)
                .OrderByDescending(mv => mv.VisitDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceVisit>> GetByEngineerIdAsync(string EngineerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mv => mv.EngineerId == EngineerId && mv.IsActive)
                .Include(mv => mv.MaintenanceRequest)
                .ThenInclude(mr => mr.Customer)
                .Include(mv => mv.MaintenanceRequest)
                .ThenInclude(mr => mr.Equipment)
                .OrderByDescending(mv => mv.VisitDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<MaintenanceVisit?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(mv => mv.MaintenanceRequest)
                .ThenInclude(mr => mr.Customer)
                .Include(mv => mv.MaintenanceRequest)
                .ThenInclude(mr => mr.Equipment)
                .Include(mv => mv.Engineer)
                .Include(mv => mv.SparePartRequest)
                .Include(mv => mv.Device)
                .Include(mv => mv.Customer)
                .Include(mv => mv.Assignees)
                .ThenInclude(a => a.Engineer)
                .Include(mv => mv.VisitReport)
                .FirstOrDefaultAsync(mv => mv.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceVisit>> GetVisitsByStatusAsync(VisitStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mv => mv.Status == status && mv.IsActive)
                .Include(mv => mv.Customer)
                .Include(mv => mv.Device)
                .Include(mv => mv.Engineer)
                .OrderBy(mv => mv.ScheduledDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceVisit>> GetVisitsByEngineerAsync(string engineerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mv => mv.IsActive && (
                    mv.EngineerId == engineerId ||
                    mv.Assignees.Any(a => a.EngineerId == engineerId)
                ))
                .Include(mv => mv.Customer)
                .Include(mv => mv.Device)
                .Include(mv => mv.MaintenanceRequest)
                .OrderBy(mv => mv.ScheduledDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceVisit>> GetVisitsPendingApprovalAsync(CancellationToken cancellationToken = default)
        {
            return await GetVisitsByStatusAsync(VisitStatus.PendingApproval, cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceVisit>> GetVisitsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mv => mv.ScheduledDate >= startDate && mv.ScheduledDate <= endDate && mv.IsActive)
                .Include(mv => mv.Customer)
                .Include(mv => mv.Device)
                .Include(mv => mv.Engineer)
                .OrderBy(mv => mv.ScheduledDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<MaintenanceVisit?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(mv => mv.MaintenanceRequest)
                .ThenInclude(mr => mr.Customer)
                .Include(mv => mv.MaintenanceRequest)
                .ThenInclude(mr => mr.Equipment)
                .Include(mv => mv.Engineer)
                .Include(mv => mv.Device)
                .Include(mv => mv.Customer)
                .Include(mv => mv.Assignees)
                .ThenInclude(a => a.Engineer)
                .Include(mv => mv.VisitReport)
                .FirstOrDefaultAsync(mv => mv.TicketNumber == ticketNumber, cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceVisit>> GetVisitsByEquipmentIdAsync(string equipmentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mv => mv.IsActive && (
                    mv.DeviceId == equipmentId ||
                    (mv.MaintenanceRequest != null && mv.MaintenanceRequest.EquipmentId == equipmentId)
                ))
                .Include(mv => mv.MaintenanceRequest)
                .ThenInclude(mr => mr.Customer)
                .Include(mv => mv.MaintenanceRequest)
                .ThenInclude(mr => mr.Equipment)
                .Include(mv => mv.Engineer)
                .Include(mv => mv.Device)
                .Include(mv => mv.Customer)
                .Include(mv => mv.Assignees)
                .ThenInclude(a => a.Engineer)
                .Include(mv => mv.VisitReport)
                .OrderByDescending(mv => mv.VisitDate != default ? mv.VisitDate : mv.ScheduledDate)
                .ToListAsync(cancellationToken);
        }
    }
}

