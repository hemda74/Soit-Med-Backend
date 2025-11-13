using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public class SparePartRequestRepository : BaseRepository<SparePartRequest>, ISparePartRequestRepository
    {
        public SparePartRequestRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<SparePartRequest>> GetByMaintenanceRequestIdAsync(int maintenanceRequestId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(spr => spr.MaintenanceRequestId == maintenanceRequestId && spr.IsActive)
                .Include(spr => spr.MaintenanceRequest)
                .Include(spr => spr.MaintenanceVisit)
                .OrderByDescending(spr => spr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SparePartRequest>> GetByStatusAsync(SparePartAvailabilityStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(spr => spr.Status == status && spr.IsActive)
                .Include(spr => spr.MaintenanceRequest)
                .ThenInclude(mr => mr.Customer)
                .OrderByDescending(spr => spr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SparePartRequest>> GetByCoordinatorIdAsync(string coordinatorId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(spr => spr.AssignedToCoordinatorId == coordinatorId && spr.IsActive)
                .Include(spr => spr.MaintenanceRequest)
                .ThenInclude(mr => mr.Customer)
                .OrderByDescending(spr => spr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SparePartRequest>> GetByInventoryManagerIdAsync(string inventoryManagerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(spr => spr.AssignedToInventoryManagerId == inventoryManagerId && spr.IsActive)
                .Include(spr => spr.MaintenanceRequest)
                .ThenInclude(mr => mr.Customer)
                .OrderByDescending(spr => spr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<SparePartRequest?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(spr => spr.MaintenanceRequest)
                .ThenInclude(mr => mr.Customer)
                .Include(spr => spr.MaintenanceRequest)
                .ThenInclude(mr => mr.Equipment)
                .Include(spr => spr.MaintenanceVisit)
                .Include(spr => spr.AssignedToCoordinator)
                .Include(spr => spr.AssignedToInventoryManager)
                .Include(spr => spr.PriceSetByManager)
                .Include(spr => spr.Payments)
                .FirstOrDefaultAsync(spr => spr.Id == id, cancellationToken);
        }
    }
}

