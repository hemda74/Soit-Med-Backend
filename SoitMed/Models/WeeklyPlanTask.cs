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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("WeeklyPlanId")]
        public virtual WeeklyPlan WeeklyPlan { get; set; } = null!;
    }
}




