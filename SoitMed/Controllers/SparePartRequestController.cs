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
    public class SparePartRequestController : BaseController
    {
        private readonly ISparePartRequestService _sparePartRequestService;
        private readonly ILogger<SparePartRequestController> _logger;

        public SparePartRequestController(
            ISparePartRequestService sparePartRequestService,
            UserManager<ApplicationUser> userManager,
            ILogger<SparePartRequestController> logger)
            : base(userManager)
        {
            _sparePartRequestService = sparePartRequestService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Engineer")]
        public async Task<IActionResult> CreateSparePartRequest([FromBody] CreateSparePartRequestDTO dto, [FromQuery] int maintenanceVisitId)
        {
            try
            {
                var result = await _sparePartRequestService.CreateSparePartRequestAsync(dto, maintenanceVisitId);
                return SuccessResponse(result, "Spare part request created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating spare part request");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSparePartRequest(int id)
        {
            try
            {
                var result = await _sparePartRequestService.GetSparePartRequestAsync(id);
                if (result == null)
                    return NotFound();

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting spare part request {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("maintenance-request/{maintenanceRequestId}")]
        public async Task<IActionResult> GetByMaintenanceRequest(int maintenanceRequestId)
        {
            try
            {
                var result = await _sparePartRequestService.GetByMaintenanceRequestAsync(maintenanceRequestId);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting spare part requests for maintenance {RequestId}", maintenanceRequestId);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/check-availability")]
        [Authorize(Roles = "SparePartsCoordinator")]
        public async Task<IActionResult> CheckAvailability(int id, [FromBody] bool isLocalAvailable)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _sparePartRequestService.CheckAvailabilityAsync(id, userId, isLocalAvailable);
                return SuccessResponse(result, "Availability checked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for spare part {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/set-price")]
        [Authorize(Roles = "MaintenanceManager")]
        public async Task<IActionResult> SetPrice(int id, [FromBody] UpdateSparePartPriceDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _sparePartRequestService.SetPriceAsync(id, dto, userId);
                return SuccessResponse(result, "Price set successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting price for spare part {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/customer-decision")]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> CustomerDecision(int id, [FromBody] CustomerSparePartDecisionDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _sparePartRequestService.CustomerDecisionAsync(id, dto, userId);
                return SuccessResponse(result, dto.Approved ? "Spare part approved" : "Spare part rejected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing customer decision for spare part {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/warehouse-approval")]
        [Authorize(Roles = "WarehouseKeeper")]
        public async Task<IActionResult> WarehouseApproval(int id, [FromBody] WarehouseApprovalDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _sparePartRequestService.WarehouseApprovalAsync(id, dto, userId);
                return SuccessResponse(result, dto.Approved ? "Spare part approved" : "Spare part rejected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing warehouse approval for spare part {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/mark-ready")]
        [Authorize(Roles = "InventoryManager")]
        public async Task<IActionResult> MarkAsReady(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _sparePartRequestService.MarkAsReadyAsync(id, userId);
                return SuccessResponse(result, "Spare part marked as ready");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking spare part as ready {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/mark-delivered")]
        [Authorize(Roles = "Engineer")]
        public async Task<IActionResult> MarkAsDeliveredToEngineer(int id)
        {
            try
            {
                var result = await _sparePartRequestService.MarkAsDeliveredToEngineerAsync(id);
                return SuccessResponse(result, "Spare part marked as delivered");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking spare part as delivered {RequestId}", id);
                return ErrorResponse(ex.Message);
            }
        }
    }
}

