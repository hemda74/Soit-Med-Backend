using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaintenanceVisitController : BaseController
    {
        private readonly IMaintenanceVisitService _maintenanceVisitService;
        private readonly ILogger<MaintenanceVisitController> _logger;

        public MaintenanceVisitController(
            IMaintenanceVisitService maintenanceVisitService,
            UserManager<ApplicationUser> userManager,
            ILogger<MaintenanceVisitController> logger)
            : base(userManager)
        {
            _maintenanceVisitService = maintenanceVisitService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Engineer")]
        public async Task<IActionResult> CreateVisit([FromBody] CreateMaintenanceVisitDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _maintenanceVisitService.CreateVisitAsync(dto, userId);
                return SuccessResponse(result, "Maintenance visit created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance visit");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVisit(int id)
        {
            try
            {
                var result = await _maintenanceVisitService.GetVisitAsync(id);
                if (result == null)
                    return NotFound();

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visit {VisitId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("request/{maintenanceRequestId}")]
        public async Task<IActionResult> GetVisitsByRequest(int maintenanceRequestId)
        {
            try
            {
                var result = await _maintenanceVisitService.GetVisitsByRequestAsync(maintenanceRequestId);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visits for request {RequestId}", maintenanceRequestId);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("engineer/my-visits")]
        [Authorize(Roles = "Engineer")]
        public async Task<IActionResult> GetMyVisits()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _maintenanceVisitService.GetVisitsByEngineerAsync(userId);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting engineer visits");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("equipment/qr/{qrCode}")]
        [Authorize(Roles = "Engineer")]
        public async Task<IActionResult> GetEquipmentByQRCode(string qrCode)
        {
            try
            {
                var result = await _maintenanceVisitService.GetEquipmentByQRCodeAsync(qrCode);
                if (result == null)
                    return NotFound("Equipment not found");

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment by QR code");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("equipment/serial/{serialCode}")]
        [Authorize(Roles = "Engineer")]
        public async Task<IActionResult> GetEquipmentBySerialCode(string serialCode)
        {
            try
            {
                var result = await _maintenanceVisitService.GetEquipmentBySerialCodeAsync(serialCode);
                if (result == null)
                    return NotFound("Equipment not found");

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment by serial code");
                return ErrorResponse(ex.Message);
            }
        }
    }
}

