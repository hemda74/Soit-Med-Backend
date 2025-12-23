using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Equipment;

namespace SoitMed.Repositories
{
    public class RepairRequestRepository : BaseRepository<RepairRequest>, IRepairRequestRepository
    {
        public RepairRequestRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<RepairRequest>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rr => rr.EquipmentId == equipmentId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RepairRequest>> GetByDoctorIdAsync(int DoctorId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rr => rr.DoctorId == DoctorId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RepairRequest>> GetByTechnicianIdAsync(int TechnicianId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rr => rr.TechnicianId == TechnicianId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RepairRequest>> GetByAssignedEngineerIdAsync(int EngineerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rr => rr.AssignedEngineerId == EngineerId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RepairRequest>> GetByStatusAsync(RepairStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rr => rr.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RepairRequest>> GetByPriorityAsync(RepairPriority priority, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rr => rr.Priority == priority)
                .ToListAsync(cancellationToken);
        }

        public async Task<RepairRequest?> GetRepairRequestWithEquipmentAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rr => rr.Equipment)
                .FirstOrDefaultAsync(rr => rr.Id == id, cancellationToken);
        }

        public async Task<RepairRequest?> GetRepairRequestWithDoctorAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rr => rr.RequestingDoctor)
                .FirstOrDefaultAsync(rr => rr.Id == id, cancellationToken);
        }

        public async Task<RepairRequest?> GetRepairRequestWithTechnicianAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rr => rr.RequestingTechnician)
                .FirstOrDefaultAsync(rr => rr.Id == id, cancellationToken);
        }

        public async Task<RepairRequest?> GetRepairRequestWithEngineerAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rr => rr.AssignedEngineer)
                .FirstOrDefaultAsync(rr => rr.Id == id, cancellationToken);
        }

        public async Task<RepairRequest?> GetRepairRequestWithAllDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rr => rr.Equipment)
                .Include(rr => rr.RequestingDoctor)
                .Include(rr => rr.RequestingTechnician)
                .Include(rr => rr.AssignedEngineer)
                .FirstOrDefaultAsync(rr => rr.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<RepairRequest>> GetActiveRepairRequestsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rr => rr.Status != RepairStatus.Completed && rr.Status != RepairStatus.Cancelled)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RepairRequest>> GetRepairRequestsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rr => rr.RequestedAt >= startDate && rr.RequestedAt <= endDate)
                .ToListAsync(cancellationToken);
        }
    }
}
