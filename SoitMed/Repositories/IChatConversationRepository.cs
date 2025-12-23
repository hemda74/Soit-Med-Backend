using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.Repositories
{
    public interface IChatConversationRepository : IBaseRepository<ChatConversation>
    {
        Task<ChatConversation?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
        Task<ChatConversation?> GetByCustomerIdAndTypeAsync(string customerId, ChatType chatType, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatConversation>> GetAdminConversationsAsync(string AdminId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatConversation>> GetAdminConversationsByTypeAsync(string AdminId, ChatType chatType, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatConversation>> GetAllActiveConversationsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatConversation>> GetActiveConversationsByTypeAsync(ChatType chatType, CancellationToken cancellationToken = default);
        Task<ChatConversation?> GetWithMessagesAsync(long conversationId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task<int> GetUnreadMessageCountAsync(long conversationId, string userId, CancellationToken cancellationToken = default);
    }
}

