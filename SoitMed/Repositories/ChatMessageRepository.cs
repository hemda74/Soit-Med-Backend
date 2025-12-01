using Microsoft.EntityFrameworkCore;
using SoitMed.Models;

namespace SoitMed.Repositories
{
    public class ChatMessageRepository : BaseRepository<ChatMessage>, IChatMessageRepository
    {
        public ChatMessageRepository(Context context) : base(context)
        {
        }

        public async Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(long conversationId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.CreatedAt) // Return in chronological order
                .ToListAsync(cancellationToken);
        }

        public async Task MarkMessagesAsReadAsync(long conversationId, string userId, CancellationToken cancellationToken = default)
        {
            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId 
                    && m.SenderId != userId 
                    && !m.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            if (unreadMessages.Any())
            {
                _context.ChatMessages.UpdateRange(unreadMessages);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<int> GetUnreadCountAsync(long conversationId, string userId, CancellationToken cancellationToken = default)
        {
            return await _context.ChatMessages
                .CountAsync(m => m.ConversationId == conversationId 
                    && m.SenderId != userId 
                    && !m.IsRead, cancellationToken);
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
        {
            return await _context.ChatMessages
                .Where(m => m.CreatedAt < cutoffDate)
                .ToListAsync(cancellationToken);
        }
    }
}

