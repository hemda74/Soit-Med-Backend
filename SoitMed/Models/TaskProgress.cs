using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents progress/updates made to a task with comprehensive tracking
    /// </summary>
    public class TaskProgress : BaseEntity
    {
        [Required]
        public long TaskId { get; set; } // Link to WeeklyPlanTask

        public long? ClientId { get; set; } // Link to Client for history tracking

        [Required]
        public string EmployeeId { get; set; } // Who made this progress

        // ========== Visit/Progress Details ==========
        [Required]
        public DateTime ProgressDate { get; set; }

        [MaxLength(50)]
        public string ProgressType { get; set; } = "Visit"; // Visit, Call, Meeting, Email

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        // ========== Visit Result ==========
        [MaxLength(20)]
        public string? VisitResult { get; set; } // "Interested", "NotInterested"

        // ========== If NOT Interested ==========
        [MaxLength(2000)]
        public string? NotInterestedComment { get; set; } // Why not interested

        // ========== If Interested - Next Step ==========
        [MaxLength(20)]
        public string? NextStep { get; set; } // "NeedsDeal", "NeedsOffer"

        // Links to created requests
        public long? OfferRequestId { get; set; }

        // ========== Follow-up ==========
        public DateTime? NextFollowUpDate { get; set; }

        [MaxLength(1000)]
        public string? FollowUpNotes { get; set; }

        // ========== Attachments ==========
        [MaxLength(2000)]
        public string? Attachments { get; set; } // JSON array of file paths

        // ========== Voice Description ==========
        [MaxLength(2000)]
        public string? VoiceDescriptionUrl { get; set; }

        // Navigation Properties
        public virtual WeeklyPlanTask Task { get; set; }
        public virtual Client? Client { get; set; }
        public virtual ApplicationUser Employee { get; set; }
        public virtual OfferRequest? OfferRequest { get; set; }

        #region Business Logic Methods
        /// <summary>
        /// Determines if this progress indicates client interest
        /// </summary>
        public bool IsClientInterested()
        {
            return VisitResult == VisitResultConstants.Interested;
        }

        /// <summary>
        /// Determines if this progress indicates client is not interested
        /// </summary>
        public bool IsClientNotInterested()
        {
            return VisitResult == VisitResultConstants.NotInterested;
        }

        /// <summary>
        /// Determines if follow-up is needed
        /// </summary>
        public bool NeedsFollowUp()
        {
            return NextFollowUpDate.HasValue && NextFollowUpDate.Value.Date <= DateTime.UtcNow.Date;
        }

        /// <summary>
        /// Updates the visit result and handles next steps
        /// </summary>
        public void UpdateVisitResult(string result, string? comment = null, string? nextStep = null)
        {
            VisitResult = result;
            
            if (result == VisitResultConstants.NotInterested && !string.IsNullOrEmpty(comment))
            {
                NotInterestedComment = comment;
            }
            
            if (result == VisitResultConstants.Interested && !string.IsNullOrEmpty(nextStep))
            {
                NextStep = nextStep;
            }
        }

        /// <summary>
        /// Sets follow-up date and notes
        /// </summary>
        public void SetFollowUp(DateTime followUpDate, string? notes = null)
        {
            NextFollowUpDate = followUpDate;
            if (!string.IsNullOrEmpty(notes))
                FollowUpNotes = notes;
        }
        #endregion
    }

    #region Constants
    /// <summary>
    /// Progress type constants
    /// </summary>
    public static class ProgressTypeConstants
    {
        public const string Visit = "Visit";
        public const string Call = "Call";
        public const string Meeting = "Meeting";
        public const string Email = "Email";

        public static readonly string[] AllTypes = { Visit, Call, Meeting, Email };

        public static bool IsValidType(string type)
        {
            return AllTypes.Contains(type);
        }
    }

    /// <summary>
    /// Visit result constants
    /// </summary>
    public static class VisitResultConstants
    {
        public const string Interested = "Interested";
        public const string NotInterested = "NotInterested";

        public static readonly string[] AllResults = { Interested, NotInterested };

        public static bool IsValidResult(string result)
        {
            return AllResults.Contains(result);
        }
    }

    /// <summary>
    /// Next step constants
    /// </summary>
    public static class NextStepConstants
    {
        public const string NeedsDeal = "NeedsDeal";
        public const string NeedsOffer = "NeedsOffer";

        public static readonly string[] AllSteps = { NeedsDeal, NeedsOffer };

        public static bool IsValidStep(string step)
        {
            return AllSteps.Contains(step);
        }
    }
    #endregion
}



