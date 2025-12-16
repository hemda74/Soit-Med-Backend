using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Type of target: Money targets are set by managers, Activity targets (visits/offers/deals) are set by salesmen themselves
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        /// Money/revenue target set by manager
        /// </summary>
        Money = 1,
        /// <summary>
        /// Activity target (visits/offers/deals) set by salesman
        /// </summary>
        Activity = 2
    }

    /// <summary>
    /// Represents targets set by managers (money) or salesmen themselves (visits/offers/deals)
    /// </summary>
    public class SalesManTarget : BaseEntity
    {
        /// <summary>
        /// SalesMan ID - null for team targets
        /// </summary>
        public string? SalesManId { get; set; }

        /// <summary>
        /// Type of target: Money (set by manager) or Activity (set by salesman)
        /// </summary>
        [Required]
        public TargetType TargetType { get; set; }

        /// <summary>
        /// Manager who created this target (null if set by salesman)
        /// </summary>
        public string? CreatedByManagerId { get; set; }
        
        /// <summary>
        /// Target year
        /// </summary>
        [Required]
        public int Year { get; set; }
        
        /// <summary>
        /// Quarter (1-4) or null for yearly target
        /// </summary>
        [Range(1, 4)]
        public int? Quarter { get; set; }
        
        /// <summary>
        /// Target number of visits
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TargetVisits { get; set; }
        
        /// <summary>
        /// Target number of successful visits (client interested)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TargetSuccessfulVisits { get; set; }
        
        /// <summary>
        /// Target number of offers to be made
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TargetOffers { get; set; }
        
        /// <summary>
        /// Target number of deals to be closed
        /// </summary>
        [Range(0, int.MaxValue)]
        public int TargetDeals { get; set; }
        
        /// <summary>
        /// Target offer acceptance rate percentage (0-100)
        /// </summary>
        [Range(0, 100)]
        public decimal? TargetOfferAcceptanceRate { get; set; }
        
        /// <summary>
        /// Target revenue/money amount (for Money type targets)
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal? TargetRevenue { get; set; }
        
        /// <summary>
        /// True if this is a team-wide target
        /// </summary>
        public bool IsTeamTarget { get; set; }
        
        /// <summary>
        /// Additional notes about the target
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }
        
        // Navigation Properties
        public virtual ApplicationUser? SalesMan { get; set; }
        public virtual ApplicationUser? Manager { get; set; }
    }
}


