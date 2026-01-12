using SoitMed.Models.Equipment;
using System.Linq.Expressions;

namespace SoitMed.Repositories
{
    public interface IEquipmentRepository : IBaseRepository<Equipment>
    {
        Task<Equipment?> GetByQRCodeAsync(string qrCode, CancellationToken cancellationToken = default);
        Task<IEnumerable<Equipment>> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Equipment>> GetByStatusAsync(EquipmentStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Equipment>> GetActiveEquipmentAsync(CancellationToken cancellationToken = default);
        Task<Equipment?> GetEquipmentWithHospitalAsync(string id, CancellationToken cancellationToken = default);
        Task<Equipment?> GetEquipmentWithRepairRequestsAsync(string id, CancellationToken cancellationToken = default);
        Task<Equipment?> GetEquipmentWithAllDetailsAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByQRCodeAsync(string qrCode, CancellationToken cancellationToken = default);
        Task<bool> ExistsByQRCodeExcludingIdAsync(string qrCode, string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Equipment>> GetEquipmentByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default);
        Task<IEnumerable<Equipment>> GetEquipmentByModelAsync(string model, CancellationToken cancellationToken = default);
    }
}
