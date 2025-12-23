using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models;
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
    public class SalesManStatisticsController : BaseController
    {
        private readonly ISalesManStatisticsService _statisticsService;
        private readonly ILogger<SalesManStatisticsController> _logger;

        public SalesManStatisticsController(
            ISalesManStatisticsService statisticsService,
            ILogger<SalesManStatisticsController> logger,
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
        [Authorize(Roles = "SalesMan")]
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
                _logger.LogError(ex, "Error retrieving statistics for salesman {SalesManId}", GetCurrentUserId());
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving statistics"));
            }
        }

        /// <summary>
        /// Get statistics for a specific salesman (Manager/SuperAdmin only)
        /// </summary>
        [HttpGet("{salesmanId}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetSalesManStatistics(string salesmanId, [FromQuery] int? year = null, [FromQuery] int? quarter = null)
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
                _logger.LogError(ex, "Error retrieving statistics for salesman {SalesManId}", salesmanId);
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
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> GetMyProgress([FromQuery] int year, [FromQuery] int? quarter = null)
        {
            try
            {
                var salesmanId = GetCurrentUserId();
                var result = await _statisticsService.GetSalesManProgressAsync(salesmanId, year, quarter);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Progress retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress for salesman {SalesManId}", GetCurrentUserId());
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving progress"));
            }
        }

        /// <summary>
        /// Get progress for a specific salesman (Manager/SuperAdmin only)
        /// </summary>
        [HttpGet("{salesmanId}/progress")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetSalesManProgress(string salesmanId, [FromQuery] int? year = null, [FromQuery] int? quarter = null)
        {
            try
            {
                // Use current year as default if year not provided
                var statisticsYear = year ?? DateTime.UtcNow.Year;
                var result = await _statisticsService.GetSalesManProgressAsync(salesmanId, statisticsYear, quarter);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Progress retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid salesman ID");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress for salesman {SalesManId}", salesmanId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving progress"));
            }
        }

        /// <summary>
        /// Create a new target for a salesman or team
        /// Money targets: SalesManager only
        /// Activity targets: SalesMan can set for themselves
        /// </summary>
        [HttpPost("targets")]
        [Authorize(Roles = "SalesManager,SalesMan")]
        public async Task<IActionResult> CreateTarget([FromBody] CreateSalesManTargetDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var currentUserId = GetCurrentUserId();
                if (string.IsNullOrEmpty(currentUserId))
                    return Unauthorized(ResponseHelper.CreateErrorResponse("User not authenticated"));
                
                var user = await GetCurrentUserAsync();
                if (user == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("User not found"));
                
                var userRoles = await UserManager.GetRolesAsync(user);
                var isManager = userRoles.Contains("SalesManager") || userRoles.Contains("SuperAdmin");
                var isSalesMan = userRoles.Contains("SalesMan");

                // Log for debugging
                _logger.LogInformation("CreateTarget - UserId: {UserId}, UserName: {UserName}, Roles: [{Roles}], TargetType: {TargetType}, IsManager: {IsManager}, IsSalesMan: {IsSalesMan}",
                    currentUserId, user.UserName, string.Join(", ", userRoles), dto.TargetType, isManager, isSalesMan);

                string? managerId = null;
                string? salesmanId = null;

                if (dto.TargetType == TargetType.Money)
                {
                    if (!isManager)
                    {
                        _logger.LogWarning("User {UserId} attempted to create money target but is not a manager. Roles: {Roles}", 
                            currentUserId, string.Join(",", userRoles));
                        return StatusCode(403, ResponseHelper.CreateErrorResponse("Only managers can create money targets"));
                    }
                    managerId = currentUserId;
                }
                else if (dto.TargetType == TargetType.Activity)
                {
                    if (!isSalesMan)
                    {
                        _logger.LogWarning("User {UserId} attempted to create activity target but is not a salesman. Roles: {Roles}", 
                            currentUserId, string.Join(",", userRoles));
                        return StatusCode(403, ResponseHelper.CreateErrorResponse($"Only salesmen can create activity targets. Your roles: {string.Join(", ", userRoles)}"));
                    }
                    
                    // SalesMan can only set targets for themselves (not team targets)
                    // Ignore dto.SalesManId from client - always use current user's ID for security
                    if (!dto.IsTeamTarget)
                    {
                        // Force salesmanId to be the current user for individual targets
                        salesmanId = currentUserId;
                        dto.SalesManId = currentUserId;
                    }
                    else
                    {
                        // For team targets, salesmanId should be null
                        salesmanId = null;
                        dto.SalesManId = null;
                    }
                }

                var result = await _statisticsService.CreateTargetAsync(dto, managerId, salesmanId);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Target created successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized target creation: {Message}", ex.Message);
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
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
        /// Money targets: SalesManager only
        /// Activity targets: SalesMan can update their own
        /// </summary>
        [HttpPut("targets/{targetId}")]
        [Authorize(Roles = "SalesManager,SalesMan")]
        public async Task<IActionResult> UpdateTarget(long targetId, [FromBody] CreateSalesManTargetDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var currentUserId = GetCurrentUserId();
                var result = await _statisticsService.UpdateTargetAsync(targetId, dto, currentUserId);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Target updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized target update: {Message}", ex.Message);
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
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
        /// Partially update an existing target (PATCH)
        /// Money targets: SalesManager only
        /// Activity targets: SalesMan can update their own
        /// </summary>
        [HttpPatch("targets/{targetId}")]
        [Authorize(Roles = "SalesManager,SalesMan")]
        public async Task<IActionResult> PatchTarget(long targetId, [FromBody] CreateSalesManTargetDTO dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _statisticsService.UpdateTargetAsync(targetId, dto, currentUserId);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Target updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized target update: {Message}", ex.Message);
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
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
        [HttpGet("targets/SalesMan/{salesmanId}")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetSalesManTargets(string salesmanId, [FromQuery] int year)
        {
            try
            {
                var result = await _statisticsService.GetTargetsForSalesManAsync(salesmanId, year);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Targets retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving targets for salesman {SalesManId}", salesmanId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving targets"));
            }
        }

        /// <summary>
        /// Get team target for a specific period
        /// </summary>
        [HttpGet("targets/team")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
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
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> GetMyTargets([FromQuery] int year)
        {
            try
            {
                var salesmanId = GetCurrentUserId();
                var result = await _statisticsService.GetTargetsForSalesManAsync(salesmanId, year);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "Targets retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving targets for salesman {SalesManId}", GetCurrentUserId());
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving targets"));
            }
        }
    }
}

