using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    /// <summary>
    /// DTO for QR code verification when starting a visit
    /// </summary>
    public class VerifyMachineDTO
    {
        [Required]
        [MaxLength(200)]
        public string ScannedQrCode { get; set; } = string.Empty;
    }
}

