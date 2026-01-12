using System.ComponentModel.DataAnnotations;

namespace SoitMed.Models.Location
{
    public class Governorate
    {
        [Key]
        public string GovernorateId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation property for many-to-many relationship with Engineers
        public virtual ICollection<EngineerGovernorate> EngineerGovernorates { get; set; } = new List<EngineerGovernorate>();
    }
}
