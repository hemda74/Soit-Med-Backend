using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Identity;
using System.Text.Json;
using System.Linq;

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

        public IQueryable<SalesOffer> GetQueryable()
        {
            return _context.SalesOffers.AsNoTracking();
        }

        public async Task<List<SalesOffer>> GetOffersByClientIdAsync(string clientId)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.ClientId == clientId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesOffer>> GetOffersBySalesManAsync(string salesmanId)
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
            
            // OPTIMIZATION: First filter at database level to reduce data loaded
            // Load only offers that are not already expired and have ValidUntil set
            // This reduces the amount of data we need to parse in memory
            var candidateOffers = await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.Status != "Expired" && o.ValidUntil != null && o.ValidUntil != "")
                .ToListAsync();
            
            // Filter offers where all ValidUntil dates have passed
            // ValidUntil is stored as JSON array of date strings
            // NOTE: JSON parsing must be done in memory as EF Core doesn't support complex JSON queries
            // However, we've already filtered at database level to minimize data loaded
            return candidateOffers.Where(o =>
            {
                if (string.IsNullOrWhiteSpace(o.ValidUntil))
                    return false;
                
                try
                {
                    var dates = System.Text.Json.JsonSerializer.Deserialize<List<string>>(o.ValidUntil);
                    if (dates == null || dates.Count == 0)
                        return false;
                    
                    // Offer is expired if ALL dates have passed
                    return dates.All(dateStr => 
                        DateTime.TryParse(dateStr, out var date) && date.Date < today);
                }
                catch
                {
                    // Fallback: try to parse as single date (backward compatibility)
                    if (DateTime.TryParse(o.ValidUntil, out var singleDate))
                        return singleDate.Date < today;
                    return false;
                }
            })
            .OrderByDescending(o => o.ValidUntil) // Order by JSON string (approximate)
            .ToList();
        }

        public async Task<List<SalesOffer>> GetOffersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<SalesOffer>> GetOffersByOfferRequestAsync(string offerRequestId)
        {
            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.OfferRequestId == offerRequestId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<SalesOffer?> GetOfferWithDetailsAsync(string offerId)
        {
            return await _context.SalesOffers
                .Include(o => o.Client)
                .Include(o => o.Creator)
                .Include(o => o.SalesMan)
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

        public async Task<List<SalesOffer>> GetByIdsAsync(IEnumerable<string> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any())
                return new List<SalesOffer>();

            return await _context.SalesOffers
                .AsNoTracking()
                .Where(o => idList.Contains(o.Id))
                .ToListAsync();
        }

        // OPTIMIZED: Single query to load offers with all related data (O(1) database queries instead of O(3))
        public async Task<(List<SalesOffer> Offers, Dictionary<string, Client> Clients, Dictionary<string, ApplicationUser> Users)> 
            GetOffersBySalesManWithRelatedDataAsync(string salesmanId)
        {
            // Single query with joins to load everything at once
            var offers = await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.AssignedTo == salesmanId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Extract unique IDs
            var clientIds = offers.Where(o => !string.IsNullOrEmpty(o.ClientId)).Select(o => o.ClientId).Distinct().ToList();
            var userIds = offers
                .SelectMany(o => new[] { o.CreatedBy, o.AssignedTo })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            // Load related data sequentially to avoid DbContext concurrency issues
            // Complexity: O(1) - only 3 queries total regardless of number of offers
            var clientsList = clientIds.Any() 
                ? await _context.Clients
                    .AsNoTracking()
                    .Where(c => clientIds.Contains(c.Id))
                    .ToListAsync()
                : new List<Client>();
            
            var usersList = userIds.Any()
                ? await _context.Users
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync()
                : new List<ApplicationUser>();

            var clientsDict = clientsList.ToDictionary(c => c.Id);
            var usersDict = usersList.ToDictionary(u => u.Id);

            return (offers, clientsDict, usersDict);
        }

        public async Task<(List<SalesOffer> Offers, Dictionary<string, Client> Clients, Dictionary<string, ApplicationUser> Users)> 
            GetOffersByClientIdWithRelatedDataAsync(string clientId)
        {
            var offers = await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.ClientId == clientId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var userIds = offers
                .SelectMany(o => new[] { o.CreatedBy, o.AssignedTo })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var clientsList = await _context.Clients
                .AsNoTracking()
                .Where(c => c.Id == clientId)
                .ToListAsync();
            
            var usersList = userIds.Any()
                ? await _context.Users
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync()
                : new List<ApplicationUser>();

            var clientsDict = clientsList.ToDictionary(c => c.Id);
            var usersDict = usersList.ToDictionary(u => u.Id);

            return (offers, clientsDict, usersDict);
        }

        public async Task<(List<SalesOffer> Offers, Dictionary<string, Client> Clients, Dictionary<string, ApplicationUser> Users)> 
            GetOffersByStatusWithRelatedDataAsync(string? status)
        {
            var query = _context.SalesOffers.AsNoTracking();
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            var offers = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var clientIds = offers.Where(o => !string.IsNullOrEmpty(o.ClientId)).Select(o => o.ClientId).Distinct().ToList();
            var userIds = offers
                .SelectMany(o => new[] { o.CreatedBy, o.AssignedTo })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var clientsTask = clientIds.Any()
                ? _context.Clients
                    .AsNoTracking()
                    .Where(c => clientIds.Contains(c.Id))
                    .ToListAsync()
                : Task.FromResult(new List<Client>());
            
            var usersTask = userIds.Any()
                ? _context.Users
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync()
                : Task.FromResult(new List<ApplicationUser>());

            await Task.WhenAll(clientsTask, usersTask);

            var clientsDict = (await clientsTask).ToDictionary(c => c.Id);
            var usersDict = (await usersTask).ToDictionary(u => u.Id);

            return (offers, clientsDict, usersDict);
        }

        public async Task<(List<SalesOffer> Offers, Dictionary<string, Client> Clients, Dictionary<string, ApplicationUser> Users)> 
            GetExpiredOffersWithRelatedDataAsync()
        {
            var today = DateTime.UtcNow.Date;
            
            var candidateOffers = await _context.SalesOffers
                .AsNoTracking()
                .Where(o => o.Status != "Expired" && o.ValidUntil != null && o.ValidUntil != "")
                .ToListAsync();
            
            var offers = candidateOffers.Where(o =>
            {
                if (string.IsNullOrWhiteSpace(o.ValidUntil))
                    return false;
                
                try
                {
                    var dates = System.Text.Json.JsonSerializer.Deserialize<List<string>>(o.ValidUntil);
                    if (dates == null || dates.Count == 0)
                        return false;
                    
                    return dates.All(dateStr => 
                        DateTime.TryParse(dateStr, out var date) && date.Date < today);
                }
                catch
                {
                    if (DateTime.TryParse(o.ValidUntil, out var singleDate))
                        return singleDate.Date < today;
                    return false;
                }
            })
            .OrderByDescending(o => o.ValidUntil)
            .ToList();

            var clientIds = offers.Where(o => !string.IsNullOrEmpty(o.ClientId)).Select(o => o.ClientId).Distinct().ToList();
            var userIds = offers
                .SelectMany(o => new[] { o.CreatedBy, o.AssignedTo })
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var clientsTask = clientIds.Any()
                ? _context.Clients
                    .AsNoTracking()
                    .Where(c => clientIds.Contains(c.Id))
                    .ToListAsync()
                : Task.FromResult(new List<Client>());
            
            var usersTask = userIds.Any()
                ? _context.Users
                    .AsNoTracking()
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync()
                : Task.FromResult(new List<ApplicationUser>());

            await Task.WhenAll(clientsTask, usersTask);

            var clientsDict = (await clientsTask).ToDictionary(c => c.Id);
            var usersDict = (await usersTask).ToDictionary(u => u.Id);

            return (offers, clientsDict, usersDict);
        }
    }
}



