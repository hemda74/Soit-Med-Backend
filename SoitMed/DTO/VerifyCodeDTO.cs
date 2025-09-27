using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class VerifyCodeDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 characters")]
        public required string Code { get; set; }
    }
}
