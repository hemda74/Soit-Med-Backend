using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SoitMed.DTO
{
    public class CreateSalesManagerWithImageDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        [Description("Email address for the Sales Manager account")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        [Description("Password for the Sales Manager account (minimum 6 characters)")]
        public required string Password { get; set; }

        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [Description("First name of the Sales Manager")]
        public string? FirstName { get; set; }

        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        [Description("Last name of the Sales Manager")]
        public string? LastName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Department ID must be a positive number")]
        [Description("Department ID (optional - will auto-assign to Sales department)")]
        public int? DepartmentId { get; set; }

        [MaxLength(500, ErrorMessage = "Alt text cannot exceed 500 characters")]
        [Description("Alternative text for profile image")]
        public string? AltText { get; set; }

        // Sales Manager specific fields
        [MaxLength(200, ErrorMessage = "Sales territory cannot exceed 200 characters")]
        [Description("Sales territory assigned to the manager")]
        public string? SalesTerritory { get; set; }

        [MaxLength(100, ErrorMessage = "Sales team cannot exceed 100 characters")]
        [Description("Sales team name")]
        public string? SalesTeam { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Sales target must be a positive number")]
        [Description("Sales target amount for the manager")]
        public decimal? SalesTarget { get; set; }
    }

    public class CreatedSalesManagerWithImageResponseDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public SalesManagerImageInfo? ProfileImage { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? SalesTerritory { get; set; }
        public string? SalesTeam { get; set; }
        public decimal? SalesTarget { get; set; }
    }

    public class SalesManagerImageInfo
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
