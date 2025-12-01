using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ChatConversationRepository : BaseRepository<ChatConversation>, IChatConversationRepository
    {
        public ChatConversationRepository(Context context) : base(context)
        {
        }

        public async Task<ChatConversation?> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatConversations
                .Include(c => c.Customer)
                .Include(c => c.Admin)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<ChatConversation>> GetAdminConversationsAsync(string adminId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatConversations
                .Include(c => c.Customer)
                .Include(c => c.Admin)
                .Where(c => c.AdminId == adminId && c.IsActive)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ChatConversation>> GetAllActiveConversationsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ChatConversations
                .Include(c => c.Customer)
                .Include(c => c.Admin)
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<ChatConversation?> GetWithMessagesAsync(long conversationId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            return await _context.ChatConversations
                .Include(c => c.Customer)
                .Include(c => c.Admin)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize))
                .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
        }

        public async Task<int> GetUnreadMessageCountAsync(long conversationId, string userId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatMessages
                .CountAsync(m => m.ConversationId == conversationId 
                    && m.SenderId != userId 
                    && !m.IsRead, cancellationToken);
        }
    }
}

