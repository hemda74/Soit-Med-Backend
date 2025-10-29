using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    // ==================== Client DTOs ====================
    // Note: CreateClientDTO and ClientResponseDTO are already defined in ClientDTOs.cs

    // NEW - Client Profile with Complete History
    public class ClientProfileDTO
    {
        public ClientResponseDTO ClientInfo { get; set; } = new();
        public List<TaskProgressSummaryDTO> AllVisits { get; set; } = new(); // All visits/interactions
        public List<OfferSummaryDTO> AllOffers { get; set; } = new(); // All offers
        public List<DealSummaryDTO> AllDeals { get; set; } = new(); // All deals (success/failed)
        public ClientStatisticsDTO Statistics { get; set; } = new();
    }


    // ==================== WeeklyPlan DTOs ====================
    // Note: CreateWeeklyPlanDTO, WeeklyPlanResponseDTO, WeeklyPlanTaskResponseDTO, and ReviewWeeklyPlanDTO are already defined in WeeklyPlanDTOs.cs

    // ==================== TaskProgress DTOs ====================
    public class CreateTaskProgressDTO
    {
        [Required]
        public long TaskId { get; set; }

        [Required]
        public DateTime ProgressDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProgressType { get; set; } = "Visit"; // Visit, Call, Meeting, Email

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // Visit Result
        [MaxLength(20)]
        public string? VisitResult { get; set; } // Interested, NotInterested

        [MaxLength(2000)]
        public string? NotInterestedComment { get; set; }

        // If Interested
        [MaxLength(20)]
        public string? NextStep { get; set; } // NeedsDeal, NeedsOffer

        // Follow-up
        public DateTime? NextFollowUpDate { get; set; }

        [MaxLength(1000)]
        public string? FollowUpNotes { get; set; }

        // Satisfaction
        [Range(1, 5)]
        public int? SatisfactionRating { get; set; }

        [MaxLength(2000)]
        public string? Feedback { get; set; }
    }

    public class TaskProgressResponseDTO
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public long? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime ProgressDate { get; set; }
        public string ProgressType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? VisitResult { get; set; }
        public string? NotInterestedComment { get; set; }
        public string? NextStep { get; set; }
        public long? OfferRequestId { get; set; }
        public long? OfferId { get; set; }
        public long? DealId { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public int? SatisfactionRating { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TaskProgressSummaryDTO
    {
        public long Id { get; set; }
        public DateTime ProgressDate { get; set; }
        public string ProgressType { get; set; } = string.Empty;
        public string? VisitResult { get; set; }
        public string? NextStep { get; set; }
        public int? SatisfactionRating { get; set; }
    }

    // ==================== OfferRequest DTOs ====================
    public class CreateOfferRequestDTO
    {
        [Required]
        public long ClientId { get; set; }

        public long? TaskProgressId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string RequestedProducts { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? SpecialNotes { get; set; }
    }

    public class OfferRequestResponseDTO
    {
        public long Id { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string RequestedByName { get; set; } = string.Empty;
        public long ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string RequestedProducts { get; set; } = string.Empty;
        public string? SpecialNotes { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
        public long? CreatedOfferId { get; set; }
    }

    // ==================== Offer DTOs ====================
    public class CreateOfferDTO
    {
        public long? OfferRequestId { get; set; } // Optional - can create offer without request

        [Required]
        public long ClientId { get; set; }

        [Required]
        public string AssignedTo { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Products { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [MaxLength(2000)]
        public string? PaymentTerms { get; set; }

        [MaxLength(2000)]
        public string? DeliveryTerms { get; set; }

        [Required]
        public DateTime ValidUntil { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
        
        // Optional fields for enhanced offer features
        [MaxLength(50)]
        public string? PaymentType { get; set; } // Cash, Installments, Other
        
        [Range(0.01, double.MaxValue)]
        public decimal? FinalPrice { get; set; } // Required if creating installments
        
        [MaxLength(200)]
        public string? OfferDuration { get; set; }
    }

    public class OfferResponseDTO
    {
        public long Id { get; set; }
        public long? OfferRequestId { get; set; }
        public long ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public string Products { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? PaymentTerms { get; set; }
        public string? DeliveryTerms { get; set; }
        public DateTime ValidUntil { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SentToClientAt { get; set; }
        public string? ClientResponse { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OfferSummaryDTO
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ValidUntil { get; set; }
    }

    // ==================== Enhanced Offer DTOs ====================
    
    // Equipment DTOs
    public class OfferEquipmentDTO
    {
        public long Id { get; set; }
        public long OfferId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Provider { get; set; }
        public string? Country { get; set; }
        public string? ImagePath { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public bool InStock { get; set; } = true;
    }

    public class CreateOfferEquipmentDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Model { get; set; }
        
        [MaxLength(100)]
        public string? Provider { get; set; }
        
        [MaxLength(100)]
        public string? Country { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool InStock { get; set; } = true;
    }

    // Terms DTOs
    public class OfferTermsDTO
    {
        public long Id { get; set; }
        public long OfferId { get; set; }
        public string? WarrantyPeriod { get; set; }
        public string? DeliveryTime { get; set; }
        public string? MaintenanceTerms { get; set; }
        public string? OtherTerms { get; set; }
    }

    public class CreateOfferTermsDTO
    {
        [MaxLength(500)]
        public string? WarrantyPeriod { get; set; }
        
        [MaxLength(500)]
        public string? DeliveryTime { get; set; }
        
        [MaxLength(2000)]
        public string? MaintenanceTerms { get; set; }
        
        [MaxLength(2000)]
        public string? OtherTerms { get; set; }
    }

    // Installment Plan DTOs
    public class InstallmentPlanDTO
    {
        public long Id { get; set; }
        public long OfferId { get; set; }
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CreateInstallmentPlanDTO
    {
        [Required]
        public int NumberOfInstallments { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [MaxLength(50)]
        public string PaymentFrequency { get; set; } = "Monthly"; // Monthly, Weekly, Quarterly
    }

    // Enhanced Offer DTOs
    public class EnhancedOfferResponseDTO : OfferResponseDTO
    {
        public string? PaymentType { get; set; }
        public decimal? FinalPrice { get; set; }
        public string? OfferDuration { get; set; }
        public List<OfferEquipmentDTO> Equipment { get; set; } = new();
        public OfferTermsDTO? Terms { get; set; }
        public List<InstallmentPlanDTO> Installments { get; set; } = new();
    }

    // ==================== Deal DTOs ====================
    public class CreateDealDTO
    {
        [Required]
        public long OfferId { get; set; }

        [Required]
        public long ClientId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal DealValue { get; set; }

        [MaxLength(2000)]
        public string? PaymentTerms { get; set; }

        [MaxLength(2000)]
        public string? DeliveryTerms { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }
    }

    public class DealResponseDTO
    {
        public long Id { get; set; }
        public long? OfferId { get; set; }
        public long ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string SalesmanId { get; set; } = string.Empty;
        public string SalesmanName { get; set; } = string.Empty;
        public decimal DealValue { get; set; }
        public DateTime ClosedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ManagerRejectionReason { get; set; }
        public string? ManagerComments { get; set; }
        public string? SuperAdminRejectionReason { get; set; }
        public string? SuperAdminComments { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DealSummaryDTO
    {
        public long Id { get; set; }
        public DateTime ClosedDate { get; set; }
        public decimal DealValue { get; set; }
        public string Status { get; set; } = string.Empty; // Success, Failed, Pending, etc.
    }

    public class ApproveDealDTO
    {
        public bool Approved { get; set; }

        [MaxLength(50)]
        public string? RejectionReason { get; set; } // Money, CashFlow, OtherNeeds

        [MaxLength(2000)]
        public string? Comments { get; set; }
    }

    // ==================== Constants ====================
    public static class TaskTypeConstants
    {
        public const string Visit = "Visit";
        public const string FollowUp = "FollowUp";

        public static readonly string[] AllTypes = { Visit, FollowUp };
    }

    public static class ProgressTypeConstants
    {
        public const string Visit = "Visit";
        public const string Call = "Call";
        public const string Meeting = "Meeting";
        public const string Email = "Email";

        public static readonly string[] AllTypes = { Visit, Call, Meeting, Email };
    }

    public static class VisitResultConstants
    {
        public const string Interested = "Interested";
        public const string NotInterested = "NotInterested";

        public static readonly string[] AllResults = { Interested, NotInterested };
    }

    // ==================== Additional DTOs for Controllers ====================
    
    public class CreateTaskProgressWithOfferRequestDTO : CreateTaskProgressDTO
    {
        [Required]
        public long ClientId { get; set; }
        
        [Required]
        [MaxLength(2000)]
        public string RequestedProducts { get; set; } = string.Empty;
        
        [MaxLength(2000)]
        public string? SpecialNotes { get; set; }
    }

    public class AssignOfferRequestDTO
    {
        [Required]
        public string AssignedTo { get; set; } = string.Empty;
    }

    public class UpdateOfferRequestStatusDTO
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class CompleteDealDTO
    {
        [MaxLength(2000)]
        public string? CompletionNotes { get; set; }
    }

    public class FailDealDTO
    {
        [Required]
        [MaxLength(2000)]
        public string FailureNotes { get; set; } = string.Empty;
    }

    public class ClientStatisticsDTO
    {
        public int TotalVisits { get; set; }
        public int TotalOffers { get; set; }
        public int SuccessfulDeals { get; set; }
        public int FailedDeals { get; set; }
        public decimal? TotalRevenue { get; set; }
        public double? AverageSatisfaction { get; set; }
    }

    public static class NextStepConstants
    {
        public const string NeedsDeal = "NeedsDeal";
        public const string NeedsOffer = "NeedsOffer";

        public static readonly string[] AllSteps = { NeedsDeal, NeedsOffer };
    }

}


