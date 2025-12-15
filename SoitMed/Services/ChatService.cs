using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SoitMed.DTO;
using SoitMed.Hubs;
using SoitMed.Models;
using SoitMed.Models.Core;
using SoitMed.Models.Enums;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatService(
            IUnitOfWork unitOfWork,
            IHubContext<ChatHub> hubContext,
            INotificationService notificationService,
            ILogger<ChatService> logger,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _logger = logger;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<ChatConversationResponseDTO> GetOrCreateConversationAsync(string customerId, ChatType chatType, string? adminId = null, CancellationToken cancellationToken = default)
        {
            // Check if conversation already exists for this customer and chat type
            var existingConversation = await _unitOfWork.ChatConversations.GetByCustomerIdAndTypeAsync(customerId, chatType, cancellationToken);
            
            if (existingConversation != null)
            {
                return await MapToConversationDTOAsync(existingConversation, cancellationToken);
            }

            // Auto-assign admin based on chat type if not provided
            if (string.IsNullOrEmpty(adminId))
            {
                adminId = await AutoAssignAdminByChatTypeAsync(chatType, cancellationToken);
            }

            // Create new conversation
            var conversation = new ChatConversation
            {
                CustomerId = customerId,
                AdminId = adminId, // Can be null if no support staff available
                ChatType = chatType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            await _unitOfWork.ChatConversations.CreateAsync(conversation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new chat conversation. ConversationId: {ConversationId}, CustomerId: {CustomerId}, ChatType: {ChatType}, AdminId: {AdminId}", 
                conversation.Id, customerId, chatType, adminId ?? "None");

            return await MapToConversationDTOAsync(conversation, cancellationToken);
        }

        /// <summary>
        /// Auto-assigns an admin/support staff based on chat type
        /// </summary>
        private async Task<string?> AutoAssignAdminByChatTypeAsync(ChatType chatType, CancellationToken cancellationToken)
        {
            try
            {
                string targetRole;
                switch (chatType)
                {
                    case ChatType.Support:
                        targetRole = UserRoles.Admin;
                        break;
                    case ChatType.Sales:
                        targetRole = UserRoles.SalesSupport;
                        break;
                    case ChatType.Maintenance:
                        targetRole = UserRoles.MaintenanceSupport;
                        break;
                    default:
                        targetRole = UserRoles.Admin;
                        break;
                }

                // Get all users with the target role
                var supportUsers = await _userManager.GetUsersInRoleAsync(targetRole);
                var activeUsers = supportUsers.Where(u => u.IsActive).ToList();

                if (!activeUsers.Any())
                {
                    _logger.LogWarning("No active {Role} users found for chat type {ChatType}. Conversation will be created without assignment.", 
                        targetRole, chatType);
                    return null;
                }

                // For now, assign to the first active user
                // TODO: Could implement round-robin or load balancing here
                var assignedUser = activeUsers.First();
                _logger.LogInformation("Auto-assigned {Role} user {UserId} ({UserName}) for chat type {ChatType}", 
                    targetRole, assignedUser.Id, assignedUser.UserName, chatType);

                return assignedUser.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-assigning admin for chat type {ChatType}", chatType);
                return null;
            }
        }

        public async Task<IEnumerable<ChatConversationResponseDTO>> GetConversationsAsync(string userId, List<string> userRoles, CancellationToken cancellationToken = default)
        {
            IEnumerable<ChatConversation> conversations;
            var isSuperAdmin = userRoles.Contains(UserRoles.SuperAdmin);
            var isAdmin = userRoles.Contains(UserRoles.Admin);
            var isSalesSupport = userRoles.Contains(UserRoles.SalesSupport);
            var isMaintenanceSupport = userRoles.Contains(UserRoles.MaintenanceSupport);
            var isCustomer = userRoles.Contains("Customer");

            if (isSuperAdmin)
            {
                // SuperAdmin can see ALL conversations regardless of type
                conversations = await _unitOfWork.ChatConversations.GetAllActiveConversationsAsync(cancellationToken);
            }
            else if (isAdmin || isSalesSupport || isMaintenanceSupport)
            {
                // Support staff can see conversations assigned to them or unassigned conversations of their type
                var allConversations = new List<ChatConversation>();

                // Get conversations assigned to this user - FILTERED BY CHAT TYPE based on role
                IEnumerable<ChatConversation> assignedConversations = Enumerable.Empty<ChatConversation>();
                if (isAdmin)
                {
                    // Admin can only see Support type conversations assigned to them
                    assignedConversations = await _unitOfWork.ChatConversations.GetAdminConversationsByTypeAsync(userId, ChatType.Support, cancellationToken);
                }
                else if (isSalesSupport)
                {
                    // SalesSupport can only see Sales type conversations assigned to them
                    assignedConversations = await _unitOfWork.ChatConversations.GetAdminConversationsByTypeAsync(userId, ChatType.Sales, cancellationToken);
                }
                else if (isMaintenanceSupport)
                {
                    // MaintenanceSupport can only see Maintenance type conversations assigned to them
                    assignedConversations = await _unitOfWork.ChatConversations.GetAdminConversationsByTypeAsync(userId, ChatType.Maintenance, cancellationToken);
                }

                // Get unassigned conversations based on role
                if (isAdmin)
                {
                    var unassignedSupport = await _unitOfWork.ChatConversations.GetActiveConversationsByTypeAsync(ChatType.Support, cancellationToken);
                    allConversations.AddRange(unassignedSupport.Where(c => string.IsNullOrEmpty(c.AdminId)));
                }
                if (isSalesSupport)
                {
                    var unassignedSales = await _unitOfWork.ChatConversations.GetActiveConversationsByTypeAsync(ChatType.Sales, cancellationToken);
                    allConversations.AddRange(unassignedSales.Where(c => string.IsNullOrEmpty(c.AdminId)));
                }
                if (isMaintenanceSupport)
                {
                    var unassignedMaintenance = await _unitOfWork.ChatConversations.GetActiveConversationsByTypeAsync(ChatType.Maintenance, cancellationToken);
                    allConversations.AddRange(unassignedMaintenance.Where(c => string.IsNullOrEmpty(c.AdminId)));
                }

                // Combine assigned and unassigned conversations
                allConversations.AddRange(assignedConversations);
                conversations = allConversations.DistinctBy(c => c.Id);
            }
            else if (isCustomer)
            {
                // Customer can see all their conversations (one per chat type)
                var supportConv = await _unitOfWork.ChatConversations.GetByCustomerIdAndTypeAsync(userId, ChatType.Support, cancellationToken);
                var salesConv = await _unitOfWork.ChatConversations.GetByCustomerIdAndTypeAsync(userId, ChatType.Sales, cancellationToken);
                var maintenanceConv = await _unitOfWork.ChatConversations.GetByCustomerIdAndTypeAsync(userId, ChatType.Maintenance, cancellationToken);

                conversations = new[] { supportConv, salesConv, maintenanceConv }
                    .Where(c => c != null)
                    .Cast<ChatConversation>();
            }
            else
            {
                // No conversations for other roles
                conversations = Enumerable.Empty<ChatConversation>();
            }

            var result = new List<ChatConversationResponseDTO>();
            foreach (var conv in conversations)
            {
                var dto = await MapToConversationDTOAsync(conv, cancellationToken);
                result.Add(dto);
            }

            return result.OrderByDescending(c => c.LastMessageAt);
        }

        public async Task<ChatConversationResponseDTO?> GetConversationByIdAsync(long conversationId, string userId, List<string> userRoles, CancellationToken cancellationToken = default)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);
            
            if (conversation == null)
                return null;

            var isSuperAdmin = userRoles.Contains(UserRoles.SuperAdmin);
            var isAdmin = userRoles.Contains(UserRoles.Admin);
            var isSalesSupport = userRoles.Contains(UserRoles.SalesSupport);
            var isMaintenanceSupport = userRoles.Contains(UserRoles.MaintenanceSupport);
            var isCustomer = userRoles.Contains("Customer");

            // Security check
            if (isSuperAdmin)
            {
                // SuperAdmin can access any conversation
            }
            else if (isCustomer)
            {
                // Customer can only access their own conversations
                if (conversation.CustomerId != userId)
                {
                    throw new UnauthorizedAccessException("You do not have access to this conversation");
                }
            }
            else
            {
                // Support staff can only access conversations of their type
                bool hasAccess = false;
                if (isAdmin && conversation.ChatType == ChatType.Support)
                    hasAccess = true;
                else if (isSalesSupport && conversation.ChatType == ChatType.Sales)
                    hasAccess = true;
                else if (isMaintenanceSupport && conversation.ChatType == ChatType.Maintenance)
                    hasAccess = true;

                // Also allow if assigned to them
                if (!hasAccess && conversation.AdminId == userId)
                    hasAccess = true;

                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("You do not have access to this conversation type");
                }
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

        public async Task<ChatMessageResponseDTO> SendTextMessageAsync(long conversationId, string senderId, string content, List<string> userRoles, CancellationToken cancellationToken = default)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);
            if (conversation == null)
                throw new ArgumentException("Conversation not found", nameof(conversationId));

            // Check if sender is customer
            var isCustomer = conversation.CustomerId == senderId;
            var isSuperAdmin = userRoles.Contains(UserRoles.SuperAdmin);
            var isAdmin = userRoles.Contains(UserRoles.Admin);
            var isSalesSupport = userRoles.Contains(UserRoles.SalesSupport);
            var isMaintenanceSupport = userRoles.Contains(UserRoles.MaintenanceSupport);

            // Security check: sender must be either the customer or authorized support staff
            if (!isCustomer)
            {
                bool isAuthorized = false;
                if (isSuperAdmin)
                    isAuthorized = true;
                else if (isAdmin && conversation.ChatType == ChatType.Support)
                    isAuthorized = true;
                else if (isSalesSupport && conversation.ChatType == ChatType.Sales)
                    isAuthorized = true;
                else if (isMaintenanceSupport && conversation.ChatType == ChatType.Maintenance)
                    isAuthorized = true;

                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException("You are not authorized to send messages in this conversation type");
                }
            }

            // If support staff is sending and AdminId is null, assign them to the conversation
            if (!isCustomer && string.IsNullOrEmpty(conversation.AdminId))
            {
                conversation.AdminId = senderId;
                conversation.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.ChatConversations.UpdateAsync(conversation, cancellationToken);
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

        public async Task<ChatMessageResponseDTO> SendVoiceMessageAsync(long conversationId, string senderId, string voiceFilePath, int voiceDuration, List<string> userRoles, CancellationToken cancellationToken = default)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);
            if (conversation == null)
                throw new ArgumentException("Conversation not found", nameof(conversationId));

            // Check if sender is customer
            var isCustomer = conversation.CustomerId == senderId;
            var isSuperAdmin = userRoles.Contains(UserRoles.SuperAdmin);
            var isAdmin = userRoles.Contains(UserRoles.Admin);
            var isSalesSupport = userRoles.Contains(UserRoles.SalesSupport);
            var isMaintenanceSupport = userRoles.Contains(UserRoles.MaintenanceSupport);

            // Security check: sender must be either the customer or authorized support staff
            if (!isCustomer)
            {
                bool isAuthorized = false;
                if (isSuperAdmin)
                    isAuthorized = true;
                else if (isAdmin && conversation.ChatType == ChatType.Support)
                    isAuthorized = true;
                else if (isSalesSupport && conversation.ChatType == ChatType.Sales)
                    isAuthorized = true;
                else if (isMaintenanceSupport && conversation.ChatType == ChatType.Maintenance)
                    isAuthorized = true;

                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException("You are not authorized to send messages in this conversation type");
                }
            }

            // If support staff is sending and AdminId is null, assign them to the conversation
            if (!isCustomer && string.IsNullOrEmpty(conversation.AdminId))
            {
                conversation.AdminId = senderId;
                conversation.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.ChatConversations.UpdateAsync(conversation, cancellationToken);
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

        public async Task<IEnumerable<ChatMessageResponseDTO>> GetMessagesAsync(long conversationId, string userId, List<string> userRoles, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);
            if (conversation == null)
                throw new ArgumentException("Conversation not found", nameof(conversationId));

            var isSuperAdmin = userRoles.Contains(UserRoles.SuperAdmin);
            var isAdmin = userRoles.Contains(UserRoles.Admin);
            var isSalesSupport = userRoles.Contains(UserRoles.SalesSupport);
            var isMaintenanceSupport = userRoles.Contains(UserRoles.MaintenanceSupport);
            var isCustomer = userRoles.Contains("Customer");

            // Security check
            if (isSuperAdmin)
            {
                // SuperAdmin can view any conversation
            }
            else if (isCustomer)
            {
                // Customer can only view their own conversations
                if (conversation.CustomerId != userId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to view messages in this conversation");
                }
            }
            else
            {
                // Support staff can only view conversations of their type
                bool hasAccess = false;
                if (isAdmin && conversation.ChatType == ChatType.Support)
                    hasAccess = true;
                else if (isSalesSupport && conversation.ChatType == ChatType.Sales)
                    hasAccess = true;
                else if (isMaintenanceSupport && conversation.ChatType == ChatType.Maintenance)
                    hasAccess = true;

                // Also allow if assigned to them
                if (!hasAccess && conversation.AdminId == userId)
                    hasAccess = true;

                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("You are not authorized to view messages in this conversation type");
                }
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

            // Get customer image URL
            string? customerImageUrl = null;
            if (customer?.ProfileImage != null && !string.IsNullOrEmpty(customer.ProfileImage.FilePath))
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    customerImageUrl = $"{request.Scheme}://{request.Host}/{customer.ProfileImage.FilePath.Replace('\\', '/')}";
                }
            }

            return new ChatConversationResponseDTO
            {
                Id = conversation.Id,
                CustomerId = conversation.CustomerId,
                CustomerName = customer != null ? $"{customer.FirstName} {customer.LastName}".Trim() : null,
                CustomerFirstName = customer?.FirstName,
                CustomerLastName = customer?.LastName,
                CustomerEmail = customer?.Email,
                CustomerImageUrl = customerImageUrl,
                AdminId = conversation.AdminId,
                AdminName = admin != null ? $"{admin.FirstName} {admin.LastName}".Trim() : null,
                ChatType = conversation.ChatType,
                ChatTypeName = conversation.ChatType switch
                {
                    ChatType.Support => "Support",
                    ChatType.Sales => "Sales",
                    ChatType.Maintenance => "Maintenance",
                    _ => "Support"
                },
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

