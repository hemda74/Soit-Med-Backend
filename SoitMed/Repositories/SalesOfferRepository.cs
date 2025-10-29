using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    /// <summary>
    /// Repository for managing sales offers
    /// </summary>
    public class SalesOfferRepository : BaseRepository<SalesOffer>, ISalesOfferRepository
    {
        public SalesOfferRepository(Context context) : base(context)
        {
        }

        public async Task<List<SalesOffer>> GetOffersByClientIdAsync(long clientId)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.ClientId == clientId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesOffer>> GetOffersBySalesmanAsync(string salesmanId)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.AssignedTo == salesmanId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesOffer>> GetOffersByStatusAsync(string status)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesOffer>> GetOffersByCreatorAsync(string creatorId)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.CreatedBy == creatorId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesOffer>> GetExpiredOffersAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.ValidUntil < today && o.Status != "Expired")
                .OrderByDescending(o => o.ValidUntil)
                .ToListAsync();
        }

        public async Task<List<SalesOffer>> GetOffersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesOffer>> GetOffersByOfferRequestAsync(long offerRequestId)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.OfferRequestId == offerRequestId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<SalesOffer?> GetOfferWithDetailsAsync(long offerId)
        {
            return await _context.SalesOffers
                .Include(o => o.Client)
                .Include(o => o.Creator)
                .Include(o => o.Salesman)
                .Include(o => o.OfferRequest)
                .FirstOrDefaultAsync(o => o.Id == offerId);
        }

        public async Task<List<SalesOffer>> GetOffersNeedingFollowUpAsync()
        {
            var today = DateTime.UtcNow.Date;
            var followUpDate = today.AddDays(-7); // Follow up after 7 days

            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.SentToClientAt.HasValue && 
                           o.SentToClientAt.Value.Date <= followUpDate &&
                           o.Status == "Sent")
                .OrderBy(o => o.SentToClientAt)
                .ToListAsync();
        }

        public async Task<int> GetOfferCountByStatusAsync(string status)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .CountAsync(o => o.Status == status);
        }

        public async Task<decimal> GetTotalOfferValueByStatusAsync(string status)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.Status == status)
                .SumAsync(o => o.TotalAmount);
        }
    }
}



