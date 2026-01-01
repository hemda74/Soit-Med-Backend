using SoitMed.Models.Equipment;

namespace SoitMed.Services
{
    /// <summary>
    /// Interface for maintenance visit lifecycle management service
    /// </summary>
    public interface IMaintenanceService
    {
        /// <summary>
        /// Creates a new maintenance visit with proper state management
        /// </summary>
        Task<MaintenanceVisit> CreateVisitAsync(CreateVisitDTO dto, string userId);

        /// <summary>
        /// Approves a pending visit (transitions to Scheduled)
        /// </summary>
        Task<MaintenanceVisit> ApproveVisitAsync(int visitId, string approverId);

        /// <summary>
        /// Assigns engineers to a visit
        /// </summary>
        Task<MaintenanceVisit> AssignEngineersAsync(int visitId, List<string> engineerIds, string assignedById);

        /// <summary>
        /// Verifies machine QR code and starts visit (transitions to InProgress)
        /// </summary>
        Task<MaintenanceVisit> VerifyMachineAndStartVisitAsync(int visitId, string scannedQrCode, string engineerId);
    }
}

