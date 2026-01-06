using System.Text.Json.Serialization;

namespace SoitMed.Integrations.Paymob.DTOs
{
    /// <summary>
    /// Response from Paymob payment processing endpoint
    /// </summary>
    public class PaymobPayResponse
    {
        [JsonPropertyName("redirect_url")]
        public string? RedirectUrl { get; set; }

        [JsonPropertyName("pending")]
        public bool? Pending { get; set; }

        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("data")]
        public PaymobPayResponseData? Data { get; set; }
    }
}


