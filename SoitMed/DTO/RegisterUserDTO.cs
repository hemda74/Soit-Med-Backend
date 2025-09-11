using System.ComponentModel.DataAnnotations;

namespace SoitMed.DTO
{
	public class RegisterUserDTO
	{
		[Required]
		public required string Password { get; set; }
		
		[Required]
		[EmailAddress]
		public required string Email { get; set; }
		
		[Required]
		public required string Role { get; set; }

		// Optional user details
		public string? FirstName { get; set; }
		public string? LastName { get; set; }

		// Department assignment (optional - will be auto-assigned based on role if not provided)
		public int? DepartmentId { get; set; }
	}
}
