using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        public ClientRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<Client>> SearchClientsAsync(string query, int page = 1, int pageSize = 20)
        {
            var baseQuery = _context.Clients
                .AsNoTracking()
                .AsQueryable();
            
            // If query is provided, filter by it
            // Note: After migration adds computed columns, use NameLower and EmailLower
            // For now, using EF.Functions.Like for better index usage
            if (!string.IsNullOrWhiteSpace(query))
            {
                var normalizedQuery = query.ToLower();
                // Use EF.Functions.Like for case-insensitive search that can use indexes
                baseQuery = baseQuery.Where(c => 
                    EF.Functions.Like(c.Name, $"%{query}%") ||
                    (c.Specialization != null && EF.Functions.Like(c.Specialization, $"%{query}%")) ||
                    (c.Location != null && EF.Functions.Like(c.Location, $"%{query}%")) ||
                    (c.Phone != null && EF.Functions.Like(c.Phone, $"%{query}%")) ||
                    (c.Email != null && EF.Functions.Like(c.Email, $"%{query}%")));
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