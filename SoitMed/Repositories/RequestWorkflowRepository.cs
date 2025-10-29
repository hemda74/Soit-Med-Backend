using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public class RequestWorkflowRepository : BaseRepository<RequestWorkflow>, IRequestWorkflowRepository
    {
        public RequestWorkflowRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<RequestWorkflow>> GetRequestsByRoleAsync(string role, RequestStatus? status = null, CancellationToken cancellationToken = default)
        {
            var query = _context.RequestWorkflows
                .Where(rw => rw.ToRole == role);

            if (status.HasValue)
            {
                query = query.Where(rw => rw.Status == status.Value);
            }

            return await query
                .Include(rw => rw.ActivityLog)
                .Include(rw => rw.Offer)
                .Include(rw => rw.Deal)
                .Include(rw => rw.DeliveryTerms)
                .Include(rw => rw.PaymentTerms)
                .OrderByDescending(rw => rw.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RequestWorkflow>> GetRequestsByUserIdAsync(string userId, RequestStatus? status = null, CancellationToken cancellationToken = default)
        {
            var query = _context.RequestWorkflows
                .Where(rw => rw.FromUserId == userId);

            if (status.HasValue)
            {
                query = query.Where(rw => rw.Status == status.Value);
            }

            return await query
                .Include(rw => rw.ActivityLog)
                .Include(rw => rw.Offer)
                .Include(rw => rw.Deal)
                .Include(rw => rw.DeliveryTerms)
                .Include(rw => rw.PaymentTerms)
                .OrderByDescending(rw => rw.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RequestWorkflow>> GetAssignedRequestsAsync(string userId, RequestStatus? status = null, CancellationToken cancellationToken = default)
        {
            var query = _context.RequestWorkflows
                .Where(rw => rw.ToUserId == userId);

            if (status.HasValue)
            {
                query = query.Where(rw => rw.Status == status.Value);
            }

            return await query
                .OrderByDescending(rw => rw.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<RequestWorkflow?> GetRequestWithDetailsAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _context.RequestWorkflows
                .Include(rw => rw.ActivityLog)
                .Include(rw => rw.Offer)
                .Include(rw => rw.Deal)
                .Include(rw => rw.DeliveryTerms)
                .Include(rw => rw.PaymentTerms)
                .FirstOrDefaultAsync(rw => rw.Id == id, cancellationToken);
        }
    }
}
