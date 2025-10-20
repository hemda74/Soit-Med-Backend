using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Enums;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents an offer with comprehensive tracking and business logic
    /// </summary>
    public class Offer : BaseEntity
    {
        #region Properties
        [Required]
        public long ActivityLogId { get; set; }
        public ActivityLog ActivityLog { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string OfferDetails { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DocumentUrl { get; set; }

        [Required]
        public OfferStatus Status { get; set; } = OfferStatus.Pending;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Determines if this offer is still valid based on creation date
        /// </summary>
        public bool IsValid(int validityDays = 30)
        {
            return CreatedAt.AddDays(validityDays) > DateTime.UtcNow;
        }

        /// <summary>
        /// Calculates the age of this offer in days
        /// </summary>
        public int GetAgeInDays()
        {
            return (DateTime.UtcNow - CreatedAt).Days;
        }

        /// <summary>
        /// Determines if this offer is overdue for response
        /// </summary>
        public bool IsOverdue(int responseDaysThreshold = 7)
        {
            return Status == OfferStatus.Pending && 
                   CreatedAt.AddDays(responseDaysThreshold) < DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the offer status
        /// </summary>
        public void UpdateStatus(OfferStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds notes to the offer
        /// </summary>
        public void AddNotes(string additionalNotes)
        {
            if (!string.IsNullOrEmpty(additionalNotes))
            {
                Notes = string.IsNullOrEmpty(Notes) 
                    ? additionalNotes 
                    : $"{Notes}\n{additionalNotes}";
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Determines if this offer was accepted
        /// </summary>
        public bool WasAccepted()
        {
            return Status == OfferStatus.Accepted;
        }

        /// <summary>
        /// Determines if this offer was rejected
        /// </summary>
        public bool WasRejected()
        {
            return Status == OfferStatus.Rejected;
        }

        /// <summary>
        /// Determines if this offer is still pending
        /// </summary>
        public bool IsPending()
        {
            return Status == OfferStatus.Pending;
        }

        /// <summary>
        /// Marks the offer as accepted
        /// </summary>
        public void Accept(string? notes = null)
        {
            Status = OfferStatus.Accepted;
            UpdatedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(notes))
                AddNotes($"Accepted: {notes}");
        }

        /// <summary>
        /// Marks the offer as rejected
        /// </summary>
        public void Reject(string? reason = null)
        {
            Status = OfferStatus.Rejected;
            UpdatedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(reason))
                AddNotes($"Rejected: {reason}");
        }

        /// <summary>
        /// Determines if this offer needs follow-up
        /// </summary>
        public bool NeedsFollowUp()
        {
            return Status == OfferStatus.Pending && 
                   CreatedAt.AddDays(3) < DateTime.UtcNow;
        }
        #endregion
    }
}



