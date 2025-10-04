using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Hospital;

namespace SoitMed.Repositories
{
    public class DoctorHospitalRepository : BaseRepository<DoctorHospital>, IDoctorHospitalRepository
    {
        public DoctorHospitalRepository(Context context) : base(context)
        {
        }

        public async Task<DoctorHospital?> GetByDoctorAndHospitalAsync(int doctorId, string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(dh => dh.DoctorId == doctorId && dh.HospitalId == hospitalId, cancellationToken);
        }

        public async Task<IEnumerable<DoctorHospital>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(dh => dh.DoctorId == doctorId && dh.IsActive)
                .Include(dh => dh.Hospital)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DoctorHospital>> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(dh => dh.HospitalId == hospitalId && dh.IsActive)
                .Include(dh => dh.Doctor)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByDoctorAndHospitalAsync(int doctorId, string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(dh => dh.DoctorId == doctorId && dh.HospitalId == hospitalId && dh.IsActive, cancellationToken);
        }

        public async Task<DoctorHospital?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<System.Func<DoctorHospital, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }
    }
}
