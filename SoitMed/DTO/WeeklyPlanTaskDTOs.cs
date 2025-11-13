using System.ComponentModel.DataAnnotations;
using SoitMed.Models;

namespace SoitMed.DTO
{
    /// <summary>
    /// DTO for creating a new weekly plan task
    /// Simplified: Only Client info, Title, Date, Description
    /// </summary>
    public class CreateWeeklyPlanTaskDTO
    {
        [Required(ErrorMessage = "Weekly plan ID is required")]
        public long WeeklyPlanId { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [MaxLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
        public string Title { get; set; } = string.Empty;

        // Client Information - For existing clients
        public long? ClientId { get; set; } // Will be resolved from ClientName if provided

        [MaxLength(20, ErrorMessage = "Client status cannot exceed 20 characters")]
        public string? ClientStatus { get; set; } // "Old", "New"

        // For NEW clients - basic info
        [MaxLength(200, ErrorMessage = "Client name cannot exceed 200 characters")]
        public string? ClientName { get; set; }

        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? ClientPhone { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? ClientAddress { get; set; }

        [MaxLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        public string? ClientLocation { get; set; }

        // Client Classification
        [MaxLength(1, ErrorMessage = "Client classification must be a single character")]
        public string? ClientClassification { get; set; } // A, B, C, D

        // Task Planning
        public DateTime? PlannedDate { get; set; }

        // Task Description
        [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing weekly plan task
    /// Simplified: Only Client info, Title, Date, Description
    /// </summary>
    public class UpdateWeeklyPlanTaskDTO
    {
        [MaxLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
        public string? Title { get; set; }

        // Client Information updates
        public long? ClientId { get; set; }

        [MaxLength(20, ErrorMessage = "Client status cannot exceed 20 characters")]
        public string? ClientStatus { get; set; }

        [MaxLength(200, ErrorMessage = "Client name cannot exceed 200 characters")]
        public string? ClientName { get; set; }

        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? ClientPhone { get; set; }

        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? ClientAddress { get; set; }

        [MaxLength(100, ErrorMessage = "Location cannot exceed 100 characters")]
        public string? ClientLocation { get; set; }

        [MaxLength(1, ErrorMessage = "Client classification must be a single character")]
        public string? ClientClassification { get; set; }

        // Task Planning updates
        public DateTime? PlannedDate { get; set; }

        // Task Description
        [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for weekly plan task response (detailed)
    /// Simplified: Only Client info, Title, Date, Description
    /// </summary>
    public class WeeklyPlanTaskDetailResponseDTO
    {
        public long Id { get; set; }
        public long WeeklyPlanId { get; set; }
        public string Title { get; set; } = string.Empty;
        public long? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? ClientStatus { get; set; }
        public string? ClientPhone { get; set; }
        public string? ClientAddress { get; set; }
        public string? ClientLocation { get; set; }
        public string? ClientClassification { get; set; }
        public DateTime? PlannedDate { get; set; }
        public string? Notes { get; set; }
        public int ProgressCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<TaskProgressSimpleDTO> Progresses { get; set; } = new();
        public List<OfferRequestSimpleDTO> OfferRequests { get; set; } = new();
        public List<SalesOfferSimpleDTO> Offers { get; set; } = new();
        public List<SalesDealSimpleDTO> Deals { get; set; } = new();
    }
}



