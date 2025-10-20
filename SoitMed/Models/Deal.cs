using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [Column(TypeName = "decimal(18,2)")]
        public decimal DealValue { get; set; }

        [Required]
        public DateTime ExpectedCloseDate { get; set; }

        [Required]
        public DealStatus Status { get; set; } = DealStatus.Pending;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Determines if this deal is overdue for closing
        /// </summary>
        public bool IsOverdue()
        {
            return Status == DealStatus.Pending && 
                   ExpectedCloseDate < DateTime.UtcNow;
        }

        /// <summary>
        /// Calculates the days until expected close date
        /// </summary>
        public int GetDaysUntilClose()
        {
            return (ExpectedCloseDate - DateTime.UtcNow).Days;
        }

        /// <summary>
        /// Calculates the age of this deal in days
        /// </summary>
        public int GetAgeInDays()
        {
            return (DateTime.UtcNow - CreatedAt).Days;
        }

        /// <summary>
        /// Determines if this deal is closing soon (within specified days)
        /// </summary>
        public bool IsClosingSoon(int daysThreshold = 7)
        {
            return Status == DealStatus.Pending && 
                   ExpectedCloseDate <= DateTime.UtcNow.AddDays(daysThreshold);
        }

        /// <summary>
        /// Updates the deal status
        /// </summary>
        public void UpdateStatus(DealStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the deal value
        /// </summary>
        public void UpdateValue(decimal newValue)
        {
            if (newValue > 0)
            {
                DealValue = newValue;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Updates the expected close date
        /// </summary>
        public void UpdateExpectedCloseDate(DateTime newDate)
        {
            ExpectedCloseDate = newDate;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Adds notes to the deal
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
        /// Determines if this deal was closed successfully
        /// </summary>
        public bool WasClosed()
        {
            return Status == DealStatus.Closed;
        }

        /// <summary>
        /// Determines if this deal was lost
        /// </summary>
        public bool WasLost()
        {
            return Status == DealStatus.Lost;
        }

        /// <summary>
        /// Determines if this deal is still pending
        /// </summary>
        public bool IsPending()
        {
            return Status == DealStatus.Pending;
        }

        /// <summary>
        /// Marks the deal as closed
        /// </summary>
        public void Close(string? notes = null)
        {
            Status = DealStatus.Closed;
            UpdatedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(notes))
                AddNotes($"Closed: {notes}");
        }

        /// <summary>
        /// Marks the deal as lost
        /// </summary>
        public void MarkAsLost(string? reason = null)
        {
            Status = DealStatus.Lost;
            UpdatedAt = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(reason))
                AddNotes($"Lost: {reason}");
        }

        /// <summary>
        /// Determines if this deal needs follow-up
        /// </summary>
        public bool NeedsFollowUp()
        {
            return Status == DealStatus.Pending && 
                   CreatedAt.AddDays(5) < DateTime.UtcNow;
        }

        /// <summary>
        /// Calculates the probability of closing based on age and value
        /// </summary>
        public decimal CalculateClosingProbability()
        {
            if (Status != DealStatus.Pending)
                return Status == DealStatus.Closed ? 100 : 0;

            var ageInDays = GetAgeInDays();
            var daysUntilClose = GetDaysUntilClose();
            
            // Simple probability calculation based on deal age and time to close
            var ageFactor = Math.Max(0, 1 - (ageInDays / 30.0m)); // Decreases over 30 days
            var timeFactor = Math.Max(0, 1 - (Math.Abs(daysUntilClose) / 14.0m)); // Better if close to expected date
            
            return Math.Min(100, (ageFactor + timeFactor) * 50); // Max 100%
        }
        #endregion
    }
}