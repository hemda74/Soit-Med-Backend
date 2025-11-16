using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models
{
    /// <summary>
    /// Daily progress notes added by salesman
    /// </summary>
    public class DailyProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long WeeklyPlanId { get; set; }

        [Required]
        public DateOnly ProgressDate { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Notes { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? TasksWorkedOn { get; set; } // Comma-separated task IDs

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("WeeklyPlanId")]
        public virtual WeeklyPlan WeeklyPlan { get; set; } = null!;
    }
}




