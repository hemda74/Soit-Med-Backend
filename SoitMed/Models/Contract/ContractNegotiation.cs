using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Contract
{
    /// <summary>
    /// Contract negotiation history and notes
    /// </summary>
    public class ContractNegotiation
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long ContractId { get; set; }

        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string ActionType { get; set; } = string.Empty; // e.g., "Comment", "Revision", "Approval"

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Notes { get; set; } = string.Empty;

        [Required]
        [MaxLength(450)]
        public string SubmittedBy { get; set; } = string.Empty;

        [ForeignKey("SubmittedBy")]
        public virtual ApplicationUser Submitter { get; set; } = null!;

        [MaxLength(50)]
        public string? SubmitterRole { get; set; }

        [MaxLength(2000)]
        public string? Attachments { get; set; } // Comma-separated file names (transformed paths)

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

