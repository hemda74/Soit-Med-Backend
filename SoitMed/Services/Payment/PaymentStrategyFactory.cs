using SoitMed.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace SoitMed.Services.Payment
{
    /// <summary>
    /// Factory for creating payment strategies based on payment method
    /// </summary>
    public class PaymentStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentStrategyFactory> _logger;

        public PaymentStrategyFactory(
            IServiceProvider serviceProvider,
            ILogger<PaymentStrategyFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Get the appropriate payment strategy for the given payment method
        /// </summary>
        public IPaymentStrategy GetStrategy(PaymentMethod method)
        {
            // Get all registered payment strategies
            var strategies = _serviceProvider.GetServices<IPaymentStrategy>();

            var strategy = strategies.FirstOrDefault(s => s.SupportsMethod(method));

            if (strategy == null)
            {
                _logger.LogError("No payment strategy found for method {Method}", method);
                throw new NotSupportedException($"Payment method {method} is not supported");
            }

            return strategy;
        }
    }
}


