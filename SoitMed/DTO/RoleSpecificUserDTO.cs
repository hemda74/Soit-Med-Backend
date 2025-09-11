using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    // Base DTO for common user properties
    public class BaseUserCreationDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? DepartmentId { get; set; }
    }

    // Doctor-specific creation DTO
    public class CreateDoctorDTO : BaseUserCreationDTO
    {
        [Required]
        public required string Specialty { get; set; }

        [Required]
        public required string HospitalId { get; set; }
    }

    // Engineer-specific creation DTO
    public class CreateEngineerDTO : BaseUserCreationDTO
    {
        [Required]
        public required string Specialty { get; set; }

        [Required]
        public required List<int> GovernorateIds { get; set; } = new List<int>();
    }

    // Technician-specific creation DTO
    public class CreateTechnicianDTO : BaseUserCreationDTO
    {
        [Required]
        public required string Department { get; set; }

        [Required]
        public required string HospitalId { get; set; }
    }

    // Admin-specific creation DTO
    public class CreateAdminDTO : BaseUserCreationDTO
    {
        public string? AccessLevel { get; set; } // Optional: Full, Limited, etc.
    }

    // Finance Manager creation DTO
    public class CreateFinanceManagerDTO : BaseUserCreationDTO
    {
        // BudgetAuthority removed as requested
    }

    // Legal Manager creation DTO
    public class CreateLegalManagerDTO : BaseUserCreationDTO
    {
        public string? LegalSpecialty { get; set; } // Optional: contracts, compliance, etc.
    }

    // Salesman creation DTO
    public class CreateSalesmanDTO : BaseUserCreationDTO
    {
        public string? Territory { get; set; } // Optional: sales territory
        // SalesTarget removed as requested
    }

    // Response DTOs for created users
    public class CreatedUserResponseDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; // Email is now the username
        public string Role { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CreatedDoctorResponseDTO : CreatedUserResponseDTO
    {
        public int DoctorId { get; set; }
        public string Specialty { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
    }

    public class CreatedEngineerResponseDTO : CreatedUserResponseDTO
    {
        public int EngineerId { get; set; }
        public string Specialty { get; set; } = string.Empty;
        public List<string> AssignedGovernorates { get; set; } = new List<string>();
    }

    public class CreatedTechnicianResponseDTO : CreatedUserResponseDTO
    {
        public int TechnicianId { get; set; }
        public string Department { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
    }
}

