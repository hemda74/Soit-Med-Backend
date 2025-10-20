using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents client analytics with comprehensive tracking and business logic
    /// </summary>
    public class ClientAnalytics : BaseEntity
    {
        #region Properties
        [Required]
        public long ClientId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Period { get; set; } = AnalyticsPeriodConstants.Daily;

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }

        public int TotalVisits { get; set; }

        public int TotalInteractions { get; set; }

        public int TotalSales { get; set; }

        public decimal AverageVisitDuration { get; set; }

        public DateTime? LastVisitDate { get; set; }

        public DateTime? NextScheduledVisit { get; set; }

        public decimal? ClientSatisfactionScore { get; set; }

        public decimal? ConversionRate { get; set; }

        public decimal? Revenue { get; set; }

        public decimal? GrowthRate { get; set; }

        [MaxLength(2000)]
        public string? TopProducts { get; set; } // JSON array of product names

        [MaxLength(2000)]
        public string? KeyMetrics { get; set; } // JSON object with additional metrics

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Calculates the total activity score combining visits and interactions
        /// </summary>
        public decimal CalculateActivityScore()
        {
            return TotalVisits * 2 + TotalInteractions; // Visits weighted more heavily
        }

        /// <summary>
        /// Determines if this client is highly active
        /// </summary>
        public bool IsHighlyActive(int threshold = 10)
        {
            return CalculateActivityScore() >= threshold;
        }

        /// <summary>
        /// Calculates the average revenue per visit
        /// </summary>
        public decimal? GetAverageRevenuePerVisit()
        {
            return TotalVisits > 0 ? Revenue / TotalVisits : null;
        }

        /// <summary>
        /// Calculates the average revenue per interaction
        /// </summary>
        public decimal? GetAverageRevenuePerInteraction()
        {
            return TotalInteractions > 0 ? Revenue / TotalInteractions : null;
        }

        /// <summary>
        /// Determines if this client has positive growth
        /// </summary>
        public bool HasPositiveGrowth()
        {
            return GrowthRate.HasValue && GrowthRate.Value > 0;
        }

        /// <summary>
        /// Determines if this client is declining
        /// </summary>
        public bool IsDeclining()
        {
            return GrowthRate.HasValue && GrowthRate.Value < -10; // More than 10% decline
        }

        /// <summary>
        /// Calculates the days since last visit
        /// </summary>
        public int? GetDaysSinceLastVisit()
        {
            return LastVisitDate.HasValue ? (DateTime.UtcNow - LastVisitDate.Value).Days : null;
        }

        /// <summary>
        /// Determines if this client needs attention (no recent activity)
        /// </summary>
        public bool NeedsAttention(int daysThreshold = 30)
        {
            var daysSinceLastVisit = GetDaysSinceLastVisit();
            return daysSinceLastVisit.HasValue && daysSinceLastVisit.Value > daysThreshold;
        }

        /// <summary>
        /// Determines if this client is at risk of churning
        /// </summary>
        public bool IsAtRisk()
        {
            return NeedsAttention(60) || IsDeclining() || 
                   (ClientSatisfactionScore.HasValue && ClientSatisfactionScore.Value < 3);
        }

        /// <summary>
        /// Calculates the client health score (0-100)
        /// </summary>
        public decimal CalculateHealthScore()
        {
            decimal score = 0;
            
            // Activity score (40 points max)
            var activityScore = CalculateActivityScore();
            score += Math.Min(40, activityScore * 2);
            
            // Satisfaction score (30 points max)
            if (ClientSatisfactionScore.HasValue)
                score += ClientSatisfactionScore.Value * 6; // 5 * 6 = 30
            
            // Growth score (20 points max)
            if (GrowthRate.HasValue)
            {
                if (GrowthRate.Value > 0)
                    score += Math.Min(20, GrowthRate.Value * 2);
                else
                    score += Math.Max(0, 20 + GrowthRate.Value); // Penalty for negative growth
            }
            
            // Revenue score (10 points max)
            if (Revenue.HasValue && Revenue.Value > 0)
                score += Math.Min(10, 10); // Full points if has revenue
            
            return Math.Min(100, Math.Max(0, score));
        }

        /// <summary>
        /// Updates the analytics with new visit data
        /// </summary>
        public void RecordVisit(DateTime visitDate, decimal? visitDuration = null)
        {
            TotalVisits++;
            LastVisitDate = visitDate;
            
            if (visitDuration.HasValue)
            {
                // Update average visit duration
                AverageVisitDuration = ((AverageVisitDuration * (TotalVisits - 1)) + visitDuration.Value) / TotalVisits;
            }
        }

        /// <summary>
        /// Updates the analytics with new interaction data
        /// </summary>
        public void RecordInteraction()
        {
            TotalInteractions++;
        }

        /// <summary>
        /// Updates the analytics with new sales data
        /// </summary>
        public void RecordSale(decimal saleAmount)
        {
            TotalSales++;
            Revenue = (Revenue ?? 0) + saleAmount;
        }

        /// <summary>
        /// Updates the satisfaction score
        /// </summary>
        public void UpdateSatisfactionScore(decimal newScore)
        {
            if (newScore >= 1 && newScore <= 5)
            {
                ClientSatisfactionScore = newScore;
            }
        }
        #endregion

        #region Constants
        /// <summary>
        /// Analytics period constants
        /// </summary>
        public static class AnalyticsPeriodConstants
        {
            public const string Daily = "daily";
            public const string Weekly = "weekly";
            public const string Monthly = "monthly";
            public const string Quarterly = "quarterly";
            public const string Yearly = "yearly";

            public static readonly string[] AllPeriods = { Daily, Weekly, Monthly, Quarterly, Yearly };

            public static bool IsValidPeriod(string period)
            {
                return AllPeriods.Contains(period);
            }
        }
        #endregion
    }
}