using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class CreateWeeklyPlanDTO
    {
        [Required]
        public DateTime WeekStartDate { get; set; }

        [Required]
        public DateTime WeekEndDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string PlanTitle { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? PlanDescription { get; set; }
    }

    public class UpdateWeeklyPlanDTO
    {
        [MaxLength(200)]
        public string? PlanTitle { get; set; }

        [MaxLength(1000)]
        public string? PlanDescription { get; set; }
    }

    public class CreateWeeklyPlanItemDTO
    {
        [Required]
        public long WeeklyPlanId { get; set; }

        public long? ClientId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ClientType { get; set; }

        [MaxLength(100)]
        public string? ClientSpecialization { get; set; }

        [MaxLength(100)]
        public string? ClientLocation { get; set; }

        [MaxLength(20)]
        public string? ClientPhone { get; set; }

        [MaxLength(100)]
        public string? ClientEmail { get; set; }

        [Required]
        public DateTime PlannedVisitDate { get; set; }

        [MaxLength(20)]
        public string? PlannedVisitTime { get; set; }

        [MaxLength(500)]
        public string? VisitPurpose { get; set; }

        [MaxLength(1000)]
        public string? VisitNotes { get; set; }

        [MaxLength(50)]
        public string Priority { get; set; } = "Medium";

        public bool IsNewClient { get; set; }
    }

    public class UpdateWeeklyPlanItemDTO
    {
        [MaxLength(200)]
        public string? ClientName { get; set; }

        [MaxLength(50)]
        public string? ClientType { get; set; }

        [MaxLength(100)]
        public string? ClientSpecialization { get; set; }

        [MaxLength(100)]
        public string? ClientLocation { get; set; }

        [MaxLength(20)]
        public string? ClientPhone { get; set; }

        [MaxLength(100)]
        public string? ClientEmail { get; set; }

        public DateTime? PlannedVisitDate { get; set; }

        [MaxLength(20)]
        public string? PlannedVisitTime { get; set; }

        [MaxLength(500)]
        public string? VisitPurpose { get; set; }

        [MaxLength(1000)]
        public string? VisitNotes { get; set; }

        [MaxLength(50)]
        public string? Priority { get; set; }
    }

    public class CompletePlanItemDTO
    {
        [MaxLength(2000)]
        public string? Results { get; set; }

        [MaxLength(2000)]
        public string? Feedback { get; set; }

        [Range(1, 5)]
        public int? SatisfactionRating { get; set; }

        public DateTime? NextVisitDate { get; set; }

        [MaxLength(1000)]
        public string? FollowUpNotes { get; set; }
    }

    public class CancelPlanItemDTO
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class PostponePlanItemDTO
    {
        [Required]
        public DateTime NewDate { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class ApprovePlanDTO
    {
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class RejectPlanDTO
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }

    public class WeeklyPlanResponseDTO
    {
        public long Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public string PlanTitle { get; set; } = string.Empty;
        public string? PlanDescription { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ApprovalNotes { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<WeeklyPlanItemResponseDTO> PlanItems { get; set; } = new();
    }

    public class WeeklyPlanItemResponseDTO
    {
        public long Id { get; set; }
        public long WeeklyPlanId { get; set; }
        public long? ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientType { get; set; }
        public string? ClientSpecialization { get; set; }
        public string? ClientLocation { get; set; }
        public string? ClientPhone { get; set; }
        public string? ClientEmail { get; set; }
        public DateTime PlannedVisitDate { get; set; }
        public string? PlannedVisitTime { get; set; }
        public string? VisitPurpose { get; set; }
        public string? VisitNotes { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsNewClient { get; set; }
        public DateTime? ActualVisitDate { get; set; }
        public string? Results { get; set; }
        public string? Feedback { get; set; }
        public int? SatisfactionRating { get; set; }
        public DateTime? NextVisitDate { get; set; }
        public string? FollowUpNotes { get; set; }
        public string? CancellationReason { get; set; }
        public string? PostponementReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
