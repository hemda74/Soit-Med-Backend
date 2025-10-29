using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ClientVisitRepository : BaseRepository<ClientVisit>, IClientVisitRepository
    {
        public ClientVisitRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<ClientVisit>> GetClientVisitsAsync(long clientId, DateTime? startDate = null, DateTime? endDate = null, string? salesmanId = null, string? status = null, int page = 1, int pageSize = 20)
        {
            var query = _context.ClientVisits
                .Where(v => v.ClientId == clientId);

            if (startDate.HasValue)
                query = query.Where(v => v.VisitDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(v => v.VisitDate <= endDate.Value);

            if (!string.IsNullOrEmpty(salesmanId))
                query = query.Where(v => v.SalesmanId == salesmanId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(v => v.Status == status);

            return await query
                .OrderByDescending(v => v.VisitDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientVisit>> GetSalesmanVisitsAsync(string salesmanId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            var query = _context.ClientVisits
                .Where(v => v.SalesmanId == salesmanId);

            if (startDate.HasValue)
                query = query.Where(v => v.VisitDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(v => v.VisitDate <= endDate.Value);

            return await query
                .OrderByDescending(v => v.VisitDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<decimal> GetAverageVisitDurationAsync(long clientId)
        {
            var visits = await _context.ClientVisits
                .Where(v => v.ClientId == clientId && v.Status == "Completed")
                .ToListAsync();

            return visits.Any() ? (decimal)visits.Count() : 0;
        }

        public async Task<int> GetTotalVisitsCountAsync(long clientId)
        {
            return await _context.ClientVisits
                .CountAsync(v => v.ClientId == clientId);
        }

        public async Task<DateTime?> GetLastVisitDateAsync(long clientId)
        {
            var lastVisit = await _context.ClientVisits
                .Where(v => v.ClientId == clientId && v.Status == "Completed")
                .OrderByDescending(v => v.VisitDate)
                .FirstOrDefaultAsync();

            return lastVisit?.VisitDate;
        }

        public async Task<DateTime?> GetNextScheduledVisitAsync(long clientId)
        {
            var nextVisit = await _context.ClientVisits
                .Where(v => v.ClientId == clientId && v.Status == "Scheduled")
                .OrderBy(v => v.VisitDate)
                .FirstOrDefaultAsync();

            return nextVisit?.VisitDate;
        }
    }
}