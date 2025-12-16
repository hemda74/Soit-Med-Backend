using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;
using System.Text.Json;
using System.Linq;

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
        public string AssignedTo { get; set; } = string.Empty; // SalesMan ID
        #endregion

        #region Offer Details
        [Required]
        [MaxLength(2000)]
        public string Products { get; set; } = string.Empty; // JSON or detailed list
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        // Payment Terms, Delivery Terms, Warranty Terms, and ValidUntil stored as JSON arrays
        // These are stored as JSON strings in the database, but exposed as List<string> in DTOs
        [Column(TypeName = "nvarchar(max)")]
        public string? PaymentTerms { get; set; } // JSON array of strings (e.g., "[\"term1\", \"term2\"]")
        
        [Column(TypeName = "nvarchar(max)")]
        public string? DeliveryTerms { get; set; } // JSON array of strings
        
        [Column(TypeName = "nvarchar(max)")]
        public string? WarrantyTerms { get; set; } // JSON array of strings
        
        // ValidUntil is now stored as JSON array of date strings (ISO format: "YYYY-MM-DD")
        [Column(TypeName = "nvarchar(max)")]
        public string? ValidUntil { get; set; } // JSON array of date strings (e.g., "[\"2025-12-05\", \"2025-12-15\"]")
        
        [MaxLength(50)]
        public string? PaymentType { get; set; } // Cash, Installments, Other
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? FinalPrice { get; set; }
        
        [MaxLength(200)]
        public string? OfferDuration { get; set; }
        #endregion

        #region Status and Workflow
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = OfferStatus.Draft; // Draft, PendingSalesManagerApproval, Sent, UnderReview, Accepted, Rejected, NeedsModification, Expired
        
        public DateTime? SentToClientAt { get; set; }
        
        [MaxLength(2000)]
        public string? ClientResponse { get; set; }
        #endregion

        #region SalesManager Approval
        public string? SalesManagerApprovedBy { get; set; }
        public DateTime? SalesManagerApprovedAt { get; set; }
        
        [MaxLength(2000)]
        public string? SalesManagerComments { get; set; }
        
        [MaxLength(2000)]
        public string? SalesManagerRejectionReason { get; set; }
        #endregion

        #region Additional Information
        [MaxLength(2000)]
        public string? Documents { get; set; } // JSON array of file paths
        
        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? PdfPath { get; set; }

        public DateTime? PdfGeneratedAt { get; set; }
        #endregion

        #region Navigation Properties
        public virtual OfferRequest? OfferRequest { get; set; }
        public virtual Client Client { get; set; } = null!;
        public virtual ApplicationUser Creator { get; set; } = null!;
        public virtual ApplicationUser SalesMan { get; set; } = null!;
        public virtual SalesDeal? Deal { get; set; }
        public virtual ICollection<OfferEquipment> Equipment { get; set; } = new List<OfferEquipment>();
        public virtual OfferTerms? Terms { get; set; }
        public virtual ICollection<InstallmentPlan> InstallmentPlans { get; set; } = new List<InstallmentPlan>();
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
        /// Checks if the offer is expired (checks all validUntil dates)
        /// </summary>
        public bool IsExpired()
        {
            if (string.IsNullOrWhiteSpace(ValidUntil))
                return false; // No expiration dates defined
            
            try
            {
                var dates = System.Text.Json.JsonSerializer.Deserialize<List<string>>(ValidUntil);
                if (dates == null || dates.Count == 0)
                    return false;
                
                var now = DateTime.UtcNow;
                // Offer is expired if ALL dates have passed
                return dates.All(dateStr => 
                    DateTime.TryParse(dateStr, out var date) && now > date);
            }
            catch
            {
                // Fallback: try to parse as single date (backward compatibility)
                if (DateTime.TryParse(ValidUntil, out var singleDate))
                    return DateTime.UtcNow > singleDate;
                return false;
            }
        }

        /// <summary>
        /// Checks if the offer can be modified
        /// </summary>
        public bool CanBeModified()
        {
            return Status == OfferStatus.Draft || Status == OfferStatus.NeedsModification;
        }

        /// <summary>
        /// Marks the offer as needing modification
        /// </summary>
        public void MarkAsNeedsModification(string? reason = null)
        {
            Status = OfferStatus.NeedsModification;
            if (!string.IsNullOrEmpty(reason))
            {
                Notes = string.IsNullOrEmpty(Notes)
                    ? $"Needs Modification: {reason}"
                    : $"{Notes}\n\nNeeds Modification: {reason}";
            }
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the offer as under review
        /// </summary>
        public void MarkAsUnderReview()
        {
            Status = OfferStatus.UnderReview;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the offer as expired
        /// </summary>
        public void MarkAsExpired()
        {
            Status = OfferStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the offer as pending SalesManager approval
        /// </summary>
        public void MarkAsPendingSalesManagerApproval()
        {
            Status = OfferStatus.PendingSalesManagerApproval;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Approves the offer by SalesManager and sends it to SalesMan
        /// </summary>
        public void ApproveBySalesManager(string salesManagerId, string? comments = null)
        {
            if (Status != OfferStatus.PendingSalesManagerApproval)
                throw new InvalidOperationException("Offer is not pending SalesManager approval");

            Status = OfferStatus.Sent;
            SalesManagerApprovedBy = salesManagerId;
            SalesManagerApprovedAt = DateTime.UtcNow;
            SalesManagerComments = comments;
            SentToClientAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Rejects the offer by SalesManager
        /// </summary>
        public void RejectBySalesManager(string salesManagerId, string rejectionReason, string? comments = null)
        {
            if (Status != OfferStatus.PendingSalesManagerApproval)
                throw new InvalidOperationException("Offer is not pending SalesManager approval");

            Status = OfferStatus.Rejected;
            SalesManagerApprovedBy = salesManagerId; // Track who rejected
            SalesManagerApprovedAt = DateTime.UtcNow;
            SalesManagerRejectionReason = rejectionReason;
            SalesManagerComments = comments;
            UpdatedAt = DateTime.UtcNow;
        }
        #endregion
    }

    #region Constants
    public static class OfferStatus
    {
        public const string Draft = "Draft";
        public const string PendingSalesManagerApproval = "PendingSalesManagerApproval";
        public const string Sent = "Sent";
        public const string UnderReview = "UnderReview";
        public const string Accepted = "Accepted";
        public const string Rejected = "Rejected";
        public const string NeedsModification = "NeedsModification";
        public const string Expired = "Expired";
        public const string Completed = "Completed";
        
        public static readonly string[] AllStatuses = { Draft, PendingSalesManagerApproval, Sent, UnderReview, Accepted, Rejected, NeedsModification, Expired, Completed };
    }
    #endregion
}



