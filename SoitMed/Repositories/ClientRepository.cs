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
            var normalizedQuery = query.ToLower();
            
            return await _context.Clients
                .Where(c => c.Name.ToLower().Contains(normalizedQuery) ||
                           (c.Specialization != null && c.Specialization.ToLower().Contains(normalizedQuery)) ||
                           (c.Location != null && c.Location.ToLower().Contains(normalizedQuery)) ||
                           (c.Phone != null && c.Phone.Contains(query)) ||
                           (c.Email != null && c.Email.ToLower().Contains(normalizedQuery)))
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Client?> FindByNameAsync(string name)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<Client?> FindByNameAndTypeAsync(string name, string type)
        {
            return await _context.Clients
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && 
                                        c.Type.ToLower() == type.ToLower());
        }

        public async Task<IEnumerable<Client>> GetMyClientsAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _context.Clients
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
                .Where(c => c.CreatedBy == userId || c.AssignedTo == userId)
                .ToListAsync();

            var totalClients = await _context.Clients.CountAsync();

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
    }
}