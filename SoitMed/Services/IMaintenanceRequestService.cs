using SoitMed.DTO;
using SoitMed.Models.Enums;

namespace SoitMed.Services
{
    public interface IMaintenanceRequestService
    {
        Task<MaintenanceRequestResponseDTO> CreateMaintenanceRequestAsync(CreateMaintenanceRequestDTO dto, string customerId);
        Task<MaintenanceRequestResponseDTO?> GetMaintenanceRequestAsync(int id);
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetCustomerRequestsAsync(string customerId);
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetEngineerRequestsAsync(string EngineerId);
        Task<IEnumerable<MaintenanceRequestResponseDTO>> GetPendingRequestsAsync();
        Task<MaintenanceRequestResponseDTO> AssignToEngineerAsync(int requestId, AssignMaintenanceRequestDTO dto, string maintenanceSupportId);
        Task<MaintenanceRequestResponseDTO> UpdateStatusAsync(int requestId, MaintenanceRequestStatus status, string? notes = null);
        Task<MaintenanceRequestResponseDTO> CancelRequestAsync(int requestId, string userId, string? reason = null);
        Task<MaintenanceRequestResponseDTO> FinalizeJobAndProcessPaymentAsync(int requestId, FinalizeJobDTO dto, string engineerId);
    }
}

