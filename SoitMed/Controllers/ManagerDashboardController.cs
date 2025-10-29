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
    [Route("api/manager")]
    [Authorize]
    public class ManagerDashboardController : BaseController
    {
        private readonly IManagerDashboardService _dashboardService;
        private readonly ILogger<ManagerDashboardController> _logger;

        public ManagerDashboardController(IManagerDashboardService dashboardService, ILogger<ManagerDashboardController> logger, UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics for a manager
        /// </summary>
        [HttpGet("dashboard-stats")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                var managerId = GetCurrentUserId();
                if (string.IsNullOrEmpty(managerId))
                {
                    return Unauthorized();
                }

                // Validate date range
                if (startDate >= endDate)
                {
                    return ErrorResponse("Start date must be before end date", 400);
                }

                // Limit date range to prevent performance issues
                if ((endDate - startDate).TotalDays > 365)
                {
                    return ErrorResponse("Date range cannot exceed 365 days", 400);
                }

                var stats = await _dashboardService.GetDashboardStatisticsAsync(managerId, startDate, endDate, cancellationToken);
                return SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return ErrorResponse("An error occurred while retrieving dashboard statistics", 500);
            }
        }
    }
}
