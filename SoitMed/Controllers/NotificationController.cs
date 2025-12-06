using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
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

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger,
            Microsoft.AspNetCore.Identity.UserManager<Models.Identity.ApplicationUser> userManager)
            : base(userManager)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all notifications for the current user (base endpoint for web app)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] bool unreadOnly = false)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize, unreadOnly);
                
                return Ok(ResponseHelper.CreateSuccessResponse(notifications));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notifications");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving notifications"));
            }
        }

        /// <summary>
        /// Get all notifications for the current user (mobile app endpoint)
        /// </summary>
        [HttpGet("my-notifications")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] bool unreadOnly = false)
        {
            try
            {
                var userId = GetCurrentUserId();
                var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize, unreadOnly);
                
                return Ok(ResponseHelper.CreateSuccessResponse(notifications));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notifications");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving notifications"));
            }
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
                
                return Ok(ResponseHelper.CreateSuccessResponse(count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving unread count"));
            }
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPost("{id}/mark-read")]
        [HttpPut("{id}/read")] // Web app uses PUT with /read endpoint
        public async Task<IActionResult> MarkAsRead(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.MarkNotificationAsReadAsync(id, userId);
                
                return Ok(ResponseHelper.CreateSuccessResponse(null, "Notification marked as read"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read. NotificationId: {NotificationId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while marking notification as read"));
            }
        }

        /// <summary>
        /// Mark all notifications as read for the current user
        /// </summary>
        [HttpPost("mark-all-read")]
        [HttpPut("mark-all-read")] // Support both POST and PUT
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                
                return Ok(ResponseHelper.CreateSuccessResponse(null, "All notifications marked as read"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while marking all notifications as read"));
            }
        }
    }
}
