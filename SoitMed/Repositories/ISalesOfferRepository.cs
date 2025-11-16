using SoitMed.Models;
using SoitMed.Models.Identity;
using System.Linq;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Interface for sales offer repository
    /// </summary>
    public interface ISalesOfferRepository : IBaseRepository<SalesOffer>
    {
        IQueryable<SalesOffer> GetQueryable();
        Task<List<SalesOffer>> GetOffersByClientIdAsync(long clientId);
        Task<List<SalesOffer>> GetOffersBySalesmanAsync(string salesmanId);
        Task<List<SalesOffer>> GetOffersByStatusAsync(string status);
        Task<List<SalesOffer>> GetOffersByCreatorAsync(string creatorId);
        Task<List<SalesOffer>> GetExpiredOffersAsync();
        Task<List<SalesOffer>> GetOffersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<SalesOffer>> GetOffersByOfferRequestAsync(long offerRequestId);
        Task<SalesOffer?> GetOfferWithDetailsAsync(long offerId);
        Task<List<SalesOffer>> GetOffersNeedingFollowUpAsync();
        Task<int> GetOfferCountByStatusAsync(string status);
        Task<decimal> GetTotalOfferValueByStatusAsync(string status);
        Task<List<SalesOffer>> GetByIdsAsync(IEnumerable<long> ids);
        
        // Optimized methods that load related data in a single query
        Task<(List<SalesOffer> Offers, Dictionary<long, Client> Clients, Dictionary<string, ApplicationUser> Users)> 
            GetOffersBySalesmanWithRelatedDataAsync(string salesmanId);
        Task<(List<SalesOffer> Offers, Dictionary<long, Client> Clients, Dictionary<string, ApplicationUser> Users)> 
            GetOffersByCustomerWithRelatedDataAsync(string customerId);
        Task<(List<SalesOffer> Offers, Dictionary<long, Client> Clients, Dictionary<string, ApplicationUser> Users)> 
            GetOffersByClientIdWithRelatedDataAsync(long clientId);
        Task<(List<SalesOffer> Offers, Dictionary<long, Client> Clients, Dictionary<string, ApplicationUser> Users)> 
            GetOffersByStatusWithRelatedDataAsync(string? status);
        Task<(List<SalesOffer> Offers, Dictionary<long, Client> Clients, Dictionary<string, ApplicationUser> Users)> 
            GetExpiredOffersWithRelatedDataAsync();
    }
}



