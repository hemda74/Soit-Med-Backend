using System.Text.Json.Serialization;

namespace SoitMed.Integrations.Paymob.DTOs
{
    /// <summary>
    /// Response from Paymob authentication endpoint
    /// </summary>
    public class PaymobAuthResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}

