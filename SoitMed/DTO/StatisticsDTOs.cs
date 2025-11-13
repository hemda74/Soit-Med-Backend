using System.ComponentModel.DataAnnotations;
using SoitMed.Models;

namespace SoitMed.DTO
{
    /// <summary>
    /// Statistics for a salesman for a specific period
    /// </summary>
    public class SalesmanStatisticsDTO
    {
        public string SalesmanId { get; set; } = string.Empty;
        public string SalesmanName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int? Quarter { get; set; }

        // Visit Statistics
        public int TotalVisits { get; set; }
        public int SuccessfulVisits { get; set; } // VisitResult = "Interested"
        public int FailedVisits { get; set; } // VisitResult = "NotInterested"
        public decimal SuccessRate { get; set; } // percentage

        // Offer Statistics
        public int TotalOffers { get; set; }
        public int AcceptedOffers { get; set; }
        public int RejectedOffers { get; set; }
        public decimal OfferAcceptanceRate { get; set; }

        // Deal Statistics
        public int TotalDeals { get; set; }
        public decimal TotalDealValue { get; set; }
    }

    /// <summary>
    /// DTO for creating a new salesman target
    /// </summary>
    public class CreateSalesmanTargetDTO
    {
        public string? SalesmanId { get; set; } // null = team target
        public int Year { get; set; }
        
        [Range(1, 4)]
        public int? Quarter { get; set; } // null = yearly

        [Required]
        public TargetType TargetType { get; set; } // Money (manager) or Activity (salesman)

        // Activity targets (visits/offers/deals) - set by salesman
        // Nullable for PATCH support - null means field not provided
        [Range(0, int.MaxValue)]
        public int? TargetVisits { get; set; }
        
        [Range(0, int.MaxValue)]
        public int? TargetSuccessfulVisits { get; set; }
        
        [Range(0, int.MaxValue)]
        public int? TargetOffers { get; set; }
        
        [Range(0, int.MaxValue)]
        public int? TargetDeals { get; set; }
        
        [Range(0, 100)]
        public decimal? TargetOfferAcceptanceRate { get; set; }

        // Money target - set by manager
        [Range(0, double.MaxValue)]
        public decimal? TargetRevenue { get; set; }

        public bool IsTeamTarget { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for returning salesman target details
    /// </summary>
    public class SalesmanTargetDTO
    {
        public long Id { get; set; }
        public string? SalesmanId { get; set; }
        public string? SalesmanName { get; set; }
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public TargetType TargetType { get; set; }

        // Activity targets (visits/offers/deals)
        public int TargetVisits { get; set; }
        public int TargetSuccessfulVisits { get; set; }
        public int TargetOffers { get; set; }
        public int TargetDeals { get; set; }
        public decimal? TargetOfferAcceptanceRate { get; set; }

        // Money target
        public decimal? TargetRevenue { get; set; }

        public bool IsTeamTarget { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByManagerName { get; set; }
        public string? CreatedBySalesmanName { get; set; } // For self-set targets
    }

    /// <summary>
    /// DTO for salesman progress showing current statistics against targets
    /// </summary>
    public class SalesmanProgressDTO
    {
        public SalesmanStatisticsDTO CurrentStatistics { get; set; } = null!;
        public SalesmanTargetDTO? IndividualMoneyTarget { get; set; }
        public SalesmanTargetDTO? IndividualActivityTarget { get; set; }
        public SalesmanTargetDTO? TeamMoneyTarget { get; set; }
        public SalesmanTargetDTO? TeamActivityTarget { get; set; }

        // Progress Percentages (0-100)
        public decimal VisitsProgress { get; set; }
        public decimal SuccessfulVisitsProgress { get; set; }
        public decimal OffersProgress { get; set; }
        public decimal DealsProgress { get; set; }
        public decimal? OfferAcceptanceRateProgress { get; set; }
        public decimal? RevenueProgress { get; set; }
    }
}

