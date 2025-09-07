using System.ComponentModel.DataAnnotations;

namespace SoitMed.Models.Hospital
{
    public class Hospital
    {
        [Key]
        public string HospitalId { get; set; } = string.Empty; // Unique code

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty; // City or area

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty; // Full written address

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
        public virtual ICollection<Technician> Technicians { get; set; } = new List<Technician>();
        public virtual ICollection<Equipment.Equipment> Equipment { get; set; } = new List<Equipment.Equipment>();
    }
}
