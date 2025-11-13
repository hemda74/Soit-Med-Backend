using SoitMed.Models.Payment;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetByMaintenanceRequestIdAsync(int maintenanceRequestId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetBySparePartRequestIdAsync(int sparePartRequestId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetPendingPaymentsAsync(CancellationToken cancellationToken = default);
        Task<Payment?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    }
}

