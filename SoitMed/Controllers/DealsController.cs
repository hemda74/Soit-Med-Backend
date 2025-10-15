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
    public class DealsController : BaseController
    {
        private readonly IActivityService _activityService;
        private readonly ILogger<DealsController> _logger;

        public DealsController(IActivityService activityService, ILogger<DealsController> logger, UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _activityService = activityService;
            _logger = logger;
        }

        /// <summary>
        /// Update a deal
        /// </summary>
        [HttpPut("{dealId}")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> UpdateDeal(long dealId, [FromBody] UpdateDealDto updateDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _activityService.UpdateDealAsync(dealId, userId, updateDto, cancellationToken);
                if (result == null)
                {
                    return ErrorResponse("Deal not found or you don't have permission to update it", 404);
                }

                return SuccessResponse(result, "Deal updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating deal {DealId}", dealId);
                return ErrorResponse("An error occurred while updating the deal", 500);
            }
        }
    }
}
