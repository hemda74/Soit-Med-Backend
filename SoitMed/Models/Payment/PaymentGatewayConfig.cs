using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Payment
{
    public class PaymentGatewayConfig
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string GatewayName { get; set; } = string.Empty; // "Stripe", "PayPal", "Fawry", etc.

        [Required]
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? ApiKey { get; set; } // Encrypted

        [MaxLength(500)]
        public string? SecretKey { get; set; } // Encrypted

        [MaxLength(500)]
        public string? MerchantId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? AdditionalConfig { get; set; } // JSON for gateway-specific config

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

