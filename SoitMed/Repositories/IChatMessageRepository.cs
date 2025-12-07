using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IChatMessageRepository : IBaseRepository<ChatMessage>
    {
        Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(long conversationId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task MarkMessagesAsReadAsync(long conversationId, string userId, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(long conversationId, string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatMessage>> GetMessagesOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
    }
}

