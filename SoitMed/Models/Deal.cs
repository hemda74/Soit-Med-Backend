using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a sales deal
    /// </summary>
    public class Deal
    {
        public long Id { get; set; }
        public long ActivityLogId { get; set; } // Foreign Key (One-to-One)
        public string UserId { get; set; } = string.Empty;
        public decimal DealValue { get; set; }
        public Enums.DealStatus Status { get; set; } = Enums.DealStatus.Pending;
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ActivityLog ActivityLog { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
    }
}
