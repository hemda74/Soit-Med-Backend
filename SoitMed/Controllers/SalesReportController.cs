using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;
using SoitMed.Validators;
using FluentValidation;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    public class SalesReportController : BaseController
    {
        private readonly ISalesReportService _salesReportService;
        private readonly IValidator<CreateSalesReportDto> _createValidator;
        private readonly IValidator<UpdateSalesReportDto> _updateValidator;
        private readonly IValidator<FilterSalesReportsDto> _filterValidator;
        private readonly IValidator<RateSalesReportDto> _rateValidator;

        public SalesReportController(
            ISalesReportService salesReportService,
            UserManager<ApplicationUser> userManager,
            IValidator<CreateSalesReportDto> createValidator,
            IValidator<UpdateSalesReportDto> updateValidator,
            IValidator<FilterSalesReportsDto> filterValidator,
            IValidator<RateSalesReportDto> rateValidator) : base(userManager)
        {
            _salesReportService = salesReportService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _filterValidator = filterValidator;
            _rateValidator = rateValidator;
        }

        /// <summary>
        /// Create a new sales report (SalesEmployee only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> CreateReport([FromBody] CreateSalesReportDto createDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(createDto, _createValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _salesReportService.CreateReportAsync(createDto, userId, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("A report with the same type and date already exists for this employee.", 409);
            }

            return CreatedAtAction(nameof(GetReportById), new { id = result.Id }, 
                CreateSuccessResponse(result, "Report created successfully"));
        }

        /// <summary>
        /// Update an existing sales report (SalesEmployee only - own reports)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] UpdateSalesReportDto updateDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(updateDto, _updateValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _salesReportService.UpdateReportAsync(id, updateDto, userId, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Report not found or you don't have permission to update it.", 404);
            }

            return SuccessResponse(result, "Report updated successfully");
        }

        /// <summary>
        /// Delete a sales report (SalesEmployee only - own reports)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Salesman")]
        public async Task<IActionResult> DeleteReport(int id, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _salesReportService.DeleteReportAsync(id, userId, cancellationToken);
            if (!result)
            {
                return ErrorResponse("Report not found or you don't have permission to delete it.", 404);
            }

            return SuccessResponse(message: "Report deleted successfully");
        }

        /// <summary>
        /// Get a specific sales report by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReportById(int id, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var (user, authError) = await ControllerAuthorizationHelper.GetCurrentUserAsync(userId, UserManager);
            if (authError != null)
                return authError;

            var isManager = await ControllerAuthorizationHelper.IsManagerAsync(user!, UserManager);

            var canAccess = await _salesReportService.CanAccessReportAsync(id, userId!, isManager, cancellationToken);
            if (!canAccess)
            {
                return ErrorResponse("Report not found or you don't have permission to view it.", 404);
            }

            var result = await _salesReportService.GetReportByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Report not found.", 404);
            }

            return SuccessResponse(result, "Report retrieved successfully");
        }

        /// <summary>
        /// Get sales reports with optional filtering (SalesManager/SuperAdmin: all reports, SalesEmployee: own reports)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReports([FromQuery] FilterSalesReportsDto filterDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(filterDto, _filterValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var userId = GetCurrentUserId();
            var (user, authError) = await ControllerAuthorizationHelper.GetCurrentUserAsync(userId, UserManager);
            if (authError != null)
                return authError;

            var isManager = await ControllerAuthorizationHelper.IsManagerAsync(user!, UserManager);

            PaginatedSalesReportsResponseDto result = isManager
                ? await _salesReportService.GetReportsAsync(filterDto, cancellationToken)
                : await _salesReportService.GetReportsForEmployeeAsync(userId!, filterDto, cancellationToken);

            return SuccessResponse(result, $"Found {result.TotalCount} report(s)");
        }

        /// <summary>
        /// Rate a sales report (SalesManager and SuperAdmin only)
        /// </summary>
        [HttpPost("{id}/rate")]
        [Authorize(Roles = "SalesManager,SuperAdmin")]
        public async Task<IActionResult> RateReport(int id, [FromBody] RateSalesReportDto rateDto, CancellationToken cancellationToken = default)
        {
            var validationError = await ValidateDtoAsync(rateDto, _rateValidator, cancellationToken);
            if (validationError != null)
                return validationError;

            var result = await _salesReportService.RateReportAsync(id, rateDto, cancellationToken);
            if (result == null)
            {
                return ErrorResponse("Report not found.", 404);
            }

            return SuccessResponse(result, "Report rated successfully");
        }
    }
}

