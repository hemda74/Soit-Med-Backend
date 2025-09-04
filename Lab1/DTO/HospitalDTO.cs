using System.ComponentModel.DataAnnotations;

namespace Lab1.DTO
{
    public class HospitalDTO
    {
        [Required]
        public required string HospitalId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Location { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Address { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public required string PhoneNumber { get; set; }
    }

    public class HospitalResponseDTO
    {
        public string HospitalId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int DoctorCount { get; set; }
        public int TechnicianCount { get; set; }
    }

    public class DoctorDTO
    {
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Specialty { get; set; }

        [Required]
        public required string HospitalId { get; set; }

        public string? UserId { get; set; }
    }

    public class TechnicianDTO
    {
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Department { get; set; }

        [Required]
        public required string HospitalId { get; set; }

        public string? UserId { get; set; }
    }
}
