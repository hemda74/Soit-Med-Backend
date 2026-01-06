using System.Text.Json.Serialization;

namespace SoitMed.Integrations.Paymob.DTOs
{
    /// <summary>
    /// Response from Paymob order creation endpoint
    /// </summary>
    public class PaymobOrderResponse
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }
    }
}


