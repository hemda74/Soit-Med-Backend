using SoitMed.Models;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Interface for sales offer repository
    /// </summary>
    public interface ISalesOfferRepository : IBaseRepository<SalesOffer>
    {
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
    }
}



