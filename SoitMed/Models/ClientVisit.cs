using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a client visit with comprehensive tracking and business logic
    /// </summary>
    public class ClientVisit : BaseEntity
    {
        #region Properties
        [Required]
        public long ClientId { get; set; }

        [Required]
        public DateTime VisitDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string VisitType { get; set; } = VisitTypeConstants.Initial;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(500)]
        public string? Purpose { get; set; }

        [MaxLength(1000)]
        public string? Attendees { get; set; } // JSON array of attendee names

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [MaxLength(2000)]
        public string? Results { get; set; }

        public DateTime? NextVisitDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = VisitStatusConstants.Completed;

        [Required]
        public string SalesManId { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Attachments { get; set; } // JSON array of file paths

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        // Navigation properties
        public virtual Client Client { get; set; } = null!;
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Determines if this visit requires follow-up
        /// </summary>
        public bool RequiresFollowUp()
        {
            return !string.IsNullOrEmpty(Results) && 
                   (Results.Contains("follow-up", StringComparison.OrdinalIgnoreCase) ||
                    Results.Contains("follow up", StringComparison.OrdinalIgnoreCase) ||
                    NextVisitDate.HasValue);
        }

        /// <summary>
        /// Calculates the duration since the visit
        /// </summary>
        public TimeSpan GetDurationSinceVisit()
        {
            return DateTime.UtcNow - VisitDate;
        }

        /// <summary>
        /// Determines if the visit is overdue for follow-up
        /// </summary>
        public bool IsFollowUpOverdue(int daysThreshold = 7)
        {
            return NextVisitDate.HasValue && 
                   NextVisitDate.Value < DateTime.UtcNow.AddDays(-daysThreshold);
        }

        /// <summary>
        /// Updates the visit status
        /// </summary>
        public void UpdateStatus(string newStatus)
        {
            if (VisitStatusConstants.IsValidStatus(newStatus))
            {
                Status = newStatus;
            }
        }

        /// <summary>
        /// Adds notes to the visit
        /// </summary>
        public void AddNotes(string additionalNotes)
        {
            if (!string.IsNullOrEmpty(additionalNotes))
            {
                Notes = string.IsNullOrEmpty(Notes) 
                    ? additionalNotes 
                    : $"{Notes}\n{additionalNotes}";
            }
        }

        /// <summary>
        /// Determines if the visit was successful based on results
        /// </summary>
        public bool WasSuccessful()
        {
            if (string.IsNullOrEmpty(Results))
                return false;

            var successKeywords = new[] { "successful", "completed", "achieved", "positive", "good" };
            return successKeywords.Any(keyword => 
                Results.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region Constants
        /// <summary>
        /// Visit type constants
        /// </summary>
        public static class VisitTypeConstants
        {
            public const string Initial = "Initial";
            public const string FollowUp = "Follow-up";
            public const string Maintenance = "Maintenance";
            public const string Support = "Support";
            public const string Demo = "Demo";
            public const string Training = "Training";

            public static readonly string[] AllTypes = { Initial, FollowUp, Maintenance, Support, Demo, Training };

            public static bool IsValidType(string type)
            {
                return AllTypes.Contains(type);
            }
        }

        /// <summary>
        /// Visit status constants
        /// </summary>
        public static class VisitStatusConstants
        {
            public const string Completed = "Completed";
            public const string Scheduled = "Scheduled";
            public const string Cancelled = "Cancelled";
            public const string Postponed = "Postponed";
            public const string InProgress = "In Progress";

            public static readonly string[] AllStatuses = { Completed, Scheduled, Cancelled, Postponed, InProgress };

            public static bool IsValidStatus(string status)
            {
                return AllStatuses.Contains(status);
            }
        }
        #endregion
    }
}