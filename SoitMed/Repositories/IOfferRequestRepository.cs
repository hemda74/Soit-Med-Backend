using SoitMed.Models;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Interface for offer request repository
    /// </summary>
    public interface IOfferRequestRepository : IBaseRepository<OfferRequest>
    {
        Task<List<OfferRequest>> GetRequestsByStatusAsync(string status);
        Task<List<OfferRequest>> GetRequestsBySalesManAsync(string salesmanId);
        Task<List<OfferRequest>> GetRequestsAssignedToAsync(string supportId);
        Task<List<OfferRequest>> GetRequestsByClientIdAsync(long clientId);
        Task<List<OfferRequest>> GetRequestsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<OfferRequest>> GetPendingRequestsAsync();
        Task<List<OfferRequest>> GetCompletedRequestsAsync();
        Task<List<OfferRequest>> GetCancelledRequestsAsync();
        Task<OfferRequest?> GetRequestWithDetailsAsync(long requestId);
        Task<List<OfferRequest>> GetRequestsByTaskProgressIdAsync(long taskProgressId);
        Task<int> GetRequestCountByStatusAsync(string status);
        Task<int> GetRequestCountBySalesManAsync(string salesmanId, DateTime? startDate, DateTime? endDate);
        Task<int> GetRequestCountBySupportAsync(string supportId, DateTime? startDate, DateTime? endDate);
        Task<List<OfferRequest>> GetOverdueRequestsAsync();
        Task<List<OfferRequest>> GetRequestsWithOffersAsync();
        Task<List<OfferRequest>> GetRequestsWithoutOffersAsync();
        Task<TimeSpan?> GetAverageProcessingTimeAsync(DateTime? startDate, DateTime? endDate);
        Task<List<OfferRequest>> GetByIdsAsync(IEnumerable<long> ids);
    }
}



