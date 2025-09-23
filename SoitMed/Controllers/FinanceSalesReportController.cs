using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;
using SoitMed.Validators;
using FluentValidation;

namespace SoitMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinanceSalesReportController : ControllerBase
    {
        private readonly ISalesReportService _salesReportService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IValidator<CreateSalesReportDto> _createValidator;
        private readonly IValidator<UpdateSalesReportDto> _updateValidator;
        private readonly IValidator<FilterSalesReportsDto> _filterValidator;
        private readonly IValidator<RateSalesReportDto> _rateValidator;

        public FinanceSalesReportController(
            ISalesReportService salesReportService,
            UserManager<ApplicationUser> userManager,
            IValidator<CreateSalesReportDto> createValidator,
            IValidator<UpdateSalesReportDto> updateValidator,
            IValidator<FilterSalesReportsDto> filterValidator,
            IValidator<RateSalesReportDto> rateValidator)
        {
            _salesReportService = salesReportService;
            _userManager = userManager;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _filterValidator = filterValidator;
            _rateValidator = rateValidator;
        }

        /// <summary>
        /// Create a new sales report (FinanceEmployee only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "FinanceEmployee")]
        public async Task<IActionResult> CreateReport([FromBody] CreateSalesReportDto createDto, CancellationToken cancellationToken = default)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed. Please check the following fields:",
                    errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    timestamp = DateTime.UtcNow
                });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _salesReportService.CreateReportAsync(createDto, userId, cancellationToken);
            if (result == null)
            {
                return Conflict(new
                {
                    success = false,
                    message = "A report with the same type and date already exists for this employee.",
                    timestamp = DateTime.UtcNow
                });
            }

            return CreatedAtAction(nameof(GetReportById), new { id = result.Id }, new
            {
                success = true,
                data = result,
                message = "Finance sales report created successfully",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Update an existing sales report (FinanceEmployee only - own reports)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "FinanceEmployee")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] UpdateSalesReportDto updateDto, CancellationToken cancellationToken = default)
        {
            var validationResult = await _updateValidator.ValidateAsync(updateDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed. Please check the following fields:",
                    errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    timestamp = DateTime.UtcNow
                });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _salesReportService.UpdateReportAsync(id, updateDto, userId, cancellationToken);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Report not found or you don't have permission to update it.",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                success = true,
                data = result,
                message = "Finance sales report updated successfully",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Delete a sales report (FinanceEmployee only - own reports)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "FinanceEmployee")]
        public async Task<IActionResult> DeleteReport(int id, CancellationToken cancellationToken = default)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _salesReportService.DeleteReportAsync(id, userId, cancellationToken);
            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Report not found or you don't have permission to delete it.",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                success = true,
                message = "Finance sales report deleted successfully",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get a specific sales report by ID (FinanceEmployee - own reports only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "FinanceEmployee")]
        public async Task<IActionResult> GetReportById(int id, CancellationToken cancellationToken = default)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _salesReportService.GetReportByIdForEmployeeAsync(id, userId, cancellationToken);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Report not found or you don't have permission to view it.",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                success = true,
                data = result,
                message = "Finance sales report retrieved successfully",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get finance sales reports with optional filtering (FinanceEmployee - own reports only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "FinanceEmployee")]
        public async Task<IActionResult> GetReports([FromQuery] FilterSalesReportsDto filterDto, CancellationToken cancellationToken = default)
        {
            var validationResult = await _filterValidator.ValidateAsync(filterDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed. Please check the following fields:",
                    errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    timestamp = DateTime.UtcNow
                });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _salesReportService.GetReportsForEmployeeAsync(userId, filterDto, cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                message = $"Found {result.TotalCount} finance sales report(s)",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get all finance sales reports (FinanceManager only)
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "FinanceManager")]
        public async Task<IActionResult> GetAllFinanceReports([FromQuery] FilterSalesReportsDto filterDto, CancellationToken cancellationToken = default)
        {
            var validationResult = await _filterValidator.ValidateAsync(filterDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed. Please check the following fields:",
                    errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    timestamp = DateTime.UtcNow
                });
            }

            var result = await _salesReportService.GetReportsAsync(filterDto, cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                message = $"Found {result.TotalCount} finance sales report(s)",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Rate a finance sales report (FinanceManager only)
        /// </summary>
        [HttpPost("{id}/rate")]
        [Authorize(Roles = "FinanceManager")]
        public async Task<IActionResult> RateReport(int id, [FromBody] RateSalesReportDto rateDto, CancellationToken cancellationToken = default)
        {
            var validationResult = await _rateValidator.ValidateAsync(rateDto, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validation failed. Please check the following fields:",
                    errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()),
                    timestamp = DateTime.UtcNow
                });
            }

            var result = await _salesReportService.RateReportAsync(id, rateDto, cancellationToken);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Report not found.",
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                success = true,
                data = result,
                message = "Finance sales report rated successfully",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
