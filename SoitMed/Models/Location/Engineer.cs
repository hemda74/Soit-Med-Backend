using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Location
{
    public class Engineer
    {
        [Key]
        public int EngineerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Optional: Link to ApplicationUser if engineers are also system users
        public string? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Navigation property for many-to-many relationship with Governorates
        public virtual ICollection<EngineerGovernorate> EngineerGovernorates { get; set; } = new List<EngineerGovernorate>();
        
        // Navigation property for repair requests assigned to this engineer
        public virtual ICollection<Equipment.RepairRequest> AssignedRepairRequests { get; set; } = new List<Equipment.RepairRequest>();
    }
}
