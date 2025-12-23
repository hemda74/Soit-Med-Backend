using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaintenanceRequestController : BaseController
    {
        private readonly IMaintenanceRequestService _maintenanceRequestService;
        private readonly ILogger<MaintenanceRequestController> _logger;

        public MaintenanceRequestController(
            IMaintenanceRequestService maintenanceRequestService,
            UserManager<ApplicationUser> userManager,
            ILogger<MaintenanceRequestController> logger)
            : base(userManager)
        {
            _maintenanceRequestService = maintenanceRequestService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> CreateMaintenanceRequest([FromBody] CreateMaintenanceRequestDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _maintenanceRequestService.CreateMaintenanceRequestAsync(dto, userId);
                return SuccessResponse(result, "Maintenance request created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance request");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMaintenanceRequest(int id)
        {
            try
            {
                var result = await _maintenanceRequestService.GetMaintenanceRequestAsync(id);
                if (result == null)
                    return NotFound();

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance request {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("customer/my-requests")]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> GetMyRequests()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _maintenanceRequestService.GetCustomerRequestsAsync(userId);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer requests");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("Engineer/my-assigned")]
        [Authorize(Roles = "Engineer")]
        public async Task<IActionResult> GetMyAssignedRequests()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _maintenanceRequestService.GetEngineerRequestsAsync(userId);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Engineer requests");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "MaintenanceSupport,MaintenanceManager,SuperAdmin")]
        public async Task<IActionResult> GetPendingRequests()
        {
            try
            {
                var result = await _maintenanceRequestService.GetPendingRequestsAsync();
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending requests");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/assign")]
        [Authorize(Roles = "MaintenanceSupport,MaintenanceManager,SuperAdmin")]
        public async Task<IActionResult> AssignToEngineer(int id, [FromBody] AssignMaintenanceRequestDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _maintenanceRequestService.AssignToEngineerAsync(id, dto, userId);
                return SuccessResponse(result, "Request assigned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning request {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "MaintenanceSupport,MaintenanceManager,Engineer,SuperAdmin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateMaintenanceRequestStatusDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _maintenanceRequestService.UpdateStatusAsync(id, dto.Status, dto.Notes);
                return SuccessResponse(result, "Status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for request {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "MaintenanceSupport,MaintenanceManager,Doctor,Technician,Manager,SuperAdmin")]
        public async Task<IActionResult> CancelRequest(int id, [FromBody] CancelMaintenanceRequestDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _maintenanceRequestService.CancelRequestAsync(id, userId, dto.Reason);
                return SuccessResponse(result, "Request cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling request {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }
    }
}

