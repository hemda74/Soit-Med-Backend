using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IClientVisitRepository : IBaseRepository<ClientVisit>
    {
        Task<IEnumerable<ClientVisit>> GetClientVisitsAsync(long clientId, DateTime? startDate = null, DateTime? endDate = null, string? salesmanId = null, string? status = null, int page = 1, int pageSize = 20);
        Task<IEnumerable<ClientVisit>> GetSalesmanVisitsAsync(string salesmanId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);
        Task<decimal> GetAverageVisitDurationAsync(long clientId);
        Task<int> GetTotalVisitsCountAsync(long clientId);
        Task<DateTime?> GetLastVisitDateAsync(long clientId);
        Task<DateTime?> GetNextScheduledVisitAsync(long clientId);
    }
}