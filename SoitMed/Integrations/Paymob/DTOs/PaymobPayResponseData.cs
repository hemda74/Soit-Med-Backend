using System.Text.Json.Serialization;

namespace SoitMed.Integrations.Paymob.DTOs
{
    /// <summary>
    /// Response data object from Paymob payment endpoint (contains bill_reference for Fawry)
    /// </summary>
    public class PaymobPayResponseData
    {
        [JsonPropertyName("bill_reference")]
        public string? BillReference { get; set; }
    }
}


