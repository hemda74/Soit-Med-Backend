<<<<<<< HEAD
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> dev
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
<<<<<<< HEAD
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
=======
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Deal? Deal { get; set; }
        public Offer? Offer { get; set; }
    }
}
>>>>>>> dev
