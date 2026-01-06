using SoitMed.Models.Payment;

namespace SoitMed.Services.Payment
{
    /// <summary>
    /// Strategy pattern interface for payment processing
    /// Allows easy extension for InstallmentPaymentStrategy in the future
    /// </summary>
    public interface IPaymentStrategy
    {
        /// <summary>
        /// Process a payment transaction
        /// </summary>
        Task<PaymentResult> ProcessPaymentAsync(PaymentTransaction transaction, decimal amount, Dictionary<string, object>? additionalData = null);

        /// <summary>
        /// Check if this strategy supports the given payment method
        /// </summary>
        bool SupportsMethod(Models.Enums.PaymentMethod method);
    }

    /// <summary>
    /// Result of a payment processing operation
    /// </summary>
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? TransactionId { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}


