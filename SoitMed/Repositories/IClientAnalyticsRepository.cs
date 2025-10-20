using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IClientAnalyticsRepository : IBaseRepository<ClientAnalytics>
    {
        Task<ClientAnalytics?> GetClientAnalyticsAsync(long clientId, string period);
        Task<IEnumerable<ClientAnalytics>> GetClientAnalyticsHistoryAsync(long clientId, int page = 1, int pageSize = 20);
        Task<object?> GetClientSummaryAsync(long clientId);
        Task<object> GetClientTimelineAsync(long clientId, int limit = 50);
        Task UpdateClientAnalyticsAsync(long clientId, string period);
    }
}