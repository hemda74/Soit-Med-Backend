using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoitMed.Models.Hospital
{
    public class DoctorHospital
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key to Doctor
        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;

        // Foreign Key to Hospital
        [Required]
        public string HospitalId { get; set; } = string.Empty;

        [ForeignKey("HospitalId")]
        public virtual Hospital Hospital { get; set; } = null!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
