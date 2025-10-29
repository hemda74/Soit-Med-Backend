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
    public class ActivitiesController : BaseController
    {
        private readonly IActivityService _activityService;
        private readonly ILogger<ActivitiesController> _logger;

        public ActivitiesController(IActivityService activityService, ILogger<ActivitiesController> logger, UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _activityService = activityService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new activity for a specific task
        /// </summary>
        [HttpPost("tasks/{taskId}/activities")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> CreateActivity(int taskId, [FromBody] CreateActivityRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _activityService.CreateActivityAsync(taskId, userId, request, cancellationToken);
                return SuccessResponse(result, "Activity created successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating activity");
                return ErrorResponse(ex.Message, 400);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to create activity");
                return ErrorResponse(ex.Message, 403);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                return ErrorResponse("An error occurred while creating the activity", 500);
            }
        }

        /// <summary>
        /// Get activities for the current user
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetActivities([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var activities = await _activityService.GetActivitiesByUserAsync(userId, startDate, endDate, cancellationToken);
                return SuccessResponse(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities");
                return ErrorResponse("An error occurred while retrieving activities", 500);
            }
        }

        /// <summary>
        /// Get a specific activity by ID
        /// </summary>
        [HttpGet("{activityId}")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetActivity(long activityId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var activity = await _activityService.GetActivityByIdAsync(activityId, userId, cancellationToken);
                if (activity == null)
                {
                    return ErrorResponse("Activity not found", 404);
                }

                return SuccessResponse(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity {ActivityId}", activityId);
                return ErrorResponse("An error occurred while retrieving the activity", 500);
            }
        }
    }
}
