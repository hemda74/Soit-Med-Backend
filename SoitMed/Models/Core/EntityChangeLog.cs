using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Core
{
    /// <summary>
    /// Audit log table for tracking all critical entity changes
    /// Stores old and new values as JSON for complete audit trail
    /// </summary>
    public class EntityChangeLog
    {
        [Key]
        public int Id { get; set; }

        // Entity information
        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty; // e.g., "MaintenanceVisit"

        [Required]
        public int EntityId { get; set; }

        // User who made the change
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        // Change details
        [Column(TypeName = "nvarchar(max)")]
        public string? OldValue { get; set; } // JSON serialized old entity state

        [Column(TypeName = "nvarchar(max)")]
        public string? NewValue { get; set; } // JSON serialized new entity state

        [Required]
        [MaxLength(50)]
        public string ChangeType { get; set; } = string.Empty; // "Created", "Updated", "Deleted"

        [Required]
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

        // Optional: Additional context about the change
        [MaxLength(1000)]
        public string? ChangeDescription { get; set; }
    }
}

