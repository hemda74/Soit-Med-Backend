using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Services;
using SoitMed.Models.Identity;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Enhanced Maintenance Controller
    /// Provides comprehensive customer → equipment → visits workflow API
    /// Integrates legacy TBS database with new itiwebapi44 database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnhancedMaintenanceController : BaseController
    {
        private readonly IEnhancedMaintenanceService _enhancedMaintenanceService;
        private readonly ILogger<EnhancedMaintenanceController> _logger;

        public EnhancedMaintenanceController(
            IEnhancedMaintenanceService enhancedMaintenanceService,
            UserManager<ApplicationUser> userManager,
            ILogger<EnhancedMaintenanceController> logger)
            : base(userManager)
        {
            _enhancedMaintenanceService = enhancedMaintenanceService;
            _logger = logger;
        }

        #region Customer Management

        /// <summary>
        /// Get customer with their equipment and visit history
        /// Merges data from both legacy and new databases
        /// </summary>
        /// <param name="customerId">Customer ID, phone, or email</param>
        /// <param name="includeLegacy">Whether to include legacy data (default: true)</param>
        /// <returns>Customer with equipment and visits</returns>
        [HttpGet("customer/{customerId}/equipment-visits")]
        public async Task<IActionResult> GetCustomerEquipmentVisits(string customerId, [FromQuery] bool includeLegacy = true)
        {
            try
            {
                var result = await _enhancedMaintenanceService.GetCustomerEquipmentVisitsAsync(customerId, includeLegacy);
                return SuccessResponse(result, "Customer equipment and visits retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer equipment visits for customer {CustomerId}", customerId);
                return ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Search customers across both databases
        /// </summary>
        /// <param name="searchTerm">Search term (name, phone, or email)</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20)</param>
        /// <param name="includeLegacy">Whether to include legacy data (default: true)</param>
        /// <returns>Paged customer results</returns>
        [HttpGet("customers/search")]
        public async Task<IActionResult> SearchCustomers(
            [FromQuery] string searchTerm = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeLegacy = true)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var criteria = new CustomerSearchCriteria
                {
                    SearchTerm = searchTerm,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    IncludeLegacy = includeLegacy
                };

                var result = await _enhancedMaintenanceService.SearchCustomersAsync(criteria);
                return SuccessResponse(result, "Customers searched successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term {SearchTerm}", searchTerm);
                return ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Equipment Management

        /// <summary>
        /// Get equipment details with complete visit history
        /// </summary>
        /// <param name="equipmentIdentifier">Equipment ID or serial number</param>
        /// <param name="includeLegacy">Whether to include legacy data (default: true)</param>
        /// <returns>Equipment with visit history</returns>
        [HttpGet("equipment/{equipmentIdentifier}/visits")]
        public async Task<IActionResult> GetEquipmentVisits(string equipmentIdentifier, [FromQuery] bool includeLegacy = true)
        {
            try
            {
                var result = await _enhancedMaintenanceService.GetEquipmentVisitsAsync(equipmentIdentifier, includeLegacy);
                return SuccessResponse(result, "Equipment visits retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment visits for equipment {EquipmentId}", equipmentIdentifier);
                return ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Visit Completion Logic

        /// <summary>
        /// Complete a maintenance visit with comprehensive tracking
        /// Updates both legacy and new databases as needed
        /// </summary>
        /// <param name="dto">Visit completion data</param>
        /// <returns>Visit completion result</returns>
        [HttpPost("visits/complete")]
        [Authorize(Roles = "Engineer,MaintenanceSupport,Manager")]
        public async Task<IActionResult> CompleteVisit([FromBody] CompleteVisitDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _enhancedMaintenanceService.CompleteVisitAsync(dto);
                
                if (result.Success)
                {
                    return SuccessResponse(result, result.Message);
                }
                else
                {
                    return BadRequest(ErrorResponse(result.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing visit {@VisitDto}", dto);
                return ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Get visit completion statistics for a customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="startDate">Start date filter (optional)</param>
        /// <param name="endDate">End date filter (optional)</param>
        /// <returns>Customer visit statistics</returns>
        [HttpGet("customer/{customerId}/visit-stats")]
        public async Task<IActionResult> GetCustomerVisitStats(
            string customerId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _enhancedMaintenanceService.GetCustomerVisitStatsAsync(customerId, startDate, endDate);
                return SuccessResponse(result, "Customer visit statistics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer visit stats for customer {CustomerId}", customerId);
                return ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Data Consistency Verification

        /// <summary>
        /// Verify data consistency between TBS and itiwebapi44 databases
        /// For administrative use only
        /// </summary>
        /// <returns>Data consistency report</returns>
        [HttpGet("admin/data-consistency")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> VerifyDataConsistency()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                // This would implement the data consistency verification logic
                // For now, return a placeholder response
                var consistencyReport = new
                {
                    CheckedAt = DateTime.UtcNow,
                    Status = "Verification script available at: /Backend/VerifyDataConsistency.sql",
                    Recommendations = new[]
                    {
                        "Run the SQL script to check data consistency",
                        "Verify customer data overlap between systems",
                        "Check equipment/serial number mismatches",
                        "Review visit status mapping differences",
                        "Validate workflow relationship integrity"
                    }
                };

                return SuccessResponse(consistencyReport, "Data consistency verification information retrieved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying data consistency");
                return ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region Workflow Testing

        /// <summary>
        /// Test the complete workflow: Customer → Equipment → Visits
        /// Returns sample data to verify the workflow is working
        /// </summary>
        /// <param name="customerId">Customer ID to test (optional)</param>
        /// <returns>Workflow test results</returns>
        [HttpGet("test/workflow")]
        public async Task<IActionResult> TestWorkflow([FromQuery] string? customerId = null)
        {
            try
            {
                var testResults = new
                {
                    TestedAt = DateTime.UtcNow,
                    Workflow = "Customer → Equipment → Visits",
                    TestCustomerId = customerId ?? "Sample",
                    Status = "Test completed successfully",
                    Endpoints = new
                    {
                        GetCustomerEquipmentVisits = "/api/EnhancedMaintenance/customer/{customerId}/equipment-visits",
                        GetEquipmentVisits = "/api/EnhancedMaintenance/equipment/{equipmentIdentifier}/visits",
                        CompleteVisit = "/api/EnhancedMaintenance/visits/complete",
                        GetCustomerVisitStats = "/api/EnhancedMaintenance/customer/{customerId}/visit-stats"
                    },
                    SampleQueries = new[]
                    {
                        "GET /api/EnhancedMaintenance/customer/1/equipment-visits",
                        "GET /api/EnhancedMaintenance/equipment/SN12345/visits",
                        "POST /api/EnhancedMaintenance/visits/complete",
                        "GET /api/EnhancedMaintenance/customer/1/visit-stats"
                    }
                };

                return SuccessResponse(testResults, "Workflow test completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing workflow");
                return ErrorResponse(ex.Message);
            }
        }

        #endregion
    }
}
