using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Equipment;

namespace SoitMed.Repositories
{
    public class EquipmentRepository : BaseRepository<Equipment>, IEquipmentRepository
    {
        public EquipmentRepository(Context context) : base(context)
        {
        }

        public async Task<Equipment?> GetByQRCodeAsync(string qrCode, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(e => e.QRCode == qrCode, cancellationToken);
        }

        public async Task<IEnumerable<Equipment>> GetByHospitalIdAsync(string hospitalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.HospitalId == hospitalId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Equipment>> GetByStatusAsync(EquipmentStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Equipment>> GetActiveEquipmentAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<Equipment?> GetEquipmentWithHospitalAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.Hospital)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Equipment?> GetEquipmentWithRepairRequestsAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.RepairRequests)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<Equipment?> GetEquipmentWithAllDetailsAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => e.Hospital)
                .Include(e => e.RepairRequests)
                .ThenInclude(rr => rr.AssignedEngineer)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByQRCodeAsync(string qrCode, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(e => e.QRCode == qrCode, cancellationToken);
        }

        public async Task<bool> ExistsByQRCodeExcludingIdAsync(string qrCode, string id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(e => e.QRCode == qrCode && e.Id != id, cancellationToken);
        }

        public async Task<IEnumerable<Equipment>> GetEquipmentByManufacturerAsync(string manufacturer, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.Manufacturer == manufacturer)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Equipment>> GetEquipmentByModelAsync(string model, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(e => e.Model == model)
                .ToListAsync(cancellationToken);
        }
    }
}
