using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a weekly plan item with comprehensive tracking and business logic
    /// </summary>
    public class WeeklyPlanItem : BaseEntity
    {
        #region Properties
        [Required]
        public long WeeklyPlanId { get; set; }

        public long? ClientId { get; set; } // Nullable for new clients

        [Required]
        [MaxLength(200)]
        public string ClientName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ClientType { get; set; }

        [MaxLength(100)]
        public string? ClientSpecialization { get; set; }

        [MaxLength(100)]
        public string? ClientLocation { get; set; }

        [MaxLength(20)]
        public string? ClientPhone { get; set; }

        [MaxLength(100)]
        public string? ClientEmail { get; set; }

        [Required]
        public DateTime PlannedVisitDate { get; set; }

        [MaxLength(20)]
        public string? PlannedVisitTime { get; set; }

        [MaxLength(500)]
        public string? VisitPurpose { get; set; }

        [MaxLength(1000)]
        public string? VisitNotes { get; set; }

        [MaxLength(50)]
        public string Priority { get; set; } = PlanItemPriorityConstants.Medium;

        [MaxLength(50)]
        public string Status { get; set; } = PlanItemStatusConstants.Planned;

        public bool IsNewClient { get; set; }

        public DateTime? ActualVisitDate { get; set; }

        [MaxLength(2000)]
        public string? Results { get; set; }

        [MaxLength(2000)]
        public string? Feedback { get; set; }

        public int? SatisfactionRating { get; set; } // 1-5 scale

        public DateTime? NextVisitDate { get; set; }

        [MaxLength(1000)]
        public string? FollowUpNotes { get; set; }

        [MaxLength(1000)]
        public string? CancellationReason { get; set; }

        [MaxLength(1000)]
        public string? PostponementReason { get; set; }

        // Navigation properties
        public virtual WeeklyPlan WeeklyPlan { get; set; } = null!;
        public virtual Client? Client { get; set; }
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Determines if this plan item is overdue
        /// </summary>
        public bool IsOverdue()
        {
            return Status == PlanItemStatusConstants.Planned && 
                   PlannedVisitDate < DateTime.UtcNow.Date;
        }

        /// <summary>
        /// Determines if this plan item is scheduled for today
        /// </summary>
        public bool IsScheduledForToday()
        {
            return PlannedVisitDate.Date == DateTime.UtcNow.Date;
        }

        /// <summary>
        /// Determines if this plan item is scheduled for tomorrow
        /// </summary>
        public bool IsScheduledForTomorrow()
        {
            return PlannedVisitDate.Date == DateTime.UtcNow.AddDays(1).Date;
        }

        /// <summary>
        /// Calculates the delay between planned and actual visit
        /// </summary>
        public TimeSpan? GetVisitDelay()
        {
            if (ActualVisitDate.HasValue)
            {
                return ActualVisitDate.Value - PlannedVisitDate;
            }
            return null;
        }

        /// <summary>
        /// Updates the plan item status
        /// </summary>
        public void UpdateStatus(string newStatus)
        {
            if (PlanItemStatusConstants.IsValidStatus(newStatus))
            {
                Status = newStatus;
            }
        }

        /// <summary>
        /// Updates the plan item priority
        /// </summary>
        public void UpdatePriority(string newPriority)
        {
            if (PlanItemPriorityConstants.IsValidPriority(newPriority))
            {
                Priority = newPriority;
            }
        }

        /// <summary>
        /// Marks the visit as completed
        /// </summary>
        public void MarkAsCompleted(DateTime? actualDate = null, string? results = null, string? feedback = null)
        {
            Status = PlanItemStatusConstants.Completed;
            ActualVisitDate = actualDate ?? DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(results))
                Results = results;
                
            if (!string.IsNullOrEmpty(feedback))
                Feedback = feedback;
        }

        /// <summary>
        /// Cancels the visit with a reason
        /// </summary>
        public void Cancel(string reason)
        {
            Status = PlanItemStatusConstants.Cancelled;
            CancellationReason = reason;
        }

        /// <summary>
        /// Postpones the visit with a reason and new date
        /// </summary>
        public void Postpone(string reason, DateTime newDate)
        {
            Status = PlanItemStatusConstants.Postponed;
            PostponementReason = reason;
            PlannedVisitDate = newDate;
        }

        /// <summary>
        /// Determines if the visit was successful based on satisfaction rating
        /// </summary>
        public bool WasSuccessful()
        {
            return SatisfactionRating.HasValue && SatisfactionRating.Value >= 4;
        }

        /// <summary>
        /// Determines if follow-up is needed based on satisfaction rating
        /// </summary>
        public bool NeedsFollowUp()
        {
            return SatisfactionRating.HasValue && SatisfactionRating.Value <= 3;
        }

        /// <summary>
        /// Updates client information if this is a new client
        /// </summary>
        public void UpdateClientInfo(string name, string? type = null, string? specialization = null, 
                                   string? location = null, string? phone = null, string? email = null)
        {
            ClientName = name;
            ClientType = type;
            ClientSpecialization = specialization;
            ClientLocation = location;
            ClientPhone = phone;
            ClientEmail = email;
            IsNewClient = true;
        }
        #endregion

        #region Constants
        /// <summary>
        /// Plan item priority constants
        /// </summary>
        public static class PlanItemPriorityConstants
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
        /// Plan item status constants
        /// </summary>
        public static class PlanItemStatusConstants
        {
            public const string Planned = "Planned";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
            public const string Postponed = "Postponed";
            public const string InProgress = "In Progress";

            public static readonly string[] AllStatuses = { Planned, Completed, Cancelled, Postponed, InProgress };

            public static bool IsValidStatus(string status)
            {
                return AllStatuses.Contains(status);
            }
        }
        #endregion
    }
}