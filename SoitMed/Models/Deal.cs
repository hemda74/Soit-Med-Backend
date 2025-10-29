using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Core;
using SoitMed.Models.Enums;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a deal with comprehensive tracking and business logic
    /// </summary>
    public class Deal : BaseEntity
    {
        #region Properties
        [Required]
        public long ActivityLogId { get; set; }
        public ActivityLog ActivityLog { get; set; } = null!;

        [Required]
        public long ClientId { get; set; }
        public Client Client { get; set; } = null!;

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime? ExpectedCloseDate { get; set; }

        public DateTime? ActualCloseDate { get; set; }

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [MaxLength(100)]
        public string? ContactEmail { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? RejectedBy { get; set; }

        public DateTime? RejectedAt { get; set; }
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Approves the deal
        /// </summary>
        public void Approve(string approvedBy, string? notes = null)
        {
            Status = "Approved";
            ApprovedBy = approvedBy;
            ApprovedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(notes))
                ApprovalNotes = notes;
        }

        /// <summary>
        /// Rejects the deal
        /// </summary>
        public void Reject(string rejectedBy, string reason, string? notes = null)
        {
            Status = "Rejected";
            RejectedBy = rejectedBy;
            RejectedAt = DateTime.UtcNow;
            RejectionReason = reason;
            if (!string.IsNullOrEmpty(notes))
                ApprovalNotes = notes;
        }

        /// <summary>
        /// Marks the deal as completed
        /// </summary>
        public void Complete()
        {
            Status = "Completed";
            ActualCloseDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the deal as closed
        /// </summary>
        public void Close()
        {
            Status = "Closed";
            ActualCloseDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the deal as lost
        /// </summary>
        public void MarkAsLost(string? reason = null)
        {
            Status = "Lost";
            if (!string.IsNullOrEmpty(reason))
                RejectionReason = reason;
        }

        /// <summary>
        /// Determines if the deal is pending
        /// </summary>
        public bool IsPending()
        {
            return Status == "Pending";
        }

        /// <summary>
        /// Determines if the deal is approved
        /// </summary>
        public bool IsApproved()
        {
            return Status == "Approved";
        }

        /// <summary>
        /// Determines if the deal is rejected
        /// </summary>
        public bool IsRejected()
        {
            return Status == "Rejected";
        }

        /// <summary>
        /// Determines if the deal is completed
        /// </summary>
        public bool IsCompleted()
        {
            return Status == "Completed";
        }

        /// <summary>
        /// Determines if the deal is closed
        /// </summary>
        public bool IsClosed()
        {
            return Status == "Closed";
        }

        /// <summary>
        /// Determines if the deal is lost
        /// </summary>
        public bool IsLost()
        {
            return Status == "Lost";
        }

        /// <summary>
        /// Updates the deal value
        /// </summary>
        public void UpdateValue(decimal newValue)
        {
            Value = newValue;
        }

        /// <summary>
        /// Updates the expected close date
        /// </summary>
        public void UpdateExpectedCloseDate(DateTime? expectedDate)
        {
            ExpectedCloseDate = expectedDate;
        }

        /// <summary>
        /// Updates contact information
        /// </summary>
        public void UpdateContactInfo(string? person, string? phone, string? email)
        {
            if (!string.IsNullOrEmpty(person)) ContactPerson = person;
            if (!string.IsNullOrEmpty(phone)) ContactPhone = phone;
            if (!string.IsNullOrEmpty(email)) ContactEmail = email;
        }
        #endregion
    }
}