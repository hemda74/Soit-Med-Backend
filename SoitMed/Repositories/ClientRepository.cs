using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        public ClientRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<Client>> SearchClientsAsync(string query, string? classification = null, int page = 1, int pageSize = 20)
        {
            var baseQuery = _context.Clients
                .AsNoTracking()
                .AsQueryable();
            
            // If query is provided, filter by it
            // Search in Name, Phone, and OrganizationName only
            if (!string.IsNullOrWhiteSpace(query))
            {
                // Use EF.Functions.Like for case-insensitive search that can use indexes
                baseQuery = baseQuery.Where(c => 
                    EF.Functions.Like(c.Name, $"%{query}%") ||
                    (c.Phone != null && EF.Functions.Like(c.Phone, $"%{query}%")) ||
                    (c.OrganizationName != null && EF.Functions.Like(c.OrganizationName, $"%{query}%")));
            }
            
            // Filter by classification if provided
            if (!string.IsNullOrWhiteSpace(classification))
            {
                baseQuery = baseQuery.Where(c => c.Classification == classification);
            }
            
            // Apply pagination
            return await baseQuery
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Client?> FindByNameAsync(string name)
        {
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, name));
        }

        public async Task<Client?> FindByNameAndOrganizationAsync(string name, string? organizationName)
        {
            if (string.IsNullOrWhiteSpace(organizationName))
            {
                return await FindByNameAsync(name);
            }
            
            return await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, name) && 
                                        c.OrganizationName != null &&
                                        EF.Functions.Like(c.OrganizationName, organizationName));
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

        // Note: GetClientsNeedingFollowUpAsync removed - no longer needed without NextContactDate and Status fields

        public async Task<Client?> FindOrCreateClientAsync(string name, string? organizationName, string? phone, string createdBy)
        {
            // First try to find existing client by name
            var existingClient = await FindByNameAsync(name);
            if (existingClient != null)
            {
                return existingClient;
            }

            // Create new client if not found
            var newClient = new Client
            {
                Name = name,
                OrganizationName = organizationName,
                Phone = phone,
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

            var clientsByOrganization = myClients
                .Where(c => !string.IsNullOrEmpty(c.OrganizationName))
                .GroupBy(c => c.OrganizationName)
                .Select(g => new { OrganizationName = g.Key, Count = g.Count() })
                .ToList();

            return new
            {
                MyClientsCount = myClients.Count,
                TotalClientsCount = totalClients,
                ClientsByOrganization = clientsByOrganization
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