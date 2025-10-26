using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    public class WeeklyPlan : BaseEntity
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ApplicationUser? Employee { get; set; }

        [Required]
        public DateTime WeekStartDate { get; set; }

        [Required]
        public DateTime WeekEndDate { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Manager Review Fields
        public int? Rating { get; set; } // 1-5

        [MaxLength(1000)]
        public string? ManagerComment { get; set; }

        public DateTime? ManagerReviewedAt { get; set; }

        public string? ReviewedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<WeeklyPlanTask> Tasks { get; set; } = new List<WeeklyPlanTask>(); // NEW: Tasks instead of Items
    }
}