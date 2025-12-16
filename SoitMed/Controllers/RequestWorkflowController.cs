using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RequestWorkflowsController : BaseController
    {
        private readonly IRequestWorkflowService _requestWorkflowService;
        private readonly ILogger<RequestWorkflowsController> _logger;

        public RequestWorkflowsController(IRequestWorkflowService requestWorkflowService, ILogger<RequestWorkflowsController> logger, UserManager<ApplicationUser> userManager)
            : base(userManager)
        {
            _requestWorkflowService = requestWorkflowService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new request workflow (e.g., salesman sending an offer to sales support)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> CreateRequestWorkflow([FromBody] CreateWorkflowRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _requestWorkflowService.CreateRequestWorkflowAsync(userId, request, cancellationToken);
                return SuccessResponse(result, "Request workflow created successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for creating workflow");
                return ErrorResponse(ex.Message, 400);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to create workflow");
                return ErrorResponse(ex.Message, 403);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating request workflow");
                return ErrorResponse("An error occurred while creating the request workflow", 500);
            }
        }

        /// <summary>
        /// Get requests sent by the current user
        /// </summary>
        [HttpGet("sent")]
        [Authorize(Roles = "SalesMan")]
        public async Task<IActionResult> GetSentRequests([FromQuery] RequestStatus? status = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var requests = await _requestWorkflowService.GetSentRequestsAsync(userId, status, cancellationToken);
                return SuccessResponse(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sent requests for user {UserId}", GetCurrentUserId());
                return ErrorResponse("An error occurred while retrieving sent requests", 500);
            }
        }

        /// <summary>
        /// Get requests assigned to the current user (e.g., sales support receiving an offer request)
        /// </summary>
        [HttpGet("assigned")]
        [Authorize(Roles = "SalesSupport,LegalManager,LegalEmployee")] // Roles that can receive requests
        public async Task<IActionResult> GetAssignedRequests([FromQuery] RequestStatus? status = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var requests = await _requestWorkflowService.GetAssignedRequestsAsync(userId, status, cancellationToken);
                return SuccessResponse(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assigned requests for user {UserId}", GetCurrentUserId());
                return ErrorResponse($"An error occurred while retrieving assigned requests: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Update the status of a request workflow
        /// </summary>
        [HttpPut("{workflowId}/status")]
        [Authorize(Roles = "SalesSupport,LegalManager,LegalEmployee")] // Roles that can update status
        public async Task<IActionResult> UpdateRequestWorkflowStatus(long workflowId, [FromBody] UpdateWorkflowRequestStatusDto updateDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _requestWorkflowService.UpdateWorkflowStatusAsync(workflowId, userId, updateDto, cancellationToken);
                if (result == null)
                {
                    return ErrorResponse("Request workflow not found or you don't have permission to update it", 404);
                }
                return SuccessResponse(result, "Request workflow status updated successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for updating workflow status");
                return ErrorResponse(ex.Message, 400);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access to update workflow status");
                return ErrorResponse(ex.Message, 403);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request workflow status {WorkflowId}", workflowId);
                return ErrorResponse("An error occurred while updating the request workflow status", 500);
            }
        }
    }
}