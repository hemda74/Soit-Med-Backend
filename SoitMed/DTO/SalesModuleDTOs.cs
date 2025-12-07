using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [MaxLength(2000)]
        public string? VoiceDescriptionUrl { get; set; }

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
        public string? VoiceDescriptionUrl { get; set; }
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
        public long? ClientId { get; set; } // Optional - will be auto-resolved for customer roles

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

        // Payment Terms, Delivery Terms, Warranty Terms as arrays (lists)
        public List<string>? PaymentTerms { get; set; }

        public List<string>? DeliveryTerms { get; set; }

        public List<string>? WarrantyTerms { get; set; }

        // ValidUntil as array of date strings (ISO format: "YYYY-MM-DD")
        public List<string>? ValidUntil { get; set; }

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

    public class OfferItemInputDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(100)]
        public string? Factory { get; set; } // maps to Provider

        [MaxLength(100)]
        public string? Country { get; set; }

        public int? Year { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? ProviderImagePath { get; set; } // Provider logo/image path

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool InStock { get; set; } = true;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }

    public class ApproveOfferDTO
    {
        public bool Approved { get; set; }

        [MaxLength(2000)]
        public string? RejectionReason { get; set; }

        [MaxLength(2000)]
        public string? Comments { get; set; }
    }

    /// <summary>
    /// DTO for updating offers (PATCH) - all fields are optional
    /// </summary>
    public class UpdateOfferDTO
    {
        public long? ClientId { get; set; }

        public string? AssignedTo { get; set; }

        [MaxLength(2000)]
        public string? Products { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? TotalAmount { get; set; }

        // Payment Terms, Delivery Terms, Warranty Terms as arrays (lists)
        public List<string>? PaymentTerms { get; set; }

        public List<string>? DeliveryTerms { get; set; }

        public List<string>? WarrantyTerms { get; set; }

        // ValidUntil as array of date strings (ISO format: "YYYY-MM-DD")
        public List<string>? ValidUntil { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
        
        // Optional fields for enhanced offer features
        [MaxLength(50)]
        public string? PaymentType { get; set; } // Cash, Installments, Other
        
        [Range(0.01, double.MaxValue)]
        public decimal? FinalPrice { get; set; }
        
        [MaxLength(200)]
        public string? OfferDuration { get; set; }
    }

    public class CreateOfferWithItemsDTO
    {
        public long? OfferRequestId { get; set; }

        [Required]
        public long ClientId { get; set; }

        [Required]
        public string AssignedTo { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<OfferItemInputDTO> Products { get; set; } = new();

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        // Payment Terms, Delivery Terms, Warranty Terms as arrays (lists)
        public List<string>? PaymentTerms { get; set; }

        public List<string>? DeliveryTerms { get; set; }

        public List<string>? WarrantyTerms { get; set; }

        // ValidUntil as array of date strings (ISO format: "YYYY-MM-DD")
        public List<string>? ValidUntil { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? PaymentType { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? FinalPrice { get; set; }

        [MaxLength(200)]
        public string? OfferDuration { get; set; }
    }

    public class OfferResponseDTO
    {
        public List<OfferEquipmentDTO> Equipment { get; set; } = new();
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
        public List<string>? PaymentTerms { get; set; }
        public List<string>? DeliveryTerms { get; set; }
        public List<string>? WarrantyTerms { get; set; }
        public List<string>? ValidUntil { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SentToClientAt { get; set; }
        public string? ClientResponse { get; set; }
        public string? OfferRequestRequesterId { get; set; }
        public string? OfferRequestRequesterName { get; set; }
        // SalesManager Approval/Rejection fields
        public string? SalesManagerApprovedBy { get; set; }
        public DateTime? SalesManagerApprovedAt { get; set; }
        public string? SalesManagerComments { get; set; }
        public string? SalesManagerRejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsSalesManagerApproved { get; set; }
        public bool CanSendToSalesman { get; set; }
    }

    public class OfferSummaryDTO
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string>? ValidUntil { get; set; }
    }

    public class PaginatedOffersResponseDTO
    {
        public List<OfferResponseDTO> Offers { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    // ==================== Enhanced Offer DTOs ====================
    
    // Equipment DTOs
    public class OfferEquipmentDTO
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("offerId")]
        public long OfferId { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("model")]
        public string? Model { get; set; }
        [JsonPropertyName("provider")]
        public string? Provider { get; set; }
        [JsonPropertyName("country")]
        public string? Country { get; set; }
        [JsonPropertyName("year")]
        public int? Year { get; set; }
        [JsonPropertyName("imagePath")]
        public string? ImagePath { get; set; }
        [JsonPropertyName("providerImagePath")]
        public string? ProviderImagePath { get; set; }
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("inStock")]
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
        
        public int? Year { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool InStock { get; set; } = true;

        [MaxLength(500)]
        public string? ImagePath { get; set; }

        [MaxLength(500)]
        public string? ProviderImagePath { get; set; }
    }

    public class UpdateOfferEquipmentDTO : CreateOfferEquipmentDTO
    {
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
        public decimal TotalValue { get; set; } // Alias for DealValue for mobile compatibility
        public DateTime ClosedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ManagerRejectionReason { get; set; }
        public string? ManagerComments { get; set; }
        public bool? ManagerApproved { get; set; } // true if approved, false if rejected, null if pending
        public DateTime? ManagerApprovedAt { get; set; }
        public string? ManagerApprovedByName { get; set; }
        public string? SuperAdminRejectionReason { get; set; }
        public string? SuperAdminComments { get; set; }
        public bool? SuperAdminApproved { get; set; } // true if approved, false if rejected, null if pending
        public DateTime? SuperAdminApprovedAt { get; set; }
        public string? SuperAdminApprovedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? CompletionNotes { get; set; }
        public string? FailureNotes { get; set; }
        public string? ReportText { get; set; }
        public string? ReportAttachments { get; set; }
        public DateTime? ReportSubmittedAt { get; set; }
        public DateTime? SentToLegalAt { get; set; }
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

    public class AssignOfferToSalesmanDTO
    {
        [Required]
        public string SalesmanId { get; set; } = string.Empty;
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

    public class SubmitReportDTO
    {
        [Required(ErrorMessage = "Report text is required")]
        [MaxLength(5000, ErrorMessage = "Report text cannot exceed 5000 characters")]
        public string ReportText { get; set; } = string.Empty;

        [MaxLength(2000, ErrorMessage = "Attachments JSON cannot exceed 2000 characters")]
        public string? ReportAttachments { get; set; } // JSON array of file paths/URLs
    }

    public class RecordClientResponseDTO
    {
        [Required(ErrorMessage = "Response is required")]
        [MaxLength(2000, ErrorMessage = "Response cannot exceed 2000 characters")]
        public string Response { get; set; } = string.Empty;

        [Required(ErrorMessage = "Accepted status is required")]
        public bool Accepted { get; set; }
    }

    public class CompleteOfferDTO
    {
        [MaxLength(2000)]
        public string? CompletionNotes { get; set; }
    }

    public class NeedsModificationDTO
    {
        [MaxLength(1000)]
        public string? Reason { get; set; }
    }

    public class OfferActivityDTO
    {
        public long OfferId { get; set; }
        public string Type { get; set; } = string.Empty; // Accepted, Completed, Sent, Rejected
        public string Description { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public string? SalesmanName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime Timestamp { get; set; }
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


