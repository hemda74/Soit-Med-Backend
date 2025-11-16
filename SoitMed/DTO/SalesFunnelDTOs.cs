using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Enums;

namespace SoitMed.DTO
{
    // ==================== REQUEST DTOs ====================

    /// <summary>
    /// DTO for creating a new activity log entry
    /// </summary>
    public class CreateActivityRequestDto
    {
        [Required(ErrorMessage = "Interaction type is required.")]
        public InteractionType InteractionType { get; set; }

        [Required(ErrorMessage = "Client type is required.")]
        public ClientType ClientType { get; set; }

        [Required(ErrorMessage = "Activity result is required.")]
        public ActivityResult Result { get; set; }

        public RejectionReason? Reason { get; set; }
        public string? Comment { get; set; }
        public CreateDealDto? DealInfo { get; set; }
        public CreateOfferDto? OfferInfo { get; set; }
    }

    /// <summary>
    /// DTO for creating a new deal
    /// </summary>
    public class CreateDealDto
    {
        [Required(ErrorMessage = "Client ID is required.")]
        public long ClientId { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Deal value is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deal value must be greater than 0.")]
        public decimal DealValue { get; set; }

        public DateTime? ExpectedCloseDate { get; set; }
    }

    /// <summary>
    /// DTO for creating a new offer
    /// </summary>
    public class CreateOfferDto
    {
        [Required(ErrorMessage = "Client ID is required.")]
        public long ClientId { get; set; }

        [MaxLength(200)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Offer details are required.")]
        [MaxLength(2000, ErrorMessage = "Offer details cannot exceed 2000 characters.")]
        public string OfferDetails { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Offer value must be greater than 0.")]
        public decimal? Value { get; set; }

        [MaxLength(500, ErrorMessage = "Document URL cannot exceed 500 characters.")]
        public string? DocumentUrl { get; set; }
    }

    /// <summary>
    /// DTO for updating a deal
    /// </summary>
    public class UpdateDealDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Deal value must be greater than 0.")]
        public decimal? DealValue { get; set; }

        public string? Status { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
    }

    /// <summary>
    /// DTO for updating an offer
    /// </summary>
    public class UpdateOfferDto
    {
        [MaxLength(2000, ErrorMessage = "Offer details cannot exceed 2000 characters.")]
        public string? OfferDetails { get; set; }

        public string? Status { get; set; }

        [MaxLength(500, ErrorMessage = "Document URL cannot exceed 500 characters.")]
        public string? DocumentUrl { get; set; }
    }

    // ==================== RESPONSE DTOs ====================

    /// <summary>
    /// DTO for activity log response
    /// </summary>
    public class ActivityResponseDto
    {
        public long Id { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public InteractionType InteractionType { get; set; }
        public string InteractionTypeName { get; set; } = string.Empty;
        public ClientType ClientType { get; set; }
        public string ClientTypeName { get; set; } = string.Empty;
        public ActivityResult Result { get; set; }
        public string ResultName { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? ReasonName { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DealResponseDto? Deal { get; set; }
        public OfferResponseDto? Offer { get; set; }
    }

    /// <summary>
    /// DTO for deal response
    /// </summary>
    public class DealResponseDto
    {
        public long Id { get; set; }
        public long ActivityLogId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public decimal DealValue { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for offer response
    /// </summary>
    public class OfferResponseDto
    {
        public long Id { get; set; }
        public long ActivityLogId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string OfferDetails { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string? DocumentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for manager dashboard statistics
    /// </summary>
    public class DashboardStatsResponseDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalActivities { get; set; }
        public int TotalVisits { get; set; }
        public int TotalFollowUps { get; set; }
        public int TotalDeals { get; set; }
        public int TotalOffers { get; set; }
        public int WonDeals { get; set; }
        public int LostDeals { get; set; }
        public int PendingDeals { get; set; }
        public int AcceptedOffers { get; set; }
        public int RejectedOffers { get; set; }
        public int DraftOffers { get; set; }
        public int SentOffers { get; set; }
        public decimal TotalDealValue { get; set; }
        public decimal WonDealValue { get; set; }
        public decimal AverageDealValue { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal OfferAcceptanceRate { get; set; }
        public List<ClientTypeStatsDto> ClientTypeStats { get; set; } = new();
        public List<SalespersonStatsDto> SalespersonStats { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }

    /// <summary>
    /// DTO for client type statistics
    /// </summary>
    public class ClientTypeStatsDto
    {
        public ClientType ClientType { get; set; }
        public string ClientTypeName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal TotalValue { get; set; }
    }

    /// <summary>
    /// DTO for salesperson statistics
    /// </summary>
    public class SalespersonStatsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int TotalActivities { get; set; }
        public int TotalDeals { get; set; }
        public int WonDeals { get; set; }
        public decimal TotalValue { get; set; }
        public decimal WonValue { get; set; }
        public decimal ConversionRate { get; set; }
    }

    /// <summary>
    /// DTO for recent activities
    /// </summary>
    public class RecentActivityDto
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public InteractionType InteractionType { get; set; }
        public string InteractionTypeName { get; set; } = string.Empty;
        public ClientType ClientType { get; set; }
        public string ClientTypeName { get; set; } = string.Empty;
        public ActivityResult Result { get; set; }
        public string ResultName { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal? DealValue { get; set; }
        public string? OfferDetails { get; set; }
    }

    /// <summary>
    /// DTO for individual salesman statistics
    /// </summary>
    public class SalesmanStatsResponseDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        
        // Activity Statistics
        public int TotalActivities { get; set; }
        public int TotalVisits { get; set; }
        public int TotalFollowUps { get; set; }
        public int InterestedActivities { get; set; }
        public int NotInterestedActivities { get; set; }
        
        // Deal Statistics
        public int TotalDeals { get; set; }
        public int WonDeals { get; set; }
        public int LostDeals { get; set; }
        public int PendingDeals { get; set; }
        public decimal TotalDealValue { get; set; }
        public decimal WonDealValue { get; set; }
        public decimal AverageDealValue { get; set; }
        
        // Offer Statistics
        public int TotalOffers { get; set; }
        public int AcceptedOffers { get; set; }
        public int RejectedOffers { get; set; }
        public int DraftOffers { get; set; }
        public int SentOffers { get; set; }
        
        // Performance Metrics
        public decimal ConversionRate { get; set; }
        public decimal OfferAcceptanceRate { get; set; }
        public decimal InterestRate { get; set; }
        
        // Client Type Breakdown
        public List<ClientTypeStatsDto> ClientTypeStats { get; set; } = new();
        
        // Recent Activities
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        
        // Weekly/Monthly Trends
        public List<WeeklyTrendDto> WeeklyTrends { get; set; } = new();
    }

    /// <summary>
    /// DTO for weekly trend data
    /// </summary>
    public class WeeklyTrendDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public int Activities { get; set; }
        public int Visits { get; set; }
        public int FollowUps { get; set; }
        public int Deals { get; set; }
        public int WonDeals { get; set; }
        public decimal DealValue { get; set; }
        public int Offers { get; set; }
        public int AcceptedOffers { get; set; }
    }
}
