using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models
{
    public class SalesReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Body { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = string.Empty; // "daily", "weekly", "monthly", "custom"

        [Required]
        public DateOnly ReportDate { get; set; }

        [Required]
        [MaxLength(450)]
        public string EmployeeId { get; set; } = string.Empty;

        public int? Rating { get; set; } // 1-5 stars, nullable

        [MaxLength(500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("EmployeeId")]
        public virtual ApplicationUser Employee { get; set; } = null!;
    }
}
