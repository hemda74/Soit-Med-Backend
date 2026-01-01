using SoitMed.Models.Enums;
using SoitMed.Models.Payment;

namespace SoitMed.Services.Payment
{
    /// <summary>
    /// Payment strategy for installment payments
    /// RESERVED FOR FUTURE IMPLEMENTATION - Not implemented yet
    /// </summary>
    public class InstallmentPaymentStrategy : IPaymentStrategy
    {
        private readonly ILogger<InstallmentPaymentStrategy> _logger;

        public InstallmentPaymentStrategy(ILogger<InstallmentPaymentStrategy> logger)
        {
            _logger = logger;
        }

        public bool SupportsMethod(PaymentMethod method)
        {
            return method == PaymentMethod.Installment;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(
            PaymentTransaction transaction, 
            decimal amount, 
            Dictionary<string, object>? additionalData = null)
        {
            // RESERVED FOR FUTURE IMPLEMENTATION
            // This will handle:
            // - Creating installment schedule
            // - Assigning collection delegate
            // - Tracking monthly payments
            // - Sending reminders

            _logger.LogWarning("InstallmentPaymentStrategy is not yet implemented. Transaction {TransactionId} cannot be processed.", 
                transaction.Id);

            return new PaymentResult
            {
                Success = false,
                ErrorMessage = "Installment payments are not yet implemented. This feature is reserved for future development."
            };
        }
    }
}

