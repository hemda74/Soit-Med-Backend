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
        private readonly IMobileNotificationService _mobileNotificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService, 
            IMobileNotificationService mobileNotificationService,
            ILogger<NotificationController> logger, 
            UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _notificationService = notificationService;
            _mobileNotificationService = mobileNotificationService;
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

        /// <summary>
        /// Register push notification token for the current user
        /// </summary>
        [HttpPost("register-push-token")]
        public async Task<IActionResult> RegisterPushToken([FromBody] RegisterPushTokenDTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(dto.Token))
                {
                    return ErrorResponse("Push token is required", 400);
                }

                var platform = dto.Platform?.ToLower() ?? "android";
                if (platform != "android" && platform != "ios")
                {
                    return ErrorResponse("Platform must be 'android' or 'ios'", 400);
                }

                await _mobileNotificationService.RegisterDeviceTokenAsync(userId, dto.Token, platform, cancellationToken);
                _logger.LogInformation("  Push token registered for user {UserId}", userId);
                
                return SuccessResponse("Push token registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering push token");
                return ErrorResponse("An error occurred while registering push token", 500);
            }
        }

        /// <summary>
        /// Unregister push notification token for the current user
        /// </summary>
        [HttpPost("unregister-push-token")]
        public async Task<IActionResult> UnregisterPushToken([FromBody] UnregisterPushTokenDTO dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(dto.Token))
                {
                    return ErrorResponse("Push token is required", 400);
                }

                await _mobileNotificationService.UnregisterDeviceTokenAsync(userId, dto.Token, cancellationToken);
                _logger.LogInformation("  Push token unregistered for user {UserId}", userId);
                
                return SuccessResponse("Push token unregistered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering push token");
                return ErrorResponse("An error occurred while unregistering push token", 500);
            }
        }
    }

    public class RegisterPushTokenDTO
    {
        public string Token { get; set; } = string.Empty;
        public string? Platform { get; set; } = "android";
    }

    public class UnregisterPushTokenDTO
    {
        public string Token { get; set; } = string.Empty;
    }
}
