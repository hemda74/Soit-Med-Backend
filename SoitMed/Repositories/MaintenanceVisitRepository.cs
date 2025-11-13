using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Equipment;

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

        public async Task<IEnumerable<MaintenanceVisit>> GetByEngineerIdAsync(string engineerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mv => mv.EngineerId == engineerId && mv.IsActive)
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
                .FirstOrDefaultAsync(mv => mv.Id == id, cancellationToken);
        }
    }
}

