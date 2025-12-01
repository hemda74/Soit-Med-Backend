using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SoitMed.Hubs
{
    /// <summary>
    /// SignalR hub for real-time chat messaging
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("User {UserId} connected to chat hub", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("User {UserId} disconnected from chat hub", userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a conversation room
        /// </summary>
        public async Task JoinConversation(long conversationId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var groupName = $"Conversation_{conversationId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("User {UserId} joined conversation {ConversationId}", userId, conversationId);
        }

        /// <summary>
        /// Leave a conversation room
        /// </summary>
        public async Task LeaveConversation(long conversationId)
        {
            var userId = GetUserId();
            var groupName = $"Conversation_{conversationId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("User {UserId} left conversation {ConversationId}", userId, conversationId);
        }

        /// <summary>
        /// Send typing indicator
        /// </summary>
        public async Task Typing(long conversationId, bool isTyping)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            await Clients.Group($"Conversation_{conversationId}").SendAsync("UserTyping", new
            {
                ConversationId = conversationId,
                UserId = userId,
                IsTyping = isTyping
            });
        }

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}

