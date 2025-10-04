using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoitMed.Models.Identity;

namespace SoitMed.Models.Hospital
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; } = string.Empty;

        // Additional properties
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Optional: Link to ApplicationUser if doctors are also system users
        public string? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        // Navigation property for many-to-many relationship with Hospitals
        public virtual ICollection<DoctorHospital> DoctorHospitals { get; set; } = new List<DoctorHospital>();

        // Navigation property for repair requests
        public virtual ICollection<Equipment.RepairRequest> RepairRequests { get; set; } = new List<Equipment.RepairRequest>();
    }
}
