using System.ComponentModel.DataAnnotations;
using SoitMed.Models.Enums;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    /// <summary>
    /// Request workflow for handling offers and deals between different roles
    /// </summary>
    public class RequestWorkflow
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long ActivityLogId { get; set; }

        [Required]
        public long? OfferId { get; set; }

        [Required]
        public long? DealId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequestType { get; set; } = string.Empty; // "Offer", "Deal"

        [Required]
        [MaxLength(50)]
        public string FromRole { get; set; } = string.Empty; // "SalesMan"

        [Required]
        [MaxLength(50)]
        public string ToRole { get; set; } = string.Empty; // "SalesSupport", "LegalManager"

        [Required]
        [MaxLength(450)]
        public string FromUserId { get; set; } = string.Empty;

        [MaxLength(450)]
        public string? ToUserId { get; set; } // Assigned by manager

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [MaxLength(1000)]
        public string? Comment { get; set; }

        [MaxLength(200)]
        public string? ClientName { get; set; }

        [MaxLength(500)]
        public string? ClientAddress { get; set; }

        [MaxLength(1000)]
        public string? EquipmentDetails { get; set; }

        public long? DeliveryTermsId { get; set; }

        public long? PaymentTermsId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public virtual ActivityLog ActivityLog { get; set; } = null!;
        public virtual Offer? Offer { get; set; }
        public virtual Deal? Deal { get; set; }
        public virtual DeliveryTerms? DeliveryTerms { get; set; }
        public virtual PaymentTerms? PaymentTerms { get; set; }
        public virtual ApplicationUser FromUser { get; set; } = null!;
        public virtual ApplicationUser ToUser { get; set; } = null!;
    }

}
