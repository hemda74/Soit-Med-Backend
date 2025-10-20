using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ClientInteractionRepository : BaseRepository<ClientInteraction>, IClientInteractionRepository
    {
        public ClientInteractionRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<ClientInteraction>> GetClientInteractionsAsync(long clientId, int page = 1, int pageSize = 20)
        {
            return await _context.ClientInteractions
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.InteractionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientInteraction>> GetInteractionsByTypeAsync(long clientId, string interactionType, int page = 1, int pageSize = 20)
        {
            return await _context.ClientInteractions
                .Where(i => i.ClientId == clientId && i.InteractionType == interactionType)
                .OrderByDescending(i => i.InteractionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<ClientInteraction>> GetPendingFollowUpsAsync(long clientId)
        {
            var today = DateTime.UtcNow.Date;
            
            return await _context.ClientInteractions
                .Where(i => i.ClientId == clientId && 
                           i.FollowUpRequired && 
                           i.FollowUpDate.HasValue && 
                           i.FollowUpDate.Value.Date <= today &&
                           i.Status == "Open")
                .OrderBy(i => i.FollowUpDate)
                .ToListAsync();
        }

        public async Task<decimal> GetAverageInteractionDurationAsync(long clientId)
        {
            var interactions = await _context.ClientInteractions
                .Where(i => i.ClientId == clientId && i.Status == "Closed")
                .ToListAsync();

            return interactions.Any() ? (decimal)interactions.Count() : 0;
        }

        public async Task<int> GetTotalInteractionsCountAsync(long clientId)
        {
            return await _context.ClientInteractions
                .CountAsync(i => i.ClientId == clientId);
        }

        public async Task<DateTime?> GetLastInteractionDateAsync(long clientId)
        {
            var lastInteraction = await _context.ClientInteractions
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.InteractionDate)
                .FirstOrDefaultAsync();

            return lastInteraction?.InteractionDate;
        }
    }
}