using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class CreateWeeklyPlanDTO
    {
        [Required]
        public DateTime WeekStartDate { get; set; }

        /// <summary>
        /// End date is auto-calculated as 7 days from start date.
        /// This field is optional and will be ignored if provided.
        /// </summary>
        public DateTime? WeekEndDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Optional: Tasks to create along with the weekly plan
        /// </summary>
        public List<CreateWeeklyPlanTaskDTO>? Tasks { get; set; }
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

    public class WeeklyPlanFiltersDTO
    {
        public string? EmployeeId { get; set; }
        public DateTime? WeekStartDate { get; set; }
        public DateTime? WeekEndDate { get; set; }
        public bool? IsViewed { get; set; }
    }


    public class WeeklyPlanResponseDTO
    {
        public long Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public EmployeeInfoDTO? Employee { get; set; } // Employee details for managers
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? Rating { get; set; }
        public string? ManagerComment { get; set; }
        public DateTime? ManagerReviewedAt { get; set; }
        public DateTime? ManagerViewedAt { get; set; }
        public string? ViewedBy { get; set; }
        public bool IsViewed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<WeeklyPlanTaskResponseDTO> Tasks { get; set; } = new();
    }

    public class EmployeeInfoDTO
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class WeeklyPlanTaskResponseDTO
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
        public List<TaskProgressSimpleDTO> Progresses { get; set; } = new();
        public List<OfferRequestSimpleDTO> OfferRequests { get; set; } = new();
        public List<SalesOfferSimpleDTO> Offers { get; set; } = new();
        public List<SalesDealSimpleDTO> Deals { get; set; } = new();
    }

    public class TaskProgressSimpleDTO
    {
        public long Id { get; set; }
        public DateTime ProgressDate { get; set; }
        public string ProgressType { get; set; } = string.Empty;
        public string? Description { get; set; }
		public string? VoiceDescriptionUrl { get; set; }
        public string? VisitResult { get; set; }
        public string? NextStep { get; set; }
        public long? OfferRequestId { get; set; }
    }

    public class OfferRequestSimpleDTO
    {
        public long Id { get; set; }
        public string RequestedProducts { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public long? CreatedOfferId { get; set; }
    }

    public class SalesOfferSimpleDTO
    {
        public long Id { get; set; }
        public string Products { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<string>? ValidUntil { get; set; } // Changed to List<string> to match SalesOffer model
        public string Status { get; set; } = string.Empty;
        public DateTime? SentToClientAt { get; set; }
    }

    public class SalesDealSimpleDTO
    {
        public long Id { get; set; }
        public decimal DealValue { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ManagerApprovedAt { get; set; }
        public DateTime? SuperAdminApprovedAt { get; set; }
    }

}
