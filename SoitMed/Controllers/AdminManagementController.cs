using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.DTO;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Enhanced Admin controller for comprehensive user and client management
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminManagementController : ControllerBase
    {
        private readonly IAdminManagementService _adminService;
        private readonly ILogger<AdminManagementController> _logger;

        public AdminManagementController(
            IAdminManagementService adminService,
            ILogger<AdminManagementController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Get all clients with email status flags
        /// GET /api/AdminManagement/clients/with-email-status
        /// </summary>
        [HttpGet("clients/with-email-status")]
        public async Task<IActionResult> GetClientsWithEmailStatus(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? emailStatus = null)
        {
            try
            {
                var result = await _adminService.GetClientsWithEmailStatusAsync(
                    page, pageSize, searchTerm, emailStatus);
                
                return Ok(new
                {
                    success = true,
                    message = "Clients retrieved successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients with email status");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving clients",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Add or update client email (Admin/SuperAdmin only)
        /// POST /api/AdminManagement/clients/{clientId}/email
        /// </summary>
        [HttpPost("clients/{clientId}/email")]
        public async Task<IActionResult> SetClientEmail(
            long clientId, 
            [FromBody] SetClientEmailDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _adminService.SetClientEmailAsync(clientId, dto, User.Identity?.Name);
                
                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                _logger.LogInformation("Client {ClientId} email set by {AdminUser}", 
                    clientId, User.Identity?.Name);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting email for client {ClientId}", clientId);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while setting client email",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Update client contact person information (Admin/SuperAdmin only)
        /// POST /api/AdminManagement/clients/{clientId}/contact-person
        /// </summary>
        [HttpPost("clients/{clientId}/contact-person")]
        public async Task<IActionResult> UpdateContactPerson(
            long clientId, 
            [FromBody] UpdateContactPersonDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _adminService.UpdateContactPersonAsync(clientId, dto, User.Identity?.Name);
                
                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                _logger.LogInformation("Client {ClientId} contact person updated by {AdminUser}", 
                    clientId, User.Identity?.Name);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact person for client {ClientId}", clientId);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while updating contact person",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get client email creation history
        /// GET /api/AdminManagement/clients/{clientId}/email-history
        /// </summary>
        [HttpGet("clients/{clientId}/email-history")]
        public async Task<IActionResult> GetClientEmailHistory(long clientId)
        {
            try
            {
                var history = await _adminService.GetClientEmailHistoryAsync(clientId);
                
                return Ok(new
                {
                    success = true,
                    message = "Email history retrieved successfully",
                    data = history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting email history for client {ClientId}", clientId);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving email history",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get users management dashboard with email statistics
        /// GET /api/AdminManagement/dashboard
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            try
            {
                var dashboard = await _adminService.GetAdminDashboardAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "Dashboard data retrieved successfully",
                    data = dashboard
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving dashboard data",
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Deactivate/Activate user account (Admin/SuperAdmin only)
        /// POST /api/AdminManagement/users/{userId}/toggle-status
        /// </summary>
        [HttpPost("users/{userId}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            try
            {
                var result = await _adminService.ToggleUserStatusAsync(userId, User.Identity?.Name);
                
                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                _logger.LogInformation("User {UserId} status toggled by {AdminUser}", 
                    userId, User.Identity?.Name);

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling status for user {UserId}", userId);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while toggling user status",
                    error = ex.Message 
                });
            }
        }
    }
}
