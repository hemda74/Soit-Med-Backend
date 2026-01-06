using System.Text.Json.Serialization;

namespace SoitMed.Integrations.Paymob.DTOs
{
    /// <summary>
    /// Response from Paymob payment key generation endpoint
    /// </summary>
    public class PaymobKeyResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}


