using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class CreateEngineerWithImageDTO
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


        [Required(ErrorMessage = "Specialty is required")]
        [MaxLength(100, ErrorMessage = "Specialty cannot exceed 100 characters")]
        public required string Specialty { get; set; }

        [Required(ErrorMessage = "At least one governorate must be selected")]
        [MinLength(1, ErrorMessage = "At least one governorate must be selected")]
        public required List<int> GovernorateIds { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Department ID must be a positive number")]
        public int? DepartmentId { get; set; }

        [MaxLength(500, ErrorMessage = "Alt text cannot exceed 500 characters")]
        public string? AltText { get; set; }
    }
}
