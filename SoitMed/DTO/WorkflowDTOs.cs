using System.ComponentModel.DataAnnotations;
using SoitMed.Models;
using SoitMed.Models.Enums;

namespace SoitMed.DTO
{
    // ==================== REQUEST DTOs ====================

    /// <summary>
    /// DTO for creating workflow requests
    /// </summary>
    public class CreateWorkflowRequestDto
    {
        [Required]
        public long ActivityLogId { get; set; }
        public long? OfferId { get; set; }
        public long? DealId { get; set; }
        [Required]
        public string ToUserId { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public long? DeliveryTermsId { get; set; }
        public long? PaymentTermsId { get; set; }
    }

    /// <summary>
    /// DTO for updating workflow request status
    /// </summary>
    public class UpdateWorkflowRequestStatusDto
    {
        [Required]
        public RequestStatus Status { get; set; }
        public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for creating delivery terms
    /// </summary>
    public class CreateDeliveryTermsDto
    {
        [Required(ErrorMessage = "Delivery method is required.")]
        [MaxLength(200, ErrorMessage = "Delivery method cannot exceed 200 characters.")]
        public string DeliveryMethod { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery address is required.")]
        [MaxLength(500, ErrorMessage = "Delivery address cannot exceed 500 characters.")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }

        [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string? State { get; set; }

        [MaxLength(20, ErrorMessage = "Postal code cannot exceed 20 characters.")]
        public string? PostalCode { get; set; }

        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
        public string? Country { get; set; }

        public int? EstimatedDeliveryDays { get; set; }

        [MaxLength(1000, ErrorMessage = "Special instructions cannot exceed 1000 characters.")]
        public string? SpecialInstructions { get; set; }

        public bool IsUrgent { get; set; } = false;

        public DateTime? PreferredDeliveryDate { get; set; }

        [MaxLength(200, ErrorMessage = "Contact person cannot exceed 200 characters.")]
        public string? ContactPerson { get; set; }

        [MaxLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters.")]
        public string? ContactPhone { get; set; }

        [MaxLength(100, ErrorMessage = "Contact email cannot exceed 100 characters.")]
        public string? ContactEmail { get; set; }
    }

    /// <summary>
    /// DTO for creating payment terms
    /// </summary>
    public class CreatePaymentTermsDto
    {
        [Required(ErrorMessage = "Payment method is required.")]
        [MaxLength(100, ErrorMessage = "Payment method cannot exceed 100 characters.")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0.")]
        public decimal TotalAmount { get; set; }

        public decimal? DownPayment { get; set; }

        public int? InstallmentCount { get; set; }

        public decimal? InstallmentAmount { get; set; }

        public int? PaymentDueDays { get; set; }

        [MaxLength(200, ErrorMessage = "Bank name cannot exceed 200 characters.")]
        public string? BankName { get; set; }

        [MaxLength(50, ErrorMessage = "Account number cannot exceed 50 characters.")]
        public string? AccountNumber { get; set; }

        [MaxLength(50, ErrorMessage = "IBAN cannot exceed 50 characters.")]
        public string? IBAN { get; set; }

        [MaxLength(100, ErrorMessage = "Swift code cannot exceed 100 characters.")]
        public string? SwiftCode { get; set; }

        [MaxLength(1000, ErrorMessage = "Payment instructions cannot exceed 1000 characters.")]
        public string? PaymentInstructions { get; set; }

        public bool RequiresAdvancePayment { get; set; } = false;

        public decimal? AdvancePaymentPercentage { get; set; }

        [MaxLength(200, ErrorMessage = "Currency cannot exceed 200 characters.")]
        public string Currency { get; set; } = "EGP";
    }

    /// <summary>
    /// DTO for sending offer request to sales support
    /// </summary>
    public class SendOfferRequestDto
    {
        [Required(ErrorMessage = "Activity log ID is required.")]
        public long ActivityLogId { get; set; }

        [Required(ErrorMessage = "Offer ID is required.")]
        public long OfferId { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        [MaxLength(200, ErrorMessage = "Client name cannot exceed 200 characters.")]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Client address is required.")]
        [MaxLength(500, ErrorMessage = "Client address cannot exceed 500 characters.")]
        public string ClientAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Equipment details are required.")]
        [MaxLength(1000, ErrorMessage = "Equipment details cannot exceed 1000 characters.")]
        public string EquipmentDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery terms are required.")]
        public CreateDeliveryTermsDto DeliveryTerms { get; set; } = new();

        [Required(ErrorMessage = "Payment terms are required.")]
        public CreatePaymentTermsDto PaymentTerms { get; set; } = new();

        [MaxLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for sending deal request to legal manager
    /// </summary>
    public class SendDealRequestDto
    {
        [Required(ErrorMessage = "Activity log ID is required.")]
        public long ActivityLogId { get; set; }

        [Required(ErrorMessage = "Deal ID is required.")]
        public long DealId { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        [MaxLength(200, ErrorMessage = "Client name cannot exceed 200 characters.")]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Client address is required.")]
        [MaxLength(500, ErrorMessage = "Client address cannot exceed 500 characters.")]
        public string ClientAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Equipment details are required.")]
        [MaxLength(1000, ErrorMessage = "Equipment details cannot exceed 1000 characters.")]
        public string EquipmentDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery terms are required.")]
        public CreateDeliveryTermsDto DeliveryTerms { get; set; } = new();

        [Required(ErrorMessage = "Payment terms are required.")]
        public CreatePaymentTermsDto PaymentTerms { get; set; } = new();

        [MaxLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for assigning request to user
    /// </summary>
    public class AssignRequestDto
    {
        [Required(ErrorMessage = "User ID is required.")]
        [MaxLength(450, ErrorMessage = "User ID cannot exceed 450 characters.")]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for updating request status
    /// </summary>
    public class UpdateRequestStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        public RequestStatus Status { get; set; }

        [MaxLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }

    // ==================== RESPONSE DTOs ====================

    /// <summary>
    /// DTO for delivery terms response
    /// </summary>
    public class DeliveryTermsResponseDto
    {
        public int Id { get; set; }
        public string DeliveryMethod { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public int? EstimatedDeliveryDays { get; set; }
        public string? SpecialInstructions { get; set; }
        public bool IsUrgent { get; set; }
        public DateTime? PreferredDeliveryDate { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for payment terms response
    /// </summary>
    public class PaymentTermsResponseDto
    {
        public int Id { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal? DownPayment { get; set; }
        public int? InstallmentCount { get; set; }
        public decimal? InstallmentAmount { get; set; }
        public int? PaymentDueDays { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? IBAN { get; set; }
        public string? SwiftCode { get; set; }
        public string? PaymentInstructions { get; set; }
        public bool RequiresAdvancePayment { get; set; }
        public decimal? AdvancePaymentPercentage { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for request workflow response
    /// </summary>
    public class RequestWorkflowResponseDto
    {
        public long Id { get; set; }
        public long ActivityLogId { get; set; }
        public long? OfferId { get; set; }
        public long? DealId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string FromRole { get; set; } = string.Empty;
        public string ToRole { get; set; } = string.Empty;
        public string FromUserId { get; set; } = string.Empty;
        public string FromUserName { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
        public string ToUserName { get; set; } = string.Empty;
        public RequestStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public string? ClientName { get; set; }
        public string? ClientAddress { get; set; }
        public string? EquipmentDetails { get; set; }
        public DeliveryTermsResponseDto? DeliveryTerms { get; set; }
        public PaymentTermsResponseDto? PaymentTerms { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// DTO for notification response
    /// </summary>
    public class NotificationResponseDto
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Priority { get; set; }
        public long? RequestWorkflowId { get; set; }
        public long? ActivityLogId { get; set; }
        public bool IsRead { get; set; }
        public bool IsMobilePush { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
