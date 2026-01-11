using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoitMed.DTO;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Temporarily for testing
    public class ComprehensiveMaintenanceController : ControllerBase
    {
        private readonly IComprehensiveMaintenanceService _maintenanceService;
        private readonly ILogger<ComprehensiveMaintenanceController> _logger;

        public ComprehensiveMaintenanceController(
            IComprehensiveMaintenanceService maintenanceService,
            ILogger<ComprehensiveMaintenanceController> logger)
        {
            _maintenanceService = maintenanceService;
            _logger = logger;
        }

        #region Customer Endpoints
        [HttpGet("customers/{customerId}/equipment-visits")]
        public async Task<ActionResult<CustomerEquipmentVisitsDTO>> GetCustomerEquipmentVisits(
            string customerId, [FromQuery] bool includeLegacy = true)
        {
            try
            {
                var result = await _maintenanceService.GetCustomerEquipmentVisitsAsync(customerId, includeLegacy);
                if (result == null)
                    return NotFound($"Customer with ID {customerId} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer equipment visits for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("customers/search")]
        public async Task<ActionResult<PagedResult<CustomerDTO>>> SearchCustomers([FromBody] CustomerSearchCriteria criteria)
        {
            try
            {
                var result = await _maintenanceService.SearchCustomersAsync(criteria);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with criteria: {@Criteria}", criteria);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("customers")]
        public async Task<ActionResult<CustomerDTO>> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            try
            {
                var result = await _maintenanceService.CreateCustomerAsync(request);
                return CreatedAtAction(nameof(GetCustomerEquipmentVisits), new { customerId = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer with request: {@Request}", request);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("customers/{customerId}")]
        public async Task<ActionResult<CustomerDTO>> UpdateCustomer(string customerId, [FromBody] UpdateCustomerRequest request)
        {
            try
            {
                var result = await _maintenanceService.UpdateCustomerAsync(customerId, request);
                if (result == null)
                    return NotFound($"Customer with ID {customerId} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("customers/{customerId}")]
        public async Task<ActionResult> DeleteCustomer(string customerId)
        {
            try
            {
                var result = await _maintenanceService.DeleteCustomerAsync(customerId);
                if (!result)
                    return NotFound($"Customer with ID {customerId} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("customers/{customerId}/statistics")]
        public async Task<ActionResult<CustomerVisitStats>> GetCustomerStatistics(
            string customerId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var result = await _maintenanceService.GetCustomerVisitStatisticsAsync(customerId, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region Equipment Endpoints
        [HttpGet("equipment/{equipmentId}")]
        public async Task<ActionResult<EquipmentDTO>> GetEquipment(string equipmentId)
        {
            try
            {
                var result = await _maintenanceService.GetEquipmentAsync(equipmentId);
                if (result == null)
                    return NotFound($"Equipment with ID {equipmentId} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment {EquipmentId}", equipmentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("customers/{customerId}/equipment")]
        public async Task<ActionResult<PagedResult<EquipmentDTO>>> GetCustomerEquipment(
            string customerId, [FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _maintenanceService.GetCustomerEquipmentAsync(customerId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("equipment")]
        public async Task<ActionResult<EquipmentDTO>> CreateEquipment([FromBody] CreateEquipmentRequest request)
        {
            try
            {
                var result = await _maintenanceService.CreateEquipmentAsync(request);
                return CreatedAtAction(nameof(GetEquipment), new { equipmentId = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating equipment with request: {@Request}", request);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("equipment/{equipmentId}")]
        public async Task<ActionResult<EquipmentDTO>> UpdateEquipment(string equipmentId, [FromBody] UpdateEquipmentRequest request)
        {
            try
            {
                var result = await _maintenanceService.UpdateEquipmentAsync(equipmentId, request);
                if (result == null)
                    return NotFound($"Equipment with ID {equipmentId} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating equipment {EquipmentId}", equipmentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("equipment/{equipmentId}")]
        public async Task<ActionResult> DeleteEquipment(string equipmentId)
        {
            try
            {
                var result = await _maintenanceService.DeleteEquipmentAsync(equipmentId);
                if (!result)
                    return NotFound($"Equipment with ID {equipmentId} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment {EquipmentId}", equipmentId);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region Visit Endpoints
        [HttpGet("visits/{visitId}")]
        public async Task<ActionResult<MaintenanceVisitDTO>> GetVisit(string visitId)
        {
            try
            {
                var result = await _maintenanceService.GetVisitAsync(visitId);
                if (result == null)
                    return NotFound($"Visit with ID {visitId} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visit {VisitId}", visitId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("equipment/{equipmentId}/visits")]
        public async Task<ActionResult<PagedResult<MaintenanceVisitDTO>>> GetEquipmentVisits(
            string equipmentId, [FromQuery] VisitSearchCriteria criteria)
        {
            try
            {
                var result = await _maintenanceService.GetEquipmentVisitsAsync(equipmentId, criteria);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting visits for equipment {EquipmentId}", equipmentId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("visits")]
        public async Task<ActionResult<MaintenanceVisitDTO>> CreateVisit([FromBody] CreateVisitRequest request)
        {
            try
            {
                var result = await _maintenanceService.CreateVisitAsync(request);
                return CreatedAtAction(nameof(GetVisit), new { visitId = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating visit with request: {@Request}", request);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("visits/{visitId}")]
        public async Task<ActionResult<MaintenanceVisitDTO>> UpdateVisit(string visitId, [FromBody] UpdateVisitRequest request)
        {
            try
            {
                var result = await _maintenanceService.UpdateVisitAsync(visitId, request);
                if (result == null)
                    return NotFound($"Visit with ID {visitId} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating visit {VisitId}", visitId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("visits/{visitId}")]
        public async Task<ActionResult> DeleteVisit(string visitId)
        {
            try
            {
                var result = await _maintenanceService.DeleteVisitAsync(visitId);
                if (!result)
                    return NotFound($"Visit with ID {visitId} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting visit {VisitId}", visitId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("visits/{visitId}/complete")]
        public async Task<ActionResult<VisitCompletionResponse>> CompleteVisit(string visitId, [FromBody] CompleteVisitRequest request)
        {
            try
            {
                var result = await _maintenanceService.CompleteVisitAsync(visitId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing visit {VisitId}", visitId);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region Contract Endpoints
        [HttpGet("contracts/{contractId}")]
        public async Task<ActionResult<MaintenanceContractDTO>> GetContract(string contractId)
        {
            try
            {
                var result = await _maintenanceService.GetContractAsync(contractId);
                if (result == null)
                    return NotFound($"Contract with ID {contractId} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract {ContractId}", contractId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("customers/{customerId}/contracts")]
        public async Task<ActionResult<PagedResult<MaintenanceContractDTO>>> GetCustomerContracts(
            string customerId, [FromQuery] PagedRequest request)
        {
            try
            {
                var result = await _maintenanceService.GetCustomerContractsAsync(customerId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contracts for customer {CustomerId}", customerId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("contracts")]
        public async Task<ActionResult<MaintenanceContractDTO>> CreateContract([FromBody] CreateContractRequest request)
        {
            try
            {
                var result = await _maintenanceService.CreateContractAsync(request);
                return CreatedAtAction(nameof(GetContract), new { contractId = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract with request: {@Request}", request);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("contracts/{contractId}")]
        public async Task<ActionResult<MaintenanceContractDTO>> UpdateContract(string contractId, [FromBody] UpdateContractRequest request)
        {
            try
            {
                var result = await _maintenanceService.UpdateContractAsync(contractId, request);
                if (result == null)
                    return NotFound($"Contract with ID {contractId} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract {ContractId}", contractId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("contracts/{contractId}")]
        public async Task<ActionResult> DeleteContract(string contractId)
        {
            try
            {
                var result = await _maintenanceService.DeleteContractAsync(contractId);
                if (!result)
                    return NotFound($"Contract with ID {contractId} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {ContractId}", contractId);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion

        #region Dashboard & Statistics
        [HttpGet("dashboard/statistics")]
        public async Task<ActionResult<MaintenanceDashboardStats>> GetDashboardStatistics()
        {
            try
            {
                var result = await _maintenanceService.GetMaintenanceDashboardStatsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public ActionResult TestEndpoint()
        {
            return Ok(new { message = "Comprehensive Maintenance API is working", timestamp = DateTime.UtcNow });
        }
        #endregion
    }
}
