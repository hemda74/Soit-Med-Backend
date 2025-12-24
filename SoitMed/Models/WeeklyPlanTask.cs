using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models
{
    /// <summary>
    /// Individual task within a weekly plan
    /// </summary>
    public class WeeklyPlanTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WeeklyPlanId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        // Client Information
        public long? ClientId { get; set; }

        [MaxLength(20)]
        public string? ClientStatus { get; set; } // "Old", "New"

        [MaxLength(200)]
        public string? ClientName { get; set; }

        [MaxLength(20)]
        public string? ClientPhone { get; set; }

        [MaxLength(500)]
        public string? ClientAddress { get; set; }

        [MaxLength(100)]
        public string? ClientLocation { get; set; }

        [MaxLength(1)]
        public string? ClientClassification { get; set; } // A, B, C, D

        public DateTime? PlannedDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("WeeklyPlanId")]
        public virtual WeeklyPlan WeeklyPlan { get; set; } = null!;

        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }

        public virtual ICollection<TaskProgress> Progresses { get; set; } = new List<TaskProgress>();
    }
}




