using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a request for an offer from a salesman to sales support
    /// </summary>
    public class OfferRequest : BaseEntity
    {
        [Required]
        public string RequestedBy { get; set; } // Salesman ID

        [Required]
        public long ClientId { get; set; }

        public long? TaskProgressId { get; set; } // Link to the progress that triggered this request

        [Required, MaxLength(2000)]
        public string RequestedProducts { get; set; } // JSON or comma-separated

        [MaxLength(2000)]
        public string? SpecialNotes { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Requested"; // Requested, Assigned, InProgress, Ready, Sent, Cancelled

        public string? AssignedTo { get; set; } // Sales Support ID

        public long? CreatedOfferId { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(1000)]
        public string? CompletionNotes { get; set; }

        // Navigation Properties
        public virtual ApplicationUser Requester { get; set; }
        public virtual Client Client { get; set; }
        public virtual TaskProgress? TaskProgress { get; set; }
        public virtual ApplicationUser? AssignedSupportUser { get; set; }
        public virtual SalesOffer? CreatedOffer { get; set; }

        #region Business Logic Methods
        /// <summary>
        /// Assigns the request to a sales support user
        /// </summary>
        public void AssignTo(string supportUserId)
        {
            AssignedTo = supportUserId;
            Status = "Assigned";
        }

        /// <summary>
        /// Marks the request as completed
        /// </summary>
        public void MarkAsCompleted(string? notes = null, long? offerId = null)
        {
            Status = "Ready";
            CompletedAt = DateTime.UtcNow;
            if (offerId.HasValue)
                CreatedOfferId = offerId.Value;
            if (!string.IsNullOrEmpty(notes))
                CompletionNotes = notes;
        }

        /// <summary>
        /// Marks the request as sent to salesman
        /// </summary>
        public void MarkAsSent()
        {
            Status = "Sent";
        }

        /// <summary>
        /// Cancels the request
        /// </summary>
        public void Cancel(string? reason = null)
        {
            Status = "Cancelled";
            if (!string.IsNullOrEmpty(reason))
                CompletionNotes = reason;
        }

        /// <summary>
        /// Determines if the request is pending
        /// </summary>
        public bool IsPending()
        {
            return Status == "Requested" || Status == "Assigned" || Status == "InProgress";
        }

        /// <summary>
        /// Determines if the request is completed
        /// </summary>
        public bool IsCompleted()
        {
            return Status == "Ready" || Status == "Sent";
        }

        /// <summary>
        /// Determines if the request is cancelled
        /// </summary>
        public bool IsCancelled()
        {
            return Status == "Cancelled";
        }
        #endregion
    }

    #region Constants
    /// <summary>
    /// Offer request status constants
    /// </summary>
    public static class OfferRequestStatusConstants
    {
        public const string Requested = "Requested";
        public const string Assigned = "Assigned";
        public const string InProgress = "InProgress";
        public const string Ready = "Ready";
        public const string Sent = "Sent";
        public const string Cancelled = "Cancelled";

        public static readonly string[] AllStatuses = { Requested, Assigned, InProgress, Ready, Sent, Cancelled };

        public static bool IsValidStatus(string status)
        {
            return AllStatuses.Contains(status);
        }
    }
    #endregion
}
