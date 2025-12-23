using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Controller for managing task progress in the sales workflow
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskProgressController : BaseController
    {
        private readonly ITaskProgressService _taskProgressService;
        private readonly ILogger<TaskProgressController> _logger;
        private readonly IVoiceUploadService _voiceUploadService;

        public TaskProgressController(
            ITaskProgressService taskProgressService,
            ILogger<TaskProgressController> logger,
            UserManager<ApplicationUser> userManager,
            IVoiceUploadService voiceUploadService) 
            : base(userManager)
        {
            _taskProgressService = taskProgressService;
            _logger = logger;
            _voiceUploadService = voiceUploadService;
        }

        /// <summary>
        /// Create new progress for a task
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SalesMan,SalesManager")]
        public async Task<IActionResult> CreateProgress([FromBody] CreateTaskProgressDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _taskProgressService.CreateProgressAsync(createDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Task progress created successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for creating task progress by user {UserId}", GetCurrentUserId());
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating task progress");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task progress");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating task progress"));
            }
        }

        /// <summary>
        /// Upload a voice description file for task progress
        /// </summary>
        [HttpPost("upload-voice")]
        [Authorize(Roles = "SalesMan,SalesManager")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> UploadVoiceDescription([FromForm] IFormFile voiceFile)
        {
            try
            {
                if (voiceFile == null || voiceFile.Length == 0)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Voice file is required"));
                }

                var userId = GetCurrentUserId();
                var uploadResult = await _voiceUploadService.UploadVoiceFileAsync(voiceFile, userId);

                if (!uploadResult.Success)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse(uploadResult.ErrorMessage ?? "Failed to upload voice file"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse(uploadResult, "Voice file uploaded successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading voice description");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while uploading voice file"));
            }
        }

        /// <summary>
        /// Create progress and trigger offer request when client is interested
        /// </summary>
        [HttpPost("with-offer-request")]
        [Authorize(Roles = "SalesMan,SalesManager")]
        public async Task<IActionResult> CreateProgressWithOfferRequest([FromBody] CreateTaskProgressWithOfferRequestDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _taskProgressService.CreateProgressAndOfferRequestAsync(createDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Task progress created and offer request triggered successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for creating task progress with offer request by user {UserId}", GetCurrentUserId());
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating task progress with offer request");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task progress with offer request");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating task progress with offer request"));
            }
        }

        /// <summary>
        /// Get all task progress for the current user (role-based filtering)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetAllProgress([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                // For SalesMan, return only their own progress
                if (userRole == "SalesMan")
                {
                    var myProgress = await _taskProgressService.GetProgressesByEmployeeAsync(userId, startDate, endDate);
                    return Ok(ResponseHelper.CreateSuccessResponse(myProgress, "Task progress retrieved successfully"));
                }
                
                // For SalesManager and SuperAdmin, return all progress
                var result = await _taskProgressService.GetAllProgressesAsync(startDate, endDate);
                return Ok(ResponseHelper.CreateSuccessResponse(result, "All task progress retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task progress");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving task progress"));
            }
        }

        /// <summary>
        /// Get all progress for a specific task
        /// </summary>
        [HttpGet("task/{taskId}")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetProgressByTask(long taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                var result = await _taskProgressService.GetProgressesByTaskAsync(taskId, userId, userRole);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Task progress retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to task progress");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task progress");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving task progress"));
            }
        }

        /// <summary>
        /// Get all progress for a specific client (for client history)
        /// </summary>
        [HttpGet("by-client/{clientId}")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetProgressByClient(long clientId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                var result = await _taskProgressService.GetProgressesByClientAsync(clientId, userId, userRole);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Client progress retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to client progress");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client progress");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving client progress"));
            }
        }

        /// <summary>
        /// Update existing progress
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SalesMan,SalesManager")]
        public async Task<IActionResult> UpdateProgress(long id, [FromBody] CreateTaskProgressDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _taskProgressService.UpdateProgressAsync(id, updateDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Task progress updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for updating task progress by user {UserId}", GetCurrentUserId());
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for updating task progress");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task progress");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating task progress"));
            }
        }

        /// <summary>
        /// Get progress by employee and date range
        /// </summary>
        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetProgressByEmployee(string employeeId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var result = await _taskProgressService.GetProgressesByEmployeeAsync(employeeId, startDate, endDate);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Employee progress retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee progress");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving employee progress"));
            }
        }
    }
}
