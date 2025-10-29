using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SoitMed.Hubs
{
    /// <summary>
    /// SignalR hub for real-time notifications
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group for targeted notifications
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                
                // Add user to role-based groups for broadcast notifications
                var userRole = GetUserRole();
                if (!string.IsNullOrEmpty(userRole))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{userRole}");
                }

                _logger.LogInformation($"User {userId} connected to notification hub");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userId} disconnected from notification hub");
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a specific group for targeted notifications
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"User {GetUserId()} joined group {groupName}");
        }

        /// <summary>
        /// Leave a specific group
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"User {GetUserId()} left group {groupName}");
        }

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string? GetUserRole()
        {
            return Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
