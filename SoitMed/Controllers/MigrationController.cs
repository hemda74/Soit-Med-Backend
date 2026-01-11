using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.DTO;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Migration Controller - MCP-like API for Equipment-to-Client linking operations
    /// Provides endpoints for migration, diagnostics, and verification
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only admins can run migration operations
    public class MigrationController : ControllerBase
    {
        private readonly IMigrationService _migrationService;
        private readonly ILegacyEmployeeMigrationService _legacyEmployeeMigrationService;
        private readonly ILogger<MigrationController> _logger;

        public MigrationController(
            IMigrationService migrationService,
            ILegacyEmployeeMigrationService legacyEmployeeMigrationService,
            ILogger<MigrationController> logger)
        {
            _migrationService = migrationService;
            _legacyEmployeeMigrationService = legacyEmployeeMigrationService;
            _logger = logger;
        }

        /// <summary>
        /// Link equipment to clients using all 4 methods
        /// POST /api/Migration/link-equipment
        /// </summary>
        [HttpPost("link-equipment")]
        [ProducesResponseType(typeof(EquipmentLinkingResultDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<EquipmentLinkingResultDto>> LinkEquipmentToClients(
            [FromQuery] string? adminUserId = null)
        {
            try
            {
                _logger.LogInformation("Starting equipment linking operation. AdminUserId: {AdminUserId}", adminUserId);
                
                var result = await _migrationService.LinkEquipmentToClientsAsync(adminUserId);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LinkEquipmentToClients endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Link equipment via Visits (Method 1)
        /// POST /api/Migration/link-via-visits
        /// </summary>
        [HttpPost("link-via-visits")]
        [ProducesResponseType(typeof(LinkingMethodResultDto), 200)]
        public async Task<ActionResult<LinkingMethodResultDto>> LinkViaVisits(
            [FromQuery] string? adminUserId = null)
        {
            try
            {
                var result = await _migrationService.LinkViaVisitsAsync(adminUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LinkViaVisits endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Link equipment via Maintenance Contracts (Method 2)
        /// POST /api/Migration/link-via-contracts
        /// </summary>
        [HttpPost("link-via-contracts")]
        [ProducesResponseType(typeof(LinkingMethodResultDto), 200)]
        public async Task<ActionResult<LinkingMethodResultDto>> LinkViaMaintenanceContracts(
            [FromQuery] string? adminUserId = null)
        {
            try
            {
                var result = await _migrationService.LinkViaMaintenanceContractsAsync(adminUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LinkViaMaintenanceContracts endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Link equipment via Sales Invoices (Method 3)
        /// POST /api/Migration/link-via-sales-invoices
        /// </summary>
        [HttpPost("link-via-sales-invoices")]
        [ProducesResponseType(typeof(LinkingMethodResultDto), 200)]
        public async Task<ActionResult<LinkingMethodResultDto>> LinkViaSalesInvoices(
            [FromQuery] string? adminUserId = null)
        {
            try
            {
                var result = await _migrationService.LinkViaSalesInvoicesAsync(adminUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LinkViaSalesInvoices endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Link equipment via Order Out (Method 4)
        /// POST /api/Migration/link-via-order-out
        /// </summary>
        [HttpPost("link-via-order-out")]
        [ProducesResponseType(typeof(LinkingMethodResultDto), 200)]
        public async Task<ActionResult<LinkingMethodResultDto>> LinkViaOrderOut(
            [FromQuery] string? adminUserId = null)
        {
            try
            {
                var result = await _migrationService.LinkViaOrderOutAsync(adminUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LinkViaOrderOut endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get diagnostic statistics about equipment linking
        /// GET /api/Migration/diagnostics
        /// </summary>
        [HttpGet("diagnostics")]
        [ProducesResponseType(typeof(EquipmentLinkingDiagnosticsDto), 200)]
        public async Task<ActionResult<EquipmentLinkingDiagnosticsDto>> GetDiagnostics()
        {
            try
            {
                var diagnostics = await _migrationService.GetDiagnosticsAsync();
                return Ok(diagnostics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDiagnostics endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get unlinked equipment report
        /// GET /api/Migration/unlinked-equipment?pageNumber=1&pageSize=50
        /// </summary>
        [HttpGet("unlinked-equipment")]
        [ProducesResponseType(typeof(UnlinkedEquipmentReportDto), 200)]
        public async Task<ActionResult<UnlinkedEquipmentReportDto>> GetUnlinkedEquipment(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var report = await _migrationService.GetUnlinkedEquipmentReportAsync(pageNumber, pageSize);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUnlinkedEquipment endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Verify equipment linking for a specific client
        /// GET /api/Migration/verify-client/{clientId}
        /// </summary>
        [HttpGet("verify-client/{clientId}")]
        [ProducesResponseType(typeof(ClientEquipmentVerificationDto), 200)]
        public async Task<ActionResult<ClientEquipmentVerificationDto>> VerifyClientEquipment(long clientId)
        {
            try
            {
                var verification = await _migrationService.VerifyClientEquipmentAsync(clientId);
                return Ok(verification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyClientEquipment endpoint for client {ClientId}", clientId);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

