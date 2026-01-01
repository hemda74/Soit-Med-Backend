using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Equipment
{
    /// <summary>
    /// Many-to-many relationship between MaintenanceVisit and ApplicationUser (Engineers)
    /// Allows multiple engineers to be assigned to a single visit
    /// </summary>
    public class VisitAssignees
    {
        [Key]
        [Column(Order = 0)]
        public int VisitId { get; set; }

        [ForeignKey("VisitId")]
        public virtual MaintenanceVisit Visit { get; set; } = null!;

        [Key]
        [Column(Order = 1)]
        [MaxLength(450)]
        public string EngineerId { get; set; } = string.Empty;

        [ForeignKey("EngineerId")]
        public virtual ApplicationUser Engineer { get; set; } = null!;

        // Timestamp when engineer was assigned
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // User who assigned this engineer
        [MaxLength(450)]
        public string? AssignedById { get; set; }

        [ForeignKey("AssignedById")]
        public virtual ApplicationUser? AssignedBy { get; set; }
    }
}

