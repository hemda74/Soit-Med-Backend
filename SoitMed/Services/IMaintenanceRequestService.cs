using SoitMed.DTO;
using SoitMed.Models.Enums;

namespace SoitMed.Services
{
    public interface IMaintenanceRequestService
    {
        Task<MaintenanceRequestResponseDTO> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDTO dto, string customerId);
        Task<MaintenanceRequestResponseDTO?> GetMaintenanceRequestAsync(int id);
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetCustomerRequestsAsync(string customerId);
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetEngineerRequestsAsync(string engineerId);
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetPendingRequestsAsync();
        Task<MaintenanceRequestResponseDTO> AssignToEngineerAsync(int requestId, AssignMaintenanceRequestDTO dto, string maintenanceSupportId);
        Task<MaintenanceRequestResponseDTO> UpdateStatusAsync(int requestId, MaintenanceRequestStatus status, string? notes = null);
    }
}

