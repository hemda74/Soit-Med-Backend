using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Location
{
    public class EngineerGovernorate
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key to Engineer
        [Required]
        public int EngineerId { get; set; }

        [ForeignKey("EngineerId")]
        public virtual Engineer Engineer { get; set; } = null!;

        // Foreign Key to Governorate
        [Required]
        public int GovernorateId { get; set; }

        [ForeignKey("GovernorateId")]
        public virtual Governorate Governorate { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
