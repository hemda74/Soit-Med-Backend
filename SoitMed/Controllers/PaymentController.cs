using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SoitMed.Common;
using SoitMed.DTO;
using SoitMed.Models.Identity;
using SoitMed.Services;

namespace SoitMed.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            UserManager<ApplicationUser> userManager,
            ILogger<PaymentController> logger)
            : base(userManager)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _paymentService.CreatePaymentAsync(dto, userId);
                return SuccessResponse(result, "Payment created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            try
            {
                var result = await _paymentService.GetPaymentAsync(id);
                if (result == null)
                    return NotFound();

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpGet("customer/my-payments")]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> GetMyPayments()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _paymentService.GetCustomerPaymentsAsync(userId);
                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer payments");
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/stripe")]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> ProcessStripePayment(int id, [FromBody] StripePaymentDTO dto)
        {
            try
            {
                var result = await _paymentService.ProcessStripePaymentAsync(id, dto);
                return SuccessResponse(result, "Payment processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe payment {PaymentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/paypal")]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> ProcessPayPalPayment(int id, [FromBody] PayPalPaymentDTO dto)
        {
            try
            {
                var result = await _paymentService.ProcessPayPalPaymentAsync(id, dto);
                return SuccessResponse(result, "Payment processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal payment {PaymentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/cash")]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> RecordCashPayment(int id, [FromBody] CashPaymentDTO dto)
        {
            try
            {
                var result = await _paymentService.RecordCashPaymentAsync(id, dto);
                return SuccessResponse(result, "Cash payment recorded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cash payment {PaymentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/bank-transfer")]
        [Authorize(Roles = "Doctor,Technician,Manager")]
        public async Task<IActionResult> RecordBankTransfer(int id, [FromBody] BankTransferDTO dto)
        {
            try
            {
                var result = await _paymentService.RecordBankTransferAsync(id, dto);
                return SuccessResponse(result, "Bank transfer recorded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording bank transfer {PaymentId}", id);
                return ErrorResponse(ex.Message);
            }
        }

        [HttpPost("{id}/refund")]
        [Authorize(Roles = "FinanceManager,SuperAdmin")]
        public async Task<IActionResult> ProcessRefund(int id, [FromBody] RefundDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _paymentService.ProcessRefundAsync(id, dto, userId);
                return SuccessResponse(result, "Refund processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund {PaymentId}", id);
                return ErrorResponse(ex.Message);
            }
        }
    }
}

