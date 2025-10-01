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
                .Where(d => d.DoctorHospitals.Any(dh => dh.HospitalId == hospitalId && dh.IsActive))
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

        public async Task<Doctor?> GetDoctorWithHospitalsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.DoctorHospitals)
                .ThenInclude(dh => dh.Hospital)
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

        public async Task<bool> IsDoctorAssignedToHospitalAsync(int doctorId, string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _context.DoctorHospitals
                .AnyAsync(dh => dh.DoctorId == doctorId && dh.HospitalId == hospitalId && dh.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsWithHospitalsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(d => d.DoctorHospitals)
                .ThenInclude(dh => dh.Hospital)
                .Where(d => d.IsActive)
                .ToListAsync(cancellationToken);
        }
    }
}
