using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Services;
using SoitMed.Models.Identity;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Controller for managing offer requests in the sales workflow
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OfferRequestController : BaseController
    {
        private readonly IOfferRequestService _offerRequestService;
        private readonly ILogger<OfferRequestController> _logger;

        public OfferRequestController(
            IOfferRequestService offerRequestService,
            ILogger<OfferRequestController> logger,
            UserManager<ApplicationUser> userManager) 
            : base(userManager)
        {
            _offerRequestService = offerRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Create new offer request
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Salesman,SalesManager")]
        public async Task<IActionResult> CreateOfferRequest([FromBody] CreateOfferRequestDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerRequestService.CreateOfferRequestAsync(createDto, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer request created successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating offer request");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer request");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while creating offer request"));
            }
        }

        /// <summary>
        /// Get all offer requests
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Salesman,SalesManager,SalesSupport,SuperAdmin")]
        public async Task<IActionResult> GetOfferRequests([FromQuery] string? status = null, [FromQuery] string? requestedBy = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                
                var result = await _offerRequestService.GetOfferRequestsAsync(status, requestedBy, userId, userRole);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer requests retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offer requests");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving offer requests"));
            }
        }

        /// <summary>
        /// Get offer request by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Salesman,SalesManager,SalesSupport,SuperAdmin")]
        public async Task<IActionResult> GetOfferRequest(long id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = "Salesman"; // This should be replaced with actual role checking
                
                var result = await _offerRequestService.GetOfferRequestAsync(id, userId, userRole);

                if (result == null)
                {
                    return NotFound(ResponseHelper.CreateErrorResponse("Offer request not found"));
                }

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer request retrieved successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to offer request");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving offer request");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving offer request"));
            }
        }

        /// <summary>
        /// Reassign offer request to another SalesSupport member (if needed)
        /// Note: Requests are automatically assigned to SalesSupport when created
        /// </summary>
        [HttpPut("{id}/assign")]
        [Authorize(Roles = "SalesManager,SalesSupport,SuperAdmin")]
        public async Task<IActionResult> AssignOfferRequest(long id, [FromBody] AssignOfferRequestDTO assignDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerRequestService.AssignToSupportAsync(id, assignDto.AssignedTo, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer request reassigned successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for assigning offer request");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning offer request");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while assigning offer request"));
            }
        }

        /// <summary>
        /// Update offer request status
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> UpdateOfferRequestStatus(long id, [FromBody] UpdateOfferRequestStatusDTO statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ValidationHelperService.FormatValidationErrors(ModelState));
                }

                var userId = GetCurrentUserId();
                var result = await _offerRequestService.UpdateStatusAsync(id, statusDto.Status, statusDto.Notes, userId);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Offer request status updated successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for updating offer request status");
                return BadRequest(ResponseHelper.CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating offer request status");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while updating offer request status"));
            }
        }

        /// <summary>
        /// Get offer requests by salesman
        /// </summary>
        [HttpGet("salesman/{salesmanId}")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetOfferRequestsBySalesman(string salesmanId, [FromQuery] string? status = null)
        {
            try
            {
                var result = await _offerRequestService.GetOfferRequestsBySalesmanAsync(salesmanId, status);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Salesman offer requests retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving salesman offer requests");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving salesman offer requests"));
            }
        }

        /// <summary>
        /// Get offer requests assigned to sales support
        /// </summary>
        [HttpGet("assigned/{supportId}")]
        [Authorize(Roles = "SalesSupport,SalesManager,SuperAdmin")]
        public async Task<IActionResult> GetOfferRequestsAssignedTo(string supportId, [FromQuery] string? status = null)
        {
            try
            {
                var result = await _offerRequestService.GetOfferRequestsAssignedToAsync(supportId, status);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "Assigned offer requests retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assigned offer requests");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving assigned offer requests"));
            }
        }

        /// <summary>
        /// Get my own offer requests (for salesmen)
        /// </summary>
        [HttpGet("my-requests")]
        [Authorize(Roles = "Salesman,SalesManager")]
        public async Task<IActionResult> GetMyOfferRequests([FromQuery] string? status = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _offerRequestService.GetOfferRequestsBySalesmanAsync(userId, status);

                return Ok(ResponseHelper.CreateSuccessResponse(result, "My offer requests retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving my offer requests");
                return StatusCode(500, ResponseHelper.CreateErrorResponse("An error occurred while retrieving your offer requests"));
            }
        }
    }
}


