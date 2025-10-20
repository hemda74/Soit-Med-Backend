using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;

namespace SoitMed.Models
{
    public class WeeklyPlan : BaseEntity
    {
        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        public DateTime WeekStartDate { get; set; }

        [Required]
        public DateTime WeekEndDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string PlanTitle { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? PlanDescription { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? ApprovedBy { get; set; }

        public string? RejectedBy { get; set; }

        // Navigation properties
        public virtual ICollection<WeeklyPlanItem> PlanItems { get; set; } = new List<WeeklyPlanItem>();
    }
}