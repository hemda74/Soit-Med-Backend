using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IMaintenanceVisitService
    {
        Task<MaintenanceVisitResponseDTO> CreateVisitAsync(CreateMaintenanceVisitDTO dto, string engineerId);
        Task<MaintenanceVisitResponseDTO?> GetVisitAsync(int id);
        Task<IEnumerable<MaintenanceVisitResponseDTO>> GetVisitsByRequestAsync(int maintenanceRequestId);
        Task<IEnumerable<MaintenanceVisitResponseDTO>> GetVisitsByEngineerAsync(string engineerId);
        Task<EquipmentDTO?> GetEquipmentByQRCodeAsync(string qrCode);
        Task<EquipmentDTO?> GetEquipmentBySerialCodeAsync(string serialCode);
    }
}

