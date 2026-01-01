using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Controller for triggering legacy data import
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin,MaintenanceManager")]
    public class LegacyImportController : ControllerBase
    {
        private readonly ILegacyImporterService _importerService;
        private readonly ILogger<LegacyImportController> _logger;

        public LegacyImportController(
            ILegacyImporterService importerService,
            ILogger<LegacyImportController> logger)
        {
            _importerService = importerService;
            _logger = logger;
        }

        /// <summary>
        /// Import all legacy data (Clients, Equipment, MaintenanceVisits)
        /// </summary>
        [HttpPost("import-all")]
        public async Task<IActionResult> ImportAll(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Legacy import all triggered by user: {UserId}", User.Identity?.Name);
                var result = await _importerService.ImportAllAsync(cancellationToken);

                return Ok(new
                {
                    success = true,
                    message = "Legacy import completed",
                    data = new
                    {
                        successCount = result.SuccessCount,
                        failureCount = result.FailureCount,
                        skippedCount = result.SkippedCount,
                        logFilePath = result.LogFilePath,
                        errors = result.Errors.Take(50) // Return first 50 errors
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during legacy import");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Import only clients from legacy system
        /// </summary>
        [HttpPost("import-clients")]
        public async Task<IActionResult> ImportClients(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Legacy import clients triggered by user: {UserId}", User.Identity?.Name);
                var result = await _importerService.ImportClientsAsync(cancellationToken);

                return Ok(new
                {
                    success = true,
                    message = "Clients import completed",
                    data = new
                    {
                        successCount = result.SuccessCount,
                        failureCount = result.FailureCount,
                        skippedCount = result.SkippedCount,
                        logFilePath = result.LogFilePath,
                        errors = result.Errors.Take(50)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during clients import");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Import only equipment from legacy system
        /// </summary>
        [HttpPost("import-equipment")]
        public async Task<IActionResult> ImportEquipment(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Legacy import equipment triggered by user: {UserId}", User.Identity?.Name);
                var result = await _importerService.ImportEquipmentAsync(cancellationToken);

                return Ok(new
                {
                    success = true,
                    message = "Equipment import completed",
                    data = new
                    {
                        successCount = result.SuccessCount,
                        failureCount = result.FailureCount,
                        skippedCount = result.SkippedCount,
                        logFilePath = result.LogFilePath,
                        errors = result.Errors.Take(50)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during equipment import");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Import only maintenance visits from legacy system
        /// </summary>
        [HttpPost("import-visits")]
        public async Task<IActionResult> ImportVisits(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Legacy import visits triggered by user: {UserId}", User.Identity?.Name);
                var result = await _importerService.ImportMaintenanceVisitsAsync(cancellationToken);

                return Ok(new
                {
                    success = true,
                    message = "Maintenance visits import completed",
                    data = new
                    {
                        successCount = result.SuccessCount,
                        failureCount = result.FailureCount,
                        skippedCount = result.SkippedCount,
                        logFilePath = result.LogFilePath,
                        errors = result.Errors.Take(50)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during visits import");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}

