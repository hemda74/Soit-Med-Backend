using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class SuperAdminPasswordUpdateDTO
    {
        [Required(ErrorMessage = "User ID is required")]
        public required string UserId { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare(nameof(NewPassword), ErrorMessage = "Password and confirm password do not match")]
        public required string ConfirmPassword { get; set; }
    }
}
