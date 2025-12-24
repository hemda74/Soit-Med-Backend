using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;
using FluentValidation;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    public class WeeklyPlanController : BaseController
    {
        private readonly IWeeklyPlanService _weeklyPlanService;
        private readonly ILogger<WeeklyPlanController> _logger;
        private readonly IValidator<CreateWeeklyPlanDto> _createPlanValidator;
        private readonly IValidator<UpdateWeeklyPlanDto> _updatePlanValidator;
        private readonly IValidator<AddTaskToWeeklyPlanDto> _addTaskValidator;
        private readonly IValidator<UpdateWeeklyPlanTaskDto> _updateTaskValidator;
        private readonly IValidator<CreateDailyProgressDto> _createProgressValidator;
        private readonly IValidator<UpdateDailyProgressDto> _updateProgressValidator;
        private readonly IValidator<ReviewWeeklyPlanDto> _reviewValidator;
        private readonly IValidator<FilterWeeklyPlansDto> _filterValidator;

        public WeeklyPlanController(
            IWeeklyPlanService weeklyPlanService,
            UserManager<ApplicationUser> userManager,
            ILogger<WeeklyPlanController> logger,
            IValidator<CreateWeeklyPlanDto> createPlanValidator,
            IValidator<UpdateWeeklyPlanDto> updatePlanValidator,
            IValidator<AddTaskToWeeklyPlanDto> addTaskValidator,
            IValidator<UpdateWeeklyPlanTaskDto> updateTaskValidator,
            IValidator<CreateDailyProgressDto> createProgressValidator,
            IValidator<UpdateDailyProgressDto> updateProgressValidator,
            IValidator<ReviewWeeklyPlanDto> reviewValidator,
            IValidator<FilterWeeklyPlansDto> filterValidator) : base(userManager)
        {
            _weeklyPlanService = weeklyPlanService;
            _logger = logger;
            _createPlanValidator = createPlanValidator;
            _updatePlanValidator = updatePlanValidator;
            _addTaskValidator = addTaskValidator;
            _updateTaskValidator = updateTaskValidator;
            _createProgressValidator = createProgressValidator;
            _updateProgressValidator = updateProgressValidator;
            _reviewValidator = reviewValidator;
            _filterValidator = filterValidator;
        }

        #region Weekly Plan CRUD

        /// <summary>
        /// Create a new weekly plan with tasks (Salesman only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> CreateWeeklyPlan([FromBody] CreateWeeklyPlanDto createDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(createDto, _createPlanValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _weeklyPlanService.CreateWeeklyPlanAsync(createDto, userId, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("A plan already exists for this week or invalid data provided.", 409);
            }

            return CreatedAtAction(nameof(GetWeeklyPlanById), new { id = result.Id },
                CreateSuccessResponse(result, "Weekly plan created successfully"));
        }

        /// <summary>
        /// Update an existing weekly plan (Salesman only - own plans)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> UpdateWeeklyPlan(long id, [FromBody] UpdateWeeklyPlanDto updateDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(updateDto, _updatePlanValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _weeklyPlanService.UpdateWeeklyPlanAsync(id, updateDto, userId, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Plan not found or you don't have permission to update it.", 404);
            }

            return SuccessResponse(result, "Weekly plan updated successfully");
        }

        /// <summary>
        /// Delete a weekly plan (Salesman only - own plans)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> DeleteWeeklyPlan(long id, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _weeklyPlanService.DeleteWeeklyPlanAsync(id, userId, cancellationToken);
            if (!result)
            {
                return ErrorResponse("Plan not found or you don't have permission to delete it.", 404);
            }

            return SuccessResponse(message: "Weekly plan deleted successfully");
        }

        /// <summary>
        /// Get a specific weekly plan by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWeeklyPlanById(long id, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var (user, authError) = await ControllerAuthorizationHelper.GetCurrentUserAsync(userId, UserManager);
            if (authError != null)
                return authError;

            var isManager = await ControllerAuthorizationHelper.IsManagerAsync(user!, UserManager);

            var canAccess = await _weeklyPlanService.CanAccessWeeklyPlanAsync(id, userId!, isManager, cancellationToken);
            if (!canAccess)
            {
                return ErrorResponse("Plan not found or you don't have permission to view it.", 404);
            }

            var result = await _weeklyPlanService.GetWeeklyPlanByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Plan not found.", 404);
            }

            return SuccessResponse(result, "Weekly plan retrieved successfully");
        }

        /// <summary>
        /// Get weekly plans with optional filtering (SalesManager/SuperAdmin: all plans, Salesman: own plans)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWeeklyPlans([FromQuery] FilterWeeklyPlansDto filterDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(filterDto, _filterValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            var (user, authError) = await ControllerAuthorizationHelper.GetCurrentUserAsync(userId, UserManager);
            if (authError != null)
                return authError;

            var isManager = await ControllerAuthorizationHelper.IsManagerAsync(user!, UserManager);

            PaginatedWeeklyPlansResponseDto result = isManager
                ? await _weeklyPlanService.GetWeeklyPlansAsync(filterDto, cancellationToken)
                : await _weeklyPlanService.GetWeeklyPlansForEmployeeAsync(userId!, filterDto, cancellationToken);

            return SuccessResponse(result, $"Found {result.TotalCount} weekly plan(s)");
        }

        /// <summary>
        /// Get all salesmen (for filter dropdown - SalesManager and SuperAdmin only)
        /// </summary>
        [HttpGet("salesmen")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetAllSalesmen([FromQuery] string? q = null)
        {
            try
            {
                var salesmen = await UserManager.GetUsersInRoleAsync("SalesMan");

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var term = q.Trim().ToLower();
                    salesmen = salesmen
                        .Where(u =>
                            (!string.IsNullOrEmpty(u.FirstName) && u.FirstName.ToLower().Contains(term)) ||
                            (!string.IsNullOrEmpty(u.LastName) && u.LastName.ToLower().Contains(term)) ||
                            (!string.IsNullOrEmpty(u.UserName) && u.UserName.ToLower().Contains(term)))
                        .ToList();
                }

                var salesmenList = salesmen.Select(u => new
                {
                    id = u.Id,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email,
                    phoneNumber = u.PhoneNumber,
                    userName = u.UserName
                }).ToList();

                return SuccessResponse(salesmenList, "Salesmen retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all salesmen for weekly plan filter");
                return ErrorResponse("An error occurred while retrieving salesmen", 500);
            }
        }

        #endregion

        #region Task Operations

        /// <summary>
        /// Add a new task to an existing weekly plan (Salesman only - own plans)
        /// </summary>
        [HttpPost("{weeklyPlanId}/tasks")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> AddTask(long weeklyPlanId, [FromBody] AddTaskToWeeklyPlanDto taskDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(taskDto, _addTaskValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _weeklyPlanService.AddTaskToWeeklyPlanAsync(weeklyPlanId, taskDto, userId, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Plan not found or you don't have permission to add tasks to it.", 404);
            }

            return SuccessResponse(result, "Task added successfully");
        }

        /// <summary>
        /// Update a task in a weekly plan (Salesman only - own plans)
        /// </summary>
        [HttpPut("{weeklyPlanId}/tasks/{taskId}")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> UpdateTask(long weeklyPlanId, int taskId, [FromBody] UpdateWeeklyPlanTaskDto updateDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(updateDto, _updateTaskValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _weeklyPlanService.UpdateTaskAsync(weeklyPlanId, taskId, updateDto, userId, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Plan or task not found, or you don't have permission to update it.", 404);
            }

            return SuccessResponse(result, "Task updated successfully");
        }

        /// <summary>
        /// Delete a task from a weekly plan (Salesman only - own plans)
        /// </summary>
        [HttpDelete("{weeklyPlanId}/tasks/{taskId}")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> DeleteTask(long weeklyPlanId, int taskId, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _weeklyPlanService.DeleteTaskAsync(weeklyPlanId, taskId, userId, cancellationToken);
            if (!result)
            {
                return ErrorResponse("Plan or task not found, or you don't have permission to delete it.", 404);
            }

            return SuccessResponse(message: "Task deleted successfully");
        }

        #endregion

        #region Daily Progress Operations

        /// <summary>
        /// Add daily progress to a weekly plan (Salesman only - own plans)
        /// </summary>
        [HttpPost("{weeklyPlanId}/progress")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> AddDailyProgress(long weeklyPlanId, [FromBody] CreateDailyProgressDto progressDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(progressDto, _createProgressValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _weeklyPlanService.AddDailyProgressAsync(weeklyPlanId, progressDto, userId, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Plan not found, progress already exists for this date, or date is outside the week range.", 409);
            }

            return SuccessResponse(result, "Daily progress added successfully");
        }

        /// <summary>
        /// Update daily progress in a weekly plan (Salesman only - own plans)
        /// </summary>
        [HttpPut("{weeklyPlanId}/progress/{progressId}")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> UpdateDailyProgress(long weeklyPlanId, int progressId, [FromBody] UpdateDailyProgressDto updateDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(updateDto, _updateProgressValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _weeklyPlanService.UpdateDailyProgressAsync(weeklyPlanId, progressId, updateDto, userId, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Plan or progress not found, or you don't have permission to update it.", 404);
            }

            return SuccessResponse(result, "Daily progress updated successfully");
        }

        /// <summary>
        /// Delete daily progress from a weekly plan (Salesman only - own plans)
        /// </summary>
        [HttpDelete("{weeklyPlanId}/progress/{progressId}")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> DeleteDailyProgress(long weeklyPlanId, int progressId, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _weeklyPlanService.DeleteDailyProgressAsync(weeklyPlanId, progressId, userId, cancellationToken);
            if (!result)
            {
                return ErrorResponse("Plan or progress not found, or you don't have permission to delete it.", 404);
            }

            return SuccessResponse(message: "Daily progress deleted successfully");
        }

        #endregion

        #region Manager Review

        /// <summary>
        /// Review/rate a weekly plan (SalesManager and SuperAdmin only)
        /// </summary>
        [HttpPost("{id}/review")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> ReviewWeeklyPlan(long id, [FromBody] ReviewWeeklyPlanDto reviewDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(reviewDto, _reviewValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var result = await _weeklyPlanService.ReviewWeeklyPlanAsync(id, reviewDto, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Plan not found.", 404);
            }

            return SuccessResponse(result, "Weekly plan reviewed successfully");
        }

        #endregion
    }
}










