using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Controller for managing weekly plan tasks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WeeklyPlanTaskController : BaseController
    {
        private readonly IWeeklyPlanTaskService _taskService;
        private readonly ILogger<WeeklyPlanTaskController> _logger;

        public WeeklyPlanTaskController(
            IWeeklyPlanTaskService taskService,
            ILogger<WeeklyPlanTaskController> logger,
            UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _taskService = taskService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new task in a weekly plan
        /// </summary>
        /// <param name="createDto">Task creation data</param>
        /// <returns>Created task details</returns>
        [HttpPost]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> CreateTask([FromBody] CreateWeeklyPlanTaskDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Validation failed", ModelState));
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var result = await _taskService.CreateTaskAsync(createDto, userId);
                return CreatedAtAction(nameof(GetTask), new { id = result.Id }, 
                    ResponseHelper.CreateSuccessResponse(result, "Task created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when creating task");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when creating task");
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating weekly plan task");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating the task"));
            }
        }

        /// <summary>
        /// Get a specific task by ID
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <returns>Task details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetTask(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var userRole = await GetCurrentUserRoleAsync();
                var task = await _taskService.GetTaskAsync((int)id, userId, userRole);

                if (task == null)
                    return NotFound(ResponseHelper.CreateErrorResponse("Task not found"));

                return Ok(ResponseHelper.CreateSuccessResponse(task));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when retrieving task {TaskId}", id);
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving the task"));
            }
        }

        /// <summary>
        /// Get all tasks for a specific weekly plan
        /// </summary>
        /// <param name="weeklyPlanId">Weekly plan ID</param>
        /// <returns>List of tasks</returns>
        [HttpGet("by-plan/{weeklyPlanId}")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetTasksByPlan(long weeklyPlanId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var userRole = await GetCurrentUserRoleAsync();
                var tasks = await _taskService.GetTasksByPlanAsync(weeklyPlanId, userId, userRole);

                return Ok(ResponseHelper.CreateSuccessResponse(tasks, $"Found {tasks.Count} task(s)"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when retrieving tasks for plan {WeeklyPlanId}", weeklyPlanId);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when retrieving tasks for plan {WeeklyPlanId}", weeklyPlanId);
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for plan {WeeklyPlanId}", weeklyPlanId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving tasks"));
            }
        }

        /// <summary>
        /// Update an existing task
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <param name="updateDto">Task update data</param>
        /// <returns>Updated task details</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> UpdateTask(long id, [FromBody] UpdateWeeklyPlanTaskDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ResponseHelper.CreateErrorResponse("Validation failed", ModelState));
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var result = await _taskService.UpdateTaskAsync((int)id, updateDto, userId);

                if (result == null)
                    return NotFound(ResponseHelper.CreateErrorResponse("Task not found"));

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Task updated successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument when updating task {TaskId}", id);
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when updating task {TaskId}", id);
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating the task"));
            }
        }

        /// <summary>
        /// Delete a task
        /// </summary>
        /// <param name="id">Task ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SalesMan,SalesManager,SuperAdmin")]
        public async Task<IActionResult> DeleteTask(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var result = await _taskService.DeleteTaskAsync((int)id, userId);

                if (!result)
                    return NotFound(ResponseHelper.CreateErrorResponse("Task not found"));

                return Ok(ResponseHelper.CreateSuccessResponse(null, "Task deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access when deleting task {TaskId}", id);
                return StatusCode(403, ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while deleting the task"));
            }
        }
    }
}



