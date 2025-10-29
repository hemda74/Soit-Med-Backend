using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SoitMed.DTO
{
    public class CreateSalesSupportWithImageDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        [Description("Email address for the Sales Support account")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        [Description("Password for the Sales Support account (minimum 6 characters)")]
        public required string Password { get; set; }

        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [Description("First name of the Sales Support")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Description("Last name of the Sales Support")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Description("Phone number of the Sales Support")]
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Please provide a valid personal email address")]
        [MaxLength(200, ErrorMessage = "Personal mail cannot exceed 200 characters")]
        [Description("Personal email address of the Sales Support")]
        public string? PersonalMail { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Department ID must be a positive number")]
        [Description("Department ID (optional - will auto-assign to Sales department)")]
        public int? DepartmentId { get; set; }

        [MaxLength(500, ErrorMessage = "Alt text cannot exceed 500 characters")]
        [Description("Alternative text for profile image")]
        public string? AltText { get; set; }

        // Sales Support specific fields
        [MaxLength(200, ErrorMessage = "Support specialization cannot exceed 200 characters")]
        [Description("Area of specialization for sales support")]
        public string? SupportSpecialization { get; set; }

        [MaxLength(100, ErrorMessage = "Support level cannot exceed 100 characters")]
        [Description("Level of support (e.g., Junior, Senior, Lead)")]
        public string? SupportLevel { get; set; }

        [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Description("Additional notes about the sales support role")]
        public string? Notes { get; set; }
    }

    public class CreatedSalesSupportWithImageResponseDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public SalesSupportImageInfo? ProfileImage { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? SupportSpecialization { get; set; }
        public string? SupportLevel { get; set; }
        public string? Notes { get; set; }
    }

    public class SalesSupportImageInfo
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


