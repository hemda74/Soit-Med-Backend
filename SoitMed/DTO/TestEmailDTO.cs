using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
    public class TestEmailDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }
    }
}

