using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Core;

namespace SoitMed.Models
{
    /// <summary>
    /// Represents a client in the system with comprehensive tracking capabilities
    /// </summary>
    public class Client : BaseEntity
    {
        #region Basic Information
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Doctor, Hospital, Clinic, etc.

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }
        #endregion

        #region Contact Information
        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Governorate { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }
        #endregion

        #region Contact Person Information
        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        public string? ContactPersonPhone { get; set; }

        [MaxLength(100)]
        public string? ContactPersonEmail { get; set; }

        [MaxLength(100)]
        public string? ContactPersonPosition { get; set; }
        #endregion

        #region Business Information
        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = ClientStatus.Potential;

        [MaxLength(50)]
        public string Priority { get; set; } = ClientPriority.Medium;

        // Client Classification (A, B, C, D)
        [MaxLength(1)]
        public string? Classification { get; set; } // A, B, C, D

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PotentialValue { get; set; }

        public DateTime? LastContactDate { get; set; }

        public DateTime? NextContactDate { get; set; }

        [Range(1, 5)]
        public int? SatisfactionRating { get; set; }
        #endregion

        #region System Information
        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public string? AssignedTo { get; set; }
        #endregion

        #region Navigation Properties - Complete History
        public virtual ICollection<TaskProgress> TaskProgresses { get; set; } = new List<TaskProgress>(); // All visits/interactions
        public virtual ICollection<Offer> Offers { get; set; } = new List<Offer>(); // All offers
        public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>(); // All deals (success/failed)
        #endregion

        #region Business Logic Methods
        /// <summary>
        /// Updates the client's contact information
        /// </summary>
        public void UpdateContactInfo(string? phone, string? email, string? website)
        {
            if (!string.IsNullOrEmpty(phone)) Phone = phone;
            if (!string.IsNullOrEmpty(email)) Email = email;
            if (!string.IsNullOrEmpty(website)) Website = website;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the client's address information
        /// </summary>
        public void UpdateAddress(string? address, string? city, string? governorate, string? postalCode)
        {
            if (!string.IsNullOrEmpty(address)) Address = address;
            if (!string.IsNullOrEmpty(city)) City = city;
            if (!string.IsNullOrEmpty(governorate)) Governorate = governorate;
            if (!string.IsNullOrEmpty(postalCode)) PostalCode = postalCode;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the client's contact person information
        /// </summary>
        public void UpdateContactPerson(string? name, string? phone, string? email, string? position)
        {
            if (!string.IsNullOrEmpty(name)) ContactPerson = name;
            if (!string.IsNullOrEmpty(phone)) ContactPersonPhone = phone;
            if (!string.IsNullOrEmpty(email)) ContactPersonEmail = email;
            if (!string.IsNullOrEmpty(position)) ContactPersonPosition = position;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the client's business status
        /// </summary>
        public void UpdateStatus(string status, string priority, decimal? potentialValue = null)
        {
            Status = status;
            Priority = priority;
            if (potentialValue.HasValue) PotentialValue = potentialValue;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Records a new contact with the client
        /// </summary>
        public void RecordContact(DateTime contactDate, DateTime? nextContactDate = null)
        {
            LastContactDate = contactDate;
            if (nextContactDate.HasValue) NextContactDate = nextContactDate;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the client's satisfaction rating
        /// </summary>
        public void UpdateSatisfactionRating(int rating)
        {
            if (rating >= 1 && rating <= 5)
            {
                SatisfactionRating = rating;
                UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Checks if the client needs follow-up
        /// </summary>
        public bool NeedsFollowUp()
        {
            return NextContactDate.HasValue && 
                   NextContactDate.Value.Date <= DateTime.UtcNow.Date && 
                   Status == ClientStatus.Active;
        }

        /// <summary>
        /// Checks if the client is active
        /// </summary>
        public bool IsActive()
        {
            return Status == ClientStatus.Active;
        }

        /// <summary>
        /// Checks if the client is high priority
        /// </summary>
        public bool IsHighPriority()
        {
            return Priority == ClientPriority.High;
        }
        #endregion
    }

    #region Constants
    public static class ClientStatus
    {
        public const string Potential = "Potential";
        public const string Active = "Active";
        public const string Inactive = "Inactive";
        public const string Lost = "Lost";
    }

    public static class ClientPriority
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
    }

    public static class ClientTypeConstants
    {
        public const string Doctor = "Doctor";
        public const string Hospital = "Hospital";
        public const string Clinic = "Clinic";
        public const string Pharmacy = "Pharmacy";
        public const string Laboratory = "Laboratory";
    }
    #endregion
}