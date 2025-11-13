using Microsoft.AspNetCore.Http;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;

namespace SoitMed.Services
{
    public interface IMaintenanceAttachmentService
    {
        Task<MaintenanceRequestAttachment> UploadAttachmentAsync(
            IFormFile file,
            int maintenanceRequestId,
            AttachmentType attachmentType,
            string? description = null,
            string? uploadedById = null);

        Task<bool> DeleteAttachmentAsync(int attachmentId);
        Task<MaintenanceRequestAttachment?> GetAttachmentAsync(int attachmentId);
        Task<IEnumerable<MaintenanceRequestAttachment>> GetAttachmentsByRequestAsync(int maintenanceRequestId);
        bool IsValidFile(IFormFile file, AttachmentType attachmentType);
    }
}

