using SoitMed.Models;

namespace SoitMed.Repositories
{
    public interface IChatConversationRepository : IBaseRepository<ChatConversation>
    {
        Task<ChatConversation?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatConversation>> GetAdminConversationsAsync(string adminId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatConversation>> GetAllActiveConversationsAsync(CancellationToken cancellationToken = default);
        Task<ChatConversation?> GetWithMessagesAsync(long conversationId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task<int> GetUnreadMessageCountAsync(long conversationId, string userId, CancellationToken cancellationToken = default);
    }
}

