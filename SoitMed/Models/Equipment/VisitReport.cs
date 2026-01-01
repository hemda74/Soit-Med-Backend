using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Equipment
{
    /// <summary>
    /// One-to-one relationship with MaintenanceVisit
    /// Contains detailed report information for a completed visit
    /// </summary>
    public class VisitReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VisitId { get; set; }

        [ForeignKey("VisitId")]
        public virtual MaintenanceVisit Visit { get; set; } = null!;

        // Report text content
        [Column(TypeName = "nvarchar(max)")]
        public string? ReportText { get; set; }

        // Media URLs stored as JSON array
        // Format: ["url1", "url2", ...]
        [Column(TypeName = "nvarchar(max)")]
        public string? MediaUrls { get; set; }

        // Check-in time (when engineer arrived at location)
        public DateTime? CheckInTime { get; set; }

        // Check-out time (when engineer left location)
        public DateTime? CheckOutTime { get; set; }

        // GPS coordinates (format: "latitude,longitude")
        [MaxLength(100)]
        public string? GPSCoordinates { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

