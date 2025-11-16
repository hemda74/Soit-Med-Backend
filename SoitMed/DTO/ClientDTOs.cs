using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class CreateClientDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? OrganizationName { get; set; }

        [MaxLength(1)]
        public string? Classification { get; set; } // A, B, C, or D

        public string? AssignedTo { get; set; }
    }

    public class UpdateClientDTO
    {
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? OrganizationName { get; set; }

        [MaxLength(1)]
        public string? Classification { get; set; } // A, B, C, or D

        public string? AssignedTo { get; set; }
    }

    public class SearchClientDTO
    {
        [Required]
        public string Query { get; set; } = string.Empty;

        [MaxLength(1)]
        public string? Classification { get; set; } // A, B, C, or D

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class FindOrCreateClientDTO
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? OrganizationName { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }
    }

    public class ClientResponseDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? OrganizationName { get; set; }
        public string? Classification { get; set; } // A, B, C, or D
        public string CreatedBy { get; set; } = string.Empty;
        public string? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
