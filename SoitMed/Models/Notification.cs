using System.ComponentModel.DataAnnotations;

namespace SoitMed.Models
{
    /// <summary>
    /// Notification system for real-time updates
    /// </summary>
    public class Notification
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // "Request", "Assignment", "Update", "Reminder"

        [MaxLength(50)]
        public string? Priority { get; set; } // "Low", "Medium", "High", "Urgent"

        public long? RequestWorkflowId { get; set; }

        public long? ActivityLogId { get; set; }

        public bool IsRead { get; set; } = false;

        public bool IsMobilePush { get; set; } = false; // For mobile push notifications

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Navigation properties
        public virtual RequestWorkflow? RequestWorkflow { get; set; }
        public virtual ActivityLog? ActivityLog { get; set; }
    }
}

