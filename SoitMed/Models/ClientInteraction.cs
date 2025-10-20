using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a client interaction with comprehensive tracking and business logic
    /// </summary>
    public class ClientInteraction : BaseEntity
    {
        #region Properties
        [Required]
        public long ClientId { get; set; }

        [Required]
        public DateTime InteractionDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string InteractionType { get; set; } = InteractionTypeConstants.Call;

        [MaxLength(200)]
        public string? Subject { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? Participants { get; set; } // JSON array of participant names

        [MaxLength(2000)]
        public string? Outcome { get; set; }

        public bool FollowUpRequired { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [MaxLength(50)]
        public string Priority { get; set; } = InteractionPriorityConstants.Medium;

        [MaxLength(50)]
        public string Status { get; set; } = InteractionStatusConstants.Open;

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Determines if this interaction is urgent based on priority and follow-up date
        /// </summary>
        public bool IsUrgent()
        {
            return Priority == InteractionPriorityConstants.High || 
                   (FollowUpRequired && FollowUpDate.HasValue && 
                    FollowUpDate.Value <= DateTime.UtcNow.AddDays(1));
        }

        /// <summary>
        /// Calculates the duration since the interaction
        /// </summary>
        public TimeSpan GetDurationSinceInteraction()
        {
            return DateTime.UtcNow - InteractionDate;
        }

        /// <summary>
        /// Determines if the follow-up is overdue
        /// </summary>
        public bool IsFollowUpOverdue(int daysThreshold = 3)
        {
            return FollowUpRequired && 
                   FollowUpDate.HasValue && 
                   FollowUpDate.Value < DateTime.UtcNow.AddDays(-daysThreshold);
        }

        /// <summary>
        /// Updates the interaction status
        /// </summary>
        public void UpdateStatus(string newStatus)
        {
            if (InteractionStatusConstants.IsValidStatus(newStatus))
            {
                Status = newStatus;
            }
        }

        /// <summary>
        /// Updates the interaction priority
        /// </summary>
        public void UpdatePriority(string newPriority)
        {
            if (InteractionPriorityConstants.IsValidPriority(newPriority))
            {
                Priority = newPriority;
            }
        }

        /// <summary>
        /// Adds outcome to the interaction
        /// </summary>
        public void AddOutcome(string additionalOutcome)
        {
            if (!string.IsNullOrEmpty(additionalOutcome))
            {
                Outcome = string.IsNullOrEmpty(Outcome) 
                    ? additionalOutcome 
                    : $"{Outcome}\n{additionalOutcome}";
            }
        }

        /// <summary>
        /// Determines if the interaction was successful based on outcome
        /// </summary>
        public bool WasSuccessful()
        {
            if (string.IsNullOrEmpty(Outcome))
                return false;

            var successKeywords = new[] { "successful", "completed", "achieved", "positive", "good", "resolved" };
            return successKeywords.Any(keyword => 
                Outcome.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Marks the interaction as requiring follow-up
        /// </summary>
        public void MarkForFollowUp(DateTime? followUpDate = null)
        {
            FollowUpRequired = true;
            FollowUpDate = followUpDate ?? DateTime.UtcNow.AddDays(1);
        }

        /// <summary>
        /// Closes the interaction
        /// </summary>
        public void Close()
        {
            Status = InteractionStatusConstants.Closed;
            FollowUpRequired = false;
        }
        #endregion

        #region Constants
        /// <summary>
        /// Interaction type constants
        /// </summary>
        public static class InteractionTypeConstants
        {
            public const string Call = "Call";
            public const string Email = "Email";
            public const string Meeting = "Meeting";
            public const string Visit = "Visit";
            public const string VideoCall = "Video Call";
            public const string Text = "Text";
            public const string SocialMedia = "Social Media";

            public static readonly string[] AllTypes = { Call, Email, Meeting, Visit, VideoCall, Text, SocialMedia };

            public static bool IsValidType(string type)
            {
                return AllTypes.Contains(type);
            }
        }

        /// <summary>
        /// Interaction priority constants
        /// </summary>
        public static class InteractionPriorityConstants
        {
            public const string Low = "Low";
            public const string Medium = "Medium";
            public const string High = "High";
            public const string Critical = "Critical";

            public static readonly string[] AllPriorities = { Low, Medium, High, Critical };

            public static bool IsValidPriority(string priority)
            {
                return AllPriorities.Contains(priority);
            }
        }

        /// <summary>
        /// Interaction status constants
        /// </summary>
        public static class InteractionStatusConstants
        {
            public const string Open = "Open";
            public const string Closed = "Closed";
            public const string Pending = "Pending";
            public const string InProgress = "In Progress";
            public const string Cancelled = "Cancelled";

            public static readonly string[] AllStatuses = { Open, Closed, Pending, InProgress, Cancelled };

            public static bool IsValidStatus(string status)
            {
                return AllStatuses.Contains(status);
            }
        }
        #endregion
    }
}