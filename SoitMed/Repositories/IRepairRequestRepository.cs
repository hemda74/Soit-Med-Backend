using SoitMed.Models.Equipment;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IRepairRequestRepository : IBaseRepository<RepairRequest>
    {
        Task<IEnumerable<RepairRequest>> GetByEquipmentIdAsync(int equipmentId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RepairRequest>> GetByDoctorIdAsync(int DoctorId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RepairRequest>> GetByTechnicianIdAsync(int TechnicianId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RepairRequest>> GetByAssignedEngineerIdAsync(int EngineerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RepairRequest>> GetByStatusAsync(RepairStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<RepairRequest>> GetByPriorityAsync(RepairPriority priority, CancellationToken cancellationToken = default);
        Task<RepairRequest?> GetRepairRequestWithEquipmentAsync(int id, CancellationToken cancellationToken = default);
        Task<RepairRequest?> GetRepairRequestWithDoctorAsync(int id, CancellationToken cancellationToken = default);
        Task<RepairRequest?> GetRepairRequestWithTechnicianAsync(int id, CancellationToken cancellationToken = default);
        Task<RepairRequest?> GetRepairRequestWithEngineerAsync(int id, CancellationToken cancellationToken = default);
        Task<RepairRequest?> GetRepairRequestWithAllDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<RepairRequest>> GetActiveRepairRequestsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<RepairRequest>> GetRepairRequestsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
