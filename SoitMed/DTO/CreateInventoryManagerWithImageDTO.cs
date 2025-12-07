using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class CreateInventoryManagerWithImageDTO
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

        [Required(ErrorMessage = "Specialty is required")]
        [MaxLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")]
        public required string Specialty { get; set; }

        [MaxLength(500, ErrorMessage = "Alt text cannot exceed 500 characters")]
        public string? AltText { get; set; }
    }

    public class CreatedInventoryManagerWithImageResponseDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public InventoryManagerImageInfo? ProfileImage { get; set; }
        public string Message { get; set; } = string.Empty;
        public int InventoryManagerId { get; set; }
        public string Specialty { get; set; } = string.Empty;
    }

    public class InventoryManagerImageInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public string? AltText { get; set; }
        public bool IsProfileImage { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}

