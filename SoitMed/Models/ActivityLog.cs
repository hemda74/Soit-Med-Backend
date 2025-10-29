using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a sales activity log entry
    /// </summary>
    public class ActivityLog
    {
        public long Id { get; set; }
        public int PlanTaskId { get; set; }
        public string UserId { get; set; } = string.Empty; // Foreign Key to Salesperson
        public InteractionType InteractionType { get; set; }
        public ClientType ClientType { get; set; }
        public ActivityResult Result { get; set; }
        public RejectionReason? Reason { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual WeeklyPlanTask? PlanTask { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Deal? Deal { get; set; }
        public virtual Offer? Offer { get; set; }
    }
}
