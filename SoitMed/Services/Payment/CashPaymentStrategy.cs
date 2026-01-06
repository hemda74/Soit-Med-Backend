using SoitMed.Models.Enums;
using SoitMed.Models.Payment;

namespace SoitMed.Services.Payment
{
    /// <summary>
    /// Payment strategy for cash payments
    /// </summary>
    public class CashPaymentStrategy : IPaymentStrategy
    {
        private readonly ILogger<CashPaymentStrategy> _logger;

        public CashPaymentStrategy(ILogger<CashPaymentStrategy> logger)
        {
            _logger = logger;
        }

        public bool SupportsMethod(PaymentMethod method)
        {
            return method == PaymentMethod.Cash;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(
            PaymentTransaction transaction, 
            decimal amount, 
            Dictionary<string, object>? additionalData = null)
        {
            try
            {
                _logger.LogInformation("Processing cash payment for transaction {TransactionId}, amount: {Amount}", 
                    transaction.Id, amount);

                // Cash payments are immediately marked as completed
                // They require manual confirmation from Accounts/Admin
                transaction.Status = PaymentStatus.Pending; // Will be confirmed by Accounts
                transaction.Amount = amount;

                return new PaymentResult
                {
                    Success = true,
                    TransactionId = transaction.Id.ToString(),
                    AdditionalData = new Dictionary<string, object>
                    {
                        { "requiresConfirmation", true },
                        { "confirmationMessage", "Cash payment requires manual confirmation from Accounts department" }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cash payment for transaction {TransactionId}", transaction.Id);
                return new PaymentResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}


