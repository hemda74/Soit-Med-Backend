using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    // ==================== CREATE DTOs ====================
    
    /// <summary>
    /// DTO for creating a weekly plan with tasks
    /// </summary>
    public class CreateWeeklyPlanDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Week start date is required.")]
        public DateTime WeekStartDate { get; set; }

        [Required(ErrorMessage = "Week end date is required.")]
        public DateTime WeekEndDate { get; set; }

        public List<CreateWeeklyPlanTaskDto> Tasks { get; set; } = new();
    }

    /// <summary>
    /// DTO for creating a task within a weekly plan
    /// </summary>
    public class CreateWeeklyPlanTaskDto
    {
        [Required(ErrorMessage = "Task title is required.")]
        [MaxLength(300, ErrorMessage = "Task title cannot exceed 300 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Task description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// DTO for adding a task to an existing weekly plan
    /// </summary>
    public class AddTaskToWeeklyPlanDto
    {
        [Required(ErrorMessage = "Task title is required.")]
        [MaxLength(300, ErrorMessage = "Task title cannot exceed 300 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Task description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// DTO for creating a daily progress entry
    /// </summary>
    public class CreateDailyProgressDto
    {
        [Required(ErrorMessage = "Progress date is required.")]
        public DateOnly ProgressDate { get; set; }

        [Required(ErrorMessage = "Progress notes are required.")]
        [MaxLength(2000, ErrorMessage = "Progress notes cannot exceed 2000 characters.")]
        public string Notes { get; set; } = string.Empty;

        public List<int> TasksWorkedOn { get; set; } = new(); // Task IDs
    }

    // ==================== UPDATE DTOs ====================

    /// <summary>
    /// DTO for updating a weekly plan
    /// </summary>
    public class UpdateWeeklyPlanDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for updating a task
    /// </summary>
    public class UpdateWeeklyPlanTaskDto
    {
        [Required(ErrorMessage = "Task title is required.")]
        [MaxLength(300, ErrorMessage = "Task title cannot exceed 300 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Task description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public bool IsCompleted { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    /// <summary>
    /// DTO for updating daily progress
    /// </summary>
    public class UpdateDailyProgressDto
    {
        [Required(ErrorMessage = "Progress notes are required.")]
        [MaxLength(2000, ErrorMessage = "Progress notes cannot exceed 2000 characters.")]
        public string Notes { get; set; } = string.Empty;

        public List<int> TasksWorkedOn { get; set; } = new(); // Task IDs
    }

    /// <summary>
    /// DTO for manager to review/rate a weekly plan
    /// </summary>
    public class ReviewWeeklyPlanDto
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int? Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? ManagerComment { get; set; }
    }

    // ==================== RESPONSE DTOs ====================

    /// <summary>
    /// Response DTO for weekly plan
    /// </summary>
    public class WeeklyPlanResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public string? ManagerComment { get; set; }
        public DateTime? ManagerReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        
        public List<WeeklyPlanTaskResponseDto> Tasks { get; set; } = new();
        public List<DailyProgressResponseDto> DailyProgresses { get; set; } = new();

        // Computed fields
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
    }

    /// <summary>
    /// Response DTO for weekly plan task
    /// </summary>
    public class WeeklyPlanTaskResponseDto
    {
        public int Id { get; set; }
        public long WeeklyPlanId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response DTO for daily progress
    /// </summary>
    public class DailyProgressResponseDto
    {
        public int Id { get; set; }
        public long WeeklyPlanId { get; set; }
        public DateOnly ProgressDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<int> TasksWorkedOn { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // ==================== FILTER DTOs ====================

    /// <summary>
    /// DTO for filtering weekly plans
    /// </summary>
    public class FilterWeeklyPlansDto
    {
        public string? EmployeeId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool? HasManagerReview { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Paginated response for weekly plans
    /// </summary>
    public class PaginatedWeeklyPlansResponseDto
    {
        public List<WeeklyPlanResponseDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}




