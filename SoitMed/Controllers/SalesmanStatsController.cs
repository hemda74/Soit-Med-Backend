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
    [Route("api/salesman")]
    [Authorize]
    public class SalesmanStatsController : BaseController
    {
        private readonly ISalesmanStatsService _salesmanStatsService;
        private readonly ILogger<SalesmanStatsController> _logger;

        public SalesmanStatsController(
            ISalesmanStatsService salesmanStatsService, 
            ILogger<SalesmanStatsController> logger, 
            UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _salesmanStatsService = salesmanStatsService;
            _logger = logger;
        }

        /// <summary>
        /// Get salesman statistics for a specific date range
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetSalesmanStats(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var salesmanId = GetCurrentUserId();
                if (string.IsNullOrEmpty(salesmanId))
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

                var stats = await _salesmanStatsService.GetSalesmanStatisticsAsync(
                    salesmanId, 
                    startDate, 
                    endDate, 
                    cancellationToken);

                return SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salesman statistics");
                return ErrorResponse("An error occurred while retrieving statistics", 500);
            }
        }

        /// <summary>
        /// Get salesman statistics for current week
        /// </summary>
        [HttpGet("stats/current-week")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetCurrentWeekStats(CancellationToken cancellationToken = default)
        {
            try
            {
                var salesmanId = GetCurrentUserId();
                if (string.IsNullOrEmpty(salesmanId))
                {
                    return Unauthorized();
                }

                var stats = await _salesmanStatsService.GetSalesmanCurrentWeekStatsAsync(
                    salesmanId, 
                    cancellationToken);

                return SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current week statistics");
                return ErrorResponse("An error occurred while retrieving current week statistics", 500);
            }
        }

        /// <summary>
        /// Get salesman statistics for current month
        /// </summary>
        [HttpGet("stats/current-month")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetCurrentMonthStats(CancellationToken cancellationToken = default)
        {
            try
            {
                var salesmanId = GetCurrentUserId();
                if (string.IsNullOrEmpty(salesmanId))
                {
                    return Unauthorized();
                }

                var stats = await _salesmanStatsService.GetSalesmanCurrentMonthStatsAsync(
                    salesmanId, 
                    cancellationToken);

                return SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current month statistics");
                return ErrorResponse("An error occurred while retrieving current month statistics", 500);
            }
        }
    }
}
