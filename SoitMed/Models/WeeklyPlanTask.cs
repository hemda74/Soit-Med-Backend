using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Core;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a simple task within a weekly plan
    /// Contains only: Client info, Title, Date, Description
    /// All progress details are tracked in TaskProgress
    /// </summary>
    public class WeeklyPlanTask : BaseEntity
    {
        [Required]
        public long WeeklyPlanId { get; set; }

        // ========== Task Title ==========
        [Required, MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        // ========== Client Information ==========
        public long? ClientId { get; set; } // NULL for new clients

        [MaxLength(20)]
        public string? ClientStatus { get; set; } // "Old", "New"

        // For NEW clients - basic info
        [MaxLength(200)]
        public string? ClientName { get; set; }

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

        // ========== Task Description ==========
        [MaxLength(1000)]
        public string? Notes { get; set; } // Description/Notes

        // ========== Active Status ==========
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual WeeklyPlan WeeklyPlan { get; set; }
        public virtual Client? Client { get; set; }
        public virtual ICollection<TaskProgress> Progresses { get; set; } = new List<TaskProgress>();

        #region Business Logic Methods
        /// <summary>
        /// Determines if this task is scheduled for today
        /// </summary>
        public bool IsScheduledForToday()
        {
            return PlannedDate.HasValue && PlannedDate.Value.Date == DateTime.UtcNow.Date;
        }

        /// <summary>
        /// Updates client information if this is a new client
        /// </summary>
        public void UpdateClientInfo(string name, string? phone = null, string? address = null, string? location = null)
        {
            ClientName = name;
            ClientPhone = phone;
            ClientAddress = address;
            ClientLocation = location;
            ClientStatus = "New";
        }
        #endregion
    }

    #region Constants
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
