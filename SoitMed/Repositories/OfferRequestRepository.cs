using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Repository for managing offer requests
    /// </summary>
    public class OfferRequestRepository : BaseRepository<OfferRequest>, IOfferRequestRepository
    {
        public OfferRequestRepository(Context context) : base(context)
        {
        }

        public async Task<List<OfferRequest>> GetRequestsByStatusAsync(string status)
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.Status == status)
                .OrderByDescending(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<List<OfferRequest>> GetRequestsBySalesManAsync(string salesmanId)
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.RequestedBy == salesmanId)
                .OrderByDescending(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<List<OfferRequest>> GetRequestsAssignedToAsync(string supportId)
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.AssignedTo == supportId)
                .OrderByDescending(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<List<OfferRequest>> GetRequestsByClientIdAsync(long clientId)
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.ClientId == clientId.ToString())
                .OrderByDescending(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<List<OfferRequest>> GetRequestsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.RequestDate >= startDate && or.RequestDate <= endDate)
                .OrderByDescending(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<List<OfferRequest>> GetPendingRequestsAsync()
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.Status == "Requested" || or.Status == "InProgress")
                .OrderBy(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<List<OfferRequest>> GetCompletedRequestsAsync()
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.Status == "Ready" || or.Status == "Sent")
                .OrderByDescending(or => or.CompletedAt)
                .ToListAsync();
        }

        public async Task<List<OfferRequest>> GetCancelledRequestsAsync()
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.Status == "Cancelled")
                .OrderByDescending(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<OfferRequest?> GetRequestWithDetailsAsync(long requestId)
        {
            return await _context.OfferRequests
                .Include(or => or.Requester)
                .Include(or => or.Client)
                .Include(or => or.AssignedSupportUser)
                .Include(or => or.TaskProgress)
                .Include(or => or.CreatedOffer)
                .FirstOrDefaultAsync(or => or.Id == requestId.ToString());
        }

        public async Task<List<OfferRequest>> GetRequestsByTaskProgressIdAsync(long taskProgressId)
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.TaskProgressId == taskProgressId)
                .OrderByDescending(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<int> GetRequestCountByStatusAsync(string status)
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .CountAsync(or => or.Status == status);
        }

        public async Task<int> GetRequestCountBySalesManAsync(string salesmanId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.RequestedBy == salesmanId);

            if (startDate.HasValue)
                query = query.Where(or => or.RequestDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(or => or.RequestDate <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<int> GetRequestCountBySupportAsync(string supportId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.AssignedTo == supportId);

            if (startDate.HasValue)
                query = query.Where(or => or.RequestDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(or => or.RequestDate <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<List<OfferRequest>> GetOverdueRequestsAsync()
        {
            var overdueDate = DateTime.UtcNow.AddDays(-3); // Requests older than 3 days

            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.Status == "Requested" && or.RequestDate <= overdueDate)
                .OrderBy(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<List<OfferRequest>> GetRequestsWithOffersAsync()
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.CreatedOffer != null)
                .OrderByDescending(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<List<OfferRequest>> GetRequestsWithoutOffersAsync()
        {
            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => or.CreatedOffer == null && or.Status != "Cancelled")
                .OrderBy(or => or.RequestDate)
                .ToListAsync();
        }

        public async Task<TimeSpan?> GetAverageProcessingTimeAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.OfferRequests
                .Where(or => or.CompletedAt.HasValue);

            if (startDate.HasValue)
                query = query.Where(or => or.RequestDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(or => or.RequestDate <= endDate.Value);

            var requests = await query.ToListAsync();
            if (!requests.Any())
                return null;

            var totalTime = requests.Sum(r => (r.CompletedAt!.Value - r.RequestDate).TotalMilliseconds);
            var averageTime = totalTime / requests.Count;

            return TimeSpan.FromMilliseconds(averageTime);
        }

        public async Task<List<OfferRequest>> GetByIdsAsync(IEnumerable<long> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any())
                return new List<OfferRequest>();

            return await _context.OfferRequests
                .AsNoTracking()
                .Where(or => idList.Contains(long.Parse(or.Id)))
                .ToListAsync();
        }
    }
}
