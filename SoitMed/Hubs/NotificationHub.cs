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
                _logger.LogInformation("  User {UserId} added to personal group: User_{UserId}", userId, userId);
                
                // Add user to ALL their role-based groups for broadcast notifications
                var userRoles = GetUserRoles();
                if (userRoles != null && userRoles.Any())
                {
                    foreach (var role in userRoles)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
                        _logger.LogInformation("  User {UserId} added to role group: Role_{Role}", userId, role);
                    }
                }

                _logger.LogInformation("  User {UserId} connected to notification hub with roles: {Roles}", 
                    userId, userRoles != null ? string.Join(", ", userRoles) : "None");
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

        private List<string>? GetUserRoles()
        {
            return Context.User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        }
    }
}
