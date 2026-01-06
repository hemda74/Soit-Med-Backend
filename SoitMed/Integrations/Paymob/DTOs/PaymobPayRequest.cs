using System.Text.Json.Serialization;

namespace SoitMed.Integrations.Paymob.DTOs
{
    /// <summary>
    /// Request to process a payment in Paymob
    /// </summary>
    public class PaymobPayRequest
    {
        [JsonPropertyName("source")]
        public PaymobPaymentSource? Source { get; set; }

        [JsonPropertyName("payment_token")]
        public string? PaymentToken { get; set; }
    }
}


