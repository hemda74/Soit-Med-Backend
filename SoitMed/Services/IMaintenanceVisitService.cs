using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IMaintenanceVisitService
    {
        Task<MaintenanceVisitResponseDTO> CreateVisitAsync(CreateMaintenanceVisitDTO dto, string EngineerId);
        Task<MaintenanceVisitResponseDTO?> GetVisitAsync(int id);
        Task<IEnumerable<MaintenanceVisitResponseDTO>> GetVisitsByRequestAsync(int maintenanceRequestId);
        Task<IEnumerable<MaintenanceVisitResponseDTO>> GetVisitsByEngineerAsync(string EngineerId);
        Task<IEnumerable<MaintenanceVisitResponseDTO>> GetVisitsByEquipmentIdAsync(string equipmentId);
        Task<EquipmentDTO?> GetEquipmentByQRCodeAsync(string qrCode);
        Task<EquipmentDTO?> GetEquipmentBySerialCodeAsync(string serialCode);
    }
}

