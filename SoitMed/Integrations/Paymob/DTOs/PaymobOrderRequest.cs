using System.Text.Json.Serialization;

namespace SoitMed.Integrations.Paymob.DTOs
{
    /// <summary>
    /// Request to create an order in Paymob
    /// </summary>
    public class PaymobOrderRequest
    {
        [JsonPropertyName("auth_token")]
        public string? AuthToken { get; set; }

        [JsonPropertyName("delivery_needed")]
        public bool? DeliveryNeeded { get; set; }

        [JsonPropertyName("amount_cents")]
        public long? AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("merchant_order_id")]
        public string? MerchantOrderId { get; set; }

        [JsonPropertyName("items")]
        public List<PaymobOrderItem>? Items { get; set; }
    }
}


