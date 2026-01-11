using System.Text.Json.Serialization;

namespace SoitMed.DTO
{
    /// <summary>
    /// DTO for customer machines response matching Media API format
    /// </summary>
    public class CustomerMachinesDto
    {
        [JsonPropertyName("customerId")]
        public int CustomerId { get; set; }

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("customerAddress")]
        public string? CustomerAddress { get; set; }

        [JsonPropertyName("customerPhone")]
        public string? CustomerPhone { get; set; }

        [JsonPropertyName("machineCount")]
        public int MachineCount { get; set; }

        [JsonPropertyName("machines")]
        public List<MachineDto> Machines { get; set; } = new();
    }

    /// <summary>
    /// DTO for machine details with media files and visit count
    /// </summary>
    public class MachineDto
    {
        [JsonPropertyName("machineId")]
        public int MachineId { get; set; }

        [JsonPropertyName("serialNumber")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("modelName")]
        public string? ModelName { get; set; }

        [JsonPropertyName("modelNameEn")]
        public string? ModelNameEn { get; set; }

        [JsonPropertyName("itemCode")]
        public string? ItemCode { get; set; }

        [JsonPropertyName("visitCount")]
        public int VisitCount { get; set; }

        [JsonPropertyName("mediaFiles")]
        public List<MediaFileDto> MediaFiles { get; set; } = new();
    }

    /// <summary>
    /// DTO for media file metadata matching Media API format
    /// </summary>
    public class MediaFileDto
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("fileUrl")]
        public string FileUrl { get; set; } = string.Empty;

        [JsonPropertyName("fileType")]
        public string? FileType { get; set; }

        [JsonPropertyName("isImage")]
        public bool IsImage { get; set; }

        [JsonPropertyName("isPdf")]
        public bool IsPdf { get; set; }
    }
}
