using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Payment;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && p.IsActive)
                .Include(p => p.MaintenanceRequest)
                .Include(p => p.SparePartRequest)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Status == status && p.IsActive)
                .Include(p => p.Customer)
                .Include(p => p.MaintenanceRequest)
                .Include(p => p.SparePartRequest)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetByMaintenanceRequestIdAsync(int maintenanceRequestId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.MaintenanceRequestId == maintenanceRequestId && p.IsActive)
                .Include(p => p.Customer)
                .Include(p => p.Transactions)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetBySparePartRequestIdAsync(int sparePartRequestId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.SparePartRequestId == sparePartRequestId && p.IsActive)
                .Include(p => p.Customer)
                .Include(p => p.Transactions)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing) && p.IsActive)
                .Include(p => p.Customer)
                .Include(p => p.MaintenanceRequest)
                .Include(p => p.SparePartRequest)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Payment?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.MaintenanceRequest)
                .ThenInclude(mr => mr.Equipment)
                .Include(p => p.SparePartRequest)
                .Include(p => p.ProcessedByAccountant)
                .Include(p => p.Transactions)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.CreatedAt >= from && p.CreatedAt <= to && p.IsActive)
                .Include(p => p.Customer)
                .Include(p => p.MaintenanceRequest)
                .Include(p => p.SparePartRequest)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}

