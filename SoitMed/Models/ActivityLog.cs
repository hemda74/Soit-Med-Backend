using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a sales activity log entry
    /// </summary>
    public class ActivityLog : BaseEntity
    {
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public int TaskId { get; set; }

        [Required]
        public InteractionType InteractionType { get; set; }

        [Required]
        public ClientType ClientType { get; set; }

        [Required]
        public ActivityResult Result { get; set; }

        [MaxLength(50)]
        public string? Reason { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        // Navigation properties
        public Deal? Deal { get; set; }
        public Offer? Offer { get; set; }
    }
}
