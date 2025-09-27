using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Hospital;

namespace SoitMed.Repositories
{
    public class DoctorRepository : BaseRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(Context context) : base(context)
        {
        }

        public async Task<Doctor?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<Doctor>> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.HospitalId == hospitalId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Doctor>> GetActiveDoctorsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(d => d.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<Doctor?> GetDoctorWithUserAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == id, cancellationToken);
        }

        public async Task<Doctor?> GetDoctorWithHospitalAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.Hospital)
                .FirstOrDefaultAsync(d => d.DoctorId == id, cancellationToken);
        }

        public async Task<Doctor?> GetDoctorWithRepairRequestsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.RepairRequests)
                .FirstOrDefaultAsync(d => d.DoctorId == id, cancellationToken);
        }

        public async Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(d => d.UserId == userId, cancellationToken);
        }
    }
}
