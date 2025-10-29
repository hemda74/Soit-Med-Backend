using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents targets set by managers for individual salesmen or the entire team
    /// </summary>
    public class SalesmanTarget : BaseEntity
    {
        /// <summary>
        /// Salesman ID - null for team targets
        /// </summary>
        public string? SalesmanId { get; set; }

        /// <summary>
        /// Manager who created this target
        /// </summary>
        [Required]
        public string CreatedByManagerId { get; set; } = string.Empty;
        
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
        /// True if this is a team-wide target
        /// </summary>
        public bool IsTeamTarget { get; set; }
        
        /// <summary>
        /// Additional notes about the target
        /// </summary>
        [MaxLength(2000)]
        public string? Notes { get; set; }
        
        // Navigation Properties
        public virtual ApplicationUser? Salesman { get; set; }
        public virtual ApplicationUser Manager { get; set; } = null!;
    }
}


