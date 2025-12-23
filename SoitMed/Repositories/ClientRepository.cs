using Microsoft.EntityFrameworkCore;
using SoitMed.Models;
using SoitMed.Models.Location;

namespace SoitMed.Repositories
{
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        public ClientRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<Client>> SearchClientsAsync(SoitMed.DTO.SearchClientDTO searchDto)
        {
            var baseQuery = _context.Clients
                .AsNoTracking()
                .AsQueryable();
            
            // If query is provided, filter by it
            if (!string.IsNullOrWhiteSpace(searchDto.Query))
            {
                var query = searchDto.Query;
                var pattern = "%" + query + "%";
                baseQuery = baseQuery.Where(c => 
                    EF.Functions.Like(c.Name, pattern) ||
                    (c.Specialization != null && EF.Functions.Like(c.Specialization, pattern)) ||
                    (c.Location != null && EF.Functions.Like(c.Location, pattern)) ||
                    (c.Phone != null && EF.Functions.Like(c.Phone, pattern)) ||
                    (c.Email != null && EF.Functions.Like(c.Email, pattern)));
            }

            // Filter by classification
            if (!string.IsNullOrWhiteSpace(searchDto.Classification))
            {
                baseQuery = baseQuery.Where(c => c.Classification == searchDto.Classification);
            }

            // Filter by assigned salesman
            if (!string.IsNullOrWhiteSpace(searchDto.AssignedSalesManId))
            {
                baseQuery = baseQuery.Where(c => c.AssignedTo == searchDto.AssignedSalesManId);
            }

            // Filter by city
            if (!string.IsNullOrWhiteSpace(searchDto.City))
            {
                var cityPattern = "%" + searchDto.City + "%";
                baseQuery = baseQuery.Where(c => c.City != null && EF.Functions.Like(c.City, cityPattern));
            }

            // Filter by governorate - need to look up governorate name from ID
            if (searchDto.GovernorateId.HasValue)
            {
                // Look up the governorate name from the Governorates table first
                var governorate = await _context.Governorates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GovernorateId == searchDto.GovernorateId.Value);
                
                if (governorate != null && !string.IsNullOrWhiteSpace(governorate.Name))
                {
                    var govPattern = "%" + governorate.Name + "%";
                    baseQuery = baseQuery.Where(c => c.Governorate != null && 
                        EF.Functions.Like(c.Governorate, govPattern));
                }
            }

            // Filter by equipment categories
            if (searchDto.EquipmentCategories != null && searchDto.EquipmentCategories.Any())
            {
                // InterestedEquipmentCategories is stored as JSON array string like ["Category1", "Category2"]
                // Use a simple approach: check if JSON contains any of the category names
                // Pre-process categories to avoid string operations in LINQ
                var categoryList = searchDto.EquipmentCategories.ToList();
                
                // Build a simple OR condition for the first category
                var firstCat = categoryList[0];
                var firstCatSpace = firstCat.Replace("+", " ");
                var firstCatPlus = firstCat.Replace(" ", "+");
                var firstQuoted = "\"" + firstCat + "\"";
                var firstQuotedSpace = "\"" + firstCatSpace + "\"";
                var firstQuotedPlus = "\"" + firstCatPlus + "\"";
                
                baseQuery = baseQuery.Where(c => 
                    !string.IsNullOrEmpty(c.InterestedEquipmentCategories) &&
                    (c.InterestedEquipmentCategories.Contains(firstQuoted) ||
                     c.InterestedEquipmentCategories.Contains(firstQuotedSpace) ||
                     c.InterestedEquipmentCategories.Contains(firstQuotedPlus) ||
                     c.InterestedEquipmentCategories.Contains(firstCat) ||
                     c.InterestedEquipmentCategories.Contains(firstCatSpace) ||
                     c.InterestedEquipmentCategories.Contains(firstCatPlus))
                );
                
                // Add remaining categories using OR
                for (int i = 1; i < categoryList.Count; i++)
                {
                    var cat = categoryList[i];
                    var catSpace = cat.Replace("+", " ");
                    var catPlus = cat.Replace(" ", "+");
                    var quoted = "\"" + cat + "\"";
                    var quotedSpace = "\"" + catSpace + "\"";
                    var quotedPlus = "\"" + catPlus + "\"";
                    
                    baseQuery = baseQuery.Where(c => 
                        !string.IsNullOrEmpty(c.InterestedEquipmentCategories) &&
                        (c.InterestedEquipmentCategories.Contains(firstQuoted) ||
                         c.InterestedEquipmentCategories.Contains(firstQuotedSpace) ||
                         c.InterestedEquipmentCategories.Contains(firstQuotedPlus) ||
                         c.InterestedEquipmentCategories.Contains(firstCat) ||
                         c.InterestedEquipmentCategories.Contains(firstCatSpace) ||
                         c.InterestedEquipmentCategories.Contains(firstCatPlus) ||
                         c.InterestedEquipmentCategories.Contains(quoted) ||
                         c.InterestedEquipmentCategories.Contains(quotedSpace) ||
                         c.InterestedEquipmentCategories.Contains(quotedPlus) ||
                         c.InterestedEquipmentCategories.Contains(cat) ||
                         c.InterestedEquipmentCategories.Contains(catSpace) ||
                         c.InterestedEquipmentCategories.Contains(catPlus))
                    );
                }
            }
            
            // Apply pagination
            return await baseQuery
                .OrderBy(c => c.Name)
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToListAsync();
        }

        public async Task<Client?> FindByNameAsync(string name)
        {
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, name));
        }

        public async Task<Client?> FindByNameAndTypeAsync(string name, string type)
        {
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, name) && 
                                        EF.Functions.Like(c.Type, type));
        }

        public async Task<IEnumerable<Client>> GetMyClientsAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _context.Clients
                .AsNoTracking()
                .Where(c => c.CreatedBy == userId || c.AssignedTo == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Client>> GetClientsNeedingFollowUpAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            
            return await _context.Clients
                .AsNoTracking()
                .Where(c => (c.CreatedBy == userId || c.AssignedTo == userId) &&
                           c.NextContactDate.HasValue &&
                           c.NextContactDate.Value.Date <= today &&
                           c.Status == "Active")
                .OrderBy(c => c.NextContactDate)
                .ToListAsync();
        }

        public async Task<Client?> FindOrCreateClientAsync(string name, string type, string? specialization, string createdBy)
        {
            // First try to find existing client
            var existingClient = await FindByNameAndTypeAsync(name, type);
            if (existingClient != null)
            {
                return existingClient;
            }

            // Create new client if not found
            var newClient = new Client
            {
                Name = name,
                Type = type,
                Specialization = specialization,
                Status = "Potential",
                Priority = "Medium",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Clients.AddAsync(newClient);
            await _context.SaveChangesAsync();
            
            return newClient;
        }

        public async Task<object> GetClientStatisticsAsync(string userId)
        {
            var myClients = await _context.Clients
                .AsNoTracking()
                .Where(c => c.CreatedBy == userId || c.AssignedTo == userId)
                .ToListAsync();

            var totalClients = await _context.Clients
                .AsNoTracking()
                .CountAsync();

            var clientsByType = myClients
                .GroupBy(c => c.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            var clientsByStatus = myClients
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var clientsByPriority = myClients
                .GroupBy(c => c.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToList();

            return new
            {
                MyClientsCount = myClients.Count,
                TotalClientsCount = totalClients,
                ClientsByType = clientsByType,
                ClientsByStatus = clientsByStatus,
                ClientsByPriority = clientsByPriority
            };
        }

        public async Task<List<Client>> GetByIdsAsync(IEnumerable<long> ids)
        {
            var idList = ids.ToList();
            if (!idList.Any())
                return new List<Client>();

            // Safety limit to prevent memory issues and connection pool exhaustion
            const int maxBatchSize = 1000;
            if (idList.Count > maxBatchSize)
            {
                idList = idList.Take(maxBatchSize).ToList();
            }

            return await _context.Clients
                .AsNoTracking()
                .Where(c => idList.Contains(c.Id))
                .ToListAsync();
        }
    }
}