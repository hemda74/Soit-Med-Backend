using System.Text.Json.Serialization;

namespace SoitMed.Integrations.Paymob.DTOs
{
    /// <summary>
    /// Request to generate a payment key in Paymob
    /// </summary>
    public class PaymobKeyRequest
    {
        [JsonPropertyName("auth_token")]
        public string? AuthToken { get; set; }

        [JsonPropertyName("amount_cents")]
        public long? AmountCents { get; set; }

        [JsonPropertyName("expiration")]
        public int? Expiration { get; set; }

        [JsonPropertyName("order_id")]
        public int? OrderId { get; set; }

        [JsonPropertyName("billing_data")]
        public PaymobBillingData? BillingData { get; set; }

        [JsonPropertyName("integration_id")]
        public int? IntegrationId { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("lock_order_when_paid")]
        public bool? LockOrderWhenPaid { get; set; }
    }
}

