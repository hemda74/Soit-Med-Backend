using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger, UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get user notifications
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool unreadOnly = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize, unreadOnly, cancellationToken);
                return SuccessResponse(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return ErrorResponse("An error occurred while retrieving notifications", 500);
            }
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var count = await _notificationService.GetUnreadNotificationCountAsync(userId, cancellationToken);
                return SuccessResponse(new { UnreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notification count");
                return ErrorResponse("An error occurred while retrieving unread notification count", 500);
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(long notificationId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _notificationService.MarkNotificationAsReadAsync(notificationId, userId, cancellationToken);
                return SuccessResponse("Notification marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return ErrorResponse("An error occurred while marking notification as read", 500);
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                await _notificationService.MarkAllNotificationsAsReadAsync(userId, cancellationToken);
                return SuccessResponse("All notifications marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return ErrorResponse("An error occurred while marking all notifications as read", 500);
            }
        }
    }
}
