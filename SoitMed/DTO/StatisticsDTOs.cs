using System.ComponentModel.DataAnnotations;

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

        [Range(0, int.MaxValue)]
        public int TargetVisits { get; set; }
        
        [Range(0, int.MaxValue)]
        public int TargetSuccessfulVisits { get; set; }
        
        [Range(0, int.MaxValue)]
        public int TargetOffers { get; set; }
        
        [Range(0, int.MaxValue)]
        public int TargetDeals { get; set; }
        
        [Range(0, 100)]
        public decimal? TargetOfferAcceptanceRate { get; set; }

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

        public int TargetVisits { get; set; }
        public int TargetSuccessfulVisits { get; set; }
        public int TargetOffers { get; set; }
        public int TargetDeals { get; set; }
        public decimal? TargetOfferAcceptanceRate { get; set; }

        public bool IsTeamTarget { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByManagerName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for salesman progress showing current statistics against targets
    /// </summary>
    public class SalesmanProgressDTO
    {
        public SalesmanStatisticsDTO CurrentStatistics { get; set; } = null!;
        public SalesmanTargetDTO? IndividualTarget { get; set; }
        public SalesmanTargetDTO? TeamTarget { get; set; }

        // Progress Percentages (0-100)
        public decimal VisitsProgress { get; set; }
        public decimal SuccessfulVisitsProgress { get; set; }
        public decimal OffersProgress { get; set; }
        public decimal DealsProgress { get; set; }
        public decimal? OfferAcceptanceRateProgress { get; set; }
    }
}

