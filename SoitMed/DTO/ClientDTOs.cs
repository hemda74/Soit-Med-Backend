using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class CreateClientDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Governorate { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Potential";

        [MaxLength(50)]
        public string Priority { get; set; } = "Medium";

        [MaxLength(10)]
        public string? Classification { get; set; }

        public int? Rating { get; set; }

        public decimal? PotentialValue { get; set; }

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        public string? ContactPersonPhone { get; set; }

        [MaxLength(100)]
        public string? ContactPersonEmail { get; set; }

        [MaxLength(100)]
        public string? ContactPersonPosition { get; set; }

        public string? AssignedTo { get; set; }
    }

    public class UpdateClientDTO
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Type { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Governorate { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(50)]
        public string? Priority { get; set; }

        public decimal? PotentialValue { get; set; }

        [MaxLength(200)]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        public string? ContactPersonPhone { get; set; }

        [MaxLength(100)]
        public string? ContactPersonEmail { get; set; }

        [MaxLength(100)]
        public string? ContactPersonPosition { get; set; }

        public string? AssignedTo { get; set; }
    }

    public class SearchClientDTO
    {
        [Required]
        public string Query { get; set; } = string.Empty;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class FindOrCreateClientDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Specialization { get; set; }
    }

    public class ClientResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? Location { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Governorate { get; set; }
        public string? PostalCode { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? Classification { get; set; }
        public int? Rating { get; set; }
        public decimal? PotentialValue { get; set; }
        public string? ContactPerson { get; set; }
        public string? ContactPersonPhone { get; set; }
        public string? ContactPersonEmail { get; set; }
        public string? ContactPersonPosition { get; set; }
        public DateTime? LastContactDate { get; set; }
        public DateTime? NextContactDate { get; set; }
        public int? SatisfactionRating { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
