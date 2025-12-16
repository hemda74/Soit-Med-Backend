using SoitMed.Models.Hospital;

namespace SoitMed.Repositories
{
    public interface IDoctorHospitalRepository : IBaseRepository<DoctorHospital>
    {
        Task<DoctorHospital?> GetByDoctorAndHospitalAsync(int DoctorId, string hospitalId, CancellationToken cancellationToken = default);
        Task<IEnumerable<DoctorHospital>> GetByDoctorIdAsync(int DoctorId, CancellationToken cancellationToken = default);
        Task<IEnumerable<DoctorHospital>> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByDoctorAndHospitalAsync(int DoctorId, string hospitalId, CancellationToken cancellationToken = default);
        Task<DoctorHospital?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<System.Func<DoctorHospital, bool>> predicate, CancellationToken cancellationToken = default);
    }
}
