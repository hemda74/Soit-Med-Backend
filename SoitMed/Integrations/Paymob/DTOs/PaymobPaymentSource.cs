using System.Text.Json.Serialization;

namespace SoitMed.Integrations.Paymob.DTOs
{
    /// <summary>
    /// Payment source for Wallet/Fawry payments
    /// </summary>
    public class PaymobPaymentSource
    {
        [JsonPropertyName("identifier")]
        public string? Identifier { get; set; }

        [JsonPropertyName("subtype")]
        public string? Subtype { get; set; }
    }
}

