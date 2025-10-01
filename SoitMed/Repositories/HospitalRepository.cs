using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Hospital;

namespace SoitMed.Repositories
{
    public class HospitalRepository : BaseRepository<Hospital>, IHospitalRepository
    {
        public HospitalRepository(Context context) : base(context)
        {
        }

        public async Task<Hospital?> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId, cancellationToken);
        }

        public async Task<bool> ExistsByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(h => h.HospitalId == hospitalId, cancellationToken);
        }

        public async Task<bool> ExistsByHospitalIdExcludingIdAsync(string hospitalId, string id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(h => h.HospitalId == hospitalId && h.HospitalId != id, cancellationToken);
        }

        public async Task<IEnumerable<Hospital>> GetActiveHospitalsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(h => h.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<Hospital?> GetHospitalWithDoctorsAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(h => h.DoctorHospitals)
                .ThenInclude(dh => dh.Doctor)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId, cancellationToken);
        }

        public async Task<Hospital?> GetHospitalWithTechniciansAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(h => h.Technicians)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId, cancellationToken);
        }

        public async Task<Hospital?> GetHospitalWithEquipmentAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(h => h.Equipment)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId, cancellationToken);
        }

        public async Task<Hospital?> GetHospitalWithAllDetailsAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(h => h.DoctorHospitals)
                .ThenInclude(dh => dh.Doctor)
                .Include(h => h.Technicians)
                .Include(h => h.Equipment)
                .FirstOrDefaultAsync(h => h.HospitalId == hospitalId, cancellationToken);
        }

        public async Task<IEnumerable<Hospital>> GetHospitalsWithDoctorsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(h => h.DoctorHospitals)
                .ThenInclude(dh => dh.Doctor)
                .Where(h => h.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsHospitalAssignedToDoctorAsync(string hospitalId, int doctorId, CancellationToken cancellationToken = default)
        {
            return await _context.DoctorHospitals
                .AnyAsync(dh => dh.HospitalId == hospitalId && dh.DoctorId == doctorId && dh.IsActive, cancellationToken);
        }
    }
}
