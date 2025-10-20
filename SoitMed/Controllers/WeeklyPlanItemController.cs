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
    public class WeeklyPlanItemController : BaseController
    {
        private readonly IWeeklyPlanItemService _weeklyPlanItemService;
        private readonly ILogger<WeeklyPlanItemController> _logger;

        public WeeklyPlanItemController(
            IWeeklyPlanItemService weeklyPlanItemService, 
            ILogger<WeeklyPlanItemController> logger, 
            UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _weeklyPlanItemService = weeklyPlanItemService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlanItem([FromBody] CreateWeeklyPlanItemDTO createDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("غير مصرح لك"));

                var planItem = await _weeklyPlanItemService.CreatePlanItemAsync(createDto, currentUser.Id);
                return CreatedAtAction(nameof(GetPlanItem), new { id = planItem.Id }, 
                    ResponseHelper.CreateSuccessResponse(planItem));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plan item");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("حدث خطأ في إضافة عنصر الخطة"));
            }
        }

        [HttpGet("plan/{planId}")]
        public async Task<IActionResult> GetPlanItems(long planId)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("غير مصرح لك"));

                var items = await _weeklyPlanItemService.GetPlanItemsAsync(planId, currentUser.Id);
                return Ok(ResponseHelper.CreateSuccessResponse(items));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plan items for plan {PlanId}", planId);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("حدث خطأ في جلب عناصر الخطة"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlanItem(long id)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("غير مصرح لك"));

                var item = await _weeklyPlanItemService.GetPlanItemAsync(id, currentUser.Id);
                if (item == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("عنصر الخطة غير موجود"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse(item));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plan item {ItemId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("حدث خطأ في جلب عنصر الخطة"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlanItem(long id, [FromBody] UpdateWeeklyPlanItemDTO updateDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("غير مصرح لك"));

                var item = await _weeklyPlanItemService.UpdatePlanItemAsync(id, updateDto, currentUser.Id);
                if (item == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("عنصر الخطة غير موجود"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse(new
                {
                    Id = item.Id,
                    ClientName = item.ClientName,
                    PlannedVisitDate = item.PlannedVisitDate,
                    VisitPurpose = item.VisitPurpose,
                    Priority = item.Priority,
                    UpdatedAt = item.UpdatedAt
                }));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plan item {ItemId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("حدث خطأ في تحديث عنصر الخطة"));
            }
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompletePlanItem(long id, [FromBody] CompletePlanItemDTO completeDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("غير مصرح لك"));

                var success = await _weeklyPlanItemService.CompletePlanItemAsync(id, completeDto, currentUser.Id);
                if (!success)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("عنصر الخطة غير موجود"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse("تم تحديث حالة العنصر بنجاح"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing plan item {ItemId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("حدث خطأ في تحديث حالة العنصر"));
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelPlanItem(long id, [FromBody] CancelPlanItemDTO cancelDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("غير مصرح لك"));

                var success = await _weeklyPlanItemService.CancelPlanItemAsync(id, cancelDto, currentUser.Id);
                if (!success)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("عنصر الخطة غير موجود"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse("تم إلغاء العنصر بنجاح"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling plan item {ItemId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("حدث خطأ في إلغاء العنصر"));
            }
        }

        [HttpPost("{id}/postpone")]
        public async Task<IActionResult> PostponePlanItem(long id, [FromBody] PostponePlanItemDTO postponeDto)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("غير مصرح لك"));

                var success = await _weeklyPlanItemService.PostponePlanItemAsync(id, postponeDto, currentUser.Id);
                if (!success)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("عنصر الخطة غير موجود"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse("تم تأجيل العنصر بنجاح"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error postponing plan item {ItemId}", id);
                return StatusCode(500, ResponseHelper.CreateErrorResponse("حدث خطأ في تأجيل العنصر"));
            }
        }

        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueItems()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("غير مصرح لك"));

                var items = await _weeklyPlanItemService.GetOverdueItemsAsync(currentUser.Id);
                return Ok(ResponseHelper.CreateSuccessResponse(items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue items");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("حدث خطأ في جلب العناصر المتأخرة"));
            }
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingItems([FromQuery] int days = 7)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                    return Unauthorized(ResponseHelper.CreateErrorResponse("غير مصرح لك"));

                var items = await _weeklyPlanItemService.GetUpcomingItemsAsync(currentUser.Id, days);
                return Ok(ResponseHelper.CreateSuccessResponse(items));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming items");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("حدث خطأ في جلب العناصر القادمة"));
            }
        }
    }
}
