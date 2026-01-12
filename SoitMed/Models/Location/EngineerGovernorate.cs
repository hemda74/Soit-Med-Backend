using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Location
{
    public class EngineerGovernorate
    {
        [Key]
        public string Id { get; set; }

        // Foreign Key to Engineer
        [Required]
        public string EngineerId { get; set; }

        [ForeignKey("EngineerId")]
        public virtual Engineer Engineer { get; set; } = null!;

        // Foreign Key to Governorate
        [Required]
        public string GovernorateId { get; set; }

        [ForeignKey("GovernorateId")]
        public virtual Governorate Governorate { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
