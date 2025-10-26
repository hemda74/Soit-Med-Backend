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
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateWeeklyPlanDTO
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class ReviewWeeklyPlanDTO
    {
        [Range(1, 5)]
        public int? Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }


    public class WeeklyPlanResponseDTO
    {
        public long Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? Rating { get; set; }
        public string? ManagerComment { get; set; }
        public DateTime? ManagerReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<WeeklyPlanTaskResponseDTO> Tasks { get; set; } = new();
    }

    public class WeeklyPlanTaskResponseDTO
    {
        public long Id { get; set; }
        public string TaskType { get; set; } = string.Empty;
        public long? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? ClientStatus { get; set; }
        public string? ClientClassification { get; set; }
        public DateTime? PlannedDate { get; set; }
        public string? PlannedTime { get; set; }
        public string? Purpose { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ProgressCount { get; set; }
    }

}
