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
    public class OffersController : BaseController
    {
        private readonly IActivityService _activityService;
        private readonly ILogger<OffersController> _logger;

        public OffersController(IActivityService activityService, ILogger<OffersController> logger, UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _activityService = activityService;
            _logger = logger;
        }

        /// <summary>
        /// Update an offer
        /// </summary>
        [HttpPut("{offerId}")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> UpdateOffer(long offerId, [FromBody] UpdateOfferDto updateDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _activityService.UpdateOfferAsync(offerId, userId, updateDto, cancellationToken);
                if (result == null)
                {
                    return ErrorResponse("Offer not found or you don't have permission to update it", 404);
                }

                return SuccessResponse(result, "Offer updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating offer {OfferId}", offerId);
                return ErrorResponse("An error occurred while updating the offer", 500);
            }
        }
    }
}
