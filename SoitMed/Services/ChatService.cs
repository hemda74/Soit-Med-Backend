using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Hubs;
using SoitMed.Models;
using SoitMed.Models.Identity;
using SoitMed.Repositories;
using System.Security.Claims;

namespace SoitMed.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ChatService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatService(
            IUnitOfWork unitOfWork,
            IHubContext<ChatHub> hubContext,
            INotificationService notificationService,
            ILogger<ChatService> logger,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _logger = logger;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ChatConversationResponseDTO> GetOrCreateConversationAsync(string customerId, string? adminId, CancellationToken cancellationToken = default)
        {
            // Check if conversation already exists
            var existingConversation = await _unitOfWork.ChatConversations.GetByCustomerIdAsync(customerId, cancellationToken);
            
            if (existingConversation != null)
            {
                return await MapToConversationDTOAsync(existingConversation, cancellationToken);
            }

            // Create new conversation
            var conversation = new ChatConversation
            {
                CustomerId = customerId,
                AdminId = adminId, // Can be null for auto-assignment
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            await _unitOfWork.ChatConversations.CreateAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new chat conversation. ConversationId: {ConversationId}, CustomerId: {CustomerId}", 
                conversation.Id, customerId);

            return await MapToConversationDTOAsync(conversation, cancellationToken);
        }

        public async Task<IEnumerable<ChatConversationResponseDTO>> GetConversationsAsync(string userId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            IEnumerable<ChatConversation> conversations;

            if (isAdmin)
            {
                // Admin can see all conversations or assigned conversations
                conversations = await _unitOfWork.ChatConversations.GetAllActiveConversationsAsync(cancellationToken);
            }
            else
            {
                // Customer can only see their own conversation
                var conversation = await _unitOfWork.ChatConversations.GetByCustomerIdAsync(userId, cancellationToken);
                conversations = conversation != null ? new[] { conversation } : Enumerable.Empty<ChatConversation>();
            }

            var result = new List<ChatConversationResponseDTO>();
            foreach (var conv in conversations)
            {
                var dto = await MapToConversationDTOAsync(conv, cancellationToken);
                result.Add(dto);
            }

            return result.OrderByDescending(c => c.LastMessageAt);
        }

        public async Task<ChatConversationResponseDTO?> GetConversationByIdAsync(long conversationId, string userId, bool isAdmin, CancellationToken cancellationToken = default)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);
            
            if (conversation == null)
                return null;

            // Security check: Customer can only access their own conversation
            if (!isAdmin && conversation.CustomerId != userId)
            {
                throw new UnauthorizedAccessException("You do not have access to this conversation");
            }

            return await MapToConversationDTOAsync(conversation, cancellationToken);
        }

        public async Task<ChatConversationResponseDTO> AssignAdminAsync(long conversationId, string adminId, CancellationToken cancellationToken = default)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);
            if (conversation == null)
                throw new ArgumentException("Conversation not found", nameof(conversationId));

            conversation.AdminId = adminId;
            conversation.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.ChatConversations.UpdateAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Assigned admin to conversation. ConversationId: {ConversationId}, AdminId: {AdminId}", 
                conversationId, adminId);

            // Notify admin via SignalR
            await _hubContext.Clients.Group($"User_{adminId}").SendAsync("ConversationAssigned", new
            {
                ConversationId = conversationId,
                CustomerId = conversation.CustomerId
            });

            return await MapToConversationDTOAsync(conversation, cancellationToken);
        }

        public async Task<ChatMessageResponseDTO> SendTextMessageAsync(long conversationId, string senderId, string content, CancellationToken cancellationToken = default)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);
            if (conversation == null)
                throw new ArgumentException("Conversation not found", nameof(conversationId));

            // Security check
            if (conversation.CustomerId != senderId && conversation.AdminId != senderId)
            {
                throw new UnauthorizedAccessException("You are not authorized to send messages in this conversation");
            }

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                MessageType = "Text",
                Content = content,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ChatMessages.CreateAsync(message, cancellationToken);

            // Update conversation
            conversation.LastMessageAt = DateTime.UtcNow;
            conversation.LastMessagePreview = content.Length > 200 ? content.Substring(0, 200) : content;
            conversation.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ChatConversations.UpdateAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageDTO = await MapToMessageDTOAsync(message, cancellationToken);

            // Determine recipient
            var recipientId = conversation.CustomerId == senderId ? conversation.AdminId : conversation.CustomerId;

            // Send via SignalR
            await _hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("ReceiveMessage", messageDTO);

            // Send notification if recipient is offline
            if (!string.IsNullOrEmpty(recipientId))
            {
                try
                {
                    var sender = await _unitOfWork.Users.GetByIdAsync(senderId, cancellationToken);
                    var senderName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : "Someone";
                    
                    await _notificationService.CreateNotificationAsync(
                        recipientId,
                        "New Chat Message",
                        $"{senderName}: {content}",
                        "Chat",
                        "Medium",
                        null,
                        null,
                        true, // Mobile push
                        new Dictionary<string, object> { { "conversationId", conversationId }, { "messageId", message.Id } },
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send notification for chat message");
                }
            }

            _logger.LogInformation("Text message sent. MessageId: {MessageId}, ConversationId: {ConversationId}", 
                message.Id, conversationId);

            return messageDTO;
        }

        public async Task<ChatMessageResponseDTO> SendVoiceMessageAsync(long conversationId, string senderId, string voiceFilePath, int voiceDuration, CancellationToken cancellationToken = default)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);
            if (conversation == null)
                throw new ArgumentException("Conversation not found", nameof(conversationId));

            // Security check
            if (conversation.CustomerId != senderId && conversation.AdminId != senderId)
            {
                throw new UnauthorizedAccessException("You are not authorized to send messages in this conversation");
            }

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                MessageType = "Voice",
                VoiceFilePath = voiceFilePath,
                VoiceDuration = voiceDuration,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ChatMessages.CreateAsync(message, cancellationToken);

            // Update conversation
            conversation.LastMessageAt = DateTime.UtcNow;
            conversation.LastMessagePreview = "Voice message";
            conversation.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ChatConversations.UpdateAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var messageDTO = await MapToMessageDTOAsync(message, cancellationToken);

            // Determine recipient
            var recipientId = conversation.CustomerId == senderId ? conversation.AdminId : conversation.CustomerId;

            // Send via SignalR
            await _hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("ReceiveMessage", messageDTO);

            // Send notification if recipient is offline
            if (!string.IsNullOrEmpty(recipientId))
            {
                try
                {
                    var sender = await _unitOfWork.Users.GetByIdAsync(senderId, cancellationToken);
                    var senderName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : "Someone";
                    
                    await _notificationService.CreateNotificationAsync(
                        recipientId,
                        "New Voice Message",
                        $"{senderName} sent a voice message",
                        "Chat",
                        "Medium",
                        null,
                        null,
                        true, // Mobile push
                        new Dictionary<string, object> { { "conversationId", conversationId }, { "messageId", message.Id } },
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send notification for voice message");
                }
            }

            _logger.LogInformation("Voice message sent. MessageId: {MessageId}, ConversationId: {ConversationId}", 
                message.Id, conversationId);

            return messageDTO;
        }

        public async Task<IEnumerable<ChatMessageResponseDTO>> GetMessagesAsync(long conversationId, string userId, bool isAdmin, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);
            if (conversation == null)
                throw new ArgumentException("Conversation not found", nameof(conversationId));

            // Security check
            if (!isAdmin && conversation.CustomerId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view messages in this conversation");
            }

            var messages = await _unitOfWork.ChatMessages.GetConversationMessagesAsync(conversationId, page, pageSize, cancellationToken);
            
            var result = new List<ChatMessageResponseDTO>();
            foreach (var message in messages)
            {
                var dto = await MapToMessageDTOAsync(message, cancellationToken);
                result.Add(dto);
            }

            return result;
        }

        public async Task MarkMessagesAsReadAsync(long conversationId, string userId, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.ChatMessages.MarkMessagesAsReadAsync(conversationId, userId, cancellationToken);

            // Notify other participants that messages were read
            await _hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("MessagesRead", new
            {
                ConversationId = conversationId,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

            _logger.LogInformation("Messages marked as read. ConversationId: {ConversationId}, UserId: {UserId}", 
                conversationId, userId);
        }

        public async Task<int> GetUnreadCountAsync(long conversationId, string userId, CancellationToken cancellationToken = default)
        {
            return await _unitOfWork.ChatMessages.GetUnreadCountAsync(conversationId, userId, cancellationToken);
        }

        public async Task DeleteOldMessagesAsync(CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var oldMessages = await _unitOfWork.ChatMessages.GetMessagesOlderThanAsync(cutoffDate, cancellationToken);

            if (oldMessages.Any())
            {
                // Delete voice files
                foreach (var message in oldMessages.Where(m => m.MessageType == "Voice" && !string.IsNullOrEmpty(m.VoiceFilePath)))
                {
                    try
                    {
                        var filePath = Path.Combine(_environment.WebRootPath, message.VoiceFilePath!);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            _logger.LogInformation("Deleted old voice file: {FilePath}", filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete voice file: {FilePath}", message.VoiceFilePath);
                    }
                }

                // Delete messages from database
                await _unitOfWork.ChatMessages.DeleteRangeAsync(oldMessages, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted {Count} old chat messages older than 30 days", oldMessages.Count());
            }
        }

        private async Task<ChatConversationResponseDTO> MapToConversationDTOAsync(ChatConversation conversation, CancellationToken cancellationToken)
        {
            var customer = await _unitOfWork.Users.GetByIdAsync(conversation.CustomerId, cancellationToken);
            ApplicationUser? admin = null;
            if (!string.IsNullOrEmpty(conversation.AdminId))
            {
                admin = await _unitOfWork.Users.GetByIdAsync(conversation.AdminId, cancellationToken);
            }

            // Get unread count (for the current user - we'll need to pass userId, but for now use admin or customer)
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int unreadCount = 0;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                unreadCount = await _unitOfWork.ChatMessages.GetUnreadCountAsync(conversation.Id, currentUserId, cancellationToken);
            }

            return new ChatConversationResponseDTO
            {
                Id = conversation.Id,
                CustomerId = conversation.CustomerId,
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}".Trim() : null,
                CustomerEmail = customer?.Email,
                AdminId = conversation.AdminId,
                AdminName = admin != null ? $"{admin.FirstName} {admin.LastName}".Trim() : null,
                LastMessageAt = conversation.LastMessageAt,
                LastMessagePreview = conversation.LastMessagePreview,
                IsActive = conversation.IsActive,
                UnreadCount = unreadCount,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt
            };
        }

        private async Task<ChatMessageResponseDTO> MapToMessageDTOAsync(ChatMessage message, CancellationToken cancellationToken)
        {
            var sender = await _unitOfWork.Users.GetByIdAsync(message.SenderId, cancellationToken);
            var senderName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : null;

            string? voiceFileUrl = null;
            if (message.MessageType == "Voice" && !string.IsNullOrEmpty(message.VoiceFilePath))
            {
                // Build full URL for voice file
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    voiceFileUrl = $"{request.Scheme}://{request.Host}/{message.VoiceFilePath.Replace('\\', '/')}";
                }
            }

            return new ChatMessageResponseDTO
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderName = senderName,
                MessageType = message.MessageType,
                Content = message.Content,
                VoiceFilePath = message.VoiceFilePath,
                VoiceFileUrl = voiceFileUrl,
                VoiceDuration = message.VoiceDuration,
                IsRead = message.IsRead,
                ReadAt = message.ReadAt,
                CreatedAt = message.CreatedAt
            };
        }
    }
}

