using SoitMed.Models.Hospital;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IHospitalRepository : IBaseRepository<Hospital>
    {
        Task<Hospital?> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<bool> ExistsByHospitalIdExcludingIdAsync(string hospitalId, string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Hospital>> GetActiveHospitalsAsync(CancellationToken cancellationToken = default);
        Task<Hospital?> GetHospitalWithDoctorsAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<Hospital?> GetHospitalWithTechniciansAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<Hospital?> GetHospitalWithEquipmentAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<Hospital?> GetHospitalWithAllDetailsAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Hospital>> GetHospitalsWithDoctorsAsync(CancellationToken cancellationToken = default);
        Task<bool> IsHospitalAssignedToDoctorAsync(string hospitalId, int DoctorId, CancellationToken cancellationToken = default);
    }
}
