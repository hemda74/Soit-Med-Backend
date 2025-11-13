using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "FinanceManager,FinanceEmployee,SuperAdmin")]
    public class AccountingController : BaseController
    {
        private readonly IAccountingService _accountingService;
        private readonly ILogger<AccountingController> _logger;

        public AccountingController(
            IAccountingService accountingService,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountingController> logger)
            : base(userManager)
        {
            _accountingService = accountingService;
            _logger = logger;
        }

        [HttpGet("payments/pending")]
        public async Task<IActionResult> GetPendingPayments()
        {
            try
            {
                var result = await _accountingService.GetPendingPaymentsAsync();
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending payments");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments([FromQuery] PaymentFilters filters)
        {
            try
            {
                var result = await _accountingService.GetPaymentsByDateRangeAsync(
                    filters.FromDate ?? DateTime.UtcNow.AddDays(-30),
                    filters.ToDate ?? DateTime.UtcNow);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("payments/{id}")]
        public async Task<IActionResult> GetPaymentDetails(int id)
        {
            try
            {
                var result = await _accountingService.GetPaymentDetailsAsync(id);
                if (result == null)
                    return NotFound();

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment details {PaymentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("payments/{id}/confirm")]
        public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _accountingService.ConfirmPaymentAsync(id, dto, userId);
                return SuccessResponse(result, "Payment confirmed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("payments/{id}/reject")]
        public async Task<IActionResult> RejectPayment(int id, [FromBody] RejectPaymentDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _accountingService.RejectPaymentAsync(id, dto, userId);
                return SuccessResponse(result, "Payment rejected");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting payment {PaymentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("reports/daily")]
        public async Task<IActionResult> GetDailyReport([FromQuery] DateTime? date)
        {
            try
            {
                var reportDate = date ?? DateTime.UtcNow.Date;
                var result = await _accountingService.GetDailyReportAsync(reportDate);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily report");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("reports/monthly")]
        public async Task<IActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var result = await _accountingService.GetMonthlyReportAsync(year, month);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly report");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("reports/yearly")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
        {
            try
            {
                var result = await _accountingService.GetYearlyReportAsync(year);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting yearly report");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("statistics/payment-methods")]
        public async Task<IActionResult> GetPaymentMethodStatistics([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                var result = await _accountingService.GetPaymentMethodStatisticsAsync(from, to);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment method statistics");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("statistics/outstanding")]
        public async Task<IActionResult> GetOutstandingPayments()
        {
            try
            {
                var result = await _accountingService.GetOutstandingPaymentsAsync();
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting outstanding payments");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("maintenance/payments")]
        public async Task<IActionResult> GetMaintenancePayments([FromQuery] PaymentFilters filters)
        {
            try
            {
                var result = await _accountingService.GetMaintenancePaymentsAsync(filters);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance payments");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("spare-parts/payments")]
        public async Task<IActionResult> GetSparePartsPayments([FromQuery] PaymentFilters filters)
        {
            try
            {
                var result = await _accountingService.GetSparePartsPaymentsAsync(filters);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting spare parts payments");
                return ErrorResponse(ex.Message);
            }
        }
    }
}

