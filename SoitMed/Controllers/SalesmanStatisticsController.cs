using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Controller for managing salesman statistics and targets
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesmanStatisticsController : BaseController
    {
        private readonly ISalesmanStatisticsService _statisticsService;
        private readonly ILogger<SalesmanStatisticsController> _logger;

        public SalesmanStatisticsController(
            ISalesmanStatisticsService statisticsService,
            ILogger<SalesmanStatisticsController> logger,
            UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get statistics for the current salesman
        /// </summary>
        [HttpGet("my-statistics")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetMyStatistics([FromQuery] int? year = null, [FromQuery] int? quarter = null)
        {
            try
            {
                var salesmanId = GetCurrentUserId();
                // Use current year as default if year not provided
                var statisticsYear = year ?? DateTime.UtcNow.Year;
                var result = await _statisticsService.GetStatisticsAsync(salesmanId, statisticsYear, quarter);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for salesman {SalesmanId}", GetCurrentUserId());
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving statistics"));
            }
        }

        /// <summary>
        /// Get statistics for a specific salesman (Manager/SuperAdmin only)
        /// </summary>
        [HttpGet("{salesmanId}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetSalesmanStatistics(string salesmanId, [FromQuery] int? year = null, [FromQuery] int? quarter = null)
        {
            try
            {
                // Use current year as default if year not provided
                var statisticsYear = year ?? DateTime.UtcNow.Year;
                var result = await _statisticsService.GetStatisticsAsync(salesmanId, statisticsYear, quarter);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Statistics retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid salesman ID");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for salesman {SalesmanId}", salesmanId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving statistics"));
            }
        }

        /// <summary>
        /// Get statistics for all salesmen (Manager/SuperAdmin only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetAllStatistics([FromQuery] int? year = null, [FromQuery] int? quarter = null)
        {
            try
            {
                // Use current year as default if year not provided
                var statisticsYear = year ?? DateTime.UtcNow.Year;
                var result = await _statisticsService.GetAllSalesmenStatisticsAsync(statisticsYear, quarter);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "All statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all statistics");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving statistics"));
            }
        }

        /// <summary>
        /// Get progress for the current salesman (statistics + targets)
        /// </summary>
        [HttpGet("my-progress")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetMyProgress([FromQuery] int year, [FromQuery] int? quarter = null)
        {
            try
            {
                var salesmanId = GetCurrentUserId();
                var result = await _statisticsService.GetSalesmanProgressAsync(salesmanId, year, quarter);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Progress retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress for salesman {SalesmanId}", GetCurrentUserId());
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving progress"));
            }
        }

        /// <summary>
        /// Get progress for a specific salesman (Manager/SuperAdmin only)
        /// </summary>
        [HttpGet("{salesmanId}/progress")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetSalesmanProgress(string salesmanId, [FromQuery] int? year = null, [FromQuery] int? quarter = null)
        {
            try
            {
                // Use current year as default if year not provided
                var statisticsYear = year ?? DateTime.UtcNow.Year;
                var result = await _statisticsService.GetSalesmanProgressAsync(salesmanId, statisticsYear, quarter);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Progress retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid salesman ID");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress for salesman {SalesmanId}", salesmanId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving progress"));
            }
        }

        /// <summary>
        /// Create a new target for a salesman or team
        /// </summary>
        [HttpPost("targets")]
        [Authorize(Roles = "SalesManager")]
        public async Task<IActionResult> CreateTarget([FromBody] CreateSalesmanTargetDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var managerId = GetCurrentUserId();
                var result = await _statisticsService.CreateTargetAsync(dto, managerId);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Target created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid target data");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Target already exists");
                return Conflict(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating target");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating target"));
            }
        }

        /// <summary>
        /// Update an existing target
        /// </summary>
        [HttpPut("targets/{targetId}")]
        [Authorize(Roles = "SalesManager")]
        public async Task<IActionResult> UpdateTarget(long targetId, [FromBody] CreateSalesmanTargetDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var result = await _statisticsService.UpdateTargetAsync(targetId, dto);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Target updated successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Target not found");
                return NotFound(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating target {TargetId}", targetId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating target"));
            }
        }

        /// <summary>
        /// Delete a target
        /// </summary>
        [HttpDelete("targets/{targetId}")]
        [Authorize(Roles = "SalesManager")]
        public async Task<IActionResult> DeleteTarget(long targetId)
        {
            try
            {
                var result = await _statisticsService.DeleteTargetAsync(targetId);
                if (!result)
                    return NotFound(ResponseHelper.CreateErrorResponse("Target not found"));

                return Ok(ResponseHelper.CreateSuccessResponse(null, "Target deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting target {TargetId}", targetId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while deleting target"));
            }
        }

        /// <summary>
        /// Get targets for a specific salesman
        /// </summary>
        [HttpGet("targets/salesman/{salesmanId}")]
        [Authorize(Roles = "Salesman,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetSalesmanTargets(string salesmanId, [FromQuery] int year)
        {
            try
            {
                var result = await _statisticsService.GetTargetsForSalesmanAsync(salesmanId, year);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Targets retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving targets for salesman {SalesmanId}", salesmanId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving targets"));
            }
        }

        /// <summary>
        /// Get team target for a specific period
        /// </summary>
        [HttpGet("targets/team")]
        [Authorize(Roles = "Salesman,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetTeamTarget([FromQuery] int year, [FromQuery] int? quarter = null)
        {
            try
            {
                var result = await _statisticsService.GetTeamTargetAsync(year, quarter);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Team target retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team target");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving team target"));
            }
        }

        /// <summary>
        /// Get targets for the current salesman
        /// </summary>
        [HttpGet("my-targets")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> GetMyTargets([FromQuery] int year)
        {
            try
            {
                var salesmanId = GetCurrentUserId();
                var result = await _statisticsService.GetTargetsForSalesmanAsync(salesmanId, year);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Targets retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving targets for salesman {SalesmanId}", GetCurrentUserId());
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving targets"));
            }
        }
    }
}

