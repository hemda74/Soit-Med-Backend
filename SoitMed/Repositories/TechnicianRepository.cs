using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Hospital;

namespace SoitMed.Repositories
{
    public class TechnicianRepository : BaseRepository<Technician>, ITechnicianRepository
    {
        public TechnicianRepository(Context context) : base(context)
        {
        }

        public async Task<Technician?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Technician>> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.HospitalId == hospitalId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Technician>> GetActiveTechniciansAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<Technician?> GetTechnicianWithUserAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TechnicianId == id, cancellationToken);
        }

        public async Task<Technician?> GetTechnicianWithHospitalAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Hospital)
                .FirstOrDefaultAsync(t => t.TechnicianId == id, cancellationToken);
        }

        public async Task<Technician?> GetTechnicianWithRepairRequestsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.RepairRequests)
                .FirstOrDefaultAsync(t => t.TechnicianId == id, cancellationToken);
        }

        public async Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(t => t.UserId == userId, cancellationToken);
        }
    }
}
