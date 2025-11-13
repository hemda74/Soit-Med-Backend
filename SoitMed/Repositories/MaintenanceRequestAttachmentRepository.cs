using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Equipment;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public class MaintenanceRequestAttachmentRepository : BaseRepository<MaintenanceRequestAttachment>, IMaintenanceRequestAttachmentRepository
    {
        public MaintenanceRequestAttachmentRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<MaintenanceRequestAttachment>> GetByMaintenanceRequestIdAsync(int maintenanceRequestId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.MaintenanceRequestId == maintenanceRequestId && a.IsActive)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceRequestAttachment>> GetByAttachmentTypeAsync(int maintenanceRequestId, AttachmentType attachmentType, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(a => a.MaintenanceRequestId == maintenanceRequestId && 
                           a.AttachmentType == attachmentType && 
                           a.IsActive)
                .OrderByDescending(a => a.UploadedAt)
                .ToListAsync(cancellationToken);
        }
    }
}

