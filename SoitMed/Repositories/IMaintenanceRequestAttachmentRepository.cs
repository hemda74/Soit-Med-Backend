using SoitMed.Models.Equipment;

namespace SoitMed.Repositories
{
    public interface IMaintenanceRequestAttachmentRepository : IBaseRepository<MaintenanceRequestAttachment>
    {
        Task<IEnumerable<MaintenanceRequestAttachment>> GetByMaintenanceRequestIdAsync(int maintenanceRequestId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceRequestAttachment>> GetByAttachmentTypeAsync(int maintenanceRequestId, Models.Enums.AttachmentType attachmentType, CancellationToken cancellationToken = default);
    }
}

