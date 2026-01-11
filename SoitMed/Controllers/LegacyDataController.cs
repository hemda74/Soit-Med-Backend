using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoitMed.DTO;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    /// <summary>
    /// Legacy Data Controller - Provides endpoints to access TBS data using the same logic as MediaApi
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")] // Only admins and super admins can access legacy data
    public class LegacyDataController : ControllerBase
    {
        private readonly ILegacyDataSyncService _legacyDataSyncService;
        private readonly ILogger<LegacyDataController> _logger;

        public LegacyDataController(
            ILegacyDataSyncService legacyDataSyncService,
            ILogger<LegacyDataController> logger)
        {
            _legacyDataSyncService = legacyDataSyncService;
            _logger = logger;
        }

        /// <summary>
        /// Get customer machines from TBS (same as MediaApi GetMachinesByCustomerIdAsync)
        /// GET /api/LegacyData/customer/{legacyCustomerId}/machines
        /// </summary>
        [HttpGet("customer/{legacyCustomerId}/machines")]
        [ProducesResponseType(typeof(CustomerMachinesSyncDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CustomerMachinesSyncDto>> GetCustomerMachines(int legacyCustomerId)
        {
            try
            {
                var result = await _legacyDataSyncService.GetCustomerMachinesFromTbsAsync(legacyCustomerId);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Customer {legacyCustomerId} not found in TBS" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer machines for customer {CustomerId}", legacyCustomerId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get visits for a machine from TBS (same as MediaApi GetMachineHistoryAsync)
        /// GET /api/LegacyData/machine/{ooiId}/visits
        /// </summary>
        [HttpGet("machine/{ooiId}/visits")]
        [ProducesResponseType(typeof(MachineVisitsSyncDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MachineVisitsSyncDto>> GetMachineVisits(int ooiId)
        {
            try
            {
                var result = await _legacyDataSyncService.GetMachineVisitsFromTbsAsync(ooiId);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Machine {ooiId} not found in TBS" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting machine visits for machine {OoiId}", ooiId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync customer machines from TBS to ITIWebApi44
        /// POST /api/LegacyData/customer/{legacyCustomerId}/sync
        /// </summary>
        [HttpPost("customer/{legacyCustomerId}/sync")]
        [ProducesResponseType(typeof(SyncResultDto), 200)]
        public async Task<ActionResult<SyncResultDto>> SyncCustomerMachines(int legacyCustomerId)
        {
            try
            {
                var result = await _legacyDataSyncService.SyncCustomerMachinesToNewSystemAsync(legacyCustomerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing customer machines for customer {CustomerId}", legacyCustomerId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync visits for a machine from TBS to ITIWebApi44
        /// POST /api/LegacyData/machine/{ooiId}/sync-visits
        /// </summary>
        [HttpPost("machine/{ooiId}/sync-visits")]
        [ProducesResponseType(typeof(SyncResultDto), 200)]
        public async Task<ActionResult<SyncResultDto>> SyncMachineVisits(int ooiId)
        {
            try
            {
                var result = await _legacyDataSyncService.SyncMachineVisitsToNewSystemAsync(ooiId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing machine visits for machine {OoiId}", ooiId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync all customers with their machines and visits from TBS to ITIWebApi44
        /// POST /api/LegacyData/sync-all?batchSize=10
        /// </summary>
        [HttpPost("sync-all")]
        [ProducesResponseType(typeof(BulkSyncResultDto), 200)]
        public async Task<ActionResult<BulkSyncResultDto>> SyncAllCustomersData([FromQuery] int? batchSize = null)
        {
            try
            {
                _logger.LogInformation("Starting bulk sync for all customers. Batch size: {BatchSize}", batchSize ?? 10);
                var result = await _legacyDataSyncService.SyncAllCustomersDataAsync(batchSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk sync");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all customers from TBS (paginated)
        /// GET /api/LegacyData/customers?pageNumber=1&pageSize=50
        /// </summary>
        [HttpGet("customers")]
        [ProducesResponseType(typeof(List<LegacyCustomerDto>), 200)]
        public async Task<ActionResult<List<LegacyCustomerDto>>> GetAllCustomers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var customers = await _legacyDataSyncService.GetAllCustomersFromTbsAsync(pageNumber, pageSize);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers from TBS");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Sync all customers from TBS to ITIWebApi44 (creates clients if missing)
        /// POST /api/LegacyData/customers/sync?batchSize=50
        /// </summary>
        [HttpPost("customers/sync")]
        [ProducesResponseType(typeof(BulkSyncResultDto), 200)]
        public async Task<ActionResult<BulkSyncResultDto>> SyncAllCustomersOnly([FromQuery] int? batchSize = null)
        {
            try
            {
                _logger.LogInformation("Starting customer sync. Batch size: {BatchSize}", batchSize ?? 50);
                var result = await _legacyDataSyncService.SyncAllCustomersFromTbsAsync(batchSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing customers");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

