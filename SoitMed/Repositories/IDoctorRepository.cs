using SoitMed.Models.Hospital;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IDoctorRepository : IBaseRepository<Doctor>
    {
        Task<Doctor?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Doctor>> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Doctor>> GetActiveDoctorsAsync(CancellationToken cancellationToken = default);
        Task<Doctor?> GetDoctorWithUserAsync(int id, CancellationToken cancellationToken = default);
        Task<Doctor?> GetDoctorWithHospitalAsync(int id, CancellationToken cancellationToken = default);
        Task<Doctor?> GetDoctorWithRepairRequestsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    }
}
