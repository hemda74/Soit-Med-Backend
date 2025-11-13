using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Equipment
{
    public class MaintenanceRequestRating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaintenanceRequestId { get; set; }

        [ForeignKey("MaintenanceRequestId")]
        public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;

        // Customer who rated
        [Required]
        [MaxLength(450)]
        public string CustomerId { get; set; } = string.Empty; // ApplicationUser

        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; } = null!;

        // Engineer being rated
        [Required]
        [MaxLength(450)]
        public string EngineerId { get; set; } = string.Empty; // ApplicationUser (Engineer)

        [ForeignKey("EngineerId")]
        public virtual ApplicationUser Engineer { get; set; } = null!;

        // Rating details
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // 1-5 stars

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}

