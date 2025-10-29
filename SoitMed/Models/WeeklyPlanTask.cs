using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a task within a weekly plan with comprehensive tracking capabilities
    /// </summary>
    public class WeeklyPlanTask : BaseEntity
    {
        [Required]
        public long WeeklyPlanId { get; set; }

        // ========== Task Title ==========
        [Required, MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        // ========== Task Type ==========
        [Required, MaxLength(50)]
        public string TaskType { get; set; } = "Visit"; // Visit, FollowUp

        // ========== Client Information ==========
        public long? ClientId { get; set; } // NULL for new clients

        [MaxLength(20)]
        public string? ClientStatus { get; set; } // "Old", "New"

        // For NEW clients - basic info
        [MaxLength(200)]
        public string? ClientName { get; set; }

        [MaxLength(200)]
        public string? PlaceName { get; set; }

        [MaxLength(50)]
        public string? PlaceType { get; set; } // Hospital, Clinic, Lab

        [MaxLength(20)]
        public string? ClientPhone { get; set; }

        [MaxLength(500)]
        public string? ClientAddress { get; set; }

        [MaxLength(100)]
        public string? ClientLocation { get; set; }

        // ========== Client Classification ==========
        [MaxLength(1)]
        public string? ClientClassification { get; set; } // A, B, C, D

        // ========== Task Planning ==========
        public DateTime? PlannedDate { get; set; }

        [MaxLength(20)]
        public string? PlannedTime { get; set; }

        [MaxLength(500)]
        public string? Purpose { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string Priority { get; set; } = "Medium"; // High, Medium, Low

        [MaxLength(50)]
        public string Status { get; set; } = "Planned"; // Planned, InProgress, Completed, Cancelled

        // Navigation Properties
        public virtual WeeklyPlan WeeklyPlan { get; set; }
        public virtual Client? Client { get; set; } // Link to existing client if Old
        public virtual ICollection<TaskProgress> Progresses { get; set; } = new List<TaskProgress>(); // Multiple progress updates

        #region Business Logic Methods
        /// <summary>
        /// Determines if this task is overdue
        /// </summary>
        public bool IsOverdue()
        {
            return Status == "Planned" && PlannedDate.HasValue && PlannedDate.Value < DateTime.UtcNow.Date;
        }

        /// <summary>
        /// Determines if this task is scheduled for today
        /// </summary>
        public bool IsScheduledForToday()
        {
            return PlannedDate.HasValue && PlannedDate.Value.Date == DateTime.UtcNow.Date;
        }

        /// <summary>
        /// Updates the task status
        /// </summary>
        public void UpdateStatus(string newStatus)
        {
            if (TaskStatusConstants.IsValidStatus(newStatus))
            {
                Status = newStatus;
            }
        }

        /// <summary>
        /// Updates the task priority
        /// </summary>
        public void UpdatePriority(string newPriority)
        {
            if (TaskPriorityConstants.IsValidPriority(newPriority))
            {
                Priority = newPriority;
            }
        }

        /// <summary>
        /// Marks the task as completed
        /// </summary>
        public void MarkAsCompleted()
        {
            Status = "Completed";
        }

        /// <summary>
        /// Cancels the task
        /// </summary>
        public void Cancel()
        {
            Status = "Cancelled";
        }

        /// <summary>
        /// Updates client information if this is a new client
        /// </summary>
        public void UpdateClientInfo(string name, string? placeName = null, string? placeType = null, 
                                   string? phone = null, string? address = null, string? location = null)
        {
            ClientName = name;
            PlaceName = placeName;
            PlaceType = placeType;
            ClientPhone = phone;
            ClientAddress = address;
            ClientLocation = location;
            ClientStatus = "New";
        }
        #endregion
    }

    #region Constants
    /// <summary>
    /// Task type constants
    /// </summary>
    public static class TaskTypeConstants
    {
        public const string Visit = "Visit";
        public const string FollowUp = "FollowUp";

        public static readonly string[] AllTypes = { Visit, FollowUp };

        public static bool IsValidType(string type)
        {
            return AllTypes.Contains(type);
        }
    }

    /// <summary>
    /// Task priority constants
    /// </summary>
    public static class TaskPriorityConstants
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";

        public static readonly string[] AllPriorities = { Low, Medium, High };

        public static bool IsValidPriority(string priority)
        {
            return AllPriorities.Contains(priority);
        }
    }

    /// <summary>
    /// Task status constants
    /// </summary>
    public static class TaskStatusConstants
    {
        public const string Planned = "Planned";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";

        public static readonly string[] AllStatuses = { Planned, InProgress, Completed, Cancelled };

        public static bool IsValidStatus(string status)
        {
            return AllStatuses.Contains(status);
        }
    }

    /// <summary>
    /// Client status constants
    /// </summary>
    public static class ClientStatusConstants
    {
        public const string Old = "Old";
        public const string New = "New";

        public static readonly string[] AllStatuses = { Old, New };

        public static bool IsValidStatus(string status)
        {
            return AllStatuses.Contains(status);
        }
    }

    /// <summary>
    /// Client classification constants
    /// </summary>
    public static class ClientClassificationConstants
    {
        public const string A = "A";
        public const string B = "B";
        public const string C = "C";
        public const string D = "D";

        public static readonly string[] AllClassifications = { A, B, C, D };

        public static bool IsValidClassification(string classification)
        {
            return AllClassifications.Contains(classification);
        }
    }
    #endregion
}
