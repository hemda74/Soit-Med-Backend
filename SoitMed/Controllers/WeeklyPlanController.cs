using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WeeklyPlanController : BaseController
    {
        private readonly IWeeklyPlanService _weeklyPlanService;
        private readonly ILogger<WeeklyPlanController> _logger;

        public WeeklyPlanController(
            IWeeklyPlanService weeklyPlanService, 
            ILogger<WeeklyPlanController> logger, 
            UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _weeklyPlanService = weeklyPlanService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWeeklyPlan([FromBody] CreateWeeklyPlanDTO createDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var plan = await _weeklyPlanService.CreateWeeklyPlanAsync(createDto, currentUser.Id);
                return CreatedAtAction(nameof(GetWeeklyPlan), new { id = plan.Id }, 
                    ResponseHelper.CreateSuccessResponse(plan));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating weekly plan");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating the weekly plan"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWeeklyPlans([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var (plans, totalCount) = await _weeklyPlanService.GetWeeklyPlansAsync(currentUser.Id, page, pageSize);

                return Ok(ResponseHelper.CreateSuccessResponse(new
                {
                    plans,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly plans");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving weekly plans"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWeeklyPlan(long id)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var plan = await _weeklyPlanService.GetWeeklyPlanAsync(id, currentUser.Id);
                if (plan == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Weekly plan not found"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse(plan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly plan {PlanId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving the weekly plan"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWeeklyPlan(long id, [FromBody] UpdateWeeklyPlanDTO updateDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var plan = await _weeklyPlanService.UpdateWeeklyPlanAsync(id, updateDto, currentUser.Id);
                if (plan == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Weekly plan not found"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse(new
                {
                    Id = plan.Id,
                    Title = plan.Title,
                    Description = plan.Description,
                    UpdatedAt = plan.UpdatedAt
                }));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating weekly plan {PlanId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating the weekly plan"));
            }
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitWeeklyPlan(long id)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var success = await _weeklyPlanService.SubmitWeeklyPlanAsync(id, currentUser.Id);
                if (!success)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Weekly plan not found"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse("Weekly plan submitted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting weekly plan {PlanId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while submitting the weekly plan"));
            }
        }

        [HttpPost("{id}/review")]
        [Authorize(Roles = "Admin,SalesManager")]
        public async Task<IActionResult> ReviewWeeklyPlan(long id, [FromBody] ReviewWeeklyPlanDTO reviewDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var success = await _weeklyPlanService.ReviewWeeklyPlanAsync(id, reviewDto, currentUser.Id);
                if (!success)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Weekly plan not found"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse("Weekly plan reviewed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing weekly plan {PlanId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while reviewing the weekly plan"));
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentWeeklyPlan()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("Unauthorized access"));

                var plan = await _weeklyPlanService.GetCurrentWeeklyPlanAsync(currentUser.Id);
                if (plan == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("No current weekly plan found"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse(plan));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current weekly plan");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving the current weekly plan"));
            }
        }
    }
}
