using SoitMed.DTO;

namespace SoitMed.Services
{
    public interface IChatService
    {
        Task<ChatConversationResponseDTO> GetOrCreateConversationAsync(string customerId, string? adminId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatConversationResponseDTO>> GetConversationsAsync(string userId, bool isAdmin, CancellationToken cancellationToken = default);
        Task<ChatConversationResponseDTO?> GetConversationByIdAsync(long conversationId, string userId, bool isAdmin, CancellationToken cancellationToken = default);
        Task<ChatConversationResponseDTO> AssignAdminAsync(long conversationId, string adminId, CancellationToken cancellationToken = default);
        Task<ChatMessageResponseDTO> SendTextMessageAsync(long conversationId, string senderId, string content, CancellationToken cancellationToken = default);
        Task<ChatMessageResponseDTO> SendVoiceMessageAsync(long conversationId, string senderId, string voiceFilePath, int voiceDuration, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatMessageResponseDTO>> GetMessagesAsync(long conversationId, string userId, bool isAdmin, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task MarkMessagesAsReadAsync(long conversationId, string userId, CancellationToken cancellationToken = default);
        Task<int> GetUnreadCountAsync(long conversationId, string userId, CancellationToken cancellationToken = default);
        Task DeleteOldMessagesAsync(CancellationToken cancellationToken = default);
    }
}

