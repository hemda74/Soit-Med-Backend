using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    // Base DTO for common user properties
    public class BaseUserCreationDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public required string Password { get; set; }

        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Please provide a valid personal email address")]
        [MaxLength(200, ErrorMessage = "Personal mail cannot exceed 200 characters")]
        public string? PersonalMail { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Department ID must be a positive number")]
        public int? DepartmentId { get; set; }

        // Image fields
        public IFormFile? ProfileImage { get; set; }
        public string? ImageAltText { get; set; }
    }

    // Doctor-specific creation DTO
    public class CreateDoctorDTO : BaseUserCreationDTO
    {
        [Required(ErrorMessage = "Medical specialty is required")]
        [MaxLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")]
        public required string Specialty { get; set; }

        [Required(ErrorMessage = "Hospital ID is required")]
        [MaxLength(50, ErrorMessage = "Hospital ID cannot exceed 50 characters")]
        public required string HospitalId { get; set; }
    }

    // Engineer-specific creation DTO
    public class CreateEngineerDTO : BaseUserCreationDTO
    {
        [Required(ErrorMessage = "Engineering specialty is required")]
        [MaxLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")]
        public required string Specialty { get; set; }

        [Required(ErrorMessage = "At least one governorate must be assigned")]
        [MinLength(1, ErrorMessage = "At least one governorate must be assigned")]
        public required List<int> GovernorateIds { get; set; } = new List<int>();
    }

    // Technician-specific creation DTO
    public class CreateTechnicianDTO : BaseUserCreationDTO
    {
        [Required(ErrorMessage = "Technical department is required")]
        [MaxLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public required string Department { get; set; }

        [Required(ErrorMessage = "Hospital ID is required")]
        [MaxLength(50, ErrorMessage = "Hospital ID cannot exceed 50 characters")]
        public required string HospitalId { get; set; }
    }

    // Admin-specific creation DTO
    public class CreateAdminDTO : BaseUserCreationDTO
    {
        [MaxLength(50, ErrorMessage = "Access level cannot exceed 50 characters")]
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
        [MaxLength(100, ErrorMessage = "Legal specialty cannot exceed 100 characters")]
        public string? LegalSpecialty { get; set; } // Optional: contracts, compliance, etc.
    }

    // Salesman creation DTO
    public class CreateSalesmanDTO : BaseUserCreationDTO
    {
        [MaxLength(100, ErrorMessage = "Territory cannot exceed 100 characters")]
        public string? Territory { get; set; } // Optional: sales territory
        // SalesTarget removed as requested
    }

    // Finance Employee creation DTO
    public class CreateFinanceEmployeeDTO : BaseUserCreationDTO
    {
        [MaxLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string? JobTitle { get; set; } // Optional: Accountant, Financial Analyst, etc.
    }

    // Legal Employee creation DTO
    public class CreateLegalEmployeeDTO : BaseUserCreationDTO
    {
        [MaxLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string? JobTitle { get; set; } // Optional: Legal Assistant, Paralegal, etc.
        
        [MaxLength(100, ErrorMessage = "Legal specialty cannot exceed 100 characters")]
        public string? LegalSpecialty { get; set; } // Optional: contracts, compliance, etc.
    }

    // Maintenance Manager creation DTO
    public class CreateMaintenanceManagerDTO : BaseUserCreationDTO
    {
        [MaxLength(100, ErrorMessage = "Maintenance specialty cannot exceed 100 characters")]
        public string? MaintenanceSpecialty { get; set; } // Optional: Equipment, HVAC, Electrical, etc.
        
        [MaxLength(100, ErrorMessage = "Certification cannot exceed 100 characters")]
        public string? Certification { get; set; } // Optional: Professional certifications
    }

    // Maintenance Support creation DTO
    public class CreateMaintenanceSupportDTO : BaseUserCreationDTO
    {
        [MaxLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
        public string? JobTitle { get; set; } // Optional: Maintenance Technician, Support Specialist, etc.
        
        [MaxLength(100, ErrorMessage = "Technical skills cannot exceed 100 characters")]
        public string? TechnicalSkills { get; set; } // Optional: Specific technical skills
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
        public UserImageDTO? ProfileImage { get; set; }
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

