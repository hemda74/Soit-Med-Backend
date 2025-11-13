using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public class MaintenanceRequestRepository : BaseRepository<MaintenanceRequest>, IMaintenanceRequestRepository
    {
        public MaintenanceRequestRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mr => mr.CustomerId == customerId && mr.IsActive)
                .Include(mr => mr.Equipment)
                .Include(mr => mr.AssignedToEngineer)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetByEngineerIdAsync(string engineerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mr => mr.AssignedToEngineerId == engineerId && mr.IsActive)
                .Include(mr => mr.Customer)
                .Include(mr => mr.Equipment)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetByStatusAsync(MaintenanceRequestStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mr => mr.Status == status && mr.IsActive)
                .Include(mr => mr.Customer)
                .Include(mr => mr.Equipment)
                .Include(mr => mr.AssignedToEngineer)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<MaintenanceRequest?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(mr => mr.Customer)
                .Include(mr => mr.Hospital)
                .Include(mr => mr.Equipment)
                .Include(mr => mr.AssignedToEngineer)
                .Include(mr => mr.AssignedByMaintenanceSupport)
                .Include(mr => mr.Attachments)
                .Include(mr => mr.Visits)
                .Include(mr => mr.Payments)
                .Include(mr => mr.Ratings)
                .FirstOrDefaultAsync(mr => mr.Id == id, cancellationToken);
        }

        public async Task<MaintenanceRequest?> GetWithAttachmentsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(mr => mr.Attachments)
                .FirstOrDefaultAsync(mr => mr.Id == id, cancellationToken);
        }

        public async Task<MaintenanceRequest?> GetWithVisitsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(mr => mr.Visits)
                .ThenInclude(v => v.Engineer)
                .FirstOrDefaultAsync(mr => mr.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(mr => mr.Status == MaintenanceRequestStatus.Pending && mr.IsActive)
                .Include(mr => mr.Customer)
                .Include(mr => mr.Equipment)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}

