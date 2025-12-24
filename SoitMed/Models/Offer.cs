using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a sales offer
    /// </summary>
    public class Offer
    {
        public long Id { get; set; }
        public long ActivityLogId { get; set; } // Foreign Key (One-to-One)
        public string UserId { get; set; } = string.Empty;
        public string OfferDetails { get; set; } = string.Empty;
        public Enums.OfferStatus Status { get; set; } = Enums.OfferStatus.Pending;
        public string? DocumentUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ActivityLog ActivityLog { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
    }
}
