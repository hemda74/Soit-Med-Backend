using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Admin endpoint for triggering contract migration from TBS to ITIWebApi44
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ContractMigrationController : ControllerBase
    {
        private readonly IContractMigrationService _migrationService;
        private readonly ILogger<ContractMigrationController> _logger;

        public ContractMigrationController(
            IContractMigrationService migrationService,
            ILogger<ContractMigrationController> logger)
        {
            _migrationService = migrationService;
            _logger = logger;
        }

        /// <summary>
        /// Migrate all contracts from TBS to ITIWebApi44
        /// </summary>
        [HttpPost("migrate-all")]
        public async Task<IActionResult> MigrateAllContracts()
        {
            try
            {
                var adminUserId = User.Identity?.Name ?? "System";
                var result = await _migrationService.MigrateAllContractsAsync(adminUserId);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = new
                        {
                            contractsMigrated = result.ContractsMigrated,
                            installmentsMigrated = result.InstallmentsMigrated,
                            negotiationsCreated = result.NegotiationsCreated,
                            errors = result.Errors
                        }
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Migration completed with errors",
                        errors = result.ErrorMessages,
                        data = new
                        {
                            contractsMigrated = result.ContractsMigrated,
                            installmentsMigrated = result.InstallmentsMigrated,
                            negotiationsCreated = result.NegotiationsCreated,
                            errors = result.Errors
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during contract migration");
                return StatusCode(500, new { success = false, message = "Internal server error during migration", error = ex.Message });
            }
        }

        /// <summary>
        /// Migrate a specific contract by legacy ID
        /// </summary>
        [HttpPost("migrate/{legacyContractId}")]
        public async Task<IActionResult> MigrateContract(int legacyContractId)
        {
            try
            {
                var adminUserId = User.Identity?.Name ?? "System";
                var result = await _migrationService.MigrateContractAsync(legacyContractId, adminUserId);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = new
                        {
                            contractsMigrated = result.ContractsMigrated,
                            installmentsMigrated = result.InstallmentsMigrated,
                            negotiationsCreated = result.NegotiationsCreated
                        }
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Migration failed",
                        errors = result.ErrorMessages
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating contract {ContractId}", legacyContractId);
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get migration statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _migrationService.GetMigrationStatisticsAsync();
                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration statistics");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }
    }
}

