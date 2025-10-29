using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Core;
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

        public DateTime? ValidUntil { get; set; }

        public DateTime? SentAt { get; set; }

        public DateTime? ResponseAt { get; set; }

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

        [MaxLength(1000)]
        public string? ClientResponse { get; set; }
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Approves the offer
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
        /// Rejects the offer
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
        /// Sends the offer to client
        /// </summary>
        public void Send()
        {
            Status = "Sent";
            SentAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Marks the offer as completed
        /// </summary>
        public void Complete()
        {
            Status = "Completed";
        }

        /// <summary>
        /// Marks the offer as accepted by client
        /// </summary>
        public void Accept(string? clientResponse = null)
        {
            Status = "Accepted";
            ResponseAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(clientResponse))
                ClientResponse = clientResponse;
        }

        /// <summary>
        /// Records client response
        /// </summary>
        public void RecordClientResponse(string response)
        {
            ClientResponse = response;
            ResponseAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Determines if the offer is pending
        /// </summary>
        public bool IsPending()
        {
            return Status == "Pending";
        }

        /// <summary>
        /// Determines if the offer is approved
        /// </summary>
        public bool IsApproved()
        {
            return Status == "Approved";
        }

        /// <summary>
        /// Determines if the offer is rejected
        /// </summary>
        public bool IsRejected()
        {
            return Status == "Rejected";
        }

        /// <summary>
        /// Determines if the offer is sent
        /// </summary>
        public bool IsSent()
        {
            return Status == "Sent";
        }

        /// <summary>
        /// Determines if the offer is completed
        /// </summary>
        public bool IsCompleted()
        {
            return Status == "Completed";
        }

        /// <summary>
        /// Determines if the offer is accepted
        /// </summary>
        public bool IsAccepted()
        {
            return Status == "Accepted";
        }

        /// <summary>
        /// Determines if the offer is expired
        /// </summary>
        public bool IsExpired()
        {
            return ValidUntil.HasValue && ValidUntil.Value < DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the offer value
        /// </summary>
        public void UpdateValue(decimal newValue)
        {
            Value = newValue;
        }

        /// <summary>
        /// Updates the validity period
        /// </summary>
        public void UpdateValidUntil(DateTime? validUntil)
        {
            ValidUntil = validUntil;
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