using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IClientInteractionRepository : IBaseRepository<ClientInteraction>
    {
        Task<IEnumerable<ClientInteraction>> GetClientInteractionsAsync(long clientId, int page = 1, int pageSize = 20);
        Task<IEnumerable<ClientInteraction>> GetInteractionsByTypeAsync(long clientId, string interactionType, int page = 1, int pageSize = 20);
        Task<IEnumerable<ClientInteraction>> GetPendingFollowUpsAsync(long clientId);
        Task<decimal> GetAverageInteractionDurationAsync(long clientId);
        Task<int> GetTotalInteractionsCountAsync(long clientId);
        Task<DateTime?> GetLastInteractionDateAsync(long clientId);
    }
}