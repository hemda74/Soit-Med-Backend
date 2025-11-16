using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Core;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a client in the system - simplified to essential fields only
    /// </summary>
    public class Client : BaseEntity
    {
        #region Basic Information
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? OrganizationName { get; set; }

        [MaxLength(1)]
        public string? Classification { get; set; } // A, B, C, or D
        #endregion

        #region System Information
        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? AssignedTo { get; set; }
        #endregion

        #region Navigation Properties - Complete History
        public virtual ICollection<TaskProgress> TaskProgresses { get; set; } = new List<TaskProgress>(); // All visits/interactions
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>(); // All offers
        public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>(); // All deals (success/failed)
        #endregion
    }
}