using SoitMed.Models.Enums;
using SoitMed.Models.Payment;

namespace SoitMed.Services.Payment
{
    /// <summary>
    /// Payment strategy for Visa/Credit Card payments via Payment Gateway
    /// </summary>
    public class VisaPaymentStrategy : IPaymentStrategy
    {
        private readonly ILogger<VisaPaymentStrategy> _logger;
        // TODO: Inject payment gateway service (Paymob, Stripe, etc.) when ready

        public VisaPaymentStrategy(ILogger<VisaPaymentStrategy> logger)
        {
            _logger = logger;
        }

        public bool SupportsMethod(PaymentMethod method)
        {
            return method == PaymentMethod.CreditCard || 
                   method == PaymentMethod.DebitCard || 
                   method == PaymentMethod.Paymob ||
                   method == PaymentMethod.Gateway;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(
            PaymentTransaction transaction, 
            decimal amount, 
            Dictionary<string, object>? additionalData = null)
        {
            try
            {
                _logger.LogInformation("Processing Visa/Gateway payment for transaction {TransactionId}, amount: {Amount}", 
                    transaction.Id, amount);

                // Extract payment token from additional data
                var paymentToken = additionalData?.ContainsKey("paymentToken") == true 
                    ? additionalData["paymentToken"]?.ToString() 
                    : null;

                if (string.IsNullOrEmpty(paymentToken))
                {
                    return new PaymentResult
                    {
                        Success = false,
                        ErrorMessage = "Payment token is required for gateway payments"
                    };
                }

                // TODO: Integrate with actual payment gateway (Paymob, Stripe, etc.)
                // For now, simulate payment processing
                transaction.Status = PaymentStatus.Processing;
                transaction.Amount = amount;
                transaction.GatewayTransactionId = $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{transaction.Id}";

                // Simulate async payment processing
                await Task.Delay(100);

                // In real implementation, this would call the payment gateway API
                // For now, mark as completed (in production, this would be set by webhook)
                transaction.Status = PaymentStatus.Completed;

                return new PaymentResult
                {
                    Success = true,
                    TransactionId = transaction.GatewayTransactionId,
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "gateway", "Paymob" }, // or Stripe, etc.
                        { "redirectUrl", additionalData?.ContainsKey("redirectUrl") == true ? additionalData["redirectUrl"] : null }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Visa/Gateway payment for transaction {TransactionId}", transaction.Id);
                transaction.Status = PaymentStatus.Failed;
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}


