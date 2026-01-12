using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    /// <summary>
    /// DTO for linking a Client to a User account
    /// </summary>
    public class LinkClientAccountDTO
    {
        [Required(ErrorMessage = "ClientId is required")]
        public string ClientId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for marking QR code as printed
    /// </summary>
    public class MarkQrPrintedDTO
    {
        [Required(ErrorMessage = "EquipmentId is required")]
        public int EquipmentId { get; set; }
    }
}


