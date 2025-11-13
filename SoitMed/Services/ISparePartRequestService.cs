using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface ISparePartRequestService
    {
        Task<SparePartRequestResponseDTO> CreateSparePartRequestAsync(CreateSparePartRequestDTO dto, int maintenanceVisitId);
        Task<SparePartRequestResponseDTO?> GetSparePartRequestAsync(int id);
        Task<IEnumerable<SparePartRequestResponseDTO>> GetByMaintenanceRequestAsync(int maintenanceRequestId);
        Task<SparePartRequestResponseDTO> CheckAvailabilityAsync(int sparePartRequestId, string coordinatorId, bool isLocalAvailable);
        Task<SparePartRequestResponseDTO> SetPriceAsync(int sparePartRequestId, UpdateSparePartPriceDTO dto, string managerId);
        Task<SparePartRequestResponseDTO> CustomerDecisionAsync(int sparePartRequestId, CustomerSparePartDecisionDTO dto, string customerId);
        Task<SparePartRequestResponseDTO> MarkAsReadyAsync(int sparePartRequestId, string inventoryManagerId);
        Task<SparePartRequestResponseDTO> MarkAsDeliveredToEngineerAsync(int sparePartRequestId);
    }
}

