using Microsoft.AspNetCore.Identity;
using SoitMed.DTO;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;
using SoitMed.Models.Payment;
using SoitMed.Repositories;
using System.Collections.Generic;

namespace SoitMed.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(CreatePaymentDTO dto, string customerId)
        {
            try
            {
                var customer = await _userManager.FindByIdAsync(customerId);
                if (customer == null)
                    throw new ArgumentException("Customer not found", nameof(customerId));

                var payment = new Models.Payment.Payment
                {
                    MaintenanceRequestId = dto.MaintenanceRequestId,
                    SparePartRequestId = dto.SparePartRequestId,
                    CustomerId = customerId,
                    Amount = dto.Amount,
                    PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod),
                    Status = PaymentStatus.Pending
                };

                await _unitOfWork.Payments.CreateAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment created. PaymentId: {PaymentId}, CustomerId: {CustomerId}, Amount: {Amount}", 
                    payment.Id, customerId, dto.Amount);

                return await MapToResponseDTO(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment. CustomerId: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<PaymentResponseDTO?> GetPaymentAsync(int id)
        {
            var payment = await _unitOfWork.Payments.GetWithDetailsAsync(id);
            if (payment == null)
                return null;

            return await MapToResponseDTO(payment);
        }

        public async Task<IEnumerable<PaymentResponseDTO>> GetCustomerPaymentsAsync(string customerId)
        {
            var payments = await _unitOfWork.Payments.GetByCustomerIdAsync(customerId);
            var result = new List<PaymentResponseDTO>();

            foreach (var payment in payments)
            {
                result.Add(await MapToResponseDTO(payment));
            }

            return result;
        }

        public async Task<PaymentResponseDTO> ProcessStripePaymentAsync(int paymentId, StripePaymentDTO dto)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found", nameof(paymentId));

            // TODO: Integrate with Stripe API
            payment.Status = PaymentStatus.Processing;
            payment.TransactionId = dto.Token; // Placeholder - should be actual transaction ID from Stripe

            // Simulate payment processing
            await Task.Delay(1000); // Simulate API call

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.ConfirmedAt = DateTime.UtcNow;

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Create payment transaction record
            await CreatePaymentTransactionAsync(paymentId, "Payment", payment.Amount, dto.Token, PaymentStatus.Completed, "Stripe payment completed");

            _logger.LogInformation("Stripe payment processed. PaymentId: {PaymentId}", paymentId);

            return await MapToResponseDTO(payment);
        }

        public async Task<PaymentResponseDTO> ProcessPayPalPaymentAsync(int paymentId, PayPalPaymentDTO dto)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found", nameof(paymentId));

            // TODO: Integrate with PayPal API
            payment.Status = PaymentStatus.Processing;
            payment.TransactionId = dto.PaymentId;

            await Task.Delay(1000); // Simulate API call

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.ConfirmedAt = DateTime.UtcNow;

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Create payment transaction record
            await CreatePaymentTransactionAsync(paymentId, "Payment", payment.Amount, dto.PaymentId, PaymentStatus.Completed, "PayPal payment completed");

            return await MapToResponseDTO(payment);
        }

        public async Task<PaymentResponseDTO> ProcessLocalGatewayPaymentAsync(int paymentId, LocalGatewayPaymentDTO dto)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found", nameof(paymentId));

            // TODO: Integrate with local gateway API
            payment.Status = PaymentStatus.Processing;
            payment.TransactionId = dto.PaymentToken;

            await Task.Delay(1000); // Simulate API call

            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.ConfirmedAt = DateTime.UtcNow;

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Create payment transaction record
            await CreatePaymentTransactionAsync(paymentId, "Payment", payment.Amount, dto.PaymentToken, PaymentStatus.Completed, "Local gateway payment completed");

            return await MapToResponseDTO(payment);
        }

        public async Task<PaymentResponseDTO> RecordCashPaymentAsync(int paymentId, CashPaymentDTO dto)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found", nameof(paymentId));

            payment.PaymentReference = dto.ReceiptNumber ?? dto.PaymentReference;
            payment.Status = PaymentStatus.Pending; // Needs accounting confirmation

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Notify accounting
            await _notificationService.SendNotificationToRoleGroupAsync(
                "FinanceManager",
                "New cash payment requires confirmation",
                $"Payment #{paymentId}: {payment.Amount} EGP",
                new Dictionary<string, object> { { "PaymentId", paymentId } }
            );
            await _notificationService.SendNotificationToRoleGroupAsync(
                "FinanceEmployee",
                "New cash payment requires confirmation",
                $"Payment #{paymentId}: {payment.Amount} EGP",
                new Dictionary<string, object> { { "PaymentId", paymentId } }
            );

            return await MapToResponseDTO(payment);
        }

        public async Task<PaymentResponseDTO> RecordBankTransferAsync(int paymentId, BankTransferDTO dto)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found", nameof(paymentId));

            payment.PaymentReference = dto.BankReference ?? dto.PaymentReference;
            payment.Status = PaymentStatus.Pending; // Needs accounting confirmation

            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            // Notify accounting
            await _notificationService.SendNotificationToRoleGroupAsync(
                "FinanceManager",
                "New bank transfer requires confirmation",
                $"Payment #{paymentId}: {payment.Amount} EGP - Reference: {dto.BankReference}",
                new Dictionary<string, object> { { "PaymentId", paymentId } }
            );
            await _notificationService.SendNotificationToRoleGroupAsync(
                "FinanceEmployee",
                "New bank transfer requires confirmation",
                $"Payment #{paymentId}: {payment.Amount} EGP - Reference: {dto.BankReference}",
                new Dictionary<string, object> { { "PaymentId", paymentId } }
            );

            return await MapToResponseDTO(payment);
        }

        public async Task<PaymentResponseDTO> ProcessRefundAsync(int paymentId, RefundDTO dto, string userId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found", nameof(paymentId));

            if (payment.Status != PaymentStatus.Completed)
                throw new InvalidOperationException("Only completed payments can be refunded");

            var refundAmount = dto.Amount > 0 ? dto.Amount : payment.Amount;

            // Create refund transaction
            var transaction = new PaymentTransaction
            {
                PaymentId = paymentId,
                TransactionType = "Refund",
                Amount = refundAmount,
                Status = PaymentStatus.Completed,
                Notes = dto.Reason
            };

            // TODO: Process refund through payment gateway if needed

            payment.Status = PaymentStatus.Refunded;
            await _unitOfWork.Payments.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment refunded. PaymentId: {PaymentId}, Amount: {Amount}", paymentId, refundAmount);

            return await MapToResponseDTO(payment);
        }

        private async Task CreatePaymentTransactionAsync(
            int paymentId,
            string transactionType,
            decimal amount,
            string? gatewayTransactionId,
            PaymentStatus status,
            string? notes = null)
        {
            try
            {
                var transaction = new Models.Payment.PaymentTransaction
                {
                    PaymentId = paymentId,
                    TransactionType = transactionType,
                    Amount = amount,
                    GatewayTransactionId = gatewayTransactionId,
                    Status = status,
                    Notes = notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.GetContext().PaymentTransactions.AddAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment transaction created. TransactionId: {TransactionId}, PaymentId: {PaymentId}", 
                    transaction.Id, paymentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating payment transaction for payment {PaymentId}", paymentId);
                // Don't throw - transaction creation is optional
            }
        }

        private async Task<PaymentResponseDTO> MapToResponseDTO(Models.Payment.Payment payment)
        {
            var customer = await _userManager.FindByIdAsync(payment.CustomerId);
            var accountant = payment.ProcessedByAccountantId != null
                ? await _userManager.FindByIdAsync(payment.ProcessedByAccountantId)
                : null;

            return new PaymentResponseDTO
            {
                Id = payment.Id.ToString(),
                MaintenanceRequestId = payment.MaintenanceRequestId?.ToString(),
                SparePartRequestId = payment.SparePartRequestId?.ToString(),
                CustomerId = payment.CustomerId,
                CustomerName = customer?.UserName ?? "",
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod.ToString(),
                PaymentMethodName = payment.PaymentMethod.ToString(),
                Status = payment.Status.ToString(),
                StatusName = payment.Status.ToString(),
                TransactionId = payment.TransactionId,
                PaymentReference = payment.PaymentReference,
                ProcessedByAccountantId = payment.ProcessedByAccountantId,
                ProcessedByAccountantName = accountant?.UserName ?? "",
                ProcessedAt = payment.ProcessedAt ?? DateTime.UtcNow,
                AccountingNotes = payment.AccountingNotes,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt,
                ConfirmedAt = payment.ConfirmedAt
            };
        }
    }
}

