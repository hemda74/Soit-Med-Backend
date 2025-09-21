using SoitMed.Models.Hospital;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface ITechnicianRepository : IBaseRepository<Technician>
    {
        Task<Technician?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Technician>> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Technician>> GetActiveTechniciansAsync(CancellationToken cancellationToken = default);
        Task<Technician?> GetTechnicianWithUserAsync(int id, CancellationToken cancellationToken = default);
        Task<Technician?> GetTechnicianWithHospitalAsync(int id, CancellationToken cancellationToken = default);
        Task<Technician?> GetTechnicianWithRepairRequestsAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    }
}
