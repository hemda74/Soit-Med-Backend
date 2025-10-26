using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents an offer in the sales module with complete workflow tracking
    /// </summary>
    public class SalesOffer : BaseEntity
    {
        #region Basic Information
        public long? OfferRequestId { get; set; }
        
        [Required]
        public long ClientId { get; set; }
        
        [Required]
        public string CreatedBy { get; set; } = string.Empty; // Sales Support ID
        
        [Required]
        public string AssignedTo { get; set; } = string.Empty; // Salesman ID
        #endregion

        #region Offer Details
        [Required]
        [MaxLength(2000)]
        public string Products { get; set; } = string.Empty; // JSON or detailed list
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [MaxLength(2000)]
        public string? PaymentTerms { get; set; }
        
        [MaxLength(2000)]
        public string? DeliveryTerms { get; set; }
        
        [Required]
        public DateTime ValidUntil { get; set; }
        #endregion

        #region Status and Workflow
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = OfferStatus.Draft; // Draft, Sent, UnderReview, Accepted, Rejected, NeedsModification, Expired
        
        public DateTime? SentToClientAt { get; set; }
        
        [MaxLength(2000)]
        public string? ClientResponse { get; set; }
        #endregion

        #region Additional Information
        [MaxLength(2000)]
        public string? Documents { get; set; } // JSON array of file paths
        
        [MaxLength(2000)]
        public string? Notes { get; set; }
        #endregion

        #region Navigation Properties
        public virtual OfferRequest? OfferRequest { get; set; }
        public virtual Client Client { get; set; } = null!;
        public virtual ApplicationUser Creator { get; set; } = null!;
        public virtual ApplicationUser Salesman { get; set; } = null!;
        public virtual SalesDeal? Deal { get; set; }
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Marks the offer as sent to client
        /// </summary>
        public void MarkAsSent()
        {
            Status = OfferStatus.Sent;
            SentToClientAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Records client response to the offer
        /// </summary>
        public void RecordClientResponse(string response, bool accepted)
        {
            ClientResponse = response;
            Status = accepted ? OfferStatus.Accepted : OfferStatus.Rejected;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if the offer is expired
        /// </summary>
        public bool IsExpired()
        {
            return DateTime.UtcNow > ValidUntil;
        }

        /// <summary>
        /// Checks if the offer can be modified
        /// </summary>
        public bool CanBeModified()
        {
            return Status == OfferStatus.Draft || Status == OfferStatus.NeedsModification;
        }
        #endregion
    }

    #region Constants
    public static class OfferStatus
    {
        public const string Draft = "Draft";
        public const string Sent = "Sent";
        public const string UnderReview = "UnderReview";
        public const string Accepted = "Accepted";
        public const string Rejected = "Rejected";
        public const string NeedsModification = "NeedsModification";
        public const string Expired = "Expired";
        
        public static readonly string[] AllStatuses = { Draft, Sent, UnderReview, Accepted, Rejected, NeedsModification, Expired };
    }
    #endregion
}



