using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Stores push notification tokens for mobile devices
    /// </summary>
    public class DeviceToken : BaseEntity
    {
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty; // Expo Push Token

        [MaxLength(50)]
        public string Platform { get; set; } = "android"; // android, ios

        [MaxLength(200)]
        public string? DeviceId { get; set; } // Optional device identifier

        public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}


