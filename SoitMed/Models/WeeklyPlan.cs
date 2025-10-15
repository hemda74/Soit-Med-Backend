using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Weekly plan created by salesman at the beginning of each week
    /// </summary>
    public class WeeklyPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateOnly WeekStartDate { get; set; }

        [Required]
        public DateOnly WeekEndDate { get; set; }

        [Required]
        [MaxLength(450)]
        public string EmployeeId { get; set; } = string.Empty;

        // Manager review fields
        public int? Rating { get; set; } // 1-5 stars, nullable

        [MaxLength(1000)]
        public string? ManagerComment { get; set; }

        public DateTime? ManagerReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public virtual ApplicationUser Employee { get; set; } = null!;

        public virtual ICollection<WeeklyPlanTask> Tasks { get; set; } = new List<WeeklyPlanTask>();
        
        public virtual ICollection<DailyProgress> DailyProgresses { get; set; } = new List<DailyProgress>();
    }
}




