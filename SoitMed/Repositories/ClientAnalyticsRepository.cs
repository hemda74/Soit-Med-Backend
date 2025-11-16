using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ClientAnalyticsRepository : BaseRepository<ClientAnalytics>, IClientAnalyticsRepository
    {
        public ClientAnalyticsRepository(Context context) : base(context)
        {
        }

        public async Task<ClientAnalytics?> GetClientAnalyticsAsync(long clientId, string period)
        {
            return await _context.ClientAnalytics
                .FirstOrDefaultAsync(a => a.ClientId == clientId && a.Period == period);
        }

        public async Task<IEnumerable<ClientAnalytics>> GetClientAnalyticsHistoryAsync(long clientId, int page = 1, int pageSize = 20)
        {
            return await _context.ClientAnalytics
                .Where(a => a.ClientId == clientId)
                .OrderByDescending(a => a.PeriodStart)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<object?> GetClientSummaryAsync(long clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return null;

            var totalVisits = await _context.ClientVisits.CountAsync(v => v.ClientId == clientId);
            var totalInteractions = await _context.ClientInteractions.CountAsync(i => i.ClientId == clientId);
            var lastVisit = await _context.ClientVisits
                .Where(v => v.ClientId == clientId && v.Status == "Completed")
                .OrderByDescending(v => v.VisitDate)
                .FirstOrDefaultAsync();
            var nextVisit = await _context.ClientVisits
                .Where(v => v.ClientId == clientId && v.Status == "Scheduled")
                .OrderBy(v => v.VisitDate)
                .FirstOrDefaultAsync();

            return new
            {
                ClientId = clientId,
                ClientName = client.Name,
                TotalVisits = totalVisits,
                TotalInteractions = totalInteractions,
                LastVisitDate = lastVisit?.VisitDate,
                NextScheduledVisit = nextVisit?.VisitDate,
                ClientSatisfactionScore = (int?)null, // Removed from simplified Client model
                Status = (string?)null, // Removed from simplified Client model
                Priority = (string?)null, // Removed from simplified Client model
                PotentialValue = (decimal?)null // Removed from simplified Client model
            };
        }

        public async Task<object> GetClientTimelineAsync(long clientId, int limit = 50)
        {
            var visits = await _context.ClientVisits
                .Where(v => v.ClientId == clientId)
                .OrderByDescending(v => v.VisitDate)
                .Take(limit)
                .Select(v => new
                {
                    Type = "Visit",
                    Date = v.VisitDate,
                    Description = v.Purpose,
                    Status = v.Status,
                    Results = v.Results
                })
                .ToListAsync();

            var interactions = await _context.ClientInteractions
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.InteractionDate)
                .Take(limit)
                .Select(i => new
                {
                    Type = "Interaction",
                    Date = i.InteractionDate,
                    Description = i.Subject,
                    Status = i.Status,
                    Results = i.Outcome
                })
                .ToListAsync();

            var timeline = visits.Concat(interactions)
                .OrderByDescending(x => x.Date)
                .Take(limit)
                .ToList();

            return timeline;
        }

        public async Task UpdateClientAnalyticsAsync(long clientId, string period)
        {
            var (startDate, endDate) = GetPeriodDates(period);
            
            var visits = await _context.ClientVisits
                .Where(v => v.ClientId == clientId && 
                           v.VisitDate >= startDate && 
                           v.VisitDate <= endDate)
                .ToListAsync();

            var interactions = await _context.ClientInteractions
                .Where(i => i.ClientId == clientId && 
                           i.InteractionDate >= startDate && 
                           i.InteractionDate <= endDate)
                .ToListAsync();

            var analytics = await GetClientAnalyticsAsync(clientId, period);
            if (analytics == null)
            {
                analytics = new ClientAnalytics
                {
                    ClientId = clientId,
                    Period = period,
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };
                await _context.ClientAnalytics.AddAsync(analytics);
            }

            analytics.TotalVisits = visits.Count;
            analytics.TotalInteractions = interactions.Count;
            analytics.AverageVisitDuration = visits.Any() ? (decimal)visits.Count() : 0;
            analytics.LastVisitDate = visits.OrderByDescending(v => v.VisitDate).FirstOrDefault()?.VisitDate;
            analytics.NextScheduledVisit = visits.Where(v => v.Status == "Scheduled").OrderBy(v => v.VisitDate).FirstOrDefault()?.VisitDate;

            await _context.SaveChangesAsync();
        }

        private (DateTime startDate, DateTime endDate) GetPeriodDates(string period)
        {
            var now = DateTime.UtcNow;
            
            return period.ToLower() switch
            {
                "daily" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
                "weekly" => (now.Date.AddDays(-(int)now.DayOfWeek), now.Date.AddDays(7 - (int)now.DayOfWeek).AddTicks(-1)),
                "monthly" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1)),
                "yearly" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31, 23, 59, 59)),
                _ => (now.Date, now.Date.AddDays(1).AddTicks(-1))
            };
        }
    }
}